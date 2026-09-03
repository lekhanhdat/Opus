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
using System.IO;
using System.Globalization;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.Client;
using System.Reflection;
namespace AvePoint.ObjectModel.Common
{
    class AveFieldCollection : AveAbstractCommonCollection<IAveField>, IAveFieldCollection
    {
        private AveWeb mWeb;
        private AveList mParentList;
        private IAveRequest mRequest;
        private string mFieldColSource;
        private Dictionary<string, object> mContentTypeProp;
        private bool IsTaxCatchAllFieldAdded = false;
        protected static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveFieldCollection(AveWeb web, AveList list, IAveRequest request, string fieldSource, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldColProperties)
        {
            mWeb = web;
            mParentList = list;
            mFieldColSource = fieldSource;
            mRequest = request;
            mContentTypeProp = contentTypeProp;
            InitSchemaXml(ref fieldColProperties);
            base.DataCache.AddPropertyies(fieldColProperties);
            InitSchemaForFieldXml();//在初始化field前将所有field写入到field.xml,避免过多的IO操作
            InitFieldCollection();
        }

        private void InitSchemaXml(ref Dictionary<string, object> fieldCollectionProperties)
        {
            if (fieldCollectionProperties.ContainsKey("SchemaXml"))
            {
                string listId = string.Empty;
                string fieldCollectionId = string.Empty;
                if (mParentList != null)
                {
                    fieldCollectionId = mParentList.ID.ToString();
                    listId = mParentList.ID.ToString();
                }
                else
                {
                    fieldCollectionId = mWeb.ID.ToString();
                }
                AveClientCacheHandler.WriteSchemaXml(fieldCollectionProperties["SchemaXml"].ToString(), mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, fieldCollectionId, SchemaType.FieldCollection);
                fieldCollectionProperties.Remove("SchemaXml");
            }
        }

        internal void InitSchemaForFieldXml()
        {
            string listId = mParentList == null ? string.Empty : mParentList.ID.ToString();
            AveClientCacheHandler.WriteSchemaXml(GetFieldProperties(), this.mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, SchemaType.Field);
        }

        private IEnumerable<KeyValuePair<string, string>> GetFieldProperties()
        {
            var fieldPropertiesList = base.DataCache.GetChildren();
            foreach (var properties in fieldPropertiesList)
            {
                object idObject;
                object schemaXmlObject;
                if (properties.TryGetValue("Id", out idObject) && properties.TryGetValue("SchemaXml", out schemaXmlObject))
                {
                    string id = idObject.ToString();
                    string schemaXml = (string)schemaXmlObject;
                    yield return new KeyValuePair<string, string>(id, schemaXml);
                    properties.Remove("SchemaXml");
                }
            }
        }

