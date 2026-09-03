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


namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Reflection;
    using System.Text;

    static class TypeExtension
    {
        //可以将Field，Constructor都封装一层
        #region GetMethod
        //Usage:
        //静态方法:
        //TResult StaticClass.Method(TArg1,TArg2)，泛型TDelegate=Func<TArg1,TArg2,TResult>
        //void    StaticClass.Method(TArg1,TArg2)，泛型TDelegate=Action<TArg1,TArg2>
        //实例方法:
        //TResult InstanceClass.Method(TArg1,TArg2)
        //InstanceClass为Public    : 泛型TDelegate=Func<InstanceClass,TArg1,TArg2,TResult>
        //InstanceClass为Non-Public: 泛型TDelegate=Func<object,TArg1,TArg2,TResult>
        //void    StaticClass.Method(TArg1,TArg2)
        //InstanceClass为Public    : 泛型TDelegate=Action<InstanceClass,TArg1,TArg2>
        //InstanceClass为Non-Public: 泛型TDelegate=Action<object,TArg1,TArg2>

        public static TDelegate GetMethod<TDelegate>(this Type type, string name) where TDelegate : class
        {
            var info = type.GetMethod(name);
            CheckResult(type, name, info);
            return DelegateMaker.CreateDelegate<TDelegate>(info);
        }
        public static TDelegate GetMethod<TDelegate>(this Type type, string name, BindingFlags bindingAttr) where TDelegate : class
        {
            var info = type.GetMethod(name, bindingAttr);
            CheckResult(type, name, info);
            return DelegateMaker.CreateDelegate<TDelegate>(info);
        }
        public static TDelegate GetMethod<TDelegate>(this Type type, string name, Type[] types) where TDelegate : class
        {
            var info = type.GetMethod(name, types);
            CheckResult(type, name, types, info);
            return DelegateMaker.CreateDelegate<TDelegate>(info);
        }
        public static TDelegate GetMethod<TDelegate>(this Type type, string name, Type[] types, ParameterModifier[] modifiers) where TDelegate : class
        {
            var info = type.GetMethod(name, types, modifiers);
            CheckResult(type, name, types, info);
            return DelegateMaker.CreateDelegate<TDelegate>(info);
        }
        public static TDelegate GetMethod<TDelegate>(this Type type, string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers) where TDelegate : class
        {
            var info = type.GetMethod(name, bindingAttr, binder, types, modifiers);
            CheckResult(type, name, types, info);
            return DelegateMaker.CreateDelegate<TDelegate>(info);
        }
        public static TDelegate GetMethod<TDelegate>(this Type type, string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) where TDelegate : class
        {
            var info = type.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
            CheckResult(type, name, types, info);
            return DelegateMaker.CreateDelegate<TDelegate>(info);
        }
        #endregion

        //保留两个CheckResult重载，虽然内部实现类似，但会减少一次方法调用。
        private static void CheckResult(Type type, string methodName, MethodInfo info)
        {
            if (info == null) throw new MissingMethodException(string.Format("Method not found, more info:{0}{1}", Environment.NewLine, MakeMethodFullName(type, methodName, null)));
        }
        private static void CheckResult(Type type, string methodName, Type[] paramters, MethodInfo info)
        {
            if (info == null) throw new MissingMethodException(string.Format("Method not found, more info:{0}{1}", Environment.NewLine, MakeMethodFullName(type, methodName, paramters)));
        }

        ///Assembly: Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c(16.0.4316.1217)
        ///Microsoft.SharePoint.SPFile.ContinueUpload(System.Guid, System.IO.Stream)
        private static string MakeMethodFullName(Type type, string methodName, Type[] paramters)
        {
            var builder = new StringBuilder();
            builder.AppendLine(MakeAssemblyInfo(type.Assembly));                           //Assembly: Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c(16.0.4316.1217)
            builder.Append("Method: ");                                                    //Method: 
            builder.AppendFormat("{0}.{1}(", type.FullName, methodName);                   //Method: Microsoft.SharePoint.SPFile.ContinueUpload(
            if (paramters != null && paramters.Length > 0)
            {
                paramters.ForEach<Type>(t => builder.AppendFormat("{0}, ", t.FullName));   //System.Guid, System.IO.Stream, 
                builder.Remove(builder.Length - 2, 2);                                     //System.Guid, System.IO.Stream
            }
            builder.Append(")");                                                           //System.Guid, System.IO.Stream)
            return builder.ToString();
        }

        private static string MakeAssemblyInfo(Assembly assembly)
        {
            string fileVersion;
            try
            {
                fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
            }
            catch (Exception ex)
            {
                ex.EatException();
                fileVersion = "Cannot get file version";
            }

            return string.Format("Assembly: {0}({1})", assembly.FullName, fileVersion);
        }
    }
}
