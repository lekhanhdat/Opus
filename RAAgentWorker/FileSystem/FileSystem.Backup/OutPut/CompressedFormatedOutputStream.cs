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
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.FilteringBox;
    using AvePoint.Media.Core.IO;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    #endregion

    public class CompressedFormatedOutputStream : FilterGeneralOutputStream
    {
       // AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IDataFilteringBox compressionFilteringBox;
        WritingState writingState = WritingState.Unknwon;

        public CompressedFormatedOutputStream(IGeneralOutputStream innerOutput, CompressionMethods method, int compressionType)
            : base(innerOutput)
        {
            this.compressionFilteringBox = DataFilteringBoxFactory.GetCompressionFilteringBox(method, compressionType);
        }

        public override void Open()
        {
            this.writingState = WritingState.Open;
            this.InnerOutputStream.Open();
        }

        public override void WriteHeaderXml(string headerXml)
        {
            this.writingState = WritingState.HeaderXml;
            this.InnerOutputStream.WriteHeaderXml(headerXml);
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
                BeginCompress(true);
                writingState = WritingState.MetaDataPart1;
            }
            if (writingState == WritingState.ContentData)
            {
                EndCompress(false);
                BeginCompress(true);
                writingState = WritingState.MetaDataPart2;
            }

            if (writingState == WritingState.MetaDataPart1
                || writingState == WritingState.MetaDataPart2)
            {
                Compressing(data, offset, count, true);
            }
            else
            {
                throw new System.NotSupportedException(string.Format($"Encrypted Formated OutputStream WriteMetaData Internal Exception:{writingState}"));
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
                BeginCompress(false);
                writingState = WritingState.ContentData;
            }
            if (writingState == WritingState.MetaDataPart1)
            {
                EndCompress(true);
                BeginCompress(false);
                writingState = WritingState.ContentData;
            }
            if (writingState == WritingState.ContentData)
            {
                Compressing(data, offset, count, false);
            }
            else
            {
                throw new System.NotSupportedException(string.Format($"Encrypted Formated OutputStream WriteMetaData Internal Exception:{writingState}"));
            }
        }

        public override void EndItem(IndexBase basicIndex)
        {
            if (writingState == WritingState.MetaDataPart1
                || writingState == WritingState.MetaDataPart2)
            {
                EndCompress(true);
            }
            else if (writingState == WritingState.ContentData)
            {
                EndCompress(false);
            }
            this.InnerOutputStream.EndItem(basicIndex);
        }

        public override void WriteTailXml(string tailXml)
        {           
            this.InnerOutputStream.WriteTailXml(tailXml);
        }

        public override StorageResult Close()
        {
            return this.InnerOutputStream.Close();
        }

        private void BeginCompress(bool isMetaData)
        {
            compressionFilteringBox.InputBegin();
        }

        private void Compressing(byte[] buffer, int offset, int count, bool isMetaData)
        {
            compressionFilteringBox.Input(buffer, offset, count);

            byte[] output = new byte[64 * 1024];
            int readLen = 0;
            while (true)
            {
                readLen = compressionFilteringBox.ReceiveOutput(output, 0, output.Length);
                if (readLen == 0) break;
                if (isMetaData)
                {
                    this.InnerOutputStream.WriteMetaData(output, 0, readLen);
                }
                else
                {
                    this.InnerOutputStream.WriteContentData(output, 0, readLen);
                }
            }
        }

        private void EndCompress(bool isMetaData)
        {
            compressionFilteringBox.InputEnd();

            byte[] output = new byte[64 * 1024];
            int readLen = 0;
            while (true)
            {
                readLen = compressionFilteringBox.ReceiveOutput(output, 0, output.Length);
                if (readLen == -1) break;
                if (isMetaData)
                {
                    this.InnerOutputStream.WriteMetaData(output, 0, readLen);
                }
                else
                {
                    this.InnerOutputStream.WriteContentData(output, 0, readLen);
                }
            }
        }
    }
}
