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
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public class AveIOHelper
    {
        private const int DEFAULT_BUFFER_SIZE = 65520; //the same size as default AveDataBlock

        public static int SafeRead(Stream stream, byte[] buffer, int offset, int count)
        {
            int totalLen = 0;
            int len = 0;
            int needReadLen = count;
            int tryTimes = 0;
            do
            {
                len = stream.Read(buffer, offset, count);
                totalLen += len;
                offset += len;
                count -= len;
                tryTimes = len == 0 ? (tryTimes + 1) : 0;
            } while (totalLen < needReadLen && tryTimes < 10);

            return totalLen;
        }

        public static void Copy(Stream src, Stream dest, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int len = 0;
            while ((len = src.Read(buffer, 0, bufferSize)) != 0)
            {
                dest.Write(buffer, 0, len);
            }
        }

        public static void Copy(Stream src, Stream dest)
        {
            Copy(src, dest, DEFAULT_BUFFER_SIZE);
        }

        public static void CloseStreamQuietly(Stream stream)
        {
            if (stream != null)
            {
                stream.Close();
            }
        }
    }
}
