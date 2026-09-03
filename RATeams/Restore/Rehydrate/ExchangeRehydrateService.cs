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
//    using System.Data.SQLite;
//    using System.Linq;

//    using AvePoint.Common;
//    using AvePoint.GCommon.Contract.Media.TCPRequest;
//    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
//    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
//    using AvePoint.GCommon.Contract.Storage.Entity;
//    using AvePoint.Media.Resource.MediaServiceApplicationModel;
//    
//    using AvePoint.Media.Resource.MediaServiceGranularBackup;
//    using AvePoint.Media.Service;
//    using AvePoint.Media.Service.DomainModel;
    
//    using AvePoint.Media.Service.SupportabilityModel;
//    using AvePoint.RA.CommonUtil;
//    using AvePoint.RehydrateCore;

    

//    using Storage;
//    public class ExchangeRehydrateService : IRehydrateService
//    {
//        private readonly RALogger logger = RALogger.GetInstance(typeof(ExchangeRehydrateService));
//        protected IXSystem indexLogicalDevice { get; set; }
//        protected Dictionary<string, long> IndexDataBlockSizeCache { get; set; }
//        protected long BlockCount { get; set; }
//        protected long IndexItemCount { get; set; }

//        protected AveMemorySender<RehydrateBaseItem> mSender { get; set; }
//        public ICacheService CacheManager { get; set; }
//        public ExchangeIndexService IndexService { get; set; }
//        public ExchangeRestoreJob RestoreJob { get; set; }
//        //public IExchangeRestoreTreeHandler TreeHandler { get; set; }
//        public IExchangeRestoreTreeHandler TreeHandler { get; set; }
//        public IStorageDeviceManager StorageDeviceManager { get; set; }
//        public IExchangeRehydrateIndexService RehydrateIndexService { get; set; }


//        public long HandleRehydrateRequest(MediaTCPRequest request, AveMemorySender<RehydrateBaseItem> sender)
//        {
//            try
//            {
//                mSender = sender;
//                InitRestoreJob(request);
//                logger.Info("Open index logical device.");
//                indexLogicalDevice = this.StorageDeviceManager.Open(RestoreJob.LogicalDevice.GetXRIS(PhysicalDeviceUsage.Index), DeviceAccess.Read);
//                logger.Info("Open cache manager.");
//                CacheManager.Open(RestoreJob.CacheSetting);
//                var mailBoxNodes = RestoreJob.ExchangeTreeRoot.Children[0].Children.SelectMany(group => group.Children).ToList();
//                logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreServiceOpenCalculateTotleItemNumStartCalculate);
//                foreach (var mailBox in mailBoxNodes)
//                {
//                    RestoreConfig.CurrentMailbox = string.Format("{0}(GroupInfo)", mailBox.Name);
//                    RestoreConfig.CurrentMailboxType = mailBox.MailboxType;
//                    RestoreConfig.CurrentMailboxIndexCode = RestoreCommonUtility.GetAgentIndexName(RestoreConfig.CurrentMailbox, RestoreConfig.CurrentMailboxType, true);
//                    IndexService.Open(new ExchangeIndexServiceOpenParameter(RestoreJob, CacheManager.CacheSystem, indexLogicalDevice, $"{mailBox.Name}(GroupInfo)", mailBox.MailboxType));
//                    mSender.Send(new RehydrateHeaderItem
//                    {
//                        SourceIndexVolumn = VolumeGeneratorFactory.GetVolumeGenerator(VolumeType.ExchangeBackup).GenerateIndexVolume(new VolumeParameter(request as ExchangeRestoreRequest)),
//                        TargetIndexVolumn = VolumeGeneratorFactory.GetVolumeGenerator(VolumeType.ExchangeBackup).GenerateIndexVolume(new VolumeParameter(request as ExchangeRestoreRequest) { ModulePath = $"Rehydrate##{request.JobId}" }),
//                        DataUris = RestoreJob.LogicalDevice.GetXRIS(PhysicalDeviceUsage.Data),
//                        IndexUris = RestoreJob.IndexDBLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index),
//                        IndexDatabaseName = IndexService.CurrentOpenIndexDatabaseName
//                    });
//                    IndexDataBlockSizeCache = RehydrateIndexService.GetAllMasterIndexMaxDataSize();
//                    this.TreeHandler.IndexItemProceed += new EventHandler<IndexItemProceedEventArgs>(TreeHandlerIndexItemProceed);
//                    this.TreeHandler.ProcessTreeNode(new TreeNodeParameter { ExchangeTree = mailBox, RestoreJob = RestoreJob, IsJustCalculateCount = false });
//                    this.TreeHandler.IndexItemProceed -= new EventHandler<IndexItemProceedEventArgs>(TreeHandlerIndexItemProceed);
//                    logger.Info($"[Monitor]Index Processed:{IndexItemCount},BlockCount:{BlockCount},Complete.");
//                    logger.Info(MediaServiceGranularBackupResource.GranularRestoreServiceOpenCalculateTotleItemNumDataCount, $"IndexItemCount:{IndexItemCount},BlockCount:{BlockCount}");
//                }
//                mSender.Send(new RehydrateCompleteItem { Success = true });
//            }
//            catch (SQLiteException se)
//            {
//                var errorMessage = CatchHelper.ProcessException(se);
//                logger.Error(MediaServiceApplicationModelResource.RestoreServiceBaseHandleRequestRestoreError, se.ToString());
//                mSender.Send(new RehydrateCompleteItem { Success = false, Message = errorMessage });
//            }
//            catch (Exception e)
//            {
//                var errorMessage = CatchHelper.ProcessException(e);
//                logger.Error(MediaServiceApplicationModelResource.RestoreServiceBaseHandleRequestRestoreError, e.ToString());
//                mSender.Send(new RehydrateCompleteItem { Success = false, Message = errorMessage });
//            }
//            return IndexItemCount;
//        }

