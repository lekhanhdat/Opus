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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.Discover.Base;
using RAArchiverCommon.DestructionCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.Wrapper.Common;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.Hybrid.ClientLibrary.SDK.Services;
using BaseJobDto = AvePoint.RA.Contract.JobMonitor.BaseJobDto;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.Tenant;
using AvePoint.GCommon.Utility;
using DocumentFormat.OpenXml.Office2010.Excel;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.Object;
using AvePoint.Item.Restore;
using PnP.Framework.Extensions;
using AvePoint.RA.Contract.RMWeb;
using System.IO;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.SharePoint.RestoreReport.Worker;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Object;
using Aspose.Pdf.Operators;
using AvePoint.RA.SharePoint.Common;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.SharePoint.ArchiverCommon;
using System.Text.RegularExpressions;
using AvePoint.RA.Contract.Explorer;

namespace AvePoint.RA.SharePoint.RestoreReport.Statistic
{
    public class StatisticRestoreJobDetailsExecutor
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(StatisticRestoreJobDetailsExecutor));
        private string _restoreProfileId;
        private bool haveError = false;
        private IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private ITenantInfoDao TenantInfoDao => PlatformWindsorManager.GetService<ITenantInfoDao>();

        private HashSet<string> scInitStatusInCurrentRestoreJob;

        private IJobDetailService JobDetailService
        {
            get
            {
                if (mJobDetailService == null)
                {
                    mJobDetailService = (IJobDetailService)PlatformWindsorManager.GetService(typeof(IJobDetailService));
                }
                return mJobDetailService;
            }
        }
        private IRMReportService mReportService;
        protected IRMReportService ReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
            }
        }
        private IJobDetailService mJobDetailService;

        private int jobProcessTotal = 10000000 * 100;
        private int phaseIncreaseProcessCount = 10000000 * 25;

        private DateTime startUtcTime;
        private DateTime endUtcTime;
        private RestoreReportScDetailWorker scDetailWorker = new RestoreReportScDetailWorker();

        private HashSet<string> scUrlSet = new HashSet<string>();

        private List<RMJobMonitor> statisticJobList = new List<RMJobMonitor>();
        private JobContext jobContext;
        public JobReportImps mJobreport;
        public SourceFlag source = SourceFlag.None;

        public StatisticRestoreJobDetailsExecutor(DateTime startUtcTime, DateTime endUtcTime,string restoreProfileId, JobContext jobcontext, SourceFlag source = SourceFlag.None)
        {
            this._restoreProfileId = restoreProfileId;
            this.startUtcTime = startUtcTime;
            this.endUtcTime = endUtcTime;
            this.jobContext = jobcontext;
            this.source = source;
            this.jobContext.ReportManager.IncreaseBase(jobProcessTotal);
            this.jobContext.ReportManager.StartUpdateJobProgress();
        }

        public void StatictisRestoreJobDetails()
        {
            try
            {
                StatisticMigrationRestoreJobs();
                StatisticNewLogicRestoreJobs();
                UploadScRptAndUpdateStatictisStatus();
                GenerateRestoreReport();
                if (haveError)
                {
                    FinishJob(JobStatus.FinishWithException);
                }
                else
                {
                    FinishJob(JobStatus.Finished);
                }
            }
            catch (JobStopException)
            {
                FinishJob(JobStatus.Stopped);
            }
            catch (Exception ex) 
            {
                mLog.Error($"statistic or generate report failed ,error{ex}");
                FinishJob(JobStatus.Failed);
            }
        }
        private void GenerateRestoreReport()
        {
            int increasedJobProcessCount = 0;
            string currentUrl = string.Empty;
            try
            {
                mJobreport = new JobReportImps(jobContext.ReportManager);
                RMProfileDto profile = ReportService.GetProfileByIdAsync(this._restoreProfileId).GetAwaiter().GetResult();
                mLog.Info($"get profile by id finish ,profile:{SerializerHelper.SerializeByJsonConvert(profile, true)}");
                List<string> selectedSiteUrls = GetSelectSitesUrl(profile);
                mLog.Info($"get selected site urls finish ,count:{selectedSiteUrls?.Count}, urls:{SerializerHelper.SerializeByJsonConvert(selectedSiteUrls, true)}");
                foreach (var url in selectedSiteUrls)
                {
                    try
                    {
                        mLog.Info($"start generate report for site url:{url}");
                        currentUrl = url;
                        int totalCount = 0;
                        string condition = $"StartTime >{profile.StartTime.Ticks} and FinishTime < {profile.EndTime.Ticks} and Level in ('RM_JS_Rule_ObjectLevel_DocumentVersion','Document','RM_JS_Rule_ObjectLevel_Document','ItemVersion','RM_JS_Rule_ObjectLevel_ItemVersion','Attachment','RM_JS_Rule_ObjectLevel_Attachment','Item','RM_JS_Rule_ObjectLevel_Item') and Status = '{(int)JobDetailsStatus.Successful}'";
                        mLog.Info($"generate report condition:{condition},url:{url}");
                        int startPage = 1;
                        IEnumerable<JMJobDetails> scResult;
                        do
                        {
                            scResult = scDetailWorker.GetData(1000, startPage, ref totalCount, condition, url);
                            if (scResult != null && scResult.Count() != 0)
                            {
                                CheckJobDetailsUrl(scResult, url);
                                InsertIntoRestoreReport(scResult);
                                startPage++;
                            }
                        }
                        while (scResult != null && scResult.Count() > 0);
                        mJobreport.AddGenerateRestoreReport(currentUrl, JobDetailsStatus.Successful);
                        mLog.Info($"finish generate report for site url:{url}");
                    }
                    finally
                    {
                        this.jobContext.ReportManager.Increase(phaseIncreaseProcessCount / selectedSiteUrls.Count);
                        increasedJobProcessCount += phaseIncreaseProcessCount / selectedSiteUrls.Count;
                    }                    
                }

            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception e)
            {
                mLog.Error($"generate report error ,error:{e},url:{currentUrl}");
                mJobreport.AddGenerateRestoreReport(currentUrl, JobDetailsStatus.Failed);
                mJobreport.HasErrorNode = true;
                throw;
            }

        }

        private void CheckJobDetailsUrl(IEnumerable<JMJobDetails> scResult, string targetSC)
        {
            if(scResult == null || scResult.Count() == 0)
            {
                return;
            }
            foreach (var item in scResult)
            {
                if(item == null)
                {
                    continue;
                }
                try
                {
                    JMRestoreScDetails tempDetail = item as JMRestoreScDetails;
                    if (tempDetail != null && tempDetail.SourceURL != null && !tempDetail.SourceURL.StartsWith(targetSC, StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Error($"data error ,url:{targetSC},jobId:{tempDetail.JobId},sourceUrl:{tempDetail.SourceURL} ");
                    }
                }
                catch(Exception ex)
                {
                    mLog.Error($"check job details url error, item:{SerializerHelper.SerializeByJsonConvert(item, true)},error:{ex}");
                }
            }
        }

        private void FinishJob(JobStatus status, string comment = "")
        {
            jobContext.ReportManager.SetJobFinished(status, comment);
        }
        private void InsertIntoRestoreReport(IEnumerable<JMJobDetails> scResult)
        {
            List<BaseReport> resultReport = new List<BaseReport>();
            foreach (var itemDetail in scResult)
            {
                resultReport.Add(ConvertToRestoreFileReport(itemDetail));
            }
            var jobDto = new BaseJobDto()
            {
                Id = jobContext.MainJobId,
                JobType = (int)JobType.GenerateRestoreReport,
            };
            ReportService.SyncReportJobDatas(resultReport, jobDto);
        }
        private RestoreFileReport ConvertToRestoreFileReport(JMJobDetails jobDetail)
        {
            JMRestoreScDetails tempDetail = jobDetail as JMRestoreScDetails;
            RestoreFileReport re = new RestoreFileReport();
            re.Size = tempDetail.Size;
            re.RestoreBy = tempDetail.RestoreBy;
            re.JobId = tempDetail.JobId;
            re.StartTime = tempDetail.StartTime;
            re.EndTime = tempDetail.FinishTime;
            re.RestoreTo = tempDetail.RestoreTo;
            re.IsDaoMigration = tempDetail.IsDaoMigration;
            re.IsEndUserOpt = tempDetail.IsEndUserOpt;
            re.Status = tempDetail.Status;
            re.Comment = tempDetail.Comment;
            re.TitleOrName = tempDetail.Name;
            re.Url = tempDetail.SourceURL;
            re.ObjectLevel = (int)JobReportUtility.ConvertDaoOrOpusLevelToObjectLevel(tempDetail.Level);
            return re;
        }
        private List<string> GetSelectSitesUrl(RMProfileDto profile)
        {
            try
            {
                List<RMSPTreeNode> sites = new List<RMSPTreeNode>();
                List<string> result = new List<string>();
                if(profile.Type == JobType.TeamsRestoreReport)
                {
                    mLog.Info($"start get teams site urls");
                    sites = ReportService.AssembleSitesAsync(profile, RMBrowseTreeNodeSourceType.Teams).GetAwaiter().GetResult();
                }
                else
                {
                    mLog.Info($"start get site urls, profile.Type:{profile.Type}");
                    RMBrowseTreeNodeSourceType type = profile.Type == JobType.RestoreReport ? RMBrowseTreeNodeSourceType.SharepointOnline : RMBrowseTreeNodeSourceType.SkyDrivePro;
                    sites = ReportService.AssembleSitesAsync(profile, type, false).GetAwaiter().GetResult();
                }
                foreach (var site in sites)
                {
                    result.Add(site.FullPath);
                }
                mLog.Info($"get site urls finish ,count:{result.Count}");
                return result;
            }
            catch (Exception ex) 
            {
                mLog.Error($"get site urls failed ,error:{ex}");
                return new List<string>();
            }
        }
        private void UploadScRptAndUpdateStatictisStatus()
        {
            int increasedJobProcessCount = 0;
            try
            {
                foreach (string url in scUrlSet)
                {
                    try
                    {
                        scDetailWorker.UploadReports(url);
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        haveError = true;
                        mLog.Error(@$"Fail upload sc rpt , sc url:{url} , ex:{ex}");
                    }
                    finally
                    {
                        this.jobContext.ReportManager.Increase(phaseIncreaseProcessCount / scUrlSet.Count);
                        increasedJobProcessCount += phaseIncreaseProcessCount / scUrlSet.Count;
                    }
                }
                if (statisticJobList == null || statisticJobList.Count == 0)
                {
                    mLog.Info($@"No restore job need update statistic status");
                    return;
                }
                foreach (var batch in statisticJobList.Batch(500))
                {
                    JMDao.BatchUpdate(batch.ToList());
                }
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception ex)
            {
                haveError = true;
                mLog.Error($@"Fail update restore job statictic status, ex:{ex}");
            }
            if (increasedJobProcessCount < phaseIncreaseProcessCount)
            {
                this.jobContext.ReportManager.Increase(phaseIncreaseProcessCount - increasedJobProcessCount);
            }
        }

        private void StatisticNewLogicRestoreJobs()
        {
            int increasedJobProcessCount = 0;
            mLog.Info($@"Start statistic new logic restore jobs");
            try
            {
                List<RMJobMonitor> unStatisticJobs = JMDao.GetUnstatisticFinishRestoreJobsByTime(startUtcTime.Ticks, endUtcTime.Ticks);
                foreach (RMJobMonitor jobMonitor in unStatisticJobs)
                {
                    scInitStatusInCurrentRestoreJob = new HashSet<string>();
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
                        jobRptPath = JobDetailService.DownloadReports(jobDto);
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
                        mLog.Error(@$"Fail statistic jobDetail,jobId:{jobMonitor.Id},ex:{ex}");
                    }
                    finally
                    {
                        SafeDeleteFile(jobRptPath);
                        statisticJobList.Add(jobMonitor);
                        this.jobContext.ReportManager.Increase(phaseIncreaseProcessCount / unStatisticJobs.Count);
                        increasedJobProcessCount += phaseIncreaseProcessCount / unStatisticJobs.Count;
                    }
                }
                mLog.Info($@"Finish statistic new logic restore jobs");
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception ex)
            {
                mLog.Error($@"Fail statistic new logic restore jobs,ex:{ex}");
            }
            if (increasedJobProcessCount < phaseIncreaseProcessCount)
            {
                this.jobContext.ReportManager.Increase(phaseIncreaseProcessCount - increasedJobProcessCount);
            }
        }

        private void DoNewLoginRestoreJobDetails(BaseJobDto jobDto, RMJobMonitor jobMonitor)
        {
            mLog.Info($@"Start insert job detail, jobId:{jobMonitor.Id}");
            int pageIndex = 1;
            int pageSize = 1000;
            int totalCount = 0;
            IEnumerable<JMRestoreScDetails> restoreDetails = new List<JMRestoreScDetails>();
            do
            {
                IEnumerable<JMRestoreActionJobDetailes> jMJobDetails = JobDetailService.GetData(pageSize, pageIndex, ref totalCount, null, jobDto)?.Cast<JMRestoreActionJobDetailes>();
                if (jMJobDetails==null)
                {
                    mLog.Error($@"Unable read job detail from rpt, jobId:{jobMonitor.Id}");
                    throw new Exception(@$"Unable read job details from rpt, jobId:{jobMonitor.Id}");
                }
                restoreDetails = jMJobDetails.Select(detail => ConvertJobDetailsToSCDetails(detail, jobMonitor));
                if(restoreDetails !=null && restoreDetails.Count()>0)
                {
                    InsertScData(restoreDetails);
                }
            } while (totalCount > pageIndex++ * pageSize);
            mLog.Info(@$"Finish insert job detail, jobId:{jobMonitor.Id}");
        }


        private void StatisticMigrationRestoreJobs()
        {
            int increasedJobProcessCount = 0;
            try
            {
                mLog.Info($@"Start statistic migration restore job");
                List<RMJobMonitor> unStatisticJobs = JMDao.GetUnstatisticFinishMigrationRestoreJobsByTime(startUtcTime.Ticks, endUtcTime.Ticks);
                foreach (RMJobMonitor jobMonitor in unStatisticJobs)
                {
                    scInitStatusInCurrentRestoreJob = new HashSet<string>();
                    string jobRptPath = string.Empty;
                    try
                    {
                        using (new CheckJobStopScope()) { }
                        ArchiverMigratedJobExtension jobExtension = new ArchiverMigratedJobExtension();
                        try
                        {
                            jobExtension = SerializerHelper.DeserializeByJsonConvert<ArchiverMigratedJobExtension>(jobMonitor.AdditionalInformation);
                        }
                        catch (Exception e)
                        {
                            mLog.Warn($"Deserialize ArchiverMigratedJobExtension Error {e}");
                        }
                        var jobDto = new BaseJobDto()
                        {
                            Id = jobMonitor.Id,
                            JobType = jobMonitor.JobType,
                            PlanId = jobExtension.PlanId,
                            Category = jobExtension.JobCategory,
                            TenantGroupEmail = TenantInfoDao.GetTenantInfo(TenantLocalValue.LogonGroupId)?.RegisterEmail,
                            IsMergeRpt = true
                        };
                        jobRptPath = JobDetailService.DownloadReports(jobDto);
                        DoMigrationRestoreJobDetails(jobMonitor, jobDto);
                        
                        jobMonitor.RestoreStatisticStatus = (int)MonitorStatisticStatus.Success;
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        jobMonitor.RestoreStatisticStatus = (int)MonitorStatisticStatus.PossbileFail;
                        mLog.Error(@$"Fail statistic jobDetail,jobId:{jobMonitor.Id},ex:{ex}");
                    }
                    finally
                    {
                        SafeDeleteFile(jobRptPath);
                        statisticJobList.Add(jobMonitor);
                        this.jobContext.ReportManager.Increase(phaseIncreaseProcessCount / unStatisticJobs.Count);
                        increasedJobProcessCount += phaseIncreaseProcessCount / unStatisticJobs.Count;
                    }
                }
                mLog.Info($@"Finish statistic migration restore job");
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception ex)
            {
                mLog.Error($@"Fail statistic migration restore job,ex:{ex}");
            }
            finally
            {
                if (increasedJobProcessCount < phaseIncreaseProcessCount)
                {
                    this.jobContext.ReportManager.Increase(phaseIncreaseProcessCount - increasedJobProcessCount);
                }
            }
        }

        private void DoMigrationRestoreJobDetails(RMJobMonitor jobMonitor, BaseJobDto jobDto)
        {
            mLog.Info($@"Start insert migration restore job details, jobid:{jobMonitor.Id}");
            int pageIndex = 1;
            int pageSize = 10000;
            int totalCount = 0;
            bool isOutOfRestore = JobDetailService.GetData(pageSize, pageIndex, ref totalCount, " Type = 'Document'", jobDto).Count() > 0;
            IEnumerable<JMRestoreScDetails> restoreDetails = new List<JMRestoreScDetails>();
            do
            {
                using (new CheckJobStopScope()) { }
                IEnumerable<JMJobDetails> jMJobDetails = JobDetailService.GetData(pageSize, pageIndex, ref totalCount, null, jobDto);
                if (jMJobDetails == null)
                {
                    mLog.Error($@"Unable read job detail from migraion resoter job rpt, jobId:{jobMonitor.Id}");
                    throw new Exception(@$"Unable read job details migraion resoter job from rpt, jobId:{jobMonitor.Id}");
                }
                restoreDetails = jMJobDetails.Cast<JMDisposalJobDetails>().Select(detail => ConvertJobDetailsToSCDetails(detail, jobMonitor, isOutOfRestore));
                if(restoreDetails != null && restoreDetails.Count() > 0)
                {
                    InsertScData(restoreDetails);
                }
            } while (totalCount>pageIndex++ *pageSize);
            mLog.Info($@"Finish insert migration restore job details , jobid:{jobMonitor.Id}");
        }

        private void InsertScData(IEnumerable<JMRestoreScDetails> detailsList)
        {
            //按照url分类
            Dictionary<string, List<JMRestoreScDetails>> detailsGroups = GroupRestoreScDetails(detailsList);
            foreach (KeyValuePair<string, List<JMRestoreScDetails>> group in detailsGroups)
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    if (source == SourceFlag.Teams && group.Key.IsNullOrEmpty()) continue;
                    //尝试下载已经存在的归档数据
                    string sCRptPath = scDetailWorker.DownloadReports(group.Key);
                    if (sCRptPath != null)
                    {
                        if (!scInitStatusInCurrentRestoreJob.Contains(group.Key))
                        {
                            scInitStatusInCurrentRestoreJob.Add(group.Key);
                            scDetailWorker.DeleteData(@$" JobId = '{group.Value.First().JobId}'", group.Key);
                        }
                    }
                    IList<JMRestoreScDetails> detailList = new List<JMRestoreScDetails>();
                    foreach (JMRestoreScDetails details in group.Value)
                    {
                        detailList.Add(details);
                        if (detailList.Count>1000)
                        {
                            scDetailWorker.InsertData(detailList, group.Key);
                            detailList.Clear();
                        }
                    }
                    if (detailList.Count>0)
                    {
                        scDetailWorker.InsertData(detailList, group.Key);
                        detailList.Clear();
                    }
                    scUrlSet.Add(group.Key);
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLog.Error(@$"Fail inset data into sc rpt:{group.Key},ex:{ex}");
                    throw;
                }
            }
        }

        private Dictionary<string, List<JMRestoreScDetails>> GroupRestoreScDetails(IEnumerable<JMRestoreScDetails> detailsList)
        {
            IOrderedEnumerable<string> scUrls = detailsList.Where(detail => detail.Level == "RM_JS_Rule_ObjectLevel_SiteCollection")
                .Select(detail => detail.SourceURL).Distinct().OrderDescending();
            Dictionary<string, List<JMRestoreScDetails>> res = new Dictionary<string, List<JMRestoreScDetails>>();
            foreach (var detail in detailsList)
            {
                string[] pathArr = detail.SourceURL.Split('/');
                if ((pathArr.Length > 1 && IsValidEmail(pathArr[0]) || IsValidEmail(detail.SourceURL)) && source == SourceFlag.Teams) continue;
                string scUrl = scUrls.FirstOrDefault(url => detail.SourceURL.StartsWith(url), GetScUrl(detail.SourceURL));
                List<JMRestoreScDetails> detailList = res.GetValueOrDefault(scUrl, new List<JMRestoreScDetails>());
                detailList.Add(detail);
                res[scUrl] = detailList;
            }
            return res;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        private string GetScUrl(string url)
        {
            string[] pathArr = url.Split('/');
            if (pathArr.Length<3)
            {
                return "";
            }
            else if (pathArr.Length < 5 || (pathArr[3] != "sites" && pathArr[3] != "personal"))
            {
                return pathArr[0] + "//" + pathArr[2];
            }
            else
            {
                return pathArr[0] + "//" + pathArr[2] + "/" + pathArr[3] + "/" + pathArr[4];
            }
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
                mLog.Error("delete file faile." + ex);
            }
        }



        private JMRestoreScDetails ConvertJobDetailsToSCDetails(JMDisposalJobDetails jobDetaile, RMJobMonitor jobMonitor, bool isOutOfRestore)
        {
            JMRestoreScDetails sCRestoreDetails = new JMRestoreScDetails()
            {
                Status = jobDetaile.Status,
                Comment = jobDetaile.Comment,
                Level = jobDetaile.Type,
                Name = jobDetaile.SourceURL.Split("/").LastOrDefault(),
                SourceURL = jobDetaile.SourceURL,
                Size = jobDetaile.SizeNumber,
                RestoreBy = jobMonitor.UserName,
                JobId = jobMonitor.Id,
                StartTime = jobMonitor.StartTime,
                FinishTime = jobMonitor.EndTime,
                RestoreTo = isOutOfRestore ? "RM_JS_JM_JobType_ArchiverOutPlaceRestore" : "RM_JS_JM_JobType_ArchiverRestore",
                IsDaoMigration = jobMonitor.DAOMigrated == true ? 1 : 0,
                IsEndUserOpt = jobMonitor.AdditionalInformation != null ? 1 : 0
            };
            return sCRestoreDetails;
        }

        private JMRestoreScDetails ConvertJobDetailsToSCDetails(JMRestoreActionJobDetailes jobDetaile, RMJobMonitor jobMonitor)
        {
            long size = 0;
            long.TryParse(jobDetaile.Size, out size);
            JMRestoreScDetails sCRestoreDetails = new JMRestoreScDetails()
            {
                Status = jobDetaile.Status,
                Comment = jobDetaile.Comment,
                Level = jobDetaile.Level,
                Name = jobDetaile.SourceLocation.Split("/").LastOrDefault(),
                SourceURL = jobDetaile.SourceLocation,
                Size = size,
                RestoreBy = jobMonitor.UserName,
                JobId = jobMonitor.Id,
                StartTime = jobMonitor.StartTime,
                FinishTime = jobDetaile.FinishTime,
                IsDaoMigration = jobMonitor.DAOMigrated == true ? 1 : 0,
                IsEndUserOpt = jobMonitor.AdditionalInformation != null ? 1 : 0
            };
            switch (jobMonitor.JobType)
            {
                case (int)JobType.ArchiverOutPlaceRestore:
                    sCRestoreDetails.RestoreTo = "RM_JS_JM_JobType_ArchiverOutPlaceRestore";
                    break;
                case (int)JobType.ArchiverRestore:
                case (int)JobType.AOSPRestore:
                    sCRestoreDetails.RestoreTo = "RM_JS_JM_JobType_ArchiverRestore";
                    break;
                case (int)JobType.TeamsArchiverRestore:
                    sCRestoreDetails.RestoreTo = "RM_JS_JM_JobType_TeamsArchiverRestore";
                    break;
                case (int)JobType.TeamsOutPlaceRestore:
                    sCRestoreDetails.RestoreTo = "RM_JS_JM_JobType_TeamsOutPlaceRestore";
                    break;
                case (int)JobType.StubOopRestore:
                    sCRestoreDetails.RestoreTo = "RM_JS_JM_JobType_StubOopRestore";
                    break;
                case (int)JobType.ArchiverToSpoRestore:
                    sCRestoreDetails.RestoreTo = "RM_JS_JM_JobType_ArchiverToSpoRestore";
                    break;
                case (int)JobType.StubArchiverRestore:
                    sCRestoreDetails.RestoreTo = "RM_JS_JM_JobType_StubArchiverRestore";
                    break;
                case (int)JobType.M365InPlaceArchiverRestore:
                    sCRestoreDetails.RestoreTo = "RM_JS_JM_JobType_M365InPlaceArchiverRestore";
                    break;
                default:
                    sCRestoreDetails.RestoreTo = "RM_JS_JM_JobType_ArchiverOutPlaceRestore";
                    break;
            }
            return sCRestoreDetails;
        }

    }
}
