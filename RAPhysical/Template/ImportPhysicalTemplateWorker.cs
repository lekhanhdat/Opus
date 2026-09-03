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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using Cloud.Sdk.Telemetry.Data.Alita;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Template
{
    public class ImportPhysicalTemplateWorker
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(ImportPhysicalTemplateWorker));

        private static readonly ITemplateManagementService _templateManagementService = PlatformWindsorManager.GetService<ITemplateManagementService>();

        private static readonly IRMTemplateRelationshipDao _templateRelationshipDao = PlatformWindsorManager.GetService<IRMTemplateRelationshipDao>();

        private static readonly IPhysicalUniqueIdSettingDao _physicalUniqueIdSettingDao = PlatformWindsorManager.GetService<IPhysicalUniqueIdSettingDao>();

        private static readonly IRMTemplateDao _templateDao = PlatformWindsorManager.GetService<IRMTemplateDao>();

        private static readonly IRMSuiteDao _suiteDao = PlatformWindsorManager.GetService<IRMSuiteDao>();

        private readonly string Structure_Column_SuiteName = "Template suite name";
        private readonly string Structure_Column_StartFrom = "Start from (Box/Folder)";
        private readonly string Structure_Column_BoxTemplateName = "Box template name";
        private readonly string Structure_Column_FolderTemplateName = "Folder template name";
        private readonly string Structure_Column_RecordTemplateName = "Record template name";
        private readonly string Structure_Column_PhysicalType = "Physical item type (Box/Folder/Record)";
        private readonly string Structure_Column_UniqueIDPrefix = "Prefix for unique ID";
        private readonly string Structure_Column_UniqueIDDigits = "Number of digits in unique ID";

        private readonly string Template_Column_TemplateName = "Template name";
        private readonly string Template_Column_TemplateType = "Physical item type (Box/Folder/Record)";
        private readonly string Template_Column_TemplateColumnName = "Column name";
        private readonly string Template_Column_TemplateColumnCategory = "Column category";
        private readonly string Template_Column_TemplateColumnType = "Mapped Opus column type";
        private readonly string Template_Column_TemplateColumnRequired = "Required (Y/N)";
        private readonly string Template_Column_TemplateColumnValue = "Options of Choice type column";

        private static readonly string _templateStructure = "Template Structure";

        private static readonly string _templateColumns = "Template Columns";

        private static readonly string[] _separator = ["; "];

        private static readonly Dictionary<string, int> StructurecolumnIndexDic = [];

        private static readonly Dictionary<string, int> TemplateColumnIndexDic = [];

        private static List<StructrueObejct> StructureList = [];

        private static List<TemplateColumnObject> TemplateColumnList = [];

        private static List<SimplifySuiteDto> AllSuites = [];

        private readonly bool _isGlobleUniqueIDSetting = false;

        private readonly string _filePath = "";

        public ImportPhysicalTemplateWorker(string jobId, string blobName)
        {
            var setting = _physicalUniqueIdSettingDao.LoadingUniqueIdSetting();
            _isGlobleUniqueIDSetting = (setting?.IsGlobalSetting).GetValueOrDefault();
            ImportPhysicalTemplateManager.Init(jobId, _isGlobleUniqueIDSetting);
            AllSuites = _templateManagementService.LoadAllSuites();
            _filePath = JobReportUtility.GetImportJobCSVFile(blobName);
            ReadImportExcel();
        }

        public async Task RunAsync()
        {
            if (StructureList.IsNullOrEmpty())
            {
                _logger.Error("Please check if the imported template is right.");
                ImportPhysicalTemplateManager.SetJobFailed("RM_Phy_TemplateImport_ImportFileFormatFailed");
                return;
            }

            var suiteGroup = StructureList.GroupBy(item => item.SuiteName).ToDictionary(item => item.Key, item => item.ToList());
            foreach (var suite in suiteGroup)
            {
                try
                {
                    if (suite.Value.GroupBy(item => item.StartFrom).Count() > 1)
                    {
                        _logger.Error("A suite cannot contain more than one start from");
                        ImportPhysicalTemplateManager.AddSuiteFailedDetail(suite.Key, GetSuiteStartFrom(suite.Value[0].StartFrom), "RM_Phy_TemplateImport_MultipleStartFrom");
                        continue;
                    }

                    _logger.Info($"Start to process suite [{suite.Key}], start from is [{suite.Value[0].StartFrom}] , template count is [{suite.Value.Count}]");
                    var suiteName = suite.Key;
                    var startFrom = GetSuiteStartFrom(suite.Value[0].StartFrom);
                    var templateInSuite = suite.Value;
                    var suiteUniqueId = Guid.Empty;
                    var existSuite = AllSuites.Where(suite => suite.Name.Equals(suiteName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (existSuite == null)
                    {
                        _logger.Info($"Current suite [{suite.Key}] can not be fount, create new.");
                        suiteUniqueId = CreateNewSuite(suiteName, startFrom);
                    }
                    else
                    {
                        if (existSuite.StartFrom != startFrom)
                        {
                            _logger.Error("The start from the imported Suite is inconsistent with the start from the existing suite.");
                            ImportPhysicalTemplateManager.AddSuiteFailedDetail(suite.Key, startFrom, "RM_Phy_TemplateImport_DifferentStartFrom");
                            continue;
                        }
                        suiteUniqueId = existSuite.UniqueId;
                    }

                    if (startFrom == SuiteStartFromType.Box)
                    {
                        await ProcessStartFromBoxTemplate(templateInSuite, suiteUniqueId, suiteName);
                    }
                    else if (startFrom == SuiteStartFromType.Folder)
                    {
                        await ProcessStartFromFolderTemplate(templateInSuite, suiteUniqueId, [suiteUniqueId.ToString()], suiteName, SuiteStartFromType.Folder);
                    }
                    else
                    {
                        _logger.Error("Cannot manipulate a suite of types other than Box/Folder");
                        ImportPhysicalTemplateManager.AddSuiteFailedDetail(suite.Key, startFrom, "RM_Phy_TemplateImport_StartFromTypeFailed");
                    }
                }
                catch (StartFromTypeException)
                {
                    _logger.Error($"Process suite [{suite.Key}] failed, error: current suite not support");
                    ImportPhysicalTemplateManager.AddSuiteFailedDetail(suite.Key, SuiteStartFromType.None, "RM_Phy_TemplateImport_StartFromTypeNotSupport");
                }
                catch (Exception e)
                {
                    _logger.Error($"Process suite [{suite.Key}] failed, error: {e}");
                }
            }

            ImportPhysicalTemplateManager.SetJobFinished();
        }
        private async Task ProcessStartFromBoxTemplate(List<StructrueObejct> templateInSuite, Guid suiteUnqiueId, string suiteName)
        {
            var boxTemplateGroup = templateInSuite
                        .GroupBy(template => template.BoxTemplateName ?? "")
                        .ToDictionary(item => item.Key, item => item.ToList());

            var allBoxTemplateIds = _templateRelationshipDao.GetAllByParent(suiteUnqiueId, [suiteUnqiueId.ToString()]);
            var allBoxTemplateNames = (await _templateDao.FindListAsync(o => allBoxTemplateIds.Contains(o.UniqueId))).Select(o => o.Name).ToList();
            var boxTemplateIndex = allBoxTemplateIds.Count;

            foreach (var boxTemplate in boxTemplateGroup)
            {
                try
                {
                    var boxTemplateName = boxTemplate.Key;
                    var boxStructureList = boxTemplate.Value;

                    if (boxTemplateName.IsNullOrEmpty())
                    {
                        _logger.Error($"Current box template name is empty, suite id: [{suiteUnqiueId}], please check import file");
                        ImportPhysicalTemplateManager.AddFailedDetail(suiteName, boxTemplateName, TemplateType.Box, SuiteStartFromType.Box, "RM_Phy_TemplateImport_TemplateNameEmpty");
                        continue;
                    }

                    var currentBoxTemplate = boxStructureList.Where(box => "Box".Equals(box.PhysicalType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if(currentBoxTemplate == null)
                    {
                        _logger.Error($"Current box template type is wrong, suite id: [{suiteUnqiueId}], please check import file");
                        ImportPhysicalTemplateManager.AddFailedDetail(suiteName, boxTemplateName, TemplateType.Box, SuiteStartFromType.Box, "RM_Phy_TemplateImport_TemplateTypeError");
                        continue;
                    }
                    _logger.Info($"Start to process box template [{boxTemplate.Key}], suite id [{suiteUnqiueId}]");
                    if(boxTemplateIndex > 0)
                    {
                        _logger.Info($"Current suite start from box, can only import one box template, suite id: [{suiteUnqiueId}], so skip");
                        ImportPhysicalTemplateManager.AddSkippedDetail(currentBoxTemplate, TemplateType.Box, "RM_Phy_TemplateImport_MultipleBoxTemplate");
                        continue;
                    }

                    if (allBoxTemplateNames.Any(name => boxTemplateName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.Info($"Current box template [{boxTemplate.Key}] exist in suite, suite id: [{suiteUnqiueId}], so skip");
                        ImportPhysicalTemplateManager.AddSkippedDetail(currentBoxTemplate, TemplateType.Box, "RM_Phy_TemplateImport_TemplateExist");
                        continue;
                    }

                    var boxTemplatePrefix = currentBoxTemplate.UniqueIDPrefix;
                    var boxTemplateDigits = currentBoxTemplate.UniqueIDDigits;
                    if (_isGlobleUniqueIDSetting)
                    {
                        boxTemplatePrefix = string.Empty;
                        boxTemplateDigits = string.Empty;
                    }
                    var newBoxTemplate = await CreateTemplate(AllSuites.Where(suite => suite.UniqueId == suiteUnqiueId).FirstOrDefault(), boxTemplateName, TemplateType.Box, boxTemplatePrefix, boxTemplateDigits, [suiteUnqiueId.ToString()], SuiteStartFromType.Box);
                    allBoxTemplateNames.Add(newBoxTemplate.Name);
                    _logger.Info($"Start to process folder template under box template: [{boxTemplate.Key}], suite id [{suiteUnqiueId}]");
                    await ProcessStartFromFolderTemplate(boxStructureList, suiteUnqiueId, [suiteUnqiueId.ToString(), newBoxTemplate.Id.ToString()], suiteName, SuiteStartFromType.Box);
                    boxTemplateIndex++;
                }
                catch (Exception e)
                {
                    _logger.Error($"Process box template [{boxTemplate.Key}] failed, error: {e}");
                }
            }
        }

        private async Task ProcessStartFromFolderTemplate(List<StructrueObejct> templateInSuite, Guid suiteUnqiueId, List<string> parentList, string suiteName, SuiteStartFromType startFromType)
        {
            var folderTemplateGroup = templateInSuite.Where(template => !"Box".Equals(template.PhysicalType, StringComparison.OrdinalIgnoreCase))
                    .GroupBy(template => template.FolderTemplateName ?? "")
                    .ToDictionary(template => template.Key, template => template.ToList());

            var allFolderTemplateIds = _templateRelationshipDao.GetAllByParent(suiteUnqiueId, parentList);
            var allFolderTemplateNames = (await _templateDao.FindListAsync(o => allFolderTemplateIds.Contains(o.UniqueId))).Select(o => o.Name).ToList();
            var folderTemplateIndex = allFolderTemplateIds.Count;

            foreach (var folderTemplate in folderTemplateGroup)
            {
                try
                {
                    var folderTemplateName = folderTemplate.Key;
                    var folderStructureList = folderTemplate.Value;

                    if (folderTemplateName.IsNullOrEmpty())
                    {
                        _logger.Error($"Current folder template name is empty, suite id: [{suiteUnqiueId}], please check import file");
                        ImportPhysicalTemplateManager.AddFailedDetail(suiteName, folderTemplateName, TemplateType.Folder, startFromType, "RM_Phy_TemplateImport_TemplateNameEmpty");
                        continue;
                    }

                    var currentFolderTemplate = folderStructureList.Where(folder => "Folder".Equals(folder.PhysicalType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (currentFolderTemplate == null)
                    {
                        _logger.Error($"Current box template type is wrong, suite id: [{suiteUnqiueId}], please check import file");
                        ImportPhysicalTemplateManager.AddFailedDetail(suiteName, folderTemplateName, TemplateType.Folder, startFromType, "RM_Phy_TemplateImport_TemplateTypeError");
                        continue;
                    }
                    _logger.Info($"Start to process box template [{folderTemplateName}], suite id [{suiteUnqiueId}]");
                    if(startFromType == SuiteStartFromType.Folder && folderTemplateIndex > 0)
                    {
                        _logger.Info($"Current suite start from folder, can only import one folder template, folder template [{folderTemplateName}], suite id: [{suiteUnqiueId}], so skip");
                        ImportPhysicalTemplateManager.AddSkippedDetail(currentFolderTemplate, TemplateType.Folder, "RM_Phy_TemplateImport_MultipleFolderTemplate");
                        folderTemplateIndex++;
                        continue;
                    }

                    if (allFolderTemplateNames.Any(name => folderTemplateName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.Info($"Current folder template [{folderTemplate.Key}] exist in box template, suite id: [{suiteUnqiueId}], so skip");
                        ImportPhysicalTemplateManager.AddSkippedDetail(currentFolderTemplate, TemplateType.Folder, "RM_Phy_TemplateImport_TemplateExist");
                        continue;
                    }


                    var folderTemplatePrefix = currentFolderTemplate.UniqueIDPrefix;
                    var folderTemplateDigits = currentFolderTemplate.UniqueIDDigits;
                    if (_isGlobleUniqueIDSetting)
                    {
                        folderTemplatePrefix = string.Empty;
                        folderTemplateDigits = string.Empty;
                    }
                    var newFolderTemplate = await CreateTemplate(AllSuites.Where(suite => suite.UniqueId == suiteUnqiueId).FirstOrDefault(), folderTemplateName, TemplateType.Folder, folderTemplatePrefix, folderTemplateDigits, parentList, startFromType);
                    allFolderTemplateNames.Add(newFolderTemplate.Name);
                    folderTemplateIndex++;
                    _logger.Info($"Start to process record template under folder template: [{folderTemplate.Key}], suite id [{suiteUnqiueId}]");
                    var recordTemplateList = folderStructureList.Where(template => !"Folder".Equals(template.PhysicalType, StringComparison.OrdinalIgnoreCase));

                    var allRecordTemplateIds = _templateRelationshipDao.GetAllByParent(suiteUnqiueId, [.. parentList, newFolderTemplate.Id.ToString()]);
                    var allRecordTemplateNames = (await _templateDao.FindListAsync(o => allRecordTemplateIds.Contains(o.UniqueId))).Select(o => o.Name).ToList();

                    foreach (var recordTemplate in recordTemplateList)
                    {
                        try
                        {
                            var recordTemplateName = recordTemplate.RecordTemplateName;

                            if (recordTemplateName.IsNullOrEmpty())
                            {
                                _logger.Error($"Current record template name is empty, suite id: [{suiteUnqiueId}], please check import file");
                                ImportPhysicalTemplateManager.AddFailedDetail(recordTemplate, TemplateType.Records, "RM_Phy_TemplateImport_TemplateNameEmpty");
                                continue;
                            }

                            if (!"Record".Equals(recordTemplate.PhysicalType, StringComparison.OrdinalIgnoreCase))
                            {
                                _logger.Error($"Current box template type is wrong, suite id: [{suiteUnqiueId}], please check import file");
                                ImportPhysicalTemplateManager.AddFailedDetail(suiteName, recordTemplateName, TemplateType.Records, startFromType, "RM_Phy_TemplateImport_TemplateTypeError");
                                continue;
                            }

                            if (allRecordTemplateNames.Any(name => recordTemplateName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                            {
                                _logger.Info($"Current record template [{recordTemplateName}] exist in folder template, suite id: [{suiteUnqiueId}], so skip");
                                continue;
                            }

                            var recordTemplatePrefix = recordTemplate.UniqueIDPrefix;
                            var recordTemplateDigits = recordTemplate.UniqueIDDigits;
                            if (_isGlobleUniqueIDSetting)
                            {
                                recordTemplatePrefix = string.Empty;
                                recordTemplateDigits = string.Empty;
                            }

                            var newRecordTemplate = await CreateTemplate(AllSuites.Where(suite => suite.UniqueId == suiteUnqiueId).FirstOrDefault(), recordTemplateName, TemplateType.Records, recordTemplatePrefix, recordTemplateDigits, [.. parentList, newFolderTemplate.Id.ToString()], SuiteStartFromType.None);
                            allRecordTemplateNames.Add(newRecordTemplate.Name);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Process reocrd template failed, error: [{e}]");
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"Process folder template failed, error: [{e}]");
                }
            }
        }

        private async Task<SimplifyTemplateDto> CreateTemplate(SimplifySuiteDto suiteInfo, string templateName, TemplateType templateType, string templatePrefix, string templateDigits, List<string> parentList, SuiteStartFromType startFromType)
        {
            _logger.Info($"Start to create template, template name: [{templateName}], template type: [{templateType}], prefix: [{templatePrefix}], digits: [{templateDigits}], parent list :[{string.Join("-> ", parentList)}]");
            var currentTemplateInfo = TemplateColumnList.Where(item => templateName.Equals(item.TemplateName, StringComparison.OrdinalIgnoreCase));

            if (currentTemplateInfo.Any(templateInfo => 
                !"Box".Equals(templateInfo.TemplateType, StringComparison.OrdinalIgnoreCase) 
                && !"Folder".Equals(templateInfo.TemplateType, StringComparison.OrdinalIgnoreCase)
                && !"Record".Equals(templateInfo.TemplateType, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.Error($"Create template [{templateName}] failed, error: get template column failed.");
                ImportPhysicalTemplateManager.AddFailedDetail(suiteInfo.Name, templateName, templateType, suiteInfo.StartFrom, "RM_Phy_TemplateImport_TemplateColumnTypeError");
                throw new Exception();
            }

            var currentTemplateCategors = TemplateColumnList.Where(item => templateName.Equals(item.TemplateName, StringComparison.OrdinalIgnoreCase) && GetTemplateType(item.TemplateType) == templateType)
                .GroupBy(item => item.TemplateColumnCategory ?? "")
                .ToDictionary(item => item.Key, item => item.ToList());// && GetTemplateType(item.TemplateType) == templateType

            var templateDto = _templateManagementService.GetDefaultCategoryAndColumn((int)templateType);
            templateDto.name = templateName;
            templateDto.type = templateType;
            templateDto.prefix = templatePrefix;
            templateDto.numberOfDigits = int.TryParse(templateDigits, out var value) ? value : 0;
            templateDto.ParentTemplateIdList = parentList;

            if (!_isGlobleUniqueIDSetting)
            {
                try
                {
                    CheckTemplateUniqueData(templateDto.prefix, templateDigits);
                }
                catch (UniqueIdPrefixException)
                {
                    _logger.Error($"Create template [{templateName}] failed, error: unique id prefix format error.");
                    ImportPhysicalTemplateManager.AddFailedDetail(suiteInfo, templateDto, "RM_Phy_TemplateImport_UniqueIdPrefixFormatError");
                    throw;
                }
                catch (UniqueIdDigitsException)
                {
                    _logger.Error($"Create template [{templateName}] failed, error: unique id digits format error.");
                    ImportPhysicalTemplateManager.AddFailedDetail(suiteInfo, templateDto, "RM_Phy_TemplateImport_UniqueIdDigitsFormatError");
                    throw;
                }
            }

            try
            {
                foreach (var category in currentTemplateCategors)
                {
                    var categoryName = category.Key;
                    var columnInfoes = category.Value;

                    _logger.Info($"Start to build category, category name: [{categoryName}], cloumn count : [{columnInfoes.Count}]");
                    if (templateType == TemplateType.Box || templateType == TemplateType.Folder || templateType == TemplateType.Records)
                    {
                        int basicIndex = -1;

                        if (string.IsNullOrEmpty(categoryName) || categoryName.Equals(I18NEntity.GetString("RM_Template_Cagegory_Name_Basic"), StringComparison.OrdinalIgnoreCase))
                        {
                            basicIndex = 0;
                        }
                        else if (templateType == TemplateType.Folder || templateType == TemplateType.Records)
                        {
                            if (categoryName.Equals(I18NEntity.GetString("RM_Template_Cagegory_Name_Classification"), StringComparison.OrdinalIgnoreCase))
                            {
                                basicIndex = 1;
                            }
                            else if (categoryName.Equals(I18NEntity.GetString("RM_Template_Cagegory_Name_Statement"), StringComparison.OrdinalIgnoreCase))
                            {
                                basicIndex = 2;
                            }
                        }

                        if (basicIndex != -1)
                        {
                            _logger.Info($"Start to add custom column to default category name: [{categoryName}]");
                            templateDto.categories[basicIndex].columns = BuildTemplateCustomColumn(templateDto.categories[basicIndex].id, columnInfoes, templateDto.categories[basicIndex].columns);
                        }
                        else
                        {
                            _logger.Info($"Start to add custom column to custom category name: [{categoryName}]");
                            var customCategory = BuildTemplateCustomCategore(category.Key, category.Value);
                            templateDto.categories.Add(customCategory);
                        }
                    }
                }

                var checkResult = await _templateManagementService.CheckTemplateBeforeSavingAsync(templateDto.uniqueId, templatePrefix, templateName, templateType, parentList);
                if (checkResult == SaveTemplateResult.None)
                {
                    var result = _templateManagementService.SaveTemplateWithColumns(templateDto);
                    if (result.result)
                    {
                        ImportPhysicalTemplateManager.AddSuccessdDetail(suiteInfo, templateDto);
                        return new()
                        {
                            Name = result.dto.name,
                            Id = result.dto.id,
                        };
                    }

                    throw new Exception("RM_Phy_TemplateImport_SaveTemplateFailed");
                }
                else if (checkResult == SaveTemplateResult.PrefixDuplicate)
                {
                    throw new PrefixDuplicateException();
                }
                else if(checkResult == SaveTemplateResult.NameDuplicate)
                {
                    _logger.Info($"Curremt template [{templateName}] exist in other suite, add exist template to this suite [{suiteInfo.Name}]");
                    var otherTemplate = _templateDao.Find(template => templateName.Equals(template.Name, StringComparison.OrdinalIgnoreCase));

                    if(otherTemplate != null)
                    {
                        if (otherTemplate.Type != templateType)
                        {
                            throw new SameNameDifferentTypeException();
                        }

                        if(templateType == TemplateType.Box && startFromType == SuiteStartFromType.Box)
                        {
                            throw new StartFromAddExsitingTemplateException();
                        }

                        if(templateType == TemplateType.Folder && startFromType == SuiteStartFromType.Folder)
                        {
                            throw new StartFromAddExsitingTemplateException();
                        }
                    }

                    _templateManagementService.AddExistingTemplates(new()
                    {
                        Ids = [otherTemplate.UniqueId],
                        TemplateIdList = parentList,
                        SuiteId = suiteInfo.UniqueId,
                    });
                    ImportPhysicalTemplateManager.AddSuccessdDetail(suiteInfo, new TemplateDto { name = otherTemplate.Name, type = otherTemplate.Type, prefix = otherTemplate.Prefix, numberOfDigits = (int)otherTemplate.NumberOfDigits });
                    return new()
                    {
                        Name = otherTemplate.Name,
                        Id = otherTemplate.Id,
                    };
                }

                throw new Exception("RM_Phy_TemplateImport_SaveTemplateFailed");
            }
            catch (BuildColumnOptionException)
            {
                _logger.Error($"Create template [{templateName}] failed, error: column type error.");
                ImportPhysicalTemplateManager.AddFailedDetail(suiteInfo, templateDto, "RM_Phy_TemplateImport_ColumnOptionError");
                throw;
            }
            catch (ColumnTypeException)
            {
                _logger.Error($"Create template [{templateName}] failed, error: column type error.");
                ImportPhysicalTemplateManager.AddFailedDetail(suiteInfo, templateDto, "RM_Phy_TemplateImport_ColumnTypeError");
                throw;
            }
            catch (BuildColumnException)
            {
                _logger.Error($"Create template [{templateName}] failed, error: build column failed.");
                ImportPhysicalTemplateManager.AddFailedDetail(suiteInfo, templateDto, "RM_Phy_TemplateImport_BuildColumnFailed");
                throw;
            }
            catch(PrefixDuplicateException)
            {
                _logger.Error($"Create template [{templateName}] failed, error: prefix is duplicate.");
                ImportPhysicalTemplateManager.AddFailedDetail(suiteInfo, templateDto, "RM_Template_DuplicatePrefix");
                throw;
            }
            catch (ColumnDuplicateException)
            {
                _logger.Error($"Create template [{templateName}] failed, error: column name is duplicate.");
                ImportPhysicalTemplateManager.AddFailedDetail(suiteInfo, templateDto, "RM_Phy_TemplateImport_TemplateColumnDuplicate");
                throw;
            }
            catch (ColumnEmptyException)
            {
                _logger.Error($"Create template [{templateName}] failed, error: column name is empty.");
                ImportPhysicalTemplateManager.AddFailedDetail(suiteInfo, templateDto, "RM_Phy_TemplateImport_TemplateColumnEmpty");
                throw;
            }            
            catch (SameNameDifferentTypeException)
            {
                _logger.Error($"Create template [{templateName}] failed, error: exist template name with different type.");
                ImportPhysicalTemplateManager.AddFailedDetail(suiteInfo, templateDto, "RM_Phy_TemplateImport_SameNameDifferentType");
                throw;
            }            
            catch (StartFromAddExsitingTemplateException)
            {
                _logger.Error($"Create template [{templateName}] failed, error: start from template can not add existing template.");
                ImportPhysicalTemplateManager.AddFailedDetail(suiteInfo, templateDto, "RM_Phy_TemplateImport_StartFromAddExsitingTemplate");
                throw;
            }
            catch (Exception e)
            {
                _logger.Error($"Create template [{templateName}] failed, error: [{e}]");
                ImportPhysicalTemplateManager.AddFailedDetail(suiteInfo, templateDto, e.Message);
                throw;
            }
        }

        private static TemplateCategoryDto BuildTemplateCustomCategore(string categoryName, List<TemplateColumnObject> columnInfo)
        {
            var categoryId = Guid.NewGuid();
            var customCategoreList = new TemplateCategoryDto
            {
                id = categoryId,
                name = categoryName,
                allowEdit = true,
                columns = BuildTemplateCustomColumn(categoryId, columnInfo, [])
            };
            return customCategoreList;
        }

        private static List<TemplateColumnDto> BuildTemplateCustomColumn(Guid categoryId, List<TemplateColumnObject> columnInfoes, List<TemplateColumnDto> existColumns)
        {
            try
            {
                foreach (var columnInfo in columnInfoes)
                {
                    _logger.Info($"Start to build custom column, column name: [{columnInfo.TemplateColumnName}]");
                    if (columnInfo.TemplateColumnName.IsNullOrEmpty())
                    {
                        _logger.Error($"Current column is empty, category id: [{categoryId}]");
                        throw new ColumnEmptyException();
                    }
                    if (existColumns.Any(column => I18NEntity.GetString(column.columnName).Equals(columnInfo.TemplateColumnName, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.Error($"Current column [{columnInfo.TemplateColumnName}] exist in category, category id: [{categoryId}], so skip");
                        throw new ColumnDuplicateException();
                    }
                    var columnDto = new TemplateColumnDto();
                    var columnType = GetColumnType(columnInfo.TemplateColumnType);
                    columnDto.categoryId = categoryId;
                    columnDto.columnName = columnInfo.TemplateColumnName;
                    columnDto.typeId = (int)columnType;
                    columnDto.required = columnInfo.TemplateColumnRequired;
                    columnDto.optionsJSON = JsonConvert.SerializeObject(new Dictionary<int, string>());
                    columnDto.optionsMaxIdReachedValue = 1;
                    if (columnType == ColumnType.SingleChoice || columnType == ColumnType.MultipleChoice)
                    {
                        var optionDic = BuildColumnOptionDictionary(columnInfo.TemplateColumnValue);
                        columnDto.optionsJSON = JsonConvert.SerializeObject(optionDic);
                        columnDto.optionsMaxIdReachedValue = optionDic.Count;
                    }
                    columnDto.categoryId = categoryId;
                    columnDto.allowEdit = true;
                    columnDto.allowEditSort = GetIfAllowEditSort(columnType);
                    columnDto.allowSort = false;
                    columnDto.uniqueId = Guid.NewGuid();
                    existColumns.Add(columnDto);
                }

                return existColumns;
            }
            catch (BuildColumnOptionException)
            {
                throw;
            }
            catch(ColumnTypeException)
            {
                throw;
            }
            catch (ColumnDuplicateException)
            {
                throw;
            }
            catch(ColumnEmptyException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.Error($"Build template custom column failed, error: {e}");
                throw new BuildColumnException();
            }
        }
        private static void CheckTemplateUniqueData(string prefix, string digits)
        {
            prefix = prefix.Trim();
            if (string.IsNullOrEmpty(prefix) || prefix.Length > 10)//max length 10
            {
                throw new UniqueIdPrefixException();
            }
            Regex regForNumberOfDigits = new Regex("(^[2-9]$)|(^1[0-5]$)", RegexOptions.None, RecordsConstants.REGEX_DEFAULT_MATCH_TIMEOUT);//2-15
            if (!regForNumberOfDigits.IsMatch(digits))
            {
                throw new UniqueIdDigitsException();
            }
        }
        private static bool GetIfAllowEditSort(ColumnType columnType)
        {
            if (columnType == ColumnType.SingleText || columnType == ColumnType.DateTime || columnType == ColumnType.SingleChoice || columnType == ColumnType.Number)
            {
                return true;
            }

            return false;
        }
        private static Dictionary<int, string> BuildColumnOptionDictionary(string optionInfo)
        {
            var optionDic = new Dictionary<int, string>();
            string[] optionArray = optionInfo.Split(_separator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < optionArray.Length; i++)
            {
                string option = optionArray[i].Trim();
                if (optionDic.ContainsValue(option))
                {
                    throw new BuildColumnOptionException();
                }
                optionDic.Add(i, option);
            }

            return optionDic;
        }
        private List<StructrueObejct> ConvertToStructrueObejct(IEnumerable<string[]> structrueInfo)
        {
            var structrueObejctList = new List<StructrueObejct>();
            foreach (var structrue in structrueInfo)
            {
                if (structrue[StructurecolumnIndexDic[Structure_Column_SuiteName]].IsNullOrEmpty() || structrue[StructurecolumnIndexDic[Structure_Column_StartFrom]].IsNullOrEmpty())
                {
                    continue;
                }
                structrueObejctList.Add(new()
                {
                    SuiteName = structrue[StructurecolumnIndexDic[Structure_Column_SuiteName]],
                    StartFrom = structrue[StructurecolumnIndexDic[Structure_Column_StartFrom]],
                    BoxTemplateName = structrue[StructurecolumnIndexDic[Structure_Column_BoxTemplateName]],
                    FolderTemplateName = structrue[StructurecolumnIndexDic[Structure_Column_FolderTemplateName]],
                    RecordTemplateName = structrue[StructurecolumnIndexDic[Structure_Column_RecordTemplateName]],
                    PhysicalType = structrue[StructurecolumnIndexDic[Structure_Column_PhysicalType]],
                    UniqueIDPrefix = structrue[StructurecolumnIndexDic[Structure_Column_UniqueIDPrefix]],
                    UniqueIDDigits = structrue[StructurecolumnIndexDic[Structure_Column_UniqueIDDigits]],
                });
            }
            return structrueObejctList;
        }
        private List<TemplateColumnObject> ConvertToTemplateColumnObject(IEnumerable<string[]> templateColumnInfo)
        {
            var templateCloumnObjectList = new List<TemplateColumnObject>();
            foreach (var templateColumn in templateColumnInfo)
            {
                if (templateColumn[TemplateColumnIndexDic[Template_Column_TemplateName]].IsNullOrEmpty() || templateColumn[TemplateColumnIndexDic[Template_Column_TemplateType]].IsNullOrEmpty())
                {
                    continue;
                }
                templateCloumnObjectList.Add(new()
                {
                    TemplateName = templateColumn[TemplateColumnIndexDic[Template_Column_TemplateName]],
                    TemplateType = templateColumn[TemplateColumnIndexDic[Template_Column_TemplateType]],
                    TemplateColumnName = templateColumn[TemplateColumnIndexDic[Template_Column_TemplateColumnName]],
                    TemplateColumnCategory = templateColumn[TemplateColumnIndexDic[Template_Column_TemplateColumnCategory]],
                    TemplateColumnType = templateColumn[TemplateColumnIndexDic[Template_Column_TemplateColumnType]],
                    TemplateColumnRequired = "Y".Equals(templateColumn[TemplateColumnIndexDic[Template_Column_TemplateColumnRequired]], StringComparison.OrdinalIgnoreCase)
                        || "Yes".Equals(templateColumn[TemplateColumnIndexDic[Template_Column_TemplateColumnRequired]], StringComparison.OrdinalIgnoreCase),
                    TemplateColumnValue = templateColumn[TemplateColumnIndexDic[Template_Column_TemplateColumnValue]]
                });
            }
            return templateCloumnObjectList;
        }
        private static TemplateType GetTemplateType(string templateType)
        {
            return templateType switch
            {
                "Box" => TemplateType.Box,
                "Folder" => TemplateType.Folder,
                "Record" => TemplateType.Records,
                _ => throw new TemplateTypeException(),
            };
        }
        private static SuiteStartFromType GetSuiteStartFrom(string startFrom)
        {
            return startFrom switch
            {
                "Box" => SuiteStartFromType.Box,
                "Folder" => SuiteStartFromType.Folder,
                _ => throw new StartFromTypeException(),
            };
        }
        private static ColumnType GetColumnType(string columnType)
        {
            return columnType switch
            {
                "Single line of text" => ColumnType.SingleText,
                "Multiple lines of text" => ColumnType.MultipleText,
                "Date and Time" => ColumnType.DateTime,
                "Choice (single selection)" => ColumnType.SingleChoice,
                "Choice (multiple selections)" => ColumnType.MultipleChoice,
                "Person or Group" => ColumnType.PeopleOrGroup,
                "Number" => ColumnType.Number,
                _ => throw new ColumnTypeException(),
            };
        }
        private void GetStructureHeaderIndexInfo(string[] header)
        {
            try
            {
                for (int i = 0; i < header.Length; i++)
                {
                    switch (header[i])
                    {
                        case var columnName when Structure_Column_SuiteName.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            StructurecolumnIndexDic.TryAdd(Structure_Column_SuiteName, i);
                            break;
                        case var columnName when Structure_Column_StartFrom.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            StructurecolumnIndexDic.TryAdd(Structure_Column_StartFrom, i);
                            break;
                        case var columnName when Structure_Column_BoxTemplateName.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            StructurecolumnIndexDic.TryAdd(Structure_Column_BoxTemplateName, i);
                            break;
                        case var columnName when Structure_Column_FolderTemplateName.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            StructurecolumnIndexDic.TryAdd(Structure_Column_FolderTemplateName, i);
                            break;
                        case var columnName when Structure_Column_RecordTemplateName.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            StructurecolumnIndexDic.TryAdd(Structure_Column_RecordTemplateName, i);
                            break;
                        case var columnName when Structure_Column_PhysicalType.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            StructurecolumnIndexDic.TryAdd(Structure_Column_PhysicalType, i);
                            break;
                        case var columnName when Structure_Column_UniqueIDPrefix.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            StructurecolumnIndexDic.TryAdd(Structure_Column_UniqueIDPrefix, i);
                            break;
                        case var columnName when Structure_Column_UniqueIDDigits.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            StructurecolumnIndexDic.TryAdd(Structure_Column_UniqueIDDigits, i);
                            break;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        private void GetTemplateHeaderIndexInfo(string[] header)
        {
            try
            {
                for (int i = 0; i < header.Length; i++)
                {
                    switch (header[i])
                    {
                        case var columnName when Template_Column_TemplateName.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            TemplateColumnIndexDic.TryAdd(Template_Column_TemplateName, i);
                            break;
                        case var columnName when Template_Column_TemplateType.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            TemplateColumnIndexDic.TryAdd(Template_Column_TemplateType, i);
                            break;
                        case var columnName when Template_Column_TemplateColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            TemplateColumnIndexDic.TryAdd(Template_Column_TemplateColumnName, i);
                            break;
                        case var columnName when Template_Column_TemplateColumnCategory.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            TemplateColumnIndexDic.TryAdd(Template_Column_TemplateColumnCategory, i);
                            break;
                        case var columnName when Template_Column_TemplateColumnType.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            TemplateColumnIndexDic.TryAdd(Template_Column_TemplateColumnType, i);
                            break;
                        case var columnName when Template_Column_TemplateColumnRequired.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            TemplateColumnIndexDic.TryAdd(Template_Column_TemplateColumnRequired, i);
                            break;
                        case var columnName when Template_Column_TemplateColumnValue.Equals(columnName, StringComparison.OrdinalIgnoreCase):
                            TemplateColumnIndexDic.TryAdd(Template_Column_TemplateColumnValue, i);
                            break;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        private static Guid CreateNewSuite(string suiteName, SuiteStartFromType startFrom)
        {
            try
            {
                var suiteDto = new SuiteDto
                {
                    UniqueId = Guid.NewGuid(),
                    Name = suiteName,
                    StartFromType = startFrom,
                    RootTemplateCreateType = SuiteRootTemplateCreateType.New,
                };
                var createResult = _suiteDao.CreateSuite(suiteDto);
                if (createResult)
                {
                    AllSuites.Add(new SimplifySuiteDto { UniqueId = suiteDto.UniqueId, Name = suiteDto.Name, StartFrom = startFrom });//
                    return suiteDto.UniqueId;
                }

                throw new Exception($"Create new suite [{suiteName}] failed.");
            }
            catch (Exception e)
            {
                _logger.Error($"Create new suite [{suiteName}] failed, error: {e}");
                throw;
            }
        }
        private void ReadImportExcel()
        {
            var datas = new Dictionary<string, List<string[]>>();
            try
            {
                using var fs = new FileStream(_filePath, FileMode.Open);
                datas = ExcelUtil.ReadExcelWithHeader(fs);

                _ = datas.TryGetValue(_templateStructure, out var structureInfo);
                _ = datas.TryGetValue(_templateColumns, out var templateColumnInfo);
                var structureHeader = structureInfo[0];
                var templateHeader = templateColumnInfo[0];
                GetStructureHeaderIndexInfo(structureHeader);
                GetTemplateHeaderIndexInfo(templateHeader);

                StructureList = ConvertToStructrueObejct(structureInfo.Skip(1));
                TemplateColumnList = ConvertToTemplateColumnObject(templateColumnInfo.Skip(1));
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);
                ImportPhysicalTemplateManager.SetJobFailed("RM_Phy_TemplateImport_ReadImportFileFailed");
                throw new Exception("Failed to read file conntent");
            }
        }
    }
    public class StructrueObejct
    {
        public string SuiteName { get; set; }
        public string StartFrom { get; set; }
        public string BoxTemplateName { get; set; }
        public string FolderTemplateName { get; set; }
        public string RecordTemplateName { get; set; }
        public string PhysicalType { get; set; }
        public string UniqueIDPrefix { get; set; }
        public string UniqueIDDigits { get; set; }
    }
    public class TemplateColumnObject
    {
        public string TemplateName { get; set; }
        public string TemplateType { get; set; }
        public string TemplateColumnName { get; set; }
        public string TemplateColumnCategory { get; set; }
        public string TemplateColumnType { get; set; }
        public bool TemplateColumnRequired { get; set; }
        public string TemplateColumnValue { get; set; }
    }

    public class SimplifyTemplateDto
    {
        public string Name { get; set; }
        
        public int Id { get; set; }
    }
}
