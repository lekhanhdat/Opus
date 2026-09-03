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
using System.Net;
using System.Xml;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Portal.WebControls;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server16
{
    class AveWebCollection : AveAbstractCommonCollection<IAveWeb>, IAveWebCollection
    {
        private SPWebCollection mWebs;
        private ICredentials mWebsCredentials;
        private string mWebsUrl;
        private AveSite mSite;

        public AveWebCollection(AveSite site, SPWebCollection webs)
            : base(webs)
        {
            mSite = site;
            mWebs = webs;
        }

        /// <summary>
        /// Constructor Method for Webs
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="url"></param>
        public AveWebCollection(ICredentials credentials, string url)
            : base(null)
        {
            mWebsCredentials = credentials;
            mWebsUrl = url;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveWeb(mSite, t as SPWeb);
        }

        #region IAveWebCollection Members

        public IAveWeb Add(AveWebCreationInformation webCreationInfo)
        {
            string url = webCreationInfo.Url;
            string title = webCreationInfo.Title;
            string description = webCreationInfo.Description;
            uint language = (uint)webCreationInfo.Language;
            string webTemplate = webCreationInfo.WebTemplate;
            bool useUniquePermissions = !webCreationInfo.UseSamePermissionsAsParentSite;
            return new AveWeb(mSite, mWebs.Add(url, title, description, language, webTemplate, useUniquePermissions, false));
        }

        public IAveWeb Add(string strWebUrl, string strTitle, string strDescription, uint nLCID, IAveWebTemplate WebTemplate, bool useUniquePermissions, bool bConvertIfThere)
        {
            return new AveWeb(mSite, mWebs.Add(strWebUrl, strTitle, strDescription, nLCID, (WebTemplate as AveWebTemplate).WebTemplate, useUniquePermissions, bConvertIfThere));
        }

        public IAveWeb Add(string strWebUrl, string strTitle, string strDescription, uint nLCID, string strWebTemplate, bool useUniquePermissions, bool bConvertIfThere)
        {
            return new AveWeb(mSite, mWebs.Add(strWebUrl, strTitle, strDescription, nLCID, strWebTemplate, useUniquePermissions, bConvertIfThere));
        }

        public IAveWeb this[string name]
        {
            get
            {
                return new AveWeb(mSite, mWebs[name]);
            }
        }

        public IAveWeb this[Guid id]
        {
            get
            {
                return new AveWeb(mSite, mWebs[id]);
            }
        }

        public string WebUrlFromPageUrl(string pageUrl)
        {
            Webs webs = new Webs();
            using (webs as IDisposable)
            {
                webs.Url = mWebsUrl;
                webs.Credentials = mWebsCredentials;
                return webs.WebUrlFromPageUrl(pageUrl);
            }
        }

        public override IAveWeb this[int index]
        {
            get
            {
                return new AveWeb(mSite, mWebs[index]);
            }
        }

        public XmlNode GetWeb(string siteDirUrl)
        {
            using (Webs webs = new Webs())
            {
                webs.Credentials = mWebsCredentials;
                webs.Url = mWebsUrl;
                return webs.GetWeb(siteDirUrl);
            }
        }

        public override int Count
        {
            get { return mWebs.Count; }
        }

        #endregion
    }
}