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

using Storage;
namespace MediaDataIO;

using MediaContract;

internal class ChunkedReadOnlyStream : ReadOnlyStreamBase
{
    private long? CurrentFileNumber;
    private Stream InternalStream;
    private long? ReadLength;

    private Action<string>? unDeleteSoftDeletedDataBlock;

    protected IDataPathGenerator PathGenerator { get; set; }
    protected DataPosition DataPosition { get; set; }
    protected IXSystem Device { get; set; }

    public override long Position { get { return ReadLength.GetValueOrDefault(); } set { throw new NotSupportedException(); } }


    internal ChunkedReadOnlyStream(IXSystem system, DataPosition dataPosition, IDataPathGenerator pathGenerator, Action<string> unDeleteSoftDeletedDataBlockCallBack)
    {
        Device = system;
        DataPosition = dataPosition;
        PathGenerator = pathGenerator;
        unDeleteSoftDeletedDataBlock = unDeleteSoftDeletedDataBlockCallBack;
    }

    private Memory<Byte> bufferLeft = Memory<byte>.Empty;


    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (null == InternalStream)
        {
            //first read
            CurrentFileNumber = DataPosition.StartFileNumber;
            await DisposeInternalStream();
            InternalStream = await InitialStream(Device, PathGenerator, DataPosition.PrefixNumber, DataPosition.StartFileNumber, DataPosition.StartOffset, DataPosition.ContentLength - ReadLength.GetValueOrDefault(), DataPosition.FileType);
            ReadLength = 0;
        }
        if (ReadLength >= DataPosition.ContentLength)
        {
            return await Task.FromResult(0);
        }
        if (!bufferLeft.IsEmpty)
        {
            bufferLeft.CopyTo(buffer);
            int count = bufferLeft.Length;
            bufferLeft = Memory<byte>.Empty;
            ReadLength += count;
            return count;
        }
        var result = await InternalStream.ReadAsync(buffer, cancellationToken);

        //change to next block
        if (result == 0 && ReadLength < DataPosition.ContentLength)
        {
            CurrentFileNumber++;

            await DisposeInternalStream();
            long nextStartPosition = GetNextReadDataPosition(DataPosition);
            InternalStream = await InitialStream(Device, PathGenerator, DataPosition.PrefixNumber, CurrentFileNumber.Value, nextStartPosition, DataPosition.ContentLength - ReadLength.GetValueOrDefault(), DataPosition.FileType);
            result = await InternalStream.ReadAsync(buffer, cancellationToken);
        }

        if (ReadLength + result > DataPosition.ContentLength)
        {
            int readCount = (int)(DataPosition.ContentLength - ReadLength);
            int leftSize = result - readCount;
            bufferLeft = buffer.Slice(readCount, leftSize);
            ReadLength += readCount;
            return readCount;
        }
        else
        {

            ReadLength += result;
            return result;
        }
    }

    /// <summary>
    /// content next block should have 4k block header and 4k aligned data header
    /// metadata next block should have 4k block header and 62 bytes data header
    /// </summary>
    /// <param name="dataPosition"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private static long GetNextReadDataPosition(DataPosition dataPosition)
    {
        return dataPosition.FileType switch
        {
            FileType.MetaData => IOConstants.BlockHeaderSize + IOConstants.DataHeaderSize,
            FileType.Content => dataPosition.ItemPageSize == 1 ? throw new EndOfStreamException() : IOConstants.BlockHeaderSize + dataPosition.ItemPageSize,
            _ => throw new NotSupportedException(dataPosition.FileType.ToString())
        };
    }

    private async Task<Stream> InitialStream(IXSystem system, IDataPathGenerator dataPathGenerator, long prefixNumber, long fileNumber, long startOffset, long length, FileType fileType)
    {
        string fullPath = dataPathGenerator.GenerateFileNamePath(prefixNumber, fileNumber, DataPosition.FileType);
        var storageInfo = new StorageInfo { LowName = fullPath };
        (var fileInfo, var workingSystem) = system.OpenFileExt(storageInfo);
        if (fileInfo == null)
        {
            if (unDeleteSoftDeletedDataBlock != null)
            {
                unDeleteSoftDeletedDataBlock(fullPath);
                (fileInfo, workingSystem) = system.OpenFileExt(storageInfo);
                if (fileInfo == null)
                {
                    throw new FileNotFoundException($"{fullPath} does not exist after retry undelete soft delete block.", fullPath);
                }
            }
            else
            {
                throw new FileNotFoundException($"{fullPath} does not exist.", fullPath);
            }
        }
        var expectedReadLength = startOffset + length > fileInfo.FileSize ? fileInfo.FileSize - startOffset : length;
        return await workingSystem.OpenReadAsync(new StorageInfo { LowName = fullPath, Offset = startOffset, Length = expectedReadLength });
    }

    public async override ValueTask DisposeAsync()
    {
        await DisposeInternalStream();
        Device = null;
        //DataPosition = null;
        PathGenerator = null;
        await base.DisposeAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (InternalStream != null)
        {
            InternalStream.Dispose();
            InternalStream = null;
        }
        base.Dispose(disposing);
    }

    private async Task DisposeInternalStream()
    {
        if (InternalStream != null)
        {
            await InternalStream.DisposeAsync();
            InternalStream = null;
        }
    }

    public override int Read(Span<byte> buffer)
    {
        if (null == InternalStream)
        {
            //first read
            CurrentFileNumber = DataPosition.StartFileNumber;
            DisposeInternalStream().ExecuteAsyncTask();
            InternalStream = InitialStream(Device, PathGenerator, DataPosition.PrefixNumber, DataPosition.StartFileNumber, DataPosition.StartOffset, DataPosition.ContentLength - ReadLength.GetValueOrDefault(), DataPosition.FileType).ExecuteAsyncTask();
            ReadLength = 0;
        }
        if (ReadLength >= DataPosition.ContentLength)
        {
            return 0;
        }
        if (!bufferLeft.IsEmpty)
        {
            bufferLeft.Span.CopyTo(buffer);
            int count = bufferLeft.Length;
            bufferLeft = Memory<byte>.Empty;
            ReadLength += count;
            return count;
        }
        var result = InternalStream.Read(buffer);

        //change to next block
        if (result == 0 && ReadLength < DataPosition.ContentLength)
        {
            CurrentFileNumber++;

            DisposeInternalStream().ExecuteAsyncTask();
            long nextStartPosition = GetNextReadDataPosition(DataPosition);
            InternalStream = InitialStream(Device, PathGenerator, DataPosition.PrefixNumber, CurrentFileNumber.Value, nextStartPosition, DataPosition.ContentLength - ReadLength.GetValueOrDefault(), DataPosition.FileType).ExecuteAsyncTask();
            result = InternalStream.Read(buffer);
        }

        if (ReadLength + result > DataPosition.ContentLength)
        {
            int readCount = (int)(DataPosition.ContentLength - ReadLength);
            int leftSize = result - readCount;
            bufferLeft = buffer.Slice(readCount, leftSize).ToArray().AsMemory();
            ReadLength += readCount;
            return readCount;
        }
        else
        {

            ReadLength += result;
            return result;
        }

    }
}