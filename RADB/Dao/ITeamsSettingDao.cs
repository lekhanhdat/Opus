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
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao
{
    public interface ITeamsSettingDao : IBaseDao<RMTeamsSetting>
    {
        List<RMTeamsSetting> LoadGroupSetting(bool isRecheckRule = true);
        RMTeamsSetting LoadTeamsSetting(Guid id, Guid teamsId, Guid siteId, bool includeOnlySetPhysicalNode = false);
        RMTeamsSetting LoadClosestContainerSetting(RMSPTreeNode treeNode, Guid containerId, Guid teamsId, Guid siteId);
        RMTeamsSetting LoadChannalSetting(Guid teamsId, Guid siteId);
        List<RMTeamsSetting> LoadTeamsSettings(List<Guid> id, List<Guid> teamsId);

        List<RMTeamsSetting> LoadTeamsSettings(Guid groupId, bool includeOnlySetPhysicalNode = false);

        List<RMTeamsSetting> LoadSettingsUnderTeams(Guid groupId, List<Guid> teamId);
        
        List<RMTeamsSetting> LoadSettingsUnderSite(Guid groupId, Guid teamId, Guid siteId);

        RMTeamsSetting GetParentNode(Expression<Func<RMTeamsSetting, bool>> whereLambda);
        Task<bool> CleanSettingJobTimeAsync(RMSPTreeNode node);
        void UpdateBCSColumnName(Guid groupId, string bcsColumnName, string bcsColumnDescription, bool columnRequired = true, bool columnHidden = false);
        Task AddOrUpdateGlobalSettingAsync(RMSPTreeNode node);

        Task<List<RMTeamsSetting>> AddTeamsSettingAsync(List<RMSharePointSetting> spSettings, Guid teamsId);

        Task AddOrUpdateCustomSettingAsync(RMSPTreeNode node, Guid teamsId, Guid siteId);
        void FlagCustomSettingNewColumn(Guid siteGroupId);
        List<RMTeamsSetting> GetColumnInfos(string[] ids);
        Task AddOrUpdateGlobalSettingUsingExistColumnAsync(RMSPTreeNode node, bool isNewEditd = false);
        Task DeleteTeamsSettingAsync(Guid id, Guid teamsId, Guid siteId);
        void RemoveDescendantsSetting(RMSPTreeNode node, string profileIdPath);
        RMTeamsSetting GetSettingInfoByAgentGroupId(string id);
        List<RMTeamsSetting> LoadRunJobSetting();
        List<RMTeamsSetting> LoadAllSetting();
        RMTeamsSetting GetSettingInfoByScope(Guid groupId, Guid teamsId, Guid siteId, Guid scopeId);
        List<RMTeamsSetting> LoadExcludeTeamsSetting();
        Task SetSettingJobTimeWithGroupIdAsync(Guid groupId, Guid scopeId, bool isFailedConfigColumn, bool isFailedConfigProperty);
        RMTeamsSetting LoadTeamsSettingForImportSetting(Guid teamsId, Guid scopeId);
        List<RMTeamsSetting> LoadShowUniqueIdSetting();
        Task SetSettingJobTimeAsync(Guid scopeId,Guid teamsId, Guid siteId, bool isFailedColumn, bool isFailedProperty);
        List<RMTeamsSetting> GetFolderSettingUnderList(Guid listId, Guid siteId, Guid teamsId);
        string GetMedataColumn(Guid nodeId);
        List<RMTeamsSetting> GetAllGroupSettings();
        bool GetSettingEnableInfoByScope(Guid groupId, Guid teamId, Guid siteId, Guid scopeId);
        bool ExistShowUniqueIdSetting();
        List<RMTeamsSetting> GetDescendantsDisableNodes(RMSPTreeNode node);
        List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetRecordOwnersBySettingId(int settingId);
        bool CheckGroupSettingExist(List<string> groupIds);
        Dictionary<Guid, string> GetSiteCollectionIdAndUrlAsync(IEnumerable<string> siteCollectionIds);
        List<RMTeamsSetting> GetAllSettings();
        bool CheckHasInheritChanged(Guid groupId, Guid teamsId);
        bool CheckHasInheritChangedUnderGroup(Guid groupId);
        bool CheckGroupHasInheritChanged(Guid groupId);
        int UpdateChangedInheritOptionFlag(Guid groupId, Guid teamsId);
    }
}
