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
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage;
    using AvePoint.Media.Storage.Util;

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

    internal class ConcordanceExportService
        : ExportServiceBase
    {
        IConcordanceCompatibleFormatGenerator formatGenerator = ConcordanceCompatibleFormatGeneratorFactory.Create();
        List<String> cachedData = new List<String>();

        Boolean isMetaDataStreamInit;
        XStream metaDataStream;

        protected override ExportDataFileResult ExportDataFile(
            DataReadAction read,
            MetaData metaData,
            ExportInfo exportInfo)
        {
            var result = new ExportDataFileResult();
            var buffer = new Byte[64 * 1024];
            var exportDataFileStorageInfo = XConvert.FromNames(exportInfo.FolderName, metaData.FileName);
            using (var exportDataFileStream = this.ExportDevice.OpenStream(exportDataFileStorageInfo, FileMode.OpenOrCreate))
            {
                while (true)
                {
                    var readLen = read(buffer, 0, buffer.Length);
                    if (readLen <= 0) break;
                    exportDataFileStream.Write(buffer, 0, readLen);
                }
                result.FileSize = exportDataFileStream.Length;
                exportDataFileStream.Commit();
                exportDataFileStream.Close();
            }
            result.FileName = metaData.FileName;
            metaData.ExportPath = Path.Combine(exportInfo.FolderName, metaData.FileName);
            return result;
        }

        /// <summary>
        /// As a matter of fact, The concordance dat file format is Unicode 16- little-endian, The format of the file
        /// is use a BOM to identify the file, you can see the section of BOM details in the following uri
        /// <seealso cref="http://www.unicode.org/faq/utf_bom.html"/>
        /// <remarks>
        /// ZERO WIDTH NON-BREAKING SPACE (ZWNBSP)
        /// </remarks>
        /// </summary>
        /// <param name="metaData">meta data info of the item</param>
        private void InitMetaDataStream(MetaData metaData)
        {
            var exportMetaDataFileStorageInfo = XConvert.FromNames(this.ExportServiceInfo.JobId, "MetaData.dat");
            if (!this.ExportDevice.FileExists(exportMetaDataFileStorageInfo))
            {
                this.GenerateCpl();
                this.metaDataStream = this.ExportDevice.OpenStream(exportMetaDataFileStorageInfo, FileMode.OpenOrCreate);
                this.metaDataStream.Write(new Byte[] { 0xFF, 0xFE }, 0, 2);

                var headLine = this.formatGenerator.GenerateHeaderLine(metaData);
                var headLineByteDataArray = Encoding.Unicode.GetBytes(headLine);
                this.metaDataStream.Write(headLineByteDataArray, 0, headLineByteDataArray.Length);
            }
            else
            {
                this.metaDataStream = this.ExportDevice.OpenStream(exportMetaDataFileStorageInfo, FileMode.OpenOrCreate);
                this.metaDataStream.Position = this.metaDataStream.CanSeek ? this.metaDataStream.Length : this.metaDataStream.Position;
            }
            this.isMetaDataStreamInit = true;
        }

        protected override ExportMetaDataFileResult ExportMetaDataFile(
            MetaData metaData,
            ExportInfo exportInfo)
        {
            var result = new ExportMetaDataFileResult();
            if (!this.isMetaDataStreamInit)
            {
                this.InitMetaDataStream(metaData);
            }
            var dataLine = this.formatGenerator.GenerateDataLine(metaData);
            this.Write(dataLine);
            result.MetaData = metaData;
            return result;
        }

        protected override void InternalClose()
        {
            this.InternalWrite();
            this.cachedData.Clear();
            if (this.metaDataStream != null)
            {
                this.metaDataStream.Close();
                this.metaDataStream = null;
            }
        }

        protected override void OnClosed(ExportEventArgs eventArgs)
        {
            base.OnClosed(eventArgs);
        }

        private void GenerateCpl()
        {
            CplGenerateService cplService = new CplGenerateService();
            cplService.Generate(this.ExportDevice, this.ExportServiceInfo);
        }

        private void InternalWrite()
        {
            if (this.metaDataStream != null)
            {
                this.cachedData.ForEach(dataLine =>
                {
                    var data = Encoding.Unicode.GetBytes(dataLine);
                    this.metaDataStream.Write(data, 0, data.Length);
                    this.metaDataStream.Commit();
                });
            }
        }

        private void Write(String dataLine)
        {
            if (this.cachedData.Count > 100)
            {
                this.InternalWrite();
                this.cachedData.Clear();
            }
            this.cachedData.Add(dataLine);
        }
    }
}