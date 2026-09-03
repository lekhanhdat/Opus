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
        const string sp10LogoUrl = "/_layouts/nintexforms/images/NF_Form_header_Lemon_ffe7a6.png";
        const string sp13LogoUrl = "/_layouts/15/nintexforms/images/NF2013_237x62_BannerStrapLogo.png";
        const string onlineLogoUrl = "/Images/NintexForms0365_banner.png";
        public ImageControl(AveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager)
            : base(web, list, contentTypeId, controlNode, nsManager)
        {
        }

        public override void ProcessControl(bool isPost)
        {
            var imageUrlNode = GetPropertyNode("d2p1:ImageUrl");
            var imageUrl = imageUrlNode == null ? string.Empty : imageUrlNode.InnerText;
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }
            imageUrl = imageUrl.Replace(sp10LogoUrl, onlineLogoUrl).Replace(sp13LogoUrl, onlineLogoUrl);
            if (imageUrl.Equals(onlineLogoUrl,StringComparison.OrdinalIgnoreCase))
            {
                imageUrlNode.InnerText = imageUrl;
                return;
            }
            string newUrl;
            if (!InternalUrlReplaced(imageUrl, out newUrl, isPost))
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
