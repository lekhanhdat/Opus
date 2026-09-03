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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Browser.IndividualLevel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using BposInfo = AvePoint.GCommon.Contract.CentralAdmin.Object.BposInfo;
using BPOSMode = AvePoint.GCommon.Contract.CentralAdmin.Object.BPOSMode;
using BposUserAccountInfo = AvePoint.GCommon.Contract.CentralAdmin.Object.BposUserAccountInfo;
using NodeLevel = AvePoint.GCommon.Contract.Tree.Object.NodeLevel;
using NodeType = AvePoint.GCommon.Contract.Tree.Object.NodeType;
using RemoveNodeType = AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType;
using SPTreeNodeDto = AvePoint.RA.Contract.Object.RMSPTreeNode;
using SPType = AvePoint.GCommon.Contract.Tree.Object.SPType;

namespace AvePoint.RA.ArchiverMigration.ArchiverMigration
{
    public class SPNodeMigrationService
    {
        protected RALogger logger = RALogger.GetInstance(typeof(SPNodeMigrationService));

        private Dictionary<string, RemoteWebApplication?> siteGroupInfoMapping = new Dictionary<string, RemoteWebApplication?>();

        private Dictionary<string, Guid> siteGroupNameAndIdMapping = new Dictionary<string, Guid>();
        private Dictionary<string, (Guid, Guid, Guid)> siteUrlAndIdMapping = new Dictionary<string, (Guid, Guid, Guid)>();  // value: (RemoteNodeId, SPObjectId, SiteGroupId)


        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();



        public Guid GetTargetSiteGroupId(string? sourceGroupName, int nodeLevel)
        {
            Guid targetGroupId = Guid.Empty;
            if (!string.IsNullOrEmpty(sourceGroupName))
            {
                var key = $"{sourceGroupName}|{nodeLevel}";
                if (!siteGroupNameAndIdMapping.TryGetValue(key, out targetGroupId))
                {
                    var groupId = RemoteNodeService.GetContainerIdByName(sourceGroupName, nodeLevel);
                    if (string.IsNullOrEmpty(groupId))
                    {
                        logger.Warn($"Can't find container. Name: {sourceGroupName}, Level: {nodeLevel}");
                    }
                    else
                    {
                        targetGroupId = new Guid(groupId);
                    }
                    siteGroupNameAndIdMapping[key] = targetGroupId;
                }
            }
            return targetGroupId;
        }

        // get sitecollection sp object id
        public Guid GetSPSiteId(string siteUrl)
        {
            return GetRemoteSiteInfo(siteUrl).Item2;
        }
        // get sitecollection node id
        public Guid GetSiteNodeId(string siteUrl)
        {
            return GetRemoteSiteInfo(siteUrl).Item1;
        }
        // get container id by site url
        public Guid GetGroupNodeId4Site(string siteUrl)
        {
            return GetRemoteSiteInfo(siteUrl).Item3;
        }

        // (RemoteNodeId, SPObjectId, SiteGroupId)
        private (Guid, Guid, Guid) GetRemoteSiteInfo(string siteUrl)
        {
            var spSiteId = Guid.Empty;
            var siteNodeId = Guid.Empty;
            var siteGroupId = Guid.Empty;
            if (!string.IsNullOrEmpty(siteUrl))
            {
                if (!siteUrlAndIdMapping.TryGetValue(siteUrl, out var siteIdInfo))
                {
                    var node = RemoteNodeService.GetRemoteSiteCollectionByUrl(siteUrl);
                    if (node != null)
                    {
                        if (!Guid.TryParse(node.ObjectId, out spSiteId))
                        {
                            logger.Error($"Get sp site id by url failed: {siteUrl}");
                        }
                        siteNodeId = new Guid(node.id);

                        if(!Guid.TryParse(node.parentId, out siteGroupId))
                        {
                            logger.Error($"Get site group id by site url failed: {siteUrl}");
                        }
                    }
                    else
                    {
                        logger.Error($"Could not find site by url: {siteUrl}");
                    }

                    siteIdInfo = (siteNodeId, spSiteId, siteGroupId);
                    siteUrlAndIdMapping[siteUrl] = siteIdInfo;
                }
                else
                {
                    return siteIdInfo;
                }

            }

            return (siteNodeId, spSiteId, siteGroupId);
        }

        public SourceFlag GetSourceFlagBySiteGroupId(string siteGroupId)
        {
            var siteGroup = GetSiteGroupInfo(siteGroupId);
            if(siteGroup != null)
            {
                return siteGroup.NodeType == RemoveNodeType.SkyDrivePro ? SourceFlag.OneDrive : SourceFlag.SharePoint;
            }
            return SourceFlag.None;
        }

