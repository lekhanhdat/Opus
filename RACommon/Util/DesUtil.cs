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
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.ComponentModel;
using AvePoint.RA.I18N.Core;
using AvePoint.GCommon.Utility.Cryptography.Encryption;

namespace AvePoint.RA.Common.Util
{
    public static class DesUtil
    {

        //public static string Decode(byte[] encryptedData, byte[] key, byte[] iv)
        //{
        //    using (MemoryStream stream = new MemoryStream(encryptedData))
        //    {
        //        return Decode(stream, key, iv);
        //    }
        //}

        //public static string Decode(string encryptedData, byte[] key, byte[] iv)
        //{
        //    using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(encryptedData)))
        //    {
        //        return Decode(stream, key, iv);
        //    }
        //}

        //public static string Decode(Stream stream, byte[] key, byte[] iv)
        //{
        //    DESEncryption encryption = new DESEncryption(key);
        //    encryption.IV = iv;
        //    using (CryptoStream cryptoStream = encryption.CreateDecryptStream(stream, CryptoStreamMode.Read))
        //    {
        //        using (StreamReader reader = new StreamReader(cryptoStream))
        //        {
        //            return reader.ReadToEnd();
        //        }
        //    }
        //}

        //public static string Encode(string data, byte[] key, byte[] iv)
        //{
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        DESEncryption encryption = new DESEncryption(key);
        //        encryption.IV = iv;
        //        using (CryptoStream cryptoStream = encryption.CreateEncryptStream(stream, CryptoStreamMode.Write))
        //        {
        //            using (StreamWriter writer = new StreamWriter(cryptoStream))
        //            {
        //                writer.Write(data);
        //                writer.Flush();
        //                cryptoStream.FlushFinalBlock();
        //                return Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 扩展方法，获得枚举的Description，根据枚举description attribute中的key，获取对应词条
        /// </summary>
        /// <param name="value">枚举值</param>
        /// <returns>枚举的Description</returns>
        public static string ToDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name == null)
            {
                return null;
            }

            FieldInfo field = type.GetField(name);
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (attribute == null)
            {
                return value.ToString();
            }
            return I18NEntity.GetString(attribute.Description);
        }

    }
}
