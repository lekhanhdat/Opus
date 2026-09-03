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
    class LabelControl : BaseControl
    {
        public LabelControl(IAveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager, string prefix)
            : base(web, list, contentTypeId, controlNode, nsManager, prefix)
        {
        }
        public override void ProcessControl(bool isPost)
        {
            base.ProcessControl(isPost);
            var textNode = mControlNode.SelectSingleNode(GetXPath("Text"), nsManager);
            string innerHtmlText = System.Web.HttpUtility.HtmlDecode(textNode.InnerText);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(innerHtmlText);
            foreach (HtmlNode node in doc.DocumentNode.ChildNodes)
            {
                ReplaceImageUrl(node, isPost);
            }
            textNode.InnerText = doc.DocumentNode.InnerHtml;
        }

        private void ReplaceImageUrl(HtmlNode node, bool isPost)
        {
            if (string.Equals(node.Name, "img", StringComparison.OrdinalIgnoreCase))
            {
                var value = node.GetAttributeValue("src", null);
                string newUrl;
                if (!InternalUrlReplaced(value, out newUrl, isPost, true))
                {
                    throw new AveNintexFormPostException("web", value, contentTypeId);
                }
                else
                {
                    log.Debug("Replace Url for label node,src:{0},dest:{1},isPost:{2}",
                        value, newUrl, isPost);
                    node.SetAttributeValue("src", newUrl);
                }
            }
            foreach (var child in node.ChildNodes)
            {
                ReplaceImageUrl(child, isPost);
            }
        }
    }
}
