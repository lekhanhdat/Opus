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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server19
{
    class AveServerAssemblyInit
    {
        static AveServerAssemblyInit()
        {
            mAssembly = Assembly.GetExecutingAssembly();
        }

        private static AveLogger log = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected static Assembly mAssembly;
        private const string mAveTypeNamePro = "AvePoint.ObjectModel.Server19.Ave";
        private const string mAveOffcieTypeNamePro = "AvePoint.ObjectModel.Server19.Office.Ave";

        public static void LoadAssembly()
        {
            AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
            AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.SharePoint.Search, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
            AveAssemblyUtility.AddTypeSearchAssembly(Assembly.LoadFile(System.Environment.GetEnvironmentVariable("CommonProgramFiles") + @"\Microsoft Shared\Web Server Extensions\16\CONFIG\BIN\Microsoft.SharePoint.ApplicationPages.dll"));
            //由于2013 Foundation中也添加了Search Service，所以需要Load该Dll，07和10均无需Load。
            AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.Office.Server.Search, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
            if (WrapperRuntime.CurrentContext.IsMoss)
            {
                AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.SharePoint.Portal, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
                AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.SharePoint.Taxonomy, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
                AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.SharePoint.Publishing, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
                AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.Office.InfoPath.Server, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
                AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.Office.DocumentManagement, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
                AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.Office.Server, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
                AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.Office.Policy, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
                AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.Office.Server.UserProfiles, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
                AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.SharePoint.WorkflowServices, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));
                AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load("Microsoft.SharePoint.Search.Extended.Administration, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"));

            }
        }

        public static object CreateElement(Type aveBaseInterfaceType, object obj)
        {
            return CreateElement(aveBaseInterfaceType, new object[] { obj });
        }

        public static object CreateElement(Type aveBaseInterfaceType, object[] paramsObj)
        {
            if (paramsObj == null)
            {
                return null;
            }
            object paramObj = paramsObj.Last<object>();
            Type instanceType = GetAveType(aveBaseInterfaceType, paramObj);
            object retObj = null;
            try
            {
                retObj = Activator.CreateInstance(instanceType, paramsObj);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, ServerAPIResource.CreateElementInstanceError, e.ToString());
            }
            return retObj;
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

        internal static Type GetAveType(Type aveBaseInterfaceType, object unknowTypeObj)
        {
            Type retType = null;
            if (unknowTypeObj == null)
            {
                return null;
            }
            Type currType = unknowTypeObj.GetType();
            while (true)
            {
                if (currType != typeof(object))
                {
                    retType = GetRealType(currType);
                }
                else
                {
                    break;
                }
                if (retType != null)
                {
                    return retType;
                }
                currType = currType.BaseType;
            }

            if (retType == null)
            {
                foreach (Type type in unknowTypeObj.GetType().GetInterfaces())
                {
                    retType = GetRealType(type);
                    if (retType != null)
                        return retType;
                }
                //if we can not get type from Ave assemby, that means we did not implement this type, then we will get it parent type from T;
                if (aveBaseInterfaceType.Name.Substring(4).StartsWith("O", StringComparison.OrdinalIgnoreCase))
                {
                    retType = mAssembly.GetType(mAveOffcieTypeNamePro + aveBaseInterfaceType.Name.Substring(4));
                }
                else
                {
                    retType = mAssembly.GetType(mAveTypeNamePro + aveBaseInterfaceType.Name.Substring(4));
                }
            }
            return retType;
        }

        internal static Type GetRealType(Type currType)
        {
            string[] typeNames = currType.FullName.Split('.');
            string typeName = typeNames[typeNames.Length - 1];
            string aveTypeName = string.Empty;
            if (typeName.Contains('+'))
            {
                typeName = typeName.Split('+').Last<string>();
            }
            switch (typeNames[1])
            {
                case "SharePoint":
                    if (typeName.StartsWith("SP", StringComparison.Ordinal))
                    {
                        typeName = typeName.Substring(2);
                    }
                    aveTypeName = mAveTypeNamePro + typeName;
                    break;
                case "Office":
                    aveTypeName = mAveOffcieTypeNamePro + "O" + typeName;
                    break;
                default:
                    if (typeName.StartsWith("SP", StringComparison.Ordinal))
                    {
                        typeName = typeName.Substring(2);
                    }
                    aveTypeName = mAveTypeNamePro + typeName;
                    break;
            }
            return mAssembly.GetType(aveTypeName);
        }
    }
}
