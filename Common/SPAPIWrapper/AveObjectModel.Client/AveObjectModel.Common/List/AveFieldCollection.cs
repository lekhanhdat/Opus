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
    class AveFieldCollection : AveAbstractCommonCollection<IAveField> , IAveFieldCollection
    {
        private IAveRequest mRequest;
        public AveFieldCollection()
        {
            mListData = new List<IAveField>();
        }
        public AveFieldCollection(IAveRequest request, Dictionary<string,object> fieldColProperties)
        {
            mRequest = request;
            base.DataCache.AddPropertyies(fieldColProperties);
            InitFieldCollection();
        }
        internal void InitFieldCollection()
        {
            List<Dictionary<string, object>> fieldPropertiesList = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            mListData = new List<IAveField>(fieldPropertiesList.Count);
            foreach (Dictionary<string, object> fieldProperties in fieldPropertiesList)
            {
                AveField field = new AveField();
                field.DataCache.AddPropertyies(fieldProperties);
                mListData.Add(field);
            }
        }
        public String SchemaXml
        { 
            get
            {
                return base.DataCache.GetProperty<String>("SchemaXml");
            }
        }
        public new IAveField this[int index] 
        { 
            get
            {
                return mListData[index];
            } 
        }
        public IAveField this[Guid id] 
        { 
            get
            {
                return GetById(id);
            } 
        }
        public IAveField this[string name] 
        {
            get
            {
                return mListData.Find(
                   delegate(IAveField field)
                   {
                       return field.StaticName.Equals(name);
                   });
            }
        }

        public bool IsDirty
        {
            get 
            {
                throw new NotImplementedException(); 
            }
        }

        public IAveField Add(IAveField aveField)
        {
            mListData.Add(aveField);
            return aveField;
        }
        public IAveField AddFieldAsXml(String fieldXml)
        {
            throw new NotImplementedException();
        }
        public IAveField AddFieldAsXml(String fieldXml, bool addToDefaultView, AveAddFieldOptions op)
        {
            throw new NotImplementedException();
        }
        public bool Contains(Guid fieldId)
        {
            return mListData.Find(
                delegate(IAveField field) 
                {
                    return field.Id.Equals(fieldId);
                }) == null ? false : true;
        }
        public bool ContainsField(string fieldName)
        {
            return mListData.Find(
                delegate(IAveField field)
                {
                    return field.StaticName.Equals(fieldName);
                }) == null ? false : true;
        }
        public void Delete(string strName)
        {
            throw new NotImplementedException();
        }
        public IAveField GetById(Guid id)
        {
            return mListData.Find(
                    delegate(IAveField field)
                    {
                        return field.Id.Equals(id);
                    });
        }
        public IAveField GetFieldByInternalName(string internalName)
        {
            throw new NotImplementedException();
        }
        public IAveField GetByInfo(String name, String type)
        {
            return mListData.Find(
                    delegate(IAveField field)
                    {
                        return field.StaticName.Equals(name) && (field.Type == (AveFieldType)int.Parse(type));
                    });
        }
        public string Add(string strDisplayName, AveFieldType fieldType, bool bRequired)
        {
            throw new NotImplementedException();
        }
        public IAveField GetField(string strName)
        {
            throw new NotImplementedException();
        }
        public IAveField TryGetFieldByStaticName(string staticName)
        {
            return mListData.Find(
                    delegate(IAveField field)
                    {
                        return field.StaticName.Equals(staticName);
                    });
        }

        #region IAveFieldCollection Members


        public AveFieldCollectionInfo GetFieldInfoObj()
        {
            throw new NotImplementedException();
        }

        public List<string> GetFields()
        {
            throw new NotImplementedException();
        }

        public List<string> GetFieldsFromSchema(string fieldSchema)
        {
            throw new NotImplementedException();
        }

        public string GetList(IAveSite site, Guid webId, Guid listId)
        {
            throw new NotImplementedException();
        }

        public string GetWeb(IAveSite site, Guid webId)
        {
            throw new NotImplementedException();
        }

        public string TransListIdToTitle(IAveWeb aveWeb, IAveList aveList, string xml)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
