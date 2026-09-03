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



using System.ServiceModel;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Factory;
using AvePoint.GCommon.Transfer.MQ.Interface;

namespace AvePoint.GCommon.Transfer.MQ.Channel
{
    internal class MessageChannelFactory
    {
        public static IMessageChannel GetMessageChannel(string host, int port, string relatedBaseUrl, string jobId, IMQClientCallback callback)
        {
            if (AveMQConfigure.MQChannelMode == AveChannelMode.WCF)
            {
                if (object.Equals(callback, null))
                {
                    return new WCFMessageChannel<IMQWCFServiceOneWay>(new WCFChannelFactory<IMQWCFServiceOneWay>(host, port, relatedBaseUrl, WCFServiceHostType.MQOneWay.ToString(), jobId));
                }
                else
                {
                    return new WCFMessageChannel<IMQWCFService>(new WCFDuplexChannelFactory<IMQWCFService>(new InstanceContext(callback), host, port, relatedBaseUrl, WCFServiceHostType.MQ.ToString(), jobId));
                }
            }

            return null;
        }

        public static IMessageChannel GetMessageChannel(TransferCommunicationSettings communicationSettings, IMQClientCallback callback)
        {
            if (AveMQConfigure.MQChannelMode == AveChannelMode.WCF)
            {
                if (object.Equals(callback, null))
                {
                    return new WCFMessageChannel<IMQWCFServiceOneWay>(new WCFChannelFactory<IMQWCFServiceOneWay>(communicationSettings, WCFServiceHostType.MQOneWay.ToString()));
                }
                else
                {
                    return new WCFMessageChannel<IMQWCFService>(new WCFDuplexChannelFactory<IMQWCFService>(new InstanceContext(callback), communicationSettings, WCFServiceHostType.MQ.ToString()));
                }
            }

            return null;
        }

        public static IMessageChannel GetInProcessMessageChannel(AveMQServer server, IMQClientCallback clientCallback)
        {
            return new InProcessMessageChannel(server, clientCallback);
        }
    }
}
