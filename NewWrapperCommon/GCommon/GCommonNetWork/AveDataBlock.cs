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
using AvePoint.GCommon;

namespace AvePoint.GCommon.Network
{
    public enum AveDataBlockType : byte
    {
        UNKNOW_TYPE = 0,

        HEADER_TYPE = 1,
        DATA_TYPE = 2,
        CONTENTDATA_TYPE = 3,
        TAIL_TYPE = 4,
        COMPLEX_TYPE = 5,
        FLUSH_TYPE = 6,
        SOLUTION_TYPE,
        ALIVE_TYPE = 7,
        SYNC_TYPE = 8,

        UNUSED_TYPE = 50,
        MESSAGE_TYPE = 80,

        FILE_OPEN_TYPE = 85,
        FILE_DATA_TYPE = 87,
        FILE_REOPEN_TYPE = 86,
        FILE_CLOSE_TYPE = 84,

        FILE_RECEIVE_OPEN_TYPE = 88,
        FILE_RECEIVE_REOPEN_TYPE = 89,

        SEND_OPEN_CONNECTION_TYPE = 104,
        SEND_REOPEN_CONNECTION_TYPE = 105,

        RECV_OPEN_CONNECTION_TYPE = 101,
        RECV_REOPEN_CONNECTION_TYPE = 102,

        CLOSE_CONNECTION_TYPE = 106,
        ENCRYPTION_INFO_EXCHANGE_TYPE = 108,
    }

    /// <summary>
    /// 0--Type
    /// 1--Flag
    /// 2--Encryption Method
    /// 3--(Reserved)maybe compression method later
    /// 4,5,6,7--Data Size
    /// 8,9,10,11--Serial Number
    /// 12,13,14,15--(Reserved)
    /// </summary>
    public class AveDataBlock
    {
        public const int DATA_BLOCK_SIZE = 64 * 1024;
        public const int DATA_BLOCK_HEADER_LEN = 16;
        public const int DATA_BLOCK_DATA_LEN = DATA_BLOCK_SIZE - DATA_BLOCK_HEADER_LEN;

        private byte[] mBuffer = null;

        public AveDataBlock(int bufSize = DATA_BLOCK_SIZE)
        {
            mBuffer = new byte[bufSize];
            ClearDataBuffer();
        }

        public AveDataBlock(byte[] buffer)
        {
            mBuffer = buffer;
        }

        /// <summary>
        /// Get ro set the type of the data block.
        /// </summary>
        public AveDataBlockType Type
        {
            get { return (AveDataBlockType)mBuffer[0]; }
            set { mBuffer[0] = (byte)value; }
        }

        /// <summary>
        /// Get or set the operation flag
        /// </summary>
        public byte Flag
        {
            get { return mBuffer[1]; }
            set { mBuffer[1] = value; }
        }

        /// <summary>
        /// Get or set the operation flag
        /// </summary>
        public byte EncryptMethod
        {
            get { return mBuffer[2]; }
            set { mBuffer[2] = value; }
        }

        /// <summary>
        /// Get or set the size of the real data.
        /// </summary>
        public int DataSize
        {
            get
            {
                return NetworkBytesConverter.ToBigInt(mBuffer, 4);
            }
            set
            {
                NetworkBytesConverter.ToBigBytes(value, mBuffer, 4);
            }
        }

        /// <summary>
        /// Get or set the serial number
        /// </summary>
        public uint SerialNumber
        {
            get
            {
                return NetworkBytesConverter.ToBigUint(mBuffer, 8);
            }
            set
            {
                NetworkBytesConverter.ToBigBytes(value, mBuffer, 8);
            }
        }

        /// <summary>
        /// Get or set the data buffer
        /// </summary>
        public byte[] Buffer
        {
            get { return mBuffer; }
            set { mBuffer = value; }
        }

        /// <summary>
        /// 将data（覆盖）拷贝到DataBlock中，如果DataBlock大小不够则扩充DataBlock，并自动调整DataSize
        /// </summary>
        /// <param name="data">Binary data</param>
        public void PutBinary(byte[] data)
        {
            PutBinary(data, 0, data.Length);
        }

        /// <summary>
        /// 将data（覆盖）拷贝到DataBlock中，如果DataBlock大小不够则扩充DataBlock，并自动调整DataSize
        /// </summary>
        /// <param name="data">Binary data</param>
        public void PutBinary(byte[] data, int offset, int len)
        {
            if (len > this.Buffer.Length - AveDataBlock.DATA_BLOCK_HEADER_LEN)//AveDataBlock.DATA_BLOCK_DATA_LEN)
            {
                byte[] tHeader = new byte[AveDataBlock.DATA_BLOCK_HEADER_LEN];
                Array.Copy(mBuffer, 0, tHeader, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);
                mBuffer = new byte[len + AveDataBlock.DATA_BLOCK_HEADER_LEN];
                Array.Copy(tHeader, 0, mBuffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);
            }
            Array.Copy(data, offset, mBuffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, len);
            DataSize = len;
        }

        /// <summary>
        /// HELPER : Put string data to buffer
        /// </summary>
        /// <param name="str">String</param>
        public void PutString(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            PutBinary(data);
        }