        private RemoteWebApplication? GetSiteGroupInfo(string siteGroupId)
        {
            RemoteWebApplication? siteGroup = null;
            if (!string.IsNullOrEmpty(siteGroupId))
            {
                if (!siteGroupInfoMapping.TryGetValue(siteGroupId, out siteGroup))
                {
                    siteGroup = RemoteNodeService.GetWebApplicationById(siteGroupId);
                    siteGroupInfoMapping[siteGroupId] = siteGroup;
                }
            }
            return siteGroup;
        }

        private SPTreeNodeDto GetFarmNode()
        {
            const string FarmName = "Remote Farm";
            const string FarmDisplayName = "My Registered Sites";
            var id = Guid.NewGuid().ToString();
            return new SPTreeNodeDto()
            {
                SPType = (int)SPType.BPOS,
                Id = Guid.NewGuid().ToString(),
                SPObjectId = id,
                Name = FarmName,
                Level = (int)NodeLevel.Farm,
                //Type = Contract.Object.ContentSourceType.SharePoint,
                FarmId = id,
                DisplayName = FarmDisplayName
            };
        }

        private void SetParent(SPTreeNodeDto? node, SPTreeNodeDto? parentNode)
        {
            if (node == null || parentNode == null) return;

            node.Parent = parentNode;
            node.ParentId = parentNode.Id;
            node.FarmId = parentNode.FarmId;
        }

        public SPTreeNodeDto GetSiteGroupNodeWithParents(string siteGroupId)
        {
            var farmNode = GetFarmNode();
            var siteGroupNode = GetSiteGroupNode(siteGroupId, farmNode);

            return siteGroupNode;
        }

        private SPTreeNodeDto GetSiteGroupNode(string siteGroupId, SPTreeNodeDto parentNode)
        {
            var siteGroup = GetSiteGroupInfo(siteGroupId);
            if (siteGroup == null)
            {
                logger.Error($"Can't find site group by id: {siteGroupId}");
                return null;
            }

            return new SPTreeNodeDto()
            {
                Id = siteGroup.id,
                SPObjectId = siteGroup.id,
                Name = siteGroup.url,
                DisplayName = siteGroup.url,
                FullPath = siteGroup.url,
                Level = (int)NodeLevel.WebApplication,
                NodeType = (int)ConvertRemoveNodeType2ContainerNodeType(siteGroup.NodeType),
                SPType = (int)SPType.BPOS,
                FarmId = parentNode.FarmId,

                Parent = parentNode,
                ParentId = parentNode.Id,
            };
        }

        public SPTreeNodeDto GetSiteCollectionNodeWithParents(string siteUrl)
        {
            var siteCollection = RemoteNodeService.GetRemoteSiteCollectionByUrl(siteUrl);
            if (siteCollection == null)
            {
                logger.Error($"Can't find sitecollection by site url: {siteUrl}");
                return null;
            }
            var farmNode = GetFarmNode();
            var siteGroupNode = GetSiteGroupNode(siteCollection.parentId, farmNode);
            if (siteGroupNode == null) { return null; }
            var siteCollectionNode = GetSiteCollectionNode(siteCollection, siteGroupNode);

            return siteCollectionNode;
        }

        private SPTreeNodeDto GetSiteCollectionNode(RemoteSiteCollection siteCollection, SPTreeNodeDto parentNode)
        {
            var nodeDto = new SPTreeNodeDto()
            {
                Id = siteCollection.id,
                SPObjectId = siteCollection.id,
                Name = siteCollection.url,
                DisplayName = GetDisplayNameByNodeType(siteCollection),
                FullPath = siteCollection.url,
                NodeType = (int)ConvertRemoveNodeType2ContainerNodeType(siteCollection.NodeType),
                SPType = (int)SPType.BPOS,
                FarmId = parentNode.FarmId,
                Level = (int)NodeLevel.SiteCollection,
                O365TenantId = siteCollection.TenantId,

                Parent = parentNode,
                ParentId = parentNode.Id,
            };
            nodeDto.BposInfo = new BposInfo()
            {
                SiteUrl = string.Empty,
                AppType = siteCollection.AppType,
                ConnectionType = siteCollection.AuthType,
                UserAccountInfo = new BposUserAccountInfo()
                {
                    Domain = siteCollection.domain,
                    Username = siteCollection.username,
                    Password = string.Empty,
                    AdminUrl = siteCollection.AdminUrl,
                    TenantId = siteCollection.TenantId
                },
                Mode = new DateTime(siteCollection.CreateTime).AddDays(1) <= DateTime.UtcNow ? BPOSMode.Office365 : BPOSMode.Undetermined
            };
            //nodeDto.TemplateId = siteCollection.TemplateTitle;
            //nodeDto.IsPublicWebSite = siteCollection.IsPublicWebSite;
            //nodeDto.TeamName = teamName;

            return nodeDto;
        }

