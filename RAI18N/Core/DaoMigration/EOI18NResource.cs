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

namespace AvePoint.RA.I18N.Core.DaoMigration
{
    public class EOI18NResource
    {
        public static string Execution(string key, params object[] args)
        {
            return GetString(key);
        }

        private const string EOBErrorMessage_AutoDiscoverCannotLocate = "EOBErrorMessage_AutoDiscoverCannotLocate";
        private const string EOBErrorMessage_AutoDiscoverReturnError = "EOBErrorMessage_AutoDiscoverReturnError";
        private const string EOBErrorMessage_CannotExport = "EOBErrorMessage_CannotExport";
        private const string EOBErrorMessage_DatabaseUnavaliable = "EOBErrorMessage_DatabaseUnavaliable";
        private const string EOBErrorMessage_FolderNotExist = "EOBErrorMessage_FolderNotExist";
        private const string EOBErrorMessage_GetContentFailed = "EOBErrorMessage_GetContentFailed";
        private const string EOBErrorMessage_IndexDatabaseDamaged = "EOBErrorMessage_IndexDatabaseDamaged";
        private const string EOBErrorMessage_InvalidRefreshToken = "EOBErrorMessage_InvalidRefreshToken";
        private const string EOBErrorMessage_MailboxFailed = "EOBErrorMessage_MailboxFailed";
        private const string EOBErrorMessage_MailboxNotFound = "EOBErrorMessage_MailboxNotFound";
        private const string EOBErrorMessage_MailboxOverdue = "EOBErrorMessage_MailboxOverdue";
        private const string EOBErrorMessage_Nonexist = "EOBErrorMessage_Nonexist";
        private const string EOBErrorMessage_NoSmtpAddress = "EOBErrorMessage_NoSmtpAddress";
        private const string EOBErrorMessage_ServerBusy = "EOBErrorMessage_ServerBusy";
        private const string EOBErrorMessage_StoragePolicyConnectionException = "ExchangeOnline.Service_BC582D6D-10DE-4E60-9D81-827AAA2EEEA2";
        private const string EOBErrorMessage_Unauthorized = "EOBErrorMessage_Unauthorized";
        private const string EOBFilterResultMessage = "EOBFilterResultMessage";
        private const string EORErrorMessage_FolderMetadata = "EORErrorMessage_FolderMetadata";
        private const string EORErrorMessage_ItemFailed = "EORErrorMessage_ItemFailed";
        private const string EORErrorMessage_ItemMetadata = "EORErrorMessage_ItemMetadata";
        private const string EORErrorMessage_MailboxFailed = "EORErrorMessage_MailboxFailed";
        private const string EORErrorMessage_MailboxMetadata = "EORErrorMessage_MailboxMetadata";
        private const string EORErrorMessage_MetadataIsEmpty = "EORErrorMessage_MetadataIsEmpty";
        private const string EORErrorMessage_NoRootFolder = "EORErrorMessage_NoRootFolder";
        private const string EOBErrorMessage_DecryptKeyNotFound = "EOBErrorMessage_DecryptKeyNotFound";
        private const string EOBErrorMessage_BackupDataPathNotFound = "EOBErrorMessage_BackupDataPathNotFound";
        private const string EORSkipMessage_FolderExist = "EORSkipMessage_FolderExist";
        private const string EORSkipMessage_ItemExist = "EORSkipMessage_ItemExist";
        private const string EORSkipMessage_MailboxExist = "EORSkipMessage_MailboxExist";
        private const string EORSkipMessage_ReceiveIndexFailed = "EORSkipMessage_ReceiveIndexFailed";

