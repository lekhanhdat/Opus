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

namespace AvePoint.Wrapper.Restore.NintexForm.Online
{
    class ImageControl : BaseControl
    {
        private Dictionary<string, string> logoUrlMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "/_layouts/nintexforms/images/NF_Form_header_Lemon_ffe7a6.png","/Images/NintexForms0365_banner.png" },
            { "/_layouts/15/nintexforms/images/NF2013_237x62_BannerStrapLogo.png","/Images/NintexForms0365_banner.png" },
            { "/_layouts/15/nintexforms/images/NF2013_514x98_RightBannerStrap.png","/Images/NintexForms0365_banner.png" },
            { "/_layouts/15/nintexforms/images/NF-responsive-banner-930x92.png","/Images/NF-responsive-banner-930x92.png" },
        };
        public ImageControl(IAveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager, string prefix)
            : base(web, list, contentTypeId, controlNode, nsManager, prefix)
        {
        }

        public override void ProcessControl(bool isPost)
        {
            base.ProcessControl(isPost);
            var imageUrlNode = GetPropertyNode(GetXPath("ImageUrl"));
            var imageUrl = imageUrlNode == null ? string.Empty : imageUrlNode.InnerText;
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }
            string logoUrl;
            if (logoUrlMappings.TryGetValue(imageUrl, out logoUrl))
            {
                imageUrlNode.InnerText = logoUrl;
                return;
            }
            string newUrl;
            if (!InternalUrlReplaced(imageUrl, out newUrl, isPost, true))
            {
                throw new AveNintexFormPostException("web", imageUrl, contentTypeId);
            }
            else
            {
                log.Debug("Replace Url for image node, src:{0}, dest: {1},isPost: {2}",
                    imageUrl, newUrl, isPost);
                imageUrlNode.InnerText = newUrl;
            }
        }
    }
}
