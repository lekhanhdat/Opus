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

using AvePoint.GCommon.Contract.Agent.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.AgentProxy
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AgentProxyDataContract : IProfileContent
    {
        [DataMember]
        public List<AgentProxyContent> ProxyAgents { get; set; }

        public AgentProxyDataContract()
        {
            ProxyAgents = new List<AgentProxyContent>();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AgentProxyContent
    {
        [DataMember]
        public String ServiceId { get; set; }

        [DataMember]
        public String ServiceName { get; set; }

        [DataMember]
        public ProxyType ProxyType { get; set; }

        [DataMember]
        public String Address { get; set; }

        [DataMember]
        public String Port { get; set; }

        [DataMember]
        public String Username { get; set; }

        [DataMember]
        public String Password { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AgentProxyResponseInfo
    {
        [DataMember]
        public int SuccessCount { get; set; }

        [DataMember]
        public int FailedCount { get; set; }

        [DataMember]
        public String SuccessAgentProxyName { get; set; }

        [DataMember]

        public List<String> FailedAgentProxyNames { get; set; }
    }
}
