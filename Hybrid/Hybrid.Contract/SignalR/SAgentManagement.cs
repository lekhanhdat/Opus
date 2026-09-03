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
using AvePoint.Hybrid.Contract.Object;
using CommonModel.MethodInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.Contract.SignalR
{

    public class SAgentManagement : RemoteMessage<AgentManagementArgs>
    {

        public override AgentManagementArgs MethodArgs { get; set; }

        public override string MethodName { get { return MethodMapping.MT[typeof(SAgentManagement)]; } }

    }

    public enum MessageType
    {
        KeepAlive,
        Onstop
    }

    public class AgentManagementArgs
    {

        public string TenantId { set; get; }

        public string AgentId { set; get; }

        public MessageType Type { set; get; }

        public ServiceErrors Errors { get; set; }

        public int JobCounts { set; get; }

        public long TimeStamp { set; get; }

        public long CPUHZ { set; get; }

        public long CPUUSage { set; get; }

        public long TotalMemory { set; get; }

        public long AvailableMemeory { set; get; }

        public string OSName { set; get; }

        public int OSVersionNumber { set; get; }

        public string HostName { set; get; }

        public string Version { set; get; }

        public bool IsSupportUpgrade { set; get; }

    }

}
