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




namespace AvePoint.Media.Core.IO.Input
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.FilteringBox;
    using AvePoint.Media.Service.DomainModel;
    #endregion

    public class CompressedFormatedInputStream : FilterGeneralInputStream
    {
        IDataFilteringBox deCompressionFilteringBox;
        ReadingState readingState = ReadingState.Unknown;

        public CompressedFormatedInputStream(IMediaGeneralInputStream innerInput)
            : base(innerInput)
        { }

        public override void Open()
        {
            this.InnerInputStream.Open();
            this.readingState = ReadingState.Open;
        }

        private Boolean IsNeedMediaDecompress()
        {
            var isMediaCompressedData = (this.CurrentItemIndex.CurrentItemDataMode & GConstants.TransferFlag.MEDIA_COMPRESSED) == GConstants.TransferFlag.MEDIA_COMPRESSED;
            var isAgentCompressedDataButRestoreToFS = (this.CurrentItemIndex.CurrentItemDataMode & GConstants.TransferFlag.AGENT_COMPRESSED) == GConstants.TransferFlag.AGENT_COMPRESSED && CurrentItemIndex.IsRestoreToFS;
            if (isMediaCompressedData || isAgentCompressedDataButRestoreToFS)
                return true;
            else return false;

        }

        public override int ReadMetaDataPart1(byte[] data, int offset, int count)
        {
            if (IsNeedMediaDecompress())
            {
                if (this.readingState == ReadingState.NewItem)
                {
                    this.deCompressionFilteringBox.InputBegin();
                    this.readingState = ReadingState.MetaDataPart1;
                }
                if (this.readingState == ReadingState.MetaDataPart1)
                {
                    int outputLen = this.deCompressionFilteringBox.ReceiveOutput(data, offset, count);
                    if (outputLen != 0) return outputLen;
                    while (true)
                    {
                        byte[] buffer = new byte[IOConstants.WriteBufferMaxSize];
                        int readLen = this.InnerInputStream.ReadMetaDataPart1(buffer, 0, buffer.Length);
                        if (readLen == -1)
                        {
                            this.deCompressionFilteringBox.InputEnd();
                        }
                        else
                        {
                            this.deCompressionFilteringBox.Input(buffer, 0, readLen);
                        }
                        outputLen = this.deCompressionFilteringBox.ReceiveOutput(data, offset, count);
                        if (outputLen == 0) continue;
                        return outputLen;
                    }
                }
                else
                {
                    throw new System.NotSupportedException(string.Format($"CompressedFormated InputStream Read MetaData Part1 Exception:{readingState}"));
                }
            }
            else
            {
                return this.InnerInputStream.ReadMetaDataPart1(data, offset, count);
            }
        }

        public override int ReadContent(byte[] data, int offset, int count)
        {
            if (IsNeedMediaDecompress())
            {
                if (this.readingState == ReadingState.MetaDataPart1 || (this.readingState == ReadingState.NewItem && CurrentItemIndex.IsRestoreToFS))
                {
                    this.deCompressionFilteringBox.InputBegin();
                    this.readingState = ReadingState.ContentData;
                }
                if (this.readingState == ReadingState.ContentData)
                {
                    int outputLen = this.deCompressionFilteringBox.ReceiveOutput(data, offset, count);
                    if (outputLen != 0) return outputLen;
                    while (true)
                    {
                        byte[] buffer = new byte[IOConstants.WriteBufferMaxSize];
                        int readLen = this.InnerInputStream.ReadContent(buffer, 0, buffer.Length);
                        if (readLen == -1)
                        {
                            this.deCompressionFilteringBox.InputEnd();
                        }
                        else
                        {
                            this.deCompressionFilteringBox.Input(buffer, 0, readLen);
                        }
                        outputLen = this.deCompressionFilteringBox.ReceiveOutput(data, offset, count);
                        if (outputLen == 0) continue;
                        return outputLen;
                    }
                }
                else
                {
                    throw new System.NotSupportedException(string.Format($"CompressedFormated InputStream Read MetaData Part1 Exception:{readingState}"));
                }
            }
            else
            {
                return this.InnerInputStream.ReadContent(data, offset, count);
            }
        }

        public override int ReadMetaDataPart2(byte[] data, int offset, int count)
        {
            if (IsNeedMediaDecompress())
            {
                if (this.readingState == ReadingState.ContentData)
                {
                    this.deCompressionFilteringBox.InputBegin();
                    this.readingState = ReadingState.MetaDataPart2;
                }
                if (this.readingState == ReadingState.MetaDataPart2)
                {
                    int outputLen = this.deCompressionFilteringBox.ReceiveOutput(data, offset, count);
                    if (outputLen != 0) return outputLen;
                    while (true)
                    {
                        byte[] buffer = new byte[IOConstants.WriteBufferMaxSize];
                        int readLen = this.InnerInputStream.ReadMetaDataPart2(buffer, 0, buffer.Length);
                        if (readLen == -1)
                        {
                            this.deCompressionFilteringBox.InputEnd();
                        }
                        else
                        {
                            this.deCompressionFilteringBox.Input(buffer, 0, readLen);
                        }
                        outputLen = this.deCompressionFilteringBox.ReceiveOutput(data, offset, count);
                        if (outputLen == 0) continue;
                        return outputLen;
                    }
                }
                else
                {
                    throw new System.NotSupportedException(string.Format($"CompressedFormated InputStream ReadMetaData Part1 Exception:{readingState}"));
                }
            }
            else
            {
                return this.InnerInputStream.ReadMetaDataPart2(data, offset, count);
            }
        }

        public override void EndItem()
        {
            this.InnerInputStream.EndItem();
        }

        public override void Close()
        {
            this.InnerInputStream.Close();
            this.readingState = ReadingState.Close;
        }

        public override String NextItem(IndexBase itemIndex)
        {
            var encryptionInfo = this.InnerInputStream.NextItem(itemIndex);
            this.readingState = ReadingState.NewItem;

            deCompressionFilteringBox = DataFilteringBoxFactory.GetDeCompressionFilteringBox(this.CurrentItemIndex.CurrentItemCompressionMethod);
            return encryptionInfo;
        }

        public override void SetEncryptionInfos(Dictionary<string, DataEncryptionInfo> encryptionInfos, Func<string, DataEncryptionInfo> dataEncryptInfoGetter = null)
        {
            InnerInputStream.SetEncryptionInfos(encryptionInfos, dataEncryptInfoGetter);
        }
    }
}