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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using Newtonsoft.Json;
using RACloudFS.FSFolderJob;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGlobalSearch.Actions
{
    public class PhysicalBulkUpdateAction : IGlobalSearchAction
    {
        private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(PhysicalBulkUpdateAction));

        private IExplorerService mExplorerService;
        public IExplorerService ExplorerService
        {
            get
            {
                if (mExplorerService == null)
                {
                    mExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
                }
                return mExplorerService;
            }
        }
        private IExplorerDao _explorerDao;
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao();
                }
                return _explorerDao;
            }
        }
        private IRMKeyValueDao mRMKeyValueDao;
        public IRMKeyValueDao RMKeyValueDao
        {
            get
            {
                if (mRMKeyValueDao == null)
                {
                    mRMKeyValueDao = (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
                }
                return mRMKeyValueDao;
            }
        }

        private static readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private int mFailedCount = 0;
        private int mSuccessCount = 0;

        public PhysicalBulkUpdateAction()
        {
        }
        public Task DoActionAsync(List<BaseRecordDto> recordDtos, SourceFlag flag, object actionExtension, string jobId, bool isJob)
        {
            logger.Info("Start process physical bulk update action.");
            try
            {
                var bulkMetaInfoDic = SerializerHelper.DeserializeByDataContractSerializer<Dictionary<string, string>>(actionExtension.ToString());
                List<Guid> failedDataGuid = new List<Guid>();
                List<Guid> skipDataGuid = new List<Guid>();

                var recordIds = recordDtos.Select(r => r.Id).ToList();
                var records = ExplorerDao.GetRecordByIds(recordIds);
                foreach (var record in records)
                {
                    //if (record.RecordStatus == (int)RMRecordStatus.Destroyed)
                    //{
                    //    skipDataGuid.Add(record.Id);
                    //    ProcessDetail(record, JobDetailsStatus.Skipped);
                    //}
                    var metaInfo = string.IsNullOrEmpty(record.MetaInfo) ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(record.MetaInfo);
                    foreach (var bulkColumnId in bulkMetaInfoDic.Keys)
                    {
                        if (metaInfo != null)
                        {
                            var bulkColumn = bulkMetaInfoDic[bulkColumnId];
                            metaInfo[bulkColumnId] = bulkColumn;
                            var modifedBy = (AccountDao.GetUserByUserIdAsync(TenantLocalValue.LogonUserId).GetAwaiter().GetResult()).DisplayName;
                            record.ModifiedBy = modifedBy;
                            record.TimeModified = DateTime.UtcNow.Ticks;
                        }
                    }
                    record.MetaInfo = JsonConvert.SerializeObject(metaInfo);
                }
                var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                if (bulkSize == default)
                {
                    bulkSize = CosmosBulkOperator.DefualtBufferSize;
                }
                logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                failedDataGuid = ExplorerDao.BatchUpdate(records, bulkSize);
                foreach (var rec in records)
                {
                    if (failedDataGuid.Contains(rec.Id))
                    {
                        ProcessDetail(rec, JobDetailsStatus.Failed);
                        mFailedCount++;
                    }
                    else
                    {
                        ProcessDetail(rec, JobDetailsStatus.Successful);
                        mSuccessCount++;
                    }
                }
            }
            catch (Exception e)
            {
                mFailedCount++;
                logger.Error($"An error occurred while doing physical bulk update. Error:{e}");
            }
            logger.Info("Process physical bulk update action finished.");
            return Task.CompletedTask;
        }

        private void ProcessDetail(Record record, JobDetailsStatus status, string comment = "")
        {
            var dirPath = ExplorerService.GetPhysicalObjectFullPath(record.Id) + "/" + record.LeafName;
            var typeString = "";
            switch ((RMNodeLevel)record.NodeType)
            {
                case RMNodeLevel.PhysicalBox:
                    typeString = "RM_Common_ObjectLevel_PhysicalBox";
                    break;
                case RMNodeLevel.PhysicalFile:
                    typeString = "RM_JS_Rule_ObjectLevel_PhysicalFile";
                    break;
                case RMNodeLevel.PhysicalRecord:
                    typeString = "RM_JS_Rule_ObjectLevel_PhysicalRecord";
                    break;
                default:
                    break;
            }
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = record.LeafName,
                FullPath = dirPath,
                Action = "RM_JM_GlobalSearch_PhysicalUpdate",
                Type = typeString,
                Status = status,
                Comment = comment
            });
            ReportMangerFactory.Instance.ReportManager.Increase(1);
        }

        public int GetSuccessCount()
        {
            return mSuccessCount;
        }

        public int GetFailedCount()
        {
            return mFailedCount;
        }
    }
}
