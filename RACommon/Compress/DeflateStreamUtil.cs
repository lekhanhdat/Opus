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
using System.IO.Compression;

namespace AvePoint.RA.Common.Compress
{
    /// <summary>
    /// using DeflateStream to compress or decompress data
    /// </summary>
    public class DeflateStreamUtil
    {
        /// <summary>
        /// Decompress the byte array to a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Decompress(byte[] input)
        {
            using (MemoryStream inputStream = new MemoryStream(input))
            {
                using (var decompressStream =
                  new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    using (StreamReader reader =
                      new StreamReader(decompressStream, System.Text.Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// Compress the string to byte array
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] Compress(String str)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (var compressStream =
                  new DeflateStream(output, CompressionMode.Compress))
                {
                    using (StreamWriter writer =
                      new StreamWriter(compressStream, System.Text.Encoding.UTF8))
                    {
                        writer.Write(str);
                    }
                }

                return output.ToArray();
            }
        }
    }
}
