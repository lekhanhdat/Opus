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



namespace AvePoint.GCommon.FileTransfer
{
    #region using directives
    using System;
    using Utility.FilteringBox;
    using Network;
    using Utility;
    using Utility.Cryptography.DataEncryptionManagement;
    using Utility.Cryptography;
    using Contract.Server.ControlPanel.Cryptography;
    using System.Xml;
    #endregion

    internal class SecurityInputStream : IBlockReader
    {
        private IBlockReader prevousReader;
        private IDataFilteringBox filteringBox;
        private AveDataBlockType lastBlockType;
        private byte[] lastBlockHeader;
        private AveDataBlock cachedBlock;
        private Boolean needProcess;
        private Boolean isTailBlockCached;
        private Boolean isDataBlockCached;
        private Boolean isEncryptionStream;

        public SecurityInputStream(IBlockReader preReader, bool isEncryptStream)
        {
            prevousReader = preReader;
            isEncryptionStream = isEncryptStream;

            lastBlockType = AveDataBlockType.UNKNOW_TYPE;
            lastBlockHeader = new byte[AveDataBlock.DATA_BLOCK_HEADER_LEN];
            cachedBlock = new AveDataBlock();
        }

        public void ReadDataBlock(AveDataBlock dataBlock)
        {
            while (true)
            {
                RealReadDataBlock(dataBlock);
                if ((dataBlock.Type == AveDataBlockType.DATA_TYPE || dataBlock.Type == AveDataBlockType.CONTENTDATA_TYPE)
                    && dataBlock.DataSize == 0)
                {
                }
                else
                {
                    break;
                }
            }
        }

        private void RealReadDataBlock(AveDataBlock dataBlock)
        {
            if (needProcess)
            {
                ReceiveOutputAsDataBlock(dataBlock);
                if (dataBlock.DataSize > 0) return;

                if (isTailBlockCached)
                {
                    cachedBlock.CopyTo(dataBlock);
                    isTailBlockCached = false;
                    return;
                }

                if (isDataBlockCached)
                {
                    filteringBox.InputBegin();
                    cachedBlock.CopyFromHeader(lastBlockHeader);
                    filteringBox.Input(cachedBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, cachedBlock.DataSize);
                    ReceiveOutputAsDataBlock(dataBlock);
                    isDataBlockCached = false;
                    lastBlockType = dataBlock.Type;
                    return;
                }
            }

            prevousReader.ReadDataBlock(dataBlock);

            switch (dataBlock.Type)
            {
                case AveDataBlockType.HEADER_TYPE:
                    if (isEncryptionStream)
                    {
                        needProcess = (dataBlock.Flag & GConstants.TransferFlag.AGENT_ENCRYPTED) != 0;
                        if (needProcess)
                        {
                            var headerXml = dataBlock.RetrieveString();
                            var xmlDocument = new XmlDocument();
                            xmlDocument.LoadXml(headerXml);
                            // ReSharper disable PossibleNullReferenceException
                            string serializedEncryptionInfo = xmlDocument.DocumentElement.GetAttribute("encryptionInfo");
                            // ReSharper restore PossibleNullReferenceException
                            var encryptionInfo = (DataEncryptionInfo)SerializerHelper.DeserializeFromBase64StringByDataContractSerializer(serializedEncryptionInfo, typeof(DataEncryptionInfo));
                            var wrapper = DataEncryptionInfoManager.ResolveDynamicKey(encryptionInfo);
                            filteringBox = DataFilteringBoxFactory.GetDecryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey);
                        }
                    }
                    else
                    {
                        needProcess = (dataBlock.Flag & GConstants.TransferFlag.AGENT_COMPRESSED) != 0;
                        if (needProcess)
                        {
                            filteringBox = DataFilteringBoxFactory.GetDeCompressionFilteringBox(CompressionMethods.ZLIB_COMPRESSION);
                        }
                    }
                    if (needProcess)
                    {
                        filteringBox.InputBegin();
                    }
                    break;
                case AveDataBlockType.DATA_TYPE:
                case AveDataBlockType.CONTENTDATA_TYPE:
                    if (needProcess)
                    {
                        if ((lastBlockType == AveDataBlockType.DATA_TYPE && dataBlock.Type == AveDataBlockType.CONTENTDATA_TYPE)
                            || (lastBlockType == AveDataBlockType.CONTENTDATA_TYPE && dataBlock.Type == AveDataBlockType.DATA_TYPE))
                        {
                            //cache the block as start of next stream segment, and finish the current segment stream
                            dataBlock.CopyTo(cachedBlock);
                            isDataBlockCached = true;
                            filteringBox.InputEnd();
                            ReceiveOutputAsDataBlock(dataBlock);
                        }
                        else
                        {
                            dataBlock.CopyFromHeader(lastBlockHeader);
                            filteringBox.Input(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                            ReceiveOutputAsDataBlock(dataBlock);
                        }
                    }
                    break;
                case AveDataBlockType.TAIL_TYPE:
                    if (needProcess)
                    {
                        dataBlock.CopyTo(cachedBlock);
                        isTailBlockCached = true;

                        filteringBox.InputEnd();
                        ReceiveOutputAsDataBlock(dataBlock);
                    }
                    break;
            }
            lastBlockType = dataBlock.Type;
        }

        private void ReceiveOutputAsDataBlock(AveDataBlock dataBlock)
        {
            int realLen = filteringBox.ReceiveOutput(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, AveDataBlock.DATA_BLOCK_DATA_LEN);
            dataBlock.CopyToHeader(lastBlockHeader);
            dataBlock.DataSize = realLen > 0 ? realLen : 0;
        }

        public IBlockReader PrevReader
        {
            get { return prevousReader; }
            set { prevousReader = value; }
        }

        public void Close(string errorMessage)
        {
            prevousReader.Close(errorMessage);
        }


        public void SendDataBlock(AveDataBlock sendBlock)
        {
            prevousReader.SendDataBlock(sendBlock);
        }
    }
}
