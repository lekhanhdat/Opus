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
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.Media.Storage.Cloud.Common
{
    class CommonUtility
    {
        public static string GetMD5Hash(string input)
        {
            IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(AvePoint.GCommon.Utility.Cryptography.HashAlgorithm.MD5);
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();

            // 遍历字节数组，将每一个字节转换为十六进制字符串后，追加到StringBuilder实例的结尾
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // 返回一个十六进制字符串
            return sBuilder.ToString();

        }

        public static string Base64Encoded128BitMD5Digest(string content)
        {
            IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(AvePoint.GCommon.Utility.Cryptography.HashAlgorithm.MD5);
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
            return Convert.ToBase64String(data);
        }

        public static string Base64(string strVal)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(strVal));
        }
    }
}
