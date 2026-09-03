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
using AngleSharp.Dom;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common.Setting;
using AvePoint.RA.SharePoint.Common.Setting.Model;
using AvePoint.RA.SharePoint.Teams.ColumnSetting.ImportTeamsSetting.Model;
using AvePoint.Wrapper.Common;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Teams.ColumnSetting.ImportTeamsSetting.Helper
{
    public class ImportSettingHelper
    {
        private const Char PathSeparator = '|';
        private const string NoManualSetting = "no manual setting";
        private const string WorkflowProcess = "workflow process";
        private const string RecordOwner = "record owner";
        private const string AutoApprove = "skip manual review for this location";
        private const string ManuallyChooseATerm = "manually choose a term";
        private const string SetADefaultTerm = "set a default term";
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(ImportSettingHelper));
        private readonly ITeamsSettingTreeService _teamsSettingTreeService = PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private readonly IBrowseTreeService _browseTreeService = PlatformWindsorManager.GetService<IBrowseTreeService>();
        private readonly ITermGroupDao _termGroupDao = PlatformWindsorManager.GetService<ITermGroupDao>();
        private readonly ITermSetDao _termSetDao = PlatformWindsorManager.GetService<ITermSetDao>();
        private readonly ITermDao _termDao = PlatformWindsorManager.GetService<ITermDao>();
        private readonly ITermSetMembershipDao _termSetMembershipDao = PlatformWindsorManager.GetService<ITermSetMembershipDao>();
        public IAccountWrapperService _accountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();
        public IUserService _userSerive => PlatformWindsorManager.GetService<IUserService>();
        private IRMTeamsSettingsService _rMTeamsSettingsService => PlatformWindsorManager.GetService<IRMTeamsSettingsService>();
        private IRMWorkflowDefinitionDao _rMWorkflowDefinitionDao => PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();
        private readonly SettingHelper _settingHelper = new SettingHelper();
        private readonly AveContextHelper _aveContextHelper = new AveContextHelper();
        public string GetImportJobCSVFile(string csvPath)
        {
            try
            {
                return JobReportUtility.GetImportJobCSVFile(csvPath);
            }
            catch (Exception e)
            {
                _logger.Error("can not download file:{0},error:{1}", csvPath, e.ToString());
                throw;
            }
        }

        public ImportTeamsSettingData ConvertPathObject(string[] data, JMImportSPSettingDetail detail)
        {
            ImportTeamsSettingData obj = new ImportTeamsSettingData()
            {
                ContainerName = data[0],
                TeamsOrGroupName = WebUtil.UrlDecode(data[1]),
                SiteCollectionUrl = WebUtil.UrlDecode(data[2]),
                SitePath = WebUtil.UrlDecode(data[3]),
                ListPath = WebUtil.UrlDecode(data[4]),
                FolderPath = WebUtil.UrlDecode(data[5]),
            };
            SettingLevel currentSettingLevel = SettingLevel.None;
            obj.FullUrl = GetFullUrlAndLevel(obj, ref currentSettingLevel);
            obj.SettingLevel = currentSettingLevel;
            return obj;
        }

        public ImportTeamsSettingData ConvertToSettingObject(string[] data, JMImportSPSettingDetail detail)
        {
            ImportTeamsSettingData obj = new ImportTeamsSettingData()
            {
                ContainerName = data[0],
                TeamsOrGroupName = WebUtil.UrlDecode(data[1]),
                SiteCollectionUrl = WebUtil.UrlDecode(data[2]),
                SitePath = WebUtil.UrlDecode(data[3]),
                ListPath = WebUtil.UrlDecode(data[4]),
                FolderPath = WebUtil.UrlDecode(data[5]),
                DeployTermMethod = GetDeployTermMethod(data[6]),
                TermScopePath = ReplaceFullAngleString(data[7]),
                DefaultTermPath = ReplaceFullAngleString(data[8]),
                ApplyExisting = GetBoolColumnValue(data.ElementAtOrDefault(9)),
                IncludeDeclaredDoc = GetBoolColumnValue(data.ElementAtOrDefault(10)),
                ApplyTermsOnFolders = GetBoolColumnValue(data.ElementAtOrDefault(11)),
                IsOverwrite = GetBoolColumnValue(data.ElementAtOrDefault(12)),
                ApprovalType = string.IsNullOrEmpty(data[13]) ? (int)ManualApprovalType.InheritParent : GetManualApprovalType(ReplaceFullAngleString(data[13])),
                WorkflowName = string.IsNullOrEmpty(data[14]) ? string.Empty : data[14],
                IsSendEmail = GetBoolColumnValue(data.ElementAtOrDefault(15)),
            };

            SettingLevel currentSettingLevel = SettingLevel.None;
            obj.FullUrl = GetFullUrlAndLevel(obj, ref currentSettingLevel);
            obj.SettingLevel = currentSettingLevel;

            detail.ObjectName = GetObjectName(obj);
            string[] names = obj.TermScopePath.Split(PathSeparator);
            if (string.IsNullOrEmpty(obj.TermScopePath))
            {
                detail.Url = obj.FullUrl;
                throw new Exception("RM_JS_BCM_ImportSetting_TermScopeError");

            }

            if (names.Length < 2)
            {
                detail.Url = obj.FullUrl;
                throw new Exception("RM_JS_BCM_ImportSetting_TermScopeFormatError");
            }

            if (string.IsNullOrEmpty(obj.DefaultTermPath) && obj.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
            {
                detail.Url = obj.FullUrl;
                throw new Exception("RM_JS_BCM_ImportSetting_TermEmpty");
            }
            obj.TermGroup = names[0];
            obj.TermSet = names[1];
            if (names.Length > 2)
            {
                obj.TermScopeRelativePath = obj.TermScopePath.Substring(obj.TermScopePath.IndexOf(obj.TermSet) + obj.TermSet.Length + 1);
            }

            return obj;
        }
        private string GetObjectName(ImportTeamsSettingData data)
        {
            return data.SettingLevel switch
            {
                SettingLevel.Container => data.ContainerName,
                SettingLevel.TeamsOrGroup => data.TeamsOrGroupName,
                _ => data.SiteCollectionUrl.Substring(data.SiteCollectionUrl.LastIndexOf(@"/") + 1),
            };
        }
        public ToUserInfo ConvertUserInfo(AOSUserDto user)
        {
            return new ToUserInfo()
            {
                UserId = user.UserId,
                UserName = user.UserName,
                UserPrincipalName = user.UserPrincipalName,
                Email = user.Email,
                DisplayName = user.DisplayName,
                InviteType = user.InviteType,
                RMUserId = user.RMUserId,
                Id = user.Id,
                SurName = user.SurName,
                GivenName = user.UserName,
                TenantId = user.TenantId
            };
        }
        private static int GetDeployTermMethod(string DeployTerm)
        {
            int deployTerm = 0;
            switch (DeployTerm.ToLowerInvariant())
            {
                case ManuallyChooseATerm:
                    deployTerm = (int)DeployTermMethod.NoDefaultTerm;
                    break;
                case SetADefaultTerm:
                    deployTerm = (int)DeployTermMethod.UseDefaultTerm;
                    break;
            }
            return deployTerm;
        }
        public void SetDoclevelSetting(ref RMSPTreeNode node, RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm,
            bool applyExisting, bool includeDeclared, bool isOverwrite, string workflowId, int approvalType, List<ToUserInfo> userInfos, bool isSendEmail, bool applyTermsOnFolders, DeployTermMethod deployTermMethod)
        {
            node.TeamsId = (node.Level == (int)NodeLevel.Office365GroupEntire) ? node.Id : node.TeamsId;
            node.TermSetId = termSet.UniqueId;
            node.TermSetName = termSet.Name;
            node.TermId = scopeTerm != null ? scopeTerm.UniqueId : Guid.Empty;
            node.TermName = scopeTerm != null ? scopeTerm.Name : string.Empty;
            node.DefaultTermId = defaultTerm != null ? defaultTerm.UniqueId : Guid.Empty;
            node.DefaultTermName = defaultTerm != null ? defaultTerm.Name : string.Empty;
            node.DeployTermMethod = deployTermMethod;
            node.EnableRecordManagement = (int)EnableRecordManagementSetting.Enable;
            node.NeedCheckDefaultValue = applyExisting;
            node.ApplyTermIncludeFolder = applyTermsOnFolders;
            if (applyExisting)
            {
                node.IncludeDeclaredRecords = includeDeclared;
            }

            if (applyExisting || applyTermsOnFolders)
            {
                node.ApplyExistType = isOverwrite ? (int)ApplyExistingTermType.OverWrite : (int)ApplyExistingTermType.SkipAndKeep;
            }
            if (approvalType != (int)ManualApprovalType.InheritParent)
            {
                node.ApprovalType = (int)ManualApprovalType.NoManualSetting;
            }
            if (approvalType == (int)ManualApprovalType.ManualWorkflowProcess && workflowId != string.Empty)
            {
                node.WorkflowReferenceId = workflowId;
                node.ApprovalType = approvalType;
                node.EMailToRecordOwner = isSendEmail;
            }
            else if (approvalType == (int)ManualApprovalType.ReocrdOwner && userInfos.Count != 0)
            {
                node.ApprovalType = approvalType;
                node.RecordOwner = userInfos;
                node.EMailToRecordOwner = isSendEmail;
            }
            else if (approvalType == (int)ManualApprovalType.AutoApprove)
            {
                node.ApprovalType = approvalType;
            }
        }
        public RMWorkflowDefinition VerifyManualWorkflow(ImportTeamsSettingData data)
        {
            var approvalType = data.ApprovalType;
            var workflowDef = new RMWorkflowDefinition();
            if (approvalType == 1)
            {
                workflowDef = _rMWorkflowDefinitionDao.GetWorkflowByName(data.WorkflowName);
                if (workflowDef == null)
                {
                    throw new Exception("RM_JS_BCM_ImportSetting_NoWrokflow");
                }
            }
            return workflowDef;
        }
        public async Task<List<ToUserInfo>> VerifyManualRecordOwnerAsync(ImportTeamsSettingData settingObj)
        {
            var approvalType = settingObj.ApprovalType;
            var userNames = settingObj.WorkflowName.Split('|').ToList().ConvertAll(user => user.ToLowerInvariant()).Distinct().ToList();
            var finalUsers = new List<ToUserInfo>();
            var dbUsers = new List<AOSUserDto>();
            var failedUsers = new List<string>();
            if (approvalType == 2)
            {
                if (settingObj.WorkflowName == string.Empty)
                {
                    throw new Exception("RM_JS_BCM_ImportSetting_NoUsers");
                }
                if (userNames.Count == 0)
                {
                    throw new Exception("RM_JS_BCM_ImportSetting_NoUsers");
                }
                dbUsers = await _userSerive.SearchUsersAsync(userNames);
                finalUsers.AddRange(dbUsers.ConvertAll(user => ConvertUserInfo(user)));
                if (dbUsers.Count < userNames.Count)
                {
                    var adUsers = new List<ToUserInfo>();
                    var dbUserNames = dbUsers.Select(user => user.UserPrincipalName.ToLowerInvariant()).ToList();
                    var needFindUsers = userNames.Where(user => !dbUserNames.Contains(user)).ToList();
                    foreach (var fuser in needFindUsers)
                    {
                        if (!fuser.Contains('@'))
                        {
                            failedUsers.Add(fuser);
                            continue;
                        }
                        var accountsFromAD = _accountWrapperService.SearchAccounts(TenantLocalValue.LogonGroupId, fuser, 20, false);
                        var userFromAD = accountsFromAD.Where(u => u.UserPrincipalName.ToLowerInvariant() == fuser).FirstOrDefault();
                        if (userFromAD == null)
                        {
                            failedUsers.Add(fuser);
                            continue;
                        }
                        var userInfo = ConvertUserInfo(AADAccount.Convert2AOSUserDto(userFromAD));
                        adUsers.Add(userInfo);
                    }
                    await _rMTeamsSettingsService.SyncADUsersAsync(adUsers);
                    _logger.Info("Sync ad users to db sucess.");
                    finalUsers.AddRange(adUsers);
                }

                if (finalUsers.Count == 0)
                {
                    throw new Exception(string.Format(I18NEntity.GetString("RM_JS_BCM_ImportSetting_FailedUsers"), string.Join("|", userNames)));
                }
                if (finalUsers.Count != userNames.Count)
                {
                    throw new Exception(string.Format(I18NEntity.GetString("RM_JS_BCM_ImportSetting_FailedUsers"), string.Join("|", failedUsers)));
                }
            }
            return finalUsers;
        }
        public void VerifyInheritTermSetting(RMTeamsSetting inheritSetting, RMTermSet termSet)
        {
            if (inheritSetting == null)
            {
                _logger.Error(I18NEntity.GetString("RM_JS_BCM_ImportSetting_NoGroupSetting"));
                throw new Exception("You have not configured document level term settings on the teams/group node in the system");
            }
            else
            {
                if (!HasDocumentLevelSetting(inheritSetting))
                {
                    _logger.Error($"Inherit setting does not have document level setting.");
                    throw new Exception("RM_JS_BCM_ImportSetting_NoGroupSetting");
                }
                else if (!IsSameTermGroup(inheritSetting.TermSetId, termSet))
                {
                    _logger.Error($"Current term group is not same with inherit setting term group.");
                    throw new Exception("RM_JS_BCM_ImportSetting_DifferentTermGroup");
                }
            }
        }
        private bool HasDocumentLevelSetting(RMTeamsSetting setting)
        {
            if ((setting.IsUsingExistColumnName && setting.SetDocLevelTermForExistColumn && setting.TermSetId != Guid.Empty)
                || (!setting.IsUsingExistColumnName && setting.TermSetId != Guid.Empty))
            {
                return true;
            }
            return false;
        }
        private bool IsSameTermGroup(Guid inheritTermSetId, RMTermSet curTermSet)
        {
            RMTermSet inheritTermSet = _termSetDao.GetRMTermSetByGuid(inheritTermSetId);
            if (inheritTermSet.TermGroupId == curTermSet.TermGroupId)
            {
                return true;
            }
            return false;
        }

        public void VerifyFolderTerm(RMTeamsSetting inheritSetting, ImportTeamsSettingData settingObj, RMTerm defaultTerm, ref RMTerm scopeTerm)
        {
            if (settingObj.SettingLevel == SettingLevel.Folder)
            {
                if (inheritSetting.TermId != Guid.Empty)
                {
                    _logger.Info($"Current object is folder, so get inherit term scope. Url:[{settingObj.FullUrl}]");
                    scopeTerm = _termDao.GetRMTermByGuId(inheritSetting.TermId);

                    if (!IsTermInTermScope(defaultTerm.Id, scopeTerm.Id))
                    {
                        _logger.Error("Current term is not in term scope");
                        throw new Exception("RM_JS_BCM_ImportSetting_TermNotInScope");
                    }
                }
            }
        }
        private bool IsTermInTermScope(int termId, int scopeTermId)
        {
            string scopeTermPath = string.Empty;
            var scopeMemberShip = _termSetMembershipDao.GetMembershipByTermId(scopeTermId);
            if (scopeMemberShip != null)
            {
                scopeTermPath = scopeMemberShip.Path;
            }

            var termPath = string.Empty;
            var termMemberShip = _termSetMembershipDao.GetMembershipByTermId(termId);
            if (termMemberShip != null)
            {
                termPath = termMemberShip.Path;
            }

            if (termPath.StartsWith(scopeTermPath))
            {
                return true;
            }
            return false;
        }

        public void VerifyFolderTermScope(object aveSPObj, RemoteSiteCollection remoteSC, ImportTeamsSettingData data, RMTerm scopeTerm, string containerId)
        {
            if (data.SettingLevel == SettingLevel.Folder)
            {
                var folder = (IAveFolder)aveSPObj;
                if (folder == null)
                {
                    _logger.Error(I18NEntity.GetString("RM_JS_BCM_ImportSetting_NoGroupSetting"));
                    throw new Exception("RM_JS_BCM_ImportSetting_NoGroupSetting");
                }
                

                var inherParentSetting = _settingHelper.LoadInheritSeting(folder.ParentList, remoteSC, containerId: containerId);
                if (inherParentSetting != null)
                {
                    var isTermSetScope = scopeTerm == null;
                    if (isTermSetScope && inherParentSetting.TermId != Guid.Empty || !isTermSetScope && inherParentSetting.TermId != scopeTerm.UniqueId)
                    {
                        throw new Exception("RM_BCM_IS_Msg_FailedToVerifyTermScope");
                    }
                }
            }
        }
        public async Task<RMSPTreeNode> CreateNodeAsync(object aveSPObj, RemoteSiteCollection remoteSC, RMTeamsSetting inheritSetting, ImportTeamsSettingData settingObj,
    RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm, string workflowId, int approvalType, List<ToUserInfo> userInfos, RMSPSampleTreeNode parentSiteCollection, string teamsId)
        {
            RMSPTreeNode curNode = null;
            Guid spObjectId = _aveContextHelper.GetAveObjId(aveSPObj, remoteSC);
            if (inheritSetting.ScopeId == spObjectId)
            {
                curNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(inheritSetting.NodeInfo);
                SetDoclevelSetting(ref curNode, termSet, scopeTerm, defaultTerm, settingObj.ApplyExisting, settingObj.IncludeDeclaredDoc, settingObj.IsOverwrite, workflowId, approvalType, userInfos, settingObj.IsSendEmail, settingObj.ApplyTermsOnFolders, (DeployTermMethod)settingObj.DeployTermMethod);
            }
            else
            {
                RMSPTreeNode inheritNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(inheritSetting.NodeInfo);
                var bposInfo = await _aveContextHelper.CreateBposInfoAsync(remoteSC);
                NodeLevel treeNodeLevel = NodeLevel.Undefined;
                string title = _aveContextHelper.GetAveObjTitleAndLevel(aveSPObj, ref treeNodeLevel);
                curNode = ConstructTreeNode(inheritNode, settingObj, title, treeNodeLevel, spObjectId, termSet, scopeTerm, defaultTerm, bposInfo, workflowId, approvalType, userInfos, teamsId);
                await _aveContextHelper.CreateParentNodesAsync(aveSPObj, inheritNode, remoteSC, curNode, parentSiteCollection);
            }
            return curNode;
        }
        public RMSPTreeNode CreateNodeForTeamsAsync(RMTeamsSetting inheritSetting, RMSPSampleTreeNode teamOrGroup,ImportTeamsSettingData settingObj,
    RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm, string workflowId, int approvalType, List<ToUserInfo> userInfos)
        {
            RMSPTreeNode curNode = null;
            if (inheritSetting.ScopeId == new Guid(teamOrGroup.Id))
            {
                curNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(inheritSetting.NodeInfo);
                SetDoclevelSetting(ref curNode, termSet, scopeTerm, defaultTerm, settingObj.ApplyExisting, settingObj.IncludeDeclaredDoc, settingObj.IsOverwrite, workflowId, approvalType, userInfos, settingObj.IsSendEmail, settingObj.ApplyTermsOnFolders, (DeployTermMethod)settingObj.DeployTermMethod);
            }
            else
            {
                RMSPTreeNode inheritNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(inheritSetting.NodeInfo);
                NodeLevel treeNodeLevel = NodeLevel.Office365GroupEntire;
                curNode = ConstructTreeNode(inheritNode, settingObj, teamOrGroup.Name, treeNodeLevel, new Guid(teamOrGroup.Id), termSet, scopeTerm, defaultTerm, null, workflowId, approvalType, userInfos, teamOrGroup.Id);
                curNode.Parent = RMDtoConverter.ConvertSPTree2RMTree(RMDtoConverter.ConvertRMSampleTree2SPTree(teamOrGroup.Parent));
            }
            return curNode;
        }
        private RMSPTreeNode ConstructTreeNode(RMSPTreeNode inheritNode, ImportTeamsSettingData settingObj, string title,
           NodeLevel level, Guid spObjectId, RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm, BposInfo bposInfo,
           string workflowId, int approvalType, List<ToUserInfo> userInfos, string teamsId)
        {
            RMSPTreeNode currentNode = new RMSPTreeNode();
            #region 继承的属性
            currentNode.SiteGroupId = inheritNode.SiteGroupId;
            currentNode.ColumnName = inheritNode.ColumnName;
            currentNode.Description = inheritNode.Description;
            currentNode.IsUsingExistColumnName = inheritNode.IsUsingExistColumnName;
            currentNode.SetDocLevelTermForExistColumn = inheritNode.SetDocLevelTermForExistColumn;
            currentNode.ExistColumnName = inheritNode.ExistColumnName;
            currentNode.EnableRelatedRecords = inheritNode.EnableRelatedRecords;
            currentNode.isEnableClassification = inheritNode.isEnableClassification;
            currentNode.TermNameOfContainer = inheritNode.TermNameOfContainer;
            currentNode.TermIdOfContainer = inheritNode.TermIdOfContainer;
            currentNode.TermNameOfContainer = inheritNode.TermNameOfContainer;
            currentNode.EMailToRecordOwner = inheritNode.EMailToRecordOwner;
            currentNode.IsDisplyaTermPath = inheritNode.IsDisplyaTermPath;
            currentNode.ColumnRequired = inheritNode.ColumnRequired;
            currentNode.ColumnHidden = inheritNode.ColumnHidden;
            currentNode.IsSyncData = inheritNode.IsSyncData;
            currentNode.RecordOwner = inheritNode.RecordOwner;
            currentNode.ApprovalType = inheritNode.ApprovalType;
            currentNode.WorkflowReferenceId = inheritNode.WorkflowReferenceId;
            currentNode.RecordOwner = inheritNode.RecordOwner;
            currentNode.TeamsId = teamsId;
            #endregion

            currentNode.BposInfo = bposInfo;
            currentNode.FullPath = settingObj.FullUrl;
            currentNode.Level = (int)level;
            currentNode.Id = spObjectId.ToString();
            currentNode.SPObjectId = spObjectId.ToString();

            switch (level)
            {
                case NodeLevel.Folder:
                case NodeLevel.List:
                case NodeLevel.Office365GroupEntire:
                    currentNode.Name = title;
                    break;
                case NodeLevel.Site:
                    if (settingObj.SettingLevel == SettingLevel.RootWeb)
                    {
                        currentNode.Name = ".";
                        currentNode.Title = title;
                    }
                    else
                    {
                        currentNode.Name = title;
                    }
                    break;
                case NodeLevel.SiteCollection:
                    currentNode.Name = settingObj.FullUrl;
                    break;
                default:
                    break;
            }
            SetDoclevelSetting(ref currentNode, termSet, scopeTerm, defaultTerm, settingObj.ApplyExisting, settingObj.IncludeDeclaredDoc, settingObj.IsOverwrite, workflowId, approvalType, userInfos, settingObj.IsSendEmail, settingObj.ApplyTermsOnFolders, (DeployTermMethod)settingObj.DeployTermMethod);

            return currentNode;
        }
        private static int GetManualApprovalType(string ApprovalType)
        {
            int manualApprovalType = 0;
            switch (ApprovalType.ToLowerInvariant())
            {
                case NoManualSetting:
                    manualApprovalType = (int)ManualApprovalType.NoManualSetting;
                    break;
                case WorkflowProcess:
                    manualApprovalType = (int)ManualApprovalType.ManualWorkflowProcess;
                    break;
                case RecordOwner:
                    manualApprovalType = (int)ManualApprovalType.ReocrdOwner;
                    break;
                case AutoApprove:
                    manualApprovalType = (int)ManualApprovalType.AutoApprove;
                    break;
            }
            return manualApprovalType;
        }
        private string ReplaceFullAngleString(string sourceStr)
        {
            if (!string.IsNullOrEmpty(sourceStr) && (sourceStr.Contains("&") || sourceStr.Contains("\"")))
            {
                return sourceStr.Replace('&', '＆').Replace('"', '＂');
            }
            return sourceStr;
        }
        private bool GetBoolColumnValue(string value)
        {
            bool result;
            if (!bool.TryParse(value, out result))
            {
                result = false;
            }
            return result;
        }
        private string GetFullUrlAndLevel(ImportTeamsSettingData obj, ref SettingLevel currentLevel)
        {
            string fullPath = string.Empty;

            if (string.IsNullOrEmpty(obj.TeamsOrGroupName))
            {
                currentLevel = SettingLevel.Container;
                fullPath = obj.ContainerName;
            }
            else if (string.IsNullOrEmpty(obj.SiteCollectionUrl))
            {
                currentLevel = SettingLevel.TeamsOrGroup;
                fullPath = obj.TeamsOrGroupName;
            }
            else if (string.IsNullOrEmpty(obj.SitePath))
            {
                currentLevel = SettingLevel.SiteCollection;
                fullPath = obj.SiteCollectionUrl;
            }
            else if (string.IsNullOrEmpty(obj.ListPath))
            {
                if (obj.SitePath.Equals("."))
                {
                    currentLevel = SettingLevel.RootWeb;
                    fullPath = obj.SiteCollectionUrl;
                }
                else
                {
                    currentLevel = SettingLevel.SubWeb;
                    fullPath = $"{obj.SiteCollectionUrl}/{obj.SitePath}";
                }
            }
            else if (string.IsNullOrEmpty(obj.FolderPath))
            {
                currentLevel = SettingLevel.List;
                fullPath = obj.SitePath.Equals(".") ? $"{obj.SiteCollectionUrl}/{obj.ListPath}" : $"{obj.SiteCollectionUrl}/{obj.SitePath}/{obj.ListPath}";
            }
            else
            {
                currentLevel = SettingLevel.Folder;
                fullPath = obj.SitePath.Equals(".") ?
                            $"{obj.SiteCollectionUrl}/{obj.ListPath}/{obj.FolderPath}" :
                            $"{obj.SiteCollectionUrl}/{obj.SitePath}/{obj.ListPath}/{obj.FolderPath}";
            }
            return fullPath;
        }
    }
}
