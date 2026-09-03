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



using System.Collections.Generic;
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;

namespace AvePoint.Item.Restore
{
    public class ItemVersionFilter
    {
        public static bool EnableVersionFilter;
        private static bool mMajorVersionOnly;
        private static int mAllowedCount;
        private static ItemVersionFilter mVersionFilterInstance;
        private readonly AveSPItem mItem;
        private readonly List<int> mRestoreVersions;
        private readonly int mRowId = -1;
        
        public static bool MajorVersionOnly 
        {
            get { return mMajorVersionOnly; }
        }

        public static int AllowedCount 
        {
            get { return mAllowedCount; }
        }

        private ItemVersionFilter(AveSPItem item, List<int> versions, int rowId)
        {
            this.mItem = item;
            this.mRowId = rowId;
            this.mRestoreVersions = GetRestoreVersions(versions);
        }

        public IReadOnlyList<int> RestoreVersions
        {
            get { return this.mRestoreVersions.AsReadOnly(); }
        }

        public static void SetConfigAttr(RestoreVersionSetting restoreVersionSetting, int allowedCount)
        {
            EnableVersionFilter = (restoreVersionSetting != RestoreVersionSetting.All);
            mMajorVersionOnly = (restoreVersionSetting == RestoreVersionSetting.MajorOnly);
            mAllowedCount = allowedCount;
        }

        public static ItemVersionFilter GetInstance(AveSPItem item, AveMetadata versionData, int rowId)
        {
            if (item == null || versionData == null || rowId < 0)
            {
                return null;
            }
            var versions = versionData.GetMetadata<List<int>>();
            if (versions.Count > 0)
            {
                if (mVersionFilterInstance != null && mVersionFilterInstance.mItem.ParentFolder == item.ParentFolder && rowId != 0 && mVersionFilterInstance.mRowId.Equals(rowId))
                    //current version and previous version is same item.
                {
                    return mVersionFilterInstance;
                }
//current version and previous version are different items.
                mVersionFilterInstance = new ItemVersionFilter(item, versions, rowId);
            }
            return mVersionFilterInstance;
        }

        private List<int> GetRestoreVersions(List<int> versions)
        {
            List<int> restoreVersions = null;
            if (versions != null)
            {
                versions.Sort((left, right) => right - left);
                restoreVersions = new List<int>();

                #region Make current version as a valid version

                if (versions.Count > 0)
                {
                    int currentVersion = versions[0];
                    restoreVersions.Add(currentVersion);
                    versions.Remove(currentVersion);
                }

                #endregion

                int savedVersions = 0;
                foreach (int version in versions)
                {
                    if (savedVersions < mAllowedCount)
                    {
                        if (mMajorVersionOnly)
                        {
                            if (version%512 == 0)
                            {
                                restoreVersions.Add(version);
                                savedVersions++;
                            }
                        }
                        else
                        {
                            restoreVersions.Add(version);
                            savedVersions++;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return restoreVersions;
        }
    }
}