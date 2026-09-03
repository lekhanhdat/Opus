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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using Newtonsoft.Json;
using OpenNLP.Tools.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Export
{
    public class ExportHoldsRecordsProcessor
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ExportHoldsRecordsProcessor));
        #region interface
        private static readonly IJobInfoUpdater JobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));


        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;


        private static readonly IRMSubJobDao SubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));

        private static readonly IExplorerService ExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));

        private static readonly IGeneralSettingService GeneralSettingService = (IGeneralSettingService)PlatformWindsorManager.GetService(typeof(IGeneralSettingService));

        private static readonly IHoldDao HoldDao = (IHoldDao)PlatformWindsorManager.GetService(typeof(IHoldDao));


        private static readonly RA.DB.Explorer.Dao.IExplorerDao ExplorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();

        private static readonly IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        #endregion
        private string mJobId = string.Empty;
        private string JobId = string.Empty;
        private string FolderPath { get; set; }
        private bool HasSucceedDetail { get; set; }
        private bool HasFailedDetail { get; set; }
        private string JobComment { get; set; }
        private readonly int CountOfOneSheet = 200000;
        private readonly int PageSize = 1000;
        private bool mIsJob = false;
        private ExportHoldsRecordsCache exportCache = new ExportHoldsRecordsCache();
        public ExportHoldsRecordsProcessor(string mjobId, string jobId)
        {
            mJobId = mjobId;
            JobId = jobId;
            ReportMangerFactory.Instance.Init(mJobId, JobType.ExportHoldRecords);
            JobInfoUpdater.UpdateJobState(mJobId, (int)JobStatus.InProgress);
            ReportManager.StartUpdateJobProgress();
        }

        public async Task RunAsync()
        {
            logger.Info("Start to run export hold records job.");
            RMSubJob subJob = SubJobDao.GetSubJob(mJobId, true);
            var holdIds = SerializerHelper.DeserializeByDataContractSerializer<List<string>>(subJob.JobContext.Content);
            var holdsByIds = HoldDao.GetHoldByIds(holdIds);
            var holdMap = holdsByIds.ToDictionary(h => h.Id, h => h.Name);
            var downloadDataInfo = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int>() { (int)DownloadContentJobStatus.Wait })
                .Where(item => item.JobId == JobId).First();
            try
            {
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.InProgress);

                await GenerateHoldRecordsReportAsync(holdMap);

                var fileInfo = await UploadBlobAsync();
                if (fileInfo != null)
                {
                    downloadDataInfo.FileSize = fileInfo.Length;
                }

                downloadDataInfo.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                HasSucceedDetail = true;
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Finished);
            }
            catch (Exception e)
            {
                HasFailedDetail = true;
                JobComment = e.Message;
                logger.Error($"Export hold records failed, error: {e}");
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Failed);
            }
            finally
            {
                var jobFinishStatus = HasSucceedDetail && HasFailedDetail
                    ? JobStatus.FinishWithException
                    : (HasFailedDetail ? JobStatus.Failed : JobStatus.Finished);
                ReportManager.SetJobFinished(jobFinishStatus, JobComment);
                PerformanceMonitor.WritePerformanceResult();
            }
        }

        private async Task GenerateHoldRecordsReportAsync(Dictionary<string, string> holdMap)
        {
            string folderPath = string.Empty;
            var generatedFiles = new List<string>();

            try
            {
                var nowTimeStr = (await GeneralSettingService
                    .ConvertTiksToDateTimeAsync(DateTime.UtcNow.Ticks, false))
                    .DataTime
                    .ToString(AveDateTimeUtility.DATETYPE022);

                folderPath = JobReportUtility.GetDownloadHoldRecordReportTempleFolder(Guid.NewGuid().ToString());
                FolderPath = folderPath;

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                logger.Info("Start streaming data to CSV in folder {0}.", folderPath);
                var gls = await GeneralSettingService.GetGeneralSettingAsync();

                var headers = new[]
                {
                    I18NEntity.GetString("RM_PRM_PRE_Column_Name"),
                    I18NEntity.GetString("RM_PRM_PRE_Column_ID"),
                    I18NEntity.GetString("RM_PRM_PRE_Column_HoldRecordPath"),
                    I18NEntity.GetString("RM_PRM_PRE_Column_CreatedTime"),
                    I18NEntity.GetString("RM_PRM_PRE_Column_Creator"),
                    I18NEntity.GetString("RM_PRM_PRE_Column_ModifiedTime"),
                    I18NEntity.GetString("RM_PRM_PRE_Column_Modifier"),
                    I18NEntity.GetString("RM_PRM_PRE_Column_DisposalStatus"),
                    I18NEntity.GetString("RM_PRM_PRE_Column_HoldType"),
                    I18NEntity.GetString("RM_PRM_PRE_Column_HoldUntil"),
                    I18NEntity.GetString("RM_PRM_PRE_Column_HoldBy")
                };

                var fileIndex = 1;
                string continuationToken = null;
                bool hasNextPage = true;
                var holdIdList = holdMap.Keys.ToList();
                bool isFileFull = false;
                Expression<Func<Record, bool>> filterExpression = item =>
                    (item.HoldId != null && holdIdList.Contains(item.HoldId)) ||
                    (item.AppendHolds_Array != null && item.AppendHolds_Array.Any(id => holdIdList.Contains(id)));

                do
                {
                    string fileName = $"{I18NEntity.GetString("RM_JS_JM_JobType_ExportHoldRecords")}_{nowTimeStr}_{fileIndex}.csv";
                    string fileFullPath = SecurityUtils.SafeCombinePath(folderPath, fileName);
                    generatedFiles.Add(fileFullPath);
                    var currentRowCount = 0;
                    using var stream = new FileStream(fileFullPath, FileMode.Create, FileAccess.ReadWrite);
                    using var writer = new StreamWriter(stream, Encoding.UTF8);
                    writer.WriteLine(StringUtils.ToCSVString(headers));
                    do
                    {
                        Tuple<IEnumerable<Record>, string> queryResult = ExplorerDao.QueryDataWithoutTotal(continuationToken, this.PageSize, out hasNextPage, filterExpression);

                        var recordsChunk = queryResult?.Item1;
                        continuationToken = queryResult?.Item2;

                        if (recordsChunk == null || !recordsChunk.Any())
                        {
                            break;
                        }

                        foreach (var record in recordsChunk)
                        {
                            writer.WriteLine(StringUtils.ToCSVString(BuildDataRow(record, holdMap, gls)));
                            currentRowCount++;
                            if (currentRowCount >= CountOfOneSheet)
                            {
                                isFileFull = true;
                                break;
                            }
                        }
                        isFileFull = (currentRowCount >= CountOfOneSheet);
                        logger.Info($"Insert data to csv {fileIndex} success, current row count is {currentRowCount}");

                    }
                    while (!string.IsNullOrEmpty(continuationToken) && hasNextPage && !isFileFull);

                    fileIndex++;

                }
                while (!string.IsNullOrEmpty(continuationToken) && hasNextPage && isFileFull);
                exportCache.Clear();
                logger.Info("All hold records report generation completed successfully. Total sheets: {0}", generatedFiles.Count);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while generating hold records report. Error: {0}", e);
                throw;
            }
        }
        private string[] BuildDataRow(Record record, Dictionary<string, string> holdMap, GeneralSettingModel gls)
        {
            var selectedHoldIds = GetAllHoldIds(record).Where(id => holdMap.ContainsKey(id)).ToList();
            var holdNames = ResolveHoldNames(selectedHoldIds, holdMap);
            var holdUntil = ResolveHoldUntilTimes(record, holdMap, gls, selectedHoldIds);
            var holdBy = ResolveHoldByUsers(record, holdMap);

            var createdTime = record.TimeCreated > 0
                ? GeneralSettingService.ConvertTiksToDateTime(gls, record.TimeCreated, true).SimplifyFormatTime
                : string.Empty;
            var modifiedTime = record.TimeModified > 0
                ? GeneralSettingService.ConvertTiksToDateTime(gls, record.TimeModified, true).SimplifyFormatTime
                : string.Empty;

            return new[]
                {
                    record.LeafName ?? string.Empty,
                    record.RecordsId ?? string.Empty,
                    GetRecordFullPath(record),
                    createdTime,
                    record.CreatedBy ?? string.Empty,
                    modifiedTime,
                    record.ModifiedBy ?? string.Empty,
                    record.HoldStatus ? "Yes" : "No",
                    holdNames,
                    holdUntil,
                    holdBy
                };
        }

        private List<string> GetAllHoldIds(Record record)
        {
            var ids = new List<string>();
            if (!string.IsNullOrEmpty(record.HoldId))
            {
                ids.Add(record.HoldId);
            }
            if (record.AppendHolds_Array != null)
            {
                ids.AddRange(record.AppendHolds_Array.Where(id => !string.IsNullOrEmpty(id)));
            }
            return ids;
        }

        private string ResolveHoldNames(List<string> selectedHoldIds, Dictionary<string, string> holdMap)
        {
            return selectedHoldIds
                .Where(id => holdMap.ContainsKey(id))
                .Select(id => holdMap[id])
                .Distinct()
                .Aggregate(string.Empty, (acc, name) => acc.Length == 0 ? name : acc + "; " + name);
        }

        private string ResolveHoldUntilTimes(Record record, Dictionary<string, string> holdMap, GeneralSettingModel gls, List<string> selectedHoldIds)
        {
            var allUntilTimes = string.IsNullOrEmpty(record.HoldUntilTimes)
                ? new List<HoldUntilTime>()
                : JsonConvert.DeserializeObject<List<HoldUntilTime>>(record.HoldUntilTimes);

            if (record.HoldStatus && allUntilTimes.Count == 0)
            {
                allUntilTimes.Add(new HoldUntilTime { HoldId = record.HoldId, UntilTime = record.HoldReleaseTime });
            }

            return selectedHoldIds
                .Select(id => allUntilTimes.FirstOrDefault(u => u.HoldId == id))
                .Where(u => u != null && u.UntilTime > 0)
                .Select(u => GeneralSettingService.ConvertTiksToDateTime(gls, u.UntilTime, true).SimplifyFormatTime)
                .Aggregate(string.Empty, (acc, t) => acc.Length == 0 ? t : acc + "; " + t);
        }

        private string ResolveHoldByUsers(Record record, Dictionary<string, string> holdMap)
        {
            var holdIds = GetAllHoldIds(record);
            var allUsers = string.IsNullOrEmpty(record.HoldByUsers)
                ? new List<HoldUser>()
                : JsonConvert.DeserializeObject<List<HoldUser>>(record.HoldByUsers);

            if (record.HoldStatus && allUsers.Count == 0 && !string.IsNullOrEmpty(record.HoldBy))
            {
                allUsers.Add(new HoldUser { HoldId = record.HoldId, HoldBy = record.HoldBy });
            }

            return holdIds
                .Select(id => allUsers.FirstOrDefault(u => u.HoldId == id)?.HoldBy)
                .Where(b => !string.IsNullOrEmpty(b))
                .Distinct()
                .Aggregate(string.Empty, (acc, b) => acc.Length == 0 ? b : acc + "; " + b);
        }
        private string GetRecordFullPath(Record record)
        {
            string fullPath = string.Empty;
            PerformanceScope performance = null;
            try
            {
                if (mIsJob)
                {
                    performance = new PerformanceScope("ExportHoldsRecords.GetRecordFullPath", addToStatistics: mIsJob);
                }
                switch (record.SourceFlag)
                {
                    case (int)SourceFlag.SharePoint:
                    case (int)SourceFlag.SharePointOnPrem:
                    case (int)SourceFlag.OneDrive:
                    case (int)SourceFlag.Teams:
                        var scopeFullPath = exportCache.GetScopeFullPath(record.ScopeId, record.AveSiteId);
                        fullPath = string.IsNullOrEmpty(scopeFullPath)
                            ? string.Empty
                            : WebUtil.MakeFullUrl(scopeFullPath, record.DirPath);
                        if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
                        {
                            fullPath = WebUtil.GetListItemRealPath(fullPath);
                        }
                        break;
                    case (int)SourceFlag.Exchange:
                        fullPath = string.Format(AvePoint.RA.Common.RecordsConstants.EXOLocationFormat, record.EmailAddress, record.DirPath, new DateTime(record.TimeCreated).ToString("R"));
                        break;
                    case (int)SourceFlag.Google:
                    case (int)SourceFlag.Physical:
                        var locationPath = exportCache.GetLocationNodePath(record.LocationId);
                        fullPath = string.IsNullOrEmpty(locationPath) ? record.LeafName : $"{locationPath}/{record.LeafName}";
                        break;
                    case (int)SourceFlag.FileSystem:
                        fullPath = record.DirPath + "/" + record.LeafName;
                        break;
                    case (int)SourceFlag.AzureFileShare:
                        fullPath = record.DirPath + "/" + record.LeafName;
                        break;
                    case (int)SourceFlag.Box:
                        fullPath = record.DirPath;
                        break;
                    case >= 1000:
                        fullPath = record.LeafName;
                        break;
                    default:
                        logger.Warn("Invalid source flag, node id:{0} flag:{1}", record.NodeId, record.SourceFlag);
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting full path. NodeId:{0} Error:{1}", record.NodeId, e.ToString());
            }
            finally
            {
                if (mIsJob && performance != null)
                {
                    performance.Dispose();
                }
            }
            return fullPath;
        }
        private async Task<FileInfo> UploadBlobAsync()
        {
            using (new PerformanceScope("Upload blob to azure storage", "", true))
            {
                AvePoint.GCommon.ZipUtil.ZipFolder(FolderPath, FolderPath + ".zip", Encoding.UTF8);
                var customId = TenantLocalValue.LogonGroupId;
                var blobName = SecurityUtils.SafeCombinePath(customId, JobId + ".zip");
                try
                {
                    await Retryer.RetryAsync(() =>
                    {
                        blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, FolderPath + ".zip");
                        logger.Info($"Upload holds's records export success");
                        return Task.CompletedTask;
                    });
                }
                catch (Exception e)
                {
                    logger.Error($"Upload holds's records export failed,error is :{e}");
                    throw;
                }

                logger.Info($"finish to upload blob name:{blobName}");
                return new FileInfo(FolderPath + ".zip");
            }
        }

        private void UpdateDownloadDataInfo(RMDownloadDataInfo downCenterInfo, DownloadContentJobStatus downloadStatus)
        {
            downCenterInfo.JobStatus = (int)downloadStatus;
            var success = DownloadDataInfoDao.UpdateDownloadInfo(downCenterInfo);
            if (success)
            {
                logger.Info($"Update download file status to {downloadStatus} finished.");
            }
            else
            {
                logger.Info($"Update download file status to {downloadStatus} failed, retry update.");
                success = DownloadDataInfoDao.UpdateDownloadInfo(downCenterInfo);
                var status = success ? "finished" : "failed";
                logger.Info($"Update retry download file {status}.");
            }
        }
    }
}
