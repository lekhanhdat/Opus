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
    public interface IEXOSettingDao: IBaseDao<RMExchangeOnlineSetting>
    {
        List<RMExchangeOnlineSetting> LoadRunJobSetting();//Get Node have not config before.
        List<RMExchangeOnlineSetting> LoadAllSettingForAS();
        List<RMExchangeOnlineSetting> LoadAllSettingForDS();
        List<RMExchangeOnlineSetting> LoadAllSetting();
        List<RMExchangeOnlineSetting> LoadExcludeSiteCollectionSetting();
        Task SetSettingInfoAsync(Guid scopeId, long timeTicks, bool runAutoFullJob);
        Task SetSettingInfoAsync(Guid groupId, Guid scopeId, long timeTicks, bool runAutoFullJob);
        Task<bool> CleanSettingJobTimeAsync(RMEXOTreeNode node);
        Task AddOrUpdateGlobalSettingAsync(RMEXOTreeNode node);
        Task AddOrUpdateCustomSettingAsync(RMEXOTreeNode node, Guid siteId);
        RMExchangeOnlineSetting LoadSharePointSetting(Guid id, Guid siteId, bool includeOnlySetPhysicalNode = false);
        RMExchangeOnlineSetting LoadExchangeOnlineSetting(Guid currentNodeId, Guid parentId);
        Task DeleteSharePointSettingAsync(Guid id, Guid siteId);
        List<RMExchangeOnlineSetting> GetColumnInfos(string[] ids);
        List<RMExchangeOnlineSetting> LoadExchangeOnlineGroupSetting();
        RMExchangeOnlineSetting GetSettingInfoByAgentGroupId(string id);

        List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetReocrdOwnersBySettingId(int settingId);
        List<RecordOwnerGroupDto> GetRecordOwners(HashSet<Guid> groupIds, HashSet<Guid> siteIds);
        void UpdateRecordOwnerUserPrincipalName(RecordOwnerDto owner);
        void MarkRemovedSharePointSetting(Guid scopeId);
        Task MarkRemovedSharePointSettingUnderCurrentAsync(Expression<Func<RMExchangeOnlineSetting, bool>> lambda);
        Task AddOrUpdateGlobalSettingUsingExistColumnAsync(RMEXOTreeNode node, bool isNewEditd = false);
        RMExchangeOnlineSetting GetSettingInfoByScope(Guid groupId, Guid siteId, Guid scopeId);
        Task AddOrUpdateCustomSettingAsync(RMExchangeOnlineSetting setting);
        void UpdateRunningJobStartStatus(List<int> spIds, string startJobId);
        void UpdateRunningJobFinishStatus(List<int> spIds);
        bool CheckJobIsSkip();
        void FlagCustomSettingNewColumn(Guid siteGroupId);
        Dictionary<Guid, int> GetDisableDocClassification();
        void RemoveDescendantsSetting(RMEXOTreeNode node, string profileIdPath);
        List<RMExchangeOnlineSetting> GetDescendantsDisableNodes(RMEXOTreeNode node);
        List<RMExchangeOnlineSetting> GetDescendantsBreakNodesForNullClassification(RMEXOTreeNode node);
        List<RMExchangeOnlineSetting> GetAllSettingsForGroup(RMEXOTreeNode current);
        List<RMExchangeOnlineSetting> LoadAllGroupSettings();
    }
}
