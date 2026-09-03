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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.AuthenticationManager
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuthenticationManagerDto : ISystemSettingContent
    {
        [DataMember]
        public Dictionary<AveAuthenticationTypes, AuthenticationCatalogDto> AuthenticationCollect { get; set; }
        /// <summary>
        /// 0 未被选为自动登录方式  1被选为自动登录方式；各种登录方式只能存在唯一一个DefaultValue = 1的类型
        /// </summary>
        [DataMember]
        public AveAuthenticationTypes DefaultType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuthenticationCatalogDto : ISystemSettingContent
    {
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// 状态：0 关闭；1 开启并配置完毕；2 关闭但是配置完毕。只有2显示Enable
        /// </summary>
        [DataMember]
        public int Status { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AveAuthenticationTypes
    {
        [EnumMember]
        LocalSystem = 0,

        [EnumMember]
        WindowsIntegration = 1,

        [EnumMember]
        ADIntegration = 2,

        [EnumMember]
        ADFSIntegration = 3,

        [EnumMember]
        Federation = 4,
    }

    public class AveAuthenticationTypesStatus
    {
        public static int CLOSE = 0;
        public static int ENABLED = 1;
        public static int DISABLED = 2;
    }

}
