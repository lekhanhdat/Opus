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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.Client;
namespace AvePoint.ObjectModel.Common
{
    class AveRecycleBinItem : AveClientObject, IAveRecycleBinItem
    {
        private IAveRequest mRequest;
        private IAveSite mParentSite;
        private AveRecycleBinItemCollection mRecycleItemCollection;
        static AveLogger mLogger = AveLogger.GetInstance(typeof(AveRecycleBinItem));
        
        public AveRecycleBinItem(IAveRequest request, IAveSite parentSite, AveRecycleBinItemCollection recycleItemCollection, IDictionary<string, object> prop)
        {
            mRequest = request;
            mParentSite = parentSite;
            mRecycleItemCollection = recycleItemCollection;
            base.DataCache.AddPropertyies(prop);
        }
        public void DeleteObject()
        {
            try
            {
                this.mRecycleItemCollection.Delete(new Guid[] { this.ID });
            }
            catch (Exception e)
            {
                mLogger.Debug(AveObjectModel_CommonResource.DeleteObjectFromRecycleBinError, this.LeafName, this.Site.Url, e.ToString());
                //Log
            }
        }
        public void Restore()
        {
            try
            {
                this.mRecycleItemCollection.Restore(this.ID);
            }
            catch (Exception e)
            {
                mLogger.Debug(AveObjectModel_CommonResource.RestoreObjectFromRecycleBinError, this.LeafName, this.Site.Url, e.ToString());
                //Log
            }
        }

        public IAveUser Author
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Author") && base.DataCache.IsPropertyAvailable("Author" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    string loginName = base.DataCache.GetProperty<string>("Author" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveUser author = this.mParentSite.RootWeb.SiteUsers.GetByLoginName(loginName) as AveUser;
                    base.DataCache.AddProperty("Author", author);
                }
                return base.DataCache.GetProperty<IAveUser>("Author");
            }
        }
        public IAveUser DeletedBy
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DeletedBy") && base.DataCache.IsPropertyAvailable("DeletedBy" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    string loginName = base.DataCache.GetProperty<string>("DeletedBy" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveUser user = this.mParentSite.RootWeb.SiteUsers.GetByLoginName(loginName) as AveUser;
                    base.DataCache.AddProperty("DeletedBy", user);
                }
                return base.DataCache.GetProperty<IAveUser>("DeletedBy");
            }
        }

        public DateTime DeletedDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("DeletedDate");
            }
        }
        public string DirName
        {
            get
            {
                return base.DataCache.GetProperty<string>("DirName");
            }
        }
        public Guid ID
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }
        public AveRecycleBinItemState ItemState
        {
            get
            {
                return base.DataCache.GetProperty<AveRecycleBinItemState>("ItemState");
            }
        }
        public AveRecycleBinItemType ItemType
        {
            get
            {
                return base.DataCache.GetProperty<AveRecycleBinItemType>("ItemType");
            }
        }
        public string LeafName
        {
            get
            {
                return base.DataCache.GetProperty<string>("LeafName");
            }
        }
        public long Size
        {
            get
            {
                return base.DataCache.GetProperty<long>("Size");
            }
        }
        public string Title
        {
            get
            {
                return base.DataCache.GetProperty<string>("Title");
            }
        }
        public IAveSite Site
        {
            get { return mParentSite; }
        }
        public IAveWeb Web
        {
            get { throw new NotImplementedException(); }
        }
    }
}
