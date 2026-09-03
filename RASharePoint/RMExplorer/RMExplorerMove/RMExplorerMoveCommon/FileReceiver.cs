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
using AvePoint.GCommon.FileTransfer;
using AvePoint.GCommon.Network;
using AvePoint.RA.Common.Global.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    //目前Move 功能只是将文件导入到本地，在将本地文件还原到目的端。所以RAFileReceiver 只需要提供少数实现即可，没必要使用IFileReceiver，也就可以不引用common 的一些dll， 后期完全脱离DA 的时候可以考虑使用自定义接口
    public class RAFileReceiver : IFileReceiver, IDisposable
    {
        #region - Params -

        private readonly string fileName;
        private FileStream stream;
        private AveDataBlock currentDataBlock;
        private int lastReadLength;

        byte[] mReplaceContent = null;
        #endregion

        public RAFileReceiver()
        {
        }

        public RAFileReceiver(string fileName, byte[] replaceContent = null)
        {
            this.fileName = fileName;
            this.stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            mReplaceContent = replaceContent;
        }

        public string Open(string host, int port, string info)
        {
            throw new NotImplementedException();
        }

        private AveDataBlock GetDataBlock(AveDataBlock dataBlock)
        {
            if (dataBlock == null)
            {
                dataBlock = new AveDataBlock();
            }

            Read(dataBlock.Buffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);

            var dataSize = dataBlock.DataSize;

            if (dataSize > 0)
            {
                Read(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataSize);
            }

            return dataBlock;
        }

        private void Read(byte[] buffer, int offset, int length)
        {
            while (length > 0)
            {
                var read = stream.Read(buffer, offset, length);

                if (read <= 0)
                {
                    break;
                }

                offset += read;
                length -= read;
            }

            if (length > 0)
            {
                throw new Exception(string.Format("There is no available content from file:{0}", fileName));
            }
        }

        private AveDataBlock GetSpecialDataBlock(AveDataBlock dataBlock, AveDataBlockType dataBlockType)
        {
            while (true)
            {
                dataBlock = GetDataBlock(dataBlock);

                if (dataBlock.Type == dataBlockType)
                {
                    break;
                }
            }

            return dataBlock;
        }

        public string GetNextFileHead()
        {
            currentDataBlock = GetDataBlock(currentDataBlock);

            if (currentDataBlock.Type != AveDataBlockType.HEADER_TYPE)
            {
                throw new Exception(string.Format("The current type:{0} is not header", currentDataBlock.Type));
            }

            return currentDataBlock.RetrieveString();
        }

        public string GetFileTail()
        {
            if (currentDataBlock.Type != AveDataBlockType.TAIL_TYPE)
            {
                currentDataBlock = GetSpecialDataBlock(currentDataBlock, AveDataBlockType.TAIL_TYPE);
            }

            return currentDataBlock.RetrieveString();
        }

        public int CRC32Match()
        {
            throw new NotImplementedException();
        }

        public int ReadBytes(byte[] buffer, int len)
        {
            return ReadBytes(buffer, 0, len);
        }

        public int ReadBytes(byte[] buffer, int offset, int len)
        {
            int readLen = 0;
            var type = AveDataBlockType.UNKNOW_TYPE;

            if (currentDataBlock != null)
            {
                type = currentDataBlock.Type;
            }

            if (type != AveDataBlockType.DATA_TYPE && type != AveDataBlockType.CONTENTDATA_TYPE)
            {
                currentDataBlock = GetDataBlock(currentDataBlock);
                type = currentDataBlock.Type;
                lastReadLength = 0;
                //if (type != AveDataBlockType.DATA_TYPE && type != AveDataBlockType.CONTENTDATA_TYPE)
                //{
                //    return 0;
                //    //throw new Exception(string.Format("The next type is {0}, which is not data type.", type));
                //}
            }

            if (type == AveDataBlockType.DATA_TYPE || type == AveDataBlockType.CONTENTDATA_TYPE)
            {
                ArgumentCheck.NotNull(currentDataBlock, nameof(currentDataBlock));
                while (len > 0)
                {

                    if (mReplaceContent != null && currentDataBlock.Type == AveDataBlockType.CONTENTDATA_TYPE)
                    {
                        Array.Copy(mReplaceContent, lastReadLength, buffer,
                                  offset, len);
                        lastReadLength += len;
                        offset += len;
                        readLen += len;
                        len = 0;
                        return readLen;
                    }
                    var dataSize = currentDataBlock.DataSize - lastReadLength;

                    if (dataSize >= len)
                    {
                        Array.Copy(currentDataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN + lastReadLength, buffer,
                                   offset, len);

                        lastReadLength += len;
                        offset += len;
                        readLen += len;
                        len = 0;
                    }
                    else
                    {
                        Array.Copy(currentDataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN + lastReadLength, buffer,
                                   offset, dataSize);
                        lastReadLength = 0;
                        offset += dataSize;
                        readLen += dataSize;
                        len -= dataSize;
                        currentDataBlock = GetDataBlock(currentDataBlock);
                        if (currentDataBlock.Type != AveDataBlockType.DATA_TYPE &&
                            currentDataBlock.Type != AveDataBlockType.CONTENTDATA_TYPE)
                        {
                            break;
                        }
                    }
                }
            }
            return readLen;
        }

        public string Close(string errorMsg)
        {
            Dispose();

            return string.Empty;
        }

        public void Dispose()
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
        }
    }
}
