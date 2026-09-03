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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common.Configurations.Bootstrap;
using AvePoint.RA.CommonUtil;
using DataExportCore.Cache;
using DataExportCore.Utils;
using RecordsHotfixMaintenanceService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Util;

namespace DataExportCore
{
    public class ExportTeamsController(Dictionary<ArchiverSiteBase, List<ArchiverSiteBase>> archiverTeams, ExportOption option, ProgressManager progressManager, List<CommonSiteMasterIndexExportDto> commonSiteIndexes)
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(ExportTeamsController));
        private Dictionary<ArchiverSiteBase,List<ArchiverSiteBase>> _archiverTeams = archiverTeams;
        private ExportOption _exportOption = option;
        private IndexDatabaseHelper _dbHelper = new();
        private ProgressManager _progressManager = progressManager;
        private List<CommonSiteMasterIndexExportDto> _commonSiteIndexes = commonSiteIndexes;
        private DateTime _jobStartTime = DateTime.Now;
        private readonly List<string> reportHeader = new List<string> {
            I18NEntity.GetString("SATool_ObjectLevel_Teams"),
            I18NEntity.GetString("SATool_ReportHeader_Destination"),
            I18NEntity.GetString("SATool_ReportHeader_Status"),
            I18NEntity.GetString("SATool_ReportHeader_Comment"),
        };

        public async Task Execute()
        {
            try
            {
                logger.Info($"Starting export Teams process execution at [{_jobStartTime}].");
                Initialize();
                logger.Info($"ExportLocation: {_exportOption.ExportLocation},DataType: {_exportOption.DataType}, MaxThreadCount: {_exportOption.MaxThreadCount},TargetStorageType: {_exportOption.TargetStorageType} ");
                using (var semaphore = new SemaphoreSlim(_exportOption.MaxThreadCount, _exportOption.MaxThreadCount))
                {
                    var tasks = new List<Task>();
                    foreach (var teams in _archiverTeams)
                    {
                        foreach(var archiverSite in teams.Value)
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
                                        ExportUtility.BuildExportPath(Path.Combine(GlobalCache.ExportLocation, teams.Key.GroupAddress, I18NEntity.GetString("SATool_ExportPath_SiteCollections")), "", archiverSite.SiteUrl, NodeType.Site),
                                        _exportOption.DataType
                                    );
                                    _progressManager.AddProgressReport(reporter);
                                    ExportProcessor exportProcessor = new ExportProcessor(archiverSite, _dbHelper, _exportOption, reporter, teams.Key.GroupAddress);
                                    exportProcessor.Execute();

                                    logger.Info($"Finished export site [{archiverSite.SiteUrl}], status [{reporter.GetJobStatus()}].");
                                }
                                catch (Exception ex)
                                {
                                    logger.Error($"Error while exporting site [{archiverSite.SiteUrl}]. Error: {ex}");
                                    ExportUtility.AddUploadedTeamsToReport(teams.Key.GroupAddress, archiverSite.SiteUrl, ex.Message, GlobalCache.TargetStorageType, NodeType.Site, Enum.ExportStatus.Failed);
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }));
                        }

                        await semaphore.WaitAsync();
                        tasks.Add(Task.Run(() =>
                        {
                            Guid reportId = Guid.NewGuid();
                            try
                            {
                                logger.Info($"Begin execute export conversation [{teams.Key.GroupAddress}].");
                                var reporter = new Reporter
                                (
                                    reportId,
                                    Path.Combine(GlobalCache.ExportLocation, teams.Key.GroupAddress, I18NEntity.GetString("SATool_ExportPath_ChannelConversations")),
                                    _exportOption.DataType
                                );
                                _progressManager.AddProgressReport(reporter);
                                var commonSiteIndexInfo = _commonSiteIndexes.Where(_ => _.SiteURL.Equals(teams.Key.GroupAddress)).FirstOrDefault();
                                ArchiverGroupSiteMasterIndexExtension archiverInfo = null;
                                if (commonSiteIndexInfo != null)
                                    archiverInfo = SerializerHelper.DeserializeByDataContractSerializer<ArchiverGroupSiteMasterIndexExtension>(commonSiteIndexInfo.Extension);
                                else
                                {
                                    logger.Info($"Cannot find common site master index for current Teams channel conversation [{teams.Key.GroupAddress}].");
                                    //ExportUtility.AddUploadedTeamsToReport(teams.Key.GroupAddress, I18NEntity.GetString("SATool_ExportPath_ChannelConversations"), I18NEntity.GetString("SATool_ConversationDontArchiverBefore"), GlobalCache.TargetStorageType, NodeType.Conversation, Enum.ExportStatus.Skipped);
                                    _progressManager.SetCompletedReport(reportId, AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Skipped, 0);
                                    return;
                                }
                                ArchiverSiteBase archiverSite = new ArchiverSiteBase
                                {
                                    SiteUrl = archiverInfo.SPGroupSiteURL,
                                    SiteId = teams.Key.SiteId,
                                    GroupAddress = teams.Key.GroupAddress
                                };
                                ConversationExportProcessor exportProcessor = new ConversationExportProcessor(archiverSite, _dbHelper, _exportOption, reporter, teams.Key.GroupAddress);
                                exportProcessor.Execute();

                                logger.Info($"Finished export conversation [{archiverSite.SiteUrl}], status [{reporter.GetJobStatus()}].");
                            }
                            catch (Exception ex)
                            {
                                logger.Error($"Error while exporting conversation [{teams.Key.SiteUrl}]. Error: {ex}");
                                //ExportUtility.AddUploadedTeamsToReport(teams.Key.GroupAddress, I18NEntity.GetString("SATool_ExportPath_ChannelConversations"), ex.Message, GlobalCache.TargetStorageType, NodeType.Conversation, Enum.ExportStatus.Failed);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }));

                        await semaphore.WaitAsync();
                        tasks.Add(Task.Run(() =>
                        {
                            Guid reportId = Guid.NewGuid();
                            try
                            {
                                logger.Info($"Begin execute export mail box [{teams.Key.GroupAddress}].");
                                var reporter = new Reporter
                                (
                                    reportId,
                                    Path.Combine(GlobalCache.ExportLocation, teams.Key.GroupAddress, I18NEntity.GetString("SATool_ExportPath_GroupMailBoxes")),
                                    _exportOption.DataType
                                );
                                _progressManager.AddProgressReport(reporter);
                                var commonSiteIndexInfo = _commonSiteIndexes.Where(_ => _.SiteURL.Equals(teams.Key.GroupAddress)).FirstOrDefault();
                                ArchiverGroupSiteMasterIndexExtension archiverInfo = null;
                                if (commonSiteIndexInfo != null)
                                    archiverInfo = SerializerHelper.DeserializeByDataContractSerializer<ArchiverGroupSiteMasterIndexExtension>(commonSiteIndexInfo.Extension);
                                else
                                {
                                    logger.Info($"Cannot find common site master index for current Teams MailBox [{teams.Key.GroupAddress}].");
                                    //ExportUtility.AddUploadedTeamsToReport(teams.Key.GroupAddress, I18NEntity.GetString("SATool_ExportPath_GroupMailBoxes"), I18NEntity.GetString("SATool_MailBoxDontArchiverBefore"), GlobalCache.TargetStorageType, NodeType.ExchangeOnlineMailbox, Enum.ExportStatus.Skipped);
                                    _progressManager.SetCompletedReport(reportId, AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Skipped, 0);
                                    return;
                                }
                                ArchiverSiteBase archiverSite = new ArchiverSiteBase
                                {
                                    SiteUrl = archiverInfo.SPGroupSiteURL,
                                    SiteId = teams.Key.SiteId,
                                    GroupAddress = teams.Key.GroupAddress
                                };
                                ExchangeExportProcessor exportProcessor = new ExchangeExportProcessor(archiverSite, _dbHelper, _exportOption, reporter, teams.Key.GroupAddress);
                                exportProcessor.Execute();

                                logger.Info($"Finished export site [{archiverSite.SiteUrl}], status [{reporter.GetJobStatus()}].");
                            }
                            catch (Exception ex)
                            {
                                logger.Error($"Error while exporting mail box [{teams.Key.SiteUrl}]. Error: {ex}");
                                //ExportUtility.AddUploadedTeamsToReport(teams.Key.GroupAddress, I18NEntity.GetString("SATool_ExportPath_GroupMailBoxes"), ex.Message, GlobalCache.TargetStorageType, NodeType.ExchangeOnlineMailbox, Enum.ExportStatus.Failed);
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
            catch(Exception e)
            {
                logger.Error($"An error occured when executing export Teams process. Error: {e}");
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
                AsposeLicenseBootstrap.Setup();
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

        private void RecordToFile()
        {
            try
            {
                var csvContent = new StringBuilder();
                var reportFileName = $"{I18NEntity.GetString("SATool_ReportSummary_Name")}_{_jobStartTime.ToString("yyyyMMddHHmmss")}.csv";
                var summaryReports = GlobalCache.TeamsSummaryReportDtos.ToList();
                csvContent.AppendLine(string.Join(",", reportHeader));

                foreach (var row in summaryReports)
                {
                    if (row != null)
                    {
                        var rowContent = string.Join(",", row.GetType()
                            .GetProperties()
                            .Where(p => p.Name != nameof(TeamsSummaryReportDto.ObjectName))
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

        private void DeleteCacheData()
        {
            DeleteIndexFile("data_archive");
            DeleteIndexFile("data_exo_archive");
            DeleteIndexFile("data_teams_archive");
        }

        private void DeleteIndexFile(string indexName)
        {
            try
            {
                if (GlobalDeviceCache.CacheManager?.CacheSystem?.SystemLocation == null)
                {
                    logger.Info($"Cache Location not init. Skip dispose");
                    return;
                }

                var indexVolumeCachePath = Path.Combine(GlobalDeviceCache.CacheManager.CacheSystem.SystemLocation, indexName);

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
    }
}
