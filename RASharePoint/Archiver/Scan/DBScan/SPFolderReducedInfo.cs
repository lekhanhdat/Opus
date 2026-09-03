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

namespace AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan
{
    internal class SPFolderReducedInfo
    {
        internal int ID;
        /// <summary>
        /// 1.目前没有测试过Folder大数据的case，比如100w folder，或者多层sub folder的case，按照common之前的经验，当folder达到一定量后，缓存ServerRelativeUrl会有内存问题.
        /// 2.目前只有SP Query方式才会调用到此逻辑，一般使用SP Query的case都是Query一部分数据，造成内存问题可能性较小.
        /// 3.后续如果遇到内存问题，可以考虑Folder Path MD5.
        /// </summary>
        internal string ServerRelativeUrl;
    }
}
