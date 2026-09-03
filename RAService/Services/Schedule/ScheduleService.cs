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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Schedule;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.SourceTreeQuery.Model;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Multi_Geo;
using AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC.AuditHandler;
using AvePoint.RA.Service.Services.Schedule.AuditHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Schedule
{
    /// <summary>
    /// have reviewed by allen yin
    /// </summary>
    [Audit]
    public class ScheduleService : RMServiceBase, IScheduleService
    {
        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRMProductVersionInfo ProductVersionInfo => PlatformWindsorManager.GetService<IRMProductVersionInfo>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private RALogger logger = RALogger.GetInstance(typeof(ScheduleService));

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermSynchronization, Action = AuditAction.ConfigureScheduleForTermSynchronization, BeforeHandler = typeof(RMScheduleBeforeAuditHandler), AfterHandler = typeof(RMScheduleAfterAuditHandler))]
        public Task<string> CreateScheduleServiceAsync(ScheduleInfo scheduleInfo, bool checkStartTime = true, string nodeFullPath = "")
        {
            return InnerCreateScheduleServiceAsync(scheduleInfo, checkStartTime, nodeFullPath);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermSynchronization, Action = AuditAction.ConfigureScheduleForTermSynchronization, BeforeHandler = typeof(RMScheduleBeforeAuditHandler), AfterHandler = typeof(RMScheduleAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.ConfigureDisposalJobSchedule4FS, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public Task<string> CreateScheduleServiceForFSAsync(ScheduleInfo scheduleInfo, bool checkStartTime = true, string nodeFullPath = "")
        {
            return InnerCreateScheduleServiceAsync(scheduleInfo, checkStartTime, nodeFullPath);
        }

        public async Task UpsertScheduleServiceAsyncForGoogleOne(List<ScheduleInfo> scheduleInfos, bool checkStartTime = true, string nodeFullPath = "")
        {
            foreach (var scheduleInfo in scheduleInfos)
            {
                if (scheduleInfo.NoSchedule && scheduleInfo.Id.IsNotNullOrEmpty())
                {
                    DeleteScheduleService(scheduleInfo.Id);
                    continue;
                }
                var schedule = await GetScheduleByTypeServiceAsync(scheduleInfo.JobCategory);
                if (schedule.IsNotNullOrEmpty())
                {
                    await UpdateScheduleAsync(scheduleInfo);
                    continue;
                }
                await InnerCreateScheduleServiceAsync(scheduleInfo);
            }
        }

        public Task<string> CopyCreateScheduleServiceAsync(ScheduleInfo scheduleInfo, bool checkStartTime = true, string nodeFullPath = "")
        {
            //use in DAO, no auditor
            return InnerCreateScheduleServiceAsync(scheduleInfo, checkStartTime, nodeFullPath);
        }
        private async Task<string> InnerCreateScheduleServiceAsync(ScheduleInfo scheduleInfo, bool checkStartTime = true, string nodeFullPath = "")
        {
            if (!CheckScheduleInfo(scheduleInfo)) { return "-1"; }
            DateTime startTime = DateTime.Parse(scheduleInfo.StartTime);
            DateTime endTime = DateTime.Parse(scheduleInfo.EndTime);
            startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);
            startTime = DateTimeUtil.ConvertTimeToUtcDate(startTime, GeneralSettingConfig.FindSystemTimeZoneById(scheduleInfo.TimeZoneId), !scheduleInfo.IsDaylightSaving);
            endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Unspecified);
            endTime = DateTimeUtil.ConvertTimeToUtcDate(endTime, GeneralSettingConfig.FindSystemTimeZoneById(scheduleInfo.TimeZoneId), !scheduleInfo.IsDaylightSaving);
            DateTime utcNow = DateTime.UtcNow;
            if (checkStartTime)
            {
                if (startTime < utcNow)
                {
                    return "-1";
                }
            }
            else
            {
                if (startTime < utcNow)
                {
                    DateTime nextTime = scheduleInfo.NextTime;
                    do
                    {
                        logger.Info("Caculate schedule job next time.");
                        nextTime = ScheduleHelper.CalculateNextTime(scheduleInfo);
                        scheduleInfo.NextTime = nextTime;
                        //REC-4259
                        logger.Info("Caculate schedule job next time again.");
                        nextTime = ScheduleHelper.CalculateNextTime(scheduleInfo);
                        scheduleInfo.NextTime = nextTime;
                    }
                    while (!ScheduleHelper.isLongAfterTime(nextTime) && DateTime.Compare(nextTime, DateTime.UtcNow) < 0);
                }
                else
                {
                    logger.Info("Copy schedule -- startTime > utcNow, set start time as next time.");
                    logger.Info("start time:{0}, next time{1}.", startTime.ToString(), scheduleInfo.NextTime.ToString());
                    scheduleInfo.NextTime = startTime;
                }
            }
            if (scheduleInfo.EndType == EndType.EndByTime && (DateTime.Compare(startTime, endTime) > 0))
            {
                return "-1";
            }
            scheduleInfo.StartTime = startTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
            scheduleInfo.EndTime = endTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);

            logger.Info("Save schedule to DB, schedule type {0}", scheduleInfo.JobCategory);
            if (checkStartTime)
            {
                return await RMScheduleDao.CreateScheduleAsync(this.ConvertToRMSchedule(scheduleInfo));
            }
            else
            {
                return await RMScheduleDao.CreateScheduleAsync(this.ConvertToRMScheduleForBackground(scheduleInfo));
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermSynchronization, Action = AuditAction.ConfigureScheduleForTermSynchronization, BeforeHandler = typeof(RMScheduleBeforeAuditHandler), AfterHandler = typeof(RMScheduleAfterAuditHandler))]
        public async Task<string> UpdateScheduleServiceAsync(ScheduleInfo scheduleInfo, string nodeFullPath = "")
        {
            return await UpdateScheduleAsync(scheduleInfo, nodeFullPath);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermSynchronization, Action = AuditAction.ConfigureScheduleForTermSynchronization, BeforeHandler = typeof(RMScheduleBeforeAuditHandler), AfterHandler = typeof(RMScheduleAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.ConfigureDisposalJobSchedule4FS, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<string> UpdateScheduleServiceForFSAsync(ScheduleInfo scheduleInfo, string nodeFullPath = "")
        {
            return await UpdateScheduleAsync(scheduleInfo, nodeFullPath);
        }

        public async Task<List<string>> UpdateScheduleServiceAsyncForGoogleOne(List<ScheduleInfo> scheduleInfos, string nodeFullPath = "")
        {
            var responses = new List<string>();
            foreach (var scheduleInfo in scheduleInfos)
            {
                var res = await UpdateScheduleAsync(scheduleInfo, nodeFullPath);
                responses.Add(res);
            }
            return responses;
        }

        public int UpdateTeamsScheduleService(List<ScheduleInfo> scheduleInfoes)
        {
            return RMScheduleDao.BatchUpdateScheduleAsync(scheduleInfoes.ConvertAll(this.ConvertToRMSchedule));
        }

        public async Task<string> UpdateScheduleAsync(ScheduleInfo scheduleInfo, string nodeFullPath = "")
        {
            if (!CheckScheduleInfo(scheduleInfo)) { return "-1"; }
            bool hasNoChange = false;
            //RMSchedule rmSchedule = RMScheduleDao.GetSchedule(scheduleInfo.Id);
            //if (!this.IsScheduleChanged(scheduleInfo, rmSchedule))
            //{
            //    logger.Info(string.Format("Schedule have not changed.Id:[{0}]", scheduleInfo.Id));
            //    hasNoChange = true;
            //}
            // time zone 
            DateTime startTime = DateTime.Parse(scheduleInfo.StartTime);
            DateTime endTime = DateTime.Parse(scheduleInfo.EndTime);
            startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);
            startTime = DateTimeUtil.ConvertTimeToUtcDate(startTime, GeneralSettingConfig.FindSystemTimeZoneById(scheduleInfo.TimeZoneId), !scheduleInfo.IsDaylightSaving);
            DateTime utcNow = DateTime.UtcNow;
            if (!hasNoChange && startTime < utcNow)
            {
                return "-1";
            }
            endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Unspecified);
            endTime = DateTimeUtil.ConvertTimeToUtcDate(endTime, GeneralSettingConfig.FindSystemTimeZoneById(scheduleInfo.TimeZoneId), !scheduleInfo.IsDaylightSaving);
            scheduleInfo.StartTime = startTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
            scheduleInfo.EndTime = endTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
            if (!hasNoChange && scheduleInfo.EndType == EndType.EndByTime && (DateTime.Compare(startTime, endTime) > 0))
            {
                return "-1";
            }
            //if (hasNoChange)
            //{
            //    return scheduleInfo.Id;
            //}
            logger.Info(string.Format("Schedule updated.Id:[{0}]", scheduleInfo.Id));
            await RMScheduleDao.UpdateScheduleAsync(this.ConvertToRMSchedule(scheduleInfo));
            return scheduleInfo.Id;
        }

        public void RemoveScheduleNodeInfo(ScheduleType type)
        {
            RMScheduleDao.RemoveNodeInfo(type);
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermSynchronization, Action = AuditAction.ConfigureSharePointSettingsSchedule, BeforeHandler = typeof(RMScheduleBeforeAuditHandler), AfterHandler = typeof(RMScheduleAfterAuditHandler))]
        public void CreateNoSchedule(SettingScheduleType type, string nodeFullPath = "")
        {
            logger.Info("break node by no schedule, type: {0}, path: {1}", type, nodeFullPath);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermSynchronization, Action = AuditAction.ConfigureSharePointSettingsSchedule, BeforeHandler = typeof(RMScheduleBeforeAuditHandler), AfterHandler = typeof(RMScheduleAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.ConfigureDisposalJobSchedule4FS, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public void CreateNoScheduleForFS(SettingScheduleType type, string nodeFullPath = "")
        {
            logger.Info("break node by no schedule, type: {0}, path: {1}", type, nodeFullPath);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermSynchronization, Action = AuditAction.ConfigureScheduleForTermSynchronization, BeforeHandler = typeof(RMScheduleBeforeAuditHandler), AfterHandler = typeof(RMScheduleAfterAuditHandler))]
        public void DeleteScheduleService(string scheduleId, string nodeFullPath = "")
        {
            logger.Info(string.Format("Delete schedule. ScheduleId:[{0}]", scheduleId));
            RMScheduleDao.DeleteSchedule(scheduleId);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermSynchronization, Action = AuditAction.ConfigureScheduleForTermSynchronization, BeforeHandler = typeof(RMScheduleBeforeAuditHandler), AfterHandler = typeof(RMScheduleAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.ConfigureDisposalJobSchedule4FS, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public void DeleteScheduleServiceForFS(string scheduleId, string nodeFullPath = "")
        {
            logger.Info(string.Format("Delete schedule. ScheduleId:[{0}]", scheduleId));
            RMScheduleDao.DeleteSchedule(scheduleId);
        }

        /// <summary>
        /// 为将来支持多类型job schedule保留
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <returns></returns>
        public ScheduleInfo GetScheduleService(string scheduleId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// for term synchronization job, there will be always only one job returned 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<List<ScheduleInfo>> GetScheduleByTypeServiceAsync(ScheduleType type)
        {
            logger.Info("Get schedule by job type {0}.", type);
            List<RMSchedule> rmSchedules = RMScheduleDao.GetScheduleByType(type);
            List<ScheduleInfo> returnList = new List<ScheduleInfo>();
            foreach (RMSchedule rmSchedule in rmSchedules)
            {
                // time zone change datetime
                ScheduleInfo tempSchedule = this.ConvertToScheduleInfo(rmSchedule);
                await ConvertToGeneralSettingTimeZoneAsync(tempSchedule);
                returnList.Add(tempSchedule);
            }
            return returnList;
        }
        public async Task<List<ScheduleInfo>> GetScheduleByTypeServiceAsyncForGoogleOne()
        {
            List<ScheduleInfo> response = new List<ScheduleInfo>();
            List<ScheduleType> types = new List<ScheduleType> { ScheduleType.GoogleSettingSchedule, ScheduleType.GoogleDataSyncSchedule, ScheduleType.ArchiveDataRetentionSchedule };
            foreach (var type in types)
            {
                logger.Info("Get schedule by job type {0}.", type);
                List<RMSchedule> schedules = RMScheduleDao.GetScheduleByType(type);
                foreach (RMSchedule schedule in schedules)
                {
                    ScheduleInfo tempSchedule = this.ConvertToScheduleInfo(schedule);
                    ConvertScheduleByTimezone(tempSchedule, false);
                    response.Add(tempSchedule);
                }
            }
            return response;
        }


        public List<ScheduleInfo> GetScheduleByTypeAndGroupIdService(string groupId, ScheduleType type)
        {
            return RMScheduleDao.GetScheduleByTypeAndGroupId(groupId, type).ConvertAll(this.ConvertToScheduleInfo);
        }

        public async Task<ScheduleInfo> GetScheduleByIdAsync(string id)
        {
            logger.Info("Get schedule by job id {0}.", id);
            RMSchedule rmSchedule = null;
            try
            {
                rmSchedule = RMScheduleDao.GetSchedule(id);
            }
            catch (Exception e)
            {
                logger.Warn("Get schedule error: {0}", e.ToString());
            }
            if (rmSchedule == null)
            {
                return null;
            }
            ScheduleInfo returnList = new ScheduleInfo();

            // time zone change datetime
            var result = this.ConvertToScheduleInfo(rmSchedule);
            await ConvertToGeneralSettingTimeZoneAsync(result);
            return result;
        }

        public async Task<ScheduleInfo> GetScheduleByProfileIdAsync(string profileId)
        {
            logger.Info("Get schedule by profile id {0}.", profileId);
            List<RMSchedule> rmSchedules = RMScheduleDao.GetScheduleByProfileId(profileId);
            ScheduleInfo result = null;
            if (rmSchedules != null && rmSchedules.Count > 0)
            {
                RMSchedule rmSchedule = rmSchedules[0];
                result = this.ConvertToScheduleInfo(rmSchedule);
                await ConvertToGeneralSettingTimeZoneAsync(result);
            }
            return result;
        }
        /// <summary>
        /// For "Content Repository Management" page use only, StartTime/EndTime has been formatted as general settings 
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<ScheduleInfo> GetScheduleAsync(string profileId, ScheduleType type)
        {
            logger.Info("Get {1} schedule by job id {0}.", profileId, type);
            if (string.IsNullOrEmpty(profileId))
            {
                return null;
            }
            RMSchedule rmSchedule = RMScheduleDao.GetSchedule(profileId, type);
            if (rmSchedule == null)
            {
                return null;
            }
            var result = this.ConvertToScheduleInfo(rmSchedule);
            await ConvertToGeneralSettingTimeZoneAsync(result);
            return result;
        }

        public async Task<ScheduleInfo> GetAncestryScheduleAsync(string profileId, ScheduleType type)
        {
            logger.Info("Get {1} schedule by job id {0}.", profileId, type);
            RMSchedule rmSchedule = RMScheduleDao.GetAncestrySchedule(profileId, type);
            if (rmSchedule == null)
            {
                return null;
            }
            ScheduleInfo returnList = new ScheduleInfo();
            var result = this.ConvertToScheduleInfo(rmSchedule);
            await ConvertToGeneralSettingTimeZoneAsync(result);
            return result;
        }

        public bool CheckIsContainScheduleForOwnAndChildNodes(string nodeId, string groupId)
        {
            return RMScheduleDao.CheckIsContainScheduleForOwnAndChildNodes(nodeId, groupId);
        }

        /// <summary>
        /// for timer service
        /// </summary>
        /// <returns></returns>
        public async Task<List<ScheduleInfo>> GetRunableScheduleAsync()
        {
            //logger.Info("Get runable schedule,next time < current time");
            //为了解决Schedule时自动起job导致的SharedDBContext没有释放的问题
            List<RMSchedule> rmSchedules = RMScheduleDao.GetRunableSchedule();
            List<ScheduleInfo> returnList = new List<ScheduleInfo>();
            foreach (RMSchedule rmSchedule in rmSchedules)
            {
                logger.Info("Schedule {0} next time is {1}", rmSchedule.Id, new DateTime(rmSchedule.NextTime).ToString());
                returnList.Add(this.ConvertToScheduleInfo(rmSchedule));
            }
            foreach (RMSchedule rmSchedule in rmSchedules)
            {
                ScheduleInfo innerScheduleInfo = this.ConvertToScheduleInfo(rmSchedule);
                logger.Info("Reset next time for schedule.");
                this.ResetNextTime(innerScheduleInfo);
                logger.Info("Update DB to reset next time for schedule. new next time is {0}", innerScheduleInfo.NextTime);
                await RMScheduleDao.UpdateScheduleAsync(this.ConvertToRMScheduleForTask(innerScheduleInfo));
            }
            return returnList;
        }
        public async Task<List<ScheduleInfo>> GetRunableScheduleByTypeAsync(List<ScheduleType> types)
        {
            List<RMSchedule> rmSchedules = RMScheduleDao.GetRunableScheduleByTypes(types);
            List<ScheduleInfo> returnList = new List<ScheduleInfo>();
            foreach (RMSchedule rmSchedule in rmSchedules)
            {
                logger.Info("Schedule {0} next time is {1}", rmSchedule.Id, new DateTime(rmSchedule.NextTime).ToString());
                returnList.Add(this.ConvertToScheduleInfo(rmSchedule));
            }
            foreach (RMSchedule rmSchedule in rmSchedules)
            {
                ScheduleInfo innerScheduleInfo = this.ConvertToScheduleInfo(rmSchedule);
                logger.Info("Reset next time for schedule.");
                this.ResetNextTime(innerScheduleInfo);
                logger.Info("Update DB to reset next time for schedule. new next time is {0}", innerScheduleInfo.NextTime);
                await RMScheduleDao.UpdateScheduleAsync(this.ConvertToRMScheduleForTask(innerScheduleInfo));
            }
            return returnList;
        }

        public bool NeedRunSchedule()
        {
            var currentVersion = ProductVersionInfo.GetRMProductVersion();
            var packageVersion = WebUtil.GetProductVersion();
            if (!string.Equals(currentVersion, packageVersion, StringComparison.OrdinalIgnoreCase))
            {
                logger.Info("current version not equal package version, current:{0}, package:{1}.", currentVersion, packageVersion);
                return false;
            }
            return true;
        }

        public List<string> GetDisposalBreakNodes(string parentId)
        {
            return RMScheduleDao.GetDisposalBreakNodes(parentId);
        }

        public System.Threading.Tasks.Task UpdateExtentionsByTypeAsync(ScheduleType type, string extension)
        {
            return RMScheduleDao.UpdateExtentionsByTypeAsync(type, extension);
        }

        private RMSchedule ConvertToRMSchedule(ScheduleInfo scheduleInfo)
        {
            logger.Info(string.Format("Convert scheduleInfo to rmschedule.Id:[{0}]", scheduleInfo.Id));
            RMSchedule sc = new RMSchedule();
            sc.JobCategory = (int)scheduleInfo.JobCategory;
            if (!scheduleInfo.NoSchedule)
            {
                //ensure next time is UTC            
                scheduleInfo.NextTime = DateTime.Parse(scheduleInfo.StartTime);
                sc.StartTime = DateTime.Parse(scheduleInfo.StartTime).Ticks;
                if (scheduleInfo.EndType != EndType.EndByTime)
                {
                    sc.EndTime = ScheduleHelper.getLongAfterTime().Ticks;
                }
                else
                {
                    sc.EndTime = DateTime.Parse(scheduleInfo.EndTime).Ticks;
                }
                sc.NextTime = scheduleInfo.NextTime.Ticks;
                sc.TimeZoneId = scheduleInfo.TimeZoneId;
                sc.EndType = (int)scheduleInfo.EndType;
                sc.OccurrencesTotal = scheduleInfo.OccurrencesTotal;
                sc.Occurrences = 0;
                sc.Interval = scheduleInfo.Interval;
                sc.IntervalType = (int)scheduleInfo.IntervalType;
                sc.ProfileId = scheduleInfo.ProfileId;
                sc.IsDaylightSaving = scheduleInfo.IsDaylightSaving;
            }
            sc.Id = scheduleInfo.Id;
            sc.NoSchedule = scheduleInfo.NoSchedule;
            sc.Extentions = scheduleInfo.Extentions;
            sc.DAOMigrated = scheduleInfo.DAOMigrated;
            sc.DayOfMonth = (int)scheduleInfo.DayOfMonth;
            sc.WeekType = (int)scheduleInfo.WeekType;
            return sc;
        }

        private RMSchedule ConvertToRMScheduleForTask(ScheduleInfo scheduleInfo)
        {
            logger.Info("Convert scheduleInfo to rmschedule for task.");
            RMSchedule sc = new RMSchedule();
            sc.Id = scheduleInfo.Id;
            sc.StartTime = DateTime.Parse(scheduleInfo.StartTime).Ticks;
            sc.EndTime = DateTime.Parse(scheduleInfo.EndTime).Ticks;
            sc.NextTime = scheduleInfo.NextTime.Ticks;
            sc.TimeZoneId = scheduleInfo.TimeZoneId;
            sc.EndType = (int)scheduleInfo.EndType;
            sc.OccurrencesTotal = scheduleInfo.OccurrencesTotal;
            sc.Occurrences = scheduleInfo.Occurrences;
            sc.Interval = scheduleInfo.Interval;
            sc.IntervalType = (int)scheduleInfo.IntervalType;
            sc.JobCategory = (int)scheduleInfo.JobCategory;
            sc.ProfileId = scheduleInfo.ProfileId;
            sc.IsDaylightSaving = scheduleInfo.IsDaylightSaving;
            sc.Extentions = scheduleInfo.Extentions;
            sc.NoSchedule = scheduleInfo.NoSchedule;
            sc.DAOMigrated = scheduleInfo.DAOMigrated;
            sc.DayOfMonth = (int)scheduleInfo.DayOfMonth;
            sc.WeekType = (int)scheduleInfo.WeekType;
            return sc;
        }

        private ScheduleInfo ConvertToScheduleInfo(RMSchedule rmSchedule)
        {
            logger.Info("Convert rmschedule to scheduleInfo.");
            ScheduleInfo si = new ScheduleInfo();
            si.Id = rmSchedule.Id;
            si.StartTime = new DateTime(rmSchedule.StartTime).ToString();
            si.NextTime = new DateTime(rmSchedule.NextTime);
            si.EndTime = new DateTime(rmSchedule.EndTime).ToString();
            si.TimeZoneId = rmSchedule.TimeZoneId;
            si.EndType = (EndType)rmSchedule.EndType;
            si.OccurrencesTotal = rmSchedule.OccurrencesTotal;
            si.Occurrences = rmSchedule.Occurrences;
            si.Interval = rmSchedule.Interval;
            si.IntervalType = (IntervalType)rmSchedule.IntervalType;
            si.JobCategory = (ScheduleType)rmSchedule.JobCategory;
            si.ProfileId = rmSchedule.ProfileId;
            si.IsDaylightSaving = rmSchedule.IsDaylightSaving;
            si.Extentions = rmSchedule.Extentions;
            si.NoSchedule = rmSchedule.NoSchedule;
            si.DayOfMonth = rmSchedule.DayOfMonth;
            si.WeekType = (Contract.Schedule.DayOfWeek)rmSchedule.WeekType;
            return si;
        }

        private DateTime ResetNextTime(ScheduleInfo schedule)
        {
            DateTime nextTime = schedule.NextTime;
            do
            {
                logger.Info("Caculate schedule job next time again.");
                nextTime = ScheduleHelper.CalculateNextTime(schedule);
                schedule.NextTime = nextTime;
            }
            while (!ScheduleHelper.isLongAfterTime(nextTime) && DateTime.Compare(nextTime, DateTime.UtcNow) < 0);
            return nextTime;
        }
        public bool DeleteScheduleByType(ScheduleType type)
        {
            try
            {
                List<RMSchedule> rmSchedules = RMScheduleDao.GetScheduleByType(type);
                if (rmSchedules != null && rmSchedules.Count > 0)
                {
                    foreach (var rmSchedule in rmSchedules)
                    {
                        RMScheduleDao.DeleteSchedule(rmSchedule.Id);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("An error has occured  delete schedule ,message:{0}", ex.Message);
                return false;
            }
        }
        public async System.Threading.Tasks.Task CreateCustomScheduleAsync(bool isOperationGeneralSetting, ScheduleType scheduleType)
        {
            List<ScheduleInfo> infos = await GetScheduleByTypeServiceAsync(scheduleType);
            ScheduleInfo oldSchedule = null;
            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            if (infos != null && infos.Count > 0)
            {
                oldSchedule = infos[0];
            }
            bool isAddSchedule = true;
            if (oldSchedule == null)
            {
                if (isOperationGeneralSetting)
                {
                    isAddSchedule = false;
                }
            }
            else
            {
                if (oldSchedule.TimeZoneId == generalSetting.TimeZoneId && oldSchedule.IsDaylightSaving == generalSetting.DayLight)
                {
                    isAddSchedule = false;
                }

                if (CheckOldScheduleByStartTime(oldSchedule.StartTime, scheduleType))
                {
                    isAddSchedule = true;
                }
            }
            if (isAddSchedule)
            {
                DeleteScheduleByType(scheduleType);
                ScheduleInfo info = new ScheduleInfo();
                info.Id = Guid.NewGuid().ToString();
                var startTime = GetRandomStartTime(scheduleType, generalSetting.TimeZoneId);
                info.StartTime = startTime.ToString();
                info.EndTime = startTime.ToString();
                info.EndType = 0;
                info.Interval = 1;
                info.IntervalType = IntervalType.Daily;
                info.JobCategory = scheduleType;
                info.OccurrencesTotal = 1;
                info.TimeZoneId = generalSetting.TimeZoneId;
                info.IsDaylightSaving = generalSetting.DayLight;
                await CreateScheduleServiceAsync(info);
            }
        }

        public async System.Threading.Tasks.Task UpdateDashboardNextRunTimeAsync()
        {
            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();

            var scheduleInfo = RMScheduleDao.Find(item => item.JobCategory == (int)ScheduleType.Dashboard);
            if (scheduleInfo == null)
            {
                logger.Warn($"Can't find dashboard schdule info.");
                return;
            }

            if (scheduleInfo.TimeZoneId == generalSetting.TimeZoneId)
            {
                logger.Warn($"TimeZone not change.");
                return;
            }

            var nextTime = GetRandomStartTime();
            nextTime = DateTime.SpecifyKind(nextTime, DateTimeKind.Unspecified);
            nextTime = ConvertTimeToUtcDate(nextTime, GeneralSettingConfig.FindSystemTimeZoneById(generalSetting.TimeZoneId), generalSetting.DayLight);
            nextTime = DateTime.Parse(nextTime.ToString(APIDateTimeFormat.DATETYPEForAPI003));

            scheduleInfo.NextTime = nextTime.Ticks;
            scheduleInfo.TimeZoneId = generalSetting.TimeZoneId;
            await RMScheduleDao.UpdateAsync(scheduleInfo);

            DateTime ConvertTimeToUtcDate(DateTime datetime, TimeZoneInfo sourceTimezone, bool useDst)// = true)
            {
                datetime = DateTime.SpecifyKind(datetime, DateTimeKind.Unspecified);
                // 时间为夏令时时间 且指定不使用夏令时 加一小时
                if (useDst && sourceTimezone.SupportsDaylightSavingTime && sourceTimezone.IsDaylightSavingTime(datetime))
                {
                    datetime = datetime.AddHours(1);
                }
                return TimeZoneInfo.ConvertTimeToUtc(datetime, sourceTimezone);
            }

            DateTime GetRandomStartTime()
            {
                /* Fortify Issue Type: Insecure Randomness 
                * Sink Details:  this class UpdateDashboardNextRunTimeAsync
                * Ignore Reason: random用于生成下次执行时间
                */
                Random random = new Random((int)DateTime.Now.Ticks);
                var hour = random.Next(-2, 3);
                hour = hour < 0 ? hour + 24 : hour;
                var min = random.Next(0, 59);
                var second = random.Next(0, 59);

                var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GeneralSettingConfig.FindSystemTimeZoneById(generalSetting.TimeZoneId));

                var startTime = localNow;
                if (hour <= localNow.Hour)
                {
                    startTime = localNow.AddDays(1);
                }

                return new DateTime(startTime.Year, startTime.Month, startTime.Day, hour, min, second);
            }
        }

        public async System.Threading.Tasks.Task UpdateManualApprovalEmailScheduleNextRunTimeAsync()
        {
            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();

            var scheduleInfo = RMScheduleDao.Find(item => item.JobCategory == (int)ScheduleType.ManualApprovalEmailSchedule);
            if (scheduleInfo == null)
            {
                logger.Warn($"Can't find manual approval email schedule schdule info.");
                return;
            }

            if (scheduleInfo.TimeZoneId == generalSetting.TimeZoneId)
            {
                logger.Warn($"TimeZone not change.");
                return;
            }

            var nextTime = await GetRandomStartTimeAsync();
            nextTime = DateTime.SpecifyKind(nextTime, DateTimeKind.Unspecified);
            nextTime = ConvertTimeToUtcDate(nextTime, GeneralSettingConfig.FindSystemTimeZoneById(generalSetting.TimeZoneId), generalSetting.DayLight);
            nextTime = DateTime.Parse(nextTime.ToString(APIDateTimeFormat.DATETYPEForAPI003));

            scheduleInfo.NextTime = nextTime.Ticks;
            scheduleInfo.TimeZoneId = generalSetting.TimeZoneId;
            await RMScheduleDao.UpdateAsync(scheduleInfo);

            DateTime ConvertTimeToUtcDate(DateTime datetime, TimeZoneInfo sourceTimezone, bool useDst)// = true)
            {
                datetime = DateTime.SpecifyKind(datetime, DateTimeKind.Unspecified);
                // 时间为夏令时时间 且指定不使用夏令时 加一小时
                if (useDst && sourceTimezone.SupportsDaylightSavingTime && sourceTimezone.IsDaylightSavingTime(datetime))
                {
                    datetime = datetime.AddHours(1);
                }
                return TimeZoneInfo.ConvertTimeToUtc(datetime, sourceTimezone);
            }

            async Task<DateTime> GetRandomStartTimeAsync()
            {
                /* Fortify Issue Type: Insecure Randomness 
                * Sink Details: this class  CreateCustomScheduleAsync
                * Ignore Reason: random用于生成下次执行时间
                */
                Random random = new Random((int)DateTime.Now.Ticks);
                var hour = random.Next(-2, 3);
                hour = hour < 0 ? hour + 24 : hour;
                var min = random.Next(0, 59);
                var second = random.Next(0, 59);

                var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GeneralSettingConfig.FindSystemTimeZoneById(generalSetting.TimeZoneId));

                var startTime = localNow;
                if (hour <= localNow.Hour)
                {
                    startTime = localNow.AddDays(1);
                }

                return new DateTime(startTime.Year, startTime.Month, startTime.Day, hour, min, second);
            }
        }

        private DateTime GetRandomStartTime(ScheduleType scheduleType, string timeZoneId)
        {
            /* Fortify Issue Type: Insecure Randomness 
            *  Sink Details:  thisclass CreateCustomScheduleAsync
            *  Ignore Reason: random用于生成时间，格式是 时分秒 
            */
            Random ran = new Random((int)DateTime.Now.Ticks);
            var hour = ran.Next(-2, 3);
            var min = ran.Next(0, 59);
            var second = ran.Next(0, 59);

            DateTime startTime = DateTime.MinValue;
            DateTime utcNow = DateTime.UtcNow;

            TimeZoneInfo localZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);
            if (hour < 0)
            {
                hour = 24 + hour;
                startTime = localNow;
            }
            else
            {
                startTime = localNow.AddDays(1);
            }
            startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, hour, min, second);
            if (startTime < localNow)
            {
                startTime = startTime.AddDays(1);
            }
            //switch (scheduleType)
            //{
            //    case ScheduleType.UniqueIDSettingSchedule:
            //        startTime = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0);
            //        break;

            //    case ScheduleType.ManualApprovalScheduleTimer:
            //        startTime = new DateTime(localNow.Year, localNow.Month, localNow.Day, 1, 0, 0);
            //        break;
            //    case ScheduleType.EnforceRetention:
            //        startTime = new DateTime(localNow.Year, localNow.Month, localNow.Day, 2, 0, 0);
            //        break;
            //    default:
            //        break;
            //}
            return startTime;
        }
        public bool CheckOldScheduleByStartTime(string startTime, ScheduleType scheduleType)
        {
            switch (scheduleType)
            {
                case ScheduleType.UniqueIDSettingSchedule:
                case ScheduleType.SPOnPremUniqueIDSettingSchedule:
                case ScheduleType.TeamsUniqueIDSettingSchedule:
                    return startTime.EndsWith("00:00:00");
                case ScheduleType.ManualApprovalScheduleTimer:
                    return startTime.EndsWith("01:00:00");
                case ScheduleType.EnforceRetention:
                    return startTime.EndsWith("02:00:00");
                default:
                    return false;
            }
        }
        private RMSchedule ConvertToRMScheduleForBackground(ScheduleInfo scheduleInfo)
        {
            logger.Info(string.Format("Convert scheduleInfo to rmschedule.Id:[{0}]", scheduleInfo.Id));
            //ensure next time is UTC
            //scheduleInfo.NextTime = DateTime.Parse(scheduleInfo.StartTime);
            RMSchedule sc = new RMSchedule();
            sc.Id = scheduleInfo.Id;
            sc.StartTime = DateTime.Parse(scheduleInfo.StartTime).Ticks;
            if (scheduleInfo.EndType != EndType.EndByTime)
            {
                sc.EndTime = ScheduleHelper.getLongAfterTime().Ticks;
            }
            else
            {
                sc.EndTime = DateTime.Parse(scheduleInfo.EndTime).Ticks;
            }
            sc.NextTime = scheduleInfo.NextTime.Ticks;
            sc.TimeZoneId = scheduleInfo.TimeZoneId;
            sc.EndType = (int)scheduleInfo.EndType;
            sc.OccurrencesTotal = scheduleInfo.OccurrencesTotal;
            sc.Occurrences = 0;
            sc.Interval = scheduleInfo.Interval;
            sc.IntervalType = (int)scheduleInfo.IntervalType;
            sc.JobCategory = (int)scheduleInfo.JobCategory;
            sc.ProfileId = scheduleInfo.ProfileId;
            sc.IsDaylightSaving = scheduleInfo.IsDaylightSaving;
            sc.Extentions = scheduleInfo.Extentions;
            return sc;
        }

        #region getProfile id

        private List<int> displayNodeLevels = new List<int>(){
                (int)NodeLevel.SkyDriveProGroup,
                (int)NodeLevel.WebApplication,
                (int)NodeLevel.Office365GroupEntire,
                (int)NodeLevel.SiteCollection,
                (int)NodeLevel.Site,
                (int)NodeLevel.List,
                (int)NodeLevel.Folder,
            };
        public string GetProfileId(RMSPTreeNode tree)
        {
            string profileId = string.Empty;

            if (!displayNodeLevels.Contains(tree.Level))
            {
                return profileId;
            }
            var groupNode = GetGroupNode(tree);
            var teamsNode = GetTeamsNode(tree);
            var siteNode = GetSiteCollectionNode(tree);

            var groupId = "00000000-0000-0000-0000-000000000000";
            var siteId = "00000000-0000-0000-0000-000000000000";
            var teamsId = "00000000-0000-0000-0000-000000000000";

            if (groupNode != null)
            {
                groupId = groupNode.SPObjectId;
            }
            if (teamsNode != null)
            {
                teamsId = teamsNode.TeamsId;
            }
            var teamsIdStr = tree.Type == ContentSourceType.Teams ? teamsId + "|" : "";

            if (siteNode != null)
            {
                siteId = siteNode.SPObjectId;
            }
            if (tree.Level == (int)NodeLevel.SiteCollection)
            {
                return groupId + "|" + teamsIdStr + siteId;
            }
            if (tree.Level == (int)NodeLevel.Office365GroupEntire)
            {
                return (groupId + "|" + teamsIdStr).TrimEnd('|');
            }
            if (tree.Level == (int)NodeLevel.WebApplication || tree.Level == (int)NodeLevel.SkyDriveProGroup)
            {
                return groupId;
            }
            var parentWebId = GetParentWebIds(tree);
            if (!string.IsNullOrEmpty(parentWebId))
            {
                parentWebId += "|";
            }
            var libraryId = GetParentLibraryId(tree);
            if (libraryId != "")
            {
                libraryId += "|";
            }
            var parentFolderId = GetParentFolderIds(tree);
            if (parentFolderId != "")
            {
                parentFolderId += "|";
            }
            profileId = groupId + "|" + teamsIdStr + siteId + "|" + parentWebId + libraryId + parentFolderId + tree.SPObjectId;
            return profileId;
        }
        public RMSPTreeNode GetGroupNode(RMSPTreeNode node)
        {
            while (node != null && (node.Level != (int)NodeLevel.WebApplication && node.Level != (int)NodeLevel.SkyDriveProGroup))
            {
                node = node.Parent;
            }
            return node;
        }

        public RMSPTreeNode GetTeamsNode(RMSPTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.Office365GroupEntire)
            {
                node = node.Parent;
            }
            return node;
        }

        public RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }
        private string GetParentWebIds(RMSPTreeNode node)
        {
            var result = "";
            while (node != null && node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
                if (node != null && node.Level == (int)NodeLevel.Site)
                {
                    result = result == "" ? node.SPObjectId : node.SPObjectId + "|" + result;
                }
            }
            return result;
        }
        private string GetParentLibraryId(RMSPTreeNode node)
        {
            var result = "";
            if (node.Level != (int)NodeLevel.WebApplication && node.Level != (int)NodeLevel.SiteCollection && node.Level != (int)NodeLevel.Site && node.Level != (int)NodeLevel.List)
            {
                while (node != null && node.Level != (int)NodeLevel.List)
                {
                    node = node.Parent;
                }
                ArgumentCheck.NotNull(node, nameof(node));
                result = node.SPObjectId;
            }
            return result;
        }
        private string GetParentFolderIds(RMSPTreeNode node)
        {
            var result = "";
            while (node != null && node.Level != (int)NodeLevel.List)
            {
                node = node.Parent;
                if (node != null && node.Level == (int)NodeLevel.Folder)
                {
                    result = result == "" ? node.SPObjectId : node.SPObjectId + "|" + result;
                }
            }
            return result;
        }


        public string GetProfileId(RMSPSampleTreeNode tree)
        {
            string profileId = string.Empty;

            if (tree == null)
            {
                return profileId;
            }

            if (!displayNodeLevels.Contains(tree.Level))
            {
                return profileId;
            }
            var groupNode = GetGroupNode(tree);
            var teamsNode = GetTeamsNode(tree);
            var siteNode = tree.Level == (int)NodeLevel.SiteCollection ? tree : GetSiteCollectionNode(tree);

            var groupId = "00000000-0000-0000-0000-000000000000";
            var siteId = "00000000-0000-0000-0000-000000000000";
            var teamsId = "00000000-0000-0000-0000-000000000000";

            if (groupNode != null)
            {
                groupId = groupNode.SPObjectId;
            }
            if (teamsNode != null)
            {
                teamsId = teamsNode.TeamsId;
            }
            var teamsIdStr = tree.SourceType == (int)SourceFlag.Teams ? teamsId + "|" : "";

            if (siteNode != null)
            {
                siteId = siteNode.SPObjectId;
            }
            if (tree.Level == (int)NodeLevel.SiteCollection)
            {
                return groupId + "|" + teamsIdStr + siteId;
            }
            if (tree.Level == (int)NodeLevel.Office365GroupEntire)
            {
                return (groupId + "|" + teamsIdStr).TrimEnd('|');
            }
            if (tree.Level == (int)NodeLevel.WebApplication)
            {
                return groupId;
            }
            var parentWebId = GetParentWebIds(tree);
            if (!string.IsNullOrEmpty(parentWebId))
            {
                parentWebId += "|";
            }
            var libraryId = GetParentLibraryId(tree);
            if (libraryId != "")
            {
                libraryId += "|";
            }
            var parentFolderId = GetParentFolderIds(tree);
            if (parentFolderId != "")
            {
                parentFolderId += "|";
            }
            profileId = groupId + "|" + teamsIdStr + siteId + "|" + parentWebId + libraryId + parentFolderId + tree.SPObjectId;
            return profileId;
        }
        public RMSPSampleTreeNode GetGroupNode(RMSPSampleTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.WebApplication)
            {
                if (node.Parent == null && node.Level == (int)NodeLevel.SiteCollection && Guid.TryParse(node.ParentId, out var containerId))
                {
                    var containerNode = RMRemoteNodeDao.GetRemoteNodeById(containerId);
                    if (containerNode == null)
                    {
                        return null;
                    }

                    return new RMSPSampleTreeNode
                    {
                        Id = containerNode.Id,
                        SPObjectId = containerNode.Id,
                        Name = containerNode.Url,
                        DisplayName = containerNode.Url,
                        FullPath = containerNode.Url,
                        Level = (int)NodeLevel.WebApplication,
                        ParentId = containerNode.ParentId,
                        TeamsId = node.TeamsId,
                        SourceType = node.SourceType,
                    };
                }

                node = node.Parent;
            }
            return node;
        }

        public RMSPSampleTreeNode GetTeamsNode(RMSPSampleTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.Office365GroupEntire)
            {
                if (node.Parent == null && node.Level == (int)NodeLevel.SiteCollection && !string.IsNullOrEmpty(node.TeamsId))
                {
                    var teamsNode = RMRemoteNodeDao.GetTeamsNodeByTeamsId(node.TeamsId);
                    if (teamsNode == null)
                    {
                        return null;
                    }

                    return new RMSPSampleTreeNode
                    {
                        Id = teamsNode.Id,
                        SPObjectId = teamsNode.SPObjectId,
                        Name = teamsNode.Name,
                        DisplayName = teamsNode.DisplayName,
                        FullPath = teamsNode.FullPath,
                        Level = teamsNode.Level,
                        ParentId = teamsNode.ParentId,
                        TeamsId = teamsNode.TeamsId,
                        SourceType = node.SourceType,
                    };
                }

                node = node.Parent;
            }
            return node;
        }

        public RMSPSampleTreeNode GetSiteCollectionNode(RMSPSampleTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }
        private string GetParentWebIds(RMSPSampleTreeNode node)
        {
            var result = "";
            while (node != null && node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
                if (node != null && node.Level == (int)NodeLevel.Site)
                {
                    result = result == "" ? node.SPObjectId : node.SPObjectId + "|" + result;
                }
            }
            return result;
        }
        private string GetParentLibraryId(RMSPSampleTreeNode node)
        {
            var result = "";
            if (node.Level != (int)NodeLevel.WebApplication && node.Level != (int)NodeLevel.SiteCollection && node.Level != (int)NodeLevel.Site && node.Level != (int)NodeLevel.List)
            {
                while (node != null && node.Level != (int)NodeLevel.List)
                {
                    node = node.Parent;
                }
                ArgumentCheck.NotNull(node, nameof(node));
                result = node.SPObjectId;
            }
            return result;
        }
        private string GetParentFolderIds(RMSPSampleTreeNode node)
        {
            var result = "";
            while (node != null && node.Level != (int)NodeLevel.List)
            {
                node = node.Parent;
                if (node != null && node.Level == (int)NodeLevel.Folder)
                {
                    result = result == "" ? node.SPObjectId : node.SPObjectId + "|" + result;
                }
            }
            return result;
        }


        public string GetProfileId(RMEXOTreeNode node)
        {
            var result = node.Id.ToString();

            while (node != null && node.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup && node.Level != (int)NodeLevel.ExchangeOnlineO365Group)
            {
                node = node.Parent;
                if (node != null)
                {
                    result = result == "" ? node.Id.ToString() : node.Id.ToString() + "|" + result;
                }
            }
            return result;
        }

        public string GetProfileId(RMSampleEXOTreeNode node)
        {
            var result = node.Id.ToString();

            while (node != null && node.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup && node.Level != (int)NodeLevel.ExchangeOnlineO365Group)
            {
                node = node.Parent;
                if (node != null)
                {
                    result = result == "" ? node.Id.ToString() : node.Id.ToString() + "|" + result;
                }
            }
            return result;
        }

        public string GetProfileId(BoxTreeNode node)
        {
            var result = node.Id.ToString();
            ArgumentNullException.ThrowIfNull(node);
            while (node?.Level != (RMNodeLevel)NodeLevel.BoxConnectionGroup)
            {
                node = node?.Parent;
                if (node != null)
                {
                    result = result == "" ? node.Id.ToString() : node.Id.ToString() + "|" + result;
                }
            }
            return result;
        }

        public string GetProfileId(RMSampleGoogleTreeNode node)
        {
            var result = node.Id.ToString();

            while (node != null && (node.Level != (int)NodeLevel.GoogleMyDriveContainer && node.Level != (int)NodeLevel.GoogleSharedDriveContainer))
            {
                node = node.Parent;
                if (node != null)
                {
                    result = result == "" ? node.Id : node.Id + "|" + result;
                }
            }
            return result;
        }

        public string GetProfileId(RMGoogleTreeNode node)
        {
            var result = node.Id;
            while (node != null && (node.Level != (int)NodeLevel.GoogleMyDriveContainer && node.Level != (int)NodeLevel.GoogleSharedDriveContainer))
            {
                node = node.Parent;
                if (node != null)
                {
                    result = result == "" ? node.Id : node.Id + "|" + result;
                }
            }
            return result;
        }

        public string GetProfileId(List<Guid> ids)
        {
            return string.Join("|", ids);
        }

        public string GetProfileId(RMFSTreeNode node)
        {
            var result = node.Id.ToString();
            while (node != null && node.Level != (int)NodeLevel.WebApplication)
            {
                node = node.Parent;
                if (node != null)
                {
                    result = result == "" ? node.Id.ToString() : node.Id.ToString() + "|" + result;
                }
            }
            return result;
        }
        #endregion

        public void DeleteSchedules(ScheduleType type, string profileIdPath)
        {
            try
            {
                RMScheduleDao.DeleteSchedules(type, profileIdPath);
                logger.Info("success delete schedules  path [{0}],type[{1}]", profileIdPath, (int)type);
            }
            catch (Exception ex)
            {
                logger.Warn("error delete schedules  path [{0}],type[{1}] message:{2}", profileIdPath, (int)type, ex.ToString());
            }
        }

        private bool CheckScheduleInfo(ScheduleInfo info)
        {
            var result = true;
            if (info.Interval < 1 || (info.EndType == EndType.EndByOccurrences && info.OccurrencesTotal < 1))
            {
                result = false;
            }
            if (info.IntervalType == IntervalType.Monthly && ((info.DayOfMonth > 31 && info.DayOfMonth < 100) || info.DayOfMonth > 103))
            {
                result = false;
            }
            return result;
        }

        private async System.Threading.Tasks.Task ConvertToGeneralSettingTimeZoneAsync(ScheduleInfo result, bool formatDateTimeByGeneralSetting = false)
        {
            if (result.NoSchedule)
            {
                return;
            }
            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            
            bool isEnableJPMC = RMKeyValueDao.IsEnableJPMCFileSystemFeature();
            var dateTimeFormat = JSDateTimeFormat.DEFAULT_TIME_FORMAT;

            if (formatDateTimeByGeneralSetting || isEnableJPMC)
            {
                string timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), gls.TimeFormatId), true)];
                string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
                dateTimeFormat = $"{dateFormat} {timeFormat}"; ;
            }

            result.StartTime = DateTimeUtil.ConvertFromUTCDateTime(result.StartTime, gls, dateTimeFormat);
            result.EndTime = DateTimeUtil.ConvertFromUTCDateTime(result.EndTime, gls, dateTimeFormat);

            result.TimeZoneId = gls.TimeZoneId;
            result.IsDaylightSaving = gls.DayLight;
        }

        public string GetProfileId(AzureFileShareTreeNode node)
        {
            var result = node.Id;
            while (node != null && node.Level != AvePoint.RA.Contract.RMWeb.Tree.Base.RMNodeLevel.AzureFileShareGroup)
            {
                node = node.Parent;
                if (node != null)
                {
                    result = result == "" ? node.Id : node.Id + "|" + result;
                }
            }
            return result;
        }
        [Audit(Module = AuditModule.GoogleDrive, Category = AuditCategory.SharePointSettings, Action = AuditAction.ConfigureGoogleDisposalJobSchedule, BeforeHandler = typeof(RMScheduleBeforeAuditHandler), AfterHandler = typeof(RMScheduleAfterAuditHandler))]
        public async Task<string> CreateScheduleServiceForGoogleAsync(ScheduleInfo scheduleInfo, bool checkStartTime = true, string nodeFullPath = "")
        {
            return await InnerCreateScheduleServiceAsync(scheduleInfo, checkStartTime, nodeFullPath);
        }
        [Audit(Module = AuditModule.GoogleDrive, Category = AuditCategory.SharePointSettings, Action = AuditAction.ConfigureGoogleDisposalJobSchedule, BeforeHandler = typeof(RMScheduleBeforeAuditHandler), AfterHandler = typeof(RMScheduleAfterAuditHandler))]
        public async Task<string> UpdateScheduleServiceForGoogleAsync(ScheduleInfo scheduleInfo, string nodeFullPath = "")
        {
            return await UpdateScheduleAsync(scheduleInfo, nodeFullPath);
        }
        [Audit(Module = AuditModule.GoogleDrive, Category = AuditCategory.SharePointSettings, Action = AuditAction.ConfigureGoogleDisposalJobSchedule, BeforeHandler = typeof(RMScheduleBeforeAuditHandler), AfterHandler = typeof(RMScheduleAfterAuditHandler))]
        public void DeleteScheduleServiceForGoogle(string scheduleId, string nodeFullPath = "")
        {
            logger.Info(string.Format("Delete schedule. ScheduleId:[{0}]", scheduleId));
            RMScheduleDao.DeleteSchedule(scheduleId);
        }

        #region GoogleOne time conversation

        public void ConvertScheduleByTimezone(ScheduleInfo scheduleInfo, bool isNeedSwapBoth = true, string timeFormat = null)
        {
            try
            {
                if (scheduleInfo == null || scheduleInfo.NoSchedule || string.IsNullOrEmpty(TenantLocalValue.TimezoneId)) return;

                var g1TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TenantLocalValue.TimezoneId);

                logger.Info($"Before convert timezone: StartTime: {scheduleInfo.StartTime}, EndTime: {scheduleInfo.EndTime}, TimeZoneId: {scheduleInfo.TimeZoneId}.");

                if (isNeedSwapBoth)
                {
                    scheduleInfo.StartTime = DateTimeUtil.GetFormattedTimeBetweenTimezones(scheduleInfo.StartTime, scheduleInfo.TimeZoneId, g1TimeZoneInfo.Id, JSDateTimeFormat.DEFAULT_TIME_FORMAT);
                    scheduleInfo.EndTime = DateTimeUtil.GetFormattedTimeBetweenTimezones(scheduleInfo.EndTime, scheduleInfo.TimeZoneId, g1TimeZoneInfo.Id, JSDateTimeFormat.DEFAULT_TIME_FORMAT);
                }
                else
                {
                    scheduleInfo.StartTime = DateTimeUtil.GetFormattedTimeFromUtc(scheduleInfo.StartTime, g1TimeZoneInfo.Id);
                    scheduleInfo.EndTime = DateTimeUtil.GetFormattedTimeFromUtc(scheduleInfo.EndTime, g1TimeZoneInfo.Id);
                }

                scheduleInfo.TimeZoneId = g1TimeZoneInfo.Id;
                scheduleInfo.IsDaylightSaving = g1TimeZoneInfo.SupportsDaylightSavingTime;

                logger.Info($"After convert timezone: StartTime: {scheduleInfo.StartTime}, EndTime: {scheduleInfo.EndTime}, TimeZoneId: {scheduleInfo.TimeZoneId}.");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured while convert timezone. Ex: {ex.Message}");
            }
        }

        public async Task CreateScheduleNotificationAsync(ScheduleType scheduleType)
        {
            var jobNotificationSchedule = await GetScheduleByTypeServiceAsync(scheduleType);
            if (jobNotificationSchedule != null && jobNotificationSchedule.Count > 0)
            {
                return;
            }
            var generalSetting = GeneralSettingService.GetGeneralSettingAsync();
            var info = new ScheduleInfo
            {
                Id = Guid.NewGuid().ToString()
            };

            var utcNow = DateTime.UtcNow;
            var globalTimeZoneId = (await generalSetting).TimeZoneId;
            TimeZoneInfo localZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);
            localNow = localNow.AddDays(1);

            var startTime = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0);
            info.StartTime = startTime.ToString();
            info.EndTime = startTime.ToString();
            info.EndType = 0;
            info.Interval = 1;
                     info.IntervalType = IntervalType.Daily;
                    info.JobCategory = scheduleType;
                    info.OccurrencesTotal = 1;
                    info.TimeZoneId = (await generalSetting).TimeZoneId;
                    await CreateScheduleServiceAsync(info);
                }

        #endregion

        public Task<string> CreateScheduleWithoutAuditAsync(ScheduleInfo scheduleInfo, bool checkStartTime = true, string nodeFullPath = "")
        {
            return InnerCreateScheduleServiceAsync(scheduleInfo, checkStartTime, nodeFullPath);
        }

        public async Task<string> UpdateScheduleWithoutAuditAsync(ScheduleInfo scheduleInfo, string nodeFullPath = "")
        {
            return await UpdateScheduleAsync(scheduleInfo, nodeFullPath);
        }

        public void DeleteScheduleWithoutAudit(string scheduleId, string nodeFullPath = "")
        {
            logger.Info(string.Format("Delete schedule without audit. ScheduleId:[{0}]", scheduleId));
            RMScheduleDao.DeleteSchedule(scheduleId);
        }
    }
}
