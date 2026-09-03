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

namespace ExchangeBackupUtility
{
    using AvePoint.RA.Common;
    using AvePoint.RA.CommonUtil;
    using ExchangeUtility;
    using Microsoft.Exchange.WebServices.Data;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using AvePoint.Common;
    using AvePoint.RA.Common.Global.Utils;
    using ExchangeBackupUtility.Graph;
    using ExchangeUpdateItemResult = ExchangeUtility.Graph.ExchangeUpdateItemResult;

    public class ExchangeItemBulkHelper : ExchangeObjectBase, IExchangeItemBulkHelper
    {
        protected static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private ExchangeService service;
        private ExchangeFolder folder;
        private string tempFolder;

        public ExchangeItemBulkHelper(ExchangeFolder folder, string tempFolder)
            : base(folder.AuthObject)
        {
            this.service = CloneExchangeService(folder.service, 5);
            this.folder = folder;
            //if (firstItem.ItemSize >= 50 * 1024 * 1024)//50M
            //{
            //    //大邮件需要将EnableSeekableResponseStreamCache设置成true, ExportItems方法内部对大文件会有优化处理, 否则会占用大量内存。
            //    largeFile = true;
            //    this.service.EnableSeekableResponseStreamCache = true;
            //    logger.Info("Set EnableSeekableResponseStreamCache=true. Item name: {0}, size: {1}bytes", firstItem.ItemName, firstItem.ItemSize);
            //}
            //InitServiceBinding(fristItem);
            this.tempFolder = tempFolder;
        }

        public ExchangeItemBulkHelper(ExchangeFolder folder)
            : base(folder.AuthObject)
        {
            this.service = CloneExchangeService(folder.service, 5);
            this.folder = folder;
        }

        #region Get
        public void LoadExtendProperties(IEnumerable<ExchangeItem> exchangeItems, params PropertyDefinitionBase[] definitions)
        {
            logger.Info("Start load extend properties for items");
            var items = exchangeItems.Select(r => r.currentItem).ToList();
            if (items.Count > 0)
            {
                var defs = definitions.ToList();
                defs.Add(ItemSchema.Attachments);
                service.LoadPropertiesForItems(items, new PropertySet(BasePropertySet.FirstClassProperties, defs)).GetAwaiter().GetResult();
                //var result = service.BindToItems(itemIds, new PropertySet(BasePropertySet.FirstClassProperties, definitions));
            }
            logger.Info("Finish load extend properties for items");
        }

        public System.Threading.Tasks.Task LoadExtendProperties(IEnumerable<IExchangeItem> items, bool isNullClassification)
        {
            var sensitivityDef = new ExtendedPropertyDefinition(DefaultExtendedPropertySet.InternetHeaders, "msip_labels", MapiPropertyType.String);
            var def = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);
            var ewsItems = items.ConvertAll(item => item as ExchangeItem);
            if (!isNullClassification)
            {
                this.LoadExtendProperties(ewsItems, def, sensitivityDef);
            }
            else
            {
                this.LoadExtendProperties(ewsItems, def);
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }
        #endregion

