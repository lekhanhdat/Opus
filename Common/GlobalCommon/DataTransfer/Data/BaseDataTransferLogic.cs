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
using System.Text;
using System.Threading;
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Factory;
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon.Transfer.Data
{
    /// <summary>
    /// online数据传输的公共类，提供一些公用方法的封装
    /// </summary>
    public class BaseDataTransferLogic : IDisposable
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(BaseDataTransferLogic), false);

        #region BaseDataTransfer Fields

        private int bufferSize;

        /// <summary>
        /// 当前传输中数据包的分类编号
        /// </summary>
        private string sessionId;
        private string localIdentifier;
        private string remoteIdentifier;
        /// <summary>
        /// 实际发送数据信道接口，提供实际的数据发送处理实现
        /// </summary>
        private ITransferChannel transferChannel = null;
        private DataTransferSetting dataTransferSetting = null;
        private bool filterByServerFlag;

        private AveDataBlockQueue inputQueue;
        private AveDataBlock workingBlock;
        private DataBlockProcessorV2 processor;

        private int reconnectTimeout = -1;
        private DataTransferWorkStatus dataTransferWorkStatus = DataTransferWorkStatus.Stopped;    //当前传输逻辑的工作状况
        private string dataTransferErrorMessage = string.Empty; //异常错误消息。
        private DataTransferResultStatus dataTransferResultStatus = null;
        private Thread mainThread;
        private DateTime lastCommunicationTime = DateTime.MinValue;

        private AveThreadWrapper convertThread;
        private AveThreadWrapper transferThread;
        /// <summary>
        /// 0, not started,
        /// 1, running,
        /// 2, end
        /// </summary>
        private int dataTransferRunningStatus = 0;
        private CommonPerformanceTimerPool performanceTimerPool;
        internal DataPerformanceCounter performanceCounter;
        #endregion

        #region Public Properties
        /// <summary>
        /// 打印网络带宽控制信息到log中
        /// </summary>
        /// <returns>格式化的字符串</returns>
        public string ThrottleControlInfo
        {
            get
            {
                string throttleControlInfo = string.Empty;

                if (dataTransferSetting != null && dataTransferSetting.ThrottleControlInfo != null)
                {
                    throttleControlInfo = dataTransferSetting.ThrottleControlInfo.ToString();
                }
                else
                {
                    throttleControlInfo = "Throttle Control Status: Disabled.";
                }

                return throttleControlInfo;
            }
        }
        public int ReconnectTimeout
        {
            get
            {
                if (reconnectTimeout == -1)
                {
                    reconnectTimeout = DataTransferConfiguration.DefaultReconnectTimeout;
                }
                return reconnectTimeout;
            }
            set
            {
                if (value < DataTransferConfiguration.MinReconnectTimeout)
                {
                    reconnectTimeout = DataTransferConfiguration.MinReconnectTimeout;
                }
                else
                {
                    reconnectTimeout = value;
                }
                SetSessionTimeout();
            }
        }
        public DataTransferResultStatus DataTransferStatus
        {
            get
            {
                if (transferChannel != null && transferChannel.CurrentWorkStatus != null)
                {
                    return transferChannel.CurrentWorkStatus;
                }

                return dataTransferResultStatus;
            }
        }
        public DataTransferWorkStatus DataTransferWorkStatus
        {
            get { return dataTransferWorkStatus; }
            set { dataTransferWorkStatus = value; }
        }

        public DataTransferSetting DataTransferSetting
        {
            get { return dataTransferSetting; }
        }

        public string DataTransferErrorMessage
        {
            get { return dataTransferErrorMessage; }
            set { dataTransferErrorMessage = value; }
        }
        internal AveDataBlock WorkingBlock
        {
            get { return workingBlock; }
            set { workingBlock = value; }
        }
        internal ITransferChannel TransferChannel
        {
            get { return transferChannel; }
            set { transferChannel = value; }
        }
        internal AveDataBlockQueue InputQueue
        {
            get { return inputQueue; }
        }
        internal DataBlockProcessorV2 Processor
        {
            get { return processor; }
        }
        internal int DataTransferRunningStatus
        {
            get 
            {
                if (dataTransferRunningStatus == 1)
                {
                    if((transferThread == null || (!transferThread.IsAlive)) &&
                        (convertThread == null || (!convertThread.IsAlive)) &&
                        (processor == null || processor.ProcessBufferThread == null || (!processor.ProcessBufferThread.IsAlive)))
                    {
                        dataTransferRunningStatus = 2;
                    }
                }

                return dataTransferRunningStatus; 
            }
            set { dataTransferRunningStatus = value; }
        }
        internal DateTime LastCommunicationTime
        {
            get { return lastCommunicationTime; }
            set { lastCommunicationTime = value; }
        }
        internal string SessionId
        {
            get { return sessionId; }
        }
        internal string LocalIdentifier
        {
            get { return localIdentifier; }
        }
        internal string RemoteIdentifer
        {
            get { return remoteIdentifier; }
        }
        internal Thread MainThread
        {
            get { return mainThread; }
        }
        internal CommonPerformanceTimerPool PerformanceTimerPool
        {
            get { return performanceTimerPool; }
        }
        #endregion

        public BaseDataTransferLogic(bool filterByServerFlag = true, int bufferSize = 100)
            : this(string.Empty, string.Empty, filterByServerFlag, bufferSize)
        {
        }

        public BaseDataTransferLogic(string localIdentifier, string remoteIdentifier, bool filterByServerFlag = true, int bufferSize = 100)
        {
            this.localIdentifier = localIdentifier;
            this.remoteIdentifier = remoteIdentifier;
            this.filterByServerFlag = filterByServerFlag;
            this.bufferSize = bufferSize > 1 ? bufferSize : 100;
            this.reconnectTimeout = DataTransferConfiguration.DefaultReconnectTimeout;
            this.performanceTimerPool = new CommonPerformanceTimerPool(DataTransferConfiguration.DisablePerformanceLogger);
            this.performanceCounter = new DataPerformanceCounter();
        }

        public virtual Boolean Open(DataTransferSetting setting, string sessionId)
        {
            Reset(true, true, false, true);
            //初始化主进程
            InitMainThread();
            //根据传输配置信息对象设定数据传输方式。
            dataTransferSetting = setting;
            ReconnectTimeout = setting.ReconnectTimeout;
            //设定当前的数据标实
            this.sessionId = sessionId;
            this.performanceCounter.Init(DataTransferConfiguration.EnablePerformanceCounter, this.sessionId);
            //启动连接方法
            SetupConnection(true);
            //用来初始化服务器上的Timeout时间，保证两端Timeout时间一致
            SetSessionTimeout();
            //初始化工作线程
            InitDataTransferWorkerThread();
            return true;
        }

        /// <summary>
        /// 关闭所有线程。
        /// </summary>
        protected void Reset(bool force, bool clearAllSession, bool checkClose, bool clearStatus)
        {
            //DataTransferLogger.Logger("start to reset the data transfer, sessionId:{0}, identifier:{1}, remoteIdentifier:{2}, force:{3}, clearAllSession:{4}", mSessionId, mLocalIdentifier, mRemoteIdentifier, force, clearAllSession );
            try
            {
                this.PerformanceTimerPool.Action("Reset", true);
                if (convertThread != null)
                {
                    this.PerformanceTimerPool.Action("Stop Convert Thread", true);
                    convertThread.Stop(2000, string.Empty, force);
                    this.PerformanceTimerPool.Action("Stop Convert Thread", false);
                }
                if (transferThread != null)
                {
                    this.PerformanceTimerPool.Action("Stop Transfer Thread", true);
                    transferThread.Stop(2000, string.Empty, force);
                    this.PerformanceTimerPool.Action("Stop Transfer Thread", false);
                }
                if (processor != null)
                {
                    this.PerformanceTimerPool.Action("Stop Processor Thread", true);
                    processor.Close(force);
                    processor.Dispose();
                    this.PerformanceTimerPool.Action("Stop Processor Thread", false);
                }
                if (checkClose)
                {
                    this.PerformanceTimerPool.Action("Wait to close", true);
                    WaitHandShakeToClose();
                    this.PerformanceTimerPool.Action("Wait to close", false);
                }
                if (transferChannel != null)
                {
                    transferChannel.ClearBufferInSession(clearAllSession);
                    transferChannel.Close();
                    dataTransferResultStatus = transferChannel.CurrentWorkStatus;
                }

                if (inputQueue != null)
                {
                    inputQueue.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Reset Failed:{0} when the session is {1}.", ex.ToString(), SessionId);
            }
            finally
            {
                this.PerformanceTimerPool.Action("Reset", false);
            }

            sessionId = string.Empty;
            transferChannel = null;
            dataTransferSetting = null;

            inputQueue = null;
            workingBlock = null;
            processor = null;

            DataTransferWorkStatus = DataTransferWorkStatus.Stopped;    //当前传输逻辑的工作状况
            DataTransferErrorMessage = string.Empty; //异常错误消息。

            convertThread = null;
            transferThread = null;
            mainThread = null;

            dataTransferRunningStatus = 0;

            if (clearStatus)
            {
                dataTransferResultStatus = null;
            }

            //DataTransferLogger.Logger("end to reset the data transfer, sessionId:{0}, identifier:{1}, remoteIdentifier:{2}, force:{3}, clearAllSession:{4}", mSessionId, mLocalIdentifier, mRemoteIdentifier, force, clearAllSession);
        }

        /// <summary>
        /// 启动工作线程，
        /// </summary>
        private void InitDataTransferWorkerThread()
        {
            this.PerformanceTimerPool.Action("Initiate Worker Thread", true);
            CheckDataTransferIsValid();

            inputQueue = new AveDataBlockQueue(bufferSize);
            inputQueue.TimeOut = DataTransferConfiguration.TakeDataBlockTimeOut;
            processor = new DataBlockProcessorV2(dataTransferSetting.IsEncryption, dataTransferSetting.DataEncryptionInfo, dataTransferSetting.IsCompression, dataTransferSetting.CompressionLevel, filterByServerFlag, localIdentifier, this.SessionId);
            processor.DataProcessorExceptionCallback = ChangeWorkStatus;
            processor.PerformanceTimerPool = this.performanceTimerPool;
            processor.SetReadTimeoutDelegate(CheckDataTransferIsValid);
            processor.SetWriteTimeoutDelegate(CheckDataTransferIsValid);
            processor.Run();

            //convertThread = AveThreadUtility.StartThread(ConvertThread, string.Format("Convert Thread {0} {1}", this.sessionId, this.localIdentifier), string.Empty);
            transferThread = AveThreadUtility.StartThread(TransferThread, string.Format("Transfer Thread {0} {1}", this.sessionId, this.localIdentifier), string.Empty);
            //transferThread = AveThreadUtility.StartThread(TransferThreadTemp, string.Format("Transfer Thread {0} {1}", this.sessionId, this.localIdentifier), string.Empty);

            logger.Info("{0}\r\nReconnectTimeoutMinutes:{1}, Encryption:{2},Compression:{3}, CompressionLevel:{4}, FilterByServerFlag:{5}, BufferSize:{6}, IsSender:{7}, SessionId:{8}",
                          ThrottleControlInfo, ReconnectTimeout, dataTransferSetting.IsEncryption, dataTransferSetting.IsCompression, dataTransferSetting.CompressionLevel, filterByServerFlag, bufferSize,
                          dataTransferSetting.IsSender, this.SessionId);

            this.PerformanceTimerPool.Action("Initiate Worker Thread", false);
        }

        /// <summary>
        /// 启动一个客户端连接到数据传输服务端
        /// </summary>
        /// <returns>是否连接成功</returns>
        protected bool SetupConnection(bool isFirst)
        {
            this.PerformanceTimerPool.Action("Setup Connection", true);
            bool setupSuccessfully = false;
            DateTime currentTime = DateTime.UtcNow;
            DataTransferLogger.Logger("start to setup connection for sessionId:{0}, identifier:{1}, remoteIdentifier:{2}, isSender:{3}", sessionId, localIdentifier, remoteIdentifier, filterByServerFlag);
            while (true)
            {
                try
                {
                    if (OpeningConnection())
                    {
                        DataTransferWorkStatus = DataTransferWorkStatus.Running;
                        dataTransferRunningStatus = 1;
                        setupSuccessfully = true;
                        break;
                    }
                    else
                    {
                        logger.Warn("Cannot open data channel:{0} when the session is {1}.", DataTransferErrorMessage, this.SessionId);
                        if (DataTransferWorkStatus != Common.DataTransferWorkStatus.Retrying)
                        {
                            DataTransferWorkStatus = DataTransferWorkStatus.OpenError;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Cannot open data channel:{0} when the session is {1}.", ex.ToString(), this.SessionId);
                    DataTransferErrorMessage = "Setup connection failed:" + ex.ToString();
                    if (DataTransferWorkStatus != Common.DataTransferWorkStatus.Retrying)
                    {
                        DataTransferWorkStatus = DataTransferWorkStatus.OpenError;
                    }
                }

                if (currentTime.AddMinutes(ReconnectTimeout) < DateTime.UtcNow)
                {
                    DataTransferWorkStatus = DataTransferWorkStatus.Timeout;
                    break;
                }
                Thread.Sleep(1000);
            }

            this.PerformanceTimerPool.Action("Setup Connection", false);
            //DataTransferLogger.Logger("end to setup connection for sessionId:{0}, identifier:{1}, remoteIdentifier:{2}, isSender:{3}", mSessionId, mLocalIdentifier, mRemoteIdentifier, mFilterByServerFlag);

            if (isFirst)
            {
                InitSession();
            }

            return setupSuccessfully;
        }

        /// <summary>
        /// 使用实际的连接方式建立连接(WCF,Socket,SSL)
        /// </summary>
        /// <returns>是否连接成功</returns>
        private Boolean OpeningConnection()
        {
            transferChannel = DataChannelFactory.GetTransferChannel(dataTransferSetting);

            string errorMsg = string.Empty;
            Boolean ret = transferChannel.Open(sessionId, localIdentifier, remoteIdentifier, dataTransferSetting, out errorMsg);//, mDataTransferSetting.mIsSender, mDataTransferSetting.mDataFileDir);
            DataTransferErrorMessage = errorMsg;
            return ret;
        }

        private void InitSession()
        {
            this.PerformanceTimerPool.Action("Initiate Session", true);
            //DataTransferLogger.Logger("start to init session for sessionId:{0}, identifier:{1}, remoteIdentifier:{2}, isSender:{3}", mSessionId, mLocalIdentifier, mRemoteIdentifier, mFilterByServerFlag);
            DateTime currentTime = DateTime.UtcNow;
            if (filterByServerFlag)
            {
                while (true)
                {
                    var result = transferChannel.InitSession(sessionId, remoteIdentifier, false);
                    if (result == SessionStatus.IsReady)
                    {
                        break;
                    }
                    else if (result == SessionStatus.NonExist)
                    {
                        Thread.Sleep(2000);
                    }
                    else //if (result == SessionStatus.InitedOK)
                    {
                        DataTransferErrorMessage += string.Format("There is an error during the session initialization. the server flag:{0}, result:{1}", filterByServerFlag, result);
                        DataTransferWorkStatus = DataTransferWorkStatus.LogicError;
                        logger.Error(DataTransferErrorMessage);
                    }

                    if (currentTime.AddMinutes(ReconnectTimeout) < DateTime.UtcNow)
                    {
                        DataTransferWorkStatus = DataTransferWorkStatus.Timeout;
                        DataTransferErrorMessage += string.Format("Wait for destination to init the session timeout:{0}", ReconnectTimeout);
                    }

                    CheckDataTransferIsValid();
                }
            }
            else
            {
                var result = transferChannel.InitSession(sessionId, localIdentifier, true);
                if (result != SessionStatus.InitedOK)
                {
                    DataTransferErrorMessage += string.Format("There is an error during the session initialization. the server flag:{0}, result:{1}", filterByServerFlag, result);
                    DataTransferWorkStatus = DataTransferWorkStatus.LogicError;
                    logger.Error(DataTransferErrorMessage);
                }
                CheckDataTransferIsValid();
            }
            this.PerformanceTimerPool.Action("Initiate Session", false);
            //DataTransferLogger.Logger("end to init session for sessionId:{0}, identifier:{1}, remoteIdentifier:{2}, isSender:{3}", mSessionId, mLocalIdentifier, mRemoteIdentifier, mFilterByServerFlag);
        }

        /// <summary>
        /// 结束时候的等待操作。
        /// </summary>
        protected void WaitClose(bool clearSession)
        {
            this.PerformanceTimerPool.Action("WaitClose", true);
            Reset(false, clearSession, NeedRelativeNodeHandShake, false);
            this.PerformanceTimerPool.Action("WaitClose", false);
        }
        /// <summary>
        /// 根据当前的实际工作状态决定是否需要抛出异常
        /// </summary>
        protected void CheckDataTransferIsValid()
        {
            string tempErrorMessage = DataTransferErrorMessage;
            switch (DataTransferWorkStatus)
            {
                case DataTransferWorkStatus.Running:
                case DataTransferWorkStatus.Retrying:
                case DataTransferWorkStatus.Created:
                case DataTransferWorkStatus.Stopped:
                    break;
                case DataTransferWorkStatus.Timeout:
                case DataTransferWorkStatus.SendError:
                case DataTransferWorkStatus.ReceiverError:
                    if ((!string.IsNullOrEmpty(DataTransferErrorMessage)) && DataTransferErrorMessage.Length > 101)
                    {
                        DataTransferErrorMessage = DataTransferErrorMessage.Substring(0, 100);
                    }
                    throw new DataTransferNetworkException(tempErrorMessage);
                case DataTransferWorkStatus.DataSequenceConfusion:
                    throw new Exception(string.Format("The sequence of data is not correct when the session is {0}", this.SessionId));
                default:
                    if ((!string.IsNullOrEmpty(DataTransferErrorMessage)) && DataTransferErrorMessage.Length > 101)
                    {
                        DataTransferErrorMessage = DataTransferErrorMessage.Substring(0, 100);
                    }
                    throw new Exception(string.Format("The transfer status:{0} when the session is:{1}, exception:{2}", DataTransferWorkStatus, this.SessionId, tempErrorMessage));
            }
        }
        /// <summary>
        /// 数据收发处理线程函数，这里面是实际发送发送或接收数据的位置
        /// </summary>
        protected virtual void TransferThread()
        {

        }
        /// <summary>
        /// 将数据进行分包处理的地方
        /// </summary>
        protected virtual void ConvertThread()
        {

        }

        protected virtual void TransferThreadTemp()
        {
        }

        /// <summary>
        /// 用于结束时等待对方结束确认的线程
        /// </summary>
        protected virtual void WaitHandShakeToClose()
        {

        }

        /// <summary>
        /// 启动网络限速功能，根据实际设定适当的进行sleep操作。
        /// </summary>
        /// <param name="length">当前发送的数据字节大小</param>
        protected void ActiveThrottleControlInfo(long length)
        {
            if (dataTransferSetting != null && dataTransferSetting.ThrottleControlInfo != null)
            {
                this.PerformanceTimerPool.Action("ActiveThrottleControl", true);
                dataTransferSetting.ThrottleControlInfo.WriteBytesCount(length);
                this.PerformanceTimerPool.Action("ActiveThrottleControl", false);
            }
        }

        /// <summary>
        /// 调用CallBack
        /// </summary>
        protected void CallBackReconnectedRunCode()
        {
            if (dataTransferSetting != null && dataTransferSetting.CodeToRun != null)
            {
                try
                {
                    this.PerformanceTimerPool.Action("CallBackReconnectedRunCode", true);
                    dataTransferSetting.CodeToRun();
                }
                catch (Exception ex)
                {
                    logger.Error("Call callBack function failed:{0} when the session is {1}.", ex.ToString(), this.SessionId);
                }
                finally
                {
                    this.PerformanceTimerPool.Action("CallBackReconnectedRunCode", false);
                }
            }
        }

        /// <summary>
        /// 激活上层程序
        /// </summary>
        protected void ActiveTransferNotifier()
        {
            if (dataTransferSetting != null && dataTransferSetting.Notifier != null)
            {
                try
                {
                    this.PerformanceTimerPool.Action("TransferNotifier", true);
                    dataTransferSetting.Notifier.OnActive();
                }
                catch (Exception ex)
                {
                    logger.Error("Active transfer notifier failed:{0} when the session is {1}.", ex.ToString(), SessionId);
                }
                finally
                {
                    this.PerformanceTimerPool.Action("TransferNotifier", false);
                }
            }
        }

        protected void KeepAliveWithRelayService()
        {
            CheckDataTransferIsValid();
            this.PerformanceTimerPool.Action("KeepAlive With Relay Service", true);
            if (LastCommunicationTime == DateTime.MinValue)
            {
                LastCommunicationTime = DateTime.Now;
            }
            else
            {
                if (ReconnectTimeout != 0)
                {
                    bool shouldKeepAlive = false;

                    if (LastCommunicationTime.AddMinutes(ReconnectTimeout / 2.0) < DateTime.Now)
                    {
                        if (MainThread != null && MainThread.IsAlive)
                        {
                            shouldKeepAlive = true;
                        }
                        else
                        {
                            //如果进程不存在，或者Thread不是Alive，则不去更新
                            shouldKeepAlive = false;
                        }
                    }

                    if (shouldKeepAlive)
                    {
                        LastCommunicationTime = DateTime.Now;

                        DateTime currentTime = DateTime.Now;

                        while (true)
                        {
                            try
                            {
                                //DataTransferWorkStatus = DataTransferWorkStatus.Running;
                                if (filterByServerFlag)
                                {
                                    TransferChannel.KeepAlive(SessionId, RemoteIdentifer, true);
                                }
                                else
                                {
                                    TransferChannel.KeepAlive(SessionId, LocalIdentifier, false);
                                }
                                break;
                            }
                            catch (Exception ex)
                            {
                                //网络出现异常，过一会重试
                                logger.Error("Keep alive with relay service failed:{0} when the session is {1}.", ex.ToString(), this.SessionId);
                                
                                DataTransferWorkStatus = DataTransferWorkStatus.Retrying;

                                SetupConnection(false);

                                CallBackReconnectedRunCode();

                                if (currentTime.AddMinutes(ReconnectTimeout / 2.0) < DateTime.Now)
                                {
                                    DataTransferWorkStatus = DataTransferWorkStatus.Timeout;
                                    DataTransferErrorMessage = string.Format("Cannot keep alive with relay service, retry timeout:{0} minute.", ReconnectTimeout / 2.0);
                                }
                            }

                            CheckDataTransferIsValid();
                        }
                        CheckDataTransferIsValid();
                    }
                }
            }
            this.PerformanceTimerPool.Action("KeepAlive With Relay Service", false);
        }

        protected AveDataBlock TakeFreeBlock(bool checkRunningStatus)
        {
            AveDataBlock dataBlock = null;

            int times = 0;

            this.PerformanceTimerPool.Action("TakeFreeBlock", true);

            while (true)
            {
                try
                {
                    dataBlock = InputQueue.TakeFreeBlock();
                    if (dataBlock != null)
                    {
                        break;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (BlockQueueSyncException e)
                {
                    logger.Warn("Exception occured while taking free block. {0}", e.ToString());
                    if (checkRunningStatus && DataTransferRunningStatus == 2)
                    {
                        times++;
                        if (times > 3)
                        {
                            DataTransferWorkStatus = DataTransferWorkStatus.LogicError;
                            DataTransferErrorMessage += "The working process is not alive.";
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    DataTransferWorkStatus = DataTransferWorkStatus.UnHandlerError;
                    DataTransferErrorMessage += ex.ToString();
                }

                CheckDataTransferIsValid();
            }

            this.PerformanceTimerPool.Action("TakeFreeBlock", false);

            return dataBlock;
        }

        protected AveDataBlock TakeWorkingBlock(bool checkRunningStatus)
        {
            AveDataBlock dataBlock = null;

            int times = 0;

            this.PerformanceTimerPool.Action("TakeWorkingBlock", true);

            while (true)
            {
                try
                {
                    dataBlock = InputQueue.TakeWorkingBlock();
                    if (dataBlock != null)
                    {
                        break;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (BlockQueueSyncException e)
                {
                    logger.Warn("Exception occured while taking free block. {0}", e.ToString());
                    if (checkRunningStatus && DataTransferRunningStatus == 2)
                    {
                        times++;
                        if (times > 3)
                        {
                            DataTransferWorkStatus = DataTransferWorkStatus.LogicError;
                            DataTransferErrorMessage += "The working process is not alive.";
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    DataTransferWorkStatus = DataTransferWorkStatus.UnHandlerError;
                    DataTransferErrorMessage += ex.ToString();
                }

                CheckDataTransferIsValid();
            }

            this.PerformanceTimerPool.Action("TakeWorkingBlock", false);

            return dataBlock;
        }

        protected bool NeedRelativeNodeHandShake
        {
            get { return !(DataTransferWorkStatus == DataTransferWorkStatus.Timeout || DataTransferWorkStatus == DataTransferWorkStatus.SendError || DataTransferWorkStatus == DataTransferWorkStatus.ReceiverError); }
        }

        internal void InitMainThread()
        {
            mainThread = Thread.CurrentThread;
        }

        /// <summary>
        /// 用于Processor获取其他程序改变work status用
        /// </summary>
        /// <param name="obj"></param>
        internal void ChangeWorkStatus(object obj)
        {
            if (obj != null && obj is Tuple<DataTransferWorkStatus, string>)
            {
                Tuple<DataTransferWorkStatus, string> status = obj as Tuple<DataTransferWorkStatus, string>;
                DataTransferWorkStatus = status.Item1;
                DataTransferErrorMessage += status.Item2;
            }
        }

        /// <summary>
        /// 更新下服务器端的Timeout时间
        /// </summary>
        internal void SetSessionTimeout()
        {
            if (transferChannel != null)
            {
                this.PerformanceTimerPool.Action("SetSessionTimeout", true);
                if (filterByServerFlag)
                {
                    transferChannel.SetTimeout(sessionId, remoteIdentifier, reconnectTimeout, true);
                }
                else
                {
                    transferChannel.SetTimeout(sessionId, localIdentifier, reconnectTimeout, false);
                }
                this.PerformanceTimerPool.Action("SetSessionTimeout", false);
            }
        }

        /// <summary>
        /// 最后用来输出一些log信息
        /// </summary>
        internal void OutputDetails()
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                DataTransferResultStatus status = this.DataTransferStatus;
                builder.AppendLine();
                builder.AppendFormat("SessionId:{0}, Local Identifier:{1}, Remote Identifier:{2}.\r\n", sessionId, localIdentifier, remoteIdentifier);
                if (status != null)
                {
                    builder.AppendFormat("Data Status-> TotalSentSize:{0, 10}, SentSpeed:{1, 10}, TotalReceivedSize:{2, 10}, ReceivedSpeed:{3, 10}\r\n",
                        status.TotalBytesSent, status.BytesSentSpeed, status.TotalBytesReceived, status.BytesReceivedSpeed);
                }
                builder.AppendFormat("Performance Report:\r\n{0}\r\n", this.performanceTimerPool.ToString());

                logger.Info(builder.ToString());
            }
            catch (Exception ex)
            {
                logger.Warn("Output the details of data transfer failed:{0}.", ex.ToString());
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Reset(true, false, false, false);
        }

        #endregion
    }

    ///// <summary>
    ///// online数据传输的公共类，提供一些公用方法的封装
    ///// </summary>
    //public class BaseDataTransferLogicV2
    //{
    //    protected static AveLogger logger = AveLogger.GetInstance(typeof(BaseDataTransferLogicV2));

    //    #region BaseDataTransfer Fields
    //    private int bufferSize;
    //    /// <summary>
    //    /// 当前传输中数据包的分类编号
    //    /// </summary>
    //    private string localSessionId = string.Empty;
    //    private string localIdentifier = string.Empty;
    //    private string remoteIdentifier = string.Empty;
    //    /// <summary>
    //    /// 实际发送数据信道接口，提供实际的数据发送处理实现
    //    /// </summary>
    //    private ITransferChannel transferChannel = null;
    //    private DataTransferSetting dataTransferSetting = null;

    //    private AveDataBlockQueue inputQueue;
    //    private AveDataBlockQueue outputQueue;
    //    private AveDataBlock inputWorkingBlock;
    //    private AveDataBlock outputWorkingBlock;
    //    private DataBlockProcessorV2 inputDataProcessor;
    //    private DataBlockProcessorV2 outputDataProcessor;

    //    private int reconnectTimeout;
    //    private DataTransferWorkStatus dataTransferStatus = DataTransferWorkStatus.Stopped;    //当前传输逻辑的工作状况
    //    private string errorMessage = string.Empty; //异常错误消息。

    //    private AveThreadWrapper convertThread;
    //    private AveThreadWrapper transferThread;

    //    #endregion

    //    #region public properties
    //    public int ReconnectTimeout
    //    {
    //        get 
    //        {
    //            if (reconnectTimeout < 0)
    //            {
    //                reconnectTimeout = DataTransferConfiguration.DefaultReconnectTimeout;
    //            }
    //            return reconnectTimeout;
    //        }
    //        set 
    //        {
    //            if (value < DataTransferConfiguration.MinReconnectTimeout)
    //            {
    //                reconnectTimeout = DataTransferConfiguration.MinReconnectTimeout;
    //            }
    //            else
    //            {
    //                reconnectTimeout = value;
    //            }
    //        }
    //    }
    //    public DataTransferResultStatus TransferStatus
    //    {
    //        get 
    //        {
    //            if (transferChannel != null)
    //            {
    //                return transferChannel.CurrentWorkStatus;
    //            }
    //            return null;
    //        }
    //    }
    //    public DataTransferWorkStatus State
    //    {
    //        get { return dataTransferStatus; }
    //    }
    //    public string ErrorMessage
    //    {
    //        get { return errorMessage; }
    //    }
    //    /// <summary>
    //    /// 打印网络带宽控制信息到log中
    //    /// </summary>
    //    /// <returns>格式化的字符串</returns>
    //    private string ThrottleControlInfo
    //    {
    //        get
    //        {
    //            string throttleControlInfo = string.Empty;

    //            if (dataTransferSetting != null && dataTransferSetting.ThrottleControlInfo != null)
    //            {
    //                throttleControlInfo = dataTransferSetting.ThrottleControlInfo.ToString();
    //            }
    //            else
    //            {
    //                throttleControlInfo = "Throttle Control Status: Disabled.";
    //            }

    //            return throttleControlInfo;
    //        }
    //    }
    //    #endregion



    //    public BaseDataTransferLogicV2(int bufferSize = 100)
    //    {
    //        this.bufferSize = bufferSize > 1 ? bufferSize : 100;
    //    }

    //    public Boolean Open(DataTransferSetting setting, string sessionId)
    //    {
    //        Reset();
    //        //根据传输配置信息对象设定数据传输方式。
    //        this.dataTransferSetting = setting;
    //        this.ReconnectTimeout = setting.ReconnectTimeout;
    //        //设定当前的数据标实
    //        this.localSessionId = sessionId;
    //        //启动连接方法
    //        SetupConnection();
    //        //初始化工作线程
    //        InitDataTransferWorkerThread();
    //        return true;
    //    }

    //    #region private function
    //    /// <summary>
    //    /// 结束时候的等待操作。
    //    /// </summary>
    //    private void WaitClose(bool clearSession)
    //    {
    //        //mProcessor.WaitForExit(1800 * 1000);

    //        //convertThread.Join();

    //        //mTransferStream.FinishWrite();
    //        //transferThread.Join();

    //        //if (mTransferChannel != null)
    //        //{
    //        //    mTransferChannel.ClearBufferInSession(clearSession);
    //        //    mTransferChannel.Close();
    //        //    mTransferChannel = null;
    //        //}
    //        //dataTransferStatus = DataTransferWorkStatus.Stopped;
    //    }
    //    /// <summary>
    //    /// 根据当前的实际工作状态决定是否需要抛出异常
    //    /// </summary>
    //    private void CheckDataTransferIsValid()
    //    {
    //        switch (dataTransferStatus)
    //        {
    //            case DataTransferWorkStatus.Timeout:
    //                throw new DataTransferNetworkException(errorMessage);
    //                break;
    //            case DataTransferWorkStatus.DataSequenceConfusion:
    //                throw new Exception("The Sequence of data is not correct.");
    //                break;

    //        }
    //    }
    //    /// <summary>
    //    /// 数据收发处理线程函数，这里面是实际发送发送或接收数据的位置
    //    /// </summary>
    //    private void TransferThead()
    //    {

    //    }
    //    /// <summary>
    //    /// 将数据进行分包处理的地方
    //    /// </summary>
    //    private void ConvertThread()
    //    {

    //    }
    //    #region call back function

    //    protected void ActiveTransferNotifer()
    //    {
    //        if (this.dataTransferSetting != null && this.dataTransferSetting.Notifier != null)
    //        {
    //            AvePerformanceTimerPool.Start(DataTransferConstants.TransferNotifer);

    //            try
    //            {
    //                this.dataTransferSetting.Notifier.OnActive();
    //            }
    //            catch { }

    //            AvePerformanceTimerPool.Stop(DataTransferConstants.TransferNotifer);
    //        }
    //    }

    //    protected void CallBackReconnectedRunCode()
    //    {
    //        if (this.dataTransferSetting != null && this.dataTransferSetting.CodeToRun != null)
    //        {
    //            try
    //            {
    //                this.dataTransferSetting.CodeToRun();
    //            }
    //            catch { }
    //        }
    //    }
    //    #endregion

    //    private void Reset()
    //    {
    //        try
    //        {
    //            if (transferChannel != null)
    //            {
    //                transferChannel.Close();
    //                transferChannel = null;
    //            }
    //            localSessionId = string.Empty;
    //            localIdentifier = string.Empty;
    //            remoteIdentifier = string.Empty;
    //            dataTransferSetting = null;
    //            if (inputQueue != null)
    //            {
    //                inputQueue.Dispose();
    //                inputQueue = null;
    //            }
    //            if (outputQueue != null)
    //            {
    //                outputQueue.Dispose();
    //                outputQueue = null;
    //            }
    //            inputWorkingBlock = null;
    //            outputWorkingBlock = null;
    //            if (inputDataProcessor != null)
    //            {
    //                inputDataProcessor.Close();
    //            }
    //            if (outputDataProcessor != null)
    //            {
    //                outputDataProcessor.Close();
    //            }
    //            dataTransferStatus = DataTransferWorkStatus.Stopped;
    //            errorMessage = string.Empty;
    //            convertThread = null;
    //            transferThread = null;
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.Warn("Reset Data Transfer failed:{0}", ex.ToString());
    //        }
    //    }

    //    /// <summary>
    //    /// 启动一个客户端连接到数据传输服务端
    //    /// </summary>
    //    /// <returns>是否连接成功</returns>
    //    private bool SetupConnection()
    //    {
    //        bool setupSuccessfully = false;
    //        DateTime currentTime = DateTime.Now;
    //        while (true)
    //        {
    //            try
    //            {
    //                if (OpeningConnection())
    //                {
    //                    dataTransferStatus = DataTransferWorkStatus.Running;
    //                    setupSuccessfully = true;
    //                    break;
    //                }
    //                else
    //                {
    //                    logger.Warn("Cannot open data channel:" + errorMessage);
    //                    dataTransferStatus = DataTransferWorkStatus.OpenError;
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                logger.Warn("Cannot open data channel:" + ex.ToString());
    //                errorMessage = "Setup connection failed:" + ex.ToString();
    //                dataTransferStatus = DataTransferWorkStatus.OpenError;
    //            }

    //            if (currentTime.AddMinutes(DataTransferConfiguration.DefaultReconnectTimeout) < DateTime.Now)
    //            {
    //                dataTransferStatus = DataTransferWorkStatus.Timeout;
    //                break;
    //            }
    //            Thread.Sleep(1000);
    //        }

    //        return setupSuccessfully;
    //    }

    //    /// <summary>
    //    /// 启动网络限速功能，根据实际设定适当的进行sleep操作。
    //    /// </summary>
    //    /// <param name="length">当前发送的数据字节大小</param>
    //    private void ActiveThrottleControlInfo(long length)
    //    {
    //        if (this.dataTransferSetting != null && this.dataTransferSetting.ThrottleControlInfo != null)
    //        {
    //            this.dataTransferSetting.ThrottleControlInfo.WriteBytesCount(length);
    //        }
    //    }

    //    /// <summary>
    //    /// 启动工作线程，
    //    /// </summary>
    //    private void InitDataTransferWorkerThread()
    //    {
    //        CheckDataTransferIsValid();

    //        inputQueue = new AveDataBlockQueue(bufferSize);
    //        outputQueue = new AveDataBlockQueue(bufferSize);

    //        //mProcessor = new DataBlockProcessor(inputQueue, outputQueue, dataTransferSetting.IsEncryption, dataTransferSetting.CompressionLevel, filterByServerFlag);
    //        //mProcessor.Start();

    //        convertThread = AveThreadUtility.StartThread(ConvertThread, "Convert Thread", string.Empty);
    //        transferThread = AveThreadUtility.StartThread(TransferThead, "Transfer Thread", string.Empty);

    //        logger.Info("{0}\r\nReconnectTimeoutMinutes:{1}, Encryption:{2}, CompressionLevel:{3}, BufferSize:{4}, IsSender:{5}",
    //                      ThrottleControlInfo, reconnectTimeout, dataTransferSetting.IsEncryption, dataTransferSetting.CompressionLevel, bufferSize,
    //                      dataTransferSetting.IsSender);
    //    }
    //    /// <summary>
    //    /// 使用实际的连接方式建立连接(WCF,Socket,SSL)
    //    /// </summary>
    //    /// <returns>是否连接成功</returns>
    //    private Boolean OpeningConnection()
    //    {
    //        transferChannel = DataChannelFactory.GetTransferChannel(dataTransferSetting);

    //        string errorMsg = string.Empty;
    //        Boolean ret = transferChannel.Open(localSessionId, localIdentifier, remoteIdentifier, dataTransferSetting, out errorMsg);//, mDataTransferSetting.mIsSender, mDataTransferSetting.mDataFileDir);
    //        errorMessage = errorMsg;
    //        return ret;
    //    }
    //    #endregion
    //}
}
