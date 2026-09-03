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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class RMTrimImportedRecordDeletion
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMTrimImportedRecordDeletion));
        #region Job Param
        private JobType jobType;
        private string jobRunBy;
        private string mCurrentJobId; 
        private AvePoint.RA.SharePoint.Object.JobResult Result;
        private string physicalRecordsTxtPath;

        private string commomErrorMessage = "RM_TS_SS_Summary"; 


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
        #endregion


        public IExplorerDao ExplorerDao { set; get; } = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
        public IRMManagedRecordRelatedDao recordRelatedDao { set; get; } = PlatformWindsorManager.GetService<IRMManagedRecordRelatedDao>();
        public IRecordLoanAllianceDao RecordLoanAllianceDao { set; get; } = PlatformWindsorManager.GetService<IRecordLoanAllianceDao>();


        public RMTrimImportedRecordDeletion(RMImportJobMessage msg)
        {
            this.jobType = msg.JobType;
            this.jobRunBy = msg.JobRunBy;
            mCurrentJobId = msg.JobID; 
            ReportMangerFactory.Instance.Init(mCurrentJobId, this.jobType); 
            Result = new AvePoint.RA.SharePoint.Object.JobResult(); 
            switch (jobType)
            {
                case JobType.TrimRecordsDeletion:
                    #region ImportPhysicalRecords
                    //physicalRecordsCSVPath = msg.PhysicalRecordsCSVPath;

                    try
                    {
                        physicalRecordsTxtPath = JobReportUtility.GetImportJobCSVFile(msg.PhysicalRecordsCSVPath);
                    }
                    catch (Exception e)
                    {
                        logger.Error("can not download file:{0},error:{1}", msg.PhysicalRecordsCSVPath, e.ToString());
                        throw;
                    }

                    #endregion
                    break;
                default:
                    break;
            }

            //默认初始化 进度为2
            ReportManager.Increase(2);
            ReportManager.StartUpdateJobProgress();
        }

        public async Task ExecuteAsync()
        {
            logger.Info("Start to process deletion.");
            List<string> datas = ReadUniqueIdList();
            logger.Info("UniqueId count {0}", datas.Count);

            JobStatus status = JobStatus.None;
            try
            {
                if (datas.Count == 0)
                {
                    throw new InvalidDataException("No available content in txt file");
                }
                if (datas.Count <= 100)
                {
                    await BatchDeletionAsync(datas);
                }
                else
                {
                    int start = 0;
                    int pageSize = 100;
                    while (start < datas.Count)
                    {
                        List<string> temp = datas.Skip(start).Take(pageSize).ToList();
                        start += pageSize;
                        await BatchDeletionAsync(temp);
                    }

                }
                status = Result.HasFailed
                   ? Result.HasSuccessful
                       ? JobStatus.FinishWithException
                       : JobStatus.Failed
                   : JobStatus.Finished;
                System.IO.File.Delete(physicalRecordsTxtPath);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {

                string jobComment = (status == JobStatus.FinishWithException || status == JobStatus.Failed)
                    ? commomErrorMessage
                    : string.Empty;
                ReportManager.SetJobFinished(status, jobComment); 
            }
        }

        private List<string> ReadUniqueIdList()
        {
            List<string> datas = new List<string>();
            try
            {
                using (FileStream fs = new FileStream(physicalRecordsTxtPath, FileMode.Open, FileAccess.Read))
                {
                    if (physicalRecordsTxtPath.EndsWith("txt", StringComparison.OrdinalIgnoreCase))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            while (!sr.EndOfStream)
                            {
                                string csvLine = sr.ReadLine();
                                if (csvLine != null && csvLine != string.Empty)
                                {
                                    datas.Add(csvLine.Trim());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw new Exception("Failed to read file conntent");
            }
            return datas;
        }


        public async Task BatchDeletionAsync(List<string> datas)
        {
            List<Record> list = ExplorerDao.QueryAll(a => a.ScopeId == Guid.Empty && datas.Contains(a.RecordsId)).ToList();
            if(datas.Count > list.Count)
            {
                List<string> unavailableIds = datas.Where(a => !list.Any(l => l.RecordsId == a)).ToList();
                SendNotFoundReport(unavailableIds);
            }
            List<Record> records = list.Where(a => a.NodeType == (int)RMNodeType.PhyRecord).ToList();
            logger.Info("Total records found by uniqueId file is {0}", records.Count);
            ProcessRecords(records);
            await ProcessContainerAsync(list);
        }

        private void ProcessRecords(List<Record> records)
        {
            foreach(Record rec in records)
            {
                JMImportedPhysicalRecordsDeletionDetail detail = new JMImportedPhysicalRecordsDeletionDetail() { ObjectName = rec.LeafName, UniqueId = rec.RecordsId };
                try
                {
                    logger.Info("Deleting record, unique id {0}, level:{1} id {2}", rec.RecordsId,rec.NodeType, rec?.Id);
                    ExplorerDao.Delete(rec.CreateDate, rec.Id);
                    detail.Status = JobDetailsStatus.Successful;
                    Result.HasSuccessful = true;
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                    detail.Status = JobDetailsStatus.Failed;
                    Result.HasFailed = true;
                    detail.Comment = e.Message;
                }
                ReportManager.SendJobDetail(detail);
            }
        }

        private async Task ProcessContainerAsync(List<Record> list)
        {
            List<Record> folders = list.Where(a => a.NodeType == (int)RMNodeType.PhyFile).ToList();
            logger.Info("Folder count {0}", folders.Count);
            if (await RemoveFolderLoanInformationAsync(folders))
            {
                ProcessFolder(folders);
            }
            else
            {
                logger.Warn("Failed to remove load information. send failed report");
            }
            List<Record> boxes = list.Where(a => a.NodeType == (int)RMNodeType.PhyBox).ToList();
            logger.Info("Box count {0}", boxes.Count);
            await ProcessBoxAsync(boxes);
            List<Record> customs = list.Where(a => a.NodeType == (int)RMNodeType.PhyCustom).ToList();
            logger.Info("Custom rec count {0}", customs.Count);
            await ProcessCustomAsync(customs);
        }

        private void ProcessFolder(List<Record> list)
        {
            foreach (Record rec in list)
            {
                JMImportedPhysicalRecordsDeletionDetail detail = new JMImportedPhysicalRecordsDeletionDetail() { ObjectName = rec.LeafName, UniqueId = rec.RecordsId };
                try
                {
                    List<Record> recInFolders = ExplorerDao.QueryAll(a => a.ScopeId == Guid.Empty && a.FolderId == rec.Id).ToList();
                    logger.Info("Records in folder {0}, count is {1}", rec.RecordsId, recInFolders.Count);
                    foreach(Record sub in recInFolders)
                    {
                        logger.Info("Deleting record in folder, unique id {0}, level:{1} id {2}", rec.RecordsId, rec.NodeType, rec?.Id);
                        ExplorerDao.Delete(rec.CreateDate, rec.Id);
                    }
                    logger.Info("Deleting folder, unique id {0}, level:{1} id {2}", rec.RecordsId, rec.NodeType, rec?.Id);
                    ExplorerDao.Delete(rec.CreateDate, rec.Id);
                    detail.Status = JobDetailsStatus.Successful;
                    Result.HasSuccessful = true;
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                    detail.Status = JobDetailsStatus.Failed;
                    Result.HasFailed = true;
                    detail.Comment = e.Message;
                }
                ReportManager.SendJobDetail(detail);
            }
        }
        private async Task ProcessBoxAsync(List<Record> list)
        {
            if (list.IsNullOrEmpty())
            {
                logger.Info("No box");
                return;
            }
            int nodeType_Folder = (int)RMNodeType.PhyFile; 
            int nodeType_Record = (int)RMNodeType.PhyRecord; 
            foreach (Record box in list)
            { 
                JMImportedPhysicalRecordsDeletionDetail detail = new JMImportedPhysicalRecordsDeletionDetail() { ObjectName = box.LeafName, UniqueId = box.RecordsId };
                List<Record> recFolders = ExplorerDao.QueryAll(a => a.ScopeId == Guid.Empty && a.BoxId == box.Id && a.NodeType == nodeType_Folder ).ToList();
                logger.Info("Folder count in box {0}, is {1}", box.RecordsId, recFolders.Count);
                if (!await RemoveFolderLoanInformationAsync(recFolders))
                {
                    logger.Warn("Failed to remove loan information");
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = "Failed to remove loan information.";
                    ReportManager.SendJobDetail(detail);
                    continue;
                } 
                List<Record> recRecords = ExplorerDao.QueryAll(a => a.ScopeId == Guid.Empty && a.BoxId == box.Id && a.NodeType == nodeType_Record).ToList();
                try
                {
                    logger.Info("Start to delete records in box {0}, count {1}", box.RecordsId, recRecords.Count);
                    foreach (Record rec in recRecords)
                    {
                        ExplorerDao.Delete(rec.CreateDate, rec.Id);
                    }
                    logger.Info("Start to delete folders in box {0}, count {1}", box.RecordsId, recFolders.Count);
                    foreach (Record rec in recFolders)
                    {
                        ExplorerDao.Delete(rec.CreateDate, rec.Id);
                    }
                }
                catch (Exception e)
                {
                    Result.HasFailed = true;
                    logger.Error(e.Message, e); 
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = "Failed to remove sub records in box.";
                    ReportManager.SendJobDetail(detail);
                    continue;
                }
                try
                {
                    logger.Info("Start to delete box {0}", box.RecordsId);
                    ExplorerDao.Delete(box.CreateDate, box.Id);
                    detail.Status = JobDetailsStatus.Successful;
                    Result.HasSuccessful = true;
                }
                catch (Exception e)
                {
                    Result.HasFailed = true;
                    logger.Error(e.Message, e);
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = "Failed to remove box. e:" + e.Message; 
                }
                ReportManager.SendJobDetail(detail);

            }
        }

        private async Task ProcessCustomAsync(List<Record> list)
        {
            if (list.IsNullOrEmpty())
            {
                logger.Info("No custom record");
                return;
            }
            //int nodeType_Folder = (int)RMNodeType.PhyFile;
            //int nodeType_Record = (int)RMNodeType.PhyRecord;
            foreach (Record custom in list)
            {
                JMImportedPhysicalRecordsDeletionDetail detail = new JMImportedPhysicalRecordsDeletionDetail() { ObjectName = custom.LeafName, UniqueId = custom.RecordsId };
                if(!await ProcessSubCustomAsync(custom, detail))
                {
                    continue;
                }
                try
                {
                    logger.Info("Start to delete box {0}", custom.RecordsId);
                    ExplorerDao.Delete(custom.CreateDate, custom.Id);
                    detail.Status = JobDetailsStatus.Successful;
                    Result.HasSuccessful = true;
                }
                catch (Exception e)
                {
                    Result.HasFailed = true;
                    logger.Error(e.Message, e);
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = "Failed to remove box. e:" + e.Message;
                }
                ReportManager.SendJobDetail(detail);

            }
        }

        private async Task<bool> ProcessSubCustomAsync(Record custom, JMImportedPhysicalRecordsDeletionDetail detail)
        {
            List<Record> subRecords = ExplorerDao.QueryAll(a=>a.Ancestors.Contains(custom.Id)).ToList();
            logger.Info("Folder sub cutom count in parent {0}, is {1}", custom.RecordsId, subRecords.Count);
            if (!await RemoveFolderLoanInformationAsync(subRecords.Where(a=>a.NodeType == (int)RMNodeType.PhyFile).ToList()))
            {
                logger.Warn("Failed to remove loan information");
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = "Failed to remove loan information.";
                ReportManager.SendJobDetail(detail);
                return false;
            }
            List<Record> recRecords = subRecords.Where(a => a.NodeType == (int)RMNodeType.PhyFile).ToList();
            try
            {
                logger.Info("Start to delete sub in custom rec {0}, count {1}", custom.RecordsId, subRecords.Count);
                foreach (Record rec in subRecords)
                {
                    ExplorerDao.Delete(rec.CreateDate, rec.Id);
                }
            }
            catch (Exception e)
            {
                Result.HasFailed = true;
                logger.Error(e.Message, e);
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = "Failed to remove sub records.";
                ReportManager.SendJobDetail(detail);
                return false;
            }
            return true;
        }
        private async Task<bool> RemoveFolderLoanInformationAsync(List<Record> folders)
        {
            try
            {
                if(folders.Count > 0)
                {
                    List<Guid> ids = folders.Select(a => a.Id).ToList();
                    await RecordLoanAllianceDao.BatchDeleteRecordAllianceByIdsAsync(ids);
                }
            }
            catch (Exception e)
            {
                Result.HasFailed = true;
                logger.Warn(e.Message, e);
                return false;
            }
            return true;
        }

        private void SendNotFoundReport(List<string> unavailableIds)
        {
            foreach(string uniqueId in unavailableIds)
            {
                JMImportedPhysicalRecordsDeletionDetail detail = new JMImportedPhysicalRecordsDeletionDetail();
                detail.Status = JobDetailsStatus.Skipped;
                detail.UniqueId = uniqueId;
                detail.Comment = "Record not found.";
                ReportManager.SendJobDetail(detail);
            }
        }
    }
}
