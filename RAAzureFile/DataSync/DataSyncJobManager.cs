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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.AzureFileShare.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAAzureFile.DataSync
{
    public class DataSyncJobManager
    {
        private static readonly IRMSubJobDao SubJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
        
        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;

        private static bool HasSucceedDetail { get; set; }

        private static bool HasFailedDetail { get; set; }

        private static readonly Dictionary<int, string> NodeTypeI18ns = new Dictionary<int, string>
        {
            { (int)RMNodeLevel.AzureFileShareDirectory, "RM_RDM_RecordDetails_DataType_AzureFileDirectory" },
            { (int)RMNodeLevel.AzureFileShareFile, "RM_JS_Rule_ObjectLevel_Document" },
        };

        public static void Init(string jobId)
        {
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.AzureFileShareDataSynchronisation);
            ReportManager.StartUpdateJobProgress(60);
        }

        public static string GetJobContent(string jobId)
        {
            var jobInfo = SubJobDao.GetSubJob(jobId, true);
            if(string.IsNullOrEmpty(jobInfo?.JobContext?.Content))
            {
                throw new Exception("Can't find job context info.");
            }
            return jobInfo.JobContext.Content;
        }

        public static void AddSucceedJobDetail(Record item)
        {
            var detail = new JMAzureFileShareDataSyncDetail
            {
                ObjectName = item.LeafName,
                FullPath = AzureFileShareApiUtil.UrlCombin(item.DirPath, item.LeafName),
                ItemType = NodeTypeI18ns[item.NodeType],
                Status = JobDetailsStatus.Successful,
            };
            ReportManager.SendJobDetail(detail);
            HasSucceedDetail = true;
        }

        public static void AddFailedJobDetail(Record item, string comment)
        {
            var detail = new JMAzureFileShareDataSyncDetail
            {
                ObjectName = item.LeafName,
                FullPath = AzureFileShareApiUtil.UrlCombin(item.DirPath, item.LeafName),
                ItemType = NodeTypeI18ns[item.NodeType],
                Status = JobDetailsStatus.Failed,
                Comment = comment
            };
            ReportManager.SendJobDetail(detail);
            HasFailedDetail = true;
        }

        public static void AddFailedJobDetail(SyncFailureItemEntity item, string comment)
        {
            var detail = new JMAzureFileShareDataSyncDetail 
            { 
                ObjectName = item.FullPath.Split('/').Last(),
                FullPath = item.FullPath,
                ItemType = item.IsDirectory ? "RM_RDM_RecordDetails_DataType_AzureFileDirectory" : "RM_JS_Rule_ObjectLevel_Document",
                Status = JobDetailsStatus.Failed,
                Comment = comment
            };
            ReportManager.SendJobDetail(detail);
            HasFailedDetail = true;
        }

        public static void AddFailedJobDetail(AzureFileShareApiItem item, string comment)
        {
            var detail = new JMAzureFileShareDataSyncDetail
            {
                ObjectName = item.Name,
                FullPath = item.FullPath,
                ItemType = item.IsDirectory ? "RM_RDM_RecordDetails_DataType_AzureFileDirectory" : "RM_JS_Rule_ObjectLevel_Document",
                Status = JobDetailsStatus.Failed,
                Comment = comment
            };
            ReportManager.SendJobDetail(detail);
            HasFailedDetail = true;
        }

        public static void AddFailedJobDetail(AzureFileShareApiDirectoryClient directory, string comment)
        {
            var detail = new JMAzureFileShareDataSyncDetail
            {
                ObjectName = directory.Name,
                FullPath = directory.FullPath,
                ItemType = "RM_RDM_RecordDetails_DataType_AzureFileDirectory",
                Status = JobDetailsStatus.Failed,
                Comment = comment
            };
            ReportManager.SendJobDetail(detail);
            HasFailedDetail = true;
        }

        public static void SetJobFinished()
        {
            var jobFinishStatus = HasSucceedDetail && HasFailedDetail ?
                JobStatus.FinishWithException :
                (
                    HasFailedDetail ?
                    JobStatus.Failed :
                    JobStatus.Finished
                );
            ReportManager.SetJobFinished(jobFinishStatus);
        }

        public static void SetJobFailed(string comment)
        {
            ReportManager.SetJobFinished(JobStatus.Failed, comment);
        }
    }
}
