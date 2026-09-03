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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.BackupIndex;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Common;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Destroy;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Excel;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ActionStatus = AvePoint.RA.Contract.Schedule.ActionStatus;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC
{
    public class ExportSiteMetricsProcessor
    {
        public string JPMCFakeAndCombineRuleId = "e854d182-595e-4174-8137-471c690a031c";
        public string JPMCFakeOrCombineRuleId = "4e785347-854c-4a59-a3fa-d8866489a277";

        private JobContext jobContext = null;
        private string JobId = string.Empty;
        private RALogger logger = RALogger.GetInstance(typeof(ExportSiteMetricsProcessor));

        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMArchiveSiteInfoDao ArchiveSiteInfoDao => PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        private readonly List<RemoteSiteCollection> mRemoteSiteCollections;

        private readonly List<string> mDesignLists = [];
        private List<JPMCTenantConfig> mJPMCTenantConfigs;
        private JPMCExcelJsonConfig mJPMCExcelConfig;

        private SiteMetricsJobParameterDto mJobParameter;
        private RemoteSiteCollection mSiteCollection;
        private IAveSite mSPSite;
        private readonly List<IAveList> mSupportLibraries = [];
        private ExcelExportProcessor4JPMC mExportProcessor;
        private IScanDataReader4JPMC mActiveReader;
        private BackupDataIndexReader4JPMC mBackupReader;
        private DestroyProcessor4JPMC mDestroyProcessor;
        private string JobSummary = string.Empty;

        public ExportSiteMetricsProcessor(string jobId)
        {
            this.JobId = jobId;
            jobContext = JobContext.GetInstance(jobId, JobType.ExportSiteMetrics);
            jobContext.ReportManager.StartUpdateJobProgress();

            // mRemoteSiteCollections = RABrowserClient.GetAuthorisedRemoteSiteCollectionsByUser();
            mDesignLists = WebUtil.GetDesignLists(false);
            InitJPMConfig();
        }

        private void InitJPMConfig()
        {
            var jsonConfig = KeyValueDao.GetValueByKey("JPMC_Customization");
            List<JPMCTenantConfig> configs = null;
            if (jsonConfig != null)
            {
                configs = JsonConvert.DeserializeObject<List<JPMCTenantConfig>>(jsonConfig.Value);
                configs.ForEach(c =>
                {
                    // var remoteSite = mRemoteSiteCollections.FirstOrDefault(s => s.url == c.ConfigSiteUrl);
                    var remoteSite = RABrowserClient.GetRemoteSiteCollectionByUrl(c.ConfigSiteUrl);
                    if (remoteSite != null)
                    {
                        c.ConfigSite = remoteSite;
                        c.M365TenantId = remoteSite.TenantId;
                    }
                    else
                    {
                        logger.Warn($"Can not get this site:{c.ConfigSiteUrl}");
                    }
                });
            }
            mJPMCTenantConfigs = configs ?? [];

            var canConnectConfigSite = false;
            var canConnectConfigFile = false;

            foreach (var tenantConfig in mJPMCTenantConfigs)
            {
                try
                {
                    if (tenantConfig.ConfigSite == null)
                    {
                        continue;
                    }

                    logger.Info($"Start getting the excel config file from site:{tenantConfig.ConfigSite.url}");
                    var bposInfo = PoolUserUtil.GetBPOSInfoAsync(tenantConfig.ConfigSite).GetAwaiter().GetResult();
                    var mFactory = MultiAppUtil.CreateAveObjectModelFactory(tenantConfig.ConfigSite.url, bposInfo, AveContextKind.ClientObjectModel);
                    var spSite = mFactory.CreateSite(tenantConfig.ConfigSite.url);
                    if (spSite == null)
                    {
                        logger.Warn($"Cannot connect to config site:{tenantConfig.ConfigSite.url}");
                        continue;
                    }
                    canConnectConfigSite = true;

                    var configList = spSite.RootWeb.GetListFromUrl(JPMCSiteMetricsReportCache.ConfigListName);
                    if (configList == null)
                    {
                        logger.Warn($"Cannot get config list from site:{tenantConfig.ConfigSite.url}, configListName: {JPMCSiteMetricsReportCache.ConfigListName}");
                        continue;
                    }

                    var jpmcExcelJsonConfigFile = configList.RootFolder.Files[JPMCSiteMetricsReportCache.JPMCExcelJsonConfigFileName];
                    if (jpmcExcelJsonConfigFile == null || !jpmcExcelJsonConfigFile.Exists)
                    {
                        logger.Warn($"Cannot get config file from site:{tenantConfig.ConfigSite.url}, configFilePath: {configList.RootFolder.ServerRelativeUrl}/{JPMCSiteMetricsReportCache.JPMCExcelJsonConfigFileName}");
                        continue;
                    }

                    using Stream fileStream = jpmcExcelJsonConfigFile.OpenBinaryStream();
                    using StreamReader reader = new(fileStream);
                    var jpmcExcelJsonConfigString = reader.ReadToEnd();
                    if (jpmcExcelJsonConfigString != null)
                    {
                        try
                        {
                            mJPMCExcelConfig = JsonConvert.DeserializeObject<JPMCExcelJsonConfig>(jpmcExcelJsonConfigString);
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"Deserialize excel config from config file {jpmcExcelJsonConfigFile.Name} error: {e}");
                        }
                        if (mJPMCExcelConfig?.SheetConfigs?.Count != 5)
                        {
                            mJPMCExcelConfig = null;
                            logger.Warn($"the jpmc excel config file content is invalid");
                        }
                        else
                        {
                            logger.Info($"Load jpmc config success from site:{tenantConfig.ConfigSite.url}");
                            canConnectConfigFile = true;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"An error occured while getting config file from config site: {tenantConfig.ConfigSite.url}. Ex: {e}");
                }
            }

            if (!canConnectConfigSite)
            {
                logger.Warn($"Cannot connect to any jpmc config site. Will use default jpmc config");
                //throw new Exception("RM_SS_CannotConnectConfigSite");
            }
            else if (!canConnectConfigFile)
            {
                logger.Warn($"Cannot get jpmc config file from any jpmc config site. Will use default jpmc config");
            }

            if (mJPMCExcelConfig == null)
            {
                mJPMCExcelConfig = JsonConvert.DeserializeObject<JPMCExcelJsonConfig>(JPMCDefaultExcelConfigurationJson.Default_JSON_String);
                logger.Info($"Using default jpmc config");
            }
        }

        private async Task InitSiteInfoScheduleConfigAsync(ScheduleConfiguration config)
        {
            RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(config.SiteCollectionUrl);
            config.AveSiteId = remoteSiteCollection.id;
            var webapp = RABrowserClient.GetWebApplicationById(remoteSiteCollection.parentId);
            config.WebAppId = webapp.id;
            config.WebAppUrl = webapp.url;
            AveBPOSAccountInfo bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection);
            config.user = bposInfo;
            config.aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(config.SiteCollectionUrl, bposInfo, AveContextKind.ClientObjectModel);

            try
            {
                string siteID = config.aveObjectModelFactory.CreateSite(config.SiteCollectionUrl).ID.ToString();
                logger.Info("Get right Site id for site: {0}, ID: {1},ArchiverMessage SiteID:{2}.", config.SiteCollectionUrl, siteID, config.SiteCollectionID);
                config.SiteCollectionID = new Guid(siteID);
            }
            catch (AveSkipLockSiteException ex)
            {
                logger.Info("site locked error,Message:{0}.", ex.ToString());
            }
            catch (Exception ex)
            {
                logger.Info("Can not get right SiteID,Message:{0}.", ex.ToString());
            }
            try
            {
                config.siteUrlSchemeAndHost = new Uri(config.SiteCollectionUrl).Scheme + @"://" + new Uri(config.SiteCollectionUrl).Authority;
                logger.Info($"mConfiguration siteUrlSchemeAndHost:{config.siteUrlSchemeAndHost}.");
            }
            catch (Exception ex)
            {
                logger.Warn("Can not get Site Collection URL while Init Keep Data Arguments." + ex.ToString());
            }
        }

        public async Task RunAsync()
        {
            var jobStatus = JobStatus.Finished;
            var jobDetailsStatus = JobDetailsStatus.Successful;
            AvePerformanceMonitor.SetDisable(false);
            if (mJPMCExcelConfig == null)
            {
                logger.Error("Need a configuration json.");
                return;
            }
            try
            {
                using AvePerformanceScope pc = new("ExportSiteMetricsProcessor.Run");
                ReportManager.SetProgress(10);
                ThrowUtil.ThrowIfNullOrEmpty(jobContext.JobContextSetting, "job context info empty.");
                ThrowUtil.ThrowIfNullOrEmpty(jobContext.JobContextContent, "job setting info empty.");
                mSiteCollection = SerializerHelper.DeserializeByDataContractSerializer<RemoteSiteCollection>(jobContext.JobContextContent);
                mJobParameter = SerializerHelper.DeserializeByDataContractSerializer<SiteMetricsJobParameterDto>(jobContext.JobContextSetting);
                logger.Info($"Start process site: {mSiteCollection.url}");
                logger.Info($"Today time is: {mJobParameter.StartTime} - {mJobParameter.EndTime}");
                var mainJobId = JobId.Split("_")[0];
                var downCenterInfo = DownloadDataInfoDao.GetDownloadDataInfosByStatus([(int)DownloadContentJobStatus.Wait]).FirstOrDefault(item => item.JobId == mainJobId);
                if (downCenterInfo != null)
                {
                    downCenterInfo.JobStatus = (int)DownloadContentJobStatus.InProgress;
                    DownloadDataInfoDao.UpdateDownloadInfo(downCenterInfo);
                }
                var bposInfo = PoolUserUtil.GetBPOSInfoAsync(mSiteCollection).GetAwaiter().GetResult();
                var mFactory = MultiAppUtil.CreateAveObjectModelFactory(mSiteCollection.url, bposInfo, AveContextKind.ClientObjectModel);
                try
                {
                    mSPSite = mFactory.CreateSite(mSiteCollection.url);
                }
                catch (Exception e)
                {
                    logger.Error($"Cannot connect to site: {mSiteCollection.url}, error: {e}");
                    JobSummary = "RM_SYNC_InitException";
                    throw;
                }

                if (!string.IsNullOrEmpty(mJobParameter.WebUrl) && !string.IsNullOrEmpty(mJobParameter.LibraryRelativePath))
                {
                    mJobParameter.WebUrl = HttpUtility.UrlDecode(mJobParameter.WebUrl);
                    mJobParameter.LibraryRelativePath = HttpUtility.UrlDecode(mJobParameter.LibraryRelativePath);
                    logger.Info($"Export report file to WebUrl: {mJobParameter.WebUrl}, LibraryRelativePath: {mJobParameter.LibraryRelativePath}");
                    var desSite = RABrowserClient.GetRemoteSiteCollectionByListUrl(mJobParameter.WebUrl);
                    var desBposInfo = await PoolUserUtil.GetBPOSInfoAsync(desSite);
                    var desmFactory = MultiAppUtil.CreateAveObjectModelFactory(desSite.url, desBposInfo, AveContextKind.ClientObjectModel);
                    logger.Info($"Factory is null: {desmFactory == null}");
                    if (desmFactory == null)
                    {
                        logger.Error($"Cannot create factory for site: {desSite.url}");
                        JobSummary = "RM_JS_AM_AddDomain_UnableConnetDomain_Failed";
                        throw new Exception($"Cannot create factory for site: {desSite.url}");
                    }
                    var listFullPath = string.Empty;
                    using IAveSite mSite = desmFactory.CreateSite(desSite.url);
                    var (isValid, _) = SPOExportUtility.ValidateWebUrl(mSite, mJobParameter.WebUrl, mJobParameter.LibraryRelativePath, desBposInfo, desSite.id, true);
                    if (!isValid)
                    {
                        logger.Error($"Cannot find the library: {mJobParameter.LibraryRelativePath}");
                        JobSummary = "StorageOptimization13_SOARSORecordManagerLibraryNotExist";
                        throw new Exception($"Cannot find the library: {mJobParameter.LibraryRelativePath}");
                    }
                }

                var requiredSitePropertyName = mJPMCTenantConfigs
                    ?.FirstOrDefault(c => string.Equals(c.ConfigSiteUrl, mSiteCollection.url, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.M365TenantId, mSiteCollection.TenantId, StringComparison.OrdinalIgnoreCase))
                    ?.ProvisionedByGAOPropertyName ?? "";

                if (!string.IsNullOrEmpty(requiredSitePropertyName)
                    && (!mSPSite.RootWeb.AllProperties.ContainsKey(requiredSitePropertyName)
                        || !Boolean.TryParse(mSPSite.RootWeb.AllProperties[requiredSitePropertyName].ToString(), out var value)
                        || !value)
                    )
                {
                    logger.Warn($"Only export report for site has required propperty {requiredSitePropertyName} and this site is not. SiteUrl: {mSPSite.Url}");
                    jobDetailsStatus = JobDetailsStatus.Skipped;
                    return;
                }

                InitSupportLibraries();

                mExportProcessor = new ExcelExportProcessor4JPMC(mJPMCExcelConfig);
                await mExportProcessor.InitAsync(JobId);

                var jpmcConfig = mJPMCTenantConfigs?.FirstOrDefault(c => c.M365TenantId == mSiteCollection.TenantId);
                if (jpmcConfig == null)
                {
                    logger.Warn($"Skip not configrate jpmc site collection.");
                    jobDetailsStatus = JobDetailsStatus.Skipped;
                    return;
                }

                ScanJobSettings scanJobSettings = await PrepareScanSetting(jpmcConfig);

                try
                {
                    var activeScanner = SiteMetricsScannerSelector4JPMC.Create(scanJobSettings, jpmcConfig, mSiteCollection?.url);
                    await activeScanner.RunAsync();
                    mActiveReader = activeScanner.GetScanDataReader();
                    ReportManager.SetProgress(20);

                    mBackupReader = new BackupDataIndexReader4JPMC();
                    mBackupReader.Init(mSiteCollection);
                    ReportManager.SetProgress(30);

                    mDestroyProcessor = new DestroyProcessor4JPMC(mSiteCollection.ObjectId);

                    try
                    {
                        await ExportSiteStatus();
                        ReportManager.SetProgress(40);
                    }
                    catch (Exception e)
                    {
                        jobDetailsStatus = JobDetailsStatus.Exception;
                        jobStatus = JobStatus.FinishWithException;
                        logger.Error($"ExportSiteStatus error {e}");
                    }

                    try
                    {
                        await ExportLibrarys();
                        ReportManager.SetProgress(50);
                    }
                    catch (Exception e)
                    {
                        jobDetailsStatus = JobDetailsStatus.Exception;
                        jobStatus = JobStatus.FinishWithException;
                        logger.Error($"ExportSiteStatus error {e}");
                    }

                    try
                    {
                        ExportDER();
                        ReportManager.SetProgress(60);
                    }
                    catch (Exception e)
                    {
                        jobDetailsStatus = JobDetailsStatus.Exception;
                        jobStatus = JobStatus.FinishWithException;
                        logger.Error($"ExportSiteStatus error {e}");
                    }

                    try
                    {
                        ExportRCCs();
                        ReportManager.SetProgress(70);
                    }
                    catch (Exception e)
                    {
                        jobDetailsStatus = JobDetailsStatus.Exception;
                        jobStatus = JobStatus.FinishWithException;
                        logger.Error($"ExportSiteStatus error {e}");
                    }

                    try
                    {
                        ExportAllSites();
                        ReportManager.SetProgress(80);
                    }
                    catch (Exception e)
                    {
                        jobDetailsStatus = JobDetailsStatus.Exception;
                        jobStatus = JobStatus.FinishWithException;
                        logger.Error($"ExportSiteStatus error {e}");
                    }

                    await mExportProcessor.UploadBlobAsync(JobId);
                    ReportManager.SetProgress(90);

                    using (CheckJobStopScope jScope = new CheckJobStopScope()) { }
                }
                finally
                {
                    mActiveReader?.Dispose();
                    mDestroyProcessor?.Dispose();
                }
            }
            catch (JobStopException)
            {
                jobDetailsStatus = JobDetailsStatus.Pending;
                jobStatus = JobStatus.Stopped;
                throw;
            }
            catch (Exception e)
            {
                jobDetailsStatus = JobDetailsStatus.Failed;
                logger.Error($"Process site [{mSiteCollection.url}] error:{e}");
                jobStatus = JobStatus.Failed;
            }
            finally
            {
                ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails() { ObjectName = mSPSite?.RootWeb?.Title, Type = "RM_JS_Rule_ObjectLevel_SiteCollection", FullPath = mSiteCollection.url, Status = jobDetailsStatus, Comment = JobSummary });
                ReportManager.SetJobFinished(jobStatus, JobSummary);
                PerformanceMonitor.WritePerformanceResult();
                AvePerformanceMonitor.WritePerformanceResult();
            }
        }

        private void InitSupportLibraries()
        {
            var allLists = mSPSite.AllWebs.SelectMany(w => w.Lists);
            foreach (var list in allLists)
            {
                var listFullURL = list.FullUrl();
                if (list.Hidden)
                {
                    logger.Info("Skip the hidden list {0}", listFullURL);
                    continue;
                }
                logger.Info($"List title is: {list.RootFolder?.Name}, list base template is {(int)list.BaseTemplate}");
                if (CheckIsDesignList(list.RootFolder?.Name + ((int)list.BaseTemplate).ToString()))
                {
                    logger.Info("Skip the design list {0}", listFullURL);
                    continue;
                }
                if (NeedSkipGenericList(list))
                {
                    logger.Info("Skip general list. List url: {0} .", listFullURL);
                    continue;
                }

                if (list.BaseTemplate != AveListTemplateType.DocumentLibrary)
                {
                    logger.Info("Skip this list, only support document library. List url: {0} .", listFullURL);
                    continue;
                }
                mSupportLibraries.Add(list);
            }
        }

        private async Task ExportSiteStatus()
        {
            var scanGroupedResult = mActiveReader.GetArchiveApproveReportsGroupByColumns(JPMCFakeAndCombineRuleId);
            var totalActive = scanGroupedResult.Sum(a => a.TotalCount);
            
            var totalArchived = mBackupReader.GetTotalCount();

            var destroyFileCount = mDestroyProcessor.GetTotalCount();
            var destroyBackupCount = await ArchiveSiteInfoDao.GetDestructionFileNumberBySite(mSiteCollection.url);
            var totalDestroy = (destroyFileCount + destroyBackupCount);
            logger.Info($"Site collection {mSiteCollection.url} destory file count: {destroyFileCount}; destory backup count: {destroyBackupCount}");

            var properties = mSPSite.RootWeb.AllProperties;
            var columnDic = mJPMCExcelConfig?.SheetConfigs[ExcelExportProcessor4JPMC.ExcelSheetIndex_SiteStats]?.Columns ?? [];
            var definedPropsDic = new Dictionary<string, string>
            {
                { "Site ID", mSPSite.ID.ToString() },
                { "Site Name", mSPSite.RootWeb.Title },
                { "Site URL", mSiteCollection.url },
                { "Total Libraries", mSupportLibraries.Count.ToString() },
                { "Total Active Records", totalActive.ToString() },
                { "Total Archived Records", totalArchived.ToString() },
                { "Total Destroyed Records", totalDestroy.ToString() },
                { "Total Managed Records", (totalActive + totalArchived + totalDestroy).ToString() }
            };
            var exportData = mExportProcessor.ConvertData4SiteStats(properties, columnDic, definedPropsDic);
            mExportProcessor.Export(GetSheetName(ExcelExportProcessor4JPMC.ExcelSheetIndex_SiteStats), exportData);
        }

        private async Task ExportLibrarys()
        {
            var columnDic = mJPMCExcelConfig?.SheetConfigs[ExcelExportProcessor4JPMC.ExcelSheetIndex_Libraries]?.Columns ?? [];
            Dictionary<int, Dictionary<string, string>> definedPropsDicList = [];
            var rowIndex = 1;
            var properties = mSPSite.RootWeb.AllProperties;
            foreach (var list in mSupportLibraries)
            {
                var listFullURL = list.FullUrl();
                var scanGroupedResult = mActiveReader.GetArchiveApproveReportsGroupByColumns(JPMCFakeAndCombineRuleId, list.ID.ToString());
                var totalActive = scanGroupedResult.Sum(a => a.TotalCount);
                logger.Info($"Library scan report start {listFullURL}:");
                foreach (var item in scanGroupedResult)
                {
                    logger.Info($"Library Scan report, {item.RecordStatus}-{item.ClassCode}-{item.CountryCode}, Count:{item.TotalCount}");
                }

                var totalArchived = mBackupReader.GetTotalCountByList(listFullURL);

                var destroyFileCount = mDestroyProcessor.GetTotalCount(list.ID.ToString());
                var destroyBackupCount = await ArchiveSiteInfoDao.GetDestructionFileNumberBySite(mSiteCollection.url, listFullURL);
                var totalDestroy = destroyFileCount + destroyBackupCount;

                logger.Info($"Library {listFullURL} destory file count: {destroyFileCount}; destory backup count: {destroyBackupCount}");
                var definedPropsDic = new Dictionary<string, string>
                {
                    { "Site ID", list.ParentWeb.Site.ID.ToString() },
                    { "Library Name", list.Title },
                    { "Library URL", listFullURL },
                    { "Library Type", list.BaseTemplate.ToString() },
                    { "Total Active Records", totalActive.ToString() },
                    { "Total Archived Records", totalArchived.ToString() },
                    { "Total Destroyed Records", totalDestroy.ToString() },
                    { "Total Managed Records", (totalActive + totalArchived + totalDestroy).ToString() }
                };
                definedPropsDicList.Add(rowIndex, definedPropsDic);
                rowIndex++;
            }
            
            var exportData = mExportProcessor.ConvertData4Libraries(properties, columnDic, definedPropsDicList);
            mExportProcessor.Export(GetSheetName(ExcelExportProcessor4JPMC.ExcelSheetIndex_Libraries), exportData);
        }

        private void ExportRCCs()
        {
            List<RCCSheetDto> dataDto = [];
            var scanGroupedResultAllColumn = mActiveReader.GetArchiveApproveReportsGroupByColumns(JPMCFakeAndCombineRuleId);
            var scanGroupedResultMissColumn = mActiveReader.GetArchiveApproveReportsGroupByColumns(JPMCFakeOrCombineRuleId);
            var scanGroupedResult = scanGroupedResultAllColumn.Concat(scanGroupedResultMissColumn);
            logger.Info($"RCC site collection scan report start: {mSiteCollection.url}");
            var properties = mSPSite.RootWeb.AllProperties;
            string GetWebPropertyValue(string displayName)
            {
                var propertyName = GetTitleCoinfig(ExcelExportProcessor4JPMC.ExcelSheetIndex_RCCs, displayName).PropertyName;
                return properties.ContainsKey(propertyName) ? properties[propertyName]?.ToString() : "";
            }

            var columnDic = mJPMCExcelConfig?.SheetConfigs[ExcelExportProcessor4JPMC.ExcelSheetIndex_RCCs]?.Columns ?? [];
            Dictionary<int, Dictionary<string, string>> definedPropsDicList = [];
            var rowIndex = 1;
            foreach (var item in scanGroupedResult)
            {
                logger.Info($"RCC scan report, {item.RecordStatus}-{item.ClassCode}-{item.CountryCode}, Count:{item.TotalCount}");
                var definedPropsDic = new Dictionary<string, string>()
                {
                    { "Site ID", mSPSite.ID.ToString() },
                    { "Site URL", mSiteCollection.url },
                    { "RCC Country", item.CountryCode },
                    { "RCC Status", item.RecordStatus },
                    { "Record Class code(Term)", item.ClassCode.Split("|").FirstOrDefault() ?? "" },
                    { "Record Count", item.TotalCount.ToString() },
                };
                definedPropsDicList.Add(rowIndex, definedPropsDic);
                rowIndex++;
            }
            
            var exportData = mExportProcessor.ConvertData4RCCs(properties, columnDic, definedPropsDicList);
            mExportProcessor.Export(GetSheetName(ExcelExportProcessor4JPMC.ExcelSheetIndex_RCCs), exportData);
        }

        private void ExportDER()
        {
            var scanTotalResult = mActiveReader.GetArchiveApproveReportsTotalSize(JPMCFakeAndCombineRuleId).FirstOrDefault() ?? new ArchiveApproveReport4JPMTotalSize() { TotalCount = 0, TotalSize = 0 };

            var explorerDao = new ExplorerDao();

            Expression<Func<Record, bool>> basePredicate = record => record.IsManualSynced
                                            && (record.SourceFlag == (int)SourceFlag.SharePoint || record.SourceFlag == (int)SourceFlag.Teams)
                                            && record.ScopeId == new Guid(mSiteCollection.ObjectId)
                                            && record.ManualArchiveStatus != (int)ActionStatus.Archiverd
                                            && record.RecordStatus != (int)RMRecordStatus.Hidden && record.RecordStatus != (int)RMRecordStatus.RMDeleted
                                            && record.ManualExtendTime < DateTime.UtcNow.Ticks
                                            && record.NodeType == (int)RMNodeLevel.Item
                                            && record.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem";
            Expression<Func<Record, bool>> CombineToBaseExpressions(Expression<Func<Record, bool>> additionalPredicate)
            {
                var Combine = CombineExpressionsHelper.CombineExpressions(basePredicate, additionalPredicate, Expression.AndAlso);
                logger.Info($"Combine Linq is: {Combine}");
                return Combine;
            }


            Expression<Func<Record, bool>> todayApprovedPredicate = record => record.ManualApprovedStatus == (int)SOApproveDBStatus.Approved
                                        && record.ManualActionTime > mJobParameter.StartTime.GetValueOrDefault().Ticks
                                        && record.ManualActionTime < mJobParameter.EndTime.GetValueOrDefault().Ticks;
            var queryTodayApprovePredicate = CombineToBaseExpressions(todayApprovedPredicate);
            var todayApprovedCount = explorerDao.QueryCount(queryTodayApprovePredicate);
            var todayApproveTotalSize = QueryFileSize(explorerDao, queryTodayApprovePredicate);

            Expression<Func<Record, bool>> allWaitingPredicate = record => record.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove;
            var queryAllWaitingPredicate = CombineToBaseExpressions(allWaitingPredicate);
            var allWaitingCount = explorerDao.QueryCount(queryAllWaitingPredicate);
            var allWaitingTotalSize = QueryFileSize(explorerDao, queryAllWaitingPredicate);

            var startTime = mJobParameter.StartTime.GetValueOrDefault();
            var endTime = mJobParameter.EndTime.GetValueOrDefault();

            var range1End = startTime.AddDays(-365);

            var range2Start = startTime.AddDays(-365);
            var range2End = startTime.AddDays(-180);

            var range3Start = startTime.AddDays(-180);
            var range3End = startTime.AddDays(-90);

            var range4Start = startTime.AddDays(-90);
            var range4End = startTime.AddDays(-60);

            var range5Start = startTime.AddDays(-60);
            var range5End = endTime;

            logger.Info($"Query date range:" +
                $"\n\t({DateTime.MinValue},{range1End}), " +
                $"\n\t[{range2Start},{range2End}), " +
                $"\n\t[{range3Start},{range3End}), " +
                $"\n\t[{range4Start},{range4End}), " +
                $"\n\t[{range5Start},{range5End})");

            Expression<Func<Record, bool>> pendingApprovalThan365Predicate = record => record.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove
                                        && record.ManualCollectionTime < range1End.Ticks;

            Expression<Func<Record, bool>> pendingApproval180To365Predicate = record => record.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove
                                        && record.ManualCollectionTime >= range2Start.Ticks
                                        && record.ManualCollectionTime < range2End.Ticks;

            Expression<Func<Record, bool>> pendingApproval90To180Predicate = record => record.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove
                                        && record.ManualCollectionTime >= range3Start.Ticks
                                        && record.ManualCollectionTime < range3End.Ticks;

            Expression<Func<Record, bool>> pendingApproval60To90Predicate = record => record.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove
                                        && record.ManualCollectionTime >= range4Start.Ticks
                                        && record.ManualCollectionTime < range4End.Ticks;

            Expression<Func<Record, bool>> pendingApproval0To60Predicate = record => record.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove
                                        && record.ManualCollectionTime >= range5Start.Ticks
                                        && record.ManualCollectionTime < range5End.Ticks;

            var pendingApproval0To60Count = explorerDao.QueryCount(CombineToBaseExpressions(pendingApproval0To60Predicate));
            var pendingApproval60To90Count = explorerDao.QueryCount(CombineToBaseExpressions(pendingApproval60To90Predicate));
            var pendingApproval90To180Count = explorerDao.QueryCount(CombineToBaseExpressions(pendingApproval90To180Predicate));
            var pendingApproval180To365Count = explorerDao.QueryCount(CombineToBaseExpressions(pendingApproval180To365Predicate));
            var pendingApprovalThan365Count = explorerDao.QueryCount(CombineToBaseExpressions(pendingApprovalThan365Predicate));

            logger.Info($"Query date range result:" +
                $"\n\t({DateTime.MinValue.Ticks},{range1End.Ticks}) \t{pendingApprovalThan365Count}, " +
                $"\n\t[{range2Start.Ticks},{range2End.Ticks}) \t{pendingApproval180To365Count}, " +
                $"\n\t[{range3Start.Ticks},{range3End.Ticks}) \t{pendingApproval90To180Count}, " +
                $"\n\t[{range4Start.Ticks},{range4End.Ticks}) \t{pendingApproval60To90Count}, " +
                $"\n\t[{range5Start.Ticks},{range5End.Ticks}) \t{pendingApproval0To60Count}");

            var properties = mSPSite.RootWeb.AllProperties;
            string GetWebPropertyValue(string displayName)
            {
                var propertyName = GetTitleCoinfig(ExcelExportProcessor4JPMC.ExcelSheetIndex_DERs, displayName).PropertyName;
                return properties.ContainsKey(propertyName) ? properties[propertyName]?.ToString() : "";
            }
            static string ShowGB(long num) { return $"{num / (1024F * 1024F * 1024F):F2}"; }

            var columnDic = mJPMCExcelConfig?.SheetConfigs[ExcelExportProcessor4JPMC.ExcelSheetIndex_DERs]?.Columns ?? [];
            var definedPropsDic = new Dictionary<string, string>()
            {
                { "Site ID", mSPSite.ID.ToString() },
                { "Site URL", mSiteCollection.url },
                { "Total Active Records", scanTotalResult.TotalCount.ToString() },
                { "Total Record Volume(GB)", ShowGB(scanTotalResult.TotalSize) },
                { "Total Records Eligible Destruction Today(Count)", todayApprovedCount.ToString() },
                { "Total Records Eligible Destruction Today Volume(GB)", ShowGB(todayApproveTotalSize) },
                { "Total Records Eligible Disposed Till Date(Count)", allWaitingCount.ToString() },
                { "Total Records Eligible Disposed Till Date Volume(GB)", ShowGB(allWaitingTotalSize) },
                { "Record Pending Approval  (0-60 Days)", pendingApproval0To60Count.ToString() },
                { "Record Pending Approval  (60-90 Days)", pendingApproval60To90Count.ToString() },
                { "Record Pending Approval  (90 -180 Days)", pendingApproval90To180Count.ToString() },
                { "Record Pending Approval  (180-365 Days)", pendingApproval180To365Count.ToString() },
                { "Record Pending Approval  (>365 Days)", pendingApprovalThan365Count.ToString() },
            };
            var exportData = mExportProcessor.ConvertData4DERs(properties, columnDic, definedPropsDic);
            mExportProcessor.Export(GetSheetName(ExcelExportProcessor4JPMC.ExcelSheetIndex_DERs), exportData);
        }

        private void ExportAllSites() {

            var properties = mSPSite.RootWeb.AllProperties;
            var columnDic = mJPMCExcelConfig?.SheetConfigs[ExcelExportProcessor4JPMC.ExcelSheetIndex_AllSites]?.Columns ?? [];
            var definedPropsDic = new Dictionary<string, string>()
            {
                { "Site ID", mSPSite.ID.ToString() },
                { "Site URL", mSiteCollection.url },
                { "Site Name", mSPSite.RootWeb.Title },
                { "Site Description", mSPSite.RootWeb.Description },
            };
            var exportData = mExportProcessor.ConvertData4AllSites(properties, columnDic, definedPropsDic);
            mExportProcessor.Export(GetSheetName(ExcelExportProcessor4JPMC.ExcelSheetIndex_AllSites), exportData);
        }

        private JPMCExcelColumnConfig GetTitleCoinfig(int index, string configKey, bool useConfigKeyToDisplay = true)
        {
            return mExportProcessor.GetTitleCoinfig(index, configKey, useConfigKeyToDisplay);
        }

        private string GetSheetName(int index)
        {
            return mJPMCExcelConfig?.SheetConfigs[index]?.SheetName;
        }

        private async Task<ScanJobSettings> PrepareScanSetting(JPMCTenantConfig jpmcConfig)
        {
            ScheduleConfiguration mConfiguration = new ScheduleConfiguration(JobId)
            {
                ScanDBName = BuildDeterministicScanDbName(mSiteCollection?.id ?? string.Empty)
            };

            mConfiguration.ContainerId = new Guid(mSiteCollection.parentId);
            mConfiguration.SiteCollectionUrl = mSiteCollection.url;
            mConfiguration.SiteCollectionID = new Guid(mSiteCollection.id);
            mConfiguration.RunJobNodeLevel = (int)NodeLevel.SiteCollection;
            mConfiguration.JobReportDto = new JobReportImps(ReportManager);
            mConfiguration.ProgressDto = mConfiguration.JobReportDto;
            mConfiguration.ScopePath = mSiteCollection.url.Replace("/", "_");
            await InitSiteInfoScheduleConfigAsync(mConfiguration);
            var jobStartTime = jobContext?.JobStartTime ?? DateTime.UtcNow;
            if (jobStartTime.Kind == DateTimeKind.Local)
            {
                jobStartTime = jobStartTime.ToUniversalTime();
            }
            else if (jobStartTime.Kind == DateTimeKind.Unspecified)
            {
                jobStartTime = DateTime.SpecifyKind(jobStartTime, DateTimeKind.Utc);
            }
            mConfiguration.IncrementalDiscoverEndTimeTicks = jobStartTime.Ticks;

            var treeNode = RMDtoConverter.ConvertRemoteSite2RMTree(mSiteCollection);
            var group = RABrowserClient.GetWebApplicationById(mSiteCollection.parentId);
            treeNode.Parent = RMDtoConverter.ConvertRemoteWebApplication2RMTree(group);
            ScanJobSettings scanJobSettings = new ScanJobSettings()
            {
                SubJobId = JobId,
                Id = JobId,
                TreeNode = treeNode,
                Configuration = mConfiguration,
            };

            mConfiguration.RuleCollection = BuildDiscoverRule(jpmcConfig);
            return scanJobSettings;
        }

        private Dictionary<int, Rule> BuildDiscoverRule(JPMCTenantConfig jpmcConfig)
        {
            var filterArray = new List<FilterPolicy> {
                new FilterPolicy {
                    Condition = PolicyCondition.IsNotEmpty,
                    Level = PolicyLevel.Document,
                    Rule = new ColumnTextRule() { Value1 = $"[{jpmcConfig.CustomColumns.RecordStatus}]"},
                    RuleType = PolicyRuleType.Column,
                    SequenceNo = 1
                },
                new FilterPolicy {
                    Condition = PolicyCondition.IsNotEmpty,
                    Level = PolicyLevel.Document,
                    Rule = new ColumnTextRule() { Value1 = $"[{jpmcConfig.CustomColumns.CountryCode}]" },
                    RuleType = PolicyRuleType.Column,
                    SequenceNo = 2
                },
                new FilterPolicy {
                    Condition = PolicyCondition.IsNotEmpty,
                    Level = PolicyLevel.Document,
                    Rule = new ColumnTextRule() { Value1 = $"[{jpmcConfig.CustomColumns.ClassCode}]" },
                    RuleType = PolicyRuleType.Column,
                    SequenceNo = 3
                }
            };
            Dictionary<int, Rule> rules = new()
            {
                {
                    1,
                    new Rule
                    {
                        Id = JPMCFakeAndCombineRuleId,
                        Filters = filterArray,
                        PolicyLevel = PolicyLevel.Document,
                        AndOrExpression = new Dictionary<PolicyLevel, string>() { { PolicyLevel.Document, "(1 and 2 and 3)" } },
                        Order = 1,
                        ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule,
                        IncludeNew = "1"
                    }
                },
                {
                    2,
                    new Rule
                    {
                        Id = JPMCFakeOrCombineRuleId,
                        Filters = filterArray,
                        PolicyLevel = PolicyLevel.Document,
                        AndOrExpression = new Dictionary<PolicyLevel, string>() { { PolicyLevel.Document, "(1 or 2 or 3)" } },
                        Order = 1,
                        ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule,
                        IncludeNew = "1"
                    }
                },

            };
            foreach (var rule in rules)
            {
                rule.Value.SOFilters = new List<SOFilterPolicy>();
                foreach (var filter in rule.Value.Filters)
                {
                    var soFilter = new SOFilterPolicy
                    {
                        Condition = filter.Condition,
                        Level = filter.Level,
                        Rule = filter.Rule,
                        RuleType = filter.RuleType,
                        SequenceNo = filter.SequenceNo
                    };
                    rule.Value.SOFilters.Add(soFilter);
                }
            }

            return rules;
        }

        private bool CheckIsDesignList(string listInfo)
        {
            bool isDesignList = false;
            try
            {
                if (mDesignLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error has occurred when CheckIsDesignList, message:{e.Message}");
            }
            return isDesignList;
        }

        private bool NeedSkipGenericList(IAveList list)
        {
            return list.BaseType == AveBaseType.GenericList;
        }

        private long QueryFileSize(ExplorerDao explorerDao, Expression<Func<Record, bool>> queryPredicate)
        {
            var totalSize = 0L;
            bool hasNext = true;
            string continuation = string.Empty;
            var pateSize = 5000;
            while (hasNext)
            {
                Tuple<IEnumerable<Record>, string> result = explorerDao.QueryByPage(queryPredicate, pateSize, continuation);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                continuation = result.Item2;
                var tempList = result.Item1.ToList();
                foreach (var r in tempList)
                {
                    if (!string.IsNullOrEmpty(r.MetaInfo))
                    {
                        try
                        {
                            var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(r.MetaInfo);
                            totalSize += metaInfo.FileSize;
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"Deserialize MetaInfo error: {e}");
                        }
                    }
                }
            }
            return totalSize;
        }

        private static string BuildDeterministicScanDbName(string siteCollectionId)
        {
            const string format = "scan.{0}.db";
            if (Guid.TryParse(siteCollectionId, out Guid parsed) && parsed != Guid.Empty)
            {
                return string.Format(format, parsed.ToString());
    }

            return string.Format(format, Guid.NewGuid());
        }

    }

}
