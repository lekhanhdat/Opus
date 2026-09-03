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




namespace AvePoint.Media.Core.IO
{
    using Microsoft.InformationProtection.Exceptions;
    #region using directives
    using System.IO;
    #endregion

    public class DataReaderUtility
    {
        public static uint ReadInt32(byte[] data, int offset)
        {
            return (uint)(((data[offset] | (data[offset + 1] << 8)) | (data[offset + 2] << 0x10)) | (data[offset + 3] << 0x18));
        }

        //public static uint ReadInt32(Stream stream)
        //{
        //    byte[] data = new byte[4];
        //    stream.Read(data, 0, 4);
        //    return ReadInt32(data, 0);
        //}

        public static int ReadBigInt32(Stream stream)
        {
            byte[] data = new byte[4];
            int bRead = 0;
            while (bRead < 4)
            {
                int rd = stream.Read(data, bRead, 4 - bRead);
                if (rd == -1)
                {
                    throw new FileIOException("file is unusually small");
                }
                bRead += rd;
            }
            return ToBigInt(data, 0);
        }

        public static ushort ReadInt16(byte[] data, int offset)
        {
            return (ushort)(data[offset] | (data[offset + 1] << 8));
        }

        //public static ushort ReadInt16(Stream stream)
        //{
        //    byte[] data = new byte[2];
        //    stream.Read(data, 0, 2);
        //    return ReadInt16(data, 0);
        //}

        public static int ToBigBytes(int a, byte[] buf, int offset)
        {
            buf[offset + 3] = (byte)a;
            a >>= 8;
            buf[offset + 2] = (byte)a;
            a >>= 8;
            buf[offset + 1] = (byte)a;
            a >>= 8;
            buf[offset + 0] = (byte)a;
            a >>= 8;
            return 4;
        }

        public static int ToBigBytes(uint a, byte[] buf, int offset)
        {
            buf[offset + 3] = (byte)a;
            a >>= 8;
            buf[offset + 2] = (byte)a;
            a >>= 8;
            buf[offset + 1] = (byte)a;
            a >>= 8;
            buf[offset + 0] = (byte)a;
            a >>= 8;
            return 4;
        }

        public static int ToBigBytes(short a, byte[] buf, int offset)
        {
            buf[offset + 1] = (byte)a;
            a >>= 8;
            buf[offset + 0] = (byte)a;
            a >>= 8;
            return 2;
        }

        public static int ToBigBytes(ushort a, byte[] buf, int offset)
        {
            buf[offset + 1] = (byte)a;
            a >>= 8;
            buf[offset + 0] = (byte)a;
            a >>= 8;
            return 2;
        }

        public static int ToBigInt(byte[] buf, int offset)
        {
            int i;
            int a = 0;
            for (i = 0; i < 4; i++)
            {
                a <<= 8;
                a += buf[offset++];
            }
            return a;
        }

        public static uint ToBigUint(byte[] buf, int offset)
        {
            int i;
            uint a = 0;
            for (i = 0; i < 4; i++)
            {
                a <<= 8;
                a += buf[offset++];
            }
            return a;
        }

        public static short ToBigShort(byte[] buf, int offset)
        {
            int i;
            short a = 0;
            for (i = 0; i < 2; i++)
            {
                a <<= 8;
                a += buf[offset++];
            }
            return a;
        }

        public static ushort ToBigUShort(byte[] buf, int offset)
        {
            int i;
            ushort a = 0;
            for (i = 0; i < 2; i++)
            {
                a <<= 8;
                a += buf[offset++];
            }
            return a;
        }

        public static void FillBuffer(Stream stream, byte[] buff, int totalBytes)
        {
            int offset = 0;
            int readLen = 0;
            do
            {
                readLen = stream.Read(buff, offset, totalBytes - offset);

                offset += readLen;
            }
            while (offset < totalBytes);
        }

        public static int AllignPageSize(Stream stream, int pageSize)
        {
            long oldPos = stream.Position;
            long newPos = ((oldPos + pageSize) / pageSize) * pageSize;
            stream.SetLength(newPos);
            stream.Position = newPos;
            return (int)(newPos - oldPos);
        }
    }
}
