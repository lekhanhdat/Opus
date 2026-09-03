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

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using Microsoft365.Graph.Service.ImportItems;


//namespace ExchangeRestoreUtility;

//public class ExchangeGraphBulkHelper : ExchangeBulkHelper
//{
//    private static List<string> needUpdateItemIds = new List<string>();
//    public ExchangeGraphBulkHelper(ExchangeGraphFolder graphFolder) : base(graphFolder)
//    {
//    }

//    public Dictionary<string, ExportAndImportItemResult> ImportItems(Dictionary<string, Stream> importItemsDic)
//    {
//        var needAddItem = new Dictionary<string, Stream>();
//        var needUpdateItem = new Dictionary<string, GraphUpdateMailItemParameter>();
//        foreach (var item in importItemsDic)
//        {
//            if (needUpdateItemIds.Contains(item.Key))
//            {
//                needUpdateItem[item.Key] = new GraphUpdateMailItemParameter
//                {
//                    ItemId = item.Key.Replace('+', '_').Replace('/', '-'),
//                    ChangeKey = ItemIdChangeKeyDic[item.Key],
//                    Data = item.Value
//                };
//            }
//            else
//            {
//                needAddItem.Add(item.Key, item.Value);
//            }
//        }

//        var addResult = ImportItems(needAddItem, (dataList, ids) =>
//        {
//            return GraphImportItemsInternal(dataList, ids, item =>
//            {
//                return (parentFolder as ExchangeGraphFolder).ImportItem(item);
//            });
//        });

//        var updateResult = ImportItems(needUpdateItem, (dataList, ids) =>
//        {
//            return GraphImportItemsInternal(dataList, ids, item =>
//            {
//                return (parentFolder as ExchangeGraphFolder).UpdateItem(item.ItemId, item.ChangeKey, item.Data);
//            });
//        });

//        needUpdateItemIds.Clear();
//        var result = addResult.Concat(updateResult).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

//        return result;
//    }

//    public override void DeleteItemSimple(List<string> deleteItemUniqueIds)
//    {
//        needUpdateItemIds = deleteItemUniqueIds;
//    }

//    private (Dictionary<string, ExportAndImportItemResult>, Dictionary<string, ExportAndImportItemResult>) GraphImportItemsInternal<T>(IEnumerable<T> uploadItems, List<string> ids, Func<T, ImportItemResponse> importAction)
//    {
//        var successfulResult = new Dictionary<string, ExportAndImportItemResult>();
//        var failedResult = new Dictionary<string, ExportAndImportItemResult>();
//        var index = 0;
//        uploadItems.ForEach(i =>
//        {
//            try
//            {
//                var item = importAction(i);
//                HandleImportResult(ids[index], index++, item?.ItemId.IsNotNullOrEmpty() ?? false);
//            }
//            catch (Exception ex)
//            {
//                HandleImportResult(ids[index], index++, false, ex);
//            }
//        });
//        return (successfulResult, failedResult);

//        void HandleImportResult(string id, int index, bool success, Exception ex = null)
//        {
//            if (success)
//            {
//                successfulResult.Add(id, ExportAndImportItemResult.CreateSuccessfulResult(id));
//            }
//            else
//            {
//                failedResult.Add(id, ExportAndImportItemResult.CreateFailedResult(id, ex.Message));
//            }
//        }
//    }
//}

//public class GraphUpdateMailItemParameter
//{
//    public string ItemId { get; set; }

//    public string ChangeKey { get; set; }

//    public Stream Data { get; set; }
//}