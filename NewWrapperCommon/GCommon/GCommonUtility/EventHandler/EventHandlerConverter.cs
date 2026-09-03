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



namespace AvePoint.GCommon.Utility
{
    #region using directives

    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    #endregion using directives

    /// <summary>
    ///
    /// </summary>
    public class EventHandlerConverter : IEventHandlerConverter
    {
        Delegate IEventHandlerConverter.Convert(Delegate eventHandler, Type targetEventHandlerType)
        {
            return Convert(eventHandler, targetEventHandlerType);
        }

        TEventHandler IEventHandlerConverter.Convert<TEventHandler>(Delegate eventHandler)
        {
            return Convert<TEventHandler>(eventHandler);
        }

        private static Delegate Convert(Delegate eventHandler, Type targetEventHandlerType)
        {
            ParameterInfo[] destinationParameters;

            if (!IsValidEventHandler(targetEventHandlerType, out destinationParameters))
                throw new InvalidOperationException();

            if (eventHandler.GetType() == targetEventHandlerType)
                return eventHandler;

            ParameterInfo[] sourceParameters;
            if (!IsValidEventHandler(eventHandler.GetType(), out sourceParameters))
                throw new InvalidOperationException();

            var paramTypes = new Type[destinationParameters.Length + 1];
            paramTypes[0] = eventHandler.GetType();

            for (var i = 0; i < destinationParameters.Length; i++)
            {
                paramTypes[i + 1] = destinationParameters[i].ParameterType;
            }

            var method = new DynamicMethod("DynamicWrappedEventHandler", null, paramTypes);
            var invoker = paramTypes[0].GetMethod("Invoke");
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);

            if (!sourceParameters[1].ParameterType.IsAssignableFrom(destinationParameters[1].ParameterType))
            { il.Emit(OpCodes.Castclass, sourceParameters[1].ParameterType); }

            il.Emit(OpCodes.Call, invoker);
            il.Emit(OpCodes.Ret);
            return method.CreateDelegate(targetEventHandlerType, eventHandler);
        }

        private static TEventHandler Convert<TEventHandler>(Delegate eventHandler)
        {
            return (TEventHandler)(object)Convert(eventHandler, typeof(TEventHandler));
        }

        private static Boolean IsValidEventHandler(Type eventHandlerType, out ParameterInfo[] parameters)
        {
            if (!typeof(Delegate).IsAssignableFrom(eventHandlerType))
            {
                parameters = new ParameterInfo[0];
                return false;
            }

            var invokeMethod = eventHandlerType.GetMethod("Invoke");

            if (invokeMethod.ReturnType != typeof(void))
            {
                parameters = new ParameterInfo[0];
                return false;
            }

            parameters = invokeMethod.GetParameters();
            if (parameters.Length != 2 || parameters[0].ParameterType != typeof(object))
            {
                return false;
            }
            return typeof(EventArgs).IsAssignableFrom(parameters[1].ParameterType);
        }
    }
}