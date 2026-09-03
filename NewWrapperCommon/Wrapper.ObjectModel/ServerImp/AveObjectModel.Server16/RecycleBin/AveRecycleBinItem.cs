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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveRecycleBinItem : IAveRecycleBinItem
    {
        private SPRecycleBinItem mRecycleBinItem;
        private AveUser mAuthor;
        private AveUser mDeletedBy;
        private AveRecycleBinItemCollection mRecycleBinItems;
        private AveWeb mWeb;
        public AveRecycleBinItem(AveRecycleBinItemCollection recycleBinItems, SPRecycleBinItem recycleBinItem)
        {
            mRecycleBinItems = recycleBinItems;
            mRecycleBinItem = recycleBinItem;
        }

        #region IAveRecycleBinItem Members

        public void DeleteObject()
        {
            mRecycleBinItem.Delete();
        }

        public void Restore()
        {
            mRecycleBinItem.Restore();
        }

        public IAveUser Author
        {
            get
            {
                if (mAuthor == null)
                {
                    SPUser user = mRecycleBinItem.Author;
                    if (user != null)
                    {
                        mAuthor = new AveUser(this.Web as AveWeb, user);
                    }
                }
                return mAuthor;
            }
        }

        public IAveUser DeletedBy
        {
            get
            {
                if (mDeletedBy == null)
                {
                    mDeletedBy = new AveUser(this.Web as AveWeb, mRecycleBinItem.DeletedBy);
                }
                return mDeletedBy;
            }
        }

        public DateTime DeletedDate
        {
            get { return mRecycleBinItem.DeletedDate; }
        }

        public string DirName
        {
            get { return mRecycleBinItem.DirName; }
        }

        public Guid ID
        {
            get { return mRecycleBinItem.ID; }
        }

        public AveRecycleBinItemState ItemState
        {
            get { return (AveRecycleBinItemState)mRecycleBinItem.ItemState; }
        }

        public AveRecycleBinItemType ItemType
        {
            get { return (AveRecycleBinItemType)mRecycleBinItem.ItemType; }
        }

        public string LeafName
        {
            get { return mRecycleBinItem.LeafName; }
        }

        public long Size
        {
            get { return mRecycleBinItem.Size; }
        }

        public string Title
        {
            get { return mRecycleBinItem.Title; }
        }

        public IAveSite Site
        {
            get
            {
                if (this.mRecycleBinItems.Site != null)
                {
                    return this.mRecycleBinItems.Site;
                }
                return this.mRecycleBinItems.Web.Site;
            }
        }

        public IAveWeb Web
        {
            get
            {
                if (mWeb == null)
                {
                    SPWeb web = mRecycleBinItem.Web;
                    if (web != null)
                    {
                        mWeb = mRecycleBinItems.Site.AllWebs[web.ID] as AveWeb;
                    }
                }
                return mWeb;
            }
        }

        #endregion
    }
}
