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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.Services.RMMachineLearning;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Models.ReportCenter;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.BusinessClassification
{
    [RMApiAuthorize(RMPermissionMasks.FSAdmin, RMDiscoveryFileSystemPermissionMask.AccessAll, PermissionJoinType.Any, PermissionJoinType.Any, preferred: false)]
    public class FSSettingApiController: BaseApiController
    {
        #region Interface
        private IRMFileSystemSettingsService _FileSystemSettingsService;
        private IRMFileSystemSettingsService FileSystemSettingsService => PlatformWindsorManager.GetService(ref _FileSystemSettingsService);
        private IRMFileSystemBrowserService _FileSystemBrowserService;
        private IRMFileSystemBrowserService FileSystemBrowserService => PlatformWindsorManager.GetService(ref _FileSystemBrowserService);
        private IRMSharePointSettingsService _RMSPSettingsService;
        private IRMSharePointSettingsService RMSPSettingsService => PlatformWindsorManager.GetService(ref _RMSPSettingsService);
        private IScheduleService _ScheduleService;
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);
        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private IManualProcessManagementService _ManualProcessManagementService;
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService(ref _ManualProcessManagementService);
        private IAgentMgmtService _AgentMgmtService;
        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService(ref _AgentMgmtService);
        private IRMKeyValueDao _RMKeyValueDao;
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService(ref _RMKeyValueDao);

        private IRMFileSystemRegisterService _FSRegisterService;
        private IRMFileSystemRegisterService FSRegisterService => PlatformWindsorManager.GetService(ref _FSRegisterService);
        private IFSAuditSinkService _FSAuditSinkService;
        private IFSAuditSinkService FSAuditSinkService => PlatformWindsorManager.GetService(ref _FSAuditSinkService); private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);

        #endregion

        #region Browse
        [HttpPost]
        public async Task<RMFSTreeNode> FSBrowse([FromBody]RMFSTreeNode curRMNode)
        {
            //RAReturnMessage returnMessage = new RAReturnMessage();
            List<RMFSTreeNode> children = new List<RMFSTreeNode>();
            //returnMessage.Extension = SerializerHelper.SerializeByJsonConvert(children);
            string name = string.Empty;
            try
            {
                if(AgentMgmtService.HasAgentsInUpgradingProcess())
                    throw new NotAvailableAgentException("There are agents in upgrading process, please try again later.");

                if (!await FileSystemSettingsService.CheckFullPathConnectionAsync(curRMNode))
                    throw new Exception(string.Format("Path not vaild:{0}", curRMNode.FullPath));
                
                name = curRMNode.Name;
                if (!string.IsNullOrEmpty(curRMNode.FullPath))
                {
                    ////对path解密
                    //curRMNode.FullPath = EncodeUtil.DecryptByCommunicationKey(curRMNode.FullPath);
                    //if (!TreeNodeUtil.CheckPathTraversal(curRMNode.FullPath))
                    //{
                    //    throw new Exception(string.Format("Path not vaild:{0}", curRMNode.FullPath));
                    //}
                }
                children = (await FileSystemBrowserService.FSBrowseAsync(curRMNode)).OrderBy(a => a.Name).ToList();
                curRMNode.Children = children;
                if (!curRMNode.IsSearch)
                {
                    FileSystemSettingsService.LoadFSSettingIcon(children);
                    foreach (var child in children)
                    {
                        //if (!string.IsNullOrEmpty(child.FullPath))
                        //{
                        //    //对path加密,防止Path Traversal
                        //    child.FullPath = EncodeUtil.EncryptByCommunicationKey(child.FullPath);
                        //}
                        await FileSystemSettingsService.LoadFSNodeSettingAsync(child);
                    }
                }
                else
                {
                    foreach (var child in children)
                    {
                        await FileSystemSettingsService.LoadFSNodeSettingAsync(child);
                        FileSystemSettingsService.LoadFSSettingIcon(new List<RMFSTreeNode> { child });
                    }
                }
                
               // children?.ForEach(n => n.Parent = null);
            }
            catch (NotAvailableAgentException e)
            {
                //returnMessage.MessageType = RAMessageType.Failed;
                //returnMessage.FaildType = RAFailedType.NotAvailableAgent;
                //need navi to CP Link, move this to js
                //returnMessage.ErrorMessage = I18NEntity.GetString("RM_FS_NOAvailableAgent_BrowserTree");
                Logger.Error($"no available Agent to start. NodeName:[{name}] Error:{e}");
            }
            catch (AgentProcessException e)
            {
                //returnMessage.MessageType = RAMessageType.Failed;
                //returnMessage.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_LoadTreeFailed");//need a new string -> agent process error
                Logger.Error("An error occurred when browe node.NodeName:[{0}] Error:{1}", name, e.ToString());
            }
            catch (AgentNotifyWebApiException e)
            {
                //returnMessage.MessageType = RAMessageType.Failed;
                //returnMessage.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_LoadTreeFailed");//need a new string -> agent process error
                Logger.Error("An error occurred when browe node.NodeName:[{0}] Error:{1}", name, e.ToString());
            }
            catch (Exception e)
            {
                //returnMessage.MessageType = RAMessageType.Failed;
                //returnMessage.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_LoadTreeFailed");
                Logger.Error("An error occurred when browe node.NodeName:[{0}] Error:{1}", name, e.ToString());
            }
            return curRMNode;
        }

        [HttpPost]
        public async Task<RMFSTreeNode> FSBrowseTreeWithoutSetting([FromBody] RMFSTreeNode curRMNode)
        {
            string result = string.Empty;
            string name = string.Empty;
            try
            {
                if (AgentMgmtService.HasAgentsInUpgradingProcess())
                    throw new NotAvailableAgentException("There are agents in upgrading process, please try again later.");

                if (!await FileSystemSettingsService.CheckFullPathConnectionAsync(curRMNode))
                    throw new Exception(string.Format("Path not vaild:{0}", curRMNode.FullPath));

                List<RMFSTreeNode> children = new List<RMFSTreeNode>();
                name = curRMNode.Name;
                if (!string.IsNullOrEmpty(curRMNode.FullPath))
                {
                    //对path解密
                    //curRMNode.FullPath = EncodeUtil.DecryptByCommunicationKey(curRMNode.FullPath);
                    //if (!TreeNodeUtil.CheckPathTraversal(curRMNode.FullPath))
                    //{
                    //    throw new Exception(string.Format("Path not vaild:{0}", curRMNode.FullPath));
                    //}
                }
                children = (await FileSystemBrowserService.FSBrowseAsync(curRMNode)).OrderBy(a => a.Name).ToList();
                curRMNode.Children = children;
                //mFileSystemSettingsService.LoadFSSettingIcon(children);
                foreach (var child in children)
                {
                    //mFileSystemSettingsService.LoadFSNodeSetting(child);
                }
                //result = SerializerHelper.SerializeByJsonConvert(children);
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when browe node.NodeName:[{0}] Error:{1}", name, e.ToString());
            }
            return curRMNode;
        }

        [HttpPost]
        public async Task<RMFSTreeNode> FSBrowseTreeByPager([FromBody] RMFSTreeNode curRMNode)
        {
            if (curRMNode == null) throw new ArgumentNullException(nameof(curRMNode));

            if (AgentMgmtService.HasAgentsInUpgradingProcess())
                throw new NotAvailableAgentException("There are agents in upgrading process, please try again later.");

            try
            {
                var children = (await FileSystemBrowserService.FSBrowseAsync(curRMNode)).OrderBy(x => x.Name).ToList();
                curRMNode.ChildrenCount = children.Count;
                var resultChild = children;
                if (RMCosmosDBIndependentController.IsEnabledIndependent() && (curRMNode.Level < (int)NodeLevel.WebApplication))
                {
                    resultChild = children.Skip(curRMNode.PageIndex * curRMNode.PageSize).Take(curRMNode.PageSize).ToList();
                }
                if (!RMCosmosDBIndependentController.IsEnabledIndependent() && curRMNode.Level < (int)NodeLevel.SiteCollection)
                {
                    resultChild = children.Skip(curRMNode.PageIndex * 10).Take(10).ToList();
                }
                curRMNode.Children = resultChild;
                curRMNode.ChildrenIds = resultChild.Select(x => x.Id.ToString()).ToList();
                return curRMNode;
            }
            catch (Exception ex)
            {
                Logger.Error("Browse node failed. NodeName:[{0}] Error:{1}", curRMNode.Name, ex);
                throw;
            }
        }

        [HttpGet]
        public Task<bool> CheckHasAvailableAgent()
        {
            return FileSystemBrowserService.CheckHasAvailableAgentAsync();
        }
        #endregion

        #region Load & Save Node Settings
        [HttpPost]
        public async Task<string> LoadFSNodeSetting([FromBody] RMFSTreeNode node)
        {
            var settings = await FileSystemSettingsService.LoadFSNodeSettingAsync(node,loadLocalInfo: true);
            if (settings.ApprovalType == (int)ApprovalType.ApprovalProcess)
            {
                var result = Guid.TryParse(settings.WorkflowReferenceId, out var referenceId);
                if (result)
                {
                    var workflow = ManualProcessManagementService.GetWorkflow(referenceId);
                    settings.WorkflowReferenceName = workflow?.Name;
                }
            }
            return SerializerHelper.SerializeByJsonConvert(settings);
        }

        [HttpPost]
        public async Task<string> FSActiveSetting([FromBody] RMFSTreeNode curSetting)
        {
            //return SaveSettingProcessorFunc(() =>
            //{
            //    return mFileSystemSettingsService.SaveFSActiveSetting(curSetting);
            //}, "Save FS Active Setting Failed");
            var result = SaveSPSettingResult.Sucess;
            try
            {
                //Active Deactive
                await FileSystemSettingsService.SaveFSActiveSettingAsync(curSetting);
            }
            catch (Exception ex)
            {
                result = SaveSPSettingResult.Failed;
                Logger.Error("Save SharePoint Settings Failed.ERROR:{0}", ex.Message);
            }
            return result.ToString();
        }

        [HttpPost]
        public async Task<string> SaveFSGroupDocLevelSetting([FromBody] RMFSTreeNode curSetting)
        {
            var result = SaveSPSettingResult.Sucess;
            try
            {
                await FileSystemSettingsService.SaveFSNodeSettingAsync(curSetting);
            }
            catch (Exception ex)
            {
                result = SaveSPSettingResult.Failed;
                Logger.Error("Save SharePoint Settings Failed.ERROR:{0}", ex.Message);
            }
            return result.ToString();
        }

        [HttpPost]
        public async Task<string> SaveFSDocLevelSetting([FromBody] RMFSTreeNode curSetting)
        {
            var result = SaveSPSettingResult.Sucess;
            try
            {
                var validateMessage = FileSystemSettingsService.CheckNodeInfo(curSetting);
                if (validateMessage.MessageType == RAMessageType.Successful)
                {
                    if (!curSetting.DefaultTermId.Equals(Guid.Empty) && TaxonomyService.IsOrphanedTerm(curSetting.DefaultTermId))
                    {
                        return "DefaultTermIsOrphaned";
                    }
                    else
                    {
                        await FileSystemSettingsService.SaveFSNodeSettingAsync(curSetting);
                    }
                }
                else
                {
                    result = SaveSPSettingResult.Failed;
                }
            }
            catch (Exception ex)
            {
                result = SaveSPSettingResult.Failed;
                Logger.Error("Save FS Failed.ERROR:{0}", ex.Message);
            }
            return result.ToString();
        }

        [HttpPost]
        public async Task<string> SaveFSGeneralSetting([FromBody] RMFSTreeNode curSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result = await FileSystemSettingsService.SaveFSGeneralSetting4JPMC(curSetting);
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                Logger.Error("Save FS General Settings Failed.ERROR:{0}", ex.Message);
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidFSParameterActionFilter("ValidateSaveFSTermSetting")]
        public async Task<string> SaveFSLoactionOwners([FromBody] RMFSTreeNode curSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                var validateMessage = FileSystemSettingsService.CheckNodeInfo(curSetting);
                if (validateMessage.MessageType == RAMessageType.Successful)
                {
                    var syncUserResult = await RMSPSettingsService.SyncADUsersAsync(curSetting.RecordOwner);
                    if (syncUserResult.MessageType == RAMessageType.Successful)
                    {
                        await FileSystemSettingsService.AddFSLocationOwnersAsync(curSetting);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.ErrorMessage = syncUserResult.ErrorMessage;
                    }
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = validateMessage.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                Logger.Error("Save FS Loaction Owner Settings Failed.ERROR:{0}", ex.Message);
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> InheritFSParentSetting([FromBody] RMFSTreeNode curSetting)
        {
            var result = SaveSPSettingResult.Sucess;
            try
            {
                await FileSystemSettingsService.InheritFSParentSettingAsync(curSetting);
            }
            catch (Exception ex)
            {
                Logger.Error("Inherit GlobalSettings Failed.ERROR:{0}", ex.ToString());
                result = SaveSPSettingResult.Failed;
            }
            return result.ToString();
        }

        [HttpPost]
        public async Task<RAReturnMessage> SaveClassCodePolicy([FromBody] ClassCodePolicyInfo classCodePolicyInfo)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                result = await FileSystemSettingsService.SaveClassCodePolicyAsync(classCodePolicyInfo);
            }
            catch (Exception ex)
            {
                Logger.Error("Save Class Code Policy Failed.ERROR:{0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = I18NEntity.GetString("RM_FS_ClassCodePolicy_SaveFailed");
            }
            return result;
        }
        #endregion

        #region Run Job

        [HttpPost]
        public async Task<string> RunFSCollectionJob([FromBody] RMFSTreeNode selectedTree)
        {
            //var tree = JsonConvert.DeserializeObject<RMSPTreeNode>(selectedTree);
            var message = new RAReturnMessage();
            var validateMessage = FileSystemSettingsService.CheckNodeInfo(selectedTree);
            if (validateMessage.MessageType == RAMessageType.Successful)
            {
                message = await FileSystemSettingsService.RunDataSyncJobAsync(selectedTree, JobRunBy.Control);
            }
            else 
            {
                message.MessageType = RAMessageType.Failed;
                message.ErrorMessage = validateMessage.ErrorMessage;
            }
            return JsonConvert.SerializeObject(message);
        }

        [HttpPost]
        public async Task<string> RunFSSyncDataJob([FromBody]bool fromTimerJobPage)
        {
            return JsonConvert.SerializeObject(await FileSystemSettingsService.RunDataSyncJobAsync(null, JobRunBy.Control));
        }

        [HttpPost]
        public async Task<RAReturnMessage> RunFSDisposalJob([FromBody] RMFSTreeNode node)
        {
            var message = new RAReturnMessage() { MessageType = RAMessageType.Failed };
            var validateResult = FileSystemSettingsService.CheckNodeInfo(node);
            if (validateResult.MessageType == RAMessageType.Successful)
            {
                try
                {
                    //var treeNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(node);
                    message = await FileSystemSettingsService.RunDisposalJobAsync(node, JobRunBy.Control);
                }
                catch (Exception e)
                {
                    Logger.Error("An error occurred while running file system disposal job. Error:{1}", e.ToString());
                }
            }
            else
            {
                message.MessageType = RAMessageType.Failed;
                message.ErrorMessage = validateResult.ErrorMessage;
            }
            return message;
        }
        [HttpPost]
        public async Task<RAReturnMessage> RunFSClassCodeDisposalJob([FromBody] AvePoint.RA.Contract.JPMC.FSDisposalByClassCodeRequest request)
        {
            var message = new RAReturnMessage() { MessageType = RAMessageType.Failed };
            try
            {
                var enableJPMCFileSystemFeature = RMKeyValueDao.GetValueByKeyAsync<bool>(KeyNameCollection.EnableJPMCFileSystemFeature, false).GetAwaiter().GetResult();
                if (!enableJPMCFileSystemFeature)
                {
                    message.MessageType = RAMessageType.Failed;
                    message.ErrorMessage = "The feature is not enabled for non-JPMC users.";
                    return message;
                }
                else 
                {
                    var validateResult = await ValidateDisposalByClassCodeRequestAsync(request);
                    if (validateResult.MessageType != RAMessageType.Successful)
                    {
                        message.ErrorMessage = validateResult.ErrorMessage;
                        return message;
                    }

                    message = await FileSystemSettingsService.RunDisposalByClassCodeJobAsync(request, JobRunBy.Control);
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while running FS disposal by class code job. Error:{0}", e.ToString());
            }
            return message;
        }

        private async Task<RAReturnMessage> ValidateDisposalByClassCodeRequestAsync(AvePoint.RA.Contract.JPMC.FSDisposalByClassCodeRequest request)
        {
            var result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            if (request == null)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "Request body is null.";
                return result;
            }
            if (request.ConnectionGroupID == Guid.Empty)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "ConnectionGroupID is required.";
                return result;
            }
            if (request.TermID == null || request.TermID.Count == 0)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "At least one TermID is required.";
                return result;
            }

            var nodeSettings = await FileSystemSettingsService.LoadFSNodeSettingAsync(new RMFSTreeNode
            {
                Id = request.NodeId,
                ConnGroupId = request.ConnectionGroupID
            });
            if (nodeSettings == null)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "The selected node settings could not be found.";
                return result;
            }
            if (nodeSettings.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.Disable)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "The selected node is not enabled for record management.";
                return result;
            }
            if (nodeSettings.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.ParentDisable)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "The selected node inherits from a parent node that is not enabled for record management.";
                return result;
            }
            return result;
        }

        private string BuildClassCodeNameListById(List<Guid> termIds)
        {
            var names = termIds
                .Select(id =>
                {
                    var json = TaxonomyService.GetRMTermByGuId(id);
                    if (string.IsNullOrEmpty(json)) return null;
                    var term = JsonConvert.DeserializeObject<AvePoint.RA.Contract.TaxonomyModel.RMTermInfo>(json);
                    return term?.Name;
                })
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            return string.Join(", ", names);
        }

        [HttpPost]
        public async Task<RAReturnMessage> RunFSApplyClassCodeJob([FromBody] ApplyClassCodeSettingDto settingDto)
        {
            var message = new RAReturnMessage() { MessageType = RAMessageType.Failed };

            try
            {
                var key = RMKeyValueDao.GetValueByKey("ENABLE_JPMC_FILE_SYSTEM_FEATURE");
                bool.TryParse(key?.Value, out bool result);
                if (result == false)
                {
                    message.ErrorMessage = "Bad request.";
                    return message;
                }
                if (settingDto != null && string.IsNullOrEmpty(settingDto.ClassCode) || string.IsNullOrEmpty(settingDto.CountryCode))
                {
                    message.ErrorMessage = "Bad request.";
                    return message;
                }
                foreach(var fsNode in settingDto.FSTreeNode)
                {
                    if (fsNode.EnableRecordManagement == 1)
                    {
                        var enableRecordManagement = (await FileSystemSettingsService.LoadFSNodeSettingAsync(fsNode)).EnableRecordManagement;
                        if (enableRecordManagement != 1)
                        {
                            message.ErrorMessage = $"Failed to enable record management for node {fsNode.Name}.";
                            return message;
                        }
                    }
                }
                //var treeNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(node);
                message = await FileSystemSettingsService.RunApplyClassCodeJobAsync(settingDto, JobRunBy.Control);
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while running file system apply class code. Error:{1}", e.ToString());
            }

            return message;
        }

        #endregion

        #region Dispose Schedule
        [HttpPost]
        public async Task<string> UpdateFSDisposeSchedule([FromBody] RMFSTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                var validateMessage = FileSystemSettingsService.CheckNodeInfo(nodeSetting);
                if (validateMessage.MessageType == RAMessageType.Successful)
                {
                    var cloneNodeInfo = nodeSetting.Clone();
                    cloneNodeInfo.DisposeScheduleInfo = null;
                    //cloneNodeInfo.SkipRemoveContentAndDestroyAction = nodeSetting.DisposeScheduleInfo.Extentions.Equals("true", StringComparison.OrdinalIgnoreCase);
                    nodeSetting.DisposeScheduleInfo.Extentions = JsonConvert.SerializeObject(cloneNodeInfo);
                    var schedule = await ScheduleService.UpdateScheduleServiceForFSAsync(nodeSetting.DisposeScheduleInfo, nodeSetting.FullPath);
                    if (schedule == "-1")
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.ScheduleServiceFailed;
                    }
                }
                else 
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = validateMessage.ErrorMessage;
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
        public async Task<string> CreateFSDisposeSchedule([FromBody] RMFSTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                nodeSetting.DisposeScheduleInfo.Id = Guid.NewGuid().ToString();
                var cloneNodeInfo = nodeSetting.Clone();
                cloneNodeInfo.DisposeScheduleInfo = null;
                nodeSetting.DisposeScheduleInfo.Extentions = JsonConvert.SerializeObject(cloneNodeInfo);
                nodeSetting.DisposeScheduleInfo.ProfileId = ScheduleService.GetProfileId(nodeSetting);
                var schedule = await ScheduleService.CreateScheduleServiceForFSAsync(nodeSetting.DisposeScheduleInfo, true, nodeSetting.FullPath);
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
        public string DeleteFSDisposeSchedule([FromBody] RMFSTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                ScheduleService.DeleteScheduleServiceForFS(nodeSetting.DisposeScheduleInfo.Id, nodeSetting.FullPath);
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
        public string BreakFSDisposeSchedule([FromBody] RMFSTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                nodeSetting.DisposeScheduleInfo.Id = "";
                ScheduleService.CreateNoScheduleForFS(SettingScheduleType.Dispose, nodeSetting?.Id.ToString());
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

        [HttpGet]
        public async Task<RAReturnMessage> GetConnectionPermissions(Guid connectionId)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                var enableJPMCFileSystemFeature = await RMKeyValueDao.GetValueByKeyAsync<bool>(KeyNameCollection.EnableJPMCFileSystemFeature, false);
                if (!enableJPMCFileSystemFeature)
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = "The feature is not enabled for non-JPMC users.";
                    return result;
                }
                else
                {
                var connection = await FSRegisterService.GetConnectionByIdAsync(connectionId);
                if (connection != null)
                {
                    result.MessageType = RAMessageType.Successful;
                    result.Extension = JsonConvert.SerializeObject(connection);
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = "Connection not found";
                }
            }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "Failed to get connection permissions. Please try again.";
                Logger.Error($"Get Connection Permissions Failed. ConnectionId:{connectionId} ERROR:{ex.Message}");
            }
            return result;
        }

        [HttpPost]
        [ValidJPMCActionFilter]
        public async Task<FSAuditQueryResult> GetJPMCAuditByConnectionLevel([FromBody] FSAuditQueryParam fsQueryDto)
        {
            if (fsQueryDto.Filters == null)
            {
                fsQueryDto.Filters = new List<FSAuditQueryFilter>();
            }

            if (!string.IsNullOrWhiteSpace(fsQueryDto.SearchKey))
            {
                fsQueryDto.Filters.Add(new FSAuditQueryFilter
                {
                    ColumnName = nameof(RMFSAudit.ObjectName),
                    ColumnValues = new List<string> { fsQueryDto.SearchKey },
                });
            }

            int skip = (fsQueryDto.PageIndex - 1) * fsQueryDto.PageSize;
            int take = fsQueryDto.PageSize;

            var (items, totalCount) = await FSAuditSinkService.QueryAsync(fsQueryDto.Filters, skip, take, fsQueryDto.Order);

            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var item in items)
            {
                item.FormattedTime = GeneralSettingService.ConvertTiksToDateTime(gls, item.ActionTimeUtc, true).SimplifyFormatTime;
            }

            return new FSAuditQueryResult
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        [HttpGet]
        [ValidJPMCActionFilter]
        public FilterSource GetJPMCAuditFilterSources()
        {
            try
            {
                return new FilterSource
                {
                    UserItems = FSAuditSinkService.FetchAllAuditUsers(),
                    ActionItems = FSAuditSinkService.FetchAllAuditTypes()
                };
        }
            catch (Exception ex)
            {
                Logger.Error($"GetJPMCAuditFilterSources Failed. ERROR:{ex.Message}");
                return new();
            }
        }
        #endregion

        #region Term

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ManualReviewEnduser | RMPermissionMasks.FSAdmin, RMDiscoveryFileSystemPermissionMask.AccessAll, joinType: PermissionJoinType.Any, permissionJoinType = DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        public int GetClassificationLevel()
        { 
            return FileSystemSettingsService.GetClassificationLevel();
        }

        [HttpPost]
        public Task SetClassificationLevel([FromBody] int classificationLevel)
        {
            return FileSystemSettingsService.SetClassificationLevelAsync(classificationLevel);
        }

        [HttpPost]
        public Task<string> GetFSSubTerm([FromBody] FSTreePage tree)
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
        public Task<string> GetFSSavedTerm([FromBody] CurrentSettingsInfo settingInfo)
        {
            return TaxonomyService.GetFSSavedTermAsync(settingInfo, true);
        }

        [HttpPost]
        public async Task<List<ClassCodeCascadeDataDto>> GetClassCodeCascadeData([FromBody] CurrentSettingsInfo settingInfo)
        {
            var result = await TaxonomyService.GetClassCodeCascadeDataAsync(settingInfo);
            return result;
        }
        #endregion

        #region Other Page
        [HttpPost]
        public string GetFSTreeInitData()
        {
            var fsRoot = FileSystemBrowserService.LoadFSRoot()[0];
            if (fsRoot == null || fsRoot.Id == Guid.Empty)
            {
                Logger.Warn("Farm node is null.Please refresh page.");
            }
            else
            {
                if (fsRoot.Children != null)
                {
                    //删除Children属性，避免以后convert to SPTree时出现死循环
                    fsRoot.Children = null;
                }
            }
            return SerializerHelper.SerializeByJsonConvert(fsRoot);
        }
        [HttpPost]
        public async Task<RMFSTreeNode> FSMoveBrowse([FromBody] RMFSTreeNode curRMNode)
        {
            string result = string.Empty;
            string name = string.Empty;
            try
            {
                List<RMFSTreeNode> children = new List<RMFSTreeNode>();
                name = curRMNode.Name;
                if (!string.IsNullOrEmpty(curRMNode.FullPath))
                {
                    //对path解密
                    curRMNode.FullPath = EncodeUtil.DecryptByCommunicationKey(curRMNode.FullPath);
                    if (curRMNode.Level != (int)NodeLevel.WebApplication && !TreeNodeUtil.CheckPathTraversal(curRMNode.FullPath))
                    {
                        throw new Exception(string.Format("Path not vaild:{0}", curRMNode.FullPath));
                    }
                }
                children = (await FileSystemBrowserService.FSBrowseAsync(curRMNode)).OrderBy(a => a.Name).ToList();
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    children[i].IsActive = true;//没有配置的节点的默认值修改成Active，显示在FSMoveTree上
                    if (!string.IsNullOrEmpty(children[i].FullPath))
                    {
                        //对path加密,防止Path Traversal
                        children[i].FullPath = EncodeUtil.EncryptByCommunicationKey(children[i].FullPath);
                    }
                    await FileSystemSettingsService.LoadFSNodeSettingAsync(children[i]);
                    if (!children[i].IsActive && children[i].Level == (int)NodeLevel.FSFolder)
                    {
                        children.RemoveAt(i);
                    }
                }
                curRMNode.Children = children;
                //children?.ForEach(n => n.Parent = null);
                //foreach (var child in children)
                //{
                //    mRMSPSettingsService.LoadFSNodeSetting(child);
                //}
                result = SerializerHelper.SerializeByJsonConvert(children);
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when browe node.NodeName:[{0}] Error:{1}", name, e.ToString());
                throw;
            }
            return curRMNode;
        }

        #endregion

        [HttpPost]
        public async Task<bool> CheckApplyClassJobRunning([FromBody] RMFSTreeNode node)
        {
            return await FileSystemSettingsService.HasRunningJobOnSelectedNode(node);
        }
    }
}
