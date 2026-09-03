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
namespace AvePoint.Wrapper.Restore.Core
{
    using System;
    using System.Collections.Generic;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;
    using AvePoint.Wrapper.Common;
    using GCommon;

    class ClientSideWebPartFileViewerWorker : ClientSideWebPartCommonWorker, IClientSideWebPartWorker
    {
        public const string Id = "b7dd04e1-19ce-4b24-9132-b60a1c2b910d";

        protected override AveLogger logger { get { return AveLogger.GetInstance(typeof(ClientSideWebPartFileViewerWorker)); } }

        public override bool Process(AveClientSideWebPart webPart, AveSPDoc document, bool lastPost)
        {
            var requirePostAction = false;
            var fileUrlToken = webPart.Properties["file"];

            if (fileUrlToken != null)
            {
                var fileUrl = (string)fileUrlToken;

                if (fileUrl != null && fileUrl.Length > 0)
                {
                    if (fileUrl[0] == '/')
                    {
                        requirePostAction = ProcessInternal(webPart, fileUrl, document);
                    }
                    else
                    {
                        requirePostAction = ProcessAcrossSite(webPart, fileUrl, document);
                    }
                }
            }

            return !requirePostAction;
        }

        private bool ProcessInternal(AveClientSideWebPart webPart, string fileUrl, AveSPDoc document)
        {
            var requirePostAction = false;

            var uniqueIdToken = webPart.Properties["uniqueId"];

            IAveFile file = null;
            var uniqueId = Guid.Empty;

            if (uniqueIdToken != null)
            {
                uniqueId = (Guid)uniqueIdToken;
                var targetId = document.ParentSite.MappingManager.SiteMappingManager.GetDocumentUniqueIdMapping(uniqueId);
                logger.Warn($"sourceId:{uniqueId}, targetId:{targetId}");
                if (targetId != Guid.Empty)
                {
                    file = document.ParentFolder.ParentList.ParentWeb.SPWeb.GetFile(targetId);
                }
                else
                {
                    file = document.ParentFolder.ParentList.ParentWeb.SPWeb.GetFile(uniqueId);
                }
            }

            if (file == null || file.Exists == false)
            {
                var targetUrl = ReplaceUrl(fileUrl, document.ParentSite);

                file = document.ParentFolder.ParentList.ParentWeb.SPWeb.GetFile(targetUrl);

                if (file == null || !file.Exists)
                {
                    file = null;
                    requirePostAction = true;
                    return requirePostAction;
                }
            }
            Replace(webPart, file, uniqueId);

            return requirePostAction;
        }

        private void Replace(AveClientSideWebPart webPart, IAveFile file, Guid uniqueId)
        {
            if (file.UniqueId != uniqueId)
            {
                webPart.Properties["file"] = file.ServerRelativeUrl;
                webPart.Properties["listId"] = file.ParentFolder.ParentListId.ToString("D");
                webPart.Properties["siteId"] = file.Web.Site.ID.ToString("D");
                webPart.Properties["uniqueId"] = file.UniqueId.ToString("D");
                webPart.Properties["webId"] = file.Web.ID.ToString("D");
                var jObject = new Newtonsoft.Json.Linq.JObject();
                jObject["properties"] = webPart.Properties;
                var processedContent = new Newtonsoft.Json.Linq.JObject();
                jObject["serverProcessedContent"] = processedContent;
                processedContent["searchablePlainTexts"] =
                    Newtonsoft.Json.Linq.JObject.FromObject(new Dictionary<string, string>() { { "title", file.Title } });
                processedContent["links"] =
                    Newtonsoft.Json.Linq.JObject.FromObject(new Dictionary<string, string>() {
                        { "serverRelativeUrl",
                            file.ServerRelativeUrl },
                        { "wopiurl",
                            file.Web.ServerRelativeUrl.TrimEnd('/') + string.Format("/_layouts/15/WopiFrame.aspx?sourcedoc={0}&action=interactivepreview", file.UniqueId.ToString("B"))} }
                    );
                webPart.PropertiesJson = jObject.ToString(0, new Newtonsoft.Json.JsonConverter[0]);
            }
        }

