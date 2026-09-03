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
using AvePoint.Wrapper.Resource.Client;

namespace AvePoint.ObjectModel.Common
{
    class AveContentTypeCollection : AveAbstractCommonCollection<IAveContentType>, IAveContentTypeCollection
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveContentTypeCollection));
        private IAveRequest mRequest;
        private AveWeb mParentWeb;
        private AveList mParentList;
        private string mContentTypeSource;
        private bool mIsDirty;
        private Dictionary<string, Guid> ContentTypeFeatureMapping = new Dictionary<string, Guid>();

        private object privateLock = new object();
        private bool mIsCollectionDirty = false;
        internal bool IsCollectionDirty
        {
            get 
            {
                lock (privateLock)
                {
                    return mIsCollectionDirty; 
                }
            }
            set
            {
                lock (privateLock)
                {
                    mIsCollectionDirty = value; 
                }
            }
        }

        public AveContentTypeCollection()
        {
            ContentTypeFeatureMapping["System Page Layout"] = AveSP2010FeatureDefinitions.PublishingSite;
            ContentTypeFeatureMapping["Page Layout"] = AveSP2010FeatureDefinitions.PublishingSite;
            ContentTypeFeatureMapping["System Master Page"] = AveSP2010FeatureDefinitions.PublishingSite;
            ContentTypeFeatureMapping["Publishing Master Page"] = AveSP2010FeatureDefinitions.PublishingSite;
            ContentTypeFeatureMapping["System Page"] = AveSP2010FeatureDefinitions.PublishingSite;
            ContentTypeFeatureMapping["Page"] = AveSP2010FeatureDefinitions.PublishingSite;
        }

        [Obsolete("Use new construction method instead.")]
        public AveContentTypeCollection(IAveRequest request, IAveWeb parentWeb, IAveList parentList, string contentTypeSource, Dictionary<string, object> contentTypesPro)
            : this()
        {
            mRequest = request;
            mParentWeb = parentWeb as AveWeb;
            mParentList = parentList as AveList;
            mContentTypeSource = contentTypeSource;
            base.DataCache.AddPropertyies(contentTypesPro);
            InitContentTypes();
        }

        public AveContentTypeCollection(IAveWeb web, IAveList list, string contentTypeSource)
        {
            lock (privateLock)
            {
                mParentWeb = web as AveWeb;
                mParentList = list as AveList;
                mContentTypeSource = contentTypeSource;
                mRequest = ((AveSite)(mParentWeb.Site)).Request;
                Dictionary<string, object> prop = mRequest.GetContentTypes(mParentWeb.ServerRelativeUrl, mParentList == null ? null : mParentList.Title, mParentList == null ? Guid.Empty : mParentList.ID, mContentTypeSource);
                mListData = new List<IAveContentType>(prop.Count);
                base.DataCache.AddPropertyies(prop);
                InitContentTypes();
            }
        }

        internal void UpdateCollectionInternally()
        {
            lock (privateLock)
            {
                Dictionary<string, object> prop = mRequest.GetContentTypes(mParentWeb.ServerRelativeUrl, mParentList == null ? null : mParentList.Title, mParentList == null ? Guid.Empty : mParentList.ID, mContentTypeSource);
                base.DataCache.RemoveProperty(AveObjectModelConstant.ChildrenProperties);
                mListData.Clear();
                base.DataCache.AddPropertyies(prop);
                InitContentTypes();
                IsCollectionDirty = false;
                IsDirty = false;
            }
        }

        private void InitContentTypes()
        {
            mListData = new List<IAveContentType>();
            var contentTypes = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            HandleContentTypeSchemaXml(contentTypes);
            foreach (Dictionary<string, object> properties in contentTypes)
            {
                AveContentType contentType = new AveContentType(mRequest, this.ParentWeb, this.ParentList, this, mContentTypeSource, properties);
                this.mListData.Add(contentType);
            }
        }
        
        /// <summary>
        /// 处理各个Content type的Schema xml。放到这里处理，防止过多IO操作，提升效率。ADO-159184
        /// </summary>
        /// <param name="contentTypes"></param>
        private void HandleContentTypeSchemaXml(List<Dictionary<string, object>> contentTypes)
        {
            if (contentTypes != null && contentTypes.Count > 0)
            {
                var contentTypeIdSchemaXmlMappings = new Dictionary<string, string>();
                object id;
                object schemaXml;
                foreach (var ct in contentTypes)
                {
                    if (ct.TryGetValue("Id" + AveObjectModelConstant.ObjectPropertySuffix, out id) && ct.TryGetValue("SchemaXml", out schemaXml))
                    {
                        contentTypeIdSchemaXmlMappings[id.ToString()] = schemaXml.ToString();
                    }
                }
                AveClientCacheHandler.WriteSchemaXml(contentTypeIdSchemaXmlMappings, this.ParentWeb.CacheHandlerId, this.ParentWeb.ID.ToString(),
                    this.ParentList == null ? string.Empty : this.ParentList.ID.ToString(), SchemaType.ContentType);
                foreach (var ct in contentTypes)
                {
                    if (ct.ContainsKey("SchemaXml"))
                    {
                        ct.Remove("SchemaXml");
                    }
                }
            }
        }

        internal AveWeb ParentWeb
        {
            get
            {
                return mParentWeb;
            }
        }

        internal AveList ParentList
        {
            get
            {
                return mParentList;
            }
        }

        #region IAveContentType Members
        public IAveContentType this[IAveContentTypeId contentTypeId]
        {
            get
            {
                lock (privateLock)
                {
                    return mListData.Find(
                        delegate(IAveContentType contentType)
                        {
                            return contentType.ID.Equals(contentTypeId);
                        });
                }
            }
        }
        public IAveContentType this[string name]
        {
            get
            {
                lock (privateLock)
                {
                    return mListData.Find(
                                delegate(IAveContentType contentType)
                                {
                                    return contentType.Name.Equals(name);
                                }); 
                }
            }
        }
        public IAveContentType Add(AveContentTypeCreationInformation contentTypeCreationInfo)
        {
            AveContentType nct = new AveContentType(contentTypeCreationInfo.ParentContentType as AveContentType, this, contentTypeCreationInfo.Name);
            return this.Add(nct);
        }

        public IAveContentType Add(IAveContentType contentType)
        {
            AveContentType ct = contentType as AveContentType;
            ct.EnsureContentTypeData();
            string listUrl = mParentList == null ? null : mParentList.DefaultViewUrl;
            string listTitle = mParentList == null ? null : mParentList.Title;
            Guid listId = mParentList == null ? Guid.Empty : mParentList.ID;
            Dictionary<string, object> contentTypeProperties = mRequest.AddContentType(mParentWeb.ServerRelativeUrl, listTitle, listId, mContentTypeSource, ct.DataCache.ChangedProperties);
            AveContentType newContentType = new AveContentType(mRequest, this.ParentWeb, this.ParentList, this, mContentTypeSource, contentTypeProperties);
            newContentType.SiteWeb = mParentWeb;
            newContentType.ContentTypes = this;
            lock (privateLock)
            {
                mListData.Add(newContentType);
            }
            if (mParentWeb != null && string.Equals(mContentTypeSource, "web.contentTypes", StringComparison.OrdinalIgnoreCase) && mParentWeb.DataCache.IsPropertyAvailable("AvailableContentTypes"))
            {
                (mParentWeb.AvailableContentTypes as AveContentTypeCollection).ListData.Add(newContentType);
            }
            if (mParentList != null)
            {
                mParentList.InvalidFields();
            }
            return newContentType;
        }

        public IAveContentType AddExistingContentType(IAveContentType contentType)
        {
            return this.Add(contentType);
        }
        public void AddSitePolicy(string policySchema, string siteUrl)
        {
            (mRequest ).AddSitePolicy(policySchema, siteUrl);
        }
        public IAveContentTypeId BestMatch(IAveContentTypeId contentTypeId)
        {
            IAveContentType contentType = this[contentTypeId];
            if (contentType != null)
            {
                return contentType.ID;
            }
            else
            {
                return AveContentTypeId.BestMatch((AveContentTypeId)contentTypeId, this);
            }
        }
        public IAveContentType GetById(string contentTypeId)
        {
            lock (privateLock)
            {
                return mListData.Find(
                    delegate(IAveContentType contentType)
                    {
                        return contentTypeId.Equals(contentType.ID.ToString());
                    });
            }
        }

        #endregion

        #region IAveContentTypeCollection Members

        private bool IsBelongedFeatureActive(string CTName)
        {
            return !ContentTypeFeatureMapping.ContainsKey(CTName) || this.Web.Site.Features[ContentTypeFeatureMapping[CTName]] != null;
        }

        /// <summary>
        /// 备份时使用的方法，先不用加锁，以后如果有问题再具体看
        /// </summary>
        /// <param name="backupParent"></param>
        /// <returns></returns>
        public AveContentTypeCollectionInfo GetContentTypeInfos(bool backupParent)
        {
            AveContentTypeCollectionInfo contentTypeInfoCollection = new AveContentTypeCollectionInfo();

            foreach (AveContentType ct in base.mListData)
            {
                bool backUpResourceFolder = true;

                AveContentTypeInfo ctInfo = new AveContentTypeInfo();
                ctInfo.Name = ct.Name;
                ctInfo.Id = ct.ID.ToString();
                try
                {
                    if (!IsBelongedFeatureActive(ctInfo.Name) || AveBuiltInContentTypeId.Contains(ctInfo.Id))
                    {
                        continue;
                    }
                    ctInfo.ReadOnly = ct.ReadOnly;
                    ctInfo.Description = ct.Description;
                    //ctInfo.FieldsSchemaXml = ct.Fields.SchemaXml;
                    ctInfo.DocumentTemplate = ct.DocumentTemplate;
                    ctInfo.Group = ct.Group;
                    ctInfo.DisplayFormTemplateName = ct.DisplayFormTemplateName;
                    ctInfo.DisplayFormUrl = ct.DisplayFormUrl;
                    ctInfo.DocumentTemplateUrl = ct.DocumentTemplateUrl;
                    ctInfo.EditFormTemplateName = ct.EditFormTemplateName;
                    ctInfo.EditFormUrl = ct.EditFormUrl;
                    ctInfo.Hidden = ct.Hidden;
                    ctInfo.NewDocumentControl = ct.NewDocumentControl;
                    ctInfo.NewFormTemplateName = ct.NewFormTemplateName;
                    ctInfo.NewFormUrl = ct.NewFormUrl;
                    ctInfo.RequireClientRenderingOnNew = ct.RequireClientRenderingOnNew;
                    ctInfo.NameResource = ct.NameResource.GetUserResourceInfo(this.Web);
                    ctInfo.DescriptionResource = ct.DescriptionResource.GetUserResourceInfo(this.Web);
                    try
                    {
                        ctInfo.ResourceFolder = ct.ResourceFolder != null ? ct.ResourceFolder.Url : null;
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("Get resource folder's url in content type failed in GetContentTypeInformation. Backup will ignore this content type. Content type name:{0}\tId:{1}\n{2}", ctInfo.Name, ctInfo.Id, e.ToString());
                        backUpResourceFolder = false;
                    }
                    ctInfo.SchemaXml = ct.SchemaXml;
                    ctInfo.FieldsSchemaXml = ct.GetFieldLinkSchemaXml();
                    ctInfo.Scope = ct.Scope;
                    //ctInfo.Sealed = ct.Sealed;
                    //ctInfo.Version = ct.Version;
                    ctInfo.ParentName = ct.Parent.Name;
                    if (backupParent)
                    {
                        GetParentContentTypeInfoTree(ctInfo, ct.Parent as AveContentType);
                    }
                    try
                    {
                        if (backUpResourceFolder && (ct.ResourceFolder != null) && WrapperRuntime.CurrentContext.BackupContentTypeDocumentTemplateFile)
                        {
                            foreach (AveFile temFile in ct.ResourceFolder.Files)
                            {
                                ctInfo.ResourceFolderFiles.Add(new AveContentTypeFileInfo(temFile.Url, temFile.OpenBinary(), temFile.Properties, temFile.TimeCreated, temFile.TimeLastModified));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("An error occurred when get files in resource folder in GetContentTypeInformation. Content type name:{0}\tId:{1}\n{2}", ctInfo.Name, ctInfo.Id, ex.ToString());
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
                    contentTypeInfoCollection.ContentTypes.Add(ctInfo);
                }
                catch (Exception exc)
                {
                    mLogger.Warn("An error occurred while backing up content type in GetContentTypeInformation. Name:{0}\tId:{1}\n{2}", ctInfo.Name, ctInfo.Id, exc.ToString());
                }
            }

            return contentTypeInfoCollection;
        }

        public List<AveContentTypeFileInfo> GetResources(Guid siteId, string folderUrl)
        {
            return null;
        }

        public string GetContentTypeName(Guid siteId, byte[] contentTypeId)
        {
            string id = Encoding.UTF8.GetString(contentTypeId);
            return this[new AveContentTypeId(id)].Name;//contentTypeId)].Name;
        }

        public List<byte[]> GetParentContentTypeIdList(string id)
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

        //备份list的contentType的时候，需要备份contentType的整个继承关系，让restore端在找不着parentContentType的时候可以可以创建出来。
        private void GetParentContentTypeInfoTree(AveContentTypeInfo contentTypeInfo, AveContentType ct)
        {

            if (!AveBuiltInContentTypeId.Contains(ct.ID))
            {
                bool backUpResourceFolder = true;

                AveContentTypeInfo ctInfo = new AveContentTypeInfo();
                ctInfo.Name = ct.Name;
                ctInfo.Id = ct.ID.ToString();
                ctInfo.ReadOnly = ct.ReadOnly;
                ctInfo.Description = ct.Description;
                //ctInfo.FieldsSchemaXml = ct.Fields.SchemaXml;
                ctInfo.FieldsSchemaXml = ct.GetFieldLinkSchemaXml();
                ctInfo.DocumentTemplate = ct.DocumentTemplate;
                ctInfo.Group = ct.Group;
                ctInfo.DisplayFormTemplateName = ct.DisplayFormTemplateName;
                ctInfo.DisplayFormUrl = ct.DisplayFormUrl;
                ctInfo.DocumentTemplateUrl = ct.DocumentTemplateUrl;
                ctInfo.EditFormTemplateName = ct.EditFormTemplateName;
                ctInfo.EditFormUrl = ct.EditFormUrl;
                ctInfo.Hidden = ct.Hidden;
                ctInfo.NewDocumentControl = ct.NewDocumentControl;
                ctInfo.NewFormTemplateName = ct.NewFormTemplateName;
                ctInfo.NewFormUrl = ct.NewFormUrl;
                ctInfo.RequireClientRenderingOnNew = ct.RequireClientRenderingOnNew;
                ctInfo.NameResource = ct.NameResource.GetUserResourceInfo(this.Web);
                ctInfo.DescriptionResource = ct.DescriptionResource.GetUserResourceInfo(this.Web);

                try
                {
                    ctInfo.ResourceFolder = ct.ResourceFolder != null ? ct.ResourceFolder.Url : null;
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Get content type:{0} resource folder failed.Error Message:{1}.", ct.Name, ex.ToString());
                    backUpResourceFolder = false;
                }
                ctInfo.SchemaXml = ct.SchemaXml;
                ctInfo.Scope = ct.Scope;
                ctInfo.Sealed = ct.Sealed;
                //ctInfo.Version = ct.Version;
                ctInfo.ParentName = ct.Parent.Name;

                try
                {
                    if (backUpResourceFolder && ct.ResourceFolder != null && ct.ResourceFolder.Exists)
                    {
                        foreach (AveFile temFile in ct.ResourceFolder.Files)
                        {
                            ctInfo.ResourceFolderFiles.Add(new AveContentTypeFileInfo(temFile.Url, temFile.OpenBinary(), temFile.Properties, temFile.TimeCreated, temFile.TimeLastModified));
                        }
                    }
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveObjectModel_CommonResource.GetParentContentTypeInfoTreeError, ctInfo.Name, this.List != null ? this.List.Title : string.Empty, this.Web != null ? this.Web.Url : string.Empty, e.ToString());
                    
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
                GetParentContentTypeInfoTree(ctInfo, ct.Parent as AveContentType);
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
        #endregion


        public IAveWeb Web
        {
            get
            {
                return mParentWeb;
            }
        }

        public IAveList List
        {
            get
            {
                return mParentList;
            }
        }

        public AveContentTypeCollectionInfo GetContentTypeInfos(Guid listId, Guid webId, Guid siteId, string scope, bool backupParent)
        {
            return GetContentTypeInfos(backupParent);
        }

        public AveContentTypeCollectionInfo GetContentTypeInfos(Guid siteId, string scope, bool backupParent)
        {
            return GetContentTypeInfos(backupParent);
        }

        public bool CheckContentTypeExist(Guid siteId, string ctId)
        {
            return this[new AveContentTypeId(ctId)] != null;
        }

        public void Update()
        {

        }

        public System.Collections.Hashtable DictId
        {
            get { return null; }
        }


        public AveContentTypeCollectionInfo GetContentTypeInfos(List<string> names, Guid siteId, string scope, bool backupParent)
        {
            throw new NotImplementedException();
        }

        public IAveContentType AddContentType(IAveContentType contentType, bool updateResourceFileProperty, bool checkName, bool setNextChildByte)
        {
            throw new NotImplementedException();
        }


        public bool CheckIfContentTypeExistInChildren(Guid siteId, string scope, string ctId)
        {
            return this[new AveContentTypeId(ctId)] != null;
        }


        public bool IsDirty
        {
            get { return mIsDirty; }
            set { mIsDirty = value; }
        }
    }
}
