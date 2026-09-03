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
using System.Xml;
using System.Globalization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using AvePoint.Wrapper.Common;
using System.Collections;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server16
{
    class AveContentTypeCollection : AveAbstractCommonCollection<IAveContentType>, IAveContentTypeCollection
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveContentTypeCollection));

        private SPContentTypeCollection mContentTypes;
        private AveWeb mWeb;
        private AveList mList;
        private AveSite mSite;
        private bool mIsDirty;
        private bool mIsDirtySetted;

        private static readonly Dictionary<string, Guid> ContentTypeFeatureMapping = new Dictionary<string, Guid>();

        static AveContentTypeCollection()
        {
            ContentTypeFeatureMapping["System Page Layout"] = AveSP2010FeatureDefinitions.PublishingSite;
            ContentTypeFeatureMapping["Page Layout"] = AveSP2010FeatureDefinitions.PublishingSite;
            ContentTypeFeatureMapping["System Master Page"] = AveSP2010FeatureDefinitions.PublishingSite;
            ContentTypeFeatureMapping["Publishing Master Page"] = AveSP2010FeatureDefinitions.PublishingSite;
            ContentTypeFeatureMapping["System Page"] = AveSP2010FeatureDefinitions.PublishingSite;
            ContentTypeFeatureMapping["Page"] = AveSP2010FeatureDefinitions.PublishingSite;
        }

        public AveContentTypeCollection(AveWeb web, SPContentTypeCollection contentTypeCollection)
            : base(contentTypeCollection)
        {
            mWeb = web;
            mSite = web.Site as AveSite;
            mContentTypes = contentTypeCollection;
        }

        public AveContentTypeCollection(AveList list, SPContentTypeCollection contentTypeCollection)
            : base(contentTypeCollection)
        {
            mList = list;
            mWeb = list.ParentWeb as AveWeb;
            mContentTypes = contentTypeCollection;
            mSite = list.ParentWeb.Site as AveSite;
        }

        internal SPContentTypeCollection ContentTypeCollection
        {
            get
            {
                return mContentTypes;
            }
        }

        #region IAveContentTypeCollection Members

        public IAveContentType Add(AveContentTypeCreationInformation parameters)
        {
            SPContentType contentType = new SPContentType((parameters.ParentContentType as AveContentType).ContentType, mContentTypes, parameters.Name);
            if (!string.IsNullOrEmpty(parameters.Group))
            {
                contentType.Group = parameters.Group;
            }
            if (!string.IsNullOrEmpty(parameters.Description))
            {
                contentType.Description = parameters.Description;
            }
            return new AveContentType(this, mContentTypes.Add(contentType));
        }

        public IAveContentType AddExistingContentType(IAveContentType contentType)
        {
            return new AveContentType(this, mContentTypes.Add((contentType as AveContentType).ContentType));
        }

        public IAveContentType GetById(string contentTypeId)
        {
            foreach (SPContentType spContentType in mContentTypes)
            {
                if (string.Equals(spContentType.Id.ToString(), contentTypeId))
                {
                    return new AveContentType(this, spContentType);
                }
            }
            return null;
        }

        public bool IsDirty
        {
            get
            {
                if (!mIsDirtySetted)
                {
                    mIsDirty = (bool)AveAssemblyUtility.GetPropertyValue(mContentTypes, "IsDirty");
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

        public IAveContentType this[string name]
        {
            get
            {
                SPContentType contentType = mContentTypes[name];
                if (contentType == null)
                {
                    return null;
                }
                return new AveContentType(this, contentType);
            }
        }

        public override IAveContentType this[int index]
        {
            get
            {
                SPContentType contentType = mContentTypes[index];
                if (contentType == null)
                {
                    return null;
                }
                return new AveContentType(this, contentType);
            }
        }

        public IAveContentType this[IAveContentTypeId contentTypeId]
        {
            get
            {
                SPContentType contentType = mContentTypes[(contentTypeId as AveContentTypeId).ContentTypeId];
                if (contentType == null)
                {
                    return null;
                }
                return new AveContentType(this, contentType);
            }
        }

        public IAveContentType Add(IAveContentType contentType)
        {
            SPContentType type  = mContentTypes.Add((contentType as AveContentType).ContentType);
            if (type == null)
            {
                return null;
            }
            return new AveContentType(this, type);
        }

        public IAveContentType AddContentType(IAveContentType contentType, bool updateResourceFileProperty, bool checkName, bool setNextChildByte)
        {
            SPContentType type = null;
            type = (SPContentType)AveAssemblyUtility.InvokeMethod(mContentTypes, "AddContentType", new Type[] { typeof(SPContentType), typeof(bool), typeof(bool), typeof(bool) }, new object[] { (contentType as AveContentType).ContentType, updateResourceFileProperty, checkName, setNextChildByte });

            if (null == type)
            {
                return null;
            }
            return new AveContentType(this, type);
        }

        public IAveContentTypeId BestMatch(IAveContentTypeId contentTypeId)
        {
            return new AveContentTypeId(mContentTypes.BestMatch((contentTypeId as AveContentTypeId).ContentTypeId));
        }

        #endregion

        //备份list的contentType的时候，需要备份contentType的整个继承关系，让restore端在找不着parentContentType的时候可以可以创建出来。
        private void GetParentContentTypeInfoTree(AveContentTypeInfo contentTypeInfo, SPContentType ct)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveContentTypeCollection.GetParentContentTypeInfoTree"))
            {

                if (!AveBuiltInContentTypeId.Contains(ct.Id.ToString()))
                {
                    bool backUpResourceFolder = true;

                    AveContentTypeInfo ctInfo = new AveContentTypeInfo();
                    ctInfo.Name = ct.Name;
                    ctInfo.Id = ct.Id.ToString();
                    ctInfo.ReadOnly = ct.ReadOnly;
                    ctInfo.Description = ct.Description;
                    ctInfo.FieldsSchemaXml = ct.Fields.SchemaXml;
                    ctInfo.DocumentTemplate = ct.DocumentTemplate;
                    ctInfo.Group = ct.Group;
                    ctInfo.DisplayFormTemplateName = ct.DisplayFormTemplateName;
                    ctInfo.DisplayFormUrl = ct.DisplayFormUrl;
                    ctInfo.DocumentTemplateUrl = ct.DocumentTemplateUrl;
                    ctInfo.EditFormTemplateName = ct.EditFormTemplateName;
                    ctInfo.EditFormUrl = ct.EditFormUrl;
                    ctInfo.Hidden = ct.Hidden;
                    //ctInfo.NewDocumentControl = ct.NewDocumentControl;
                    ctInfo.NewFormTemplateName = ct.NewFormTemplateName;
                    ctInfo.NewFormUrl = ct.NewFormUrl;
                    //ctInfo.RequireClientRenderingOnNew = ct.RequireClientRenderingOnNew;

                    try
                    {
                        ctInfo.ResourceFolder = ct.ResourceFolder != null ? ct.ResourceFolder.Url : null;
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetCTResourceFolderError, e.ToString());
                        backUpResourceFolder = false;
                    }
                    ctInfo.SchemaXml = ct.SchemaXml;
                    ctInfo.Scope = ct.Scope;
                    ctInfo.Sealed = ct.Sealed;
                    //ctInfo.Version = ct.Version;
                    ctInfo.ParentName = ct.Parent.Name;

                    try
                    {
                        if (backUpResourceFolder)
                        {
                            foreach (SPFile temFile in ct.ResourceFolder.Files)
                            {
                                ctInfo.ResourceFolderFiles.Add(new AveContentTypeFileInfo(temFile.Url, temFile.OpenBinary(), temFile.Properties, temFile.TimeCreated, temFile.TimeLastModified));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Get the resource folder filled by content type: {0} and name: {1}, exception: {2}", ctInfo.Id, ctInfo.Name, e.ToString());
                    }

                    foreach (string str in ct.XmlDocuments)
                    {
                        ctInfo.XmlDocuments.Add(str);
                    }
                    //if (AveSPDocumentSet.IsDocumentSet(ctInfo))
                    //{
                    //    AveSPDocumentSet ctDocumentSet = null;
                    //    if (mCTScope == ContentTypeScope.Web)
                    //    {
                    //        ctDocumentSet = new AveSPDocumentSet(ctInfo, mAveSPWeb);
                    //    }
                    //    else if (mCTScope == ContentTypeScope.List)
                    //    {
                    //        ctDocumentSet = new AveSPDocumentSet(ctInfo, mAveSPList);
                    //    }

                    //    ctDocumentSet.ReplaceXmlDocuments();
                    //}
                    contentTypeInfo.ParentContentTypeInfo = ctInfo;
                    GetParentContentTypeInfoTree(ctInfo, ct.Parent);
                }

            }

        }

        public List<AveContentTypeFileInfo> GetResources(Guid siteId, string folderUrl)
        {
            return mSite.QueryService.GetContentTypeCollectionResources(siteId, folderUrl);
        }

        public string GetContentTypeName(Guid siteId, byte[] contentTypeId)
        {
            return mSite.QueryService.GetContentTypeName(siteId, contentTypeId);
        }

        public List<byte[]> GetParentContentTypeIdList(string id)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveContentTypeCollection.GetParentContentTypeIdList"))
            {

                List<byte[]> parentIdList = new List<byte[]>();

                byte[] contentTypeId = ConvertHexStringToBytes(id);
                int i = contentTypeId.Length - 1;
                while (i > 0)
                {
                    byte[] temp = null;
                    if (i >= 16 && contentTypeId[i - 16] == 0)
                    {
                        temp = new byte[i - 16];
                        Array.Copy(contentTypeId, temp, i - 16);
                        parentIdList.Add(temp);
                        i = i - 17;
                    }
                    else
                    {
                        temp = new byte[i];
                        Array.Copy(contentTypeId, temp, i);
                        parentIdList.Add(temp);
                        i = i - 1;
                    }
                    if (AveBuiltInContentTypeId.Contains(temp))
                    {
                        break;
                    }
                }
                return parentIdList;

            }

        }

        public byte[] ConvertHexStringToBytes(string hexString)
        {
            if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && hexString.Length % 2 == 0)
            {
                byte[] bts = new byte[(hexString.Length - 2) / 2];
                for (int i = 2; i < hexString.Length; i = i + 2)
                {
                    bts[i / 2 - 1] = Convert.ToByte(hexString.Substring(i, 2), 16);
                }
                return bts;
            }
            return null;
        }

        public AveContentTypeCollectionInfo GetContentTypeInfos(bool backupParent)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveContentTypeCollection.GetContentTypeInfos"))
            {

                AveContentTypeCollectionInfo infos = new AveContentTypeCollectionInfo();
                foreach (SPContentType spCT in mContentTypes)
                {
                    AveContentTypeInfo ctInfo = new AveContentTypeInfo();
                    try
                    {

                        ctInfo = (new AveContentType(this, spCT)).GetContentTypeInfo(backupParent);
                        infos.ContentTypes.Add(ctInfo);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Get content type information by id: {0} and name: {1}, exception: {2}", ctInfo.Id, ctInfo.Name, e.ToString());
                    }
                }

                return infos;

            }

        }

        protected override object CreatElementInstance(object t)
        {
            return new AveContentType(this, t as SPContentType);
        }

        public override int Count
        {
            get { return mContentTypes.Count; }
        }

        public IAveWeb Web
        {
            get { return mWeb; }
        }

        public IAveList List
        {
            get { return mList; }
        }

        #region IAveContentTypeCollection Members

        public AveContentTypeCollectionInfo GetContentTypeInfos(Guid listId, Guid webId, Guid siteId, string scope, bool backupParent)
        {

            AveContentTypeCollectionInfo infos = new AveContentTypeCollectionInfo();
            if (scope.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                scope = scope.Substring(1);
            }
            try
            {
                string contentTypesContent = mSite.QueryService.GetContentTypeSchema(mSite.ID, listId, webId);
                if (contentTypesContent != null)
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.InnerXml = "<ContentTypes>" + contentTypesContent + "</ContentTypes>";
                    for (int i = 0; i < xDoc.ChildNodes[0].ChildNodes.Count; i++)
                    {
                        try
                        {
                            XmlElement xe = (XmlElement)xDoc.ChildNodes[0].ChildNodes[i];

                            AveContentTypeInfo ctInfo = new AveContentTypeInfo();
                            string id = xe.Attributes["ID"].Value;
                            if (AveBuiltInContentTypeId.Contains(id))
                            {
                                continue;
                            }

                            //系统自带的Content Type的Element是ContentTypeRef，再目的端的ID会不一样，所以也需要做个备份，为了添加到Mapping中
                            if (xe.Name.Equals("ContentTypeRef", StringComparison.OrdinalIgnoreCase))
                            {
                                IAveContentType tmpCT = this[new AveContentTypeId(id)];
                                if (null != tmpCT)
                                {
                                    ctInfo = tmpCT.GetContentTypeInfo(backupParent);
                                }
                                else if (xe["Folder"] != null)
                                {
                                    ctInfo.Name = xe["Folder"].GetAttribute("TargetName");
                                    ctInfo.Id = id;
                                }
                                else
                                {
                                    continue;
                                }

                            }
                            else if (!xe.HasAttribute("Name"))
                            {
                                logger.Debug("The content type schema: {0} of list: {1} not have the name attribute.", xe.OuterXml, mList.DefaultViewUrl);
                                //don't backup contenttype which doesn't have a name attribute
                                continue;
                            }
                            else
                            {
                                ctInfo.Name = xe.Attributes["Name"].Value;
                                if (ctInfo.Name.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                {
                                    ctInfo.Name = SPUtility.GetLocalizedString(ctInfo.Name, "core", (uint)mWeb.UICulture.LCID);
                                }
                                ctInfo.Id = xe.Attributes["ID"].Value;
                                ctInfo.ReadOnly = xe.HasAttribute("ReadOnly") && xe.Attributes["ReadOnly"].Value == "TRUE";
                                ctInfo.Description = xe.HasAttribute("Description") ? xe.Attributes["Description"].Value : "";
                                if (ctInfo.Description.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                {
                                    ctInfo.Description = SPUtility.GetLocalizedString(ctInfo.Description, "core", (uint)mWeb.UICulture.LCID);
                                }
                                ctInfo.ResourceFolder = xe["Folder"] != null ? xe["Folder"].Attributes["TargetName"].Value : null;
                                IAveContentType tmpCT = this[new AveContentTypeId(id)];
                                if (tmpCT != null)
                                {
                                    ctInfo.FieldsSchemaXml = tmpCT.GetFieldLinkSchemaXml();                                    
                                    ctInfo.DocumentTemplate = tmpCT.DocumentTemplate;
                                    ctInfo.DocumentTemplateUrl = tmpCT.DocumentTemplateUrl;
                                }
                                else
                                {
                                    string fieldRefs = xe["FieldRefs"] != null ? xe["FieldRefs"].InnerXml : "";
                                    fieldRefs = "<Fields>" + fieldRefs + "</Fields>";
                                    ctInfo.FieldsSchemaXml = fieldRefs;
                                }
                                if (xe.Attributes["Group"] != null)
                                {
                                    ctInfo.Group = xe.Attributes["Group"].Value;
                                    if (ctInfo.Group.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                    {
                                        ctInfo.Group = SPUtility.GetLocalizedString(ctInfo.Group, "core", (uint)mWeb.UICulture.LCID);
                                    }
                                }
                                ctInfo.Hidden = xe.HasAttribute("Hidden") && xe.Attributes["Hidden"].Value == "TRUE";
                                ctInfo.SchemaXml = xe.OuterXml;
                                if (xe.HasAttribute("RequireClientRenderingOnNew") && "false".Equals(xe.GetAttribute("RequireClientRenderingOnNew"), StringComparison.OrdinalIgnoreCase))
                                {
                                    ctInfo.RequireClientRenderingOnNew = false;
                                }
                                else
                                {
                                    ctInfo.RequireClientRenderingOnNew = true;
                                }

                                if (xe.HasAttribute("NewDocumentControl"))
                                {
                                    ctInfo.NewDocumentControl = xe.GetAttribute("NewDocumentControl");
                                }

                                if (xe.HasAttribute("FeatureId"))
                                {
                                    ctInfo.SolutionId = xe.GetAttribute("FeatureId");
                                }

                                if (xe.HasAttribute("Sealed") && "true".Equals(xe.GetAttribute("Sealed"), StringComparison.OrdinalIgnoreCase))
                                {
                                    ctInfo.Sealed = true;
                                }
                                

                                if (xe["XmlDocuments"] != null)
                                {
                                    foreach (XmlNode node in xe["XmlDocuments"].ChildNodes)
                                    {
                                        if (node.InnerXml.StartsWith("<", StringComparison.OrdinalIgnoreCase))
                                        {
                                            ctInfo.XmlDocuments.Add(node.InnerXml);
                                        }
                                        else
                                        {
                                            string temp = AveCompressedUtility.GetStringFromBase64String(node.InnerText);
                                            ctInfo.XmlDocuments.Add(temp);
                                        }
                                    }
                                }
                            }
                            HandleDocumentSetXmlDocuments(ctInfo, false);
                            infos.ContentTypes.Add(ctInfo);
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Analyze the content type xml: {0} failed: {1}", contentTypesContent, e.ToString());
                        }
                    }
                    List<byte[]> parentIdList = null;
                    foreach (AveContentTypeInfo info in infos.ContentTypes)
                    {
                        try
                        {
                            if (info.ResourceFolder != null && WrapperRuntime.CurrentContext.BackupContentTypeDocumentTemplateFile)
                            {
                                string folderUrl = "/" + scope + "/" + info.ResourceFolder;
                                var resourceFolder = Web.GetFolder(folderUrl);
                                foreach (AveFile temFile in resourceFolder.Files)
                                {
                                    info.ResourceFolderFiles.Add(new AveContentTypeFileInfo(temFile.Url, temFile.OpenBinary(), temFile.Properties, temFile.TimeCreated, temFile.TimeLastModified));
                                }
                                //info.ResourceFolderFiles = mSite.QueryService.GetContentTypeCollectionResources(siteId, folderUrl);
                            }
                            parentIdList = GetParentContentTypeIdList(info.Id);
                            byte[] ctId;
                            while (parentIdList.Count > 0)
                            {
                                ctId = parentIdList[0];
                                info.ParentName = GetContentTypeName(siteId, ctId);
                                //AvailableContentTypes不存在此CT，则继续找parent,模拟SPCotentType.Parent内部实现。目前只发现ListCotentType有此问题。
                                if (info.ParentName == null)
                                {
                                    parentIdList.Remove(ctId);
                                }
                                else
                                {
                                    if (info.ParentName.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                    {
                                        info.ParentName = SPUtility.GetLocalizedString(info.ParentName, "core", (uint)mWeb.UICulture.LCID);
                                    }
                                    break;
                                }
                            }
                            if (backupParent && parentIdList.Count > 0)
                            {
                                GetParentContentTypeInfoTree(info, siteId, parentIdList);
                                HandleContentTypeParentInfo(info);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Get the parent content type of {0} failed: {1}", info.Name, e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Get the content type of list: {0} with scope: {1} failed: {2}.", listId, scope, e.ToString());
            }
            return infos;

            //return AveDBQueryService.GetContentTypeInfos(listId, webId, siteId, scope, backupParent, this);
        }

        private void HandleDocumentSetXmlDocuments(AveContentTypeInfo ctInfo,bool needClear)
        {
            if (ctInfo.Id != null && AveSPDocumentSet.IsDocumentSet(new AveContentTypeId(ctInfo.Id)))
            {
                AveSPDocumentSet ctDocumentSet = null;
                if (mList != null)
                {
                    ctDocumentSet = new AveSPDocumentSet(ctInfo, mList);

                }
                else if (mWeb != null)
                {
                    ctDocumentSet = new AveSPDocumentSet(ctInfo, mWeb);
                }
                if (ctDocumentSet != null)
                {
                    ctDocumentSet.ReplaceXmlDocuments();
                }
            }
            else if(needClear)
            {
                ctInfo.XmlDocuments.Clear();
            }
        }

        private void HandleContentTypeParentInfo(AveContentTypeInfo ctInfo)
        {
            if(ctInfo.ParentContentTypeInfo!= null)
            {
                HandleDocumentSetXmlDocuments(ctInfo.ParentContentTypeInfo, false);
                HandleContentTypeParentInfo(ctInfo.ParentContentTypeInfo);
            }
        }
        

        private bool IsBelongedFeatureActive(string name)
        {
            return !ContentTypeFeatureMapping.ContainsKey(name) || mSite.Features[ContentTypeFeatureMapping[name]] != null;
        }

        public AveContentTypeCollectionInfo GetContentTypeInfos(List<string> names, Guid siteId, string scope, bool backupParent)
        {
            AveContentTypeCollectionInfo infos = new AveContentTypeCollectionInfo();
            try
            {
                infos = mSite.QueryService.GetContentTypeInfos(siteId, scope);
                List<byte[]> parentIdList = null;
                List<AveContentTypeInfo> filtedContentTypes = new List<AveContentTypeInfo>();
                foreach (AveContentTypeInfo info in infos.ContentTypes)
                {
                    try
                    {
                        if (!names.Contains(info.Name))
                        {
                            filtedContentTypes.Add(info);
                            continue;
                        }
                        // skip when belonged feature is not active in site
                        if (!IsBelongedFeatureActive(info.Name))
                        {
                            filtedContentTypes.Add(info);
                        }
                        HandleDocumentSetXmlDocuments(info, false);
                        if (info.ResourceFolder != null)
                        {
                            string folderUrl = scope + "/" + info.ResourceFolder;
                            info.ResourceFolderFiles = mSite.QueryService.GetContentTypeCollectionResources(siteId, folderUrl);
                        }
                        parentIdList = GetParentContentTypeIdList(info.Id);
                        if (parentIdList.Count > 0)
                        {
                            info.ParentName = mSite.QueryService.GetContentTypeName(siteId, parentIdList[0]);
                        }
                        if (backupParent)
                        {
                            GetParentContentTypeInfoTree(info, siteId, parentIdList);
                        }
                        if (info.Name.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                        {
                            info.Name = SPUtility.GetLocalizedString(info.Name, "core", (uint)mWeb.UICulture.LCID);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Get content type information with scope: {0} failed: {1}", scope, e.ToString());
                    }
                }
                foreach (AveContentTypeInfo skipedContentTypeInfo in filtedContentTypes)
                {
                    infos.ContentTypes.Remove(skipedContentTypeInfo);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Get content type information with scope: {0} failed: {1}", scope, e.ToString());
            }
            return infos;
        }

        public AveContentTypeCollectionInfo GetContentTypeInfos(Guid siteId, string scope, bool backupParent)
        {
            AveContentTypeCollectionInfo infos = new AveContentTypeCollectionInfo();
            try
            {
                infos = mSite.QueryService.GetContentTypeInfos(siteId, scope);
                List<byte[]> parentIdList = null;
                List<AveContentTypeInfo> filtedContentTypes = new List<AveContentTypeInfo>();
                foreach (AveContentTypeInfo info in infos.ContentTypes)
                {
                    try
                    {
                        // skip when belonged feature is not active in site
                        if (!IsBelongedFeatureActive(info.Name))
                        {
                            filtedContentTypes.Add(info);
                        }
                        using (var tmpCT = this[new AveContentTypeId(info.Id)] as AveContentType)
                        {
                            if (tmpCT != null)
                            {
                                info.FieldsSchemaXml = tmpCT.GetFieldLinkSchemaXml();
                            }
                        }
                        HandleDocumentSetXmlDocuments(info, false);
                        if (info.ResourceFolder != null && WrapperRuntime.CurrentContext.BackupContentTypeDocumentTemplateFile)
                        {
                            string folderUrl = scope + "/" + info.ResourceFolder;
                            info.ResourceFolderFiles = mSite.QueryService.GetContentTypeCollectionResources(siteId, folderUrl);
                        }
                        parentIdList = GetParentContentTypeIdList(info.Id);
                        if (parentIdList.Count > 0)
                        {
                            info.ParentName = mSite.QueryService.GetContentTypeName(siteId, parentIdList[0]);
                        }
                        if (backupParent)
                        {
                            GetParentContentTypeInfoTree(info, siteId, parentIdList);
                            HandleContentTypeParentInfo(info);
                        }
                        if (info.Name.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                        {
                            info.Name = SPUtility.GetLocalizedString(info.Name, "core", (uint)mWeb.UICulture.LCID);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Get content type information with scope: {0} failed: {1}", scope, e.ToString());
                    }
                }
                foreach (AveContentTypeInfo skipedContentTypeInfo in filtedContentTypes)
                {
                    infos.ContentTypes.Remove(skipedContentTypeInfo);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Get content type information with scope: {0} failed: {1}", scope, e.ToString());
            }
            return infos;
        }

        public void GetParentContentTypeInfoTree(AveContentTypeInfo contentTypeInfo, Guid siteId, List<byte[]> parentIdList)
        {
            mSite.QueryService.GetParentContentTypeInfoTree(contentTypeInfo, siteId, parentIdList);
        }

        public void GetParentContentTypeInfoTree(AveContentTypeInfo contentTypeInfo, IAveContentType ct)
        {
            GetParentContentTypeInfoTree(contentTypeInfo, ((AveContentType)ct).ContentType);
        }

        public bool CheckContentTypeExist(Guid siteId, string ctId)
        {
            return mSite.QueryService.CheckContentTypeExist(siteId, ConvertHexStringToBytes(ctId));
        }

        public bool CheckIfContentTypeExistInChildren(Guid siteId, string scope, string ctId)
        {
            return mSite.QueryService.CheckIfContentTypeExistInChildren(siteId, scope, ConvertHexStringToBytes(ctId));
        }

        public void Update()
        {
            AveAssemblyUtility.InvokeMethod(mContentTypes, mContentTypes.GetType(), "Update", new object[] { });
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")] 
        public Hashtable DictId
        {
            get
            {
                Hashtable spcts = AveAssemblyUtility.GetFieldValue(this.ContentTypeCollection, "m_dictId") as Hashtable;
                Hashtable cts = new Hashtable();
                foreach (SPContentTypeId id in spcts.Keys)
                {
                    SPContentType contentType = spcts[id] as SPContentType;
                    if (contentType != null)
                    {
                        cts.Add(new AveContentTypeId(id), new AveContentType(this, contentType));
                    }
                    else
                    {
                        cts.Add(new AveContentTypeId(id), null);
                    }
                }
                return cts;
            }
        }

        #endregion


        public List<string> ContentTypeNames
        {
            get;
            set;
        }

        public new IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void AddSitePolicy(string policySchema, string siteUrl)
        {
            throw new NotImplementedException();
        }
    }
}