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
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class AveBrowserOption
    {

        #region add for paging
        public bool NeedPaging = false; //to do
        public int StartIndex = 0;
        public uint PerPage = 0;
        public string PageInfo = string.Empty; // to do
        #endregion

        #region add for common
        public Guid ParentSiteId = Guid.Empty;
        public Guid ParentWebId = Guid.Empty;
        public Guid ParentListId = Guid.Empty;
        public Guid ParentFolderId = Guid.Empty;
        public string ParentFolderServerRelativeUrl = string.Empty;
        //public AveBrowserFolderType FolderType = AveBrowserFolderType.Normal;

        public int ChildrenTotalCount = 0;
        public string SiteUrl = string.Empty;
        #endregion

        #region add for item version
        public Guid ParentItemUniqueId = Guid.Empty;
        public string ParentWebServerRelativeUrl = string.Empty;
        public String ParentListTitle = string.Empty;
        #endregion

        #region for filter
        public bool NeedFilter = false;
        public bool FilterAppWeb = true; // to do
        public bool FilterSystemFolder = false; //to do
        #endregion
    }

    public enum AveBrowserFolderType
    {
        Normal = 0,
        RootFolderOfList = 1,
        RootFolderOfWeb = 2,
    }
}
