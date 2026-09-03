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
using AvePoint.Hybrid.ClientCore;
using AvePoint.Hybrid.ClientLibrary;
using AvePoint.Hybrid.ClientLibrary.SDK;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.SharePointBrowser;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using AvePoint.RA.RACommonUtility.Common;

namespace AvePoint.RA.RACommonUtility.Browser
{
    public class SharePointBrowser
    {

        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected static readonly IRMRemoteNodeService RemoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private static readonly Dictionary<NodeLevel, Func<SPTreeNodeDto, List<SPTreeNodeDto>>> NodeLevelMapping = new Dictionary<NodeLevel, Func<SPTreeNodeDto, List<SPTreeNodeDto>>>()
        {
            {NodeLevel.Root, RootBrowse },
            //{NodeLevel.Farm, FarmBrowse },
            {NodeLevel.WebApplication, WebApplicationBrowse },
            {NodeLevel.SkyDriveProGroup, WebApplicationBrowse },
            {NodeLevel.Site, SiteBrowse },
            {NodeLevel.RootFolder, FolderBrowse },
            {NodeLevel.Folder, FolderBrowse }
        };

        public static SPTreeMessage Browse(SPTreeMessage message, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline)
        {
            Logger.Info($"Start browse sharepoint tree, level: {message.Node.Level}.");
            var children = new List<SPTreeNodeDto>();
            if (NodeLevel.Farm == message.Node.Level)
            {
                children = FarmBrowse(message.Node, type);
            }
            else
            {
                if (!NodeLevelMapping.TryGetValue(message.Node.Level, out var browseFunc))
                {
                    return BposChildrenBrowse(message, type == RMBrowseTreeNodeSourceType.SkyDrivePro && !message.Node.IsSOMode ? BrowserType.OneDrive : BrowserType.SharePointOnline);
                }
                children = browseFunc(message.Node);
            }

            SetNodesProperties(children, message.Node);
            var res = new SPTreeMessage
            {
                Node = message.Node,
                NodeList = children,
                ChildrenCount = children.Count()
            };
            SetTreeCredentialPasswordEmpty(res.Node);
            Logger.Info($"End browse sharepoint tree, level: {message.Node.Level}, children count: {res.ChildrenCount}.");
            return res;
        }

        private static List<SPTreeNodeDto> RootBrowse(SPTreeNodeDto node)
        {
            const string FarmName = "Remote Farm";
            const string FarmDisplayName = "My Registered Sites";
            const int FarmOffset = 0;
            var id = Guid.NewGuid().ToString();

            return new List<SPTreeNodeDto>
            {
                new SPTreeNodeDto()
                {
                    SPType = SPType.BPOS,
                    ID = id,
                    SPObjectId = id,
                    Name = FarmName,
                    Level = NodeLevel.Farm,
                    Type = NodeType.Unused,
                    CanChildrenBeLoaded = true,
                    FarmID = id,
                    Offset = FarmOffset,
                    DisplayName = FarmDisplayName
                }
            };
        }

        private static List<SPTreeNodeDto> FarmBrowse(SPTreeNodeDto node, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline)
        {
            var res = new List<SPTreeNodeDto>();
            try
            {
                Logger.Info($"Begin browse sharepoint all container. type : {type}");
                var webAppList = RemoteNodeService.GetAllWebApplications(type);
                foreach(var webApp in webAppList)
                {
                    var nodeDto = new SPTreeNodeDto()
                    {
                        ID = webApp.id,
                        SPObjectId = webApp.id,
                        Name = webApp.url,
                        DisplayName = webApp.url,
                        FullPath = webApp.url,
                        Level = NodeLevel.WebApplication,
                        Type = ConvertRemoveNodeType2ContainerNodeType(webApp.NodeType),
                        SPType = SPType.BPOS,
                        FarmID = node.FarmID
                    };
                    res.Add(nodeDto);
                }

                Logger.Info("End browse sharepoint all container. count: {}");
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while browse sharepoint all container. Error: {e}");
            }
            return res;
        }

