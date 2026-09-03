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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.SourceTreeQuery.Model;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.BusinessClassification
{
    [RMApiAuthorize(RMPermissionExtensionMasks.AzureFSAdmin, preferred: false)]
    public class AzureFileSettingApiController : BaseApiController
    {
        #region Interface
        public IRMAzureFileSettingsService _RMAzureFileSettingsService;
        public IRMAzureFileSettingsService RMAzureFileSettingsService => PlatformWindsorManager.GetService(ref _RMAzureFileSettingsService);
        public IRMSharePointSettingsService _RMSPSettingsService;
        public IRMSharePointSettingsService RMSPSettingsService => PlatformWindsorManager.GetService(ref _RMSPSettingsService);
        public IScheduleService _ScheduleService;
        public IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);
        public ITaxonomyService _TaxonomyService;
        public ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        public IManualProcessManagementService _ManualProcessManagementService;
        public IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService(ref _ManualProcessManagementService);

        #endregion

        #region Load & Save Node Settings
        [HttpPost]
        public async Task<AzureFileSettingDto> LoadAzureFileNodeSetting([FromBody] AzureFileShareTreeNode node)
        {
            var settings = await RMAzureFileSettingsService.LoadNodeSettingAsync(node);
            //if (settings.ApprovalType == (int)ApprovalType.ApprovalProcess)
            //{
            //    var workflow = ManualProcessManagementService.GetWorkflow(new Guid(settings.WorkflowReferenceId));
            //    settings.WorkflowReferenceName = workflow?.Name;
            //}
            return settings;
        }

        [HttpPost]
        public async Task<string> SaveSettings([FromBody] AzureFileSettingDto dto)
        {
            var result = SaveSPSettingResult.Sucess;
            try
            {
                await RMAzureFileSettingsService.SaveNodeSettingAsync(dto);
            }
            catch (Exception ex)
            {
                result = SaveSPSettingResult.Failed;
                Logger.Error($"[Azure File]Failed to Save Azure File Settings. ERROR:{ex}");
            }
            return result.ToString();
        }

        [HttpPost]
        public string InheritParentSetting([FromBody] AzureFileShareTreeNode node)
        {
            var result = SaveSPSettingResult.Sucess;
            try
            {
                RMAzureFileSettingsService.InheritParentSetting(node);
            }
            catch (Exception ex)
            {
                Logger.Error($"[Azure File]Failed to Inherit GlobalSettings.ERROR:{ex}");
                result = SaveSPSettingResult.Failed;
            }
            return result.ToString();
        }

        [HttpPost]
        public async Task<string> SaveAzureFileActiveSetting([FromBody] AzureFileSettingDto dto)
        {
            var result = SaveSPSettingResult.Sucess;
            try
            {
                await RMAzureFileSettingsService.SaveActiveSettingAsync(dto);
            }
            catch (Exception ex)
            {
                result = SaveSPSettingResult.Failed;
                Logger.Error("[Azure File]Failed to Save Active Settings.ERROR:{0}", ex.Message);
            }
            return result.ToString();
        }
        #endregion

        #region Term
        [HttpPost]
        public Task<string> GetAzureFileSavedTerm([FromBody] CurrentSettingsInfo settingInfo)
        {
            return TaxonomyService.GetAzureFileSavedTermAsync(settingInfo, true);
        }
        
        #endregion

        #region Run Job

        [HttpPost]
        public string RunCollectionJob([FromBody] AzureFileShareTreeNode selectedTree)
        {
            var message = RMAzureFileSettingsService.RunDataSyncJob(selectedTree);
            return JsonConvert.SerializeObject(message);
        }

        [HttpPost]
        public RAReturnMessage RunDataSyncScheduleJob()
        {
            RMAzureFileSettingsService.RunDataSyncScheduleJob();
            return new RAReturnMessage();
        }
        #endregion
    }
}