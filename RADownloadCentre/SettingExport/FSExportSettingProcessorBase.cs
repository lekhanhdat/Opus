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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using RADownloadCenter;
using RADownloadCentre.SettingExport.Base;

namespace RADownloadCentre.SettingExport
{
    public abstract class FSExportSettingProcessorBase : ExportSettingProcessor<RMFileSystemSetting>
    {
        protected readonly RALogger Logger = RALogger.GetInstance(typeof(FSExportSettingProcessorBase));

        #region Service and DAO
        protected readonly IFSConnectionDao FSConnectionDAO = PlatformWindsorManager.GetService<IFSConnectionDao>();
        protected readonly IRMFileSystemBrowserService FileSystemBrowserService = PlatformWindsorManager.GetService<IRMFileSystemBrowserService>();
        protected readonly IFSConnectionGroupWithAgentMemebershipDao FSConnectionGroupWithAgentMembershipDao = PlatformWindsorManager.GetService<IFSConnectionGroupWithAgentMemebershipDao>();
        protected readonly IFSConnectionGroupDao FSConnectionGroupDAO = PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        protected readonly IFileSystemSettingDao FileSystemSettingDAO = PlatformWindsorManager.GetService<IFileSystemSettingDao>();
        protected readonly IExplorerService ExplorerService = PlatformWindsorManager.GetService<IExplorerService>();
        
