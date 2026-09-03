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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using Storage;

    #endregion

    #region CodeReview

    [AveCodeReview(
    "2012/3/21",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
    "ADO-28249",
    false)]
    #endregion

    public class EndUserArchiverDownloadService
        : DownloadServiceBase<EndUserDownloadInfo, EndUserDownloadResult>
        , IDownloadService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXSystem indexLogicalDevice;
        ArchiverRestoreJob archiverRestoreJob;
        Dictionary<String, Int32> nameDictionary = new Dictionary<String, Int32>();

        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }
        public IEndUserDownloadIndexService DownloadIndexService { get; set; }
        public IDataReader<ArchiverRestoreJob> DataReader { get; set; }
        public IEncryptionInfoManager EncryptionInfoManager { get; set; }

        public override void Open(EndUserDownloadInfo downloadInfo)
        {
            this.indexLogicalDevice = this.StorageDeviceManager.Open(downloadInfo.IndexDevice.ToXRIS());
            this.IndexService.Open(new ArchiverIndexServiceOpenParameter(downloadInfo, this.indexLogicalDevice));
            this.archiverRestoreJob = new ArchiverRestoreJob { LogicalDevice = downloadInfo.DataDevice, DataVolume = downloadInfo.DataVolume };
            this.DataReader.Open(this.archiverRestoreJob);
            var encryptionInfoDic = this.EncryptionInfoManager.PutEncryptionInfos(downloadInfo.RestoreSecurityInfos);
            DataReader.SetEncryptionInfos(encryptionInfoDic);
        }

        public override EndUserDownloadResult Download(EndUserDownloadInfo downloadInfo)
        {
            var result = new EndUserDownloadResult();
            var filePathList = new List<String>();
            var buffer = new Byte[64 * 1024];
            //用来区分不同download job的文件存放路径，以免混淆
            var downloadId = Guid.NewGuid().ToString();
            var downloadPath = Path.Combine(MediaEnvironment.MediaServer.MediaServiceAppliactionTempDirectoryPath, downloadId);
            Directory.CreateDirectory(downloadPath);
            result.DownloadId = downloadId;
            result.DownloadPath = downloadPath;
            foreach (String pathMD5 in downloadInfo.PathMD5List)
            {
                this.logger.Info(MediaServiceArchiverBackupResource.EndUserArchiverDownloadServiceDownloadPathMD5, pathMD5);
                var index = this.DownloadIndexService.GetCurrentIndex(pathMD5);
                index.IsRestoreToFS = true;
                this.DataReader.GetNextItem(index);
                if (DataReader.Input.HasContent)
                {
                    var fileName = this.GenerateFileName(index);
                    var filePath = Path.Combine(downloadPath, fileName);
                    filePathList.Add(filePath);
                    logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceWriteDataToFileWrite, fileName, filePath);
                    using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
                    {
                        DataReader.Input.BeginRead(FileType.Content);
                        while (true)
                        {
                            Int32 len = DataReader.Input.ReadContent(buffer, 0, buffer.Length);
                            if (len <= 0) break;
                            stream.Write(buffer, 0, len);
                        }
                        DataReader.Input.EndRead(FileType.Content);
                        stream.Flush();
                    }
                }
            }
            result.FilePathList = filePathList;
            return result;
        }

        public override void ProcessException(Exception e)
        {
            e = e.InnerException ?? e;
            this.logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupBrowserServiceProcessExceptionError, e.ToString());
        }

        public override void Dispose()
        {
            if (this.IndexService != null)
                this.IndexService.Close();
            this.StorageDeviceManager.Close(this.indexLogicalDevice);
            this.DataReader.Close();
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupBrowserServiceDisposeEnd);
        }

        String GenerateFileName(ArchiverBasicIndex index)
        {
            var fileName = index.Name;
            var flag = index.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
            if (index.Type.EqualsIgnoreCase("A"))
                fileName = index.Name.Substring(flag + 1);
            else if (index.Type.EqualsIgnoreCase("D") || index.Type.EqualsIgnoreCase("V"))
            {
                var name = index.ItemName.Remove(index.ItemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
                var extension = index.ItemName.Substring(index.ItemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
                fileName = flag > 0 ? name + '_' + index.Name.Substring(flag + 1) + extension : index.Name;
            }
            if (!nameDictionary.ContainsKey(fileName))
                nameDictionary.Add(fileName, 0);
            else
            {
                this.nameDictionary[fileName] = this.nameDictionary[fileName] + 1;
                var name = fileName.Remove(fileName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
                var extension = fileName.Substring(fileName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
                fileName = name + '_' + this.nameDictionary[fileName] + extension;
            }
            return fileName;
        }
    }
}