        public static string GetString(string comment)
        {
            switch (comment)
            {
                case EOBErrorMessage_AutoDiscoverCannotLocate:
                    return Get("ExchangeOnline.Service_cedc8e8f-6118-4082-9fa0-39c6ff1c0c64", "The network connection is not stable. Or need to configure DNS record for the specified domain.");
                case EOBErrorMessage_AutoDiscoverReturnError:
                    return Get("ExchangeOnline.Service_a4f5d43c-3789-4b56-bb92-75f5e2381dc3", "Cannot use the Service Account to connect to the Exchange Online Server. Service Account may not have a mailbox.");
                case EOBErrorMessage_CannotExport:
                    return Get("ExchangeOnline.Service_822a3821-a070-481a-846e-71d262b52306", "Cannot export the item from the source.");
                case EOBErrorMessage_DatabaseUnavaliable:
                    return Get("ExchangeOnline.Service_12451df7-ecd2-4184-958b-16d15378e34b", "Mailbox database is offline, corrupt, shutting down, or exhibiting other conditions that make the mailbox temporarily unavailable.");
                case EOBErrorMessage_FolderNotExist:
                    return Get("ExchangeOnline.Service_f3c0fbe6-f996-4d61-b0b2-f0af9e5816af", "The folder does not exist.");
                case EOBErrorMessage_GetContentFailed:
                    return Get("ExchangeOnline.Service_ed69df19-83fd-47e0-a4c4-9ef5703163db", "Failed to obtain the item's content information.");
                case EOBErrorMessage_IndexDatabaseDamaged:
                    return Get("ExchangeOnline.Service_7a831596-e0d0-4973-a72b-2635ac30bb24", "The mailbox index database is damaged or contains duplicate records.");
                case EOBErrorMessage_InvalidRefreshToken:
                    return Get("ExchangeOnline.Service_8174dd78-151a-48b1-bdf2-4a4ebe131ead", "Invalid refresh token.");
                case EOBErrorMessage_MailboxFailed:
                    return Get("ExchangeOnline.Service_996084a3-423a-4517-b3ff-d70079721c95", "The SMTP address has no mailbox associated with it.");
                case EOBErrorMessage_MailboxNotFound:
                    return Get("ExchangeOnline.Service_e415063d-fc2a-46af-bb42-1ce0f094b813", "Cannot find the original mailbox. Please check if it is deleted in your destination.");
                case EOBErrorMessage_MailboxOverdue:
                    return Get("ExchangeOnline.Service_61e443b8-b3c7-4b4b-9529-9c3fbad6ecdd", "The specified mailbox may have expired.");
                case EOBErrorMessage_Nonexist:
                    return Get("ExchangeOnline.Service_db631c98-0566-4910-b123-1fafe6da4aef", "The specified object was not found in the store.");
                case EOBErrorMessage_NoSmtpAddress:
                    return Get("ExchangeOnline.Service_001d77dd-5a80-40a0-94e1-2f7e9571a5c8", "Cannot find the mailbox with this e-mail address. The mailbox may have been deleted, or this account may not have a mailbox associated with it.");
                case EOBErrorMessage_ServerBusy:
                    return Get("ExchangeOnline.Service_ae870ca5-03dd-429d-8cf2-c836f5ebe7bc", "Cannot backup the specified data from Exchange Online Server. The server is busy.");
                case EOBErrorMessage_StoragePolicyConnectionException:
                    return Get("ExchangeOnline.Service_BC582D6D-10DE-4E60-9D81-827AAA2EEEA2", "Connecting storage error, please check the storage status.");
                case EOBErrorMessage_Unauthorized:
                    return Get("ExchangeOnline.Service_4634ebbb-af88-45c8-9d6a-1a4f207b4792", "Fail to connect to the mailbox.");
                case EOBFilterResultMessage:
                    return Get("ExchangeOnline.Service_0e9083fc-ebdc-475f-9840-fac4498672c1", "The item does not meet the filter policies.");
                case EORErrorMessage_FolderMetadata:
                    return Get("ExchangeOnline.Service_f37c3d13-4c17-459c-9dd3-3f09f0852742", "Failed to restore the folder. The received metadata information from the Media service does not match the metadata information of the folder type.");
                case EORErrorMessage_ItemFailed:
                    return Get("ExchangeOnline.Service_22ff5ec9-40f6-4eba-8914-0bc389aa68bf", "An occurred while restore item.");
                case EORErrorMessage_ItemMetadata:
                    return Get("ExchangeOnline.Service_75d478e5-b20b-4fd2-a13c-8ddaea1bca8f", "Failed to restore the item. The received metadata information from the Media service does not match the metadata information of the item type.");
                case EORErrorMessage_MailboxFailed:
                    return Get("ExchangeOnline.Service_c402eead-cb97-41d1-ade3-5ea52343d156", "Failed to restore the mailbox.");
                case EORErrorMessage_MailboxMetadata:
                    return Get("ExchangeOnline.Service_78e525f6-f90c-42ef-b36b-27abda18eb55", "Failed to restore the mailbox. The received metadata information from the Media service does not match the metadata information of the mailbox type.");
                case EORErrorMessage_MetadataIsEmpty:
                    return Get("ExchangeOnline.Service_9da0ef32-379f-4ea9-899d-7726d9767ae9", "Failed to restore the object. The received metadata information from the Media service is empty.");
                case EORErrorMessage_NoRootFolder:
                    return Get("ExchangeOnline.Service_5c64e521-e776-4077-b0ee-34dd5d5f4806", "Cannot find the root folder of the mailbox.");
                case EORSkipMessage_FolderExist:
                    return Get("ExchangeOnline.Service_78a728bb-7510-4424-a47a-18e38ff307a3", "This folder already exists in the destination.");
                case EORSkipMessage_ItemExist:
                    return Get("ExchangeOnline.Service_c67c4f2c-7eec-4633-85f4-bbf664e03462", "This item already exists in the destination.");
                case EORSkipMessage_MailboxExist:
                    return Get("ExchangeOnline.Service_093f790b-fc1e-4d7a-8bf9-3f25b85c0e33", "This mailbox already exists in the destination.");
                case EORSkipMessage_ReceiveIndexFailed:
                    return Get("ExchangeOnline.Service_94a2c114-a731-4fb0-ba82-74c0b4d6a175", "Receive index information with exception.");
                case EOBErrorMessage_DecryptKeyNotFound:
                    return Get("ExchangeOnline.Service_6dd41060-9d59-4747-810a-4483a7fb003c", "The decrypt key of restore data is not found. ");
                case EOBErrorMessage_BackupDataPathNotFound:
                    return Get("ExchangeOnline.Service_9b0ab835-78df-4bdc-8960-1670f78a3e42", "The backup datablock path can not be found in storage.");
                default:
                    return string.Empty;
            }
        }

        private static string Get(string key, string defaultValue, params object[] args)
        {
            var value = I18NEntity.GetString(key, args);
            if (string.IsNullOrEmpty(key))
            {
                return defaultValue;
            }
            return value;
        }
    }
}
