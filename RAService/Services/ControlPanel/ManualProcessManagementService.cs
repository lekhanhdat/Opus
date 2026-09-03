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
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureCosmosDB;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.ControlPanel.AuditHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ControlPanel
{
    [Audit]
    public class ManualProcessManagementService : RMServiceBase, IManualProcessManagementService
    {
        private RALogger logger = RALogger.GetInstance(typeof(ManualProcessManagementService));
        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        public IRMWorkflowDefinitionDao RMWorkflowDefinitionDao => PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();

        private readonly IGControlPlatformApprovalProcessService _gControlPlatformApprovalProcessService = PlatformWindsorManager.GetService<IGControlPlatformApprovalProcessService>();
        public IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }

        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.WorkflowManagement, Action = AuditAction.DeleteWorkflow, BeforeHandler = typeof(WorkflowManagementBeforeAuditHandler), AfterHandler = typeof(WorkflowManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> DeleteProcessAsync(Guid id)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            try
            {
                var container = await RMAzureCosmosDBContext.GetContainerAsync();
                var exists = await container.UseLinqQuery().Where(
                        item =>
                        item.ManualWorkflowDefinitionId == id &&
                        item.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove)
                    .AsResultSet()
                    .ExistAsync();
                    
                if (exists || RMWorkflowDefinitionDao.IsRunningWorkflow(id))
                {
                    returnMessage.FaildType = RAFailedType.HasRunningWorkflowInstance;
                    returnMessage.MessageType = RAMessageType.Failed;
                }
                else {
                    RMWorkflowDefinitionDao.DeleteWorkflow(id);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when delete manual process, id:[{id}], error:{ex.ToString()}");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        public async Task<QueryProcessesResultDto> GetProcessesAsync(ProcessQueryDto dto)
        {
            //var wfViewDtos = new List<WorkflowDefinitionViewDto>();
            var wfViewDtos = new List<NewWorkflowDefinitionViewDto>();
            int totalCount = 0;
            var dbItems = RMWorkflowDefinitionDao.QueryWorkflows(dto, out totalCount);
            var gls = await mGeneralSettingService.GetGeneralSettingAsync();
            var dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
            if (dbItems.Count > 0)
            {
                await dbItems.ForEachAsync(async (item) =>
                {
                    var wfViewDto = new NewWorkflowDefinitionViewDto();
                    wfViewDto = NewConvertToWFDefinitionViewDto(item, gls, dateFormat);
                    var userIds = RMWorkflowDefinitionDao.GetReviewerIds(item.Id);
                    if (userIds.Count > 0)
                    {
                        var users = await GetReviewersAsync(userIds);
                        if (users != null && users.Count > 0)
                        {
                            wfViewDto.UserDisplayNames = users.Select(u => u.DisplayName).ToList();
                        }
                    }
                    wfViewDtos.Add(wfViewDto);

                });
            }
            var resultDto = new QueryProcessesResultDto();
            resultDto.TotalCount = totalCount;
            resultDto.ResultList = wfViewDtos;
            return resultDto;
        }

        public WorkflowDefinitionDto LoadProcess(Guid id)
        {
            var dbWorkflow = RMWorkflowDefinitionDao.LoadWorkflow(id);
            return ConvertToWFDefinitionDto(dbWorkflow);
        }

        /// <summary>
        /// 获取相同referenceId的最大verison workflow
        /// </summary>
        /// <param name="referenceId"></param>
        /// <returns></returns>
        public WorkflowDefinitionDto GetWorkflow(Guid referenceId)
        {
            var dbWorkflow = RMWorkflowDefinitionDao.GetWorkflowByReferenceId(referenceId);
            if (dbWorkflow != null)
            {
                return ConvertToWFDefinitionDto(dbWorkflow);
            }
            return null;
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.WorkflowManagement, Action = AuditAction.CreateWorkflow, BeforeHandler = typeof(WorkflowManagementBeforeAuditHandler), AfterHandler = typeof(WorkflowManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> SaveAsync(WorkflowDefinitionDto dto)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            var workflowName = "";
            var workflowId = Guid.Empty;
            var workflowXaml = "";
            try
            {

                var syncUserResult = await SyncADUsersAsync(dto);
                if (syncUserResult.MessageType != RAMessageType.Successful)
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.ErrorMessage = syncUserResult.ErrorMessage;
                    return returnMessage;
                }

                if (dto.Content.WorkflowNodes.Any(t => t.Reviewers.Any(r => r.RMUserId == 0)))
                {
                    throw new Exception("Exist RMUserId is 0 reviewer, can not save workflow");
                }

                workflowName = dto.Name;
                workflowId = dto.Id;
                //验证workflow
                ValidateWorkflow(dto);
                //workflowXaml = dto.XamlStr;

                //dto.HashCode = GetHashCode(workflowXaml);
                if (needUpgradeVersion(dto))
                {
                    logger.Info($"This workflow needs to be upgraded to new version, Id:[{dto.Id}] ReferenceId:[{dto.ReferenceId}]");
                    //升级Version,Step Node信息需要重新构造
                    RebuildWorkflowNodes(dto);
                    //throw new Exception("obsoleted");
                    ////workflowXaml = XamlBuilder.BuildXaml(dto);
                    ////dto.HashCode = GetHashCode(workflowXaml);
                    dto.UpgradeVersion = true;
                }
                else {
                    dto.UpgradeVersion = false;
                }
                //dto.XamlStr = workflowXaml;
                dto.ContentStr = JsonConvert.SerializeObject(dto.Content);
                
                dto.LevelCount = CalculateStepLevel(dto.Content.WorkflowNodes);
                var accountId = TenantLocalValue.LogonUserId;
                var loginUser = AccountDao.Find(s => s.UserId == accountId);
                dto.CreatedBy = loginUser.DisplayName;

                await RMWorkflowDefinitionDao.SaveWorkflowAsync(dto);
            }
            catch (WorkflowNameConflictException)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_RDM_WorkFlow_Msg_NameAlreadyExists");
            }
            catch (WorkflowNoConfigReviewerException)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_RDM_WorkFlow_Msg_NotFillReviewers");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when save the name of workflow is {workflowName}, id:[{workflowId}], message:{ex.ToString()}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = dto.Id == Guid.Empty ? I18NEntity.GetString("RM_RDM_WorkFlow_Msg_CreateFailed") : I18NEntity.GetString("RM_RDM_WorkFlow_Msg_EditFailed");
            }
            return returnMessage;
        }

        public bool IsUpgradeVerion(WorkflowDefinitionDto dto)
        {
            //throw new Exception("obsoleted");
            //var workflowXaml = XamlBuilder.BuildXaml(dto);
            //dto.HashCode = GetHashCode(workflowXaml);
            //return dto.Id != Guid.Empty && RMWorkflowDefinitionDao.NeedUpgradeVersion(dto);
            return needUpgradeVersion(dto);
        }

        public void PrepareManualProcessReplicaRequest(WorkflowDefinitionDto dto)
        {
            var savedWorkflow = LoadSavedWorkflowForReplica(dto);
            CopyWorkflowDefinition(savedWorkflow, dto);
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.WorkflowManagement, Action = AuditAction.CreateWorkflow, BeforeHandler = typeof(WorkflowManagementBeforeAuditHandler), AfterHandler = typeof(WorkflowManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> ApplyManualProcessAsync(WorkflowDefinitionDto dto)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (dto == null || dto.Id == Guid.Empty)
                {
                    throw new ArgumentException("Manual process replica request is invalid.", nameof(dto));
                }

                dto.ContentStr = !string.IsNullOrEmpty(dto.ContentStr)
                    ? dto.ContentStr
                    : JsonConvert.SerializeObject(dto.Content);
                dto.LevelCount = dto.LevelCount == 0 && dto.Content?.WorkflowNodes != null
                    ? CalculateStepLevel(dto.Content.WorkflowNodes)
                    : dto.LevelCount;

                await RMWorkflowDefinitionDao.UpsertReplicaWorkflowAsync(dto);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when apply manual process replica data, id:[{dto?.Id}], error:{ex}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_RDM_WorkFlow_Msg_EditFailed");
            }

            return returnMessage;
        }

        public WorkflowDefinitionViewDto ConvertToWFDefinitionViewDto(RMWorkflowDefinition dbItem, GeneralSettingModel gls)
        {
            var dto = new WorkflowDefinitionViewDto();
            dto.Id = dbItem.Id;
            dto.Name = dbItem.Name;
            dto.Description = dbItem.Description;
            dto.CreatedOnStr = mGeneralSettingService.ConvertTiksToDateTime(gls, dbItem.CreationTime.Ticks, true).DataTime.ToString("MM/dd/yyyy");
            dto.LevelCount = dbItem.Level;
            dto.StepInfo = JsonConvert.DeserializeObject<RMWorkflowContentDto>(dbItem.ContentStr);
            return dto;
        }
        public NewWorkflowDefinitionViewDto NewConvertToWFDefinitionViewDto(RMWorkflowDefinition dbItem, GeneralSettingModel gls, string dataFormat)
        {
            var dto = new NewWorkflowDefinitionViewDto();
            dto.Id = dbItem.Id;
            dto.Name = dbItem.Name;
            dto.Description = dbItem.Description;
            dto.ContentStr = dbItem.ContentStr;
            dto.CreatedOnStr = mGeneralSettingService.ConvertTiksToDateTime(gls, dbItem.CreationTime.Ticks, true).DataTime.ToString(dataFormat);
            dto.LevelCount = dbItem.Level;
            return dto;
        }

        public WorkflowDefinitionDto ConvertToWFDefinitionDto(RMWorkflowDefinition dbItem)
        {
            var dto = new WorkflowDefinitionDto();
            dto.Id = dbItem.Id;
            dto.Name = dbItem.Name;
            dto.Description = dbItem.Description;
            dto.ReferenceId = dbItem.ReferenceId;
            dto.Type = dbItem.Type;
            dto.LevelCount = dbItem.Level;
            dto.ContentStr = dbItem.ContentStr;
            if (!string.IsNullOrEmpty(dbItem.ContentStr))
            {
                dto.Content = JsonConvert.DeserializeObject<RMWorkflowContentDto>(dbItem.ContentStr);
            }
            dto.XamlStr = dbItem.XamlStr;
            dto.CreatedBy = dbItem.CreatedBy;
            dto.CreatedOn = dbItem.CreationTime;
            dto.HashCode = dbItem.HashCode;
            dto.LastUpdatedTime = dbItem.LastUpdatedTime;
            dto.Version = dbItem.Version;
            return dto;
        }


        public void BrowseWorkflowNode(List<Contract.RMWeb.CP.RMWorkflowStepNode> allNodes, List<Contract.RMWeb.CP.RMWorkflowStepNode> parentNodes, ref int stepCount)
        {
            var pIds = parentNodes.Select(t => t.Id).ToList();
            var childNodes = allNodes.Where(t => pIds.Contains(t.ParentId)).ToList();
            if (childNodes.Count > 0)
            {
                if (!childNodes.Any(c => c.NodeType == WorkflowNodeType.Destroy || c.NodeType == WorkflowNodeType.NotDestroy))
                {
                    stepCount++;
                    BrowseWorkflowNode(allNodes, childNodes, ref stepCount);
                }
            }
        }

        public int CalculateStepLevel(List<Contract.RMWeb.CP.RMWorkflowStepNode> allNodes)
        {
            return allNodes.Count(item => item.NodeType == WorkflowNodeType.BeginDisposalReview
            || item.NodeType == WorkflowNodeType.DisposalReview);
            //var firstNodes = allNodes.Where(t => t.NodeType == WorkflowNodeType.BeginDisposalReview).ToList();
            //int stepCount = 1;
            //BrowseWorkflowNode(allNodes, firstNodes, ref stepCount);
            //return stepCount;
        }

        public async Task<List<ReviewerUser>> GetReviewersAsync(List<string> reviewerIds)
        {
            return (await AccountDao.FindListAsync(o => reviewerIds.Contains(o.UserId)))
                .Select(o => ConvertToAccount(o))
                .DistinctBy(item => item.UserId)
                .ToList();
        }

        private ReviewerUser ConvertToAccount(RMAccount owner)
        {
            return new ReviewerUser()
            {
                UserId = owner.UserId,
                DisplayName = owner.DisplayName,
                UserPrincipalName = owner.UserPrincipalName,
            };
        }

        public async Task<WorkflowDefinitionViewDto> LoadWorkflowViewDtoAsync(Guid id)
        {
            var dbWorkflow = RMWorkflowDefinitionDao.LoadWorkflow(id);
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            var dto = new WorkflowDefinitionViewDto();
            dto = ConvertToWFDefinitionViewDto(dbWorkflow, gls);
            var newdto = ConvertToWFDefinitionDto(dbWorkflow);
            dto.UserDisplayNames = await GetReviewerNamesAsync(id);
            foreach (var node in newdto.Content.WorkflowNodes)
            {
                switch (node.ReviewerType)
                {
                    case WorkflowReviewerType.SiteOwners:
                        dto.UserDisplayNames.Add(I18NEntity.GetString("RM_RDM_WorkFlow_RecordOwnerText"));
                        break;
                    case WorkflowReviewerType.SharePointGroup:
                        dto.UserDisplayNames.Add(I18NEntity.GetString("RM_RDM_WorkFlow_SharePointGroupText"));
                        break;
                    case WorkflowReviewerType.InformationOwner:
                        dto.UserDisplayNames.Add(I18NEntity.GetString("RM_RDM_WorkFlow_InformationOwnerText"));
                        break;
                    default:
                        break;
                }
            }
            return dto;
        }

        public async Task<List<string>> GetReviewerNamesAsync(Guid workflowId)
        {
            var displayNames = new List<string>();
            var userIds = RMWorkflowDefinitionDao.GetReviewerIds(workflowId);
            if (userIds.Count > 0)
            {
                var users = await GetReviewersAsync(userIds);
                if (users != null && users.Count > 0)
                {
                    displayNames = users.Select(u => u.DisplayName).ToList();
                }
                else
                {
                    List<string> displayName = new List<string>();
                    displayName.Add(I18NEntity.GetString("RM_RDM_WorkFlow_RecordOwnerText"));
                    displayNames = displayName;
                }
            }
            return displayNames;
        }

        private void ValidateWorkflow(WorkflowDefinitionDto dto)
        {
            ValidateWorkflowName(dto);
            ValidateWorkflowNodeSetting(dto.Content);
            //ValidateWorkflowXaml(dto);
        }

        private void ValidateWorkflowName(WorkflowDefinitionDto dto)
        {
            RMWorkflowDefinitionDao.CheckSameWorkflow(dto);
        }

        private void ValidateWorkflowNodeSetting(RMWorkflowContentDto content)
        {
            var hasError = false;
            if (content == null)
            {
                hasError = true;
            }
            else
            {
                //目前只坚持是否配置reviewer,并且一个setting中reviewer不可以重复
                foreach (var node in content.WorkflowNodes)
                {
                    if (node.NodeType == WorkflowNodeType.BeginDisposalReview || node.NodeType == WorkflowNodeType.DisposalReview)
                    {
                        if(node.ReviewerType == WorkflowReviewerType.SiteOwners)
                        {
                            continue;
                        }
                        if (node.ReviewerType == WorkflowReviewerType.InformationOwner)
                        {
                            continue;
                        }

                        if (node.ReviewerType == WorkflowReviewerType.SharePointGroup)
                        {
                            if (!string.IsNullOrEmpty(node.DisplayName))
                            {
                                continue;
                            }
                            else
                            {
                                hasError = true;
                                continue;
                            }
                        }

                        if (node.Reviewers == null || node.Reviewers.Count == 0)
                        {
                            hasError = true;
                            break;
                        }
                        else
                        {
                            if (node.Reviewers.GroupBy(t => t.UserId).Count() != node.Reviewers.Count)
                            {
                                hasError = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (hasError)
            {
                throw new WorkflowNoConfigReviewerException();
            }
        }



        public List<string> GetReviewerNames(RMWorkflowContentDto content)
        {
            var names = new List<string>();
            var nodes = content.WorkflowNodes;
            if (nodes != null && nodes.Count > 0)
            {
                foreach (var node in content.WorkflowNodes)
                {
                    switch (node.ReviewerType)
                    {
                        case WorkflowReviewerType.SiteOwners:
                            names.Add(I18NEntity.GetString("RM_RDM_WorkFlow_RecordOwnerText"));
                            break;
                        case WorkflowReviewerType.SharePointGroup:
                            names.Add(I18NEntity.GetString("RM_RDM_WorkFlow_SharePointGroupText"));
                            break;
                        case WorkflowReviewerType.InformationOwner:
                            names.Add(I18NEntity.GetString("RM_RDM_WorkFlow_InformationOwnerText"));
                            break;
                        default:
                            break;
                    }
                    if (node.Reviewers != null && node.Reviewers.Count > 0)
                    {
                        foreach (var item in node.Reviewers)
                        {
                            if (!names.Contains(item.UserId))
                            {
                                names.Add(item.DisplayName);
                            }
                        }
                    }
                }
            }
            return names;
        }

        public List<WorkflowSimpleDto> GetAllSimpleProcesses()
        {
            var simpleWorkflows = new List<WorkflowSimpleDto>();
            var dbWorkflows = RMWorkflowDefinitionDao.GetAllWorkflows();
            foreach (var item in dbWorkflows)
            {
                simpleWorkflows.Add(new WorkflowSimpleDto
                {
                    ReferenceId = item.ReferenceId,
                    Name = item.Name,
                    Checked = false
                });
            }
            return simpleWorkflows;
        }

        public WorkflowSimpleDto GetSimpleProcessByName(string name)
        {
            RMWorkflowDefinition dbWorkflow = RMWorkflowDefinitionDao.LoadWorkflow(name);
            WorkflowSimpleDto simpleDto = new WorkflowSimpleDto();
            if (dbWorkflow != null)
            {
                simpleDto.ReferenceId = dbWorkflow.ReferenceId;
                simpleDto.Name = dbWorkflow.Name;
                simpleDto.Checked = false;
            }
            return simpleDto;
        }

        public void RebuildWorkflowNodes(WorkflowDefinitionDto dto)
        {
            var uiNodes = dto.Content.WorkflowNodes;
            var generateIdsMapping = new Dictionary<Guid, Guid>();
            var generateIds = new HashSet<Guid>();
            var nodeQueue = new Queue<Contract.RMWeb.CP.RMWorkflowStepNode>();
            var startNode = uiNodes.First(item => item.NodeType == WorkflowNodeType.Start);
            nodeQueue.Enqueue(startNode);
            while(nodeQueue.Any())
            {
                var node = nodeQueue.Dequeue();

                if (!generateIds.Contains(node.Id))
                {
                    var oldId = node.Id;
                    node.Id = Guid.NewGuid();
                    generateIdsMapping.Add(oldId, node.Id);
                    generateIds.Add(node.Id);
                }

                var newChildIds = new List<Guid>();

                node.ChildrenIds.ForEach(childId =>
                {
                    if (!generateIdsMapping.ContainsKey(childId))
                    {
                        var childNode = uiNodes.Where(item => childId == item.Id).First();
                        childNode.Id = Guid.NewGuid();
                        childNode.ParentId = node.Id;
                        generateIdsMapping.Add(childId, childNode.Id);
                        generateIds.Add(childNode.Id);
                        nodeQueue.Enqueue(childNode);
                    }

                    newChildIds.Add(generateIdsMapping[childId]);
                });

                node.ChildrenIds = newChildIds;
            }

            //var startNode = uiNodes.Where(n => n.NodeType == WorkflowNodeType.Start).FirstOrDefault();
            //var endNodeId = Guid.NewGuid();
            //RebuildChildNodes(startNode, ref uiNodes, ref endNodeId);
            //var endNode = uiNodes.Where(n => n.NodeType == WorkflowNodeType.End).FirstOrDefault();
            //endNode.Id = endNodeId;
        }

        public void RebuildChildNodes(Contract.RMWeb.CP.RMWorkflowStepNode pNode, ref List<Contract.RMWeb.CP.RMWorkflowStepNode> uiNodes, ref Guid endNodeId)
        {
            var childIds = pNode.ChildrenIds;
            if (pNode.NodeType == WorkflowNodeType.End)
            {
                return;
            }
            if (pNode.NodeType == WorkflowNodeType.Start)
            {
                pNode.Id = Guid.NewGuid();
            }
            if (childIds.Count > 0)
            {
                var childrenNodes = uiNodes.Where(n => childIds.Contains(n.Id)).ToList();
                var newChildIds = new List<Guid>();
                foreach (var node in childrenNodes)
                {
                    node.ParentId = pNode.Id;
                    if (node.NodeType == WorkflowNodeType.End)
                    {
                        newChildIds.Add(endNodeId);
                    }
                    else
                    {
                        node.Id = Guid.NewGuid();
                        newChildIds.Add(node.Id);
                    }
                    RebuildChildNodes(node, ref uiNodes, ref endNodeId);
                }
                pNode.ChildrenIds = newChildIds;
            }
        }

        public async Task<RAReturnMessage> SyncADUsersAsync(WorkflowDefinitionDto dto)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (dto.Content != null && dto.Content.WorkflowNodes != null)
                {
                    var reviewers = new List<ReviewerUser>();
                    dto.Content.WorkflowNodes.ForEach(o =>
                    {
                        o.Reviewers.ForEach(r =>
                        {
                            //if (string.IsNullOrEmpty(r.UserId) && !userAADIds.Contains(r.Id))
                            //{
                                reviewers.Add(r);
                            //}
                        });
                    });
                    await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, reviewers);

                    if (reviewers.Count > 0)
                    {
                        dto.Content.WorkflowNodes.ForEach(o =>
                        {
                            var newRegisterUsers = o.Reviewers.Where(r => string.IsNullOrEmpty(r.UserId)).ToList();
                            newRegisterUsers.ForEach(r =>
                            {
                                var user = reviewers.Where(u => u.Id.Equals(r.Id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                                if (user != null)
                                {
                                    r.UserId = user.UserId;//成功注册到AOS后需要更新页面Dto中的UserId，Save workflow的时候使用
                                }
                            });
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_RegisterUser_Error_Message");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        public async Task<WorkflowDefinitionDto> LoadProcessFromGControl(Guid id)
        {
            return await _gControlPlatformApprovalProcessService.GetPlatformApprovalProcess(id);
        }

        private WorkflowDefinitionDto LoadSavedWorkflowForReplica(WorkflowDefinitionDto dto)
        {
            RMWorkflowDefinition workflow = null;

            if (dto.UpgradedVersionId != Guid.Empty)
            {
                workflow = RMWorkflowDefinitionDao.LoadAsync(dto.UpgradedVersionId).GetAwaiter().GetResult();
            }

            if (workflow == null && dto.ReferenceId != Guid.Empty)
            {
                workflow = RMWorkflowDefinitionDao.GetWorkflowByReferenceId(dto.ReferenceId);
            }

            if (workflow == null && dto.Id != Guid.Empty)
            {
                workflow = RMWorkflowDefinitionDao.LoadAsync(dto.Id).GetAwaiter().GetResult();
            }

            if (workflow == null && !string.IsNullOrWhiteSpace(dto.Name))
            {
                workflow = RMWorkflowDefinitionDao.LoadWorkflow(dto.Name);
            }

            if (workflow == null)
            {
                throw new InvalidOperationException($"Unable to load saved manual process for replica. Id: [{dto.Id}], ReferenceId: [{dto.ReferenceId}], Name: [{dto.Name}].");
            }

            return ConvertToWFDefinitionDto(workflow);
        }

        private static void CopyWorkflowDefinition(WorkflowDefinitionDto source, WorkflowDefinitionDto target)
        {
            target.Id = source.Id;
            target.ReferenceId = source.ReferenceId;
            target.OperationUniqueId = source.OperationUniqueId;
            target.UpgradedVersionId = source.UpgradedVersionId;
            target.Name = source.Name;
            target.Description = source.Description;
            target.Type = source.Type;
            target.LevelCount = source.LevelCount;
            target.ContentStr = source.ContentStr;
            target.XamlStr = source.XamlStr;
            target.CreatedBy = source.CreatedBy;
            target.CreatedOn = source.CreatedOn;
            target.HashCode = source.HashCode;
            target.LastUpdatedTime = source.LastUpdatedTime;
            target.Version = source.Version;
            target.Content = source.Content;
            target.UpgradeVersion = source.UpgradeVersion;
        }

        private bool needUpgradeVersion(WorkflowDefinitionDto dto)
        {
            if (dto.Id == Guid.Empty)
            {
                return false;
            }
            if (RMWorkflowDefinitionDao.NeedUpgradeVersion(dto))
            {
                logger.Info("In progress record of definition {0}, exists old in progress data.");
                return true;
            }
            bool exitsInprogress = ExplorerDao.Exist(a => a.ManualWorkflowDefinitionId == dto.Id && a.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress && a.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove);
            logger.Info("In progress record of definition {0}, exists? {1}", dto.Id, exitsInprogress);
            return exitsInprogress;
        }
    }
}
