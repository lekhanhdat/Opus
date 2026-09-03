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
using System.Collections.Generic;

namespace ExchangeUtility.Graph
{
    public class ExchangeGlobalConfig
    {
        static ExchangeGlobalConfig()
        {
            try
            {
                
                //studo:ProductVersion = ServiceVersionHelper.GetVersion().CloudBackupVersion; 
            }
            catch { ProductVersion = string.Empty; }
        }

        public static bool IncludeFolderPermission { get; set; }

        public static bool IncludeDeletedItems { get; set; } = true;

        public static bool IncludeJunkEmail { get; set; } = true;
        public static bool BackupExceptPersonMetadata { get; set; }
        public static bool IsRecoverableItemsMailbox { get; set; }

        //WARN: enable trace log will write log to AveLogger
        public static bool EnableTraceLog { get; set; } = true;

        public static bool EnableBatchRestore { get; set; }

        public static string ProductVersion { get; set; }

        public static bool SetImpersonateId { get; set; } = false;

        public static Dictionary<string, List<string>> FolderFilterProfile { get; set; }

        public static Dictionary<string, string> MailboxDisplayNameDic { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static bool EnableHideFolder { get; set; } = false;
    }
}