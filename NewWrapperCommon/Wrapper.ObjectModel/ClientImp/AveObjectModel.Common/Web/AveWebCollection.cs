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
        public ISPWebCollectionProvider WebCollectionProvider { internal get; set; }
        private readonly IAveRequest mRequest;
        private readonly AveSite mSite;
        private bool websDataEnabled;

        private readonly object websDataLock = new object();

        public AveWebCollection(IAveRequest request, AveSite site, ISPWebCollectionProvider webCollectionProvider)
        {
            WebCollectionProvider = webCollectionProvider;
            mRequest = request;
            mSite = site;
            mListData = new List<IAveWeb>();
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
            var proerties = this.WebCollectionProvider.Add(strWebUrl, strTitle, strDescription, nLCID, strWebTemplate, useUniquePermissions, bConvertIfThere);
            SetWebsDataAsDirty();
            return new AveWeb(this.mSite, proerties);
        }

        public string WebUrlFromPageUrl(string pageUrl)
        {
            throw new NotImplementedException();
        }

        public System.Xml.XmlNode GetWeb(string siteDirUrl)
        {
            throw new NotImplementedException();
        }

        public IAveWeb this[Guid webId]
        {
            get
            {
                return this.mSite.OpenWeb(webId);
            }
        }

        public IAveWeb this[string name]
        {
            get
            {
                return this.WebCollectionProvider.OpenWeb(name);
            }
        }

        public int Count
        {
            get
            {
                EnsureWebsData();
                return this.mListData.Count;
            }
        }

        private void EnsureWebsData()
        {
            lock (websDataLock)
            {
                if (!this.websDataEnabled)
                {
                    mListData.Clear();
                    foreach (var webDictionary in this.WebCollectionProvider.GetWebsData())
                    {
                        AveWeb web = new AveWeb(this.mSite, (Guid)webDictionary["Id"]);
                        mListData.Add(web);
                    }
                    websDataEnabled = true;
                }
            }
        }

        public IEnumerator<IAveWeb> GetEnumerator()
        {
            return this.WebCollectionProvider.GetWebsData().Select(data => this.mSite.OpenWeb((Guid)data["Id"])).GetEnumerator();
        }

        internal void SetWebsDataAsDirty()
        {
            this.websDataEnabled = false;
        }


        /// <summary>
        /// 此处模仿LocalAPI的实现，主要用来区分处理Web.Webs 和AveSite.AllWebs的不同逻辑
        /// </summary>
        internal interface ISPWebCollectionProvider
        {
            IEnumerable<Dictionary<string, object>> GetWebsData();
            Dictionary<string, object> Add(string strWebUrl, string strTitle, string strDescription, uint nLCID, string strWebTemplate, bool useUniquePermissions, bool bConvertIfThere);
            IAveWeb OpenWeb(string name);
        }
    }
}
