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
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RoleAssignments;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Contract.RMWeb.Explorer
{
    public static class ExplorerQueryOptionV2Extension
    {
        /// <summary>
        /// Do the security trimming. it will remove those source falgs or nodetypes without permission
        /// </summary>
        /// <param name="queryOption"></param>
        /// <param name="userPermission"></param>
        /// <returns></returns>
        public static void SecurityTrimming(this ExplorerQueryOptionV2 queryOption, RMPermissionMasks userPermission)
        {
            userPermission.RemoveNoPermissionFourceFlags(queryOption.FilterOption.SourceFlags);
            userPermission.RemoveNoPermissionNodeTypes(queryOption.FilterOption.NodeTypes);
        }

        /// <summary>
        /// trim the search key and convert to lowercase
        /// </summary>
        /// <param name="queryOption"></param>
        public static void ConvertSearchKey2LowerCase(this ExplorerQueryOptionV2 queryOption)
        {
            if (queryOption.SearchOption != null && !string.IsNullOrEmpty(queryOption.SearchOption.Key)) //convert the search key to lower case
            {
                queryOption.SearchOption.Key = queryOption.SearchOption.Key.Trim().ToLower();
            }
        }

        /// <summary>
        /// Check if there is any sourceFlag included in the parameters
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static bool HasAnySourceFlag(this ExplorerQueryOptionV2 queryOption)
        {
            return queryOption?.FilterOption?.SourceFlags?.Count > 0;
        }

        /// <summary>
        /// if need to check node types but has no value, return true
        /// </summary>
        /// <param name="queryOption"></param>
        /// <returns></returns>
        public static bool HasInvalidNodeType(this ExplorerQueryOptionV2 queryOption)
        {
            if (queryOption == null || queryOption.FilterOption.NodeTypes == null) return false;

            return queryOption.FilterOption.NodeTypes.Count == 0;
        }

        /// <summary>
        /// Judge if any one of the columnId in columIds is included in the search option
        /// </summary>
        /// <param name="searchOption"></param>
        /// <param name="columnIds"></param>
        /// <returns></returns>
        public static bool IfHasSearchColumns(this ExplorerSearchOptionV2 searchOption, IEnumerable<string> columnIds)
        {
            if (searchOption != null && searchOption.Columns != null)
            {
                var interSect = columnIds.Intersect(searchOption.Columns.Select(o => o.Id)).ToList();
                return interSect.Count > 0;
            }

            return false;
        }
    }
}
