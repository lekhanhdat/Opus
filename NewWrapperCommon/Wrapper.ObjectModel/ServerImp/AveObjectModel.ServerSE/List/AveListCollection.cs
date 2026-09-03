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
using System.Net;
using System.Globalization;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System.Xml;
using Microsoft.SharePoint.Portal.WebControls;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveListCollection : AveAbstractCommonCollection<IAveList>, IAveListCollection
    {
        private ICredentials mListsCredentials;
        private string mListsUrl;
        private AveWeb mWeb;

        public AveListCollection(AveWeb web, SPListCollection lists)
            : base(lists)
        {
            mWeb = web;
        }

        /// <summary>
        /// Constructor Method for Lists
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="url"></param>
        public AveListCollection(ICredentials credentials, string url)
            : base(null)
        {
            mListsCredentials = credentials;
            mListsUrl = url;
        }

        internal AveList CreateListByType(SPList list)
        {
            return AveServerAssemblyInit.CreateElement(typeof(IAveList), new object[] { this, list }) as AveList;
        }

        internal SPListCollection Lists
        {
            get
            {
                return this.mWeb.Web.Lists;
            }
        }

        #region IAveListCollection Members

        public IAveList Add(AveListCreationInformation parameters)
        {
            string title = string.Empty;
            Guid guid;
            title = parameters.Title;
            string description = parameters.Description;
            string url = parameters.Url;
            IDictionary<string, string> dataSourceProperties = parameters.DataSourceProperties;
            SPListTemplate.QuickLaunchOptions quickLaunchOption = (SPListTemplate.QuickLaunchOptions)parameters.QuickLaunchOption;
            if ((dataSourceProperties != null) && (dataSourceProperties.Count > 0))
            {
                SPListDataSource datasrc = new SPListDataSource();
                foreach (KeyValuePair<string, string> pair in dataSourceProperties)
                {
                    datasrc.SetProperty(pair.Key, pair.Value);
                }
                string srurlDefaultView = null;
                //guid = this.Lists.Add(title, description, url, datasrc, null, quickLaunchOption, out srurlDefaultView);
                guid = (Guid)AveAssemblyUtility.InvokeMethod(this.Lists, typeof(SPListCollection), "Add", new object[] { title, description, url, datasrc, null, quickLaunchOption, srurlDefaultView });
            }
            else
            {
                string str4;
                if (parameters.TemplateFeatureId == Guid.Empty)
                {
                    str4 = null;
                }
                else
                {
                    str4 = parameters.TemplateFeatureId.ToString();
                }
                int templateType = parameters.TemplateType;
                string docTemplateType = null;
                if (parameters.DocumentTemplateType != 0)
                {
                    docTemplateType = parameters.DocumentTemplateType.ToString(CultureInfo.InvariantCulture);
                }
                string customSchemaXml = parameters.CustomSchemaXml;
                guid = this.Lists.Add(title, description, url, str4, templateType, docTemplateType, customSchemaXml, null, quickLaunchOption);
            }
            SPList list = this.Lists.GetList(guid, false);
            if (list == null)
            {
                return null;
            }
            return CreateListByType(list);
        }

        public IAveList GetById(Guid uniqueId)
        {
            SPList list = this.Lists[uniqueId];
            if (list == null)
            {
                return null;
            }
            return CreateListByType(list);
        }

        public IAveList GetByTitle(string strListName)
        {
            //由于切换语言会出现list在数据库里面的字段和list.Title属性不一致，所以先使用Title获取
            foreach (SPList list in this.Lists)
            {
                if (list.Title.Equals(strListName, StringComparison.OrdinalIgnoreCase))
                {
                    return CreateListByType(list);
                }
            }
            SPList tempList = this.Lists[strListName];
            return CreateListByType(tempList);
        }

        public IAveList this[Guid id]
        {
            get
            {
                return GetById(id);
            }
        }

        public IAveList this[string name]
        {
            get
            {
                return GetByTitle(name);
            }
        }

        public Guid Add(string strTitle, string strDescription, AveListTemplateType templateType)
        {
            return this.Lists.Add(strTitle, strDescription, (SPListTemplateType)templateType);
        }

        public Guid Add(string title, string description, IAveListTemplate template)
        {
            return this.Lists.Add(title, description, (template as AveListTemplate).ListTemplate);
        }

        public Guid Add(string title, string description, string url, IAveListDataSource dataSource)
        {
            return this.Lists.Add(title, description, url, (dataSource as AveListDataSource).ListDataSource);
        }

        public Guid Add(string title, string description, string url, string dataSource)
        {
            AveListDataSource dataS = new AveListDataSource(dataSource);
            return this.Add(title, description, url, dataS);
        }

        public void Delete(Guid uniqueID)
        {
            this.Lists.Delete(uniqueID);
        }

        public IAveList GetList(Guid uniqueId, bool fetchMetadata)
        {
            SPList list = this.Lists.GetList(uniqueId, fetchMetadata);
            if (list == null)
            {
                return null;
            }
            return CreateListByType(list);
        }

        public Guid Add(string title, string description, string url, string featureId, int templateType, string docTemplateType, AveQuickLaunchOptions quickLaunchOptions)
        {
            return this.Lists.Add(title, description, url, featureId, templateType, docTemplateType, (SPListTemplate.QuickLaunchOptions)quickLaunchOptions);
        }

        public XmlNode GetList(string listName)
        {
            Lists lists = new Lists();
            lists.Credentials = mListsCredentials;
            lists.Url = mListsUrl;
            return lists.GetList(listName);
        }

        #endregion

        public override IAveList this[int index]
        {
            get
            {
                return CreateListByType(this.Lists[index]);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return CreateListByType(t as SPList);
        }

        public override int Count
        {
            get { return this.Lists.Count; }
        }

        public XmlNode GetListCollection()
        {
            using (Lists lists = new Lists())
            {
                lists.Credentials = mListsCredentials;
                lists.Url = mListsUrl;
                return lists.GetListCollection();
            }
        }

        public IAveWeb Web
        {
            get
            {
                return mWeb;
            }
        }

        public Guid Add(string title, string description, string url, string featureId, int templateType, string docTemplateType)
        {
            return this.Lists.Add(title, description, url, featureId, templateType, docTemplateType);
        }

        public IAveList GetListByName(string strListName, bool bThrowException)
        {
            SPList temp = (SPList)AveAssemblyUtility.InvokeMethod(this.Lists, "GetListByName", new Type[] { typeof(string), typeof(bool) }, new object[] { strListName, bThrowException });
            if (temp == null)
            {
                return null;
            }
            return this.CreateListByType(temp);
        }

        public Guid Add(string title, string description, IAveListTemplate template, IAveDocTemplate documentTemplate)
        {
            return this.Lists.Add(title, description, (template as AveListTemplate).ListTemplate, (documentTemplate as AveDocTemplate).DocTemplate);
        }

        #region IAveListCollection Members

        internal void Reload()
        {
            base.mEnumerable = Lists;
        }

        public IAveList GetListById(Guid uniqueId, bool bThrowException)
        {
            SPList temp = (SPList)AveAssemblyUtility.InvokeMethod(this.Lists, "GetListById", new Type[] { typeof(Guid), typeof(bool) }, new object[] { uniqueId, bThrowException });
            if (temp == null)
            {
                return null;
            }
            return this.CreateListByType(temp);
        }

        public IAveList TryGetList(string listTitle)
        {
            SPList temp = this.Lists.TryGetList(listTitle);
            if (temp == null)
            {
                return null;
            }
            return this.CreateListByType(temp);
        }
        #endregion

        //This is to restore my task in SP2013
        public Guid Add(string title, string description, string url, string featureId, int templateType, string docTemplateType, string listSchema, AveQuickLaunchOptions quickLaunchOptions)
        {
            return this.Lists.Add(title, description, url, featureId, templateType, docTemplateType, listSchema, null, (SPListTemplate.QuickLaunchOptions)quickLaunchOptions);
        }
    }
}
