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
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.GraphAPI;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Castle.Components.DictionaryAdapter.Xml;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType;

using TreeNodeType = AvePoint.GCommon.Contract.Tree.Object.NodeType;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMRemoteNodeDao : BaseDao<RMRemoteNode>, IRMRemoteNodeDao
    {
        private sealed class SearchSiteCollectionLazyLoadRow
        {
            public string Id { get; set; }
            public string ObjectId { get; set; }
            public string Url { get; set; }
            public string ParentId { get; set; }
            public int NodeLevel { get; set; }
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public int SiteCollectionType { get; set; }
            public string TeamId { get; set; }
            public string TenantId { get; set; }
            public string SPVersion { get; set; }
        }

        private sealed class TeamsSiteCollectionRow
        {
            public string Id { get; set; }
            public string Url { get; set; }
            public string ParentId { get; set; }
            public string DisplayName { get; set; }
            public string Name { get; set; }
            public int NodeLevel { get; set; }
            public int SiteCollectionType { get; set; }
            public string TenantId { get; set; }
            public string TeamId { get; set; }
            public int State { get; set; }
            public bool FromDAO { get; set; }
        }

        private ILnkUserRoleDao LnkUserRoleDao => PlatformWindsorManager.GetService<ILnkUserRoleDao>();

        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private RALogger logger = RALogger.GetInstance(typeof(RMRemoteNodeDao));
        private const string TABLE_NAME = "RMRemoteNodes";
        private const int NodeLevel_SiteCollection = (int)NodeLevel.SiteCollection;
        private const int NodeLevel_WebApplication = (int)NodeLevel.WebApplication;
        private const int NodeLevel_SkyDrivePro = (int)NodeLevel.SkyDrivePro;
        private const int NodeLevel_SkyDriveProGroup = (int)NodeLevel.SkyDriveProGroup;
        private const int NodeLevel_O365GroupSites = (int)NodeLevel.O365GroupSites;
        private const int NodeLevel_O365GroupSitesGroup = (int)NodeLevel.O365GroupSitesGroup;
        private const int NodeLevel_PrivateChannel = (int)NodeLevel.PrivateChannel;
        private const int NodeLevel_SharedChannel = (int)NodeLevel.SharedChannel;
        private const int NodeLevel_PrivateChannelSitesGroup = (int)NodeLevel.PrivateChannelGroup;
        private static bool IsGroupSiteType(int type) => type == (int)SiteCollectionType.Teams || type == (int)SiteCollectionType.Group;

        private static HashSet<string> _defaultContainerUrl = 
            [
                RMConstants.DEFAULT_SPSITES_GROUP, 
                RMConstants.DEFAULT_O365_SITES_GROUP, 
                RMConstants.DefaultPrivateChannelSitesGroup
            ];

        private string GetFullTableName(Core.RMDbContext context)
        {
            return $"[{context.SchemaName}].[{TABLE_NAME}]";
        }
        private string GetFullTableName()
        {
            return $"[{GetTenantSchemaName()}].[{TABLE_NAME}]";
        }

        public void DeleteRemoteWebApplication(List<string> ids)
        {
            ThrowUtil.ThrowIfNull(ids, nameof(ids));
            logger.Debug("DeleteRemoteWebApplication id count {0}", ids.Count);

            DatabaseUtility.BatchOperation(ids, batchIds =>
            {
                ExecuteWithRetry(context =>
                {
                    var inSql = DatabaseUtility.BuildInClause(batchIds, out var inParams);
                    string sql = $"DELETE FROM {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} WHERE Id IN {inSql};";
                    context.Database.ExecuteSqlCommand(sql, inParams.ToArray());
                });
            });
        }

        public void ClearAll()
        {
            logger.Debug("Clear Remote Nodes");
            ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 600;
                string sql = $"DELETE FROM {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))}";
                context.Database.ExecuteSqlCommand(sql);
            });
        }

        public void CreateRemoteWebApplications(List<RemoteWebApplication> webApplications)
        {
            ThrowUtil.ThrowIfNull(webApplications, "RemoteWebApplications");

            if (webApplications.Count == 0)
            {
                return;
            }

            var existsIDs = GetExistIDs(webApplications.Select(s => s.id));
            var nodeIDs = new HashSet<string>();
            var addingWebApps = new List<RMRemoteNode>();
            var containerNames = new List<string>();
            foreach (var webApplication in webApplications)
            {
                var nodeId = webApplication.id.ToLower();
                if (!nodeIDs.Add(nodeId))
                {
                    logger.Warn($"Repeat node id: {webApplication.id}, {webApplication.url}");
                    continue;
                }
                if (existsIDs.Contains(nodeId))
                {
                    logger.Warn($"Exists node id: {webApplication.id}, {webApplication.url}");
                    continue;
                }

                RMRemoteNode domain = new RMRemoteNode();
                ConvertToDomain(webApplication, domain);
                domain.CreateTime = DateTime.UtcNow.Ticks;
                addingWebApps.Add(domain);
                containerNames.Add(webApplication.url);
            }

            if (addingWebApps.Count == 0)
            {
                return;
            }

            ExecuteWithRetry(context =>
            {
                context.RMRemoteNodes.AddRange(addingWebApps);
                context.SaveChanges();
            });

            logger.Debug("CreateRemoteWebApplications: {0}", string.Join(", ", containerNames));
        }

        public void UpdateRemoteWebApplications(List<RemoteWebApplication> webApplications)
        {
            ThrowUtil.ThrowIfNull(webApplications, "RemoteWebApplications");
            var containerNames = new List<string>();
            foreach (var webApplication in webApplications)
            {
                ExecuteWithRetry(context =>
                {

                    var existWebApp = context.RMRemoteNodes.FirstOrDefault(item => item.Id == webApplication.id);
                    existWebApp.Url = webApplication.url;
                    ApplyCurrentValues(context, existWebApp);
                    containerNames.Add(existWebApp.Url);
                });
            }

            logger.Debug("UpdateRemoteWebApplications: {0}", string.Join(", ", containerNames));
        }

        #region Site Collection


        #endregion

        #region SkyDrive Pro
        #endregion

        #region Office365 Group Sites
        public List<string> GetO365GroupSiteUrlsByNames(List<string> names)
        {
            ThrowUtil.ThrowIfNull(names, nameof(names));
            var urls = new List<string>();
            DatabaseUtility.BatchOperation(names, batchNames =>
            {
                ExecuteWithRetry(context =>
                {
                    List<SqlParameter> paras = null;
                    string sql = $"SELECT url FROM {GetFullTableName(context)} WHERE NodeLevel = @nodeLevel AND Name IN {DatabaseUtility.BuildInClause(batchNames, out paras)};";
                    paras.Add(new SqlParameter("nodeLevel", NodeLevel_O365GroupSites));
                    urls.AddRange(context.Database.SqlQuery<string>(sql, paras.ToArray()).ToList());
                });
            });
            return urls;
        }
        #endregion

        public void CreateRemoteSiteCollectionsByCurrentGroupId(List<RemoteSiteCollection> siteCollections)
        {
            ThrowUtil.ThrowIfNull(siteCollections, "siteCollections");
            logger.Debug("CreateRemoteSiteCollection count {0}", siteCollections.Count);
            using (new PerformanceScope("CreateRemoteSiteCollection"))
            {
                var tableName = GetFullTableName();
                using (var table = ConvertToDataTable(siteCollections))
                {
                    if (table.Rows.Count == 0)
                    {
                        return;
                    }
                    logger.Debug("Finish convert DataTable.");
                    table.TableName = tableName;
                    BatchAdd(table, tableName);
                }
            }
        }
        private DataTable ConvertToDataTable(List<RemoteSiteCollection> siteCollections)
        {
            Dictionary<string, string> encryptUserNames = new Dictionary<string, string>();
            string tempEncryptName = null;
            var table = new DataTable();
            table.Columns.Add("Id", typeof(String));
            table.Columns.Add("ObjectId", typeof(String));
            table.Columns.Add("DomainName", typeof(String));
            table.Columns.Add("UserName", typeof(String));
            table.Columns.Add("Password", typeof(String));
            table.Columns.Add("Url", typeof(String));
            table.Columns.Add("ParentId", typeof(String));
            table.Columns.Add("State", typeof(Int32));
            table.Columns.Add("AgentGroupId", typeof(String));
            table.Columns.Add("AgentGroupName", typeof(String));
            table.Columns.Add("Description", typeof(String));
            table.Columns.Add("ModifiedDate", typeof(String));
            table.Columns.Add("BposMode", typeof(String));
            table.Columns.Add("CreateTime", typeof(Int64));
            table.Columns.Add("TemplateName", typeof(String));
            table.Columns.Add("SPVersion", typeof(String));
            table.Columns.Add("NodeLevel", typeof(Int32));
            table.Columns.Add("Name", typeof(String));
            table.Columns.Add("DisplayName", typeof(String));
            table.Columns.Add("AvailableAgentIds", typeof(String));
            table.Columns.Add("TemplateTitle", typeof(String));
            table.Columns.Add("IsPublicWebSite", typeof(Boolean));
            table.Columns.Add("SiteCollectionType", typeof(Int32));
            table.Columns.Add("AdminUrl", typeof(String));
            table.Columns.Add("ServiceAccountId", typeof(String));
            table.Columns.Add("TenantId", typeof(String));
            table.Columns.Add("AuthType", typeof(Int32));
            table.Columns.Add("AppType", typeof(Int32));
            table.Columns.Add("ScanSource", typeof(Int32));
            table.Columns.Add("TeamId", typeof(String));
            table.Columns.Add("SecondParentId", typeof(String));
            table.Columns.Add("FromDAO", typeof(Boolean));

            var existsIDs = GetExistIDs(siteCollections.Select(s => s.id));
            HashSet<string> nodeIDs = new HashSet<string>();
            foreach (var siteCollection in siteCollections)
            {
                var nodeId = siteCollection.id.ToLower();
                if (!nodeIDs.Add(nodeId))
                {
                    logger.Warn($"Repeat node id: {siteCollection.id}, {siteCollection.url}");
                    continue;
                }
                if (existsIDs.Contains(nodeId))
                {
                    logger.Warn($"Exists node id: {siteCollection.id}, {siteCollection.url}");
                    continue;
                }
                if (string.IsNullOrEmpty(siteCollection.username))
                {
                    tempEncryptName = null;
                }
                else if (!encryptUserNames.TryGetValue(siteCollection.username, out tempEncryptName))
                {
                    tempEncryptName = RMDatabaseDefaultEncryptor.EncryptToString(siteCollection.username);
                    encryptUserNames[siteCollection.username] = tempEncryptName;
                }
                var row = table.NewRow();
                row["Id"] = siteCollection.id;
                row["ObjectId"] = siteCollection.ObjectId;
                row["DomainName"] = siteCollection.domain;
                row["UserName"] = tempEncryptName;
                row["Password"] = null;
                row["Url"] = siteCollection.url;
                row["ParentId"] = siteCollection.parentId;
                row["State"] = (int)siteCollection.state;
                row["AgentGroupId"] = null;
                row["AgentGroupName"] = null;
                row["Description"] = null;
                row["ModifiedDate"] = DateTime.UtcNow.Ticks;
                row["BposMode"] = siteCollection.BPOSMould;
                row["CreateTime"] = DateTime.UtcNow.Ticks;
                row["TemplateName"] = siteCollection.TemplateName;
                row["SPVersion"] = siteCollection.SPVersion;
                row["NodeLevel"] = (int)ConvertRemoteNodeTypeToNodeLevel(siteCollection);
                row["Name"] = siteCollection.Name;
                row["DisplayName"] = null;
                row["AvailableAgentIds"] = null;
                row["TemplateTitle"] = siteCollection.TemplateTitle;
                row["IsPublicWebSite"] = siteCollection.IsPublicWebSite;
                row["SiteCollectionType"] = (int)siteCollection.SiteCollectionType;
                row["AdminUrl"] = siteCollection.AdminUrl;
                row["ServiceAccountId"] = siteCollection.ServiceAccountId;
                row["TenantId"] = siteCollection.TenantId;
                row["AuthType"] = (int)siteCollection.AuthType;
                row["AppType"] = (int)siteCollection.AppType;
                row["ScanSource"] = (int)siteCollection.ScanSource;
                row["TeamId"] = siteCollection.TeamId;
                row["SecondParentId"] = null;
                row["FromDAO"] = siteCollection.FromDAO;
                table.Rows.Add(row);
            }

            return table;
        }

        private HashSet<string> GetExistIDs(IEnumerable<string> ids)
        {
            HashSet<string> idList = new HashSet<string>();
            DatabaseUtility.BatchOperation<string>(ids, (batchIds) =>
            {
                ExecuteWithRetry(context =>
                {
                    List<SqlParameter> paras = null;
                    string sql = $"SELECT Id From {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} Where Id in {DatabaseUtility.BuildInClause(batchIds, out paras)}";
                    var exists = context.Database.SqlQuery<string>(sql, paras.ToArray()).ToList();
                    foreach (var id in exists)
                    {
                        idList.Add(id.ToLower());
                    }
                });
            });
            return idList;
        }

        public void DeleteRemoteSiteCollectionsByUrl(IEnumerable<string> urls)
        {
            ThrowUtil.ThrowIfNull(urls, nameof(urls));
            DatabaseUtility.BatchOperation<string>(urls, (batchUrls) =>
            {
                ExecuteWithRetry(context =>
                {
                    List<SqlParameter> paras = null;
                    string sql = $"Delete From {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} Where Url in {DatabaseUtility.BuildInClause(batchUrls, out paras)}";
                    context.Database.ExecuteSqlCommand(sql, paras.ToArray());
                });
            });
        }

        public void DeleteRemoteSiteCollectionByParentId(IEnumerable<string> parentIds)
        {
            ThrowUtil.ThrowIfNull(parentIds, nameof(parentIds));
            DatabaseUtility.BatchOperation<string>(parentIds, (batchIds) =>
            {
                ExecuteWithRetry(context =>
                {
                    context.Database.CommandTimeout = 600;
                    List<SqlParameter> paras = null;
                    string sql = $"Delete From {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} Where ParentId in {DatabaseUtility.BuildInClause(batchIds, out paras)}";
                    context.Database.ExecuteSqlCommand(sql, paras.ToArray());
                });
            });
        }

        public List<RemoteNodePara> GetRemoteWebApplicationNodes()
        {
            List<RemoteNodePara> result = null;
            ExecuteWithRetry(context =>
            {
                result = context.RMRemoteNodes
                   .AsNoTracking()
                   .Where(r => r.NodeLevel == NodeLevel_WebApplication || r.NodeLevel == NodeLevel_SkyDriveProGroup ||
                        r.NodeLevel == NodeLevel_O365GroupSitesGroup)
                   .Select(r => new { r.Id, r.Url, r.NodeLevel, r.AosId })
                   .ToList()
                   .Select(r => new RemoteNodePara()
                   {
                       NodeId = r.Id,
                       NodeName = r.Url,
                       NodeType = ConvertNodeLevelToType(r.NodeLevel),
                       NodeLevel = (NodeLevel)r.NodeLevel,
                       AosId = r.AosId
                   }).ToList();
            });
            return result;
        }

        public Dictionary<string, string> GetContainerNameBySiteUrls(IEnumerable<string> urls)
        {
            var items = ExecuteWithRetry(context =>
            {
                return (from p in context.RMRemoteNodes
                        from c in context.RMRemoteNodes
                        where p.Id == c.ParentId && urls.Contains(c.Url)
                        select new { Container = p.Url, c.Url }
                    ).ToList();
            });
            var results = new Dictionary<string, string>();
            foreach (var item in items)
            {
                results[item.Url?.ToLower()] = item.Container;
            }
            return results;
        }

        public int GetRemoteNodesCount()
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes.Count();
            });
        }

        public List<SyncRemoteNodePara> GetAllSiteCollectionNodesByPage(int pageIndex, int pageSize)
        {
            var tenantGroupId = TenantLocalValue.LogonGroupId;
            if (string.IsNullOrEmpty(tenantGroupId))
            {
                logger.Warn("Current tenant group is null");
                return null;
            }

            return ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 900;
                var sqlParams = new SqlParameter[]
                {
                    new SqlParameter("SiteLevel", NodeLevel_SiteCollection),
                    new SqlParameter("OneDriveLevel", NodeLevel_SkyDrivePro),
                    new SqlParameter("O365GroupSitesLevel", NodeLevel_O365GroupSites),
                    new SqlParameter("GroupId", tenantGroupId),
                };
                var sql =
$@"Select r.Url AS NodeName,r.ParentId,r.AuthType,r.AppType,r.ServiceAccountId,r.ScanSource,
    r.TenantId,r.TeamId,r.NodeLevel,r.SecondParentId, r.ObjectId
From {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} as r 
Where (r.NodeLevel = @SiteLevel or r.NodeLevel = @OneDriveLevel or r.NodeLevel = @O365GroupSitesLevel)
ORDER BY r.CreateTime
OFFSET {pageIndex * pageSize} ROW FETCH NEXT {pageSize} ROWS ONLY
";
                return context.Database.SqlQuery<SyncRemoteNodePara>(sql, sqlParams).ToList();
            });
        }

        public void UpdateSyncSiteCollections(List<SyncRemoteNodePara> siteCollections)
        {
            var urlAndNodeMap = new Dictionary<string, SyncRemoteNodePara>();
            siteCollections.ForEach(m =>
            {
                if (!string.IsNullOrEmpty(m.NodeName))
                {
                    urlAndNodeMap[m.NodeName.ToLowerInvariant()] = m;
                }
            });
            SyncRemoteNodePara tempItem = null;
            DatabaseUtility.BatchOperation<string>(urlAndNodeMap.Keys, (batchUrls) =>
            {
                ExecuteWithRetry(context =>
                {
                    foreach (var node in context.RMRemoteNodes.Where(m => batchUrls.Contains(m.Url)))
                    {
                        if (!urlAndNodeMap.TryGetValue(node.Url.ToLowerInvariant(), out tempItem))
                        {
                            continue;
                        }
                        node.ParentId = tempItem.ParentId;
                        node.NodeLevel = (int)tempItem.NodeLevel;
                        node.Name = tempItem.RelatedName;
                        node.AppType = (int)tempItem.AppType;
                        node.AuthType = (int)tempItem.AuthType;
                        node.ServiceAccountId = tempItem.ServiceAccountId;
                        node.ScanSource = (int)tempItem.ScanSource;
                        node.TenantId = tempItem.TenantId;
                        node.ModifiedDate = DateTime.UtcNow.Ticks;
                        node.TeamId = tempItem.TeamId;
                        node.SecondParentId = tempItem.SecondParentId;
                    }
                    context.SaveChanges();
                });
            });
        }
        public List<RemoteSiteCollection> GetRemoteSiteCollectionBySiteUrls(IEnumerable<string> urls)
        {
            return GetRemoteSiteCollectionBySiteUrls(urls, null);
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionBySiteUrls(IEnumerable<string> urls, IEnumerable<string> containerId)
        {
            ThrowUtil.ThrowIfNull(urls, "urls");
            List<RemoteSiteCollection> siteCollections = new List<RemoteSiteCollection>();
            DatabaseUtility.BatchOperation(urls, batchUrls =>
            {
                batchUrls = batchUrls.ToList();
                ExecuteWithRetry(context =>
                {
                    var query = context.RMRemoteNodes.Where(r => batchUrls.Contains(r.Url));
                    if(containerId != null)
                    {
                        query = query.Where(r => containerId.Contains(r.ParentId));
                    }
                    var items = query.AsNoTracking().Select(r => new
                       {
                           r.Id,
                           r.DomainName,
                           r.Url,
                           r.ServiceAccountId,
                           r.TenantId,
                           r.ParentId,
                           r.AdminUrl,
                           r.Name,
                           r.State,
                           r.AuthType,
                           r.AppType,
                           r.NodeLevel,
                           r.ScanSource,
                           r.FromDAO,
                           r.ObjectId,
                       }).ToList()
                       .Select(r => new RemoteSiteCollection()
                       {
                           id = r.Id,
                           domain = r.DomainName,
                           Name = r.Name,
                           url = r.Url,
                           ServiceAccountId = r.ServiceAccountId,
                           TenantId = r.TenantId,
                           parentId = r.ParentId,
                           AdminUrl = r.AdminUrl,
                           state = (SiteCollectionState)r.State,
                           AuthType = (BposConnectionType)r.AuthType,
                           AppType = (AppType)r.AppType,
                           NodeType = ConvertNodeLevelToType(r.NodeLevel),
                           ScanSource = (RemoteNodeScanSource)r.ScanSource,
                           FromDAO = r.FromDAO,
                           ObjectId = r.ObjectId
                       }).ToList();
                    siteCollections.AddRange(items);
                });
            });
            return siteCollections;
        }

        public Dictionary<string, string> GetTeamsIdsOfSites(IEnumerable<string> scUrls)
        {
            if(scUrls == null || !scUrls.Any())
            {
                return new ();
            }
            using (var context = GetNewContext())
            {
                return context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(r => scUrls.Contains(r.Url) && r.TeamId != null 
                    && new List<int>() { (int)SiteCollectionType.Teams, (int)SiteCollectionType.Group, (int)SiteCollectionType.PrivateChannel }.Contains(r.SiteCollectionType))
                    .ToDictionary(node => node.Url, node => node.TeamId);
            }
        }

        public HashSet<string> GetHavePermissionTeams(IEnumerable<string> teamIds, IEnumerable<string> permissionContainer)
        {
            if (teamIds == null || !teamIds.Any() || permissionContainer == null || !permissionContainer.Any())
            {
                return new();
            }
            using (var context = GetNewContext())
            {
                return context.RMRemoteNodes.AsNoTracking().Where(r => teamIds.Contains(r.TeamId) && permissionContainer.Contains(r.ParentId)).Select(node => node.TeamId).ToHashSet();
            }
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionByParam(List<string> param, bool isUrl = true)
        {
            var sites = new List<RemoteSiteCollection>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            //var sql = "select Url, NodeLevel, Name, ServiceAccountId, TenantId, AuthType, AppType from RMRemoteNodes where" + (isUrl ? " Url" : " Name") + $" in {DatabaseUtility.BuildInClause(param, out parameters)}";
            using (var context = GetNewContext())
            {
                List<RMRemoteNode> remoteNodes = new List<RMRemoteNode>();
                if (isUrl)
                {
                    remoteNodes = context.RMRemoteNodes.AsNoTracking().Where(r => param.Contains(r.Url)).ToList();
                }
                else
                {
                    remoteNodes = context.RMRemoteNodes.AsNoTracking().Where(r => param.Contains(r.Name)).ToList();
                }
                foreach (var reader in remoteNodes)
                {
                    sites.Add(new RemoteSiteCollection()
                    {
                        ObjectId = reader.ObjectId,
                        url = reader.Url,
                        Name = reader.Name,
                        ServiceAccountId = reader.ServiceAccountId,
                        TenantId = reader.TenantId,
                        AuthType = (BposConnectionType)reader.AuthType,
                        AppType = (AppType)reader.AppType,
                        TeamId = reader.TeamId,
                        ChannelType = reader.NodeLevel == (int)NodeLevel.PrivateChannel? TeamsChannelType.Private: reader.NodeLevel == (int)NodeLevel.SharedChannel ? TeamsChannelType.Shared: TeamsChannelType.None,
                        TemplateName = reader.TemplateName
                });
                }
            }
            return sites;
        }

        public int GetRemoteSiteCollectionCountByParentId(string parentId, bool includeOrphenNode = true)
        {
            ThrowUtil.ThrowIfNullOrEmpty(parentId, nameof(parentId));
            return ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 600;
                var queryStates = new[] { (int)SiteCollectionState.AccessAll, (int)SiteCollectionState.AccessSome };

                List<SqlParameter> stateParameters = null;
                string stateInClause = DatabaseUtility.BuildInClause(queryStates, out stateParameters);
                stateParameters.Add(new SqlParameter("@ParentId", parentId));

                string sql = $@"SELECT COUNT(1)
                    FROM {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} WITH (NOLOCK)
                    WHERE ParentId = @ParentId
                    AND (
                            NodeLevel = {NodeLevel_SiteCollection}
                            OR NodeLevel = {NodeLevel_SkyDrivePro}
                            OR NodeLevel = {NodeLevel_O365GroupSites}
                            OR NodeLevel = {NodeLevel_PrivateChannel}
                            OR NodeLevel = {NodeLevel_SharedChannel}
                        )
                    AND State IN {stateInClause}
                    AND ({(includeOrphenNode ? "1=1" : $"(Name IS NOT NULL OR NodeLevel != {NodeLevel_SkyDrivePro})")})";

                return context.Database.SqlQuery<int>(sql, stateParameters.ToArray()).FirstOrDefault();
            });
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionsByParentId(string parentId, SiteCollectionState[] states)
        {
            ThrowUtil.ThrowIfNullOrEmpty(parentId, "parentId");
            ThrowUtil.ThrowIfNull(states, "states");
            //var siteCollections = new List<RemoteSiteCollection>();
            var queryStates = states.Select(s => (int)s);
            return ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 600;
                var siteCollections = context.RMRemoteNodes
                    .Where(m => m.ParentId == parentId
                    && (
                        m.NodeLevel == NodeLevel_SiteCollection
                        || m.NodeLevel == NodeLevel_SkyDrivePro
                        || m.NodeLevel == NodeLevel_O365GroupSites
                        || m.NodeLevel == NodeLevel_PrivateChannel
                        || m.NodeLevel == NodeLevel_SharedChannel
                        )
                    && queryStates.Contains(m.State)
                    )
                    .OrderBy(n => n.Url)
                    .ToList();
                return siteCollections.ConvertAll(m => new RemoteSiteCollection()
                {
                    id = m.Id,
                    url = m.Url,
                    parentId = m.ParentId,
                    domain = m.DomainName,
                    state = (SiteCollectionState)m.State,
                    BPOSMould = m.BposMode,
                    CreateTime = m.CreateTime,
                    TemplateName = m.TemplateName,
                    SPVersion = m.SPVersion,
                    TemplateTitle = m.TemplateTitle,
                    IsPublicWebSite = m.IsPublicWebSite,
                    Name = m.Name,
                    NodeType = GetNodeTypeByNodeLevel(m.NodeLevel),
                    SiteCollectionType = (SiteCollectionType)m.SiteCollectionType,
                    AdminUrl = m.AdminUrl,
                    ServiceAccountId = m.ServiceAccountId,
                    TenantId = m.TenantId,
                    AuthType = (BposConnectionType)m.AuthType,
                    AppType = (AppType)m.AppType,
                    ScanSource = (RemoteNodeScanSource)m.ScanSource,
                    TeamId = m.TeamId,
                    AvailableAgentIds = m.AvailableAgentIds != null ? new List<string>(m.AvailableAgentIds.Split(',')) : null,
                    FromDAO = m.FromDAO
                });
            });
        }

        /// <summary>
        /// Retrieves a single page of site collections without running a total-count query.
        /// This method is intended for large-scale background traversal scenarios to reduce
        /// per-page database overhead.
        /// </summary>
        /// <param name="parentId">The ID of the parent node that contains the site collections.</param>
        /// <param name="states">The states of the site collections to include.</param>
        /// <param name="lastId">The ID of the last item retrieved in the previous page, used for keyset pagination.</param>
        /// <param name="pageSize">The number of items to retrieve in the current page.</param>
        /// <param name="includeOrphenNode">Whether orphan OneDrive nodes should be included.</param>
        /// <param name="types">Optional site collection types to filter in the database query.</param>
        /// <returns>The list of site collections for the current page.</returns>
        public List<RemoteSiteCollection> GetRemoteSiteCollectionsByParentIdByCursor(string parentId, SiteCollectionState[] states, ref string lastId, int pageSize, bool includeOrphenNode = true, SiteCollectionType[] types = null)
        {
            ThrowUtil.ThrowIfNullOrEmpty(parentId, "parentId");
            ThrowUtil.ThrowIfNull(states, "states");
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            var queryStates = states.Select(s => (int)s).ToList();
            var queryTypes = types?.Select(type => (int)type)?.ToList();
            var cursorLastId = lastId;
            var queryResult = ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 900;

                var queryResults = context.RMRemoteNodes
                    .Where(m => m.ParentId == parentId
                        && (
                            m.NodeLevel == NodeLevel_SiteCollection
                            || m.NodeLevel == NodeLevel_SkyDrivePro
                            || m.NodeLevel == NodeLevel_O365GroupSites
                            || m.NodeLevel == NodeLevel_PrivateChannel
                            || m.NodeLevel == NodeLevel_SharedChannel
                        )
                        && queryStates.Contains(m.State)
                        && (includeOrphenNode || m.Name != null || m.NodeLevel != (int)NodeLevel.SkyDrivePro));

                if (queryTypes != null && queryTypes.Count > 0)
                {
                    queryResults = queryResults.Where(m => queryTypes.Contains(m.SiteCollectionType));
                }

                if (!string.IsNullOrEmpty(cursorLastId))
                {
                    queryResults = queryResults.Where(m => string.Compare(m.Id, cursorLastId) > 0);
                }

                var list = queryResults
                    .OrderBy(m => m.Id)
                    .Take(pageSize)
                    .ToList();

                var nextLastId = list.Count > 0 ? list[list.Count - 1].Id : cursorLastId;
                return Tuple.Create(list.ConvertAll(ConvertToSiteCollection), nextLastId);
            });

            lastId = queryResult.Item2;
            return queryResult.Item1;
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionsByParentId(string parentId, SiteCollectionState[] states, SiteCollectionType[] types, string[] names)
        {
            ThrowUtil.ThrowIfNullOrEmpty(parentId, "parentId");
            ThrowUtil.ThrowIfNull(states, "states");
            ThrowUtil.ThrowIfNull(types, "types");
            //var siteCollections = new List<RemoteSiteCollection>();
            var queryStates = states.Select(s => (int)s);
            var querySiteCollectionType = types.Select(s => (int)s);
            return ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 600;
                var siteCollections = context.RMRemoteNodes
                    .Where(m => m.ParentId == parentId
                    && (
                        m.NodeLevel == NodeLevel_SiteCollection
                        || m.NodeLevel == NodeLevel_SkyDrivePro
                        || m.NodeLevel == NodeLevel_O365GroupSites
                        || m.NodeLevel == NodeLevel_PrivateChannel
                        || m.NodeLevel == NodeLevel_SharedChannel
                        )
                    && queryStates.Contains(m.State)
                    && querySiteCollectionType.Contains(m.SiteCollectionType));
                if (names != null && names.Any())
                {
                    siteCollections = siteCollections.Where(node => Enumerable.Contains(names, node.Name));
                }
                siteCollections = siteCollections.OrderBy(n => n.SiteCollectionType).ThenBy(n => n.Url);
                return siteCollections.ToList().ConvertAll(m => new RemoteSiteCollection()
                {
                    id = m.Id,
                    url = m.Url,
                    parentId = m.ParentId,
                    domain = m.DomainName,
                    state = (SiteCollectionState)m.State,
                    BPOSMould = m.BposMode,
                    CreateTime = m.CreateTime,
                    TemplateName = m.TemplateName,
                    SPVersion = m.SPVersion,
                    TemplateTitle = m.TemplateTitle,
                    IsPublicWebSite = m.IsPublicWebSite,
                    Name = m.Name,
                    NodeType = GetNodeTypeByNodeLevel(m.NodeLevel),
                    SiteCollectionType = (SiteCollectionType)m.SiteCollectionType,
                    AdminUrl = m.AdminUrl,
                    ServiceAccountId = m.ServiceAccountId,
                    TenantId = m.TenantId,
                    AuthType = (BposConnectionType)m.AuthType,
                    AppType = (AppType)m.AppType,
                    ScanSource = (RemoteNodeScanSource)m.ScanSource,
                    TeamId = m.TeamId,
                    AvailableAgentIds = m.AvailableAgentIds != null ? new List<string>(m.AvailableAgentIds.Split(',')) : null,
                    FromDAO = m.FromDAO
                });
            });
        }
        public RMSPSampleTreeNode GetSiteCollections(RMSPSampleTreeNode node, bool checkPermission, bool includeOrphenNode = false)
        {
            var parentId = node.Id;
            if (!checkPermission)
            {
                if (!string.IsNullOrEmpty(node.SearchKey))
                {
                    return GetChildrenNodesPaged(node, GetNewContext(), m => m.ParentId == parentId && m.Url.Contains(node.SearchKey)
                    && (includeOrphenNode || m.Name != null || m.NodeLevel != (int)NodeLevel.SkyDrivePro)
                    && (node.SourceType != (int)SourceFlag.Teams
                        || m.SiteCollectionType == (int)SiteCollectionType.Teams
                        || m.SiteCollectionType == (int)SiteCollectionType.Group));
                }

                return GetChildrenNodesPaged(node, GetNewContext(), m => m.ParentId == parentId
                    && (includeOrphenNode || m.Name != null || m.NodeLevel != (int)NodeLevel.SkyDrivePro)
                    && (node.SourceType != (int)SourceFlag.Teams
                        || m.SiteCollectionType == (int)SiteCollectionType.Teams
                        || m.SiteCollectionType == (int)SiteCollectionType.Group));
            }

            string userId = TenantLocalValue.LogonUserId;

            var orderingClause = "ORDER BY Url ASC";
            if (node.SourceType == (int)SourceFlag.Teams)
            {
                orderingClause = "ORDER BY SiteCollectionType ASC, Url ASC";
            }
            return GetAuthorizedChildrenNodesPaged(node, orderingClause, context =>
            {
                string queryAllSql =
$@"SELECT * FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMRemoteNodes WHERE EXISTS (
  SELECT ScopeId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMScopeRoleAssignments AS p 
    JOIN [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMSecurityGroupMemberships m ON p.GroupId=m.GroupId 
	  AND m.UserId IN (
        SELECT UserId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMAccounts WHERE IsRemoved=0 AND (
          UserId= @userId OR UserId IN (
            SELECT GroupId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMLnkUserGroups WHERE UserId= @userId
          )
	    )
      )
    WHERE p.ScopeId=@ParendId
) AND ParentId=@ParendId AND [Url] like '%' + @SearchKey + '%' ";
                if (!includeOrphenNode)
                {
                    queryAllSql += "And (Name is not null or NodeLevel != 6000)";
                }
                if (node.SourceType == (int)SourceFlag.Teams)
                {
                    queryAllSql += " And (SiteCollectionType = 2 or SiteCollectionType = 4) ";
                }
                return Tuple.Create(queryAllSql, new SqlParameter[] { new SqlParameter("@ParendId", parentId), new SqlParameter("@SearchKey", node.SearchKey ?? string.Empty), new SqlParameter("@userId", userId) });
            });
        }
        public RMSPSampleTreeNode GetSiteCollectionsUnderTeams(RMSPSampleTreeNode node)
        {
            var states = new [] { (int)SiteCollectionState.AccessAll, (int)SiteCollectionState.AccessSome };
            try
            {
                var context = GetNewContext();
                context.Database.CommandTimeout = 900;
                var queryResults = context.RMRemoteNodes.Where(m => m.TeamId == node.Parent.TeamsId
                    && (
                        m.NodeLevel == NodeLevel_O365GroupSites
                        || m.NodeLevel == NodeLevel_PrivateChannel
                        || m.NodeLevel == NodeLevel_SharedChannel
                        )
                    && Enumerable.Contains(states, m.State));
                int count = 0;
                using (new PerformanceScope("GetChildrenNodesPaged count"))
                {
                    count = queryResults.Count();
                }
                ResetPagerInfo(node, count);

                if (node.ChildrenCount > 0)
                {
                    // OrderBy SiteCollectionType to always show the teams/group site first => private sites => shared sites, ThenBy url
                    queryResults = queryResults
                        .OrderBy(n => n.SiteCollectionType).ThenBy(n => n.Url)
                        .Skip(node.PageIndex * node.PageSize).Take(node.PageSize);
                    logger.Info($"GetChildrenNodesPaged list: Get children list exp is : {queryResults.Expression}");
                    List<RMRemoteNode> list = null;
                    using (new PerformanceScope("GetChildrenNodesPaged list"))
                    {
                        list = queryResults.ToList();
                    }
                    node.Children = list.ConvertAll(Convert2SitesUnderTeamsTreeNode);
                }
                else
                {
                    node.Children = new List<RMSPSampleTreeNode>();
                }
                return node;
            }
            catch (Exception ex)
            {
                logger.Error($"GetChildrenNodesPaged error : {ex}");
                if (ex.InnerException != null)
                {
                    logger.Error($"GetChildrenNodesPaged InnerException : {ex.InnerException}");
                }
                throw;
            }
        }

        public List<Guid> GetOrphanedODIds()
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes.Where(item => item.Name == null && item.NodeLevel == (int)NodeLevel.SkyDrivePro).Select(item => new Guid(item.Id)).ToList();
            });
        }

        public RMSPSampleTreeNode GetSiteCollectionBySearch(RMSPSampleTreeNode node, bool checkPermission, string searchKey, bool includeOrphenNode = false)
        {
            var parentId = node.Id;
            var isExactlySearch = (node.SourceType == (int)SourceFlag.Teams || node.SourceType == (int)SourceFlag.SharePoint)
                && !string.IsNullOrEmpty(searchKey) && searchKey.StartsWith('"') && searchKey.EndsWith('"');
            var searchKeyTrimmed = isExactlySearch ? searchKey.Trim('"') : searchKey;
            if (!checkPermission)
            {
                using var context = GetNewContext();
                if (!string.IsNullOrEmpty(searchKey))
                {
                    return GetChildrenNodesPaged(node, context, m => m.ParentId == parentId 
                    && ((isExactlySearch && m.Url.Equals(searchKeyTrimmed)) || m.Url.Contains(searchKey))
                    && (includeOrphenNode || m.Name != null || m.NodeLevel != (int)NodeLevel.SkyDrivePro));
                }

                return GetChildrenNodesPaged(node, context, m => m.ParentId == parentId
                    && (includeOrphenNode || m.Name != null || m.NodeLevel != (int)NodeLevel.SkyDrivePro));
            }

            string userId = TenantLocalValue.LogonUserId;

            return GetAuthorizedChildrenNodesPagedForSearch(node, "ORDER BY Url ASC", context =>
            {
                string schema = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                string queryAllSql =
$@"SELECT * FROM [{schema}].RMRemoteNodes WHERE EXISTS (
  SELECT ScopeId FROM [{schema}].RMScopeRoleAssignments AS p 
    JOIN [{schema}].RMSecurityGroupMemberships m ON p.GroupId=m.GroupId 
	  AND m.UserId IN (
        SELECT UserId FROM [{schema}].RMAccounts WHERE IsRemoved=0 AND (
          UserId= @userId OR UserId IN (
            SELECT GroupId FROM [{schema}].RMLnkUserGroups WHERE UserId= @userId
          )
	    )
      )
    WHERE p.ScopeId=@ParentId
) ";
                if (isExactlySearch)
                {
                    queryAllSql += " AND [Url] = @SearchKey AND ParentId=@ParentId";
                }
                else
                {
                    queryAllSql += " AND ParentId=@ParentId AND [Url] like '%' + @SearchKey + '%' ";
                }
                if (!includeOrphenNode)
                {
                    queryAllSql += " And (Name is not null or NodeLevel != 6000) ";
                }
                return Tuple.Create(queryAllSql, new SqlParameter[] { new SqlParameter("@ParentId", parentId), new SqlParameter("@SearchKey", searchKeyTrimmed ?? string.Empty), new SqlParameter("@userId", userId) });
            });
        }

        public RMSPSampleTreeNode GetTeamsBySearch(RMSPSampleTreeNode node, bool checkPermission, string searchKey, bool includeOrphenNode = false)
        {
            var parentId = node.Id;
            var isExactlySearch = (node.SourceType == (int)SourceFlag.Teams || node.SourceType == (int)SourceFlag.SharePoint) 
                && !string.IsNullOrEmpty(searchKey) && searchKey.StartsWith('"') && searchKey.EndsWith('"');
            var searchKeyTrimmed = isExactlySearch ? searchKey.Trim('"') : searchKey;
            if (!checkPermission)
            {
                using var context = GetNewContext();
                if (!string.IsNullOrEmpty(searchKey))
                {
                    return GetChildrenNodesPaged(node, context, m => m.ParentId == parentId
                    && ((isExactlySearch && m.Name.Equals(searchKeyTrimmed)) || m.Name.Contains(searchKey))
                    && (includeOrphenNode || m.Name != null || m.NodeLevel != (int)NodeLevel.SkyDrivePro)
                    && (node.SourceType != (int)SourceFlag.Teams
                        || m.SiteCollectionType == (int)SiteCollectionType.Teams
                        || m.SiteCollectionType == (int)SiteCollectionType.Group));
                }

                return GetChildrenNodesPaged(node, context, m => m.ParentId == parentId
                    && (includeOrphenNode || m.Name != null || m.NodeLevel != (int)NodeLevel.SkyDrivePro)
                    && (node.SourceType != (int)SourceFlag.Teams
                        || m.SiteCollectionType == (int)SiteCollectionType.Teams
                        || m.SiteCollectionType == (int)SiteCollectionType.Group));
            }

            string userId = TenantLocalValue.LogonUserId;

            return GetAuthorizedChildrenNodesPagedForSearch(node, "ORDER BY Url ASC", context =>
            {
                string schema = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                string queryAllSql =
$@"SELECT * FROM [{schema}].RMRemoteNodes WHERE EXISTS (
  SELECT ScopeId FROM [{schema}].RMScopeRoleAssignments AS p 
    JOIN [{schema}].RMSecurityGroupMemberships m ON p.GroupId=m.GroupId 
	  AND m.UserId IN (
        SELECT UserId FROM [{schema}].RMAccounts WHERE IsRemoved=0 AND (
          UserId= @userId OR UserId IN (
            SELECT GroupId FROM [{schema}].RMLnkUserGroups WHERE UserId= @userId
          )
	    )
      )
    WHERE p.ScopeId=@ParendId
) AND ParentId=@ParendId ";
                if (isExactlySearch)
                {
                    queryAllSql += " AND [Name] = @SearchKey ";
                }
                else
                {
                    queryAllSql += " AND [Name] like '%' + @SearchKey + '%' ";
                }
                if (!includeOrphenNode)
                {
                    queryAllSql += " And (Name is not null or NodeLevel != 6000) ";
                }
                if (node.SourceType == (int)SourceFlag.Teams)
                {
                    queryAllSql += " And (SiteCollectionType = 2 or SiteCollectionType = 4) ";
                }
                return Tuple.Create(queryAllSql, new SqlParameter[] { new SqlParameter("@ParendId", parentId), new SqlParameter("@SearchKey", searchKeyTrimmed ?? string.Empty), new SqlParameter("@userId", userId) });
            });
        }

        public List<NodeCollection> GetNodeCollectionByUrls(List<string> urls)
        {
            ThrowUtil.ThrowIfNull(urls, "urls");
            List<NodeCollection> siteCollections = new List<NodeCollection>();
            DatabaseUtility.BatchOperation(urls, batchUrls =>
            {
                ExecuteWithRetry(context =>
                {
                    var items = context.RMRemoteNodes
                       .Where(r => batchUrls.Contains(r.Url))
                       .Select(r => new NodeCollection()
                       {
                           NodeId = r.Id,
                           Scope = r.Url
                       }).ToList();
                    siteCollections.AddRange(items);
                });
            });
            return siteCollections;
        }

        public Dictionary<string, List<NodeCollection>> GetSiteCollectionByParentIds(List<string> parentIds)
        {
            ThrowUtil.ThrowIfNull(parentIds, nameof(parentIds));
            Dictionary<string, List<NodeCollection>> dic = new Dictionary<string, List<NodeCollection>>();
            List<NodeCollection> tempList = null;
            DatabaseUtility.BatchOperation(parentIds, batchIds =>
            {
                ExecuteWithRetry(context =>
                {
                    context.Database.CommandTimeout = 600;
                    var items = context.RMRemoteNodes
                       .Where(r => batchIds.Contains(r.ParentId))
                       .Select(r => new
                       {
                           NodeId = r.Id,
                           Scope = r.Url,
                           ParentId = r.ParentId
                       });
                    foreach (var item in items)
                    {
                        var node = new NodeCollection() { NodeId = item.NodeId, Scope = item.Scope };
                        if (dic.TryGetValue(item.ParentId, out tempList))
                        {
                            tempList.Add(node);
                        }
                        else
                        {
                            dic.Add(item.ParentId, new List<NodeCollection>() { node });
                        }
                    }
                });
            });
            return dic;
        }

        public Dictionary<string, string> GetTeamId2TeamNameDicByTeamIds(List<string> teamIds)
        {
            var result = new Dictionary<string, string>();
            DatabaseUtility.BatchOperation(teamIds, batchIds =>
            {
                ExecuteWithRetry(context =>
                {
                    context.Database.CommandTimeout = 600;
                    var items = context.RMRemoteNodes
                       .Where(r => r.NodeLevel != 6060 && batchIds.Contains(r.TeamId))
                       .Select(r => new
                       {
                           r.TeamId,
                           r.Name
                       });
                    foreach (var item in items)
                    {
                        result[item.TeamId] = item.Name;
                    }
                });
            });
            return result;
        }

        public RemoteNodePara GetGroupByNameAndNodeLevel(string name, int nodeLevel)
        {
            return ExecuteWithRetry(context =>
            {
                var node = context.RMRemoteNodes
                    .Where(m => m.NodeLevel == nodeLevel && m.Url == name)
                    .Select(m => new
                    {
                        NodeId = m.Id,
                        NodeName = m.Url,
                        NodeLevel = m.NodeLevel
                    }).FirstOrDefault();
                return node == null ? null : new RemoteNodePara()
                {
                    NodeId = node.NodeId,
                    NodeName = node.NodeName,
                    NodeType = ConvertNodeLevelToType(node.NodeLevel)
                };
            });
        }

        public RemoteNodePara GetGroupByAosIdAndNodeLevel(string aosId, int nodeLevel)
        {
            return ExecuteWithRetry(context =>
            {
                var node = context.RMRemoteNodes
                    .Where(m => m.NodeLevel == nodeLevel && m.AosId == aosId)
                    .Select(m => new
                    {
                        NodeId = m.Id,
                        NodeName = m.Url,
                        NodeLevel = m.NodeLevel,
                        AosId = m.AosId
                    }).FirstOrDefault();
                return node == null ? null : new RemoteNodePara()
                {
                    NodeId = node.NodeId,
                    NodeName = node.NodeName,
                    NodeType = ConvertNodeLevelToType(node.NodeLevel),
                    AosId = node.AosId
                };
            });
        }

        #region Private Channel

        public List<SyncRemoteNodePara> GetAllPrivateChannelByPage(int pageIndex, int pageSize)
        {
            return ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 600;
                var sqlParams = new SqlParameter[]
                {
                    new SqlParameter("PrivateChannelLevel", NodeLevel_PrivateChannel),
                    new SqlParameter("SharedChannelLevel", NodeLevel_SharedChannel),
                };
                var sql =
$@"Select Url AS NodeName, ParentId, AuthType, AppType, ServiceAccountId, ScanSource, TenantId, TeamId, NodeLevel, SecondParentId, ObjectId 
From {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} Where NodeLevel in (@PrivateChannelLevel, @SharedChannelLevel)
ORDER BY CreateTime
OFFSET {pageIndex * pageSize} ROW FETCH NEXT {pageSize} ROWS ONLY
";
                return context.Database.SqlQuery<SyncRemoteNodePara>(sql, sqlParams).ToList();
            });
        }

        public List<RMRemoteNode> GetAllPrivateChannelNodesByPage(int pageIndex, int pageSize)
        {
            return ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 600;
                var sqlParams = new SqlParameter[]
                {
                    new SqlParameter("PrivateChannelLevel", NodeLevel_PrivateChannel),
                    new SqlParameter("SharedChannelLevel", NodeLevel_SharedChannel),
                };
                var sql =
$@"Select Id, Url, ParentId, TenantId, TeamId, NodeLevel, ObjectId 
From {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} Where NodeLevel in (@PrivateChannelLevel, @SharedChannelLevel)
ORDER BY CreateTime
OFFSET {pageIndex * pageSize} ROW FETCH NEXT {pageSize} ROWS ONLY
";
                return context.Database.SqlQuery<RMRemoteNode>(sql, sqlParams).ToList();
            });
        }

        public List<SyncRemoteNodePara> GetAllPrivateChannel()
        {
            return ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 600;
                var sqlParams = new SqlParameter[]
                {
                    new SqlParameter("PrivateChannelLevel", NodeLevel_PrivateChannel),
                    new SqlParameter("SharedChannelLevel", NodeLevel_SharedChannel),
                };
                var sql =
$@"Select Url AS NodeName, ParentId, AuthType, AppType, ServiceAccountId, ScanSource, TenantId, TeamId, NodeLevel, SecondParentId, ObjectId 
From {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} Where NodeLevel in (@PrivateChannelLevel, @SharedChannelLevel)";
                return context.Database.SqlQuery<SyncRemoteNodePara>(sql, sqlParams).ToList();
            });
        }

        public bool IsPrivateChannelGroupExist()
        {
            return ExecuteWithRetry(context =>
            {
                var sqlParams = new SqlParameter[]
                {
                    new SqlParameter("PrivateChannelLevel", NodeLevel_PrivateChannelSitesGroup),
                };
                var sql = $"Select Count(Id) From {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} Where NodeLevel = @PrivateChannelLevel";
                var count = context.Database.SqlQuery<int>(sql, sqlParams).FirstOrDefault();
                return count > 0;
            });
        }

        public List<string> GetPrivateChannelByGroupTeamSiteContainerIds(List<string> groupTeamSiteContainerIds)
        {
            List<string> results = new List<string>();
            DatabaseUtility.BatchOperation(groupTeamSiteContainerIds, batchIds =>
            {
                ExecuteWithRetry(context =>
                {
                    context.Database.CommandTimeout = 600;
                    List<SqlParameter> parameters = null;
                    var sql =
$@"Select url from {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} 
  where NodeLevel in (@PrivateChannelLevel,@SharedChannelLevel) and teamId in 
    (select teamId from {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} 
      where parentId in {DatabaseUtility.BuildInClause(groupTeamSiteContainerIds, out parameters)})";
                    parameters.Add(new SqlParameter("PrivateChannelLevel", NodeLevel_PrivateChannel));
                    parameters.Add(new SqlParameter("SharedChannelLevel", NodeLevel_SharedChannel));
                    results.AddRange(context.Database.SqlQuery<string>(sql, parameters.ToArray()).ToList());
                });
            });
            return results;
        }
        #endregion

        public HashSet<string> GetO365GroupSiteByUrls(List<string> urls)
        {
            var result = new HashSet<string>();
            DatabaseUtility.BatchOperation(urls, batchUrls =>
            {
                batchUrls = DatabaseUtility.EscapeSqlParam(batchUrls);
                ExecuteWithRetry(context =>
                {
                    var items = context.RMRemoteNodes
                        .Where(r => batchUrls.Contains(r.Url) && r.NodeLevel == (int)NodeLevel.O365GroupSites)
                        .Select(r => r.Url)
                        .ToList();

                    items.ForEach(item => result.Add(item));
                });
            });
            return result;
        }

        public Dictionary<string, string> GetAllSiteAndSPGroupMapping()
        {
            var siteAndMailboxMapping = new Dictionary<string, string>();

            var channelAndTeamsMapping = new Dictionary<string, string>();
            var teamsAndMailboxMapping = new Dictionary<string, string>();
            ExecuteWithRetry(context =>
            {
                var items = context.RMRemoteNodes
                    .Where(r => r.NodeLevel == (int)NodeLevel.O365GroupSites || r.NodeLevel == (int)NodeLevel.PrivateChannel || r.NodeLevel == (int)NodeLevel.SharedChannel)
                    .Select(r => new { r.Url, r.Name, r.NodeLevel, r.TeamId } )
                    .ToList();

                items.ForEach(s => {
                    siteAndMailboxMapping[s.Url] = s.Name;
                    if (s.NodeLevel == (int)NodeLevel.O365GroupSites)
                    {
                        siteAndMailboxMapping[s.Url] = s.Name;
                        teamsAndMailboxMapping[s.TeamId] = s.Name;
                    }
                    else
                    {
                        channelAndTeamsMapping[s.Url] = s.TeamId;
                    }
                });
            });

            foreach (var item in channelAndTeamsMapping)
            {
                if (teamsAndMailboxMapping.TryGetValue(item.Value, out var mailboxUrl))
                {
                    siteAndMailboxMapping[item.Key] = mailboxUrl;
                }
            }

            return siteAndMailboxMapping;
        }

        public Dictionary<string, List<string>> GetO365GroupSiteName2UrlDicByNames(List<string> names)
        {
            ThrowUtil.ThrowIfNull(names, nameof(names));
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            List<string> tempList = null;
            DatabaseUtility.BatchOperation(names, batchNames =>
            {
                batchNames = DatabaseUtility.EscapeSqlParam(batchNames);
                ExecuteWithRetry(context =>
                {
                    var items = context.RMRemoteNodes
                        .Where(r => batchNames.Contains(r.Name))
                        .Select(r => new { r.Name, r.Url })
                        .ToList();
                    foreach (var item in items)
                    {
                        if (result.TryGetValue(item.Name, out tempList))
                        {
                            tempList.Add(item.Name);
                        }
                        else
                        {
                            result.Add(item.Name, new List<string>() { item.Name });
                        }
                    }
                });
            });
            return result;
        }

        public void UpdateO365GroupSiteByUrls(List<RemoteSiteCollection> o365GroupSiteCollections)
        {
            try
            {
                var urlAndNodeMap = new Dictionary<string, RemoteSiteCollection>();
                o365GroupSiteCollections.ForEach(m =>
                {
                    if (!string.IsNullOrEmpty(m.url))
                    {
                        urlAndNodeMap[m.url.ToLower()] = m;
                    }
                });

                RemoteSiteCollection tempItem = null;
                DatabaseUtility.BatchOperation<string>(urlAndNodeMap.Keys, (batchUrls) =>
                {
                    ExecuteWithRetry(context =>
                    {
                        foreach (var node in context.RMRemoteNodes.Where(m => batchUrls.Contains(m.Url)))
                        {
                            tempItem = urlAndNodeMap[node.Url.ToLower()];
                            node.Url = tempItem.url;
                            node.ParentId = tempItem.parentId;
                            node.NodeLevel = (int)ConvertRemoteNodeTypeToNodeLevel(tempItem);
                            node.Name = tempItem.Name;
                            node.AppType = (int)tempItem.AppType;
                            node.AuthType = (int)tempItem.AuthType;
                            node.ServiceAccountId = tempItem.ServiceAccountId;
                            node.ScanSource = (int)tempItem.ScanSource;
                            node.TenantId = tempItem.TenantId;
                            node.ModifiedDate = DateTime.UtcNow.Ticks;
                            node.TeamId = tempItem.TeamId;
                        }
                        context.SaveChanges();
                    });
                });
            }
            catch (Exception e)
            {
                logger.Error("Update o365 group site by names failed. exception is " + e.ToString());
                throw;
            }
        }

        public void UpdateSiteCollectionSecondParentId(List<SyncRemoteNodePara> siteCollections)
        {
            ExecuteWithRetry(context =>
            {
                string sql = $"Update {SecurityUtils.SanitizeSQLSchemaName(GetFullTableName(context))} set SecondParentId=@SecondParentId,ModifiedDate=@ModifiedDate Where Url=@Url";
                foreach (var sc in siteCollections)
                {
                    var paras = new SqlParameter[]
                    {
                        new SqlParameter("Url", sc.NodeName),
                        new SqlParameter("SecondParentId", sc.SecondParentId),
                        new SqlParameter("ModifiedDate", DateTime.UtcNow.Ticks)
                    };
                    context.Database.ExecuteSqlCommand(sql, paras);
                }
            });
        }

        public RemoteSiteCollection GetRemoteSiteCollectionById(string id)
        {
            return ExecuteWithRetry(context =>
            {
                var node = context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(m => m.Id == id && (m.NodeLevel == NodeLevel_SiteCollection
                    || m.NodeLevel == NodeLevel_SkyDrivePro
                    || m.NodeLevel == NodeLevel_O365GroupSites
                    || m.NodeLevel == NodeLevel_PrivateChannel
                    || m.NodeLevel == NodeLevel_SharedChannel
                    ))
                    .FirstOrDefault();
                return ConvertToSiteCollection(node);
            });
        }

        public (RemoteSiteCollection, List<RemoteSiteCollection>) GetTeamsGroupAndChannelsCollectionByTeamsId(string teamsId, bool needChannel = false)
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes
                    .Where(m => m.TeamId == teamsId
                        && (needChannel || (m.NodeLevel == NodeLevel_O365GroupSites
                        && (m.SiteCollectionType == (int)SiteCollectionType.Teams || m.SiteCollectionType == (int)SiteCollectionType.Group))))
                    .Select(ConvertToSiteCollection)
                    .ToList();

                var teamsGroup = nodes.FirstOrDefault(r => r.NodeType == RemoveNodeType.O365GroupSites
                    && (r.SiteCollectionType == SiteCollectionType.Teams || r.SiteCollectionType == SiteCollectionType.Group));

                return (teamsGroup, teamsGroup != null ? nodes.Where(r => !r.id.Equals(teamsGroup.id)).ToList() : nodes);
            });
        }

        public List<RemoteSiteCollection> GetTeamsGroupAndChannelsCollectionByListTeamsId(IEnumerable<string> teamsId)
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes
                    .Where(m => teamsId.Contains(m.TeamId)
                                && (m.NodeLevel == NodeLevel_O365GroupSites
                                    && (m.SiteCollectionType == (int)SiteCollectionType.Teams || m.SiteCollectionType == (int)SiteCollectionType.Group))).AsEnumerable()
                    .Select(ConvertToSiteCollection)
                    .ToList();

                var teamsGroup = nodes.Where(r => r.NodeType == RemoveNodeType.O365GroupSites
                                                           && (r.SiteCollectionType == SiteCollectionType.Teams || r.SiteCollectionType == SiteCollectionType.Group)).ToList();

                return teamsGroup;
            });
        }

        public RemoteSiteCollection GetRemoteSiteCollectionByObjectId(string objectId)
        {
            return ExecuteWithRetry(context =>
            {
                var node = context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(m => m.ObjectId == objectId && (m.NodeLevel == NodeLevel_SiteCollection
                    || m.NodeLevel == NodeLevel_SkyDrivePro
                    || m.NodeLevel == NodeLevel_O365GroupSites
                    || m.NodeLevel == NodeLevel_PrivateChannel
                    || m.NodeLevel == NodeLevel_SharedChannel
                    ))
                    .FirstOrDefault();
                return ConvertToSiteCollection(node);
            });
        }

        public bool CheckIsOrphanedOD(string scId)
        {
            using (var context = GetNewContext())
            {
                return context.RMRemoteNodes.Count(node => node.ObjectId == scId && node.Name == null && node.NodeLevel == (int)NodeLevel.SkyDrivePro) > 0;
            }
        }

        public string GetUrlById(string id)
        {
            return ExecuteWithRetry(context =>
            {
                var node = context.RMRemoteNodes.AsNoTracking().Where(m => m.Id == id).FirstOrDefault();
                return node?.Url ?? string.Empty;
            });
        }

        
        public List<RemoteSiteCollection> GetRemoteSiteCollectionByIds(List<string> ids)
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(m => ids.Contains(m.Id) && (m.NodeLevel == NodeLevel_SiteCollection
                    || m.NodeLevel == NodeLevel_SkyDrivePro
                    || m.NodeLevel == NodeLevel_O365GroupSites
                    || m.NodeLevel == NodeLevel_PrivateChannel
                    || m.NodeLevel == NodeLevel_SharedChannel
                    )).ToList();
                return nodes.ConvertAll(ConvertToSiteCollection);
            });
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionByObjectIds(List<string> ids)
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(m => ids.Contains(m.ObjectId) && (m.NodeLevel == NodeLevel_SiteCollection
                    || m.NodeLevel == NodeLevel_SkyDrivePro
                    || m.NodeLevel == NodeLevel_O365GroupSites
                    || m.NodeLevel == NodeLevel_PrivateChannel
                    || m.NodeLevel == NodeLevel_SharedChannel
                    )).ToList();
                return nodes.ConvertAll(ConvertToSiteCollection);
            });
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionsByUrls(IEnumerable<string> urls)
        {
            var urlList = urls.ToList();
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(m => urlList.Contains(m.Url)
                        && (m.NodeLevel == NodeLevel_SiteCollection
                        || m.NodeLevel == NodeLevel_SkyDrivePro
                        || m.NodeLevel == NodeLevel_O365GroupSites
                        || m.NodeLevel == NodeLevel_PrivateChannel
                        || m.NodeLevel == NodeLevel_SharedChannel))
                    .ToList();
                return nodes.ConvertAll(ConvertToSiteCollection);
            });
        }
        public List<RemoteSiteCollection> GetRemoteSiteCollectionsByNodeLevel(int type)
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(QuerySiteCollections((RMBrowseTreeNodeSourceType) type))
                    .ToList();
                return nodes.ConvertAll(ConvertToSiteCollection);
            });
        }
        public RemoteSiteCollection GetRemoteSiteCollectionByUrl(string url)
        {
            return ExecuteWithRetry(context =>
            {
                var node = context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(m => m.Url == url
                        && (m.NodeLevel == NodeLevel_SiteCollection
                        || m.NodeLevel == NodeLevel_SkyDrivePro
                        || m.NodeLevel == NodeLevel_O365GroupSites
                        || m.NodeLevel == NodeLevel_PrivateChannel
                        || m.NodeLevel == NodeLevel_SharedChannel))
                    .FirstOrDefault();
                return ConvertToSiteCollection(node);
            });
        }

        public RemoteSiteCollection GetRemoteSiteCollectionByExactUrl(string url)
        {
            ThrowUtil.ThrowIfNullOrEmpty(url, nameof(url));

            return ExecuteWithRetry(context =>
            {
                var node = context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(m => m.Url == url
                        && (m.NodeLevel == NodeLevel_SiteCollection
                        || m.NodeLevel == NodeLevel_SkyDrivePro
                        || m.NodeLevel == NodeLevel_O365GroupSites
                        || m.NodeLevel == NodeLevel_PrivateChannel
                        || m.NodeLevel == NodeLevel_SharedChannel))
                    .FirstOrDefault();
                return ConvertToSiteCollection(node);
            });
        }

        public RemoteSiteCollection GetRemoteSiteCollectionByHostUrl(string url)
        {
            return ExecuteWithRetry(context =>
            {
                var node = context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(m => m.Url.StartsWith(url)
                        && (m.NodeLevel == NodeLevel_SiteCollection
                        || m.NodeLevel == NodeLevel_SkyDrivePro
                        || m.NodeLevel == NodeLevel_O365GroupSites
                        || m.NodeLevel == NodeLevel_PrivateChannel
                        || m.NodeLevel == NodeLevel_SharedChannel))
                    .FirstOrDefault();
                return ConvertToSiteCollection(node);
            });
        }

        public RemoteSiteCollection GetRemoteSiteCollectionByListUrl(string listUrl)
        {
            Func<RMDbContext, Tuple<string, SqlParameter[]>> getSCQuery = context =>
            {
                string query = $"SELECT * FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMRemoteNodes where((CAST(CHARINDEX(Url, @listUrl) AS int)) = 1) order by LEN(Url) desc";
                SqlParameter[] parameters = new SqlParameter[1] { new SqlParameter("@listUrl", listUrl) };
                return new Tuple<string, SqlParameter[]>(query, parameters);
            };
            var node = ExecuteWithRetry(context =>
            {
                var rn = context.Database.SqlQuery<RMRemoteNode>(getSCQuery(context).Item1, getSCQuery(context).Item2).FirstOrDefault();
                return rn;
            });
            return ConvertToSiteCollection(node);
        }

        public List<RemoteSiteCollection> GetAllRemoteSiteCollections()
        {
            return GetAllRemoteSiteCollectionsAsync().GetAwaiter().GetResult();
        }

        public List<RemoteSiteCollection> GetAllRemoteSiteCollections(int pageIndex, int pageSize, out int totalCount)
        {
            return GetAllRemoteSiteCollections(pageIndex, pageSize, null, out totalCount);
        }

        public List<RemoteSiteCollection> GetAllRemoteSiteCollections(int pageIndex, int pageSize, string key, out int totalCount)
        {
            if (pageIndex < 1 || pageSize < 1)
            {
                throw new ArgumentException("Invalid pagination parameters.");
            }

            var result = ExecuteWithRetry(context =>
            {
                var query = context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(m =>
                        m.NodeLevel == NodeLevel_SiteCollection
                        || m.NodeLevel == NodeLevel_SkyDrivePro
                        || m.NodeLevel == NodeLevel_O365GroupSites
                        || m.NodeLevel == NodeLevel_PrivateChannel
                        || m.NodeLevel == NodeLevel_SharedChannel);

                if (!string.IsNullOrWhiteSpace(key))
                {
                    var normalizedKey = key.Trim();
                    query = query.Where(m => m.Url.Contains(normalizedKey) || m.Name.Contains(normalizedKey));
                }

                var queryTotalCount = query.Count();
                var skip = (pageIndex - 1) * pageSize;
                var pageNodes = query
                    .OrderBy(m => m.Url)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                return (TotalCount: queryTotalCount, Items: pageNodes.Select(ConvertToSiteCollection).ToList());
            });

            totalCount = result.TotalCount;
            return result.Items;
        }

        public List<RemoteSiteCollection> GetMappedRemoteSitesPaged(int pageIndex, int pageSize, string keyword, List<string> selectedNodeIds, out int totalCount)
        {
            if (pageIndex < 1 || pageSize < 1) throw new ArgumentException("Invalid pagination parameters.");

            int tempTotalCount = 0;

            var resultItems = ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 900;
                string schema = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);

                var sqlParams = new List<SqlParameter>();
                string whereClause = "(r.NodeLevel IN (100, 6000, 6010, 6020, 6060, 6061))";

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    whereClause += " AND r.[Url] LIKE '%' + @Keyword + '%'";
                    sqlParams.Add(new SqlParameter("@Keyword", keyword.Trim()));
                }

                string countSql = $"SELECT COUNT(1) FROM [{schema}].[RMRemoteNodes] r WITH (NOLOCK) WHERE {whereClause}";
                tempTotalCount = context.Database.SqlQuery<int>(countSql, sqlParams.Select(p => ((ICloneable)p).Clone()).ToArray()).FirstOrDefault();

                if (tempTotalCount == 0) return new List<RemoteSiteCollection>();

                bool hasSelectedNodes = selectedNodeIds != null && selectedNodeIds.Any();
                string joinClause = string.Empty;
                string priorityOrder = string.Empty;

                if (hasSelectedNodes)
                {
                    sqlParams.Add(new SqlParameter("@SelectedIds", string.Join(",", selectedNodeIds)));
                    joinClause = "LEFT JOIN (SELECT [value] FROM STRING_SPLIT(@SelectedIds, ',') WHERE [value] <> '') s ON r.Id = s.[value]";
                    priorityOrder = "CASE WHEN s.[value] IS NOT NULL THEN 0 ELSE 1 END ASC,";
                }

                sqlParams.Add(new SqlParameter("@Skip", (pageIndex - 1) * pageSize));
                sqlParams.Add(new SqlParameter("@Take", pageSize));

                string querySql = $@"
                    SELECT r.* 
                    FROM [{schema}].[RMRemoteNodes] r WITH (NOLOCK)
                    {joinClause}
                    WHERE {whereClause}
                    ORDER BY {priorityOrder} r.[Url] ASC, r.[Id] ASC
                    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

                var siteCollections = context.Database.SqlQuery<RMRemoteNode>(querySql, sqlParams.ToArray())
                                             .Select(ConvertToSiteCollection)
                                             .ToList();

                if (hasSelectedNodes)
                {
                    var selectedSet = new HashSet<string>(selectedNodeIds, StringComparer.OrdinalIgnoreCase);
                    siteCollections.ForEach(site => site.isPlanProfileSelected = selectedSet.Contains(site.id));
                }

                return siteCollections;
            });

            totalCount = tempTotalCount;
            return resultItems;
        }

        private async Task<List<RemoteSiteCollection>> GetAllRemoteSiteCollectionsAsync()
        {
            var allSites = new List<RemoteSiteCollection>();
            await foreach (var batch in GetAllRemoteNodesAsync())
            {
                var siteCollections = batch
                    .Where(m =>
                        m.NodeLevel == NodeLevel_SiteCollection
                        || m.NodeLevel == NodeLevel_SkyDrivePro
                        || m.NodeLevel == NodeLevel_O365GroupSites
                        || m.NodeLevel == NodeLevel_PrivateChannel
                        || m.NodeLevel == NodeLevel_SharedChannel
                    );
                allSites.AddRange(siteCollections.ConvertAll(siteCollection => ConvertToSiteCollection(siteCollection)));
            }
            return allSites;
        }



        public bool IsRemoteSiteExist()
        {
            return ExecuteWithRetry(context => {
                context.Database.CommandTimeout = 900;
                return context.RMRemoteNodes.Any((m =>
                        m.NodeLevel == NodeLevel_SiteCollection
                        || m.NodeLevel == NodeLevel_SkyDrivePro
                        || m.NodeLevel == NodeLevel_O365GroupSites
                        || m.NodeLevel == NodeLevel_PrivateChannel
                        || m.NodeLevel == NodeLevel_SharedChannel
                    ));
            });
        }

        public RemoteWebApplication GetWebApplicationById(string id)
        {
            return ExecuteWithRetry(context =>
            {
                var node = context.RMRemoteNodes
                    .Where(m => m.Id == id && (m.NodeLevel == NodeLevel_WebApplication || m.NodeLevel == NodeLevel_SkyDriveProGroup || m.NodeLevel == NodeLevel_O365GroupSitesGroup || m.NodeLevel == NodeLevel_PrivateChannelSitesGroup))
                    .FirstOrDefault();
                return ConvertToWebApplication(node);
            });
        }

        public List<RemoteWebApplication> GetWebApplicationByIds(List<string> ids)
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes
                    .Where(m => ids.Contains(m.Id) && (m.NodeLevel == NodeLevel_WebApplication || m.NodeLevel == NodeLevel_SkyDriveProGroup || m.NodeLevel == NodeLevel_O365GroupSitesGroup || m.NodeLevel == NodeLevel_PrivateChannelSitesGroup))
                    .ToList();
                return nodes.ConvertAll(ConvertToWebApplication);
            });
        }


        public string GetContainerIdByName(string containerName, int nodeLevel)
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes
                    .Where(m => m.Url == containerName && m.NodeLevel == nodeLevel)
                    .Select(n => n.Id)
                    .FirstOrDefault();
            });
        }

        private Expression<Func<RMRemoteNode, bool>> QueryWebApplications(RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.All)
        {
            switch (type)
            {
                case RMBrowseTreeNodeSourceType.SharepointOnline:
                    if(RMKeyValueDao.HasUpgradeTeams())
                    {
                        return r => r.NodeLevel == NodeLevel_WebApplication;
                    }
                    return r =>
                        r.NodeLevel == NodeLevel_WebApplication ||
                        r.NodeLevel == NodeLevel_O365GroupSitesGroup ||
                        r.NodeLevel == NodeLevel_PrivateChannelSitesGroup;
                case RMBrowseTreeNodeSourceType.SkyDrivePro:
                    return r => r.NodeLevel == NodeLevel_SkyDriveProGroup;
                case RMBrowseTreeNodeSourceType.Teams:
                    return r => r.NodeLevel == NodeLevel_O365GroupSitesGroup;
                case RMBrowseTreeNodeSourceType.SPAndOD:
                    if(RMKeyValueDao.HasUpgradeTeams())
                    {
                        return r =>
                            r.NodeLevel == NodeLevel_WebApplication ||
                            r.NodeLevel == NodeLevel_SkyDriveProGroup;
                    }
                    return r =>
                        r.NodeLevel == NodeLevel_WebApplication ||
                        r.NodeLevel == NodeLevel_O365GroupSitesGroup ||
                        r.NodeLevel == NodeLevel_PrivateChannelSitesGroup ||
                        r.NodeLevel == NodeLevel_SkyDriveProGroup;
                case RMBrowseTreeNodeSourceType.All:
                default:
                    return r =>
                        r.NodeLevel == NodeLevel_WebApplication ||
                        r.NodeLevel == NodeLevel_SkyDriveProGroup ||
                        r.NodeLevel == NodeLevel_O365GroupSitesGroup ||
                        r.NodeLevel == NodeLevel_PrivateChannelSitesGroup;
            }
        }
        private Expression<Func<RMRemoteNode, bool>> QuerySiteCollections(RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.All)
        {
            switch (type)
            {
                case RMBrowseTreeNodeSourceType.SharepointOnline:
                    if (RMKeyValueDao.HasUpgradeTeams())
                    {
                        return r => r.NodeLevel == NodeLevel_SiteCollection;
                    }
                    return r =>
                        r.NodeLevel == NodeLevel_SiteCollection ||
                        r.NodeLevel == NodeLevel_O365GroupSites ||
                        r.NodeLevel == NodeLevel_PrivateChannel ||
                        r.NodeLevel == NodeLevel_SharedChannel;
                case RMBrowseTreeNodeSourceType.SkyDrivePro:
                    return r => r.NodeLevel == NodeLevel_SkyDrivePro;
                case RMBrowseTreeNodeSourceType.Teams:
                    return r => 
                       r.NodeLevel == NodeLevel_O365GroupSites ||
                       r.NodeLevel == NodeLevel_PrivateChannel ||
                       r.NodeLevel == NodeLevel_SharedChannel;
                case RMBrowseTreeNodeSourceType.SPAndOD:
                    if (RMKeyValueDao.HasUpgradeTeams())
                    {
                        return r =>
                            r.NodeLevel == NodeLevel_SiteCollection ||
                            r.NodeLevel == NodeLevel_SkyDrivePro;
                    }
                    return r =>
                        r.NodeLevel == NodeLevel_SiteCollection ||
                        r.NodeLevel == NodeLevel_O365GroupSites ||
                        r.NodeLevel == NodeLevel_PrivateChannel ||
                        r.NodeLevel == NodeLevel_SharedChannel ||
                        r.NodeLevel == NodeLevel_SkyDrivePro;
                case RMBrowseTreeNodeSourceType.All:
                default:
                    return r =>
                        r.NodeLevel == NodeLevel_SiteCollection ||
                        r.NodeLevel == NodeLevel_SkyDrivePro ||
                        r.NodeLevel == NodeLevel_O365GroupSites ||
                        r.NodeLevel == NodeLevel_PrivateChannel ||
                        r.NodeLevel == NodeLevel_SharedChannel;
            }
        }
        public List<RemoteWebApplication> GetAllWebApplications(RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.All)
        {
            return ExecuteWithRetry(context =>
            {
                List<RMRemoteNode> result = context.RMRemoteNodes.Where(QueryWebApplications(type)).OrderBy(n => n.Url).ToList();
                return result.ConvertAll(node => ConvertToWebApplication(node));
            });
        }

        public RMSPSampleTreeNode Convert2TreeNode(RMRemoteNode node, RMDbContext context, int sourceFlag = 0)
        {
            var treeNode = new RMSPSampleTreeNode();
            treeNode.Id = node.Id;
            treeNode.SPObjectId = node.Id;
            treeNode.Name = node.Url;
            treeNode.DisplayName =  node.Url;
            treeNode.OrphanNameSuffix = (node.Name == null && node.NodeLevel == (int)NodeLevel.SkyDrivePro) ? "(" + I18NEntity.GetString("RM_JS_SPS_Orphaned_OneDrive") + ")" : null;
            treeNode.FullPath = node.Url;
            treeNode.Level = string.IsNullOrEmpty(node.ParentId) ? (int)NodeLevel.WebApplication : (int)NodeLevel.SiteCollection;
            treeNode.ChannelType = (int)Convert2ChannelType(node.NodeLevel);
            treeNode.NodeType = (int)ConvertSPNodeTypeByNodeLevel(node.NodeLevel);
            treeNode.SPType = (int)SPType.BPOS;
            treeNode.SourceType = sourceFlag;
            treeNode.TeamsId = node.TeamId;
            treeNode.O365TenantId = node.TenantId;
            if (node.NodeLevel == (int)NodeLevel.O365GroupSites && !string.IsNullOrEmpty(node.Name))
            {
                treeNode.TeamName = node.Name.Split('@').FirstOrDefault();
            }
            else if ((node.NodeLevel == (int)NodeLevel.PrivateChannel || node.NodeLevel == (int)NodeLevel.SharedChannel) && !string.IsNullOrEmpty(node.Name))
            {
                var teamId = node.TeamId;
                var groupSite = context.RMRemoteNodes.FirstOrDefault(item => item.ObjectId == teamId);
                if (groupSite == null)
                {
                    treeNode.TeamName = node.Name;
                }
                else
                {
                    treeNode.TeamName = groupSite.Name.Split('@').FirstOrDefault();
                }
            }
            if (sourceFlag == (int)SourceFlag.Teams)
            {
                treeNode.TeamsId = node.TeamId;
                treeNode.SourceType = (int)SourceFlag.Teams;
                if (node.SiteCollectionType == (int)SiteCollectionType.Teams || node.SiteCollectionType == (int)SiteCollectionType.Group)
                {
                    treeNode.Id = node.TeamId;
                    treeNode.SPObjectId = node.TeamId;
                    treeNode.Name = node.Name;
                    treeNode.FullPath = node.Name;
                    treeNode.TeamName = node.DisplayName;
                    //treeNode.NodeType = (int)NodeLevel.Office365GroupEntire;
                    treeNode.Level = (int)NodeLevel.Office365GroupEntire;
                    treeNode.NodeType = (SiteCollectionType)node.SiteCollectionType switch
                    {
                        SiteCollectionType.Teams => (int)TreeNodeType.O365TeamSites,
                        SiteCollectionType.Group => (int)TreeNodeType.O365GroupSites,
                        _ => treeNode.NodeType,
                    };
                }

            }
            int spVersion = 0;
            if (int.TryParse(node.SPVersion, out spVersion))
            {
                treeNode.SPVersion = spVersion;
            }
            return treeNode;
        }

        public RMSPTreeNode Convert2RMSPTreeNode(RMRemoteNode node, RMDbContext context, int sourceFlag = 0)
        {
            var treeNode = new RMSPTreeNode();
            treeNode.Id = node.Id;
            treeNode.SPObjectId = node.Id;
            treeNode.Name = node.Url;
            treeNode.DisplayName =  node.Url;
            treeNode.O365TenantId = node.TenantId;
            treeNode.ParentId = node.ParentId;
            treeNode.OrphanNameSuffix = (node.Name == null && node.NodeLevel == (int)NodeLevel.SkyDrivePro) ? "(" + I18NEntity.GetString("RM_JS_SPS_Orphaned_OneDrive") + ")" : null;
            treeNode.FullPath = node.Url;
            treeNode.Level = string.IsNullOrEmpty(node.ParentId) ? (int)NodeLevel.WebApplication : (int)NodeLevel.SiteCollection;
            //treeNode.ChannelType = (int)Convert2ChannelType(node.NodeLevel);
            treeNode.NodeType = (int)ConvertSPNodeTypeByNodeLevel(node.NodeLevel);
            treeNode.SPType = (int)SPType.BPOS;
            //treeNode.SourceType = sourceFlag;
            treeNode.TeamsId = node.TeamId;
            if (node.NodeLevel == (int)NodeLevel.O365GroupSites && !string.IsNullOrEmpty(node.Name))
            {
                treeNode.TeamName = node.Name.Split('@').FirstOrDefault();
            }
            else if ((node.NodeLevel == (int)NodeLevel.PrivateChannel || node.NodeLevel == (int)NodeLevel.SharedChannel) && !string.IsNullOrEmpty(node.Name))
            {
                var teamId = node.TeamId;
                var groupSite = context.RMRemoteNodes.FirstOrDefault(item => item.ObjectId == teamId);
                if (groupSite == null)
                {
                    treeNode.TeamName = node.Name;
                }
                else
                {
                    treeNode.TeamName = groupSite.Name.Split('@').FirstOrDefault();
                }
            }
            if (sourceFlag == (int)SourceFlag.Teams)
            {
                treeNode.TeamsId = node.TeamId;
                //treeNode.SourceType = (int)SourceFlag.Teams;
                if (node.SiteCollectionType == (int)SiteCollectionType.Teams || node.SiteCollectionType == (int)SiteCollectionType.Group)
                {
                    treeNode.Id = node.TeamId;
                    treeNode.SPObjectId = node.TeamId;
                    treeNode.Name = node.Name;
                    treeNode.FullPath = node.Name;
                    treeNode.TeamName = node.DisplayName;
                    treeNode.O365TenantId = node.TenantId;
                    //treeNode.NodeType = (int)NodeLevel.Office365GroupEntire;
                    treeNode.Level = (int)NodeLevel.Office365GroupEntire;
                    treeNode.NodeType = (SiteCollectionType)node.SiteCollectionType switch
                    {
                        SiteCollectionType.Teams => (int)TreeNodeType.O365TeamSites,
                        SiteCollectionType.Group => (int)TreeNodeType.O365GroupSites,
                        _ => treeNode.NodeType,
                    };
                }

            }
            int spVersion = 0;
            if (int.TryParse(node.SPVersion, out spVersion))
            {
                treeNode.SPVersion = spVersion;
            }
            return treeNode;
        }

        public RMSPSampleTreeNode GetWebApplications(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission)
        {
            if (!checkPermission)
            {
                return ExecuteWithRetry(
                    context => GetChildrenNodesPaged(node, context, QueryWebApplications(type)));
            }

            string userId = TenantLocalValue.LogonUserId;
            string dataSourceCondition = string.Empty;
            switch (type)
            {
                case RMBrowseTreeNodeSourceType.SkyDrivePro:
                    dataSourceCondition = $"DataSourceType={(int)SourceFlag.OneDrive}";
                    break;
                case RMBrowseTreeNodeSourceType.SharepointOnline:
                    dataSourceCondition = $"DataSourceType={(int)SourceFlag.SharePoint}";
                    break;
                case RMBrowseTreeNodeSourceType.Teams:
                    dataSourceCondition = $"DataSourceType={(int)SourceFlag.Teams}";
                    break;
                default:
                    dataSourceCondition = $"DataSourceType IN ({(int)SourceFlag.SharePoint},{(int)SourceFlag.OneDrive},{(int)SourceFlag.Teams})";
                    break;
            }

            if(RMKeyValueDao.HasUpgradeTeams() && type == RMBrowseTreeNodeSourceType.SharepointOnline)
            {
                return GetAuthorizedChildrenNodesPaged(node, "ORDER BY Url ASC", context =>
                {
                    string queryAllSql =
                        $@"SELECT * FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMRemoteNodes WHERE NodeLevel = 2 and Id IN ( 
                      SELECT ScopeId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMScopeRoleAssignments AS p 
                        JOIN [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMSecurityGroupMemberships m ON p.GroupId=m.GroupId 
	                      AND m.UserId IN (
                            SELECT UserId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMAccounts WHERE IsRemoved=0 AND (
                              UserId= @userId OR UserId IN (
                                SELECT GroupId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMLnkUserGroups WHERE UserId= @userId
                              )
	                        )
                          )
                        WHERE {dataSourceCondition}
                    )";
                    return new Tuple<string, SqlParameter[]>(queryAllSql, new SqlParameter[] { new SqlParameter("@userId", userId) });
                });
            }
            else
            {
                return GetAuthorizedChildrenNodesPaged(node, "ORDER BY Url ASC", context =>
                {
                    string queryAllSql =
                            $@"SELECT * FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMRemoteNodes WHERE Id IN ( 
                          SELECT ScopeId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMScopeRoleAssignments AS p 
                            JOIN [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMSecurityGroupMemberships m ON p.GroupId=m.GroupId 
	                          AND m.UserId IN (
                                SELECT UserId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMAccounts WHERE IsRemoved=0 AND (
                                  UserId= @userId OR UserId IN (
                                    SELECT GroupId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMLnkUserGroups WHERE UserId= @userId
                                  )
	                            )
                              )
                            WHERE {dataSourceCondition}
                        )";
                    return new Tuple<string, SqlParameter[]>(queryAllSql, new SqlParameter[] { new SqlParameter("@userId", userId) });
                });
            }
        }

        public async Task<RMSPSampleTreeNode> GetWebApplicationsForSearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission)
        {
            using var context = GetNewContext();
            context.Database.CommandTimeout = 900;
            var parentPermissionIds = new Dictionary<string, bool>();
            //var remoteNodes = new List<RMRemoteNode>();
            var searchKey = node.SearchKey;
            var isSupportContainerSearch = type == RMBrowseTreeNodeSourceType.SharepointOnline || type == RMBrowseTreeNodeSourceType.Teams;
            var defaultContainerI18NName =
                new Dictionary<string, string> // mapping of default container for SPO and Teams source
                {
                    { RMConstants.DEFAULT_SPSITES_GROUP, I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup") },
                    { RMConstants.DEFAULT_O365_SITES_GROUP, I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer") },
                    { RMConstants.DefaultPrivateChannelSitesGroup, I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer") },
                }
            .Where(r => r.Value.Contains(searchKey, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(StringComparer.OrdinalIgnoreCase);

            if (!checkPermission)
            {
                if (isSupportContainerSearch)
                {
                    parentPermissionIds = context.RMRemoteNodes
                        .Where(QueryWebApplications(type))
                        .Where(r => (r.Url.Contains(searchKey) && !_defaultContainerUrl.Contains(r.Url))
                            || defaultContainerI18NName.Keys.Contains(r.Url)
                            )
                        .Select(r => r.Id)
                        .Distinct()
                        .ToDictionary(r => r, v => true);
                }

                if (type == RMBrowseTreeNodeSourceType.Teams)
                {
                    context.RMRemoteNodes
                        .Where(r => r.Name.Contains(searchKey)
                            && !string.IsNullOrEmpty(r.ParentId)
                            && (r.SiteCollectionType == (int)SiteCollectionType.Teams
                                || r.SiteCollectionType == (int)SiteCollectionType.Group))
                        .Select(r => r.ParentId)
                        .Distinct().ForEach(r => parentPermissionIds.TryAdd(r, false));
                }
                else
                {
                    context.RMRemoteNodes
                        .Where(r => r.Url.Contains(searchKey)
                            && !string.IsNullOrEmpty(r.ParentId))
                        .Select(r => r.ParentId).Distinct().ForEach(r => parentPermissionIds.TryAdd(r, false));
                }
            }
            else
            {
                parentPermissionIds = (await GetPermissionContainerIdsAsync(type)).ToDictionary(r => r, v => false);
            }

            var remoteNodes = context.RMRemoteNodes.Where(r => parentPermissionIds.Keys.Contains(r.Id)).Where(QueryWebApplications(type)).ToList();

            if (checkPermission)
            {
                foreach (var webNode in remoteNodes)
                {
                    if ((webNode.Url.Contains(searchKey, StringComparison.OrdinalIgnoreCase) && !_defaultContainerUrl.Contains(webNode.Url)) 
                        || defaultContainerI18NName.ContainsKey(webNode.Url))
                    {
                        parentPermissionIds[webNode.Id] = true;
                    }
                }
            }

            node.Children = remoteNodes.OrderBy(n => n.Url).Skip(node.PageIndex * node.PageSize).Take(node.PageSize).ToList().ConvertAll(i => Convert2TreeNode(i, context, node.SourceType));
            node.Children.ForEach(r => r.Loaded = parentPermissionIds.TryGetValue(r.Id, out var isFoundContaner) && isFoundContaner);
            node.ChildrenCount = parentPermissionIds.Count;
            return node;
        }

        public async Task<RMSPSampleTreeNode> GetWebApplicationsOnlyForSearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission)
        {
            using var context = GetNewContext();
            context.Database.CommandTimeout = 900;
            var searchKey = node.SearchKey ?? string.Empty;
            var isSupportContainerSearch = type == RMBrowseTreeNodeSourceType.SharepointOnline || type == RMBrowseTreeNodeSourceType.Teams;
            var defaultContainerI18NName =
                new Dictionary<string, string>
                {
        { RMConstants.DEFAULT_SPSITES_GROUP, I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup") },
        { RMConstants.DEFAULT_O365_SITES_GROUP, I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer") },
        { RMConstants.DefaultPrivateChannelSitesGroup, I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer") },
                }
                .Where(r => r.Value.Contains(searchKey, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(StringComparer.OrdinalIgnoreCase);

            var remoteNodesQuery = context.RMRemoteNodes.Where(QueryWebApplications(type));

            if (checkPermission)
            {
                var permittedIds = await GetPermissionContainerIdsAsync(type);
                remoteNodesQuery = remoteNodesQuery.Where(r => permittedIds.Contains(r.Id));
            }
            else if (!isSupportContainerSearch)
            {
                node.Children = new List<RMSPSampleTreeNode>();
                node.ChildrenCount = 0;
                return node;
            }

            remoteNodesQuery = remoteNodesQuery.Where(r =>
                (r.Url.Contains(searchKey) && !_defaultContainerUrl.Contains(r.Url)) || defaultContainerI18NName.Keys.Contains(r.Url));

            var total = remoteNodesQuery.Count();

            var remoteNodes = remoteNodesQuery
                .OrderBy(n => n.Url)
                .Skip(node.PageIndex * node.PageSize)
                .Take(node.PageSize)
                .ToList();

            node.Children = remoteNodes.ConvertAll(i => Convert2TreeNode(i, context, node.SourceType));
            node.Children.ForEach(r => r.Loaded = false);
            node.ChildrenCount = total;
            return node;
        }

        public async Task<RMSPSampleTreeNode> GetWebApplicationsForExactlySearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission, bool includeOrphanNode)
        {
            using var context = GetNewContext();
            context.Database.CommandTimeout = 900;
            var searchKey = node.SearchKey;
            var searchKeyTrimmed = node.SearchKey.Trim('"');
            var searchKeyIsSiteUrl = searchKeyTrimmed.StartWithIgnoreCase("https://");

            new Dictionary<string, string> // mapping of default container for SPO and Teams source
                {
                    { I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup"), RMConstants.DEFAULT_SPSITES_GROUP },
                    { I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer"), RMConstants.DEFAULT_O365_SITES_GROUP },
                    { I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer") , RMConstants.DefaultPrivateChannelSitesGroup },
                }
                .ForEach(a => {
                    if (a.Key.Equals(searchKeyTrimmed, StringComparison.OrdinalIgnoreCase))
                    {
                        searchKeyTrimmed = a.Value;
                    }
                });

            node.Children = new List<RMSPSampleTreeNode>();
            node.ChildrenCount = 0;

            if (type == RMBrowseTreeNodeSourceType.SkyDrivePro && !searchKeyIsSiteUrl)
            {
                // OneDrive content source only support search by Site URL, if search key is not site url, return empty result directly to avoid unnecessary permission check and query
                return node;
            }
            if (type == RMBrowseTreeNodeSourceType.Teams && searchKeyIsSiteUrl)
            {
                // if searched node is not exist, return empty result directly to avoid unnecessary permission check and query
                return node;
            }

            RMSPSampleTreeNode searchedContainerTreeNode = null;
            RMRemoteNode searchedNode = null;
            if (type == RMBrowseTreeNodeSourceType.Teams)
            {
                searchedNode = await context.RMRemoteNodes.AsNoTracking().AsQueryable()
                    .FirstOrDefaultAsync(
                        r => searchKeyTrimmed.Equals(r.Name) && (r.SiteCollectionType == (int)SiteCollectionType.Teams || r.SiteCollectionType == (int)SiteCollectionType.Group)
                            || searchKeyTrimmed.Equals(r.Url) && r.NodeLevel == NodeLevel_O365GroupSitesGroup);
            }
            else
            {
                searchedNode = await context.RMRemoteNodes.AsNoTracking().AsQueryable().FirstOrDefaultAsync(r => r.Url.Equals(searchKeyTrimmed));
            }

            if (searchedNode == null)
            {
                // if searched node is not exist, return empty result directly to avoid unnecessary permission check and query
                return node;
            }

            var searchedTreeNode = Convert2TreeNode(searchedNode, context, node.SourceType);

            if (string.IsNullOrEmpty(searchedNode.ParentId))
            {
                searchedContainerTreeNode = searchedTreeNode;
            }
            else
            {
                if(!includeOrphanNode && searchedNode.Name == null && searchedNode.NodeLevel == (int)NodeLevel.SkyDrivePro)
                {
                    logger.Warn($"Skip orphan onedrive node");
                    return node;
                }

                var containerNode = await context.RMRemoteNodes.AsNoTracking().AsQueryable().FirstOrDefaultAsync(r => searchedNode.ParentId.Equals(r.Id));
                searchedContainerTreeNode = Convert2TreeNode(containerNode, context, node.SourceType);

                searchedContainerTreeNode.Children = new List<RMSPSampleTreeNode>() { searchedTreeNode };
                searchedContainerTreeNode.ChildrenCount = 1;
                searchedContainerTreeNode.Loaded = true;
                searchedContainerTreeNode.Expanded = true;
                searchedTreeNode.Parent = searchedContainerTreeNode;
            }

            if (checkPermission)
            {
                var parentPermissionIds = (await GetPermissionContainerIdsAsync(type)).ToDictionary(r => r, v => false);
                if (parentPermissionIds.Keys.Contains(searchedContainerTreeNode.Id))
                {
                    return node;
                }
            }

            node.Children.Add(searchedContainerTreeNode);
            node.ChildrenCount = 1;
            node.Loaded = true;
            node.Expanded = true;
            return node;
        }

        public async Task<RMSPSampleTreeNode> GetWebApplicationsOnlyForExactlySearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission, bool includeOrphanNode)
        {
            using var context = GetNewContext();
            context.Database.CommandTimeout = 900;
            var searchKeyTrimmed = node.SearchKey.Trim('"');

            new Dictionary<string, string> // mapping of default container for SPO and Teams source
                {
                    { I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup"), RMConstants.DEFAULT_SPSITES_GROUP },
                    { I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer"), RMConstants.DEFAULT_O365_SITES_GROUP },
                    { I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer") , RMConstants.DefaultPrivateChannelSitesGroup },
                }
                .ForEach(a =>
                {
                    if (a.Key.Equals(searchKeyTrimmed, StringComparison.OrdinalIgnoreCase))
                    {
                        searchKeyTrimmed = a.Value;
                    }
                });
            node.Children = new List<RMSPSampleTreeNode>();
            node.ChildrenCount = 0;
            RMRemoteNode searchedNode = null;
            if (type == RMBrowseTreeNodeSourceType.Teams)
            {
                searchedNode = await context.RMRemoteNodes.AsNoTracking().AsQueryable()
                    .FirstOrDefaultAsync(
                        r => searchKeyTrimmed.Equals(r.Name) && (r.SiteCollectionType == (int)SiteCollectionType.Teams || r.SiteCollectionType == (int)SiteCollectionType.Group)
                            || searchKeyTrimmed.Equals(r.Url) && r.NodeLevel == NodeLevel_O365GroupSitesGroup);
            }
            else
            {
                searchedNode = await context.RMRemoteNodes.AsNoTracking().AsQueryable().FirstOrDefaultAsync(r => r.Url.Equals(searchKeyTrimmed) && r.NodeLevel == NodeLevel_WebApplication);
            }
            if (searchedNode == null)
            {
                // if searched node is not exist, return empty result directly to avoid unnecessary permission check and query
                return node;
            }

            if (checkPermission)
            {
                var permissionContainerId = string.IsNullOrEmpty(searchedNode.ParentId) ? searchedNode.Id : searchedNode.ParentId;
                var parentPermissionIds = (await GetPermissionContainerIdsAsync(type)).ToDictionary(r => r, v => false);
                if (!parentPermissionIds.Keys.Contains(permissionContainerId))
                {
                    return node;
                }
            }

            var searchedTreeNode = Convert2TreeNode(searchedNode, context, node.SourceType);
            node.Children.Add(searchedTreeNode);
            node.ChildrenCount = 1;
            node.Loaded = false;
            node.Expanded = true;
            return node;
        }

        private async Task<List<string>> GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType type)
        {
            var containerIds = new List<string>();
            try
            {
                var userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                var allContainers = (await RMScopeRoleAssignmentDao.GetAllContainersByUsersAsync(userAndGroupUserIds)).Where(x => GetPermissionDataSrouceType(type).Contains(x.Key));
                foreach (KeyValuePair<int, List<Guid>> item in allContainers)
                {
                    item.Value.ForEach(o =>
                    {
                        if (!containerIds.Contains(o.ToString()))
                        {
                            containerIds.Add(o.ToString());
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
            if (RMBrowseTreeNodeSourceType.SkyDrivePro == type)
            {
                types.Add((int)SourceFlag.OneDrive);
            }
            if (RMBrowseTreeNodeSourceType.Teams == type)
            {
                types.Add((int)SourceFlag.Teams);
            }
            return types;
        }

        private void ResetPagerInfo(RMSPSampleTreeNode node, int childrenCount)
        {
            node.ChildrenCount = childrenCount;
            if (node.PageIndex * node.PageSize >= node.ChildrenCount)
            {
                node.PageIndex = (node.ChildrenCount - 1) / node.PageSize;
            }
        }

        private RMSPSampleTreeNode GetChildrenNodesPaged(RMSPSampleTreeNode node, RMDbContext context, Expression<Func<RMRemoteNode, bool>> expWhere)
        {
            try
            {
                context.Database.CommandTimeout = 900;
                var queryResults = context.RMRemoteNodes.Where(expWhere);
                int count = 0;
                logger.Info($"GetChildrenNodesPaged count: Get children node count exp is : {expWhere.Body}");
                using (new PerformanceScope("GetChildrenNodesPaged count"))
                {
                    count = queryResults.Count();
                }
                ResetPagerInfo(node, count);

                if (node.ChildrenCount > 0)
                {
                    if (node.SourceType == (int)SourceFlag.Teams)
                    {
                        queryResults = queryResults.OrderBy(n => n.Name).ThenBy(n => n.Url).Skip(node.PageIndex * node.PageSize).Take(node.PageSize);
                    }
                    else {
                        queryResults = queryResults.OrderBy(n => n.Url).Skip(node.PageIndex * node.PageSize).Take(node.PageSize);
                    }
                    logger.Info($"GetChildrenNodesPaged list: Get children list exp is : {queryResults.Expression}");
                    List<RMRemoteNode> list = null;
                    using (new PerformanceScope("GetChildrenNodesPaged list"))
                    {
                        list = queryResults.ToList();
                    }
                    node.Children = list.ConvertAll(i => Convert2TreeNode(i, context, node.SourceType));
                }
                else
                {
                    node.Children = new List<RMSPSampleTreeNode>();
                }
                return node;
            }
            catch (Exception ex)
            {
                logger.Error($"GetChildrenNodesPaged error : {ex}");
                if (ex.InnerException != null)
                {
                    logger.Error($"GetChildrenNodesPaged InnerException : {ex.InnerException}");
                }
                throw;
            }
        }

        private RMSPSampleTreeNode GetAuthorizedChildrenNodesPaged(RMSPSampleTreeNode node, string orderingClause, Func<RMDbContext, Tuple<string, SqlParameter[]>> getQueryAllSql)
        {
            ExecuteWithRetry(context =>
            {
                var queryAllSqlInfo = getQueryAllSql(context);
                var queryParameters = queryAllSqlInfo.Item2 ?? new SqlParameter[] { };
                string queryAllSql = queryAllSqlInfo.Item1;
                string queryCountSql = $"SELECT COUNT(1) {queryAllSql.Substring(queryAllSql.IndexOf("FROM", StringComparison.OrdinalIgnoreCase))}";
                string pagedQuerySql = DatabaseUtility.GetPaginatedSQL(node.PageIndex * node.PageSize, node.PageSize, queryAllSql, orderingClause);
                var total = context.Database.SqlQuery<int>(queryCountSql, queryParameters).FirstOrDefault();
                ResetPagerInfo(node, total);
                if (total > 0)
                {
                    var results = context.Database.SqlQuery<RMRemoteNode>(
                        pagedQuerySql, queryParameters.Select(p => (p as ICloneable).Clone()).ToArray()
                    ).ToList();
                    node.Children = results.ConvertAll(i =>
                    {
                        var child = Convert2TreeNode(i, context, node.SourceType);
                        child.Parent = node;
                        child.ParentId = node.Id;
                        return child;
                    });
                }
                else
                {
                    node.Children = new List<RMSPSampleTreeNode>();
                }

            });

            return node;
        }

        private RMSPSampleTreeNode GetAuthorizedChildrenNodesPagedForSearch(RMSPSampleTreeNode node, string orderingClause, Func<RMDbContext, Tuple<string, SqlParameter[]>> getQueryAllSql)
        {
            using var context = GetNewContext();
            context.Database.CommandTimeout = 900;
            var queryAllSqlInfo = getQueryAllSql(context);
            var queryParameters = queryAllSqlInfo.Item2 ?? new SqlParameter[] { };
            string queryAllSql = queryAllSqlInfo.Item1;
            string queryCountSql = $"SELECT COUNT(1) {queryAllSql.Substring(queryAllSql.IndexOf("FROM", StringComparison.OrdinalIgnoreCase))}";
            string pagedQuerySql = DatabaseUtility.GetPaginatedSQL(node.PageIndex * node.PageSize, node.PageSize, queryAllSql, orderingClause);
            var total = context.Database.SqlQuery<int>(queryCountSql, queryParameters).FirstOrDefault();
            ResetPagerInfo(node, total);
            if (total > 0)
            {
                var results = context.Database.SqlQuery<RMRemoteNode>(
                    pagedQuerySql, queryParameters.Select(p => (p as ICloneable).Clone()).ToArray()
                ).ToList();
                node.Children = results.ConvertAll(i =>
                {
                    var child = Convert2TreeNode(i, context, node.SourceType);
                    child.Parent = node;
                    child.ParentId = node.Id;
                    return child;
                });
            }
            else
            {
                node.Children = new List<RMSPSampleTreeNode>();
            }

            return node;
        }

        public List<RemoteWebApplication> GetRemoteWebApplications()
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes.Where(r => r.NodeLevel == NodeLevel_WebApplication).ToList();
                return nodes.ConvertAll(ConvertToWebApplication);
            });
        }

        public List<RemoteWebApplication> GetAuthorisedSkyDriveProGroups()
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes.Where(r => r.NodeLevel == NodeLevel_SkyDriveProGroup).ToList();
                return nodes.ConvertAll(ConvertToWebApplication);
            });
        }

        public List<RemoteWebApplication> GetAuthorisedOffice365GroupSitesGroups()
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes.Where(r => r.NodeLevel == NodeLevel_O365GroupSitesGroup).ToList();
                return nodes.ConvertAll(ConvertToWebApplication);
            });
        }

        public List<RemoteWebApplication> GetAuthorisedPrivateChannelSitesGroups()
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes.Where(r => r.NodeLevel == NodeLevel_PrivateChannelSitesGroup).ToList();
                return nodes.ConvertAll(ConvertToWebApplication);
            });
        }

        public List<RemoteWebApplication> GetAuthorisedAllSiteGroups(bool includeO365 = false, bool includePrivateChannel = false)
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes.Where(r => r.NodeLevel == NodeLevel_WebApplication || r.NodeLevel == NodeLevel_SkyDriveProGroup ||
                        (includeO365 && r.NodeLevel == NodeLevel_O365GroupSitesGroup) || (includePrivateChannel && r.NodeLevel == NodeLevel_PrivateChannelSitesGroup)).ToList();
                return nodes.ConvertAll(ConvertToWebApplication);
            });
        }

        public List<RMRemoteNode> GetAllContainers()
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes.Where(item => string.IsNullOrEmpty(item.ParentId)).ToList();
            });
        }

        public List<string> GetAllSPContainerIds()
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes.Where(item => string.IsNullOrEmpty(item.ParentId) && (item.NodeLevel == NodeLevel_WebApplication ||
                        item.NodeLevel == NodeLevel_O365GroupSitesGroup ||
                        item.NodeLevel == NodeLevel_PrivateChannelSitesGroup)).Select(item => item.Id).ToList();
            });
        }

        public List<string> GetAllTeamsContainerIds()
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes.Where(item => string.IsNullOrEmpty(item.ParentId) && (item.NodeLevel == NodeLevel_O365GroupSitesGroup ||
                        item.NodeLevel == NodeLevel_PrivateChannelSitesGroup)).Select(item => item.Id).ToList();
            });
        }

        public List<RMRemoteNode> GetAllTeamsContainers()
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes.Where(item => string.IsNullOrEmpty(item.ParentId) && (item.NodeLevel == NodeLevel_O365GroupSitesGroup ||
                        item.NodeLevel == NodeLevel_PrivateChannelSitesGroup)).ToList();
            });
        }

        public Dictionary<string, List<RMRemoteNode>> GetAllHasChannelTeamsNodes(string containerId)
        {

            Func<RMDbContext, Tuple<string, SqlParameter[]>> getSCQuery = context =>
            {
                var query = @$"SELECT rn.TeamId " +
                $" FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].[RMRemoteNodes] rn" +
                $" INNER JOIN " +
                $" (SELECT TeamId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].[RMRemoteNodes]" +
                $" GROUP BY TeamId " +
                $" HAVING COUNT(*) > 1) " +
                $" groupedTeams ON rn.TeamId = groupedTeams.TeamId " +
                $" WHERE (rn.NodeLevel = {NodeLevel_O365GroupSites} " +
                $" AND rn.ParentId = @parentId);";
                SqlParameter[] parameters = new SqlParameter[1] { new SqlParameter("@parentId", containerId) };
                return new Tuple<string, SqlParameter[]>(query, parameters);
            };
            var node = ExecuteWithRetry(context =>
            {
                var rn = context.Database.SqlQuery<string>(getSCQuery(context).Item1, getSCQuery(context).Item2).ToList();
                var result = context.RMRemoteNodes.Where(item => rn.Contains(item.TeamId)).GroupBy(item => item.TeamId).ToDictionary(item => item.Key, item => item.ToList());
                return result;
            });

            return node;
        }

        public long GetChannnelNodeCount()
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes.Count(item => item.NodeLevel == NodeLevel_PrivateChannel ||
                        item.NodeLevel == NodeLevel_SharedChannel);
            });
        }

        public void UpdateContainers(List<RMRemoteNode> containers)
        {
            using (var context = GetNewContext())
            {
                foreach (var container in containers)
                {
                    this.ApplyCurrentValues(context, container);
                }
            }
        }


        private RemoveNodeType ConvertNodeLevelToType(int level)
        {
            switch (level)
            {
                case NodeLevel_WebApplication:
                    return RemoveNodeType.SiteCollection;
                case NodeLevel_SkyDriveProGroup:
                case NodeLevel_SkyDrivePro:
                    return RemoveNodeType.SkyDrivePro;
                case NodeLevel_O365GroupSitesGroup:
                    return RemoveNodeType.O365GroupSites;
                case NodeLevel_PrivateChannelSitesGroup:
                    return RemoveNodeType.PrivateChannel;
                default:
                    return RemoveNodeType.SiteCollection;
            }
        }

        protected RemoteSiteCollection ConvertToSiteCollection(RMRemoteNode domain)
        {
            if (domain == null)
            {
                return null;
            }
            var dto = new RemoteSiteCollection();
            dto.id = domain.Id;
            dto.url = domain.Url;
            dto.parentId = domain.ParentId;
            dto.username = RMDatabaseDefaultEncryptor.DecryptToString(domain.UserName);
            dto.domain = domain.DomainName;
            dto.state = (SiteCollectionState)domain.State;
            dto.BPOSMould = domain.BposMode;
            dto.CreateTime = domain.CreateTime;
            dto.TemplateName = domain.TemplateName;
            dto.SPVersion = domain.SPVersion;
            dto.TemplateTitle = domain.TemplateTitle;
            dto.IsPublicWebSite = domain.IsPublicWebSite;
            dto.Name = domain.Name;
            //CP中用来区分SiteCollection和SkyDrive Pro（Create、Update）
            dto.NodeType = GetNodeTypeByNodeLevel(domain.NodeLevel);
            dto.SiteCollectionType = (SiteCollectionType)domain.SiteCollectionType;
            dto.AdminUrl = domain.AdminUrl;
            dto.ServiceAccountId = domain.ServiceAccountId;
            dto.TenantId = domain.TenantId;
            dto.AuthType = (BposConnectionType)domain.AuthType;
            dto.AppType = (AppType)domain.AppType;
            dto.ScanSource = (RemoteNodeScanSource)domain.ScanSource;
            dto.TeamId = domain.TeamId;
            dto.ObjectId = domain.ObjectId;
            //dto.SecondParentId = domain.SecondParentId;
            if (domain.AvailableAgentIds != null)
            {
                dto.AvailableAgentIds = new List<string>(domain.AvailableAgentIds.Split(','));
            }
            dto.FromDAO = domain.FromDAO;
            return dto;
        }

        protected RemoteWebApplication ConvertToWebApplication(RMRemoteNode domain)
        {
            if (domain == null)
            {
                return null;
            }
            var dto = new RemoteWebApplication();
            dto.id = domain.Id;
            dto.url = domain.Url;
            dto.description = domain.Description;
            dto.modifiedDate = domain.ModifiedDate;
            dto.domainName = domain.DomainName;
            //CP中用来区分SiteCollection和SkyDrive Pro（Create、Update）
            dto.NodeType = GetNodeTypeByNodeLevel(domain.NodeLevel);
            dto.FromDAO = domain.FromDAO;
            return dto;
        }

        private RemoveNodeType GetNodeTypeByNodeLevel(int nodeLevel)
        {
            if (nodeLevel == NodeLevel_SkyDriveProGroup || nodeLevel == NodeLevel_SkyDrivePro)
            {
                return RemoveNodeType.SkyDrivePro;
            }
            if (nodeLevel == NodeLevel_O365GroupSitesGroup || nodeLevel == NodeLevel_O365GroupSites)
            {
                return RemoveNodeType.O365GroupSites;
            }
            if (nodeLevel == NodeLevel_PrivateChannelSitesGroup || nodeLevel == NodeLevel_PrivateChannel || nodeLevel == NodeLevel_SharedChannel)
            {
                return RemoveNodeType.PrivateChannel;
            }
            return RemoveNodeType.SiteCollection;
        }

        private static AvePoint.GCommon.Contract.Tree.Object.NodeType ConvertSPNodeTypeByNodeLevel(int nodeLevel)
        {
            if (nodeLevel == NodeLevel_SkyDriveProGroup || nodeLevel == NodeLevel_SkyDrivePro)
            {
                return AvePoint.GCommon.Contract.Tree.Object.NodeType.SkyDriveProSitesGroup;
            }
            if (nodeLevel == NodeLevel_O365GroupSitesGroup || nodeLevel == NodeLevel_O365GroupSites)
            {
                return AvePoint.GCommon.Contract.Tree.Object.NodeType.O365GroupSitesGroup;
            }
            if (nodeLevel == NodeLevel_PrivateChannelSitesGroup || nodeLevel == NodeLevel_PrivateChannel || nodeLevel == NodeLevel_SharedChannel)
            {
                return AvePoint.GCommon.Contract.Tree.Object.NodeType.PrivateChannelSitesGroup;
            }
            return AvePoint.GCommon.Contract.Tree.Object.NodeType.SharePointSitesGroup;
        }

        private static TeamsChannelType Convert2ChannelType(int nodeLevel)
        {
            if (nodeLevel == NodeLevel_PrivateChannel)
            {
                return TeamsChannelType.Private;
            }
            if (nodeLevel == NodeLevel_SharedChannel)
            {
                return TeamsChannelType.Shared;
            }

            return TeamsChannelType.None;
        }

        protected RMRemoteNode ConvertToDomain(RemoteSiteCollection siteCollection)
        {
            if (siteCollection == null)
            {
                return null;
            }
            var domain = new RMRemoteNode();
            domain.Id = siteCollection.id;
            domain.Url = siteCollection.url;
            domain.ParentId = siteCollection.parentId;
            domain.UserName = RMDatabaseDefaultEncryptor.EncryptToString(siteCollection.username);
            domain.DomainName = siteCollection.domain;
            domain.State = (int)siteCollection.state;
            domain.BposMode = siteCollection.BPOSMould;
            domain.CreateTime = siteCollection.CreateTime;
            domain.TemplateName = siteCollection.TemplateName;
            domain.SPVersion = siteCollection.SPVersion;
            //CP中用来区分SiteCollection和SkyDrive Pro（Create、Update）
            domain.NodeLevel = (int)ConvertRemoteNodeTypeToNodeLevel(siteCollection);
            domain.TemplateTitle = siteCollection.TemplateTitle;
            domain.IsPublicWebSite = siteCollection.IsPublicWebSite;
            domain.Name = siteCollection.Name;
            domain.SiteCollectionType = (int)siteCollection.SiteCollectionType;
            domain.AdminUrl = siteCollection.AdminUrl;
            domain.ServiceAccountId = siteCollection.ServiceAccountId;
            domain.TenantId = siteCollection.TenantId;
            domain.AppType = (int)siteCollection.AppType;
            domain.AuthType = (int)siteCollection.AuthType;
            if (siteCollection.AvailableAgentIds != null)
            {
                domain.AvailableAgentIds = string.Join(",", siteCollection.AvailableAgentIds.ToArray());
            }
            domain.FromDAO = siteCollection.FromDAO;
            return domain;
        }

        protected void ConvertToDomain(RemoteSiteCollection siteCollection, RMRemoteNode domain)
        {
            if (siteCollection == null)
            {
                return;
            }
            domain.Id = siteCollection.id;
            domain.ObjectId = siteCollection.ObjectId;
            domain.Url = siteCollection.url;
            domain.ParentId = siteCollection.parentId;
            domain.UserName = RMDatabaseDefaultEncryptor.EncryptToString(siteCollection.username);
            domain.DomainName = siteCollection.domain;
            domain.State = (int)siteCollection.state;
            domain.BposMode = siteCollection.BPOSMould;
            domain.CreateTime = siteCollection.CreateTime;
            domain.TemplateName = siteCollection.TemplateName;
            domain.SPVersion = siteCollection.SPVersion;
            //CP中用来区分SiteCollection和SkyDrive Pro（Create、Update）
            domain.NodeLevel = (int)ConvertRemoteNodeTypeToNodeLevel(siteCollection);
            domain.TemplateTitle = siteCollection.TemplateTitle;
            domain.IsPublicWebSite = siteCollection.IsPublicWebSite;
            domain.Name = siteCollection.Name;
            domain.SiteCollectionType = (int)siteCollection.SiteCollectionType;
            domain.AdminUrl = siteCollection.AdminUrl;
            domain.ServiceAccountId = siteCollection.ServiceAccountId;
            domain.TenantId = siteCollection.TenantId;
            domain.AuthType = (int)siteCollection.AuthType;
            domain.AppType = (int)siteCollection.AppType;
            domain.ScanSource = (int)siteCollection.ScanSource;
            domain.TeamId = siteCollection.TeamId;
            if (siteCollection.AvailableAgentIds != null)
            {
                domain.AvailableAgentIds = string.Join(",", siteCollection.AvailableAgentIds.ToArray());
            }
            domain.FromDAO = siteCollection.FromDAO;
        }

        private NodeLevel ConvertRemoteNodeTypeToNodeLevel(RemoteSiteCollection node)
        {
            NodeLevel nodeLevel = NodeLevel.Undefined;
            switch (node.NodeType)
            {
                case RemoveNodeType.SiteCollection:
                    nodeLevel = NodeLevel.SiteCollection;
                    break;
                case RemoveNodeType.SkyDrivePro:
                    nodeLevel = NodeLevel.SkyDrivePro;
                    break;
                case RemoveNodeType.O365GroupSites:
                    nodeLevel = NodeLevel.O365GroupSites;
                    break;
                case RemoveNodeType.PrivateChannel:
                    nodeLevel = node.ChannelType == TeamsChannelType.Private ? NodeLevel.PrivateChannel : NodeLevel.SharedChannel;
                    break;
            }
            return nodeLevel;
        }

        protected RMRemoteNode ConvertToDomain(RemoteWebApplication webApplication)
        {
            if (webApplication == null)
            {
                return null;
            }
            var domain = new RMRemoteNode();
            domain.Id = webApplication.id;
            domain.Url = webApplication.url;
            domain.Description = webApplication.description;
            domain.ModifiedDate = webApplication.modifiedDate;
            //CP中用来区分SiteCollection和SkyDrive Pro（Create、Update）
            domain.NodeLevel = (int)ConvertRemoteNodeTypeToGroupNodeLevel(webApplication.NodeType);
            domain.FromDAO = webApplication.FromDAO;
            return domain;
        }

        protected void ConvertToDomain(RemoteWebApplication webApplication, RMRemoteNode domain)
        {
            if (webApplication == null)
            {
                return;
            }
            domain.Id = webApplication.id;
            domain.Url = webApplication.url;
            domain.Description = webApplication.description;
            domain.ModifiedDate = DateTime.UtcNow.Ticks;
            //CP中用来区分SiteCollection和SkyDrive Pro（Create、Update）
            domain.NodeLevel = (int)ConvertRemoteNodeTypeToGroupNodeLevel(webApplication.NodeType);
            domain.FromDAO = webApplication.FromDAO;
            domain.AosId = webApplication.AosId;
        }

        private NodeLevel ConvertRemoteNodeTypeToGroupNodeLevel(RemoveNodeType nodeType)
        {
            NodeLevel nodeLevel = NodeLevel.Undefined;
            switch (nodeType)
            {
                case RemoveNodeType.SiteCollection:
                    nodeLevel = NodeLevel.WebApplication;
                    break;
                case RemoveNodeType.SkyDrivePro:
                    nodeLevel = NodeLevel.SkyDriveProGroup;
                    break;
                case RemoveNodeType.O365GroupSites:
                    nodeLevel = NodeLevel.O365GroupSitesGroup;
                    break;
                case RemoveNodeType.PrivateChannel:
                    nodeLevel = NodeLevel.PrivateChannelGroup;
                    break;
            }
            return nodeLevel;
        }

        public List<TreeNodeCollection> GetAuthorisedAllSites()
        {
            if (IsOwnerOrPowerUser())
            {
                List<TreeNodeCollection> result = GetRemotenodeCollection();
                logger.Info("GetAuthorisedRemoteSiteCollections result count {0}", result.Count);
                return result;
            }
            else
            {
                List<TreeNodeCollection> result = GetRemotenodeCollection();
                if (result.Count > 0)
                {
                    var authorisedRemoteNodeIds = GetAllAuthorisedSiteCollectionIds();
                    result = result.FindAll(r => authorisedRemoteNodeIds.Contains(r.ParentId) || authorisedRemoteNodeIds.Contains(r.NodeId));
                    logger.Info("GetAuthorisedRemoteSiteCollections result count {0}", result.Count);
                }
                return result;
            }
        }

        public bool CheckSiteExistBySiteId(string siteId)
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes.Where(item => item.Id == siteId).Any();
            });
        }

        public RMRemoteNode GetRemoteNodeByParentId(Guid parentId) 
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes.Where(item => item.ParentId == parentId.ToString()).FirstOrDefault();
            });
        }

        public RMRemoteNode GetRemoteNodeById(Guid id)
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes.Where(item => item.Id == id.ToString()).FirstOrDefault();
            });
        }

        private bool IsOwnerOrPowerUser()
        {
            if (!string.IsNullOrEmpty(TenantThreadLocalValue.LogonUserId))
            {
                logger.Info($"IsOwnerOrPowerUser {TenantThreadLocalValue.LogonGroupId} {TenantThreadLocalValue.LogonUserId}");
                return IsOwnerOrPowerUser(TenantThreadLocalValue.LogonUserId);
            }
            else
            {
                logger.Info($"IsOwnerOrPowerUser {TenantThreadLocalValue.LogonGroupId}");
                return true;
            }
        }
        private bool IsOwnerOrPowerUser(string accountId)
        {
            if (accountId.Equals("DocAve System"))
            {
                logger.Info("current login user is DAO system.");
                return true;
            }
            var role = LnkUserRoleDao.GetAccountRole(accountId).RoleId;
            return role == (int)ObjectRoleType.Owner || role == (int)ObjectRoleType.PowerUser;
        }
        private List<TreeNodeCollection> GetRemotenodeCollection()
        {
            List<TreeNodeCollection> result = new List<TreeNodeCollection>();
            var reader = base.FindAll();
            foreach (var node in reader)
            {
                var nodeLevel = node.NodeLevel;
                if (nodeLevel == NodeLevel_SiteCollection || nodeLevel == NodeLevel_SkyDrivePro || nodeLevel == NodeLevel_O365GroupSites || nodeLevel == NodeLevel_PrivateChannel)
                {
                    var parentId = node.ParentId;
                    result.Add(new TreeNodeCollection()
                    {
                        NodeId = node.Id,
                        ParentId = parentId,
                        Scope = node.Url,
                        TenantId= node.TenantId
                    }); ;
                }
            }
            return result;
        }
        private List<string> GetAllAuthorisedSiteCollectionIds(string accountId = null)
        {
            List<string> result = new List<string>();
            string userId = TenantLocalValue.LogonUserId;
            var dataSourceCondition = $"DataSourceType IN ({(int)SourceFlag.SharePoint},{(int)SourceFlag.OneDrive})";
            Func<RMDbContext, Tuple<string, SqlParameter[]>> getQueryAllSql = context =>
            {
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                string queryAllSql =
$@"SELECT * FROM [{schemaName}].RMRemoteNodes WHERE Id IN ( 
  SELECT ScopeId FROM [{schemaName}].RMScopeRoleAssignments AS p 
    JOIN [{schemaName}].RMSecurityGroupMemberships m ON p.GroupId=m.GroupId 
	  AND m.UserId IN (
        SELECT UserId FROM [{schemaName}].RMAccounts WHERE IsRemoved=0 AND (
          UserId='{userId}' OR UserId IN (
            SELECT GroupId FROM [{schemaName}].RMLnkUserGroups WHERE UserId='{userId}'
          )
	    )
      )
    WHERE {dataSourceCondition}
)";
                return new Tuple<string, SqlParameter[]>(queryAllSql, null);
            };
            var rmRemoteNodes = ExecuteWithRetry(context =>
            {
                List<RMRemoteNode> rn = context.Database.SqlQuery<RMRemoteNode>(getQueryAllSql(context).Item1, new SqlParameter[] { }).ToList();
                return rn;
            });
            foreach (var temp in rmRemoteNodes)
            {
                result.Add(temp.Id);
            }
            return result.Distinct().ToList();
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionsByTeamsId(string teamsId, SiteCollectionState[] states)
        {
            ThrowUtil.ThrowIfNullOrEmpty(teamsId, "teamsId");
            ThrowUtil.ThrowIfNull(states, "states");
            //var siteCollections = new List<RemoteSiteCollection>();
            var queryStates = states.Select(s => (int)s);
            return ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 600;
                var siteCollections = context.RMRemoteNodes
                    .Where(m => m.TeamId == teamsId
                    && (
                        //m.NodeLevel == NodeLevel_SiteCollection
                        //|| m.NodeLevel == NodeLevel_SkyDrivePro
                        //|| 
                        m.NodeLevel == NodeLevel_O365GroupSites
                        || m.NodeLevel == NodeLevel_PrivateChannel
                        || m.NodeLevel == NodeLevel_SharedChannel
                        )
                    && queryStates.Contains(m.State)
                    )
                    // OrderBy SiteCollectionType to always show the teams/group site first => private sites => shared sites, ThenBy url
                    .OrderBy(n => n.SiteCollectionType).ThenBy(n => n.Url)
                    .ToList();
                return siteCollections.ConvertAll(m => new RemoteSiteCollection()
                {
                    id = m.Id,
                    url = m.Url,
                    parentId = m.ParentId,
                    domain = m.DomainName,
                    state = (SiteCollectionState)m.State,
                    BPOSMould = m.BposMode,
                    CreateTime = m.CreateTime,
                    TemplateName = m.TemplateName,
                    SPVersion = m.SPVersion,
                    TemplateTitle = m.TemplateTitle,
                    IsPublicWebSite = m.IsPublicWebSite,
                    Name = string.IsNullOrWhiteSpace(m.DisplayName) ? m.Name : m.DisplayName,
                    NodeType = GetNodeTypeByNodeLevel(m.NodeLevel),
                    SiteCollectionType = (SiteCollectionType)m.SiteCollectionType,
                    AdminUrl = m.AdminUrl,
                    ServiceAccountId = m.ServiceAccountId,
                    TenantId = m.TenantId,
                    AuthType = (BposConnectionType)m.AuthType,
                    AppType = (AppType)m.AppType,
                    ScanSource = (RemoteNodeScanSource)m.ScanSource,
                    TeamId = m.TeamId,
                    AvailableAgentIds = m.AvailableAgentIds != null ? new List<string>(m.AvailableAgentIds.Split(',')) : null,
                    FromDAO = m.FromDAO
                });
            });
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionsByTeamsIds(List<string> teamsIds, SiteCollectionState[] states)
        {
            ThrowUtil.ThrowIfNull(teamsIds, nameof(teamsIds));
            ThrowUtil.ThrowIfNull(states, nameof(states));
            if (teamsIds.Count == 0)
            {
                return new List<RemoteSiteCollection>();
            }

            var teamIdList = teamsIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (teamIdList.Count == 0)
            {
                return new List<RemoteSiteCollection>();
            }

            var queryStates = states
                .Select(state => (int)state)
                .Distinct()
                .ToList();
            if (queryStates.Count == 0)
            {
                return new List<RemoteSiteCollection>();
            }

            return ExecuteWithRetry(context =>
            {
                var schema = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@TeamIds", string.Join(",", teamIdList)),
                    new SqlParameter("@States", string.Join(",", queryStates)),
                    new SqlParameter("@NodeLevel_O365GroupSites", NodeLevel_O365GroupSites),
                    new SqlParameter("@NodeLevel_PrivateChannel", NodeLevel_PrivateChannel),
                    new SqlParameter("@NodeLevel_SharedChannel", NodeLevel_SharedChannel)
                };

                var sql = $@"
                    SELECT
                        rn.Id,
                        rn.Url,
                        rn.ParentId,
                        rn.DisplayName,
                        rn.Name,
                        rn.NodeLevel,
                        rn.SiteCollectionType,
                        rn.TenantId,
                        rn.TeamId,
                        rn.State,
                        rn.FromDAO
                    FROM [{schema}].[RMRemoteNodes] rn WITH (NOLOCK)
                    WHERE rn.TeamId IN (
                        SELECT [value] FROM STRING_SPLIT(@TeamIds, ',') WHERE [value] <> ''
                    )
                    AND rn.State IN (
                        SELECT [value] FROM STRING_SPLIT(@States, ',') WHERE [value] <> ''
                    )
                    AND (
                        rn.NodeLevel = @NodeLevel_O365GroupSites
                        OR rn.NodeLevel = @NodeLevel_PrivateChannel
                        OR rn.NodeLevel = @NodeLevel_SharedChannel
                    )
                    ORDER BY rn.SiteCollectionType ASC, rn.Url ASC;";

                var list = context.Database.SqlQuery<TeamsSiteCollectionRow>(sql, sqlParams.ToArray()).ToList();

                return list.Select(node => new RemoteSiteCollection
                {
                    id = node.Id,
                    url = node.Url,
                    parentId = node.ParentId,
                    Name = string.IsNullOrWhiteSpace(node.DisplayName) ? node.Name : node.DisplayName,
                    NodeType = node.NodeLevel == NodeLevel_PrivateChannel || node.NodeLevel == NodeLevel_SharedChannel
                        ? RemoveNodeType.PrivateChannel
                        : RemoveNodeType.O365GroupSites,
                    SiteCollectionType = (SiteCollectionType)node.SiteCollectionType,
                    TenantId = node.TenantId,
                    TeamId = node.TeamId,
                    state = (SiteCollectionState)node.State,
                    FromDAO = node.FromDAO
                }).ToList();
            });
        }

        public bool CheckTeamsExistByTeamsId(string teamsId)
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes
                .Where(item => item.TeamId == teamsId
                    && (item.SiteCollectionType == (int)SiteCollectionType.Teams || item.SiteCollectionType == (int)SiteCollectionType.Group))
                .Any();
            });
        }

        public RemoteSiteCollection GetTeamsNodeBySiteUrl(string url)
        {
            var siteNode = ExecuteWithRetry(context =>
            {
                var node = context.RMRemoteNodes.Where(m =>
                          m.Url == url
                          && (m.NodeLevel == NodeLevel_SiteCollection
                          || m.NodeLevel == NodeLevel_SkyDrivePro
                          || m.NodeLevel == NodeLevel_O365GroupSites
                          || m.NodeLevel == NodeLevel_PrivateChannel
                          || m.NodeLevel == NodeLevel_SharedChannel
                          )
                     ).FirstOrDefault();
                return node;
            });

            if (siteNode != null)
            {
                return ExecuteWithRetry(context =>
                {
                    var teamsNode = context.RMRemoteNodes
                        .Where(m => m.TeamId == siteNode.TeamId && (m.SiteCollectionType == (int)SiteCollectionType.Teams || m.SiteCollectionType == (int)SiteCollectionType.Group))
                        .FirstOrDefault();
                    return ConvertToSiteCollection(teamsNode);
                });
            }

            return null;
        }

        public Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> GetTeamsGroupAndChannelsCollectionByTeamsAddress(List<string> teamsAddress, bool needChannel = false)
        {
            return ExecuteWithRetry(context =>
            {
                var teamsNodes = context.RMRemoteNodes
                    .Where(m => teamsAddress.Contains(m.Name)
                        && (m.State == (int)SiteCollectionState.AccessSome || m.State == (int)SiteCollectionState.AccessAll)
                        && ((m.NodeLevel == NodeLevel_O365GroupSites
                        && (m.SiteCollectionType == (int)SiteCollectionType.Teams || m.SiteCollectionType == (int)SiteCollectionType.Group))))
                    .Select(ConvertToSiteCollection)
                    .ToList();
                var teamsIds = teamsNodes.Select(_ => _.TeamId).ToList();
                var nodes = context.RMRemoteNodes.Where( m =>
                        teamsIds.Contains(m.TeamId)
                        && (m.State == (int)SiteCollectionState.AccessSome || m.State == (int)SiteCollectionState.AccessAll)
                        && (needChannel || (m.NodeLevel == NodeLevel_O365GroupSites
                        && (m.SiteCollectionType == (int)SiteCollectionType.Teams || m.SiteCollectionType == (int)SiteCollectionType.Group))))
                .Select(ConvertToSiteCollection)
                .ToList();
                Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> result = new Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>>();
                foreach(var teams in teamsNodes)
                {
                    result.Add(teams, nodes.Where(_ => _.TeamId.Equals(teams.TeamId)).ToList());
                }
                return result;
            });
        }

        public Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> GetTeamsGroupAndChannelsCollectionBySiteUrls(List<string> siteUrls)
        {
            if (siteUrls == null || siteUrls.Count == 0)
            {
                return new Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>>();
            }

            return ExecuteWithRetry(context =>
            {
                var siteNodes = LoadAccessibleSitesByUrls(context, siteUrls);
                var teamsIds = siteNodes.Select(_ => _.TeamId)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .ToList();

                if (teamsIds.Count == 0)
                {
                    return new Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>>();
                }

                var teamsNodes = LoadTeamsGroupsByTeamIds(context, teamsIds);

                Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> result = new Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>>();
                foreach (var teams in teamsNodes)
                {
                    result.Add(teams, siteNodes.Where(_ => _.TeamId != null && _.TeamId.Equals(teams.TeamId)).ToList());
                }
                return result;
            });
        }

        private List<RemoteSiteCollection> LoadAccessibleSitesByUrls(RMDbContext context, IEnumerable<string> siteUrls)
        {
            var urlList = siteUrls?.ToList() ?? new List<string>();
            var result = new List<RemoteSiteCollection>();
            if (urlList.Count == 0)
            {
                return result;
            }

            foreach (var batch in SplitIntoBatches(urlList, 1000))
            {
                var batchResult = context.RMRemoteNodes
                    .Where(m => batch.Contains(m.Url)
                        && (m.State == (int)SiteCollectionState.AccessSome || m.State == (int)SiteCollectionState.AccessAll))
                    .ToList()
                    .Select(m => ConvertToSiteCollection(m));
                result.AddRange(batchResult);
            }

            return result;
        }

        private List<RemoteSiteCollection> LoadTeamsGroupsByTeamIds(RMDbContext context, IEnumerable<string> teamsIds)
        {
            var teamIdList = teamsIds?.ToList() ?? new List<string>();
            var result = new List<RemoteSiteCollection>();
            if (teamIdList.Count == 0)
            {
                return result;
            }

            foreach (var batch in SplitIntoBatches(teamIdList, 1000))
            {
                var batchResult = context.RMRemoteNodes
                    .Where(m => batch.Contains(m.TeamId)
                        && (m.State == (int)SiteCollectionState.AccessSome || m.State == (int)SiteCollectionState.AccessAll)
                        && m.NodeLevel == NodeLevel_O365GroupSites
                        && (m.SiteCollectionType == (int)SiteCollectionType.Teams || m.SiteCollectionType == (int)SiteCollectionType.Group))
                    .ToList()
                    .Select(m => ConvertToSiteCollection(m));
                result.AddRange(batchResult);
            }

            return result;
        }

        private static IEnumerable<List<T>> SplitIntoBatches<T>(IList<T> source, int batchSize)
        {
            for (int i = 0; i < source.Count; i += batchSize)
            {
                yield return source.Skip(i).Take(batchSize).ToList();
            }
        }

        public RemoteSiteCollection GetTeamsNodeByTeamsAddress(string teamsAddress)
        {
            return ExecuteWithRetry(context =>
            {
                var teamsNode = context.RMRemoteNodes
                    .Where(m => m.Name == teamsAddress && (m.SiteCollectionType == (int)SiteCollectionType.Teams || m.SiteCollectionType == (int)SiteCollectionType.Group))
                    .FirstOrDefault();
                return ConvertToSiteCollection(teamsNode);
            });
        }

        public RMSPTreeNode GetSPTeamsNodeByTeamsAddress(string teamsAddress)
        {
            return ExecuteWithRetry(context =>
            {
                var teamsNode = context.RMRemoteNodes
                    .Where(m => m.Name == teamsAddress && (m.SiteCollectionType == (int)SiteCollectionType.Teams || m.SiteCollectionType == (int)SiteCollectionType.Group))
                    .FirstOrDefault();
                return Convert2RMSPTreeNode(teamsNode, null, (int)SourceFlag.Teams);
            });
        }


        public RMSPTreeNode GetTeamsNodeByTeamsId(string teamsId)
        {
            return ExecuteWithRetry(context =>
            {
                var teamsNode = context.RMRemoteNodes
                    .Where(m => m.TeamId == teamsId && (m.SiteCollectionType == (int)SiteCollectionType.Teams || m.SiteCollectionType == (int)SiteCollectionType.Group))
                    .FirstOrDefault();
                return Convert2RMSPTreeNode(teamsNode, null, (int)SourceFlag.Teams);
            });
        }

        public RemoteSiteCollection GetO365TenantIdByName(string name)
        {
            return ExecuteWithRetry(context =>
            {
                var result = context.RMRemoteNodes.Where(item => item.Name == name);
                return ConvertToSiteCollection(result?.FirstOrDefault());
            });
        }

        public async IAsyncEnumerable<List<RMRemoteNode>> GetAllRemoteNodesAsync()
        {
            const int batchCount = 1_000;

            for (var index = 0; ; index++)
            {
                using var context = GetNewContext();
                context.Database.CommandTimeout = 900;
                var nodes = await context.RMRemoteNodes.OrderBy(item => item.Id).Skip(index * batchCount).Take(batchCount)
                    .ToListAsync();

                if (nodes.Count > 0)
                {
                    yield return nodes;
                }
                
                if(nodes.Count < batchCount)
                {
                    yield break;
                }
            }
        }

        public List<string> GetTeamsIdByContainerId(List<string> containerIds)
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes
                    .Where(m => containerIds.Contains(m.ParentId) && (m.SiteCollectionType == (int)SiteCollectionType.Teams || m.SiteCollectionType == (int)SiteCollectionType.Group))
                    .Select(_ => _.TeamId).Distinct().ToList();
            });
        }

        public Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> GetTeamsGroupAndChannelsCollectionByTeamsIds(List<string> teamsId, bool needChannel = false)
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(m => teamsId.Contains(m.TeamId)
                        && (needChannel || (m.NodeLevel == NodeLevel_O365GroupSites
                        && (m.SiteCollectionType == (int)SiteCollectionType.Teams || m.SiteCollectionType == (int)SiteCollectionType.Group))))
                    .Select(ConvertToSiteCollection)
                    .ToList();
                var teamsGroups = nodes.Where(r => r.NodeType == RemoveNodeType.O365GroupSites
                    && (r.SiteCollectionType == SiteCollectionType.Teams || r.SiteCollectionType == SiteCollectionType.Group)).ToList();
                var nodesByTeamId = nodes.ToLookup(node => node.TeamId, StringComparer.OrdinalIgnoreCase);
                Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> result = new Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>>();
                foreach (var teams in teamsGroups)
                {
                    result.Add(teams, nodesByTeamId[teams.TeamId].ToList());
                }
                return result;
            });
        }

        public RMSPSampleTreeNode Convert2SitesUnderTeamsTreeNode(RMRemoteNode node)
        {
            var treeNode = new RMSPSampleTreeNode();
            treeNode.Id = node.Id;
            treeNode.SPObjectId = node.Id;
            treeNode.Name = node.Url;
            treeNode.DisplayName = node.Url;
            treeNode.FullPath = node.Url;
            treeNode.Level = (int)NodeLevel.SiteCollection;
            treeNode.ChannelType = (int)Convert2ChannelType(node.NodeLevel);
            treeNode.NodeType = (int)ConvertSPNodeTypeByNodeLevel(node.NodeLevel);
            treeNode.SPType = (int)SPType.BPOS;
            treeNode.TeamsId = node.TeamId;
            treeNode.TeamName = string.IsNullOrWhiteSpace(node.DisplayName) ? node.Name : node.DisplayName;
            treeNode.SourceType = (int)SourceFlag.Teams;
            int spVersion = 0;
            if (int.TryParse(node.SPVersion, out spVersion))
            {
                treeNode.SPVersion = spVersion;
            }
            return treeNode;
        }

        public async IAsyncEnumerable<List<RMRemoteNode>> GetAllTeamsSiteAsync()
        {
            using var context = GetNewContext();
            context.Database.CommandTimeout = 900;

            const int batchCount = 1_000;

            for (var index = 0; ; index++)
            {
                var nodes = await context.RMRemoteNodes.Where(node => node.NodeLevel == NodeLevel_O365GroupSites || node.NodeLevel == NodeLevel_PrivateChannel || node.NodeLevel == NodeLevel_SharedChannel)
                    .OrderBy(node => node.TeamId)
                    .Skip(index * batchCount).Take(batchCount)
                    .ToListAsync();

                if (nodes.Count > 0)
                {
                    yield return nodes;
                }

                if (nodes.Count < batchCount)
                {
                    yield break;
                }
            }    
        }

        public List<RMRemoteNode> GetAllRemoteSiteCollectionURLsBySource(RMBrowseTreeNodeSourceType type)
        {
            var nodeLevels = new List<int>();

            bool hasUpgradeTeams = RMKeyValueDao.HasUpgradeTeams();

            switch (type)
            {
                case RMBrowseTreeNodeSourceType.SharepointOnline:
                    nodeLevels.Add(NodeLevel_SiteCollection);
                    if (!hasUpgradeTeams)
                    {
                        nodeLevels.AddRange(
                        [
                            NodeLevel_O365GroupSites,
                            NodeLevel_PrivateChannel,
                            NodeLevel_SharedChannel
                        ]);
                    }
                    break;
                case RMBrowseTreeNodeSourceType.SkyDrivePro:
                    nodeLevels.Add(NodeLevel_SkyDrivePro);
                    break;
                case RMBrowseTreeNodeSourceType.Teams:
                    if (hasUpgradeTeams)
                    {
                        nodeLevels.AddRange(
                        [
                            NodeLevel_O365GroupSites,
                            NodeLevel_PrivateChannel,
                            NodeLevel_SharedChannel
                        ]);
                    }
                    break;
                default:
                    nodeLevels.AddRange(
                    [
                        NodeLevel_SiteCollection,
                        NodeLevel_SkyDrivePro,
                        NodeLevel_O365GroupSites,
                        NodeLevel_PrivateChannel,
                        NodeLevel_SharedChannel
                    ]);
                    break;
            }

            if (nodeLevels.Count == 0) return new();

            return ExecuteWithRetry(context =>
            {
                context.Database.CommandTimeout = 900;
                return context.RMRemoteNodes
                    .Where(m => nodeLevels.Contains(m.NodeLevel))
                    .ToList();
            });
        }

        public Dictionary<string, List<string>> GetGroupAddressAndRelatedSiteUrlsDic(IEnumerable<string> siteUrls, Dictionary<string, string> teamsIdAddressMapping)
        {
            return ExecuteWithRetry(context =>
            {
                var nodes = context.RMRemoteNodes
                    .Where(m => siteUrls.Contains(m.Url) && !string.IsNullOrEmpty(m.TeamId))
                    .Select(m => new { m.Url, m.TeamId, m.SiteCollectionType, m.Name })
                    .ToList();

                return nodes
                    .GroupBy(m => m.TeamId)
                    .ToDictionary(
                        m => teamsIdAddressMapping.TryGetValue(m.Key, out var teamsAddress) ? teamsAddress : "",
                        r => r.Select(g => g.Url).ToList()
                    );
            });
        }

        public Dictionary<string, string> GetAllTeamId2TeamNameMapping()
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes
                    .Where(m => !string.IsNullOrEmpty(m.TeamId) 
                        && (m.SiteCollectionType == (int)SiteCollectionType.Teams || m.SiteCollectionType == (int)SiteCollectionType.Group))
                    .Select(m => new { m.TeamId, m.Name })
                    .ToDictionary(m => m.TeamId, m => m.Name);
            });
        }

        public Dictionary<string,string> GetAllGoogleDriveName(string searchKey, List<string> scopeIds)
        {
            using var context = GetNewContext();
            return context.RMRemoteNodes.Where(r => scopeIds.Contains(r.Id) && (r.NodeLevel == (int)NodeLevel.GoogleMyDrive || r.NodeLevel == (int)NodeLevel.GoogleSharedDrive)
                        && (string.IsNullOrEmpty(searchKey) || r.Name.Contains(searchKey)))
                        .OrderBy(_ => _.Name)
                        .ToDictionary(_ => _.Id, _ => _.Name);
        }

        public string GetTenantIdByObjectId(string objectId)
        {
            return ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes
                    .FirstOrDefault(remoteNode => remoteNode.ObjectId == objectId)?.TenantId ?? string.Empty;
            });
        }

        public RMRemoteNode GetGoogleDriveByName(string driveName)
        {
            using var context = GetNewContext();
            return context.RMRemoteNodes.Where(r => r.Name.Equals(driveName, StringComparison.OrdinalIgnoreCase) && (r.NodeLevel == (int)NodeLevel.GoogleMyDrive || r.NodeLevel == (int)NodeLevel.GoogleSharedDrive))
                .FirstOrDefault();
        }

        public string GetTenantNameByO365TenantId(string tenantId)
        {
            var node = ExecuteWithRetry(context =>
            {
                return context.RMRemoteNodes.FirstOrDefault(remoteNode => remoteNode.TenantId == tenantId);
            });

            var url = node.AdminUrl ?? node.Url;
            return WebUtil.GetTenantName(url);
        }

        public SearchSiteCollectionLazyLoadResponse SearchSiteCollectionLazyLoad(SearchSiteCollectionLazyLoadRequest condition, bool checkPermission, bool includeOrphenNode = false)
        {
            var response = new SearchSiteCollectionLazyLoadResponse();
            using var context = GetNewContext();
            context.Database.CommandTimeout = 900;
            var parentId = condition.ContainerId;
            // Protect against null parentId: treat null as empty string so SQL and LINQ comparisons are consistent
            if (parentId == null) parentId = string.Empty;
            const string enableTeamsFeatureKey = "EnableTeamsFeature";
            const string hasUpgradeTeamsKey = "HasUpgradeTeams";
            var featureFlags = context.RMKeyValue
                .Where(k => k.Key == enableTeamsFeatureKey || k.Key == hasUpgradeTeamsKey)
                .ToList();
            var hasUpgradeTeams = featureFlags.FirstOrDefault(k => k.Key == hasUpgradeTeamsKey);

            var hasUpgradeTeamsEnabled = hasUpgradeTeams != null
                && bool.TryParse(hasUpgradeTeams.Value, out var hasUpgradeParsed)
                && hasUpgradeParsed;

            var searchKey = condition.SearchKey;
            var isExactlySearch = (condition.SourceFlag == (int)SourceFlag.Teams || condition.SourceFlag == (int)SourceFlag.SharePoint)
                && !string.IsNullOrEmpty(searchKey) && searchKey.StartsWith('"') && searchKey.EndsWith('"');
            var searchKeyTrimmed = isExactlySearch ? searchKey.Trim('"') : searchKey;
            var results = new List<RMRemoteNode>(condition.PageSize);
            var lastUrl = condition.LastUrl;

            logger.Info($"GetSiteCollectionsByCursor list: parentId={parentId}, LoadCount={condition.PageSize}, SourceFlag={condition.SourceFlag}");
            using (new PerformanceScope("GetSiteCollectionsByCursor list"))
            {
                string querySql;
                List<SqlParameter> parameters;
                if (condition.SourceFlag == (int)SourceFlag.SharePoint && !hasUpgradeTeamsEnabled)
                {
                    logger.Info($"Building SharePoint and Teams search site collection lazy load hasUpgradeTeamsEnabled={hasUpgradeTeamsEnabled}");
                    querySql = BuildSharePointAndTeamsSearchSiteCollectionLazyLoadSql(context, condition, parentId, includeOrphenNode, checkPermission, isExactlySearch, lastUrl);
                    parameters = BuildSearchSiteCollectionLazyLoadParameters(condition, parentId, checkPermission, searchKey, searchKeyTrimmed, lastUrl);
                }
                else
                {
                    querySql = condition.SourceFlag switch
                    {
                        (int)SourceFlag.Teams => BuildTeamsSearchSiteCollectionLazyLoadSql(context, parentId, includeOrphenNode, checkPermission, isExactlySearch, lastUrl),
                        (int)SourceFlag.SharePoint => BuildSharePointSearchSiteCollectionLazyLoadSql(context, condition, parentId, includeOrphenNode, checkPermission, isExactlySearch, lastUrl),
                        _ => throw new ArgumentException($"Unsupported SourceFlag {condition.SourceFlag} for SearchSiteCollectionLazyLoad", nameof(condition.SourceFlag)),
                    };

                    parameters = BuildSearchSiteCollectionLazyLoadParameters(condition, parentId, checkPermission, searchKey, searchKeyTrimmed, lastUrl);
                }

                var rows = context.Database.SqlQuery<SearchSiteCollectionLazyLoadRow>(querySql, parameters.ToArray()).ToList();
                results = rows.Select(row => new RMRemoteNode
                {
                    Id = row.Id,
                    ObjectId = row.ObjectId,
                    Url = row.Url,
                    ParentId = row.ParentId,
                    NodeLevel = row.NodeLevel,
                    Name = row.Name,
                    DisplayName = row.DisplayName,
                    SiteCollectionType = row.SiteCollectionType,
                    TeamId = row.TeamId,
                    TenantId = row.TenantId,
                    SPVersion = row.SPVersion,
                }).ToList();
            }

            var hasNextPage = results.Count > condition.PageSize;
            if (hasNextPage)
            {
                results = results.Take(condition.PageSize).ToList();
            }

            response.Children = results.ConvertAll(i => Convert2TreeNode(i, context, condition.SourceFlag));

            // Build parent node map in one query to avoid per-child database lookups.
            var parentIds = new HashSet<string>(results
                .Where(r => !string.IsNullOrEmpty(r.ParentId))
                .Select(r => r.ParentId));
            if (!string.IsNullOrEmpty(parentId))
            {
                parentIds.Add(parentId);
            }

            var parentNodeById = new Dictionary<string, RMSPSampleTreeNode>();
            if (parentIds.Count > 0)
            {
                var parentRemotes = context.RMRemoteNodes
                    .AsNoTracking()
                    .Where(r => parentIds.Contains(r.Id))
                    .ToList();
                parentNodeById = parentRemotes.ToDictionary(
                    r => r.Id,
                    r =>
                    {
                        ConvertNodeName(r);
                        return Convert2TreeNode(r, context, condition.SourceFlag);
                    });
            }

            parentNodeById.TryGetValue(parentId ?? string.Empty, out var parentNode);

            var childParentDict = new Dictionary<string, RMSPSampleTreeNode>();
            if (parentNode == null)
            {
                foreach (var child in results)
                {
                    if (!string.IsNullOrEmpty(child.ParentId)
                        && parentNodeById.TryGetValue(child.ParentId, out var resolvedParent))
                    {
                        childParentDict[child.Url] = resolvedParent;
                    }
                }
            }

            response.Children?.ForEach(n =>
            {
                var resolvedParent = parentNode;
                if (resolvedParent == null)
                {
                    childParentDict.TryGetValue(n.DisplayName, out resolvedParent);
                }

                n.ParentId = resolvedParent?.Id;
                n.Parent = resolvedParent;
                n.ParentName = resolvedParent?.DisplayName;
            });

            response.HasNextPage = hasNextPage;
            response.LastUrl = results.Count > 0
                ? results[results.Count - 1].Url
                : null;

            return response;
        }

        private static void ConvertNodeName(RMRemoteNode node)
        {
            if (node.Url == "Default Office 365 Group Sites Group")
            {
                node.Url = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer");
            }
            if (node.Url == "Default_ SharePoint Sites_ Group")
            {
                node.Url = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup");
            }
            if (node.Url == "Default OneDrive for Business Group")
            {
                node.Url = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultOneDriveforBusinessGroup");
            }
            if (node.Url == "Default Private Channel Sites Container")
            {
                node.Url = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer");
            }
        }

        private string BuildTeamsSearchSiteCollectionLazyLoadSql(
            RMDbContext context,
            string parentId,
            bool includeOrphenNode,
            bool checkPermission,
            bool isExactlySearch,
            string lastUrl)
        {
            var querySql = BuildSearchSiteCollectionLazyLoadBaseSql(context, parentId, checkPermission);
            if (!includeOrphenNode)
            {
                querySql += " And (r.Name is not null or r.NodeLevel != 6000)";
            }
            querySql += $" AND ( {(isExactlySearch ? "r.[Name] = @SearchKeyTrimmed" : "r.[Name] like '%' + @SearchKey + '%'")} ) And (r.SiteCollectionType = 2 or r.SiteCollectionType = 4)";
            if (!string.IsNullOrEmpty(lastUrl))
            {
                querySql += " And r.Url > @LastUrl";
            }
            return querySql + " ORDER BY r.Url ASC";
        }

        private string BuildSharePointSearchSiteCollectionLazyLoadSql(
            RMDbContext context,
            SearchSiteCollectionLazyLoadRequest condition,
            string parentId,
            bool includeOrphenNode,
            bool checkPermission,
            bool isExactlySearch,
            string lastUrl)
        {
            var querySql = BuildSearchSiteCollectionLazyLoadBaseSql(context, parentId, checkPermission);
            if (!includeOrphenNode)
            {
                querySql += " And (r.Name is not null or r.NodeLevel != 6000)";
            }

            if (condition.IsArchiverTree)
            {
                querySql += $" AND {(isExactlySearch ? "r.[Url] = @SearchKeyTrimmed" : "r.[Url] like '%' + @SearchKey + '%'")} And (r.NodeLevel = {NodeLevel_SiteCollection} or (r.NodeLevel = {NodeLevel_SkyDrivePro} and r.Name is null))";
            }
            else
            {
                querySql += $" AND {(isExactlySearch ? "r.[Url] = @SearchKeyTrimmed" : "r.[Url] like '%' + @SearchKey + '%'")} And r.NodeLevel = {NodeLevel_SiteCollection}";
            }

            if (!string.IsNullOrEmpty(lastUrl))
            {
                querySql += " And r.Url > @LastUrl";
            }
            return querySql + " ORDER BY r.Url ASC";
        }

        private string BuildSharePointAndTeamsSearchSiteCollectionLazyLoadSql(
            RMDbContext context,
            SearchSiteCollectionLazyLoadRequest condition,
            string parentId,
            bool includeOrphenNode,
            bool checkPermission,
            bool isExactlySearch,
            string lastUrl)
        {
            var querySql = BuildSearchSiteCollectionLazyLoadBaseSql(context, parentId, checkPermission);
            if (!includeOrphenNode)
            {
                querySql += " And (r.Name is not null or r.NodeLevel != 6000)";
            }

            var sharePointSearchCondition = isExactlySearch
                ? "r.[Url] = @SearchKeyTrimmed"
                : "r.[Url] like '%' + @SearchKey + '%'";
            var sharePointNodeCondition = condition.IsArchiverTree
                ? $"(r.NodeLevel = {NodeLevel_SiteCollection} or (r.NodeLevel = {NodeLevel_SkyDrivePro} and r.Name is null))"
                : $"r.NodeLevel = {NodeLevel_SiteCollection}";
            var teamsSearchCondition = isExactlySearch
                ? "(r.[Url] = @SearchKeyTrimmed OR r.[Name] = @SearchKeyTrimmed)"
                : "(r.[Url] like '%' + @SearchKey + '%' OR r.[Name] like '%' + @SearchKey + '%')";

            querySql += $" AND (({sharePointSearchCondition} And {sharePointNodeCondition}) OR ({teamsSearchCondition} And (r.SiteCollectionType >= 2 and r.SiteCollectionType <= 4)))";

            if (!string.IsNullOrEmpty(lastUrl))
            {
                querySql += " And r.Url > @LastUrl";
            }
            return querySql + " ORDER BY r.Url ASC";
        }

        private string BuildSearchSiteCollectionLazyLoadBaseSql(RMDbContext context, string parentId, bool checkPermission)
        {
            const string selectedColumns = "r.Id, r.ObjectId, r.Url, r.ParentId, r.NodeLevel, r.Name, r.DisplayName, r.SiteCollectionType, r.TeamId, r.TenantId, r.SPVersion";
            if (checkPermission)
            {
                return
$@"SELECT TOP (@Take) {selectedColumns} FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMRemoteNodes r
WHERE EXISTS (
    SELECT ScopeId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMScopeRoleAssignments AS p
        JOIN [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMSecurityGroupMemberships m ON p.GroupId = m.GroupId
            AND m.UserId IN (
                SELECT UserId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMAccounts WHERE IsRemoved = 0 AND (
                    UserId = @UserId OR UserId IN (
                        SELECT GroupId FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMLnkUserGroups WHERE UserId = @UserId
                    )
                )
            )
        WHERE {(string.IsNullOrEmpty(parentId) ? "p.DataSourceType = @DataSourceType AND p.ScopeId IS NOT NULL AND p.ScopeId = r.ParentId" : "p.ScopeId = @ParentId")}
)
AND {(string.IsNullOrEmpty(parentId) ? "(1=1)" : "r.ParentId = @ParentId")}";
            }

            return
        $@"SELECT TOP (@Take) {selectedColumns} FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMRemoteNodes r
WHERE {(string.IsNullOrEmpty(parentId) ? "(1=1)" : "r.ParentId = @ParentId")}";
        }

        private List<SqlParameter> BuildSearchSiteCollectionLazyLoadParameters(
            SearchSiteCollectionLazyLoadRequest condition,
            string parentId,
            bool checkPermission,
            string searchKey,
            string searchKeyTrimmed,
            string lastUrl)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@Take", condition.PageSize + 1),
                new SqlParameter("@SearchKey", searchKey ?? string.Empty),
                new SqlParameter("@SearchKeyTrimmed", searchKeyTrimmed ?? string.Empty)
            };

            if (!string.IsNullOrEmpty(parentId))
            {
                parameters.Insert(1, new SqlParameter("@ParentId", parentId));
            }
            if (checkPermission)
            {
                parameters.Add(new SqlParameter("@UserId", TenantLocalValue.LogonUserId));
                if (string.IsNullOrEmpty(parentId))
                {
                    parameters.Add(new SqlParameter("@DataSourceType", condition.SourceFlag));
                }
            }
            if (!string.IsNullOrEmpty(lastUrl))
            {
                parameters.Add(new SqlParameter("@LastUrl", lastUrl));
            }

            return parameters;
        }

        public RemoteNodePara GetRemoteSiteCollectionNodeByUrl(string url)
        {
            int[] allowedNodeLevels = RMKeyValueDao.HasUpgradeTeams()
                ? [NodeLevel_SiteCollection]
                : [NodeLevel_SiteCollection,
                    NodeLevel_O365GroupSites,
                    NodeLevel_O365GroupSitesGroup,
                    NodeLevel_PrivateChannel,
                    NodeLevel_SharedChannel,
                    NodeLevel_PrivateChannelSitesGroup];

            return ExecuteWithRetry(context =>
            {
                var node = context.RMRemoteNodes
                    .Where(m => Enumerable.Contains(allowedNodeLevels, m.NodeLevel) && m.Url == url)
                    .Select(m => new
                    {
                        m.Url,
                        m.NodeLevel
                    }).FirstOrDefault();
                return node == null ? null : new RemoteNodePara()
                {
                    NodeName = node.Url,
                    NodeLevel = (NodeLevel)node.NodeLevel
                };
            });
        }

        public async Task<(string, string, string)> GetChannelSiteInfoAsync(string siteCollectionUrl)
        {
            using var context = GetNewContext();
            var teamsId = await context.RMRemoteNodes
                .Where(n => n.Url.ToLower() == siteCollectionUrl.ToLower() && n.SiteCollectionType == (int)SiteCollectionType.PrivateChannel)
                .Select(n => n.TeamId)
                .FirstOrDefaultAsync();
            var teamsInfo = await context.RMRemoteNodes
                .FirstOrDefaultAsync(n => n.TeamId == teamsId && (n.SiteCollectionType == (int)SiteCollectionType.Teams || n.SiteCollectionType == (int)SiteCollectionType.Group));
            return (teamsInfo?.Name, teamsInfo?.Url, teamsInfo?.TenantId);
        }
    }
}
