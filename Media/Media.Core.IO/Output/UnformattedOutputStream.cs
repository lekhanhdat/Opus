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

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.PlatformRecovery;
    using AvePoint.GCommon.Contract.PlatformRecovery.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    using AvePoint.GCommon.Network;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaCoreIO;
    using AvePoint.Media.Service.DomainModel;
    using Storage;

    #endregion using directives

    public class UnformattedOutputStream : IGeneralOutputStream
    {
        //AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IOutputDataListener dataListener;
        Stream metaDataStream;
        String fileName;
        String jobId;
        String farmName;

        /// <summary>
        /// UnformattedOutputStream类目前只用于写'S'类型的文件(setting file)
        /// </summary>
        public UnformattedOutputStream(OpenOutputStreamParameterEx exPar)
        {
            this.dataListener = exPar.DataListener;
            this.fileName = exPar.FileName;
            this.metaDataStream = dataListener.ChangeDataBlock(FileType.MetaData, fileName);
            this.jobId = exPar.JobId;
            this.farmName = exPar.FarmName;
        }

        public StorageResult Close()
        {
            StorageResult result = null;
            if (metaDataStream != null)
            {
                metaDataStream.Close();
                metaDataStream = null;
            }

            return result;
        }

        public void WriteMetaData(byte[] data, int offset, int count)
        {
            metaDataStream.Write(data, offset, count);
        }

        public void Write(AveDataBlock dataBlock)
        {
            switch (dataBlock.Type)
            {
                case AveDataBlockType.DATA_TYPE:
                    WriteMetaData(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                    break;
                default:
                    throw new NotImplementedException(string.Format(MediaCoreIOResource.UnformattedOutputStreamWriteException, dataBlock.Type.ToString()));
            }
        }

        public void Open() { throw new NotImplementedException(); }

        public void BeforeItem(IndexBase basicIndex) { throw new NotImplementedException(); }

        public void EndItem(IndexBase basicIndex) { throw new NotImplementedException(); }

        public void WriteHeaderXml(string headerXml) { throw new NotImplementedException(); }

        public void WriteContentData(byte[] data, int offset, int count) { throw new NotImplementedException(); }

        public void WriteTailXml(string tailXml) { throw new NotImplementedException(); }
    }
}