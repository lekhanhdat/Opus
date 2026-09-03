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

        List<string> GetItemStubAttachments(Guid siteId, Guid webId, Guid listId, int itemId);

        List<StubDocumentInfo> GetItemStubAttachmentsByDB(IAveListItem listItem, int startNum, int endNum, ref int totalNum);

        List<string> GetItemStubAttachmentsInFolder(Guid siteId, Guid webId, Guid listId, Guid parentId);

        List<StubDocumentInfo> GetItemStubAttachmentsInFolderByDB(Guid siteId, Guid webId, Guid listId, Guid parentId, int startNum, int endNum, ref int totalNum);

        List<string> GetStubFilesUrlInFolder(Guid siteId,Guid parentId);

        List<StubDocumentInfo> GetStubFilesInFolderByDB(IAveFolder folder, int startNum, int endNum, ref int totalNum);

        Guid GetStubIdByRbsId(object rbsId, ref bool isD6Stub);

        void UpdateStubFileStream(Guid siteId, Guid parentId, Guid uniqueId, byte[] bytes, long length, int version);

        void UpdateRbsID(Guid siteId, Guid parentId, Guid uniqueId, int uiVersion, byte[] data, int type, AveStorageInfo storageInfo);

        void UpdateDocumentSize(AveSPItemNativeInfo docInfo);

        void UpdateStubDocumentSize(int level, Guid parentId, Guid docId, Guid siteId, int size,long nextBSN);

        long GetMaxRbs(Guid siteId, Guid docId);

        void BeginReadBufferEx(Guid siteId, Guid itemId, int internalVersion, long size, Stream dataStream);


        #endregion

        void UpdateContentNative13(List<AveShredStubInfo> shredInfoList, Guid siteId, Guid DocId, Stream stream);

        void UpdateEBSStubByNative(Guid siteId, Guid parentId, Guid docId, int uiVersion, AveStorageInfo storageInfo, byte[] content);
    }

    public enum DiscoverStubOption
    {
        All = 0,
        OnlyDiscoverStub = 2,
        OnlyDiscoverNoneStub = 4,
    }
}
