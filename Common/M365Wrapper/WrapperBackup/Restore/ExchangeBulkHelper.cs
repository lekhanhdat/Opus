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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ExchangeUtility.Graph;
using Microsoft.Exchange.WebServices.Data;


namespace ExchangeRestoreUtility
{
    public class ExchangeBulkHelper : ExchangeObjectBase
    {
        private ExchangeFolder parentFolder;
        public static Dictionary<string, string> itemIdsDic = new Dictionary<string, string>();
        public static Dictionary<string, string> ExchangeIdsDic { get; set; } = new Dictionary<string, string>();

        public ExchangeBulkHelper(ExchangeFolder folder) : base(folder.AuthObject)
        {
            //this.service = CloneExchangeService(folder.Service, -1);
            this.parentFolder = folder;
        }

        #region ---------Uplaod----------

        public Dictionary<string, ExportAndImportItemResult> ImportItems(Dictionary<string, UploadItemParameter> importItemsDic)
        {
            var service = CloneExchangeService(parentFolder.Service, -1);
            var retry = 0;
            var remainItemDic = importItemsDic;
            Dictionary<string, ExportAndImportItemResult> failedItems;
            Dictionary<string, ExportAndImportItemResult> items = null;
            do
            {
                if (retry > 0) logger.Info("Retry import items, retry count: {0}, remain item count: {1}", retry, remainItemDic.Count);

                try
                {
                    var uploadItems = remainItemDic.Values;
                    //uploadItems.ForEach(itemArg => itemArg.DataStream.Seek(0, SeekOrigin.Begin));
                    (var successfulItems, failedItems) = ImportItemsInternal(service, uploadItems, remainItemDic.Keys.ToList());
                    items = items.Merge(successfulItems);
                    if (!WaitForNextRequest(failedItems)) break;
                }
                catch (Exception ex)
                {
                    failedItems = AssemblyFailedItems(remainItemDic.Keys.ToList(), ex);
                    logger.Error("Failed to import items, retry: {0}, error: {1}.", retry, ex);
                    if (retry >= 10) break;
                    if (!WaitForNextRequest(ex)) break;
                }
                remainItemDic = failedItems?.Keys.ToDictionary(idArg => idArg, key => remainItemDic[key]);
                ++retry;
            }
            while (!failedItems.IsEmpty() && retry < 11);
            return items.Merge(failedItems);
        }

        private (Dictionary<string, ExportAndImportItemResult>, Dictionary<string, ExportAndImportItemResult>) ImportItemsInternal(ExchangeService service, IEnumerable<UploadItemParameter> uploadItems, List<string> ids)
        {
            const int lagerFileLimit = 50 * 1024 * 1024;

            var successfulResult = new Dictionary<string, ExportAndImportItemResult>();
            var failedResult = new Dictionary<string, ExportAndImportItemResult>();

            // if (uploadItems.Sum(i => i.DataSize) > lagerFileLimit)
            // {
            //     var index = 0;
            //     uploadItems.ForEach(i =>
            //     {
            //         var item = service.ImportLargeItem(i).ExecuteAsyncTask();
            //         HandleImportResult(item, index++);
            //     });
            // }
            // else
            // {
            //     var items = service.ImportItems(uploadItems).ExecuteAsyncTask();
            //     HandleImportResult(items, 0);
            // }

            return (successfulResult, failedResult);

            void HandleImportResult(ServiceResponseCollection<UploadItemsResponse> items, int index)
            {
                items.ForEach(i =>
                {
                    var result = AssemblyOneItem(i);
                    if (result.IsFailed)
                        failedResult.Add(ids[index], result);
                    else
                        successfulResult.Add(ids[index], result);
                    ++index;
                });
            }
        }

        #endregion

        #region ---------Delete----------

