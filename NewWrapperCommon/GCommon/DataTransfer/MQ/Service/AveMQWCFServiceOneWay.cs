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
using AvePoint.GCommon.Transfer.MQ.Interface;
using System.ServiceModel;

namespace AvePoint.GCommon.Transfer.MQ.Service
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
    internal class AveMQWCFServiceOneWay : IMQWCFServiceOneWay
    {
        private static AveMQServer mServer = AveMQServer.GetInstance();

        #region IMQWCFServiceBase Members

        public bool RegisterMQClient(string sessionId, string identifier)
        {
            mServer.AddOrUpdateMQClientPeer(sessionId, identifier, null);

            return true;
        }

        public bool UnRegisterMQClient(string sessionId, string identifier)
        {
            mServer.RemoveMQClientPeer(sessionId, identifier);

            return true;
        }

        public bool KeepAlive(string sessionId, string identifier)
        {
            return mServer.IsClientPeerAvailable(sessionId, identifier);
        }

        public bool SendMessage(AveMessage message)
        {
            mServer.PutMessage(message);

            return true;
        }

        public bool ReceiverMessage(string sessionId, string identifier, out AveMessage message)
        {
            message = mServer.GetMessage(sessionId, identifier);

            if (message != null)
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}
