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
using System.Collections.Concurrent;

namespace AvePoint.ObjectModel.Common.WebPart
{
    public abstract class AveWebPartPropertyUpdater
    {
        public AveWebPartCache Cache = null;
        public AveWebPartLinkUpdater LinkUpdater;
        public IAveWeb mWeb;
        protected static AveLogger mLogger = AveLogger.GetInstance(typeof(AveWebPartPropertyUpdater));

        protected AveWebPartPropertyUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
        {
            mWeb = web;
            Cache = webPartCache;
            LinkUpdater = webPartLinkUpdater;
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
                if (Cache.ListIdMapping.ContainsKey(oldLibId))
                {
                    #region add to post action cache if the view id is not found for replicator library level multi-thread.
                    XmlNode viewIdNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'ViewGuid']");
                    lock (Cache.ViewGuidMapping)
                    {
                        if (viewIdNode != null && Cache.ViewGuidMapping.ContainsKey(new Guid(viewIdNode.InnerText)))
                        {
                            viewIdNode.InnerText = Cache.ViewGuidMapping[new Guid(viewIdNode.InnerText)].ToString();
                            libNode.InnerText = Cache.ListIdMapping[oldLibId].ToString();
                        }
                        else
                        {
                            webpartInfo.ListId = oldLibId;
                            needPostRestore = true;
                        }
                    }
                    #endregion
                }
                else
                {
                    webpartInfo.ListId = oldLibId;
                    needPostRestore = true;
                }
            }
            return needPostRestore;
        }

        protected bool ReplaceListIdProperties(XmlDocument definationXmlDoc)
        {
            string[] nodeNames = new string[] { "ListName", "ListId" };
            bool needPostRestore = false;
            foreach (string nodeName in nodeNames)
            {
                XmlNode listNode = definationXmlDoc.SelectSingleNode(".//*[@name = '" + nodeName + "']");
                if (listNode != null && IsGuid(listNode.InnerText))
                {
                    Guid listId = new Guid(listNode.InnerText);
                    if (listId.Equals(Guid.Empty))
                    {
                        continue;
                    }
                    else if (this.Cache.ListIdMapping.ContainsKey(listId))
                    {
                        listNode.InnerText = string.Equals(nodeName, "ListName", StringComparison.OrdinalIgnoreCase) ?
                            "{" + this.Cache.ListIdMapping[listId].ToString() + "}" :
                            this.Cache.ListIdMapping[listId].ToString();
                    }
                    else
                    {
                        needPostRestore = true;
                    }
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

        protected void UpdateLinkUrl(HtmlDocument replaceDocument, string rootNodeName, string linkNodeName)
        {
            HtmlNodeCollection linkNodes = replaceDocument.DocumentNode.SelectNodes(rootNodeName);
            if (linkNodes != null)
            {
                foreach (HtmlNode linkNode in linkNodes)
                {
                    if (!string.IsNullOrEmpty(linkNode.GetAttributeValue(linkNodeName, string.Empty)))
                    {
                        string urlNeedReplace = linkNode.GetAttributeValue(linkNodeName, string.Empty);
                        string replaceUrl = AveReplaceProcessor.UrlReplace(urlNeedReplace, Cache.SiteManagedMappings, new ReplaceOption(true, true), Cache.SourceSiteInfo, Cache.DestSiteInfo.ServerRelativeUrl);
                        linkNode.SetAttributeValue(linkNodeName, replaceUrl);
                    }
                }
            }
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
                    TypeInfo typeInfo = TypeInfo.Parse(assembly);
                    XmlNode assemblyNode = root.OwnerDocument.CreateElement("Assembly");
                    assemblyNode.InnerText = typeInfo.Assembly.FullName;
                    tempRoot.AppendChild(assemblyNode);

                    XmlNode webPartTypeNode = root.OwnerDocument.CreateElement("TypeName");
                    webPartTypeNode.InnerText = typeInfo.Name;
                    tempRoot.AppendChild(webPartTypeNode);
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
        public TagCloudWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web) :
            base(webPartCache, webPartLinkUpdater, web) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            //ConvertV3DefinitionXmlToV2(webpartInfo, definationXmlDoc.FirstChild);
            return UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }
    }
    public class SocialCommentWebPartUpdater : AveWebPartPropertyUpdater
    {
        public SocialCommentWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web) :
            base(webPartCache, webPartLinkUpdater, web) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            ConvertV3DefinitionXmlToV2(webpartInfo, definationXmlDoc.FirstChild);
            return UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }
    }
    public class TableOfContentsWebPartUpdater : XsltListViewWebPartUpdater
    {
        public TableOfContentsWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            this.UpdateAnchorLocation(definationXmlDoc);
            return UpdateWebId(definationXmlDoc)
                || UpdateListName(webpartInfo, definationXmlDoc)
                || base.UpdateRelativeUrl(definationXmlDoc)
                || UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }

        private void UpdateAnchorLocation(XmlDocument definationXmlDoc)
        {
            List<string> replaceProperties = new List<string>();
            replaceProperties.Add("AnchorLocation");
            ReplaceRelativeInfo(definationXmlDoc, replaceProperties);
        }
    }

    public class ContentEditorWebPartUpdater : AveWebPartPropertyUpdater
    {
        public ContentEditorWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
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
                HtmlDocument replaceDocument = new HtmlDocument();
                replaceDocument.OptionOutputOriginalCase = true;
                if (propertyEle.FirstChild == null || string.IsNullOrEmpty(propertyEle.FirstChild.InnerText))
                {
                    continue;
                }
                replaceDocument.LoadHtml(propertyEle.FirstChild.InnerText);
                Dictionary<string, string> linkNodes = new Dictionary<string, string>();
                linkNodes["//img"] = "src";
                linkNodes["//a"] = "href";
                foreach (KeyValuePair<string, string> linkNode in linkNodes)
                {
                    UpdateLinkUrl(replaceDocument, linkNode.Key, linkNode.Value);
                }
                propertyEle.FirstChild.InnerText = replaceDocument.DocumentNode.OuterHtml;
            }
        }
    }

    public class ScriptEditorWebPartUpdater : AveWebPartPropertyUpdater
    {
        public ScriptEditorWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            UpdateScriptUrl(definationXmlDoc);
            return false;
        }

        private void UpdateScriptUrl(XmlDocument needReplaceScript)
        {
            XmlNode scriptContent = needReplaceScript.DocumentElement.SelectSingleNode(".//*[@name='Content']");
            if (scriptContent == null || string.IsNullOrEmpty(scriptContent.InnerText))
            {
                return;
            }
            HtmlDocument replaceDocument = new HtmlDocument();
            replaceDocument.OptionOutputOriginalCase = true;
            replaceDocument.LoadHtml(scriptContent.InnerText);
            Dictionary<string, string> linkNodes = new Dictionary<string, string>();
            linkNodes["//img"] = "src";
            //linkNodes["//a"] = "href";
            linkNodes["//a"] = "rel";
            linkNodes["//link"] = "href";
            linkNodes["//script"] = "src";
            linkNodes["//image"] = "src";
            linkNodes["//embed"] = "src";
            linkNodes["//bgsound"] = "src";

            foreach (KeyValuePair<string, string> linkNode in linkNodes)
            {
                UpdateLinkUrl(replaceDocument, linkNode.Key, linkNode.Value);
            }
            scriptContent.InnerText = replaceDocument.DocumentNode.OuterHtml;
        }
    }

    public class ExcelWebRendererWebPartUpdater : AveWebPartPropertyUpdater
    {
        public ExcelWebRendererWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
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
        public VisioWebAccessWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
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
        public DefaultWebPartUrlUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        private bool UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            return false;
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            bool needPostRestore = UpdateLibraryGuid(webpartInfo, definationXmlDoc);
            needPostRestore = ReplaceListIdProperties(definationXmlDoc) || needPostRestore;
            return UpdateRelativeUrl(definationXmlDoc) || needPostRestore;
        }
    }
    public class XMLWebPartUpdater : AveWebPartPropertyUpdater
    {
        public XMLWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web) :
            base(webPartCache, webPartLinkUpdater, web) { }
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
                StringBuilder sb = new StringBuilder(node.InnerXml);
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
                node.InnerXml = sb.ToString();
            }
        }

        public static Dictionary<int, string> GetTagList(string sHtmlText, string tag, string attr)
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
    public class SummaryLinkWebpartUpdater : AveWebPartPropertyUpdater
    {
        public SummaryLinkWebpartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web) :
            base(webPartCache, webPartLinkUpdater, web) { }
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
                        urlNode.InnerText = AveReplaceProcessor.UrlReplace(urlNode.InnerText.ToString(), this.Cache.SiteManagedMappings, new ReplaceOption(true, true), this.Cache.SourceSiteInfo, this.Cache.DestSiteInfo.ServerRelativeUrl, true);
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
                    if (listId.Equals(Guid.Empty))
                    {
                        continue;
                    }
                    else if (this.Cache.ListIdMapping.ContainsKey(listId))
                    {
                        listNode.InnerText = "{" + this.Cache.ListIdMapping[listId].ToString() + "}";
                    }
                    else
                    {
                        //SAAS-34134 - Set the list ID to fuid.Empty, so don't need to udate list while udate webpart properties.
                        listNode.InnerText = "{" + Guid.Empty.ToString() + "}";
                        needPost = true;
                    }
                }
            }
            //[SAAS-34134] For summary links webparts, don't need to update list ID since the list ID may not exist and it doesn't affect the links showing in the page
            return false;
        }
    }

    public class BlogLinksWebPartUpdater : AveWebPartPropertyUpdater
    {
        public BlogLinksWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        private bool ReplaceListIdProperties(XmlDocument definationXmlDoc)
        {
            bool needPost = false;
            XmlNode listNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'ListId']");
            if (listNode != null && IsGuid(listNode.InnerText))
            {
                Guid listId = new Guid(listNode.InnerText);
                if (listId.Equals(Guid.Empty))
                {
                    return needPost;
                }
                if (this.Cache.ListIdMapping.ContainsKey(listId))
                {
                    listNode.InnerText = this.Cache.ListIdMapping[listId].ToString();
                }
                else
                {
                    needPost = true;
                }
            }
            return needPost;
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            return ReplaceListIdProperties(definationXmlDoc);
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
            return UpdateContentTypeIdAndFormLocation(webpartInfo, definationXmlDoc);
        }

        private bool UpdateContentTypeIdAndFormLocation(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            bool needPost = false;
            string contentTypeIdXPath = ".//*[@name = 'ContentTypeId']";
            XmlNode contentTypeIdNode = definationXmlDoc.SelectSingleNode(contentTypeIdXPath);
            string formLocationXPath = ".//*[@name = 'FormLocation']";
            XmlNode formLocationNode = definationXmlDoc.SelectSingleNode(formLocationXPath);
            if (contentTypeIdNode != null && formLocationNode != null)
            {
                string destListUrl = AveReplaceProcessor.UrlReplace(formLocationNode.InnerText, this.Cache.ListUrlMapping, new ReplaceOption(true, true), this.Cache.SourceSiteInfo, this.Cache.DestSiteInfo.ServerRelativeUrl);
                IAveList list = mWeb.GetList(destListUrl);
                if (list != null)
                {
                    formLocationNode.InnerText = destListUrl;
                    if (this.Cache.ListLevelCTIdMapping.ContainsKey(contentTypeIdNode.InnerText))
                    {
                        contentTypeIdNode.InnerText = this.Cache.ListLevelCTIdMapping[contentTypeIdNode.InnerText].ToString();
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
    public class ContentBySearchWebPartUpdater : AveWebPartPropertyUpdater
    {
        public ContentBySearchWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        private bool UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            return false;
        }

        private bool UpdateDataProviderJSON(AveWebPartCache cache,XmlNode definitionXml)
        {
            XmlNode webPartNode = definitionXml.FirstChild;
            if ((webPartNode as XmlElement) == null)
            {
                foreach (XmlNode node in definitionXml.ChildNodes)
                {
                    if (node is XmlElement)
                    {
                        webPartNode = node;
                    }
                }
            }
            var childNodes = webPartNode?.ChildNodes;
            if (childNodes == null)
            {
                mLogger.Error("child nodes is null");
                throw new Exception("child nodes is null");
            }
            if (string.Equals(webPartNode?.Name, "WebParts", StringComparison.OrdinalIgnoreCase))
            {
                foreach (XmlNode node in childNodes)
                {
                    if ((node is XmlElement) && string.Equals(node.Name, "WebPart", StringComparison.OrdinalIgnoreCase))
                    {
                        webPartNode = node;
                        break;
                    }
                }
            }
            bool needPost = false;
            foreach (XmlNode pNode in childNodes)
            {
                if (pNode.Name.Equals("data"))
                {
                    XmlNode tempNode = pNode.FirstChild;
                    if (tempNode == null)
                    {
                        return needPost;
                    }
                    foreach (XmlNode node in tempNode.ChildNodes)
                    {
                        if (string.Equals(node.Attributes[0].Value, "DataProviderJSON", StringComparison.OrdinalIgnoreCase))
                        {
                            node.InnerText = AveContentBySearchWebPartUtility.UpdateDataProviderJsonProperty(node.InnerText, cache, out needPost);
                        }
                    }
                }
            }
            return needPost;
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            bool needPostRestore = UpdateLibraryGuid(webpartInfo, definationXmlDoc);
            needPostRestore = ReplaceListIdProperties(definationXmlDoc) || needPostRestore;
            needPostRestore |= UpdateDataProviderJSON(Cache, definationXmlDoc);
            return UpdateRelativeUrl(definationXmlDoc) || needPostRestore;
        }
    }
    public class ContentByQueryWebPartUpdater : AveWebPartPropertyUpdater
    {
        private const string mXPath = ".//*[@name = 'ListGuid']";

        public ContentByQueryWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            return UpdateRelativeUrl(webpartInfo, definationXmlDoc) || UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }

        private bool UpdateRelativeUrl(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode libNode = definationXmlDoc.SelectSingleNode(mXPath);
            if (AveTypeHelper.IsGuid(libNode.InnerText))
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                if (oldLibId.Equals(Guid.Empty))
                {
                    return needPostRestore;
                }
                else if (!string.IsNullOrEmpty(webpartInfo.ListTitle))
                {
                    IAveWeb currentWeb = mWeb;
                    XmlNode webNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'WebUrl']");
                    if (webNode != null)
                    {
                        string webUrl = "";
                        if (currentWeb.Site.ServerRelativeUrl.Equals("/"))
                        {
                            webUrl = webNode.InnerText.Replace("~sitecollection", ""); //SAAS-11428 以前没考虑到subsite的情况
                        }
                        else
                        {
                            webUrl = webNode.InnerText.Replace("~sitecollection", currentWeb.Site.ServerRelativeUrl); //SAAS-11428 以前没考虑到subsite的情况
                        }
                        //string webUrl = webNode.InnerText.Replace("~sitecollection", "").TrimEnd('/');
                        if (!mWeb.ServerRelativeUrl.Equals(webUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            currentWeb = mWeb.Site.OpenWeb(webUrl);
                        }
                    }
                    IAveList list = currentWeb.Lists.GetByTitle(webpartInfo.ListTitle);
                    if (list != null)
                    {
                        libNode.InnerText = list.ID.ToString();
                        return needPostRestore;
                    }
                }
                if (Cache.ListIdMapping == null || !Cache.ListIdMapping.ContainsKey(oldLibId))
                {
                    needPostRestore = true;
                    return needPostRestore;
                }

                try
                {
                    XmlNode contenttypeid = definationXmlDoc.SelectSingleNode(".//*[@name = 'ContentTypeBeginsWithId']");
                    string ctID = contenttypeid.InnerText;
                    XmlNode listGuidNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'ListGuid']");
                    string listGuidid = listGuidNode.InnerText;

                    if (Cache.DesListCTIdMapping != null && ctID.Trim() != "" && listGuidid.Trim() != "")
                    {
                        if (listGuidid != Guid.Empty.ToString())
                        {
                            string desListID = string.Empty;
                            if (Cache.ListIdMapping != null)
                            {
                                Guid listid = new Guid(listGuidid);
                                if (Cache.ListIdMapping.ContainsKey(listid))
                                {
                                    desListID = Cache.ListIdMapping[listid].ToString();
                                }
                            }
                            if (desListID != string.Empty)
                            {
                                if (Cache.DesListCTIdMapping.ContainsKey(desListID))
                                {
                                    Dictionary<string, IAveContentTypeId> temp = Cache.DesListCTIdMapping[desListID];
                                    string tempkey = string.Empty;
                                    foreach (string key in temp.Keys)
                                    {
                                        if (key.Contains(ctID) && (tempkey == string.Empty || key.Length < tempkey.Length))
                                        {
                                            tempkey = key;
                                        }
                                    }
                                    if (tempkey != string.Empty)
                                    {
                                        contenttypeid.InnerText = temp[tempkey].Parent.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("An error occurred while set webpart'ct id: " + e.Message + e.StackTrace);
                }

                libNode.InnerText = Cache.ListIdMapping[oldLibId].ToString();
            }
            return needPostRestore;
        }

        protected override bool IsDependentObjectRestored(XmlDocument definationXmlDoc)
        {
            XmlNode libNode = definationXmlDoc.SelectSingleNode(mXPath);
            if (AveTypeHelper.IsGuid(libNode.InnerText))
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                return Cache.ListIdMapping.ContainsKey(oldLibId);
            }
            return true;
        }
    }

    public class SlideShowWebPartUpdater : AveWebPartPropertyUpdater
    {
        public SlideShowWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            return UpdateLibraryGuid(webpartInfo, definationXmlDoc);
        }
    }

    public abstract class AveBaseViewWebPartUpdater : AveWebPartPropertyUpdater
    {

        public IAveWeb Web;
        public IAveList List;

        public AveBaseViewWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
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
                if (!this.Cache.WebIDMapping.ContainsKey(webId))
                {
                    needPostRestore = true;
                    return needPostRestore;
                }
                webNode.InnerText = Cache.WebIDMapping[webId].ToString();
            }
            return needPostRestore;
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
                                fieldName = GetFieldInternalName(fieldName);
                                if (fieldName != String.Empty)
                                {
                                    XmlElement newNode = xmlDoc.CreateElement("FieldRef");
                                    newNode.SetAttribute("Name", fieldName);
                                    foreach (XmlAttribute attr in childNode.Attributes)
                                    {
                                        if (attr.Value.Equals("Name", StringComparison.Ordinal))
                                        {
                                            continue;
                                        }
                                        newNode.SetAttribute(attr.Name, attr.Value);
                                    }
                                    rootNode.AppendChild(newNode);
                                }
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Ensure View field:{0} failed.Error Message:{1}", fieldName, ex.ToString());
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
            /*review-qlluo*/catch (Exception e)
            {
                if (Cache.FieldDisplayNameMapping.ContainsKey(sourceFieldName))
                {
                    destinationFieldName = Cache.FieldDisplayNameMapping[sourceFieldName];
                }
                else if (Cache.FieldInternalNameMapping.ContainsKey(sourceFieldName))
                {
                    destinationFieldName = Cache.FieldInternalNameMapping[sourceFieldName];
                }
                else
                {
                    throw new Exception(e.ToString());
                }
            }
            return destinationFieldName;
        }

        protected string GetFieldInternalName(string sourceInternalName)
        {
            string destinationFieldName = sourceInternalName;
            if (Cache.ListFieldsMapping.ContainsKey(List.ID))
            {
                IAveFieldMapping fieldMapping = Cache.ListFieldsMapping[List.ID];
                destinationFieldName = fieldMapping.GetMappingRestoredFieldInternalName(sourceInternalName);
                if (string.IsNullOrEmpty(destinationFieldName))
                {
                    destinationFieldName = sourceInternalName;
                }
            }
            else if (Cache.FieldInternalNameMapping.ContainsKey(sourceInternalName))
            {
                destinationFieldName = Cache.FieldInternalNameMapping[sourceInternalName];
            }
            else
            {
                IAveField desField = List.Fields.GetField(sourceInternalName);
                destinationFieldName = desField.InternalName;
            }
            return destinationFieldName;
        }
    }

    public class RssWebPartUpdater : XsltListViewWebPartUpdater
    {
        public RssWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
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

        protected bool UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode libNode = definationXmlDoc.SelectSingleNode(mXPath);
            if (AveTypeHelper.IsGuid(libNode.InnerText))
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                if (oldLibId.Equals(Guid.Empty))
                {
                    return needPostRestore;
                }
                if (Cache.ListIdMapping != null && Cache.ListIdMapping.ContainsKey(oldLibId))
                {
                    libNode.InnerText = Cache.ListIdMapping[oldLibId].ToString();
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

        //private void UpdateWebId(XmlDocument definationXmlDoc)
        //{
        //    XmlNode webNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'WebId']");
        //    if (webNode != null)
        //    {
        //        Guid webId = new Guid(webNode.InnerText);
        //        if (webId == Guid.Empty || Cache.WebIDMapping.ContainsKey(webId))
        //        {
        //            if (webId != Guid.Empty)
        //            {
        //                webNode.InnerText = Cache.WebIDMapping[webId].ToString();
        //            }
        //        }
        //    }
        //}

        protected bool UpdateListName(AveWebPartBaseInfo webPartInfo, XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode listNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'ListName']");
            if (listNode != null && AveTypeHelper.IsGuid(listNode.InnerText))
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
                            if (Cache.ListIdMapping.ContainsKey(listId))
                            {
                                List = Web.Lists.GetById(Cache.ListIdMapping[listId]);
                            }
                            else
                            {
                                List = Web.GetListByName(listTitle, false);
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Warn("Get list:{0} failed.Error Message:{1}", listTitle, ex.ToString());
                            List = null;
                        }
                    }
                }
                if (this.Cache.ListIdMapping.ContainsKey(listId) || !string.IsNullOrEmpty(listTitle))
                {
                    XmlNode listIdNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'ListId']");
                    XmlNode defNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'XmlDefinition']");
                    Guid currentViewId = Guid.Empty;
                    if (defNode != null && !string.IsNullOrEmpty(defNode.InnerText))
                    {
                        XmlDocument viewNode = new XmlDocument();
                        viewNode.LoadXml(defNode.InnerText);
                        string viewId = viewNode.DocumentElement.GetAttribute("Name");
                        Guid viewGuid = new Guid(viewId);
                        lock (Cache.ViewGuidMapping)
                        {
                            if (this.Cache.ViewGuidMapping.ContainsKey(viewGuid))
                            {
                                currentViewId = this.Cache.ViewGuidMapping[viewGuid];
                                viewNode.DocumentElement.SetAttribute("Name", "{" + currentViewId.ToString() + "}");
                                if (this.Cache.ViewInfo != null && this.Cache.ViewInfo.Views.ContainsKey(currentViewId))
                                {
                                    webPartInfo.IsViewBuildInWebPart = true;
                                }
                            }
                        }
                        EnsureViewFields(viewNode);
                        defNode.InnerText = viewNode.OuterXml;
                    }
                    if (this.Cache.ListIdMapping.ContainsKey(listId))
                    {
                        listNode.InnerText = "{" + this.Cache.ListIdMapping[listId].ToString() + "}";
                        if (listIdNode != null)
                        {
                            listIdNode.InnerText = this.Cache.ListIdMapping[listId].ToString();
                        }
                    }
                    else
                    {
                        if (List != null)
                        {
                            listId = List.ID;
                            listNode.InnerText = "{" + listId.ToString() + "}";
                            if (listIdNode != null)
                            {
                                listIdNode.InnerText = listId.ToString();
                            }
                        }
                        else
                        {
                            //return 2;
                            needPostRestore = true;
                        }
                    }
                    //return 1;
                }
                else
                {
                    //return 2;
                    needPostRestore = true;
                }
            }
            //else
            //{
            //    // return 1;
            //}
            return needPostRestore;
        }
    }

    public class ProjectFieldPartUpdater : AveBaseViewWebPartUpdater
    {
        private const string mFieldListXPath = ".//*[@name = 'FieldList']";

        public ProjectFieldPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        { }


        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            return UpdateFieldList(definationXmlDoc);
        }

        public bool UpdateFieldList(XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode libNode = definationXmlDoc.SelectSingleNode(mFieldListXPath);
            string fieldIds = libNode.InnerText;
            string[] fieldList = fieldIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (fieldList.Length > 0)
            {
                ConcurrentDictionary<Guid, Guid> mapping = Cache.ProjectCustomFieldIdMapping;
                Guid sourceFiledId;
                Guid tempFiledId;
                foreach (var fieldId in fieldList)
                {
                    if (AveTypeHelper.IsGuid(fieldId))
                    {
                        sourceFiledId = new Guid(fieldId);
                        if (mapping.TryGetValue(sourceFiledId, out tempFiledId))
                        {
                            fieldIds = fieldIds.Replace(fieldId, tempFiledId.ToString());
                            needPostRestore = true;
                        }
                        else
                        {
                            mLogger.Warn("Canot find custom field in mapping: {0}", fieldId);
                        }
                    }
                }       
            }
            if (needPostRestore)
            {
                mLogger.Info("Need update CustomFiled in Project Field Webpart, Source: {0}{2}Destination: {1}", libNode.InnerText, fieldIds, Environment.NewLine);
                libNode.InnerText = fieldIds;
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
        private bool UpdateListName(AveWebPartBaseInfo webPartInfo, XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode listIdNode = definationXmlDoc.SelectSingleNode("//*[name() = 'ListId']");
            XmlNode listNameNode = definationXmlDoc.SelectSingleNode("//*[name() = 'ListName']");
            if (listIdNode != null && AveTypeHelper.IsGuid(listIdNode.InnerText))
            {
                Guid listId = new Guid(listIdNode.InnerText);
                string listTitle = string.Empty;
                XmlNode listTitleNode = definationXmlDoc.SelectSingleNode(".//*[name() = 'Title']");
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
                            if (Cache.ListIdMapping.ContainsKey(listId))
                            {
                                List = Web.Lists[Cache.ListIdMapping[listId]];
                            }
                            else
                            {
                                List = Web.Lists[listTitle];
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Warn("Get list:{0} failed.Error Message:{1}", listTitle, ex.ToString());
                            List = null;
                        }
                    }
                }
                if (this.Cache.ListIdMapping.ContainsKey(listId) || !string.IsNullOrEmpty(listTitle))
                {
                    XmlNode defNode = definationXmlDoc.SelectSingleNode("//*[name() = 'ListViewXml']");
                    Guid currentViewId = Guid.Empty;
                    if (defNode != null)
                    {
                        XmlDocument viewNode = new XmlDocument();
                        viewNode.LoadXml(defNode.InnerText);
                        string viewId = viewNode.DocumentElement.GetAttribute("Name");
                        Guid viewGuid = new Guid(viewId);
                        lock (Cache.ViewGuidMapping)
                        {
                            if (this.Cache.ViewGuidMapping.ContainsKey(viewGuid))
                            {
                                currentViewId = this.Cache.ViewGuidMapping[viewGuid];
                                viewNode.DocumentElement.SetAttribute("Name", "{" + currentViewId.ToString() + "}");
                                if (this.Cache.ViewInfo != null && this.Cache.ViewInfo.Views.ContainsKey(currentViewId))
                                {
                                    webPartInfo.IsViewBuildInWebPart = true;
                                }
                            }
                        }
                        EnsureViewFields(viewNode);
                        defNode.InnerText = viewNode.OuterXml;
                    }
                    if (this.Cache.ListIdMapping.ContainsKey(listId))
                    {
                        if (listNameNode != null)
                        {
                            listNameNode.InnerText = "{" + this.Cache.ListIdMapping[listId].ToString() + "}";
                        }
                        if (listIdNode != null)
                        {
                            listIdNode.InnerText = this.Cache.ListIdMapping[listId].ToString();
                        }
                    }
                    else
                    {
                        if (List != null)
                        {
                            listId = List.ID;
                            if (listNameNode != null)
                            {
                                listNameNode.InnerText = "{" + listId.ToString() + "}";
                            }
                            if (listIdNode != null)
                            {
                                listIdNode.InnerText = listId.ToString();
                            }
                        }
                        else
                        {
                            //return 2;
                            needPostRestore = true;
                        }
                    }
                    //return 1;
                }
                else
                {
                    //return 2;
                    needPostRestore = true;
                }
            }
            //else
            //{
            //   //return 1;
            //}
            return needPostRestore;
        }
    }
    public class DataFormWebPartUpdater : AveBaseViewWebPartUpdater
    {
        public DataFormWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            var needPostRestore = ReplaceListIdProperties(definationXmlDoc);
            return UpdateDataFormRelativeProperties(definationXmlDoc) || needPostRestore;
        }

        private bool UpdateDataFormRelativeProperties(XmlDocument definationXmlDoc)
        {
            var needPostRestore = false;

            try
            {
                var parameterBindingsNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'ParameterBindings']");
                if (parameterBindingsNode != null && !string.IsNullOrEmpty(parameterBindingsNode.InnerText))
                {
                    // 缓存原端目的端 listId 和 weburl 的mapping
                    Dictionary<string, string> sourceDestPropertiesMapping = new Dictionary<string, string>();

                    var xmlDoc = new XmlDocument();
                    var parameterBindingsXml = string.Format("<ParameterBindings>{0}</ParameterBindings>", parameterBindingsNode.InnerText);
                    xmlDoc.LoadXml(parameterBindingsXml);

                    XmlNode listNode = xmlDoc.SelectSingleNode(".//*[@Name = 'ListID']");
                    if (listNode != null)
                    {
                        var listIdAttribute = listNode.Attributes.GetNamedItem("DefaultValue");
                        if (listIdAttribute != null && IsGuid(listIdAttribute.Value))
                        {
                            Guid listId = new Guid(listIdAttribute.Value);
                            if (!listId.Equals(Guid.Empty))
                            {
                                var mappedGuid = Guid.Empty;
                                if (this.Cache.ListIdMapping.TryGetValue(listId, out mappedGuid))
                                {
                                    listIdAttribute.Value = mappedGuid.ToString("B");
                                    sourceDestPropertiesMapping[listId.ToString("B")] = listIdAttribute.Value;
                                }
                                else
                                {
                                    needPostRestore = true;
                                }
                            }
                        }
                    }

                    var weburlNode = xmlDoc.SelectSingleNode(".//*[@Name = 'weburl']");
                    if (weburlNode != null)
                    {
                        var weburlAttribute = weburlNode.Attributes.GetNamedItem("DefaultValue");
                        if (weburlAttribute != null && !string.IsNullOrEmpty(weburlAttribute.Value))
                        {
                            var hasMapped = false;
                            var mappedWeburl = string.Empty;
                            foreach (var mappings in Cache.SiteManagedMappings)
                            {
                                if (mappings.TryGetValue(weburlAttribute.Value, out mappedWeburl))
                                {
                                    hasMapped = true;
                                    break;
                                }
                            }
                            if (hasMapped && !string.IsNullOrEmpty(mappedWeburl))//?
                            {
                                sourceDestPropertiesMapping[weburlAttribute.Value] = mappedWeburl;
                                weburlAttribute.Value = mappedWeburl;
                            }
                            else
                            {
                                needPostRestore = true;
                            }
                        }
                    }
                    parameterBindingsNode.InnerText = xmlDoc.DocumentElement.InnerXml;

                    // 根据 ParameterBindings 得到的mapping 替换 listId 和 weburl
                    var dataSourcesStringNode = definationXmlDoc.SelectSingleNode(".//*[@name = 'DataSourcesString']");
                    if (dataSourcesStringNode != null && !string.IsNullOrEmpty(dataSourcesStringNode.InnerText))
                    {
                        foreach (var mapping in sourceDestPropertiesMapping)
                        {
                            dataSourcesStringNode.InnerText = Regex.Replace(dataSourcesStringNode.InnerText, string.Format("\"{0}\"", mapping.Key), string.Format("\"{0}\"", mapping.Value), RegexOptions.IgnoreCase);
                            //dataSourcesStringNode.InnerText.Replace(string.Format("\"{0}\"", mapping.Key), string.Format("\"{0}\"", mapping.Value));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Error while updating DataFormWebpart:{0}, due to:{1}", definationXmlDoc.OuterXml, ex);
            }

            return needPostRestore;
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
            if (libNode != null && AveTypeHelper.IsGuid(libNode.InnerText))
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                if (oldLibId.Equals(Guid.Empty))
                {
                    return needPostRestore;
                }
                if (Cache.ListIdMapping == null || !Cache.ListIdMapping.ContainsKey(oldLibId))
                {
                    needPostRestore = true;
                    return needPostRestore;
                }
                libNode.InnerText = Cache.ListIdMapping[oldLibId].ToString();
            }
            return needPostRestore;
        }

        //private void UpdateWebId(XmlDocument definationXmlDoc)
        //{
        //    XmlNode webNode = definationXmlDoc.SelectSingleNode("//*[name() = 'WebId']");
        //    if (webNode != null)
        //    {
        //        Guid webId = new Guid(webNode.InnerText);
        //        if (webId != Guid.Empty && this.Cache.WebIDMapping.ContainsKey(webId))
        //        {
        //            webNode.InnerText = this.Cache.WebIDMapping[webId].ToString();
        //        }
        //    }
        //}

        private bool UpdateListName(AveWebPartBaseInfo webPartInfo, XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode listIdNode = definationXmlDoc.SelectSingleNode("//*[name() = 'ListId']");
            XmlNode listNameNode = definationXmlDoc.SelectSingleNode("//*[name() = 'ListName']");
            if (listIdNode != null && AveTypeHelper.IsGuid(listIdNode.InnerText))
            {
                Guid listId = new Guid(listIdNode.InnerText);
                string listTitle = string.Empty;
                XmlNode listTitleNode = definationXmlDoc.SelectSingleNode(".//*[name() = 'Title']");
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
                            if (Cache.ListIdMapping.ContainsKey(listId))
                            {
                                List = Web.Lists[Cache.ListIdMapping[listId]];
                            }
                            else
                            {
                                List = Web.Lists[listTitle];
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Warn("Get list:{0} failed.Error Message:{1}", listTitle, ex.ToString());
                            List = null;
                        }
                    }
                }
                if (this.Cache.ListIdMapping.ContainsKey(listId) || !string.IsNullOrEmpty(listTitle))
                {
                    XmlNode defNode = definationXmlDoc.SelectSingleNode("//*[name() = 'ListViewXml']");
                    Guid currentViewId = Guid.Empty;
                    if (defNode != null)
                    {
                        XmlDocument viewNode = new XmlDocument();
                        viewNode.LoadXml(defNode.InnerText);
                        string viewId = viewNode.DocumentElement.GetAttribute("Name");
                        Guid viewGuid = new Guid(viewId);
                        lock (Cache.ViewGuidMapping)
                        {
                            if (this.Cache.ViewGuidMapping.ContainsKey(viewGuid))
                            {
                                currentViewId = this.Cache.ViewGuidMapping[viewGuid];
                                viewNode.DocumentElement.SetAttribute("Name", "{" + currentViewId.ToString() + "}");
                                if (this.Cache.ViewInfo != null && this.Cache.ViewInfo.Views.ContainsKey(currentViewId))
                                {
                                    webPartInfo.IsViewBuildInWebPart = true;
                                }
                            }
                        }
                        EnsureViewFields(viewNode);
                        defNode.InnerText = viewNode.OuterXml;
                    }
                    if (this.Cache.ListIdMapping.ContainsKey(listId))
                    {
                        listNameNode.InnerText = "{" + this.Cache.ListIdMapping[listId].ToString() + "}";
                        if (listIdNode != null)
                        {
                            listIdNode.InnerText = this.Cache.ListIdMapping[listId].ToString();
                        }
                    }
                    else
                    {
                        if (List != null)
                        {
                            listId = List.ID;
                            listNameNode.InnerText = "{" + listId.ToString() + "}";
                            if (listIdNode != null)
                            {
                                listIdNode.InnerText = listId.ToString();
                            }
                        }
                        else
                        {
                            //return 2;
                            needPostRestore = true;
                        }
                    }
                    //return 1;
                }
                else
                {
                    //return 2;
                    needPostRestore = true;
                }
            }
            //else
            //{
            //   //return 1;
            //}
            return needPostRestore;
        }
    }

    public class SiteInCategoryWebPartUpdater : AveWebPartPropertyUpdater
    {
        private const string mXPath = ".//*[@name = 'ListId']";
        public SiteInCategoryWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            //UpdateRelativeUrl(definationXmlDoc);
            //UpdateWebId(definationXmlDoc);
            return UpdateRelativeUrl(definationXmlDoc);
        }

        private bool UpdateRelativeUrl(XmlDocument definationXmlDoc)
        {
            bool needPostRestore = false;
            XmlNode libNode = definationXmlDoc.SelectSingleNode(mXPath);
            if (libNode != null && AveTypeHelper.IsGuid(libNode.InnerText))
            {
                Guid oldLibId = new Guid(libNode.InnerText);
                if (oldLibId.Equals(Guid.Empty))
                {
                    return needPostRestore;
                }
                if (Cache.ListIdMapping == null || !Cache.ListIdMapping.ContainsKey(oldLibId))
                {
                    needPostRestore = true;
                    return needPostRestore;
                }
                libNode.InnerText = Cache.ListIdMapping[oldLibId].ToString();
            }
            return needPostRestore;
        }

    }

    public class TimeLineWebpartUpdater : AveWebPartPropertyUpdater
    {

        public TimeLineWebpartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            return UpdateListId(definationXmlDoc, webpartInfo);
        }
        private bool UpdateListId(XmlDocument definationXmlDoc, AveWebPartBaseInfo webpartInfo)
        {
            bool needPostRestore = false;
            XmlNode listId = definationXmlDoc.SelectSingleNode(".//*[@name = 'ListId']");
            if (listId != null)
            {
                Guid oldLibId = new Guid(listId.InnerText);
                if (oldLibId.Equals(Guid.Empty))
                {
                    return needPostRestore;
                }
                if (Cache.ListIdMapping.ContainsKey(oldLibId))
                {
                    listId.InnerText = Cache.ListIdMapping[oldLibId].ToString();
                    XmlNode sourceSelection = definationXmlDoc.SelectSingleNode(".//*[@name = 'SourceSelection']");
                    if (sourceSelection != null && Cache.ListIdMapping.ContainsKey(new Guid(sourceSelection.InnerText)))
                    {
                        sourceSelection.InnerText = Cache.ListIdMapping[new Guid(sourceSelection.InnerText)].ToString();
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
    }

    public class CategoriesWebPartUpdater : AveBaseViewWebPartUpdater
    {
        public CategoriesWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web) { }
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            return UpdateListIDAndName(webpartInfo, definationXmlDoc);
        }
        private bool UpdateListIDAndName(AveWebPartBaseInfo webPartInfo, XmlDocument definationXmlDoc)
        {
            XmlNamespaceManager webpartXmlNS = new XmlNamespaceManager(definationXmlDoc.NameTable);
            //目前将namespace写死，如果遇到其他的namespace再做处理
            webpartXmlNS.AddNamespace("wp", "http://schemas.microsoft.com/WebPart/v3");
            XmlNode listIdNode = definationXmlDoc.SelectSingleNode("//wp:property[@name='ListId']", webpartXmlNS);
            XmlNode listNameNode = definationXmlDoc.SelectSingleNode("//wp:property[@name='ListName']", webpartXmlNS);
            if (listIdNode != null && AveTypeHelper.IsGuid(listIdNode.InnerText))
            {
                Guid listId = new Guid(listIdNode.InnerText);
                if (this.Cache.ListIdMapping.ContainsKey(listId))
                {
                    if (listNameNode != null)
                    {
                        listNameNode.InnerText = "{" + this.Cache.ListIdMapping[listId].ToString() + "}";
                    }
                    if (listIdNode != null)
                    {
                        listIdNode.InnerText = this.Cache.ListIdMapping[listId].ToString();
                    }
                }
                else
                {
                    if (List != null)
                    {
                        listId = List.ID;
                        if (listNameNode != null)
                        {
                            listNameNode.InnerText = "{" + listId.ToString() + "}";
                        }
                        if (listIdNode != null)
                        {
                            listIdNode.InnerText = listId.ToString();
                        }
                    }
                }
            }
            return false;
        }
    }
    #endregion

    public class AveClientWebPartUrlHandlerFactory
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveClientWebPartUrlHandlerFactory));
        public static AveWebPartPropertyUpdater GenerateWebPartUrlHanlder(string definitionXml, IAveWeb web, XmlNode webPartNode, AveWebPartCache webPartCache)
        {
            return CreateWebpartUpdateInstance(definitionXml, web, webPartCache, GreateLinkUpdateInstance(webPartNode));
        }

        private static AveWebPartPropertyUpdater CreateWebpartUpdateInstance(string definitionXml, IAveWeb web, AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater)
        {
            IWebPartPropertyExtractor webpartPropertyExtractor = WebPartExtractorFactory.Create(definitionXml);
            logger.Info($"CreateWebpartUpdateInstance from type full name:{webpartPropertyExtractor.TypeFullName}");

            TypeInfo currentTypeInfo = TypeInfo.Parse(webpartPropertyExtractor.TypeFullName);
            AveWebPartType webPartType = AveWebPartTypeMapping.GetWebPartType(currentTypeInfo);

            switch (webPartType)
            {
                case AveWebPartType.ContactDetailWebPart:
                    return new ContactDetailWebPart(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.ContentByQueryWebPart:
                    return new ContentByQueryWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.ContentEditorWebPart:
                    return new ContentEditorWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.ExcelWebRendererWebPart:
                    return new ExcelWebRendererWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.TableOfContentsWebPart:
                    return new TableOfContentsWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.VisioWebAccessWebPart:
                    return new VisioWebAccessWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.ListViewWebPart:
                    return new ListViewWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.XsltListViewWebPart:
                    return new XsltListViewWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.ListFormWebPart:
                    return new ListFormWebpartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.SummaryLinkWebPart:
                    return new SummaryLinkWebpartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.SocialCommentWebPart:
                    return new SocialCommentWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.TagCloudWebPart:
                    return new TagCloudWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.XMLWebPart:
                    return new XMLWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.RssWebPart:
                    return new RssWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.SlideshowWebPart:
                    return new SlideShowWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.TimelineWebPart:
                    return new TimeLineWebpartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.BrowserFormWebPart:
                    return new BroswerFormWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.SiteInCategory:
                    return new SiteInCategoryWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.BusinessDataList:
                    return new BusinessDataListWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.BusinessDataDetails:
                    return new BusinessDataDetailsWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.BlogLinksWebPart:
                    return new BlogLinksWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.ScriptEditorWebPart:
                    return new ScriptEditorWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.CategoriesWebPart:
                    return new CategoriesWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.DataFormWebPart:
                    return new DataFormWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.ProjectFieldPart:
                    return new ProjectFieldPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.ContentBySearchWebPart:
                    return new ContentBySearchWebPartUpdater(webPartCache, webPartLinkUpdater, web);
                case AveWebPartType.DefaultWebpartType:
                default:
                    return new DefaultWebPartUrlUpdater(webPartCache, webPartLinkUpdater, web);
            }
        }

        private static AveWebPartLinkUpdater GreateLinkUpdateInstance(XmlNode webPartNode)
        {
            if (string.IsNullOrEmpty(webPartNode.NamespaceURI))
            {
                webPartNode = webPartNode.FirstChild;
            }
            if (webPartNode.NamespaceURI.Equals(V2WebPartPropertyExtractor.WebPartV2NameSpace, StringComparison.OrdinalIgnoreCase))
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
                if (node.Name.Equals("ID", StringComparison.OrdinalIgnoreCase) && node.InnerText.StartsWith("g_"))
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
                        if (node.Attributes.Count > 0 && (node.Attributes[0].Value.EndsWith("Link", StringComparison.OrdinalIgnoreCase) ||
                            node.Attributes[0].Value.EndsWith("Uri", StringComparison.OrdinalIgnoreCase) || node.Attributes[0].Value.EndsWith("URL", StringComparison.OrdinalIgnoreCase) ||
                            node.Attributes[0].Value.Equals("MediaSource", StringComparison.OrdinalIgnoreCase)
                            || node.Attributes[0].Value.Equals("PreviewImageSource", StringComparison.OrdinalIgnoreCase)
                            || node.Attributes[0].Value.EndsWith("Address", StringComparison.OrdinalIgnoreCase)))
                        {
                            if (node.InnerText.StartsWith("~sitecollection", StringComparison.OrdinalIgnoreCase))
                            {
                                // check whether SRC and DES is root site collection
                                bool isSrcRSC = cache.SourceSiteInfo.ServerRelativeUrl.Equals("/", StringComparison.OrdinalIgnoreCase);
                                bool isDesRSC = cache.DestSiteInfo.ServerRelativeUrl.Equals("/", StringComparison.OrdinalIgnoreCase);
                                string targetString = cache.SourceSiteInfo.ServerRelativeUrl;
                                if (isSrcRSC)
                                {
                                    targetString = "";
                                }
                                //string url = node.InnerText.Replace("~sitecollection", targetString);
                                string url = Regex.Replace(node.InnerText, "~sitecollection", targetString, RegexOptions.IgnoreCase);
                                List<string> links = new List<string>();
                                foreach (string link in node.InnerText.Split('|'))
                                {
                                    string replaceUrl = AveReplaceProcessor.UrlReplace(link, cache.SiteManagedMappings, new ReplaceOption(true, true), cache.SourceSiteInfo, cache.DestSiteInfo.ServerRelativeUrl);
                                    links.Add(isDesRSC ? string.Concat("~sitecollection", replaceUrl) : replaceUrl.Replace(cache.DestSiteInfo.ServerRelativeUrl, "~sitecollection"));
                                }
                                node.InnerText = string.Join("|", links);
                            }
                            else if (node.InnerText.ToString().Contains('/'))
                            {
                                node.InnerText = AveReplaceProcessor.UrlReplace(node.InnerText.ToString(), cache.SiteManagedMappings, new ReplaceOption(true, true), cache.SourceSiteInfo, cache.DestSiteInfo.ServerRelativeUrl);
                            }
                        }
                        //if (string.Equals(node.Attributes[0].Value, "DataProviderJSON", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    bool needPost = false;
                        //    node.InnerText = AveContentBySearchWebPartUtility.UpdateDataProviderJsonProperty(node.InnerText, cache,out needPost);
                        //}
                    }
                }
            }
        }
    }
    #endregion
}