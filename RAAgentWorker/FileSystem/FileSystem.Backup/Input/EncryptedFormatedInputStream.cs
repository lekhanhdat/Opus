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
    using AvePoint.GCommon.Utility.Cryptography;
    using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
    using AvePoint.GCommon.Utility.FilteringBox;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    #endregion

    public class EncryptedFormatedInputStream : FilterGeneralInputStream
    {
        Func<string, DataEncryptionInfo> encryptionInfoGetter;
        Dictionary<string, DataEncryptionInfo> encryptionInfos;
        Dictionary<string, IDataFilteringBox> decryptionFilteringBoxes;
        ReadingState readingState = ReadingState.Unknown;

        public EncryptedFormatedInputStream(IMediaGeneralInputStream innerInput)
            : base(innerInput)
        { }
        public override void Open()
        {
            this.InnerInputStream.Open();
            this.readingState = ReadingState.Open;
        }

        private long GetDataMode(bool checkForContent)
        {
            //if (checkForContent)
            //{
            //    var archiverIndex = this.CurrentItemIndex as ArchiverBasicIndex;
            //    if (archiverIndex != null && archiverIndex.IsDeduplicateData)
            //    {
            //        return archiverIndex.DedupSourceFileFlag;
            //    }
            //}

            return this.CurrentItemIndex.CurrentItemDataMode;
        }

        private string GetBackupJobId(bool checkForContent)
        {
            //if (!checkForContent)
            //{
            //    var archiverIndex = this.CurrentItemIndex as ArchiverBasicIndex;
            //    if (archiverIndex != null && archiverIndex.IsDeduplicateData)
            //    {
            //        return archiverIndex.JobId;
            //    }
            //}

            return this.CurrentItemIndex.BackupJobId;
        }

        private bool IsNeedMediaDecrypt(bool forItemContent)
        {
            var dateMode = GetDataMode(forItemContent);
            var isMediaEncryptedData = (dateMode & GConstants.TransferFlag.MEDIA_ENCRYPTED) == GConstants.TransferFlag.MEDIA_ENCRYPTED;
            var isAgentEncryptedDataButRestoreToFS = (dateMode & GConstants.TransferFlag.AGENT_ENCRYPTED) == GConstants.TransferFlag.AGENT_ENCRYPTED && CurrentItemIndex.IsRestoreToFS;
            if (isMediaEncryptedData || isAgentEncryptedDataButRestoreToFS)
                return true;
            else return false;
        }

        private bool IsNeedAgentOrMediaDecrypt(bool forItemContent)
        {
            var dateMode = GetDataMode(forItemContent);
            var isMediaEncryptedData = (dateMode & GConstants.TransferFlag.MEDIA_ENCRYPTED) == GConstants.TransferFlag.MEDIA_ENCRYPTED;
            var isAgentEncryptedData = (dateMode & GConstants.TransferFlag.AGENT_ENCRYPTED) == GConstants.TransferFlag.AGENT_ENCRYPTED;
            return isAgentEncryptedData || isMediaEncryptedData;
        }

        public override int ReadMetaDataPart1(byte[] data, int offset, int count)
        {
            if (IsNeedMediaDecrypt(false))
            {
                var decryptionFilteringBox = GetDecryptionFilteringBox(GetBackupJobId(false));
                if (this.readingState == ReadingState.NewItem)
                {
                    decryptionFilteringBox.InputBegin();
                    this.readingState = ReadingState.MetaDataPart1;
                }
                if (this.readingState == ReadingState.MetaDataPart1)
                {
                    int outputLen = decryptionFilteringBox.ReceiveOutput(data, offset, count);
                    if (outputLen != 0) return outputLen;
                    while (true)
                    {
                        byte[] buffer = new byte[IOConstants.WriteBufferMaxSize];
                        int readLen = this.InnerInputStream.ReadMetaDataPart1(buffer, 0, buffer.Length);
                        if (readLen == -1)
                        {
                            decryptionFilteringBox.InputEnd();
                        }
                        else
                        {
                            decryptionFilteringBox.Input(buffer, 0, readLen);
                        }
                        outputLen = decryptionFilteringBox.ReceiveOutput(data, offset, count);
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
                return this.InnerInputStream.ReadMetaDataPart1(data, offset, count);
            }
        }

        public override int ReadContent(byte[] data, int offset, int count)
        {
            if (IsNeedMediaDecrypt(false))
            {
                var decryptionFilteringBox = GetDecryptionFilteringBox(GetBackupJobId(true));
                if (this.readingState == ReadingState.MetaDataPart1 || (this.readingState == ReadingState.NewItem && CurrentItemIndex.IsRestoreToFS))
                {
                    decryptionFilteringBox.InputBegin();
                    this.readingState = ReadingState.ContentData;
                }
                if (this.readingState == ReadingState.ContentData)
                {
                    int outputLen = decryptionFilteringBox.ReceiveOutput(data, offset, count);
                    if (outputLen != 0) return outputLen;
                    while (true)
                    {
                        byte[] buffer = new byte[IOConstants.WriteBufferMaxSize];
                        int readLen = this.InnerInputStream.ReadContent(buffer, 0, buffer.Length);
                        if (readLen == -1)
                        {
                            decryptionFilteringBox.InputEnd();
                        }
                        else
                        {
                            decryptionFilteringBox.Input(buffer, 0, readLen);
                        }
                        outputLen = decryptionFilteringBox.ReceiveOutput(data, offset, count);
                        if (outputLen == 0) continue;
                        return outputLen;
                    }
                }
                else
                {
                    throw new System.NotSupportedException(string.Format($"CompressedFormated InputStream ReadMetaData Part1 Exception{readingState}"));
                }
            }
            else
            {
                return this.InnerInputStream.ReadContent(data, offset, count);
            }
        }

        public override int ReadMetaDataPart2(byte[] data, int offset, int count)
        {
            if (IsNeedMediaDecrypt(false))
            {
                var decryptionFilteringBox = GetDecryptionFilteringBox(GetBackupJobId(false));
                if (this.readingState == ReadingState.ContentData)
                {
                    decryptionFilteringBox.InputBegin();
                    this.readingState = ReadingState.MetaDataPart2;
                }
                if (this.readingState == ReadingState.MetaDataPart2)
                {
                    int outputLen = decryptionFilteringBox.ReceiveOutput(data, offset, count);
                    if (outputLen != 0) return outputLen;
                    while (true)
                    {
                        byte[] buffer = new byte[IOConstants.WriteBufferMaxSize];
                        int readLen = this.InnerInputStream.ReadMetaDataPart2(buffer, 0, buffer.Length);
                        if (readLen == -1)
                        {
                            decryptionFilteringBox.InputEnd();
                        }
                        else
                        {
                            decryptionFilteringBox.Input(buffer, 0, readLen);
                        }
                        outputLen = decryptionFilteringBox.ReceiveOutput(data, offset, count);
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

        private IDataFilteringBox GetDecryptionFilteringBox(string backupJobId)
        {
            if (!decryptionFilteringBoxes.TryGetValue(backupJobId, out var decryptionFilteringBox))
            {
                throw new Exception(string.Format("One job decrypt key is not found. JobId: {0}. ", backupJobId));
            }
            return decryptionFilteringBox;
        }

        private DataEncryptionInfo GetDataEncryptionInfo(bool forItemContent, string backupJobId)
        {
            var encryptionInfo = new DataEncryptionInfo();
            if (IsNeedAgentOrMediaDecrypt(forItemContent))
            {
                if (!encryptionInfos.TryGetValue(backupJobId, out encryptionInfo))
                {
                    if(this.encryptionInfoGetter != null)
                    {
                        encryptionInfo = this.encryptionInfoGetter(backupJobId);
                    }

                    if(encryptionInfo == null)
                    {
                        encryptionInfo = DataEncryptionInfoManager.StaticBlowfishEncryptionInfo;
                    }

                    encryptionInfos[backupJobId] = encryptionInfo;
                }
            }

            return encryptionInfo;
        }

        public override String NextItem(IndexBase itemIndex)
        {
            this.InnerInputStream.NextItem(itemIndex);
            this.readingState = ReadingState.NewItem;
            var encryptionInfo = GetDataEncryptionInfo(true, itemIndex.BackupJobId);

            return SerializerHelper.SerializeToBase64StringByDataContractSerializer(encryptionInfo);
        }

        public override void SetEncryptionInfos(Dictionary<string, DataEncryptionInfo> encryptionInfos, Func<string, DataEncryptionInfo> dataEncryptInfoGetter = null)
        {
            this.encryptionInfoGetter = dataEncryptInfoGetter;
            this.encryptionInfos = encryptionInfos;
            decryptionFilteringBoxes = new Dictionary<string, IDataFilteringBox>();
            foreach (var item in encryptionInfos)
            {
                var backupJobId = item.Key;
                var encryptionInfo = item.Value;
                DataEncryptionInfoWrapper wrapper = DataEncryptionInfoManager.ResolveDynamicKey(encryptionInfo);
                var decryptionFilteringBox = DataFilteringBoxFactory.GetDecryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey);
                decryptionFilteringBoxes[backupJobId] = decryptionFilteringBox;
            }
        }
    }
}