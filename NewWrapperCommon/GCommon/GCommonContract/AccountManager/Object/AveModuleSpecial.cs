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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.AccountManager.Object
{

    /// <summary>
    /// 用于判断当前向后台传输的页面
    /// 由于account manger 页面中的permission level分为（full，system，tenant permission他们显示的内容不一样）（agent group，agent monitor 需要显示），
    /// 所以利用标签的方式去控制当前模块是否会被显示 。具体请看Administration.cs (标签)
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AveModuleSpecial
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SystemPermission = 1,//"System Permission"
        [EnumMember]
        TenantPermission = 2,//"Tenant Permission"
        [EnumMember]
        AgentMonitor = 3,
        [EnumMember]
        AentGroup = 4,
        [EnumMember]
        FullPermission = 5,//"Full Permission"

    }
}
