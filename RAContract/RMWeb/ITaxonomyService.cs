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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Label;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.TaxonomyModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface ITaxonomyService
    {
        Task<string> GetTaxonomyTreeDataAsync(string typeName, string treeNodeId, int pageIndex, int pageCount, List<RMSPTreeNode> spTreeNodes, int SettingType, FilterTermObjOption filterOption);
        public Task<string> GetTaxonomyGoogleTermTreeDataAsync(FilterTermObjOption filterOption, int pageIndex, int pageCount);
        public Task<string> GetTaxonomyAllGoogleTermTreeDataAsync(FilterTermObjOption filterOption, int pageIndex, int pageCount);
        public Task<string> GetTaxonomyGoogleTermTreeApplySettingDataAsync(string nodeId, int pageIndex, int pageCount, string searchKey);
        Task<string> CreateTermAsync(TermInfo dto);
        Task<string> CreateTermGroupAsync(string termGroupName);
        Task<string> CreateTermGroupAsync(TermInfo termInfo);
        Task<string> CreateTermSetAsync(string termSetName, Guid termGroupId);
        Task<string> CreateTermSetAsync(TermInfo termInfo);
        Task<string> SearchAsync(int termSetId, string termLabel, Guid termGroupId, bool withRuleName = false);
        Task<string> SearchAsync(int termSetId, string termLabel, List<Guid> termGroupIds, bool withRuleName = false);
        Task<string> SearchAsync(int termSetId, string termLabel, Guid termGroupId, string containerId, SourceFlag sourceFlag);
        string DeprecateTerm(int termId);
        Task<string> RenameTermAsync(int termId, string termName, int termSetId);
        Task<string> GetSPSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false);
        Task<string> GetOneDriveSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false);
        Task<string> GetTeamsSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false);
        Task<string> DeleteTermAsync(int termId);
        string GetTermByTermId(string termId);
        string GetTermSetByTermSetId(string termSetId);
        string GetTermSetDescByTermSetId(string termSetId);
        Task<string> GetTaxonomyTreeDataAsync(string typeName, string treeNodeId, bool fetchDeprecated = true, bool needCheckPermission = false);
        Task<string> GetTaxonomyTreeDataAsync(string typeName, string treeNodeId, FilterTermObjOption filterTermObjOption, bool fetchDeprecated = true);
        TermGroupAuditInfo GetTermGroupInfoById(int termGroupId);
        Task<string> SaveTermSettingInheritToParentAsync(int termId, TermSettingsInfo termInfo);
        Task<string> SaveTermSettingAsync(TermSettingsInfo termRuleInfo);
        Task<string> UpdateTermSetAsync(int termSetId, string termSetName, string des);
        Task<RAReturnMessage> UpdateTermGroupAsync(int termGroupId, string termGroupName, string des, List<RMSiteInfo> siteInfo, bool UsingMMSDefault, int m365SyncOption, int googleSyncOption);
        Task<string> GetParentInhertSettingAsync(int termId);
        Task<string> GetTermSettingWithGoogleRuleAsync(int termId);
        Task<string> GetRuleAssicationWithTermIdAsync(int termId);
        string GetTermTimeSettings(int termId);
        string GetParentTermTimeSettings(int termId);
        Task<string> GetTermRuleInfoByTermidAsync(int termId);
        string GetTermRuleInfoByTermIdAndSourceFlag(int termId, SourceFlag sourceFlag = SourceFlag.All);
        string GetParentSettingInfoByTermId(int termId);
        string GetTermNamesPathByTermId(int termId);

        string GetGermSetNamesPathByTermSetId(int termSetId);
        string GetTermNameByTermId(int termId);
        string GetTermDescriptionByTermId(int termId);
        string GetTermAdvancedSettingsByTermId(int termId);
        Task<TermAuditInfo> GetTermRuleInfosByTermIdAsync(int termId);
        string GetTermGroupNameById(Guid groupId);
        string GetTermGroupNameById(int groupId);
        string GetTermSetNameById(int termSetId);
        bool IsOrphanedTerm(Guid id);
        string RunImportTermStructure(JobRunBy jobRunBy, string extension, string bytes, bool isControlPlus = false);
        Task<RAReturnMessage> RunExportTermStructure(JobRunBy jobRunBy);
        Task<string> RealRunExportTermStructureJob(JobRunBy jobRunBy, string jobRunByUser);
        Task<List<RMSiteInfo>> GetRegisteredSiteMMSInfoAsync();
        List<RMSiteInfo> GetRelativedSiteMMSInfo(Guid termGroupId);
        Task<string> RenameTermGroupAsync(int termGroupId, string termGroupName);
        Task<string> RenameTermSetAsync(int termSetId, string termSetName, Guid termGroupId);
        Task<RMSiteInfo> GetRegisteredSiteMMSInfoByUrlAsync(string url);
        Task<string> DeleteTermGroupAsync(Guid termGroupId);
        string RealRunImportTermStructureJob(JobRunBy jobRunBy, string jobRunByUser, string extension, string strBytes, bool isControlPlus = false);
        string EnableTerm(int termId);
        string GetTermTree(CurrentSettingsInfo settingInfo);
        System.Threading.Tasks.Task GenerateReportForTermInfoAsync(string folderPath, string fileName, string sheetName);
        bool GetTermPermanentByTermId(int termId, bool onlyParent);
        string GetTermWithPathByTermId(Guid termId);
        string GetTermPathByTermId(Guid termId, bool forExport = false);
        DeclarationSetting GetTermRetentionInfoByTermId(Guid termId);
        Task<string> GetTaxonomyTermAsync(string typeName, string treeNodeId, int pageIndex, int pageCount, string groupId, int SettingType, bool needCheckPermission = false);
        Task<string> GetEXOSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false);
        Task<List<RMTermInfo>> GetTaxonomyTreeDataAsync(RMTermType typeName, string treeNodeId, int pageIndex, int pageCount);
        Contract.RMReport.TermTreeNode GetTermTreeByMailBox(string mailBox);

        RMTermInfo GetDefaultTermByMailBox(string mailBox);
        string DeleteRootTerms(int termSetId);

        Task<string> GetPRSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false);

        Task<string> LoadTermGroupsAsync(FilterTermObjOption filterTermObjOption);
        Task<string> LoadClassCodeGroupsAsync(FilterTermObjOption filterOption, Guid termSetId, string searchTerm = null, int pageIndex = 0, int pageSize = 0);
        Task<string> GetTermSetsAsync(List<Guid> termGroupIds);

        void CreateExportStatusRecord(Guid uniqueId);
        void UpdateExportStatus(Guid uniqueId, ExportTermsWithRulesStatus status);
        ExportTermsWithRulesStatus CheckExportStatus(Guid uniqueId);
        Task<string> GetFSSavedTermAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false);
        Task<string> GetAzureFileSavedTermAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false);
        Task<string> GetBoxSavedTermAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false);
        Task<Dictionary<string, string>> GetAllTermGroups();
        Task<Dictionary<string, string>> GetAllTermGroupsByMultipleNodes(RMClassificationGroupMultipleNodes nodes);

        Task<List<RMSiteInfo>> GetGoogleTermGroupSettingAsync();
        Task<Dictionary<string, List<string>>> GetTermGroupNameAndGoogleTenantsAsync(Guid termGroupId);

        Task<string> AddFirstTermSetAsync(Guid termGroupId);

        Task<int> FindOrAddFirstTermSetAsync(Guid termGroupId);
        Task<string> GetTermsByGroupId(Guid termGroupId);

        Task<RAReturnMessage> AIRecomendationAsync(AIRecomentdation aIRecomentdation);
        Task<MemoryStream> GetStreamAIRecommendation(string industry, List<RecordCategory> records, bool isControlPlus = false);
        #region fs
        string GetAllTermsUnderTermSet(int termSetId);
        string GetRMTermByGuId(Guid termId);
        string GetAllSubLocationTerm(int termId);
        List<FSTermDto> GetAllTermsForce();
        Dictionary<int, List<Guid>> GetTermRuleMapping();
        string GetTermTreeForSecurityGroup(QueryTermObjDto queryDto);
        SecurityTermInfo BuildSecurityTermTree(SecurityTermInfo dbRootNode, int groupId);
        SecurityRuleInfo BuildSecurityRuleTree(SecurityRuleInfo dbRootNode, int groupId);
        Task<List<ClassCodeCascadeDataDto>> GetClassCodeCascadeDataAsync(CurrentSettingsInfo settingsInfo);
        Task<List<RMMyhubClassCodeCascadeDataDto>> RMMyhubGetClassCodeCascadeDataAsync(string termSetId);
        #endregion
        OlderThanTimeDto GetTheRetentionUnitByClassCode(ApplyClassCodeSettingDto dto);
        #region Onpremise SP
        Task<string> GetSPOnPremSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false);
        List<AgentTermSetDto> GetAllTermSetsForce();
        List<AgentTermSetMembershipDto> GetAllTermSetMemberShipsForce();
        #endregion

        string GetTemplateFilePath();
        bool HasTermGroupName(string termGroupName);
        bool HasTermSetName(string termSetName, Guid termGroupId);
        #region googleone
        string GetTermRuleInfoByTermIdAndSourceFlagForGoogleOne(int termId, SourceFlag sourceFlag = SourceFlag.All);
        List<RMTermInfo> SearchTermWithLimit(string searchValue, int limit);
        Task<List<RMTermInfo>> SearchLabelWithLimit(string searchValue, int limit);
        #endregion
    }
}
