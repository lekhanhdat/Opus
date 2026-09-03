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
using System.Net;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Xml;
using System.Xml.Linq;

namespace AvePoint.ObjectModel.Common
{
    class AveContentType : AveClientObject, IAveContentType
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveContentType));
        private AveSite mSite;
        private AveWeb mWeb;
        private AveContentTypeCollection mContentTypes;
        private IAveRequest mRequest;
        private string mContentTypeSource;
        private string[] mFieldLinksOrder;
        private string mMD5;
        private Dictionary<Guid, XmlElement> mContentTypeFieldLinkSchema;
        private AveFieldCollection mFields;
        private AveFieldLinkCollection mFieldLinks;
        private string mNewDocumentControl;
        private bool mRequireClientRenderingOnNew;
        private readonly object mFieldsLock = new object();
        private readonly object mFieldLinksLock = new object();

        public AveContentType(IAveRequest request, IAveWeb parentWeb, IAveList parentList, AveContentTypeCollection contentTypes, string contentTypeSource, IDictionary<string, object> contentTypeProp)
        {
            mRequest = request;
            mContentTypes = contentTypes;
            mContentTypeSource = contentTypeSource;
            mWeb = parentWeb as AveWeb;
            contentTypeProp["ParentWeb"] = parentWeb;
            contentTypeProp["ParentList"] = parentList;
            InitSchemaXml(ref contentTypeProp, parentList);
            base.DataCache.AddPropertyies(contentTypeProp);
        }


        /// <summary>
        /// 为了减少IO请求，把初始化schema xml的动作统一管理。所以这里需要对从schemaxml中读取的属性做一次初始化操作。
        /// </summary>
        /// <param name="request"></param>
        /// <param name="parentWeb"></param>
        /// <param name="parentList"></param>
        /// <param name="contentTypes"></param>
        /// <param name="contentTypeSource"></param>
        /// <param name="contentTypeProp"></param>
        /// <param name="schema"></param>
        public AveContentType(IAveRequest request, IAveWeb parentWeb, IAveList parentList, AveContentTypeCollection contentTypes, string contentTypeSource, IDictionary<string, object> contentTypeProp, string schema)
            :this(request, parentWeb, parentList, contentTypes, contentTypeSource, contentTypeProp)
        {
            InitSchemaXmlWithoutCache(schema);
        }

        public AveContentType(IAveContentType parentContentType, IAveContentTypeCollection contentTypeCol, string name)
        {
            mContentTypes = contentTypeCol as AveContentTypeCollection;
            base.DataCache.AddChangedProperty("IsNew", true);
            base.DataCache.AddChangedProperty("HasParentContentType", true);
            base.DataCache.AddChangedProperty("Name", name);
            base.DataCache.AddChangedProperty("ParentContentId", (parentContentType as AveContentType).ID.ToString());
            base.DataCache.AddProperty("Parent",parentContentType);
        }

        public AveContentType(AveContentTypeId contentTypeId, AveContentTypeCollection contentTypeCol, string name)
        {
            AveContentTypeId tempContentTypeId = new AveContentTypeId();
            tempContentTypeId = contentTypeId;
            mContentTypes = contentTypeCol;
            mWeb = contentTypeCol.ParentWeb;
            base.DataCache.AddChangedProperty("IsNew", true);
            base.DataCache.AddChangedProperty("Name", name);
                base.DataCache.AddChangedProperty("ContentTypeId", contentTypeId.ToString());
            base.DataCache.AddChangedProperty("ParentContentId", contentTypeId.Parent.ToString());

            var parent = contentTypeId.Parent;
            IAveContentType parentContentType;
            while(true)
            {
                parentContentType = mWeb.AvailableContentTypes[parent];

                if (parentContentType != null || AveBuiltInContentTypeId.Contains(parent))
                {
                    break;
                }

                parent = parent.Parent;
            }

            DataCache.AddProperty("Parent", parentContentType);
            DataCache.AddProperty("ContentTypeId", contentTypeId);
        }

        private void GetNewDocCtrlAndReqClientRenOnNew(string schema) 
        {
            XmlDocument schemaDoc = new XmlDocument();
            schemaDoc.LoadXml(schema);
            if (schemaDoc.DocumentElement.HasAttribute("NewDocumentControl"))
            {
                mNewDocumentControl = schemaDoc.DocumentElement.GetAttribute("NewDocumentControl");
            }
            if (schemaDoc.DocumentElement.HasAttribute("RequireClientRenderingOnNew"))
            {
                mRequireClientRenderingOnNew = Convert.ToBoolean(schemaDoc.DocumentElement.GetAttribute("RequireClientRenderingOnNew"));
            }
            else
            {
                mRequireClientRenderingOnNew = true;
            }
        }

        private void InitSchemaXmlWithoutCache(string schemaXml)
        {
            GetNewDocCtrlAndReqClientRenOnNew(schemaXml);

            Dictionary<string, string> xmlDocumentsProperties = AveXmlDocumentCollection.GetXmlDocumentDataFromSchemalXml(schemaXml);
            AveXmlDocumentCollection xmlDocuments = new AveXmlDocumentCollection(this, mRequest, xmlDocumentsProperties);
            base.DataCache.AddProperty("XmlDocuments",xmlDocuments);
        }

        private void InitSchemaXml(ref IDictionary<string, object> contentTypeProperty, IAveList parentList)
        {
            if (contentTypeProperty.ContainsKey("SchemaXml"))
            {
                string contentTypeId = Guid.Empty.ToString();
                string listId = parentList == null ? string.Empty : parentList.ID.ToString();
                if (contentTypeProperty.ContainsKey("Id" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    contentTypeId = contentTypeProperty["Id" + AveObjectModelConstant.ObjectPropertySuffix].ToString();
                }
                string schemaXml = contentTypeProperty["SchemaXml"].ToString();
                GetNewDocCtrlAndReqClientRenOnNew(schemaXml);
                AveClientCacheHandler.WriteSchemaXml(contentTypeProperty["SchemaXml"].ToString(), mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, contentTypeId, SchemaType.ContentType);
                contentTypeProperty.Remove("SchemaXml");
            }
        }

        internal void EnsureContentTypeData()
        {
            Dictionary<string, object> parentContentTypeProperties = (this.Parent as AveContentType).Location;
            base.DataCache.AddChangedProperty("ParentContentType" + AveObjectModelConstant.ObjectPropertySuffix, parentContentTypeProperties);
            if (!base.DataCache.GetProperty<bool>("IsNew"))
            {
                base.DataCache.AddChangedProperties(this.Location);
            }
        }

        internal Dictionary<string, object> Location
        {
            get
            {
                Dictionary<string, object> ctLocatioin = new Dictionary<string, object>();
                ctLocatioin[AveObjectModelConstant.WebServerRelativeUrl] = ParentWeb.ServerRelativeUrl;
                ctLocatioin[AveObjectModelConstant.ListTitle] = ParentList == null ? null : ParentList.Title;
                ctLocatioin[AveObjectModelConstant.ListId] = ParentList == null ? Guid.Empty : ParentList.ID;
                ctLocatioin["ContentTypeSource"] = mContentTypeSource;
                ctLocatioin["ContentTypeId"] = this.ID.ToString();
                return ctLocatioin;
            }
        }

        internal string[] fieldLinksOrder
        {
            set
            {
                mFieldLinksOrder = value;
            }
        }

        internal AveContentTypeCollection ContentTypes
        {
            set
            {
                mContentTypes = value;
            }
        }

        internal AveWeb SiteWeb
        {
            set
            {
                mWeb = value;
                mSite = mWeb.Site as AveSite;
                mRequest = mSite.Request;
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
            set
            {
                base.DataCache.AddChangedProperty("Name", value);
            }
        }
        public IAveContentTypeId ID
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Id"))
                {
                    AveContentTypeId Id = new AveContentTypeId(base.DataCache.GetProperty<string>("Id" + AveObjectModelConstant.ObjectPropertySuffix));
                    base.DataCache.AddProperty("Id",Id);
                    return Id;
                }
                return base.DataCache.GetProperty<IAveContentTypeId>("Id");
            }
        }
        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
            set
            {
                base.DataCache.AddChangedProperty("Description", value);
            }
        }
        public string Group
        {
            get
            {
                return base.DataCache.GetProperty<string>("Group");
            }
            set
            {
                base.DataCache.AddChangedProperty("Group", value);
            }
        }
        public string Scope
        {
            get
            {
                return base.DataCache.GetProperty<string>("Scope");
            }
        }
        public string DisplayFormTemplateName
        {
            get
            {
                return base.DataCache.GetProperty<string>("DisplayFormTemplateName");
            }
            set
            {
                base.DataCache.AddChangedProperty("DisplayFormTemplateName", value);
            }
        }
        public string DisplayFormUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("DisplayFormUrl");
            }
            set
            {
                base.DataCache.AddChangedProperty("DisplayFormUrl", value);
            }
        }
        public string DocumentTemplate
        {
            get
            {
                return base.DataCache.GetProperty<string>("DocumentTemplate");
            }
            set
            {
                base.DataCache.AddChangedProperty("DocumentTemplate", value);
            }
        }
        public string DocumentTemplateUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("DocumentTemplateUrl");
            }
        }
        public string EditFormTemplateName
        {
            get
            {
                return base.DataCache.GetProperty<string>("EditFormTemplateName");
            }
            set
            {
                base.DataCache.AddChangedProperty("EditFormTemplateName", value);
            }
        }
        public string EditFormUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("EditFormUrl");
            }
            set
            {
                base.DataCache.AddChangedProperty("EditFormUrl", value);
            }
        }
        public IAveFieldLinkCollection FieldLinks
        {
            get
            {
                if (mFieldLinks == null)
                {
                    lock (mFieldLinksLock)
                    {
                        if (mFieldLinks == null)
                        {
                            InitCTPropsBySchemaXml();
                            if (mFieldLinks == null)
                            {
                                string listUrl = mContentTypes.ParentList == null ? null : mContentTypes.ParentList.DefaultViewUrl;
                                string listTitle = mContentTypes.ParentList == null ? null : mContentTypes.ParentList.Title;
                                Guid listId = mContentTypes.ParentList == null ? Guid.Empty : mContentTypes.ParentList.ID;
                                Dictionary<string, object> fieldLinksProperties = mRequest.GetFieldLinks(mContentTypes.ParentWeb.ServerRelativeUrl, listUrl, listTitle, listId, this.ID.ToString(), mContentTypeSource);
                                mFieldLinks = new AveFieldLinkCollection(mRequest, this, fieldLinksProperties);                                
                            }
                        }
                    }
                }
                return mFieldLinks;
            }
        }

        public IAveUserResource NameResource
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded(AveContentTypeUserResourceConstants.Name_Resource))
                {
                    var titleResource = new AveContentTypeUserResource(mRequest, mWeb.ServerRelativeUrl, this.ParentList, mContentTypeSource,
                        AveUserResourceConstants.NAME_RESOUCE, ID.ToString(), base.DataCache);
                    base.DataCache.AddProperty(AveContentTypeUserResourceConstants.Name_Resource,titleResource);
                    return titleResource;
                }
                return base.DataCache.GetProperty<AveContentTypeUserResource>(AveContentTypeUserResourceConstants.Name_Resource);
            }
        }

        public IAveUserResource DescriptionResource
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded(AveContentTypeUserResourceConstants.Description_Resource))
                {
                    var titleResource = new AveContentTypeUserResource(mRequest, mWeb.ServerRelativeUrl, this.ParentList, mContentTypeSource,
                        AveUserResourceConstants.DESCRIPTION_RESOUCE,ID.ToString(),base.DataCache);
                    base.DataCache.AddProperty(AveContentTypeUserResourceConstants.Description_Resource,titleResource);
                    return titleResource;
                }
                return base.DataCache.GetProperty<AveContentTypeUserResource>(AveContentTypeUserResourceConstants.Description_Resource);
            }
        }

        private void InitCTPropsBySchemaXml(string property = null)
        {
            if (string.IsNullOrEmpty(this.SchemaXml))
            {
                return;
            }
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(this.SchemaXml);
                switch (property)
                {
                    case "FieldLinks":
                        InitFieldLinks(xdoc, false);
                        break;
                    case "FieldLinksSchema":
                        InitCTFieldLinkSchema(xdoc);
                        break;
                    default:
                        InitFieldLinks(xdoc, true);
                        break;
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Error occurred when initializing content type properties: {0} by SchemaXml.Error Message:{1}.", property, ex.ToString());
            }
        }

        private void InitCTFieldLinkSchema(XmlDocument xdoc)
        {
            mContentTypeFieldLinkSchema = new Dictionary<Guid, XmlElement>();
            XmlNode fields = xdoc.SelectSingleNode(@"ContentType/Fields");
            if (fields == null)
            {
                return;
            }
            foreach (XmlElement xe in fields.ChildNodes)
            {
                if (xe.HasAttribute("ID"))
                {
                    Guid Id = new Guid(xe.GetAttribute("ID"));
                    if (!mContentTypeFieldLinkSchema.ContainsKey(Id))
                    {
                        mContentTypeFieldLinkSchema.Add(Id, xe);
                    }
                }
            }
        }

        private void InitFieldLinks(XmlDocument xdoc, bool addCache)
        {
            XmlNode fields = xdoc.SelectSingleNode(@"ContentType/Fields");            
            if (addCache)
            {
                mContentTypeFieldLinkSchema = new Dictionary<Guid, XmlElement>();
            }
            IDictionary<string, object> fieldLinksProp = new Dictionary<string, object>();
            var fieldLinksList = new List<IDictionary<string, object>>();
            if (fields != null)
            {               
                foreach (XmlElement children in fields.ChildNodes)
                {
                    fieldLinksList.Add(XmlNodeToFieldLinkProps(children));
                    if (addCache && children.HasAttribute("ID"))
                    {
                        Guid Id = new Guid(children.GetAttribute("ID"));
                        if (!mContentTypeFieldLinkSchema.ContainsKey(Id))
                        {
                            mContentTypeFieldLinkSchema.Add(Id, children);
                        }
                    }
                }
            }
            fieldLinksProp.AddChildren(fieldLinksList);
            mFieldLinks = new AveFieldLinkCollection(mRequest, this, fieldLinksProp);
        }

        private Dictionary<string, object> XmlNodeToFieldLinkProps(XmlElement node)
        {
            Dictionary<string, object> fieldLinkPro = new Dictionary<string, object>();
            StringBuilder linkXml = new StringBuilder();
            linkXml.Append("<FieldRef ");
            string[] fieldLinkPropertyStrings = new string[] { "ID", "Name", "DisplayName", "Required", "Hidden" , "ReadOnly" };
            foreach (string propertyName in fieldLinkPropertyStrings)
            {
                bool hasAttribute = node.HasAttribute(propertyName);
                switch (propertyName)
                {
                    case "ID"://Guid
                        fieldLinkPro["Id"] = hasAttribute ? new Guid(node.GetAttribute(propertyName)) : Guid.Empty;
                        linkXml.Append(string.Format("{0}='{1}' ", propertyName, fieldLinkPro["Id"].ToString()));
                        continue;
                    case "Hidden":
                    case "Required":
                    case "ReadOnly":
                        fieldLinkPro[propertyName] = hasAttribute ? bool.Parse(node.GetAttribute(propertyName)) : false;
                        break;
                    default://string
                        fieldLinkPro[propertyName] = hasAttribute ? node.GetAttribute(propertyName) : string.Empty;
                        break;
                }
                linkXml.Append(string.Format("{0}='{1}' ", propertyName, fieldLinkPro[propertyName].ToString()));
            }
            linkXml.Append("/>");
            fieldLinkPro["SchemaXml"] = linkXml.ToString();
            return fieldLinkPro;
        }

        public IAveFieldCollection Fields
        {
            get
            {
                if (mFields == null)
                {
                    lock (mFieldsLock)
                    {
                        if (mFields == null)
                        {
                            this.EnsureContentTypeData();
                            string listUrl = mContentTypes.ParentList == null ? null : mContentTypes.ParentList.DefaultViewUrl;
                            string listTitle = mContentTypes.ParentList == null ? null : mContentTypes.ParentList.Title;
                            Guid listId = mContentTypes.ParentList == null ? Guid.Empty : mContentTypes.ParentList.ID;
                            Dictionary<string, object> fieldsProperties = mRequest.GetFields(mContentTypes.ParentWeb.ServerRelativeUrl, listUrl, listTitle, listId, "contentType.fields", base.DataCache.ChangedProperties, AveUserResourceExtension.SupportedResourceCultureNames);
                            mFields = new AveFieldCollection(mContentTypes.ParentWeb, mContentTypes.ParentList, mRequest, "contentType.fields", this.DataCache.ChangedProperties, fieldsProperties);                                                        
                        }
                    }
                }
                return mFields;
            }
        }
        public bool Hidden
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Hidden");
            }
            set
            {
                base.DataCache.AddChangedProperty("Hidden", value);
            }
        }
        public string NewFormTemplateName
        {
            get
            {
                return base.DataCache.GetProperty<string>("NewFormTemplateName");
            }
            set
            {
                base.DataCache.AddChangedProperty("NewFormTemplateName", value);
            }
        }
        public string NewFormUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("NewFormUrl");
            }
            set
            {
                base.DataCache.AddChangedProperty("NewFormUrl", value);
            }
        }

        public Guid FeatureId
        {
            get { throw new NotImplementedException(); }
        }

        public IAveContentType Parent
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Parent"))
                {
                    var parentId = base.DataCache.GetProperty<string>("ParentId");
                    if (!string.IsNullOrEmpty(parentId))
                    {
                        var parentContentTypeId = new AveContentTypeId(parentId);
                        IAveContentType parent = null;
                        while (true)
                        {
                            parent = this.mWeb.ContentTypes[parentContentTypeId];

                            if (parent == null)
                            {
                                parent = this.mWeb.AvailableContentTypes[parentContentTypeId];
                            }

                            if (parent == null)
                            {
                                if (AveBuiltInContentTypeId.Contains(parentContentTypeId))
                                {
                                    break;
                                }
                                else
                                {
                                    parentContentTypeId = parentContentTypeId.Parent as AveContentTypeId;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        base.DataCache.AddProperty("Parent",parent);
                    }
                    else
                    {
                        base.DataCache.AddProperty("Parent",default(IAveContentType));
                    }
                }
                return base.DataCache.GetProperty<IAveContentType>("Parent");
            }
        }
        public IAveList ParentList
        {
            get
            {
                return base.DataCache.GetProperty<IAveList>("ParentList");
            }
        }
        public IAveWeb ParentWeb
        {
            get
            {
                return mWeb;
            }
        }
        public bool ReadOnly
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ReadOnly");
            }
            set
            {
                base.DataCache.AddChangedProperty("ReadOnly", value);
            }
        }

        public IAveWorkflowAssociationCollection WorkflowAssociations
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("WorkflowAssociations"))
                {
                    //this.EnsureContentTypeData();

                    Dictionary<string, object> wfAssociationsProp = null;

                    if (base.DataCache.IsPropertyAvailable("SPOWorkflowAssociations"))
                    {
                        wfAssociationsProp = base.DataCache.GetProperty<Dictionary<string, object>>("SPOWorkflowAssociations");
                    }
                    else
                    {
                        base.DataCache.AddChangedProperties(this.Location);
                        string webServerRelativeUrl = mContentTypes.ParentWeb == null ? null : mContentTypes.ParentWeb.ServerRelativeUrl;
                        string listTitle = null;
                        Guid listId = Guid.Empty;
                        if (mContentTypes.ParentList != null)
                        {
                            listTitle = mContentTypes.ParentList.Title;
                            listId = mContentTypes.ParentList.ID;
                        }
                        wfAssociationsProp = mRequest.GetWorkflowAssociations(webServerRelativeUrl, listTitle, listId, "contentType.workflow", base.DataCache.ChangedProperties);
                    }
                    //SAAS-26520 由于WorkflowAssociation不包含contentTypeId，所以需要在此进行操作，将ContentTypeId添加到字典中
                    //此ContentTypeID在DeleteWorkflowAssociation时会使用到
                    var wfAssociationsList = wfAssociationsProp.GetChildren();
                    if (wfAssociationsList != null)
                    {
                        foreach (var dic in wfAssociationsList)
                        {
                            dic["ContentTypeId"] = this.ID;
                        }
                    }
                    AveWorkflowAssociationCollection workflowAssociation = new AveWorkflowAssociationCollection(ParentWeb, ParentList, "contentType.workflow", wfAssociationsProp);
                    base.DataCache.AddProperty("WorkflowAssociations",workflowAssociation);
                    return workflowAssociation;
                }
                return base.DataCache.GetProperty<IAveWorkflowAssociationCollection>("WorkflowAssociations");
            }
        }

        public IAveFolder ResourceFolder
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ResourceFolder"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(this.SchemaXml);
                    XmlNode fields = doc.DocumentElement.GetElementsByTagName("Folder")[0];
                    if (fields != null)
                    {
                        string resourceFolder = fields.Attributes["TargetName"].Value;
                        Dictionary<string, object> resourceFolderProperties = new Dictionary<string, object>();
                        if (mContentTypeSource.Equals("list.contentTypes"))
                        {
                            if (!resourceFolder.StartsWith("/"))
                            {
                                resourceFolder = ParentList.RootFolder.ServerRelativeUrl + "/" + resourceFolder;
                            }
                            resourceFolderProperties = mRequest.GetFolder(mWeb.ServerRelativeUrl, ParentList.Title, resourceFolder);
                        }
                        else
                        {
                            if (!resourceFolder.StartsWith("/"))
                            {
                                resourceFolder = mWeb.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/" + resourceFolder;
                            }
                            resourceFolderProperties = mRequest.GetFolder(mWeb.ServerRelativeUrl, null, resourceFolder);
                        }
                        AveFolder folder = new AveFolder(mRequest, mWeb, null, null, resourceFolderProperties);
                        base.DataCache.AddProperty("ResourceFolder",folder);
                        return folder;
                    }
                    return null;
                }
                return base.DataCache.GetProperty<IAveFolder>("ResourceFolder");
            }
        }

        internal string ResourceFolderTargetName
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("InternalResourceFolderTargetName"))
                {
                    return ExtractResourceFolderTargetName(this.SchemaXml);
                }
                return base.DataCache.GetProperty<string>("InternalResourceFolderTargetName");
            }
        }

        internal string ExtractResourceFolderTargetName(string schemaXml)
        {
            if (base.DataCache.IsPropertyNotLoaded("InternalResourceFolderTargetName"))
            {
                string resourceFolder = null;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(schemaXml);
                XmlNode fields = doc.DocumentElement.GetElementsByTagName("Folder")[0];
                if (fields != null)
                {
                    resourceFolder = fields.Attributes["TargetName"].Value;
                    DataCache.AddProperty("InternalResourceFolderTargetName", resourceFolder);
                }
                return resourceFolder;
            }
            return base.DataCache.GetProperty<string>("InternalResourceFolderTargetName");
        }

        internal string ResourceFolderUrl
        {
            get
            {
                var targetName = ResourceFolderTargetName;

                if (!string.IsNullOrEmpty(targetName))
                {
                    if (ParentList != null)
                    {
                        return (Scope + "/" + targetName).Substring(mWeb.RootFolder.ServerRelativeUrl.Length).TrimStart('/');
                    }
                    else
                    {
                        return targetName;
                    }
                }

                return targetName;
            }
        }

        internal string ResourceFolderServerRelativeUrl
        {
            get
            {
                var targetName = ResourceFolderTargetName;
                if (!string.IsNullOrEmpty(targetName))
                {
                    return Scope.TrimEnd('/') + "/" + targetName;
                }

                return null;
            }
        }

        public string SchemaXml
        {
            get
            {
                string listId = this.ParentList == null ? string.Empty : this.ParentList.ID.ToString();
                string schemaXml = AveClientCacheHandler.GetSchemaXml(mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, this.ID.ToString(), SchemaType.ContentType);
                return schemaXml == string.Empty ? "<ContentType></ContentType>" : schemaXml;//ywzhang
            }
        }

        public bool Sealed
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Sealed");
            }
            set
            {
                base.DataCache.AddChangedProperty("Sealed", value);
            }
        }

        public IAveXmlDocumentCollection XmlDocuments
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("XmlDocuments"))
                {
                    Dictionary<string, string> xmlDocumentsProperties = AveXmlDocumentCollection.GetXmlDocumentDataFromSchemalXml(this.SchemaXml);
                    AveXmlDocumentCollection xmlDocuments = new AveXmlDocumentCollection(this, mRequest, xmlDocumentsProperties);
                    base.DataCache.AddProperty("XmlDocuments",xmlDocuments);
                    return xmlDocuments;
                }
                return base.DataCache.GetProperty<IAveXmlDocumentCollection>("XmlDocuments");
            }
        }

        public void Update()
        {
            this.Update(false);
        }

        public void Update(bool updateChildren)
        {
            //Solution Gallery contentype不支持更新
            if (String.Equals(this.Name, "Solution Gallery", StringComparison.OrdinalIgnoreCase) && this.ParentList!=null && this.ParentList.BaseTemplate== AveListTemplateType.SolutionCatalog) return;
            Dictionary<string, object> newProp = mRequest.UpdateContentType(this.ParentWeb.ServerRelativeUrl, ParentList == null ? null : ParentList.Title, ParentList == null ? Guid.Empty : ParentList.ID, this.ID.ToString(), updateChildren, mContentTypeSource, base.DataCache.ChangedProperties, this.ReadOnly, AveUserResourceExtension.SupportedResourceCultureNames);

            if (this.DataCache.ChangedProperties.ContainsKey("AddFieldLink") || this.DataCache.ChangedProperties.ContainsKey("DeleteFieldLink"))
            {
                base.DataCache.RemoveProperty("Fields");
                base.DataCache.RemoveProperty("FieldLinks");
                this.mFields = null;
                this.mFieldLinks = null;
            }
            if (newProp.ContainsKey("SchemaXml"))
            {
                string listId = this.ParentList != null ? this.ParentList.ID.ToString() : string.Empty;
                AveClientCacheHandler.WriteSchemaXml(newProp["SchemaXml"].ToString(), mWeb.CacheHandlerId, this.mWeb.ID.ToString(), listId, this.ID.ToString(), SchemaType.ContentType);
                newProp.Remove("SchemaXml");
            }
            this.DataCache.UpdateProperties(newProp);
        }


        public IAveWorkflowAssociation AddWorkflowAssociation(IAveWorkflowAssociation association)
        {
            Dictionary<string, object> props;
            if (ParentList != null)
            {
                props = mRequest.CreateListContentTypeAssociation(this.ParentWeb.ServerRelativeUrl, this.ParentList.ID, ID, "web.workflowTemplates", association);
            }
            else
            {
                props = mRequest.CreatWebContentTypeAssociation(this.ParentWeb.ServerRelativeUrl, this.ID, "web.workflowTemplates", association);
            }
            return new AveWorkflowAssociation(ParentWeb, this.ParentList, string.Empty, props);
        }

        public void UpdateWorkflowAssociation(IAveWorkflowAssociation workflowAssociation)
        {
            ((AveWorkflowAssociation)workflowAssociation).Update(this.ID.ToString());
        }

        public void UpdateWorkflowAssociationsOnChildren()
        {
            try
            {
                this.mRequest.UpdateWorkflowAssociationsOnChildren(this.mWeb.Url, this.ID.ToString());
            }
            catch (Exception ex)
            {
                mLogger.Warn("Update related content type with workflow association settings failed.content type id:{0}.error message:{1}",this.ID,ex.ToString());
            }
        }

        public string NewDocumentControl
        {
            get
            {
                return mNewDocumentControl;                
            }
            set
            {
                mNewDocumentControl = value;
                base.DataCache.AddChangedProperty("NewDocumentControl", value);
            }
        }

        public bool RequireClientRenderingOnNew
        {
            get
            {
                return mRequireClientRenderingOnNew;
            }
            set
            {
                mRequireClientRenderingOnNew = value;
                base.DataCache.AddChangedProperty("RequireClientRenderingOnNew", value);
            }
        }

        public string SchemaXmlWithResourceTokens
        {
            get
            {
                return base.DataCache.GetProperty<string>("SchemaXmlWithResourceTokens");
            }
            set
            {
                base.DataCache.AddChangedProperty("SchemaXmlWithResourceTokens", value);
            }
        }

        public IAveList List
        {
            get
            {
                return mContentTypes.ParentList;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public IAveWeb Web
        {
            get
            {
                return mContentTypes.ParentWeb;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public void UpdateIncludingSealedAndReadOnly(bool updateChildren)
        {
            throw new NotSupportedException();
        }

        public AveContentTypeInfo GetContentTypeInfo(bool backupParent)
        {
            bool backUpResourceFolder = true;

            AveContentTypeInfo ctInfo = new AveContentTypeInfo();
            ctInfo.Name = this.Name;
            ctInfo.Id = this.ID.ToString();
            try
            {
                ctInfo.ReadOnly = this.ReadOnly;
                ctInfo.Description = this.Description;
                ctInfo.NameResourceInfo
                    = DataCache.GetProperty<Dictionary<string, string>>(AveContentTypeUserResourceConstants.Name_Resource);
                ctInfo.DescriptionResourceInfo 
                    = DataCache.GetProperty<Dictionary<string, string>>(AveContentTypeUserResourceConstants.Description_Resource);
                //ctInfo.FieldsSchemaXml = this.Fields.SchemaXml;
                ctInfo.DocumentTemplate = this.DocumentTemplate;
                ctInfo.Group = this.Group;
                ctInfo.DisplayFormTemplateName = this.DisplayFormTemplateName;
                ctInfo.DisplayFormUrl = this.DisplayFormUrl;
                ctInfo.DocumentTemplateUrl = this.DocumentTemplateUrl;
                ctInfo.EditFormTemplateName = this.EditFormTemplateName;
                ctInfo.EditFormUrl = this.EditFormUrl;
                ctInfo.Hidden = this.Hidden;
                //ctInfo.NewDocumentControl = ct.NewDocumentControl;
                ctInfo.NewFormTemplateName = this.NewFormTemplateName;
                ctInfo.NewFormUrl = this.NewFormUrl;
                //ctInfo.RequireClientRenderingOnNew = ct.RequireClientRenderingOnNew;
                try
                {
                    ctInfo.ResourceFolder = this.ResourceFolder != null ? this.ResourceFolder.Url : null;
                }
                catch (Exception e)
                {
                    mLogger.Warn("Get resource folder's url in content type {0} failed. Backup will ignore this content type. \n{1}", this.Name, e.ToString());
                    backUpResourceFolder = false;
                }
                ctInfo.SchemaXml = this.SchemaXml;
                ctInfo.Scope = this.Scope;
                //ctInfo.Sealed = ct.Sealed;
                //ctInfo.Version = ct.Version;
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ctInfo.SchemaXml);
                XmlNode fields = doc.DocumentElement.GetElementsByTagName("Fields")[0];
                if (fields != null)
                {
                    ctInfo.FieldsSchemaXml = fields.OuterXml;
                }
                ctInfo.ParentName = this.Parent.Name;
                if (backupParent)
                {
                    ctInfo.ParentContentTypeInfo = GetParentContentTypeInfo(backupParent);
                }
                try
                {
                    if (backUpResourceFolder && this.ResourceFolder != null && this.ResourceFolder.Exists)
                    {
                        foreach (AveFile temFile in this.ResourceFolder.Files)
                        {
                            ctInfo.ResourceFolderFiles.Add(new AveContentTypeFileInfo(temFile.Url, temFile.OpenBinary()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    // mLog.Log(AveLogLevel.WARN, "WP10BKAveSPCT351", ctInfo.Id, ctInfo.Name, e);
                    mLogger.Warn("An error occurred when get files in resource folder. Content type name:{0}\tId:{1}\n{2}", ctInfo.Name, ctInfo.Id, ex.ToString());
                }

                foreach (string str in this.XmlDocuments)
                {
                    ctInfo.XmlDocuments.Add(str);
                }
            }
            catch (Exception exc)
            {
                mLogger.Warn("An error occurred while backing up content type. Name:{0}\tId:{1}\n{2}", ctInfo.Name, ctInfo.Id, exc.ToString());
            }
            return ctInfo;
        }

        public AveContentTypeInfo GetParentContentTypeInfo(bool backupParent)
        {
            if (AveBuiltInContentTypeId.Contains(Parent.ID))
            {
                return null;
            }
            return Parent.GetContentTypeInfo(backupParent);
        }


        public string GetFieldLinkSchemaXml()
        {
            return string.Empty;
        }

        public void Delete()
        {
            if (this.ParentList == null)
            {
                if (mRequest.DeleteContextType(this.ID.ToString(), this.ParentWeb.ServerRelativeUrl, Guid.Empty))
                {
                    (this.ParentWeb.ContentTypes as AveContentTypeCollection).ListData.Remove(this);
                }  
            }
            else
            {
                if (mRequest.DeleteContextType(this.ID.ToString(), this.ParentWeb.ServerRelativeUrl, this.ParentList.ID))
                {
                    (this.ParentList.ContentTypes as AveContentTypeCollection).ListData.Remove(this);
                }
            }
            
        }

        public void Initialize(IAveContentTypeCollection collection)
        {
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
    }
}
