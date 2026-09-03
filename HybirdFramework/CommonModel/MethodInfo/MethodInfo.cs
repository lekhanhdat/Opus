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
    /// <summary>
    /// base abstract class stands for one remote call
    /// </summary>
    public abstract class RemoteMethod
    {
        public abstract string MethodName { get; }
    }

    /// <summary>
    /// stands for manager to agent one way invoke, there is result returned imediately
    /// </summary>
    public abstract class RemoteInvoke: RemoteMethod
    {
        /// <summary>
        /// use this id to track one invoke
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// indiate where the iovoken comes from
        /// </summary>
        public string ManagerId { get; set; }
    }

    /// <summary>
    /// send message to remote, no result returned.
    /// </summary>
    /// <typeparam name="Args"></typeparam>
    public abstract class RemoteMessage<Args> : RemoteMethod
    {
        public abstract Args MethodArgs { get; set; }
    }

    /// <summary>
    /// stands for manager to agent one way invoke, there is result returned imediately
    /// the reason Why put Result in this class is for validation purpose
    /// </summary>
    /// <typeparam name="Args"></typeparam>
    /// <typeparam name="Result"></typeparam>
    public abstract class RemoteInvoke<Args, Result>: RemoteInvoke
    {
        public abstract Args MethodArgs { get; set; }

        public abstract Result MethodResult { get; set; }
    }

    public abstract class RemoteInvoke<Result>: RemoteInvoke
    {
        public abstract Result MethodResult { get; set; }
    }

    public class AgentProxyCallback : RemoteInvoke<object>
    {
        public override object MethodResult { get; set; }

        public override string MethodName => "internal";
    }


}
