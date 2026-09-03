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
    using AvePoint.GCommon.Network;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    #endregion

    public abstract class FilterGeneralOutputStream : IGeneralOutputStream
    {
        protected IGeneralOutputStream InnerOutputStream { get; private set; }

        public FilterGeneralOutputStream(IGeneralOutputStream innerOutput)
        {
            this.InnerOutputStream = innerOutput;
        }

        public abstract void Open();
        public void BeforeItem(IndexBase basicIndex)
        {
            this.InnerOutputStream.BeforeItem(basicIndex);
        }

        public void Write(AveDataBlock dataBlock)
        {
            switch (dataBlock.Type)
            {
                case AveDataBlockType.HEADER_TYPE:
                    string headerXml = Encoding.UTF8.GetString(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                    WriteHeaderXml(headerXml);
                    break;
                case AveDataBlockType.DATA_TYPE:
                    WriteMetaData(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                    break;
                case AveDataBlockType.CONTENTDATA_TYPE:
                    WriteContentData(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                    break;
                case AveDataBlockType.TAIL_TYPE:
                    string tailXml = Encoding.UTF8.GetString(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                    WriteTailXml(tailXml);
                    break;
                default:
                    throw new System.NotSupportedException($"FormatedOutputStream Write Exception:{dataBlock.Type}");
            }
        }

        public abstract StorageResult Close();

        public abstract void WriteHeaderXml(string headerXml);
        public abstract void WriteMetaData(byte[] data, int offset, int count);
        public abstract void WriteContentData(byte[] data, int offset, int count);
        public abstract void WriteTailXml(string tailXml);
        public abstract void EndItem(IndexBase basicIndex);
    }
}
