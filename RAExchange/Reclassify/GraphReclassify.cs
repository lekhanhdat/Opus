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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RAExchange.Common;
using ExchangeBackupUtility.Graph;
using ExchangeCommonWrapper;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.RAExchange.Reclassify
{
    public class GraphReclassify : IReclassify
    {
        private readonly IRALogger _logger = RALogger.GetInstance(typeof(GraphReclassify));
        private IExchangeFolder _folder;
        private ExchangeGraphItemBulkHelper _bulkHelper;
        private List<Guid> _itemIdsFailed = [];
        public List<Record> ChangeRecordTermAction(ExchangeOnlineTreeNodeDto mailBox, List<Record> records, string termName, Guid termId, ref List<Guid> failedIds, ref int failedCount)
        {
            List<Record> successRecords = [];
            _itemIdsFailed = [];

            try
            {
                InitializeExchangeContext(mailBox);

                var itemIdsFailed = new HashSet<string>();

                foreach (var needUpdateItems in records.Batch(20))
                {
                    var items = GetExchangeItems(needUpdateItems);

                    itemIdsFailed.UnionWith(UpdateItemsTerm(items, termId));
                }

                HandleItemsFailed(records, itemIdsFailed, ref failedIds, ref failedCount);

                successRecords = records.Where(r => !_itemIdsFailed.Contains(r.Id)).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"update item term failed, mailbox is {mailBox.ID}, error is {ex.ToString}");
            }
            finally
            {
                _logger.Info("update item term finish,mailbox is {0}", mailBox.ID);
            }
            return successRecords;
        }

        private void InitializeExchangeContext(ExchangeOnlineTreeNodeDto mailBox)
        {
            var treeManager = new TreeManagement();
            var mailboxGuid = treeManager.GetRealMailboxGuid(mailBox);
            mailBox.UsingModernApp = true;
            _folder = treeManager.GetExchangeFolderFromTreeNodeV2(mailBox, mailboxGuid, true);
            _bulkHelper = new ExchangeGraphItemBulkHelper(mailboxGuid, _folder.FolderId, _folder.GetCredential());
        }

        private List<IExchangeItem> GetExchangeItems(IEnumerable<Record> records)
        {
            _logger.Info("Get items by Graph API");

            var itemEntities = records.Select(r => new FailedItemEntity
            {
                Id = r.ExternalId.ToRestId()
            }).ToList();

            var (itemsSuccessful, itemsFoundButFailed) = _folder.GetItemsByIds(itemEntities);

            var itemIdsSuccessful = itemsSuccessful.Select(x => x.ItemId).ToHashSet();

            _itemIdsFailed.AddRange(records.Where(r => !itemIdsSuccessful.Contains(r.ExternalId)).Select(r => r.Id));

            return itemsSuccessful ?? [];
        }

        private HashSet<string> UpdateItemsTerm(List<IExchangeItem> items, Guid termId)
        {
            var itemMapping = items.ToDictionary(item => item, _ => termId.ToString());

            var propDefinition = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);

            var updateItems = _bulkHelper.BatchAddExtendProperty(itemMapping, _folder.FolderId, _folder.MailBoxId, propDefinition);

            return updateItems.Where(i => i.Value.IsFailed).Select(i => i.Key).ToHashSet();
        }

        private void HandleItemsFailed(List<Record> records, HashSet<string> itemIdsFailed, ref List<Guid> failedIds, ref int failedCount)
        {
            _itemIdsFailed.AddRange(records.Where(r => itemIdsFailed.Contains(r.ExternalId)).Select(r => r.Id).ToList());

            failedIds.AddRange(_itemIdsFailed);

            failedCount += _itemIdsFailed.Count;
        }

        public object GetGroupKey(Record record)
        {
            return new
            {
                record.AveSiteId,
                record.EmailAddress
            };
        }
    }
}
