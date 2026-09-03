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
using System.Collections;

namespace AvePoint.ObjectModel.Common
{
    class AveWebCollection : AveAbstractCommonCollection<IAveWeb>, IAveWebCollection
    {
        private IAveRequest mRequest;
        private AveSite mSite;
        private AveWeb mParentWeb;

        public AveWebCollection(IAveRequest request, AveSite site, AveWeb parentWeb, Dictionary<string, object> webColProperties)
        {
            mRequest = request;
            mSite = site;
            mParentWeb = parentWeb;
            base.DataCache.AddPropertyies(webColProperties);
            InitWebCollection();
        }

        internal void InitWebCollection()
        {
            var webPropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveWeb>(webPropertiesList.Count);
            foreach (var webProperties in webPropertiesList)
            {
                AveWeb web = new AveWeb(mRequest, mSite, this, webProperties);
                mListData.Add(web);
            }
        }

        #region IAveWebCollection Members
        public IAveWeb this[Guid webId]
        {
            get
            {
                return mListData.Find(
                    delegate(IAveWeb web)
                    {
                        return web.ID.Equals(webId);
                    });
            }
        }
        public IAveWeb this[string name]
        {
            get
            {
                return mListData.Find(
                    delegate(IAveWeb web)
                    {
                        return web.Name == name;
                    });
            }
        }

        public IAveWeb Add(AveWebCreationInformation webCreationInfo)
        {
            return this.Add(webCreationInfo.Url, webCreationInfo.Title, webCreationInfo.Description, (uint)webCreationInfo.Language, webCreationInfo.WebTemplate, webCreationInfo.UseSamePermissionsAsParentSite, false);
        }
        public IAveWeb Add(string strWebUrl, string strTitle, string strDescription, uint nLCID, IAveWebTemplate webTemplate, bool useUniquePermissions, bool bConvertIfThere)
        {
            return this.Add(strWebUrl, strTitle, strDescription, nLCID, webTemplate.Name, useUniquePermissions, bConvertIfThere);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strWebUrl"> full url </param>
        /// <param name="strTitle"></param>
        /// <param name="strDescription"></param>
        /// <param name="nLCID"></param>
        /// <param name="strWebTemplate"></param>
        /// <param name="useUniquePermissions"></param>
        /// <param name="bConvertIfThere"></param>
        /// <returns></returns>
        public IAveWeb Add(string strWebUrl, string strTitle, string strDescription, uint nLCID, string strWebTemplate, bool useUniquePermissions, bool bConvertIfThere)
        {
            Dictionary<string, object> webProperties = new Dictionary<string, object>();
            if (this.mParentWeb != null)
            {
                if (strWebUrl.Contains("/"))
                {
                    strWebUrl = strWebUrl.Substring(strWebUrl.TrimEnd('/').LastIndexOf('/') + 1);
                }
                webProperties = this.mRequest.AddWeb(this.mParentWeb.ServerRelativeUrl, strWebUrl, strDescription, nLCID, strTitle, !useUniquePermissions, strWebTemplate, bConvertIfThere);
            }
            else
            {
                string parentWebServerRelativeUrl = string.Empty;
                if (strWebUrl.Contains("/"))
                {
                    parentWebServerRelativeUrl = strWebUrl.Substring(0, strWebUrl.TrimEnd('/').LastIndexOf('/'));
                    //strWebUrl = strWebUrl.Substring(strWebUrl.TrimEnd('/').LastIndexOf('/') + 1);
                }
                strWebUrl = strWebUrl.Substring(strWebUrl.TrimEnd('/').LastIndexOf('/') + 1);
                if (mSite.ServerRelativeUrl.Equals(strWebUrl))
                {
                    throw new Exception(string.Format("{0} is root web url", strWebUrl));
                }
                else
                {
                    webProperties = this.mRequest.AddWeb(parentWebServerRelativeUrl, strWebUrl, strDescription, nLCID, strTitle, !useUniquePermissions, strWebTemplate, bConvertIfThere);
                }
            }
            AveWeb web = new AveWeb(mRequest, mSite, this, webProperties);
            (mSite as AveSite).DataCache.AddWeakReferenceHandler("OpenWeb" + web.ServerRelativeUrl, web);
            mListData.Add(web);
            return web;
        }
        #endregion
        public void Dispose()
        {
            mListData = null;
        }
        public string WebUrlFromPageUrl(string pageUrl)
        {
            throw new NotImplementedException();
        }


        public System.Xml.XmlNode GetWeb(string siteDirUrl)
        {
            throw new NotImplementedException();
        }
    }
}
