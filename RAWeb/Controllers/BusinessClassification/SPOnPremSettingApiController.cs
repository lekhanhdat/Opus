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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.SharePoint;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using System.Threading.Tasks;
using System.IO;
using AvePoint.GCommon;
using System.Linq;
using System.Text.RegularExpressions;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using System.Net;
using Microsoft.AspNetCore.StaticFiles;
using AvePoint.RA.DB.Dao;

namespace AvePoint.RA.Web.Controllers.BusinessClassification
{
    [RMApiAuthorize(RMPermissionMasks.SPOnPremEnduser, preferred: false)]
    public class SPOnPremSettingApiController : BaseApiController
    {

        private IRMSharePointOnPremScanNodeService _SharePointOnPremScanNodeService;
        private IRMSharePointOnPremScanNodeService SharePointOnPremScanNodeService => PlatformWindsorManager.GetService(ref _SharePointOnPremScanNodeService);
        private IRMSharePointSettingsService _RMSPSService;
        private IRMSharePointSettingsService RMSPSService => PlatformWindsorManager.GetService(ref _RMSPSService);
        private IRMSharePointOnPremSettingsService _RMSharePointOnPremSettingsService;
        private IRMSharePointOnPremSettingsService RMSharePointOnPremSettingsService => PlatformWindsorManager.GetService(ref _RMSharePointOnPremSettingsService);
        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private IScheduleService _ScheduleService;
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);




        private IManualProcessManagementService _ManualProcessManagementService;
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService(ref _ManualProcessManagementService);
        private IUniqueIdSettingService _UniqueIdSettingService;
        private IUniqueIdSettingService UniqueIdSettingService => PlatformWindsorManager.GetService(ref _UniqueIdSettingService);
        private IRMKeyValueDao _RMKeyValueDao;
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService(ref _RMKeyValueDao);

