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
using System.Text;
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public interface IAveSOIntegrationQueryService : IAveQueryService
    {
        #region SOIntegrationUtility

        List<IAveFile> GetItemStubAttachments(IAveListItem listItem);

        List<StubDocumentInfo> GetItemStubAttachmentsByDB(IAveListItem listItem, int startNum, int endNum, ref int totalNum);

        List<IAveFile> GetItemStubAttachmentsInFolder(IAveFolder folder);

        List<StubDocumentInfo> GetItemStubAttachmentsInFolderByDB(IAveFolder folder, int startNum, int endNum, ref int totalNum);

        List<IAveFile> GetStubFilesInFolder(IAveFolder folder);

        List<StubDocumentInfo> GetStubFilesInFolderByDB(IAveFolder folder, int startNum, int endNum, ref int totalNum);

        Guid GetStubIdByRbsId(object rbsId, ref bool isD6Stub);

        [Obsolete("Please use void UpdateStubFileStream(Guid siteId, Guid parentId, Guid uniqueId, byte[] bytes, long length, int version) instead")]
        void UpdateStubFileStream(Guid siteId, Guid uniqueId, byte[] bytes, long length, int version);

        void UpdateStubFileStream(Guid siteId, Guid parentId, Guid uniqueId, byte[] bytes, long length, int version);

        [Obsolete("Please use UpdateRbsID(Guid siteId, Guid parentId, Guid uniqueId, int uiVersion, byte[] data, int type, AveStorageInfo storageInfo) instead")]
        void UpdateRbsID(Guid siteId, Guid uniqueId, int uiVersion, byte[] data, int type, AveStorageInfo storageInfo);

        void UpdateRbsID(Guid siteId, Guid parentId, Guid uniqueId, int uiVersion, byte[] data, int type, AveStorageInfo storageInfo);

        [Obsolete("Please use UpdateFileStubByNative(Guid siteId, Guid parentId, Guid uniqueId, int currentVersion, int newCreatedVersion, int dataType, AveStorageInfo storageInfo, byte[] stubStreamByte) instead")]
        void UpdateFileStubByNative(Guid siteId, Guid uniqueId, int currentVersion, int newCreatedVersion, int dataType, AveStorageInfo storageInfo, byte[] stubStreamByte);

        void UpdateFileStubByNative(Guid siteId, Guid parentId, Guid uniqueId, int currentVersion, int newCreatedVersion, int dataType, AveStorageInfo storageInfo, byte[] stubStreamByte);

        void UpdateDocumentSize(AveSPItemNativeInfo docInfo);

        void BeginReadBufferEx(Guid siteId, Guid itemId, int internalVersion, long size, Stream dataStream);

        void RecordRollbackInformation(Guid siteId, Guid itemId, int level, int uiVersion, int internalVersion, int blobType);

        #endregion
    }
}
