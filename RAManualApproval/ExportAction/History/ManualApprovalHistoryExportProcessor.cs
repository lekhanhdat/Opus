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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Myhub.Items.Views;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.Exchange.WebServices.Data;
using Newtonsoft.Json;
using OpenNLP.Tools.Util;
using RAManualApproval.ExportAction.UnderReview;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace RAManualApproval.ExportAction.History
{
    public class ManualApprovalHistoryExportProcessor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualApprovalHistoryExportProcessor));

        private static readonly IRMSecurityTrimmingHelper SecurityTrimmingHelper = PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private static readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private static readonly IUserService UserService = PlatformWindsorManager.GetService<IUserService>();

        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static readonly IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        private static readonly IRMSubJobDao SubJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();

        private static readonly IRMCustomizeConnectorContentSourceDao CustomizeConnectorContentSourceDao = PlatformWindsorManager.GetService<IRMCustomizeConnectorContentSourceDao>();

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private static readonly GeneralSettingModel GeneralSetting = GeneralSettingService.GetGeneralSettingAsync().Result;
        private static readonly IFSConnectionDao FSConnectionDao = PlatformWindsorManager.GetService<IFSConnectionDao>();
        private static readonly IMyhubReportJobDao MyhubReportJobDao = PlatformWindsorManager.GetService<IMyhubReportJobDao>();

        private readonly Dictionary<int, string> UserDisplayNameCache = new();

        private readonly Dictionary<string, List<string>> UserDisplayNameListCache = new();

        private readonly ManualApprovalHistoryOption HistoryOption;

        private static readonly int countOfOneSheet = 200000;

        private static readonly int PageSize = 1000;
        private string GenerateDateTimeStr = string.Empty;
        private string ConnectionName = string.Empty;
        private string JPMCId = string.Empty;
        private string MyhubFileName = string.Empty;
        private string MyhubDisplayName = string.Empty;
        private string StartFileName = "Disposal_history_report_";
        private string FullPath { get; set; }
        private string FolderPath { get; set; }
        private bool IsAdmin { get; set; }
        private string JobId { get; set; }
        private int TotalCount { get; set; }
        private bool IsMyhub { get; set; } = false;
        private List<int> UserHasPermissionIntIds { get; set; }
        private Dictionary<int, string> ContentSourceInfoes { get; set; }

        public ManualApprovalHistoryExportProcessor(string subJobId, string useId)
        {
            TenantLocalValue.LogonUserId = useId;
            var subJob = SubJobDao.GetSubJob(subJobId, true);
            var jobId = subJob.ParentId;
            HistoryOption = SerializerHelper.DeserializeByJsonSerializer<ManualApprovalHistoryOption>(subJob.JobContext.Content);
            ManualApprovalHistoryExportJobManager.Init(jobId);
            IsAdmin = SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(AvePoint.RA.Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin).Result;
            UserHasPermissionIntIds = UserService.GetUserWithRemovedAndGroupIds(useId);
            ContentSourceInfoes = CustomizeConnectorContentSourceDao.GetAllSimpleInfoes(CustomizeConnectorOrigin.BuildIn, CustomizeConnectorOrigin.ExternalCustomize)
           .GetAwaiter().GetResult()
           .ToDictionary(item => item.Flag, item => I18NEntity.GetString(item.Name));
            JobId = jobId;

            if (HistoryOption.LatestExportType == 4 && HistoryOption.CustomDate.TimeZoneId != null)
            {
                GeneralSetting.TimeZoneId = HistoryOption.CustomDate.TimeZoneId;
                GeneralSetting.DayLight = HistoryOption.CustomDate.IsDaylight;
            }
            var nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
            FolderPath = JobReportUtility.GetDownloadManualApprovalReviewReportTempleFolder("Temple") + Path.DirectorySeparatorChar + I18NEntity.GetString("RM_DAM_ManualApprovalReviewReport") + "_" + nowDateTimeStr + Guid.NewGuid();
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }

        public async System.Threading.Tasks.Task RunAsync()
        {

            var historyReport = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int>() { (int)DownloadContentJobStatus.Wait }).Where(item => item.JobId == JobId).FirstOrDefault();
            try
            {
                using (new PerformanceScope("Export History Datas"))
                {
                    historyReport.JobStatus = (int)DownloadContentJobStatus.InProgress;

                    await DownloadDataInfoDao.UpdateAsync(historyReport);

                    var manualHistoryObj = string.IsNullOrWhiteSpace(historyReport.ExtendString1)
                        ? new ManualApprovalHistoryOption()
                        : JsonConvert.DeserializeObject<ManualApprovalHistoryOption>(historyReport.ExtendString1) ?? new ManualApprovalHistoryOption();

                    if (!string.IsNullOrEmpty(manualHistoryObj.FullPath))
                    {
                        IsMyhub = true;

                        var connection = FSConnectionDao.GetConnectionById(new Guid(manualHistoryObj.PartitionKeyId));
                        JPMCId = connection.JPMCConnectionId ?? string.Empty;
                        ConnectionName = connection.Name ?? connection.UNCPath ?? string.Empty;

                        string sanitizedJpmcId = SanitizeFileName(JPMCId);
                        string sanitizedConnName = SanitizeFileName(ConnectionName);

                        if (string.IsNullOrWhiteSpace(sanitizedJpmcId) && string.IsNullOrWhiteSpace(sanitizedConnName))
                        {
                            Logger.Warn($"Connection name and JPMCId are empty after sanitization. Skipping.");
                            ManualApprovalHistoryExportJobManager.AddJobDetail(ConnectionName, manualHistoryObj.FullPath, isSuccess: false, comment: "Connection name and JPMCId are empty after sanitization.");
                            historyReport.JobStatus = (int)DownloadContentJobStatus.Failed;
                            await DownloadDataInfoDao.UpdateAsync(historyReport);
                            await MyhubReportJobDao.UpdateStatusByJobId(historyReport.JobId, MyhubReportJobStatus.Failed);
                            ManualApprovalHistoryExportJobManager.SendJobDetail();
                            return; 
                        }

                        sanitizedJpmcId = TruncateString(sanitizedJpmcId, 100);
                        sanitizedConnName = TruncateString(sanitizedConnName, 100);

                        if (historyReport.FileDownloadTime > 0)
                        {
                            var timeModel = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, historyReport.FileDownloadTime, false);
                            GenerateDateTimeStr = timeModel.DataTime.ToString("yyyyMMdd_HHmmss");
                        }
                        else
                        {
                            GenerateDateTimeStr = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                        }

                        MyhubFileName = $"{StartFileName}{sanitizedJpmcId}_{sanitizedConnName}_{GenerateDateTimeStr}.csv";
                        MyhubDisplayName = $"{StartFileName}{sanitizedJpmcId}_{sanitizedConnName}_{GenerateDateTimeStr}.zip";

                        manualHistoryObj.DisplayName = $"{StartFileName}{sanitizedJpmcId}_{sanitizedConnName}.zip";
                        historyReport.ExtendString1 = JsonConvert.SerializeObject(manualHistoryObj);

                        await MyhubReportJobDao.UpdateStatusByJobId(historyReport.JobId, MyhubReportJobStatus.InProgress);
                    }

                    await GenerateAzureHistoryDatasSheetAsync(HistoryOption.LatestExportType, manualHistoryObj);

                    if (TotalCount == 0)
                    {
                        ManualApprovalHistoryExportJobManager.JobComment = "RM_RDM_ExportHistory_NoDatas";
                        ManualApprovalHistoryExportJobManager.HasSucceed = true;
                    }

                    Logger.Info($"Export history datas success, total count is {TotalCount}");

                    if (ManualApprovalHistoryExportJobManager.HasSucceed)
                    {
                        await UploadBlobAsync(FolderPath, JobId);
                        var fileInfo = new FileInfo(FolderPath + ".zip");
                        historyReport.FileSize = fileInfo.Length;
                        historyReport.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                        historyReport.JobStatus = (int)DownloadContentJobStatus.Finished;

                        if (IsMyhub)
                        {
                            manualHistoryObj.DisplayName = MyhubDisplayName;
                            historyReport.ExtendString1 = JsonConvert.SerializeObject(manualHistoryObj);
                            historyReport.Name = MyhubDisplayName;
                        }
                        
                        if (IsMyhub) await MyhubReportJobDao.UpdateStatusByJobId(historyReport.JobId, MyhubReportJobStatus.Finished);
                    }
                    else
                    {
                        historyReport.JobStatus = (int)DownloadContentJobStatus.Failed;

                        if (IsMyhub) await MyhubReportJobDao.UpdateStatusByJobId(historyReport.JobId, MyhubReportJobStatus.Failed);
                    }

                    await DownloadDataInfoDao.UpdateAsync(historyReport);

                    ManualApprovalHistoryExportJobManager.SendJobDetail();
                }
            }
            catch (Exception e)
            {
                ManualApprovalHistoryExportJobManager.HasFailed = true;
                historyReport.JobStatus = (int)DownloadContentJobStatus.Failed;
                if (IsMyhub) await MyhubReportJobDao.UpdateStatusByJobId(historyReport.JobId, MyhubReportJobStatus.Failed);
                await DownloadDataInfoDao.UpdateAsync(historyReport);
                Logger.Error($"Run export history data job failed, error : {e}");
                Logger.Info($"Success total count is {TotalCount}");
            }
            finally
            {
                ManualApprovalHistoryExportJobManager.SetJobFinished();
            }
        }

        private static async System.Threading.Tasks.Task UploadBlobAsync(string folderPath, string jobId)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Logger.Info("folder path is empty, no file need to upload!");
                return;
            }
            AvePoint.GCommon.ZipUtil.ZipFolder(folderPath, folderPath + ".zip", Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = Path.Combine(customId, jobId + ".zip");
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, folderPath + ".zip");
                    Logger.Info($"Upload history report success");
                    return System.Threading.Tasks.Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Upload history report failed,error is :{e}");
                throw;
            }

            Logger.Info($"finish to upload blob name:{blobName}");
        }

        private async System.Threading.Tasks.Task GenerateAzureHistoryDatasSheetAsync(int lastTime, ManualApprovalHistoryOption manualApprovalHistoryOption)
        {
            var currentDateMonth = DateTime.UtcNow;
            var startMonth = DateTime.UtcNow;
            var IntMaxMonth = int.Parse(currentDateMonth.ToString("yyyyMM"));
            long startTime = 0;
            long endTime = 0;
            switch (lastTime)
            {
                case (int)TimeRange.After3Month:
                    startMonth = currentDateMonth.AddMonths(-2);
                    break;
                case (int)TimeRange.After6Month:
                    startMonth = currentDateMonth.AddMonths(-5);
                    break;
                case (int)TimeRange.After1Year:
                    startMonth = currentDateMonth.AddMonths(-11);
                    break;
                case (int)TimeRange.All:
                    var historyData = await RMRecordStorageAzureTableContext.ManualApproveHistories.FirstOrDefault(item => item.ActionTime != 0);
                    startMonth = new DateTime(historyData.ActionTime);
                    break;
                case (int)TimeRange.Custom:
                    var timeZoneId = GeneralSetting.TimeZoneId;
                    var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);
                    startMonth = HistoryOption.CustomDate.StartDateTime;
                    IntMaxMonth = int.Parse(HistoryOption.CustomDate.EndDateTime.ToString("yyyyMM"));
                    startTime = TimeZoneInfo.ConvertTimeToUtc(HistoryOption.CustomDate.StartDateTime, timeZone).Ticks;
                    endTime = TimeZoneInfo.ConvertTimeToUtc(HistoryOption.CustomDate.EndDateTime, timeZone).Ticks;
                    Logger.Info($"Custom start time utc ticks is : {startTime}, end time utc ticks is : {endTime}, time zone id is {timeZoneId}, time zone info is : {timeZone.DisplayName}");
                    break;
            }

            await GenerateExcelWithTimeAsync(startMonth, IntMaxMonth, IsAdmin, startTime, endTime, UserHasPermissionIntIds, manualApprovalHistoryOption);
        }

        private async Task<string> GenerateRecordItemStringForCsvAsync(RMManualApproveHistoryTableEntity history)
        {
            var dataLine = string.Empty;
            try
            {
                if ((int)history.ApprovedStatus == (int)SOApproveDBStatus.Approved) 
                {
                    history.QuickReason = string.Empty;
                }
                var isArchived = history.RetentionStatus == 1 ? $"({I18NEntity.GetString("RM_MA_Extended_RetentionStatus")})" : string.Empty;
                var fields = new List<string>
                    {
                        GetI18NOfSourceFlag((SourceFlag)history.Source) ,
                        history.LeafName ?? string.Empty ,
                        history.RecordsId ?? string.Empty,
                        history.FullPath ?? string.Empty,
                        history.FolderPath ?? string.Empty,
                        !string.IsNullOrEmpty(history.FileExtension) ? I18NEntity.GetString(history.FileExtension) + isArchived : string.Empty ,
                         I18NEntity.GetString($"RM_JS_MA_ApproveStatus_{(SOApproveDBStatus)history.ApprovedStatus}"),
                        history.RuleName ?? string.Empty,
                        history.RuleDisposalClass ?? string.Empty,
                        GetRelateRecordsInfo(history.RelatedRecords),
                        string.IsNullOrWhiteSpace(history.RelatedRecords) ? string.Empty : GetRelatedRecordsAction(history.RelatedRecordsAction),
                        await GetUserDisplayNameAsync(history.EscalateFrom),
                        !string.IsNullOrEmpty(history.EscalateTo) ? string.Join(";", await GetUsersDisplayNamesAsync(history.EscalateTo)) : string.Empty,
                         await GetUserDisplayNameAsync(history.ApprovedBy) ,
                         history.QuickReason ?? string.Empty,
                         history.ManualApprovalComment ?? string.Empty,
                        history.EscalatedComment ?? string.Empty,
                        history.ModifiedBy ?? string.Empty,
                        history.CreatedBy ?? string.Empty ,
                        GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, history.ActionTime, true).SimplifyFormatTime ,
                        GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, history.CollectionTime, true).SimplifyFormatTime,
                        history.ModifiedTime > 0 ? GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, history.ModifiedTime, true).SimplifyFormatTime  : string.Empty ,
                    };

                dataLine = StringUtils.ToCSVString(fields.ToArray());
                ManualApprovalHistoryExportJobManager.HasSucceed = true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Convert history to cell failed,history item id {history.RowKey},{ex}");
                ManualApprovalHistoryExportJobManager.AddFailedJobDetail(history, string.Join(";", (await GetUsersDisplayNamesAsync(history.EscalateTo)).ToArray()), ex.Message);
            }

            return dataLine;
        }

        private string GetRelateRecordsInfo(string relatedRecords)
        {
            var historyRelatedRecords = string.IsNullOrEmpty(relatedRecords) ?
                                        new List<ReportRelatedRecords>() :
                                        SerializerHelper.DeserializeFromXmlString<List<ReportRelatedRecords>>(relatedRecords);
            if (historyRelatedRecords.Count > 0)
            {
                StringBuilder sBuilder = new();
                var reportRelatedRecords = historyRelatedRecords;
                foreach (var rProp in reportRelatedRecords)
                {
                    if (!string.IsNullOrEmpty(rProp.Url) && rProp.Url.StartsWith("/Root/PRM/RecordsExplorer"))//physical data
                    {
                        rProp.Url = HistoryOption.ServiceUrl + rProp.Url;
                    }
                    sBuilder.AppendFormat("{0}:\n{1}\n", rProp.Name, rProp.Url);
                }
                return sBuilder.ToString();
            }
            else
            {
                return string.Empty;
            }

        }

        private string GetRelatedRecordsAction(int relatedAction)
        {
            if (relatedAction == 0)
            {
                return I18NEntity.GetString("RM_JS_RDM_RelatedRecordsAction_None");
            }
            else if (relatedAction == 1)
            {
                return I18NEntity.GetString("RM_JS_RDM_RelatedRecordsAction_Both");
            }
            else
            {
                return string.Empty;
            }
        }

        private static List<string> AssembleMaReviewInfoHeaderTittleForCsv()
        {
            return new List<string>
            {
                I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_Source"),
                I18NEntity.GetString("RM_JS_MA_Grid_Title"),
                I18NEntity.GetString("RM_PRM_PRE_Column_ID"),
                I18NEntity.GetString("RM_JS_MA_Grid_FullPath"),
                I18NEntity.GetString("RM_JS_MA_Grid_FolderPath"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Type"),
                I18NEntity.GetString("RM_JS_MA_Grid_ApprovalStatus"),
                I18NEntity.GetString("RM_JS_MA_Grid_Rule"),
                I18NEntity.GetString("RM_JS_Rule_DisposalClass_Title"),
                I18NEntity.GetString("RM_JS_MA_Grid_RelatedRecords"),
                I18NEntity.GetString("RM_JS_MA_Grid_RelatedRecordsAction"),
                I18NEntity.GetString("RM_MA_Grid_EscalateOrReassignFrom"),
                I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_RecordsOwner"),
                I18NEntity.GetString("RM_JS_MA_Grid_ApprovedBy"),
                I18NEntity.GetString("RM_MA_QuickReason"),
                I18NEntity.GetString("RM_MA_ApprovalComment"),
                I18NEntity.GetString("RM_JS_MA_Grid_Reassigned_Comment"),
                I18NEntity.GetString("RM_JS_MA_Grid_ModifiedBy"),
                I18NEntity.GetString("RM_JS_MA_Grid_CreatedBy"),
                I18NEntity.GetString("RM_JS_MA_Grid_ActionTime"),
                I18NEntity.GetString("RM_JS_MA_Grid_CreatedTime"),
                I18NEntity.GetString("RM_JS_MA_Grid_ModifiedTime"),
            };
        }

        private string GetI18NOfSourceFlag(SourceFlag sourceFlag)
        {
            if (ContentSourceInfoes.ContainsKey((int)sourceFlag))
            {
                return ContentSourceInfoes[(int)sourceFlag];
            }
            return I18NEntity.GetString("RM_CP_Connector");
        }

        private async Task<List<string>> GetUsersDisplayNamesAsync(string userIntIdsStr)
        {
            if (string.IsNullOrEmpty(userIntIdsStr))
            {
                return new List<string>();
            }
            if (!UserDisplayNameListCache.ContainsKey(userIntIdsStr))
            {
                var userIntIds = userIntIdsStr.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList().ConvertAll(item => int.Parse(item));
                if (userIntIds == null || userIntIds.Count == 0)
                {
                    return new List<string>();
                }
                var users = await AccountDao.GetUserWithRemovedByIds(userIntIds.ToHashSet().ToList());
                var displayNames = users.ConvertAll(item => item.DisplayName);
                UserDisplayNameListCache[userIntIdsStr] = displayNames;
            }

            return UserDisplayNameListCache[userIntIdsStr];
        }

        private async Task<string> GetUserDisplayNameAsync(int userIntId)
        {
            if (userIntId <= 0)
            {
                return string.Empty;
            }

            if (!UserDisplayNameCache.ContainsKey(userIntId))
            {
                var user = await AccountDao.GetUserWithRemovedByIds(new List<int> { userIntId });
                UserDisplayNameCache[userIntId] = user.FirstOrDefault()?.DisplayName;
            }
            return UserDisplayNameCache[userIntId];

        }

        private void GenerateFullPath()
        {
            var nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
            var fileName = string.Empty;

            if (!string.IsNullOrEmpty(MyhubFileName))
            {
                string safeJpmc = SanitizeFileName(JPMCId);
                string safeConn = SanitizeFileName(ConnectionName);

                safeJpmc = TruncateString(safeJpmc, 100);
                safeConn = TruncateString(safeConn, 100);

                fileName = $"{StartFileName}{safeJpmc}_{safeConn}_{nowDateTimeStr}";
            }
            else
            {
                fileName = I18NEntity.GetString("RM_DAM_ManualHistoryReport") + "_" + nowDateTimeStr;
            }

            FullPath = FolderPath + Path.DirectorySeparatorChar + fileName + ".csv";

            if (!System.IO.Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }


        private async System.Threading.Tasks.Task GenerateExcelWithTimeAsync(
            DateTime minActionTime,
            int intMaxMonth,
            bool isAdmin,
            long beginTime,
            long endTime,
            List<int> userHasPermissionIntIds,
            ManualApprovalHistoryOption manualApprovalHistoryOption)
        {

            var hasNextFile = false;
            var continueTime = minActionTime;
            var continution = string.Empty;
            var fileIndex = 0;
            List<int> sourceList = null;
            if (manualApprovalHistoryOption.Filters.Any(f => f.FilterOption == ManualApprovalFilterOptions.Source))
            {
                var sourceFilter = manualApprovalHistoryOption.Filters
                    .First(f => f.FilterOption == ManualApprovalFilterOptions.Source);

                sourceList = JsonConvert.DeserializeObject<List<int>>(sourceFilter.Value);
            }
            do
            {
                var currentCount = 0;
                GenerateFullPath();
                var CreateHeader = true;
                var ioio = int.Parse(continueTime.ToString("yyyyMM"));
                var ioi = int.Parse(continueTime.ToString("yyyyMMdd"));
                for (var startTime = continueTime; intMaxMonth >= int.Parse(startTime.ToString("yyyyMM")); startTime = startTime.AddMonths(1))
                {
                    do
                    {
                        var queryValues = new List<RMManualApproveHistoryTableEntity>();
                        using (new PerformanceScope($"Query Manual Approval History Count {PageSize},partition key {startTime.ToString("yyyyMM")}", "", true))
                        {
                            string targetPartitionKey = startTime.ToString("yyyyMM");
                            Expression<Func<RMManualApproveHistoryTableEntity, bool>> queryFilter;

                            if (!string.IsNullOrEmpty(manualApprovalHistoryOption.FullPath))
                            {
                                string exactPath = manualApprovalHistoryOption.FullPath.TrimEnd('\\');
                                string pathWithSlash = exactPath + "\\";

                                char lastChar = pathWithSlash[pathWithSlash.Length - 1];
                                string upperBound = pathWithSlash.Substring(0, pathWithSlash.Length - 1) + (char)(lastChar + 1);

                                queryFilter = item => item.PartitionKey == targetPartitionKey
                                                   && (item.FullPath == exactPath ||
                                                      (item.FullPath.CompareTo(pathWithSlash) >= 0 && item.FullPath.CompareTo(upperBound) < 0));
                            }
                            else
                            {
                                queryFilter = item => item.PartitionKey == targetPartitionKey;
                            }

                            var (ContinuatioinToken, Values) = await RMRecordStorageAzureTableContext.ManualApproveHistories.QueryWithPagination(
                                queryFilter,
                                PageSize,
                                continution);

                            if (sourceList != null)
                            {
                                Values = Values.Where(v => sourceList.Contains(v.Source)).ToList();
                            }

                            if (!isAdmin)
                            {
                                Values = Values.Where(item => !string.IsNullOrEmpty(item.EscalateTo) && item.EscalateTo.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList().ConvertAll(item => int.Parse(item)).Any(i => userHasPermissionIntIds.Contains(i))).ToList();
                            }

                            if (beginTime != 0 && endTime != 0)
                            {
                                Values = Values.Where(item => item.ActionTime >= beginTime && item.ActionTime <= endTime).ToList();
                            }

                            continution = ContinuatioinToken;
                            queryValues = Values.ToList();
                            if (queryValues.Count == 0)
                            {
                                continue;
                            }
                            using var writer = new StreamWriter(FullPath, true, Encoding.UTF8);
                            if (CreateHeader)
                            {
                                var headers = AssembleMaReviewInfoHeaderTittleForCsv();
                                var headerLine = StringUtils.ToCSVString(headers.ToArray());
                                await writer.WriteLineAsync(headerLine);
                                CreateHeader = false;
                            }
                            foreach(var value in queryValues)
                            {
                                var itemString = await GenerateRecordItemStringForCsvAsync(value);
                                if (!string.IsNullOrEmpty(itemString))
                                {
                                    await writer.WriteLineAsync(itemString);
                                }
                            }                          
                            currentCount += queryValues.Count;
                            TotalCount += queryValues.Count;
                            hasNextFile = (currentCount >= countOfOneSheet);
                            Logger.Info($"Insert data to csv {fileIndex} success,current row count is {currentCount}");
                        }
                    } while (!string.IsNullOrEmpty(continution) && !hasNextFile);

                    if (hasNextFile)
                    {
                        fileIndex++;
                        continueTime = startTime;
                        if (string.IsNullOrEmpty(continution))
                        {
                            continueTime = continueTime.AddMonths(1);
                        }
                        break;
                    }
                }

            } while (hasNextFile && (intMaxMonth >= int.Parse(continueTime.ToString("yyyyMM")) || !string.IsNullOrEmpty(continution)));
        }

        /*private static Queue<int> GetLastAnyMonths(int lastTime)
        {
            var res = new Queue<int>(lastTime);
            var now = DateTime.UtcNow;
            for (var i = 0; i < lastTime; i++)
            {
                var month = now.ToString("yyyyMM");
                res.Enqueue(int.Parse(month));
                now = now.AddMonths(-1);
            }

            return res;
        }*/
        private bool HasInvalidFileNameChars(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;
        }

        private string TruncateString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length > maxLength ? text.Substring(0, maxLength) : text;
        }
        private static string SanitizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(input.Length);

            foreach (char c in input)
            {
                sb.Append(Array.IndexOf(invalidChars, c) >= 0 ? '_' : c);
            }

            string sanitized = sb.ToString();

            sanitized = Regex.Replace(sanitized, @"[^a-zA-Z0-9\s\.\-]{2,}", "_");

            sanitized = Regex.Replace(sanitized, @"_{2,}", "_");

            return sanitized.Trim();
        }
    }

    public enum TimeRange
    {
        None = 0,
        After3Month = 1,
        After6Month = 2,
        After1Year = 3,
        Custom = 4,
        All = 5
    }
}
