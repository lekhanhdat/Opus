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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Schedule;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using Cloud.Sdk.Data.Dao;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Presentation;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeLevel = AvePoint.GCommon.Contract.Tree.Object.NodeLevel;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMArchiverSettingDao : BaseDao<RMArchiverSetting>, IRMArchiverSettingDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMArchiverSettingDao));

        private IEXOSettingRuleDao EXOSettingRuleDao => PlatformWindsorManager.GetService<IEXOSettingRuleDao>();

        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private IRMRemoteNodeDao mRMRemoteNodeDao;
        protected IRMRemoteNodeDao RMRemoteNodeDao
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

        public void DeleteArchiverSetting(Guid id, Guid siteId)
        {
            using (var context = GetNewContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    var groupId = GetGroupIdBySiteId(siteId);
                    //RMArchiverSetting spSetting = context.RMArchiverSettings.AsQueryable().Where(s => s.SiteGroupId.Equals(groupId) && s.SPObjectId.Equals(id) && s.SiteId.Equals(siteId)).FirstOrDefault();
                    RMArchiverSetting spSetting = context.RMArchiverSettings.AsQueryable().Where(s => s.SPObjectId.Equals(id) && s.SiteId.Equals(siteId) && s.SiteGroupId.Equals(groupId) && s.ContentSourceType != (int)ContentSourceType.Teams).FirstOrDefault();
                    if (spSetting != null)
                    {
                        var ruleMapping = context.RMExchangeOnlineSettingRuleMappings.Where(o => o.ScopeId.Equals(spSetting.Id));
                        if (ruleMapping != null)
                        {
                            context.RMExchangeOnlineSettingRuleMappings.RemoveRange(ruleMapping);
                            context.SaveChanges();
                        }
                        context.RMArchiverSettings.Remove(spSetting);
                        context.SaveChanges();
                    }
                    tran.Commit();
                }
            }
        }

        public void DeleteArchiverSetting(Guid id, Guid teamsId, Guid siteId)
        {
            using (var context = GetNewContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    var teams = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsId.ToString());
                    var groupId = Guid.Empty;
                    if (teams.Item1 != null)
                    {
                        groupId = new Guid(teams.Item1.parentId);
                    }
                    RMArchiverSetting spSetting = context.RMArchiverSettings.AsQueryable()
                        .Where(s => s.SPObjectId.Equals(id) && s.SiteId.Equals(siteId) && s.TeamsId.Equals(teamsId) && s.SiteGroupId.Equals(groupId)
                        && s.ContentSourceType == (int)ContentSourceType.Teams).FirstOrDefault();
                    if (spSetting != null)
                    {
                        var ruleMapping = context.RMExchangeOnlineSettingRuleMappings.Where(o => o.ScopeId.Equals(spSetting.Id));
                        if (ruleMapping != null)
                        {
                            context.RMExchangeOnlineSettingRuleMappings.RemoveRange(ruleMapping);
                            context.SaveChanges();
                        }
                        context.RMArchiverSettings.Remove(spSetting);
                        context.SaveChanges();
                    }
                    tran.Commit();
                }
            }
        }

        public void DeleteArchiverSettingByContentSourceType(Guid id, Guid siteId, Guid teamsId = default, ContentSourceType type = ContentSourceType.SharePoint)
        {
            if (type == ContentSourceType.Teams)
            {
                DeleteArchiverSetting(id, teamsId, siteId);
                return;
            }
            DeleteArchiverSetting(id, siteId);
        }

        public async Task<int> DeleteMigratedArchiverSettings()
        {
            var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].RMArchiverSettings WHERE DAOMigrated=1";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public async Task<int> DeleteMigratedRuleMappings()
        {
            var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].RMExchangeOnlineSettingRuleMappings WHERE DAOMigrated=1";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public async Task<int> DeleteMigratedSchedulesAsync()
        {
            var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].RMSchedules WHERE DAOMigrated=1";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        private Guid GetGroupIdBySiteId(Guid siteId)
        {
            var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteId.ToString());
            return site != null ? new Guid(site.parentId) : Guid.Empty;
        }
        public RMArchiverSetting LoadArchiverSetting(Guid id, Guid siteId)
        {
            using (var context = GetNewContext())
            {
                RMArchiverSetting spSetting = null;
                if (siteId != Guid.Empty)
                {
                    var remoteSite = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteId.ToString());
                    var groupId = remoteSite?.parentId;
                    if (remoteSite.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.PrivateChannel && RMKeyValueDao.HasUpgradeTeams())
                    {
                        var groupSite = RMRemoteNodeDao.GetTeamsNodeByTeamsId(remoteSite.TeamId);
                        groupId = groupSite?.ParentId;
                    }
                        
                    if (!string.IsNullOrEmpty(groupId))
                    {
                        spSetting = context.RMArchiverSettings.AsQueryable()
                            .Where(s => s.SPObjectId.Equals(id) && s.SiteId.Equals(siteId) && s.SiteGroupId.Equals(new Guid(groupId)))
                            .ToList()
                            .OrderByDescending(a => a.ContentSourceType)
                            .ThenByDescending(a => a.CreateTime)
                            .FirstOrDefault();
                    }
                    else
                    {
                        spSetting = context.RMArchiverSettings.AsQueryable()
                            .Where(s => s.SPObjectId.Equals(id) && s.SiteId.Equals(siteId))
                            .ToList()
                            .OrderByDescending(a => a.ContentSourceType)
                            .ThenByDescending(a => a.CreateTime)
                            .FirstOrDefault();
                    }
                }
                if (spSetting == null)
                {
                    //add this for RA 3.1 old data.
                    spSetting = context.RMArchiverSettings.AsQueryable()
                        .Where(s => s.SPObjectId.Equals(id) && s.SiteId.Equals(Guid.Empty))
                        .ToList()
                        .OrderByDescending(a => a.ContentSourceType)
                        .ThenByDescending(a => a.CreateTime)
                        .FirstOrDefault();
                }
                return spSetting;
            }
        }

        public RMArchiverSetting LoadChannelArchiverSetting(Guid scopeId, string id)
        {
            using var context = GetNewContext();
            RMArchiverSetting spSetting = null;

            if (string.IsNullOrEmpty(id) || id == "null")
            {
                spSetting = context.RMArchiverSettings.AsQueryable()
                        .Where(s => s.SPObjectId.Equals(scopeId) && s.ContentSourceType == (int)ContentSourceType.SharePoint)
                        .ToList()
                        .OrderByDescending(a => a.ContentSourceType)
                        .ThenByDescending(a => a.CreateTime)
                        .FirstOrDefault();
            }
            else
            {
                spSetting = context.RMArchiverSettings.AsQueryable()
                        .Where(s => s.SPObjectId.Equals(scopeId) && s.Id.Equals(new Guid(id)) && s.ContentSourceType == (int)ContentSourceType.SharePoint)
                        .ToList()
                        .OrderByDescending(a => a.ContentSourceType)
                        .ThenByDescending(a => a.CreateTime)
                        .FirstOrDefault();
            }


            if (spSetting == null)
            {
                var channelContainerId = "41cfe969-e07b-45cb-a7d0-b022f967e929";
                spSetting = context.RMArchiverSettings.AsQueryable()
                    .Where(s => s.SPObjectId.Equals(new Guid(channelContainerId)) && s.ContentSourceType == (int)ContentSourceType.SharePoint)
                    .ToList()
                    .OrderByDescending(a => a.ContentSourceType)
                    .ThenByDescending(a => a.CreateTime)
                    .FirstOrDefault();
            }
            return spSetting;
        }


        public List<RMArchiverSetting> LoadAllArchiverSettingWithType(ContentSourceType type)
        {
            using (var context = GetNewContext())
            {
                return context.RMArchiverSettings.Where(_ => _.ContentSourceType == (int)type).ToList();
            }
        }

        public RMArchiverSetting LoadTeamsArchiverSetting(Guid id, Guid siteId, Guid teamsId)
        {
            using (var context = GetNewContext())
            {
                RMArchiverSetting spSetting = null;
                string groupId = null;

                if (teamsId != Guid.Empty)
                {
                    var remoteSite = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsId.ToString());
                    groupId = remoteSite.Item1?.parentId;
                }

                if (siteId != Guid.Empty)
                {
                    spSetting = context.RMArchiverSettings.AsQueryable()
                        .FirstOrDefault(s => s.SPObjectId == id && s.SiteId == siteId && s.ContentSourceType == (int)ContentSourceType.Teams &&
                            s.TeamsId == teamsId && (string.IsNullOrEmpty(groupId) || s.SiteGroupId == new Guid(groupId)));

                    if (spSetting != null) return spSetting;
                }

                if (siteId == Guid.Empty && teamsId != Guid.Empty)
                {
                    spSetting = context.RMArchiverSettings.AsQueryable()
                        .FirstOrDefault(s => s.SPObjectId == id && s.TeamsId == teamsId && s.SiteId == Guid.Empty && s.ContentSourceType == (int)ContentSourceType.Teams &&
                        (string.IsNullOrEmpty(groupId) || s.SiteGroupId == new Guid(groupId)));

                    if (spSetting != null) return spSetting;
                }

                return context.RMArchiverSettings.AsQueryable()
                    .FirstOrDefault(s => s.SPObjectId == id && s.TeamsId == Guid.Empty && s.SiteId == Guid.Empty && s.ContentSourceType == (int)ContentSourceType.Teams);
            }
        }

        /// <summary>
        ///  currently only check for Teams because SPO and OD are handled the same
        /// </summary>
        public RMArchiverSetting LoadArchiverSettingByContentSource(Guid id, Guid siteId, Guid teamsId = default, ContentSourceType type = ContentSourceType.SharePoint)
        {
            if (type == ContentSourceType.Teams)
            {
                return LoadTeamsArchiverSetting(id, siteId, teamsId);
            }

            return LoadArchiverSetting(id, siteId);
        }

        public RMArchiverSetting LoadCurrentNodeArchiverSettingByUrl(string siteUrl, ContentSourceType type = ContentSourceType.SharePoint)
        {
            using (var context = GetNewContext())
            {
                RMArchiverSetting spSetting = null;
                if (type == ContentSourceType.Teams)
                {
                    spSetting = context.RMArchiverSettings.AsQueryable().Where(s => s.Url.Equals(siteUrl) && s.ContentSourceType == (int)ContentSourceType.Teams).FirstOrDefault();

                }
                else
                {
                    spSetting = context.RMArchiverSettings.AsQueryable().Where(s => s.Url.Equals(siteUrl) && s.ContentSourceType != (int)ContentSourceType.Teams).FirstOrDefault();
                }

                return spSetting;
            }
        }
        public RMArchiverSetting LoadSiteArchiverSettingByUrl(string siteUrl, bool isTeamsContentSource = false)
        {
            var remoteSite = RMRemoteNodeDao.GetRemoteSiteCollectionByUrl(siteUrl);

            if (remoteSite == null)
            {
                return null;
            }

            var isTeamsAvaliable = IsTeamsAvailableAsync().GetAwaiter().GetResult();
            if (!isTeamsContentSource)
            {
                isTeamsContentSource = isTeamsAvaliable && !string.IsNullOrWhiteSpace(remoteSite.TeamId);
            }

            var parentId = remoteSite.parentId;
            if (remoteSite.SiteCollectionType == GCommon.Contract.SharePointBrowser.SiteCollectionType.PrivateChannel && isTeamsContentSource)
            {
                var (teamInfo, _) = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(remoteSite.TeamId);
                parentId = teamInfo.parentId;
            }

            RMArchiverSetting spSetting = null;
            using (var context = GetNewContext())
            {
                logger.Info($"restore job will load archive setting,site url :{siteUrl},parent id:{remoteSite.parentId}, team parent id: {parentId}");
                spSetting = context.RMArchiverSettings.AsQueryable()
                    .Where(s => s.Url.Equals(siteUrl) && s.SiteGroupId.Equals(new Guid(parentId)) && (!isTeamsContentSource || s.ContentSourceType == (int)ContentSourceType.Teams))
                    .ToList()
                    .OrderByDescending(a => a.ContentSourceType)
                    .ThenByDescending(a => a.CreateTime)
                    .FirstOrDefault();

                if (spSetting == null && isTeamsContentSource)
                {
                    spSetting = context.RMArchiverSettings.AsQueryable()
                    .Where(s => s.SPObjectId.Equals(new Guid(remoteSite.TeamId)) && s.SiteGroupId.Equals(new Guid(parentId)) && (!isTeamsContentSource || s.ContentSourceType == (int)ContentSourceType.Teams))
                    .ToList()
                    .OrderByDescending(a => a.ContentSourceType)
                    .ThenByDescending(a => a.CreateTime)
                    .FirstOrDefault();
                }

                if (spSetting == null)
                {
                    spSetting = context.RMArchiverSettings.AsQueryable()
                            .Where(s => s.SPObjectId.Equals(new Guid(parentId)) && (!isTeamsContentSource || s.ContentSourceType == (int)ContentSourceType.Teams))
                            .ToList()
                            .OrderByDescending(a => a.ContentSourceType)
                            .ThenByDescending(a => a.CreateTime)
                            .FirstOrDefault();
                }
            }
            return spSetting;
        }


        public List<RMArchiverSetting> LoadArchiverSettings()
        {
            using (var context = GetNewContext())
            {
                return context.RMArchiverSettings.AsNoTracking().ToList();
            }
        }

        public List<RMArchiverSetting> LoadArchiverSettingsUnderGroup(Guid groupId, ContentSourceType type = ContentSourceType.SharePoint)
        {
            using (var context = GetNewContext())
            {
                if (type == ContentSourceType.Teams)
                {
                    return context.RMArchiverSettings.AsQueryable().Where(s => s.SiteGroupId.Equals(groupId) && !s.SPObjectId.Equals(Guid.Empty) && s.ContentSourceType == (int)ContentSourceType.Teams).ToList();
                }
                return context.RMArchiverSettings.AsQueryable().Where(s => s.SiteGroupId.Equals(groupId) && !s.SPObjectId.Equals(Guid.Empty) && s.ContentSourceType != (int)ContentSourceType.Teams).ToList();
            }
        }
        public List<RMArchiverSetting> LoadArchiverSettingsUnderSite(Guid siteId, ContentSourceType sourceType = ContentSourceType.SharePoint)
        {
            using (var context = GetNewContext())
            {
                if (sourceType == ContentSourceType.Teams)
                {
                    return context.RMArchiverSettings.AsQueryable().Where(s => s.SiteId.Equals(siteId) && !s.SPObjectId.Equals(Guid.Empty) && s.ContentSourceType == (int)sourceType).ToList();
                }
                return context.RMArchiverSettings.AsQueryable().Where(s => s.SiteId.Equals(siteId) && !s.SPObjectId.Equals(Guid.Empty) && s.ContentSourceType != (int)ContentSourceType.Teams).ToList();
            }
        }
        public List<RMArchiverSetting> LoadArchiverSettingsUnderTeams(Guid teamsId, bool isOnlySiteSetting = false)
        {
            using (var context = GetNewContext())
            {
                return context.RMArchiverSettings.AsQueryable().Where(s => s.TeamsId.Equals(teamsId) && !s.SPObjectId.Equals(Guid.Empty) && s.ContentSourceType == (int)ContentSourceType.Teams && (!isOnlySiteSetting || s.SPObjectId.Equals(s.SiteId))).ToList();
            }
        }

        public List<RMArchiverSetting> LoadArchiverSettingsUnderTeamsIds(IEnumerable<Guid> teamsIds, bool isOnlySiteSetting = false)
        {
            using (var context = GetNewContext())
            {
                return context.RMArchiverSettings.AsQueryable().Where(s => teamsIds.Contains(s.TeamsId) && !s.SPObjectId.Equals(Guid.Empty) && s.ContentSourceType == (int)ContentSourceType.Teams && (!isOnlySiteSetting || s.SPObjectId.Equals(s.SiteId))).ToList();
            }
        }

        public RMArchiverSetting LoadArchiverSettingBySPObjectId(Guid objectId, Guid siteId, Guid siteGroupId)
        {
            using (var context = GetNewContext())
            {
                RMArchiverSetting spSetting = null;
                spSetting = context.RMArchiverSettings.AsQueryable().Where(s => s.SPObjectId.Equals(objectId) && s.SiteId.Equals(siteId) && s.SiteGroupId.Equals(siteGroupId) && s.ContentSourceType != (int)ContentSourceType.Teams).FirstOrDefault();
                return spSetting;
            }
        }

        public RMArchiverSetting LoadArchiverSettingBySPObjectId(Guid objectId, Guid siteId, Guid teamsId, Guid teamsGroupId)
        {
            using (var context = GetNewContext())
            {
                RMArchiverSetting spSetting = null;
                spSetting = context.RMArchiverSettings.AsQueryable()
                    .Where(s => s.SPObjectId.Equals(objectId) && s.SiteId.Equals(siteId) && s.TeamsId.Equals(teamsId) && s.SiteGroupId.Equals(teamsGroupId) && s.ContentSourceType == (int)ContentSourceType.Teams).FirstOrDefault();
                return spSetting;
            }
        }

        public List<RMArchiverSetting> LoadArchiverSettingBySiteGroupIds(List<Guid> siteGroupIds, ContentSourceType type = ContentSourceType.SharePoint)
        {
            using var context = GetNewContext();
            return [.. context.RMArchiverSettings.AsQueryable().Where(s => siteGroupIds.Contains(s.SiteGroupId))];
        }

        public int LoadArchiverSettingCountBySiteGroupIds(List<Guid> siteGroupIds, ContentSourceType type = ContentSourceType.SharePoint)
        {
            using var context = GetNewContext();
            return context.RMArchiverSettings.AsQueryable().Count(s => siteGroupIds.Contains(s.SiteGroupId));
        }

        public int UpgradeTeamsSettings(List<RMArchiverSetting> archiverSettings)
        {
            using var context = GetNewContext();
            return this.BatchUpdate(context, archiverSettings);
        }

        public async Task<bool> CleanSettingJobTimeAsync(RMSPTreeNode node)
        {
            try
            {
                if (node.Type == ContentSourceType.Teams && node.Level == (int)NodeLevel.SiteCollections) // virtual node no setting
                {
                    return false;
                }
                using (var context = GetNewContext())
                {
                    var groupId = Guid.Empty;
                    var teamsId = string.IsNullOrEmpty(node.TeamsId) ? Guid.Empty : new Guid(node.TeamsId);
                    var scopeId = new Guid(node.SPObjectId);
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        groupId = scopeId;
                    }
                    else
                    {
                        groupId = GetGroupIdByScopeId(scopeId, context, node.Type);
                    }
                    var setting = context.RMArchiverSettings.AsQueryable()
                        .Where(s => s.SiteGroupId == groupId && s.SPObjectId.Equals(new Guid(node.SPObjectId)) && s.TeamsId == teamsId
                        && s.ContentSourceType == (int)node.Type)
                        .FirstOrDefault();
                    if (setting != null)
                    {
                        setting.SettingTime = 0;
                        await UpdateAsync(setting);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"CleanSettingJobTime error {e}");
                return false;
            }
        }
        private Guid GetGroupIdByScopeId(Guid scopeId, RMDbContext context, ContentSourceType type)
        {
            var setting = context.RMArchiverSettings.Where(s => s.SPObjectId == scopeId && s.ContentSourceType == (int)type).FirstOrDefault();
            if (setting != null)
            {
                if (type == ContentSourceType.Teams)
                {
                    var teamsId = setting.TeamsId;
                    var teams = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsId.ToString()).Item1;
                    return teams != null ? new Guid(teams.parentId) : Guid.Empty;
                }
                else
                {
                    var siteId = setting.SiteId;
                    var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteId.ToString());
                    return site != null ? new Guid(site.parentId) : Guid.Empty;
                }
            }
            return Guid.Empty;
        }

        public async Task<RAReturnMessage> SaveArchiverSettingAsync(RMSPTreeNode node)
        {
            RAReturnMessage result = new RAReturnMessage();
            RMArchiverSetting archiverSetting = new RMArchiverSetting();
            archiverSetting.Id = Guid.NewGuid();
            archiverSetting.isEnableSuperUserDecrypt = node.IsEnableSuperUserDecrypt;
            archiverSetting.SupportLockedSite = node.Level <= (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Office365GroupEntire ? node.SupportLockedSite : IsSupportLockedSite(node);
            archiverSetting.SupportArchivedTeams = node.SupportArchivedTeams;
            archiverSetting.isIncludeManagedMetadataService = node.IsManagedMetadataService;
            archiverSetting.isEnableRemoveRetentionLabel = node.IsEnableRemoveRetentionLabel;
            archiverSetting.isIncludeWorkflowDefinition = node.IsWorkflowDefinition;
            archiverSetting.ContentSourceType = (int)node.Type;
            archiverSetting.EnableArchiverManagement = node.EnableArchiverManagement;
            archiverSetting.SPObjectId = new Guid(node.SPObjectId);
            archiverSetting.CreateTime = DateTime.UtcNow.Ticks;
            RMSPTreeNode siteNode = node;
            while (siteNode != null && siteNode.Level != (int)NodeLevel.SiteCollection)
            {
                siteNode = siteNode.Parent;
            }
            if (node.Level == (int)NodeLevel.Folder)
            {
                archiverSetting.Url = WebUtil.MakeFullUrl(siteNode?.FullPath, node.FullPath);
            }
            else
            {
                archiverSetting.Url = node.FullPath;
            }
            Guid siteId = Guid.Empty;
            if (siteNode != null)
            {
                siteId = new Guid(siteNode.SPObjectId);
                archiverSetting.SiteId = siteId;
            }

            RMSPTreeNode teamsNode = node;
            while (teamsNode != null && teamsNode.Level != (int)NodeLevel.Office365GroupEntire)
            {
                teamsNode = teamsNode.Parent;
            }

            Guid teamsId = Guid.Empty;
            if (teamsNode != null)
            {
                teamsId = new Guid(teamsNode.TeamsId);
                archiverSetting.TeamsId = teamsId;
            }

            RMSPTreeNode groupNode = node;
            while (groupNode != null && groupNode.Level != (int)NodeLevel.WebApplication)
            {
                groupNode = groupNode.Parent;
            }

            Guid groupId = Guid.Empty;
            if (groupNode != null)
            {
                groupId = new Guid(groupNode.SPObjectId);
                archiverSetting.SiteGroupId = groupId;
            }

            try
            {
                using (var context = GetNewContext())
                {
                    var mSetting = LoadArchiverSettingByContentSource(new Guid(node.SPObjectId), siteId, teamsId, node.Type);
                    if (mSetting == null)
                    {
                        context.RMArchiverSettings.Add(archiverSetting);
                        context.SaveChanges();
                    }
                    else
                    {
                        if (mSetting.CleanRestoredOption != null)
                        {
                            archiverSetting.CleanRestoredOption = mSetting.CleanRestoredOption;
                        }
                        archiverSetting.Id = mSetting.Id;
                        await this.UpdateAsync(archiverSetting);
                    }
                }
                if (node.Rules != null)
                {
                    EXOSettingRuleDao.SaveArchiverMappingRules(node, archiverSetting.Id);
                }
                result.MessageType = RAMessageType.Successful;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"SaveArchiverSettingAsync error {ex}");
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        public async Task<RAReturnMessage> SaveOrUpdateGeneralSettingAsync(RMSPTreeNode node)
        {
            RAReturnMessage result = new RAReturnMessage();
            RMArchiverSetting archiverSetting = new RMArchiverSetting();
            archiverSetting.CreateTime = DateTime.UtcNow.Ticks;
            try
            {
                using (var context = GetNewContext())
                {
                    RMSPTreeNode siteNode = node;
                    while (siteNode != null && siteNode.Level != (int)NodeLevel.SiteCollection)
                    {
                        siteNode = siteNode.Parent;
                    }

                    Guid siteId = Guid.Empty;
                    if (siteNode != null)
                    {
                        siteId = new Guid(siteNode.SPObjectId);
                        archiverSetting.SiteId = siteId;
                    }

                    RMSPTreeNode teamsNode = node;
                    while (teamsNode != null && teamsNode.Level != (int)NodeLevel.Office365GroupEntire)
                    {
                        teamsNode = teamsNode.Parent;
                    }

                    Guid teamsId = Guid.Empty;
                    if (teamsNode != null)
                    {
                        teamsId = new Guid(teamsNode.TeamsId);
                        archiverSetting.TeamsId = teamsId;
                    }

                    RMSPTreeNode groupNode = node;
                    while (groupNode != null && groupNode.Level != (int)NodeLevel.WebApplication)
                    {
                        groupNode = groupNode.Parent;
                    }
                    Guid groupId = Guid.Empty;
                    if (groupNode != null)
                    {
                        groupId = new Guid(groupNode.SPObjectId);
                    }
                    var mSetting = node.Type switch
                    {
                        ContentSourceType.Teams => LoadArchiverSettingBySPObjectId(new Guid(node.SPObjectId), siteId, teamsId, groupId),
                        _ => LoadArchiverSettingBySPObjectId(new Guid(node.SPObjectId), siteId, groupId),
                    };
                    if (mSetting == null)
                    {
                        archiverSetting.SiteGroupId = groupId;
                        archiverSetting.Id = Guid.NewGuid();
                        archiverSetting.ContentSourceType = (int)node.Type;
                        archiverSetting.EnableArchiverManagement = node.EnableArchiverManagement;
                        archiverSetting.isIncludeManagedMetadataService = node.IsManagedMetadataService;
                        archiverSetting.isIncludeWorkflowDefinition = node.IsWorkflowDefinition;
                        archiverSetting.SPObjectId = new Guid(node.SPObjectId);
                        archiverSetting.ContentSourceType = (int)node.Type;
                        CleanRestoredItemsExtension tempCleanRestoreSetting = new CleanRestoredItemsExtension()
                        {
                            EnableDelArchivedData = node.EnableDelArchivedData,
                            EnableCleanStubs = node.EnableCleanStubs,
                            CleanupAndDelRestoredType = node.CleanupAndDelRestoredType,
                            DayNum = node.DayNum
                        };
                        archiverSetting.CleanRestoredOption = SerializerHelper.SerializeByDataContractSerializer(tempCleanRestoreSetting);
                        if (node.Level == (int)NodeLevel.Folder)
                        {
                            archiverSetting.Url = WebUtil.MakeFullUrl(siteNode?.FullPath, node.FullPath);
                        }
                        else
                        {
                            archiverSetting.Url = node.FullPath;
                        }
                        archiverSetting.EnableArchiverManagement = node.EnableArchiverManagement;
                        context.RMArchiverSettings.Add(archiverSetting);
                        context.SaveChanges();
                    }
                    else
                    {
                        archiverSetting.Id = mSetting.Id;
                        archiverSetting.SupportLockedSite = mSetting.SupportLockedSite;
                        archiverSetting.isEnableRemoveRetentionLabel = mSetting.isEnableRemoveRetentionLabel;
                        archiverSetting.isEnableSuperUserDecrypt = mSetting.isEnableSuperUserDecrypt;
                        archiverSetting.isIncludeManagedMetadataService = mSetting.isIncludeManagedMetadataService;
                        archiverSetting.isIncludeWorkflowDefinition = mSetting.isIncludeWorkflowDefinition;
                        archiverSetting.SiteGroupId = mSetting.SiteGroupId;
                        archiverSetting.SiteId = mSetting.SiteId;
                        archiverSetting.TeamsId = mSetting.TeamsId;
                        archiverSetting.SPObjectId = mSetting.SPObjectId;
                        archiverSetting.ContentSourceType = mSetting.ContentSourceType;
                        if (LicenseHelperService.IsEnableDeleteRestoreDataFeature())
                        {
                            CleanRestoredItemsExtension tempCleanRestoreSetting = new CleanRestoredItemsExtension()
                            {
                                EnableCleanStubs = node.EnableCleanStubs,
                                EnableDelArchivedData = node.EnableDelArchivedData,
                                CleanupAndDelRestoredType = node.CleanupAndDelRestoredType,
                                DayNum = node.DayNum
                            };
                            archiverSetting.CleanRestoredOption = SerializerHelper.SerializeByDataContractSerializer(tempCleanRestoreSetting);
                        }
                        if (node.Level == (int)NodeLevel.Folder)
                        {
                            archiverSetting.Url = WebUtil.MakeFullUrl(siteNode?.FullPath, node.FullPath);
                        }
                        else
                        {
                            archiverSetting.Url = node.FullPath;
                        }
                        archiverSetting.EnableArchiverManagement = node.EnableArchiverManagement;
                        await this.UpdateAsync(archiverSetting);
                    }
                }
                result.MessageType = RAMessageType.Successful;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"SaveOrUpdateGeneralSettingAsync error {ex}");
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        public async Task SaveMigratedArchiverSettingAsync(ArchiverMigrationRuleSetting amRuleSetting, List<RMExchangeOnlineSettingRuleMapping> ruleMappings)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    RMArchiverSetting archiverSetting = new RMArchiverSetting();
                    archiverSetting.DAOMigrated = true;
                    archiverSetting.SiteGroupId = amRuleSetting.SiteGroupId;
                    archiverSetting.SiteId = amRuleSetting.SiteId;
                    archiverSetting.Id = new Guid(amRuleSetting.Id);
                    archiverSetting.ContentSourceType = amRuleSetting.ContentSourceType;
                    archiverSetting.isEnableSuperUserDecrypt = amRuleSetting.IsEnableSuperUserDecrypt;
                    archiverSetting.isEnableRemoveRetentionLabel = amRuleSetting.IsEnableRemoveRetentionLabel;
                    archiverSetting.EnableArchiverManagement = amRuleSetting.EnableArchiverManagement;
                    archiverSetting.isIncludeManagedMetadataService = amRuleSetting.IsIncludeManagedMetadataService;
                    archiverSetting.isIncludeWorkflowDefinition = amRuleSetting.IsIncludeWorkflowDefinition;
                    archiverSetting.SPObjectId = new Guid(amRuleSetting.NodeId);
                    archiverSetting.Url = amRuleSetting.Url;
                    var amScheduleDto = amRuleSetting.Schedule;
                    if (amScheduleDto != null)
                    {
                        archiverSetting.ScheduleId = amScheduleDto.Id;

                        RMSchedule sc = new RMSchedule();
                        sc.DAOMigrated = true;
                        sc.Id = amScheduleDto.Id;
                        sc.StartTime = amScheduleDto.StartTime;
                        sc.EndTime = amScheduleDto.EndTime;
                        sc.NextTime = amScheduleDto.NextTime;
                        sc.TimeZoneId = amScheduleDto.TimeZoneId;
                        sc.EndType = amScheduleDto.EndType;
                        sc.OccurrencesTotal = amScheduleDto.OccurrencesTotal;
                        sc.Occurrences = amScheduleDto.Occurrences;
                        sc.Interval = amScheduleDto.Interval;
                        sc.IntervalType = amScheduleDto.IntervalType;
                        sc.JobCategory = amScheduleDto.JobCategory;
                        sc.IsDaylightSaving = amScheduleDto.IsDaylightSaving;
                        sc.ProfileId = amScheduleDto.ProfileId;
                        sc.Extentions = amScheduleDto.Extentions;

                        context.Schedule.Add(sc);
                    }

                    context.RMArchiverSettings.Add(archiverSetting);

                    if (ruleMappings != null && ruleMappings.Count > 0)
                    {
                        context.RMExchangeOnlineSettingRuleMappings.AddRange(ruleMappings);
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"SaveMigratedArchiverSettingAsync error {ex}");
                throw;
            }
        }

        public async Task SaveMigratedDisabledArchiverSettingAsync(ArchiverMigrationRuleSetting amRuleSetting)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    RMArchiverSetting archiverSetting = new RMArchiverSetting();
                    archiverSetting.DAOMigrated = true;
                    archiverSetting.SiteGroupId = amRuleSetting.SiteGroupId;
                    archiverSetting.SiteId = amRuleSetting.SiteId;
                    archiverSetting.Id = new Guid(amRuleSetting.Id);
                    archiverSetting.ContentSourceType = amRuleSetting.ContentSourceType;
                    archiverSetting.isEnableSuperUserDecrypt = amRuleSetting.IsEnableSuperUserDecrypt;
                    archiverSetting.isEnableRemoveRetentionLabel = amRuleSetting.IsEnableRemoveRetentionLabel;
                    archiverSetting.EnableArchiverManagement = (int)EnableRecordManagementSetting.Disable;
                    archiverSetting.isIncludeManagedMetadataService = amRuleSetting.IsIncludeManagedMetadataService;
                    archiverSetting.isIncludeWorkflowDefinition = amRuleSetting.IsIncludeWorkflowDefinition;
                    archiverSetting.SPObjectId = new Guid(amRuleSetting.NodeId);
                    archiverSetting.Url = amRuleSetting.Url;

                    context.RMArchiverSettings.Add(archiverSetting);

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"SaveMigratedDisabledArchiverSettingAsync error {ex}");
                throw;
            }
        }

        public List<string> LoadBreakInheritNodeUrls(string scopeUrl, string siteObjectId = "", bool isTeamsContentSource = false)
        {
            List<string> result = new List<string>();
            using (var context = GetNewContext())
            {
                List<string> nodes = new List<string>();
                string temp = scopeUrl.TrimEnd('/') + '/';

                if (!isTeamsContentSource)
                {
                    if (string.IsNullOrEmpty(siteObjectId))
                    {
                        nodes = context.RMArchiverSettings.AsQueryable().Where(s => s.Url.StartsWith(temp) || s.Url.Equals(scopeUrl)).Select(p => p.Url).ToList();
                    }
                    else
                    {
                        nodes = context.RMArchiverSettings.AsQueryable().Where(s => (s.Url.StartsWith(temp) || s.Url.Equals(scopeUrl)) && s.SiteId.ToString().Equals(siteObjectId, StringComparison.OrdinalIgnoreCase)).Select(p => p.Url).ToList();
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(siteObjectId))
                    {
                        nodes = context.RMArchiverSettings.AsQueryable()
                            .Where(s => (s.Url.StartsWith(temp) || s.Url.Equals(scopeUrl)) && s.ContentSourceType == (int)ContentSourceType.Teams)
                            .Select(p => p.Url).ToList();
                    }
                    else
                    {
                        nodes = context.RMArchiverSettings.AsQueryable()
                            .Where(s => (s.Url.StartsWith(temp) || s.Url.Equals(scopeUrl)) && s.SiteId.ToString().Equals(siteObjectId, StringComparison.OrdinalIgnoreCase) && s.ContentSourceType == (int)ContentSourceType.Teams)
                            .Select(p => p.Url).ToList();
                    }
                }

                foreach (var n in nodes)
                {
                    if (!result.Contains(n))
                    {
                        logger.Info($"BreakInheritNodeUrl : {n}");
                        result.Add(n);
                    }
                }
            }
            return result;
        }

        private static async Task<bool> IsTeamsAvailableAsync()
        {
            const string enableTeamsFeatureKey = "EnableTeamsFeature";
            const string hasUpgradeTeamsKey = "HasUpgradeTeams";
            using var context = RMDBContextManager.GetNewDBContext();

            var keys = new[] { enableTeamsFeatureKey, hasUpgradeTeamsKey };
            var keyValues = await context.RMKeyValue.Where(k => Enumerable.Contains(keys, k.Key)).ToListAsync();
            var enableKv = keyValues.FirstOrDefault(k => k.Key == enableTeamsFeatureKey);
            var upgradeKv = keyValues.FirstOrDefault(k => k.Key == hasUpgradeTeamsKey);

            if (enableKv != null)
            {
                if (!bool.TryParse(enableKv.Value, out var enableParsed) || !enableParsed)
                {
                    return false;
                }
            }

            if (upgradeKv != null && bool.TryParse(upgradeKv.Value, out var upgraded) && upgraded)
            {
                return true;
            }

            return false;
        }

        #region Support Locked Site

        private bool IsSupportLockedSite(RMSPTreeNode node)
        {
            var groupNode = node.GetGroupNode();
            var siteNode = node.GetSiteCollectionNode();
            var groupNodeId = groupNode != null ? new Guid(groupNode.SPObjectId) : Guid.Empty;
            var siteNodeId = siteNode != null ? new Guid(siteNode.SPObjectId) : Guid.Empty;
            if (node.Type == ContentSourceType.Teams)
            {
                var teamsNode = node.GetTeamsNode();
                var teamsNodeId = teamsNode != null ? new Guid(teamsNode.SPObjectId) : Guid.Empty;
                return IsSupportLockedSiteForTeams(node.Parent, siteNodeId, teamsNodeId, groupNodeId);
            }
            else if (node.Type == ContentSourceType.SharePoint)
            {
                return IsSupportLockedSiteForSPO(node.Parent, siteNodeId, groupNodeId);
            }
            return false;
        }

        private bool IsVirtualNode(int nodeLevel)
        {
            var level = SafeConvertExtensions.ToEnum<NodeLevel>(nodeLevel);
            return level == NodeLevel.Lists ||
                level == NodeLevel.RootFolder ||
                level == NodeLevel.Items ||
                level == NodeLevel.Folders ||
                level == NodeLevel.Apps ||
                level == NodeLevel.Sites ||
                level == NodeLevel.SiteCollections;
        }

        private bool IsSupportLockedSiteForSPO(RMSPTreeNode node, Guid siteId, Guid siteGroupId)
        {
            while (node != null)
            {
                if (node.SupportLockedSite)
                    return true;
                if (!IsVirtualNode(node.Level) && Guid.TryParse(node.SPObjectId, out var spObjectId))
                {
                    var settings = LoadArchiverSettingBySPObjectId(spObjectId, siteId, Guid.Empty, siteGroupId, ContentSourceType.SharePoint, SafeConvertExtensions.ToEnum<NodeLevel>(node.Level));
                    if (settings is not null)
                        return settings.SupportLockedSite;
                }
                node = node.Parent;
            }
            return false;
        }

        private bool IsSupportLockedSiteForTeams(RMSPTreeNode node, Guid siteId, Guid teamsId, Guid teamsGroupId)
        {
            while (node != null)
            {
                if (node.SupportLockedSite)
                    return true;
                if (!IsVirtualNode(node.Level) && Guid.TryParse(node.SPObjectId, out var spObjectId))
                {
                    var settings = LoadArchiverSettingBySPObjectId(spObjectId, siteId, teamsId, teamsGroupId, ContentSourceType.Teams, SafeConvertExtensions.ToEnum<NodeLevel>(node.Level));
                    if (settings is not null)
                        return settings.SupportLockedSite;
                }
                node = node.Parent;
            }
            return false;
        }

        private RMArchiverSetting LoadArchiverSettingBySPObjectId(Guid objectId, Guid siteId, Guid teamsId, Guid groupId, ContentSourceType sourceType, NodeLevel nodeLevel)
        {
            RMArchiverSetting result = null;
            if (sourceType == ContentSourceType.Teams)
            {
                result = nodeLevel switch
                {
                    NodeLevel.SiteCollection => LoadArchiverSettingBySPObjectId(objectId, siteId, teamsId, groupId),
                    NodeLevel.Office365GroupEntire => LoadArchiverSettingBySPObjectId(objectId, Guid.Empty, teamsId, groupId),
                    NodeLevel.WebApplication => LoadArchiverSettingBySPObjectId(objectId, Guid.Empty, Guid.Empty, groupId),
                    _ => LoadArchiverSettingBySPObjectId(objectId, siteId, teamsId, groupId),
                };
            }
            else if (sourceType == ContentSourceType.SharePoint)
            {
                result = nodeLevel switch
                {
                    NodeLevel.SiteCollection => LoadArchiverSettingBySPObjectId(objectId, siteId, groupId),
                    NodeLevel.WebApplication => LoadArchiverSettingBySPObjectId(objectId, Guid.Empty, groupId),
                    _ => LoadArchiverSettingBySPObjectId(objectId, siteId, groupId),
                };
            }
            return result;
        }

        public async Task<bool> ForceSupportLockedSiteForBrokenChildNodesAsync(RMSPTreeNode node)
        {
            if (node is null || node.Parent is null)
                return false;
            if (node.Level != (int)NodeLevel.SiteCollection && node.Level != (int)NodeLevel.Office365GroupEntire)
            {
                return false;
            }
            var groupNode = node.GetGroupNode();
            var siteNode = node.GetSiteCollectionNode();
            var teamsNode = node.GetTeamsNode();
            var groupNodeId = groupNode != null ? new Guid(groupNode.SPObjectId) : Guid.Empty;
            var siteNodeId = siteNode != null ? new Guid(siteNode.SPObjectId) : Guid.Empty;
            var teamsNodeId = teamsNode != null ? new Guid(teamsNode.SPObjectId) : Guid.Empty;
            var contentSourceType = node.Type;
            var candidates = new List<Func<RMArchiverSetting>>();
            if (contentSourceType == ContentSourceType.Teams)
            {
                if (node.Level == (int)NodeLevel.SiteCollection)
                {
                    // Teams/Group level
                    candidates.Add(() => LoadArchiverSettingBySPObjectId(teamsNodeId, Guid.Empty, teamsNodeId, groupNodeId, ContentSourceType.Teams, NodeLevel.Office365GroupEntire));
                }
                // Container level
                candidates.Add(() => LoadArchiverSettingBySPObjectId(groupNodeId, Guid.Empty, Guid.Empty, groupNodeId, ContentSourceType.Teams, NodeLevel.WebApplication));
            }
            else if (contentSourceType == ContentSourceType.SharePoint)
            {
                // Container level
                candidates.Add(() => LoadArchiverSettingBySPObjectId(groupNodeId, Guid.Empty, Guid.Empty, groupNodeId, ContentSourceType.SharePoint, NodeLevel.WebApplication));
            }
            // Execute fallback chain
            RMArchiverSetting archiverSetting = null;
            foreach (var candidate in candidates)
            {
                archiverSetting = candidate();
                if (archiverSetting != null)
                {
                    if (node.Level == (int)NodeLevel.Office365GroupEntire)
                    {
                        siteNodeId = Guid.Empty;
                    }
                    break;
                }
            }
            if (archiverSetting is not null)
            {
                return await UpdateSupportLockedSiteAsync(siteNodeId, groupNodeId, teamsNodeId, contentSourceType, archiverSetting.SupportLockedSite) > 0;
            }
            return false;
        }

        private async Task<int> UpdateSupportLockedSiteAsync(Guid siteId, Guid siteGroupId, Guid teamsId, ContentSourceType sourceType, bool isSupportLockedSite)
        {
            using var context = GetNewContext();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                int result = 0;
                string sql = $"UPDATE [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].[RMArchiverSettings]" +
                    $" SET [SupportLockedSite] = @SupportLockedSite" +
                    $" WHERE [ContentSourceType] = @ContentSourceType AND [SiteGroupId] = @SiteGroupId AND [TeamsId] = @TeamsId";
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@SupportLockedSite", isSupportLockedSite));
                parameters.Add(new SqlParameter("@ContentSourceType", (int)sourceType));
                parameters.Add(new SqlParameter("@SiteGroupId", siteGroupId));
                parameters.Add(new SqlParameter("@TeamsId", teamsId));
                if (siteId != Guid.Empty)
                {
                    sql += $" AND [SiteId] = @SiteId";
                    parameters.Add(new SqlParameter("@SiteId", siteId));
                }
                else
                {
                    if (sourceType == ContentSourceType.Teams)
                    {
                        var excludedSiteIds = context.RMArchiverSettings.AsQueryable()
                            .Where(s => s.SPObjectId == s.SiteId && s.TeamsId == teamsId && s.ContentSourceType == (int)sourceType)
                            .Select(s => s.SiteId)
                            .ToList();
                        if (excludedSiteIds.Count > 0)
                        {
                            sql += $" AND [SiteId] NOT IN {DatabaseUtility.BuildInClause(excludedSiteIds)}";
                        }
                    }
                }
                result = await context.Database.ExecuteSqlCommandAsync(sql, parameters.ToArray());
                transaction.Commit();
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"UpdateSupportLockedSiteAsync error {ex}");
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    logger.Error($"Rollback failed: {rollbackEx}");
                }
            }
            return 0;
        }

        #endregion
    }
}