        #region Update
        //Need to check the retry logic with the BackOffMilliseconds
        public Dictionary<string, UpdateItemResult> BatchAddExtendPorperty(Dictionary<ExchangeItem, string> itemIdAndTermIdMapping, ExtendedPropertyDefinition def)
        {

            var updateResult = new Dictionary<string, UpdateItemResult>();
            try
            {
                var service = CloneExchangeService(this.service, 5);
                var itemIds = itemIdAndTermIdMapping.Keys.Select(a => new ItemId(a.ItemId)).ToList();
                var itemList = new List<Item>();
                var propSet = new PropertySet(BasePropertySet.FirstClassProperties);
                var bindItemsResponse = service.BindToItems(itemIds, propSet).GetAwaiter().GetResult();
                itemList = bindItemsResponse.Select(bindItem => bindItem.Item).ToList();
                //此处认为bind后item顺序和传人顺序相同，之后需要处理bind失败的情况
                //https://docs.microsoft.com/en-us/exchange/client-developer/exchange-web-services/how-to-process-email-messages-in-batches-by-using-ews-in-exchange
                var bindItemIndex = 0;
                itemList.ForEach(item => item.SetExtendedProperty(def, itemIdAndTermIdMapping.Values.ToList()[bindItemIndex++]));
                
                ServiceResponseCollection<UpdateItemResponse> items = null;
                try
                {
                    items = service.UpdateItems(itemList, folder.FolderId, ConflictResolutionMode.AlwaysOverwrite, MessageDisposition.SaveOnly, SendInvitationsOrCancellationsMode.SendToNone).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    logger.Error($"Error in update items, reason : {ex.ToString()}.");
                }

                var index = 0;
                AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(items);
                foreach (var item in items)
                {
                    var id = itemIds[index].ToString();
                    //var result = AssemblyOneItem(item, itemList);
                    if (item.Result != ServiceResult.Success)
                    {
                        var result = UpdateItemResult.CreateFailedResult(id, item.ErrorMessage, item.ErrorCode);

                        updateResult.Add(id, result);
                    }
                    else
                    {
                        var result = UpdateItemResult.CreateSuccessfulResult(id);
                        updateResult.Add(id, result);
                    }
                    ++index;
                }

            }
            catch (Exception ex)
            {
                logger.Error("Failed to update items, error: {0}.", ex.ToString());
                Thread.Sleep(GetWaitTime(ex));
                Thread.Sleep(10 * 1000);
            }

            return updateResult;

        }

