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

using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// Restore Profiler
    /// </summary>
    public interface ISPImportProfiler
    {
        /// <summary>
        /// call this function when start restore
        /// </summary>
        void BeginRestore();

        /// <summary>
        /// call this function when restore is end
        /// </summary>
        void EndRestore();

        /// <summary>
        /// call this function when start restore metadata
        /// </summary>
        /// <param name="type"></param>
        void BeginRestoreMetadata(AveMetadataType type);

        /// <summary>
        /// call this function when restore metadata is end
        /// </summary>
        /// <param name="type"></param>
        void EndRestoreMetadata(AveMetadataType type);

        /// <summary>
        /// status changed, for example:
        /// create site collection failed.
        /// skip restore properties....
        /// </summary>
        /// <param name="eventArgs"></param>
        void OnStatusChanged(SPImportEventArgs eventArgs);

        /// <summary>
        /// 主要是更新状态，比如开始创建site，开始还原site等信息
        /// </summary>
        /// <param name="eventArgs"></param>
        void OnProgressUpdated(SPImportEventArgs eventArgs);

        /// <summary>
        /// Generate Report
        /// </summary>
        /// <returns></returns>
        SPFileRestoreReport GenerateReport();
    }

    internal static class ISPImportProfilerExtension
    {
        internal static void OnStatusChangedSafe(this ISPImportProfiler profile, SPImportEventArgs eventArgs)
        {
            if (profile != null)
            {
                profile.OnStatusChanged(eventArgs);
            }
        }
    }
}
