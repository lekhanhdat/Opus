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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserConfigurationDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ConfigurationType Type { get; set; }

        [DataMember]
        public string Configuration { get; set; }
    }

    //  ConfigurationType的值要存储到数据库，所以必须显式地指定数字
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConfigurationType : int
    {
        [EnumMember]
        Undefined = 0,

        [EnumMember]
        PasswordPolicy = 1,

        [EnumMember]
        SecuritySetting = 2,

        [EnumMember]
        JobPruningSetting = 3,

        [EnumMember]
        UserDialog = 4,

        [EnumMember]
        UserView = 5,
    }
}
