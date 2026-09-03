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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Common;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ExchangeBackupUtility.Graph;
using ExchangeFolder = ExchangeBackupUtility.ExchangeFolder;
using ExchangeItem = ExchangeBackupUtility.ExchangeItem;
using ExchangeItemBulkHelper = ExchangeBackupUtility.ExchangeItemBulkHelper;

namespace AvePoint.RA.RAExchange.Report
{
    public class EXOTermUsageReportProcessor : EXOReportProcessor
    {
        private Dictionary<Guid, RMTermIdentity> mUsageTermInfo;
        private List<Guid> mUsageTermIds;
        private IExplorerDao ExplorerDao = new ExplorerDao();
        private bool isOrphanedTermReport;
        private bool mIsRetiredTermReport;

        protected override bool IsGroupItems => true;

        public EXOTermUsageReportProcessor(string jobId, string profileId, bool IsOrphanedTermReport, bool isRetiredTermReport)
            : base(jobId, (int)JobType.EXOTermUsageReport, IsOrphanedTermReport)
        {
            RMProfileDto profile = ReportService.GetProfileByIdForReportJob(profileId);
            isOrphanedTermReport = IsOrphanedTermReport;
            mIsRetiredTermReport = isRetiredTermReport;
            if (IsOrphanedTermReport)
            {
                mUsageTermInfo = ReportService.GetOrphanedTermsOfRMAsync().Result;
            }
            else if (isRetiredTermReport)
            {
                mUsageTermInfo = ReportService.GetRetiredTermsOfRMAsync().Result;
            }
            else
            {
                mUsageTermInfo = ReportService.GetTermIDsFromBCSTermTreeAsync(profile.Extension1).Result;
            }
            mUsageTermIds = mUsageTermInfo.Select(_ => _.Key).ToList();
            SendJobReportSummary();
        }

        public override bool CheckRunReportJobIsPrepared(out string message)
        {
            if (mUsageTermInfo == null || mUsageTermInfo.Count == 0)
            {
                message = "RM_RC_TUR_NoTermForReport";
                return false;
            }
            else
            {
                return base.CheckRunReportJobIsPrepared(out message);
            }
        }

        private void SendJobReportSummary()
        {
            List<JMJobDetails> details = new List<JMJobDetails>();
            foreach (var term in mUsageTermInfo.Values)
            {
                details.Add(new JMTermSelection()
                {
                    Term = term.Name,
                    TermFullPath = term.FullPath
                });
            }
            ReportManager.BatchSendJobDetail(details);
        }

        protected override void ProcessItem(ExchangeItem item)
        {
            using (PerformanceScope scope = new PerformanceScope("RAExchangeTermUsageReportProcessor.ProcessItem"))
            {
                mLog.Info("Process Item: {0}.", item.ItemId);
                BCSTermUsageReport report = new BCSTermUsageReport();
                var isAddReport = true;
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        report.TitleOrName = item.ItemName;
                        report.Url = mCachedNodeNameForPath + item.ItemPath + "_" + item.SendDateUTC.ToString("R");
                        report.ObjectLevel = (int)NodeLevel.ExchangeOnlineItem;
                        report.CreatedBy = item.Sender;
                        report.CreatedTime = item.Created.Ticks;
                        report.LastModifiedBy = item.ModifiedBy;
                        report.LastModifiedTime = item.Modified.Ticks;
                        report.SPWebTimeZoneName = "";

                        Guid termId = Guid.Empty;
                        if (GetSingleTaxonomyFieldValue(item, out termId))
                        {
                            report.BCSTermId = termId.ToString();
                            if (mUsageTermInfo.ContainsKey(termId))
                            {
                                report.TermStatus = mUsageTermInfo[termId].Status;
                                report.BCSTermName = mUsageTermInfo[termId].Name;
                                report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                            }
                            else
                            {
                                mLog.Debug("Skip add Report Item {0}, Unknown term id {1}.", item.ItemId, termId.ToString());
                                isAddReport = false;
                            }
                        }
                        else
                        {
                            mLog.Debug("Skip add Report Item {0}, no valid term id.", item.ItemId);
                            isAddReport = false;
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    mLog.Warn("Report item failed. item url: {0}, Error message: {1}.", item.ItemId, ex.ToString());
                }
                finally
                {
                    if (!CheckJobStatusUtility.isStopping)
                    {
                        if (isAddReport)
                        {
                            mLog.Debug("Add Report Item: {0}.", item.ItemId);
                            ReportManager.SendJobReport(report);
                            //SendJobReportDetails(item, JobDetailsStatus.Successful, "");
                        }
                        else
                        {
                            //SendJobReportDetails(item, JobDetailsStatus.Skipped, ".Item not matched.");
                        }
                    }
                }
            }
        }

        protected override void ProcessItem(IExchangeItem item)
        {
            throw new NotImplementedException();
        }