        private static List<SPTreeNodeDto> WebApplicationBrowse(SPTreeNodeDto node)
        {
            var res = new List<SPTreeNodeDto>();
            try
            {
                Logger.Info($"Begin browse sharepoint site collections by container id: {node.ID}, name: {node.Name}.");
                var states = new SiteCollectionState[] { SiteCollectionState.AccessAll, SiteCollectionState.AccessSome };
                var siteCollections = RemoteNodeService.GetRemoteSiteCollectionsByParentId(node.SPObjectId, states);
                Logger.Info($"Success browse sharepoint site colllectioins, count: {siteCollections.Count}.");
                RMAosApiClient.SetPassWordBySiteCollectionuserName(siteCollections);
                var scUrlToAppProfileDict = RMAosApiClient.GetRemoteNodeUrlToAppProfileDict(siteCollections, TenantLocalValue.LogonGroupId);
                Dictionary<string, string> teamId2TeamName = new Dictionary<string, string>();
                var teamIds = siteCollections.Where(s => !string.IsNullOrEmpty(s.TeamId)).Select(s => s.TeamId).Distinct().ToList();
                if (teamIds.Count != 0)
                {
                    teamId2TeamName = RemoteNodeService.GetTeamId2TeamNameDicByTeamIds(teamIds);
                }
                foreach (var siteCollection in siteCollections)
                {
                    var nodeDto = new SPTreeNodeDto()
                    {
                        ID = siteCollection.id,
                        SPObjectId = siteCollection.id,
                        Name = siteCollection.url,
                        DisplayName = GetDisplayNameByNodeType(siteCollection),
                        FullPath = siteCollection.url,
                        Type = ConvertRemoveNodeType2ContainerNodeType(siteCollection.NodeType),
                        SPType = SPType.BPOS,
                        FarmID = node.FarmID,
                        Level = NodeLevel.SiteCollection,
                        O365TenantId = siteCollection.TenantId,
                        IsOrphenOneDrive = siteCollection.NodeType == RemoveNodeType.SkyDrivePro && string.IsNullOrEmpty(siteCollection.Name)
                    };
                    nodeDto.NodeExtension.BposInfo = new BposInfo()
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
                    nodeDto.NodeExtension.TemplateName = siteCollection.TemplateName;
                    nodeDto.NodeExtension.IsPublicWebSite = siteCollection.IsPublicWebSite;
                    nodeDto.NodeExtension.BposInfo.AddCertInfo(siteCollection, scUrlToAppProfileDict);
                    if (node.NodeExtension.TreeType == TreeType.ReplicatorSrcTree)
                    {
                        nodeDto.NodeExtension.BposInfo.ConnectionType = (siteCollection.AuthType == BposConnectionType.AppToken && !string.IsNullOrEmpty(siteCollection.ServiceAccountId))
                            ? BposConnectionType.ServiceAccount : siteCollection.AuthType;
                    }
                    string teamName = null;
                    if (!string.IsNullOrEmpty(siteCollection.TeamId) && teamId2TeamName.TryGetValue(siteCollection.TeamId, out teamName))
                    {
                        nodeDto.TeamName = teamName;
                    }
                    res.Add(nodeDto);
                }
                Logger.Info("End browse sharepoint site collections.");
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while browse sharepoint site collections. Error: {e}");
            }
            return res;
        }

        private static List<SPTreeNodeDto> FolderBrowse(SPTreeNodeDto node)
        {
            var res = new List<SPTreeNodeDto>();
            var items = CreateVirtualNode(NodeLevel.Items, GConstants.SPNodeName.Items);
            items.PageNodeType = PageNodeType.PreNext;
            items.FarmID = node.FarmID;
            items.Offset = 0;
            items.SPVersion = node.SPVersion;

            var folders = CreateVirtualNode(NodeLevel.Folders, GConstants.SPNodeName.Folders);
            folders.FarmID = node.FarmID;
            folders.Offset = items.Offset + 1;
            folders.SPVersion = node.SPVersion;
            folders.SPVersion = node.SPVersion;

            res.Add(items);
            res.Add(folders);
            return res;
        }

