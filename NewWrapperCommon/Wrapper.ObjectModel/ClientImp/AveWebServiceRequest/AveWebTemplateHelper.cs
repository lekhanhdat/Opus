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
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Common;
using AveClientRequest.Common;
using System.Text.RegularExpressions;
using AvePoint.Office365.Api;

namespace AvePoint.ObjectModel.WebService
{
    public class AveWebTemplateHelper
    {
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "g_wsaSiteTemplateId is a key")]
        public static Dictionary<string, object> GetWebTemplateConfigurationProperty(string webFullUrl, object obj, string version, int level)
        {
            return GetWebTemplateConfigurationProperty(webFullUrl, obj, null, version, level);
        }
        public static Dictionary<string, object> GetWebTemplateConfigurationProperty(string webFullUrl, object obj,ITokenProvider provider, string version, int level)
        {
            return GetWebTemplateConfiguration(webFullUrl, obj, version, level, true, provider);
        }
        public static string GetWebTemplateConfiguration(string webFullUrl, object obj, string version, int level)
        {
            return GetWebTemplateConfiguration(webFullUrl, obj, null, version, level);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "g_wsaSiteTemplateId is a key")]
        public static string GetWebTemplateConfiguration(string webFullUrl, object obj,ITokenProvider provider, string version, int level)
        {
            Dictionary<string, object> result = GetWebTemplateConfiguration(webFullUrl, obj, version, level, false, provider);
            if (result.ContainsKey("Configuration"))
            {
                return result["Configuration"].ToString();
            }
            return string.Empty;
        }

        private static Dictionary<string, object> GetWebTemplateConfiguration(string webFullUrl, object obj, string version, int level, bool needTemplateId, ITokenProvider tokenProvider = null)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            string htmlUrl = webFullUrl.Trim('/') + (!string.IsNullOrEmpty(version) && string.Compare(version, "15.", StringComparison.OrdinalIgnoreCase) > 0 && level == 15 ? "/_layouts/15/settings.aspx" : "/_layouts/settings.aspx");
            string searchContent = "g_wsaSiteTemplateId";
            string endContent = ";";
            string html = AveHttpWebRequestUtility.HttpGet(htmlUrl, obj, tokenProvider);
            string configuration = string.Empty;
            string property = string.Empty;
            if (!string.IsNullOrEmpty(html))
            {
                int index = html.IndexOf(searchContent, StringComparison.OrdinalIgnoreCase);
                int secondIndex = html.IndexOf(searchContent, index + searchContent.Length, StringComparison.OrdinalIgnoreCase);

                if (secondIndex < 0)
                {
                    configuration = AveHttpWebRequestUtility.GetInput(html, searchContent, endContent);
                }
                else
                {//Support Express Team site in Office 365
                    configuration = AveHttpWebRequestUtility.GetInput(html, secondIndex, searchContent, endContent);
                }
                if (!string.IsNullOrEmpty(configuration))
                {
                    configuration = configuration.Remove(0, searchContent.Length).Trim(new char[] { '=', '\'', ';', '\t', '\n', '\v', '\f', '\r', ' ', '\x0085', '\x00a0', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '​', '\u2028', '\u2029', '　', '﻿' });
                    result["Configuration"] = configuration;
                }
                if (!needTemplateId)
                {
                    return result;
                }
                else
                {
                    string templateIDContent = "_spPageContextInfo";
                    secondIndex = html.IndexOf(searchContent, index + searchContent.Length, StringComparison.OrdinalIgnoreCase);
                    if (secondIndex < 0)
                    {
                        property = AveHttpWebRequestUtility.GetInput(html, templateIDContent, endContent);
                    }
                    else
                    {//Support Express Team site in Office 365
                        property = AveHttpWebRequestUtility.GetInput(html, secondIndex, templateIDContent, endContent);
                    }
                    Regex regex = new Regex("webTemplate: \"\\d+\",");
                    Match match = regex.Match(property);
                    if (match.Success)
                    {
                        string webtemplateIdstring = match.Value.Substring(match.Value.IndexOf('\"'), match.Value.LastIndexOf('\"') - match.Value.IndexOf('\"')).Trim('\"');
                        int webtemplateId;
                        if (int.TryParse(webtemplateIdstring, out webtemplateId))
                        {
                            result["WebTemplateId"] = webtemplateId;
                        }
                    }
                }
            }
            return result;
        }
        //public static string GetWebTemplateTitle(string configuration, string siteUrl, object mObj, string SPVersion, AveBPOSAccountInfo user)
        //{
        //    using (AveWebServiceRequest aveWebServiceRequest = new AveWebServiceRequest(siteUrl, user, mObj, SPVersion))
        //    {
        //        Dictionary<string, object> WebTemplates = aveWebServiceRequest.GetWebTemplates("", aveWebServiceRequest.GetWebLanguage(), false, "");
        //        return GetWebTemplateNameById(configuration, WebTemplates);
        //    }
        //}

        private static string GetWebTemplateNameById(string configuration, Dictionary<string, object> webTemplates)
        {
            string webTemplateStr = string.Empty;
            foreach (object sWebTemplate in webTemplates["ChildrenProperties"] as List<Dictionary<string, object>>)
            {
                Dictionary<string, object> WebTemplates = sWebTemplate as Dictionary<string, object>;
                if (WebTemplates["Name"].ToString().EndsWith(configuration, StringComparison.OrdinalIgnoreCase))
                {
                    webTemplateStr = WebTemplates["Title"].ToString();
                    break;
                }
            }
            return webTemplateStr;
        }
    }
}
