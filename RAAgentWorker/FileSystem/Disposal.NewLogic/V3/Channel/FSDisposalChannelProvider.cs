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
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.DataSync.V2;

namespace RAFileSystem.FileSystem.Disposal.NewLogic.V3
{
    /// <summary>
    /// Manages bounded channels for the disposal pipeline stages:
    /// Discovery -> Worker -> Updater -> CosmosSend/CosmosReceive -> Report.
    /// </summary>
    public class FSDisposalChannelProvider
    {
        private readonly Task[] _readerCompletions;
        
        private int _discoverActiveItemCount = 0;
        
        public Channel<(FSAzureTableEntityDto, FileSystemRecordDto)> DiscoveryToWorkerChannel => FSJobCache.Instance.DiscoveryToWorker;
        
        public Channel<FSAzureTableEntityDto> DiscoveryToCosmoChannel => FSJobCache.Instance.DiscoveryToCosmos;
        
        public Channel<FSAzureTableEntityDto> ManualInFolderToCosmoChannel => FSJobCache.Instance.ManualInFolderToCosmos;


        public FSDisposalChannelProvider(int batchCapacity)
        {
            FSJobCache.Instance.DiscoveryToWorker = CreateBounded<(FSAzureTableEntityDto, FileSystemRecordDto)>(batchCapacity);
            FSJobCache.Instance.WorkerToUpdater = CreateBounded<(FSAzureTableEntityDto, FileSystemRecordDto)>(batchCapacity);
            FSJobCache.Instance.DiscoveryToCosmos = CreateBounded<FSAzureTableEntityDto>(batchCapacity);
            FSJobCache.Instance.ManualInFolderToCosmos = CreateBounded<FSAzureTableEntityDto>(batchCapacity);
        }

        public async Task WriteToCosmosAsync(FSAzureTableEntityDto dto) => await DiscoveryToCosmoChannel.Writer.WriteAsync(dto);
        
        public async Task WriteToWorkerAsync((FSAzureTableEntityDto, FileSystemRecordDto) dto) => await DiscoveryToWorkerChannel.Writer.WriteAsync(dto);
        
        private static Channel<T> CreateBounded<T>(int capacity)
        {
            return Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
            {
                SingleReader = false,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            });
        }
    }
}

