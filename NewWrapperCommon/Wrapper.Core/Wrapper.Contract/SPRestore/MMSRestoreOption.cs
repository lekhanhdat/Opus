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
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// SiteCollection中managed metadata的还原选项。
    /// </summary>
    public class SPManagedMetadataRestoreOption
    {
        /// <summary>
        /// 是否需要Restore，如果不还原，就会cache起来
        /// </summary>
        public SPManagedMetadataRestoreType RestoreType { get; set; }
        /// <summary>不还原Global的Term Group及其Term Set</summary>
        public bool SkipGlobalTermGroup { get; set; }
        /// <summary>不还原Local的Term Group及其Term Set</summary>
        public bool SkipLocalTermGroup { get; set; }
        /// <summary>优先考虑从cache中获取数据，数据有更改的话就走一般的流程</summary>
        public bool EnableCache { get; set; }

        /// <summary>
        /// Filter Action
        /// </summary>
        public Action<List<AvePoint.Wrapper.Common.AveTermStoreInfo>> FilterAction { get; set; }
    }

    /// <summary>
    /// metadata Restore type
    /// </summary>
    public enum SPManagedMetadataRestoreType : byte
    {
        /// <summary>
        /// Restore
        /// </summary>
        Restore = 1,
        /// <summary>
        /// Cache
        /// </summary>
        Cache = 2,
        /// <summary>
        /// No action
        /// </summary>
        None = 3,
    }
}
