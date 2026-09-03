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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Linq;

namespace AvePoint.RA.Service.RMTasks
{
    public class CheckTimeoutJobQueueMessageTaskExecutor : ITaskExecutor
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(CheckTimeoutJobQueueMessageTaskExecutor));

        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();


        /// <summary>
        /// Delete queue messages with an UpdateTime older than 7 days
        /// Re-enter queue messages with an UpdateTime older than 2 days
        /// </summary>
        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                int batchLimit = 100;
                string anchorMessageId = null;

                do
                {
                    var deletePeriod = DateTime.UtcNow.AddDays(-7).Ticks;
                    var timeoutPeriod = DateTime.UtcNow.AddDays(-2).Ticks;
                    var timeoutMessageList = JobQueueService.GetTimeoutProcessingMessages(timeoutPeriod, anchorMessageId, batchLimit);
                    mLogger.Info($"Got timeout queue messages {timeoutMessageList.Count}, period: {timeoutPeriod}, anchorId: {anchorMessageId}.");

                    var reEnterMessageIdList = timeoutMessageList.Where(m => m.UpdateTime >= deletePeriod).Select(m => m.MessageId).ToList();
                    try
                    {
                        var count = await JobQueueService.ReEnterQueueMessageBatchAsync(reEnterMessageIdList);
                        mLogger.Info($"Success re-enter queue messages {count}");
                    }
                    catch (Exception ex)
                    {
                        mLogger.Info($"Failed re-enter queue messages {string.Join(",", reEnterMessageIdList)}");
                    }

                    var deleteMessageList = timeoutMessageList.Where(m => m.UpdateTime < deletePeriod).ToList();
                    try
                    {
                        if(deleteMessageList.Count > 0)
                        {
                            var count = await JobQueueService.DeleteQueueMessageBatchAsync(deleteMessageList.Select(m => m.MessageId).ToList());
                            mLogger.Info($"Success delete timeout messages {count}");
                            foreach(var message in deleteMessageList)
                            {
                                mLogger.Info($"Deleted timeout message details: {JobQueueMessageToString(message)}");
                            }
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        mLogger.Info($"Failed to delete timeout queue messages: {string.Join(",", reEnterMessageIdList)}. Error: {ex}");
                    }

                    anchorMessageId = timeoutMessageList.Count >= batchLimit ? timeoutMessageList.LastOrDefault()?.MessageId : null;
                    
                } while(!string.IsNullOrEmpty(anchorMessageId));
                
            }
            catch (Exception e)
            {
                mLogger.Error($"An error occurred when check timeout job queue message.ERROR: {e}");
            }

        }

        private string JobQueueMessageToString(JobQueueDto queueMsg)
        {
            return SerializerHelper.SerializeByJsonConvert(queueMsg);
        }
    }
}
