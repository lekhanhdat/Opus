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
using AvePoint.RA.Common.Configurations;
using System;
using System.Collections.Concurrent;

namespace AvePoint.RA.Common.AzureService
{

    public enum QueueMessageType
    {
        Job,
        RealTime,
        CopMessage,
        O365Job,
        CustomerJob,
    }
    public class QueueMessageUtilFactory
    {
        static QueueMessageUtilFactory() {  }
        private static readonly object _locker = new object();
        private static ConcurrentDictionary<QueueMessageType, QueueMessageUtil> _utilDic = new ConcurrentDictionary<QueueMessageType, QueueMessageUtil>();
        public static QueueMessageUtil GetUtil(QueueMessageType type, string jobQueueName = "")
        {
            if (!_utilDic.ContainsKey(type))
            {
                lock (_locker)
                {
                    if (!_utilDic.ContainsKey(type))
                    {
                        var connectionString = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SERVICE_BUS_CONNECTION_STRING];
                        var queueName = string.Empty;
                        switch (type)
                        {
                            case QueueMessageType.Job:
                                queueName = jobQueueName;
                                break;
                            case QueueMessageType.O365Job:
                                queueName = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.HIGH_PRIORITY_JOB_QUEUE_NAME];
                                break;
                            case QueueMessageType.RealTime:
                                queueName = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.REALTIME_QUEUE_NAME];
                                break;
                            case QueueMessageType.CopMessage:
                                queueName = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.COP_QUEUE_NAME];
                                break;
                            case QueueMessageType.CustomerJob:
                                queueName = jobQueueName;
                                break;
                            default:
                                throw new ArgumentException($"message type is not supported. QueueMessageType : {type.ToString()}");
                        }
                        var util = new QueueMessageUtil(connectionString, queueName);
                        _utilDic.TryAdd(type, util);
                    }
                }
            }
            return _utilDic[type];
        }
    }
}
