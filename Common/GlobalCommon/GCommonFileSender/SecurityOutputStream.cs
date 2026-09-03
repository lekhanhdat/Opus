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
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.GCommon.Utility.FilteringBox;

namespace AvePoint.GCommon.FileTransfer
{
    internal class SecurityOutputStream : IGeneralOutputStream
    {
        const int MaxWriteBufferSize = 64 * 1024;
      
        IDataFilteringBox filteringBox;
        IGeneralOutputStream innerOutputStream;
        AveDataBlockType blockType;

        public SecurityOutputStream(CompressionMethods method, CompressionTypes compressionLevel, IGeneralOutputStream innerStream)
        {
            innerOutputStream = innerStream;
            filteringBox = DataFilteringBoxFactory.GetCompressionFilteringBox(method, (int)compressionLevel);
        }

        public SecurityOutputStream(DataEncryptionInfo info, IGeneralOutputStream innerStream)
        {
            innerOutputStream = innerStream;
            DataEncryptionInfoWrapper wrapper = DataEncryptionInfoManager.ResolveDynamicKey(info);
            filteringBox = DataFilteringBoxFactory.GetEncryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey);
        }

        public void Open()
        {
            this.innerOutputStream.Open();
        }

        public void WriteHeaderXml(string headerXml)
        {
            this.blockType = AveDataBlockType.HEADER_TYPE;
            this.innerOutputStream.WriteHeaderXml(headerXml);
        }

        public void WriteMetaData(byte[] data, int offset, int count)
        {
            while (count > MaxWriteBufferSize)
            {
                WriteMetaDataInternal(data, offset, MaxWriteBufferSize);
                offset += MaxWriteBufferSize;
                count -= MaxWriteBufferSize;
            }
            if (count > 0)
            {
                WriteMetaDataInternal(data, offset, count);
            }
        }

        private void WriteMetaDataInternal(byte[] data, int offset, int count)
        {
            if (blockType == AveDataBlockType.HEADER_TYPE)
            {
                Begin(true);
                blockType = AveDataBlockType.DATA_TYPE;
            }
            if (blockType == AveDataBlockType.CONTENTDATA_TYPE)
            {
                End(false);
                Begin(true);
                blockType = AveDataBlockType.DATA_TYPE;
            }
            if (blockType == AveDataBlockType.DATA_TYPE)
            {
                Processing(data, offset, count, true);
            }
            else
            {
                throw new ArgumentException("State error.");
            }
        }

        public void WriteContentData(byte[] data, int offset, int count)
        {
            while (count > MaxWriteBufferSize)
            {
                WriteContentDataInternal(data, offset, MaxWriteBufferSize);
                offset += MaxWriteBufferSize;
                count -= MaxWriteBufferSize;
            }
            if (count > 0)
            {
                WriteContentDataInternal(data, offset, count);
            }
        }

        private void WriteContentDataInternal(byte[] data, int offset, int count)
        {
            if (blockType == AveDataBlockType.HEADER_TYPE)
            {
                Begin(false);
                blockType = AveDataBlockType.CONTENTDATA_TYPE;
            }
            if (blockType == AveDataBlockType.DATA_TYPE)
            {
                End(true);
                Begin(false);
                blockType = AveDataBlockType.CONTENTDATA_TYPE;
            }
            if (blockType == AveDataBlockType.CONTENTDATA_TYPE)
            {
                Processing(data, offset, count, false);
            }
            else
            {
                throw new ArgumentException("State error.");
            }
        }

        public void WriteTailXml(string tailXml)
        {
            if (blockType == AveDataBlockType.HEADER_TYPE)
            {
            }
            else if (blockType == AveDataBlockType.DATA_TYPE)
            {
                End(true);
            }
            else if (blockType == AveDataBlockType.CONTENTDATA_TYPE)
            {
                End(false);
            }
            else
            {
                throw new ArgumentException("State error.");
            }
            this.innerOutputStream.WriteTailXml(tailXml);
        }

        public void Close(string errorMessage)
        {
            this.innerOutputStream.Close(errorMessage);
        }

        private void Begin(bool isMetaData)
        {
            filteringBox.InputBegin();
        }

        private void Processing(byte[] buffer, int offset, int count, bool isMetaData)
        {
            filteringBox.Input(buffer, offset, count);

            byte[] output = new byte[64 * 1024];
            int readLen = 0;
            while (true)
            {
                readLen = filteringBox.ReceiveOutput(output, 0, output.Length);
                if (readLen == 0) break;
                if (isMetaData)
                {
                    this.innerOutputStream.WriteMetaData(output, 0, readLen);
                }
                else
                {
                    this.innerOutputStream.WriteContentData(output, 0, readLen);
                }
            }
        }
       
        private void End(bool isMetaData)
        {
            filteringBox.InputEnd();

            byte[] output = new byte[64 * 1024];
            int readLen = 0;
            while (true)
            {
                readLen = filteringBox.ReceiveOutput(output, 0, output.Length);
                if (readLen == -1) break;
                if (isMetaData)
                {
                    this.innerOutputStream.WriteMetaData(output, 0, readLen);
                }
                else
                {
                    this.innerOutputStream.WriteContentData(output, 0, readLen);
                }
            }
        }
    }
}
