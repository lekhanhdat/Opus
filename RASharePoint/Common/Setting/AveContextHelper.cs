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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common.Setting.Model;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Common.Setting
{
    public class AveContextHelper
    {
        private readonly static RALogger _logger = RALogger.GetInstance(typeof(AveContextHelper));
        private readonly Dictionary<string, AveObjectModelFactory> _factoryCache = new();
        private readonly Dictionary<string, IAveSite> _aveSiteCache = new();
        private readonly Dictionary<string, IAveWeb> _aveWebCache = new();
        private readonly Dictionary<string, AveBPOSAccountInfo> _aveBposInfoCache = new();        
        private readonly Dictionary<string, RemoteSiteCollection> _remoteSCCache = new();


        public (string SiteRelatedUrl, string ListRelatedUrl, string FolderRelatedUrl, bool isValid, SettingLevel level) GetStructOfObject(string siteCollectionUrl ,string fullPath, AveBPOSAccountInfo userInfo, bool isSiteCollectionLevel, Guid sPObjectId)
        {
            try
            {
                if(isSiteCollectionLevel) return (string.Empty, string.Empty, string.Empty, true, SettingLevel.SiteCollection);
                if (!isSiteCollectionLevel && siteCollectionUrl.Equals(fullPath)) return (".", string.Empty, string.Empty, true, SettingLevel.RootWeb);
                string siteUrl = string.Empty, listUrl = string.Empty, folderUrl = string.Empty;
                var factory = GetFactory(siteCollectionUrl, userInfo);
                var aveSite = GetAveSite(factory, siteCollectionUrl);
                var aveWeb = aveSite.OpenWeb();
                int index = 0;
                var fullPathSub = fullPath.Substring(siteCollectionUrl.Length + 1).Split("/");
                GetSubWeb(fullPathSub, index, aveSite, aveWeb, factory, ref siteUrl, ref listUrl, aveWeb.ServerRelativeUrl);
                if (siteUrl.EndsWith("/")) siteUrl = siteUrl.TrimEnd('/');
                string tempPath = siteCollectionUrl + "/" + (siteUrl.Equals(".") ? string.Empty : siteUrl + "/") + listUrl;
                if(tempPath.EndsWith('/'))
                    tempPath = tempPath.TrimEnd('/');
                folderUrl = fullPath.Substring(tempPath.Length);
                var sitePath = siteCollectionUrl + (siteUrl.Equals(".") ? string.Empty : "/" + siteUrl);
                SettingLevel level = SettingLevel.SiteCollection;
                level = GetSettingLevelOfObject(siteCollectionUrl, siteUrl, listUrl, folderUrl);
                return (siteUrl, listUrl.TrimStart('/'), folderUrl.TrimStart('/'), CheckAveTeamsObjectIsExist(fullPath, level, siteCollectionUrl, userInfo, sPObjectId), level);
            }
            catch(Exception e)
            {
                _logger.Error($"Error occurs in GetStructOfObject, error: {e}");
                return (string.Empty, string.Empty, string.Empty, false, SettingLevel.Container);
            }
        }

        private SettingLevel GetSettingLevelOfObject(string siteCollectionUrl, string siteUrl, string listUrl, string folderUrl)
        {
            if (!string.IsNullOrEmpty(folderUrl)) return SettingLevel.Folder;
            if (!string.IsNullOrEmpty(listUrl)) return SettingLevel.List;
            if (!string.IsNullOrEmpty(siteUrl))
            {
                if (siteUrl.Equals(".")) return SettingLevel.RootWeb;
                else return SettingLevel.SubWeb;
            }
            return SettingLevel.SiteCollection;
        }

        private void GetSubWeb(string[] fullPathSub, int index, IAveSite aveSite, IAveWeb aveWeb, AveObjectModelFactory factory, ref string siteUrl, ref string listUrl, string serverRelativeUrl)
        {
            var discoverWeb = new AveDiscoverWeb(aveSite, serverRelativeUrl, DiscoverModule.Archive, factory);
            var subWebs = discoverWeb.GetSubWebs();
            foreach (var subWeb in subWebs.Values)
            {
                if (subWeb.FullUrl.Split("/")[subWeb.FullUrl.Split("/").Length - 1].Equals(fullPathSub[index]))
                {
                    siteUrl += fullPathSub[index] + "/";
                    index++;
                    if (fullPathSub.Length > index)
                    {
                        GetSubWeb(fullPathSub, index, aveSite, aveWeb, factory, ref siteUrl, ref listUrl, serverRelativeUrl + "/" + fullPathSub[index - 1]);
                    }
                    return;
                }
            }
            if (fullPathSub[index].Equals("Lists") && fullPathSub.Length > index + 1) listUrl = "Lists" + "/" + fullPathSub[index + 1];
            else if (fullPathSub.Length > index)
                listUrl = fullPathSub[index];
            if (string.IsNullOrEmpty(siteUrl)) siteUrl = ".";
        }

        public bool CheckAveTeamsObjectIsExist(string settingFullPath, SettingLevel level, string siteFullPath, AveBPOSAccountInfo userInfo, Guid objectId)
        {
            try
            {
                _logger.Info("Start to get teams object");

                var factory = GetFactory(siteFullPath, userInfo);
                var aveSite = GetAveSite(factory, siteFullPath);

                IAveWeb? aveWeb = null;
                if ((int)level > (int)SettingLevel.RootWeb)
                {
                    string webServerRelativeUrl = level == SettingLevel.SubWeb
                        ? WebUtil.MakeServerRelativeUrl(settingFullPath)
                        : WebUtil.MakeServerRelativeUrl(factory.CreateSiteServiceHelper().TryToRectifySiteUrl(settingFullPath, userInfo));

                    aveWeb = GetAveWeb(aveSite, webServerRelativeUrl);
                    _logger.Info($"Web Url: {aveWeb?.Url}");

                    if (aveWeb == null || !aveWeb.Exists)
                    {
                        _logger.Error("Cannot find web in Teams");
                        return false;
                    }
                }

                return CheckSharePointObjectIsExist(level, settingFullPath, siteFullPath, aveSite, aveWeb, objectId);
            }
            catch (Exception ex)
            {
                _logger.Error($"error occured in FindAveTeamsObject, error: {ex.ToString()}");
                return false;
            }
        }

        public object? FindAveTeamsObject(string settingFullPath, SettingLevel level, string siteFullPath, AveBPOSAccountInfo userInfo)
        {
            try
            {
                _logger.Info("Start to get teams object");

                var factory = GetFactory(siteFullPath, userInfo);
                var aveSite = GetAveSite(factory, siteFullPath);

                IAveWeb? aveWeb = null;
                if ((int)level > (int)SettingLevel.RootWeb)
                {
                    string webServerRelativeUrl = level == SettingLevel.SubWeb
                        ? WebUtil.MakeServerRelativeUrl(settingFullPath)
                        : WebUtil.MakeServerRelativeUrl(factory.CreateSiteServiceHelper().TryToRectifySiteUrl(settingFullPath, userInfo));

                    aveWeb = GetAveWeb(aveSite, webServerRelativeUrl);
                    _logger.Info($"Web Url: {aveWeb?.Url}");

                    if (aveWeb == null || !aveWeb.Exists)
                    {
                        _logger.Error("Cannot find web in Teams");
                        throw new Exception("RM_JS_BCM_ImportSetting_NoSPObject");
                    }
                }

                return TryGetTeamsObject(level, settingFullPath, siteFullPath, aveSite, aveWeb);
            }
            catch (Exception ex)
            {
                _logger.Error($"error occured in FindAveTeamsObject, error: {ex.ToString()}");
                return null;
            }
        }

        private bool CheckSharePointObjectIsExist(SettingLevel level, string settingFullPath, string siteFullPath, IAveSite aveSite, IAveWeb aveWeb, Guid objectId)
        {
            string path = WebUtil.MakeServerRelativeUrl(settingFullPath);

            switch (level)
            {
                case SettingLevel.SiteCollection:
                    return true;
                case SettingLevel.RootWeb:
                    return GetAveWeb(aveSite, WebUtil.MakeServerRelativeUrl(siteFullPath)).ID == objectId;
                case SettingLevel.SubWeb:
                    return aveWeb.ID == objectId;
                case SettingLevel.List:
                    return aveWeb.GetList(path).ID == objectId;
                case SettingLevel.Folder:
                    var folder = aveWeb.GetFolder(path);
                    return folder?.Exists == true && folder?.UniqueId == objectId ? true : false;
                default:
                    return false;
            }
        }

        private object? TryGetTeamsObject(SettingLevel level, string settingFullPath, string siteFullPath, IAveSite aveSite, IAveWeb aveWeb)
        {
            object? result = null;
            string path = WebUtil.MakeServerRelativeUrl(settingFullPath);

            switch (level)
            {
                case SettingLevel.SiteCollection:
                    result = aveSite;
                    break;
                case SettingLevel.RootWeb:
                    result = GetAveWeb(aveSite, WebUtil.MakeServerRelativeUrl(siteFullPath));
                    break;
                case SettingLevel.SubWeb:
                    result = aveWeb;
                    break;
                case SettingLevel.List:
                    result = aveWeb.GetList(path);
                    break;
                case SettingLevel.Folder:
                    var folder = aveWeb.GetFolder(path);
                    result = folder?.Exists == true ? folder : null;
                    break;
                default:
                    return result;
            }

            return result;
        }
        
        public RemoteSiteCollection GetRemoteSite(string scUrl)
        {
            RemoteSiteCollection site = null;
            if (!_remoteSCCache.TryGetValue(scUrl, out site))
            {
                site = RABrowserClient.GetRemoteSiteCollectionByUrl(scUrl);
                if (site == null)
                {
                    _logger.Warn($"Can not find sitecollection.Url: {scUrl}");
                    return null;
                }
                _remoteSCCache.Add(scUrl, site);
            }
            return site;
        }


        private AveObjectModelFactory GetFactory(string siteFullPath, AveBPOSAccountInfo userInfo)
        {
            if (!_factoryCache.TryGetValue(siteFullPath, out var factory))
            {
                factory = MultiAppUtil.CreateAveObjectModelFactory(siteFullPath, userInfo, AveContextKind.ClientObjectModel);
                _factoryCache.Add(siteFullPath, factory);
            }
            return factory;
        }

        private IAveSite GetAveSite(AveObjectModelFactory factory, string scUrl)
        {
            if (!_aveSiteCache.TryGetValue(scUrl, out var aveSite))
            {
                aveSite = factory.CreateSite(scUrl);
                if (aveSite != null)
                {
                    _aveSiteCache.Add(scUrl, aveSite);
                }
            }
            return aveSite;
        }

        private IAveWeb GetAveWeb(IAveSite aveSite, string serverRelativeUrl)
        {
            if (!_aveWebCache.TryGetValue(serverRelativeUrl, out var aveWeb))
            {
                aveWeb = aveSite.OpenWeb(serverRelativeUrl);
                if (aveWeb != null && aveWeb.Exists)
                {
                    _aveWebCache.Add(serverRelativeUrl, aveWeb);
                }
            }
            return aveWeb;
        }

        public async Task<AveBPOSAccountInfo> GetAveBPOSInfoAsync(RemoteSiteCollection siteCollection)
        {
            AveBPOSAccountInfo result = null;
            if (!_aveBposInfoCache.TryGetValue(siteCollection.id, out result))
            {
                result = await PoolUserUtil.GetBPOSInfoAsync(siteCollection);
                if (result != null)
                {
                    _aveBposInfoCache.Add(siteCollection.id, result);
                }
            }
            return result;
        }
        public async Task<BposInfo> CreateBposInfoAsync(RemoteSiteCollection sc)
        {
            _logger.Info($"SCUrl:[{sc.url}] ConnectionType:[{sc.AuthType.ToString()}] AppType:[{sc.AppType.ToString()}] ");
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
        public Guid GetAveObjId(object aveSPObj, RemoteSiteCollection remoteSC)
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
            }
            else
            {
                return Guid.Empty;
            }
        }
        public async Task CreateParentNodesAsync(object curAveObj, RMSPTreeNode nodeInheritFrom, RemoteSiteCollection remoteSC, RMSPTreeNode curNode, RMSPSampleTreeNode nodeParentSiteCollection)
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

                    var fullUrl = WebUtil.MakeFullUrl(remoteSC.url, web.ParentWeb.Url);
                    var parentWebTreeNode = ConstructNoSettingNode(NodeLevel.Site, web.ParentWeb.IsRootWeb ? "." : web.ParentWeb.Name, web.ParentWeb.ID, fullUrl, bposInfo);
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

                    var scTreeNode = ConstructNoSettingNode(NodeLevel.SiteCollection, web.Site.Url, new Guid(remoteSC.id), web.Site.Url, bposInfo);
                    curNode.Parent = scTreeNode;
                    curNode.ParentId = scTreeNode.Id;

                    parentObj = web.Site;
                    nextNode = scTreeNode;
                }
            }
            else if (curAveObj is IAveSite)
            {
                curNode.Parent = RMDtoConverter.ConvertSPTree2RMTree(RMDtoConverter.ConvertRMSampleTree2SPTree(nodeParentSiteCollection));
                curNode.ParentId = curNode.Parent.ParentId;
                return;
            }
            await CreateParentNodesAsync(parentObj, nodeInheritFrom, remoteSC, nextNode, nodeParentSiteCollection);
        }
        public RMSPTreeNode ConstructNoSettingNode(NodeLevel level, string name, Guid id, string fullPath, BposInfo bposInfo)
        {
            RMSPTreeNode node = new RMSPTreeNode();
            node.IconStatus = IconStatus.Inhert;
            node.SPType = (int)SPType.BPOS;
            node.SPVersion = GConstants.SPVersion.MOSS13;
            node.Expanded = true;
            node.Level = (int)level;
            node.Name = name;
            node.Id = id.ToString();
            node.SPObjectId = id.ToString();
            node.FullPath = fullPath;
            node.Expanded = true;
            node.BposInfo = bposInfo;
            return node;
        }
        public string GetAveObjTitleAndLevel(object aveSPObj, ref NodeLevel level)
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
                level = NodeLevel.List;
                return aveList.Title;
            }
            else if (aveSPObj is IAveWeb)
            {
                var web = (IAveWeb)aveSPObj;
                level = NodeLevel.Site;
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
        public void DisposeWebCache()
        {
            foreach (var web in _aveWebCache.Values)
            {
                try
                {
                    using (web) { }
                }
                catch (Exception e)
                {
                    _logger.Warn($"Dipose web error.Url:[{web.Url}] Error:{e.ToString()}");
                }
            }
        }
        public void DisposeSiteCache()
        {
            foreach (var site in _aveSiteCache.Values)
            {
                try
                {
                    using (site) { }
                }
                catch (Exception e)
                {
                    _logger.Warn($"Dipose site error.Url:[{site.Url}] Error:{e.ToString()}");
                }
            }
        }
    }
}
