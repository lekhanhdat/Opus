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
    internal class CacheManager
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private List<Cache> mCaches = new List<Cache>(AveBackupRestoreConfig.CACHECOUNT);
        private List<string> mColumns = new List<string>();

        public CacheManager(int count, List<string> columns, List<AveBRItemInfo> incItems = null)
        {
            mColumns = columns;
        }

        /// <summary>
        /// Read data from cache
        /// Delete data after reading
        /// </summary>
        public void Read()
        { }

        /// <summary>
        /// Read data from cache by order(order by row Id)
        /// Delete data after reading
        /// </summary>
        public void ReadByOrder()
        { }

        /// <summary>
        /// Write data to cache
        /// </summary>
        public void Write()
        { }

        /// <summary>
        /// Delete data in cache
        /// </summary>
        public void Delete()
        { }

    }

    internal class Cache
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveOD4BRequestController mController = null;
        //columns that need to backup its value     
        private List<string> mColumns = new List<string>();
        //item count that waiting for be backed up
        private int mLeftItemsCount = AveBackupRestoreConfig.ITEMCOUNTPERCACHE;
        //when in an inc job, these items mean items need to be backed up
        //TODO when there are a lot of items changed in one folder
        private List<AveBRItemInfo> mIncItems = null;
        //internal collection stores data in one cache
        private List<AveBRItemInfo> items = null;
        //internal collection stores item version info in case of there are a lot of versions of one item
        private Dictionary<Guid, Dictionary<int, AveBRItemVersionInfo>> versions = new Dictionary<Guid, Dictionary<int, AveBRItemVersionInfo>>(AveBackupRestoreConfig.ITEMVERSIONCOUNTPERCACHE);

        public Cache(int count, List<string> columns, List<AveBRItemInfo> incItems = null)
        {
            if (count > AveBackupRestoreConfig.ITEMCOUNTPERCACHE)
            {
                mLeftItemsCount = count - AveBackupRestoreConfig.ITEMCOUNTPERCACHE;
                items = new List<AveBRItemInfo>(AveBackupRestoreConfig.ITEMCOUNTPERCACHE);
            }
            else
            {
                mLeftItemsCount = 0;
                items = new List<AveBRItemInfo>(count);
            }
            mColumns = columns;
            if (incItems != null && incItems.Count > 0)
            {
                this.mIncItems = incItems;
            }
            mController = new AveOD4BRequestController();
        }

        public void Read()
        { }

        public void ReadByOrder()
        { }

        public void Write()
        { }

        public void Delete()
        { }

        private void FillinContent()
        {
        }
    }
}
