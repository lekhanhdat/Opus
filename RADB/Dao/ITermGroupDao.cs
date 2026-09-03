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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Google.Model;
using AvePoint.RA.DB.Core;
using AvePoint.RA.Contract.TaxonomyModel;

namespace AvePoint.RA.DB.Dao
{
    public interface ITermGroupDao : IBaseDao<RMTermGroup>
    {
        void AddTermGroupInfo(string groupName, string groupDescription);

        RMTermGroup GetRMTermGruop(int termGroupId);
        List<RMTermGroup> LoadGroupsData(bool containTerms = true, List<Guid> groupUniqueIds = null, List<string> userAndGroupUserIds = null, FilterTermObjOption filterOption = null, int pageIndex = 0, int pageSize = 0);

        Task<RMTermGroup> LoadGroupsData(Guid termGroupId);
        public Task<List<RMTermGroup>> LoadGoogleGroupsData(List<Guid> groupIds, List<string> userAndGroupIds, FilterTermObjOption filterOption, int pageIndex, int pageSize);
        public Task<List<RMTermGroup>> LoadGoogleGroupsData(List<Guid> groupIds);
        RMTermGroup LoadTermDataById(Guid termGroupId, bool isBussiness = false, FilterTermObjOption filterOption = null);
        List<RMTermGroup> LoadTermGroup(bool isWithDelGroup = true, FilterTermObjOption filterOption = null);
        List<RMTermGroup> LoadSPTermGroup();
        Task<RMTermGroup> UpdateTermGroupAsync(int termGroupId, string termGroupName, string description, bool usingMMSSpecified, int m365SyncOption, int googleSyncOption);
        Task UpdateGoogleTermGroupSettingAsync(RMGoogleTermGroupSetting setting);
        Task<RMTermGroup> UpdateTermGroupAsync(int termGroupId, string termGroupName, string description);
        bool HasSameNameTermGroup(string termGroupName);
        bool ReNameHasSameNameTermGroup(int termGroupId, string termGroupName);
        void CreateTermGroupById(Guid termGroupId, string termGroupName, string description, bool usingMMSSpecified);
        RMTermGroup CreateTermGroupByName(string name);

        RMTermGroup GetTermGroupById(int id);
        RMTermGroup GetTermGroupByUniqueIdForGoogleOne(Guid uniqueId);

        List<RMTermGroup> GetTermGroupsByIds(IEnumerable<int> ids);

        RMTermGroup GetTermGroupByGuid(Guid termGroupId);
        RMTermGroup GetTermGroupByName(string termGroupName);
        void DeleteTermGroup();
        Task<RMTermGroup> RenameTermGroupAsync(int termGroupId, string termGroupName);
        List<RMTermGroup> LoadLocationData();
        List<RMTermSet> LoadLocationSet();
        Task DeleteTermGroupAsync(Guid termGroupId);
        List<RMTermGroup> GetTermGroups(PagerInfo pager, out int totalCount);
        List<string> GetFarmIdsBySpecificSites();
        bool IsExistNeedSyncTermGroup(SiteType siteType);
        bool IsExistNeedSyncTermGroupGoogle();
        List<Guid> GetTermGroupIdsByFarmId(string farmId);
        List<RMTermGroup> LoadNeedSyncTermGroups(List<SiteType> siteType);
        Task<Dictionary<string, string>> GetAllTermGroups();
        Task<Dictionary<string, string>> GetAllTermGroupsByMultipleNodes(RMClassificationGroupMultipleNodes nodes);
        Task<List<string>> GetSpecifiedGoogleTenants(Guid termGroupId);
        Task<Dictionary<string,List<string>>> GetTermGroupNameAndGoogleTenant(Guid termGroupId);
        Task<RMTermGroup> GetTermGroupTreeDataAsync(RMDbContext context, string tenantId, int pageIndex, int pageCount, List<Guid> groupIds, List<string> userAndGroupIds, string searchKey = null);
        Task<bool> CheckIsAllTermGroupsBothNoneOption();
        Task<string> GetTermGroupIdByTermUniqueId(Guid termUniqueId);
        Task<IEnumerable<RMTermGroup>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertTermGroupTableAsync(IEnumerable<RMTermGroup> termGroups);
        Task<long> MultiGeoDeleteAllTermGroupAsync();
    }
}
