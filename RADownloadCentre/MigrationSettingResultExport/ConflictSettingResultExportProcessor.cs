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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Teams;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using RADownloadCenter;
using RATeams.Upgrade.Module;
using System.Text;

namespace RADownloadCentre.MigrationSettingResultExport
{
    public class ConflictSettingResultExportProcessor : GenerateAndUploadFileExecutor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ConflictSettingResultExportProcessor));

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private static readonly ISharePointSettingDao s_sharePointSettingDao = PlatformWindsorManager.GetService<ISharePointSettingDao>();

        private static ITeamsChannelConflictSettingDao TeamsChannelConflictSettingDao => PlatformWindsorManager.GetService<ITeamsChannelConflictSettingDao>();

        private static ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();

        private readonly BaseJobDto BaseJobDto;

        private readonly string JobId;

        private readonly string FolderPath;

        private readonly string FilePath;

        private readonly int CountOfOneSheet = 65535;

        private readonly int PageSize = 1000;

        private readonly string ILSheetName = "Information lifecycle detail";

        private readonly string SOSheetName = "Storage optimization detail";

        private int currentSheetIndex = 0;

        protected override string BaseJobId => JobId;

        protected override ArchiverExportReportDto ExportReportDto => throw new NotImplementedException();

        private bool isCreateHeader = true;

        private Dictionary<string, string> workflowNameCache = new Dictionary<string, string>();

        public ConflictSettingResultExportProcessor(string jobId)
        {
            BaseJobDto = new BaseJobDto
            {
                Id = jobId,
                JobType = (int)JobType.ConflictSettingDetailExport
            };
            JobId = jobId;
            GenerateAndUploadFileManager.Init(JobId, JobType.ConflictSettingDetailExport);
            FolderPath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto);
            FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto,".xlsx");
            if(!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }

        protected override async Task GenerateDataAsync()
        {
            await GenerateScanSettingData(AvePoint.RA.Contract.Teams.ModuleType.LifeCycle);
            await GenerateScanSettingData(AvePoint.RA.Contract.Teams.ModuleType.SO);
        }

        private async Task GenerateScanSettingData(AvePoint.RA.Contract.Teams.ModuleType type)
        {
            var pageIndex = 0;
            var currentCount = 0;
            var sheetIndex = 0;
            var detailDatas = TeamsChannelConflictSettingDao.GetTeamsChannelConflictSettings(TenantLocalValue.LogonGroupId, type, PageSize, pageIndex);
            bool isFirstSheet = true;
            do
            {
                try
                {
                    currentCount += detailDatas.Count();
                    var datas = new string[detailDatas.Count() + 1][];
                    pageIndex++;
                    if (isCreateHeader)
                    {
                        datas = await GenerateConflictSettingsDetailDataAsync(datas, detailDatas, true);
                        ReportUtil.CreateExcel(FilePath, type == AvePoint.RA.Contract.Teams.ModuleType.SO ? SOSheetName : ILSheetName, datas);
                        Logger.Info($"Create Excel with header success,current count is {currentCount}, ModuleType is {type.ToString()}");
                        isFirstSheet = false;
                        isCreateHeader = false;
                        continue;
                    }

                    if (currentCount >= CountOfOneSheet || isFirstSheet)
                    {
                        if(isFirstSheet)
                        {
                            isFirstSheet = false;
                        }
                        else
                        {
                            sheetIndex++;
                        }
                        currentSheetIndex++;
                        datas = await GenerateConflictSettingsDetailDataAsync(datas, detailDatas, true);
                        ReportUtil.InsertWorksheet(FilePath, (type == AvePoint.RA.Contract.Teams.ModuleType.SO ? SOSheetName : ILSheetName) + (sheetIndex == 0 ? string.Empty : sheetIndex), datas);
                        currentCount = detailDatas.Count();
                        Logger.Info($"Insert Excel with header success,current count is {currentCount},current sheet index is {currentSheetIndex}, ModuleType is {type.ToString()}");
                        continue;
                    }

                    datas = await GenerateConflictSettingsDetailDataAsync(datas, detailDatas, false);
                    ReportUtil.InsertDataToSheet(FilePath, datas, currentSheetIndex);
                    Logger.Info($"Insert data to sheet success,current count is {currentCount},current sheet index is {currentSheetIndex}, ModuleType is {type.ToString()}");

                }
                catch (Exception e)
                {
                    Logger.Error($"Generate report detail to Excel error,current count is {currentCount}, current sheet index is {currentSheetIndex}, ModuleType is {type.ToString()},error : {e}");
                    GenerateAndUploadFileManager.HasFailed = true;
                    throw;
                }

            } while ((detailDatas = TeamsChannelConflictSettingDao.GetTeamsChannelConflictSettings(TenantLocalValue.LogonGroupId, type, PageSize, pageIndex)).Any());
            currentSheetIndex++;
        }

        private async Task<string[][]> GenerateConflictSettingsDetailDataAsync(string[][] datas, IEnumerable<TeamsChannelConflictSetting> detailDatas, bool isCreateHeader)
        {
            try
            {
                if (isCreateHeader)
                {
                    datas = AssembleConflictSettingsDetailHeaderTittle(datas);
                }
                return await ConvertConflictSettingDetailToArrayAsync(detailDatas, datas);
            }
            catch (Exception e)
            {
                Logger.Error($"Generate report for export job failed {e}");
                throw;
            }
        }

        private async Task<string[][]> ConvertConflictSettingDetailToArrayAsync(IEnumerable<TeamsChannelConflictSetting> detailDatas, string[][] datas)
        {
            int rowCount = 1;
            var settings = new List<RMSharePointSetting>();
            var lifeCycleDetails = detailDatas.Where(detail => detail.ModuleType == AvePoint.RA.Contract.Teams.ModuleType.LifeCycle);
            var soDetails = detailDatas.Where(detail => detail.ModuleType == AvePoint.RA.Contract.Teams.ModuleType.SO);

            if (lifeCycleDetails.Any())
            {
                var settingScopeIds = detailDatas.Where(item => !string.IsNullOrEmpty(item.SettingString)).Select(item => new Guid(item.SettingString)).ToList();
                settings = s_sharePointSettingDao.GetAllSettingsByScopeIds(settingScopeIds).ToList();
            }

            foreach (var detail in lifeCycleDetails)
            {
                if (detail == null) continue;
                try
                {
                    RMSharePointSetting? settingInfo = null;
                    if (!string.IsNullOrEmpty(detail.Id))
                    {
                        _ = int.TryParse(detail.Id, out var id);
                        settingInfo = settings.FirstOrDefault(setting => setting.ScopeId == new Guid(detail.SettingString) && setting.Id == id);
                    }
                    else
                    {
                        settingInfo = settings.FirstOrDefault(setting => setting.ScopeId == new Guid(detail.SettingString));
                    }
                    datas[rowCount] = new string[3];
                    datas[rowCount][0] = detail.FullPath ?? string.Empty;
                    datas[rowCount][1] = detail.IsConflict ? "TRUE" : "FALSE";
                    datas[rowCount][2] = BuildILSettingInfo(settingInfo) ?? string.Empty;
                    rowCount++;
                }
                catch (Exception e)
                {
                    Logger.Error($"Convert return history to array failed {e}");
                    rowCount++;
                    throw;
                }
            }

            foreach (TeamsChannelConflictSetting detail in soDetails)
            {
                try
                {
                    if (detail == null) continue;

                    datas[rowCount] = new string[3];
                    datas[rowCount][0] = detail.FullPath ?? string.Empty;
                    datas[rowCount][1] = detail.IsConflict ? "TRUE" : "FALSE";
                    datas[rowCount][2] = BuildSOSettingInfo(detail.SettingString) ?? string.Empty;
                    rowCount++;
                }
                catch (Exception e)
                {
                    Logger.Error($"Convert return history to array failed {e}");
                    rowCount++;
                    throw;
                }
            }
            return datas;
        }

        private string[][] AssembleConflictSettingsDetailHeaderTittle(string[][] datas)
        {
            datas[0] = new string[3];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_MigrateSettingDetailColumn_FullPath");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_MigrateSettingDetailColumn_IsConflict");
            datas[0][2] = I18NEntity.GetString("RM_JS_RC_MigrateSettingDetailColumn_Settings");
            return datas;
        }

        private string BuildILSettingInfo(RMSharePointSetting setting)
        {
            if (setting == null)
            {
                return string.Empty;
            }

            var teamsNodeILSettingInfo = new StringBuilder();
            teamsNodeILSettingInfo.AppendLine(BuildGeneralSettingInfo(setting));
            if (setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
            {
            teamsNodeILSettingInfo.AppendLine(BuildColumnSettingInfo(setting, false));
                if (!setting.IsUsingExistColumnName || (setting.IsUsingExistColumnName && setting.SetDocLevelTermForExistColumn))
                {
            teamsNodeILSettingInfo.AppendLine(BuildDocumentLevelSettingInfo(setting, false));
                }
            teamsNodeILSettingInfo.AppendLine(BuildContainerLevelSettingInfo(setting));
            teamsNodeILSettingInfo.AppendLine(BuildManualApprovalSettingInfo(setting));
            }
            return teamsNodeILSettingInfo.ToString();
        }

        private string BuildSOSettingInfo(string settingString)
        {
            if (string.IsNullOrEmpty(settingString))
            {
                return string.Empty;
            }

            var teamsNodeSOSetting = SerializerHelper.DeserializeByDataContractSerializer<SOTeamsNodeSetting>(settingString);
            var teamsNodeSOSettingInfo = new StringBuilder();
            teamsNodeSOSettingInfo.AppendLine(BuildSOGeneralSettingInfo(teamsNodeSOSetting));
            if(teamsNodeSOSetting.EnableArchiverManagement == (int)EnableRecordManagementSetting.Enable)
            {
            teamsNodeSOSettingInfo.AppendLine(BuildSOArchiveSettingInfo(teamsNodeSOSetting));
            }
            teamsNodeSOSettingInfo.ToString();
            return teamsNodeSOSettingInfo.ToString();
        }

        private string BuildGeneralSettingInfo(RMSharePointSetting nodeSetting)
        {
            var generalSettingString = new StringBuilder();
            var generalSettingTitle = I18NEntity.GetString("RM_JS_SPS_EditTitle_GeneralManagement");
            generalSettingString.AppendLine(generalSettingTitle);
            try
            {
                var enableManagementTitle = I18NEntity.GetString("RM_JS_SPS_EnableRecordsManagement");
                var enableManagementValue = nodeSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable
                    ? I18NEntity.GetString("RM_JS_Common_Yes")
                    : I18NEntity.GetString("RM_JS_Common_No");
                generalSettingString.AppendLine($"{enableManagementTitle}: {enableManagementValue}");
                if (nodeSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    var nodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(nodeSetting.NodeInfo);
                    if (nodeInfo.Level == (int)NodeLevel.WebApplication || nodeInfo.Level == (int)NodeLevel.SiteCollection)
                    {
                        var isSyncTitle = I18NEntity.GetString("RM_JS_SPS_EnableDataSync");
                        var isSyncValue = nodeSetting.IsSyncData
                            ? I18NEntity.GetString("RM_JS_Common_Yes")
                            : I18NEntity.GetString("RM_JS_Common_No");
                        generalSettingString.AppendLine($"{isSyncTitle}: {isSyncValue}");
                    }
                }
                return generalSettingString.ToString();
            }
            catch(Exception e)
            {
                Logger.Error($"Build General Setting Info failed, error: {e}");
                return generalSettingString.ToString();
            }

        }

        private string BuildColumnSettingInfo(RMSharePointSetting nodeSetting, bool isCSDTenant)
        {
            var columnSettingString = new StringBuilder();
            var columnSettingTitle = I18NEntity.GetString("RM_JS_SPS_EditTitle_ColumnSetting");
            columnSettingString.AppendLine(columnSettingTitle);
            try
            {
                var nodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(nodeSetting.NodeInfo);
                string GetKeepSpDefaultValueSettingContent(RMSharePointSetting setting)
                {
                    if (setting.IsKeepSharePointDefaultValue && setting.SetTermForEmptyDefaultValue)
                    {
                        return $"{I18NEntity.GetString("RM_JS_Common_Yes")}; {I18NEntity.GetString("RM_SPS_NoSetTermForEmptyDefaultValue_Title")}";
                    }
                    return setting.IsKeepSharePointDefaultValue
                        ? I18NEntity.GetString("RM_JS_Common_Yes")
                        : I18NEntity.GetString("RM_JS_Common_No");
                }

                bool HasConfigColumn(RMSharePointSetting setting)
                {
                    return !string.IsNullOrEmpty(setting.ColumnName) || setting.IsUsingExistColumnName;
                }

                if (nodeSetting.IsUsingExistColumnName)
                {
                    var enterColNameDesc = TrimEndColon(I18NEntity.GetString("RM_JS_SPS_EnterColNameDesc"));
                    columnSettingString.AppendLine($"{enterColNameDesc}: {nodeSetting.ExistColumnName}");

                    if (nodeSetting.SetDocLevelTermForExistColumn)
                    {
                        columnSettingString.AppendLine(string.Format(I18NEntity.GetString("RM_JS_SPS_ExistingColumn"),
                            I18NEntity.GetString("RM_JS_SPS_UseTermSettingsDefinedInRecords")));
                    }
                    else
                    {
                        columnSettingString.AppendLine(string.Format(I18NEntity.GetString("RM_JS_SPS_ExistingColumn"),
                            I18NEntity.GetString("RM_JS_SPS_UseTermSettingsDefinedInSP")));
                    }

                    var showUniqueIdTitle = I18NEntity.GetString("RM_JS_SPS_EditKey_ShowUniqueID");
                    var showUniqueIdValue = nodeSetting.IsShowUniqueId != null && (bool)nodeSetting.IsShowUniqueId
                        ? I18NEntity.GetString("RM_JS_Common_Yes")
                        : I18NEntity.GetString("RM_JS_Common_No");
                    columnSettingString.AppendLine($"{showUniqueIdTitle}: {showUniqueIdValue}");

                    if (!isCSDTenant)
                    {
                        var keepSpDefaultTitle = I18NEntity.GetString("RM_JS_SPS_EditKey_KeepSPDefaultValue");
                        var keepSpDefaultValue = GetKeepSpDefaultValueSettingContent(nodeSetting);
                        columnSettingString.AppendLine($"{keepSpDefaultTitle}: {keepSpDefaultValue}");
                    }

                    if (!Is21VEnv() && nodeInfo.Level == (int)NodeLevel.WebApplication)
                    {
                        var relatedRecordsTitle = I18NEntity.GetString("RM_SP_SettingRelatedRecords");
                        var relatedRecordsValue = nodeSetting.EnableRelatedRecords
                            ? I18NEntity.GetString("RM_JS_Common_Yes")
                            : I18NEntity.GetString("RM_JS_Common_No");
                        columnSettingString.AppendLine($"{relatedRecordsTitle}: {relatedRecordsValue}");
                    }
                }
                else
                {
                    var enterColNameDesc = TrimEndColon(I18NEntity.GetString("RM_JS_SPS_EnterColNameDesc"));
                    columnSettingString.AppendLine($"{enterColNameDesc}: {nodeSetting.ColumnName}");

                    var columnNameDescTitle = I18NEntity.GetString("RM_JS_SPS_EditKey_ColumnNameDescription");
                    columnSettingString.AppendLine($"{columnNameDescTitle}: {nodeSetting.Description}");

                    if (!isCSDTenant && HasConfigColumn(nodeSetting))
                    {
                        var hiddenColumnTitle = I18NEntity.GetString("RM_JS_SPS_HiddenColumn");
                        var hiddenColumnValue = nodeSetting.ColumnHidden != null && (bool)nodeSetting.ColumnHidden
                            ? I18NEntity.GetString("RM_JS_Common_Yes")
                            : I18NEntity.GetString("RM_JS_Common_No");
                        columnSettingString.AppendLine($"{hiddenColumnTitle}: {hiddenColumnValue}");
                    }

                    if (HasConfigColumn(nodeSetting))
                    {
                        var displayColumnRequiredTitle = TrimEndColon(I18NEntity.GetString("RM_JS_SPS_DisplayColumnRequired"));
                        var displayColumnRequiredValue = nodeSetting.ColumnRequired != null && (bool)nodeSetting.ColumnRequired
                            ? I18NEntity.GetString("RM_JS_Common_Yes")
                            : I18NEntity.GetString("RM_JS_Common_No");
                        columnSettingString.AppendLine($"{displayColumnRequiredTitle}: {displayColumnRequiredValue}");

                        var showUniqueIdTitle = I18NEntity.GetString("RM_JS_SPS_EditKey_ShowUniqueID");
                        var showUniqueIdValue = nodeSetting.IsShowUniqueId != null && (bool)nodeSetting.IsShowUniqueId
                            ? I18NEntity.GetString("RM_JS_Common_Yes")
                            : I18NEntity.GetString("RM_JS_Common_No");
                        columnSettingString.AppendLine($"{showUniqueIdTitle}: {showUniqueIdValue}");
                    }

                    if (!isCSDTenant && HasConfigColumn(nodeSetting))
                    {
                        var keepSpDefaultTitle = I18NEntity.GetString("RM_JS_SPS_EditKey_KeepSPDefaultValue");
                        var keepSpDefaultValue = GetKeepSpDefaultValueSettingContent(nodeSetting);
                        columnSettingString.AppendLine($"{keepSpDefaultTitle}: {keepSpDefaultValue}");
                    }

                    if (!Is21VEnv() && nodeInfo.Level == (int)NodeLevel.WebApplication && HasConfigColumn(nodeSetting))
                    {
                        var relatedRecordsTitle = I18NEntity.GetString("RM_SP_SettingRelatedRecords");
                        var relatedRecordsValue = nodeSetting.EnableRelatedRecords
                            ? I18NEntity.GetString("RM_JS_Common_Yes")
                            : I18NEntity.GetString("RM_JS_Common_No");
                        columnSettingString.AppendLine($"{relatedRecordsTitle}: {relatedRecordsValue}");
                    }
                }

                return columnSettingString.ToString();
            }
            catch (Exception e)
            {
                Logger.Error($"Build Column Setting Info failed, error: {e}");
                return columnSettingString.ToString();
            }
        }

        private string BuildDocumentLevelSettingInfo(RMSharePointSetting nodeSetting, bool isCSDTenant)
        {
            var docLevelSettingString = new StringBuilder();
            var docLevelSettingTitle = I18NEntity.GetString("RM_JS_SPS_EditTitle_DocumentLevelSetting");
            docLevelSettingString.AppendLine(docLevelSettingTitle);
            try
            {
                bool HasConfigDocumentLevel()
                {
                    return !GuidIsEmpty(nodeSetting.TermId) ||
                           !GuidIsEmpty(nodeSetting.TermSetId);
                }

                string GetDeployTermMethod()
                {
                    if (string.IsNullOrEmpty(nodeSetting.TermSetName))
                        return string.Empty;

                    switch (nodeSetting.DeployTermMethod)
                    {
                        case (int)DeployTermMethod.UseDefaultTerm:
                            return I18NEntity.GetString("RM_JS_SPS_AutoClassification_UseDefault");
                        case (int)DeployTermMethod.NoDefaultTerm:
                            return I18NEntity.GetString("RM_JS_SPS_AutoClassification_NoDefaultValue");
                        case (int)DeployTermMethod.UseAutoClassification:
                            return I18NEntity.GetString("RM_JS_SPS_AutoClassification_UseRule");
                        case (int)DeployTermMethod.UseIntelligenceClassification:
                            return I18NEntity.GetString("RM_MachineLearning_DeployTermMethodIntelligence");
                        default:
                            return string.Empty;
                    }
                }

                bool IsDefaultTermConfigured()
                {
                    return nodeSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm;
                }

                string ApplyActionString()
                {
                    if (nodeSetting.NeedCheckDefaultValue)
                    {
                        var includeString = nodeSetting.IncludeDeclaredRecords
                            ? "; " + I18NEntity.GetString("RM_JS_SPS_IncludeDeclaredRecords")
                            : "";

                        if (nodeSetting.ApplyExistType == (int)ApplyExistingTermType.OverWrite)
                        {
                            return I18NEntity.GetString("RM_JS_Common_Yes") +
                                   "; " + I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplyOverwirteTerm") +
                                   includeString;
                        }
                        else if (nodeSetting.ApplyExistType == (int)ApplyExistingTermType.SkipAndKeep)
                        {
                            return I18NEntity.GetString("RM_JS_Common_Yes") +
                                   "; " + I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplySkipTerm") +
                                   includeString;
                        }
                    }
                    return I18NEntity.GetString("RM_JS_Common_No");
                }

                bool IsAutoClassificationConfigured()
                {
                    return nodeSetting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification;
                }

                string GetSkipOverrideStr()
                {
                    switch (nodeSetting.AutoJobOption)
                    {
                        case (int)AutoJobOption.SkipAndKeep:
                            return I18NEntity.GetString("RM_JS_SPS_AutoClassification_SkipOverrideOption_Skip");
                        case (int)AutoJobOption.Override:
                            return I18NEntity.GetString("RM_JS_SPS_AutoClassification_SkipOverrideOption_Override");
                        default:
                            return string.Empty;
                    }
                }

                string ApplyDocumentSetsAndFoldersStringForDefault()
                {
                    if (nodeSetting.ApplyTermIncludeFolder != null && (bool)nodeSetting.ApplyTermIncludeFolder)
                    {
                        if (nodeSetting.ApplyExistType == (int)ApplyExistingTermType.OverWrite)
                        {
                            return I18NEntity.GetString("RM_JS_Common_Yes") +
                                   "; " + I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplySetsOverwirteTerm");
                        }
                        else if (nodeSetting.ApplyExistType == (int)ApplyExistingTermType.SkipAndKeep)
                        {
                            return I18NEntity.GetString("RM_JS_Common_Yes") +
                                   "; " + I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplySetsSkipTerm");
                        }
                    }
                    return I18NEntity.GetString("RM_JS_Common_No");
                }

                string ApplyDocumentSetsAndFoldersStringForAuto()
                {
                    return nodeSetting.ApplyTermIncludeFolder != null && (bool)nodeSetting.ApplyTermIncludeFolder
                        ? I18NEntity.GetString("RM_JS_Common_Yes")
                        : I18NEntity.GetString("RM_JS_Common_No");
                }

                bool IsAIClassificationConfigured()
                {
                    return nodeSetting.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification;
                }

                bool IsAIClassificationConfiguredOrAiInAuto()
                {
                    return IsAIClassificationConfigured() ||
                           nodeSetting.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault;
                }

                bool IsAIApprovalTypeSelectOwner()
                {
                    return nodeSetting.AIApprovalType == ApprovalType.RecordOwners;
                }

                var nodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(nodeSetting.NodeInfo);
                // Term Scope
                docLevelSettingString.AppendLine($"{I18NEntity.GetString("RM_JS_SPS_EditKey_TermScope")}: {nodeInfo.TermScopeFullPath}");
                if (nodeInfo.IsTermRemoved)
                {
                    docLevelSettingString.AppendLine(I18NEntity.GetString("RM_JS_SPS_TermDelete"));
                }
                else if (nodeInfo.IsTermDeprecated)
                {
                    docLevelSettingString.AppendLine(I18NEntity.GetString("RM_JS_SPS_IsTermRetired"));
                }

                // Term Display Form (if not folder)
                if (!IsFolder(nodeInfo))
                {
                    var termDisplayTitle = I18NEntity.GetString("RM_JS_SPS_EditKey_TermDisplayForm");
                    var termDisplayValue = nodeSetting.IsDisplyaTermPath
                        ? I18NEntity.GetString("RM_SPS_DisplayTerm_EntirePath")
                        : I18NEntity.GetString("RM_SPS_DisplayTerm_TermLabel");
                    docLevelSettingString.AppendLine($"{termDisplayTitle}: {termDisplayValue}");
                }

                // Deploy Term Method
                var deployMethodTitle = TrimEndColon(I18NEntity.GetString("RM_SPS_AutoClassification_DeployTermMethod"));
                docLevelSettingString.AppendLine($"{deployMethodTitle}: {GetDeployTermMethod()}");

                // Default Term Configuration
                if (IsDefaultTermConfigured())
                {
                    // Default Value
                    var defaultValueTitle = I18NEntity.GetString("RM_JS_SPS_EditKey_DefaultValue");
                    docLevelSettingString.AppendLine($"{defaultValueTitle}: {nodeInfo.DefaultTermFullPath}");
                    if (nodeInfo.IsDefaultTermRemoved)
                    {
                        docLevelSettingString.AppendLine(I18NEntity.GetString("RM_JS_SPS_TermDelete"));
                    }
                    else if (nodeInfo.IsDefaultTermDeprecated)
                    {
                        docLevelSettingString.AppendLine(I18NEntity.GetString("RM_JS_SPS_IsTermRetired"));
                    }

                    // Action
                    var actionTitle = I18NEntity.GetString("RM_JS_SPS_EditKey_Action");
                    docLevelSettingString.AppendLine($"{actionTitle}: {ApplyActionString()}");
                }

                // Auto Classification Configuration
                if (IsAutoClassificationConfigured())
                {
                    // Note: The complex renderConditionsCriteria() from React would need to be simplified 
                    // or handled differently in C# since we're just building a string
                    var applyPolicyTitle = TrimEndColon(I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplyPolicy"));
                    docLevelSettingString.AppendLine($"{applyPolicyTitle}:");
                    List<ClassificationRule>? rules = nodeSetting.AutoClassificationRules == null ?
                        null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(nodeSetting.AutoClassificationRules);
                    if (rules != null)
                    {
                        for (int i = 1; i < rules.Count; i++)
                        {
                            docLevelSettingString.AppendLine(BuildClassificationConditionString(rules[i]));
                        }
                        docLevelSettingString.AppendLine(BuildClassificationConditionString(rules[0]));
                    }
                    string BuildClassificationConditionString(ClassificationRule rule)
                    {
                        StringBuilder condition = new StringBuilder();
                        void GetClassificationCondition(FilterGroup currentGroup)
                        {
                            if (currentGroup.Filters != null && currentGroup.Filters.Count > 0)
                            {
                                foreach (var filter in currentGroup.Filters)
                                {
                                    condition.AppendLine(filter.FilterCretia);
                                }
                            }
                            if (currentGroup.FilterGroups != null && currentGroup.FilterGroups.Count > 0)
                            {
                                foreach (var group in currentGroup.FilterGroups)
                                {
                                    GetClassificationCondition(group);
                                }
                            }
                        }
                        if (!rule.IsDefaultRule)
                        {
                            foreach (var group in rule.FilterGroups)
                            {
                                GetClassificationCondition(group);
                            }
                            if (rule.AndOrExpression.Length == 1)
                            {
                                condition.AppendLine($"({rule.AndOrExpression})");
                            }
                            else
                            {
                                condition.AppendLine(rule.AndOrExpression);
                            }
                            condition.AppendLine($"{I18NEntity.GetString("RM_JS_SPS_AutoClassification_DisplayPolicyApplyTerm")} {rule.TermName ?? string.Empty}");
                        }
                        else
                        {
                            if(!rule.NoDefaultTerm)
                                condition.AppendLine($"{I18NEntity.GetString("RM_JS_SPS_AutoClassification_DisplayPolicyDefaultTerm")} {rule.TermName}");
                        }
                        return condition.ToString();
                    }
                    // Include Declared Records
                    var includeDeclaredTitle = I18NEntity.GetString("RM_JS_SPS_EditKey_IncludeDeclaredRecords");
                    var includeDeclaredValue = nodeSetting.IncludeDeclaredRecords
                        ? I18NEntity.GetString("RM_JS_Common_Yes")
                        : I18NEntity.GetString("RM_JS_Common_No");
                    docLevelSettingString.AppendLine($"{includeDeclaredTitle}: {includeDeclaredValue}");
                }

                // AI Classification Configuration
                if (IsAIClassificationConfiguredOrAiInAuto())
                {
                    if (IsAIApprovalTypeSelectOwner() && nodeInfo.AIReviewers != null)
                    {
                        var reviewersTitle = I18NEntity.GetString("RM_MachineLearning_IntelligenceReviewers");
                        var reviewers = string.Join(", ", nodeInfo.AIReviewers.Select(r => r.DisplayName));
                        docLevelSettingString.AppendLine($"{reviewersTitle}: {reviewers}");
                    }

                    var sendEmailTitle = TrimEndColon(I18NEntity.GetString("RM_JS_MA_IsSendEmail"));
                    var sendEmailValue = nodeSetting.AISendEMail
                        ? I18NEntity.GetString("RM_JS_Common_Yes")
                        : I18NEntity.GetString("RM_JS_Common_No");
                    docLevelSettingString.AppendLine($"{sendEmailTitle}: {sendEmailValue}");

                    if (nodeSetting.AIThenIsDefaultTermMethod)
                    {
                        var defaultTermTitle = I18NEntity.GetString("RM_MachineLearning_IntelligenceDefaultTerm");
                        docLevelSettingString.AppendLine($"{defaultTermTitle}: {nodeSetting.AIThenDefaultTermName}");
                    }
                }

                // Skip/Override Option (for both Auto and AI Classification)
                if (IsAutoClassificationConfigured() || IsAIClassificationConfigured())
                {
                    var skipOverrideTitle = TrimEndColon(I18NEntity.GetString("RM_SPS_AutoClassification_SkipOverrideOption"));
                    docLevelSettingString.AppendLine($"{skipOverrideTitle}: {GetSkipOverrideStr()}");

                    var fullJobTitle = TrimEndColon(I18NEntity.GetString("RM_SPS_AutoClassification_FullJobDescription"));
                    var fullJobValue = nodeSetting.RunAutoFullJob
                        ? I18NEntity.GetString("RM_JS_Common_Yes")
                        : I18NEntity.GetString("RM_JS_Common_No");
                    docLevelSettingString.AppendLine($"{fullJobTitle}: {fullJobValue}");
                }

                // Document Sets and Folders (if not CSD Tenant)
                if (!isCSDTenant && (IsDefaultTermConfigured() || IsAutoClassificationConfigured()))
                {
                    var includeDsetsTitle = I18NEntity.GetString("RM_JS_SPS_Expander_IncludeDSetAndFolder");
                    string includeDsetsValue;

                    if (IsAutoClassificationConfigured() || IsAIClassificationConfigured())
                    {
                        includeDsetsValue = ApplyDocumentSetsAndFoldersStringForAuto();
                    }
                    else
                    {
                        includeDsetsValue = ApplyDocumentSetsAndFoldersStringForDefault();
                    }

                    docLevelSettingString.AppendLine($"{includeDsetsTitle}: {includeDsetsValue}");
                }

                // Related Records (for certain environments and node types)
                if (!Is21VEnv() &&
                    (IsSiteCollection(nodeInfo) ||
                     IsSite(nodeInfo) ||
                     IsTeams(nodeInfo)))
                {
                    var relatedRecordsTitle = I18NEntity.GetString("RM_SP_SettingRelatedRecords");
                    var relatedRecordsValue = nodeSetting.EnableRelatedRecords
                        ? I18NEntity.GetString("RM_JS_Common_Yes")
                        : I18NEntity.GetString("RM_JS_Common_No");
                    docLevelSettingString.AppendLine($"{relatedRecordsTitle}: {relatedRecordsValue}");
                }

                return docLevelSettingString.ToString();
            }
            catch (Exception e)
            {
                Logger.Error($"Build Document Level Setting Info failed, error: {e}");
                return docLevelSettingString.ToString();
            }

        }

        private string BuildContainerLevelSettingInfo(RMSharePointSetting nodeSetting)
        {
            var containerSettingString = new StringBuilder();
            var containerSettingTitle = I18NEntity.GetString("RM_JS_SPS_EditTitle_ContainerLevelTermSetting");
            containerSettingString.AppendLine(containerSettingTitle);
            try
            {
                var nodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(nodeSetting.NodeInfo);
                // Common term path display logic
                void AppendTermPathInfo()
                {
                    containerSettingString.AppendLine($"{TrimEndColon(I18NEntity.GetString("RM_JS_BCM_Explorer_Details_TermName"))}: {nodeInfo.ContainerTermFullPath}");

                    if (nodeInfo.IsClassificationTermRemoved)
                    {
                        containerSettingString.AppendLine(I18NEntity.GetString("RM_JS_SPS_TermDelete"));
                    }
                    else if (nodeInfo.IsClassificationTermDeprecated)
                    {
                        containerSettingString.AppendLine(I18NEntity.GetString("RM_JS_SPS_IsTermRetired"));
                    }
                }

                    // Enable Classification
                    var enableClassificationTitle = TrimEndColon(I18NEntity.GetString("RM_JS_SPS_EditKey_EnableClassification"));
                var enableClassificationValue = nodeInfo.Level == 2 ? I18NEntity.GetString("RM_JS_Common_No") : 
                    (nodeSetting.isEnableClassification ? I18NEntity.GetString("RM_JS_Common_Yes")
                    : I18NEntity.GetString("RM_JS_Common_No"));
                    containerSettingString.AppendLine($"{enableClassificationTitle}: {enableClassificationValue}");

                    // Term name/path
                    AppendTermPathInfo();

                    // Description
                    var descriptionTitle = TrimEndColon(I18NEntity.GetString("RM_JS_SPS_EditKey_ColumnNameDescription"));
                    containerSettingString.AppendLine($"{descriptionTitle}: {nodeSetting.DescriptionOfContainer}");

                //Inherit parent classification
                var inheritParentClassificationTitle = TrimEndColon(I18NEntity.GetString("RM_JS_SPS_ContainerLevel_InheritParentTerm"));
                containerSettingString.AppendLine($"{inheritParentClassificationTitle}: {(nodeSetting.IsInheritParentTerm ? I18NEntity.GetString("RM_JS_Common_Yes")
                    : I18NEntity.GetString("RM_JS_Common_No"))}");
                return containerSettingString.ToString();
            }
            catch (Exception e)
            {
                Logger.Error($"Build Container Level Setting Info failed, error: {e}");
                return containerSettingString.ToString();
            }
            
        }

        private string BuildManualApprovalSettingInfo(RMSharePointSetting nodeSetting)
        {

            var approvalSettingString = new StringBuilder();
            var approvalSettingTitle = I18NEntity.GetString("RM_BCM_ManualApproval_Title_ManualApprovalSettings");
            approvalSettingString.AppendLine(approvalSettingTitle);
            try
            {
                var nodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(nodeSetting.NodeInfo);
                // Handle user list rendering
                string RenderUserSetting()
                {
                    if (nodeInfo.RecordOwner == null || nodeInfo.RecordOwner.Count == 0)
                        return string.Empty;

                    return string.Join(", ", nodeInfo.RecordOwner.Select(u => u.DisplayName));
                }

                switch (nodeSetting.ApprovalType)
                {
                    case ApprovalType.None:
                        var enableApprovalTitle = I18NEntity.GetString("RM_BCM_ManualApproval_Title_EnableApproval");
                        approvalSettingString.AppendLine($"{enableApprovalTitle}: {I18NEntity.GetString("RM_JS_Common_No")}");
                        break;

                    case ApprovalType.ApprovalProcess:
                    case ApprovalType.RecordOwners:
                        var sendEmailTitle = TrimEndColon(I18NEntity.GetString("RM_JS_MA_IsSendEmail"));
                        var sendEmailValue = nodeSetting.EMailToRecordOwner
                            ? I18NEntity.GetString("RM_JS_Common_Yes")
                            : I18NEntity.GetString("RM_JS_Common_No");
                        approvalSettingString.AppendLine($"{sendEmailTitle}: {sendEmailValue}");

                        if (nodeSetting.ApprovalType == ApprovalType.ApprovalProcess)
                        {
                            var processTitle = I18NEntity.GetString("RM_BCM_ManualApproval_Title_Process");
                            if (workflowNameCache.ContainsKey(nodeInfo.WorkflowReferenceId))
                            {
                                nodeInfo.WorkflowReferenceName = workflowNameCache[nodeInfo.WorkflowReferenceId];
                            }else
                            {
                                var result = Guid.TryParse(nodeSetting.WorkflowReferenceId, out var referenceId);
                                if (result)
                                {
                                    var workflow = ManualProcessManagementService.GetWorkflow(referenceId);
                                    nodeInfo.WorkflowReferenceName = workflow?.Name;
                                    workflowNameCache[nodeSetting.WorkflowReferenceId] = workflow?.Name ?? string.Empty;
                                }
                                else
                                {
                                    Logger.Info($"Can not parse guid workflow {nodeSetting.WorkflowReferenceId}");
                                    nodeInfo.WorkflowReferenceName = string.Empty;
                                    workflowNameCache[nodeSetting.WorkflowReferenceId] = string.Empty;
                                }
                            }
                            approvalSettingString.AppendLine($"{processTitle}: {nodeInfo.WorkflowReferenceName}");
                        }
                        else // SelectOwnerRecords
                        {
                            var ownersTitle = TrimEndColon(I18NEntity.GetString("RM_SPS_RecordOwners"));
                            approvalSettingString.AppendLine($"{ownersTitle}: {RenderUserSetting()}");
                        }
                        break;

                    case ApprovalType.AutoApproval:
                        var autoApproveTitle = I18NEntity.GetString("RM_BCM_ManualApproval_Detail_AutoApprove");
                        approvalSettingString.AppendLine($"{autoApproveTitle}: {I18NEntity.GetString("RM_JS_Common_Yes")}");
                        break;
                }

                return approvalSettingString.ToString();
            }
            catch(Exception e)
            {
                Logger.Error($"Build Manual Approval Setting Info failed, error: {e}");
                return approvalSettingString.ToString();
            }
        }

        private string BuildSOGeneralSettingInfo(SOTeamsNodeSetting nodeSetting)
        {
            var sb = new StringBuilder();
            sb.AppendLine(I18NEntity.GetString("RM_JS_SPS_EditTitle_GeneralManagement"));
            try
            {
                var enableArchiveTitle = I18NEntity.GetString("RM_AR_SPS_General_EnableArchiveManagement");
                var enableArchiveValue = nodeSetting.EnableArchiverManagement == (int)EnableRecordManagementSetting.Enable
                    ? I18NEntity.GetString("RM_JS_Common_Yes")
                    : I18NEntity.GetString("RM_JS_Common_No");
                sb.AppendLine($"{enableArchiveTitle}: {enableArchiveValue}");
                return sb.ToString();
            }
            catch(Exception e)
            {
                Logger.Error($"Build SO General Setting Info failed, error: {e}");
                return sb.ToString();
            }
        }

        private string BuildSOArchiveSettingInfo(SOTeamsNodeSetting nodeSetting)
        {
            var sb = new StringBuilder();
            sb.AppendLine(I18NEntity.GetString("RM_AR_SPS_EditTitle_ArchiveSetting"));

            try
            {
                string GetSettingRuleNames()
                {
                    if (nodeSetting.Rules != null && nodeSetting.Rules.Count > 0)
                    {
                        var ruleNames = new StringBuilder();
                        for (int i = 0; i < nodeSetting.Rules.Count; i++)
                        {
                            ruleNames.AppendLine($"{i + 1}. {nodeSetting.Rules[i].RuleName}");
                        }
                        return ruleNames.ToString();
                    }
                    return string.Empty;
                }

                sb.AppendLine(I18NEntity.GetString("RM_JS_SPS_RuleNames_Title") + ":");
                sb.Append(GetSettingRuleNames());
                string GetOptions()
                {
                    var options = new List<string>();

                    if (nodeSetting.isIncludeManagedMetadataService)
                    {
                        options.Add(I18NEntity.GetString("RM_AR_SPS_Options_Managed"));
                    }
                    if (nodeSetting.isEnableSuperUserDecrypt)
                    {
                        options.Add(I18NEntity.GetString("RM_AR_SPS_Options_SuperUser"));
                    }
                    if (nodeSetting.isEnableRemoveRetentionLabel)
                    {
                        options.Add(I18NEntity.GetString("RM_AR_SPS_Options_Remove_RetentionLabel"));
                    }

                    return string.Join("; ", options);
                }

                sb.AppendLine(I18NEntity.GetString("RM_AR_SPS_Title_Options") + ": " + GetOptions());
                return sb.ToString();
            }
            catch(Exception e)
            {
                Logger.Error($"Build SO Archive Setting Info failed, error: {e}");
            }
            return sb.ToString();
        }

        private bool Is21VEnv()
        {
            return "21V China North".Equals(RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME], StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSiteCollection(RMSPTreeNode node) =>
            node != null && node.Level == (int)NodeLevel.SiteCollection;

        private static bool IsSite(RMSPTreeNode node) =>
            node != null && node.Level == (int)NodeLevel.Site;        
        
        private static bool IsTeams(RMSPTreeNode node) =>
            node != null && node.Level == (int)NodeLevel.Office365GroupEntire;

        private bool IsFolder(RMSPTreeNode node)
        {
            if (node == null)
                return false;

            return node.Level == (int)NodeLevel.Folder
                || node.Level == (int)NodeLevel.Folders
                || node.Level == (int)NodeLevel.RootFolder;
        }

        private static bool GuidIsEmpty(Guid guid) => guid == Guid.Empty;

        private static string TrimEndColon(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return System.Text.RegularExpressions.Regex.Replace(input, ":+$", "");
        }

        protected async override Task UploadBlobAsync()
        {
            AvePoint.GCommon.ZipUtil.ZipFolder(FolderPath, FolderPath + ".zip", Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = SecurityUtils.SafeCombinePath(customId, JobId + ".zip");//Path.Combine(customId, JobId + ".zip");
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, FolderPath + ".zip");
                    Logger.Info($"Upload conflict setting result export success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Upload conflict setting result export failed,error is :{e}");
                throw;
            }

            Logger.Info($"finish to upload blob name:{blobName}");
            fileInfo = new FileInfo(FolderPath + ".zip");
        }
    }
}
