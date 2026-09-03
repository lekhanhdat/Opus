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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Common;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using RAArchiverCommon;
using RecordsHotfixMaintenanceService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal class ArchiverMergeIndexJobHandler
    {
        IMergeIndexService MergeIndexService;
        IAJobStatusUpdater jobStatusUpdater = new JobManagement();
        AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        readonly static object syncRoot = new object();
        Dictionary<String, JobStatusInfo> cachedRunningJobStatusInfoDictionary = new Dictionary<String, JobStatusInfo>();
        long prevStamp = 0;


        public ArchiverMergeIndexJobHandler()
        {
            prevStamp = DateTime.UtcNow.Ticks;
            MergeIndexService = MediaServiceFactory.CreateArchiverMergeIndexService();
        }

        public void PerformMergeIndexJob(ArchiverMessage message)
        {
            logger.Info("Register for update progress events");
            JobStatusUpdater.JobStatusInfoUpdated += new EventHandler<JobStatusInfoEventArgs>(this.MediaJobStatusUpdater_JobStatusInfoUpdated);
            if (!message.MergeIndexJobInfos.ContainsKey(message.SubJobId))
            {
                throw new Exception(string.Format("Can't find specific sub job index information. SubJobId:{0}", message.SubJobId));
            }
            MergeIndexJobInfo job = message.MergeIndexJobInfos[message.SubJobId];
            job.FarmName = "";
            job.CacheLocation = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
            DiskInfoDto disk = new DiskInfoDto()
            {
                //Path = AveEnv.AgentCacheFolder,
                Path = BackgroundSettings.GetInstance().ArchiveCache,
                Type = DeviceType.LocalPath,
                Password = null,
                UserName = string.Empty,
                Usage = null
            };
            job.CacheLocation.Extension.Path.Add(new PathMap() { DiskInfo = disk });
            job.CacheLocation.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
            MergeIndexSubJobInfo subJobInfo = new MergeIndexSubJobInfo(job, ProductModule.ArchiverBackup);
            subJobInfo.JobDto.Id = message.SubJobId;
            MergeIndexService.Merge(new List<MergeIndexSubJobInfo>() { subJobInfo });
            JobStatusUpdater.JobStatusInfoUpdated -= new EventHandler<JobStatusInfoEventArgs>(this.MediaJobStatusUpdater_JobStatusInfoUpdated);
        }


        public void PerformMergeIndexSubJob(ArchiverMessage message)
        {
            if (!message.MergeIndexJobInfos.ContainsKey(message.SubJobId))
            {
                throw new Exception(string.Format("Can't find specific sub job index information. SubJobId:{0}", message.SubJobId));
            }
            MergeIndexJobInfo job = message.MergeIndexJobInfos[message.SubJobId];
            job.FarmName = "";
            job.CacheLocation = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
            DiskInfoDto disk = new DiskInfoDto()
            {
                //Path = AveEnv.AgentCacheFolder,
                Path = BackgroundSettings.GetInstance().ArchiveCache,
                Type = DeviceType.LocalPath,
                Password = null,
                UserName = string.Empty,
                Usage = null
            };
            job.CacheLocation.Extension.Path.Add(new PathMap() { DiskInfo = disk });
            job.CacheLocation.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
            MergeIndexSubJobInfo subJobInfo = new MergeIndexSubJobInfo(job, ProductModule.ArchiverBackup);
            subJobInfo.JobDto.Id = message.SubJobId;
            subJobInfo.IgnoreUpdateJobState = true;
            MergeIndexService.Merge(new List<MergeIndexSubJobInfo>() { subJobInfo });
        }

        public void PerformMergeIndexSubJob(MergeIndexJobInfo job,string jobId, string subJobId)
        {

            job.FarmName = "";
            job.CacheLocation = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
            DiskInfoDto disk = new DiskInfoDto()
            {
                //Path = AveEnv.AgentCacheFolder,
                Path = BackgroundSettings.GetInstance().ArchiveCache,
                Type = DeviceType.LocalPath,
                Password = null,
                UserName = string.Empty,
                Usage = null
            };
            job.CacheLocation.Extension.Path.Add(new PathMap() { DiskInfo = disk });
            job.CacheLocation.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
            MergeIndexSubJobInfo subJobInfo = new MergeIndexSubJobInfo(job, ProductModule.ArchiverBackup);
            subJobInfo.JobDto.Id = subJobId;
            //subJobInfo.IgnoreUpdateJobState = true;
            //need further optimization
            MergeIndexService.Merge(new List<MergeIndexSubJobInfo>() { subJobInfo });
        }

        private void MediaJobStatusUpdater_JobStatusInfoUpdated(object sender, JobStatusInfoEventArgs e)
        {
            lock (syncRoot)
            {
                if (e.JobStatus.Progress < 100 && !e.IsFinalStatusUpdateByMedia)
                {
                    try
                    {
                        JobStatusInfo prevJobStatus;
                        if (this.cachedRunningJobStatusInfoDictionary.TryGetValue(e.JobStatus.Id, out prevJobStatus))
                        {
                            if (e.JobStatus.Progress != prevJobStatus.Progress || (new TimeSpan(DateTime.UtcNow.Ticks - prevStamp).TotalMinutes > 5))
                            {
                                logger.Info($"UpdateJobProgress {e.JobStatus.Progress}");
                                e.JobStatus.Stamp = DateTime.UtcNow.Ticks;
                                prevStamp = DateTime.UtcNow.Ticks;
                                this.jobStatusUpdater.UpdateJobProgress(e.JobStatus);
    }
                        }
                        else
                        {
                            logger.Info($"UpdateJobProgress {e.JobStatus.Progress}");
                            e.JobStatus.Stamp = DateTime.UtcNow.Ticks;
                            prevStamp = DateTime.UtcNow.Ticks;
                            this.jobStatusUpdater.UpdateJobProgress(e.JobStatus);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Failed to update the progress of job {0}, details: {1}.", e.JobStatus.Id, ex.ToString());
                    }
                }
                else
                {
                    if (e.IsFinalStatusUpdateByMedia)
                    {
                        e.JobStatus.Progress = 100;
                        try
                        {
                            this.jobStatusUpdater.UpdateJobStatus(e.JobStatus);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Failed to update the progress of job {0}, details: {1}.", e.JobStatus.Id, ex.ToString());
                        }
                    }
                }
                this.cachedRunningJobStatusInfoDictionary[e.JobStatus.Id] = e.JobStatus;
                if (e.IsFinal)
                    this.cachedRunningJobStatusInfoDictionary.Remove(e.JobStatus.Id);
            }
        }
    }
}
