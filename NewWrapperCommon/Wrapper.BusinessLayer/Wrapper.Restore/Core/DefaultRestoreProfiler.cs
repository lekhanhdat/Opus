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

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.SPRestore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Restore.Core
{
    /// <summary>
    /// Default Restore Profiler
    /// </summary>
    abstract class DefaultRestoreProfiler : ISPImportProfiler
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(DefaultRestoreProfiler), false);

        private Stopwatch timer;
        private Stopwatch metadataTimer;
        protected TimeSpan totalRestoreTime;
        protected Dictionary<AveMetadataType, MetadataRestoreReport> metadataStatus;

        public DefaultRestoreProfiler()
        {
            this.timer = new Stopwatch();
            this.metadataTimer = new Stopwatch();
            this.metadataStatus = new Dictionary<AveMetadataType, MetadataRestoreReport>();
        }

        public virtual void BeginRestore()
        {
            this.timer.Start();
        }

        public virtual void EndRestore()
        {
            this.timer.Stop();
            this.totalRestoreTime = this.timer.Elapsed;
        }

        public virtual void BeginRestoreMetadata(AveMetadataType type)
        {
            this.metadataTimer.Start();
        }
        public virtual void EndRestoreMetadata(AveMetadataType type)
        {
            this.metadataTimer.Stop();

            var result = EnsureMetadataResult(type);

            result.AddTimeUsage(this.metadataTimer.Elapsed);
        }

        /// <summary>
        /// Ensure metadata status and get related metadata result via type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected MetadataRestoreReport EnsureMetadataResult(AveMetadataType type)
        {
            MetadataRestoreReport result = null;

            lock (metadataStatus)
            {
                if (!metadataStatus.TryGetValue(type, out result))
                {
                    result = new MetadataRestoreReport(type);
                    metadataStatus[type] = result;
                }
            }

            return result;
        }

        public abstract void OnStatusChanged(SPImportEventArgs eventArgs);

        public abstract void OnProgressUpdated(SPImportEventArgs eventArgs);

        public SPFileRestoreReport GenerateReport()
        {
            var report = new SPFileRestoreReport();
            report.UpdateTimeUsage(this.totalRestoreTime);

            if (metadataStatus != null)
            {
                foreach (var item in metadataStatus)
                {
                    report.Add(item.Key, item.Value);
                }
            }

            return report;
        }
    }
}
