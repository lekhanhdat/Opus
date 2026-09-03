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
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.FileTransfer;
using AvePoint.GCommon.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    //目前Move 功能只是将文件导入到本地，在将本地文件还原到目的端。所以RAFileSender 只需要提供少数实现即可，没必要使用IFileSender，也就可以不引用common 的一些dll， 后期完全脱离DA 的时候可以考虑使用自定义接口
    public class RAFileSender : IFileSender, IDisposable
    {
        private readonly string file;
        private Stream stream;
        private AveDataBlock dataBlock;

        public RAFileSender()
        {
        }

        public RAFileSender(string file)
        {
            this.file = file;
            this.stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        }

        public string Open(string host, int port, string connectInfo, string reconnectInfo)
        {
            throw new NotImplementedException();
        }

        public string Open(Dictionary<string, int> mediaHosts, string connectInfo, int reconnectTimeOut = 1800000, int reconnectInterval = 30000)
        {
            throw new NotImplementedException();
        }

        public void SetServerFlag(long flag)
        {
            throw new NotImplementedException();
        }

        public void SetEncryptionInfo(DataEncryptionInfo info)
        {
            throw new NotImplementedException();
        }

        public void SetQueueBufferSize(int blockCount)
        {
            throw new NotImplementedException();
        }

        public void SetTestRunFlag(bool isTestRun)
        {
            throw new NotImplementedException();
        }

        public void SetCertificationFlag(int useCRC)
        {
            throw new NotImplementedException();
        }

        public void ReceiveDataBlock(ref AveDataBlock dataBlock)
        {
            throw new NotImplementedException();
        }

        public void WriteHead(string xml)
        {
            var block = new AveDataBlock();
            block.Type = AveDataBlockType.HEADER_TYPE;
            block.PutString(xml);
            WriteBlock(block);
        }

        private void WriteBlock(AveDataBlock block)
        {
            var buffer = block.Buffer;
            stream.Write(buffer, 0, block.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
            block.ClearDataBuffer();
        }

        public void WriteData(byte[] buf, int offset, int length)
        {
            if (dataBlock == null)
            {
                dataBlock = new AveDataBlock();
                dataBlock.Type = AveDataBlockType.DATA_TYPE;
            }
            else if (dataBlock.Type == AveDataBlockType.CONTENTDATA_TYPE)
            {
                WriteBlock(dataBlock);
                dataBlock.Type = AveDataBlockType.DATA_TYPE;
            }
            if (dataBlock.Type != AveDataBlockType.DATA_TYPE)
            {
                throw new Exception(string.Format("the current type:{0} is not data type.", dataBlock.Type));
            }

            var freeLength = AveDataBlock.DATA_BLOCK_DATA_LEN - AveDataBlock.DATA_BLOCK_HEADER_LEN - dataBlock.DataSize;

            while (length > 0)
            {
                if (length >= freeLength)
                {
                    dataBlock.AppendBuffer(buf, offset, freeLength);
                    length -= freeLength;
                    offset += freeLength;
                    WriteBlock(dataBlock);
                    dataBlock.Type = AveDataBlockType.DATA_TYPE;
                    freeLength = AveDataBlock.DATA_BLOCK_SIZE - AveDataBlock.DATA_BLOCK_HEADER_LEN;
                }
                else
                {
                    dataBlock.AppendBuffer(buf, offset, length);
                    offset += length;
                    length = 0;
                    freeLength -= length;
                }
            }
        }

        public void WriteContentData(byte[] buf, int offset, int length)
        {
            if (dataBlock == null)
            {
                dataBlock = new AveDataBlock();
                dataBlock.Type = AveDataBlockType.CONTENTDATA_TYPE;
            }
            else if (dataBlock.Type == AveDataBlockType.DATA_TYPE)
            {
                WriteBlock(dataBlock);
                dataBlock.Type = AveDataBlockType.CONTENTDATA_TYPE;
            }
            if (dataBlock.Type != AveDataBlockType.CONTENTDATA_TYPE)
            {
                throw new Exception(string.Format("the current type:{0} is not content type.", dataBlock.Type));
            }
            var freeLength = AveDataBlock.DATA_BLOCK_DATA_LEN - AveDataBlock.DATA_BLOCK_HEADER_LEN - dataBlock.DataSize;
            while (length > 0)
            {
                if (length >= freeLength)
                {
                    dataBlock.AppendBuffer(buf, offset, freeLength);
                    length -= freeLength;
                    offset += freeLength;
                    WriteBlock(dataBlock);
                    dataBlock.Type = AveDataBlockType.CONTENTDATA_TYPE;
                    freeLength = AveDataBlock.DATA_BLOCK_SIZE - AveDataBlock.DATA_BLOCK_HEADER_LEN;
                }
                else
                {
                    dataBlock.AppendBuffer(buf, offset, length);
                    offset += length;
                    length = 0;
                    freeLength -= length;
                }
            }
        }

        public long WriteTail(string xml)
        {
            return WriteTail(xml, true);
        }

        public long WriteTail(string xml, bool isOk)
        {
            if (dataBlock.DataSize > 0)
            {
                WriteBlock(dataBlock);
            }
            var block = new AveDataBlock();
            block.Type = AveDataBlockType.TAIL_TYPE;
            block.PutString(xml);
            WriteBlock(block);
            return 0;
        }

        public void SetReadMessageWorker(IFileSenderResponseWorker worker)
        {
            throw new NotImplementedException();
        }

        public void Close(string message)
        {
            Dispose();
        }

        public void Dispose()
        {
            if (stream != null)
            {
                if (dataBlock != null && dataBlock.DataSize > 0)
                {
                    WriteBlock(dataBlock);
                }
                stream.Flush();
                stream.Close();
                stream = null;
            }
        }
    }
}