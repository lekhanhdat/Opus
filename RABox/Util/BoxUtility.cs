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
using Box.V2.Models;

namespace RABox
{
    public class BoxUtility
    {
        public static readonly string BoxRootFolderId = "0";
        public static readonly string BoxAnonymousUserId = "2";

        public static readonly List<string> ModifiedEventTypes = new List<string>()
        {
            "ITEM_MODIFY", "ITEM_RENAME", 
            "ITEM_CREATE" , "ITEM_UPLOAD", "ITEM_MAKE_CURRENT_VERSION",
            "ITEM_UNDELETE_VIA_TRASH","ITEM_TRASH",
            "ITEM_MOVE", "ITEM_COPY", 
        };

        public static readonly string TrashedEventType = "ITEM_TRASH";

        public static readonly List<string> ItemFields = new List<string>() { BoxItem.FieldName, BoxItem.FieldOwnedBy, BoxItem.FieldCreatedBy, BoxItem.FieldModifiedBy, BoxItem.FieldCreatedAt, BoxItem.FieldModifiedAt, BoxItem.FieldPathCollection, BoxItem.FieldSize, BoxItem.FieldSharedLink, BoxFile.FieldVersionNumber, BoxItem.FieldParent, BoxItem.FieldTrashedAt, BoxFile.FieldUploaderDisplayName, BoxFile.FieldFileVersion, BoxFile.FieldDispositionAt, BoxItem.FieldTrashedAt};
        public static readonly List<string> ItemBasicFields = new List<string>() { BoxItem.FieldName, BoxItem.FieldOwnedBy, BoxItem.FieldPathCollection, BoxItem.FieldSize, BoxFile.FieldVersionNumber };
    }
}
