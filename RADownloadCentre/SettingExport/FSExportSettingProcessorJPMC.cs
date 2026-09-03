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
using AngleSharp.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.FileSystemRegister.JPMC;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;

namespace RADownloadCentre.SettingExport
{
    public class FSExportSettingProcessorJPMC : FSExportSettingProcessorBase
    {
        #region JPMC columns
        private string GroupNameColumn = I18NEntity.GetString("RM_JS_BCM_Export_GroupNameColumn");
        private string PathColumn = I18NEntity.GetString("RM_JS_BCM_Export_PathColumn");
        private string ConnectionNameColumn2 = I18NEntity.GetString("RM_JS_BCM_Export_ConnectionNameColumn");
        private string EnableILColumn = I18NEntity.GetString("RM_FS_Export_EnableILColumn");
        private string AllowDownloadRCCColumn = I18NEntity.GetString("RM_FS_Export_AllowDownloadRCCColumn");
        private string ClassCodeScopeColumn = I18NEntity.GetString("RM_FS_Export_ClassCodeScopeColumn");
        private string ClassCodeColumn = I18NEntity.GetString("RM_FS_Export_ClassCodeColumn");
        private string CountryCodeColumn = I18NEntity.GetString("RM_FS_Export_CountryCodeColumn");
        private string RetentionTypeColumn = I18NEntity.GetString("RM_FS_Export_RetentionTypeColumn");
        private string StartDateColumn = I18NEntity.GetString("RM_FS_Export_StartDateColumn");
        private string EffectScopeColumn = I18NEntity.GetString("RM_FS_Export_EffectScopeColumn");
        #endregion

        private readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        public FSExportSettingProcessorJPMC(RMExportSettingJobMessage jobMsg) : base(jobMsg.JobID, jobMsg.JobType)
        {
            FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto, BaseJobDto.Id.Replace("ESFS", StartFileName), ".xlsx");
            ReportManager.IncreaseBase(10);
            ReportManager.StartUpdateJobProgress();
        }

        protected override string GetPathValidationErrorKey() => "RM_FS_ImportJob_PathMsg";

        protected override bool ShouldExportSetting(RMFileSystemSetting setting, FSConnection connection)
        {
            return true;
        }

        protected override string BuildExportPath(string fullPath, string connectionUNCPath)
        {
            return fullPath;
        }

        protected override Task GetListGroupHasSetting()
        {
            var groupIds = FSConnectionGroupDAO.LoadAllGroups().Select(_ => _.Id).ToList();
            foreach (var groupId in groupIds)
            {
                var connections = FSConnectionDAO.GetAllConnectionsByGroupId(groupId);
                if (connections == null || !connections.Any())
                {
                    continue;
                }
                foreach (var connection in connections)
                {
                    bool hasConnectionRootSetting = FileSystemSettingDAO
                        .LoadAllSettingsByConnectionGroupIdAndConnectionPath(groupId, connection.UNCPath)
                        .Any(s => s.FullPath.Equals(connection.UNCPath));
                    if (hasConnectionRootSetting)
                    {
                        GroupSettingIds.Add(groupId);
                        break;
                    }
                }
            }
            return Task.CompletedTask;
        }

