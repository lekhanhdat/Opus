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
using System.Diagnostics.CodeAnalysis;

namespace LS.BinarySerialization
{
    public class LSResourceReader
    {
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint type name")] 
        public static string[] TypesSafeForDeserialization
        {
            get
            {
               return (string[])LSInvoker.GetStaticProperty(Type.GetType("System.Resources.ResourceReader"), "TypesSafeForDeserialization");
            }
        }

    }

    public class LSResourceManager
    {
        private static Type typeofResourceManager=null;
        static LSResourceManager()
        {
            if (typeofResourceManager == null)
                typeofResourceManager = Type.GetType("System.Resources.ResourceManager");
        }
        internal static bool CompareNames(string asmTypeName1, string typeName2, AssemblyName asmName2)
        {

            return (bool)LSInvoker.CallStaticMethod(typeofResourceManager,"",new Type[]{typeof(string),typeof(string),typeof(AssemblyName)},
                new object[] { asmTypeName1, typeName2, asmName2 });
        }
    }
}