        public Dictionary<string, ExportAndImportItemResult> DeleteItems(List<string> deleteItemUniqueIds)
        {
            var service = CloneExchangeService(parentFolder.Service, -1);
            var retry = 0;
            var remainDeleteItemUniqueIds = deleteItemUniqueIds;
            Dictionary<string, ExportAndImportItemResult> failedItems;
            Dictionary<string, ExportAndImportItemResult> items = null;
            do
            {
                if (retry > 0) logger.Info("<Batch>: Retry delete items, retry count: {0}, remain item count: {1}", retry, remainDeleteItemUniqueIds.Count);
                try
                {
                    var deleteItemIds = remainDeleteItemUniqueIds.Select(deleteItemId => new ItemId(deleteItemId)).ToList();
                    var temp = DeleteItemsInternal(service, deleteItemIds, out failedItems);
                    items = items.Merge(temp);
                    logger.Info("<Batch>: Delelte items ,Successfull items count:[{0}],Failed items Count:[{1}]", temp.Count, failedItems.Count);
                }
                catch (Exception ex)
                {
                    failedItems = AssemblyFailedItems(remainDeleteItemUniqueIds, ex);
                    logger.Error("<Batch>: Failed to delete items, retry: {0}, error: {1}", retry, ex);
                    if (retry >= 5) break;
                    if (!WaitForNextRequest(ex)) break;
                }
                remainDeleteItemUniqueIds = failedItems != null ? failedItems.Keys.ToList() : null;
                ++retry;
            }
            while (!failedItems.IsEmpty() && retry < 5);
            return items.Merge(failedItems);
        }

        private Dictionary<string, ExportAndImportItemResult> DeleteItemsInternal(ExchangeService service, List<ItemId> itemIds, out Dictionary<string, ExportAndImportItemResult> failedResult)
        {
            var successfulResults = new Dictionary<string, ExportAndImportItemResult>();
            failedResult = new Dictionary<string, ExportAndImportItemResult>();
            var items = service.DeleteItems(itemIds, DeleteMode.HardDelete, SendCancellationsMode.SendToNone, AffectedTaskOccurrence.SpecifiedOccurrenceOnly).ExecuteAsyncTask();
            var index = 0;
            foreach (var item in items)
            {
                var result = AssemblyOneItem(item, itemIds[index]);
                if (result.IsFailed)
                {
                    failedResult.Add(itemIds[index].UniqueId, result);
                }
                else
                {
                    successfulResults.Add(itemIds[index].UniqueId, result);
                }
                ++index;
            }
            return successfulResults;
        }

        public void DeleteItemSimple(List<string> deleteItemUniqueIds)
        {
            try
            {
                logger.Info("<Batch>: Begin to delete items.");
                var service = CloneExchangeService(parentFolder.Service, -1);
                var deleteItemIds = deleteItemUniqueIds.Select(deleteItemId => new ItemId(deleteItemId)).ToList();
                service.DeleteItems(deleteItemIds, DeleteMode.HardDelete, SendCancellationsMode.SendToNone, AffectedTaskOccurrence.AllOccurrences).ExecuteAsyncTask();
                logger.Info("<Batch>: Delete items Finish, Successfull items count:[{0}]", deleteItemIds.Count);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to delete items, error: {0}", ex);
                WaitForNextRequest(ex);
                Thread.Sleep(10 * 1000);
            }
        }

        #endregion

        #region ---------Update----------

        public Dictionary<string, ExportAndImportItemResult> UpdateItems(ExchangeService service, Dictionary<string, string> itemIds)
        {
            service = CloneExchangeService(parentFolder.Service, -1);
            var retry = 0;
            Dictionary<string, ItemId> remainItemDic = itemIds.ToDictionary(key => key.Value, value => new ItemId(value.Key));
            Dictionary<string, ExportAndImportItemResult> failedItems;
            Dictionary<string, ExportAndImportItemResult> items = null;
            do
            {
                if (retry > 0) logger.Info("Retry update items, retry count: {0}, remain item count: {1}", retry, remainItemDic.Count);
                try
                {
                    var temp = UpdateItemsInternal(service, remainItemDic, out failedItems);
                    items = items.Merge(temp);
                }
                catch (Exception ex)
                {
                    failedItems = AssemblyFailedItems(remainItemDic.Keys.ToList(), ex);
                    logger.Error("Failed to update items, retry: {0}, error: {1}", retry, ex);
                    if (retry >= 5) break;
                    if (!WaitForNextRequest(ex)) break;
                }
                remainItemDic = failedItems != null ? failedItems.Keys.ToDictionary(idArg => idArg, key => remainItemDic[key]) : null;
                ++retry;
            }
            while (!failedItems.IsEmpty() && retry < 5);
            return items.Merge(failedItems);
        }