        protected override async Task GenerateDataAsync()
        {
            var allGroups = FSConnectionGroupDAO.LoadAllGroups().Where(g => GroupSettingIds.Contains(g.Id)).ToList();

            if (!allGroups.Any())
            {
                Logger.Info("There is nothing to export JPMC FS settings.");
                return;
            }

            int sheetIndex = 0;
            bool isFirstSheet = true;

            foreach (var group in allGroups)
            {
                var accessConnectionType = FSConnectionGroupDAO.GetTypeByGroupId(group.Id);
                var agentIds = FSConnectionGroupWithAgentMembershipDao.GetAgentIdByGroupId(group.Id);
                var connections = FSConnectionDAO.GetAllConnectionsByGroupId(group.Id).ToList();
                if (connections == null || connections.Count == 0)
                {
                    Logger.Warn($"No connections found for group [{group.Name}].");
                    continue;
                }
                // Collect all exportable settings for this group with their connection name attached
                var groupSettings = new List<(string ConnectionName, RMFileSystemSetting Setting)>();
                var connectionUNCPaths = connections.Select(c => c.UNCPath);
                var settingsUnderGroup = FileSystemSettingDAO.LoadAllConnectionSettingsUnderGroup(group.Id, connectionUNCPaths);

                if (settingsUnderGroup == null || settingsUnderGroup.Count == 0)
                {
                    Logger.Warn($"No settings found for connections under group [{group.Name}].");
                    continue;
                }

                var scopeIdUNCPathDic = GetUNCPathDicInternal(settingsUnderGroup);
                var connectionSettingDic = settingsUnderGroup.ToDictionary(s => s.ScopeId, s => s);
                var resultList = await FileSystemBrowserService.ValidateUNCPathsAsync(scopeIdUNCPathDic, accessConnectionType, agentIds);

                if (resultList == null || resultList.Count == 0)
                {
                    Logger.Warn($"No valid UNC paths found for group [{group.Name}].");
                    continue;
                }
                
                ReportManager.IncreaseBase(connections.Count);

                foreach (var connection in connections)
                {
                    try
                    {
                        if (!(connectionSettingDic.TryGetValue(connection.Id, out var setting) && setting != null))
                        {
                            Logger.Warn($"No setting found for connection [{connection.Name}], Id [{connection.Id}].");
                            continue;
                        }

                        if (!resultList.Contains(setting.ScopeId))
                        {
                            GenerateJobDetail(connection.Name, connection.UNCPath, GetPathValidationErrorKey(), false);
                            continue;
                        }

                        if (setting.ApprovalType == ApprovalType.ApprovalProcess && !Guid.TryParse(setting.WorkflowReferenceId, out _))
                        {
                            GenerateJobDetail(connection.Name, connection.UNCPath, "RM_JS_BCM_ExportSetting_WorkFlowProcessEmpty", false);
                            continue;
                        }

                        if (resultList.Contains(setting.ScopeId))
                        {
                            groupSettings.Add((connection.Name, setting));
                            GenerateJobDetail(connection.Name, connection.UNCPath);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"ValidateUNCPaths failed for connection [{connection.Name}], error:{e}");
                        GenerateJobDetail(connection.Name, connection.UNCPath, GetPathValidationErrorKey(), false);
                    }
                    ReportManager.Increase();
                }

                if (groupSettings.Count == 0)
                {
                    continue;
                }

                var datas = await BuildGroupSheetDataAsync(group.Name, groupSettings);

                if (isFirstSheet)
                {
                    ReportUtil.CreateExcel(FilePath, SheetName + (sheetIndex + 1), datas);
                    isFirstSheet = false;
                    Logger.Info($"Created Excel sheet for group [{group.Name}].");
                }
                else
                {
                    ReportUtil.InsertWorksheet(FilePath, SheetName + (sheetIndex + 1), datas);
                    Logger.Info($"Inserted Excel sheet for group [{group.Name}].");
                }

                sheetIndex++;
            }
        }

