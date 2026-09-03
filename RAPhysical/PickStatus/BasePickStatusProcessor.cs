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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.PickStatus
{
    public abstract class BasePickStatusProcessor
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(BasePickStatusProcessor));
        #region Interface
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

        protected IRMReportManager mReportManager;
        protected IRMReportManager ReportManager
        {
            get
            {
                if (mReportManager == null)
                {
                    mReportManager = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManager;
            }
        }

        private IRMSubJobDao mSubJobDao;
        public IRMSubJobDao SubJobDao
        {
            get
            {
                if (mSubJobDao == null)
                {
                    mSubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
                }
                return mSubJobDao;
            }
        }

        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }
        private IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService<IExplorerQueryService>();
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        protected IPickListService PickListService => PlatformWindsorManager.GetService<IPickListService>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        #endregion
        protected string JobId = string.Empty;
        protected const int PageSize = 100;
        protected List<Guid> successIds = new List<Guid>();
        protected List<Guid> failedIds = new List<Guid>();
        protected List<RMLocation> Locations = new List<RMLocation>();
        public BasePickStatusProcessor(JobType jobType, string jobId)
        {
            JobId = jobId;
            ReportMangerFactory.Instance.Init(JobId, jobType, true);
            JobInfoUpdater.UpdateJobState(JobId, (int)JobStatus.InProgress);
            ReportManager.StartUpdateJobProgress();
        }

        public async Task RunAsync()
        {
            logger.Info("Start to run pick status job.");
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(JobId, true);
            logger.Info("Get job message:{0}", subJobWithContext.JobContext.Content);
            var jobParam = SerializerHelper.DeserializeByDataContractSerializer<PickListJobMessage>(subJobWithContext.JobContext.Content);
            Tuple<List<BaseRecordDto>, ExplorerPagingInfo> result = new Tuple<List<BaseRecordDto>, ExplorerPagingInfo>(null, new ExplorerPagingInfo() { });
            await PrepareProcessAsync();
            do
            {
                result = await GetDataByPagerAsync(jobParam.ActionParam, result.Item2.PageIndex);
                Locations.AddRange(LocationDao.GetLocationByUniqueIds(result.Item1.Select(record => record.LocationId).ToList()));
                foreach (var rec in result.Item1)
                {
                    try
                    {
                        if (rec != null)
                        {
                            await ProcessRecordsAsync(rec);
                        }
                    }
                    catch (Exception e)
                    {
                        if (rec != null)
                        {
                            failedIds.Add(rec.Id);
                        }
                        SendDetails(rec, JobDetailsStatus.Failed);
                        logger.Warn($"Update pick status error:{e}");
                    }
                }
            } while (result.Item2.HasNextPage);
            await AfterProcessAsync();

            if (failedIds.Count > 0)
            {
                if (successIds.Count == 0)
                {
                    ReportManager.SetJobFinished(JobStatus.Failed);
                }
                else
                {
                    ReportManager.SetJobFinished(JobStatus.FinishWithException);
                }
            }
            else
            {
                ReportManager.SetJobFinished(JobStatus.Finished);
            }
        }

        public void SendDetails(BaseRecordDto record, JobDetailsStatus status = JobDetailsStatus.Successful, string comment = "")
        {
            if (successIds.Count(r => r == record.Id) > 1 || failedIds.Count(r => r == record.Id) > 1)
            {
                return;
            }
            var fullPath = ExplorerService.GetPhysicalObjectFullPath(record.Id, true) + "/" + record.LeafName;
            ReportManager.SendJobDetail(new JMPickCompleteJobDetails()
            {
                Name = record.LeafName,
                FullPath = fullPath,
                Status = status,
                Comment = comment
            });
        }

        public void SendDetails(Record record, JobDetailsStatus status = JobDetailsStatus.Successful, string comment = "")
        {
            SendDetails(new BaseRecordDto() { LeafName = record.LeafName, Id = record.Id }, status, comment);
        }

        public async Task<Tuple<List<BaseRecordDto>, ExplorerPagingInfo>> GetDataByPagerAsync(CompleteActionParam jobParam, string pageIndex)
        {
            ExplorerQueryV3Dto dto = GetQueryDto(jobParam, pageIndex);
            var resultInfo = await ExplorerQueryService.QueryDataListWithoutTotalAsync(dto);
            return new Tuple<List<BaseRecordDto>, ExplorerPagingInfo>(resultInfo.Datas, resultInfo.PagingInfo);
        }

        protected abstract Task PrepareProcessAsync();
        protected abstract Task AfterProcessAsync();
        protected abstract Task ProcessRecordsAsync(BaseRecordDto rec);

        protected abstract ExplorerQueryV3Dto GetQueryDto(CompleteActionParam jobParam, string pageIndex);

    }
}
