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
    class ImageControl : BaseControl
    {
        const string sp10LogoUrl = "/_layouts/nintexforms/images/NF_Form_header_Lemon_ffe7a6.png";
        const string sp13LogoUrl = "/_layouts/15/nintexforms/images/NF2013_237x62_BannerStrapLogo.png";
        public ImageControl(IAveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager, string prefix)
            : base(web, list, contentTypeId, controlNode, nsManager, prefix)
        {
        }
        public override void ProcessControl(bool isPost)
        {
            var imageUrlNode = GetPropertyNode(GetXPath("ImageUrl"));
            var imageUrl = imageUrlNode == null ? string.Empty : imageUrlNode.InnerText;
            var siteMappingManager = mWeb.ParentSite.MappingManager.SiteMappingManager;
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }
            if (mWeb.ParentSite.SPContextKind == AveContextKind.Server13ObjectModel
                || mWeb.ParentSite.SPContextKind == AveContextKind.Server16ObjectModel)//change for migration.
            {
                imageUrl = imageUrl.Replace(sp10LogoUrl, sp13LogoUrl);
            }
            imageUrlNode.InnerText = AveReplaceProcessor.UrlReplace(imageUrl, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
        }
    }
}
