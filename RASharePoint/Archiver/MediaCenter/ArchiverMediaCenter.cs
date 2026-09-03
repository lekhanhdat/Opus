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
using AvePoint.Archiver.Media;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Common;
using AvePoint.Media.Service.DomainModel;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class ArchiverMediaCenter
    {
        private static AveLogger mLog = new AveLogger(typeof(ArchiverMediaCenter));

        private string jobId;
        private JobState state=JobState.Finished;

        static ArchiverMediaCenter()
        {
            InitMedia();
        }

        private static void InitMedia()
        {
            try
            {
                MediaEnvironment.MediaServer = MediaServiceFactory.CreateMediaServer();

                MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo();

                MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo();

            }
            catch (Exception ex)
            {
                mLog.Error(string.Format("Can't initialize media information. Message:{0}", ex.ToString()));
                throw;
            }
        }

        public void HandleArchiverMediaMessage(ArchiverMessage message)
        {
            try
            {
                jobId = message.SubJobId;
                switch (message.Action)
                {
                    case ArchiverAction.ARCHIVER_MERGEINDEX_REQUEST:
                        ArchiverMergeIndexJobHandler mergeIndexHandler = new ArchiverMergeIndexJobHandler();
                        mergeIndexHandler.PerformMergeIndexJob(message);
                        break;
                    case ArchiverAction.ARCHIVER_RETENTION_METADATA:
                        ArchiverRetentionJobHandler retentionHandler = new ArchiverRetentionJobHandler();
                        retentionHandler.PerformRetentionJob(message);
                        break;
                    default:
                        throw new NotSupportedException("Invalid archiver media message.");
                }
            }
            catch (Exception ex)
            {
                mLog.Error(string.Format("Failed to merge current index. Message:{0}", ex.ToString()));
                state = JobState.Failed;
            }
            finally
            {
                UpdateJobStatus(jobId, state);
            }
        }

        public JobState HandleArchiverMediaMessageWithState(ArchiverMessage message)
        {
            state = JobState.Finished;
            try
            {
                jobId = message.SubJobId;
                switch (message.Action)
                {
                    case ArchiverAction.ARCHIVER_MERGEINDEX_REQUEST:
                        ArchiverMergeIndexJobHandler mergeIndexHandler = new ArchiverMergeIndexJobHandler();
                        mergeIndexHandler.PerformMergeIndexJob(message);
                        break;
                    case ArchiverAction.ARCHIVER_RETENTION_METADATA:
                        ArchiverRetentionJobHandler retentionHandler = new ArchiverRetentionJobHandler();
                        retentionHandler.PerformRetentionJob(message);
                        break;
                    default:
                        throw new NotSupportedException("Invalid archiver media message.");
                }
            }
            catch (Exception ex)
            {
                mLog.Error(string.Format("Failed to merge current index. Message:{0}", ex.ToString()));
                state = JobState.Failed;
            }
            finally
            {
                UpdateJobStatus(jobId, state);
            }
            return state;
        }

        public JobState HandleArchiverSubJobMergeIndexMessageWithState(ArchiverMessage message)
        {
            state = JobState.Finished;
            try
            {
                jobId = message.SubJobId;
                switch (message.Action)
                {
                    case ArchiverAction.ARCHIVER_MERGEINDEX_REQUEST:
                        ArchiverMergeIndexJobHandler mergeIndexHandler = new ArchiverMergeIndexJobHandler();
                        mergeIndexHandler.PerformMergeIndexSubJob(message);
                        break;
                    default:
                        throw new NotSupportedException("Invalid archiver media message.");
                }
            }
            catch (Exception ex)
            {
                mLog.Error(string.Format("Failed to merge current index. Message:{0}", ex.ToString()));
                state = JobState.Failed;
            }
            finally
            {
                UpdateJobStatus(jobId, state);
            }
            return state;
        }

        public JobState HandleArchiverSubJobMergeIndexMessageWithState(MergeIndexJobInfo job, string subJobId)
        {
            state = JobState.Finished;
            try
            {
                jobId = subJobId;
                ArchiverMergeIndexJobHandler mergeIndexHandler = new ArchiverMergeIndexJobHandler();
                mergeIndexHandler.PerformMergeIndexSubJob(job, jobId, subJobId);
            }
            catch (Exception ex)
            {
                mLog.Error(string.Format("Failed to merge current index. Message:{0}", ex.ToString()));
                state = JobState.Failed;
            }
            finally
            {
                UpdateJobStatus(jobId, state);
            }
            return state;
        }

        private void UpdateJobStatus(string jobId, JobState state)
        {
            JobStatusInfo jobInfo = new JobStatusInfo();
            jobInfo.IsSubJob = jobId.IndexOf('_') > 0 ? true : false;
            jobInfo.Id = jobId;
            jobInfo.State = (int)state;
            try
            {
                var jobStatusUpdater = new JobManagement();
                jobStatusUpdater.UpdateJobStatus(jobInfo);
            }
            catch (Exception ex)
            {
                mLog.Warn("failed to update job status,jobId:{0}.due to {1}", jobId, ex.ToString());
            }
        }
    }
}