        private static List<SPTreeNodeDto> SiteBrowse(SPTreeNodeDto node)
        {
            var res = new List<SPTreeNodeDto>();

            var lists = CreateVirtualNode(NodeLevel.Lists, GConstants.SPNodeName.Lists);
            lists.FarmID = node.FarmID;
            lists.Offset = 0;
            lists.SPVersion = node.SPVersion;

            var sites = CreateVirtualNode(NodeLevel.Sites, GConstants.SPNodeName.Sites);
            sites.FarmID = node.FarmID;
            sites.Offset = lists.Offset + 1;
            sites.SPVersion = node.SPVersion;

            res.Add(lists);
            res.Add(sites);

            if (node.SPVersion == GConstants.SPVersion.MOSS13)
            {
                var apps = CreateVirtualNode(NodeLevel.Apps, GConstants.SPNodeName.Apps);
                apps.FarmID = node.FarmID;
                apps.Offset = lists.Offset + 2;
                apps.SPVersion = node.SPVersion;
                res.Add(apps);
            }
            return res;
        }

        private static SPTreeMessage BposChildrenBrowse(SPTreeMessage message, BrowserType browserType)
        {
            var siteNode = message.Node.GetSiteCollectionNode();
            using var siteStateScope = new SiteStateTransitionScopeUtility(siteNode?.FullPath, Wrapper.Common.SiteState.ReadOnly, true);
            var clinet = GetClient();
            var contract = new RABrowserContract(JsonConvert.SerializeObject(message), browserType, TenantLocalValue.LogonUserEmail, TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserId);
            var result = Task.Run(() => clinet.SharePointBrowserService.Browser(contract)).Result;
            return JsonConvert.DeserializeObject<SPTreeMessage>(result, SerializerSettings);
        }

        private static void SetTreeCredentialPasswordEmpty(SPTreeNodeDto node)
        {
            if (node != null)
            {
                if (node.Level == NodeLevel.SiteCollection)
                {
                    if (node?.NodeExtension?.BposInfo?.UserAccountInfo != null)
                    {
                        node.NodeExtension.BposInfo.UserAccountInfo.Password = string.Empty;
                    }
                }
                else if (node.Level > NodeLevel.SiteCollection)
                {
                    SetTreeCredentialPasswordEmpty(node.Parent);
                }
            }
        }

        private static void SetNodesProperties(IList<SPTreeNodeDto> children, SPTreeNodeDto currentNode)
        {
            if (children != null)
            {
                foreach (SPTreeNodeDto child in children)
                {
                    child.SPType = SPType.BPOS;
                    if (child.Level != NodeLevel.ItemVersion && child.Level != NodeLevel.AppData)
                    {
                        child.CanChildrenBeLoaded = true;
                    }
                    if (currentNode != null)
                    {
                        child.SPVersion = currentNode.SPVersion;
                    }
                }
            }
        }

        public static SPTreeNodeDto CreateVirtualNode(NodeLevel level, string name)
        {
            var id = Guid.NewGuid().ToString();
            var virtualNode = new SPTreeNodeDto()
            {
                ID = id,
                SPObjectId = id,
                Name = name,
                DisplayName = name,
                Level = level,
                FullPath = "",
                SPType = SPType.BPOS
            };
            return virtualNode;
        }

        public static NodeType ConvertRemoveNodeType2ContainerNodeType(RemoveNodeType removeNodeType)
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

        public static string GetDisplayNameByNodeType(RemoteSiteCollection siteCollection)
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

        private static HybridAgentApiClient GetClient()
        {
            var identityServer = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.IDENTITY_SERVICE_URL];
            var indentityClientId = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.CLIENT_ID_IN_IDENTITY_SERVICE];
            var apiUrl = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.RECO_API_URL];
            var certificate = RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords);

            var services = new ServiceCollection();
            services.AddHybridCloudSdk(RecordsConstants.RECORDS_APPLICATION_NAME, certificate)
                .ConfigureIdentityServer(identityServer, indentityClientId, HBContractConstants.HybridInernalScope, true)
                .ConfigureDefaultHttpClient("RABrowserClient", client =>
                {
                    client.ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        return new HttpClientHandler()
                        {
#if DEBUG
                            ServerCertificateCustomValidationCallback = (msg, cert, chain, err) => true
#endif
                        };

                    });
                })
                .AddHybridAgentApi(apiUrl);

            var serviceProvider = services.BuildServiceProvider();

            var factory = serviceProvider.GetService<ICloudSdkHybridAgentClientFactory>();

            return factory.CreateHybridAgentClient(TenantLocalValue.LogonGroupId, HBContractConstants.HybridInernalScope);
        }
    }
}
