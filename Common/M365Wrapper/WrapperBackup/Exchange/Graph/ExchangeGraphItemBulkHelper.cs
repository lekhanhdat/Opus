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
using AvePoint.GCommon.Contract.StorageOptimization.Connector.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using ExchangeUtility.Graph;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Graph;
using Microsoft.Graph.Beta.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft365.Graph.Extensions;
using Microsoft365.Graph.Service;
using Microsoft365.Graph.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Util.MSAzure;
using ExtendedPropertyDefinition = Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition;
using MessageV1 = Microsoft.Graph.Models.Message;
using SingleValueLegacyExtendedPropertyV1 = Microsoft.Graph.Models.SingleValueLegacyExtendedProperty;
using Task = System.Threading.Tasks.Task;


namespace ExchangeBackupUtility.Graph;

public class ExchangeGraphItemBulkHelper : IExchangeItemBulkHelper
{
    private static readonly RALogger logger = RALogger.GetInstance(typeof(ExchangeGraphItemBulkHelper));

    private readonly string _mailboxId;
    private readonly string _folderId;
    private readonly GraphService _service;

    public ExchangeGraphItemBulkHelper(string mailboxId, string folderId, IAuthObject authObj)
    {
        _mailboxId = mailboxId;
        _folderId = folderId.ToRestId();
        var tempAuthObj = authObj as AOSTokenAuthObjectV2;
        var baseUrl = Endpoints.GetEndpoints(tempAuthObj.CloudType).MicrosoftGraph;
        _service = new GraphService(baseUrl, tempAuthObj.TokenProvider);;
    }

    public Dictionary<string, ExchangeUpdateItemResult> BatchAddExtendProperty(Dictionary<IExchangeItem, string> itemIdAndTermIdMapping, string folderId, string mailboxId, ExtendedPropertyDefinition extendProperties)
    {
        var updateResult = new Dictionary<string, ExchangeUpdateItemResult>();
        var updateMapping = new Dictionary<string, MessageV1>();
        Dictionary<string, MessageV1> items = null;

        try
        {
            foreach (var mapping in itemIdAndTermIdMapping)
            {
                var messageId = mapping.Key.ItemId;
                var termIdValue = mapping.Value;
                var messageUpdate = new MessageV1
                {
                    SingleValueExtendedProperties = new List<SingleValueLegacyExtendedPropertyV1>
                    {
                            new SingleValueLegacyExtendedPropertyV1
                            {
                                Id = extendProperties.ToGraphExtendedPropId(),
                                Value = termIdValue
                            }
                    }
                };
                updateMapping.Add(messageId.ToRestId(), messageUpdate);
                logger.Info($"Update Message to message Id {messageId}, TermId: {termIdValue}");
            }
            items = _service.Mails.BatchUpdateMessagesAsync(mailboxId, folderId.ToRestId(), updateMapping).ExecuteAsyncTask();
        }
        catch (Exception ex)
        {
            logger.Error($"Error in update items, reason : {ex.ToString()}.");
        }
        AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(items);
        foreach (var item in items)
        {
            string id = item.Key;
            MessageV1 message = item.Value;
            bool isSuccess = message.AdditionalData != null &&
                     message.AdditionalData.TryGetValue("Result", out var res) &&
                     res?.ToString() == ExchangeGraphServiceResult.Success.ToString();

            if (isSuccess)
            {
                updateResult.Add(id, ExchangeUpdateItemResult.CreateSuccessfulResult(id));
                logger.Info($"apply term successfull to message id {id}");
            }
            else
            {
                message.AdditionalData.TryGetValue("ErrorMessage", out var errorMsg);
                message.AdditionalData.TryGetValue("ErrorCode", out var errorCode);
                message.AdditionalData.TryGetValue("Code", out var httpCode);

                string finalErrorCode = errorCode?.ToString()
                                     ?? httpCode?.ToString()
                                     ?? ResultMessageType.UnknownError.ToString();

                //string finalErrorMessage = errorMsg?.ToString()
                //                        ?? (httpCode != null ? $"HTTP Error {httpCode}" : "Request failed");

                string finalErrorMessage = "RM_Connector_InsertDatasFailed";

                var failedResult = ExchangeUpdateItemResult.CreateFailedResult(id, finalErrorMessage);
                updateResult.Add(id, failedResult);
                logger.Info($"apply term fail to message id {id}. Error code: {finalErrorCode}");
            }
        }
        return updateResult;
    }

