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
using AvePoint.GCommon.Contract.AccountManager.Object;

namespace AvePoint.GCommon.Contract.AccountManager
{
    public interface ITenantGroupInitService
    {
        [Obsolete]
        bool Init(GroupDto group, AccountMappingDto owner);

        /// <summary>
        /// 初始化Tenant Group default settings
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>
        /// true   初始化成功
        /// false  初始化失败
        /// </returns>
        bool Init(string groupId);
    }

}
