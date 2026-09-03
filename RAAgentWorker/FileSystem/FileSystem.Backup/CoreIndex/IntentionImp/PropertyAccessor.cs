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
    using System.Linq.Expressions;
    using System.Reflection;
    #endregion

    public class PropertyAccessor : IPropertyAccessor
    {
        private Func<Object, Object> getter;
        private MethodInvoker m_setMethodInvoker;

        public PropertyInfo PropertyInfo { get; private set; }

        public PropertyAccessor(PropertyInfo propertyInfo)
        {
            this.PropertyInfo = propertyInfo;
            this.InitializeGet(propertyInfo);
            this.InitializeSet(propertyInfo);
        }

        private void InitializeGet(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead) return;

            // Target: (Object)(((TInstance)instance).Property)

            // preparing parameter, Object type
            var instance = Expression.Parameter(typeof(Object), "instance");

            // non-instance for static method, or ((TInstance)instance)
            var instanceCast = propertyInfo.GetGetMethod(true).IsStatic ? null :
                Expression.Convert(instance, propertyInfo.ReflectedType);

            // ((TInstance)instance).Property
            var propertyAccess = Expression.Property(instanceCast, propertyInfo);

            // (Object)(((TInstance)instance).Property)
            var castPropertyValue = Expression.Convert(propertyAccess, typeof(Object));

            // Lambda expression
            var lambda = Expression.Lambda<Func<Object, Object>>(castPropertyValue, instance);

            this.getter = lambda.Compile();
        }

        private void InitializeSet(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite) return;
            this.m_setMethodInvoker = new MethodInvoker(propertyInfo.GetSetMethod(true));
        }

        public Object GetValue(Object o)
        {
            if (this.getter == null)
            {
                throw new NotSupportedException("Get method is not defined for this property.");
            }

            return this.getter(o);
        }

        public void SetValue(Object o, Object value)
        {
            if (this.m_setMethodInvoker == null)
            {
                throw new NotSupportedException("Set method is not defined for this property.");
            }

            this.m_setMethodInvoker.Invoke(o, new Object[] { value });
        }

        #region IPropertyAccessor Members

        Object IPropertyAccessor.GetValue(Object instance)
        {
            return this.GetValue(instance);
        }

        void IPropertyAccessor.SetValue(Object instance, Object value)
        {
            this.SetValue(instance, value);
        }

        #endregion
    }
}