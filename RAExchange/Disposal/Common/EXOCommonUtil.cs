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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.RAExchange.Common;
using Cloud.sdk.Data.Opus.GoogleOne;
using ExchangeBackupUtility.Graph;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.Disposal.Common
{
    public static class EXOCommonUtil
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(EXOCommonUtil));

        private static readonly object lockObj = new object();

        private static EXOReportCenter reportCenter;

        private static EXOReportCenter ReportCenter
        {
            get
            {
                if(reportCenter == null)
                {
                    lock (lockObj)
                    {
                        if(reportCenter == null)
                        {
                            reportCenter = new EXOReportCenter(ReportMangerFactory.Instance.ReportManager);
                        }
                    }
                }
                return reportCenter;
            }
        }

        public static void AddDetail(Item EXOItem, string fullPath, string ruleName, string destinationUrl, JobDetailsStatus status, string action, string errorMessage = null)
        {
            string dateTimeSent = string.Empty;
            try
            {
                //To avoid get dateTimeSent failed and no job detail.
                dateTimeSent = EXOItem.DateTimeSent.ToUniversalTime().ToString("R");
            }
            catch (Exception ex)
            {
                logger.Info($"Can not get mail DateTimeSent:{fullPath}.Message:{ex}.");
            }
            ReportCenter.AddReportRecord(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOEnforceRuleActionJobDetails()
            {
                Action = action,
                ObjectName = EXOItem?.Subject,
                FullPath = EXOItem != null ? fullPath + "_" + dateTimeSent : fullPath,
                RuleName = ruleName,
                DestinationUrl = destinationUrl,
                ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                Status = ProcessJobDetailStatus(errorMessage, status),
                Comment = ProcessJobDetailMessage(errorMessage, status),
            });
            if (status == JobDetailsStatus.Successful)
            {
                JobManagement jm = JobManagement.GetInstance("", JobType.EXORecordsDisposal);
                jm.HasSuccessNode = true;
            }
            EXOEnforceRuleActionStatisticStore.RecordDetail(status, action);
        }

        public static void AddDetail(IExchangeItem EXOItem, string fullPath, string ruleName, string destinationUrl, JobDetailsStatus status, string action, string errorMessage = null)
        {
            string dateTimeSent = string.Empty;
            try
            {
                //To avoid get dateTimeSent failed and no job detail.
                dateTimeSent = EXOItem.SendDateUTC.ToUniversalTime().ToString("R");
            }
            catch (Exception ex)
            {
                logger.Info($"Can not get mail DateTimeSent:{fullPath}.Message:{ex}.");
            }
            ReportCenter.AddReportRecord(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOEnforceRuleActionJobDetails()
            {
                Action = action,
                ObjectName = EXOItem?.ItemName,
                FullPath = EXOItem != null ? fullPath + "_" + dateTimeSent : fullPath,
                RuleName = ruleName,
                DestinationUrl = destinationUrl,
                ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                Status = ProcessJobDetailStatus(errorMessage, status),
                Comment = ProcessJobDetailMessage(errorMessage, status),
            });
            AnalyzeStatus(status, JobType.EXORecordsDisposal);
            EXOEnforceRuleActionStatisticStore.RecordDetail(status, action);
        }

        public static void AddDetail(NodeLevel nodeLevel, string subject, string fullPath, string ruleName, string destinationUrl, JobDetailsStatus status, string action, string errorMessage = null)
        {
            ReportCenter.AddReportRecord(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOEnforceRuleActionJobDetails()
            {
                Action = action,
                ObjectName = subject,
                FullPath = fullPath,
                RuleName = ruleName,
                DestinationUrl = destinationUrl,
                ItemType = JobReportUtility.ConvertItemTypeForDetails(nodeLevel),
                Status = ProcessJobDetailStatus(errorMessage, status),
                Comment = ProcessJobDetailMessage(errorMessage, status),
            });
            if(status == JobDetailsStatus.Successful)
            {
                JobManagement jm = JobManagement.GetInstance("", JobType.EXORecordsDisposal);
                jm.HasSuccessNode = true;
            }
            EXOEnforceRuleActionStatisticStore.RecordDetail(status, action);
        }

        public static JobDetailsStatus ProcessJobDetailStatus(string oldErrorMessage, JobDetailsStatus oldJobDetailStatus)
        {
            if (oldErrorMessage != null && oldErrorMessage.Contains("Timeout performing RPUSH"))
            {
                return JobDetailsStatus.Exception;
            }
            return oldJobDetailStatus;
        }

        public static string ProcessJobDetailMessage(string oldErrorMessage, JobDetailsStatus oldJobDetailStatus)
        {
            if (oldErrorMessage != null)
            {
                if (oldErrorMessage.Contains("Timeout performing RPUSH"))
                {
                    return "RM_EXODisposal_Exception_TimeOut";
                }
                else if (oldErrorMessage.Contains("Access is denied. Check credentials and try again."))
                {
                    return "RM_Aos_CustomApp_Permission";
                }
            }
            return oldErrorMessage;
        }

        public static void AddJobSummaryStatistic()
        {
            ReportCenter.CommitDisposalAnalysis();
        }
        public static void AddDetailItem(string action, IExchangeItem item, string mailBoxAddress, JobDetailsStatus status, string message, string classification)
        {
            ReportCenter.AddRecordDetail(new JMEXOApplySettingJobDetails()
            {
                Action = action,
                ObjectName = item.ItemName,
                FullPath = mailBoxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R"),
                ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                Status = ProcessJobDetailStatus(message, status),
                Comment = ProcessJobDetailMessage(message, status),
                Classification = classification
            });
            AnalyzeStatus(status, JobType.EXOApplySetting);
        }

        public static void AddDetailsForSyncDataJob(IExchangeItem item, string mailBoxAddress, JobDetailsStatus status, string message)
        {
            ReportCenter.AddRecordDetail(new JMEXODataSyncJobDetails()
            {
                ObjectName = item.ItemName,
                FullPath = mailBoxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R"),
                ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                Status = ProcessJobDetailStatus(message, status),
                Comment = ProcessJobDetailMessage(message, status),
            });
            AnalyzeStatus(status, JobType.EXODataSynchronisation);
        }

        public static void AnalyzeStatus(JobDetailsStatus status, JobType jobType, string jobId = "")
        {
            JobManagement jm = JobManagement.GetInstance(jobId, jobType);
            switch (status)
            {
                case JobDetailsStatus.Successful:
                case JobDetailsStatus.Skipped:
                    jm.HasSuccessNode = true;
                    break;
                case JobDetailsStatus.Failed:
                    jm.HasErrorNode = true;
                    break;
                default:
                    break;
            }
        }
    }
}