        protected override void ProcessFolder(ExchangeFolder folder)
        {
            using (PerformanceScope scope = new PerformanceScope("RAExchangeReportProcessor.ProcessFolder"))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        var childFolders = GetFolders(folder);
                        if (childFolders != null && childFolders.Count > 0)
                        {
                            ReportManager.IncreaseBase(childFolders.Count);
                            foreach (var mFolder in childFolders)
                            {
                                ReportManager.Increase();
                                ProcessFolder(mFolder);
                            }
                        }

                        var total = HanderItemsUnderFolderInCosmosDB(folder);
                        SendJobReportDetails(folder, total > 0 ? JobDetailsStatus.Successful : JobDetailsStatus.Skipped, "");
                        if(total < 1)
                        {
                            mLog.Info("No items under current mailbox folder. Folder url: {0}.", folder.DisplayFolderPath ?? string.Empty);
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    mJobHasException = true;
                    SendJobReportDetails(folder, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                    mLog.Error("An error occurred while prosess mail box, fullPath is :{0}, error message: {1}.", folder.DisplayFolderPath, e.ToString());
                }
                finally
                {
                    cachedMailTermMapping.Clear();
                }
            }
        }

        protected override void ProcessFolder(IExchangeFolder folder)
        {
            mLog.Info($"GraphFolder {folder.DisplayFolderPath} processing started");

            using (PerformanceScope scope = new PerformanceScope("RAExchangeReportProcessor.ProcessFolder"))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        var childFolders = GetFolders(folder);
                        if (childFolders != null && childFolders.Count > 0)
                        {
                            ReportManager.IncreaseBase(childFolders.Count);
                            foreach (var childFolder in childFolders)
                            {
                                ReportManager.Increase();
                                ProcessFolder(childFolder);
                            }
                        }

                        var total = HanderItemsUnderFolderInCosmosDB(folder);
                        SendJobReportDetails(folder, total > 0 ? JobDetailsStatus.Successful : JobDetailsStatus.Skipped, "");
                        if (total < 1)
                        {
                            mLog.Info("No items under current mailbox folder. Folder url: {0}.", folder.DisplayFolderPath ?? string.Empty);
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    mJobHasException = true;
                    SendJobReportDetails(folder, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                    mLog.Error("An error occurred while prosess mail box, fullPath is :{0}, error message: {1}.", folder.DisplayFolderPath, e.ToString());
                }
                finally
                {
                    cachedMailTermMapping.Clear();
                }
            }
        }

        protected int HanderItemsUnderFolderInCosmosDB(IExchangeFolder folder)
        {
            mLog.Info($"GraphFolder {folder.DisplayFolderPath}, folder id {folder.FolderId.ToMd5()}");
            Guid folderId = folder.FolderId.ToMd5();
            int pageSize = 1000;
            string pageIndex = string.Empty;
            Tuple<IEnumerable<Record>, string> queryResult = null;
            var total = 0;
            do
            {
                queryResult = ExplorerDao.QueryByPage(record => record.ContainerId == ContainerId
                  && record.ScopeId == aosMailboxId && record.FolderId == folderId && mUsageTermIds.Contains(record.TermId)
                  && record.RecordStatus == (int)RMRecordStatus.Active && record.SourceFlag == (int)SourceFlag.Exchange && record.NodeType == (int)NodeLevel.ExchangeOnlineItem, pageSize, pageIndex);
                total += ProcessItems(queryResult.Item1);
                pageIndex = queryResult.Item2;
            } while (!string.IsNullOrEmpty(pageIndex));
            return total;
        }

        protected int HanderItemsUnderFolderInCosmosDB(ExchangeFolder folder)
        {
            mLog.Info($"folder id {folder.FolderId.ToMd5()}");
            Guid folderId = folder.FolderId.ToMd5();
            int pageSize = 1000;
            string pageIndex = string.Empty;
            Tuple<IEnumerable<Record>, string> queryResult = null;
            var total = 0;
            do
            {
                queryResult = ExplorerDao.QueryByPage(_ => _.ContainerId == ContainerId
                  && _.ScopeId == aosMailboxId && _.FolderId == folderId && mUsageTermIds.Contains(_.TermId)
                  && _.RecordStatus == (int)RMRecordStatus.Active && _.SourceFlag == (int)SourceFlag.Exchange && _.NodeType == (int)NodeLevel.ExchangeOnlineItem, pageSize, pageIndex);
                total += ProcessItems(queryResult.Item1);
                pageIndex = queryResult.Item2;
            } while (!string.IsNullOrEmpty(pageIndex));
            return total;
        }

        private int ProcessItems(IEnumerable<Record> items)
        {
            var total = 0;
            foreach(var item in items)
            {
                total += ProcessItem(item);
            }
            return total;
        }

