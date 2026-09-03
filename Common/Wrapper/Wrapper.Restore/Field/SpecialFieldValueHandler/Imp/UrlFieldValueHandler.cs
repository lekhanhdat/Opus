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
namespace AvePoint.Wrapper.Restore
{
    using AvePoint.Wrapper.Common;
    using System.Collections.Generic;

    public class UrlFieldValueHandler : BaseFieldValueHandler, IFieldValueHandler
    {

        public UrlFieldValueHandler(AveSPSite parentAveSite)
            : base(parentAveSite)
        { }

        public object Process(IAveField field, object value, bool isSiteUrlReplaced)
        {
            if (!(value is IAveFieldUrlValue))
            {
                return value;
            }
            var list = field.ParentList;
            IAveFieldUrlValue urlValue = value as IAveFieldUrlValue;
            string newUrl;
            Dictionary<string, string> replaceMapping;
            urlValue.Url = System.Web.HttpUtility.UrlDecode(urlValue.Url);
            if (AttachmentUrlUtility.IsAttachmentUrl(urlValue.Url))
            {
                if (AttachmentUrlUtility.HandleUrlReplacementV1(urlValue.Url, list, isSiteUrlReplaced, mParentSite.MappingManager.SiteMappingManager, out replaceMapping, out newUrl))
                //if (AttachmentUrlUtility.HandleUrlReplacement(urlValue.Url, list, isSiteUrlReplaced, mParentSite.MappingManager.SiteMappingManager, out replaceMapping, out newUrl))
                {
                    urlValue.Url = newUrl;
                    foreach (string key in replaceMapping.Keys)
                    {
                        urlValue.Description = urlValue.Description.ToLower().Replace(key, replaceMapping[key]);
                    }
                }
            }
            //else if (PermissionLinkUtility.IsListPermissionLink(urlValue.Url))
            //{
            //    if (PermissionLinkUtility.HandlePermissionLinkUrl(urlValue.Url, isSiteUrlReplaced, mParentSite.MappingManager.SiteMappingManager, out replaceMapping, out newUrl))
            //    {
            //        urlValue.Url = newUrl;
            //        foreach (string key in replaceMapping.Keys)
            //        {
            //            urlValue.Description = urlValue.Description.ToLower().Replace(key.ToLower(), replaceMapping[key].ToLower());
            //        }
            //    }
            //}
            return urlValue;
        }
    }
}
