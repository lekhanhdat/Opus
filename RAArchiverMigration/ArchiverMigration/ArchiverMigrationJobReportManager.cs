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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;

namespace AvePoint.RA.ArchiverMigration
{
    public class ArchiverMigrationJobReportManager
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(ArchiverMigrationJobReportManager));
        private IJobDetailService jobDetailService = PlatformWindsorManager.GetService<IJobDetailService>();
        private List<JMJobDetails> cacheDetails = new List<JMJobDetails>();
        private int detailsBufferSize = 200;
        private BaseJobDto baseJobInfo;
        private bool hasFailedDetail = false;

        public bool HasFailedDetail { get { return hasFailedDetail; } }
        public int DetailsBufferSize { set { detailsBufferSize = value; } }

        public ArchiverMigrationJobReportManager(string jobId)
        {
            this.baseJobInfo = new BaseJobDto() { Id = jobId, JobType = (int)JobType.CloudArchiverMigration };
        }

        public void UploadReportFile()
        {
            lock (this.baseJobInfo)
            {
                if(cacheDetails.Count > 0)
                {
                    jobDetailService.SyncJobDetails(cacheDetails, baseJobInfo);
                    cacheDetails.Clear();
                }

                jobDetailService.UploadReportFile(baseJobInfo);
            }
        }

        public void UpdateJobDetails(IEnumerable<JMJobDetails> details)
        {
            if (!hasFailedDetail && details.Any(d => d.Status == JobDetailsStatus.Failed))
            {
                hasFailedDetail = true;
            }

            lock (this.baseJobInfo)
            {
                cacheDetails.AddRange(details);
                TryRealSyncJobDetails();
            }
        }
        public void UpdateJobDetails(JMJobDetails detail)
        {
            if(detail.Status == JobDetailsStatus.Failed)
            {
                hasFailedDetail = true;
            }

            lock (this.baseJobInfo)
            {
                cacheDetails.Add(detail);
                TryRealSyncJobDetails();
            }
        }
        private void TryRealSyncJobDetails()
        {
            try
            {
                List<JMJobDetails> remainedDetails = null;
                if (cacheDetails.Count >= detailsBufferSize)
                {
                    DatabaseUtility.BatchOperation(
                        cacheDetails,
                        (batchDetails) =>
                        {
                            if (batchDetails.Count() < detailsBufferSize)
                            {
                                remainedDetails = batchDetails.ToList();
                            }
                            else
                            {
                                jobDetailService.SyncJobDetails(batchDetails, baseJobInfo);
                            }
                        },
                        detailsBufferSize);

                    cacheDetails = remainedDetails ?? new List<JMJobDetails>();
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while update job detail, ERROR:{0}", ex.ToString());
                cacheDetails = new List<JMJobDetails>();
            }
        }

        public void AddJobDetail(JobDetailsStatus status, string objectName, string objectType, string? comment = null)
        {
            UpdateJobDetails(new JMArchiverMigrationJobDetails()
            {
                ObjectName = objectName,
                ObjectType = objectType,
                Status = status,
                Comment = comment
            });
        }
    }
}