        /// <summary>
        /// HELPER : Retrieve string from data
        /// </summary>
        /// <returns>String data</returns>
        public string RetrieveString()
        {
            return Encoding.UTF8.GetString(
                mBuffer,
                AveDataBlock.DATA_BLOCK_HEADER_LEN,
                DataSize
            );
        }

        /// <summary>
        /// Copy this data block to another with header
        /// </summary>
        /// <param name="another">Another data block</param>
        public void CopyTo(AveDataBlock another)
        {
            // I copy all the buffer
            // Maybe this should be changed
            //changed by lrh, m_buffer.Length should be equal to AveDataBlock.DATA_BLOCK_SIZE most time.
            if (mBuffer.Length > AveDataBlock.DATA_BLOCK_SIZE)
            {
                another.mBuffer = new byte[mBuffer.Length];
            }
            Array.Copy(
                mBuffer,
                0,
                another.mBuffer,
                0,
                mBuffer.Length
            );
        }

        /// <summary>
        /// Copy data to the given buffer without header
        /// </summary>
        /// <param name="buffer">Destination buffer</param>
        /// <param name="offset">Destination offset</param>
        /// <param name="length">Data length</param>
        public void CopyTo(byte[] buffer, int offset, int length)
        {
            Array.Copy(
                mBuffer, DATA_BLOCK_HEADER_LEN, buffer,
                offset, length
            );
        }

        /// <summary>
        /// 调整DataBlock内部数据位置，（除头信息外）将DataBlock数据向前移动len长度，覆盖前len长的字节，并自动调节DataBlock大小
        /// 注意：调用这个函数后，不要再手动改变DataSize大小
        /// </summary>
        /// <param name="len"></param>
        public void AdjustDataBlock(int len)
        {
            Array.Copy(
                        mBuffer, len + AveDataBlock.DATA_BLOCK_HEADER_LEN,
                        mBuffer, AveDataBlock.DATA_BLOCK_HEADER_LEN,
                        DataSize - len
                    );
            DataSize = DataSize - len;
        }

        /// <summary>
        /// 清空DataBlock中的Data部分，并将状态置为UNUSED_TYPE
        /// </summary>
        public void ClearDataBuffer()
        {
            //实际上并不需要真正清空mBuffer中的内容，只需将DataSize置为0即可
            for (int i = 0; i < AveDataBlock.DATA_BLOCK_HEADER_LEN; i++)
            {
                mBuffer[i] = 0;
            }
            Type = AveDataBlockType.UNUSED_TYPE;
        }

        /// <summary>
        /// 将buffer中内容拷贝到DataBlock，作为头信息
        /// </summary>
        /// <param name="buffer">buffer长度为16</param>
        /// <param name="offset"></param>
        /// <returns>false buffer开始拷贝长度大于或等于16，true 拷贝成功</returns>
        public bool CopyToHeader(byte[] buffer, int offset = 0)
        {
            if (buffer.Length < AveDataBlock.DATA_BLOCK_HEADER_LEN + offset)
                return false;
            Array.Copy(buffer, offset, mBuffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);
            return true;
        }

        /// <summary>
        /// 将DataBlock中的头信息拷贝到buffer中
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool CopyFromHeader(byte[] buffer, int offset = 0)
        {
            if (buffer.Length < AveDataBlock.DATA_BLOCK_HEADER_LEN + offset)
                return false;
            Array.Copy(mBuffer, 0, buffer, offset, AveDataBlock.DATA_BLOCK_HEADER_LEN);
            return true;
        }

        /// <summary>
        /// 将buffer中的数据附加到DataBlock数据区中
        /// </summary>
        /// <param name="buffer">需要附加的buffer</param>
        /// <param name="offset">buffer起始位置</param>
        /// <param name="length">附加长度</param>
        /// <param name="enlargeBuffer">如果DataBlock数据区不够长，是否允许改变长度以容纳buffer中的内容</param>
        /// <returns>true buffer内容附加拷贝到DataBlock中，false 无法拷贝</returns>
        public bool AppendBuffer(byte[] buffer, int offset, int length, bool enlargeBuffer = false)
        {
            int realDataSize = DataSize;
            if (this.Buffer.Length - AveDataBlock.DATA_BLOCK_HEADER_LEN - realDataSize >= length)//DataBlock中有足够的空间可以附加buffer的内容
            {
                Array.Copy(buffer, offset, mBuffer, realDataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN, length);
                DataSize = realDataSize + length;
                return true;
            }
            else if (enlargeBuffer)//允许扩大DataBlock数据长度
            {
                byte[] newBuffer = new byte[realDataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN + length];
                Array.Copy(mBuffer, 0, newBuffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN + realDataSize);//将原mBuffer的内容（包括头和数据）拷贝到newBuffer
                Array.Copy(buffer, offset, newBuffer, AveDataBlock.DATA_BLOCK_HEADER_LEN + realDataSize, length);//将buffer的内容附加到newBuffer
                mBuffer = newBuffer;
                DataSize = realDataSize + length;
                return true;
            }
            return false;
        }

    }

}
