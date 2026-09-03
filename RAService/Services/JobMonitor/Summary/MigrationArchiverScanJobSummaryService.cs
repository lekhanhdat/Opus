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
using AvePoint.Common;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core.DaoMigration;
using AvePoint.RA.Service.Services.JobMonitor.Detail;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RAJobType = AvePoint.RA.Contract.JobMonitor.JobType;

namespace AvePoint.RA.Service.Services.JobMonitor.Summary
{
    public class MigrationArchiverScanJobSummaryService: MigrationBaseJobSummaryService
    {
        public override string[] GetSummaryAttributes()
        {
            return new string[] {
                JOBINFORMATION,
                JOBID,
                SCOPE,
                STARTTIME,
                ENDTIME,
                JOBOPERATEDBY,

                STATISTICS,
                STATUS,
                COMMENTS,
                NUMBEROFSUCCEEDEDOBJECTS,
                NUMBEROFFAILEDOBJECTS,
                NUMBEROFSKIPPEDOBJECTS,
                TOTALSIZE
            };

        }

        public override RMJobSummaryInfos GetJobSummaryInfo(BaseJobDto job, GeneralSettingModel gsm)
        {
            RMJobSummaryInfos result = new()
            {
                SummaryItem = new List<RMJobSummaryItem>(),
                JobId = job.Id,
                JobType = (RAJobType)job.Type
            };
            if (GetSummaryAttributes().Contains(JOBINFORMATION))
            {
                RMJobSummaryItem jobInfoItem = new()
                {
                    Title = SOI18NResource.Get("StorageOptimization.Service_Job Information", "Job Information"),
                    SummaryRow = GetJobInfoRows(job, gsm)
                };
                result.SummaryItem.Add(jobInfoItem);
            }
            if (GetSummaryAttributes().Contains(STATISTICS))
            {
                RMJobSummaryItem statisticsItem = new()
                {
                    Title = SOI18NResource.Get("StorageOptimization.Service_4f0bd5df-04eb-432b-9c0f-a908f0d005da", "Statistics"),
                    SummaryRow = GetStaticticsRows(job)
                };
                result.SummaryItem.Add(statisticsItem);
            }
            return result;
        }
        private List<RMJobSummaryRow> GetStaticticsRows(BaseJobDto jobDto)
        {
            JobReportDetailEntityType[] entityTypes = new JobReportDetailEntityType[] { JobReportDetailEntityType.NormalInfo };
            AbstractDaoMigrationJobDetailWorker worker = GetDetailWorker(jobDto);
            if (worker != null)
            {
                Contract.JobMonitor.BaseJobDto raJobDto = GetRABaseJobDto(jobDto);
                List<JobSummary> jobSummaries = worker.GetJobSummary(raJobDto, entityTypes);
                return GetStatisticsRows(jobDto, entityTypes, jobSummaries);
            }
            return null;
        }

