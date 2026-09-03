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

namespace AvePoint.Wrapper.Restore.NintexForm.Online
{
    class ButtonControl : BaseControl
    {
        private const string OnPremiseRedirectUrl = "RedirectUrl";
        private const string OnlineRedirectUrl = "RedirectionUrl";
        public ButtonControl(IAveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager, string prefix)
            : base(web, list, contentTypeId, controlNode, nsManager, prefix)
        {
        }

        public override void ProcessControl(bool isPost)
        {
            base.ProcessControl(isPost);
            ProcessRedirectUrl(isPost);
        }

        private XmlNode GetRedirectUrl(ref bool needAddOnlineRedirectionUrlNode)
        {
            var redirectUrlNode = GetPropertyNode(GetXPath(OnlineRedirectUrl));//先尝试获取online node
            if (redirectUrlNode == null)
            {
                redirectUrlNode = GetPropertyNode(GetXPath(OnPremiseRedirectUrl));// 尝试获取onpremise node
                needAddOnlineRedirectionUrlNode = true;
            }
            return redirectUrlNode;
        }
        private void ProcessRedirectUrl(bool isPost)
        {
            bool needAddOnlineRedirectionUrlNode = false;
            var redirectUrlNode = GetRedirectUrl(ref needAddOnlineRedirectionUrlNode);

            if (redirectUrlNode == null)
            {
                return;
            }

            string redirectUrl = redirectUrlNode.InnerText;

            string newUrl;
            if (!InternalUrlReplaced(redirectUrl, out newUrl, isPost, true))
            {
                throw new AveNintexFormPostException("web", redirectUrl, contentTypeId);
            }
            else
            {
                log.Debug("Replace Url for button redirect url node, src:{0}, dest: {1},isPost: {2}",
                    redirectUrl, newUrl, isPost);
                redirectUrlNode.InnerText = newUrl;
            }

            if (needAddOnlineRedirectionUrlNode)
            {
                var onlineRedirectUrlNode = mControlNode.OwnerDocument.CreateElement(Prefix, OnlineRedirectUrl, mControlNode.GetNamespaceOfPrefix(Prefix));
                onlineRedirectUrlNode.InnerText = newUrl;
                mControlNode.InsertBefore(onlineRedirectUrlNode, redirectUrlNode);
            }
        }
    }
}
