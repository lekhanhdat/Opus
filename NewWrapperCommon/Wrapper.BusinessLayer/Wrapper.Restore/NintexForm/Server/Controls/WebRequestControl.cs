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

namespace AvePoint.Wrapper.Restore.NintexForm.Server
{
    class WebRequestControl : BaseControl
    {
        public WebRequestControl(IAveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager, string prefix)
            : base(web, list, contentTypeId, controlNode, nsManager, prefix)
        {
            ResetPrefixAndAddNameSpace();
        }

        public override void ProcessControl(bool isPost)
        {
            var webServiceUrlNode = GetPropertyNode(GetXPath("ServiceURL"));
            var url = webServiceUrlNode == null ? string.Empty : webServiceUrlNode.InnerText;
            if(string.IsNullOrEmpty(url))
            {
                return;
            }
            var siteMappingManager = mWeb.ParentSite.MappingManager.SiteMappingManager;
            webServiceUrlNode.InnerText =AveReplaceProcessor.UrlReplace(url, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
        }
        public void ResetPrefixAndAddNameSpace()
        {
            Prefix= mControlNode.GetPrefixOfNamespace("http://schemas.datacontract.org/2004/07/Nintex.Forms.SharePoint.FormControls");
            nsManager.AddNamespace(Prefix, "http://schemas.datacontract.org/2004/07/Nintex.Forms.SharePoint.FormControls");
        }
    }
}
