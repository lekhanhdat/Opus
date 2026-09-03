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
using System.Text;
using System.Data;
using System.IO;
using System.Reflection;
using System.Xml;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.I18N;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Resource.Common;

namespace AvePoint.Wrapper.Common
{
    public class AveSPDocumentSet
    {
        private const string DefaultDocument = "http://schemas.microsoft.com/office/documentsets/defaultdocuments";
        private const string AllowedContentTypes = "http://schemas.microsoft.com/office/documentsets/allowedcontenttypes";
        private const string SharedFields = "http://schemas.microsoft.com/office/documentsets/sharedfields";
        private const string WelcomePageFields = "http://schemas.microsoft.com/office/documentsets/welcomepagefields";
        //public const string Forms = "http://schemas.microsoft.com/sharepoint/v3/contenttype/forms";

        private const string CONTENTTYPE = "CONTENTTYPE";
        private const string FIELD = "FIELD";
        private const string ATTR_NAME = "tempName";

        private IAveWeb mAveSPWeb = null;
        private IAveList mAveSPList = null;
        private static IAveSite mAveSPSite = null;
        private AveContentTypeInfo mCTInfo = null;
        private ContentTypeScope mCTScope;

        private AveMappingManager mMappingManager;
        private IAveContentType mCT = null;
        // private AveRestoreOption mOption = null;
        private bool isWebContentTypeUpdate = false;

        public bool NeedUpdate { get; set; }

        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool IsDocumentSet(IAveContentTypeId id)
        {
            while (!AveBuiltInContentTypeId.Contains(id))
            {
                id = id.Parent;
            }
            return AveBuiltInContentTypeId.DocumentSet.Equals(id.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Obsolete("Only for building")]
        public static void ActivateDocumentSetFeature(IAveSite site)
        { }

        public static void ActivateDocumentSetFeature(IAveSite site, IAveWeb web)
        {
            Guid documentSetFeatrueId = new Guid("3BAE86A2-776D-499d-9DB8-FA4CDC7884F8");

            if (site.Features[documentSetFeatrueId] == null)
            {
                try
                {
                    site.Features.Add(documentSetFeatrueId, true);
                    web.AvailableContentTypes.IsDirty = true;
                }
                catch (Exception e)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.ActivateFeatureFailedEventMessage(documentSetFeatrueId, e));
                }
            }
        }

        public AveSPDocumentSet(AveContentTypeInfo _ctInfo, IAveWeb _aveSPWeb)
        {
            NeedUpdate = false;
            mCTInfo = _ctInfo;
            mAveSPWeb = _aveSPWeb;
            mAveSPSite = mAveSPWeb.Site;
            mCTScope = ContentTypeScope.Web;
        }

        public AveSPDocumentSet(AveContentTypeInfo _ctInfo, IAveList _aveSPList)
        {
            NeedUpdate = false;
            mCTInfo = _ctInfo;
            mAveSPWeb = _aveSPList.ParentWeb;
            mAveSPList = _aveSPList;
            mAveSPSite = mAveSPList.ParentWeb.Site;
            mCTScope = ContentTypeScope.List;
        }

        public AveSPDocumentSet(AveContentTypeInfo ctInfo, IAveContentType ct, IAveList aveSPList, AveMappingManager mappingManager, bool isWebContentTypeUpdate)
            : this(ctInfo, aveSPList)
        {
            mCT = ct;
            this.isWebContentTypeUpdate = isWebContentTypeUpdate;
            this.mMappingManager = mappingManager;
        }

