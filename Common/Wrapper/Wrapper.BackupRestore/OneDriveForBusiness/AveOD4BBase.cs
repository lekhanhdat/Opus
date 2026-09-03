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
using System.Text;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.AveO365LightWeightRequest;

namespace AvePoint.Wrapper.BackupRestore
{
    internal abstract class AveOD4BBase : IAveBackupRestoreBase
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected Dictionary<BackupOption, Func<IAveBackupStream, ProcessResult>> ExportMethods = new Dictionary<BackupOption, Func<IAveBackupStream, ProcessResult>>();
        //use this property to invoke request
        protected readonly AveOD4BRequestController mController = null;
        //FilterEngine --- protected

        protected BatchInfoCache mInternCache = new BatchInfoCache();
        protected bool mCacheInited = false;
        protected bool mBackupAll = true;
        protected DateTime mBackupStartTime;
        protected DateTime mBackupEndTime;

        //0 means not inited, otherwise 1
        private int mChangedItemsInited = 0;
        //[pending] change List<AveBRItemInfo> to List<AveChangeItemInfo>
        private List<AveBRChangeObject> mChangedItems;
        protected List<AveBRChangeObject> ChangedItems
        {
            get
            {
                EnsureChangedItems();
                return this.mChangedItems;
            }
        }

        internal AveOD4BRequestController Controller
        {
            get { return mController; }
        }

        protected AveOD4BBase(AveOD4BRequestController controller)
        {
            mController = controller;
            EnsureExportMethods();
        }

        protected AveOD4BBase(RequestConfig config)
        {
            mController = new AveOD4BRequestController(config);
            EnsureExportMethods();
        }

        public List<ProcessResult> Export(IAveBackupStream stream, BackupOption options)
        {
            mLog.Info("Try to backup {0} info at {1} level", options.ToString(), Level);
            List<ProcessResult> results = new List<ProcessResult>();

            foreach (BackupOption option in ExportMethods.Keys)
            {
                if (options.HasFlag(option))
                {
                    try
                    {
                        results.Add(ExportMethods[option](stream));
                    }
                    catch (Exception ex)
                    {
                        mLog.Error("An error occurred while doing backup {0} at {1} level. Error:{2}", option.ToString(), Level, ex);
                        ProcessResult result = new ProcessResult(option);
                        result.SetFailed(ex);
                        results.Add(result);
                    }
                }
            }
            return results;
        }

        public void SetBackupTimeRange(DateTime startTime, DateTime endTime)
        {
            if (startTime != DateTime.MinValue)
            {
                this.mBackupAll = false;
            }
            this.mBackupStartTime = startTime;
            this.mBackupEndTime = endTime;
        }

        private void EnsureChangedItems()
        {
            if (mBackupAll)
            {
                return;
            }
            if (Interlocked.CompareExchange(ref this.mChangedItemsInited, 1, 1) == 1)
            {
                return;
            }

            this.mChangedItems = GetChangedObjects();
            StringBuilder builder = new StringBuilder("All changed items in this list:\r\n");
            foreach (var item in this.mChangedItems)
            {
                if (item.Exception != null)
                {
                    builder.AppendFormat("[Failed change item. Id:{0} ChangeType:{1} Time:{2}]\r\n", item.ItemId, item.ChangeType.ToString(), item.Time.ToString("MM/dd/yyyy HH:mm:ss"));
                }
                else
                {
                    builder.AppendFormat("[Url:{0} ChangeType:{1} Time:{2}]\r\n", item.ServerRelativeUrl, item.ChangeType.ToString(), item.Time.ToString("MM/dd/yyyy HH:mm:ss"));
                }
            }
            mLog.Info(builder.ToString());
            Interlocked.CompareExchange(ref this.mChangedItemsInited, 1, 0);
        }

        protected abstract List<AveBRChangeObject> GetChangedObjects();

        private void EnsureCacheData(bool throwEx)
        {
            if (!this.mCacheInited)
            {
                ProcessResult result = new ProcessResult();
                try
                {
                    FillCacheData(result);
                }
                catch (Exception ex)
                {
                    if (throwEx)
                    {
                        throw;
                    }
                    mLog.Error("Failed to init {0} info. Error:{1}", Level, ex);
                    result.SetFailed(ex);
                    AddFakeData(result);
                }
                finally
                {
                    this.mCacheInited = true;
                }
            }
        }

        protected void VerifyCacheData(string dataName)
        {
            EnsureCacheData(false);
            CacheItem item;
            if (this.mInternCache.TryGet(dataName, out item))
            {
                if (!item.Result.IsSuccessful)
                {
                    throw item.Result.Exception;
                }
            }
        }

        protected abstract void FillCacheData(ProcessResult result);

        protected abstract void AddFakeData(ProcessResult result);
        
        protected abstract void EnsureExportMethods();

        protected abstract string Level { get; }
    }
}
