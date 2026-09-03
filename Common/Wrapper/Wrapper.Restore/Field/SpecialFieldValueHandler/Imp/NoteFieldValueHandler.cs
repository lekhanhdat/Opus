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
namespace AvePoint.Wrapper.Restore
{
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Resource;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;

    public class NoteFieldValueHandler : BaseFieldValueHandler, IFieldValueHandler
    {
        private static IAveLogger mLog = AveLogger.GetInstance(typeof(NoteFieldValueHandler));
        private static Dictionary<string, List<string>> linkResoverMapping;

        static NoteFieldValueHandler()
        {
            linkResoverMapping = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "a",new List<string>(){ "href"} },
                { "area",new List<string>{ "href"} },
                { "img",new List<string>{ "src"} }
            };
        }
        public NoteFieldValueHandler(AveSPSite parentAveSite)
            : base(parentAveSite)
        {

        }

        public static PostActionType GetRichTextPostActionType(string value, IAveList currentList)
        {
            PostActionType postType = PostActionType.None;
            try
            {
                HtmlDocument fieldDoc = new HtmlDocument();
                fieldDoc.OptionOutputOriginalCase = true;
                fieldDoc.LoadHtml("<ReplaceXmlLinks>" + value + "</ReplaceXmlLinks>");
                List<string> links = new List<string>();
                GetAllLinks(links, fieldDoc.DocumentNode);
                foreach (string link in links)
                {
                    if (AttachmentUrlUtility.IsCurrentListAttachmentUrl(link, currentList))
                    {
                        postType = PostActionType.ListPostAction;
                    }
                    if (AttachmentUrlUtility.IsAttachmentUrl(link))
                    {
                        postType = PostActionType.SitePostAction;
                        break;
                    }
                    if (LotusNotesLinkHanddler.IsLotusNotesLink(link))
                    {
                        postType = PostActionType.SitePostAction;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Convert rich text value to html failed.Value:{0},Error:{1}", value, ex);
            }
            return postType;
        }

        private static void GetAllLinks(List<string> links, HtmlNode node)
        {
            foreach (HtmlNode child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Element)
                {
                    List<string> attributeList;
                    if (linkResoverMapping.TryGetValue(child.Name, out attributeList))
                    {
                        attributeList.ForEach(t =>
                        {
                            string attributeValue = child.GetAttributeValue(t, "");
                            if (!string.IsNullOrEmpty(attributeValue))
                            {
                                links.Add(attributeValue);
                            }
                        });
                    }
                    GetAllLinks(links, child);
                }
            }
        }

        public object Process(IAveField field, object value, bool isSiteUrlReplaced)
        {
            var richTextField = field as IAveFieldMultiLineText;
            var fieldValue = value as string;
            if (richTextField.RichText)
            {
                return ReplaceXmlLinks(mParentSite, fieldValue, field, isSiteUrlReplaced);
            }
            return value;
        }

        private static string ReplaceXmlLinks(AveSPSite parentSite, string fieldValue, IAveField field, bool isSiteUrlReplaced)
        {
            try
            {
                HtmlDocument fieldDoc = new HtmlDocument();
                fieldDoc.OptionOutputOriginalCase = true;
                fieldDoc.LoadHtml("<ReplaceXmlLinks>" + fieldValue + "</ReplaceXmlLinks>");
                List<HtmlNode> nodes = new List<HtmlNode>();
                GetLinkNodes(nodes, fieldDoc.DocumentNode);
                foreach (HtmlNode node in nodes)
                {
                    ReplaceXmlLinksInternal(node, field, parentSite, isSiteUrlReplaced);
                }
                return fieldDoc.DocumentNode.FirstChild.InnerHtml;
            }
            catch (Exception ex)
            {
                mLog.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ReplaceXmlLinksError, ex.ToString());
                try
                {
                    fieldValue = ReplaceStringLinks(fieldValue, field, parentSite, isSiteUrlReplaced);
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while replace xml Links. fieldName:{0}\n error message:{1}", field.InternalName, e));
                }
                return fieldValue;
            }
        }

        private static string ReplaceStringLinks(string strValue, IAveField field, AveSPSite parentSite, bool isSiteUrlReplaced)
        {
            int length = strValue.Length;
            int index = 0;
            List<string> links = new List<string>();
            while (index < length)
            {
                if (strValue[index] == '<')
                {
                    if ((index + 2 < length) && strValue.Substring(index, 3) == "<a ")
                    {
                        int end = strValue.IndexOf('>', index);
                        if (end > 0)
                        {
                            int p1 = strValue.IndexOf("href=" + '"', index, StringComparison.OrdinalIgnoreCase);
                            int p2 = -1;
                            if (p1 > 0)
                            {
                                p1 = p1 + 6;
                                p2 = strValue.IndexOf('"', p1);
                            }
                            if ((index < p1) && (p1 < p2) && (p2 < end))
                            {
                                string str = strValue.Substring(p1, p2 - p1);
                                links.Add(strValue.Substring(p1, p2 - p1));
                            }
                        }
                        index = end;
                    }
                    else if ((index + 4 < length) && strValue.Substring(index, 5) == "<img ")
                    {
                        int end = strValue.IndexOf('>', index);
                        if (end > 0)
                        {
                            int p1 = strValue.IndexOf("src=" + '"', index, StringComparison.OrdinalIgnoreCase);
                            int p2 = -1;
                            if (p1 > 0)
                            {
                                p1 = p1 + 5;
                                p2 = strValue.IndexOf('"', p1);
                            }
                            if ((index < p1) && (p1 < p2) && (p2 < end))
                            {
                                string str = strValue.Substring(p1, p2 - p1);
                                links.Add(strValue.Substring(p1, p2 - p1));
                            }
                        }
                        index = end;
                    }
                }
                index++;
            }
            StringBuilder builder = new StringBuilder(strValue);
            foreach (string link in links)
            {
                string newLink = ReplaceUrl(field, parentSite, link, isSiteUrlReplaced);
                builder.Replace(link, newLink);
            }
            return builder.ToString();

        }

        private static void ReplaceXmlLinksInternal(HtmlNode node, IAveField field, AveSPSite parentSite, bool isSiteUrlReplaced)
        {
            HtmlAttribute linkAttribute = null;
            List<string> resolverList;
            if (linkResoverMapping.TryGetValue(node.Name, out resolverList))
             {
                //HTML语言中忽略大小写，所以href、src可能是大写，也可能是小写
                foreach (HtmlAttribute attr in node.Attributes)
                {
                    if (resolverList.Contains(attr.Name,StringComparison.OrdinalIgnoreCase))
                    {
                        linkAttribute = attr;
                        break;
                    }
                }
            }
            if (linkAttribute == null)
            {
                return;
            }
            string hrefLink = HttpUtility.HtmlDecode(linkAttribute.Value);
            string newLinkValue;
            newLinkValue = ReplaceUrl(field, parentSite, hrefLink, isSiteUrlReplaced);
            linkAttribute.Value = HttpUtility.UrlPathEncode(newLinkValue);
            if (node.HasChildNodes)
            {
                foreach (HtmlNode child in node.ChildNodes)
                {
                    HtmlTextNode textNode = child as HtmlTextNode;
                    if (textNode != null && textNode.NodeType == HtmlNodeType.Text)
                    {
                        if (HttpUtility.UrlDecode(textNode.Text).Equals(hrefLink))
                        {
                            textNode.Text = HttpUtility.UrlDecode(linkAttribute.Value);
                        }
                        else if (HttpUtility.UrlDecode(textNode.Text).EndsWith(hrefLink, StringComparison.OrdinalIgnoreCase) &&
                            (HttpUtility.UrlDecode(textNode.Text).StartsWith("http://", StringComparison.OrdinalIgnoreCase) || HttpUtility.UrlDecode(textNode.Text).StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                        {
                            var subRef = HttpUtility.UrlDecode(textNode.Text);
                            string textValue = ReplaceUrl(field, parentSite, subRef, isSiteUrlReplaced);
                            textNode.Text = HttpUtility.UrlPathEncode(textValue);
                        }
                    }
                }
            }
        }

        private static string ReplaceUrl(IAveField field, AveSPSite parentSite, string hrefLink, bool isSiteUrlReplaced)
        {
            string newLinkValue= HttpUtility.HtmlDecode(HttpUtility.UrlDecode(hrefLink));
            bool isAttachmentUrl = AttachmentUrlUtility.IsAttachmentUrl(hrefLink);
            if (isAttachmentUrl)
            {
                isAttachmentUrl = AttachmentUrlUtility.HandleUrlReplacement(hrefLink, field.ParentList, isSiteUrlReplaced, parentSite.MappingManager.SiteMappingManager, out newLinkValue);
            }
            if (!isAttachmentUrl)
            {
                if (LotusNotesLinkHanddler.IsLotusNotesLink(hrefLink))
                {
                    mLog.Info("Handle LotusNotesLink [{0}]", hrefLink);
                    var dic = new Dictionary<string, string>();
                    bool isLotusNotesLinkReplaced = LotusNotesLinkHanddler.HandleLotusNotesLink(hrefLink, field.ParentList, parentSite.MappingManager.SiteMappingManager, out dic, out newLinkValue);
                    if (!isLotusNotesLinkReplaced)
                    {
                        newLinkValue = AveReplaceProcessor.UrlReplace(hrefLink, parentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), parentSite.SourceSiteInfo, parentSite.ServerRelativeUrl, true);
                    }
                }
                else
                {
                    newLinkValue = AveReplaceProcessor.UrlReplace(hrefLink, parentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), parentSite.SourceSiteInfo, parentSite.ServerRelativeUrl, true);
                }
            }


            return newLinkValue;
        }

        private static void GetLinkNodes(List<HtmlNode> nodes, HtmlNode node)
        {
            foreach (HtmlNode child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Element)
                {
                    if (linkResoverMapping.ContainsKey(child.Name))
                    {
                        nodes.Add(child);
                    }
                    //if (child.Name.Equals("a", StringComparison.OrdinalIgnoreCase) || child.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    nodes.Add(child);
                    //}
                    GetLinkNodes(nodes, child);
                }
            }
        }

    }

}
