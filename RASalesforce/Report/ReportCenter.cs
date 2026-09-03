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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Discovery.Salesforce.Enum;
using RASalesforce.APIs;
using RASalesforce.Util;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

namespace RASalesforce.Report;

public class ReportCenter
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(ReportCenter));
    public JobType JobType => _jobType;
    private IJobInfoUpdater _jobInfoUpdater;
    protected IJobInfoUpdater JobInfoUpdater
    {
        get
        {
            if (_jobInfoUpdater == null)
            {
                _jobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
            }
            return _jobInfoUpdater;
        }
    }
    public IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
    private readonly IRMReportManager _reportManager = ReportMangerFactory.Instance.ReportManager;
    private readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
    private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
    private readonly ObjectStatistic _successfulObj;
    private readonly ObjectStatistic _failedObj;
    private readonly ObjectStatistic _skippedObj;
    private readonly object _lockSuccessfulObj = new();
    private readonly object _lockFailedObj = new();
    private readonly object _lockSkippedObj = new();
    private string _organizationId;
    private string _jobId;
    public bool JobHasStopped { get; set; }

    private JobType _jobType;

    public ReportCenter()
    {
        _successfulObj = new ObjectStatistic();
        _failedObj = new ObjectStatistic();
        _skippedObj = new ObjectStatistic();
    }

    public ReportCenter Init(string apiName, string organizationId)
    {
        _organizationId = organizationId;
        return this;
    }

    public string GetTenantId()
    {
        return _organizationId;
    }
    
    public void RecordSkipCommon(JMJobDetails detail)
    {
        RecordSkip(detail);
    }

    public void InitCurrentJobInfo(string jobId, JobType jobType)
    {
        _jobId = jobId;
        _jobType = jobType;
        JobInfoUpdater.UpdateJobState(jobId, (int)JobStatus.InProgress);
        JobInfoUpdater.UpdateJobProgress(jobId, 1);
        ReportMangerFactory.Instance.Init(jobId, jobType, true);
        _reportManager.StartUpdateJobProgress();
    }
    

    public double GetProgress(string jobId)
    {
        return _subJobDao.GetSubJob(jobId)?.Progress ?? 100;
    }

    public void SetProgress(string jobId, int x)
    {
        _reportManager.SetProgress(x);
        _jobInfoUpdater.UpdateJobProgress(jobId, x);
    }

    public JobStatus GetMainJobState()
    {
        var parentId = _jobId.Split('_')?[0];
        var parentStatus = _jobMonitorService.GetJobStatus(parentId);
        return parentStatus;
    }

    public void RecordSuccessful(JMJobDetails detail)
    {
        IncreaseObjCount(_successfulObj, _lockSuccessfulObj);
        _reportManager.SendJobDetail(detail);
    }

    public void RecordFailed(JMJobDetails detail)
    {
        IncreaseObjCount(_failedObj, _lockFailedObj);
        _reportManager.SendJobDetail(detail);
    }

    public void RecordSuccessfulCommon(JMJobDetails detail)
    {
        RecordSuccessful(detail);
    }

    public void RecordFailedCommon(JMJobDetails detail)
    {
        RecordFailed(detail);
    }
    

    public void RecordSkip(JMJobDetails detail)
    {
        detail.Status = JobDetailsStatus.Skipped;
        IncreaseObjCount(_skippedObj, _lockSkippedObj);
        _reportManager.SendJobDetail(detail);
    }

    private void IncreaseObjCount(ObjectStatistic obj, object lockObj)
    {
        lock (lockObj)
        {
            obj.SObjectCount++;
        }
    }

    public void SetJobFinish(JobStatus jobStatus, string comment = "")
    {
        _logger.Info($"Job finished with status: [{jobStatus}], comment: [{comment}]");
        if (jobStatus == JobStatus.Stopped)
        {
            _reportManager.SetJobFinished(jobStatus);
            return;
        }

        if (jobStatus != JobStatus.Finished && string.IsNullOrEmpty(comment))
        {
            comment = "RM_TS_SS_Summary";
        }
        _reportManager.SetJobFinished(jobStatus, comment);
    }

    #region generate job detail

    public JMJobDetails GenerateCommonJobDetail(JobType jobType, RMDiscoverySalesforceObjectInfo objectInfo, JobDetailsStatus status, string comment = "")
    {
        switch (jobType)
        {
            case JobType.SFDiscoveryJob:
                return new JMSalesforceDiscoveryJob
                {
                    ObjectName = objectInfo.DisplayName,
                    ObjectType = I18NResource.GetSalesforceObjectTypeI18N((RMDiscoverySalesforceObjectType)objectInfo.ObjectType),
                    TotalItemCount = objectInfo.TotalItemCount,
                    Status = status,
                    TotalSize = objectInfo.TotalSize,
                    TenantId = _organizationId,
                    Comment = comment
                };
            default:
                return new JMJobDetailsCommon();
        }
    }

    #endregion

    public void Completed(string comment = "")
    {
        var jobFinishStatus = (_successfulObj.SObjectCount > 0 || _skippedObj.SObjectCount > 0) && _failedObj.SObjectCount > 0 ?
            JobStatus.FinishWithException :

                _failedObj.SObjectCount > 0 ?
                JobStatus.Failed :
                JobStatus.Finished
            ;
        if (JobHasStopped)
        {
            var parentId = _jobId.Split('_')?[0];
            var runningSubjobIds = _subJobDao.GetAllSubJobIds(parentId, [(int)JobStatus.InProgress, (int)JobStatus.Wait]);
            _logger.Info($"Those [{string.Join(',', runningSubjobIds)}] subjobs are running");
            foreach (var id in runningSubjobIds)
            {
                if (id.Equals(_jobId))
                {
                    continue;
                }
                _subJobDao.UpdateStatus(id, (int)JobStatus.Failed, DateTime.UtcNow.Ticks);
                _logger.Info($"Stopping subjob [{id}] at {DateTime.UtcNow.Ticks}");
            }
            jobFinishStatus = JobStatus.Failed;
        }
        SetJobFinish(jobFinishStatus, comment);
    }
    
    
    public ObjectStatusEnum GetObjectStatus<T>(List<string> errors, List<T> tempList)
    {
        return errors.Any() switch
        {
            true when tempList.Any() => ObjectStatusEnum.FinishedWithException,
            false when tempList.Any() => ObjectStatusEnum.Success,
            true when !tempList.Any() => ObjectStatusEnum.Failed,
            _ => ObjectStatusEnum.Success
        };
    }
}