        public AveSPDocumentSet(AveContentTypeInfo ctInfo, IAveContentType ct, IAveWeb aveSPWeb, AveMappingManager mappingManager, bool isWebContentTypeUpdate) :
            this(ctInfo, aveSPWeb)
        {
            mCT = ct;
            this.isWebContentTypeUpdate = isWebContentTypeUpdate;
            this.mMappingManager = mappingManager;
        }
        #region For ContentType Restore

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Special URL")]
        public void Update()
        {
            foreach (string str in mCTInfo.XmlDocuments)
            {
                try
                {
                    bool update = false;
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(str);
                    switch (xDoc.DocumentElement.NamespaceURI)
                    {
                        case DefaultDocument:
                            update = UpdateAction(ref xDoc, new UpdateSetting("CONTENTTYPE", "idContentType"));
                            break;

                        case AllowedContentTypes:
                            update = UpdateAction(ref xDoc, new UpdateSetting("CONTENTTYPE", "id"));
                            break;

                        case SharedFields:
                        case WelcomePageFields:
                            update = UpdateAction(ref xDoc, new UpdateSetting("FIELD", "id"));
                            break;

                        default:
                            break;
                    }

                    if (update || string.IsNullOrEmpty(mCT.XmlDocuments[xDoc.DocumentElement.NamespaceURI]))
                    {
                        ReplaceXmlDocument(xDoc);
                        NeedUpdate = true;
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCUpdateDocumentSetError, str, ex.ToString());
                }
            }

            if (NeedUpdate)
            {

                try
                {
                    if(mCTScope == ContentTypeScope.Web)
                    {
                        mCT.Update(isWebContentTypeUpdate);
                    }
                    else
                    {
                        mCT.Update();
                    }
                }
                catch (Exception ex)
                {
                    log.Info("Content type can not update. Exception: {0} ",ex);
                    try
                    {
                        mCT.UpdateIncludingSealedAndReadOnly(isWebContentTypeUpdate);
                    }
                    catch (NotSupportedException notSupportEx)
                    {
                        log.Warn("Office 365 does not support update including sealed and read onle content type. Exception: {0}",notSupportEx);
                    }
                }
                AveAssemblyUtility.InvokeMethod(mCT.ParentWeb.ContentTypes, mCT.ParentWeb.ContentTypes.GetType(), "Update", new object[] { });
            }
        }

        private bool UpdateAction(ref XmlDocument xDoc, UpdateSetting updateSetting)
        {
            bool res = false;

            if (string.Equals(updateSetting.scope, CONTENTTYPE, StringComparison.OrdinalIgnoreCase))
            {
                res = UpdateContentTypeNode(ref xDoc, updateSetting);
            }
            else if (string.Equals(updateSetting.scope, FIELD, StringComparison.OrdinalIgnoreCase))
            {
                res = UpdateFieldsNode(ref xDoc, updateSetting);
            }
            return res;
        }

        private bool UpdateContentTypeNode(ref XmlDocument xDoc, UpdateSetting updateSetting)
        {
            bool res = false;
            IAveWeb web = mCTScope == ContentTypeScope.Web ? mAveSPWeb : mAveSPList.ParentWeb;
            List<XmlElement> needRemoveNodes = new List<XmlElement> { };
            foreach (XmlElement node in xDoc.DocumentElement.ChildElements())
            {
                IAveContentType spCT = null;
                if (node.HasAttribute(ATTR_NAME))
                {
                    string ctName = node.Attributes[ATTR_NAME].Value;
                    while (web != null && (spCT = web.ContentTypes[ctName]) == null)
                    {
                        web = web.ParentWeb;
                    }
                    if (spCT != null)
                    {
                        if (!IsContentTypeAllowedInDocumentSet(spCT))
                        {
                            needRemoveNodes.Add(node);
                        }
                        else
                        {
                            node.Attributes[updateSetting.attributeName].Value = spCT.ID.ToString();
                        }
                        res = true;
                    }
                }
                else if (node.HasAttribute(updateSetting.attributeName))
                {
                    string sourceId = node.Attributes[updateSetting.attributeName].Value;
                    string desId = GetMappingContentTypeId(sourceId);
                    if (!sourceId.Equals(desId, StringComparison.OrdinalIgnoreCase))
                    {
                        node.Attributes[updateSetting.attributeName].Value = desId;
                    }
                    IAveContentTypeId ctId = WrapperRuntime.CurrentContext.ModelFactory.CreateContentTypeId(desId);
                    while (web != null && (spCT = web.ContentTypes[ctId]) == null)
                    {
                        web = web.ParentWeb;
                    }
                    if (spCT != null)
                    {
                        if (!IsContentTypeAllowedInDocumentSet(spCT))
                        {
                            needRemoveNodes.Add(node);
                        }
                        res = true;
                    }
                }
            }
            for (int k = 0; k < needRemoveNodes.Count; k++)
            {
                xDoc.DocumentElement.RemoveChild(needRemoveNodes[k] as XmlNode);
            }
            return res;
        }

        private string GetMappingContentTypeId(string contentTypeId)
        {
            if (!String.IsNullOrEmpty(contentTypeId) && !AveBuiltInContentTypeId.Contains(contentTypeId) && mMappingManager != null)
            {
                IAveContentTypeId mappingId;
                if (mMappingManager.WebMappingManager.WebLevelCTIdMapping.TryGetValue(contentTypeId, out mappingId) && mappingId != null)
                {
                    return mappingId.ToString();
                }
            }
            return contentTypeId;
        }

