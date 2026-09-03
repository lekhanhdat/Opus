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

namespace AvePoint.Hybrid.Contract.SignalR
{
    internal class RecordsAgentUpgrade : RemoteMessage<RecordsAgentUpgradeArgs>
    {
        public override RecordsAgentUpgradeArgs MethodArgs { get; set; }
        public override string MethodName { get { return nameof(RecordsAgentUpgrade); } }
    }

    public class RecordsAgentUpgradeExecute : RemoteInvoke<RecordsAgentUpgradeArgs, RecordsAgentUpgradeResult>
    {
        public override RecordsAgentUpgradeArgs MethodArgs { get; set; }
        public override RecordsAgentUpgradeResult MethodResult { get; set; }

        public override string MethodName => MethodMapping.MT[typeof(RecordsAgentUpgradeExecute)];
    }

    public class RecordsAgentUpgradeArgs
    {
        public AgentInfo AgentInfo { get; set; }
        public string TargetVersion { get; set; } 
    }

    public class RecordsAgentUpgradeResult
    {
        public Guid AgentId { get; set; }
        public RMAgentUpgradeResult Result { set; get; }
        public string Message { set; get; }
    }

    public enum RMAgentUpgradeResult
    {
        Succeed,
        Failed
    }
}
