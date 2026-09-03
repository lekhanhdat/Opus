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

namespace AvePoint.Metadata;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Base class for Agent backup service.
/// Write backup data in a standard format(cloud-backup\BackupLite\common\Data\Metadata\Stream\Documents)
/// It is easy to build your own service base on it, base class will take care of Data Format and Serialization
/// </summary>
public abstract class BackupServiceBase : IBackupService
{
    protected IAveBackupStream output;

    public BackupServiceBase(IAveBackupStream output)
    {
        this.output = output ?? throw new ArgumentNullException(nameof(output));
    }
    public BackupServiceBase(BackupServiceBase other)
        : this(other?.output)
    {
    }


    public virtual async Task ExportAsync()
    {
        this.output.BeginWriteMetadata();
        try
        {
            await ExportMetadataAsync();
        }
        finally
        {
            this.output.EndWriteMetadata();
        }
        await ExportContentAsync();
    }

    protected abstract Task ExportMetadataAsync();
    protected virtual async Task ExportContentAsync()
    {
        this.output.FlushMetadata(0L);
        await Task.CompletedTask;
    }


    #region Change the implementation to async after media is upgraded to async, then remove await Task.CompletedTask.
    protected virtual async Task WriteContentAsync(long size, Stream stream, CancellationToken token)
    {
        this.output.FlushMetadata(size);
        await stream.CopyToAsync(this.output.ToStream(), token);
    }
    protected async Task WriteMetadataAsync(AveMetadataType metadataType, object value)
    {
        this.output.WriteMetadata(metadataType, value);
        await Task.CompletedTask;
    }
    protected async Task WriteHeadAsync(string head)
    {
        this.output.WriteHead(head);
        await Task.CompletedTask;
    }
    protected async Task WriteTailAsync(string tail)
    {
        this.output.WriteTail(tail);
        await Task.CompletedTask;
    }
    protected async Task WriteTailAsync(string tail, bool isOK)
    {
        this.output.WriteTail(tail, isOK);
        await Task.CompletedTask;
    }

    public virtual void Dispose()
    {
    }
    #endregion
}