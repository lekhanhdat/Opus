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

namespace AvePoint.Media.ClassicStorage.Box
{
    [Serializable]
    public class BoxFileInfo : XFileInfo
    {
         string objectId;

        public BoxFileInfo(string highName, string lowName, long length)
            : base(highName, lowName)
        {
            this.FileSize = length;
        }

        public BoxFileInfo(string highName, string lowName, long length, string id)
            : base(highName, lowName)
        {
            this.FileSize = length;
            this.objectId = id;
        }
    }

    public class BoxAuthInfo
    {
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public string Time { get; set; }
    }

    class BoxObject
    {
        public Int32 Total_Count { get; set; }

        public String Type { get; set; }

        public String Id { get; set; }

        /// <summary>
        /// A unique ID for use with the/events endpoint
        /// </summary>
        public String Sequence_id { get; set; }

        /// <summary>
        /// A unique string identifying the version of this file
        /// </summary>
        public String Etag { get; set; }

        /// <summary>
        /// the sha1 hash of this file
        /// </summary>
        public String Sha1 { get; set; }

        public String Name { get; set; }

        public String Description { get; set; }

        /// <summary>
        /// size of this file in bytes
        /// </summary>
        public Int64 Size { get; set; }

        /// <summary>
        /// The path of folders to this item, starting at the root folder
        /// </summary>
        public List<BoxObject> Path_collection { get; set; }

        public String Created_at { get; set; }

        public String Modified_at { get; set; }

        /// <summary>
        /// When this file was last moved to the trash
        /// </summary>
        public String Trashed_at { get; set; }

        /// <summary>
        /// when this file will be permanently deleted
        /// </summary>
        public String Purged_at { get; set; }

        public String Content_created_at { get; set; }

        public String Content_modified_at { get; set; }

        public BoxUser Created_by { get; set; }

        public BoxUser Modified_by { get; set; }

        public BoxUser Owned_by { get; set; }

        public BoxObject Parent { get; set; }

        /// <summary>
        /// whether this item is deleted or not
        /// </summary>
        public String Item_status { get; set; }

        public List<BoxObject> Entries { get; set; }
        /// <summary>
        /// whether this folder will be synced by the box sync clients or not
        /// </summary>
        public String Sync_state { get; set; }

        public List<string> Tags { get; set; }

        public BoxObject()
        {
            Path_collection = new List<BoxObject>();
            Created_by = new BoxUser();
            Modified_by = new BoxUser();
            Owned_by = new BoxUser();
        }
    }

    class BoxUser
    {
        public String Type { get; set; }

        public String Id { get; set; }

        public String Name { get; set; }

        public String Login { get; set; }
    }
}
