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




namespace AvePoint.Media.Core.IO.Output
{
    #region using directives
    using System.Text;
    using System.Reflection;
    using AvePoint.GCommon;

    using AvePoint.GCommon.Utility.FilteringBox;
    using AvePoint.Media.Core.IO;
    using Merged18NResources.MediaCoreIO;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    #endregion

    public class EncryptedFormatedOutputStream : FilterGeneralOutputStream
    {
        IDataFilteringBox encryptionFilteringBox;
        WritingState writingState = WritingState.Unknwon;
        byte[] innerBuffer = new byte[64 * 1024];

        public EncryptedFormatedOutputStream(IGeneralOutputStream innerOutput, DataEncryptionInfo encryptionInfo)
            : base(innerOutput)
        {
            this.encryptionFilteringBox = EncryptedFormatedOutputStreamUtility.EncryptedFormatedOutputStreamInit(encryptionInfo);
        }

        public override void Open()
        {
            this.writingState = WritingState.Open;
            this.InnerOutputStream.Open();
        }

        public override void WriteHeaderXml(string headerXml)
        {
            this.writingState = WritingState.HeaderXml;
            this.InnerOutputStream.WriteHeaderXml(EncryptedString(headerXml));
        }

        private string EncryptedString(string value)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                var bytesValue = Encoding.UTF8.GetBytes(value);
                encryptionFilteringBox.InputBegin();
                int count = bytesValue.Length;
                int offset = 0;
                int readLen = 0;
                while (count > IOConstants.WriteBufferMaxSize)
                {
                    encryptionFilteringBox.Input(bytesValue, offset, IOConstants.WriteBufferMaxSize);
                    offset += IOConstants.WriteBufferMaxSize;
                    count -= IOConstants.WriteBufferMaxSize;
                    while (true)
                    {
                        readLen = encryptionFilteringBox.ReceiveOutput(innerBuffer, 0, innerBuffer.Length);
                        if (readLen == 0) break;
                        stream.Write(innerBuffer, 0, readLen);
                    }
                }
                if (count > 0)
                {
                    encryptionFilteringBox.Input(bytesValue, offset, count);
                }
                encryptionFilteringBox.InputEnd();


                while (true)
                {
                    readLen = encryptionFilteringBox.ReceiveOutput(innerBuffer, 0, innerBuffer.Length);
                    if (readLen == -1) break;
                    stream.Write(innerBuffer, 0, readLen);
                }
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public override void WriteMetaData(byte[] data, int offset, int count)
        {
            while (count > IOConstants.WriteBufferMaxSize)
            {
                WriteMetaDataInternal(data, offset, IOConstants.WriteBufferMaxSize);
                offset += IOConstants.WriteBufferMaxSize;
                count -= IOConstants.WriteBufferMaxSize;
            }
            if (count > 0)
            {
                WriteMetaDataInternal(data, offset, count);
            }
        }

        private void WriteMetaDataInternal(byte[] data, int offset, int count)
        {
            if (writingState == WritingState.HeaderXml)
            {
                BeginEncrypt(true);
                writingState = WritingState.MetaDataPart1;
            }
            if (writingState == WritingState.ContentData)
            {
                EndEncrypt(false);
                BeginEncrypt(true);
                writingState = WritingState.MetaDataPart2;
            }

            if (writingState == WritingState.MetaDataPart1
                || writingState == WritingState.MetaDataPart2)
            {
                Encrypting(data, offset, count, true);
            }
            else
            {
                throw new System.NotSupportedException(string.Format(MediaCoreIOResource.EncryptedFormatedOutputStreamWriteMetaDataInternalException, writingState.ToString()));
            }
        }

        public override void WriteContentData(byte[] data, int offset, int count)
        {
            while (count > IOConstants.WriteBufferMaxSize)
            {
                WriteContentDataInternal(data, offset, IOConstants.WriteBufferMaxSize);
                offset += IOConstants.WriteBufferMaxSize;
                count -= IOConstants.WriteBufferMaxSize;
            }
            if (count > 0)
            {
                WriteContentDataInternal(data, offset, count);
            }
        }

        private void WriteContentDataInternal(byte[] data, int offset, int count)
        {
            if (writingState == WritingState.HeaderXml)
            {
                BeginEncrypt(false);
                writingState = WritingState.ContentData;
            }
            if (writingState == WritingState.MetaDataPart1)
            {
                EndEncrypt(true);
                BeginEncrypt(false);
                writingState = WritingState.ContentData;
            }
            if (writingState == WritingState.ContentData)
            {
                Encrypting(data, offset, count, false);
            }
            else
            {
                throw new System.NotSupportedException(string.Format(MediaCoreIOResource.EncryptedFormatedOutputStreamWriteMetaDataInternalException, writingState.ToString()));
            }
        }

        public override void EndItem(IndexBase basicIndex)
        {
            if (writingState == WritingState.MetaDataPart1
                || writingState == WritingState.MetaDataPart2)
            {
                EndEncrypt(true);
            }
            else if (writingState == WritingState.ContentData)
            {
                EndEncrypt(false);
            }
            this.InnerOutputStream.EndItem(basicIndex);
        }

        public override void WriteTailXml(string tailXml)
        {
            this.InnerOutputStream.WriteTailXml(EncryptedString(tailXml));
        }

        public override StorageResult Close()
        {
            return this.InnerOutputStream.Close();
        }

        private void BeginEncrypt(bool isMetaData)
        {
            encryptionFilteringBox.InputBegin();
        }

        private void Encrypting(byte[] buffer, int offset, int count, bool isMetaData)
        {
            encryptionFilteringBox.Input(buffer, offset, count);

            //byte[] output = new byte[64 * 1024]; //move this to local variable to reduce the allocate memory
            int readLen = 0;
            while (true)
            {
                readLen = encryptionFilteringBox.ReceiveOutput(innerBuffer, 0, innerBuffer.Length);
                if (readLen == 0) break;
                if (isMetaData)
                {
                    this.InnerOutputStream.WriteMetaData(innerBuffer, 0, readLen);
                }
                else
                {
                    this.InnerOutputStream.WriteContentData(innerBuffer, 0, readLen);
                }
            }
        }

        private void EndEncrypt(bool isMetaData)
        {
            encryptionFilteringBox.InputEnd();

            //byte[] output = new byte[64 * 1024];
            int readLen = 0;
            while (true)
            {
                readLen = encryptionFilteringBox.ReceiveOutput(innerBuffer, 0, innerBuffer.Length);
                if (readLen == -1) break;
                if (isMetaData)
                {
                    this.InnerOutputStream.WriteMetaData(innerBuffer, 0, readLen);
                }
                else
                {
                    this.InnerOutputStream.WriteContentData(innerBuffer, 0, readLen);
                }
            }
        }
    }
}
