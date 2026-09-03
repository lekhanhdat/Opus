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
using AvePoint.RA.Contract.Label;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Label;

namespace AvePoint.RA.DB.Dao
{
    public interface ITermDao : IBaseDao<RMTerm>
    {
        List<RMTerm> GetTermFromTermSet(int termSetId, int pageIndex, int pageCount);
        List<RMTerm> GetTermFromTermSet(int termSetId, bool containRemovedTerm = false);
        List<RMTerm> GetTermFromParentTermWithoutDeletedTerm(int parentTermId);

        List<RMTerm> GetActiveTermByTermSetIds(IEnumerable<int> termSetIds);

        List<RMTerm> GetActiveTermByTermSetId(int termsetId);

        List<RMTerm> GetActiveTermByParentId(int parentTermId);

        List<RMTerm> GetTermFromTermSetWithoutDeletedTerm(int termSetId);
        TermTreeNode GetRATermSetTree(Guid termSetId);
        List<RMTerm> GetTermFromParentTerm(int parentTermId, int pageIndex, int pageCount);
        List<RMTerm> GetTermFromParentTermForRuleUsageReport(RMTerm parentTerm);
        List<RMTerm> GetTermFromParentTerm(RMTerm parentTerm);
        int GetParentTermIdByPath(string path, int termSetId);
        bool CheckTermExist(int parentTermId, string termName, int termSetId, out int termId);
        bool CheckTermExistByLabelId(string labelId, Guid termGroupUniqueId, out int termId);
        RMTerm CreateTerm(TermInfo dto);
        RMTerm CreateGoogleTerm(TermInfo dto, RMGoogleLabelInfo labelInfo);
        RMTerm CreateTermForImport(string termName, int parentTermId, int termSetId, bool isDeprecated, Guid termUniqueId, string description = null);
        Task<RMTerm> UpdateTermAsync(string termName, int parentTermId, int termSetId, bool isDeprecated, Guid termUniqueId, string description = null);
        RMTerm UpdateTerm(string termName, int termId, int parentTermId, bool breakInherit, int termSetId, string description = null);
        RMTerm UpdateGoogleTerm(int termId, bool breakInherit, TermInfo newDto, RMGoogleLabelInfo labelInfo = null);
        void UpdateLabelState(Guid labelUniqueId, Contract.Label.State state);
        RMTerm UpdateTermForJPMC(string termName, int termId, int parentTermId, bool breakInherit, int termSetId, string advancedSettings = null);
        void DeleteAllTerm();
        RMTerm GetRMTermByTermId(int termId, bool needRetentionLable = true);
        RMTerm GetRMTermByUniqueId(Guid uniqueId, bool needCheckExpired = true);
        RMTerm GetRMTermByGuId(Guid id);
        List<RMTerm> GetRMTermsByLabelId(string labelId, bool includeRemoved = false);
        RMTerm GetRMTermByLabelId(string labelId, string tenantId, bool includeRemoved = false);
        bool TryGetGoogleLabelInfo(string uniqueTermId, out RMGoogleLabelInfo labelId, string tenantId, bool includeRemoved = false);
        Dictionary<Tuple<Guid, string>, RMGoogleLabelInfo> GetGoogleLabelInfos();
        RMTerm GetAvailableTermByGuId(Guid id);
        RMTerm GetRMTermWithPathByTermId(Guid termId, bool forExport = false);
        Task<RMGoogleLabelInfo> GetGoogleTermInfoByUniqueId(string uniqueId, string tenantId);

        List<int> GetAllTermIds();
        //此Mapping作用于全局缓存，为了提升效率。不要讲Dictionary 的value 改成大对象
        Dictionary<Guid, string> GetTermIdAndNameMapping();
        Dictionary<Guid, string> GetExistingTermIdAndNameMapping();
        /// <summary>
        /// no need 
        /// </summary>
        /// <param name="termId"></param>
        /// <param name="parentTermId"></param>
        RMTerm DeprecateTerm(int termId);
        int SubTermCount(int termId);
        List<RMTermGroup> GetRMTermsBySearch(string termLable, Guid termGroupId, bool withRuleName, FilterTermObjOption filterOption = null);
        List<RMTermGroup> GetRMTermsBySearch(string termLable, List<Guid> termGroupIds, bool withRuleName, FilterTermObjOption filterOption = null);
        Task<RMTerm> SaveTermSettingAsync(int termId, TermSettingsInfo settingInfo);
        Task DeleteTermAsync(int termId, List<Guid> deletedTermIds);
        Task<RMTerm> RenameTermAsync(int termId, string termName, int termSetId);
        RMTerm InheritSettingToParent(int termId, TermSettingsInfo settingInfo);
        int SubTermCountByTermSetId(int termSetId);
        bool ParentTermHasSetting(int termId);
        RMTerm GetParentInhertSetting(int termId);
        RMTerm GetTermTimeSettings(int termId);
        void GetAllInheritTermsByRootTerm(int TermId, ref List<RMTerm> Terms, long timePoint = 0);
        List<Guid> GetAllSubTermUniqueIds(Guid termId);
        List<RMTerm> GetRMTermsByTermIds(int[] termsIds);
        List<RMTerm> GetRMTermsByTermIds(List<Guid> termsIds);
        RMTerm GetParentTermTimeSettings(int termId);
        /// <summary>
        /// 获取termId full path,从TermSetId开始 eg:1/2/3
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        string GetTermIdPath(Guid termId);