        private const string LinkPhysicalRecordsWithinSPPKey = "LinkPhysicalRecordsWithinSPP";


        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyAdmin)]
        public RAReturnMessage RunScanLocalNodeJob([FromBody] bool fromTimerJobPage)
        {
            var id = SharePointOnPremScanNodeService.RunScheduleJob(JobRunBy.Control);
            if (string.IsNullOrEmpty(id))
            {
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }
            return new RAReturnMessage();
        }

        #region Load & Save Node Settings

        [HttpPost]
        public async Task<string> LoadSampleNodeSettings([FromBody] RMSPSampleTreeNode node)
        {
            var settings = await RMSharePointOnPremSettingsService.LoadSampleNodeSettingsAsync(node);
            if (settings.ApprovalType == (int)ApprovalType.ApprovalProcess)
            {
                var workflow = ManualProcessManagementService.GetWorkflow(new Guid(settings.WorkflowReferenceId));
                settings.WorkflowReferenceName = workflow?.Name;
            }
            return JsonConvert.SerializeObject(settings);
        }

        [HttpPost]
        public async Task<string> SaveEnableColumnSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSharePointOnPremSettingsService.AddEnableColumnSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> SaveColumnSettingExistColumn([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSharePointOnPremSettingsService.AddUsingExistColumnSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> SaveColumnSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSharePointOnPremSettingsService.AddColumnSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> SaveIsSyncDataSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSharePointOnPremSettingsService.AddIsSyncSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> SaveGroupLevelSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSharePointOnPremSettingsService.AddGlobalColumnAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> SaveDocumentLevelSetting([FromBody] RMSPTreeNode curSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                if (!curSetting.DefaultTermId.Equals(Guid.Empty) && TaxonomyService.IsOrphanedTerm(curSetting.DefaultTermId))
                {
                    result.FaildType = RAFailedType.DefaultTermIsOrphaned;
                    result.MessageType = RAMessageType.Failed;
                }
                else
                {
                    //(curSettings.allRMSPTreeNode);
                    result = await RMSharePointOnPremSettingsService.AddCustomColumnAsync(curSetting);
                }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                Logger.Error("Save SharePoint Settings Failed.ERROR:{0}", ex.Message);
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> SaveContainerLevelSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSharePointOnPremSettingsService.AddContainerTermAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> SaveLoactionOwners([FromBody] RMSPTreeNode curSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            var syncUserResult = await RMSPSService.SyncADUsersAsync(curSetting.RecordOwner);
            if (syncUserResult.MessageType == RAMessageType.Successful)
            {
                result = await RMSharePointOnPremSettingsService.AddLocationOwnersAsync(curSetting);
            }
            else
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = syncUserResult.ErrorMessage;
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> InheritParentSettings([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSharePointOnPremSettingsService.InheritParentSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> SaveGeneralSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSharePointOnPremSettingsService.AddSPOnPremGeneralSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        public Task<bool> CheckHasAvailableAgent()
        {
            return RMSharePointOnPremSettingsService.CheckHasAvailableAgentAsync();
        }

        [HttpPost]
        // [AllowAnonymous]
        public async Task<string> SaveApiAuth([FromBody] SPOnPremApiAuthInfo apiAuthInfo)
        {
            var result = new RAReturnMessage();

            if (apiAuthInfo == null)
            {
                result.MessageType = RAMessageType.Failed;
                result.FaildType = RAFailedType.None;
                result.ErrorMessage = "apiAuthInfo is required.";
                Logger.Warn("Save SP On-Prem API auth info failed. ERROR:{0}", result.ErrorMessage);
                return JsonConvert.SerializeObject(result);
            }
            if (apiAuthInfo.IsLink == true)
            {
                apiAuthInfo.ClientId = apiAuthInfo.ClientId?.Trim();
                apiAuthInfo.ThumbPrint = apiAuthInfo.ThumbPrint?.Trim();
                if (string.IsNullOrWhiteSpace(apiAuthInfo.ClientId))
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.None;
                    result.ErrorMessage = "clientId is required.";
                    Logger.Warn("Save SP On-Prem API auth info failed. ERROR:{0}", result.ErrorMessage);
                    return JsonConvert.SerializeObject(result);
                }

                if (string.IsNullOrWhiteSpace(apiAuthInfo.ThumbPrint))
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.None;
                    result.ErrorMessage = "thumbPrint is required.";
                    Logger.Warn("Save SP On-Prem API auth info failed. ERROR:{0}", result.ErrorMessage);
                    return JsonConvert.SerializeObject(result);
                }

                if (!Regex.IsMatch(apiAuthInfo.ClientId, "^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"))
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.None;
                    result.ErrorMessage = "clientId must be a 36-character lowercase UUID in xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx format.";
                    Logger.Warn("Save SP On-Prem API auth info failed. ERROR:{0}", result.ErrorMessage);
                    return JsonConvert.SerializeObject(result);
                }

                if (!Regex.IsMatch(apiAuthInfo.ThumbPrint, "^[0-9A-Fa-f]{40}$"))
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.None;
                    result.ErrorMessage = "thumbPrint must be exactly 40 hexadecimal characters with no separators.";
                    Logger.Warn("Save SP On-Prem API auth info failed. ERROR:{0}", result.ErrorMessage);
                    return JsonConvert.SerializeObject(result);
                }
            }

            try
            {
                var value = JsonConvert.SerializeObject(apiAuthInfo);
                await RMKeyValueDao.UpsertAsync(LinkPhysicalRecordsWithinSPPKey, value);

                result.MessageType = RAMessageType.Successful;
            }
            catch (Exception ex)
            {
                var correlationId = Guid.NewGuid().ToString("N");
                result.MessageType = RAMessageType.Failed;
                result.FaildType = RAFailedType.None;
                result.ErrorMessage = "Unexpected error occurred; please contact support.";
                result.Extension = correlationId;
                Logger.Error("Save SP On-Prem API auth info failed. CorrelationId:{0}. ERROR:{1}", correlationId, ex.ToString());
            }

            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        public string GetApiAuthInfo()
        {
            var authInfo = new SPOnPremApiAuthInfo();
            try
            {
                var value = RMKeyValueDao.GetValueByKey(LinkPhysicalRecordsWithinSPPKey)?.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    authInfo = JsonConvert.DeserializeObject<SPOnPremApiAuthInfo>(value) ?? new SPOnPremApiAuthInfo();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get SP On-Prem API auth info failed. ERROR:{0}", ex.Message);
            }

            return JsonConvert.SerializeObject(authInfo);
        }
        #endregion

        #region Run Job

        [HttpPost]
        public async Task<string> ApplySettings([FromBody] RunApplySettingjobParam dto)
        {
            try
            {
                var needRunNodes = new List<RMSPTreeNode>();
                if (UniqueIdSettingService.ValidSPOnPremUniqueIdSetting())
                {
                    //如果Online已设置过UniqueId setting，不重新编辑UniqueIdSetting的情况下，需要Run apply setting时创建Schedule
                    await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.SPOnPremUniqueIDSettingSchedule);

                    if (await RMSharePointOnPremSettingsService.NeedRunUniqueIdJobAsync(needRunNodes))
                    {

                        Logger.Debug("need run sp-onprem unique id job.");
                        var jobId = UniqueIdSettingService.RunUniqueIDSettingScheduleJob(
                            JobRunBy.Control,
                            JobType.SPOnPremUniqueIDSettingFullSchedule
                            );
                        Logger.Debug("Run sp-onprem unique id job[{0}].", jobId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Run sp-onprem unique id job error{0}.", ex.ToString());

            }
            var message = RMSharePointOnPremSettingsService.ApplySettings(JobRunBy.Control, dto.FromTimerJobPage, dto.RunJobMethod);
            return JsonConvert.SerializeObject(message);
        }

        [HttpPost]
        public async Task<string> RunSPSyncDataJob([FromBody] bool fromTimerJobPage)
        {
            var message = await RMSharePointOnPremSettingsService.RunDataSyncJobAsync(null, JobRunBy.Control);
            return JsonConvert.SerializeObject(message);
        }

        [HttpPost]
        public async Task<string> RunCollectionJob([FromBody] RMSPTreeNode selectedTree)
        {
            var message = await RMSharePointOnPremSettingsService.RunDataSyncJobAsync(selectedTree, JobRunBy.Control);
            return JsonConvert.SerializeObject(message);
        }

        [HttpPost]
        public string RunOnpremiseEnforceRuleActionJob([FromBody] string node)
        {
            RMSPTreeNode selectedNode = null;
            var message = new RAReturnMessage() { MessageType = RAMessageType.Failed };
            selectedNode = SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(node);
            Logger.Info("Run OnpremiseEnforceRuleAction job Node FullPath:[{0}].", selectedNode?.FullPath);
            try
            {
                message = RMSharePointOnPremSettingsService.RunOnpremiseEnforceRuleActionJob(selectedNode, JobRunBy.Control);
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while running onpremise enforce rule action job. Error:{1}.", e.ToString());
            }
            return JsonConvert.SerializeObject(message);
        }

        #endregion


        #region Dispose Schedule
        [HttpPost]
        public async Task<string> UpdateDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string objectId = Guid.Empty.ToString();
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    RMSPTreeNode siteCollectionNode = RMSharePointOnPremSettingsService.GetSiteCollectionNode(nodeSetting);
                    objectId = siteCollectionNode.SPObjectId;
                }
                if (!RMSharePointOnPremSettingsService.CheckParentNodeDisable(nodeSetting, objectId))
                {

                    var cloneNodeInfo = nodeSetting.Clone();
                    cloneNodeInfo.DisposeScheduleInfo = null;
                    cloneNodeInfo.SkipRemoveContentAndDestroyAction = nodeSetting.DisposeScheduleInfo.Extentions.Equals("true", StringComparison.OrdinalIgnoreCase);
                    nodeSetting.DisposeScheduleInfo.Extentions = JsonConvert.SerializeObject(cloneNodeInfo);
                    var schedule = await ScheduleService.UpdateScheduleServiceAsync(nodeSetting.DisposeScheduleInfo, GetNodeFullPath(nodeSetting));
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
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
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
        public async Task<string> CreateDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string objectId = Guid.Empty.ToString();
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    RMSPTreeNode siteCollectionNode = RMSharePointOnPremSettingsService.GetSiteCollectionNode(nodeSetting);
                    objectId = siteCollectionNode.SPObjectId;
                }
                if (!RMSharePointOnPremSettingsService.CheckParentNodeDisable(nodeSetting, objectId))
                {
                    nodeSetting.DisposeScheduleInfo.Id = Guid.NewGuid().ToString();
                    var cloneNodeInfo = nodeSetting.Clone();
                    cloneNodeInfo.DisposeScheduleInfo = null;
                    cloneNodeInfo.SkipRemoveContentAndDestroyAction = nodeSetting.DisposeScheduleInfo.Extentions.Equals("true", StringComparison.OrdinalIgnoreCase);
                    nodeSetting.DisposeScheduleInfo.Extentions = JsonConvert.SerializeObject(cloneNodeInfo);
                    nodeSetting.DisposeScheduleInfo.ProfileId = ScheduleService.GetProfileId(nodeSetting);
                    var schedule = await ScheduleService.CreateScheduleServiceAsync(nodeSetting.DisposeScheduleInfo, true, GetNodeFullPath(nodeSetting));
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
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
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
        public string DeleteDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string objectId = Guid.Empty.ToString();
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    RMSPTreeNode siteCollectionNode = RMSharePointOnPremSettingsService.GetSiteCollectionNode(nodeSetting);
                    objectId = siteCollectionNode.SPObjectId;
                }
                if (!RMSharePointOnPremSettingsService.CheckParentNodeDisable(nodeSetting, objectId))
                {
                    ScheduleService.DeleteScheduleService(nodeSetting.DisposeScheduleInfo.Id, GetNodeFullPath(nodeSetting));
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
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
        public string BreakDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string objectId = Guid.Empty.ToString();
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    RMSPTreeNode siteCollectionNode = RMSharePointOnPremSettingsService.GetSiteCollectionNode(nodeSetting);
                    objectId = siteCollectionNode.SPObjectId;
                }
                if (!RMSharePointOnPremSettingsService.CheckParentNodeDisable(nodeSetting, objectId))
                {
                    nodeSetting.DisposeScheduleInfo.Id = "";
                    ScheduleService.CreateNoSchedule(SettingScheduleType.Dispose, GetNodeFullPath(nodeSetting));
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
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

        #endregion

        #region Tool Method

        public string GetNodeFullPath([FromBody] RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.WebApplication)
            {
                return node.FullPath;
            }
            return WebUtil.MakeFullUrl(node.GetSiteCollectionNode().FullPath, node.FullPath);
        }

        #endregion

        [HttpPost]
        public Task<string> GetSavedTree([FromBody] CurrentSettingsInfo settingInfo)
        {
            return TaxonomyService.GetSPOnPremSettingSavedTreeAsync(settingInfo, true);
        }


        public IActionResult DownloadRelatedApp()
        {
            try
            {
                var appFolderPath = "Config";
                var exportFolderPath = I18NEntity.GetString("RM_SPS_RelatedRecords_Configurations");
                var appFileName = "related-records-spse-app.sppkg";
                var formFileName = "AvePointRelatedRecordsSPSESolution.wsp";
                var appFilePath = Path.Combine(WebUtil.GetInstallPath(), appFolderPath, appFileName);
                var formFilePath = Path.Combine(WebUtil.GetInstallPath(), appFolderPath, formFileName);
                var tempBaseFolder = Path.Combine(WebUtil.GetInstallPath(), "Temp", exportFolderPath);
                CreateDirectory(tempBaseFolder);
                var copyAppFilePath = Path.Combine(tempBaseFolder, appFileName);
                System.IO.File.Copy(appFilePath, copyAppFilePath, true);
                RebuildRelatedAppConfig(tempBaseFolder);
                var copyFormFilePath = Path.Combine(tempBaseFolder, formFileName);
                System.IO.File.Copy(formFilePath, copyFormFilePath, true);

                if (System.IO.File.Exists(tempBaseFolder + ".zip"))
                {
                    System.IO.File.Delete(tempBaseFolder + ".zip");
                }

                ZipUtil.ZipFolder(tempBaseFolder, tempBaseFolder + ".zip");
                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(tempBaseFolder + ".zip", FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                return File(memoryStream, GetContentType(tempBaseFolder + ".zip"), Path.GetFileName(tempBaseFolder + ".zip"));
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }

        private void RebuildRelatedAppConfig(string appPackagePath)
        {
            if (string.IsNullOrWhiteSpace(appPackagePath) || !Directory.Exists(appPackagePath))
            {
                return;
            }

            var innerAppPackagePath = Path.Combine(appPackagePath, "related-records-spse-app.sppkg");
            if (!System.IO.File.Exists(innerAppPackagePath))
            {
                return;
            }
            var authInfo = GetSavedApiAuthInfo();
            if (authInfo == null || string.IsNullOrWhiteSpace(authInfo.ClientId) || string.IsNullOrWhiteSpace(authInfo.ThumbPrint))
            {
                return;
            }

            var tempUnzipFolder = Path.Combine(appPackagePath, "related-records-spse-app");
            CreateDirectory(tempUnzipFolder);
            ZipUtil.UnZipFile(innerAppPackagePath, tempUnzipFolder);

            var replacements = new Dictionary<string, string>
            {
                { "opusIdentityUrl_7c4921d8-90ef-427b-a65c-891df32750e6", RMGlobalConfiguration.AppConfig[RMAppSettingKey.PUBLIC_IDENTITY_SERVICE_URL] },
                { "opusWebApiUrl_7c4921d8-90ef-427b-a65c-891df32750e6", RMGlobalConfiguration.AppConfig[RMAppSettingKey.PUBLIC_RECO_API_URL] },
                { "clientId_7c4921d8-90ef-427b-a65c-891df32750e6", authInfo.ClientId },
                { "thumbPrint_7c4921d8-90ef-427b-a65c-891df32750e6", authInfo.ThumbPrint },
                { "tenantId_7c4921d8-90ef-427b-a65c-891df32750e6", TenantLocalValue.LogonGroupId }
            };

            var clientSideAssetsFiles = Directory.GetFiles(Path.Combine(tempUnzipFolder, "ClientSideAssets"), "*.js", SearchOption.AllDirectories);
            foreach (var clientSideAssetsFilePath in clientSideAssetsFiles)
            {
                var fileContent = System.IO.File.ReadAllText(clientSideAssetsFilePath);
                var modifiedContent = fileContent;

                foreach (var replaceItem in replacements)
                {
                    modifiedContent = ReplaceAppConfigValue(modifiedContent, replaceItem.Key, replaceItem.Value ?? string.Empty);
                }

                if (!string.Equals(fileContent, modifiedContent, StringComparison.Ordinal))
                {
                    System.IO.File.WriteAllText(clientSideAssetsFilePath, modifiedContent);
                }
            }

            if (System.IO.File.Exists(innerAppPackagePath))
            {
                System.IO.File.Delete(innerAppPackagePath);
            }

            ZipUtil.ZipFolder(tempUnzipFolder, innerAppPackagePath);
            Directory.Delete(tempUnzipFolder, true);
        }

        private static string ReplaceAppConfigValue(string fileContent, string replaceKey, string replaceValue)
        {
            if (string.IsNullOrEmpty(fileContent) || string.IsNullOrEmpty(replaceKey))
            {
                return fileContent;
            }

            Regex replaceConfigRegex = new($"{Regex.Escape(replaceKey)}\\s?:\\s?\\\".*?\\\"", RegexOptions.None, TimeSpan.FromMinutes(3));
            if (replaceConfigRegex.IsMatch(fileContent))
            {
                var configSetting = replaceConfigRegex.Match(fileContent).Value;
                return fileContent.Replace(configSetting, $"{replaceKey}:\"{replaceValue}\"");
            }

            return fileContent.Replace(replaceKey, replaceValue);
        }

        private SPOnPremApiAuthInfo GetSavedApiAuthInfo()
        {
            var authInfo = new SPOnPremApiAuthInfo();
            try
            {
                var value = RMKeyValueDao.GetValueByKey(LinkPhysicalRecordsWithinSPPKey)?.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    authInfo = JsonConvert.DeserializeObject<SPOnPremApiAuthInfo>(value) ?? new SPOnPremApiAuthInfo();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get saved SP On-Prem API auth info failed. ERROR:{0}", ex.Message);
            }

            return authInfo;
        }

        private static void CreateDirectory(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, true);
            }
            CreateDirectoryIfNotExist(filePath);
        }

        private static void CreateDirectoryIfNotExist(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }
        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        public class SPOnPremApiAuthInfo
        {
            [JsonProperty("isLink")]
            public bool IsLink { get; set; }

            [JsonProperty("clientId")]
            public string ClientId { get; set; }

            [JsonProperty("thumbPrint")]
            public string ThumbPrint { get; set; }
        }
    }
}