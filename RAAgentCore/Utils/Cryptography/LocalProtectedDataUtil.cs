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
using AvePoint.Hybrid.Utility.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hybrid.Utility.Cryptography
{
    public class LocalProtectedDataUtil
    {
        public static string Protect(string userData)
        {
            var bytes = Encoding.UTF8.GetBytes(userData);
            return AveProtectedData.ProtectWithBase64(bytes);
        }

        public static string UnProtect(string base64Str)
        {
            var bytes = AveProtectedData.UnProtectWithBase64(base64Str);
            return Encoding.UTF8.GetString(bytes);
        }

        public static void ProtectToFile(string userData, string filePath)
        {
            var bytes = Encoding.UTF8.GetBytes(userData);
            byte[] result = AveProtectedData.Protect(bytes);
            using (var fs = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                fs.Write(result, 0, result.Length);
            }
        }

        public static string GetFromFile(string filePath)
        {
            if(!File.Exists(filePath))
            {
                return null;
            }
            byte[] result = null;
            using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                result = new byte[fs.Length];
                var read = fs.Read(result, 0, result.Length);

                if (read != result.Length)
                {
                    throw new Exception("not read completely. please modify this method to ensure all content can be read.");
                }
            }
            byte[] bytes = AveProtectedData.UnProtect(result);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
