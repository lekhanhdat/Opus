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
using System.Reflection;
using System.Collections.Generic;

using AvePoint.GCommon;

namespace AvePoint.Wrapper.BackupRestore
{
    internal class AveOD4BCacheFolder : AveOD4BFolder
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool mIsRootFolder = false;
        private List<AveBRChangeObject> mCache;
        internal AveOD4BCacheFolder(string webUrl, string folderUrl, AveOD4BList parentList, List<AveBRChangeObject> cache, AveBRFolderInfo folderInfo = null) 
            : base(webUrl, folderUrl, parentList, folderInfo)
        {
            this.mCache = cache;
            mLog.Info("Cache folder url:{0}, parent list url:{1}", folderUrl, parentList.FullUrl);
            mIsRootFolder = parentList.FullUrl.TrimEnd('/').EndsWith(folderUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
        }

        protected override IEnumerable<AveBRItemInfo> GetFiles(string webUrl, string listUrl, string parentFolderUrl, bool includeVersions, bool includeVersionMetadata, bool includeSecurity, List<string> columns)
        {
            foreach (var item in this.mCache)
            {
                if (IsUnderFolder(parentFolderUrl, item))
                {
                    yield return GetItemInfo(item);
                }
            }
        }

        private AveBRItemInfo GetItemInfo(AveBRChangeObject changeObj)
        {
            object obj;
            if (changeObj.ItemProps.TryGetValue("Item", out obj))
            {
                return obj as AveBRItemInfo;
            }
            return null;
        }

        private bool IsUnderFolder(string folderUrl, AveBRChangeObject changeObj)
        {
            //filter out folder change
            if (string.Equals(changeObj.ItemType, "1", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            //在RootFolder 返回所有失败的ChangeItem
            if (this.mIsRootFolder && changeObj.Exception != null)
            {
                return true;
            }

            var itemUrl = changeObj.ServerRelativeUrl;
            if (!string.IsNullOrEmpty(changeObj.ParentObjServerRelativeUrl))
            {
                return string.Equals(folderUrl, changeObj.ParentObjServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                int index = itemUrl.LastIndexOf('/');
                if (index > 0)
                {
                    changeObj.ParentObjServerRelativeUrl = itemUrl.Substring(0, index);
                    var newItemUrl = folderUrl.TrimEnd('/') + "/" + itemUrl.Substring(index + 1);
                    return string.Equals(itemUrl, newItemUrl, StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }

        protected override IAveBackupRestoreFolder CreateSubFolder(string webUrl, string folderServerRelativeUrl, AveOD4BList parentList, AveBRFolderInfo folderInfo)
        {
            // AOSBR-3834 Folder有Rename事件,底下的files url改变,全备份
            if (this.mCache.Exists(f => (f.ChangeType == 4 || f.ChangeType == 1) && folderServerRelativeUrl.StartsWith(f.ServerRelativeUrl) && string.Equals(f.ItemType, "1", StringComparison.OrdinalIgnoreCase)))
            {
                return new AveOD4BFolder(webUrl, folderServerRelativeUrl, parentList, folderInfo);
            }

            return new AveOD4BCacheFolder(webUrl, folderServerRelativeUrl, parentList, this.mCache, folderInfo);
        }
    }
}
