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
namespace AvePoint.ObjectModel.WebService
{
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using Microsoft365.Authentication;
    using Microsoft365.SharePoint.WebService;
    using System;
    using System.Xml;
    public class AveWebServiceNetWork : IAveWebServiceNetWork
    {
        protected virtual string WebUrl { get; set; }
        protected virtual AveBPOSAccountInfo BposInfo { get; set; } 
        protected virtual ITokenProvider TokenProvider { get; set; }
        protected Func<string> CookieProvider { get; set; }
        protected ISharePointWebService Service { get; set; }
        public AveWebServiceNetWork(AveBPOSAccountInfo user, string webUrl, ITokenProvider tokenProvider)
        {
            WebUrl = webUrl;
            TokenProvider=tokenProvider;
            BposInfo = user;
            CookieProvider = () => { return TokenProvider.GetToken(new Uri(new Uri(WebUrl).GetLeftPart(UriPartial.Authority))); };
        }

        #region initial
        public bool InitialNetWorker(AveWebServiceType type, string netWorkUrl)
        {
            WebUrl = netWorkUrl;
            return true;
        }
        #endregion initial


        public void BrowserEnableUserFormTemplate(string formTemplateUrl)
        {
            using (var service = new FormsService(WebUrl, CookieProvider))
            {
                service.BrowserEnableUserFormTemplate(formTemplateUrl);
            }
        }

        public void CheckInFile(string pageUrl, string comment, int checkinType)
        {
            using (var service = new ListsService(WebUrl, CookieProvider))
            {
                service.CheckInFile(pageUrl, comment, checkinType.ToString());
            }
        }

        public void Dispose()
        {
            WebUrl = null;
            TokenProvider = null;
            BposInfo = null;
            CookieProvider = null;
        }

        public XmlNode GetSite(string siteUrl)
        {
            using (var service = new SiteService(WebUrl, CookieProvider))
            {
                return service.GetSite(siteUrl);
            }
        }

        public XmlNode ListGetList(string listTile)
        {
            using (var service = new ListsService(WebUrl, CookieProvider))
            {
               return service.GetList(listTile);
            }
        }

        public void ListUpdateList(string listGuid, XmlNode listProperties, XmlNode newFields, XmlNode updateFields, XmlNode deleteFields, string listVersion)
        {
            using (var service = new ListsService(WebUrl, CookieProvider))
            {
                 service.UpdateList(listGuid, listProperties, newFields, updateFields, deleteFields, listVersion);
            }
        }

        public void SetFormsForListItem(int lcid, string base64FormTemplate, string applicationId, string listGuid, string contentTypeId)
        {
            using (var service = new FormsService(WebUrl, CookieProvider))
            {
                service.SetFormsForListItem(lcid, base64FormTemplate, applicationId, listGuid, contentTypeId);
            }
        }

        public XmlNode UpdateContentType(string listName, string ctId, XmlNode node)
        {
            using (var service = new ListsService(WebUrl, CookieProvider))
            {
                return service.UpdateContentType(listName, ctId, node, null, null, null, string.Empty);
            }
        }

        public XmlNode UpdateContentTypeXmlDocuments(string listName, string ctId, XmlNode node)
        {
            using (var service = new ListsService(WebUrl, CookieProvider))
            {
                return service.UpdateContentTypeXmlDocument(listName, ctId, node);
            }
        }

        public XmlNode UpdateListItems(string listName, XmlNode updates)
        {
            using (var service = new ListsService(WebUrl, CookieProvider))
            {
                return service.UpdateListItems(listName, updates);
            }
        }

        #region WebPartPagesService
        public string GetWebPartPage(string documentName)
        {
            using (var service = new WebPartPagesService(WebUrl, CookieProvider))
            {
                return service.GetWebPartPage(documentName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentName">full url of aspx page</param>
        /// <returns>v2 and v3 webparts</returns>
        public XmlNode GetWebPartProperties2(string documentName)
        {
            using (var service = new WebPartPagesService(WebUrl, CookieProvider))
            {
                return service.GetWebPartProperties2(documentName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentName">full url of aspx page</param>
        /// <returns>v2 webparts</returns>
        public XmlNode GetWebPartProperties(string documentName)
        {
            using (var service = new WebPartPagesService(WebUrl, CookieProvider))
            {
                return service.GetWebPartProperties(documentName);
            }
        }

        public string AssociateWorkflowMarkup(string configUrl, string configVersion)
        {
            using (var service = new WebPartPagesService(WebUrl, CookieProvider))
            {
                return service.AssociateWorkflowMarkup(configUrl, configVersion);
            }
        }
        #endregion WebPartPagesService
    }
}