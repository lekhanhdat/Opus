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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// Report需要包含performance report，默认不包含，这样外围可以控制。
    /// </summary>
    public sealed class SPFileRestoreReport : IEnumerable<KeyValuePair<AveMetadataType, MetadataRestoreReport>> 
    {
        /// <summary>
        /// Time Usage
        /// </summary>
        public TimeSpan TimeUsage { get; internal set; }

        private readonly Dictionary<AveMetadataType, MetadataRestoreReport> reports = new Dictionary<AveMetadataType, MetadataRestoreReport>();
 
        /// <summary>
        /// 加入report到集合中
        /// </summary>
        /// <param name="metadataType"></param>
        /// <param name="restoreReport"></param>
        internal void Add(AveMetadataType metadataType, MetadataRestoreReport restoreReport)
        {
            if (restoreReport != null)
            {
                reports[metadataType] = restoreReport;
            }
        }

        public IEnumerator<KeyValuePair<AveMetadataType, MetadataRestoreReport>> GetEnumerator()
        {
            return reports.GetEnumerator();
        }

        /// <summary>
        /// for foreach
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return reports.GetEnumerator();
        }

        /// <summary>
        /// 更新Time Usage
        /// </summary>
        /// <param name="time"></param>
        internal void UpdateTimeUsage(TimeSpan time)
        {
            TimeUsage = time;
        }
    }
}
