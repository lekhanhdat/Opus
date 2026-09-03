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
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// 为了避免很多Link，所以重新整理一个给AveLogger来加密一些信息
    /// </summary>
    //internal class InternalCrypto
    //{
    //    private static byte[] key = { 15, 218, 43, 167, 98, 156, 234, 134 };
    //    private static byte[] iv = { 145, 138, 67, 7, 198, 56, 224, 113 };

    //    public static string EncryptMessage(string message)
    //    {
    //        string result = string.Empty;

    //        if (!string.IsNullOrEmpty(message))
    //        {
    //            try
    //            {
    //                using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
    //                {
    //                    using (MemoryStream stream = new MemoryStream())
    //                    {
    //                        using (CryptoStream cryptoStream = new CryptoStream(stream, aesProvider.CreateEncryptor(key, iv), CryptoStreamMode.Write))
    //                        {
    //                            byte[] buffer = Encoding.UTF8.GetBytes(message);
    //                            cryptoStream.Write(buffer, 0, buffer.Length);
    //                            cryptoStream.Close();
    //                            result = Convert.ToBase64String(stream.ToArray());
    //                        }
    //                    }
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                result = string.Format("[Details]:{0}\r\n[Exception]:{1}", message, ex.ToString());
    //            }
    //        }

    //        return result;
    //    }

    //    public static string DecryptMessage(string message)
    //    {
    //        string result = string.Empty;

    //        if (!string.IsNullOrEmpty(message))
    //        {
    //            try
    //            {
    //                using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider() )
    //                {
    //                    using (MemoryStream stream = new MemoryStream())
    //                    {
    //                        using (CryptoStream cryptoStream = new CryptoStream(stream, aesProvider.CreateDecryptor(key, iv), CryptoStreamMode.Write))
    //                        {
    //                            byte[] buffer = Convert.FromBase64String(message);//Encoding.UTF8.GetBytes(message);
    //                            cryptoStream.Write(buffer, 0, buffer.Length);
    //                            cryptoStream.Close();
    //                            result = Encoding.UTF8.GetString(stream.ToArray());
    //                        }
    //                    }
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                result = string.Format("[Details]:{0}\r\n[Exception]:{1}", message, ex.ToString());
    //            }
    //        }

    //        return result;
    //    }
    //}

    public class FormatResolver : IFormatResolver
    {
        public int Order { get { return 3; } }
        public void Invoke(StringBuilder builder, int level, object key, object value)
        {
            string strValue = Convert.ToString(value);
            if ((key != null && ResolverFactory.IsSensitivePropertyKey(key.ToString())) //filter key
    || (!string.IsNullOrWhiteSpace(strValue) && ResolverFactory.IsSensitivePropertyValue(strValue)) //filter value
    )
            {
                //SAAS-40833
                //strValue = string.Format("Encrypted:[{0}]",InternalCrypto.EncryptMessage(strValue));
                strValue = "***";
            }
            builder.AppendLineByLevel(level, string.Format("<{0}:{1}>", key, strValue));
        }

        public bool IsTypeQualified(object value)
        {
            var type = value==null?null:value.GetType();
            return type==null||ResolverFactory.BasicTypes.Contains(type) || type.IsEnum;
        }
    }

    public class DictionaryFormatResolver : IFormatResolver
    {
        public int Order { get { return 2; } }
        public void Invoke(StringBuilder builder, int level, object key, object value)
        {
            var dictionary = value as IDictionary;
            if (dictionary != null)
            {
                builder.AppendLineByLevel(level, string.Format("<{0}>", key));
                foreach (DictionaryEntry entry in dictionary)
                {
                    ResolverFactory.GetResolver(entry.Value).Invoke(builder, level + 1, entry.Key, entry.Value);
                }
                builder.AppendLineByLevel(level, string.Format("</{0}>", key));
            }
        }

        public bool IsTypeQualified(object value)
        {
            return value.GetType().GetInterfaces().FirstOrDefault(t => t.Name.StartsWith("IDictionary")) != null;
        }
    }

    public class EnumerableFormatResolver : IFormatResolver
    {
        public int Order { get { return 1; } }
        public void Invoke(StringBuilder builder, int level, object key, object value)
        {
            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                builder.AppendLineByLevel(level, string.Format("<{0}>", key));
                foreach (object childValue in enumerable)
                {
                    ResolverFactory.GetResolver(childValue).Invoke(builder, level + 1, "", childValue);
                }
                builder.AppendLineByLevel(level, string.Format("</{0}>", key));
            }
        }

        public bool IsTypeQualified(object value)
        {
            return value.GetType().GetInterfaces().FirstOrDefault(t => t.Name.StartsWith("IEnumerable"))!=null;
        }
    }

    public class GenericFormatResolver : IFormatResolver
    {
        public int Order { get { return 0; }  }

        public void Invoke(StringBuilder builder, int level, object key, object value)
        {
            builder.AppendLineByLevel(level, string.Format("<{0}:{1}>", key, value.GetType().Name));
            var type = value.GetType();
            System.Reflection.PropertyInfo[] propertyInfos = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (System.Reflection.PropertyInfo propertyInfo in propertyInfos)
            {
                string childName = propertyInfo.Name;
                object obj = propertyInfo.GetValue(value);
                ResolverFactory.GetResolver(obj).Invoke(builder, level + 1, childName, obj);
            }
        }

        public bool IsTypeQualified(object value)
        {
            var type = value.GetType();
            return type.IsGenericType;
        }
    }


}
