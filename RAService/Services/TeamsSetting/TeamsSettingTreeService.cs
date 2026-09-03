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
using AvePoint.Common.RemoteNode.Impl;
using AvePoint.GCommon.Contract.Server.Common.RemoteNode;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.TeamsSetting
{
    public class TeamsSettingTreeService : RMServiceBase, ITeamsSettingTreeService
    {
        #region Services

        private RALogger logger = RALogger.GetInstance(typeof(TeamsSettingTreeService));
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        #endregion

        public SPTreeMessage Browse(SPTreeNodeDto currentNode)
        {
            RMDtoConverter.ConvertSPTreeBeforeToJSON(currentNode);
            logger.Info("Start to browse tree node Id {0} and node Level {1}", currentNode?.ID, currentNode?.Level);
            try
            {
                var treeMessage = new SPTreeMessage() { TreeType = TreeType.SOArchiverTree, Node = currentNode };
                return RABrowserClient.BrowseTeams(treeMessage, RMBrowseTreeNodeSourceType.Teams);
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

        public async Task<List<RemoteSiteCollection>> GetTeamsUnderContainer(string containerSPObjectId, List<string> teamNames, bool browseInherit = false)
        {
            var states = new SiteCollectionState[] { SiteCollectionState.AccessAll, SiteCollectionState.AccessSome };
            var siteCollectionTypes = new SiteCollectionType[] { SiteCollectionType.Teams, SiteCollectionType.Group };
            var teams = RemoteNodeService.GetRemoteSiteCollectionsByParentId(containerSPObjectId, states, siteCollectionTypes, teamNames?.ToArray());
            if (browseInherit && teams.Any())
            {
                var settingIds = ArchiverSettingDao.LoadArchiverSettings()
.Where(st => st.ContentSourceType == (int)ContentSourceType.Teams)
.OrderBy(item => item.SPObjectId).Select(a => a.SPObjectId).ToList();
                teams = teams.Where(team => settingIds.Contains(new Guid(team.ObjectId))).ToList();
            }
            return teams;
        }

        public async Task<List<RMSPTreeNode>> BrowseAsync(RMSPTreeNode parent, bool needCheckPermission = false,bool browseInherit = false, bool needChannel = false)
        {
            List<RMSPTreeNode> result = new List<RMSPTreeNode>();
            SPTreeMessage msg = Browse(RMDtoConverter.ConvertRMTree2SPTree(parent));
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
                        containers = await GetPermissionContainerIdsAsync();
                    }
                }
                List<Guid> settingIds = new List<Guid>();
                if (browseInherit)
                {
                    settingIds = ArchiverSettingDao.LoadArchiverSettings()
    .Where(st => st.ContentSourceType == (int)ContentSourceType.Teams)
    .OrderBy(item => item.SPObjectId).Select(a=>a.SPObjectId).ToList();
                }
                foreach (SPTreeNodeDto sp in msg.NodeList)
                {
                    if (browseInherit)
                    {
                        if (settingIds.Contains(new Guid(sp.SPObjectId)))
                        {
                            logger.Info("Skip this node due to it is inherit from archiver setting. SPObjectId:{0}.", sp.SPObjectId);
                            continue;
                        }
                    }
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
                    if(child.Level ==  (int)NodeLevel.Office365GroupEntire)
                    {
                        child.DisplayName = sp.Url;
                    }
                    child.Parent = parent;
                    result.Add(child);

                    if (needChannel && !string.IsNullOrEmpty(child.TeamsId))
                    {
                        var (_, channels) = RemoteNodeService.GetTeamsGroupAndChannelsCollectionByTeamsId(child.TeamsId, true);
                        if (channels is not null && channels.Count > 0)
                        {
                            foreach (var channel in channels)
                            {
                                var channelTreeNode = RMDtoConverter.ConvertRemoteSite2RMTree(channel);
                                channelTreeNode.DisplayName = channel.url;
                                channelTreeNode.Parent = child;
                                channelTreeNode.O365TenantId = child.O365TenantId;
                                result.Add(channelTreeNode);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public async Task<List<RMSPTreeNode>> BrowseDirectSitesByTeamNode(SPTreeNodeDto teamNode)
        {
            try
            {
                var virtualNode = Browse(teamNode).NodeList.FirstOrDefault();
                if (virtualNode == null)
                {
                    logger.Warn($"Could not browse current node, Path [{teamNode.FullPath}], Name [{teamNode.Name}]");
                    return new();
                }
                virtualNode.ParentId = teamNode.ID;
                virtualNode.Parent = teamNode;
                return await BrowseAsync(RMDtoConverter.ConvertSPTree2RMTree(virtualNode));
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

        public List<SPTreeNodeDto> BrowseTeamsTreeNode(SPTreeNodeDto parent)
        {
            List<SPTreeNodeDto> result = new List<SPTreeNodeDto>();
            SPTreeMessage msg = this.Browse(parent);
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

        public List<RMSPTreeNode> LoadFarm()
        {
            List<RMSPTreeNode> result = new List<RMSPTreeNode>();
            SPTreeMessage msg = InitFarm();
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

        public void TransChildrenNodeName(RMSPSampleTreeNode node)
        {
            if (node?.Children?.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    DoI18N(node, child);
                }
            }
        }

        public void TransChildrenNodeName(SearchSiteCollectionLazyLoadResponse response)
        {
            if (response?.Children?.Count > 0)
            {
                foreach (var child in response.Children)
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
            }
        }

        public async Task<List<RMSPSampleTreeNode>> BrowseSampleTreeAsync(RMSPSampleTreeNode parent, bool needCheckPermission = false, bool needI18N = true, bool loadOrphanedOD = true)
        {
            List<RMSPSampleTreeNode> result = new List<RMSPSampleTreeNode>();
            SPTreeMessage msg = this.Browse(RMDtoConverter.ConvertRMSampleTree2SPTree(parent));
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
                        containers = await GetPermissionContainerIdsAsync();
                    }
                }
                foreach (SPTreeNodeDto sp in msg.NodeList)
                {
                    if (!loadOrphanedOD && sp.IsOrphenOneDrive == true)
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
                    if (needI18N) DoI18N(parent, child);
                    #endregion
                    child.Parent = parent;
                    child.ParentId = parent.Id;
                    result.Add(child);
                }
            }
            return result;
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

        #region private methods

        private void DoI18N(RMSPSampleTreeNode parent, RMSPSampleTreeNode child)
        {
            if (parent.Level == (int)NodeLevel.Farm)
            {
                if (child.Name == RMConstants.DEFAULT_O365_SITES_GROUP)
                {
                    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer");
                }
                //if (child.Name == RMConstants.DEFAULT_SPSITES_GROUP)
                //{
                //    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup");
                //}
                //if (child.Name == RMConstants.DEFAULT_SKYDRIVEPROS_GROUP)
                //{
                //    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultOneDriveforBusinessGroup");
                //}
                //if (child.Name == RMConstants.DefaultPrivateChannelSitesGroup)
                //{
                //    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer");
                //}
            }
            if (parent.Level == (int)NodeLevel.Office365GroupEntire)
            {
                if (child.Name == "Site Collections")
                {
                    child.Name = I18N.Core.I18NEntity.GetString("RM_SPS_TeamsTreeNodeSiteCollections");
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

        private SPTreeMessage InitFarm()
        {
            logger.Info("Init farm level node list.");
            try
            {
                var treeMessage = new SPTreeMessage() { TreeType = TreeType.SOArchiverTree, Node = new SPTreeNodeDto() { Level = NodeLevel.Root } };
                return RABrowserClient.BrowseTeams(treeMessage);
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

        private Task<bool> IsSuperAdminAsync()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsAdmin);
        }

        private Task<bool> IsSOSuperAdminAsync()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.TeamsAdmin);
        }

        private async Task<List<Guid>> GetPermissionContainerIdsAsync()
        {
            var containerIds = new List<Guid>();
            try
            {
                var userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                var allContainers = (await RMScopeRoleAssignmentDao.GetAllContainersByUsersAsync(userAndGroupUserIds)).Where(x => x.Key == (int)SourceFlag.Teams);
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
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to get container ids, error:{ex}");
            }
            return containerIds;
        }

        #endregion
    }
}