        private int ProcessItem(Record item)
        {
            int result = 1;
            using (PerformanceScope scope = new PerformanceScope("RAExchangeTermUsageReportProcessor.ProcessItem"))
            {
                mLog.Info("Process Item: {0}.", item.ItemId);
                BCSTermUsageReport report = new BCSTermUsageReport();
                var isAddReport = true;
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        report.TitleOrName = item.LeafName;
                        report.Url = MailboxAddress + item.DirPath + "_" + (new DateTime(item.TimeCreated)).ToString("R");
                        report.ObjectLevel = (int)NodeLevel.ExchangeOnlineItem;
                        report.CreatedBy = item.CreatedBy;
                        report.CreatedTime = item.TimeCreated;
                        report.LastModifiedBy = item.ModifiedBy;
                        report.LastModifiedTime = item.TimeModified;
                        report.SPWebTimeZoneName = "";

                        Guid termId = Guid.Empty;
                        termId = item.TermId;
                        report.BCSTermId = termId.ToString();
                        if (mUsageTermInfo.ContainsKey(termId))
                        {
                            report.TermStatus = mUsageTermInfo[termId].Status;
                            report.BCSTermName = mUsageTermInfo[termId].Name;
                            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                        }
                        else
                        {
                            mLog.Debug("Skip add Report Item {0}, Unknown term id {1}.", item.ItemId, termId.ToString());
                            isAddReport = false;
                        }
                    }
                    
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    result = 0;
                    mLog.Warn("Report item failed. item url: {0}, Error message: {1}.", item.ItemId, ex.ToString());
                }
                finally
                {
                    if (!CheckJobStatusUtility.isStopping)
                    {
                        if (isAddReport)
                        {
                            mLog.Debug("Add Report Item: {0}.", item.ItemId);
                            ReportManager.SendJobReport(report);
                            //SendJobReportDetails(item, JobDetailsStatus.Successful, "");
                        }
                        else
                        {
                            //SendJobReportDetails(item, JobDetailsStatus.Skipped, ".Item not matched.");
                        }
                    }
                }
            }
            return result;
        }

        protected override void ProcessGroupItems(ExchangeFolder folder, IEnumerable<ExchangeItem> items)
        {
            ExchangeItemBulkHelper bulkHelper = new ExchangeItemBulkHelper(folder, "");
            var def = new Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, Microsoft.Exchange.WebServices.Data.MapiPropertyType.String);
            var sensitivityLabelDef = new Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition(Microsoft.Exchange.WebServices.Data.DefaultExtendedPropertySet.InternetHeaders, "msip_labels", Microsoft.Exchange.WebServices.Data.MapiPropertyType.String);
            bulkHelper.LoadExtendProperties(items, def, sensitivityLabelDef);
            foreach(var item in items)
            {
                ProcessItemV2(item);
            }
        }
        
        protected override void ProcessGroupItems(IExchangeFolder folder, IEnumerable<IExchangeItem> items)
        {
            foreach(var item in items)
            {
                ProcessItemV2(item);
            }
        }

        private void ProcessItemV2(ExchangeItem item)
        {
            using (PerformanceScope scope = new PerformanceScope("RAExchangeTermUsageReportProcessor.ProcessItem"))
            {
                mLog.Info("Process Item: {0}.", item.ItemId);
                BCSTermUsageReport report = new BCSTermUsageReport();
                var isAddReport = true;
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        report.TitleOrName = item.ItemName;
                        report.Url = mCachedNodeNameForPath + item.ItemPath + "_" + item.SendDateUTC.ToString("R");
                        report.ObjectLevel = (int)NodeLevel.ExchangeOnlineItem;
                        report.CreatedBy = item.Sender;
                        report.CreatedTime = item.Created.Ticks;
                        report.LastModifiedBy = item.ModifiedBy;
                        report.LastModifiedTime = item.Modified.Ticks;
                        report.SPWebTimeZoneName = "";

                        Guid termId = Guid.Empty;
                        string value = string.Empty;
                        var idDefinition = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);
                        if (item.TryGetExtendProperties(idDefinition, out value))
                        {
                            termId = new Guid(value);
                            report.BCSTermId = termId.ToString();
                            if (mUsageTermInfo.ContainsKey(termId))
                            {
                                report.TermStatus = mUsageTermInfo[termId].Status;
                                report.BCSTermName = mUsageTermInfo[termId].Name;
                                report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                            }
                            else
                            {
                                mLog.Debug("Skip add Report Item {0}, Unknown term id {1}.", item.ItemId, termId.ToString());
                                isAddReport = false;
                            }
                        }
                        else
                        {
                            mLog.Debug("Skip add Report Item {0}, no valid term id.", item.ItemId);
                            isAddReport = false;
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    mLog.Warn("Report item failed. item url: {0}, Error message: {1}.", item.ItemId, ex.ToString());
                }
                finally
                {
                    if (!CheckJobStatusUtility.isStopping)
                    {
                        if (isAddReport)
                        {
                            mLog.Debug("Add Report Item: {0}.", item.ItemId);
                            ReportManager.SendJobReport(report);
                            //SendJobReportDetails(item, JobDetailsStatus.Successful, "");
                        }
                        else
                        {
                            //SendJobReportDetails(item, JobDetailsStatus.Skipped, ".Item not matched.");
                        }
                    }
                }
            }
        }

        private void ProcessItemV2(IExchangeItem item)
        {
            
        }
    }
}