        public async Task<SPTreeNodeDto> GetRootSiteNodeWithParentsAsync(string siteUrl)
        {
            var siteCollectionNode = GetSiteCollectionNodeWithParents(siteUrl);
            if(siteCollectionNode == null)
            {
                return null;
            }

            using var aveSite = await GetAveSiteAsync(siteCollectionNode);

            return GetSiteNode(aveSite.RootWeb, siteCollectionNode);
        }

        public async Task<SPTreeNodeDto> GetSiteNodeWithParentsAsync(string siteUrl, Guid webId)
        {
            var siteCollectionNode = GetSiteCollectionNodeWithParents(siteUrl);
            if (siteCollectionNode == null)
            {
                return null;
            }

            using var aveSite = await GetAveSiteAsync(siteCollectionNode);
            var webNode = GetSiteNodeWithParents(siteCollectionNode, aveSite, webId);

            return webNode;
        }

        private SPTreeNodeDto GetSiteNodeWithParents(SPTreeNodeDto siteCollectionNode, IAveSite aveSite, Guid webId)
        {
            var currentWeb = aveSite.OpenWeb(webId);
            SPTreeNodeDto currentWebNode = GetSiteNode(currentWeb, null);

            IAveWeb web = currentWeb;
            SPTreeNodeDto webNode = currentWebNode;
            while (!web.IsRootWeb && web.ParentWeb != null)
            {
                var parentNode = GetSiteNode(web.ParentWeb, null);
                var sitesNode = GetSitesNode(parentNode);
                SetParent(webNode, sitesNode);

                web = web.ParentWeb;
                webNode = parentNode;
            }

            // set parent node for root web node
            SetParent(webNode, siteCollectionNode);

            return currentWebNode;
        }

        private async Task<IAveSite> GetAveSiteAsync(SPTreeNodeDto siteCollectionNode)
        {
            var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(new RemoteSiteCollection
            {
                url = siteCollectionNode.FullPath,
                TenantId = siteCollectionNode.O365TenantId
            });
            var factory = MultiAppUtil.CreateAveObjectModelFactory(siteCollectionNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);
            return factory.CreateSite(siteCollectionNode.FullPath);
        }

        private SPTreeNodeDto GetSiteNode(IAveWeb aveWeb, SPTreeNodeDto? parentNode)
        {
            SPTreeNodeDto webDto = new SPTreeNodeDto();
            webDto.FullPath = aveWeb.Url;
            webDto.SPObjectId = aveWeb.ID.ToString();
            if (aveWeb.IsRootWeb)
            {
                webDto.Name = ".";
            }
            else
            {
                webDto.Name = aveWeb.Name;
            }
            webDto.DisplayName = webDto.Name;
            webDto.FullPath = aveWeb.Url;

            webDto.TemplateId = aveWeb.WebTemplateId;
            webDto.Title = aveWeb.Title;
            webDto.Level = (int)NodeLevel.Site;
            webDto.FarmId = parentNode?.FarmId;

            SetParent(webDto, parentNode);

            return webDto;
        }

        private SPTreeNodeDto GetSitesNode(SPTreeNodeDto parentNode)
        {
            var sitesNode = CreateVirtualNode(NodeLevel.Sites, GConstants.SPNodeName.Sites);
            sitesNode.SPVersion = 0;

            SetParent(sitesNode, parentNode);
            return sitesNode;
        }

        private SPTreeNodeDto GetListsNode(SPTreeNodeDto parentNode)
        {
            var listsNode = CreateVirtualNode(NodeLevel.Lists, GConstants.SPNodeName.Lists);
            listsNode.SPVersion = 0;

            SetParent(listsNode, parentNode);
            return listsNode;
        }

        public async Task<SPTreeNodeDto> GetListNodeWithParentsAsync(string siteUrl, Guid webId, Guid listId)
        {
            var siteCollectionNode = GetSiteCollectionNodeWithParents(siteUrl);
            if (siteCollectionNode == null)
            {
                return null;
            }

            using var aveSite = await GetAveSiteAsync(siteCollectionNode);
            var listNode = GetListNodeWithParents(siteCollectionNode, aveSite, webId, listId);

            return listNode;
        }

        private SPTreeNodeDto GetListNodeWithParents(SPTreeNodeDto siteCollectionNode, IAveSite aveSite, Guid webId, Guid listId)
        {
            var webNode = GetSiteNodeWithParents(siteCollectionNode, aveSite, webId);
            var listsNode = GetListsNode(webNode);

            var web = aveSite.OpenWeb(webId);
            var list = web.GetList(listId);
            var listNode = GetListNode(list, listsNode, aveSite.Url);

            return listNode;
        }