        private Dictionary<string, ExportAndImportItemResult> UpdateItemsInternal(ExchangeService service, Dictionary<string, ItemId> itemIds, out Dictionary<string, ExportAndImportItemResult> failedResult)
        {
            var itemList = new List<Item>();
            var successfulResults = new Dictionary<string, ExportAndImportItemResult>();
            failedResult = new Dictionary<string, ExportAndImportItemResult>();
            PropertySet propSet = new PropertySet(BasePropertySet.FirstClassProperties);
            var def = new ExtendedPropertyDefinition(new Guid("0006200A-0000-0000-C000-000000000046"), 0xF555, MapiPropertyType.String);

            var bindItemsResponse = service.BindToItems(itemIds.Values, propSet).ExecuteAsyncTask();
            itemList = bindItemsResponse.Select(bindItem => bindItem.Item).ToList();
            //此处认为bind后item顺序和传人顺序相同，之后需要处理bind失败的情况
            var bindItemIndex = 0;
            itemList.ForEach(item => item.SetExtendedProperty(def, itemIds.Keys.ToList()[bindItemIndex]));

            var items = service.UpdateItems(itemList, parentFolder.FolderId, ConflictResolutionMode.AlwaysOverwrite, MessageDisposition.SaveOnly, SendInvitationsOrCancellationsMode.SendToNone).ExecuteAsyncTask();
            var index = 0;
            foreach (var item in items)
            {
                var result = AssemblyOneItem(item);
                if (result.IsFailed)
                {
                    failedResult.Add(itemIds.Keys.ToList()[index], result);
                }
                else
                {
                    successfulResults.Add(itemIds.Keys.ToList()[index], result);
                }
                ++index;
            }
            return successfulResults;
        }

        public void UpdateItemsSimple(Dictionary<string, string> itemIds)
        {
            try
            {
                var service = base.CloneExchangeService(parentFolder.Service, -1);
                var itemIdDic = itemIds.ToDictionary(key => key.Value, value => new ItemId(value.Key));//<原ItemId，新ItemId>
                var itemList = new List<Item>();
                var propSet = new PropertySet(BasePropertySet.FirstClassProperties);
                var def = new ExtendedPropertyDefinition(new Guid("0006200A-0000-0000-C000-000000000046"), 0xF555, MapiPropertyType.String);

                var bindItemsResponse = service.BindToItems(itemIdDic.Values, propSet).ExecuteAsyncTask();//新Items
                itemList = bindItemsResponse.Select(bindItem => bindItem.Item).ToList();
                //此处认为bind后item顺序和传人顺序相同，之后需要处理bind失败的情况
                var bindItemIndex = 0;
                itemList.ForEach(item => item.SetExtendedProperty(def, itemIds.Values.ToList()[bindItemIndex++]));
                service.UpdateItems(itemList, parentFolder.FolderId, ConflictResolutionMode.AlwaysOverwrite, MessageDisposition.SaveOnly, SendInvitationsOrCancellationsMode.SendToNone);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to update items, error: {0}", ex);
                WaitForNextRequest(ex);
                Thread.Sleep(10 * 1000);
            }
        }

        #endregion

        #region ---------Get-------------

        public void CacheAllItems(FolderId folderId, string folderName)
        {
            var service = CloneExchangeService(parentFolder.Service);
            itemIdsDic.Clear();
            ExchangeIdsDic.Clear();
            //#1.获取所有有扩展属性（我们还原回去的）的Item的ID的字典，格式<OldId,NewId>
            var itemsDicByExtendedProperty = GetAllItemsByExtendedProperty(service, folderId);
            //#2.获取所有Folder下所有ItemId（NewId）
            var itemIdsByFolderId = GetAllItemsByFolderId(service, folderId).Select(item => item.Id.UniqueId).ToList();
            //#3.#1的Value和#2取差集 Items without extended properties
            var itemsByFolderIdExceptExtendedProperty = itemIdsByFolderId.Except(itemsDicByExtendedProperty.Values.ToList());
            //#4.#3的结果ToDictionary|Items without extended properties|<NewId,NewId>
            var itemsDicByFolderIdExceptExtendedProperty = itemsByFolderIdExceptExtendedProperty.ToDictionary(key => key, value => value);
            //#5.#1和#4取合集，此集合中会有之前还原回来的<OldId,NewId>和没经过还原的<NewId,NewId>,此种格式有利于冲突处理时快速查找到OldId(备份的itemId)所对应的NewId(item的实际id)
            var sameKeys = itemsDicByExtendedProperty.Keys.ToList().Intersect(itemsDicByFolderIdExceptExtendedProperty.Keys.ToList());
            logger.Info("<Batch>:Same Keys Count : [{0}].", sameKeys.Count());
            //itemIdsDic = itemsDicByExtendedProperty.Union(itemsDicByFolderIdExceptExtendedProperty).ToDictionary(key => key.Key, value => value.Value);
            itemIdsDic = itemsDicByExtendedProperty.Union(itemsDicByFolderIdExceptExtendedProperty).ToLookup(t => t.Key, t => t.Value).ToDictionary(t => t.Key, t => t.First());
            //Case where the item has a fake item ID after processing the move item.
            itemsDicByFolderIdExceptExtendedProperty.ForEach(kv =>
            {
                var exchangeId = ExchangeConstants.ConvertItemId(kv.Key);
                if (!ExchangeIdsDic.TryAdd(exchangeId, kv.Value))
                {
                    logger.Warn($"ExcahgneId [{exchangeId}] already has the same record.");
                }
            });
            logger.Info("<Batch>:Item Count is [{0}],Folder Name:[{1}] .", itemIdsDic.Count, folderName);
        }

