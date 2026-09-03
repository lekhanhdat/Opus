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
using Storage;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Media.Common.ClassicStorageApi;

namespace AvePoint.RA.SharePoint.ExportJob
{
    public class JobExportToLocationDisposalProcessor
    {
        protected static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(JobExportToLocationDisposalProcessor));
        private IJobMonitorService mJobService;

        private IRMJobExportSettingDao mJESDao;
        protected IJobMonitorService RMJobService
        {
            get
            {
                if (mJobService == null)
                {
                    mJobService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
                }
                return mJobService;
            }
        }

        protected IRMJobExportSettingDao JESDao
        {
            get
            {
                if (mJESDao == null)
                {
                    mJESDao = (IRMJobExportSettingDao)PlatformWindsorManager.GetService(typeof(IRMJobExportSettingDao));
                }
                return mJESDao;
            }
        }
       
        public IJobMonitorDetailDownloadWorker JobMonitorDetailDownloadWorker { get; set; }

        public async Task RunJobAsync(string jobId, string baseJobId, string jobRunByUser)
        {
            RMJobService.UpdateJobProgress(jobId, 15);
            try
            {
                var job = await RMJobService.GetJobAsync(baseJobId.Contains("_")? baseJobId.Substring(0, baseJobId.IndexOf("_")) : baseJobId);
                var baseJobDto = new BaseJobDto();
                baseJobDto.Id = baseJobId;
                baseJobDto.JobType = job.JobTypeCode;
                baseJobDto.SubJobCount = job.SubJobCount;
                baseJobDto.JobVersion = job.JobVersion;
                if (baseJobDto.JobType == (int)JobType.EXOEnforceRetention || baseJobDto.JobType == (int)JobType.OneDriveEnforceRetention)
                {
                    baseJobDto.JobType = (int)JobType.EnforceRetention;
                }
                JobMonitorDetailDownloadWorker = (IJobMonitorDetailDownloadWorker)PlatformWindsorManager.GetService(typeof(IJobMonitorDetailDownloadWorker));
                string baseFolder = JobReportUtility.GetDownloadJobMonitorDetailTempleFolder(Guid.NewGuid().ToString());
                logger.Info("base folder: {0}", baseFolder);
                await JobMonitorDetailDownloadWorker.GenerateSingleAsync(baseFolder, baseJobDto, true);
                var zipPath = baseFolder + JobMonitorConstants.ZIP;
                ZipUtil.ZipFolder(baseFolder, zipPath, Encoding.UTF8);
                RMJobService.UpdateJobProgress(jobId, 75);
                //ZipUtil.ZipFolder(baseFolder, JobReportUtility.GetDownloadJobMonitorDetailTempleFolder(string.Empty) + "\\" + baseJobId + DateTime.Now.ToString("_yyyy_MM_dd_hh_mm_ss") + JobMonitorConstants.ZIP, Encoding.UTF8);
                //Directory.Delete(baseFolder, true);
                var setting = JESDao.GetExportSetting();
                if (setting.ExportSetting == 0)
                {
                    throw new Exception(I18NEntity.GetString("RM_EL_JM_DownloadSettingError"));
                }
                DAOAPIClientV1 Client1 = new DAOAPIClientV1();
                var containerInfo = Client1.GetExportLocationNameAndConntionbyId(setting.ExportLocationId.ToString());
                if (string.IsNullOrEmpty(containerInfo.connectionString))
                {
                    logger.Warn("export location not found, location name:{0}, Id:{1}", setting.LocationName, setting.ExportLocationId);
                    throw new Exception(string.Format(I18NEntity.GetString("RM_EL_NoExportLocation"), setting.LocationName));
                }

                //logger.Info("connString is {0}", connString);
                using (FileStream fs = File.OpenRead(zipPath))
                {
                    using (IXSystem system = XFactoryCommon.InstanceSystem(containerInfo.connectionString))
                    {
                        system.Open();
                        var result = ValidateStorage(system, containerInfo.name);
                        if (!string.IsNullOrEmpty(result))
                        {
                            throw new Exception(result);
                        }
                        system.CommitStream(fs, new StorageInfo() { HighName = "JobExport", LowName = "Job_Report_" + baseJobId + ".zip", Length = fs.Length });
                    }
                }
                RMJobService.UpdateJobStatus(jobId, JobStatus.Finished);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                if(ex.Message.Contains("Name does not resolve", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = "RM_EL_JM_NameNotResolve";
                }
                logger.Error("job failed, error:{0}", ex.ToString());
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, errorMessage);
            }
        }

        private string ValidateStorage(IXSystem system, string containerName)
        {
            var result = system.Validate();
            if (result.SystemHealth == XSystemHealth.AvailableAndNotFull
             || result.SystemHealth == XSystemHealth.Available)
            {
                if (result.TotalFreeSpace > 1024 * 1024 * 1024)  //>1g
                {
                    logger.Info($"Validate {containerName} successfully.");
                    return string.Empty;
                }
                else
                {
                    logger.Info($"Validate {containerName} successfully,but the total free space is not enough 1gb");
                    return "RM_AR_Storage_SpaceNotEnough_ErrorMessage";
                }
            }
            else
            {
                logger.Warn($"Validate {containerName} failed, system health: {result.SystemHealth}");
                return "RM_AR_Storage_Account_ErrorMessage";
            }

            return string.Empty;
        }
    }
}