//        private void InitRestoreJob(MediaTCPRequest request)
//        {
//            RestoreJob = Activator.CreateInstance(typeof(ExchangeRestoreJob), request) as ExchangeRestoreJob;
//            InitCacheLocation();
//        }

//        private void InitCacheLocation()
//        {
//            RestoreJob.CacheSetting = new CacheSettingDto() { Extension = new CacheSettingExtension() };
//            RestoreJob.CacheSetting.Extension.Path = new List<PathMap> { new PathMap() { DiskInfo = new DiskInfoDto() } };
//            RestoreJob.CacheSetting.Extension.Path[0].DiskInfo.Path = AveEnv.GetAgentTempFolder(ContextLevel.Process);
//        }

//        private void TreeHandlerIndexItemProceed(object sender, IndexItemProceedEventArgs args)
//        {
//            this.ExchangeIndexHandled(args.IndexItem as GroupBasicIndex);
//        }

//        private void ExchangeIndexHandled(GroupBasicIndex index)
//        {
//            var indexItem = new RehydrateIndexItem(GetObjectType(index).ToString());
//            indexItem.DataLength = index.CurrentItemMetaDataAndContentDataTotalLength;
//            var indexCode = IndexService.CurrentOpenIndexDatabaseName.Replace("index", "").Replace(".db", "");
//            var contentBlocks = ProcessDataBlocks("content", index.ContentLength, index.ContentDataOffset, GetBlockSize(index), index.ContentDataFileNumber, index.ContentDataFilePrefixNumber, indexCode).ToList();
//            var metaBlocks = ProcessDataBlocks("meta", index.DataFileLength, index.DataFileOffset, GetBlockSize(index), index.DataFileNumber, index.DataFilePrefixNumber, indexCode).ToList();
//            indexItem.BackupJobId = index.JobId;
//            indexItem.BlockNameList = new List<string>();
//            indexItem.BlockNameList.AddRange(contentBlocks);
//            indexItem.BlockNameList.AddRange(metaBlocks);
//            logger.Info($"Process index {index.Type} - {index.ParentPathMD5}/{index.PathMD5} - {index.DataFilePrefixNumber}|{index.DataFileNumber}|{index.DataFileOffset}|{index.DataFileLength} - {index.ContentDataFilePrefixNumber}| {index.ContentDataFileNumber}| {index.ContentOffset}|{index.ContentLength} - MetadataBlock({metaBlocks.Count}):{string.Join(";", metaBlocks.ToArray())}, - ContentBlock({contentBlocks.Count}):{string.Join(";", contentBlocks.ToArray())}.");
//            mSender.Send(indexItem);
//            BlockCount += indexItem.BlockNameList.Count;
//            IndexItemCount++;
//            if (IndexItemCount % 100 == 0)
//            {
//                logger.Info($"[Monitor]Index Processed:{IndexItemCount},BlockCount:{BlockCount}");
//            }
//        }

//        private long GetBlockSize(GroupBasicIndex index)
//        {
//            return IndexDataBlockSizeCache.TryGetValue(index.JobId, out long size) ? size * 1024 * 1024 : throw new Exception("Index size not found.");
//        }

//        private IEnumerable<string> ProcessDataBlocks(string blockPrefixName, long dataLength, long currentOffset, long jobmaxdatablocksize, long dataFileNumber, long dataFilePrefixNumber,string indexCode)
//        {
//            long contentDataCount = (dataLength + currentOffset) / jobmaxdatablocksize;
//            //logger.Debug($"{blockPrefixName} size:{dataLength + currentOffset},content dat count:{contentDataCount+1}");
//            for (int i = 0; i <= contentDataCount; i++)
//            {
//                long fileNumber = dataFileNumber + i;
//                yield return $"{blockPrefixName}{dataFilePrefixNumber}_{fileNumber}{indexCode}.dat";
//            }
//        }

//        private ExchangeObjectType GetObjectType(GroupBasicIndex index)
//        {
//            ExchangeObjectType eObjectType = ExchangeObjectType.Item;
//            switch (index.Type)
//            {
//                case 0:
//                    eObjectType = ExchangeObjectType.Mailbox;
//                    break;
//                case 1:
//                    eObjectType = ExchangeObjectType.Folder;
//                    break;
//                case 2:
//                    eObjectType = ExchangeObjectType.Item;
//                    break;
//                case 3:
//                    eObjectType = ExchangeObjectType.Index;
//                    break;
//                default:
//                    logger.Warn(MediaServiceGranularBackupResource.GranularRestoreStatisticsCalculatorGranularIndexHandledUnknownType, index.Type);
//                    break;
//            }
//            return eObjectType;
//        }

//        public void Dispose()
//        {
//            try
//            {
//                if (IndexService != null) IndexService.Close();
//            }
//            catch (Exception ex)
//            {
//                logger.Error(MediaServiceGranularBackupResource.GranularBackupDataWriterCloseDataWriterCloseIndexServiceError, ex.ToString());
//                throw;
//            }
//            finally
//            {
//                CacheManager.Close();
//                StorageDeviceManager.Close(this.indexLogicalDevice);
//                logger.Info(MediaServiceGranularBackupResource.GranularBackupDataWriterCloseDataWriterClose);
//            }
//            logger.Info("Rehydrate job finished successfully.");
//        }
//    }
//}