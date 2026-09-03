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
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    [AveCodeReview("2012/06/11", "sid.you@avepoint.com", "kexin.guo@AvePoint.com", new string[0] { }, null, true)]
    public class AveContentTypeHelper: IDisposable
    {
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveSPContentTypeCollection));

        private AveMappingManager mMappingManager;
        private AveObjectModelFactory mObjectModelFactory;
        private IAveWeb mWeb;
        private IAveList mList;
        private IAveContentTypeMapping mContentTypeMapping;
        //private Dictionary<string, string> mContentTypeIdMapping;
        // private Dictionary<string, NameMapping> mContentTypeNameMapping = new Dictionary<string, NameMapping>();
        private List<Dictionary<string, string>> mAvaliableContentTypeIdMappings = new List<Dictionary<string, string>>();
        private Dictionary<Guid, Guid> mFieldIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, string> mEnsureFields;
        private Dictionary<string, string> mFieldInternalNameMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> mSourceTextTaxonomyDic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private IReport report = new AveWrapperReport();
        private Dictionary<IAveContentTypeId, string> mNeedUpdateDocumentTemplateContentTypes;
        public List<Dictionary<string, string>> AvaliableContentTypeIdMappings
        {
            get
            {
                return mAvaliableContentTypeIdMappings;
            }
        }
        public Dictionary<IAveContentTypeId,Dictionary<Guid,Tuple<bool?,bool?,bool?>>> ContentTypeFieldShowInFormCache { get;set; } 
        public Dictionary<IAveContentTypeId, List<Guid>> ReqiredFieldCache = new Dictionary<IAveContentTypeId, List<Guid>>();
        private List<IAveContentTypeId> listContentTypeIds;
        public List<IAveContentTypeId> ListContentTypeIds
        {
            get
            {
                if (listContentTypeIds == null)
                {
                    this.listContentTypeIds = this.mWeb.GetAllListContentTypeIds();
                }
                return listContentTypeIds;
            }
        }
        //public Dictionary<string, NameMapping> ContentTypeNameMapping
        //{
        //    get
        //    {
        //        return mContentTypeNameMapping;
        //    }
        //}
        #region Constructor
        /// <summary>
        /// Constuctor of content type helper.
        /// </summary>
        /// <param name="web">The parent web of content type helper.</param>
        /// <param name="list">The parent list of content type helper.</param>
        /// <param name="mappingManager">Mapping manager of AveSPSite instance.</param>
        /// <param name="objectModelFactory">Wrapper object model factory.</param>
        public AveContentTypeHelper(IAveWeb web, IAveList list, AveMappingManager mappingManager, Dictionary<string, string> sourceTextTaxonomyDic, AveObjectModelFactory objectModelFactory)
        {
            mWeb = web;
            mList = list;
            mMappingManager = mappingManager;
            mSourceTextTaxonomyDic = sourceTextTaxonomyDic;
            mObjectModelFactory = objectModelFactory;
        }
        #endregion

        /// <summary>
        /// Initialize the content type id mapping and field mapping of the content type helper.
        /// </summary>
        public void Initialize(IAveFieldMapping fieldMapping, IAveContentTypeMapping ctMapping)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.Initialize"))
            {
#endif
                if (null == mWeb)
                {
                    return;
                }
                mContentTypeMapping = ctMapping;
                Dictionary<string, string> idMapping = GetContentTypeIdMapping();
                mContentTypeMapping.SetContentTypeIdMapping(idMapping);
                mAvaliableContentTypeIdMappings.Add(idMapping);
                mAvaliableContentTypeIdMappings = GetAvaliableContentTypeIdMappings();
                mFieldIdMapping = fieldMapping.EnumFieldIdMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                mFieldInternalNameMapping = fieldMapping.EnumFieldInternalNameMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                //if (null != mMappingManager)
                //{
                //    if (null != mList)
                //    {
                //        mFieldInternalNameMapping = mMappingManager.SiteMappingManager.ListFieldsInternalNameMapping.ContainsKey(mList.ID) ? mMappingManager.SiteMappingManager.ListFieldsInternalNameMapping[mList.ID] : null;
                //        //mEnsureFields = mMappingManager.SiteMappingManager.ListEnsureFields.ContainsKey(mList.ID) ? mMappingManager.SiteMappingManager.ListEnsureFields[mList.ID] : null;
                //    }
                //    else
                //    {
                //        mFieldIdMapping = mMappingManager.SiteMappingManager.WebFieldsIdMapping.ContainsKey(mWeb.ID) ? mMappingManager.SiteMappingManager.WebFieldsIdMapping[mWeb.ID] : null;
                //        mFieldInternalNameMapping = mMappingManager.SiteMappingManager.WebFieldsInternalNameMapping.ContainsKey(mWeb.ID) ? mMappingManager.SiteMappingManager.WebFieldsInternalNameMapping[mWeb.ID] : null;
                //    }
                //}
#if PerformanceLog
            }
#endif
        }
        //add this method for unit test
        public void Initialize()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.Initialize_1"))
            {
#endif
                if (null == mWeb)
                {
                    return;
                }
                //mContentTypeIdMapping = GetContentTypeIdMapping();
                mAvaliableContentTypeIdMappings = GetAvaliableContentTypeIdMappings();
#if PerformanceLog
            }
#endif
        }
        #region Handle web content type mapping property
        /// <summary>
        /// Get the content type id mapping which stored in List or Web properties.
        /// </summary>
        /// <returns>Dictionary<string, string></returns>
        private Dictionary<string, string> GetContentTypeIdMapping()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetContentTypeIdMapping"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }



        private Dictionary<string, string> GetContentTypeIdMappingFromHashtable(Hashtable properties)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetContentTypeIdMappingFromHashtable"))
            {
#endif
                Dictionary<string, string> mapping = new Dictionary<string, string>();
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
#if PerformanceLog
            }
#endif
        }

        private List<Dictionary<string, string>> GetAvaliableContentTypeIdMappings()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetAvaliableContentTypeIdMappings"))
            {
#endif
                List<Dictionary<string, string>> avaliableMappings = new List<Dictionary<string, string>>();
                if (null != mList || null == mWeb)
                {
                    return new List<Dictionary<string, string>>();
                }
                //avaliableMappings.Add(mContentTypeIdMapping);
                IAveWeb parent = mWeb.ParentWeb;

                while (null != parent && parent.Exists)
                {
                    Dictionary<string, string> mappings = GetContentTypeIdMappingFromHashtable(parent.AllProperties);
                    if (mappings.Count > 0)
                    {
                        avaliableMappings.Add(mappings);
                    }
                    parent = parent.ParentWeb;
                }
                return avaliableMappings;
#if PerformanceLog
            }
