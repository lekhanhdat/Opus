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



namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives

    using System;
    using System.IO;
    using AvePoint.GCommon.Contract.CodeReview;
    using Storage;

    #endregion using directives

    #region CodeReview

    [AveCodeReview(
    "2012/4/11",
    "dwxue@avepoint.com",
    "xiaofeiwang@avepoint.com",
    new string[] { },
    null,
    true)]

    #endregion CodeReview

    public class HoldFileExporter
        : IHoldFileExporter
    {
        IXSystem holdDevice;
        IMetaDataAnalyzerFactory metaDataAnalyzerFactory;

        public void Open(HoldFileExportServiceInfo exportServiceInfo)
        {
            this.holdDevice = XFactory.InstanceLibrary(exportServiceInfo.HoldDevice.ToXRIS());
            this.holdDevice.Open();
            this.metaDataAnalyzerFactory = new MetaDataAnalyzerFactory();
        }

        public ExportResult Export(ExportFileInfo fileInfo)
        {
            var result = new ExportResult(new ExportDataFileResult(), new ExportMetaDataFileResult());
            var analyzer = this.metaDataAnalyzerFactory.CreateAnalyzer(fileInfo.MetaDataFormat);
            var datafileStorageInfo = XConvert.FromNames(fileInfo.ContentDataStorageInfo.HighName, fileInfo.ContentDataStorageInfo.LowName);
            var metaDataFileStorageInfo = XConvert.FromNames(fileInfo.MetaDataStorageInfo.HighName, fileInfo.MetaDataStorageInfo.LowName);
            XStream datafileStream = this.holdDevice.OpenStream(datafileStorageInfo, FileMode.Open);
            result.ExprotDataFileResult.DataFileStream = datafileStream;
            using (Stream metaDataFileStream = this.holdDevice.OpenStream(metaDataFileStorageInfo, FileMode.Open))
            {
                var metaDataByteArray = this.GetMetaData(metaDataFileStream);
                var metaData = analyzer.Analyze(metaDataByteArray);
                result.ExportMetaDataFileResult.MetaData = metaData;
                //result = this.exportService.Export(datafileStream, metaData);
            }

            return result;
        }

        private Byte[] GetMetaData(Stream metaDataFileStream)
        {
            var buffer = new Byte[64 * 1024];
            var metaData = default(Byte[]);
            using (var metaDataBufferStream = new MemoryStream())
            {
                while (true)
                {
                    var readLen = metaDataFileStream.Read(buffer, 0, buffer.Length);
                    if (readLen <= 0) break;
                    metaDataBufferStream.Write(buffer, 0, readLen);
                }
                metaData = metaDataBufferStream.ToArray();
            }
            return metaData;
        }

        public void Close()
        {
            this.holdDevice.Close();
        }
    }
}