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




namespace System.Reflection
{
    #region using directives
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Extension;
    #endregion

    #region CodeReview

    [AveCodeReview(
    "2012/1/9",
    "yhzhang@avepoint.com",
    "yhzhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_9 },
    null,
    true)]
    #endregion

    ///<Summary>
    /// extension of the System.Int64 class
    ///</Summary>
    public static class FastReflectionExtension
    {
        public static Object FastInvoke(this MethodInfo methodInfo, Object instance, params Object[] parameters)
        {
            return FastReflectionCaches.MethodInvokerCache.Get(methodInfo).Invoke(instance, parameters);
        }

        public static void FastSetValue(this PropertyInfo propertyInfo, Object instance, Object value)
        {
            FastReflectionCaches.PropertyAccessorCache.Get(propertyInfo).SetValue(instance, value);
        }

        public static Object FastGetValue(this PropertyInfo propertyInfo, Object instance)
        {
            return FastReflectionCaches.PropertyAccessorCache.Get(propertyInfo).GetValue(instance);
        }

        public static Object FastGetValue(this FieldInfo fieldInfo, Object instance)
        {
            return FastReflectionCaches.FieldAccessorCache.Get(fieldInfo).GetValue(instance);
        }

        public static Object FastInvoke(this ConstructorInfo constructorInfo, params Object[] parameters)
        {
            return FastReflectionCaches.ConstructorInvokerCache.Get(constructorInfo).Invoke(parameters);
        }

        public static T GetAttribute<T>(this PropertyInfo propertyInfo)
            where T : Attribute
        {
            return Attribute.GetCustomAttribute(propertyInfo, typeof(T)) as T;
        }
    }
}