#endif
        }
        /// <summary>
        /// Update current content type id mapping.
        /// </summary>
        /// <param name="sourceId">The content type id of the source.</param>
        /// <param name="destinationId">The content type id of the destination</param>
        //public void SetContentTypeIdMapping(string sourceId, string destinationId)
        //{
        //    if (!mContentTypeIdMapping.ContainsKey(sourceId))
        //    {
        //        mContentTypeIdMapping.Add(sourceId, destinationId);
        //    }
        //    else
        //    {
        //        mContentTypeIdMapping[sourceId] = destinationId;
        //    }
        //}
        //public void SetContentTypeNameMapping(string sourceId, string sourceName, string desName)
        //{
        //    if (!mContentTypeNameMapping.ContainsKey(sourceId))
        //    {
        //        mContentTypeNameMapping.Add(sourceId, new NameMapping(sourceName, desName));
        //    }
        //    else
        //    {
        //        mContentTypeNameMapping[sourceId] = new NameMapping(sourceName, desName);
        //    }
        //}

        /// <summary>
        /// Write the content type id mapping to the list or web properties.
        /// </summary>
        //public void UpdateContentTypeIdMappingProperty()
        //{
        //    UpdateContentTypeIdMappingProperty(mContentTypeIdMapping);
        //}

        /// <summary>
        /// Write the content type id mapping to the list or web properties using the specified mapping.
        /// </summary>
        /// <param name="mappings">The mappings of the source and destination ids.</param>
        public void UpdateContentTypeIdMappingProperty(Dictionary<string, string> mappings)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateContentTypeIdMappingProperty"))
            {
#endif
                if (null != mList)
                {
                    UpdateContentTypeIdMappingProperty(mList, mappings);
                }
                else
                {
                    UpdateContentTypeIdMappingProperty(mWeb, mappings);
                }
                //mContentTypeIdMapping = mappings;
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// Write the content type id mapping to the specified web properties using the specified mapping.        
        /// </summary>
        /// <param name="web">The web to update.</param>
        /// <param name="mappings">The mappings of the source and destination ids.</param>
        public void UpdateContentTypeIdMappingProperty(IAveWeb web, Dictionary<string, string> mappings)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateContentTypeIdMappingProperty_web"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsDirectChildOfBuildInContentTypeForListContentType(string contentTypeId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.IsDirectChildOfBuildInContentTypeForListContentType"))
            {
#endif
                bool isBuildInChild = false;
                IAveContentTypeId ctId = GetContentTypeId(contentTypeId);
                isBuildInChild = AveBuiltInContentTypeId.Contains(ctId.Parent);
                return isBuildInChild;
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// Write the content type id mapping to the specified list properties using the specified mapping.  
        /// </summary>
        /// <param name="list">The list to update.</param>
        /// <param name="mappings">The mappings of the source and destination ids.</param>
        public void UpdateContentTypeIdMappingProperty(IAveList list, Dictionary<string, string> mappings)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateContentTypeIdMappingProperty_list"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// Convert the xml string with the specified format to mapping dictionary.
        /// </summary>
        /// <param name="xml">The formatted xml string.</param>
        /// <returns></returns>
        internal Dictionary<string, string> ConvertXmlToCTIDMapping(string xml)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.ConvertXmlToCTIDMapping"))
            {
#endif
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                if (!doc.DocumentElement.Name.Equals("AvePointWebContentTypeMappings", StringComparison.OrdinalIgnoreCase))
                {
                    throw new AveException(string.Format("The mapping xml is not valid. \r\nXml={0}", doc.OuterXml));
                }

                Dictionary<string, string> mapping = new Dictionary<string, string>();
                Dictionary<string, NameMapping> nameMapping = new Dictionary<string, NameMapping>();
                foreach (XmlElement ele in doc.DocumentElement.ChildNodes)
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
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// Convert the content type id mapping dictionary to formatted xml string.
        /// </summary>
        /// <param name="mappings">Content type id mappings.</param>
        /// <returns>xml string</returns>
        internal string ConvertCTIDMappingToXml(Dictionary<string, string> mappings)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.ConvertCTIDMappingToXml"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        //public string ContentTypeIDMappingToXml()
        //{
        //    return ConvertCTIDMappingToXml(mContentTypeIdMapping);
        //}
        #endregion

        #region Find content type
        /// <summary>
        /// Compare the buildin parents content types of the source and destination content type ids.
        /// </summary>
        /// <param name="sourceCTId">Source content type id.</param>
        /// <param name="destinationCTId">Destination content type id.</param>
        /// <returns>true, if the source content type id equals the destination or the source content type id is the child of the destination id.</returns>
        public bool IsBaseBuildinContentTypeMatch(IAveContentTypeId sourceCTId, IAveContentTypeId destinationCTId)
        {
            return IsBaseBuildinContentTypeMatch(sourceCTId, destinationCTId, false);
        }

        /// <summary>
        /// Compare the buildin parents content types of the source and destination content type ids.
        /// </summary>
        /// <param name="sourceCTId">Source content type id.</param>
        /// <param name="destinationCTId">Destination content type id.</param>
        /// <param name="isStrictCompare">true, just use IAveContentTypeId.Equals to compare the 2 content type ids.</param>
        /// <returns>true, if the source parent id equals the destination or the source parent id is the child of the destination.</returns>
        public bool IsBaseBuildinContentTypeMatch(IAveContentTypeId sourceCTId, IAveContentTypeId destinationCTId, bool isStrictCompare)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.IsBaseBuildinContentTypeMatch"))
            {
#endif
                IAveContentTypeId srcId = mObjectModelFactory.CreateContentTypeId(sourceCTId.ToString());
                IAveContentTypeId desId = mObjectModelFactory.CreateContentTypeId(destinationCTId.ToString());
                srcId = GetBaseBuildinContentTypeID(srcId);
                desId = GetBaseBuildinContentTypeID(desId);
                return (srcId == desId || (!isStrictCompare && (srcId.IsChildOf(desId) || desId.IsChildOf(srcId))));
#if PerformanceLog
            }
#endif
        }

        public IAveContentTypeId GetBaseBuildinContentTypeID(IAveContentTypeId ctId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetBaseBuildinContentTypeID"))
            {
#endif
                while (!AveBuiltInContentTypeId.Contains(ctId))
                {
                    ctId = ctId.Parent;
                }
                return ctId;
#if PerformanceLog
            }
#endif
        }

        public IAveContentTypeId GetContentTypeId(string id)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetContentTypeId"))
            {
#endif
                if (id == null)
                {
                    throw new ArgumentException();
                }
                if ((id.Length % 2) != 0)
                {
                    throw new ArgumentException();
                }
                return mObjectModelFactory.CreateContentTypeId(id);
#if PerformanceLog
            }
#endif
        }

        public IAveContentTypeId GetContentTypeIdFromMapping(string id)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetContentTypeIdFromMapping"))
            {
#endif
                //if (null != ContentTypeMapping && ContentTypeMapping.ContainsKey(id))
                //{
                //    return GetContentTypeId(ContentTypeMapping[id]);
                //}
                //else
                //{
                for (int index = mAvaliableContentTypeIdMappings.Count; index > 0; )
                {
                    Dictionary<string, string> mapping = mAvaliableContentTypeIdMappings[--index];
                    if (0 < mapping.Count && mapping.ContainsKey(id))
                    {
                        return GetContentTypeId(mapping[id]);
                    }
                }
                //}
                return null;
#if PerformanceLog
            }