        private List<Item> GetAllItemsByFolderId(ExchangeService service, FolderId folderId)
        {
            var items = new List<Item>();
            var pageSize = 512;
            var itemView = new ItemView(pageSize) { PropertySet = new PropertySet(BasePropertySet.IdOnly) };
            FindItemsResults<Item> searchResult;
            do
            {
                searchResult = service.FindItems(folderId, itemView).ExecuteAsyncTask();
                items.AddRange(searchResult.Items);
                itemView.Offset += pageSize;
            }
            while (searchResult.MoreAvailable);
            return items;
        }

        private Dictionary<string, string> GetAllItemsByExtendedProperty(ExchangeService service, FolderId folderId)
        {
            var items = new List<Item>();
            var resultItems = new Dictionary<string, string>();
            var extendedPropertyDefinition = new ExtendedPropertyDefinition(new Guid("0006200A-0000-0000-C000-000000000046"), 0xF555, MapiPropertyType.String);
            var propertySet = new PropertySet(BasePropertySet.IdOnly, extendedPropertyDefinition);
            var pageSize = 512;
            var itemView = new ItemView(pageSize);
            itemView.PropertySet = propertySet;
            SearchFilter filter = new SearchFilter.Exists(extendedPropertyDefinition);
            FindItemsResults<Item> searchResult = null;
            do
            {
                searchResult = service.FindItems(folderId, filter, itemView).ExecuteAsyncTask();
                items.AddRange(searchResult.Items);
                itemView.Offset += pageSize;
            }
            while (searchResult.MoreAvailable);
            foreach (var item in items)
            {
                var oldItemId = new object();
                item.TryGetProperty(extendedPropertyDefinition, out oldItemId);
                var oldId = oldItemId as string;
                if (!resultItems.ContainsKey(oldId)) resultItems.Add(oldId, item.Id.UniqueId);
            }
            return resultItems;
        }

        #endregion

        private bool WaitForNextRequest(Dictionary<string, ExportAndImportItemResult> failedItems)
        {
            if (failedItems.IsEmpty()) return false;
            var maxBackOffMilliseconds = failedItems.Values.Max(resultArg => resultArg.ErrorCode.BackOffMilliseconds());
            logger.Info("<Batch>:Wait Time {0}.", maxBackOffMilliseconds);
            return ServiceResponseExceptionExtension.WaitForNextRequest(maxBackOffMilliseconds);
        }

        public UploadItemParameter ConvertUploadItemParameter(Stream contentStream, long? dataSize)
        {
            if (!string.Equals(this.parentFolder.ParentFolderId.UniqueId, this.parentFolder.GetCurrentFolderId()))
                logger.Info("<Batch>:ParentFolderId: [{0}],CurrentFolderId:[{1}]", this.parentFolder.ParentFolderId.UniqueId, this.parentFolder.GetCurrentFolderId());
            return new UploadItemParameter()
            {
                CreateAction = CreateAction.CreateNew,
                // DataStream = contentStream,
                // DataSize = dataSize,
                IsAssociated = false,
                ParentFolderId = new FolderId(parentFolder.ParentFolderId.UniqueId)
            };
        }

        private ExportAndImportItemResult AssemblyOneItem(UploadItemsResponse baseItem)
        {
            var item = baseItem as UploadItemsResponse;
            if (item.Result == ServiceResult.Success)
            {
                return ExportAndImportItemResult.CreateSuccessfulResult(item.ItemId.UniqueId);
            }
            else
            {
                var itemId = item.ItemId != null ? item.ItemId.UniqueId : null;
                return ExportAndImportItemResult.CreateFailedResult(itemId, item.ErrorMessage, item.ErrorCode);
            }
        }

