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
    using System.Linq;
    using System.Text;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;
    using AvePoint.Wrapper.Common;
    using GCommon;

    class ClientSideWebPartEventsWorker : ClientSideWebPartCommonWorker, IClientSideWebPartWorker
    {
        public const string Id = "20745d7d-8581-4a6c-bf26-68279bc123fc";

        protected override AveLogger logger { get { return AveLogger.GetInstance(typeof(ClientSideWebPartEventsWorker)); } }

        public override bool Process(AveClientSideWebPart webPart, AveSPDoc document, bool lastPost)
        {
            if (webPart == null || document == null)
            {
                throw new ArgumentNullException($"webPart is null:{webPart == null}, document is null:{document == null}");
            }
            logger.Info($"Process event webpart, properties:{webPart.PropertiesJson}");
            var requirePostAction = false;

            var listIdToken = webPart.Properties["selectedListId"];

            var builder = new StringBuilder();

            if (listIdToken != null)
            {
                Guid listId = Guid.Empty;
                try
                {
                    listId = (Guid)listIdToken;
                }
                catch (Exception ex)
                {
                    //if selectedListId is "", cannot convert it to guid
                    logger.Warn($"Cannot get the listId from selectedListId:{listIdToken} in webPart:{webPart.PropertiesJson}, ex:{ex}");
                }

                if (listId != Guid.Empty)
                {
                    IAveList list = null;

                    var targetListId = document.ParentSite.MappingManager.SiteMappingManager.GetListIdMapping(listId);

                    if (targetListId == Guid.Empty)
                    {
                        list = document.ParentFolder.ParentList.ParentWeb.SPWeb.Lists.GetById(listId);
                    }
                    else
                    {
                        list = document.ParentFolder.ParentList.ParentWeb.SPWeb.GetList(targetListId);
                    }

                    if (list == null && lastPost)
                    {
                        list = document.ParentFolder.ParentList.ParentWeb.SPWeb.Lists.FirstOrDefault(a => a.BaseTemplate == AveListTemplateType.Events);

                        if (list != null)
                        {
                            logger.Warn($"find the first events list for this webpart:{list.Title}, the source list id:{listId}");
                        }
                    }

                    if (list != null)
                    {
                        if (list.ID != listId)
                        {
                            webPart.Properties["selectedListId"] = list.ID.ToString("D");
                            webPart.Properties["webId"] = list.ParentWeb.ID.ToString("D");
                            webPart.Properties["siteId"] = list.ParentWeb.Site.ID.ToString("D");

                            string title = null;

                            var parserOptions = new HtmlParserOptions() { IsEmbedded = true };
                            using (IHtmlDocument htmlDocument = new HtmlParser(parserOptions).ParseDocument(webPart.HtmlPropertiesData))
                            {
                                foreach (var item in htmlDocument.All)
                                {
                                    var name = item.GetAttribute("data-sp-prop-name");
                                    if (name != null)
                                    {
                                        if (name.Equals("title", StringComparison.OrdinalIgnoreCase))
                                        {
                                            title = item.InnerHtml;
                                            break;
                                        }
                                    }
                                }
                            }

                            var jObject = new Newtonsoft.Json.Linq.JObject();
                            jObject["properties"] = webPart.Properties;
                            var processedContent = new Newtonsoft.Json.Linq.JObject();
                            jObject["serverProcessedContent"] = processedContent;
                            processedContent["searchablePlainTexts"] =
                                Newtonsoft.Json.Linq.JObject.FromObject(new Dictionary<string, string>() { { "title", title } });
                            processedContent["links"] =
                                Newtonsoft.Json.Linq.JObject.FromObject(new Dictionary<string, string>() { { "baseUrl", list.ParentWeb.ServerRelativeUrl } });
                            webPart.PropertiesJson = jObject.ToString(0, new Newtonsoft.Json.JsonConverter[0]);
                        }
                    }
                    else
                    {
                        builder.AppendFormat("Cannot get the list information with {0}\r\n", webPart.PropertiesJson);
                        requirePostAction = true;
                    }
                }
            }

            if (builder.Length > 0)
            {
                logger.Warn("Process {0} with information:{1}", typeof(ClientSideWebPartEventsWorker).Name, builder.ToString());
            }

            return !requirePostAction;
        }
    }
}
