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
using Amazon.Runtime.Internal.Transform;
using Aspose.Words.Bibliography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common.Setting;
using AvePoint.RA.SharePoint.Common.Setting.Model;
using AvePoint.RA.SharePoint.Object;
using AvePoint.RA.SharePoint.Teams.ColumnSetting.ImportTeamsSetting.Helper;
using AvePoint.RA.SharePoint.Teams.ColumnSetting.ImportTeamsSetting.Model;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Teams.ColumnSetting.ImportTeamsSetting
{
    public class RMImportTeamsSettingProcessor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(RMImportTeamsSettingProcessor));
        private List<RMSPSampleTreeNode> _containerNodes = [];
        private string _csvPath;
        private AveContextHelper _aveContextHelper;
        private SettingCsv _settingCsv;
        private ImportSettingHelper _importSettingHelper;
        private SettingHelper _settingHelper;
        private Dictionary<string, List<RMSPSampleTreeNode>> _teamOrGroupInContainerCache = new();
        private Dictionary<string, RemoteSiteCollection> _remoteSiteCollectionCache = new();
        private Dictionary<string, List<RMSPSampleTreeNode>> _siteInTeamsOrGroupCache = new();
        private Dictionary<Guid, List<Guid>> _termPermissionsDic;
        private TermPermissionMethod _termPermissionType;
        private RMSPSampleTreeNode _siteCollection = new();
        private JobResult _result;
        private string _commomErrorMessage;
        private const Char PathSeparator = '|';
        private readonly ITeamsSettingTreeService _teamsSettingTreeService = PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private readonly IBrowseTreeService _browseTreeService = PlatformWindsorManager.GetService<IBrowseTreeService>();
        private readonly ITeamsSettingDao _teamsSettingDao = PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private readonly ITermGroupDao _termGroupDao = PlatformWindsorManager.GetService<ITermGroupDao>();
        private readonly ITermSetDao _termSetDao = PlatformWindsorManager.GetService<ITermSetDao>();
        private readonly ITermDao _termDao = PlatformWindsorManager.GetService<ITermDao>();
        private readonly ITermSetMembershipDao _termSetMembershipDao = PlatformWindsorManager.GetService<ITermSetMembershipDao>();
        private IRMWorkflowDefinitionDao _rMWorkflowDefinitionDao => PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();
        public ILicenseHelperService _licenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        public IAccountWrapperService _accountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();
        public IUserService _userSerive => PlatformWindsorManager.GetService<IUserService>();
        private IRMTeamsSettingsService _rMTeamsSettingsService => PlatformWindsorManager.GetService<IRMTeamsSettingsService>();
        private IRMSecurityGroupDao _rMSecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();

        protected IRMReportManager _reportManager;
        private IMultiGeoDataCenterService _multiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        public RMImportTeamsSettingProcessor(RMImportSPSettingJobMessage jobMsg)
        {
            _aveContextHelper = new AveContextHelper();
            _settingCsv = new SettingCsv();
            _importSettingHelper = new ImportSettingHelper();
            _settingHelper = new SettingHelper();
            _settingHelper.GetRMTeamsSettings();
            _containerNodes = [];
            ReportMangerFactory.Instance.Init(jobMsg.JobID, jobMsg.JobType);
            _reportManager = ReportMangerFactory.Instance.ReportManager;
            _result = new JobResult();
            _csvPath = _importSettingHelper.GetImportJobCSVFile(jobMsg.CSVPath);
        }

        public async Task RunAsync()
        {
            _reportManager.IncreaseBase(10);
            _reportManager.StartUpdateJobProgress();
            JobStatus status = JobStatus.None;
            try
            {
                var currentUser = (await _userSerive.SearchUsersAsync(new List<string> { TenantLocalValue.LogonUserEmail })).FirstOrDefault();
                (_termPermissionType, _termPermissionsDic) = _rMSecurityGroupDao.GetTermGroupIdUserScopePermission(currentUser?.UserId);

                await GetAllContainers();
                var (settingDatas, settingDataFail, illegalCharactersErrorMessage) = _settingCsv.ReadCsv(_csvPath);
                if (illegalCharactersErrorMessage.IsNotNullOrEmpty())
                {
                    _result.HasFailed = true;
                    _commomErrorMessage = illegalCharactersErrorMessage;
                }
                else
                {
                    foreach (var detail in settingDataFail)
                    {
                        _reportManager.SendJobDetail(detail);
                    }
                    _reportManager.IncreaseBase(settingDatas.Count);
                    settingDatas = settingDatas.OrderBy(_ => _.ContainerName.Length).ThenBy(_ => _.TeamsOrGroupName.Length).ThenBy(_ => _.SiteCollectionUrl.Length).ToList();
                    bool isNewOpus = await _licenseHelperService.IsNewOpus();
                    foreach (var settingData in settingDatas)
                    {
                        await AddCustomSettingAsync(settingData, isNewOpus);
                        _reportManager.Increase(1);
                    }
                }
            }
            catch (Exception ex)
            {
                _result.HasFailed = true;
                _logger.Error($"error occured in RMImportTeamsSettingProcessor, error: {ex.Message}");
            }
            finally
            {
                status = _result.HasFailed ? _result.HasSuccessful
                                            ? JobStatus.FinishWithException
                                            : JobStatus.Failed
                                        : JobStatus.Finished;
                string jobComment = (status == JobStatus.FinishWithException || status == JobStatus.Failed)
                    ? _commomErrorMessage
                    : string.Empty;
                _reportManager.SetJobFinished(status, jobComment);
                try
                {
                    System.IO.File.Delete(this._csvPath);
                }
                catch (Exception e)
                {
                    _logger.Warn($"Delete csvs error:{e.ToString()}");
                }
                _aveContextHelper.DisposeWebCache();
                _aveContextHelper.DisposeSiteCache();
            }
        }
        private async Task GetAllContainers()
        {
            try
            {
                var farmNode = _teamsSettingTreeService.LoadFarmSampleTree()[0];
                if (farmNode == null) return;

                farmNode.PageSize = int.MaxValue;
                farmNode.SourceType = (int)SourceFlag.Teams;
                var returnNode = await _browseTreeService.BrowseSPOTreeAsync(farmNode, RMBrowseTreeNodeSourceType.Teams, true);
                _teamsSettingTreeService.TransChildrenNodeName(returnNode);

                var containers = returnNode?.Children;
                if (containers.IsNullOrEmpty()) return;
                containers.ForEach(c => c.SourceType = (int)SourceFlag.Teams);
                _containerNodes.AddRange(containers);
            }
            catch (Exception e)
            {
                _logger.Error($"Get all container occur error: {e.ToString()}");
            }
        }
        public void GetUniqueContainers(List<ImportTeamsSettingData> datas)
        {
            try
            {
                var containerSettings = datas.Where(_ => _.SettingLevel == SettingLevel.Container);
                var groupContainers = containerSettings.GroupBy(c => c.ContainerName).ToList();
                var duplicateContainers = groupContainers.Where(g => g.Count() > 1)
                                                    .Select(g => g)
                                                    .ToList();
                foreach (var container in duplicateContainers)
                {
                    _logger.Warn($"The container setting is duplicated in the Excel file: Container name {container.First().ContainerName}");
                    JMImportSPSettingDetail reportDetail = new JMImportSPSettingDetail()
                    {
                        ObjectName = container.First().ContainerName,
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_JS_BCM_ImportSetting_DuplicateContainerNameInExcel",
                    };
                    _reportManager.SendJobDetail(reportDetail);
                    foreach (var containerSetting in container)
                    {
                        datas.Remove(containerSetting);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"GetUniqueContainers occur error: {e.ToString()}");
            }
        }
        private async Task AddCustomSettingAsync(ImportTeamsSettingData data, bool isNewOpus)
        {
            using CheckJobStopScope jScope = new();
            try
            {
                if (!isNewOpus && data.ApprovalType == (int)ManualApprovalType.AutoApprove)
                {
                    _logger.Error("current account is old account and the import file has auto approval,job should failed.");
                    throw new Exception("RM_JM_ImportSetting_ErrorMessage");
                }
                _logger.Info($"Start to process {data.FullUrl}");
                //Get Term
                _logger.Info("Start to get term");
                var (termSet, scopeTerm, defaultTerm) = HandleTerm(data);
                //Manual approval
                var (approvalType, workflowDef, userInfos) = await HandleManualApproval(data);
                //Create setting for container
                var node = await ValidateContainerOrTeamsOrGroup(data);
                //Check term group and term set permission
                await ValidateTermPermissionAsync(TenantLocalValue.LogonUserId, termSet, data, SourceFlag.Teams);

                if (data.SettingLevel == SettingLevel.Container)
                {
                    await CreateSettingForContainerAsync(data, node, termSet, scopeTerm, defaultTerm, approvalType, workflowDef, userInfos);
                    return;
                }
                if (data.SettingLevel == SettingLevel.TeamsOrGroup)
                {
                    await CreateSettingForTeamsOrGroupAsync(data, node, termSet, scopeTerm, defaultTerm, approvalType, workflowDef, userInfos);
                    return;
                }
                await CreateSettingForSiteAsync(data, node, termSet, scopeTerm, defaultTerm, approvalType, workflowDef, userInfos);
            }
            catch (Exception e)
            {
                _result.HasFailed = true;
                _logger.Error($"Import Custom Setting Error:{e.ToString()}");
                GenerateJobDetail(data, JobDetailsStatus.Failed, e.Message);
            }
        }
        private (RMTermSet, RMTerm, RMTerm) HandleTerm(ImportTeamsSettingData data)
        {
            //termGroup
            var termGroup = _termGroupDao.GetTermGroupByName(data.TermGroup);
            if (termGroup is null)
            {
                _logger.Error($"Can not find termGroup.Name:[{data.TermGroup}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoTermGroup");
            }

            switch (_termPermissionType)
            {
                case TermPermissionMethod.All:
                    break;
                case TermPermissionMethod.None:
                    _logger.Error($"Current user don't have permission for term group .Name:[{data.TermGroup}]");
                    throw new Exception("RM_JS_BCM_ImportSetting_NoTermGroupPermission");
                case TermPermissionMethod.SpecifyScope:
                    {
                        if (_termPermissionsDic.ContainsKey(termGroup.UniqueId))
                            break;
                        _logger.Error($"Current user don't have permission for term group .Name:[{data.TermGroup}]");
                        throw new Exception("RM_JS_BCM_ImportSetting_NoTermGroupPermission");
                    }
            }

            //termSet
            var termSet = _termSetDao.GetRMTermSetsByGroupUniqueId(termGroup.UniqueId).FirstOrDefault(t => t.Name.Equals(data.TermSet));
            if (termSet is null)
            {
                _logger.Error($"Can not find termSet. Name:[{data.TermSet}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoTermSet");
            }

            if (_termPermissionType != TermPermissionMethod.All && (!_termPermissionsDic.ContainsKey(termGroup.UniqueId) || !_termPermissionsDic[termGroup.UniqueId].Contains(termSet.UniqueId)))
            {
                _logger.Error($"Current user don't have permission for term set .Name:[{data.TermSet}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoTermSetPermission");
            }
                //termScope
            var scopeTerm = GetScopeTerm(termSet, data.TermScopeRelativePath);
            var defaultTerm = data.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm ? GetDefaultTerm(termSet, scopeTerm == null ? termSet.Id : scopeTerm.Id, data.DefaultTermPath, scopeTerm == null) : null;
            return (termSet, scopeTerm, defaultTerm);
        }
        private RMTerm GetScopeTerm(RMTermSet termSet, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            _logger.Info($"Get term with path:{path}");
            string[] tNames = path.Split(PathSeparator);
            RMTermSetMembership ship = null;
            for (int i = 0; i < tNames.Length; i++)
            {
                int parentId = ship == null ? termSet.Id : ship.TermId;
                _logger.Debug("Get parent membership with id {0}, name {1}", parentId, tNames[i]);
                ship = _termSetMembershipDao.GetByTermNameAndParentId(parentId, tNames[i], ship == null);
            }
            if (ship == null)
            {
                _logger.Error($"Can not find scope term.Path:[{path}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoScopeTerm");
            }
            return _termDao.GetRMTermByTermId(ship.TermId);
        }
        private RMTerm GetDefaultTerm(RMTermSet termSet, int parentId, string path, bool isRootTerm)
        {
            _logger.Info($"Get default term with path:{path}");
            string[] tNames = path.Split(PathSeparator);
            RMTermSetMembership ship = null;
            for (int i = 0; i < tNames.Length; i++)
            {
                int tempParentId = ship == null ? parentId : ship.TermId;
                _logger.Debug("Get parent membership with id {0}, name {1}", parentId, tNames[i]);
                ship = _termSetMembershipDao.GetByTermNameAndParentId(tempParentId, tNames[i], isRootTerm);
                isRootTerm = false;
            }
            if (ship == null)
            {
                _logger.Error($"can not find default term.Path:[{path}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoDefaultTerm");
            }
            return _termDao.GetRMTermByTermId(ship.TermId);
        }
        private async Task<(int, RMWorkflowDefinition, List<ToUserInfo>)> HandleManualApproval(ImportTeamsSettingData data)
        {
            var workflowDef = _importSettingHelper.VerifyManualWorkflow(data);
            var userInfos = await _importSettingHelper.VerifyManualRecordOwnerAsync(data);
            return (data.ApprovalType, workflowDef, userInfos);
        }
        private async Task CreateSettingForSiteAsync(ImportTeamsSettingData data, RMSPSampleTreeNode node, RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm, int approvalType, RMWorkflowDefinition workflowDef, List<ToUserInfo> userInfos)
        {
            try
            {
                var siteCollection = await CheckSiteCollectionInTeamsOrGroup(data, node);
                if (siteCollection == null)
                {
                    throw new Exception("RM_JS_BCM_ImportSetting_SiteNotBelongTermOrGroup");
                }
                var remoteSiteCollection = GetRemoteSite(data.SiteCollectionUrl);
                object aveTeamsObj = _aveContextHelper.FindAveTeamsObject(data.FullUrl, data.SettingLevel, data.SiteCollectionUrl, await _aveContextHelper.GetAveBPOSInfoAsync(remoteSiteCollection));
                if (aveTeamsObj == null)
                {
                    _logger.Error($"Cannot find Teams object with url.");
                    throw new Exception("RM_JS_BCM_ImportSetting_NoSPObject");
                }
                _logger.Info("Load and verify inheritSetting");
                var containerId = siteCollection.Parent.Parent.Parent.Id;
                var inheritSetting = _settingHelper.LoadInheritSeting(aveTeamsObj, remoteSiteCollection, containerId: containerId);
                _importSettingHelper.VerifyFolderTermScope(aveTeamsObj, remoteSiteCollection, data, scopeTerm, containerId);
                _importSettingHelper.VerifyInheritTermSetting(inheritSetting, termSet);
                _importSettingHelper.VerifyFolderTerm(inheritSetting, data, defaultTerm, ref scopeTerm);
                RMSPTreeNode curNode = await _importSettingHelper.CreateNodeAsync(aveTeamsObj, remoteSiteCollection, inheritSetting, data, termSet, scopeTerm, defaultTerm, workflowDef.ReferenceId.ToString(), approvalType, userInfos, siteCollection.Parent, node.Id);
                _logger.Info("Start to save setting");
                var returnMsg = await _rMTeamsSettingsService.AddCustomColumnAsync(curNode);
                if (returnMsg.MessageType == RAMessageType.Successful)
                {
                    _result.HasSuccessful = true;
                }
                else
                {
                    throw new Exception(returnMsg.ErrorMessage);
                }
                GenerateJobDetail(data, JobDetailsStatus.Successful);
                _logger.Info($"Finish processing {data.FullUrl}");
            }
            catch (Exception ex)
            {
                _result.HasFailed = true;
                _logger.Error($"error occured CreateSettingForSiteAsync, error: {ex}]");
                GenerateJobDetail(data, JobDetailsStatus.Failed, ex.Message);
            }
        }
        private async Task<RMSPSampleTreeNode> CheckSiteCollectionInTeamsOrGroup(ImportTeamsSettingData data, RMSPSampleTreeNode teamsOrGroup)
        {
            List<RMSPSampleTreeNode> sites = [];
            if (!_siteInTeamsOrGroupCache.ContainsKey(teamsOrGroup.Name))
            {
                teamsOrGroup.PageSize = int.MaxValue;
                var sitecollectionOfTeamsOrGroup = (await _browseTreeService.BrowseSPOTreeAsync(teamsOrGroup, RMBrowseTreeNodeSourceType.Teams, true)).Children;
                sitecollectionOfTeamsOrGroup[0].PageSize = int.MaxValue;
                var siteOfTeamsOrGroup = (await _browseTreeService.BrowseSPOTreeAsync(sitecollectionOfTeamsOrGroup[0], RMBrowseTreeNodeSourceType.Teams, true)).Children;
                data.TeamsGroupId = _containerNodes.First(x => x.Name == data.ContainerName).SPObjectId;
                sites = siteOfTeamsOrGroup;
                _siteInTeamsOrGroupCache.Add(teamsOrGroup.Name, siteOfTeamsOrGroup);
            }
            else
            {
                sites = _siteInTeamsOrGroupCache.GetValue(teamsOrGroup.Name);
            }
            return sites.FirstOrDefault(x => x.FullPath.Contains(data.SiteCollectionUrl));
        }
        private async Task CreateSettingForContainerAsync(ImportTeamsSettingData data, RMSPSampleTreeNode node, RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm, int approvalType, RMWorkflowDefinition workflowDef, List<ToUserInfo> userInfos)
        {
            try
            {
                _logger.Info($"Start processing container name {data.ContainerName}");
                var nodeTeamsId = data.SettingLevel == SettingLevel.Container ? Guid.Empty : new Guid(node.TeamsId);
                var nodeSetting = _teamsSettingDao.LoadTeamsSetting(new Guid(node.SPObjectId), nodeTeamsId, Guid.Empty);
                if (nodeSetting == null)
                {
                    _logger.Info($"The container setting {data.ContainerName} is null");
                    throw new Exception("RM_JS_BCM_ImportSetting_NotColumnSetting");
                }
                if (string.IsNullOrEmpty(nodeSetting.ColumnName) && !nodeSetting.IsUsingExistColumnName)
                {
                    _logger.Info("The container setting is not set column setting");
                    throw new Exception("RM_JS_BCM_ImportSetting_NotColumnSetting");
                }
                var containerNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(nodeSetting.NodeInfo);
                _importSettingHelper.SetDoclevelSetting(ref containerNode, termSet, scopeTerm, defaultTerm, data.ApplyExisting, data.IncludeDeclaredDoc, data.IsOverwrite, workflowDef.ReferenceId.ToString(), approvalType, userInfos, data.IsSendEmail, data.ApplyTermsOnFolders, (DeployTermMethod)data.DeployTermMethod);
                var message = await _rMTeamsSettingsService.AddGlobalColumnAsync(containerNode);
                if (message.MessageType == RAMessageType.Successful)
                {
                    _result.HasSuccessful = true;
                }
                else
                {
                    throw new Exception(message.ErrorMessage);
                }
                GenerateJobDetail(data, JobDetailsStatus.Successful);
                _logger.Info($"Finish processing {data.ContainerName}");
            }
            catch (Exception ex)
            {
                _result.HasFailed = true;
                _logger.Error($"error occured CreateSettingForContainerAsync, error: {ex}]");
                GenerateJobDetail(data, JobDetailsStatus.Failed, ex.Message);
            }
        }
        private async Task CreateSettingForTeamsOrGroupAsync(ImportTeamsSettingData data, RMSPSampleTreeNode node, RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm, int approvalType, RMWorkflowDefinition workflowDef, List<ToUserInfo> userInfos)
        {
            try
            {
                node.TeamsId = node.Id;
                var inheritSetting = _settingHelper.LoadInheritSeting(null, null, node);
                _importSettingHelper.VerifyInheritTermSetting(inheritSetting, termSet);
                RMSPTreeNode curNode = _importSettingHelper.CreateNodeForTeamsAsync(inheritSetting, node, data, termSet, scopeTerm, defaultTerm, workflowDef.ReferenceId.ToString(), approvalType, userInfos);
                _logger.Info("Start to save setting");
                var returnMsg = await _rMTeamsSettingsService.AddCustomColumnAsync(curNode);
                if (returnMsg.MessageType == RAMessageType.Successful)
                {
                    _result.HasSuccessful = true;
                }
                else
                {
                    throw new Exception(returnMsg.ErrorMessage);
                }
                GenerateJobDetail(data, JobDetailsStatus.Successful);
                _logger.Info($"Finish processing {data.FullUrl}");
            }
            catch (Exception ex)
            {
                _result.HasFailed = true;
                _logger.Error($"error occured CreateSettingForTeamsOrGroupAsync, error: {ex}]");
                GenerateJobDetail(data, JobDetailsStatus.Failed, ex.Message);
            }
        }
        private void GenerateJobDetail(ImportTeamsSettingData data, JobDetailsStatus status, string comment = "")
        {
            var objectName = data.SettingLevel switch
            {
                SettingLevel.Container => data.ContainerName,
                SettingLevel.TeamsOrGroup => data.TeamsOrGroupName,
                _ => data.FullUrl.Substring(data.FullUrl.LastIndexOf(@"/") + 1),
            };
            var detail = new JMImportSPSettingDetail()
            {
                Url = data.FullUrl,
                ObjectName = objectName,
                Status = status,
                Comment = comment
            };
            _reportManager.SendJobDetail(detail);
        }
        private async Task<RMSPSampleTreeNode> ValidateContainerOrTeamsOrGroup(ImportTeamsSettingData data)
        {
            var containers = _containerNodes.Where(_ => _.Name.Equals(data.ContainerName)).ToList();

            if (containers.IsNullOrEmpty()) throw new Exception("RM_JS_BCM_ExportSetting_ContainerNotExist");

            if (containers.Count > 1) throw new Exception("RM_JS_BCM_ImportSetting_DuplicateContainerName");
            if (data.SettingLevel == SettingLevel.Container)
            {
                return containers.First();
            }
            else
            {
                var teamsOrGroups = new List<RMSPSampleTreeNode>();
                if (!_teamOrGroupInContainerCache.ContainsKey(data.ContainerName))
                {
                    containers[0].PageSize = int.MaxValue;
                    var listTeamsOrGroupOfContainer = (await _browseTreeService.BrowseSPOTreeAsync(containers[0], RMBrowseTreeNodeSourceType.Teams, true)).Children;
                    listTeamsOrGroupOfContainer.ForEach(item => item.SourceType = (int)SourceFlag.Teams);
                    //listTeamsOrGroupOfContainer.ForEach(item => item.Parent = containers[0]);
                    _teamOrGroupInContainerCache.Add(data.ContainerName, listTeamsOrGroupOfContainer);
                    teamsOrGroups = listTeamsOrGroupOfContainer.Where(item => item.Name.Equals(data.TeamsOrGroupName)).ToList();
                }
                else
                {
                    teamsOrGroups = _teamOrGroupInContainerCache.GetValue(data.ContainerName).Where(item => item.Name == data.TeamsOrGroupName).ToList();
                }

                if (teamsOrGroups.IsNullOrEmpty()) throw new Exception("RM_JS_BCM_ImportSetting_TermGroupNotExist");

                return teamsOrGroups[0];
            }
        }
        private RemoteSiteCollection GetRemoteSite(string siteCollectionUrl)
        {
            RemoteSiteCollection site = null;
            if (!_remoteSiteCollectionCache.TryGetValue(siteCollectionUrl, out site))
            {
                site = RABrowserClient.GetRemoteSiteCollectionByUrl(siteCollectionUrl);
                if (site == null)
                {
                    _logger.Warn($"Can not find sitecollection.Url:{siteCollectionUrl}");
                    throw new Exception("RM_JS_BCM_ImportSetting_NoSC");
                }
                _remoteSiteCollectionCache.Add(siteCollectionUrl, site);
            }
            return site;
        }
        private async Task ValidateTermPermissionAsync(string userId, RMTermSet termSet, ImportTeamsSettingData data, SourceFlag sourceFlag)
        {
            var containers = _containerNodes.Where(_ => _.Name.Equals(data.ContainerName)).ToList();
            var termGroup = _termGroupDao.GetTermGroupByName(data.TermGroup);

            var filterOption = new FilterTermObjOption
            {
                NeedCheckPermission = true,
                FilterByContentSource = true,
                ExcludeBuiltIn = true,
                ForPhysicalView = true,
                SourceFlag = sourceFlag,
                ContainerId = containers.First().SPObjectId,
            };

            var permissionTermGroup = await DoesUserHasPermisionToTermAsync(userId, SecurityTermLevel.TermGroup, [termGroup.UniqueId], filterOption);
            if (!permissionTermGroup)
            {
                _logger.Error($"Current user don't have permission for term group .Name:[{termGroup.Name}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoTermGroupPermission");
            }

            var permissionTermSet = await DoesUserHasPermisionToTermAsync(userId, SecurityTermLevel.TermSet, [termSet.UniqueId], filterOption);
            if (!permissionTermSet)
            {
                _logger.Error($"Current user don't have permission for term set .Name:[{termSet.Name}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoTermSetPermission");
            }
        }
        private async Task<bool> DoesUserHasPermisionToTermAsync(string userId, SecurityTermLevel level, List<Guid> termObjIds, FilterTermObjOption filterOption)
        {
            var hasPermission = false;
            try
            {
                if (termObjIds != null && termObjIds.Count > 0)
                {
                    var userAndGroupIds = await _userSerive.GetUserAndGroupUserIdsAsync(userId);
                    QuerySecurityTermObjDto dto = new QuerySecurityTermObjDto
                    {
                        Level = level,
                        UserAndGroupIds = userAndGroupIds,
                        FilterByContentSource = filterOption.FilterByContentSource,
                        ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                        ContainerId = filterOption.ContainerId,
                        SourceFlag = filterOption.SourceFlag,
                        ForPhysicalView = filterOption.ForPhysicalView,
                    };
                    hasPermission = _rMSecurityGroupDao.DoesUserHasPermisionToTerm(termObjIds, dto);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"An error while check termobj permission for user, termObjId:{string.Join(";", termObjIds)}, level: {level} message: {ex}");
            }
            return hasPermission;
        }
    }

}
