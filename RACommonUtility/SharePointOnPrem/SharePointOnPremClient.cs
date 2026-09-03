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
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Global.Exceptions;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.SharePoint;
using AvePoint.RA.Contract.SharePoint.OnPrem;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.SharePointOnPrem
{
    public class SharePointOnPremClient
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SharePointOnPremClient));

        private static readonly AveRetryPolicy RetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(12, TimeSpan.FromSeconds(10)));

        private static readonly IAgentMgmtService AgentMgmtService = PlatformWindsorManager.GetService<IAgentMgmtService>();

        private static readonly ISignalRService SignalRService = PlatformWindsorManager.GetService<ISignalRService>();

        private static readonly IFileSystemTreeCacheDao FileSystemTreeCacheDao = PlatformWindsorManager.GetService<IFileSystemTreeCacheDao>();

        private static readonly IRMLocalNodeService LocalNodeService = PlatformWindsorManager.GetService<IRMLocalNodeService>();

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private static readonly Dictionary<NodeLevel, Func<SPTreeNodeDto, List<SPTreeNodeDto>>> NodeLevelMapping = new Dictionary<NodeLevel, Func<SPTreeNodeDto, List<SPTreeNodeDto>>>()
        {
            {NodeLevel.Root, RootBrowse },
            {NodeLevel.Farm, FarmBrowse },
            {NodeLevel.WebApplication, WebApplicationBrowse },
            {NodeLevel.Site, SiteBrowse },
            {NodeLevel.RootFolder, FolderBrowse },
            {NodeLevel.Folder, FolderBrowse }
        };

        public static async Task<SPTreeMessage> BrowseFarmsAsync()
        {
            try
            {
                var farmNodes = new List<SPTreeNodeDto>();
                Logger.Info("Begin browse all farm.");
                var farmIds = (await AgentMgmtService.GetAvailableAgentsBySourceTypeAsync(TenantLocalValue.LogonGroupId, Hybrid.Contract.Object.SourceType.SharePoint)).Where(item => !string.IsNullOrEmpty(item.FarmId)).Select(item => item.FarmId).Distinct();
                foreach(var farmId in farmIds)
                {
                    farmNodes.Add(new SPTreeNodeDto
                    {
                        SPType = SPType.Moss,
                        ID = farmId,
                        SPObjectId = farmId,
                        Level = NodeLevel.Farm,
                        Type = NodeType.Unused,
                        CanChildrenBeLoaded = true,
                        FarmID = farmId
                    });
                }
                return new SPTreeMessage() { NodeList = farmNodes };
            }
            catch (Exception e)
            {
                Logger.Info($"An error occur while browse farms. Error: {e}");
            }
            return null;
        }

        public static async Task<SPTreeMessage> Browse4SLNJobOnlyAsync(SPTreeMessage message)
        {
            try
            {
                if (message?.Node == null)
                {
                    throw new ArgumentException("message", new ArgumentNullException("Node"));
                }
                if(message.Node.Level > NodeLevel.WebApplication)
                {
                    throw new ArgumentException("Not support browse to greater than web application level.");
                }
                var batchId = await BrowseForAgentAsync(message);
                var cache = GetReturnInfoFromDB(batchId);
                return JsonConvert.DeserializeObject<SPTreeMessage>(cache.TreeData, SerializerSettings);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occur while browse for sync local node job. Error: {e}");
            }
            return null;
        }

        #region Browse

        public static async Task<SPTreeMessage> BrowseAsync(SPTreeMessage message)
        {
            Logger.Info($"Start borwse sharepoint on-prem tree, level: {message.Node.Level}.");
            if(!NodeLevelMapping.TryGetValue(message.Node.Level, out var browseFunc))
            {
                var batchId = await BrowseForAgentAsync(message);
                var cache = GetReturnInfoFromDB(batchId);
                return JsonConvert.DeserializeObject<SPTreeMessage>(cache.TreeData, SerializerSettings);
            }
            var children = browseFunc(message.Node);
            children.Sort((node1, node2) => string.Compare(node1.Name, node2.Name, StringComparison.CurrentCulture));
            var res = new SPTreeMessage
            {
                Node = message.Node,
                NodeList = children,
                ChildrenCount = children.Count()
            };
            Logger.Info($"End browse sharepoint on-prem tree, level: {message.Node.Level}, children count: {res.ChildrenCount}.");
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
                    SPType = SPType.Moss,
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

        private static List<SPTreeNodeDto> FarmBrowse(SPTreeNodeDto node)
        {
            var res = new List<SPTreeNodeDto>();
            try
            {
                var webAppList = GetAllLocalWebApplicationsAsync().Result;
                foreach(var webApp in webAppList)
                {
                    res.Add(new SPTreeNodeDto
                    {
                        ID = webApp.Id,
                        SPObjectId = webApp.Id, // 应该用 ObjectId, 但为了统一 Online 逻辑, 同时为了保证相应 JOB 做小改动, 暂用 ID
                        Name = webApp.Name,
                        Url = webApp.Url,
                        FullPath = webApp.Url,
                        Level = NodeLevel.WebApplication,
                        SPType = SPType.Moss,
                        FarmID = webApp.FarmId,
                        CanChildrenBeLoaded = true
                    });
                }
                Logger.Info($"Successful browse sharepoint on-prem all web application. Count: [{res.Count}]");
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while browse sharepoint on-prem all web application. Error: {e}");
            }
            return res;
        }
        

        private static List<SPTreeNodeDto> WebApplicationBrowse(SPTreeNodeDto node)
        {
            var res = new List<SPTreeNodeDto>();
            try
            {
                var siteCollections = GetLocalSiteCollectionsByWebAppIdAsync(node.ID).Result;
                foreach (var site in siteCollections)
                {
                    res.Add(new SPTreeNodeDto
                    {
                        ID = site.Id,
                        SPObjectId = site.Id, // 应该用 ObjectId, 但为了统一 Online 逻辑, 同时为了保证相应 JOB 做小改动, 暂用 ID
                        Name = site.Url,
                        Url = site.Url,
                        FullPath = site.Url,
                        Level = NodeLevel.SiteCollection,
                        SPType = SPType.Moss,
                        FarmID = site.FarmId,
                        CanChildrenBeLoaded = true,
                        ParentId = node.ID,
                        Parent = node,
                    });
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while browse sharepoint site collections by web application: [{node?.FullPath}]. Error: [{e}]");
            }
            return res;
        }

        private static List<SPTreeNodeDto> FolderBrowse(SPTreeNodeDto node)
        {
            var res = new List<SPTreeNodeDto>();

            var items = CreateVirtualNode(NodeLevel.Items, GConstants.SPNodeName.Items, node.SPType);
            items.PageNodeType = PageNodeType.PreNext;
            items.FarmID = node.FarmID;
            items.Offset = 0;
            items.CanChildrenBeLoaded = true;

            var folders = CreateVirtualNode(NodeLevel.Folders, GConstants.SPNodeName.Folders, node.SPType);
            folders.FarmID = node.FarmID;
            folders.Offset = items.Offset + 1;
            folders.CanChildrenBeLoaded = true;

            res.Add(items);
            res.Add(folders);

            return res;
        }

        private static List<SPTreeNodeDto> SiteBrowse(SPTreeNodeDto node)
        {
            var res = new List<SPTreeNodeDto>();

            var lists = CreateVirtualNode(NodeLevel.Lists, GConstants.SPNodeName.Lists, node.SPType);
            lists.FarmID = node.FarmID;
            lists.Offset = 0;
            lists.SPVersion = node.SPVersion;
            lists.CanChildrenBeLoaded = true;

            var sites = CreateVirtualNode(NodeLevel.Sites, GConstants.SPNodeName.Sites, node.SPType);
            sites.FarmID = node.FarmID;
            sites.Offset = lists.Offset + 1;
            sites.SPVersion = node.SPVersion;
            sites.CanChildrenBeLoaded = true;

            res.Add(lists);
            res.Add(sites);

            if (node.SPVersion == GConstants.SPVersion.MOSS13)
            {
                var apps = CreateVirtualNode(NodeLevel.Apps, GConstants.SPNodeName.Apps, node.SPType);
                apps.FarmID = node.FarmID;
                apps.Offset = lists.Offset + 2;
                apps.SPVersion = node.SPVersion;
                apps.CanChildrenBeLoaded = true;
                res.Add(apps);
            }
            return res;
        }

        private static SPTreeNodeDto CreateVirtualNode(NodeLevel level, string name, SPType spType)
        {
            var id = Guid.NewGuid().ToString();
            return new SPTreeNodeDto
            {
                ID = id,
                SPObjectId = id,
                Name = name,
                Level = level,
                FullPath = "",
                SPType = spType
            };
        }

        #endregion

        #region Site Collection

        public static RMSiteCollection GetLocalSiteCollectionById(string id)
        {
            try
            {
                return LocalNodeService.GetLocalSiteCollectionById(id);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get local site collection by id: [{id}]. Error: {e}");
                throw;
            }
        }

        public static async Task<List<RMSiteCollection>> GetLocalSiteCollectionsByIdListAsync(List<string> ids)
        {
            try
            {
                return await LocalNodeService.GetLocalSiteCollectionsByIdListAsync(ids);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get local site collection by id list: [{string.Join(",", ids)}]. Error: {e}");
                throw;
            }
        }

        public static async Task<List<RMSiteCollection>> GetAllLocalSiteCollectionsAsync()
        {
            try
            {
                return await LocalNodeService.GetAllLocalSiteCollectionsAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get all local site collections. Error: {e}");
                throw;
            }
        }

        public static bool IsLocalSiteCollectionExistByUrl(string url)
        {
            try
            {
                return LocalNodeService.IsLocalSiteCollectionExistByUrl(url);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while is local site collection exist by url: [{url}]. Error: {e}");
                throw;
            }
        }

        public static RMSiteCollection GetLocalSiteCollectionByUrl(string url)
        {
            try
            {
                return LocalNodeService.GetLocalSiteCollectionByUrl(url);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get local site collection by url: [{url}]. Error: {e}");
                throw;
            }
        }

        public static async Task<List<RMSiteCollection>> GetLocalSiteCollectionsByWebAppIdAsync(string webappId)
        {
            try
            {
                return await LocalNodeService.GetLocalSiteCollectionsByWebAppIdAsync(webappId);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get local site collection by web application id: [{webappId}]. Error: {e}");
                throw;
            }
        }

        #endregion

        #region Web Application

        public static async Task<List<RMWebApplication>> GetAllLocalWebApplicationsAsync()
        {
            try
            {
                return await LocalNodeService.GetAllLocalWebApplicationsAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get all local web applications. Error: {e}");
                throw;
            }
        }

        public static RMWebApplication GetLocalWebApplicationById(string id)
        {
            try
            {
                return LocalNodeService.GetLocalWebApplicationById(id);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get local web applications by id: [{id}]. Error: {e}");
                throw;
            }
        }
        #endregion

        #region Term

        public static async Task<OnPremiseSPTermInfo> GetTermStoreInfoBySiteUrlAsync(string siteUrl)
        {
            try
            {
                var siteCollection = GetLocalSiteCollectionByUrl(siteUrl);
                if (siteCollection == null)
                {
                    throw new ArgumentException($"Can't find site collection: [{siteUrl}].");
                }
                var farmId = siteCollection.FarmId;
                var batchId = await BrowseTermForAgentAsync(siteUrl, farmId);
                var cache = GetReturnInfoFromDB(batchId);
                return JsonConvert.DeserializeObject<OnPremiseSPTermInfo>(cache.TreeData);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get term store info by site url: [{siteUrl}]. Error: {e}");
            }
            return null;
        }

        private static async Task<Guid> BrowseTermForAgentAsync(string message, string farmId)
        {
            if (string.IsNullOrEmpty(farmId))
            {
                throw new ArgumentNullException("farmId");
            }

            var batchId = Guid.NewGuid();

            Logger.Info("Begin get proxy");
            var proxy = RetryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            Logger.Info("End get proxy");

            var agents = await SignalRService.GetAgentsByFarmIdAsync(TenantLocalValue.LogonGroupId, farmId);
            Logger.Info($"Farm: [{farmId}] all available agent count: [{agents.Count}]");
            var agent = agents.FirstOrDefault();
            Logger.Info($"Farm: [{farmId}] used agent: [{agent?.AgentId}].");

            var args = new SharePointOnPremTermBrowserArgs
            {
                BatchId = batchId.ToString(),
                Message = message
            };

            var result = Task.Run(() =>
                proxy.InvokeOneAgentAysnc<SharePointOnPremTermBrowserExecute, SharePointOnPremTermBrowserArgs, SharePointOnPremTermBrowserResult>(agent, new SharePointOnPremTermBrowserExecute { MethodArgs = args })
            ).Result;

            if (result.Result == SharePointOnPremTermBrowserResultEnum.Failed)
            {
                Logger.Error($"Browser sharepoint on-prem message failed. Error: {result.Message}");
            }

            return batchId;
        }

        #endregion

        #region Others

        public static async Task<SharePointOnPremQuererResult> GetSPOnPremiseItem(Guid siteId, Guid webId, Guid listId, Guid itemId, bool isUsingExistColumnName = false, string existColumnName = "")
        {

            Logger.Info("Begin get proxy");
            var proxy = RetryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            Logger.Info("End get proxy");

            var siteCollection = GetLocalSiteCollectionById(siteId.ToString());
            if (siteCollection == null)
            {
                throw new ArgumentException($"Can't find site collection: [{siteId}].");
            }
            var farmId = siteCollection.FarmId;

            var agents = await SignalRService.GetAgentsByFarmIdAsync(TenantLocalValue.LogonGroupId, farmId);
            Logger.Info($"Farm: [{farmId}] all available agent count: [{agents.Count}]");
            var agent = agents.FirstOrDefault();
            Logger.Info($"Farm: [{farmId}] used agent: [{agent?.AgentId}].");

            var args = new SharePointOnPremQuererArgs
            {
                SiteUrl = siteCollection.Url,
                SiteId = siteId,
                WebId = webId,
                ListId = listId,
                ItemId = itemId,
                IsUsingExistColumnName = isUsingExistColumnName,
                ExistColumnName = existColumnName
            };
            
            var result = Task.Run(() =>
                proxy.InvokeOneAgentAysnc<SharePointOnPremQuererExecute, SharePointOnPremQuererArgs, SharePointOnPremQuererResult>(agent, new SharePointOnPremQuererExecute { MethodArgs = args })
            ).Result;

            return result;
        }

        public static async Task<SharePointOnPremRelatedResult> UpdateSPItemRelatedProperties(string siteUrl, Guid siteId, Guid webId, string webUrl, Guid listId, int itemRowId, string name, string relatedItemInfo)
        {
            Logger.Info("Begin get proxy");
            var proxy = RetryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            Logger.Info("End get proxy");

            var siteCollection = GetLocalSiteCollectionByUrl(siteUrl);
            if (siteCollection == null)
            {
                throw new ArgumentException($"Can't find site collection: [{siteUrl}].");
            }
            var farmId = siteCollection.FarmId;

            var agents = await SignalRService.GetAgentsByFarmIdAsync(TenantLocalValue.LogonGroupId, farmId);
            Logger.Info($"Farm: [{farmId}] all available agent count: [{agents.Count}]");
            var agent = agents.FirstOrDefault();
            Logger.Info($"Farm: [{farmId}] used agent: [{agent?.AgentId}].");

            var args = new SharePointOnPremRelatedArgs
            {
                SiteId = siteId,
                SiteUrl = siteUrl,
                WebId = webId,
                WebUrl = webUrl,
                ListId = listId,
                ItemRowId = itemRowId,
                Name = name,
                RelatedItemInfo = relatedItemInfo
            };

            var result = Task.Run(() =>
                proxy.InvokeOneAgentAysnc<SharePointOnPremRelatedExecute, SharePointOnPremRelatedArgs, SharePointOnPremRelatedResult>(agent, new SharePointOnPremRelatedExecute { MethodArgs = args })
            ).Result;

            return result;
        }

        public static async Task<SharePointOnPremDisposalResult> DisposeSPItems(SharePointOnPremDisposalArgs args)
        {
            Logger.Info("Begin get proxy");
            var proxy = RetryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            Logger.Info("End get proxy");

            var siteCollection = GetLocalSiteCollectionByUrl(args.SiteUrl);
            if (siteCollection == null)
            {
                throw new ArgumentException($"Can't find site collection: [{args.SiteUrl}].");
            }
            var farmId = siteCollection.FarmId;

            var agents = await SignalRService.GetAgentsByFarmIdAsync(TenantLocalValue.LogonGroupId, farmId);
            Logger.Info($"Farm: [{farmId}] all available agent count: [{agents.Count}]");
            var agent = agents.FirstOrDefault();
            Logger.Info($"Farm: [{farmId}] used agent: [{agent?.AgentId}].");
            var result = Task.Run(() =>
                proxy.InvokeOneAgentAysnc<SharePointOnPremDisposalExecute, SharePointOnPremDisposalArgs, SharePointOnPremDisposalResult>(agent, new SharePointOnPremDisposalExecute { MethodArgs = args })
            ).Result;

            return result;
        }
        
        public static Task<List<string>> GetRootParentIdsFromFolderAsync(string siteUrl, string webId, string listId, string folderServerRelativeUrl)
        {
            var args = new OnPremiseSPBrowseParentIdsArgs
            {
                SiteUrl = siteUrl,
                WebId = webId,
                ListId = listId,
                FolderServerRelativeUrl = folderServerRelativeUrl,
                Level = SharePointOnPremBrowseParentIdsLevel.Folder
            };
            return GetRootParentIdsAsync(args);
        }

        public static Task<List<string>> GetRootParentIdsFromWebAsync(string siteUrl, string webId)
        {
            var args = new OnPremiseSPBrowseParentIdsArgs
            {
                SiteUrl = siteUrl,
                WebId = webId,
                Level = SharePointOnPremBrowseParentIdsLevel.Web
            };
            return GetRootParentIdsAsync(args);
        }

        private static async Task<List<string>> GetRootParentIdsAsync(OnPremiseSPBrowseParentIdsArgs args)
        {
            var siteUrl = args.SiteUrl;
            var farmId = GetLocalSiteCollectionByUrl(siteUrl)?.FarmId;
            if(string.IsNullOrEmpty(farmId))
            {
                throw new ArgumentException($"Can't find farmId of site collection: [{siteUrl}].");
            }

            var batchId = await BrowseRootParentIdsForAgentAsync(JsonConvert.SerializeObject(args, SerializerSettings), farmId);
            var cache = GetReturnInfoFromDB(batchId);
            return JsonConvert.DeserializeObject<List<string>>(cache.TreeData);
        }

        private static async Task<Guid> BrowseRootParentIdsForAgentAsync(string message, string farmId)
        {
            if (string.IsNullOrEmpty(farmId))
            {
                throw new ArgumentNullException("farmId");
            }

            var batchId = Guid.NewGuid();

            Logger.Info("Begin get proxy");
            var proxy = RetryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            Logger.Info("End get proxy");

            var agents = await SignalRService.GetAgentsByFarmIdAsync(TenantLocalValue.LogonGroupId, farmId);
            Logger.Info($"Farm: [{farmId}] all available agent count: [{agents.Count}]");
            var agent = agents.FirstOrDefault();
            Logger.Info($"Farm: [{farmId}] used agent: [{agent?.AgentId}].");

            var args = new SharePointOnPremParentIdsBrowserArgs
            {
                BatchId = batchId.ToString(),
                Message = message
            };

            var result = Task.Run(() =>
                proxy.InvokeOneAgentAysnc<SharePointOnPremParentIdsBrowserExecute, SharePointOnPremParentIdsBrowserArgs, SharePointOnPremParentIdsBrowserResult>(agent, new SharePointOnPremParentIdsBrowserExecute { MethodArgs = args })
            ).Result;

            if (result.Result == SharePointOnPremParentIdsBrowserResultEnum.Failed)
            {
                Logger.Error($"Browser parent id of sharepoint on-prem message failed. Error: {result.Message}");
            }

            return batchId;
        }

        #endregion

        private static Task<Guid> BrowseForAgentAsync(SPTreeMessage message)
        {
            if(string.IsNullOrEmpty(message?.Node?.FarmID))
            {
                throw new ArgumentException("message", new ArgumentNullException("FarmId"));
            }
            var farmId = message.Node.FarmID;
            return BrowseForAgentAsync(JsonConvert.SerializeObject(message, SerializerSettings), farmId);
        }

        private static async Task<Guid> BrowseForAgentAsync(string message, string farmId)
        {
            if (string.IsNullOrEmpty(farmId))
            {
                throw  new ArgumentNullException("farmId");
            }

            var batchId = Guid.NewGuid();

            Logger.Info("Begin get proxy");
            var proxy = RetryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            Logger.Info("End get proxy");

            var agents = await SignalRService.GetAgentsByFarmIdAsync(TenantLocalValue.LogonGroupId, farmId);
            Logger.Info($"Farm: [{farmId}] all available agent count: [{agents.Count}]");
            var agent = agents.FirstOrDefault();
            Logger.Info($"Farm: [{farmId}] used agent: [{agent?.AgentId}].");

            var args = new SharePointOnPremBrowserArgs
            {
                BatchId = batchId.ToString(),
                Message = message
            };

            var result = Task.Run(() =>
                proxy.InvokeOneAgentAysnc<SharePointOnPremBrowserExecute, SharePointOnPremBrowserArgs, SharePointOnPremBrowserResult>(agent, new SharePointOnPremBrowserExecute { MethodArgs = args })
            ).Result;

            if (result.Result == SharePointOnPremBrowserResultEnum.Failed)
            {
                Logger.Error($"Browser sharepoint on-prem message failed. Error: {result.Message}");
            }

            return batchId;
        }

        private static FileSystemTreeCache GetReturnInfoFromDB(Guid batchId)
        {
            Logger.Info($"Start [GetReturnInfoFromDB] batchId: {batchId}");
            FileSystemTreeCache info = FileSystemTreeCacheDao.GetTreeNodeInfoByBatchId(batchId);
            if (info != null)
            {
                Logger.Info($"successfully get tree info from db");
                FileSystemTreeCacheDao.Delete(info);
            }
            else
            {
                Logger.Warn($"agent return success, but can not get return data from db.");
                throw new AgentNotifyWebApiException();
            }
            return info;
        }

    }
}
