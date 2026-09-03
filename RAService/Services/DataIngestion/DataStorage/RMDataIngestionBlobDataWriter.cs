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
using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.Contract.DataIngestion;
using ProtoBuf;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DataIngestion.DataStorage;

public class RMDataIngestionBlobDataWriter : IAsyncDisposable
{
    private readonly string _blobName;

    private readonly RMDataIngestionAzureStorageBlobHandler _blobClient;

    private Stream _writeStream;

    public static async Task<RMDataIngestionBlobDataWriter> CreateAsync(string blobName, RMDataIngestionAzureStorageBlobHandler blobClient)
    {
        var writer = new RMDataIngestionBlobDataWriter(blobName, blobClient);
        writer._writeStream = await blobClient.OpenWriteAsync(blobName);
        ProtobufRuntimeHelper.EnsureTypeRegistered<RMDataIngestionAgentWorkItemExecutionResult>();
        return writer;
    }

    private RMDataIngestionBlobDataWriter(string blobName, RMDataIngestionAzureStorageBlobHandler blobClient)
    {
        _blobName = blobName;
        _blobClient = blobClient;
    }

    public void WriteItem<T>(T item)
    {
        Serializer.SerializeWithLengthPrefix(_writeStream, item, PrefixStyle.Base128, 1);
    }
    public async ValueTask DisposeAsync()
    {
        await _writeStream.FlushAsync();
        await _writeStream.DisposeAsync();
    }
}
