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

namespace AvePoint.ObjectModel.Common
{
    class AvePublishingWeb : AveClientObject, IAvePublishingWeb
    {
        private AveSite mSite;
        private IAveRequest mRequest;
        private AveWeb mWeb;
        private AveInheritableStringProperty mCustomMasterUrl;
        private AveInheritableStringProperty mMasterUrl;
        private AveInheritableStringProperty mAlternateCssUrl;
        private AveFile mDefaultPage;

        public AvePublishingWeb(AveSite site, AveWeb web, Dictionary<string, object> publishingWebProperties)
        {
            mSite = site;
            mRequest = site.Request;
            mWeb = web;
            base.DataCache.AddPropertyies(publishingWebProperties);
        }

        #region IAvePublishingWeb Members

        public IAvePageLayout[] GetAvailablePageLayouts(IAveContentTypeId contentTypeId)
        {
            throw new NotImplementedException();
        }

        public IAvePublishingPageCollection GetPublishingPages()
        {
            throw new NotImplementedException();
        }

        public IAvePublishingWeb GetPublishingWeb(IAveWeb web)
        {
            Dictionary<string, object> publishingWebProperties = mRequest.GetPublishingWeb(web.ServerRelativeUrl);
            return new AvePublishingWeb(web.Site as AveSite, web as AveWeb, publishingWebProperties);
        }

        public bool IsPublishingWeb(IAveWeb web)
        {
            return web.IsPublish;
        }

        public void Update()
        {
            throw new NotImplementedException();
            //Dictionary<string, object> newProperites = mRequest.UpdatePublishingWeb(mWeb.ServerRelativeUrl, base.DataCache.ChangedProperties);
            //base.DataCache.RefreshProperties(newProperites);
        }

        public IAveFile DefaultPage
        {
            get
            {
                if (mDefaultPage == null && base.DataCache.IsPropertyNotLoaded("DefaultPage") && base.DataCache.IsPropertyAvailable("DefaultPage" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> fileProperties = base.DataCache.GetProperty<Dictionary<string, object>>("DefaultPage" + AveObjectModelConstant.ObjectPropertySuffix);                    
                    mDefaultPage = new AveFile(mRequest, mWeb, null, null, fileProperties);                    
                }
                return mDefaultPage;
            }
            set
            {
                mDefaultPage = value as AveFile;
                base.DataCache.AddChangedProperty("DefaultPageRelativeUrl", value.ServerRelativeUrl);
            }
        }

        public string PagesListName
        {
            get { throw new NotImplementedException(); }
        }

        public IAveList PagesList
        {
            get { throw new NotImplementedException(); }
        }

        public IAveInheritableStringProperty CustomMasterUrl
        {
            get
            { 
                if (mCustomMasterUrl == null && base.DataCache.IsPropertyAvailable("CustomMasterUrl" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    mCustomMasterUrl = new AveInheritableStringProperty("CustomMasterUrl", mWeb.DataCache.ChangedProperties, base.DataCache.GetProperty<Dictionary<string, object>>("CustomMasterUrl" + AveObjectModelConstant.ObjectPropertySuffix));
                }
                return mCustomMasterUrl;
            }
        }

        public IAveInheritableStringProperty MasterUrl
        {
            get 
            {
                if (mMasterUrl == null && base.DataCache.IsPropertyAvailable("MasterUrl" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    mMasterUrl = new AveInheritableStringProperty("MasterUrl", mWeb.DataCache.ChangedProperties, base.DataCache.GetProperty<Dictionary<string, object>>("MasterUrl" + AveObjectModelConstant.ObjectPropertySuffix));
                }
                return mMasterUrl;
            }
        }

        public IAveInheritableStringProperty AlternateCssUrl
        {
            get 
            {
                if (mAlternateCssUrl == null && base.DataCache.IsPropertyAvailable("AlternateCssUrl" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    mAlternateCssUrl = new AveInheritableStringProperty("AlternateCssUrl", mWeb.DataCache.ChangedProperties, base.DataCache.GetProperty<Dictionary<string, object>>("AlternateCssUrl" + AveObjectModelConstant.ObjectPropertySuffix));
                }
                return mAlternateCssUrl;
            }
        }

        public Guid VariationRelationshipsListId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("VariationRelationshipsListId");
            }
            set
            {
                base.DataCache.AddChangedProperty("VariationRelationshipsListId", value);
            }
        }

        public Guid GetPagesListId(IAveWeb web)
        {
            //在server上实现，Client没有实现,返回默认值
            return default(Guid);
        }

        #endregion


        public IAvePageLayout[] GetAvailablePageLayouts()
        {
            throw new NotImplementedException();
        }

        public IAvePageLayout DefaultPageLayout
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsAllowingAllPageLayouts
        {
            get { throw new NotImplementedException(); }
        }


        public IAvePortalNavigation Navigation
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
