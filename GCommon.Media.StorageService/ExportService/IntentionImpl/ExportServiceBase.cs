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


using AvePoint.ObjectModel.ClientOM;
using AvePoint.Wrapper.Common;

namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives

    using System;
    using System.Collections.Generic;
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

    /// <summary>
    ///   using the interface to rewrite the implementation of
    ///   export service and export service event
    /// </summary>
    internal abstract partial class ExportServiceBase
        : IExportService
    {
        ExportServiceInfo exportServiceInfo;
        IXSystem exportDevice;

        protected Dictionary<String, Int32> fileDictionary = new Dictionary<String, Int32>();

        public IXSystem ExportDevice { get { return this.exportDevice; } }

        public ExportServiceInfo ExportServiceInfo { get { return this.exportServiceInfo; } }

        public AveClientOM2013Request SharePointExportService { get; set; }

        public void Open(ExportServiceInfo exportServiceInfo)
        {
            this.FireEvent(ExportEventType.Opening);
            this.exportServiceInfo = exportServiceInfo;
            this.exportDevice = XFactory.InstanceSystem(this.exportServiceInfo.ExportDevice.BuildXRI());
            this.exportDevice.Open();
            this.FireEvent(ExportEventType.Opened);
        }
        
        public void Open(string siteUrl, AveBPOSAccountInfo user)
        {
            SharePointExportService = new AveClientOM2013Request(siteUrl, user, true);
        }

        public ExportResult Export(Stream dataStream, MetaData metaData, ExportInfo exportInfo)
        {
            return this.Export(dataStream.Read, metaData, exportInfo);
        }

        public ExportResult Export(IDataReader dataReader, MetaData metaData, ExportInfo exportInfo)
        {
            return this.Export(dataReader.Read, metaData, exportInfo);
        }

        public ExportResult Export(DataReadAction dataReadAction, MetaData metaData, ExportInfo exportInfo)
        {
            this.FireEvent(ExportEventType.Exporting);
            var exportResult = default(ExportResult);
            metaData.FileName = this.GetFileName(metaData.FileName);
            var exprotDataFileResult = this.ExportDataFile(dataReadAction, metaData, exportInfo);
            var exportMetaDataFileResult = this.ExportMetaDataFile(metaData, exportInfo);
            exportResult = new ExportResult(exprotDataFileResult, exportMetaDataFileResult);
            this.FireEvent(ExportEventType.Exported);
            return exportResult;
        }

        public ExportDataFileResult Export(Stream dataStream, ExportInfo exportInfo)
        {
            //modify for RevIM export 支持veo文件格式export到azure storage，开流的时候
            //需要得到length的长度，所以在此增加参数length
            //return this.Export(dataStream.Read, exportInfo);
            return this.ExportDataFile(dataStream, exportInfo, dataStream.Length);
        }

        public ExportDataFileResult Export(IDataReader dataReader, ExportInfo exportInfo)
        {
            return this.Export(dataReader.Read, exportInfo);
        }

        public ExportDataFileResult Export(DataReadAction read, ExportInfo exportInfo)
        {
            return this.ExportDataFile(read, exportInfo);
        }
        #region add for RevIM export
        public ExportDataFileResult Export(DataReadAction read, ExportInfo exportInfo, long length)
        {
            return this.ExportDataFile(read, exportInfo, length);
        }
        #endregion
        public ExportMetaDataFileResult Export(MetaData metaData, ExportInfo exportInfo)
        {
            return this.ExportMetaDataFile(metaData, exportInfo);
        }

        public Stream GetExportStream(ExportInfo exportInfo)
        {
            var exportDataFileStorageInfo = XConvert.FromNames(exportInfo.FolderName, exportInfo.FileName);
            return this.ExportDevice.OpenStream(exportDataFileStorageInfo, FileMode.OpenOrCreate);
        }

        public void Close()
        {
            this.FireEvent(ExportEventType.Closing);
            this.fileDictionary.Clear();
            this.InternalClose();
            this.exportDevice?.Close();
            this.FireEvent(ExportEventType.Closed);
        }

        protected virtual ExportDataFileResult ExportDataFile(DataReadAction read, ExportInfo exportInfo)
        {
            this.FireEvent(ExportEventType.Exporting);
            var result = new ExportDataFileResult();
            var buffer = new Byte[64 * 1024];
            exportInfo.FileName = this.GetFileName(exportInfo.FileName);
            var exportDataFileStorageInfo = XConvert.FromNames(exportInfo.FolderName, exportInfo.FileName);
            using (var exportDataFileStream = this.ExportDevice.OpenStream(exportDataFileStorageInfo, FileMode.OpenOrCreate))
            {
                Int32 readLen;
                while (true)
                {
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
                exportDataFileStream.Commit();
                exportDataFileStream.Close();
            }

            result.FileName = Path.Combine(this.ExportDevice.SystemLocation, Path.Combine(exportInfo.FolderName, exportInfo.FileName));
            this.FireEvent(ExportEventType.Exported);
            return result;
        }
        protected virtual ExportDataFileResult ExportDataFile(DataReadAction read, MetaData metaData, ExportInfo exportInfo)
        {
            return this.Export(
                read,
                new ExportInfo
                {
                    FolderName = this.ExportServiceInfo.JobId,
                    FileName = metaData.FileName
                });
        }

        protected virtual ExportDataFileResult ExportDataFile(DataReadAction read, ExportInfo exportInfo, long length = 0)
        {
            this.FireEvent(ExportEventType.Exporting);
            var result = new ExportDataFileResult();
            var buffer = new Byte[64 * 1024];
            exportInfo.FileName = this.GetFileName(exportInfo.FileName);
            var exportDataFileStorageInfo = XConvert.FromNames(exportInfo.FolderName, exportInfo.FileName);
            exportDataFileStorageInfo.Length = length;//add for RevIM export
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

            result.FileName = Path.Combine(exportInfo.FolderName, exportInfo.FileName);
            this.FireEvent(ExportEventType.Exported);
            return result;
        }

        protected virtual ExportDataFileResult ExportDataFile(Stream stream, ExportInfo exportInfo, long length = 0)
        {
            this.FireEvent(ExportEventType.Exporting);
            var result = new ExportDataFileResult();
            var buffer = new Byte[64 * 1024];
            exportInfo.FileName = this.GetFileName(exportInfo.FileName);
            var exportDataFileStorageInfo = XConvert.FromNames(exportInfo.FolderName, exportInfo.FileName);
            exportDataFileStorageInfo.Length = length;//add for RevIM export
            var res = this.ExportDevice.UploadAsync(stream, exportDataFileStorageInfo).GetAwaiter().GetResult();
            result.FileSize = length;
            result.FileName = Path.Combine(exportInfo.FolderName, exportInfo.FileName);
            this.FireEvent(ExportEventType.Exported);
            return result;
        }

        protected virtual ExportMetaDataFileResult ExportMetaDataFile(MetaData metaData, ExportInfo exportInfo)
        {
            var result = new ExportMetaDataFileResult();
            result.MetaData = metaData;
            return result;
        }

        protected virtual void InternalClose() { }

        protected virtual String GetFileName(String fileName)
        {
            String name = fileName;
            if (!this.fileDictionary.ContainsKey(name))
            {
                this.fileDictionary.Add(name, 0);
            }
            else
            {
                this.fileDictionary[name] = this.fileDictionary[name] + 1;
                if (fileName.Contains("."))
                {
                    var prefixName = fileName.Split('.');
                    var tempName = fileName.Remove(fileName.LastIndexOf('.')) + "_" + fileDictionary[name];
                    fileName = tempName + "." + prefixName[prefixName.Length - 1];
                }
                else
                {
                    fileName = fileName + "_" + fileDictionary[name];
                }
            }
            if (this.fileDictionary.Count >= 1000)
            {
                this.fileDictionary.Clear();
            }
            return fileName;
        }
    }
}