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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
namespace AvePoint.Wrapper.Restore
{
    class SpecialNameDataFormat : BaseDataFormat
    {
        //private static AveLogger log = AveLogger.GetInstance(typeof(SpecialNameDataFormat));
        private Dictionary<string, object> userData;
        private int originalVersion;
        public SpecialNameDataFormat(AveXmlField xmlField, IAveField destField, AveSPItem mItem, Dictionary<string, object> userData, int originalVersion) :
            base(xmlField, destField, mItem)
        {
            this.userData = userData;
            this.originalVersion = originalVersion;
        }

        public override object CheckFieldValue(object value)
        {
            var option = new ReplaceOption(true) { NeedReplaceAbsoluteUrl = true };
            switch (destField.InternalName)
            {
                case "TemplateUrl":
                    bool replace = false;
                    string url = value as string;
                    if (!string.IsNullOrEmpty(url) && url.Contains("&#39;"))
                    {
                        url = url.Replace("&#39;", "'");
                        replace = true;
                    }
                    url = AveReplaceProcessor.UrlReplace(url, this.mItem.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, option, this.mItem.ParentSite.SourceSiteInfo, this.mItem.ParentSite.ServerRelativeUrl);
                    if (replace && !string.IsNullOrEmpty(url) && url.Contains("'"))
                    {
                        url = url.Replace("'", "&#39;");
                    }
                    return url;
                case "ContentType":
                    string ctId = AveConvert.ConvertByteToContentTypeId(this.mItem.ParentSite.ObjectModelFactory, (byte[])userData["#tp_ContentTypeId"]).ToString();
                    value = ctId;
                    break;
            }
            return value;
        }


    }
}
