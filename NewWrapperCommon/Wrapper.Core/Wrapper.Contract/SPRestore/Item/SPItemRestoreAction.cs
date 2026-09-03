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
    /// Item Restore Action
    /// </summary>
    [Flags]
    public enum SPItemRestoreAction : short
    {
        /// <summary>
        /// 默认值
        /// </summary>
        Default,
        /// <summary>
        /// Overwrite，删创建，后还原
        /// </summary>
        Overwrite,
        /// <summary>
        /// 新Version，即需要创建（Item或者是涨Version）
        /// </summary>
        NewVersion,
        /// <summary>
        /// Skip 跳过，不需要进行还原
        /// </summary>
        Skip,
        /// <summary>
        /// DiscardCheckOut
        /// </summary>
        DiscardCheckOut,

        #region Old
        /// <summary>
        /// 没有进行Conflict Check操作，默认值
        /// </summary>
        //None,

        /// <summary>
        /// restore item without deleting item.
        /// </summary>
        //Default,

        /// <summary>
        /// delete + create
        /// </summary>
        //Overwrite,

        /// <summary>
        /// append a new item or file
        /// </summary>
        //Append,

        /// <summary>
        /// append a new version or file
        /// </summary>
        //AppendVersion,

        /// <summary>
        /// skip restore this item.
        /// </summary>
        //Skip,
        /// <summary>
        /// Undo check out action
        /// </summary>
        //DiscardCheckOut,
        /// <summary>
        /// 如果存在则move到conflict folder中，然后继续还原
        /// </summary>
        //MoveToConflictFolder,
        /// <summary>
        /// 自定义冲突处理方式
        /// </summary>
        //Custom
        #endregion
    }
}