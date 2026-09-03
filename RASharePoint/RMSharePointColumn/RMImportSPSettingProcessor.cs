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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Object;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class RMImportSPSettingProcessor
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMImportSPSettingProcessor));
        private string mCsvPath;
        private JobResult Result;
        private string commomErrorMessage = "RM_TS_SS_Summary";
        private Dictionary<string, AveObjectModelFactory> mFactoryCache;
        private Dictionary<string, IAveSite> mAveSiteCache;
        private Dictionary<string, IAveWeb> mAveWebCache;
        private Dictionary<string, RemoteSiteCollection> mRemoteSCCache;
        private Dictionary<string, AveBPOSAccountInfo> mAveBposInfoCache;
        private Dictionary<string, List<string>> mContainerCache;
        private bool isCSVContainsIllegalCharacters;
        private string illegalCharactersErrorMessage;
        private ITermDao mTermDao;
        private const Char PathSeparator = '|';
        private const string NoManualSetting = "no manual setting";
        private const string WorkflowProcess = "workflow process";
        private const string RecordOwner = "record owner";
        private const string AutoApprove = "skip manual review for this location"; 
        private const string ManuallyChooseATerm = "manually choose a term";
        private const string SetADefaultTerm = "set a default term";
        private List<RMSPSampleTreeNode> ContainerNodes;
        private Dictionary<Guid, List<Guid>> termPermissionsDic;
        private TermPermissionMethod termPermissionType;
        public ITermDao TermDAO
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mTermDao;
            }
        }
        private ITermSetDao mTermSetDao;
        public ITermSetDao TermSetDAO
        {
            get
            {
                if (mTermSetDao == null)
                {
                    mTermSetDao = (ITermSetDao)PlatformWindsorManager.GetService(typeof(ITermSetDao));
                }
                return mTermSetDao;
            }
        }
        private ITermGroupDao mTermGroupDAO;
        public ITermGroupDao TermGroupDAO
        {
            get
            {
                if (mTermGroupDAO == null)
                {
                    mTermGroupDAO = (ITermGroupDao)PlatformWindsorManager.GetService(typeof(ITermGroupDao));
                }
                return mTermGroupDAO;
            }
        }
        private ITermSetMembershipDao mTermSetMembershipDAO;
        public ITermSetMembershipDao TermSetMembershipDAO
        {
            get
            {
                if (mTermSetMembershipDAO == null)
                {
                    mTermSetMembershipDAO = (ITermSetMembershipDao)PlatformWindsorManager.GetService(typeof(ITermSetMembershipDao));
                }
                return mTermSetMembershipDAO;
            }
        }
        private ISharePointSettingDao mSharePointSettingDao;
        public ISharePointSettingDao SharePointSettingDao
        {
            get
            {
                if (mSharePointSettingDao == null)
                {
                    mSharePointSettingDao = (ISharePointSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointSettingDao));
                }
                return mSharePointSettingDao;
            }
        }
        private IRMSharePointSettingsService mRMSPSettingsService;
        private IRMSharePointSettingsService RMSPSettingsService
        {
            get
            {
                if (mRMSPSettingsService == null)
                {
                    mRMSPSettingsService = (IRMSharePointSettingsService)PlatformWindsorManager.GetService(typeof(IRMSharePointSettingsService));
                }
                return mRMSPSettingsService;
            }
        }

        protected IRMReportManager mReportManager;
        protected IRMReportManager ReportManager
        {
            get
            {
                if (mReportManager == null)
                {
                    mReportManager = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManager;
            }
        }
        private IRMWorkflowDefinitionDao RMWorkflowDefinitionDao => PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();

        public IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();

        public IUserService UserSerive => PlatformWindsorManager.GetService<IUserService>();

        private IRMSecurityGroupDao RMSecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IRMSharePointSettingsService RMSPSService => PlatformWindsorManager.GetService<IRMSharePointSettingsService>();
        private readonly IBrowseTreeService BrowseTreeService = PlatformWindsorManager.GetService<IBrowseTreeService>();
        private readonly ISPSettingTreeService SPSettingTreeService = PlatformWindsorManager.GetService<ISPSettingTreeService>();
        public ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IMultiGeoDataCenterService mMultiGeoDataCenterService;
        private IMultiGeoDataCenterService MultiGeoDataCenterService
        {
            get
            {
                return mMultiGeoDataCenterService ??= PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
            }
        }
        public RMImportSPSettingProcessor(RMImportSPSettingJobMessage jobMsg)
        {
            mFactoryCache = new Dictionary<string, AveObjectModelFactory>();
            mAveSiteCache = new Dictionary<string, IAveSite>();
            mAveWebCache = new Dictionary<string, IAveWeb>();
            mRemoteSCCache = new Dictionary<string, RemoteSiteCollection>();
            mAveBposInfoCache = new Dictionary<string, AveBPOSAccountInfo>();
            mContainerCache = new Dictionary<string, List<string>>();
            ContainerNodes = new List<RMSPSampleTreeNode>();
            ReportMangerFactory.Instance.Init(jobMsg.JobID, jobMsg.JobType);
            Result = new JobResult();
            try
            {
                mCsvPath = JobReportUtility.GetImportJobCSVFile(jobMsg.CSVPath);
            }
            catch (Exception e)
            {
                logger.Error("can not download file:{0},error:{1}", jobMsg.CSVPath, e.ToString());
                throw;
            }

            
            ReportManager.IncreaseBase(10);
            ReportManager.StartUpdateJobProgress();
        }
        public async System.Threading.Tasks.Task ImportCustomSettingAsync()
        {
            JobStatus status = JobStatus.None;
            try
            {
                var currentUser = (await UserSerive.SearchUsersAsync(new List<string> { TenantLocalValue.LogonUserEmail })).FirstOrDefault();
                (termPermissionType, termPermissionsDic) = RMSecurityGroupDao.GetTermGroupIdUserScopePermission(currentUser?.UserId);
                await GetAllContainers();
                //读CSV
                List<RMImportSettingObject> settingObjects = ReadCsv(mCsvPath);
                if (isCSVContainsIllegalCharacters)
                {
                    Result.HasFailed = true;
                    commomErrorMessage = illegalCharactersErrorMessage;
                }
                else
                {
                    ReportManager.IncreaseBase(settingObjects.Count);
                    settingObjects = settingObjects.OrderBy(_ => _.ContainerName.Length).ThenBy(_ => _.SiteCollectionUrl.Length).ToList();
                    bool isNewOpus = await LicenseHelperService.IsNewOpus();
                    //导入
                    foreach (var settingObj in settingObjects)
                    {
                        await AddCustomSettingAsync(settingObj, isNewOpus);
                        ReportManager.Increase(1);
                    }
                }
            }
            catch (Exception e)
            {
                Result.HasFailed = true;
                logger.Error($"ImportCustomSetting Error:{e.ToString()}");
            }
            finally
            {
                status = Result.HasFailed
                    ? Result.HasSuccessful
                        ? JobStatus.FinishWithException
                        : JobStatus.Failed
                    : JobStatus.Finished;
                string jobComment = (status == JobStatus.FinishWithException || status == JobStatus.Failed)
                    ? commomErrorMessage
                    : string.Empty;
                ReportManager.SetJobFinished(status, jobComment);
                try
                {
                    System.IO.File.Delete(this.mCsvPath);
                }
                catch (Exception e)
                {
                    logger.Warn($"Delete csvs error:{e.ToString()}");
                }
                DisposeWebCache();
                DisposeSiteCache();
            }
        }

        private async Task<AveBPOSAccountInfo> GetAveBPOSInfoAsync(RemoteSiteCollection sc)
        {
            AveBPOSAccountInfo result = null;
            if (!mAveBposInfoCache.TryGetValue(sc.id,out result))
            {
                result = await PoolUserUtil.GetBPOSInfoAsync(sc);
                if (result!=null)
                {
                    mAveBposInfoCache.Add(sc.id, result);
                }
            }
            return result;
        }
        private async System.Threading.Tasks.Task AddCustomSettingAsync(RMImportSettingObject settingObj,bool isNewOpus)
        {
            JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
            {
                Url = settingObj.FullUrl,
                ObjectName = settingObj.SettingLevel == SettingLevel.Container ? settingObj.ContainerName :  settingObj.FullUrl.Substring(settingObj.FullUrl.LastIndexOf('/') + 1)
            };
            try
            {
                if (!isNewOpus && settingObj.ApprovalType==(int)ManualApprovalType.AutoApprove)
                {
                    logger.Error("current account is old account and the import file has auto approval,job should failed.");
                    throw new Exception("RM_JM_ImportSetting_ErrorMessage");
                }
                logger.Info($"Start to process {settingObj.FullUrl}");
                //Get Term
                logger.Info("Start to get term");
                Guid termGroupId = GetTermGroupId(settingObj.TermGroup);
                var termSet = GetTermSet(termGroupId, settingObj.TermSet);
                var scopeTerm = GetScopeTerm(termSet, settingObj.TermScopeRelativePath);
                RMTerm defaultTerm = settingObj.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm ? GetDefaultTerm(termSet, scopeTerm == null ? termSet.Id : scopeTerm.Id, settingObj.DefaultTermPath, scopeTerm == null) : null;
                //Manual approval
                var approvalType = settingObj.ApprovalType;
                var workflowDef = VerifyManualWorkflow(settingObj);
                var userInfos = await VerifyManualRecordOwnerAsync(settingObj);
                //Create setting for container
                var containers = ContainerNodes.Where(_ => _.Name.Equals(settingObj.ContainerName)).ToList();
                if (containers.Count == 0)
                {
                    throw new Exception("RM_JS_BCM_ExportSetting_ContainerNotExist");
                }
                if (containers.Count > 1)
                {
                    throw new Exception("RM_JS_BCM_ImportSetting_DuplicateContainerName");
                }
                //Check term group and term set permission
                await ValidateTermPermissionAsync(TenantLocalValue.LogonUserId, containers[0].SPObjectId, settingObj.TermGroup, [termGroupId], termSet.Name,[termSet.UniqueId], SourceFlag.SharePoint);
                if (settingObj.SettingLevel == SettingLevel.Container)
                {
                    logger.Info($"Start processing container name {settingObj.ContainerName}");
                    var containerSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(containers[0].SPObjectId), Guid.Empty);
                    if (containerSetting == null)
                    {
                        logger.Info($"The container setting {settingObj.ContainerName} is null");
                        throw new Exception("RM_JS_BCM_ImportSetting_NotColumnSetting");
                    }
                    if (string.IsNullOrEmpty(containerSetting.ColumnName) && !containerSetting.IsUsingExistColumnName)
                    {
                        logger.Info("The container setting is not set column setting");
                        throw new Exception("RM_JS_BCM_ImportSetting_NotColumnSetting");
                    }
                    var containerNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(containerSetting.NodeInfo);
                    SetDoclevelSetting(ref containerNode, termSet, scopeTerm, defaultTerm, settingObj.ApplyExisting, settingObj.IncludeDeclaredDoc, settingObj.IsOverwrite, workflowDef.ReferenceId.ToString(), approvalType, userInfos, settingObj.IsSendEmail, settingObj.ApplyTermsOnFolders, (DeployTermMethod)settingObj.DeployTermMethod);
                    var message = await RMSPSettingsService.AddGlobalColumnAsync(containerNode);
                    if (message.MessageType == RAMessageType.Successful)
                    {
                        Result.HasSuccessful = true;
                    }
                    else
                    {
                        throw new Exception(message.ErrorMessage);
                    }
                    detail.Status = JobDetailsStatus.Successful;
                    logger.Info($"Finish processing {settingObj.ContainerName}");
                    return;
                }
                //Check if SC exists
                var checkSC = await CheckSiteCollectionBelongContainer(settingObj.SiteCollectionUrl, settingObj.ContainerName);
                if(!checkSC)
                {
                    throw new Exception("RM_JS_BCM_ImportSetting_SiteNotBelongContainer");
                }
                var remoteSC = GetRemoteSite(settingObj.SiteCollectionUrl);
                //3. 找到SP对象，验证对象是否正确
                object aveSPObj = FindAveSPObject(settingObj, await GetAveBPOSInfoAsync(remoteSC));
                //4. 构造Setting，存到DB里，
                logger.Info("Load and verify inheritSetting");
                var inheritSetting = LoadInheritSeting(aveSPObj, remoteSC);
                VerifyFolderTermScope(aveSPObj, remoteSC, settingObj, scopeTerm);
                VerifyInheritTermSetting(inheritSetting, termSet);
                VerifyFolderTerm(inheritSetting, settingObj, defaultTerm, ref scopeTerm);
                logger.Info("Start to construct node");

                RMSPTreeNode curNode = await CreateNodeAsync(aveSPObj, remoteSC, inheritSetting, settingObj, termSet, scopeTerm, defaultTerm, workflowDef.ReferenceId.ToString(), approvalType, userInfos);
                logger.Info("Start to save setting");
                var returnMsg = await RMSPSettingsService.AddCustomColumnAsync(curNode);
                if (returnMsg.MessageType == RAMessageType.Successful)
                {
                    Result.HasSuccessful = true;
                }
                else
                {
                    throw new Exception(returnMsg.ErrorMessage);
                }
                detail.Status = JobDetailsStatus.Successful;
                logger.Info($"Finish processing {settingObj.FullUrl}");
            }
            catch (Exception e)
            {
                Result.HasFailed = true;
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = e.Message;
                logger.Error($"Import Custom Setting Error:{e.ToString()}");
            }
            finally
            {
                ReportManager.SendJobDetail(detail);
            }
        }

        private async Task ValidateTermPermissionAsync(string userId, string containerId, string termGroupName, List<Guid> termGroupIds, string termSetName, List<Guid> termSetIds, SourceFlag sourceFlag)
        {
            var filterOption = new FilterTermObjOption
            {
                NeedCheckPermission = true,
                FilterByContentSource = true,
                ExcludeBuiltIn = true,
                ForPhysicalView = true,
                SourceFlag = sourceFlag,
                ContainerId = containerId,
            };

            var permissionTermGroup = await DoesUserHasPermisionToTermAsync(userId, SecurityTermLevel.TermGroup, termGroupIds, filterOption);
            if (!permissionTermGroup)
            {
                logger.Error($"Current user don't have permission for term group .Name:[{termGroupName}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoTermGroupPermission");
            }

            var permissionTermSet = await DoesUserHasPermisionToTermAsync(userId, SecurityTermLevel.TermSet, termSetIds, filterOption);
            if (!permissionTermSet)
            {
                logger.Error($"Current user don't have permission for term set .Name:[{termSetName}]");
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
                    var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(userId);
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
                    hasPermission = RMSecurityGroupDao.DoesUserHasPermisionToTerm(termObjIds, dto);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error while check termobj permission for user, termObjId:{string.Join(";", termObjIds)}, level: {level} message: {ex}");
            }
            return hasPermission;
        }

        private async Task<RMSPTreeNode> CreateNodeAsync(object aveSPObj, RemoteSiteCollection remoteSC, RMSharePointSetting inheritSetting, RMImportSettingObject settingObj,
            RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm, string workflowId, int approvalType, List<ToUserInfo> userInfos)
        {
            RMSPTreeNode curNode = null;
            Guid spObjectId = GetAveObjId(aveSPObj, remoteSC);
            if (inheritSetting.ScopeId == spObjectId)
            {
                curNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(inheritSetting.NodeInfo);
                SetDoclevelSetting(ref curNode, termSet, scopeTerm, defaultTerm, settingObj.ApplyExisting, settingObj.IncludeDeclaredDoc, settingObj.IsOverwrite, workflowId, approvalType, userInfos, settingObj.IsSendEmail, settingObj.ApplyTermsOnFolders, (DeployTermMethod)settingObj.DeployTermMethod);
            }
            else
            {
                RMSPTreeNode inheritNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(inheritSetting.NodeInfo);
                var bposInfo = await CreateBposInfoAsync(remoteSC);
                NodeLevel treeNodeLevel = NodeLevel.Undefined;
                string title = GetAveObjTitleAndLevel(aveSPObj, ref treeNodeLevel);
                curNode = ConstructTreeNode(inheritNode, settingObj, title, treeNodeLevel, spObjectId, termSet, scopeTerm, defaultTerm, bposInfo, workflowId, approvalType, userInfos);
                await CreateParentNodesAsync(aveSPObj, inheritNode, remoteSC, curNode);
            }
            return curNode;
        }

        private RMWorkflowDefinition VerifyManualWorkflow(RMImportSettingObject settingObj)
        {
            var approvalType = settingObj.ApprovalType;
            var workflowDef = new RMWorkflowDefinition();
            if (approvalType == 1)
            {
                workflowDef = RMWorkflowDefinitionDao.GetWorkflowByName(settingObj.WorkflowName);
                if (workflowDef == null)
                {
                    throw new Exception("RM_JS_BCM_ImportSetting_NoWrokflow");
                }
            }
            return workflowDef;
        }

        private async Task<List<ToUserInfo>> VerifyManualRecordOwnerAsync(RMImportSettingObject settingObj)
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
                dbUsers = await UserSerive.SearchUsersAsync(userNames);
                finalUsers.AddRange(dbUsers.ConvertAll(user => ConvertUserInfo(user)));
                if (dbUsers.Count < userNames.Count)
                {
                    var adUsers = new List<ToUserInfo>();
                    var dbUserNames = dbUsers.Select(user => user.UserPrincipalName.ToLowerInvariant()).ToList();               
                    var needFindUsers = userNames.Where(user => !dbUserNames.Contains(user)).ToList();
                    foreach(var fuser in needFindUsers)
                    {
                        if (!fuser.Contains('@'))
                        {
                            failedUsers.Add(fuser);
                            continue;
                        }
                        var accountsFromAD = AccountWrapperService.SearchAccounts(TenantLocalValue.LogonGroupId, fuser, 20, false);
                        var userFromAD = accountsFromAD.Where(u => u.UserPrincipalName.ToLowerInvariant() == fuser).FirstOrDefault();
                        if (userFromAD == null)
                        {
                            failedUsers.Add(fuser);
                            continue;
                        }
                        var userInfo = ConvertUserInfo(AADAccount.Convert2AOSUserDto(userFromAD));
                        adUsers.Add(userInfo);
                    }
                    await RMSPSService.SyncADUsersAsync(adUsers);
                    logger.Info("Sync ad users to db sucess.");
                    finalUsers.AddRange(adUsers);
                }

                if (finalUsers.Count == 0)
                {
                    throw new Exception(string.Format(I18NEntity.GetString("RM_JS_BCM_ImportSetting_FailedUsers"), string.Join("|", userNames)));
                }
                if(finalUsers.Count != userNames.Count)
                {
                    throw new Exception(string.Format(I18NEntity.GetString("RM_JS_BCM_ImportSetting_FailedUsers"), string.Join("|", failedUsers)));
                }
            }
            return finalUsers;
        }

        private void VerifyInheritTermSetting(RMSharePointSetting inheritSetting,RMTermSet termSet)
        {
            if (inheritSetting == null)
            {
                logger.Error(I18NEntity.GetString("RM_JS_BCM_ImportSetting_NoGroupSetting"));
                throw new Exception("RM_JS_BCM_ImportSetting_NoGroupSetting");
            }
            else
            {
                if (!HasDocumentLevelSetting(inheritSetting))
                {
                    logger.Error($"Inherit setting does not have document level setting.");
                    throw new Exception("RM_JS_BCM_ImportSetting_NoGroupSetting");
                }
                else if (!IsSameTermGroup(inheritSetting.TermSetId, termSet))
                {
                    logger.Error($"Current term group is not same with inherit setting term group.");
                    throw new Exception("RM_JS_BCM_ImportSetting_DifferentTermGroup");
                }
            }
        }

        private void VerifyFolderTerm(RMSharePointSetting inheritSetting, RMImportSettingObject settingObj, RMTerm defaultTerm, ref RMTerm scopeTerm)
        {
            if (settingObj.SettingLevel == SettingLevel.Folder)
            {
                if (inheritSetting.TermId != Guid.Empty)
                {
                    logger.Info($"Current object is folder, so get inherit term scope. Url:[{settingObj.FullUrl}]");
                    scopeTerm = TermDAO.GetRMTermByGuId(inheritSetting.TermId);

                    if (!IsTermInTermScope(defaultTerm.Id, scopeTerm.Id))
                    {
                        logger.Error("Current term is not in term scope");
                        throw new Exception("RM_JS_BCM_ImportSetting_TermNotInScope");
                    }
                }
            }
        }

        private void VerifyFolderTermScope(object aveSPObj, RemoteSiteCollection remoteSC, RMImportSettingObject settingObj, RMTerm scopeTerm)
        {
            if (settingObj.SettingLevel == SettingLevel.Folder)
            {
                //folder节点TermScope需要与最近打破继承的父节点TermScope保持一致，因为Folder不能设置TermScope.
                var folder = (IAveFolder)aveSPObj;
                var inherParentSetting = LoadInheritSeting(folder.ParentList, remoteSC);
                if (inherParentSetting != null)
                {
                    var isTermSetScope = scopeTerm == null;//csv中folder填写的scope
                    if (isTermSetScope && inherParentSetting.TermId != Guid.Empty || !isTermSetScope && inherParentSetting.TermId != scopeTerm.UniqueId)
                    {
                        throw new Exception("RM_BCM_IS_Msg_FailedToVerifyTermScope");
                    }
                }
            }
        }

        private bool IsTermInTermScope(int termId,int scopeTermId)
        {
            string scopeTermPath = string.Empty;
            var scopeMemberShip = TermSetMembershipDAO.GetMembershipByTermId(scopeTermId);
            if (scopeMemberShip != null)
            {
                scopeTermPath = scopeMemberShip.Path;
            }

            var termPath = string.Empty;
            var termMemberShip = TermSetMembershipDAO.GetMembershipByTermId(termId);
            if (termMemberShip != null)
            {
                termPath = termMemberShip.Path;
            }

            if (termPath.StartsWith(scopeTermPath))
            {
                return true;
            }
            return false;
            //var scopeTermPath = TermSetMembershipDAO.GetSubTermMembershipByTermId(scopeTermId);
        }
        private bool IsSameTermGroup(Guid inheritTermSetId, RMTermSet curTermSet)
        {
            RMTermSet inheritTermSet = TermSetDAO.GetRMTermSetByGuid(inheritTermSetId);
            if (inheritTermSet.TermGroupId == curTermSet.TermGroupId)
            {
                return true;
            }
            return false;
        }
        private void SetDoclevelSetting(ref RMSPTreeNode node, RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm,
            bool applyExisting, bool includeDeclared, bool isOverwrite, string workflowId, int approvalType, List<ToUserInfo> userInfos, bool isSendEmail,bool applyTermsOnFolders, DeployTermMethod deployTermMethod)
        {
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
            if(approvalType != (int)ManualApprovalType.InheritParent)
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

        private static ToUserInfo ConvertUserInfo(AOSUserDto user)
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
        private Guid GetAveObjId(object aveSPObj, RemoteSiteCollection remoteSC)
        {
            if (aveSPObj is IAveFolder)
            {
                return ((IAveFolder)aveSPObj).UniqueId;
            }
            else if (aveSPObj is IAveList)
            {
                return ((IAveList)aveSPObj).ID;
            }
            else if (aveSPObj is IAveWeb)
            {
                return ((IAveWeb)aveSPObj).ID;
            }
            else if (aveSPObj is IAveSite)
            {
                return new Guid(remoteSC.id);
                //return ((IAveSite)aveSPObj).ID;
            }
            else
            {
                return Guid.Empty;
            }
        }
        private string GetAveObjTitleAndLevel(object aveSPObj, ref NodeLevel level)
        {
            if (aveSPObj is IAveFolder)
            {
                var folder = (IAveFolder)aveSPObj;
                level = NodeLevel.Folder;
                return folder.Name;
            }
            else if (aveSPObj is IAveList)
            {
                var aveList = (IAveList)aveSPObj;
                //if (aveList.BaseTemplate == AveListTemplateType.DocumentLibrary)
                //{
                //    level = NodeLevel.Library;
                //}
                //else
                //{
                    level = NodeLevel.List;
                //}
                return aveList.Title;
            }
            else if (aveSPObj is IAveWeb)
            {
                var web = (IAveWeb)aveSPObj;
                level = NodeLevel.Site;
                //if (web.IsRootWeb)
                //{
                //    return ".";
                //}
                return web.Title;
            }
            else if (aveSPObj is IAveSite)
            {
                level = NodeLevel.SiteCollection;
                return ((IAveSite)aveSPObj).RootWeb.Title;
            }
            else
            {
                level = NodeLevel.Undefined;
                return string.Empty;
            }
        }


        private RMSPTreeNode ConstructNoSettingNode(NodeLevel level, string name, Guid id, string fullPath, BposInfo bposInfo)
        {
            RMSPTreeNode node = new RMSPTreeNode();
            node.IconStatus = IconStatus.Inhert;
            node.SPType = (int)SPType.BPOS;
            node.SPVersion = GConstants.SPVersion.MOSS13;
            node.Expanded = true;
            //node.FarmId
            node.Level = (int)level;
            node.Name = name;
            node.Id = id.ToString();//TODO check this 注意一下这个Id 在load tree的时候是否每次都变
            node.SPObjectId = id.ToString();
            node.FullPath = fullPath;
            node.Expanded = true;
            node.BposInfo = bposInfo;
            return node;
        }
        private async System.Threading.Tasks.Task CreateParentNodesAsync(object curAveObj, RMSPTreeNode nodeInheritFrom, RemoteSiteCollection remoteSC, RMSPTreeNode curNode)
        {
            object parentObj = null;
            RMSPTreeNode nextNode = null;
            var bposInfo = await CreateBposInfoAsync(remoteSC);
            if (curAveObj is IAveFolder)
            {
                #region 构造folder的ParentTreeNode
                var folder = (IAveFolder)curAveObj;
                if (folder.ParentFolder != null && folder.ParentFolder.Exists && folder.ParentFolder.ServerRelativeUrl != folder.ParentList.RootFolder.ServerRelativeUrl)
                {
                    if (new Guid(nodeInheritFrom.SPObjectId).Equals(folder.ParentFolder.UniqueId))
                    {
                        curNode.Parent = nodeInheritFrom;
                        curNode.ParentId = nodeInheritFrom.Id;
                        return;
                    }
                    var fullUrl = WebUtil.MakeFullUrl(remoteSC.url, folder.ParentFolder.ServerRelativeUrl);
                    var parentFolderTreeNode = ConstructNoSettingNode(NodeLevel.Folder, folder.ParentFolder.Name, folder.UniqueId, fullUrl, bposInfo);
                    curNode.Parent = parentFolderTreeNode;
                    curNode.ParentId = parentFolderTreeNode.Id;

                    parentObj = folder.ParentFolder;
                    nextNode = parentFolderTreeNode;
                }
                else
                {
                    var foldersTreeNode = ConstructNoSettingNode(NodeLevel.Folders, NodeLevel.Folders.ToString(), Guid.NewGuid(), string.Empty, bposInfo);
                    curNode.Parent = foldersTreeNode;
                    curNode.ParentId = foldersTreeNode.Id;

                    var rootFolderTreeNode = ConstructNoSettingNode(NodeLevel.RootFolder, NodeLevel.RootFolder.ToString(), Guid.NewGuid(), string.Empty, bposInfo);
                    foldersTreeNode.Parent = rootFolderTreeNode;
                    foldersTreeNode.ParentId = rootFolderTreeNode.Id;

                    if (new Guid(nodeInheritFrom.SPObjectId).Equals(folder.ParentList.ID))
                    {
                        foldersTreeNode.Parent = nodeInheritFrom;
                        foldersTreeNode.ParentId = nodeInheritFrom.Id;
                        return;
                    }

                    //这里与界面保持一致，都传list
                    var listTreeNode = ConstructNoSettingNode(NodeLevel.List, folder.ParentList.Title, folder.ParentList.ID, folder.ParentList.RootFolder.Url, bposInfo);
                    rootFolderTreeNode.Parent = listTreeNode;
                    rootFolderTreeNode.ParentId = listTreeNode.Id;

                    parentObj = folder.ParentList;
                    nextNode = listTreeNode;
                }
                #endregion
            }
            else if (curAveObj is IAveList)
            {
                var list = (IAveList)curAveObj;
                
                var listsTreeNode = ConstructNoSettingNode(NodeLevel.Lists, NodeLevel.Lists.ToString(), Guid.NewGuid(), string.Empty, bposInfo);
                curNode.Parent = listsTreeNode;
                curNode.ParentId = listsTreeNode.Id;

                if (new Guid(nodeInheritFrom.SPObjectId).Equals(list.ParentWeb.ID))
                {
                    listsTreeNode.Parent = nodeInheritFrom;
                    listsTreeNode.ParentId = nodeInheritFrom.Id;
                    return;
                }

                //TODO 检查下web的名字是title还是name
                var parentWeb = list.ParentWeb;
                var fullUrl = WebUtil.MakeFullUrl(remoteSC.url, parentWeb.Url);
                var webTreeNode = ConstructNoSettingNode(NodeLevel.Site, parentWeb.IsRootWeb ? "." : parentWeb.Name, parentWeb.ID, fullUrl, bposInfo);
                listsTreeNode.Parent = webTreeNode;
                listsTreeNode.ParentId = webTreeNode.Id;

                parentObj = list.ParentWeb;
                nextNode = webTreeNode;
            }
            else if (curAveObj is IAveWeb)
            {
                var web = (IAveWeb)curAveObj;

                if (web.ParentWeb != null)
                {
                    
                    var websTreeNode = ConstructNoSettingNode(NodeLevel.Sites, NodeLevel.Sites.ToString(), Guid.NewGuid(), string.Empty, bposInfo);
                    curNode.Parent = websTreeNode;
                    curNode.ParentId = websTreeNode.Id;

                    if (new Guid(nodeInheritFrom.SPObjectId).Equals(web.ParentWeb.ID))
                    {
                        websTreeNode.Parent = nodeInheritFrom;
                        websTreeNode.ParentId = nodeInheritFrom.Id;
                        return;
                    }

                    //TODO 检查下web的名字是title还是name
                    var fullUrl = WebUtil.MakeFullUrl(remoteSC.url, web.ParentWeb.Url);
                    var parentWebTreeNode = ConstructNoSettingNode(NodeLevel.Site, web.ParentWeb.IsRootWeb ? "." : web.ParentWeb.Name, web.ParentWeb.ID, fullUrl, bposInfo);
                    websTreeNode.Parent = parentWebTreeNode;
                    websTreeNode.ParentId = parentWebTreeNode.Id;

                    parentObj = web.ParentWeb;
                    nextNode = parentWebTreeNode;
                }
                else
                {
                    ////TODO 看看RootWeb的Id是什么
                    //var rootWebTreeNode = ConstructNoSettingNode(NodeLevel.Site, ".", web.Site.RootWeb.ID);
                    //curNode.Parent = rootWebTreeNode;
                    //curNode.ParentId = rootWebTreeNode.Id;

                    if (new Guid(nodeInheritFrom.SPObjectId) == new Guid(remoteSC.id))
                    {
                        curNode.Parent = nodeInheritFrom;
                        curNode.ParentId = nodeInheritFrom.Id;
                        return;
                    }

                    var scTreeNode = ConstructNoSettingNode(NodeLevel.SiteCollection, web.Site.Url, new Guid(remoteSC.id), web.Site.Url, bposInfo);
                    curNode.Parent = scTreeNode;
                    curNode.ParentId = scTreeNode.Id;

                    parentObj = web.Site;
                    nextNode = scTreeNode;
                }
            }
            else if (curAveObj is IAveSite)
            {
                //var site = (IAveSite)curAveObj;
                //if (nodeInheritFrom.SPObjectId.ToString().Equals(web.Site.ID))
                //{
                    curNode.Parent = nodeInheritFrom;
                    curNode.ParentId = nodeInheritFrom.Id;
                    return;
                //}
            }
            await CreateParentNodesAsync(parentObj, nodeInheritFrom, remoteSC, nextNode);
        }
        private bool HasDocumentLevelSetting(RMSharePointSetting setting)
        {
            if ((setting.IsUsingExistColumnName && setting.SetDocLevelTermForExistColumn && setting.TermSetId != Guid.Empty)
                || (!setting.IsUsingExistColumnName && setting.TermSetId != Guid.Empty))
            {
                return true;
            }
            return false;
        }
        private RMSharePointSetting LoadInheritSeting(object aveSPObj, RemoteSiteCollection remoteSC)
        {
            RMSharePointSetting SPSetting = null;
            var siteId = new Guid(remoteSC.id);
            if (aveSPObj is IAveFolder)
            {
                var folder = (IAveFolder)aveSPObj;
                SPSetting = SharePointSettingDao.LoadSharePointSettingForImportSetting(siteId, folder.UniqueId);
                if (SPSetting == null)
                {
                    object parentObj;
                    if (folder.ParentFolder != null && folder.ParentFolder.Exists && folder.ParentFolder.ServerRelativeUrl != folder.ParentList.RootFolder.ServerRelativeUrl)
                    {
                        parentObj = folder.ParentFolder;
                    }
                    else
                    {
                        parentObj = folder.ParentList;
                    }
                    return LoadInheritSeting(parentObj, remoteSC);
                }
            }
            else if(aveSPObj is IAveList)
            {
                var list = (IAveList)aveSPObj;
                SPSetting = SharePointSettingDao.LoadSharePointSettingForImportSetting(siteId, list.ID);
                if (SPSetting == null)
                {
                    return LoadInheritSeting(list.ParentWeb, remoteSC);
                }
            }
            else if (aveSPObj is IAveWeb)
            {
                var web = (IAveWeb)aveSPObj;
                SPSetting = SharePointSettingDao.LoadSharePointSettingForImportSetting(siteId, web.ID);
                if (SPSetting == null)
                {
                    object parentObj;
                    if (web.ParentWeb != null)
                    {
                        parentObj = web.ParentWeb;
                    }
                    else
                    {
                        parentObj = web.Site;
                    }
                    return LoadInheritSeting(parentObj, remoteSC);
                }
            }
            else if (aveSPObj is IAveSite)
            {
                var site = (IAveSite)aveSPObj;
                //site collection查Setting的时候要用Remote SC的id
                SPSetting = SharePointSettingDao.LoadSharePointSettingForImportSetting(siteId, new Guid(remoteSC.id));
                if (SPSetting == null)
                {
                    SPSetting = SharePointSettingDao.LoadSharePointSettingForImportSetting(Guid.Empty, new Guid(remoteSC.parentId));
                }
            }

            return SPSetting;
        }

        private RMSPTreeNode ConstructTreeNode(RMSPTreeNode inheritNode, RMImportSettingObject settingObj, string title,
            NodeLevel level, Guid spObjectId, RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm, BposInfo bposInfo,
            string workflowId, int approvalType, List<ToUserInfo> userInfos)
        {
            RMSPTreeNode currentNode = new RMSPTreeNode();
            //currentNode = inheritNode.Clone();
            #region 继承的属性
            currentNode.SiteGroupId = inheritNode.SiteGroupId;
            currentNode.ColumnName = inheritNode.ColumnName;
            currentNode.Description = inheritNode.Description;
            currentNode.IsUsingExistColumnName = inheritNode.IsUsingExistColumnName;
            currentNode.SetDocLevelTermForExistColumn = inheritNode.SetDocLevelTermForExistColumn;
            currentNode.ExistColumnName = inheritNode.ExistColumnName;
            currentNode.EnableRelatedRecords = inheritNode.EnableRelatedRecords;//TODO check this
            currentNode.isEnableClassification = inheritNode.isEnableClassification;//指的是Container Level
            currentNode.DescriptionOfContainer = inheritNode.DescriptionOfContainer;
            currentNode.IsInheritParentTerm = inheritNode.IsInheritParentTerm;
            currentNode.TermIdOfContainer = inheritNode.TermIdOfContainer;
            currentNode.TermNameOfContainer = inheritNode.TermNameOfContainer;
            currentNode.EMailToRecordOwner = inheritNode.EMailToRecordOwner;//TODO check this
            currentNode.IsDisplyaTermPath = inheritNode.IsDisplyaTermPath;
            currentNode.ColumnRequired = inheritNode.ColumnRequired;
            currentNode.ColumnHidden = inheritNode.ColumnHidden;
            currentNode.IsSyncData = inheritNode.IsSyncData;
            currentNode.RecordOwner = inheritNode.RecordOwner;
            currentNode.ApprovalType = inheritNode.ApprovalType;
            currentNode.WorkflowReferenceId = inheritNode.WorkflowReferenceId;
            currentNode.RecordOwner = inheritNode.RecordOwner;
            #endregion

            //自己的属性
            currentNode.BposInfo = bposInfo;
            currentNode.FullPath = settingObj.FullUrl;
            currentNode.Level = (int)level;
            currentNode.Id = spObjectId.ToString();
            currentNode.SPObjectId = spObjectId.ToString();
            
            //currentNode.Title = title;
            switch (level)
            {
                case NodeLevel.Folder:
                case NodeLevel.List:
                    currentNode.Name = title;
                    break;
                case NodeLevel.Site:
                    if (settingObj.SettingLevel==SettingLevel.RootWeb)
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
            SetDoclevelSetting(ref currentNode, termSet, scopeTerm, defaultTerm, settingObj.ApplyExisting, settingObj.IncludeDeclaredDoc, settingObj.IsOverwrite, workflowId, approvalType, userInfos, settingObj.IsSendEmail,settingObj.ApplyTermsOnFolders, (DeployTermMethod)settingObj.DeployTermMethod);
            #region 废弃
            //currentNode.TermSetId = termSet.UniqueId;
            //currentNode.TermSetName = termSet.Name;
            //currentNode.TermId = termScope.UniqueId;
            //currentNode.TermName = termScope.Name;
            //currentNode.DefaultTermId = defaultTerm.UniqueId;
            //currentNode.DefaultTermName = defaultTerm.Name;
            //currentNode.DeployTermMethod = (int)DeployTermMethod.UseDefaultTerm;
            //currentNode.FullPath = settingObj.FullUrl;
            ////result.IsNewEdited = true;
            //currentNode.EnableRecordManagement = (int)EnableRecordManagementSetting.Enable;
            ////result.SettingTime = 0;
            ////currentNode
            #endregion

            return currentNode;
        }

        //private NodeLevel GetAveNodeLevel(RMImportSettingObject settingObj)
        //{
        //    switch (settingObj.SettingLevel)
        //    {
        //        case SettingLevel.SiteCollection:
        //            return NodeLevel.SiteCollection;
        //        case SettingLevel.RootWeb:
        //        case SettingLevel.SubWeb:
        //            return NodeLevel.Site;
        //        case SettingLevel.List:
        //            return NodeLevel.Library;//TODO check list
        //        case SettingLevel.Folder:
        //            return NodeLevel.Folder;
        //        default:
        //            break;
        //    }
        //}

        private async Task GetAllContainers()
        {
            try
            {
                var farmNode = SPSettingTreeService.LoadFarmSampleTree()[0];
                farmNode.PageSize = int.MaxValue;
                var returnNode = await BrowseTreeService.BrowseSPOTreeAsync(farmNode, RMBrowseTreeNodeSourceType.SharepointOnline, true);
                SPSettingTreeService.TransChildrenNodeName(returnNode);
                var webApplications = returnNode.Children;
                if (webApplications != null && webApplications.Count > 0)
                {
                    foreach (var webApplication in webApplications)
                    {
                        ContainerNodes.Add(webApplication);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Get all container occur error: {e.ToString()}");
            }
        }

       
        private Guid GetTermGroupId(string groupName)
        {
            var termGroup = TermGroupDAO.GetTermGroupByName(groupName);
            if (termGroup!=null)
            {
                switch (termPermissionType)
                {
                    case TermPermissionMethod.All:
                        break;
                    case TermPermissionMethod.None:
                        logger.Error($"Current user don't have permission for term group .Name:[{groupName}]");
                        throw new Exception("RM_JS_BCM_ImportSetting_NoTermGroupPermission");
                    case TermPermissionMethod.SpecifyScope:
                        {
                            if (termPermissionsDic.ContainsKey(termGroup.UniqueId))
                                break;
                            logger.Error($"Current user don't have permission for term group .Name:[{groupName}]");
                            throw new Exception("RM_JS_BCM_ImportSetting_NoTermGroupPermission");
                        }
                }
                return termGroup.UniqueId;
            }
            else
            {
                logger.Error($"Can not find termGroup.Name:[{groupName}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoTermGroup");
            }
        }
        private RMTermSet GetTermSet(Guid groupId, string termSetName)
        {
            RMTermSet termSet = null;
            List<RMTermSet> termSets = TermSetDAO.GetRMTermSetsByGroupUniqueId(groupId);
            if (termSets != null && termSets.Count != 0)
            {
                termSet = termSets.FirstOrDefault(t => t.Name.Equals(termSetName));
            }
            if (termSet != null)
            {
                if (termPermissionType == TermPermissionMethod.All || (termPermissionsDic.ContainsKey(groupId) && termPermissionsDic[groupId].Contains(termSet.UniqueId)))
                    return termSet;
                logger.Error($"Current user don't have permission for term set .Name:[{termSetName}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoTermSetPermission");
            }
            else
            {
                logger.Error($"Can not find termSet. Name:[{termSetName}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoTermSet");
            }
            
        }
        private RMTerm GetScopeTerm(RMTermSet termSet,string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            logger.Info($"Get term with path:{path}");
            string[] tNames = path.Split(PathSeparator);
            RMTermSetMembership ship = null;
            for (int i = 0; i < tNames.Length; i++)
            {
                int parentId = ship == null ? termSet.Id : ship.TermId;
                logger.Debug("Get parent membership with id {0}, name {1}", parentId, tNames[i]);
                ship = TermSetMembershipDAO.GetByTermNameAndParentId(parentId, tNames[i], ship == null);
            }
            if (ship == null)
            {
                logger.Error($"Can not find scope term.Path:[{path}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoScopeTerm");
            }
            return TermDAO.GetRMTermByTermId(ship.TermId);
        }

        private RMTerm GetDefaultTerm(RMTermSet termSet, int parentId, string path, bool isRootTerm)
        {
            logger.Info($"Get default term with path:{path}");
            string[] tNames = path.Split(PathSeparator);
            RMTermSetMembership ship = null;
            for (int i = 0; i < tNames.Length; i++)
            {
                int tempParentId = ship == null ? parentId : ship.TermId;
                logger.Debug("Get parent membership with id {0}, name {1}", parentId, tNames[i]);
                ship = TermSetMembershipDAO.GetByTermNameAndParentId(tempParentId, tNames[i], isRootTerm);
                isRootTerm = false;
            }
            if (ship==null)
            {
                logger.Error($"can not find default term.Path:[{path}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoDefaultTerm");
            }
            return TermDAO.GetRMTermByTermId(ship.TermId);
        }

        private AveObjectModelFactory GetFactory(RMImportSettingObject settingObject, AveBPOSAccountInfo userInfo)
        {
            AveObjectModelFactory factory = null;
            if (!mFactoryCache.TryGetValue(settingObject.SiteCollectionUrl, out factory))
            {
                factory = MultiAppUtil.CreateAveObjectModelFactory(settingObject.SiteCollectionUrl, userInfo, AveContextKind.ClientObjectModel);
                mFactoryCache.Add(settingObject.SiteCollectionUrl, factory);
            }
            return factory;
        }

        private void DisposeWebCache()
        {
            foreach (var web in mAveWebCache.Values)
            {
                try
                {
                    using (web) { }
                }
                catch (Exception e)
                {
                    logger.Warn($"Dipose web error.Url:[{web.Url}] Error:{e.ToString()}");
                }
            }
        }
        private void DisposeSiteCache()
        {
            foreach (var site in mAveSiteCache.Values)
            {
                try
                {
                    using (site) { }
                }
                catch (Exception e)
                {
                    logger.Warn($"Dipose site error.Url:[{site.Url}] Error:{e.ToString()}");
                }
            }
        }

        private IAveSite GetAveSite(AveObjectModelFactory factory, string scUrl)
        {
            IAveSite aveSite = null;
            if (!mAveSiteCache.TryGetValue(scUrl,out aveSite))
            {
                aveSite= factory.CreateSite(scUrl);
                if (aveSite != null)
                {
                    mAveSiteCache.Add(scUrl, aveSite);
                }
            }
            return aveSite;
        }

        private IAveWeb GetAveWeb(IAveSite aveSite, string serverRelativeUrl)
        {
            IAveWeb aveWeb = null;
            if (!mAveWebCache.TryGetValue(serverRelativeUrl,out aveWeb))
            {
                aveWeb = aveSite.OpenWeb(serverRelativeUrl);
                if (aveWeb != null && aveWeb.Exists)
                {
                    mAveWebCache.Add(serverRelativeUrl, aveWeb);
                }
            }
            return aveWeb;
        }

        private object FindAveSPObject(RMImportSettingObject settingObject, AveBPOSAccountInfo userInfo)
        {
            object result = null;
            try
            {
                logger.Info("Start to get sp object");
                AveObjectModelFactory factory = GetFactory(settingObject,userInfo);
                IAveSite aveSite = GetAveSite(factory, settingObject.SiteCollectionUrl);
                IAveWeb aveWeb = null;
                if ((int)settingObject.SettingLevel > (int)SettingLevel.RootWeb)
                {
                    //对于subsite的subsite，TryToRectifySiteUrl方法有时不能获取正确的weburl,所以subWeb需要单独处理
                    string webServerRelativeUrl = string.Empty;
                    if (settingObject.SettingLevel == SettingLevel.SubWeb)
                    {
                        webServerRelativeUrl = WebUtil.MakeServerRelativeUrl(settingObject.FullUrl);
                    }
                    else
                    {
                        webServerRelativeUrl = WebUtil.MakeServerRelativeUrl(factory.CreateSiteServiceHelper().TryToRectifySiteUrl(settingObject.FullUrl, userInfo));
                    }
                    aveWeb = GetAveWeb(aveSite, webServerRelativeUrl);
                    logger.Info($"Web Url:{aveWeb.Url}");
                    if (aveWeb == null || !aveWeb.Exists)
                    {
                        logger.Error($"Cannot find web in SharePoint Online");
                        throw new Exception("RM_JS_BCM_ImportSetting_NoSPObject");
                    }
                }
                switch (settingObject.SettingLevel)
                {
                    case SettingLevel.SiteCollection:
                        result = aveSite;
                        break;
                    case SettingLevel.RootWeb:
                        //result = aveSite.RootWeb;
                        result = GetAveWeb(aveSite, WebUtil.MakeServerRelativeUrl(settingObject.SiteCollectionUrl));
                        break;
                    case SettingLevel.SubWeb:
                        result = aveWeb;
                        break;
                    case SettingLevel.List:
                        ArgumentCheck.CheckNotNull(aveWeb);
                        result = aveWeb?.GetList(WebUtil.MakeServerRelativeUrl(settingObject.FullUrl));
                        break;
                    case SettingLevel.Folder:
                        ArgumentCheck.CheckNotNull(aveWeb);
                        var folder = aveWeb?.GetFolder(WebUtil.MakeServerRelativeUrl(settingObject.FullUrl));
                        result = folder == null ? null : (folder.Exists ? folder : null);
                        break;
                    default:
                        result = null;
                        break;
                }
                if (result == null)
                {
                    logger.Error($"Cannot find SharePoint Online object with url.");
                    throw new Exception("RM_JS_BCM_ImportSetting_NoSPObject");
                }
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw new Exception("RM_JS_BCM_ImportSetting_NoSPObject");
            }
            return result;

        }

        private async Task<bool> CheckSiteCollectionBelongContainer(string scUrl, string containerName)
        {
            List<string> siteUrls = new List<string>();
            if(!mContainerCache.ContainsKey(containerName))
            {
                var container = ContainerNodes.Where(_ => _.Name.Equals(containerName)).FirstOrDefault();
                if (container == null) return false;
                container.PageSize = int.MaxValue;
                var siteOfWebApplication = (await BrowseTreeService.BrowseSPOTreeAsync(container, RMBrowseTreeNodeSourceType.SharepointOnline, true)).Children;
                siteUrls = siteOfWebApplication.Select(_ => _.FullPath).ToList();
                mContainerCache.Add(containerName, siteUrls);
            }
            else
            {
                siteUrls = mContainerCache.GetValue(containerName);
            }
            return siteUrls.Any() && siteUrls.Contains(scUrl);
        }

        private RemoteSiteCollection GetRemoteSite(string scUrl)
        {
            RemoteSiteCollection site = null;
            if (!mRemoteSCCache.TryGetValue(scUrl, out site))
            {
                //DAOAPIClientV1 test = new DAOAPIClientV1();
                //site = test.GetRemoteSiteCollectionByUrl(scUrl);
                site = RABrowserClient.GetRemoteSiteCollectionByUrl(scUrl);
                if (site == null)
                {
                    logger.Warn($"Can not find sitecollection.Url:{scUrl}");
                    throw new Exception("RM_JS_BCM_ImportSetting_NoSC");
                }
                mRemoteSCCache.Add(scUrl, site);
            }
            return site;
        }

        private async Task<BposInfo> CreateBposInfoAsync(RemoteSiteCollection sc)
        {
            logger.Info($"SCUrl:[{sc.url}] ConnectionType:[{sc.AuthType.ToString()}] AppType:[{sc.AppType.ToString()}] ");
            return new BposInfo()
            {
                SiteUrl = sc.url,
                UserAccountInfo = new BposUserAccountInfo()
                {
                    Domain = sc.domain,
                    Username = sc.username,
                    Password = sc.password,
                    TenantId = sc.TenantId,
                    AdminUrl = sc.AdminUrl,
                    AADEnvironment = (AADEnvironment)(await GetAveBPOSInfoAsync(sc)).AADEnvironment,
                },
                Mode = BPOSMode.Office365,
                AppType = sc.AppType,
                ConnectionType = sc.AuthType,
            };
        }
        
        private List<RMImportSettingObject> ReadCsv(string path)
        {
            List<RMImportSettingObject> datas = new List<RMImportSettingObject>();
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    if (path.EndsWith("csv"))
                    {
                        byte[] header = new byte[2];
                        int bytesRead = fs.Read(header, 0, header.Length);
                        fs.Seek(0, SeekOrigin.Begin);
                        if (bytesRead == header.Length && header[0] == 0x50 && header[1] == 0x4B)
                        {
                            isCSVContainsIllegalCharacters = true;
                            illegalCharactersErrorMessage = I18NEntity.GetString("RM_JS_BCM_ImportSetting_InvalidFileFormat");
                            return new List<RMImportSettingObject>();
                        }
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            int count = 0;
                            while (!sr.EndOfStream)
                            {
                                JMImportSPSettingDetail detail = new JMImportSPSettingDetail();
                                try
                                {
                                    //ReportManager.IncreaseBase(1);

                                    string csvLine = sr.ReadLine();
                                    if(csvLine != null)
                                    {
                                        count++;
                                        //logger.Info($"data row:{csvLine}");

                                        if(csvLine.StartsWith("\"=\""))
                                        {
                                            string[] cols = csvLine.Substring(3).Split("\"\",\"=\"");
                                            csvLine = string.Join(",", cols);
                                        }
                                        string[] currentRow = CSVHelper.AnalyseCSVRow2Array(csvLine);

                                        foreach (var contentCell in currentRow)
                                        {
                                            if (contentCell.Contains("\t"))
                                            {
                                                isCSVContainsIllegalCharacters = true;
                                                illegalCharactersErrorMessage = string.Format(I18NEntity.GetString("RM_JS_BCM_ImportSetting_IllegalCharacters"), count, contentCell.Replace("\t", "\\t"));
                                                return new List<RMImportSettingObject>();
                                            }
                                        }

                                        if (currentRow.Length >= 16 && currentRow[15].EndsWith("\""))
                                        {
                                            currentRow[15] = currentRow[15].Substring(0, currentRow[15].Length - 1);
                                        }

                                        if (count == 1 || string.IsNullOrEmpty(currentRow[0]))
                                        {
                                            logger.Info("Skip header and empty row.");
                                            continue;
                                        }
                                        else if((currentRow.Length >= 16 && bool.TryParse(currentRow[15], out var isInherit) && isInherit))
                                        {
                                            logger.Info("Skip inherit row.");
                                            var settingObj = ConvertToPathObject(currentRow, detail);
                                            JMImportSPSettingDetail reportDetail = new JMImportSPSettingDetail()
                                            {
                                                Url = settingObj.FullUrl,
                                                ObjectName = settingObj.SettingLevel == SettingLevel.Container ? settingObj.ContainerName : settingObj.FullUrl.Substring(settingObj.FullUrl.LastIndexOf('/') + 1),
                                                Status = JobDetailsStatus.Skipped,
                                                Comment = "RM_JS_BCM_ImportSetting_SkipInherit",
                                            };
                                            ReportManager.SendJobDetail(reportDetail);
                                            continue;
                                        }
                                        else if (!SetADefaultTerm.EqualIgnoreCase(currentRow[5]) && !ManuallyChooseATerm.EqualIgnoreCase(currentRow[5]))
                                        {
                                            logger.Info("The option deploy term method is not deploy default term or manual");
                                            var settingObj = ConvertToPathObject(currentRow, detail);
                                            JMImportSPSettingDetail reportDetail = new JMImportSPSettingDetail()
                                            { 
                                                Url = settingObj.FullUrl,
                                                ObjectName = settingObj.SettingLevel == SettingLevel.Container ? settingObj.ContainerName : settingObj.FullUrl.Substring(settingObj.FullUrl.LastIndexOf('/') + 1),
                                                Status = JobDetailsStatus.Skipped,
                                                Comment = "RM_JS_BCM_ImportSetting_SkipDoesNotMethod",
                                            };
                                            ReportManager.SendJobDetail(reportDetail);
                                            continue;
                                        }
                                        else
                                        {
                                            datas.Add(ConvertToSettingObject(currentRow, detail));
                                        }

                                        //verify duplicate container setting in excel file
                                        var containerSettings = datas.Where(_ => _.SettingLevel == SettingLevel.Container);
                                        var groupContainers = containerSettings.GroupBy(c => c.ContainerName).ToList();
                                        var duplicateContainers = groupContainers.Where(g => g.Count() > 1)
                                                                            .Select(g => g)
                                                                            .ToList();
                                        foreach (var container in duplicateContainers)
                                        {
                                            logger.Warn($"The container setting is duplicated in the Excel file: Container name {container.First().ContainerName}");
                                            JMImportSPSettingDetail reportDetail = new JMImportSPSettingDetail()
                                            {
                                                ObjectName = container.First().ContainerName,
                                                Status = JobDetailsStatus.Failed,
                                                Comment = "RM_JS_BCM_ImportSetting_DuplicateContainerNameInExcel",
                                            };
                                            ReportManager.SendJobDetail(detail);
                                            foreach (var containerSetting in container)
                                            {
                                                datas.Remove(containerSetting);
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Result.HasFailed = true;
                                    detail.Comment = e.Message;
                                    detail.Status = JobDetailsStatus.Failed;
                                    logger.Error($"Convert csv line to object error:{e.ToString()}");
                                    ReportManager.SendJobDetail(detail);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw new Exception("Failed to read file conntent");
            }
            return datas;
        }

        private string GetFullUrlAndLevel(RMImportSettingObject obj, ref SettingLevel currentLevel)
        {
            string fullPath = string.Empty;

            if(string.IsNullOrEmpty(obj.SiteCollectionUrl))
            {
                currentLevel = SettingLevel.Container;
                fullPath = obj.ContainerName;
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

        private RMImportSettingObject ConvertToPathObject(string[] data, JMImportSPSettingDetail detail)
        {
            RMImportSettingObject obj = new RMImportSettingObject()
            {
                ContainerName = data[0],
                SiteCollectionUrl = WebUtil.UrlDecode(data[1]),
                SitePath = WebUtil.UrlDecode(data[2]),
                ListPath = WebUtil.UrlDecode(data[3]),
                FolderPath = WebUtil.UrlDecode(data[4]),
            };
            SettingLevel currentSettingLevel = SettingLevel.None;
            obj.FullUrl = GetFullUrlAndLevel(obj, ref currentSettingLevel);
            obj.SettingLevel = currentSettingLevel;
            return obj;
        }

        private RMImportSettingObject ConvertToSettingObject(string[] data, JMImportSPSettingDetail detail)
        {
            RMImportSettingObject obj = new RMImportSettingObject()
            {
                ContainerName = data[0],
                SiteCollectionUrl = WebUtil.UrlDecode(data[1]),
                SitePath = WebUtil.UrlDecode(data[2]),
                ListPath = WebUtil.UrlDecode(data[3]),
                FolderPath = WebUtil.UrlDecode(data[4]),
                DeployTermMethod = GetDeployTermMethod(data[5]),
                TermScopePath = ReplaceFullAngleString(data[6]),
                DefaultTermPath = ReplaceFullAngleString(data[7]),
                ApplyExisting = GetBoolColumnValue(data.ElementAtOrDefault(8)),
                IncludeDeclaredDoc = GetBoolColumnValue(data.ElementAtOrDefault(9)),
                ApplyTermsOnFolders = GetBoolColumnValue(data.ElementAtOrDefault(10)),
                IsOverwrite = GetBoolColumnValue(data.ElementAtOrDefault(11)),
                ApprovalType = string.IsNullOrEmpty(data[12]) ? (int)ManualApprovalType.InheritParent : GetManualApprovalType(ReplaceFullAngleString(data[12])),
                WorkflowName = string.IsNullOrEmpty(data[13]) ? string.Empty : data[13],
                IsSendEmail = GetBoolColumnValue(data.ElementAtOrDefault(14)),
            };

            SettingLevel currentSettingLevel = SettingLevel.None;
            obj.FullUrl = GetFullUrlAndLevel(obj, ref currentSettingLevel);
            obj.SettingLevel = currentSettingLevel;


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
            if (names.Length>2)
            {
                obj.TermScopeRelativePath = obj.TermScopePath.Substring(obj.TermScopePath.IndexOf(obj.TermSet) + obj.TermSet.Length + 1);
            }
            
            return obj;
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

        private bool GetBoolColumnValue(string value)
        {
            bool result;
            if (!bool.TryParse(value, out result))
            {
                result = false;
            }
            return result;
        }

        private string ReplaceFullAngleString(string sourceStr)
        {
            if (!string.IsNullOrEmpty(sourceStr) && (sourceStr.Contains("&") || sourceStr.Contains("\"")))
            {
                return sourceStr.Replace('&', '＆').Replace('"', '＂');
            }
            return sourceStr;
        }
    }

    public class RMImportSettingObject
    {
        #region csv column
        public string ContainerName { get; set; }
        public string SiteCollectionUrl { get; set; }
        public string SitePath { get; set; }
        public string ListPath { get; set; }
        public string FolderPath { get; set; }
        public string TermScopePath { get; set; }
        public string DefaultTermPath { get; set; }
        public bool ApplyExisting { get; set; }
        public bool IncludeDeclaredDoc { get; set; }
        public bool IsOverwrite { get; set; }
        public string WorkflowName { get; set; }
        public int ApprovalType { get; set; }
        public bool IsSendEmail { get; set; }
        public bool ApplyTermsOnFolders { get; set; }
        public int DeployTermMethod { get; set; }
        #endregion

        #region 计算出来的属性
        public string TermGroup { get; set; }
        public string TermSet { get; set; }
        public string TermScopeRelativePath { get; set; }
        public SettingLevel SettingLevel { get; set; }
        public string FullUrl { get; set; }
        #endregion
    }

    public enum SettingLevel
    {
        None = 0,
        SiteCollection = 1,
        RootWeb = 2,
        SubWeb = 3,
        List = 4,
        Folder = 5,
        Container = 6
    }

    public enum ManualApprovalType
    {
        NoManualSetting = 0,
        ManualWorkflowProcess = 1,
        ReocrdOwner = 2,
        AutoApprove = 3,
        InheritParent = 4,
    }
}
