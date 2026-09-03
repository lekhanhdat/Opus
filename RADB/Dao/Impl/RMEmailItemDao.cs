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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMEmailItemDao : BaseDao<RMEmailItem>, IRMEmailItemDao
    {
        public async Task AddWorkflowManualItemAsync(RMEmailItem item)
        {
            using (var context = GetNewContext())
            {
                var exist = await context.EmailItem.AnyAsync(d => d.Id == item.Id);
                if (!exist)
                {
                    context.EmailItem.Add(item);
                    await context.SaveChangesAsync();
                }
                else
                {
                    await UpdateAsync(item);
                }
            }
        }

        public Dictionary<string, List<Guid>> GetAllWaittingUserAndInstanceId()
        {
            using (var context = GetNewContext())
            {
                Dictionary<string, List<Guid>> userIdsAndInstanceIds = new Dictionary<string, List<Guid>>();

                Dictionary<string, List<Guid>> stepIdAndInstanceIds = new Dictionary<string, List<Guid>>();

                List<Guid> instanceIds = context.EmailItem.Where(i => i.Status != RMSendEmailStatus.Completed && i.Flag == 0).Select(i => i.Id).ToList();
                var stepIdAndInstanceArray = context.WorkflowInstance.Where(t => instanceIds.Contains(t.Id)).Select(o => new { o.CurStepId, o.Id }).GroupBy(t => t.CurStepId).ToList();
                foreach (var item in stepIdAndInstanceArray)
                {
                    List<Guid> ids = new List<Guid>();
                    var itemValueList = item.ToList();
                    foreach (var temp in itemValueList)
                    {
                        ids.Add(temp.Id);
                    }
                    stepIdAndInstanceIds.Add(item.Key, ids);
                }

                foreach (var item in stepIdAndInstanceIds)
                {
                    List<string> ownerIds = context.WorkflowStepConfiguration.Where(t => t.StepId.ToString().Equals(item.Key)).Select(t => t.OwnerId).Distinct().ToList();
                    foreach (string ownerId in ownerIds)
                    {
                        if (userIdsAndInstanceIds.ContainsKey(ownerId))
                        {
                            userIdsAndInstanceIds[ownerId].AddRange(item.Value);
                        }
                        else
                        {
                            userIdsAndInstanceIds.Add(ownerId, item.Value);
                        }
                    }
                }
                return userIdsAndInstanceIds;
            }
        }
        
        public async Task<int> Empty()
        {
            using var context = GetNewContext();
            var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].[RMEmailItems]";
            return await context.Database.ExecuteSqlCommandAsync(sql);
        }

        public List<Guid> GetRecordsIdForNewWorkflow()
        {
            using (var context = GetNewContext())
            {
                List<Guid> instanceIds = context.EmailItem.Where(i => i.Status != RMSendEmailStatus.Completed && i.Flag == 0).Select(i => i.Id).ToList();
                return instanceIds;
            }
        }

        public void UpdateWorkflowManualItem(List<Guid> ids)
        {
            using (var context = GetNewContext())
            {
                var instances = context.EmailItem.Where(i => ids.Contains(i.Id)).ToList();
                foreach (var instance in instances)
                {
                    instance.Status = RMSendEmailStatus.Completed;
                    instance.ModifyTime = DateTime.UtcNow;
                }
                this.BatchUpdate(context, instances);
            }
        }


    }
}
