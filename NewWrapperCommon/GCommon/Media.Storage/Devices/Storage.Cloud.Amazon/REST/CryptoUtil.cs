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
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AvePoint.Media.Storage.Cloud.Amazon
{
    public class CryptoUtil
    {
        public static byte[] ComputeHash(string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            return ComputeHash(buffer, 0, buffer.Length);
        }

        public static byte[] ComputeHash(byte[] data, int offset, int length)
        {
            SHA256 signature = SHA256.Create();
            byte[] bytes = signature.ComputeHash(data, offset, length);
            return bytes;
        }

        public static byte[] ComputeHash(byte[] awsSecretAccessKeyByte, string canonicalString)
        {
            HMACSHA256 signature = new HMACSHA256(awsSecretAccessKeyByte);
            byte[] bytes = signature.ComputeHash(Encoding.UTF8.GetBytes(canonicalString));
            return bytes;
        }

        public static byte[] ComputeHash(byte[] awsSecretAccessKeyByte, byte[] canonicalStringByte)
        {
            HMACSHA256 signature = new HMACSHA256(awsSecretAccessKeyByte);
            byte[] bytes = signature.ComputeHash(canonicalStringByte);
            return bytes;
        }

        public static string ToHex(byte[] data, bool lowercase)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString(lowercase ? "x2" : "X2", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }
    }
}
