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
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace AvePoint.RA.Common.AzureService
{
    public class ServiceBusUtil
    {
        //public static QueueClient GetQueueClient(string connStringOrEndpoint)
        //{
        //    return new QueueClient(new ServiceBusConnectionStringBuilder(connStringOrEndpoint), receiveMode: ReceiveMode.ReceiveAndDelete, retryPolicy: RetryPolicy.Default);
        //}

        public static QueueClient GetQueueClient(string serviceBusConnStringOrEndpoint, string queueName)
        {
            return new QueueClient(serviceBusConnStringOrEndpoint, queueName, receiveMode: ReceiveMode.ReceiveAndDelete, retryPolicy: RetryPolicy.Default);
        }

        //public static void SendMessage(string connStringOrEndpoint, string message)
        //{
        //    var client = GetQueueClient(connStringOrEndpoint);
        //    client.SendAsync(new Message(Encoding.UTF8.GetBytes(message))).Wait();
        //}

        public static void SendMessage(string serviceBusConnStringOrEndpoint, string queueName, string message)
        {
            var client = GetQueueClient(serviceBusConnStringOrEndpoint, queueName);
            client.SendAsync(new Message(Encoding.UTF8.GetBytes(message))).Wait();

        }

        //public static void SendMessage(string connStringOrEndpoint, object message)
        //{
        //    SendMessage(connStringOrEndpoint, JsonSerializer.Serialize(message));
        //}

        public static void SendMessage(string serviceBusConnStringOrEndpoint, string queueName, object message)
        {
            SendMessage(serviceBusConnStringOrEndpoint, queueName, Newtonsoft.Json.JsonConvert.SerializeObject(message));
        }
    }
}
