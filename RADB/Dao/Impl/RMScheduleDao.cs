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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Model;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMScheduleDao : BaseDao<RMSchedule>, IRMScheduleDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMScheduleDao));
        public async Task<string> CreateScheduleAsync(RMSchedule rmSchedule)
        {
            using var context = GetNewContext();
            //if ((rmSchedule.StartTime == null || rmSchedule.StartTime == 0) || rmSchedule.TimeZoneId == string.Empty || rmSchedule.EndType == null
            //|| rmSchedule.JobCategory == null)
            //{
            //}
            var schedule = context.Schedule.AsQueryable().Where(sc => sc.JobCategory == rmSchedule.JobCategory && (string.IsNullOrEmpty(sc.ProfileId) || sc.ProfileId.Equals(rmSchedule.ProfileId, StringComparison.OrdinalIgnoreCase)) && !sc.IsRemoved).FirstOrDefault();
            if (schedule != null)
            {
                rmSchedule.Id = schedule.Id;
                await this.UpdateAsync(rmSchedule);
            }
            else
            {
                context.Schedule.Add(rmSchedule);
                context.SaveChanges();
            }

            return rmSchedule.Id;
        }

        public async Task UpdateScheduleAsync(RMSchedule rmSchedule)
        {
            RMSchedule oldRecord = GetSchedule(rmSchedule.Id);
            rmSchedule.DAOMigrated = oldRecord.DAOMigrated;
            await this.UpdateAsync(rmSchedule);
        }

        public int BatchUpdateScheduleAsync(List<RMSchedule> rmSchedules)
        {
            using var context = GetNewContext();
            return this.BatchUpdate(context, rmSchedules);
        }

        public void DeleteSchedule(string rmScheduleId)
        {
            using var context = GetNewContext();
            var schedules = context.Schedule.AsQueryable().Where(sc => sc.Id.Equals(rmScheduleId) && !sc.IsRemoved).ToList();
            if (schedules.Count > 0)
            {
                this.Delete(schedules[0]);
            }
            else
            {
                logger.Warn("Can not find schedule by id:{0}", rmScheduleId);
            }
        }

        public RMSchedule GetSchedule(string rmScheduleId)
        {
            using var context = GetNewContext();
            var schedule = context.Schedule.AsQueryable().Where(sc => sc.Id.Equals(rmScheduleId) && !sc.IsRemoved).FirstOrDefault();
            return schedule;
        }

        private List<RMSchedule> CompatibleOldDataConvert(List<RMSchedule> scheduleInfo)
        {
            foreach (var sdu in scheduleInfo)
            {
                if(sdu.ProfileId == null)
                {
                    continue;
                }
                var pathIds = sdu.ProfileId.Split('|');
                Guid groupId0 = Guid.Empty;
                Guid siteId1 = Guid.Empty;
                Guid scopeId2 = Guid.Empty;
                int length = pathIds.Length;
                if (length == 3 && sdu.JobCategory != (int)ScheduleType.PRDisposalSchedule)
                {
                    try
                    {
                        var node = JsonConvert.DeserializeObject<RMSPTreeNode>(sdu.Extentions);
                        Guid.TryParse(pathIds[0], out groupId0);
                        Guid.TryParse(pathIds[1], out siteId1);
                        Guid.TryParse(pathIds.Last(), out scopeId2);
                        if (groupId0 == scopeId2 && Guid.Empty == siteId1)
                        {
                            //group
                            sdu.ProfileId = groupId0.ToString();
                        }
                        if (siteId1 == scopeId2)
                        {
                            //site collection
                            sdu.ProfileId = groupId0.ToString() + "|" + siteId1.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"An error when CompatibleOldDataConvert, profileId:{sdu.ProfileId} message:{ex.Message}");
                    }
                }
            }
            return scheduleInfo;
        }

        public RMSchedule GetSchedule(string profileId, ScheduleType type)
        {
            using var context = GetNewContext();
            RMSchedule result = new RMSchedule();
            var schedules = CompatibleOldDataConvert(context.Schedule.AsQueryable().Where(sc => sc.JobCategory == (int)type).ToList());
            return schedules.FirstOrDefault(sc => sc.ProfileId != null && sc.ProfileId.Equals(profileId));
        }

        public RMSchedule GetAncestrySchedule(string profileId, ScheduleType type)
        {
            using var context = GetNewContext();
            RMSchedule result = new RMSchedule();
            var allSchedules = context.Schedule.AsQueryable().Where(sc => sc.JobCategory == (int)type && profileId.Contains(sc.ProfileId)).ToDictionary(s => s.ProfileId);
            if (allSchedules == null || allSchedules.Count == 0)
            {
                return null;
            }
            var maxLengthKey = allSchedules.Keys.FirstOrDefault();
            foreach (var k in allSchedules.Keys)
            {
                if (k.Length > maxLengthKey?.Length)
                {
                    maxLengthKey = k;
                }
            }
            result = allSchedules[maxLengthKey];
            return result;
        }

        public RMSchedule GetPhysicalScheduleByLocationId(Guid locationId)
        {
            using var context = GetNewContext();
            var schedule = context.Schedule.AsQueryable().Where(sc => sc.ProfileId.EndsWith(locationId.ToString()) 
                                                                    && sc.JobCategory == (int)ScheduleType.PRDisposalSchedule 
                                                                    && !sc.IsRemoved).FirstOrDefault();
            return schedule;
        }

        public List<RMSchedule> GetScheduleByType(ScheduleType type)
        {
            using var context = GetNewContext();
            var schedules = context.Schedule.AsQueryable().Where(sc => sc.JobCategory == (int)type && !sc.IsRemoved).ToList();
            return schedules;
        }

        public List<RMSchedule> GetScheduleByTypeAndGroupId(string groupId, ScheduleType type)
        {
            using var context = GetNewContext();
            var schedules = context.Schedule.AsQueryable().Where(sc => sc.JobCategory == (int)type && sc.ProfileId.StartsWith(groupId) && !sc.IsRemoved).ToList();
            return schedules;
        }

        public List<string> GetDisposalBreakNodes(string parentId)
        {
            using var context = GetNewContext();
            var breakNodes = context.Schedule.AsQueryable()
                .Where(
                sc => 
                (sc.JobCategory == (int)ScheduleType.DisposalSchedule 
                || sc.JobCategory == (int)ScheduleType.EXODisposalSchedule 
                || sc.JobCategory == (int)ScheduleType.FSDisposalSchedule
                || sc.JobCategory == (int)ScheduleType.SPOnPremDisposalSchedule
                || sc.JobCategory == (int)ScheduleType.OneDriveDisposalSchedule
                || sc.JobCategory == (int)ScheduleType.BoxDisposalSchedule
                || sc.JobCategory == (int)ScheduleType.GoogleDisposalSchedule
                || sc.JobCategory == (int)ScheduleType.TeamsDisposalSchedule
                ) && sc.ProfileId.Contains(parentId) && !sc.IsRemoved)
                .Select(s => s.Extentions).ToList();
            return breakNodes;
        }

        public List<string> GetScheduleBreakNodes(string parentId)
        {
            using var context = GetNewContext();
            var breakNodes = context.Schedule.AsQueryable()
                .Where(
                sc =>
                (sc.JobCategory == (int)ScheduleType.OneDriveArchiveJobSchedule
                || sc.JobCategory == (int)ScheduleType.SPArchiveJobSchedule
                || sc.JobCategory == (int)ScheduleType.TeamsArchiveJobSchedule
                ) && sc.ProfileId.Contains(parentId) && !sc.IsRemoved)
                .Select(s => s.ProfileId).ToList();
            return breakNodes;
        }
        
        public async Task UpdateExtentionsByTypeAsync(ScheduleType type, string extension)
        {
            using var context = GetNewContext();
            var schedule = context.Schedule.AsQueryable().Where(sc => sc.JobCategory == (int)type && !sc.IsRemoved).ToList();
            if (schedule != null && schedule.Count > 0)
            {
                schedule[0].Extentions = extension;
                await this.UpdateAsync(schedule[0]);
            }
        }

        public List<RMSchedule> GetScheduleByProfileId(string profileId)
        {
            using var context = GetNewContext();
            var schedules = context.Schedule.AsQueryable().Where(sc => sc.ProfileId.Equals(profileId) && !sc.IsRemoved).ToList();
            if (schedules != null && schedules.Count > 0)
            {
                logger.Info("The sharepoint  node schedule setting is not null,nodeid is {0}", profileId);
            }
            return schedules;
        }

        public List<RMSchedule> GetRunableSchedule()
        {
            try
            {
                using (var context = GetNewContext())
                {
                    long currentTime = DateTime.UtcNow.Ticks;
                    logger.Debug("Current utc time {0}", new DateTime(currentTime));
                    var schedules = context.Schedule.AsQueryable().Where(sc => !sc.NoSchedule && sc.NextTime < currentTime && !sc.IsRemoved).ToList();
                    if (schedules != null && schedules.Count > 0)
                    {
                        logger.Info("Current utc time {0}", new DateTime(schedules[0].NextTime));
                    }
                    return schedules;
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error has occurred when GetRunableSchedule, message:{e.Message}");
            }
            return new List<RMSchedule>();

        }

        public List<RMSchedule> GetRunableSchedule(ScheduleType type)
        {
            using var context = GetNewContext();
            long currentTime = DateTime.UtcNow.Ticks;
            var schedules = context.Schedule.AsQueryable().Where(sc => sc.NextTime < currentTime && sc.JobCategory == (int)type && !sc.IsRemoved).ToList();
            return schedules;
        }

        public List<RMSchedule> GetRunableScheduleByTypes(List<ScheduleType> type)
        {
            using var context = GetNewContext();
            var typeIds = type.Select(t => (int)t).ToList();
            long currentTime = DateTime.UtcNow.Ticks;
            var schedules = context.Schedule.AsQueryable().Where(sc => sc.NextTime < currentTime && typeIds.Contains(sc.JobCategory) && !sc.IsRemoved).ToList();
            return schedules;
        }

        public bool CheckIsContainScheduleForOwnAndChildNodes(string nodeId, string groupId)
        {
            var isContainScheduleChildNode = false;
            using var context = GetNewContext();
            var schedules = context.Schedule.AsQueryable().Where(sc => sc.ProfileId.Contains(groupId) && sc.ProfileId.Contains(nodeId) && !sc.IsRemoved).ToList();
            isContainScheduleChildNode = schedules != null && schedules.Count > 0 ? true : false;
            return isContainScheduleChildNode;
        }

        public async Task MarkScheduleRemovedAsync(string rmScheduleId)
        {
            using var context = GetNewContext();
            var schedule = context.Schedule.AsQueryable().Where(sc => sc.Id.Equals(rmScheduleId) && !sc.IsRemoved).FirstOrDefault();
            if (schedule != null)
            {
                schedule.IsRemoved = true;
                await this.UpdateAsync(schedule);
            }
        }
        public void RemoveNodeInfo(ScheduleType type)
        {
            using (var ctx = GetNewContext())
            {
                var entities = ctx.Schedule.Where(s => s.JobCategory == (int)type).ToList();
                foreach (var entity in entities)
                {
                    entity.ProfileId = "";
                    entity.Extentions = "";
                }
                BatchUpdate(entities);
            }
        }
        public void DeleteSchedules(ScheduleType type, string profileId)
        {
            using var context = GetNewContext();
            var entities = context.Schedule.Where(s => s.JobCategory == (int)type && s.ProfileId.StartsWith(profileId)).ToList();
            if (entities != null && entities.Count > 0)
            {
                this.BatchDelete(entities);
            }
        }
    }
}
