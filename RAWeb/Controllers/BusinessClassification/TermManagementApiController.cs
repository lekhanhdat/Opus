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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Cache;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.Filters.RuleApiFilter;
using AvePoint.RA.Web.Common.WIF;
using FluentFTP.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.Discover;
using RAExportCommon;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Google.Model;
using AvePoint.RA.Contract.Label;
using RATeams;
using AvePoint.RA.Common.Threads;
using Util.AI.Text.Extractor;
using SkiaSharp;
using Microsoft.SemanticKernel;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.Web.Extentions.Authorize;

namespace RecordManager.Controllers.BusinessClassification
{
    [RMApiAuthorize(RMPermissionMasks.TermManagementEnduser, preferred: false)]
    public class TermManagementApiController : BaseApiController
    {
        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private IRuleManagerService _RuleManagerService;
        private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService(ref _RuleManagerService);
        private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);
        private IEnforceRetentionService _EnforceRetentionService;
        private IEnforceRetentionService EnforceRetentionService => PlatformWindsorManager.GetService(ref _EnforceRetentionService);
        private IRMSecurityTrimmingHelper _SecurityTrimmingHelper;
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService(ref _SecurityTrimmingHelper);
        private IRuleContainerService _RuleContainerService;
        private IRuleContainerService RuleContainerService => PlatformWindsorManager.GetService(ref _RuleContainerService);
        private IRMGoogleJobService GoogleJobService => PlatformWindsorManager.GetService(ref _GoogleJobService);
        private IRMGoogleJobService _GoogleJobService;
        private IRMKeyValueDao _KeyValueDao;
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService(ref _KeyValueDao);
        private ITermDao _TermDao;
        private ITermDao TermDao => PlatformWindsorManager.GetService(ref _TermDao);
        private ITermSetDao _TermSetDao;
        private ITermSetDao TermSetDao => PlatformWindsorManager.GetService(ref _TermSetDao);
        private ITermGroupDao _TermGroupDao;
        private ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService(ref _TermGroupDao);
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private Task<GeneralSettingModel> GeneralSetting
        {
            get
            {
                return GeneralSettingService.GetGeneralSettingAsync();
            }
        }
        [HttpGet]
        [ValidTermTreeParameterFilter("BrowseTermManageTree")]
        public Task<string> GetChildrenByDB([FromQuery] TreePage tree)
        {
            int pIndex = 0;
            if (tree.PageIndex != null)
            {
                int.TryParse(tree.PageIndex.ToString(), out pIndex);
            }
            int pSize = 0;
            if (tree.PageSize != null)
            {
                int.TryParse(tree.PageSize.ToString(), out pSize);
            }
            pIndex = pIndex == 0 ? pIndex : pIndex - 1;

            string nodeId = string.Empty;
            if (tree.NodeId != null)
            {
                nodeId = tree.NodeId;
            }

            string nodeType = string.Empty;
            if (tree.NodeType != null)
            {
                nodeType = tree.NodeType;
            }
            var filterOption = new FilterTermObjOption
            {
                NeedCheckPermission = true,
                FilterByContentSource = true,
                ExcludeBuiltIn = tree.ExcludeBuiltIn,
                SourceFlag = tree.SourceFlag,
                ContainerId = tree.ContainerId,
                ForPhysicalView = tree.ForPhysicalView
            };
            return TaxonomyService.GetTaxonomyTreeDataAsync(nodeType, nodeId, pIndex, pSize, tree.SPTreeNodes, 0, filterOption);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManualReviewEnduser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        [ValidPhysicalEndUserParameterActionFilter]
        public Task<string> GetChildrenByDBForView([FromQuery] TreePage tree)
        {
            int pIndex = 0;
            if (tree.PageIndex != null)
            {
                int.TryParse(tree.PageIndex.ToString(), out pIndex);
            }
            int pSize = 0;
            if (tree.PageSize != null)
            {
                int.TryParse(tree.PageSize.ToString(), out pSize);
            }
            pIndex = pIndex == 0 ? pIndex : pIndex - 1;

            string nodeId = string.Empty;
            if (tree.NodeId != null)
            {
                nodeId = tree.NodeId;
            }

            string nodeType = string.Empty;
            if (tree.NodeType != null)
            {
                nodeType = tree.NodeType;
            }
            var filterOption = new FilterTermObjOption
            {
                NeedCheckPermission = !tree.ShowAllTerms,
                FilterByContentSource = true,
                ExcludeBuiltIn = tree.ExcludeBuiltIn,
                SourceFlag = tree.SourceFlag,
                ContainerId = tree.ContainerId,
                ForPhysicalView = true
            };

            if (tree.SourceFlag == SourceFlag.Google)
            {
                return TaxonomyService.GetTaxonomyGoogleTermTreeDataAsync(filterOption, pIndex, pSize);
            }
            return TaxonomyService.GetTaxonomyTreeDataAsync(nodeType, nodeId, pIndex, pSize, tree.SPTreeNodes, 0, filterOption);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)]
        public Task<string> GetGroups()
        {
            return TaxonomyService.LoadTermGroupsAsync(new FilterTermObjOption() { NeedCheckPermission = true });
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)]
        public Task<string> GetClassCodeGroup([FromBody] ClassCodeRequest request)
        {
            int pageIndex = request.PageIndex > 0 ? request.PageIndex - 1 : 0;
            return TaxonomyService.LoadClassCodeGroupsAsync(new FilterTermObjOption() { NeedCheckPermission = true }, request.TermSetId, request.SearchKey, pageIndex, request.PageSize);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyEnduser)]
        public Task<string> LoadGroups(string containerId, SourceFlag sourceFlag)
        {
            return TaxonomyService.LoadTermGroupsAsync(new FilterTermObjOption()
            {
                NeedCheckPermission = true,
                FilterByContentSource = true,
                ExcludeBuiltIn = true,
                ContainerId = containerId,
                SourceFlag = sourceFlag
            });
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionExtensionMasks.GoogleAdmin)]
        public async Task<string> GetGoogleTermsTreeApplySetting([FromBody] LabelDisplayConditions conds)
        {
            return await TaxonomyService.GetTaxonomyGoogleTermTreeApplySettingDataAsync(conds.NodeId, conds.PageNumber, conds.PageSize, conds.SearchKey);
        }

        [HttpGet]
        public Task<string> LoadGroupsWithPermission()
        {
            return TaxonomyService.LoadTermGroupsAsync(new FilterTermObjOption() { NeedCheckPermission = true });
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)]
        public Task<string> GetAllChildren([FromBody] TreePage tree)
        {
            string nodeId = string.Empty;
            if (tree.NodeId != null)
            {
                nodeId = tree.NodeId;
            }

            string nodeType = string.Empty;
            if (tree.NodeType != null)
            {
                nodeType = tree.NodeType;
            }
            var filterOption = new FilterTermObjOption
            {
                NeedCheckPermission = true,
                FilterByContentSource = true,
                ExcludeBuiltIn = tree.ExcludeBuiltIn,
                SourceFlag = tree.SourceFlag,
                ContainerId = tree.ContainerId
            };
            return TaxonomyService.GetTaxonomyTreeDataAsync(nodeType, nodeId, filterOption, true);
        }


        [HttpPost]
        [ValidPermissionFilter(AvePoint.RA.Contract.RoleAssignments.RMPermissionMasks.TermManagementEnduser)]
        [ValidTermTreeParameterFilter("CreateTerm")]
        public Task<string> CreateTerm([FromBody] TermInfo termModel)
        {
            return RouteMultiGeoApiActionAsync<TermInfo, string>(
                termModel,
                MultiGeoOperationType.CreateTerm,
                async request =>
                {
                    request.TermName = this.replaceStr(request.TermName);
                    return await TaxonomyService.CreateTermAsync(request);
                },
                (request, _) =>
                {
                    BuildCreateTermReplicaRequest(request);
                    return Task.CompletedTask;
                },
                _ => "-1");
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("CreateTerm")]
        public Task<string> RenameTerm([FromBody] TermInfo termModel)
        {
            return RouteMultiGeoApiActionAsync<TermInfo, string>(
                termModel,
                MultiGeoOperationType.RenameTerm,
                async request =>
                {
                    return await TaxonomyService.RenameTermAsync(request.TermId, this.replaceStr(request.TermName), request.TermSetId);
                },
                (request, _) =>
                {
                    BuildRenameTermReplicaRequest(request);
                    return Task.CompletedTask;
                },
                _ => JsonConvert.SerializeObject(new { message = "-2" }));
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("RenameTermGroup")]
        public Task<string> RenameTermGroup([FromBody] TermInfo termModel)
        {
            return RouteMultiGeoApiActionAsync<TermInfo, string>(
                termModel,
                MultiGeoOperationType.RenameTermGroup,
                async request =>
                {
                    return await TaxonomyService.RenameTermGroupAsync(request.TermId, this.replaceStr(request.TermName));
                },
                (request, _) =>
                {
                    BuildRenameTermGroupReplicaRequest(request);
                    return Task.CompletedTask;
                },
                _ => JsonConvert.SerializeObject(new { message = "-2" }));
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("RenameTermSet")]
        public Task<string> RenameTermSet([FromBody] TermInfo termModel)
        {
            return RouteMultiGeoApiActionAsync<TermInfo, string>(
                termModel,
                MultiGeoOperationType.RenameTermSet,
                async request =>
                {
                    return await TaxonomyService.RenameTermSetAsync(request.TermId, this.replaceStr(request.TermName), request.TermGroupUniqueId);
                },
                (request, _) =>
                {
                    BuildRenameTermSetReplicaRequest(request);
                    return Task.CompletedTask;
                },
                _ => JsonConvert.SerializeObject(new { message = "-2" }));
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        [ValidTermTreeParameterFilter("OperatorTerm")]
        public Task<string> DeprecateTerm([FromBody] int termId)
        {
            return RouteMultiGeoApiActionAsync<int, string>(
                termId,
                MultiGeoOperationType.DeprecateTerm,
                _ => Task.FromResult(TaxonomyService.DeprecateTerm(termId)),
                _ => "-1");
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("OperatorTerm")]
        public Task<string> EnableTerm([FromBody] int termId)
        {
            return RouteMultiGeoApiActionAsync<int, string>(
                termId,
                MultiGeoOperationType.EnableTerm,
                _ => Task.FromResult(TaxonomyService.EnableTerm(termId)),
                _ => "-1");
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("OperatorTerm")]
        public Task<string> DeleteTerm([FromBody] int termId)
        {
            return RouteMultiGeoApiActionAsync<int, string>(
                termId,
                MultiGeoOperationType.DeleteTerm,
                _ => TaxonomyService.DeleteTermAsync(termId),
                _ => "-1");
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("DeleteRootTerms")]
        public Task<string> DeleteRootTerms([FromBody] int termSetId)
        {
            return RouteMultiGeoApiActionAsync<int, string>(
                termSetId,
                MultiGeoOperationType.DeleteRootTerms,
                _ => Task.FromResult(TaxonomyService.DeleteRootTerms(termSetId)),
                _ => "-1");
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("DeleteTermGroup")]
        public Task<string> DeleteTermGroup([FromBody] Guid termGroupId)
        {
            return RouteMultiGeoApiActionAsync<Guid, string>(
                termGroupId,
                MultiGeoOperationType.DeleteTermGroup,
                request => TaxonomyService.DeleteTermGroupAsync(request),
                _ => "-1");
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.TermManagementEnduser)]
        public Task<string> Search(string termLabel, Guid termGroupId, string withRuleName)
        {
            return TaxonomyService.SearchAsync(1, this.replaceStr(termLabel), termGroupId, bool.Parse(withRuleName));
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)]
        public Task<string> SearchForCRM(string termLabel, Guid termGroupId, string containerId, SourceFlag sourceFlag)
        {
            return TaxonomyService.SearchAsync(1, this.replaceStr(termLabel), termGroupId, containerId, sourceFlag);
        }

        [HttpGet]
        public Task<string> GetRuleListFromDB(int termId)
        {
            return TaxonomyService.GetTermRuleInfoByTermidAsync(termId);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ManualReviewEnduser | RMPermissionMasks.TermManagementEnduser, permissionJoinType = PermissionJoinType.Any)]
        public string GetTermRuleList(int termId, int sourceFlag)
        {
            return TaxonomyService.GetTermRuleInfoByTermIdAndSourceFlag(termId, (SourceFlag)sourceFlag);
        }

        [HttpGet]
        [ValidPermissionFilter(AvePoint.RA.Contract.RoleAssignments.RMPermissionMasks.RuleManagementEnduser)]
        public async Task<string> GetRuleListFromDA()
        {
            //from da
            List<RMRuleInfos> listRuleFromDA = new List<RMRuleInfos>();
            try
            {
                Logger.Info("Get Rules from DA ");
                using (PerformanceScope scope = new PerformanceScope("Term rules"))
                {
                    var allRuleContainers = await RuleContainerService.GetAllRuleContainersAsync();
                    listRuleFromDA = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync(allRuleContainers.Select(c => c.ContainerId).ToList());
                }
                Logger.Info("Rule count {0}", listRuleFromDA.Count);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while get rules:{0}", ex.ToString());
            }

            return getJsonByObj(listRuleFromDA);
        }

        [HttpGet]
        [ValidPermissionFilter(AvePoint.RA.Contract.RoleAssignments.RMPermissionMasks.RuleManagementEnduser)]
        public async Task<string> GetRecordsRuleListFromDA()
        {
            //from da
            List<RMRuleInfos> listRuleFromDA = new List<RMRuleInfos>();
            try
            {
                Logger.Info("Get Records Rules from DA ");
                using (PerformanceScope scope = new PerformanceScope("Term rules"))
                {
                    var allRuleContainers = await RuleContainerService.GetAllRuleContainersAsync();
                    listRuleFromDA = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync(allRuleContainers.Select(c => c.ContainerId).ToList());
                }
                Logger.Info("Rule count {0}", listRuleFromDA.Count);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while get rules:{0}", ex.ToString());
            }

            return getJsonByObj(listRuleFromDA);
        }

        [HttpGet]
        [RMApiAuthorize(RMSOPermissionMasks.RuleManagementEnduser)]
        public async Task<string> GetArchiverRuleListFromDA()
        {
            //from da
            List<RMRuleInfos> listRuleFromDA = new List<RMRuleInfos>();
            try
            {
                Logger.Info("Get archiver Rules ");
                using (PerformanceScope scope = new PerformanceScope("archive rules"))
                {
                    var allRuleContainers = await RuleContainerService.GetAllRuleContainersAsync();
                    listRuleFromDA = await RuleManagerService.GetSimpleArchiverRulesFromDBAsync(allRuleContainers.Select(c => c.ContainerId).ToList());
                }
                Logger.Info("Rule count {0}", listRuleFromDA.Count);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while get rules:{0}", ex.ToString());
            }

            return getJsonByObj(listRuleFromDA);
        }


        [HttpGet]
        [ValidPermissionFilter(AvePoint.RA.Contract.RoleAssignments.RMPermissionMasks.RuleManagementEnduser)]
        [ValidateTermPermissionFilter]
        public async Task<string> GetAvailableRuleList(int termId)
        {
            List<RMRuleInfos> listRuleFromDA = new List<RMRuleInfos>();
            List<RMRuleInfos> availableRules = new List<RMRuleInfos>();
            try
            {
                Logger.Info("Get Rules from DA ");
                using (PerformanceScope scope = new PerformanceScope("Term rules"))
                {
                    var allRuleContainers = await RuleContainerService.GetAllRuleContainersAsync();
                    listRuleFromDA = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync(allRuleContainers.Select(c => c.ContainerId).ToList());
                    var scopeRuleContainers = SecurityTrimmingHelper.GetRuleScopeByTermId(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserId, termId.ToString());
                    var associateAvailableRule = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync(scopeRuleContainers);
                    var availableRuleIds = associateAvailableRule.Select(r => r.RuleId).ToList();
                    availableRules = listRuleFromDA.Where(r => availableRuleIds.Contains(r.RuleId)).ToList();
                }
                Logger.Info("Rule count {0}", listRuleFromDA.Count);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while get rules:{0}", ex.ToString());
            }

            return getJsonByObj(new { allRules = listRuleFromDA, availableRules });
        }

        [HttpPost]
        [ValidPermissionFilter(AvePoint.RA.Contract.RoleAssignments.RMPermissionMasks.TermManagementAdmin)]
        public IActionResult DownloadTemplate()
        {
            try
            {
                string filepath = TaxonomyService.GetTemplateFilePath();
                var name = System.IO.Path.GetFileName(filepath);
                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(memoryStream);
                }
                if (KeyValueDao.GetValueByKey("JPMC_Customization") != null || TeamsPermissionHelper.HasUpgradeTeamsFeature() || (!DataCenterUtil.Is21V() && TenantService.IsNewOpusTenant()))
                {
                    memoryStream = EditTemplateForJpmc(memoryStream);
                }
                memoryStream.Position = 0;
                var ContentType = GetContentType(filepath);
                return File(memoryStream, ContentType, name);
            }
            catch (Exception e)
            {
                Logger.Error($"Fail download term and rule template,ex:{e}");
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }

        private MemoryStream EditTemplateForJpmc(MemoryStream memoryStream)
        {
            var isEnableJPMC = KeyValueDao.GetValueByKey("JPMC_Customization") != null;
            ExportAddition exportAddition;
            exportAddition = new ExportAddition();
            exportAddition.IsSupportRecordLabelFunction = !DataCenterUtil.Is21V() && TenantService.IsNewOpusTenant();
            exportAddition.IsNeedAddRowData = true;
            exportAddition.TermColumArray = GetAdditionTermColumnArray(exportAddition, isEnableJPMC);
            exportAddition.RuleColumArray = new string[] { JPMCTemplateColumn.ADDITION_RULE_COL };
            exportAddition.ConditionArray = !isEnableJPMC ? new string[] {} : new string[] { JPMCTemplateColumn.ADDITION_CONTITION };
            var content = ExcelUtil.ReadExcelWithHeader(memoryStream);
            var termContent = content["Terms"];
            var ruleContent = content["Rules"];
            #region 更改Term
            try
            {
                //判断当前要插入的列是否被占用
                if (termContent[0][TermPropertyIndex.TimeZone] != null && termContent[0][TermPropertyIndex.TimeZone + 1] == "Notes")
                {
                    for (int i = 1; i < termContent.Count; i++)
                    {
                        List<string> termItem = new List<string>(termContent[i]);
                        for(int j = 0; j < exportAddition.TermColumArray.Length - 1; j++)
                        {
                            termItem.Insert(TermPropertyIndex.TimeZone + j + 1, "");
                        }
                        termContent[i] = termItem.ToArray();
                    }
                    termContent.RemoveAt(0);
                }
                else
                {
                    Logger.Error("JPMC-Column in template is occupied");
                    throw new Exception("JPMC- Column in template is occupied");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("JPMC - Edit Term Failed", ex);
                throw;
            }
            #endregion
            #region 更改Rule
            ruleContent.RemoveAt(0);
            #endregion
            #region 创建新模板
            var newStream = new MemoryStream();
            string tempFilePath = null;
            try
            {
                var tempFolderPath = Path.Combine(WebUtil.GetInstallPath(), "Temp", "Config");
                if (!Directory.Exists(tempFolderPath))
                {
                    Logger.Info("Temp path not find Create Path");
                    Directory.CreateDirectory(tempFolderPath);
                }
                var fileName = $"Temp excel for download{Guid.NewGuid().ToString("N")}.xlsx";
                tempFilePath = Path.Combine(tempFolderPath, fileName);
                ReportUtil.CreateTermsAndRulesSheets(tempFilePath, ruleContent, termContent, exportAddition);
                
                
                using (var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(newStream);
                }
            }
            catch(Exception ex)
            {
                Logger.Error("JPMC - Create new Template Failed", ex);
                throw;
            }
            finally
            {
                try
                {
                    memoryStream.Dispose();
                }
                catch(Exception ex)
                {
                    Logger.Warn("Dispose memory stream Failed",ex);
                }
                try
                {
                    if (!string.IsNullOrEmpty(tempFilePath))
                    {
                        if (System.IO.File.Exists(tempFilePath))
                        {
                            System.IO.File.Delete(tempFilePath);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Logger.Warn("Dispose temp file path Failed");
                }

            }
            #endregion
            return newStream;
        }

        private string[] GetAdditionTermColumnArray(ExportAddition exportAddition, bool isEnableJPMC)
        {
            List<string> result = new();
            if (isEnableJPMC) result.Add("RM_TM_AdvanceSetting");
            if (TeamsPermissionHelper.HasUpgradeTeamsFeature())
            {
                exportAddition.HasUpgradeTeams = true;
                result.Add("RM_TM_Retention_Teams_Label");
            }
            result.Add("Notes");
            return result.ToArray();
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

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser)]
        public async Task<string> GetRuleViewInfoByRuleId(string ruleId)
        {
            RMRuleInfos ruleInfo = await RuleManagerService.LoadRuleAsync(ruleId);
            return getJsonByObj(ruleInfo);
        }
        [HttpPost]
        [ValidTermTreeParameterFilter("InheritSettingToParent")]
        public async Task<string> InheritSettingToParent([FromBody] TermSettingsInfo termInfo)
        {
            return await RouteMultiGeoApiActionAsync<TermSettingsInfo, string>(
                termInfo,
                MultiGeoOperationType.InheritSettingToParent,
                async request =>
                {
                    Logger.Info("Inherit Rule to Parent {0}", request.tId);
                    string strTermDescription = string.Empty;

                    if (!string.IsNullOrEmpty(request.des))
                    {
                        strTermDescription = request.des;
                    }

                    request.des = strTermDescription;
                    string result = await TaxonomyService.SaveTermSettingInheritToParentAsync(request.tId, request);
                    Logger.Info("End inherit rule {0}", request.tId);
                    return result;
                },
                (request, _) =>
                {
                    BuildTermSettingsReplicaRequest(request);
                    return Task.CompletedTask;
                },
                _ => "-1");
        }

        [HttpPost]
        public Task<string> CreateTermGroup([FromBody] TermInfo termModel)
        {
            return RouteMultiGeoApiActionAsync<TermInfo, string>(
                termModel,
                MultiGeoOperationType.CreateTermGroup,
                async request =>
                {
                    return await TaxonomyService.CreateTermGroupAsync(this.replaceStr(request.TermGroupName));
                },
                (request, _) =>
                {
                    BuildCreateTermGroupReplicaRequest(request);
                    return Task.CompletedTask;
                },
                _ => "-1");
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("CreateTermSet")]
        public Task<string> CreateTermSet([FromBody] TermInfo termModel)
        {
            return RouteMultiGeoApiActionAsync<TermInfo, string>(
                termModel,
                MultiGeoOperationType.CreateTermSet,
                async request =>
                {
                    return await TaxonomyService.CreateTermSetAsync(this.replaceStr(request.TermSetName), request.TermGroupUniqueId);
                },
                (request, _) =>
                {
                    BuildCreateTermSetReplicaRequest(request);
                    return Task.CompletedTask;
                },
                _ => "-1");
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("SaveTermSettings")]
        public Task<String> SaveTermSettings([FromBody] TermSettingsInfo setting)
        {
            return RouteMultiGeoApiActionAsync<TermSettingsInfo, string>(
                setting,
                MultiGeoOperationType.SaveTermSettings,
                request =>
                {
                    string strTermDescription = null;

                    if (!string.IsNullOrEmpty(request.des))
                    {
                        strTermDescription = request.des;
                    }

                    request.des = strTermDescription;
                    return TaxonomyService.SaveTermSettingAsync(request);
                },
                (request, _) =>
                {
                    BuildTermSettingsReplicaRequest(request);
                    return Task.CompletedTask;
                },
                _ => "-1");
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("SaveTermSet")]
        public Task<string> SaveTermSet([FromBody] TermInfo termModel)
        {
            return RouteMultiGeoApiActionAsync<TermInfo, string>(
                termModel,
                MultiGeoOperationType.SaveTermSet,
                async request =>
                {
                    return await TaxonomyService.UpdateTermSetAsync(request.TermSetId, this.replaceStr(request.TermSetName), request.Description);
                },
                (request, _) =>
                {
                    BuildSaveTermSetReplicaRequest(request);
                    return Task.CompletedTask;
                },
                _ => "-1");
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("SaveTermGroup")]
        public Task<RAReturnMessage> SaveTermGroup([FromBody] TermInfo termModel)
        {
            return RouteMultiGeoApiActionAsync<TermInfo, RAReturnMessage>(
                termModel,
                MultiGeoOperationType.SaveTermGroup,
                async request =>
                {
                    return await TaxonomyService.UpdateTermGroupAsync(request.TermGroupId, this.replaceStr(request.TermGroupName), request.Description, request.ReSiteInfos, request.UsingMMSSpecified, request.M365TermSyncOption, request.GoogleTermSyncOption);
                },
                (request, _) =>
                {
                    BuildSaveTermGroupReplicaRequest(request);
                    return Task.CompletedTask;
                },
                _ => new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")
                });
        }

        [HttpGet]
        [ValidTermTreeParameterFilter("OperatorTerm")]
        public async Task<string> GetParentInhertSetting(int termId)
        {
            try
            {
                return await TaxonomyService.GetParentInhertSettingAsync(termId);
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while TaxonomyService.GetParentInhertSetting, termId:[{termId}], ERROR:{ex.ToString()}");
                return string.Empty;
            }
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("OperatorTerm")]
        public string GetTermTimeSettings([FromBody] int termId)
        {
            try
            {
                return TaxonomyService.GetTermTimeSettings(termId);
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while TaxonomyService.GetTermTimeSettings, termId:[{termId}], ERROR:{ex.ToString()}");
                return string.Empty;
            }
        }

        [HttpGet]
        public string GetParentTermTimeSettings(int termId)
        {
            try
            {
                return TaxonomyService.GetParentTermTimeSettings(termId);
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while TaxonomyService.GetParentTermTimeSettings, termId:[{termId}], ERROR:{ex.ToString()}");
                return string.Empty;
            }
        }

        [HttpGet]
        [ValidTermTreeParameterFilter("OperatorTerm")]
        public string GetParentSettingInfoByTermId(int termId)
        {
            try
            {
                return TaxonomyService.GetParentSettingInfoByTermId(termId);
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while TaxonomyService.GetParentSettingInfoByTermId, termId:[{termId}], ERROR:{ex.ToString()}");
                return string.Empty;
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.TermManagementAdmin)]
        public string RunEnforceRetentionJob()
        {
            return EnforceRetentionService.RunScheduleJob(JobRunBy.Control, JobType.EnforceRetention);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.TermManagementAdmin)]
        public string RunTeamsEnforceRetentionJob()
        {
            if (!TeamsPermissionHelper.HasUpgradeTeamsFeature()) return "The account does not upgrade teams";
            return EnforceRetentionService.RunTeamsScheduleJob(JobRunBy.Control, JobType.TeamsEnforceRetention);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.TermManagementAdmin)]
        public string RunEXOEnforceRetentionJob()
        {
            return EnforceRetentionService.RunEXOScheduleJob(JobRunBy.Control, JobType.EXOEnforceRetention);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.TermManagementAdmin)]
        public string RunOneDriveEnforceRetentionJob()
        {
            return EnforceRetentionService.RunOneDriveScheduleJob(JobRunBy.Control, JobType.OneDriveEnforceRetention);
        }

        [HttpGet]
        public string GetTermPermanent(int termId)
        {
            return TaxonomyService.GetTermPermanentByTermId(termId, false).ToString().ToLower();
        }

        [HttpGet]
        public string GetParentTermPermanent(int termId)
        {
            return TaxonomyService.GetTermPermanentByTermId(termId, true).ToString().ToLower();
        }

        #region AI recommendation
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.TermManagementAdmin)]
        public async Task<RAReturnMessage> AIRecommendation([FromForm] IFormFile fileUp, [FromForm] AIRecomentdation recomentdation)
        {
            var tenantGroupId = TenantLocalValue.LogonGroupId;
            bool isTenantAIEnabled = false;
            if (!string.IsNullOrEmpty(tenantGroupId) && TenantService.CheckTenantExist(tenantGroupId))
            {
                isTenantAIEnabled = await RMAosApiClient.IsEnableAIRecommendation(tenantGroupId);
            }
            if (!isTenantAIEnabled)
            {
                return new RAReturnMessage()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Not Enable AIRecommendation"
                };
            }
            if (string.IsNullOrEmpty(recomentdation.Industry))
            {
                return new RAReturnMessage()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Illegal Parameter"//Not display in UI, only for API
                };
            }
            if (!string.IsNullOrEmpty(recomentdation.Requirement) && recomentdation.Requirement.Length > 20000)
            {
                return new RAReturnMessage()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_TM_AI_Recommendations_RequirementValidate")
                };
            }
            var content = new List<string[]>();
            if (fileUp != null && fileUp.Length > 0)
            {
                using (var stream = fileUp.OpenReadStream())
                {
                    Dictionary<string, int> sheetNameCountDic = new Dictionary<string, int>
                    {
                        { "Terms", 6 }
                    };

                    var fileContent = ExcelUtil.ReadExcel(stream, sheetNameCountDic);
                    if (fileContent.TryGetValue("Terms", out var termContent))
                    {
                        if (termContent.Count > 0)
                        {
                            content = termContent;
                        }
                        else
                        {
                            return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_TM_AI_Recommendations_Template_Content") };
                        }
                    }
                    else
                    {
                        if (fileContent.Count > 0)
                        {
                            content = fileContent.First().Value;
                        }
                    }
                }
                recomentdation.FileName = fileUp.FileName;
            }
            recomentdation.FileContent = content;
            return await TaxonomyService.AIRecomendationAsync(recomentdation);
        }

        [HttpPost]
        public IActionResult DownloadTemplateAIRecommendation()
        {
            try
            {
                var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "Classification Scheme Template.xlsx");
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
        public async Task<IActionResult> ExportAIRecommendation()
        {
            try
            {
                var records = Request.Form["records"];
                var industry = Request.Form["industry"];
                var memoryStream = new MemoryStream();
                memoryStream = await TaxonomyService.GetStreamAIRecommendation(industry ,JsonConvert.DeserializeObject<List<RecordCategory>>(records));
                memoryStream.Position = 0;
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(memoryStream, contentType, $"{I18NEntity.GetString("RM_BCM_AI_Export_ExportFileName")}.xlsx");
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }
        #endregion
        private string replaceStr(string sourceStr)
        {
            string resultStr = "";
            if (!string.IsNullOrEmpty(sourceStr))
            {
                Regex reg = new Regex(@"[;<>|]+");
                sourceStr = reg.Replace(sourceStr.Trim(), "");
                if (!string.IsNullOrEmpty(sourceStr) && (sourceStr.Contains("&") || sourceStr.Contains("\"")))
                {
                    //替换成全角的
                    resultStr = sourceStr.Replace('&', '＆').Replace('"', '＂');
                }
                else
                {
                    resultStr = sourceStr;
                }
            }
            return resultStr;
        }


        [HttpGet]
        [ValidTermTreeParameterFilter("DeleteTermGroup")]
        public List<RMSiteInfo> GetRelativedMmsInfo(Guid termGroupId)
        {
            return TaxonomyService.GetRelativedSiteMMSInfo(termGroupId);
        }

        [HttpPost]
        public System.Threading.Tasks.Task<List<RMSiteInfo>> GetAllMmsInfo()
        {
            return TaxonomyService.GetRegisteredSiteMMSInfoAsync();
        }
        
        [HttpGet]
        [RMApiAuthorize(RMPermissionExtensionMasks.GoogleAdmin)]
        public async Task<List<RMSiteInfo>> GetAllGoogleTenants()
        {
            return await TaxonomyService.GetGoogleTermGroupSettingAsync();
        }
        
        [HttpGet]
        public async Task<Dictionary<string,string>> GetAllTermGroups()
        {
            return await TaxonomyService.GetAllTermGroups();
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)]
        public string GetTermWithPath(Guid termId)
        {
            return TaxonomyService.GetTermWithPathByTermId(termId);
        }

        [HttpPost]
        public async Task<string> GetMmsInfoByUrl([FromBody] string url)
        {
            var siteInfo = await TaxonomyService.GetRegisteredSiteMMSInfoByUrlAsync(url);
            if (siteInfo != null)
            {
                return JsonConvert.SerializeObject(siteInfo);
            }
            return string.Empty;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.TermManagementAdmin)]
        public async Task<string> ImportData()
        {
            return await RouteMultiGeoApiActionAsync<string, string>(
                string.Empty,
                MultiGeoOperationType.ImportTermAndRule,
                async _ =>
                {
                    string jobId = "";
                    try
                    {
                        var file = Request.Form.Files["fileUp"];
                        Logger.Info("tm import file,file name :{0}", file.FileName);
                        CheckFile(file);
                        string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                        DateTime dt = DateTime.Now;
                        string fileName = "Terms_" + dt.Ticks.ToString() + ".csv";
                        if (extension.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
                        {
                            fileName = "TermsAndRules_" + dt.Ticks.ToString() + ".xlsx";
                        }
                        else if (extension.Equals("xml", StringComparison.OrdinalIgnoreCase))
                        {
                            fileName = "Terms_" + dt.Ticks.ToString() + ".xml";
                        }
                        var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
                        RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
                        //string upFilePath = this.GetCsvPath();
                        //string fileNameAndPath = upFilePath + fileName;
                        //file.SaveAs(fileNameAndPath);
                        Trace.TraceError("save file success.");
                        if (TaxonomyService == null)
                        {
                            Trace.TraceError("TaxonomyService null.");
                        }
                        jobId = TaxonomyService.RunImportTermStructure(JobRunBy.Control, extension, blobName);
                        await SecurityTrimmingHelper.RemovePermissionCacheAsync();
                        RedisCacheService.CacheProvider.KeyDel(CacheKeyPrefix.SecurityTermCacheKeyPrefix + TenantLocalValue.LogonGroupId);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("error occurred import data:{0}", ex.ToString());
                    }
                    return jobId;
                },
                (_, __) => Task.CompletedTask,
                _ => string.Empty);

        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.TermManagementAdmin, RMPermissionExtensionMasks.GoogleAdmin)]
        public async Task<RAReturnMessage> ImportGoogleData([FromBody] RMGoogleTermGroupSetting setting)
        {
            return await GoogleJobService.RunImportGoogleTermStructure(JobRunBy.Control, setting);
        }

        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.TermManagementAdmin)]
        public async Task<IActionResult> DownLoadReport()
        {
            try
            {
                using (var scope = new PerformanceScope("download export term report"))
                {
                    string exportFlag = HttpUtility.UrlDecode(Request.Form["exportFlag"]);
                    Guid exportUniqueId = !string.IsNullOrEmpty(exportFlag) ? new Guid(exportFlag) : Guid.Empty;
                    TaxonomyService.CreateExportStatusRecord(exportUniqueId);
                    DateTime nowTime = DateTime.UtcNow;
                    string nowTimeStr = (await GeneralSettingService.ConvertTiksToDateTimeAsync(nowTime.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
                    string fileName = I18NEntity.GetString("RM_RC_Audit_Action_ExportTerm") + "_" + nowTimeStr;
                    string folderPath = JobReportUtility.GetDownloadTermInfoReportTempleFolder("Temple") + Path.DirectorySeparatorChar + fileName + Guid.NewGuid();
                    await TaxonomyService.GenerateReportForTermInfoAsync(folderPath, fileName, I18NEntity.GetString("RM_RC_RUR_TermDetail"));
                    AvePoint.GCommon.ZipUtil.ZipFolder(folderPath, folderPath + ".zip", Encoding.UTF8);
                    var memoryStream = new MemoryStream();
                    using (var stream = new FileStream(folderPath + ".zip", FileMode.Open, FileAccess.Read))
                    {
                        await stream.CopyToAsync(memoryStream);
                    }
                    memoryStream.Position = 0;
                    TaxonomyService.UpdateExportStatus(exportUniqueId, ExportTermsWithRulesStatus.Finished);
                    return File(memoryStream, GetContentType(folderPath + ".zip"), fileName + ".zip");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"download report error:{e.ToString()}");
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }

        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.TermManagementAdmin)]
        public async Task<RAReturnMessage> DownLoadReportJob()
        {
          return await TaxonomyService.RunExportTermStructure(JobRunBy.Control);
        }

        [HttpGet]
        public ExportTermsWithRulesStatus CheckExportTermStatus(Guid exportUniqueId)
        {
            return TaxonomyService.CheckExportStatus(exportUniqueId);
        }

        private void CheckFile(IFormFile file)
        {
            string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            var allowFileExts = new List<FileExtension> { FileExtension.CSV, FileExtension.XLSX, FileExtension.XML };
            WebUtil.CheckFileExtension(extension, allowFileExts);
            //WebUtil.CheckFileHeadCode(file.InputStream, allowFileExts);
        }

        private TermInfo BuildCreateTermReplicaRequest(TermInfo request)
        {
            if (TermDao.CheckTermExist(request.ParentTermId, request.TermName, request.TermSetId, out var termId))
            {
                request.TermUniqueId = TermDao.GetRMTermByTermId(termId)?.UniqueId ?? Guid.Empty;
            }

            request.TermSetUniqueId = ResolveTermSetUniqueId(request.TermSetId, request.TermSetUniqueId);
            request.ParentTermUniqueId = ResolveTermUniqueId(request.ParentTermId, request.ParentTermUniqueId);
            return request;
        }

        private TermInfo BuildRenameTermReplicaRequest(TermInfo request)
        {
            request.TermUniqueId = ResolveTermUniqueId(request.TermId, request.TermUniqueId);
            request.TermSetUniqueId = ResolveTermSetUniqueId(request.TermSetId, request.TermSetUniqueId);
            return request;
        }

        private TermInfo BuildRenameTermGroupReplicaRequest(TermInfo request)
        {
            var termGroupId = request.TermGroupId != 0 ? request.TermGroupId : request.TermId;
            request.TermGroupUniqueId = ResolveTermGroupUniqueId(termGroupId, request.TermGroupUniqueId);
            return request;
        }

        private TermInfo BuildRenameTermSetReplicaRequest(TermInfo request)
        {
            var termSetId = request.TermSetId != 0 ? request.TermSetId : request.TermId;
            request.TermSetUniqueId = ResolveTermSetUniqueId(termSetId, request.TermSetUniqueId);
            request.TermGroupUniqueId = ResolveTermGroupUniqueIdForTermSet(termSetId, request.TermGroupUniqueId);
            return request;
        }

        private TermInfo BuildCreateTermGroupReplicaRequest(TermInfo request)
        {
            var termGroup = TermGroupDao.GetTermGroupByName(this.replaceStr(request.TermGroupName));
            request.TermGroupUniqueId = termGroup?.UniqueId ?? Guid.Empty;
            return request;
        }

        private TermInfo BuildCreateTermSetReplicaRequest(TermInfo request)
        {
            request.TermGroupUniqueId = ResolveTermGroupUniqueId(request.TermGroupId, request.TermGroupUniqueId);
            request.TermSetUniqueId = TermSetDao.GetRMTermSetsByGroupUniqueIdAndTermSetName(request.TermGroupUniqueId, this.replaceStr(request.TermSetName))
                .OrderByDescending(termSet => termSet.Id)
                .FirstOrDefault()?.UniqueId ?? Guid.Empty;
            return request;
        }

        private TermInfo BuildSaveTermSetReplicaRequest(TermInfo request)
        {
            var termSetId = request.TermSetId != 0 ? request.TermSetId : request.TermId;
            request.TermSetUniqueId = ResolveTermSetUniqueId(termSetId, request.TermSetUniqueId);
            return request;
        }

        private TermInfo BuildSaveTermGroupReplicaRequest(TermInfo request)
        {
            request.TermGroupUniqueId = ResolveTermGroupUniqueId(request.TermGroupId, request.TermGroupUniqueId);
            return request;
        }

        private async Task<TermSettingsInfo> BuildTermSettingsReplicaRequest(TermSettingsInfo request)
        {
            request.TermUniqueId = ResolveTermUniqueId(request.tId, request.TermUniqueId);
            request.TimeZoneId = (await GeneralSetting).TimeZoneId;
            return request;
        }


        private Guid ResolveTermUniqueId(int termId, Guid termUniqueId)
        {
            if (termUniqueId != Guid.Empty)
            {
                return termUniqueId;
            }

            return termId > 0 ? TermDao.GetRMTermByTermId(termId)?.UniqueId ?? Guid.Empty : Guid.Empty;
        }

        private Guid ResolveTermSetUniqueId(int termSetId, Guid termSetUniqueId)
        {
            if (termSetUniqueId != Guid.Empty)
            {
                return termSetUniqueId;
            }

            return termSetId > 0 ? TermSetDao.GetRMTermSet(termSetId)?.UniqueId ?? Guid.Empty : Guid.Empty;
        }

        private Guid ResolveTermGroupUniqueId(int termGroupId, Guid termGroupUniqueId)
        {
            if (termGroupUniqueId != Guid.Empty)
            {
                return termGroupUniqueId;
            }

            return termGroupId > 0 ? TermGroupDao.GetTermGroupById(termGroupId)?.UniqueId ?? Guid.Empty : Guid.Empty;
        }

        private Guid ResolveTermGroupUniqueIdForTermSet(int termSetId, Guid termGroupUniqueId)
        {
            if (termGroupUniqueId != Guid.Empty)
            {
                return termGroupUniqueId;
            }

            return termSetId > 0 ? TermSetDao.GetRMTermSet(termSetId)?.TermGroupId ?? Guid.Empty : Guid.Empty;
        }

        private string getJsonByObj(object o)
        {
            return JsonConvert.SerializeObject(o);
        }
    }


}