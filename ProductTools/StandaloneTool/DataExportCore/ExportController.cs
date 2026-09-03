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
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using DataExportCore.Cache;
using DataExportCore.Utils;
using RecordsHotfixMaintenanceService;
using System.Text;

namespace DataExportCore;

public class ExportController
{
    private readonly RALogger logger = RALogger.GetInstance(typeof(ExportController));
    private List<ArchiverSiteBase> _archiverSites;
    private ExportOption _exportOption;
    private IndexDatabaseHelper _dbHelper;
    private ProgressManager _progressManager;
    private DateTime _jobStartTime;
    private readonly List<string> reportHeader = new List<string> {
        I18NEntity.GetString("SATool_ObjectLevel_SiteCollection"),
        I18NEntity.GetString("SATool_ReportHeader_Destination"),
        I18NEntity.GetString("SATool_ReportHeader_Status"),
        I18NEntity.GetString("SATool_ReportHeader_Comment"),
    };
    public ExportController(List<ArchiverSiteBase> archiverSites, ExportOption option, ProgressManager progressManager)
    {
        _archiverSites = archiverSites;
        _exportOption = option;
        _dbHelper = new IndexDatabaseHelper();
        _progressManager = progressManager;
        _jobStartTime = DateTime.Now;
    }

    public async Task Execute()
    {
        try
        {
            logger.Info($"Starting export process execution at [{_jobStartTime}].");
            Initialize();
            logger.Info($"ExportLocation: {_exportOption.ExportLocation},DataType: {_exportOption.DataType}, MaxThreadCount: {_exportOption.MaxThreadCount},TargetStorageType: {_exportOption.TargetStorageType} ");
            using (var semaphore = new SemaphoreSlim(_exportOption.MaxThreadCount, _exportOption.MaxThreadCount))
            {
                var tasks = new List<Task>();
                foreach (var archiverSite in _archiverSites)
                {
                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            logger.Info($"Begin execute export site [{archiverSite.SiteUrl}].");
                            var reporter = new Reporter
                            (
                                new Guid(archiverSite.SiteId),
                                ExportUtility.BuildExportPath(GlobalCache.ExportLocation, "", archiverSite.SiteUrl, NodeType.Site),
                                _exportOption.DataType
                            );
                            _progressManager.AddProgressReport(reporter);
                            ExportProcessor exportProcessor = new ExportProcessor(archiverSite, _dbHelper, _exportOption, reporter);
                            exportProcessor.Execute();

                            logger.Info($"Finished export site [{archiverSite.SiteUrl}], status [{reporter.GetJobStatus()}].");
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error while exporting site [{archiverSite.SiteUrl}]. Error: {ex}"); 
                            ExportUtility.AddUploadedSiteToReport(archiverSite.SiteUrl, ex.Message, GlobalCache.TargetStorageType, true);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }
                await Task.WhenAll(tasks);
            }
            logger.Info("Export process execution completed successfully.");
        }
        catch (Exception e)
        {
            logger.Error($"An error occured when executing export process. Error: {e}");
            throw;
        }
        finally
        {
            DeleteCacheData();
            RecordToFile();
        }
    }

