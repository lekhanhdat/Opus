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
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.SyncNode;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using RAExportCommon;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System.Data.SQLite;
using System.Text;
using AvePoint.RA.Contract.SyncNode.GoogleSyncNode;

namespace RMSynchronize.SyncNodeFromAOS.ChangeLog
{
    public class RMSyncNodeAzureChangeLogger
    {


        private readonly RMRetryer _retryer;

        private readonly bool _enable;

        private readonly string _jobId;

        private readonly string _reportFilePath;

        private readonly RMSyncNodeAzureChangeLogWorker _azureBlobWorker;

        public int ChangeLogsCount { get; private set; }

        public RMSyncNodeAzureChangeLogger(bool enable, string jobId)
        {
            _enable = enable;
            _retryer = RMRetryerBuilder.CreateBuilder().Build();
            _jobId = jobId;
            _azureBlobWorker = new(jobId, false);
            _reportFilePath = _azureBlobWorker.GetChangeLogReportPath(TenantLocalValue.LogonGroupId, false);
            _azureBlobWorker.CreateTableNew();
        }

        public async Task Record(IEnumerable<RMContainerInfoAdaption> nodes, SourceFlag contentSource, RMSyncNodeChangeType changeType)
        {
            if (!_enable)
            {
                return;
            }

            List<List<SQLiteParameter>> parameterList = new List<List<SQLiteParameter>>();
            foreach (var node in nodes)
            {
                parameterList.Add(_azureBlobWorker.BuildSQLiteParameters(node, contentSource, changeType));
                ChangeLogsCount++;
            }
            SQLCommond.BatchExecuteNonQueryStable(_reportFilePath, _azureBlobWorker.INSERT_DATE_SQL, parameterList);
        }

        public async Task RecordChangeName(RMContainerInfoAdaption node, SourceFlag contentSource, string beforeUrl, string changedUrl)
        {
            if (!_enable)
            {
                return;
            }

            List<List<SQLiteParameter>> parameterList = [_azureBlobWorker.BuildSQLiteParameters(node, contentSource, RMSyncNodeChangeType.ChangeName, beforeUrl, changedUrl)];
            ChangeLogsCount++;
            SQLCommond.BatchExecuteNonQueryStable(_reportFilePath, _azureBlobWorker.INSERT_DATE_SQL, parameterList);
        }

        public async Task Record(IEnumerable<RMSiteNodeAdaption> nodes, SourceFlag contentSource, RMSyncNodeChangeType changeType)
        {
            if (!_enable)
            {
                return;
            }

            List<List<SQLiteParameter>> parameterList = new List<List<SQLiteParameter>>();
            foreach (var node in nodes)
            {
                parameterList.Add(_azureBlobWorker.BuildSQLiteParameters(node, contentSource, changeType));
                ChangeLogsCount++;
            }
            SQLCommond.BatchExecuteNonQueryStable(_reportFilePath, _azureBlobWorker.INSERT_DATE_SQL, parameterList);
        }

        public async Task RecordChangeName(RMSiteNodeAdaption node, SourceFlag contentSource, string beforeUrl, string changedUrl)
        {
            if (!_enable)
            {
                return;
            }

            List<List<SQLiteParameter>> parameterList = [_azureBlobWorker.BuildSQLiteParameters(node, contentSource, RMSyncNodeChangeType.ChangeName, beforeUrl, changedUrl)];
            ChangeLogsCount++;
            SQLCommond.BatchExecuteNonQueryStable(_reportFilePath, _azureBlobWorker.INSERT_DATE_SQL, parameterList);
        }

        public async Task Record(IEnumerable<RMExchangeNodeAdaption> nodes, SourceFlag contentSource, RMSyncNodeChangeType changeType)
        {
            if (!_enable)
            {
                return;
            }

            List<List<SQLiteParameter>> parameterList = new List<List<SQLiteParameter>>();
            foreach (var node in nodes)
            {
                parameterList.Add(_azureBlobWorker.BuildSQLiteParameters(node, contentSource, changeType));
                ChangeLogsCount++;
            }
            SQLCommond.BatchExecuteNonQueryStable(_reportFilePath, _azureBlobWorker.INSERT_DATE_SQL, parameterList);
        }

        public async Task RecordChangeName(RMExchangeNodeAdaption node, SourceFlag contentSource, string beforeUrl, string changedUrl)
        {
            if (!_enable)
            {
                return;
            }

            List<List<SQLiteParameter>> parameterList = [_azureBlobWorker.BuildSQLiteParameters(node, contentSource, RMSyncNodeChangeType.ChangeName, beforeUrl, changedUrl)];
            ChangeLogsCount++;
            SQLCommond.BatchExecuteNonQueryStable(_reportFilePath, _azureBlobWorker.INSERT_DATE_SQL, parameterList);
        }

        public async Task Record(IEnumerable<RMGoogleNodeAdaption> nodes, SourceFlag contentSource, RMSyncNodeChangeType changeType)
        {
            if (!_enable)
            {
                return;
            }

            List<List<SQLiteParameter>> parameterList = new List<List<SQLiteParameter>>();
            foreach (var node in nodes)
            {
                parameterList.Add(_azureBlobWorker.BuildSQLiteParameters(node, contentSource, changeType));
                ChangeLogsCount++;
            }
            SQLCommond.BatchExecuteNonQueryStable(_reportFilePath, _azureBlobWorker.INSERT_DATE_SQL, parameterList);
        }

        public async Task RecordChangeName(RMGoogleNodeAdaption node, SourceFlag contentSource, string beforeUrl, string changedUrl)
        {
            if (!_enable)
            {
                return;
            }

            List<List<SQLiteParameter>> parameterList = [_azureBlobWorker.BuildSQLiteParameters(node, contentSource, RMSyncNodeChangeType.ChangeName, beforeUrl, changedUrl)];
            ChangeLogsCount++;
            SQLCommond.BatchExecuteNonQueryStable(_reportFilePath, _azureBlobWorker.INSERT_DATE_SQL, parameterList);
        }

        public void UploadChangeLogReport()
        {
            _azureBlobWorker.UploadChangeLogReport(_reportFilePath);
        }

        public void DeleteLocalChangeLogFile()
        {
            _azureBlobWorker.DeleteLocalFile(_reportFilePath);
        }
    }
}
