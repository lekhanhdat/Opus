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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public static class Expression4DynamicQuery
    {
        public static Expression GetEqualExpression(Type typeOfProp, ParameterExpression expreParam, string propName, object propValue)
        {
            return GetExpression(typeOfProp, "Equals", expreParam, propName, propValue);
        }

        public static Expression GetContainsExpression(Type typeOfProp, ParameterExpression expreParam, string propName, object propValue)
        {
            return GetExpression(typeOfProp, "Contains", expreParam, propName, propValue);
        }

        public static Expression GetInExpression(Type typeOfProp, ParameterExpression expreParam, string propName, IEnumerable<Guid> arrayValue)
        {
            return GetArrayExpression(typeOfProp, "Contains", expreParam, propName, arrayValue.Cast<object>());
        }

        public static Expression GetInExpression(Type typeOfProp, ParameterExpression expreParam, string propName, IEnumerable<object> arrayValue)
        {
            return GetArrayExpression(typeOfProp, "Contains", expreParam, propName, arrayValue);
        }

        public static Expression GetExpression(Type typeOfProp, string methonName, ParameterExpression param, string propName, object propValue)
        {
            //Expression expression = Expression.Constant(false);
            PropertyInfo pi = typeOfProp.GetProperty(propName);
            var value = Convert.ChangeType(propValue, pi.PropertyType);
            Expression right = Expression.Call
                   (
                      Expression.Property(param, typeOfProp.GetProperty(propName)),  //c.DataSourceName
                      pi.PropertyType.GetMethod(methonName, new Type[] { pi.PropertyType }),// 反射使用方法
                      Expression.Constant(value, pi.PropertyType)// .Contains(optionName)
                   );
            return right;
        }

        public static Expression GetArrayExpression(Type typeOfProp, string methonName, ParameterExpression param, string propName, IEnumerable<object> arrayValue)
        {
            PropertyInfo pi = typeOfProp.GetProperty(propName);
            List<ConstantExpression> initializers = arrayValue.Select(array => Expression.Constant(Convert.ChangeType(array, pi.PropertyType))).ToList();
            var newArrayExp = Expression.NewArrayInit(pi.PropertyType, initializers);
            var containsMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name == methonName && m.GetParameters().Length == 2).FirstOrDefault();
            Expression right = Expression.Call(
                containsMethod?.MakeGenericMethod(pi.PropertyType),
                newArrayExp,
                Expression.Property(param, typeOfProp.GetProperty(propName)));
            return right;
        }

        public static Expression GetListExpression(Type typeOfProp, string methonName, ParameterExpression param, string propName, IEnumerable<object> arrayValue)
        {
            PropertyInfo pi = typeOfProp.GetProperty(propName);
            var newListExp = Expression.New(typeof(List<>).MakeGenericType(new Type[] { pi.PropertyType }));
            var elms1 = arrayValue.Select(array => Expression.Constant(Convert.ChangeType(array, pi.PropertyType))).ToList();
            var initListExp = Expression.ListInit(newListExp, elms1);
            Expression right = Expression.Call(
                initListExp,
                initListExp.Type.GetMethod(methonName, new Type[] { pi.PropertyType }),
                Expression.Property(param, typeOfProp.GetProperty(propName)));
            return right;
        }

        public static Expression<Func<T, int>> GetExpressionBody<T>(ParameterExpression param, string propName)
        {
            PropertyInfo property = typeof(T).GetProperty(propName);
            Expression propertyAccess = param;
            propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
            var body = Expression.Lambda<Func<T, int>>(propertyAccess, param);
            return body;
        }

        #region For cosmos db

        public static Expression GetContainsExpressionIgnoreCase(Type typeOfProp, ParameterExpression expreParam, string propName, object propValue)
        {
            return GetExpressionToLower(typeOfProp, "Contains", expreParam, propName, propValue);
        }
        public static Expression GetDoubleEqualityExpressionIgnoreCase(Type typeOfProp, ParameterExpression expreParam, string propName, object propValue)
        {
            return GetExpressionToLower(typeOfProp, "Equals", expreParam, propName, propValue);
        }

        public static Expression GetDoubleEqualityExpression(Type typeOfProp, ParameterExpression expreParam, string propName, object propValue)
        {
            PropertyInfo pi = typeOfProp.GetProperty(propName);
            var value = Convert.ChangeType(propValue, pi.PropertyType);
            return Expression.Equal(Expression.Property(expreParam, typeOfProp.GetProperty(propName)), Expression.Constant(value, pi.PropertyType));
        }

        public static Expression GetNotEqualityExpression(Type typeOfProp, ParameterExpression expreParam, string propName, object propValue)
        {
            PropertyInfo pi = typeOfProp.GetProperty(propName);
            var value = Convert.ChangeType(propValue, pi.PropertyType);
            return Expression.NotEqual(Expression.Property(expreParam, typeOfProp.GetProperty(propName)), Expression.Constant(value, pi.PropertyType));
        }

        public static Expression GetGreaterThanOrEqualExpression(Type typeOfProp, ParameterExpression expreParam, string propName, object propValue)
        {
            PropertyInfo pi = typeOfProp.GetProperty(propName);
            var value = Convert.ChangeType(propValue, pi.PropertyType);
            return Expression.GreaterThanOrEqual(Expression.Property(expreParam, typeOfProp.GetProperty(propName)), Expression.Constant(value, pi.PropertyType));
        }

        public static Expression GetLessThanOrEqualExpression(Type typeOfProp, ParameterExpression expreParam, string propName, object propValue)
        {
            PropertyInfo pi = typeOfProp.GetProperty(propName);
            var value = Convert.ChangeType(propValue, pi.PropertyType);
            return Expression.LessThanOrEqual(Expression.Property(expreParam, typeOfProp.GetProperty(propName)), Expression.Constant(value, pi.PropertyType));
        }

        public static Expression GetGreaterThanExpression(Type typeOfProp, ParameterExpression expreParam, string propName, object propValue)
        {
            PropertyInfo pi = typeOfProp.GetProperty(propName);
            var value = Convert.ChangeType(propValue, pi.PropertyType);
            return Expression.GreaterThan(Expression.Property(expreParam, typeOfProp.GetProperty(propName)), Expression.Constant(value, pi.PropertyType));
        }

        public static Expression GetExpressionToLower(Type typeOfProp, string methonName, ParameterExpression param, string propName, object propValue)
        {
            PropertyInfo pi = typeOfProp.GetProperty(propName);
            var value = Convert.ChangeType(propValue, pi.PropertyType);
            MethodInfo toLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
            Expression right = Expression.Call
                   (
                      Expression.Call(Expression.Property(param, typeOfProp.GetProperty(propName)), toLower),  //c.DataSourceName
                      pi.PropertyType.GetMethod(methonName, new Type[] { pi.PropertyType }),// 反射使用方法
                      Expression.Constant(value, pi.PropertyType)// .Contains(optionName)
                   );

            return right;
        }
        #endregion
    }
}
