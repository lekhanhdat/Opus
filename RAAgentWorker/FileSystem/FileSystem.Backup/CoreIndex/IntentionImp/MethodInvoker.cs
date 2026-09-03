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




namespace AvePoint.GCommon.Extension
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    #endregion

    public class MethodInvoker : IMethodInvoker
    {
        private Func<Object, Object[], Object> invoker;

        public MethodInfo MethodInfo { get; private set; }

        public MethodInvoker(MethodInfo methodInfo)
        {
            this.MethodInfo = methodInfo;
            this.invoker = CreateInvokeDelegate(methodInfo);
        }

        public Object Invoke(Object instance, params Object[] parameters)
        {
            return this.invoker(instance, parameters);
        }

        private static Func<Object, Object[], Object> CreateInvokeDelegate(MethodInfo methodInfo)
        {
            // Target: ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)

            // parameters to execute
            var instanceParameter = Expression.Parameter(typeof(Object), "instance");
            var parametersParameter = Expression.Parameter(typeof(Object[]), "parameters");

            // build parameter list
            var parameterExpressions = new List<Expression>();
            var paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                // (Ti)parameters[i]
                BinaryExpression valueObj = Expression.ArrayIndex(
                    parametersParameter, Expression.Constant(i));
                UnaryExpression valueCast = Expression.Convert(
                    valueObj, paramInfos[i].ParameterType);

                parameterExpressions.Add(valueCast);
            }

            // non-instance for static method, or ((TInstance)instance)
            var instanceCast = methodInfo.IsStatic ? null :
                Expression.Convert(instanceParameter, methodInfo.ReflectedType);

            // static invoke or ((TInstance)instance).Method
            var methodCall = Expression.Call(instanceCast, methodInfo, parameterExpressions);

            // ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)
            if (methodCall.Type == typeof(void))
            {
                var lambda = Expression.Lambda<Action<Object, Object[]>>(
                        methodCall, instanceParameter, parametersParameter);

                Action<Object, Object[]> execute = lambda.Compile();
                return (instance, parameters) =>
                {
                    execute(instance, parameters);
                    return null;
                };
            }
            else
            {
                var castMethodCall = Expression.Convert(methodCall, typeof(Object));
                var lambda = Expression.Lambda<Func<Object, Object[], Object>>(
                    castMethodCall, instanceParameter, parametersParameter);

                return lambda.Compile();
            }
        }

        #region IMethodInvoker Members

        Object IMethodInvoker.Invoke(Object instance, params Object[] parameters)
        {
            return this.Invoke(instance, parameters);
        }

        #endregion
    }
}