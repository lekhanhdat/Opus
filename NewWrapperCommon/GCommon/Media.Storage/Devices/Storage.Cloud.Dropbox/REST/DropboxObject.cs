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


namespace AvePoint.Media.Storage.Cloud.Dropbox
{
    #region using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion
    /// <summary>
    /// json object convert to this object
    /// </summary>
    class DropboxObject
    {
        public String Tag { get; set; }
        public String Name { get; set; }
        public String Path_lower { get; set; }
        public String Path_display { get; set; }
        public String Client_modified { get; set; }
        public String Server_modified { get; set; }
        public String Rev { get; set; }
        public Int64 Size { get; set; }
        public String Id { get; set; }
        public String Cursor { get; set; }
        public Boolean Has_more { get; set; }
        public List<DropboxObject> Entries { get; set; }
        public Boolean Is_deleted { get { return this.Tag != null ? this.Tag.Equals("deleted") : false; } }
        public Boolean Is_dir { get { return this.Tag != null ? this.Tag.Equals("folder") : false; } }
        public DropboxObject()
        {
            this.Has_more = false;
            this.Entries = new List<DropboxObject>();
        }
        public class DropboxUsageInfo
        {
            public UInt64 Used { get; set; }
            public DropboxAllocation Allocation { get; set; }
        }

        public class DropboxAllocation
        {
            public UInt64 Used { get; set; }
            public UInt64 Allocated { get; set; }
        }
    }
}
