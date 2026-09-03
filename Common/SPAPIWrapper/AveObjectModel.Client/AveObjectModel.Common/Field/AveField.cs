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
using System.Xml;
using System.Reflection;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    class AveField : AveClientObject, IAveField
    {
        private static AveLogger Logger = AveLogger.GetInstance(typeof(AveField));  
        private IAveRequest mRequest;
        private AveList mParentList;
        private AveWeb mWeb;
        private AveFieldCollection mFieldCollection;
        private string mFieldSource;
        private IDictionary<string, object> mContentTypeProp;
        private string mMD5;
        private readonly object mSchemaLock = new object();
        private AveFieldSource fieldSourceScope;

        public AveField(IAveRequest request, AveList list, AveWeb web, string fieldSource, AveFieldCollection fieldCollection, IDictionary<string, object> contentTypeProp, IDictionary<string, object> prop)
        {
            mRequest = request;
            mParentList = list;
            mWeb = web;
            mFieldCollection = fieldCollection;
            mFieldSource = fieldSource;
            fieldSourceScope = GetFieldSourceScope(list, fieldSource, contentTypeProp);
            mContentTypeProp = contentTypeProp;
            base.DataCache.AddChangedProperty("fieldSource", mFieldSource);
            InitSchemaXml(ref prop);
            base.DataCache.AddPropertyies(prop);
        }

        private AveFieldSource GetFieldSourceScope(AveList list,string fieldSourceString,IDictionary<string, object> contentTypeProp)
        {
            if (list == null||list.ID==Guid.Empty)
            {
                if (contentTypeProp == null || contentTypeProp.Count == 0)
                {
                    if (string.Equals(fieldSourceString, "web.availableFields", StringComparison.OrdinalIgnoreCase))
                    {
                        return AveFieldSource.WebAvaliableFields;
                    }
                    return AveFieldSource.WebFields;
                }
                return AveFieldSource.WebContentTypeFields;
            }
            if (contentTypeProp == null || contentTypeProp.Count == 0)
            {
                return AveFieldSource.ListFields;
            }
            return AveFieldSource.ListContentTypeFields;
        }

        public string AggregationFunction
        {
            get { return base.DataCache.GetProperty<string>("AggregationFunction"); }
            set { base.DataCache.AddChangedProperty("AggregationFunction", value); }
        }
        public bool? AllowDeletion
        {
            get { return base.DataCache.GetProperty<bool?>("AllowDeletion"); }
            set { base.DataCache.AddChangedProperty("AllowDeletion", value); }
        }
        // this is only used in sql for server mode
        public string ColName
        {
            get { return base.DataCache.GetProperty<string>("ColName"); }
        }
        public string DefaultFormula
        {
            get { return base.DataCache.GetProperty<string>("DefaultFormula"); }
            set { base.DataCache.AddChangedProperty("DefaultFormula", value); }
        }
        public string Description
        {
            get { return base.DataCache.GetProperty<string>("Description"); }
            set { base.DataCache.AddChangedProperty("Description", value); }
        }
        public string Direction
        {
            get { return base.DataCache.GetProperty<string>("Direction"); }
            set { base.DataCache.AddChangedProperty("Direction", value); }
        }
        public string DisplaySize
        {
            get { return base.DataCache.GetProperty<string>("DisplaySize"); }
            set { base.DataCache.AddChangedProperty("DisplaySize", value); }
        }
        public string Group
        {
            get { return base.DataCache.GetProperty<string>("Group"); }
            set { base.DataCache.AddChangedProperty("Group", value); }
        }
        public bool Hidden
        {
            get { return base.DataCache.GetProperty<bool>("Hidden"); }
            set { base.DataCache.AddChangedProperty("Hidden", value); }
        }
        public Guid ID
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }
        public string IMEMode
        {
            get { return base.DataCache.GetProperty<string>("IMEMode"); }
            set { base.DataCache.AddChangedProperty("IMEMode", value); }
        }
        public string InternalName
        {
            get { return base.DataCache.GetProperty<string>("InternalName"); }
        }
        public bool Indexed
        {
            get { return base.DataCache.GetProperty<bool>("Indexed"); }
            set { base.DataCache.AddChangedProperty("Indexed", value); }
        }
        public bool LinkToItem
        {
            get { return base.DataCache.GetProperty<bool>("LinkToItem"); }
            set { base.DataCache.AddChangedProperty("LinkToItem", value); }
        }
        public bool NoCrawl
        {
            get { return base.DataCache.GetProperty<bool>("NoCrawl"); }
            set { base.DataCache.AddChangedProperty("NoCrawl", value); }
        }
        public XmlNode Node
        {
            get { return base.DataCache.GetProperty<XmlNode>("Node"); }
        }
        public string JumpToField
        {
            get { return base.DataCache.GetProperty<string>("JumpToField"); }
            set { base.DataCache.AddChangedProperty("JumpToField", value); }
        }
        public string PIAttribute
        {
            get { return base.DataCache.GetProperty<string>("PIAttribute"); }
            set { base.DataCache.AddChangedProperty("PIAttribute", value); }
        }
        public string PITarget
        {
            get { return base.DataCache.GetProperty<string>("PITarget"); }
            set { base.DataCache.AddChangedProperty("PITarget", value); }
        }
        public string PrimaryPIAttribute
        {
            get { return base.DataCache.GetProperty<string>("PrimaryPIAttribute"); }
            set { base.DataCache.AddChangedProperty("PrimaryPIAttribute", value); }
        }
        public string PrimaryPITarget
        {
            get { return base.DataCache.GetProperty<string>("PrimaryPITarget"); }
            set { base.DataCache.AddChangedProperty("PrimaryPITarget", value); }
        }
        public string RelatedField
        {
            get { return base.DataCache.GetProperty<string>("RelatedField"); }
            set { base.DataCache.AddChangedProperty("RelatedField", value); }
        }
        public bool ReadOnlyField
        {
            get { return base.DataCache.GetProperty<bool>("ReadOnlyField"); }
            set { base.DataCache.AddChangedProperty("ReadOnlyField", value); }
        }
        public bool Required
        {
            get { return base.DataCache.GetProperty<bool>("Required"); }
            set { base.DataCache.AddChangedProperty("Required", value); }
        }

        public String SchemaXml
        {
            get
            {
                string listId = mParentList == null ? string.Empty : mParentList.ID.ToString();
                string schemaXml = AveClientCacheHandler.GetSchemaXml(mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, this.ID.ToString(), SchemaType.Field);
                return schemaXml == string.Empty ? "<Fields></Fields>" : schemaXml;//ywzhang
            }
            set { base.DataCache.AddChangedProperty("SchemaXml", value); }
        }
        public bool Sealed
        {
            get { return base.DataCache.GetProperty<bool>("Sealed"); }
            set { base.DataCache.AddChangedProperty("Sealed", value); }
        }
        public string SchemaXmlWithResourceTokens
        {
            get { return base.DataCache.GetProperty<string>("SchemaXmlWithResourceTokens"); }
        }
        public bool? ShowInDisplayForm
        {
            get { return base.DataCache.GetProperty<bool?>("ShowInDisplayForm"); }
            set { base.DataCache.AddChangedProperty("ShowInDisplayForm", value); }
        }
        public bool? ShowInEditForm
        {
            get { return base.DataCache.GetProperty<bool?>("ShowInEditForm"); }
            set { base.DataCache.AddChangedProperty("ShowInEditForm", value); }
        }
        public bool? ShowInListSettings
        {
            get { return base.DataCache.GetProperty<bool?>("ShowInListSettings"); }
            set { base.DataCache.AddChangedProperty("ShowInListSettings", value); }
        }
        public bool? ShowInNewForm
        {
            get { return base.DataCache.GetProperty<bool?>("ShowInNewForm"); }
            set { base.DataCache.AddChangedProperty("ShowInNewForm", value); }
        }
        public bool ShowInVersionHistory
        {
            get
            {
                if (base.DataCache.IsPropertyAvailable("ShowInVersionHistory"))
                {
                    return base.DataCache.GetProperty<bool>("ShowInVersionHistory");
                }
                else
                {
                    return (!this.Hidden && !this.ReadOnlyField);
                }
            }
            set
            {
                base.DataCache.AddChangedProperty("ShowInVersionHistory", value);
            }
        }
        public bool? ShowInViewForms
        {
            get { return base.DataCache.GetProperty<bool?>("ShowInViewForms"); }
            set { base.DataCache.AddChangedProperty("ShowInViewForms", value); }
        }
        public String StaticName
        {
            get
            {
                return base.DataCache.GetProperty<String>("StaticName");
            }
            set
            {
                base.DataCache.AddChangedProperty("StaticName", value);
            }
        }
        public String Title
        {
            get { return base.DataCache.GetProperty<string>("Title"); }
            set { base.DataCache.AddChangedProperty("Title", value); }
        }
        public string TranslationXml
        {
            get
            {
                if (base.DataCache.IsPropertyAvailable("TranslationXml"))
                {
                    return base.DataCache.GetProperty<string>("TranslationXml");
                }
                return string.Empty;
            }
            set
            {
                base.DataCache.AddChangedProperty("TranslationXml", value);
            }
        }
        public AveFieldType Type
        {
            get
            {
                return base.DataCache.GetProperty<AveFieldType>("Type");
            }
            set
            {
                base.DataCache.AddChangedProperty("Type", value);
            }
        }
        public String TypeAsString
        {
            get { return base.DataCache.GetProperty<string>("TypeAsString"); }
            set { base.DataCache.AddChangedProperty("TypeAsString", value); }
        }
        public string TypeDisplayName
        {
            get { return base.DataCache.GetProperty<string>("TypeDisplayName"); }
        }
        public string ValidationFormula
        {
            get { return base.DataCache.GetProperty<string>("ValidationFormula"); }
            set { base.DataCache.AddChangedProperty("ValidationFormula", value); }
        }
        public string ValidationMessage
        {
            get { return base.DataCache.GetProperty<string>("ValidationMessage"); }
            set { base.DataCache.AddChangedProperty("ValidationMessage", value); }
        }
        public AveCompositeIndexableStatus CompositeIndexable
        {
            get { return base.DataCache.GetProperty<AveCompositeIndexableStatus>("CompositeIndexable"); }
        }
        public string DefaultValue
        {
            get { return base.DataCache.GetProperty<string>("DefaultValue"); }
            set { base.DataCache.AddChangedProperty("DefaultValue", value); }
        }
        public IAveFieldTypeDefinition FieldTypeDefinition
        {
            get { return base.DataCache.GetProperty<IAveFieldTypeDefinition>("FieldTypeDefinition"); }
        }
        public bool Reorderable
        {
            get
            {
                if (this.Hidden)
                {
                    if (((this.InternalName == "ContentType") || (this.Type == AveFieldType.Attachments)) || this.Hidden)
                    {
                        return false;
                    }
                    if (this.ReadOnlyField && (this.Type != AveFieldType.Calculated))
                    {
                        return false;
                    }
                }
                return true;

            }
        }
        public bool Indexable
        {
            get { return base.DataCache.GetProperty<bool>("Indexable"); }
        }
        public string SourceId
        {
            get { return base.DataCache.GetProperty<string>("SourceId"); }
        }
        public Type FieldValueType
        {
            get { throw new NotImplementedException(); }
        }
        public IAveList ParentList
        {
            get
            {
                return this.mParentList;
            }
        }

        public void Delete()
        {
            string listTitle = this.mParentList == null ? null : this.mParentList.Title;
            Guid listId = this.mParentList == null ? Guid.Empty : this.mParentList.ID;
            this.mRequest.DeleteField(this.mWeb.ServerRelativeUrl, listTitle, listId, this.InternalName, this.mFieldSource, this.mContentTypeProp);
            this.mFieldCollection.ListData.Remove(this);
        }
        public string GetProperty(string propName)
        {
            return null;
        }
        public virtual void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 1)  //fieldsource is always in changedproperites
            {
                try
                {
                    base.DataCache.AddChangedProperty("ObjectPath", base.DataCache.GetProperty<object>("ObjectPath"));
                    base.DataCache.AddChangedProperty("FieldType", base.DataCache.GetProperty<Type>("FieldType"));
                    //base.DataCache.AddChangedProperty("ClientContext", base.DataCache.GetProperty<object>("ClientContext"));
                    string listTitle = this.mParentList == null ? null : this.mParentList.Title;
                    Guid listID = this.mParentList == null ? Guid.Empty : this.mParentList.ID;
                    #region --Set NoCrawl Property--
                    //由于NoCrawl这个属性在Client模式下没有相应的API，所以采用更新SchemaXml的方式去还原这个属性。
                    string isNoCrawl = string.Empty;
                    if (base.DataCache.ChangedProperties.ContainsKey("NoCrawl"))
                    {
                        string xml = this.SchemaXml;//base.DataCache.GetProperty<string>("SchemaXml");
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xml);
                        XmlElement ele = xmlDoc.DocumentElement;
                        isNoCrawl = base.DataCache.ChangedProperties["NoCrawl"].ToString().ToUpper();
                        ele.SetAttribute("NoCrawl", isNoCrawl);
                        base.DataCache.ChangedProperties.Remove("NoCrawl");
                        foreach (KeyValuePair<string, object> pair in base.DataCache.ChangedProperties)
                        {
                            if (ele.HasAttribute(pair.Key))
                            {
                                ele.SetAttribute(pair.Key, pair.Value.ToString());
                            }
                        }
                        base.DataCache.AddChangedProperty("SchemaXml", xmlDoc.InnerXml);
                    }
                    #endregion
                    Dictionary<string, object> fieldProperties = InternalUpdate(listTitle, listID);
                    if (fieldProperties.ContainsKey("SchemaXml"))
                    {
                        string listId = this.mParentList != null ? this.mParentList.ID.ToString() : string.Empty;
                        AveClientCacheHandler.WriteSchemaXml(fieldProperties["SchemaXml"].ToString(), this.mWeb.CacheHandlerId, this.mWeb.ID.ToString(), listId, this.ID.ToString(), SchemaType.Field);
                        fieldProperties.Remove("SchemaXml");
                    }
                    if (base.DataCache.ChangedProperties.ContainsKey("Type"))
                    {
                        (this.mFieldCollection as AveFieldCollection).ListData.Remove(this);
                        AveField newField = (this.mFieldCollection as AveFieldCollection).CreateFieldByType(fieldProperties);
                        (this.mFieldCollection as AveFieldCollection).ListData.Add(newField);
                        if (base.DataCache.ChangedProperties.ContainsKey("fieldSource"))
                        {
                            newField.DataCache.AddChangedProperty("fieldSource", base.DataCache.ChangedProperties["fieldSource"]);
                        }
                        return;
                    }
                    base.DataCache.AddPropertyies(fieldProperties);
                    Dictionary<string, object> fieldSource = null;
                    if (base.DataCache.ChangedProperties.ContainsKey("fieldSource"))
                    {
                        fieldSource = new Dictionary<string, object>();
                        fieldSource.Add("fieldSource", base.DataCache.ChangedProperties["fieldSource"]);
                    }
                    base.DataCache.ChangedProperties.Clear();
                    if (fieldSource != null)
                    {
                        base.DataCache.AddChangedProperties(fieldSource);
                    }
                }
                catch
                {
                    Dictionary<string, object> fieldSource = null;
                    if (base.DataCache.ChangedProperties.ContainsKey("fieldSource"))
                    {
                        fieldSource = new Dictionary<string, object>();
                        fieldSource.Add("fieldSource", base.DataCache.ChangedProperties["fieldSource"]);
                    }
                    base.DataCache.ChangedProperties.Clear();
                    if (fieldSource != null)
                    {
                        base.DataCache.AddChangedProperties(fieldSource);
                    }
                    throw;
                }
            }
        }

        protected virtual Dictionary<string, object> InternalUpdate(string listTitle, Guid listId)
        {
            return this.mRequest.UpdateField(this.mWeb.ServerRelativeUrl, listTitle, listId, this.InternalName, this.mFieldSource, this.mContentTypeProp, base.DataCache.ChangedProperties, string.Empty);
        }

        public void UpdateReadOnlyField()
        {
            if (base.DataCache.ChangedProperties.Count > 1)  //fieldsource is always in changedproperites
            {
                try
                {
                    base.DataCache.AddChangedProperty("ObjectPath", base.DataCache.GetProperty<object>("ObjectPath"));
                    base.DataCache.AddChangedProperty("FieldType", base.DataCache.GetProperty<Type>("FieldType"));
                    //base.DataCache.AddChangedProperty("ClientContext", base.DataCache.GetProperty<object>("ClientContext"));
                    base.DataCache.ChangedProperties.Remove("ReadOnly");
                    string listTitle = this.mParentList == null ? null : this.mParentList.Title;
                    Guid listID = this.mParentList == null ? Guid.Empty : this.mParentList.ID;
                    Dictionary<string, object> fieldProperties = this.mRequest.UpdateReadOnlyField(this.mWeb.ServerRelativeUrl, listTitle, listID, this.InternalName, this.mFieldSource, this.mContentTypeProp, base.DataCache.ChangedProperties);
                    base.DataCache.AddPropertyies(fieldProperties);
                }
                catch
                {
                    base.DataCache.ChangedProperties.Clear();
                    base.DataCache.ChangedProperties["fieldSource"] = mFieldSource;
                    throw;
                }
            }
        }




        public bool RemoveFieldAttributeValue(string attrName)
        {
            return true;
        }

        public string SetFieldAttributeValue(string attrName, string attrValue)
        {
            return string.Empty;
        }

        public void SetFieldValue(string name, object value)
        {

        }

        /// <summary>
        /// API mode，如果不存在返回值为null，需要外围注意
        /// </summary>
        /// <param name="attrName"></param>
        /// <returns></returns>
        public string GetFieldAttributeValue(string attrName)
        {
            return GetAttributeFromSchemaXml(attrName);
        }

        public virtual string GetFieldValueAsText(object uV)
        {
            if (uV == null)
            {
                return string.Empty;
            }
            return uV.ToString();
        }

        public object GetPropertyValue(string ColName)
        {
            return string.Empty;
        }

        public virtual object GetFieldValue(string value)
        {
            return value;
        }

        public string GetAttributeFromSchemaXml(string attrName)
        {
            lock (mSchemaLock)
            {
                if (base.DataCache.IsPropertyNotLoaded("SchemaXmlDocument"))
                {
                    XmlDocument xmlD = new XmlDocument();
                    xmlD.LoadXml(this.SchemaXml);
                    base.DataCache.AddProperty("SchemaXmlDocument",xmlD);
                }
            }
            XmlDocument sxd = DataCache.GetPropertyWithoutChange<XmlDocument>("SchemaXmlDocument");
            XmlNode node = sxd.SelectSingleNode("Field");
            if (node != null && node.Attributes[attrName] != null)
            {
                return node.Attributes[attrName].Value;
            }
            else
            {
                return null;
            }
        }
        public int RowOrdinal
        {
            get { return base.DataCache.GetProperty<int>("RowOrdinal"); }
        }


        public object GetCustomProperty(string p)
        {
            XmlDocument doc = new XmlDocument();
            doc.InnerXml = this.SchemaXml;
            foreach (XmlElement customElement in doc.FirstChild.ChildNodes)
            {
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
                                        object value = null;
                                        XmlNodeList elements = propertyElement.GetElementsByTagName("Name");
                                        if (elements != null && elements.Count > 0)
                                        {
                                            XmlElement nameElement = (XmlElement)elements[0];
                                            name = nameElement.InnerText;
                                        }
                                        ArgumentCheck.CheckNotNull(name);
                                        if (name.Equals(p))
                                        {
                                            elements = propertyElement.GetElementsByTagName("Value");
                                            if (elements != null && elements.Count > 0)
                                            {
                                                XmlElement valueElement = (XmlElement)elements[0];
                                                string text = valueElement.InnerText;
                                                string type = valueElement.GetAttribute("p4:type");
                                                type = type.Substring(type.IndexOf(":") + 1);
                                                if(name == null)
                                                {
                                                    throw new ArgumentNullException(nameof(name));
                                                }
                                                if (name.Equals("TextField") || name.Equals("SspId") || name.Equals("GroupId") || name.Equals("TermSetId") || name.Equals("AnchorId"))
                                                {
                                                    type = "guid";
                                                    string tValue = valueElement.InnerText;
                                                    if (tValue.Contains('|'))
                                                    {
                                                        string[] temp = tValue.ToString().Split('|');
                                                        if (temp.Length == 2)
                                                        {
                                                            valueElement.InnerText = temp[0];
                                                            continue;
                                                        }
                                                    }
                                                }
                                                switch (type)
                                                {
                                                    case "datetime":
                                                        value = Convert.ToDateTime(valueElement.InnerText);
                                                        break;
                                                    case "boolean":
                                                        value = Convert.ToBoolean(valueElement.InnerText);
                                                        break;
                                                    case "guid":
                                                        value = new Guid(valueElement.InnerText);
                                                        break;
                                                    case "int32":
                                                    case "int":
                                                        value = Convert.ToInt32(valueElement.InnerText);
                                                        break;
                                                    case "double":
                                                        value = Convert.ToDouble(valueElement.InnerText);
                                                        break;
                                                    default:
                                                        value = valueElement.InnerText;
                                                        break;
                                                }
                                            }
                                            return value;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Debug(ex.ToString());
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
            }
            return null;
        }

        public void SetCustomProperty(string propertyName, object propertyValue)
        {
            throw new NotImplementedException();
        }

        public bool EnforceUniqueValues
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnforceUniqueValues");
            }
            set
            {
                base.DataCache.AddChangedProperty("EnforceUniqueValues", value);
            }
        }

        public virtual void SetShowInDisplayForm(bool value)
        {
            var listId= mParentList== null ? Guid.Empty : mParentList.ID;
            mRequest.SetShowInDisplayForm(mWeb.ServerRelativeUrl, 
                fieldSourceScope,listId,mContentTypeProp,ID,value);
        }

        public virtual void SetShowInEditForm(bool value)
        {
            var listId = mParentList == null ? Guid.Empty : mParentList.ID;
            mRequest.SetShowInEditForm(mWeb.ServerRelativeUrl,
                fieldSourceScope, listId, mContentTypeProp, ID, value);
        }

        public virtual void SetShowInNewForm(bool value)
        {
            var listId = mParentList == null ? Guid.Empty : mParentList.ID;
            mRequest.SetShowInNewForm(mWeb.ServerRelativeUrl,
                fieldSourceScope, listId, mContentTypeProp, ID, value);
        }

        public string MD5
        {
            get
            {
                return mMD5;
            }
            set
            {
                mMD5 = value;
            }
        }

        private void InitSchemaXml(ref IDictionary<string, object> fieldProperty)
        {
            if (fieldProperty.ContainsKey("SchemaXml"))
            {
                string fieldId = Guid.Empty.ToString();
                string listId = mParentList == null ? string.Empty : mParentList.ID.ToString();
                if (fieldProperty.ContainsKey("Id"))
                {
                    fieldId = fieldProperty["Id"].ToString();
                }
                AveClientCacheHandler.WriteSchemaXml(fieldProperty["SchemaXml"].ToString(), this.mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, fieldId, SchemaType.Field);
                fieldProperty.Remove("SchemaXml");
            }
        }

        public bool CanBeDisplayedInEditForm
        {
            get
            {
                if (!this.Reorderable)
                {
                    return false;
                }
                AveFieldType type = this.Type;
                return ((((type != AveFieldType.Computed) && (type != AveFieldType.File)) && ((type != AveFieldType.Integer) && (type != AveFieldType.Recurrence))) && (((type != AveFieldType.CrossProjectLink) && (type != AveFieldType.AllDayEvent)) && ((type != AveFieldType.Lookup) || !(this as IAveFieldLookup).IsDependentLookup)));
            }
        }


        public virtual object DefaultValueTyped
        {
            get { return null; }
        }

        public string EntityPropertyName
        {
            get { return base.DataCache.GetProperty<string>("EntityPropertyName"); }
        }

        public virtual string JSLink
        {
            get { return base.DataCache.GetProperty<string>("JSLink"); }
            set { base.DataCache.AddChangedProperty("JSLink", value); }
        }


        public string Scope
        {
            get { return base.DataCache.GetProperty<string>("Scope"); }
            set { base.DataCache.AddChangedProperty("Scope", value); }
        }


        public IAveUserResource DescriptionResource
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AveFieldDescriptionResource"))
                {
                    var descriptionResource = new AveFieldUserResource(mRequest, mWeb.ServerRelativeUrl, this.mParentList, mFieldSource, 
                        AveUserResourceConstants.DESCRIPTION_RESOUCE, this.mContentTypeProp, base.DataCache);
                    base.DataCache.AddProperty("AveFieldDescriptionResource",descriptionResource);
                    return descriptionResource;
                }
                return base.DataCache.GetProperty<AveFieldUserResource>("AveFieldDescriptionResource");
            }
        }

        public IAveUserResource TitleResource
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AveFieldTitleResource"))
                {
                    var titleResource = new AveFieldUserResource(mRequest, mWeb.ServerRelativeUrl, this.mParentList, mFieldSource,
                        AveUserResourceConstants.TITLE_RESOUCE, this.mContentTypeProp, base.DataCache);
                    base.DataCache.AddProperty("AveFieldTitleResource",titleResource);
                    return titleResource;
                }
                return base.DataCache.GetProperty<AveFieldUserResource>("AveFieldTitleResource");
            }
        }
    }
}
