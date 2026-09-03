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
using CommonModel.DataModel;
using CommonModel.MethodInfo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HybirdProxy.Interface
{
    public interface IHybirdHub
    {
        Dictionary<string, List<AgentInformation>> GetAgents();

        /// <summary>
        /// Note: The methodInfo should not have strong type in order to avoid Deserialization issue
        /// </summary>
        /// <param name="param"></param>
        /// <param name="methodInfo"></param>
        Task SendMessageToAgentAsync(HubMethodParam param, object methodInfo);


        Task SendMessageToManagerAsync(HubMethodParam param, object methodInfo);

        Task SendCallbackToManagerAsync(HubMethodParam param, object result);

        Task HandShake(string message);

        Task Heartbeat(string message);
    }
}
