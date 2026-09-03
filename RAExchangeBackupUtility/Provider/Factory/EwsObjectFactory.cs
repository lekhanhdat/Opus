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
using System;

using ExchangeAuthObj = ExchangeUtility.AuthObject;
using ExchangeMailboxType = ExchangeUtility.ExchangeMailboxType;

namespace ExchangeBackupUtility
{
    public class EwsObjectFactory : IExchangeObjectFactory
    {
        public IExchangeItemExporter CreateExItemBulkHelper(IExchangeItem item, string cachePath)
        {
            return new ExchangeItemExporter(item as Graph.ExchangeItem, cachePath);
        }

        public IExchangeFolder CreateFolder(ExchangeMailbox mailbox, string folderId, IAuthObject authObj)
        {
            return new ExchangeFolder(
                new ExchangeUtility.ExchangeMailbox(mailbox.MailboxAddress, Enum.Parse<ExchangeMailboxType>(mailbox.MailboxType.ToString())),
                folderId,
                authObj as ExchangeAuthObj);
        }

        public IExchangeRootFolder CreateRootFolder(ExchangeMailbox mailbox, IAuthObject authObj)
        {
            return new ExchangeRootFolder(
                new ExchangeUtility.ExchangeMailbox(mailbox.OriginalMailboxAddress, Enum.Parse<ExchangeMailboxType>(mailbox.MailboxType.ToString())),
                authObj as ExchangeAuthObj);
        }

        public IRecoverableItemsRoot CreateRecoverableItemsRoot(ExchangeMailbox mailbox, IAuthObject authObj)
        {
            return new RecoverableItemsRoot(mailbox, authObj as IEWSAuthObject);
        }

        public IRecoverableItemsRoot CreateArchiveRecoverableItemsRoot(ExchangeMailbox mailbox, IAuthObject authObj)
        {
            return new ArchiveRecoverableItemsRoot(mailbox, authObj as IEWSAuthObject);
        }
    }
}