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

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Restore
{
    internal class WebPartBrokenLinkFixerFactory
    {
        public static WebPartBrokenLinkFixer CreateBrokenLinkFixer(IAveWeb web, AveWebPartCache cacheInfo, HtmlNode webpartNode)
        {
            int columnIndex = webpartNode.Name.IndexOf(':');
            string webpartType = columnIndex > 0 ? webpartNode.Name.Substring(columnIndex + 1) : webpartNode.Name;
            //should be case senstive
            switch (webpartType)
            {
                case "contentbyquerywebpart":
                    return new ContentQueryWebPartBrokenLinkFixer(web, cacheInfo);                    
                default:
                    return null;
            }
        }
    }

    internal abstract class WebPartBrokenLinkFixer
    {
        protected static AveLogger mLogger = AveLogger.GetInstance(typeof(WebPartBrokenLinkFixer));
        protected AveWebPartCache mCache = null;        
        protected IAveWeb mWeb;

        public WebPartBrokenLinkFixer(IAveWeb web, AveWebPartCache cache)
        {
            mWeb = web;
            mCache = cache;
        }

        public abstract bool FixBrokenLinks(HtmlNode webpartNode);        
    }

    internal class ContentQueryWebPartBrokenLinkFixer : WebPartBrokenLinkFixer
    {
        public ContentQueryWebPartBrokenLinkFixer(IAveWeb web, AveWebPartCache cache) : base(web, cache)
        {
        }

        public override bool FixBrokenLinks(HtmlNode webpartNode)
        {
            return FixListId(webpartNode);
        }

        private bool FixListId(HtmlNode webpartNode)
        {
            var isFixed = false;
            //ListGuid="26d6a4bb-eed7-4646-9436-52e60242c913" ListName="zzFooterLayoutItems"
            string listName = webpartNode.GetAttributeValue("ListName", string.Empty);
            if (!string.IsNullOrEmpty(listName))
            {
                IAveList list = mWeb.Lists.GetByTitle(listName);
                if (list != null)
                {
                    var listGuid = webpartNode.GetAttributeValue("ListGuid", string.Empty);
                    var currentListGuid = list.ID.ToString();
                    if (!currentListGuid.Equals(listGuid, StringComparison.OrdinalIgnoreCase))
                    {
                        webpartNode.SetAttributeValue("ListGuid", list.ID.ToString());
                        isFixed = true;
                    }
                }
            }

            return isFixed;
        }
    }
}
