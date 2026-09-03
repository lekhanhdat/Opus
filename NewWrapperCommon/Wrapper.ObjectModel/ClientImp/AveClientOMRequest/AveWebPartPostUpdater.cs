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
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using AvePoint.GCommon;
using System.Xml;
using System.Collections.Specialized;
using AveClientRequest.Common;
using AvePoint.ObjectModel.WebService;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AvePoint.ObjectModel.ClientOM
{
    internal class AveWebPartPostUpdater
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof (AveWebPartPostUpdater));
        protected Guid WebPartId { get; set; }
        protected AveWebPartBaseInfo WebPartInfo { get; set; }
        protected IAveWeb ParentWeb { get; set; }
        protected string FileServerRelativeUrl { get; set; }

        protected object Credentials { get; set; }

        public static AveWebPartPostUpdater CreateInstance(Guid webPartId, AveWebPartBaseInfo webPartInfo, IAveWeb parentWeb, string fileUrl, object credentials)
        {
            switch (AveClientWebPartUrlHandlerFactory.GetWebPartTypeId(webPartInfo.WebPartTypeId))
            {
                case AveWebPartType.BrowserFormWebPart:
                    return new BrowserFormWebPartPostUpdater(webPartId,webPartInfo, parentWeb, fileUrl, credentials);
                default:
                    return new AveWebPartPostUpdater(webPartId,webPartInfo, parentWeb, fileUrl, credentials);
            }
        }

        public AveWebPartPostUpdater(Guid webPartId, AveWebPartBaseInfo webPartInfo, IAveWeb parentWeb, string fileUrl, object credentials)
        {
            WebPartId = webPartId;
            WebPartInfo = webPartInfo;
            ParentWeb = parentWeb;
            FileServerRelativeUrl = fileUrl;
            Credentials = credentials;
        }

        public virtual void PostUpdate()
        {
        }
    }

    internal class BrowserFormWebPartPostUpdater : AveWebPartPostUpdater
    {
        public BrowserFormWebPartPostUpdater(Guid webPartId,AveWebPartBaseInfo webPartInfo, IAveWeb parentWeb, string fileUrl,object credentials) 
            : base(webPartId,webPartInfo, parentWeb, fileUrl, credentials)
        {
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "_vti_aut is a part of post url")]
        public override void PostUpdate()
        {
            logger.Debug("Begin Process BrowserFormWebPartPostUpdater.WebUrl:{0},FileUrl:{1}", ParentWeb.Url, FileServerRelativeUrl);
            try
            {
                //todo:wbhu, move it to web service request later, add a web service request obj to AveWebPartRestore
                string webUrl = ParentWeb.Url;
                string webServerRelativeUrl = ParentWeb.ServerRelativeUrl;
                string fileServerRelativeUrl = FileServerRelativeUrl;
                IWebPartPropertyExtractor wpExtractor = WebPartExtractorFactory.Create(WebPartInfo.DefinitionXml);
                string contentTypeId = wpExtractor.GetProperty("ContentTypeId");
                string formLocation = wpExtractor.GetProperty("FormLocation");
                if (contentTypeId != null && formLocation != null)
                {
                    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(null, webUrl, Credentials))
                    {
                        Guid newId = WebPartId;
                        string fileRelativeUrl = AveUrlUtility.GetRelativeUrl(webServerRelativeUrl, fileServerRelativeUrl);
                        string url = webUrl.TrimEnd('/') + "/_vti_bin/_vti_aut/author.dll";
                        MetaInfoHandler metaInfoHandler = new MetaInfoHandler();
                        metaInfoHandler.Add(new MetaInfoProperty("vti_modifiedby", "SHAREPOINT\\system"));
                        MetaInfoProperty modifiedTimeProperty = new MetaInfoProperty("vti_timelastmodified", DateTime.UtcNow.ToString("dd MMM yyyy HH:mm:ss") + " -0000") {Type = MetaInfoValueType.Time};
                        metaInfoHandler.Add(modifiedTimeProperty);
                        string headInfo = "method=put+document&service%5fname=" + Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlKeyValueEncode(webServerRelativeUrl)
                                          + "&document=" + Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlKeyValueEncode("[document_name=" + fileRelativeUrl
                                                                                                                               + ";meta_info=[" + metaInfoHandler.ToUpdateString().TrimEnd(';') + "]]")
                                          + "&put%5foption=edit%2cdiscardstreamchanges&comment=&keep%5fchecked%5fout=true";
                        mNetWork.InitialNetWorker(AveWebServiceType.WebPartPages, webUrl);
                        string sWppHtml = mNetWork.WebPagePagesGetOrignalHtmlOnPage(fileRelativeUrl);
                        sWppHtml = sWppHtml.Substring(sWppHtml.IndexOf("</html>", StringComparison.Ordinal) + "</html>".Length).TrimStart();
                        sWppHtml = AddMetaProgId(sWppHtml);
                        HtmlDocument htmlDoc = new HtmlDocument {OptionOutputOriginalCase = true};
                        htmlDoc.LoadHtml(sWppHtml);
                        HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//node()[@runat='server']");
                        foreach (HtmlNode subNode in nodes)
                        {
                            subNode.Attributes.Remove("__Preview");
                            subNode.Attributes.Remove("__designer:Preview");
                            subNode.Attributes.Remove("__designer:Values");
                            subNode.Attributes.Remove("__designer:templates");
                        }
                        //remove <!--[if gte mso 9] node
                        HtmlNodeCollection commentNodes = htmlDoc.DocumentNode.SelectNodes("//comment()");
                        if (commentNodes != null)
                        {
                            foreach (HtmlNode commentNode in commentNodes)
                            {
                                if (!string.IsNullOrEmpty(commentNode.InnerHtml) && commentNode.InnerHtml.StartsWith("<!--[if gte mso 9]",StringComparison.OrdinalIgnoreCase))
                                {
                                    commentNode.Remove();
                                }
                            }
                        }
                        HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//node()[@__webpartid='{" + newId.ToString().ToUpper(CultureInfo.InvariantCulture) + "}']");
                        if (node == null)
                        {
                            logger.Info("__webpartid Node not found in html node while post update form location and ContentTypeId.WebUrl:{0},FileUrl:{1}", ParentWeb.Url, FileServerRelativeUrl);
                            return;
                        }
                        node.SetAttributeValue("FormLocation", formLocation);
                        node.SetAttributeValue("ContentTypeId", contentTypeId);
                        byte[] body = Encoding.UTF8.GetBytes(headInfo + "\n" + htmlDoc.DocumentNode.OuterHtml.TrimEnd() + "\r\n");
                        Dictionary<string, object> headerInformation = new Dictionary<string, object>
                        {
                            {"X-Vermeer-Content-Type", "application/x-vermeer-urlencoded"}
                        };
                        string result = AveHttpWebRequestUtility.HttpReturn(url, Credentials, "application/x-vermeer-urlencoded", body, headerInformation, "MSFrontPage/15.0");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while post update browserFormWebPart properties.WebUrl:{0},FileUrl:{1},Error:{2}", ParentWeb.Url, FileServerRelativeUrl, e);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "webpartpageexpansion is a key word")]
        private string AddMetaProgId(string aspxContent)
        {
            if (aspxContent.StartsWith("<HasByteOrderMark/>",StringComparison.OrdinalIgnoreCase))
            {
                aspxContent = aspxContent.Substring("<HasByteOrderMark/>".Length);
            }
            //doesn't need to add meta progid if the page is inherit from TemplateRedirectionPage
            if (aspxContent.IndexOf("Inherits=\"Microsoft.SharePoint.Publishing.TemplateRedirectionPage", StringComparison.Ordinal) >= 0)
            {
                return aspxContent;
            }
            const string pageDirective = "<%@ Page";
            int startIndexOfPageDirective = aspxContent.IndexOf(pageDirective, StringComparison.Ordinal);
            int endIndexOfPageDirective = aspxContent.IndexOf("%>", startIndexOfPageDirective, StringComparison.Ordinal);
            string pageDirectiveContent = aspxContent.Substring(startIndexOfPageDirective, endIndexOfPageDirective - startIndexOfPageDirective);
            if (!pageDirectiveContent.Contains("meta:progid=\"SharePoint.WebPartPage.Document\""))
            {
                string pageDirectiveContentWithMetaInfo = pageDirectiveContent + " meta:webpartpageexpansion=\"full\" meta:progid=\"SharePoint.WebPartPage.Document\" ";
                return aspxContent.Replace(pageDirectiveContent, pageDirectiveContentWithMetaInfo);
            }
            if (!pageDirectiveContent.Contains("meta:webpartpageexpansion=\"full\""))
            {
                string pageDirectiveContentWithMetaInfo = pageDirectiveContent + " meta:webpartpageexpansion=\"full\" ";
                return aspxContent.Replace(pageDirectiveContent, pageDirectiveContentWithMetaInfo);
            }
            return aspxContent;
        }


    }
}
