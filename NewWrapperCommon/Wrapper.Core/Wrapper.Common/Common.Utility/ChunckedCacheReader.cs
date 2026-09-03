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
    public class ChunckedCacheReader : IChunckedCacheReader
    {
        private Stream mStream;
        private int mCurrentLength;
        private const int MEMORY_LIMIT = 500 * 1024 * 1024;

        public ChunckedCacheReader(Stream stream)
        {
            mStream = stream;
            mCurrentLength = 0;
        }

        public string ReadMetaString()
        {
            if (mCurrentLength > MEMORY_LIMIT)
            {
                throw new InsufficientMemoryException("Memory request reached limit size, enlarge the limit or check for exceptions.");
            }
            byte[] lengthBytes = new byte[4];
            mStream.Read(lengthBytes, 0, 4);
            mCurrentLength = BitConverter.ToInt32(lengthBytes, 0);
            return ReadString();
        }

        public byte[] ReadBytes(int len)
        {
            byte[] content = new byte[len];
            if (len > 0)
            {
                if (mStream.Read(content, 0, len) > 0)
                {
                    return content;
                }
                else
                {
                    return null;
                }
            }
            return new byte[0];
        }

        public int ReadHeader()
        {
            int header = mStream.ReadByte();
            if (header != -1)
            {
                byte[] lengthBytes = new byte[4];
                mStream.Read(lengthBytes, 0, 4);
                mCurrentLength = BitConverter.ToInt32(lengthBytes, 0);
            }
            return header;
        }

        public byte[] ReadBytes()
        {                        
            byte[] content = new byte[mCurrentLength];
            if (mCurrentLength > 0)
            {
                mStream.Read(content, 0, mCurrentLength);
                mCurrentLength = 0;
            }
            return content;
        }

        public string ReadString()
        {
            if (mCurrentLength > MEMORY_LIMIT)
            {
                throw new InsufficientMemoryException("Memory request reached limit size, enlarge the limit or check for exceptions.");
            }
            if (mCurrentLength == 0)
            {
                return string.Empty;
            }
            byte[] buffer = this.ReadBytes();            
            return Encoding.UTF8.GetString(buffer);
        }        
    }
}
