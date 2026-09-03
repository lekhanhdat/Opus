/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                    AvePoint, Inc.
 *                    525 Washington Blvd, Suite 1400
 *                    Jersey City, NJ 07310
 *                    United States of America
 *                    Telephone: +1-201-793-1111
 *                    WWW: www.avepoint.com
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
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.GCommon.Utility.AzureBlobStorage;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RMExplorer;
using AvePoint.Wrapper.Common;
using ExchangeUtility.Graph;
using Microsoft365.Authentication;
using CommonAveBPOSAccountInfo = AvePoint.Wrapper.Common.AveBPOSAccountInfo;
using System.IO;
using System.Text;
using Util;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using BaseJobDto = AvePoint.RA.Contract.JobMonitor.BaseJobDto;
using SerializerHelper = AvePoint.RA.Common.Global.Utils.SerializerHelper;

namespace RADownloadCenter.ReportExport
{
    public class ReportExportProcessor : GenerateAndUploadFileExecutor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ReportExportProcessor));

        private static readonly IRMReportService RMReportService = PlatformWindsorManager.GetService<IRMReportService>();

        private static readonly IJobMonitorDao JobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();

        private static readonly IRMSubJobDao SubJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
        private static readonly IRMRemoteNodeDao RemoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private readonly BaseJobDto BaseJobDto;

        private readonly int PageSize = 1000;

        private readonly string FolderPath;

        private readonly string FilePath;

        private readonly string JobId;

        private readonly int CountOfOneSheet = 65535;

        private readonly string SheetName = "Sheet";

        private readonly int NewReportJobType;
        private readonly string DestinationLibraryUrl;

        public ReportExportProcessor(string subJobId)
        {
            var rmSubJob = SubJobDao.GetSubJob(subJobId, true);
            var exportReportParameter = SerializerHelper.DeserializeByJsonSerializer<ExportReportCommonModel>(rmSubJob.JobContext.Content);
            var reportJobType = int.Parse(exportReportParameter.ReportJobType);
            var profile = RMReportService.GetProfileByIdForReportJob(exportReportParameter.ProfileId);
            if (reportJobType == JobTypeConstants.SOArchivedSiteReportPageType && profile != null)
            {
                reportJobType = (int)profile.Type;
            }
            DestinationLibraryUrl = GetDestinationLibraryUrl(profile?.Extension3);
            JobId = rmSubJob.ParentId;
            GenerateAndUploadFileManager.Init(rmSubJob.ParentId, JobType.ExportReportDetails);
            if (reportJobType == (int)JobType.ItemsFilesDueDisposal)
            {
                NewReportJobType = JobMonitorDao.GetJob(exportReportParameter.ReportJobId).JobType;
            }
            if (reportJobType == (int)JobType.RestoreReport || reportJobType == (int)JobType.OneDriverRestoreReport || reportJobType == (int)JobType.TeamsRestoreReport)
            {
                reportJobType = (int)JobType.GenerateRestoreReport;
            }
            BaseJobDto = new BaseJobDto()
            {
                Id = exportReportParameter.ReportJobId,
                JobType = reportJobType,
                ProfileName = exportReportParameter.ProfileName,
            };
            FolderPath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto);
            FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto, ".xlsx");
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }

        protected override string BaseJobId => JobId;

        protected override ArchiverExportReportDto ExportReportDto => throw new NotImplementedException();


        public override async Task RunAsync()
        {
            Logger.Info($"The job id is : [{BaseJobId}]");

            var reportProfile = DownloadDataInfoDao.GetDownloadDataInfoByJobId(BaseJobId);
            try
            {
                if (reportProfile == null)
                {
                    GenerateAndUploadFileManager.HasFailed = true;
                    throw new ArgumentNullException(nameof(reportProfile), "Can not find report download info!");
                }

                Logger.Info($"The DownloadDataInfo JobStatus is [{reportProfile.JobStatus}], JobId is [{reportProfile.JobId}]");

                reportProfile.JobStatus = (int)DownloadContentJobStatus.InProgress;

                await DownloadDataInfoDao.UpdateAsync(reportProfile);

                await GenerateDataAsync();

                Logger.Info("Generate Data success!");

                await UploadBlobAsync();

                if (fileInfo != null)
                {
                    reportProfile.FileSize = fileInfo.Length;
                }

                reportProfile.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                Logger.Info("Upload blob success!");

                reportProfile.JobStatus = (int)DownloadContentJobStatus.Finished;

                DownloadDataInfoDao.UpdateDownloadInfo(reportProfile);
            }
            catch (Exception e)
            {
                if (reportProfile != null)
                {
                    reportProfile.JobStatus = (int)DownloadContentJobStatus.Failed;
                    await DownloadDataInfoDao.UpdateAsync(reportProfile);
                }
                GenerateAndUploadFileManager.HasFailed = true;
                GenerateAndUploadFileManager.JobComment = e.Message;
                Logger.Error($"Generate And Upload File failed! Error : {e}");
            }
            finally
            {
                GenerateAndUploadFileManager.SendJobDetail();
                GenerateAndUploadFileManager.SetJobFinished();
            }
        }

        protected override async Task GenerateDataAsync()
        {
            var pageIndex = 1;
            //var totalCount = 0;
            var currentCount = 0;
            var isCreateHeader = true;
            var sheetIndex = 0;
            var workbookIndex = 0;
            var currentFilePath = GetWorkbookPath(workbookIndex);
            (var reportDetails, var totalCount) = await RMReportService.GetReportJobDatasAsync(PageSize, pageIndex, null, BaseJobDto);

            do
            {
                try
                {
                    currentCount += reportDetails.Count();
                    var datas = new string[reportDetails.Count() + 1][];
                    pageIndex++;
                    if (isCreateHeader)
                    {
                        datas = await RMReportService.GenerateReportForJobAsync(BaseJobDto.JobType, datas, NewReportJobType, reportDetails, true);
                        ReportUtil.CreateExcel(currentFilePath, SheetName, datas);
                        isCreateHeader = false;
                        Logger.Info($"Create Excel with header success,current count is {currentCount}");
                        continue;
                    }

                    if (currentCount >= CountOfOneSheet)
                    {
                        ReleaseWorkbookMemory(workbookIndex, currentFilePath);
                        workbookIndex++;
                        sheetIndex = 0;
                        currentFilePath = GetWorkbookPath(workbookIndex);
                        currentCount = reportDetails.Count();
                        datas = await RMReportService.GenerateReportForJobAsync(BaseJobDto.JobType, datas, NewReportJobType, reportDetails, true);
                        ReportUtil.CreateExcel(currentFilePath, SheetName, datas);
                        Logger.Info($"Create Excel with header success,current count is {currentCount},current workbook index is {workbookIndex}");
                        continue;
                    }

                    datas = await RMReportService.GenerateReportForJobAsync(BaseJobDto.JobType, datas, NewReportJobType, reportDetails, false);
                    ReportUtil.InsertDataToSheet(currentFilePath, datas, sheetIndex);
                    Logger.Info($"Insert data to sheet success,current count is {currentCount},current sheet index is {sheetIndex}");

                }
                catch (Exception e)
                {
                    Logger.Error($"Generate report detail to Excel error,current count is {currentCount},currrent sheet index is {sheetIndex},error : {e}");
                    GenerateAndUploadFileManager.HasFailed = true;
                    throw;
                }

            } while ((reportDetails = (await RMReportService.GetReportJobDatasAsync(PageSize, pageIndex, null, BaseJobDto)).Item1).Any());

            ReleaseWorkbookMemory(workbookIndex, currentFilePath);
        }

        private void ReleaseWorkbookMemory(int workbookIndex, string workbookPath)
        {
            Logger.Info($"Finish workbook index {workbookIndex},path:{workbookPath}. Release memory.");
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private string GetWorkbookPath(int workbookIndex)
        {
            if (workbookIndex <= 0)
            {
                return FilePath;
            }

            var directory = Path.GetDirectoryName(FilePath) ?? string.Empty;
            var fileName = Path.GetFileNameWithoutExtension(FilePath);
            var extension = Path.GetExtension(FilePath);
            var indexedName = $"{fileName}_{workbookIndex + 1}{extension}";
            return Path.Combine(directory, indexedName);
        }

        protected override async Task UploadBlobAsync()
        {
            var zipFilePath = FolderPath + ".zip";
            var fileName = Path.GetFileName(zipFilePath);
            AvePoint.GCommon.ZipUtil.ZipFolder(FolderPath, FolderPath + ".zip", Encoding.UTF8);
            if (!string.IsNullOrWhiteSpace(DestinationLibraryUrl))
            {
                await UploadReportFileToDestinationLibraryAsync(zipFilePath, fileName);
                fileInfo = new FileInfo(zipFilePath);
                return;
            }

            var customId = TenantLocalValue.LogonGroupId;
            var blobName = SecurityUtils.SafeCombinePath(customId, JobId + ".zip");
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, FolderPath + ".zip");
                    Logger.Info($"Upload Export Report Details success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Upload Export Report Details failed,error is :{e}");
                throw;
            }

            Logger.Info($"finish to upload blob name:{blobName}");

            fileInfo = new FileInfo(FolderPath + ".zip");
        }

        private static string GetDestinationLibraryUrl(string extension3)
        {
            if (string.IsNullOrWhiteSpace(extension3))
            {
                Logger.Info("Report export destination is empty.");
                return null;
            }

            try
            {
                Logger.Info("Deserializing the report export destination tree. Payload length: {0}", extension3.Length);
                var root = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(extension3);
                var destinationPath = FindSelectedDestinationPath(root, new HashSet<string>());
                Logger.Info("Resolved report export destination library URL: {0}", destinationPath ?? "<none>");
                return destinationPath;
            }
            catch (Exception exception)
            {
                Logger.Error("Failed to deserialize the report export destination. Error: {0}", exception);
                return null;
            }
        }

        private static string FindSelectedDestinationPath(RMSPTreeNode node, HashSet<string> visitedNodeIds)
        {
            if (node == null || (!string.IsNullOrWhiteSpace(node.Id) && !visitedNodeIds.Add(node.Id)))
            {
                return null;
            }

            if (node.CheckNumber == 1 && !string.IsNullOrWhiteSpace(node.FullPath))
            {
                Logger.Info("Selected report export destination node. NodeId: {0}, Level: {1}, FullPath: {2}", node.Id, node.Level, node.FullPath);
                return node.FullPath;
            }

            if (node.Children == null)
            {
                return null;
            }

            foreach (var child in node.Children)
            {
                var destinationPath = FindSelectedDestinationPath(child, visitedNodeIds);
                if (!string.IsNullOrWhiteSpace(destinationPath))
                {
                    return destinationPath;
                }
            }

            return null;
        }
        private static readonly HashSet<string> SitePrefixes =
        [
            "sites",
            "teams",
            "personal"
        ];

        public static string GetParentSiteUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty.");

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid URL.");

            var segments = uri.Segments
                .Select(s => Uri.UnescapeDataString(s.Trim('/')))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            if (segments.Count == 0)
                return $"{uri.Scheme}://{uri.Host}";

            for (int i = 0; i < segments.Count - 1; i++)
            {
                if (SitePrefixes.Contains(segments[i].ToLowerInvariant()))
                {
                    return $"{uri.Scheme}://{uri.Host}/{segments[i]}/{segments[i + 1]}";
                }
            }

            return $"{uri.Scheme}://{uri.Host}";
        }

        private async Task UploadReportFileToDestinationLibraryAsync(string archivePath, string uploadFileName)
        {
            Logger.Info($"[ReportExportProcessor] Start UploadReportFileToDestinationLibraryAsync. ArchivePath: '{archivePath}', UploadFileName: '{uploadFileName}', DestinationLibraryUrl: '{DestinationLibraryUrl}'");

            try
            {
                var parentSiteUrl = GetParentSiteUrl(DestinationLibraryUrl);
                var parentSiteUri = new Uri(parentSiteUrl);
                var fullDestinationUri = new Uri(DestinationLibraryUrl);
                var siteRelativePath = parentSiteUri.AbsolutePath.TrimEnd('/');
                var destinationServerRelativeUrl = fullDestinationUri.AbsolutePath;
                var relativePathForUploader = destinationServerRelativeUrl;
                if (relativePathForUploader.StartsWith(siteRelativePath, StringComparison.OrdinalIgnoreCase))
                {
                    relativePathForUploader = relativePathForUploader.Substring(siteRelativePath.Length).TrimStart('/');
                }

                Logger.Info($"[ReportExportProcessor] Target Parsed -> ParentSiteUrl: '{parentSiteUrl}', SiteRelativePath: '{siteRelativePath}', DestinationServerRelativeUrl: '{destinationServerRelativeUrl}', ProcessedRelativePath: '{relativePathForUploader}'");
                Logger.Info($"[ReportExportProcessor] Retrieving RemoteNodeSite for ParentSiteUrl: '{parentSiteUrl}'");
                var remoteNodeSite = RemoteNodeDao.GetRemoteSiteCollectionByUrl(parentSiteUrl);
                if (remoteNodeSite == null)
                {
                    Logger.Error($"[ReportExportProcessor] RemoteNodeSite is NULL for ParentSiteUrl: '{parentSiteUrl}'");
                }

                Logger.Info($"[ReportExportProcessor] Fetching BPOSInfo from PoolUserUtil...");
                var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteNodeSite);
                if (bposInfo == null)
                {
                    Logger.Error($"[ReportExportProcessor] BPOSInfo is NULL!");
                }

                var tokenProvider = ConvertToTokenProvider(bposInfo);
                Logger.Info($"[ReportExportProcessor] TokenProvider initialized successfully: {tokenProvider != null}");

                var uploader = new TeamsFileUploader(parentSiteUrl, relativePathForUploader, tokenProvider);
                Logger.Info($"[ReportExportProcessor] TeamsFileUploader instance created successfully.");

                FileStream fileStream = null;
                int retryCount = 0;
                const int maxRetries = 5;

                while (fileStream == null && retryCount < maxRetries)
                {
                    try
                    {
                        fileStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        Logger.Info($"[ReportExportProcessor] FileStream opened successfully on attempt {retryCount + 1}. Path: '{archivePath}', FileLength: {fileStream.Length} bytes.");
                    }
                    catch (IOException ioEx)
                    {
                        retryCount++;
                        Logger.Warn($"[ReportExportProcessor] IOException on opening file '{archivePath}' (Attempt {retryCount}/{maxRetries}). Details: {ioEx.Message}");
                        if (retryCount >= maxRetries)
                        {
                            Logger.Error($"[ReportExportProcessor] Exceeded maximum retries ({maxRetries}) to open file '{archivePath}'. Rethrowing exception.");
                            throw;
                        }
                        await Task.Delay(500);
                    }
                }
                using (fileStream)
                {
                    const long chunkThreshold = 5 * 1024 * 1024;
                    bool isChunkUpload = fileStream.Length > chunkThreshold;

                    Logger.Info($"[ReportExportProcessor] Starting upload file to SharePoint. Mode: {(isChunkUpload ? "ChunkUpload" : "DirectUpload")}, FileSize: {fileStream.Length} bytes, UploadFileName: '{uploadFileName}'");

                    if (isChunkUpload)
                    {
                        uploader.UploadFileByChunkToDocumentLibrary(string.Empty, uploadFileName, fileStream, true, (int)chunkThreshold, 10, 2000);
                    }
                    else
                    {
                        uploader.UploadFileToDocumentLibrary(string.Empty, uploadFileName, fileStream, true);
                    }
                    Logger.Info($"[ReportExportProcessor] Upload completed successfully for file '{uploadFileName}' to destination path '{relativePathForUploader}'.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[ReportExportProcessor] Exception occurred during UploadReportFileToDestinationLibraryAsync. ArchivePath: '{archivePath}', DestinationLibraryUrl: '{DestinationLibraryUrl}'. Exception Details: {ex}");
                throw;
            }
        }

        private static ITokenProvider ConvertToTokenProvider(CommonAveBPOSAccountInfo info)
        {
            if (info?.TokenProvider != null)
            {
                return info.TokenProvider;
            }

            if (!string.IsNullOrEmpty(GCommonRoleConfiguration.AosTokenApiURL))
            {
                return TokenProviderFactory.GetInstance().Get(info);
            }

            throw new InvalidOperationException("Token API URL is not configured.");
        }
    }
}
