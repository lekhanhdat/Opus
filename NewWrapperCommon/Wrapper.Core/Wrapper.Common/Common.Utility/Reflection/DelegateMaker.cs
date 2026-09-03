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
using System.Reflection;

namespace AvePoint.Wrapper.Common
{
    class DelegateMaker
    {
        private Delegate delegateInstance;
        private DelegateType type = DelegateType.None;
        public DelegateMaker(string memberName)
        {

        }

        private static DelegateMaker GetInstance(string memberName)
        {
            return SingleInstanceV4<DelegateMaker, string>.GetInstance(memberName);
        }
        public static TDelegate CreateDelegate<TDelegate>(MemberInfo info) where TDelegate : class
        {
            if (typeof(TDelegate).BaseType != typeof(MulticastDelegate)) throw new ArgumentException("type must be a delegate.");
            return GetInstance(string.Format("{0}.{1}", info.ReflectedType.FullName, info)).
                CreateDelegateInternal<TDelegate>(info);
        }
        private TDelegate CreateDelegateInternal<TDelegate>(MemberInfo info) where TDelegate : class
        {
            if (delegateInstance == null)
            {
                lock(this)
                {
                    if (delegateInstance == null)
                    {
                        delegateInstance = CreateDelegateInstance(info,typeof(TDelegate));
                    }
                }
            }
            return delegateInstance as TDelegate;
        }

        private Delegate CreateDelegateInstance(MemberInfo info, Type delegateType) 
        {
            switch (info.MemberType)
            {
                case MemberTypes.Method:
                    return CreateMethodDelegateInstance(info as MethodInfo, delegateType);
                case MemberTypes.Constructor:
                    return CreateConstructorDelegateInstance(info as ConstructorInfo, delegateType);
                case MemberTypes.Field:
                    return CreateFieldDelegateInstance(info as FieldInfo, delegateType);
                default:
                    throw new ArgumentException("info must be instance of MethodInfo, ConstructorInfo or FieldInfo.");
            }
        }

        private Delegate CreateFieldDelegateInstance(FieldInfo fieldInfo, Type delegateType)
        {
            if (this.type != DelegateType.None && this.type != DelegateType.Field) throw new ArgumentException();
            this.type = DelegateType.Field;
            throw new NotImplementedException();
        }

        private Delegate CreateConstructorDelegateInstance(ConstructorInfo info, Type delegateType)
        {
            if (this.type != DelegateType.None && this.type != DelegateType.Constructor) throw new ArgumentException();
            this.type = DelegateType.Constructor;
            throw new NotImplementedException();
        }

        private Delegate CreateMethodDelegateInstance(MethodInfo info, Type delegateType)
        {
            if (this.type != DelegateType.None && this.type != DelegateType.Method) throw new ArgumentException();
            this.type = DelegateType.Method;
            return WrapperInvoker.CreateDelegate(delegateType, info);
        }

        enum DelegateType
        {
            None = 0,
            Method = 1,
            Constructor = 2,
            Field = 3,
        }
    }
}
