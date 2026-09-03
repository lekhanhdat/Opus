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
using AvePoint.GCommon.Utility.I18N;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using SPDisposeCheck;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon;
using System.Linq;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveSiteCollection : AveAbstractCommonCollection<IAveSite>, IAveSiteCollection
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveItem));
        private SPSiteCollection mSiteCollection;

        public AveSiteCollection(SPSiteCollection siteColl)
            : base(siteColl)
        {
            mSiteCollection = siteColl;
        }

        #region IAveSiteCollection Members

        public IAveSite Add(string siteUrl, string title, string description, uint nLCID, string webTemplate, string ownerLogin, string ownerName, string ownerEmail)
        {
            return new AveSite(mSiteCollection.Add(siteUrl, title, description, nLCID, webTemplate, ownerLogin, ownerName, ownerEmail));
        }

        public IAveSite Add(string siteUrl, string title, string description, uint nLCID, string webTemplate, string ownerLogin, string ownerName, string ownerEmail, string secondaryContactLogin, string secondaryContactName, string secondaryContactEmail)
        {
            return new AveSite(mSiteCollection.Add(siteUrl, title, description, nLCID, webTemplate, ownerLogin, ownerName, ownerEmail, secondaryContactLogin, secondaryContactName, secondaryContactEmail));
        }

        [SPDisposeCheckIgnore(SPDisposeCheckID._240, "This site will be Disposed by AveSite")]
        public IAveSite Add(string siteUrl, string title, string description, uint nLCID, string webTemplate, string ownerLogin, string ownerName, string ownerEmail, string secondaryContactLogin, string secondaryContactName, string secondaryContactEmail, bool useHostHeaderAsSiteName)
        {
            return new AveSite(mSiteCollection.Add(siteUrl, title, description, nLCID, webTemplate, ownerLogin, ownerName, ownerEmail, secondaryContactLogin, secondaryContactName, secondaryContactEmail, useHostHeaderAsSiteName));
        }

        public IAveSite Add(string siteUrl, string title, string description, uint nLCID, int compatibilityLevel, string webTemplate, string ownerLogin, string ownerName, string ownerEmail, string secondaryContactLogin, string secondaryContactName, string secondaryContactEmail)
        {
            return new AveSite(mSiteCollection.Add(siteUrl, title, description, nLCID, compatibilityLevel, webTemplate, ownerLogin, ownerName, ownerEmail, secondaryContactLogin, secondaryContactName, secondaryContactEmail));
        }

        public IAveSite Add(string siteUrl, string title, string description, uint nLCID, int compatibilityLevel, string webTemplate, string ownerLogin, string ownerName, string ownerEmail, string secondaryContactLogin, string secondaryContactName, string secondaryContactEmail, bool useHostHeaderAsSiteName)
        {
            return new AveSite(mSiteCollection.Add(siteUrl, title, description, nLCID, compatibilityLevel, webTemplate, ownerLogin, ownerName, ownerEmail, secondaryContactLogin, secondaryContactName, secondaryContactEmail, useHostHeaderAsSiteName));
        }

        public void Backup(string siteUrl, string filePath, bool overWrite)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSiteCollection.Backup"))
            {

                mSiteCollection.Backup(siteUrl, filePath, overWrite);

            }

        }

        public void Restore(string strSiteUrl, string strFilename, bool bOverwrite)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSiteCollection.Restore"))
            {

                mSiteCollection.Restore(strSiteUrl, strFilename, bOverwrite);

            }

        }

        public void Restore(string strSiteUrl, string strFilename, bool bOverwrite, bool hostHeaderAsSiteName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSiteCollection.Restore"))
            {

                mSiteCollection.Restore(strSiteUrl, strFilename, bOverwrite, hostHeaderAsSiteName);

            }

        }

        public IAveSite this[string strSiteName]
        {
            [SPDisposeCheckIgnore(SPDisposeCheckID._230, "This site will be Disposed by AveSite")]
            get
            {
                SPSite site = mSiteCollection[strSiteName];
                if (site == null)
                {
                    return null;
                }
                return new AveSite(site);
            }
        }

        public IAveSite this[Guid id]
        {
            get
            {
                SPSite site = mSiteCollection.FirstOrDefault(node => node.ID == id);
                if (site == null)
                {
                    return null;
                }
                return new AveSite(site);
            }
        }

        public override IAveSite this[int index]
        {
            [SPDisposeCheckIgnore(SPDisposeCheckID._230, "This site will be Disposed by AveSite")]
            get
            {
                SPSite site = mSiteCollection[index];
                if (site == null)
                {
                    return null;
                }
                return new AveSite(site);
            }
        }

        public string[] Names
        {
            get
            {
                return mSiteCollection.Names;
            }
        }

        #endregion

        protected override object CreatElementInstance(object t)
        {
            SPSite site = t as SPSite;
            if (site != null)//xluo：对于manage path 被删除而site collection没有被删除的情况，可能会有t是null的情况发生
            {
                return new AveSite(t as SPSite);
            }
            else
            {
                return null;
            }
        }

        public override int Count
        {
            get { return mSiteCollection.Count; }
        }
    }
}
