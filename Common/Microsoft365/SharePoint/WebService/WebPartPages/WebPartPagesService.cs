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
namespace Microsoft365.SharePoint.WebService
{
    using Microsoft365.SharePoint.WebService.WebPartPages;
    using System;
    using System.Xml;

    public class WebPartPagesService: ServiceBase
    {

        public WebPartPagesService(string webUrl, Func<string> cookieProvider)
            :base(webUrl, cookieProvider)
        {
        }

        protected override string ServiceEndPoint => "/_vti_bin/webpartpages.asmx";

        /// <summary>
        /// get v2 and v3 webparts
        /// </summary>
        /// <param name="pageUrl">Page full url</param>
        /// <param name="storage"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public XmlNode GetWebPartProperties2(string pageUrl, Storage storage=Storage.Shared, SPWebServiceBehavior behavior=SPWebServiceBehavior.Version3)
        {
            var request = new GetWebPartProperties2
            {
                Behavior = behavior,
                PageUrl = pageUrl,
                Storage = storage
            };
            var response = SoapClient.SendRequest<GetWebPartProperties2, GetWebPartProperties2Response>(request);
            return response.GetWebPartProperties2Result;
        }

        /// <summary>
        /// only get v2 webparts
        /// </summary>
        /// <param name="pageUrl">Page full url</param>
        /// <param name="storage"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public XmlNode GetWebPartProperties(string pageUrl, Storage storage = Storage.Shared)
        {
            var request = new GetWebPartProperties
            {
                PageUrl = pageUrl,
                Storage = storage
            };
            var response = SoapClient.SendRequest<GetWebPartProperties, GetWebPartPropertiesResponse>(request);
            return response.GetWebPartPropertiesResult;
        }

        public string GetWebPartPage(string documentName,SPWebServiceBehavior behavior = SPWebServiceBehavior.Version3)
        {
            var request = new GetWebPartPage
            {
                DocumentName = documentName,
                Behavior = behavior
            };
            var response = SoapClient.SendRequest<GetWebPartPage, GetWebPartPageResponse>(request);
            return response.GetWebPartPageResult;
        }

        public string AssociateWorkflowMarkup(string configUrl, string configVersion)
        {
            var request = new AssociateWorkflowMarkup
            {
                ConfigUrl = configUrl,
                ConfigVersion = configVersion
            };
            var response = SoapClient.SendRequest<AssociateWorkflowMarkup, AssociateWorkflowMarkupResponse>(request);
            return response.AssociateWorkflowMarkupResult;
        }
    }
}