        public Dictionary<string, UpdateItemResult> BatchUpdateExchangeItem(List<ExchangeItem> exchangeItems)
        {
            using (PerformanceScope scope = new PerformanceScope("ExchangeItemBulkHelper.BatchUpdateExchangeItem", addToStatistics: true))
            {
                var updateResult = new Dictionary<string, UpdateItemResult>();
                try
                {
                    var service = CloneExchangeService(this.service, 5);

                    var itemList = exchangeItems.Select(r => r.currentItem).ToList();

                    var itemIds = itemList.Select(r => r.Id).ToList();

                    ServiceResponseCollection<UpdateItemResponse> items = null;
                    try
                    {
                        using (PerformanceScope scope0 = new PerformanceScope("ExchangeItemBulkHelper.UpdateItems", $"ExchangeItemBulkHelper.UpdateItems.Count:{exchangeItems.Count}", true))
                        {
                            items = service.UpdateItems(itemList, folder.FolderId, ConflictResolutionMode.AlwaysOverwrite, MessageDisposition.SaveOnly, SendInvitationsOrCancellationsMode.SendToNone).GetAwaiter().GetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error in update items, reason : {ex.ToString()}.");
                    }

                    var index = 0;
                    AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(items);
                    foreach (var item in items)
                    {
                        var id = itemIds[index].ToString();
                        //var result = AssemblyOneItem(item, itemList);
                        using (PerformanceScope scope0 = new PerformanceScope("ExchangeItemBulkHelper.CreateResult", addToStatistics: true))
                        {
                            if (item.Result != ServiceResult.Success)
                            {
                                var result = UpdateItemResult.CreateFailedResult(id, item.ErrorMessage, item.ErrorCode);

                                updateResult.Add(id, result);
                            }
                            else
                            {
                                var result = UpdateItemResult.CreateSuccessfulResult(id);
                                updateResult.Add(id, result);
                            }
                        }
                        ++index;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to update items, error: {0}", ex);
                    Thread.Sleep(GetWaitTime(ex));
                    Thread.Sleep(10 * 1000);
                }
                return updateResult;
            }
        }

        public Dictionary<string, UpdateItemResult> BatchRemoveExtendPorperty(List<ExchangeItem> exchangeItems, ExtendedPropertyDefinition def)
        {

            var updateResult = new Dictionary<string, UpdateItemResult>();
            try
            {
                var service = CloneExchangeService(this.service, 5);
                var itemIds = exchangeItems.Select(a => new ItemId(a.ItemId)).ToList();
                var itemList = new List<Item>();
                var propSet = new PropertySet(BasePropertySet.FirstClassProperties, def);

                var bindItemsResponse = service.BindToItems(itemIds, propSet).GetAwaiter().GetResult();
                itemList = bindItemsResponse.Select(bindItem => bindItem.Item).ToList();
                //此处认为bind后item顺序和传人顺序相同，之后需要处理bind失败的情况
                //https://docs.microsoft.com/en-us/exchange/client-developer/exchange-web-services/how-to-process-email-messages-in-batches-by-using-ews-in-exchange
                var bindItemIndex = 0;
                itemList = itemList.Where(item => item.RemoveExtendedProperty(def)).ToList();
                ServiceResponseCollection<UpdateItemResponse> items = null;
                try
                {
                    items = service.UpdateItems(itemList, folder.FolderId, ConflictResolutionMode.AlwaysOverwrite, MessageDisposition.SaveOnly, SendInvitationsOrCancellationsMode.SendToNone).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    logger.Error($"Error in update items, reason : {ex.ToString()}.");
                }

                var index = 0;
                AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(items);
                foreach (var item in items)
                {
                    var id = itemIds[index].ToString();
                    //var result = AssemblyOneItem(item, itemList);
                    if (item.Result != ServiceResult.Success)
                    {
                        var result = UpdateItemResult.CreateFailedResult(id, item.ErrorMessage, item.ErrorCode);

                        updateResult.Add(id, result);
                    }
                    else
                    {
                        var result = UpdateItemResult.CreateSuccessfulResult(id);
                        updateResult.Add(id, result);
                    }
                    ++index;
                }

            }
            catch (Exception ex)
            {
                logger.Error("Failed to update items, error: {0}.", ex.ToString());
                Thread.Sleep(GetWaitTime(ex));
                Thread.Sleep(10 * 1000);
            }

            return updateResult;

        }
        #endregion



        private int GetWaitTime(Exception ex)
        {
            if (ex == null) return ServiceErrorExtension.DefaultBackOffMilliseconds;
            var sbEx = ex as ServerBusyException;
            if (sbEx != null) return sbEx.BackOffMilliseconds;
            var srEx = ex as ServiceResponseException;
            if (srEx != null) return srEx.ErrorCode.BackOffMilliseconds();
            //var soapEx = ex as SoapException;
            //if (soapEx != null) return soapEx.GetErrorCode().BackOffMilliseconds();

            return ServiceErrorExtension.DefaultBackOffMilliseconds;
        }

        public Dictionary<string, ExchangeUpdateItemResult> BatchAddExtendProperty(Dictionary<IExchangeItem, string> itemIdAndTermIdMapping, string folderId, string mailboxId, ExtendedPropertyDefinition extendProperties)
        {
            Dictionary<ExchangeItem, string> exchangeItemMapping =
                itemIdAndTermIdMapping.Where(kvp => kvp.Key is ExchangeItem)
                .ToDictionary(
                    kvp => (ExchangeItem)kvp.Key,
                    kvp => kvp.Value);
            if (!exchangeItemMapping.Any())
            {
                return new Dictionary<string, ExchangeUpdateItemResult>();
            }

            var result = this.BatchAddExtendPorperty(exchangeItemMapping, extendProperties);
            return result.ToDictionary(kvp => kvp.Key, kvp => new ExchangeUpdateItemResult
            {
                Id = kvp.Value.Id,
                ErrorCode = kvp.Value?.ErrorCode.ToString(),
                ErrorMessage = kvp.Value?.ErrorMessage
            });
        }

        public Dictionary<string, ExchangeUpdateItemResult> BatchRemoveExtendProperty(List<IExchangeItem> exchangeItems, string folderId, string mailboxId, ExtendedPropertyDefinition extendProperties)
        {
            var ewsItems = exchangeItems.ConvertAll(item => item as ExchangeItem);

            var result = this.BatchRemoveExtendPorperty(ewsItems, extendProperties);
            return result.ToDictionary(kvp => kvp.Key, kvp => new ExchangeUpdateItemResult
            {
                Id = kvp.Value.Id,
                ErrorCode = kvp.Value?.ErrorCode.ToString(),
                ErrorMessage = kvp.Value?.ErrorMessage
            });
        }

        public Dictionary<string, ExchangeUpdateItemResult> BatchUpdateExchangeItem(IEnumerable<IExchangeItem> exchangeItems)
        {
            var result = this.BatchUpdateExchangeItem(exchangeItems.ConvertAll(item => item as ExchangeItem).ToList());
            return result.ToDictionary(kvp => kvp.Key, kvp => new ExchangeUpdateItemResult
            {
                Id = kvp.Value.Id,
                ErrorCode = kvp.Value?.ErrorCode.ToString(),
                ErrorMessage = kvp.Value?.ErrorMessage
            });
        }

        [Serializable]
        class ExportItemsException : EWSRetryException
        {
            public ExportItemsException(string message, Exception inner) : base(message, inner) { }

            public ExportItemsException(string message, ServiceError errorCode)
                : base(message)
            {
                this.ErrorCode = errorCode;
                this.BackOffMilliseconds = this.ErrorCode.BackOffMilliseconds();
            }
            protected ExportItemsException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context)
                : base(info, context)
            { }


            public ServiceError ErrorCode { get; private set; }

            protected override int GetWaitTime(Exception ex)
            {
                if (ex == null) return ServiceErrorExtension.DefaultBackOffMilliseconds;
                var baseValue = base.GetWaitTime(ex);
                return baseValue > 0 ? baseValue : 60000;//原有逻辑对于非SoapException等待时间即为60s, 暂时保留此逻辑。
            }
        }

    }

