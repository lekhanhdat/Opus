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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.Services.AccountManager;
using RAReportCenter.ClientAuditReport.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAReportCenter.ClientAuditReport.Scanner
{
    public class TeamsAuditReportScanner : SharePointOnlineAuditReportScanner
    {
        #region private fields
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(TeamsAuditReportScanner));
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private IRMRemoteNodeDao mRMRemoteNodeDao;
        private IRMRemoteNodeDao RMRemoteNodeDao
        {
            get
            {
                if (mRMRemoteNodeDao == null)
                {
                    mRMRemoteNodeDao = (IRMRemoteNodeDao)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeDao));
                }
                return mRMRemoteNodeDao;
            }
        }

        private ITeamsSettingTreeService mRMTeamsTreeService;
        private ITeamsSettingTreeService RMTeamsTreeService
        {
            get
            {
                if (mRMTeamsTreeService == null)
                {
                    mRMTeamsTreeService = (ITeamsSettingTreeService)PlatformWindsorManager.GetService(typeof(ITeamsSettingTreeService));
                }
                return mRMTeamsTreeService;
            }
        }
        #endregion

        public TeamsAuditReportScanner(RMProfileDto profileDto, string jobId, JobType jobType) : base(profileDto, jobId, jobType)
        {
        }

        protected override async Task InitAsync(RMProfileDto profileDto, JobType jobType)
        {
            Logger.Info($"Init site Details list. TreeScope: {mClientAuditReportDto.TreeScope}");
            var selectedNodes = new List<string>();
            if (mClientAuditReportDto.TreeScope == TreeModeSettings.AllSites)
            {
                if (await IsSuperAdminAsync() && await IsSOSuperAdminAsync())
                {
                    var nodes = RMRemoteNodeDao.GetAllRemoteSiteCollectionURLsBySource(RMBrowseTreeNodeSourceType.Teams);
                    if (nodes != null && nodes.Count > 0)
                    {
                        selectedNodes = nodes.Select(t => t.Url).ToList();
                    }
                }
                else
                {
                    List<int> sourceTypeList = new List<int>() { (int)SourceFlag.Teams };
                    var userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    var allContainers = (await RMScopeRoleAssignmentDao.GetAllContainersByUsersAsync(userAndGroupUserIds)).Where(x => sourceTypeList.Contains(x.Key));
                    var containerIds = base.GetHasPermissionContainerIds(allContainers);
                    //var browserType = jobType switch
                    //{
                    //    JobType.SPOActionAuditReport => RMBrowseTreeNodeSourceType.SharepointOnline,
                    //    JobType.OneDriveActionAuditReport => RMBrowseTreeNodeSourceType.SkyDrivePro,
                    //    JobType.TeamsActionAuditReport => RMBrowseTreeNodeSourceType.Teams,
                    //    _ => RMBrowseTreeNodeSourceType.All
                    //};
                    List<RMRemoteNode> teamsNode = new List<RMRemoteNode>();
                    List<RMRemoteNode> ChannalSite = new List<RMRemoteNode>();
                    var remoteNodes = RMRemoteNodeDao.GetAllRemoteSiteCollectionURLsBySource(RMBrowseTreeNodeSourceType.Teams);
                    foreach (var node in remoteNodes)
                    {
                        if (containerIds.Contains(new Guid(node.ParentId)))
                        {
                            teamsNode.Add(node);
                        }
                    }
                    foreach (var team in teamsNode)
                    {
                        ChannalSite.AddRange(remoteNodes.Where(n => n.TeamId != null && n.TeamId.Equals(team.TeamId, StringComparison.OrdinalIgnoreCase)));
                    }
                    selectedNodes.AddRange(teamsNode.Select(t => t.Url));
                    selectedNodes.AddRange(ChannalSite.Select(t => t.Url));
                }
                
            }
            else
            {
                //process tree nodes
                var mTreeNodes = await AssembleRunableSitesAsync(profileDto, RMBrowseTreeNodeSourceType.Teams);
                var sites = from node in mTreeNodes where node.Level == (int)NodeLevel.SiteCollection select node.FullPath;
                selectedNodes.AddRange(sites);
            }

            mUrlDic = SPAuditReportUtility.GetUrlDic(selectedNodes);
            foreach (var s in selectedNodes)
            {
                if (!siteDetails.ContainsKey(s))
                {
                    siteDetails.Add(s, 0);
                }
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
        protected override async Task<List<RMSPTreeNode>> AssembleRunableSitesAsync(RMProfileDto dto, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.Teams)
        {
            var nodeList = new List<RMSPTreeNode>();
            if (!string.IsNullOrEmpty(dto.Extension2))
            {
                var farmNode = this.GetFarmSPTreeNode(dto.Extension2);
                nodeList = await this.AssembleTeamsAllTreeNodeAsync(farmNode);
            }
            return nodeList;
        }

        private async Task<List<RMSPTreeNode>> AssembleTeamsAllTreeNodeAsync(RMSPTreeNode farmNode)
        {
            var treeNodes = new List<RMSPTreeNode>();
            Logger.Info($"Start process AssembleTeamsAllTreeNodeAsync");
            var allTempGroupChildrenIds = (await RMTeamsTreeService.BrowseAsync(farmNode)).Select(gr => gr.Id).ToList();

            if (allTempGroupChildrenIds == null || allTempGroupChildrenIds.Count == 0)
            {
                Logger.Info($"No container was found.");
                return treeNodes;
            }

            foreach (var groupNode in farmNode.Children)
            {
                if (!allTempGroupChildrenIds.Contains(groupNode.Id))
                {
                    Logger.Info($"The container: [{groupNode.Id}-{groupNode.Name}] was removed.");
                    continue;
                }

                var allTempTeamsChildren = (await RMTeamsTreeService.BrowseAsync(groupNode)).ToList();

                if (allTempTeamsChildren == null || allTempTeamsChildren.Count == 0)
                {
                    Logger.Info($"No Teams was found under the container node: [{groupNode.Id}-{groupNode.Name}].");
                    continue;
                }

                if (groupNode.CheckNumber == 1)
                {
                    Logger.Info($"The container [{groupNode.Id}-{groupNode.Name}] was fully selected.");
                    foreach (var teamsNode in allTempTeamsChildren)
                    {
                        teamsNode.CheckNumber = 1;
                        await ProcessTeamsNode(allTempTeamsChildren, teamsNode, groupNode, treeNodes);
                    }
                    continue;
                }

                if (groupNode.CheckNumber == 2 && groupNode.Children != null)
                {
                    Logger.Info($"The container [{groupNode.Id}-{groupNode.Name}] was half-selected.");
                    foreach (var connectionNode in groupNode.Children)
                    {
                        await ProcessTeamsNode(allTempTeamsChildren, connectionNode, groupNode, treeNodes, true);
                    }

                    foreach (var child in allTempTeamsChildren)
                    {
                        Logger.Info($"Process the newly added Teams node: [{child.Id}-{child.Name}]");
                        child.CheckNumber = 1;
                        await ProcessTeamsNode(allTempTeamsChildren, child, groupNode, treeNodes);
                    }
                    continue;
                }

                if (groupNode.Children != null)
                {
                    Logger.Info($"The container [{groupNode.Id}-{groupNode.Name}] was not selected. Process finding the selected sub-nodes");
                    foreach (var connectionNode in groupNode.Children)
                    {
                        await ProcessTeamsNode(allTempTeamsChildren, connectionNode, groupNode, treeNodes);
                    }
                }
            }
            return treeNodes;
        }

        private static bool HasSelectNodeForTeams(RMSPTreeNode current)
        {
            if (current.CheckNumber != 0) return true;
            if (current.Children == null || current.Children.Count == 0) return false;
            else
            {
                foreach (var child in current.Children)
                {
                    if (HasSelectNodeForTeams(child))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private async Task ProcessTeamsNode(List<RMSPTreeNode> allTempTeamsChildren, RMSPTreeNode teamsNode, RMSPTreeNode containerNode, List<RMSPTreeNode> treeNodes, bool isIncludeNew = false)
        {
            Logger.Info($"Start ProcessTeamsNode for Teams node: {teamsNode.Name}, isIncludeNew: [{isIncludeNew}]");
            if (!allTempTeamsChildren.Any(t => t.Id == teamsNode.Id))
            {
                Logger.Info($"The Teams/Group: [{teamsNode.Id}-{teamsNode.Name}] was removed from container: [{containerNode.Id}-{containerNode.Name}].");
                return;
            }

            if (isIncludeNew) allTempTeamsChildren.RemoveAll(t => t.Id == teamsNode.Id);

            var allTempSiteChildren = (await RMTeamsTreeService.BrowseDirectSitesByTeamNode(RMDtoConverter.ConvertRMTree2SPTree(teamsNode)))?.ToList() ?? [];
            if (teamsNode.CheckNumber == 1)
            {
                allTempSiteChildren.ForEach(siteNode => AddSite(siteNode, treeNodes));
                return;
            }

            if (!HasSelectNodeForTeams(teamsNode)) return;

            var teamsRelatedSites = teamsNode.Children?.FirstOrDefault()?.Children;
            if (teamsRelatedSites.IsNullOrEmpty() || allTempSiteChildren.IsNullOrEmpty() || teamsNode.Children.IsNullOrEmpty())
            {
                Logger.Info($"No site was found under the Teams node: [{teamsNode.Id}-{teamsNode.Name}].");
                return;
            }

            foreach (var siteNode in teamsRelatedSites)
            {
                if (siteNode.CheckNumber == 1)
                {
                    if (!allTempSiteChildren.Any(u => u.Id == siteNode.Id))
                    {
                        Logger.Info($"The selected site: [{siteNode.Id}-{siteNode.Name}] was removed from Teams: [{teamsNode.Id}-{teamsNode.Name}].");
                        continue;
                    }
                    AddSite(siteNode, treeNodes);
                }

                if (isIncludeNew) allTempSiteChildren.RemoveAll(o => o.Id == siteNode.Id);
            }

            Logger.Info($"Start add include new sites for Teams: [{teamsNode.Id}-{teamsNode.Name}].");
            if (isIncludeNew) allTempSiteChildren.ForEach(siteNode => AddSite(siteNode, treeNodes));

            Logger.Info($"End ProcessTeamsNode for Teams node: {teamsNode.Name}");
        }

        private static void AddSite(RMSPTreeNode node, List<RMSPTreeNode> treeNodes)
        {
            if (node == null || treeNodes == null) return;

            node.CheckNumber = 1;
            Logger.Info($"Add site node [{node.Id}-{node.Name}] to the process node list.");
            treeNodes.Add(node);
        }
    }
}
