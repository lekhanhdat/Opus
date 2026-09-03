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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// Invoker for reflect
    /// </summary>
    internal static class WrapperInvoker
    {
        #region field getter and setter
        #region getter
        /// <summary>
        /// result = obj.Field
        /// Usage:
        /// obj type is public
        /// field type is public, non-static
        ///
        /// obj type is public
        /// field type is non-public, non-static, TResult = object
        /// </summary>
        /// <typeparam name="TResult">字段类型</typeparam>
        /// <typeparam name="TObj">字段所在类的类型</typeparam>
        /// <param name="info">字段的FieldInfo</param>
        /// <returns></returns>
        internal static Func<TObj, TResult> CreateGetter<TObj, TResult>(FieldInfo info)
        {
            CheckArgs<TObj, TResult>(info, false);

            string methodName = string.Format("{0}.get_{1}", info.ReflectedType, info.Name);

            var getter = new DynamicMethod(methodName, typeof(TResult), new Type[] { typeof(TObj) }, info.ReflectedType, true);
            var ilGen = getter.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, info);
            ilGen.Emit(OpCodes.Ret);
            return (Func<TObj, TResult>)getter.CreateDelegate(typeof(Func<TObj, TResult>));
        }

        /// <summary>
        /// result = obj.Field
        /// Usage:
        /// obj type is non-public
        /// field type is public, non-static
        /// 
        /// obj type is non-public
        /// field type is not-public, non-static, TResult = object   
        /// </summary>
        /// <typeparam name="TResult">字段类型</typeparam>
        /// <param name="info">字段的FieldInfo</param>
        /// <param name="objType">字段所在类的类型</param>
        /// <returns></returns>
        internal static Func<object, object> CreateGetter(FieldInfo info)
        {
            CheckArgs(info, false);
            string methodName = string.Format("{0}.get_{1}", info.ReflectedType, info.Name);

            var getter = new DynamicMethod(methodName, typeof(object), new Type[] { typeof(object) }, info.ReflectedType, true);
            var ilGen = getter.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.CastclassOrUnboxValueType(info.ReflectedType);
            ilGen.Emit(OpCodes.Ldfld, info);
            //如果field类型为值类型, 需要装箱
            ilGen.BoxValueType(info.FieldType);
            ilGen.Emit(OpCodes.Ret);
            return (Func<object, object>)getter.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        /// result = Class.Field 
        /// Usage:
        /// Class type is public, non-public 
        /// field type is public, static
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        internal static Func<TResult> CreateStaticGetter<TResult>(FieldInfo info)
        {
            CheckArgs<TResult>(info, true);
            if (info.IsLiteral) throw new InvalidOperationException("cannot get value for literal field, please use info.GetValue(null) instead.");

            string methodName = string.Format("{0}.get_{1}", info.ReflectedType, info.Name);
            var getter = new DynamicMethod(methodName, typeof(TResult), null, info.ReflectedType, true);
            var ilGen = getter.GetILGenerator();
            ilGen.Emit(OpCodes.Ldsfld, info);
            ilGen.Emit(OpCodes.Ret);
            return (Func<TResult>)getter.CreateDelegate(typeof(Func<TResult>));
        }

        /// <summary>
        /// result = Class.Field 
        /// Usage:
        /// Class type is public, non-public 
        /// field type is non-public, static
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        internal static Func<object> CreateStaticGetter(FieldInfo info)
        {
            CheckArgs(info, true);
            if (info.IsLiteral) throw new InvalidOperationException("cannot get value for literal field, please use info.GetValue(null) instead.");

            string methodName = string.Format("{0}.get_{1}", info.ReflectedType, info.Name);
            var getter = new DynamicMethod(methodName, typeof(object), null, info.ReflectedType, true);
            var ilGen = getter.GetILGenerator();
            ilGen.Emit(OpCodes.Ldsfld, info);
            ilGen.BoxValueType(info.FieldType);
            ilGen.Emit(OpCodes.Ret);
            return (Func<object>)getter.CreateDelegate(typeof(Func<object>));
        }
        #endregion

        #region setter

        /// <summary>
        /// obj.Field = value
        /// Usage:
        /// obj type is public
        /// field type is public, non-static
        /// </summary>
        /// <typeparam name="TValue">字段类型</typeparam>
        /// <typeparam name="TObj">字段所在类的类型</typeparam>
        /// <param name="info">字段的FieldInfo</param>
        /// <exception cref="ArgumentException">TObj is a value type</exception>
        /// <returns></returns>
        internal static Action<TObj, TValue> CreateSetter<TObj, TValue>(FieldInfo info)
        {
            CheckArgs<TObj, TValue>(info, false);
            if (typeof(TObj).IsValueType) throw new ArgumentException("cannot set field for value type.");
            string methodName = string.Format("{0}.set_{1}", info.ReflectedType, info.Name);
            var setter = new DynamicMethod(methodName, null, new Type[] { typeof(TObj), typeof(TValue) }, info.ReflectedType, true);
            var ilGen = setter.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Stfld, info);
            ilGen.Emit(OpCodes.Ret);
            return (Action<TObj, TValue>)setter.CreateDelegate(typeof(Action<TObj, TValue>));
        }

        /// <summary>
        /// obj.Field = value
        /// Usage:
        /// obj type is non-public
        /// field type is public, non-static
        /// </summary>
        /// <typeparam name="TValue">字段类型</typeparam>
        /// <param name="info">字段的FieldInfo</param>
        /// <param name="objType">字段所在类的类型</param>
        /// <returns></returns>
        //internal static Action<object, TValue> CreateSetter<TValue>(FieldInfo info)
        //{
        //    CheckArgs(info, false);
        //    string methodName = string.Format("{0}.set_{1}", info.ReflectedType, info.Name);
        //    var setter = new DynamicMethod(methodName, null, new Type[] { typeof(object), typeof(TValue) }, true);
        //    var ilGen = setter.GetILGenerator();
        //    ilGen.Emit(OpCodes.Ldarg_0);
        //    CastclassOrUnboxValueType(info.ReflectedType, ilGen);
        //    ilGen.Emit(OpCodes.Ldarg_1);
        //    ilGen.Emit(OpCodes.Stfld, info);
        //    ilGen.Emit(OpCodes.Ret);
        //    return (Action<object, TValue>)setter.CreateDelegate(typeof(Action<object, TValue>));
        //}

        /// <summary>
        /// obj.Field = value
        /// Usage:
        /// obj type is public
        /// field type is non-public, non-static
        /// 
        /// obj type is non-public
        /// field type is public, non-static

        /// obj type is non-public
        /// field type is non-public, non-static
        /// </summary>
        /// <param name="info"></param>
        /// <param name="objType"></param>
        /// <param name="valueType"></param>
        /// <exception cref="ArgumentException">info.ReflectedType is a value type.</exception>
        /// <returns></returns>
        internal static Action<object, object> CreateSetter(FieldInfo info)
        {
            CheckArgs(info, false);
            if (info.ReflectedType.IsValueType) throw new ArgumentException("cannot set field for value type.");

            string methodName = string.Format("{0}.set_{1}", info.ReflectedType, info.Name);
            var setter = new DynamicMethod(methodName, null, new Type[] { typeof(object), typeof(object) }, info.ReflectedType, true);
            var ilGen = setter.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Castclass, info.ReflectedType);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.CastclassOrUnboxValueType(info.FieldType);
            ilGen.Emit(OpCodes.Stfld, info);
            ilGen.Emit(OpCodes.Ret);
            return (Action<object, object>)setter.CreateDelegate(typeof(Action<object, object>));
        }

        /// <summary>
        /// Class.Field = value
        /// Usage:
        /// Class type is public, non-public 
        /// field type is public, static
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        internal static Action<TValue> CreateStaticSetter<TValue>(FieldInfo info)
        {
            CheckArgs<TValue>(info, true);
            if (info.IsLiteral) throw new InvalidOperationException("cannot set value for literal field.");

            string methodName = string.Format("{0}.set_{1}", info.ReflectedType, info.Name);
            var setter = new DynamicMethod(methodName, null, new Type[] { typeof(TValue) }, info.ReflectedType, true);
            var ilGen = setter.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Stsfld, info);
            ilGen.Emit(OpCodes.Ret);
            return (Action<TValue>)setter.CreateDelegate(typeof(Action<TValue>));
        }

        /// <summary>
        /// Class.Field = value
        /// Usage:
        /// Class type is public, non-public 
        /// field type is non-public, static
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        internal static Action<object> CreateStaticSetter(FieldInfo info)
        {
            CheckArgs(info, true);
            string methodName = string.Format("{0}.set_{1}", info.ReflectedType, info.Name);
            var setter = new DynamicMethod(methodName, null, new Type[] { typeof(object) }, info.ReflectedType, true);
            var ilGen = setter.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.CastclassOrUnboxValueType(info.FieldType);
            ilGen.Emit(OpCodes.Stsfld, info);
            ilGen.Emit(OpCodes.Ret);
            return (Action<object>)setter.CreateDelegate(typeof(Action<object>));
        }
        #endregion
        #endregion

        #region constructor

        /// <summary>
        /// 无参构造方法
        /// TObj is public
        /// </summary>
        /// <typeparam name = "TObj" ></ typeparam >
        /// < param name="info"></param>
        /// <returns></returns>
        internal static Func<TObj> CreateInstance<TObj>(ConstructorInfo info) where TObj : class
        {
            CheckArgs(info, 0);
            var constructor = new DynamicMethod(string.Empty, typeof(TObj), null, info.ReflectedType, true);
            var ilGen = constructor.GetILGenerator();
            ilGen.Emit(OpCodes.Newobj, info);
            ilGen.Emit(OpCodes.Ret);
            return (Func<TObj>)constructor.CreateDelegate(typeof(Func<TObj>));
        }

        /// <summary>
        /// 无参构造方法
        /// TObj is non-public
        /// </summary>
        /// <param name="info"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        internal static Func<object> CreateInstance(ConstructorInfo info)
        {
            CheckArgs(info, 0);
            var constructor = new DynamicMethod(string.Empty, info.ReflectedType, null, info.ReflectedType, true);
            var ilGen = constructor.GetILGenerator();
            ilGen.Emit(OpCodes.Newobj, info);
            ilGen.Emit(OpCodes.Ret);
            return (Func<object>)constructor.CreateDelegate(typeof(Func<object>));
        }

        /// <summary>
        /// 一个参数的构造方法
        /// TObj is public
        /// TArg is public
        /// </summary>
        /// <typeparam name="TArg"></typeparam>
        /// <typeparam name="TObj"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        internal static Func<TArg, TObj> CreateInstance<TArg, TObj>(ConstructorInfo info) where TObj : class
        {
            CheckArgs<TArg, int, int, int>(info, 1);
            var constructor = new DynamicMethod(string.Empty, typeof(TObj), new Type[] { typeof(TArg) }, info.ReflectedType, true);
            var ilGen = constructor.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Newobj, info);
            ilGen.Emit(OpCodes.Ret);
            return (Func<TArg, TObj>)constructor.CreateDelegate(typeof(Func<TArg, TObj>));
        }

        /// <summary>
        /// 两个参数的构造方法
        /// TObj is public
        /// TArg1 is public
        /// TArg2 is public
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TObj"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        internal static Func<TArg1, TArg2, TObj> CreateInstance<TArg1, TArg2, TObj>(ConstructorInfo info) where TObj : class
        {
            CheckArgs<TArg1, TArg2, int, int>(info, 2);
            var constructor = new DynamicMethod(string.Empty, typeof(TObj), new Type[] { typeof(TArg1), typeof(TArg2) }, info.ReflectedType, true);
            var ilGen = constructor.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Newobj, info);
            ilGen.Emit(OpCodes.Ret);
            return (Func<TArg1, TArg2, TObj>)constructor.CreateDelegate(typeof(Func<TArg1, TArg2, TObj>));
        }

        /// <summary>
        /// 三个参数的构造方法
        /// TObj is public
        /// TArg1 is public
        /// TArg2 is public
        /// TArg3 is public
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TObj"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        internal static Func<TArg1, TArg2, TArg3, TObj> CreateInstance<TArg1, TArg2, TArg3, TObj>(ConstructorInfo info) where TObj : class
        {
            CheckArgs<TArg1, TArg2, TArg3, int>(info, 3);
            var constructor = new DynamicMethod(string.Empty, typeof(TObj), new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, info.ReflectedType, true);
            var ilGen = constructor.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Ldarg_2);
            ilGen.Emit(OpCodes.Newobj, info);
            ilGen.Emit(OpCodes.Ret);
            return (Func<TArg1, TArg2, TArg3, TObj>)constructor.CreateDelegate(typeof(Func<TArg1, TArg2, TArg3, TObj>));
        }

        /// <summary>
        /// 四个参数的构造方法
        /// TObj is public
        /// TArg1 is public
        /// TArg2 is public
        /// TArg3 is public
        /// TArg4 is public
        /// </summary>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TObj"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        internal static Func<TArg1, TArg2, TArg3, TArg4, TObj> CreateInstance<TArg1, TArg2, TArg3, TArg4, TObj>(ConstructorInfo info) where TObj : class
        {
            CheckArgs<TArg1, TArg2, TArg3, TArg4>(info, 4);
            var constructor = new DynamicMethod(string.Empty, typeof(TObj), new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) }, info.ReflectedType, true);
            var ilGen = constructor.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Ldarg_2);
            ilGen.Emit(OpCodes.Ldarg_3);
            ilGen.Emit(OpCodes.Newobj, info);
            ilGen.Emit(OpCodes.Ret);
            return (Func<TArg1, TArg2, TArg3, TArg4, TObj>)constructor.CreateDelegate(typeof(Func<TArg1, TArg2, TArg3, TArg4, TObj>));
        }

        /// <summary>
        /// 构造对象实例，参数超过4个
        /// </summary>
        /// <typeparam name="TFunc"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        //internal static Delegate CreateInstance<TFunc>(ConstructorInfo info)
        //{
        //    var parameters = info.GetParameters();
        //    CheckArgs(parameters, 5);
        //    var constructor = new DynamicMethod(string.Empty, info.ReflectedType, parameters?.Select(p => p.ParameterType).ToArray(), true);
        //    var ilGen = constructor.GetILGenerator();
        //    ilGen.Emit(OpCodes.Ldarg_0);
        //    ilGen.Emit(OpCodes.Ldarg_1);
        //    ilGen.Emit(OpCodes.Ldarg_2);
        //    ilGen.Emit(OpCodes.Ldarg_3);
        //    for (byte index = 4; index < parameters.Length; ++index)
        //    {
        //        ilGen.Emit(OpCodes.Ldarg_S, index);
        //    }
        //    ilGen.Emit(OpCodes.Newobj, info);
        //    ilGen.Emit(OpCodes.Ret);
        //    return constructor.CreateDelegate(typeof(TFunc));
        //}

        //private static void CheckArgs(ParameterInfo[] parameters, int minArgsCount)
        //{
        //    const int MaxArgsCount = byte.MaxValue + 1;
        //    int argCount = parameters?.Length ?? 0;
        //    if (argCount < minArgsCount) throw new ArgumentException($"Info's argument lists count is less than {minArgsCount}.");
        //    if (argCount > MaxArgsCount) throw new ArgumentException($"Info's argument lists count is great than {MaxArgsCount}.");
        //}
        #endregion


        public static TDelegate CreateDelegate<TDelegate>(MethodInfo info) where TDelegate : class
        {
            return CreateDelegate(typeof(TDelegate), info) as TDelegate;
        }
        public static Delegate CreateDelegate(Type type, MethodInfo info)
        {
            if (type.BaseType != typeof(MulticastDelegate)) throw new ArgumentException("type must be a delegate.");
            if (info.ReflectedType.IsPublic || info.IsStatic) return Delegate.CreateDelegate(type, info);
            Type returnType;
            Type[] parameters;
            GetParametersAndReturnTypes(type, info.ReturnType != typeof(void), out returnType, out parameters);
            return CreateDelegateForInstanceMethod(type, info, returnType, parameters);
        }

        private static Delegate CreateDelegateForInstanceMethod(Type type, MethodInfo info, Type returnType, Type[] parameters)
        {
            var method = new DynamicMethod(string.Empty, returnType, parameters, info.ReflectedType, true);
            var ilGen = method.GetILGenerator();
            ilGen.LoadArgs(parameters, info.ReflectedType);
            ilGen.Emit(info.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, info);
            ilGen.Emit(OpCodes.Ret);
            return method.CreateDelegate(type);
        }
        private static void LoadArgs(this ILGenerator ilGen, Type[] args, Type firstArgType)
        {
            LoadFirst4Args(ilGen, args, firstArgType);
            if (args.Length > 4)
            {
                for (byte index = 4; index < args.Length; ++index)
                {
                    ilGen.Emit(OpCodes.Ldarg_S, index);
                }
            }
        }
        private static void LoadFirst4Args(ILGenerator ilGen, Type[] args, Type firstArgType)
        {
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.CastclassOrUnboxValueType(firstArgType);

            switch (args.Length)
            {
                case 0:
                case 1:
                    return;
                case 2:
                    ilGen.Emit(OpCodes.Ldarg_1);
                    return;
                case 3:
                    ilGen.Emit(OpCodes.Ldarg_1);
                    ilGen.Emit(OpCodes.Ldarg_2);
                    return;
                case 4:
                default:
                    ilGen.Emit(OpCodes.Ldarg_1);
                    ilGen.Emit(OpCodes.Ldarg_2);
                    ilGen.Emit(OpCodes.Ldarg_3);
                    return;
            }
        }

        private static void GetParametersAndReturnTypes(Type type, bool hasReturnType, out Type returnType, out Type[] parameters)
        {
            var arguments = type.GetGenericArguments();
            if (hasReturnType)
            {
                returnType = arguments[arguments.Length - 1];
                parameters = arguments.Take(arguments.Length - 1).ToArray();
            }
            else
            {
                returnType = null;
                parameters = arguments;
            }
        }
        private static void CheckArgs(FieldInfo fieldInfo, bool staticField)
        {
            if (fieldInfo == null) throw new ArgumentNullException("fieldInfo");
            if (fieldInfo.IsStatic != staticField) throw new ArgumentException(string.Format("This method require a {0} field.", staticField ? "static" : "non-static"));
        }

        private static void CheckArgs<TArg1, TArg2, TArg3, TArg4>(ConstructorInfo constructorInfo, int argsCount)
        {
            var pareameters = CheckArgs(constructorInfo, argsCount);
            //与MSND C# Reference中的例子相似，使用goto来避免C# fall through case的编译错误。从代码可读性上，没有很大损失。
            //https://msdn.microsoft.com/en-us/library/13940fs2.aspx
            //https://msdn.microsoft.com/en-us/library/06tc147t.aspx
            switch (argsCount)
            {
                case 4://=case 4+3+2+1
                    if (pareameters[3].ParameterType != typeof(TArg4)) throw new ArgumentException();
                    goto case 3;
                case 3://=case 3+2+1
                    if (pareameters[2].ParameterType != typeof(TArg3)) throw new ArgumentException();
                    goto case 2;
                case 2://=case 2+1
                    if (pareameters[1].ParameterType != typeof(TArg2)) throw new ArgumentException();
                    goto case 1;
                case 1:
                    if (pareameters[0].ParameterType != typeof(TArg1)) throw new ArgumentException();
                    break;
            }
            //if (argsCount >= 1 && pareameters[0].ParameterType != typeof(TArg1)) throw new ArgumentException();
            //if (argsCount >= 2 && pareameters[1].ParameterType != typeof(TArg2)) throw new ArgumentException();
            //if (argsCount >= 3 && pareameters[2].ParameterType != typeof(TArg3)) throw new ArgumentException();
            //if (argsCount >= 4 && pareameters[3].ParameterType != typeof(TArg4)) throw new ArgumentException();

        }
        private static ParameterInfo[] CheckArgs(ConstructorInfo constructorInfo, int argsCount)
        {
            if (constructorInfo == null) throw new ArgumentNullException("constructorInfo");
            var pareameters = constructorInfo.GetParameters();
            if (pareameters.Length != argsCount) throw new ArgumentException(string.Format("This method require a constructor with {0} arguments", argsCount));
            return pareameters;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "参数名.")]
                private static void CheckArgs<TObj, TResult>(FieldInfo info, bool staticField)
        {
            CheckArgs(info, staticField);
            if (info.ReflectedType != typeof(TObj)) throw new ArgumentException("TObj");
            if (info.FieldType != typeof(TResult)) throw new ArgumentException("TResult");
        }
        private static void CheckArgs<TResult>(FieldInfo info, bool staticField)
        {
            CheckArgs(info, staticField);
            if (typeof(TResult) != info.FieldType) throw new ArgumentException("TValue or TResult");
        }

        private static void CastclassOrUnboxValueType(this ILGenerator ilGen, Type type)
        {
            if (type.IsValueType)
            {
                ilGen.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                ilGen.Emit(OpCodes.Castclass, type);
            }
        }
        private static void BoxValueType(this ILGenerator ilGen, Type type)
        {
            if (type.IsValueType)
            {
                ilGen.Emit(OpCodes.Box, type);
            }
        }

    }
}
