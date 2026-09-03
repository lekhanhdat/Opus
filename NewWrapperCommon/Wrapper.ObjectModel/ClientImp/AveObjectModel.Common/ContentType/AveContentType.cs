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
        private AveEventReceiverDefinitionCollection mEventReceivers;
        private string mMD5;
        private object privateLock = new object();
        private object privateLockFields = new object();
        private AveUserResource mNameResource;
        private AveUserResource mDescriptionResource;
        private object privateLockNameResource = new object();
        private object privateLockDescriptionResource = new object();
        private Dictionary<Guid, XmlElement> mContentTypeFieldLinkSchema;
        private object workflowCollectionPrivateLock = new object();

        public AveContentType(IAveRequest request, IAveWeb parentWeb, IAveList parentList, AveContentTypeCollection contentTypes, string contentTypeSource, Dictionary<string, object> contentTypeProp)
        {
            mRequest = request;
            mContentTypes = contentTypes;
            mContentTypeSource = contentTypeSource;
            mWeb = parentWeb as AveWeb;
            contentTypeProp["ParentWeb"] = parentWeb;
            contentTypeProp["ParentList"] = parentList;
            base.DataCache.AddPropertyies(contentTypeProp);
        }

        public AveContentType(IAveContentType parentContentType, IAveContentTypeCollection contentTypeCol, string name)
        {
            mContentTypes = contentTypeCol as AveContentTypeCollection;
            mRequest = (contentTypeCol.Web.Site as AveSite).Request;
            mWeb = contentTypeCol.Web as AveWeb;
            base.DataCache.AddChangedProperty("IsNew", true);
            base.DataCache.AddChangedProperty("HasParentContentType", true);
            base.DataCache.AddChangedProperty("Name", name);
            base.DataCache.AddChangedProperty("ParentContentId", (parentContentType as AveContentType).ID.ToString());
            base.DataCache.PropertiesCache["Parent"] = parentContentType;
        }

        public AveContentType(AveContentTypeId contentTypeId, AveContentTypeCollection contentTypeCol, string name)
        {
            AveContentTypeId tempContentTypeId = new AveContentTypeId();
            tempContentTypeId = contentTypeId;
            mContentTypes = contentTypeCol;
            mRequest = (contentTypeCol.Web.Site as AveSite).Request;
            base.DataCache.AddChangedProperty("IsNew", true);
            base.DataCache.AddChangedProperty("Name", name);
            base.DataCache.AddChangedProperty("ContentTypeId", contentTypeId.ToString());
            base.DataCache.AddChangedProperty("ParentContentId", contentTypeId.Parent.ToString());
            base.DataCache.PropertiesCache["Parent"] = contentTypeCol.ParentWeb.AvailableContentTypes[contentTypeId.Parent];
            base.DataCache.PropertiesCache["ContentTypeId"] = contentTypeId;
        }

        //private void InitSchemaXml(ref Dictionary<string, object> contentTypeProperty, IAveList parentList)
        //{
        //    if (contentTypeProperty.ContainsKey("SchemaXml"))
        //    {
        //        string contentTypeId = Guid.Empty.ToString();
        //        string listId = parentList == null ? string.Empty : parentList.ID.ToString();
        //        if (contentTypeProperty.ContainsKey("Id" + AveObjectModelConstant.ObjectPropertySuffix))
        //        {
        //            contentTypeId = contentTypeProperty["Id" + AveObjectModelConstant.ObjectPropertySuffix].ToString();
        //        }
        //        AveClientCacheHandler.WriteSchemaXml(contentTypeProperty["SchemaXml"].ToString(), mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, contentTypeId, SchemaType.ContentType);
        //        contentTypeProperty.Remove("SchemaXml");
        //    }
        //}

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
                lock (privateLock)
                {
                    if (base.DataCache.IsPropertyNotLoaded("Id"))
                    {
                        AveContentTypeId Id = new AveContentTypeId(base.DataCache.GetProperty<string>("Id" + AveObjectModelConstant.ObjectPropertySuffix));
                        base.DataCache.PropertiesCache["Id"] = Id;
                        return Id;
                    }
                    return base.DataCache.GetProperty<IAveContentTypeId>("Id");
                }
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

        private Dictionary<string, object> XmlNodeToFieldLinkProps(XmlElement node)
        {
            Dictionary<string, object> fieldLinkPro = new Dictionary<string, object>();
            StringBuilder linkXml = new StringBuilder();
            linkXml.Append("<FieldRef ");
            string[] fieldLinkPropertyStrings = new string[] { "ID", "Name", "DisplayName", "Required", "Hidden" };
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
                    case "Required"://bool
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

        private void InitFieldLinks(XmlDocument xdoc, bool addCache)
        {
            XmlNode fields = xdoc.SelectSingleNode(@"ContentType/Fields");
            if (addCache)
            {
                mContentTypeFieldLinkSchema = new Dictionary<Guid, XmlElement>();
            }
            Dictionary<string, object> fieldLinksProp = new Dictionary<string, object>();
            List<Dictionary<string, object>> fieldLinksList = new List<Dictionary<string, object>>();
            if (fields != null)
            {
                foreach (XmlElement children in fields.ChildElements())
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
            fieldLinksProp[AveObjectModelConstant.ChildrenProperties] = fieldLinksList;
            base.DataCache.PropertiesCache["FieldLinks"] = new AveFieldLinkCollection(mRequest, this, fieldLinksProp);
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

        public IAveFieldLinkCollection FieldLinks
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("FieldLinks"))
                {
                    string listUrl = mContentTypes.ParentList == null ? null : mContentTypes.ParentList.DefaultViewUrl;
                    string listTitle = mContentTypes.ParentList == null ? null : mContentTypes.ParentList.Title;
                    Guid listId = mContentTypes.ParentList == null ? Guid.Empty : mContentTypes.ParentList.ID;
                    Dictionary<string, object> fieldLinksProp = mRequest.GetFieldLinks(mContentTypes.ParentWeb.ServerRelativeUrl, listUrl, listTitle, listId, this.ID.ToString(), mContentTypeSource);
                    AveFieldLinkCollection fieldLinks = new AveFieldLinkCollection(mRequest, this, fieldLinksProp);
                    base.DataCache.PropertiesCache["FieldLinks"] = fieldLinks;
                    return fieldLinks;
                }
                return base.DataCache.GetProperty<IAveFieldLinkCollection>("FieldLinks");
            }
        }
        public IAveFieldCollection Fields
        {
            get
            {
                AveFieldCollection fields = null;
                lock (privateLockFields)
                {
                    if (base.DataCache.IsPropertyNotLoaded("Fields"))
                    {
                        this.EnsureContentTypeData();
                        fields = new AveFieldCollection(mContentTypes.ParentWeb, mContentTypes.ParentList, "contentType.fields", base.DataCache.ChangedProperties);
                        base.DataCache.PropertiesCache["Fields"] = fields;
                    }
                    else
                    {
                        fields = base.DataCache.GetProperty<AveFieldCollection>("Fields");
                        if (fields.IsCollectionDirty)
                        {
                            fields.UpdateCollectionInternally();
                        }
                    }
                }
                return fields;
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
            get
            {
                return base.DataCache.GetProperty<Guid>("FeatureId");
            }
        }

        public IAveContentType Parent
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Parent"))
                {
                    AveContentTypeId parentContentTypeId = new AveContentTypeId(base.DataCache.GetProperty<string>("ParentId"));
                    IAveContentType parent = this.mWeb.ContentTypes[parentContentTypeId];
                    if (parent == null)
                    {
                        parent = this.mWeb.AvailableContentTypes[parentContentTypeId];
                    }
                    base.DataCache.PropertiesCache["Parent"] = parent;
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
                lock (privateLock)
                {
                    if (base.DataCache.IsPropertyNotLoaded("WorkflowAssociations"))
                    {
                        this.EnsureContentTypeData();
                        string webServerRelativeUrl = mContentTypes.ParentWeb == null ? null : mContentTypes.ParentWeb.ServerRelativeUrl;
                        string listTile = mContentTypes.ParentList == null ? null : mContentTypes.ParentList.Title;
                        //Dictionary<string, object> wfAssociationsProp = null;
                        AveWorkflowAssociationCollection workflowAssociation = null;
                        //wfAssociationsProp = mRequest.GetWorkflowAssociations(webServerRelativeUrl, listTile, ParentList == null ? Guid.Empty : ParentList.ID, "contentType.workflow", base.DataCache.ChangedProperties);
                        workflowAssociation = new AveWorkflowAssociationCollection(ParentWeb, ParentList, base.DataCache.ChangedProperties, "contentType.workflow");

                        base.DataCache.PropertiesCache["WorkflowAssociations"] = workflowAssociation;
                        return workflowAssociation;
                    }
                    else if (base.DataCache.GetProperty<AveWorkflowAssociationCollection>("WorkflowAssociations").IsDirty)
                    {
                        base.DataCache.GetProperty<AveWorkflowAssociationCollection>("WorkflowAssociations").UpdateCollectionInternally();
                    }
                    return base.DataCache.GetProperty<IAveWorkflowAssociationCollection>("WorkflowAssociations");
                }
            }
        }

        public IAveFolder ResourceFolder
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ResourceFolder"))
                {
                    if (string.IsNullOrEmpty(this.SchemaXml))
                    {
                        return null;
                    }
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(this.SchemaXml);

                    XmlNode fields = doc.DocumentElement.GetElementsByTagName("Folder")[0];
                    if (fields != null)
                    {
                        string resourceFolder = fields.Attributes["TargetName"].Value;
                        Dictionary<string, object> resourceFolderProperties = new Dictionary<string, object>();
                        if (mContentTypeSource.Equals("list.contentTypes"))
                        {
                            if (!resourceFolder.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                            {
                                resourceFolder = ParentList.RootFolder.ServerRelativeUrl + "/" + resourceFolder;
                            }
                            resourceFolderProperties = mRequest.GetFolder(mWeb.ServerRelativeUrl, ParentList.Title, ParentList.ID, resourceFolder);
                        }
                        else
                        {
                            if (!resourceFolder.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!string.IsNullOrEmpty(Web.ServerRelativeUrl) && !Web.ServerRelativeUrl.Equals("/", StringComparison.OrdinalIgnoreCase))
                                {
                                    Web.ServerRelativeUrl = Web.ServerRelativeUrl.TrimEnd('/');
                                }
                                resourceFolder = Web.ServerRelativeUrl.TrimEnd('/') + "/" + resourceFolder;
                            }
                            resourceFolderProperties = mRequest.GetFolder(mWeb.ServerRelativeUrl, null, Guid.Empty, resourceFolder);
                        }
                        AveFolder folder = new AveFolder(mRequest, mWeb, null, null, resourceFolderProperties);
                        base.DataCache.PropertiesCache["ResourceFolder"] = folder;
                        return folder;
                    }
                    return null;
                }
                return base.DataCache.GetProperty<IAveFolder>("ResourceFolder");
            }
        }

        public bool ResourceFolderExists
        {
            get
            {
                return true;
            }
        }

        public string SchemaXml
        {
            get
            {
                //新添加的ContentType的SchemaXml放到内存里。 Reload CotentType Collection时再写到本地。提升效率。
                if (base.DataCache.IsPropertyAvailable("SchemaXml"))
                {
                    return base.DataCache.GetProperty<string>("SchemaXml");
                }
                string listId = this.ParentList == null ? string.Empty : this.ParentList.ID.ToString();
                return AveClientCacheHandler.GetSchemaXml(mWeb.CacheHandlerId, mWeb.ID.ToString(), listId, this.ID.ToString(), SchemaType.ContentType);
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
                    base.DataCache.PropertiesCache["XmlDocuments"] = xmlDocuments;
                    return xmlDocuments;
                }
                return base.DataCache.GetProperty<IAveXmlDocumentCollection>("XmlDocuments");
            }
        }

        /// <summary>
        /// Office 365 DocumentSet 在List 级别Update 可能存在问题，建议使用另一个重载方法并将updateChildren传入true以防office 365抛出异常导致Document Set还原失败。
        /// </summary>
        public void Update()
        {
            this.Update(false);
        }

        public void Update(bool updateChildren)
        {
            Guid listGuid = ParentList == null ? Guid.Empty : ParentList.ID;
            string listTitle = ParentList == null ? null : ParentList.Title;
            Dictionary<string, object> newProp = mRequest.UpdateContentType(this.ParentWeb.ServerRelativeUrl, listTitle, listGuid, this.ID.ToString(), updateChildren, mContentTypeSource, base.DataCache.ChangedProperties);

            if (this.DataCache.ChangedProperties.ContainsKey("AddFieldLink") || this.DataCache.ChangedProperties.ContainsKey("DeleteFieldLink"))
            {
                base.DataCache.PropertiesCache.Remove("Fields");
                base.DataCache.PropertiesCache.Remove("FieldLinks");
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
            AveWorkflowAssociation newWFAssociation = new AveWorkflowAssociation(ParentWeb, this.ParentList, string.Empty, props);
            if (base.DataCache.IsPropertyAvailable("WorkflowAssociations"))
            {
                (this.WorkflowAssociations as AveWorkflowAssociationCollection).ListData.Add(newWFAssociation);
            }
            return newWFAssociation;
        }

        public void UpdateWorkflowAssociation(IAveWorkflowAssociation workflowAssociation)
        {
            ((AveWorkflowAssociation)workflowAssociation).Update(this.ID.ToString());
        }

        public void UpdateWorkflowAssociationsOnChildren()
        {
            this.mRequest.UpdateWorkflowAssociationsOnChildren(this.mWeb.Url, this.ID.ToString());
        }

        public string NewDocumentControl
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("NewDocumentControl") && !string.IsNullOrEmpty(this.SchemaXml))
                {
                    string tempValue = string.Empty;
                    XmlDocument tempDoc = new XmlDocument();
                    tempDoc.LoadXml(this.SchemaXml);
                    if (tempDoc.DocumentElement.HasAttribute("NewDocumentControl") &&
                        !string.IsNullOrEmpty(tempDoc.DocumentElement.Attributes["RequireClientRenderingOnNew"].Value))
                    {
                        tempValue = tempDoc.DocumentElement.Attributes["NewDocumentControl"].Value;
                    }
                    base.DataCache.PropertiesCache["NewDocumentControl"] = tempValue;
                }
                return base.DataCache.GetProperty<string>("NewDocumentControl");
            }
            set
            {
                base.DataCache.AddChangedProperty("NewDocumentControl", value);
            }
        }

        public bool RequireClientRenderingOnNew
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RequireClientRenderingOnNew") && !string.IsNullOrEmpty(this.SchemaXml))
                {
                    bool tempValue = true;
                    XmlDocument tempDoc = new XmlDocument();
                    tempDoc.LoadXml(this.SchemaXml);
                    if (tempDoc.DocumentElement.HasAttribute("RequireClientRenderingOnNew") &&
                        !string.IsNullOrEmpty(tempDoc.DocumentElement.Attributes["RequireClientRenderingOnNew"].Value))
                    {
                        tempValue = bool.Parse(tempDoc.DocumentElement.Attributes["RequireClientRenderingOnNew"].Value);
                    }
                    base.DataCache.PropertiesCache["RequireClientRenderingOnNew"] = tempValue;
                }
                return base.DataCache.GetProperty<bool>("RequireClientRenderingOnNew");
            }
            set
            {
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

        /// <summary>
        /// 此方法不建议除Document Set以外的Content Type进行调用。
        /// </summary>
        /// <param name="updateChildren"></param>
        public void UpdateIncludingSealedAndReadOnly(bool updateChildren)
        {
            throw new NotSupportedException();
        }

        private void InitCTFieldLinkSchema(XmlDocument xdoc)
        {
            mContentTypeFieldLinkSchema = new Dictionary<Guid, XmlElement>();
            XmlNode fields = xdoc.SelectSingleNode(@"ContentType/Fields");
            if (fields == null)
            {
                return;
            }
            foreach (XmlElement xe in fields.ChildElements())
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

        public Dictionary<Guid, XmlElement> ContentTypeFieldLinkSchema
        {
            get
            {
                if (mContentTypeFieldLinkSchema == null)
                {
                    try
                    {
                        InitCTPropsBySchemaXml("FieldLinksSchema");
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("Can not get FieldLinkSchema in contentType:{0}, Exception:{3}.", Name, e.ToString());
                    }
                }
                return mContentTypeFieldLinkSchema;
            }
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
                ctInfo.FieldsSchemaXml = this.Fields.SchemaXml;
                ctInfo.DocumentTemplate = this.DocumentTemplate;
                ctInfo.Group = this.Group;
                ctInfo.DisplayFormTemplateName = this.DisplayFormTemplateName;
                ctInfo.DisplayFormUrl = this.DisplayFormUrl;
                ctInfo.DocumentTemplateUrl = this.DocumentTemplateUrl;
                ctInfo.EditFormTemplateName = this.EditFormTemplateName;
                ctInfo.EditFormUrl = this.EditFormUrl;
                ctInfo.Hidden = this.Hidden;
                ctInfo.NewDocumentControl = this.NewDocumentControl;
                ctInfo.NewFormTemplateName = this.NewFormTemplateName;
                ctInfo.NewFormUrl = this.NewFormUrl;
                ctInfo.RequireClientRenderingOnNew = this.RequireClientRenderingOnNew;
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
                ctInfo.ParentName = this.Parent.Name;
                ctInfo.NameResource = this.NameResource.GetUserResourceInfo(this.Web);
                ctInfo.DescriptionResource = this.DescriptionResource.GetUserResourceInfo(this.Web);
                if (backupParent)
                {
                    ctInfo.ParentContentTypeInfo = GetParentContentTypeInfo(backupParent);
                }
                try
                {
                    if (backUpResourceFolder && (this.ResourceFolder != null))
                    {
                        foreach (AveFile temFile in this.ResourceFolder.Files)
                        {
                            ctInfo.ResourceFolderFiles.Add(new AveContentTypeFileInfo(temFile.Url, temFile.OpenBinary(), temFile.Properties, temFile.TimeCreated, temFile.TimeLastModified));
                        }
                    }
                }
                catch (Exception ex)
                {
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

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveContentType.GetFieldLinkSchemaXml"))
            {

            XmlDocument doc = new XmlDocument();
            StringBuilder builder = new StringBuilder();
            foreach (IAveFieldLink link in this.FieldLinks)
            {
                //对contenttype应用workflow所产生的fieldlink，不应该备份出来
                //if (Fields.Contains(link.ID) && Fields[link.ID].TypeAsString.Equals("WorkflowStatus", StringComparison.OrdinalIgnoreCase))
                if (ContentTypeFieldLinkSchema.ContainsKey(link.ID) &&
                    ContentTypeFieldLinkSchema[link.ID].HasAttribute("Type") &&
                    ContentTypeFieldLinkSchema[link.ID].GetAttribute("Type").Equals("WorkflowStatus", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                try
                {
                    IAveField relativeField = null;
                    if (ParentList != null && ParentList.Fields != null)
                    {
                        if (ParentList.Fields.Contains(link.ID))
                        {
                            relativeField = ParentList.Fields[link.ID];
                        }
                    }
                    else if (mWeb.Fields.Contains(link.ID))
                    {
                        relativeField = mWeb.Fields[link.ID];
                    }
                    //fieldLink的hidden，如果ContentType的xml中存在load ContentType的xml，否则load fieldLink中xml
                    if (ContentTypeFieldLinkSchema != null && ContentTypeFieldLinkSchema.ContainsKey(link.ID))
                    {
                        doc.LoadXml(ContentTypeFieldLinkSchema[link.ID].OuterXml);
                    }
                    else
                    {
                        doc.LoadXml(link.SchemaXml);
                    }
                    //fieldLink的hidden，如果xml中存在就以xml中的为主，如果xml不存在就以关联field的hidden属性为主
                    doc.DocumentElement.SetAttribute("Hidden", CheckFieldLinkIsHidden(doc.DocumentElement, relativeField, link).ToString());
                    doc.DocumentElement.SetAttribute("Required", CheckFieldLinkIsRequired(doc.DocumentElement, relativeField, link).ToString());
                    doc.DocumentElement.SetAttribute("ReadOnly", CheckFieldLinkIsReadOnly(doc.DocumentElement, relativeField, link).ToString());
                    builder.Append(doc.OuterXml);
                }
                catch (Exception e)
                {
                    mLogger.Warn("Can not get fieldLink info in contentType:{0}, fieldLink Id:{1},name:{2},Exception:{3}", this.Name, link.ID, link.Name, e.ToString());
                }
            }
            return "<Fields>" + builder.ToString() + "</Fields>";

            }

        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        private bool CheckFieldLinkIsHidden(XmlElement linkXml, IAveField field, IAveFieldLink fieldLink)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveContentType.CheckFieldLinkIsHidden"))
            {

            bool fieldLinkHidden = false;
            bool hasHiddenSchema = linkXml.HasAttribute("Hidden");
            if (hasHiddenSchema)
            {
                fieldLinkHidden = bool.Parse(linkXml.Attributes["Hidden"].Value);
            }
            else if (field != null)
            {
                fieldLinkHidden = field.Hidden;
            }
            else
            {
                fieldLinkHidden = fieldLink.Hidden;
            }
            return fieldLinkHidden;

            }

        }

        private bool CheckFieldLinkIsReadOnly(XmlElement linkXml, IAveField field, IAveFieldLink fieldLink)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveContentType.CheckFieldLinkIsHidden"))
            {

            bool fieldLinkReadOnly = false;
            bool hasHiddenSchema = linkXml.HasAttribute("ReadOnly");
            if (hasHiddenSchema)
            {
                fieldLinkReadOnly = bool.Parse(linkXml.Attributes["ReadOnly"].Value);
            }
            else if (field != null)
            {
                fieldLinkReadOnly = field.ReadOnlyField;

            }
            else
            {
                fieldLinkReadOnly = fieldLink.ReadOnly;
            }
            return fieldLinkReadOnly;

            }

        }

        private bool CheckFieldLinkIsRequired(XmlElement linkXml, IAveField field, IAveFieldLink fieldLink)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveContentType.CheckFieldLinkIsHidden"))
            {

            bool fieldLinkRequired = false;
            bool hasHiddenSchema = linkXml.HasAttribute("Required");
            if (hasHiddenSchema)
            {
                fieldLinkRequired = bool.Parse(linkXml.Attributes["Required"].Value);
            }
            else if (field != null)
            {
                fieldLinkRequired = field.Required;

            }
            else
            {
                fieldLinkRequired = fieldLink.Required;
            }
            return fieldLinkRequired;

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

        public IAveEventReceiverDefinitionCollection EventReceivers
        {
            get
            {
                //在server上实现，Client没有实现,返回默认值
                return default(IAveEventReceiverDefinitionCollection);
            }
        }

        public IAveUserResource NameResource
        {
            get
            {
                if (!mWeb.Site.IsOnlineSite)
                {
                    return null;
                }
                lock (privateLockNameResource)
                {
                    if(mNameResource == null)
                    {
                        mNameResource = new AveContentTypeUserResource(this, AveUserResourceConstants.TITLE_RESOUCE, mContentTypeSource, this.DataCache);
                    }
                    return mNameResource;
                }
            }
        }

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
                        mDescriptionResource = new AveContentTypeUserResource(this, AveUserResourceConstants.DESCRIPTION_RESOUCE, mContentTypeSource, this.DataCache);
                    }
                    return mDescriptionResource;
                }
            }
        }


        public string JSLink
        {
            get
            {
                return base.DataCache.GetProperty<string>("JSLink");
            }
            set
            {
                base.DataCache.AddChangedProperty("JSLink", value);
            }
        }

        public string MobileDisplayFormUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("MobileDisplayFormUrl");
            }
            set
            {
                base.DataCache.AddChangedProperty("MobileDisplayFormUrl", value);
            }
        }

        public string MobileEditFormUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("MobileEditFormUrl");
            }
            set
            {
                base.DataCache.AddChangedProperty("MobileEditFormUrl", value);
            }
        }

        public string MobileNewFormUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("MobileNewFormUrl");
            }
            set
            {
                base.DataCache.AddChangedProperty("MobileNewFormUrl", value);
            }
        }
    }
}
