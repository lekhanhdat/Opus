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
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.PersonalSetting;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.PersonalSetting.AuditHandler;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Common;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.PersonalSetting
{
    [Audit]
    public class RMPersonalSettingService : RMServiceBase, IPersonalSettingService
    {
        protected RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IPersonalSettingDao PersonalSettingDao => PlatformWindsorManager.GetService<IPersonalSettingDao>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.DeleteSearchCriteria, BeforeHandler = typeof(PersonalSettingBeforeAuditHandler), AfterHandler = typeof(PersonalSettingAfterAuditHandler))]
        public async Task<bool> DeleteAsync(RMPersonalSettingDto dto)
        {
            var result =  await PersonalSettingDao.DeleteByIdsAsync(dto.Owner, new List<int> { dto.Id}) > 0;
            if (!PersonalSettingDao.ExistsDefault(dto.Owner, dto.Type))
            {
                PersonalSettingDao.SetBuiltInAsDefault(dto.Owner, dto.Type);
            }
            return result;
        }

        public RMPersonalSettingDto GetByOwnerAndId(string userId, int id)
        {
            return GetByUserAndId(userId, id, true);
        }

        private RMPersonalSettingDto GetByUserAndId(string userId, int id, bool includeContent)
        {
            var dto = PersonalSettingDao.GetById(id, includeContent);
            if (dto == null || userId != dto.Owner)
            {
                logger.Warn($"No such setting with id: {id}, owner : {userId}");
                return null;
            }

            return dto;
        }

        public RMPersonalSettingDto GetById(int id, bool includeContent = true)
        {
            var dto = PersonalSettingDao.GetById(id, includeContent);
            return dto;
        }

        public List<RMPersonalSettingDto> GetByOwnerAndType(string userId, PersonalSettingType type)
        {
            return PersonalSettingDao.GetByOwnerAndType(userId, type, false);
        }

        public List<RMPersonalSettingDto> GetByOwnerAndTypeForGoogleOne(string userId, PersonalSettingType type)
        {
            return PersonalSettingDao.GetByOwnerAndTypeForGoogleOne(userId, type);
        }

        public List<RMPersonalSettingDto> GetSharedSettings(string userId, PersonalSettingType type)
        {
            return PersonalSettingDao.GetSharedSettings(userId, type);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.SaveSearchCriteria, BeforeHandler = typeof(PersonalSettingBeforeAuditHandler), AfterHandler = typeof(PersonalSettingAfterAuditHandler))]
        public int Save(RMPersonalSettingDto dto)
        {
            if (PersonalSettingDao.ExistSameNameEntity(dto)) throw new SameNameException();
            return PersonalSettingDao.CreateOrUpdate(dto);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.SetSearchCriteriaAsDefault, BeforeHandler = typeof(PersonalSettingBeforeAuditHandler), AfterHandler = typeof(PersonalSettingAfterAuditHandler))]
        public bool SetAsDefault(RMPersonalSettingDto param)
        {
            //var dto = GetById(param.Id, false);
            //if (dto == null) return false;

            return PersonalSettingDao.SetAsDefault(param.Id, param.Owner);
        }

        public bool ExistsBuiltIn(RMPersonalSettingDto param)
        {
            return PersonalSettingDao.ExistsBuiltIn(param.Owner, param.Type);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.ShareSearchCriteria, BeforeHandler = typeof(PersonalSettingBeforeAuditHandler), AfterHandler = typeof(PersonalSettingAfterAuditHandler))]
        public void Share(RMPersonalSettingSecurityGroupMappingDto dto)
        {
            var setting = GetByUserAndId(dto.Owner, dto.Id, false);
            if (setting == null);
            PersonalSettingDao.Share(dto.Id, dto.SecurityGroups);
        }

        public RMGlobalSearchSharedSettingDto GetSharedInfo(int id)
        {
            var groups = PersonalSettingDao.GetSharedGroups(id);
            return new RMGlobalSearchSharedSettingDto { Id = id, SecurityGroups = groups };
        }

        public bool IsSharedToUser(string userId, int settingId)
        {
            return PersonalSettingDao.IsSharedToUser(userId, settingId);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.CancelShareSearchCriteria, BeforeHandler = typeof(PersonalSettingBeforeAuditHandler), AfterHandler = typeof(PersonalSettingAfterAuditHandler))]
        public void CancelShare(string userId, int id)
        {
            var setting = GetByUserAndId(userId, id, false);
            if (setting == null);
            PersonalSettingDao.CancelShare(id);
        }

        #region Offline search

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.RunOfflineSearch, BeforeHandler = typeof(PersonalSettingBeforeAuditHandler), AfterHandler = typeof(PersonalSettingAfterAuditHandler))]
        public string RunSearchOffline(int id)
        {
            var groupId = TenantLocalValue.LogonGroupId;
            var loginName = TenantLocalValue.LogonUserEmail;
            var userId = TenantLocalValue.LogonUserId;
            JobQueueDto jqDto = new JobQueueDto()
            {
                JobType = JobType.ExplorerOfflineSearch,
                Parameters = string.Format("{0} {1}", id, userId),
                JobRunType = Contract.RMWeb.JobRunBy.Control,
                TenantGroupId = groupId,
                JobRunByUser = loginName
            };
            string jobId = mJobQueueService.AddToDBJobQueue(jqDto);
            return jobId;
        }

        public async Task<string> RealRunSearchOfflineAsync(JobRunBy jobRunBy, string jobRunByUser, int settingId, string userId)
        {
            string id = string.Empty;
            if (jobRunBy == JobRunBy.Control)
            {
                id = JobMonitorService.CreateJobWithScopeId(JobType.ExplorerOfflineSearch, jobRunByUser, settingId.ToString(), userId);
                logger.Info("Begin control explorer offline search Job {0}", id);
            } 

            List<Contract.RMWeb.JobMonitor.JMItemInfo> runningJobs = await JobMonitorService.GetEndedJobByScopeIdAsync(settingId.ToString(), new int[] { 0, 1 }, userId);  //JobMonitorService.GetRunningJobs(new List<JobType>() { JobType.ExplorerOfflineSearch }, settingId.ToString()); 
            bool isSkip = false;
            if (runningJobs.Any(j => j.JobId != id))
            {
                logger.Warn($"Running offline search job for profile Id {settingId}, {string.Join(";", runningJobs.Select(a=>a.JobId).ToArray())}");
                isSkip = true;
            }
            if (!isSkip)
            {
                StartSearchOffline(id, settingId, userId);
            }
            else
            {
                logger.Info(I18NEntity.GetString("Skipped this job. A job for this profile is already running."));
                JobMonitorService.UpdateJobStatus(id, Contract.RMWeb.JobMonitor.JobStatus.Skipped, "Skipped this job. An offline search job is already running.");
            }

            return id;
        }

        private void StartSearchOffline(string jobId, int profileId, string userId)
        { 
            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.ImportPhysicalRecords,
                CommandLine = string.Format("{0} {1} {2} {3}", JobType.ExplorerOfflineSearch, jobId, userId, profileId),
            });

        }

        #endregion
        public void UpgradeDefaultSetting(string owner, PersonalSettingType type)
        {
            try
            {
                PersonalSettingDao.UpgradeDefaultSetting(owner, type);
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while upgrading the default setting. owner : {owner}, type: {type}");
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.SetSearchCriteriaAsDefault, BeforeHandler = typeof(PersonalSettingBeforeAuditHandler), AfterHandler = typeof(PersonalSettingAfterAuditHandler))]
        public async Task<bool> SetAsDefaultForGoogleOne(RMPersonalSettingDto param)
        {
            //var dto = GetById(param.Id, false);
            //if (dto == null) return false;

            return await PersonalSettingDao.SetAsDefaultForGoogleOne(param.Id, param.Owner);
        }
    }
}
