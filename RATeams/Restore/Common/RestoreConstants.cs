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


namespace Office365GroupRestore
{
    class RestoreConstants
    {
        public const string NOINITIALIZEMESSAGE = "The related object is not initialized.";
        public const string OFFICE365GROUPHELPER = "Office365GroupRestoreHelper";

        public const string MAILBOXHELPERBATCH = "MailboxHelperBatch";
        public const string FOLDERHELPERBATCH = "FolderHelperBatch";
        public const string ITEMHELPERBATCH = "ItemHelperBatch";
        public const string ITEMTOSTORAGEHELPERBATCH = "ItemToStorageHelperBatch";

        public const string PLANHELPERBATCH = "PlanHelperBatch";
        public const string TASKHELPERBATCH = "TaskHelperBatch";

        public const string RESTORE_EXECUTOR_BATCH = "RestoreExecutorBatch";
        public const string RESTORE_TO_STORAGE_EXECUTOR_BATCH = "RestoreToStorageExecutorBatch";

        public const string RESTORE_DATAHANDLER = "RestoreDataHandler";
        public const string RESTORE_DATAHANDLER_BATCH = "RestoreDataHandlerBatch";

        public const int FileHeaderLength = 16;
        public const int CacheCount = 50;
        public const long CacheSize = 5242880;  //5 * 1024 * 1024
        public const long FolderSize = 51200;
        public const int SLEEPTIME = 600000;
        public const string RESTORE_EXECUTOR = "RestoreExecutor";
        public const string RESTORE_CONTROLLER = "RestoreController";
        public const string CONVERT_TYPE_EXCEPTION = "String was not recognized as a valid Boolean.";
        public const string DEVICE_UNAVALIABLE = "This device is not available";
        public const string NO_SMTP_ADDRESS = "The SMTP address has no mailbox associated with it.";
        public const string MAILBOX_DATABASE_UNAVALIABLE = "The mailbox database is temporarily unavailable";
        public const string SERVER_CANNOT_SERVICE_REQUEST = "The server cannot service this request right now";
        public const string MAILBOX_OVERDUE = "An internal server error occurred. The operation failed";
        public const string OBJECT_NONEXIST = "The specified object was not found in the store";
        public const string SET_PREFERRED_DATA_LOCATION_FAILED = "The requesting principal is not authorized to set group preferred data location";
        public const string AUTODISCOVER_RETURN_ERROR = "The Autodiscover service returned an error";
        public const string AUTODISCOVER_CANNOT_BE_LOCATED = "The Autodiscover service couldn't be located";
        public const string ACCOUNT_UNAUTHORIZED = "The request failed. The remote server returned an error: (401) Unauthorized.";
        public const string DATA_ARCHIVED_EXCEPTION = "EOBErrorMessage_DataArchived";
        public const string EXPORT_NOT_SUPPORT_ARCHIVED_DATA = "Media_Report_Export_NotSupportArchivedData";

        public const string DelegatedUserNotFoundKey = "Agent.Exchange.DelegatedUserNotFound_2D9B0A28-0362-4A5C-A26C-7BF0E6FBC477";
    }
}