#endif
        }
        //public string GetContentTypeNameFromMapping(string id, string sourceName)
        //{
        //    if (null != ContentTypeNameMapping && ContentTypeNameMapping.ContainsKey(id))
        //    {
        //        if (ContentTypeNameMapping[id].SourceName.Equals(sourceName, StringComparison.OrdinalIgnoreCase))
        //        {
        //            return ContentTypeNameMapping[id].DestName;
        //        }
        //    }
        //    return sourceName;
        //}

        public bool FindContentTypeInCollection(IAveContentTypeCollection collection, Guid siteId, string ctId)
        {
            return collection.CheckContentTypeExist(siteId, ctId);
        }

        public bool FindContentTypeInCollection(IAveContentTypeCollection collection, string scope, Guid siteId, string ctId)
        {
            return collection.CheckIfContentTypeExistInChildren(siteId, scope, ctId);
        }

        public bool FindContentTypeInCollection(IAveContentTypeCollection collection, IAveContentTypeId id, string name, bool needCompareBaseBuildin, ref IAveContentType contentType)
        {
            return FindContentTypeInCollection(collection, id, ref contentType) || FindContentTypeInCollection(collection, name, needCompareBaseBuildin, id, ref contentType);
        }

        public bool FindContentTypeInCollection(IAveContentTypeCollection collection, IAveContentTypeId id, ref IAveContentType contentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.FindContentTypeInCollection"))
            {
#endif
                contentType = collection[id];
                if (null == contentType)
                {
                    return false;
                }
                return true;
#if PerformanceLog
            }
#endif
        }

        public bool FindContentTypeInCollection(IAveContentTypeCollection collection, string name, ref IAveContentType contentType)
        {
            return FindContentTypeInCollection(collection, name, false, null, ref  contentType);
        }

        public bool FindContentTypeInCollection(IAveContentTypeCollection collection, string name, bool needCompareBaseBuildin, IAveContentTypeId sourceId, ref IAveContentType contentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.FindContentTypeInCollection_name"))
            {
#endif
                contentType = collection[name];
                if (null == contentType || (needCompareBaseBuildin && !IsBaseBuildinContentTypeMatch(sourceId, contentType.ID)))
                {
                    return false;
                }
                return true;
#if PerformanceLog
            }
#endif
        }

        public bool FindChildContentTypeInCollection(IAveContentTypeCollection collection, IAveContentType parentContentType, ref IAveContentType contentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.FindChildContentTypeInCollection"))
            {
#endif
                foreach (IAveContentType ct in collection)
                {
                    if (ct.Parent.ID == parentContentType.ID)
                    {
                        contentType = ct;
                        return true;
                    }
                }
                return false;
#if PerformanceLog
            }
#endif
        }

        public bool GetBuildinParentContentType(IAveContentTypeId id, ref IAveContentType contentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetBuildinParentContentType"))
            {
#endif
                return FindContentTypeInCollection(mWeb.AvailableContentTypes, GetBaseBuildinContentTypeID(id), ref contentType);
#if PerformanceLog
            }
#endif
        }

        public bool IsListContentTypeIdExist(string id)
        {
            return this.ListContentTypeIds.Contains(GetContentTypeId(id));
        }

        #endregion

        #region Compare Content type
        public bool CompareContentTypes(AveContentTypeInfo ctInfo, IAveContentType desContentType)
        {
            var compareResult = CompareContentTypesInternal(ctInfo, desContentType);
            if (!compareResult)
            {
                log.Info("compare content type conflict between {0} and {1}", ctInfo.SchemaXml, desContentType.SchemaXml);
            }

            return compareResult;
        }


        private bool CompareContentTypesInternal(AveContentTypeInfo ctInfo, IAveContentType desContentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.CompareContentTypes"))
            {
#endif
                //if (desContentType.Sealed ^ ctInfo.Sealed)
                //{
                //    return false;
                //}
                if (desContentType.ReadOnly ^ ctInfo.ReadOnly)
                {
                    log.Warn("ContentTypes conflict, desContentType.ReadOnly({0}) ^ ctInfo.ReadOnly({1}).", desContentType.ReadOnly, ctInfo.ReadOnly);
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
                    log.Warn("ContentTypes conflict, desContentType.Name : {0} != tempName : {1}", desContentType.Name, tempName);
                    return false;
                }
                //if (!string.Equals(desContentType.Description, ctInfo.Description, StringComparison.OrdinalIgnoreCase))
                //{
                //    return false;
                //}
                if (desContentType.Hidden ^ ctInfo.Hidden)
                {
                    log.Warn("ContentTypes conflict, desContentType.Hidden({0}) ^ ctInfo.Hidden({1}).", desContentType.Hidden, ctInfo.Hidden);
                    return false;
                }

                if (!string.IsNullOrEmpty(ctInfo.NewDocumentControl) && !string.Equals(desContentType.NewDocumentControl, ctInfo.NewDocumentControl, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (ctInfo.RequireClientRenderingOnNew != desContentType.RequireClientRenderingOnNew)
                {
                    return false;
                }

                //由于我们备份的问题，造成这个属性没有备份，需要还原的时候判断一下，如果不一样，就说明改变了。
                if (this.mObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                {
                    if (desContentType.RequireClientRenderingOnNew ^ ctInfo.RequireClientRenderingOnNew)
                    {
                        log.Warn("ContentTypes conflict, desContentType.RequireClientRenderingOnNew({0}) ^ ctInfo.RequireClientRenderingOnNew({1}).", desContentType.RequireClientRenderingOnNew, ctInfo.RequireClientRenderingOnNew);
                        return false;
                    }
                }
                if (!string.IsNullOrEmpty(ctInfo.Group) && !string.Equals(desContentType.Group, ctInfo.Group, StringComparison.OrdinalIgnoreCase))
                {
                    log.Warn("ContentTypes conflict, desContentType.Group : {0} != ctInfo.Group : {1}", desContentType.Group, ctInfo.Group);
                    return false;
                }
                foreach (AveContentTypeFileInfo fileinfo in ctInfo.ResourceFolderFiles)
                {
                    try
                    {
                        fileinfo.Url = desContentType.ResourceFolder.Url + fileinfo.Url.Substring(fileinfo.Url.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
                        IAveFile file = desContentType.ResourceFolder.Files[fileinfo.Url];
                        if (null == file || !file.Exists)
                        {
                            log.Warn("ContentTypes conflict, file : {0} does not exist.", fileinfo.Url);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldError, e.ToString());
                        return false;
                    }
                }
                if (!string.IsNullOrEmpty(ctInfo.DocumentTemplate))
                {
                    if (ctInfo.DocumentTemplate.IndexOf('/') >= 0)
                    {
                        if (!string.Equals(desContentType.DocumentTemplateUrl, ctInfo.DocumentTemplate, StringComparison.OrdinalIgnoreCase))
                        {
                            log.Warn("ContentTypes conflict, desContentType.DocumentTemplateUrl : {0} != ctInfo.DocumentTemplate : {1}", desContentType.DocumentTemplateUrl, ctInfo.DocumentTemplate);
                            return false;
                        }
                    }
                    else
                    {
                        if (!string.Equals(desContentType.DocumentTemplate, ctInfo.DocumentTemplate, StringComparison.OrdinalIgnoreCase))
                        {
                            log.Warn("ContentTypes conflict, desContentType.DocumentTemplate : {0} != ctInfo.DocumentTemplate : {1}", desContentType.DocumentTemplate, ctInfo.DocumentTemplate);
                            return false;
                        }
                    }
                }
                Dictionary<string, string> sourceContentTypeXmlElements = GetXmlDocumentsElements(ctInfo.XmlDocuments);
                Dictionary<string, string> desContentTypeXmlElements = GetXmlDocumentsElements(AveXMLDocumentCollectionToList(desContentType.XmlDocuments));

                if (sourceContentTypeXmlElements.Count != desContentTypeXmlElements.Count)
                {
                    log.Warn("ContentTypes conflict, sourceContentTypeXmlElements.Count : {0} != desContentTypeXmlElements.Count : {1}", sourceContentTypeXmlElements.Count, desContentTypeXmlElements.Count);
                    return false;
                }
                else
                {
                    if (sourceContentTypeXmlElements.ContainsKey("_p:Policy_p:PolicyItems_p:PolicyItem_UniqueId"))
                    {
                        sourceContentTypeXmlElements.Remove("_p:Policy_p:PolicyItems_p:PolicyItem_UniqueId");
                        if (desContentTypeXmlElements.ContainsKey("_p:Policy_p:PolicyItems_p:PolicyItem_UniqueId"))
                        {
                            desContentTypeXmlElements.Remove("_p:Policy_p:PolicyItems_p:PolicyItem_UniqueId");
                        }
                        else
                        {
                            log.Warn("ContentTypes conflict, sourceContentTypeXmlElements.ContainsKey(_p:Policy_p:PolicyItems_p:PolicyItem_UniqueId)");
                            return false;
                        }
                    }

                    bool isConflict = false;
                    List<string> keysNeedsToBeSkipped = new List<string>() { "_p:Policy_p:PolicyItems_p:PolicyItem_UniqueId", 
                                                                             "_act:AllowedContentTypes_LastModified", 
                                                                             "_act:AllowedContentTypes_AllowedContentType_id",
                                                                             "_sf:SharedFields_LastModified",
                                                                             "_sf:SharedFields_SharedField_id",
                                                                             "_wpf:WelcomePageFields_LastModified",
                                                                             "_wpv:WelcomePageView_ViewId",
                                                                             "_SharedContentType_SourceId"};
                    foreach (string key in sourceContentTypeXmlElements.Keys)
                    {
                        try
                        {
                            if (!desContentTypeXmlElements.ContainsKey(key) || !sourceContentTypeXmlElements[key].Equals(desContentTypeXmlElements[key], StringComparison.OrdinalIgnoreCase))
                            {
                                if (keysNeedsToBeSkipped.Contains(key))
                                {
                                    continue;
                                }
                                else
                                {
                                    if (!desContentTypeXmlElements.ContainsKey(key))
                                    {
                                        log.Warn("ContentTypes conflict, desContentTypeXmlElements do not contains key : {0})", key);
                                    }
                                    else
                                    {
                                        log.Warn("ContentTypes conflict, sourceContentTypeXmlElements[key] : {0} != desContentTypeXmlElements[key] : {1}, key : {2}", sourceContentTypeXmlElements[key], desContentTypeXmlElements[key], key);
                                    }
                                    isConflict = true;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, WrapperRestoreResource.SetAttributeFailed, e.ToString());
                            isConflict = true;
                        }
                    }
                    if (isConflict)
                    {
                        return false;
                    }
                }

                #region Compare Field and Field Links
                IAveFieldLinkCollection ctFieldLinks = desContentType.FieldLinks;
                //IAveFieldCollection ctFields = desContentType.Fields;
                XmlDocument doc = new XmlDocument();
                if (!string.IsNullOrEmpty(ctInfo.FieldsSchemaXml))
                {
                    doc.LoadXml(ctInfo.FieldsSchemaXml);
                }
                if (!CompareContentTypeFields(doc.DocumentElement, ctFieldLinks, null, mFieldIdMapping, mFieldInternalNameMapping))
                {
                    return false;
                }
                return true;
                #endregion
#if PerformanceLog
            }
#endif
        }

        internal List<string> AveXMLDocumentCollectionToList(IAveXmlDocumentCollection documents)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.AveXMLDocumentCollectionToList"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }
        internal Dictionary<string, string> GetXmlDocumentsElements(List<string> xmlDocuments)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetXmlDocumentsElements"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }
        internal void AddDocmentElementToDic(XmlNode sub, Dictionary<string, string> XmlDocumentElements, string parentName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.AddDocmentElementToDic"))
            {
#endif
                if (sub.Attributes == null || sub.ChildNodes == null)
                {
                    return;
                }
                if (!XmlDocumentElements.ContainsKey(parentName + "_" + sub.Name))
                {
                    if (sub.OuterXml.Contains("type=") && sub.Attributes.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(sub.InnerXml.Substring(0, sub.InnerXml.IndexOf('<') >= 0 ? sub.InnerXml.IndexOf('<') : sub.InnerXml.Length)))
                        {
                            XmlDocumentElements.Add(parentName + "_" + sub.Name + sub.Attributes[0].Value, sub.InnerXml.Substring(0, sub.InnerXml.IndexOf('<') >= 0 ? sub.InnerXml.IndexOf('<') : sub.InnerXml.Length));
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
#if PerformanceLog
            }
#endif
        }
        internal bool CompareContentTypeFields(XmlElement xmlElement, IAveFieldLinkCollection ctFieldLinks, IAveFieldCollection fieldCollection, Dictionary<Guid, Guid> fieldIdMapping, Dictionary<string, string> fieldInternalNameMapping)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.CompareContentTypeFields"))
            {
#endif
                List<string> sourceFieldList = new List<string>();
                List<string> destinationFieldList = new List<string>();
                bool isConflict = false;
                if (xmlElement != null)
                {
                    foreach (XmlElement xe in xmlElement.ChildNodes)
                    {
                        //string isFromBaseTypeStr = xe.GetAttribute("FromBaseType");
                        //bool isFromBaseType = string.IsNullOrEmpty(isFromBaseTypeStr) ? false : Convert.ToBoolean(isFromBaseTypeStr);
                        string sfieldId = xe.GetAttribute("ID");
                        string sfieldName = xe.GetAttribute("Name");
                        bool hasHiddenAttribute = !string.IsNullOrEmpty(xe.GetAttribute("Hidden"));
                        bool hasRequiredAttribute = !string.IsNullOrEmpty(xe.GetAttribute("Required"));
                        bool hasReadOnlyAttribute = !string.IsNullOrEmpty(xe.GetAttribute("ReadOnly"));
                        //if (isFromBaseType)
                        //{
                        //    sourceFieldList.Add(sfieldName);
                        //    continue;
                        //}
                        IAveFieldLink fieldLink = null;
                        Guid fieldId = Guid.Empty;
                        if (!string.IsNullOrEmpty(sfieldId))
                        {
                            fieldId = new Guid(sfieldId);
                            if (fieldIdMapping != null && fieldIdMapping.ContainsKey(fieldId))
                            {
                                fieldId = fieldIdMapping[fieldId];
                            }
                        }
                        if (!string.IsNullOrEmpty(sfieldName))
                        {
                            if (fieldInternalNameMapping != null && fieldInternalNameMapping.ContainsKey(sfieldName))
                            {
                                sfieldName = fieldInternalNameMapping[sfieldName];
                            }
                        }

                        fieldLink = ctFieldLinks[fieldId];
                        fieldLink = null == fieldLink ? ctFieldLinks[sfieldName] : fieldLink;
                        if (null == fieldLink)
                        {
                            log.Warn("ContentTypeFields conflict, fieldLink is null, fieldId:{0}, fieldName:{1}", fieldId, sfieldName);
                            isConflict = true; // return conflict
                        }

                        //if (hasHiddenAttribute && this.mWeb.Site.APIType != AveAPIType.BPOS_S)
                        //{
                        //    //判断fieldlink是否冲突选择与原端一致的方式，如果xml中有属性就以xml为准，否则要使用field的hidden属性
                        //    bool value = xe.GetAttribute("Hidden").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                        //    if (fieldCollection.Contains(fieldId))
                        //    {
                        //        if (CheckFieldLinkIsHidden(fieldLink, fieldCollection[fieldId]) != value)
                        //        {
                        //            log.Warn("ContentTypeFields conflict, CheckFieldLinkIsHidden failed.");
                        //            isConflict = true;
                        //        }
                        //    }
                        //}
                        ////bpos-s has changed this attribute in backuplistsetting method
                        //if (hasRequiredAttribute && this.mWeb.Site.APIType != AveAPIType.BPOS_S)
                        //{
                        //    bool value = xe.GetAttribute("Required").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                        //    if (fieldCollection.Contains(fieldId))
                        //    {
                        //        if (CheckFieldLinkIsRequired(fieldLink, fieldCollection[fieldId]) != value)
                        //        {
                        //            log.Warn("ContentTypeFields conflict, CheckFieldLinkIsRequired failed.");
                        //            isConflict = true;
                        //        }
                        //    }
                        //}
                        ////bpos-s dones't support this attribute
                        //if (hasReadOnlyAttribute && this.mWeb.Site.APIType != AveAPIType.BPOS_S)
                        //{
                        //    bool value = xe.GetAttribute("ReadOnly").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                        //    if (fieldCollection.Contains(fieldId))
                        //    {
                        //        if (CheckFieldLinkIsReadOnly(fieldLink, fieldCollection[fieldId]) != value)
                        //        {
                        //            log.Warn("ContentTypeFields conflict, CheckFieldLinkIsReadOnly failed.");
                        //            isConflict = true;
                        //        }
                        //    }
                        //}
                        if (fieldLink != null)
                        {
                            sourceFieldList.Add(fieldLink.Name);
                        }
                    }
                }
                //since the source side use fields too
                foreach (IAveFieldLink tmpFL in ctFieldLinks)
                {
                    destinationFieldList.Add(tmpFL.Name);
                }
                if (sourceFieldList.Count == destinationFieldList.Count)
                {
                    //client api doesn't support reorder fieldlinks
                    //if (this.mWeb.Site.APIType != AveAPIType.BPOS_S)
                    //{
                    for (int i = 0; i < sourceFieldList.Count; i++)
                    {
                        if (!string.Equals(sourceFieldList[i], destinationFieldList[i]))
                        {
                            log.Warn("ContentTypeFields conflict, sourceFieldList[i] : {0} != destinationFieldList[i] : {1}", sourceFieldList[i], destinationFieldList[i]);
                            isConflict = true;
                        }
                    }
                    //}
                }
                else
                {
                    log.Warn("ContentTypeFields conflict, sourceFieldList.Count : {0} != destinationFieldList.Count : {1}", sourceFieldList.Count, destinationFieldList.Count);
                    isConflict = true;
                }
                if (isConflict)
                {
                    return false;
                }
                return true;
#if PerformanceLog
            }
#endif
        }
        #endregion

        #region Create content type
        public IAveContentTypePublisher CreateContentTypePublisher()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.CreateContentTypePublisher"))
            {
#endif
                return mObjectModelFactory.CreateContentTypePublisher(mWeb.Site);
#if PerformanceLog
            }
#endif
        }

        public IAveContentType CreateContentType(IAveContentTypeId contentTypeId, IAveContentTypeCollection collection, string name)
        {
            return CreateContentType(contentTypeId, collection, name, false);
        }

        public IAveContentType CreateContentType(IAveContentTypeId contentTypeId, IAveContentTypeCollection collection, string name, bool useBuildinParent)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.CreateContentType"))
            {
#endif
                IAveContentType contentType = null;
                if (useBuildinParent)
                {
                    if (GetBuildinParentContentType(contentTypeId, ref contentType))
                    {
                        contentType = CreateContentType(contentType, collection, name);
                    }
                }
                else
                {
                    contentType = mObjectModelFactory.CreateContentType(contentTypeId, collection, name);
                }
                return contentType;
#if PerformanceLog
            }
#endif
        }

        public IAveContentType CreateContentType(IAveContentType parent, IAveContentTypeCollection collection, string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.CreateContentType_1"))
            {
#endif
                return mObjectModelFactory.CreateContentType(parent, collection, name);
#if PerformanceLog
            }
#endif
        }

        public IAveContentType CreateContentTypeWithoutParent(IAveContentTypeId contentTypeId, IAveContentTypeCollection collection, string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.CreateContentTypeWithoutParent"))
            {
#endif
                if (this.mObjectModelFactory.ContextKind != AveContextKind.ServerObjectModel)
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
#if PerformanceLog
            }
#endif
        }

        public string GetAvaliableWebContentTypeName(AveContentTypeInfo ctInfo, IAveContentTypeCollection desContentTypeCollection, IAveContentTypeCollection destAvailContentTypeCollection, ref IAveContentType desContentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetAvaliableWebContentTypeName"))
            {
#endif
                int extendNum = 0;
                string originalName = ctInfo.Name;
                IAveContentTypeId ctId = GetContentTypeId(ctInfo.Id);
                try
                {
                    while (extendNum++ < 500)
                    {
                        if (FindContentTypeInCollection(desContentTypeCollection, ctInfo.Name, true, ctId, ref desContentType) || FindContentTypeInCollection(destAvailContentTypeCollection, ctInfo.Name, true, ctId, ref desContentType))
                        {
                            if (CompareContentTypes(ctInfo, desContentType))
                            {
                                break;
                            }
                        }
                        else if (desContentType != null)
                        {
                            ctInfo.Name = originalName + "_" + extendNum;
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
                    log.Log(AveLogLevel.WARN, "WP10RTSPCTCol132", ctInfo.Name, e);
                }
                return ctInfo.Name;
#if PerformanceLog
            }
#endif
        }

        public string GetAvaliableContentTypeName(string originalName, IAveContentTypeCollection collection)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetAvaliableContentTypeName"))
            {
#endif
                string name = originalName;
                int extendNum = 0;
                IAveContentType ct = null;
                while (extendNum++ < 500)
                {
                    if (!FindContentTypeInCollection(collection, name, ref ct))
                    {
                        break;
                    }
                    name = originalName + "_" + extendNum;
                }
                return name;
#if PerformanceLog
            }
#endif
        }

        public IAveContentTypeId GetAvaliableContentTypeId(IAveContentTypeId contentTypeId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetAvaliableContentTypeId"))
            {
#endif
                string parentId = contentTypeId.Parent.ToString();
                byte[] rgb = AveContentTypeIdUitlity.HexStringToBytes(parentId);
                string ctId = AveContentTypeIdUitlity.CreateChildFromGuid(rgb, Guid.NewGuid());
                return GetContentTypeId(ctId);
#if PerformanceLog
            }
#endif
        }

        public string GetAvaliableListContentTypeName(AveContentTypeInfo ctInfo, IAveContentTypeCollection desContentTypeCollection, ref IAveContentType desContentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetAvaliableListContentTypeName"))
            {
#endif
                int extendNum = 0;
                string originalName = ctInfo.Name;
                IAveContentTypeId ctId = GetContentTypeId(ctInfo.Id);
                try
                {
                    while (extendNum++ < 500)
                    {
                        if (FindContentTypeInCollection(desContentTypeCollection, ctInfo.Name, true, ctId, ref desContentType))
                        {
                            if (CompareContentTypes(ctInfo, desContentType))
                            {
                                break;
                            }
                        }
                        else if (desContentType != null)
                        {
                            ctInfo.Name = originalName + "_" + extendNum;
                            //break;
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
                    log.Log(AveLogLevel.WARN, "WP10RTSPCTCol132", ctInfo.Name, e);
                }
                return ctInfo.Name;
#if PerformanceLog
            }
#endif
        }
        #endregion

        #region Update content type
        public string UpdateContentType(IAveContentTypeCollection destContentTypeCollection, IAveContentType spCT, AveContentTypeInfo ctInfo, IAveFieldCollection fields, bool isNewCreated, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateContentType"))
            {
#endif
                bool needUpdate = false;
                string exception = string.Empty;
                bool isUpdateChild = restoreOption.WEB_CONTENTTYPE_UPDATECHILD;
                bool isOverWriteFieldLinks = false;
                if (restoreOption.FIELDLINKSOPTION == ContentTypeFieldLinksOption.OverWrite)
                {
                    isOverWriteFieldLinks = true;
                }
                else if (restoreOption.FIELDLINKSOPTION == ContentTypeFieldLinksOption.OverWriteIfNewCreated)
                {
                    isOverWriteFieldLinks = isNewCreated;
                }
                else
                {
                }
                try
                {
                    #region Update content type properties
                    try
                    {
                        if (spCT.ParentList != null && !spCT.ParentList.AllowContentTypes)//Agenda、Attendees、Document Library、Objectives、Meeting Series 这几种list 不支持contenttype
                        {
                            return exception;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.UpdateContentTypeError, e.ToString());
                    }
                    if (spCT.Sealed && !ctInfo.Sealed)
                    {
                        spCT.Sealed = ctInfo.Sealed;
                        needUpdate = true;
                    }
                    else if (spCT.Sealed)
                    {
                        log.Warn("current contentType is sealed, contentType name is :{0}", spCT.Name);
                        return exception;
                    }
                    //if (spCT.Sealed ^ ctInfo.Sealed)
                    //{
                    //    spCT.Sealed = ctInfo.Sealed;
                    //    needUpdate = true;
                    //}


                    if (!string.IsNullOrEmpty(ctInfo.Name) && !ctInfo.Name.StartsWith("$Resources:", StringComparison.OrdinalIgnoreCase) && string.Compare(spCT.Name, ctInfo.Name, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        if (destContentTypeCollection[ctInfo.Name] == null)
                        {
                            spCT.Name = ctInfo.Name;
                            needUpdate = true;
                        }
                    }
                    if (string.Compare(spCT.Description, ctInfo.Description, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        spCT.Description = ctInfo.Description;
                        needUpdate = true;
                    }
                    if (spCT.Hidden != ctInfo.Hidden)
                    {
                        spCT.Hidden = ctInfo.Hidden;
                        needUpdate = true;
                    }
                    if (!string.IsNullOrEmpty(ctInfo.Group) && string.Compare(spCT.Group, ctInfo.Group, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        spCT.Group = ctInfo.Group;
                        needUpdate = true;
                    }

                    //control infopath form library
                    if (!string.IsNullOrEmpty(ctInfo.NewDocumentControl) && string.IsNullOrEmpty(spCT.NewDocumentControl))
                    {
                        spCT.NewDocumentControl = ctInfo.NewDocumentControl;
                        needUpdate = true;
                    }
                    if (ctInfo.RequireClientRenderingOnNew != spCT.RequireClientRenderingOnNew)
                    {
                        spCT.RequireClientRenderingOnNew = ctInfo.RequireClientRenderingOnNew;
                        needUpdate = true;
                    }


                    foreach (AveContentTypeFileInfo fileinfo in ctInfo.ResourceFolderFiles)
                    {
                        try
                        {
                            fileinfo.Url = spCT.ResourceFolder.Url + fileinfo.Url.Substring(fileinfo.Url.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
                            string fileName = fileinfo.Url.Substring(fileinfo.Url.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
                            spCT.ResourceFolder.Files.Add(fileName, fileinfo.FileBinary, true);
                            needUpdate = true;
                        }
                        catch (Exception e)
                        {
                            // TODO:find better way to judge if the fileCollection have the file with the same url

                            string fileUrl = (fileinfo != null && fileinfo.Url != null) ? fileinfo.Url : "file url is Empty";
                            log.Log(AveLogLevel.WARN, WrapperRestoreResource.AddResourceFileToCTFailed, ctInfo.Name, fileUrl, e);
                            exception = e.Message;
                        }
                    }
                    if ((string.Compare(spCT.DocumentTemplate, ctInfo.DocumentTemplate, StringComparison.OrdinalIgnoreCase) != 0) && !string.IsNullOrEmpty(ctInfo.DocumentTemplate))
                    {
                        try
                        {
                            //the old try catch logic handle the uploaded document template case(the url stored in db is different with API),now we move it to the source side,it sticks a princpal:the data from DB should be same as data from API
                            spCT.DocumentTemplate = ctInfo.DocumentTemplate;
                            needUpdate = true;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetContentTypeDocumentTemplateFailed, ctInfo.Id, ctInfo.Name, e);
                            exception = e.Message;
                        }
                    }
                    #endregion
                    try
                    {
                        #region retore xmlDocument

                        #region pre restore xmlDocument
                        //如果两端的xmldocument的节点数一致，则先使之一致（ADO-7542中出现的问题，选项restore正确，但是显示仍不正确）
                        string policyString = "<p:Policy xmlns:p=\"office.server.policy\"";
                        string policyNameUri = "office.server.policy";
                        //这里如果遍历到目的端contenttype的policy属性，则删除。
                        foreach (string tempDestXml in spCT.XmlDocuments)
                        {
                            if (tempDestXml.StartsWith(policyString, StringComparison.OrdinalIgnoreCase))
                            {
                                spCT.XmlDocuments.Delete(policyNameUri);
                                break;
                            }
                        }
                        #endregion
                        int totalNum = 0;
                        for (int i = 0; i < ctInfo.XmlDocuments.Count; ++i)
                        {
                            try
                            {
                                if (ctInfo.XmlDocuments[i].StartsWith(@"<customXsn", StringComparison.OrdinalIgnoreCase))
                                {
                                    ++totalNum;
                                    for (int c = 0; c < spCT.XmlDocuments.Count; ++c)
                                    {
                                        if (spCT.XmlDocuments[c].StartsWith(@"<customXsn", StringComparison.OrdinalIgnoreCase))
                                        {
                                            spCT.XmlDocuments.Delete("http://schemas.microsoft.com/office/2006/metadata/customXsn");
                                            break;
                                        }
                                    }
                                    XmlDocument temDoc = new XmlDocument();
                                    temDoc.LoadXml(ctInfo.XmlDocuments[i]);
                                    if (temDoc.GetElementsByTagName("xsnScope").Count > 0)
                                    {
                                        //keep url format,usually the scope url is absolute
                                        if (AveUrlUtility.IsUrlRelative(temDoc.GetElementsByTagName("xsnScope")[0].InnerText))
                                        {
                                            temDoc.GetElementsByTagName("xsnScope")[0].InnerText = spCT.Scope;
                                        }
                                        else
                                        {
                                            temDoc.GetElementsByTagName("xsnScope")[0].InnerText = AveUrlUtility.CombineUrl(AveUrlUtility.GetServerUrl(destContentTypeCollection.Web.Site.Url), spCT.Scope);
                                        }
                                    }
                                    if (temDoc.GetElementsByTagName("xsnLocation").Count > 0)
                                    {
                                        string documentTemplate = spCT.DocumentTemplate;
                                        if (documentTemplate.IndexOf('/') >= 0)
                                        {
                                            documentTemplate = documentTemplate.Substring(documentTemplate.LastIndexOf('/') + 1);
                                        }
                                        if (mMappingManager != null)//DOC-56939 
                                        {
                                            temDoc.GetElementsByTagName("xsnLocation")[0].InnerText = AveReplaceProcessor.UrlReplace(temDoc.GetElementsByTagName("xsnLocation")[0].InnerText, mMappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true, true), mMappingManager.SiteMappingManager.SourceSiteInfo, mMappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                                        }
                                        else
                                        {
                                            temDoc.GetElementsByTagName("xsnLocation")[0].InnerText = spCT.ParentWeb.Url + "/" + spCT.ResourceFolder.Url + "/" + documentTemplate;
                                        }
                                    }
                                    spCT.XmlDocuments.Add(temDoc);
                                    needUpdate = true;
                                    continue;
                                }
                                else
                                {
                                    XmlDocument temDoc = new XmlDocument();
                                    temDoc.LoadXml(ctInfo.XmlDocuments[i]);
                                    string namespaceUri = temDoc.FirstChild.NamespaceURI;

                                    if (spCT.XmlDocuments[namespaceUri] == null)
                                    {
                                        if (ctInfo.XmlDocuments[i].StartsWith(policyString, StringComparison.OrdinalIgnoreCase))
                                        {
                                            log.Info($"Source contentype:{ctInfo.Name} has information management policy setting, only web service can support to migrate.");
                                            ProcessInformationManagementPolicyInfo(temDoc, spCT.WorkflowAssociations);
                                        }
                                        spCT.XmlDocuments.Add(temDoc);
                                        needUpdate = true;
                                    }
                                    else if (namespaceUri != "" && spCT.XmlDocuments[namespaceUri] != null && !temDoc.FirstChild.OuterXml.Equals(spCT.XmlDocuments[namespaceUri]))
                                    {
                                        spCT.XmlDocuments.Delete(namespaceUri);
                                        if (namespaceUri.Equals("http://schemas.microsoft.com/sharepoint/events", StringComparison.OrdinalIgnoreCase))
                                        {
                                            temDoc.InnerXml = temDoc.InnerXml.Replace("<Assembly>Microsoft.Office.Policy, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c</Assembly>", "<Assembly>Microsoft.Office.Policy, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c</Assembly>");
                                        }
                                        spCT.XmlDocuments.Add(temDoc);
                                        needUpdate = true;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                log.Warn(string.Format("An error occurred while restore contentType xmlDocument.contentType:{0}, xmlDocument:{1}.error:{2}", spCT.Name, ctInfo.XmlDocuments[i].ToString(), e.ToString()));
                                exception = e.Message;
                            }
                        }
                        #endregion
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.RestoreContentTypeXmlDocumentFailed, ctInfo.Id, ctInfo.Name, e);
                        exception = e.Message;
                    }
                    #region Restore field links
                    if (!string.IsNullOrEmpty(ctInfo.FieldsSchemaXml))
                    {

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
                            IAveFieldLink tempLink = RestoreFieldLink(spCT, (XmlElement)xe, ctFieldLinks, fields, mFieldIdMapping, mFieldInternalNameMapping, mEnsureFields, ref needUpdateBeforeReorder);
                            if (tempLink != null)
                            {
                                fieldLinkNames.Add(tempLink.Name);
                                needUpdate = true;
                            }
                        }
                        if (needUpdateBeforeReorder)
                        {
                            spCT.Update(isUpdateChild && spCT.ParentList == null);
                            destContentTypeCollection.Update();
                        }
                        if (isOverWriteFieldLinks)
                        {
                            RemoveDefaultFieldLinks(spCT, ctFieldLinks, fieldLinkNames, difNames);
                        }
                        ReorderFieldLink(ctFieldLinks, fieldLinkNames, difNames);
                        needUpdate = true;
                    }
                    #endregion

                    needUpdate|=UpdateUserResource(mWeb, spCT, ctInfo);

                    if (needUpdate)
                    {
                        if (!spCT.Name.Equals("System") && !spCT.Name.Equals("Folder"))
                        {
                            spCT.Update(isUpdateChild && spCT.ParentList == null);
                        }
                        destContentTypeCollection.Update();
                    }
                    if (spCT.ReadOnly != ctInfo.ReadOnly)
                    {
                        spCT.ReadOnly = ctInfo.ReadOnly;
                        spCT.Update(isUpdateChild);
                        destContentTypeCollection.Update();
                    }
                    CacheFieldShowInView(spCT,ctInfo);
                    //UpdateFieldShowInView(spCT, ctInfo);
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.UpdateContentTypeError, ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("UpdateContentType", "UpdateContentType", AveReportObjectType.UpdateContentType, AveStatus.Skipped, "You don't have permission to UpdateContentType. " + ex.Message));
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
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPCTCol431", spCT.ParentWeb.Url, ctInfo.Id, ctInfo.Name, e);
                    exception = e.Message;
                }
                if (spCT.DocumentTemplate != ctInfo.DocumentTemplate)
                {
                    if (mNeedUpdateDocumentTemplateContentTypes == null)
                    {
                        mNeedUpdateDocumentTemplateContentTypes = new Dictionary<IAveContentTypeId, string>();
                    }
                    mNeedUpdateDocumentTemplateContentTypes.Add(spCT.ID, ctInfo.DocumentTemplate);
                }
                return exception;
#if PerformanceLog
            }
#endif
        }

        private bool ProcessInformationManagementPolicyInfo(XmlDocument sourceXmlDoc, IAveWorkflowAssociationCollection targetWorkflowAssociations)
        {
            string policyString = "<p:Policy xmlns:p=\"office.server.policy\"";
            bool needUpdate = false;
            try
            {
                if (sourceXmlDoc.InnerXml.StartsWith(policyString, StringComparison.OrdinalIgnoreCase))
                {
                    var actionNodes = sourceXmlDoc.SelectNodes("//action");
                    if (actionNodes != null && actionNodes.Count > 0)
                    {
                        foreach (XmlNode actionNode in actionNodes)
                        {
                            if (FilterWorkFlowInPolicy(actionNode) || FilterTransferAnotherLocationInPolicy(actionNode))
                            {
                                log.Info($"Don't support to restore this retention stage, so filter this content type's xml node:{actionNode.OuterXml}");
                                actionNode.ParentNode.ParentNode.RemoveChild(actionNode.ParentNode);
                                needUpdate = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("An error occured when replace content type's information management policy xml, Error:{0}", e);
            }
            return needUpdate;
        }

        /// <summary>
        /// workflowid, XmlNode like this:<action type="workflow" id="288c4aa8-cb7d-4786-9215-2639be821c98_123"/>
        /// </summary>
        /// <param name="actionNode"></param>
        /// <returns></returns>
        private bool FilterWorkFlowInPolicy(XmlNode actionNode)
        {
            if (actionNode == null) return false;
            var attri = actionNode.Attributes["id"];
            Guid guid;
            return attri != null &&
                string.Equals("workflow", actionNode.Attributes["type"] == null ? "" : actionNode.Attributes["type"].Value, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(attri.Value) &&
                Guid.TryParse(attri.Value, out guid);
        }

        /// <summary>
        /// register location XmlNode like this:<action type="action" destnExplanation="Transferred due to organizational policy" destnId="aabe731d-0ce8-4d4f-8698-056c1e8234e9" destnName="ztp2_location_url" destnUrl="https://m365x475714.sharepoint.com/sites/ztp2/_vti_bin/OfficialFile.asmx"/>
        /// </summary>
        /// <param name="actionNode"></param>
        /// <returns></returns>
        private bool FilterTransferAnotherLocationInPolicy(XmlNode actionNode)
        {
            if (actionNode == null) return false;
            Guid guid;
            var attributes = actionNode.Attributes;
            return attributes["destnUrl"] != null &&
                 !string.IsNullOrWhiteSpace(attributes["destnUrl"].Value) &&
                 attributes["destnId"] != null &&
                 !string.IsNullOrWhiteSpace(attributes["destnId"].Value) &&
                  Guid.TryParse(attributes["destnId"].Value, out guid);
        }

        private void CacheFieldShowInView(IAveContentType spCT, AveContentTypeInfo ctInfo)
        {
           
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ctInfo.FieldsSchemaXml);

                int fieldCount = doc.DocumentElement.ChildNodes.Count;

                for (int i = 0; i < fieldCount; i++)
                {
                    XmlNode xe = doc.DocumentElement.ChildNodes[i];
                    if (xe.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }
                    var ele = xe as XmlElement;
                    string IdProp = ele.GetAttribute("ID");
                    if (!string.IsNullOrEmpty(IdProp) && AvePoint.Common.Validator.IsGuid(IdProp))
                    {
                        bool? showInNewForm = null;
                        bool? showInDisplayForm = null;
                        bool? showInEditForm = null;
                        var field = spCT.Fields.GetById(new Guid(IdProp));
                        var editFormprop = ele.GetAttribute("ShowInEditForm");
                        if (!string.IsNullOrEmpty(editFormprop))
                        {
                            var editFormBoolProp = Convert.ToBoolean(editFormprop);
                            if (editFormBoolProp != field.ShowInEditForm)
                            {
                                showInEditForm = editFormBoolProp;

                            }
                        }

                        var newFormprop = ele.GetAttribute("ShowInNewForm");
                        if (!string.IsNullOrEmpty(newFormprop))
                        {
                            var newFormBoolProp = Convert.ToBoolean(newFormprop);
                            if (!string.IsNullOrEmpty(newFormprop)
                                && newFormBoolProp != field.ShowInNewForm)
                            {
                                showInNewForm = newFormBoolProp;
                            }
                        }

                        var displayFormprop = ele.GetAttribute("ShowInDisplayForm");
                        if (!string.IsNullOrEmpty(displayFormprop))
                        {
                            var displayFormBoolProp = Convert.ToBoolean(displayFormprop);
                            if (!string.IsNullOrEmpty(displayFormprop)
                                && displayFormBoolProp != field.ShowInDisplayForm)
                            {
                                showInDisplayForm = displayFormBoolProp;
                            }
                        }
                        var cachedObj = new Tuple<bool?, bool?, bool?>(showInNewForm, showInDisplayForm, showInEditForm);
                       AddToContentTypeFieldShowInFormCache(spCT.ID,field.ID,cachedObj);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Update field show in view properties failed.Error:{0}", e);
            }
           
        }

        private void AddToContentTypeFieldShowInFormCache(IAveContentTypeId ctId,Guid fieldId,Tuple<bool?,bool?,bool?> cachedObj)
        {
            if (ContentTypeFieldShowInFormCache == null)
            {
                ContentTypeFieldShowInFormCache=new Dictionary<IAveContentTypeId, Dictionary<Guid, Tuple<bool?, bool?, bool?>>>();
            }
            if (!ContentTypeFieldShowInFormCache.ContainsKey(ctId))
            {
                ContentTypeFieldShowInFormCache.Add(ctId,new Dictionary<Guid, Tuple<bool?, bool?, bool?>>());
            }
            ContentTypeFieldShowInFormCache[ctId][fieldId] = cachedObj;
        }

        private bool UpdateUserResource(IAveWeb parentWeb,IAveContentType ct, AveContentTypeInfo info)
        {
            log.Debug("Update ContentType user resource for {0}", info.Name);
            bool needUpdate = false;
            if (ct.NameResource.SetUserResource(parentWeb, info.NameResourceInfo, false))
            {
                ct.NameResource.Update();
                needUpdate = true;
            }
            if (ct.DescriptionResource.SetUserResource(parentWeb, info.DescriptionResourceInfo, false))
            {
                ct.DescriptionResource.Update();
                needUpdate = true;
            }
            return needUpdate;
        }

        private IAveFieldLink RestoreFieldLink(IAveContentType spCT, XmlElement fieldXml, IAveFieldLinkCollection ctFieldLinksCollection, IAveFieldCollection aveFields, Dictionary<Guid, Guid> fieldIdMapping, Dictionary<string, string> fieldInternalNameMapping, Dictionary<Guid, string> ensureFields, ref bool needUpdate)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.RestoreFieldLink"))
            {
#endif
                string sfieldId = fieldXml.GetAttribute("ID");
                string sfieldName = fieldXml.GetAttribute("Name");
                bool hasHiddenAttribute = !string.IsNullOrEmpty(fieldXml.GetAttribute("Hidden"));
                bool hasRequiredAttribute = !string.IsNullOrEmpty(fieldXml.GetAttribute("Required"));
                bool hasReadOnlyAttribute = !string.IsNullOrEmpty(fieldXml.GetAttribute("ReadOnly"));
                IAveFieldLink fieldLink = null;

                //if ((!string.IsNullOrEmpty(sfieldName)) && mSourceTextTaxonomyDic != null && mSourceTextTaxonomyDic.ContainsKey(sfieldName))
                //{
                //    return fieldLink;
                //}

                try
                {
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
                    //if (mEnsureFields != null && (mEnsureFields.ContainsKey(fieldId) || mEnsureFields.ContainsValue(sfieldName)))//对于反插的Field，暂时不将其添加到Contenttype中
                    //{
                    //    return null;
                    //}
                    if (fieldLink != null)
                    {
                        //#region MetaData关联的隐藏column以及TaxCatchAll由于一些操作可能在界面显示，所以暂时跳过不处理
                        //if (mSourceTextTaxonomyDic.ContainsKey(sfieldName) || sfieldName.Equals("TaxCatchAll", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    return fieldLink;
                        //}
                        //#endregion
                        //string isFromBaseTypeStr = fieldXml.GetAttribute("FromBaseType");
                        //bool isFromBaseType = string.IsNullOrEmpty(isFromBaseTypeStr) ? false : Convert.ToBoolean(isFromBaseTypeStr);
                        //if (isFromBaseType)
                        //{
                        //    return fieldLink;
                        //}
                        if (hasHiddenAttribute)
                        {
                            //采用跟原端一致的fieldLink的判断逻辑
                            bool value = fieldXml.GetAttribute("Hidden").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                            if (fieldLink.Hidden != value)
                            {
                                fieldLink.Hidden = value;//使用API赋值是正确的
                            }
                        }
                        if (!fieldLink.Hidden && hasRequiredAttribute)
                        {
                            bool value = fieldXml.GetAttribute("Required").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                            if (fieldLink.Required != value)
                            {
                                fieldLink.Required = value;
                            }
                        }
                        //if (hasReadOnlyAttribute)
                        //{
                        //    bool value = fieldXml.GetAttribute("ReadOnly").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                        //    if (fieldLink.ReadOnly != value)
                        //    {
                        //        fieldLink.ReadOnly = value;
                        //    }
                        //}
                        #region we should set "Node" value.(For info path document)
                        //if (fieldXml.HasAttribute("Node") && !string.IsNullOrEmpty(fieldXml.Attributes["Node"].Value))
                        //{
                        //    fieldLink.XPath = fieldXml.Attributes["Node"].Value;
                        //}
                        //if (fieldXml.HasAttribute("Aggregation") && !string.IsNullOrEmpty(fieldXml.Attributes["Aggregation"].Value))
                        //{
                        //    fieldLink.AggregationFunction = fieldXml.Attributes["Aggregation"].Value;
                        //}
                        #endregion
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
                            if (field == null && mList == null && mWeb != null)
                            {
                                field = mWeb.AvailableFields.GetFieldByInternalName(sfieldName, false);
                            }
                            #endregion
                        }
                        if (field == null && mWeb != null && mList != null)
                        {
                            //web list不为空，在list反插逻辑中需要从web上找到ct需要的field来添加到list上，否则还原到目的端之后，目的端list上的ct出现找不到field的错误
                            field = EnsureListField(fieldId, sfieldName, mList);
                        }

                        //sharepoint will add lookup dependent field automaticlly, and bpos will failed to update contenttype if don't skipe this field
                        if (field != null && !AveSPUtility.IsDependentLookupField(field))
                        {
                            fieldLink = mObjectModelFactory.CreateFieldLink(field);
                            if (fieldLink != null && !ctFieldLinksCollection.Any(link => link.ID == fieldLink.ID))
                            {
                                /******************  add this to restore the hidden and required property of fieldlink **********************/
                                if (hasHiddenAttribute)
                                {
                                    //使用原端属性直接赋值即可
                                    fieldLink.Hidden = fieldXml.GetAttribute("Hidden").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                                }
                                if (!fieldLink.Hidden && hasRequiredAttribute)
                                {
                                    fieldLink.Required = fieldXml.GetAttribute("Required").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                                }
                                //if (hasReadOnlyAttribute)
                                //{
                                //    fieldLink.ReadOnly = fieldXml.GetAttribute("ReadOnly").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                                //}

                                //#region we should set "Node" value.(For info path document)
                                //if (fieldXml.HasAttribute("Node") && !string.IsNullOrEmpty(fieldXml.Attributes["Node"].Value))
                                //{
                                //    fieldLink.XPath = fieldXml.Attributes["Node"].Value;
                                //}
                                //if (fieldXml.HasAttribute("Aggregation") && !string.IsNullOrEmpty(fieldXml.Attributes["Aggregation"].Value))
                                //{
                                //    fieldLink.AggregationFunction = fieldXml.Attributes["Aggregation"].Value;
                                //}
                                //if (fieldXml.HasAttribute("DisplayName") && !string.IsNullOrEmpty(fieldXml.Attributes["DisplayName"].Value))
                                //{
                                //    fieldLink.DisplayName = fieldXml.Attributes["DisplayName"].Value;
                                //}
                                //#endregion
                                ctFieldLinksCollection.Add(fieldLink);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.RestoreFieldLinkFailed, sfieldId, sfieldName, e);
                    //mLog.Warn("An error occurred while restore contentType fieldLink, fieldLink Name: {0}", sfieldName);
                }
                return fieldLink;
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// list level 的反插需要确保ct的fieldlink在list上是存在的,web level不起作用
        /// </summary>
        /// <param name="field"></param>
        /// <param name="list"></param>
        private IAveField EnsureListField(Guid fieldId, string fiedlName, IAveList list)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.EnsureListField"))
            {
#endif
                List<Guid> needKeepFieldLinkIds = new List<Guid>() { new Guid("23f27201-bee3-471e-b2e7-b64fd8b7ca38"), new Guid("1390a86a-23da-45f0-8efe-ef36edadfb39"), new Guid("f3b0adf9-c1a2-4b02-920d-943fba4b3611"), new Guid("8f6b6dd8-9357-4019-8172-966fcd502ed2") };
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
                    field = mWeb?.Fields.GetFieldByInternalName(fiedlName, false);
                    #region web level的CT应该从parentWeb上找一下field
                    if (field == null && mList == null && mWeb != null)
                    {
                        field = mWeb.AvailableFields.GetFieldByInternalName(fiedlName, false);
                    }
                    #endregion
                }
                if (field != null && list != null && !list.Fields.Contains(field.ID) && !needKeepFieldLinkIds.Contains(field.ID))
                {
                    list.Fields.Add(field);
                    list.Update();
                }
                return field;
#if PerformanceLog
            }
#endif
        }




        /// <summary>
        /// 判断fieldLink是否是readOnly，如果是fieldLink的xml中有readOnly属性就以xml为主，如果没有属性首先要看field是否是readOnly的，如果是readOnly的话就直接返回true，
        /// 如果field不是readOnly的还要继续检查field的readonlyfield属性，如果是true的话，fieldlink也是readOnly的！
        /// </summary>
        /// <param name="fieldLink"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public bool CheckFieldLinkIsReadOnly(IAveFieldLink fieldLink, IAveField field)
        {
            bool fieldLinkReadOnly = false;
            XmlDocument doc = new XmlDocument();
            try
            {
                if (!string.IsNullOrEmpty(fieldLink.SchemaXml) && field != null)
                {
                    doc.LoadXml(fieldLink.SchemaXml);
                    bool hasReadOnlySchema = doc.DocumentElement.HasAttribute("ReadOnly");
                    if (hasReadOnlySchema)
                    {
                        fieldLinkReadOnly = bool.Parse(doc.DocumentElement.Attributes["ReadOnly"].Value);
                    }
                    else
                    {
                        fieldLinkReadOnly = field.ReadOnlyField;
                    }
                }
                else
                {
                    AvePoint.GCommon.Utility.ArgumentCheck.NotNull(field, nameof(field));
                    fieldLinkReadOnly =  field.ReadOnlyField;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CheckNeedAddFieldLinkError, e.ToString());
                fieldLinkReadOnly = fieldLink.ReadOnly;
            }
            return fieldLinkReadOnly;
        }


        private void RemoveDefaultFieldLinks(IAveContentType spCT, IAveFieldLinkCollection fieldLinkCollection, List<string> needKeepLinks, Hashtable difNames)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.RemoveDefaultFieldLinks"))
            {
#endif
                try
                {
                    //keep it same as the source contenttype
                    //ADO-42425:对于开启Enterprise Keywords,list 默认的content type出现的三个field link: Enterprise Keywords， TaxKeywordTaxHTField，Taxonomy Catch All Column不应该删除
                    //List<Guid> needKeepFieldLinkIds = new List<Guid>() { new Guid("23f27201-bee3-471e-b2e7-b64fd8b7ca38"), new Guid("1390a86a-23da-45f0-8efe-ef36edadfb39"), new Guid("f3b0adf9-c1a2-4b02-920d-943fba4b3611") };
                    //对于custom field mapping changetometadata的column的textfield会被remove 导致sp出错，所以需要保留
                    List<string> removeFieldLink = new List<string>();
                    var fields = spCT.List == null ? spCT.Web.AvailableFields : spCT.List.Fields;
                    // SAAS-13944 添加的时候用的是metadata text field 直接过滤了，所以找不到会抛空引用。
                    //foreach (string fieldLinkName in needKeepLinks)
                    //{
                    //    var fieldName = difNames.Contains(fieldLinkName) ? difNames[fieldLinkName].ToString() : fieldLinkName;
                    //    IAveField taxonomyField = fields.GetField(fieldName);
                    //    if (taxonomyField is IAveTaxonomyField)
                    //    {
                    //        Guid textFieldId = (taxonomyField as IAveTaxonomyField).TextField;
                    //        var textFieldLinkName = fieldLinkCollection[textFieldId].Name;
                    //        needKeepTextFieldLinks.Add(textFieldLinkName);
                    //    }
                    //}
                    foreach (IAveFieldLink fieldLink in fieldLinkCollection)
                    {
                        try
                        {
                            if (!fieldLink.Hidden && !needKeepLinks.Contains(fieldLink.Name))
                            {
                                removeFieldLink.Add(fieldLink.Name);
                            }
                        }
                        catch (ArgumentException)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.AddToRemoveFieldLinkFailed);
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.AddToRemoveFieldLinkFailed, e);
                        }
                        if (!needKeepLinks.Contains(fieldLink.Name))
                        {
                            removeFieldLink.Add(fieldLink.Name);
                        }
                    }
                    foreach (string fieldLink in removeFieldLink)
                    {
                        try
                        {
                            fieldLinkCollection.Delete(fieldLink);
                        }
                        catch (Exception e)
                        {
                            log.Warn(string.Format("An error occurred while delete fieldLink:{0}. error:{1}", fieldLink, e.ToString()));
                        }
                    }
                    //needKeepTextFieldLinks.Clear();
                }
                catch (Exception e)
                {
                    log.Warn(string.Format("An error occurred while RemoveDefaultFieldLinks. error:{0}", e.ToString()));
                }
#if PerformanceLog
            }
#endif
        }

        private void ReorderFieldLink(IAveFieldLinkCollection ctFieldLinksCollection, List<string> originFieldNames, Hashtable names)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.ReorderFieldLink"))
            {
#endif
                string[] orderNames = new string[ctFieldLinksCollection.Count];
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
                    if (!originFieldNames.Contains(fieldLink.Name) && i < orderNames.Length)
                    {
                        orderNames[i++] = fieldLink.Name;
                    }
                }
                ctFieldLinksCollection.Reorder(orderNames);
#if PerformanceLog
            }
#endif
        }

        public static void ActivateFeature(AveSPList list, string featureId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.ActivateFeature"))
            {
#endif
                Guid featrueId = new Guid(featureId);
                if (featrueId == Guid.Empty)
                {
                    return;
                }
                //用它来判断是否需要reloadweb和list，只有开了新的feature才被置为true。
                bool needReload = false;
                try
                {
                    if (list.ParentWeb.SPWeb.Site.Features[featrueId] == null)
                    {
                        list.ParentWeb.SPWeb.Site.Features.Add(featrueId);
                        needReload = true;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ActiateFeatureFail, featrueId, e.ToString());
                    try
                    {
                        if (list.ParentWeb.SPWeb.Features[featrueId] == null)
                        {
                            list.ParentWeb.SPWeb.Features.Add(featrueId);
                            needReload = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.ActivateFeatureFailedEventMessage(featrueId, ex));
                    }
                }
                if (needReload)
                {
                    //ADO-24261,ADO-21304,因为只reload了web，导致在还原list下的文件时，对mlist的修改不能作用到file的parentlist上，所以在reloadweb的同时，reloadlist。
                    list.ParentWeb.SPWeb.ReloadWeb();
                    list.ReloadList();
                }
#if PerformanceLog
            }
#endif
        }

        public static void ActivateFeature(AveSPWeb web, string featureId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.ActivateFeature"))
            {
#endif
                Guid featrueId = new Guid(featureId);
                if (featrueId == Guid.Empty)
                {
                    return;
                }
                //用它来判断是否需要reloadweb和list，只有开了新的feature才被置为true。
                bool needReload = false;
                try
                {
                    if (web.SPWeb.Site.Features[featrueId] == null)
                    {
                        web.SPWeb.Site.Features.Add(featrueId);
                        needReload = true;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ActiateFeatureFail, featrueId, e.ToString());
                    try
                    {
                        if (web.SPWeb.Features[featrueId] == null)
                        {
                            web.SPWeb.Features.Add(featrueId);
                            needReload = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.ActivateFeatureFailedEventMessage(featrueId, ex));
                    }
                }
                if (needReload)
                {
                    web.SPWeb.ReloadWeb();
                }
#if PerformanceLog
            }
#endif
        }
        #endregion

        #region MD5 Property
        public string GetMD5FromXmlDocuments(IAveContentType spCT)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetMD5FromXmlDocuments"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        public void UpdateMD5ToXmlDocuments(IAveContentType spCT)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateMD5ToXmlDocuments"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }
        public string GetCurrentMD5Property(IAveContentType spCT)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.GetCurrentMD5Property"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }
        public string BuildXmlDocumentsMD5(IAveContentType spCT)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.BuildXmlDocumentsMD5"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }
        public string BuildResourceFolderFilesMD5(IAveContentType spCT)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.BuildResourceFolderFilesMD5"))
            {
#endif
                StringBuilder builder = new StringBuilder();
                if (spCT.ResourceFolder != null)
                {
                    foreach (IAveFile file in spCT.ResourceFolder.Files)
                    {
                        builder.Append(file.Url);
                    }
                }
                return builder.ToString();
#if PerformanceLog
            }
#endif
        }
        public string SHA1Hash(string text)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeHelper.SHA1Hash"))
            {
#endif
                if (string.IsNullOrEmpty(text))
                    return string.Empty;
                IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
                byte[] orginaldata = Encoding.Default.GetBytes(text);
                byte[] data = hash.ComputeHash(orginaldata);
                string hashValue = BitConverter.ToString(data);
                hashValue = hashValue.Replace("-", string.Empty);
                return hashValue;
#if PerformanceLog
            }
#endif
        }
        #endregion


        public void UpdateDocumentTemplate(AveSPList list)
        {
            using (new AvePerformanceScope("Restore.AveContentTypeHelper.UpdateDocumentTemplate"))
            {
                if (mNeedUpdateDocumentTemplateContentTypes != null && mNeedUpdateDocumentTemplateContentTypes.Count > 0 && list.SPList != null)
                {
                    list.SPList.Reload();
                    UpdateDocumentTemplate(list.SPList.ContentTypes);
                }
            }
        }
        private void UpdateDocumentTemplate(IAveContentTypeCollection contentTypes)
        {
            foreach (KeyValuePair<IAveContentTypeId, string> pair in mNeedUpdateDocumentTemplateContentTypes)
            {
                IAveContentType desCT = contentTypes[pair.Key];
                if (desCT != null)
                {
                    try
                    {
                        desCT.DocumentTemplate = pair.Value;
                        desCT.Update(false);
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetContentTypeDocumentTemplateFailed, desCT.ID, desCT.Name, ex);
                    }
                }
            }
        }

        public void Dispose()
        {
            if(report != null)
            {
                report.Dispose();
            }
        }
    }

    public enum ContentTypeExistStatus
    {
        None,
        Exist,
        ExistInParent,
        ConflictInChildrenByID
    }

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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeIdUitlity.HexStringFromBytes"))
            {
#endif
                StringBuilder sb = new StringBuilder("0x", 2 + ((rgb != null) ? (rgb.Length * 2) : 0));
                if (rgb != null)
                {
                    foreach (byte num in rgb)
                    {
                        CharsOfByte(num, sb);
                    }
                }
                return sb.ToString();
#if PerformanceLog
            }
#endif
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeIdUitlity.HexStringToBytes"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        internal static string CreateChildFromGuid(byte[] buffer, Guid g)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveContentTypeIdUitlity.CreateChildFromGuid"))
            {
#endif
                byte[] destinationArray = new byte[(buffer.Length + 1) + 0x10];
                if (buffer != null)
                {
                    Array.Copy(buffer, destinationArray, buffer.Length);
                }
                destinationArray[buffer.Length] = 0;
                Array.Copy(g.ToByteArray(), 0, destinationArray, buffer.Length + 1, 0x10);
                return HexStringFromBytes(destinationArray);
#if PerformanceLog
            }
#endif
        }
    }
}