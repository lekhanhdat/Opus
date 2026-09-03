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

    public class ConstructorInvoker : IConstructorInvoker
    {
        Func<Object[], Object> invoker;

        public ConstructorInfo ConstructorInfo { get; private set; }

        public ConstructorInvoker(ConstructorInfo constructorInfo)
        {
            this.ConstructorInfo = constructorInfo;
            this.invoker = InitializeInvoker(constructorInfo);
        }

        private Func<Object[], Object> InitializeInvoker(ConstructorInfo constructorInfo)
        {
            // Target: (Object)new T((T0)parameters[0], (T1)parameters[1], ...)

            // parameters to execute
            var parametersParameter = Expression.Parameter(typeof(Object[]), "parameters");

            // build parameter list
            var parameterExpressions = new List<Expression>();
            var paramInfos = constructorInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                // (Ti)parameters[i]
                var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                var valueCast = Expression.Convert(valueObj, paramInfos[i].ParameterType);

                parameterExpressions.Add(valueCast);
            }

            // new T((T0)parameters[0], (T1)parameters[1], ...)
            var instanceCreate = Expression.New(constructorInfo, parameterExpressions);

            // (Object)new T((T0)parameters[0], (T1)parameters[1], ...)
            var instanceCreateCast = Expression.Convert(instanceCreate, typeof(Object));

            var lambda = Expression.Lambda<Func<Object[], Object>>(instanceCreateCast, parametersParameter);

            return lambda.Compile();
        }

        public Object Invoke(params Object[] parameters)
        {
            return this.invoker(parameters);
        }

        #region IConstructorInvoker Members

        Object IConstructorInvoker.Invoke(params Object[] parameters)
        {
            return this.Invoke(parameters);
        }

        #endregion
    }
}