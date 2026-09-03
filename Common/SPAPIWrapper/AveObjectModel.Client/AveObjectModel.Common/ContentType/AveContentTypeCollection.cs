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
        private Dictionary<string, Guid> ContentTypeFeatureMapping = new Dictionary<string, Guid>();

        public AveContentTypeCollection()
        {
            Guid publishingFeatureId = new Guid("f6924d36-2fa8-4f0b-b16d-06b7250180fa");
            ContentTypeFeatureMapping["System Page Layout"] = publishingFeatureId;
            ContentTypeFeatureMapping["Page Layout"] = publishingFeatureId;
            ContentTypeFeatureMapping["System Master Page"] = publishingFeatureId;
            ContentTypeFeatureMapping["Publishing Master Page"] = publishingFeatureId;
            ContentTypeFeatureMapping["System Page"] = publishingFeatureId;
            ContentTypeFeatureMapping["Page"] = publishingFeatureId;
        }

        public AveContentTypeCollection(IAveRequest request, IAveWeb parentWeb, IAveList parentList, string contentTypeSource, Dictionary<string, object> contentTypesPro)
            : this()
        {
            mRequest = request;
            mParentWeb = parentWeb as AveWeb;
            mParentList = parentList as AveList;
            mContentTypeSource = contentTypeSource;
            base.DataCache.AddPropertyies(contentTypesPro);
            //将write schemaXml的逻辑也放在该方法里面进行，这样减少IO操作，一次load就可以将文件写完
            InitContentTypes(); 
        }

        private void InitContentTypes()
        {
            string listId = mParentList == null ? string.Empty : mParentList.ID.ToString();
            AveClientCacheHandler.WriteSchemaXml(InitContentTypeAndGetProperties(), this.mParentWeb.CacheHandlerId, mParentWeb.ID.ToString(), listId, SchemaType.ContentType);
        }

        private IEnumerable<KeyValuePair<string, string>> InitContentTypeAndGetProperties()
        {
            mListData = new List<IAveContentType>();
            var contentTypePropertiesList = base.DataCache.GetChildren();
            foreach (var properties in contentTypePropertiesList)
            {
                object idObject;
                object schemaXmlObject = null;
                if (properties.TryGetValue("Id" + AveObjectModelConstant.ObjectPropertySuffix, out idObject) && properties.TryGetValue("SchemaXml", out schemaXmlObject))
                {
                    string id = idObject.ToString();
                    string schemaXml = (string)schemaXmlObject;
                    yield return new KeyValuePair<string, string>(id, schemaXml);
                    properties.Remove("SchemaXml");
                    //初始化ContentType
                    AveContentType contentType = new AveContentType(mRequest, this.ParentWeb, this.ParentList, this, mContentTypeSource, properties, schemaXml);
                    mListData.Add(contentType);
                }
                else
                {
                    if (schemaXmlObject == null)
                    {
                        AveContentType contentType = new AveContentType(mRequest, this.ParentWeb, this.ParentList, this, mContentTypeSource, properties);
                        mListData.Add(contentType);
                    }
                    else
                    {
                        throw new Exception(string.Format("The content type doesn't have valid id:{0}", schemaXmlObject));
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
                return mListData.Find(
                    delegate(IAveContentType contentType)
                    {
                        return contentType.ID.Equals(contentTypeId);
                    });
            }
        }
        public IAveContentType this[string name]
        {
            get
            {
                return mListData.Find(
                    delegate(IAveContentType contentType)
                    {
                        return contentType.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
                    });
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
            mListData.Add(newContentType);
            if (string.Equals(mContentTypeSource, "web.contentTypes")
                && mParentWeb != null && mParentWeb.AvailableContentTypes[newContentType.Name] == null)
            {
                AveContentType avaliableContentType = new AveContentType(mRequest, this.ParentWeb, this.ParentList, this, "web.availableContentTypes", contentTypeProperties);
                (mParentWeb.AvailableContentTypes as AveContentTypeCollection).mListData.Add(avaliableContentType);
            }
            return newContentType;
        }
        public IAveContentType AddExistingContentType(IAveContentType contentType)
        {
            return this.Add(contentType);
        }
        public void AddSitePolicy(string policySchema, string siteUrl)
        {
            mRequest.AddSitePolicy(policySchema, siteUrl);
            if (mContentTypeSource == "web.contentTypes")
            {
                mParentWeb.DataCache.RemoveProperty("ContentTypes");
            }
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
            return mListData.Find(
                delegate(IAveContentType contentType)
                {
                    return contentTypeId.Equals(contentType.ID.ToString());
                });
        }

        #endregion

        #region IAveContentTypeCollection Members

        private bool IsBelongedFeatureActive(string CTName)
        {
            return !ContentTypeFeatureMapping.ContainsKey(CTName) || this.Web.Site.Features[ContentTypeFeatureMapping[CTName]] != null;
        }

        public AveContentTypeCollectionInfo GetContentTypeInfos(bool backupParent)
        {
            AveContentTypeCollectionInfo contentTypeInfoCollection = new AveContentTypeCollectionInfo();

            Dictionary<string, List<string>> resourceFilesIndex = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            //先load出本地文件
            string listId = mParentList == null ? string.Empty : mParentList.ID.ToString();
            var contentTypeMap = AveClientCacheHandler.GetSchemaXmlMapping(this.mParentWeb.CacheHandlerId, mParentWeb.ID.ToString(), listId, SchemaType.ContentType);
            foreach (AveContentType ct in base.mListData)
            {
                AveContentTypeInfo ctInfo = new AveContentTypeInfo();
                ctInfo.Name = ct.Name;
                ctInfo.Id = ct.ID.ToString();
                try
                {
                    if (!IsBelongedFeatureActive(ctInfo.Name))
                    {
                        continue;
                    }
                    ctInfo.ReadOnly = ct.ReadOnly;
                    ctInfo.Description = ct.Description;
                    ctInfo.NameResourceInfo
                   = ct.DataCache.GetProperty<Dictionary<string, string>>(AveUserResourceConstants.NAME_RESOUCE);
                    ctInfo.DescriptionResourceInfo
                        = ct.DataCache.GetProperty<Dictionary<string, string>>(AveUserResourceConstants.DESCRIPTION_RESOUCE);

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

                    //每循环一次就得load一次本地文件，修改为从hashtable中获取schemaXml
                    //ctInfo.SchemaXml = ct.SchemaXml;
                    ctInfo.SchemaXml = contentTypeMap[ct.ID.ToString()];

                    ct.ExtractResourceFolderTargetName(ctInfo.SchemaXml);
                    ctInfo.ResourceFolder = ct.ResourceFolderUrl;//ct.ResourceFolder != null ? ct.ResourceFolder.Url : null;

//#if DEBUG
//                    var url = ct.ResourceFolder != null ? ct.ResourceFolder.Url : null;

//                    if(string.Compare(url, ctInfo.ResourceFolder, StringComparison.OrdinalIgnoreCase) != 0)
//                    {
//                        throw new Exception("Url exception:" + url + " --> " + ctInfo.ResourceFolder);
//                    }
//#endif
                    
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(ctInfo.SchemaXml);
                    XmlNode fields = doc.DocumentElement.GetElementsByTagName("Fields")[0];
                    if (fields != null)
                    {
                        ctInfo.FieldsSchemaXml = fields.OuterXml;
                    }
                    ctInfo.Scope = ct.Scope;
                    //ctInfo.Sealed = ct.Sealed;
                    //ctInfo.Version = ct.Version;
                    var parentContentType = ct.Parent;

                    if (parentContentType != null)
                    {
                        ctInfo.ParentName = parentContentType.Name;

                        if (backupParent)
                        {
                            GetParentContentTypeInfoTree(ctInfo, parentContentType as AveContentType, resourceFilesIndex);
                        }
                    }

                    LoadCTResourceFiles(ct, ctInfo, resourceFilesIndex);

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
                    // mLog.Log(AveLogLevel.WARN, "WP10BKAveSPCT379", ctInfo.Id, ctInfo.Name, e);
                    mLogger.Warn("An error occurred while backing up content type in GetContentTypeInformation. Name:{0}, Id:{1}, HandlerId:{2}, WebId:{3}, ListId:{4}\n{5}", 
                        ctInfo.Name, ctInfo.Id, mParentWeb.CacheHandlerId, mParentWeb.ID.ToString(), listId, exc.ToString());
                }
            }

            return contentTypeInfoCollection;
        }

        private void LoadCTResourceFiles(AveContentType ct, AveContentTypeInfo ctInfo, Dictionary<string, List<string>> resourceFilesIndex)
        {
            try
            {
                var serverRelativeUrl = ct.ResourceFolderServerRelativeUrl;

                if (!string.IsNullOrEmpty(serverRelativeUrl))
                {
                    List<string> files;

                    var webServerRelativeUrl = ct.ParentWeb.ServerRelativeUrl;

                    if (ct.ParentList == null)
                    {
                        webServerRelativeUrl = ct.Scope;
                    }

                    if (serverRelativeUrl.StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!resourceFilesIndex.TryGetValue(serverRelativeUrl, out files))
                        {
                            files = mRequest.GetContentTypeResourceFiles(webServerRelativeUrl, serverRelativeUrl, resourceFilesIndex);
                        }

                        if (files != null)
                        {
                            foreach (var file in files)
                            {
                                ctInfo.ResourceFolderFiles.Add(new AveContentTypeFileInfo(file.Substring(webServerRelativeUrl.Length).TrimStart('/'),
                                    mRequest.GetFileBinary(webServerRelativeUrl, file, (int)AveOpenBinaryOptions.None, Guid.Empty)));
                            }
                        }
                    }
                    else
                    {
                        mLogger.Error("!!!Content type name:{0}, Id:{1}, ResourceFolder Url:{2}, WebServerRelativeUrl:{3}", ctInfo.Name, ctInfo.Id, serverRelativeUrl, webServerRelativeUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                // mLog.Log(AveLogLevel.WARN, "WP10BKAveSPCT351", ctInfo.Id, ctInfo.Name, e);
                mLogger.Warn("An error occurred when get files in resource folder in GetContentTypeInformation. Content type name:{0}\tId:{1}\n{2}", ctInfo.Name, ctInfo.Id, ex.ToString());
            }
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
        private void GetParentContentTypeInfoTree(AveContentTypeInfo contentTypeInfo, AveContentType ct, Dictionary<string, List<string>> resourceFilesIndex)
        {

            if (!AveBuiltInContentTypeId.Contains(ct.ID))
            {
                AveContentTypeInfo ctInfo = new AveContentTypeInfo();
                ctInfo.Name = ct.Name;
                ctInfo.Id = ct.ID.ToString();
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
                //ctInfo.NewDocumentControl = ct.NewDocumentControl;
                ctInfo.NewFormTemplateName = ct.NewFormTemplateName;
                ctInfo.NewFormUrl = ct.NewFormUrl;
                //ctInfo.RequireClientRenderingOnNew = ct.RequireClientRenderingOnNew;

                ctInfo.SchemaXml = ct.SchemaXml;

                ct.ExtractResourceFolderTargetName(ctInfo.SchemaXml);
                ctInfo.ResourceFolder = ct.ResourceFolderUrl;
                //ctInfo.ResourceFolder = ct.ResourceFolder != null ? ct.ResourceFolder.Url : null;
                
                ctInfo.Scope = ct.Scope;
                ctInfo.Sealed = ct.Sealed;
                //ctInfo.Version = ct.Version;
                ctInfo.ParentName = ct.Parent.Name;
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ctInfo.SchemaXml);
                XmlNode fields = doc.DocumentElement.GetElementsByTagName("Fields")[0];
                if (fields != null)
                {
                    ctInfo.FieldsSchemaXml = fields.OuterXml;
                }

                LoadCTResourceFiles(ct, ctInfo, resourceFilesIndex);

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
                GetParentContentTypeInfoTree(ctInfo, ct.Parent as AveContentType, resourceFilesIndex);
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
            get { return false; }
            set { }
        }
    }
}
