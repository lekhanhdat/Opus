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
using System.Xml;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore.NintexForm.Server
{
    class ButtonControl : BaseControl
    {
        public ButtonControl(IAveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager, string prefix)
            : base(web, list, contentTypeId, controlNode, nsManager, prefix)
        {
        }

        public override void ProcessControl(bool isPost)
        {
            replaceUrl(GetXPath("RibbonIconUrl"), isPost);
            replaceUrl(GetXPath("ImageUrl"), isPost);
        }

        private void replaceUrl(string nodeKey, bool isPost)
        {
            var node = GetPropertyNode(nodeKey);
            var sourceUrl = node == null ? string.Empty : node.InnerText;
            var siteMappingManager = mWeb.ParentSite.MappingManager.SiteMappingManager;
            if (!string.IsNullOrEmpty(sourceUrl))
            {
                string newUrl = node.InnerText;
                if(!InternalUrlReplaced(sourceUrl, out newUrl, isPost))
                {
                    throw new AveNintexFormPostException("list", sourceUrl, contentTypeId);
                }
                node.InnerText = newUrl;
            }
        }
    }
}
