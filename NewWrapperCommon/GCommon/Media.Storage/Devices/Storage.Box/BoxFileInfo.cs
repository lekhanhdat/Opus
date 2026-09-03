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
using System.Globalization;

namespace AvePoint.Media.Storage.Box
{
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

        /// <summary>
        /// the shared link object for this file
        /// </summary>
        public ShareLink Shared_link { get; set; }

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

        public Lock Lock { get; set; }

        public BoxObject()
        {
            Path_collection = new List<BoxObject>();
            Created_by = new BoxUser();
            Modified_by = new BoxUser();
            Owned_by = new BoxUser();
            Shared_link = new ShareLink();
        }
    }

    class Lock
    {
        public BoxUser Created_by { get; set; }
        public String Created_at { get; set; }
        public String Type { get; set; }
        public String Id { get; set; }
    }

    class ShareLink
    {
        public String Url { get; set; }
        public String Download_url { get; set; }
        public String Vanity_url { get; set; }
        public Boolean Is_password_enabled { get; set; }
        public String Unshared_at { get; set; }
        public Int32 Download_count { get; set; }
        public Int32 Preview_count { get; set; }
        public String Access { get; set; }
        public Permissions Permissions { get; set; }
    }

    class Permissions
    {
        public Boolean Can_download { get; set; }
        public Boolean Can_preview { get; set; }
    }

    class BoxLock
    {
        public BoxLock(String type, String id, BoxUser createBy, String createdAt, Boolean downloadPrevent)
        {
            Type = type;
            Id = id;
            Created_by = createBy;
            Created_at = createdAt;
            Is_DownLoad_Prevented = downloadPrevent;
        }
        public String Type { get; set; }
        public String Id { get; set; }
        public BoxUser Created_by { get; set; }
        public String Created_at { get; set; }
        public Boolean Is_DownLoad_Prevented { get; set; }
    }
    class BoxUser
    {
        public String Type { get; set; }

        public String Id { get; set; }

        public String Name { get; set; }

        public String Login { get; set; }
    }

    class BoxFileInfo : XFileInfo
    {
        BoxObject innserObject;
        BoxSystem system;

        public BoxFileInfo()
        {
        }

        public BoxFileInfo(BoxSystem system, String highName, String lowName, BoxObject boxObject)
            : base(highName, lowName)
        {
            this.innserObject = boxObject;
            this.system = system;
        }

        public override Int64 FileSize
        {
            get
            {
                return innserObject.Size;
            }
        }

        public override List<XFileInfo> Versions
        {
            get
            {
                return this.system.GetFileVersion(this.ObjectId, this.HighName, this.LowName);
            }
        }

        public override String ObjectId
        {
            get
            {
                return this.innserObject.Id;
            }
        }

        public override DateTime CreationTimeUtc
        {
            get
            {
                return DateTime.Parse(this.innserObject.Created_at).ToUniversalTime();
            }
        }

        public override DateTime LastWriteTimeUtc
        {
            get
            {
                return DateTime.Parse(this.innserObject.Modified_at).ToUniversalTime();
            }
        }

        public override DateTime ContentCreatedTime
        {
            get
            {
                return DateTime.Parse(this.innserObject.Content_created_at).ToUniversalTime();
            }
        }

        public override DateTime ContentModifiedTime
        {
            get
            {
                return DateTime.Parse(this.innserObject.Content_modified_at).ToUniversalTime();
            }
        }

        public override string CreatedBy
        {
            get
            {
                return this.innserObject.Created_by.Name;
            }
        }

        public override string ModifiedBy
        {
            get
            {
                return this.innserObject.Modified_by.Name;
            }
        }

        public override string OwnedBy
        {
            get
            {
                return this.innserObject.Owned_by.Name;
            }
        }

        public override string Description
        {
            get
            {
                return this.innserObject.Description;
            }
        }

        public override String Etag
        {
            get
            {
                return this.innserObject.Etag;
            }
        }

        public override String SetNewName
        {
            get
            {
                return base.SetNewName;
            }
            set
            {
                system.SetNewFileName(this, value);
            }
        }

        public override Boolean Exists
        {
            get
            {
                return true;
            }
        }

        public override List<string> Tags
        {
            get
            {
                var info = this.system.GetFileTags(this);
                return info.Tags;
            }
            set
            {
                base.Tags = value;
            }
        }

        public override string Url
        {
            get
            {
                return this.innserObject.Shared_link.Url;
            }
        }

        public override string DownloadUrl
        {
            get
            {
                return this.innserObject.Shared_link.Download_url;
            }
        }

        public override bool IsLocked
        {
            get
            {
                return this.system.IsLocked(this);
            }
        }
    }

    class BoxFolderInfo : XDirectoryInfo
    {
        BoxObject innserObject;
        BoxSystem system;

        public BoxFolderInfo()
        {
        }
        public BoxFolderInfo(BoxSystem sys, string highName, string lowName, BoxObject boxObject)
        {
            HighName = highName;
            LowName = lowName;
            this.innserObject = boxObject;
            this.system = sys;
        }

        public override string ClipId
        {
            get
            {
                return this.innserObject.Id;
            }
        }

        public override string SetNewName
        {
            get
            {
                return base.SetNewName;
            }
            set
            {
                system.SetNewFolderName(this, value);
            }
        }

        public override bool Exists
        {
            get
            {
                return true;
            }
        }

        public override DateTime CreationTimeUtc
        {
            get
            {
                return DateTime.Parse(this.innserObject.Created_at).ToUniversalTime();
            }
        }

        public override DateTime LastWriteTimeUtc
        {
            get
            {
                return DateTime.Parse(this.innserObject.Modified_at).ToUniversalTime();
            }
        }
        public override bool IsEmpty
        {
            get { throw new NotSupportedException(); }
        }

        public override String Name
        {
            get
            {
                return this.LowName;
            }
        }

        public override string CreatedBy
        {
            get
            {
                return this.innserObject.Created_by.Name;
            }
        }

        public override string ModifiedBy
        {
            get
            {
                return this.innserObject.Modified_by.Name;
            }
        }

        public override string OwnedBy
        {
            get
            {
                return this.innserObject.Owned_by.Name;
            }
        }

        public override string Description
        {
            get
            {
                return this.innserObject.Description;
            }
        }

        public override List<string> Tags
        {
            get
            {
                var info = this.system.GetFolderTags(this);
                return info.Tags;
            }
            set
            {
                base.Tags = value;
            }
        }

        public override string Url
        {
            get
            {
                return this.innserObject.Shared_link.Url;
            }
        }

        public override string DownloadUrl
        {
            get
            {
                return this.innserObject.Shared_link.Download_url;
            }
        }
    }

    class BoxAuthInfo
    {
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public string Time { get; set; }
        public String ClientSecret { get; set; }
        public String ClientId { get; set; }
    }

    class BoxGroupInfo
    {
        internal String Type { get; set; }
        internal String Id { get; set; }
        internal String Name { get; set; }
        internal DateTime Create_at { get; set; }
        internal DateTime Modified_at { get; set; }
    }
}
