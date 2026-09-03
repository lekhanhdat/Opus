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
using System.Diagnostics.CodeAnalysis;
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
        private Dictionary<string, object> mContentTypeProp;
        private string mMD5;
        private object threadLocker = new object();
        private AveUserResource mTitleResource;
        private AveUserResource mDescriptionResource;
        private object privateLockTitleResource = new object();
        private object privateLockDescriptionResource = new object();

        public AveField(IAveRequest request, AveList list, AveWeb web, string fieldSource, AveFieldCollection fieldCollection, Dictionary<string, object> contentTypeProp, Dictionary<string, object> prop)
        {
            mRequest = request;
            mParentList = list;
            mWeb = web;
            mFieldCollection = fieldCollection;
            mFieldSource = fieldSource;
            mContentTypeProp = contentTypeProp;
            base.DataCache.AddChangedProperty("fieldSource", mFieldSource);
            base.DataCache.AddPropertyies(prop);
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

        /// <summary>
        /// Different from local logic, the schemalXml won't change until update.
        /// </summary>
        public String SchemaXml
        {
            get
            {
                //新添加的Field的SchemaXml放到内存里。 Reload Field Collection时再写到本地。提升效率。
                if (base.DataCache.IsPropertyAvailable("SchemaXml"))
                {
                    return base.DataCache.GetProperty<string>("SchemaXml");
                }
                string listId = mParentList == null ? string.Empty : mParentList.ID.ToString();
                return AveClientCacheHandler.GetSchemaXml(mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, this.ID.ToString(), SchemaType.Field);
            }
            set
            { base.DataCache.AddChangedProperty("SchemaXml", value); }
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
                    if (this is IAveFieldComputed)
                    {
                        return false;
                    }
                    if (this is IAveFieldAttachments)
                    {
                        return false;
                    }
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
                if ((base.DataCache.GetProperty<string>("XPath") == null) || this.Hidden)
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
        public AveWeb ParentWeb
        {
            get
            {
                return this.mWeb;
            }
        }
        public bool FromBaseType
        {
            get { return base.DataCache.GetProperty<bool>("FromBaseType"); }
        }
        public IAveFieldCollection Fields { get { return mFieldCollection; } }

        public virtual void Delete()
        {
            string listTitle = this.mParentList == null ? null : this.mParentList.Title;
            Guid listId = this.mParentList == null ? Guid.Empty : this.mParentList.ID;
            this.mRequest.DeleteField(this.mWeb.ServerRelativeUrl, listTitle, listId, this.InternalName, this.mFieldSource, this.mContentTypeProp);
            this.mFieldCollection.ListData.Remove(this);
        }
        public string GetProperty(string propName)
        {
            string result = string.Empty;
            try
            {
                if (DataCache.PropertiesCache.ContainsKey(propName))
                {
                    result = base.DataCache.PropertiesCache[propName].ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get property of field failed.Error Message:{0},Property Name:{1}", ex.ToString(), propName);
            }
            return result;
        }

        public virtual void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 1)  //fieldsource is always in changedproperites
            {
                base.DataCache.AddChangedProperty("ObjectPath", base.DataCache.GetProperty<object>("ObjectPath"));
                base.DataCache.AddChangedProperty("FieldType", base.DataCache.GetProperty<Type>("FieldType"));
                //base.DataCache.AddChangedProperty("ClientContext", base.DataCache.GetProperty<object>("ClientContext"));
                string listTitle = this.mParentList == null ? null : this.mParentList.Title;
                Guid listGuid = this.mParentList == null ? Guid.Empty : this.mParentList.ID;
                #region --Set NoCrawl Property--
                //由于NoCrawl这个属性在Client模式下没有相应的API，所以采用更新SchemaXml的方式去还原这个属性。
                //string isNoCrawl = string.Empty;
                //if (base.DataCache.ChangedProperties.ContainsKey("NoCrawl"))
                //{
                //    string xml = this.SchemaXml;
                //    XmlDocument xmlDoc = new XmlDocument();
                //    xmlDoc.LoadXml(xml);
                //    XmlElement ele = xmlDoc.DocumentElement;
                //    isNoCrawl = base.DataCache.ChangedProperties["NoCrawl"].ToString().ToUpperInvariant();
                //    ele.SetAttribute("NoCrawl", isNoCrawl);
                //    base.DataCache.ChangedProperties.Remove("NoCrawl");
                //    foreach (KeyValuePair<string, object> pair in base.DataCache.ChangedProperties)
                //    {
                //        if (ele.HasAttribute(pair.Key))
                //        {
                //            ele.SetAttribute(pair.Key, pair.Value.ToString());
                //        }
                //    }
                //    base.DataCache.AddChangedProperty("SchemaXml", xmlDoc.InnerXml);
                //}
                if (base.DataCache.ChangedProperties.ContainsKey("NoCrawl"))
                {
                    this.UpdateSchemaXmlProperty("NoCrawl");
                }
                if (base.DataCache.ChangedProperties.ContainsKey("UnlimitedLengthInDocumentLibrary"))
                {
                    this.UpdateSchemaXmlProperty("UnlimitedLengthInDocumentLibrary");
                }
                #endregion
                Dictionary<string, object> fieldProperties = this.mRequest.UpdateField(this.mWeb.ServerRelativeUrl, listTitle, listGuid, this.InternalName, this.mFieldSource, this.mContentTypeProp, base.DataCache.ChangedProperties);
                if (fieldProperties.ContainsKey("SchemaXml"))
                {
                    string listId = this.mParentList != null ? this.mParentList.ID.ToString() : string.Empty;
                    AveClientCacheHandler.WriteSchemaXml(fieldProperties["SchemaXml"].ToString(), this.mWeb.CacheHandlerId, this.mWeb.ID.ToString(), listId, this.ID.ToString(), SchemaType.Field);
                    fieldProperties.Remove("SchemaXml");
                }
                if (base.DataCache.ChangedProperties.ContainsKey("FieldTypeKind"))
                {
                    (this.Fields as AveFieldCollection).ListData.Remove(this);
                    AveField newField = (this.Fields as AveFieldCollection).CreateFieldByType(fieldProperties);
                    (this.Fields as AveFieldCollection).ListData.Add(newField);
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
        }

        public void UpdateReadOnlyField()
        {
            if (base.DataCache.ChangedProperties.Count > 1)  //fieldsource is always in changedproperites
            {
                base.DataCache.AddChangedProperty("ObjectPath", base.DataCache.GetProperty<object>("ObjectPath"));
                base.DataCache.AddChangedProperty("FieldType", base.DataCache.GetProperty<Type>("FieldType"));
                //base.DataCache.AddChangedProperty("ClientContext", base.DataCache.GetProperty<object>("ClientContext"));
                base.DataCache.ChangedProperties.Remove("ReadOnly");
                string listTitle = this.mParentList == null ? null : this.mParentList.Title;
                Guid listId = this.mParentList == null ? Guid.Empty : this.mParentList.ID;
                Dictionary<string, object> fieldProperties = this.mRequest.UpdateReadOnlyField(this.mWeb.ServerRelativeUrl, listTitle, listId, this.InternalName, this.mFieldSource, this.mContentTypeProp, base.DataCache.ChangedProperties);
                base.DataCache.AddPropertyies(fieldProperties);
            }
        }

        #region private method
        private AveFieldType ConvertType(string type)
        {
            foreach (FieldInfo field in typeof(AveFieldType).GetFields())
            {
                if (field.Name.Equals(type))
                {
                    return (AveFieldType)field.GetValue(AveFieldType.Invalid);
                }
            }
            return AveFieldType.Invalid;
        }
        private AveFieldType ConvertTypeII(string type)
        {
            return (AveFieldType)Enum.Parse(typeof(AveFieldType), type, true);
        }

        private void UpdateSchemaXmlProperty(String fieldPropertyName)
        {
            string xml = this.SchemaXml;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlElement ele = xmlDoc.DocumentElement;
            var tempValue = base.DataCache.ChangedProperties[fieldPropertyName].ToString().ToUpperInvariant();
            ele.SetAttribute(fieldPropertyName, tempValue);
            base.DataCache.ChangedProperties.Remove(fieldPropertyName);
            foreach (KeyValuePair<string, object> pair in base.DataCache.ChangedProperties)
            {
                if (ele.HasAttribute(pair.Key))
                {
                    ele.SetAttribute(pair.Key, pair.Value.ToString());
                }
            }
            base.DataCache.ChangedProperties["SchemaXml"] = xmlDoc.InnerXml;
        }

        #endregion


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
            lock (threadLocker)
            {
                if (base.DataCache.IsPropertyNotLoaded("SchemaXmlDocument"))
                {
                    XmlDocument xmlD = new XmlDocument();
                    xmlD.LoadXml(this.SchemaXml);
                    base.DataCache.PropertiesCache["SchemaXmlDocument"] = xmlD;
                }
            }
            XmlDocument sxd = base.DataCache.PropertiesCache["SchemaXmlDocument"] as XmlDocument;
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
            var schemaXml = this.SchemaXml;
            if (!string.IsNullOrEmpty(schemaXml))
            {
                doc.InnerXml = schemaXml;
                foreach (XmlElement customElement in doc.FirstChild.ChildElements())
                {
                    if (customElement.Name.Equals("Customization"))
                    {
                        foreach (XmlElement element in customElement.ChildElements())
                        {
                            if (element.Name.Equals("ArrayOfProperty"))
                            {
                                foreach (XmlElement propertyElement in element.ChildElements())
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
                                            if (name.Equals(p))
                                            {
                                                elements = propertyElement.GetElementsByTagName("Value");
                                                if (elements != null && elements.Count > 0)
                                                {
                                                    XmlElement valueElement = (XmlElement)elements[0];
                                                    string text = valueElement.InnerText;
                                                    string type = valueElement.GetAttribute("p4:type");
                                                    type = type.Substring(type.IndexOf(":", StringComparison.OrdinalIgnoreCase) + 1);

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
                                        Logger.Debug("An error occurred while getting CustomProperty {0}, Exception: {1}", p, ex);
                                    }
                                }
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            return null;
        }

        public void SetCustomProperty(string propertyName, object propertyValue)
        {
            //do nothing for now
            //throw new NotImplementedException();
        }

        public string Scope
        {
            //在server上实现，Client没有实现,返回默认值
            get { return default(string); }
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

        //private void InitSchemaXml(ref Dictionary<string, object> fieldProperty)
        //{
        //    if (fieldProperty.ContainsKey("SchemaXml"))
        //    {
        //        string fieldId = Guid.Empty.ToString();
        //        string listId = mParentList == null ? string.Empty : mParentList.ID.ToString();
        //        if (fieldProperty.ContainsKey("Id"))
        //        {
        //            fieldId = fieldProperty["Id"].ToString();
        //        }
        //        AveClientCacheHandler.WriteSchemaXml(fieldProperty["SchemaXml"].ToString(), this.mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, fieldId, SchemaType.Field);
        //        fieldProperty.Remove("SchemaXml");
        //    }
        //}

        public bool CanBeDisplayedInEditForm
        {
            get { throw new NotImplementedException(); }
        }


        public virtual object DefaultValueTyped
        {
            get { return DefaultValue; }
        }

        public bool CalloutMenu
        {
            get
            {
                return base.DataCache.GetProperty<bool>("CalloutMenu");
            }
            set
            {
                base.DataCache.AddChangedProperty("CalloutMenu", value);
            }
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

        public string BaseTypeString
        {
            get { return base.DataCache.GetProperty<string>("BaseTypeString"); }
        }

        #region User Resource:need to confirm if support
        public IAveUserResource DescriptionResource
        {
            get
            {
                if (!mWeb.Site.IsOnlineSite)
                {
                    return null;
                }
                lock (privateLockDescriptionResource)
                {
                    if (mDescriptionResource == null)
                    {
                        mDescriptionResource = new AveFieldUserResource(this, AveUserResourceConstants.DESCRIPTION_RESOUCE, mFieldSource, mContentTypeProp, this.DataCache);
                    }
                    return mDescriptionResource;
                }
            }
        }

        public IAveUserResource TitleResource
        {
            get
            {
                if (!mWeb.Site.IsOnlineSite)
                {
                    return null;
                }
                lock (privateLockTitleResource)
                {
                    if(mTitleResource == null)
                    {
                        mTitleResource = new AveFieldUserResource(this, AveUserResourceConstants.TITLE_RESOUCE, mFieldSource, mContentTypeProp, this.DataCache);
                    }
                    return mTitleResource;
                }
            }
        }
        #endregion


        public bool CanBeDeleted
        {
            get { return base.DataCache.GetProperty<bool>("CanBeDeleted"); }
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ValidationEcmaScript is a part of keys")]
        public string ValidationEcmaScript
        {
            get { return base.DataCache.GetProperty<string>("ValidationEcmaScript"); }
        }


        public bool Sortable
        {
            get { return base.DataCache.GetProperty<bool>("Sortable"); }
        }
    }
}
