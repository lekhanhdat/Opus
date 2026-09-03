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

namespace Microsoft365.Common.Utility
{
    public static class TypeInvoker
    {
        public static Func<TObjType, TValueType> CreateGetter<TObjType, TValueType>(FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, typeof(TValueType), new Type[1] { typeof(TObjType) }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, field);
            gen.Emit(OpCodes.Ret);
            return (Func<TObjType, TValueType>)setterMethod.CreateDelegate(typeof(Func<TObjType, TValueType>));
        }

        public static Action<TObjType, TValueType> CreateSetter<TObjType, TValueType>(FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[2] { typeof(TObjType), typeof(TValueType) }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, field);
            gen.Emit(OpCodes.Ret);
            return (Action<TObjType, TValueType>)setterMethod.CreateDelegate(typeof(Action<TObjType, TValueType>));
        }

        public static Func<TInstanceType> CreateObjInstance<TInstanceType>(ConstructorInfo ctorInfo)
        {
            var type = typeof(TInstanceType);
            DynamicMethod dynamic = new DynamicMethod(string.Empty,
                          type,
                          Type.EmptyTypes,
                          true);
            ILGenerator il = dynamic.GetILGenerator();

            il.DeclareLocal(type);
            il.Emit(OpCodes.Newobj, ctorInfo);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            return (Func<TInstanceType>)dynamic.CreateDelegate(typeof(Func<TInstanceType>));
        }

        public static Func<TArguType, TInstanceType> CreateObjInstance<TArguType, TInstanceType>(ConstructorInfo ctorInfo)
        {
            var type = typeof(TInstanceType);
            DynamicMethod dynamic = new DynamicMethod(string.Empty,
                          type,
                          new[] { typeof(TArguType) },
                          true);
            ILGenerator il = dynamic.GetILGenerator();

            il.DeclareLocal(type);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Newobj, ctorInfo);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            return (Func<TArguType, TInstanceType>)dynamic.CreateDelegate(typeof(Func<TArguType, TInstanceType>));
        }

        public static Func<TArguType1, TArguType2, TInstanceType> CreateObjInstance<TArguType1, TArguType2, TInstanceType>(ConstructorInfo ctorInfo)
        {
            var type = typeof(TInstanceType);
            DynamicMethod dynamic = new DynamicMethod(string.Empty,
                          type,
                          new[] { typeof(TArguType1), typeof(TArguType2), },
                          true);
            ILGenerator il = dynamic.GetILGenerator();

            il.DeclareLocal(type);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Newobj, ctorInfo);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            return (Func<TArguType1, TArguType2, TInstanceType>)dynamic.CreateDelegate(typeof(Func<TArguType1, TArguType2, TInstanceType>));
        }
    }
}