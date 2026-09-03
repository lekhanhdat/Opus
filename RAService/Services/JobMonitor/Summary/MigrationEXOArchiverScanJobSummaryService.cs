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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RAJobType = AvePoint.RA.Contract.JobMonitor.JobType;

namespace AvePoint.RA.Service.Services.JobMonitor.Summary
{
    public class MigrationEXOArchiverScanJobSummaryService : MigrationBaseJobSummaryService
    {
        public override string[] GetSummaryAttributes()
        {
            return new string[] {
                JOBINFORMATION,
                JOBID,
                STARTTIME,
                ENDTIME,
                JOBOPERATEDBY,

                STATISTICS,
                STATUS,
                TOTALSIZE,
                COMMENTS,
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
                    SummaryRow = GetStatisticsRows(job)
                };
                result.SummaryItem.Add(statisticsItem);
            }
            return result;
        }

        public List<RMJobSummaryRow> GetStatisticsRows(BaseJobDto jobDto)
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

            Dictionary<string, string> jobSummaryMap = AssembleSummaryMap(jobSummaries, inCorrectSubJobs);


            int exchangeMailBoxCount = 0;
            int failedExchangeMailBoxCount = 0;
            int skippedExchangeMailBoxCount = 0;

            int exchangeFolderCount = 0;
            int failedExchangeFolderCount = 0;
            int skippedExchangeFolderCount = 0;

            int exchangeItemCount = 0;
            int failedExchangeItemCount = 0;
            int skippedExchangeItemCount = 0;


            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ExChangeMailBoxCount)) { exchangeMailBoxCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ExChangeMailBoxCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ExChangeFailedMailBoxCount)) { failedExchangeMailBoxCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ExChangeFailedMailBoxCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ExChangeSkippedMailBoxCount)) { skippedExchangeMailBoxCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ExChangeSkippedMailBoxCount]); }

            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ExChangeFolderCount)) { exchangeFolderCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ExChangeFolderCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ExChangeFailedFolderCount)) { failedExchangeFolderCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ExChangeFailedFolderCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ExChangeSkippedFolderCount)) { skippedExchangeFolderCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ExChangeSkippedFolderCount]); }

            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ExChangeItemCount)) { exchangeItemCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ExChangeItemCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ExChangeFailedItemCount)) { failedExchangeItemCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ExChangeFailedItemCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ExChangeSkippedItemCount)) { skippedExchangeItemCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ExChangeSkippedItemCount]); }

            //summaryRows
            result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("ExchangeOnline.Service_e8770b93-04c3-48a7-8859-7fa00fe87b01", "Status"), Value = ConvertJobStatusToString((JobState)jobDto.State) });
            result.Add(GetComments(jobDto, entityTypes, jobSummaryMap));

            int succeedMailBoxCount = exchangeMailBoxCount - failedExchangeMailBoxCount - skippedExchangeMailBoxCount;
            int succeedFolderCount = exchangeFolderCount - failedExchangeFolderCount - skippedExchangeFolderCount;
            int succeedItemCount = exchangeItemCount - failedExchangeItemCount - skippedExchangeItemCount;

            int succeededNumber = succeedMailBoxCount + succeedFolderCount + succeedItemCount;

            string succeedi18n = SOI18NResource.Get("ExchangeOnline.Service_838ec9a7-d882-4764-b021-dc7ea913a406",
                                         "{0} (Mailbox: {1}; Folder: {2}; Item: {3})",
                                         succeededNumber, succeedMailBoxCount, succeedFolderCount, succeedItemCount);

            result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("ExchangeOnline.Service_bf5daf76-db44-457b-a9da-797626643794", "Number of Successful Objects"), Value = succeedi18n });

            int failedNumber = failedExchangeMailBoxCount + failedExchangeFolderCount + failedExchangeItemCount;
            string failedi18n = SOI18NResource.Get("ExchangeOnline.Service_838ec9a7-d882-4764-b021-dc7ea913a406",
                                         "{0} (Mailbox: {1}; Folder: {2}; Item: {3})",
                                         failedNumber, failedExchangeMailBoxCount, failedExchangeFolderCount, failedExchangeItemCount);

            result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("ExchangeOnline.Service_955461ba-5a47-4228-b04d-24df357eca4f", "Number of Failed Objects"), Value = failedi18n });

            int skippedNumber = skippedExchangeMailBoxCount + skippedExchangeFolderCount + skippedExchangeItemCount;

            string skippedi18n = SOI18NResource.Get("ExchangeOnline.Service_838ec9a7-d882-4764-b021-dc7ea913a406",
                                         "{0} (Mailbox: {1}; Folder: {2}; Item: {3})",
                                         skippedNumber, skippedExchangeMailBoxCount, skippedExchangeFolderCount, skippedExchangeItemCount);

            result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("ExchangeOnline.Service_04447fb5-0713-4b2c-a23e-5ee819d4d65b", "Number of Filtered Objects"), Value = skippedi18n });

            if (jobDto.State == (int)JobState.Failed || jobDto.State == (int)JobState.Skiped)
            {
                result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("ExchangeOnline.Service_68e495e9-3bad-4f5d-9094-6d4949a00379", "Total Data Size"), Value = "N/A" });
            }
            else
            {
                double dataSize = 0;
                if (jobSummaryMap.ContainsKey("DataSize"))
                {
                    dataSize = Convert.ToDouble(jobSummaryMap["DataSize"]);
                }
                result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("ExchangeOnline.Service_17884aae-2f3d-4417-8219-9068cfa5285e", "Total Data Size"), Value = JobDetailHelper.GetDataSizeToView(dataSize) });
            }

            return result;
        }
    }
}
