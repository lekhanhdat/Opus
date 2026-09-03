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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveFolder
    {
        IAveFileCollection Files { get; }
        int ItemCount { get; }
        string Name { get; }
        IAveFolder ParentFolder { get; }
        Hashtable Properties { get; }
        string ServerRelativeUrl { get; }
        string WelcomePage { get; set; }
        string Url { get; }
        IAveWeb ParentWeb { get; }
        bool Exists { get; }
        IAveFolderCollection Folders { get; }
        IAveListItem Item { get; }
        Guid ParentListId { get; }
        IAveFolderCollection SubFolders { get; }
        AveStorageMetrics StorageMetrics { get; }
        DateTime? TimeCreated { get; }
        DateTime? TimeLastModified { get; }
        /// <summary>
        /// List的RootFolder的UniqueId是为空的，如果需要获取，请使用UniqueIdNew，这个还不能更改，因为Replicator的老数据问题。
        /// </summary>
        Guid UniqueId { get; }
        IList<IAveContentType> UniqueContentTypeOrder { get; set; }
        IAveList ParentList { get; set; }
        IAveAudit Audit { get; }
        List<AveHiddenFileInfo> HiddenFiles { get; }
        IAveDocumentSerializer DocumentSerializer { get; }

        void Delete();//
        void Update();
        void MoveTo(string newUrl);
        Guid Recycle();
        void Reload();
        List<int> GetItemsByColumnValue(string columnDisplayName, string value);
        IAveDocumentSet DocumentSet { get; }
        AveRestoreResult RestoreFolder(AveFolderInfo info, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData);
        int ID { get; }
    }
}
