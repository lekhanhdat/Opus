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
using AvePoint.GCommon.GraphAPI;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using ExchangeCommonWrapper;
using PnP.Framework.Diagnostics;
using RAArchiverCommon.TeamsController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public class JobReportImps
    {
        #region Private Fields

        private IRMReportManager mReportManager;
        //最大阶段数
        private int mMaxPhases;
        //当前阶段
        private int mCurrentPhase;
        //当前可达到的最大百分比值
        private int mCurrentMaxpercent;
        //job 状态
        private JobStatus mJobStatus;
        //子阶段增加百分比到一定程度和这个值比较，判断是否可以给主进度条增加百分比，这个值会随着主进度条进度不断提升
        int referenceValue;
        //完成的操作的文件数
        int finishedNum;
        //当前阶段处理的文件数
        private long mBaseCount4Phase;
        // action 要处理的步骤数
        int actionPhases;
        //action

        private ActionStatistics ScanActionStatistics;
        private ActionStatistics BackupActionStatistics;
        private ActionStatistics ExportActionStatistics;
        private ActionStatistics OtherActionStatistics;
        private ActionStatistics RestoreActionStatistics;
        private static readonly object lockObj = new object();

        #endregion

        #region public Fields
        public bool HasCompleteNode { get; set; }
        public bool HasScanCompleteNode { get; set; }
        public bool HasErrorNode { get; set; }
        public bool HasStop { get; set; }
        public string summaryComments { set; get; }
        public string summaryCommentsDetails { set; get; }
        public IRMReportManager ReportManager {get { return mReportManager; } }

        public long CalculateSize { set; get; }
        public bool IsRootNodeError { get; set; }
        #endregion
        public JobReportImps(IRMReportManager ReportManager)
        {
            mReportManager = ReportManager;
            mMaxPhases = 2;
            mCurrentPhase = 1;
            referenceValue = 1;
            finishedNum = 0;
            actionPhases = 1;
        }

        public void SetActionPhases(ActionType x) {
            switch (x) {
                case ActionType.Move:
                case ActionType.DeleteOnly:
                case ActionType.ExportBeforeDelete:
                case ActionType.DeleteDocumentToRecyleBinOnly:
                case ActionType.ExportOnly:
                case ActionType.KeepDataOnly:
                case ActionType.BackupOnly:
                case ActionType.ExportBeforeKeepDataOnly:
                    {
                        actionPhases = 1;
                        break;
                    }
                case ActionType.ArchiverAndRemove:
                case ActionType.ArchiverAndKeepData:
                case ActionType.ExportBeforeArchiver:
                case ActionType.ArchchiveToStorage:
                    {
                        actionPhases = 2;
                        break;
                    }

            }
        }

        /// <summary>
        /// 提升阶段
        /// </summary>
        public void AscendPhase()
        {
            if (mCurrentPhase == mMaxPhases)
            {
                return;
            }
            else
            {
                //提升阶段时无论如何都将总进度置成上个阶段最大进度
                if (mReportManager.GetProgress() < 30)
                {
                    mReportManager.Increase(30 - mReportManager.GetProgress());
                }

                Interlocked.Add(ref mCurrentPhase, 1);
                //还原初始值
                referenceValue = 1;
                finishedNum = 0;
                mBaseCount4Phase = 0;
            }
        }

        public void SetBaseCount4Phase(long count)
        {
            mBaseCount4Phase = count;
        }
        //第二阶段专用方法没如果文件处理发生错误，后续业务不处理，要把进度累加跳过,后面跳过几个步骤传几
        public void UpdateProgress4Error(int x) {
            for (int i = 0; i < x; i++) {
                UpdateProgress(true);
            }
        }

        /// <summary>就
        /// 增加总进度百分比
        /// </summary>
        public void UpdateProgress(bool archiveLevel = true)
        {
            try
            {
                Interlocked.Add(ref finishedNum, 1);
                //最大达到的百分比值,如果是最后一个阶段，可达到的最大百分比值直接设置为100
                if (mCurrentPhase == 1)
                {
                    mCurrentMaxpercent = 29;
                }
                else
                {
                    mCurrentMaxpercent = 99;
                }

                if (archiveLevel)
                {
                    //此处必须保留小数，如果取整要增加的进度将永远为0 result为当前阶段已处理文件数和总文件数的比
                    decimal result = decimal.Round(Convert.ToDecimal(finishedNum) / Convert.ToDecimal(mBaseCount4Phase), 2);
                    //一阶段不需，二阶段根据action要操作的步骤进行变化,需要处理的文件总数要根据action要处理的步骤翻对应倍数
                    decimal actionResult = decimal.Round(Convert.ToDecimal(finishedNum) / Convert.ToDecimal(mBaseCount4Phase * actionPhases), 2);
                    if (mReportManager.GetProgress() < mCurrentMaxpercent)
                    {
                        //此处需要计算，当前阶段是否应该增加百分比，即当前阶段（完成的文件数的百分比）乘当前阶段占总进度百分比 大于上次增加到的百分比+1时，执行总进可以增加对应总进度百分比的整数
                        //referenceValue的初始值是1，会根据已经增加的进度不断增加，但不会超过当前阶段最大百分比
                        //乘以30实际上是乘100在乘0.3的结果
                        if (result * 30 >= referenceValue + 1 && mCurrentPhase == 1)
                        {
                            var risePercentage1 = (int)(result * 30 - (referenceValue + 1));
                            //如果即将增加的百分比值与当前百分之的和小于可允许最大百分比才允许增加，否则直接将百分比置成可允许最大值
                            if (risePercentage1 > 0)
                            {
                                if (risePercentage1 + mReportManager.GetProgress() <= mCurrentMaxpercent)
                                {

                                    mReportManager.Increase(risePercentage1);
                                    Interlocked.Add(ref referenceValue, risePercentage1);
                                }
                                else
                                {
                                    mReportManager.SetProgress(mCurrentMaxpercent);
                                }
                            }
                        }
                        //乘以70实际上是乘100在乘0.7的结果
                        if (actionResult * 70 >= referenceValue + 1 && mCurrentPhase > 1)
                        {
                            int risePercentage2 = (int)(actionResult * 70 - (referenceValue + 1));
                            if (risePercentage2 > 0)
                            {
                                if (risePercentage2 + mReportManager.GetProgress() <= mCurrentMaxpercent)
                                {

                                    mReportManager.Increase(risePercentage2);
                                    Interlocked.Add(ref referenceValue, risePercentage2);
                                }
                                else
                                {
                                    mReportManager.SetProgress(mCurrentMaxpercent);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"UpdateProgress error {e}");
            }
        }

        public void UpdateProgress(int count)
        {
            mReportManager.Increase(count);
        }

        public JobStatus GetJobStatus()
        {
            if (HasScanCompleteNode) TeamsDisposalState.IsSiteHasMatchRule = true;

            if (mReportManager.JobType == JobType.SOPreScan || mReportManager.JobType == JobType.DiscoveryPreScan
                || mReportManager.JobType == JobType.TeamsPreScan
                )
            {
                HasCompleteNode = HasCompleteNode || HasScanCompleteNode;
            }

            if (mReportManager.JobType == JobType.ArchiverRestore || mReportManager.JobType == JobType.TeamsArchiverRestore || mReportManager.JobType == JobType.TeamsOutPlaceRestore)
            {
                HasCompleteNode = HasCompleteNode && !IsRootNodeError;
            }

            if (HasStop || CheckJobStatusUtility.isStopping)
            {
                mJobStatus = JobStatus.Stopped;
                summaryComments = "";
            }
            else if (HasCompleteNode && !HasErrorNode)
            {
                mJobStatus = JobStatus.Finished;
            }
            else if (HasCompleteNode && HasErrorNode)
            {
                mJobStatus = JobStatus.FinishWithException;
            }
            else if (!HasCompleteNode && !HasErrorNode)
            {
                mJobStatus = JobStatus.Finished;
            }
            else if (!HasCompleteNode && HasErrorNode)
            {
                mJobStatus = JobStatus.Failed;
            }
            return mJobStatus;
        }

        private void AnalyzeStatus(JobDetailsStatus status, int actionTab)
        {
            if ((status == JobDetailsStatus.Successful || status == JobDetailsStatus.Skipped))
            {
                if(actionTab == (int)ActionTab.Scan)
                {
                    HasScanCompleteNode = true;
                }
                else
                {
                    HasCompleteNode = true;
                }
            }
            else if (status == JobDetailsStatus.Failed || status == JobDetailsStatus.Exception)
            {
                HasErrorNode = true;
            }
        }

        #region Public Report Functions
        public void AddI18NReport(string url, long nodeSize, JobDetailsStatus status, int cacheNodeType, string subJobID, string ruleName, string mediaName, string key, string defaultValue, params object[] args)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="size">Node Size</param>
        /// <param name="message">Error Message</param>
        /// <param name="status">Backup Succesfully: 1: Error:2</param>
        public void AddReport(string url, long nodeSize, JobDetailsStatus status, int cacheNodeType, string subJobID, string ruleName, string mediaName, string message = "")
        {
            AnalyzeStatus(status, (int)ActionTab.Backup);
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
            mArchiverActionJobDetails.SourceLocation = url;
            mArchiverActionJobDetails.Size = nodeSize.ToString();
            mArchiverActionJobDetails.RuleName = ruleName;
            mArchiverActionJobDetails.Status = status;
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
            mArchiverActionJobDetails.ActionTab = (int)ActionTab.Backup;
            mArchiverActionJobDetails.Comment = message;
            mReportManager.SendJobDetail(mArchiverActionJobDetails);
            AnalyzeBackUpDetailsForSummary(nodeSize, cacheNodeType, status);
        }
        public void M365ArchiveAddReport(string url, long nodeSize, JobDetailsStatus status, int cacheNodeType, string subJobID, string ruleName, string mediaName, string message = "")
        {
            AnalyzeStatus(status, (int)ActionTab.Action);
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
            mArchiverActionJobDetails.SourceLocation = url;
            mArchiverActionJobDetails.Size = nodeSize.ToString();
            mArchiverActionJobDetails.RuleName = ruleName;
            mArchiverActionJobDetails.Status = status;
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
            mArchiverActionJobDetails.ActionTab = (int)ActionTab.Action;
            mArchiverActionJobDetails.Action = "RM_JS_RDM_CreateRule_Options_StoreInM365Archive";
            mArchiverActionJobDetails.Comment = message;
            mReportManager.SendJobDetail(mArchiverActionJobDetails);
            AnalyzeOtherDetailsForSummary(nodeSize, cacheNodeType, status);
            StatisticOtherDetailsDeleteSie(nodeSize, status, "RM_JS_RDM_CreateRule_Options_StoreInM365Archive");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="size">Node Size</param>
        /// <param name="message">Error Message</param>
        /// <param name="status">Backup Succesfully: 1: Error:2</param>
        public void AddVaultReport(string url, long nodeSize, JobDetailsStatus vaultState, int cacheNodeType, string subJobID, string ruleName, string mediaName, string message = "")
        {
            AnalyzeStatus(vaultState, (int)ActionTab.Export);
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
            mArchiverActionJobDetails.SourceLocation = url;
            mArchiverActionJobDetails.Size = nodeSize.ToString();
            mArchiverActionJobDetails.RuleName = ruleName;
            mArchiverActionJobDetails.Status = vaultState;
            mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
            mArchiverActionJobDetails.Action = "";
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.ActionTab = (int)ActionTab.Export;
            mArchiverActionJobDetails.Comment = message;
            mReportManager.SendJobDetail(mArchiverActionJobDetails);
            AnalyzeExportDetailsForSummary(nodeSize, cacheNodeType, vaultState);
        }
        public void AddRestoreReport(string url, long nodeSize, int status, string cacheNodeType, long finishTime,string path, string message = "", int conflictResolution = 0, long startTime = 0, bool isMigrationRestore = false,string pathMd5 = "", string destUrl = "")
        {
            AnalyzeStatus((JobDetailsStatus)status, (int)ActionTab.None);

            var mArchiverActionJobDetails = isMigrationRestore 
                ? new JMMigrationRestoreActionJobDetailes() { StartTime = startTime }
                : new JMRestoreActionJobDetailes();

            mArchiverActionJobDetails.SourceLocation = url;
            mArchiverActionJobDetails.Path = path;
            mArchiverActionJobDetails.Size = nodeSize.ToString();
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.ConflictResolution = conflictResolution;  
            mArchiverActionJobDetails.Status = (JobDetailsStatus)status;
            mArchiverActionJobDetails.Level = JobReportUtility.ConverTypeToLevel(cacheNodeType);
            mArchiverActionJobDetails.Comment = message;
            mArchiverActionJobDetails.PathMd5 = pathMd5;
            mArchiverActionJobDetails.PolicyLevel = cacheNodeType;
            mArchiverActionJobDetails.DestinationUrl = destUrl;
            Logger.Info($"AddRestoreReportt, DestinationUrl: {mArchiverActionJobDetails.DestinationUrl}");

            mReportManager.SendJobDetail(mArchiverActionJobDetails);
            AnalyzeRestoreDetailsForSummary(nodeSize, (int)JobReportUtility.ConverTypeToNodeLevel(cacheNodeType), (JobDetailsStatus)status);
        }
        public void AddVEOMergeReport(string fileName, string sourcePath, string destinationPath, int status, long size, DateTime dateTime, string message = "")
        {
            AnalyzeStatus((JobDetailsStatus)status, (int)ActionTab.None);
            JMVEOMergeJobDetails mVEOMergeJobDetails = new JMVEOMergeJobDetails();
            mVEOMergeJobDetails.FileName = fileName;
            mVEOMergeJobDetails.SourceLocation = sourcePath;
            mVEOMergeJobDetails.DestinationLocation = destinationPath;
            mVEOMergeJobDetails.Status = (JobDetailsStatus)status;
            mVEOMergeJobDetails.Size = size.ToString();
            mVEOMergeJobDetails.FinishTime = dateTime.Ticks;
            mVEOMergeJobDetails.Comment = message;
            mReportManager.SendJobDetail(mVEOMergeJobDetails);
        }
        public void AddGenerateRestoreReport(string url, JobDetailsStatus status,string message = "")
        {
            AnalyzeStatus((JobDetailsStatus)status, (int)ActionTab.None);
            JMRestoreReportJobDetailes mRestoreReportJobDetails = new JMRestoreReportJobDetailes();
            mRestoreReportJobDetails.Status = status;
            mRestoreReportJobDetails.Comment = message;
            mRestoreReportJobDetails.Url = url;
            mRestoreReportJobDetails.Title = url.Substring(url.LastIndexOf("/")+1);
            mRestoreReportJobDetails.Level = "RM_JS_Rule_ObjectLevel_SiteCollection";
            mReportManager.SendJobDetail(mRestoreReportJobDetails);
        }

        public void AddArchiverDedupReportJobDetailReport(string subJobId,string siteCollectionUrl, int dedupTotalCount, long dedupTotalSize, JobDetailsStatus status, string message = "")
        {
            AnalyzeStatus((JobDetailsStatus)status, (int)ActionTab.None);
            JMArchiverDedupReportDetails dedupReportDetails = new JMArchiverDedupReportDetails();
            dedupReportDetails.SubJobId = subJobId;
            dedupReportDetails.Date = DateTime.UtcNow.Ticks;
            dedupReportDetails.SrcURL = siteCollectionUrl;
            dedupReportDetails.Size = dedupTotalSize;
            dedupReportDetails.Remark1 = dedupTotalCount;
            dedupReportDetails.Status = status;
            dedupReportDetails.Comment = message;
            mReportManager.SendJobDetail(dedupReportDetails);
        }

        public void AddArchiverDedupReportJobSummaryReport(int siteCollectionCount, int failedSiteCollectionCount, int totalDedupFilesCount, long totalDedupFilesSize, string message = "")
        {
            JMArchiverDedupReportSummaryDetails dedupReportSummaryDetails = new JMArchiverDedupReportSummaryDetails();
            dedupReportSummaryDetails.SiteCollectionCount = siteCollectionCount;
            dedupReportSummaryDetails.FailedSiteCollectionCount = failedSiteCollectionCount;
            dedupReportSummaryDetails.TotalDedupFilesCount = totalDedupFilesCount;
            dedupReportSummaryDetails.TotalDedupFilesSize = totalDedupFilesSize;
            mReportManager.SendJobDetail(dedupReportSummaryDetails);
        }
        public void AddSummary(string subJobId)
        {
        }

        public void SendVaultBeforeArcSummary(string subJobId)
        {
        }

        public void AddDeletionReport(string url, long nodeSize, JobDetailsStatus status, int cacheNodeType, string subJobId, string rulename, string mediaName, string keepData, string message = "", params object[] defaultArgs)
        {
            AnalyzeStatus(status, (int)ActionTab.Action);
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
            mArchiverActionJobDetails.SourceLocation = url;
            mArchiverActionJobDetails.Size = nodeSize.ToString();
            mArchiverActionJobDetails.RuleName = rulename;
            mArchiverActionJobDetails.Status = status;
            mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
            mArchiverActionJobDetails.ActionTab = (int)ActionTab.Action;
            mArchiverActionJobDetails.Action = keepData;
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.Comment = message;
            mReportManager.SendJobDetail(mArchiverActionJobDetails);
            AnalyzeOtherDetailsForSummary(nodeSize, cacheNodeType, status);
            StatisticOtherDetailsDeleteSie(nodeSize, status, keepData);
        }

        public void StatisticOtherDetailsDeleteSie(long deleteSize, JobDetailsStatus status, string keepData)
        {
            if (OtherActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (OtherActionStatistics == null)
                    {
                        OtherActionStatistics = new ActionStatistics();
                        OtherActionStatistics.ActionTab = (int)ActionTab.Action;
                    }
                }
            }
            lock (lockObj)
            {
                if (status == JobDetailsStatus.Successful && (keepData == "SO_Action_Delete" || keepData == "SO_Action_LevelStub" || keepData == "SO_Action_Destroy" || keepData == "RM_JS_RDM_CreateRule_Options_StoreInM365Archive"))
                {
                    OtherActionStatistics.DeleteSize += deleteSize;
                }
            }
        }

        public void AddScanReport(string srcURL, long nodeSize, int cacheNodeType, string rulename, JobDetailsStatus status = JobDetailsStatus.Successful, string errorMessage = "")
        {
            try
            {
                AnalyzeStatus(status, (int)ActionTab.Scan);
                JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
                mArchiverActionJobDetails.SourceLocation = srcURL;
                mArchiverActionJobDetails.Size = nodeSize.ToString();
                mArchiverActionJobDetails.RuleName = rulename;
                mArchiverActionJobDetails.Status = status;
                mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
                mArchiverActionJobDetails.ActionTab = (int)ActionTab.Scan;
                mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
                mArchiverActionJobDetails.Comment = errorMessage;
                //if (IsFolderLevel(cacheNodeType))
                //{
                //    mArchiverActionJobDetails.Size = "0";
                //}
                mReportManager.SendJobDetail(mArchiverActionJobDetails);
                AnalyzeScanDetailsForSummary(nodeSize, cacheNodeType, status);
            }
            catch (Exception e)
            {
                Logger.Warn($"An error occurred when add scan report {e.ToString()}");
            }
        }

        /// <summary>
        /// For SO & DSO simulate job
        /// </summary>
        public void AddScanReportForSimulation(string sourceUrl, long size, int cacheNodeType, string ruleName, string action, long createdDate, string createdBy, long modifiedDate, string modifiedBy, JobDetailsStatus status = JobDetailsStatus.Successful, string errorMessage = "")
        {
            try
            {
                AnalyzeStatus(status, (int)ActionTab.Scan);
                JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
                mArchiverActionJobDetails.SourceLocation = sourceUrl;
                mArchiverActionJobDetails.Size = size.ToString();
                mArchiverActionJobDetails.RuleName = ruleName;
                mArchiverActionJobDetails.Status = status;
                mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
                mArchiverActionJobDetails.ActionTab = (int)ActionTab.Scan;
                mArchiverActionJobDetails.Action = action;
                mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
                mArchiverActionJobDetails.Comment = errorMessage;
                mArchiverActionJobDetails.Created = createdDate;
                mArchiverActionJobDetails.CreatedBy = createdBy;
                mArchiverActionJobDetails.Modified = modifiedDate;
                mArchiverActionJobDetails.ModifiedBy = modifiedBy;
                mArchiverActionJobDetails.RuleMatchFile = ruleName; //Fos DSO simulate
                mReportManager.SendJobDetail(mArchiverActionJobDetails);
                AnalyzeScanDetailsForSummary(size, cacheNodeType, status);
            }
            catch (Exception e)
            {
                Logger.Warn($"An error occurred when add scan report {e.ToString()}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">Source Url</param>
        /// <param name="desUrl">Destination Url</param>
        /// <param name="nodeSize">File Size</param>
        /// <param name="status">Record Manager Status</param>
        /// <param name="subJobID">Sub Job ID</param>
        /// <param name="ruleName"></param>
        /// <param name="mediaName"></param>
        /// <param name="message">Error Message</param>
        public void AddRecordReport(string url, string desUrl, long nodeSize, int cacheNodeType, JobDetailsStatus status, string subJobID, string ruleName, string message = "")
        {
            AnalyzeStatus(status, (int)ActionTab.Action);
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
            mArchiverActionJobDetails.SourceLocation = url;
            mArchiverActionJobDetails.DestinationLocation = desUrl;
            mArchiverActionJobDetails.Size = nodeSize.ToString();
            mArchiverActionJobDetails.RuleName = ruleName;
            mArchiverActionJobDetails.Status = status;
            mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
            mArchiverActionJobDetails.ActionTab = (int)ActionTab.Action;
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.Comment = message;
            mArchiverActionJobDetails.Action = "RM_JS_JM_JobType_RecordsExplorerMove";
            mReportManager.SendJobDetail(mArchiverActionJobDetails);
            AnalyzeOtherDetailsForSummary(nodeSize, cacheNodeType, status);
        }

        public void AddRecordReport(string url, ConvertStubAction action , JobDetailsStatus status, string message = "")
        {
            IncreaseProgressForConvertStub(action, status);
            AnalyzeStatus(status, (int)ActionTab.None);
            JMConvertStubJobDetails mArchiverActionJobDetails = new JMConvertStubJobDetails();
            mArchiverActionJobDetails.FullPath = url;
            mArchiverActionJobDetails.Status = status;
            mArchiverActionJobDetails.Action = (int)action;
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.Comment = message;
            mReportManager.SendJobDetail(mArchiverActionJobDetails);
        }

        public void IncreaseProgressForConvertStub(ConvertStubAction action, JobDetailsStatus status)
        {
            if (status == JobDetailsStatus.Successful)
            {
                mReportManager.Increase();
                return;
            }

            int step = action switch
            {
                ConvertStubAction.Scan => 3,
                ConvertStubAction.Create => 2,
                ConvertStubAction.Delete => 1,
                _ => 0
            };

            if (step > 0) mReportManager.Increase(step);
        }

        public void SendJobDetailForRetention(JMJobDetails details)
        {
            mReportManager.SendJobDetail(details);
        }

        public void AddDeletionCommons(string level)
        {

        }
        public void FinishRestoreReport()
        {
            AddRestoreJobSummaryDetails();
            if (string.IsNullOrEmpty(summaryComments))
                mReportManager.SetJobFinished(GetJobStatus());
            else
                mReportManager.SetJobFinished(GetJobStatus(), summaryComments);
        }
        public void FinishReport()
        {
            AddJobSummaryDetails();
            if (string.IsNullOrEmpty(summaryComments))
                mReportManager.SetJobFinished(GetJobStatus());
            else
                mReportManager.SetJobFinished(GetJobStatus(), summaryComments);
        }

        public void AddRestoreJobSummaryDetails()
        {
            JMRestoreSummaryDetails summaryDetails = new JMRestoreSummaryDetails();
            summaryDetails.ActionStatistics = new List<ActionStatistics>();
            if (RestoreActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(RestoreActionStatistics);
            }
            if (summaryDetails.ActionStatistics.Count > 0)
            {
                mReportManager.SendJobDetail(summaryDetails);
            }
        }

        public void AddJobSummaryDetails()
        {
            JMSOSummaryDetails summaryDetails = new JMSOSummaryDetails();
            summaryDetails.ActionStatistics = new List<ActionStatistics>();
            if (ScanActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(ScanActionStatistics);
            }
            if (BackupActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(BackupActionStatistics);
            }
            if (ExportActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(ExportActionStatistics);
            }
            if (OtherActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(OtherActionStatistics);
            }
            if(summaryDetails.ActionStatistics.Count > 0)
            {
                mReportManager.SendJobDetail(summaryDetails);
            }
        }

        public JMSOSummaryDetails GetJobSummaryDetilsCopy()
        {
            JMSOSummaryDetails summaryDetails = new JMSOSummaryDetails();
            summaryDetails.ActionStatistics = new List<ActionStatistics>();
            if (ScanActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(ScanActionStatistics);
            }
            if (BackupActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(BackupActionStatistics);
            }
            if (ExportActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(ExportActionStatistics);
            }
            if (OtherActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(OtherActionStatistics);
            }
            string serJson = SerializerHelper.SerializeByJsonConvert(summaryDetails);
            return SerializerHelper.DeserializeByJsonConvert<JMSOSummaryDetails>(serJson);
        }

        public void AddDetailOnly(string url, long nodeSize, int cacheNodeType, JobDetailsStatus status, string ruleName, string message = "", string action = "RM_JM_DeletionStatus_Retention")
        {
            AnalyzeStatus(status, (int)ActionTab.Action);
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
            mArchiverActionJobDetails.SourceLocation = url;
            mArchiverActionJobDetails.Size = nodeSize.ToString();
            mArchiverActionJobDetails.RuleName = ruleName;
            mArchiverActionJobDetails.Status = status;
            mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
            mArchiverActionJobDetails.ActionTab = (int)ActionTab.Action;
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.Comment = message;
            mArchiverActionJobDetails.Action = action;
            mReportManager.SendJobDetail(mArchiverActionJobDetails);
            AnalyzeOtherDetailsForSummary(nodeSize, cacheNodeType, status);
        }

        private void AnalyzeScanDetailsForSummary(long nodeSize, int cacheNodeType, JobDetailsStatus status)
        {
            if (ScanActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (ScanActionStatistics == null)
                    {
                        ScanActionStatistics = new ActionStatistics();
                        ScanActionStatistics.ActionTab = (int)ActionTab.Scan;
                    }
                }
            }
            lock (lockObj)
            {
                if (status == JobDetailsStatus.Successful)
                {
                    ScanActionStatistics.Size += nodeSize;
                }
                AnalyzeStatusForSummary(ScanActionStatistics, cacheNodeType, status);
            }
        }

        private void AnalyzeBackUpDetailsForSummary(long nodeSize, int cacheNodeType, JobDetailsStatus status)
        {
            if (BackupActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (BackupActionStatistics == null)
                    {
                        BackupActionStatistics = new ActionStatistics();
                        BackupActionStatistics.ActionTab = (int)ActionTab.Backup;
                    }
                }
            }
            lock (lockObj)
            {
                if (status == JobDetailsStatus.Successful)
                {
                    BackupActionStatistics.Size += nodeSize;
                }
                AnalyzeStatusForSummary(BackupActionStatistics, cacheNodeType, status);
            }

            JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(ActionTab.Backup, cacheNodeType, nodeSize);
        }

        private void AnalyzeExportDetailsForSummary(long nodeSize, int cacheNodeType, JobDetailsStatus status)
        {
            if (ExportActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (ExportActionStatistics == null)
                    {
                        ExportActionStatistics = new ActionStatistics();
                        ExportActionStatistics.ActionTab = (int)ActionTab.Export;
                    }
                }
            }
            if (status == JobDetailsStatus.Successful)
            {
                ExportActionStatistics.Size += nodeSize;
            }
            AnalyzeStatusForSummary(ExportActionStatistics, cacheNodeType, status);

            JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(ActionTab.Export, cacheNodeType, nodeSize);
        }

        private void AnalyzeRestoreDetailsForSummary(long nodeSize, int cacheNodeType, JobDetailsStatus status)
        {
            if (RestoreActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (RestoreActionStatistics == null)
                    {
                        RestoreActionStatistics = new ActionStatistics();
                        RestoreActionStatistics.ActionTab = (int)ActionTab.Restore;
                    }
                }
            }
            lock (lockObj)
            {
                if (status == JobDetailsStatus.Successful)
                {
                    RestoreActionStatistics.Size += nodeSize;
                }
                AnalyzeStatusForSummary(RestoreActionStatistics, cacheNodeType, status);
            }
        }

        private void AnalyzeOtherDetailsForSummary(long nodeSize, int cacheNodeType, JobDetailsStatus status)
        {
            if (OtherActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (OtherActionStatistics == null)
                    {
                        OtherActionStatistics = new ActionStatistics();
                        OtherActionStatistics.ActionTab = (int)ActionTab.Action;
                    }
                }
            }
            lock (lockObj)
            {
                if (status == JobDetailsStatus.Successful)
                {
                    OtherActionStatistics.Size += nodeSize;
                }
                AnalyzeStatusForSummary(OtherActionStatistics, cacheNodeType, status);
            }

            JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(ActionTab.Action, cacheNodeType, nodeSize);
        }

        private void AnalyzeStatusForSummary(ActionStatistics sta, int cacheNodeType, JobDetailsStatus status)
        {
            switch (status)
            {
                case JobDetailsStatus.Successful:
                    AnalyzeObjCount(sta.SuccessfulObj, cacheNodeType);
                    break;
                case JobDetailsStatus.Skipped:
                    AnalyzeObjCount(sta.SkippedObj, cacheNodeType);
                    break;
                case JobDetailsStatus.Failed:
                    AnalyzeObjCount(sta.FailedObj, cacheNodeType);
                    break;
                default:
                    break;
            }
        }

        private void AnalyzeObjCount(ObjectStatistic objSta, int cacheNodeType)
        {
            if (cacheNodeType == (int)CacheNodeType.Exception)
            {
                objSta.ExceptionCount++;
            }
            else if (cacheNodeType >= (int)CacheNodeType.Item)
            {
                objSta.ItemCount++;
            }
            else if (cacheNodeType > (int)CacheNodeType.List)
            {
                objSta.FolderCount++;
            }
            else if (cacheNodeType == (int)CacheNodeType.List)
            {
                objSta.ListCount++;
            }
            else if (cacheNodeType >= (int)CacheNodeType.Web)
            {
                objSta.SiteCount++;
            }
            else if (cacheNodeType == (int)CacheNodeType.SiteCollection)
            {
                objSta.SiteCollectionCount++;
            }
        }



        public void SendAndWaitFlushAllReport()
        {
            AddJobSummaryDetails();
            mReportManager.WaitFlushAllDetail();

            ScanActionStatistics = null;
            BackupActionStatistics = null;
            ExportActionStatistics = null;
            OtherActionStatistics = null;
        }

        #endregion



        #region EndUser
        public string GetRelativeDataJobState(string subJobId)
        {
            string state = string.Empty;
            string processFilePath = Path.Combine(AveEnv.AgentJobFolder, subJobId, subJobId + ".txt");
            if (!File.Exists(processFilePath))
            {
                SendEndUserNotBackupState(subJobId);
            }
            using (StreamReader stream = new StreamReader(processFilePath))
            {
                state = stream.ReadLine();
            }
            return state;
        }

        public void SendEndUserNotBackupState(string subJobId)
        {
            string folderPath = Path.Combine(AveEnv.AgentJobFolder, subJobId);
            string processFilePath = Path.Combine(AveEnv.AgentJobFolder, subJobId, subJobId + ".txt");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            if (!File.Exists(processFilePath))
            {
                using (StreamWriter sw = new StreamWriter(processFilePath, false))
                {
                    sw.WriteLine("NotBackup");
                }
            }
        }
        #endregion
    }
}
