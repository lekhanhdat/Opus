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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Common.RemoteNode;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Export.Duplication;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.Wrapper.Common;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using M365GroupTeam;
using Microsoft.SharePoint.Client;
using RAExportCommon;
using RAGoogle.Restore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JobContext = AvePoint.RA.SharePoint.Common.JobContext;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class CleanUpDuplicateDatasProcessor
    {
        private static readonly AveLogger Logger = AveLogger.GetInstance(typeof(CleanUpDuplicateDatasProcessor));
        private string JobId = string.Empty;
        private JobType mJobType;
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        public IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private readonly IRMDiscoveryOffice365ExportJobService _exportJobService = PlatformWindsorManager.GetService<IRMDiscoveryOffice365ExportJobService>();
        private JobContext jobContext;
        public JobReportImps mJobreport;
        private IReportCenter _reporter { get; set; }
        private string ExportFilePath { get; set; }
        public CleanUpDuplicateDatasProcessor(string jobId, JobType jobType)
        {
            JobId = jobId;
            mJobType = jobType;
        }
        public async Task RunNowAsync()
        {
            ExportFilePath = await _exportJobService.DownloadDuplicationReportAsync();
            await EnumerateSiteCollectionUrlsAsync();
        }

        /// <summary>
        /// 遍历所有站点 URL 并写日志。
        /// </summary>
        public async Task EnumerateSiteCollectionUrlsAsync()
        {
            try
            {
                int _relatedSubJobIndex = 1;
                List<string> siteUrls = new List<string>();
                WrapperConfiguration.HasDeleteOnlyLicense = IsPrePaidConsumption() || IsEnableDeleteOnlyOptionSetting();
                _reporter = (new ReportCenter()).Build(mJobType, JobId);
                jobContext = JobContext.GetInstance(JobId, mJobType);
                jobContext.ReportManager.StartUpdateJobProgress();
                mJobreport = new JobReportImps(jobContext.ReportManager);
                RMDiscoveryOffice365IngestorDuplicationReportProcessor duplicateProcessor = new RMDiscoveryOffice365IngestorDuplicationReportProcessor(ExportFilePath);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                await foreach (var item in duplicateProcessor.DrainDuplicationReportAsync())
                {
                    if (!siteUrls.Contains(item.SiteUrl) && !string.IsNullOrEmpty(item.SiteUrl))
                    {
                        siteUrls.Add(item.SiteUrl);
                    }
                }
                sw.Stop();
                Logger.Info($"EnumerateSiteCollectionUrlsAsync get all sites url cost:{sw.ElapsedMilliseconds}");
                foreach (var siteUrl in siteUrls)
                {
                    using (new CheckJobStopScope()) { }
                    Logger.Info("[{0}] Enumerate site url: {1}", JobId, siteUrl);
                    var remoteSiteCollection = RemoteNodeDao.GetRemoteSiteCollectionByParam(new List<string> { siteUrl });
                    if (remoteSiteCollection == null || remoteSiteCollection.Count == 0)
                    {
                        Logger.Warn($"this sitecollection not exist in remote node,will skip cleanup it,url:{siteUrl}");
                        continue;
                    }
                    else
                    {
                        var tempRemoteSiteCollectiopn = remoteSiteCollection.FirstOrDefault();
                        var subJobId = CreateSubJob(
            _relatedSubJobIndex++,
            mJobType,
            "",
            siteUrl,
            tempRemoteSiteCollectiopn.TenantId);
                        JobContext.GetInstance(subJobId, mJobType);
                        Logger.Info($"will process this sitecollection ,cleanup it,url:{siteUrl}");
                        await new DisposalActivityManagementProcessor(subJobId, mJobType, tempRemoteSiteCollectiopn, await GenerateSitesItemsAsync(siteUrl)).RunNowAsync();
                    }

                }
            }
            catch (JobStopException ex)
            {
                Logger.Warn("job is stopped");
                _reporter.StopJob();
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"has error in EnumerateSiteCollectionUrlsAsync,error:{e}");
            }
            finally
            {
                _reporter.Finish();
            }
        }
        private async Task<List<CleanUpItemEntry>> GenerateSitesItemsAsync(string siteUrl)
        {
            List<CleanUpItemEntry> result = new List<CleanUpItemEntry>();
            ExportFilePath = await _exportJobService.DownloadDuplicationReportAsync();
            RMDiscoveryOffice365IngestorDuplicationReportProcessor duplicateProcessor = new RMDiscoveryOffice365IngestorDuplicationReportProcessor(ExportFilePath);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await foreach (var item in duplicateProcessor.DrainDuplicationReportAsync())
            {
                if (siteUrl == item.SiteUrl && (item.Action.ToString().Equals(ArchiveConstants.DestroyAction, StringComparison.OrdinalIgnoreCase) || item.Action.ToString().Equals(ArchiveConstants.ArchiveAction, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Add(new CleanUpItemEntry()
                    {
                        ItemId = item.ObjectId,
                        Action = item.Action.ToString()
                    });
                    Logger.Info($"will insert into CleanUpItemEntry ,itemId:{item.ObjectId},action:{item.Action.ToString()}.");
                }
                else
                {
                    Logger.Info($"will not insert into CleanUpItemEntry ,itemId:{item.ObjectId},action:{item.Action.ToString()}.");
                }
            }
            Logger.Info($"will GenerateSitesItems ,url:{siteUrl},count:{result.Count}");
            return result;
        }
        private bool IsEnableDeleteOnlyOptionSetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableDeleteOnlyOption");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        private bool IsPrePaidConsumption()
        {
            try
            {
                var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                var info = client.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult();
                if (info.Extension is Cloud.Sdk.Data.AosModern.CloudRecordsExtension)
                {
                    Cloud.Sdk.Data.AosModern.CloudRecordsExtension extension = info.Extension as Cloud.Sdk.Data.AosModern.CloudRecordsExtension;
                    if (extension.SaleType == Cloud.Sdk.Data.AosModern.SaleType.PrePaidConsumption)
                    {
                        //RMKeyValueDao.SaveAsync(new DB.Model.RMKeyValue() { Key= keyString ,Value="true"}).GetAwaiter().GetResult();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Logger.Error($"some thing went wrong when check Delete only action enabled,error{e.ToString()}");
                return false;
            }
        }
        private string CreateSubJob(int currentSubjobIndex, JobType jobType, object jobSettings, string scope,string _o365Id)
        {
            try
            {
                using (new CheckJobStopScope()) { }
                string subJobId = string.Format(JobId + "{0:D3}", currentSubjobIndex);
                var subJob = new RMSubJob()
                {
                    Id = subJobId,
                    ParentId = JobId.Substring(0, JobId.LastIndexOf("_")),
                    StartTime = DateTime.UtcNow.Ticks,
                    JobType = (int)jobType,
                    Progress = 0,
                    Status = (int)JobStatus.InProgress,
                    Weight = 0,
                    Runable = RecordsConstants.SubJob_Runnable_Exclude,
                    O365TenantId = _o365Id,
                };
                subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(jobSettings) };
                subJob.String1 = scope;
                SubJobDao.CreateJob(subJob);
                Logger.Info($"Create sub job {subJob.Id} sucessfull, type {subJob.JobType}, Scope {scope}");
                return subJobId;
            }
            catch (JobStopException stop)
            {
                Logger.Error(stop.ToString());
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while creating a sub job on {scope}. Reason: {ex}");
                throw;
            }
        }

        private static string GetCellValueSafe(IReadOnlyList<Cell> cells, int index, SharedStringTable sharedStringTable)
        {
            if (index < 0 || index >= cells.Count)
            {
                return string.Empty;
            }

            return GetCellValue(cells[index], sharedStringTable);
        }

        private static string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
        {
            if (cell == null)
            {
                return string.Empty;
            }

            string value = cell.InnerText;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                if (int.TryParse(value, out int sharedStringIndex) && sharedStringTable != null)
                {
                    return sharedStringTable.ElementAt(sharedStringIndex).InnerText;
                }
            }

            return value ?? string.Empty;
        }
    }

}
