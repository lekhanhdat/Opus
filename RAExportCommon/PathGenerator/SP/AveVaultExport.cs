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



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Media.StorageService;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Storage.Entity;
using LOGRESOURCE = Merged18NResources.Export;
using LOGRESOURCEInternationalization = Merged18NResources.ExportForInternationalization;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;

namespace RAExportCommon
{
    
    internal class AveVaultExport : IAveVaultExport
    {
        #region Member
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IExportService exportService = null;
        private IStorageManager mStorageManager = null;
        private byte[] mEncryptionKey = null;
        private byte[] mEncryptionIV = null;
        private string mJobId = string.Empty;
        private SharePointLocationDto _sharePointLocationDto;
        #endregion

        #region Methord

        public AveVaultExport(PhysicalDeviceDto physicalDevice, string jobId, ExportFormat format, byte[] encryptionKey, byte[] encryptionIV)
        {
            var exportServiceProvider = new ExportServiceProvider();
            exportService = exportServiceProvider.Create(format);
            ExportServiceInfo serviceinfo = new ExportServiceInfo();
            serviceinfo.ExportDevice = physicalDevice;
            serviceinfo.ExportType = format;
            serviceinfo.JobId = jobId;
            exportService.Open(serviceinfo);

            mStorageManager = new StorageManager();
            StorageManagerInfo storageInfo = new StorageManagerInfo();
            storageInfo.PhysicalDevice = physicalDevice;
            mStorageManager.Open(storageInfo);

            mEncryptionKey = encryptionKey;
            mEncryptionIV = encryptionIV;
            mJobId = jobId;
        }
        
        public AveVaultExport(SharePointLocationDto spoDto, AveBPOSAccountInfo user, string siteUrl, string jobId, ExportFormat format, byte[] encryptionKey, byte[] encryptionIV)
        {
            var exportServiceProvider = new ExportServiceProvider();
            exportService = exportServiceProvider.Create("SharePoint");
            exportService.Open(siteUrl,user);

            mEncryptionKey = encryptionKey;
            mEncryptionIV = encryptionIV;
            mJobId = jobId;
            _sharePointLocationDto = spoDto;
        }

        public ExportResultInfo ExportContent(ExportInfo contentInfo, VaultExportInfo info, Stream content)
        {
            try
            {
                AvePerformanceTimerPool.Start("ExportContent");
                ExportDataFileResult result = null;
                if (content == null || content.Length == 0)
                {
                    mLog.Warn("The content of export file is null");
                    return new ExportResultInfo() { Size = 0 };
                }
                contentInfo.FolderName = info.FolderPath;
                contentInfo.FileName = info.ContentFilePath;
                if (_sharePointLocationDto != null)
                {
                    contentInfo.ParentWebUrl = _sharePointLocationDto.ParentWebUrl;
                    contentInfo.JobFolder = _sharePointLocationDto.JobFolder;
                    contentInfo.ParentFolderId = _sharePointLocationDto.ParentFolderId;
                }
                try
                {
                    if (exportService != null)
                    {
                        if (mEncryptionKey != null && mEncryptionKey.Length > 0 && mEncryptionIV != null && mEncryptionIV.Length > 0)
                        {
                            string encryptedFilePath = string.Empty;
                            try
                            {
                                mLog.Info("Create encrypted file to local.");
                                encryptedFilePath = ExchangeUtils.CreateEncryptedFile2Local(content, mJobId, mEncryptionKey, mEncryptionIV);
                                mLog.Info($"Finish create encrypted file to local,encrypted file path:{encryptedFilePath}.");
                                using (var encryptedStream = new FileStream(encryptedFilePath, FileMode.Open, FileAccess.Read))
                                {
                                    mLog.Info("Export content file to storage.");
                                    result = exportService.Export(encryptedStream, contentInfo);
                                    mLog.Info($"Finish export content file to storage,result name:{result.FileName}.");
                                }
                            }
                            finally
                            {
                                try
                                {
                                    if (!string.IsNullOrWhiteSpace(encryptedFilePath))
                                    {
                                        FileInfo file = new FileInfo(encryptedFilePath);
                                        if (file.Exists)
                                        {
                                            file.Delete();
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn($"Error occurred while deleting temp encrypted file. Path:{encryptedFilePath} Error:{e.ToString()}");
                                }
                            }
                        }
                        else
                        {
                            result = exportService.Export(content, contentInfo);
                        }
                    }
                    else
                    {
                        mLog.Error("An error occurred while get {0} deviceDto.", info.DeviceDtoId);
                    }
                    //result = exportService.Export(content, contentInfo);
                }
                catch (ExportReadException erx)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTION, "An error occurred while read stream.", erx.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTION, LOGRESOURCE.Vault_SOVTVaultUtilityExportServiceException, e.ToString());
                    throw new ExportServiceException(e);
                }
                if (result != null)
                {
                    return new ExportResultInfo() { Size = result.FileSize, Path = result.FileName, HashValue = string.Format("SHA256:{0}", result.HashValue) };
                }
                else
                {
                    return new ExportResultInfo() { Size = 0 };
                }
            }
            catch (Exception e2)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTION, "An error occurred while Export Content.", e2.ToString());
                throw;
            }
            finally
            {
                AvePerformanceTimerPool.Stop("ExportContent");
            }
        }

        public void Dispose()
        {
            //this.RemoveEmptyDirectorys();
            if (exportService != null)
            {
                exportService.Close();
                exportService = null;
            }
        }
        #endregion
    }
}
