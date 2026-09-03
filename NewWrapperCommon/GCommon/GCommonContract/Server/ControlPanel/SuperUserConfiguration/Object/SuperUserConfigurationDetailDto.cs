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

namespace AvePoint.GCommon.Contract.Server.ControlPanel.SuperUserConfiguration.Object
{
    public class SuperUserConfigurationDetailDto
    {
        [DataMember]
        public String Id { get; set; }
        [DataMember]
        public String DomainName { get; set; }
        [DataMember]
        public ConfigurationInfo ConfigInfo { get; set; }
    }
    /// <summary>
    /// Profile表Extension字段xml序列化用
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfigurationInfo : IProfileContent
    {
        [DataMember]
        public Status Status { get; set; }
        [DataMember]
        public string TenantName { get; set; }
        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public string AppPrincipalId { get; set; }
        [DataMember]
        public string Key { get; set; }
    }

    public enum Status
    {
        NotConfigured = 0,
        Configured = 1,
    }
}
