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
using AvePoint.Media.Storage;
using AvePoint.Media.Storage.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AvePoint.GCommon.Media.StorageService
{
    internal class NAAExportService : ExportServiceBase
    {
        String path = String.Empty;
        XStream metaDataStream;
        IConcordanceCompatibleFormatGenerator formatGenerator = ConcordanceCompatibleFormatGeneratorFactory.Create(ExportFormat.Csv);
        protected override ExportDataFileResult ExportDataFile(DataReadAction read, ExportInfo exportInfo)
        {
            var result = new ExportDataFileResult();
            var buffer = new Byte[64 * 1024];
            var exportDataFileStorageInfo = XConvert.FromNames(exportInfo.FolderName, exportInfo.FileName);
            exportDataFileStorageInfo.FileAccess = FileAccess.ReadWrite;
            using (var exportDataFileStream = this.ExportDevice.OpenStream(exportDataFileStorageInfo, FileMode.OpenOrCreate))
            {
                while (true)
                {
                    Int32 readLen;
                    try
                    {
                        readLen = read(buffer, 0, buffer.Length);
                    }
                    catch (Exception ex)
                    {
                        throw new ExportReadException(ex.Message, ex);
                    }
                    if (readLen <= 0) break;
                    exportDataFileStream.Write(buffer, 0, readLen);
                }
                result.FileSize = exportDataFileStream.Length;
                result.DataFileStream = exportDataFileStream;
                exportDataFileStream.Position = 0;
                result.HashValue = exportDataFileStream.ToSha256HashCode();
                exportDataFileStream.Commit();
                exportDataFileStream.Close();
            }
            //keep datetime from sp
            XFileInfo info = this.ExportDevice.OpenFile(exportDataFileStorageInfo);
            info.CreationTime = exportInfo.Created;
            info.LastWriteTime = exportInfo.Modified;
            result.FileName = exportInfo.FileName;
            return result;
        }
        protected override ExportDataFileResult ExportDataFile(
            DataReadAction read,
            MetaData metaData,
            ExportInfo exportInfo)
        {
            var result = new ExportDataFileResult();
            var buffer = new Byte[64 * 1024];
            var exportDataFileStorageInfo = XConvert.FromNames(exportInfo.FolderName, metaData.FileName);
            exportDataFileStorageInfo.FileAccess = FileAccess.ReadWrite;
            using (var exportDataFileStream = this.ExportDevice.OpenStream(exportDataFileStorageInfo, FileMode.OpenOrCreate))
            {
                while (true)
                {
                    Int32 readLen;
                    try
                    {
                        readLen = read(buffer, 0, buffer.Length);
                    }
                    catch (Exception ex)
                    {
                        throw new ExportReadException(ex.Message, ex);
                    }
                    if (readLen <= 0) break;
                    exportDataFileStream.Write(buffer, 0, readLen);
                }
                result.FileSize = exportDataFileStream.Length;
                result.DataFileStream = exportDataFileStream;
                exportDataFileStream.Position = 0;
                result.HashValue = exportDataFileStream.ToSha256HashCode();
                exportDataFileStream.Commit();
                exportDataFileStream.Close();
            }
            //keep datetime from sp
            XFileInfo info = this.ExportDevice.OpenFile(exportDataFileStorageInfo);
            info.CreationTime = exportInfo.Created;
            info.LastWriteTime = exportInfo.Modified;
            result.FileName = metaData.FileName;
            metaData.ExportPath = Path.Combine(exportInfo.FolderName, metaData.FileName);
            return result;
        }

        protected override ExportMetaDataFileResult ExportMetaDataFile(
           MetaData metaData,
           ExportInfo exportInfo)
        {
            if (!this.path.EqualsIgnoreCase(Path.Combine(exportInfo.FolderName, exportInfo.FileName)))
            {
                this.InitMetaDataStream(metaData, exportInfo);
                this.path = Path.Combine(exportInfo.FolderName, exportInfo.FileName);
            }
            var dataLine = this.formatGenerator.GenerateDataLine(metaData);
            this.Write(dataLine);
            metaDataStream.Position = 0;
            var result = new ExportMetaDataFileResult { MetaData = metaData, MetaDataStream = this.metaDataStream, HashValue = this.metaDataStream.ToSha256HashCode(), FileSize = this.metaDataStream.Length };
            return result;
        }

        private void InitMetaDataStream(MetaData metaData, ExportInfo exportInfo)
        {
            this.InternalClose();
            var exportMetaDataFileStorageInfo = XConvert.FromNames(exportInfo.FolderName, exportInfo.FileName);
            exportMetaDataFileStorageInfo.FileAccess = FileAccess.ReadWrite;
            if (!this.ExportDevice.FileExists(exportMetaDataFileStorageInfo))
            {
                this.metaDataStream = this.ExportDevice.OpenStream(exportMetaDataFileStorageInfo, FileMode.OpenOrCreate);
                this.metaDataStream.Write(new Byte[] { 0xEF, 0xBB, 0xBF }, 0, 3);
                var headLine = this.formatGenerator.GenerateHeaderLine(metaData);
                var headLineByteDataArray = Encoding.UTF8.GetBytes(headLine);
                this.metaDataStream.Write(headLineByteDataArray, 0, headLineByteDataArray.Length);
            }
            else
            {
                this.metaDataStream = this.ExportDevice.OpenStream(exportMetaDataFileStorageInfo, FileMode.OpenOrCreate);
                this.metaDataStream.Position = this.metaDataStream.CanSeek ? this.metaDataStream.Length : this.metaDataStream.Position;
            }
        }

        private void Write(String dataLine)
        {
            if (this.metaDataStream != null)
            {
                var data = Encoding.UTF8.GetBytes(dataLine);
                this.metaDataStream.Write(data, 0, data.Length);
                this.metaDataStream.Commit();
            }
        }

        protected override void InternalClose()
        {
            if (this.metaDataStream != null)
            {
                this.metaDataStream.Close();
                this.metaDataStream = null;
            }
        }

        protected override string GetFileName(string fileName)
        {
            return fileName;
        }
    }
}
