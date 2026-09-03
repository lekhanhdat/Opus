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




namespace AvePoint.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AvePoint.GCommon;
    #endregion

    public class Invoker
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static Dictionary<String, Type> typeDictionary = new Dictionary<String, Type>();
        static List<Assembly> typeSearchAssemblies = new List<Assembly>();

        public static void AddTypeSearchAssemblyTree(Assembly asm)
        {
            if (asm == null) return;

            lock (typeSearchAssemblies)
            {
                if (!typeSearchAssemblies.Contains(asm))
                {
                    typeSearchAssemblies.Add(asm);

                    // get reference assemblies
                    AssemblyName[] childrenAsmNames = asm.GetReferencedAssemblies();
                    foreach (AssemblyName asmName in childrenAsmNames)
                    {
                        // this may take more space, which is normally OK
                        try
                        {
                            Assembly child = Assembly.Load(asmName);
                            if (!typeSearchAssemblies.Contains(child))
                                typeSearchAssemblies.Add(child);
                        }
                        catch (Exception e) { logger.Debug(e.ToString()); }
                    }
                }
            }
        }

        public static void AddTypeSearchAssembly(Assembly asm)
        {
            if (asm == null) return;

            lock (typeSearchAssemblies)
            {
                if (!typeSearchAssemblies.Contains(asm))
                {
                    typeSearchAssemblies.Add(asm);
                }
            }
        }

        public static void AddTypeSearchAssembly(Type seedType)
        {
            AddTypeSearchAssembly(seedType.Assembly);
        }

        public static Type GetType(string typeFullName)
        {
            #region Get the type by Type.GetType method, if not, continue to use old method

            Type relatedType = null;

            try
            {
                relatedType = Type.GetType(typeFullName);
            }
            catch (Exception e) { string slipFxCop = e.Message; }

            if (relatedType != null)
            {
                return relatedType;
            }

            #endregion

            AddTypeSearchAssembly(Assembly.GetCallingAssembly());
            //Invoker.AddTypeSearchAssembly(Type.GetType("Microsoft.SharePoint.SPSite,Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));

            lock (typeSearchAssemblies)
            {
                if (typeDictionary.ContainsKey(typeFullName))
                {
                    return (Type)typeDictionary[typeFullName];
                }
                else
                {
                    foreach (Assembly asm in typeSearchAssemblies)
                    {
                        Type type = asm.GetType(typeFullName, false, true);
                        if (type != null)
                        {
                            typeDictionary.Add(typeFullName, type);
                            return type;
                        }
                    }
                }
            }

            // this usually means something is wrong, should throw error
            throw new Exception("Cannot get type by name:" + typeFullName);
        }

        /// <summary>
        /// Add a test method. This method is used to return NULL instead of throwing an exception.
        /// Will be improved later.
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Type TryGetType(string typeFullName)
        {
            #region Get the type by Type.GetType method, if not, continue to use old method

            Type relatedType = null;

            try
            {
                relatedType = Type.GetType(typeFullName);
            }
            catch (Exception e) { string slipFxCop = e.Message; }

            if (relatedType != null)
            {
                return relatedType;
            }

            #endregion

            AddTypeSearchAssembly(Assembly.GetCallingAssembly());
            //Invoker.AddTypeSearchAssembly(Type.GetType("Microsoft.SharePoint.SPSite,Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));

            lock (typeSearchAssemblies)
            {
                if (typeDictionary.ContainsKey(typeFullName))
                {
                    return (Type)typeDictionary[typeFullName];
                }
                else
                {
                    foreach (Assembly asm in typeSearchAssemblies)
                    {
                        Type type = asm.GetType(typeFullName, false, true);
                        if (type != null)
                        {
                            typeDictionary.Add(typeFullName, type);
                            return type;
                        }
                    }
                }
            }
            return null;
        }



        // another way to get hidden type (from a known type)
        public static Type GetHiddenType(string hiddenTypeName, Type siblingType)
        {
            //Module typeModule = siblingType.Module;
            return siblingType.Module.GetType(hiddenTypeName, false, true);
        }

        public static void VerifyType(Object objSrc, Type tExpected)
        {
            if (objSrc == null)
                throw new Exception();

            if (objSrc.GetType().Equals(tExpected) ||
                objSrc.GetType().IsSubclassOf(tExpected))
                return;

            throw new Exception();
        }

        public static void VerifyType(Object objSrc, string typeFullName)
        {
            Type tExpected = Invoker.GetType(typeFullName);
            if (tExpected == null)
                throw new Exception();

            VerifyType(objSrc, tExpected);
        }

        public static void DumpMethods(Object obj)
        {
            Type type = obj.GetType();
            BindingFlags filter = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;
            MemberInfo[] members = type.GetMembers(filter);
            //Console.WriteLine("Class: {0}", type.FullName);
            logger.Info("Class: {0}", type.FullName);
            foreach (MemberInfo member in members)
            {
                //Console.WriteLine("Member: {0}", member.Name, member.MemberType.ToString());
                logger.Info("Member: {0}", member.Name, member.MemberType.ToString());
            }
        }

        /*private static string GetTypesString(Type[] types)
        {
            if (null == types) return "";

            string sRet = "";
            for (int i = 0; i < types.Length; i++)
            {
                if (i == 0)
                    sRet += types[i].Name;
                else
                    sRet += "," + types[i].Name;
            }
            return sRet;
        }*/

        private static Type[] GetTypesFromParams(params Object[] args)
        {
            if ((args == null) || (args.Length == 0))
                return Type.EmptyTypes;

            Type[] types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null) return null;

                types[i] = args[i].GetType();
            }
            return types;
        }

        public static MethodInfo GetMethod(Type type, string funcName, Type[] paramTypes)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase;

            MethodInfo methodInfo = null;
            Type currType = type;

            while (true)
            {
                if (paramTypes == null)     // just find name match, must to be unique
                    methodInfo = currType.GetMethod(funcName, flags);
                else
                    methodInfo = currType.GetMethod(funcName, flags, null, paramTypes, null);

                if (methodInfo != null)
                    return methodInfo;

                if (currType.Equals(typeof(Object)))
                    throw new Exception();

                currType = currType.BaseType;       // need to traverse down to get base type private members
            }
        }

        /// <summary>
        /// Get the method if exists.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="funcName"></param>
        /// <param name="paramTypes"></param>
        /// <returns></returns>
        public static MethodInfo GetNativeMethod(Type type, string funcName, Type[] paramTypes)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase;

            MethodInfo methodInfo = null;

            if (paramTypes == null)
            {
                methodInfo = type.GetMethod(funcName, flags);
            }
            else
            {
                methodInfo = type.GetMethod(funcName, flags, null, paramTypes, null);
            }
            return methodInfo;
        }


        /// <summary>
        /// This method is used to get the close Generic method info 
        /// </summary>
        /// <param name="type">the type contains the method</param>
        /// <param name="funcName">method  name</param>
        /// <param name="paramTypes">the method arguments types</param>
        /// <param name="typeArguments"></param>
        /// <returns>if contains the method, then return the close generic method with special type </returns>
        public static MethodInfo GetGenericMethod(Type type, String methodName, Type[] paramTypes, params Type[] typeArguments)
        {
            var result = default(MethodInfo);
            var methodInfo = GetMethod(type, methodName, paramTypes);
            if (methodInfo.IsGenericMethodDefinition)
                result = methodInfo.MakeGenericMethod(typeArguments);
            return result;
        }


        public static bool ExistMethod(Type type, string funcName, Type[] paramTypes)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase;

            MethodInfo methodInfo = null;
            Type currType = type;

            while (true)
            {
                if (paramTypes == null)     // just find name match, must to be unique
                    methodInfo = currType.GetMethod(funcName, flags);
                else
                    methodInfo = currType.GetMethod(funcName, flags, null, paramTypes, null);

                if (methodInfo != null)
                    return true;

                if (currType.Equals(typeof(Object)))
                    return false;

                currType = currType.BaseType;       // need to traverse down to get base type private members
            }
        }

        public static Object CallMethod(Object obj, MethodInfo method, params Object[] args)
        {
            return method.Invoke(obj, args);
        }

        public static Object CallMethod(Object obj, string funcName, Type[] paramTypes, params Object[] args)
        {
            if ((paramTypes != null) && (paramTypes.Length != args.Length))
                throw new Exception();

            Type type = obj.GetType();
            MethodInfo method = null;

            try
            {
                method = GetMethod(type, funcName, paramTypes);
            }
            catch (AmbiguousMatchException exxx)
            {
#if DEBUG
                throw new Exception();
#else
                // retry if caller just specified name matching
                if (paramTypes == null)
                    paramTypes = GetTypesFromParams(args);
                if (paramTypes == null)
                    throw new Exception(string.Format("Please specify types when calling method: {0}.{1}", type.FullName, funcName));
                method = GetMethod(type, funcName, paramTypes);
#endif
            }

            return method.Invoke(obj, args);

        }

        // use this when there is no overloaded functions
        public static Object CallMethod(Object obj, string funcName, params Object[] args)
        {
            return CallMethod(obj, funcName, null, args);
        }

        /// <summary>
        /// Call a Method with a Generic Arguments
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="funcName"></param>
        /// <param name="paramTypes"></param>
        /// <param name="args"></param>
        /// <param name="typeArguments">Generic Arguments</param>
        /// <returns></returns>
        public static Object CallGenericMethod(object obj, string funcName, Type[] paramTypes, Object[] args, params Type[] typeArguments)
        {
            if ((paramTypes != null) && (paramTypes.Length != args.Length))
                throw new Exception();

            Type type = obj.GetType();
            MethodInfo genericMethod = null;

            try
            {
                genericMethod = GetGenericMethod(type, funcName, paramTypes, typeArguments);
            }
            catch (AmbiguousMatchException exxx)
            {
#if DEBUG
                throw new Exception();
#else
                // retry if caller just specified name matching
                if (paramTypes == null)
                    paramTypes = GetTypesFromParams(args);
                if (paramTypes == null)
                    throw new Exception(string.Format("Please specify types when calling method: {0}.{1}", type.FullName, funcName));
                genericMethod = GetGenericMethod(type, funcName, paramTypes, typeArguments);
#endif
            }

            return genericMethod.Invoke(obj, args);

        }

        public static Object CallStaticMethod(Type type, string funcName, Type[] paramTypes, params Object[] args)
        {
            if ((paramTypes != null) && (paramTypes.Length != args.Length))
                throw new Exception();

            MethodInfo method = null;

            try
            {
                method = GetMethod(type, funcName, paramTypes);
            }
            catch (AmbiguousMatchException exxx)
            {
#if DEBUG
                throw;
#else
                // retry if caller just specified name matching
                if (paramTypes == null)
                    paramTypes = GetTypesFromParams(args);
                if (paramTypes == null)
                    throw new Exception(string.Format("Please specify types when calling method: {0}.{1}", type.FullName, funcName));
                method = GetMethod(type, funcName, paramTypes);
#endif
            }

            return method.Invoke(null, args);

        }

        // use this when there is no overloaded functions
        public static Object CallStaticMethod(Type type, string funcName, params Object[] args)
        {
            return CallStaticMethod(type, funcName, null, args);
        }

        public static Object CallStaticMethod(string typeFullName, string funcName, Type[] paramTypes, params Object[] args)
        {
            Type type = GetType(typeFullName);
            return CallStaticMethod(type, funcName, paramTypes, args);
        }

        // use this when there is no overloaded functions
        public static Object CallStaticMethod(string typeFullName, string funcName, params Object[] args)
        {
            return CallStaticMethod(typeFullName, funcName, null, args);
        }

        public static Object GetProperty(Object obj, string propName)
        {
            return CallMethod(obj, "get_" + propName);
        }

        /// <summary>
        /// Get the property if exists.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static Object TryGetProperty(Object obj, string propName)
        {
            MethodInfo propertyGetMethod = GetNativeMethod(obj.GetType(), "get_" + propName, null);
            if (propertyGetMethod != null)
            {
                return CallMethod(obj, "get_" + propName);
            }
            return null;
        }

        public static object GetStaticProperty(Type type, string propName)
        {
            return CallStaticMethod(type, "get_" + propName);
        }

        public static object GetStaticProperty(string typeFullName, string propName)
        {
            Type type = GetType(typeFullName);
            return GetStaticProperty(type, propName);
        }

        public static void SetProperty(Object obj, string propName, Object propValue)
        {
            CallMethod(obj, "set_" + propName, null, propValue);
        }

        public static void SetStaticProperty(Type type, string propName, Object propValue)
        {
            CallStaticMethod(type, "set_" + propName, null, propValue);
        }

        public static void SetStaticProperty(string typeFullName, string propName, Object propValue)
        {
            Type type = GetType(typeFullName);
            SetStaticProperty(type, propName, propValue);
        }

        public static FieldInfo GetField(Type type, string fieldName)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase;

            FieldInfo fieldInfo = null;
            Type currType = type;
            while (((fieldInfo = currType.GetField(fieldName, flags)) == null) &&
                    !currType.Equals(typeof(Object)))
                currType = currType.BaseType;       // need to traverse down to get base type private members

            if (fieldInfo == null)
                throw new Exception("No such field with name " + fieldName);

            return fieldInfo;
        }

        public static Object GetRawProperty(Object obj, string propName)
        {
            Type type = obj.GetType();
            FieldInfo field = GetField(type, propName);
            return field.GetValue(obj);
        }

        public static Object GetStaticRawProperty(Type type, string propName)
        {
            FieldInfo field = GetField(type, propName);
            return field.GetValue(null);
        }

        public static Object GetStaticRawProperty(string typeFullName, string propName)
        {
            Type type = GetType(typeFullName);
            return GetStaticRawProperty(type, propName);
        }

        public static void SetRawProperty(Object obj, string propName, Object propValue)
        {
            Type type = obj.GetType();
            FieldInfo field = GetField(type, propName);
            field.SetValue(obj, propValue);
        }

        public static void SetStaticRawProperty(Type type, string propName, Object propValue)
        {
            FieldInfo field = GetField(type, propName);
            field.SetValue(null, propValue);
        }

        public static void SetStaticRawProperty(string typeFullName, string propName, Object propValue)
        {
            Type type = GetType(typeFullName);
            SetStaticRawProperty(type, propName, propValue);
        }

        private static ConstructorInfo GetCtor(Type type, Type[] paramTypes)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase;
            ConstructorInfo ctorInfo = null;
            ctorInfo = type.GetConstructor(flags, null, paramTypes, null);
            if (ctorInfo == null)
                throw new Exception();
            return ctorInfo;
        }

        public static object CreateNewInstance(Type type, Type[] paramTypes, params Object[] args)
        {
            ConstructorInfo ctor = GetCtor(type, paramTypes);
            return ctor.Invoke(args);
        }

        public static object CreateNewInstance(Type type)
        {
            ConstructorInfo ctor = GetCtor(type, new Type[0]);
            return ctor.Invoke(new Object[0]);
        }

        public static object CreateNewInstance(string fullTypeName, Type[] paramTypes, params Object[] args)
        {
            Type type = GetType(fullTypeName);
            return CreateNewInstance(type, paramTypes, args);
        }

        public static object CreateNewInstance(string fullTypeName)
        {
            Type type = GetType(fullTypeName);
            return CreateNewInstance(type);
        }

        public static bool IsBaseType(Type target, Type baseType)
        {
            Type currType = target;
            while (!currType.Equals(typeof(Object)))
            {
                if (currType.Equals(baseType))
                {
                    return true;
                }
                currType = currType.BaseType;
            }
            return false;
        }

        public static bool IsBaseInterface(Type target, Type interfaceType)
        {
            Type[] interfaces = target.GetInterfaces();
            foreach (Type inter in interfaces)
            {
                if (inter.Equals(interfaceType))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
