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
using System.Text;
using System.Collections;
using System.Web;
using System.Xml;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Linq;
using HtmlAgilityPack;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using AvePoint.Wrapper.Resource.Common;
using AvePoint.Wrapper.Resource.Restore;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.UserMapping;

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Wrapper.Common.AveReplaceProcessorV2.#.cctor()", MessageId = "sourcedoc")]
namespace AvePoint.Wrapper.Common
{
    public class AveReplaceProcessor
    {
        //去掉全局静态变量，改为传参控制。
        //public static bool keepHeadingUrl = false;
        public static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     是否需要替换指向sitecollection外部的url
        ///     false 绝对url与源端保持一致; 相对url转化为绝对,即实际指向与源端一致
        ///     true 绝对url根据mapping替换; 相对url不变，保持相对结构，即相对目的端
        /// </summary>
        public static bool ReplaceExternalUrl = false;

        private static Dictionary<string, IDScopes> replaceIds = new Dictionary<string, IDScopes>{
            { "guidSite",IDScopes.SiteCollection },
            { "guidWeb",IDScopes.Site },
            { "guidFile",IDScopes.Item },
        };

        /// <summary>
        /// The host header of source site collection URL.
        /// </summary>
        public static string GetHostHeader(string hostHeader)
        {
            Uri uri = new Uri(hostHeader);
            string formattedHostHeader = uri.Scheme + "://" + uri.Host;
            if (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) && (uri.Port != 80))
            {
                formattedHostHeader += (":" + uri.Port);
            }
            if (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) && (uri.Port != 443))
            {
                formattedHostHeader += (":" + uri.Port);
            }
            return formattedHostHeader;
        }

        public static string UrlReplace(string oldUrl, IEnumerable<Dictionary<string, string>> mappings, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl, bool isWorkBookUrl = false)
        {
            //ADO-151035、ADO-151036 由于excludeUrls中的数据是Decode之后的，因此oldUrl也需要Decode
            if (excludedUrls != null && excludedUrls.Contains(UrlDecode(oldUrl)))
            {
                return oldUrl;
            }
            string result = string.Empty;
            if (WrapperConfiguration.UseNewUrlReplaceProcessor)
            {
                var mapping = ConvertToDictionary(mappings);
                var newReplaceProcessor = new AveReplaceProcessorV2(mapping, sourceSiteInfo.Url, destSiteUrl, sourceSiteInfo.Prefixes);
                result = newReplaceProcessor.ReplaceUrl(oldUrl, ConvertToAveUrlReplaceOption(option), isWorkBookUrl);
            }
            else
            {
                result = UrlReplaceInternal(oldUrl, mappings, option, sourceSiteInfo, destSiteUrl);
            }
            return result;
        }


        private static AveUrlReplaceOption ConvertToAveUrlReplaceOption(ReplaceOption option)
        {
            return new AveUrlReplaceOption()
            {
                ReplaceAbsoluteUrl = option.NeedReplaceAbsoluteUrl,
                ReplaceExternalUrl = ReplaceExternalUrl,
                KeepExternalRelativeUrl = option.KeepExternalRelativeUrl
            };
        }

        /// <summary>
        /// 将外围输入的dictionary array 转为 dictionary
        /// </summary>
        /// <param name="mappings"></param>
        /// <returns></returns>
        private static Dictionary<string, string> ConvertToDictionary(IEnumerable<Dictionary<string, string>> mappings)
        {
            Dictionary<string, string> newMapping = new Dictionary<string, string>();
            foreach (var mapping in mappings)
            {
                foreach (var keyValuePair in mapping)
                {
                    string value;
                    if (newMapping.TryGetValue(keyValuePair.Key, out value))
                    {
                        if (value.Length <= keyValuePair.Value.Length)
                        {
                            newMapping[keyValuePair.Key] = keyValuePair.Value;
                        }
                    }
                    else
                    {
                        newMapping[keyValuePair.Key] = keyValuePair.Value;
                    }
                }
            }
            return newMapping;
        }

        private static string UrlReplaceInternal(string oldUrl, IEnumerable<Dictionary<string, string>> mappings, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            //ADO-22037,oldurl为空会抛错，导致之前的替换没有更新到content中
            if (string.IsNullOrEmpty(oldUrl))
            {
                return oldUrl;
            }

            //ADO-78968, if parent url starts with "<A>" and ends with "</A>", cannot replace url.
            if (oldUrl.StartsWith("&lt;A&gt;", StringComparison.OrdinalIgnoreCase))
            {
                oldUrl = oldUrl.Substring(9);
            }
            if (oldUrl.EndsWith("&lt;/A&gt;", StringComparison.OrdinalIgnoreCase))
            {
                oldUrl = oldUrl.Substring(0, oldUrl.LastIndexOf("&lt;/A&gt;", StringComparison.Ordinal));
            }
            //处理替换Url中'?'后参数属性需要替换逻辑
            string queryStr = string.Empty;
            if (oldUrl.IndexOf('?') >= 0)
            {
                int mark = oldUrl.IndexOf('?');
                queryStr = oldUrl.Substring(mark + 1);
                oldUrl = oldUrl.Substring(0, mark + 1); //以问号结尾
                queryStr = ReplaceQueryStr(queryStr, mappings, option, sourceSiteInfo, destSiteUrl);
            }

            oldUrl = UrlDecode(oldUrl);
            string newUrl = oldUrl; // to save url after replaced
            //HttpUtility.UrlDecode(oldUrl)会将Url中的'+'变成' '
            //newUrl = HttpUtility.UrlDecode(oldUrl);
            bool isAbsoluteUrl = IsAbsoluteUrl(oldUrl);
            string hostHeader = GetHostHeader(sourceSiteInfo.Url);
            if (!option.NeedReplaceAbsoluteUrl && isAbsoluteUrl)
            {
                return (oldUrl + queryStr);
            }
            if (isAbsoluteUrl)
            {
                if (oldUrl.StartsWith((hostHeader + sourceSiteInfo.ServerRelativeUrl).TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                {
                    //如果源端是root sitecollection，但是Url是指向非root sitecollection时，不需要替换Url
                    if (string.Compare(sourceSiteInfo.ServerRelativeUrl, "/", StringComparison.Ordinal) == 0)
                    {
                        foreach (string managePath in sourceSiteInfo.Prefixes)
                        {
                            if (!string.IsNullOrEmpty(managePath))
                            {
                                if (oldUrl.StartsWith(hostHeader.TrimEnd('/') + "/" + managePath + "/", StringComparison.OrdinalIgnoreCase)
                                    || oldUrl.Equals(hostHeader.TrimEnd('/') + "/" + managePath, StringComparison.OrdinalIgnoreCase))
                                {
                                    return (oldUrl + queryStr);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!ReplaceExternalUrl)
                    {
                        return (oldUrl + queryStr);
                    }
                }
            }
            if (!option.NeedReplace || IsSpecialUrl(newUrl))
            {
                return (newUrl + queryStr);
            }
            if (oldUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase) && !isAbsoluteUrl)
            {
                //if (oldUrl.StartsWith("/_layouts", StringComparison.OrdinalIgnoreCase))
                //{
                //    return oldUrl;
                //}

                foreach (string managePath in sourceSiteInfo.Prefixes)
                {
                    if (string.IsNullOrEmpty(managePath))
                    {
                        continue;
                    }
                    string tempPath = managePath.TrimEnd('/');
                    if (string.IsNullOrEmpty(tempPath))
                    {
                        continue;
                    }
                    tempPath = "/" + managePath + "/";
                    if (oldUrl.StartsWith(tempPath, StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(destSiteUrl.Trim('/')) || !newUrl.StartsWith(destSiteUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            if ((!String.IsNullOrEmpty(sourceSiteInfo.ServerRelativeUrl.Trim('/')) && !newUrl.StartsWith(sourceSiteInfo.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase)) || String.IsNullOrEmpty(sourceSiteInfo.ServerRelativeUrl.Trim('/')))
                            {
                                if (option.KeepExternalRelativeUrl || ReplaceExternalUrl)
                                {
                                    //保留headdng的相对Url结构
                                    return (oldUrl + queryStr);
                                }
                                return hostHeader + oldUrl + queryStr;
                            }
                        }
                    }
                }
            }
            //if (newUrl.Contains("RootFolder="))
            //{
            //    //"RootFolder=%2Fsites%2Fsource%2FShared%20Documents%2Ffolder1&FolderCTID=0x0120007A870534FF42704A8C299F7E4F3B65DF&View={9908FBF0-E1A6-4B77-A384-7E30833B75E0}"
            //    newUrl = newUrl.Substring(newUrl.IndexOf("RootFolder=",StringComparison.OrdinalIgnoreCase) + 11);
            //    newUrl = newUrl.Substring(0, newUrl.IndexOf('&'));
            //}
            bool appedSlashByUs = false;
            if (!newUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase) && !isAbsoluteUrl)
            {
                newUrl = "/" + newUrl;
                appedSlashByUs = true;
            }

            List<int> splitIndexs = new List<int>(); //避免产生大量String临时对象
            int index = 0;
            if (isAbsoluteUrl)
            {
                splitIndexs.Add(0);
                index = newUrl.IndexOf("//", StringComparison.OrdinalIgnoreCase) + 2;
            }
            while ((index = newUrl.IndexOf('/', index)) >= 0)
            {
                splitIndexs.Add(index++);
            }
            splitIndexs.Add(newUrl.Length);
            bool repFinished = false;
            for (int i = 0; i < splitIndexs.Count - 1; ++i)
            {
                for (int j = splitIndexs.Count - 1; j > i; --j)
                {
                    string key = newUrl.Substring(splitIndexs[0], splitIndexs[j] - splitIndexs[0]);
                    foreach (Dictionary<string, string> mapping in mappings)
                    {
                        if (mapping.ContainsKey(key))
                        {
                            if (mapping[key].Equals(key, StringComparison.OrdinalIgnoreCase))
                            {
                                // don't need to replace url if source and destination's url are the same.
                            }
                            else if (mapping[key].Equals("/")) //目的端是Root，需要处理/sites/a/Tasks--->//Tasks的情况
                            {
                                if (newUrl.Equals(key)) //Url是/sites/a,需要替换为/而不是空
                                {
                                    newUrl = "/";
                                }
                                else
                                {
                                    newUrl = string.Concat(newUrl.Substring(0, splitIndexs[0]), newUrl.Substring(splitIndexs[j]));
                                }
                            }
                            else
                            {
                                newUrl = string.Concat(newUrl.Substring(0, splitIndexs[0]), mapping[key], newUrl.Substring(splitIndexs[j]));
                            }
                            repFinished = true;
                        }
                        if (repFinished)
                        {
                            return (newUrl + queryStr);
                        }
                    }
                }
                if (i == 0 && newUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase) && !appedSlashByUs) //处理源端是Root的情况
                {
                    foreach (Dictionary<string, string> mapping in mappings)
                    {
                        if (mapping.ContainsKey("/"))
                        {
                            if (mapping["/"] != "/" && !newUrl.StartsWith(mapping["/"], StringComparison.OrdinalIgnoreCase)) //[DOC-69984] root 到 非 root 有些url已经加了一次 目的端的url 不应该再加一次 
                            {
                                newUrl = mapping["/"] + newUrl;
                            }
                            return (newUrl + queryStr);
                        }
                    }
                }
            }
            if (newUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase) && appedSlashByUs)
            {
                newUrl = newUrl.Remove(0, 1);
            }

            return (newUrl + queryStr);
        }



        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are case values")]
        private static string ReplaceQueryStr(string queryStr, IEnumerable<Dictionary<string, string>> mappings, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            if (string.IsNullOrEmpty(queryStr))
            {
                return queryStr;
            }

            StringBuilder newUrl = new StringBuilder();
            string[] splitStrs = queryStr.Split('&');
            foreach (string keyValue in splitStrs)
            {
                string key = string.Empty;
                string value = string.Empty;
                bool needEncode = false;
                if (!string.IsNullOrEmpty(keyValue) && keyValue.IndexOf('=') > 0 && (keyValue.Length > keyValue.IndexOf('=') + 1)) //"RootFolder="
                {
                    key = keyValue.Substring(0, keyValue.IndexOf('='));
                    value = keyValue.Substring(keyValue.IndexOf('=') + 1);


                    switch (key.ToLower(CultureInfo.InvariantCulture))
                    {

                        case "alert":
                            value = HttpUtility.UrlDecode(value);
                            if (MatchGuid(value))
                            {
                                Guid alertId = new Guid(value.Trim('{', '}'));
                                Guid mappingValue;
                                if (WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.GetValueFromAlertIdMapping(alertId, out mappingValue))
                                {
                                    value = mappingValue.ToString("b");
                                }
                            }
                            needEncode = true;
                            break;
                        case "rootfolder":
                        case "source":
                        case "sourcedoc":
                        case "u":
                            value = HttpUtility.UrlDecode(value);
                            var valueId = Guid.Empty;
                            if (AveTypeHelper.IsGuid(value) &&
                                WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.GetValueFromSiteAssetsFolderUniqueIdMapping(new Guid(value), out valueId))
                            {
                                value = valueId.ToString("B");
                                needEncode = false;
                            }
                            else
                            {
                                //对于Query中指向同WebApp中其他Site Collection的相对路径，不进行替换，保持相对url结构
                                value = UrlReplace(value, mappings, new ReplaceOption(option) { KeepExternalRelativeUrl = true }, sourceSiteInfo, destSiteUrl);
                                needEncode = true;
                            }
                            break;
                        case "listid":
                        case "list":
                            value = HttpUtility.UrlDecode(value);
                            if (MatchGuid(value))
                            {
                                Guid listId = new Guid(value.Trim('{', '}'));
                                var mappingId = Guid.Empty;
                                if (WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.GetValueFromListIdMapping(listId, out mappingId))
                                {
                                    value = mappingId.ToString("b");
                                }
                            }
                            needEncode = true;
                            break;
                        case "contenttypeid":
                            value = HttpUtility.UrlDecode(value);
                            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                            {
                                //to do
                            }
                            needEncode = true;
                            break;
                        case "view":
                            value = HttpUtility.UrlDecode(value);
                            if (MatchGuid(value))
                            {
                                Guid viewId = new Guid(value.Trim('{', '}'));
                                Guid viewGuidMappingValue;
                                if (WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.GetViewGuidMappingValue(viewId, out viewGuidMappingValue))
                                    value = viewGuidMappingValue.ToString("b");
                            }
                            needEncode = true;
                            break;
                        case "instance_id":
                            value = HttpUtility.UrlDecode(value);
                            if (MatchGuid(value))
                            {
                                Guid instanceId = new Guid(value.ToLower(CultureInfo.InvariantCulture).Trim('{', '}'));
                                if (WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AppInstanceIdMapping.ContainsKey(instanceId))
                                {
                                    value = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AppInstanceIdMapping[instanceId].ToString("b");
                                    break;//符合此条件的不需要encode
                                }
                            }
                            needEncode = true;
                            break;

                        default:
                            break;


                    }
                    newUrl.Append(key);
                    newUrl.Append('=');
                    if (needEncode)
                    {
                        newUrl.Append(HttpUtility.UrlEncode(value));
                    }
                    else
                    {
                        newUrl.Append(value);
                    }

                }
                else
                {
                    newUrl.Append(keyValue);
                }
                newUrl.Append('&');
            }

            return newUrl.ToString().Trim('&');
        }

        private static bool MatchGuid(string value)
        {
            var match = Regex.Match(value, @"{\w{8}-\w{4}-\w{4}-\w{4}-\w{12}}", RegexOptions.IgnoreCase);
            return match.Success;
        }

        public static string UrlReplace(string oldUrl, Dictionary<string, string> mapping, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            Dictionary<string, string>[] tmpMapping = new Dictionary<string, string>[] { mapping };
            return UrlReplace(oldUrl, tmpMapping, option, sourceSiteInfo, destSiteUrl);
        }

        public static string ReplaceUrlInXml(string xml, List<Dictionary<string, string>> mappings, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.PreserveWhitespace = true;
                xDoc.InnerXml = "<ReplaceXmlLinks>" + xml + "</ReplaceXmlLinks>";
                foreach (XmlNode node in xDoc.GetElementsByTagName("a"))
                {
                    node.Attributes["href"].Value = AveReplaceProcessor.UrlReplace(node.Attributes["href"].Value, mappings, option, sourceSiteInfo, destSiteUrl);
                }
                foreach (XmlNode node in xDoc.GetElementsByTagName("img"))
                {
                    node.Attributes["src"].Value = AveReplaceProcessor.UrlReplace(node.Attributes["src"].Value, mappings, option, sourceSiteInfo, destSiteUrl);
                }
                return xDoc.FirstChild.InnerXml;
            }
            catch (XmlException)
            {
                return xml;
            }
        }

        public static string ReplaceParentWebInXoml(string xml, AveSiteMappingManager mappings, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl, Guid destSiteId)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.PreserveWhitespace = true;
                xDoc.InnerXml = "<ReplaceXmlLinks>" + xml + "</ReplaceXmlLinks>";
                foreach (XmlNode node in xDoc.GetElementsByTagName("*"))
                {
                    if (String.Equals(node.Name, "ns1:CreateWeb2Activity"))
                    {
                        ReplaceParentWebInNode(node, "ParentWeb", mappings, option, sourceSiteInfo, destSiteUrl, destSiteId);
                    }
                    else if (String.Equals(node.Name, "ns1:CreateList2Activity"))
                    {
                        ReplaceParentWebInNode(node, "WebId", mappings, option, sourceSiteInfo, destSiteUrl, destSiteId);
                    }
                }
                return xDoc.FirstChild.InnerXml;
            }
            catch (XmlException)
            {
                return xml;
            }
        }

        private static void ReplaceParentWebInNode(XmlNode node, string parentWebIDAttributeName, AveSiteMappingManager mappings, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl, Guid destSiteId)
        {
            if (!String.IsNullOrEmpty(node.Attributes["ParentWebUrl"].Value))
            {
                node.Attributes["ParentWebUrl"].Value = AveReplaceProcessor.UrlReplace(node.Attributes["ParentWebUrl"].Value, mappings.SiteManagedMappings, option, sourceSiteInfo, destSiteUrl);
            }
            else if (!node.Attributes["SiteId"].Value.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase) && !node.Attributes[parentWebIDAttributeName].Value.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (mappings.WebIDMapping.ContainsKey(new Guid(node.Attributes[parentWebIDAttributeName].Value)))
                {
                    node.Attributes["SiteId"].Value = destSiteId.ToString();
                    node.Attributes[parentWebIDAttributeName].Value = mappings.WebIDMapping[new Guid(node.Attributes[parentWebIDAttributeName].Value)].ToString();
                }
            }
        }

        public static string ReplaceTaskContentTypeIdInXoml(string xml, AveWebMappingManager mappings)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.PreserveWhitespace = true;
                xDoc.InnerXml = "<ReplaceXmlLinks>" + xml + "</ReplaceXmlLinks>";
                foreach (XmlNode node in xDoc.GetElementsByTagName("ns1:CollectDataTaskAndEscalation"))
                {
                    if (!String.IsNullOrEmpty(node.Attributes["TaskContentTypeId"].Value))
                    {
                        if (mappings.WebLevelCTIdMapping.ContainsKey(node.Attributes["TaskContentTypeId"].Value))
                        {
                            node.Attributes["TaskContentTypeId"].Value = mappings.WebLevelCTIdMapping[node.Attributes["TaskContentTypeId"].Value].ToString();
                        }
                    }
                }
                return xDoc.FirstChild.InnerXml;
            }
            catch (XmlException)
            {
                return xml;
            }
        }

        public static string ReplaceActionContentTypeIDInXoml(string xml, Dictionary<string, IAveContentTypeId> contentTypeIdMapping)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.PreserveWhitespace = true;
                xDoc.InnerXml = "<ReplaceXmlLinks>" + xml + "</ReplaceXmlLinks>";
                foreach (XmlNode node in xDoc.GetElementsByTagName("ns1:FindValueActivity"))
                {
                    if (node.Attributes["ExternalFieldName"].Value.Equals("ContentTypeId", StringComparison.OrdinalIgnoreCase))
                    {
                        if (node.ChildNodes.Count == 1)
                        {
                            var ctid = node.ChildNodes[0].ChildNodes[0].InnerText;
                            if (!String.IsNullOrEmpty(ctid) && contentTypeIdMapping.ContainsKey(ctid))
                            {
                                node.ChildNodes[0].ChildNodes[0].InnerText = contentTypeIdMapping[ctid].ToString();
                            }
                        }
                        else if (node.ChildNodes.Count > 1)//现在没做出这种case，并且看最初修改的jira ADO-41338 也没有多个childrennode，但是修改比较久远，先暂时保留这个逻辑
                        {
                            var ctid = node.ChildNodes[1].ChildNodes[1].InnerText;
                            if (!String.IsNullOrEmpty(ctid) && contentTypeIdMapping.ContainsKey(ctid))
                            {
                                node.ChildNodes[1].ChildNodes[1].InnerText = contentTypeIdMapping[ctid].ToString();
                            }
                        }
                        
                    }
                }
                return xDoc.FirstChild.InnerXml;
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while replacing ctid.Error:{0}", e);
                return xml;
            }
        }

        public static string ReplaceStringLinks(string strValue, List<Dictionary<string, string>> mappings, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            if (string.IsNullOrEmpty(strValue))
            {
                return strValue;
            }
            List<string> links = GetLinks(strValue);
            StringBuilder builder = new StringBuilder(strValue);
            foreach (string link in links)
            {
                string newLink = AveReplaceProcessor.UrlReplace(link, mappings, option, sourceSiteInfo, destSiteUrl);
                builder.Replace(string.Format("\"{0}\"", link), string.Format("\"{0}\"", newLink));
                builder.Replace(string.Format("&quot;{0}&quot;", link), string.Format("&quot;{0}&quot;", newLink)); // process html encode
            }
            return builder.ToString();
        }

        public static string ReplaceStringLinksForEmail(string strValue, List<Dictionary<string, string>> mappings, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            if (string.IsNullOrEmpty(strValue))
            {
                return strValue;
            }
            List<string> links = GetLinks(strValue);
            StringBuilder builder = new StringBuilder(strValue);
            foreach (string link in links)
            {
                string newLink = AveReplaceProcessor.UrlReplace(link, mappings, option, sourceSiteInfo, destSiteUrl);
                builder.Replace(string.Format("\"{0}\"", link), string.Format("\"{0}\"", newLink));
                builder.Replace(string.Format(">{0}<", link), string.Format(">{0}<", newLink));
                builder.Replace(string.Format("&quot;{0}&quot;", link), string.Format("&quot;{0}&quot;", newLink)); // process html encode
            }
            return builder.ToString();
        }

        public static string ReplaceStringListId(string strValue, Dictionary<Guid, Guid> mappings, out Guid result)
        {
            result = Guid.Empty;
            List<string> links = GetLinks(strValue);
            StringBuilder builder = new StringBuilder(strValue);
            foreach (string link in links)
            {
                if (!link.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) && link.IndexOf("ListId=", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string newLink = link;
                    string oldListId = link.Substring(link.IndexOf("ListId=", StringComparison.OrdinalIgnoreCase) + 8,
                        link.Substring(link.IndexOf("ListId=", StringComparison.OrdinalIgnoreCase) + 8).IndexOf('}'));
                    if (mappings.ContainsKey(new Guid(oldListId)))
                    {
                        newLink = link.Replace(oldListId, Convert.ToString(mappings[new Guid(oldListId)]));
                        builder.Replace(link, newLink);
                    }
                    else
                    {
                        result = new Guid(oldListId);
                    }
                }
            }
            return builder.ToString();
        }

        private static List<string> GetLinks(string strValue)
        {
            int length = strValue.Length;
            int index = 0;
            List<string> links = new List<string>();
            while (index >= 0 && index < length)
            {
                if (strValue[index] == '<')
                {
                    if ((index + 2 < length) && strValue.Substring(index, 3).Equals("<a ", StringComparison.OrdinalIgnoreCase))
                    {
                        int end = strValue.IndexOf('>', index);
                        if (end > 0)
                        {
                            int p1 = strValue.IndexOf("href=" + '"', index, StringComparison.OrdinalIgnoreCase);
                            int p2 = -1;
                            if (p1 > 0)
                            {
                                p1 = p1 + 6;
                                p2 = strValue.IndexOf('"', p1);
                            }
                            if ((index < p1) && (p1 < p2) && (p2 < end))
                            {
                                string str = strValue.Substring(p1, p2 - p1);
                                links.Add(strValue.Substring(p1, p2 - p1));
                            }
                            index = end;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if ((index + 4 < length) && strValue.Substring(index, 5).Equals("<img ", StringComparison.OrdinalIgnoreCase))
                    {
                        int end = strValue.IndexOf('>', index);
                        if (end > 0)
                        {
                            int p1 = strValue.IndexOf("src=" + '"', index, StringComparison.OrdinalIgnoreCase);
                            int p2 = -1;
                            if (p1 > 0)
                            {
                                p1 = p1 + 5;
                                p2 = strValue.IndexOf('"', p1);
                            }
                            if ((index < p1) && (p1 < p2) && (p2 < end))
                            {
                                string str = strValue.Substring(p1, p2 - p1);
                                links.Add(strValue.Substring(p1, p2 - p1));
                            }
                            index = end;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if ((index + 5 < length) && strValue.Substring(index, 6).Equals("<area ", StringComparison.OrdinalIgnoreCase))
                    {
                        int end = strValue.IndexOf('>', index);
                        if (end > 0)
                        {
                            int p1 = strValue.IndexOf("href=" + '"', index, StringComparison.OrdinalIgnoreCase);
                            int p2 = -1;
                            if (p1 > 0)
                            {
                                p1 = p1 + 6;
                                p2 = strValue.IndexOf('"', p1);
                            }
                            if ((index < p1) && (p1 < p2) && (p2 < end))
                            {
                                string str = strValue.Substring(p1, p2 - p1);
                                links.Add(strValue.Substring(p1, p2 - p1));
                            }
                            index = end;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                index++;
            }
            //ADO-56006
            string needReplaceString = "STSNavigate(&quot;";
            int position = strValue.IndexOf(needReplaceString, StringComparison.OrdinalIgnoreCase);
            if (position >= 0)
            {
                string targetString = strValue.Substring(position + needReplaceString.Length);
                position = targetString.IndexOf("&quot;", StringComparison.OrdinalIgnoreCase);
                if (position > 0)
                {
                    needReplaceString = targetString.Substring(0, position);
                    links.Add(needReplaceString);
                }
            }

            return links.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// </summary>
        /// <param name="sourceContent">Source content</param>
        /// <param name="mappings">Mappings need used to replace url or id in source content</param>
        /// <param name="option">Replace option</param>
        /// <param name="sourceSiteInfo"></param>
        /// <param name="destSiteUrl"></param>
        /// <param name="attributeNeedReplace">A list contains the attribute that need to replace</param>
        /// <returns></returns>
        public static string ReplaceAspContent(string sourceContent, IEnumerable<Dictionary<string, string>> mappings, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl, List<string> attributeNeedReplace)
        {
            string tagName = "asp";
            XmlDocument doc = new XmlDocument();
            string resultContent = sourceContent;
            try
            {
                string xmlContent = AspContentToXml(sourceContent, tagName);
                doc.LoadXml(xmlContent);
                foreach (XmlNode node in doc.GetElementsByTagName(tagName))
                {
                    XmlElement ele = node as XmlElement;
                    if (ele == null)
                        continue;
                    foreach (string attributeName in attributeNeedReplace)
                    {
                        XmlAttribute attribute = ele.GetAttributeNode(attributeName);
                        if (attribute != null)
                        {
                            string oldAttributeValue = attribute.Value;
                            string newAttributeValue = oldAttributeValue;
                            newAttributeValue = UrlReplace(oldAttributeValue, mappings, option, sourceSiteInfo, destSiteUrl);
                            if (!string.Equals(newAttributeValue, oldAttributeValue, StringComparison.OrdinalIgnoreCase))
                            {
                                XmlAttribute newAttribute = doc.CreateAttribute(attributeName);
                                newAttribute.Value = newAttributeValue;
                                resultContent = resultContent.Replace(attribute.OuterXml, newAttribute.OuterXml);
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("Error while replace asp content,error:{0}", ex);
                return sourceContent;
            }
            return resultContent;
        }

        #region 下列函数用于替换listid，将AveSPFieldCollection和AveSPSite中的同名函数移到这里。
        public static string ReplaceXmlLinks(string fieldValue, AveMappingManager mappingManager, AveSiteInfo sourceSiteInfo, string destSiteUrl, IAveList currentList, ref bool needReplaceLast)
        {
            try
            {
                HtmlDocument fieldDoc = new HtmlDocument();
                fieldDoc.OptionOutputOriginalCase = true;
                fieldDoc.LoadHtml("<ReplaceXmlLinks>" + fieldValue + "</ReplaceXmlLinks>");
                List<HtmlNode> nodes = new List<HtmlNode>();
                GetLinkNodes(nodes, fieldDoc.DocumentNode);
                foreach (var node in nodes)
                {
                    if (ReplaceXmlLinks(node, mappingManager, sourceSiteInfo, currentList, destSiteUrl))
                    {
                        needReplaceLast = true;
                    }
                }
                if (nodes.Count == 0)
                {
                    return fieldValue;
                }
                //return fieldDoc.DocumentNode.InnerHtml;
                return fieldDoc.DocumentNode.FirstChild.InnerHtml;
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ReplaceXmlLinksError, ex.ToString());
                try
                {
                    fieldValue = ReplaceUrlContent(fieldValue, mappingManager.SiteMappingManager.SiteManagedMappings, sourceSiteInfo, destSiteUrl);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while replace xml Links.error message:{0}", e);
                }
                return fieldValue;
            }
        }

        internal static string ReplaceNewsFeedLinks(string fieldValue, IAveList list, AveMappingManager mappingManager, AveSiteInfo sourceSiteInfo, string destSiteUrl, ref bool needRestoreLast)
        {
            try
            {
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(fieldValue);
                var manager = new XmlNamespaceManager(xd.NameTable);
                manager.AddNamespace("MFP", "http://Microsoft/Office/Server/Microfeed");
                string[] xPaths = new string[] { "//MFP:q/MFP:L", "//MFP:q/MFP:S" };
                foreach (string path in xPaths)
                {
                    var node = xd.DocumentElement.SelectSingleNode(path, manager);
                    if (node != null)
                    {
                        node.InnerText = UrlReplace(node.InnerText,
                            mappingManager.SiteMappingManager.SiteManagedMappings,
                            new ReplaceOption(true, true),
                            sourceSiteInfo,
                            destSiteUrl);
                        node.InnerText = AttachmentItemIdReplace(node.InnerText, list, mappingManager, ref needRestoreLast);
                    }
                }
                return xd.OuterXml;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while replace news feed link. value: {0} Error: {1}", fieldValue, e);
                return fieldValue;
            }
        }

        private static bool ReplaceXmlLinks(HtmlNode node, AveMappingManager mappingManager, AveSiteInfo sourceSiteInfo, IAveList currentList, string destSiteUrl)
        {
            HtmlAttribute linkAttribute;
            bool needReplaceLast = false;
            if (node.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
            {
                linkAttribute =
                    node.Attributes.Cast<HtmlAttribute>().FirstOrDefault(attribute => attribute.Name.Equals("href", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                linkAttribute =
                    node.Attributes.Cast<HtmlAttribute>().FirstOrDefault(attribute => attribute.Name.Equals("src", StringComparison.OrdinalIgnoreCase));
            }
            if (linkAttribute == null)
            {
                return needReplaceLast;
            }
            string hrefLink = UrlDecode(linkAttribute.Value);
            string urlreplaceHrefLink = AveReplaceProcessor.UrlReplace(hrefLink,
                                                                 mappingManager.SiteMappingManager.SiteManagedMappings,
                                                                 new ReplaceOption(true, true),
                                                                 sourceSiteInfo,
                                                                 destSiteUrl);
            string idReplaceUrl = IdReplace(urlreplaceHrefLink, mappingManager, ref needReplaceLast);
            linkAttribute.Value = AttachmentItemIdReplace(idReplaceUrl, currentList, mappingManager, ref needReplaceLast);
            foreach (HtmlNode child in GetAllChildrenNodes(node))
            {
                HtmlTextNode textNode = child as HtmlTextNode;
                if (textNode != null && textNode.NodeType == HtmlNodeType.Text)
                {
                    if (HttpUtility.HtmlDecode(textNode.Text).Equals(hrefLink))
                    {
                        textNode.Text = HttpUtility.UrlDecode(linkAttribute.Value);
                    }
                    else if ((HttpUtility.HtmlDecode(textNode.Text).EndsWith(hrefLink, StringComparison.OrdinalIgnoreCase)
                        || HttpUtility.UrlDecode(textNode.Text).EndsWith(hrefLink, StringComparison.OrdinalIgnoreCase))
                        &&
                        (HttpUtility.HtmlDecode(textNode.Text).StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                        || HttpUtility.HtmlDecode(textNode.Text).StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                    {
                        hrefLink = UrlDecode(textNode.Text);
                        hrefLink = AveReplaceProcessor.UrlReplace(hrefLink,
                                                                     mappingManager.SiteMappingManager.SiteManagedMappings,
                                                                     new ReplaceOption(true, true),
                                                                     sourceSiteInfo,
                                                                     destSiteUrl);
                        idReplaceUrl = IdReplace(hrefLink, mappingManager, ref needReplaceLast);
                        textNode.Text = AttachmentItemIdReplace(idReplaceUrl, currentList, mappingManager, ref needReplaceLast);
                    }
                }
            }
            return needReplaceLast;
        }

        private static List<HtmlNode> GetAllChildrenNodes(HtmlNode node)
        {
            List<HtmlNode> children = new List<HtmlNode>();
            foreach (var child in node.ChildNodes)
            {
                foreach (var subNode in GetAllChildrenNodes(child))
                {
                    if (!children.Contains(subNode))
                    {
                        children.Add(subNode);
                    }
                }
            }
            if (!children.Contains(node))
            {
                children.Add(node);
            }
            return children;
        }

        public static string AttachmentItemIdReplace(string fileUrl, IAveList list, AveMappingManager mappingManager, ref bool needRestoreLast)
        {
            if (IsAbsoluteUrl(fileUrl) ||
                fileUrl.IndexOf("attachments", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return fileUrl;
            }
            try
            {
                //Get attachment's List.
                string listUrl = fileUrl.Substring(0, fileUrl.IndexOf("attachments", StringComparison.OrdinalIgnoreCase));
                IAveList parentList = list != null && fileUrl.StartsWith(list.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase) ? list : list.ParentWeb.GetList(listUrl);
                //Handle item id in URL.
                string[] UrlSegments = fileUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                int itemId = -1;
                if (int.TryParse(UrlSegments[UrlSegments.Length - 2], out itemId))
                {
                    if (parentList != null)
                    {
                        int tempUrlSegment = mappingManager.SiteMappingManager.GetMappingItemId(parentList.ID, itemId);
                        if (tempUrlSegment != -1)
                        {
                            UrlSegments[UrlSegments.Length - 2] = tempUrlSegment.ToString();

                            //Contact the URL segments to get the new URL.
                            string newAttachmentUrl = string.Empty;
                            foreach (string segment in UrlSegments)
                            {
                                newAttachmentUrl += "/" + segment;
                            }
                            return newAttachmentUrl;
                        }
                    }
                }
                needRestoreLast = true;
            }
            catch (Exception ex)
            {
                log.Warn("The file URL:'{0}' is not an attachment's URL.Message:{1}", fileUrl, ex.ToString());
            }
            return fileUrl;
        }

        public static string IdReplace(string oldUrl, AveMappingManager mappingManager, ref bool needReplaceLast)
        {
            try
            {
                var decodeUrl = System.Web.HttpUtility.HtmlDecode(oldUrl); //ADO-196159 需要htmldecode将&amp;decode成&

                Guid sourceDocumentId;
                if (AveUrlUtility.IsDurableLink(decodeUrl, out sourceDocumentId))
                {
                    string destDurableLink;
                    if (mappingManager.SiteMappingManager.TryGetDurableLinkUrl(sourceDocumentId, out destDurableLink))
                    {
                        return destDurableLink;
                    }
                    else
                    {
                        needReplaceLast = true;
                        return oldUrl;
                    }
                }

                Dictionary<string, string> idDic = new Dictionary<string, string>();
                string tempUrl = decodeUrl.Substring(decodeUrl.LastIndexOf('?') + 1);
                if (string.IsNullOrEmpty(tempUrl))
                {
                    return oldUrl;
                }
                string idUrl = decodeUrl.Substring(decodeUrl.LastIndexOf('?') + 1);
                string[] ids = idUrl.Split('&');
                foreach (string id in ids)
                {
                    string[] kv = id.Split('=');
                    if (kv.Length == 2)
                    {
                        idDic.Add(kv[0], kv[1]);
                    }
                }
                foreach (KeyValuePair<string, string> kvp in idDic)
                {
                    try
                    {
                        Guid id = new Guid(kvp.Value);
                        
                        if (kvp.Key.ToString().Equals("c", StringComparison.OrdinalIgnoreCase) || kvp.Key.ToString().Equals("listid", StringComparison.OrdinalIgnoreCase))
                        {
                            var valueId = Guid.Empty;
                            if (mappingManager.SiteMappingManager.GetValueFromListIdMapping(id, out valueId))
                            {
                                //Guid类型ToString是小写，idUrl中大小写不一定，做一个特殊处理
                                int index = idUrl.IndexOf(id.ToString(), StringComparison.OrdinalIgnoreCase);
                                string sourceId = idUrl.Substring(index, id.ToString().Length);
                                idUrl = idUrl.Replace(sourceId, valueId.ToString());
                            }
                            else
                            {
                                needReplaceLast = true;
                                return oldUrl;
                            }
                        }
                        else
                        {
                            IDScopes scope = IDScopes.Invalid;
                            if (replaceIds.TryGetValue(kvp.Key, out scope))
                            {
                                var mapping = GetGUIDMappingByScope(scope, mappingManager.SiteMappingManager);
                                if (mapping.ContainsKey(id))
                                {
                                    var result = AveReplaceProcessor.GuidReplace(id, mapping);
                                    int index = idUrl.IndexOf(id.ToString(), StringComparison.OrdinalIgnoreCase);
                                    string sourceId = idUrl.Substring(index, id.ToString().Length);
                                    idUrl = idUrl.Replace(sourceId, result.ToString());
                                }
                                else
                                {
                                    needReplaceLast = true;
                                    return oldUrl;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetIdFailed, e);
                    }
                }
                return decodeUrl.Replace(tempUrl, idUrl);
            }
            catch (Exception ex)
            {
                log.Warn("Replace Id Error. Message:" + ex.ToString());

            }
            return oldUrl;
        }

        private static Dictionary<Guid, Guid> GetGUIDMappingByScope(IDScopes scope , AveSiteMappingManager mapping)
        {
            switch (scope)
            {
                case IDScopes.SiteCollection:
                    return mapping.SiteIDMapping;
                case IDScopes.Site:
                    return mapping.WebIDMapping;
                case IDScopes.List:
                    return mapping.ListIdMapping;
                case IDScopes.Folder:
                    return new Dictionary<Guid, Guid>();//Not Supported at the moment
                case IDScopes.Item:
                    return mapping.DocumentUniqueIdMapping;
                case IDScopes.Invalid:
                default:
                    throw new NotSupportedException("Mapping scope is not supported.");
            }
        }

        /// <summary>
        /// 处理ReplaceXmlLinks的原有方法，保持逻辑不变
        /// </summary>
        /// <param name="content"></param>
        /// <param name="urlMappings"></param>
        /// <param name="sourceSiteInfo"></param>
        /// <param name="destSiteUrl"></param>
        /// <returns></returns>
        internal static string ReplaceUrlContent(string content, IEnumerable<Dictionary<string, string>> urlMappings, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            content = ReplaceUrlContent(content, urlMappings, new ReplaceOption(true, true), true, sourceSiteInfo, destSiteUrl);
            return content;
        }

        /// <summary>
        /// 处理Img,link等link url替换
        /// </summary>
        /// <param name="content"></param>
        /// <param name="urlMappings"></param>
        /// <param name="option">UrlReplace使用的replace option</param>
        /// <param name="needReplaceExternalAbsoluteUrl">是否替换external full url(原端webapp下其他sc中的full url)</param>
        /// <param name="sourceSiteInfo"></param>
        /// <param name="destSiteUrl"></param>
        /// <returns></returns>
        public static string ReplaceUrlContent(string content, IEnumerable<Dictionary<string, string>> urlMappings, ReplaceOption option, bool needReplaceExternalAbsoluteUrl, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            content = ReplaceUrlContent(content, "<img ", "src=", urlMappings, option, needReplaceExternalAbsoluteUrl, sourceSiteInfo, destSiteUrl);
            content = ReplaceUrlContent(content, "<a ", "href=", urlMappings, option, needReplaceExternalAbsoluteUrl, sourceSiteInfo, destSiteUrl);
            //content = ReplaceUrlContent(content, "<%@ Page ", "MasterPageFile=", urlMappings, sourceSiteInfo, destSiteUrl);
            //content = ReplaceUrlContent(content, "<SharePoint:SiteLogoImage", "LogoImageUrl=", urlMappings, sourceSiteInfo, destSiteUrl);
            //content = ReplaceUrlContent(content, "<SharePoint:SPLinkButton", "NavigateUrl=", urlMappings, sourceSiteInfo, destSiteUrl);
            //content = ReplacePageLayout(content, out hasChanged);
            //替换如<td onclick="window.location.href='/home/About us';">About Us</td>中的url
            //content = ReplaceUrlContent(content, "<td", "window.location.href=", urlMappings, sourceSiteInfo, destSiteUrl);
            return content;
        }

        private static string ReplaceUrlContent(string content, string tag, string attribute, IEnumerable<Dictionary<string, string>> urlMappings, ReplaceOption option, bool needReplaceExternalAbsoluteUrl, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            int index = 0;
            while (index != -1 && (index = content.IndexOf(tag, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                index += tag.Length;
                content = ReplaceUrlContent(content, attribute, urlMappings, option, needReplaceExternalAbsoluteUrl, sourceSiteInfo, destSiteUrl, ref index);//changge index,out index -> ref index
                if (index >= content.Length)
                {
                    break;
                }

            }
            return content;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="attribute"></param>
        /// <param name="urlMappings"></param>
        /// <param name="option"></param>
        /// <param name="needReplaceExternalAbsoluteUrl">是否替换external full url(原端webapp下其他sc中的full url)</param>
        /// <param name="sourceSiteInfo"></param>
        /// <param name="destSiteUrl"></param>
        /// <param name="newIndex"></param>
        /// <returns></returns>
        [SuppressMessage("CheckHardCode", "Z100009:CheckString", Justification = "")]
        private static string ReplaceUrlContent(string content, string attribute, IEnumerable<Dictionary<string, string>> urlMappings, ReplaceOption option, bool needReplaceExternalAbsoluteUrl, AveSiteInfo sourceSiteInfo, string destSiteUrl, ref int newIndex)// change int index out int new index
        {
            newIndex = content.IndexOf(attribute, newIndex, StringComparison.OrdinalIgnoreCase);// change s index ->new index
            if (newIndex != -1)
            {
                string url = GetAttribute(content, newIndex, attribute);
                if (!needReplaceExternalAbsoluteUrl && sourceSiteInfo != null && IsExternalAbsoluteUrl(url, sourceSiteInfo))
                {
                    //外部绝对url不需要替换
                    newIndex += url.Length;
                    return content;
                }
                if (!String.IsNullOrEmpty(url) && !url.StartsWith("/_layouts", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("../", StringComparison.OrdinalIgnoreCase))
                {
                    string newUrl = AveReplaceProcessor.UrlReplace(url, urlMappings, option, sourceSiteInfo, destSiteUrl);
                    if (!url.Equals(newUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        content = ReplaceString(content, newIndex, url, newUrl);//index->newIndex
                        newIndex += newUrl.Length;
                    }
                }
                newIndex += attribute.Length;
            }
            return content;
        }

        /// <summary>
        ///  判断是否是external full url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sourceSiteInfo"></param>
        /// <returns></returns>
        public static bool IsExternalAbsoluteUrl(string url, AveSiteInfo sourceSiteInfo)
        {
            return ((url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
                   IsExternalUrl(url, sourceSiteInfo));
        }

        /// <summary>
        /// 检查url是否是site collection外内部的url
        /// </summary>
        /// <param name="url">full url</param>
        /// <param name="sourceSiteInfo"></param>
        /// <returns></returns>
        private static bool IsExternalUrl(string oldUrl, AveSiteInfo sourceSiteInfo)
        {
            bool isExternalUrl = false;
            string hostHeader = GetHostHeader(sourceSiteInfo.Url);
            if (oldUrl.StartsWith((sourceSiteInfo.Url).TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
            {
                //如果源端是root sitecollection，但是Url是指向非root sitecollection时，不需要替换Url
                if (string.Compare(sourceSiteInfo.ServerRelativeUrl, "/", StringComparison.Ordinal) == 0)
                {
                    foreach (string managePath in sourceSiteInfo.Prefixes)
                    {
                        if (!string.IsNullOrEmpty(managePath))
                        {
                            if (oldUrl.StartsWith(hostHeader.TrimEnd('/') + "/" + managePath + "/", StringComparison.OrdinalIgnoreCase)
                                || oldUrl.Equals(hostHeader.TrimEnd('/') + "/" + managePath, StringComparison.OrdinalIgnoreCase))
                            {
                                isExternalUrl = true;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                isExternalUrl = true;
            }
            return isExternalUrl;
        }

        private static string GetAttribute(string content, int index, string tag)
        {
            int newIndex = index;
            index = content.IndexOf(tag, index, StringComparison.OrdinalIgnoreCase);
            if (index == -1)
            {
                newIndex = content.Length;
                return null;
            }
            string newContent = content.Substring(index + tag.Length);
            index += (tag.Length + 1);
            char sliptChar = newContent[0];
            if (sliptChar != '\'' && sliptChar != '\"')
            {
                sliptChar = ' ';
                index -= 1;
                int spaceIndex = newContent.IndexOf(' ');
                newIndex = newContent.IndexOf('>');

                if (spaceIndex != -1)
                {
                    newIndex = newIndex < spaceIndex ? newIndex : spaceIndex;
                }
                if (newIndex == -1)
                {
                    newIndex = newContent.Length;
                    return null;
                }
            }
            else
            {
                newIndex = newContent.IndexOf(sliptChar, 1);
                if (newIndex == -1)
                {
                    newIndex = newContent.Length;
                    return null;
                }
            }
            newContent = newContent.Substring(0, newIndex).Trim(new char[] { '\'', '\"', ' ' });

            return newContent;
        }

        private static string ReplaceString(string content, int index, string oldValue, string newValue)
        {
            if ((index = content.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                content = content.Substring(0, index) + newValue + content.Substring(index + oldValue.Length);
            }
            return content;
        }

        [Obsolete("use ReplaceUrlContent instead.")]
        private static string ReplaceStringLinks(string strValue, IEnumerable<Dictionary<string, string>> mappings, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.ReplaceStringLinks"))
            {
                int length = strValue.Length;
                int index = 0;
                List<string> links = new List<string>();
                while (index < length)
                {
                    if (strValue[index] == '<')
                    {
                        if ((index + 2 < length) && strValue.Substring(index, 3) == "<a ")
                        {
                            int end = strValue.IndexOf('>', index);
                            if (end > 0)
                            {
                                int p1 = strValue.IndexOf("href=" + '"', index, StringComparison.OrdinalIgnoreCase);
                                int p2 = -1;
                                if (p1 > 0)
                                {
                                    p1 = p1 + 6;
                                    p2 = strValue.IndexOf('"', p1);
                                }
                                if ((index < p1) && (p1 < p2) && (p2 < end))
                                {
                                    if (!links.Contains(strValue.Substring(p1, p2 - p1)))
                                    {
                                        links.Add(strValue.Substring(p1, p2 - p1));
                                    }
                                }
                            }
                            index = end;
                        }
                        else if ((index + 4 < length) && strValue.Substring(index, 5) == "<img ")
                        {
                            int end = strValue.IndexOf('>', index);
                            if (end > 0)
                            {
                                int p1 = strValue.IndexOf("src=" + '"', index, StringComparison.OrdinalIgnoreCase);
                                int p2 = -1;
                                if (p1 > 0)
                                {
                                    p1 = p1 + 5;
                                    p2 = strValue.IndexOf('"', p1);
                                }
                                if ((index < p1) && (p1 < p2) && (p2 < end))
                                {
                                    if (!links.Contains(strValue.Substring(p1, p2 - p1)))
                                    {
                                        links.Add(strValue.Substring(p1, p2 - p1));
                                    }
                                }
                            }
                            index = end;
                        }
                    }
                    index++;
                }
                StringBuilder builder = new StringBuilder(strValue);
                foreach (string link in links)
                {
                    string newLink = AveReplaceProcessor.UrlReplace(link, mappings, new ReplaceOption(true), sourceSiteInfo, destSiteUrl);
                    builder.Replace(link, newLink);
                }
                return builder.ToString();
            }
        }
        #endregion
        /// <summary>
        /// HTML语言中忽略大小写，所以link可能是大写，也可能是小写
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="node"></param>
        private static void GetLinkNodes(List<HtmlNode> nodes, HtmlNode node)
        {
            foreach (HtmlNode child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Element)
                {
                    if (child.Name.Equals("a", StringComparison.OrdinalIgnoreCase) || child.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                    {
                        nodes.Add(child);
                    }
                    GetLinkNodes(nodes, child);
                }
            }
        }


        private static string AspContentToXml(string sourceContent, string tagName)
        {
            if (string.IsNullOrEmpty(sourceContent) || string.IsNullOrEmpty(tagName))
            {
                return sourceContent;
            }
            string resultContent = sourceContent;
            string xmlLable = "<" + tagName;
            string aspLable = xmlLable + ":";
            try
            {
                #region trim blank
                resultContent = resultContent.Replace("\r", "");
                resultContent = resultContent.Replace("\n", "");
                resultContent = resultContent.Replace("\t", "");
                #endregion

                int index = 0;
                while (true)
                {
                    int index1 = resultContent.IndexOf(aspLable, index, StringComparison.OrdinalIgnoreCase);
                    if (index1 < 0)
                        break;
                    index = resultContent.IndexOf(" ", index1, StringComparison.OrdinalIgnoreCase);
                    if (index < 0)
                        break;
                    string oldLable = resultContent.Substring(index1, index - index1);
                    resultContent = resultContent.Replace(oldLable, xmlLable);
                }
            }
            catch (Exception ex)//No exception should be thrown here.(Luo Qinglong) 
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCReplaceXmltoContentError, ex);
                return sourceContent;
            }
            return resultContent;
        }

        public static Guid GuidReplace(Guid id, Dictionary<Guid, Guid> mapping)
        {
            if (mapping.ContainsKey(id))
                id = mapping[id];

            return id;
        }

        public static bool IsSpecialUrl(string url)
        {
            //一些不需要做替换的url添加到SpecialUrls中，如前缀是/_layouts/images/的。
            List<string> SpecialUrls = new List<string> { "/_layouts/images/" };
            foreach (string specailUrl in SpecialUrls)
            {
                if (url.StartsWith(specailUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsAbsoluteUrl(string strUrl)
        {
            if (strUrl != null)
            {
                if (strUrl.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                for (int i = 0; i < strUrl.Length; i++)
                {
                    char ch = strUrl[i];
                    if (ch == ':')
                    {
                        return true;
                    }
                    if (((('a' > ch) || (ch > 'z')) && (('A' > ch) || (ch > 'Z'))) && ((('0' > ch) || (ch > '9')) && (((ch != '-') && (ch != '+')) && (ch != '.'))))
                    {
                        return (strUrl.IndexOf("<%", i, StringComparison.OrdinalIgnoreCase) != -1);
                    }
                }
            }
            return false;
        }

        private static string UrlDecode(string s)
        {
            int length = s.Length;
            UrlDecoder decoder = new UrlDecoder(length, Encoding.UTF8);
            for (int i = 0; i < length; i++)
            {
                char ch = s[i];
                if ((ch == '%') && (i < (length - 2)))
                {
                    if ((s[i + 1] == 'u') && (i < (length - 5)))
                    {
                        int num3 = HexToInt(s[i + 2]);
                        int num4 = HexToInt(s[i + 3]);
                        int num5 = HexToInt(s[i + 4]);
                        int num6 = HexToInt(s[i + 5]);
                        if (((num3 < 0) || (num4 < 0)) || ((num5 < 0) || (num6 < 0)))
                        {
                            goto Label_0106;
                        }
                        ch = (char)((((num3 << 12) | (num4 << 8)) | (num5 << 4)) | num6);
                        i += 5;
                        decoder.AddChar(ch);
                        continue;
                    }
                    int num7 = HexToInt(s[i + 1]);
                    int num8 = HexToInt(s[i + 2]);
                    if ((num7 >= 0) && (num8 >= 0))
                    {
                        byte b = (byte)((num7 << 4) | num8);
                        i += 2;
                        decoder.AddByte(b);
                        continue;
                    }
                }
                Label_0106:
                if ((ch & 0xff80) == 0)
                {
                    decoder.AddByte((byte)ch);
                }
                else
                {
                    decoder.AddChar(ch);
                }
            }
            return decoder.GetString();
        }
        public static string UrlDecode(string s, bool isApp)
        {
            if (isApp)
                return HttpUtility.UrlDecode(s);
            return s;
        }
        private static int HexToInt(char h)
        {
            if ((h >= '0') && (h <= '9'))
            {
                return (h - '0');
            }
            if ((h >= 'a') && (h <= 'f'))
            {
                return ((h - 'a') + 10);
            }
            if ((h >= 'A') && (h <= 'F'))
            {
                return ((h - 'A') + 10);
            }
            return -1;
        }

        public static string SqlQueryScriptReplace(string cmdText, bool isExcludeDeleteObject)
        {
            string tempReplaceString = cmdText;
            bool isWebTable = false;
            bool isSiteTable = false;
            try
            {
                string[] replaceStrings = tempReplaceString.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder builder = new StringBuilder();
                foreach (string keyWord in replaceStrings)
                {
                    if (keyWord.Equals("Sites", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.Append("AllSites");
                        isSiteTable = true;
                    }
                    else if (keyWord.Equals("Webs", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.Append("AllWebs");
                        isWebTable = true;
                    }
                    else
                    {
                        builder.Append(keyWord);
                    }
                    builder.Append(' ');
                }
                tempReplaceString = builder.ToString().TrimEnd(' ');
                if (isExcludeDeleteObject && isSiteTable)
                {
                    int index = tempReplaceString.IndexOf("Where", StringComparison.OrdinalIgnoreCase);
                    if (index < 0)
                    {
                        tempReplaceString = tempReplaceString + " Where Deleted = CONVERT(bit, 0)";
                    }
                    else
                    {
                        tempReplaceString = tempReplaceString.Insert(index + 5, " Deleted = CONVERT(bit, 0) And");
                    }

                }
                if (isExcludeDeleteObject && isWebTable)
                {
                    int index = tempReplaceString.IndexOf("Where", StringComparison.OrdinalIgnoreCase);
                    if (index < 0)
                    {
                        tempReplaceString = tempReplaceString + " Where DeleteTransactionId = 0x";
                    }
                    else
                    {
                        tempReplaceString = tempReplaceString.Insert(index + 5, " DeleteTransactionId = 0x And");
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCReplaceSQLCommandError, e.ToString());
                return cmdText;
            }
            return tempReplaceString;
        }
        internal static ICollection<string> excludedUrls { get; set; }
    }

    /// <summary>
    /// Url替换选项
    /// </summary>
    public class ReplaceOption
    {
        /// <summary>
        /// 是否需要替换Url
        /// </summary>
        public bool NeedReplace { get; set; }

        /// <summary>
        /// 是否需要替换绝对Url
        /// </summary>
        public bool NeedReplaceAbsoluteUrl { get; set; }

        /// <summary>
        /// 是否需要替换, 指向同WebApp中其他Site Collection的相对Url
        /// true为不替换:保持相对Url结构，指向目的端WebApp下的Site Collection
        /// false 替换成绝对Url:Url指向源端WebApp下的Site Collection 即实际指向与源端保持一致
        /// </summary>
        public bool KeepExternalRelativeUrl { get; set; }

        public ReplaceOption(bool needReplace)
            : this(needReplace, false)
        { }
        public ReplaceOption(bool needReplace, bool needReplaceAbsoluteUrl)
            : this(needReplace, needReplaceAbsoluteUrl, true)
        { }
        public ReplaceOption(bool needReplace, bool needReplaceAbsoluteUrl, bool keepExternalRelativeUrl)
        {
            this.NeedReplace = needReplace;
            this.NeedReplaceAbsoluteUrl = needReplaceAbsoluteUrl;
            this.KeepExternalRelativeUrl = keepExternalRelativeUrl;
        }

        public ReplaceOption(ReplaceOption other)
            : this(other.NeedReplace, other.NeedReplaceAbsoluteUrl, other.KeepExternalRelativeUrl)
        { }
    }


    internal enum IDScopes
    {
        SiteCollection,
        Site,
        List,
        Folder,
        Item,
        Invalid
    }



    #region For New Url Replace
    public class AveReplaceProcessorV2
    {

        private static AveLogger log = AveLogger.GetInstance(typeof(AveReplaceProcessorV2));
        /// <summary>
        /// 在替换参数时，需要走URL替换逻辑的Key的集合
        /// </summary>
        private static readonly List<string> urlReplaceKeys = new List<string> { "RootFolder", "source", "sourcedoc", "u", "url" };
        /// <summary>
        /// 在替换参数时，需要走Guid替换逻辑的Key的集合
        /// </summary>
        private static readonly List<string> guidReplaceKeys = new List<string> { "Alert", "listid", "list", "view", "RootFolder", "source", "sourcedoc", "u", "instance_id" };
        private readonly string destinationSiteUrl;
        private readonly IDictionary<string, string> mappings;
        private readonly IList<string> prefixes;
        private readonly string sourceSiteUrl;
        //没有用到，暂时先注释掉
        //private string destSiteServerRelativeUrl;
        //private string destWebApplicationUrl;
        private bool needReplaceQuery = true;
        private string sourceSiteServerRelativeUrl;
        private string sourceWebApplicationUrl;


        /// <summary>
        /// </summary>
        /// <param name="mappings">需要替换的url信息，包括相对的和绝对的Url</param>
        /// <param name="sourceSiteUrl">源端SiteCollection的绝对Url</param>
        /// <param name="destinationSiteUrl">目的端SiteColletction的绝对Url</param>
        /// <param name="prefixes">源端Webapp的Prefix信息集合，主要用来判断是否站点时指向同一个Webapplication中不同的SiteCollection，为空则无法处理此情况</param>
        public AveReplaceProcessorV2(IDictionary<string, string> mappings, string sourceSiteUrl, string destinationSiteUrl,
            IList<string> prefixes)
        {
            if (mappings == null) throw new ArgumentNullException("mappings");
            if (string.IsNullOrEmpty(sourceSiteUrl)) throw new ArgumentNullException("sourceSiteUrl");
            if (string.IsNullOrEmpty(destinationSiteUrl)) throw new ArgumentNullException("destinationSiteUrl");
            this.mappings = new SortedDictionary<string, string>(mappings, new UrlComparer());
            this.sourceSiteUrl = sourceSiteUrl;
            this.destinationSiteUrl = destinationSiteUrl;
            this.prefixes = prefixes;
            InitUrlInfo();
        }


        /// <summary>
        /// </summary>
        /// <param name="mappings">需要替换的url信息，包括相对的和绝对的Url</param>
        /// <param name="sourceSiteUrl">源端SiteCollection的绝对Url</param>
        /// <param name="destinationSiteUrl">目的端SiteColletction的绝对Url</param>
        public AveReplaceProcessorV2(IDictionary<string, string> mappings, string sourceSiteUrl, string destinationSiteUrl)
        {
            if (mappings == null) throw new ArgumentNullException("mappings");
            if (string.IsNullOrEmpty(sourceSiteUrl)) throw new ArgumentNullException("sourceSiteUrl");
            if (string.IsNullOrEmpty(destinationSiteUrl)) throw new ArgumentNullException("destinationSiteUrl");
            this.mappings = new SortedDictionary<string, string>(mappings, new UrlComparer());
            this.sourceSiteUrl = sourceSiteUrl;
            this.destinationSiteUrl = destinationSiteUrl;
            InitUrlInfo();
        }

        private void InitUrlInfo()
        {
            int serverPos = sourceSiteUrl.IndexOf('/', "https://".Length);
            if (serverPos == -1)//Root Site Collection Url
            {
                sourceWebApplicationUrl = sourceSiteUrl;
                sourceSiteServerRelativeUrl = "/";
            }
            else
            {
                sourceWebApplicationUrl = sourceSiteUrl.Substring(0, serverPos);
                sourceSiteServerRelativeUrl = sourceSiteUrl.Substring(serverPos);
            }
        }
        private bool MatchGuid(string value)
        {
            var match = Regex.Match(value, @"{\w{8}-\w{4}-\w{4}-\w{4}-\w{12}}", RegexOptions.IgnoreCase);
            return match.Success;
        }
        /// <summary>
        ///     替换Url
        /// </summary>
        /// <param name="oldUrl">需要替换的Url</param>
        /// <param name="option">替换url所用的Option</param>
        /// <returns></returns>
        public string ReplaceUrl(string oldUrl, AveUrlReplaceOption option, bool isWorkBookUrl)
        {
            if (string.IsNullOrEmpty(oldUrl))
            {
                return oldUrl;
            }
            string query = String.Empty;
            string urlWithoutQuery = String.Empty;

            int queryStartPos = oldUrl.IndexOf('?');
            if (queryStartPos >= 0)
            {
                urlWithoutQuery = oldUrl.Substring(0, queryStartPos + 1);
                query = oldUrl.Substring(queryStartPos + 1);
            }
            else
            {
                urlWithoutQuery = oldUrl;
            }
            string decodeTemp = urlWithoutQuery;
            urlWithoutQuery = UrlDecode(urlWithoutQuery);
            bool needEncode = !string.Equals(decodeTemp, urlWithoutQuery, StringComparison.OrdinalIgnoreCase);

            bool isAbsoluteUrl = IsAbsoluteUrl(urlWithoutQuery);
            if (isAbsoluteUrl)
            {
                urlWithoutQuery = NeedReplaceAbsoluteUrl(urlWithoutQuery, option) ? ReplaceAbsoluteUrlInternal(urlWithoutQuery, option, isWorkBookUrl) : urlWithoutQuery;
            }
            else
            {
                urlWithoutQuery = ReplaceRelativeUrlInternal(urlWithoutQuery, option);
            }

            if (needReplaceQuery && !String.IsNullOrEmpty(query))
            {
                query = ReplaceQueryParameters(option, query, queryStartPos);
            }

            if (needEncode)//不能比较替换后的值，应该在前面判断是否被decode 改变过
            {
                urlWithoutQuery = EncodeSpecialChar(urlWithoutQuery);
            }
            return String.Concat(urlWithoutQuery, query);
        }

        /// <summary>
        /// ADO-191675
        /// 由于Url replace前会Decode url，但是decode 后的url如果包含#和%会导致url在浏览器中无法打开，因此需要把这两个特殊字符再Encode
        /// 该问题是针对O365和SP19支持#和% 的特殊修改
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string EncodeSpecialChar(string url)
        {
            return url.Replace("%", "%25").Replace("#", "%23");
        }
        /// <summary>
        /// 需要根据ReplaceAbsoluteUrl与ReplaceExternalUrl两个条件判断是否需要Replace url
        /// </summary>
        /// <param name="oldUrl"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        private bool NeedReplaceAbsoluteUrl(string oldUrl, AveUrlReplaceOption option)
        {
            if (!option.ReplaceAbsoluteUrl)
            {
                return false;
            }
            bool needReplaceAbsoluteUrl = true;
            if (!option.ReplaceExternalUrl)
            {
                //满足该条件时，只有内部Url才会替换URL，External Url不需要替换
                if (oldUrl.StartsWith((sourceWebApplicationUrl + sourceSiteServerRelativeUrl).TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                {
                    //如果源端是root sitecollection，但是Url是指向非root sitecollection时，不需要替换Url
                    if (string.Compare(sourceSiteServerRelativeUrl, "/", StringComparison.Ordinal) == 0)
                    {
                        foreach (string managePath in prefixes)
                        {
                            if (!string.IsNullOrEmpty(managePath))
                            {
                                if (oldUrl.StartsWith(sourceWebApplicationUrl.TrimEnd('/') + "/" + managePath + "/", StringComparison.OrdinalIgnoreCase)
                                    || oldUrl.Equals(sourceWebApplicationUrl.TrimEnd('/') + "/" + managePath, StringComparison.OrdinalIgnoreCase))
                                {
                                    needReplaceAbsoluteUrl = false;
                                }
                            }
                        }
                    }
                }
            }
            //如果不需要 Relace 就不需要ReplaceQuery
            needReplaceQuery = needReplaceAbsoluteUrl;
            return needReplaceAbsoluteUrl;
        }


        private string ReplaceQueryParameters(AveUrlReplaceOption option, string query, int queryStartPos)
        {

            var stringBuilder = new StringBuilder();
            string[] splitStrs = query.Split('&');
            bool needAddSplitChar = false;
            foreach (string keyValue in splitStrs)
            {
                if (needAddSplitChar)
                {
                    stringBuilder.Append("&");
                }
                var index = keyValue.IndexOf('=');
                if (index > 0 && (keyValue.Length > index + 1)) //"RootFolder="
                {
                    string key = keyValue.Substring(0, index);
                    string oldValue = keyValue.Substring(index + 1);
                    bool isEncoded = oldValue.IndexOf('%') >= 0;
                    if (isEncoded)
                    {
                        oldValue = HttpUtility.UrlDecode(oldValue);
                    }
                    var newValue = oldValue;
                    if (urlReplaceKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
                    {
                        newValue = ReplaceQueryUrl(oldValue, option);
                    }
                    if (guidReplaceKeys.Contains(key, StringComparer.OrdinalIgnoreCase) && MatchGuid(oldValue))
                    {
                        newValue = ReplaceGuid(key, oldValue);
                    }
                    if (!string.Equals(oldValue, newValue, StringComparison.OrdinalIgnoreCase))
                    {
                        if (isEncoded)
                        {
                            newValue = HttpUtility.UrlEncode(newValue);
                        }
                        stringBuilder.Append(key);
                        stringBuilder.Append("=");
                        stringBuilder.Append(newValue);
                    }
                    else //没替换用原来的值
                    {
                        stringBuilder.Append(keyValue);
                    }
                }
                else
                {
                    stringBuilder.Append(keyValue);
                }
                needAddSplitChar = true;
            }
            return stringBuilder.ToString();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are case values")]
        /// <summary>
        /// 所有Guid相关的信息都在此方法中进行替换
        /// </summary>
        /// <param name="key">需要替换的key</param>
        /// <param name="guid">需要替换的值</param>
        /// <returns></returns>
        private string ReplaceGuid(string key, string guid)
        {
            Guid mappingValue = Guid.Empty;
            Guid sourceId = new Guid(guid);
            switch (key.ToLower(CultureInfo.InvariantCulture))
            {
                case "alert":
                    WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.GetValueFromAlertIdMapping(sourceId, out mappingValue);
                    break;
                case "rootfolder":
                case "source":
                case "sourcedoc":
                case "u":
                    WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.GetValueFromSiteAssetsFolderUniqueIdMapping(sourceId, out mappingValue);
                    break;
                case "listid":
                case "list":
                    WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.GetValueFromListIdMapping(sourceId, out mappingValue);
                    break;
                case "view":
                    WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.GetViewGuidMappingValue(sourceId, out mappingValue);
                    break;
                case "instance_id":
                    //对于 instance_id 原来逻辑如果能够获取到mapping 则不需要encode，暂时不清楚为什么这么写，因此先去掉，如果有问题再修改回原来逻辑
                    WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AppInstanceIdMapping.TryGetValue(sourceId, out mappingValue);
                    break;
            }
            return mappingValue == Guid.Empty ? guid : mappingValue.ToString("b");
        }

        private string ReplaceQueryUrl(string url, AveUrlReplaceOption option)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }
            // 特殊case，URL中不能存在{，因此传入的url不是合法的URL
            if (url.StartsWith("{", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }
            return IsAbsoluteUrl(url) ?
              ReplaceAbsoluteUrlInternal(url, option) :
              //对于Query中指向同WebApp中其他Site Collection的相对路径，不进行替换，保持相对url结构
              ReplaceRelativeUrlInternal(url, new AveUrlReplaceOption(option) { KeepExternalRelativeUrl = true, });
        }

        public static bool IsAbsoluteUrl(string url)
        {
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        private string ReplaceRelativeUrlInternal(string oldUrl, AveUrlReplaceOption option)
        {
            // 判断是否是指向同一个WebApp其它SiteCollection的url，既不指向原端，也不指向目的端
            // 对于'不指向源端'逻辑的解释：
            // 首先判断oldUrl的prefix与sourceSiteServerRelativeUrl的prefix是否相同。如果不同，则oldUrl一定不指向源端site collection内，满足条件;
            // 如果相同，则继续判断oldUrl是否为sourceSiteServerRelativeUrl的子路径，如果不是，则oldUrl不指向源端site collection内，满足条件。
            if (prefixes != null
            && prefixes.Any(prefix => this.sourceSiteServerRelativeUrl.StartsWith("/" + prefix, StringComparison.OrdinalIgnoreCase) 
            && (!oldUrl.StartsWith("/" + prefix, StringComparison.OrdinalIgnoreCase) || !IsChildDirectory(oldUrl, this.sourceSiteServerRelativeUrl))))
            {
                //如果该Url不指向当前Site也就不需要替换Query里面的参数
                needReplaceQuery = false;
                return option.KeepExternalRelativeUrl || option.ReplaceExternalUrl ? oldUrl : string.Concat(sourceWebApplicationUrl, oldUrl);
            }

            #region 处理相对Url的替换

            foreach (var entry in mappings)
            {
                if (entry.Key.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || entry.Key.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    //处理相对Url跳过绝对Url的Mapping信息即可
                    continue;
                }

                if (oldUrl.StartsWith(entry.Key, StringComparison.OrdinalIgnoreCase)
                    //ADO-160286、ADO-169041、ADO-171444避免替换过的Url再次替换
                    && !(oldUrl.StartsWith(entry.Value, StringComparison.OrdinalIgnoreCase) && entry.Key.Length < entry.Value.Length))
                {
                    var subUrl = oldUrl.Substring(entry.Key.Length);
                    if (String.IsNullOrEmpty(subUrl))
                    {
                        return entry.Value;
                    }
                    #region  该部分Code暂时留着 带功能稳定了再去掉
                    //// ADO-160286 考虑源端是root site的情况，防止当传入的url是已经替换过的url，在mapping（/ -> /sites/test1）的时候替换出来/sites/test1/sites/test1的情况。
                    //if(entry.Key.Equals("/", StringComparison.OrdinalIgnoreCase) && oldUrl.StartsWith(entry.Value, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    return oldUrl;
                    //}
                    //ADO-169041  源端 /sites/ABC  目的端 /sites/ABCDE/sub1  已经替换过一次后，进入方法的old url 为/sites/ABCDE/sub1 这时再匹配，截取后为 DE/sub1，这种情况不需要再次替换了
                    //ADO-180484  此处不仅是为了避免再次替换，而是为了防止所替的Url并不是源端完整对应的url。
                    if (!entry.Key.Equals("/", StringComparison.OrdinalIgnoreCase) && subUrl.Contains('/') && !subUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    #endregion
                    if (!subUrl[0].Equals('.'))//此处需要考虑web rootfolder下跟list同名的文件.
                    {
                        //ADO-153042:要考虑目的端是rootSC的情况
                        return string.Concat(entry.Value.TrimEnd('/'), "/", subUrl.TrimStart('/'));
                    }
                    else
                    {
                        return string.Concat(entry.Value, subUrl);
                    }
                }
            }
            #endregion

            return oldUrl;
        }

        /// <summary>
        /// 检查Url是否为parent的子folder，如/sites/test/folder 为 /sites/test的子folder
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static bool IsChildDirectory(string url, string parent)
        {
            if (url.Length > parent.Length)
            {
                return url.StartsWith(parent, StringComparison.OrdinalIgnoreCase) && url[parent.TrimEnd('/').Length] == '/';
            }
            else if (url.Length == parent.Length)
            {
                return string.Equals(url, parent, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return false;
            }

        }

        private string ReplaceAbsoluteUrlInternal(string oldUrl, AveUrlReplaceOption option, bool isWorkBookUrl = false)
        {
            #region 处理绝对Url的替换

            foreach (var entry in mappings)
            {
                if (!entry.Key.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !entry.Key.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    //处理相对Url跳过相对Url的Mapping信息即可，相对Url信息在字典的后面，因此此时不在需要继续遍历
                    break;
                }
                if (isWorkBookUrl)
                {
                    if (oldUrl.StartsWith(entry.Key + "/", StringComparison.OrdinalIgnoreCase))
                    {
                        return entry.Value + oldUrl.Substring(entry.Key.Length);
                    }
                }
                else if (oldUrl.StartsWith(entry.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Concat(entry.Value, oldUrl.Substring(entry.Key.Length));
                }
            }

            #endregion

            return oldUrl;
        }

        public static string UrlDecode(string s)
        {
            int length = s.Length;
            UrlDecoder decoder = new UrlDecoder(length, Encoding.UTF8);
            for (int i = 0; i < length; i++)
            {
                char ch = s[i];
                if ((ch == '%') && (i < (length - 2)))
                {
                    if ((s[i + 1] == 'u') && (i < (length - 5)))
                    {
                        int num3 = HexToInt(s[i + 2]);
                        int num4 = HexToInt(s[i + 3]);
                        int num5 = HexToInt(s[i + 4]);
                        int num6 = HexToInt(s[i + 5]);
                        if (((num3 < 0) || (num4 < 0)) || ((num5 < 0) || (num6 < 0)))
                        {
                            goto Label_0106;
                        }
                        ch = (char)((((num3 << 12) | (num4 << 8)) | (num5 << 4)) | num6);
                        i += 5;
                        decoder.AddChar(ch);
                        continue;
                    }
                    int num7 = HexToInt(s[i + 1]);
                    int num8 = HexToInt(s[i + 2]);
                    if ((num7 >= 0) && (num8 >= 0))
                    {
                        byte b = (byte)((num7 << 4) | num8);
                        i += 2;
                        decoder.AddByte(b);
                        continue;
                    }
                }
                Label_0106:
                if ((ch & 0xff80) == 0)
                {
                    decoder.AddByte((byte)ch);
                }
                else
                {
                    decoder.AddChar(ch);
                }
            }
            var result = decoder.GetString();
            try
            {
                result = HttpUtility.HtmlDecode(result);
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while decoding url with HtmlDecode. Error:{0}", e);
            }
            return result;
        }

        private static int HexToInt(char h)
        {
            if ((h >= '0') && (h <= '9'))
            {
                return (h - '0');
            }
            if ((h >= 'a') && (h <= 'f'))
            {
                return ((h - 'a') + 10);
            }
            if ((h >= 'A') && (h <= 'F'))
            {
                return ((h - 'A') + 10);
            }
            return -1;
        }
    }

    internal class UrlDecoder
    {
        // Fields
        private int _bufferSize;
        private byte[] _byteBuffer;
        private char[] _charBuffer;
        private Encoding _encoding;
        private int _numBytes;
        private int _numChars;

        // Methods
        internal UrlDecoder(int bufferSize, Encoding encoding)
        {
            this._bufferSize = bufferSize;
            this._encoding = encoding;
            this._charBuffer = new char[bufferSize];
        }

        internal void AddByte(byte b)
        {
            if (this._byteBuffer == null)
            {
                this._byteBuffer = new byte[this._bufferSize];
            }
            this._byteBuffer[this._numBytes++] = b;
        }

        internal void AddChar(char ch)
        {
            if (this._numBytes > 0)
            {
                this.FlushBytes();
            }
            this._charBuffer[this._numChars++] = ch;
        }

        private void FlushBytes()
        {
            if (this._numBytes > 0)
            {
                this._numChars += this._encoding.GetChars(this._byteBuffer, 0, this._numBytes, this._charBuffer, this._numChars);
                this._numBytes = 0;
            }
        }

        internal string GetString()
        {
            if (this._numBytes > 0)
            {
                this.FlushBytes();
            }
            if (this._numChars > 0)
            {
                return new string(this._charBuffer, 0, this._numChars);
            }
            return string.Empty;
        }
    }

    internal class UrlEncoder
    {
        //Methods
        private static int HexToInt(char h)
        {
            if ((h >= '0') && (h <= '9'))
            {
                return (h - '0');
            }
            if ((h >= 'a') && (h <= 'f'))
            {
                return ((h - 'a') + 10);
            }
            if ((h >= 'A') && (h <= 'F'))
            {
                return ((h - 'A') + 10);
            }
            return -1;
        }

        private static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 0x30);
            }
            return (char)((n - 10) + 0x61);
        }

        private static bool IsUrlSafeChar(char ch)
        {
            if (((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z')) || ((ch >= '0') && (ch <= '9')))
            {
                return true;
            }
            switch (ch)
            {
                //should be endcoded
                //case '_':   
                //case '-':  
                //case '.':   
                //case '(':
                //case ')':
                //case '*':
                //case '!':            
                //safe char to added
                case '?':
                case '+':
                case '&':
                case '=':
                    return true;
            }
            return false;
        }

        private static string UrlEncodeSpace(string str)
        {
            if ((str != null) && (str.IndexOf(' ') >= 0))
            {
                str = str.Replace(" ", "%20");
            }
            return str;

        }

        public static string UrlEncode(string value)
        {
            if (value == null)
            { return null; }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(value, Encoding.UTF8));
        }

        private static byte[] UrlEncodeToBytes(string value, Encoding e)
        {
            if (value == null)
            {
                return null;
            }
            byte[] bytes = e.GetBytes(value);
            return UrlEncode(bytes, 0, bytes.Length, false);
        }

        private static byte[] UrlEncode(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue)
        {
            byte[] buffer = UrlEncode(bytes, offset, count);
            if ((alwaysCreateNewReturnValue && (buffer != null)) && (buffer == bytes))
            {
                return (byte[])buffer.Clone();
            }
            return buffer;
        }

        private static byte[] UrlEncode(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];
                if (ch == ' ')
                { num++; }
                else if (!IsUrlSafeChar(ch))
                { num2++; }
            }
            if ((num == 0) && (num2 == 0))
            {
                return bytes;
            }
            byte[] buffer = new byte[count + (num2 * 2)];
            int num4 = 0;
            for (int j = 0; j < count; j++)
            {
                byte num6 = bytes[offset + j];
                char ch2 = (char)num6;
                if (IsUrlSafeChar(ch2))
                {
                    buffer[num4++] = num6;
                }
                else if (ch2 == ' ')
                {
                    buffer[num4++] = 0x2b;
                }
                else
                {
                    buffer[num4++] = 0x25;
                    buffer[num4++] = (byte)IntToHex((num6 >> 4) & 15);
                    buffer[num4++] = (byte)IntToHex(num6 & 15);
                }
            }
            return buffer;
        }

        private static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count)
        {
            if ((bytes == null) && (count == 0))
            { return false; }
            if (bytes == null)
            { throw new ArgumentNullException("bytes"); }
            if ((offset < 0) || (offset > bytes.Length))
            { throw new ArgumentOutOfRangeException("offset"); }
            if ((count < 0) || ((offset + count) > bytes.Length))
            { throw new ArgumentOutOfRangeException("count"); }
            return true;
        }
    }


    public class AveUrlReplaceOption
    {
        public AveUrlReplaceOption()
        { }

        public AveUrlReplaceOption(AveUrlReplaceOption option)
        {
            this.ReplaceAbsoluteUrl = option.ReplaceAbsoluteUrl;
            this.ReplaceExternalUrl = option.ReplaceExternalUrl;
            this.KeepExternalRelativeUrl = option.KeepExternalRelativeUrl;
        }

        /// <summary>
        ///     是否需要替换绝对Url，如http或者https开头的
        /// </summary>
        public bool ReplaceAbsoluteUrl { set; get; }

        /// <summary>
        ///     是否需要替换外部url
        ///     false 绝对url与源端保持一致; 相对url转化为绝对,即实际指向与源端一致
        ///     true 绝对url根据mapping替换; 相对url不变，保持相对结构，即相对目的端
        /// </summary>
        public bool ReplaceExternalUrl { set; get; }

        /// <summary>
        /// 是否需要替换, 指向同WebApp中其他Site Collection的相对Url
        /// true为不替换:保持相对Url结构，指向目的端WebApp下的Site Collection
        /// false 替换成绝对Url:Url指向源端WebApp下的Site Collection 即实际指向与源端保持一致
        /// </summary>
        public bool KeepExternalRelativeUrl { get; set; }
    }


    public class UrlComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return String.CompareOrdinal(y, x);
        }
    }
    #endregion
}
