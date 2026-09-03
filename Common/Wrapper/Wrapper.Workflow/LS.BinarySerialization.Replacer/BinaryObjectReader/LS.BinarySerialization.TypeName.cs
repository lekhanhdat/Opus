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
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace LS.BinarySerialization
{
    internal class TypeName
    {
        // Methods
        private TypeName()
        { }
        internal static Type GetType(Assembly initialAssembly, string fullTypeName)
        {
            int num;
            ITypeName typeNameInfo = ((ITypeNameFactory)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid(0xb81ff171, 0x20f3, 0x11d2, 0x8d, 0xcc, 0, 160, 0xc9, 0xb0, 5, 0x25)))).ParseTypeName(fullTypeName, out num);
            Type type2 = null;
            if (num == -1)
            {
                type2 = LoadTypeWithPartialName(typeNameInfo, initialAssembly, fullTypeName);
            }
            return type2;

        }
        private static Type LoadTypeWithPartialName(ITypeName typeNameInfo, Assembly initialAssembly, string fullTypeName)
        {
            Type type2;
            uint nameCount = typeNameInfo.GetNameCount();
            uint typeArgumentCount = typeNameInfo.GetTypeArgumentCount();
            IntPtr[] ptrArray = new IntPtr[nameCount];
            IntPtr[] ptrArray2 = new IntPtr[typeArgumentCount];
            try
            {
                Type nestedType = null;
                if (nameCount == 0)
                {
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Remoting_BadType"), new object[] { fullTypeName }));
                }
                GCHandle handle = GCHandle.Alloc(ptrArray, GCHandleType.Pinned);
                nameCount = typeNameInfo.GetNames(nameCount, handle.AddrOfPinnedObject());
                handle.Free();
                string name = Marshal.PtrToStringBSTR(ptrArray[0]);
                string assemblyName = typeNameInfo.GetAssemblyName();
                if (!string.IsNullOrEmpty(assemblyName))
                {
                    Assembly assembly = Assembly.LoadWithPartialName(assemblyName);
                    if (assembly == null)
                    {
                        assembly = Assembly.LoadWithPartialName(new AssemblyName(assemblyName).Name);
                    }
                    nestedType = assembly.GetType(name);
                }
                else if (initialAssembly != null)
                {
                    nestedType = initialAssembly.GetType(name);
                }
                else
                {
                    nestedType = Type.GetType(name);
                }
                if (nestedType == null)
                {
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Remoting_BadType"), new object[] { fullTypeName }));
                }
                for (int i = 1; i < nameCount; i++)
                {
                    string str3 = Marshal.PtrToStringBSTR(ptrArray[i]);
                    nestedType = nestedType.GetNestedType(str3, BindingFlags.NonPublic | BindingFlags.Public);
                    if (nestedType == null)
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Remoting_BadType"), new object[] { fullTypeName }));
                    }
                }
                if (typeArgumentCount != 0)
                {
                    GCHandle handle2 = GCHandle.Alloc(ptrArray2, GCHandleType.Pinned);
                    typeArgumentCount = typeNameInfo.GetTypeArguments(typeArgumentCount, handle2.AddrOfPinnedObject());
                    handle2.Free();
                    Type[] typeArguments = new Type[typeArgumentCount];
                    for (int j = 0; j < typeArgumentCount; j++)
                    {
                        typeArguments[j] = LoadTypeWithPartialName((ITypeName)Marshal.GetObjectForIUnknown(ptrArray2[j]), null, fullTypeName);
                    }
                    return nestedType.MakeGenericType(typeArguments);
                }
                type2 = nestedType;
            }
            finally
            {
                for (int k = 0; k < ptrArray.Length; k++)
                {
                    IntPtr ptr1 = ptrArray[k];
                    Marshal.FreeBSTR(ptrArray[k]);
                }
                for (int m = 0; m < ptrArray2.Length; m++)
                {
                    IntPtr ptr2 = ptrArray2[m];
                    Marshal.Release(ptrArray2[m]);
                }
            }
            return type2;

        }

        // Nested Types
        [ComImport, Guid("B81FF171-20F3-11D2-8DCC-00A0C9B00522"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), TypeLibType((short)0x100)]
        internal interface ITypeName
        {
            uint GetNameCount();
            uint GetNames([In] uint count, IntPtr rgbszNamesArray);
            uint GetTypeArgumentCount();
            uint GetTypeArguments([In] uint count, IntPtr rgpArgumentsArray);
            uint GetModifierLength();
            uint GetModifiers([In] uint count, out uint rgModifiers);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetAssemblyName();
        }

        [ComImport, TypeLibType((short)0x100), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B81FF171-20F3-11D2-8DCC-00A0C9B00521")]
        internal interface ITypeNameFactory
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            TypeName.ITypeName ParseTypeName([In, MarshalAs(UnmanagedType.LPWStr)] string szName, out int pError);
        }
    }


}
