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
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMJobQueueDao : IBaseDao<RMJobQueue>
    {
        void DeleteQueueMessage(string Id, string tenantId);
        Task<int> DeleteQueueMessageBatchAsync(List<string> idList);
        List<RMJobQueue> GetDBJobQueueMessage(string tenantId, string useEmail, JobType jobType);
        List<RMJobQueue> GetQueueMessage(string productionId);
        string AddToJobQueue(RMJobQueue jobInfo);
        List<RMJobQueue> GetQueues(int pageIndex, int pageSize, out int totalRecord, string orderKey, bool isAsc, Expression<Func<RMJobQueue, bool>> whereLambda = null);

        RMJobQueue GetQueue(string id, string tenantid);

        int GetMessagesCount(string tenantId, JobType jobType);

        List<RMJobQueue> GetMessages(string tenantId, params JobType[] jobTypes);

        int GetTenantJobQueueCount(string tenantId);

        List<RMJobQueue> GetRCCDBJobQueueByLoginName(string loginName, List<string> scopeIds);
        List<RMJobQueue> GetDisposalHistoryDBJobQueueByLoginName(string loginName, string scopeId);
        List<RMJobQueue> GetAllDBJobQueueByLoginName(string loginName, int jobType);

        Dictionary<string, List<RMJobQueue>> GetDBJobMessageGroupByTenant(int top);
        /// <summary>
        /// Reset the status value form 1 to 0 so that it can be scheduled from queue later.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="tenantId"></param>
        void ReEnterQueueMessage(string Id, string tenantId);
        Task<int> ReEnterQueueMessageBatchAsync(List<string> idList);

        bool UpdateJobPriority(string messageId, JobPriority newPriority, string tenantId);
        List<RMJobQueue> GetTimeoutProcessingMessages(long timeoutPeriod, string anchorMessageId, int top);
    }
}
