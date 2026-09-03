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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services;
using AvePoint.RA.SharePoint.Common;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Restore;
using RATeams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.SharePointSetting
{
    [RACodeReview("Allen Yin")]
    public class SPSettingTreeService : RMServiceBase, ISPSettingTreeService
    {
        private RALogger logger = RALogger.GetInstance(typeof(SPSettingTreeService));
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private ISharePointSettingDao SharePointSettingsDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        private IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        public async Task<bool> ValidateGlobalStorageSettingAsync()
        {
            var rmSettings = await StorageDeviceService.GetAllStorageDeviceNotPagedAsync();
            logger.Info($"Get storage device count: {rmSettings?.Count}");

            if (TenantService.IsNewOpusTenant())
            {
                var indexDevice = StorageDeviceService.GetIndexDevice();
                logger.Info($"Get index device exist: {indexDevice != null}");

                if (rmSettings?.Count <= 0 || indexDevice == null)
                {
                    return false;
                }
            }
            else
            {
                if (rmSettings?.Count <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        [RACodeReview("Allen Yin")]
        public List<RMSPTreeNode> LoadFarm()
        {
            List<RMSPTreeNode> result = new List<RMSPTreeNode>();
            SPTreeMessage msg = this.InitFarm();
            if (msg != null && msg.NodeList != null)
            {
                foreach (SPTreeNodeDto sp in msg.NodeList)
                {
                    if (sp.SPType == SPType.Moss)
                    {
                        logger.Debug("Skip the moss farm {0}", sp.Name);
                        continue;
                    }
                    result.Add(RMDtoConverter.ConvertSPTree2RMTree(sp));
                }
            }
            return result;
        }

        [RACodeReview("Allen Yin")]
        public async Task<List<RMSPTreeNode>> BrowseAsync(RMSPTreeNode parent, bool needCheckPermission = false, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline)
        {
            List<RMSPTreeNode> result = new List<RMSPTreeNode>();
            SPTreeMessage msg = this.Browse(RMDtoConverter.ConvertRMTree2SPTree(parent), type);
            if (msg != null && msg.NodeList != null)
            {
                List<Guid> containers = new List<Guid>();
                bool isAdminUser = false;
                if (needCheckPermission && parent.Level == (int)NodeLevel.Farm)
                {
                    if (await IsSuperAdminAsync() && await IsSOSuperAdminAsync())
                    {
                        logger.Info("Current user is admin and skip check permission.UserId:{0}.", TenantLocalValue.LogonUserId);
                        isAdminUser = true;
                    }
                    else
                    {
                        containers = await GetPermissionContainerIdsAsync(type);
                    }
                }
                foreach (SPTreeNodeDto sp in msg.NodeList)
                {
                    //if (sp.NodeExtension != null &&
                    //    sp.NodeExtension.TemplateName != null &&
                    //    (sp.NodeExtension.TemplateName.Equals("POINTPUBLISHINGHUB#0", StringComparison.OrdinalIgnoreCase)
                    //    || sp.NodeExtension.TemplateName.Equals("SPSMSITEHOST#0", StringComparison.OrdinalIgnoreCase)))
                    //{
                    //    continue;
                    //}
                    #region I18N for tree node
                    if (parent.Level == (int)NodeLevel.Farm)
                    {
                        if (needCheckPermission)
                        {
                            if (isAdminUser)
                            {
                                logger.Info("Current user is admin and skip check permission.UserId:{0}.ContainerName:{1}.", TenantLocalValue.LogonUserId, sp.Name);
                            }
                            else
                            {
                                if (!containers.Contains(new Guid(sp.ID)))
                                {
                                    logger.Info("Skip this node due to current user does not have permission for this container.UserId:{0}.ContainerName:{1}.", TenantLocalValue.LogonUserId, sp.Name);
                                    continue;
                                }
                            }
                        }
                        if (sp.Name == "Default Office 365 Group Sites Group")
                        {
                            sp.Name = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer");
                        }
                        if (sp.Name == "Default_ SharePoint Sites_ Group")
                        {
                            sp.Name = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup");
                        }
                        if (sp.Name == "Default OneDrive for Business Group")
                        {
                            sp.Name = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultOneDriveforBusinessGroup");
                        }
                        if (sp.Name == "Default Private Channel Sites Container")
                        {
                            sp.Name = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer");
                        }
                    }
                    if (parent.Level == (int)NodeLevel.Office365GroupEntire)
                    {
                        if (sp.Name == "Site Collections")
                        {
                            sp.Name = I18N.Core.I18NEntity.GetString("RM_SPS_TeamsTreeNodeSiteCollections");
                        }
                    }
                    if (parent.Level == (int)NodeLevel.Site)
                    {
                        if (sp.Name == "Lists")
                        {
                            sp.Name = I18N.Core.I18NEntity.GetString("RM_SPS_SPTreeNodeLists");
                        }
                        if (sp.Name == "Sites")
                        {
                            sp.Name = I18N.Core.I18NEntity.GetString("RM_SPS_SPTreeNodeSites");
                        }
                    }
                    if (parent.Level == (int)NodeLevel.List)
                    {
                        if (sp.Name == "Root Folder")
                        {
                            sp.Name = I18N.Core.I18NEntity.GetString("RM_SPS_SPTreeNodeRootFolder");
                        }
                    }
                    if (parent.Level == (int)NodeLevel.RootFolder || parent.Level == (int)NodeLevel.Folder)
                    {
                        if (sp.Name == "Folders")
                        {
                            sp.Name = I18N.Core.I18NEntity.GetString("RM_SPS_SPTreeNodeFolders");
                        }
                    }
                    #endregion
                    RMSPTreeNode child = RMDtoConverter.ConvertSPTree2RMTree(sp);
                    child.Parent = parent;
                    result.Add(child);
                }
            }
            return result;
        }

        private List<int> GetPermissionDataSrouceType(RMBrowseTreeNodeSourceType type)
        {
            var types = new List<int>();
            if (type == RMBrowseTreeNodeSourceType.All)
            {
                types.Add((int)SourceFlag.SharePoint);
                types.Add((int)SourceFlag.OneDrive);
                types.Add((int)SourceFlag.Teams);
            }
            if (RMBrowseTreeNodeSourceType.SharepointOnline == type)
            {
                types.Add((int)SourceFlag.SharePoint);
            }
            if (RMBrowseTreeNodeSourceType.SPAndOD == type)
            {
                types.Add((int)SourceFlag.SharePoint);
                types.Add((int)SourceFlag.OneDrive);
                if (!TeamsPermissionHelper.HasUpgradeTeamsFeature())
                {
                    types.Add((int)SourceFlag.Teams);
                }       
            }
            if (RMBrowseTreeNodeSourceType.SkyDrivePro == type)
            {
                types.Add((int)SourceFlag.OneDrive);
            }
            if(RMBrowseTreeNodeSourceType.Teams == type)
            {
                types.Add((int)SourceFlag.Teams);
            }
            return types;
        }

        private async Task<List<Guid>> GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType type)
        {
            var containerIds = new List<Guid>();
            try
            {
                var userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                var allContainers = (await RMScopeRoleAssignmentDao.GetAllContainersByUsersAsync(userAndGroupUserIds)).Where(x => GetPermissionDataSrouceType(type).Contains(x.Key));
                foreach (KeyValuePair<int, List<Guid>> item in allContainers)
                {
                    item.Value.ForEach(o =>
                    {
                        if (!containerIds.Contains(o))
                        {
                            containerIds.Add(o);
                        }
                    });
                }
                if(TeamsPermissionHelper.HasUpgradeTeamsFeature() && type == RMBrowseTreeNodeSourceType.All && allContainers.Any(_ => _.Key == 11))
                {
                    containerIds.Add(new Guid("41cfe969-e07b-45cb-a7d0-b022f967e929")); // Id of private default container id.
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to get container ids, error:{ex}");
            }
            return containerIds;
        }

        //[RACodeReview("Allen Yin")]
        //private ITreeService GetTreeServiceProxy()
        //{
        //    ITreeService treeService = DocAveServiceHelper.CreateServiceClient<ITreeService>();
        //    return treeService;
        //}

        [RACodeReview("Allen Yin")]
        private SPTreeMessage InitFarm()
        {
            logger.Info("Init farm level node list.");
            try
            {
                var treeMessage = new SPTreeMessage() { TreeType = TreeType.SOArchiverTree, Node = new SPTreeNodeDto() { Level = NodeLevel.Root } };
                return RABrowserClient.BrowseSharePoint(treeMessage);
                //var client = new DAOAPIClientV1();
                //var farm = client.OnlineFarm();
                //return farm;
            }
            catch (AveException ae)
            {
                logger.Error(ae.Message, ae);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw new AveException("Error occured while communicating with DocAve Service, Please check the configure file or DocAve Service status.");
            }
        }


        [RACodeReview("Allen Yin")]
        public SPTreeMessage Browse(SPTreeNodeDto currentNode, RMBrowseTreeNodeSourceType type)
        {
            RMDtoConverter.ConvertSPTreeBeforeToJSON(currentNode);
            logger.Info("Start to browse tree node,Name:{0}, FullPath:{1}.", currentNode?.Name, currentNode?.FullPath);
            try
            {
                var treeMessage = new SPTreeMessage() { TreeType = TreeType.SOArchiverTree, Node = currentNode };
                if (type == RMBrowseTreeNodeSourceType.Teams)
                    return RABrowserClient.BrowseTeams(treeMessage, type);
                return RABrowserClient.BrowseSharePoint(treeMessage, type);
                //var client = new DAOAPIClientV1();
                //return client.Browse(new SPTreeMessage() { TreeType = TreeType.SOArchiverTree, Node = currentNode });
            }
            catch (AveException ae)
            {
                logger.Error(ae.Message, ae);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw new AveException("Error occured while communicating with DocAve Service, Please check the configure file or DocAve Service status.");
            }
        }


        public object GetPhysicalInfos()
        {
            List<RMSharePointSetting> list = SharePointSettingsDao.GetAllPhysicalSiteSettings();
            string libName = AvePoint.RA.SharePoint.Common.Util.GetAppSettingValue("RevIMHoldPhysicalLibraryName");
            List<string> scopeIds = new List<string>();
            foreach (var item in list)
            {
                scopeIds.Add(item.ScopeId.ToString());
            }
            return new { PhysicalLibraryName = libName, ScopeIds = scopeIds };
        }

        public List<RMSPSampleTreeNode> LoadFarmSampleTree()
        {
            List<RMSPSampleTreeNode> result = new List<RMSPSampleTreeNode>();
            SPTreeMessage msg = this.InitFarm();
            if (msg != null && msg.NodeList != null)
            {
                foreach (SPTreeNodeDto sp in msg.NodeList)
                {
                    if (sp.SPType == SPType.Moss)
                    {
                        logger.Debug("Skip the moss farm {0}", sp.Name);
                        continue;
                    }
                    result.Add(RMDtoConverter.ConvertSPTree2RMSampleTree(sp));
                }
            }
            return result;
        }

        public async Task<List<RMSPSampleTreeNode>> BrowseSampleTreeAsync(RMSPSampleTreeNode parent, bool needCheckPermission = false, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline, bool needI18N = true, bool loadOrphanedOD = true)
        {
            List<RMSPSampleTreeNode> result = new List<RMSPSampleTreeNode>();
            SPTreeMessage msg = this.Browse(RMDtoConverter.ConvertRMSampleTree2SPTree(parent), type);
            if (msg != null && msg.NodeList != null)
            {
                List<Guid> containers = new List<Guid>();
                bool isAdminUser = false;
                if (needCheckPermission && parent.Level == (int)NodeLevel.Farm)
                {
                    if (await IsSuperAdminAsync() || await IsSOSuperAdminAsync())
                    {
                        logger.Info("Current user is admin and skip check permission.UserId:{0}.", TenantLocalValue.LogonUserId);
                        isAdminUser = true;
                    }
                    else
                    {
                        containers = await GetPermissionContainerIdsAsync(type);
                    }
                }
                var filterDefaultChannelSite = new List<string>();
                if (!isAdminUser && TeamsPermissionHelper.HasUpgradeTeamsFeature() && parent.Level == (int)NodeLevel.WebApplication && parent.SPObjectId == "41cfe969-e07b-45cb-a7d0-b022f967e929" && type == RMBrowseTreeNodeSourceType.All)
                {
                    List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    var teamsPermissionContainerIds = RMScopeRoleAssignmentDao.GetContainersByUsers(userAndGroupUserIds, SourceFlag.Teams).Select(_ => _.ToString()).ToList();
                    var teamsIds = RemoteNodeDao.GetTeamsIdByContainerId(teamsPermissionContainerIds);
                    var dicNodes = RemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsIds(teamsIds, true);
                    foreach (var node in dicNodes)
                    {
                        filterDefaultChannelSite.AddRange(node.Value?.Select(_ => _.url) ?? new List<string>());
                    }
                }
                foreach (SPTreeNodeDto sp in msg.NodeList)
                {
                    if(!loadOrphanedOD && sp.IsOrphenOneDrive == true)
                    {
                        continue;
                    }

                    RMSPSampleTreeNode child = RMDtoConverter.ConvertSPTree2RMSampleTree(sp);
                    #region I18N for tree node
                    if (parent.Level == (int)NodeLevel.Farm)
                    {
                        if (needCheckPermission)
                        {
                            if (isAdminUser)
                            {
                                logger.Info("Current user is admin and skip check permission.UserId:{0}.ContainerName:{1}.", TenantLocalValue.LogonUserId, child.Name);
                            }
                            else
                            {
                                if (!containers.Contains(new Guid(child.Id)))
                                {
                                    logger.Info("Skip this node due to current user does not have permission for this container.UserId:{0}.ContainerName:{1}.", TenantLocalValue.LogonUserId, child.Name);
                                    continue;
                                }
                            }
                        }
                    }
                    if(parent.Level == (int)NodeLevel.WebApplication && filterDefaultChannelSite.Count > 0)
                    {
                        if (!filterDefaultChannelSite.Contains(child.FullPath)) continue;
                    }
                    if (needI18N) DoI18N(parent, child);
                    #endregion
                    child.Parent = parent;
                    child.ParentId = parent.Id;
                    result.Add(child);
                }
            }
            return result;
        }

        public async Task<List<RMSPSampleTreeNode>> BrowseSampleTree1Async(RMSPSampleTreeNode parent, bool needCheckPermission = false, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline, bool needI18N = true)
        {
            List<RMSPSampleTreeNode> result = new List<RMSPSampleTreeNode>();
            SPTreeMessage msg = this.Browse(RMDtoConverter.ConvertRMSampleTree2SPTree(parent), type);
            if (msg != null && msg.NodeList != null)
            {
                List<Guid> containers = new List<Guid>();
                bool isAdminUser = false;
                if (needCheckPermission && parent.Level == (int)NodeLevel.Farm)
                {
                    if (await IsSuperAdminAsync() || await IsSOSuperAdminAsync())
                    {
                        logger.Info("Current user is admin and skip check permission.UserId:{0}.", TenantLocalValue.LogonUserId);
                        isAdminUser = true;
                    }
                    else
                    {
                        containers = await GetPermissionContainerIdsAsync(type);
                    }
                }
                foreach (SPTreeNodeDto sp in msg.NodeList)
                {
                    RMSPSampleTreeNode child = RMDtoConverter.ConvertSPTree2RMSampleTree(sp);
                    #region I18N for tree node
                    if (parent.Level == (int)NodeLevel.Farm)
                    {
                        if (needCheckPermission)
                        {
                            if (isAdminUser)
                            {
                                logger.Info("Current user is admin and skip check permission.UserId:{0}.ContainerName:{1}.", TenantLocalValue.LogonUserId, child.Name);
                            }
                            else
                            {
                                if (!containers.Contains(new Guid(child.Id)))
                                {
                                    logger.Info("Skip this node due to current user does not have permission for this container.UserId:{0}.ContainerName:{1}.", TenantLocalValue.LogonUserId, child.Name);
                                    continue;
                                }
                            }
                        }
                    }
                    if (needI18N) DoI18N(parent, child);
                    #endregion
                    child.Parent = parent;
                    child.ParentId = parent.Id;
                    result.Add(child);
                }
            }
            return result;
        }

        public void TransChildrenNodeName(RMSPSampleTreeNode node)
        {
            if(node?.Children?.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    DoI18N(node, child);
                }
            }
        }

        private void DoI18N(RMSPSampleTreeNode parent, RMSPSampleTreeNode child)
        {
            if (parent.Level == (int)NodeLevel.Farm)
            {
                if (child.Name == RMConstants.DEFAULT_O365_SITES_GROUP)
                {
                    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer");
                }
                if (child.Name == RMConstants.DEFAULT_SPSITES_GROUP)
                {
                    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup");
                }
                if (child.Name == RMConstants.DEFAULT_SKYDRIVEPROS_GROUP)
                {
                    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultOneDriveforBusinessGroup");
                }
                if (child.Name == RMConstants.DefaultPrivateChannelSitesGroup)
                {
                    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer");
                }
            }
            if (parent.Level == (int)NodeLevel.Site)
            {
                if (child.Name == "Lists")
                {
                    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_SPTreeNodeLists");
                }
                if (child.Name == "Sites")
                {
                    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_SPTreeNodeSites");
                }
            }
            if (parent.Level == (int)NodeLevel.List)
            {
                if (child.Name == "Root Folder")
                {
                    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_SPTreeNodeRootFolder");
                }
            }
            if (parent.Level == (int)NodeLevel.RootFolder || parent.Level == (int)NodeLevel.Folder)
            {
                if (child.Name == "Folders")
                {
                    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_SPTreeNodeFolders");
                }
            }
        }
        private Task<bool> IsSuperAdminAsync()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
        }

        private Task<bool> IsSOSuperAdminAsync()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin);
        }

        public List<SPTreeNodeDto> BrowseSPTreeNode(SPTreeNodeDto parent, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline)
        {
            List<SPTreeNodeDto> result = new List<SPTreeNodeDto>();
            SPTreeMessage msg = this.Browse(parent, type);
            if (msg != null && msg.NodeList != null)
            {
                foreach (SPTreeNodeDto sp in msg.NodeList)
                {
                    sp.Parent = parent;
                    sp.ParentId = parent.ID;
                    result.Add(sp);
                }
            }
            return result;
        }

        #region Exchange Online

        public List<RMSampleEXOTreeNode> LoadExchangeRoot()
        {
            List<RMSampleEXOTreeNode> result = new List<RMSampleEXOTreeNode>();
            ExchangeOnlineTreeMessage msg = this.InitExchangeFarm();
            if (msg != null && msg.NodeList != null)
            {
                foreach (ExchangeOnlineTreeNodeDto tree in msg.NodeList)
                {
                    result.Add(RMDtoConverter.ConvertTreeNodeDto2RMSampleExchangeTree(tree));
                }
            }
            return result;
        }

        public List<ExchangeOnlineTreeNodeDto> BrowseExchangeTreeNode(ExchangeOnlineTreeNodeDto parent)
        {
            List<ExchangeOnlineTreeNodeDto> result = new List<ExchangeOnlineTreeNodeDto>();
            ExchangeOnlineTreeMessage msg = BrowseExchange(parent);
            if (msg != null && msg.NodeList != null)
            {
                foreach (ExchangeOnlineTreeNodeDto sp in msg.NodeList)
                {
                    sp.Parent = parent;
                    sp.ParentId = parent.ID;
                    result.Add(sp);
                }
            }
            return result;
        }
        /// <summary>
        /// 这个browse主要给前台使用. folder级别的Id是经过DM5转换的.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public async Task<List<RMSampleEXOTreeNode>> BrowseSampleExchangeTreeAsync(RMSampleEXOTreeNode parent, bool needCheckPermission = false)
        {
            List<RMSampleEXOTreeNode> result = new List<RMSampleEXOTreeNode>();
            ExchangeOnlineTreeMessage msg = BrowseExchange(RMDtoConverter.ConvertRMSampleExchangeTree2TreeNodeDto(parent));
            if (msg != null && msg.NodeList != null)
            {
                List<Guid> containers = new List<Guid>();
                bool isAdminUser = false;
                if (needCheckPermission && parent.Level == (int)NodeLevel.ExchangeOnlineFarm)
                {
                    if (await IsSuperAdminAsync())
                    {
                        logger.Info("Current user is admin and skip check permission.UserId:{0}.", TenantLocalValue.LogonUserId);
                        isAdminUser = true;
                    }
                    else
                    {
                        List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                        //List<RMAccount> userAccounts = AccountDao.GetUserByUserIds(new List<string>() { TenantLocalValue.LogonUserId, TenantLocalValue.LogonGroupId });
                        //containers = (RMScopeRoleAssignmentDao.GetAllContainersByGroupDataSource(userAndGroupIds, (int)DataSource.EXO)).Select(x => x.ToString()).ToList();
                        var allContainer = (await RMScopeRoleAssignmentDao.GetAllContainersByUsersAsync(userAndGroupUserIds)).Where(x => x.Key == (int)DataSource.EXO).Select(x => x.Value).ToList();
                        if (allContainer.Count > 0)
                        {
                            containers = allContainer[0];
                        }
                    }
                }
                foreach (ExchangeOnlineTreeNodeDto tree in msg.NodeList)
                {
                    RMSampleEXOTreeNode child = RMDtoConverter.ConvertTreeNodeDto2RMSampleExchangeTree(tree);
                    if (parent.Level == (int)NodeLevel.ExchangeOnlineFarm)
                    {
                        if (needCheckPermission)
                        {
                            if (isAdminUser)
                            {
                                logger.Info("Current user is admin and skip check permission.UserId:{0}.ContainerName:{1}.", TenantLocalValue.LogonUserId, child.Name);
                            }
                            else
                            {
                                if (!containers.Contains(new Guid(child.Id)))
                                {
                                    logger.Info("Skip this node due to current user does not have permission for this container.UserId:{0}.ContainerName:{1}.", TenantLocalValue.LogonUserId, child.Name);
                                    continue;
                                }
                            }
                        }
                    }
                    if (child.Level == (int)NodeLevel.ExchangeOnlineFolder)
                    {
                        child.Id = child.Id.ToLowerInvariant().ToMd5().ToString();
                    }
                    child.Parent = parent;
                    child.ParentId = parent.Id;
                    result.Add(child);
                }
            }
            return result;
        }

        public List<RMEXOTreeNode> BrowseExchangeTree(RMEXOTreeNode parent)
        {
            List<RMEXOTreeNode> result = new List<RMEXOTreeNode>();
            ExchangeOnlineTreeMessage msg = this.BrowseExchange(RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(parent));
            if (msg != null && msg.NodeList != null)
            {
                foreach (ExchangeOnlineTreeNodeDto tree in msg.NodeList)
                {
                    RMEXOTreeNode child = RMDtoConverter.ConvertTreeNodeDto2RMExchangeTree(tree);
                    child.Parent = parent;
                    result.Add(child);
                }
            }
            return result;
        }

        private ExchangeOnlineTreeMessage BrowseExchange(ExchangeOnlineTreeNodeDto currentNode)
        {

            logger.Info("Start to browse exchange tree node,Name:{0}.", currentNode?.Name);
            try
            {
                var treeMessage = new ExchangeOnlineTreeMessage()
                {
                    TreeType = TreeType.ExchangeOnlineArchiverTree,
                    Node = currentNode
                };
                return RABrowserClient.BrowseExchange(treeMessage);
                //var client = new DAOAPIClientV1();
                //return client.BrowseExchange(new ExchangeOnlineTreeMessage() { TreeType = TreeType.ExchangeOnlineArchiverTree, Node = currentNode });
            }
            catch (AveException ae)
            {
                logger.Error(ae.Message, ae);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw new AveException("Error occured while communicating with DocAve Service, Please check the configure file or DocAve Service status.");
            }
        }

        private ExchangeOnlineTreeMessage InitExchangeFarm()
        {
            logger.Info("Init exchange root level node list.");
            try
            {
                var treeMessage = new ExchangeOnlineTreeMessage()
                {
                    TreeType = TreeType.ExchangeOnlineArchiverTree,
                    Node = new ExchangeOnlineTreeNodeDto() { Level = NodeLevel.Root}
                };
                return RABrowserClient.BrowseExchange(treeMessage);
                //ExchangeOnlineTreeMessage root = new ExchangeOnlineTreeMessage()
                //{
                //    TreeType = TreeType.ExchangeOnlineArchiverTree,
                //    Node = null,
                //    NodeList = new List<ExchangeOnlineTreeNodeDto>()
                //};
                //var client = new DAOAPIClientV1();
                //return client.BrowseExchange(root);
            }
            catch (AveException ae)
            {
                logger.Error(ae.Message, ae);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw new AveException("Error occured while communicating with DocAve Service, Please check the configure file or DocAve Service status.");
            }
        }

        #endregion

    }
}