        protected List<RMJobSummaryRow> GetStatisticsRows(BaseJobDto jobDto, JobReportDetailEntityType[] entityTypes, List<JobSummary> jobSummaries, List<SubJobDto> inCorrectSubJobs = null)
        {
            List<RMJobSummaryRow> result = new List<RMJobSummaryRow>();

            Dictionary<string, string> jobSummaryMap = AssembleSummaryMap(jobSummaries);

            int siteCollectionCount = 0;
            int failedSiteCollectionCount = 0;
            int skippedSiteCollectionCount = 0;
            int filteredSiteCollectionCount = 0;

            int siteCount = 0;
            int failedSiteCount = 0;
            int skippedSiteCount = 0;
            int filteredSiteCount = 0;

            int listCount = 0;
            int failedListCount = 0;
            int skippedListCount = 0;
            int filteredListCount = 0;

            int folderCount = 0;
            int failedFolderCount = 0;
            int skippedFolderCount = 0;
            int filteredFolderCount = 0;

            int itemCount = 0;
            int failedItemCount = 0;
            int skippedItemCount = 0;
            int filteredItemCount = 0;

            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.SiteCollectionCount)) { siteCollectionCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.SiteCollectionCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.FailedSiteCollectionCount)) { failedSiteCollectionCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.FailedSiteCollectionCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.SkippedSiteCollectionCount)) { skippedSiteCollectionCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.SkippedSiteCollectionCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.FilteredSiteCollectionCount)) { filteredSiteCollectionCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.FilteredSiteCollectionCount]); }

            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.SiteCount)) { siteCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.SiteCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.FailedSiteCount)) { failedSiteCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.FailedSiteCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.SkippedSiteCount)) { skippedSiteCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.SkippedSiteCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.FilteredSiteCount)) { filteredSiteCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.FilteredSiteCount]); }

            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ListCount)) { listCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ListCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.FailedListCount)) { failedListCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.FailedListCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.SkippedListCount)) { skippedListCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.SkippedListCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.FilteredListCount)) { filteredListCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.FilteredListCount]); }

            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ArchiveFolderCount)) { folderCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ArchiveFolderCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ArchiveFailedFolderCount)) { failedFolderCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ArchiveFailedFolderCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ArchiveSkippedFolderCount)) { skippedFolderCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ArchiveSkippedFolderCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ArchiveFilteredFolderCount)) { filteredFolderCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ArchiveFilteredFolderCount]); }

            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ItemCount)) { itemCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ItemCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.FailedItemCount)) { failedItemCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.FailedItemCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.SkippedItemCount)) { skippedItemCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.SkippedItemCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.FilteredItemCount)) { filteredItemCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.FilteredItemCount]); }
            if (GetSummaryAttributes().Contains(STATUS))
            {
                result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_955d4d30-3430-462f-a541-aa5b253e1aee", "Status"), Value = ConvertJobStatusToString((JobState)jobDto.State) });
            }

            if (GetSummaryAttributes().Contains(COMMENTS))
            {
                result.Add(GetComments(jobDto, entityTypes, jobSummaryMap));
            }

            if (GetSummaryAttributes().Contains(NUMBEROFSUCCEEDEDOBJECTS))
            {
                int succeedSiteCollectionCount = siteCollectionCount - failedSiteCollectionCount - skippedSiteCollectionCount - filteredSiteCollectionCount;
                int succeedSiteCount = siteCount - failedSiteCount - skippedSiteCount - filteredSiteCount;
                int succeedListCount = listCount - failedListCount - skippedListCount - filteredListCount;
                int succeedFolderCount = folderCount - failedFolderCount - skippedFolderCount - filteredFolderCount;
                int succeedItemCount = itemCount - failedItemCount - skippedItemCount - filteredItemCount;

                int succeededNumber = succeedSiteCollectionCount + succeedSiteCount + succeedListCount + succeedFolderCount + succeedItemCount;

                string i18n = SOI18NResource.Get("StorageOptimization.Service_afb8c685-cfb0-4ce9-a34c-38cdc0df0d6b",
                                            "{0}(Site Collection: {1}; Site: {2}; List: {3}; Folder: {4}; Item: {5})",
                                            succeededNumber, succeedSiteCollectionCount, succeedSiteCount,
                                            succeedListCount, succeedFolderCount, succeedItemCount);

                result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_55485CCE-D8D7-48DA-8BD8-EA91B83D08D2", "The Number of Successful Objects"), Value = i18n });
            }

            if (GetSummaryAttributes().Contains(NUMBEROFFAILEDOBJECTS))
            {
                int failedNumber = failedSiteCollectionCount + failedSiteCount + failedListCount + failedFolderCount + failedItemCount;
                string i18n = SOI18NResource.Get("StorageOptimization.Service_afb8c685-cfb0-4ce9-a34c-38cdc0df0d6b",
                                             "{0}(Site Collection: {1}; Site: {2}; List: {3}; Folder: {4}; Item: {5})",
                                             failedNumber, failedSiteCollectionCount, failedSiteCount,
                                             failedListCount, failedFolderCount, failedItemCount);

                result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_7D0D2F28-52C0-40CF-BB8F-8B9E7CF991D0", "The Number of Failed Objects"), Value = i18n });
            }

            if (GetSummaryAttributes().Contains(NUMBEROFSKIPPEDOBJECTS))
            {
                int skippedNumber = skippedSiteCollectionCount + skippedSiteCount + skippedListCount + skippedFolderCount + skippedItemCount;
                string i18n = SOI18NResource.Get("StorageOptimization.Service_06602233-9BB2-4304-B7B0-118BB9920F00",
                             "{0}(Site Collection: {1}; Site: {2}; List: {3}; Folder: {4}; Item: {5})",
                             skippedNumber, skippedSiteCollectionCount, skippedSiteCount,
                             skippedListCount, skippedFolderCount, skippedItemCount);
                result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_875E7F3A-851A-4F86-ACB0-44E47EA57511", "Number of Skipped Objects"), Value = i18n });
            }

            if (GetSummaryAttributes().Contains(TOTALSIZE))
            {
                if (jobDto.State == (int)JobState.Failed || jobDto.State == (int)JobState.Skiped)
                {
                    result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_41DDD334-93A3-44BC-A4A0-D9508F1A7BFC", "Total Size"), Value = "N/A" });
                }
                else
                {
                    double dataSize = 0;
                    if (jobSummaryMap.ContainsKey(SOConstants.DataSize))
                    {
                        dataSize = Convert.ToDouble(jobSummaryMap[SOConstants.DataSize], CultureInfo.CurrentCulture);
                    }
                    result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_88F30A4C-940C-46DB-8A67-EF6BA2EC143A", "Total Size"), Value = JobDetailHelper.GetDataSizeToView(dataSize) });
                }
            }
            return result;
        }

    }
}
