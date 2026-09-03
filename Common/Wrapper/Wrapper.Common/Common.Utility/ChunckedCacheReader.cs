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



using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class ChunckedCacheReader : IChunckedCacheReader
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(ChunckedCacheReader));
        private Stream mStream;
        private int mCurrentLength;
        private const int MEMORY_LIMIT = 500 * 1024 * 1024;

        public int Length
        {
            get
            {
                return mCurrentLength;
            }
        }

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
            mStream.UnsafeRead(lengthBytes, 0, 4);
            //mStream.Read(lengthBytes, 0, 4);
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
                mStream.UnsafeRead(lengthBytes, 0, 4);
                //mStream.Read(lengthBytes, 0, 4);
                mCurrentLength = BitConverter.ToInt32(lengthBytes, 0);

                if (mCurrentLength < 0)
                {
                    try
                    {
                        mLog.Info("Read header error,write it to the job folder:{0}", WrapperConfiguration.JobDir);
                        mStream.Position = 0;
                        FileStream fs = new FileStream(Path.Combine(WrapperConfiguration.JobDir, "BrokenItem.dat"), FileMode.OpenOrCreate);
                        mStream.CopyTo(fs);
                        fs.Close();
                    }
                    catch(Exception ex)
                    {
                        mLog.Warn("Write broken item failed.Error:{0}", ex);
                    }
                    throw new ArgumentOutOfRangeException(string.Format("The length:{0} is not valid.", mCurrentLength));
                }

            }
            return header;
        }

        public byte[] ReadBytes()
        {                        
            byte[] content = new byte[mCurrentLength];
            if (mCurrentLength > 0)
            {
                //mStream.Read(content, 0, mCurrentLength);
                mStream.UnsafeRead(content, 0, mCurrentLength);
                mCurrentLength = 0;
            }
            return content;
        }

        /// <summary>
        /// 需要严格按照Length读取
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public int ReadBytes(byte[] buffer, int offset, int len)
        {
            if (mCurrentLength > 0)
            {
                return mStream.UnsafeRead(buffer, offset, len);
            }
            return 0;
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

    public static class StreamExtension
    {
        public static int SafeRead(this Stream stream, byte[] buffer, int offset, int count)
        {
            int readLen = 0;

            while (count > 0)
            {
                var currentReadLen = stream.Read(buffer, offset, count);

                if (currentReadLen <= 0)
                {
                    break;
                }

                readLen += currentReadLen;
                offset += currentReadLen;
                count -= currentReadLen;
            }

            return readLen;
        }

        public static int UnsafeRead(this Stream stream, byte[] buffer, int offset, int count)
        {
            var read = stream.SafeRead(buffer, offset, count);

            if (read != count)
            {
                throw new EndOfStreamException(string.Format("The buffer is not enough, required is {0}, the current is {1}", count, read));
            }

            return read;
        }
    }
}
