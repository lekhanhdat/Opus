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

using AvePoint.Hybrid.Utility.AveCommonLogger;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ExplorerSync.Report
{
    public class RMSPDashboardWorker
    {
        private static readonly AveRALogger logger = AveRALogger.GetInstance(typeof(RMSPDashboardWorker));
        private string mJobId;
        private bool hasErrorNode = false;
        //private JobContext jobContext = null;

        private List<GRMTerm> mTerms;
        private List<RMTermUsage> mTermUsages;
        public RMSPDashboardWorker()
        {
           // this.jobContext = jobContext;
        }

        public void ProcessBoardChangedItem(BoardItem item)
        {
            try
            {
                using (var performance = new PerformanceScope($"RMDashboard.ProcessChange"))
                {
                    switch (item.ChangeType)
                    {
                        case BoardChangeType.Modified:
                            ModifiedTermUsages(item);
                            break;
                        case BoardChangeType.Add:
                            AddOrDeleteTermUsages(item, 1);
                            AddOrDeletCollectionCount(item, 1);
                            AddDataOfDays(item, 1);
                            AddTotals(item, 1);
                            break;
                        case BoardChangeType.Delete:
                            TermUsagesForDeleteRecord(item);
                            DeletCollectionCount(item);
                            DeleteDataOfDays(item);
                            DeleteTotals(item);
                            break;
                        default:
                            break;
                    }

                }

            }
            catch (Exception ex)
            {
               // jobContext.HasErrorNode = true;
                logger.Error($"error occurred while process change:{ex.ToString()}");
                //ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                //{
                //    ObjectName = I18NEntity.GetString("Dashboard Statistics"),
                //    FullPath = string.Empty,
                //    Status = JobDetailsStatus.Failed,
                //    Comment = ex.Message
                //});
            }


        }

        //send to API web
        public void ProcessTermChange()
        {
            try
            {
                List<RMTermUsage> termUsages = new List<RMTermUsage>();
                foreach (var item in RMExplorerBoardCache.Instance.TermChangedDic)
                {

                    //var termId = item.Key;
                    //var count = item.Value;
                    //var currentTerm = mTerms.Where(s => s.UniqueId == termId && !s.IsRemoved).FirstOrDefault();
                    //var currentTermUsage = mTermUsages.Where(s => s.TermId == termId && s.SourceFlag == (int)SourceFlag.SharePoint).FirstOrDefault();
                    ////Term存在, 正常获取最新的Term Name， Term Path信息添加或更新到TermUsage表中
                    //if (currentTerm != null)
                    //{
                    //    var tempTermName = currentTerm.Name;
                    //    var tempTermPath = TermDao.GetTermNamePath(currentTerm.Id);
                    //    if (currentTermUsage != null)
                    //    {
                    //        var tempCount = currentTermUsage.Size + count;
                    //        var temp = new RMTermUsage()
                    //        {
                    //            Id = currentTermUsage.Id,
                    //            TermName = tempTermName,
                    //            TermId = item.Key,
                    //            Size = tempCount < 0 ? 0 : tempCount,
                    //            TermPath = tempTermPath,
                    //            SourceFlag = (int)SourceFlag.SharePoint
                    //        };
                    //        termUsages.Add(temp);
                    //        //RMTermUsageDao.UpdateTermUsage(new List<RMTermUsage>() { temp });
                    //        //RMTermUsageDao.Update();
                    //    }
                    //    else
                    //    {
                    //        termUsages.Add(new RMTermUsage()
                    //        {
                    //            TermName = tempTermName,
                    //            TermId = termId,
                    //            Size = count < 0 ? 0 : count,
                    //            TermPath = tempTermPath,
                    //            SourceFlag = (int)SourceFlag.SharePoint
                    //        });                            
                    //    }
                    //}
                    ////Term不存在， 若记录是新建， 则跳过， 若记录是更新， 则清空Size信息;
                    //else
                    //{
                    //    if (currentTermUsage != null)
                    //    {
                    //        RMTermUsageDao.UpdateTermUsage(new List<RMTermUsage>() {new DB.Model.RMTermUsage()
                    //        {
                    //            Id = currentTermUsage.Id,
                    //            TermName = currentTermUsage.TermName,
                    //            TermId = currentTermUsage.TermId,
                    //            Size = 0,
                    //            TermPath = currentTermUsage.TermPath,
                    //            SourceFlag = (int)SourceFlag.SharePoint
                    //        } });
                    //    }
                    //}

                }
            }
            catch (Exception ex)
            {
                //jobContext.HasErrorNode = true;
                logger.Error($"error occurred while process term change:{ex.ToString()}");
                //ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                //{
                //    ObjectName = I18NEntity.GetString("Term Usage"),
                //    FullPath = string.Empty,
                //    Status = JobDetailsStatus.Failed,
                //    Comment = ex.Message
                //});
            }

        }

        public void ProcessCollectionChange()
        {
            try
            {
                foreach (var sc in RMExplorerBoardCache.Instance.CollectionChangedDic)
                {
                    //var scopeId = sc.Key;
                    //var count = sc.Value;
                    //var scopeDic = RMScopeDao.GetExistScopeInfo().ToDictionary(s => s.ScopeId);
                    //if (scopeDic.ContainsKey(scopeId))
                    //{
                    //    var scope = scopeDic[scopeId];
                    //    var currentSiteCollectionSize = RMSiteCollectionSizeDao.FindWithNewContext(s => s.SiteUrl.Equals(scope.FullPath));
                    //    if (currentSiteCollectionSize != null)
                    //    {
                    //        var tempCount = currentSiteCollectionSize.Size + count;
                    //        RMSiteCollectionSizeDao.UpdateSiteCollectionSizes(new RMSiteCollectionSize()
                    //        {
                    //            Id = currentSiteCollectionSize.Id,
                    //            Title = scope.ScopeName,
                    //            SiteUrl = scope.FullPath,
                    //            ScopeId = scope.ScopeId,
                    //            Size = tempCount < 0 ? 0 : tempCount
                    //        });
                    //    }
                    //    else
                    //    {
                    //        RMSiteCollectionSizeDao.Create(new RMSiteCollectionSize()
                    //        {
                    //            Title = scope.ScopeName,
                    //            SiteUrl = scope.FullPath,
                    //            ScopeId = scope.ScopeId,
                    //            Size = count < 0 ? 0 : count
                    //        });
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
               // jobContext.HasErrorNode = true;
                logger.Error($"error occurred while process collection total:{ex.ToString()}");
                //ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                //{
                //    ObjectName = I18NEntity.GetString("Site Collection Count"),
                //    FullPath = string.Empty,
                //    Status = JobDetailsStatus.Failed,
                //    Comment = ex.Message
                //});
            }

        }

        public void ProcessTotalChange()
        {
            try
            {
                //var currentTotal = BoardTotalDao.FindWithNewContext(s => s.SourceFlag == (int)SourceFlag.SharePoint);
                //var temp = new BoardTotal()
                //{
                //    Id = 0,
                //    CreatedTotal = 0,
                //    WaitingTotal = 0,
                //    DestroyedTotal = 0,
                //    CollectionTime = DateTime.UtcNow.Ticks,
                //    SourceFlag = (int)SourceFlag.SharePoint
                //};
                //if (currentTotal != null)
                //{
                //    temp.Id = currentTotal.Id;
                //    temp.CreatedTotal = currentTotal.CreatedTotal;
                //    temp.WaitingTotal = currentTotal.WaitingTotal;
                //    temp.DestroyedTotal = currentTotal.DestroyedTotal;
                //}
                //foreach (var tempTotal in RMExplorerBoardCache.Instance.TotalChangedDic)
                //{
                //    if (tempTotal.Key == BoardRecordStatus.ManagedRecord)
                //    {
                //        temp.CreatedTotal += tempTotal.Value;
                //    }
                //    else if (tempTotal.Key == BoardRecordStatus.Destruction)
                //    {
                //        temp.DestroyedTotal += tempTotal.Value;
                //    }
                //}
                //BoardTotalDao.AddOrUpdate(temp);
            }
            catch (Exception ex)
            {
                //jobContext.HasErrorNode = true;
                //logger.Error($"error occurred while process total:{ex.ToString()}");
                //ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                //{
                //    ObjectName = I18NEntity.GetString("Record Count by Status"),
                //    FullPath = string.Empty,
                //    Status = JobDetailsStatus.Failed,
                //    Comment = ex.Message
                //});
            }

        }
        [Obsolete("use ProcessFullWaitingForApprovalNew")]
        public void ProcessFullWaitingForApproval()
        {
            try
            {
                //var index = 1;
                //var pageSize = 100;
                //var totalCount = 0;
                //int totalWaiting = 0;
                //Dictionary<string, int> userData = new Dictionary<string, int>();
                //Dictionary<long, int> tempWaitForApprovalTimeDic = new Dictionary<long, int>();
                //var tempDatas = ManualApproveDao.GetDatasByPager(index, pageSize, ref totalCount, m => m.SourceFlag == (int)SourceFlag.SharePoint);
                //totalWaiting = tempDatas.Count(a => a.ActionStatus == 0 && a.Status == 1);
                //ProcessWaitingDataOfDate(tempDatas, ref tempWaitForApprovalTimeDic);
                //ProcessWaitAssigner(tempDatas, ref userData);
                //while (totalCount - index * pageSize > 0)
                //{
                //    index++;
                //    var manualItems = ManualApproveDao.GetDatasByPager(index, pageSize, ref totalCount, m => m.SourceFlag == (int)SourceFlag.SharePoint);
                //    totalWaiting += manualItems.Count(a => a.ActionStatus == 0 && a.Status == 1);
                //    ProcessWaitingDataOfDate(manualItems, ref tempWaitForApprovalTimeDic);
                //    ProcessWaitAssigner(tempDatas, ref userData);
                //}
                //ProcessTop10Assigner(userData);
                //ProcessWaitingTimeLine(tempWaitForApprovalTimeDic);
                ////update total waiting
                //var currentTotal = BoardTotalDao.FindWithNewContext(s => s.SourceFlag == (int)SourceFlag.SharePoint);
                //if (currentTotal != null)
                //{
                //    currentTotal.WaitingTotal = totalWaiting;
                //    BoardTotalDao.AddOrUpdate(currentTotal);
                //}
            }
            catch (Exception ex)
            {
                //jobContext.HasErrorNode = true;
                //logger.Error($"error occurred while process waiting approval:{ex.ToString()}");
                //ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                //{
                //    ObjectName = I18NEntity.GetString("Record Dashboard"),
                //    FullPath = I18NEntity.GetString("Record Waiting Count"),
                //    Status = JobDetailsStatus.Failed,
                //    Comment = ex.Message
                //});
            }

        }

        //private void ProcessTop10Assigner(Dictionary<string, int> userData)
        //{
        //    var top9Count = 0;
        //    var userIds = userData.OrderByDescending(u => u.Value).Select(d => int.Parse(d.Key)).ToList();
        //    if (userData.Count > 9)
        //    {
        //        userIds = userIds.Take(9).ToList();

        //    }
        //    Dictionary<string, PieChartDto> ownerMapping = new Dictionary<string, PieChartDto>();
        //    var recordOwners = RecordOwnersDao.GetUserByIds(userIds);
        //    foreach (var owner in recordOwners)
        //    {
        //        if(owner.UserId == null)
        //        {
        //            continue;
        //        }
        //        if (!ownerMapping.ContainsKey(owner.UserId))
        //        {
        //            ownerMapping.Add(owner.UserId, new PieChartDto { name = owner.DisplayName, data = userData[owner.Id.ToString()] });
        //        }
        //        else
        //        {
        //            ownerMapping[owner.UserId].data += userData[owner.Id.ToString()];
        //        }
        //        top9Count += userData[owner.Id.ToString()];
        //    }
        //    var ownerList = ownerMapping.Values.OrderByDescending(o => o.data).ThenBy(o => o.name).ToList();
        //    if (userData.Count > 9)
        //    {
        //        var userTotal = userData.Values.Sum();
        //        ownerList.Add(new PieChartDto { name = I18NEntity.GetString("RM_RC_Audit_Module_Others"), data = userTotal - top9Count });
        //    }
        //    ReportCollectionService.RemoveAllAssignee();
        //    ReportCollectionService.AddApprovalAssigneeData(ownerList);
        //}

        //private void ProcessWaitingDataOfDate(List<RMManualApprove> tempTotalWaitApprovalSPData, ref Dictionary<long, int> tempWaitForApprovalTimeDic)
        //{
        //    try
        //    {
        //        //Full Process Logic for Waiting Approval Count and WaitingApprovalAssignees Table.
        //        //以下数据更新折线, 需要统计Waiting表中某Type下的全部数据

        //        foreach (var tempSPWait in tempTotalWaitApprovalSPData)
        //        {
        //            var dater = ConvertDateTimeToTicks(ConvertToShortTime(tempSPWait.CollectionTime));
        //            if (tempWaitForApprovalTimeDic.Keys.Contains(dater))
        //            {
        //                tempWaitForApprovalTimeDic[dater] = tempWaitForApprovalTimeDic[dater] + 1;
        //            }
        //            else
        //            {
        //                tempWaitForApprovalTimeDic.Add(dater, 1);
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        jobContext.HasErrorNode = true;
        //        logger.Error("process waiting for approval data of date records error {0}", ex.ToString());
        //        ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
        //        {
        //            ObjectName = I18NEntity.GetString("Record Waiting Count"),
        //            FullPath = string.Empty,
        //            Status = JobDetailsStatus.Failed,
        //            Comment = ex.Message
        //        });
        //    }
        //}

        #region New Logic for waiting approve dashboard

        //public void ProcessFullWaitingForApprovalNew()
        //{
        //    try
        //    {
        //        logger.Info("start ProcessFullWaitingForApprovalNew");
        //        ProcessWaitingDataOfDateNew();
        //        ProcessWaitAssignerNew();
        //        //下边计算所有的Waiting数据， 存的时候区分数据源， 显示的时候不区分
        //        ProcessWaitingTotal();
        //    }
        //    catch (Exception ex)
        //    {
        //        jobContext.HasErrorNode = true;
        //        logger.Error($"error occurred while process waiting approval:{ex.ToString()}");
        //        ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
        //        {
        //            ObjectName = I18NEntity.GetString("Record Dashboard"),
        //            FullPath = I18NEntity.GetString("Record Waiting Count"),
        //            Status = JobDetailsStatus.Failed,
        //            Comment = ex.Message
        //        });
        //    }
        //}
        /// <summary>
        /// 按天计算Waiting的数量， 区分数据源
        /// </summary>
        //private void ProcessWaitingDataOfDateNew()
        //{
        //    var index = 1;
        //    var pageSize = 10000;
        //    var totalCount = 0;
        //    Dictionary<long, int> waitForApprovalTimeDic = new Dictionary<long, int>();
        //    try
        //    {
        //        List<long> tempDatas = ManualApproveDao.GetAllCollectionTime(index, pageSize, ref totalCount, SourceFlag.SharePoint);
        //        logger.Info("Query collection time row count {0}, total count {1}", tempDatas.Count, totalCount);
        //        Dictionary<long, int> tempDic = tempDatas.GroupBy(a => new DateTime(a).ToString("d")).ToDictionary(o => ConvertDateTimeToTicks(o.Key), p => p.Count());
        //        AddTempDic2Total(tempDic, waitForApprovalTimeDic);
        //        while (totalCount - index * pageSize > 0)
        //        {
        //            index++;
        //            List<long> manualItems = ManualApproveDao.GetAllCollectionTime(index, pageSize, ref totalCount, SourceFlag.SharePoint);
        //            logger.Info("Query collection time row count {0}, total count {1}", tempDatas.Count, totalCount);
        //            tempDic = manualItems.GroupBy(a => new DateTime(a).ToString("d")).ToDictionary(o => ConvertDateTimeToTicks(o.Key), p => p.Count());
        //            AddTempDic2Total(tempDic, waitForApprovalTimeDic);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        jobContext.HasErrorNode = true;
        //        throw ex;
        //    }
        //    logger.Info("Total date with data count {0}", waitForApprovalTimeDic.Count);
        //    ProcessWaitingTimeLine(waitForApprovalTimeDic);
        //}
        /// <summary>
        /// 按User计算Waiting 的数据，不区分数据源
        /// </summary>
        //private void ProcessWaitAssignerNew()
        //{
        //    logger.Info("Start to process top 10 waiting assigner");
        //    //"1, 99"
        //    Dictionary<string, int> userData = new Dictionary<string, int>();
        //    //原有的非WorkFlow计算方法
        //    var index = 1;
        //    var pageSize = 2000;
        //    var totalCount = 0;
        //    List<string> owners = ManualApproveDao.GetOwnerExceptWorkflow(index, pageSize, ref totalCount);
        //    logger.Info("Query esclate to row count {0}, total count {1}", owners.Count, totalCount);
        //    AnalyzeOwnerStr(owners, userData);
        //    while (totalCount - index * pageSize > 0)
        //    {
        //        owners = ManualApproveDao.GetOwnerExceptWorkflow(index, pageSize, ref totalCount);
        //        logger.Info("Query esclate to row count {0}, total count {1}", owners.Count, totalCount);
        //        AnalyzeOwnerStr(owners, userData);
        //    }
        //    owners = null;
        //    logger.Info("Original without workflow, user data count {0}", userData.Count);
        //    //workflow的计算方法 
        //    try
        //    {
        //        //这里的Key是Guid
        //        Dictionary<string, int> workflowDic = ManualApproveDao.GetUserAndWaitingReviewCountMapping();
        //        logger.Info("manual approve workflow user data count {0}", workflowDic.Count);
        //        if (workflowDic.Count > 0)
        //        {
        //            List<string> allUserIds = workflowDic.OrderByDescending(a => a.Value).Select(s => s.Key).ToList();
        //            List<RMAccount> accounts = AccountDao.GetUserByUserIds(allUserIds);
        //            AnalyzeUserInManual(accounts, workflowDic, userData);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        jobContext.HasErrorNode = true;
        //        logger.Error("Get waiting for approval data of date records error {0}", e.ToString());
        //        throw e;
        //    }
        //    logger.Info("total mixed user data count {0}", userData.Count);
        //    ProcessTop10Assigner(userData);
        //}
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
        //暂时没用上
        //private void ProcessTop10AssignerNew(Dictionary<string, int> top10userData, int totalCount)
        //{
        //    var top9Count = 0;
        //    List<int> userIds = top10userData.Select(d => int.Parse(d.Key)).ToList();
        //    Dictionary<string, PieChartDto> ownerMapping = new Dictionary<string, PieChartDto>();
        //    var recordOwners = RecordOwnersDao.GetUserByIds(userIds);
        //    foreach (var owner in recordOwners)
        //    {
        //        if (!ownerMapping.ContainsKey(owner.UserId))
        //        {
        //            ownerMapping.Add(owner.UserId, new PieChartDto { name = owner.DisplayName, data = top10userData[owner.Id.ToString()] });
        //        }
        //        else
        //        {
        //            ownerMapping[owner.UserId].data += top10userData[owner.Id.ToString()];
        //        }
        //        top9Count += top10userData[owner.Id.ToString()];
        //    }
        //    var ownerList = ownerMapping.Values.OrderByDescending(o => o.data).ThenBy(o => o.name).ToList();
        //    if (totalCount > top9Count)
        //    {
        //        ownerList.Add(new PieChartDto { name = I18NEntity.GetString("RM_RC_Audit_Module_Others"), data = totalCount - top9Count });
        //    }
        //    ReportCollectionService.RemoveAllAssignee();
        //    ReportCollectionService.AddApprovalAssigneeData(ownerList);
        //}
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
        //private void ProcessWaitingTotal()
        //{
        //    logger.Info("Start to process total waiting count");
        //    int totalWaiting = ManualApproveDao.GetWaitingCount(SourceFlag.SharePoint);
        //    var currentTotal = BoardTotalDao.FindWithNewContext(s => s.SourceFlag == (int)SourceFlag.SharePoint);
        //    logger.Info("New total waiting for approve data count {0}, original count {1}", totalWaiting, currentTotal == null ? 0 : currentTotal.WaitingTotal);
        //    if (currentTotal != null)
        //    {
        //        currentTotal.WaitingTotal = totalWaiting;
        //        BoardTotalDao.AddOrUpdate(currentTotal);
        //    }
        //}

        #endregion
        //private void ProcessWaitingTimeLine(Dictionary<long, int> tempWaitForApprovalTimeDic)
        //{
        //    try
        //    {
        //        //Wating Approval Count
        //        foreach (var timeCountPair in tempWaitForApprovalTimeDic)
        //        {
        //            var currentDataOfDay = RMDataOfDayDao.FindWithNewContext(s => s.Dater == timeCountPair.Key && s.SourceFlag == (int)SourceFlag.SharePoint);
        //            if (currentDataOfDay != null)
        //            {
        //                RMDataOfDayDao.AddOrUpdateDatas(new List<RMDataOfDay>(){new RMDataOfDay()
        //                {
        //                    Id = currentDataOfDay.Id,
        //                    Created = currentDataOfDay.Created,
        //                    Destroyed = currentDataOfDay.Destroyed,
        //                    WaitingApproval = timeCountPair.Value,
        //                    Dater = currentDataOfDay.Dater,
        //                    Timestamp = currentDataOfDay.Timestamp,
        //                    SourceFlag = (int)SourceFlag.SharePoint
        //                } });
        //            }
        //            else
        //            {
        //                RMDataOfDayDao.Create(new RMDataOfDay()
        //                {
        //                    Created = 0,
        //                    Destroyed = 0,
        //                    WaitingApproval = timeCountPair.Value,
        //                    Dater = timeCountPair.Key,
        //                    Timestamp = new DateTime(timeCountPair.Key).ToString("d"),
        //                    SourceFlag = (int)SourceFlag.SharePoint
        //                });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        jobContext.HasErrorNode = true;
        //        logger.Error("Get waiting for approval data of date records error {0}", ex.ToString());
        //        ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
        //        {
        //            ObjectName = I18NEntity.GetString("Record Waiting Count"),
        //            FullPath = string.Empty,
        //            Status = JobDetailsStatus.Failed,
        //            Comment = ex.Message
        //        });
        //    }
        //}

        //private void ProcessWaitAssigner(List<RMManualApprove> allDatas, ref Dictionary<string, int> userData)
        //{
        //    try
        //    {
        //        var tempCurrentWaitApprovalAllData = allDatas.Where(a => a.ActionStatus == 0 && a.Status == 1).ToList();
        //        //WaitingApprovalAssignees Table.
        //        logger.Info("waiting approval item count for owner chat: {0}.", tempCurrentWaitApprovalAllData.Count);
        //        foreach (var item in tempCurrentWaitApprovalAllData)
        //        {
        //            string url = string.Empty;
        //            string fileName = string.Empty;

        //            url = item.Url;
        //            fileName = item.LeafName;
        //            logger.Info("process approval fileName:{0}.", fileName);
        //            var reviewUsers = GetApprovalUsers(item);
        //            if(reviewUsers != null)
        //            {
        //                foreach (var userId in reviewUsers)
        //                {
        //                    if (userData.ContainsKey(userId))
        //                    {
        //                        userData[userId] += 1;
        //                    }
        //                    else
        //                    {
        //                        userData.Add(userId, 1);
        //                    }
        //                }
        //            }

        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        jobContext.HasErrorNode = true;
        //        logger.Error("Get waiting for approval of user records error {0}", ex.ToString());
        //        ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
        //        {
        //            ObjectName = I18NEntity.GetString("Record Waiting Count By User"),
        //            FullPath = string.Empty,
        //            Status = JobDetailsStatus.Failed,
        //            Comment = ex.Message
        //        });
        //    }
        //}

        //private List<string> GetApprovalUsers(RMManualApprove data) 
        //{
        //    List<string> userIds = new List<string>();
        //    if (data.WorkflowInstanceId != Guid.Empty)
        //    {
        //        //通过workflow来查user
        //        var uids = WorkflowInstanceDao.GetReviewUserIdsByWFInstanceId(data.WorkflowInstanceId);
        //        var users = AccountDao.GetUserByUserIds(uids);
        //        userIds = users.Select(u => u.Id).ToList().ConvertAll(i => i.ToString());
        //    }
        //    else if (!string.IsNullOrEmpty(data.EscalateTo)) 
        //    {
        //        //通过record owner查user
        //        userIds = data.EscalateTo?.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        //    }
        //    logger.Info($"get review user id count:{userIds?.Count}");
        //    return userIds;
        //}

        public void ProcessDataOfDateChange()
        {
            try
            {
                foreach (var tempDataOfDay in RMExplorerBoardCache.Instance.DataOfDateChangedDic)
                {

                    //foreach (var item in tempDataOfDay.Value)
                    //{
                    //    var date = item.Key;
                    //    var count = item.Value;
                    //    var dater = ConvertDateTimeToTicks(date);
                    //    RMDataOfDay temp = new RMDataOfDay()
                    //    {
                    //        Created = 0,
                    //        Destroyed = 0,
                    //        WaitingApproval = 0,
                    //        Dater = dater,
                    //        Timestamp = new DateTime(dater).ToString("d"),
                    //        SourceFlag = (int)SourceFlag.SharePoint
                    //    };
                    //    var currentDataOfDay = RMDataOfDayDao.FindWithNewContext(s => s.Dater == dater && s.SourceFlag == (int)SourceFlag.SharePoint);
                    //    if (currentDataOfDay != null)
                    //    {
                    //        temp.Id = currentDataOfDay.Id;
                    //        temp.Created = currentDataOfDay.Created;
                    //        temp.Destroyed = currentDataOfDay.Destroyed;
                    //        temp.WaitingApproval = currentDataOfDay.WaitingApproval;
                    //        temp.Created = currentDataOfDay.Created;

                    //    }
                    //    if (tempDataOfDay.Key == BoardRecordStatus.Creation)
                    //    {
                    //        temp.Created += count;
                    //    }
                    //    else if (tempDataOfDay.Key == BoardRecordStatus.Destruction)
                    //    {
                    //        temp.Destroyed += count;
                    //    }
                    //    RMDataOfDayDao.AddOrUpdateDatas(new List<RMDataOfDay>() { temp });

                    //}


                }
            }
            catch (Exception ex)
            {
                //jobContext.HasErrorNode = true;
                //logger.Error("Get data of date records error {0}", ex.ToString());
                //ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                //{
                //    ObjectName = I18NEntity.GetString("Record Count By Status"),
                //    FullPath = string.Empty,
                //    Status = JobDetailsStatus.Failed,
                //    Comment = ex.Message
                //});
            }

        }

        private void AddOrDeleteTermUsages(BoardItem item, long count)
        {

            Guid termId = item.NewRecord.TermId;
            if (!termId.Equals(Guid.Empty))
            {
                RMExplorerBoardCache.Instance.AddTermChange(termId, count);
            }

        }

        private void AddDataOfDays(BoardItem item, long count)
        {
            long createdTime = item.NewRecord.TimeCreated;
            if (createdTime != 0)
            {
                var date = ConvertToShortTime(createdTime);
                RMExplorerBoardCache.Instance.AddDataOfDateChange(BoardRecordStatus.Creation, date, count);
            }

        }

        private void DeleteDataOfDays(BoardItem item)
        {
            if (item.OldRecord != null && item.OldRecord.RecordStatus == (int)RMRecordStatus.Destroyed)
            {
                long destroyedTime = item.OldRecord.DestroyedTime;
                if (destroyedTime != 0)
                {

                    var date = ConvertToShortTime(destroyedTime);
                    RMExplorerBoardCache.Instance.AddDataOfDateChange(BoardRecordStatus.Destruction, date, 1);

                }
            }

        }

        private void AddTotals(BoardItem item, long count)
        {
            RMExplorerBoardCache.Instance.AddTotalChange(BoardRecordStatus.ManagedRecord, count);

        }

        private void DeleteTotals(BoardItem item)
        {
            if (item.OldRecord != null)
            {
                if (item.OldRecord.RecordStatus == (int)RMRecordStatus.Active)
                {
                    RMExplorerBoardCache.Instance.AddTotalChange(BoardRecordStatus.ManagedRecord, -1);
                }
                else if (item.OldRecord.RecordStatus == (int)RMRecordStatus.Destroyed)
                {
                    RMExplorerBoardCache.Instance.AddTotalChange(BoardRecordStatus.ManagedRecord, -1);
                    RMExplorerBoardCache.Instance.AddTotalChange(BoardRecordStatus.Destruction, 1);
                }
            }

        }

        private void AddOrDeletCollectionCount(BoardItem item, long count)
        {

            Guid scopeId = item.NewRecord.ScopeId;

            RMExplorerBoardCache.Instance.AddCollectionChange(scopeId, count);

        }

        private void DeletCollectionCount(BoardItem item)
        {

            if (item.OldRecord != null)
            {
                Guid scopeId = item.OldRecord.ScopeId;
                if (item.OldRecord.RecordStatus == (int)RMRecordStatus.Active || item.OldRecord.RecordStatus == (int)RMRecordStatus.Destroyed)
                {
                    RMExplorerBoardCache.Instance.AddCollectionChange(scopeId, -1);
                }

            }

        }

        private void ModifiedTermUsages(BoardItem item)
        {
            //需要判断TermId是否发生了变化...
            //当前TermId == DB中的TermId, 需要判断本次收集Job时， Record是否存在History Reclassify的操作. 若存在说明有做过Reclassify相关操作, 需要找到原始Term并-1处理, 不存在则不处理
            //若当前TermId != DB中的TermId, 需要到Reclassify操作对应的临时表中找最原始的TermId，并将该TermId的Size - 1, 当前Record关联的TermId的Size + 1, 
            //若Reclassify操作对应的临时表没有该记录相关信息, 则将该Record在Explorer DB中的TermId的Size - 1.
            //找到后删除该Record相关的Reclassify操作记录;

            Guid termId = item.NewRecord.TermId;
            var previousTermId = Guid.Empty;
            if (item.OldRecord != null)
            {
                previousTermId = item.OldRecord.TermId;
            }

            var currentTermId = termId;
            var tempHistories = ClassificationHistoryDao.FindList(d => d.RecordId == item.NewRecord.Id);
            var tempHistory = tempHistories.OrderBy(j => j.OperationTime).FirstOrDefault();
            if (tempHistory != null)
            {
                previousTermId = tempHistory.PreviousTermId;
                //Delete Classification History
                ClassificationHistoryDao.BatchDelete(tempHistories);
            }

            if (previousTermId != currentTermId)
            {
                logger.Debug($"previousTermId:{previousTermId}, currentTermId:{currentTermId}");
                if (previousTermId != Guid.Empty)
                {
                    //Previous Term - 1
                    RMExplorerBoardCache.Instance.AddTermChange(previousTermId, -1);
                }
                if (currentTermId != Guid.Empty)
                {
                    //Current Term + 1
                    RMExplorerBoardCache.Instance.AddTermChange(currentTermId, 1);
                }
            }

        }

        private void TermUsagesForDeleteRecord(BoardItem item)
        {

            //数据经过多次Classify操作后删除，再收集会如何??
            if (item.OldRecord != null)
            {
                Guid termId = item.OldRecord.TermId;

                var previousTermId = Guid.Empty;
                var tempHistories = ClassificationHistoryDao.FindList(d => d.RecordId == item.OldRecord.Id);
                var tempHistory = tempHistories.OrderBy(j => j.OperationTime).FirstOrDefault();
                if (tempHistory != null)
                {
                    previousTermId = tempHistory.PreviousTermId;
                    //Delete Classification History
                    ClassificationHistoryDao.BatchDelete(tempHistories);
                }
                else
                {
                    previousTermId = termId;
                }
                //Previous Term - 1
                RMExplorerBoardCache.Instance.AddTermChange(previousTermId, -1);

            }

        }

        public bool RunNow()
        {
            try
            {
                ProcessTermChange();
                ProcessCollectionChange();
                ProcessDataOfDateChange();
                ProcessTotalChange();

                CheckHoldStatus();
            }
            catch (Exception ex)
            {
                //jobContext.HasErrorNode = true;
                //logger.Error($"error occurred while process board finish:{ex.ToString()}");
                //ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                //{
                //    ObjectName = I18NEntity.GetString("Dashboard Statistics"),
                //    FullPath = string.Empty,
                //    Status = JobDetailsStatus.Failed,
                //    Comment = ex.Message
                //});
            }
            return hasErrorNode;

        }

        private void CheckHoldStatus()
        {
            try
            {
                //var utcNow = DateTime.UtcNow.Ticks;
                //logger.Info("start to update record hold expired, utcNow:{0}.", utcNow);
                //List<Guid> expiredIds = ExplorerDao.UpdateExpiredHeldRecords();
                //RecordAllianceDao.BatchDeleteRecordAllianceByIds(expiredIds);
                //ReportManager.Increase();
                //logger.Info("record hold expired success.");
            }
            catch (Exception ex)
            {
                //jobContext.HasErrorNode = true;
                //ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                //{
                //    ObjectName = "Check Hold Status",
                //    FullPath = string.Empty,
                //    Status = JobDetailsStatus.Failed,
                //    Comment = "Check hold status error",
                //});
                //logger.Error("check hold status error {0}", ex.ToString());
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
    }   
}
