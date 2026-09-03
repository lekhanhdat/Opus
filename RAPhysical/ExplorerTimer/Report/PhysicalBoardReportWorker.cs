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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.RAPhysical.ExplorerTimer.Report
{
    public class PhysicalBoardReportWorker : IBoradCollectionWorker
    {
        private static RALogger logger = RALogger.GetInstance(typeof(PhysicalBoardReportWorker));
        protected readonly string mJobId;
        private DateTime JobStartTime { get; set; }
        public PhysicalBoardReportWorker(string jobId)
        {
            mJobId = jobId;
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.PhysicalExplorerTimer);
            JobStartTime = DateTime.UtcNow;
        }

        private bool mHasErrorNode;

        public bool HasErrorNode
        {
            get
            {
                return mHasErrorNode;
            }

            private set { }
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

        #region dao
        private IAccountDao mRecordOwnersDao;
        protected IAccountDao RecordOwnersDao
        {
            get
            {
                if (mRecordOwnersDao == null)
                {
                    mRecordOwnersDao = (IAccountDao)PlatformWindsorManager.GetService(typeof(IAccountDao));
                }
                return mRecordOwnersDao;
            }

        }
        private IRMManualApproveDao mManualApproveDao;
        protected IRMManualApproveDao ManualApproveDao
        {
            get
            {
                if (mManualApproveDao == null)
                {
                    mManualApproveDao = (IRMManualApproveDao)PlatformWindsorManager.GetService(typeof(IRMManualApproveDao));
                }
                return mManualApproveDao;
            }

        }
        private IRMScopeDao _rmScopeDao;
        public IRMScopeDao RMScopeDao
        {
            get
            {
                if (_rmScopeDao == null)
                {
                    _rmScopeDao = (IRMScopeDao)PlatformWindsorManager.GetService(typeof(IRMScopeDao));
                }
                return _rmScopeDao;
            }
        }
        private AvePoint.RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public AvePoint.RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new AvePoint.RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
                }
                return _explorerDao;
            }
        }
        private IRecordAllianceDao mIRecordAllianceDao;
        protected IRecordAllianceDao RecordAllianceDao
        {
            get
            {
                if (mIRecordAllianceDao == null)
                {
                    mIRecordAllianceDao = (IRecordAllianceDao)PlatformWindsorManager.GetService(typeof(IRecordAllianceDao));
                }
                return mIRecordAllianceDao;
            }
        }
        private IRMSiteCollectionSizeDao mRMSiteCollectionSizeDao;
        protected IRMSiteCollectionSizeDao RMSiteCollectionSizeDao
        {
            get
            {
                if (mRMSiteCollectionSizeDao == null)
                {
                    mRMSiteCollectionSizeDao = (IRMSiteCollectionSizeDao)PlatformWindsorManager.GetService(typeof(IRMSiteCollectionSizeDao));
                }
                return mRMSiteCollectionSizeDao;
            }
        }
        private IRMDataOfDayDao mIRMDataOfDayDao;
        protected IRMDataOfDayDao RMDataOfDayDao
        {
            get
            {
                if (mIRMDataOfDayDao == null)
                {
                    mIRMDataOfDayDao = (IRMDataOfDayDao)PlatformWindsorManager.GetService(typeof(IRMDataOfDayDao));
                }
                return mIRMDataOfDayDao;
            }
        }
        private IRMTermUsageDao mRMTermUsageDao;
        protected IRMTermUsageDao RMTermUsageDao
        {
            get
            {
                if (mRMTermUsageDao == null)
                {
                    mRMTermUsageDao = (IRMTermUsageDao)PlatformWindsorManager.GetService(typeof(IRMTermUsageDao));
                }
                return mRMTermUsageDao;
            }
        }
        private ITermDao mTermDao;
        protected ITermDao TermDao
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mTermDao;
            }
        }
        private IRECOSiteCollectionDao mRECOSiteCollectionDao;
        protected IRECOSiteCollectionDao RECOSiteCollectionDao
        {

            get
            {
                if (mRECOSiteCollectionDao == null)
                {
                    mRECOSiteCollectionDao = (IRECOSiteCollectionDao)PlatformWindsorManager.GetService(typeof(IRECOSiteCollectionDao));
                }
                return mRECOSiteCollectionDao;
            }
        }
        private IReportCollectionService mDashBoardService;
        protected IReportCollectionService ReportCollectionService
        {
            get
            {
                if (mDashBoardService == null)
                {
                    mDashBoardService = (IReportCollectionService)PlatformWindsorManager.GetService(typeof(IReportCollectionService));
                }
                return mDashBoardService;
            }
        }
        private IBoardTotalDao mBoardTotalDao;
        protected IBoardTotalDao BoardTotalDao
        {
            get
            {
                if (mBoardTotalDao == null)
                {
                    mBoardTotalDao = (IBoardTotalDao)PlatformWindsorManager.GetService(typeof(IBoardTotalDao));
                }
                return mBoardTotalDao;
            }

        }
        private IRMBoardCacheDao mBoardCacheDao;
        protected IRMBoardCacheDao BoardCacheDao
        {
            get
            {
                if (mBoardCacheDao == null)
                {
                    mBoardCacheDao = (IRMBoardCacheDao)PlatformWindsorManager.GetService(typeof(IRMBoardCacheDao));
                }
                return mBoardCacheDao;
            }

        }
        private IWorkflowInstanceDao mWorkflowInstance;
        protected IWorkflowInstanceDao WorkflowInstanceDao
        {
            get
            {
                if (mWorkflowInstance == null)
                {
                    mWorkflowInstance = (IWorkflowInstanceDao)PlatformWindsorManager.GetService(typeof(IWorkflowInstanceDao));
                }
                return mWorkflowInstance;
            }

        }

        private IAccountDao mAccountDao;
        protected IAccountDao AccountDao
        {
            get
            {
                if (mAccountDao == null)
                {
                    mAccountDao = (IAccountDao)PlatformWindsorManager.GetService(typeof(IAccountDao));
                }
                return mAccountDao;
            }

        }
        #endregion

        private Dictionary<string, RMDataOfDay> lineDataDic = new Dictionary<string, RMDataOfDay>();
        Dictionary<string, PieChartDto> ownerMapping = new Dictionary<string, PieChartDto>();

        public async Task<bool> RunCollectionNowAsync()
        {

            logger.Info("Begin collection data.");
            long totalWaitingCount = 0;
            try
            {
                ReportManager.Increase();
                ReportManager.StartUpdateJobProgress();
                bool hasNext = true;
                string pageIndex = string.Empty;
                List<Record> datas = new List<Record>();
                while (hasNext)
                {
                    using (new PerformanceScope("CollectionData.Report.GetDataFromExplorerDB"))
                    {
                        Tuple<IEnumerable<Record>, string> result = ExplorerDao.QueryByPage(e => e.SourceFlag == (int)SourceFlag.Physical, RecordsConstants.ExplorerQueryPageSize, pageIndex);
                        hasNext = !string.IsNullOrEmpty(result.Item2);
                        pageIndex = result.Item2;
                        datas = result.Item1.ToList();
                        ProcessCreationAndDestroy(datas);
                    }
                }
                totalWaitingCount = await ProcessFullWaitingForApprovalNewAsync();// ProcessWaitingApproval();
                ProcessTotal(totalWaitingCount);
                ProcessTermUsage();
                CheckHoldStatus();
            }
            catch (Exception ex)
            {
                mHasErrorNode = true;
                logger.Error("error occurred while collect report:{0}", ex.ToString());
            }
            finally
            {
                ReportManager.WaitReportFinish();
            }
            return mHasErrorNode;
        }

        public async Task<bool> RunIncrementalCollectionAsync()
        {

            logger.Info("Begin incremental collection data.");
            try
            {
                using (new RA.Common.PerformanceScope(string.Format("BoardReportWorker.Physical.ProcessIncrementalTermUsages")))
                {
                    await ProcessIncrementalTermUsagesAsync();
                }
                var totalWatingCount = 0;
                using (new RA.Common.PerformanceScope(string.Format("BoardReportWorker.Physical.ProcessIncrementalDataOfDays")))
                {
                    totalWatingCount = await ProcessIncrementalDataOfDaysAsync();
                }
                using (new RA.Common.PerformanceScope(string.Format("BoardReportWorker.Physical.ProcessIncrementalTotals")))
                {
                    await ProcessIncrementalTotalsAsync();
                }
                using (new RA.Common.PerformanceScope(string.Format("BoardReportWorker.Physical.ProcessFullTotalWaitings")))
                {
                    await ProcessFullTotalWaitingsAsync(totalWatingCount);
                }

                CheckHoldStatus();
            }
            catch (Exception ex)
            {
                mHasErrorNode = true;
                logger.Error("error occurred while collect report:{0}", ex.ToString());
            }
            return mHasErrorNode;
        }

        private void ProcessTermUsage()
        {
            #region term usage
            try
            {
                using (new PerformanceScope("CollectionData.Report.TermUsage"))
                {
                    Dictionary<string, int> termIdAndRelatedCount = new Dictionary<string, int>();
                    string sql = "SELECT c.termId, COUNT(1) AS termcount FROM items c where(c.recordStatus = 1 or c.recordStatus = 6 or c.recordStatus = 7) and c.termId != '00000000-0000-0000-0000-000000000000' and c.sourceFlag=4 GROUP BY c.termId";
                    Dictionary<string, int> tempDic = ExplorerDao.QueryRelatedTermCount(sql);
                    termIdAndRelatedCount = tempDic.OrderByDescending(a => a.Value).Take(10).ToDictionary(a => a.Key, a => a.Value);
                    List<RMTermUsage> termUsageDatas = new List<RMTermUsage>();
                    foreach (var temp in termIdAndRelatedCount)
                    {
                        Guid temId = new Guid(temp.Key);
                        var termPath = TermDao.GetTermNamesPathByTermId(temId);
                        var currentTerm = TermDao.Find(s => s.UniqueId == temId);
                        if (currentTerm != null)
                        {
                            termUsageDatas.Add(new RMTermUsage() { TermName = currentTerm.Name, TermId = temId, Size = temp.Value, TermPath = termPath, SourceFlag = (int)SourceFlag.Physical });
                        }
                    }
                    RMTermUsageDao.RemoveAll(SourceFlag.Physical);
                    RMTermUsageDao.UpdateTermUsage(termUsageDatas);
                    ReportManager.Increase();
                }
            }
            catch (Exception ex)
            {
                mHasErrorNode = true;
                ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                {
                    ObjectName = "Top 10 Most Used Terms",
                    FullPath = string.Empty,
                    ItemType = "",
                    RuleName = "",
                    Status = JobDetailsStatus.Failed,
                    Comment = "Process term usage failed",
                });
                logger.Error("Collect term usage error {0}", ex.ToString());
            }
            #endregion
        }

      

        private void ProcessCreationAndDestroy(List<Record> datas)
        {
            #region created & destroy data
            try
            {
                ReportManager.Increase(1);
                var fsDestroyedResult = datas.Where(d => d.RecordStatus == (int)RMRecordStatus.Destroyed).Select(t => t.DestroyedTime).GroupBy(t => ConvertToShortTime(t)).Select(t => new { key = t.Key, value = t.Count() }).ToList();
                foreach (var entity in fsDestroyedResult)
                {
                    long ticks = ConvertDateTimeToTicks(entity.key);
                    RMDataOfDay data = new RMDataOfDay
                    {
                        Dater = ticks,
                        Destroyed = entity.value,
                        Timestamp = entity.key,
                        SourceFlag = (int)SourceFlag.Physical
                    };
                    if (lineDataDic.ContainsKey(entity.key))
                    {
                        lineDataDic[entity.key].Destroyed += entity.value;
                    }
                    else
                    {
                        lineDataDic.Add(entity.key, data);
                    }
                }
            }
            catch (Exception ex)
            {
                HasErrorNode = true;
                ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                {
                    ObjectName = I18NEntity.GetString("RM_DSB_DestroyedRecords"),
                    FullPath = string.Empty,
                    ItemType = "",
                    RuleName = "",
                    Status = JobDetailsStatus.Failed,
                    Comment = "Get destroy records error",
                });
                logger.Error("Get destroy records error {0}", ex.ToString());
            }
            try
            {
                var createdRecords = datas.GroupBy(d => ConvertToShortTime(d.TimeCreated)).ToList();
                foreach (var entity in createdRecords)
                {
                    long ticks = ConvertDateTimeToTicks(entity.Key);
                    RMDataOfDay data = new RMDataOfDay
                    {
                        Dater = ticks,
                        Created = Convert.ToInt64(entity.Count()),
                        Timestamp = entity.Key,
                        SourceFlag = (int)SourceFlag.Physical
                    };
                    if (lineDataDic.ContainsKey(entity.Key))
                    {
                        lineDataDic[entity.Key].Created += Convert.ToInt64(entity.Count());
                    }
                    else
                    {
                        lineDataDic.Add(entity.Key, data);
                    }
                }
                ReportManager.Increase();
            }
            catch (Exception ex)
            {
                HasErrorNode = true;
                ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                {
                    ObjectName = I18NEntity.GetString("RM_DSB_CreatedRecords"),
                    FullPath = string.Empty,
                    ItemType = "",
                    RuleName = "",
                    Status = JobDetailsStatus.Failed,
                    Comment = "Get created records error",
                });
                logger.Error("Get created records error {0}", ex.ToString());
            }
            #endregion
        }

        private string ConvertToShortTime(long ticks)
        {
            var time = new DateTime(ticks);
            return time.ToString("d");
        }

        private long ConvertDateTimeToTicks(string timeString)
        {
            DateTime time = Convert.ToDateTime(timeString);
            return time.Ticks;
        }

        /*private async Task<long> ProcessWaitingApprovalAsync()
        {
            long totalWaitingCount = 0;
            try
            {
                logger.Info("start to analyze waiting approval.");
                var allDatas = ManualApproveDao.GetAllDatas();
                var datasOfDay = allDatas.Where(m => m.SourceFlag == (int)SourceFlag.Physical).GroupBy(d => new DateTime(d.CollectionTime).ToString("d")).ToDictionary(o => o.Key, p => p.Count());
                var waitApprovalDatas = allDatas.Where(w => w.ActionStatus == 0 && w.Status == 1).ToList();
                foreach (var item in datasOfDay)
                {
                    if (lineDataDic.ContainsKey(item.Key))
                    {
                        lineDataDic[item.Key].WaitingApproval = item.Value;
                    }
                    else
                    {
                        RMDataOfDay data = new RMDataOfDay()
                        {
                            Dater = ConvertDateTimeToTicks(item.Key),
                            WaitingApproval = item.Value,
                            Timestamp = item.Key,
                            SourceFlag = (int)SourceFlag.Physical
                        };
                        lineDataDic.Add(item.Key, data);
                    }
                }
                totalWaitingCount = allDatas.Where(w => w.ActionStatus == 0 && w.Status == 1 && w.SourceFlag == (int)SourceFlag.Physical).Count();
                Dictionary<string, int> userData = new Dictionary<string, int>();
                logger.Info("waiting approval item count:{0}.", totalWaitingCount);
                foreach (var item in waitApprovalDatas)
                {
                    string url = string.Empty;
                    string fileName = string.Empty;
                    try
                    {
                        url = item.Url;
                        fileName = item.LeafName;
                        logger.Info("process approval fileName:{0}.", fileName);
                        var reviewUsers = await GetApprovalUsersAsync(item);
                        foreach (var userId in reviewUsers)
                        {
                            if (userData.ContainsKey(userId))
                            {
                                userData[userId] += 1;
                            }
                            else
                            {
                                userData.Add(userId, 1);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        HasErrorNode = true;
                        logger.Error("process waiting approval item failed, ERROR:{0}", ex.ToString());
                        //AddFailedDetail(fileName, url, ex.Message);
                        //ReportManager.SendJobDetail(new JMCollectionDataJobDetails() { ObjectName = fileName, FullPath = url, Status = JobDetailsStatus.Failed });
                        ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                        {
                            ObjectName = fileName,
                            FullPath = url,
                            ItemType = "",
                            RuleName = "",
                            Status = JobDetailsStatus.Failed,
                            Comment = "Process waiting approval item failed",
                        });
                    }
                }
                var top9Count = 0;
                var userIds = userData.OrderByDescending(u => u.Value).Select(d => int.Parse(d.Key)).ToList();
                if (userData.Count > 9)
                {
                    userIds = userIds.Take(9).ToList();

                }
                var recordOwners = await RecordOwnersDao.GetUserByIdsAsync(userIds);
                foreach (var owner in recordOwners)
                {
                    if (!ownerMapping.ContainsKey(owner.UserId))
                    {
                        ownerMapping.Add(owner.UserId, new PieChartDto { name = owner.DisplayName, data = userData[owner.Id.ToString()] });
                    }
                    else
                    {
                        ownerMapping[owner.UserId].data += userData[owner.Id.ToString()];
                    }
                    top9Count += userData[owner.Id.ToString()];
                }
                var ownerList = ownerMapping.Values.OrderByDescending(o => o.data).ThenBy(o => o.name).ToList();
                if (userData.Count > 9)
                {
                    var userTotal = userData.Values.Sum();
                    ownerList.Add(new PieChartDto { name = I18NEntity.GetString("RM_RC_Audit_Module_Others"), data = userTotal - top9Count });
                }

                //var owners = ownerMapping.Values.ToList();
                ReportCollectionService.RemoveAllAssignee();
                ReportCollectionService.AddApprovalAssigneeData(ownerList);
                ReportManager.Increase();
            }
            catch (Exception ex)
            {
                mHasErrorNode = true;
                logger.Warn("process waiting approval error:{0}", ex.ToString());
            }
            return totalWaitingCount;
        }*/

        #region New Logic for waiting approve dashboard, 目前和SP EXO大致逻辑一样， 需要找个时机整合

        public async Task<int> ProcessFullWaitingForApprovalNewAsync()
        {
            try
            {
                logger.Info("start ProcessFullWaitingForApprovalNew");
                ProcessWaitingDataOfDateNew();
                await ProcessWaitAssignerNewAsync();
                //下边计算所有的Waiting数据， 存的时候区分数据源， 显示的时候不区分
                return ProcessWaitingTotal();
            }
            catch (Exception ex)
            {
                mHasErrorNode = true;
                logger.Error($"error occurred while process waiting approval:{ex.ToString()}");
                ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = I18NEntity.GetString("Record Dashboard"),
                    FullPath = I18NEntity.GetString("Record Waiting Count"),
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message
                });
                return 0;
            }
        }
        /// <summary>
        /// 按天计算Waiting的数量， 区分数据源
        /// </summary>
        private void ProcessWaitingDataOfDateNew()
        {
            var index = 1;
            var pageSize = 10000;
            var totalCount = 0;
            Dictionary<long, int> waitForApprovalTimeDic = new Dictionary<long, int>();
            try
            {
                List<long> tempDatas = ManualApproveDao.GetAllCollectionTime(index, pageSize, ref totalCount, SourceFlag.Physical);
                logger.Info("Query collection time row count {0}, total count {1}", tempDatas.Count, totalCount);
                Dictionary<long, int> tempDic = tempDatas.GroupBy(a => new DateTime(a).ToString("d")).ToDictionary(o => ConvertDateTimeToTicks(o.Key), p => p.Count());
                AddTempDic2Total(tempDic, waitForApprovalTimeDic);
                while (totalCount - index * pageSize > 0)
                {
                    index++;
                    List<long> manualItems = ManualApproveDao.GetAllCollectionTime(index, pageSize, ref totalCount, SourceFlag.Physical);
                    logger.Info("Query collection time row count {0}, total count {1}", tempDatas.Count, totalCount);
                    tempDic = manualItems.GroupBy(a => new DateTime(a).ToString("d")).ToDictionary(o => ConvertDateTimeToTicks(o.Key), p => p.Count());
                    AddTempDic2Total(tempDic, waitForApprovalTimeDic);
                }
            }
            catch (Exception ex)
            {
                mHasErrorNode = true;
                throw ex;
            }
            logger.Info("Total date with data count {0}", waitForApprovalTimeDic.Count);
            ProcessWaitingTimeLine(waitForApprovalTimeDic);
        }
        private void ProcessWaitingTimeLine(Dictionary<long, int> tempWaitForApprovalTimeDic)
        {
            try
            {
                var dataOfDays = lineDataDic.Values.ToList();
                RMDataOfDayDao.RemoveAll(SourceFlag.Physical);
                RMDataOfDayDao.AddOrUpdateDatas(dataOfDays);
                //Wating Approval Count
                foreach (var timeCountPair in tempWaitForApprovalTimeDic)
                {
                    var currentDataOfDay = RMDataOfDayDao.FindWithNewContext(s => s.Dater == timeCountPair.Key && s.SourceFlag == (int)SourceFlag.Physical);
                    if (currentDataOfDay != null)
                    {
                        RMDataOfDayDao.AddOrUpdateDatas(new List<RMDataOfDay>(){new RMDataOfDay()
                        {
                            Id = currentDataOfDay.Id,
                            Created = currentDataOfDay.Created,
                            Destroyed = currentDataOfDay.Destroyed,
                            WaitingApproval = timeCountPair.Value,
                            Dater = currentDataOfDay.Dater,
                            Timestamp = currentDataOfDay.Timestamp,
                            SourceFlag = (int)SourceFlag.Physical
                        } });
                    }
                    else
                    {
                        RMDataOfDayDao.Create(new RMDataOfDay()
                        {
                            Created = 0,
                            Destroyed = 0,
                            WaitingApproval = timeCountPair.Value,
                            Dater = timeCountPair.Key,
                            Timestamp = new DateTime(timeCountPair.Key).ToString("d"),
                            SourceFlag = (int)SourceFlag.Physical
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mHasErrorNode = true;
                logger.Error("Get waiting for approval data of date records error {0}", ex.ToString());
                ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = I18NEntity.GetString("Record Waiting Count"),
                    FullPath = string.Empty,
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message
                });
            }
        }
        /// <summary>
        /// 按User计算Waiting 的数据，不区分数据源
        /// </summary>
        private async Task ProcessWaitAssignerNewAsync()
        {
            logger.Info("Start to process top 10 waiting assigner");
            //"1, 99"
            Dictionary<string, int> userData = new Dictionary<string, int>();
            //原有的非WorkFlow计算方法
            var index = 1;
            var pageSize = 2000;
            var totalCount = 0;
            List<string> owners = ManualApproveDao.GetOwnerExceptWorkflow(index, pageSize, ref totalCount);
            logger.Info("Query esclate to row count {0}, total count {1}", owners.Count, totalCount);
            AnalyzeOwnerStr(owners, userData);
            while (totalCount - index * pageSize > 0)
            {
                owners = ManualApproveDao.GetOwnerExceptWorkflow(index, pageSize, ref totalCount);
                logger.Info("Query esclate to row count {0}, total count {1}", owners.Count, totalCount);
                AnalyzeOwnerStr(owners, userData);
            }
            owners = null;
            logger.Info("Original without workflow, user data count {0}", userData.Count);
            //workflow的计算方法 
            try
            {
                //这里的Key是Guid
                Dictionary<string, int> workflowDic = ManualApproveDao.GetUserAndWaitingReviewCountMapping();
                logger.Info("manual approve workflow user data count {0}", workflowDic.Count);
                if (workflowDic.Count > 0)
                {
                    List<string> allUserIds = workflowDic.OrderByDescending(a => a.Value).Select(s => s.Key).ToList();
                    List<RMAccount> accounts = await AccountDao.GetUserByUserIdsAsync(allUserIds);
                    AnalyzeUserInManual(accounts, workflowDic, userData);
                }
            }
            catch (Exception e)
            {
                mHasErrorNode = true;
                logger.Error("Get waiting for approval data of date records error {0}", e.ToString());
                throw e;
            }
            logger.Info("total mixed user data count {0}", userData.Count);
            await ProcessTop10AssignerAsync(userData);
        }

        private async Task ProcessTop10AssignerAsync(Dictionary<string, int> userData)
        {
            var top9Count = 0;
            var userIds = userData.OrderByDescending(u => u.Value).Select(d => int.Parse(d.Key)).ToList();
            if (userData.Count > 9)
            {
                userIds = userIds.Take(9).ToList();

            }
            Dictionary<string, PieChartDto> ownerMapping = new Dictionary<string, PieChartDto>();
            var recordOwners = await RecordOwnersDao.GetUserByIdsAsync(userIds);
            foreach (var owner in recordOwners)
            {
                if (!ownerMapping.ContainsKey(owner.UserId))
                {
                    ownerMapping.Add(owner.UserId, new PieChartDto { name = owner.DisplayName, data = userData[owner.Id.ToString()] });
                }
                else
                {
                    ownerMapping[owner.UserId].data += userData[owner.Id.ToString()];
                }
                top9Count += userData[owner.Id.ToString()];
            }
            var ownerList = ownerMapping.Values.OrderByDescending(o => o.data).ThenBy(o => o.name).ToList();
            if (userData.Count > 9)
            {
                var userTotal = userData.Values.Sum();
                ownerList.Add(new PieChartDto { name = I18NEntity.GetString("RM_RC_Audit_Module_Others"), data = userTotal - top9Count });
            }
            ReportCollectionService.RemoveAllAssignee();
            ReportCollectionService.AddApprovalAssigneeData(ownerList);
        }

        private void AnalyzeUserInManual(List<RMAccount> accounts, Dictionary<string, int> workflowDic, Dictionary<string, int> userData)
        {
            foreach (RMAccount acc in accounts)
            {
                int count = workflowDic[acc.UserId];
                string shortId = acc.Id.ToString();
                if (userData.ContainsKey(shortId))
                {
                    userData[shortId] = userData[shortId] + count;
                }
                else
                {
                    userData[shortId] = count;
                }
            }
        }
        private void AnalyzeOwnerStr(List<string> owners, Dictionary<string, int> userData)
        {
            foreach (string owner in owners)
            {
                //"1|2|3"
                List<string> userIds = owner.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (string user in userIds)
                {
                    if (userData.ContainsKey(user))
                    {
                        userData[user] += 1;
                    }
                    else
                    {
                        userData.Add(user, 1);
                    }
                }
            }
        }
        
        private void AddTempDic2Total(Dictionary<long, int> temp, Dictionary<long, int> total)
        {
            foreach (long key in temp.Keys)
            {
                if (total.ContainsKey(key))
                {
                    total[key] = total[key] + temp[key];
                }
                else
                {
                    total.Add(key, temp[key]);
                }
            }
        }
        /// <summary>
        /// 计算所有的Waiting数据， 存的时候区分数据源， 显示的时候不区分
        /// </summary>
        private int ProcessWaitingTotal()
        {
            logger.Info("Start to process total waiting count");
            int totalWaiting = ManualApproveDao.GetWaitingCount(SourceFlag.Physical);
            var currentTotal = BoardTotalDao.FindWithNewContext(s => s.SourceFlag == (int)SourceFlag.Physical);
            logger.Info("New total waiting for approve data count {0}, original count {1}", totalWaiting, currentTotal == null ? 0 : currentTotal.WaitingTotal);
            if (currentTotal != null)
            {
                currentTotal.WaitingTotal = totalWaiting;
                BoardTotalDao.AddOrUpdate(currentTotal);
            }
            return totalWaiting;
        }

        #endregion

        private void ProcessTotal(long totalWaitingCount)
        {
            try
            {
                using (new PerformanceScope("CollectionData.Report.Total"))
                {
                    string sql = "SELECT VALUE COUNT(1) FROM c where (c.sourceFlag=4 and (c.recordStatus = 1 or c.recordStatus = 6 or c.recordStatus = 7))";
                    var totalCreatedCount = ExplorerDao.QueryCount(sql, null);
                    var dataOfDays = lineDataDic.Values.ToList();
                    //RMDataOfDayDao.RemoveAll(SourceFlag.Physical);
                    //RMDataOfDayDao.AddOrUpdateDatas(dataOfDays);

                    //var totalCreatedCount = dataOfDays.Sum(a => a.Created);
                    var totalDestroyedCount = dataOfDays.Sum(a => a.Destroyed);
                    BoardTotalDao.AddOrUpdate(new BoardTotal() { CollectionTime = JobStartTime.Ticks, WaitingTotal = totalWaitingCount, CreatedTotal = totalCreatedCount, DestroyedTotal = totalDestroyedCount, SourceFlag = (int)SourceFlag.Physical });
                    ReportManager.Increase();
                }
            }
            catch (Exception ex)
            {
                mHasErrorNode = true;
                ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                {
                    ObjectName = "Record Count by Status",
                    FullPath = string.Empty,
                    ItemType = "",
                    RuleName = "",
                    Status = JobDetailsStatus.Failed,
                    Comment = "Process total count error",
                });
                logger.Warn("process total count error:{0}", ex.ToString());
            }
        }

        private void CheckHoldStatus()
        {
            try
            {
                var utcNow = DateTime.UtcNow.Ticks;
                logger.Info("start to update record hold expired, utcNow:{0}.", utcNow);
                List<Guid> expiredIds = ExplorerDao.UpdateExpiredHeldRecords();
                ReportManager.Increase();
                logger.Info("record hold expired success.");
            }
            catch (Exception ex)
            {
                HasErrorNode = true;
                ReportManager.SendJobDetail(new JMPhysicalExplorerTimerJobDetails()
                {
                    ObjectName = "Check Hold Status",
                    FullPath = string.Empty,
                    ItemType = "",
                    RuleName = "",
                    Status = JobDetailsStatus.Failed,
                    Comment = "Check hold status error",
                });
                logger.Error("check hold status error {0}", ex.ToString());
            }
        }

        #region Incremental Collection
        private async Task ProcessIncrementalTermUsagesAsync()
        {
            try
            {
                //从临时表获取当前Data Sync Job相关的TermId List, 以及每个Term本次Job所要统计的个数更新到已存在数据的Size中;
                var tempCaches = BoardCacheDao.GetFilterList(s => new { Id = s.Id, TermId = s.TermId, Size = s.Size }, d => d.SubJobId.Contains(mJobId) && d.Type == 1);
                //遍历集合, 更新或者添加到RMTermUsages表中
                foreach (var tempCc in tempCaches)
                {
                    var currentTerm = TermDao.Find(s => s.UniqueId == tempCc.TermId && !s.IsRemoved);
                    var currentTermUsage = RMTermUsageDao.Find(s => s.TermId == tempCc.TermId && s.SourceFlag == (int)SourceFlag.Physical);
                    //Term存在, 正常获取最新的Term Name， Term Path信息添加或更新到TermUsage表中
                    if (currentTerm != null)
                    {
                        var tempTermName = currentTerm.Name;
                        var tempTermPath = TermDao.GetTermNamePath(currentTerm.Id);
                        if (currentTermUsage != null)
                        {
                            await RMTermUsageDao.UpdateAsync(new RMTermUsage()
                            {
                                Id = currentTermUsage.Id,
                                TermName = tempTermName,
                                TermId = tempCc.TermId,
                                Size = currentTermUsage.Size + Convert.ToInt32(tempCc.Size) <= 0 ? 0 : currentTermUsage.Size + Convert.ToInt32(tempCc.Size),
                                TermPath = tempTermPath,
                                SourceFlag = (int)SourceFlag.Physical
                            });
                        }
                        else
                        {
                            RMTermUsageDao.Create(new RMTermUsage()
                            {
                                TermName = tempTermName,
                                TermId = tempCc.TermId,
                                Size = Convert.ToInt32(tempCc.Size) <= 0 ? 0 : Convert.ToInt32(tempCc.Size),
                                TermPath = tempTermPath,
                                SourceFlag = (int)SourceFlag.Physical
                            });
                        }
                    }
                    //Term不存在， 若记录是新建， 则跳过， 若记录是更新， 则清空Size信息;
                    else
                    {
                        if (currentTermUsage != null)
                        {
                            await RMTermUsageDao.UpdateAsync(new DB.Model.RMTermUsage()
                            {
                                Id = currentTermUsage.Id,
                                TermName = currentTermUsage.TermName,
                                TermId = currentTermUsage.TermId,
                                Size = 0,
                                TermPath = currentTermUsage.TermPath,
                                SourceFlag = (int)SourceFlag.Physical
                            });
                        }
                    }
                    //临时表的数据每使用完一条， 删除一条记录.
                    BoardCacheDao.DeleteById(tempCc.Id);
                }
            }
            catch (Exception ex)
            {
                mHasErrorNode = true;
                ReportManager.SendJobDetail(new JMCollectionDataJobDetails() { ObjectName = "Top 10 Most Used Terms", FullPath = string.Empty, Status = JobDetailsStatus.Failed });
                logger.Error("Collect term usage error {0}", ex.ToString());
            }
        }

        private async Task<int> ProcessIncrementalDataOfDaysAsync()
        {
            var resultCount = 0;
            try
            {
                var tempDataOfDays = BoardCacheDao.GetFilterList(s => new { Id = s.Id, Dater = s.Dater, Size = s.Size, Type = s.Type }, d => d.SubJobId.Contains(mJobId) && (d.Type == 2 || d.Type == 3));
                foreach (var tempDataOfDay in tempDataOfDays)
                {
                    var currentDataOfDay = RMDataOfDayDao.Find(s => s.Dater == tempDataOfDay.Dater && s.SourceFlag == (int)SourceFlag.Physical);
                    if (currentDataOfDay != null)
                    {
                        switch (tempDataOfDay.Type)
                        {
                            case 2:
                                await RMDataOfDayDao.UpdateAsync(new RMDataOfDay() { Id = currentDataOfDay.Id, Created = currentDataOfDay.Created + tempDataOfDay.Size, Destroyed = currentDataOfDay.Destroyed, WaitingApproval = currentDataOfDay.WaitingApproval, Dater = currentDataOfDay.Dater, Timestamp = currentDataOfDay.Timestamp, SourceFlag = (int)SourceFlag.Physical });
                                break;
                            case 3:
                                await RMDataOfDayDao.UpdateAsync(new RMDataOfDay() { Id = currentDataOfDay.Id, Created = currentDataOfDay.Created, Destroyed = currentDataOfDay.Destroyed + tempDataOfDay.Size, WaitingApproval = currentDataOfDay.WaitingApproval, Dater = currentDataOfDay.Dater, Timestamp = currentDataOfDay.Timestamp, SourceFlag = (int)SourceFlag.Physical });
                                break;
                            default:
                                logger.Error("Invalid cached date type found. Operation type: {0}", tempDataOfDay.Type.ToString());
                                break;
                        }

                    }
                    else
                    {
                        switch (tempDataOfDay.Type)
                        {
                            case 2:
                                RMDataOfDayDao.Create(new RMDataOfDay()
                                {
                                    Created = tempDataOfDay.Size,
                                    Destroyed = 0,
                                    WaitingApproval = 0,
                                    Dater = tempDataOfDay.Dater,
                                    Timestamp = new DateTime(tempDataOfDay.Dater).ToString("d"),
                                    SourceFlag = (int)SourceFlag.Physical
                                });
                                break;
                            case 3:
                                RMDataOfDayDao.Create(new RMDataOfDay()
                                {
                                    Created = 0,
                                    Destroyed = tempDataOfDay.Size,
                                    WaitingApproval = 0,
                                    Dater = tempDataOfDay.Dater,
                                    Timestamp = new DateTime(tempDataOfDay.Dater).ToString("d"),
                                    SourceFlag = (int)SourceFlag.Physical
                                });
                                break;
                            default:
                                logger.Error("Invalid cached date type found. Operation type: {0}", tempDataOfDay.Type.ToString());
                                break;
                        }
                    }
                    //临时表的数据每使用完一条， 删除一条记录.
                    BoardCacheDao.DeleteById(tempDataOfDay.Id);
                }
                //Full Process Logic for Waiting Approval Count and WaitingApprovalAssignees Table.
                var tempTotalWaitApprovalAllData = ManualApproveDao.GetFilterList(s => new { Url = s.Url, LeafName = s.LeafName, EscalateTo = s.EscalateTo, Time = s.CollectionTime, Source = s.SourceFlag, ActionStatus = s.ActionStatus, Status = s.Status, WorkflowInstanceId = s.WorkflowInstanceId }, null);
                var tempTotalWaitApprovalPhysicalData = tempTotalWaitApprovalAllData.Where(a => a.Source == (int)SourceFlag.Physical);
                var tempCurrentWaitApprovalAllData = tempTotalWaitApprovalAllData.Where(a => a.ActionStatus == 0 && a.Status == 1).ToList();

                //Total Waiting Count
                //当前Waiting的数据
                resultCount = tempTotalWaitApprovalPhysicalData.Where(a => a.ActionStatus == 0 && a.Status == 1).ToList().Count;

                //以下数据更新折线, 需要统计Waiting表中某Type下的全部数据
                Dictionary<long, int> tempWaitForApprovalTimeDic = new Dictionary<long, int>();

                foreach (var tempEXOWait in tempTotalWaitApprovalPhysicalData)
                {
                    var dater = ConvertDateTimeToTicks(ConvertToShortTime(tempEXOWait.Time));
                    if (tempWaitForApprovalTimeDic.Keys.Contains(dater))
                    {
                        tempWaitForApprovalTimeDic[dater] = tempWaitForApprovalTimeDic[dater] + 1;
                    }
                    else
                    {
                        tempWaitForApprovalTimeDic.Add(dater, 1);
                    }
                }



                //Wating Approval Count
                foreach (var timeCountPair in tempWaitForApprovalTimeDic)
                {
                    var currentDataOfDay = RMDataOfDayDao.Find(s => s.Dater == timeCountPair.Key && s.SourceFlag == (int)SourceFlag.Physical);
                    if (currentDataOfDay != null)
                    {
                        await RMDataOfDayDao.UpdateAsync(new RMDataOfDay() { Id = currentDataOfDay.Id, Created = currentDataOfDay.Created, Destroyed = currentDataOfDay.Destroyed, WaitingApproval = timeCountPair.Value, Dater = currentDataOfDay.Dater, Timestamp = currentDataOfDay.Timestamp, SourceFlag = (int)SourceFlag.Physical });
                    }
                    else
                    {
                        RMDataOfDayDao.Create(new RMDataOfDay()
                        {
                            Created = 0,
                            Destroyed = 0,
                            WaitingApproval = timeCountPair.Value,
                            Dater = timeCountPair.Key,
                            Timestamp = new DateTime(timeCountPair.Key).ToString("d"),
                            SourceFlag = (int)SourceFlag.Physical
                        });
                    }
                }
                //WaitingApprovalAssignees Table.
                Dictionary<string, int> userData = new Dictionary<string, int>();
                logger.Info("waiting approval item count for owner chat: {0}.", tempCurrentWaitApprovalAllData.Count);
                foreach (var item in tempCurrentWaitApprovalAllData)
                {
                    string url = string.Empty;
                    string fileName = string.Empty;
                    try
                    {
                        url = item.Url;
                        fileName = item.LeafName;
                        logger.Info("process approval fileName:{0}.", fileName);
                        var reviewUsers = await GetApprovalUsersAsync(item.WorkflowInstanceId, item.EscalateTo);
                        foreach (var userId in reviewUsers)
                        {
                            if (userData.ContainsKey(userId))
                            {
                                userData[userId] += 1;
                            }
                            else
                            {
                                userData.Add(userId, 1);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        HasErrorNode = true;
                        logger.Error("process waiting approval item failed, ERROR:{0}", ex.ToString());
                        //AddFailedDetail(fileName, url, ex.Message);
                        ReportManager.SendJobDetail(new JMCollectionDataJobDetails() { ObjectName = fileName, FullPath = url, Status = JobDetailsStatus.Failed });
                    }
                }
                var top9Count = 0;
                var userIds = userData.OrderByDescending(u => u.Value).Select(d => int.Parse(d.Key)).ToList();
                if (userData.Count > 9)
                {
                    userIds = userIds.Take(9).ToList();

                }
                var recordOwners = await RecordOwnersDao.GetUserByIdsAsync(userIds);
                foreach (var owner in recordOwners)
                {
                    if (!ownerMapping.ContainsKey(owner.UserId))
                    {
                        ownerMapping.Add(owner.UserId, new PieChartDto { name = owner.DisplayName, data = userData[owner.Id.ToString()] });
                    }
                    else
                    {
                        ownerMapping[owner.UserId].data += userData[owner.Id.ToString()];
                    }
                    top9Count += userData[owner.Id.ToString()];
                }
                var ownerList = ownerMapping.Values.OrderByDescending(o => o.data).ThenBy(o => o.name).ToList();
                if (userData.Count > 9)
                {
                    var userTotal = userData.Values.Sum();
                    ownerList.Add(new PieChartDto { name = I18NEntity.GetString("RM_RC_Audit_Module_Others"), data = userTotal - top9Count });
                }
                ReportCollectionService.RemoveAllAssignee();
                ReportCollectionService.AddApprovalAssigneeData(ownerList);
            }
            catch (Exception ex)
            {
                HasErrorNode = true;
                ReportManager.SendJobDetail(new JMCollectionDataJobDetails() { ObjectName = I18NEntity.GetString("RM_DSB_DestroyedRecords"), FullPath = string.Empty, Status = JobDetailsStatus.Failed });
                logger.Error("Get destroy records error {0}", ex.ToString());
            }
            return resultCount;
        }

        private async Task ProcessIncrementalTotalsAsync()
        {
            try
            {
                var tempTotals = BoardCacheDao.GetFilterList(s => new { Id = s.Id, Size = s.Size, Type = s.Type }, d => d.SubJobId.Contains(mJobId) && (d.Type == 4 || d.Type == 5));
                foreach (var tempTotal in tempTotals)
                {
                    var currentTotal = BoardTotalDao.Find(s => s.SourceFlag == (int)SourceFlag.Physical);
                    if (currentTotal != null)
                    {
                        switch (tempTotal.Type)
                        {
                            case 4:
                                await BoardTotalDao.UpdateAsync(new BoardTotal()
                                {
                                    Id = currentTotal.Id,
                                    CreatedTotal = currentTotal.CreatedTotal + tempTotal.Size <= 0 ? 0 : currentTotal.CreatedTotal + tempTotal.Size,
                                    WaitingTotal = currentTotal.WaitingTotal,
                                    DestroyedTotal = currentTotal.DestroyedTotal,
                                    CollectionTime = JobStartTime.Ticks,
                                    SourceFlag = (int)SourceFlag.Physical
                                });
                                break;
                            case 5:
                                await BoardTotalDao.UpdateAsync(new BoardTotal()
                                {
                                    Id = currentTotal.Id,
                                    CreatedTotal = currentTotal.CreatedTotal,
                                    WaitingTotal = currentTotal.WaitingTotal,
                                    DestroyedTotal = currentTotal.DestroyedTotal + tempTotal.Size,
                                    CollectionTime = JobStartTime.Ticks,
                                    SourceFlag = (int)SourceFlag.Physical
                                });
                                break;
                            default:
                                logger.Error("Invalid cached date type found. Operation type: {0}", tempTotal.Type.ToString());
                                break;
                        }

                    }
                    else
                    {
                        switch (tempTotal.Type)
                        {
                            case 4:
                                BoardTotalDao.Create(new BoardTotal()
                                {
                                    CreatedTotal = tempTotal.Size,
                                    WaitingTotal = 0,
                                    DestroyedTotal = 0,
                                    CollectionTime = JobStartTime.Ticks,
                                    SourceFlag = (int)SourceFlag.Physical
                                });
                                break;
                            case 5:
                                BoardTotalDao.Create(new BoardTotal()
                                {
                                    CreatedTotal = 0,
                                    WaitingTotal = 0,
                                    DestroyedTotal = tempTotal.Size,
                                    CollectionTime = JobStartTime.Ticks,
                                    SourceFlag = (int)SourceFlag.Physical
                                });
                                break;
                            default:
                                logger.Error("Invalid cached date type found. Operation type: {0}", tempTotal.Type.ToString());
                                break;
                        }
                    }
                    //临时表的数据每使用完一条， 删除一条记录.
                    BoardCacheDao.DeleteById(tempTotal.Id);
                }
            }
            catch (Exception ex)
            {
                mHasErrorNode = true;
                ReportManager.SendJobDetail(new JMCollectionDataJobDetails() { ObjectName = "Record Count by Status", FullPath = string.Empty, Status = JobDetailsStatus.Failed });
                logger.Warn("process total count error:{0}", ex.ToString());
            }
        }
        private async Task<List<string>> GetApprovalUsersAsync(Guid instanceId, string escalateTo)
        {
            List<string> userIds = new List<string>();
            if (instanceId != Guid.Empty)
            {
                //通过workflow来查user
                var uids = WorkflowInstanceDao.GetReviewUserIdsByWFInstanceId(instanceId);
                var users = await AccountDao.GetUserByUserIdsAsync(uids);
                userIds = users.Select(u => u.Id).ToList().ConvertAll(i => i.ToString());
            }
            else if (!string.IsNullOrEmpty(escalateTo))
            {
                //通过record owner查user
                userIds = escalateTo?.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            logger.Info($"get review user id count:{userIds?.Count}");
            return userIds;
        }
        private async Task ProcessFullTotalWaitingsAsync(int totalWatingCount)
        {
            try
            {
                var currentTotal = BoardTotalDao.Find(s => s.SourceFlag == (int)SourceFlag.Physical);
                if (currentTotal != null)
                {
                    await BoardTotalDao.UpdateAsync(new BoardTotal()
                    {
                        Id = currentTotal.Id,
                        CreatedTotal = currentTotal.CreatedTotal,
                        WaitingTotal = totalWatingCount,
                        DestroyedTotal = currentTotal.DestroyedTotal,
                        CollectionTime = JobStartTime.Ticks,
                        SourceFlag = (int)SourceFlag.Physical
                    });
                }
                else
                {
                    BoardTotalDao.Create(new BoardTotal()
                    {
                        CreatedTotal = 0,
                        WaitingTotal = totalWatingCount,
                        DestroyedTotal = 0,
                        CollectionTime = JobStartTime.Ticks,
                        SourceFlag = (int)SourceFlag.Physical
                    });
                }
            }
            catch (Exception ex)
            {
                mHasErrorNode = true;
                ReportManager.SendJobDetail(new JMCollectionDataJobDetails() { ObjectName = "Record Waiting Count", FullPath = string.Empty, Status = JobDetailsStatus.Failed });
                logger.Warn("process full total waitings count error:{0}", ex.ToString());
            }
        }
        #endregion
    }
}
