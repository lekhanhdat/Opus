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


using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class TypeInfo
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(TypeInfo));
        public string Name 
        { 
            get;
            private set; 
        }

        public string Namespace 
        { 
            get; 
            private set; 
        }

        public AssemblyName Assembly 
        { 
            get; 
            private set; 
        }        

        public static TypeInfo Parse(string typeFullName)
        {
            if (TypeNameMapping.TryGetValue(typeFullName, out string tempValue))
            {
                mLog.Warn($"WebPart name is not entire. To change from {typeFullName} to {tempValue}");
                typeFullName = tempValue;
            }
            int firstCommaIndex = typeFullName.IndexOf(',');
            if (firstCommaIndex == -1)
            {
                throw new ArgumentException(string.Format("typeFullName: {0} is invalid", typeFullName));
            }
            TypeInfo typeInfo = new TypeInfo();
            typeInfo.Name = typeFullName.Substring(0, firstCommaIndex);
            typeInfo.Namespace = typeInfo.Name.Substring(0, typeInfo.Name.LastIndexOf('.'));
            typeInfo.Assembly = new AssemblyName(typeFullName.Substring(firstCommaIndex+1).Trim());
            return typeInfo;
        }
        public static Dictionary<string, string> TypeNameMapping = new Dictionary<string, string>()
            {
                { "Microsoft.SharePoint.WebPartPages.XsltListViewWebPart", "Microsoft.SharePoint.WebPartPages.XsltListViewWebPart, Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" }
            };
    }

    public class TypeInfoIgnoreVersionEqualityComparer : IEqualityComparer<TypeInfo>
    {
        public bool Equals(TypeInfo x, TypeInfo y)
        {
            return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase) 
                && x.Assembly.Name.Equals(y.Assembly.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(TypeInfo obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
