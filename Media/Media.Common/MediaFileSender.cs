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




namespace AvePoint.Media.Common
{
    #region using directives
    using System;
    using System.Xml;
    using AvePoint.GCommon.FileTransfer;
    using AvePoint.GCommon.Network;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.RA.Common.Global.Utils;
    using SerializerHelper = GCommon.Utility.SerializerHelper;
    #endregion

    #region CodeReview
    [AveCodeReview(
        "2011/12/23",
        "yhzhang@avepoint.com",
        "dwxue@avepoint.com",
        new String[] { CodeReviewConstants.CHECK_LIST_ID_SOCKET_1 },
        null,
        true)]
    #endregion
    public class MediaFileSender
    {
        BlockSender innerBlockSender;
        Byte dataMode;
        AveDataBlock bufferBlock;
        Int64 fileSize;
        Boolean isSendTail;
        XmlDocument document;

        public MediaFileSender()
        {
            dataMode = 0;
            bufferBlock = null;
            document = new XmlDocument();
            isSendTail = true;
        }

        public void Wrap(IAveNetwork netWork)
        {
            innerBlockSender = new BlockSender(netWork);
            innerBlockSender.Start();
        }

        public void WriteHead(string xml, byte flag, long crc)
        {
            if (!isSendTail)
            {
                WriteTail(string.Empty);
                isSendTail = true;
            }
            if (crc != 0)
            {
                document.LoadXml(xml);
                var xmlElement = document.DocumentElement;
                xmlElement.SetAttribute("CRC32", crc.ToString());
                xml = document.InnerXml;
            }
            dataMode = flag;
            var block = GetFreeBlock(AveDataBlockType.HEADER_TYPE);
            block.PutString(xml);
            innerBlockSender.SendDataBlock(block);
            bufferBlock = null;
            fileSize = 0;
            isSendTail = false;
        }

        public void WriteData(byte[] buf, int offset, int length)
        {
            if (bufferBlock == null)
            {
                bufferBlock = GetFreeBlock(AveDataBlockType.DATA_TYPE);
            }
            if (bufferBlock.Type != AveDataBlockType.DATA_TYPE)
            {
                innerBlockSender.SendDataBlock(bufferBlock);
                bufferBlock = GetFreeBlock(AveDataBlockType.DATA_TYPE);
            }
            fileSize += length;
            RealWrite(AveDataBlockType.DATA_TYPE, buf, offset, length);
        }

        public void WriteContentData(byte[] buf, int offset, int length)
        {
            if (bufferBlock == null)
            {
                bufferBlock = GetFreeBlock(AveDataBlockType.CONTENTDATA_TYPE);
            }
            if (bufferBlock.Type != AveDataBlockType.CONTENTDATA_TYPE)
            {
                innerBlockSender.SendDataBlock(bufferBlock);
                bufferBlock = GetFreeBlock(AveDataBlockType.CONTENTDATA_TYPE);
            }
            fileSize += length;
            RealWrite(AveDataBlockType.CONTENTDATA_TYPE, buf, offset, length);
        }

        public void WriteOtherData(byte[] buf, int offset, int length, AveDataBlockType type)
        {
            if (length > AveDataBlock.DATA_BLOCK_DATA_LEN)
            {
                throw new Exception("WriteOtherData length is larger then " + AveDataBlock.DATA_BLOCK_DATA_LEN);
            }
            if (bufferBlock != null && bufferBlock.DataSize > 0)
            {
                innerBlockSender.SendDataBlock(bufferBlock);
            }
            bufferBlock = GetFreeBlock(type);
            bufferBlock.Flag = 0;
            bufferBlock.PutBinary(buf, offset, length);
            innerBlockSender.SendDataBlock(bufferBlock);
            bufferBlock = null;
        }

        public void WriteTail(string errorMessage)
        {
            if (bufferBlock != null && bufferBlock.DataSize > 0)
                innerBlockSender.SendDataBlock(bufferBlock);
            bufferBlock = this.GetFreeBlock(AveDataBlockType.TAIL_TYPE);
            RestoreFileTail tail = new RestoreFileTail()
            {
                FileSize = fileSize,
                HasException = !string.IsNullOrEmpty(errorMessage),
                ErrorMessage = errorMessage
            };
            var tailString = SerializerHelper.SerializeByDataContractSerializer(tail);
            bufferBlock.PutString(tailString);
            innerBlockSender.SendDataBlock(bufferBlock);
            isSendTail = true;
            bufferBlock = null;
        }

        public void Close(String msg)
        {
            if (bufferBlock != null && bufferBlock.DataSize > 0)
            {
                innerBlockSender.SendDataBlock(bufferBlock);
            }
            bufferBlock = GetFreeBlock(AveDataBlockType.CLOSE_CONNECTION_TYPE);
            bufferBlock.PutString(msg);
            innerBlockSender.SendDataBlock(bufferBlock);
            bufferBlock = null;

            innerBlockSender.WaitForSendCompleted(1800 * 1000);
            innerBlockSender.Close();
        }

        private void RealWrite(AveDataBlockType type, byte[] buf, int offset, int length)
        {
            int availableSize = AveDataBlock.DATA_BLOCK_DATA_LEN - bufferBlock.DataSize;
            while (length > availableSize)
            {
                bufferBlock.AppendBuffer(buf, offset, availableSize);
                innerBlockSender.SendDataBlock(bufferBlock);
                bufferBlock = GetFreeBlock(type);
                offset += availableSize;
                length -= availableSize;
                availableSize = AveDataBlock.DATA_BLOCK_DATA_LEN - bufferBlock.DataSize;
            }
            if (length > 0)
            {
                bufferBlock.AppendBuffer(buf, offset, length);
            }
        }

        private AveDataBlock GetFreeBlock(AveDataBlockType type)
        {
            var block = innerBlockSender.GetFreeBlock();
            block.Type = type;
            block.SerialNumber = 0;
            block.Flag = dataMode;
            block.DataSize = 0;
            return block;
        }
    }
}