        private ExportAndImportItemResult AssemblyOneItem(UpdateItemResponse baseItem)
        {
            var item = baseItem as UpdateItemResponse;
            if (item.Result == ServiceResult.Success)
            {
                return ExportAndImportItemResult.CreateSuccessfulResult(item.ReturnedItem.Id.UniqueId);
            }
            else
            {
                var itemId = item.ReturnedItem.Id != null ? item.ReturnedItem.Id.UniqueId : null;
                return ExportAndImportItemResult.CreateFailedResult(itemId, item.ErrorMessage, item.ErrorCode);
            }
        }

        private ExportAndImportItemResult AssemblyOneItem(ServiceResponse baseItem, ItemId itemId)
        {
            if (baseItem.Result == ServiceResult.Success)
            {
                return ExportAndImportItemResult.CreateSuccessfulResult(itemId.UniqueId);
            }
            else
            {
                return ExportAndImportItemResult.CreateFailedResult(itemId.UniqueId, baseItem.ErrorMessage, baseItem.ErrorCode);
            }
        }

        private static Dictionary<string, ExportAndImportItemResult> AssemblyFailedItems(List<string> ids, Exception ex)
        {
            return ids.ToDictionary(idArg => idArg, idArg => ExportAndImportItemResult.CreateFailedResult(idArg, ex.Message));
        }

        private int GetWaitTime(Exception ex)
        {
            if (ex == null) return ServiceErrorExtension.DefaultBackOffMilliseconds;
            var sbEx = ex as ServerBusyException;
            if (sbEx != null) return sbEx.BackOffMilliseconds;
            var srEx = ex as ServiceResponseException;
            if (srEx != null) return srEx.ErrorCode.BackOffMilliseconds();

            return ServiceErrorExtension.DefaultBackOffMilliseconds;
        }

        private bool WaitForNextRequest(Exception ex)
        {
            return ServiceResponseExceptionExtension.WaitForNextRequest(GetWaitTime(ex));
        }

        //private UploadItemParameter ConvertUploadItemParameter(string file)
        //{
        //    return new UploadItemParameter()
        //    {
        //        CreateAction = CreateAction.CreateNew,
        //        DataStream = GetDataStream(file),
        //        DataSize = GetDataSize(file),
        //        IsAssociated = false,
        //        ParentFolderId = new FolderId() { UniqueId = this.parentFolder.ParentFolderId.UniqueId }
        //    };
        //}
        //private Stream GetDataStream(string filePath)
        //{
        //    //const int LargeFileSize = 850 * 1024;//850k
        //    var stream = new MemoryStream();
        //    using (var fileStream = new FileStream(filePath, FileMode.Open))
        //    {
        //        //if (fileStream.Length > LargeFileSize) return fileStream;
        //        var buffer = this.contentBuffer;
        //        int length;
        //        while ((length = fileStream.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            stream.Write(buffer, 0, length);
        //        }
        //    }
        //    return stream;
        //}
        //private long? GetDataSize(string filePath)
        //{
        //    try
        //    {
        //        FileInfo fileInfo = new FileInfo(filePath);
        //        return fileInfo.Length;
        //    }
        //    catch (NotSupportedException ex)
        //    {
        //        logger.Warn("Failed to get content file length, error: {1}", ex);
        //        return null;
        //    }
        //}
    }

    public class ExportAndImportItemResult
    {
        public string Id { get; private set; }

        public string ErrorMessage { get; private set; }

        public ServiceError ErrorCode { get; private set; }

        public bool IsFailed
        {
            get { return !string.IsNullOrEmpty(this.ErrorMessage); }
        }

        private ExportAndImportItemResult()
        {
        }

        public static ExportAndImportItemResult CreateSuccessfulResult(string id)
        {
            return new ExportAndImportItemResult() { Id = id };
        }

        public static ExportAndImportItemResult CreateFailedResult(string id, string error, ServiceError errorCode)
        {
            return new ExportAndImportItemResult()
            {
                Id = id,
                ErrorMessage = string.Format("Error code: {0}.{1}{2}", errorCode, Environment.NewLine, error),
                ErrorCode = errorCode
            };
        }

        public static ExportAndImportItemResult CreateFailedResult(string id, string error)
        {
            return new ExportAndImportItemResult()
            {
                Id = id,
                ErrorMessage = error,
            };
        }
    }

    public static class DictionaryExtension
    {
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> self, Dictionary<TKey, TValue> target)
        {
            if (target == null) return self;
            if (self == null) return target;
            foreach (var kv in target)
            {
                self.Add(kv.Key, kv.Value);
            }
            return self;
        }

        public static bool IsEmpty<TKey, TValue>(this Dictionary<TKey, TValue> self)
        {
            return self == null || self.Count == 0;
        }
    }
}