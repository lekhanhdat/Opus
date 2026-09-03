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
using System.Runtime.Remoting.Messaging;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    public delegate object Fun();

    class APIProxy : System.Runtime.Remoting.Proxies.RealProxy
    {
        [ThreadStatic]
        public static Fun Current;
        private static APIProxy m_instance = null;

        public APIProxy(Type t)
            : base(t)
        {
        }

        public static APIProxy CreateInstance(Type t)
        {
            if (m_instance == null)
            {
                m_instance = new APIProxy(t);
            }

            return m_instance;
        }

        public override System.Runtime.Remoting.Messaging.IMessage Invoke(System.Runtime.Remoting.Messaging.IMessage msg)
        {
            IMethodCallMessage callmsg = msg as IMethodCallMessage;
            return new ReturnMessage(Current(), null, 0, null, callmsg);
        }
    }

    class AveProxyProvider
    {
        private static IOptimizationService m_instance;

        public static IOptimizationService GetProxy()
        {
            if (m_instance == null)
            {
                if (WrapperRuntime.CurrentContext.Opimized)
                {
                    return AveOptimizationQuery.CreateInstance();
                }
                else
                {
                    //APIProxy proxy = new APIProxy(typeof(IOptimizationService));
                    //m_instance = proxy.GetTransparentProxy() as IOptimizationService;
                    m_instance = APIProxy.CreateInstance(typeof(IOptimizationService)).GetTransparentProxy() as IOptimizationService;
                }
            }

            return m_instance;
        }
    }
}
