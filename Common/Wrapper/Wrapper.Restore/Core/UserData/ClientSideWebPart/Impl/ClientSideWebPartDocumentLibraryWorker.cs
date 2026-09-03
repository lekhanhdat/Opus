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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Restore.Core
{
    class ClientSideWebPartDocumentLibraryWorker : ClientSideWebPartCommonWorker, IClientSideWebPartWorker
    {
        public const string Id = "f92bf067-bc19-489e-a556-7fe95f508720";

        protected override AveLogger logger { get { return AveLogger.GetInstance(typeof(ClientSideWebPartDocumentLibraryWorker)); } }

        protected override List<ClientSideWebpartProperty> PotentialPropertiesAndTypes
        {
            get
            {
                return new List<ClientSideWebpartProperty>()
                {
                    new ClientSideWebpartProperty("selectedListId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.List),
                    new ClientSideWebpartProperty("selectedViewId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.View),
                    new ClientSideWebpartProperty("selectedListUrl", ClientSideWebpartPropertyTypes.Url),
                };
            }
        }
        public ClientSideWebPartDocumentLibraryWorker()
            : base()
        {
            base.HandlePropertiesJsonExtended += HandleExtraProperties;
        }

        private void HandleExtraProperties(AveClientSideWebPart webPart, AveSPDoc document, bool lastPost, ref bool needPost)
        {
            if (needPost && !lastPost)
            {
                logger.Warn($"Extension method is not executed since the Webpart needs post action");
                return;
            }

            JObject properties = JObject.Parse(webPart.PropertiesJson);

            if (properties.ContainsKey("webRelativeListUrl"))
            {
                string webRelativeListUrl = properties.Property("webRelativeListUrl").Value.ToString();
                if (!string.IsNullOrEmpty(webRelativeListUrl))
                {
                    string listUrl = properties.ContainsKey("selectedListUrl") ? properties.Property("selectedListUrl").Value.ToString() : null;
                    if (!string.IsNullOrEmpty(listUrl))
                    {
                        webRelativeListUrl = listUrl.Trim(true, false, new string[] { document.Web.ServerRelativeUrl });
                    }
                    properties.Property("webRelativeListUrl").Value = new JValue(webRelativeListUrl);
                }
            }

            if (properties.ContainsKey("selectedFolderPath"))
            {
                string folderPath = properties.Property("selectedFolderPath").Value.ToString();
                if (!string.IsNullOrEmpty(folderPath))
                {
                    //Replace?
                    properties.Property("selectedFolderPath").Value = folderPath;
                }
            }

            if (properties.ContainsKey("selectedFolderKey"))
            {
                string folderKey = properties.Property("selectedFolderKey").Value.ToString();
                if (!string.IsNullOrEmpty(folderKey))
                {
                    UrlQueryString query = new UrlQueryString(folderKey);
                    string[] keys = new string[] { "id", "listurl" };
                    foreach (string key in keys)
                    {
                        if (query.Keys.Contains(key))
                        {
                            string idUrl = query.Queries.Property(key).Value.ToString();
                            if (!ReplaceUrl(ref idUrl, document.ParentSite))
                            {
                                if (!lastPost)
                                {
                                    logger.Warn($"Extension method need post action due to property:{key}");
                                    needPost = true;
                                    return;
                                }
                            }
                            query[key] = idUrl;
                        }
                    }

                    properties.Property("selectedFolderKey").Value = query.ToString().TrimStart(new char[] { '?' });
                }
            }

            webPart.PropertiesJson = properties.ToString();
        }
    }
}
