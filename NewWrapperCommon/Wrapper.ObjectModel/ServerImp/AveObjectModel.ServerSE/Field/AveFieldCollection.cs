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
using System.Globalization;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveFieldCollection : AveAbstractCommonCollection<IAveField>, IAveFieldCollection, IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveFieldCollection));
        private SPFieldCollection mFields;
        private AveWeb mWeb;
        private AveSite mSite;
        private AveList mList;
        private bool mIsDirty;
        private bool mIsDirtySetted;
        private Nullable<bool> mContainsLookupField;
        private Dictionary<string, AveField> fieldCache = new Dictionary<string, AveField>();

        public AveFieldCollection(AveWeb web, SPFieldCollection fields)
            : base(fields)
        {
            mWeb = web;
            mSite = web.Site as AveSite;
            mFields = fields;
        }

        public AveFieldCollection(IAveWeb web, string strXml)
            : this(web as AveWeb, new SPFieldCollection((web as AveWeb).Web, strXml))
        { }

        internal SPFieldCollection FieldCollection
        {
            get
            {
                return mFields;
            }
        }

        public IAveWeb Web
        {
            get
            {
                return mWeb;
            }
        }

        internal AveField CreateFieldByType(SPField field)
        {
            if (field != null)
            {
                string key = string.Format("{0}:{1}", field.StaticName, field.Id);
                if (!fieldCache.ContainsKey(key))
                {
                    lock (fieldCache)
                    {
                        if (!fieldCache.ContainsKey(key))
                        {
                            fieldCache[key] = AveServerAssemblyInit.CreateElement(typeof(IAveField), new object[] { this, field }) as AveField;
                        }
                    }
                }
                return fieldCache[key];
            }
            return null;
        }

        #region IAveFieldCollection Members

        public IAveField Add(IAveField aveField)
        {
            string internalName = mFields.Add((aveField as AveField).Field);
            SPField field = mFields.GetFieldByInternalName(internalName);
            return CreateFieldByType(field);
        }


        public IAveField AddLookup(string displayName, Guid lookupListId, Guid lookupWebId, bool bRequired)
        {
            string internalName = mFields.AddLookup(displayName, lookupListId, lookupWebId, bRequired);
            SPField field = mFields.GetFieldByInternalName(internalName);
            return CreateFieldByType(field);
        }

        public IAveField CreateNewField(string typeName, string displayName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.CreateNewField"))
            {

                SPField field = mFields.CreateNewField(typeName, displayName);
                return CreateFieldByType(field);

            }

        }

        public IAveField AddFieldAsXml(string fieldXml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.AddFieldAsXml"))
            {

                string internalName = mFields.AddFieldAsXml(fieldXml);
                SPField field = mFields.GetFieldByInternalName(internalName);
                return CreateFieldByType(field);

            }

        }

        public IAveField AddFieldAsXml(string fieldXml, bool addToDefaultView, AveAddFieldOptions op)
        {
            string internalName = mFields.AddFieldAsXml(fieldXml, addToDefaultView, (SPAddFieldOptions)op);
            SPField field = mFields.GetFieldByInternalName(internalName);
            return CreateFieldByType(field);
        }

        public IAveField GetByInfo(string name, string type)
        {
            foreach (SPField field in mFields)
            {
                if (string.Equals(field.Title, name) && string.Equals(field.Type, type))
                {
                    return CreateFieldByType(field);
                }
            }
            return null;
        }

        public string SchemaXml
        {
            get { return mFields.SchemaXml; }
        }

        public bool ContainsField(string fieldName)
        {
            return mFields.ContainsField(fieldName);
        }

        public string Add(string strDisplayName, AveFieldType fieldType, bool bRequired)
        {
            return mFields.Add(strDisplayName, (SPFieldType)fieldType, bRequired);
        }

        public IAveField GetField(string strName)
        {
            return CreateFieldByType(mFields.GetField(strName));
        }

        public IAveField this[string name]
        {
            get
            {
                return CreateFieldByType(mFields[name]);
            }
        }

        public bool Contains(Guid fieldId)
        {
            //SharePoint API 某些情况（如external list）会抛异常
            try
            {
                return mFields.Contains(fieldId);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetFieldByIdError, e.ToString());
            }
            return false;
        }

        public void Delete(string strName)
        {
            mFields.Delete(strName);
        }

        public IAveField GetById(Guid id)
        {
            return CreateFieldByType(mFields[id]);
        }

        public IAveField GetFieldByInternalName(string internalName)
        {
            return CreateFieldByType(mFields.GetFieldByInternalName(internalName));
        }

        public IAveField TryGetFieldByStaticName(string staticName)
        {
            return CreateFieldByType(mFields.TryGetFieldByStaticName(staticName));
        }

        public IAveField this[Guid id]
        {
            get
            {
                return GetById(id);
            }
        }

        public bool IsDirty
        {
            get
            {
                if (!mIsDirtySetted)
                {
                    mIsDirty = (bool)AveAssemblyUtility.GetPropertyValue(mFields, "IsDirty");
                }
                else
                {
                    mIsDirtySetted = false;
                }
                return mIsDirty;
            }
            set
            {
                mIsDirty = value;
                mIsDirtySetted = true;
            }
        }

        public string GetWeb(IAveSite site, Guid webId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.GetWeb"))
            {

                string webUrl = string.Empty;
                try
                {
                    using (IAveWeb web = site.OpenWeb(webId))
                    {
                        webUrl = web.ServerRelativeUrl;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetWebByIdError, e.ToString());
                    return string.Empty;
                }
                return webUrl;

            }


        }

        public string GetList(IAveSite site, Guid webId, Guid listId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.GetList"))
            {

                string listTitle = string.Empty;
                try
                {
                    using (IAveWeb web = site.OpenWeb(webId))
                    {
                        IAveList list = web.Lists[listId];
                        if (list != null)
                        {
                            listTitle = list.Title;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetListByIdError, e.ToString());
                    return string.Empty;
                }
                return listTitle;

            }

        }

        public List<string> GetFieldsFromSchema(string fieldSchema)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.GetFieldsFromSchema"))
            {

                List<string> fields = new List<string>();
                XmlDocument xDoc = new XmlDocument();

                xDoc.InnerXml = fieldSchema;
                foreach (XmlNode node in xDoc.FirstChild.ChildNodes)
                {
                    fields.Add(node.OuterXml);
                }
                return fields;

            }

        }

        public List<string> GetFields()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.GetFields"))
            {

                List<string> fields = new List<string>();
                foreach (SPField field in mFields)
                {
                    fields.Add(field.SchemaXml);
                }
                return fields;

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = " ")]
        public AveFieldCollectionInfo GetFieldInfoObj()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.GetFieldInfoObj"))
            {

                AveFieldCollectionInfo fieldInfos = new AveFieldCollectionInfo();
                try
                {
                    Guid siteId = Guid.Empty;
                    string scope = string.Empty;
                    //List<string> fields = GetFields(mWeb.Site.Id, mWeb.ServerRelativeUrl);
                    //TODO...Maybe we should select some fields but not all the fields here...
                    if (this.mWeb != null)
                    {
                        siteId = this.mWeb.Site.ID;
                        scope = this.mWeb.ServerRelativeUrl;
                    }
                    List<string> fields = GetFields(siteId, scope);
                    StringBuilder schema = new StringBuilder();
                    XmlDocument xDoc = new XmlDocument();
                    XmlElement fieldElement = null;
                    foreach (string field in fields)
                    {
                        xDoc.InnerXml = field;
                        fieldElement = xDoc.FirstChild as XmlElement;
                        ReplaceFieldAttribute(fieldElement);
                        AveFieldInfo fieldInfo = new AveFieldInfo();
                        fieldInfo.Name = fieldElement.GetAttribute("Name");
                        fieldInfo.SchemaXml = xDoc.InnerXml;
                        fieldInfos.Fields.Add(fieldInfo);
                        schema.Append(fieldInfo.SchemaXml);
                    }

                    string fieldSchema = "<Fields>" + schema.ToString() + "</Fields>";
                    List<AveTaxFieldInfo> taxFieldInfos = null;
                    fieldInfos.AveSchemaXml = TransListIdToTitle(mWeb, List, fieldSchema, ref taxFieldInfos);
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.FieldInfoGetError, e);
                }
                return fieldInfos;

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = " ")]
        public AveFieldCollectionInfo GetFieldInfoObj(AveBackupOption backupColumnOption)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.GetFieldInfoObj"))
            {

                AveFieldCollectionInfo fieldInfos = new AveFieldCollectionInfo();
                try
                {
                    List<AveTaxFieldInfo> taxFieldInfos = new List<AveTaxFieldInfo>();
                    string schemaXml = string.Empty;
                    if (List == null)
                    {
                        Guid siteId = Guid.Empty;
                        string scope = string.Empty;
                        //List<string> fields = GetFields(mWeb.Site.Id, mWeb.ServerRelativeUrl);
                        //TODO...Maybe we should select some fields but not all the fields here...
                        if (this.mWeb != null)
                        {
                            siteId = this.mWeb.Site.ID;
                            scope = this.mWeb.ServerRelativeUrl;
                        }
                        List<string> fields = GetFields(siteId, scope);
                        StringBuilder schema = new StringBuilder();
                        XmlDocument xDoc = new XmlDocument();
                        XmlElement fieldElement = null;
                        foreach (string field in fields)
                        {
                            xDoc.InnerXml = field;
                            fieldElement = xDoc.FirstChild as XmlElement;
                            ReplaceFieldAttribute(fieldElement);
                            AveFieldInfo fieldInfo = new AveFieldInfo();
                            fieldInfo.Name = fieldElement.GetAttribute("Name");
                            fieldInfo.SchemaXml = xDoc.InnerXml;
                            fieldInfos.Fields.Add(fieldInfo);
                            schema.Append(fieldInfo.SchemaXml);
                        }
                        schemaXml = "<Fields>" + schema.ToString() + "</Fields>";
                    }
                    else
                    {
                        schemaXml = GetFields(mWeb.ID, List.ID,backupColumnOption.BackupFieldsByAPI);
                    }
                    fieldInfos.AveSchemaXml = TransListIdToTitle(mWeb, List, schemaXml, ref taxFieldInfos);
                    if (backupColumnOption.BackupRelatedTermSets || backupColumnOption.BackupRelatedTermsOnly)
                    {
                        fieldInfos.RelatedMetadataInfo = GetRelatedMetadataInfo(mWeb.Site, taxFieldInfos, backupColumnOption);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.FieldInfoGetError, e);
                }
                return fieldInfos;

            }

        }

        private List<AveTermStoreInfo> GetRelatedMetadataInfo(IAveSite site, List<AveTaxFieldInfo> taxFieldInfos, AveBackupOption backupOption)
        {
            List<AveTermStoreInfo> termStoreInfos = new List<AveTermStoreInfo>();
            if (taxFieldInfos == null || taxFieldInfos.Count == 0)
            {
                return termStoreInfos;
            }
            AveMetaDataServiceSerializer serializer = (site as AveSite).MetaDataServiceSerializer as AveMetaDataServiceSerializer;
            termStoreInfos = serializer.GetRelatedMetadataInfo(site, taxFieldInfos, backupOption);
            return termStoreInfos;
        }

        internal void ReplaceFieldAttribute(XmlElement fieldElement)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.ReplaceFieldAttribute"))
            {

                string attrValue = null;
                attrValue = fieldElement.GetAttribute("Name");
                if (attrValue.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                {
                    attrValue = SPUtility.GetLocalizedString(attrValue, "core", (uint)mWeb.UICulture.LCID);
                    fieldElement.SetAttribute("Name", attrValue);
                }
                attrValue = fieldElement.GetAttribute("DisplayName");
                if (attrValue != null && attrValue.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                {
                    attrValue = SPUtility.GetLocalizedString(attrValue, "core", (uint)mWeb.UICulture.LCID);
                    fieldElement.SetAttribute("DisplayName", attrValue);
                }
                attrValue = fieldElement.GetAttribute("AuthoringInfo");
                if (attrValue != null && attrValue.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                {
                    attrValue = SPUtility.GetLocalizedString(attrValue, "core", (uint)mWeb.UICulture.LCID);
                    fieldElement.SetAttribute("AuthoringInfo", attrValue);
                }

                if (fieldElement.HasAttribute("ShowField"))
                {
                    fieldElement.SetAttribute("ShowField", SPUtility.GetLocalizedString(fieldElement.GetAttribute("ShowField"), "core", (uint)mWeb.UICulture.LCID));
                }
                attrValue = fieldElement.GetAttribute("Description");
                if (attrValue != null && attrValue.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                {
                    attrValue = SPUtility.GetLocalizedString(attrValue, "core", (uint)mWeb.UICulture.LCID);
                    fieldElement.SetAttribute("Description", attrValue);
                }
                attrValue = fieldElement.GetAttribute("Group");
                if (attrValue != null && attrValue.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                {
                    attrValue = SPUtility.GetLocalizedString(attrValue, "core", (uint)mWeb.UICulture.LCID);
                    fieldElement.SetAttribute("Group", attrValue);
                }
                XmlNode node = fieldElement.SelectSingleNode("CHOICES");
                if (node != null)
                {
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        string[] tempStr = childNode.InnerText.Split(';');
                        if (tempStr.Length > 1)
                        {
                            if (tempStr[1].StartsWith("#$Resources", StringComparison.OrdinalIgnoreCase))
                            {
                                attrValue = childNode.InnerText;
                                attrValue = SPUtility.GetLocalizedString(attrValue, "core", (uint)mWeb.UICulture.LCID);
                                childNode.InnerText = attrValue;
                            }
                        }
                    }
                }
                LocalizeXmlChildrenNode(node);
                node = fieldElement.SelectSingleNode("MAPPINGS");
                LocalizeXmlChildrenNode(node);
                node = fieldElement.SelectSingleNode("Default");
                LocalizeXmlNode(node);

            }

        }

        private void LocalizeXmlChildrenNode(XmlNode node)
        {
            if (node != null)
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    LocalizeXmlNode(childNode);
                }
            }
        }

        private void LocalizeXmlNode(XmlNode childNode)
        {
            if (childNode != null && !string.IsNullOrEmpty(childNode.InnerText))
            {
                string[] tokens = childNode.InnerText.Split(';');
                foreach (string token in tokens)
                {
                    LocalizeXmlNode(childNode, token);
                }
            }
        }

        private void LocalizeXmlNode(XmlNode childNode, string token)
        {
            if (ContainsResourceKey(token))
            {
                String displayText = childNode.InnerText;
                displayText = SPUtility.GetLocalizedString(displayText, "core", (uint)mWeb.UICulture.LCID);
                childNode.InnerText = displayText;
            }
        }

        private bool ContainsResourceKey(string key)
        {
            return !string.IsNullOrEmpty(key) && key.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase);
        }

        public string TransListIdToTitle(IAveWeb aveWeb, IAveList aveList, string xml)
        {
            List<AveTaxFieldInfo> temp = null;
            return TransListIdToTitle(aveWeb, aveList, xml, ref temp);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "TransListIdToTitle is function name")]
        private string TransListIdToTitle(IAveWeb aveWeb, IAveList aveList, string xml, ref List<AveTaxFieldInfo> taxFieldInfos)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.TransListIdToTitle"))
            {

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                foreach (XmlElement element in doc.DocumentElement.GetElementsByTagName("Field"))
                {
                    try
                    {
                        XmlElement fieldInfoElement = element.GetElementsByTagName("AveFieldInfo").Count > 0 ? (XmlElement)element.GetElementsByTagName("AveFieldInfo")[0] : doc.CreateElement("AveFieldInfo");
                        string fieldTypeString = element.GetAttribute("Type");
                        AveFieldType fieldType = AveSPUtility.GetFieldType(fieldTypeString);
                        string baseType = fieldTypeString;
                        //备份User Resource
                        IAveField field = null;
                        if (aveWeb.IsMultilingual && aveWeb.SupportedUICultures.Count() > 0)
                        {
                            field = GetFieldByInternalName(element.GetAttribute("Name"));
                            XmlElement resourceElement = doc.CreateElement(SerializationConstants.ColumnConstants.RESOURCE_NODE);
                            if (GetTitleAndDescriptionResource(aveWeb, doc, resourceElement, field))
                            {
                                element.AppendChild(resourceElement);
                            }
                        }
                        if (field == null && fieldType == AveFieldType.Invalid)
                        {
                            field = GetFieldByInternalName(element.GetAttribute("Name"));
                        }
                        if (fieldType == AveFieldType.Invalid)
                        {
                            baseType = field.BaseTypeString;
                        }
                        element.SetAttribute("FieldBaseType", baseType);
                        var id = element.HasAttribute("ID") ? element.GetAttribute("ID").ToString() : Guid.Empty.ToString();
                        element.SetAttribute("FromParent", "FALSE");
                        if (aveList != null)
                        {
                            if (aveWeb.AvailableFields.Contains(new Guid(id)))
                            {
                                element.SetAttribute("FromParent", "TRUE");
                            }
                        }
                        if (baseType.Equals("Lookup", StringComparison.OrdinalIgnoreCase) || baseType.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
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
                            string sourceId = element.GetAttribute("SourceID");

                            string webUrl = String.Empty;
                            string listTitle = String.Empty;
                            string sourceType = "-1";

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
                                    listId = string.Empty;
                                }
                                else if (listId == "UserInfo" && aveWeb != null && aveWeb != null)
                                {
                                    listTitle = aveWeb.SiteUserInfoList.Title;
                                    listId = aveWeb.SiteUserInfoList.ID.ToString("B");
                                }
                                else
                                {
                                    listTitle = GetList(aveWeb.Site, new Guid(webId), new Guid(listId));
                                }

                                if (element.HasAttribute("List"))
                                {
                                    fieldInfoElement.SetAttribute("AveLookupListTitle", listTitle);
                                    fieldInfoElement.SetAttribute("AveLookupListID", listId);
                                    fieldInfoElement.SetAttribute("AveLookupWebTitle", webUrl);
                                }
                                //????
                                try
                                {
                                    if (!String.IsNullOrEmpty(sourceId))
                                    {
                                        //类似cache profile这些list 他们的sourceId=“http://schemas.microsoft.com/sharepoint/v3”,不能直接将sourceId转为Guid与listId和webId比较。
                                        if (aveList != null && aveList != null && sourceId.Equals(aveList.ID.ToString()))
                                        {
                                            sourceType = "2";
                                        }
                                        else if (sourceId.Equals(aveWeb.ID.ToString()))
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
                                catch (Exception e)
                                {
                                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.SetXmlAttError, e.ToString());
                                }
                                element.AppendChild(fieldInfoElement);
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "TransListIdToTitle, Name {0}, Error:{1} ", element.GetAttribute("Name"), e);
                            }
                        }
                        else if (baseType.Equals("User", StringComparison.OrdinalIgnoreCase) || baseType.Equals("UserMulti", StringComparison.OrdinalIgnoreCase))
                        {
                            fieldInfoElement.SetAttribute("IsRelationship", GetRelationship(aveWeb.Site.ID.ToString(), aveList == null ? string.Empty : aveList.ID.ToString(), element).ToString());
                        }

                        //TaxonomyField 是一个固定指向Taxonomyhiddenlist的lookupfield。
                        if (baseType.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) || baseType.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                        {
                            AveTaxFieldInfo taxFieldInfo = new AveTaxFieldInfo();
                            SetTaxonomyField(aveWeb.Site, element, taxFieldInfo);
                            if ((taxFieldInfo.TermSetId != Guid.Empty || taxFieldInfo.IsKeywordsColumn) && taxFieldInfos != null)
                            {
                                taxFieldInfos.Add(taxFieldInfo);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.WARN, "Failed to translate list ID to the title. Title: {0}. Field Schema: {1}, Error: {2}", aveList != null ? aveList.Title : aveWeb.Title, element.OuterXml, ex.ToString());
                    }
                }
                return doc.OuterXml;

            }

        }

        private bool GetTitleAndDescriptionResource(IAveWeb aveWeb, XmlDocument doc, XmlElement parent, IAveField field)
        {
            bool change = false;
            change |= AppendToFieldXml(doc, parent, field.TitleResource.GetUserResourceInfoXml(aveWeb), SerializationConstants.ColumnConstants.TITLE_RESOUCE_NODE);
            change |= AppendToFieldXml(doc, parent, field.DescriptionResource.GetUserResourceInfoXml(aveWeb), SerializationConstants.ColumnConstants.DESCRIPTION_RESOUCE_NODE);
            return change;
        }

        private bool AppendToFieldXml(XmlDocument doc, XmlElement parent, string resourceXml, string nodeName)
        {
            if (!string.IsNullOrEmpty(resourceXml))
            {
                var resourceNode = doc.CreateElement(nodeName);
                resourceNode.InnerXml = resourceXml;
                parent.AppendChild(resourceNode);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add names of taxonomy fields in the xml for another one search.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="mElement"></param>
        private void SetTaxonomyField(IAveSite site, XmlElement mElement, AveTaxFieldInfo taxFieldInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.SetTaxonomyField"))
            {

                Guid SspId = Guid.Empty;
                Guid GroupId = Guid.Empty;
                Guid TermSetId = Guid.Empty;
                Guid AnchorId = Guid.Empty;
                foreach (XmlElement customElement in mElement.ChildElements())
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
                                    taxFieldInfo.TermIds.Add(new Guid(strTermId));
                                }
                            }
                        }
                        continue;
                    }
                    if (customElement.Name.Equals("Customization"))
                    {
                        foreach (XmlElement element in customElement.ChildElements())
                        {
                            if (element.Name.Equals("ArrayOfProperty"))
                            {
                                XmlElement sspidEle = null;
                                foreach (XmlElement propertyElement in element.ChildElements())
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
                                            if (name.Equals("SspId"))
                                            {
                                                sspidEle = propertyElement;                                               
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
                                                                logger.Log(AveLogLevel.DEBUG, "Cannot Get Term Store, TermStoreId: {0}", SspId);
                                                                SspId = Guid.Empty;
                                                            }
                                                            else
                                                            {
                                                                elements[0].InnerText = value + "|" + termStoreName;
                                                            }
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetTermStoreNameError, e.ToString());
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
                                                                logger.Log(AveLogLevel.DEBUG, "Cannot get term group, GroupID: {0}", GroupId);
                                                                GroupId = Guid.Empty;
                                                            }
                                                            else
                                                            {
                                                                elements[0].InnerText = value + "|" + groupName;
                                                            }
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetTermGroupNameError, e.ToString());
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
                                                                logger.Log(AveLogLevel.WARN, "Cannot get taxonomy field related Term Set, it may be deleted or removed. Term Set ID: {0}", TermSetId);
                                                            }
                                                            else
                                                            {
                                                                elements[0].InnerText = value + "|" + termSetName;
                                                            }
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetTermSetNameError, e.ToString());
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
                                                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetTermNameError, e.ToString());
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
                                                if (value.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    taxFieldInfo.IsKeywordsColumn = true;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.SetTaxonomyFieldError, e.ToString());
                                    }
                                }
                                //如果在处理termset时重新获取了新的sspid，需要将记录sspid的对应element内容进行更新
                                if (sspidEle != null && SspId != Guid.Empty)
                                {
                                    try
                                    {
                                        XmlNodeList elements = sspidEle.GetElementsByTagName("Value");
                                        if (elements != null && elements.Count > 0)
                                        {
                                            XmlElement valueElement = (XmlElement)elements[0];
                                            string value = valueElement.InnerText;
                                            if (!value.Equals(SspId.ToString(), StringComparison.OrdinalIgnoreCase))
                                            {
                                                string termStoreName = AveTaxonomyFieldUtility.GetTermStoreName(site, SspId);
                                                valueElement.InnerText = SspId.ToString() + "|" + termStoreName;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetTermStoreNameError, ex.ToString());
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

        public bool GetRelationship(string siteId, string listId, XmlElement FieldNode)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.GetRelationship"))
            {

                if (string.IsNullOrEmpty(listId))
                {
                    return false;
                }
                if (String.IsNullOrEmpty(FieldNode.GetAttribute("IsRelationship")))
                {
                    if (String.IsNullOrEmpty(FieldNode.GetAttribute("ID")))
                    {
                        return false;
                    }
                    else
                    {
                        return mSite.QueryService.GetFieldCollectionRelationship(siteId, listId, new Guid(FieldNode.GetAttribute("ID")).ToString());
                    }
                }
                return Convert.ToBoolean(FieldNode.GetAttribute("IsRelationship"));

            }

        }

        public bool ContainsFieldWithStaticName(string p)
        {
            return mFields.ContainsFieldWithStaticName(p);
        }

        public Dictionary<string, object> GetDisplayFields(IAveViewFieldCollection viewFields)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.GetDisplayFields"))
            {

                Dictionary<string, object> displayFields = new Dictionary<string, object>();
                if (viewFields == null)
                {
                    return displayFields;
                }
                displayFields.Add("Title", null);
                displayFields.Add("Created", null);
                displayFields.Add("Modified", null);
                if (viewFields != null)
                {
                    foreach (string fd in viewFields)
                    {
                        if (!displayFields.ContainsKey(fd))
                        {
                            displayFields.Add(fd, null);
                        }
                    }
                }
                return displayFields;

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "AveViewFields is name if XML node")]
        public Dictionary<string, object> GetDisplayFields(string viewFieldsSchema)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.GetDisplayFields_1"))
            {

                Dictionary<string, object> displayFields = new Dictionary<string, object>();
                if (viewFieldsSchema == null)
                {
                    return displayFields;
                }
                displayFields.Add("Title", null);
                displayFields.Add("Created", null);
                displayFields.Add("Modified", null);
                XmlDocument xDoc = new XmlDocument();
                try
                {
                    xDoc.LoadXml("<AveViewFields>" + viewFieldsSchema + "</AveViewFields>");
                }
                catch (XmlException)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.XmlFormatInvalid, "<AveViewFields>" + viewFieldsSchema + "</AveViewFields>");
                }
                foreach (XmlNode node in xDoc.GetElementsByTagName("FieldRef"))
                {
                    if (node.Attributes["Name"] != null)
                    {
                        string name = node.Attributes["Name"].Value;
                        if (!displayFields.ContainsKey(name))
                        {
                            displayFields.Add(name, null);
                        }
                    }
                }
                return displayFields;

            }

        }

        public string GetViewFields(Guid siteID, Guid listID)
        {
            return mSite.QueryService.GetViewFields(siteID, listID);
        }

        public string GetFields(Guid webId, Guid listId, bool useAPI = false)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.GetFields"))
            {

                string fieldSchemal = string.Empty;
                if (useAPI)
                {
                    fieldSchemal = mList.Fields.SchemaXml;
                }
                else
                {
                    fieldSchemal = mSite.QueryService.GetFields(mSite.ID, webId, listId);
                }
                XmlDocument fieldDoc = new XmlDocument();
                fieldDoc.LoadXml(fieldSchemal);
                foreach (XmlElement fieldElement in fieldDoc.FirstChild.ChildElements())
                {
                    ReplaceFieldAttribute(fieldElement);
                }
                return fieldDoc.OuterXml;

            }

        }

        public List<string> GetFields(Guid siteId, string scope)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldCollection.GetFields_1"))
            {

                List<string> fields = new List<string>();
                fields = mSite.QueryService.GetFields(siteId, scope);
                return fields;

            }

        }

        public bool GetFieldInSiteChildren(string scope, Guid siteId, Guid fieldId)
        {
            return mSite.QueryService.GetFieldInSiteChildren(scope, siteId, fieldId);
        }

        public IAveList List
        {
            get
            {
                if (mList == null)
                {
                    SPList list = mFields.List;
                    if (list != null)
                    {
                        mList = (mWeb.Lists as AveListCollection).CreateListByType(list);
                    }
                }
                return mList;
            }
        }

        #endregion

        public IAveField GetFieldById(Guid fieldId, bool bThrowException)
        {
            return CreateFieldByType((SPField)AveAssemblyUtility.InvokeMethod(mFields, "GetFieldById", new Type[] { typeof(Guid), typeof(bool) }, new object[] { fieldId, bThrowException }));
        }

        public IAveField GetFieldByInternalName(string strName, bool bThrowException)
        {
            return CreateFieldByType((SPField)AveAssemblyUtility.InvokeMethod(mFields, "GetFieldByInternalName", new Type[] { typeof(string), typeof(bool) }, new object[] { strName, bThrowException }));
        }

        #region override method

        public override int Count
        {
            get
            {
                return mFields.Count;
            }
        }

        public override IAveField this[int index]
        {
            get
            {
                return CreateFieldByType(mFields[index]);
            }
        }

        public override IEnumerator<IAveField> GetEnumerator()
        {
            foreach (SPField field in mFields)
            {
                yield return CreateFieldByType(field);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return CreateFieldByType(t as SPField);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (mList != null)
            {
                mList.Dispose();
                mList = null;
            }
        }

        public bool ContainsFieldWithInternalName(string fieldInternalName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region For Performance
        /// <summary>
        /// 备份时每个Item都会查询AllUserDataJunction，有效率问题。查询前应该调用mParentList.Fields.ContainsLookupField
        /// </summary>
        public bool ContainsLookupField
        {
            get
            {
                if (!mContainsLookupField.HasValue)
                {
                    mContainsLookupField = false;
                    foreach (SPField field in mFields)
                    {
                        SPFieldLookup lookup = field as SPFieldLookup;
                        if (lookup != null && !string.IsNullOrEmpty(lookup.LookupList))
                        {
                            mContainsLookupField = true;
                            break;
                        }
                    }
                }
                return mContainsLookupField.Value;
            }
        }

        #endregion


    }
}
