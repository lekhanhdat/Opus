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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Object;
using PnP.Core;
using RAGoogle.Restore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGoogle.Report.RestoreReport
{
    public class StatisticGDriveRestoreJobDetailsExecutor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(StatisticGDriveRestoreJobDetailsExecutor));
        private IJobMonitorDao _jMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IJobDetailService _jobDetailService => PlatformWindsorManager.GetService<IJobDetailService>();
        private IRMReportService _reportService => PlatformWindsorManager.GetService<IRMReportService>();

        private GDriveRestoreReportDetailWorker _gDriveDetailWorker = new GDriveRestoreReportDetailWorker();

        private HashSet<string> _gDriveInitStatusInCurrentRestoreJob;

        private HashSet<string> _driveIdSet = new HashSet<string>();


        private List<RMJobMonitor> statisticJobList = new List<RMJobMonitor>();

        private JobContext _jobContext;

        private DateTime _startUtcTime;

        private DateTime _endUtcTime;

        private string _restoreProfileId;

        public StatisticGDriveRestoreJobDetailsExecutor(DateTime startUtcTime, DateTime endUtcTime, string restoreProfileId, JobContext jobcontext)
        {
            this._restoreProfileId = restoreProfileId;
            this._startUtcTime = startUtcTime;
            this._endUtcTime = endUtcTime;
            this._jobContext = jobcontext;
        }
        public void StatictisRestoreJobDetails()
        {
            try
            {
                StatisticNewLogicRestoreJobs();
                UploadGDRptAndUpdateStatictisStatus();
            }
            catch (JobStopException)
            {
                _logger.Error($"job stop");
            }
            catch (Exception ex)
            {
                _logger.Error($"statistic or generate report failed ,error{ex}");
            }
        }
        private void StatisticNewLogicRestoreJobs()
        {
            _logger.Info($@"Start statistic new logic restore jobs");
            try
            {
                List<RMJobMonitor> unStatisticJobs = _jMDao.GetUnstatisticFinishRestoreGoogleJobsByTime(_startUtcTime.Ticks, _endUtcTime.Ticks);
                foreach (RMJobMonitor jobMonitor in unStatisticJobs)
                {
                    _gDriveInitStatusInCurrentRestoreJob = new HashSet<string>();
                    string jobRptPath = string.Empty;
                    try
                    {
                        var jobDto = new BaseJobDto()
                        {
                            Id = jobMonitor.Id,
                            JobType = jobMonitor.JobType,
                            AddValues = new Dictionary<string, object>(),
                            IsMergeRpt = true
                        };
                        jobRptPath = _jobDetailService.DownloadReports(jobDto);
                        DoNewLoginRestoreJobDetails(jobDto, jobMonitor);
                        jobMonitor.RestoreStatisticStatus = (int)MonitorStatisticStatus.Success;
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        jobMonitor.RestoreStatisticStatus = (int)MonitorStatisticStatus.PossbileFail;
                        _logger.Error(@$"Fail statistic jobDetail,jobId:{jobMonitor.Id},ex:{ex}");
                    }
                    finally
                    {
                        SafeDeleteFile(jobRptPath);
                        statisticJobList.Add(jobMonitor);
                    }
                }
                _logger.Info($@"Finish statistic new logic restore jobs");
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($@"Fail statistic new logic restore jobs,ex:{ex}");
            }
        }
        private void DoNewLoginRestoreJobDetails(BaseJobDto jobDto, RMJobMonitor jobMonitor)
        {
            _logger.Info($@"Start insert job detail, jobId:{jobMonitor.Id}");
            int pageIndex = 1;
            int pageSize = 1000;
            int totalCount = 0;
            IEnumerable<JMRestoreGDriveDetails> restoreDetails = new List<JMRestoreGDriveDetails>();
            do
            {
                IEnumerable<JMGDriveRestoreActionJobDetail> jMJobDetails = _jobDetailService.GetData(pageSize, pageIndex, ref totalCount, null, jobDto)?.Cast<JMGDriveRestoreActionJobDetail>();
                if (jMJobDetails == null)
                {
                    _logger.Error($@"Unable read job detail from rpt, jobId:{jobMonitor.Id}");
                    throw new Exception(@$"Unable read job details from rpt, jobId:{jobMonitor.Id}");
                }
                restoreDetails = jMJobDetails.Select(detail => ConvertJobDetailsToGDriveDetails(detail, jobMonitor));
                if (restoreDetails != null && restoreDetails.Count() > 0)
                {
                    InsertData(restoreDetails);
                }
            } while (totalCount > pageIndex++ * pageSize);
            _logger.Info(@$"Finish insert job detail, jobId:{jobMonitor.Id}");
        }
        private void UploadGDRptAndUpdateStatictisStatus()
        {
            try
            {
                foreach (string driveId in _driveIdSet)
                {
                    try
                    {
                        _gDriveDetailWorker.UploadReports(driveId);
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(@$"Fail upload sc rpt , drive name:{driveId} , ex:{ex}");
                    }
                }
                _jMDao.BatchUpdate(statisticJobList);
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($@"Fail update restore job statictic status, ex:{ex}");
            }
        }
        private void InsertData(IEnumerable<JMRestoreGDriveDetails> detailsList)
        {
            var detailsGroups = GroupRestoreGDriveDetails(detailsList);
            foreach (var group in detailsGroups)
            {
                try
                {
                    using (new CheckJobStopScope()) { }

                    string gDriveRptPath = _gDriveDetailWorker.DownloadReports(group.Key);
                    if (gDriveRptPath != null)
                    {
                        if (!_gDriveInitStatusInCurrentRestoreJob.Contains(group.Key))
                        {
                            _gDriveInitStatusInCurrentRestoreJob.Add(group.Key);
                            _gDriveDetailWorker.DeleteData(@$" JobId = '{group.Value.First().JobId}'", group.Key);
                        }
                    }
                    IList<JMRestoreGDriveDetails> detailList = new List<JMRestoreGDriveDetails>();
                    foreach (JMRestoreGDriveDetails details in group.Value)
                    {
                        detailList.Add(details);
                        if (detailList.Count > 1000)
                        {
                            _gDriveDetailWorker.InsertData(detailList, group.Key);
                            detailList.Clear();
                        }
                    }
                    if (detailList.Count > 0)
                    {
                        _gDriveDetailWorker.InsertData(detailList, group.Key);
                        detailList.Clear();
                    }
                    _driveIdSet.Add(group.Key);
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Error(@$"Fail inset data into sc rpt:{group.Key},ex:{ex}");
                    throw;
                }
            }
        }
        private Dictionary<string, List<JMRestoreGDriveDetails>> GroupRestoreGDriveDetails(IEnumerable<JMRestoreGDriveDetails> detailsList)
        {
            IOrderedEnumerable<string> driveIds = detailsList.Where(detail => detail.Level == "RM_JS_Common_ReportType_GoogleDrive")
                .Select(detail => detail.DriveId).Distinct().OrderDescending();
            Dictionary<string, List<JMRestoreGDriveDetails>> res = new Dictionary<string, List<JMRestoreGDriveDetails>>();
            foreach (var detail in detailsList)
            {
                string driveId = driveIds.FirstOrDefault(id => id.Equals(detail.DriveId));
                List<JMRestoreGDriveDetails> detailList = res.GetValueOrDefault(driveId, new List<JMRestoreGDriveDetails>());
                detailList.Add(detail);
                res[driveId] = detailList;
            }
            return res;
        }
        private JMRestoreGDriveDetails ConvertJobDetailsToGDriveDetails(JMGDriveRestoreActionJobDetail jobDetail, RMJobMonitor jobMonitor)
        {
            long size = 0;
            long.TryParse(jobDetail.Size, out size);
            JMRestoreGDriveDetails sCRestoreDetails = new JMRestoreGDriveDetails()
            {
                DriveId = jobDetail.DriveId,
                Status = jobDetail.Status,
                Comment = jobDetail.Comment,
                Level = jobDetail.Level,
                Name = jobDetail.SourceLocation,
                SourceURL = jobDetail.Path,
                Size = size,
                RestoreBy = jobMonitor.UserName,
                JobId = jobMonitor.Id,
                StartTime = jobMonitor.StartTime,
                FinishTime = jobDetail.FinishTime,
                IsDaoMigration = jobMonitor.DAOMigrated == true ? 1 : 0,
                IsEndUserOpt = jobMonitor.AdditionalInformation != null ? 1 : 0,
                RestoreTo = "RM_JS_JM_JobType_GoogleArchiverRestore"
            };
            return sCRestoreDetails;
        }
        private void SafeDeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("delete file faile." + ex);
            }
        }
    }
}
