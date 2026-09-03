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
using AvePoint.Common;
using System;
using System.Reflection.Emit;
using System.ServiceModel;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal class XServiceModelActivity
    {
        private static Type activityType;
        private static Func<IDisposable> current;
        private static Func<IDisposable, bool, IDisposable> boundOperationCore;

        static XServiceModelActivity()
        {
            activityType = typeof(ChannelFactory).Assembly.GetType("System.ServiceModel.Diagnostics.ServiceModelActivity");
        }

        public static IDisposable Current
        {
            get
            {
                if (current == null)
                {
                    var mi = Invoker.GetMethod(activityType, "get_Current", null);
                    current = (Func<IDisposable>)Delegate.CreateDelegate(typeof(Func<IDisposable>), mi);
                }
                return current();
            }
        }

        public static IDisposable BoundOperation(IDisposable activity)
        {
            if (!XDiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            return BoundOperationCore(activity, false);
        }

        private static IDisposable BoundOperationCore(IDisposable activity, bool addTransfer)
        {
            if (boundOperationCore == null)
            {
                var mi = Invoker.GetMethod(activityType, "BoundOperationCore", null);

                var tDisposable = typeof(IDisposable);
                var tBool = typeof(bool);
                var dynamicMethod = new DynamicMethod("BoundOperationCore", tDisposable, new Type[] { tDisposable, tBool }, activityType);
                var il = dynamicMethod.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.EmitCall(OpCodes.Call, mi, null);

                boundOperationCore = (Func<IDisposable, bool, IDisposable>)dynamicMethod.CreateDelegate(typeof(Func<IDisposable, bool, IDisposable>));
            }
            return boundOperationCore(activity, addTransfer);
        }


    }
}
