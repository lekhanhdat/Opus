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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RAFileSystem.Disposal.DAL
{
    public class DataSyncRecordsSerializer
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IProgressService progressService;
        private IReportService<JMJobDetails> reportServcie;
        private MemoryListCacheService<FileSystemRecordDto> cachedRecords;
        private HybridApiClient hybridApi;
        public DataSyncRecordsSerializer()
        {
            cachedRecords = new MemoryListCacheService<FileSystemRecordDto>();
            progressService = JobContext.Current.ProgressManager.Create();
            reportServcie = JobContext.Current.JobDetailManager.Create();
            hybridApi = new HybridApiClient(RMGlobalConfiguration.AppConfig[RMAppSettingKey.TenantId]);
        }

        public void Run()
        {
            try
            {
                Thread.CurrentThread.Name = string.Format("PersistThread[{0}]", Thread.CurrentThread.ManagedThreadId);
                FSJobCache.Instance.SerializerThreadMonitor.Increment();
                while (true)
                {
                    if (FSJobCache.Instance.RecordCache.Count == 0 && FSJobCache.Instance.AnalyzerThreadMonitor.Count == 0)
                    {
                        logger.Info("There is no records to be store. AND there is no analyzer thread running. Persist Thread [{0}] Exiting....", Thread.CurrentThread.Name);
                        break;
                    }
                    if (FSJobCache.Instance.RecordCache.Count == 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    IEnumerable<FileSystemRecordDto> temp = FSJobCache.Instance.RecordCache.Take(100);
                    logger.Info("RecordsSerializer got {1} records. There are {0} records to be stored left in the cache.", FSJobCache.Instance.RecordCache.Count, temp.Count());
                    //int count = temp.Count(t => string.IsNullOrEmpty(t.RecordsId) && (t.NodeType == 2100 || t.NodeType == 2200));
                    //AllocateRecordId(count);
                    if (temp != null && temp.Count() > 0)
                    {
                        //using (new RAPerformanceScope(string.Format("RecordsSerializer--Save {0} Records", temp.Count())))
                        {
                            foreach (FileSystemRecordDto item in temp)
                            {
                                try
                                {
                                    //if (string.IsNullOrEmpty(item.RecordsId) && (item.NodeType == 2100 || item.NodeType == 2200))
                                    //{
                                    //    item.RecordsId = FSJobCache.Instance.UniqueIdService.Next();
                                    //}
                                    AddRecordsToArchiverTable(item);
                                    progressService.Increase();
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Failed to store the record to db.  Exception:{0}", ex.ToString());
                                    reportServcie.Commit(new FSCollectJobReportEntry(item.LeafName, item.FullPath, AvePoint.GCommon.Contract.Server.Job.Object.JobReportDetailStatus.Failed, ex.Message));
                                }
                            }
                        }
                    }
                }
                FinalAddRecords();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to store records to the explorer database. Exception:{0}", ex.ToString());
                JobContext.Current.JobSummaryService.NotifyManager((int)AvePoint.Common.JobState.Failed, ex.Message);
            }
            finally
            {
                FSJobCache.Instance.SerializerThreadMonitor.Decrement();
            }
        }

        private void AddRecordsToArchiverTable(FileSystemRecordDto dto)
        {
            cachedRecords.Add(dto);
            if (cachedRecords.Count > 30)
            {
                var sendRecords = cachedRecords.Take(30).ToList();
                List<Guid> failedGuids = hybridApi.SyncData(sendRecords);
                SendReports(sendRecords, failedGuids);
            }
        }

        private void FinalAddRecords()
        {
            if (cachedRecords.Count > 0)
            {
                var sendRecords = cachedRecords.TakeAll().ToList();
                List<Guid> failedGuids = hybridApi.SyncData(sendRecords);
                SendReports(sendRecords, failedGuids);
            }
        }

        private void SendReports(List<FileSystemRecordDto> sendRecords, List<Guid> failedGuids)
        {
            List<FSCollectJobReportEntry> reports = new List<FSCollectJobReportEntry>();
            sendRecords.ForEach(r =>
            {
                FSCollectJobReportEntry report;
                if (failedGuids.Contains(r.NodeId))
                {
                    report = new FSCollectJobReportEntry(r.LeafName, r.FullPath, JobReportDetailStatus.Failed, "Failed to add fs record to explorer db.");
                }
                else
                {
                    report = new FSCollectJobReportEntry(r.LeafName, r.FullPath, JobReportDetailStatus.Success);
                }
                reports.Add(report);
            });
            reportServcie.CommitBatch(reports);
        }

        private void AllocateRecordId(int count)
        {
            // using (new RAPerformanceScope("RecordsSerializer---AllocateRecordId"))
            {
                try
                {
                    FSJobCache.Instance.UniqueIdService.Allocate(count);
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to allocate uniqueid. Exception:{0}", ex.ToString());
                }
            }
        }
    }
}