        protected IRMReportManager mReportManager;
        protected IRMReportManager ReportManager
        {
            get
            {
                if (mReportManager == null)
                {
                    mReportManager = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManager;
            }
        }
        #endregion

        #region Setting info
        protected List<string> DeactiveUNCPath = new List<string>();
        protected List<Guid> GroupSettingIds = new List<Guid>();
        #endregion

        protected readonly string SheetName = "Group";
        protected readonly int PageSize = 1000;
        protected readonly string StartFileName = "ES_FileSystem_";

        #region Column and value in excel file
        protected string ConnectionNameColumn = I18NEntity.GetString("RM_JS_BCM_Export_ConnectionNameColumn");
        protected string UNCPathColumn = I18NEntity.GetString("RM_JS_BCM_Export_UNCPathColumn");
        protected string ManualApprovalTypeColumn = I18NEntity.GetString("RM_JS_BCM_Export_FSManualApprovalTypeColumn");
        #endregion

        protected FSExportSettingProcessorBase(string jobId, JobType jobType) : base(jobId, jobType)
        {
        }

        protected override async Task GenerateDataAsync()
        {
            Dictionary<string, List<RMFileSystemSetting>> sheetData = new Dictionary<string, List<RMFileSystemSetting>>();
            foreach (var groupId in GroupSettingIds)
            {
                var agentIds = FSConnectionGroupWithAgentMembershipDao.GetAgentIdByGroupId(groupId);
                var connectionOfGroup = FSConnectionDAO.GetAllConnectionsByGroupId(groupId).ToList();
                if (connectionOfGroup == null || connectionOfGroup.Count == 0)
                {
                    continue;
                }

                foreach (var connection in connectionOfGroup)
                {
                    try
                    {
                        var accessConnectionType = FSConnectionGroupDAO.GetTypeByGroupId(connection.GroupId);
                        var settingUnderConnection = FileSystemSettingDAO.LoadAllSettingsByConnectionGroupIdAndConnectionPath(connection.GroupId, connection.UNCPath);
                        if (settingUnderConnection == null || settingUnderConnection.Count == 0)
                        {
                            continue;
                        }

                        List<RMFileSystemSetting> canExportSetting = new List<RMFileSystemSetting>();
                        var scopeIdUNCPathDic = GetUNCPathDic(settingUnderConnection);
                        var resultList = await FileSystemBrowserService.ValidateUNCPathsAsync(scopeIdUNCPathDic, accessConnectionType, agentIds);
                        var successList = scopeIdUNCPathDic.AsQueryable().Where(s => resultList.Contains(s.Key)).Select(s => s.Key).ToList();
                        var failedList = scopeIdUNCPathDic.AsQueryable().Where(s => !resultList.Contains(s.Key)).Select(s => s.Value).ToList();

                        if (failedList.Count > 0)
                        {
                            foreach (var failed in failedList)
                            {
                                GenerateJobDetail(failed.Substring(failed.LastIndexOf(@"\") + 1), failed, GetPathValidationErrorKey(), false);
                            }
                        }

                        if (successList.Count > 0)
                        {
                            foreach (var setting in settingUnderConnection)
                            {
                                if (setting.FullPath.Equals(connection.UNCPath))
                                {
                                    continue;
                                }
                                if (!ShouldExportSetting(setting, connection))
                                {
                                    continue;
                                }
                                if (DeactiveUNCPath.Contains(setting.FullPath) || DeactiveUNCPath.Any(d => setting.FullPath.Contains(d + @"\")))
                                {
                                    GenerateJobDetail(setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"\") + 1), setting.FullPath, "RM_FS_DisposalDeactiveFolder_JobFailed", false);
                                    continue;
                                }
                                if (setting.ApprovalType == ApprovalType.ApprovalProcess && !Guid.TryParse(setting.WorkflowReferenceId, out _))
                                {
                                    GenerateJobDetail(setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"\") + 1), setting.FullPath, "RM_JS_BCM_ExportSetting_WorkFlowProcessEmpty", false);
                                    continue;
                                }
                                if (successList.Contains(setting.ScopeId))
                                {
                                    string fullPath = setting.FullPath;
                                    setting.FullPath = BuildExportPath(setting.FullPath, connection.UNCPath);
                                    canExportSetting.Add(setting);
                                    GenerateJobDetail(fullPath.Substring(fullPath.LastIndexOf(@"\") + 1), fullPath);
                                }
                            }
                            if (canExportSetting.Count > 0)
                            {
                                sheetData.Add(connection.Name, canExportSetting);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"ValidateUNCPaths failed, error:{e}");
                        GenerateJobDetail(connection.Name, connection.UNCPath, GetPathValidationErrorKey(), false);
                        continue;
                    }
                }
            }

            if (!sheetData.Any())
            {
                Logger.Info("There is nothing to export FS settings. Creating empty file with headers.");

                var datas = new string[2][];
                datas = AssembleSettingHeaderTittle(datas, "");
                ReportUtil.CreateExcel(FilePath, SheetName + "1", datas);
                Logger.Info("Create empty Excel with headers success.");

                GenerateAndUploadFileManager.HasSucceed = true;
                GenerateJobDetail("Export Content source Setting", "", "", true);
                return;
            }

            int sheetIndex = 0;
            bool isCreateHeader = true;
            bool nextSheet = false;
            foreach (var sheet in sheetData)
            {
                var currentCount = 0;
                int pageIndex = 0;
                var settings = sheet.Value.OrderBy(_ => _.FullPath.Length).Skip(pageIndex * PageSize).Take(PageSize).ToList();
                do
                {
                    try
                    {
                        currentCount += settings.Count;
                        var datas = new string[settings.Count + 2][];
                        pageIndex++;
                        if (isCreateHeader)
                        {
                            datas = await GenerateSettingsAsync(BaseJobDto.JobType, datas, settings, true, sheet.Key);
                            ReportUtil.CreateExcel(FilePath, SheetName + (sheetIndex + 1), datas);
                            isCreateHeader = false;
                            Logger.Info("Create Excel with header success.");
                            continue;
                        }
                        if (currentCount >= CountOfOneSheet || nextSheet)
                        {
                            datas = await GenerateSettingsAsync(BaseJobDto.JobType, datas, settings, true, sheet.Key);
                            currentCount = settings.Count;
                            ReportUtil.InsertWorksheet(FilePath, SheetName + (sheetIndex + 1), datas);
                            Logger.Info($"Insert Excel with header success, count:{currentCount}, sheet:{sheetIndex}");
                            nextSheet = false;
                            continue;
                        }
                        datas = new string[settings.Count][];
                        datas = await GenerateSettingsAsync(BaseJobDto.JobType, datas, settings, false, sheet.Key);
                        ReportUtil.InsertDataToSheet(FilePath, datas, sheetIndex, 0);
                        Logger.Info($"Insert data to sheet success, count:{currentCount}, sheet:{sheetIndex}");
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Generate report detail to Excel error, sheet:{sheetIndex}, error:{e}");
                        GenerateAndUploadFileManager.HasFailed = true;
                        throw;
                    }
                } while ((settings = sheet.Value.Skip(pageIndex * PageSize).Take(PageSize).ToList()).Any());

                sheetIndex++;
                nextSheet = true;
            }
        }

        /// <summary>
        /// Determines whether a setting row should be exported.
        /// Legacy: skips auto-classification nodes. JPMC: all enabled nodes qualify.
        /// </summary>
        protected virtual bool ShouldExportSetting(RMFileSystemSetting setting, FSConnection connection)
        {
            if (setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
            {
                GenerateJobDetailWithStatus(setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"\") + 1), setting.FullPath, JobDetailsStatus.Skipped, "RM_JS_BCM_ExportSetting_AutoClassificationSupport");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Builds the path value written into the export row.
        /// Legacy: strips the connection prefix (relative path). JPMC: keeps the full UNC path.
        /// </summary>
        protected virtual string BuildExportPath(string fullPath, string connectionUNCPath)
        {
            return fullPath.Replace(connectionUNCPath + "\\", "");
        }

        protected override Task GetListGroupHasSetting()
        {
            var groupIds = FSConnectionGroupDAO.LoadAllGroups().Select(_ => _.Id).ToList();
            foreach (var groupId in groupIds)
            {
                var groupSetting = FileSystemSettingDAO.GetSettingByConnGroupId(groupId);
                if (groupSetting != null && groupSetting.TermSetId != Guid.Empty)
                {
                    GroupSettingIds.Add(groupId);
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns the i18n key used when a UNC path validation fails.
        /// Override in subclasses for JPMC-specific keys.
        /// </summary>
        protected virtual string GetPathValidationErrorKey() => "RM_FS_ImportJob_UNCPathMsg";

        private Dictionary<Guid, string> GetUNCPathDic(List<RMFileSystemSetting> settingObjects)
        {
            Dictionary<Guid, string> scopeIdUNCPathDic = new Dictionary<Guid, string>();
            foreach (var settingObj in settingObjects)
            {
                if (!scopeIdUNCPathDic.ContainsKey(settingObj.ScopeId))
                {
                    scopeIdUNCPathDic.Add(settingObj.ScopeId, settingObj.FullPath);
                }
            }
            return scopeIdUNCPathDic;
        }
    }
}