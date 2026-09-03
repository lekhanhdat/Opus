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
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Data.Service;

namespace AvePoint.GCommon.Transfer.Data.Channel
{
    /// <summary>
    /// 进程内数举通道实现,用于支持IRelay接口
    /// 主要用于模拟和wcf服务端在进程内通讯，避免通过服务通讯减少网络资源的占用。
    /// </summary>
    public class InProcessChannel : ITransferChannel
    {
        private string mSessionId = string.Empty;//当前正在通讯的数据的sesstion id。
        private string mIdentifier = string.Empty;
        private string mRemoteIdentifier = string.Empty;
        private IRelay mRelayService;//WCF服务端接口定义
        private DataTransferResultStatus mCurrWorkStatus = new DataTransferResultStatus();//当前的数举处理工作状态

        #region ITransferChannel Members

        public DataTransferResultStatus CurrentWorkStatus
        {
            get { return mCurrWorkStatus; }
        }

        public bool Open(string sessionId, string identifier, string remoteIdentifier, DataTransferSetting settings, out string errorMessage)//Open(string sessionId, out string errorMessage, params object[] parameters)
        {
            bool openSuccessfully = false;
            errorMessage = string.Empty;
            this.Close();
            mSessionId = sessionId;
            mIdentifier = identifier;
            mRemoteIdentifier = remoteIdentifier;
            try
            {
                mRelayService = new RelayService();
                mRelayService.CheckStatus(mSessionId, mIdentifier);
                openSuccessfully = true;
                //while (true)
                //{
                //    mRelayService = GlobalWCFServiceInstanceManager.GetInstance(sessionId);
                //    if (mRelayService != null)
                //    {
                //        mRelayService.CheckStatus(mSessionId, mIdentifier);
                //        openSuccessfully = true;
                //        break;
                //    }
                //    System.Threading.Thread.Sleep(1000);
                //}
            }
            catch (Exception ex)
            {
                openSuccessfully = false;
                errorMessage = "Create Channel Failed:" + ex.ToString();
            }

            return openSuccessfully;
        }

        public SessionStatus InitSession(string sessionId, string identifier, bool isInited)
        {
            return mRelayService.InitSession(sessionId, identifier, isInited);
        }

        public BufferStatus SendBinary(long serialNo, byte[] buf)
        {
            byte[] copyBuf = new byte[buf.LongLength];
            buf.CopyTo(copyBuf, 0);

            var result = mRelayService.PutBuffer(mSessionId, mRemoteIdentifier, serialNo, copyBuf);

            if (result == BufferStatus.OK)
            {
                //没有错误，更新当前的传输状态
                mCurrWorkStatus.RecordTransferData(true, copyBuf.LongLength);
            }

            return result;
        }

        public BufferStatus CheckBinary(long serialNo, bool isSender)
        {
            if (isSender)
            {
                return mRelayService.CheckBuffer(mSessionId, mRemoteIdentifier, serialNo, isSender);
            }
            else
            {
                return mRelayService.CheckBuffer(mSessionId, mIdentifier, serialNo, isSender);
            }
        }

        public BufferStatus ReceiveBinary(long serialNo, out byte[] buf)
        {
            var result = mRelayService.GetBuffer(mSessionId, mIdentifier, serialNo, out buf);

            if (result == BufferStatus.OK)
            {
                mCurrWorkStatus.RecordTransferData(false, buf.LongLength);
            }

            return result;
        }

        public void SetTimeout(string sessionId, string identifier, int timeout, bool isSender)
        {
            mRelayService.SetTimeout(sessionId, identifier, timeout, isSender);
        }

        public bool KeepAlive(string sessionId, string identifier, bool isSender)
        {
            return mRelayService.KeepAlive(sessionId, identifier, isSender);
        }

        public string Close()
        {
             return string.Empty;
        }

        public string ClearBufferInSession(bool clearAll)
        {
            if (clearAll)
            {
                BufferStorage.ClearSessionManagement(mSessionId);
            }
            else
            {
                BufferStorage.ClearBuffer(mSessionId, mIdentifier);
            }
            return string.Empty;
        }

        public bool BufferSessinInUse(bool isSender)
        {
            return mRelayService.CheckSessionInUse(mSessionId, mRemoteIdentifier, isSender);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}
