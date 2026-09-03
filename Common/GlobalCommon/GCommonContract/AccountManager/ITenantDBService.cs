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
using AvePoint.GCommon.Contract.Gateway.Object;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.AccountManager
{
    public interface ITenantDBService
    {
        /// <summary>
        /// 初始化GroupDBInfo，如果已经存在则什么也不做，这个方法是线程安全的
        /// </summary>
        /// <param name="tenantGroupId"></param>
        /// <param name="ownerName">tenantGroup的RegisterUserEmail or ownerName</param>
        /// <param name="dbType"></param>
        void InitTenantGroupDBInfo(string tenantGroupId, string ownerName, DBType dbType);

        /// <summary>
        /// 初始化Admin GroupDBInfo，如果已经存在则什么也不做，这个方法是线程安全的
        /// </summary>
        /// <param name="groupId"></param>
        void InitAdminGroupControlDBInfo(string groupId);

        /// <summary>
        /// 清除Group对应的User，Schema和Object，如果不存在则什么都不做
        /// </summary>
        /// <param name="tenantGroupId"></param>
        /// <param name="dbType"></param>
        void ClearTenantGroupDBInfo(string tenantGroupId, DBType dbType);
     
        /// <summary>
        /// 获取当前Tenant对应类型的Group DB Info，如果不存在就创建
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        TenantGroupDBInfoDto GetGroupDBInfo(DBType dbType);

        void MarkTenantGroupAsDelete(string tenantGroupId);

        void DeleteGroupDBInfo(string tenantGroupId);

        bool CheckGroupDBInfoExists(string groupId, DBType dbType);
    }
}
