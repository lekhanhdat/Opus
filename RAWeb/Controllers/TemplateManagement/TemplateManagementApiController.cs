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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common.Filters;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.TermManagement;
using System.IO;
using System.Net;
using AvePoint.RA.Common.Util;
using Microsoft.AspNetCore.StaticFiles;
using AvePoint.RA.CommonUtil;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using AvePoint.RA.DB.SecurityTrimming.Model;

namespace AvePoint.RA.Web.Controllers.TemplateManagement
{
    [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser, preferred: false)]
    public class TemplateManagementApiController : BaseApiController
    {
        private const int MaxSuiteNameLength = 450;
        private ITemplateManagementService _TemplateManagementService;
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService(ref _TemplateManagementService);
        private IBarcodeTemplateService _BarcodeTemplateService;
        private IBarcodeTemplateService BarcodeTemplateService => PlatformWindsorManager.GetService(ref _BarcodeTemplateService);

        [HttpPost]
        public async Task<bool> ResetDefaultData()
        {
            return (await TemplateManagementService.ResetDefaultDataAsync()) > 0;
        }

        //[HttpGet]
        //public string LoadCategories(int templateId)
        //{
        //    return mTemplateManagementService.LoadCategories(templateId);
        //}

        [HttpGet]
        public Task<string> LoadTemplateDatas(int id)
        {
            return TemplateManagementService.LoadTemplateDatasAsync(id);
        }

        [HttpGet]
        public Task<string> LoadTemplateDatas(Guid uniqueId)
        {
            return TemplateManagementService.LoadTemplateDatasAsync(uniqueId);
        }

        [HttpPost]
        public Task<string> LoadTemplateDatas([FromBody] SuiteTemplateQueryDto queryDto)
        {
            return TemplateManagementService.LoadTemplateDatasAsync(queryDto);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public async Task<List<TemplateColumn4Display>> LoadAllColumns([FromBody] LoadTemplateColumn4DisplayParam param)
        {
            var result =  await TemplateManagementService.GetAllColumnsAsync();
            if (param.LoadAll) return result.Where(o => o.UniqueId != new Guid(DefaultColumnIDs.Barcode)).ToList();

            return result.Where(o => param.ColumnTypes.Contains(o.ColumnType) && o.UniqueId != new Guid(DefaultColumnIDs.Barcode)).ToList();
        }

        [HttpPost]
        public string GetColumnOptions([FromBody] TemplateColumn4Query param)
        {
            return TemplateManagementService.GetColumnOptions(param);
        }

        [HttpGet]
        public string LoadChildTemplateCategory(int id)
        {
            return TemplateManagementService.LoadChildTemplateCategory(id);
        }

        [HttpPost]
        public async Task<bool> UpdateIndexPolicy()
        {
            await TemplateManagementService.UpdateIndexPolicyAsync();
            return true;
        }

        [HttpPost]
        [ValidateTemplateParameterFilter("ValidateSaveTemplate")]
        public async Task<SaveTemplateResultWithTemplate> SaveTemplateWithColumns([FromBody] TemplateDto dto)
        {
            var checkResult = await TemplateManagementService.CheckTemplateBeforeSavingAsync(dto.uniqueId, dto.prefix, dto.name, dto.type, dto.ParentTemplateIdList);
            if (checkResult == SaveTemplateResult.None)
            {
                var saveResult = TemplateManagementService.SaveTemplateWithColumns(dto);
                return new()
                {
                    SaveTemplateResult = saveResult.result ? SaveTemplateResult.Success : SaveTemplateResult.Failed,
                    TemplateInfo = saveResult.dto,
                };
            }
            else
            {
                return new()
                {
                    SaveTemplateResult = checkResult,
                    TemplateInfo = new()
                };
            }
        }

        [HttpPost]
        public Task<string> GetAllTemplateDatas()
        {
            //mTemplateManagementService.InitDefaultData();
            return TemplateManagementService.GetAllTemplateDatasAsync();
        }

        [HttpPost]
        public bool ValidateDuplidateColName([FromBody] Models.PRM.ColumnParam param)
        {
            return TemplateManagementService.ValidateDuplicateColumn(param.typeId, param.columnName);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public Dictionary<int, int> GetNodeTypeAndTemplateIdMapping()
        {
            return TemplateManagementService.GetNodeTypeAndTemplateIdMapping();
        }

        /// <summary>
        /// UniqueId is not required for creation
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public SaveTemplateResult CreateSuite([FromBody] SuiteDto dto)
        {
            if (!Validate(dto)) return SaveTemplateResult.Failed;
            var allSuites = TemplateManagementService.LoadAllSuites();
            var allSuiteNames = allSuites.Select(s => s.Name).ToList();
            var allSuiteNamesI18N = allSuiteNames.Select(name => I18NEntity.GetString(name)).ToList();
            if (allSuiteNames.Contains(dto.Name, StringComparer.OrdinalIgnoreCase) ||
                allSuiteNamesI18N.Contains(I18NEntity.GetString(dto.Name), StringComparer.OrdinalIgnoreCase))
            {
                return SaveTemplateResult.NameDuplicate;
            }
            if (TemplateManagementService.CreateSuite(dto)) return SaveTemplateResult.Success;

            var suiteNames = TemplateManagementService.LoadAllSuites().Select(s => s.Name);
            return suiteNames.Contains(dto.Name, StringComparer.OrdinalIgnoreCase)
                ? SaveTemplateResult.NameDuplicate
                : SaveTemplateResult.Failed;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public SaveTemplateResult UpdateSuite([FromBody] SuiteDto dto)
        {
            if (!Validate(dto)) return SaveTemplateResult.Failed;
            var allSuites = TemplateManagementService.LoadAllSuites();
            var allSuiteNames = allSuites.Where(s => s.UniqueId != dto.UniqueId).Select(s => s.Name).ToList();
            if (allSuiteNames.Contains(dto.Name, StringComparer.OrdinalIgnoreCase))
            {
                return SaveTemplateResult.NameDuplicate;
            }
            return TemplateManagementService.UpdateSuite(dto) ? SaveTemplateResult.Success : SaveTemplateResult.Failed;
        }

        private bool Validate(SuiteDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Name)) return false;
            dto.Name = dto.Name.Trim();
            return dto.Name.Length <= MaxSuiteNameLength;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public RAReturnMessage DeleteSuite([FromBody]Guid suiteId)
        {
            return TemplateManagementService.DeleteSuite(suiteId);
        }

        [HttpGet]
        public SuiteDto LoadSuite(Guid id)
        {
            return TemplateManagementService.LoadSuite(id);
        }

        //[HttpPost]
        //public SuiteTemplateResultDto GetAllSuites(SuiteTemplateQueryDto queryDto)
        //{
        //    using (PerformanceScope scope = new PerformanceScope("template.initDefaultData"))
        //    {
        //        mTemplateManagementService.InitDefaultData();
        //    }
        //    return mTemplateManagementService.GetSuites(queryDto);
        //}

        //[HttpPost]
        //public SuiteTemplateResultDto GetTemplatesByParent(SuiteTemplateQueryDto queryDto)
        //{
        //    return mTemplateManagementService.GetTemplatesByParent(queryDto);
        //}

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<SuiteTemplateBrowserResultDto> Browser([FromBody] SuiteTemplateBrowserDto browserDto)
        {
            if (browserDto.Node == null || browserDto.Node.TemplateIdList.Count == 0)  //get suite
            {
                return TemplateManagementService.GetSuitesV2ByPage(browserDto);
            }
            return await TemplateManagementService.GetTemplatesByParentV2ByPageAsync(browserDto);
        }
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public RAReturnMessage DeleteTemplate([FromBody] DelTemplateParam dto)
        {
            //return mTemplateManagementService.DeleteTemplate(dto.TemplateId, dto.ParentFolderId, dto.ParentBoxId);
            return TemplateManagementService.DeleteTemplate(dto.TemplateId, dto.TemplateIdList);

        }

        [HttpPost]
        public List<SimplifySuiteDto> GetAllSimplifySuites()
        {
            //mTemplateManagementService.InitDefaultData();
            return TemplateManagementService.LoadAllSuites();
        }

        [HttpGet]
        public string GetDefaultCategoryAndColumn(int type)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(TemplateManagementService.GetDefaultCategoryAndColumn(type));
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public string GetAllDefaultCategoryAndColumn()
        {
            var dic = new Dictionary<int, TemplateDto>
            {
                [(int)TemplateType.Box] = TemplateManagementService.GetDefaultCategoryAndColumn((int)TemplateType.Box),
                [(int)TemplateType.Folder] = TemplateManagementService.GetDefaultCategoryAndColumn((int)TemplateType.Folder),
                [(int)TemplateType.Records] = TemplateManagementService.GetDefaultCategoryAndColumn((int)TemplateType.Records),
                [(int)TemplateType.Custom] = TemplateManagementService.GetDefaultCategoryAndColumn((int)TemplateType.Custom)
            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(dic);
        }

        [HttpPost]
        public TemplateInfoOfBreadCrumbs GetTemplateInfoOfBreadCrumbs([FromBody] SuiteTemplateQueryDto queryDto) {
            return TemplateManagementService.GetTemplateInfoOfBreadCrumbs(queryDto);
        }

        [HttpGet]
        public Task<ExistingTemplatesInfo> GetExistingFolderTemplatesInfo(Guid suiteId) {
            return TemplateManagementService.GetExistingFolderTemplatesInfoAsync(suiteId);
        }

        [HttpPost]
        public Task<ExistingTemplatesInfo> GetExistingTemplatesInfo([FromBody] QueryExistingTemplatesDto queryDto)
        {
            return TemplateManagementService.GetExistingTemplatesInfoAsync(queryDto);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public ExistingTemplatesInfo GetAllExistingTemplatesInfo()
        {
            var result = new ExistingTemplatesInfo();
            result.Templates = TemplateManagementService.GetAllExistingSimplifyTemplates().OrderBy(o => o.Type).ThenBy(o => o.Name).ToList(); ;
            return result;
        }

        [HttpPost]
        public bool AddExistingTemplates([FromBody] AddExistingTemplatesDto dto) {
            return TemplateManagementService.AddExistingTemplates(dto);
        }

        [HttpGet]
        public string LoadingUniqueIdSetting()
        {
            return TemplateManagementService.LoadingUniqueIdSetting();
        }

        [HttpPost]
        public Task<bool> ToggleGlobalUniqueIdSettings([FromBody]bool isGlobal)
        {
            return TemplateManagementService.ToggleGlobalUniqueIdSettingsAsync(isGlobal);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [ValidateTemplateParameterFilter("ValidateSaveGlobalUniqueId")]
        public Task<bool> SaveGlobalUniqueIdSettings([FromBody] GlobalUniqueIdSettingsDto dto)
        {
            return TemplateManagementService.UpdateGlobalUniqueIdSettingsAsync(dto);
        }

        //[HttpGet]
        //public SaveTemplateResult CheckTemplateBeforeSaving(string prefix, Guid uniqueId)
        //{
        //    return mTemplateManagementService.CheckTemplateBeforeSaving(prefix, uniqueId);
        //}

        [HttpGet]
        public TemplateColumnInfo GetAllTemplateColumn()
        {
            return BarcodeTemplateService.GetAllTemplateColumnAsync().GetAwaiter().GetResult();
        }
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [ValidateBarcodeTemplateParameterFilter]
        public string CreateBarcodeTemplate([FromBody] BarcodeTemplateDto dto)
        {
            var result = BarcodeTemplateService.CreateBarcodeTemplate(dto);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return string.Empty;
            }
            return result.ErrorMessage;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [ValidateBarcodeTemplateParameterFilter]
        public async Task<string> UpdateBarcodeTemplate([FromBody] BarcodeTemplateDto dto)
        {
            var result = await BarcodeTemplateService.UpdateBarcodeTemplateAsync(dto);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return string.Empty;
            }
            return result.ErrorMessage;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<BarcodeTemplateDto> LoadBarcodeTemplateByType([FromBody]int type)
        {
            BarcodeTemplateType tempType = (BarcodeTemplateType)type;
            BarcodeTemplateDto templateDto = await BarcodeTemplateService.GetDefaultBarcodeTemplateByTypeAsync(tempType);
            return templateDto;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [ValidateBarcodeTemplateParameterFilter("CreateCustomBarcodeTemplateSuites")]
        public async Task<RAReturnMessage> CreateCustomBarcodeTemplate([FromBody] BarcodeCustomTemplateDto dto)
        {
            try
            {
                return await BarcodeTemplateService.CreateCustomBarcodeTemplateAsync(dto);   
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in CreateCustomBarcodeTemplate: {ex.Message}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "An error occurred while creating the custom barcode template."
                };
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [ValidateBarcodeTemplateParameterFilter("UpdateCustomBarcodeTemplateSuites")]
        public async Task<RAReturnMessage> UpdateDefaultBarcodeTemplate([FromBody] BarcodeDefaultTemplateDto dto)
        {
            try
            {
                return await BarcodeTemplateService.UpdateDefaultBarcodeTemplateAsync(dto);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in UpdateCustomBarcodeTemplate: {ex.Message}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "An error occurred while updating the custom barcode template."
                };
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        // [ValidateBarcodeTemplateParameterFilter]
        public async Task<RAReturnMessage> UpdateCustomBarcodeTemplate([FromBody] BarcodeCustomTemplateDto dto)
        {
            try
            {
                return await BarcodeTemplateService.UpdateCustomBarcodeTemplateAsync(dto);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in UpdateCustomBarcodeTemplate: {ex.Message}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "An error occurred while updating the custom barcode template."
                };
            }
        }

        [HttpPost]
        public async Task<ActionResult> ExportPreviewBarcode()
        {
            var exportTypeStr = Request.Form["TemplateInfoes"];
            var dto = JsonConvert.DeserializeObject<BarcodeCustomTemplateDto>(exportTypeStr);
            var result = await BarcodeTemplateService.DownLoadPrivewBarcodeTemplateAsync(dto);
            if (result == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
            var fileName = result.FileName;
            if (GCommon.Utility.SecurityUtils.IsValidFileName(fileName))
            {
                return File(result.FileContent, "application/octet-stream", fileName);
            }
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<List<BarcodeTemplateSuiteDto>> GetAllBarcodeTemplateSuites()
        {
            try
            {
                return await BarcodeTemplateService.GetAllBarcodeTemplateSuitesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in GetAllBarcodeTemplateSuites: {ex.Message}", ex);
                return new List<BarcodeTemplateSuiteDto>();
            }
        }

        /// <summary>
        /// Get barcode template suite by unique ID
        /// </summary>
        /// <param name="suiteId">Unique ID of the template suite</param>
        /// <returns>Barcode template suite or null if not found</returns>
        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<BarcodeTemplateSuiteDto> GetBarcodeTemplateBySuiteId(Guid suiteId)
        {
            try
            {
                return await BarcodeTemplateService.GetBarcodeTemplateBySuiteIdAsync(suiteId);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in GetBarcodeTemplateBySuiteId: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// Get paged barcode template suites with optional search and filtering
        /// </summary>
        /// <param name="request">Paging and search parameters</param>
        /// <returns>Paged result with barcode template suites</returns>
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<PagedBarcodeTemplateSuiteResult> GetPagedBarcodeTemplateSuites([FromBody] PagedBarcodeTemplateSuiteRequest request)
        {
            try
            {
                return await BarcodeTemplateService.GetPagedBarcodeTemplateSuitesAsync(request);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in GetPagedBarcodeTemplateSuites: {ex.Message}", ex);
                return new PagedBarcodeTemplateSuiteResult
                {
                    PageIndex = request?.PageIndex ?? 0,
                    PageSize = request?.PageSize ?? 20,
                    TotalCount = 0,
                    Suites = new List<BarcodeTemplateSuiteDto>()
                };
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [ValidateBarcodeTemplateParameterFilter("BatchDeleteCustomBarcodeTemplateSuites")]
        public async Task<RAReturnMessage> BatchDeleteCustomBarcodeTemplateSuites([FromBody] List<Guid> suiteIds)
        {
            try
            {
                var result = await BarcodeTemplateService.BatchDeleteCustomBarcodeTemplateSuitesAsync(suiteIds);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in BatchDeleteCustomBarcodeTemplateSuites: {ex.Message}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                };
            }
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public SuiteTemplateTreeNode InitTree()
        {
            //mTemplateManagementService.InitDefaultData();
            var rootNode = new SuiteTemplateTreeNode { Name = I18NEntity.GetString("RM_PRM_TM_Title_RootNode") };
            var browserDto = new SuiteTemplateBrowserDto { 
                Node = rootNode,
                PagingInfo = new SuiteTemplatePagingInfo { PageIndex = 1, PageSize = 15}
            };
            var queryResult = TemplateManagementService.GetSuitesV2ByPage(browserDto);
            rootNode.Children = queryResult.Children;
            rootNode.ChildrenCount = queryResult.ChildrenCount;
            return rootNode;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public IActionResult DownloadTemplate()
        {
            try
            {
                var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "Bulk Create template.xlsx"); ;
                var name = Path.GetFileName(filepath);
                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                var ContentType = GetContentType(filepath);
                return File(memoryStream, ContentType, name);
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<string> ImportData()
        {
            string jobId = "";
            try
            {
                var file = Request.Form.Files["fileUp"];
                Logger.Info("tm import file,file name :{0}", file.FileName);
                CheckFile(file);
                DateTime dt = DateTime.Now;
                string fileName = "Template_" + dt.Ticks.ToString() + ".xlsx";
                var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
                RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
                jobId = TemplateManagementService.RunPhysicalTemplateImportJob(blobName);
            }
            catch (Exception ex)
            {
                Trace.TraceError("error occurred import data:{0}", ex.ToString());
            }
            return jobId;
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        private void CheckFile(IFormFile file)
        {
            string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            var allowFileExts = new List<FileExtension> { FileExtension.CSV, FileExtension.XLSX, FileExtension.XML };
            WebUtil.CheckFileExtension(extension, allowFileExts);
        }
    }
}
