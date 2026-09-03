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
namespace LS.SPWorkflowProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AvePoint.Wrapper.Common;

    /// <summary>
    /// workflow中url替换的入口,workflow工程中使用的url replace以后都会以此处为入口
    /// </summary>
    class AveWorkflowReplaceProcessor
    {
        IEnumerable<Dictionary<string, string>> mappings;
        ReplaceOption option;
        AveSiteInfo sourceSiteInfo;
        string destSiteUrl;

        internal AveWorkflowReplaceProcessor(IEnumerable<Dictionary<string, string>> urlMappings, ReplaceOption replaceOption, AveSiteInfo siteInfo, string siteUrl)
        {
            mappings = urlMappings;
            option = replaceOption;
            sourceSiteInfo = siteInfo;
            destSiteUrl = siteUrl;
        }

        /// <summary>
        /// 替换rich text的content中的url
        /// </summary>
        /// <param name="content"></param>
        /// <param name="urlMappings"></param>
        /// <param name="option"></param>
        /// <param name="sourceSiteInfo"></param>
        /// <param name="destSiteUrl"></param>
        /// <returns></returns>
        internal string ReplaceUrlContent(string content)
        {
            return AveReplaceProcessor.ReplaceUrlContent(content, mappings, option, false, sourceSiteInfo, destSiteUrl);
        }

        internal string ReplaceEmailContent(string content)
        {
            var mapping = mappings as List<Dictionary<string, string>>;
            if (mapping != null)
            {
                return AveReplaceProcessor.ReplaceStringLinksForEmail(content, mapping, option, sourceSiteInfo, destSiteUrl);
            }
            return content;
        }

        /// <summary>
        /// 替换url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal string UrlReplace(string url)
        {
            //workflow对于external full url不进行替换
            if (AveReplaceProcessor.IsExternalAbsoluteUrl(url, sourceSiteInfo))
            {
                return url;
            }
            return AveReplaceProcessor.UrlReplace(url, mappings, option, sourceSiteInfo, destSiteUrl);
        }

    }
}
