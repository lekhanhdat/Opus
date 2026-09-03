/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Threading;
using System.ServiceModel.Channels;
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon.Transfer.Data
{
    /// <summary>
    /// 实现接收接口，定义Online时候的数据接受逻辑
    /// </summary>
    public class CMDataReceiver : BaseDataTransferLogic, IDataReceiver
    {
        public CMDataReceiver(int bufferSize = 100)
            : base(DataTransferConstants.ReceiverIdentifier, DataTransferConstants.SenderIdentifier, false, bufferSize)
        {
        }

        private AveDataBlock GetWorkingBlock(AveDataBlockType type)
        {
            while (true)
            {
                GetNextBlock();

                if (WorkingBlock.Type == type)
                {
                    break;
                }
                else if (WorkingBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                {
                    break;
                }
            }
            return WorkingBlock;
        }

        private AveDataBlock GetNextBlock()
        {
            //if (WorkingBlock != null)
            //{
            //    InputQueue.PutFreeBlock(WorkingBlock);
            //    WorkingBlock = null;
            //}

            ////WorkingBlock = TakeWorkingBlock(true);
            //WorkingBlock = TakeFreeBlock(true);
            //var tempBlock = Processor.Read(WorkingBlock);
            //if (tempBlock != null)
            //{
            //    WorkingBlock = tempBlock;
            //}

            if (WorkingBlock == null)
            {
                WorkingBlock = TakeFreeBlock(true);
            }

            AveDataBlock tempBlock = Processor.Read(WorkingBlock);
            if (tempBlock != null)
            {
                WorkingBlock = tempBlock;
            }
            else if(WorkingBlock.Type != AveDataBlockType.CLOSE_CONNECTION_TYPE)
            {
                throw new Exception("There is no close connection data from sender, please review the logs first.");
            }

            return WorkingBlock;
        }

        private bool ReceiveBufferFromRelayService(long serialNo, out byte[] buffer)
        {
            CheckDataTransferIsValid();

            this.PerformanceTimerPool.Action("Receiver Buffer", true);

            DateTime currentTime = DateTime.UtcNow;
            buffer = null;
            while (true)
            {
                try
                {
                    //DataTransferWorkStatus = DataTransferWorkStatus.Running;
                    LastCommunicationTime = DateTime.Now;
                    ActiveTransferNotifier();
                    this.PerformanceTimerPool.Action("Receiver Binary", true);
                    this.performanceCounter.BeginReceive();
                    var resultCode = TransferChannel.ReceiveBinary(serialNo, out buffer);
                    this.performanceCounter.EndReceive((buffer != null) ? buffer.LongLength : 0);
                    this.PerformanceTimerPool.Action("Receiver Binary", false);
                    if (resultCode == BufferStatus.NoDataFromSender)
                    {
                        //对方结束了，不再发送数据了
                        return false;
                    }
                    else if (resultCode == BufferStatus.OK)
                    {
                        //成功取得一个buffer
                        break;
                    }
                    else if (resultCode == BufferStatus.NoBuffer)
                    {
                        //中转服务缓冲没有数据，过一会重试
                        Thread.Sleep(50);
                    }
                    else if (resultCode == BufferStatus.BufferSerialNoError)
                    {
                        DataTransferWorkStatus = DataTransferWorkStatus.DataSequenceConfusion;
                        //throw new Exception("order mismatch");
                    }
                    else if (resultCode == BufferStatus.ReadTimeout)
                    {
                        DataTransferWorkStatus = DataTransferWorkStatus.Timeout;
                        DataTransferErrorMessage += "\r\nSource does not put buffer in time.";
                    }
                    else
                    {
                        DataTransferWorkStatus = DataTransferWorkStatus.UnHandlerError;
                        DataTransferErrorMessage += string.Format("Get response from server:{0}", resultCode);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    //网络出现异常，过一会重试
                    logger.Error("Receive buffer from relay service failed:{0} when the session is {1}.", ex.ToString(), this.SessionId);

                    DataTransferWorkStatus = DataTransferWorkStatus.Retrying;

                    SetupConnection(false);

                    CallBackReconnectedRunCode();
                    //时间最好比源端长一点
                    if (currentTime.AddMinutes(ReconnectTimeout*1.2) < DateTime.UtcNow)
                    {
                        DataTransferWorkStatus = DataTransferWorkStatus.Timeout;
                        DataTransferErrorMessage = string.Format("Cannot get buffer from cache, retry timeout:{0} minute.", ReconnectTimeout);
                    }
                }

                CheckDataTransferIsValid();
            }
            CheckDataTransferIsValid();
            this.PerformanceTimerPool.Action("Receiver Buffer", false);
            return true;
        }

        protected override void TransferThreadTemp()
        {
            DateTime currentTime = DateTime.Now;
            long serialNo = 1;
            try
            {
                this.PerformanceTimerPool.Action("Transfer Thread", true);

                byte[] receiveBuffer;

                Processor.SetWriteTimeoutDelegate(KeepAliveWithRelayService);

                AveThreadWrapper threadWrapper = AveThreadUtility.CurrentThreadWrapper;
                while (threadWrapper.KeepRunning)
                {
                    bool ret = ReceiveBufferFromRelayService(serialNo, out receiveBuffer);
                    if (ret == false)
                    {
                        Processor.FinishWrite();
                        break;
                    }
                    serialNo++;
                }
            }
            catch (Exception ex)
            {
                DataTransferWorkStatus = DataTransferWorkStatus.ReceiverError;
                DataTransferErrorMessage += "Get Buffer And Save It Failed:" + ex.ToString();
                logger.Error(DataTransferErrorMessage);//TODO
            }
            finally
            {
                logger.Debug("Receiver {0} buffer within {1}.", serialNo, DateTime.Now - currentTime);
                Processor.FinishWrite();
                this.PerformanceTimerPool.Action("Transfer Thread", false);
            }
        }

        protected override void TransferThread()
        {
            DateTime currentTime = DateTime.Now;
            long serialNo = 1;
            try
            {
                this.PerformanceTimerPool.Action("Transfer Thread", true);

                byte[] receiveBuffer;

                Processor.SetWriteTimeoutDelegate(KeepAliveWithRelayService);

                AveThreadWrapper threadWrapper = AveThreadUtility.CurrentThreadWrapper;
                while (threadWrapper.KeepRunning)
                {
                    bool ret = ReceiveBufferFromRelayService(serialNo, out receiveBuffer);
                    if (ret == false)
                    {
                        Processor.FinishWrite();
                        break;
                    }
                    serialNo++;
                    Processor.Write(receiveBuffer, 0, receiveBuffer.Length);
                }
            }
            catch (Exception ex)
            {
                DataTransferWorkStatus = DataTransferWorkStatus.ReceiverError;
                DataTransferErrorMessage += "Get Buffer And Save It Failed:" + ex.ToString();
                logger.Error(DataTransferErrorMessage);//TODO
            }
            finally
            {
                logger.Debug("Receiver {0} buffer within {1}.", serialNo, DateTime.Now - currentTime);
                Processor.FinishWrite();
                this.PerformanceTimerPool.Action("Transfer Thread", false);
            }
        }

        protected override void ConvertThread()
        {
            DateTime currentTime = DateTime.Now;
            long blockCount = 0;
            try
            {
                this.PerformanceTimerPool.Action("Convert Thread", true);
                AveThreadWrapper threadWrapper = AveThreadUtility.CurrentThreadWrapper;
                while (threadWrapper.KeepRunning)
                {
                    AveDataBlock freeDataBlock = TakeFreeBlock(false);
                    var returnDataBlock = Processor.Read(freeDataBlock);

                    if (returnDataBlock != null)
                    {
                        blockCount++;
                        AveDataBlockType dataBlockType = returnDataBlock.Type;
                        InputQueue.PutWorkingBlock(freeDataBlock);

                        if (dataBlockType == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (DataTransferNetworkException e)
            {
                //如果是这个Exception，就不需要做任何操作，主线程会继续抛
                logger.Warn("There is a data transfer network exception:{0} in convert thread when the session is {1}.", e.ToString(), this.SessionId);
            }
            catch (Exception ex)
            {
                DataTransferWorkStatus = DataTransferWorkStatus.ConvertError;
                DataTransferErrorMessage += "Convert Stream To DataBlock Failed:" + ex.ToString();
                logger.Error(DataTransferErrorMessage);//TODO
            }
            finally
            {
                logger.Debug("The total working block is {0} within {1}", blockCount, DateTime.Now - currentTime);
                this.PerformanceTimerPool.Action("Convert Thread", false);
            }
        }

        #region IDataReceiver 

        public string GetNextFileHead()
        {
            this.PerformanceTimerPool.Action("GetNextFileHead", true);

            //ADO-24538，接收数据的时候，也需要检查一下Data Transfer的状态
            CheckDataTransferIsValid();

            string fileHead = null;

            GetWorkingBlock(AveDataBlockType.HEADER_TYPE);
            
            if (WorkingBlock.Type != AveDataBlockType.CLOSE_CONNECTION_TYPE)
            {
                fileHead = WorkingBlock.RetrieveString();
            }

            this.PerformanceTimerPool.Action("GetNextFileHead", false);

            return fileHead;
        }

        public string GetFileTail()
        {
            //接收数据的时候，也需要检查一下Data Transfer的状态
            CheckDataTransferIsValid();

            if (WorkingBlock == null)
            {
                throw new Exception("Logic error");
            }
            else
            {
                if (WorkingBlock.Type == AveDataBlockType.TAIL_TYPE)
                {
                    return WorkingBlock.RetrieveString();
                }
                else
                {
                    throw new Exception("Current State is " + WorkingBlock.Type.ToString());
                }
            }

            return string.Empty;
        }

        public int ReadBytes(byte[] buffer, int len)
        {
            return ReadBytes(buffer, 0, len);
        }

        public int ReadBytes(byte[] buffer, int offset, int len)
        {
            this.PerformanceTimerPool.Action("ReadBytes", true);

            //接收数据的时候，也需要检查一下Data Transfer的状态
            CheckDataTransferIsValid();

            int length = 0;
            do
            {
                if (WorkingBlock.Type == AveDataBlockType.TAIL_TYPE || WorkingBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                {
                    break;
                }
                else
                {
                    if (len == 0)
                    {
                        break;
                    }

                    if (WorkingBlock.Type == AveDataBlockType.DATA_TYPE || WorkingBlock.Type == AveDataBlockType.CONTENTDATA_TYPE)
                    {
                        int dataSize = WorkingBlock.DataSize;

                        if (len <= dataSize)
                        {
                            WorkingBlock.CopyTo(buffer, offset, len);
                            WorkingBlock.AdjustDataBlock(len);
                            offset += len;
                            length += len;
                            len = 0;
                            break;
                        }
                        else
                        {
                            if (dataSize > 0)
                            {
                                WorkingBlock.CopyTo(buffer, offset, dataSize);
                                len -= dataSize;
                                offset += dataSize;
                                length += dataSize;
                                WorkingBlock.ClearDataBuffer();
                            }
                        }
                    }
                    else
                    {
                        //do something for future
                    }

                    GetNextBlock();
                }
            } while (true);

            this.PerformanceTimerPool.Action("ReadBytes", false);

            return length;
        }

        public string Close()
        {
            string errorMessage = string.Empty;
            try
            {
                WaitClose(true);
            }
            catch (Exception e)
            {
                errorMessage = e.ToString();
            }
            finally
            {
                OutputDetails();
            }
            return errorMessage;
        }

        public void Stop(string message)
        {
            try
            {
                this.PerformanceTimerPool.Action("Stop", true);
                Processor.Close(true);
                Reset(true, true, false, false);
            }
            catch (Exception e)
            {
                logger.Warn("Stop the current data transfer failed:{0}", e.ToString());
            }
            finally
            {
                this.PerformanceTimerPool.Action("Stop", false);
                this.OutputDetails();
            }
        }

        #endregion
    }
}
