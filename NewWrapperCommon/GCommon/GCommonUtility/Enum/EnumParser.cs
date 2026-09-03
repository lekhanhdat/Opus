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
using System.Reflection;
using System.ComponentModel;
using System.Linq;


namespace AvePoint.GCommon.Utility
{

    /// <summary>
    /// 枚举帮助类
    /// </summary>
    public sealed class Enumer
    {
        private Enumer() { }
        /// <summary>
        /// 转换string值为特定Enum类型
        /// </summary>
        public static T Parse<T>(string val) where T : struct
        {
            return parse<T>(val);
        }
        /// <summary>
        /// 转换int值为特定Enum类型
        /// </summary>
        public static T Parse<T>(int val) where T : struct
        {
            return parse<T>(val);
        }

        /// <summary>
        /// 将相应的值转换为Enum T,
        /// 转换失败会取Enum默认值
        /// </summary>
        /// <typeparam name="T">Enum 类型</typeparam>
        /// <param name="val">值</param>
        private static T parse<T>(object val) where T : struct
        {
            Type type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("Type '" + type.Name + "' is not an Enum.");
            }

            if (Enum.IsDefined(typeof(T), val))
            {
                return (T)(IConvertible)val;
            }
            return default(T);
        }

        public static IEnumerable<string> GetDescriptions<T>()
        {
            Type type = typeof(T);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo field in fields)
            {
                if (field.IsLiteral)
                {
                    yield return GetDescription(typeof(DescriptionAttribute), field);
                }
            }
        }

        public static string GetDescription(Type type, FieldInfo field)
        {
            object attr = field
                .GetCustomAttributes(type, false)
                .FirstOrDefault();
            string text = field.Name;
            if (attr != null)
            {
                text = ((DescriptionAttribute)attr).Description ?? field.Name;
            }

            return text;
        }

        /// <summary>
        /// 取得Enum item 的 Description name.
        /// 如果不存在描述则取EnumName.
        /// </summary>
        public static string GetDescription(Enum item)
        {
            string val = "";
            if (item != null)
            {
                val = Enum.GetName(item.GetType(), item); //取得Enum Name， 性能高于toString().
                FieldInfo field = item.GetType().GetField(val);
                object[] os = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (os != null && os.Length > 0) //如果存在自定义描述
                {
                    DescriptionAttribute attribute = os[0] as DescriptionAttribute;

                    if (attribute != null)
                    {
                        return attribute.Description;
                    }
                }
            }
            return val;
        }

        public static string GetDescription<T>(int val) where T : struct
        {
            T t = Enumer.parse<T>(val);
            return GetDescription(t as Enum);
        }

        /// <summary>
        /// 将 Enum.A | Enum.B ... 类型的枚举值 拆分为 List
        /// </summary>
        /// <remarks>只支持int类型的转换</remarks>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="val">枚举和</param>
        /// <returns>拆分后的枚举集合</returns>
        public static List<T> Split<T>(int val)
        {
            Type type = typeof(T);
            //T[] array = Enum.GetValues(type) as T[]; //can not be use in silverlight.
            T[] enumers = GetEnumValues<T>()
                .OrderBy(e => e.GetHashCode())
                .ToArray();
            List<T> child = new List<T>();
            for (int i = enumers.Count() - 1; i > -1; i--)
            {
                T t = enumers[i];
                int step = (int)(IConvertible)t;
                if (step == 0) break;
                if (val < step) continue;
                child.Add(t);
                val -= step;
    }
            return child;
}

        public static IEnumerable<T> GetEnumValues<T>()
        {
            Type type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("Type '" + type.Name + "' is not an Enum.");
            }
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral)
                .Select(field => (T)field.GetValue(type));
        }
    }
}
