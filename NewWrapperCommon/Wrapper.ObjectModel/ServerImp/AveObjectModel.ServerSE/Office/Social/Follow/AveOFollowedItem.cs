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


namespace AvePoint.ObjectModel.ServerSE.Office
{
    using System;
    using System.Collections.Generic;
    using AvePoint.Wrapper.Common.Office;
    using Microsoft.Office.Server.UserProfiles;

    class AveOFollowedItem : IAveOFollowedItem
    {
        internal FollowedItem FollowedItem { get;  set; }
        public AveOFollowedItem(FollowedItem followedItem)
        {
            this.FollowedItem = followedItem;
        }

        public IDictionary<string, object> Data
        {
            get
            {
                return this.FollowedItem.Data;
            }

            set
            {
                this.FollowedItem.Data = value;
            }
        }

        public string FileType
        {
            get
            {
                return this.FollowedItem.FileType;
            }

            set
            {
                this.FollowedItem.FileType = value;
            }

        }

        public string FileTypeProgid
        {
            get
            {
                return this.FollowedItem.FileTypeProgid;
            }

            set
            {
                this.FollowedItem.FileTypeProgid = value;
            }
        }

        public string Flags
        {
            get
            {
                return this.FollowedItem.Flags;
            }

            set
            {
                this.FollowedItem.Flags = value;
            }
        }

        public Guid GroupId
        {
            get
            {
                return this.FollowedItem.GroupId;
            }

            set
            {
                this.FollowedItem.GroupId = value;
            }
        }

        public bool HasFeed
        {
            get
            {
                return this.FollowedItem.HasFeed;
            }

            set
            {
                this.FollowedItem.HasFeed = value;
            }
        }

        public bool Hidden
        {
            get
            {
                return this.FollowedItem.Hidden;
            }

            set
            {
                this.FollowedItem.Hidden = value;
            }
        }

        public Uri IconUrl
        {
            get
            {
                return this.FollowedItem.IconUrl;
            }

            set
            {
                this.FollowedItem.IconUrl = value;
            }
        }

        public int ItemId
        {
            get
            {
                return this.FollowedItem.ItemId;
            }

            set
            {
                this.FollowedItem.ItemId = value;
            }
        }

        public AveOFollowedItemType ItemType
        {
            get
            {
                return (AveOFollowedItemType)this.FollowedItem.ItemType;
            }

            set
            {
                this.FollowedItem.ItemType = (FollowedItemType)value;
            }
        }

        public Guid ListId
        {
            get
            {
                return this.FollowedItem.ListId;
            }

            set
            {
                this.FollowedItem.ListId = value;
            }
        }

        public Uri ParentUrl
        {
            get
            {
                return this.FollowedItem.ParentUrl;
            }

            set
            {
                this.FollowedItem.ParentUrl = value;
            }
        }

        public int Pinned
        {
            get
            {
                return this.FollowedItem.Pinned;
            }

            set
            {
                this.FollowedItem.Pinned = value;
            }
        }

        public string ServerUrlProgid
        {
            get
            {
                return this.FollowedItem.ServerUrlProgid;
            }

            set
            {
                this.FollowedItem.ServerUrlProgid = value;
            }
        }

        public Guid SiteId
        {
            get
            {
                return this.FollowedItem.SiteId;
            }

            set
            {
                this.FollowedItem.SiteId = value;
            }
        }

        public int Subtype
        {
            get
            {
                return this.FollowedItem.Subtype;
            }

            set
            {
                this.FollowedItem.Subtype = value;
            }
        }

        public string Title
        {
            get
            {
                return this.FollowedItem.Title;
            }

            set
            {
                this.FollowedItem.Title = value;
            }
        }

        public Guid UniqueId
        {
            get
            {
                return this.FollowedItem.UniqueId;
            }

            set
            {
                this.FollowedItem.UniqueId = value;
            }
        }

        public Uri Url
        {
            get
            {
                return this.FollowedItem.Url;
            }

            set
            {
                this.FollowedItem.Url = value;
            }
        }

        public Guid WebId
        {
            get
            {
                return this.FollowedItem.WebId;
            }

            set
            {
                this.FollowedItem.WebId = value;
            }
        }
    }
}
