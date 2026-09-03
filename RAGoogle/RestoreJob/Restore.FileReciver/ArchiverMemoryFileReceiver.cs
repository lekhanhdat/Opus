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
using AvePoint.Media.Service.ArchiverBackup.Restore;

namespace RAGoogle.Restore
{
    public class ArchiverMemoryFileReceiver : IFileReceiver
    {
        private IArchiverRestoreDataBlockManager dataBlockManager;

        private ArchiverRestoreDataBlock readingBlock;

        public ArchiverMemoryFileReceiver(IArchiverRestoreDataBlockManager manager)
        {
            this.dataBlockManager = manager;
        }

        /// <summary>
        /// Read next data block
        /// </summary>
        private ArchiverRestoreDataBlock ReadNextBlock()
        {
            return this.dataBlockManager.Get();
        }

        /// <summary>
        /// read next FileHeader
        /// </summary>
        /// <returns>null means got CLOSE_CONNECTION_TYPE</returns>      
        public string GetNextFileHead()
        {
            do
            {
                if (readingBlock != null && readingBlock.DataBlockType == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                {
                    //todo
                    string errorMessage = readingBlock.RestoreMessage;
                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        return null;
                    }
                    else
                    {
                        throw new ClosedWithErrorException(errorMessage);
                    }
                }
                this.readingBlock = ReadNextBlock();
            }
            while (readingBlock.DataBlockType != AveDataBlockType.HEADER_TYPE);

            string headerStr = readingBlock.RestoreMessage;
            //readingBlock.ClearDataBuffer();
            return headerStr;
        }

        public int ReadBytes(byte[] buffer, int len)
        {
            return ReadBytes(buffer, 0, len);
        }

        /// <summary>
        /// read metadata and content bytes, 
        /// Avoid to call AdjustDataBlock a lot, it will copy bytes 
        /// </summary>
        /// <param name="buffer">buffer to fill</param>
        /// <param name="offset">offset to fill</param>
        /// <param name="len">expect length to read</param>
        /// <returns>0 means reach the end of current file</returns>
        public int ReadBytes(byte[] buffer, int offset, int len)
        {
            if (readingBlock.DataBlockType == AveDataBlockType.TAIL_TYPE)
            {
                return 0;
            }
            if (readingBlock.DataBlockType == AveDataBlockType.CLOSE_CONNECTION_TYPE)
            {
                string errorMessage = readingBlock.RestoreMessage;
                throw new ClosedWithErrorException(errorMessage);
            }
            int returnLen = 0;
            int dataSize = readingBlock.DataSize;
            if (dataSize > 0)
            {
                if (len <= dataSize)
                {
                    readingBlock.CopyTo(buffer, offset, len);
                    return len;
                }
                else
                {
                    readingBlock.CopyTo(buffer, offset, dataSize);
                    len -= dataSize;
                    offset += dataSize;
                    returnLen = dataSize;
                    dataSize = 0;
                }
            }
            while (true)
            {
                this.readingBlock = ReadNextBlock();
                if (readingBlock.DataBlockType == AveDataBlockType.CLOSE_CONNECTION_TYPE || readingBlock.DataBlockType == AveDataBlockType.TAIL_TYPE)
                {
                    break;
                }

                if (readingBlock.DataBlockType == AveDataBlockType.DATA_TYPE || readingBlock.DataBlockType == AveDataBlockType.CONTENTDATA_TYPE)
                {
                    dataSize = readingBlock.DataSize;
                    if (dataSize <= len)
                    {
                        readingBlock.CopyTo(buffer, offset, dataSize);
                        offset += dataSize;
                        len -= dataSize;
                        returnLen += dataSize;

                        if (len == 0) break;
                    }
                    else
                    {
                        readingBlock.CopyTo(buffer, offset, len);
                        returnLen += len;
                        break;
                    }
                }
            }
            return returnLen;
        }

        public string GetFileTail()
        {
            if (readingBlock.DataBlockType == AveDataBlockType.CLOSE_CONNECTION_TYPE)
            {
                string errorMessage = readingBlock.RestoreMessage;
                throw new ClosedWithErrorException(errorMessage);
            }
            return readingBlock.RestoreMessage;
        }

        public string Close(string errorMsg)
        {
            return null;
        }

        /// <summary>
        /// Get the value that if the CRC is matched.
        /// </summary>
        /// <returns>If the calculate value is right, return 1; else return 0;if the CRC is disabled, return -1</returns>
        public int CRC32Match()
        {
            return -1;
        }

        public void Dispose()
        {

        }

        public string Open(string host, int port, string info)
        {
            throw new System.NotImplementedException();
        }

    }
}
