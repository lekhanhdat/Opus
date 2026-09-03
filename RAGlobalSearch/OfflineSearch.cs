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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.PersonalSetting;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Lite;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RADataBroker;
using RAGlobalSearch.Common;
using RAGlobalSearch.Discover;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RAGlobalSearch
{
    public class OfflineSearch : IDisposable
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(OfflineSearch));
        #region interface 
        private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
         
        public IExplorerQueryService ExplorerQueryService { set; get; } = PlatformWindsorManager.GetService<IExplorerQueryService>();
        
        public IPersonalSettingService PersonalSettingService { set; get; } = PlatformWindsorManager.GetService<IPersonalSettingService>();
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
        private IExplorerDao _explorerDao;
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao();
                }
                return _explorerDao;
            }
        }

        protected IJobInfoUpdater JobInfoUpdater { set; get; } = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
        
        #endregion
        private string mJobId;
        private string scopeId;
        private bool mHasError = false;
        private readonly int _defaultPageSize = 1000;
        private readonly string _defaultDBFilePrefix = "SearchResult_";
        private string resultFilePath = null;
        private int totalCount = 0;
        private readonly string _finalComments = "Result(s) count {0}";
        private string errorMsg = "";
        private ExplorerOfflineSearchWrapper explorerOfflineSearchWrapper;
        public OfflineSearch(string jobId, string userId)
        {
            mJobId = jobId;
            TenantLocalValue.LogonUserId = userId;
            resultFilePath = AvePoint.RA.Common.Util.JobReportUtility.GetSearchResultFilePath(_defaultDBFilePrefix + jobId + ".db");
            ReportMangerFactory.Instance.Init(mJobId, AvePoint.RA.Contract.JobMonitor.JobType.ExplorerOfflineSearch, true);
            JobInfoUpdater.UpdateJobState(mJobId, (int)JobStatus.InProgress);
            ReportManager.StartUpdateJobProgress();
        }

        public async Task RunAsync()
        {
            IGlobalSearchAction action = null;
            try
            {

                using (new PerformanceScope("OfflineSearch.Run", addToStatistics: true)) 
                {
                    logger.Info("Start to run global search action job.");
                    ExplorerQueryV3Dto queryV3Dto = null;
                    using (new PerformanceScope("OfflineSearch.Init", addToStatistics: true)) 
                    {
                        RMJobMonitor job = JobMonitorDao.GetJob(mJobId);
                        int settingId = int.Parse(job.ScopeId);
                        scopeId = job.ScopeId;
                        RMPersonalSettingDto profile = PersonalSettingService.GetById(settingId, true);
                        queryV3Dto = AssembleQueryDto(profile);
                    }
                    if (queryV3Dto == null)
                    {
                        //fail job.
                    }
                    int index = 0;
                    do
                    {
                        using (new PerformanceScope("OfflineSearch.QuerybyPager", addToStatistics: true)) 
                        {
                            var filterOption = await ExplorerQueryService.PrepareFilterV2Async(queryV3Dto);
                            //ExplorerResultInfo explorerResult = ExplorerQueryService.QueryDataListWithoutTotal(queryV3Dto);
                            Tuple<IEnumerable<Record>, string> result = ExplorerDao.SearchRecordsV3(queryV3Dto, filterOption);
                            ArgumentCheck.NotNull(queryV3Dto, nameof(queryV3Dto));
                            queryV3Dto.PagingInfo.HasNextPage = !string.IsNullOrEmpty(result.Item2);
                            queryV3Dto.PagingInfo.PageIndex = result.Item2;
                            int resultCount = result.Item1.Count();
                            totalCount += resultCount;
                            logger.Info($"query for {index} times, result count:{resultCount}, has next page:{queryV3Dto.PagingInfo.HasNextPage}");
                            SavePagingResult(result.Item1);
                        }
                        
                    }
                    while (queryV3Dto.PagingInfo.HasNextPage);
                    logger.Info("finish searching, total result count {0}", totalCount);
                }
               
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                mHasError = true;
                logger.Error($"An error occurred while running global search action job. Error:{e.ToString()}");
            }
            finally
            {
                explorerOfflineSearchWrapper?.Dispose();
                UploadResult();
                await UpdateJobStateAsync(action);
                PerformanceMonitor.WritePerformanceResult();
                //RemoveLocalDB();
            }
        } 

        private ExplorerQueryV3Dto AssembleQueryDto(RMPersonalSettingDto profile)
        {
            if (profile != null)
            {
                ExplorerQueryV3Dto queryV3Dto = new ExplorerQueryV3Dto();

                RMExplorerSearchCriteriaSetting setting = SerializerHelper.DeserializeByJsonConvert<RMExplorerSearchCriteriaSetting>(profile.ContentStr);
                if (setting != null && setting.AdvancedSearchs != null)
                {
                    ExplorerQueryOptionV3 optionV3 = new ExplorerQueryOptionV3() { Values = new List<ExplorerSearchOptionV3>() };
                    if (!string.IsNullOrEmpty(setting.ColumnSortSetting))
                    {
                        ExplorerQueryOrderColumn orderColumn = SerializerHelper.DeserializeByJsonConvert<ExplorerQueryOrderColumn>(setting.ColumnSortSetting);
                        optionV3.OrderColumn = orderColumn;
                    }
                    foreach (var option in setting.AdvancedSearchs)
                    {
                        if (!string.IsNullOrEmpty(option.ContentStr))
                        {
                            ExplorerSearchOptionV3 searchOption = SerializerHelper.DeserializeByJsonConvert<ExplorerSearchOptionV3>(option.ContentStr);
                            optionV3.Values.Add(searchOption);
                        }
                    }
                    queryV3Dto.QueryOption = optionV3;
                    queryV3Dto.PagingInfo = new ExplorerPagingInfo() { PageIndex = "", PageSize = _defaultPageSize, HasNextPage = true };
                    return queryV3Dto;
                }
                else
                {
                    logger.Warn("Saved search profile is null");
                }
            }
            return null;
        }

        private void SavePagingResult(IEnumerable<Record> result)
        {
            if(explorerOfflineSearchWrapper == null)
            {
                explorerOfflineSearchWrapper = new ExplorerOfflineSearchWrapper(resultFilePath);
            }
            explorerOfflineSearchWrapper.Insert(result.ToList());
        }

        private void UploadResult()
        {
            if (!string.IsNullOrEmpty(resultFilePath) && totalCount > 0)
            {
                try
                {
                    using (new PerformanceScope("OfflineSearch.UploadReport", addToStatistics: true))
                    {
                        string blobName = AvePoint.RA.Common.Util.JobReportUtility.GetSearchResultBlobPath(_defaultDBFilePrefix + this.mJobId + ".db");
                        RAStorageUtil.UploadReportBlob(blobName, resultFilePath);
                    }
                   
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                }
            }
            else
            {
                logger.Info("No need to upload db file");
            }
        }



        private async Task UpdateJobStateAsync(IGlobalSearchAction action)
        {
            JobStatus status = JobStatus.Finished;
            if (mHasError)
            {
                status = totalCount > 0 ? JobStatus.FinishWithException : JobStatus.Failed;
            } 
            
            if (status == JobStatus.Finished || status == JobStatus.FinishWithException)
            {
                JobInfoUpdater.UpdateJobState(mJobId, (int)status, string.Format(_finalComments, totalCount.ToString()));  
                try
                {
                    logger.Warn("try to delete old search job. ");
                    IJobMonitorService jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
                    await jobMonitorService.DeleteOldOfflineSearchJobAsync(scopeId, mJobId);
                }
                catch (Exception e)
                {
                    logger.Warn("Error while deleting old search job. {0}", e.ToString());
                }
            }
            else
            {
                JobInfoUpdater.UpdateJobState(mJobId, (int)status, errorMsg);
            }
        }

        public void Dispose()
        {
            explorerOfflineSearchWrapper?.Dispose();
        }
    }
}
