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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.AccountManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AccountLogonInfo : IProfileContent
    {
        /// <summary>
        /// 当前登录的时间和地址
        /// 下一次登录时 此对象保存到History中
        /// </summary>
        [DataMember]
        public LogonInfoItem CurrentInfo { get; set; }

        /// <summary>
        /// 近期登录的记录
        /// 保存数量通过ControlServicePropertiesConfig中配置 不宜过多
        /// </summary>
        [DataMember]
        public List<LogonInfoItem> History { get; set; }

        public AccountLogonInfo()
        {
            History = new List<LogonInfoItem>();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LogonInfoItem
    {
        /// <summary>
        /// 登录时间
        /// </summary>
        [DataMember]
        public long Time { get; set; }

        /// <summary>
        /// 登录DocAve所使用机器的地址
        /// </summary>
        [DataMember]
        public string Address { get; set; }
    }
}
