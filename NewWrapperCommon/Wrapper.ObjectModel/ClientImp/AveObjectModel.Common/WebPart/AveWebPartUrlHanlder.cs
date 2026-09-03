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
using System.Xml;
using AvePoint.Common;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace AvePoint.ObjectModel.Common.WebPart
{
    public abstract class AveWebPartPropertyUpdater
    {
        public AveWebPartCache Cache = null;
        public AveWebPartLinkUpdater LinkUpdater;
        protected static AveLogger mLogger = AveLogger.GetInstance(typeof(AveWebPartPropertyUpdater));
        protected AveWebPartPropertyUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater)
        {
            Cache = webPartCache;
            LinkUpdater = webPartLinkUpdater;
        }

        protected void UpdateLink()
        {
            LinkUpdater.UpdateLink(Cache);
        }
        public bool UpdateListNameAndId(AveWebPartBaseInfo webPartInfo, XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            string[] paths = new string[] { "ListName", "ListId" };
            for (int i = 0; i < paths.Length; i++)
            {
                string xpath = string.Format(".//*[@name = '{0}']", paths[i]);
                if (needPostRestore = !UpdateXmlProperties(definationXmlDoc, xpath))//更新Xml属性失败,则需要Post Restore.
                {
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
        protected bool UpdateXmlProperties(XmlDocument definationXmlDoc, string xpath)
        {
            XmlNode listNode = definationXmlDoc.SelectSingleNode(xpath);
            if (listNode != null && AveSPCommonUtility.IsGuid(listNode.InnerText)
                && !listNode.InnerText.Equals(Guid.Empty.ToString()))
            {
                Guid listId = new Guid(listNode.InnerText);
                Guid destinationListId;
                if (!this.Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out destinationListId))
                {
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
    public class TagCloudWebPartUpdater : AveWebPartPropertyUpdater
    {
        public TagCloudWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater) :
            base(webPartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            ConvertV3DefinitionXmlToV2(webpartInfo, definationXmlDoc.FirstChild);
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
        public TableOfContentsWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webPartCache, webPartLinkUpdater)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            UpdateRelativeUrl(definationXmlDoc);
            return UpdateLibraryGuid(webpartInfo, definationXmlDoc) || UpdateListNameAndId(webpartInfo, definationXmlDoc);
        }

        private void UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            List<string> replaceProperties = new List<string>();
            replaceProperties.Add("AnchorLocation");
            ReplaceRelativeInfo(definationXmlDoc, replaceProperties);
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

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            return UpdateRelativeUrl(definationXmlDoc) || UpdateLibraryGuid(webpartInfo, definationXmlDoc);
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
                if (listNode != null && AveSPCommonUtility.IsGuid(listNode.InnerText))
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
            return UpdateRelativeUrl(definationXmlDoc) || UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }

        private bool UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode libNode = definationXmlDoc.SelectSingleNode(mXPath);
            if (AveSPCommonUtility.IsGuid(libNode.InnerText))
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

        protected override bool IsDependentObjectRestored(XmlDocument webpartDoc)
        {
            XmlNode libNode = webpartDoc.SelectSingleNode(mXPath);
            if (AveSPCommonUtility.IsGuid(libNode.InnerText))
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                return Cache.SiteMappingManager.ListIdMappingContainsKey(oldLibId);
            }
            return true;
        }
    }

    public abstract class AveBaseViewWebPartUpdater : AveWebPartPropertyUpdater
    {

        public IAveWeb Web;
        public IAveList List;

        public AveBaseViewWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater)
        {
            this.Web = web;
        }

        protected bool UpdateWebId(XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode webNode = definationXmlDoc.SelectSingleNode("//*[@name = 'WebId']");
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
                    return Web.Lists[listId];
                }
                else if (!string.IsNullOrEmpty(title))
                {
                    return Web.Lists[title];
                }
                return null;
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get list:{0} failed.Error Message:{1}.", title, ex.ToString());
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
                if (node != null && AveSPCommonUtility.IsGuid(node.InnerText))
                {
                    Guid listId = new Guid(node.InnerText);
                    Guid destinationListId;
                    if (Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out destinationListId))
                    {
                        node.InnerText = destinationListId.ToString();
                        this.List = GetListByIdOrTitle(null, destinationListId);
                    }
                    else if (this.List == null) //mapping do not contain the listId in xml.
                    {
                        XmlNode titleNode = definationXmlDoc.SelectSingleNode(".//*[name() = 'Title']");
                        string listTitle = (titleNode == null || string.IsNullOrEmpty(titleNode.InnerText)) ? webPartInfo.ListTitle : titleNode.InnerText;
                        if ((this.List = GetListByIdOrTitle(listTitle, Guid.Empty)) == null)
                        {
                            needPostRestore = true;
                            break;
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
                            string fieldName = childNode.Attributes[0].Value;
                            try
                            {
                                fieldName = GetDestInternalName(fieldName);
                                if (fieldName != String.Empty)
                                {
                                    XmlElement newNode = xmlDoc.CreateElement("FieldRef");
                                    newNode.SetAttribute("Name", fieldName);
                                    rootNode.AppendChild(newNode);
                                }
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Ensure View field:{0} failed.Error Message:{1}.", fieldName, ex.ToString());
                            }
                        }
                    }
                    fieldNode.InnerXml = rootNode.InnerXml;
                }

            }
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

        private string GetDestInternalName(string fieldName)
        {
            return GetFieldFromMapping(fieldName) ?? GetFieldFromDestList(fieldName);
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
            if (Cache.SiteMappingManager.TryGetValueFromListFieldsMapping(List.ID,out fieldMapping))
            {
                return fieldMapping.GetMappingRestoredFieldInternalName(fieldName);
            }
            return null;
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
            return UpdateWebId(definationXmlDoc) || UpdateListName(webpartInfo, definationXmlDoc) || UpdateRelativeUrl(definationXmlDoc);
        }

        private bool UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode libNode = definationXmlDoc.SelectSingleNode(mXPath);
            if (AveSPCommonUtility.IsGuid(libNode.InnerText))
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
            if (listNode != null && AveSPCommonUtility.IsGuid(listNode.InnerText))
            {
                Guid listId = new Guid(listNode.InnerText);
                string listTitle = string.Empty;
                XmlNode listTitleNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'Title']");
                if (listTitleNode != null)
                {
                    listTitle = listTitleNode.InnerText;
                    if (string.IsNullOrEmpty(listTitle))
                    {
                        listTitle = webPartInfo.ListTitle;//远端back时已经将listtitle backup
                    }
                    if (!string.IsNullOrEmpty(listTitle))
                    {
                        try
                        {
                            Guid destinationListId;
                            if (Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out destinationListId))
                            {
                                List = Web.Lists[destinationListId] as AveList;
                            }
                            else
                            {
                                List = Web.Lists[listTitle] as AveList;
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Warn("Get list:{0} failed.Error Message:{1}.", listTitle, ex.ToString());
                            List = null;
                        }
                    }
                }
                Guid destListId;
                bool isListIdInCache = false;
                if (this.Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out destListId))
                {
                    isListIdInCache = true;
                }
                if (isListIdInCache || !string.IsNullOrEmpty(listTitle))
                {
                    XmlNode listIdNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'ListId']");
                    XmlNode defNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'XmlDefinition']");
                    if (defNode != null)
                    {
                        XmlDocument viewNode = new XmlDocument();
                        viewNode.LoadXml(defNode.InnerText);
                        string viewId = viewNode.DocumentElement.GetAttribute("Name");
                        Guid viewGuid = new Guid(viewId);
                        Guid viewGuidMapping;
                        if (this.Cache.SiteMappingManager.GetViewGuidMappingValue(viewGuid, out viewGuidMapping))
                        {
                            viewNode.DocumentElement.SetAttribute("Name", viewGuidMapping.ToString("B"));
                            if (this.Cache.ViewInfo != null && this.Cache.ViewInfo.Views.ContainsKey(viewGuid))
                            {
                                webPartInfo.IsViewBuildInWebPart = true;
                            }
                        }
                        EnsureViewFields(viewNode);
                        defNode.InnerText = viewNode.OuterXml;
                    }
                    if (isListIdInCache)
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
    }
    public class ListFormWebpartUpdater : AveBaseViewWebPartUpdater
    {
        public ListFormWebpartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
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
            return UpdateWebId(definationXmlDoc) || UpdateRelativeUrl(definationXmlDoc) || UpdateListName(webpartInfo, definationXmlDoc);
        }

        private bool UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode libNode = definationXmlDoc.SelectSingleNode(mXPath);
            if (libNode != null && AveSPCommonUtility.IsGuid(libNode.InnerText))
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                if (oldLibId.Equals(Guid.Empty))
                {
                    return needPostRestore;
                }
                Guid destinationListId;
                if (!Cache.SiteMappingManager.GetValueFromListIdMapping(oldLibId,out destinationListId))
                {
                    needPostRestore = true;
                    return needPostRestore;
                }
                libNode.InnerText = destinationListId.ToString();
            }
            return needPostRestore;
        }
    }
    public class BusinessDataWebpartUpdater : AveWebPartPropertyUpdater
    {
        public BusinessDataWebpartUpdater(AveWebPartCache webpartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webpartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            return UpdateListNameAndId(webpartInfo, definationXmlDoc);
        }
    }

    public class CategoryResultsWebPart : AveWebPartPropertyUpdater
    {
        public CategoryResultsWebPart(AveWebPartCache webpartCache, AveWebPartLinkUpdater webPartLinkUpdater)
            : base(webpartCache, webPartLinkUpdater) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            return UpdateListNameAndId(webpartInfo, definationXmlDoc);
        }
    }
    #endregion

    public class AveClientWebPartUrlHandlerFactory
    {
        private static Dictionary<Guid, AveWebPartType> WebPartUpdaterMapping = new Dictionary<Guid, AveWebPartType>();

        static AveClientWebPartUrlHandlerFactory()
        {
            WebPartUpdaterMapping[new Guid("7494019e-cc3c-dc3d-88ee-f9782d55ba37")] = AveWebPartType.TableOfContentsWebPart;
            WebPartUpdaterMapping[new Guid("b2b35bdf-5e78-ab22-5351-6639ca63203f")] = AveWebPartType.ContentEditorWebPart;
            WebPartUpdaterMapping[new Guid("b4bd2bdf-cf0c-ffce-ecb1-ae7c4882e17a")] = AveWebPartType.ExcelWebRendererWebPart;
            WebPartUpdaterMapping[new Guid("d9731c15-6aeb-ae5f-0994-e8f6bd13ff10")] = AveWebPartType.VisioWebAccessWebPart;
            WebPartUpdaterMapping[new Guid("107AB2DC-58A6-809C-9B41-F2E17E6E064F")] = AveWebPartType.ContentByQueryWebPart;
            WebPartUpdaterMapping[new Guid("874f5460-71f9-fecc-e894-e7e858d9713e")] = AveWebPartType.XsltListViewWebPart;
            WebPartUpdaterMapping[new Guid("2242cce6-491a-657a-c8ee-b10a2a993eda")] = AveWebPartType.ListViewWebPart;
            WebPartUpdaterMapping[new Guid("7fbf9a80-8ae1-fa7e-9c51-30a786d33155")] = AveWebPartType.ListViewWebPart;
            WebPartUpdaterMapping[new Guid("baf5274e-a800-8dc3-96d0-0003d9405663")] = AveWebPartType.ListViewWebPart;
            WebPartUpdaterMapping[new Guid("9f56656f-6aa3-0d55-a812-711bf65864ea")] = AveWebPartType.ListFormWebPart;
            WebPartUpdaterMapping[new Guid("1a8eda1f-6a8c-d5b9-0a7a-062455488c90")] = AveWebPartType.ListFormWebPart;
            WebPartUpdaterMapping[new Guid("293e8d0e-486f-e21e-40e3-75bfb77202de")] = AveWebPartType.ListFormWebPart;
            WebPartUpdaterMapping[new Guid("bdf3c494-4f90-8428-15f5-49220aa08d98")] = AveWebPartType.SummaryLinkWebPart;
            WebPartUpdaterMapping[new Guid("db128878-9a93-4768-2256-cc2c390ffb57")] = AveWebPartType.SummaryLinkWebPart;
            WebPartUpdaterMapping[new Guid("9afe11f2-9603-ac36-62a9-debeb61bcac0")] = AveWebPartType.TagCloudWebPart;
            WebPartUpdaterMapping[new Guid("e25ec220-41d8-6e8e-2d58-d685e621a47e")] = AveWebPartType.SocialCommentWebPart;

            //2013
            WebPartUpdaterMapping[new Guid("bd5d3ea4-8040-1691-574c-5bdad906238d")] = AveWebPartType.TableOfContentsWebPart;
            WebPartUpdaterMapping[new Guid("4c06cea2-364f-47e3-e1d7-08d53f441157")] = AveWebPartType.ContentEditorWebPart;
            WebPartUpdaterMapping[new Guid("066cabc4-48cb-ae18-e7c6-953875ac7ed6")] = AveWebPartType.ExcelWebRendererWebPart;
            WebPartUpdaterMapping[new Guid("bfff2915-72aa-45d2-5929-54d47ab82a4e")] = AveWebPartType.VisioWebAccessWebPart;
            WebPartUpdaterMapping[new Guid("c13236c3-5cc0-ad43-e5cc-8790ba11a7bb")] = AveWebPartType.ContentByQueryWebPart;
            WebPartUpdaterMapping[new Guid("a6524906-3fd2-ee4e-23ee-252d3c6e0dc9")] = AveWebPartType.XsltListViewWebPart;
            WebPartUpdaterMapping[new Guid("05d0fd94-372a-5ee7-b480-ccb8f9cd2c23")] = AveWebPartType.ListViewWebPart;
            WebPartUpdaterMapping[new Guid("42fddde2-e0cf-c8ab-48b7-db1fcac0a917")] = AveWebPartType.ListFormWebPart;
            WebPartUpdaterMapping[new Guid("62961f97-6029-0309-2def-fa1531f5f226")] = AveWebPartType.SummaryLinkWebPart;
            WebPartUpdaterMapping[new Guid("eb962a66-5ba1-76c6-4a2f-eaaea9486f91")] = AveWebPartType.TagCloudWebPart;
            WebPartUpdaterMapping[new Guid("e97ff0f2-57f9-7cad-bb0a-5bfe3ea30cd1")] = AveWebPartType.SocialCommentWebPart;
            WebPartUpdaterMapping[new Guid("3c5da7f7-4804-bd53-b38e-a411e20d6aeb")] = AveWebPartType.BusinessDataWebPart;
            WebPartUpdaterMapping[new Guid("a817b3e7-8db0-090a-2a28-23d054a36013")] = AveWebPartType.BusinessDataWebPart;
            WebPartUpdaterMapping[new Guid("4aaa156a-db8b-5d45-2b5f-4d941b70f309")] = AveWebPartType.BusinessDataWebPart;
            WebPartUpdaterMapping[new Guid("8bd7632b-46fb-13f4-d081-4095becac22b")] = AveWebPartType.XMLWebPart;
            WebPartUpdaterMapping[new Guid("aa995cba-0d36-1807-8224-9ad08ca39e36")] = AveWebPartType.CategoryResultsWebPart;
            WebPartUpdaterMapping[new Guid("E6218CA5-B379-8D58-1EAD-99AED88F5246")] = AveWebPartType.ScriptEditorWebPart;
        }

        public static AveWebPartPropertyUpdater GenerateWebPartUrlHanlder(Guid webPartId, IAveWeb web, XmlNode webPartNode, AveWebPartCache webPartCache)
        {
            return CreateWebpartUpdateInstance(webPartId, web, webPartCache, GreateLinkUpdateInstance(webPartNode));
        }

        private static AveWebPartPropertyUpdater CreateWebpartUpdateInstance(Guid webPartId, IAveWeb web, AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater)
        {
            AveWebPartType webPartType = AveWebPartType.DefaultWebpartType;
            WebPartUpdaterMapping.TryGetValue(webPartId, out webPartType);

            switch (webPartType)
            {
                case AveWebPartType.ContentByQueryWebPart:
                    return new ContentByQueryWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.ContentEditorWebPart:
                    return new ContentEditorWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.ExcelWebRendererWebPart:
                    return new ExcelWebRendererWebPartUpdater(webPartCache, webPartLinkUpdater);
                case AveWebPartType.TableOfContentsWebPart:
                    return new TableOfContentsWebPartUpdater(webPartCache, webPartLinkUpdater);
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
                    return new CategoryResultsWebPart(webPartCache, webPartLinkUpdater);
                case AveWebPartType.ScriptEditorWebPart:
                    return new ScriptEditorUpdater(webPartCache, webPartLinkUpdater);
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

        internal enum AveWebPartType
        {
            DefaultWebpartType,
            ContentByQueryWebPart,
            ContentEditorWebPart,
            ExcelWebRendererWebPart,
            TableOfContentsWebPart,
            VisioWebAccessWebPart,
            ListViewWebPart,
            ListFormWebPart,
            XsltListViewWebPart,
            SummaryLinkWebPart,
            TagCloudWebPart,
            SocialCommentWebPart,
            BusinessDataWebPart,
            XMLWebPart,
            CategoryResultsWebPart,
            ScriptEditorWebPart
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
                    if (node.InnerText.ToString().Contains('/'))
                    {
                        node.InnerText = AveReplaceProcessor.UrlReplace(node.InnerText.ToString(), cache.SiteManagedMappings, new ReplaceOption(true, true), cache.SourceSiteInfo, cache.DestSiteInfo.ServerRelativeUrl);
                    }
                }
                if (node.Name.Equals("ID", StringComparison.OrdinalIgnoreCase) && node.InnerText.StartsWith("g_", StringComparison.OrdinalIgnoreCase))
                {
                    node.InnerText = "";
                }
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
                        if (node.Attributes.Count > 0 && (node.Attributes[0].Value.EndsWith("Link", StringComparison.OrdinalIgnoreCase) || node.Attributes[0].Value.EndsWith("Uri", StringComparison.OrdinalIgnoreCase) || node.Attributes[0].Value.EndsWith("URL", StringComparison.OrdinalIgnoreCase) || node.Attributes[0].Value.Equals("MediaSource")))
                        {
                            if (node.InnerText.ToString().Contains('/'))
                            {
                                if (node.InnerText.StartsWith("~sitecollection", StringComparison.OrdinalIgnoreCase))
                                {
                                    string url = node.InnerText.Replace("~sitecollection", cache.SourceSiteInfo.ServerRelativeUrl);
                                    string replaceUrl = AveReplaceProcessor.UrlReplace(url, cache.SiteManagedMappings, new ReplaceOption(true, true), cache.SourceSiteInfo, cache.DestSiteInfo.ServerRelativeUrl);
                                    node.InnerText = replaceUrl.Replace(cache.DestSiteInfo.ServerRelativeUrl, "~sitecollection");
                                }
                                else
                                {
                                    node.InnerText = AveReplaceProcessor.UrlReplace(node.InnerText.ToString(), cache.SiteManagedMappings, new ReplaceOption(true, true), cache.SourceSiteInfo, cache.DestSiteInfo.ServerRelativeUrl);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion
}