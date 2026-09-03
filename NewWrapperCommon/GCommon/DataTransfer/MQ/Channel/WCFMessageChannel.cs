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
using AvePoint.GCommon.Transfer.MQ.Interface;
using System.ServiceModel;

namespace AvePoint.GCommon.Transfer.MQ.Channel
{
    internal class WcfMessageChannel<T> : IMessageChannel
    {
        #region Private Fields
        private WcfChannelFactory<T> channelFactory;
        private IMQWCFServiceBase mqService;
        private string sessionId = string.Empty;
        private string identifier = string.Empty;
        #endregion

        public WcfMessageChannel(WcfChannelFactory<T> channelFactory)
        {
            this.channelFactory = channelFactory;
        }

        #region IMessageChannel Members

        public bool Open(string sessionId, string identifier, out string errorMsg)
        {
            bool openSuccessfully = false;
            errorMsg = string.Empty;

            lock (channelFactory)
            {
                try
                {
                    channelFactory.Dispose();
                    ObjectUtility.DisposeAndCloseChannel(mqService);
                    mqService = null;

                    this.sessionId = sessionId;
                    this.identifier = identifier;
                    if (string.IsNullOrEmpty(identifier))
                    {
                        throw new Exception("Please make sure the identifier is not empty.");
                    }
                    mqService = (IMQWCFServiceBase)channelFactory.CreateChannel();
                    ((ICommunicationObject)mqService).Open();
                    mqService.RegisterMQClient(sessionId, identifier);
                    openSuccessfully = true;
                }
                catch (Exception ex)
                {
                    errorMsg = ex.ToString();
                    openSuccessfully = false;
                    throw;
                }
            }

            return openSuccessfully;
        }

        public void KeepAlive()
        {
            lock (channelFactory)
            {
                mqService.KeepAlive(sessionId, identifier);
            }
        }

        public void SendMessage(AveMessage msg)
        {
            lock (channelFactory)
            {
                if (mqService != null)
                {
                    mqService.SendMessage(msg);
                }
                else
                {
                    throw new ArgumentNullException("MQService");
                }
            }
        }

        public bool ReceiveMessage(out AveMessage msg)
        {
            msg = null;
            bool receivedSuccessfully = false;

            lock (channelFactory)
            {
                receivedSuccessfully = mqService.ReceiverMessage(sessionId, identifier, out msg);
            }

            return receivedSuccessfully;
        }

        public string Close()
        {
            try
            {
                lock (channelFactory)
                {
                    mqService.UnRegisterMQClient(sessionId, identifier);
                }
            }
            finally
            {
                Dispose();
            }

            return string.Empty;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            channelFactory.Dispose();
            channelFactory = null;

            ObjectUtility.DisposeAndCloseChannel(mqService);
            mqService = null;
        }

        #endregion
    }
}
