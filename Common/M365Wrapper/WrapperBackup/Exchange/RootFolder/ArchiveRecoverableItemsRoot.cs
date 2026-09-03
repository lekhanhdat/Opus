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


namespace ExchangeBackupUtility.Graph
{
    #region directory

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ExchangeCommonWrapper;
    using ExchangeUtility.Graph;
    using Microsoft.Exchange.WebServices.Data;

    using Folder = Microsoft.Exchange.WebServices.Data.Folder;

    #endregion

    public class ArchiveRecoverableItemsRoot : RecoverableItemsRoot
    {
        protected override List<WellKnownFolderName> SupportedWellKnownFolderNames
        {
            get
            {
                if (supportedWellKnownFolderNames == null)
                {
                    supportedWellKnownFolderNames = new List<WellKnownFolderName>()
                    {
                        WellKnownFolderName.ArchiveRecoverableItemsRoot,
                        WellKnownFolderName.ArchiveRecoverableItemsDeletions,
                        WellKnownFolderName.ArchiveRecoverableItemsPurges,
                        WellKnownFolderName.ArchiveRecoverableItemsDiscoveryHolds,
                    };
                }
                return supportedWellKnownFolderNames;
            }
        }

        public ArchiveRecoverableItemsRoot(ExchangeMailbox mailbox, IEWSAuthObject authObj) : base(mailbox, authObj)
        {
            this.isRootFolder = true;
        }
        protected override void SetFolderId(ExchangeMailbox mailbox, string folderId)
        {
            this.inputFolderId = new FolderId(WellKnownFolderName.ArchiveRecoverableItemsRoot, new Mailbox(mailbox.MailboxAddress));
        }
        protected override WellKnownFolderName GetVersionsFolder()
        {
            return WellKnownFolderName.ArchiveRecoverableItemsVersions;
        }
    }
}
