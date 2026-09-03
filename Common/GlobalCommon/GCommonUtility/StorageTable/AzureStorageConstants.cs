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


namespace StorageTable
{
    public class AzureStorageConstants
    {
        /// <summary>
        /// Request method
        /// </summary>
        private static string Method_POST = "POST";
        private static string Method_GET = "GET";
        private static string Method_PUT = "PUT";
        private static string Method_DELETE = "DELETE";

        public static string POST { get { return Method_POST; } }
        public static string GET { get { return Method_GET; } }
        public static string PUT { get { return Method_PUT; } }
        public static string DELETE { get { return Method_DELETE; } }

        /// <summary>
        /// 
        /// </summary>
        private static string Key_Type_S = "SharedKey";
        private static string Key_Type_S_Lite = "SharedKeyLite";

        public static string SharedKey { get { return Key_Type_S; } }
        public static string SharedKeyLite { get {return Key_Type_S_Lite;}}

        private static string EncryptionKey = "LOGDAOL";
        public static string Key { get { return EncryptionKey; } }

        /// <summary>
        /// 
        /// </summary>
        public static string CResource_Table = "Tables";
        public static string CResource_Blob = "Blob";

        /// <summary>
        /// 
        /// </summary>
        public static string TableURI = "https://{0}.table.core.windows.net/Tables";
        public static string EntryURI1 = "https://{0}.table.core.windows.net/{1}";
        public static string EntryURI2 = "https://{0}.table.core.windows.net/{1}(PartitionKey=\'{2}\',RowKey=\'{3}\')";
        public static string EntryURIBATCH = "https://{0}.table.core.windows.net/$batch";
        public static string EntryURIValidate = "https://{0}.table.core.windows.net/Tables()";
        public static string BatchBoundary = "batch_boundary";
        public static string ChangesetBoundary = "changeset_boundary";

    }
}
