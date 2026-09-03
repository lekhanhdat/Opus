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
using System.Threading.Tasks;
using AvePoint.Wrapper.Common;
using AveClientRequest.Common;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using AvePoint.Office365.Api;
using System.Xml;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request : AveClientOM2019Request
    {

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetFieldLinks(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string contentTypeId, string contentTypeSource)
        {
            return base.GetFieldLinks(webServerRelativeUrl, listServerRelativeUrl, listTitle, listId, contentTypeId, contentTypeSource);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetRelatedFields(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            return base.GetRelatedFields(webServerRelativeUrl, listTitle, listId);
        }
        [ReplaceByAPI]
        public override Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            Dictionary<string, object> metadataListFieldSettingsProp = new Dictionary<string, object>();
            using (var context = CreateContext())
            {
                try
                {
                    var web = context.Site.OpenWeb(webServerRelativeUrl);
                    var list = string.IsNullOrEmpty(listTitle) ? web.Lists.GetById(listId) : web.Lists.GetByTitle(listTitle);
                    var field = list.Fields.GetById(new Guid("23f27201-bee3-471e-b2e7-b64fd8b7ca38"));
                    context.ExecuteQuery();
                    metadataListFieldSettingsProp["EnableKeywordsField"] = true;
                    metadataListFieldSettingsProp["KeywordsFieldExistsInContentTypes"] = true;
                }
                catch { }
                return metadataListFieldSettingsProp;
            }
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetContentTypes(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource)
        {
            return base.GetContentTypes(webServerRelativeUrl, listName, listId, contentTypeSource);
        }

        [ReplaceByAPI]
        protected override void LoadContentTypes(ClientContext context, ContentTypeCollection contentTypes)
        {
            context.Load(contentTypes, tempContentTypes => tempContentTypes.IncludeWithDefaultProperties(temp => temp.Id, temp => temp.Parent.Id, temp => temp.SchemaXml, temp => temp.SchemaXml, temp => temp.SchemaXmlWithResourceTokens));//cts => cts.IncludeWithDefaultProperties(ct => ct.Fields, ct => ct.FieldLinks));
        }

        [ReplaceByAPIAttribute]
        public override Dictionary<string, object> AddFieldAsXml(string webServerRelativeUrl, string listName, Guid listId, String fieldXml, bool addToDefaultView, int op, string fieldSource, Dictionary<string, object> contentTypeProp)
        {
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                Dictionary<string, object> fieldProperties = new Dictionary<string, object>();
                Field field = null;
                FieldCollection fields = null;
                switch (fieldSource)
                {
                    case "list.fields":
                        List list = web.Lists.GetById(listId);
                        fields = list.Fields;
                        break;
                    case "web.fields":
                        fields = web.Fields;
                        break;
                    //case "web.availablefields":
                    //    field = web.AvailableFields.AddFieldAsXml(fieldXml, addToDefaultView, (AddFieldOptions)op);
                    //    break;
                    //case "contenttype.fields":
                    //    string id = contentTypeProp["Id"] as string;
                    //    string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                    //    ContentType contentType = GetContentTypeWithoutFields(webServerRelativeUrl, listName, contentTypeSource, id);
                    //    field = contentType.Fields.AddFieldAsXml(fieldXml, addToDefaultView, (AddFieldOptions)op);
                    //    break;
                    default:
                        break;
                }
                if (fields != null)
                {
                    field = fields.AddFieldAsXml(fieldXml, addToDefaultView, (AddFieldOptions)op);
                    // the default load can't get the right type.
                    //context.Load(field);
                    context.Load(fields, tempFields => tempFields.IncludeWithDefaultProperties().Where(temp => temp.InternalName == field.InternalName));
                    context.ExecuteQuery();
                    AssembleSingleFieldProperties(fieldProperties, fields[0] as TaxonomyField == null ? fields[0] : fields[0] as TaxonomyField);
                    //如果是TaxonomyFieldType或者TaxonomyFieldTypeMulti要把系统创建的与其关联的Note类型的field load出来
                    if ((fieldProperties["TypeAsString"] != null &&
                        (fieldProperties["TypeAsString"].Equals("TaxonomyFieldType") || fieldProperties["TypeAsString"].Equals("TaxonomyFieldTypeMulti"))) &&
                        fieldProperties.ContainsKey("TextField"))
                    {
                        Guid fieldId = (Guid)fieldProperties["TextField"];
                        Dictionary<string, object> RelatedFieldProperties = GetFieldPropertiesById(webServerRelativeUrl, fieldId, fieldSource, listName, listId);
                        if (RelatedFieldProperties.Count > 0)
                        {
                            fieldProperties.Add("RelatedNoteField", RelatedFieldProperties);
                        }
                    }
                }
                return fieldProperties;
            }
        }

        [KeepOriginalWithAPIAttribute]
        public override Dictionary<string, object> AddContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource, Dictionary<string, object> newContentTypeProperties)
        {
            return base.AddContentType(webServerRelativeUrl, listName, listId, contentTypeSource, newContentTypeProperties);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetFields(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string fieldSource, Dictionary<string, object> contentTypeProp)
        {
            return base.GetFields(webServerRelativeUrl, listServerRelativeUrl, listTitle, listId, fieldSource, contentTypeProp);
        }

        [KeepOriginalWithAPI]
        public override void DeleteAllViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId)
        {
            base.DeleteAllViewField(webServerRelativeUrl, listTitle, listId, viewId);
        }

        [KeepOriginalWithAPI]
        public override void DeleteField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, Dictionary<string, object> contentTypeProp)
        {
            base.DeleteField(webServerRelativeUrl, listName, listId, internalName, fieldSource, contentTypeProp);
        }

        [NoAPI]
        public override List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            return base.GetPublishedContentTypes();
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetTaxonomyCatchAllField(string webServerRelativeUrl, string listName, Guid listId)
        {
            return base.GetTaxonomyCatchAllField(webServerRelativeUrl, listName, listId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetRelatedFieldProperties(string webServerRelativeUrl, string fieldName, string fieldSource, string listTitle, Guid listId)
        {
            return base.GetRelatedFieldProperties(webServerRelativeUrl, fieldName, fieldSource, listTitle, listId);
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties)
        {
            //using (ClientContext context = CreateContext())
            using (ClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                //Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Web web = context.Web;
                FieldCollection fields = null;
                Field field = null;
                bool changed = false;
                ContentType contentType = this.GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, contentTypeId);
                AveObjectCopy.UpdateObjectBasicProperties(needUpdateContentTypeProperties, contentType);
                if (needUpdateContentTypeProperties.ContainsKey("AddFieldLink"))
                {
                    foreach (Dictionary<string, object> fieldLinkProp in needUpdateContentTypeProperties["AddFieldLink"] as List<Dictionary<string, object>>)
                    {
                        bool isNew = fieldLinkProp.ContainsKey("IsNew") ? (bool)fieldLinkProp["IsNew"] : false;
                        if (isNew)
                        {
                            switch (fieldLinkProp["fieldSource"].ToString())
                            {
                                case "web.fields":
                                    fields = web.Fields;
                                    break;
                                case "web.availableFields":
                                    fields = web.AvailableFields;
                                    break;
                                case "list.fields":
                                    List list = web.Lists.GetByTitle(listName);
                                    fields = list.Fields;
                                    break;
                                default:
                                    break;
                            }
                            field = fields.GetById(new Guid(fieldLinkProp["FieldId"].ToString()));
                        }
                        else
                        {
                            ContentType newContentType = GetContentTypeWithoutFields(context, AveUrlUtility.GetServerRelativeUrl(fieldLinkProp["site"].ToString()), fieldLinkProp["ParentList"] == null ? null : fieldLinkProp["ParentList"].ToString(), Guid.Empty, fieldLinkProp["contentTypeSource"].ToString(), fieldLinkProp["Id"].ToString());
                            context.Load(newContentType, c => c.FieldLinks, c => c.Fields);
                            field = newContentType.Fields.GetById(new Guid(fieldLinkProp["FieldId"].ToString()));
                        }
                        AddContentTypeFieldLink(contentType, field, fieldLinkProp);
                        changed = true;
                        //contentType.Update(updateChildren);
                    }
                }

                changed |= UpdateFieldLinkProperties(context, contentType, needUpdateContentTypeProperties, updateChildren);
                changed |= UpdateContentTypeUserResource(contentType, needUpdateContentTypeProperties);

                int propertiesCount = Convert.ToInt32(needUpdateContentTypeProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]);
                Dictionary<string, object> ctProperties = new Dictionary<string, object>();
                if (changed || propertiesCount > 0)
                {
                    contentType.Update(updateChildren);
                    context.Load(contentType);
                    context.Load(contentType, c => c.Parent);
                    context.Load(contentType, c => c.SchemaXml, c => c.SchemaXmlWithResourceTokens);
                    context.ExecuteQuery();
                    this.AssembleSingleContentTypeProperties(ctProperties, contentType);
                }

                string schemaXmlWithRT = null;
                object schemaXml = null;
                Dictionary<string, object> newProperties = null;
                if (ctProperties.TryGetValue("SchemaXmlWithResourceTokens", out schemaXml))
                {
                    schemaXmlWithRT = schemaXml as string;
                }
                bool isDocumentSet = AveSPDocumentSet.IsDocumentSet(contentTypeId);
                mLogger.Info("Update documentset contenttype xmldocument.ContentTypeId:{0},IsDocumentSet:{1},TokenType:{2}", contentTypeId, isDocumentSet, tokenProviders.MainTokenProvider.TokenType);
                if (isDocumentSet && tokenProviders.MainTokenProvider.TokenType != TokenType.Bearer)
                {

                    mWebServiceRequest.UpdateContentType(webServerRelativeUrl, listName, listId, contentTypeId, updateChildren, contentTypeSource, GetNeedUpdateContentTypePropertiesForWebService(needUpdateContentTypeProperties));
                }
                else
                {
                    newProperties = UpdateContentTypeWithSchemaXML(webServerRelativeUrl, listName, contentTypeId, schemaXmlWithRT, updateChildren, needUpdateContentTypeProperties);
                }
                if (newProperties != null && newProperties.Count > 0)
                {
                    return newProperties;
                }
                return ctProperties;
            }
        }

        private Dictionary<string, object> UpdateContentTypeWithSchemaXML(string webServerRelativeUrl, string listName, string contentTypeId, string schemaXml, bool updateChildren, Dictionary<string, object> needUpdateContentTypeProperties)
        {
            Dictionary<string, object> ctProperties = new Dictionary<string, object>();

            if (needUpdateContentTypeProperties.ContainsKey("NewDocumentControl") ||
                needUpdateContentTypeProperties.ContainsKey("RequireClientRenderingOnNew") ||
                needUpdateContentTypeProperties.ContainsKey("DeletedDocuments") ||
                needUpdateContentTypeProperties.ContainsKey("AddedDocuments"))
            {

                if (string.IsNullOrEmpty(schemaXml))
                {
                    using (AveClientContext context = CreateContext())
                    {
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        ContentType contentType = null;
                        if (string.IsNullOrEmpty(listName))
                        {
                            contentType = web.ContentTypes.GetById(contentTypeId);
                        }
                        else
                        {
                            contentType = web.Lists.GetByTitle(listName).ContentTypes.GetById(contentTypeId);
                        }

                        context.Load(contentType, c => c.SchemaXml, c => c.SchemaXmlWithResourceTokens);
                        context.ExecuteQuery();
                        if (!string.IsNullOrEmpty(contentType.SchemaXmlWithResourceTokens))
                        {
                            schemaXml = contentType.SchemaXmlWithResourceTokens;
                        }
                        else
                        {
                            schemaXml = contentType.SchemaXml;
                        }
                    }
                }

                var document = new XmlDocument();
                document.LoadXml(schemaXml);

                var changed = false;

                object keyValue;

                if (needUpdateContentTypeProperties.TryGetValue("NewDocumentControl", out keyValue))
                {
                    document.DocumentElement.SetAttribute("NewDocumentControl", keyValue != null ? keyValue.ToString() : string.Empty);
                    changed = true;
                }

                if (needUpdateContentTypeProperties.TryGetValue("RequireClientRenderingOnNew", out keyValue))
                {
                    document.DocumentElement.SetAttribute("RequireClientRenderingOnNew", keyValue != null ? keyValue.ToString() : "false");
                    changed = true;
                }

                var xmlDocuments = document.SelectSingleNode("/ContentType/XmlDocuments");

                if (needUpdateContentTypeProperties.TryGetValue("DeletedDocuments", out keyValue))
                {
                    if (xmlDocuments != null)
                    {
                        var list = keyValue as List<string>;
                        if (list != null)
                        {
                            List<XmlNode> deletedNodes = new List<XmlNode>();
                            foreach (XmlNode node in xmlDocuments.ChildNodes)
                            {
                                var namespaceUri = node.Attributes["NamespaceURI"].Value;
                                if (list.Contains(namespaceUri))
                                {
                                    deletedNodes.Add(node);
                                }
                            }

                            foreach (var node in deletedNodes)
                            {
                                changed = true;
                                xmlDocuments.RemoveChild(node);
                            }
                        }
                    }
                }

                if (needUpdateContentTypeProperties.TryGetValue("AddedDocuments", out keyValue))
                {
                    var list = keyValue as Dictionary<string, string>;

                    if (list != null && list.Count > 0)
                    {
                        Dictionary<string, XmlNode> nodeMapping = null;
                        if (xmlDocuments == null)
                        {
                            xmlDocuments = document.CreateElement("XmlDocuments");
                            document.DocumentElement.AppendChild(xmlDocuments);
                        }
                        else if (xmlDocuments.ChildNodes.Count > 0)
                        {
                            nodeMapping = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
                            foreach (XmlNode node in xmlDocuments.ChildNodes)
                            {
                                nodeMapping[node.Attributes["NamespaceURI"].Value] = node;
                            }
                        }

                        foreach (var item in list)
                        {
                            XmlNode node;
                            if (nodeMapping != null && nodeMapping.TryGetValue(item.Key, out node))
                            {
                                xmlDocuments.RemoveChild(node);
                            }

                            var xmlDocument = document.CreateElement("XmlDocument");
                            xmlDocument.SetAttribute("NamespaceURI", item.Key);
                            xmlDocument.InnerXml = item.Value;
                            xmlDocuments.AppendChild(xmlDocument);
                        }

                        changed = true;
                    }
                }

                if (changed)
                {
                    schemaXml = document.OuterXml;

                    using (AveClientContext context = CreateContext())
                    {
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        ContentType contentType = null;
                        if (string.IsNullOrEmpty(listName))
                        {
                            contentType = web.ContentTypes.GetById(contentTypeId);
                        }
                        else
                        {
                            contentType = web.Lists.GetByTitle(listName).ContentTypes.GetById(contentTypeId);
                        }

                        contentType.SchemaXmlWithResourceTokens = schemaXml;
                        contentType.Update(updateChildren);
                        context.Load(contentType);
                        context.Load(contentType, c => c.Parent);
                        context.Load(contentType, c => c.SchemaXml);
                        context.Load(contentType, c => c.SchemaXmlWithResourceTokens);
                        context.Load(contentType, c => c.WorkflowAssociations);
                        context.ExecuteQuery();
                        AssembleSingleContentTypeProperties(ctProperties, contentType);
                    }
                }
            }

            return ctProperties;
        }

        [ReplaceByAPI]
        public override void MoveFieldTo(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string field, int index)
        {
            base.MoveFieldTo(webServerRelativeUrl, listTitle, listId, viewId, field, index);
        }

        [ReplaceByAPI]
        public override Dictionary<string, string> GetContentTypeUserResource(string webServerRelativeUrl, Guid listId, string resourceName, string contentTypeResourceName, string contentTypeId, List<string> cultureNames)
        {
            using (AveClientContext context = CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl))
            {
                Dictionary<string, ClientResult<string>> values = new Dictionary<string, ClientResult<string>>();
                UserResource resource;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ContentTypeCollection contentTypes = null;
                switch (contentTypeResourceName)
                {
                    case "web.availableContentTypes":
                        contentTypes = web.AvailableContentTypes;
                        break;
                    case "web.contentTypes":
                        contentTypes = web.ContentTypes;
                        break;
                    case "list.contentTypes":
                        List list = web.Lists.GetById(listId);
                        contentTypes = list.ContentTypes;
                        break;
                    default:
                        break;
                };
                ObjectPath path = new ObjectPathMethod(context, contentTypes.Path, "GetById", new object[] { contentTypeId });
                ContentType ct = new ContentType(context, path);
                ClientResult<string> result = new ClientResult<string>();
                switch (resourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        resource = ct.NameResource;
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        resource = ct.DescriptionResource;
                        break;
                    default:
                        throw new NotSupportedException(string.Format("The resource {0} is not supported.", resourceName));
                }
                foreach (string cultureName in cultureNames)
                {
                    values.Add(cultureName, resource.GetValueForUICulture(cultureName));
                }
                context.ExecuteQuery();
                return values.ToDictionary(k => k.Key, v => v.Value.Value);
            }
        }

        [ReplaceByAPI]
        public override Dictionary<string, string> GetFieldUserResource(string webServerRelativeUrl, Guid listId, string resourceName, string fieldResourceName, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldProp, List<string> cultureNames)
        {
            using (AveClientContext context = CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl))
            {
                Dictionary<string, ClientResult<string>> values = new Dictionary<string, ClientResult<string>>();
                UserResource resource;
                Web web = context.Web;
                FieldCollection fields = null;
                switch (fieldResourceName)
                {
                    case "web.fields":
                        fields = web.Fields;
                        break;
                    case "web.availableFields":
                        fields = web.AvailableFields;
                        break;
                    case "list.fields":
                        List list = web.Lists.GetById(listId);
                        fields = list.Fields;
                        break;
                    case "contentType.fields":
                        string id = contentTypeProp["ContentTypeId"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, string.Empty, listId, contentTypeSource, id);
                        fields = contentType.Fields;
                        break;
                    default:
                        break;
                }
                Guid fieldId = GetFieldIdFromIdentity(fieldProp["ObjectPath"].ToString());
                ObjectPath path = new ObjectPathMethod(context, fields.Path, "GetById", new object[] { fieldId });
                Field field = Activator.CreateInstance(fieldProp["FieldType"] as Type, new object[] { context, path }) as Field;

                ClientResult<string> result = new ClientResult<string>();
                switch (resourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        resource = field.TitleResource;
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        resource = field.DescriptionResource;
                        break;
                    default:
                        throw new NotSupportedException(string.Format("The resource {0} is not supported.", resourceName));
                }
                foreach (string cultureName in cultureNames)
                {
                    values.Add(cultureName, resource.GetValueForUICulture(cultureName));
                }
                context.ExecuteQuery();
                return values.ToDictionary(k => k.Key, v => v.Value.Value);
            }
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetFieldValueAsTaxonomyFieldValue(string webRelativeUrl, Guid listId, Guid fieldId, string text)
        {
            return base.GetFieldValueAsTaxonomyFieldValue(webRelativeUrl, listId, fieldId, text);
        }


    }
}
