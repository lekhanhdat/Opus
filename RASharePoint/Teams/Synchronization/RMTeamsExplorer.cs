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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.ExplorerSync;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using AvePoint.RA.SharePoint.ExplorerSync.Modes;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.SharePoint.ExplorerSyncNew.Modes;

namespace AvePoint.RA.SharePoint.Teams.Synchronization
{
    public class RMTeamsExplorer : RMSPExplorerBase
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RMTeamsExplorer));
        protected override NodeFlagType nodeFlagType => NodeFlagType.TeamsSyncLibrary;

        public RMTeamsExplorer(AveDiscoverSite discoverSite, SPTreeNodeDto treeNode, JobContext jobContext) : base(discoverSite, treeNode, jobContext)
        {
            var siteId = DiscoverSite.SiteID.ToString();
            _siteCache = RMSPExplorerDataCache.Instance.SiteLevelCache[siteId];
            _ = s_keyValueDao.TryGetBoolValue(KeyNameCollection.IsEnableCustomIndexMetadata, out var isEnable);
            if (isEnable)
            {
                CustomIndexMetadatas = s_customIndexMetadataDao.GetCustomIndexMetadatasBySourceFlagAsync(AvePoint.RA.Contract.Explorer.SourceFlag.SharePoint).GetAwaiter().GetResult().ToList();
                CustomMetadataColumns = s_customMetadataColumnDao.GetAllCustomMetadataColumnsAsync().GetAwaiter().GetResult().ToList();
            }
            syncItem = new RMTeamsSyncItem(_siteCache, CustomIndexMetadatas, CustomMetadataColumns);
            var groupTreeNode = SPTreeNodeManagement.GetGroupNode(treeNode);
            var teamsGroupSetting = s_teamsSettingDao.GetSettingInfoByScope(new Guid(containerId), Guid.Empty, Guid.Empty, new Guid(groupTreeNode.SPObjectId));
            var teamsSiteSetting = s_teamsSettingDao.GetSettingInfoByScope(new Guid(containerId), new Guid(treeNode.TeamsId), new Guid(treeNode.ID), new Guid(treeNode.SPObjectId));
            var teamsSetting = s_teamsSettingDao.GetSettingInfoByScope(new Guid(containerId), new Guid(treeNode.TeamsId), Guid.Empty, new Guid(treeNode.TeamsId));
            SiteSetting = RMLifecycleSetting.FromTeamsSetting(teamsSiteSetting);
            GroupSetting = RMLifecycleSetting.FromTeamsSetting(teamsGroupSetting);
            TeamsSetting = RMLifecycleSetting.FromTeamsSetting(teamsSetting);
            var siteSettings = RMLifecycleSetting.FromTeamsSetting(s_teamsSettingDao.LoadSettingsUnderSite(new Guid(containerId), new Guid(treeNode.TeamsId), new Guid(treeNode.SPObjectId)));
            IsEnableInheritParentTerm = (GroupSetting?.IsInheritParentTerm ?? false)
                                    || (SiteSetting != null && SiteSetting.IsInheritParentTerm)
                                    || (TeamsSetting != null && TeamsSetting.IsInheritParentTerm)
                                    || siteSettings.Any(s => s.IsInheritParentTerm);
            _currentSiteSettings = siteSettings.Where(s => s.FolderId == Guid.Empty && s.WebId != Guid.Empty).OrderByDescending(s => s.FullPath).ToList();

            currentTeamsId = treeNode.TeamsId;
            containerId = groupTreeNode.ID;
            currentSiteId = treeNode.ID;
        }

        protected override void AddFailureItem2Cache(IAveListItem aveItem, Guid parentId, Exception e)
        {
            if (FailureItems.Count <= 1000)
            {
                RMSPSyncFailureItem failureItem = new RMSPSyncFailureItem()
                {
                    TeamsId = _siteCache.TeamsId.ToString(),
                    SiteId = DiscoverSite.SiteID.ToString(),
                    ListId = aveItem.ParentList.ID.ToString(),
                    IntemIntId = aveItem.ID,
                    JobId = JobContext.SubJobId,
                    ItemId = aveItem.UniqueId.ToString(),
                    ParentId = parentId.ToString(),
                    WebId = aveItem.ParentList.ParentWeb.ID.ToString()
                };
                failureItem.URL = aveItem?.Url;
                failureItem.ObjectName = aveItem?.Name;
                failureItem.Message = GetExceptionMessage(e);
                FailureItems.Add(failureItem);
            }
        }

        protected override void AddFailureItem2Cache(Record record, Exception e)
        {
            if (FailureItems.Count <= 1000)
            {
                RMSPSyncFailureItem failureItem = new RMSPSyncFailureItem()
                {
                    TeamsId = _siteCache.TeamsId.ToString(),
                    SiteId = record.ScopeId.ToString(),
                    ListId = record.ListId.ToString(),
                    IntemIntId = record.ItemRowId,
                    JobId = JobContext.SubJobId,
                    ItemId = record.ItemId.ToString(),
                    ParentId = record.FolderId.ToString(),
                    WebId = record.WebId.ToString(),
                    URL = record.FullPath,
                    ObjectName = record.LeafName,
                    Message = e?.Message
                };
                FailureItems.Add(failureItem);
            }
        }

        protected override void AddFailureItem2Azure()
        {
            try
            {
                if (FailureItems.Count > 0)
                {
                    List<SyncFailureItemEntity> failureEntities = new List<SyncFailureItemEntity>();
                    foreach (RMSPSyncFailureItem item in FailureItems)
                    {
                        SyncFailureItemEntity entity = new SyncFailureItemEntity(item.SiteId, item.ItemId);
                        entity.TeamsId = item.TeamsId;
                        entity.ListId = item.ListId;
                        entity.JobId = item.JobId;
                        entity.ParentId = item.ParentId;
                        entity.WebId = item.WebId;
                        entity.ItemId = item.IntemIntId;
                        entity.FullPath = item.URL;
                        failureEntities.Add(entity);
                    }
                    logger.Debug($"Add entity to azure, list count: {failureEntities.Count}");
                    SyncFailureItemDao.Add(TenantLocalValue.LogonGroupId, failureEntities);
                }
            }
            catch (Exception e)
            {
                JobContext.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                logger.Error(e.Message, e);
            }
        }
    }
}
