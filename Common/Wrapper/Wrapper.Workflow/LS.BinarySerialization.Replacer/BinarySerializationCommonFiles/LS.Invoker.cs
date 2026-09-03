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
using System.Text;
using System.Collections;
using System.Reflection;
using System.IO;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;


namespace LS
{
    public class LSInvoker
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static Type SqlRemoteBlobSessionClass;
        public static Type SqlSessionClass;
        public static Type SPDocumentContentDataClass;
        public static Type SPConfigurationDatabaseClass;
        public static Type SqlRemoteBlobsAssemblyClass;
        public static Type SPSiteStreamCopierClass;

        public static void GetAllTypes()
        {
            Assembly asm = Assembly.Load("Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
            SqlRemoteBlobSessionClass = asm.GetType("Microsoft.SharePoint.SqlRemoteBlobSession", false, true);
            SqlSessionClass = asm.GetType("Microsoft.SharePoint.Utilities.SqlSession",false,true);
            SPDocumentContentDataClass = asm.GetType("Microsoft.SharePoint.SPDocumentContentData", false, true);
            SPConfigurationDatabaseClass = asm.GetType("Microsoft.SharePoint.Administration.SPConfigurationDatabase", false, true);
            SqlRemoteBlobsAssemblyClass = asm.GetType("Microsoft.SharePoint.RBSWrapper.SqlRemoteBlobsAssembly", false, true);
            SPSiteStreamCopierClass = asm.GetType("Microsoft.SharePoint.SPSiteStreamCopier", false, true);
        }

        
        
        private static Hashtable m_TypeMap = new Hashtable();
        private static List<Assembly> m_TypeSearchAssemblies = new List<Assembly>();

        public static void AddTypeSearchAssemblyTree(Assembly asm)
        {
            if (asm == null) return;

            lock (m_TypeSearchAssemblies)
            {
                if (!m_TypeSearchAssemblies.Contains(asm))
                {
                    m_TypeSearchAssemblies.Add(asm);

                    // get reference assemblies
                    AssemblyName[] childrenAsmNames = asm.GetReferencedAssemblies();
                    foreach (AssemblyName asmName in childrenAsmNames)
                    {
                        // this may take more space, which is normally OK
                        try
                        {
                            Assembly child = Assembly.Load(asmName);
                            if (!m_TypeSearchAssemblies.Contains(child))
                                m_TypeSearchAssemblies.Add(child);
                        }
                        catch(Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.LoadAssemblyError, e.ToString());
                        }//need not to log
                    }
                }
            }
        }

        public static void AddTypeSearchAssembly(Assembly asm)
        {
            if (asm == null) return;

            lock (m_TypeSearchAssemblies)
            {
                if (!m_TypeSearchAssemblies.Contains(asm))
                {
                    m_TypeSearchAssemblies.Add(asm);
                }
            }
        }

        public static void AddTypeSearchAssembly(Type seedType)
        {
            AddTypeSearchAssembly(seedType.Assembly);
        }

        public static Type GetType(string typeFullName)
        {
            AddTypeSearchAssembly(Assembly.GetCallingAssembly());

            lock (m_TypeSearchAssemblies)
            {
                if (m_TypeMap.ContainsKey(typeFullName))
                {
                    return (Type)m_TypeMap[typeFullName];
                }
                else
                {
                    foreach (Assembly asm in m_TypeSearchAssemblies)
                    {
                        Type type = asm.GetType(typeFullName, false, true);
                        if (type != null)
                        {
                            m_TypeMap.Add(typeFullName, type);
                            return type;
                        }
                    }
                }
            }

            // this usually means something is wrong, should throw error
            throw new Exception("Cannot find type info for '{0}'." + typeFullName);
        }

