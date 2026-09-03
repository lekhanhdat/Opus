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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveContentType : AveServerObject, IAveContentType, IDisposable
    {
        private SPContentType mContentType;
        private AveContentType mParent;
        private AveContentTypeId mContentTypeId;
        private AveFieldCollection mFields;
        private AveList mParentList;
        private AveWeb mWeb;
        private AveSite mSite;
        private AveFolder mResourceFolder;
        private AveXmlDocumentCollection mXmlDocuments;
        private AveFieldLinkCollection mFieldLinks;
        private AveWorkflowAssociationCollection mWorkflowAssociations;
        private AveContentTypeCollection mContentTypes;
        private AveEventReceiverDefinitionCollection mEventReceivers;
        private string mMD5;
        private Dictionary<string, XmlElement> mContentTypeFieldLinkSchema;
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveContentType));
        internal SPContentType ContentType
        {
            get
            {
                return mContentType;
            }
        }

        public AveContentType(AveContentTypeCollection contentTypes, SPContentType contentType)
        {
            mContentTypes = contentTypes;
            mContentType = contentType;
            Init();
        }

        public AveContentType(IAveContentType parentContentType, IAveContentTypeCollection collection, string name)
        {
            mContentTypes = collection as AveContentTypeCollection;
            if (parentContentType != null)
            {
                mContentType = new SPContentType((parentContentType as AveContentType).ContentType, (collection as AveContentTypeCollection).ContentTypeCollection, name);
            }
            else
            {
                mContentType = new SPContentType(null, (collection as AveContentTypeCollection).ContentTypeCollection, name);
            }
            Init();
        }

        public AveContentType(IAveContentTypeId contentTypeId, IAveContentTypeCollection collection, string name)
        {
            mContentTypes = collection as AveContentTypeCollection;
            mContentType = new SPContentType((contentTypeId as AveContentTypeId).ContentTypeId, (collection as AveContentTypeCollection).ContentTypeCollection, name);
            Init();
        }

        public AveContentType()
        {
            mContentType = (SPContentType)AveAssemblyUtility.CreateInstance(typeof(SPContentType), new Type[0], new object[0]);
        }

        public AveContentType(IAveContentTypeId contentTypeId)
        {
            mContentType = (SPContentType)AveAssemblyUtility.CreateInstance(typeof(SPContentType), new Type[] { typeof(SPContentTypeId) }, new object[] { (contentTypeId as AveContentTypeId).ContentTypeId });
        }

        private void Init()
        {
            if (mContentTypes != null && mContentTypes.Web != null)
            {
                mSite = mContentTypes.Web.Site as AveSite;
                if (mContentType.ParentWeb != null && mContentType.ParentWeb.ID == mContentTypes.Web.ID)
                {
                    mWeb = mContentTypes.Web as AveWeb;
                }
            }
            if (mContentTypes != null && mContentTypes.List != null)
            {
                mParentList = mContentTypes.List as AveList;
            }
            else if (mContentType.ParentList != null)
            {
                mParentList = (this.Web.Lists as AveListCollection).CreateListByType(mContentType.ParentList);
            }
        }

        #region IAveContentType Members

        public string Name
        {
            get
            {
                CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
                if (Web != null)
                {
                    culture = Web.UICulture;
                }
                return mContentType.NameResource.GetValueForUICulture(culture);
            }
            set
            {
                mContentType.Name = value;
            }
        }

        public IAveContentTypeId ID
        {
            get
            {
                if (mContentTypeId == null)
                {
                    SPContentTypeId contentTypeId = mContentType.Id;
                    if (contentTypeId != null)
                    {
                        mContentTypeId = new AveContentTypeId(contentTypeId);
                    }
                }
                return mContentTypeId;
            }
        }

        public string Description
        {
            get
            {
                return mContentType.Description;
            }
            set
            {
                mContentType.Description = value;
            }
        }

        public string DisplayFormTemplateName
        {
            get
            {
                return mContentType.DisplayFormTemplateName;
            }
            set
            {
                mContentType.DisplayFormTemplateName = value;
            }
        }

        public string DisplayFormUrl
        {
            get
            {
                return mContentType.DisplayFormUrl;
            }
            set
            {
                mContentType.DisplayFormUrl = value;
            }
        }

        public string DocumentTemplate
        {
            get
            {
                return mContentType.DocumentTemplate;
            }
            set
            {
                mContentType.DocumentTemplate = value;
            }
        }

        public string DocumentTemplateUrl
        {
            get { return mContentType.DocumentTemplateUrl; }
        }

        public string EditFormTemplateName
        {
            get
            {
                return mContentType.EditFormTemplateName;
            }
            set
            {
                mContentType.EditFormTemplateName = value;
            }
        }

        public string EditFormUrl
        {
            get
            {
                return mContentType.EditFormUrl;
            }
            set
            {
                mContentType.EditFormUrl = value;
            }
        }

        public IAveFieldCollection Fields
        {
            get
            {
                if (mFields == null)
                {
                    mFields = new AveFieldCollection(mWeb, mContentType.Fields);
                }
                return mFields;
            }
        }

        public string Group
        {
            get
            {
                return mContentType.Group;
            }
            set
            {
                mContentType.Group = value;
            }
        }

        public string NewFormTemplateName
        {
            get
            {
                return mContentType.NewFormTemplateName;
            }
            set
            {
                mContentType.NewFormTemplateName = value;
            }
        }

        public string NewFormUrl
        {
            get
            {
                return mContentType.NewFormUrl;
            }
            set
            {
                mContentType.NewFormUrl = value;
            }
        }

        public bool Hidden
        {
            get
            {
                return mContentType.Hidden;
            }
            set
            {
                mContentType.Hidden = value;
            }
        }

        public Guid FeatureId
        {
            get { return mContentType.FeatureId; }
        }

        public IAveContentType Parent
        {
            get
            {
                if (mParent == null)
                {
                    SPContentType contentType = mContentType.Parent;
                    if (contentType != null)
                    {
                        mParent = new AveContentType(mContentTypes, contentType);
                    }
                }
                return mParent;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return mContentType.ReadOnly;
            }
            set
            {
                mContentType.ReadOnly = value;
            }
        }

        public string SchemaXml
        {
            get { return mContentType.SchemaXml; }
        }

        public string SchemaXmlWithResourceTokens
        {
            get
            {
                return mContentType.SchemaXmlWithResourceTokens;
            }
            set
            {
                mContentType.SchemaXmlWithResourceTokens = value;
            }
        }

        public string Scope
        {
            get { return mContentType.Scope; }
        }

        public IAveList ParentList
        {
            get
            {
                return mParentList;
            }
        }

        public IAveWeb ParentWeb
        {
            get
            {
                return this.Web;
            }
        }

        public IAveFolder ResourceFolder
        {
            get
            {
                if (mResourceFolder == null)
                {
                    mResourceFolder = new AveFolder(mWeb, mContentType.ResourceFolder);
                }
                return mResourceFolder;
            }
        }

        public bool ResourceFolderExists
        {
            get
            {
                bool exist = true;
                try
                {
                    //ADO-24300,this.ResourceFolder在ResourceFolder不存在的情况下会自动创建，因此改用反射来判断是否存在。
                    string resourceFolder = Invoker.GetProperty(mContentType, "ResourceFolderServerRelativeUrl").ToString().Substring(ParentWeb.ServerRelativeUrl.Length != 1 ? ParentWeb.ServerRelativeUrl.Length + 1 : 1);
                    //ADO-24981，如果没有resourceFolder，则不备份，避免因为调用ct.ResourceFolder API而创建出ResourceFolder。
                    if (!Web.GetFolder(resourceFolder).Exists)
                    {
                        exist = false;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetCTResourceFolderError, e.ToString());
                    exist = false;
                }
                return exist;
            }
        }

        public bool Sealed
        {
            get { return mContentType.Sealed; }
            set { mContentType.Sealed = value; }
        }

        public IAveXmlDocumentCollection XmlDocuments
        {
            get
            {
                if (mXmlDocuments == null)
                {
                    mXmlDocuments = new AveXmlDocumentCollection(mContentType.XmlDocuments);
                }
                return mXmlDocuments;
            }
        }

        public IAveWorkflowAssociationCollection WorkflowAssociations
        {
            get
            {
                //if (mWorkflowAssociations == null)
                //{
                //mWorkflowAssociations = new AveWorkflowAssociationCollection(mContentType.WorkflowAssociations);
                //}
                mWorkflowAssociations = new AveWorkflowAssociationCollection(this, mContentType.WorkflowAssociations);
                return mWorkflowAssociations;
            }
        }

        public void Update()
        {
            mContentType.Update();
        }

        public void Update(bool updateChildren)
        {
            mContentType.Update(updateChildren);
        }

        public void UpdateIncludingSealedAndReadOnly(bool updateChildren)
        {
            mContentType.UpdateIncludingSealedAndReadOnly(updateChildren);
        }

        public IAveFieldLinkCollection FieldLinks
        {
            get
            {
                if (mFieldLinks == null)
                {
                    SPFieldLinkCollection fieldLinkCollection = mContentType.FieldLinks;
                    if (fieldLinkCollection != null)
                    {
                        mFieldLinks = new AveFieldLinkCollection(fieldLinkCollection);
                    }
                }
                return mFieldLinks;
            }
        }

        [Obsolete]
        public IAveWorkflowAssociation AddWorkflowAssociation(IAveWorkflowAssociation association)
        {
            return new AveWorkflowAssociation(this.WorkflowAssociations, mContentType.AddWorkflowAssociation((association as AveWorkflowAssociation).WorkflowAssociation));
        }

        [Obsolete]
        public void UpdateWorkflowAssociation(IAveWorkflowAssociation association)
        {
            mContentType.UpdateWorkflowAssociation((association as AveWorkflowAssociation).WorkflowAssociation);
        }

        public void UpdateWorkflowAssociationsOnChildren()
        {
            mContentType.UpdateWorkflowAssociationsOnChildren();
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

        public string NewDocumentControl
        {
            get
            {
                return mContentType.NewDocumentControl;
            }
            set
            {
                mContentType.NewDocumentControl = value;
            }
        }

        public bool RequireClientRenderingOnNew
        {
            get
            {
                return mContentType.RequireClientRenderingOnNew;
            }
            set
            {
                mContentType.RequireClientRenderingOnNew = value;
            }
        }

        public IAveList List
        {
            get
            {
                object obj = AveAssemblyUtility.GetPropertyValueByType(mContentType, "List", null);
                if (null == obj)
                {
                    return null;
                }
                SPList tmpList = (SPList)obj;
                AveSite tmpSite = new AveSite(tmpList.ParentWeb.Site);
                AveWeb tmpWeb = new AveWeb(tmpSite, tmpList.ParentWeb);
                return (tmpWeb.Lists as AveListCollection).CreateListByType(tmpList) as IAveList;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValueByType(mContentType, "List", (value as AveList).List, null);
            }
        }

        /// <summary>
        /// 此处的web是外围构建时传进来的,或者是反射从SPConTentType中获取到的,不能dispose
        /// </summary>
        public IAveWeb Web
        {
            get
            {
                if (mWeb == null)
                {
                    object obj = AveAssemblyUtility.GetPropertyValueByType(mContentType, "Web", null);
                    if (null != obj)
                    {
                        SPWeb tmpWeb = (SPWeb)obj;
                        mWeb = new AveWeb(mSite, tmpWeb);
                    }
                }
                return mWeb;
            }
            set
            {
                mWeb = value as AveWeb;
                if (mWeb != null)
                {
                    mSite = mWeb.Site as AveSite;
                    AveAssemblyUtility.SetPropertyValueByType(mContentType, "Web", mWeb.Web, null);
                }
                else
                {
                    AveAssemblyUtility.SetPropertyValueByType(mContentType, "Web", null, null);
                }
            }
        }

        public IAveEventReceiverDefinitionCollection EventReceivers
        {
            get
            {
                if (mEventReceivers == null)
                {
                    mEventReceivers = new AveEventReceiverDefinitionCollection(mContentType.EventReceivers);
                }
                return mEventReceivers;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (mFields != null)
            {
                mFields.Dispose();
                mFields = null;
            }
            ClearLargeObject();
        }

        private void ClearLargeObject()
        {
            this.mContentTypeFieldLinkSchema = null;
        }


        #endregion

        public Dictionary<string, XmlElement> ContentTypeFieldLinkSchema
        {
            get
            {
                if (mContentTypeFieldLinkSchema == null)
                {
                    try
                    {
                        mContentTypeFieldLinkSchema = new Dictionary<string, XmlElement>();
                        if (!string.IsNullOrEmpty(this.SchemaXml))
                        {
                            XmlDocument xdoc = new XmlDocument();
                            xdoc.LoadXml(this.SchemaXml);
                            XmlNode fields = xdoc.SelectSingleNode(@"ContentType/Fields");
                            foreach (XmlNode children in fields.ChildNodes)
                            {
                                XmlElement xe = children as XmlElement;
                                if (xe.HasAttribute("ID"))
                                {
                                    string Id = xe.GetAttribute("ID");
                                    if (!mContentTypeFieldLinkSchema.ContainsKey(Id.ToLower(CultureInfo.InvariantCulture)))
                                    {
                                        mContentTypeFieldLinkSchema.Add(Id.ToLower(CultureInfo.InvariantCulture), xe);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Can not get FieldLinkSchema in ContentType: {0}, Exception: {3}", mContentType.Name, e.ToString());
                    }
                }
                return mContentTypeFieldLinkSchema;
            }
        }

        public AveContentTypeInfo GetContentTypeInfo(bool backupParent)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveContentType.GetContentTypeInfo"))
            {
                bool backUpResourceFolder = true;

                AveContentTypeInfo ctInfo = new AveContentTypeInfo();
                ctInfo.Name = this.Name;
                ctInfo.Id = this.ID.ToString();
                try
                {
                    ctInfo.ReadOnly = this.ReadOnly;
                    //当site语言环境和系统语言环境不一致时，应使用User Resource中的Description值。
                    ctInfo.Description = this.DescriptionResource.GetValueForUICulture(this.Web.UICulture);
                    //ctInfo.FieldsSchemaXml = this.Fields.SchemaXml;
                    ctInfo.FieldsSchemaXml = this.GetFieldLinkSchemaXml();
                    ctInfo.DocumentTemplate = this.DocumentTemplate;
                    ctInfo.Group = this.Group;
                    //ctInfo.DisplayFormTemplateName = this.DisplayFormTemplateName;
                    ctInfo.DisplayFormUrl = this.DisplayFormUrl;
                    ctInfo.DocumentTemplateUrl = this.DocumentTemplateUrl;
                    //ctInfo.EditFormTemplateName = this.EditFormTemplateName;
                    ctInfo.EditFormUrl = this.EditFormUrl;
                    ctInfo.Hidden = this.Hidden;
                    ctInfo.SolutionId = this.FeatureId.ToString();
                    ctInfo.NewDocumentControl = this.NewDocumentControl;
                    //ctInfo.NewFormTemplateName = this.NewFormTemplateName;
                    ctInfo.NewFormUrl = this.NewFormUrl;
                    ctInfo.RequireClientRenderingOnNew = this.RequireClientRenderingOnNew;

                    ctInfo.SchemaXml = this.SchemaXml;
                    ctInfo.Scope = this.Scope;
                    //ctInfo.Sealed = ct.Sealed;
                    //ctInfo.Version = ct.Version;
                    ctInfo.ParentName = this.Parent.Name;
                    if (backupParent)
                    {
                        ctInfo.ParentContentTypeInfo = GetParentContentTypeInfo(backupParent);
                    }
                    foreach (string str in this.XmlDocuments)
                    {
                        ctInfo.XmlDocuments.Add(str);
                    }
                    if (!this.ResourceFolderExists)
                    {
                        backUpResourceFolder = false;
                    }
                    if (backUpResourceFolder && WrapperRuntime.CurrentContext.BackupContentTypeDocumentTemplateFile && this.ResourceFolder != null)
                    {
                        ctInfo.ResourceFolder = this.ResourceFolder.ServerRelativeUrl;
                        foreach (AveFile temFile in this.ResourceFolder.Files)
                        {
                            ctInfo.ResourceFolderFiles.Add(new AveContentTypeFileInfo(temFile.Url, temFile.OpenBinary(), temFile.Properties, temFile.TimeCreated, temFile.TimeLastModified));
                        }
                    }
                    ctInfo.NameResource = this.NameResource.GetUserResourceInfo(this.Web);
                    ctInfo.DescriptionResource = this.DescriptionResource.GetUserResourceInfo(this.Web);
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.ContentTypeGetFailed, e);
                }
                return ctInfo;
            }
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
                //如果ContentType很多并且FieldSchema很大, 调用SPContentType.Fields可能引起内存问题
                //对contenttype应用workflow所产生的fieldlink，不应该备份出来
                //if (Fields.Contains(link.ID) && Fields[link.ID].TypeAsString.Equals("WorkflowStatus", StringComparison.OrdinalIgnoreCase))
                //{
                //    continue;
                //}
                try
                {
                    IAveField relativeField = null;
                    if (mParentList != null && mParentList.Fields != null)
                    {
                        if (mParentList.Fields.Contains(link.ID) && !mParentList.Fields[link.ID].TypeAsString.Equals("WorkflowStatus", StringComparison.OrdinalIgnoreCase))
                        {
                            relativeField = mParentList.Fields[link.ID];
                        }
                    }
                    else if (mWeb.AvailableFields.Contains(link.ID) && !mWeb.AvailableFields[link.ID].TypeAsString.Equals("WorkflowStatus", StringComparison.OrdinalIgnoreCase))
                    {
                        relativeField = mWeb.AvailableFields[link.ID];
                    }
                    if (relativeField == null)
                    {//对于filed找不到的fieldlink，在还原的时候还原不了而且还会导致contenttype比较冲突，没必要备份，在此给过滤掉
                        continue;
                    }
                    //fieldLink的hidden，如果ContentType的xml中存在load ContentType的xml，否则load fieldLink中xml
                    if (ContentTypeFieldLinkSchema != null && ContentTypeFieldLinkSchema.ContainsKey(link.ID.ToString("B").ToLower(CultureInfo.InvariantCulture)))
                    {
                        doc.LoadXml(ContentTypeFieldLinkSchema[link.ID.ToString("B").ToLower(CultureInfo.InvariantCulture)].OuterXml);
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
                    logger.Warn("Can not get FieldLink info in contentType: {0}, FieldLink Id: {1}, name: {2}, Exception: {3}", mContentType.Name, link.ID, link.Name, e.ToString());
                }
            }
            return "<Fields>" + builder.ToString() + "</Fields>";

            }

        }

        public void Delete()
        {
            this.ContentType.Delete();
        }

        /// <summary>
        /// 判断fieldLink是否是hidden，如果是fieldLink的xml中有hidden属性就以xml为主，如果没有属性首先要看field是否是hidden的，如果是hidden的话就直接返回true，
        /// 如果field不是hidden的还要继续检查field的readonlyfield属性，如果是true的话，fieldlink也是hidden的！
        /// 目的端需要跟原端保持一致的判断逻辑
        /// </summary>
        /// <param name="fieldLink"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        private bool CheckFieldLinkIsHidden(XmlElement linkXml, IAveField field, IAveFieldLink fieldLink)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveContentType.CheckFieldLinkIsHidden"))
            {

            bool fieldLinkHidden = false;
            bool hasHiddenSchema = linkXml.HasAttribute("Hidden");
            bool convertSuccesfully = false;
            if (hasHiddenSchema)
            {
                convertSuccesfully = bool.TryParse(linkXml.Attributes["Hidden"].Value, out fieldLinkHidden);
            }
            if (!hasHiddenSchema || !convertSuccesfully)
            {
                fieldLinkHidden = field != null ? field.Hidden : fieldLink.Hidden;
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
            AveAssemblyUtility.InvokeMethod(mContentType, typeof(SPContentType), "Initialize", new Type[] { typeof(SPContentType), typeof(SPContentTypeCollection), typeof(string) }, new object[] { mContentType.Parent, (collection as AveContentTypeCollection).ContentTypeCollection, mContentType.Name });
        }

        #region User Resource
        public IAveUserResource NameResource
        {
            get { return new AveUserResource(ContentType.NameResource); }
        }

        public IAveUserResource DescriptionResource
        {
            get { return new AveUserResource(ContentType.DescriptionResource); }
        }
        #endregion

        public string JSLink
        {
            get
            {
                return mContentType.JSLink;
            }
            set
            {
                mContentType.JSLink = value;
            }
        }

        public string MobileDisplayFormUrl
        {
            get
            {
                return mContentType.MobileDisplayFormUrl;
            }
            set
            {
                mContentType.MobileDisplayFormUrl = value;
            }
        }

        public string MobileEditFormUrl
        {
            get
            {
                return mContentType.MobileEditFormUrl;
            }
            set
            {
                mContentType.MobileEditFormUrl = value;
            }
        }

        public string MobileNewFormUrl
        {
            get
            {
                return mContentType.MobileNewFormUrl;
            }
            set
            {
                mContentType.MobileNewFormUrl = value;
            }
        }
    }
    [Serializable]
    class AveContentTypeId : IAveContentTypeId
    {
        private SPContentTypeId mContentTypeId;
        private AveContentTypeId mParent;
        private AveContentTypeId mEmpty;

        public AveContentTypeId()
        { }

        public AveContentTypeId(SPContentTypeId contentTypeId)
        {
            mContentTypeId = contentTypeId;
        }

        public AveContentTypeId(string id)
        {
            mContentTypeId = new SPContentTypeId(id);
        }

        public AveContentTypeId(byte[] id)
        {
            mContentTypeId = (SPContentTypeId)AveAssemblyUtility.CreateInstance(typeof(SPContentTypeId), new Type[] { typeof(byte[]) }, new object[] { id });
        }

        internal SPContentTypeId ContentTypeId
        {
            get
            {
                return mContentTypeId;
            }
        }

        public override string ToString()
        {
            return mContentTypeId.ToString();
        }

        #region IAveContentTypeId Members

        public IAveContentTypeId Parent
        {
            get
            {
                if (mParent == null)
                {
                    SPContentTypeId contentTypeid = mContentTypeId.Parent;
                    if (contentTypeid != null)
                    {
                        mParent = new AveContentTypeId(contentTypeid);
                    }
                }
                return mParent;
            }
        }

        public string TypeId
        {
            get
            {
                return mContentTypeId.GetType().GUID.ToString();
            }
        }

        public bool IsChildOf(IAveContentTypeId id)
        {
            return mContentTypeId.IsChildOf((SPContentTypeId)(id as AveContentTypeId).ContentTypeId);
        }

        public int CompareTo(object obj)
        {
            if (obj is AveContentTypeId)
            {
                return mContentTypeId.CompareTo((obj as AveContentTypeId).mContentTypeId);
            }
            return -1;
        }

        public IAveContentTypeId Empty
        {
            get
            {
                if (mEmpty == null)
                {
                    mEmpty = new AveContentTypeId(SPContentTypeId.Empty);
                }
                return mEmpty;
            }
        }

        public override bool Equals(object obj)
        {
            return (((obj != null) && (obj is AveContentTypeId)) && (this.CompareTo((AveContentTypeId)obj) == 0));
        }

        public override int GetHashCode()
        {
            return this.mContentTypeId.GetHashCode();
        }

        public int Length
        {
            get
            {
                return (int)AveAssemblyUtility.GetPropertyValue(mContentTypeId, "Length");
            }
        }

        public byte[] ToByteArray()
        {
            return (byte[])AveAssemblyUtility.InvokeMethod(mContentTypeId, "ToByteArray", new Type[] { }, new object[] { });
        }

        #endregion
    }
}
