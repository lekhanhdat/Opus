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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ClientOM
{
    public abstract class AveWebPartPropertyUpdater
    {
        public AveWebPartCache Cache = null;
        public AveWebPartLinkUpdater LinkUpdater;
        protected static AveLogger mLogger = AveLogger.GetInstance(typeof(AveWebPartPropertyUpdater));
        protected IAveWeb mWeb;

        protected AveWebPartPropertyUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web = null)
        {
            Cache = webPartCache;
            LinkUpdater = webPartLinkUpdater;
            mWeb = web;
        }

        public bool IsGuid(string strId)
        {
            if (string.IsNullOrEmpty(strId))
            {
                return false;
            }
            strId = strId.Trim();
            if (strId.Length < 0x20)
            {
                return false;
            }
            if (strId.Contains("x") || strId.Contains("X"))
            {
                strId = strId.Replace(" ", "");
                return Regex.IsMatch(strId, @"^\{0[x|X][a-fA-F\d]{8},(0[x|X][a-fA-F\d]{4},){2}\{(0[x|X][a-fA-F\d]{2},){7}0[x|X][a-fA-F\d]{2}\}\}$", RegexOptions.Compiled);
            }
            return Regex.IsMatch(strId, @"^([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}|\([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\)|\{[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\}|[a-fA-F\d]{32})$", RegexOptions.Compiled);
        }

        protected void UpdateLink()
        {
            LinkUpdater.UpdateLink(Cache);
        }

        public bool UpdateListIdNodes(AveWebPartBaseInfo webPartInfo, XmlDocument definationXmlDoc, string[] nodeNames)
        {
            bool needPostRestore = false;
            for (int i = 0; i < nodeNames.Length; i++)
            {
                string xpath = string.Format(".//*[@name = '{0}']", nodeNames[i]);
                Guid originListId = Guid.Empty;
                if (needPostRestore = !UpdateXmlProperties(definationXmlDoc, xpath, ref originListId))//更新Xml属性失败,则需要Post Restore.
                {
                    webPartInfo.ListId = webPartInfo.ListId.Equals(Guid.Empty) ? originListId : webPartInfo.ListId;
                    break;
                }
            }
            return needPostRestore;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="definationXmlDoc"></param>
        /// <param name="xpath"></param>
        /// <returns>If can't find the aimed list, return false, otherwise return true.</returns>
        protected bool UpdateXmlProperties(XmlDocument definationXmlDoc, string xpath, ref Guid originlistId)
        {
            XmlNode listNode = definationXmlDoc.SelectSingleNode(xpath);
            if (listNode != null && IsGuid(listNode.InnerText)
                && !listNode.InnerText.Equals(Guid.Empty.ToString()))
            {
                Guid listId = new Guid(listNode.InnerText);
                Guid destinationListId;
                if (!this.Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out destinationListId))
                {
                    originlistId = listId;
                    return false;
                }
                listNode.InnerText = destinationListId.ToString("B");
            }
            return true;
        }
        //public abstract void UpdateRelativeUrl(XmlDocument definationXmlDoc);

        //Need move to drive class
        protected bool UpdateLibraryGuid(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode libNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'LibraryGuid']");
            if (libNode != null)
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                if (oldLibId.Equals(Guid.Empty))
                {
                    return needPostRestore;
                }
                Guid destinationListId;
                if (Cache.SiteMappingManager.GetValueFromListIdMapping(oldLibId, out destinationListId))
                {
                    libNode.InnerText = destinationListId.ToString();
                    XmlNode viewIdNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'ViewGuid']");
                    Guid viewGuidMappingValue;
                    if (Cache.SiteMappingManager.GetViewGuidMappingValue(new Guid(viewIdNode.InnerText), out viewGuidMappingValue))
                    {
                        viewIdNode.InnerText = viewGuidMappingValue.ToString();
                    }
                }
                else
                {
                    webpartInfo.ListId = oldLibId;
                    needPostRestore = true;
                }
            }
            return needPostRestore;
        }

        protected void ReplaceRelativeInfo(XmlDocument informationDoc, List<string> properties)
        {
            foreach (XmlElement propertyEle in informationDoc.DocumentElement.GetElementsByTagName("property"))
            {
                if (propertyEle.HasAttribute("name") && properties.Contains(propertyEle.GetAttribute("name")))
                {
                    string urlNeedReplace = propertyEle.InnerText;
                    string replaceUrl = AveReplaceProcessor.UrlReplace(urlNeedReplace, Cache.SiteManagedMappings, new ReplaceOption(true, true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl);
                    propertyEle.InnerText = replaceUrl;
                }
            }
        }
        protected virtual void HandleSPMWebPart(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            return;
        }
        protected virtual bool IsDependentObjectRestored(XmlDocument webpartDoc)
        {
            return true;
        }

        public abstract bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc);

        /// <summary>
        /// 这个方法是remove掉不需要的property，不必要时不要使用
        /// </summary>
        /// <param name="informationDoc"></param>
        /// <param name="properties"></param>
        public void RemoveNeedRemoveNode(XmlDocument informationDoc, List<string> properties)
        {
            foreach (XmlNode xmlNode in informationDoc.DocumentElement.GetElementsByTagName("properties"))
            {
                List<XmlNode> needRemoveList = new List<XmlNode>();
                foreach (XmlNode xmlNode1 in xmlNode)
                {
                    XmlElement element = xmlNode1 as XmlElement;
                    if (element.HasAttribute("name") && properties.Contains(element.GetAttribute("name")))
                    {
                        needRemoveList.Add(xmlNode1);
                    }
                }
                foreach (XmlNode node in needRemoveList)
                {
                    xmlNode.RemoveChild(node);
                }
            }
        }

        protected void ConvertV3DefinitionXmlToV2(AveWebPartBaseInfo webpartInfo, XmlNode root)
        {
            string definitionXml = string.Empty;
            List<string> excludeProperties = new List<string>() { "AllowZoneChange", "AllowEdit", "AllowConnect", "AllowHide", "AllowMinimize" };
            try
            {
                XmlElement tempRoot = root.OwnerDocument.CreateElement("WebPart");
                XmlAttribute tempRootAttibute = root.OwnerDocument.CreateAttribute("xmlns");
                tempRootAttibute.Value = "http://schemas.microsoft.com/WebPart/v2";
                tempRoot.Attributes.Append(tempRootAttibute);
                XmlElement propertiesNode = root.SelectSingleNode(".//*[name() = 'properties']") as XmlElement;
                if (propertiesNode != null)
                {
                    //add WebPart properties into the v2 format definitionXml
                    foreach (XmlNode property in propertiesNode.ChildNodes)
                    {
                        string propertyName = property.Attributes["name"].Value;
                        if (excludeProperties.Contains(propertyName))
                        {
                            continue;
                        }
                        XmlNode tempNode = root.OwnerDocument.CreateElement(propertyName);
                        tempNode.InnerText = property.InnerText;
                        tempRoot.AppendChild(tempNode);
                    }
                }
                #region Add WebPart type and assembly into definitionXml
                XmlElement typeNode = root.SelectSingleNode(".//*[name()='type']") as XmlElement;
                if (typeNode != null)
                {
                    string assembly = typeNode.Attributes["name"].Value;
                    XmlNode assemblyNode = root.OwnerDocument.CreateElement("Assembly");
                    assemblyNode.InnerText = assembly;
                    tempRoot.AppendChild(assemblyNode);
                    string[] splitAssembly = assembly.Split(',');
                    if (splitAssembly.Length > 0)
                    {
                        string webPartType = splitAssembly[0];
                        XmlNode webPartTypeNode = root.OwnerDocument.CreateElement("TypeName");
                        webPartTypeNode.InnerText = webPartType;
                        tempRoot.AppendChild(webPartTypeNode);
                    }
                }
                #endregion
                #region add WebPart ZoneID,PartOrder,IsIncluded,WebPartIdProperty into definitionXml
                XmlElement temp = null;
                temp = root.SelectSingleNode(".//*[name()='ZoneID']") as XmlElement;
                if (temp != null)
                {
                    XmlNode zoneIDNode = root.OwnerDocument.CreateElement("ZoneID");
                    zoneIDNode.InnerText = temp.InnerText;
                    tempRoot.AppendChild(zoneIDNode);
                }
                temp = root.SelectSingleNode(".//*[name()='PartOrder']") as XmlElement;
                if (temp != null)
                {
                    XmlNode partOrderNode = root.OwnerDocument.CreateElement("PartOrder");
                    partOrderNode.InnerText = temp.InnerText;
                    tempRoot.AppendChild(partOrderNode);
                }
                #endregion
                definitionXml = tempRoot.OuterXml;
            }
            catch (Exception ex)
            {
                mLogger.Debug("An error occurred while convert v3 WebPart definitionXml to v2 WebPart for TagCloudWebPart.Message:{0}.", ex.ToString());
                definitionXml = root.OuterXml;
            }
            root.OwnerDocument.InnerXml = definitionXml;
        }
    }

    #region Webpart Instance
    public class SiteDocumentsWebPart : AveWebPartPropertyUpdater
    {
        public SiteDocumentsWebPart(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater) :
            base(webPartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            ReplaceUserTabs(definationXmlDoc);
            return false;
        }
        private void ReplaceUserTabs(XmlDocument definationXmlDoc)
        {
            string xPath = ".//*[name()='UserTabs']";
            XmlNode node = definationXmlDoc.SelectSingleNode(xPath);
            if (node != null && !string.IsNullOrEmpty(node.InnerText))
            {
                XmlDocument innerDoc = new XmlDocument();
                innerDoc.LoadXml(node.InnerText);
                XmlNodeList pairNodes = innerDoc.SelectNodes(".//*[name()='Pair']");
                if (pairNodes != null)
                {
                    foreach (XmlElement pairNode in pairNodes.OfType<XmlElement>())
                    {
                        if (pairNode.HasAttribute("Url") && !string.IsNullOrEmpty(pairNode.Attributes["Url"].Value))
                        {
                            pairNode.Attributes["Url"].Value = AveReplaceProcessor.UrlReplace(pairNode.Attributes["Url"].Value, this.Cache.SiteManagedMappings, new ReplaceOption(true, true), this.Cache.SourceSiteInfo, this.Cache.DestSiteInfo.ServerRelativeUrl);
                        }
                    }
                }
                node.InnerText = innerDoc.InnerXml;
            }
        }
    }

    public class TimeLineWebPart : AveWebPartPropertyUpdater
    {
        public TimeLineWebPart(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web) :
            base(webPartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            return UpdateListIdNodes(webpartInfo, definationXmlDoc, new string[] { "SourceSelection", "ListId" });
        }
    }

    public class ContactDetailWebPart : AveWebPartPropertyUpdater
    {
        public ContactDetailWebPart(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web) :
            base(webPartCache, webPartLinkUpdater, web) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            UpdateContact(definationXmlDoc);
            return false;
        }
        public bool UpdateContact(XmlDocument definationXmlDoc)
        {
            bool needUpdate = false;
            try
            {
                mLogger.Debug("The webpart xml is " + definationXmlDoc.OuterXml);
                string tempxml = definationXmlDoc.OuterXml;
                Regex regexContact = new Regex("<Contact.*?>[0-9]*?</Contact>");
                Match matchContact = regexContact.Match(tempxml);
                if (matchContact.Success)
                {
                    int startIndex = matchContact.Value.IndexOf(">", StringComparison.OrdinalIgnoreCase);
                    string userIDString = matchContact.Value.Substring(startIndex + 1, matchContact.Value.Length - 10 - startIndex - 1);
                    if (!string.IsNullOrEmpty(userIDString))
                    {
                        int userID = int.Parse(userIDString);
                        int newUserID = -1;
                        if (Cache.SiteUserIDMapping.ContainsKey(userID) && userID != -1)
                        {
                            object obj = Cache.SiteUserIDMapping[userID];
                            if (obj != null && obj.GetType().Name.Equals("AveSPMemberInfo"))
                            {
                                newUserID = (int)AveAssemblyUtility.GetFieldValue(obj, "NewId");
                            }
                        }
                        if (newUserID != -1)
                        {
                            needUpdate = true;
                            string newXml = matchContact.Value.Substring(0, startIndex + 1) + newUserID.ToString() + "</Contact>";
                            tempxml = tempxml.Replace(matchContact.Value, newXml);
                            definationXmlDoc.LoadXml(tempxml);
                        }
                    }
                }
                else
                {
                    Regex regexLoginName = new Regex("<ContactLoginName.*?>.*?</ContactLoginName>");
                    Match matchLoginName = regexLoginName.Match(tempxml);
                    if (matchLoginName.Success)
                    {
                        int startIndex = matchLoginName.Value.IndexOf(">", StringComparison.OrdinalIgnoreCase);
                        string userNameString = matchLoginName.Value.Substring(startIndex + 1, matchLoginName.Value.Length - 19 - startIndex - 1);
                        string newName = string.Empty;
                        if (!string.IsNullOrEmpty(userNameString))
                        {
                            string logonName = userNameString.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                            if (Cache.SiteUserNameMapping.ContainsKey(logonName))
                            {
                                newName = Cache.SiteUserNameMapping[logonName];
                            }
                            if (!string.IsNullOrEmpty(newName))
                            {
                                needUpdate = true;
                                string newXml = matchLoginName.Value.Substring(0, startIndex + 1) + newName + "</ContactLoginName>";
                                tempxml = tempxml.Replace(matchLoginName.Value, newXml);
                                definationXmlDoc.LoadXml(tempxml);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Debug("An error occurred while updating contact: " + e.Message + e.StackTrace);
            }
            return needUpdate;
        }
    }

    public class TagCloudWebPartUpdater : AveWebPartPropertyUpdater
    {
        public TagCloudWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater) :
            base(webPartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            //ConvertV3DefinitionXmlToV2(webpartInfo, definationXmlDoc.FirstChild);
            return UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }
    }
    public class SocialCommentWebPartUpdater : AveWebPartPropertyUpdater
    {
        public SocialCommentWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater) :
            base(webPartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            ConvertV3DefinitionXmlToV2(webpartInfo, definationXmlDoc.FirstChild);
            return UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }
    }
    public class TableOfContentsWebPartUpdater : AveWebPartPropertyUpdater
    {
        public TableOfContentsWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web) { }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            HandleSPMWebPart(webpartInfo, definationXmlDoc);
            UpdateLink();
            UpdateRelativeUrl(definationXmlDoc);
            return UpdateLibraryGuid(webpartInfo, definationXmlDoc) || UpdateListIdNodes(webpartInfo, definationXmlDoc, new string[] { "ListName", "ListId" });
        }

        private void UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            List<string> replaceProperties = new List<string>();
            replaceProperties.Add("AnchorLocation");
            ReplaceRelativeInfo(definationXmlDoc, replaceProperties);
        }

        protected override void HandleSPMWebPart(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            if (mWeb.Site.CompatibilityLevel != 15)
            {
                return;
            }
            XmlNode node = definationXmlDoc.SelectSingleNode(".//*[@name = 'Level1Style']");
            if (node != null && !string.IsNullOrEmpty(node.InnerText) && node.InnerText == "VerticalBold")
            {
                node.InnerText = "Vertical";
            }
        }
    }

    public class ContentEditorWebPartUpdater : AveWebPartPropertyUpdater
    {
        public ContentEditorWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webPartCache, webPartLinkUpdater)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            UpdateRelativeUrl(definationXmlDoc);
            return UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }

        private void UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            foreach (XmlElement propertyEle in definationXmlDoc.DocumentElement.GetElementsByTagName("Content"))
            {
                #region old code   ADO-60521
                //HtmlDocument replaceDocument = new HtmlDocument();
                //if (propertyEle.FirstChild == null || string.IsNullOrEmpty(propertyEle.FirstChild.InnerText))
                //{
                //    continue;
                //}
                //replaceDocument.LoadHtml(propertyEle.FirstChild.InnerText);
                //Dictionary<string, string> linkNodes = new Dictionary<string, string>();
                //linkNodes["//img"] = "src";
                //linkNodes["//a"] = "href";
                ////linkNodes["//button"] = "onclick";
                //foreach (KeyValuePair<string, string> linkNode in linkNodes)
                //{
                //    UpdateLinkUrl(replaceDocument, propertyEle, linkNode.Key, linkNode.Value);
                //}
                #endregion
                ReplaceContentLinks(propertyEle);
            }
        }

        private void UpdateLinkUrl(HtmlDocument replaceDocument, XmlElement propertyEle, string rootNodeName, string linkNodeName)
        {
            HtmlNodeCollection linkNodes = replaceDocument.DocumentNode.SelectNodes(rootNodeName);
            if (linkNodes != null)
            {
                foreach (HtmlNode linkNode in linkNodes)
                {
                    if (!string.IsNullOrEmpty(linkNode.GetAttributeValue(linkNodeName, string.Empty)))
                    {
                        string urlNeedReplace = linkNode.GetAttributeValue(linkNodeName, string.Empty);
                        string replaceUrl = AveReplaceProcessor.UrlReplace(urlNeedReplace, Cache.SiteManagedMappings, new ReplaceOption(true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl);
                        linkNode.SetAttributeValue(linkNodeName, replaceUrl);
                    }
                }
                propertyEle.FirstChild.InnerText = replaceDocument.DocumentNode.OuterHtml;
            }
        }

        private XmlElement ReplaceContentLinks(XmlElement xe)
        {
            try
            {
                foreach (XmlNode node in xe.GetElementsByTagName("a"))
                {
                    node.Attributes["href"].Value = AveReplaceProcessor.UrlReplace(node.Attributes["href"].Value, Cache.SiteManagedMappings, new ReplaceOption(true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl);
                }
                foreach (XmlNode node in xe.GetElementsByTagName("img"))
                {
                    node.Attributes["src"].Value = AveReplaceProcessor.UrlReplace(node.Attributes["src"].Value, Cache.SiteManagedMappings, new ReplaceOption(true, true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl);
                }
                foreach (XmlNode node in xe.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.CDATA)
                    {
                        string innerText = AveReplaceProcessor.ReplaceStringLinks(node.InnerText, Cache.SiteManagedMappings, new ReplaceOption(true, true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl);
                        node.InnerText = innerText;
                    }
                }
                return xe;
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.DEBUG, "An error occurred while replacing content, file: {0}. Reason: {1}", string.Empty, ex);
                return xe;
            }
        }
    }

    public class ExcelWebRendererWebPartUpdater : AveWebPartPropertyUpdater
    {
        public ExcelWebRendererWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webPartCache, webPartLinkUpdater)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            UpdateRelativeUrl(definationXmlDoc);
            return UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }

        private void UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            XmlNode node = definationXmlDoc.FirstChild.SelectSingleNode(".//*[@name='TitleUrl']");
            if (node != null && !string.IsNullOrEmpty(node.InnerText))
            {
                node.InnerText = "";
            }
            //List<string> replaceProperties = new List<string>();
            //replaceProperties.Add("WorkbookUri");
            //replaceProperties.Add("TitleIconImageUrl");
            //replaceProperties.Add("CatalogIconImageUrl");
            //replaceProperties.Add("HelpUrl");
            //ReplaceRelativeInformation(definationXmlDoc, replaceProperties);
            //replaceProperties.Clear();
            //replaceProperties.Add("TitleUrl");
            //RemoveNeedRemoveNode(definationXmlDoc, replaceProperties);
        }
    }

    public class VisioWebAccessWebPartUpdater : AveWebPartPropertyUpdater
    {
        public VisioWebAccessWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webPartCache, webPartLinkUpdater)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            UpdateRelativeUrl(definationXmlDoc);
            return UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }

        private void UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            List<string> replaceProperties = new List<string>();
            replaceProperties.Add("DiagramPath");
            ReplaceRelativeInfo(definationXmlDoc, replaceProperties);
        }
    }

    public class DataFormWebPartUpdater : AveWebPartPropertyUpdater
    {
        public DataFormWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        private bool UpdateWebUrl(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            Dictionary<string, List<string>> bindingAndDataSourceDict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            string xpath = string.Format(".//*[@name = 'ParameterBindings']");
            foreach (XmlNode parameterBindingsNode in definationXmlDoc.SelectNodes(xpath))
            {
                XmlDocument docBinding = new XmlDocument();
                docBinding.LoadXml(string.Format("<root>{0}</root>", parameterBindingsNode.InnerText));
                foreach (XmlElement node in docBinding.GetElementsByTagName("ParameterBinding"))
                {
                    string strName = node.GetAttribute("Name");
                    string value = node.GetAttribute("DefaultValue");
                    if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(value))
                    {
                        if (!bindingAndDataSourceDict.ContainsKey(strName))
                        {
                            List<string> temp = new List<string>();
                            temp.Add(value);
                            bindingAndDataSourceDict.Add(strName, temp);
                        }
                        else if (!bindingAndDataSourceDict[strName].Contains(value))
                        {
                            bindingAndDataSourceDict[strName].Add(value);
                        }
                    }
                }
            }
            xpath = string.Format(".//*[@name = 'DataSourcesString']");
            foreach (XmlNode node in definationXmlDoc.SelectNodes(xpath))
            {
                XmlDocument docDataSource = new XmlDocument();
                var dataSourceString = node.InnerText;
                if(string.IsNullOrEmpty(dataSourceString))
                {
                    continue;
                }
                int index = dataSourceString.LastIndexOf("%>", StringComparison.Ordinal) + 2;
                List<string> tagNames = GetTagNames(dataSourceString.Substring(0, index));
                dataSourceString = dataSourceString.Substring(index);
                dataSourceString = dataSourceString.Replace(':', '_');
                docDataSource.LoadXml("<root>" + dataSourceString + "</root>");
                foreach (string tagName in tagNames)
                {
                    foreach (XmlElement tempNode in docDataSource.GetElementsByTagName(tagName))
                    {
                        string strName = tempNode.GetAttribute("Name");
                        string value = tempNode.GetAttribute("DefaultValue");
                        if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(value))
                        {
                            if (!bindingAndDataSourceDict.ContainsKey(strName))
                            {
                                List<string> temp = new List<string>();
                                temp.Add(value);
                                bindingAndDataSourceDict.Add(strName, temp);
                            }
                            else if (!bindingAndDataSourceDict[strName].Contains(value))
                            {
                                bindingAndDataSourceDict[strName].Add(value);
                            }
                        }
                    }
                }
            }
            if (bindingAndDataSourceDict.ContainsKey("WebURL"))
            {
                foreach (string webUrl in bindingAndDataSourceDict["WebURL"])
                {
                    string dKey = "\"" + webUrl + "\"";
                    string dValue = "\"" + AveReplaceProcessor.UrlReplace(webUrl, this.Cache.SiteManagedMappings, new ReplaceOption(true),
                        this.Cache.SourceSiteInfo, this.Cache.DestSiteInfo.ServerRelativeUrl) + "\"";
                    if (!dKey.Equals(dValue, StringComparison.OrdinalIgnoreCase))
                    {
                        webpartInfo.DefinitionXml = webpartInfo.DefinitionXml.Replace(dKey, dValue);
                    }
                }
            }
            return false;
        }

        private List<string> GetTagNames(string dataSourceString)
        {
            string startSymbol = "<%@ Register TagPrefix=\"";
            string endSymbol = "\" Namespace=\"";
            List<string> tags = new List<string> { "asp_Parameter" };
            try
            {
                int startIndex = dataSourceString.IndexOf(startSymbol, StringComparison.Ordinal);
                int endIndex = dataSourceString.IndexOf(endSymbol, StringComparison.Ordinal);
                while (startIndex > -1)
                {
                    startIndex += startSymbol.Length;
                    if (endIndex > startIndex)
                    {
                        string prefix = dataSourceString.Substring(startIndex, endIndex - startIndex);
                        if (!prefix.Equals("sharepoint", StringComparison.OrdinalIgnoreCase))
                        {
                            tags.Add(prefix + "_DataFormParameter");
                        }
                    }
                    startIndex = dataSourceString.IndexOf(startSymbol, startIndex, StringComparison.Ordinal);
                    endIndex = dataSourceString.IndexOf(endSymbol, endIndex + endSymbol.Length, StringComparison.Ordinal);
                }
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.WARN, "Failed to get tag prefix for DataFormWebPart. Error:{0}", ex.ToString());
            }
            return tags;
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            //if (mWeb.Lists.GetByTitle(webpartInfo.ListTitle) == null)
            //{
            //    return true;
            //}
            //if (webpartInfo.OriginalListId != Guid.Empty && webpartInfo.OriginalListId != webpartInfo.ListId)
            //{
            //    webpartInfo.DefinitionXml = webpartInfo.DefinitionXml.Replace(webpartInfo.OriginalListId.ToString().ToUpper(), webpartInfo.ListId.ToString());
            //}
            //else if (this.Cache.ListIdMapping.ContainsKey(webpartInfo.OriginalListId))
            //{
            //    webpartInfo.DefinitionXml = webpartInfo.DefinitionXml.Replace(webpartInfo.OriginalListId.ToString().ToUpper(), this.Cache.ListIdMapping[webpartInfo.OriginalListId].ToString());
            //}
            //definationXmlDoc.LoadXml(webpartInfo.DefinitionXml);

            //UpdateLink();

            //return UpdateRelativeUrl(definationXmlDoc) || UpdateLibraryGuid(webpartInfo, definationXmlDoc);
            bool needPostRestore = false;
            if (UpdateListId(webpartInfo, definationXmlDoc))
            {
                needPostRestore = true;
            }
            UpdateLink();
            return needPostRestore || UpdateWebUrl(webpartInfo, definationXmlDoc) || UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }

        private bool UpdateListId(AveWebPartBaseInfo webPartInfo, XmlDocument definationXmlDoc)
        {
            string xpath = string.Format(".//*[@name = 'ListId']");
            Guid destinationListId = Guid.Empty;

            XmlNode listNode = definationXmlDoc.SelectSingleNode(xpath);
            if (listNode != null && IsGuid(listNode.InnerText)
                && !listNode.InnerText.Equals(Guid.Empty.ToString()))
            {
                Guid listId = new Guid(listNode.InnerText);
                var list = GetListByTitle(listId, webPartInfo.ListTitle);
                if (list != null)
                {
                    destinationListId = list.ID;
                    webPartInfo.DefinitionXml = webPartInfo.DefinitionXml.Replace(listId.ToString().ToUpper(System.Globalization.CultureInfo.CurrentCulture), destinationListId.ToString());
                    webPartInfo.DefinitionXml = webPartInfo.DefinitionXml.Replace(listId.ToString(), destinationListId.ToString());
                    definationXmlDoc.LoadXml(webPartInfo.DefinitionXml);
                    return false;
                }
            }
            else
            {
                //07DataFormWebpart的listid用上面xpath取不出则用下面形式再取一次
                string newxpath = string.Format(".//*[@name = 'ParameterBindings']");
                foreach (XmlNode parameterBindingsNode in definationXmlDoc.SelectNodes(newxpath))
                {
                    XmlDocument docBinding = new XmlDocument();
                    docBinding.LoadXml(string.Format("<root>{0}</root>", parameterBindingsNode.InnerText));
                    foreach (XmlElement node in docBinding.GetElementsByTagName("ParameterBinding"))
                    {
                        string strName = node.GetAttribute("Name");
                        string value = node.GetAttribute("DefaultValue");
                        if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(value) && strName.Equals("ListID", StringComparison.OrdinalIgnoreCase))
                        {
                            Guid listId = new Guid(value);
                            if (webPartInfo.ListId == Guid.Empty)
                            {//对于07DataFormWebpart的listid为Guid.Empty
                                webPartInfo.ListId = listId;
                            }
                            if (this.Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out destinationListId))
                            {
                                webPartInfo.DefinitionXml = webPartInfo.DefinitionXml.Replace(listId.ToString().ToUpper(System.Globalization.CultureInfo.CurrentCulture), destinationListId.ToString());
                                webPartInfo.DefinitionXml = webPartInfo.DefinitionXml.Replace(listId.ToString(), destinationListId.ToString());
                                definationXmlDoc.LoadXml(webPartInfo.DefinitionXml);
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        private IAveList GetListByTitle(Guid listId, string listTitle)
        {
            IAveList list = null;
            try
            {
                Guid destinationListId;
                if (Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out destinationListId))
                {
                    list = mWeb.Lists.GetById(destinationListId);
                }
                else if(!string.IsNullOrEmpty(listTitle))
                {
                    list = mWeb.Lists.GetByTitle(listTitle);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get list:{0} failed.Error Message:{1}", listTitle, ex.ToString());
                list = null;
            }
            return list;
        }
    }
    public class DefaultWebPartUrlUpdater : AveWebPartPropertyUpdater
    {
        public DefaultWebPartUrlUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webPartCache, webPartLinkUpdater)
        {
        }

        private bool UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            return false;
        }

        private void RefactorWebPartAssemblyInfo(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            if (webpartInfo.SolutionId == null || webpartInfo.SolutionId == Guid.Empty)
            {
                return;
            }
            XmlNode metaDataNode = definationXmlDoc.SelectSingleNode(".//*[name() = 'metaData']");
            if (metaDataNode == null)
            {
                return;
            }
            string typeString = webpartInfo.Class + ", " + webpartInfo.Assembly;
            XmlNode typeNode = metaDataNode.SelectSingleNode(".//*[name() = 'type']");
            if (typeNode != null && (typeNode as XmlElement).HasAttribute("name"))
            {
                typeNode.Attributes["name"].Value = typeString;
            }
            XmlNode solutionIdNode = definationXmlDoc.CreateNode(XmlNodeType.Element, "Solution", "http://schemas.microsoft.com/sharepoint/");
            (solutionIdNode as XmlElement).SetAttribute("SolutionId", webpartInfo.SolutionId.ToString());
            metaDataNode.AppendChild(solutionIdNode);
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            RefactorWebPartAssemblyInfo(webpartInfo, definationXmlDoc);
            return UpdateRelativeUrl(definationXmlDoc) || UpdateLibraryGuid(webpartInfo, definationXmlDoc) || (webpartInfo.SolutionId != null && webpartInfo.SolutionId != Guid.Empty);
        }
    }
    public abstract class AveBaseHtmlWebPartUpdater : AveWebPartPropertyUpdater
    {
        public AveBaseHtmlWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater) :
            base(webPartCache, webPartLinkUpdater) { }

        protected virtual string ReplaceHtmlInWebPartXml(string html)
        {
            StringBuilder sb = new StringBuilder(html);
            Dictionary<int, string> UrlList = null;
            //替换img标签内的src
            UrlList = GetTagList(sb.ToString(), "img", "src");
            foreach (KeyValuePair<int, string> kvp in UrlList)
            {
                sb.Replace(kvp.Value, AveReplaceProcessor.UrlReplace(kvp.Value, Cache.SiteManagedMappings, new ReplaceOption(true, true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl), kvp.Key, kvp.Value.Length);
            }
            //替换image标签内的src
            UrlList = GetTagList(sb.ToString(), "image", "src");
            foreach (KeyValuePair<int, string> kvp in UrlList)
            {
                sb.Replace(kvp.Value, AveReplaceProcessor.UrlReplace(kvp.Value, Cache.SiteManagedMappings, new ReplaceOption(true, true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl), kvp.Key, kvp.Value.Length);
            }
            //替换a标签内的href
            UrlList = GetTagList(sb.ToString(), "a", "href");
            foreach (KeyValuePair<int, string> kvp in UrlList)
            {
                sb.Replace(kvp.Value, AveReplaceProcessor.UrlReplace(kvp.Value, Cache.SiteManagedMappings, new ReplaceOption(true, true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl), kvp.Key, kvp.Value.Length);
            }
            UrlList = GetTagList(sb.ToString(), "a", "rel");
            foreach (KeyValuePair<int, string> kvp in UrlList)
            {
                sb.Replace(kvp.Value, AveReplaceProcessor.UrlReplace(kvp.Value, Cache.SiteManagedMappings, new ReplaceOption(true, true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl), kvp.Key, kvp.Value.Length);
            }
            //替换embed标签内的src
            UrlList = GetTagList(sb.ToString(), "embed", "src");
            foreach (KeyValuePair<int, string> kvp in UrlList)
            {
                sb.Replace(kvp.Value, AveReplaceProcessor.UrlReplace(kvp.Value, Cache.SiteManagedMappings, new ReplaceOption(true, true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl), kvp.Key, kvp.Value.Length);
            }
            //替换bgsound标签内的src
            UrlList = GetTagList(sb.ToString(), "bgsound", "src");
            foreach (KeyValuePair<int, string> kvp in UrlList)
            {
                sb.Replace(kvp.Value, AveReplaceProcessor.UrlReplace(kvp.Value, Cache.SiteManagedMappings, new ReplaceOption(true, true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl), kvp.Key, kvp.Value.Length);
            }
            return sb.ToString();
        }

        protected virtual Dictionary<int, string> GetTagList(string sHtmlText, string tag, string attr)
        {
            Regex regImg = new Regex(@"<" + tag + @"\b[^<>]*?\b" + attr.Trim() + @"[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);

            MatchCollection matches = regImg.Matches(sHtmlText);

            SortedDictionary<int, string> TagList = new SortedDictionary<int, string>();

            foreach (Match match in matches)
            {
                TagList.Add(match.Groups["imgUrl"].Index, match.Groups["imgUrl"].Value.TrimEnd(new char[] { '/' }));
            }
            Dictionary<int, string> tempTagList = new Dictionary<int, string>();
            List<int> keyList = new List<int>();
            foreach (KeyValuePair<int, string> kvp in TagList)
            {
                keyList.Add(kvp.Key);
            }
            for (int i = keyList.Count - 1; i >= 0; i--)
            {
                tempTagList.Add(keyList[i], TagList[keyList[i]]);
            }
            return tempTagList;
        }
    }
    public class ScriptEditorUpdater : AveBaseHtmlWebPartUpdater
    {
        public ScriptEditorUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater) :
            base(webPartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            ReplaceXml(definationXmlDoc);
            return false;
        }
        public void ReplaceXml(XmlDocument definationXmlDoc)
        {
            string xpath = ".//*[@name='Content']";
            XmlNode node = definationXmlDoc.SelectSingleNode(xpath);
            if (node != null && node.InnerText != null)
            {
                node.InnerText = ReplaceHtmlInWebPartXml(node.InnerText);
            }
        }
    }
    public class XMLWebPartUpdater : AveBaseHtmlWebPartUpdater
    {
        public XMLWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater) :
            base(webPartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            ReplaceXml(definationXmlDoc);
            return false;
        }
        public void ReplaceXml(XmlDocument definationXmlDoc)
        {
            string xpath = ".//*[name()='XML']";
            XmlNode node = definationXmlDoc.SelectSingleNode(xpath);
            if (node != null && node.InnerXml != null)
            {
                node.InnerXml = ReplaceHtmlInWebPartXml(node.InnerXml);
            }
        }
    }

    public class SummaryLinkWebpartUpdater : AveWebPartPropertyUpdater
    {
        public SummaryLinkWebpartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater) :
            base(webPartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            return replaceLinkStore(definationXmlDoc) || ReplaceListIdProperties(definationXmlDoc);
        }
        public bool replaceLinkStore(XmlDocument definationXmlDoc)
        {
            string xPath = ".//*[@name = 'SummaryLinkStore']";
            XmlNode node = definationXmlDoc.SelectSingleNode(xPath);
            if (node != null && !string.IsNullOrEmpty(node.InnerText))
            {
                XmlDocument linkXml = new XmlDocument();
                linkXml.LoadXml(node.InnerText);
                XmlNodeList urlNodes = linkXml.SelectNodes(".//*[name() = 'a']");
                if (urlNodes == null)
                {
                    return false;
                }
                foreach (XmlNode urlNode in urlNodes)
                {
                    if (urlNode.InnerText != null)
                    {
                        urlNode.InnerText = AveReplaceProcessor.UrlReplace(urlNode.InnerText.ToString(), this.Cache.SiteManagedMappings, new ReplaceOption(true, true), this.Cache.SourceSiteInfo, this.Cache.DestSiteInfo.ServerRelativeUrl);
                    }
                    if (urlNode.Attributes["href"] != null)
                    {
                        urlNode.Attributes["href"].Value = urlNode.InnerText;
                    }
                }
                node.InnerText = linkXml.OuterXml;
            }
            return false;
        }
        public bool ReplaceListIdProperties(XmlDocument definationXmlDoc)
        {
            string[] nodeNames = new string[] { "ListName", "ListId" };
            bool needPost = false;
            foreach (string nodeName in nodeNames)
            {
                XmlNode listNode = definationXmlDoc.SelectSingleNode(".//*[@name = '" + nodeName + "']");
                if (listNode != null && IsGuid(listNode.InnerText))
                {
                    Guid listId = new Guid(listNode.InnerText);
                    Guid destinationListId;
                    if (listId.Equals(Guid.Empty))
                    {
                        continue;
                    }
                    else if (this.Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out destinationListId))
                    {
                        listNode.InnerText = destinationListId.ToString("B");
                    }
                    else
                    {
                        needPost = true;
                    }
                }
            }
            return needPost;
        }
    }
    public class ContentByQueryWebPartUpdater : AveWebPartPropertyUpdater
    {
        private const string mXPath = ".//*[@name = 'ListGuid']";

        public ContentByQueryWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webPartCache, webPartLinkUpdater)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            ReplaceContentTypeId(definationXmlDoc);
            return UpdateRelativeUrl(definationXmlDoc, webpartInfo) || UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }

        private bool UpdateRelativeUrl(XmlDocument definationXmlDoc, AveWebPartBaseInfo webPartInfo)
        {
            bool needPostRestore = false;
            XmlNode libNode = definationXmlDoc.SelectSingleNode(mXPath);
            if (IsGuid(libNode.InnerText))
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                if (oldLibId.Equals(Guid.Empty))
                {
                    return needPostRestore;
                }

                var destinationListId = Guid.Empty;
                if (!Cache.SiteMappingManager.GetValueFromListIdMapping(oldLibId, out destinationListId))
                {
                    webPartInfo.ListId = webPartInfo.ListId.Equals(Guid.Empty) ? oldLibId : webPartInfo.ListId;
                    needPostRestore = true;
                    return needPostRestore;
                }
                libNode.InnerText = destinationListId.ToString();
            }
            return needPostRestore;
        }

        private void ReplaceContentTypeId(XmlDocument definationXmlDoc)
        {
            XmlNode contentTypeNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'ContentTypeBeginsWithId']");
            if (contentTypeNode == null)
            {
                return;
            }
            string ctID = contentTypeNode.InnerText;
            try
            {
                if (!string.IsNullOrEmpty(ctID))
                {
                    if (Cache.ListLevelCTIdMapping != null)
                    {
                        KeyValuePair<string, IAveContentTypeId> temp = Cache.ListLevelCTIdMapping.First(r => r.Key.Contains(ctID));
                        contentTypeNode.InnerText = temp.Value.Parent.ToString();
                    }
                    else
                    {
                        mLogger.Log(AveLogLevel.WARN, "Not replace content type in ContentByQueryWebPart. Content Type ID:{0}", ctID);
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.WARN, "Not replace content type in ContentByQueryWebPart. Content Type ID:{0}. Error:{1}", ctID, ex.ToString());
            }
        }

        protected override bool IsDependentObjectRestored(XmlDocument webpartDoc)
        {
            XmlNode libNode = webpartDoc.SelectSingleNode(mXPath);
            if (IsGuid(libNode.InnerText))
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                return Cache.SiteMappingManager.ListIdMappingContainsKey(oldLibId);
            }
            return true;
        }
    }

    public abstract class AveBaseViewWebPartUpdater : AveWebPartPropertyUpdater
    {
        public IAveList List;

        public AveBaseViewWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        protected bool UpdateWebId(XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode webNode = definationXmlDoc.SelectSingleNode("//*[@name = 'WebId']");
            if (webNode == null)// ADO-207348 特殊数据
            {
                webNode = definationXmlDoc.SelectSingleNode("//*[name() = 'WebId']");
            }
            if (webNode != null)
            {
                Guid webId = new Guid(webNode.InnerText);
                if (webId.Equals(Guid.Empty))
                {
                    return needPostRestore;
                }
                Guid mappingWebId;
                if (!this.Cache.SiteMappingManager.WebIDMapping.TryGetValue(webId, out mappingWebId))
                {
                    needPostRestore = true;
                    return needPostRestore;
                }
                webNode.InnerText = mappingWebId.ToString();
            }
            return needPostRestore;
        }

        protected virtual IAveList GetListByIdOrTitle(string title, Guid listId)
        {
            try
            {
                if (!listId.Equals(Guid.Empty))
                {
                    return mWeb.Lists[listId];
                }
                else if (!string.IsNullOrEmpty(title))
                {
                    return mWeb.Lists[title];
                }
                return null;
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get list:{0} failed.Error Message:{1}", title, ex.ToString());
                return null;
            }
        }

        protected virtual bool UpdateListName(AveWebPartBaseInfo webPartInfo, XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;

            string[] nodeNames = new string[] { "ListId", "ListName" };
            foreach (string nodeName in nodeNames)
            {
                string xPath = string.Format("//*[name() = '{0}']", nodeName);
                XmlNode node = definationXmlDoc.SelectSingleNode(xPath);
                if (node != null && IsGuid(node.InnerText))
                {
                    Guid listId = new Guid(node.InnerText);
                    Guid destinationListId;
                    if (Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out destinationListId))
                    {
                        node.InnerText = destinationListId.ToString();
                        this.List = GetListByIdOrTitle(null, destinationListId);
                    }
                    else //mapping do not contain the listId in xml.
                    {
                        if (this.List == null)
                        {
                            string listTitle = webPartInfo.ListTitle;
                            if (string.IsNullOrEmpty(listTitle))
                            {
                                XmlNode titleNode = definationXmlDoc.SelectSingleNode(".//*[name() = 'Title']");
                                listTitle = (titleNode == null || string.IsNullOrEmpty(titleNode.InnerText)) ? webPartInfo.ListTitle : titleNode.InnerText;
                            }
                            if ((this.List = GetListByIdOrTitle(listTitle, Guid.Empty)) == null)
                            {
                                needPostRestore = true;
                                break;
                            }
                        }
                        node.InnerText = List.ID.ToString();
                    }
                }
            }
            this.ReplaceListViewXml(webPartInfo, definationXmlDoc);
            return needPostRestore;
        }

        protected void ReplaceListViewXml(AveWebPartBaseInfo webPartInfo, XmlDocument definationXmlDoc)
        {
            if (this.List == null)
            {
                return;
            }
            XmlNode defNode = definationXmlDoc.SelectSingleNode("//*[name() = 'ListViewXml']");
            if (defNode != null)
            {
                XmlDocument viewNode = new XmlDocument();
                viewNode.LoadXml(defNode.InnerText);
                string viewId = viewNode.DocumentElement.GetAttribute("Name");
                Guid viewGuid = new Guid(viewId);
                Guid viewGuidMappingValue;
                if (this.Cache.SiteMappingManager.GetViewGuidMappingValue(viewGuid, out viewGuidMappingValue))
                {
                    viewNode.DocumentElement.SetAttribute("Name", "{" + viewGuidMappingValue.ToString() + "}");
                    if (this.Cache.ViewInfo != null && this.Cache.ViewInfo.Views.ContainsKey(viewGuid))
                    {
                        webPartInfo.IsViewBuildInWebPart = true;
                    }
                }
                EnsureViewFields(viewNode);
                defNode.InnerText = viewNode.OuterXml;
            }
        }

        /// <summary>
        /// 更新view webpart xml信息中的ViewTitle信息
        /// </summary>
        /// <param name="definationXmlDoc"></param>
        protected virtual void UpdateViewTitleInXml(XmlDocument definationXmlDoc)
        {
            if (definationXmlDoc == null || definationXmlDoc.DocumentElement == null)
            {
                return;
            }
            try
            {
                string viewId = definationXmlDoc.DocumentElement.Attributes["ID"].Value;
                if (Validator.IsGuid(viewId))
                {
                    Guid sourceViewId = new Guid(viewId);
                    if (sourceViewId != Guid.Empty && Cache.ViewInfo != null)
                    {
                        var viewInfo = Cache.ViewInfo.Vinfos.Find(view => view.Id == sourceViewId);
                        //部分hidden view也需要处理，example(Merge Documents(Form library),Relink Documents(Form library))
                        if (viewInfo != null)
                        {
                            XmlNode viewTitleNode = definationXmlDoc.SelectSingleNode("//*[@name = 'DisplayName']");
                            if (viewTitleNode == null)
                            {
                                viewTitleNode = definationXmlDoc.SelectSingleNode("//*[name() = 'Title']");
                            }
                            if (viewTitleNode != null)
                            {
                                viewTitleNode.InnerText = viewInfo.Title;
                            }

                            XmlNode definitionNode = definationXmlDoc.SelectSingleNode("//*[@name = 'XmlDefinition']");
                            if (definitionNode == null)
                            {
                                //Calendar(Calendar List),Graphical Summary(Survey List),alendar.aspx(task list),gantt.aspx(task list) title存储在ListViewXml节点中，需要特殊处理
                                definitionNode = definationXmlDoc.SelectSingleNode("//*[name() = 'ListViewXml']");
                            }
                            if (definitionNode != null)
                            {
                                XmlDocument viewNode = new XmlDocument();
                                viewNode.LoadXml(definitionNode.InnerText);
                                if (viewNode.DocumentElement != null)
                                {
                                    viewNode.DocumentElement.Attributes["DisplayName"].Value = viewInfo.Title;
                                }
                                definitionNode.InnerText = viewNode.OuterXml;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred while update view title information in webpart definition xml. DefinitionXml: {0}. Error: {1} ", definationXmlDoc.OuterXml, e);
            }
        }

        protected IDictionary<string, string> GetSingleViewFieldInfo(XmlNode viewFieldNode)
        {
            var infoDictionary = new Dictionary<string, string>();
            if (viewFieldNode.Attributes != null && viewFieldNode.Attributes.Count > 0)
            {
                foreach (XmlAttribute attribute in viewFieldNode.Attributes)
                {
                    if (attribute != null && attribute.Value != null)
                    {
                        infoDictionary.Add(attribute.Name, attribute.Value);
                    }
                }
            }
            return infoDictionary;
        }

        protected void EnsureViewFields(XmlDocument viewNode)
        {
            if (List != null)
            {
                XmlNode fieldNode = viewNode.SelectSingleNode("//ViewFields");
                if (fieldNode != null)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlElement rootNode = xmlDoc.CreateElement("ViewFields");
                    xmlDoc.AppendChild(rootNode);
                    foreach (XmlNode childNode in fieldNode.ChildNodes)
                    {
                        if (childNode.Attributes != null && childNode.Attributes.Count > 0)
                        {
                            try
                            {
                                var viewFieldInfo = GetSingleViewFieldInfo(childNode);
                                if (viewFieldInfo.ContainsKey("Name"))
                                {
                                    string fieldInternalName = viewFieldInfo["Name"];
                                    if (!string.IsNullOrEmpty(fieldInternalName))
                                    {
                                        string destFieldInternalName = GetDestInternalName(fieldInternalName);
                                        if (!string.IsNullOrEmpty(destFieldInternalName))
                                        {
                                            XmlElement viewFieldNode = xmlDoc.CreateElement("FieldRef");
                                            UpdateViewFieldNodeAttributes(viewFieldInfo, viewFieldNode, destFieldInternalName);
                                            rootNode.AppendChild(viewFieldNode);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Ensure View field failed,field schema:{0}.Error Message:{1}", childNode.OuterXml, ex);
                            }
                        }
                    }
                    fieldNode.InnerXml = rootNode.InnerXml;
                }

            }
        }

        private void UpdateViewFieldNodeAttributes(IDictionary<string, string> viewFieldInfo, XmlElement viewFieldNode, string destFieldInternalName)
        {
            Guid fieldId = Guid.Empty;
            if (viewFieldInfo.ContainsKey("ID"))
            {
                fieldId = GetFieldIdFromMapping(new Guid(viewFieldInfo["ID"]));
            }
            foreach (var viewFieldProperty in viewFieldInfo.Keys)
            {
                if (string.Equals(viewFieldProperty, "ID", StringComparison.OrdinalIgnoreCase))
                {
                    if (fieldId != Guid.Empty)
                    {
                        viewFieldNode.SetAttribute(viewFieldProperty, fieldId.ToString("B"));
                    }
                }
                else if (string.Equals(viewFieldProperty, "Name", StringComparison.OrdinalIgnoreCase))
                {
                    viewFieldNode.SetAttribute(viewFieldProperty, destFieldInternalName);
                }
                else
                {
                    viewFieldNode.SetAttribute(viewFieldProperty, viewFieldInfo[viewFieldProperty]);
                }
            }
        }

        private string GetDestInternalName(string fieldName)
        {
            return NeedSkipField(fieldName) ? null : GetFieldFromMapping(fieldName) ?? GetFieldFromDestList(fieldName);
        }

        private bool NeedSkipField(string fieldName)
        {
            IAveFieldMapping fieldMapping;
            if (Cache.SiteMappingManager.TryGetValueFromListFieldsMapping(List.ID, out fieldMapping))
            {
                foreach (var field in fieldMapping.EnumSkippedFields())
                {
                    if (string.Equals(field, fieldName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string GetFieldFromDestList(string fieldName)
        {
            try
            {
                return List.Fields.GetFieldByInternalName(fieldName).InternalName;
            }
            catch (Exception e)
            {
                var msg = e.ToString();
                //找不到就返回null，不需要打log
                return null;
            }
        }

        private string GetFieldFromMapping(string fieldName)
        {
            IAveFieldMapping fieldMapping;
            if (Cache.SiteMappingManager.TryGetValueFromListFieldsMapping(List.ID, out fieldMapping))
            {
                return fieldMapping.GetMappingRestoredFieldInternalName(fieldName);
            }
            return null;
        }

        private Guid GetFieldIdFromMapping(Guid fieldId)
        {
            IAveFieldMapping fieldMapping;
            if (Cache.SiteMappingManager.TryGetValueFromListFieldsMapping(List.ID, out fieldMapping))
            {
                return fieldMapping.GetMappingRestoredFieldId(fieldId);
            }
            return Guid.Empty;
        }
        protected string GetDestinationFieldDisplayName(String sourceFieldName)
        {
            String destinationFieldName = String.Empty;
            try
            {
                if (List.Fields.GetField(sourceFieldName) != null)
                {
                    destinationFieldName = sourceFieldName;
                }
            }
            catch (Exception e)
            {
                IAveFieldMapping fieldMapping;
                Cache.SiteMappingManager.TryGetValueFromListFieldsMapping(List.ID, out fieldMapping);
                if (!string.IsNullOrEmpty(fieldMapping.GetMappingRestoredFieldDisplayName(sourceFieldName)))
                {
                    destinationFieldName = fieldMapping.GetMappingRestoredFieldDisplayName(sourceFieldName);
                }
                else if (!string.IsNullOrEmpty(fieldMapping.GetMappingRestoredFieldInternalName(sourceFieldName)))
                {
                    destinationFieldName = fieldMapping.GetMappingRestoredFieldInternalName(sourceFieldName);
                }
                else
                {
                    throw new Exception(e.ToString());
                }
            }
            return destinationFieldName;
        }
    }

    public class XsltListViewWebPartUpdater : AveBaseViewWebPartUpdater
    {
        private const string mXPath = ".//*[@name = 'ListName']";

        public XsltListViewWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            UpdateViewTitleInXml(definationXmlDoc);
            return UpdateWebId(definationXmlDoc) || UpdateListName(webpartInfo, definationXmlDoc) || UpdateRelativeUrl(definationXmlDoc);
        }

        private bool UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode libNode = definationXmlDoc.SelectSingleNode(mXPath);
            if (IsGuid(libNode.InnerText))
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                if (oldLibId.Equals(Guid.Empty))
                {
                    return needPostRestore;
                }
                Guid destinationListId;
                if (Cache.SiteMappingManager.GetValueFromListIdMapping(oldLibId, out destinationListId))
                {
                    libNode.InnerText = destinationListId.ToString();
                }
                else if (List != null)//当listIdMapping中找不到Mapping关系，但是通过listname能找到对应List的情况也需要替换；
                {
                    libNode.InnerText = List.ID.ToString();
                }
                else
                {
                    needPostRestore = true;
                }
            }
            return needPostRestore;
        }

        protected override bool UpdateListName(AveWebPartBaseInfo webPartInfo, XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode listNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'ListName']");
            if (listNode != null && IsGuid(listNode.InnerText))
            {
                Guid listId = new Guid(listNode.InnerText);
                string listTitle = string.Empty;
                XmlNode listTitleNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'Title']");
                if (listTitleNode != null)
                {
                    if (!string.IsNullOrEmpty(webPartInfo.ListTitle))
                    {
                        List = GetListByTitle(listId, webPartInfo.ListTitle);//源端back时已经将list title backup
                        if(List != null)
                        {
                            listTitle = webPartInfo.ListTitle;
                        }
                    }
                    if (List == null && !string.IsNullOrEmpty(listTitleNode.InnerText))
                    {
                        List = GetListByTitle(listId, listTitleNode.InnerText);
                    }
                }
                bool listIdInCache = false;
                Guid destListId = Guid.Empty;
                if (this.Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out destListId))
                {
                    listIdInCache = true;
                }
                if (listIdInCache || !string.IsNullOrEmpty(listTitle))
                {
                    XmlNode listIdNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'ListId']");
                    XmlNode defNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'XmlDefinition']");
                    if (defNode != null)
                    {
                        XmlDocument viewNode = new XmlDocument();
                        viewNode.LoadXml(defNode.InnerText);
                        string viewId = viewNode.DocumentElement.GetAttribute("Name");
                        Guid viewGuid = new Guid(viewId);
                        Guid viewGuidMappingValue;
                        if (this.Cache.SiteMappingManager.GetViewGuidMappingValue(viewGuid, out viewGuidMappingValue))
                        {
                            viewNode.DocumentElement.SetAttribute("Name", "{" + viewGuidMappingValue.ToString() + "}");
                            if (this.Cache.ViewInfo != null && this.Cache.ViewInfo.Views.ContainsKey(viewGuid))
                            {
                                webPartInfo.IsViewBuildInWebPart = true;
                            }
                        }
                        EnsureViewFields(viewNode);
                        defNode.InnerText = viewNode.OuterXml;
                    }
                    if (listIdInCache)
                    {
                        listNode.InnerText = destListId.ToString("B");
                        if (listIdNode != null)
                        {
                            listIdNode.InnerText = destListId.ToString();
                        }
                    }
                    else
                    {
                        if (List != null)
                        {
                            listId = List.ID;
                            listNode.InnerText = listId.ToString("B");
                            if (listIdNode != null)
                            {
                                listIdNode.InnerText = listId.ToString();
                            }
                        }
                        else
                        {
                            needPostRestore = true;
                        }
                    }
                }
                else
                {
                    needPostRestore = true;
                }
            }
            return needPostRestore;
        }

        private IAveList GetListByTitle(Guid listId, string listTitle)
        {
            IAveList list = null;
            try
            {
                Guid destinationListId;
                if (Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out destinationListId))
                {
                    list = mWeb.Lists.GetById(destinationListId);
                }
                else
                {
                    list = mWeb.Lists.GetByTitle(listTitle);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get list:{0} failed.Error Message:{1}", listTitle, ex.ToString());
                list = null;
            }
            return list;
        }
    }

    public class ListFormWebpartUpdater : AveBaseViewWebPartUpdater
    {
        public ListFormWebpartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            UpdateViewTitleInXml(definationXmlDoc);
            return UpdateListName(webpartInfo, definationXmlDoc);
        }
    }
    public class ListViewWebPartUpdater : AveBaseViewWebPartUpdater
    {
        private const string mXPath = ".//*[@name = 'ListGuid']";

        public ListViewWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            //UpdateRelativeUrl(definationXmlDoc);
            //UpdateWebId(definationXmlDoc);
            UpdateViewTitleInXml(definationXmlDoc);
            return UpdateWebId(definationXmlDoc) || UpdateRelativeUrl(definationXmlDoc) || UpdateListName(webpartInfo, definationXmlDoc);
        }

        private bool UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode libNode = definationXmlDoc.SelectSingleNode(mXPath);
            if (libNode != null && IsGuid(libNode.InnerText))
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                if (oldLibId.Equals(Guid.Empty))
                {
                    return needPostRestore;
                }
                Guid destinationListId;
                if (!Cache.SiteMappingManager.GetValueFromListIdMapping(oldLibId, out destinationListId))
                {
                    needPostRestore = true;
                    return needPostRestore;
                }
                libNode.InnerText = destinationListId.ToString();
            }
            return needPostRestore;
        }
    }

    public class MediaWebPartUpdater : AveWebPartPropertyUpdater
    {
        public MediaWebPartUpdater(AveWebPartCache webpartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webpartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            ReplaceMediaWebPartProperties(webpartInfo, definationXmlDoc);
            return false;
        }
        private void ReplaceMediaWebPartProperties(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            string[] needReplaceNodesNames = new string[] { "MediaSource", "PreviewImageSource" };
            foreach (string nodeName in needReplaceNodesNames)
            {
                XmlNode node = definationXmlDoc.SelectSingleNode(".//*[@name = '" + nodeName + "']");
                if (node != null && !string.IsNullOrEmpty(node.InnerText))
                {
                    node.InnerText = AveReplaceProcessor.UrlReplace(node.InnerText.ToString(), Cache.SiteManagedMappings, new ReplaceOption(true, true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl);
                }
            }
        }
    }

    public class BusinessDataWebpartUpdater : AveWebPartPropertyUpdater
    {
        public BusinessDataWebpartUpdater(AveWebPartCache webpartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webpartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            return UpdateListIdNodes(webpartInfo, definationXmlDoc, new string[] { "ListName", "ListId" });
        }
    }

    public class BlogLinksWebPartUpdater : AveWebPartPropertyUpdater
    {
        public BlogLinksWebPartUpdater(AveWebPartCache webpartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webpartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            return UpdateListIdNodes(webpartInfo, definationXmlDoc, new string[] { "ListName", "ListId" });
        }
    }

    public class RSSAggregatorWebPartUpdater : AveWebPartPropertyUpdater
    {
        public RSSAggregatorWebPartUpdater(AveWebPartCache webpartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webpartCache, webPartLinkUpdater) { }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            return UpdateListIdNodes(webpartInfo, definationXmlDoc, new string[] { "ListName", "ListId" });
        }
    }
    public class ThisWeekInPicturesWebPart : AveWebPartPropertyUpdater
    {
        public ThisWeekInPicturesWebPart(AveWebPartCache webpartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webpartCache, webPartLinkUpdater, web) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            AddDefaultNode(definationXmlDoc);
            return false;
        }
        private void AddDefaultNode(XmlDocument definationXmlDoc)
        {
            if (definationXmlDoc.SelectSingleNode("//*[name() = 'ImageLibrary']") != null)
            {
                return;
            }
            XmlElement newElement = definationXmlDoc.CreateElement("ImageLibrary", "urn:schemas-microsoft-com:sharepoint:ThisWeekInPicturesWebPart");
            newElement.InnerText = "This Week in Pictures Library";
            definationXmlDoc.FirstChild.AppendChild((XmlNode)newElement);
        }
    }
    public class CategoryResultsWebPart : AveWebPartPropertyUpdater
    {
        public CategoryResultsWebPart(AveWebPartCache webpartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webpartCache, webPartLinkUpdater, web) { }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            HandleSPMWebPart(webpartInfo, definationXmlDoc);
            UpdateLink();
            return UpdateListIdNodes(webpartInfo, definationXmlDoc, new string[] { "ListName", "ListId" });
        }

        protected override void HandleSPMWebPart(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            if (mWeb.Site.CompatibilityLevel != 15)
            {
                return;
            }
            XmlNode webUrlNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'WebUrl']");
            if (webUrlNode != null && string.IsNullOrEmpty(webUrlNode.InnerText))
            {
                webUrlNode.InnerText = this.mWeb.ServerRelativeUrl;
            }
            XmlNode node = definationXmlDoc.SelectSingleNode(string.Format(".//*[@name = 'Level1Style']"));
            if (node != null && !string.IsNullOrEmpty(node.InnerText) && node.InnerText == "VerticalBold")
            {
                node.InnerText = "Vertical";
            }
        }
    }

    public class CategoryWebPartUpdater : AveWebPartPropertyUpdater
    {
        public CategoryWebPartUpdater(AveWebPartCache webpartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webpartCache, webPartLinkUpdater) { }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            return UpdateListIdNodes(webpartInfo, definationXmlDoc, new string[] { "ListName", "ListId" });
        }
    }
    public class ClientWebPartUpdater : AveWebPartPropertyUpdater
    {
        public ClientWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webPartCache, webPartLinkUpdater)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateClientWebPartProperty(definationXmlDoc);
            RemoveSingleNodeByPropertyName(definationXmlDoc);
            return false;
        }

        private void UpdateClientWebPartProperty(XmlDocument definationXmlDoc)
        {
            var xNodeProductWebId = definationXmlDoc.SelectSingleNode(".//*[@name = 'ProductWebId']");
            if (xNodeProductWebId != null && !string.IsNullOrEmpty(xNodeProductWebId.InnerText))
            {
                var originalWebId = new Guid(xNodeProductWebId.InnerText);
                var destWebId = Guid.Empty;
                if (Cache.SiteMappingManager.WebIDMapping.TryGetValue(originalWebId, out destWebId))
                {
                    xNodeProductWebId.InnerText = destWebId.ToString();
                }
            }
        }

        private void RemoveSingleNodeByPropertyName(XmlDocument definationXmlDoc)
        {
            var parentNode = definationXmlDoc.SelectSingleNode(".//*[name()='properties']");
            if (parentNode == null)
            {
                return;
            }
            //这些属性是我们构建出来的，在export出来的xml上没有，移除这些属性，否者import时会抛属性不存在异常
            var needEnsureProperties = new List<string> { "ID", "IsClosed", "IsIncluded", "ZoneID", "PartOrder", "WebPartIdProperty" };
            foreach (var propertyName in needEnsureProperties)
            {
                var propertyNode = parentNode.SelectSingleNode(string.Format(".//*[@name = '{0}']", propertyName));
                if (propertyNode != null)
                {
                    parentNode.RemoveChild(propertyNode);
                }
            }
        }
    }

    public class BroswerFormWebPartUpdater : AveWebPartPropertyUpdater
    {
        public BroswerFormWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            EnsureContentTypeIdAndFormLocation(webpartInfo, definationXmlDoc);
            return UpdateContentTypeIdAndFormLocation(definationXmlDoc);
        }

        /// <summary>
        /// local->365 补全属性,只做补全操作,不做任何替换
        /// </summary>
        /// <param name="webpartInfo"></param>
        /// <param name="definationXmlDoc"></param>
        private void EnsureContentTypeIdAndFormLocation(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            Dictionary<string, object> localProperties = InitPropertiesByLocalData(webpartInfo);
            if (localProperties == null || localProperties.Count == 0)
            {
                return;
            }
            var needEnsureProperties = new List<string> { "FormLocation", "ContentTypeId" };
            var propertiesNode = definationXmlDoc.SelectSingleNode(".//*[name()='properties']");
            needEnsureProperties.ForEach(property => EnsureSingleNodeByPropertyName(definationXmlDoc, propertiesNode, property, "", localProperties));
        }

        private Dictionary<string, object> InitPropertiesByLocalData(AveWebPartBaseInfo webpartInfo)
        {
            if (webpartInfo.AllUsersProperties == null && webpartInfo.PerUserProperties == null)
            {
                return new Dictionary<string, object>();
            }
            int result;
            var localProperties = AveWebPartUtility.GetProperties(webpartInfo.AllUsersProperties, webpartInfo.PerUserProperties, out result);
            return localProperties;
        }

        private void EnsureSingleNodeByPropertyName(XmlDocument definationXmlDoc, XmlNode parentNode, string propertyName, string propertyType, Dictionary<string, object> localProperties)
        {
            if (parentNode == null)
            {
                return;
            }
            var propertyNode = parentNode.SelectSingleNode(string.Format(".//*[@name = '{0}']", propertyName));
            if (propertyNode == null && localProperties.ContainsKey(propertyName))
            {
                XmlElement propertyElement = definationXmlDoc.CreateElement("property", parentNode.NamespaceURI);
                propertyElement.SetAttribute("name", propertyName);
                if (!string.IsNullOrEmpty(propertyType))
                {
                    propertyElement.SetAttribute("type", propertyType);
                }
                propertyElement.InnerText = Convert.ToString(localProperties[propertyName]);
                parentNode.AppendChild(propertyElement);
            }
        }

        private bool UpdateContentTypeIdAndFormLocation(XmlDocument definationXmlDoc)
        {
            bool needPost = false;
            string contentTypeIdXPath = ".//*[@name = 'ContentTypeId']";
            XmlNode contentTypeIdNode = definationXmlDoc.SelectSingleNode(contentTypeIdXPath);
            string formLocationXPath = ".//*[@name = 'FormLocation']";
            XmlNode formLocationNode = definationXmlDoc.SelectSingleNode(formLocationXPath);
            if (contentTypeIdNode != null && formLocationNode != null)
            {
                string destListUrl = AveReplaceProcessor.UrlReplace(formLocationNode.InnerText, Cache.SiteMappingManager.ListUrlMapping, new ReplaceOption(true, true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl);
                IAveList list = mWeb.GetList(destListUrl);
                if (list != null)
                {
                    formLocationNode.InnerText = destListUrl;
                    IAveContentTypeId destContentTypeId;
                    if (Cache.SiteMappingManager.TryGetValueFromListLevelContentTypeIdMapping(list.ID, contentTypeIdNode.InnerText, out destContentTypeId))
                    {
                        contentTypeIdNode.InnerText = destContentTypeId.ToString();
                    }
                    else
                    {
                        needPost = true;
                    }
                }
                else
                {
                    needPost = true;
                }
            }
            return needPost;
        }
    }

    public class MembersWebPartUpdater : AveWebPartPropertyUpdater
    {
        public MembersWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateMembershipGroupId(definationXmlDoc);
            return false;
        }
        private void UpdateMembershipGroupId(XmlDocument definationXmlDoc)
        {
            var elements = definationXmlDoc.DocumentElement.GetElementsByTagName("MembershipGroupId");
            if (elements.Count > 0)
            {
                var element = elements[0];
                int originGroupId;
                int newGroupId = -1;
                if (int.TryParse(element.InnerText, out originGroupId))
                {
                    if (Cache.SiteUserIDMapping.ContainsKey(originGroupId))
                    {
                        object obj = Cache.SiteUserIDMapping[originGroupId];
                        if (obj != null && obj.GetType().Name.Equals("AveSPMemberInfo"))
                        {
                            newGroupId = (int)AveAssemblyUtility.GetFieldValue(obj, "NewId");
                        }
                    }
                    if (newGroupId > -1)
                    {
                        element.InnerText = newGroupId.ToString();
                    }
                }
            }
        }
    }
    public class TermPropertyWebPartUpdater : AveWebPartPropertyUpdater
    {
        const string termStoreXPath = ".//*[@name = 'TermStoreID']";
        const string termSetXPath = ".//*[@name = 'TermSetID']";
        const string termXPath = ".//*[@name = 'TermID']";
        public TermPropertyWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            var termStoreNode = definationXmlDoc.SelectSingleNode(termStoreXPath);
            HandleTaxonomyNodeMapping(termStoreNode, Cache.TermStoreIdMapping);
            var termSetNode = definationXmlDoc.SelectSingleNode(termSetXPath);
            HandleTaxonomyNodeMapping(termSetNode, Cache.TermSetIdMapping);
            var termNode = definationXmlDoc.SelectSingleNode(termXPath);
            HandleTaxonomyNodeMapping(termNode, Cache.TermIdMapping);
            return false;
        }
        private void HandleTaxonomyNodeMapping(XmlNode node, Dictionary<Guid, Guid> mapping)
        {
            if (node != null && IsGuid(node.InnerText))
            {
                var sourceId = new Guid(node.InnerText);
                Guid destId;
                if(mapping.TryGetValue(sourceId,out destId))
                {
                    node.InnerText = destId.ToString();
                }
            }
        }
    }
    #endregion

    public class AveClientWebPartUrlHandlerFactory
    {
        public static AveWebPartPropertyUpdater GenerateWebPartUrlHanlder(Guid webPartId, IAveWeb web, XmlNode webPartNode, AveWebPartCache webPartCache)
        {
            return CreateWebpartUpdateInstance(webPartId, web, webPartCache, GreateLinkUpdateInstance(webPartNode));
        }

        internal static AveWebPartType GetWebPartTypeId(Guid webPartTypeId)
        {
            AveWebPartType webPartType;
            WebPartUpdaterMappings.TryGetValue(webPartTypeId, out webPartType);
            return webPartType;
        }

        private static AveWebPartPropertyUpdater CreateWebpartUpdateInstance(Guid webPartId, IAveWeb web, AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater)
        {
            AveWebPartType webPartType = AveWebPartType.DefaultWebpartType;
            WebPartUpdaterMappings.TryGetValue(webPartId, out webPartType);

            switch (webPartType)
            {
                case AveWebPartType.ContentByQueryWebPart:
                    return new ContentByQueryWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.ContentEditorWebPart:
                    return new ContentEditorWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.ExcelWebRendererWebPart:
                    return new ExcelWebRendererWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.TableOfContentsWebPart:
                    return new TableOfContentsWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.VisioWebAccessWebPart:
                    return new VisioWebAccessWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.ListViewWebPart:
                    return new ListViewWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.XsltListViewWebPart:
                    return new XsltListViewWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.ListFormWebPart:
                    return new ListFormWebpartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.SummaryLinkWebPart:
                    return new SummaryLinkWebpartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.SocialCommentWebPart:
                    return new SocialCommentWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.TagCloudWebPart:
                    return new TagCloudWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.BusinessDataWebPart:
                    return new BusinessDataWebpartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.XMLWebPart:
                    return new XMLWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.CategoryResultsWebPart:
                    return new CategoryResultsWebPart(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.ScriptEditorWebPart:
                    return new ScriptEditorUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.MediaWebPart:
                    return new MediaWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.RSSAggregatorWebPart:
                    return new RSSAggregatorWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.BlogLinksWebPart:
                    return new BlogLinksWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.ThisWeekInPicturesWebPart:
                    return new ThisWeekInPicturesWebPart(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.SiteDocuments:
                    return new SiteDocumentsWebPart(webPartCache, webPartLinkUpdater);
                case AveWebPartType.TimeLineWebPart:
                    return new TimeLineWebPart(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.ContactDetailWebPart:
                    return new ContactDetailWebPart(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.DataFormWebPart:
                case AveWebPartType.BlogViewWebPat:
                    return new DataFormWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.BrowserFormWebPart:
                    return new BroswerFormWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.CategoryWebPart:
                    return new CategoryWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.ClientWebPart:
                    return new ClientWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.MembersWebPart:
                    return new MembersWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.TermPropertyWebPart:
                    return new TermPropertyWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.DefaultWebpartType:
                default:
                    return new DefaultWebPartUrlUpdater(webPartCache, webPartLinkUpdater);
            }
        }
        private static AveWebPartLinkUpdater GreateLinkUpdateInstance(XmlNode webPartNode)
        {
            if (string.IsNullOrEmpty(webPartNode.NamespaceURI))
            {
                webPartNode = webPartNode.FirstChild;
            }
            if (webPartNode.NamespaceURI.Equals("http://schemas.microsoft.com/WebPart/v2", StringComparison.OrdinalIgnoreCase))
            {
                return new AveV2WebPartLinkUpdater(webPartNode);
            }
            return new AveV3WebPartLinkUpdater(webPartNode);
        }
    }

    #region WebPart link updater
    public abstract class AveWebPartLinkUpdater
    {
        protected XmlNode WebPartNode { private set; get; }

        public AveWebPartLinkUpdater(XmlNode webPartNode)
        {
            this.WebPartNode = webPartNode;
        }

        public abstract void UpdateLink(AveWebPartCache cache);
        protected abstract void ReplaceAssemblyVersion(AveWebPartCache cache);
    }

    internal class AveV2WebPartLinkUpdater : AveWebPartLinkUpdater
    {
        public AveV2WebPartLinkUpdater(XmlNode webPartNode)
            : base(webPartNode)
        {
        }

        public override void UpdateLink(AveWebPartCache cache)
        {
            foreach (XmlNode node in base.WebPartNode.ChildNodes)
            {
                if (node.Name.EndsWith("Link", StringComparison.OrdinalIgnoreCase) || node.Name.EndsWith("URL", StringComparison.OrdinalIgnoreCase) || node.Name.Equals("MediaSource"))
                {
                    if (node.InnerText.Contains('/'))
                    {
                        node.InnerText = AveReplaceProcessor.UrlReplace(node.InnerText, cache.SiteManagedMappings, new ReplaceOption(true, true), cache.SourceSiteInfo, cache.DestSiteInfo.ServerRelativeUrl);
                    }
                }
                else if (node.Name.Equals("ID", StringComparison.OrdinalIgnoreCase) && node.InnerText.StartsWith("g_", StringComparison.OrdinalIgnoreCase))
                {
                    node.InnerText = "";
                }
            }
            ReplaceIconImage(cache);
            ReplaceAssemblyVersion(cache);
        }

        protected void ReplaceIconImage(AveWebPartCache cache)
        {
            ReplaceIconImage("//*[name() = 'PartImageSmall']", cache);
            ReplaceIconImage("//*[name() = 'PartImageLarge']", cache);
        }

        private void ReplaceIconImage(string xpath, AveWebPartCache cache)
        {
            XmlNode iconImageNode = WebPartNode.SelectSingleNode(xpath);
            if (iconImageNode != null && !string.IsNullOrEmpty(iconImageNode.InnerText))
            {
                iconImageNode.InnerText = AveReplaceProcessor.UrlReplace(iconImageNode.InnerText, cache.SiteManagedMappings, new ReplaceOption(true, true), cache.SourceSiteInfo, cache.DestSiteInfo.ServerRelativeUrl);
            }
        }

        protected override void ReplaceAssemblyVersion(AveWebPartCache cache)
        {
            if (cache.SourceSiteInfo.SPVersion != null && string.Compare(cache.SourceSiteInfo.SPVersion, cache.DestSiteInfo.SPVersion, StringComparison.Ordinal) <= 0)
            {
                return;
            }
            string sourceVersion = cache.SourceSiteInfo.SPVersion.Substring(0, cache.SourceSiteInfo.SPVersion.IndexOf('.')) + ".0.0.0";
            string destVersion = cache.DestSiteInfo.SPVersion.Substring(0, cache.DestSiteInfo.SPVersion.IndexOf('.')) + ".0.0.0";
            XmlNode assemblyNode = WebPartNode.SelectSingleNode("//*[name() = 'Assembly']");
            if (assemblyNode != null && !string.IsNullOrEmpty(assemblyNode.InnerText))
            {
                assemblyNode.InnerText = assemblyNode.InnerText.Replace(sourceVersion, destVersion);
            }
        }

    }

    internal class AveV3WebPartLinkUpdater : AveWebPartLinkUpdater
    {
        public AveV3WebPartLinkUpdater(XmlNode webPartNode)
            : base(webPartNode)
        {
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property.")]
        public override void UpdateLink(AveWebPartCache cache)
        {
            foreach (XmlNode pNode in base.WebPartNode.ChildNodes)
            {
                if (pNode.Name.Equals("data"))
                {
                    XmlNode tempNode = pNode.FirstChild;
                    if (tempNode == null)
                    {
                        return;
                    }
                    foreach (XmlNode node in tempNode.ChildNodes)
                    {
                        if (node.Attributes.Count > 0)
                        {
                            if (node.Attributes[0].Value.EndsWith("Link", StringComparison.OrdinalIgnoreCase) ||
                            node.Attributes[0].Value.EndsWith("Uri", StringComparison.OrdinalIgnoreCase) ||
                            node.Attributes[0].Value.EndsWith("URL", StringComparison.OrdinalIgnoreCase) ||
                            node.Attributes[0].Value.EndsWith("Address", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("CatalogIconImageUrl", node.Attributes[0].Value, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("TitleIconImageUrl", node.Attributes[0].Value, StringComparison.OrdinalIgnoreCase))
                            {
                                if (node.InnerText.Contains('/'))
                                {
                                    if (node.InnerText.StartsWith("~sitecollection", StringComparison.OrdinalIgnoreCase))
                                    {
                                        string url = node.InnerText.Replace("~sitecollection", cache.SourceSiteInfo.ServerRelativeUrl);
                                        if (url.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                                        {
                                            url = url.Substring(1);
                                        }
                                        url = AveReplaceProcessor.UrlReplace(url, cache.SiteManagedMappings, new ReplaceOption(true, true), cache.SourceSiteInfo, cache.DestSiteInfo.ServerRelativeUrl);
                                        if (cache.DestSiteInfo.ServerRelativeUrl == "/")
                                        {
                                            url = "~sitecollection" + url;
                                        }
                                        else
                                        {
                                            url = url.Replace(cache.DestSiteInfo.ServerRelativeUrl, "~sitecollection");
                                        }
                                        node.InnerText = System.Web.HttpUtility.UrlDecode(url);
                                    }
                                    else
                                    {
                                        node.InnerText = System.Web.HttpUtility.UrlDecode(AveReplaceProcessor.UrlReplace(node.InnerText, cache.SiteManagedMappings, new ReplaceOption(true, true), cache.SourceSiteInfo, cache.DestSiteInfo.ServerRelativeUrl));
                                    }
                                }
                            }
                            if (string.Equals(node.Attributes[0].Value, "DataProviderJSON", StringComparison.OrdinalIgnoreCase))
                            {
                                node.InnerText = AveContentBySearchWebPartUtility.UpdateDataProviderJsonProperty(node.InnerText, cache);
                            }
                        }
                    }
                }
            }
            ReplaceAssemblyVersion(cache);
        }
        protected override void ReplaceAssemblyVersion(AveWebPartCache cache)
        {
            if (cache.SourceSiteInfo.SPVersion != null && string.Compare(cache.SourceSiteInfo.SPVersion, cache.DestSiteInfo.SPVersion, StringComparison.Ordinal) <= 0)
            {
                return;
            }
            string sourceVersion = cache.SourceSiteInfo.SPVersion.Substring(0, cache.SourceSiteInfo.SPVersion.IndexOf('.')) + ".0.0.0";
            string destVersion = cache.DestSiteInfo.SPVersion.Substring(0, cache.DestSiteInfo.SPVersion.IndexOf('.')) + ".0.0.0";
            XmlNode assemblyNode = WebPartNode.SelectSingleNode("//*[name() = 'type']");
            if (assemblyNode != null && assemblyNode.Attributes["name"] != null && !string.IsNullOrEmpty(assemblyNode.Attributes["name"].Value))
            {
                assemblyNode.Attributes["name"].Value = assemblyNode.Attributes["name"].Value.Replace(sourceVersion, destVersion);
            }
        }
    }
    #endregion
}
