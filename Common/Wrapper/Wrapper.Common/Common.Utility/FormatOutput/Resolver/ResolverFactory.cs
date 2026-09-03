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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public static class ResolverFactory
    {
        private static List<string> SensitivePropertyList = new List<string>();

        static ResolverFactory()
        {
            TypeResolverList = new List<IFormatResolver>();
            TypeResolverList.AddRange(
               new List<IFormatResolver>()
               {
                    new AveModernThemeInfoFormatResolver(),
                    new AveTreeNodeDtoFormatResolver(),
                    new AveBPOSAccountInfoFormatResolver(),
                    new FormatResolver(),
                    new DictionaryFormatResolver(),
                    new EnumerableFormatResolver(),
                    new GenericFormatResolver()
               });
            SensitivePropertyList.AddRange(
                new List<string>
                {
                    "ConnectionString",
                    "Password",
                    "Key",//SAAS-40820
                    "secret",//SAAS-40833
                    "DynamicKey", //SAAS-41834
                });
        }
        //public List<IFormatResolver>
        public static List<Type> BasicTypes
        {
            get
            {
                return new List<Type>
                {
                    typeof(string),
                    typeof(bool),
                    typeof(byte),
                    typeof(char),
                    typeof(short),
                    typeof(int),
                    typeof(long),
                    typeof(float),
                    typeof(double),
                    typeof(decimal),
                    typeof(Uri),
                    typeof(Guid),
                    typeof(DateTime),
                    typeof(byte[]),
                    typeof(sbyte),
                    typeof(ushort),
                    typeof(uint),
                    typeof(ulong)
                };
            }
        }

        public static void AddSensitiveProperty(params string[] values)
        {
            foreach (var name in values)
            {
                lock (SensitivePropertyList)
                {
                    if (!SensitivePropertyList.Contains(name))
                    {
                        SensitivePropertyList.Add(name);
                    }
                }
            }
        }

        public static bool IsSensitivePropertyKey(string name)
        {
            lock (SensitivePropertyList)
            {
                return SensitivePropertyList.Contains(name, StringComparer.OrdinalIgnoreCase);
            }
        }

        public static bool IsSensitivePropertyValue(string propertyValue)
        {
            lock (SensitivePropertyList)
            {
                return !string.IsNullOrEmpty(propertyValue) && SensitivePropertyList.Any(it => propertyValue.ToLower().Contains(it.ToLower()));
            }
        }



        public static List<IFormatResolver> TypeResolverList
        {
            get;set;
        }

        public static IFormatResolver GetResolver(object value)
        {
            if (value==null)
            {
                return new FormatResolver();
            }
            var type = value.GetType();
            foreach (var resolverInstance in TypeResolverList)
            {
                if (resolverInstance.IsTypeQualified(value))
                {
                    return resolverInstance;
                }
            }
            return TypeResolverList.Last();
        }
    }
}
