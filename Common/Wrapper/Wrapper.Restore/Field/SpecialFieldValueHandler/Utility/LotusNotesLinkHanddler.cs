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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.Wrapper.Restore
{
    class LotusNotesLinkHanddler
    {
        public static bool IsLotusNotesLink(string link)
        {
            return link.IndexOf(@"LotusNotesLinkTrackingLibrary/LotusNotesLinkTracking.aspx?", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsFullUrl(string url)
        {
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        private static string FindBestMatchWebUrl(string url, AveSiteMappingManager mapping)
        {
            string webUrl = "";
            string serverRelativeUrl = url;
            if (IsFullUrl(url))
            {
                serverRelativeUrl = AveUrlUtility.GetServerRelativeUrl(url);
            }
            foreach (var dest in mapping.WebUrlMapping.Values)
            {
                if (serverRelativeUrl.StartsWith(dest.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase)
                    && dest.Length > webUrl.Length)
                {
                    webUrl = dest;
                }

            }
            return webUrl;
        }

        public static bool HandleLotusNotesLink(string link,IAveList currentList,AveSiteMappingManager mapping, out Dictionary<string, string> replaceDictionary, out string newUrl)
        {
            newUrl = HttpUtility.UrlDecode(link);
            replaceDictionary = new Dictionary<string, string>();
            string lotusLink = "/LotusNotesLinkTrackingLibrary/LotusNotesLinkTracking.aspx?";
            int lotusLinkIndex = newUrl.IndexOf(lotusLink);
            if (lotusLinkIndex < 0)
            {
                //replace
                return false;
            }
            string urlWithoutQuery = newUrl.Substring(0,lotusLinkIndex+lotusLink.Length-1);
            string queryString = newUrl.Substring(urlWithoutQuery.Length);
            string newUrlWithoutQuery= AveReplaceProcessor.UrlReplace(urlWithoutQuery, mapping.SiteManagedMappings, 
                new ReplaceOption(true, true), mapping.SourceSiteInfo, mapping.DestSiteInfo.ServerRelativeUrl);

            bool paramChanged = false;
            if (!string.IsNullOrEmpty(queryString))
            {
                queryString = queryString.StartsWith("?") ? queryString : "?" + queryString;
                var parameters = GetQueryParameters(queryString);
                var keys = parameters.Keys.ToList();
                foreach (string paramKey in keys)
                {
                    var paramValue =parameters[paramKey];
                    Uri uri;
                    if ((string.Equals(paramKey, "amp;SharePointLink", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(paramKey, "SharePointLink", StringComparison.OrdinalIgnoreCase))
                        && Uri.TryCreate(paramValue, UriKind.Absolute,out uri))
                    {
                        
                        string spLink = paramValue;
                       // Uri uri = new Uri(spLink);
                        string subQuery = uri.Query;
                        
                        string subUrlWithoutQuery = spLink.Substring(0, spLink.Length - subQuery.Length);
                        string newListUrl = AveReplaceProcessor.UrlReplace(subUrlWithoutQuery, mapping.SiteManagedMappings, new ReplaceOption(true, true), mapping.SourceSiteInfo, mapping.DestSiteInfo.ServerRelativeUrl);
                        IAveList relativeList = GetRelativeList(currentList,newListUrl,mapping);
                        if (relativeList == null)
                        {
                            continue;
                        }
                        paramChanged = true;
                        var subParams = GetQueryParameters(subQuery);
                        bool idReplaced = false;
                        if (subParams.ContainsKey("ID"))
                        {

                            var id = subParams["ID"];
                            int rowId;
                            if (int.TryParse(id, out rowId))
                            {
                                int newId = mapping.GetMappingItemId(relativeList.ID, rowId);
                                if (newId != -1)
                                {
                                    idReplaced = true;
                                    subParams["ID"] = newId.ToString();
                                    string newItemUrl = newListUrl + BuildParameterString(subParams);
                                    replaceDictionary.Add(parameters[paramKey], newItemUrl);
                                    parameters[paramKey] = newItemUrl;
                                }
                            }
                            if (!idReplaced)
                            {
                                parameters[paramKey] = AveReplaceProcessor.UrlReplace(spLink, mapping.SiteManagedMappings, new ReplaceOption(true, true), mapping.SourceSiteInfo, mapping.DestSiteInfo.ServerRelativeUrl);
                            }
                        }
                    }
                }
                if (paramChanged)
                {
                    newUrl = newUrlWithoutQuery.TrimEnd('?') + BuildParameterString(parameters);
                }
            }
            return paramChanged;
        }

        private static IAveList GetRelativeList(IAveList currentList,string newListUrl,AveSiteMappingManager mapping)
        {
            IAveList relativeList = null;
            if (newListUrl.StartsWith(currentList.FullUrl().TrimEnd('/') + "/"))
            {
                relativeList = currentList;
            }
            else
            {
                var webUrl = FindBestMatchWebUrl(newListUrl, mapping);
                if (string.IsNullOrEmpty(webUrl))
                {
                    return relativeList;
                }
                IAveWeb relativeWeb = null;
                if (string.Equals(currentList.ParentWeb.ServerRelativeUrl, webUrl, StringComparison.OrdinalIgnoreCase))
                {
                    relativeWeb = currentList.ParentWeb;
                }
                else
                {
                    relativeWeb = currentList.ParentWeb.Site.OpenWeb(webUrl);
                }
                if (!relativeWeb.Exists)
                {
                    return relativeList;
                }
                relativeList = relativeWeb.GetList(newListUrl);
            }
            return relativeList;
        }
        public static string BuildParameterString(IDictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return string.Empty;
            }
            var builder = new StringBuilder();
            bool isFirst = true;
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

        public static IDictionary<string, string> GetQueryParameters(string queryString)
        {
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var parameterList = HttpUtility.ParseQueryString(queryString);

            foreach (string name in parameterList.AllKeys)
            {
                parameters[name] = parameterList[name];
            }
            return parameters;
        }

    }
}
