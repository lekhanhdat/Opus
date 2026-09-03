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
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.HostManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VMHostProfileMessage : AveMessage
    {
        /// <summary>
        /// 通信的HostProfile对象
        /// </summary>
        [DataMember]
        public VMCredentialProfileDto HostProfile { get; set; }
        /// <summary>
        /// 验证结果
        /// </summary>
        [DataMember]
        public CredentialErrorCode Result { get; set; }
        /// <summary>
        /// HostName，可以是FQDN或者IPAddress.
        /// 可以作为Host唯一标识,Test时Agent获取
        /// </summary>
        [DataMember]
        public string HostName { get; set; }
        /// <summary>
        /// 验证信息
        /// </summary>
        [DataMember]
        public string Message { get; set; }

    }
}
