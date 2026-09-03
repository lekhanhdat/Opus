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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cloud;

using AvePoint.PhysicalCore.SQL;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using Microsoft.SharePoint.Client;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class DisposalRelatedItemUtility
    {
        public static string DisposeRelatedItemsForArchiveAndRemove(CallProcess callProcess, ScheduleConfiguration Configuration, Rule currentRule, string recordRelatedValue, SendReportHandler SendReportHandler)
        {
            //StringBuilder jobIds = new StringBuilder();
            //var request = new EndUserRequest();
            //var relatedItems = RelatedRecordsUtility.GetRelatedProperties(recordRelatedValue);
            //foreach (var itemInfo in relatedItems)
            //{
            //    //老数据并没有存SourceFlag， 所以使用0 记录
            //    if (itemInfo.SourceFlag == (int)SOSourceFlag.SharePoint || itemInfo.SourceFlag == 0)
            //    {
            //        if (Configuration.soArchiverQueryWorker != null && Configuration.soArchiverQueryWorker.TryGetCurrentVersionInTable(itemInfo.id))
            //        {
            //            mLog.Info("Current related item fit rule in this job so skip it.FilePath:{0}.RuleID:{1}.", itemInfo.url, itemInfo.id);
            //            continue;
            //        }
            //        string jobMetadata = string.Empty;
            //        string jobId = callProcess.GenerateJobId(ArchiveConstants.EndUserJob);
            //        string planId = "PLAN" + callProcess.GeneratePlanId();
            //        var endUserContract = request.GetEndUserArchiverContract(itemInfo, currentRule.Id, jobMetadata);
            //        var msg = Configuration.CloneArchiveMessageFromCurrentJob();
            //        msg.Action = ArchiverAction.ENDUSER_ARCHIVER_BACKUP_JOB_REQUEST;
            //        msg.EndUserArchiverMetaData = endUserContract.MetaData;
            //        msg.SubJobId = jobId + "_000";
            //        msg.RunDAOArchiverJobProduct = 1;
            //        msg.Job.Id = jobId;
            //        msg.Job.PlanId = planId;
            //        msg.Job.Scope = itemInfo.url;
            //        if (msg.ArchiverBackupRequest == null)
            //        {
            //            msg.ArchiverBackupRequest = new GCommon.Contract.Media.TCPRequest.Backup.ArchiverBackupRequest();
            //        }
            //        msg.ArchiverBackupRequest.PlanId = planId;
            //        msg.ArchiverBackupRequest.ParentJobId = jobId;
            //        if (msg.ArchiverBackupRequest.IndexLogicalDevice == null)
            //        {
            //            msg.ArchiverBackupRequest.IndexLogicalDevice = msg.PhysicalRecordsLogicalDevice;
            //        }
            //        msg.ArchiverBackupRequest.Rules = new Dictionary<string, GCommon.Contract.StorageOptimization.Object.Rule>();
            //        //Physical Related SP, StoragePolicyDto is null and need get encrypt/compress
            //        currentRule = AddRecordsGlobalStorageSettingsToPhysicalRule(currentRule, msg.RecordsGlobalStorageSettingsDto);
            //        msg.ArchiverBackupRequest.Rules.Add(currentRule.Id, currentRule);
            //        msg.ArchiverBackupRequest.ArchiverSiteInfoDto = new GCommon.Contract.Storage.Entity.ArchiverSiteInfoDto();

            //        msg.ArchiverBackupRequest.ArchiverSiteInfoDto.SiteUrl = itemInfo.SiteUrl;
            //        msg.ArchiverBackupRequest.ArchiverSiteInfoDto.NewSiteUrl = itemInfo.SiteUrl;
            //        //Set site id here, but the site id may be wrong after backup and restore job
            //        //DAOAPIClient client = new DAOAPIClient(msg.TenantGroupId, msg.TenantGroupOwner);
            //        Guid mDAOSiteID = Guid.Empty;
            //        string mDAOGroupID = string.Empty;
            //        var daoSite = Configuration.GetRemoteSiteCollectionByDAO(itemInfo.SiteUrl);
            //        //Configuration.isRAJob ? Configuration.GetRemoteSiteCollectionByRecords(itemInfo.SiteUrl) : Configuration.GetRemoteSiteCollectionByDAO(itemInfo.SiteUrl);
            //        mLog.Info("End User Job ArchiverMessage:" + jobId + ".  " + SerializerHelper.SerializeByDataContractSerializer(msg));
            //        if (!CheckRelatedRecordExist(msg))
            //        {
            //            mLog.Warn("Related record:{0} does not exist in Share Point, skip this record.", itemInfo.url);
            //            continue;
            //        }
            //        string archiveMsgFolder = AveEnv.AgentTempFolder + "\\" + msg.SubJobId;
            //        string msgFileName = "jobInfo.dat";
            //        string archiveMessagePath = archiveMsgFolder + "\\" + msgFileName;
            //        callProcess.WriteArchiveMsgToLocal(archiveMsgFolder, msgFileName, msg);
            //        mLog.Info("Current Agent machine SPStorageOptimizationMessageCenter.exe count is:{0}.", Process.GetProcessesByName("SPStorageOptimizationMessageCenter").Count());
            //        while (Process.GetProcessesByName("SPStorageOptimizationMessageCenter").Count() > 10)
            //        {
            //            mLog.Info("Current machine SPStorageOptimizationMessageCenter.exe count over 10 and wait processor.");
            //            Thread.Sleep(30 * 1000);
            //        }
            //        callProcess.StartSOMessageCenterProcess(ArchiveConstants.EndUserJob, archiveMessagePath);
            //        if (!string.IsNullOrEmpty(msg.SubJobId))
            //        {
            //            jobIds.Append(msg.SubJobId);
            //            jobIds.Append(";");
            //        }
            //        else
            //        {
            //            throw new Exception("Cannot start remove related item job");
            //        }
            //    }
            //    else if (itemInfo.SourceFlag == (int)SOSourceFlag.PhysicalObject)
            //    {
            //        PhyscialDisposalAction action = new PhyscialDisposalAction(DateTime.UtcNow);
            //        //PhyBox = 9300,PhyFile = 9400,PhyRecord = 9500,
            //        if (itemInfo.NodeType == 9400)
            //        {
            //            var file = new PhysicalFile(itemInfo.id);
            //            if (!RecordsDBOperation.IsRecordsHold(file, DateTime.UtcNow.Ticks))
            //            {
            //                    action.DisposalFile(file, currentRule, SendReportHandler, false);
            //            }
            //            else
            //            {
            //                mLog.Info("Current physical folder IsRecordsHold,DirPath:{0}.", file.Id);
            //                SendReportHandler(file.Name, file.DirPath, PhysicalDisposalActionType.Disposal, String.Empty, ((int)RMNodeLevel.PhysicalFile).ToString(), JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldFolder");
            //            }
            //        }
            //        //当前不会关联box ，所以写在这为了以后可能用到
            //        else if (itemInfo.NodeType == 9300)
            //        {
            //            var box = new PhysicalBox(itemInfo.id);
            //            action.DisposalBox(box, currentRule, SendReportHandler);
            //        }
            //        else if (itemInfo.NodeType == 9500)
            //        {
            //            var record = new PhysicalRecord(itemInfo.id);
            //            if (!RecordsDBOperation.IsRecordsHold(record.ParentFile, DateTime.UtcNow.Ticks))
            //            {
            //                action.DisposalRecord(record, currentRule, false);
            //                SendReportHandler(record.Name, record.DirPath, PhysicalDisposalActionType.Disposal, String.Empty, ((int)RMNodeLevel.PhysicalRecord).ToString(), JobDetailsStatus.Successful, "");
            //            }
            //            else
            //            {
            //                mLog.Info("Current physical folder IsRecordsHold,DirPath:{0}.", record.Id);
            //                SendReportHandler(record.Name, record.DirPath, PhysicalDisposalActionType.Disposal, String.Empty, ((int)RMNodeLevel.PhysicalRecord).ToString(), JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldRecord");
            //            }
            //        }
            //    }
            //}
            //return jobIds.ToString().TrimEnd(';');
            return string.Empty;
        }

        /*public static bool DisposeRelatedItemsForDeleteOnly(ScheduleConfiguration Configuration, string recordRelatedValue, Rule currentRule, SendReportHandler SendReportHandler)
        {
            bool hasFailedDeleteObject = false;
            //StringBuilder jobIds = new StringBuilder();
            //var request = new EndUserRequest();
            //var relatedItems = RelatedRecordsUtility.GetRelatedProperties(recordRelatedValue);
            //foreach (var itemInfo in relatedItems)
            //{
            //    JobDetailsStatus status = JobDetailsStatus.Successful;
            //    //老数据并没有存SourceFlag， 所以使用0 记录
            //    if (itemInfo.SourceFlag == (int)SOSourceFlag.SharePoint || itemInfo.SourceFlag == 0)
            //    {
            //        if (Configuration.soArchiverQueryWorker.TryGetCurrentVersionInTable(itemInfo.id))
            //        {
            //            mLog.Info("Current related item fit rule in this job so skip it.FilePath:{0}.RuleID:{1}.", itemInfo.url, itemInfo.id);
            //            continue;
            //        }
            //        SPObjectDeleteUtility utility = SPObjectDeleteUtility.GetInstance(Configuration);
            //        utility.Init(itemInfo.SiteUrl, itemInfo.WebId, itemInfo.ListId, Configuration);
            //        if (Configuration.CheckItemIsRecordsHold(itemInfo.id))
            //        {
            //            mLog.Info("Current related item is on hold.FilePath:{0}.RuleID:{1}.", itemInfo.url, itemInfo.id);
            //            hasFailedDeleteObject = true;
            //        }
            //        else
            //        {
            //            if (itemInfo.level == SOEndUserArchiverNodeLevel.Document)
            //            {
            //                status = utility.DeleteDocument(itemInfo.id, itemInfo.url, SendReportHandler);
            //            }
            //            else
            //            {
            //                status = utility.DeleteListItem(itemInfo.id, itemInfo.DocLibRowId, itemInfo.url, SendReportHandler);
            //            }
            //        }
            //    }
            //    else if (itemInfo.SourceFlag == (int)SOSourceFlag.PhysicalObject)
            //    {
            //        PhyscialDisposalAction action = new PhyscialDisposalAction(DateTime.UtcNow, Configuration);
            //        //PhyBox = 9300,PhyFile = 9400,PhyRecord = 9500,
            //        if (itemInfo.NodeType == 9400)
            //        {
            //            var file = new PhysicalFile(itemInfo.id);
            //            if (!RecordsDBOperation.IsRecordsHold(file, DateTime.UtcNow.Ticks))
            //            {
            //                action.DisposalFile(file, currentRule, SendReportHandler, false);
            //            }
            //            else
            //            {
            //                mLog.Info("Current physical folder IsRecordsHold,DirPath:{0}.", file.Id);
            //                SendReportHandler(file.Name, file.DirPath, PhysicalDisposalActionType.Disposal, String.Empty, ((int)RMNodeLevel.PhysicalFile).ToString(), JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldFolder");
            //            }
            //        }
            //        //当前不会关联box ，所以写在这为了以后可能用到
            //        else if (itemInfo.NodeType == 9300)
            //        {
            //            var box = new PhysicalBox(itemInfo.id);
            //            action.DisposalBox(box, currentRule, SendReportHandler);
            //        }
            //        else if (itemInfo.NodeType == 9500)
            //        {
            //            var record = new PhysicalRecord(itemInfo.id);
            //            if (!RecordsDBOperation.IsRecordsHold(record.ParentFile, DateTime.UtcNow.Ticks))
            //            {
            //                action.DisposalRecord(record, currentRule, false);
            //                SendReportHandler(record.Name, record.DirPath, PhysicalDisposalActionType.Disposal, String.Empty, ((int)RMNodeLevel.PhysicalRecord).ToString(), JobDetailsStatus.Successful, "");
            //            }
            //            else
            //            {
            //                SendReportHandler(record.Name, record.DirPath, PhysicalDisposalActionType.Disposal, String.Empty, ((int)RMNodeLevel.PhysicalRecord).ToString(), JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldRecord");
            //            }
            //        }
            //    }
            //    if (status == JobDetailsStatus.Failed)
            //    {
            //        hasFailedDeleteObject = true;
            //    }
            //}
            return hasFailedDeleteObject;
        }*/
    }
}