        private bool IsChild(IAveContentTypeId id, string parentBuildinId)
        {
            IAveContentTypeId tempId = id;
            while (!AveBuiltInContentTypeId.Contains(tempId))
            {
                tempId = tempId.Parent;
            }
            return tempId.IsChildOf(WrapperRuntime.CurrentContext.ModelFactory.CreateContentTypeId(parentBuildinId));
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "documentsets,shadowbacklink are parts of a url")]
        private bool IsShadowContentType(IAveContentType contentType)
        {
            string str = contentType.XmlDocuments["http://schemas.microsoft.com/office/documentsets/shadowbacklink"];
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            return true;
        }

        private bool IsContentTypeAllowedInDocumentSet(IAveContentType type)
        {

            bool allowed = true;
            if ((((mAveSPSite.CompatibilityLevel == 14) && IsChild(type.ID, AveBuiltInContentTypeId.Folder))
                || (((IsChild(type.ID, AveBuiltInContentTypeId.Folder)
                || (type.Group == "_Hidden"))
                || (IsChild(type.ID, AveBuiltInContentTypeId.DocumentSet)
                || IsChild(type.ID, AveBuiltInContentTypeId.UntypedDocument)))
                || (IsChild(type.ID, AveBuiltInContentTypeId.WikiDocument)
                || (!IsChild(type.ID, AveBuiltInContentTypeId.Document) && !IsChild(type.ID, AveBuiltInContentTypeId.Folder)))))
                || IsShadowContentType(type))
            {
                allowed = false;
            }
            return allowed;
        }

        private bool UpdateFieldsNode(ref XmlDocument xDoc, UpdateSetting updateSetting)
        {
            bool res = false;
            if (mCTScope == ContentTypeScope.Web)
            {
                foreach (XmlElement node in xDoc.DocumentElement.ChildElements())
                {
                    if (node.HasAttribute(ATTR_NAME))
                    {
                        string fieldName = node.Attributes[ATTR_NAME].Value;
                        IAveField spField = null;
                        IAveWeb web = mAveSPWeb;
                        while (web != null && (spField = web.Fields[fieldName]) == null)
                        {
                            web = web.ParentWeb;
                        }
                        if (spField != null)
                        {
                            node.Attributes[updateSetting.attributeName].Value = spField.ID.ToString();
                            res = true;
                        }
                    }
                }
            }
            else if (mCTScope == ContentTypeScope.List)
            {
                foreach (XmlElement node in xDoc.DocumentElement.ChildElements())
                {
                    if (node.HasAttribute(ATTR_NAME))
                    {
                        string fieldName = node.Attributes[ATTR_NAME].Value;
                        IAveField spField = mAveSPList.Fields.GetField(fieldName);
                        if (spField != null)
                        {
                            node.Attributes[updateSetting.attributeName].Value = spField.ID.ToString();
                            res = true;
                        }
                    }
                }
            }
            return res;
        }

        private void ReplaceXmlDocument(XmlDocument xDoc)
        {
            foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
            {
                ((XmlElement)node).RemoveAttribute(ATTR_NAME);
            }
            mCT.XmlDocuments.Delete(xDoc.DocumentElement.NamespaceURI);
            mCT.XmlDocuments.Add(xDoc);
        }

        #endregion

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Special URL")]
        public void ReplaceXmlDocuments()
        {
            List<string> XmlDocuments = new List<string>();
            foreach (string xml in mCTInfo.XmlDocuments)
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                switch (xDoc.DocumentElement.NamespaceURI)
                {
                    case DefaultDocument:
                        ReplaceAction(ref xDoc, new ReplaceSetting("CONTENTTYPE", "idContentType"));
                        break;

                    case AllowedContentTypes:
                        ReplaceAction(ref xDoc, new ReplaceSetting("CONTENTTYPE", "id"));
                        break;

                    case SharedFields:
                    case WelcomePageFields:
                        ReplaceAction(ref xDoc, new ReplaceSetting("FIELD", "id"));
                        break;

                    default:
                        break;
                }
                XmlDocuments.Add(xDoc.OuterXml);
            }
            mCTInfo.XmlDocuments = XmlDocuments;

        }

        private void ReplaceAction(ref XmlDocument xDoc, ReplaceSetting replaceSetting)
        {
            if (replaceSetting.scope.Equals(CONTENTTYPE, StringComparison.CurrentCultureIgnoreCase))
            {
                IAveContentTypeCollection ctCollection = null;
                IAveSite aveSPSite = null;
                if (mCTScope == ContentTypeScope.Web)
                {
                    ctCollection = mAveSPWeb.ContentTypes;
                    aveSPSite = mAveSPWeb.Site;
                }
                else if (mCTScope == ContentTypeScope.List)
                {
                    ctCollection = mAveSPList.ContentTypes;
                    aveSPSite = mAveSPList.ParentWeb.Site;
                }
                foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
                {
                    string ctName = aveSPSite.GetWebCTNameById(node.Attributes[replaceSetting.attributeName].Value);
                    //AveSqlQuery.GetWebCTNameById(aveSPSite, node.Attributes[replaceSetting.attributeName].Value);
                    ((XmlElement)node).SetAttribute(ATTR_NAME, ctName);
                }
            }

            else if (replaceSetting.scope.Equals(FIELD, StringComparison.CurrentCultureIgnoreCase))
            {
                if (mCTScope == ContentTypeScope.Web)
                {
                    foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
                    {
                        Guid fieldId = new Guid(node.Attributes[replaceSetting.attributeName].Value);
                        IAveField spField = null;
                        IAveWeb web = mAveSPWeb;
                        //skip dirty fields
                        try
                        {
                            while (web != null && (spField = web.Fields[fieldId]) == null)
                            {
                                web = web.ParentWeb;
                            }
                            //Not sure if we should use static name instead
                            //Need more test
                            string fieldName = spField.Title;
                            ((XmlElement)node).SetAttribute(ATTR_NAME, fieldName);
                        }
                        catch (Exception ex)
                        {
                            log.Debug(string.Format("Skip current web field(Id:{0}) for Document set content type, the field may be deleted already. Message:{1}", fieldId, ex.ToString()));
                        }
                    }
                }
                else if (mCTScope == ContentTypeScope.List)
                {
                    IAveFieldCollection fieldCollection = mAveSPList.Fields;
                    foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
                    {
                        Guid fieldId = new Guid(node.Attributes[replaceSetting.attributeName].Value);
                        //skip dirty fields
                        try
                        {
                            IAveField field = fieldCollection[fieldId];
                            if (field != null)
                            {
                                //Not sure if we should use static name instead
                                //Need more test
                                string fieldName = field.Title;
                                ((XmlElement)node).SetAttribute(ATTR_NAME, fieldName);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Debug(string.Format("Skip current list field(Id:{0}) for Document set content type, the field may be deleted already. Message:{1}", fieldId, ex.ToString()));
                        }
                    }
                }
            }
        }


        public static bool IsDocumentSet(string stringId)
        {
            string parentBuiltinId = FindBuiltinId(stringId);

            return AveBuiltInContentTypeId.DocumentSet.Equals(parentBuiltinId, StringComparison.OrdinalIgnoreCase);
        }

        private static string FindBuiltinId(string id)
        {
            if (!AveBuiltInContentTypeId.Contains(id))
            {
                string childId = GetParentId(id);
                if (string.IsNullOrEmpty(childId))
                {
                    return null;
                }
                return FindBuiltinId(childId);
            }
            else
            {
                return id;
            }
        }

        private static string GetParentId(string id)
        {
            var binaryId = AveSPUtility.ParseContentTypeId(id);
            int length = 0;
            for (int i = 0; i < binaryId.Length; i++)
            {
                length = i;
                if (binaryId[i] == 0)
                {
                    i += 0x10;
                }
            }
            byte[] destinationArray = null;
            if (length > 0)
            {
                destinationArray = new byte[length];
                Array.Copy(binaryId, destinationArray, length);
                string parentId = AveConvert.HexStringFromBytes(destinationArray);
                return parentId;
            }
            return null;
        }
    }

    public class UpdateSetting
    {
        public string attributeName = string.Empty;
        public string scope = string.Empty;

        public UpdateSetting()
        {
        }

        public UpdateSetting(string _scope, string _attributeName)
        {
            attributeName = _attributeName;
            scope = _scope;
        }
    }

    public class ReplaceSetting
    {
        public string attributeName = string.Empty;
        public string scope = string.Empty;

        public ReplaceSetting()
        {
        }

        public ReplaceSetting(string _scope, string _attributeName)
        {
            attributeName = _attributeName;
            scope = _scope;
        }
    }
}
