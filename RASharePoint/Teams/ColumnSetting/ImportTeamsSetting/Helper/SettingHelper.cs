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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.Common.Setting;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Teams.ColumnSetting.ImportTeamsSetting.Helper
{
    public class SettingHelper
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(SettingHelper));
        private readonly ITeamsSettingDao _teamsSettingDao = PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private readonly AveContextHelper _aveContextHelper = new AveContextHelper();
        private List<RMTeamsSetting> _allTeamsSettingCache = [];

        public void GetRMTeamsSettings()
        {
            _allTeamsSettingCache = _teamsSettingDao.LoadAllSetting();
        }
        public RMTeamsSetting LoadInheritSeting(object aveTeamsObj, RemoteSiteCollection remoteSC , RMSPSampleTreeNode teamOrGroup = null, string containerId = null)
        {
            RMTeamsSetting teamsSetting = null;
            var teamsGroupId = Guid.Empty;
            if (!string.IsNullOrEmpty(containerId) && Guid.TryParse(containerId, out var tempTeamsGroupId))
            {
                teamsGroupId = tempTeamsGroupId;
            }
            
            if(teamOrGroup != null)
            {
                _logger.Info($"LoadInheritSeting from cache. ObjectType:[TeamOrGroup] ScopeId:[{teamOrGroup.Id}] ParentScopeId:[{teamOrGroup.ParentId}] SiteId:[{Guid.Empty}] TeamsGroupId:[{teamsGroupId}]");
                teamsSetting = _allTeamsSettingCache.FirstOrDefault(x => x.ScopeId == new Guid(teamOrGroup.Id));
                if (teamsSetting == null) teamsSetting = _allTeamsSettingCache.FirstOrDefault(x => x.ScopeId == new Guid(teamOrGroup.ParentId));
                return teamsSetting;
            }
            var siteId = new Guid(remoteSC.id);
            if (aveTeamsObj is IAveFolder)
            {
                var folder = (IAveFolder)aveTeamsObj;
                _logger.Info($"LoadInheritSeting from cache. ObjectType:[Folder] ScopeId:[{folder.UniqueId}] SiteId:[{siteId}] TeamsGroupId:[{teamsGroupId}]");
                teamsSetting = _allTeamsSettingCache.FirstOrDefault(x => x.ScopeId == folder.UniqueId && x.SiteId == siteId && x.TeamsGroupId == teamsGroupId);
                if (teamsSetting == null)
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
                    return LoadInheritSeting(parentObj, remoteSC, containerId: containerId);
                }
            }
            else if (aveTeamsObj is IAveList)
            {
                var list = (IAveList)aveTeamsObj;
                _logger.Info($"LoadInheritSeting from cache. ObjectType:[List] ScopeId:[{list.ID}] SiteId:[{siteId}] TeamsGroupId:[{teamsGroupId}]");
                teamsSetting = _allTeamsSettingCache.FirstOrDefault(x => x.ScopeId == list.ID && x.SiteId == siteId && x.TeamsGroupId == teamsGroupId);
                if (teamsSetting == null)
                {
                    return LoadInheritSeting(list.ParentWeb, remoteSC, containerId: containerId);
                }
            }
            else if (aveTeamsObj is IAveWeb)
            {
                var web = (IAveWeb)aveTeamsObj;
                _logger.Info($"LoadInheritSeting from cache. ObjectType:[Web] ScopeId:[{web.ID}] SiteId:[{siteId}] TeamsGroupId:[{teamsGroupId}]");
                teamsSetting = _allTeamsSettingCache.FirstOrDefault(x => x.ScopeId == web.ID && x.SiteId == siteId && x.TeamsGroupId == teamsGroupId);
                if (teamsSetting == null)
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
                    return LoadInheritSeting(parentObj, remoteSC,containerId: containerId);
                }
            }
            else if (aveTeamsObj is IAveSite)
            {
                var site = (IAveSite)aveTeamsObj;
                _logger.Info($"LoadInheritSeting from cache. ObjectType:[Site] ScopeId:[{remoteSC.id}] SiteId:[{siteId}] TeamsGroupId:[{teamsGroupId}]");
                teamsSetting = _allTeamsSettingCache.FirstOrDefault(x => x.ScopeId == new Guid(remoteSC.id));
                if (teamsSetting == null)
                {
                    _logger.Info($"LoadInheritSeting from cache. ObjectType:[Team] ScopeId:[{remoteSC.TeamId}] SiteId:[{Guid.Empty}] TeamsGroupId:[{teamsGroupId}]");
                    teamsSetting = _allTeamsSettingCache.FirstOrDefault(x => x.ScopeId == new Guid(remoteSC.TeamId));
                    if (teamsSetting == null)
                    {
                        _logger.Info($"LoadInheritSeting from cache. ObjectType:[Container] ScopeId:[{containerId}] SiteId:[{Guid.Empty}] TeamsGroupId:[{teamsGroupId}]");
                        teamsSetting = _allTeamsSettingCache.FirstOrDefault(x => x.ScopeId == new Guid(containerId));
                    }
                }
            }

            return teamsSetting;
        }
        private async Task CreateParentNodesAsync(object curAveObj, RMSPTreeNode nodeInheritFrom, RemoteSiteCollection remoteSC, RMSPTreeNode curNode)
        {
            object parentObj = null;
            RMSPTreeNode nextNode = null;
            var bposInfo = await _aveContextHelper.CreateBposInfoAsync(remoteSC);
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
                    var parentFolderTreeNode = _aveContextHelper.ConstructNoSettingNode(NodeLevel.Folder, folder.ParentFolder.Name, folder.UniqueId, fullUrl, bposInfo);
                    curNode.Parent = parentFolderTreeNode;
                    curNode.ParentId = parentFolderTreeNode.Id;

                    parentObj = folder.ParentFolder;
                    nextNode = parentFolderTreeNode;
                }
                else
                {
                    var foldersTreeNode = _aveContextHelper.ConstructNoSettingNode(NodeLevel.Folders, NodeLevel.Folders.ToString(), Guid.NewGuid(), string.Empty, bposInfo);
                    curNode.Parent = foldersTreeNode;
                    curNode.ParentId = foldersTreeNode.Id;

                    var rootFolderTreeNode = _aveContextHelper.ConstructNoSettingNode(NodeLevel.RootFolder, NodeLevel.RootFolder.ToString(), Guid.NewGuid(), string.Empty, bposInfo);
                    foldersTreeNode.Parent = rootFolderTreeNode;
                    foldersTreeNode.ParentId = rootFolderTreeNode.Id;

                    if (new Guid(nodeInheritFrom.SPObjectId).Equals(folder.ParentList.ID))
                    {
                        foldersTreeNode.Parent = nodeInheritFrom;
                        foldersTreeNode.ParentId = nodeInheritFrom.Id;
                        return;
                    }

                    var listTreeNode = _aveContextHelper.ConstructNoSettingNode(NodeLevel.List, folder.ParentList.Title, folder.ParentList.ID, folder.ParentList.RootFolder.Url, bposInfo);
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

                var listsTreeNode = _aveContextHelper.ConstructNoSettingNode(NodeLevel.Lists, NodeLevel.Lists.ToString(), Guid.NewGuid(), string.Empty, bposInfo);
                curNode.Parent = listsTreeNode;
                curNode.ParentId = listsTreeNode.Id;

                if (new Guid(nodeInheritFrom.SPObjectId).Equals(list.ParentWeb.ID))
                {
                    listsTreeNode.Parent = nodeInheritFrom;
                    listsTreeNode.ParentId = nodeInheritFrom.Id;
                    return;
                }

                var parentWeb = list.ParentWeb;
                var fullUrl = WebUtil.MakeFullUrl(remoteSC.url, parentWeb.Url);
                var webTreeNode = _aveContextHelper.ConstructNoSettingNode(NodeLevel.Site, parentWeb.IsRootWeb ? "." : parentWeb.Name, parentWeb.ID, fullUrl, bposInfo);
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

                    var websTreeNode = _aveContextHelper.ConstructNoSettingNode(NodeLevel.Sites, NodeLevel.Sites.ToString(), Guid.NewGuid(), string.Empty, bposInfo);
                    curNode.Parent = websTreeNode;
                    curNode.ParentId = websTreeNode.Id;

                    if (new Guid(nodeInheritFrom.SPObjectId).Equals(web.ParentWeb.ID))
                    {
                        websTreeNode.Parent = nodeInheritFrom;
                        websTreeNode.ParentId = nodeInheritFrom.Id;
                        return;
                    }

                    var fullUrl = WebUtil.MakeFullUrl(remoteSC.url, web.ParentWeb.Url);
                    var parentWebTreeNode = _aveContextHelper.ConstructNoSettingNode(NodeLevel.Site, web.ParentWeb.IsRootWeb ? "." : web.ParentWeb.Name, web.ParentWeb.ID, fullUrl, bposInfo);
                    websTreeNode.Parent = parentWebTreeNode;
                    websTreeNode.ParentId = parentWebTreeNode.Id;

                    parentObj = web.ParentWeb;
                    nextNode = parentWebTreeNode;
                }
                else
                {
                    if (new Guid(nodeInheritFrom.SPObjectId) == new Guid(remoteSC.id))
                    {
                        curNode.Parent = nodeInheritFrom;
                        curNode.ParentId = nodeInheritFrom.Id;
                        return;
                    }

                    var scTreeNode = _aveContextHelper.ConstructNoSettingNode(NodeLevel.SiteCollection, web.Site.Url, new Guid(remoteSC.id), web.Site.Url, bposInfo);
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

    }
}
