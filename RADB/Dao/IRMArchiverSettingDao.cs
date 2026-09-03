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
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RMSPTreeNode = AvePoint.RA.Contract.Object.RMSPTreeNode;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMArchiverSettingDao:IBaseDao<RMArchiverSetting>
    {
        RMArchiverSetting LoadArchiverSetting(Guid id, Guid siteId);
        RMArchiverSetting LoadArchiverSettingBySPObjectId(Guid objectId, Guid siteId, Guid siteGroupId);
        List<RMArchiverSetting> LoadArchiverSettings();
        Task<RAReturnMessage> SaveArchiverSettingAsync(RMSPTreeNode node);
        Task<RAReturnMessage> SaveOrUpdateGeneralSettingAsync(RMSPTreeNode node);
        Task SaveMigratedArchiverSettingAsync(ArchiverMigrationRuleSetting amRuleSetting, List<RMExchangeOnlineSettingRuleMapping> ruleMappings);
        Task SaveMigratedDisabledArchiverSettingAsync(ArchiverMigrationRuleSetting amRuleSetting);
        void DeleteArchiverSetting(Guid id, Guid siteId);
        void DeleteArchiverSetting(Guid id, Guid teamsId, Guid siteId);
        void DeleteArchiverSettingByContentSourceType(Guid id, Guid siteId, Guid teamsId = default, ContentSourceType type = ContentSourceType.SharePoint);
        /// <summary>
        /// 只有在 Cloud Archiver Migration Job里可以调用此方法来清理历史数据
        /// </summary>
        Task<int> DeleteMigratedArchiverSettings();
        /// <summary>
        /// 只有在 Cloud Archiver Migration Job里可以调用此方法来清理历史数据
        /// </summary>
        Task<int> DeleteMigratedRuleMappings();
        /// <summary>
        /// 只有在 Cloud Archiver Migration Job里可以调用此方法来清理历史数据
        /// </summary>
        Task<int> DeleteMigratedSchedulesAsync();
        Task<bool> CleanSettingJobTimeAsync(RMSPTreeNode node);
        List<RMArchiverSetting> LoadArchiverSettingsUnderGroup(Guid groupId, ContentSourceType contentSourceType = ContentSourceType.SharePoint);
        List<RMArchiverSetting> LoadArchiverSettingsUnderSite(Guid siteId, ContentSourceType sourceType = ContentSourceType.SharePoint);
        List<RMArchiverSetting> LoadArchiverSettingsUnderTeams(Guid teamsId, bool isOnlySiteSetting = false);
        List<RMArchiverSetting> LoadArchiverSettingsUnderTeamsIds(IEnumerable<Guid> teamsIds, bool isOnlySiteSetting = false);
        List<string> LoadBreakInheritNodeUrls(string scopeUrl, string siteObjectId = "", bool isTeamsContentSource = false);
        RMArchiverSetting LoadSiteArchiverSettingByUrl(string siteUrl, bool isTeamsContentSource = false);
        RMArchiverSetting LoadCurrentNodeArchiverSettingByUrl(string siteUrl, ContentSourceType sourceType = ContentSourceType.SharePoint);
        RMArchiverSetting LoadTeamsArchiverSetting(Guid id, Guid siteId, Guid teamsId);
        RMArchiverSetting LoadArchiverSettingByContentSource(Guid id, Guid siteId, Guid teamsId = default, ContentSourceType type = ContentSourceType.SharePoint);

        List<RMArchiverSetting> LoadArchiverSettingBySiteGroupIds(List<Guid> siteGroupIds, ContentSourceType type = ContentSourceType.SharePoint);

        int LoadArchiverSettingCountBySiteGroupIds(List<Guid> siteGroupIds, ContentSourceType type = ContentSourceType.SharePoint);

        int UpgradeTeamsSettings(List<RMArchiverSetting> archiverSettings);

        RMArchiverSetting LoadChannelArchiverSetting(Guid scopeId, string id);
        List<RMArchiverSetting> LoadAllArchiverSettingWithType(ContentSourceType type);
        Task<bool> ForceSupportLockedSiteForBrokenChildNodesAsync(RMSPTreeNode node);
    }
}
