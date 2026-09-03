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
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AvePoint.RA.DB.Model;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Web.Common.Filters;

namespace AvePoint.RA.Web.Controllers.BusinessClassification
{
    [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin, preferred: false)]
    public class PRSettingApiController : BaseApiController
    {
        #region Interface
        private IRMPhysicalRecordSettingsService _PhysicalRecordSettingService;
        private IRMPhysicalRecordSettingsService PhysicalRecordSettingService => PlatformWindsorManager.GetService(ref _PhysicalRecordSettingService);
        private ILocationManagementService _LocationManagementService;
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService(ref _LocationManagementService);
        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private IScheduleService _ScheduleService;
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);
        private IExplorerService _ExplorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);
        private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);
        private IRMJobService _RMJobService;
        private IRMJobService RMJobService => PlatformWindsorManager.GetService(ref _RMJobService);
        private IManualProcessManagementService _ManualProcessManagementService;
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService(ref _ManualProcessManagementService);
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        #endregion

        #region Profile Id
        [HttpGet]
        public string GetProfileId(Guid locationUid)
        {
            try
            {
                return PhysicalRecordSettingService.GetProfileId(locationUid);
            }
            catch (Exception ex)
            {
                Logger.Error("Inherit GlobalSettings Failed.ERROR:{0}", ex.ToString());
                return (-1).ToString();
            }
        }
        #endregion

        #region Load & Save Node Settings

        [HttpGet]
        public async Task<string> LoadPhysicalRecordSetting(Guid locationUid)
        {
            var setting = await PhysicalRecordSettingService.LoadPhysicalRecordSettingAsync(locationUid);
            if (setting.ApprovalType == (int)ApprovalType.ApprovalProcess)
            {
                var result = Guid.TryParse(setting.WorkflowReferenceId, out var referenceId);
                if (result)
                {
                    var workflow = ManualProcessManagementService.GetWorkflow(referenceId);
                    setting.WorkflowReferenceName = workflow?.Name;
                }
            }
            return JsonConvert.SerializeObject(setting);
        }

        [HttpPost]
        public async Task<string> SavePRTermSetting([FromBody] RMPRSaveTermDto curSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                if (!curSetting.DefaultTermId.Equals(Guid.Empty) && TaxonomyService.IsOrphanedTerm(curSetting.DefaultTermId))
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DefaultTermIsOrphaned;
                }
                else
                {
                    result = PhysicalRecordSettingService.SaveTerm(curSetting);
                }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                Logger.Error("Save PR Settings Failed.ERROR:{0}", ex.Message);
                throw;
            }
            await CreateExplorerTimerScheduleAsync();
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidFSParameterActionFilter("ValidateSavePRTermSetting")]
        public async Task<string> SaveRecordOwners([FromBody] RMPRSaveRecordOwnerDto curSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                var syncUserResult = await PhysicalRecordSettingService.SyncADUsersAsync(curSetting.RecordOwner);
                if (syncUserResult.MessageType == RAMessageType.Successful)
                {
                    result = await PhysicalRecordSettingService.SaveRecordOwnerAsync(curSetting);
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = syncUserResult.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                Logger.Error("Save PR Settings Failed.ERROR:{0}", ex.Message);
                throw;
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public string InheritParentPRSettings([FromBody]Guid locationUid)
        {
            var result = SaveSPSettingResult.Sucess;
            try
            {
                PhysicalRecordSettingService.InheritParentSetting(locationUid);
            }
            catch (Exception ex)
            {
                Logger.Error("Inherit GlobalSettings Failed.ERROR:{0}", ex.ToString());
                result = SaveSPSettingResult.Failed;
            }
            return result.ToString();
        }

        #endregion

        #region Dispose Schedule
        [HttpPost]
        public async Task<string> UpdatePRDisposeSchedule([FromBody] RMPRTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                    //var cloneNodeInfo = nodeSetting.Clone();
                    //cloneNodeInfo.DisposeScheduleInfo = null;
                    //nodeSetting.DisposeScheduleInfo.Extentions = JsonConvert.SerializeObject(cloneNodeInfo);
                    var schedule = await ScheduleService.UpdateScheduleServiceAsync(nodeSetting.DisposeScheduleInfo, GetNodeFullPath(nodeSetting));
                    if (schedule == "-1")
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.ScheduleServiceFailed;
                    }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                Logger.Error("Update Dispose Schedule Service Failed.ERROR:{0}", ex.Message);
                throw;
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> CreatePRDisposeSchedule([FromBody] RMPRTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                    nodeSetting.DisposeScheduleInfo.Id = Guid.NewGuid().ToString();
                    //var cloneNodeInfo = nodeSetting.Clone();
                    //cloneNodeInfo.DisposeScheduleInfo = null;
                    //nodeSetting.DisposeScheduleInfo.Extentions = JsonConvert.SerializeObject(cloneNodeInfo);
                    nodeSetting.DisposeScheduleInfo.ProfileId = PhysicalRecordSettingService.GetProfileId(nodeSetting.UniqueId);
                    var schedule = await ScheduleService.CreateScheduleServiceAsync(nodeSetting.DisposeScheduleInfo, true, GetNodeFullPath(nodeSetting));
                    if (schedule == "-1")
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.ScheduleServiceFailed;
                    }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                Logger.Error("Update Dispose Schedule Service Failed.ERROR:{0}", ex.Message);
                throw;
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public string DeletePRDisposeSchedule([FromBody] RMPRTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                ScheduleService.DeleteScheduleService(nodeSetting.DisposeScheduleInfo.Id, GetNodeFullPath(nodeSetting));
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                Logger.Error("Delete Collection Schedule Service Failed.ERROR:{0}", ex.Message);
                throw;
            }
            return JsonConvert.SerializeObject(result);
        }
        
        [HttpPost]
        public string BreakPRDisposeSchedule([FromBody] RMPRTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                nodeSetting.DisposeScheduleInfo.Id = "";
                ScheduleService.CreateNoSchedule(SettingScheduleType.Dispose, GetNodeFullPath(nodeSetting));
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                Logger.Error("Break Collection Schedule Service Failed.ERROR:{0}", ex.Message);
                throw;
            }
            return JsonConvert.SerializeObject(result);
        }
        #endregion

        #region Run Job

        [HttpPost]
        public string RunPhysicalTimerJob()
        {
            return ExplorerService.RunPhysicalTimerJob(JobRunBy.Control);
        }

        [HttpPost]
        public async Task<string> RunPhysicalJob([FromBody] RunPhysicalJobParam dto)
        {
            if (TenantService.IsNewOpusTenant())
            {
                return JsonConvert.SerializeObject(await PhysicalRecordSettingService.RunPhysicalRecordsDisposalJobAsync(dto.Id, JobRunBy.Control, dto.SkipRemove));
            }
            else
            {
                return JsonConvert.SerializeObject(await RMJobService.OldOpusTenantRunPhysicalJobNowAsync(dto.Id, JobRunBy.Control, dto.SkipRemove));
            }
            //return JsonConvert.SerializeObject(mRMSPSettingsService.RunPhysicalDisposalJob(id, JobRunBy.Control));
        }
       
        #endregion

        #region Term
        [HttpPost]
        public Task<string> GetPRSavedTree([FromBody] CurrentSettingsInfo settingInfo)
        {
            using (RA.Common.PerformanceScope scope = new RA.Common.PerformanceScope("Get PR SettingSavedTree"))
            {
                return TaxonomyService.GetPRSettingSavedTreeAsync(settingInfo, true);
            }
        }

        [HttpPost]
        public Task<string> GetPRSubTerm([FromBody] FSTreePage tree)
        {
            int pIndex = tree.PageIndex ?? 0;
            int pSize = tree.PageSize ?? 0;

            //调整一下index，和前台匹配
            if (pIndex > 0)
            {
                pIndex -= 1;
            }

            string nodeId = tree.NodeId ?? string.Empty;
            string nodeType = tree.NodeType ?? string.Empty;
            int SettingType = tree.SettingType != null ? Convert.ToInt32(tree.SettingType) : 0;
            return TaxonomyService.GetTaxonomyTermAsync(nodeType, nodeId, pIndex, pSize, tree.ConnGroupId, SettingType, true);
        }
        #endregion

        #region Private Method

        private string GetNodeFullPath(RMPRTreeNode nodeSetting)
        {
            return LocationManagementService.GetLocationPathById(nodeSetting.UniqueId);
        }

        private async Task<string> CreateExplorerTimerScheduleAsync()
        {
            List<ScheduleInfo> infos = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.PRExplorerTimer);
            ScheduleInfo oldSchedule = null;
            if (infos != null && infos.Count > 0)
            {
                oldSchedule = infos[0];
            }
            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            if (oldSchedule == null || oldSchedule.TimeZoneId != generalSetting.TimeZoneId)
            {
                if (oldSchedule != null)
                {
                    ScheduleService.DeleteScheduleByType(ScheduleType.PRExplorerTimer);
                }
                ScheduleInfo info = new ScheduleInfo();
                info.Id = Guid.NewGuid().ToString();

                DateTime utcNow = DateTime.UtcNow;
                var globalTimeZoneId = generalSetting.TimeZoneId;
                TimeZoneInfo localZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
                var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);
                localNow = localNow.AddDays(1);

                DateTime startTime = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0);
                info.StartTime = startTime.ToString();
                info.EndTime = startTime.ToString();
                info.EndType = 0;
                info.Interval = 1;
                info.IntervalType = IntervalType.Daily;
                info.JobCategory = ScheduleType.PRExplorerTimer;
                info.OccurrencesTotal = 1;
                info.TimeZoneId = generalSetting.TimeZoneId;
                await ScheduleService.CreateScheduleServiceAsync(info);
            }
            return string.Empty;
        }

        #endregion

    }
}