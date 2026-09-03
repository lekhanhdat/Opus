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
using AngleSharp.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.RA.SharePoint.Object;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.Telemetry.Data.CloudRecords;
using Microsoft.Graph.Models.CallRecords;
using Newtonsoft.Json;
using RAArchiverCommon.DisposalProgress.Impl;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Common.JobExecutionProcess
{
    internal class JobExecutionProcessStatisticExecutor : IDisposable
    {

        private static readonly AveLogger Logger = AveLogger.GetInstance(typeof(JobExecutionProcessStatisticExecutor));

        private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();

        private IRMSubJobDao RMSubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();

        private volatile JobExecutionProcessStatictics jobExecutionProcessStatictics = new JobExecutionProcessStatictics();

        private volatile CloudRecordsReportJobExecutionRecord jobExecutionRecord;
        private volatile ScheduleConfiguration mConfiguration;

        private (string ruleId, DateTime startTime, Guid deleteSummaryCookie) _deleteSumamrySession;
        private (string ruleId, DateTime startTime, Guid archiveSummaryCookie) _archiveSumamrySession;

        public const string EXCEPTION_RULE_ID = "00000000-0000-0000-0000-000000000000";

        private Double OneKiloByte => 1024d;
        private Double OneMegaByte => OneKiloByte * 1024;
        private Double OneGigaByte => OneMegaByte * 1024;

        private DateTime LastPrintLogTime = DateTime.MinValue;

        private readonly static object _lock = new object();

        private static JobExecutionProcessStatisticExecutor _instance;

        public static JobExecutionProcessStatisticExecutor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new JobExecutionProcessStatisticExecutor();
                        }
                    }
                }
                return _instance;
            }
        }

        internal void StartCalculateRuleAndSummary(string nodeLevel, string nodePath)
        {
            try
            {
                lock (_lock)
                {
                    jobExecutionProcessStatictics.ScanSummary ??= new ScanSummary()
                    {
                        ScanStartTime = DateTime.UtcNow,
                        NodeLevel = nodeLevel,
                        JobNode = nodePath
                    };
                    jobExecutionProcessStatictics.RuleSummaryDic ??= new Dictionary<string, RuleSummary>();
                }

                JobExecutionProgressStatisticExecutor.Instance.StartProgressForScan();
                LogJobExecutionRecord();
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when start calculate Rule Scummary and Scan Summary.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when start calculate Rule Scummary and Scan Summary.ex:{ex}");
            }
        }

        internal void CalculateRuleAndScanSummary(ProcessResult processResult, ArchiveApproveReport itemNode, Rule rule)
        {
            try
            {
                if (processResult != ProcessResult.FitParentRule && processResult != ProcessResult.Default)
                {
                    return;
                }

                string ruleId = rule?.Id;
                ruleId = Guid.TryParse(ruleId, out Guid result) && result != Guid.Empty ? ruleId : itemNode.RuleId;
                ruleId = Guid.TryParse(ruleId, out result) && result != Guid.Empty ? ruleId : EXCEPTION_RULE_ID;

                CalculateScanSummary(processResult, itemNode, ruleId);
                CalculateRuleSummary(processResult, itemNode, rule, ruleId);
                EachHourLogJobExecutionRecord();
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when CalculateRuleAndScanSummary.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when CalculateRuleAndScanSummary.ex:{ex}");
            }
        }

        private void CalculateRuleSummary(ProcessResult processResult, ArchiveApproveReport itemNode, Rule rule, string ruleId)
        {
            try
            {
                lock (_lock)
                {
                    if (jobExecutionProcessStatictics.RuleSummaryDic == null)
                    {
                        jobExecutionProcessStatictics.RuleSummaryDic = new Dictionary<string, RuleSummary>();
                    }

                    IDictionary<string, RuleSummary> ruleSummaryDic = jobExecutionProcessStatictics.RuleSummaryDic;
                    RuleSummary nowRuleSummary = ruleSummaryDic.GetOrDefault(ruleId, new RuleSummary() { RuleId = ruleId });
                    ruleSummaryDic.TryAdd(ruleId, nowRuleSummary);

                    nowRuleSummary.MatchRuleFileCount++;
                    nowRuleSummary.MatchRuleFileSize += itemNode.DocumentSize;
                    nowRuleSummary.MatchRuleFileGBSize = nowRuleSummary.MatchRuleFileSize / OneGigaByte;
                    
                    CheckRuleAction(rule, JobServiceUtility.GetCacheNodeType(itemNode.CacheNodeType), out bool isExport, out bool isArchive, out bool isOtherActions);
                    isArchive |= ((rule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup) && itemNode.IsRelativeDataJob;
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseTotalMatchedRuleFiles(isExport, isArchive, isOtherActions);

                    if(nowRuleSummary.Rule == null && rule != null)
                    {
                        nowRuleSummary.RuleName = rule?.Name;
                        if (mConfiguration.IsOneDriverSite)
                        {
                            nowRuleSummary.Rule = rule.OneDriveRule;
                        }
                        else
                        {
                            nowRuleSummary.Rule = rule;
                        }
                    }


                }
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when calculate Rule Scummary.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when calculate Rule Scummary.ex:{ex}");
            }
        }

        private void CheckRuleAction(Rule currentRule, CacheNodeType cacheNodeType, out bool isExport, out bool isArchive, out bool isOtherActions)
        {
            bool isItemVersion = cacheNodeType == CacheNodeType.ItemVersion || cacheNodeType == CacheNodeType.HSMItemVersion;
            bool isExportOnly = currentRule.ExportInfo != null && currentRule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive;

            bool hasExport = IsRuleHasExport(currentRule);
            bool hasArchive = IsRuleHasArchive(currentRule);
            bool hasOther = IsRuleHasOtherActions(currentRule);

            bool isKeepLatestMajorAndMinorVersion = (currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersion) == (int)KeepDataOption.KeepLatestVersion && currentRule.KeepLatestMajorAndMinorVersion > 0;
            bool isRuleSupportVersionAction = IsRuleSupportVersionAction(cacheNodeType, currentRule);

            if (currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Folder && (currentRule.KeepDataOption & (int)KeepDataOption.TagContent) == (int)KeepDataOption.TagContent)
            {
                hasOther = hasOther && cacheNodeType == CacheNodeType.Folder;
            }

            isExport = hasExport;
            isArchive = hasArchive;
            isOtherActions = hasOther && (!isExportOnly
                && ((currentRule.KeepDataOption & (int)KeepDataOption.Keep) != (int)KeepDataOption.Keep || (currentRule.KeepDataOption & (int)KeepDataOption.TagContent) == (int)KeepDataOption.TagContent)
                && ((!isItemVersion && !isKeepLatestMajorAndMinorVersion) || (isItemVersion && isRuleSupportVersionAction)));
        }

        public bool IsRuleSupportVersionAction(CacheNodeType cacheNodeType, Rule currentRule)
        {
            bool isItemVersion = cacheNodeType == CacheNodeType.ItemVersion || cacheNodeType == CacheNodeType.HSMItemVersion;
            bool isKeepLatestMajorAndMinorVersion = (currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersion) == (int)KeepDataOption.KeepLatestVersion && currentRule.KeepLatestMajorAndMinorVersion > 0;
            bool isRuleSupportVersionAction = !isItemVersion
                || (currentRule.PolicyLevel.ToString().Contains("Version"))
                || (isKeepLatestMajorAndMinorVersion)
                || ((currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers && currentRule.KeepLatestMajorAndMinorVersionAndArchiveOthers >= 0)
                || (currentRule.spMoveOption is not null && currentRule.spMoveOption.MoveDestination is not null && currentRule.spMoveOption.MoveDestination.IsMoveVersions);
            return isRuleSupportVersionAction;
        }

        public bool IsRuleHasExport(Rule currentRule)
        {
            bool hasExport =
                currentRule.ExportInfo != null && currentRule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive
                || currentRule.ExportType == GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA
                || currentRule.ExportType == GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA
                || currentRule.ExportType == GCommon.Contract.StorageOptimization.Object.ExportTypeValue.VEO;
            return hasExport;
        }

        public bool IsRuleHasArchive(Rule currentRule)
        {
            bool hasArchive =
                (currentRule.KeepDataOption & (int)KeepDataOption.Keep) == (int)KeepDataOption.Keep
                || (currentRule.KeepDataOption & (int)KeepDataOption.Archive) == (int)KeepDataOption.Archive
                || (currentRule.KeepDataOption & (int)KeepDataOption.ArchiverOnly) == (int)KeepDataOption.ArchiverOnly
                || (currentRule.KeepDataOption & (int)KeepDataOption.ArchiveAndLeaveStub) == (int)KeepDataOption.ArchiveAndLeaveStub
                || (currentRule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemove) == (int)KeepDataOption.ArchiveBackupAndRemove
                || (currentRule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub
                || ((currentRule.KeepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument && (currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) != (int)KeepDataOption.NotBackup)
                || (currentRule.KeepDataOption & (int)KeepDataOption.Remove) == (int)KeepDataOption.Remove
                || currentRule.KeepDataOption == (int)KeepDataOption.Delete;
            return hasArchive;
        }

        public bool IsRuleHasOtherActions(Rule currentRule)
        {
            bool hasOther =
                (currentRule.MoveToRecordCenterAndDelareSetting != null && currentRule.MoveToRecordCenterAndDelareSetting.OperateDataMode == OperatingSharePointDataMode.MoveToRecordCenterAndDelare)
                || (currentRule.KeepDataOption & (int)KeepDataOption.Delete) == (int)KeepDataOption.Delete
                || (currentRule.KeepDataOption & (int)KeepDataOption.Remove) == (int)KeepDataOption.Remove
                || (currentRule.KeepDataOption & (int)KeepDataOption.ArchiveAndLeaveStub) == (int)KeepDataOption.ArchiveAndLeaveStub
                || (currentRule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemove) == (int)KeepDataOption.ArchiveBackupAndRemove
                || (currentRule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub
                || (currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup
                || (currentRule.KeepDataOption & (int)KeepDataOption.DeleteOnly) == (int)KeepDataOption.DeleteOnly
                || IsRuleM365Archive(currentRule);
            return hasOther;
        }

        private bool IsRuleM365Archive(Rule currentRule)
        {
            bool isM365Archive = (currentRule.KeepDataOption & (int)KeepDataOption.TriggerMicrosoft365Archiving) == (int)KeepDataOption.TriggerMicrosoft365Archiving;
            return isM365Archive;
        }

        private void IncreaseArchivedFiles(Rule rule, long fileSize)
        {
            if (IsRuleM365Archive(rule))
            {
                JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherActions();
            }
            else
            {
                JobExecutionProgressStatisticExecutor.Instance.IncreaseArchivedFiles(fileSize);
            }
        }

        private void CalculateScanSummary(ProcessResult processResult, ArchiveApproveReport itemNode , string ruleId)
        {
            try
            {
                lock (_lock)
                {
                    if (jobExecutionProcessStatictics.ScanSummary == null)
                    {
                        jobExecutionProcessStatictics.ScanSummary = new ScanSummary()
                        {
                            ScanStartTime = DateTime.UtcNow
                        };
                    }

                    ScanSummary scanSummary = jobExecutionProcessStatictics.ScanSummary;

                    if (ruleId == EXCEPTION_RULE_ID)
                    {
                        scanSummary.ScanMatchOthersFileCount++;
                        scanSummary.ScanMatchOthersFileSize += itemNode.DocumentSize;
                        scanSummary.ScanMatchOthersFileGBSize = scanSummary.ScanMatchOthersFileSize / OneGigaByte;
                    }
                    else
                    {
                        scanSummary.ScanMatchRuleFileCount++;
                        scanSummary.ScanMatchRuleFileSize += itemNode.DocumentSize;
                        scanSummary.ScanMatchRuleFileGBSize = scanSummary.ScanMatchRuleFileSize / OneGigaByte;
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when calculate Scan Summary.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when calculate Scan Summary.ex:{ex}");
            }
        }

        private void SendRuleInfosTelemetory()
        {
            try
            {
                if(jobExecutionProcessStatictics.RuleSummaryDic == null || !jobExecutionProcessStatictics.RuleSummaryDic.Any())
                {
                    return;
                }

                foreach(RuleSummary summary in jobExecutionProcessStatictics.RuleSummaryDic.Values)
                {
                    RealSendRuleInfoTelemetory(summary);
                }

                TelemetryContext.FlushAsync().GetAwaiter().GetResult();
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when SendRuleInfosTelemetory.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when SendRuleInfosTelemetory.ex:{ex}");
            }
        }

        private void RealSendRuleInfoTelemetory(RuleSummary summary)
        {
            try
            {
                if (summary.AlreadyCheckRuleRegionInfo)
                {
                    return;
                }

                if (!Guid.TryParse(summary?.Rule?.StoragePolicyId, out Guid result))
                {
                    return;
                }

                StorageDeviceDto deviceInfo = StorageDeviceService.GetStorageDeviceById(result.ToString());
                if (deviceInfo == null || deviceInfo.IsSystemStorage || deviceInfo.Type != (int)StorageDeviceType.CloudAzure)
                {
                    return;
                }

                CloudRecordsJobRuleInfoRecord cloudRecordsJobRuleInfoRecord = BuildJobRuleInfoRecord(deviceInfo, summary);

                if (RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
                {
                    LogCloudRecordsJobRuleInfoRecord(cloudRecordsJobRuleInfoRecord, "GCP ENV");
                    return;
                }

                RAReturnMessage checkRegionRes = StorageDeviceService.CheckAzureRegion(deviceInfo.mCurrentXRI.Params["accesspoint"], deviceInfo.mCurrentXRI.Params["name"], "").GetAwaiter().GetResult();
                if (checkRegionRes?.MessageType == RAMessageType.Successful)
                {
                    return;
                }

                LogCloudRecordsJobRuleInfoRecord(cloudRecordsJobRuleInfoRecord, checkRegionRes.ErrorMessage);
                if (checkRegionRes.ErrorMessage == I18NEntity.GetString("RM_AR_Storage_DC_Unmatch_WarnMessage") || checkRegionRes.ErrorMessage == "RM_AR_Storage_DC_Unmatch_WarnMessage")
                {
                    object[] args = new object[1];
                    args[0] = cloudRecordsJobRuleInfoRecord;
                    TelemetryContext.SendToQueue(TelemetryModule.JobRuleInfoRecord, TelemetryEventType.RunJob, args);
                }
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when single SendRuleInfoTelemetory.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when single SendRuleInfoTelemetory.ex:{ex}");
            }
            finally
            {
                if(summary != null)
                {
                    summary.AlreadyCheckRuleRegionInfo = true;
                }
            }
        }



        private CloudRecordsJobRuleInfoRecord BuildJobRuleInfoRecord(StorageDeviceDto deviceInfo, RuleSummary summary)
        {
            CloudRecordsJobRuleInfoRecord cloudRecordsJobRuleInfoRecord = new CloudRecordsJobRuleInfoRecord();
            cloudRecordsJobRuleInfoRecord.JobId = mConfiguration?.JobId;
            cloudRecordsJobRuleInfoRecord.MainJobId = mConfiguration?.MainJobId;
            cloudRecordsJobRuleInfoRecord.JobType = mConfiguration?.jobtype == null ? "" : ((int)mConfiguration?.jobtype).ToString();
            cloudRecordsJobRuleInfoRecord.StartTime = jobExecutionProcessStatictics?.ScanSummary?.ScanStartTime ?? default;
            cloudRecordsJobRuleInfoRecord.EndTime = jobExecutionProcessStatictics?.ScanSummary?.ScanEndTime ?? default;
            cloudRecordsJobRuleInfoRecord.RuleId = summary.RuleId;
            cloudRecordsJobRuleInfoRecord.RuleName = summary.RuleName;
            cloudRecordsJobRuleInfoRecord.MatchRuleItemCount = summary.MatchRuleFileCount;
            cloudRecordsJobRuleInfoRecord.MatchRuleItemSize = summary.MatchRuleFileSize;
            cloudRecordsJobRuleInfoRecord.DCRegion = StorageDeviceService.GetAzureRegionOfDataCenter();
            cloudRecordsJobRuleInfoRecord.StorageId = summary.Rule?.StoragePolicyId;
            cloudRecordsJobRuleInfoRecord.StorageDomain = StorageDeviceService.GetAzureAccessPointUrl(deviceInfo.mCurrentXRI.Params["accesspoint"], deviceInfo.mCurrentXRI.Params["name"]);
            return cloudRecordsJobRuleInfoRecord;
        }


        internal void EndCalculateRuleAndScanSummary(long totalScanCount, IAveSite site)
        {
            try
            {
                lock (_lock)
                {
                    ScanSummary scanSummary = jobExecutionProcessStatictics.ScanSummary;
                    scanSummary.SiteCollectionFileCount = totalScanCount;
                    scanSummary.SiteCollectionSize = site?.Size ?? 0;
                    scanSummary.SiteCollectionGBSize = scanSummary.SiteCollectionSize / OneGigaByte;
                    scanSummary.ScanEndTime = DateTime.UtcNow;

                    SendRuleInfosTelemetory();
                    LogJobExecutionRecord();
                }
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when end calculate Rule Scummary and Scan Summary.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when calculate Rule end Scummary and Scan Summary.ex:{ex}");
            }
        }


        internal void StartCalculateDeleteAndStubSummary(String ruleId, out Guid deleteSummaryCookie)
        {
            deleteSummaryCookie = Guid.NewGuid();
            try
            {
                lock (_lock)
                {
                    _deleteSumamrySession = (ruleId, DateTime.UtcNow, deleteSummaryCookie);
                    jobExecutionProcessStatictics.DeleteAndStubSummaryDic ??= new Dictionary<String, DeleteAndStubSummary>();

                    if (!Guid.TryParse(ruleId, out Guid result))
                    {
                        ruleId = EXCEPTION_RULE_ID;
                    }

                    jobExecutionProcessStatictics.DeleteAndStubSummaryDic.TryAdd(ruleId, new DeleteAndStubSummary { });
                }

                JobExecutionProgressStatisticExecutor.Instance.StartProgressForOther();
                LogJobExecutionRecord();
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when start calculate Delete And Stub Summary.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when start calculate Delete And Stub Summary.ex:{ex}");
            }
        }

        internal void EndCalculateDeleteAndStubSummary(String ruleId, Guid deleteSummaryCookie)
        {
            try
            {
                LogJobExecutionRecord();
                lock (_lock)
                {
                    if (_deleteSumamrySession.deleteSummaryCookie == deleteSummaryCookie
                    && jobExecutionProcessStatictics?.DeleteAndStubSummaryDic?.TryGetValue(ruleId, out DeleteAndStubSummary summary) == true
                    && summary != null)
                    {
                        summary.DeleteAndStubEndTime += DateTime.UtcNow - _deleteSumamrySession.startTime;
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when end calculate Delete And Stub Summary.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when end calculate Delete And Stub Summary.ex:{ex}");
            }
            finally
            {
                lock (_lock)
                {
                    _deleteSumamrySession = default;
                }
            }
        }

        internal void CalculateArchiveSummary(Rule rule, long size, int cacheNodeType, JobDetailsStatus status = JobDetailsStatus.Successful)
        {
            try
            {
                if (cacheNodeType != (int)CacheNodeType.Item && cacheNodeType != (int)CacheNodeType.ItemVersion && cacheNodeType != (int)CacheNodeType.Attachment
                    && cacheNodeType != (int)CacheNodeType.HSMItem && cacheNodeType != (int)CacheNodeType.HSMItemVersion)
                {
                    return;
                }
                if (status != JobDetailsStatus.Successful)
                {
                    IncreaseArchivedFiles(rule, 0);
                    return;
                }

                string ruleId = rule?.Id;
                if (!Guid.TryParse(ruleId, out Guid result))
                {
                    ruleId = EXCEPTION_RULE_ID;
                }

                lock (_lock)
                {
                    jobExecutionProcessStatictics.ArchiveSummaryDic ??= new Dictionary<string, ArchiveSummary>();
                    ArchiveSummary archiveSummary = jobExecutionProcessStatictics.ArchiveSummaryDic.GetOrDefault(ruleId, new ArchiveSummary { });
                    jobExecutionProcessStatictics.ArchiveSummaryDic.TryAdd(ruleId, archiveSummary);

                    archiveSummary.ArchivedFileCount++;
                    archiveSummary.ArchivedFileSize += size;
                    archiveSummary.ArchivedFileGBSize = archiveSummary.ArchivedFileSize / OneGigaByte;
                }

                IncreaseArchivedFiles(rule, size);
                EachHourLogJobExecutionRecord();
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when calculate Archive Summary.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when calculate Archive Summary.ex:{ex}");
            }
        }

        internal void CalculateArchiveSummary(ArchiveApproveReport entity, long size, Rule currentRule,JobDetailsStatus status = JobDetailsStatus.Successful)
        {
            try
            {
                if (entity.CacheNodeType != (int)CacheNodeType.Item && entity.CacheNodeType != (int)CacheNodeType.ItemVersion && entity.CacheNodeType != (int)CacheNodeType.Attachment
                    && entity.CacheNodeType != (int)CacheNodeType.HSMItem && entity.CacheNodeType != (int)CacheNodeType.HSMItemVersion)
                {
                    return;
                }
                if (status != JobDetailsStatus.Successful)
                {
                    IncreaseArchivedFiles(currentRule, 0);
                    return;
                }

                string ruleId = currentRule?.Id;
                ruleId = Guid.TryParse(ruleId, out Guid result) && result != Guid.Empty ? ruleId : entity.RuleId;
                ruleId = Guid.TryParse(ruleId, out result) && result != Guid.Empty ? ruleId : EXCEPTION_RULE_ID;

                lock (_lock)
                {
                    jobExecutionProcessStatictics.ArchiveSummaryDic ??= new Dictionary<string, ArchiveSummary>();
                    ArchiveSummary archiveSummary = jobExecutionProcessStatictics.ArchiveSummaryDic.GetOrDefault(ruleId, new ArchiveSummary {});
                    jobExecutionProcessStatictics.ArchiveSummaryDic.TryAdd(ruleId, archiveSummary);

                    archiveSummary.ArchivedFileCount++;
                    archiveSummary.ArchivedFileSize += size;
                    archiveSummary.ArchivedFileGBSize = archiveSummary.ArchivedFileSize / OneGigaByte;
                }

                IncreaseArchivedFiles(currentRule, size);
                EachHourLogJobExecutionRecord();
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when calculate Archive Summary.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when calculate Archive Summary.ex:{ex}");
            }
        }

        internal void StartCalculateArchiveSummary(String ruleId, out Guid archiveSummaryCookie)
        {
            archiveSummaryCookie = Guid.NewGuid();
            try
            {
                _archiveSumamrySession = (ruleId, DateTime.UtcNow, archiveSummaryCookie);

                JobExecutionProgressStatisticExecutor.Instance.StartProgressForArchived();
                LogJobExecutionRecord();
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when start calculate Archive Summary.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when start calculate Archive Summary.ex:{ex}");
            }
        }

        internal void EndCalculateArchiveSummary(String ruleId, Guid archiveSummaryCookie)
        {
            try
            {
                LogJobExecutionRecord();
                lock (_lock)
                {
                    if (_archiveSumamrySession.archiveSummaryCookie == archiveSummaryCookie
                    && jobExecutionProcessStatictics?.ArchiveSummaryDic?.TryGetValue(ruleId, out ArchiveSummary summary) == true
                    && summary != null)
                    {
                        summary.ArchiveEndTime += DateTime.UtcNow - _archiveSumamrySession.startTime;
                    }
                    _archiveSumamrySession = default;
                }
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when end calculate Archive Summary.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when end calculate Archive Summary.ex:{ex}");
            }
            finally 
            {
                lock (_lock)
                {
                    _archiveSumamrySession = default;
                }
            }
        }

        internal void CalculateSuccessDeleteAndStubSummary(JobDetailsStatus status, string ruleId, string action, int cacheNodeType)
        {
            try
            {
                if (cacheNodeType != (int)CacheNodeType.Item && cacheNodeType != (int)CacheNodeType.ItemVersion && cacheNodeType != (int)CacheNodeType.Attachment
                    && cacheNodeType != (int)CacheNodeType.HSMItem && cacheNodeType != (int)CacheNodeType.HSMItemVersion)
                {
                    return;
                }
                if (status != JobDetailsStatus.Successful)
                {
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherActions();
                    return;
                }

                if (!Guid.TryParse(ruleId, out Guid result))
                {
                    ruleId = EXCEPTION_RULE_ID;
                }

                lock (_lock)
                {
                    jobExecutionProcessStatictics.DeleteAndStubSummaryDic ??= new Dictionary<String, DeleteAndStubSummary>();

                    var deleteAndStubSummaryDic = jobExecutionProcessStatictics.DeleteAndStubSummaryDic;
                    DeleteAndStubSummary deleteAndStubSummary = deleteAndStubSummaryDic.GetOrDefault(ruleId, new DeleteAndStubSummary());
                    deleteAndStubSummaryDic.TryAdd(ruleId, deleteAndStubSummary);

                    if (action == "SO_Action_Delete" || action == "SO_Action_Destroy")
                    {
                        deleteAndStubSummary.DeletedFileCount++;
                    }
                    else if (action == "SO_Action_LevelStub")
                    {
                        deleteAndStubSummary.StubedFileCount++;
                        deleteAndStubSummary.DeletedFileCount++;
                    }
                }

                // Track delete/stub as "other" action in progress statistics (size is not available here)
                JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherActions();
                EachHourLogJobExecutionRecord();
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn($"a null reference exception occur when Calculate Delete and Stub Summary.ex:{ex}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"a exception occur when Calculate Delete and Stub Summary.ex:{ex}");
            }
        }

        internal void StartCalculateReportJobExecitonRecordInfo(ScheduleConfiguration configuration)
        {
            try
            {
                mConfiguration = configuration;
                lock (_lock)
                {
                    jobExecutionRecord = new CloudRecordsReportJobExecutionRecord()
                    {
                        StartTime = DateTime.UtcNow,
                    };
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"Start calculate restore job telemetry failed,error:{e}");
            }
        }

        internal void SendReportJobExecutionRecordInfo(string jobId, string mainJobId, JobType jobType, JobReportImps jobReportDto)
        {
            try
            {
                lock (_lock)
                {
                    jobExecutionRecord.JobId = jobId;
                    jobExecutionRecord.MainJobId = mainJobId;
                    jobExecutionRecord.JobType = jobType.ToString();
                    jobExecutionRecord.EndTime = DateTime.UtcNow;
                    jobExecutionRecord.TenantId = TenantLocalValue.LogonGroupId;
                    jobExecutionRecord.JobStatus = jobReportDto.GetJobStatus().ToString();
                    string executionRecord = JsonConvert.SerializeObject(jobExecutionProcessStatictics);
                    jobExecutionRecord.JobExecutionRecord = executionRecord;

                    long deleteSize = 0;
                    JMSOSummaryDetails jMSOSummaryDetails = jobReportDto.GetJobSummaryDetilsCopy();
                    ActionStatistics archiveStatistics = jMSOSummaryDetails.ActionStatistics.FirstOrDefault(detail => detail.ActionTab == (int)ActionTab.Backup);
                    if (archiveStatistics != null)
                    {
                        deleteSize -= archiveStatistics.Size;
                    }
                    ActionStatistics otherStatistics = jMSOSummaryDetails.ActionStatistics.FirstOrDefault(detail => detail.ActionTab == (int)ActionTab.Action);
                    if (otherStatistics != null)
                    {
                        deleteSize += otherStatistics.DeleteSize;
                    }
                    jobExecutionRecord.DeleteSize = deleteSize;

                    object[] args = new object[1];
                    args[0] = jobExecutionRecord;
                    TelemetryContext.SendToQueue(TelemetryModule.ReportJobExecutionRecord, TelemetryEventType.RunJob, args);
                    //TelemetryContext.FlushAsync().GetAwaiter().GetResult();

                    RMSubJobDao.UpdateContentOfSubJobContext(jobId, executionRecord);

                    LogJobExecutionRecord();
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"send restore job telemetry failed,error:{e}");
            }
        }

        internal void EachHourLogJobExecutionRecord()
        {
            if(DateTime.UtcNow - LastPrintLogTime > new TimeSpan(TimeSpan.TicksPerHour))
            {
                LastPrintLogTime = DateTime.UtcNow;
            }
            else
            {
                return;
            }
            LogJobExecutionRecord();
        }

        public void Dispose()
        {
            lock (_lock)
            {
                mConfiguration = null;
                JobExecutionProcessStatisticExecutor._instance = null;
            }
        }

        internal void LogJobExecutionRecord()
        {
            try
            {
                StringBuilder logContent = new StringBuilder($"-------- start print job execute record----------");

                logContent.Append(BuildScanSummaryLog());
                logContent.Append(BuildRuleSummaryLog());
                logContent.Append(BuildArchiveSummaryLog());
                logContent.Append(BuildDeleteAndStubSummaryLog());

                logContent.AppendLine("-------- end print job execute record----------");
                Logger.Info(logContent.ToString());
                RACustomLogger.WriteJobProgressLog($"NOW time: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}  \r\n" + logContent.ToString());
            }
            catch(Exception e)
            {
                Logger.Error($"Fail LogJobExecutionRecord,ex:{e}");
            }
            
        }

        internal string BuildScanSummaryLog()
        {
            try
            {
                var scanSummary = jobExecutionProcessStatictics.ScanSummary;
                if (scanSummary == null)
                {
                    return "";
                }

                StringBuilder logContent = new StringBuilder();
                logContent.AppendLine();
                logContent.AppendLine("ScanSummary Details");
                logContent.AppendLine($"ScanStartTime: {scanSummary.ScanStartTime}");
                logContent.AppendLine($"ScanEndTime: {scanSummary.ScanEndTime}");
                logContent.AppendLine($"JobNode: {scanSummary.JobNode}");
                logContent.AppendLine($"NodeLevel: {scanSummary.NodeLevel}");
                logContent.AppendLine($"SiteCollectionFileCount: {scanSummary.SiteCollectionFileCount}");
                logContent.AppendLine($"SiteCollectionSize: {scanSummary.SiteCollectionSize}");
                logContent.AppendLine($"SiteCollectionGBSize: {scanSummary.SiteCollectionGBSize}GB");
                logContent.AppendLine($"ScanMatchRuleFileCount: {scanSummary.ScanMatchRuleFileCount}");
                logContent.AppendLine($"ScanMatchRuleFileSize: {scanSummary.ScanMatchRuleFileSize}");
                logContent.AppendLine($"ScanMatchRuleFileGBSize: {scanSummary.ScanMatchRuleFileGBSize}GB");
                logContent.AppendLine($"ScanMatchOthersFileCount: {scanSummary.ScanMatchOthersFileCount}");
                logContent.AppendLine($"ScanMatchOthersFileSize: {scanSummary.ScanMatchOthersFileSize}");
                logContent.AppendLine($"ScanMatchOthersFileGBSize: {scanSummary.ScanMatchOthersFileGBSize}GB");

                return logContent.ToString();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Exception occurred while logging ScanSummary: {ex}");
                return "\r\nException occurred while logging ScanSummary";
            }
        }
        internal string BuildRuleSummaryLog()
        {
            try
            {
                var ruleSummaryDic = jobExecutionProcessStatictics.RuleSummaryDic;
                if (ruleSummaryDic == null || ruleSummaryDic.Count == 0)
                {
                    return "";
                }

                StringBuilder logContent = new StringBuilder();
                logContent.AppendLine();
                logContent.AppendLine("RuleSummary Details");

                foreach (var ruleSummary in ruleSummaryDic)
                {
                    logContent.AppendLine($"RuleId: {ruleSummary.Key}");
                    logContent.AppendLine($"RuleName: {ruleSummary.Value?.RuleName}");
                    logContent.AppendLine($"MatchRuleFileCount: {ruleSummary.Value?.MatchRuleFileCount}");
                    logContent.AppendLine($"MatchRuleFileSize: {ruleSummary.Value?.MatchRuleFileSize}");
                    logContent.AppendLine($"MatchRuleFileGBSize: {ruleSummary.Value?.MatchRuleFileGBSize}GB");
                    logContent.AppendLine();
                }

                return logContent.ToString();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Exception occurred while logging RuleSummary: {ex}");
                return "\r\nException occurred while logging RuleSummary";
            }
        }
        internal string BuildArchiveSummaryLog()
        {
            try
            {
                var archiveSummaryDic = jobExecutionProcessStatictics.ArchiveSummaryDic;
                if (archiveSummaryDic == null || archiveSummaryDic.Count == 0)
                {
                    return "";
                }

                StringBuilder logContent = new StringBuilder();
                logContent.AppendLine();
                logContent.AppendLine("ArchiveSummary Details");

                foreach (var archiveSummary in archiveSummaryDic)
                {
                    logContent.AppendLine($"RuleId: {archiveSummary.Key}");
                    logContent.AppendLine($"ArchivedFileCount: {archiveSummary.Value?.ArchivedFileCount}");
                    logContent.AppendLine($"ArchivedFileSize: {archiveSummary.Value?.ArchivedFileSize}");
                    logContent.AppendLine($"ArchivedFileGBSize: {archiveSummary.Value?.ArchivedFileGBSize}GB");
                    logContent.AppendLine();
                }

                return logContent.ToString();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Exception occurred while logging ArchiveSummary: {ex}");
                return "\r\nException occurred while logging ArchiveSummary";
            }
        }


        internal string BuildDeleteAndStubSummaryLog()
        {
            try
            {
                var deleteAndStubSummaryDic = jobExecutionProcessStatictics.DeleteAndStubSummaryDic;
                if (deleteAndStubSummaryDic == null || deleteAndStubSummaryDic.Count == 0)
                {
                    return "";
                }

                StringBuilder logContent = new StringBuilder();
                logContent.AppendLine();
                logContent.AppendLine("Delete And Stub Summary Details");

                foreach (var deleteAndStubSummary in deleteAndStubSummaryDic)
                {
                    logContent.AppendLine($"RuleId: {deleteAndStubSummary.Key}");
                    logContent.AppendLine($"StubedFileCount: {deleteAndStubSummary.Value?.StubedFileCount}");
                    logContent.AppendLine($"DeletedFileCount: {deleteAndStubSummary.Value?.DeletedFileCount}");
                    logContent.AppendLine();
                }

                return logContent.ToString();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Exception occurred while logging BuildDeleteAndStubSummaryLog: {ex}");
                return "\r\nException occurred while logging BuildDeleteAndStubSummaryLog";
            }
        }
        internal void LogCloudRecordsJobRuleInfoRecord(CloudRecordsJobRuleInfoRecord record, string preLog = "")
        {
            if (record == null)
            {
                Logger.Warn("CloudRecordsJobRuleInfoRecord is null.");
                return;
            }

            try
            {
                StringBuilder logContent = new StringBuilder();
                logContent.AppendLine(preLog + "CloudRecordsJobRuleInfoRecord Details:");
                logContent.AppendLine($"JobId: {record.JobId}");
                logContent.AppendLine($"MainJobId: {record.MainJobId}");
                logContent.AppendLine($"JobType: {record.JobType}");
                logContent.AppendLine($"StartTime: {record.StartTime}");
                logContent.AppendLine($"EndTime: {record.EndTime}");
                logContent.AppendLine($"RuleId: {record.RuleId}");
                logContent.AppendLine($"RuleName: {record.RuleName}");
                logContent.AppendLine($"MatchRuleItemCount: {record.MatchRuleItemCount}");
                logContent.AppendLine($"MatchRuleItemSize: {record.MatchRuleItemSize}");
                logContent.AppendLine($"DCRegion: {record.DCRegion}");
                logContent.AppendLine($"StorageId: {record.StorageId}");
                logContent.AppendLine($"StorageDomain: {record.StorageDomain}");

                Logger.Info(logContent.ToString());
                RACustomLogger.WriteJobProgressLog($"NOW time: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}  \r\n" + logContent.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception occurred while logging CloudRecordsJobRuleInfoRecord: {ex}");
                RACustomLogger.WriteJobProgressLog($"NOW time: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}  \r\nException occurred while logging CloudRecordsJobRuleInfoRecord: {ex}");
            }
        }

    }
}
