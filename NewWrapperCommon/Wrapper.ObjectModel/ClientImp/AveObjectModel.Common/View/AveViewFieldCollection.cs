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
    class AveViewFieldCollection : AveAbstractCommonCollection<string>, IAveViewFieldCollection
    {
        private IAveRequest mRequest;
        private IAveList mParentList;
        private AveView mView;

        public AveViewFieldCollection(IAveRequest request, IAveList parentList, AveView view, Dictionary<string, object> prop)
        {
            mRequest = request;
            mParentList = parentList;
            mView = view;
            InitSchemaXml(ref prop);
            base.DataCache.AddPropertyies(prop);
            InitDataList();
        }

        private void InitDataList()
        {
            this.mListData = new List<string>();
            foreach ( string field in base.DataCache.GetProperty<List<string>>(AveObjectModelConstant.ChildrenProperties) )
            {
                mListData.Add(field);
            }
        }

        private void InitSchemaXml(ref Dictionary<string,object> viewFieldCollectionProperty)
        {
            if (viewFieldCollectionProperty.ContainsKey("SchemaXml"))
            {
                Guid cacheHandlerId = (mParentList.ParentWeb as AveWeb).CacheHandlerId;
                string schema=viewFieldCollectionProperty["SchemaXml"].ToString();
                AveClientCacheHandler.WriteSchemaXml(schema, cacheHandlerId, mParentList.ParentWeb.ID.ToString(), mParentList.ID.ToString(), mParentList.ID.ToString(), SchemaType.ViewFieldCollection);
                viewFieldCollectionProperty.Remove("SchemaXml");
            }
        }

        #region IAveViewFieldCollection Members

        public void Add(IAveField field)
        {
            this.Add(field.StaticName);
        }

        public void Add(string strField)
        {
            if (!this.mView.DataCache.ChangedProperties.ContainsKey("AddViewFields"))
            {
                List<string> addViewFieldproperties = new List<string>();
                addViewFieldproperties.Add(strField);
                this.mView.DataCache.AddChangedProperty("AddViewFields", addViewFieldproperties);
            }
            else
            {
                this.mView.DataCache.GetProperty<List<string>>("AddViewFields").Add(strField);
            }
            mListData.Add(strField);
        }

        public void Remove(AveField field)
        {
            this.Remove(field.InternalName);
        }
        public void MoveFieldTo(string field, int index)
        {
            if (index < 0)
            {
                throw new Exception("Index out of bounds.");
            }
            string fieldName = mListData.Find(
                delegate(string fName)
                {
                    return fName.Equals(field);
                });
            if (!string.IsNullOrEmpty(fieldName))
            {
                if (!this.mView.DataCache.ChangedProperties.ContainsKey("MoveFieldTo"))
                {
                    List<Dictionary<string, object>> moveFieldList = new List<Dictionary<string, object>>();
                    this.mView.DataCache.AddChangedProperty("MoveFieldTo", moveFieldList);
                }
                Dictionary<string, object> moveFields = new Dictionary<string, object>();
                moveFields.Add("fieldName", field);
                moveFields.Add("index", index);
                ((List<Dictionary<string, object>>)this.mView.DataCache.ChangedProperties["MoveFieldTo"]).Add(moveFields);
                mListData.Remove(field);
                mListData.Insert(index, field);
            }
        }

        public void Remove(string strField)
        {
            string fieldName = mListData.Find(
                delegate(string fName)
                {
                    return fName.Equals(strField);
                });
            if (!string.IsNullOrEmpty(fieldName))
            {
                if (!this.mView.DataCache.ChangedProperties.ContainsKey("DeleteViewFields"))
                {
                    List<string> deleteViewFieldproperties = new List<string>();
                    deleteViewFieldproperties.Add(strField);
                    this.mView.DataCache.AddChangedProperty("DeleteViewFields", deleteViewFieldproperties);
                }
                else
                {
                    this.mView.DataCache.GetProperty<List<string>>("DeleteViewFields").Add(strField);
                }
                mListData.Remove(strField);
            }
        }

        public void RemoveAll()
        {
            this.mView.DataCache.AddChangedProperty("DeleteAllFields", "DeleteAll");
            mListData.Clear();
        }

        public bool Exists(string name)
        {
            string fieldName = mListData.Find(
                delegate(string str)
                {
                    return str.Equals(name);
                });
            if ( fieldName == null )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string SchemaXml
        {
            get
            {
                Guid cacheHandlerId = (mParentList.ParentWeb as AveWeb).CacheHandlerId;
                return AveClientCacheHandler.GetSchemaXml(cacheHandlerId, mParentList.ParentWeb.ID.ToString(), this.mParentList.ID.ToString(), this.mParentList.ID.ToString(), SchemaType.ViewFieldCollection);
            }
        }

        #endregion        

    }
}
