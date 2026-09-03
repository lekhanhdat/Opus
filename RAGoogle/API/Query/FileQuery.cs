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

namespace RAGoogle.API
{
    public class FileQuery : BaseQuery
    {
        public bool IncludeItemsFromAllDrives { get; set; }
        public bool IncludeRemoved { get; set; }
        public bool IncludeTrash { get; set; }
        public bool SupportsAllDrives { get; set; }
        public string SharedDriveId { get; set; }
        public string SharedDriveName { get; set; }
        public string IncludeLabels { get; set; }

        public string OrderBy { get; set; }
        public bool UseDomainAdminAccess { get; set; }
        public bool TransferOwnership { get; set; }
        public bool SearchTrashFile { get; set; }
        public bool SearchInDrive { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string? PageToken { get; set; }

        #region For drive label
        public bool PublishedOnly { get; set; }
        public bool IsLabelViewFull { get; set; }
        public bool IsRestrictToMyDrive { get; set; }
        #endregion
    }
}