    public Dictionary<string, ExchangeUpdateItemResult> BatchRemoveExtendProperty(List<IExchangeItem> exchangeItems, string folderId, string mailboxId, ExtendedPropertyDefinition extendProperties)
    {
        var updateResult = new Dictionary<string, ExchangeUpdateItemResult>();
        var updateMapping = new Dictionary<string, MessageV1>();
        Dictionary<string, MessageV1> items = null;

        try
        {
            var messageUpdate = new MessageV1
            {
                SingleValueExtendedProperties = new List<SingleValueLegacyExtendedPropertyV1>
                    {
                            new SingleValueLegacyExtendedPropertyV1
                            {
                                Id = extendProperties.ToGraphExtendedPropId(),
                                AdditionalData = new Dictionary<string, object> 
                                { 
                                    { "value", null } 
                                }
                            }
                    }
            };
            foreach (var item in exchangeItems)
            {
                updateMapping[item.ItemId.ToRestId()] = messageUpdate;
            }

            items = _service.Mails.BatchUpdateMessagesAsync(mailboxId, folderId.ToRestId(), updateMapping).ExecuteAsyncTask();
        }
        catch (Exception ex)
        {
            logger.Error($"Error in update items, reason : {ex.ToString()}.");
        }

        AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(items);

        foreach (var item in items)
        {
            string id = item.Key;
            MessageV1 message = item.Value;
            bool isSuccess = message.AdditionalData != null &&
                     message.AdditionalData.TryGetValue("Result", out var res) &&
                     res?.ToString() == ExchangeGraphServiceResult.Success.ToString();

            if (isSuccess)
            {
                updateResult.Add(id, ExchangeUpdateItemResult.CreateSuccessfulResult(id));
                logger.Info($"remove term successfull to message id {id}");
            }
            else
            {
                message.AdditionalData.TryGetValue("ErrorMessage", out var errorMsg);
                message.AdditionalData.TryGetValue("ErrorCode", out var errorCode);
                message.AdditionalData.TryGetValue("Code", out var httpCode);

                string finalErrorCode = errorCode?.ToString()
                                     ?? httpCode?.ToString()
                                     ?? ResultMessageType.UnknownError.ToString();

                string finalErrorMessage = errorMsg?.ToString()
                                        ?? (httpCode != null ? $"HTTP Error {httpCode}" : "Request failed");

                var failedResult = ExchangeUpdateItemResult.CreateFailedResult(id, finalErrorMessage, finalErrorCode);
                updateResult.Add(id, failedResult);
                logger.Info($"remove term fail to message id {id}. Error code: {finalErrorCode}");
            }
        }
        return updateResult;
    }

    public Dictionary<string, ExchangeUpdateItemResult> BatchUpdateExchangeItem(IEnumerable<IExchangeItem> exchangeItems)
    {
        Dictionary<string, string> requestIds = [];
        Dictionary<string, Dictionary<string, string>> requestInfo = [];

        foreach (var item in exchangeItems)
        {
            var mappingId = Guid.NewGuid().ToString();
            requestIds.Add(mappingId, item.ItemId);

            var props = item.GetExtendedProperties();
            props.Add("mailboxId", _mailboxId);
            props.Add("folderId", _folderId);
            props.Add("itemId", item.ItemId.ToRestId());
            props.Add("itemType", item.ItemType);
            requestInfo.Add(mappingId, props);
        }

        var responseList = _service.Users.ProcessBatchAsync(requestInfo);
        var updateResult = AddBatchResultAsync(responseList, requestIds).ExecuteAsyncTask();
        return updateResult;
    }

    private async Task<Dictionary<string, ExchangeUpdateItemResult>> AddBatchResultAsync(IAsyncEnumerable<BatchResponseResult> responseItems, Dictionary<string, string> requestIds)
    {
        var updateResult = new Dictionary<string, ExchangeUpdateItemResult>();
        await foreach (var response in responseItems)
        {
            var itemId = requestIds.GetValue(response.RequestId);
            if (!response.IsSuccessStatusCode)
            {
                var result = ExchangeUpdateItemResult.CreateFailedResult(itemId, "RM_Connector_InsertDatasFailed");
                updateResult.Add(itemId, result);
            }
            else
            {
                var result = ExchangeUpdateItemResult.CreateSuccessfulResult(itemId);
                updateResult.Add(itemId, result);
            }
        }
        return updateResult;
    }

    public async Task LoadExtendProperties(IEnumerable<IExchangeItem> items, bool isNullClassification)
    {
        
    }

    public void LoadExtendProperties(IEnumerable<IExchangeItem> items, params IMapiExtendedPropertyDefinition[] propertyDefinitions)
    {
        logger.Info($"Start load extend properties for graph items count:{items.Count()}, folder id:{_folderId}, mailboxId:{_mailboxId}");
        if (!items.Any())
        {
            logger.Info("No items to load extend properties");
            return;
        }
        var itemIds = items.Select(i => i.ItemId.ToRestId()).ToList();

        var batchResponse = _service.Mails.LoadExtendPropertiesAsync(_mailboxId, _folderId, itemIds, propertyDefinitions.ToGraphSingleValueExpandString()).ExecuteAsyncTask();

        foreach (var item in items)
        {
            var tempItem = batchResponse.GetResponseByIdAsync<MailboxItem>(item.ItemId.ToRestId()).ExecuteAsyncTask();
            if (tempItem is not null)
            {
                if (tempItem.SingleValueExtendedProperties.IsNotNullOrEmpty())
                {
                    foreach (var prop in tempItem.SingleValueExtendedProperties)
                        item.SetExtendProperty(prop.Id, prop.Value);
                }
            }
        }
        logger.Info($"Finish load extend properties for items , folder id:{_folderId}, mailboxId:{_mailboxId}");
    }
}