        private bool ProcessAcrossSite(AveClientSideWebPart webPart, string fileUrl, AveSPDoc document)
        {
            var requirePostAction = false;

            var uniqueIdToken = webPart.Properties["uniqueId"];

            IAveFile file = null;
            var uniqueId = Guid.Empty;

            if (uniqueIdToken != null)
            {
                uniqueId = (Guid)uniqueIdToken;
                var targetId = document.ParentSite.MappingManager.SiteMappingManager.GetDocumentUniqueIdMapping(uniqueId);

                if (targetId != Guid.Empty)
                {
                    var webId = (Guid)webPart.Properties["webId"];
                    Guid targetWebId;

                    if (document.ParentSite.MappingManager.SiteMappingManager.WebIDMapping.TryGetValue(webId, out targetWebId))
                    {
                        logger.Info($"find the target web id:{targetWebId} and file id:{targetId}");
                        file = document.ParentSite.SPSite.OpenWeb(targetWebId).GetFile(targetId);
                    }
                    else
                    {
                        logger.Info($"find the target file id:{targetId}");
                        file = document.ParentFolder.ParentList.ParentWeb.SPWeb.GetFile(targetId);
                    }
                }
            }

            if ((file == null || file.Exists == false) && !string.IsNullOrEmpty(webPart.HtmlPropertiesData))
            {
                string serverRelativeUrl = null;
                string wopiurl = null;

                var parserOptions = new HtmlParserOptions() { IsEmbedded = true };
                using (IHtmlDocument htmlDocument = new HtmlParser(parserOptions).ParseDocument(webPart.HtmlPropertiesData))
                {
                    foreach (var item in htmlDocument.All)
                    {
                        var name = item.GetAttribute("data-sp-prop-name");
                        if (name != null)
                        {
                            if (name.Equals("serverRelativeUrl", StringComparison.OrdinalIgnoreCase))
                            {
                                serverRelativeUrl = System.Web.HttpUtility.UrlDecode(item.GetAttribute("href"));
                            }
                            else if (name.Equals("wopiurl", StringComparison.OrdinalIgnoreCase))
                            {
                                wopiurl = System.Web.HttpUtility.UrlDecode(item.GetAttribute("href"));
                            }
                        }
                    }
                }

                if (wopiurl != null && serverRelativeUrl != null && fileUrl.EndsWith(serverRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    string sourceHost = fileUrl.Substring(0, fileUrl.Length - serverRelativeUrl.Length);
                    string sourceWebServerRelativeUrl = null;
                    string targetHost = null;
                    string targetWebServerRelativeUrl = null;
                    string targetFileServerRelativeUrl = null;

                    targetFileServerRelativeUrl = ReplaceUrl(serverRelativeUrl, document.ParentSite);

                    if (wopiurl[0] == '/')
                    {
                        sourceWebServerRelativeUrl = wopiurl.Substring(0, wopiurl.IndexOf("/_layouts/"));
                        targetHost = new Uri(document.ParentSite.SPSite.Url).GetLeftPart(UriPartial.Authority);
                        targetWebServerRelativeUrl = ReplaceUrl(sourceWebServerRelativeUrl, document.ParentSite);
                    }
                    else if (wopiurl.StartsWith(sourceHost, StringComparison.OrdinalIgnoreCase))
                    {
                        sourceWebServerRelativeUrl = wopiurl.Substring(sourceHost.Length, wopiurl.IndexOf("/_layouts/") - sourceHost.Length);
                        targetHost = new Uri(document.ParentSite.SPSite.Url).GetLeftPart(UriPartial.Authority);
                        targetWebServerRelativeUrl = ReplaceUrl(sourceWebServerRelativeUrl, document.ParentSite);

                        if (sourceHost.IndexOf("-my.") > 0)
                        {
                            var index = targetHost.IndexOf('.');

                            if (index > 0)
                            {
                                targetHost = targetHost.Substring(0, index) + "-my" + targetHost.Substring(index);
                            }
                        }
                        else if (targetHost.IndexOf("-my.") > 0)
                        {
                            targetHost = targetHost.Replace("-my.", ".");
                        }
                    }

                    var webUrl = targetHost + targetWebServerRelativeUrl;

                    logger.Info($"try to find the file with url:{webUrl}, serverRelativeUrl:{targetWebServerRelativeUrl}, fileUrl:{targetFileServerRelativeUrl}, sourceFileUrl:{fileUrl}, sourceServerRelativeUrl:{serverRelativeUrl}, sourceWOPI:{wopiurl}");

                    var objectModelFactory = AveObjectModelFactory.CreateObjectModelFactory(webUrl, document.ParentSite.ObjectModelFactory.AccountInfo);

                    using (IAveSite site = objectModelFactory.CreateSite())
                    {
                        using (var web = site.OpenWeb(targetWebServerRelativeUrl))
                        {
                            file = web.GetFile(targetFileServerRelativeUrl);
                        }
                    }
                }
                else
                {
                    logger.Warn("Mismatch error, the file url:{0}, the server relative url:{1}", fileUrl, serverRelativeUrl);
                }
            }


            if (file == null || file.Exists == false)
            {
                requirePostAction = true;
            }
            else
            {
                Replace(webPart, file, uniqueId);
            }


            return requirePostAction;
        }
    }
}
