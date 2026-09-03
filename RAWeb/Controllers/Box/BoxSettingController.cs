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
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Connections;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RMFileSystemSettings;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvePoint.RA.Web.Controllers.Box
{
    [RMApiAuthorize(RMPermissionExtensionMasks.BoxAdmin, preferred: false)]
    public class BoxSettingController : BaseApiController
    {
        public IRMBoxSettingsService _RMBoxSettingsService;
        public IRMBoxSettingsService RMBoxSettingsService => PlatformWindsorManager.GetService(ref _RMBoxSettingsService);
        public ITaxonomyService _TaxonomyService;
        public ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private IManualProcessManagementService _ManualProcessManagementService;
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService(ref _ManualProcessManagementService);
        private IScheduleService _ScheduleService;
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);
        private IRMBoxBrowser _RMBoxBrowser;
        private IRMBoxBrowser RMBoxBrowser => PlatformWindsorManager.GetService(ref _RMBoxBrowser);

        [HttpPost]
        public bool RunCollectionJob([FromBody] BoxTreeNode selectedTree)
        {
            return RMBoxSettingsService.EnqueueDataSyncJob(selectedTree);
        }

        [HttpPost]
        public RAReturnMessage RunJob([FromBody] string node)
        {
            BoxSettingDto boxSetting = null;
            try
            {
                boxSetting = JsonConvert.DeserializeObject<BoxSettingDto>(node);
                return RMBoxSettingsService.EnqueueRunRecordsDisposalJob(boxSetting.SelectedNode);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to run job. NodeId:[{0}] Error:{1}", boxSetting?.SelectedNode.Id, e.ToString());
                throw;

            }
        }

        [HttpPost]
        public RAReturnMessage RunDataSyncScheduleJob()
        {
            RMBoxSettingsService.EnqueueDataSyncScheduleJob(true);
            return new RAReturnMessage();
        }

        #region Load & Save Node Settings
        [HttpPost]
        public async Task<BoxSettingDto> LoadBoxNodeSetting([FromBody] BoxTreeNode node)
        {
            var settings = await RMBoxSettingsService.LoadNodeSettingAsync(node);
            if (settings.ApprovalType == (int)ApprovalType.ApprovalProcess)
            {
                var workflow = ManualProcessManagementService.GetWorkflow(new Guid(settings.WorkflowReferenceId));
                settings.WorkflowReferenceName = workflow?.Name;
            }
            return settings;
        }

        [HttpPost]
        public async Task<ConnectionResponse> SaveSettings([FromBody] BoxSettingDto dto)
        {
            try
            {
                await RMBoxSettingsService.SaveNodeSettingAsync(dto);
                return ConnectionResponse.Succeeded();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save box setting. Error: {ex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, "Unknown error");
            }
        }

        [HttpPost]
        public async Task<ConnectionResponse> SaveBoxLocationOwners([FromBody] BoxSettingDto dto)
        {
            try
            {
                var syncUserResult = await RMBoxSettingsService.SyncADUsersAsync(dto.RecordOwner);
                if (!syncUserResult)
                    return ConnectionResponse.Failed(ConnectionResponseErrorType.ValidationError, I18NEntity.GetString("RM_RegisterUser_Error_Message"));
                await RMBoxSettingsService.SaveNodeSettingAsync(dto);
                return ConnectionResponse.Succeeded();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save box setting. Error: {ex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, "Unknown error");
            }
        }

        [HttpPost]
        public async Task<ConnectionResponse> InheritParentSettingAsync([FromBody] BoxTreeNode node)
        {
            try
            {
                await RMBoxSettingsService.InheritParentSettingAsync(node);
                return ConnectionResponse.Succeeded();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to inherit parent setting. Error: {ex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, "Unknown error");
            }
        }

        [HttpPost]
        public async Task<ConnectionResponse> SaveBoxActiveSetting([FromBody] BoxSettingDto dto)
        {
            try
            {
                await RMBoxSettingsService.SaveActiveSettingAsync(dto);
                return ConnectionResponse.Succeeded();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save box active setting. Error: {ex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, "Unknown error");
            }
        }
        #endregion

        #region Term

        [HttpPost]
        public Task<string> GetBoxSavedTerm([FromBody] CurrentSettingsInfo settingInfo)
        {
            return TaxonomyService.GetBoxSavedTermAsync(settingInfo, true);
        }
        #endregion

        [HttpPost]
        public string BreakBoxDisposeSchedule([FromBody] BoxSettingDto nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                if (nodeSetting.DisposeScheduleInfo != null)
                {
                    nodeSetting.DisposeScheduleInfo.Id = "";
                    ScheduleService.CreateNoSchedule(SettingScheduleType.Dispose, nodeSetting.SelectedNode.FullPath);
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                }
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

        [HttpPost]
        public async Task<string> CreateBoxDisposeSchedule([FromBody] BoxSettingDto nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                if (nodeSetting.DisposeScheduleInfo != null)
                {
                    nodeSetting.DisposeScheduleInfo.Id = Guid.NewGuid().ToString();
                    var cloneNodeInfo = nodeSetting.Clone();
                    cloneNodeInfo.DisposeScheduleInfo = null;
                    nodeSetting.DisposeScheduleInfo.Extentions = JsonConvert.SerializeObject(cloneNodeInfo);
                    nodeSetting.DisposeScheduleInfo.ProfileId = ScheduleService.GetProfileId(nodeSetting.SelectedNode);
                    var schedule = await ScheduleService.CreateScheduleServiceAsync(nodeSetting.DisposeScheduleInfo, true, nodeSetting.SelectedNode.FullPath);
                    if (schedule == "-1")
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.ScheduleServiceFailed;
                    }
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
        public async Task<string> UpdateBoxDisposeSchedule([FromBody] BoxSettingDto nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                if (nodeSetting.DisposeScheduleInfo != null)
                {
                    var cloneNodeInfo = nodeSetting.Clone();
                    cloneNodeInfo.DisposeScheduleInfo = null;
                    nodeSetting.DisposeScheduleInfo.Extentions = JsonConvert.SerializeObject(cloneNodeInfo);
                    var schedule = await ScheduleService.UpdateScheduleServiceAsync(nodeSetting.DisposeScheduleInfo, nodeSetting.SelectedNode.FullPath);
                    if (schedule == "-1")
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.ScheduleServiceFailed;
                    }
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                Logger.Error("Update Dispose Schedule Service Failed.ERROR:{0}", ex.Message);
                throw;
            }
            return JsonConvert.SerializeObject(result); ;
        }

        [HttpPost]
        public string DeleteBoxDisposeSchedule([FromBody] BoxSettingDto nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                if (nodeSetting.DisposeScheduleInfo != null)
                {
                    ScheduleService.DeleteScheduleService(nodeSetting.DisposeScheduleInfo.Id, nodeSetting.SelectedNode.FullPath);
                }
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
        public string GetBoxTreeInitData()
        {
            var boxRoot = RMBoxBrowser.GetRootNode();
            if (boxRoot == null || string.IsNullOrEmpty(boxRoot.Id))
            {
                Logger.Warn("Farm node is null. Please refresh the page.");
            }
            else
            {
                if (boxRoot.Children != null)
                {
                    boxRoot.Children = null;
                }
            }
            return SerializerHelper.SerializeByJsonConvert(boxRoot);
        }

    }
}
