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
    using System.Text;
    using AvePoint.GCommon.Contract.CodeReview;
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

    internal class AutonomyExportService
        : ExportServiceBase
    {
        IAutonomyCompatibleForamatGenerator generator = new AutonomyCompatibleForamatGenerator();

        protected override ExportDataFileResult ExportDataFile(DataReadAction read, ExportInfo exportInfo, long length)
        {
            var result = new ExportDataFileResult();
            var buffer = new Byte[64 * 1024];
            var exportDataFileStorageInfo = XConvert.FromNames(exportInfo.FolderName, exportInfo.FileName);
            exportDataFileStorageInfo.Length = length;
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

            result.FileName = exportInfo.FileName;
            return result;
        }

        protected override ExportMetaDataFileResult ExportMetaDataFile(
            MetaData metaData,
            ExportInfo exportInfo)
        {
            var result = new ExportMetaDataFileResult();
            var highName = exportInfo == null ?
                "{0}{1}".FormatWith("Autonomy", this.ExportServiceInfo.JobId) : exportInfo.FolderName;
            var lowName = exportInfo == null ?
                "{0}{1}".FormatWith(metaData.ExportPath, "MetaData.idx") : exportInfo.FileName;
            var exportMetaDataFileStorageInfo = XConvert.FromNames(highName, lowName);
            using (var stream = this.ExportDevice.OpenStream(exportMetaDataFileStorageInfo, FileMode.OpenOrCreate))
            {
                var idxData = this.generator.Generate(metaData);
                var dataInByteArray = Encoding.UTF8.GetBytes(idxData);
                result.FileSize = dataInByteArray.Length;
                stream.Write(dataInByteArray, 0, dataInByteArray.Length);
                stream.Commit();
                stream.Close();
            }
            result.MetaData = metaData;
            return result;
        }
    }
}