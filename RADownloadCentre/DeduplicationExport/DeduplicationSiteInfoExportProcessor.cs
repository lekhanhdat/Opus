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
using System.Text;
using AvePoint.Common;
using AvePoint.GCommon.Contract.Server.Common.TimeZone;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Dedeplication;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.Archiver.Deduplication;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using Media.Service.ArchiverBackup.Index;
using RAArchiverCommon;
using RADownloadCenter;
using Storage;

namespace RADownloadCentre.DeduplicationExport
{
    public class DeduplicationSiteInfoExportProcessor : GenerateAndUploadFileExecutor
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DeduplicationSiteInfoExportProcessor));

        #region service
        private IStorageDeviceManager StorageDeviceManager => PlatformWindsorManager.GetService<IStorageDeviceManager>();
        public IGeneralSettingDao GeneralSettingDao => PlatformWindsorManager.GetService<IGeneralSettingDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();

        #endregion

        #region constant

        private static int indexLimit = 32257;
        private static int MAX_ROW_NUMBER_IN_ONE_SHEET = 500000;
        private static int MAX_SHEET_NUMBER_IN_ONE_BOOK = 4;
        public static readonly string SelectAllDedupFilesCount = $"SELECT COUNT(*) FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_3 = 1 AND COL_DEL_STATUS = 1 AND COL_RECYCLE_TIME > @DedupFrom AND COL_RECYCLE_TIME <= @DedupTo;";
        public static readonly string SelectAllDedupFiles = $"SELECT * FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_3 = 1 AND COL_DEL_STATUS = 1 AND COL_RECYCLE_TIME > @DedupFrom AND COL_RECYCLE_TIME <= @DedupTo LIMIT @OFFSET, @LENGTH;";
        #endregion

        #region base type cache
        private int workBookSheetIndex;
        private long sheetRowIndex;
        private string[][] datas;
        private Dictionary<string, string> deviceIdDeviceNameMap = new Dictionary<string, string>();

        public int totalDedupFilesCount = 0;
        public long totalDedupFilesSize = 0;

        public string mTimeZone;
        public TimeSpan mTimeZoneOffset = new TimeSpan();
        protected override string BaseJobId => jobId;
        private string FolderPath;
        private string fileName;
        private int fileIndex;
        private readonly string jobId;
        #endregion

        #region object instance cache
        private IXSystem indexLogicalDevice;
        public DedeplicationExportReportDto ExportDto;
        private ArchiverDeduplicationService DedupInfoManagement = new ArchiverDeduplicationService();
        public JobReportImps mJobreport;
        private CacheSettingDto cacheSetting;
        private IVolumeGenerator volumeGenerator = new VolumeGeneratorFactory().GetVolumeGenerator(ProductModule.ArchiverBackup);
        private ICacheService CacheManager = PlatformWindsorManager.GetService<ICacheService>();

        private IIndexDatabaseSynchronizer IndexSynchronizer = PlatformWindsorManager.GetService<IIndexDatabaseSynchronizer>();
        private IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        private GeneralSettingModel GeneralSetting;
        private RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();
        protected override ArchiverExportReportDto ExportReportDto => throw new NotImplementedException();
        #endregion



        public DeduplicationSiteInfoExportProcessor(string jobId, string param)
        {
            GenerateAndUploadFileManager.Init(jobId, JobType.ArchiverDeduplicationReport);
            GeneralSetting = GeneralSettingService.GetGeneralSettingAsync().Result;

            this.jobId = jobId;
            ExportDto = SerializerHelper.DeserializeByDataContractSerializer<DedeplicationExportReportDto>(param);

            var nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
            FolderPath = SecurityUtils.SafeCombinePath(
                JobReportUtility.GetDownloadArchiveDepulicationSiteInfoReportTempleFolder("Temple"),
                I18NEntity.GetString("RM_AR_Report_ExportArchiverDepulicationSite") + "_" + nowDateTimeStr + Guid.NewGuid());
            GenerateFolder(FolderPath);
            fileIndex++;
            fileName = I18NEntity.GetString("RM_AR_Report_ExportArchiverDepulicationSite") + ".xlsx";

            mJobreport = new JobReportImps(ReportMangerFactory.Instance.ReportManager);

            ReportMangerFactory.Instance.ReportManager.IncreaseBase(10);
            ReportMangerFactory.Instance.ReportManager.Increase(1);

            #region get time zone of config
            RMCPGeneralSetting rMCPGeneralSetting = GeneralSettingDao.GetGeneralSettingByUserAsync(TenantLocalValue.LogonGroupId).GetAwaiter().GetResult();
            List<AveTimeZone> timeZones = DateTimeUtil.GetAllStaticTimeZones();
            if (rMCPGeneralSetting != null && timeZones != null)
            {
                mTimeZone = timeZones.FirstOrDefault(timeZone => timeZone.Id == rMCPGeneralSetting.TimeZone)?.Zone ?? string.Empty;
                mTimeZoneOffset = timeZones.FirstOrDefault(timeZone => timeZone.Id == rMCPGeneralSetting.TimeZone)?.BaseUtcOffset ?? new TimeSpan();
            }
            #endregion
        }

        protected override async Task GenerateDataAsync()
        {
            Init();
            ExportAllDedupFiles();
        }

        private void GenerateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        protected override async Task UploadBlobAsync()
        {
            AvePoint.GCommon.ZipUtil.ZipFolder(FolderPath, FolderPath + ".zip", Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = SecurityUtils.SafeCombinePath(customId, jobId + ".zip");
            try
            {
                await Retryer.RetryAsync(() => 
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, FolderPath + ".zip");
                    Logger.Info($"Upload Deduplication Site Info Export success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Upload Deduplication Site Info Export failed,error is :{e}");
                throw;
            }

            Logger.Info($"finish to upload blob name:{blobName}");
            fileInfo = new FileInfo(FolderPath + ".zip");
        }

        private void Init()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("DedupReport.Init"))
            {
                Logger.Info($"Begin init");
                var indexStroage = StorageDeviceService.GetIndexDevice();
                if (indexStroage == null)
                {
                    throw new Exception("Cannot find index Storage Device.");
                }
                var indexLogicalDeviceDto = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexStroage);
                this.indexLogicalDevice = this.StorageDeviceManager.Open(indexLogicalDeviceDto.GetXRIS(PhysicalDeviceUsage.Index));

                this.cacheSetting = GetCacheSetting();
                this.CacheManager.Open(this.cacheSetting, false, true);
                Logger.Info($"Opened indexLogicalDevice successfully.");
            }
        }

        private CacheSettingDto GetCacheSetting()
        {
            var archiveTemp = BackgroundSettings.GetInstance().ArchiveTemp;
            if (!Directory.Exists(archiveTemp))
            {
                Directory.CreateDirectory(archiveTemp);
            }

            CacheSettingDto cache = new CacheSettingDto()
            {
                Extension = new CacheSettingExtension()
                {
                    Path = new List<PathMap>() {
                        new PathMap() {
                            DiskInfo = new DiskInfoDto() {
                                Path = archiveTemp
                            }
                        }
                    }
                }
            };
            return cache;
        }

        private void ExportAllDedupFiles()
        {
            Logger.Info($"Start get all dedup files from {this.ExportDto.DedupFrom} to {this.ExportDto.DedupTo}");
            var dedupSiteCollections = DedupInfoManagement.GetDedupSiteCollections(this.ExportDto.DedupFrom.Ticks, this.ExportDto.DedupTo.Ticks);
            int completedCount = 0;
            int totalCount = dedupSiteCollections.Count;
            ReportMangerFactory.Instance.ReportManager.IncreaseBase((int)(totalCount * 1.5));
            ReportMangerFactory.Instance.ReportManager.Increase((int)(totalCount * 0.2));
            int failedCount = 0;
            Logger.Info($"Total sitecollections has dedup files: {totalCount}");

            foreach (var siteUrl in dedupSiteCollections)
            {
                completedCount++;
                Logger.Info($"Start export dedup files of site: [{completedCount}/{totalCount}] - {siteUrl}");
                try
                {
                    ExportDedupFiles(siteUrl);
                }
                catch (Exception ex)
                {
                    failedCount++;
                    Logger.Error($"Export dedup files fails for: {siteUrl}. {ex}");
                }
                finally
                {
                    ReportMangerFactory.Instance.ReportManager.Increase(1);
                }
            }

            mJobreport.AddArchiverDedupReportJobSummaryReport(totalCount, failedCount, totalDedupFilesCount, totalDedupFilesSize);
            FlushDataToReportFile();//zailaige1 try catch
            Logger.Info($"end export all dedup files from {this.ExportDto.DedupFrom} to {this.ExportDto.DedupTo}");
        }

        private void ExportDedupFiles(string siteUrl)
        {
            var dataVolumn = this.volumeGenerator.GenerateDataVolume(new VolumeParameter() { SiteCollectionUrl = siteUrl, FarmName = "" });
            int successFilesCount = 0;
            long successFilesSize = 0;
            int failedFilesCount = 0;
            JobDetailsStatus stauts = JobDetailsStatus.Successful;
            foreach (var fileInfo in GetAllDedupFiles(siteUrl))
            {
                try
                {
                    totalDedupFilesCount++;
                    totalDedupFilesSize += fileInfo.ContentLength;
                    WriteToReportFile(dataVolumn, fileInfo);
                    successFilesCount++;
                    successFilesSize += fileInfo.ContentLength;
                }
                catch (Exception ex)
                {
                    failedFilesCount++;
                    stauts = JobDetailsStatus.Exception;
                    Logger.Error($"Real del data from device fails. Id:{fileInfo.Id}. {ex}");
                }
            }
            mJobreport.AddArchiverDedupReportJobDetailReport(jobId, siteUrl, successFilesCount, successFilesSize, stauts);
            Logger.Info($"Export Dedup Files Result. Success: {successFilesCount}, Failed: {failedFilesCount}, SC: {siteUrl}");
        }

        private void WriteToReportFile(string dataVolumn, ArchiverBodyIndex fileInfo)
        {
            if (datas == null || sheetRowIndex == 0)
            {
                sheetRowIndex = 0;
                datas = new string[MAX_ROW_NUMBER_IN_ONE_SHEET][];
                datas[sheetRowIndex++] = CreateExcelTitle();
            }
            datas[sheetRowIndex++] = ConvertFileInfoToExcelRow(fileInfo, dataVolumn);
            if (this.sheetRowIndex >= MAX_ROW_NUMBER_IN_ONE_SHEET)
            {
                FlushDataToReportFile();
            }
        }

        private void FlushDataToReportFile()
        {
            if (sheetRowIndex <= 0)
            {
                return;
            }
            this.sheetRowIndex = 0;
            if (++this.workBookSheetIndex == 1)
            {
                ReportUtil.CreateExcel(FolderPath + "/" + fileName, "Sheet", datas.Where(row => row != null).ToArray());
            }
            else
            {
                ReportUtil.InsertWorksheet(FolderPath + "/" + fileName, "Sheet" + workBookSheetIndex, datas.Where(row => row != null).ToArray());
            }
            if (this.workBookSheetIndex >= MAX_SHEET_NUMBER_IN_ONE_BOOK)
            {
                ++fileIndex;
                if (fileIndex > 1)
                {
                    fileName = I18NEntity.GetString("RM_AR_Report_ExportArchiverDepulicationSite") +  "("+ fileIndex + ")" + ".xlsx";
                }
                else
                {
                    fileName = I18NEntity.GetString("RM_AR_Report_ExportArchiverDepulicationSite") + ".xlsx";
                }
                this.workBookSheetIndex = 0;
            }
            datas = null;
        }




        private string[] CreateExcelTitle()
        {
            string[] title = new string[9];
            title[0] = I18NEntity.GetString("RM_JS_ArchiverDedupReportHeaderFileName");
            title[1] = I18NEntity.GetString("RM_JS_ArchiverDedupReportHeaderFileUrl");
            title[2] = @$"{I18NEntity.GetString("RM_JS_ArchiverDedupReportHeaderFileSize")}({I18NEntity.GetString("RM_FA_Progress_Unit_KB")})";
            title[3] = I18NEntity.GetString("RM_JS_ArchiverDedupReportHeaderSiteUrl");
            title[4] = $@"{I18NEntity.GetString("RM_JS_ArchiverDedupReportHeaderDedupTime")}{mTimeZone}";
            title[5] = I18NEntity.GetString("RM_JS_ArchiverDedupReportHeaderStoragePolicyAfterDedup");
            title[6] = I18NEntity.GetString("RM_JS_ArchiverDedupReportHeaderBlobPathAfterDedup");
            title[7] = I18NEntity.GetString("RM_JS_ArchiverDedupReportHeaderStoragePolicyBeforeDedup");
            title[8] = I18NEntity.GetString("RM_JS_ArchiverDedupReportHeaderBlobPathBeforeDedup");
            return title;
        }

        public string[] ConvertFileInfoToExcelRow(ArchiverBodyIndex fileInfo, string dataVolumn)
        {
            string[] data = new string[9];
            data[0] = fileInfo.Name;
            data[1] = FRTCommonUtility.GetFileUrl(fileInfo.ExtraInfo);
            data[2] = ConvertUnitUtil.ConvertToKB(JobDetailHelper.GetDataSizeToView(fileInfo.ContentLength));
            data[3] = fileInfo.SitePath;
            data[4] = (new DateTime(fileInfo.DedupTime, DateTimeKind.Utc) + mTimeZoneOffset).ToString(AveDateTimeUtility.DATETYPE011);
            data[5] = GetSourceFileStoragePolicy(fileInfo);
            data[6] = Path.Combine(dataVolumn, $"{fileInfo.DedupSourceFileJobId}_content_{fileInfo.ContentDataFileNumber}.dat");
            data[7] = GetStorageDeviceName(fileInfo.StoragePolicyId);
            data[8] = Path.Combine(dataVolumn, $"{fileInfo.JobId}_content_{fileInfo.DuplicateFileNumber}.dat");
            return data;
        }

        public string GetStorageDeviceName(string deviceId)
        {
            if(deviceId == null)
            {
                return string.Empty;
            }
            if (!deviceIdDeviceNameMap.ContainsKey(deviceId))
            {
                string storageDeviceName = string.Empty;
                
                try
                {
                    storageDeviceName = StorageDeviceService.GetStorageDeviceById(deviceId)?.Name ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Logger.Warn($@"have exception when use GetStorageDeviceById(deviceId) to get device,deviceId:{deviceId},ex:{ex}");
                }

                try
                {
                    if (string.IsNullOrWhiteSpace(storageDeviceName))
                    {
                        storageDeviceName = StorageDeviceService.GetStorageDeviceByDAOStoragePolicyId(deviceId)?.Name ?? string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($@"have exception when use GetStorageDeviceByDAOStoragePolicyId(deviceId) to get device,deviceId:{deviceId},ex:{ex}");
                }
                deviceIdDeviceNameMap[deviceId] = storageDeviceName;
            }
            return deviceIdDeviceNameMap[deviceId];
        }

        private string GetSourceFileStoragePolicy(ArchiverBodyIndex fileInfo)
        {
            DedupExtensionInfo dedupExtInfo = null;
            try
            {
                dedupExtInfo = SerializerHelper.DeserializeByDataContractJsonSerializer<DedupExtensionInfo>(fileInfo.DedupExtension);
            }
            catch (Exception ex)
            {
                Logger.Error($"Deserialize dedup extension fail. ExtStr: {fileInfo.DedupExtension}. Error: {ex}");
            }
            return GetStorageDeviceName(dedupExtInfo?.SourceFileStoragePolicyId);
        }





        private IIndexProcessor<ArchiverDedupIndexProcessorParameter> OpenDedupFileIndex(string siteUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("OpenDedupFileIndex"))
            {
                Logger.Info($"Begin opening Dedup File Index - {siteUrl}");

                var indexServiceOpenParameter = new ArchiverDedupIndexServiceOpenParameter()
                {
                    IndexDatabaseName = ServiceConstants.DedupIndexDBName,
                    IndexVolume = GetIndexVolume(siteUrl),
                    TreeMode = TreeMode.SiteCollectionMode,
                    IndexLogicalDeviceSystem = this.indexLogicalDevice,
                    IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                    CacheSetting = this.cacheSetting,
                };
                IndexSynchronizer.Initialize(indexServiceOpenParameter);
                return this.InitDedupFileIndexProcessor(indexServiceOpenParameter);
            }
        }

        private string GetIndexVolume(string siteUrl)
        {
            return this.volumeGenerator.GenerateIndexVolume(new VolumeParameter() { SiteCollectionUrl = siteUrl, FarmName = "" });
        }

        private IEnumerable<ArchiverBodyIndex> GetAllDedupFiles(string siteUrl)
        {
            IIndexProcessor<ArchiverDedupIndexProcessorParameter> dedupIndexProcessor = null;
            try
            {
                dedupIndexProcessor = OpenDedupFileIndex(siteUrl);
                var total = GetDedupFilesIndexesCount(dedupIndexProcessor);
                Logger.Info($"Total dedup files: {total}");

                for (int offset = 0; offset < total; offset += indexLimit)
                {
                    var dedupFiles = GetDedupFilesIndexes(dedupIndexProcessor, offset, indexLimit);
                    foreach (var fileInfo in dedupFiles)
                    {
                        yield return fileInfo;
                    }
                }
            }
            finally
            {
                try
                {
                    dedupIndexProcessor?.Close();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Close dedup index db processor fails for: {siteUrl}. {ex}");
                }
            }
        }

        private List<ArchiverBodyIndex> GetDedupFilesIndexes(IIndexProcessor<ArchiverDedupIndexProcessorParameter> dedupIndexProcessor, int offset, int pageSize)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GetDedupFilesIndexes"))
            {
                return dedupIndexProcessor.ExecuteQuery<ArchiverBodyIndex>(
                    SelectAllDedupFiles,
                    new Dictionary<string, object>
                    {
                        { "@OFFSET", offset },
                        { "@LENGTH", pageSize },
                        { "@DedupFrom", ExportDto.DedupFrom.Ticks },
                        { "@DedupTo", ExportDto.DedupTo.Ticks }
                    });
            }
        }

        private int GetDedupFilesIndexesCount(IIndexProcessor<ArchiverDedupIndexProcessorParameter> dedupIndexProcessor)
        {
            using (AvePerformanceScope pc1 = new AvePerformanceScope("GetDedupFilesIndexesCount"))
            {
                return Convert.ToInt32(dedupIndexProcessor.ExecuteScalar(
                    SelectAllDedupFilesCount,
                    new Dictionary<string, object>()
                    {
                        { "@DedupFrom", ExportDto.DedupFrom.Ticks },
                        { "@DedupTo", ExportDto.DedupTo.Ticks }
                    }
                ));
            }
        }

        private IIndexProcessor<ArchiverDedupIndexProcessorParameter> InitDedupFileIndexProcessor(ArchiverDedupIndexServiceOpenParameter openParam)
        {
            var realIndexDevice = this.CacheManager.CacheSystem;
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            var logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);

            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                if (MediaConfigInfo.CommonConfigInfo == null)
                {
                    MediaConfigInfo.CommonConfigInfo= PlatformWindsorManager.GetService<CommonConfigInfo>();
                }
                if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                {
                    var dbInfo = new IndexDatabaseInfo(openParam);
                    indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                }
                else
                {
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
                }
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));

                //azure不存在 dedup index，本地新创建，如果存在缓存的dedup index，此处会抛错，因此azure不存在时先删除本地cache的dedup index.
                FileInfo finfo = new FileInfo(indexDownLoadInfo.IndexFullPath);
                if (finfo.Exists)
                {
                    Logger.Info($"The dedup index file exist in media cache and delete it.Path:{indexDownLoadInfo.IndexFullPath}.");
                    try
                    {
                        finfo.Delete();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Delete dedup index file failed.Path:{indexDownLoadInfo.IndexFullPath}.Error:{ex}.");
                    }
                }
            }

            realIndexDevice.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            IdentityManager.IdentityMode = IdentityMode.Process;
            var param = new ArchiverDedupIndexProcessorParameter(IdentityManager.IdentityContent)
            {
                DownLoadResult = indexDownLoadInfo,
                IndexWorkingSystem = realIndexDevice,
            };
            //param.IsNeedCheckIntegrity = true;

            var dedupIndexProcessor = new IndexProcessor<ArchiverDedupIndexProcessorParameter>();
            dedupIndexProcessor.Open(param);
            Logger.Info("Open DedupFileIndex Finished.");
            return dedupIndexProcessor;
        }


    }
}
