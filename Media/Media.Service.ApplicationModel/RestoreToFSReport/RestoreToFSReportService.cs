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




namespace AvePoint.Media.Service
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.Job;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.GCommon;
    using System.Reflection;
    using Merged18NResources.MediaServiceApplicationModel;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using global::Media.Common;
    #endregion

    public class RestoreToFSReportService : IRestoreToFSReportService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        SubJobDto subJobInfo;
        JobDetail currentJobDetail;
        List<JobDetail> indexDetails;
        RestoreToFSReportParameter jobReport;

        public IAJobDetailService ReportService { get; set; }

        public void PrepareForReport(RestoreJobBase restoreToFS)
        {
            this.jobReport = new RestoreToFSReportParameter();
            this.indexDetails = new List<JobDetail>();
            this.jobReport.Destination = restoreToFS.DestinationFSDevice == null ? string.Empty : this.RemoveUnnecessaryString(restoreToFS.DestinationFSDevice.ConnectionString);
            this.jobReport.MediaAddress = MediaEnvironment.MediaServer.MediaServerHostOrIpAddress;
            //this.jobReport.JobID = restoreToFS.JobId;
            this.jobReport.JobID = restoreToFS.SubJobId;
            this.jobReport.PlanId = restoreToFS.PlanId;
            //TODO Records
            //this.ReportService = JobReportServiceFactory.CreateJobDetailService();
            if (restoreToFS is ArchiverRestoreJob || restoreToFS is ArchiverExportJob)
            {
                //this.jobInfo = new BaseJobDto { Id = jobReport.JobID, Category = (Int32)PlanCategory.ArchiverRestore, PlanId = jobReport.PlanId, Type = 28 };
                this.subJobInfo = new SubJobDto { Id = jobReport.JobID,  ParentId = jobReport.JobID.Split('_')[0],PlanId = jobReport.PlanId, Type = 28 };
            }
            else
            {
                //this.jobInfo = new BaseJobDto { Id = jobReport.JobID, Category = (Int32)PlanCategory.GranularRestore, PlanId = jobReport.PlanId, Type = 8 };
                this.subJobInfo = new SubJobDto { Id = jobReport.JobID, ParentId = jobReport.JobID.Split('_')[0], PlanId = jobReport.PlanId, Type = 8 };
            }
        }

        public void SendJobSummary(JobSummaryMessage jobSummaryMessage)
        {
            this.ReportService.UpdateSubJobDetails(indexDetails, subJobInfo);
            indexDetails.Clear();
            List<JobSummary> summaryList = this.GetJobSummary(jobSummaryMessage);
            this.ReportService.UpdateSubJobSummary(summaryList, subJobInfo);
        }

        public void SendDetailReport(ItemDetailMessage itemDetailMessage)
        {
            currentJobDetail = new JobDetail();
            switch (itemDetailMessage.Status)
            {
                case 0:
                    this.FillSucceededJobReport(itemDetailMessage);
                    break;
                case 1:
                    this.FillFailedJobReport(itemDetailMessage);
                    break;
                case 2:
                    this.FillSkipedJobReport(itemDetailMessage);
                    break;
                default:
                    break;
            }
            this.FillJobDetail(itemDetailMessage);
            this.logger.Info(MediaServiceApplicationModelResource.RestoreToFSReportServiceSendDetailReportDetail, SensitiveLogExtension.FormatURLInLog(this.currentJobDetail?.SrcURL), this.currentJobDetail?.Type, this.currentJobDetail?.Size);
            this.indexDetails.Add(currentJobDetail);
            if (indexDetails.Count == 20)
            {
                ReportService.UpdateSubJobDetails(indexDetails, this.subJobInfo);
                this.logger.Info(MediaServiceApplicationModelResource.RestoreToFSReportServiceSendDetailReportFinish);
                this.indexDetails.Clear();
            }
        }

        private String RemoveUnnecessaryString(String destination)
        {
            destination = destination.Remove(destination.IndexOf("&", StringComparison.OrdinalIgnoreCase));
            destination = destination.Remove(0, destination.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1);
            return destination;
        }

        private List<JobSummary> GetJobSummary(JobSummaryMessage jobSummaryMessage)
        {
            List<JobSummary> summaryList = new List<JobSummary>();
            string jobStatus = GetJobStateName(jobSummaryMessage.JobStatus);
            summaryList.Add(new JobSummary() { Key = "Status", Value = jobStatus });
            summaryList.Add(new JobSummary() { Key = "Comments", Value = jobSummaryMessage.ErrorMessage });
            summaryList.Add(new JobSummary() { Key = "DataSize", Value = jobSummaryMessage.TotalSize.ToString() });

            var objectsCount = new Dictionary<NodeLevel, Int32>();
            var skippedObjectsCount = new Dictionary<NodeLevel, Int32>();
            var succeededObjectsCount = new Dictionary<NodeLevel, Int32>();
            //目前遇到的情况是：当创建出的路径超长时，zip folder会抛出异常，导致job状态是failed，但是此时的succeed数量已经统计完了，所以会出现job failed，但是object全是success这种情况
            //为了避免客户针对这个现象提出问题，在这里暂时做如下修改，将succeed的object都改成了failed。如果有更好的解决方案，可以随时替换此段代码。--AOSBR-3855
            if (jobStatus.Equals("Failed", StringComparison.OrdinalIgnoreCase)
                && (jobReport.SucceededSiteNum > 0 || jobReport.SucceededSiteCollectionNum > 0 || jobReport.SucceededlistNum > 0 || jobReport.SucceededFolderNum > 0 || jobReport.SucceededItemNum > 0))
            {
                //objectsCount.Add(NodeLevel.WebApplication, 1);
                objectsCount.Add(NodeLevel.SiteCollection, jobReport.SucceededSiteCollectionNum + jobReport.FailedSiteCollectionNum + jobReport.SkipedSiteCollectionNum);
                objectsCount.Add(NodeLevel.Site, jobReport.SucceededSiteNum + jobReport.FailedSiteNum + jobReport.SkipedSiteNum);
                objectsCount.Add(NodeLevel.List, jobReport.SucceededlistNum + jobReport.FailedListNum + jobReport.SkipedListNum);
                objectsCount.Add(NodeLevel.Folder, jobReport.SucceededFolderNum + jobReport.FailedFolderNum + jobReport.SkipedFolderNum);
                objectsCount.Add(NodeLevel.Item, jobReport.SucceededItemNum + jobReport.FailedItemNum + jobReport.SkipedItemNum);
                summaryList.AddRange(GetSummaryObjectsString(objectsCount, String.Empty));

                //job failed, 强制将所有object都设为failed,AOSBR-3855
                var forceToFailedObjects = new Dictionary<NodeLevel, Int32>();
                forceToFailedObjects.Add(NodeLevel.SiteCollection, jobReport.SucceededSiteCollectionNum);
                forceToFailedObjects.Add(NodeLevel.Site, jobReport.SucceededSiteNum);
                forceToFailedObjects.Add(NodeLevel.List, jobReport.SucceededlistNum);
                forceToFailedObjects.Add(NodeLevel.Folder, jobReport.SucceededFolderNum);
                forceToFailedObjects.Add(NodeLevel.Item, jobReport.SucceededItemNum);
                summaryList.AddRange(GetSummaryObjectsString(forceToFailedObjects, "Failed"));

                //succeededObjectsCount.Add(NodeLevel.WebApplication, 1);//Current is 1 webapp
                succeededObjectsCount.Add(NodeLevel.SiteCollection, 0);
                succeededObjectsCount.Add(NodeLevel.Site, 0);
                succeededObjectsCount.Add(NodeLevel.List, 0);
                succeededObjectsCount.Add(NodeLevel.Folder, 0);
                succeededObjectsCount.Add(NodeLevel.Item, 0);
                summaryList.AddRange(GetSummaryObjectsString(succeededObjectsCount, "Succeed"));

                skippedObjectsCount.Add(NodeLevel.SiteCollection, jobReport.SkipedSiteCollectionNum);
                skippedObjectsCount.Add(NodeLevel.Site, jobReport.SkipedSiteNum);
                skippedObjectsCount.Add(NodeLevel.List, jobReport.SkipedListNum);
                skippedObjectsCount.Add(NodeLevel.Folder, jobReport.SkipedFolderNum);
                skippedObjectsCount.Add(NodeLevel.Item, jobReport.SkipedItemNum);
                summaryList.AddRange(GetSummaryObjectsString(skippedObjectsCount, "Skipped"));

                return summaryList;
            }

            //var objectsCount = new Dictionary<NodeLevel, Int32>();
            //objectsCount.Add(NodeLevel.WebApplication, 1);
            objectsCount.Add(NodeLevel.SiteCollection, jobReport.SucceededSiteCollectionNum + jobReport.FailedSiteCollectionNum + jobReport.SkipedSiteCollectionNum);
            objectsCount.Add(NodeLevel.Site, jobReport.SucceededSiteNum + jobReport.FailedSiteNum + jobReport.SkipedSiteNum);
            objectsCount.Add(NodeLevel.List, jobReport.SucceededlistNum + jobReport.FailedListNum + jobReport.SkipedListNum);
            objectsCount.Add(NodeLevel.Folder, jobReport.SucceededFolderNum + jobReport.FailedFolderNum + jobReport.SkipedFolderNum);
            objectsCount.Add(NodeLevel.Item, jobReport.SucceededItemNum + jobReport.FailedItemNum + jobReport.SkipedItemNum);
            summaryList.AddRange(GetSummaryObjectsString(objectsCount, String.Empty));

            //var succeededObjectsCount = new Dictionary<NodeLevel, Int32>();
            //succeededObjectsCount.Add(NodeLevel.WebApplication, 1);//Current is 1 webapp
            succeededObjectsCount.Add(NodeLevel.SiteCollection, jobReport.SucceededSiteCollectionNum);
            succeededObjectsCount.Add(NodeLevel.Site, jobReport.SucceededSiteNum);
            succeededObjectsCount.Add(NodeLevel.List, jobReport.SucceededlistNum);
            succeededObjectsCount.Add(NodeLevel.Folder, jobReport.SucceededFolderNum);
            succeededObjectsCount.Add(NodeLevel.Item, jobReport.SucceededItemNum);
            summaryList.AddRange(GetSummaryObjectsString(succeededObjectsCount, "Succeed"));

            var failedObjectsCount = new Dictionary<NodeLevel, Int32>();
            failedObjectsCount.Add(NodeLevel.SiteCollection, jobReport.FailedSiteCollectionNum);
            failedObjectsCount.Add(NodeLevel.Site, jobReport.FailedSiteNum);
            failedObjectsCount.Add(NodeLevel.List, jobReport.FailedListNum);
            failedObjectsCount.Add(NodeLevel.Folder, jobReport.FailedFolderNum);
            failedObjectsCount.Add(NodeLevel.Item, jobReport.FailedItemNum);
            summaryList.AddRange(GetSummaryObjectsString(failedObjectsCount, "Failed"));

            //var skippedObjectsCount = new Dictionary<NodeLevel, Int32>();
            skippedObjectsCount.Add(NodeLevel.SiteCollection, jobReport.SkipedSiteCollectionNum);
            skippedObjectsCount.Add(NodeLevel.Site, jobReport.SkipedSiteNum);
            skippedObjectsCount.Add(NodeLevel.List, jobReport.SkipedListNum);
            skippedObjectsCount.Add(NodeLevel.Folder, jobReport.SkipedFolderNum);
            skippedObjectsCount.Add(NodeLevel.Item, jobReport.SkipedItemNum);
            summaryList.AddRange(GetSummaryObjectsString(skippedObjectsCount, "Skipped"));

            return summaryList;
        }

        private List<JobSummary> GetSummaryObjectsString(Dictionary<NodeLevel, Int32> objectsCount, String jobMessage)
        {
            List<JobSummary> summaryList = new List<JobSummary>();

            foreach (var item in objectsCount)
            {
                switch (item.Key)
                {
                    case NodeLevel.WebApplication:
                        summaryList.Add(new JobSummary() { Key = jobMessage + GConstants.JobSummaryKey.WebAppCount, Value = "" + item.Value });
                        break;
                    case NodeLevel.SiteCollection:
                        summaryList.Add(new JobSummary() { Key = jobMessage + GConstants.JobSummaryKey.SiteCollectionCount, Value = "" + item.Value });
                        break;
                    case NodeLevel.Site:
                        summaryList.Add(new JobSummary() { Key = jobMessage + GConstants.JobSummaryKey.SiteCount, Value = "" + item.Value });
                        break;
                    case NodeLevel.List:
                        summaryList.Add(new JobSummary() { Key = jobMessage + GConstants.JobSummaryKey.ListCount, Value = "" + item.Value });
                        break;
                    case NodeLevel.Item:
                        summaryList.Add(new JobSummary() { Key = jobMessage + GConstants.JobSummaryKey.ItemCount, Value = "" + item.Value });
                        break;
                    default:
                        break;
                }
            }
            return summaryList;
        }

        private String GetJobStateName(Int32 status)
        {
            String jobStateName = String.Empty;
            switch (status)
            {
                case 3:
                    jobStateName = "Failed";
                    break;
                case 4:
                    jobStateName = "Stopped";
                    break;
                case 2:
                    jobStateName = "Finished";
                    break;
                case 7:
                    jobStateName = "Finished With Exception";
                    break;
                default:
                    jobStateName = "Unknown";
                    break;
            }
            return jobStateName;
        }

        private void FillFailedJobReport(ItemDetailMessage detailMessage)
        {
            switch (detailMessage.Type)
            {
                case "E":
                    this.jobReport.FailedSiteCollectionNum++;
                    break;
                case "W":
                    this.jobReport.FailedSiteNum++;
                    break;
                case "L":
                    this.jobReport.FailedListNum++;
                    break;
                case "F":
                    this.jobReport.FailedFolderNum++;
                    break;
                case "I":
                case "D":
                case "A":
                case "V":
                case "U":
                    this.jobReport.FailedItemNum++;
                    break;
                default:
                    break;
            }
        }

        private void FillSucceededJobReport(ItemDetailMessage detailMessage)
        {
            switch (detailMessage.Type)
            {
                case "E":
                    this.jobReport.SucceededSiteCollectionNum++;
                    break;
                case "W":
                    this.jobReport.SucceededSiteNum++;
                    break;
                case "L":
                    this.jobReport.SucceededlistNum++;
                    break;
                case "F":
                    this.jobReport.SucceededFolderNum++;
                    break;
                case "I":
                case "D":
                case "A":
                case "V":
                case "U":
                    this.jobReport.SucceededItemNum++;
                    break;
                default:
                    break;
            }
        }

        private void FillSkipedJobReport(ItemDetailMessage detailMessage)
        {
            switch (detailMessage.Type)
            {
                case "E":
                    this.jobReport.SkipedSiteCollectionNum++;
                    break;
                case "W":
                    this.jobReport.SkipedSiteNum++;
                    break;
                case "L":
                    this.jobReport.SkipedListNum++;
                    break;
                case "F":
                    this.jobReport.SkipedFolderNum++;
                    break;
                case "I":
                case "D":
                case "A":
                case "V":
                case "U":
                    this.jobReport.SkipedItemNum++;
                    break;
                default:
                    break;
            }
        }

        private void FillJobDetail(ItemDetailMessage detailMessage)
        {
            this.currentJobDetail.MediaHost = jobReport.MediaAddress;
            this.currentJobDetail.SrcURL = detailMessage.Name;
            this.currentJobDetail.Size = detailMessage.ContentLength;
            this.currentJobDetail.Status = detailMessage.Status;
            this.currentJobDetail.Message = detailMessage.Message;
            this.currentJobDetail.EntityType = detailMessage.EntityType;
            this.currentJobDetail.Option = detailMessage.Action;
            this.currentJobDetail.Title = detailMessage.Title;
            this.currentJobDetail.Version = detailMessage.Version;
            switch (detailMessage.Type)
            {
                case "E":
                    currentJobDetail.Type = "Site Collection";
                    break;
                case "W":
                    currentJobDetail.Type = "Site";
                    break;
                case "L":
                    currentJobDetail.Type = "List";
                    break;
                case "D":
                case "V":
                    currentJobDetail.Type = "Document";
                    break;
                case "A":
                    currentJobDetail.Type = "Attachment";
                    break;
                case "F":
                    currentJobDetail.Type = "Folder";
                    break;
                case "I":
                case "U":
                    currentJobDetail.Type = "Item";
                    break;
                default:
                    break;
            }
            //currentJobDetail.DestURL = jobReport.Destination + "\\" + detailMessage.DirPath;
        }

        public void SetString8InJobTable(string tenantGroupId)
        {
            try
            {
                //TODO Records

                //var mArchiverJobManagementService = JobReportServiceFactory.CreateArchiverJobManagementService();
                //SOJobStatistics statistics = new SOJobStatistics();
                //statistics.Successful = jobReport.SucceededItemNum;
                //statistics.Failed = jobReport.FailedItemNum;
                //statistics.Skipped = jobReport.SkipedItemNum;
                //statistics.TotalCount = statistics.Successful + statistics.Failed + statistics.Skipped;
                //var value = SerializerHelper.SerializeByDataContractSerializer(statistics, statistics.GetType());
                //mArchiverJobManagementService.UpdateEndUserJobStatisticsByJobId(jobReport.JobID.Split('_')[0], value, tenantGroupId);
            }
            catch (Exception e)
            {
                logger.Warn("UpdateEndUserJobStatisticsByJobId error:{0}", e.ToString());
            }
        }
    }
}
