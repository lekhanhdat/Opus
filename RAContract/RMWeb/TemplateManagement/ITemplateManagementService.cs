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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.TemplateManagement
{
    public interface ITemplateManagementService
    {
        #region remove code
        //int CreateCategory(int parentTemplateId, string name, TemplateType type);
        //bool CreateCategory(string name);
        //bool CheckColumnsSameName(string columnName, int templateId);
        #endregion
        Task<int> InitDefaultDataAsync();
        Task<int> ResetDefaultDataAsync();
        Task<TemplateDto> LoadTemplateDtoAsync(int id, bool forBulkUpdate = false);
        Task<TemplateDto> LoadTemplateDtoAsync(int id, Contract.Explorer.PhysicalObjectDto dto);
        Task<TemplateDto> LoadTemplateDtoAsync(Guid uniqueId);
        Task<TemplateDto> LoadTemplateDtoAsync(Guid uniqueId, Contract.Explorer.PhysicalObjectDto dto);
        Task<TemplateDto> GetTemplateByNodeTypeAsync(RMNodeLevel nodeType);
        Task<string> LoadTemplateDatasAsync(int id, bool forBulkUpdate = false);
        Task<string> LoadTemplateDatasAsync(Guid uniquIid);
        Task<string> LoadTemplateDatasAsync(SuiteTemplateQueryDto queryDto);
        bool ValidateDuplicateColumn(int typeId, string columnName);
        string LoadChildTemplateCategory(int id);

        (TemplateDto dto, bool result) SaveTemplateWithColumns(TemplateDto dto);
        Task<string> GetAllTemplateDatasAsync();
        Task<List<TemplateDto>> GetAllTemplateDtosAsync();
        Task<TemplateDto> GetTemplateDtosByNameAsync(string templateName);
        Dictionary<int, int> GetNodeTypeAndTemplateIdMapping();
        int ValidHasUniqueIdSettings(Guid templateId);
        bool CreateSuite(SuiteDto dto);
        bool UpdateSuite(SuiteDto dto);
        RAReturnMessage DeleteSuite(Guid suiteId);
        //[Obsolete]
        //RAReturnMessage DeleteTemplate(Guid templateId, Guid parentFolderId, Guid parentBoxId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="idPathList">Id list start from suite, first one is suite unique id, others are template id(not unique id)</param>
        /// <returns></returns>
        RAReturnMessage DeleteTemplate(Guid templateId, List<string> idPathList);

        SuiteDto LoadSuite(Guid id);
        //[Obsolete]
        //SuiteTemplateResultDto GetSuites(SuiteTemplateQueryDto queryDto);
        //[Obsolete]
        //SuiteTemplateResultDto GetTemplatesByParent(SuiteTemplateQueryDto queryDto);

        /// <summary>
        /// get template
        /// </summary>
        /// <param name="browserDto"></param>
        /// <returns></returns>
        Task<SuiteTemplateBrowserResultDto> GetTemplatesByParentV2ByPageAsync(SuiteTemplateBrowserDto browserDto);

        SuiteTemplateBrowserResultDto GetSuitesV2ByPage(SuiteTemplateBrowserDto browserDto);

        List<SimplifySuiteDto> LoadAllSuites();
        /// <summary>
        /// get all root templates for this location
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        Task<List<SimplifyTemplateDto>> GetAllTemplatesByLocationId4ExplorerAsync(Guid locationId);
        Task<List<SimplifyTemplateDto>> GetTemplatesByPhysicalObject4ExplorerAsync(PhysicalObjectDto phyObjDto);

        /// <summary>
        /// get all sub templates under templateIdPath based on types
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="templateIdPath"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        Task<List<SimplifyTemplateDto>> GetTemplatesByIdPathAsync(Guid templateId, string templateIdPath, List<TemplateType> types);
        TemplateDto GetDefaultCategoryAndColumn(int type);
        TemplateInfoOfBreadCrumbs GetTemplateInfoOfBreadCrumbs(SuiteTemplateQueryDto queryDto);
        Task<ExistingTemplatesInfo> GetExistingFolderTemplatesInfoAsync(Guid suiteId);
        Task<ExistingTemplatesInfo> GetExistingTemplatesInfoAsync(QueryExistingTemplatesDto queryDto);
        bool AddExistingTemplates(AddExistingTemplatesDto dto);
        string LoadingUniqueIdSetting();
        Task<bool> ToggleGlobalUniqueIdSettingsAsync(bool isGlobal);
        Task<bool> UpdateGlobalUniqueIdSettingsAsync(GlobalUniqueIdSettingsDto settingsDto);
        Task<SaveTemplateResult> CheckTemplateBeforeSavingAsync(Guid uniqueId, string prefix, string name, TemplateType templateType, List<string> idPathList);
        /// <summary>
        /// load all columns of all templates.
        /// </summary>
        /// <returns></returns>
        Task<List<TemplateColumn4Display>> GetAllColumnsAsync();

        Task<List<TemplateColumn4Display>> GetCustomMetadataColumnsAsync();
        string GetColumnOptions(TemplateColumn4Query param);
        /// <summary>
        /// return the template id path for the current physical record. path is like '6feecea2-2076-4557-ae9c-a90f9eb91617/1/', first part is suite id, the others are template id.
        /// </summary>
        /// <param name="phyObjDto"></param>
        /// <returns></returns>
        Task<string> GetTemplateIdPathAsync(AvePoint.RA.Contract.Explorer.PhysicalObjectDto phyObjDto);

        /// <summary>
        /// update custom column index policy to Cosmos DB
        /// </summary>
        System.Threading.Tasks.Task UpdateIndexPolicyAsync();

        /// <summary>
        /// get templates with some fields, e.g, name,id,uniqueId, type
        /// </summary>
        /// <returns></returns>
        List<SimplifyTemplateDto> GetAllExistingSimplifyTemplates();

        void CheckCategoriesAndColumnsData(TemplateDto template);

        string RunPhysicalTemplateImportJob(string blobName);
        string RealRunPhysicalTemplateImportJob(JobRunBy jobRunBy, string jobRunByUser ,string blobName);

        #region Google One
        bool IsExcludeTemplateColumn(Guid columnId);
        #endregion
    }
}
