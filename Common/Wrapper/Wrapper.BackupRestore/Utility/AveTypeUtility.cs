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
using System.Reflection.Emit;
using System.Collections.Generic;

using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.BackupRestore
{
    internal static class AveTypeUtility
    {
        public delegate object CreateInstance();

        /// <summary>
        /// Save 下面已经添加lock
        /// </summary>
        private static Dictionary<Type, CreateInstance> collections = new Dictionary<Type, CreateInstance>();

        public static CreateInstance GetConstructorMethod(Type type)
        {
            CreateInstance instance = null;

            lock (collections)
            {
                if (collections.ContainsKey(type))
                {
                    instance = collections[type];
                }
            }

            if (instance == null)
            {
                instance = MakeConstructorMethod(type);
                lock (collections)
                {
                    if (!collections.ContainsKey(type))
                    {
                        collections[type] = instance;
                    }
                }
            }

            return instance;
        }

        public static Action<object, object> CreateFieldSetterDelegate(FieldInfo field)
        {
            if (field.ReflectedType.IsValueType) throw new ArgumentException("cannot set field for value type.");

            string methodName = string.Format("{0}.set_{1}", field.ReflectedType, field.Name);
            var setter = new DynamicMethod(methodName, null, new Type[] { typeof(object), typeof(object) }, field.ReflectedType, true);
            var ilGen = setter.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            UnboxIfNeeded(ilGen, field.FieldType);
            ilGen.Emit(OpCodes.Stfld, field);
            ilGen.Emit(OpCodes.Ret);
            return (Action<object, object>)setter.CreateDelegate(typeof(Action<object, object>));
        }

        public static Func<object, object> CreateFieldGetterDelegate(FieldInfo info)
        {
            string methodName = string.Format("{0}.get_{1}", info.ReflectedType, info.Name);

            var getter = new DynamicMethod(methodName, typeof(object), new Type[] { typeof(object) }, info.ReflectedType, true);
            var ilGen = getter.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, info);
            BoxIfNeeded(ilGen, info.FieldType);
            ilGen.Emit(OpCodes.Ret);
            return (Func<object, object>)getter.CreateDelegate(typeof(Func<object, object>));
        }

        private static void UnboxIfNeeded(ILGenerator ilGen, Type type)
        {
            if (type.IsValueType)
            {
                ilGen.Emit(OpCodes.Unbox_Any, type);
            }
        }

        private static void BoxIfNeeded(ILGenerator ilGen, Type type)
        {
            if (type.IsValueType)
            {
                ilGen.Emit(OpCodes.Box, type);
            }
        }

        public static CreateInstance MakeConstructorMethod(Type type)
        {
            ConstructorInfo constructorInfo = type.GetConstructor(new Type[0]);
            if (constructorInfo == null)
            {
                throw new Exception(string.Format("Cannot find default constructor for type:{0}", type.FullName));
            }
            DynamicMethod dynamicMethod = new DynamicMethod(type.FullName + "Ctor", type, new Type[0], type.Module);
            ILGenerator generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Newobj, constructorInfo);
            generator.Emit(OpCodes.Ret);

            return (CreateInstance)dynamicMethod.CreateDelegate(typeof(CreateInstance));
        }

        public static T CreateNewInstance<T>()
        {
#if DEBUG
            using (AvePerformanceScope scope = new AvePerformanceScope("BRInfoConverter"))
            {
#endif
                CreateInstance instance = GetConstructorMethod(typeof(T));
                return (T)instance();
#if DEBUG
            }
#endif
        }
    }
}
