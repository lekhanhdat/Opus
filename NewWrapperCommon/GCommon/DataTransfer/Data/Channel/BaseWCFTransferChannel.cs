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
using AvePoint.GCommon.Transfer.Factory;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;
using System.IO;

namespace AvePoint.GCommon.Transfer.Data.Channel
{
    /// <summary>
    /// 使用WCF实现的Channel的基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseWcfTransferChannel<T> : ITransferChannel
    {
        private WcfChannelFactory<T> channelFactory;//WCF客户端创建工厂
        private T channel;//WCF服务端接口定义
        private string sessionId = string.Empty;//当前正在通讯的数据的sesstion id。
        private string identifier = string.Empty;
        private string remoteIdentifier = string.Empty;
        private DataTransferResultStatus currentWorkStatus = new DataTransferResultStatus();
        protected DataTransferSetting transferSetting;

        public WcfChannelFactory<T> ChannelFactory
        {
            get { return channelFactory; }
            set { channelFactory = value; }
        }
        public T Channel
        {
            get { return channel; }
            set { channel = value; }
        }
        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
        public string Identifier
        {
            get { return identifier; }
            set { identifier = value; }
        }
        public string RemoteIdentifier
        {
            get { return remoteIdentifier; }
            set { remoteIdentifier = value; }
        }
        public DataTransferResultStatus CurrentWorkStatus
        {
            get { return currentWorkStatus; }
            set { currentWorkStatus = value; }
        }

        public BaseWcfTransferChannel(WcfChannelFactory<T> channelFactory)
        {
            this.channelFactory = channelFactory;
        }

        #region ITransferChannel Members

        public virtual bool Open(string sessionId, string identifier, string remoteIdentifier, DataTransferSetting settings, out string errorMessage)//Open(string sessionId, out string errorMessage, params object[] parameters)
        {
            errorMessage = string.Empty;

            this.sessionId = sessionId;
            this.identifier = identifier;
            this.remoteIdentifier = remoteIdentifier;
            this.transferSetting = settings;
            this.channel = channelFactory.CreateChannel();

            return true;
        }

        public abstract BufferStatus SendBinary(long serialNo, byte[] buf);
        public abstract BufferStatus CheckBinary(long serialNo, bool isSender);
        public abstract BufferStatus ReceiveBinary(long serialNo, out byte[] buf);
        public abstract SessionStatus InitSession(string sessionId, string identifier, bool isInited, int timeout);
        public abstract void SetTimeout(string sessionId, string identifier, int timeout, bool isSender);
        public abstract bool KeepAlive(string sessionId, string identifier, bool isSender);
        public virtual double KeepAliveTimeout
        {
            get 
            {
                if (this.transferSetting != null)
                {
                    return this.transferSetting.ReconnectTimeout / 2.0;
                }
                return int.MaxValue;
            }
        }

        /// <summary>
        /// only release channel without releasing the channelfactory
        /// </summary>
        /// <returns></returns>
        public virtual string Close()
        {
            try
            {
                //channelFactory.Dispose();
                //channelFactory = null;

                ObjectUtility.DisposeAndCloseChannel(channel);
                //mChannel = null;
                return string.Empty;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public abstract string ClearBufferInSession(bool clearAll);
        public abstract bool BufferSessionInUse(bool isSender);

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            Close();

            try
            {
                channelFactory.Dispose();
                channelFactory = null;
            }
            catch (Exception ex)
            {
                DataTransferLogger.Logger(AveLogLevel.ERROR, "release channel factory failed:{0}", ex);
            }
        }

        #endregion
    }
}
