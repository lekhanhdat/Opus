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
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;

    public static class AttachmentUrlUtility
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveSPFieldCollection));
        public const string AttachmentPart = "/Attachments/";
        public const string AttachmentUrlRegexFormat = "{0}[0-9]*/";
        public static bool IsAttachmentUrl(string url)
        {
            return url != null && url.IndexOf(AttachmentPart, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsCurrentListAttachmentUrl(string link,IAveList currentList)
        {
            string url = currentList.RootFolder.ServerRelativeUrl + "/Attachments/";
            string encodedUrl = HttpUtility.UrlPathEncode(url);
            if (link.IndexOf(url, StringComparison.OrdinalIgnoreCase) >= 0 || link.IndexOf(encodedUrl, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            return false;
        }

        private static bool IsFullUrl(string url)
        {
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        public static bool HandleUrlReplacement(string url, IAveList currentList, bool isSiteUrlReplaced, AveSiteMappingManager mapping, out string newUrl)
        {
            Dictionary<string, string> replaceDictionary;
            return HandleUrlReplacementV1(url, currentList, isSiteUrlReplaced, mapping, out replaceDictionary, out newUrl);
            // return HandleUrlReplacement(url, currentList, isSiteUrlReplaced, mapping, out replaceDictionary, out newUrl);
        }


        public static bool HandleUrlReplacement(string url, IAveList currentList, bool isSiteUrlReplaced, AveSiteMappingManager mapping, out Dictionary<string, string> replaceDictionary, out string newUrl)
        {
            string sourceListUrl = "";
            string currentListUrl = "";
            Guid currentListId = currentList.ID;
            if (IsFullUrl(url))
            {
                currentListUrl = currentList.FullUrl();
                sourceListUrl = isSiteUrlReplaced ? currentListUrl : mapping.AbsoluteUrlMapping.Select(t => t.Key).Where(t => string.Equals(t, currentList.FullUrl(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }
            else
            {
                currentListUrl = currentList.RootFolder.ServerRelativeUrl;
                sourceListUrl = isSiteUrlReplaced ? currentListUrl : mapping.ListUrlMapping.Select(t => t.Key).Where(t => string.Equals(t, currentList.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }
            newUrl = url;
            replaceDictionary = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(sourceListUrl))
            {
                return false;
            }
            return HandleRelativeUrlReplacement(url, sourceListUrl, currentListUrl, currentListId, mapping, out replaceDictionary, out newUrl);
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

        private static IAveList GetRelativeList(IAveList currentList, string newListUrl, AveSiteMappingManager mapping)
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

        public static bool HandleUrlReplacementV1(string url,IAveList currentList,bool isSiteUrlReplaced, AveSiteMappingManager mapping, out Dictionary<string, string> replaceDictionary, out string newUrl)
        {
            log.Info("Handle attachment url replace.{0}",url);
            int attachmentsIndex = url.LastIndexOf(AttachmentPart);
            if (attachmentsIndex < 0)
            {
                newUrl = url;
                log.Info("Url is not attachment url.{0}", url);
                replaceDictionary = new Dictionary<string, string>();
                return false;
            }
            string listUrl = url.Substring(0, attachmentsIndex);
            string newListUrl = AveReplaceProcessor.UrlReplace(listUrl, mapping.SiteManagedMappings, new ReplaceOption(true, true), mapping.SourceSiteInfo, mapping.DestSiteInfo.ServerRelativeUrl);

            var list = GetRelativeList(currentList, newListUrl, mapping);
            if (list == null)
            {
                replaceDictionary = new Dictionary<string, string>();
                newUrl = url;
                log.Info("Relative list is not found.New List Url:{0}", newListUrl);
                //Relative list was not found in destination.
                return false;
            }
            string sourceListUrl = listUrl;
            string currentListUrl = list.RootFolder.ServerRelativeUrl;
            if (IsFullUrl(sourceListUrl))
            {
                currentListUrl = list.FullUrl();
            }
            bool result= HandleRelativeUrlReplacement(url, sourceListUrl, newListUrl, list.ID,mapping,out replaceDictionary,out newUrl);
            log.Info("HandleUrlReplacementV1 Result:{0}", newUrl);
            return result;
        }
        private static bool HandleRelativeUrlReplacement(string url, string sourceListUrl, string currentListUrl, Guid currentListId, AveSiteMappingManager mapping, out Dictionary<string, string> replaceDictionary, out string newUrl)
        {
            string sourceAttachmentsFolderUrl = sourceListUrl + AttachmentPart;
            string newAttachmentsFolderUrl = currentListUrl + AttachmentPart;
            Regex regex = new Regex(string.Format(AttachmentUrlRegexFormat, sourceAttachmentsFolderUrl), RegexOptions.IgnoreCase);
            replaceDictionary = new Dictionary<string, string>();
            newUrl = url;
            var match = regex.Match(url);
            if (match.Success)
            {
                string originalItemAttachmentFolderUrl = match.Value;
                string idStr = originalItemAttachmentFolderUrl.Substring(sourceAttachmentsFolderUrl.Length).TrimEnd('/');
                int id;
                if (int.TryParse(idStr, out id))
                {
                    int newId;
                    if (mapping.ItemIdMapping[currentListId].TryGetValue(id, out newId))
                    {
                        string itemAttachmentFolderUrl = newAttachmentsFolderUrl + newId + "/";
                        replaceDictionary.Add(originalItemAttachmentFolderUrl, itemAttachmentFolderUrl);
                        newUrl = url.Replace(originalItemAttachmentFolderUrl, itemAttachmentFolderUrl);
                    }
                }
                return true;
            }
            return false;

        }
    }
}
