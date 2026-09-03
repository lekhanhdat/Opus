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
using System.ServiceModel.Channels;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Factory;
using AvePoint.GCommon.Transfer.Common;

namespace AvePoint.GCommon.Transfer.Data.Channel
{
    /// <summary>
    /// 实现WCF数据传输方式
    /// </summary>
    public class WCFDataTransferChannel : BaseWcfTransferChannel<IRelay>
    {
        public WCFDataTransferChannel(WcfChannelFactory<IRelay> channelFactory) : base(channelFactory)
        {
        }

        #region ITransferChannel Members

        public override bool Open(string sessionId, string identifier, string remoteIdentifier, DataTransferSetting settings, out string errorMessage)//Open(string sessionId,out string errorMessage, params object[] parameters)
        {
            bool openSuccessfully = false;
            errorMessage = string.Empty;
            this.SessionId = sessionId;
            this.Identifier = identifier;
            this.RemoteIdentifier = remoteIdentifier;
            try
            {
                base.Open(sessionId, identifier, remoteIdentifier, settings, out errorMessage);//, parameters);
                ((ICommunicationObject)Channel).Open();
                Channel.CheckStatus(sessionId, identifier);
                openSuccessfully = true;
            }
            catch (Exception ex)
            {
                openSuccessfully = false;
                errorMessage = "Create Channel Failed:" + ex.ToString();
            }

            return openSuccessfully;
        }

        public override SessionStatus InitSession(string sessionId, string identifier, bool isInited, int timeout)
        {
            SessionStatus status = Channel.InitSession(sessionId, identifier, isInited);
            if (status == SessionStatus.IsReady || status == SessionStatus.InitedOK)
            {
                Channel.SetTimeout(sessionId, identifier, timeout, identifier == DataTransferConstants.SenderIdentifier);
                //return InitSessionStream(sessionId, identifier, isInited);
            }
            return status;
        }

        public override BufferStatus SendBinary(long serialNo, byte[] buf)
        {
            var result = Channel.PutBuffer(SessionId, RemoteIdentifier, serialNo, buf);

            if (result == BufferStatus.OK)
            {
                //没有错误，更新当前的传输状态
                CurrentWorkStatus.RecordTransferData(true, buf.LongLength);
            }

            return result;
        }

        public override BufferStatus CheckBinary(long serialNo, bool isSender)
        {
            if (isSender)
            {
                return Channel.CheckBuffer(SessionId, RemoteIdentifier, serialNo, isSender);
            }
            else
            {
                return Channel.CheckBuffer(SessionId, Identifier, serialNo, isSender);
            }
        }

        public override BufferStatus ReceiveBinary(long serialNo, out byte[] buf)
        {
            var result = Channel.GetBuffer(SessionId, Identifier, serialNo, out buf);

            if (result == BufferStatus.OK)
            {
                CurrentWorkStatus.RecordTransferData(false, buf.LongLength);
            }

            return result;
        }

        public override void SetTimeout(string sessionId, string identifier, int timeout, bool isSender)
        {
            Channel.SetTimeout(sessionId, identifier, timeout, isSender);
        }

        public override bool KeepAlive(string sessionId, string identifier, bool isSender)
        {
            return Channel.KeepAlive(sessionId, identifier, isSender);
        }

        public override string ClearBufferInSession(bool clearAll)
        {
            if (clearAll)
            {
                Channel.ClearSessionManagement(SessionId);
            }
            else
            {
                Channel.ClearSession(SessionId, Identifier);
            }
            return string.Empty;
        }

        public override bool BufferSessionInUse(bool isSender)
        {
            return Channel.CheckSessionInUse(SessionId, RemoteIdentifier, isSender);
        }
        #endregion
    }
}
