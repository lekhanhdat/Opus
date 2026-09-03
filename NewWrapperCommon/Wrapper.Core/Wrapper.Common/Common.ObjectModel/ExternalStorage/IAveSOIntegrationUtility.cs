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
using AvePoint.Common;
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public interface IAveSOIntegrationUtility
    {
        #region Stub Discovery
        /// <summary>
        /// 得到Folder下所有Item的Attachment
        /// </summary>
        List<IAveFile> GetItemStubAttachmentsInFolder(IAveFolder folder);

        /// <summary>
        /// 分页查询Folder下所有Item的Attachement
        /// </summary>
        List<StubDocumentInfo> GetItemStubAttachmentsInFolderByDB(IAveFolder folder,int startNum,int endNum,ref int totalNum);

       /// <summary>
       ///得到Item的Stub Attachment
       /// </summary>
        List<IAveFile> GetItemStubAttachments(IAveListItem item);

        /// <summary>
        /// 分页查询Item的Stub Attachment
        /// </summary>
        List<StubDocumentInfo> GetItemStubAttachmentsByDB(IAveListItem item, int startNum, int endNum, ref int totalNum);

        /// <summary>
        /// 得到Stub File（不包括Version）
        /// </summary>
        List<IAveFile> GetStubFilesInFolder(IAveFolder folder);

        /// <summary>
        /// 分页查询所有Stub File和Version
        /// </summary>
        List<StubDocumentInfo> GetStubFilesInFolderByDB(IAveFolder folder, int startNum, int endNum, ref int totalNum);
        #endregion

        AveStorageInfo BackupRBSStorageInfo(IAveSite iAveSite, Guid fileUniqueID, int uiVersion, int fileLevel, IAveBackupRestoreQueryService queryService, AveRBSStubInfo rbsInfo, byte[] RbsId);

        AveStorageInfo13 BackupRBSStorageInfo13(IAveSite iAveSite, Guid fileUniqueID, int uiVersion, int fileLevel, IAveBackupRestoreQueryService queryService, List<AveRBSStubInfo13> rbsInfo);

        AveStorageInfo BackupEBSStorageInfo(IAveSite iAveSite, Guid fileUniqueID, int uiVersion, int fileLevel, IAveItem item, AveBaseItemInfo baseItemInfo);

        //IAveFile AddStubWithStream(IAveFolder folder, string fileName, System.IO.Stream stream);

        Guid GetStubIdByRbsId(object rbsId, ref bool isD6Stub);

        void RestoreStubDBInfo(string stubDBInfoBase64);
        Stream GetSourceFileStream(Guid siteID, int uiVersion, string webURL, Guid listID, Guid itemID);
    }

}
