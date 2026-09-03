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

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// 默认的一些冲突选项，有判断是否存在的，有判断modified time。
    /// </summary>
    [Flags]
    public enum SPItemConflictCheckOption
    {
        /// <summary>
        /// 不需要进行Check，直接跳过进行还原，根据用户选择的option来还原
        /// </summary>
        None,
        /// <summary>
        /// Check the destination item whether exists，如果目的端存在，则冲突，否则不冲突。
        /// </summary>
        CheckExist,
        /// <summary>
        /// 判断version number是否冲突，存在也冲突，当前version比需要还原的大也算冲突，只有不存在或者比需要还原的小就不算冲突
        /// </summary>
        CheckVersionNumber,
        /// <summary>
        /// 判断recycle bin是否存在对应的记录，如果存在则冲突。
        /// </summary>
        CheckRecycleBin,
        /// <summary>
        /// 如果源端的modified time比目的端的大，则冲突，否则不冲突。
        /// </summary>
        CheckNewChanged,
        /// <summary>
        /// 如果modified time一样，则不冲突，如果不一样则冲突。
        /// </summary>
        CheckModifiedTime,
        ///// <summary>
        ///// 如果modified time和指定的不一样，则冲突，skipped，如果不冲突，则按照restoreOption来还原。
        ///// </summary>
        //VerifyModifiedTime,
    }

    public enum SPItemConflictHandleOption
    {
        /// <summary>
        /// 不会进行Restore
        /// </summary>
        Skip,
        /// <summary>
        /// overwrite, 先删除，后还原
        /// </summary>
        Overwrite,
        /// <summary>
        /// 根据ModifiedTime进行check，最新修改的战胜
        /// </summary>
        OverwriteByLastModifiedTime,
        /// <summary>
        /// 外围自己定义冲突处理方法，返回给Wrapper需要进行的Action
        /// </summary>
        Custom
    }
}