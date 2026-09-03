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
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    public static class PermissionLinkUtility
    {
        public const string GuidRegexFormat = @"[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}|\([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\)|\{[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\}|%7[b|B][a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}%7[d|D]|[a-fA-F\d]{32}";
        public static string ListPermissionRegexString = string.Format(@"[http://|https://][\s\S]*/_layouts/15/user.aspx?obj={0},list&List={0}[\s\S]*",GuidRegexFormat);
        public static bool IsListPermissionLink(string url)
        {
            return Regex.IsMatch(url, ListPermissionRegexString);
        }

        private static string ConvertArrayToStringWithSplit(string[] array, char split)
        {
            bool isFirst = true;
            StringBuilder builder = new StringBuilder();
            foreach (var str in array)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    builder.Append(split);
                }
                builder.Append(str);
            }
            return builder.ToString();
        }

        public static bool HandlePermissionLinkUrl(string url,bool isSiteUrlReplaced, AveSiteMappingManager mapping, out Dictionary<string, string> replaceDictionary, out string newUrl)
        {
            string originalString = HttpUtility.UrlDecode(url);
            var linkUri = new Uri(originalString);
            string queryString = linkUri.Query;
            var parameters = GetQueryParameters(queryString);
            string urlWithoutQuery = originalString.Substring(0,originalString.Length-queryString.Length);
            newUrl = url;
            replaceDictionary = new Dictionary<string, string>();
            bool allReplaced = true;
            bool changed = false;
            foreach (string paramName in parameters.Keys)
            {
                if (string.Equals(paramName, "List", StringComparison.OrdinalIgnoreCase))
                {
                    string value = parameters[paramName];
                    if (Regex.IsMatch(value, GuidRegexFormat))
                    {
                        Guid listId = new Guid(HttpUtility.UrlDecode(value));
                        Guid newId;
                        if (mapping.ListIdMapping.TryGetValue(listId, out newId))
                        {
                            replaceDictionary.Add(listId.ToString("B"), newId.ToString("B"));
                            parameters[paramName] = newId.ToString("B");
                            changed = true;
                        }
                        else
                        {
                            allReplaced = false;
                        }
                    }
                }
                else if (string.Equals(paramName, "Obj", StringComparison.OrdinalIgnoreCase))
                {
                    string value = parameters[paramName];
                    string[] array = value.Split(',');
                    if (array.Length >= 2)
                    {
                        string idStr = array[0];
                        if (Regex.IsMatch(idStr, GuidRegexFormat))
                        {
                            Guid listId = new Guid(idStr);
                            Guid newId;
                            if (mapping.ListIdMapping.TryGetValue(listId, out newId))
                            {
                                string newIdString = newId.ToString("B");
                                replaceDictionary.Add(idStr, newIdString);
                                array[0]=newIdString ;
                                parameters[paramName] = ConvertArrayToStringWithSplit(array,',');
                                changed = true;
                            }
                            else
                            {
                                allReplaced = false;
                            }
                        }
                    }
                }
            }
            if (!isSiteUrlReplaced)
            {
                urlWithoutQuery = AveReplaceProcessor.UrlReplace(urlWithoutQuery, mapping.SiteManagedMappings, new ReplaceOption(true, true), mapping.SourceSiteInfo, mapping.DestSiteInfo.ServerRelativeUrl);
            }
            if (changed)
            {
                
                newUrl = urlWithoutQuery+BuildParameterString(parameters);
            }
            return true;
        }

        public static IDictionary<string, string> GetQueryParameters(string queryString)
        {
            var parameters = new Dictionary<string, string>();
            var parameterList=HttpUtility.ParseQueryString(queryString);
            foreach (string parameterString in parameterList)
            {
                int splitIndex = parameterString.IndexOf("=");
                if (splitIndex > 0)
                {
                    string key = parameterString.Substring(0, splitIndex);
                    string value = parameterString.Substring(splitIndex + 1);
                    parameters[key] = value;
                }
                else
                {
                    parameters[parameterString] = "";
                }

            }
            return parameters;
        }

        public static string BuildParameterString(IDictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return string.Empty;
            }
            var builder = new StringBuilder();
            bool isFirst=true;
            foreach (var key in parameters.Keys)
            {
                if (isFirst)
                {
                    builder.Append("?");
                }
                else
                {
                    builder.Append("&");
                }
                if (string.IsNullOrEmpty(parameters[key]))
                {
                    builder.Append(key);
                }
                else
                {
                    builder.Append(key);
                    builder.Append("=");
                    builder.Append(parameters[key]);
                }
                isFirst = false;
            }
            return builder.ToString();
        }


    }
}
