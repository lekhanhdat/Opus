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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RACloudFS.FSDashBoard
{
    public class FSDashBoardReportWorker
    {
        private static RALogger logger = RALogger.GetInstance(typeof(FSDashBoardReportWorker));
        protected readonly string mJobId;
        private DateTime JobStartTime { get; set; }

        #region dao
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



        private int failedCount = 0;

        public FSDashBoardReportWorker(string jobId)
        {
            mJobId = jobId;
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.FSDashBoard);
            JobStartTime = DateTime.UtcNow;
        }
        public async Task RunCollectionNowAsync()
        {

            logger.Info("Begin collection data.");
            long totalWaitingCount = 0;
            try
            {
                ReportManager.Increase(1);
                ReportManager.StartUpdateJobProgress();
                bool hasNext = true;
                string pageIndex = string.Empty;
                List<Record> datas = new List<Record>();
                while (hasNext)
                {
                    using (new PerformanceScope("CollectionData.Report.GetDataFromExplorerDB"))
                    {
                        Tuple<IEnumerable<Record>, string> result = ExplorerDao.QueryByPage(e => e.SourceFlag == (int)SourceFlag.FileSystem && e.NodeType == 2200 && e.RecordStatus != (int)RMRecordStatus.Moved, RecordsConstants.ExplorerQueryPageSize, pageIndex);
                        hasNext = !string.IsNullOrEmpty(result.Item2);
                        pageIndex = result.Item2;
                        datas = result.Item1.ToList();
                        ProcessCreationAndDestroy(datas);
                    }
                }
                ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
                {
                    Action = "RM_DSB_DestroyedRecords",
                    Status = JobDetailsStatus.Successful,
                });
                ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
                {
                    Action = "RM_DSB_CreatedRecords",
                    Status = JobDetailsStatus.Successful,
                });
                totalWaitingCount = await ProcessFullWaitingForApprovalNewAsync();
                ProcessTotal(totalWaitingCount);
                ProcessTermUsage();
                CheckHoldStatus();
                if (failedCount > 0)
                {
                    ReportManager.SetJobFinished(JobStatus.FinishWithException);
                }
                else
                {
                    ReportManager.SetJobFinished(JobStatus.Finished);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Run fs dashboard has error {0}", ex.ToString());
                ReportManager.SetJobFinished(JobStatus.Failed, ex.Message.ToString());
            }
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
                        SourceFlag = (int)SourceFlag.FileSystem
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
                failedCount++;
                ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
                {
                    Action = "RM_DSB_DestroyedRecords",
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message,
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
                        SourceFlag = (int)SourceFlag.FileSystem
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
                failedCount++;
                ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
                {
                    Action = "RM_DSB_CreatedRecords",
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message,
                });
                logger.Error("Get created records error {0}", ex.ToString());
            }
            #endregion
        }
        public async Task<int> ProcessFullWaitingForApprovalNewAsync()
        {
            try
            {
                ReportManager.Increase(10);
                logger.Info("start ProcessFullWaitingForApprovalNew");
                ProcessWaitingDataOfDateNew();
                await ProcessWaitAssignerNewAsync();
                //下边计算所有的Waiting数据， 存的时候区分数据源，
                return ProcessWaitingTotal();
            }
            catch (Exception ex)
            {
                failedCount++;
                logger.Error($"error occurred while process waiting approval:{ex.ToString()}");
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
                List<long> tempDatas = ManualApproveDao.GetAllCollectionTime(index, pageSize, ref totalCount, SourceFlag.FileSystem);
                logger.Info("Query collection time row count {0}, total count {1}", tempDatas.Count, totalCount);
                Dictionary<long, int> tempDic = tempDatas.GroupBy(a => new DateTime(a).ToString("d")).ToDictionary(o => ConvertDateTimeToTicks(o.Key), p => p.Count());
                AddTempDic2Total(tempDic, waitForApprovalTimeDic);
                while (totalCount - index * pageSize > 0)
                {
                    index++;
                    List<long> manualItems = ManualApproveDao.GetAllCollectionTime(index, pageSize, ref totalCount, SourceFlag.FileSystem);
                    logger.Info("Query collection time row count {0}, total count {1}", tempDatas.Count, totalCount);
                    tempDic = manualItems.GroupBy(a => new DateTime(a).ToString("d")).ToDictionary(o => ConvertDateTimeToTicks(o.Key), p => p.Count());
                    AddTempDic2Total(tempDic, waitForApprovalTimeDic);
                }
            }
            catch (Exception ex)
            {
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
                RMDataOfDayDao.RemoveAll(SourceFlag.FileSystem);
                RMDataOfDayDao.AddOrUpdateDatas(dataOfDays);
                //Wating Approval Count
                foreach (var timeCountPair in tempWaitForApprovalTimeDic)
                {
                    var currentDataOfDay = RMDataOfDayDao.FindWithNewContext(s => s.Dater == timeCountPair.Key && s.SourceFlag == (int)SourceFlag.FileSystem);
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
                            SourceFlag = (int)SourceFlag.FileSystem
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
                            SourceFlag = (int)SourceFlag.FileSystem
                        });
                    }
                }
                ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
                {
                    Action = "RM_DSB_RecordWaiting",
                    Status = JobDetailsStatus.Successful,
                });
            }
            catch (Exception ex)
            {
                failedCount++;
                logger.Error("Get waiting for approval data of date records error {0}", ex.ToString());
                ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
                {
                    Action = "RM_DSB_RecordWaiting",
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

        /// 计算所有的Waiting数据， 存的时候区分数据源， 显示的时候不区分
        /// </summary>
        private int ProcessWaitingTotal()
        {
            logger.Info("Start to process total waiting count");
            int totalWaiting = ManualApproveDao.GetWaitingCount(SourceFlag.FileSystem);
            var currentTotal = BoardTotalDao.FindWithNewContext(s => s.SourceFlag == (int)SourceFlag.FileSystem);
            logger.Info("New total waiting for approve data count {0}, original count {1}", totalWaiting, currentTotal == null ? 0 : currentTotal.WaitingTotal);
            if (currentTotal != null)
            {
                currentTotal.WaitingTotal = totalWaiting;
                BoardTotalDao.AddOrUpdate(currentTotal);
            }
            return totalWaiting;
        }


        private void ProcessTermUsage()
        {
            try
            {
                using (new PerformanceScope("CollectionData.Report.Total"))
                {
                    ReportManager.Increase(10);
                    Dictionary<string, int> termIdAndRelatedCount = new Dictionary<string, int>();
                    string sql = "SELECT c.termId, COUNT(1) AS termcount FROM items c where c.recordStatus = 1 and c.termId != '00000000-0000-0000-0000-000000000000' and c.sourceFlag=2 and c.nodeType=2200 GROUP BY c.termId";
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
                            termUsageDatas.Add(new RMTermUsage() { TermName = currentTerm.Name, TermId = temId, Size = temp.Value, TermPath = termPath, SourceFlag = (int)SourceFlag.FileSystem });
                        }
                    }
                    RMTermUsageDao.RemoveAll(SourceFlag.FileSystem);
                    RMTermUsageDao.UpdateTermUsage(termUsageDatas);
                    ReportManager.Increase();
                    ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
                    {
                        Action = "RM_DSB_FSMostUsedTerms",
                        Status = JobDetailsStatus.Successful,
                    });
                }
            }
            catch (Exception ex)
            {
                failedCount++;
                ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
                {
                    Action = "RM_DSB_FSMostUsedTerms",
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message
                });
                logger.Error("process fs term usage error:{0}", ex.ToString());
            }
        }
        private void ProcessTotal(long totalWaitingCount)
        {
            try
            {
                ReportManager.Increase(10);
                using (new PerformanceScope("CollectionData.Report.Total"))
                {
                    string sql = "SELECT VALUE COUNT(1) FROM c where (c.sourceFlag=2 and c.nodeType=2200 and c.recordStatus=1)";
                    var totalCreatedCount = ExplorerDao.QueryCount(sql,null);
                    var dataOfDays = lineDataDic.Values.ToList();
                    var totalDestroyedCount = dataOfDays.Sum(a => a.Destroyed);
                    BoardTotalDao.AddOrUpdate(new BoardTotal() { CollectionTime = JobStartTime.Ticks, WaitingTotal = totalWaitingCount, CreatedTotal = totalCreatedCount, DestroyedTotal = totalDestroyedCount, SourceFlag = (int)SourceFlag.FileSystem });
                    ReportManager.Increase();
                    ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
                    {
                        Action = "RM_DSB_FSRecordCount",
                        Status = JobDetailsStatus.Successful,
                    });
                }
            }
            catch (Exception ex)
            {
                failedCount++;
                ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
                {
                    Action = "RM_DSB_FSRecordCount",
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message,
                });
                logger.Error("process total count error:{0}", ex.ToString());
            }

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

        private void CheckHoldStatus()
        {
            try
            {
                ReportManager.Increase(10);
                var utcNow = DateTime.UtcNow.Ticks;
                logger.Info("start to update record hold expired, utcNow:{0}.", utcNow);
                List<Guid> expiredIds = ExplorerDao.UpdateExpiredHeldRecords();
                ReportManager.Increase();
                ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
                {
                    Action = "RM_DSB_CheckHoldStatus",
                    Status = JobDetailsStatus.Successful,
                });
                logger.Info("record hold expired success.");
            }
            catch (Exception ex)
            {
                failedCount++;
                ReportManager.SendJobDetail(new JMFSDashBoardJobDetail()
                {
                    Action = "RM_DSB_CheckHoldStatus",
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message,
                });
                logger.Error("check hold status error {0}", ex.ToString());
            }
        }

    }
}
