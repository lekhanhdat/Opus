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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.ServerSE
{
    class AvePublishingWeb : IAvePublishingWeb
    {
        private const string mPublishingWeb_Type = "Microsoft.SharePoint.Publishing.PublishingWeb";
        private PublishingWeb mPublishingWeb;
        private AveFile mDefaultPage;
        private AveWeb mWeb;
        private AveList mPagesList;
        readonly AveLogger logger = AveLogger.GetInstance(typeof(AvePublishingWeb));
        public AvePublishingWeb(AveWeb web, PublishingWeb publishingWeb)
        {
            mWeb = web;
            mPublishingWeb = publishingWeb;
        }

        public AvePublishingWeb(IAveWeb web)
        {
            mWeb = web as AveWeb;
            mPublishingWeb = PublishingWeb.GetPublishingWeb((web as AveWeb).Web);
        }

        /// <summary>
        /// Contruct method for calling static method
        /// </summary>
        public AvePublishingWeb()
        { }

        #region IAvePublishingWeb Members

        public bool IsPublishingWeb(IAveWeb web)
        {
            try
            {
                return AvePublishing.IsPublishingWeb(web as AveWeb);
            }
            catch (Exception e)
            {
                logger.Debug("Check publishing web error. {0}",e.ToString());
                return web.Features[AveSP2013FeatureDefinitions.PublishingWeb] == null;
            }
        }

        public Guid GetPagesListId(IAveWeb web)
        {
            return mPublishingWeb.PagesListId;
            //return PublishingWeb.GetPagesListId((web as AveWeb).Web);
        }

        public IAvePublishingWeb GetPublishingWeb(IAveWeb web)
        {
            return new AvePublishingWeb(web as AveWeb, PublishingWeb.GetPublishingWeb((web as AveWeb).Web));
        }

        public IAvePageLayout[] GetAvailablePageLayouts(IAveContentTypeId contentTypeId)
        {
            PageLayout[] pageLayouts = mPublishingWeb.GetAvailablePageLayouts((contentTypeId as AveContentTypeId).ContentTypeId);
            IAvePageLayout[] AvePageLayouts = new IAvePageLayout[pageLayouts.Length];
            for (int i = 0; i < pageLayouts.Length; i++)
            {
                AvePageLayouts[i] = new AvePageLayout(pageLayouts[i]);
            }
            return AvePageLayouts;
        }

        public IAvePublishingPageCollection GetPublishingPages()
        {
            PublishingPageCollection publishingPageCollection = mPublishingWeb.GetPublishingPages();
            if (publishingPageCollection == null)
            {
                return null;
            }
            return new AvePublishingPageCollection(this.PagesList as AveList, publishingPageCollection);
        }

        public string PagesListName
        {
            get
            {
                return mPublishingWeb.PagesListName;
            }
        }

        public void Update()
        {
            mPublishingWeb.Update();
        }

        public IAveFile DefaultPage
        {
            get
            {
                if (mDefaultPage == null)
                {
                    SPFile file = mPublishingWeb.DefaultPage;
                    if (file != null)
                    {
                        mDefaultPage = new AveFile(mWeb, file);
                    }
                }
                return mDefaultPage;
            }
            set
            {
                mDefaultPage = value as AveFile;
                if (mDefaultPage != null)
                {
                    mPublishingWeb.DefaultPage = mDefaultPage.File;
                }
                else
                {
                    mPublishingWeb.DefaultPage = null;
                }
            }
        }

        public IAveList PagesList
        {
            get
            {
                if (mPagesList == null)
                {
                    SPList list = mPublishingWeb.PagesList;
                    if (list != null)
                    {
                        mPagesList = (mWeb.Lists as AveListCollection).CreateListByType(list);
                    }
                }
                return mPagesList;
            }
        }

        public IAveInheritableStringProperty CustomMasterUrl
        {
            get
            {
                return new AveInheritableStringProperty(this.mPublishingWeb.CustomMasterUrl);
            }
        }

        public IAveInheritableStringProperty MasterUrl
        {
            get
            {
                return new AveInheritableStringProperty(this.mPublishingWeb.MasterUrl);
            }
        }

        public IAveInheritableStringProperty AlternateCssUrl
        {
            get
            {
                return new AveInheritableStringProperty(this.mPublishingWeb.AlternateCssUrl);
            }
        }

        public Guid VariationRelationshipsListId
        {
            get
            {
                return (Guid)AveAssemblyUtility.GetPropertyValue(mPublishingWeb, "VariationRelationshipsListId");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mPublishingWeb, "VariationRelationshipsListId", value);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (mDefaultPage != null)
            {
                mDefaultPage.Dispose();
                mDefaultPage = null;
            }
            if (mPagesList != null)
            {
                mPagesList.Dispose();
                mPagesList = null;
            }
            if(mPublishingWeb != null)
            {
                mPublishingWeb.Close();
            }
        }

        #endregion


        public IAvePageLayout[] GetAvailablePageLayouts()
        {
            PageLayout[] pageLayouts = mPublishingWeb.GetAvailablePageLayouts();
            IAvePageLayout[] list = new IAvePageLayout[pageLayouts.Length];
            for (int i = 0; i < pageLayouts.Length; i++)
            {
                list[i] = new AvePageLayout(pageLayouts[i]);
            }
            return list;
        }

        public IAvePageLayout DefaultPageLayout
        {
            get
            {
                return new AvePageLayout(mPublishingWeb.DefaultPageLayout);
            }
        }

        public bool IsAllowingAllPageLayouts
        {
            get
            {
                return mPublishingWeb.IsAllowingAllPageLayouts;
            }
        }


        public IAvePortalNavigation Navigation
        {
            get { return new AvePortalNavigation(mPublishingWeb); }
        }
    }
}
