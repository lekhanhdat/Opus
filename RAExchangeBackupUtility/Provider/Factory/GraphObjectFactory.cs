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
using ExchangeBackupUtility.Graph;
using ExchangeUtility.Graph;
using Microsoft365.Graph.Service;
using System;
using Util.MSAzure;

namespace ExchangeBackupUtility
{
    public class GraphObjectFactory : IExchangeObjectFactory
    {
        public IExchangeItemExporter CreateExItemBulkHelper(IExchangeItem item, string cachePath)
        {
            var graphItem = item as ExchangeGraphItem ?? throw new NotSupportedException($"Item type not supported: {item.GetType()}");
            if (item.ItemSize > 870400)
            {
                return new ExchangeGraphItemExporter(graphItem.MailboxId, graphItem.Service, cachePath);
            }
            return new ExchangeGraphItemBulkExporter(graphItem.MailboxId, graphItem.Service);
        }

        public IExchangeFolder CreateFolder(ExchangeMailbox mailbox, string folderId, IAuthObject authObj)
        {
            var service = GetOrCreateGraphService(authObj);
            return new ExchangeGraphFolder(mailbox, folderId, authObj, service);
        }

        public IExchangeRootFolder CreateRootFolder(ExchangeMailbox mailbox, IAuthObject authObj)
        {
            var service = GetOrCreateGraphService(authObj);
            return new ExchangeGraphRootFolder(mailbox, authObj, service);
        }

        public IRecoverableItemsRoot CreateRecoverableItemsRoot(ExchangeMailbox mailbox, IAuthObject authObj)
        {
            var service = GetOrCreateGraphService(authObj);
            throw new NotImplementedException();
        }

        public IRecoverableItemsRoot CreateArchiveRecoverableItemsRoot(ExchangeMailbox mailbox, IAuthObject authObj)
        {
            var service = GetOrCreateGraphService(authObj);
            throw new NotImplementedException();
        }

        GraphService GetOrCreateGraphService(IAuthObject authObj)
        {
            var tempAuthObj = authObj as AOSTokenAuthObjectV2;
            if (tempAuthObj == null)
                throw new NotSupportedException($"AuthObj type not supported: {authObj.GetType()}");
            var baseUrl = Endpoints.GetEndpoints(tempAuthObj.CloudType).MicrosoftGraph;
            return new GraphService(baseUrl, tempAuthObj.TokenProvider);
        }
    }
}