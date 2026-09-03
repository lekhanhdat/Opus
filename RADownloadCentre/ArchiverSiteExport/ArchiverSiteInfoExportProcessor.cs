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
using System.Xml;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Google.GDriveDeletedSizeInfo;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.Service.Services.JobMonitor.Detail;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using OpenNLP.Tools.Util;

namespace RADownloadCenter.ArchiverSiteExport
{
    public class ArchiverSiteInfoExportProcessor : GenerateAndUploadFileExecutor
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ArchiverSiteInfoExportProcessor));

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        private static readonly IStorageDeviceService StorageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();
        private static readonly ISettingProfilesDao SettingProfileDao = PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private static readonly GeneralSettingModel GeneralSetting = GeneralSettingService.GetGeneralSettingAsync().Result;

        private static readonly IArchiverIndexSubInfoDao s_archiverIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();

        private static readonly IArchiverSiteMasterIndexDao s_archiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();

        private static readonly IRestoreSearchService s_restoreSearchService = PlatformWindsorManager.GetService<IRestoreSearchService>();

        private static IRMArchiveTeamsGroupInfoDao ArchiveTeamsGroupInfoDao => PlatformWindsorManager.GetService<IRMArchiveTeamsGroupInfoDao>();

        private static IArchiverSiteMasterIndexService ArchiverSiteMasterIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexService>();
        private static IRMArchiveSiteInfoDao ArchiveSiteInfoDao => PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();

        private static IRMSiteDeletedSizeInfoDao siteDeletedSizeInfoDao => PlatformWindsorManager.GetService<IRMSiteDeletedSizeInfoDao>();
        private static IRMGDriveDeletedSizeInfoDao _gDriveDeletedSizeInfoDao => PlatformWindsorManager.GetService<IRMGDriveDeletedSizeInfoDao>();
        private static ICommonSiteMasterIndexDao s_commonMasterIndexDao => PlatformWindsorManager.GetService<ICommonSiteMasterIndexDao>();
        private static readonly IEXOArchiverIndexSubInfoDao s_exoArchiverIndexSubInfoDao = PlatformWindsorManager.GetService<IEXOArchiverIndexSubInfoDao>();
        private static IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private static string FullPath = @"C:\RECO_Reports\Job Report\Archiver Sites File Information";

        private static readonly int CountOfOneSheet = 200000;

        private readonly string FolderPath;

        private readonly string JobId;

        public readonly ArchiverExportReportDto ExportDto;
        private IMCacheSettingService _CacheSettingService;
        public IMCacheSettingService CacheSettingService
        {
            get
            {
                if (_CacheSettingService == null)
                {
                    _CacheSettingService = new CacheSettingService();
                    return _CacheSettingService;
                }
                else
                {
                    return _CacheSettingService;
                }
            }
        }
        public ArchiverSiteInfoExportProcessor(string jobId, string param)
        {
            GenerateAndUploadFileManager.Init(jobId, JobType.ArchiverExport);
            JobId = jobId;
            ExportDto = SerializerHelper.DeserializeByDataContractSerializer<ArchiverExportReportDto>(param);
            var nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
            FolderPath = SecurityUtils.SafeCombinePath(
                JobReportUtility.GetDownloadArchiverSiteInfoReportTempleFolder("Temple"), I18NEntity.GetString("RM_AR_Report_ExportArchiverSite") + "_" + nowDateTimeStr + Guid.NewGuid());
        }

        protected override string BaseJobId => JobId;
        protected override ArchiverExportReportDto ExportReportDto => ExportDto;

        protected override async Task GenerateDataAsync()
        {
            using (new PerformanceScope("Create csv file async", "", true))
            {
                try
                {
                    Logger.Info($"Start to generate data for report type :{ExportDto.ReportType}, time range: {ExportDto.TimeRange}");
                    switch (ExportDto.ReportType)
                    {
                        case ReportType.SiteCollection:
                            await GenerateAllSiteData();
                            break;
                        case ReportType.AllItem:
                            await GenerateAllItemsData();
                            break;
                        case ReportType.AllSubSite:
                            await GenerateAllSubSitesData();
                            break;
                        case ReportType.AllTeamsGroup:
                            await GenerateAllTeamsGroupData();
                            break;
                        case ReportType.AllRetentionSimulate:
                            await GenerateAllRetentionSimualteData();
                            break;
                        case ReportType.AllGoogleDrive:
                            await GenerateAllGoogleDriveData();
                            break;
                        case ReportType.AllGoogleItem:
                            await GenerateAllGDriveItemsData();
                            break;
                        default:
                            Logger.Warn($"the report type cannot be reported,type:{ExportDto.ReportType.ToString()}");
                            break;
                    }

                    Logger.Info($"Finish generating data for report type :{ExportDto.ReportType}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Create excel error, error :{ex.ToString()}");
                    throw;
                }
            };
        }
        private async Task GenerateAllSiteData()
        {
            try
            {
                List<ArchiverSiteSizeInfo> queryResult = await CollectSiteArchived();
                List<ArchiverSiteSizeInfo> splitedResult = new List<ArchiverSiteSizeInfo>();
                foreach (ArchiverSiteSizeInfo info in queryResult)
                {
                    if (splitedResult.Count() == CountOfOneSheet)
                    {
                        BulkWriteItemStringToCsv(splitedResult);
                        splitedResult.Clear();
                    }
                    splitedResult.Add(info);
                }
                BulkWriteItemStringToCsv(splitedResult);
            }
            catch (Exception e)
            {
                Logger.Error($"Create excel error, error :{e}");
                throw;
            }
        }
        private async Task GenerateAllGoogleDriveData()
        {
            try
            {
                List<ArchiverSiteSizeInfo> queryResult = await CollectDriveArchived();
                List<ArchiverSiteSizeInfo> splitedResult = new List<ArchiverSiteSizeInfo>();
                foreach (ArchiverSiteSizeInfo info in queryResult)
                {
                    if (splitedResult.Count() == CountOfOneSheet)
                    {
                        BulkWriteGDriveItemStringToCsv(splitedResult);
                        splitedResult.Clear();
                    }
                    splitedResult.Add(info);
                }
                BulkWriteGDriveItemStringToCsv(splitedResult);
            }
            catch (Exception e)
            {
                Logger.Error($"Create excel error, error :{e}");
                throw;
            }
        }

        private  IRMRetentionSimulateInfosDao RetentionSimulateInfosDao => PlatformWindsorManager.GetService<IRMRetentionSimulateInfosDao>();
        private  IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();


        private  long GetNextRetentionRunTime()
        {
            var schedule = RMScheduleDao.GetScheduleByType(AvePoint.RA.Contract.Schedule.ScheduleType.ArchiveDataRetentionSchedule);
            if (schedule != null && schedule.Count > 0)
            {
                return schedule.FirstOrDefault().NextTime;
            }
            return 0;
        }

        private async Task GenerateAllRetentionSimualteData()
        {
            try
            {
                ArchiverRetentionDashboardDetailWorker worker = new ArchiverRetentionDashboardDetailWorker();

                var mainJob = RetentionSimulateInfosDao.GetAll().FirstOrDefault(r => r.SourceFlag == (int)SourceFlag.All);
                if (mainJob == null || mainJob.MergeReportState != (int)MergeIndexState.Succeed)
                {
                    return;
                }

                var jobDto = new BaseJobDto()
                {
                    Id = $"{mainJob.RetentionJobId}",
                    JobType = (int)JobType.ArchiverRetentionSimulate,
                    //AddValues = addValues
                };

                var queryResult = worker.GetData(int.MaxValue, 1, "", jobDto);

                List<JMJobDetails> splitedResult = new List<JMJobDetails>();

                foreach (var info in queryResult)
                {
                    if (splitedResult.Count() == CountOfOneSheet)
                    {
                        BulkWriteItemStringToCsv(splitedResult);
                        splitedResult.Clear();
                    }
                    splitedResult.Add(info);
                }
                BulkWriteItemStringToCsv(splitedResult);
            }
            catch (Exception e)
            {
                Logger.Error($"Create excel error, error :{e}");
                throw;
            }
        }

        private void BulkWriteItemStringToCsv(List<JMJobDetails> siteInfos)
        {
            if (siteInfos == null || siteInfos.Count() == 0)
            {
                return;
            }
            List<string> headers = new List<string>
            {
                 I18NEntity.GetString("RM_DSB_Retention_Column_FileName"),
                 I18NEntity.GetString("RM_DSB_Retention_Column_Url"),
                 I18NEntity.GetString("RM_DSB_Retention_Column_ContentSource"),
                 I18NEntity.GetString("RM_DSB_Retention_Column_Size"),
                 I18NEntity.GetString("RM_DSB_Retention_Column_Setting"),
                 I18NEntity.GetString("RM_DSB_Retention_Column_Storage"),
            };

            String nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
            String fileName = I18NEntity.GetString("RM_AR_Report_ExportRententionSimulateData") + "_" + nowDateTimeStr;
            using StreamWriter writer = GetStreamWriter(fileName, headers);
            siteInfos.ForEach(info => writer.WriteLine(GenerateStringForCsv(info)));

            string GenerateStringForCsv(JMJobDetails data)
            {
                try
                {
                    if (data is JMArchiverRententionDashboardDetails item)
                    {
                        var fields = new List<string>
                    {
                        item.FileName,
                        item.SiteUrl,
                        TelemetryUtility.ConvertSourceFlag(item.SourceFlag),
                        item.SizeStr,
                        string.Format(I18NEntity.GetString("RM_DSB_Retention_Column_SettingValue"),item.RetentionSource,item.RetentionKeepDate,GenerateRetentionKeepDateUnitStr(item.RetentionKeepDateUnit)),
                        item.SrcStorageName
                    };
                        return StringUtils.ToCSVString(fields.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Convert history to cell failed. ex:{ex}");
                }
                return null;
            }

            string GenerateRetentionKeepDateUnitStr(int retentionKeepDateUnit)
            {
                switch (retentionKeepDateUnit)
                {
                    case 0:
                        return I18NEntity.GetString("RM_DSB_Retention_DayUnit");
                    case 1:
                        return I18NEntity.GetString("RM_DSB_Retention_WeekUnit");
                    case 2:
                        return I18NEntity.GetString("RM_DSB_Retention_Column_Storage");
                    case 3:
                        return I18NEntity.GetString("RM_DSB_Retention_YearUnit");
                }
                return "";
            }
        }


        private void BulkWriteItemStringToCsv(List<ArchiverSiteSizeInfo> siteInfos)
        {
            if (siteInfos == null || siteInfos.Count() == 0)
            {
                return;
            }
            List<string> headers = AssembleArchiverSiteHeaderTittleForCsv();
            String nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
            String fileName = I18NEntity.GetString("RM_AR_Report_ExportArchiverSite") + "_" + nowDateTimeStr;
            using StreamWriter writer = GetStreamWriter(fileName, headers);
            siteInfos.ForEach(info => writer.WriteLine(GenerateSiteCollectionStringForCsv(info)));
        }
        private void BulkWriteGDriveItemStringToCsv(List<ArchiverSiteSizeInfo> siteInfos)
        {
            if (siteInfos == null || siteInfos.Count() == 0)
            {
                return;
            }
            List<string> headers = AssembleArchiverGDriveHeaderTittleForCsv();
            String nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
            String fileName = I18NEntity.GetString("RM_AR_Report_ExportArchiveGDrive") + "_" + nowDateTimeStr;
            using StreamWriter writer = GetStreamWriter(fileName, headers);
            siteInfos.ForEach(info => writer.WriteLine(GenerateRecordItemStringForCsv(info)));
        }
        private static List<string> AssembleArchiverGDriveHeaderTittleForCsv()
        {
            return new List<string>
            {
                 I18NEntity.GetString("RM_DSB_Column_DriveName"),
                 I18NEntity.GetString("RM_DSB_Column_Size_CSV"),
                 I18NEntity.GetString("RM_DSB_Column_Deleted_Size_CSV"),
            };
        }

        private void BulkWriteSubSiteStringToCsv(List<ArchiverSubSiteInfo> subSiteInfos, string fileName)
        {
            List<string> headers = AssembleHeadersForExportAllSubSites();
            String nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
            fileName = fileName.Contains('/') ? fileName.Substring(fileName.LastIndexOf("/") + 1) : fileName;
            using StreamWriter writer = GetStreamWriter(fileName, headers);
            subSiteInfos.ForEach(info => writer.WriteLine(GenerateRecordSubSiteStringForCsv(info)));
        }

        private async Task<List<ArchiverSiteSizeInfo>> CollectSiteArchived()
        {
            try
            {
                var allSiteCollectionInfos = s_archiverSiteMasterIndexDao.GetAllSiteCollectionNodsInfo(new List<int> { (int) SourceFlag.Google}).DistinctBy(item => item.SiteURL).ToList();
                Logger.Info($"Get distinct archived site collection info success, count is: {allSiteCollectionInfos.Count}");
                Dictionary<string, List<string>> siteUrlAndJobIdMapping = null;
                Dictionary<string, Tuple<string, long>> deletedSizeInfos = null;
                switch (ExportDto.TimeRange)
                {
                    case TimeRange.All:
                        deletedSizeInfos = siteDeletedSizeInfoDao.GetSiteDeleteSizeInfoWithSiteId();
                        siteUrlAndJobIdMapping = s_archiverSiteMasterIndexDao.GetAllBackupSiteCollectionDistinctJobIdMappings(allSiteCollectionInfos.Select(site => site.SiteURL).ToList());
                        break;
                    case TimeRange.Custom:
                        deletedSizeInfos = siteDeletedSizeInfoDao.GetSiteDeleteSizeInfoWithSiteId(ExportDto.StartTime.Ticks, ExportDto.EndTime.Ticks);
                        siteUrlAndJobIdMapping = s_archiverSiteMasterIndexDao.GetAllBackupSiteCollectionDistinctJobIdMappings(
                        allSiteCollectionInfos.Select(site => site.SiteURL).ToList(), ExportDto.StartTime.Ticks, ExportDto.EndTime.Ticks);
                        break;
                    case TimeRange.None:
                    default:
                        Logger.Error($"Time range exception, time range:{ExportDto.TimeRange}");
                        break;
                }

                var siteUrlAndSizeMapping = s_archiverIndexSubInfoDao.GetAllArchiverIndexSubInfoBySiteUrls(siteUrlAndJobIdMapping);
                var allSiteCollectionContract = allSiteCollectionInfos.ConvertAll(site => ConvertToDto(site));
                var allSiteCollectionNodesInfo = allSiteCollectionContract.ConvertAll(site => new SiteCollectionNodesInfo() { SiteGroupId = site.WebId, SiteUrl = site.SiteURL, SPObjectId = site.SiteId }).ToList();
                var filterPolicy = new ArchiverRestoreResult()
                {
                    SerchContract = new BackupDataSearchContract() { FilterPolicy = new ArchiverRestoreFilter() { Level = PolicyLevel.Document } }
                };
                var allArchivedSiteInfoStr = await s_restoreSearchService.GetSearchTreeResultForJobAsync(allSiteCollectionContract, filterPolicy, allSiteCollectionNodesInfo);
                var allArchivedSiteInfos = SerializerHelper.DeserializeByDataContractSerializer<List<RMArchiveSiteInfo>>(allArchivedSiteInfoStr);
                Logger.Info($"Get distinct archived site info from index success, count is: {allArchivedSiteInfos.Count}");



                allArchivedSiteInfos.ForEach(site =>
                {
                    site.ArchivedSize = siteUrlAndSizeMapping[site.SiteUrl];
                    site.DeletedSize = deletedSizeInfos.ContainsKey(site.SiteUrl) ? (double)deletedSizeInfos[site.SiteUrl]?.Item2 / AvePoint.RA.Contract.Common.ContractConstants.GBSizeInterval : 0;
                });

                var siteUrlList = allArchivedSiteInfos.Select(site => site.SiteUrl).ToList();
                foreach (var info in deletedSizeInfos)
                {
                    if (!siteUrlList.Contains(info.Key))
                    {
                        allArchivedSiteInfos.Add(new RMArchiveSiteInfo()
                        {
                            Id = Guid.NewGuid().ToString(),
                            SiteUrl = info.Key,
                            SiteId = info.Value.Item1,
                            ArchivedSize = 0,
                            DeletedSize = (double)info.Value.Item2 / AvePoint.RA.Contract.Common.ContractConstants.GBSizeInterval,
                            VersionNumber = 0,
                            FileNumber = 0,
                        });
                    }
                }
                string doubleFormatStr = "0.###############################";// unable use scientific notation
                var siteInfos = allArchivedSiteInfos.Select(site => new ArchiverSiteSizeInfo()
                {
                    SiteUrl = site.SiteUrl,
                    TotalSize = site.ArchivedSize.ToString(doubleFormatStr) + "GB",
                    SiteId = site.SiteId,
                    TotalDeleteSize = site.DeletedSize.ToString(doubleFormatStr) + "GB",
                    TotalSizeArchivedByM365 = site.ArchiveBy365Size.ToString(doubleFormatStr) + "GB",
                }).ToList();
                return siteInfos;
            }
            catch
            {
                throw;
            }
        }

        private async Task<List<ArchiverSiteSizeInfo>> CollectDriveArchived()
        {
            try
            {
                var allGDriveInfos = s_archiverSiteMasterIndexDao.GetAllGoogleNodesInfo().DistinctBy(item => item.SiteId).ToList();
                Logger.Info($"Get distinct archived drive info success, count is: {allGDriveInfos.Count}");
                Dictionary<string, List<string>> driveIdsAndJobIdMapping = null;
                Dictionary<string, GDriveDeletedSizeInfo> deletedSizeInfos = null;
                switch (ExportDto.TimeRange)
                {
                    case TimeRange.All:
                        deletedSizeInfos = _gDriveDeletedSizeInfoDao.GetGDriveDeleteSizeInfoWithDriveId();
                        driveIdsAndJobIdMapping = s_archiverSiteMasterIndexDao.GetAllBackupGDriveDistinctJobIdMappings(allGDriveInfos.Select(site => site.SiteId).ToList());
                        break;
                    case TimeRange.Custom:
                        deletedSizeInfos = _gDriveDeletedSizeInfoDao.GetGDriveDeleteSizeInfoWithDriveId(ExportDto.StartTime.Ticks, ExportDto.EndTime.Ticks);
                        driveIdsAndJobIdMapping = s_archiverSiteMasterIndexDao.GetAllBackupGDriveCollectionDistinctJobIdMappings(
                        allGDriveInfos.Select(site => site.SiteId).ToList(), ExportDto.StartTime.Ticks, ExportDto.EndTime.Ticks);
                        break;
                    case TimeRange.None:
                    default:
                        Logger.Error($"Time range exception, time range:{ExportDto.TimeRange}");
                        break;
                }

                var driveIdAndSizeMapping = s_archiverIndexSubInfoDao.GetAllGoogleArchiverIndexSubInfoByDriveIds(driveIdsAndJobIdMapping);
                var allGDriveContract = allGDriveInfos.ConvertAll(drive => ConvertToDto(drive));
                var allGDriveNodesInfo = allGDriveContract.ConvertAll(site => new SiteCollectionNodesInfo() { SiteGroupId = site.WebId, SiteUrl = site.SiteURL, SPObjectId = site.SiteId }).ToList();
                var filterPolicy = new ArchiverRestoreResult()
                {
                    SerchContract = new BackupDataSearchContract() { FilterPolicy = new ArchiverRestoreFilter() { Level = PolicyLevel.Document } }
                };
                var allArchivedGDriveInfoStr = await s_restoreSearchService.GetGDriveSearchTreeResultForJobAsync(allGDriveContract, filterPolicy, allGDriveNodesInfo);
                var allArchivedGDriveInfos = SerializerHelper.DeserializeByDataContractSerializer<List<RMArchiveGDriveInfo>>(allArchivedGDriveInfoStr);
                Logger.Info($"Get distinct archived drive info from index success, count is: {allArchivedGDriveInfos.Count}");

                allArchivedGDriveInfos.ForEach(site =>
                {
                    site.ArchivedSize = driveIdAndSizeMapping[site.DriveId];
                    site.DeletedSize = deletedSizeInfos.ContainsKey(site.DriveId) ? (deletedSizeInfos[site.DriveId]?.DeletedSize ?? 0.00f) / ContractConstants.GBSizeInterval : 0;
                });

                var driveIdList = allArchivedGDriveInfos.Select(site => site.DriveId).ToList();
                foreach (var info in deletedSizeInfos)
                {
                    if (!driveIdList.Contains(info.Key))
                    {
                        allArchivedGDriveInfos.Add(new RMArchiveGDriveInfo()
                        {
                            Id = Guid.NewGuid().ToString(),
                            DriveId = info.Key,
                            DriveName = info.Value.DriveName,
                            ArchivedSize = 0,
                            DeletedSize = (double)info.Value.DeletedSize / AvePoint.RA.Contract.Common.ContractConstants.GBSizeInterval,
                            VersionNumber = 0,
                            FileNumber = 0,
                        });
                    }
                }
                string doubleFormatStr = "0.###############################";
                var siteInfos = allArchivedGDriveInfos.Select(drive => new ArchiverSiteSizeInfo()
                {
                    SiteUrl = drive.DriveName,
                    TotalSize = drive.ArchivedSize.ToString(doubleFormatStr) + "GB",
                    SiteId = drive.DriveId,
                    TotalDeleteSize = drive.DeletedSize.ToString(doubleFormatStr) + "GB",
                }).ToList();
                return siteInfos;
            }
            catch
            {
                throw;
            }
        }

        private async Task GenerateAllGDriveItemsData()
        {
            try
            {
                var gls = await GeneralSettingService.GetGeneralSettingAsync();
                List<string> itemHeaders = AssembleHeadersForExportAllItems();
                int indexCount = 1;
                foreach (var siteNode in ExportDto.SiteInfos)
                {
                    GDriveRestoreParamDto paramDto = AssembleGDriveExportParamDto(siteNode.SiteId, siteNode.SiteUrl);
                    GDriveBrowseInfo browseInfo = new GDriveBrowseInfo(paramDto, ProductModule.GDriveArchiverBackup);
                    string csvFileName = siteNode.SiteUrl;
                    csvFileName = csvFileName + $"({indexCount})";
                    using var writer = GetStreamWriter(csvFileName, itemHeaders);
                    var advancedSearchService = new ArchiverAdvancedSearchService();
                    var searchResult = advancedSearchService.SearchForGDriveExportItems(browseInfo, ExportDto.TimeRange);
                    var itemLines = GenerateAllGDriveItemsStringForCsv(searchResult, gls);
                    itemLines.ForEach(writer.WriteLine);
                    indexCount++;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Create item excel error, error :{e}");
                throw;
            }
        }


        private static ArchiverSiteMasterIndexContract ConvertToDto(AvePoint.RA.DB.Model.ArchiverSiteMasterIndex domain)
        {
            ArchiverSiteMasterIndexContract contract = null;
            if (domain != null)
            {
                contract = new ArchiverSiteMasterIndexContract();
                contract.ArchiverTime = domain.ArchiverTime;
                contract.Id = domain.Id;
                contract.JobId = domain.JobId;
                contract.JobState = domain.JobState;
                contract.SiteId = domain.SiteId;
                contract.SiteURL = domain.SiteURL;
                contract.SPVersion = domain.SPVersion;
                contract.WebId = domain.SiteGroupId;
                contract.MergeIndexState = (MergeIndexState)domain.MergeIndexState;
                contract.StorageInfo = domain.StorageInfo;
                if (!string.IsNullOrWhiteSpace(domain.Extension))
                {
                    contract.Extension = SerializerHelper.DeserializeByDataContractSerializer<ArchiverSiteMasterIndexExtension>(domain.Extension);
                }
            }
            return contract;
        }




        private StreamWriter GetStreamWriter(string fileName, List<string> headers)
        {
            FullPath = GenerateFullPath(fileName);
            var stream = new FileStream(FullPath, FileMode.CreateNew, FileAccess.ReadWrite);
            var writer = new StreamWriter(stream, Encoding.UTF8);
            var headerLine = StringUtils.ToCSVString(headers.ToArray());
            writer.WriteLine(headerLine);
            return writer;
        }

        private async Task GenerateAllSubSitesData()
        {
            Logger.Info($"Start to generate all sub site data for site collections, site count :{ExportDto.SiteInfos.Count}");
            try
            {
                List<string> archivedSiteUrls = ArchiverSiteMasterIndexService.GetExistingSiteCollectionUrls(ExportDto.SiteInfos.Select(info => info.SiteUrl));
                Logger.Info($"Get existing archived site collection url success, count is :{archivedSiteUrls.Count}");
                foreach (var siteNode in ExportDto.SiteInfos)
                {
                    if (!archivedSiteUrls.Contains(siteNode.SiteUrl))
                    {
                        Logger.Info($"no any archive record for :{siteNode.SiteUrl}, skip it");
                        BulkWriteSubSiteStringToCsv(new List<ArchiverSubSiteInfo>(), $"{siteNode.SiteUrl}(1)");
                        continue;
                    }
                    Logger.Info($"Start to generate all sub site data for site :{siteNode.SiteUrl}");
                    ArchiverRestoreParamDto paramDto = AssembleExportParamDto(siteNode.SiteUrl);
                    ArchiverBrowseInfo browseInfo = new ArchiverBrowseInfo(paramDto);
                    var advancedSearchService = new ArchiverAdvancedSearchService();
                    List<ArchiverBasicIndex> subSites = advancedSearchService.SearchSubSiteForExportSubSites(browseInfo, ExportDto.TimeRange);
                    Logger.Info($"Get sub site list for site :{siteNode.SiteUrl} success, sub site count is :{subSites.Count}");
                    List<ArchiverSubSiteInfo> SubSiteInfos = new List<ArchiverSubSiteInfo>();
                    foreach (var subSite in subSites)
                    {
                        ArchiverSubSiteInfo info = new ArchiverSubSiteInfo() { SubSiteUrl = subSite.Url };
                        info.TotalSize = advancedSearchService.SearchArchivedSizeForExportSubSites(browseInfo, subSite.Url, ExportDto.TimeRange);
                        SubSiteInfos.Add(info);
                    }

                    int fileIndex = 1;
                    List<ArchiverSubSiteInfo> splitedResult = new List<ArchiverSubSiteInfo>();
                    foreach (ArchiverSubSiteInfo info in SubSiteInfos)
                    {
                        if (splitedResult.Count() == CountOfOneSheet)
                        {

                            BulkWriteSubSiteStringToCsv(splitedResult, $"{siteNode.SiteUrl}({fileIndex++})");
                            splitedResult.Clear();
                        }
                        splitedResult.Add(info);
                    }
                    BulkWriteSubSiteStringToCsv(splitedResult, $"{siteNode.SiteUrl}({fileIndex++})");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Create subSite excel error, error :{ex}");
                throw;
            }
        }


        private async Task GenerateAllItemsData()
        {
            Logger.Info($"Start to generate all items data for site collections, site count :{ExportDto.SiteInfos.Count}");
            try
            {
                List<string> archivedSiteUrls = ArchiverSiteMasterIndexService.GetExistingSiteCollectionUrls(ExportDto.SiteInfos.Select(info => info.SiteUrl));
                Logger.Info($"Get existing archived site collection url success, count is :{archivedSiteUrls.Count}");
                var gls = await GeneralSettingService.GetGeneralSettingAsync();
                List<string> itemHeaders = AssembleHeadersForExportAllItems();
                int indexCount = 1;
                foreach (var siteNode in ExportDto.SiteInfos)
                {
                    string csvFileName = siteNode.SiteUrl.Contains('/') ? siteNode.SiteUrl.Substring(siteNode.SiteUrl.LastIndexOf("/") + 1) : siteNode.SiteUrl;
                    csvFileName = csvFileName + $"({indexCount++})";
                    using var writer = GetStreamWriter(csvFileName, itemHeaders);
                    if (!archivedSiteUrls.Contains(siteNode.SiteUrl))
                    {
                        Logger.Info($"current site :{siteNode.SiteUrl} no any archive data, skip process");
                        continue;
                    }
                    Logger.Info($"Start to generate all items data for site :{siteNode.SiteUrl}");
                    var advancedSearchService = new ArchiverAdvancedSearchService();
                    ArchiverRestoreParamDto paramDto = AssembleExportParamDto(siteNode.SiteUrl);
                    ArchiverBrowseInfo browseInfo = new ArchiverBrowseInfo(paramDto);
                    await foreach (var index in advancedSearchService.SearchForExportItems(browseInfo, ExportDto.TimeRange))
                    {
                        var line = GenerateItemStringForCsv(index, gls);
                        if (!string.IsNullOrEmpty(line))
                        {
                            await writer.WriteLineAsync(line);
                        }
                    }

                    Logger.Info($"Finish generating all items data for site :{siteNode.SiteUrl}");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Create item excel error, error :{e}");
                throw;
            }
        }
        private ArchiverRestoreParamDto AssembleExportParamDto(string siteUrl)
        {
            StorageDeviceDto Indexdevice = null;
            SettingProfileDto indexDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.IndexDevice,
                Name = "UsingIndexDevice"
            };
            var indexDBInfo = SettingProfileDao.Load(indexDto);
            if (indexDBInfo != null)
            {
                Indexdevice = StorageDeviceService.GetStorageDeviceById(indexDBInfo.Settings, needDecryptSecert: true);
            }
            ArchiverRestoreParamDto param = new ArchiverRestoreParamDto
            {
                Path = siteUrl,
                //Level = searchNode.Level,
                //BackupJobId = index.JobId,
                FarmName = string.Empty,
                //BackupPlanId = index.PlanId,
                //LogicalDevice = SOUtilityService.GetLogicalDeviceInfo(index.LogicalDeviceId),
                IndexLogicalDevice = Indexdevice,
                LoadTreeOption = ArchiverLoadTreeOption.SiteCollectionMode,
                //StorageInfo = index.StorageInfo,
                SiteUrl = siteUrl,
                StartTime = ExportDto.StartTime.Ticks,
                EndTime = ExportDto.EndTime.Ticks,
            };
            param.CacheLocation = CacheSettingService.GetBrowserCacheInfo();
            return param;
        }
        private GDriveRestoreParamDto AssembleGDriveExportParamDto(string driveId, string driveName)
        {
            StorageDeviceDto Indexdevice = null;
            SettingProfileDto indexDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.IndexDevice,
                Name = "UsingIndexDevice"
            };
            var indexDBInfo = SettingProfileDao.Load(indexDto);
            if (indexDBInfo != null)
            {
                Indexdevice = StorageDeviceService.GetStorageDeviceById(indexDBInfo.Settings, needDecryptSecert: true);
            }
            var param = new GDriveRestoreParamDto
            {
                Path = driveName,
                FarmName = string.Empty,
                IndexLogicalDevice = Indexdevice,
                LoadTreeOption = ArchiverLoadTreeOption.SiteCollectionMode,
                SiteUrl = driveName,
                StartTime = ExportDto.StartTime.Ticks,
                EndTime = ExportDto.EndTime.Ticks,
                DriveId = driveId,
                TenantId = RemoteNodeDao.GetTenantIdByObjectId(driveId)
            };
            param.CacheLocation = CacheSettingService.GetBrowserCacheInfo();
            return param;
        }

        private List<string> AssembleHeadersForExportAllItems()
        {
            return new List<string>() {
                I18NEntity.GetString("RM_JS_JMD_Grid_Type"),
                I18NEntity.GetString("StorageOptimization.Service_684E2AE1-DC1D-47DF-AD8F-025251ABF811"),
                I18NEntity.GetString("RM_DSB_Column_Size_CSV"),
                I18NEntity.GetString("StorageOptimization.Service_84F15AC4-BDBF-4F4D-A036-B63EBA03C404"),
                I18NEntity.GetString("StorageOptimization.Service_86D5507D-A47C-46F8-8D85-C7CBD183B23F"),
                I18NEntity.GetString("StorageOptimization.Service_1D64CD2C-D447-4C0D-813C-20925D93E1C3")
            };
        }
        private static List<string> AssembleHeadersForExportAllSubSites()
        {
            return new List<string>() {
                 I18NEntity.GetString("StorageOptimization.Service_684E2AE1-DC1D-47DF-AD8F-025251ABF811"),
                 I18NEntity.GetString("RM_DSB_Column_Size_CSV"),
            };
        }

        protected override async Task UploadBlobAsync()
        {
            AvePoint.GCommon.ZipUtil.ZipFolder(FolderPath, FolderPath + ".zip", Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = SecurityUtils.SafeCombinePath(customId, JobId + ".zip");
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, FolderPath + ".zip");
                    Logger.Info($"Upload Archiver Site Info Export success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Upload Archiver Site Info Export failed,error is :{e}");
                throw;
            }

            Logger.Info($"finish to upload blob name:{blobName}");
            fileInfo = new FileInfo(FolderPath + ".zip");
        }

        private static List<string> AssembleArchiverSiteHeaderTittleForCsv()
        {
            return new List<string>
            {
                 I18NEntity.GetString("RM_DSB_Column_URL"),
                 I18NEntity.GetString("RM_DSB_Column_External_Archived_Size_CSV"),
                 I18NEntity.GetString("RM_DSB_Column_Destroyed_Size_CSV"),
                 I18NEntity.GetString("RM_DSB_Column_M365_Archived_Size_CSV"),
            };
        }

        private static string? GenerateSiteCollectionStringForCsv(ArchiverSiteSizeInfo site)
        {
            try
            {
                var fields = new List<string>
                    {
                        site.SiteUrl,
                        ConvertUnitUtil.ConvertToKB(site.TotalSize),
                        ConvertUnitUtil.ConvertToKB(site.TotalDeleteSize),
                        ConvertUnitUtil.ConvertToKB(site.TotalSizeArchivedByM365),
                    };
                return StringUtils.ToCSVString(fields.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Convert history to cell failed,history item id {site.SiteUrl},{ex}");
                var detail = new JMImportSPSettingDetail() { ObjectName = site.SiteUrl, Url = site.SiteUrl, Status = JobDetailsStatus.Failed, Comment = ex.Message };
                GenerateAndUploadFileManager.JobDetailList.Add(detail);
                return null;
            }
        }

        private string GenerateFullPath(string fileName)
        {
            FullPath = SecurityUtils.SafeCombinePath(FolderPath, fileName + ".csv");
            //FullPath = FolderPath + Path.DirectorySeparatorChar + fileName + ".csv";
            if (!System.IO.Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            return FullPath;
        }

        private static string? GenerateRecordSubSiteStringForCsv(ArchiverSubSiteInfo subSite)
        {
            try
            {
                var fields = new List<string>
                    {
                        subSite.SubSiteUrl,
                        ConvertUnitUtil.ConvertToKB(JobDetailHelper.GetDataSizeToView(subSite.TotalSize))
                    };
                return StringUtils.ToCSVString(fields.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Convert history to cell failed,history subSite {subSite.SubSiteUrl},{ex}");
                var detail = new JMImportSPSettingDetail() { ObjectName = subSite.SubSiteUrl, Url = subSite.SubSiteUrl, Status = JobDetailsStatus.Failed, Comment = ex.Message };
                GenerateAndUploadFileManager.JobDetailList.Add(detail);
                return null;
            }
        }

        private static string? GenerateRecordItemStringForCsv(ArchiverSiteSizeInfo site)
        {
            try
            {
                var fields = new List<string>
                    {
                        site.SiteUrl,
                        ConvertUnitUtil.ConvertToKB(site.TotalSize),
                        ConvertUnitUtil.ConvertToKB(site.TotalDeleteSize),
                    };
                return StringUtils.ToCSVString(fields.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Convert history to cell failed,history item id {site.SiteUrl},{ex}");
                var detail = new JMImportSPSettingDetail() { ObjectName = site.SiteUrl, Url = site.SiteUrl, Status = JobDetailsStatus.Failed, Comment = ex.Message };
                GenerateAndUploadFileManager.JobDetailList.Add(detail);
                return null;
            }
        }


        private string? GenerateItemStringForCsv(ArchiverBasicIndex index, GeneralSettingModel gls)
        {
            try
            {
                var fields = new List<string>
                    {
                        I18NEntity.GetString(JobReportUtility.ConverTypeToLevel(index.Type)),
                        GetFullPath(index.ExtraInfo, index.Url),
                        ConvertUnitUtil.ConvertToKB(JobDetailHelper.GetDataSizeToView(index.ContentLength)),
                        index.CreateTime == 0 ? "N/A":GeneralSettingService.ConvertTiksToDateTime(gls, index.CreateTime, true).SimplifyFormatTime,
                        index.ModifyTime == 0 ? "N/A":GeneralSettingService.ConvertTiksToDateTime(gls, index.ModifyTime, true).SimplifyFormatTime,
                        index.ArchiveTime == 0 ? "N/A":GeneralSettingService.ConvertTiksToDateTime(gls, index.ArchiveTime, true).SimplifyFormatTime
                    };

                return StringUtils.ToCSVString(fields.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Convert history to cell failed,history item id {index.SitePath},{ex}");
                var detail = new JMImportSPSettingDetail() { ObjectName = index.SitePath, Url = index.SitePath, Status = JobDetailsStatus.Failed, Comment = ex.Message };
                GenerateAndUploadFileManager.JobDetailList.Add(detail);
                return null;
            }
        }
        private List<string> GenerateAllGDriveItemsStringForCsv(List<GoogleBasicIndex> itemsIndexInfo, GeneralSettingModel gls)
        {
            var res = new List<string>();

            foreach (var index in itemsIndexInfo)
            {
                try
                {
                    var fields = new List<string>
                    {
                        I18NEntity.GetString(JobReportUtility.ConvertTypeToLevel(index.Type)),
                        index.Type == (int)GDriveDataType.FileVersion ? $"{index.Path}:{index.VersionNumber}" : index.Path,
                        ConvertUnitUtil.ConvertToKB(JobDetailHelper.GetDataSizeToView(index.ContentLength)),
                        ConvertTimeToStringCsv(index.CreateTime, index.Type, gls),
                        ConvertTimeToStringCsv(index.ModifyTime, index.Type, gls),
                        index.ArchiveTime == 0 ? "N/A":GeneralSettingService.ConvertTiksToDateTime(gls, index.ArchiveTime, true).SimplifyFormatTime
                    };

                    var dataLine = StringUtils.ToCSVString(fields.ToArray());
                    res.Add(dataLine);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Convert history to cell failed,history item id {index.DriveName},{ex}");
                    var detail = new JMImportSPSettingDetail() { ObjectName = index.DriveName, Url = index.DriveName, Status = JobDetailsStatus.Failed, Comment = ex.Message };
                    GenerateAndUploadFileManager.JobDetailList.Add(detail);
                }
            }

            return res;
        }

        private string ConvertTimeToStringCsv(long time, int type, GeneralSettingModel gls)
        {
            return type switch
            {
                (int)GDriveDataType.MyDrive => "N/A",
                (int)GDriveDataType.SharedDrive => "N/A",
                (int)GDriveDataType.Folder => "N/A",
                _ => time == 0 ? "N/A" : GeneralSettingService.ConvertTiksToDateTime(gls, time, true).SimplifyFormatTime,
            };
        }

        private string GetFullPath(string extraInfo, string url)
        {
            var document = new XmlDocument();
            document.LoadXml(extraInfo);
            var apUrlElements = document.GetElementsByTagName("HeaderExtraAttribute");
            if (apUrlElements != null && apUrlElements.Count > 0)
            {
                var apUrl = apUrlElements[0]?.Attributes["APUrl"]?.Value ?? url;
                return apUrl.Contains("\\") ? apUrl?.Replace("\\", "/") : apUrl;
            }
            return url;
        }

        #region Teams group
        private async Task GenerateAllTeamsGroupData()
        {
            try
            {
                List<ArchiverTeamsGroupSizeInfo> queryResult = await CollectTeamsGroupArchivedData();
                List<ArchiverTeamsGroupSizeInfo> splitedResult = new List<ArchiverTeamsGroupSizeInfo>();
                foreach (ArchiverTeamsGroupSizeInfo info in queryResult)
                {
                    if (splitedResult.Count() == CountOfOneSheet)
                    {
                        BulkWriteItemStringToCsv(splitedResult);
                        splitedResult.Clear();
                    }
                    splitedResult.Add(info);
                }
                BulkWriteItemStringToCsv(splitedResult);
            }
            catch (Exception e)
            {
                Logger.Error($"Create excel error, error :{e}");
                throw;
            }
        }

        private List<ArchiverTeamsGroupSizeInfo> CollectTeamsGroupArchivedDataByTimeRange()
        {
            Logger.Info($"Collect by custom time range, StartTime - EndTime: [{ExportDto.StartTime.Ticks} - {ExportDto.EndTime.Ticks}]");
            var timeRange = (ExportDto.StartTime.Ticks, ExportDto.EndTime.Ticks);

            Logger.Info("Collect existing group/channel site's sp group");
            Dictionary<string, string> existingSiteAndMailboxMapping = RemoteNodeDao.GetAllSiteAndSPGroupMapping();

            Logger.Info("Collect all teams archived size by custom time range");
            (Dictionary<string, double> teamsAndSizeMapping, Dictionary<string, string> siteAndMailboxMapping) = s_commonMasterIndexDao.GetAllTeamsArchivedSizeAndSiteURLs(timeRange);

            existingSiteAndMailboxMapping.ForEach(i => siteAndMailboxMapping[i.Key] = i.Value);

            Logger.Info("Collect all sites archived size by custom time range");
            var allSiteArchivedSizeMapping = s_archiverSiteMasterIndexDao.GetAllSiteArchivedSizeInGBAndGroupMailBox(ExportDto.StartTime.Ticks, ExportDto.EndTime.Ticks);

            allSiteArchivedSizeMapping.ForEach(i => siteAndMailboxMapping[i.Key] = i.Value.groupMailboxAddress);

            Logger.Info("Collect all mailbox archived size by custom time range");
            var mailboxAndSizeMapping = s_exoArchiverIndexSubInfoDao.GetAllEXOArchivedSizeMapping(timeRange);

            Dictionary<string, (double, double)> groupAndSizesMapping = new();

            foreach (var item in teamsAndSizeMapping)
            {
                RecordArchivedSize(item.Key, item.Value, false);
            }

            foreach (var item in mailboxAndSizeMapping)
            {
                RecordArchivedSize(item.Key, item.Value, false);
            }

            foreach (var item in allSiteArchivedSizeMapping)
            {
                var siteUrl = item.Key;
                if (siteAndMailboxMapping.TryGetValue(siteUrl, out var spGroup))
                {
                    RecordArchivedSize(spGroup, item.Value.archivedSizeInGB, true);
                }
                else
                {
                    Logger.Info($"Site url [{siteUrl}] not found in mailbox address mapping.");
                }
            }

            string doubleFormatStr = "0.###############################";// unable use scientific notation
            var teamsGroupInfoes = groupAndSizesMapping.Select(item => new ArchiverTeamsGroupSizeInfo()
            {
                //TeamsGroupId = site.TeamsGroupId,
                MailboxAddress = item.Key,
                TotalArchivedSize = item.Value.Item1.ToString(doubleFormatStr) + "GB",
                TotalArchivedSizeWithoutRelatedSites = item.Value.Item2.ToString(doubleFormatStr) + "GB",
            }).OrderBy(i => i.MailboxAddress).ToList();

            return teamsGroupInfoes;


            void RecordArchivedSize(string spGroup, double size, bool isArchivedSiteSize)
            {
                if (!groupAndSizesMapping.TryGetValue(spGroup, out var sizes))
                {
                    sizes = (0, 0);
                }
                groupAndSizesMapping[spGroup] = (sizes.Item1 + size, isArchivedSiteSize ? sizes.Item2 : (sizes.Item2 + size));
            }
        }

        private async Task<List<ArchiverTeamsGroupSizeInfo>> CollectTeamsGroupArchivedData()
        {
            Logger.Info("Start collect Teams group archived data.");

            switch (ExportDto.TimeRange)
            {
                case TimeRange.All:
                    var allData = await ArchiveTeamsGroupInfoDao.GetAllArchiverTeamsSizeInfoAsync();
                    return allData.OrderBy(i => i.MailboxAddress).ToList();
                case TimeRange.Custom:
                    return CollectTeamsGroupArchivedDataByTimeRange();
                case TimeRange.None:
                default:
                    throw new Exception($"Time range exception, time range:{ExportDto.TimeRange}");
            }
        }

        private static RMArchiveTeamsGroupInfo ConvertToArchiverTeamsGroupInfo(AvePoint.RA.DB.Model.CommonSiteMasterIndex domain)
        {
            RMArchiveTeamsGroupInfo contract = null;
            if (domain != null)
            {
                contract = new RMArchiveTeamsGroupInfo();
                contract.Id = Guid.NewGuid().ToString();
                contract.ArchivedSize = 0;
                contract.ArchivedSizeWithoutRelatedSites = 0;
                contract.MailboxAddress = domain.SiteURL;
                contract.TeamsGroupId = domain.TeamId;
            }
            return contract;
        }

        private void BulkWriteItemStringToCsv(List<ArchiverTeamsGroupSizeInfo> infoes)
        {
            List<string> headers = AssembleArchiverTeamsGroupHeaderTitleForCsv();
            String nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
            String fileName = I18NEntity.GetString("RM_AR_Report_ExportArchiverTeamsGroups") + "_" + nowDateTimeStr;
            using StreamWriter writer = GetStreamWriter(fileName, headers);
            if (infoes != null && infoes.Any())
            {
                infoes.OrderBy(i => i.MailboxAddress).ForEach(info => writer.WriteLine(GenerateRecordItemStringForCsv(info)));
            }
        }

        private static string? GenerateRecordItemStringForCsv(ArchiverTeamsGroupSizeInfo info)
        {
            try
            {
                var fields = new List<string>
                    {
                        info.MailboxAddress,
                        ConvertUnitUtil.ConvertToKB(info.TotalArchivedSize),
                        ConvertUnitUtil.ConvertToKB(info.TotalArchivedSizeWithoutRelatedSites),
                    };
                return StringUtils.ToCSVString(fields.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Convert history to cell failed,history item id {info.MailboxAddress},{ex}");
                var detail = new JMImportSPSettingDetail() { ObjectName = info.MailboxAddress, Url = info.MailboxAddress, Status = JobDetailsStatus.Failed, Comment = ex.Message };
                GenerateAndUploadFileManager.JobDetailList.Add(detail);
                return null;
            }

        }

        private static List<string> AssembleArchiverTeamsGroupHeaderTitleForCsv()
        {
            return new List<string>
            {
                 I18NEntity.GetString("RM_DSB_Column_TeamsAndGroups"),
                 I18NEntity.GetString("RM_DSB_Column_Teams_TotalArchivedSize_CSV"),
                 I18NEntity.GetString("RM_DSB_Column_Teams_TotalSize_CSV"),
            };
        }
        #endregion
    }
}
