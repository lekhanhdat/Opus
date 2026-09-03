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
using AvePoint.GCommon.Contract.Server.ControlPanel.AgentProxy;
using AvePoint.GCommon.Contract.SharePointBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Contract.Agent.SharePointBrowser.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AgentProxyMessageContract : BrowserContractBase
    {
        [DataMember]
        public string HostName { get; set; }
        [DataMember]
        public string Port { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public ProxyType ProxyType { get; set; }
        [DataMember]
        public ResultStatus ResultStatus { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ResultStatus
    {
        [EnumMember]
        NoUpdate = 0,
        [EnumMember]
        Successful = 1,
        [EnumMember]
        ConfigFileUnavailable = 2,
        [EnumMember]
        ProxyInformationUnavailable = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProxyType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Http = 1,
        [EnumMember]
        Socket = 2,
    }
}
