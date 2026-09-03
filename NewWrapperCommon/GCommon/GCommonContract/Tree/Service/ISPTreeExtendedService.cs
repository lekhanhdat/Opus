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
namespace AvePoint.GCommon.Contract.Tree.Service
{
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.AccountManager.Object;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Tree.Object;

    public interface ISPTreeExtendedService
    {
        /// <summary>
        /// 更新当前AD用户对展开的子节点的权限信息
        /// </summary>
        /// <param name="accountMapping"></param>
        /// <param name="farmId"></param>
        /// <param name="permissionList"></param>
        void UpdateCacheTreePermission(AccountMappingDto accountMapping, SPTreeNodeDto currentNode, List<SPTreePermissionMappingDto> permissionList);

        /// <summary>
        /// 更新farm下所有AD用户的权限信息
        /// </summary>
        /// <param name="farmDto"></param>
        void UpdateCacheTreePermission(FarmDto farmDto);

        /// <summary>
        /// 对展开的子节点进行trim操作，对NodeExtension.IsAccessible属性赋值
        /// </summary>
        /// <param name="accountMapping"></param>
        /// <param name="nodes"></param>
        /// <returns>对NodeExtension.IsAccessible属性赋值之后的nodes参数</returns>
        List<SPTreeNodeDto> SecurityTrimmingTree(AccountMappingDto accountMapping, List<SPTreeNodeDto> nodes);

        /// <summary>
        /// 对整个tree进行trim操作，对NodeExtension.IsAccessible属性赋值
        /// </summary>
        /// <param name="accountMapping"></param>
        /// <param name="tree"></param>
        /// <param name="prune"></param>
        /// <returns></returns>
        SPTreeNodeDto SecurityTrimmingTree(AccountMappingDto accountMapping, SPTreeNodeDto tree, bool prune = false);

        /// <summary>
        /// 如果满足以下条件，则会更新用户的权限信息到数据库中
        /// 1.AD用户
        /// 2.在Web Application Tenant Group中
        /// </summary>
        /// <param name="accountMapping"></param>
        /// <returns></returns>
        bool NeedUpdatePermission(AccountMappingDto accountMapping);

        /// <summary>
        /// 如果满足以下条件，则会进行Security Trimming处理
        /// 1.AD用户
        /// 2.在Web Application Tenant Group中
        /// 3.Group设置了权限验证
        /// </summary>
        /// <param name="accountMapping"></param>
        /// <returns></returns>
        bool NeedSecurityTrimming(AccountMappingDto accountMapping);
    }
}