    public class ExportItemResult
    {
        public string Id { get; private set; }
        public string TempFilePath { get; private set; }
        public string ErrorMessage { get; private set; }
        public ServiceError ErrorCode { get; private set; }
        public bool Error
        {
            get { return string.IsNullOrEmpty(this.TempFilePath); }
        }

        public bool SkippedError
        {
            get { return this.Error && this.ErrorCode == ServiceError.ErrorItemNotFound; }
        }

        public long Size
        {
            get
            {
                if (this.Error) return 0L;
                return new FileInfo(this.TempFilePath).Length;
            }
        }

        private ExportItemResult() { }

        public static ExportItemResult CreateSuccessfulResult(string id, string tempFilePath)
        {
            return new ExportItemResult() { Id = id, TempFilePath = tempFilePath, };
        }
        public static ExportItemResult CreateFailedResult(string id, string error, ServiceError errorCode)
        {
            return new ExportItemResult()
            {
                Id = id,
                ErrorMessage = string.Format("Error code: {0}.{1}{2}", errorCode, Environment.NewLine, error),
                ErrorCode = errorCode
            };
        }
        public static ExportItemResult CreateFailedResult(string id, string error)
        {
            return new ExportItemResult()
            {
                Id = id,
                ErrorMessage = error,
            };
        }
    }

    public class UpdateItemResult
    {
        public string Id { get; private set; }
        public string ErrorMessage { get; private set; }
        public ServiceError ErrorCode { get; private set; }
        public bool IsFailed
        {
            get { return !string.IsNullOrEmpty(this.ErrorMessage); }
        }
        private UpdateItemResult() { }
        public static UpdateItemResult CreateSuccessfulResult(string id)
        {
            return new UpdateItemResult() { Id = id };
        }
        public static UpdateItemResult CreateFailedResult(string id, string error, ServiceError errorCode)
        {
            return new UpdateItemResult()
            {
                Id = id,
                ErrorMessage = string.Format("Error code: {0}.{1}{2}", errorCode, Environment.NewLine, error),
                ErrorCode = errorCode
            };
        }
        public static UpdateItemResult CreateFailedResult(string id, string error)
        {
            return new UpdateItemResult()
            {
                Id = id,
                ErrorMessage = error,
            };
        }
    }

    static class DictionaryExtension
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
