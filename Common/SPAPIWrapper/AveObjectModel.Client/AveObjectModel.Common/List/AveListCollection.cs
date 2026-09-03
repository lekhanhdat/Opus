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
using System.Xml;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    class AveListCollection : AveAbstractCommonCollection<IAveList>, IAveListCollection
    {

        private IAveRequest mRequest;
        private AveWeb mParentWeb;
        private readonly object privateLock = new object();

        public AveListCollection(IAveRequest request, AveWeb web, Dictionary<string, object> listsPro)
        {
            mRequest = request;
            mParentWeb = web;
            base.DataCache.AddPropertyies(listsPro);
            InitListProperites();
        }

        private void InitListProperites()
        {
            var listPropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveList>(listPropertiesList.Count);
            foreach (var dic in listPropertiesList)
            {
                lock (privateLock)
                {
                    mListData.Add(CreateInstance(dic));
                }
            }
        }

        private AveList CreateInstance(IDictionary<string, object> listProperties)
        {
            AveList list = null;
            switch ((AveBaseType)listProperties["BaseType"])
            {
                case AveBaseType.DocumentLibrary:
                    list = new AveDocumentLibrary(mRequest, mParentWeb, listProperties);
                    break;
                default:
                    list = new AveList(mRequest, mParentWeb, listProperties);
                    break;
            }
            return list;
        }

        #region IAveListCollection Members

        public Guid Add(string title, string description, IAveListTemplate template, string featureId = null)
        {
            if (!template.IsCustomTemplate)
            {
                return Add(title, description, template.Type, template.FeatureId.ToString());
            }
            Dictionary<string, object> prop = this.mRequest.AddList(this.mParentWeb.ServerRelativeUrl, title, description, template);
            AveList newList = CreateInstance(prop);
            lock (privateLock)
            {
                mListData.Add(newList);
            }
            return newList.ID;
        }

        public Guid Add(string title, string description, string url, string dataSource)
        {
            AveListDataSource dataS = new AveListDataSource(dataSource);
            return this.Add(title, description, url, dataS);
        }

        public Guid Add(string title, string description, string url, IAveListDataSource dataSource)
        {
            Dictionary<string, object> dataSD = new Dictionary<string, object>();
            dataSD.Add(AveBDCProperties.Entity, dataSource.GetProperty(AveBDCProperties.Entity));
            dataSD.Add(AveBDCProperties.EntityNamespace, dataSource.GetProperty(AveBDCProperties.EntityNamespace));
            dataSD.Add(AveBDCProperties.LobSystemInstance, dataSource.GetProperty(AveBDCProperties.LobSystemInstance));
            dataSD.Add(AveBDCProperties.SpecificFinder, dataSource.GetProperty(AveBDCProperties.SpecificFinder));
            Dictionary<string, object> newListProp = mRequest.AddList(this.mParentWeb.ServerRelativeUrl, title, description, url, dataSD);
            AveList newList = new AveList(mRequest, mParentWeb, newListProp);
            lock (privateLock)
            {
                mListData.Add(newList);
            }
            return newList.ID;
        }

        public IAveList Add(AveListCreationInformation listCreationInfo)
        {
            //copy properties to dic           
            Dictionary<string, object> newListProp = mRequest.AddList(this.mParentWeb.ServerRelativeUrl, listCreationInfo.Title, listCreationInfo.Description, listCreationInfo.Url, listCreationInfo.TemplateFeatureId.ToString(), listCreationInfo.TemplateType, listCreationInfo.DocumentTemplateType.ToString(), (int)listCreationInfo.QuickLaunchOption);
            AveList newList = CreateInstance(newListProp);
            lock (privateLock)
            {
                mListData.Add(newList);
            }
            return newList;
        }

        public Guid Add(string strTitle, string strDescription, AveListTemplateType templateType, string featureId = null)
        {
            Dictionary<string, object> prop = mRequest.AddList(this.mParentWeb.ServerRelativeUrl, strTitle, strDescription, (int)templateType, featureId);
            AveList newList = CreateInstance(prop);
            lock (privateLock)
            {
                mListData.Add(newList);
            }
            return newList.ID;
        }

        public Guid Add(string title, string description, string url, string featureId, int templateType, string docTemplateType, AveQuickLaunchOptions quickLaunchOptions)
        {
            Dictionary<string, object> prop = mRequest.AddList(this.mParentWeb.ServerRelativeUrl, title, description, url, featureId, templateType, docTemplateType, (int)quickLaunchOptions);
            AveList list = CreateInstance(prop);
            lock (privateLock)
            {
                mListData.Add(list);
            }
            return list.ID;
        }

        public IAveList GetById(Guid uniqueId)
        {
            lock (privateLock)
            {
                return mListData.Find(
                     delegate (IAveList list)
                     {
                         return list.ID.Equals(uniqueId);
                     });
            }
        }

        public IAveList GetByTitle(string strListName)
        {
            lock (privateLock)
            {
                return mListData.Find(
                    delegate(IAveList list)
                    {
                        return list.Title?.Equals(strListName, StringComparison.OrdinalIgnoreCase) == true;
                    });
            }
        }

        public IAveList GetList(Guid uniqueId, bool fetchMetadata)
        {
            return GetById(uniqueId);
        }

        public IAveList this[Guid id]
        {
            get
            {
                //GetById doesn't throw out exception, but this function does.
                IAveList temp = GetById(id);
                if (temp == null)
                {
                    throw new ArgumentException("The list specified by Guid:" + id.ToString() + " does not exist");
                }
                if (temp.Exception != null)
                {
                    throw temp.Exception;
                }
                return temp;
            }
        }

        public IAveList this[string name]
        {
            get
            {
                //GetByTitle doesn't throw out exception, but this function does.
                IAveList temp = GetByTitle(name);
                if (temp == null)
                {
                    throw new ArgumentException("The list specified by strListName:" + name + " does not exist");
                }
                if (temp.Exception != null)
                {
                    throw temp.Exception;
                }
                return temp;
            }
        }

        public void Delete(Guid uniqueID)
        {
            IAveList list = this[uniqueID];
            list.Delete();
        }

        public XmlNode GetList(string listName)
        {
            throw new NotImplementedException();
        }

        public XmlNode GetListCollection()
        {
            throw new NotImplementedException();
        }

        public IAveWeb Web
        {
            get
            {
                return mParentWeb;
            }
        }
        #endregion

        public Guid Add(string title, string description, string url, string featureId, int templateType, string docTemplateType)
        {
            return this.Add(title, description, url, featureId, templateType, docTemplateType, AveQuickLaunchOptions.DefaultValue);
        }

        public IAveList GetListByName(string strListName, bool bThrowException)
        {
            return GetByTitle(strListName);
            }

        public Guid Add(string title, string description, IAveListTemplate template, IAveDocTemplate documentTemplate)
        {
            throw new NotImplementedException();
        }

        public IAveList GetListById(Guid uniqueId, bool bThrowException)
        {
            //throw new NotImplementedException();
            return GetById(uniqueId);
        }

        public IAveList TryGetList(string strListName)
        {
            return GetByTitle(strListName);
        }

        internal void EnsureTitleResource(string cultureName, Dictionary<Guid, string> titleResource)
        {
            foreach(var list in this)
            {
                string newTitle = string.Empty;
                if(titleResource.TryGetValue(list.ID, out newTitle))
                {
                    ((AveUserResource)list.TitleResource).EnsureResource(cultureName, newTitle);
                }
            }
        }
    }
}
