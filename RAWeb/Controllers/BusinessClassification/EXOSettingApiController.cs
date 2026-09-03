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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.BusinessClassification
{
    [RMApiAuthorize(RMPermissionMasks.EXOEnduser, preferred: false)]
    public class EXOSettingApiController : BaseApiController
    {
        #region interface
        private ISPSettingTreeService _SPSettingTreeService;
        private IRMSharePointSettingsService _RMSPSettingsService;


        private ITaxonomyService _TaxonomyService;
        private IScheduleService _ScheduleService;



        private IRMJobService _RMJobService;
        private IManualProcessManagementService _ManualProcessManagementService;
        private IRuleManagerService _RuleManagerService;

        private IRMSecurityTrimmingHelper _SecurityTrimmingHelper;

        private ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService(ref _SPSettingTreeService);
        private IRMSharePointSettingsService RMSPSettingsService => PlatformWindsorManager.GetService(ref _RMSPSettingsService);

        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);

        private IRMJobService RMJobService => PlatformWindsorManager.GetService(ref _RMJobService);
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService(ref _ManualProcessManagementService);
        private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService(ref _RuleManagerService);

        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService(ref _SecurityTrimmingHelper);

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        #endregion

        #region Browse
        [HttpGet]
        public RMSampleEXOTreeNode GetEXORootNode()
        {
            RMSampleEXOTreeNode exchangeRoot = null;
            try
            {
                exchangeRoot = SPSettingTreeService.LoadExchangeRoot()[0];
                if (exchangeRoot == null || exchangeRoot.Id.Equals(System.Guid.Empty))
                {
                    Logger.Warn("exchage farm node is null.Please refresh page.");
                }
                else
                {
                    if (exchangeRoot.Children != null)
                    {
                        //删除Children属性，避免以后convert to SPTree时出现死循环
                        exchangeRoot.Children = null;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when get exo root node.Error:{0}", e.ToString());
            }
            return exchangeRoot;
        }

        [HttpPost]
        [ValidEXOTreeParameterFilter]
        public async Task<string> BrowseExchange([FromBody] RMSampleEXOTreeNode node)
        {
            string result = string.Empty;
            RMSampleEXOTreeNode curRMNode = null;
            try
            {
                List<RMSampleEXOTreeNode> children = new List<RMSampleEXOTreeNode>();
                curRMNode = node;
                children = (await SPSettingTreeService.BrowseSampleExchangeTreeAsync(curRMNode, true)).OrderBy(a => a.Name).ToList();
                await RMSPSettingsService.LoadExchangeSettingIconAsync(children);
                result = JsonConvert.SerializeObject(children);
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when browe exchange node.NodeName:[{0}] Error:{1}", curRMNode?.Name, e.ToString());
                throw;
            }
            return result;
        }

        #endregion

        #region Load & Save Node Settings
        [HttpPost]
        [ValidEXOTreeParameterFilter]
        public async Task<string> LoadExchangeNodeSetting([FromBody] RMSampleEXOTreeNode node)
        {
            var settings = await RMSPSettingsService.LoadExchangeNodeSettingAsync(node);
            if (settings.ApprovalType == (int)ApprovalType.ApprovalProcess)
            {
                var result = Guid.TryParse(settings.WorkflowReferenceId, out var referenceId);
                if (result)
                {
                    var workflow = ManualProcessManagementService.GetWorkflow(referenceId);
                    settings.WorkflowReferenceName = workflow?.Name;
                }
            }
            return JsonConvert.SerializeObject(settings);
        }

        [HttpPost]
        [ValidEXOParameterActionFilter]
        public async Task<string> SaveEXOLoactionOwners([FromBody] RMEXOTreeNode curSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                var syncUserResult = await RMSPSettingsService.SyncADUsersAsync(curSetting.RecordOwner);
                if (syncUserResult.MessageType == RAMessageType.Successful)
                {
                    result = await RMSPSettingsService.AddEXOLocationOwnersAsync(curSetting);
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
                Logger.Error("Save EXO Settings Failed.ERROR:{0}", ex.Message);
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidEXOParameterActionFilter("ValidateSaveGroupEXOTermSetting")]
        public async Task<string> SaveGroupEXOTermSetting([FromBody] RMEXOTreeNode curSetting)
        {
            var result = await RMSPSettingsService.SaveEXONodeSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidEXOParameterActionFilter]
        public async Task<string> SaveCustomEXOTermSetting([FromBody] RMEXOTreeNode curSetting)
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
                    result = await RMSPSettingsService.SaveEXONodeSettingAsync(curSetting);
                }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                Logger.Error("Save EXO Settings Failed.ERROR:{0}", ex.Message);
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidEXOParameterActionFilter]
        public async Task<string> SaveEnableEXOManagementSetting([FromBody] RMEXOTreeNode curSetting)
        {
            var result = await RMSPSettingsService.AddEnableColumnSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidEXOParameterActionFilter]
        public async Task<string> SaveIsSyncDataEXOSetting([FromBody] RMEXOTreeNode curSetting)
        {
            var result = await RMSPSettingsService.AddIsSyncEXOSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidEXOParameterActionFilter]
        public async Task<string> InheritParentEXOSettings([FromBody] RMEXOTreeNode curSetting)
        {
            var result = SaveSPSettingResult.Sucess;
            try
            {
                //AddParentProperty(inheritSettings.allRMSPTreeNode);
                await RMSPSettingsService.InheritParentEXOSettingAsync(curSetting);
            }
            catch (Exception ex)
            {
                Logger.Error("Inherit GlobalSettings Failed.ERROR:{0}", ex.ToString());
                result = SaveSPSettingResult.Failed;
            }
            return result.ToString();
        }


        [HttpPost]
        [ValidEXOParameterActionFilter]
        public async Task<string> SaveGeneralSetting([FromBody] RMEXOTreeNode curSetting)
        {
            var result = await RMSPSettingsService.AddEXOGeneralSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        #endregion

        #region Dispose Schedule
        [HttpPost]
        [ValidEXOParameterActionFilter]
        public async Task<string> UpdateEXODisposeSchedule([FromBody] RMEXOTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (RMSPSettingsService.CheckEXONodeDisable(nodeSetting))
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
                    //else
                    //{
                    //    mRMSPSettingsService.AddEXONodeSettingDisposeSchedule(nodeSetting);
                    //}
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
                Logger.Error("Update Dispose Schedule Service Failed.ERROR:{0}", ex.Message);
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidEXOParameterActionFilter]
        public async Task<string> CreateEXODisposeSchedule([FromBody] RMEXOTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (RMSPSettingsService.CheckEXONodeDisable(nodeSetting))
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
                    //else
                    //{
                    //    nodeSetting.DisposeScheduleInfo.Id = schedule;//Create Schedule Contains Update
                    //    result = mRMSPSettingsService.AddEXONodeSettingDisposeSchedule(nodeSetting);
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
                Logger.Error("Update Dispose Schedule Service Failed.ERROR:{0}", ex.Message);
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidEXOParameterActionFilter]
        public string DeleteEXODisposeSchedule([FromBody] RMEXOTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (RMSPSettingsService.CheckEXONodeDisable(nodeSetting))
                {
                    ScheduleService.DeleteScheduleService(nodeSetting.DisposeScheduleInfo.Id, GetNodeFullPath(nodeSetting));
                    //mRMSPSettingsService.AddEXONodeSettingDisposeSchedule(nodeSetting, true);
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
                Logger.Error("Delete Collection Schedule Service Failed.ERROR:{0}", ex.Message);
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidEXOParameterActionFilter]
        public string BreakEXODisposeSchedule([FromBody] RMEXOTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (RMSPSettingsService.CheckEXONodeDisable(nodeSetting))
                {
                    nodeSetting.DisposeScheduleInfo.Id = "";
                    ScheduleService.CreateNoSchedule(SettingScheduleType.Dispose, GetNodeFullPath(nodeSetting));
                    //mRMSPSettingsService.AddEXONodeSettingDisposeSchedule(nodeSetting);
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
                Logger.Error("Break Collection Schedule Service Failed.ERROR:{0}", ex.Message);
            }
            return JsonConvert.SerializeObject(result);
        }

        #endregion

        #region Run Job
        [HttpGet]
        public bool CheckRunningEXOSettingJob()
        {
            return RMSPSettingsService.CheckRunningEXOSettingJob();
        }

        [HttpPost]
        public string ApplyEXOSettings([FromBody] RunApplySettingjobParam dto)
        {
            if (!RMSPSettingsService.ExistConfiguredSettings(JobType.EXOApplySetting))
            {
                RAReturnMessage msg = new RAReturnMessage();
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_ApplySetting_NoSettings");
                return JsonConvert.SerializeObject(msg);
            }
            return JsonConvert.SerializeObject(RMSPSettingsService.ApplyEXOSettings(JobRunBy.Control, dto.FromTimerJobPage));
        }

        [HttpPost]
        [ValidEXOParameterActionFilter]
        public async Task<RAReturnMessage> RunEXOJob([FromBody] RMEXOTreeNode selectedNode)
        {
            //RMFSTreeNode selectedNode = null;
            try
            {
                //selectedNode = SerializerHelper.DeserializeByJsonConvert<RMFSTreeNode>(node);
                //var selectedNode = SPTreeCacheUtil.GetNodeById(spObjectId, RAModule.Common);
                if (TenantService.IsNewOpusTenant())
                {
                    return RMSPSettingsService.RunEXORecordsDisposalJob(selectedNode, JobRunBy.Control);
                }
                else
                {
                    return await RMJobService.RunEXONowAsync(selectedNode, JobRunBy.Control);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to run job. Node Id:[{0}] Error:{1}", selectedNode?.Id, e.ToString());
                throw;
            }
        }

        [HttpPost]
        [ValidEXOParameterActionFilter]
        public string RunEXOCollectionJob([FromBody] RMEXOTreeNode selectedTree)
        {            
            return JsonConvert.SerializeObject(RMSPSettingsService.RunEXODataSyncJob(selectedTree, JobRunBy.Control));
        }
        
        [HttpPost]
        public string RunEXOSyncDataJob([FromBody] bool fromTimerJobPage)
        {          
            return JsonConvert.SerializeObject(RMSPSettingsService.RunEXODataSyncJob(null, JobRunBy.Control));
        }      
        #endregion

        #region Term

        [HttpPost]
        public Task<string> GetEXOSubTerm([FromBody] FSTreePage tree)
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

        [HttpPost]
        [ValidCurrentEXOSettingsParameterFilter]
        public Task<string> GetEXOSavedTree([FromBody] CurrentSettingsInfo settingInfo)
        {
            using (RA.Common.PerformanceScope scope = new RA.Common.PerformanceScope("GetEXOSettingSavedTree"))
            {
                return TaxonomyService.GetEXOSettingSavedTreeAsync(settingInfo, true);
            }
        }
        #endregion

        #region Tool Method
        public string GetNodeFullPath([FromBody] RMEXOTreeNode node)
        {
            if (node.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                if (node.Name == "Default_ Mailbox_ Group")
                {
                    return I18NEntity.GetString("RM_EXO_Default_Container");
                }
                return node.Name;
            }
            else
            {
                return node.Name;
            }
        }
        #endregion

        [HttpPost]
        [ValidPermissionFilter(RMPermissionMasks.RuleManagementEnduser)]
        public async Task<List<RMRuleInfos>> GetAvailableRuleList([FromBody]string containerId)
        {
            List<RMRuleInfos> listRuleFromDA = new List<RMRuleInfos>();
            List<RMRuleInfos> availableRules = new List<RMRuleInfos>();
            try
            {
                Logger.Info("Get Rules from DA ");
                using (PerformanceScope scope = new PerformanceScope("setting rules"))
                {
                    var securityGroupIds = SecurityTrimmingHelper.GetSecurityGroupsByContentScope(new List<string> { containerId }, SourceFlag.Exchange);
                    var ruleContainerIds = SecurityTrimmingHelper.GetRuleScopeBySecurityGroupIds(securityGroupIds);
                    listRuleFromDA = await RuleManagerService.GetExchangeRulesAsync(ruleContainerIds);
                    var associateAvailableRule = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync(ruleContainerIds);
                    var availableRuleIds = associateAvailableRule.Select(r => r.RuleId).ToList();
                    availableRules = listRuleFromDA.Where(r => availableRuleIds.Contains(r.RuleId)).ToList();
                }
                Logger.Info("Rule count {0}", listRuleFromDA.Count);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while get rules:{0}", ex.ToString());
            }

            return availableRules;
        }
        #region Custom index metadata

        [HttpPost]
        [ValidCustomMetadataParameterActionFilter("SaveOrUpdateCustomColumns")]
        public Task<RAReturnMessage> SaveCustomMetadataColumns([FromBody] List<CustomMetadataColumnInfo> customIndexMetadatas)
        {
            return RMSPSettingsService.AddOrUpdateCustomMetadataColumnAsync(customIndexMetadatas);
        }

        [HttpGet]
        public Task<List<CustomMetadataColumnInfo>> GetCustomMetadataColumns()
        {
            return RMSPSettingsService.GetAllCustomMetadataColumnInfoAsync();
        }

        [HttpGet]
        public Task<List<CustomMetadataColumnInfo>> GetInUsedCustomMetadataColumns()
        {
            return RMSPSettingsService.GetInUsedCustomMetadataColumnInfoAsync();
        }

        [HttpPost]
        [ValidCustomMetadataParameterActionFilter("SaveOrUpdateCustomMetadatas")]
        public Task<RAReturnMessage> SaveCustomIndexMetadatas([FromBody] CustomIndexMetadataInfo customIndexMetadatas)
        {
            return RMSPSettingsService.AddOrUpdateCustomIndexMetadataAsync(customIndexMetadatas, SourceFlag.Exchange);
        }

        [HttpGet]
        public Task<CustomIndexMetadataInfo> GetCustomIndexMetadatas()
        {
            return RMSPSettingsService.GetAllCustomIndexMetadataAsync();
        }

        [HttpGet]
        public Task<CustomIndexMetadataInfo> GetCustomIndexMetadatasBySourceFlag()
        {
            return RMSPSettingsService.GetCustomIndexMetadatasBySourceFlagAsync(SourceFlag.Exchange);
        }

        #endregion
    }
}