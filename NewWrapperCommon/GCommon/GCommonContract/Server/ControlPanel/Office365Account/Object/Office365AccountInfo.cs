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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365AccountInfo
    {
        [DataMember]
        public String Id { get; set; }
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public AccountRole AccountRole { get; set; }
        [DataMember]
        public String UserName { get; set; }
        [DataMember]
        public String Password { get; set; }
        [DataMember]
        public String AdminUrl { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>
    /// Profile表Extension字段xml序列化用
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365AccountProfile : IProfileContent
    {
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public AccountRole AccountRole { get; set; }
        [DataMember]
        public String UserName { get; set; }
        [DataMember]
        public String Password { get; set; }
        [DataMember]
        public String AdminUrl { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AccountRole
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        SharepointAdministrator = 1,

        [EnumMember]
        GlobalAdministrator = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365AccountValidateResult
    {
        [DataMember]
        public ErrorInfo ErrorInfo { get; set; }

        [DataMember]
        public UnknownErrorType UnknownErrorType { get; set; }

        [DataMember]
        public AccountRole AccountRole { get; set; }

        [DataMember]
        public ServiceDto AgentInfo { get; set; }
    }

    //Office365AccountValidate 当Manager的逻辑判断出错时旧逻辑都是返回Unknown类型 现将Manager部分逻辑细分便于前台提示
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UnknownErrorType
    {
        //AgentCommonBrowser返回的Unknown错误或目前未做过滤的错误
        [EnumMember]
        None = 0,
        [EnumMember]
        AccountRoleIsMismatch = 1,
    }
}