        private SPTreeNodeDto GetListNode(IAveList list, SPTreeNodeDto parentNode, string siteUrl)
        {
            SPTreeNodeDto listDto = new SPTreeNodeDto();
            listDto.Name = list.Title;
            listDto.FullPath = new Uri(siteUrl).GetLeftPart(UriPartial.Authority) + list.RootFolder.ServerRelativeUrl;
            listDto.SPObjectId = list.ID.ToString();
            listDto.DisplayName = listDto.Name;
            listDto.Level = (int)NodeLevel.List;
            listDto.TemplateId = (int)list.BaseTemplate;
            listDto.Hidden = list.Hidden
                || list.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase)
                || (!list.AllowDeletion && !ScheduleConfiguration.ListTemplate.Contains((int)list.BaseTemplate));
            listDto.NodeType = list.BaseType == AveBaseType.DocumentLibrary ? (int)NodeType.DocumentLibrary : (int)NodeType.GenericList;
            listDto.Title = list.RootFolder.Name;

            SetParent(listDto, parentNode);
            return listDto;
        }

        public async Task<SPTreeNodeDto> GetFolderNodeWithParentsAsync(string siteUrl, Guid webId, Guid listId, string folderFullUrl)
        {
            var siteCollectionNode = GetSiteCollectionNodeWithParents(siteUrl);
            if (siteCollectionNode == null)
            {
                return null;
            }

            using var aveSite = await GetAveSiteAsync(siteCollectionNode);
            var listNode = GetListNodeWithParents(siteCollectionNode, aveSite, webId, listId);

            var web = aveSite.OpenWeb(webId);
            var list = web.GetList(listId);
            var rootFolderId = list.RootFolder.UniqueId;
            var folderServerRelativeUrl = new Uri(folderFullUrl).LocalPath;
            var currentFolder = web.GetFolder(folderServerRelativeUrl);
            var currentFolderNode = GetFolderNode(currentFolder);

            var folder = currentFolder;
            var folderNode = currentFolderNode;

            do
            {
                var parentNode = GetFolderNode(folder.ParentFolder);
                var foldersNode = GetFoldersNode(parentNode);
                SetParent(folderNode, foldersNode);

                folder = folder.ParentFolder;
                folderNode = parentNode;
            } while (folder.UniqueId != rootFolderId && folder.ParentFolder != null && folder.ParentFolder.Exists);

                // set parent node for root folder node
                SetParent(folderNode, listNode);

            return currentFolderNode;
        }

        private SPTreeNodeDto GetFoldersNode(SPTreeNodeDto parentNode)
        {
            var foldersNode = CreateVirtualNode(NodeLevel.Folders, GConstants.SPNodeName.Folders);
            foldersNode.SPVersion = 0;

            SetParent(foldersNode, parentNode);
            return foldersNode;
        }

        private SPTreeNodeDto GetFolderNode(IAveFolder folder)
        {
            var isRootFolder = folder.UniqueId == folder.ParentList.RootFolder.UniqueId;
            SPTreeNodeDto folderNode = new SPTreeNodeDto();
            folderNode.Name = isRootFolder ? "Root Folder" : folder.Name;
            folderNode.DisplayName = folderNode.Name;
            //folderNode.FullPath = folder.Url;
            folderNode.FullPath = folder.ServerRelativeUrl;
            folderNode.ParentId = folder.ParentListId.ToString();
            folderNode.Level = isRootFolder ? (int)NodeLevel.RootFolder : (int)NodeLevel.Folder;
            folderNode.SPObjectId = folder.UniqueId.ToString();

            return folderNode;
        }

        private SPTreeNodeDto CreateVirtualNode(NodeLevel level, string name)
        {
            var id = Guid.NewGuid().ToString();
            var virtualNode = new SPTreeNodeDto()
            {
                Id = id,
                SPObjectId = id,
                Name = name,
                DisplayName = name,
                Level = (int)level,
                FullPath = "",
                SPType = (int)SPType.BPOS
            };
            return virtualNode;
        }

        private NodeType ConvertRemoveNodeType2ContainerNodeType(RemoveNodeType removeNodeType)
        {
            switch (removeNodeType)
            {
                case RemoveNodeType.PrivateChannel:
                    return NodeType.PrivateChannelSitesGroup;
                case RemoveNodeType.O365GroupSites:
                    return NodeType.O365GroupSitesGroup;
                case RemoveNodeType.SkyDrivePro:
                    return NodeType.SkyDriveProSitesGroup;
                default:
                    return NodeType.SharePointSitesGroup;
            }
        }

        private string GetDisplayNameByNodeType(RemoteSiteCollection siteCollection)
        {
            if (siteCollection.NodeType == RemoveNodeType.PrivateChannel)
            {
                return siteCollection.url;
            }
            if (siteCollection.NodeType == RemoveNodeType.SkyDrivePro)
            {
                return siteCollection.Name;
            }
            if (siteCollection.NodeType == RemoveNodeType.O365GroupSites)
            {
                return siteCollection.Name;
            }
            return siteCollection.url;
        }

    }
}
