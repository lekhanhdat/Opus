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
using AvePoint.GCommon;
using System.Collections;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility.Cryptography;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    [AveCodeReview("2012/06/11", "sid.you@avepoint.com", "kexin.guo@AvePoint.com", new string[0] { }, null, true)]
    [AveCodeReview("2012/10/19", "fengfu.zhang@avepoint.com", "fengfu.zhang@avepoint.com", new string[3] { CodeReviewConstants.CHECK_LIST_ID_CO_8, CodeReviewConstants.CHECK_LIST_ID_FA_4, CodeReviewConstants.CHECK_LIST_ID_FA_10 }, null, true)]
    public class AveContentTypeHelper : IDisposable
    {
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveSPContentTypeCollection));

        private AveMappingManager mMappingManager;
        private AveObjectModelFactory mObjectModelFactory;
        private IAveWeb mWeb;
        private IAveList mList;
        private AveLanguageProcesser mLanguageProcesser;
        private IAveContentTypeMapping mContentTypeMapping;
        private IAveFieldMapping mAveFieldMapping;
        private Dictionary<Guid, Guid> mFieldIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, string> mEnsureFields = new Dictionary<Guid, string>();
        private Dictionary<string, string> mFieldInternalNameMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> mFieldDisplayNameMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private List<string> mSkippedFields = new List<string>();
        private List<string> mFailedFields = new List<string>();
        private IReport report = new AveWrapperReport();
        public List<Dictionary<string, string>> AvaliableContentTypeIdMappings { get; private set; }

        public Dictionary<IAveContentTypeId, string> WebcomePageViewXmls { get; private set; }

        public Dictionary<IAveContentTypeId, List<Guid>> ReqiredFieldCache = new Dictionary<IAveContentTypeId, List<Guid>>();

        private Dictionary<Guid, Guid> metadataFieldandTextFieldIdMappings = new Dictionary<Guid, Guid>();
        private Dictionary<string, string> metadataFieldandTextFieldInterNameMappings = new Dictionary<string, string>();
        private List<string> needRemoveTextfieldsWhenReorder = new List<string>();

        private Dictionary<IAveContentTypeId, string> mNeedUpdateDocumentTemplateContentTypes = new Dictionary<IAveContentTypeId, string>();
        private List<IAveContentTypeId> mNeedUpdateWFRetentionIds = new List<IAveContentTypeId>();
        #region Constructor
        /// <summary>
        /// Constuctor of content type helper.
        /// </summary>
        /// <param name="web">The parent web of content type helper.</param>
        /// <param name="list">The parent list of content type helper.</param>
        /// <param name="mappingManager">Mapping manager of AveSPSite instance.</param>
        /// <param name="objectModelFactory">Wrapper object model factory.</param>
        public AveContentTypeHelper(IAveWeb web, IAveList list, AveMappingManager mappingManager, AveObjectModelFactory objectModelFactory, AveLanguageProcesser languageProcesser)
        {
            AvaliableContentTypeIdMappings = new List<Dictionary<string, string>>();
            WebcomePageViewXmls = new Dictionary<IAveContentTypeId, string>();
            mWeb = web;
            mList = list;
            mMappingManager = mappingManager;
            mObjectModelFactory = objectModelFactory;
            mLanguageProcesser = languageProcesser;
        }
        #endregion

        public void InitMetadataFieldAndTextFieldMapping(bool needReloadFields)
        {
            if (needReloadFields)
            {
                mList.ReloadFields();
            }
            try
            {
                if (mList != null && metadataFieldandTextFieldIdMappings.Count == 0)
                {
                    foreach (IAveField field in mList.Fields)
                    {
                        IAveTaxonomyField taxField = field as IAveTaxonomyField;
                        if (taxField != null)
                        {
                            IAveField textField = mList.Fields[taxField.TextField];
                            metadataFieldandTextFieldIdMappings[taxField.ID] = textField.ID;
                            metadataFieldandTextFieldInterNameMappings[taxField.InternalName] = textField.InternalName;
                        }
                        if (field.InternalName.Equals("TaxCatchAll", StringComparison.OrdinalIgnoreCase))
                        {
                            metadataFieldandTextFieldIdMappings[field.ID] = field.ID;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Info("Can not get the need skip fields failed. Error:{0}", ex);
            }
        }
        /// <summary>
        /// Initialize the content type id mapping, field mapping, skippedFields and failedFields of the content type helper and ensure fields mapping.
        /// </summary>
        public void Initialize(IAveFieldMapping fieldMapping, IAveContentTypeMapping ctMapping, Dictionary<Guid, string> mEnsureFieldsMapping)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.Initialize"))
            {
                if (null == mWeb)
                {
                    return;
                }
                mContentTypeMapping = ctMapping;
                mAveFieldMapping = fieldMapping;
                AvaliableContentTypeIdMappings = GetAvailableContentTypeIdMappings();
                Dictionary<string, string> idMapping = GetContentTypeIdMapping();
                mContentTypeMapping.SetContentTypeIdMapping(idMapping);
                AvaliableContentTypeIdMappings.Add(idMapping);
                mFieldIdMapping = fieldMapping.EnumFieldIdMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                mFieldInternalNameMapping = fieldMapping.EnumFieldInternalNameMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                mFieldDisplayNameMapping = fieldMapping.EnumFieldDisplayNameMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                this.mEnsureFields = mEnsureFieldsMapping;
                mSkippedFields = fieldMapping.EnumSkippedFields().ToList();
                mFailedFields = fieldMapping.EnumFailedFields().ToList();
                InitMetadataFieldAndTextFieldMapping(false);
            }
        }

        /// <summary>
        /// Initialize the content type id mapping, field mapping, skippedFields and failedFields of the content type helper.
        /// </summary>
        public void Initialize(IAveFieldMapping fieldMapping, IAveContentTypeMapping ctMapping)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.Initialize"))
            {
                if (null == mWeb)
                {
                    return;
                }
                mContentTypeMapping = ctMapping;
                mAveFieldMapping = fieldMapping;
                AvaliableContentTypeIdMappings = GetAvailableContentTypeIdMappings();
                Dictionary<string, string> idMapping = GetContentTypeIdMapping();
                mContentTypeMapping.SetContentTypeIdMapping(idMapping);
                AvaliableContentTypeIdMappings.Add(idMapping);
                mFieldIdMapping = fieldMapping.EnumFieldIdMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                mFieldInternalNameMapping = fieldMapping.EnumFieldInternalNameMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                mFieldDisplayNameMapping = fieldMapping.EnumFieldDisplayNameMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                mSkippedFields = fieldMapping.EnumSkippedFields().ToList();
                mFailedFields = fieldMapping.EnumFailedFields().ToList();
                InitMetadataFieldAndTextFieldMapping(false);
            }
        }
        //add this method for unit test
        public void Initialize()
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.Initialize_1"))
            {
                if (null == mWeb)
                {
                    return;
                }
                //mContentTypeIdMapping = GetContentTypeIdMapping();
                AvaliableContentTypeIdMappings = GetAvailableContentTypeIdMappings();
            }
        }

        #region Handle web content type mapping property
        /// <summary>
        /// Get the content type id mapping which stored in List or Web properties.
        /// </summary>
        /// <returns>Dictionary<string, string></returns>
        private Dictionary<string, string> GetContentTypeIdMapping()
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.GetContentTypeIdMapping"))
            {
                Hashtable properties = null;
                if (null != mList)
                {
                    properties = mList.RootFolder.Properties;
                }
                else if (null != mWeb)
                {
                    properties = mWeb.AllProperties;
                }
                return GetContentTypeIdMappingFromHashtable(properties);
            }
        }

        private Dictionary<string, string> GetContentTypeIdMappingFromHashtable(Hashtable properties)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.GetContentTypeIdMappingFromHashtable"))
            {
                Dictionary<string, string> mapping = new Dictionary<string, string>(); ;
                if (null != properties && properties.Contains("ContentTypes_Mapping"))
                {
                    try
                    {
                        string mappingXml = (string)properties["ContentTypes_Mapping"];
                        mapping = ConvertXmlToCTIDMapping(mappingXml);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ConvertXmlToContentTypeError, e.ToString());
                    }
                }
                return mapping;
            }
        }

        private List<Dictionary<string, string>> GetAvailableContentTypeIdMappings()
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.GetAvailableContentTypeIdMappings"))
            {
                List<Dictionary<string, string>> avaliableMappings = new List<Dictionary<string, string>>();
                if (null != mList || null == mWeb)
                {
                    return new List<Dictionary<string, string>>();
                }
                //avaliableMappings.Add(mContentTypeIdMapping);
                IAveWeb parent = mWeb.ParentWeb;

                while (null != parent)
                {
                    using (parent)
                    {
                        Dictionary<string, string> mappings = GetContentTypeIdMappingFromHashtable(parent.AllProperties);
                        if (mappings.Count > 0)
                        {
                            avaliableMappings.Add(mappings);
                        }
                        parent = parent.ParentWeb;
                    }
                }
                return avaliableMappings;
            }
        }

        /// <summary>
        /// Write the content type id mapping to the list or web properties using the specified mapping.
        /// </summary>
        /// <param name="mappings">The mappings of the source and destination ids.</param>
        public void UpdateContentTypeIdMappingProperty(Dictionary<string, string> mappings)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateContentTypeIdMappingProperty"))
            {
                if (null != mList)
                {
                    UpdateContentTypeIdMappingProperty(mList, mappings);
                }
                else
                {
                    UpdateContentTypeIdMappingProperty(mWeb, mappings);
                }
            }
        }

        /// <summary>
        /// Write the content type id mapping to the specified web properties using the specified mapping.        
        /// </summary>
        /// <param name="web">The web to update.</param>
        /// <param name="mappings">The mappings of the source and destination ids.</param>
        public void UpdateContentTypeIdMappingProperty(IAveWeb web, Dictionary<string, string> mappings)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateContentTypeIdMappingProperty_web"))
            {
                if (null == mappings || mappings.Count == 0)
                {
                    return;
                }
                string mappingStr = ConvertCTIDMappingToXml(mappings);
                if (!web.AllProperties.Contains("ContentTypes_Mapping"))
                {
                    web.AllProperties.Add("ContentTypes_Mapping", mappingStr);
                }
                else
                {
                    web.AllProperties["ContentTypes_Mapping"] = mappingStr;
                }
                web.Update();
            }
        }

        /// <summary>
        /// Write the content type id mapping to the specified list properties using the specified mapping.  
        /// </summary>
        /// <param name="list">The list to update.</param>
        /// <param name="mappings">The mappings of the source and destination ids.</param>
        public void UpdateContentTypeIdMappingProperty(IAveList list, Dictionary<string, string> mappings)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateContentTypeIdMappingProperty_list"))
            {
                if (null == mappings || mappings.Count == 0 || list.RootFolder.Properties == null)
                {
                    return;
                }
                string mappingStr = ConvertCTIDMappingToXml(mappings);
                if (!list.RootFolder.Properties.Contains("ContentTypes_Mapping"))
                {
                    list.RootFolder.Properties.Add("ContentTypes_Mapping", mappingStr);
                }
                else
                {
                    list.RootFolder.Properties["ContentTypes_Mapping"] = mappingStr;
                }
                list.RootFolder.Update();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsDirectChildOfBuildInContentTypeForListContentType(string contentTypeId)
        {
            bool isBuildInChild = false;
            IAveContentTypeId ctId = GetContentTypeId(contentTypeId);
            isBuildInChild = AveBuiltInContentTypeId.Contains(ctId.Parent);
            return isBuildInChild;
        }

        /// <summary>
        /// Convert the xml string with the specified format to mapping dictionary.
        /// </summary>
        /// <param name="xml">The formatted xml string.</param>
        /// <returns></returns>
        internal Dictionary<string, string> ConvertXmlToCTIDMapping(string xml)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.ConvertXmlToCTIDMapping"))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                if (!doc.DocumentElement.Name.Equals("AvePointWebContentTypeMappings", StringComparison.OrdinalIgnoreCase))
                {
                    throw new AveException(string.Format("The mapping xml is not valid. \r\nXml={0}", doc.OuterXml));
                }

                Dictionary<string, string> mapping = new Dictionary<string, string>();
                Dictionary<string, NameMapping> nameMapping = new Dictionary<string, NameMapping>();
                foreach (XmlElement ele in doc.DocumentElement.ChildElements())
                {
                    string sourceId = ele.GetAttribute("SourceID");
                    string destId = ele.GetAttribute("CTID");
                    string sourceName = ele.GetAttribute("SourceName");
                    string destName = ele.GetAttribute("CTName");
                    mapping.Add(sourceId, destId);
                    nameMapping.Add(sourceId, new NameMapping { SourceName = sourceName, DestName = destName });
                }
                mContentTypeMapping.SetContentTypeNameMappingById(nameMapping);
                return mapping;
            }
        }

        /// <summary>
        /// Convert the content type id mapping dictionary to formatted xml string.
        /// </summary>
        /// <param name="mappings">Content type id mappings.</param>
        /// <returns>xml string</returns>
        internal string ConvertCTIDMappingToXml(Dictionary<string, string> mappings)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.ConvertCTIDMappingToXml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.AppendChild(doc.CreateElement("AvePointWebContentTypeMappings"));
                foreach (KeyValuePair<string, string> mapping in mappings)
                {
                    XmlElement mappingElement = doc.CreateElement("Mapping");
                    mappingElement.SetAttribute("SourceID", mapping.Key);
                    mappingElement.SetAttribute("CTID", mapping.Value);
                    NameMapping mappingValue = mContentTypeMapping.GetMappingRestoredContentTypeNameMappingById(mapping.Key);
                    if (mappingValue != null)
                    {
                        mappingElement.SetAttribute("SourceName", mappingValue.SourceName);
                        mappingElement.SetAttribute("CTName", mappingValue.DestName);
                    }
                    doc.DocumentElement.AppendChild(mappingElement);
                }
                return doc.OuterXml;
            }
        }
        #endregion

        #region Find content type
        /// <summary>
        /// Compare the buildin parents content types of the source and destination content type ids.
        /// </summary>
        /// <param name="sourceCTId">Source content type id.</param>
        /// <param name="destinationCTId">Destination content type id.</param>
        /// <returns>true, if the source content type id equals the destination or the source content type id is the child of the destination id.</returns>
        public bool IsBaseBuiltinContentTypeMatch(IAveContentTypeId sourceCTId, IAveContentTypeId destinationCTId)
        {
            return IsBaseBuiltinContentTypeMatch(sourceCTId, destinationCTId, false);
        }

        /// <summary>
        /// Compare the buildin parents content types of the source and destination content type ids.
        /// </summary>
        /// <param name="sourceCTId">Source content type id.</param>
        /// <param name="destinationCTId">Destination content type id.</param>
        /// <param name="isStrictCompare">true, just use IAveContentTypeId.Equals to compare the 2 content type ids.</param>
        /// <returns>true, if the source parent id equals the destination or the source parent id is the child of the destination.</returns>
        public bool IsBaseBuiltinContentTypeMatch(IAveContentTypeId sourceCTId, IAveContentTypeId destinationCTId, bool isUseMapping)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.IsBaseBuiltinContentTypeMatch"))
            {
                IAveContentTypeId srcId = sourceCTId;
                IAveContentTypeId desId = destinationCTId;
                srcId = GetBaseBuiltinContentTypeID(srcId);
                desId = GetBaseBuiltinContentTypeID(desId);
                if (isUseMapping)
                {
                    return (srcId.Equals(desId) || srcId.IsChildOf(desId) || desId.IsChildOf(srcId));
                }
                else
                {
                    return srcId.Equals(desId);
                }
            }
        }

        public IAveContentTypeId GetBaseBuiltinContentTypeID(IAveContentTypeId ctId)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.GetBaseBuiltInContentTypeID"))
            {
                while (!AveBuiltInContentTypeId.Contains(ctId))
                {
                    ctId = ctId.Parent;
                }
                return ctId;
            }
        }

        public IAveContentTypeId GetContentTypeId(string id)
        {
            if (id == null)
            {
                throw new ArgumentException();
            }
            if ((id.Length % 2) != 0)
            {
                throw new ArgumentException();
            }
            return mObjectModelFactory.CreateContentTypeId(id);
        }

        public IAveContentTypeId GetContentTypeIdFromMapping(string id)
        {
            for (int index = AvaliableContentTypeIdMappings.Count; index > 0;)
            {
                var mapping = AvaliableContentTypeIdMappings[--index];
                if (0 < mapping.Count && mapping.ContainsKey(id))
                {
                    return GetContentTypeId(mapping[id]);
                }
            }
            return null;
        }

        public IAveContentType FindContentTypeById(IAveContentTypeCollection collection, IAveContentTypeId id)
        {
            return collection[id];
        }

        public IAveContentType FindContentTypeByName(IAveContentTypeCollection collection, string name)
        {
            return FindContentTypeByName(collection, name, false, null, false);
        }

        public IAveContentType FindContentTypeByName(IAveContentTypeCollection collection, string name, bool needCompareBaseBuildin, IAveContentTypeId sourceId, bool isUseMapping)
        {
            IAveContentType contentType = collection[name];
            if (null == contentType || (needCompareBaseBuildin && !IsBaseBuiltinContentTypeMatch(sourceId, contentType.ID, isUseMapping)))
            {
                return null;
            }
            return contentType;
        }

        public IAveContentType FindChildContentTypeInCollection(IAveContentTypeCollection collection, IAveContentTypeId parentId)
        {
            foreach (IAveContentType ct in collection)
            {
                if (ct.Parent != null && ct.Parent.ID.Equals(parentId))
                {
                    return ct;
                }
            }
            return null;
        }

        public IAveContentType GetBuildinParentContentType(IAveContentTypeId id)
        {
            return FindContentTypeById(mWeb.AvailableContentTypes, GetBaseBuiltinContentTypeID(id));
        }

        public bool IsListContentTypeIdExist(string id)
        {
            if (mList != null)
            {
                return mList.ContentTypes[GetContentTypeId(id)] != null;
            }
            return false;
        }

        #endregion

        #region Compare Content type
        public bool CompareContentTypes(AveContentTypeInfo ctInfo, IAveContentType desContentType)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.CompareContentTypes"))
            {
                //if (desContentType.Sealed ^ ctInfo.Sealed)
                //{
                //    return false;
                //}
                if (desContentType.ReadOnly != ctInfo.ReadOnly)
                {
                    return false;
                }
                string tempName = mContentTypeMapping.GetMappingRestoredContentTypeNameById(ctInfo.Id, ctInfo.Name);
                //D5数据系统自带的Content Type只备份了Id和Name，其中Name包含/.
                if (tempName.IndexOf("/", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    return true;
                }
                if (!string.IsNullOrEmpty(tempName) && !tempName.StartsWith("$Resources:", StringComparison.OrdinalIgnoreCase) && !string.Equals(desContentType.Name, tempName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                //if (!string.Equals(desContentType.Description, ctInfo.Description, StringComparison.OrdinalIgnoreCase))
                //{
                //    return false;
                //}
                if (desContentType.Hidden != ctInfo.Hidden)
                {
                    return false;
                }
                if (desContentType.RequireClientRenderingOnNew != ctInfo.RequireClientRenderingOnNew)
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(ctInfo.Group) && !string.Equals(desContentType.Group, ctInfo.Group, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                if (!CompareContentTypeResourceFiles(ctInfo, desContentType))
                {
                    return false;
                }
                if (!CompareContentTypeDocumentTemplate(ctInfo, desContentType))
                {
                    return false;
                }
                if (!CompareContentTypeXmlElement(ctInfo, desContentType))
                {
                    return false;
                }
                if (!CompareContentTypeFields(ctInfo, desContentType))
                {
                    return false;
                }
                if (!CompareContentTypeResource(ctInfo, desContentType))
                {
                    return false;
                }
                return true;
            }
        }

        private bool CompareContentTypeResource(AveContentTypeInfo ctInfo, IAveContentType desContentType)
        {
            return desContentType.NameResource.CompareUserResource(mWeb, ctInfo.NameResource) &&
                desContentType.DescriptionResource.CompareUserResource(mWeb, ctInfo.DescriptionResource);
        }

        /// <summary>
        /// When the content type is conflict with the destination, return false.only compare the property which will affect the item value restore
        /// </summary>
        /// <param name="xmlField"></param>
        /// <param name="spField"></param>
        /// <returns>true, when conflict</returns>
        public bool CompareEnsureContentTypes(AveContentTypeInfo ctInfo, IAveContentType desContentType)
        {//contenttype在反插的时候认为兼容就可以，暂时不需要添加其他属性的比较，若发现有属性需要比较，在此处添加
            return true;
        }

        internal List<string> AveXMLDocumentCollectionToList(IAveXmlDocumentCollection documents)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.AveXMLDocumentCollectionToList"))
            {
                if (documents == null)
                {
                    return null;
                }
                List<string> xmlList = new List<string>();
                foreach (string document in documents)
                {
                    if (!document.StartsWith("<AveMD5Property", StringComparison.OrdinalIgnoreCase))
                    {
                        xmlList.Add(document);
                    }
                }
                return xmlList;
            }
        }

        internal Dictionary<string, string> GetXmlDocumentsElements(List<string> xmlDocuments)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.GetXmlDocumentsElements"))
            {
                try
                {
                    Dictionary<string, string> XmlDocumentElements = new Dictionary<string, string>();
                    XmlDocument mCtInfoDoc;
                    foreach (string XmlDocument in xmlDocuments)
                    {
                        mCtInfoDoc = new System.Xml.XmlDocument();
                        mCtInfoDoc.LoadXml(XmlDocument);
                        AddDocmentElementToDic(mCtInfoDoc.DocumentElement, XmlDocumentElements, "");
                    }
                    return XmlDocumentElements;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetXmlDocumentsElementsFailed, e);
                    return null;
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "AddDocmentElementToDic is method name.")]
        internal void AddDocmentElementToDic(XmlNode sub, Dictionary<string, string> XmlDocumentElements, string parentName)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.AddDocmentElementToDic"))
            {
                if (sub.Attributes == null || sub.ChildNodes == null)
                {
                    return;
                }
                if (!XmlDocumentElements.ContainsKey(parentName + "_" + sub.Name))
                {
                    if (sub.OuterXml.Contains("type=") && sub.Attributes.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(sub.InnerXml.Substring(0, sub.InnerXml.IndexOf('<') >= 0 ? sub.InnerXml.IndexOf('<') : sub.InnerXml.Length)) && !string.IsNullOrEmpty(sub.InnerXml.Substring(0, sub.InnerXml.IndexOf('<') >= 0 ? sub.InnerXml.IndexOf('<') : sub.InnerXml.Length)))
                        {
                            if (!XmlDocumentElements.ContainsKey(parentName + "_" + sub.Name + sub.Attributes[0].Value))
                            {
                                XmlDocumentElements.Add(parentName + "_" + sub.Name + sub.Attributes[0].Value, sub.InnerXml.Substring(0, sub.InnerXml.IndexOf('<') >= 0 ? sub.InnerXml.IndexOf('<') : sub.InnerXml.Length));
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(sub.InnerXml.Substring(0, sub.InnerXml.IndexOf('<') >= 0 ? sub.InnerXml.IndexOf('<') : sub.InnerXml.Length)))
                    {
                        XmlDocumentElements.Add(parentName + "_" + sub.Name, sub.InnerXml.Substring(0, sub.InnerXml.IndexOf('<') >= 0 ? sub.InnerXml.IndexOf('<') : sub.InnerXml.Length));
                    }
                }
                foreach (XmlAttribute attribute in sub.Attributes)
                {
                    if (!XmlDocumentElements.ContainsKey(parentName + "_" + sub.Name + "_" + attribute.Name) && !string.IsNullOrEmpty(attribute.Value))
                    {
                        XmlDocumentElements.Add(parentName + "_" + sub.Name + "_" + attribute.Name, attribute.Value);
                    }
                    else if (!string.IsNullOrEmpty(attribute.Value))
                    {
                        XmlDocumentElements[parentName + "_" + sub.Name + "_" + attribute.Name] += attribute.Value;
                    }
                }
                foreach (XmlNode node in sub.ChildNodes)
                {
                    AddDocmentElementToDic(node, XmlDocumentElements, parentName + "_" + sub.Name);
                }
            }
        }

        internal bool CompareContentTypeResourceFiles(AveContentTypeInfo ctInfo, IAveContentType desContentType)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.CompareContentTypeResourceFiles"))
            {
                foreach (AveContentTypeFileInfo fileinfo in ctInfo.ResourceFolderFiles)
                {
                    try
                    {
                        fileinfo.Url = desContentType.ResourceFolder.Url + fileinfo.Url.Substring(fileinfo.Url.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
                        IAveFile file = desContentType.ResourceFolder.Files[fileinfo.Url];
                        if (null == file || !file.Exists)
                        {
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldError, e.ToString());
                        return false;
                    }
                }
                return true;
            }
        }

        internal bool CompareContentTypeDocumentTemplate(AveContentTypeInfo ctInfo, IAveContentType desContentType)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.CompareContentTypeDocumentTemplate"))
            {
                if (!string.IsNullOrEmpty(ctInfo.DocumentTemplate))
                {
                    if (ctInfo.DocumentTemplate.IndexOf('/') >= 0)
                    {
                        if (!string.Equals(desContentType.DocumentTemplateUrl, ctInfo.DocumentTemplate, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!string.Equals(desContentType.DocumentTemplate, ctInfo.DocumentTemplate, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        internal bool CompareContentTypeXmlElement(AveContentTypeInfo ctInfo, IAveContentType desContentType)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.CompareContentTypeXmlElement"))
            {
                Dictionary<string, string> sourceContentTypeXmlElements = GetXmlDocumentsElements(ctInfo.XmlDocuments);
                Dictionary<string, string> desContentTypeXmlElements = GetXmlDocumentsElements(AveXMLDocumentCollectionToList(desContentType.XmlDocuments));

                if (sourceContentTypeXmlElements.Count != desContentTypeXmlElements.Count)
                {
                    return false;
                }
                else
                {
                    string policyId = "_p:Policy_p:PolicyItems_p:PolicyItem_UniqueId";
                    if (sourceContentTypeXmlElements.ContainsKey(policyId))
                    {
                        sourceContentTypeXmlElements.Remove(policyId);
                        if (desContentTypeXmlElements.ContainsKey(policyId))
                        {
                            desContentTypeXmlElements.Remove(policyId);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    string[] NeedSkipProperties = new string[] { "_act:AllowedContentTypes_LastModified" };
                    foreach (string key in sourceContentTypeXmlElements.Keys)
                    {
                        if (!NeedSkipProperties.Contains(key) && (!desContentTypeXmlElements.ContainsKey(key) || !sourceContentTypeXmlElements[key].Equals(desContentTypeXmlElements[key])))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 判断是否存在对应的attribute，如果存在，则根据Type获取对应个attribute value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xe"></param>
        /// <param name="attributeName"></param>
        /// <param name="parseMethod"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        private bool CheckSourceFieldLinkPropertyHasValue<T>(XmlElement xe, string attributeName, Func<string, T> parseMethod, out T attributeValue)
        {
            attributeValue = default(T);
            var attribute = xe.GetAttribute(attributeName);
            if (!string.IsNullOrEmpty(attribute))
            {
                attributeValue = parseMethod(attribute);
                return true;
            }
            return false;
        }

        internal bool CompareContentTypeFields(AveContentTypeInfo ctInfo, IAveContentType desContentType)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.CompareContentTypeFields"))
            {
                IAveFieldLinkCollection ctFieldLinks = desContentType.FieldLinks;
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ctInfo.FieldsSchemaXml);
                List<string> sourceFieldList = new List<string>();
                List<string> destinationFieldList = new List<string>();
                foreach (XmlElement xe in doc.DocumentElement.ChildElements())
                {
                    string sfieldId = xe.GetAttribute("ID");
                    string sfieldName = xe.GetAttribute("Name");
                    IAveFieldLink fieldLink = null;
                    Guid fieldId = Guid.Empty;
                    if (!string.IsNullOrEmpty(sfieldId))
                    {
                        fieldId = new Guid(sfieldId);
                        if (mFieldIdMapping != null && mFieldIdMapping.ContainsKey(fieldId))
                        {
                            fieldId = mFieldIdMapping[fieldId];
                        }
                    }
                    if (!string.IsNullOrEmpty(sfieldName))
                    {
                        if (mFieldInternalNameMapping != null && mFieldInternalNameMapping.ContainsKey(sfieldName))
                        {
                            sfieldName = mFieldInternalNameMapping[sfieldName];
                        }
                    }

                    fieldLink = ctFieldLinks[fieldId];
                    fieldLink = null == fieldLink ? ctFieldLinks[sfieldName] : fieldLink;
                    if (null == fieldLink)
                    {
                        return false; // return conflict
                    }
                    if (mWeb.Site.APIType != AveAPIType.BPOS_S || true)
                    {
                        IAveFieldCollection fieldCollection = null;
                        if (mList != null && mList.Fields != null)
                        {
                            fieldCollection = mList.Fields;
                        }
                        else
                        {
                            fieldCollection = mWeb.Fields;
                        }
                        if (fieldCollection.Contains(fieldId))
                        {
                            bool hidden;
                            if (CheckSourceFieldLinkPropertyHasValue<bool>(xe, "Hidden", attribute => attribute.Equals("true", StringComparison.CurrentCultureIgnoreCase), out hidden))
                            {
                                if (CheckFieldLinkProperty("Hidden", fieldLink.SchemaXml, fieldCollection[fieldId] != null ? fieldCollection[fieldId].Hidden : fieldLink.Hidden, fieldLink.Hidden) != hidden)
                                {
                                    return false;
                                }
                            }
                            bool required;
                            if (CheckSourceFieldLinkPropertyHasValue<bool>(xe, "Required", attribute => attribute.Equals("true", StringComparison.CurrentCultureIgnoreCase), out required))
                            {
                                if (CheckFieldLinkProperty("Required", fieldLink.SchemaXml, fieldCollection[fieldId] != null ? fieldCollection[fieldId].Required : fieldLink.Required, fieldLink.Required) != required)
                                {
                                    return false;
                                }
                            }
                            bool readOnly;
                            if (CheckSourceFieldLinkPropertyHasValue<bool>(xe, "ReadOnly", attribute => attribute.Equals("true", StringComparison.CurrentCultureIgnoreCase), out readOnly))
                            {
                                if (CheckFieldLinkProperty("ReadOnly", fieldLink.SchemaXml, fieldCollection[fieldId] != null ? fieldCollection[fieldId].ReadOnlyField : fieldLink.ReadOnly, fieldLink.ReadOnly) != readOnly)
                                {
                                    return false;
                                }
                            }
                            string displayName;
                            Func<string, string> parseMethod = attribute =>
                            {
                                // displayName需要走mapping逻辑
                                if (mFieldDisplayNameMapping != null && mFieldDisplayNameMapping.ContainsKey(attribute))
                                {
                                    return mFieldDisplayNameMapping[attribute];
                                }
                                return attribute;
                            };
                            if (CheckSourceFieldLinkPropertyHasValue<string>(xe, "DisplayName", parseMethod, out displayName))
                            {
                                XmlDocument destdoc = new XmlDocument();
                                destdoc.LoadXml(fieldLink.SchemaXml);
                                //ADO-158713 当目的端DisplayName 为Empty时，覆盖目的端会导致该问题
                                if (!string.IsNullOrEmpty(destdoc.DocumentElement.GetAttribute("DisplayName")))
                                {
                                    if (!string.Equals(destdoc.DocumentElement.GetAttribute("DisplayName"), displayName, StringComparison.CurrentCulture))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    sourceFieldList.Add(fieldLink.Name);
                }
                foreach (IAveFieldLink tmpFL in ctFieldLinks)
                {
                    destinationFieldList.Add(tmpFL.Name);
                }
                if (sourceFieldList.Count == destinationFieldList.Count)
                {
                    //client api doesn't support reorder fieldlinks
                    if (this.mWeb.Site.APIType != AveAPIType.BPOS_S)
                    {
                        for (int i = 0; i < sourceFieldList.Count; i++)
                        {
                            if (!string.Equals(sourceFieldList[i], destinationFieldList[i]))
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
        }
        #endregion

        #region Create content type
        public IAveContentTypePublisher CreateContentTypePublisher()
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.CreateContentTypePublisher"))
            {
                return mObjectModelFactory.CreateContentTypePublisher(mWeb.Site);
            }
        }

        public IAveContentType CreateContentType(IAveContentTypeId contentTypeId, IAveContentTypeCollection collection, string name)
        {
            return CreateContentType(contentTypeId, collection, name, false);
        }

        public IAveContentType CreateContentType(IAveContentTypeId contentTypeId, IAveContentTypeCollection collection, string name, bool useBuildinParent)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.CreateContentType"))
            {
                IAveContentType contentType = null;
                if (useBuildinParent)
                {
                    contentType = GetBuildinParentContentType(contentTypeId);
                    if (contentType != null)
                    {
                        contentType = CreateContentTypeWithParent(contentType, collection, name);
                    }
                }
                else
                {
                    contentType = mObjectModelFactory.CreateContentType(contentTypeId, collection, name);
                }
                return contentType;
            }
        }

        public IAveContentType CreateContentTypeWithParent(IAveContentType parent, IAveContentTypeCollection collection, string name)
        {
            return mObjectModelFactory.CreateContentType(parent, collection, name);
        }

        public IAveContentType CreateContentTypeWithoutParent(IAveContentTypeId contentTypeId, IAveContentTypeCollection collection, string name)
        {
            if (this.mObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
            {
                return null;
            }
            IAveContentType contentType = mObjectModelFactory.CreateContentType(contentTypeId);
            if (null != collection.List)
            {
                contentType.List = collection.List;
            }
            contentType.Web = collection.Web;
            contentType.Name = name;
            contentType.Initialize(collection);
            return contentType;
        }

        public IAveContentType CreateContentTypeWithSameParent(IAveContentTypeCollection collection, IAveContentType contentType)
        {
            if (this.mObjectModelFactory.ContextKind.IsServerMode10Upper())
            {
                return this.mObjectModelFactory.AddSameParentContentType(collection, contentType);
            }
            return null;
        }

        public string GetAvailableContentTypeName(string originalName, IAveContentTypeCollection collection)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.GetAvailableContentTypeName"))
            {
                string name = originalName;
                int extendNum = 0;
                while (extendNum++ < 500)
                {
                    if (FindContentTypeByName(collection, name) == null)
                    {
                        break;
                    }
                    name = originalName + "_" + extendNum;
                }
                return name;
            }
        }

        public IAveContentTypeId GetAvailableContentTypeId(IAveContentTypeId contentTypeId)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.GetAvailableContentTypeId"))
            {
                string parentId = contentTypeId.Parent.ToString();
                byte[] rgb = AveContentTypeIdUitlity.HexStringToBytes(parentId);
                string ctId = AveContentTypeIdUitlity.CreateChildFromGuid(rgb, Guid.NewGuid());
                return GetContentTypeId(ctId);
            }
        }

        public string GetAvailableContentTypeName(AveContentTypeInfo ctInfo, IAveContentTypeCollection desContentTypeCollection, ref IAveContentType desContentType)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.GetAvailableContentTypeName"))
            {
                int extendNum = 0;
                string originalName = ctInfo.Name;
                IAveContentTypeId ctId = GetContentTypeId(ctInfo.Id);
                try
                {
                    while (extendNum++ < 500)
                    {
                        desContentType = FindContentTypeByName(desContentTypeCollection, ctInfo.Name, false, ctId, !string.IsNullOrEmpty(ctInfo.MappingName));
                        if (desContentType != null)
                        {
                            if (IsBaseBuiltinContentTypeMatch(ctId, desContentType.ID) && CompareContentTypes(ctInfo, desContentType))
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                        ctInfo.Name = originalName + "_" + extendNum;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while GetAvailableContentTypeName. contentType Name: {0}  Error: {2}", ctInfo.Name, e);
                }
                return ctInfo.Name;
            }
        }
        #endregion

        #region Update content type
        public string UpdateContentType(IAveContentTypeCollection destContentTypeCollection, IAveContentType spCT, AveContentTypeInfo ctInfo, IAveFieldCollection fields, bool isNewCreated, AveContentTypeRestoreOption restoreOption, bool isHighVersionToLowVersion = false)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateContentType"))
            {
                string exception = string.Empty;
                if (spCT.ParentList != null && !spCT.ParentList.AllowContentTypes)//Agenda、Attendees、Document Library、Objectives、Meeting Series 这几种list 不支持contenttype
                {
                    return exception;
                }

                bool isUpdateChild = restoreOption.WEB_CONTENTTYPE_UPDATECHILD;
                bool isOverWriteFieldLinks = false;
                switch (restoreOption.FIELDLINKSOPTION)
                {
                    case ContentTypeFieldLinksOption.OverWrite:
                        isOverWriteFieldLinks = true;
                        break;
                    case ContentTypeFieldLinksOption.OverWriteIfNewCreated:
                        isOverWriteFieldLinks = isNewCreated;
                        break;
                }
                try
                {
                    if (spCT.ReadOnly)
                    {
                        spCT.ReadOnly = false;
                    }
                    SetValue(spCT.Sealed, ctInfo.Sealed, v => spCT.Sealed = v);
                    if (!string.IsNullOrEmpty(ctInfo.Name) && !ctInfo.Name.StartsWith("$Resources:", StringComparison.OrdinalIgnoreCase) && destContentTypeCollection[ctInfo.Name] == null)
                    {
                        SetValue(spCT.Name, ctInfo.Name, v => spCT.Name = v);
                    }
                    spCT.NameResource.SetUserResource(mWeb, ctInfo.NameResource);
                    SetValue(spCT.Description, ctInfo.Description, v => spCT.Description = v);
                    spCT.DescriptionResource.SetUserResource(mWeb, ctInfo.DescriptionResource);
                    SetValue(spCT.Hidden, ctInfo.Hidden, v => spCT.Hidden = v);
                    if (!string.IsNullOrEmpty(ctInfo.Group))
                    {
                        SetValue(spCT.Group, ctInfo.Group, v => spCT.Group = v);
                    }
                    if (!string.IsNullOrEmpty(ctInfo.EditFormUrl))
                    {
                        SetValue(spCT.EditFormUrl, AveReplaceProcessor.UrlReplace(ctInfo.EditFormUrl, mMappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mMappingManager.SiteMappingManager.SourceSiteInfo, mMappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl), v => spCT.EditFormUrl = v);
                    }
                    if (!string.IsNullOrEmpty(ctInfo.DisplayFormUrl))
                    {
                        //Merge CI ADO-140236，对DisplayFormUrl进行特殊处理，以对应非英语语言环境中blog page url还原出现定向错误的问题。 
                        if (spCT.ID.ToString().StartsWith("0x0110", StringComparison.OrdinalIgnoreCase))
                        {
                            if (mList != null)
                            {
                                string listUrl = mList.RootFolder.Url;
                                SetValue(spCT.DisplayFormUrl, listUrl + "/Post.aspx", v => spCT.DisplayFormUrl = v);
                            }
                            else
                            {
                                SetValue(spCT.DisplayFormUrl, AveReplaceProcessor.UrlReplace(ctInfo.DisplayFormUrl, mMappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mMappingManager.SiteMappingManager.SourceSiteInfo, mMappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl), v => spCT.DisplayFormUrl = v);
                            }
                        }
                        else
                        {
                            SetValue(spCT.DisplayFormUrl, AveReplaceProcessor.UrlReplace(ctInfo.DisplayFormUrl, mMappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mMappingManager.SiteMappingManager.SourceSiteInfo, mMappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl), v => spCT.DisplayFormUrl = v);
                        }
                    }
                    if (!string.IsNullOrEmpty(ctInfo.NewFormUrl))
                    {
                        SetValue(spCT.NewFormUrl, AveReplaceProcessor.UrlReplace(ctInfo.NewFormUrl, mMappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mMappingManager.SiteMappingManager.SourceSiteInfo, mMappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl), v => spCT.NewFormUrl = v);
                    }
                    if (!string.IsNullOrEmpty(ctInfo.EditFormTemplateName))
                    {
                        SetValue(spCT.EditFormTemplateName, ctInfo.EditFormTemplateName, v => spCT.EditFormTemplateName = v);
                    }
                    if (!string.IsNullOrEmpty(ctInfo.NewDocumentControl) && string.IsNullOrEmpty(spCT.NewDocumentControl))
                    {
                        spCT.NewDocumentControl = ctInfo.NewDocumentControl;
                    }
                    SetValue(spCT.RequireClientRenderingOnNew, ctInfo.RequireClientRenderingOnNew, v => spCT.RequireClientRenderingOnNew = v);

                    exception = RestoreResourceFiles(ctInfo, spCT, isHighVersionToLowVersion);

                    //CI-31725 DocumentTemplate Set需要传入ServerRelativeUrl 否则会找不到对应的template file
                    //code revert 后,CI-31725 此问题并没有重现. 而且修改导致bug ADO-169503，因此将修改revert 回去
                    if (!string.IsNullOrEmpty(ctInfo.DocumentTemplate))
                    {
                        //string docurl = AveReplaceProcessor.UrlReplace(ctInfo.DocumentTemplateUrl, mMappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mMappingManager.SiteMappingManager.SourceSiteInfo, mMappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                        try
                        {
                            if (this.mObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel && !string.Equals(spCT.DocumentTemplate, ctInfo.DocumentTemplate))
                            {
                                mNeedUpdateDocumentTemplateContentTypes.Add(spCT.ID, ctInfo.DocumentTemplate);
                            }
                            else
                            {
                                SetValue(spCT.DocumentTemplate, ctInfo.DocumentTemplate, v => spCT.DocumentTemplate = v);
                                //SetValue(spCT.DocumentTemplate, docurl, v => spCT.DocumentTemplate = v);
                            }
                        }
                        catch (Exception e)
                        {
                            mNeedUpdateDocumentTemplateContentTypes.Add(spCT.ID, ctInfo.DocumentTemplate);
                            log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetContentTypeDocumentTemplateFailed, ctInfo.Id, ctInfo.Name, e);
                            exception = e.Message;
                        }
                    }
                    try
                    {
                        UpdateXmlDocument(spCT, ctInfo, destContentTypeCollection.Web.Site.Url);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.RestoreContentTypeXmlDocumentFailed, ctInfo.Id, ctInfo.Name, e);
                        exception = e.Message;
                    }
                    foreach (string eachXml in spCT.XmlDocuments)
                    {
                        if (eachXml.StartsWith("<p:Policy", StringComparison.OrdinalIgnoreCase) && eachXml.Contains("action type=\"workflow\""))
                        {
                            mNeedUpdateWFRetentionIds.Add(spCT.ID);
                        }
                    }
                    if (!string.IsNullOrEmpty(ctInfo.FieldsSchemaXml))
                    {
                        UpdateFieldLinks(destContentTypeCollection, spCT, ctInfo, fields, isUpdateChild, isOverWriteFieldLinks, restoreOption);
                    }
                    //ADO-153555/修复转移list content type到目的端后修改为read only，新建item，overwrite方式反插，目的端content type未被overwrite的bug。
                    if (!spCT.Name.Equals("System", StringComparison.OrdinalIgnoreCase)
                        && !String.Equals(spCT.ID.ToString(), AveBuiltInContentTypeId.System, StringComparison.OrdinalIgnoreCase)
                        && !spCT.Name.Equals("Folder", StringComparison.OrdinalIgnoreCase)
                        && !String.Equals(spCT.ID.ToString(), AveBuiltInContentTypeId.Folder, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            spCT.Update(isUpdateChild && spCT.ParentList == null);
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while after UpdateFieldLinks,exception:{0}", e);
                            try
                            {
                                destContentTypeCollection.Update();
                            }
                            catch (Exception ex)
                            {
                                log.Warn(WrapperRestoreResource.UpdateContentTypeError, ex);
                            }
                            spCT.Update(isUpdateChild && spCT.ParentList == null);
                        }
                    }
                    destContentTypeCollection.Update();
                    if (spCT.ReadOnly != ctInfo.ReadOnly)
                    {
                        spCT.ReadOnly = ctInfo.ReadOnly;
                        spCT.Update(isUpdateChild && spCT.ParentList == null);
                        destContentTypeCollection.Update();
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.UpdateContentTypeError, ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("UpdateContentType", "UpdateContentType", AveReportObjectType.UpdateContentType, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToUpdateContentType, ex.Message));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.UpdateContentTypeError, e.ToString());
                    try
                    {
                        destContentTypeCollection.Update();
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.UpdateContentTypeError, ex.ToString());
                    }

                    log.Warn("update the content type:{0} with id:{1} in the web:{2} failed:{3}.", ctInfo.Name, ctInfo.Id, spCT.ParentWeb.Url, e.ToString());
                    exception = e.Message;
                }
                return exception;
            }
        }

        private string RestoreResourceFiles(AveContentTypeInfo ctInfo, IAveContentType spCt, bool isHighVersionToLowVersion)
        {
            string exception = string.Empty;
            foreach (AveContentTypeFileInfo fileInfo in ctInfo.ResourceFolderFiles)
            {
                exception = RestoreSingleResourceFile(fileInfo, spCt, isHighVersionToLowVersion);
            }
            return exception;
        }

        private string RestoreSingleResourceFile(AveContentTypeFileInfo fileInfo, IAveContentType spCt, bool isHighVersionToLowVersion)
        {
            if (fileInfo == null)
            {
                return "FileInfo is null.";
            }
            var exception = string.Empty;
            try
            {

                fileInfo.Url = spCt.ResourceFolder.ServerRelativeUrl + fileInfo.Url.Substring(fileInfo.Url.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
                var file = spCt.Web.GetFile(fileInfo.Url);

                if (file == null || (!file.Exists) || file.TimeLastModified != fileInfo.TimeLastModified)
                {
                    if (file != null && file.Exists)
                    {
                        if (isHighVersionToLowVersion && NeedSkipDocument(file, fileInfo, spCt.Web))
                            return exception;
                    }
                    if (IsGhostFile(fileInfo.MetaInfo) && spCt.ParentWeb.Site.APIType == AveAPIType.Server)
                    {
                        if (file == null || !file.Exists)
                        {
                            var setupPath = Convert.ToString(fileInfo.MetaInfo["vti_setuppath"]);
                            spCt.ResourceFolder.Files.AddGhosted(setupPath, fileInfo.Url, true);
                        }
                        else
                        {
                            file.RevertContentStream();
                            log.Debug("Revert Ghost File :{0}.", fileInfo.Url);
                        }
                    }
                    else
                    {
                        if (fileInfo.TimeLastModified != DateTime.MinValue)
                        {
                            spCt.ResourceFolder.Files.Add(fileInfo.Url, fileInfo.FileBinary, fileInfo.MetaInfo, spCt.Web.Author, spCt.Web.Author, fileInfo.TimeCreated, fileInfo.TimeLastModified, true);
                        }
                        else
                        {
                            spCt.ResourceFolder.Files.Add(fileInfo.Url, fileInfo.FileBinary, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // TODO:find better way to judge if the fileCollection have the file with the same url
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.AddResourceFileToCTFailed, spCt.Name, fileInfo.Url, e);
                exception = e.Message;
            }
            return exception;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "vti_hasdefaultcontent is a property name.")]
        private bool IsGhostFile(Hashtable metaInfo)
        {
            var isGhostFile = false;
            if (metaInfo != null)
            {
                var setupPath = Convert.ToString(metaInfo["vti_setuppath"]);
                if (!string.IsNullOrEmpty(setupPath))
                {
                    var hasDefaultContent = Convert.ToBoolean(metaInfo["vti_hasdefaultcontent"]);
                    if (hasDefaultContent)
                    {
                        isGhostFile = true;
                    }
                }
            }
            return isGhostFile;
        }

        private void SetValue<T>(T dest, T source, Action<T> setter) where T : struct
        {
            if (!source.Equals(dest))
            {
                setter(source);
            }
        }

        private void SetValue(string dest, string source, Action<string> setter)
        {
            if (!string.Equals(source, dest, StringComparison.OrdinalIgnoreCase))
            {
                setter(source);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of url. ")]
        private void UpdateXmlDocument(IAveContentType spCT, AveContentTypeInfo ctInfo, string siteUrl)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateXmlDocument"))
            {

                var needClearUris = new List<string> { "http://schemas.microsoft.com/sharepoint/events", "office.server.policy", "microsoft.office.server.policy.changes", "http://schemas.microsoft.com/sharepoint/v3/contenttype/forms/url" };

                foreach (var needClearUri in needClearUris)
                {
                    spCT.XmlDocuments.Delete(needClearUri);
                }

                for (int i = 0; i < ctInfo.XmlDocuments.Count; ++i)
                {
                    try
                    {
                        var currentNodeString = ctInfo.XmlDocuments[i];

                        if (currentNodeString.StartsWith(@"<customXsn", StringComparison.OrdinalIgnoreCase))
                        {
                            ProcessXSNNode(spCT, currentNodeString, siteUrl);
                        }
                        else
                        {
                            ProcessPolicyAndOthers(spCT, currentNodeString);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn(string.Format("An error occurred while restore contentType xmlDocument.contentType:{0}, xmlDocument:{1}.error:{2}", spCT.Name, ctInfo.XmlDocuments[i].ToString(), e.ToString()));
                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of url. ")]
        private void ProcessPolicyAndOthers(IAveContentType spCT, string currentNodeString)
        {

            XmlDocument temDoc = new XmlDocument();
            temDoc.LoadXml(currentNodeString);
            string namespaceUri = string.Empty;

            for (int j = 0; j < temDoc.FirstChild.Attributes.Count; j++)
            {
                if (temDoc.FirstChild.Attributes[j].Name.StartsWith("xmlns", StringComparison.OrdinalIgnoreCase))
                {
                    namespaceUri = temDoc.FirstChild.Attributes[j].Value;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(namespaceUri))
            {
                if (namespaceUri.Equals("http://schemas.microsoft.com/office/documentsets/welcomepageview", StringComparison.OrdinalIgnoreCase))
                {
                    if (temDoc.DocumentElement.Attributes["ViewId"] != null)
                    {
                        WebcomePageViewXmls.Add(spCT.ID, currentNodeString);
                        log.Debug("Add welcome page view mapping. ContentType Name:{0}, NodeString:{1}", spCT.Name, currentNodeString);
                    }
                }
                if (spCT.XmlDocuments[namespaceUri] == null)
                {
                    if (namespaceUri.Equals("http://schemas.microsoft.com/sharepoint/events", StringComparison.OrdinalIgnoreCase))
                    {
                        ReplaceAssmeblyVersion(temDoc);
                    }
                    spCT.XmlDocuments.Add(temDoc);
                }
            }
            //ADO-200605 支持转移Document Set 以及Information management policy settings等相关信息
            else if (spCT.XmlDocuments[namespaceUri] != null)
            {
                spCT.XmlDocuments.Delete(namespaceUri);
                spCT.XmlDocuments.Add(temDoc);
            }
        }

        private void ReplaceAssmeblyVersion(XmlDocument temDoc)
        {
            //原有逻辑，暂时保留
            temDoc.InnerXml = temDoc.InnerXml.Replace(
                        "<Assembly>Microsoft.Office.Policy, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c</Assembly>",
                        "<Assembly>Microsoft.Office.Policy, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c</Assembly>");
            InternalReplaceAssemblyVersion(temDoc);
        }

        private void InternalReplaceAssemblyVersion(XmlDocument temDoc)
        {
            //document set在ChangeContentTypeAssemblyXml中已经替换，但只针对document set。
            foreach (var assemblyXml in temDoc.DocumentElement.SelectNodes(@"Receiver/Assembly").OfType<XmlElement>())
            {
                if (string.IsNullOrEmpty(assemblyXml.InnerText)) continue;
                var assemblyName = new AssemblyName(assemblyXml.InnerText);
                if (assemblyName.Version == null) continue;

                var destSPVersion = new Version(mWeb.Site.SPVersion);
                var sourceSPVersion = assemblyName.Version;
                //原端Assembly Version大于目的端Assembly Version
                if (sourceSPVersion.Major > destSPVersion.Major && IsSharePointPublicKeyToken(assemblyName.GetPublicKeyToken()))
                {
                    //只替换MajorVersion，其他保持不变
                    assemblyName.Version = new Version(destSPVersion.Major, sourceSPVersion.Minor, sourceSPVersion.Build, sourceSPVersion.Revision);
                    assemblyXml.InnerText = assemblyName.FullName;
                }
            }
        }

        //通过强签名的public key token判断
        private bool IsSharePointPublicKeyToken(byte[] actual)
        {
            if (actual == null) return false;
            var expected = new byte[] { 113, 233, 188, 225, 17, 233, 66, 156 };
            return expected.SequenceEqual(actual);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of url. ")]
        private void ProcessXSNNode(IAveContentType spCT, string currentNodeString, string siteUrl)
        {
            for (int c = 0; c < spCT.XmlDocuments.Count; ++c)
            {
                if (spCT.XmlDocuments[c].StartsWith(@"<customXsn", StringComparison.OrdinalIgnoreCase))
                {
                    spCT.XmlDocuments.Delete("http://schemas.microsoft.com/office/2006/metadata/customXsn");
                    break;
                }
            }
            XmlDocument temDoc = new XmlDocument();
            temDoc.LoadXml(currentNodeString);
            if (temDoc.GetElementsByTagName("xsnScope").Count > 0)
            {
                //keep url format,usually the scope url is absolute
                if (AveUrlUtility.IsUrlRelative(temDoc.GetElementsByTagName("xsnScope")[0].InnerText))
                {
                    temDoc.GetElementsByTagName("xsnScope")[0].InnerText = spCT.Scope;
                }
                else
                {
                    temDoc.GetElementsByTagName("xsnScope")[0].InnerText = AveUrlUtility.CombineUrl(AveUrlUtility.GetServerUrl(siteUrl), spCT.Scope);
                }
            }
            if (temDoc.GetElementsByTagName("xsnLocation").Count > 0)
            {
                string documentTemplate = spCT.DocumentTemplate;
                if (documentTemplate.IndexOf('/') >= 0)
                {
                    documentTemplate = documentTemplate.Substring(documentTemplate.LastIndexOf('/') + 1);
                }
                if (mMappingManager != null) //DOC-56939 
                {
                    temDoc.GetElementsByTagName("xsnLocation")[0].InnerText =
                        AveReplaceProcessor.UrlReplace(temDoc.GetElementsByTagName("xsnLocation")[0].InnerText,
                            mMappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true),
                            mMappingManager.SiteMappingManager.SourceSiteInfo,
                            mMappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                }
                else
                {
                    temDoc.GetElementsByTagName("xsnLocation")[0].InnerText = spCT.ParentWeb.Url + "/" + spCT.ResourceFolder.Url +
                                                                              "/" + documentTemplate;
                }
            }
            spCT.XmlDocuments.Add(temDoc);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of url.")]
        private void UpdateWelcomePageViewId(IAveContentType contentType, Guid viewId, XmlDocument tempDoc)
        {
            const string welcomePageView = "http://schemas.microsoft.com/office/documentsets/welcomepageview";
            if (contentType.XmlDocuments[welcomePageView] != null)
            {
                contentType.XmlDocuments.Delete(welcomePageView);
            }
            tempDoc.DocumentElement.Attributes["ViewId"].Value = viewId.ToString();
            contentType.XmlDocuments.Add(tempDoc);
            contentType.Update(false);

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of url.")]
        public void UpdateWelcomePageViewId(AveSPList list)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateWelcomePageViewId"))
            {
                if (WebcomePageViewXmls.Keys.Count > 0)
                {
                    list.SPList.Reload();
                }
                Dictionary<Guid, Guid> listViewMapping = list.ParentSite.MappingManager.ListMappingManager.ListViewMapping;
                if (WebcomePageViewXmls.Count != 0 && listViewMapping.Count != 0)
                {
                    foreach (KeyValuePair<IAveContentTypeId, string> data in WebcomePageViewXmls)
                    {
                        try
                        {
                            XmlDocument tempDoc = new XmlDocument();
                            tempDoc.LoadXml(data.Value);
                            Guid viewId = new Guid(tempDoc.DocumentElement.Attributes["ViewId"].Value);
                            if (listViewMapping.ContainsKey(viewId))
                            {
                                IAveContentType desCT = list.SPList.ContentTypes[data.Key];
                                if (desCT != null)
                                {
                                    UpdateWelcomePageViewId(desCT, listViewMapping[viewId], tempDoc);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Warn("Update welcome page view error. Id:{0} .Error:{1}", data.Key, ex);
                        }
                    }
                }
                else
                {
                    XmlDocument tempDoc = new XmlDocument();
                    var contentTypes = list.SPList.ContentTypes;
                    foreach (var contentType in contentTypes)
                    {
                        try
                        {
                            const string welcomePageView = "http://schemas.microsoft.com/office/documentsets/welcomepageview";
                            if (contentType.XmlDocuments[welcomePageView] != null)
                            {
                                tempDoc.LoadXml(contentType.XmlDocuments[welcomePageView].ToString());
                                var viewIdAttribute = tempDoc.DocumentElement.GetAttribute("ViewId");
                                if (string.IsNullOrEmpty(viewIdAttribute)) continue;//如果是default view可能不包含ViewId这个属性

                                var viewId = new Guid(viewIdAttribute);
                                if (listViewMapping.ContainsKey(viewId))
                                {
                                    UpdateWelcomePageViewId(contentType, listViewMapping[viewId], tempDoc);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Warn("Update welcome page view with out content type restore error.Error:{0}", ex);
                        }
                    }
                }
            }
        }

        public void UpdateDocumentTemplate(AveSPList list)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateDocumentTemplate"))
            {
                if (mNeedUpdateDocumentTemplateContentTypes.Count > 0 && list.SPList != null)
                {
                    list.SPList.Reload();
                    UpdateDocumentTemplate(list.SPList.ContentTypes);
                }
            }
        }

        public void UpdateDefaultContentTypeFieldLink(AveSPList list)
        {
            IAveContentType defaultCt = null;
            IAveFieldLink fieldLink = null;
            try
            {
                using (new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateDefaultContentTypeFieldLink"))
                {
                    if (list.AveList != null && list.NeedUpdateToDefaultContentType != null && list.NeedUpdateToDefaultContentType.Count > 0)
                    {
                        var aveList = list.AveList;
                        defaultCt = aveList.ContentTypes[0];
                        foreach (string internalName in list.NeedUpdateToDefaultContentType)
                        {
                            var field = aveList.Fields.GetFieldByInternalName(internalName);
                            fieldLink = mObjectModelFactory.CreateFieldLink(field);
                            defaultCt.FieldLinks.Add(fieldLink);
                            defaultCt.Update();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while updating field link to default content type, content type: {0}, field link: {1}, error message: {2}", defaultCt.Name, fieldLink.Name, e);
            }
        }

        public void UpdateDocumentTemplate(AveSPWeb web)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateDocumentTemplate"))
            {
                if (mNeedUpdateDocumentTemplateContentTypes.Count > 0 && web.SPWeb != null)
                {
                    UpdateDocumentTemplate(web.SPWeb.ContentTypes);
                }
            }
        }

        private void UpdateDocumentTemplate(IAveContentTypeCollection contentTypes)
        {
            foreach (KeyValuePair<IAveContentTypeId, string> data in mNeedUpdateDocumentTemplateContentTypes)
            {
                IAveContentType desCT = contentTypes[data.Key];
                if (desCT != null)
                {
                    try
                    {
                        desCT.DocumentTemplate = data.Value;
                        desCT.Update(false);
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetContentTypeDocumentTemplateFailed, desCT.ID, desCT.Name, ex);
                    }
                }
            }
        }

        public void UpdateStartWFRetention(AveSPList list)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateContentTypeWFRetention"))
            {
                if (list.SPList != null && mNeedUpdateWFRetentionIds.Count > 0)
                {
                    list.SPList.Reload();
                    foreach (var ctID in mNeedUpdateWFRetentionIds)
                    {
                        var destCT = list.SPList.ContentTypes[ctID];
                        foreach (var eachXml in destCT.XmlDocuments)
                        {
                            if (eachXml.StartsWith("<p:Policy", StringComparison.OrdinalIgnoreCase) && eachXml.Contains("action type=\"workflow\""))
                            {
                                destCT.XmlDocuments.Delete("office.server.policy");
                                var doc = new XmlDocument();
                                doc.LoadXml(eachXml);
                                var nodeList = doc.GetElementsByTagName("action");
                                foreach (var eachNode in nodeList)
                                {
                                    if ((eachNode as XmlElement).GetAttribute("type").Equals("workflow", StringComparison.OrdinalIgnoreCase))
                                    {
                                        var sourceId = new Guid((eachNode as XmlElement).GetAttribute("id"));
                                        Guid targetId;
                                        if (list.ParentSite.MappingManager.SiteMappingManager.TryGetValueFromWorkflowIdMapping(sourceId, out targetId))
                                        {
                                            (eachNode as XmlElement).SetAttribute("id", targetId.ToString());
                                        }
                                    }
                                }
                                try
                                {
                                    destCT.XmlDocuments.Add(doc);
                                    destCT.Update();
                                }
                                catch (Exception ex)
                                {
                                    log.Warn("An error occurred while getting the contentType workflow retention. ContentType id:{0}, ContentType name:{1}. Error message:{2}..", destCT.ID.ToString(), destCT.Name, ex);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateFieldLinks(IAveContentTypeCollection destContentTypeCollection, IAveContentType spCT, AveContentTypeInfo ctInfo, IAveFieldCollection fields, bool isUpdateChild, bool isOverWriteFieldLinks, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateFieldLinks"))
            {
                if (restoreOption.SkipFieldLinkWhenFindInContentTypeMapping && !string.IsNullOrEmpty(ctInfo.MappingName))
                {
                    //ADO-205916 CM遇到了数个客户 源端的content type不再用了，改用目的端的content type，而且不想修改目的端content type 的field link，因此添加了该属性，
                    //如果设置了content type mapping，并且skip field link的话，就不还原fileld  link
                    log.Info("Skip restore content tyepe field link because of contentType mapping, content  type name: {0}, mapping name:{1}", ctInfo.Name, ctInfo.MappingName);
                    return;
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ctInfo.FieldsSchemaXml);
                IAveFieldLinkCollection ctFieldLinks = spCT.FieldLinks;
                int fieldCount = doc.DocumentElement.ChildNodes.Count;
                bool needUpdateBeforeReorder = false;
                List<string> fieldLinkNames = new List<string>();
                Hashtable difNames = new Hashtable();
                for (int i = 0; i < fieldCount; ++i)
                {
                    XmlNode xe = doc.DocumentElement.ChildNodes[i];
                    if (xe.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }
                    IAveFieldLink tempLink = RestoreFieldLink(spCT, (XmlElement)xe, ctFieldLinks, fields, mFieldIdMapping, mFieldInternalNameMapping, mFieldDisplayNameMapping, mEnsureFields, mSkippedFields, mFailedFields, ref needUpdateBeforeReorder, restoreOption);
                    if (tempLink != null)
                    {
                        if (spCT.Fields.Contains(tempLink.ID))
                        {
                            try
                            {
                                string fieldInternalName = spCT.Fields[tempLink.ID].InternalName;
                                if (!fieldInternalName.Equals(tempLink.Name))//不能忽略大小写，否则后面取不到field。
                                {
                                    difNames.Add(tempLink.Name, fieldInternalName);
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldByInternalNameError, e.ToString());
                            }
                        }
                        if (!fieldLinkNames.Contains(tempLink.Name))
                        {
                            fieldLinkNames.Add(tempLink.Name);
                        }
                    }
                }
                if (needUpdateBeforeReorder)
                {
                    // 还原FieldLinks的时候会update list，导致list的version增长，但是在update ContentType的时候会去checkContentTypeCollection中的listVersion，如果和list的version不一致会抛出异常。调用ContentTypeCollection的update方法可以update这个值。所以在要把Collection的update放在ContentType的update之前。
                    destContentTypeCollection.Update();
                    spCT.Update(isUpdateChild && spCT.ParentList == null);
                }
                if (isOverWriteFieldLinks)
                {
                    RemoveDefaultFieldLinks(spCT, ctFieldLinks, fieldLinkNames, difNames);
                }
                ReorderFieldLink(ctFieldLinks, fieldLinkNames, difNames);
            }
        }

        /// <summary>
        /// Build in 的Field Link 不应该更新，要准确比较值不同才更新，否则会导致FieldLink 的 XML 改变。
        /// </summary>
        /// <param name="fieldLink"></param>
        /// <param name="field"></param>
        /// <param name="fieldXml"></param>
        /// <param name="fieldDisplayNameMapping"></param>
        private void UpdateFieldLinkProperty(IAveFieldLink fieldLink, IAveField field, XmlElement fieldXml, Dictionary<string, string> fieldDisplayNameMapping)
        {
            string sfieldDisplayName = fieldXml.GetAttribute("DisplayName");
            bool hasHiddenAttribute = !string.IsNullOrEmpty(fieldXml.GetAttribute("Hidden"));
            bool hasRequiredAttribute = !string.IsNullOrEmpty(fieldXml.GetAttribute("Required"));
            bool hasReadOnlyAttribute = !string.IsNullOrEmpty(fieldXml.GetAttribute("ReadOnly"));

            #region we should set "Node" value.(For info path document)
            if (fieldXml.HasAttribute("Node") && !string.IsNullOrEmpty(fieldXml.Attributes["Node"].Value))
            {
                if (!string.Equals(fieldLink.XPath, fieldXml.Attributes["Node"].Value, StringComparison.OrdinalIgnoreCase))
                {
                    fieldLink.XPath = fieldXml.Attributes["Node"].Value;
                }
            }
            if (fieldXml.HasAttribute("Aggregation") && !string.IsNullOrEmpty(fieldXml.Attributes["Aggregation"].Value))
            {
                if (!string.Equals(fieldLink.AggregationFunction, fieldXml.Attributes["Aggregation"].Value, StringComparison.OrdinalIgnoreCase))
                {
                    fieldLink.AggregationFunction = fieldXml.Attributes["Aggregation"].Value;
                }
            }
            #endregion
            if (!string.IsNullOrEmpty(sfieldDisplayName))
            {
                var sfieldMappingDisplayName = String.Empty;
                var destDisplayName = GetDestinationFieldLinkDisplayName(fieldLink, field);
                if (fieldDisplayNameMapping != null && fieldDisplayNameMapping.TryGetValue(sfieldDisplayName, out sfieldMappingDisplayName))
                {
                    if (!string.Equals(destDisplayName, sfieldMappingDisplayName, StringComparison.OrdinalIgnoreCase))
                    {
                        fieldLink.DisplayName = sfieldMappingDisplayName;
                    }
                }
                //对buid in的fieldLink应用fieldLanguageNameMapping
                else if (mLanguageProcesser != null && mLanguageProcesser.FieldMapping != null && mLanguageProcesser.FieldMapping.TryGetValue(sfieldDisplayName, out sfieldMappingDisplayName))
                {
                    if (!string.Equals(destDisplayName, sfieldMappingDisplayName, StringComparison.OrdinalIgnoreCase))
                    {
                        fieldLink.DisplayName = sfieldMappingDisplayName;
                    }
                }
                else
                {
                    if (!string.Equals(destDisplayName, sfieldDisplayName, StringComparison.OrdinalIgnoreCase))
                    {
                        fieldLink.DisplayName = sfieldDisplayName;
                    }
                }
            }

            #region 这三个属性放到最后更新。ADO-158300
            if (hasHiddenAttribute)
            {
                //采用跟原端一致的fieldLink的判断逻辑
                bool value = fieldXml.GetAttribute("Hidden").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                if (CheckFieldLinkProperty("Hidden", fieldLink.SchemaXml, field != null ? field.Hidden : fieldLink.Hidden, fieldLink.Hidden) != value)
                {
                    fieldLink.Hidden = value;//使用API赋值是正确的
                }
            }
            if (hasRequiredAttribute)
            {
                bool value = fieldXml.GetAttribute("Required").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                if (CheckFieldLinkProperty("Required", fieldLink.SchemaXml, field != null ? field.Required : fieldLink.Required, fieldLink.Required) != value)
                {
                    fieldLink.Required = value;
                }
            }
            if (hasReadOnlyAttribute)
            {
                bool value = fieldXml.GetAttribute("ReadOnly").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                if (CheckFieldLinkProperty("ReadOnly", fieldLink.SchemaXml, field != null ? field.ReadOnlyField : fieldLink.ReadOnly, fieldLink.ReadOnly) != value)
                {
                    fieldLink.ReadOnly = value;
                }
            }
            #endregion
        }

        private string GetDestinationFieldLinkDisplayName(IAveFieldLink fieldLink, IAveField field)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.GetDestinationFieldLinkDisplayName"))
            {
                XmlDocument doc = new XmlDocument();
                var displayName = field.Title;
                try
                {
                    if (!string.IsNullOrEmpty(fieldLink.SchemaXml))
                    {
                        doc.LoadXml(fieldLink.SchemaXml);
                        if (doc.DocumentElement.HasAttribute("DisplayName"))
                        {
                            displayName = doc.DocumentElement.GetAttribute("DisplayName");
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CheckNeedAddFieldLinkError, e.ToString());
                    displayName = fieldLink.DisplayName;//BPOS下无法获取到fieldlink的SchemaXml属性，所以只能使用fieldlink的属性来判断
                }
                return displayName;
            }
        }

        private IAveFieldLink RestoreFieldLink(IAveContentType spCT, XmlElement fieldXml, IAveFieldLinkCollection ctFieldLinksCollection, IAveFieldCollection aveFields, Dictionary<Guid, Guid> fieldIdMapping, Dictionary<string, string> fieldInternalNameMapping, Dictionary<string, string> fieldDisplayNameMapping, Dictionary<Guid, string> ensureFields, List<string> skippedFields, List<string> failedFields, ref bool needUpdate, AveContentTypeRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.RestoreFieldLink"))
            {
                string sfieldId = fieldXml.GetAttribute("ID");
                string sfieldName = fieldXml.GetAttribute("Name");
                IAveFieldLink fieldLink = null;
                try
                {
                    if (!string.IsNullOrEmpty(sfieldName))
                    {
                        //Skip 的Field 只是不走还原逻辑，但不应该在fieldLink 中过滤出去
                        //if ((skippedFields.Count > 0 && skippedFields.Contains(sfieldName)) || (failedFields.Count > 0 && failedFields.Contains(sfieldName)))
                        if (failedFields.Count > 0 && failedFields.Contains(sfieldName))
                        {
                            return null;
                        }
                    }
                    Guid fieldId = Guid.Empty;
                    IAveField field = null;
                    if (!string.IsNullOrEmpty(sfieldId))
                    {
                        fieldId = new Guid(sfieldId);
                        if (fieldIdMapping != null && fieldIdMapping.ContainsKey(fieldId))
                        {
                            fieldId = fieldIdMapping[fieldId];
                        }
                        if (ctFieldLinksCollection[fieldId] != null)
                        {
                            fieldLink = ctFieldLinksCollection[fieldId];
                        }
                    }
                    if (!string.IsNullOrEmpty(sfieldName) && fieldLink == null)
                    {
                        if (fieldInternalNameMapping != null && fieldInternalNameMapping.ContainsKey(sfieldName))
                        {
                            sfieldName = fieldInternalNameMapping[sfieldName];
                        }
                        if (ctFieldLinksCollection[sfieldName] != null)
                        {
                            fieldLink = ctFieldLinksCollection[sfieldName];
                        }
                    }

                    if (fieldLink != null && spCT.Fields.Contains(fieldLink.ID))
                    {
                        field = spCT.Fields[fieldLink.ID];
                        UpdateFieldLinkProperty(fieldLink, field, fieldXml, fieldDisplayNameMapping);
                    }
                    else
                    {
                        needUpdate = true;
                        if (fieldId != Guid.Empty)
                        {
                            try
                            {
                                field = aveFields.GetById(fieldId);
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldError, e.ToString());
                                #region web level的CT应该从parentWeb上找一下field
                                if (mList == null && mWeb != null)
                                {
                                    try
                                    {
                                        field = mWeb.AvailableFields.GetById(fieldId);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldError, ex.ToString());
                                    }
                                }
                                #endregion
                            }
                        }
                        if (field == null && !string.IsNullOrEmpty(sfieldName))
                        {
                            field = aveFields.GetFieldByInternalName(sfieldName, false);
                            #region web level的CT应该从parentWeb上找一下field
                            if (field == null && mWeb != null)
                            {
                                field = mWeb.AvailableFields.GetFieldByInternalName(sfieldName, false);
                            }
                            #endregion
                        }
                        if (field == null && mWeb != null && mList != null)
                        {
                            //web list不为空，在list反插逻辑中需要从web上找到ct需要的field来添加到list上，否则还原到目的端之后，目的端list上的ct出现找不到field的错误
                            if (!spCT.ID.ToString().StartsWith("0x010100806213320A313D4DA11D1B1D6CC700CF", StringComparison.OrdinalIgnoreCase) && restoreOption.List_ContentType_CheckWebFieldLink)
                            {
                                field = EnsureListField(fieldId, sfieldName, mList);
                            }
                        }

                        //sharepoint will add lookup dependent field automaticlly, and bpos will failed to update contenttype if don't skipe this field
                        if (field != null && !AveSPUtility.IsDependentLookupField(field))
                        {
                            if (metadataFieldandTextFieldIdMappings.Values.Contains(field.ID))
                            {
                                return null;
                            }
                            fieldLink = mObjectModelFactory.CreateFieldLink(field);
                            if (fieldLink != null && !ctFieldLinksCollection.Any(link => link.ID == fieldLink.ID))
                            {
                                UpdateFieldLinkProperty(fieldLink, field, fieldXml, fieldDisplayNameMapping);
                                ctFieldLinksCollection.Add(fieldLink);
                            }
                        }
                    }
                    if (field != null && mEnsureFields.Keys.Contains(field.ID))
                    {
                        mAveFieldMapping.AddFieldInternalNameMapping(sfieldName, field.InternalName);
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.RestoreFieldLinkFailed, sfieldId, sfieldName, e);
                    //mLog.Warn("An error occurred while restore contentType fieldLink, fieldLink Name: {0}", sfieldName);
                }
                return fieldLink;
            }
        }

        /// <summary>
        /// list level 的反插需要确保ct的fieldlink在list上是存在的,web level不起作用
        /// </summary>
        /// <param name="field"></param>
        /// <param name="list"></param>
        private IAveField EnsureListField(Guid fieldId, string fiedlName, IAveList list)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.EnsureListField"))
            {
                IAveField field = null;
                if (fieldId != Guid.Empty)
                {
                    try
                    {
                        field = mWeb.Fields.GetById(fieldId);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldError, e.ToString());
                        #region web level的CT应该从parentWeb上找一下field
                        if (mList == null && mWeb != null)
                        {
                            try
                            {
                                field = mWeb.AvailableFields.GetById(fieldId);
                            }
                            catch (Exception ex)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldError, ex.ToString());
                            }
                        }
                        #endregion
                    }
                }
                if (field == null && !string.IsNullOrEmpty(fiedlName))
                {
                    field = mWeb.Fields.GetFieldByInternalName(fiedlName, false);
                    #region web level的CT应该从parentWeb上找一下field
                    if (field == null && mList == null && mWeb != null)
                    {
                        field = mWeb.AvailableFields.GetFieldByInternalName(fiedlName, false);
                    }
                    #endregion
                }
                if (field != null && list != null && !list.Fields.Contains(field.ID))
                {
                    list.Fields.Add(field);
                    list.Update();
                }
                return field;
            }
        }

        /// <summary>
        /// 使用id来创建的时候原端ct的parent是event的情况下，目的端fieldlink添加frecurrency的时候目的端ct出现异常
        /// WorkspaceLink的添加会导致item在edit的时候出错，所以也要跳过
        /// 所以暂时不能添加，只做跳过处理，不影响正常逻辑
        /// 这个逻辑只能暂时处理，6.1之后考虑反插使用最底层parent进行创建来避免这类问题
        /// </summary>
        /// <param name="spCT"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        private bool FieldLinkNeedAdded(IAveContentType spCT, IAveField field)
        {
            bool needAdded = true;
            try
            {
                IAveContentTypeId srcId = mObjectModelFactory.CreateContentTypeId(spCT.ID.ToString());
                IAveContentTypeId eventId = mObjectModelFactory.CreateContentTypeId(AveBuiltInContentTypeId.Event);
                if ((field.ID.ToString().Equals("f2e63656-135e-4f1c-8fc2-ccbe74071901") || field.ID.ToString().Equals("08fc65f9-48eb-4e99-bd61-5946c439e691")) && (srcId.IsChildOf(eventId) && mWeb.ContentTypes[srcId.Parent] == null && mWeb.AvailableContentTypes[srcId.Parent] == null))
                {
                    needAdded = false;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CheckNeedAddFieldLinkError, e);
            }
            return needAdded;
        }

        /// <summary>
        /// 判断fieldLink是否是hidden，如果是fieldLink的xml中有hidden属性就以xml为主，如果没有属性首先要看field是否是hidden的，如果是hidden的话就直接返回true，
        /// 如果field不是hidden的还要继续检查field的readonlyfield属性，如果是true的话，fieldlink也是hidden的！
        /// </summary>
        /// <param name="fieldLink"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        private bool CheckFieldLinkProperty(string property, string fieldLinkSchema, bool realValue, bool fieldLinkValue)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.CheckFieldLinkIsHidden"))
            {
                bool fieldLink = false;
                XmlDocument doc = new XmlDocument();
                try
                {
                    if (!string.IsNullOrEmpty(fieldLinkSchema))
                    {
                        doc.LoadXml(fieldLinkSchema);
                        bool hasSchema = doc.DocumentElement.HasAttribute(property);
                        if (!hasSchema || !bool.TryParse(doc.DocumentElement.Attributes[property].Value, out fieldLink))
                        {
                            fieldLink = realValue;
                        }
                    }
                    else
                    {
                        fieldLink = realValue;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CheckNeedAddFieldLinkError, e.ToString());
                    fieldLink = fieldLinkValue;//BPOS下无法获取到fieldlink的SchemaXml属性，所以只能使用fieldlink的属性来判断
                }
                return fieldLink;
            }
        }

        private void RemoveDefaultFieldLinks(IAveContentType spCT, IAveFieldLinkCollection fieldLinkCollection, List<string> needKeepLinks, Hashtable difNames)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.RemoveDefaultFieldLinks"))
            {
                try
                {
                    needRemoveTextfieldsWhenReorder.Clear();
                    //ADO-42425:对于开启Enterprise Keywords,list 默认的content type出现的三个field link: Enterprise Keywords， TaxKeywordTaxHTField，Taxonomy Catch All Column不应该删除
                    List<Guid> needKeepFieldLinkIds = new List<Guid>() { new Guid("23f27201-bee3-471e-b2e7-b64fd8b7ca38"), new Guid("1390a86a-23da-45f0-8efe-ef36edadfb39"), new Guid("f3b0adf9-c1a2-4b02-920d-943fba4b3611") };
                    //对于custom field mapping changetometadata的column的textfield会被remove 导致sp出错，所以需要保留
                    List<string> needKeepTextFieldLinks = new List<string>();
                    List<string> removeFieldLink = new List<string>();
                    var fields = spCT.List == null ? spCT.Web.AvailableFields : spCT.List.Fields;
                    foreach (string fieldLinkName in needKeepLinks)
                    {
                        try
                        {
                            var fieldName = difNames.Contains(fieldLinkName) ? difNames[fieldLinkName].ToString() : fieldLinkName;
                            IAveField taxonomyField = fields.GetField(fieldName);
                            if (taxonomyField is IAveTaxonomyField)
                            {
                                Guid textFieldId = (taxonomyField as IAveTaxonomyField).TextField;
                                var textFieldLinkName = fieldLinkCollection[textFieldId].Name;
                                needKeepTextFieldLinks.Add(textFieldLinkName);
                            }
                        }
                        catch (Exception e)
                        {
                            log.Debug("Can not get the field:{0},error:{1}", fieldLinkName, e);
                        }
                    }
                    foreach (IAveFieldLink fieldLink in fieldLinkCollection)
                    {
                        IAveField field = null;
                        try
                        {
                            field = spCT.Fields.GetFieldByInternalName(fieldLink.Name);
                            if (!field.ReadOnlyField && !fieldLink.Hidden && !needKeepLinks.Contains(fieldLink.Name) && !needKeepTextFieldLinks.Contains(fieldLink.Name) && !needKeepFieldLinkIds.Contains(fieldLink.ID))
                            {
                                if (metadataFieldandTextFieldIdMappings.Values.Contains(field.ID))
                                {
                                    continue;
                                }
                                removeFieldLink.Add(fieldLink.Name);
                            }
                        }
                        catch (ArgumentException)
                        { }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.AddToRemoveFieldLinkFailed, e);
                        }
                        if (!needKeepLinks.Contains(fieldLink.Name) && !needKeepTextFieldLinks.Contains(fieldLink.Name)
                            && !needKeepFieldLinkIds.Contains(fieldLink.ID) && !metadataFieldandTextFieldIdMappings.Values.Contains(fieldLink.ID))
                        {
                            if (!removeFieldLink.Contains(fieldLink.Name))
                            {
                                removeFieldLink.Add(fieldLink.Name);
                            }
                        }
                    }
                    foreach (string fieldLink in removeFieldLink)
                    {
                        try
                        {
                            log.Debug("Remove field link:{0}", fieldLink);
                            if (metadataFieldandTextFieldInterNameMappings.ContainsKey(fieldLink))//如果Metadata column被delete。则metadata text column不能参加Reorder。
                            {
                                needRemoveTextfieldsWhenReorder.Add(metadataFieldandTextFieldInterNameMappings[fieldLink]);
                            }
                            fieldLinkCollection.Delete(fieldLink);
                        }
                        catch (Exception e)
                        {
                            log.Warn(string.Format("An error occurred while delete fieldLink:{0}. error:{1}", fieldLink, e.ToString()));
                        }
                    }
                    needKeepTextFieldLinks.Clear();
                }
                catch (Exception e)
                {
                    log.Warn(string.Format("An error occurred while RemoveDefaultFieldLinks. error:{0}", e.ToString()));
                }
            }
        }

        private void ReorderFieldLink(IAveFieldLinkCollection ctFieldLinksCollection, List<string> originFieldNames, Hashtable names)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.ReorderFieldLink"))
            {
                string[] orderNames = new string[ctFieldLinksCollection.Count - needRemoveTextfieldsWhenReorder.Count];
                int i = 0;
                foreach (string name in originFieldNames)
                {
                    if (ctFieldLinksCollection[name] != null && i < orderNames.Length)
                    {
                        if (names.ContainsKey(name))
                        {
                            orderNames[i++] = names[name].ToString();
                        }
                        else
                        {
                            orderNames[i++] = name;
                        }
                    }
                }
                foreach (IAveFieldLink fieldLink in ctFieldLinksCollection)
                {
                    //needRemoveTextfieldsWhenReorder里的Field不能参与Reorder，否则order失败。
                    if (!originFieldNames.Contains(fieldLink.Name) && i < orderNames.Length && !needRemoveTextfieldsWhenReorder.Contains(fieldLink.Name))
                    {
                        orderNames[i++] = fieldLink.Name;
                    }
                }
                ctFieldLinksCollection.Reorder(orderNames);
            }
        }

        public static void ActivateFeature(AveSPList list, string featureId)
        {
            if (ActivateFeature(list.ParentWeb, featureId))
            {
                //ADO-24261,ADO-21304,因为只reload了web，导致在还原list下的文件时，对mlist的修改不能作用到file的parentlist上，所以在reloadweb的同时，reloadlist。
                list.ReloadList();
            }
        }

        public static bool ActivateFeature(AveSPWeb web, string featureId)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.ActivateFeature"))
            {
                Guid featureGuid = new Guid(featureId);
                if (featureGuid == Guid.Empty)
                {
                    return false;
                }
                //用它来判断是否需要reloadweb和list，只有开了新的feature才被置为true。
                bool needReload = false;
                try
                {
                    if (web.SPWeb.Site.Features[featureGuid] == null)
                    {
                        web.SPWeb.Site.Features.Add(featureGuid);
                        needReload = true;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ActiateFeatureFail, featureGuid, e.ToString());
                    try
                    {
                        if (web.SPWeb.Features[featureGuid] == null)
                        {
                            web.SPWeb.Features.Add(featureGuid);
                            needReload = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.ActivateFeatureFailedEventMessage(featureGuid, ex));
                    }
                }
                if (needReload)
                {
                    web.SPWeb.ReloadWeb();
                }
                return needReload;
            }
        }

        private bool NeedSkipDocument(IAveFile file, AveContentTypeFileInfo mDocumentInfo, IAveWeb mParentWeb)
        {
            //if (this.mDocumentInfo.IsView) //After ProcessViewInfo();
            //{
            //    return false;
            //}
            bool needSkip = false;
            try
            {
                if (!mParentWeb.Site.IsOnlineSite)
                {
                    //Version sourceVersion = new Version(mParentWeb.ParentSite.SourceSiteInfo.SPVersion);
                    //Version targetVersion = new Version(mParentWeb.Site.SPVersion);
                    //if (sourceVersion.Major < 16 || sourceVersion.Major <= targetVersion.Major)
                    //{//此问题是解决local到local，源端version比目的端高的时候，一些文件被skip了的情况，以后sharepoint出新版本时候需要注意这个地方
                    //    return needSkip;
                    //}
                    switch (file.ParentFolder.ParentList.BaseTemplate)
                    {
                        case AveListTemplateType.MasterPageCatalog:
                            needSkip = true;
                            break;
                        case AveListTemplateType.GenericList:
                            needSkip = file.ParentFolder.ServerRelativeUrl.StartsWith(file.ParentFolder.ParentList.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
                            break;
                        case AveListTemplateType.DocumentLibrary:
                            needSkip = file.ParentFolder.ServerRelativeUrl.StartsWith(file.ParentFolder.ParentList.RootFolder.ServerRelativeUrl + "/Forms", StringComparison.OrdinalIgnoreCase);
                            break;
                        //Online-Local, 16-15, skip restoring pages.
                        default:
                            needSkip = mDocumentInfo.Url.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase);
                            break;
                    }
                }
                return needSkip;
            }
            catch (Exception ex)
            {
                log.Debug(string.Format("Can not recognize the resource file. Message:{0}", ex.ToString()));
                return false;
            }
        }

        #endregion

        #region MD5 Property
        public string GetMD5FromXmlDocuments(IAveContentType spCT)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.GetMD5FromXmlDocuments"))
            {
                string md5 = string.Empty;
                if (spCT != null)
                {
                    try
                    {
                        string namespaceUri = "AveMD5Property";
                        if (spCT.XmlDocuments[namespaceUri] != null)
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(spCT.XmlDocuments[namespaceUri]);
                            if (doc.DocumentElement.HasAttribute("MD5"))
                            {
                                md5 = doc.DocumentElement.GetAttribute("MD5");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("GetMD5FromXmlDocuments Error.Exception:" + ex.ToString());
                    }
                }
                return md5;
            }
        }

        public void UpdateMD5ToXmlDocuments(IAveContentType spCT)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateMD5ToXmlDocuments"))
            {
                try
                {
                    if (spCT != null)
                    {
                        string namespaceUri = "AveMD5Property";
                        if (spCT.XmlDocuments[namespaceUri] != null)
                        {
                            spCT.XmlDocuments.Delete(namespaceUri);
                        }
                        XmlDocument doc = new XmlDocument();
                        XmlElement root = doc.CreateElement(namespaceUri, namespaceUri);
                        doc.AppendChild(root);
                        doc.DocumentElement.SetAttribute("MD5", GetCurrentMD5Property(spCT));
                        spCT.XmlDocuments.Add(doc);
                        spCT.Update();
                    }
                }
                catch (Exception ex)
                {
                    log.Error("UpdateMD5ToXmlDocuments Error.Exception:" + ex.ToString());
                }
            }
        }
        public string GetCurrentMD5Property(IAveContentType spCT)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.GetCurrentMD5Property"))
            {
                if (String.IsNullOrEmpty(spCT.MD5))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(spCT.ReadOnly.ToString());
                    builder.Append(";" + spCT.Name);
                    builder.Append(";" + spCT.Hidden.ToString());
                    builder.Append(";" + spCT.RequireClientRenderingOnNew.ToString());
                    builder.Append(";" + spCT.Group);
                    builder.Append(";" + BuildResourceFolderFilesMD5(spCT));
                    builder.Append(";" + spCT.DocumentTemplateUrl);
                    builder.Append(";" + spCT.DocumentTemplate);
                    builder.Append(";" + BuildXmlDocumentsMD5(spCT));
                    builder.Append(";" + spCT.GetFieldLinkSchemaXml());
                    spCT.MD5 = SHA1Hash(builder.ToString());
                }
                return spCT.MD5;
            }
        }
        public string BuildXmlDocumentsMD5(IAveContentType spCT)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.BuildXmlDocumentsMD5"))
            {
                StringBuilder builder = new StringBuilder();
                string MD5Property = String.Empty;
                if (spCT.XmlDocuments["AveMD5Property"] != null)
                {
                    MD5Property = spCT.XmlDocuments["AveMD5Property"];
                }
                foreach (string doc in spCT.XmlDocuments)
                {
                    if (doc.Equals(MD5Property))
                    {
                        continue;
                    }
                    builder.Append(doc);
                }
                return builder.ToString();
            }
        }
        public string BuildResourceFolderFilesMD5(IAveContentType spCT)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.BuildResourceFolderFilesMD5"))
            {
                StringBuilder builder = new StringBuilder();
                if (spCT.ResourceFolder != null)
                {
                    foreach (IAveFile file in spCT.ResourceFolder.Files)
                    {
                        builder.Append(file.Url);
                    }
                }
                return builder.ToString();
            }
        }
        public string SHA1Hash(string text)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.SHA1Hash"))
            {
                if (string.IsNullOrEmpty(text))
                    return string.Empty;
                IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
                byte[] orginaldata = Encoding.Default.GetBytes(text);
                byte[] data = hash.ComputeHash(orginaldata);
                string hashValue = BitConverter.ToString(data);
                hashValue = hashValue.Replace("-", string.Empty);
                return hashValue;
            }
        }
        #endregion
        public void Dispose()
        {
            report.Dispose();
        }

    }

    #region moved to wrapper contract
    //public enum ContentTypeExistStatus
    //{
    //    None,
    //    Exist,
    //    ExistInParent,
    //    ConflictInChildrenById
    //}
    #endregion

    public class AveContentTypeIdUitlity
    {
        static char[] s_mphex2ch = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        internal static void CharsOfByte(byte b, StringBuilder sb)
        {
            sb.Append(s_mphex2ch[b >> 4]);
            sb.Append(s_mphex2ch[b & 15]);
        }

        internal static string HexStringFromBytes(byte[] rgb)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeIdUtility.HexStringFromBytes"))
            {

                StringBuilder sb = new StringBuilder("0x", 2 + ((rgb != null) ? (rgb.Length * 2) : 0));
                if (rgb != null)
                {
                    foreach (byte num in rgb)
                    {
                        CharsOfByte(num, sb);
                    }
                }
                return sb.ToString();

            }

        }

        internal static byte Hex(char ch)
        {
            switch (ch)
            {
                case '0':
                    return 0;

                case '1':
                    return 1;

                case '2':
                    return 2;

                case '3':
                    return 3;

                case '4':
                    return 4;

                case '5':
                    return 5;

                case '6':
                    return 6;

                case '7':
                    return 7;

                case '8':
                    return 8;

                case '9':
                    return 9;

                case 'A':
                case 'a':
                    return 10;

                case 'B':
                case 'b':
                    return 11;

                case 'C':
                case 'c':
                    return 12;

                case 'D':
                case 'd':
                    return 13;

                case 'E':
                case 'e':
                    return 14;

                case 'F':
                case 'f':
                    return 15;
            }
            throw new ArgumentException();
        }

        internal static byte[] HexStringToBytes(string id)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeIdUtility.HexStringToBytes"))
            {

                if (id == null)
                {
                    throw new ArgumentException();
                }
                if ((id.Length % 2) != 0)
                {
                    throw new ArgumentException();
                }
                char[] chArray = id.ToCharArray();
                if (((chArray.Length < 2) || (chArray[0] != '0')) || (chArray[1] != 'x'))
                {
                    throw new ArgumentException();
                }
                int index = 2;
                int num2 = (chArray.Length - index) / 2;
                byte[] buffer = null;
                if (num2 > 0)
                {
                    int num3 = 0;
                    buffer = new byte[num2];
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = (byte)((Hex(chArray[index]) << 4) | Hex(chArray[index + 1]));
                        index += 2;
                        if (num3 > 0)
                        {
                            num3--;
                        }
                        else if (buffer[i] == 0)
                        {
                            num3 = 0x10;
                        }
                    }
                    if (num3 > 0)
                    {
                        throw new ArgumentException();
                    }
                }
                else
                {
                    buffer = new byte[0];
                }
                return buffer;

            }

        }

        internal static string CreateChildFromGuid(byte[] buffer, Guid g)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeIdUtility.CreateChildFromGuid"))
            {

                byte[] destinationArray = new byte[(buffer.Length + 1) + 0x10];
                if (buffer != null)
                {
                    Array.Copy(buffer, destinationArray, buffer.Length);
                }
                destinationArray[buffer.Length] = 0;
                Array.Copy(g.ToByteArray(), 0, destinationArray, buffer.Length + 1, 0x10);
                return HexStringFromBytes(destinationArray);

            }

        }
    }
}