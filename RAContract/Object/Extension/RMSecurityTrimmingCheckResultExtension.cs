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
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Object.Extension
{
    public static class RMSecurityTrimmingCheckResultExtension
    {

        /// <summary>
        /// 得到需要检查的container id list
        /// </summary>
        /// <param name="permissionCheckResult"></param>
        /// <returns></returns>
        public static List<string> GetContainerIds(this RMSecurityTrimmingCheckResult permissionCheckResult)
        {
            var containerIds = new List<string>();
            var allContainers = permissionCheckResult.DataSources.Values.Where(o => o.NeedCheck)
                .Select(o => o.Containers).ToList();
            foreach (var containers in allContainers)
            {
                containerIds.AddRange(containers);
            }
            return containerIds;
        }

        /// <summary>
        /// 删除没有权限的source flag
        /// </summary>
        /// <param name="permissionCheckResult"></param>
        /// <param name="sourceFlags"></param>
        public static void RemoveSourceFlags(this RMSecurityTrimmingCheckResult permissionCheckResult, List<SourceFlag> sourceFlags)
        {
            foreach (var key in permissionCheckResult.DataSources.Keys)
            {
                var t = permissionCheckResult.DataSources[key];
                if (t.NeedCheck && t.Containers.Count == 0)
                {
                    sourceFlags.Remove(key);
                }
            }
        }
    }
}
