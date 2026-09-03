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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Explorer
{
    public interface IExplorerQueryParamProcesser
    {
        /// <summary>
        /// 预处理查询参数.比如转换为小写， 去除没有权限的source flags, nodetypes,...
        /// </summary>
        /// <param name="queryOption"></param>
        System.Threading.Tasks.Task ProcessAsync(ExplorerQueryOptionV2 queryOption);

        /// <summary>
        /// 预处理Advanced search参数
        /// </summary>
        /// <param name="queryOptionV3"></param>
        System.Threading.Tasks.Task ProcessV3Async(ExplorerQueryOptionV3 queryOptionV3);
        Task<bool> IsPhysicalEndUserAsync();
        Task<List<int>> GetPermissionConditionAsync();
    }
}
