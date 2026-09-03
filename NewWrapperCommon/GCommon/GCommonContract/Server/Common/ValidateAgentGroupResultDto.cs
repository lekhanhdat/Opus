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
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common
{
    /// <summary>
    ///用于存储使用该Agent Group的相关模块的信息。
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ValidateAgentGroupResultDto
    {
        /// <summary>
        /// 使用该Agent Group的模块名，请把国际化之后的值传过来
        /// </summary>
        [DataMember]
        public string ModuleName { get; set; }  
        /// <summary>
        /// 被占用的AgentGroupId
        /// </summary>
        [DataMember]
        public string AgentGroupId { get; set; }

        /// <summary>
        /// ProfileName 与 ObjectInfo
        /// </summary>
        [DataMember]
        public List<ReferanceDto> Referance { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReferanceDto
    {
        /// <summary>
        /// Plan, Profile, Processing Pool, Site Group 的name
        /// </summary>
        [DataMember]
        public string ProfileName { get; set; }

        /// <summary>
        /// User Group ID SharePoint Sites 特殊处理专用，其他模块忽略
        /// </summary>
        [DataMember]
        public string AccountGroupId { get; set; }

        /// <summary>
        /// 创建该plan , Profile, Processing Pool, Site Group的ObjectInfo  
        /// </summary>
        [DataMember]
        public ObjectInfoDto UserInfo { get; set; }

        #region Gui 前台使用，各模块请忽略
        /// <summary>
        /// AgentGroup 自己使用，各模块不需要对此属性赋值
        /// </summary>
        [DataMember]
        public string AccountGroupName { get; set; }

        [DataMember]
        public bool IsTenantGroup { get; set; }
        #endregion

    }
}
