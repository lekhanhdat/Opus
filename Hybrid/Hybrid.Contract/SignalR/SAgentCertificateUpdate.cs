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
using CommonModel.MethodInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.Contract.SignalR
{
    public class SAgentCertificateUpdateExecute : RemoteInvoke<AgentCertificateUpdateArgs, AgentCertificateUpdateResult>
    {
        public override AgentCertificateUpdateArgs MethodArgs { get; set; }
        public override AgentCertificateUpdateResult MethodResult { get; set; }

        public override string MethodName => MethodMapping.MT[typeof(SAgentCertificateUpdateExecute)];
    }

    public class AgentCertificateUpdateArgs
    {
        public Guid AgentId { get; set; }
        /// <summary>
        /// AES encrypted base64 string of AgentConfigurtion object
        /// </summary>
        public string AgentName { get; set; }
        public string AgentConfigurtionContent { get; set; }
    }

    public class AgentCertificateUpdateResult
    {
        public Guid AgentId { get; set; }
        public string AgentName { get; set; }
        public AgentCertificateUpdateResultEnum Result { set; get; }
        public string Message { set; get; }
    }

    public enum AgentCertificateUpdateResultEnum
    {
        Succeed,
        Failed
    }
}
