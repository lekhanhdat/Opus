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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace AvePoint.RA.Service.RMTasks
{
    public class ObserveAOSNotificationTaskExecutor : ITaskExecutor
    {
        private static RALogger logger = RALogger.GetInstance(typeof(ObserveAOSNotificationTaskExecutor));

        private IRMAOSNotificationService AOSNotificationService => PlatformWindsorManager.GetService<IRMAOSNotificationService>();
        private ICommonService CommonService => PlatformWindsorManager.GetService<ICommonService>();

        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                logger.Info("Start to observe AOS notification ServiceBus Topic.");
                var hostName = Dns.GetHostName();
                if (CommonService.IsPrimaryTimer(hostName))
                {
                    AosNotificationSubscriptionClient.Process(CacheQueueMessage);
                }
                else
                {
                    logger.Info($"Only primary timer need run this task. {hostName}");
                }
                logger.Info("Finish to observe AOS notification ServiceBus Topic.");
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while observe AOS notification. ERROR:{0}", e.ToString());
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private void CacheQueueMessage(RMAosQueueMessage message)
        {
            if (message.MessageType == RMAosQueueMessageType.SyncNodes)
            {
                var content = message.SyncNodesMessage?.Content;
                if (content == null ||
                    (content.IsNewMessage && string.IsNullOrEmpty(content.StorageSasUri)) ||
                    (!content.IsNewMessage && string.IsNullOrEmpty(content.StorageXri)) ||
                    string.IsNullOrEmpty(content.FileLowName))
                {
                    logger.Warn($"StorageXri Or FileLowName is null. Sync nodes message: {JsonUtil.JsonSerializer(message)}");
                    return;
                }
            }
            else if (message.MessageType == RMAosQueueMessageType.DeleteNodes)
            {
                var content = message.DeleteNodesMessage?.Content;
                if (content == null ||
                    (content.IsNewMessage && string.IsNullOrEmpty(content.StorageSasUri)) ||
                    (!content.IsNewMessage && string.IsNullOrEmpty(content.StorageXri)) ||
                    string.IsNullOrEmpty(content.FileLowName))
                {
                    logger.Warn($"StorageXri Or FileLowName is null. Delete nodes message: {JsonUtil.JsonSerializer(message)}");
                    return;
                }
            }

            AOSNotificationService.Refresh(message);
        }

    }
}
