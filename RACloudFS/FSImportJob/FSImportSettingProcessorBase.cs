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
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.Records.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RACloudFS.FSImportJob
{
    public abstract class FSImportSettingProcessorBase
    {
        protected const string NoManualSetting = "no manual setting";
        protected const string WorkflowProcess = "workflow process";
        protected const string RecordOwner = "record owner";
        protected const char PathSeparator = '|';

        protected RALogger logger;
        protected string FilePath;
        protected JobResult Result;
        protected string commomErrorMessage = "RM_TS_SS_Summary";
        protected bool isContainsIllegalCharacters;
        protected string illegalCharactersErrorMessage;
        protected Dictionary<string, TermCache> TermScopeCache = new Dictionary<string, TermCache>();
        protected Dictionary<string, TermCache> DefaulTermCache = new Dictionary<string, TermCache>();
        protected List<string> DeactiveUNCPath = new List<string>();

        #region ServiceAndDao

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

        protected IHybridBrowserService mHybridBrowserService;
        protected IHybridBrowserService hybridBrowserService
        {
            get
            {
                if (mHybridBrowserService == null)
                {
                    mHybridBrowserService = (IHybridBrowserService)PlatformWindsorManager.GetService(typeof(IHybridBrowserService));
                }
                return mHybridBrowserService;
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

        private ITermDao mTermDao;
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

        private IFSConnectionDao mFSConnectionDao;
        public IFSConnectionDao FSConnectionDao
        {
            get
            {
                if (mFSConnectionDao == null)
                {
                    mFSConnectionDao = (IFSConnectionDao)PlatformWindsorManager.GetService(typeof(IFSConnectionDao));
                }
                return mFSConnectionDao;
            }
        }

        private IFileSystemSettingDao mFileSystemSettingDao;
        public IFileSystemSettingDao FileSystemSettingDao
        {
            get
            {
                if (mFileSystemSettingDao == null)
                {
                    mFileSystemSettingDao = (IFileSystemSettingDao)PlatformWindsorManager.GetService(typeof(IFileSystemSettingDao));
                }
                return mFileSystemSettingDao;
            }
        }

        private IRMFileSystemSettingsService mFileSystemSettingsService;
        public IRMFileSystemSettingsService FileSystemSettingsService
        {
            get
            {
                if (mFileSystemSettingsService == null)
                {
                    mFileSystemSettingsService = (IRMFileSystemSettingsService)PlatformWindsorManager.GetService(typeof(IRMFileSystemSettingsService));
                }
                return mFileSystemSettingsService;
            }
        }

        private IRMFileSystemBrowserService mFileSystemBrowserService;
        public IRMFileSystemBrowserService FileSystemBrowserService
        {
            get
            {
                if (mFileSystemBrowserService == null)
                {
                    mFileSystemBrowserService = (IRMFileSystemBrowserService)PlatformWindsorManager.GetService(typeof(IRMFileSystemBrowserService));
                }
                return mFileSystemBrowserService;
            }
        }

        private IFSConnectionGroupWithAgentMemebershipDao mFSConnectionGroupWithAgentMemebershipDao;
        public IFSConnectionGroupWithAgentMemebershipDao FSConnectionGroupWithAgentMemebershipDao
        {
            get
            {
                if (mFSConnectionGroupWithAgentMemebershipDao == null)
                {
                    mFSConnectionGroupWithAgentMemebershipDao = (IFSConnectionGroupWithAgentMemebershipDao)PlatformWindsorManager.GetService(typeof(IFSConnectionGroupWithAgentMemebershipDao));
                }
                return mFSConnectionGroupWithAgentMemebershipDao;
            }
        }

        private IFSConnectionGroupDao mFSConnectionGroupDao;
        public IFSConnectionGroupDao FSConnectionGroupDao
        {
            get
            {
                if (mFSConnectionGroupDao == null)
                {
                    mFSConnectionGroupDao = (IFSConnectionGroupDao)PlatformWindsorManager.GetService(typeof(IFSConnectionGroupDao));
                }
                return mFSConnectionGroupDao;
            }
        }

        #endregion

        protected IRMWorkflowDefinitionDao RMWorkflowDefinitionDao => PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();
        public IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();
        protected IUserService UserSerive => PlatformWindsorManager.GetService<IUserService>();
        protected IRMSharePointSettingsService RMSPSService => PlatformWindsorManager.GetService<IRMSharePointSettingsService>();
        protected IRMSecurityGroupDao RMSecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();
        protected ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();
        protected readonly IMultiGeoDataCenterService MultiGeoDataCenterService = PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        protected readonly IExplorerService ExplorerService = PlatformWindsorManager.GetService<IExplorerService>();

        public abstract Task RunAsync();

        protected bool IsSameTermGroup(Guid inheritTermSetId, RMTermSet curTermSet)
        {
            RMTermSet inheritTermSet = TermSetDAO.GetRMTermSetByGuid(inheritTermSetId);
            return inheritTermSet.TermGroupId == curTermSet.TermGroupId;
        }

        protected RMWorkflowDefinition VerifyManualWorkflow(IFSImportSettingBase settingObj)
        {
            var workflowDef = new RMWorkflowDefinition();
            if (settingObj.ApprovalType == 1)
            {
                workflowDef = RMWorkflowDefinitionDao.GetWorkflowByName(settingObj.WorkflowName);
                if (workflowDef == null)
                {
                    throw new Exception("RM_JS_BCM_ImportSetting_NoWrokflow");
                }
            }
            return workflowDef;
        }

        protected async Task<List<ToUserInfo>> VerifyManualRecordOwnerAsync(IFSImportSettingBase settingObj)
        {
            var approvalType = settingObj.ApprovalType;
            var userNames = string.IsNullOrEmpty(settingObj.WorkflowName)
                ? new List<string>()
                : settingObj.WorkflowName.Split('|').ToList().ConvertAll(u => u.ToLowerInvariant()).Distinct().ToList();
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
                finalUsers.AddRange(dbUsers.ConvertAll(u => ConvertUserInfo(u)));
                if (dbUsers.Count < userNames.Count)
                {
                    var adUsers = new List<ToUserInfo>();
                    var dbUserNames = dbUsers.Select(u => u.UserPrincipalName.ToLowerInvariant()).ToList();
                    var needFindUsers = userNames.Where(u => !dbUserNames.Contains(u)).ToList();
                    foreach (var fuser in needFindUsers)
                    {
                        if (!fuser.Contains('@'))
                        {
                            failedUsers.Add(fuser);
                            continue;
                        }
                        var accountsFromAD = AccountWrapperService.SearchAccounts(TenantLocalValue.LogonGroupId, fuser, 20, false);
                        var userFromAD = accountsFromAD.FirstOrDefault(u => u.UserPrincipalName.ToLowerInvariant() == fuser);
                        if (userFromAD == null)
                        {
                            failedUsers.Add(fuser);
                            continue;
                        }
                        adUsers.Add(ConvertUserInfo(AADAccount.Convert2AOSUserDto(userFromAD)));
                    }
                    await RMSPSService.SyncADUsersAsync(adUsers);
                    logger.Info("Sync ad users to db success.");
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

        protected RMFSTreeNode ConstructNoSettingNode(NodeLevel level, string name, Guid id, string fullPath)
        {
            RMFSTreeNode node = new RMFSTreeNode();
            node.IconStatus = IconStatus.Inhert;
            node.Expanded = true;
            node.Level = (int)level;
            node.Name = name;
            node.Id = id;
            node.FullPath = fullPath;
            return node;
        }

        protected void CreateParentNodes(RMFSTreeNode inheritNode, RMFileSystemSetting groupSetting, FSConnection connection, ref RMFSTreeNode curNode)
        {
            var parentFullPath = curNode.FullPath.Substring(0, curNode.FullPath.LastIndexOf('\\')).TrimEnd('\\');
            if (connection.UNCPath.Equals(parentFullPath, StringComparison.OrdinalIgnoreCase))
            {
                var scNode = ConstructNoSettingNode(NodeLevel.SiteCollection, connection.Name, connection.Id, parentFullPath);
                RMFSTreeNode groupNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(groupSetting.NodeInfo);
                scNode.Parent = groupNode;
                scNode.ParentId = groupNode.Id.ToString();
                curNode.Parent = scNode;
                curNode.ParentId = scNode.Id.ToString();
                return;
            }
            var parentName = parentFullPath.Substring(parentFullPath.LastIndexOf('\\') + 1);
            var parentId = parentFullPath.ToLower().ToMd5();
            var parentNode = ConstructNoSettingNode(NodeLevel.FSFolder, parentName, parentId, parentFullPath);
            curNode.Parent = parentNode;
            curNode.ParentId = parentId.ToString();
            CreateParentNodes(inheritNode, groupSetting, connection, ref parentNode);
        }

        protected void CreateParentNode(RMFSTreeNode inheritNode, ref RMFSTreeNode curNode)
        {
            var parentName = inheritNode.Name;
            var parentId = inheritNode.Id;
            var parentFullPath = inheritNode.FullPath;
            curNode.Parent = ConstructNoSettingNode(NodeLevel.WebApplication, parentName, parentId, parentFullPath);
            curNode.ParentId = parentId.ToString();
        }

        protected virtual bool IsConnectionRootPath(string parentFullPath, FSConnection connection)
        {
            return connection.UNCPath.Equals(parentFullPath, StringComparison.OrdinalIgnoreCase);
        }

        protected RMFileSystemSetting LoadInheritSeting(string uncPath, FSConnection Connection)
        {
            if (uncPath.ToLowerInvariant() != Connection.UNCPath.ToLowerInvariant())
            {
                var scopeId = uncPath.ToLowerInvariant().ToMd5();
                var fsSetting = FileSystemSettingDao.LoadFSSetting(scopeId, Connection.GroupId);
                if (fsSetting == null)
                {
                    var parentPath = uncPath.Remove(uncPath.LastIndexOf(@"\"));
                    string orginPath = @"\" + parentPath.Replace(@"\\", @"\");
                    return LoadInheritSeting(orginPath, Connection);
                }
                else
                {
                    return fsSetting;
                }
            }
            else
            {
                var connSetting = FileSystemSettingDao.LoadFSSetting(Connection.Id, Connection.GroupId);
                if (connSetting == null)
                {
                    var groupSetting = FileSystemSettingDao.LoadFSSetting(Connection.GroupId, Connection.GroupId);
                    return groupSetting;
                }
                return connSetting;
            }
        }

        protected RMFileSystemSetting LoadGroupSetting(FSConnection connection)
        {
            return FileSystemSettingDao.LoadFSSetting(connection.GroupId, connection.GroupId);
        }

        protected RMFileSystemSetting LoadGroupSetting(Guid groupId)
        {
            return FileSystemSettingDao.LoadFSSetting(groupId, groupId);
        }

        protected RMTerm GetDefaultTerm(RMTermSet termSet, int parentId, string path, bool isRootTerm)
        {
            logger.Info("Get default term with path");
            string[] tNames = path.Split(PathSeparator);
            RMTermSetMembership ship = null;
            for (int i = 0; i < tNames.Length; i++)
            {
                int tempParentId = ship == null ? parentId : ship.TermId;
                logger.Debug("Get parent membership with id {0}, name {1}", tempParentId, tNames[i]);
                ship = TermSetMembershipDAO.GetByTermNameAndParentId(tempParentId, tNames[i], isRootTerm);
                isRootTerm = false;
            }
            if (ship == null)
            {
                logger.Error($"Cannot find default term. Path:[{path}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoDefaultTerm");
            }
            RMTerm term = TermDAO.GetRMTermByTermId(ship.TermId);
            if (term.IsDeprecated)
            {
                throw new Exception("RM_JS_BCM_ImportSetting_TermRetired");
            }
            return term;
        }

        protected RMTerm GetScopeTerm(RMTermSet termSet, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            logger.Info("Get term with path");
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
                logger.Error($"Cannot find scope term. Path:[{path}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoScopeTerm");
            }
            var term = TermDAO.GetRMTermByTermId(ship.TermId);
            if (term.IsDeprecated)
            {
                throw new Exception("RM_JS_BCM_ImportSetting_TermScopeRetired");
            }
            return term;
        }

        protected Guid GetTermGroupId(string groupName)
        {
            var termGroup = TermGroupDAO.GetTermGroupByName(groupName);
            if (termGroup != null)
            {
                return termGroup.UniqueId;
            }
            logger.Error($"Cannot find termGroup. Name:[{groupName}]");
            throw new Exception("RM_JS_BCM_ImportSetting_NoTermGroup");
        }

        protected RMTermSet GetTermSet(Guid groupId, string termSetName)
        {
            List<RMTermSet> termSets = TermSetDAO.GetRMTermSetsByGroupUniqueId(groupId);
            RMTermSet termSet = termSets?.FirstOrDefault(t => t.Name.Equals(termSetName));
            if (termSet != null)
            {
                return termSet;
            }
            logger.Error($"Cannot find termSet. Name:[{termSetName}]");
            throw new Exception("RM_JS_BCM_ImportSetting_NoTermSet");
        }

        protected bool GetBoolColumnValue(string value)
        {
            bool.TryParse(value, out bool result);
            return result;
        }

        protected ToUserInfo ConvertUserInfo(AOSUserDto user)
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

        protected void ReportValidationError(string objectName, string url, string commentKey, int sheetIndex, int rowIndex, int colIndex)
        {
            JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
            {
                ObjectName = objectName,
                Url = url,
                Status = JobDetailsStatus.Failed,
                Comment = string.Format(I18NEntity.GetString(commentKey), sheetIndex, rowIndex, colIndex),
            };
            ReportManager.SendJobDetail(detail);
        }

        protected bool IsRowEmpty(string[] row)
        {
            return row.All(cell => string.IsNullOrEmpty(cell));
        }
    }

    /// <summary>
    /// Marker interface to allow shared workflow/approval validation on both setting object types.
    /// </summary>
    public interface IFSImportSettingBase
    {
        int ApprovalType { get; }
        string WorkflowName { get; }
    }

    public class TermCache
    {
        public Guid UniqueId { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
    }
}