        public static Type GetType(string assemName, string className)
        {
            try
            { 
                if(string.IsNullOrEmpty(assemName) || string.IsNullOrEmpty(className))
                    return null;
                Assembly assem=null;
                if(assemName.IndexOf('\\')>0)
                    assem=Assembly.LoadFrom(assemName);
                else
                    assem=Assembly.Load(assemName);

                Type type=assem.GetType(className);
                return type;
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetObjectTypeError, e.ToString());
                return null;
            }
            
        }

        // another way to get hidden type (from a known type)
        public static Type GetHiddenType(string hiddenTypeName, Type siblingType)
        {
            Module typeModule = siblingType.Module;
            return typeModule.GetType(hiddenTypeName, false, true);
        }

        public static void VerifyType(Object objSrc, Type tExpected)
        {
            if (objSrc == null)
                throw new Exception("Input object is empty. (Expected Type="+ tExpected.FullName+").");

            if (objSrc.GetType().Equals(tExpected) ||
                objSrc.GetType().IsSubclassOf(tExpected))
                return;

            throw new Exception("Object {0] type '{1}' does not match expected type: {2}");
        }

        public static void VerifyType(Object objSrc, string typeFullName)
        {
            Type tExpected = LSInvoker.GetType(typeFullName);
            if (tExpected == null)
                throw new Exception("Cannot find type info: "+ typeFullName);

            VerifyType(objSrc, tExpected);
        }

        public static void DumpMethods(Object obj)
        {
            Type type = obj.GetType();
            BindingFlags filter = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;
            MemberInfo[] members = type.GetMembers(filter);
            //Console.WriteLine("Class: {0}", type.FullName);
            log.Debug($"Class: {type.FullName}");
            foreach (MemberInfo member in members)
            {
                //Console.WriteLine("Member: {0}", member.Name, member.MemberType.ToString());
                log.Debug("Member: {0}", member.Name, member.MemberType.ToString());
            }
        }

        private static string GetTypesString(Type[] types)
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
        }

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
                    throw new Exception("Cannot find method: "+type.FullName+"."+funcName+GetTypesString(paramTypes));

                currType = currType.BaseType;       // need to traverse down to get base type private members
            }
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
            try
            {
                return method.Invoke(obj, args);
            }
            catch(Exception e)
            {
                throw new Exception("Exception when call invoker.",e);
            }
        }

        public static Object CallMethod(Object obj, string funcName, Type[] paramTypes, params Object[] args)
        {
            if ((paramTypes != null) && (paramTypes.Length != args.Length))
                throw new Exception("Input number of args is invalid for "+ funcName+ GetTypesString(paramTypes)+":"+ args.Length);

            Type type = obj.GetType();
            MethodInfo method = null;

            try
            {
                method = GetMethod(type, funcName, paramTypes);
            }
            catch (AmbiguousMatchException)
            {
#if DEBUG
                throw new Exception("Please specify types when calling method: "+ type.FullName+"."+ funcName);
#else
                // retry if caller just specified name matching
                if (paramTypes == null)
                    paramTypes = GetTypesFromParams(args);
                if (paramTypes == null)
                    throw new Exception("Please specify types when calling method: "+ type.FullName+"."+ funcName);
                method = GetMethod(type, funcName, paramTypes);
#endif
            }
            try
            {
                return method.Invoke(obj, args);
            }
            catch(Exception e)
            {
                throw new Exception("Exception when call invoker.",e);
            }
        }

        // use this when there is no overloaded functions
        public static Object CallMethod(Object obj, string funcName, params Object[] args)
        {
            return CallMethod(obj, funcName, null, args);
        }

        public static Object CallStaticMethod(Type type, string funcName, Type[] paramTypes, params Object[] args)
        {
            if ((paramTypes != null) && (paramTypes.Length != args.Length))
                throw new Exception("Input number of args is invalid for {0}({1}): {2}"+ funcName+"("+ GetTypesString(paramTypes)+"):"+ args.Length);

            MethodInfo method = null;

            try
            {
                method = GetMethod(type, funcName, paramTypes);
            }
            catch (AmbiguousMatchException)
            {
#if DEBUG
                throw new Exception("Please specify types when calling method: " +type.FullName+"."+ funcName);
#else
                // retry if caller just specified name matching
                if (paramTypes == null)
                    paramTypes = GetTypesFromParams(args);
                if (paramTypes == null)
                    throw new Exception("Please specify types when calling method: "+ type.FullName+"."+ funcName);
                method = GetMethod(type, funcName, paramTypes);
#endif
            }
            try
            {
                return method.Invoke(null, args);
            }
            catch(Exception e)
            {
                throw new Exception("Exception when call invoker.",e);
            }
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

        private static FieldInfo GetField(Type type, string fieldName)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase;

            FieldInfo fieldInfo = null;
            Type currType = type;
            while (((fieldInfo = currType.GetField(fieldName, flags)) == null) &&
                    !currType.Equals(typeof(Object)))
                currType = currType.BaseType;       // need to traverse down to get base type private members

            if (fieldInfo == null)
                throw new Exception("Cannot find field: {0}.{1}"+ type.FullName+"."+ fieldName);

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
                throw new Exception("Cannot find constructor for: {0}({1})"+ type.FullName+"("+ GetTypesString(paramTypes)+")");
            return ctorInfo;
        }

        public static object CreateNewInstance(Type type, Type[] paramTypes, params Object[] args)
        {
            ConstructorInfo ctor = GetCtor(type, paramTypes);
            try
            {
                return ctor.Invoke(args);
            }
            catch(Exception e)
            {
                throw new Exception("Exception when call invoker.",e);
            }
        }

        public static object CreateNewInstance(Type type)
        {
            ConstructorInfo ctor = GetCtor(type, new Type[0]);
            try
            {
                return ctor.Invoke(new Object[0]);
            }
            catch(Exception e)
            {
                throw new Exception("Exception when call invoker.",e);
            }
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

