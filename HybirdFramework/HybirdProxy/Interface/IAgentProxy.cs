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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HybirdProxy.Interface
{
    /// <summary>
    /// Does not return Task when send message for safe concern, that is, the exception will throw if forget to wait task complete                         
    /// </summary>
    public interface IAgentProxy: IProxy
    {
        Task SendToAgentAsync<T>(string tenantId, string agentId, T methodInfo) where T: RemoteMethod;

        Task SendToAllAgentAsync<T>(string tenantId, T methodInfo) where T : RemoteMethod;

        Task SendToOneAgentAsync<T>(string tenantId, T methodInfo) where T : RemoteMethod;

        Task<Result> InvokeAgentAysnc<Func,Arg, Result>(string tenantId, string agentId, Func methodInfo) where Func : RemoteInvoke<Arg, Result>;

        Task<Result> InvokeOneAgentAysnc<Func,Arg, Result>(string tenantId, Func methodInfo) where Func : RemoteInvoke<Arg, Result>;

        ICollection<AgentInformation> GetAgents(string tenantId);

        ICollection<AgentInformation> GetAgentsForce(string tenantId);

        Dictionary<string,List<AgentInformation>> GetAllAgents();

        Dictionary<string, List<AgentInformation>> GetAllAgentsForce();

        event EventHandler AgentConnectionStateChange;
    }  
}
