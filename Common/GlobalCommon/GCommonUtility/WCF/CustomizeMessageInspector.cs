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





namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    #endregion

    /// <summary>
    /// Customize Message Inspector
    /// </summary>
    public class CustomizeMessageInspector : IDispatchMessageInspector
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        String contractName;

        public CustomizeMessageInspector(string contractName)
        {
            this.contractName = contractName;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            request = TraceMessage(request.CreateBufferedCopy(int.MaxValue), true);
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            reply = TraceMessage(reply.CreateBufferedCopy(int.MaxValue), false);
        }

        private Message TraceMessage(MessageBuffer buffer, bool preRealCall)
        {
            Message msg = buffer.CreateMessage();
            if (preRealCall)
            {
                string logMsg = string.Format("Contract Name:{0}\tRequest Time:{1}\tRequest Message:\n{2}", contractName, DateTime.Now.ToString(), msg.ToString());
                logger.Debug(logMsg);
            }
            else
            {
                string logMsg = string.Format("Contract Name:{0}\tReply Time:{1}\tReply Message:\n{2}", contractName, DateTime.Now.ToString(), msg.ToString());
                logger.Debug(logMsg);
            }
            return buffer.CreateMessage();
        }
    }
}
