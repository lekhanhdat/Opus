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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Media.StorageService;
using RAExportCommon;

namespace RAGoogle
{
    public class GoogleExportUtil : IGoogleExportUtil
    {
        private IExportService _exportService;
        private IStorageManager _storageManager;
        private string _jobId;

        public GoogleExportUtil(PhysicalDeviceDto physicalDevice, string jobId, ExportFormat format)
        {
            _exportService = new ExportServiceProvider().Create(format);

            ExportServiceInfo serviceInfo = new ExportServiceInfo
            {
                ExportDevice = physicalDevice,
                ExportType = format,
                JobId = jobId
            };
            _exportService.Open(serviceInfo);

            _storageManager = new StorageManager();
            _storageManager.Open(new StorageManagerInfo { PhysicalDevice = physicalDevice });
            _jobId = jobId;
        }

        public void Dispose()
        {
            _exportService?.Close();
        }

        public ExportResultInfo ExportContent(ExportInfo contentInfo, GoogleExportInfo info, Stream content)
        {
            if (content == null)
            {
                return new ExportResultInfo()
                {
                    Size = 0
                };
            }

            contentInfo.FileName = info.ContentFilePath;
            contentInfo.FolderName = info.FolderPath;

            ExportDataFileResult result;
            if (info is { EncryptionKey: not null, EncryptionIV: not null })
            {
                string encryptedFilePath = ExchangeUtils.CreateEncryptedFile2Local(content, _jobId, info.EncryptionKey, info.EncryptionIV);
                using (var encryptedStream = new FileStream(encryptedFilePath, FileMode.Open, FileAccess.Read))
                {
                    result = _exportService.Export(encryptedStream, contentInfo);
                }
                File.Delete(encryptedFilePath);
            }
            else
            {
                result = _exportService.Export(content, contentInfo);
            }

            return new ExportResultInfo()
            {
                Size = result.FileSize,
                Path = result.FileName,
                HashValue = $"SHA256:{result.HashValue}"
            };
        }
    }

}
