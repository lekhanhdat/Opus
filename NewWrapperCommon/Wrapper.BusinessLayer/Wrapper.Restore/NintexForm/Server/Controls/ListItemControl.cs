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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;

namespace AvePoint.Wrapper.Restore.NintexForm.Server
{
    class ListItemControl : BaseControl//Responsive Mode do not have this control
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public ListItemControl(IAveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager, string prefix)
            : base(web, list, contentTypeId, controlNode, nsManager, prefix)
        {
        }
        public override void ProcessControl(bool isPost)
        {
            var siteMappingManager = mWeb.ParentSite.MappingManager.SiteMappingManager;
            IAveList destinationList;
            IAveWeb web = null;
            string webUrl;
            bool needDispose = false;
            if (!NeedContinue(out webUrl))
            {
                return;
            }
            try
            {
                if (string.IsNullOrEmpty(webUrl) ||
                    webUrl.Trim(new char[] { '/' }).Equals(mWeb.SPWeb.ServerRelativeUrl.Trim(new char[] { '/' }), StringComparison.OrdinalIgnoreCase))
                {
                    web = mWeb.SPWeb;
                }
                else
                {
                    web = mWeb.ParentSite.SPSite.OpenWeb(webUrl);
                    needDispose = true;
                }
                if (!web.Exists)
                {
                    throw new AveNintexFormPostException("Web", webUrl, contentTypeId);
                }
                #region Handle List
                var listNode = GetPropertyNode("d3p1:ListId");//只能是List Title
                var listTitle = listNode == null ? string.Empty : listNode.InnerText;
                if (string.IsNullOrEmpty(listTitle))
                {
                    return;
                }
                string destinationListTitle;
                if (!siteMappingManager.GetValueFromListTitleMappnig(web.ID, listTitle, out destinationListTitle))
                {
                    if (!isPost)
                    {
                        //可能List还没有被还原。
                        throw new AveNintexFormPostException("List", listTitle, contentTypeId);
                    }
                    destinationListTitle = listTitle;
                }
                listNode.InnerText = destinationListTitle;
                #endregion
                #region Handle Item
                var itemNode = GetPropertyNode("d3p1:ListItemId");
                var listItemIdString = itemNode == null ? string.Empty : itemNode.InnerText;
                var listItemId = 0;
                int.TryParse(listItemIdString, out listItemId);
                if (listItemId <= 0)
                {
                    return;
                }
                destinationList = web.GetListByName(destinationListTitle, false);
                if (destinationList == null)
                {
                    throw new AveNintexFormPostException("List", listTitle, contentTypeId);
                }
                listNode.InnerText = destinationList.Title;
                var destinationListItemId = siteMappingManager.GetMappingItemId(destinationList.ID, listItemId);
                if (destinationListItemId == -1)
                {
                    if (!isPost)
                    {
                        throw new AveNintexFormPostException("Item", listItemId.ToString(), contentTypeId);
                    }
                    log.Warn("This item did restored in this job. Use source Item id to publish nintex form. Web: {0}, List: {1}, Item: {2}", webUrl, listTitle, listItemId);
                }
                else
                {
                    itemNode.InnerText = destinationListItemId.ToString();
                }
                #endregion
            }
            catch (AveNintexFormPostException)
            {
                throw;
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while process list item form. Error: {0}", e);
                throw new AveNintexFormPostException("Web", webUrl, contentTypeId);
            }
            finally
            {
                if (needDispose)
                {
                    web.Dispose();
                    web = null;
                }
            }
        }
        /// <summary>
        /// 指向当前site collection走替换逻辑。否则认为是外部URL，不做替换。
        /// </summary>
        /// <param name="destinationWebUrl"></param>
        /// <returns></returns>
        private bool NeedContinue(out string destinationWebUrl)
        {
            destinationWebUrl = string.Empty;
            var webUrlNode = GetPropertyNode("d3p1:WebUrl");
            destinationWebUrl = ReplaceUrl(webUrlNode);

            var dummyWebNode = GetPropertyNode("d3p1:DummyWeb");
            var dummyDestinationWebUrl = ReplaceUrl(dummyWebNode);

            if (IsInternalUrl(destinationWebUrl))
            {
                return true;
            }
            if (IsInternalUrl(dummyDestinationWebUrl))
            {
                destinationWebUrl = dummyDestinationWebUrl;
                return true;
            }
            return false;
        }

        private bool IsInternalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            var siteMappingManager = mWeb.ParentSite.MappingManager.SiteMappingManager;
            return url.StartsWith(siteMappingManager.DestSiteInfo.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase)
                || url.StartsWith(siteMappingManager.DestSiteInfo.Url, StringComparison.OrdinalIgnoreCase);
        }
        private string ReplaceUrl(XmlNode urlNode)
        {
            if (urlNode == null)
            {
                return string.Empty;
            }
            var url = urlNode.InnerText;
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }
            var siteMappingManager = mWeb.ParentSite.MappingManager.SiteMappingManager;
            urlNode.InnerText = AveReplaceProcessor.UrlReplace(url, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
            return urlNode.InnerText;
        }

        public override void AddControlNameSpace()
        {
            nsManager.AddNamespace("d3p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.SharePoint.FormControls");
        }
    }
}
