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

using AvePoint.GCommon.Contract.Media.Object;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Office365GroupRestore
{
    public class RestoreDataHandlerBatch : RestoreDataHandlerBase
    {
        //internal OutPlaceMappingManager OutPlaceMappingManager { get { return Config.OutPlaceMappingManager; } }
        private BlockingCollection<RestoreDataBlockCollection> outputCollection = new BlockingCollection<RestoreDataBlockCollection>(30);

        private HashSet<ExchangeDataType> ContainerDataType = [ExchangeDataType.SiteCollection, ExchangeDataType.SiteFolder, ExchangeDataType.Web, ExchangeDataType.SiteList];
        public override void Add(ExchangeDataBlock dataBlock)
        {
            var dataBlockForBatch = new ExchangeDataBlockForBatch()
            {
                FileTail = dataBlock.FileTail,
                IsFinish = dataBlock.IsTimeOut,
                FileHeader = dataBlock.FileHeader,
                IsException = dataBlock.IsException,
                ExceptionMessage = dataBlock.ExceptionMessage,
            };
            try
            {
                if (!dataBlockForBatch.IsException && !dataBlockForBatch.IsFinish)
                {
                    dataBlockForBatch.RestoreData = new ExchangeRestoreDataForBatch()
                    {
                        RestoreStream = dataBlock.RestoreData.RestoreStream,
                        MetadataLists = dataBlock.RestoreData.MetadataLists
                    };
                    RestoreDataBlockCollection.GroupNormalDataBlock(dataBlockForBatch, this.outputCollection, Config.MaxBulkItemsCount, Config.MaxBulkItemSize);
                }
                else
                {
                    RestoreDataBlockCollection.GroupFinishOrExceptionDataBlock(dataBlockForBatch, this.outputCollection);
                }
            }
            catch (Exception ex)
            {
                var parentFullPath = dataBlockForBatch.FileHeader == null ? string.Empty : dataBlockForBatch.FileHeader.ParentFullPath;
                var name = dataBlockForBatch.FileHeader == null ? string.Empty : dataBlockForBatch.FileHeader.Name;
                logger.Error("An error occured when adding datablock to batch collection.DisplayPath:[{0}]-[{1}],Exception:{2}", parentFullPath, name, ex);
            }
        }
        public override void AddForEXO(ExchangeDataBlock dataBlock)
        {
            var dataBlockForBatch = new ExchangeDataBlockForBatch()
            {
                FileTail = dataBlock.FileTail,
                IsFinish = dataBlock.IsTimeOut,
                FileHeader = dataBlock.FileHeader,
                IsException = dataBlock.IsException,
                ExceptionMessage = dataBlock.ExceptionMessage,
            };
            try
            {
                dataBlockForBatch.RestoreData = new ExchangeRestoreDataForBatch()
                {
                    RestoreStream = dataBlock.RestoreData.RestoreStream,
                    MetadataLists = dataBlock.RestoreData.MetadataLists,
                    ContentStream = dataBlock.RestoreData.ContentStream,
                };
                RestoreDataBlockCollection.AddEXODataBlock(dataBlockForBatch, this.outputCollection);
            }
            catch (Exception ex)
            {
                var parentFullPath = dataBlockForBatch.FileHeader == null ? string.Empty : dataBlockForBatch.FileHeader.ParentFullPath;
                var name = dataBlockForBatch.FileHeader == null ? string.Empty : dataBlockForBatch.FileHeader.Name;
                logger.Error("An error occured when adding datablock to batch collection.DisplayPath:[{0}]-[{1}],Exception:{2}", parentFullPath, name, ex);
            }
        }

        public override void AddForSite(ExchangeDataBlock dataBlock)
        {
            var dataBlockForBatch = new ExchangeDataBlockForBatch()
            {
                FileTail = dataBlock.FileTail,
                IsFinish = dataBlock.IsTimeOut,
                FileHeader = dataBlock.FileHeader,
                IsException = dataBlock.IsException,
                ExceptionMessage = dataBlock.ExceptionMessage,
            };
            try
            {
                if (ContainerDataType.Contains(dataBlock.FileHeader.DataType))
                {
                    RestoreDataBlockCollection.AddSiteDataBlock(dataBlockForBatch, this.outputCollection);
                    return;
                }
                dataBlockForBatch.RestoreData = new ExchangeRestoreDataForBatch()
                {
                    RestoreStream = dataBlock.RestoreData.RestoreStream,
                    MetadataLists = dataBlock.RestoreData.MetadataLists,
                    ContentStream = dataBlock.RestoreData.ContentStream,
                };
                RestoreDataBlockCollection.AddSiteDataBlock(dataBlockForBatch, this.outputCollection);
            }
            catch (Exception ex)
            {
                var parentFullPath = dataBlockForBatch.FileHeader == null ? string.Empty : dataBlockForBatch.FileHeader.ParentFullPath;
                var name = dataBlockForBatch.FileHeader == null ? string.Empty : dataBlockForBatch.FileHeader.Name;
                logger.Error("An error occured when adding datablock to batch collection.DisplayPath:[{0}]-[{1}],Exception:{2}", parentFullPath, name, ex);
            }
        }

        public override IEnumerable<RestoreDataBlockCollection> GetDateBlockCollection()
        {
            return this.outputCollection.GetConsumingEnumerable();
        }

        public override Int32 GetOutputCollectionCount()
        {
            return this.outputCollection.Count;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.outputCollection?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}