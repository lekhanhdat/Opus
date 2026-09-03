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
using System.Linq;
using System.Text;
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public interface IAveStorageOptimizationQueryService : IAveQueryService
    {
        void ConvertToContent(Guid siteId, Guid itemId, int level, int uiVersion, int internalVersion, int blobType, Stream stream, int targetTable, bool blobUseCRC32, long stubCRC);

        void CreatePool(byte[] blobInfo, bool canStoreNewBlobs, Guid siteId);

        byte[] GetBlobIdFromContentDB(Guid siteId, Guid itemId, int level, int uiVersion, int internalVersion, int targetTable);

        long EnsureContentSize(Guid siteId, Guid itemId, int internalVersion);

        int ReadContentOfItem(Guid siteId, Guid itemId, int internalVersion, long position, byte[] buffer, int offset, int count);

        void CreateEBSBlob(Guid siteId, Guid itemId, int level, int uiVersion, int internalVersion, byte[] blobId, int targetTable);

        void CreateRBSBlob(Guid siteId, Guid itemId, int level, int uiVersion, int internalVersion, byte[] poolId, byte[] blobId, long blobSize, int targetTable, bool updateOnly);

        int EnsureInternalVersion(Guid siteId, Guid itemId, int level, int uiVersion, ref int targetTable, ref int internalVersion);
        
        [Obsolete("Please use ModifyExtDoc(Guid id, Guid parentId, Guid siteId) instead")]
        void ModifyExtDoc(Guid id, Guid siteId);

        void ModifyExtDoc(Guid id, Guid parentId, Guid siteId);

        void BeginReadBufferEx(Guid siteId, Guid itemId, int internalVersion, long stubInfoSize, Stream dataStream);

        List<object> GetPoolList(Guid siteId);

        void CheckPoolCapacity(ref Dictionary<Guid, bool> results);
    }
}
