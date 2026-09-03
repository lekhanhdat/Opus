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
using System.Configuration.Assemblies;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Policy;

namespace LS.BinarySerialization
{
    public static class ExtentionClass
    {
        public static void Init(this AssemblyName assmName, string name,
            byte[] publicKey, byte[] publicKeyToken,
            Version version, CultureInfo cultureInfo,
            AssemblyHashAlgorithm hashAlgorithm,
            AssemblyVersionCompatibility versionCompatibility,
            string codeBase, AssemblyNameFlags flags, StrongNameKeyPair keyPair)
        {
            LSInvoker.CallMethod(assmName, "Init", 
                new Type[] {typeof(string),typeof(byte[]),typeof(byte[]),
                typeof(Version),typeof(CultureInfo),
                typeof(AssemblyHashAlgorithm),
                typeof(AssemblyVersionCompatibility),
                typeof(string),typeof(AssemblyNameFlags),typeof(StrongNameKeyPair)},
                new object[] { name, publicKey, publicKeyToken,
                version,cultureInfo,
                hashAlgorithm,
                versionCompatibility,
                codeBase,flags,keyPair});

        }

        public static string nGetSimpleName(this Assembly assembly)
        {
            return (string)LSInvoker.CallMethod(assembly, "nGetSimpleName");
        }

        public static byte[] nGetPublicKey(this Assembly assembly)
        {
            return (byte[])LSInvoker.CallMethod(assembly, "nGetPublicKey");
        }

        public static CultureInfo GetLocale(this Assembly assembly)
        {
            return (CultureInfo)LSInvoker.CallMethod(assembly, "GetLocale");
        }

        public static void RegisterString(this ObjectManager objectManager, string obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member)
        {
            LSInvoker.CallMethod(objectManager, "RegisterString",
                new Type[] { typeof(string),typeof(long),info.ParentType,typeof(long),typeof(MemberInfo)},
                new object[] { obj, objectID, info.Parent, idOfContainingObj, member });
        }

        public static void RegisterObject(this ObjectManager objectManager,object obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member, int[] arrayIndex)
        {
            LSInvoker.CallMethod(objectManager, "RegisterObject",
                new Type[] { typeof(object), typeof(long), info.ParentType, typeof(long), typeof(MemberInfo),typeof(int[]) },
                new object[] { obj, objectID, info.Parent, idOfContainingObj, member, arrayIndex });
        }
    }

    internal sealed class LSEnvironment
    {
        internal static string GetResourceString(string key)
        {
            Type typeofEnvironment = Type.GetType("System.Environment");
            return (string)LSInvoker.CallStaticMethod(typeofEnvironment, "GetResourceString", new Type[] { typeof(string) }, new object[] { key });
        }
    }

    internal sealed class LSAssembly
    {
        internal static Assembly LoadWithPartialNameInternal(string partialName, Evidence securityEvidence, ref StackCrawlMark stackMark)
        {
            Assembly assm = null; 
            object StackCrawlMarkOrig = Enum.ToObject(Type.GetType(""), (int)stackMark);

            object[] invokeArgs = new object[] { partialName, securityEvidence, stackMark};

            MethodInfo invokeMethod = typeof(Assembly).GetMethod("LoadWithPartialNameInternal", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (invokeMethod != null)
                assm = (Assembly)invokeMethod.Invoke(null, invokeArgs);
            stackMark = (StackCrawlMark)invokeArgs[2];
            return assm;
        }
    }
}