        /// <summary>
        /// 获取Term Name full path, 从TermGroup开始，eg: TermGroup/TermSet/Term/Term1
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        string GetTermNamePath(int termId, bool forExport = false);
        string GetTermNamesPathByTermId(Guid termId);
        string GetTermSetNamesPathByTermSetId(int termSetId);
        string GetTermSetNamesPathByTermSetId(Guid termSetId);
        string GetTermNameByTermId(int termId);
        string GetTermFullPathForDestroyReport(Guid termId);
        string GetTermFullPathByTermId(Guid termId);
        string GetTimeZoneNameById(string timeZoneId);
        RMTerm SetTermIsExpired(RMTerm term, RMTerm subTerm);
        bool IsExpiredTerm(int termId);
        string GetTermGroupNameById(int groupId);
        string GetTermSetNameById(int termSetId);
        List<RMTerm> GetOrphanedTerms(int termSetId);

        List<RMTerm> GetOprhanedTerms();

        List<RMTerm> GetretiredTerms(int termSetId);

        List<RMTerm> GetRetiredTerms();

        TermTreeNode GetRATermSetTreeOfOrphanedTerm(Guid termSetId);
        List<RMTerm> GetAllTerms(int termSetId = 1);
        List<RMTerm> GetAllTerms(List<int> termSeIds);
        List<Guid> GetDeletedLableUniqueIds(string tenantId, Guid termGroupUniqueId, List<string> availableLabelIds);
        List<RMTerm> GetAllSubLocationTerm(int id);
        void DeleteTermByTermSetId(int termId);
        RMTerm EnableTerm(int termId);
        bool GetTermPermanentByTermId(int termId, bool onlyParent);//TODO
        /// <summary>
        /// get retention setting
        /// </summary>
        /// <param name="enforceRetention">0: disable 1:enable(block delete)</param>
        /// <returns></returns>
        Dictionary<Guid, TermSettingsInfo> GetRetetionTermDic(List<Guid> termIds);
        RMTerm GetParentInhertSetting(Guid termId);
        TermTreeNode GetSubTermTreeNode(RMTerm term, Guid parentId);
        List<Guid> GetAllValidEnforceRetentionTermIds();
        List<RMTerm> GetAllTermsForce();
        List<RMTerm> GetAllNotRemoveTermsForce();
        List<RMTermSet> GetAllTermSetsForce();
        List<RMTermSetMembership> GetAllTermSetMemberShipsForce();
        List<RMTerm> FSGetAllTermsUnderTermSet(int id);
        List<Guid> GetAllSubTermUniqueIdsByTermSetId(Guid termSetId);
        List<Guid> GetAllSubTermUniqueIdsByTermId(Guid termId);
        List<RMTerm> GetTermFromParentId(int parentTermId);
        Dictionary<Guid, string> GetTermUniqueIdAndNameMapping();

        List<Guid> GetTermSetIdListByTermIds(List<int> termIds);

        RMTerm GetActiveTermById(int termId);
        Dictionary<int, string> GetTermFullPathByTermIds(List<int> termIds);
        List<RMTerm> GetWillTrainingTerms(string termLable, int pageIndex, int pageSize, out int totalCount, FilterTermObjOption filterOption = null);

        List<string> GetSettingDefaultTermNames(List<Guid> termIds);
        List<RMTerm> GetAllTermHasAdvanceSettingsTerms();
        RMTermSet GetRMTermSetByGuid(Guid id);
        List<RMTerm> GetTermByTermGroupIdIncludeTermRemoved(Guid termGroupId);
        Task<List<RMTermGroup>> GetPaginatedTermsStructureAsync(string nodeId, int pageIndex, int pageCount,List<Guid> groupIds, List<string> userAndGroupIds, string searchKey = null);
        string GetTermGroupUniqueIdByTermId(int termId);
        RMTerm GetTermByNameAndScopeId(string termName, Guid scopeId);
        Task<bool> CheckTermExistGoogleLabelInfor(List<Guid> scopes, Guid termUniqueId);
        Task<Dictionary<Guid, string>> GetAllDeletedTermAndLabelByTenantId(string tenantId);
        Task<List<int>> GetActiveTermSets(List<int> termSetIds);
        List<RMTerm> GetActiveTermsByTermSetId(int termsetId);
        bool CheckTermDeletedByIds(List<Guid> termsIds);
        List<RMTerm> SearchTermWithLimit(string searchValue, int limit);
        Task<List<RMTerm>> SearchLabelWithLimit(string searchValue, int limit);
        Task<List<RMTerm>> GetTermFromTermSetUniqueId (Guid termSetUniqueId);
        Task<IEnumerable<RMTerm>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertTermTableAsync(IEnumerable<RMTerm> terms);
        Task<long> MultiGeoDeleteAllTermAsync();

        Task<RMTerm> GetTermFromTermSetUniqueIdAndName(Guid termSetId, string termName);
        Task<RMTermSet> GetTermSetFromTermUniqueId(Guid termUniqueId);
    }
}
