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
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Transfer.Data.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AvePoint.GCommon.Transfer.Data.Multiple.Util
{

    public sealed class SegmentedDataTransfer : IDataSender, IDataReceiver, IDisposable
    {
        private LinkedList<SegmentedStream> streamList = new LinkedList<SegmentedStream>();
        private AveDataBlock writeBlock;
        private AveDataBlock readBlock;
        //private AveDataBlock innerBlock;
        private int readPos;
        private SegmentedStream readStream;
        private SegmentedStream writeStream;
        private LinkedListNode<SegmentedStream> currentNode = null;
        private string tempFolder;
        public SegmentedDataTransfer(string tempFolder)
        {
            this.tempFolder = tempFolder;
        }

        private void ResetWriteBlock(AveDataBlockType blockType)
        {
            writeBlock.Type = blockType;
            writeBlock.DataSize = 0;
            writeBlock.SerialNumber++;
        }

        private void EnsureDataBlock(bool isWrite)
        {
            if (isWrite)
            {
                if (writeBlock == null) writeBlock = new AveDataBlock();
            }
            else
            {
                if (readBlock == null) readBlock = new AveDataBlock();
            }
        }

        public void WriteHead(string xml)
        {
            EnsureDataBlock(true);

            this.writeStream = new SegmentedStream(tempFolder);
            streamList.AddLast(this.writeStream);

            ResetWriteBlock(AveDataBlockType.HEADER_TYPE);
            writeBlock.PutString(xml);
            writeStream.Write(writeBlock.Buffer, 0, writeBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
        }
        public void WriteData(byte[] buf, int offset, int length)
        {
            ResetWriteBlock(AveDataBlockType.DATA_TYPE);
            writeBlock.PutBinary(buf, offset, length);
            writeStream.Write(writeBlock.Buffer, 0, writeBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
        }

        public void WriteTail(string xml)
        {
            WriteTail(xml, true);
        }

        public void WriteTail(string xml, bool isOK)
        {
            if (isOK)
            {
                ResetWriteBlock(AveDataBlockType.TAIL_TYPE);
            }
            else
            {
                ResetWriteBlock(AveDataBlockType.FLUSH_TYPE);//添加Type比较费劲，就用Flush type来替代为失败的tail
            }
            writeBlock.PutString(xml);
            writeStream.Write(writeBlock.Buffer, 0, writeBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);

            writeStream.CloseFileWriteHandle();
            this.writeBlock = null;
        }

        private int SafeRead(Stream stream, byte[] buffer, int offset, int len)
        {
            int read = 0;

            while (read < len)
            {
                var l = stream.Read(buffer, offset + read, len - read);
                if (l <= 0)
                {
                    throw new Exception("Read from a closed stream.");
                }
                read += l;
            }
            return read;
        }
        private void ReadNextBlock()
        {
            if (this.readBlock.Type == AveDataBlockType.UNKNOW_TYPE || this.readBlock.Type == AveDataBlockType.UNUSED_TYPE)
            {
                SafeRead(readStream, readBlock.Buffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);
                if (readBlock.DataSize > readBlock.Buffer.Length - AveDataBlock.DATA_BLOCK_HEADER_LEN)
                {
                    var largeBlock = new AveDataBlock(readBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN * 2);
                    Array.Copy(readBlock.Buffer, 0, largeBlock.Buffer, 0, readBlock.Buffer.Length);
                    readBlock = largeBlock;
                }

                SafeRead(readStream, readBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, readBlock.DataSize);
            }
        }

        private void SetCurrentSteam(bool needWait)
        {
            if (readStream == null)
            {
                currentNode = this.streamList.First;
            }
            else
            {
                readStream = null;
                if (needWait)
                {
                    var start = DateTime.Now;
                    while (currentNode.Next == null)
                    {
                        if (start.AddMinutes(30) < DateTime.Now)
                            break;

                        Thread.Sleep(100);
                    }
                }
                currentNode = currentNode.Next;
            }

            if (currentNode != null)
            {
                readStream = currentNode.Value;
            }
        }

        public string GetNextFileHeadComplete()
        {
            EnsureDataBlock(false);

            SetCurrentSteam(false);

            if (readStream == null)
            {
                return null;
            }
            this.readBlock.ClearDataBuffer();

            ReadNextBlock();

            if (readBlock.Type != AveDataBlockType.HEADER_TYPE)
            {
                throw new Exception("Invalid data type: " + readBlock.Type.ToString());
            }

            var header = readBlock.RetrieveString();
            readBlock.Type = AveDataBlockType.UNKNOW_TYPE;
            readBlock.DataSize = 0;

            return header;
        }

        public string GetNextFileHead()
        {
            EnsureDataBlock(false);

            SetCurrentSteam(true);

            if (readStream == null)
            {
                return null;
            }
            this.readBlock.ClearDataBuffer();

            ReadNextBlock();

            if (readBlock.Type != AveDataBlockType.HEADER_TYPE)
            {
                throw new Exception("Invalid data type: " + readBlock.Type.ToString());
            }

            var header = readBlock.RetrieveString();
            readBlock.Type = AveDataBlockType.UNKNOW_TYPE;
            readBlock.DataSize = 0;

            return header;
        }

        public string GetFileTail()
        {
            if (readBlock.Type != AveDataBlockType.TAIL_TYPE)
            {
                throw new Exception("Invalid data type:" + readBlock.Type.ToString());
            }

            var tail = readBlock.RetrieveString();

            this.readStream.Close();
            this.readBlock = null;

            return tail;
        }

        public int ReadBytes(byte[] buffer, int len)
        {
            return ReadBytes(buffer, 0, len);
        }

        public int ReadBytes(byte[] buffer, int offset, int len)
        {
            ReadNextBlock();

            if (readBlock.Type != AveDataBlockType.DATA_TYPE)
            {
                return -1;
            }

            var rest = readBlock.DataSize - readPos;
            if (rest > len)
            {
                Array.Copy(readBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN + readPos, buffer, offset, len);
                readPos += len;

                return len;
            }
            else
            {
                Array.Copy(readBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN + readPos, buffer, offset, rest);
                readBlock.ClearDataBuffer();
                readPos = 0;

                return rest;
            }
        }

        public bool Open(GCommon.Transfer.Common.DataTransferSetting setting, string sessionId)
        {
            return true;
        }

        public string Close()
        {
            Dispose();
            return string.Empty;
        }

        public GCommon.Transfer.Common.DataTransferResultStatus DataTransferStatus
        {
            get { throw new NotImplementedException(); }
        }

        public void Stop(string message)
        {
            foreach (var stream in streamList)
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        public void Dispose()
        {
            Stop("");
        }
    }
}