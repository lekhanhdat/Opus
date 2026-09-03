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
using AvePoint.RA.CommonUtil;
using Microsoft.Exchange.WebServices.Data;

namespace ExchangeUtility.Graph
{
    public class ExchangeMailbox
    {
        private static RALogger logger = RALogger.GetInstance(typeof(ExchangeMailbox));

        public string OriginalMailboxAddress { get; private set; }

        public string MailboxAddress { get; private set; }

        public bool IsArchiveMailbox { get; private set; }

        public bool IsResourceMailbox { get; private set; }

        public bool IsRecoverableItemsMailbox { get; private set; }

        public bool IsArchiveRecoverableItemsMailbox { get; private set; }

        public bool IsResourceRecoverableItemsMailbox { get; private set; }

        public bool IsPublicFolder { get { return (this.MailboxType == ExchangeMailboxType.PublicFolder || this.MailboxType == ExchangeMailboxType.PublicFolderMetadata); } }

        public ExchangeMailboxType MailboxType { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public WellKnownFolderName MsgFolderRoot
        {
            get
            {
                switch (this.MailboxType)
                {
                    case ExchangeMailboxType.PublicFolder:
                        return WellKnownFolderName.PublicFoldersRoot;

                    default:
                        return GetRootFolderName();
                        //return this.IsArchiveMailbox ?
                        //    WellKnownFolderName.ArchiveMsgFolderRoot : (this.IsRecoverableItemsMailbox ? WellKnownFolderName.RecoverableItemsRoot : WellKnownFolderName.MsgFolderRoot);
                }
            }
        }

        private WellKnownFolderName GetRootFolderName()
        {
            if (this.IsArchiveMailbox)
            {
                return WellKnownFolderName.ArchiveMsgFolderRoot;
            }
            else if (this.IsArchiveRecoverableItemsMailbox)
            {
                return WellKnownFolderName.ArchiveRecoverableItemsRoot;
            }
            else if (this.IsRecoverableItemsMailbox || this.IsResourceRecoverableItemsMailbox)
            {
                return WellKnownFolderName.RecoverableItemsRoot;
            }
            else
            {
                return WellKnownFolderName.MsgFolderRoot;
            }
        }
        public FolderId RootFolderId
        {
            get
            {
                return new FolderId(
                    this.MsgFolderRoot,
                    new Mailbox()
                    {
                        Address = this.MailboxAddress
                    });
            }
        }

        public string ObjectId { get; set; }

        public ExchangeMailbox(string originalMailAddress, ExchangeMailboxType type, string objectId) : this(originalMailAddress, type)
        {
            ObjectId = objectId;
        }

        public ExchangeMailbox(string originalMailAddress, ExchangeMailboxType type)
        {
            if (originalMailAddress == null) throw new ArgumentNullException("originalMailAddress");

            this.OriginalMailboxAddress = originalMailAddress;

            this.MailboxType = type;
            string mailboxAddress = originalMailAddress;
            if (IsArchiveMailboxAddress(originalMailAddress, out mailboxAddress))
            {
                this.IsArchiveMailbox = true;
            }
            else if (IsResourceMailboxAddress(originalMailAddress, out mailboxAddress))
            {
                this.IsResourceMailbox = true;
            }
            else if (IsRecoverableItemsMailboxAddress(originalMailAddress, out mailboxAddress))
            {
                this.IsRecoverableItemsMailbox = true;
                ExchangeGlobalConfig.IsRecoverableItemsMailbox = true;
            }
            else if (IsArchiveRecoverableItemsMailboxAddress(originalMailAddress, out mailboxAddress))
            {
                this.IsArchiveRecoverableItemsMailbox = true;
                ExchangeGlobalConfig.IsRecoverableItemsMailbox = true;
            }
            else if (IsResourceRecoverableItemsMailboxAddress(originalMailAddress, out mailboxAddress))
            {
                this.IsResourceRecoverableItemsMailbox = true;
                ExchangeGlobalConfig.IsRecoverableItemsMailbox = true;
            }
            this.MailboxAddress = mailboxAddress;
        }

        public static bool IsArchiveMailboxAddress(string originalMailboxAddress, out string mailboxAddress)
        {
            mailboxAddress = originalMailboxAddress;

            int index = originalMailboxAddress.LastIndexOf(string.Format("({0})", ExchangeConstants.InPlaceArchiveMailbox));
            if (index > 0)
            {
                mailboxAddress = originalMailboxAddress.Substring(0, index);
                return true;
            }
            return false;
        }

        private bool IsResourceMailboxAddress(string originalMailboxAddress, out string mailboxAddress)
        {
            mailboxAddress = originalMailboxAddress;

            int index = originalMailboxAddress.LastIndexOf(string.Format("({0})", ExchangeConstants.ResourceMailbox));
            if (index > 0)
            {
                mailboxAddress = originalMailboxAddress.Substring(0, index);
                return true;
            }
            return false;
        }

        private bool IsRecoverableItemsMailboxAddress(string originalMailboxAddress, out string mailboxAddress)
        {
            mailboxAddress = originalMailboxAddress;

            int index = originalMailboxAddress.LastIndexOf(string.Format("({0})", ExchangeConstants.RecoverableItemsMailbox));
            if (index > 0)
            {
                mailboxAddress = originalMailboxAddress.Substring(0, index);
                return true;
            }
            return false;
        }

        private bool IsArchiveRecoverableItemsMailboxAddress(string originalMailboxAddress, out string mailboxAddress)
        {
            mailboxAddress = originalMailboxAddress;

            int index = originalMailboxAddress.LastIndexOf(string.Format("({0})", ExchangeConstants.Archive_RecoverableItemsMailbox));
            if (index > 0)
            {
                mailboxAddress = originalMailboxAddress.Substring(0, index);
                return true;
            }
            return false;
        }

        private bool IsResourceRecoverableItemsMailboxAddress(string originalMailboxAddress, out string mailboxAddress)
        {
            mailboxAddress = originalMailboxAddress;

            int index = originalMailboxAddress.LastIndexOf(string.Format("({0})", ExchangeConstants.Resource_RecoverableItemsMailbox));
            if (index > 0)
            {
                mailboxAddress = originalMailboxAddress.Substring(0, index);
                return true;
            }
            return false;
        }


        public static string DecodeEmailAddress(string old)
        {
            return new ExchangeMailbox(old, ExchangeMailboxType.None).MailboxAddress;
        }
    }

    public enum ExchangeMailboxType
    {
        None = 0,
        PublicFolder = 1,
        User = 2,
        Group = 3,
        PublicFolderMetadata = 4,
        Teams = 5,
        Yammer = 6,
    }
}