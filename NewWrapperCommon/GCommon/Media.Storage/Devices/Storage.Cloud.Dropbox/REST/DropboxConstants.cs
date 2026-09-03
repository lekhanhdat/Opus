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

using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.Dropbox.DropboxConstants.#.cctor()", MessageId = "fileops")]
namespace AvePoint.Media.Storage.Cloud.Dropbox
{
    #region using
    using System;
    #endregion

    static class DropboxConstants
    {
        public static readonly String StorageType = "DropBoxSystem";
        public static readonly String Allocated = "allocated\":[^}]*";
        public static readonly String Used = "used\":[^,]*";
        public static readonly String ObjectSize = "bytes\":[^,]*";
        public static readonly String SessionId = "session_id\":[^,]*";
        public static readonly String ErrorSummary = "error_summary\": [^,]*";
        //upload file, each trunk size
        public static readonly Int32 ChunkSize = 4 * 1024 * 1024;
        //upload file, distinguish normal or big file
        public static readonly Int64 UploadLimitSize = 100 * 1024 * 1024;
    }
}
