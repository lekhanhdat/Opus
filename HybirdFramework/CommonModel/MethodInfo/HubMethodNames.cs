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

namespace CommonModel.MethodInfo
{
    public class HubMethodNames
    {
        public const string SendMessageToAgent = "SendMessageToAgentAsync";
        public const string GetAgents = "GetAgents";
        public const string HandShake = "HandShake";
        public const string Heartbeat = "Heartbeat";
        public const string SendMessageToManager = "SendMessageToManagerAsync";
        public const string SendCallbacvkToManagerAsync = "SendCallbackToManagerAsync";


        #region for proxy

        public const string AgentConnectionNotification = "AgentConnectionNotification";
        public static string AgentRPCCallback = "AgentRPCCallback";



        #endregion
    }

    public class APIScope
    {
        public const string Manager = "signalrmanager.readwrite.all";
        public const string Agent = "signalragent.readwrite.all";
        public const string Common = "signalrcommon.readwrite.all";
    }

    public class HubMethodParam
    {
        public DeliverMode Mode { get; set; }
        public string AgentId {get;set;}
        public string TenantId { get; set; }

        public string ManagerId { get; set; }

        public string MethodName { get; set; }
    }

    public enum DeliverMode
    {
        One = 0,
        All = 1,
        RPCResult = 2,
        RPCInvoke = 3,
    }
}