        internal void InitFieldCollection()
        {
            var fieldPropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveField>(fieldPropertiesList.Count);
            foreach (var fieldProperties in fieldPropertiesList)
            {
                AveField field = CreateFieldByType(fieldProperties);
                mListData.Add(field);
            }
        }
        internal AveField CreateFieldByType(IDictionary<string, object> fieldProperties)
        {
            AveFieldType type;
            int rawType = (int)fieldProperties["Type"];
            string fieldTypeAsString = fieldProperties["TypeAsString"] as string;
            string realType = fieldProperties["RealType"] as string;
            try
            {
                type = (AveFieldType)rawType;
            }
            catch (Exception ex)
            {
                mLog.Warn("Get Field type:{0} failed.Error Message:{1}", rawType, ex.ToString());
                type = AveFieldType.Invalid;
            }
            switch (type)
            {
                case AveFieldType.Attachments:
                    return new AveFieldAttachments(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.Boolean:
                case AveFieldType.AllDayEvent:
                    return new AveFieldBoolean(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.Calculated:
                    return new AveFieldCalculated(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.Computed:
                    return new AveFieldComputed(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.Currency:
                    return new AveFieldCurrency(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.DateTime:
                    return new AveFieldDateTime(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.Lookup:
                    return new AveFieldLookup(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.ModStat:
                    return new AveFieldModStat(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.Choice:
                    return new AveFieldChoice(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.OutcomeChoice:
                    return new AveOutcomeChoiceField(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);    
                case AveFieldType.MultiChoice:
                    return new AveFieldMultiChoice(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.Note:
                    return new AveFieldMultiLineText(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.Number:
                case AveFieldType.Integer:
                case AveFieldType.WorkflowEventType:
                    return new AveFieldNumber(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.Text:
                    return new AveFieldText(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.URL:
                    return new AveFieldUrl(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.User:
                    return new AveFieldUser(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                case AveFieldType.Invalid:
                    if (fieldTypeAsString == "TaxonomyFieldType" || fieldTypeAsString == "TaxonomyFieldTypeMulti")
                    {
                        return new AveTaxonomyField(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                    }
                    else if (realType == "FieldNumber")
                    {
                        return new AveFieldNumber(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                    }
                    else
                    {
                        return new AveField(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
                    }
                default:
                    return new AveField(this.mRequest, this.mParentList, this.mWeb, this.mFieldColSource, this, this.mContentTypeProp, fieldProperties);
            }
            throw new Exception("type not support");
        }

        public String SchemaXml
        {
            get
            {
                string fieldCollectionId = mParentList == null ? mWeb.ID.ToString() : mParentList.ID.ToString();
                string listId = mParentList == null ? string.Empty : mParentList.ID.ToString();
                string schemaXml = AveClientCacheHandler.GetSchemaXml(mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, fieldCollectionId, SchemaType.FieldCollection);
                return schemaXml == string.Empty ? "<Fields></Fields>" : schemaXml;//ywzhang
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
                IAveField findedField = mListData.Find(
                   delegate(IAveField field)
                   {
                       return field.Title.Equals(name);
                   });
                if (findedField == null)
                {
                    throw new ArgumentException(string.Format("Field Not Exist{0}", name));
                }
                return findedField;
            }
        }

        public bool IsDirty
        {
            get
            {
                return false;
            }
            set
            { }
        }

        public IAveField Add(IAveField aveField)
        {
            return this.AddFieldAsXml(aveField.SchemaXml, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToNoContentType); //SAAS-7227
        }

        public IAveField AddFieldAsXml(String fieldXml)
        {
            return AddFieldAsXml(fieldXml, false, AveAddFieldOptions.DefaultValue);
        }

        public IAveField AddFieldAsXml(String fieldXml, bool addToDefaultView, AveAddFieldOptions op)
        {
            string listTitle = this.mParentList == null ? null : this.mParentList.Title;
            Guid listId = this.mParentList == null ? Guid.Empty : this.mParentList.ID;
            Dictionary<string, object> fieldProperties = this.mRequest.AddFieldAsXml(this.mWeb.ServerRelativeUrl, listTitle, listId, fieldXml, addToDefaultView, (int)op, this.mFieldColSource, this.mContentTypeProp);
            if (!IsTaxCatchAllFieldAdded && fieldProperties.ContainsKey("TypeAsString") && fieldProperties["TypeAsString"].Equals("TaxonomyFieldType"))
            {
                Dictionary<string, object> TaxCatchAllField = this.mRequest.GetTaxonomyCatchAllField(this.mWeb.ServerRelativeUrl, listTitle, listId);
                if (TaxCatchAllField != null)
                {
                    fieldProperties["TaxCatchAllField"] = TaxCatchAllField;
                }
            }
            if (fieldProperties.ContainsKey("TaxCatchAllField"))
            {
                if (!IsTaxCatchAllFieldAdded)
                {
                    Dictionary<string, object> taxFieldProperties = fieldProperties["TaxCatchAllField"] as Dictionary<string, object>;
                    AveTaxonomyField taxCatchAllField = new AveTaxonomyField(mRequest, mParentList, mWeb, this.mFieldColSource, this, this.mContentTypeProp, taxFieldProperties);
                    mListData.Add(taxCatchAllField);
                    IsTaxCatchAllFieldAdded = true;
                }
                fieldProperties.Remove("TaxCatchAllField");
            }
            if (fieldProperties.ContainsKey("TextField" + AveObjectModelConstant.ObjectPropertySuffix))
            {
                Dictionary<string, object> textFieldProperties = fieldProperties["TextField" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                AveTaxonomyField textField = new AveTaxonomyField(mRequest, mParentList, mWeb, this.mFieldColSource, this, this.mContentTypeProp, textFieldProperties);
                mListData.Add(textField);
            }
            AveField field = CreateFieldByType(fieldProperties);
            mListData.Add(field);
            //当一个field里面有RelatedField的时候，加进去后会把RelatedField也一并加进去，这时需要把RelatedField也给插进mListData中。
            if (!string.IsNullOrEmpty(field.RelatedField))
            {
                Dictionary<string, object> relatedFieldProperties = this.mRequest.GetRelatedFieldProperties(this.mWeb.ServerRelativeUrl, field.RelatedField, this.mFieldColSource, listTitle, listId);
                if (relatedFieldProperties != null && relatedFieldProperties.Count != 0)
                {
                    AveField relatedField = CreateFieldByType(relatedFieldProperties);
                    mListData.Add(relatedField);
                }
            }
            //AveFieldCollection webAvaiableFields = mWeb.AvailableFields as AveFieldCollection;
            if (mFieldColSource == "web.fields" )
            {
                AveFieldCollection webAvaiableFields = mWeb.AvailableFields as AveFieldCollection;
                if (webAvaiableFields != null)
                {
                    webAvaiableFields.ListData.Add(field);
                }
            }
            return field;
        }

        public bool Contains(Guid fieldId)
        {
            return mListData.Find(
                delegate(IAveField field)
                {
                    return field.ID.Equals(fieldId);
                }) == null ? false : true;
        }

        public bool ContainsField(string fieldName)
        {
            try
            {
                return GetField(fieldName) == null ? false : true;
            }
            catch (Exception e)
            {
                mLog.Info(e.ToString());
                return false;
            }
            //return mListData.Find(
            //    delegate(IAveField field)
            //    {
            //        return field.Title.Equals(fieldName);
            //    }) == null ? false : true;
        }

        public bool ContainsFieldWithInternalName(string fieldInternalName)
        {
            IAveField findedField = GetFieldByInternalName(fieldInternalName, false);
            return findedField != null;
        }

        public void Delete(string strName)
        {
            string listTitle = this.mParentList == null ? null : this.mParentList.Title;
            Guid listId = this.mParentList == null ? Guid.Empty : this.mParentList.ID;
            this.mRequest.DeleteField(this.mWeb.ServerRelativeUrl, listTitle, listId, strName, this.mFieldColSource, this.mContentTypeProp);
            mListData.Remove(this.GetField(strName));
        }

        public IAveField GetById(Guid id)
        {
            IAveField findedField = mListData.Find(
                    delegate(IAveField field)
                    {
                        return field.ID.Equals(id);
                    });
            if (findedField == null)
            {
                throw new ArgumentException(string.Format("Field Not Exist{0}", id.ToString("B")));
            }
            return findedField;
        }

        private IAveField GetFieldByInternalName(string internalName, bool isSafe)
        {
            IAveField findedField = mListData.Find(
                    delegate(IAveField field)
                    {
                        return field.InternalName.Equals(internalName);
                    });
            return findedField;
        }

        public IAveField GetFieldByInternalName(string internalName)
        {
            IAveField findedField = GetFieldByInternalName(internalName, false);
            if (findedField == null)
            {
                throw new ArgumentException(string.Format("Field Not Exist {0}", internalName));
            }
            return findedField;
        }

        public IAveField GetByInfo(String name, String type)
        {
            return mListData.Find(
                    delegate(IAveField field)
                    {
                        return field.Title.Equals(name) && (field.Type.ToString().Equals(type));
                    });
        }

        public string Add(string strDisplayName, AveFieldType fieldType, bool bRequired)
        {
            string xml = this.GetFieldXml(strDisplayName, fieldType, bRequired);
            return this.AddFieldAsXml(xml).Title;
        }

        public IAveField GetField(string strName)
        {
            IAveField field = GetFieldByInternalName(strName, true);
            if (field == null)
            {
                field = TryGetFieldByStaticName(strName);
            }

            if (field == null)
            {
                field = TryGetFieldByEntityPropertyName(strName);
            }
            if (field == null)
            {
                field = this[strName];
            }
            return field;
        }

        public IAveField TryGetFieldByEntityPropertyName(string entityPropertyName)
        {
            return mListData.Find(delegate(IAveField field)
            {
                return string.Equals(entityPropertyName, field.EntityPropertyName);
            });
        }

        public IAveField TryGetFieldByStaticName(string staticName)
        {
            return mListData.Find(
                    delegate(IAveField field)
                    {
                        return field.StaticName.Equals(staticName);
                    });
        }
        public AveFieldCollectionInfo GetFieldInfoObj()
        {
            return GetFieldInfoObj(new AveBackupOption());
        }

        public AveFieldCollectionInfo GetFieldInfoObj(AveBackupOption backupOption, IAveList list = null, String fieldSchema = "")
        {
            AveFieldCollectionInfo fieldInfos = new AveFieldCollectionInfo();
            try
            {
                List<AveTaxFieldInfo> taxFieldInfos = new List<AveTaxFieldInfo>();
                if (string.IsNullOrEmpty(fieldSchema))
                {
                    List<string> fields = GetFields();
                    XmlDocument xDoc = new XmlDocument();
                    StringBuilder schema = new StringBuilder();
                    foreach (string field in fields)
                    {
                        xDoc.InnerXml = field;
                        AveFieldInfo fieldInfo = new AveFieldInfo();
                        fieldInfo.Name = xDoc.FirstChild.Attributes["Name"].Value;
                        fieldInfo.SchemaXml = field;
                        fieldInfos.Fields.Add(fieldInfo);
                        schema.Append(field);
                    }
                    fieldSchema = "<Fields>" + schema.ToString() + "</Fields>";
                }
                var tempInfo = new AveFieldCollectionInfo();
                tempInfo.AveSchemaXml = fieldSchema;
                if (backupOption.BeforeExportFieldsAction != null)
                {
                    backupOption.BeforeExportFieldsAction(tempInfo);
                }
                fieldInfos.AveSchemaXml = TransListIdToTitle(mWeb, list, tempInfo.AveSchemaXml, ref taxFieldInfos, backupOption.BackupMultiLanguage);
                if (backupOption.BackupRelatedTermSets || backupOption.BackupRelatedTermsOnly)
                {
                    fieldInfos.RelatedMetadataInfo = GetRelatedMetadataInfo(mWeb.Site, taxFieldInfos, backupOption);
                }
            }
            catch (Exception e)
            {
                mLog.Warn(AveObjectModel_CommonResource.GetFieldInfoObjectError, this.List != null ? this.List.Title : string.Empty, this.mWeb != null ? this.mWeb.Url : string.Empty, e.ToString());
                // mLog.Log(AveLogLevel.WARN, "WP10BKAveSPFC583", e);
            }
            return fieldInfos;
        }

        public List<string> GetFields()
        {
            string listId = mParentList == null ? string.Empty : mParentList.ID.ToString();
            var sxhemaXmlMapping = AveClientCacheHandler.GetSchemaXmlElements(this.mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, SchemaType.Field);

            List<string> fields = new List<string>();
            //获取type为lookup的field
            var fieldMap = new Dictionary<Guid, AveField>();
            foreach (AveField field in mListData)
            {
                if (field.Type == AveFieldType.Lookup)
                {
                    fieldMap.Add(field.ID, field);
                }
            }
            foreach (var keyValue in sxhemaXmlMapping)
            {
                Guid id = new Guid(keyValue.Key);
                string schema = keyValue.Value.OuterXml;
                string type = keyValue.Value.GetAttribute("Type");
                if ("Lookup".Equals(type, StringComparison.OrdinalIgnoreCase))
                {
                    AveField field;
                    if (fieldMap.TryGetValue(id, out field))
                    {
                        schema = AddWebFieldSpecialProperty(field, keyValue.Value);
                    }
                }
                fields.Add(schema);
            }
            return fields;
        }


        private string AddWebFieldSpecialProperty(AveField field, XmlElement element)
        {
            if (!element.HasAttribute("IsRelationship"))
            {
                element.SetAttribute("IsRelationship", (field as AveFieldLookup).IsRelationship.ToString());
            }
            return element.OuterXml;
        }

        public List<string> GetFieldsFromSchema(string fieldSchema)
        {
            List<string> fields = new List<string>();
            XmlDocument xDoc = new XmlDocument();
            try
            {
                xDoc.InnerXml = fieldSchema;
                foreach (XmlNode node in xDoc.FirstChild.ChildNodes)
                {
                    fields.Add(node.OuterXml);
                }
            }
            catch (Exception e)
            {
                mLog.Debug(AveWebServiceRequestResource.GetFieldsFromSchemaXml, this.mParentList.Title, this.mWeb.Url, e.ToString());
                throw;
            }
            return fields;
        }

        public string GetList(IAveSite site, Guid webId, Guid listId)
        {
            string listTitle = string.Empty;
            try
            {
                IAveWeb web = site.OpenWeb(webId);
                IAveList list = web.Lists[listId];
                if (list != null)
                {
                    listTitle = list.Title;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Can't get list{0} with web id:{1}.Error Message:{2}", listId, webId, ex.ToString());
                return string.Empty;
            }
            return listTitle;
        }

        public string GetWeb(IAveSite site, Guid webId)
        {
            string webUrl = string.Empty;
            try
            {
                IAveWeb web = site.OpenWeb(webId);
                webUrl = web.ServerRelativeUrl;
            }
            catch (Exception ex)
            {
                mLog.Warn("Can't get web{0}.Error Message:{1}", webId, ex.ToString());
                return string.Empty;
            }
            return webUrl;
        }

        public string TransListIdToTitle(IAveWeb aveWeb, IAveList aveList, string xml)
        {
            List<AveTaxFieldInfo> temp = null;
            return TransListIdToTitle(aveWeb, aveList, xml, ref temp, false);
        }

        public string TransListIdToTitle(IAveWeb aveWeb, IAveList aveList, string xml, ref List<AveTaxFieldInfo> taxFieldInfos, bool backupMultiLanguage)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            IDictionary<Guid, IDictionary<string, object>> fieldsPropertiesDictionary = null;
            if (backupMultiLanguage && aveWeb.IsMultilingual)
            {
                var fieldsProperties = base.DataCache.GetChildren();
                fieldsPropertiesDictionary = fieldsProperties.ToDictionary(key => (Guid)key["Id"], value => value);
            }

            foreach (XmlElement element in doc.DocumentElement.GetElementsByTagName("Field"))
            {
                XmlElement fieldInfoElement = doc.CreateElement("AveFieldInfo");

                // 备份User Resource
                try
                {
                    if (fieldsPropertiesDictionary != null && fieldsPropertiesDictionary.Count > 0)
                    {
                        XmlElement resourceElement = doc.CreateElement(AveUserResourceConstants.RESOURCE_NODE);
                        if (GetTitleAndDescriptionResource(aveWeb, doc, element, resourceElement, fieldsPropertiesDictionary))
                        {
                            element.AppendChild(resourceElement);
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("failed to backup user resource, field info:{0}, due to:{1}", element.OuterXml, ex);
                }
                element.AppendChild(fieldInfoElement);
                //????
                try
                {
                    string sourceId = element.GetAttribute("SourceID");
                    string sourceType = "-1";
                    if (AveSPCommonUtility.IsGuid(sourceId))
                    {
                        Guid sourceGUID = new Guid(sourceId);
                        if (aveList != null && sourceGUID.Equals(aveList.ID))
                        {
                            sourceType = "2";
                        }
                        else if (IsParentWebID(sourceGUID, aveWeb))
                        {
                            sourceType = "1";
                        }
                        else
                        {
                            sourceType = "0";
                        }
                    }
                    fieldInfoElement.SetAttribute("AveSourceType", sourceType);
                }
                catch (Exception ex)
                {
                    mLog.Warn("Set XmlElement Attribute:AveSourceType failed.Error Message:{0}", ex.ToString());
                }
                if (AveSPUtility.GetFieldType(element.GetAttribute("Type")) == AveFieldType.Lookup || element.GetAttribute("Type") == "TaxonomyFieldType" || element.GetAttribute("Type") == "TaxonomyFieldTypeMulti")
                {
                    if (GetRelationship(aveWeb.Site.ID.ToString(), aveList == null ? string.Empty : aveList.ID.ToString(), element))
                    {
                        fieldInfoElement.SetAttribute("IsRelationship", "True");
                    }
                    if (!element.HasAttribute("List") && !element.HasAttribute("WebId"))
                    {
                        continue;
                    }
                    string listId = element.GetAttribute("List");
                    string webId = element.GetAttribute("WebId");
                    string webUrl = String.Empty;
                    string listTitle = String.Empty;
                    try
                    {
                        if (String.IsNullOrEmpty(webId) ||
                            webId.Equals(Guid.Empty.ToString()) ||
                            webId.Equals(aveWeb.ID.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            webId = aveWeb.ID.ToString();
                            webUrl = "/" + aveWeb.ServerRelativeUrl.Trim('/');
                        }
                        else
                        {
                            webUrl = "/" + GetWeb(aveWeb.Site, new Guid(webId)).Trim('/');
                        }

                        if ((listId == "Self") && (aveList != null))
                        {
                            listTitle = aveList.Title;
                            listId = aveList.ID.ToString("B");
                        }
                        else if (listId == "Docs")
                        {
                            listTitle = string.Empty;
                        }
                        else if (listId == "UserInfo" && aveWeb != null && aveWeb != null)
                        {
                            listTitle = aveWeb.SiteUserInfoList.Title;
                            listId = aveWeb.SiteUserInfoList.ID.ToString("B");
                        }
                        else if (AveSPCommonUtility.IsGuid(listId))
                        {
                            listTitle = GetList(aveWeb.Site, new Guid(webId), new Guid(listId));

                            if (string.IsNullOrEmpty(listTitle))
                            {
                                mLog.Warn("Cannot get list information for field:{0}", element.OuterXml);
                            }
                        }
                        else
                        {
                            listTitle = string.Empty;
                        }

                        if (element.HasAttribute("List"))
                        {
                            fieldInfoElement.SetAttribute("AveLookupListTitle", listTitle);
                            fieldInfoElement.SetAttribute("AveLookupListID", listId);
                            fieldInfoElement.SetAttribute("AveLookupWebTitle", webUrl);
                        }
                        
                    }
                    catch (Exception e)
                    {
                        mLog.Warn(AveObjectModel_CommonResource.TransListIdToTitleError, listTitle, this.mWeb.Url, e.ToString());
                        //mLog.Log(AveLogLevel.WARN, "WP10BKAveSPFC456", element.GetAttribute("Name"), e);
                    }
                }
                else if (AveSPUtility.GetFieldType(element.GetAttribute("Type")) == AveFieldType.User)
                {
                    fieldInfoElement.SetAttribute("IsRelationship", GetRelationship(aveWeb.Site.ID.ToString(), aveList == null ? string.Empty : aveList.ID.ToString(), element).ToString());
                }
                //TaxonomyField 是一个固定指向Taxonomyhiddenlist的lookupfield。
                if (element.GetAttribute("Type") == "TaxonomyFieldType" || element.GetAttribute("Type") == "TaxonomyFieldTypeMulti")
                {
                    AveTaxFieldInfo taxFieldInfo = new AveTaxFieldInfo();
                    SetTaxonomyField(aveWeb.Site, element, taxFieldInfo);
                    if ((taxFieldInfo.TermSetId != Guid.Empty || taxFieldInfo.IsKeywordsColumn) && taxFieldInfos != null)
                    {
                        taxFieldInfos.Add(taxFieldInfo);
                    }
                }
            }
            return doc.OuterXml;
        }

        private bool GetTitleAndDescriptionResource(IAveWeb aveWeb, XmlDocument doc, XmlElement fieldElement, XmlElement resourceElement, IDictionary<Guid, IDictionary<string, object>> fieldsPropertiesDictionary)
        {
            var change = false;

            string id = fieldElement.GetAttribute("ID");
            if (!string.IsNullOrEmpty(id))
            {
                Guid fieldId = new Guid(id);
                IDictionary<string, object> properties = new Dictionary<string, object>();
                if (fieldsPropertiesDictionary.TryGetValue(fieldId, out properties))
                {
                    change |= CreateResourceNode(AveUserResourceConstants.TITLE_RESOUCE_NODE, AveUserResourceConstants.TITLE_RESOUCE, properties, doc, resourceElement);
                    change |= CreateResourceNode(AveUserResourceConstants.DESCRIPTION_RESOUCE_NODE, AveUserResourceConstants.DESCRIPTION_RESOUCE, properties, doc, resourceElement);
                }
            }

            return change;
        }

        private bool CreateResourceNode(string nodeName, string propKey, IDictionary<string, object> properties, XmlDocument xmlDoc, XmlElement parentNode)
        {
            object resource;
            if (properties.TryGetValue(propKey, out resource))
            {
                var values = resource as Dictionary<string, string>;
                if (values != null)
                {
                    XmlElement resourceNode = xmlDoc.CreateElement(nodeName);
                    foreach (var item in values)
                    {
                        resourceNode.SetAttribute(item.Key, item.Value);
                    }

                    parentNode.AppendChild(resourceNode);
                    return true;
                }
            }
            return false;
        }

        private bool IsParentWebID(Guid sourceGUID, IAveWeb aveWeb)
        {
            if (aveWeb != null && aveWeb.Exists)
            {
                if (sourceGUID.Equals(aveWeb.ID))
                {
                    return true;
                }
                else
                {
                    if (!aveWeb.IsRootWeb)
                    {
                        return IsParentWebID(sourceGUID, aveWeb.ParentWeb);
                    }
                }
            }
            return false;
        }


        private void SetTaxonomyField(IAveSite site, XmlElement mElement, AveTaxFieldInfo taxFieldInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Client.AveFieldCollection.SetTaxonomyField"))
            {
                Guid SspId = Guid.Empty;
                Guid GroupId = Guid.Empty;
                Guid TermSetId = Guid.Empty;
                Guid AnchorId = Guid.Empty;
                foreach (XmlElement customElement in mElement)
                {
                    if (customElement.Name.Equals("Default"))
                    {
                        if (customElement.InnerText != null)
                        {
                            string[] values = customElement.InnerText.Split(';');
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (values[i].StartsWith("#", StringComparison.OrdinalIgnoreCase) && values[i].IndexOf('|') > 0)
                                {
                                    string strTermId = values[i].Substring(values[i].IndexOf('|') + 1);
                                    Guid temp = Guid.Empty;
                                    if (Guid.TryParse(strTermId, out temp))
                                    {
                                        taxFieldInfo.TermIds.Add(temp);
                                    }
                                    else
                                    {
                                        mLog.Warn("get taxonomy field default value failed.xml:{0}", customElement.OuterXml);
                                    }
                                }
                            }
                        }
                        continue;
                    }
                    if (customElement.Name.Equals("Customization"))
                    {
                        foreach (XmlElement element in customElement.ChildNodes)
                        {
                            if (element.Name.Equals("ArrayOfProperty"))
                            {
                                foreach (XmlElement propertyElement in element.ChildNodes)
                                {
                                    try
                                    {
                                        if (propertyElement.Name.Equals("Property"))
                                        {
                                            string name = null;
                                            string value = null;
                                            XmlNodeList elements = propertyElement.GetElementsByTagName("Name");
                                            if (elements != null && elements.Count > 0)
                                            {
                                                XmlElement nameElement = (XmlElement)elements[0];
                                                name = nameElement.InnerText;
                                            }
                                            elements = propertyElement.GetElementsByTagName("Value");
                                            if (elements != null && elements.Count > 0)
                                            {
                                                XmlElement valueElement = (XmlElement)elements[0];
                                                value = valueElement.InnerText;
                                            }
                                            if (name == null)
                                            {
                                                throw new ArgumentNullException();
                                            }
                                            if (name.Equals("SspId"))
                                            {
                                                if (value != null)
                                                {
                                                    try
                                                    {
                                                        SspId = new Guid(value);
                                                        if (SspId != Guid.Empty)
                                                        {
                                                            string termStoreName = AveTaxonomyFieldUtility.GetTermStoreName(site, SspId);
                                                            if (string.IsNullOrEmpty(termStoreName))
                                                            {
                                                                SspId = Guid.Empty;
                                                                mLog.Log(AveLogLevel.DEBUG, "Cannot Get TermStore, TermStoreId: {0}", SspId);
                                                            }
                                                            else
                                                            {
                                                                AvePoint.GCommon.Utility.ArgumentCheck.NotNull(elements, nameof(elements));
                                                                elements[0].InnerText = value + "|" + termStoreName;
                                                            }
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        mLog.Log(AveLogLevel.DEBUG, "An error occurred while getting term store name  , message:{0}", e.ToString());
                                                        SspId = Guid.Empty;
                                                    }
                                                    taxFieldInfo.SspId = SspId;
                                                }
                                            }
                                            else if (name.Equals("GroupId"))
                                            {
                                                if (value != null)
                                                {
                                                    try
                                                    {
                                                        GroupId = new Guid(value);
                                                        if (GroupId != Guid.Empty)
                                                        {
                                                            string groupName = AveTaxonomyFieldUtility.GetTermGroupName(site, ref SspId, GroupId);
                                                            if (string.IsNullOrEmpty(groupName))
                                                            {
                                                                GroupId = Guid.Empty;
                                                                mLog.Log(AveLogLevel.DEBUG, "cannot get term group, groupId:{0}", GroupId);
                                                            }
                                                            else
                                                            {
                                                                elements[0].InnerText = value + "|" + groupName;
                                                            }
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        mLog.Log(AveLogLevel.DEBUG, "An error occurred while getting term group name   , message:{0}", e.ToString());
                                                        GroupId = Guid.Empty;
                                                    }
                                                    taxFieldInfo.GroupId = GroupId;
                                                }
                                            }
                                            else if (name.Equals("TermSetId"))
                                            {
                                                if (value != null)
                                                {
                                                    try
                                                    {
                                                        TermSetId = new Guid(value);
                                                        if (TermSetId != Guid.Empty)
                                                        {
                                                            string termSetName = AveTaxonomyFieldUtility.GetTermSetName(site, ref SspId, ref GroupId, TermSetId);
                                                            if (string.IsNullOrEmpty(termSetName))
                                                            {
                                                                mLog.Log(AveLogLevel.WARN, "cannot get taxonomy field related term set, it may be deleted or removed. term set id:{0}", TermSetId);
                                                            }
                                                            else
                                                            {
                                                                elements[0].InnerText = value + "|" + termSetName;
                                                            }
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        mLog.Log(AveLogLevel.DEBUG, "An error occurred while getting term set name   , message:{0}", e.ToString());
                                                        TermSetId = Guid.Empty;
                                                    }
                                                    taxFieldInfo.SspId = SspId;
                                                    taxFieldInfo.GroupId = GroupId;
                                                    taxFieldInfo.TermSetId = TermSetId;
                                                }
                                            }
                                            else if (name.Equals("AnchorId"))
                                            {
                                                if (value != null)
                                                {
                                                    try
                                                    {
                                                        AnchorId = new Guid(value);
                                                        if (AnchorId != Guid.Empty)
                                                        {
                                                            string termName = AveTaxonomyFieldUtility.GetTermName(site, SspId, TermSetId, AnchorId);
                                                            elements[0].InnerText = value + "|" + termName;
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        mLog.Log(AveLogLevel.DEBUG, "An error occurred while getting term name   , message:{0}", e.ToString());
                                                        AnchorId = Guid.Empty;
                                                    }
                                                    if (AnchorId != Guid.Empty)
                                                    {
                                                        taxFieldInfo.TermIds.Add(AnchorId);
                                                    }
                                                }
                                            }
                                            else if (name.Equals("IsKeyword", StringComparison.OrdinalIgnoreCase))
                                            {
                                                ArgumentCheck.CheckNotNull(value);
                                                if (value.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    taxFieldInfo.IsKeywordsColumn = true;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        mLog.Log(AveLogLevel.DEBUG, "An error occurred while setting taxonomy field  , message:{0}", e.ToString());
                                    }
                                }
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }

        private bool GetRelationship(string siteId, string listId, XmlElement FieldNode)
        {
            if (string.IsNullOrEmpty(listId))
            {
                return false;
            }
            if (!FieldNode.HasAttribute("IsRelationship"))//
            {
                return false;
            }
            return Convert.ToBoolean(FieldNode.GetAttribute("IsRelationship"));
        }

        public IAveField CreateNewField(string fieldType, string fieldName)
        {
            AveFieldType type = (AveFieldType)Enum.Parse(typeof(AveFieldType), fieldType);
            string xml = this.GetFieldXml(fieldName, type, false);
            return AddFieldAsXml(xml);
        }

        public IAveList List
        {
            get
            {
                return this.mParentList;
            }
        }

        private string GetFieldXml(string strDisplayName, AveFieldType fieldType, bool bRequired)
        {
            string str = strDisplayName;
            if ((((fieldType == AveFieldType.Attachments) || (fieldType == AveFieldType.Recurrence)) || ((fieldType == AveFieldType.CrossProjectLink) || (fieldType == AveFieldType.Computed))) || ((fieldType == AveFieldType.File) || (fieldType == AveFieldType.AllDayEvent)))
            {
                throw new InvalidOperationException();
            }
            if (((fieldType == AveFieldType.GridChoice) && (this.mParentList != null)) && (this.mParentList.BaseType != AveBaseType.Survey))
            {
                throw new InvalidOperationException();
            }
            string str2 = bRequired ? "TRUE" : "FALSE";
            StringBuilder sb = new StringBuilder();
            StringWriter w = new StringWriter(sb, CultureInfo.InvariantCulture);
            XmlTextWriter writer2 = new XmlTextWriter(w);
            writer2.WriteStartElement("Field");

            writer2.WriteAttributeString("DisplayName", str);
            writer2.WriteAttributeString("Type", fieldType.ToString());
            writer2.WriteAttributeString("Required", str2);
            switch (fieldType)
            {
                case AveFieldType.Lookup:
                    //throw new ArgumentException();
                    string Id = this.mParentList == null ? null : this.mParentList.ID.ToString();
                    writer2.WriteAttributeString("List", Id);
                    writer2.WriteEndElement();
                    break;
                case AveFieldType.Calculated:
                    writer2.WriteAttributeString("ResultType", "Text");
                    writer2.WriteStartElement("Formula");
                    writer2.WriteString("=\"\"");
                    writer2.WriteEndElement();
                    break;
                case AveFieldType.User:
                    writer2.WriteAttributeString("List", "UserInfo");
                    writer2.WriteEndElement();
                    break;
            }
            writer2.Flush();
            writer2.Close();
            w.Flush();
            w.Close();
            return sb.ToString();
        }

        public bool ContainsFieldWithStaticName(string p)
        {
            return this.TryGetFieldByStaticName(p) != null;
        }

        public Dictionary<string, object> GetDisplayFields(IAveViewFieldCollection viewFields)
        {
            Dictionary<string, object> displayFields = new Dictionary<string, object>();
            if (viewFields == null)
            {
                return displayFields;
            }
            displayFields.Add("Title", null);
            displayFields.Add("Created", null);
            displayFields.Add("Modified", null);
            try
            {
                foreach (string fd in viewFields)
                {
                    if (!displayFields.ContainsKey(fd))
                    {
                        displayFields.Add(fd, null);
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Debug(AveObjectModel_CommonResource.GetDisplayFieldsError, this.List.Title, this.mWeb.Url, e.ToString());
                //mLog.Log(AveLogLevel.WARN, "WP10BKAveSPFC174", e);
            }
            return displayFields;
        }

        public Dictionary<string, object> GetDisplayFields(string viewFieldsSchema)
        {
            Dictionary<string, object> displayFields = new Dictionary<string, object>();
            if (viewFieldsSchema == null)
            {
                return displayFields;
            }
            displayFields.Add("Title", null);
            displayFields.Add("Created", null);
            displayFields.Add("Modified", null);
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml("<AveViewFields>" + viewFieldsSchema + "</AveViewFields>");
                foreach (XmlNode node in xDoc.GetElementsByTagName("FieldRef"))
                {
                    string name = node.Attributes["Name"].Value;
                    if (!displayFields.ContainsKey(name))
                    {
                        displayFields.Add(name, null);
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Debug(AveObjectModel_CommonResource.GetDisplayFieldsErrorWithSchemaString, this.List.Title, this.mWeb.Url, e.ToString());
                //mLog.Log(AveLogLevel.WARN, "WP10BKAveSPFC206", e);
            }
            return displayFields;
        }

        public string GetViewFields(Guid siteID, Guid listID)
        {
            IAveView defaultView = mParentList.DefaultView;
            if (defaultView != null)
            {
                return defaultView.ViewFields.SchemaXml;
            }
            return null;
        }

        public string GetFields(Guid webId, Guid listId)
        {
            return AddListFieldSpecialProperty(this.SchemaXml);
        }

        public List<string> GetInternalNamesBySchema()
        {
            List<string> internalNames = new List<string>();            
            try
            {
                foreach (IAveField field in this)
                {
                    internalNames.Add(field.InternalName);
                }
            }
            catch (Exception e)
            {
                mLog.Debug(string.Format("Get fields internal name list failed. List:{0}, Web:{1}, Message:{2}", this.List.Title, this.mWeb.Url, e.ToString()));
            }
            return internalNames;

        }

        /// <summary>
        /// Wrapper逻辑层需要的某些属性需要server和client同时赋值，暂时看到的有lookup column的 IsRelationship属性
        /// </summary>
        /// <param name="fieldXml"></param>
        /// <returns></returns>
        private string AddListFieldSpecialProperty(string fieldXml)
        {
            mLogger.Info("AddListFieldSpecialProperty,Schema:{0}",fieldXml);
            XmlDocument fieldDocumentXml = new XmlDocument();
            fieldDocumentXml.LoadXml(fieldXml);
            foreach (XmlElement fieldElement in fieldDocumentXml.GetElementsByTagName("Field"))
            {
                string fieldType = fieldElement.GetAttribute("Type");
                bool containValue = fieldElement.HasAttribute("IsRelationship");
                if (fieldType.Equals("Lookup", StringComparison.OrdinalIgnoreCase) && !containValue)
                {
                    IAveField field = null;
                    try
                    {
                        field = GetById(new Guid(fieldElement.GetAttribute("ID")));
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Can not get field by Field Id, field information in xml is:{0}.Error Message:{1}", fieldElement.OuterXml, ex.ToString());
                    }
                    if (field != null && field is IAveFieldLookup)
                    {
                        fieldElement.SetAttribute("IsRelationship", (field as IAveFieldLookup).IsRelationship.ToString());
                    }
                }
                else if (fieldType.Equals("Calculated", StringComparison.OrdinalIgnoreCase))//Calculated column 也需要加下
                {
                    IAveField field = null;
                    try
                    {
                        field = GetById(new Guid(fieldElement.GetAttribute("ID")));
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Can not get field by Field Id, field information in xml is:{0}.Error Message:{1}", fieldElement.OuterXml, ex.ToString());
                    }
                    //if (field != null && field is IAveFieldCalculated)
                    //{
                    //    if (!fieldElement.HasAttribute("Formula") )
                    //    {
                    //        fieldElement.SetAttribute("Formula", (field as IAveFieldCalculated).Formula.ToString());
                    //    }
                    //    if (!fieldElement.HasAttribute("DateFormat"))
                    //    {
                    //        fieldElement.SetAttribute("DateFormat", (field as IAveFieldCalculated).DateFormat.ToString());
                    //    }
                    //}
                }
            }
            return fieldDocumentXml.OuterXml;

        }


        public List<string> GetFields(Guid siteId, string scope)
        {
            List<string> fields = new List<string>();
            foreach (AveField field in this.mListData)
            {
                fields.Add(field.SchemaXml);
            }
            return fields;
        }

        public IAveField GetFieldById(Guid fieldId, bool bThrowException)
        {            
            IAveField findedField = mListData.Find(
                    delegate(IAveField field)
                    {
                        return field.ID == fieldId;
                    });
            if (findedField == null && bThrowException)
            {
                throw new ArgumentException(string.Format("Field Not Exist {0}", fieldId));                
            }
            return findedField;
        }

        IAveField IAveFieldCollection.GetFieldByInternalName(string strName, bool bThrowException)
        {
            return this.GetFieldByInternalName(strName, bThrowException);
        }

        public bool GetFieldInSiteChildren(string scope, Guid siteId, Guid fieldId)
        {
            throw new NotImplementedException();
        }


        public IAveField AddLookup(string displayName, Guid lookupListId, Guid lookupWebId, bool bRequired)
        {
            String lookupXml = this.GetFieldXml(displayName, AveFieldType.Lookup, bRequired);
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(lookupXml);
            xDoc.DocumentElement.SetAttribute("StaticName", displayName);
            xDoc.DocumentElement.SetAttribute("Name", displayName);
            xDoc.DocumentElement.SetAttribute("List", lookupListId.ToString());
            xDoc.DocumentElement.SetAttribute("ID", Guid.NewGuid().ToString());
            return this.AddFieldAsXml(xDoc.OuterXml);
        }

        private List<AveTermStoreInfo> GetRelatedMetadataInfo(IAveSite site, List<AveTaxFieldInfo> taxFieldInfos, AveBackupOption backupOption)
        {
            if (taxFieldInfos == null || taxFieldInfos.Count == 0)
            {
                return  new List<AveTermStoreInfo>();
            }
            AveMetaDataServiceSerializer serializer = (site as AveSite).MetaDataServiceSerializer as AveMetaDataServiceSerializer;
            return serializer.GetRelatedMetadataInfo(site, taxFieldInfos, backupOption);
        }

        public Dictionary<string, string> GetFieldMap(IAveFieldCollection fields) 
        {
            string listId = mParentList == null ? string.Empty : mParentList.ID.ToString();

            return AveClientCacheHandler.GetSchemaXmlMapping(this.mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, SchemaType.Field);
        }
    }
}
