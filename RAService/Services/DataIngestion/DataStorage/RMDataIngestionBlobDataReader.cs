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
namespace AvePoint.RA.Service.Services.DataIngestion.DataStorage;

using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.CommonUtil;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RMDataIngestionBlobDataReader
{
    private RALogger _logger = RALogger.GetInstance(typeof(RMDataIngestionBlobDataReader));
    private readonly string _blobName;

    private readonly RMDataIngestionAzureStorageBlobHandler _blobClient;

    public RMDataIngestionBlobDataReader(string blobName, RMDataIngestionAzureStorageBlobHandler blobClient)
    {
        _blobName = blobName;
        _blobClient = blobClient;
    }

    public void RegisterProtobufModel<T>()
    {
        ProtobufRuntimeHelper.EnsureTypeRegistered<T>();
    }

    public async IAsyncEnumerable<T> ReadItemsAsync<T>()
    {
        await using var stream = await _blobClient.OpenReadAsync(_blobName).ConfigureAwait(false);
        IEnumerable<T> items;
        try
        {
            items = Serializer.DeserializeItems<T>(stream, PrefixStyle.Base128, 1);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to deserialize items from blob {0}. Exception: {1}", _blobName, ex);
            yield break;
        }
        foreach (var item in items)
        {
            yield return item;
        }
    }

    public async Task CompleteAsync()
    {
        await _blobClient.DeleteAsync(_blobName);
    }
}

