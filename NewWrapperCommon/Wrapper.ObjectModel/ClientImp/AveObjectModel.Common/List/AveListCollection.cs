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
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveListCollection));
        private IAveRequest mRequest;
        private AveWeb mParentWeb;
        private object privateLock = new object();
        private bool isDirty = false;
        internal bool IsDirty
        {
            get
            {
                lock (privateLock)
                {
                    return isDirty;
                }
            }
            set
            {
                lock (privateLock)
                {
                    isDirty = value;
                }
            }
        }

        [Obsolete]
        public AveListCollection(IAveRequest request, AveWeb web, Dictionary<string, object> listsPro)
        {
            mRequest = request;
            mParentWeb = web;
            lock (privateLock)
            {
                base.DataCache.AddPropertyies(listsPro);
                InitListProperites();
            }
        }

        public AveListCollection(IAveRequest request, AveWeb web)
        {
            mRequest = request;
            mParentWeb = web;
            lock (privateLock)
            {
                Dictionary<string, object> listsPro = request.GetLists(web.ServerRelativeUrl);
                base.DataCache.AddPropertyies(listsPro);
                InitListProperites();
            }
        }

        internal void UpdateCollectionInternally(IAveRequest request, AveWeb web)
        {
            lock (privateLock)
            {
                Dictionary<string, object> listsPro = request.GetLists(web.ServerRelativeUrl);
                base.DataCache.RemoveProperty(AveObjectModelConstant.ChildrenProperties);
                mListData.Clear();
                base.DataCache.AddPropertyies(listsPro);
                InitListProperites();
                IsDirty = false;
            }
        }

        private void InitListProperites()
        {
            List<Dictionary<string, object>> listPropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveList>(listPropertiesList.Count);
            foreach (Dictionary<string, object> dic in listPropertiesList)
            {
                mListData.Add(CreateInstance(dic));
            }
        }

        private AveList CreateInstance(Dictionary<string, object> listProperties)
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

        public Guid Add(string title, string description, IAveListTemplate template)
        {
            if (!template.IsCustomTemplate)
            {
                return this.Add(title, description, template.FeatureId, template.Type);
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

        public Guid Add(string strTitle, string strDescription, AveListTemplateType templateType)
        {
            return Add(strTitle, strDescription, Guid.Empty, templateType);
        }

        private Guid Add(string strTitle, string strDescription, Guid featureId, AveListTemplateType templateType)
        {
            Dictionary<string, object> prop = mRequest.AddList(this.mParentWeb.ServerRelativeUrl, strTitle, strDescription, featureId, (int)templateType);
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
                    delegate (IAveList list)
                    {
                        return list.Title.Equals(strListName, StringComparison.OrdinalIgnoreCase);
                    });
            }
        }

        public IAveList TryGetList(string strListName)
        {
            return GetByTitle(strListName);
        }

        public IAveList GetList(Guid uniqueId, bool fetchMetadata)
        {
            return GetById(uniqueId);
        }

        public IAveList this[Guid id]
        {
            get
            {
                //return mListData.Find(
                //    delegate(IAveList list)
                //    {
                //        return list.ID.Equals(id);
                //    });
                //此处逻辑和getByid（）相同，改用getByid（）和server逻辑相同
                IAveList temp = GetById(id);
                if (temp == null)
                {
                    throw new ArgumentException("The list specified by Guid:" + id.ToString() + " does not exist.");
                }
                return temp;
            }
        }

        public IAveList this[string name]
        {
            get
            {
                //return mListData.Find(
                //    delegate(IAveList list)
                //    {
                //        return list.Title.Equals(name, StringComparison.OrdinalIgnoreCase);
                //    });

                //此处逻辑和getByTitle（）相同，改用getByTitle（）和server逻辑相同
                IAveList temp = GetByTitle(name);
                if (temp == null)
                {
                    throw new ArgumentException("The list specified by ListName:" + name + " does not exist.");
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
            //throw new NotImplementedException();
            lock (privateLock)
            {
                return mListData.Find(
                    delegate (IAveList list)
                    {
                        return list.Title.Equals(strListName, StringComparison.OrdinalIgnoreCase);
                    });
            }
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

        //This is to restore my task in SP2013
        public Guid Add(string title, string description, string url, string featureId, int templateType, string docTemplateType, string listSchema, AveQuickLaunchOptions quickLaunchOptions)
        {
            throw new NotImplementedException();
        }
    }
}
