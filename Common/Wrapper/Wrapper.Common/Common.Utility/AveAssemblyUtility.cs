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
using System.Reflection;
using System.Collections;
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Common
{
    public static class AveAssemblyUtility
    {

        private static Hashtable typeMap = new Hashtable();
        private static List<Assembly> typeSearchAssemblies = new List<Assembly>();

        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private const BindingFlags INVOKEFLAGS = BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.IgnoreCase;

        private static MethodInfo GetMethodInternal(Type type, string methodName, Type[] paramTypes)
        {
            #region ArgumentCheck
            if (type == null) throw new ArgumentNullException("type");

            MethodInfo methodInfo = null;
            if (paramTypes == null)
            {
                methodInfo = type.GetMethod(methodName, INVOKEFLAGS);
            }
            else
            {
                methodInfo = type.GetMethod(methodName, INVOKEFLAGS, null, paramTypes, null);
            }
            #endregion
            if (methodInfo == null)
            {
                throw new AveNullResultException(string.Format(WrapperCommonResource.AWCAveAssUtilGetMethodInternal, type.FullName, methodName, GetTypesInfo(paramTypes)));
            }
            return methodInfo;
        }

        private static PropertyInfo GetPropertyInternal(Type type, string propertyName)
        {
            #region CheckPara
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            #endregion

            PropertyInfo propertyInfo = type.GetProperty(propertyName, INVOKEFLAGS);
            if (propertyInfo == null)
            {
                throw new AveNullResultException(string.Format(WrapperCommonResource.AWCAveAssUtilGetPropertyInternal, type.FullName, propertyName));
            }
            return propertyInfo;
        }

        private static FieldInfo GetFieldInternal(Type type, string fieldName)
        {
            #region CheckPara
            if (type == null) throw new ArgumentNullException("type");
            #endregion

            FieldInfo fieldInfo = type.GetField(fieldName, INVOKEFLAGS);
            if (fieldInfo == null)
            {
                throw new AveNullResultException(string.Format(WrapperCommonResource.AWCAveAssUtilGetFieldInternal, type.FullName, fieldName));
            }
            return fieldInfo;
        }

        private static ConstructorInfo GetCtorInternal(Type type, Type[] paramTypes)
        {
            if (type == null) throw new ArgumentNullException("type");

            const BindingFlags ctorFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;
            ConstructorInfo ctorInfo = type.GetConstructor(ctorFlag, null, paramTypes, null);
            if (ctorInfo == null)
            {
                throw new AveNullResultException(string.Format(WrapperCommonResource.AWCAveAssUtilGetCtorInternal, type.FullName, GetTypesInfo(paramTypes)));
            }
            return ctorInfo;
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

        private static Type[] GetTypesFromParams(Object[] args)
        {
            if ((args == null) || (args.Length == 0))
                return Type.EmptyTypes;

            Type[] types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                    return null;
                types[i] = args[i].GetType();
            }
            return types;
        }

        private static string GetTypesInfo(Type[] types)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ParaTypeInfo:");
            if (types == null || types.Length == 0)
            {
                return sb.Append("types[] has 0 element").ToString();
            }
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == null) continue;

                sb.Append("Para");
                sb.Append(i);
                sb.Append(":");
                sb.Append(types[i].FullName);
                sb.Append("     ");
            }
            return sb.ToString();
        }

        private static MethodInfo GetGenericMethodInternal(Type type, string methodName, Type[] paramTypes, Type[] typeArguments, bool throwsException = false)
        {
            if (type == null) throw new ArgumentNullException("type");
            MethodInfo methodInfo = null;
            Exception ex = null;
            try
            {
                methodInfo = type.GetMethod(methodName, INVOKEFLAGS);//通过name去找，只能找到methodname没有重载的情况
            }
            catch (AmbiguousMatchException ambiguousMatchException)
            {
                ex = ambiguousMatchException;
            }
            catch (Exception e)
            {
                ex = e;
            }

            int matchedNumber = 0;
            if (methodInfo == null)
            {
                foreach (MethodInfo method in type.GetMethods(INVOKEFLAGS)) //遍历所有方法，可以找到有重载，但是重载参数个数不同的情况
                {
                    if (string.Equals(method.Name, methodName, StringComparison.OrdinalIgnoreCase) && method.GetParameters().Length == paramTypes.Length)
                    {
                        methodInfo = method;
                        ++matchedNumber;
                    }
                }
            }
            try
            {
                if (matchedNumber > 1) //泛型方法参数个数相同的重载，必须通过传入的paraTypes来找
                {
                    methodInfo = type.GetMethod(methodName, INVOKEFLAGS, null, paramTypes, null);
                }
            }
            catch (Exception e)
            {
                ex = e;
            }

            var exception = new AveNullResultException(string.Format(WrapperCommonResource.AWCAveAssUtilGetMethodInternal, type.FullName, methodName + "(Generic)", GetTypesInfo(paramTypes)), ex);
            if (methodInfo == null)
            {
                if (throwsException)
                {
                    throw exception;
                }
                logger.Warn(exception.ToString());
                return null;
            }
            if (methodInfo.ContainsGenericParameters)
            {
                methodInfo = methodInfo.MakeGenericMethod(typeArguments);
            }
            return methodInfo;
        }

        public static void SetFieldValue(object target, string fieldName, object value)
        {
            if (target == null) throw new ArgumentNullException("target");
            SetFieldValue(target, target.GetType(), fieldName, value);
        }

        public static void SetFieldValue(object target, Type objType, string fieldName, object value, bool throwsException = false)
        {
            FieldInfo field;
            try
            {
                field = GetFieldInternal(objType, fieldName);
            }
            catch (AveNullResultException e)
            {
                if (throwsException)
                {
                    throw;
                }
                else
                {
                    logger.Warn(e.ToString());
                }
                return;
            }
            field.SetValue(target, value);
        }

        public static object GetFieldValue(object target, Type objType, string fieldName, bool throwsException = false)
        {
            FieldInfo fieldInfo;
            try
            {
                fieldInfo = GetFieldInternal(objType, fieldName);
            }
            catch (AveNullResultException e)
            {
                if (throwsException)
                {
                    throw;
                }
                else
                {
                    logger.Warn(e.ToString());
                }
                return null;
            }
            return fieldInfo.GetValue(target);
        }

        public static object GetFieldValue(object target, string fieldName)
        {
            if (target == null) throw new ArgumentNullException("target");
            return GetFieldValue(target, target.GetType(), fieldName);
        }

        public static void SetPropertyValue(object target, string propertyName, object value, bool throwsException = false)
        {
            if (target == null) throw new ArgumentNullException("target");
            SetPropertyValue(target, target.GetType(), propertyName, value, throwsException);
        }

        public static void SetPropertyValue(object target, Type type, string propertyName, object value, bool throwsException = false)
        {

            PropertyInfo proInfo;
            try
            {
                proInfo = GetPropertyInternal(type, propertyName);
            }
            catch (AveNullResultException e)
            {
                if (throwsException)
                {
                    throw;
                }
                else
                {
                    logger.Warn(e.ToString());
                }
                return;
            }
            proInfo.SetValue(target, value, null);
        }

        public static void SetPropertyValueByType(object target, string propertyName, object value, Type propertyType)
        {
            if (target == null) throw new ArgumentNullException("target");

            Type objType = target.GetType();
            PropertyInfo property = null;
            if (propertyType == null)
            {
                property = objType.GetProperty(propertyName, INVOKEFLAGS);
            }
            else
            {
                property = objType.GetProperty(propertyName, propertyType);
            }
            property.SetValue(target, value, null);
        }

        public static void SetStaticPropertyValue(Type type, string propName, object propValue)
        {
            SetPropertyValue(null, type, propName, propValue);
        }



        public static object GetPropertyValue(object target, string propertyName)
        {
            if (target == null) throw new ArgumentNullException("target");
            return GetPropertyValue(target, target.GetType(), propertyName);
        }

        public static object GetPropertyValueByType(object target, string propertyName, Type propertyType)
        {
            if (target == null) throw new ArgumentNullException("target");
            Type objType = target.GetType();
            return GetPropertyValueBySpecialType(target, objType, propertyName, propertyType);
        }

        public static object GetStaticPropertyValue(Type objType, string propertyName)
        {
            return GetPropertyValue(null, objType, propertyName);
        }

        public static object GetStaticPropertyValue(string typeFullName, string propName)
        {
            Type type = GetType(typeFullName);
            return GetStaticPropertyValue(type, propName);
        }

        public static object GetPropertyValue(object target, Type objType, string propertyName, bool throwsException = false)
        {
            PropertyInfo propInfo;
            try
            {
                propInfo = GetPropertyInternal(objType, propertyName);
            }
            catch (AveNullResultException e)
            {
                if (throwsException)
                {
                    throw;
                }
                else
                {
                    logger.Warn(e.ToString());
                }
                return null;
            }
            return propInfo.GetValue(target, null);
        }

        private static object GetPropertyValueBySpecialType(object target, Type objType, string propertyName, Type propertyType)
        {
            PropertyInfo property = null;
            if (propertyType == null)
            {
                property = objType.GetProperty(propertyName, INVOKEFLAGS);
            }
            else
            {
                property = objType.GetProperty(propertyName, propertyType);
            }
            if (property != null)
            {
                return property.GetValue(target, null);
            }
            return null;
        }

        public static object CreateInstance(string assemblyName, string typeName)
        {
            return CreateInstance(assemblyName, typeName, new Type[] { }, new object[] { });
        }

        public static object CreateInstance(Type type, Type[] paramTypes, object[] args, bool throwsException = false)
        {
            ConstructorInfo ctor;
            try
            {
                ctor = GetCtorInternal(type, paramTypes);
            }
            catch (AveNullResultException e)
            {
                if (throwsException)
                {
                    logger.Warn(e.ToString());
                }
                else
                {
                    throw;
                }
                return null;
            }
            return ctor.Invoke(args);
        }

        public static object CreateInstance(string fullTypeName, Type[] paramTypes, params object[] args)
        {
            Type type = GetType(fullTypeName);
            return CreateInstance(type, paramTypes, args);
        }

        public static object CreateInstance(string fullTypeName)
        {
            Type type = GetType(fullTypeName);
            return CreateInstanceByType(type);
        }

        public static object CreateInstance(string assemblyName, string typeName, Type[] constructorParamType, object[] constructorParam)
        {
            Assembly assembly = Assembly.Load(new AssemblyName(assemblyName));
            return CreateInstance(assembly, typeName, constructorParamType, constructorParam);
        }

        public static object CreateInstance(Assembly assembly, string typeName, Type[] constructorParamType, object[] constructorParam)
        {
            Type type = assembly.GetType(typeName);
            if (type != null)
            {
                ConstructorInfo construtor = GetCtorInternal(type, constructorParamType);
                if (construtor != null)
                {
                    return construtor.Invoke(constructorParam);
                }
            }
            return null;
        }

        public static object CreateInstanceByType(Type type)
        {
            return CreateInstance(type, new Type[0], new object[0]);
        }

        public static object InvokeMethod(object target, string methodName, Type[] paramTypes, params object[] args)
        {
            if (target == null) throw new ArgumentNullException("target");

            return InvokeMethod(target, target.GetType(), methodName, paramTypes, args);
        }

        public static object InvokeMethod(object target, string methodName, params object[] args)
        {
            var paramTypes = GetTypesFromParams(args);
            return InvokeMethod(target, target.GetType(), methodName, paramTypes, args);
        }

        public static object InvokeMethod(object target, Type type, string methodName, object[] args = null, bool throwsException = false)
        {
            var paramTypes = GetTypesFromParams(args);
            return InvokeMethod(target, type, methodName, paramTypes, args, throwsException);
        }

        /// <summary>
        /// use this method to invoke method is suggested
        /// </summary>
        /// <param name="target">The object on which to invoke the method. If a method is static, this argument is ignored.</param>
        /// <param name="type">The object type on which to invoke the method. If a method is static, this argument is required</param>
        /// <param name="methodName">The string containing the name of the method to get.</param>
        /// <param name="paramTypes">An array of Type objects representing the number, order, and type of the parameters for the method to get. -or - An empty array of Type objects (as provided by the EmptyTypes field) to get a method that takes no parameters. </param>
        /// <param name="args">An argument list for the invoked method . This is an array of objects with the same number, order, and type as the parameters of the method  to be invoked. If there are no parameters, parameters should be object[0]</param>
        /// <param name="throwsException">set true if you want to throw exception while method not found,false to print warning log instead.</param>
        /// <returns></returns>
        public static object InvokeMethod(object target, Type type, string methodName, Type[] paramTypes, object[] args, bool throwsException = false)
        {
            //如果args中有一个值是null，在GetTypesFromParams方法中paramTypes返回null，引起在该方法中找不到methodInfo,在此先将||改成&&
            if (paramTypes == null && args == null)
            {
                paramTypes = new Type[0];
                args = new object[0];
            }
            if (type == null && target != null)
            {
                type = target.GetType();
            }
            MethodInfo methodInfo;
            try
            {
                methodInfo = GetMethodInternal(type, methodName, paramTypes);
            }
            catch (AveNullResultException e)
            {
                if (throwsException)
                {
                    throw;
                }
                else
                {
                    logger.Warn(e.ToString());
                }
                return null;
            }
            return methodInfo.Invoke(target, args);
        }

        public static object InvokeGenericMethod(object target, string methodName, object[] args, params Type[] typeArguments)
        {
            var paraType = GetTypesFromParams(args);
            return InvokeGenericMethod(target, methodName, paraType, args, typeArguments);
        }

        public static object InvokeGenericMethod(object target, string methodName, Type[] paramTypes, object[] args, Type[] typeArguments)
        {
            return InvokeGenericMethod(target, target.GetType(), methodName, paramTypes, args, typeArguments);
        }

        public static object InvokeGenericMethod(object target, Type type, string methodName, Type[] paramTypes, object[] args, Type[] typeArguments, bool throwsException = false)
        {
            MethodInfo genericMethod = GetGenericMethodInternal(type, methodName, paramTypes, typeArguments);
            if (genericMethod == null)
            {
                return null;
            }
            return genericMethod.Invoke(target, args);
        }

        public static object InvokeGenericStaticMethod(Type type, string methodName, Type[] paramTypes, object[] args, Type[] typeArguments)
        {
            return InvokeGenericMethod(null, type, methodName, paramTypes, args, typeArguments);
        }

        public static object InvokeStaticMethod(Type type, string methodName, Type[] paramTypes, params object[] args)
        {
            return InvokeMethod(null, type, methodName, paramTypes, args);
        }

        public static object InvokeStaticMethod(Type type, string methodName, params object[] args)
        {
            return InvokeMethod(null, type, methodName, args);
        }

        public static object InvokeStaticMethod(string typeFullName, string methodName, params object[] args)
        {
            var paramTypes = GetTypesFromParams(args);
            return InvokeStaticMethod(typeFullName, methodName, paramTypes, args);
        }

        public static object InvokeStaticMethod(string typeFullName, string methodName, Type[] paramTypes, params object[] args)
        {
            Type type = GetType(typeFullName);
            return InvokeStaticMethod(type, methodName, paramTypes, args);
        }

        public static Type GetType(string assemblyName, string type)
        {
            Assembly assembly = Assembly.Load(new AssemblyName(assemblyName));

            if (assembly != null)
            {
                return assembly.GetType(type);
            }
            return null;
        }

        public static Type GetType(string typeFullName)
        {
            AddTypeSearchAssembly(Assembly.GetCallingAssembly());

            lock (typeSearchAssemblies)
            {
                if (typeMap.ContainsKey(typeFullName))
                {
                    return (Type)typeMap[typeFullName];
                }
                foreach (Assembly asm in typeSearchAssemblies)
                {
                    Type type = asm.GetType(typeFullName, false, true);
                    if (type != null)
                    {
                        typeMap.Add(typeFullName, type);
                        return type;
                    }
                }
            }

            // this usually means something is wrong, should throw error
            throw new Exception("Cannot get type by name:" + typeFullName);
        }

        public static Type GetGenerticType(Type aveType, string typeMapping)
        {
            AveType aveTypeFlag;
            if (!string.IsNullOrEmpty(typeMapping))
            {
                foreach (Assembly assembly in typeSearchAssemblies)
                {
                    Type retType = assembly.GetType(typeMapping);
                    if (retType != null)
                    {
                        return retType;
                    }
                }
            }
            string[] typeNames = aveType.FullName.Split('.');
            string typeFlagName = typeNames[typeNames.Length - 2];
            string typeName = typeNames[typeNames.Length - 1];
            string spTypeNamePro = string.Empty;
            switch (typeFlagName)
            {
                case "Office":
                    spTypeNamePro = typeName.Substring(5);
                    aveTypeFlag = AveType.Office;
                    break;
                default:
                    spTypeNamePro = typeName.Substring(4);
                    aveTypeFlag = AveType.SharePoint;
                    break;
            }
            List<Type> matchedType = new List<Type>();
            foreach (Assembly assembly in typeSearchAssemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    switch (aveTypeFlag)
                    {
                        case AveType.Office:
                            if (type.Name.Equals(spTypeNamePro, StringComparison.Ordinal))
                            {
                                matchedType.Add(type);
                            }
                            break;
                        case AveType.SharePoint:
                            if (type.Name == "SP" + spTypeNamePro || type.Name.Equals(spTypeNamePro, StringComparison.Ordinal))
                            {
                                matchedType.Add(type);
                            }
                            break;
                    }
                }
            }

            if (matchedType.Count == 1)
            {
                return matchedType[0];
            }
            return null;
        }

        public static void SetStaticFieldValue(Assembly assem, string typeName, string fieldName, object value)
        {
            Type type = assem.GetType(typeName);
            SetFieldValue(null, type, fieldName, value);
        }



        //public static void GetAllFieldValues(object obj, IDictionary<string, object> fields)
        //{
        //    if (obj == null)
        //    {
        //        return;
        //    }
        //    Type objType = obj.GetType();
        //    foreach (FieldInfo fieldInfo in objType.GetFields(INVOKEFLAGS))
        //    {
        //        object fieldValue = fieldInfo.GetValue(obj);
        //        if (fieldValue == null)
        //        {
        //            continue;
        //        }
        //        fields[fieldInfo.Name] = fieldInfo.GetValue(obj);
        //    }
        //}

        //public static void GetAllFieldValues(object obj, IDictionary<string, object> fields, IDictionary<string, IEnumerable> collectionField)
        //{
        //    if (obj == null)
        //    {
        //        return;
        //    }
        //    Type objType = obj.GetType();
        //    foreach (FieldInfo fieldInfo in objType.GetFields(INVOKEFLAGS))
        //    {
        //        object fieldValue = fieldInfo.GetValue(obj);
        //        if (fieldValue == null)
        //        {
        //            continue;
        //        }
        //        if (fieldValue is IEnumerable)
        //        {
        //            if (fieldValue is Array && fieldValue.GetType().GetElementType() == typeof(byte))
        //            {
        //                fields[fieldInfo.Name] = fieldValue;
        //            }
        //            else
        //            {
        //                collectionField[fieldInfo.Name] = (IEnumerable)fieldValue;
        //            }
        //        }
        //        else
        //        {
        //            fields[fieldInfo.Name] = fieldValue;
        //        }
        //    }
        //}

        //public static void SetAllFieldValues(object obj, IDictionary<string, object> data)
        //{
        //    if (obj == null)
        //    {
        //        return;
        //    }
        //    Type objType = obj.GetType();
        //    foreach (KeyValuePair<string, object> kv in data)
        //    {
        //        FieldInfo fieldInfo = objType.GetField(kv.Key);
        //        if (fieldInfo != null)
        //        {
        //            fieldInfo.SetValue(obj, kv.Value);
        //        }
        //    }
        //}

        //public static void SetStaticPropertyValue(string typeFullName, string propName, object propValue)
        //{
        //    Type type = GetType(typeFullName);
        //    SetStaticPropertyValue(type, propName, propValue);
        //}

        //public static void AddTypeSearchAssemblyTree(Assembly asm)
        //{
        //    if (asm == null) return;

        //    lock (typeSearchAssemblies)
        //    {
        //        if (!typeSearchAssemblies.Contains(asm))
        //        {
        //            typeSearchAssemblies.Add(asm);

        //            // get reference assemblies
        //            AssemblyName[] childrenAsmNames = asm.GetReferencedAssemblies();
        //            foreach (AssemblyName asmName in childrenAsmNames)
        //            {
        //                // this may take more space, which is normally OK
        //                try
        //                {
        //                    Assembly child = Assembly.Load(asmName);
        //                    if (!typeSearchAssemblies.Contains(child))
        //                        typeSearchAssemblies.Add(child);
        //                }
        //                catch { }//no need to log
        //            }
        //        }
        //    }
        //}

        //public static void AddTypeSearchAssembly(Type seedType)
        //{
        //    AddTypeSearchAssembly(seedType.Assembly);
        //}


    }

    [Serializable]
    public class AveNullResultException : Exception
    {
        public AveNullResultException()
        {
        }

        public AveNullResultException(string message)
            : base(message)
        {
        }

        public AveNullResultException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected AveNullResultException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }



    public enum AveType
    {
        SharePoint,
        Office
    }
}