    private void Initialize()
    {
        try
        {
            logger.Info("Initializing export process configuration.");
            ExportUtility.SetupConfiguration();
            CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
            GlobalCache.CustomPassword = _exportOption.EncryptedPassword;

            logger.Info("Opening export database with provided encrypted password.");
            _dbHelper.Open(_exportOption.EncryptedDBPath, GlobalCache.CustomPassword.ToPlainString());

            var settingProfiles = _dbHelper.ExecuteReader<SettingProfileExportDto>("SELECT * FROM SettingProfiles", []).ToDictionary(k => k.Name, v => v.Settings) ?? throw new Exception("SettingProfiles not found in Export db");

            GlobalCache.InitializeGlobalCache(settingProfiles, _exportOption);

            if (ExportUtility.IsNeedUploadAndDeleteCache())
            {
                GlobalCache.ExportLocation = Path.Combine(RecordsEnv.AppDomainRootFolder, "ArchiverCache", "restore", "content"); // for report 
            }

            if (string.IsNullOrEmpty(GlobalCache.MasterKey) || string.IsNullOrEmpty(GlobalCache.IndexDeviceId))
            {
                throw new Exception("Export db is missing important data. Cannot proceed exporting.");
            }

            var localDevices = _dbHelper.ExecuteReader<RMStorageDeviceInfoExportDto>("SELECT * FROM RMStorageDeviceInfoes", []).ToDictionary(v => v.Id, ConvertUtil.ConvertStorageDeviceExportDtoToLogicalDeviceDto, StringComparer.OrdinalIgnoreCase);

            var indexDevice = localDevices.GetValueOrDefault(GlobalCache.IndexDeviceId)?.PhysicalDrives.FirstOrDefault();

            if (GlobalCache.IsSkipAPData && (GlobalCache.IndexDeviceId.Equals(ExportUtility.AVEPOINT_STORAGE_ID, StringComparison.OrdinalIgnoreCase) 
                || indexDevice?.IsSystemStorage == true))
            {
                logger.Info($"The index storage is Avepoint Storage and the Skip AP data option is selected.");
                _progressManager.SetCompletedReport(Guid.NewGuid(), AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished, 0);
                throw new Exception("The index storage is Avepoint Storage and the Skip AP data option is selected");
            }

            GlobalDeviceCache.InitGlobalDeviceCaches(localDevices, _exportOption.AvepointMappingStorage, _exportOption.TargetStorage);

            logger.Info("Initialization completed successfully.");
        }
        catch (Exception e)
        {
            logger.Error($"Initialization failed. Error: {e}");
            throw;
        }
    }

    private void DeleteCacheData()
    {
        try
        {
            if (GlobalDeviceCache.CacheManager?.CacheSystem?.SystemLocation == null) 
            {
                logger.Info($"Cache Location not init. Skip dispose");
                return;
            }
            var indexVolumeCachePath = Path.Combine(GlobalDeviceCache.CacheManager.CacheSystem.SystemLocation, "data_archive");

            if (Directory.Exists(indexVolumeCachePath))
            {
                Directory.Delete(indexVolumeCachePath, true);
                logger.Info($"Deleted index volume cache: {indexVolumeCachePath}");

            }
            else
            {
                logger.Warn($"Cannot find index volume cache with path: {indexVolumeCachePath}");
            }
        }
        catch (Exception e)
        {
            logger.Error($"An error occurred while deleting cache data. Ex: {e}");
        }
    }

    private void RecordToFile()
    {
        try
        {
            var csvContent = new StringBuilder();
            var reportFileName = $"{I18NEntity.GetString("SATool_ReportSummary_Name")}_{_jobStartTime.ToString("yyyyMMddHHmmss")}.csv";
            var summaryReports = GlobalCache.SummaryReportDtos.ToList();
            csvContent.AppendLine(string.Join(",",reportHeader));

            foreach (var row in summaryReports)
            {
                if (row != null)
                {
                    var rowContent = string.Join(",", row.GetType()
                        .GetProperties()
                        .Select(p =>
                        {
                            var value = p.GetValue(row)?.ToString() ?? string.Empty;

                            if (value.Contains(',') || value.Contains('"'))
                            {
                                value = $"\"{value.Replace("\"", "\"\"")}\"";
                            }

                            return value;
                        }));
                    csvContent.AppendLine(rowContent);
                }
            }

            using (var writer = new StreamWriter($"{Path.Combine(GlobalCache.ExportLocation, reportFileName)}", false, Encoding.UTF8))
            {
                writer.Write(csvContent.ToString());
            }

            logger.Info($"Summary report file was successfully written to: {GlobalCache.ExportLocation}");
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred while writing report details to CSV file. Error: {ex}");
        }
    }
}