        private async Task<string[][]> BuildGroupSheetDataAsync(string groupName, List<(string ConnectionName, RMFileSystemSetting Setting)> groupSettings)
        {
            // Row 0: Group Name header, Row 1: column titles, then data rows
            var datas = new string[groupSettings.Count + 2][];

            datas[0] = new string[2];
            datas[0][0] = GroupNameColumn;
            datas[0][1] = groupName;

            datas[1] = new string[13];
            datas[1][0] = ConnectionNameColumn2;
            datas[1][1] = PathColumn;
            datas[1][2] = EnableILColumn;
            datas[1][3] = AllowDownloadRCCColumn;
            datas[1][4] = ClassCodeScopeColumn;
            datas[1][5] = ClassCodeColumn;
            datas[1][6] = CountryCodeColumn;
            datas[1][7] = RetentionTypeColumn;
            datas[1][8] = StartDateColumn;
            datas[1][9] = EffectScopeColumn;
            datas[1][10] = ManualApprovalTypeColumn;
            datas[1][11] = SendEmailForPersonColumn;
            datas[1][12] = SendEmailNotificationColumn;

            int rowCount = 2;
            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var (connectionName, setting) in groupSettings)
            {
                int colCount = 0;
                try
                {
                    datas[rowCount] = new string[13];
                    for (int i = 0; i < 13; i++) { datas[rowCount][i] = string.Empty; }

                    // [0] Connection Name
                    datas[rowCount][colCount++] = connectionName;

                    // [1] Path
                    datas[rowCount][colCount++] = setting.FullPath;

                    // [2] Enable IL
                    bool isILEnabled = setting.EnableRecordManagement == 1;
                    datas[rowCount][colCount++] = isILEnabled ? "TRUE" : "FALSE";
                    if (!isILEnabled)
                    {
                        rowCount++;
                        continue;
                    }

                    // [3] Allow Download RCC Report
                    datas[rowCount][colCount++] = setting.IsAllowUserDownloadRCCReport ? "TRUE" : "FALSE";

                    // [4] Class Code Scope
                    string termScope = setting.TermSetId != Guid.Empty
                        ? _termDAO.GetTermSetNamesPathByTermSetId(setting.TermSetId)
                        : string.Empty;
                    datas[rowCount][colCount++] = termScope.Replace('/', PathSeparator);

                    // [5] Class Code
                    datas[rowCount][colCount++] = setting.ClassCode ?? string.Empty;

                    // [6] Country Code
                    datas[rowCount][colCount++] = setting.CountryCode ?? string.Empty;

                    // [7] Retention Type
                    bool isEvent = setting.RetentionScheduleType == RetentionScheduleType.Event;
                    datas[rowCount][colCount++] = isEvent ? "Event" : "Flat";

                    // [8] Start Date
                    datas[rowCount][colCount++] = isEvent && setting.StartDate > 0
                        ? GeneralSettingService.ConvertTiksToDateTime(gls, setting.StartDate, false).SimplifyFormatTime
                        : string.Empty;

                    // [9] Effect Scope
                    datas[rowCount][colCount++] = ConvertEffectScopeToString(setting.ApplyExistDocument);

                    // [10] Manual Approval Type & [11] Record Reviewer/Process Name
                    if (setting.ApprovalType == ApprovalType.None)
                    {
                        datas[rowCount][colCount++] = NoManualSetting;
                        datas[rowCount][colCount++] = string.Empty;
                    }
                    else if (setting.ApprovalType == ApprovalType.ApprovalProcess)
                    {
                        var workflow = RMWorkflowDefinitionDAO.GetWorkflowByReferenceId(new Guid(setting.WorkflowReferenceId));
                        if (workflow == null)
                        {
                            GenerateJobDetail(setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"\") + 1), setting.FullPath, "RM_JS_BCM_ExportSetting_WorkFlowProcessEmpty", false);
                            rowCount++;
                            continue;
                        }
                        datas[rowCount][colCount++] = WorkflowProcess;
                        datas[rowCount][colCount++] = workflow.Name;
                    }
                    else if (setting.ApprovalType == ApprovalType.RecordOwners)
                    {
                        datas[rowCount][colCount++] = RecordOwner;
                        var usernames = (await RecordOwnerDao.GetRecordOwnerAccountsAsync(setting.Id, RecordOwnerSettingType.FileSystem)).Select(_ => _.UserPrincipalName);
                        datas[rowCount][colCount++] = string.Join(PathSeparator.ToString(), usernames);
                    }
                    else
                    {
                        datas[rowCount][colCount++] = AutoApprove;
                        datas[rowCount][colCount++] = string.Empty;
                    }

                    // [12] Send Email
                    datas[rowCount][colCount++] = setting.EMailToRecordOwner ? "TRUE" : "FALSE";

                    rowCount++;
                }
                catch (Exception e)
                {
                    Logger.Error($"Convert JPMC File system setting to array failed: {e}");
                    rowCount++;
                    throw;
                }
            }

            return datas;
        }

        protected override async Task<string[][]> ConvertSettingToArrayAsync(List<RMFileSystemSetting> settings, string[][] datas)
        {
            return datas;
        }

        protected override string[][] AssembleSettingHeaderTittle(string[][] datas, string connectionName)
        {
            return datas;
        }

        private static string ConvertEffectScopeToString(bool effectScope)
        {
            return effectScope switch
            {
                true => "Apply to the selected node itself and all its child nodes",
                _ => "Only apply to selected node itself",
            };
        }

        private static Dictionary<Guid, string> GetUNCPathDicInternal(List<RMFileSystemSetting> settings)
        {
            var dic = new Dictionary<Guid, string>();
            foreach (var s in settings)
            {
                if (!dic.ContainsKey(s.ScopeId))
                {
                    dic.Add(s.ScopeId, s.FullPath);
                }
            }
            return dic;
        }
    }
}