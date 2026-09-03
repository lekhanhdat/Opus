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
using System.Threading.Tasks;

namespace AvePoint.RA.FileSystem.Core
{
    public interface IProgressService
    {
        /// <summary>
        /// 已经完成任务个数
        /// </summary>
        long Finished { get; }
        /// <summary>
        /// 任务总数
        /// </summary>
        long Total { get; }
        /// <summary>
        /// 增加基数.
        /// </summary>
        /// <param name="value"></param>
        void IncreaseBase(long value);
        /// <summary>
        /// 完成数量增加1.
        /// </summary>
        void Increase();
        /// <summary>
        /// 完成数增加N
        /// </summary>
        /// <param name="dicisor"></param>
        void Increase(int dicisor);
        /// <summary>
        /// 增加到100%
        /// </summary>
        void IncreaseToComplete();
        void SetTotal(long total);
    }
}
