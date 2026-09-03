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




namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.IO;
    #endregion

    /// <summary>
    /// This class is used for replacing MemoryStream for performance
    /// 
    /// <example>
    ///  BufferedMemoryOutputStream ms = new BufferedMemoryOutputStream();
    ///  ms.Write(buffer, 0, buffer.Length); //loop
    ///  byte[] outBuffer = ms.ToArray();
    /// </example>
    /// </summary>
    public class BufferedMemoryOutputStream : Stream
    {
        // Fields
        const int ChunkSize = 64 * 1024;

        private byte[][] chunks = new byte[4][];
        private int chunkCount;

        private byte[] currentChunk;
        private int currentChunkSize;

        private int totalSize = 0;

        public BufferedMemoryOutputStream()
        {
            this.currentChunk = new byte[ChunkSize];
            this.currentChunkSize = 0;

            this.chunks[0] = this.currentChunk;
            this.chunkCount = 1;

        }

        private void AllocNextChunk()
        {
            if (this.chunkCount == this.chunks.Length)
            {
                byte[][] destinationArray = new byte[this.chunks.Length + 4][];
                Array.Copy(this.chunks, destinationArray, this.chunks.Length);
                this.chunks = destinationArray;
            }
            this.currentChunk = new byte[ChunkSize]; ;
            this.currentChunkSize = 0;
            this.chunks[this.chunkCount++] = this.currentChunk;
        }

        public byte[] ToArray()
        {
            byte[] buffer = new byte[totalSize];
            int dstOffset = 0;
            for (int i = 0; i < this.chunkCount - 1; i++)
            {
                byte[] src = this.chunks[i];
                Buffer.BlockCopy(src, 0, buffer, dstOffset, src.Length);
                dstOffset += src.Length;
            }
            Buffer.BlockCopy(this.currentChunk, 0, buffer, dstOffset, this.currentChunkSize);
            return buffer;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.totalSize += count;

            while (count > 0)
            {
                int spaceLeft = this.currentChunk.Length - this.currentChunkSize;
                if (count <= spaceLeft)
                {
                    Buffer.BlockCopy(buffer, offset, this.currentChunk, this.currentChunkSize, count);
                    this.currentChunkSize += count;
                    break;
                }
                else
                {
                    Buffer.BlockCopy(buffer, offset, this.currentChunk, this.currentChunkSize, spaceLeft);
                    this.currentChunkSize += spaceLeft;
                    offset += spaceLeft;
                    count -= spaceLeft;
                    this.AllocNextChunk();
                }
            }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanRead
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanSeek
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanWrite
        {
            get { throw new NotImplementedException(); }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}
