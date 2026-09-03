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
using System.IO;
using System.Text;
using AvePoint.GCommon.Utility.FilteringBox;

namespace AvePoint.GCommon.Utility
{
    public class ZlibUtil
    {
        public static byte[] ZipFile(string path, int level = 1)
        {
            var content = File.ReadAllBytes(path);
            return ZipBytes(content, level);
        }

        public static byte[] ZipString(string content, int level = 1)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            return ZipBytes(bytes, level);
        }

        public static byte[] ZipBytes(byte[] content, int level=1)
        {
            var zlibStream = new ZlibCompressionFilteringBox(level);
            return DoZip(zlibStream, content);
        }

        public static string UnZipString(byte[] content)
        {
            var result = UnZipBytes(content);
            return Encoding.UTF8.GetString(result);
        }

        public static byte[] UnZipBytes(byte[] content)
        {
            var zlibStream = new ZlibCompressionFilteringBox();
            return DoZip(zlibStream, content);
        }


        private static byte[] DoZip(ZlibCompressionFilteringBox zlibStream, byte[] content)
        {
            if (content == null || content.Length == 0)
            {
                return null;
            }
            zlibStream.InputBegin();
            zlibStream.Input(content, 0, content.Length);

            var results = new List<byte[]>();
            int length = Math.Min(1 << 16, Math.Max(content.Length, 16));
            int totalLength = 0;
            while (true)
            {
                var result = new byte[length];
                int outLen = zlibStream.ReceiveOutput(result, 0, result.Length);
                if (outLen == -1) break;
                if (outLen == 0)
                {//解压的时候必须先使用zlibConst.Z_NO_FLUSH取完，才能使用zlibConst.Z_FINISH
                    zlibStream.InputEnd();
                }
                else if (outLen == result.Length)
                {
                    results.Add(result);
                }
                else
                {
                    var realResult = new byte[outLen];
                    Array.Copy(result, realResult, realResult.Length);
                    results.Add(realResult);
                }
                totalLength += outLen;
            }
            if (results.Count == 0)
            {
                return null;
            }
            if (results.Count == 1)
            {
                return results[0];
            }
            var ouput = new byte[totalLength];
            int index = 0;
            foreach (var result in results)
            {
                Array.Copy(result, 0, ouput, index, result.Length);
                index += result.Length;
            }
            return ouput;
        }
    }
}
