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
using HybirdProxy.EndpointHandler;
using HybirdProxy.Implement;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace HybirdProxy.Extensions
{
    public static class HubExtension
    {
        public static IDisposable On<Func>(this HubConnection hubConnection, Action<Func> handler) where Func: RemoteMethod
        {
            EnsureMethodRegistered(typeof(Func));
            return hubConnection.On<Func>(MethodTable.MT[typeof(Func)], handler);
        }

        public static IDisposable On<Func>(this HubConnection hubConnection, EndpointHandlerBase<Func> handler) where Func: RemoteMethod
        {
            EnsureMethodRegistered(typeof(Func));
            return hubConnection.On<Func>(MethodTable.MT[typeof(Func)], handler.Handle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="Arg">arguments type</typeparam>
        /// <typeparam name="Result">result type</typeparam>
        /// <typeparam name="Func"></typeparam>
        /// <param name="hubConnection"></param>
        /// <param name="proxy">proxy used to receive callback</param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IDisposable OnFuncWithReturn<Func, Arg, Result>(this HubConnection hubConnection, ManagerProxy proxy, Func<Arg, Result> handler) where Func : RemoteInvoke<Arg, Result>
        {
            Action<Func> internalHandler = new Action<Func>((param) => {

                var result = handler(param.MethodArgs);
                param.MethodResult = result;
                //Push result back here
                proxy.SendCallbackToManagerAsync<Func>(param).Wait();
            });

            EnsureMethodRegistered(typeof(Func));
            return hubConnection.On<Func>(MethodTable.MT[typeof(Func)], internalHandler);
        }

        public static IDisposable OnFuncWithReturn<Func, Arg, Result>(this HubConnection hubConnection, ManagerProxy proxy, EndpointHandlerBase<Func,Arg,Result> handler) where Func:RemoteInvoke<Arg,Result>
        {
            Action<Func> internalHandler = new Action<Func>((param) => {

                var result = handler.Handle(param.MethodArgs);
                param.MethodResult = result;
                //Push result back here
                proxy.SendCallbackToManagerAsync<Func>(param).Wait();
            });

            EnsureMethodRegistered(typeof(Func));
            return hubConnection.On<Func>(MethodTable.MT[typeof(Func)], internalHandler);
        }

        /// <summary>
        /// Why implement using this pattern? just because there is no abstract static property in c#. And due to package dependency, we do not want to move this extension method outside.
        /// </summary>
        /// <param name="Func"></param>
        public static void EnsureMethodRegistered(Type Func)
        {
            if(!MethodTable.MT.ContainsKey(Func))
            {
                throw new UnexpectedException("The function not registered yet! please ensure the register your method in MethodTable before using the extension method");
            }
        }


    }
}
