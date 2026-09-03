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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.AzureCosmosDB;
using AvePoint.RA.DB.Core;
using Azure.Data.Tables;
using Azure.Storage.Blobs;

namespace RAFileSystem.FSBatchUpload.Processor
{
    // sample Worker for DataCollection operation type
    public class FSDataCollectionBatchProcessor : FSBatchProcessorBase<FileSystemRecordDto>
    {
        private static RALogger logger = RALogger.GetInstance(typeof(FSBatchHandler));
        private RMAzureCosmosDBContainer Container;

        public override async Task InitializeAsync(BlobClient dataBlob, TableClient tableClient, BlobClient reportBlobClient)
        {
            await base.InitializeAsync(dataBlob, tableClient, reportBlobClient);
            Container = RMAzureCosmosDBContext.GetContainerAsync().GetAwaiter().GetResult();
        }


        protected override async Task<List<FSItemReportDto>> ProcessBatchAsync(BatchPackage<FileSystemRecordDto> batchPackage)
        {
            var dtos = batchPackage.Items;
            if (dtos == null || dtos.Count == 0) return [];

            Dictionary<Guid, string> failedItemDict = [];

            var resultList = new List<FSItemReportDto>();

            foreach (var item in await Container.UpsertRangeWithOptimisticLockAsync(dtos.ConvertAll(ConvertUtil.ConvertFSDtoToRMBaseRecord)))
            {
                failedItemDict[item.Item.NodeId] = item.Exception.Message;
            }

            foreach (var item in dtos)
            {
                var itemReport = new FSItemReportDto
                {
                    ItemId = item.NodeId,
                    ObjectName = item.LeafName,
                    Type = item.NodeType == (int)NodeLevel.FSFile ? "File" : "Folder",
                    OriginalFullPath = item.FullPath,
                    FinishTime = DateTime.UtcNow.Ticks,
                    Status = JobDetailsStatus.Successful,
                };

                if (failedItemDict.TryGetValue(item.NodeId, out var errorMessage))
                {
                    itemReport.Status = JobDetailsStatus.Failed;
                    itemReport.ErrorMessage = errorMessage;
                }

                resultList.Add(itemReport);
            }

            return resultList;

        }
    }
}
