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

namespace AvePoint.Wrapper.Common
{
    public class ChunckedCacheWriter : IChunckedCacheWriter
    {
        private Stream mStream;

        public ChunckedCacheWriter(Stream stream)
        {
            mStream = stream;
        }

        public void WriteHeader(byte header)
        {
            mStream.WriteByte(header);            
        }

        public int WriteBytes(byte[] buffer, int offset, int count)
        {
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
            }
            mStream.Write(BitConverter.GetBytes(count), 0, 4);
            mStream.Write(buffer, offset, count);
            return count;
        }

        public int WriteBytesWithoutLength(byte[] buffer, int offset, int count)
        {
            int leftCount = buffer.Length - offset;
            int actualCount = leftCount > count ? count : leftCount;
            mStream.Write(buffer, offset, count);
            return actualCount;
        }

        public int WriteString(string stringContent)
        {
            if (string.IsNullOrEmpty(stringContent))
            {
                mStream.Write(BitConverter.GetBytes(0), 0, 4);
                return 0;
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(stringContent);
                return this.WriteBytes(bytes, 0, bytes.Length);
            }
        }
    }
}
