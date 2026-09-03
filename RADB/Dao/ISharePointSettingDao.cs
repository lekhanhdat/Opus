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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface ISharePointSettingDao : IBaseDao<RMSharePointSetting>
    {
        List<RMSharePointSetting> LoadRunJobSetting();//Get Node have not config before.
        List<RMSharePointSetting> LoadAllSetting();
        IEnumerable<RMSharePointSetting> LoadSyncDataSettings(int batchSize = 100);
        List<RMSharePointSetting> LoadExcludeSiteCollectionSetting();
        List<RMSharePointSetting> LoadShowUniqueIdSetting();
        List<RMSharePointSetting> LoadGroupSetting(bool isRecheckRule = true);
        Task SetSettingJobTimeWithGroupIdAsync(Guid groupId,Guid scopeId, bool isFailedConfigColumn, bool isFailedConfigProperty);
        Task SetSettingJobTimeAsync(Guid scopeId, bool isFailedConfigColumn, bool isFailedConfigProperty);
        Task SetSettingJobTimeAsync(Guid scopeId, Guid siteId, bool isFailedColumn, bool isFailedProperty);
        Task<bool> CleanSettingJobTimeAsync(RMSPTreeNode node);
        Task AddOrUpdateGlobalSettingAsync(RMSPTreeNode node);
        Task AddOrUpdateCustomSettingAsync(RMSPTreeNode node, Guid siteId);

        void UpdateBCSColumnName(Guid groupId, string bcsColumnName, string bcsColumnDescription, bool columnRequired = true,bool columnHidden=false);

        //Guid UpdateGlobalSetting(RMSPTreeNode node);
        List<RMSharePointSetting> LoadSharePointSettings(Guid groupId, bool includeOnlySetPhysicalNode = false);
        RMSharePointSetting LoadSharePointSetting(Guid id,Guid siteId,bool includeOnlySetPhysicalNode = false);

        RMSharePointSetting LoadChannelSetting(Guid scopeId, int id);
        Task DeleteSharePointSettingAsync(Guid id,Guid siteId);
        string GetMedataColumn();
        string GetMedataColumn(Guid nodeId);
        List<RMSharePointSetting> GetAllPhysicalSiteSettings();
        //void AddOrUpdateColumnInfo(Guid siteId, Guid webId, Guid listId, Guid fieldId, RMSPTreeNode node);
        //Guid UpdateColumnInfo(Guid siteId, Guid webId, Guid listId, Guid fieldId, RMSPTreeNode node);
        //Guid GetSiteColumnId(Guid siteId);
        //Guid GetListColumnId(Guid siteId,Guid webId, Guid listId);
       // void DeleteCustomSetting(Guid siteId, Guid webId, Guid listId);//for save column logic 
        //RMSharePointSetting GetSiteColumnInfo(Guid siteId);
        List<RMSharePointSetting> GetColumnInfos(string[] ids);
       // RMSharePointSetting GetListClassificationSetting(Guid siteId, Guid webId, Guid listId);

        RMSharePointSetting GetGroupLevelGlobalSetting(string groupName, Guid scopeId);
        RMSharePointSetting GetSiteLevelSetting(string fullPath, Guid scopeId);
        //void AddOrUpdateGlobalSettingUsingExistColumn(RMSPTreeNode node);
        Task DeleteSharePointSettingBySiteIdAsync(Guid siteId);
        bool IsUsingExistingColumnByGroupIds(List<Guid> ids);
        RMSharePointSetting GetSettingInfoByAgentGroupId(string id);

        List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetReocrdOwnersBySettingId(int settingId);

        List<RecordOwnerGroupDto> GetRecordOwners(List<Guid> groupIds, List<Guid> siteIds);
        List<RecordOwnerGroupDto> GetRecordOwnersForEXO(List<Guid> groupIds, List<Guid> siteIds);
        List<RecordOwnerGroupDto> GetRecordOwnersForSPLocal(List<Guid> groupIds, List<Guid> siteIds);
        List<RecordOwnerGroupDto> GetRecordOwnersForOneDrive(List<Guid> groupIds, List<Guid> siteIds);
        void UpdateRecordOwnerUserPrincipalName(RecordOwnerDto owner);

        List<RMSharePointSetting> GetAllSettingsForLevel(RMSPTreeNode current, NodeLevel level);

        void MarkRemovedSharePointSetting(Guid scopeId);

        Task MarkRemovedSharePointSettingUnderCurrentAsync(Expression<Func<RMSharePointSetting, bool>> lambda);




        Task AddOrUpdateGlobalSettingUsingExistColumnAsync(RMSPTreeNode node, bool isNewEditd = false);
        RMSharePointSetting GetSettingInfoByScope(Guid groupId, Guid siteId, Guid scopeId);
        Task AddOrUpdateCustomSettingAsync(RMSharePointSetting spSetting);

        Task UpdateRunningJobStartStatusAsync(List<int> spIds, string startJobId);
        Task UpdateRunningJobFinishStatusAsync(List<int> spIds);
        bool CheckJobIsSkip();
        void FlagCustomSettingNewColumn(Guid siteGroupId);
        bool ExistShowUniqueIdSetting();
        Dictionary<Guid, int> GetDisableDocClassification();
        void RemoveDescendantsSetting(RMSPTreeNode node, string profileIdPath);
        List<RMSharePointSetting> GetDescendantsDisableNodes(RMSPTreeNode node);

        RMSharePointSetting GetParentNode(Expression<Func<RMSharePointSetting, bool>> whereLambda);
        bool GetSettingEnableInfoByScope(Guid groupId, Guid siteId, Guid scopeId);
        List<RMSharePointSetting> GetDescendantsFolderBreakNodes(RMSPTreeNode node);
        RMSharePointSetting GetParentLibraryCustomSetting(Guid listId);
        List<RMSharePointSetting> GetFolderSettingUnderList(Guid listId, Guid siteId);
        List<RMSharePointSetting> GetAllGroupSettings();
        bool ChickGroupSettingExist(List<string> groupIds);

        RMSharePointSetting LoadSharePointSettingForImportSetting(Guid siteId, Guid scopeId);
        RMSharePointSetting LoadSharePointSetting(string fullPath);
        RMSharePointSetting LoadContainerSharePointSettingByContainerName(string containerName);
        List<RMSharePointSetting> LoadSPSettingsUnderSite(Guid siteId);

        RMSharePointSetting LoadSPSiteSettingEnableManualApprovalFirst();

        List<RMSharePointSetting> GetAllSettingsBySiteGroupIds(List<Guid> siteGroupId);

        int GetSettingsCountBySiteGroupIds(List<Guid> siteGroupIds);

        List<RMSharePointSetting> GetAllSettingsByScopeIds(List<Guid> scopeIds);

        RMSharePointSetting LoadClosestContainerSetting(RMSPTreeNode treeNode, Guid containerId, Guid siteId);

        bool CheckHasInheritChanged(Guid groupId);

        bool CheckHasInheritChanged(Guid groupId, Guid siteId);
        bool UpdateChangedInheritOptionFlag(Guid groupId, Guid siteId);
        int UpdateChangedInheritOptionFlag(Guid groupId);
    }
}
