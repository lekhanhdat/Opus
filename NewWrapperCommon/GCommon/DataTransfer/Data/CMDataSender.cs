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
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.IO;
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon.Transfer.Data
{
    /// <summary>
    /// 实现发送接口，定义Online时候的数据发送逻辑
    /// </summary>
    public class CMDataSender : BaseDataTransferLogic, IDataSender
    {
        private long mDataSize;
        private long inputDataBlockSize = 0L;

        public CMDataSender(int bufferSize = 100)
            : base(DataTransferConstants.SenderIdentifier, DataTransferConstants.ReceiverIdentifier, true, bufferSize)
        {
            inputDataBlockSize = 0L;
        }

        private void SendBufferToRelayService(long serialNo, byte[] buffer)
        {
            CheckDataTransferIsValid();
            this.PerformanceTimerPool.Action("SendBuffer", true);
            DateTime currentTime = DateTime.UtcNow;

            bool checking = false;

            while (true)
            {
                try
                {
                    //DataTransferWorkStatus = DataTransferWorkStatus.Running;
                    ActiveTransferNotifier();

                    BufferStatus resultCode = BufferStatus.NotInited;
                    if (!checking)
                    {
                        this.PerformanceTimerPool.Action("SendBinary", true);
                        this.performanceCounter.BeginSend();
                        resultCode = TransferChannel.SendBinary(serialNo, buffer);
                        this.performanceCounter.EndSend(buffer.LongLength);
                        this.PerformanceTimerPool.Action("SendBinary", false);

                        if (resultCode == BufferStatus.OK)
                        {
                            ActiveThrottleControlInfo(buffer.LongLength);
                            //成功发送到中转服务
                            break;
                        }
                        else if (resultCode == BufferStatus.WriteTimeout)
                        {
                            DataTransferWorkStatus = DataTransferWorkStatus.Timeout;
                            DataTransferErrorMessage += "\r\nDestination does not read buffer in time.";
                            break;
                        }
                        else if (resultCode == BufferStatus.NotInited)
                        {
                            DataTransferWorkStatus = DataTransferWorkStatus.UnHandlerError;
                            DataTransferErrorMessage += "\r\nDestination stop the buffer session.";
                            break;
                        }
                        else if (resultCode == BufferStatus.BufferIsFull)
                        {
                            Thread.Sleep(10);
                            checking = true;
                        }
                        else
                        {
                            DataTransferWorkStatus = DataTransferWorkStatus.UnHandlerError;
                            DataTransferErrorMessage += "\r\nThe result code from destination is " + resultCode;
                            break;
                        }
                        //checking = true;
                    }
                    else
                    {
                        this.PerformanceTimerPool.Action("CheckBinary", true);
                        resultCode = TransferChannel.CheckBinary(serialNo, true);
                        this.PerformanceTimerPool.Action("CheckBinary", false);
                        if (resultCode == BufferStatus.OK)
                        {
                            checking = false;
                        }
                        else if (resultCode == BufferStatus.WriteTimeout)
                        {
                            DataTransferWorkStatus = DataTransferWorkStatus.Timeout;
                            DataTransferErrorMessage += "\r\nDestination does not read buffer in time.";
                            break;
                        }
                        else if (resultCode == BufferStatus.NotInited)
                        {
                            DataTransferWorkStatus = DataTransferWorkStatus.UnHandlerError;
                            DataTransferErrorMessage += "\r\nDestination stop the buffer session.";
                            break;
                        }
                        else if (resultCode == BufferStatus.BufferIsFull)
                        {
                            //中转服务缓冲已满，过一会重试
                            Thread.Sleep(500);
                            //continue...
                        }
                        else
                        {
                            DataTransferWorkStatus = DataTransferWorkStatus.UnHandlerError;
                            DataTransferErrorMessage += "\r\nThe result code from destination is " + resultCode;
                            break;
                        }
                    }

                    currentTime = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    //网络出现异常，过一会重试
                    logger.Error("Send Buffer To Relay Service with session:{0} failed:{1}", this.SessionId, ex.ToString());

                    DataTransferWorkStatus = DataTransferWorkStatus.Retrying;

                    if (TransferChannel != null)
                    {
                        TransferChannel.Close();
                        //CloseChannel();
                    }
                    if (SetupConnection(false))
                    {
                        //调用重连后的处理事件
                        CallBackReconnectedRunCode();
                    }

                    if (currentTime.AddMinutes(ReconnectTimeout) < DateTime.UtcNow)
                    {
                        DataTransferWorkStatus = DataTransferWorkStatus.Timeout;
                        DataTransferErrorMessage += string.Format("Cannot put buffer into cache, retry timeout:{0} minute.", ReconnectTimeout);
                    }
                }

                CheckDataTransferIsValid();
            }
            CheckDataTransferIsValid();
            this.PerformanceTimerPool.Action("SendBuffer", false);
        }

        protected override void TransferThreadTemp()
        {
            this.PerformanceTimerPool.Action("TransferThread", true);
            DateTime currentTime = DateTime.Now;
            long serialNo = 1;
            try
            {
                byte[] buffer = new byte[DataTransferSetting.SendBufferSize];
                Processor.SetReadTimeoutDelegate(KeepAliveWithRelayService);
                Random tempRandom = new Random();
                tempRandom.NextBytes(buffer);
                AveThreadWrapper threadWrapper = AveThreadUtility.CurrentThreadWrapper;
                while (threadWrapper.KeepRunning)
                {
                    for (int i = 0; i < 9600; i++)
                    {
                        SendBufferToRelayService(serialNo, buffer);
                        serialNo++;
                    }
                    SendBufferToRelayService(-1, new byte[0]);
                    break;
                }
            }
            catch (Exception ex)
            {
                DataTransferWorkStatus = DataTransferWorkStatus.SendError;
                DataTransferErrorMessage += "Get Buffer And Send To Relay Service Failed:" + ex.ToString();
                logger.Error("Transfer the data failed:{0} when the session is {1}.", ex.ToString(), this.SessionId);
            }
            finally
            {
                logger.Info("Send {0} buffer within {1}.", serialNo, DateTime.Now - currentTime);
                this.PerformanceTimerPool.Action("TransferThread", false);
            }
        }

        protected override void TransferThread()
        {
            this.PerformanceTimerPool.Action("TransferThread", true);
            DateTime currentTime = DateTime.Now;
            long serialNo = 1;
            try
            {
                byte[] buffer = new byte[DataTransferSetting.SendBufferSize];
                Processor.SetReadTimeoutDelegate(KeepAliveWithRelayService);
                AveThreadWrapper threadWrapper = AveThreadUtility.CurrentThreadWrapper;
                while (threadWrapper.KeepRunning)
                {
                    int needRead = DataTransferSetting.SendBufferSize;
                    int totalRead = 0;
                    while (totalRead < DataTransferSetting.SendBufferSize)
                    {
                        int readLen = Processor.Read(buffer, totalRead, needRead, false);//mTransferStream.Read(buffer, totalRead, needRead);
                        if (readLen == 0) break;
                        totalRead += readLen;
                        needRead -= readLen;
                    }
                    if (totalRead == 0)
                    {
                        //发送流已经结束，没有数据要再发送
                        SendBufferToRelayService(-1, new byte[0]);
                        break;
                    }
                    byte[] sendBuffer = buffer;
                    if (totalRead < DataTransferSetting.SendBufferSize)
                    {
                        //发送流的末尾，不够一个SEND_BUFFER_SIZE
                        sendBuffer = new byte[totalRead];
                        Array.Copy(buffer, 0, sendBuffer, 0, totalRead);
                    }
                    SendBufferToRelayService(serialNo, sendBuffer);
                    serialNo++;
                }
            }
            catch (Exception ex)
            {
                lock (DataTransferWorkStatusLock)
                {
                    //TransferThread exception status can overwrite other thread status;
                    DataTransferWorkStatus = DataTransferWorkStatus.SendError;
                    DataTransferErrorMessage += "Get Buffer And Send To Relay Service Failed:" + ex.ToString();
                }
                logger.Error("Transfer the data failed:{0} when the session is {1}.", ex.ToString(), this.SessionId);
            }
            finally
            {
                logger.Info("Send {0} buffer within {1}.", serialNo, DateTime.Now - currentTime);
                this.PerformanceTimerPool.Action("TransferThread", false);
            }
        }

        protected override void ConvertThread()
        {
            this.PerformanceTimerPool.Action("ConvertThread", true);
            DateTime currentTime = DateTime.Now;
            long blockCount = 0L;
            try
            {
                AveThreadWrapper threadWrapper = AveThreadUtility.CurrentThreadWrapper;
                while (threadWrapper.KeepRunning)
                {
                    AveDataBlock dataBlock = TakeWorkingBlock(false);
                    AveDataBlockType currentDataBlockType = dataBlock.Type;
                    Processor.Write(dataBlock);
                    InputQueue.PutFreeBlock(dataBlock);
                    blockCount++;
                    if (currentDataBlockType == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                    {
                        Processor.FinishWrite();
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
                DataTransferErrorMessage += "Convert DataBlock To Stream Failed:" + ex.ToString();
                logger.Error(DataTransferErrorMessage);//TODO
            }
            finally
            {
                logger.Debug("The total working buffer is {0} within {1}.", blockCount, DateTime.Now - currentTime);
                Processor.FinishWrite();
                this.PerformanceTimerPool.Action("ConvertThread", false);
            }
        }

        protected override void WaitHandShakeToClose()
        {
            DateTime dateTime = DateTime.Now;
            logger.Info("Sender has finish to send data, and wait for relative node to close. Current Time is:{0}, sessionId:{1}", dateTime.ToString(), SessionId);

            CheckDataTransferIsValid();

            while (dateTime.AddSeconds(DataTransferSetting.CloseTimeOut) > DateTime.Now)
            {
                if (TransferChannel.BufferSessionInUse(false))
                {
                    Thread.Sleep(200);
                }
                else
                {
                    logger.Info("Receiver receive and restore finish. Handshake finish, sessionId:{0}.", SessionId);
                    break;
                }
            }
        }

        private AveDataBlock GetWorkingBlock(AveDataBlockType type)
        {
            //TODO need to catch the timeout exception when cannot get free block
            WorkingBlock = TakeFreeBlock(true);
            WorkingBlock.SerialNumber = 0;
            WorkingBlock.DataSize = 0;
            WorkingBlock.Type = type;

            return WorkingBlock;
        }

        private void PutWorkingBlock()
        {
            //InputQueue.PutWorkingBlock(WorkingBlock);
            Processor.Write(WorkingBlock);
            InputQueue.PutFreeBlock(WorkingBlock);
            WorkingBlock = null;
            inputDataBlockSize++;
        }

        #region IDataSender 接口的实现

        public void WriteHead(string xml)
        {
            this.PerformanceTimerPool.Action("WriteHead", true);

            CheckDataTransferIsValid();

            mDataSize = 0;
            if (WorkingBlock != null)
            {
                PutWorkingBlock();
            }


            GetWorkingBlock(AveDataBlockType.HEADER_TYPE);
            WorkingBlock.PutString(xml);
            PutWorkingBlock();
            this.PerformanceTimerPool.Action("WriteHead", false);
        }

        public void WriteData(byte[] buf, int offset, int length)
        {
            this.PerformanceTimerPool.Action("WriteData", true);

            CheckDataTransferIsValid();

            mDataSize += length;

            if (WorkingBlock != null && WorkingBlock.Type != AveDataBlockType.DATA_TYPE)
            {
                PutWorkingBlock();
            }

            if (WorkingBlock == null)
            {
                GetWorkingBlock(AveDataBlockType.DATA_TYPE);
            }

            int availableSpace = WorkingBlock.Buffer.Length - AveDataBlock.DATA_BLOCK_HEADER_LEN - WorkingBlock.DataSize;

            while (length > availableSpace)
            {
                if (availableSpace > 0)
                {
                    WorkingBlock.AppendBuffer(buf, offset, availableSpace);
                    offset += availableSpace;
                    length -= availableSpace;
                }
                PutWorkingBlock();
                GetWorkingBlock(AveDataBlockType.DATA_TYPE);

                availableSpace = WorkingBlock.Buffer.Length - AveDataBlock.DATA_BLOCK_HEADER_LEN - WorkingBlock.DataSize;
            }

            if (length > 0)
            {
                WorkingBlock.AppendBuffer(buf, offset, length);
            }
            this.PerformanceTimerPool.Action("WriteData", false);
        }

        public void WriteTail(string xml)
        {
            WriteTail(xml, true);
        }

        public void WriteTail(string xml, bool isOK)
        {
            this.PerformanceTimerPool.Action("WriteTail", true);
            CheckDataTransferIsValid();

            //if (isOK)
            //{
            //    xml = "<FileTail length=\"" + mDataSize + "\">" + xml + "</FileTail>";
            //}
            //else
            //{
            //    xml = "<FileTail failed=\"true\" length=\"" + mDataSize + "\">" + xml + "</FileTail>";
            //}

            if (WorkingBlock != null)
            {
                PutWorkingBlock();
            }

            GetWorkingBlock(AveDataBlockType.TAIL_TYPE);
            WorkingBlock.PutString(xml);
            PutWorkingBlock();
            this.PerformanceTimerPool.Action("WriteTail", false);
        }

        public string Close()
        {
            string errorMessage = string.Empty;

            ///由于HTTP Mode备份比较快，所以可能会出现close的时候数据没有发送完。
            CheckDataTransferIsValid();

            try
            {
                this.PerformanceTimerPool.Action("Close", true);
                if (WorkingBlock != null)
                {
                    PutWorkingBlock();
                }

                GetWorkingBlock(AveDataBlockType.CLOSE_CONNECTION_TYPE);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    WorkingBlock.PutString(string.Empty);
                }
                else
                {
                    WorkingBlock.PutString(errorMessage);
                }
                PutWorkingBlock();

                logger.Debug("Input working block number is {0} when the session is {1}.", inputDataBlockSize, this.SessionId);

                Processor.FinishWrite();

                WaitClose(false);
            }
            catch (DataTransferNetworkException)
            {
                throw;
            }
            catch (Exception e)
            {
                errorMessage = e.ToString();
            }
            finally
            {
                if (Processor != null)
                {
                    Processor.FinishWrite();
                }
                this.PerformanceTimerPool.Action("Close", false);
                this.OutputDetails();
            }
            return errorMessage;
        }

        public void Stop(string message)
        {
            try
            {
                this.PerformanceTimerPool.Action("Stop", true);
                logger.Debug("Input working block number is {0} when the session is {1}.", inputDataBlockSize, this.SessionId);
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

    /// <summary>
    /// CMDataSenderV1 which support stream mode
    /// </summary>
    internal class DataTransferWithStream : IDataSender, IDataReceiver
    {
        public bool Open(DataTransferSetting setting, string sessionId)
        {
            throw new NotImplementedException();
        }

        public void WriteHead(string xml)
        {
            throw new NotImplementedException();
        }

        public void WriteData(byte[] buf, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public void WriteTail(string xml)
        {
            throw new NotImplementedException();
        }

        public void WriteTail(string xml, bool isOK)
        {
            throw new NotImplementedException();
        }

        public string Close()
        {
            throw new NotImplementedException();
        }

        public DataTransferResultStatus DataTransferStatus
        {
            get { throw new NotImplementedException(); }
        }

        public void Stop(string message)
        {
            throw new NotImplementedException();
        }

        public string GetNextFileHead()
        {
            throw new NotImplementedException();
        }

        public string GetFileTail()
        {
            throw new NotImplementedException();
        }

        public int ReadBytes(byte[] buffer, int len)
        {
            throw new NotImplementedException();
        }

        public int ReadBytes(byte[] buffer, int offset, int len)
        {
            throw new NotImplementedException();
        }
    }
}
