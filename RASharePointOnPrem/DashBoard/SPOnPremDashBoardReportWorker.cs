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
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RASharePointOnPrem.DashBoard
{
    public class SPOnPremDashBoardReportWorker
    {

        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;

        private static readonly IExplorerDao ExplorerDao = new ExplorerDao(true);

        private static readonly IAccountDao AccountDao = (IAccountDao)PlatformWindsorManager.GetService(typeof(IAccountDao));


        private static readonly IRMDataOfDayDao RMDataOfDayDao = (IRMDataOfDayDao)PlatformWindsorManager.GetService(typeof(IRMDataOfDayDao));

        private static readonly IRMTermUsageDao RMTermUsageDao = (IRMTermUsageDao)PlatformWindsorManager.GetService(typeof(IRMTermUsageDao));

        private static readonly ITermDao TermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));

        private static readonly IReportCollectionService ReportCollectionService = (IReportCollectionService)PlatformWindsorManager.GetService(typeof(IReportCollectionService));

        private static readonly IBoardTotalDao BoardTotalDao = (IBoardTotalDao)PlatformWindsorManager.GetService(typeof(IBoardTotalDao));

        private static readonly IRMManualApproveDao ManualApproveDao = (IRMManualApproveDao)PlatformWindsorManager.GetService(typeof(IRMManualApproveDao));

        private static readonly IRMSiteCollectionSizeDao RMSiteCollectionSizeDao = (IRMSiteCollectionSizeDao)PlatformWindsorManager.GetService(typeof(IRMSiteCollectionSizeDao));

        public static readonly IGeneralSettingService GeneralSettingService = (IGeneralSettingService)PlatformWindsorManager.GetService(typeof(IGeneralSettingService));

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SPOnPremDashBoardReportWorker));

        private readonly Dictionary<string, RMDataOfDay> LineDataDic = new Dictionary<string, RMDataOfDay>();

        private readonly DateTime JobStartTime;

        private int FailedCount = 0;

        public SPOnPremDashBoardReportWorker(string jobId)
        {
            JobStartTime = DateTime.UtcNow;
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.SPOnPremDashBoard);
        }

        public async Task RunCollectionNowAsync()
        {
            Logger.Info("Begin collect sharepoint on-prem data");
            long totalWaitingCount = 0;
            try
            {
                ReportManager.Increase(1);
                ReportManager.StartUpdateJobProgress();
                var hasNext = true;
                var pageIndex = string.Empty;
                var datas = new List<Record>();
                while(hasNext)
                {
                    var result = ExplorerDao.QueryByPage(e => e.SourceFlag == (int)SourceFlag.SharePointOnPrem && (e.NodeType == 500 || e.NodeType == 531) && e.RecordStatus != (int)RMRecordStatus.Moved, RecordsConstants.ExplorerQueryPageSize, pageIndex);
                    hasNext = !string.IsNullOrEmpty(result.Item2);
                    pageIndex = result.Item2;
                    datas = result.Item1.ToList();
                    ProcessCreationAndDestroy(datas);
                }
                ReportManager.SendJobDetail(new JMSPOnPremDashBoardJobDetail
                {
                    Action = "RM_DSB_DestroyedRecords",
                    Status = JobDetailsStatus.Successful,
                });
                ReportManager.SendJobDetail(new JMSPOnPremDashBoardJobDetail
                {
                    Action = "RM_DSB_CreatedRecords",
                    Status = JobDetailsStatus.Successful,
                });
                totalWaitingCount = await ProcessFullWaitingForApprovalNewAsync();
                ProcessTotal(totalWaitingCount);
                ProcessTermUsage();
                ProcessSiteCollectionUsage();
                CheckHoldStatus();
                if (FailedCount > 0)
                {
                    ReportManager.SetJobFinished(JobStatus.FinishWithException);
                }
                else
                {
                    ReportManager.SetJobFinished(JobStatus.Finished);
                }
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while run sharepoint on-prem dashboard job. Error: {e}");
                ReportManager.SetJobFinished(JobStatus.Failed, e.Message.ToString());
            }
        }

        private void ProcessSiteCollectionUsage()
        {
            var sql = "SELECT c.aveSiteId, COUNT(1) AS siteUsageCount FROM items c where c.recordStatus = 1 and c.sourceFlag=5 and (c.nodeType=500 or c.nodeType=531) GROUP BY c.aveSiteId";
            var siteCollectionUsageDic = ExplorerDao.QuerySiteCollectionUsageCount(sql).OrderByDescending(a => a.Value).Take(10);
            RMSiteCollectionSizeDao.RemoveAll((int)SourceFlag.SharePointOnPrem);
            foreach (var data in siteCollectionUsageDic)
            {
                var site = SharePointOnPremClient.GetLocalSiteCollectionById(data.Key);
                if(site == null)
                {
                    Logger.Warn($"Can't find site collection by id: [{data.Key}].");
                    continue;
                }
                RMSiteCollectionSizeDao.Create(new RMSiteCollectionSize
                {
                    Title = site.Name,
                    SiteUrl = site.Url,
                    ScopeId = new Guid(site.SPObjectId),
                    Size = data.Value,
                    SourceFlag = (int)SourceFlag.SharePointOnPrem
                });
            }
        }

        private void ProcessTermUsage()
        {
            try
            {
                using (new PerformanceScope("CollectionData.Report.Total"))
                {
                    ReportManager.Increase(10);
                    Dictionary<string, int> termIdAndRelatedCount = new Dictionary<string, int>();
                    string sql = "SELECT c.termId, COUNT(1) AS termcount FROM items c where c.recordStatus = 1 and c.termId != '00000000-0000-0000-0000-000000000000' and c.sourceFlag=5 and (c.nodeType=500 or c.nodeType=531) GROUP BY c.termId";
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
                            termUsageDatas.Add(new RMTermUsage() { TermName = currentTerm.Name, TermId = temId, Size = temp.Value, TermPath = termPath, SourceFlag = (int)SourceFlag.SharePointOnPrem });
                        }
                    }
                    RMTermUsageDao.RemoveAll(SourceFlag.SharePointOnPrem);
                    RMTermUsageDao.UpdateTermUsage(termUsageDatas);
                    ReportManager.Increase();
                    ReportManager.SendJobDetail(new JMSPOnPremDashBoardJobDetail()
                    {
                        Action = "RM_DSB_FSMostUsedTerms",
                        Status = JobDetailsStatus.Successful,
                    });
                }
            }
            catch (Exception ex)
            {
                FailedCount++;
                ReportManager.SendJobDetail(new JMSPOnPremDashBoardJobDetail()
                {
                    Action = "RM_DSB_FSMostUsedTerms",
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message
                });
                Logger.Error("process fs term usage error:{0}", ex.ToString());
            }
        }

        private void CheckHoldStatus()
        {
            try
            {
                ReportManager.Increase(10);
                var utcNow = DateTime.UtcNow.Ticks;
                Logger.Info("start to update record hold expired, utcNow:{0}.", utcNow);
                List<Guid> expiredIds = ExplorerDao.UpdateExpiredHeldRecords();
                ReportManager.Increase();
                ReportManager.SendJobDetail(new JMSPOnPremDashBoardJobDetail()
                {
                    Action = "RM_DSB_CheckHoldStatus",
                    Status = JobDetailsStatus.Successful,
                });
                Logger.Info("record hold expired success.");
            }
            catch (Exception ex)
            {
                FailedCount++;
                ReportManager.SendJobDetail(new JMSPOnPremDashBoardJobDetail()
                {
                    Action = "RM_DSB_CheckHoldStatus",
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message,
                });
                Logger.Error("check hold status error {0}", ex.ToString());
            }
        }

        private void ProcessTotal(long totalWaitingCount)
        {
            try
            {
                ReportManager.Increase(10);
                using (new PerformanceScope("CollectionData.Report.Total"))
                {
                    string sql = "SELECT VALUE COUNT(1) FROM c where (c.sourceFlag=5 and (c.nodeType=500 or c.nodeType=531) and c.recordStatus=1)";
                    var totalCreatedCount = ExplorerDao.QueryCount(sql, null);
                    var dataOfDays = LineDataDic.Values.ToList();
                    var totalDestroyedCount = dataOfDays.Sum(a => a.Destroyed);
                    BoardTotalDao.AddOrUpdate(new BoardTotal() { CollectionTime = JobStartTime.Ticks, WaitingTotal = totalWaitingCount, CreatedTotal = totalCreatedCount, DestroyedTotal = totalDestroyedCount, SourceFlag = (int)SourceFlag.SharePointOnPrem });
                    ReportManager.Increase();
                    ReportManager.SendJobDetail(new JMSPOnPremDashBoardJobDetail()
                    {
                        Action = "RM_DSB_FSRecordCount",
                        Status = JobDetailsStatus.Successful,
                    });
                }
            }
            catch (Exception ex)
            {
                FailedCount++;
                ReportManager.SendJobDetail(new JMSPOnPremDashBoardJobDetail()
                {
                    Action = "RM_DSB_FSRecordCount",
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message,
                });
                Logger.Error("process total count error:{0}", ex.ToString());
            }

        }

        private void ProcessCreationAndDestroy(List<Record> datas)
        {
            try
            {
                ReportManager.Increase(1);
                var spOnPremDestoryedResult = datas.Where(d => d.RecordStatus == (int)RMRecordStatus.Destroyed).Select(t => t.DestroyedTime).GroupBy(t => ConvertToShortTimeAsync(t).Result).Select(t => new { key = t.Key, value = t.Count() }).ToList();
                foreach(var entity in spOnPremDestoryedResult)
                {
                    long ticks = ConvertDateTimeToTicks(entity.key);
                    var data = new RMDataOfDay
                    {
                        Dater = ticks,
                        Destroyed = entity.value,
                        Timestamp = entity.key,
                        SourceFlag = (int)SourceFlag.SharePointOnPrem
                    };
                    if(LineDataDic.ContainsKey(entity.key))
                    {
                        LineDataDic[entity.key].Destroyed += entity.value;
                    }
                    else
                    {
                        LineDataDic.Add(entity.key, data);
                    }
                }
            }
            catch(Exception e)
            {
                FailedCount++;
                ReportManager.SendJobDetail(new JMSPOnPremDashBoardJobDetail
                {
                    Action = "RM_DSB_DestroyedRecords",
                    Status = JobDetailsStatus.Failed,
                    Comment = e.Message
                });
                Logger.Error($"An error occurred while get sharepoint on-prem destory records. Error: {e}");
            }

            try
            {
                var createdRecords = datas.GroupBy(d => ConvertToShortTimeAsync(d.TimeCreated).Result).ToList();
                foreach(var entity in createdRecords)
                {
                    var ticks = ConvertDateTimeToTicks(entity.Key);
                    var data = new RMDataOfDay
                    {
                        Dater = ticks,
                        Created = Convert.ToInt64(entity.Count()),
                        Timestamp = entity.Key,
                        SourceFlag = (int)SourceFlag.SharePointOnPrem
                    };
                    if (LineDataDic.ContainsKey(entity.Key))
                    {
                        LineDataDic[entity.Key].Created += Convert.ToInt64(entity.Count());
                    }
                    else
                    {
                        LineDataDic.Add(entity.Key, data);
                    }
                }
                ReportManager.Increase();
            }
            catch(Exception e)
            {
                FailedCount++;
                ReportManager.SendJobDetail(new JMSPOnPremDashBoardJobDetail
                {
                    Action = "RM_DSB_CreatedRecords",
                    Status = JobDetailsStatus.Failed,
                    Comment = e.Message,
                });
                Logger.Error($"An error occurred while get sharepoint on-prem created records. Error: {e}");
            }
        }

        private async Task<int> ProcessFullWaitingForApprovalNewAsync()
        {
            try
            {
                ReportManager.Increase(10);
                Logger.Info($"Start process full waiting for approval new.");
                ProcessWaitingDataOfDateNew();
                await ProcessWaitAssignerNewAsync();
                return ProcessWaitingTotal();
            }
            catch(Exception e)
            {
                FailedCount++;
                Logger.Error($"An error occurred while sharepoint on-prem process full waiting for approval. Error: {e}");
            }
            return 0;
        }

        private int ProcessWaitingTotal()
        {
            Logger.Info("Start to process total waiting count");
            int totalWaiting = ManualApproveDao.GetWaitingCount(SourceFlag.SharePointOnPrem);
            var currentTotal = BoardTotalDao.FindWithNewContext(s => s.SourceFlag == (int)SourceFlag.SharePointOnPrem);
            Logger.Info("New total waiting for approve data count {0}, original count {1}", totalWaiting, currentTotal == null ? 0 : currentTotal.WaitingTotal);
            if (currentTotal != null)
            {
                currentTotal.WaitingTotal = totalWaiting;
                BoardTotalDao.AddOrUpdate(currentTotal);
            }
            return totalWaiting;
        }

        private async Task ProcessWaitAssignerNewAsync()
        {
            Logger.Info("Start to process top 10 waiting assigner");
            //"1, 99"
            Dictionary<string, int> userData = new Dictionary<string, int>();
            //原有的非WorkFlow计算方法
            var index = 1;
            var pageSize = 2000;
            var totalCount = 0;
            List<string> owners = ManualApproveDao.GetOwnerExceptWorkflow(index, pageSize, ref totalCount);
            Logger.Info("Query esclate to row count {0}, total count {1}", owners.Count, totalCount);
            AnalyzeOwnerStr(owners, userData);
            while (totalCount - index * pageSize > 0)
            {
                owners = ManualApproveDao.GetOwnerExceptWorkflow(index, pageSize, ref totalCount);
                Logger.Info("Query esclate to row count {0}, total count {1}", owners.Count, totalCount);
                AnalyzeOwnerStr(owners, userData);
            }
            owners = null;
            Logger.Info("Original without workflow, user data count {0}", userData.Count);
            //workflow的计算方法 
            try
            {
                //这里的Key是Guid
                Dictionary<string, int> workflowDic = ManualApproveDao.GetUserAndWaitingReviewCountMapping();
                Logger.Info("manual approve workflow user data count {0}", workflowDic.Count);
                if (workflowDic.Count > 0)
                {
                    List<string> allUserIds = workflowDic.OrderByDescending(a => a.Value).Select(s => s.Key).ToList();
                    List<RMAccount> accounts = await AccountDao.GetUserByUserIdsAsync(allUserIds);
                    AnalyzeUserInManual(accounts, workflowDic, userData);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Get waiting for approval data of date records error {0}", e.ToString());
                throw e;
            }
            Logger.Info("total mixed user data count {0}", userData.Count);
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
            var recordOwners = await AccountDao.GetUserByIdsAsync(userIds);
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

        private void ProcessWaitingDataOfDateNew()
        {
            var index = 1;
            var pageSize = 10000;
            var totalCount = 0;
            var waitForApprovalTimeDic = new Dictionary<long, int>();

            try
            {
                var tempDatas = ManualApproveDao.GetAllCollectionTime(index, pageSize, ref totalCount, SourceFlag.SharePointOnPrem);
                Logger.Info($"Query sharepoint on-prem collection time row count: [{tempDatas.Count}], total count: [{totalCount}].");
                var tempDic = tempDatas.GroupBy(a => ConvertToShortTimeAsync(a).Result).ToDictionary(o => ConvertDateTimeToTicks(o.Key), p => p.Count());
                AddTempDic2Total(tempDic, waitForApprovalTimeDic);
                while(totalCount - index * pageSize > 0)
                {
                    index++;
                    List<long> manualItems = ManualApproveDao.GetAllCollectionTime(index, pageSize, ref totalCount, SourceFlag.SharePointOnPrem);
                    Logger.Info($"Query sharepoint on-prem collection time row count: [{tempDatas.Count}], total count: [{totalCount}].");
                    tempDic = manualItems.GroupBy(a => ConvertToShortTimeAsync(a).Result).ToDictionary(o => ConvertDateTimeToTicks(o.Key), p => p.Count());
                    AddTempDic2Total(tempDic, waitForApprovalTimeDic);
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            Logger.Info($"Total data with data count: [{waitForApprovalTimeDic.Count}]");
            ProcessWaitingTimeLine(waitForApprovalTimeDic);
        }

        private void ProcessWaitingTimeLine(Dictionary<long, int> tempWaitForApprovalTimeDic)
        {
            try
            {
                var dataOfDays = LineDataDic.Values.ToList();
                RMDataOfDayDao.RemoveAll(SourceFlag.SharePointOnPrem);
                RMDataOfDayDao.AddOrUpdateDatas(dataOfDays);
                //Wating Approval Count
                foreach (var timeCountPair in tempWaitForApprovalTimeDic)
                {
                    var currentDataOfDay = RMDataOfDayDao.FindWithNewContext(s => s.Dater == timeCountPair.Key && s.SourceFlag == (int)SourceFlag.SharePointOnPrem);
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
                            SourceFlag = (int)SourceFlag.SharePointOnPrem
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
                            SourceFlag = (int)SourceFlag.SharePointOnPrem
                        });
                    }
                }
                ReportManager.SendJobDetail(new JMSPOnPremDashBoardJobDetail()
                {
                    Action = "RM_DSB_RecordWaiting",
                    Status = JobDetailsStatus.Successful,
                });
            }
            catch (Exception ex)
            {
                FailedCount++;
                Logger.Error("Get waiting for approval data of date records error {0}", ex.ToString());
                ReportManager.SendJobDetail(new JMSPOnPremDashBoardJobDetail()
                {
                    Action = "RM_DSB_RecordWaiting",
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message
                });
            }
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

        private async Task<string> ConvertToShortTimeAsync(long ticks)
        {
            return (await GeneralSettingService.ConvertTiksToDateTimeAsync(ticks, true)).DataTime.ToString("d");
        }

        private long ConvertDateTimeToTicks(string timeString)
        {
            return Convert.ToDateTime(timeString).Ticks;
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
    }
}
