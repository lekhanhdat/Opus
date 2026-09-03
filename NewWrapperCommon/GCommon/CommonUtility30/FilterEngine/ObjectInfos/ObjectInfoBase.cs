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



namespace AvePoint.Common.FilterEngine
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    public abstract class ObjectInfoBase
    {
        internal virtual FilterLevel Level { get { return FilterLevel.Invalid; } }
    }

    internal enum FilterLevel
    {
        Invalid = 0x0,
        #region SharePoint Container Object 1-15
        WebApp = 0x1,
        SiteCollection,
        Site,
        List,
        Folder,
        #endregion
        #region SharePoint Content Object 16-255
        Item = 0x10,
        MicroFeedItem,
        ItemVersion,
        Attachment,

        Document = 0x20,
        DocumentVersion,
        #endregion
        #region Others >256
        TreeNode = 21,
        FSFolder = 1000,
        FSFile = 1001,
        PhysicalBox = 2000,
        PhysicalFolder = 2001,
        #endregion
    }
}
