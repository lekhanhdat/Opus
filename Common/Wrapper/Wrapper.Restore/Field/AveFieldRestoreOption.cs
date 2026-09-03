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

namespace AvePoint.Wrapper.Restore
{
    /// <summary>
    /// The restore option of field restore, 
    /// </summary>
    public class AveFieldRestoreOption
    {
        /// <summary>
        /// 查找option，有使用Id匹配，也有Name匹配的，也有schema匹配的。
        /// </summary>
        public FieldFindOption[] FindOption = new FieldFindOption[] { FieldFindOption.FindByCustomMapping, FieldFindOption.FindBySchema, FieldFindOption.FindById, FieldFindOption.FindByInternalName, FieldFindOption.FindByStaticName };
        /// <summary>
        /// Field冲突解决方案
        /// </summary>
        public FieldConflictOption ConflictOption = FieldConflictOption.Overwrite;
        public bool WEB_FIELD = true;
        public bool WEB_FIELD_NEWDISPLAYNAME = true;
        public bool LIST_FIELD_NEWDISPLAYNAME = true;
        public bool LIST_FIELD_SKIP = true;
        public bool COMPARE_MD5 = false;

        /// <summary>
        /// 现在只有Replicator模块才会使用这个参数，主要是如果目的端用ID能找到对应的Field，则无需要判断Type冲突，直接更新既可以。
        /// 如果其他模块也需要这个功能，请外围直接设置这个参数。
        /// </summary>
        public bool KEEPCONSISTENGBYID = false;
    }

    public enum FieldFindOption
    {
        /// <summary>
        /// 使用我们生成的field mapping关系
        /// </summary>
        FindBySchema,
        /// <summary>
        /// 使用Id
        /// </summary>
        FindById,
        /// <summary>
        /// 使用internal name来匹配
        /// </summary>
        FindByInternalName,
        /// <summary>
        /// 使用static name来匹配
        /// </summary>
        FindByStaticName,
        /// <summary>
        /// 使用display name来匹配
        /// </summary>
        FindByDisplayName,
        /// <summary>
        /// 使用自定义mapping，需要客户提供mapping关系
        /// </summary>
        FindByCustomMapping,
        Children
    }

    public enum FieldConflictOption
    {
        /// <summary>
        /// 不还原
        /// </summary>
        Skip,
        /// <summary>
        /// 把目的端rename，源端直接创建
        /// </summary>
        AppendSourceWin,
        /// <summary>
        /// 不破坏目的端，rename源端的title进行创建
        /// </summary>
        AppendDestinationWin,
        /// <summary>
        /// 直接update
        /// </summary>
        Overwrite
    }
    
}
