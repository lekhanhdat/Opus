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
using AvePoint.Application.StorageApiModern;
using Storage;

namespace AvePoint.Media.Service
{
    public class MoveServiceBase
    {
        protected async Task<StorageResult> MoveLargeItemAsync(StorageInfo sourceInfo, IXSystem sourceDevice, StorageInfo destinationInfo, IXSystem destinationDevice, bool overWrite = true, CancellationToken cancellationToken = default)
        {
            StorageResult storageResult;
            using (var sourceStream = await sourceDevice.OpenReadAsync(sourceInfo, cancellationToken))
            {
                storageResult = await destinationDevice.UploadAsyncExt(sourceStream, destinationInfo, overWrite, cancellationToken);
            }
            return storageResult;
        }

        protected async Task<StorageResult> MoveSmallItemAsync(StorageInfo sourceInfo, IXSystem sourceDevice, StorageInfo destinationInfo, IXSystem destinationDevice, bool overWrite = true, CancellationToken cancellationToken = default)
        {
            StorageResult storageResult;
            const int bufferSize = 1 * XConstants.MB;
            using (var sourceStream = await sourceDevice.OpenReadAsync(sourceInfo, cancellationToken))
            {
                var tempStream = new MemoryStream();
                sourceStream.CopyTo(tempStream, bufferSize);
                tempStream.Position = 0;
                storageResult = await destinationDevice.UploadAsyncExt(tempStream, destinationInfo, overWrite, cancellationToken);
            }
            return storageResult;
        }
    }
}
