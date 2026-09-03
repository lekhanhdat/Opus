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
using AvePoint.GCommon.Transfer.MQ.Interface;

namespace AvePoint.GCommon.Transfer.MQ.Channel
{
    internal class InProcessMessageChannel : IMessageChannel
    {
        #region Private Fields
        private string mSessionId = string.Empty;
        private string mIdentifier = string.Empty;
        private AveMQServer mServer = null;
        private IMQClientCallback mClientCallback = null;
        #endregion

        public InProcessMessageChannel(AveMQServer server, IMQClientCallback clientCallback)
        {
            mServer = server;
            mClientCallback = clientCallback;
        }

        #region IMessageChannel Members

        public bool Open(string sessionId, string identifier, out string errorMsg)
        {
            errorMsg = string.Empty;
            this.mSessionId = sessionId;
            this.mIdentifier = identifier;

            if (string.IsNullOrEmpty(mIdentifier))
            {
                throw new Exception("Please make sure the identifier is not empty.");
            }

            mServer.AddOrUpdateMQClientPeer(mSessionId, mIdentifier, mClientCallback);

            return true;
        }

        public void KeepAlive()
        {
        }

        public void SendMessage(AveMessage msg)
        {
            mServer.PutMessage(msg);
        }

        public bool ReceiveMessage(out AveMessage msg)
        {
            msg = null;
            return false;
        }

        public string Close()
        {
            return string.Empty;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
