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

//namespace Office365GroupRestore
//{
//    using System;
//    using System.Collections.Generic;
//    using System.IO;
//    using System.Linq;
//    using Office365GroupRestore.Worker;
//    using AvePoint.Application.Common.Utils;
    
//    using WorkerServiceWrapper;
//    using AvePoint.Common;

//    public class RestoreToStorageExecutorBatch : IRestoreExecutor
//    {
//        private static readonly RALogger logger = RALogger.GetInstance(typeof(RestoreExecutorBatch));

//        private RestoreConfig config;

//        public void Execute(RestoreConfig config, IRestoreDataHandlerBase restoreDataHandlerBase)
//        {
//            var restoreDataHandler = restoreDataHandlerBase as RestoreDataHandlerBatch;
//            this.config = config;
//            PerformanceDataManager pDataManager = new PerformanceDataManager();
//            pDataManager.Start();
//            InitPath();
//            logger.Info("Restore to storage executor start.");
//            foreach (var datablockCollection in restoreDataHandler.GetDateBlockCollection())
//            {
//                logger.Info("Get datablock collection,Output Collection Count:{3}, Type: {0}, ItemCount: {1}, Size: {2}.", datablockCollection.CollectionType, datablockCollection.ItemsCount, datablockCollection.TotalSize, restoreDataHandler.GetOutputCollectionCount());
//                pDataManager.CollectPerformanceData(datablockCollection.CollectionType, datablockCollection.TotalSize, datablockCollection.ItemsCount);
//                switch (datablockCollection.CollectionType)
//                {
//                    case ExchangeDataBlockType.Finish:
//                        break;

//                    case ExchangeDataBlockType.Exception:
//                        var exceptionCollection = datablockCollection as ExceptionDataBlockCollection;
//                        throw new DataBlockException(exceptionCollection.ExceptionMessage);
//                    case ExchangeDataBlockType.Item:
//                        RestoreItem(datablockCollection);
//                        break;
//                    case ExchangeDataBlockType.Folder:
//                        RestoreFolder(datablockCollection);
//                        break;
//                    case ExchangeDataBlockType.Mailbox:
//                    default:
//                        break;
//                }
//            }
//            pDataManager.Finish();
//            CommitFIle();
//            logger.Info("Restore to storage executor finish.");
//        }

//        private void InitPath()
//        {
//            RestoreToStorageConstants.HtmlFilesParentPath = Path.Combine(config.JobDir, RestoreToStorageConstants.HtmlFolderName);
//            RestoreToStorageConstants.ZipFileParentPath = Path.Combine(config.JobDir, RestoreToStorageConstants.ZipFolderName);
//            if (!Directory.Exists(RestoreToStorageConstants.HtmlFilesParentPath)) Directory.CreateDirectory(RestoreToStorageConstants.HtmlFilesParentPath);
//            if (!Directory.Exists(RestoreToStorageConstants.ZipFileParentPath)) Directory.CreateDirectory(RestoreToStorageConstants.ZipFileParentPath);
//            RestoreToStorageConstants.HtmlFiles = new List<string>(12);
//        }
//        private void CommitFIle()
//        {
//            var succes = CompressedFiles(RestoreToStorageConstants.HtmlFilesParentPath, RestoreToStorageConstants.ZipFileParentPath);
//            logger.Info("Compressed files : {0}", succes);
//            UploadZipFile(RestoreToStorageConstants.ZipFileParentPath, succes);
//            Clear(RestoreToStorageConstants.HtmlFilesParentPath, RestoreToStorageConstants.ZipFileParentPath);
//        }
//        /// <summary>
//        /// CI-AOSBR-12915  日本客户使用 Lhaplus 解压，解压出来的文件名存在乱码；建议客户使用windows自带的解压工具后，结果正常。
//        /// </summary>
//        /// <param name="htmlFilesParentPath"></param>
//        /// <param name="zipFileParentPath"></param>
//        /// <returns></returns>
//        private Boolean CompressedFiles(String htmlFilesParentPath, String zipFileParentPath)
//        {
//            var zipFileFullName = Path.Combine(zipFileParentPath, RestoreToStorageConstants.ZipFileName);
//            try
//            {
//                ZipUtil.ZipFolder(htmlFilesParentPath, zipFileFullName, config.ZipFilePassword);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                logger.Info(ex.ToString());
//                try
//                {
//                    if (File.Exists(zipFileFullName)) File.Delete(zipFileFullName);
//                }
//                catch (Exception e)
//                {
//                    logger.Error("Delete zip file failed : {0}", e.ToString());
//                }
//            }
//            return false;
//        }
//        private void UploadZipFile(String zipFileParentPath, Boolean onlyOneZipFIle = true)
//        {
//            var exchangeRestoreToStorageService = RestoreToStorageService.GetInstance();
//            try
//            {
//                exchangeRestoreToStorageService.Open(config.JobId, config.DestStorageInfo);
//                if (onlyOneZipFIle)
//                {
//                    exchangeRestoreToStorageService.Restore(Path.Combine(zipFileParentPath, RestoreToStorageConstants.ZipFileName));
//                }
//                else
//                {
//                    foreach (var file in RestoreToStorageConstants.HtmlFiles)
//                    {
//                        exchangeRestoreToStorageService.Restore(file);
//                    }
//                }
//            }
//            finally
//            {
//                exchangeRestoreToStorageService.Close();
//                logger.Info("Start delete temp export date.");
//            }
//        }
//        private void Clear(String htmlFilesParentPath, String zipFileParentPath)
//        {
//            try
//            {
//                if (Directory.Exists(zipFileParentPath)) Directory.Delete(zipFileParentPath, true);
//            }
//            catch (Exception ex)
//            {
//                logger.Warn("Clear ZipFileParentPath failed. Reason: {0}", ex.ToString());
//            }
//            try
//            {
//                if (Directory.Exists(htmlFilesParentPath)) Directory.Delete(htmlFilesParentPath, true);
//            }
//            catch (Exception ex)
//            {
//                logger.Warn("Clear HtmlFilesParentPath failed. Reason: {0}", ex.ToString());
//            }
//        }

//        private void RestoreItem(Object itemBlock)
//        {
//            if (itemBlock is RestoreDataBlockCollection)
//            {
//                RealProcess(itemBlock as RestoreDataBlockCollection);
//            }
//        }

//        private void RestoreFolder(RestoreDataBlockCollection dataBlockCollection)
//        {
//            BaseRestoreHelperBatch.CurrentChannel = new ChannelCache() { DisplayName = dataBlockCollection.Items.First().FileHeader.Name };
//        }

//        private void RealProcess(RestoreDataBlockCollection dataBlockCollection)
//        {
//            var collectionType = dataBlockCollection.CollectionType;

//            var restoreHelper = InitiateRestoreHelper(collectionType);

//            if (restoreHelper != null)
//            {
//                restoreHelper.Init(dataBlockCollection.Items.First().FileHeader, config);
//                (restoreHelper as ItemToStorageHelperBatch).Restore(null, null, dataBlockCollection.Items);
//            }
//        }

//        private IRestoreHelperBatch InitiateRestoreHelper(ExchangeDataBlockType dataType)
//        {
//            var restoreHelperType = RestoreHelperFactory.GetRestoreHelperType(this.config.RestoreType, this.config.MailboxType, dataType);
//            return WorkerServiceLocator.GetService<IRestoreHelperBatch>(i => i.IsType(restoreHelperType));
//        }
//    }
//}