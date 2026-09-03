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
using System.Xml;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;
using System.Linq;
using System.Security;

namespace AvePoint.Wrapper.Common
{
    public class AveReplaceProcessor
    {
        public static bool keepHeadingUrl = true;
        public static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
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

        public static string UrlReplace(string oldUrl, IEnumerable<Dictionary<string, string>> mappings, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl, bool decodePath = false)
        {
            //ADO-22037,oldurl为空会抛错，导致之前的替换没有更新到content中
            if (string.IsNullOrEmpty(oldUrl))
            {
                return oldUrl;
            }
            oldUrl = decodePath ? UrlPathDecode(oldUrl) : UrlDecode(oldUrl);
            string newUrl = oldUrl;  // to save url after replaced
            if(string.Equals(sourceSiteInfo.WebTemplate, "GROUP#0", StringComparison.OrdinalIgnoreCase) &&　oldUrl.StartsWith("https://outlook.office365.com/owa/?"))
            {
                newUrl = oldUrl.Replace(sourceSiteInfo.Url.Substring(sourceSiteInfo.Url.LastIndexOf('/') + 1), destSiteUrl.Substring(destSiteUrl.LastIndexOf('/') + 1));
                return newUrl;
            }
            //HttpUtility.UrlDecode(oldUrl)会将Url中的'+'变成' '
            //newUrl = HttpUtility.UrlDecode(oldUrl);
            bool isAbsoluteUrl = IsAbsoluteUrl(oldUrl);
            string hostHeader = GetHostHeader(sourceSiteInfo.Url);
            if (!option.NeedReplaceAbsoluteUrl && isAbsoluteUrl)
            {
                return oldUrl;
            }
            if (isAbsoluteUrl && !option.NeedReplaceHostHeader)
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
                                    return oldUrl;
                                }
                            }
                        }
                    }
                }
                else
                {
                    return oldUrl;
                }
            }
            if (!option.IsNeedReplace || IsSpecialUrl(newUrl))
            {
                return newUrl;
            }
            if (oldUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase) && !isAbsoluteUrl)
            {
                //if (oldUrl.StartsWith("/_layouts", StringComparison.OrdinalIgnoreCase))
                //{
                //    return oldUrl;
                //}
                List<string> Prefixes = new List<string>() { "sites", "teams", "personal", "portals" };
                foreach (string managePath in Prefixes)
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
                                if (keepHeadingUrl)
                                {	//保留headdng的相对Url结构
                                    return oldUrl;
                                }
                                return hostHeader + oldUrl;
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

            List<int> splitIndexs = new List<int>();//避免产生大量String临时对象
            int index = 0;
            if (isAbsoluteUrl)
            {
                splitIndexs.Add(0);
                index = newUrl.IndexOf("/sites/", StringComparison.OrdinalIgnoreCase);
                if (index >= 0 && !option.NeedReplaceHostHeader)
                {
                    index += 7;
                }
                else
                {
                    index = newUrl.IndexOf("//", StringComparison.OrdinalIgnoreCase) + 2;
                }
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
                        if (option.NeedReplaceHostHeader && mapping.ContainsKey(key + "/"))
                        {
                            return string.Concat(newUrl.Substring(0, splitIndexs[0]), mapping[key + "/"].TrimEnd('/'), newUrl.Substring(splitIndexs[j]));
                        }
                        if (mapping.ContainsKey(key))
                        {
                            if (mapping[key].Equals(key, StringComparison.OrdinalIgnoreCase))
                            {
                                // don't need to replace url if source and destination's url are the same.
                            }
                            else if (mapping[key].Equals("/"))//目的端是Root，需要处理/sites/a/Tasks--->//Tasks的情况
                            {
                                if (newUrl.Equals(key))//Url是/sites/a,需要替换为/而不是空
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
                            return newUrl;
                        }
                    }
                }
                if (i == 0 && newUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase) && !appedSlashByUs)//处理源端是Root的情况
                {
                    foreach (Dictionary<string, string> mapping in mappings)
                    {
                        if (mapping.ContainsKey("/"))
                        {
                            if (mapping["/"] != "/" && !newUrl.StartsWith(mapping["/"], StringComparison.OrdinalIgnoreCase))//[DOC-69984] root 到 非 root 有些url已经加了一次 目的端的url 不应该再加一次 
                            {
                                newUrl = mapping["/"] + newUrl;
                            }
                            return newUrl;
                        }
                    }
                }
            }
            if (newUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase) && appedSlashByUs)
            {
                newUrl = newUrl.Remove(0, 1);
            }
            return newUrl;
        }

        public static string UrlPathDecode(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            int queryParamerterIndex = s.IndexOf('?');
            string path = null;
            if (queryParamerterIndex > 0)
            {
                path = s.Substring(0, queryParamerterIndex);
            }
            else
            {
                path = s;
            }
            int length = path.Length;
            UrlDecoder decoder = new UrlDecoder(length, Encoding.UTF8);
            for (int i = 0; i < length; i++)
            {
                char ch = path[i];
                if ((ch == '%') && (i < (length - 2)))
                {
                    if ((path[i + 1] == 'u') && (i < (length - 5)))
                    {
                        int num3 = HexToInt(path[i + 2]);
                        int num4 = HexToInt(path[i + 3]);
                        int num5 = HexToInt(path[i + 4]);
                        int num6 = HexToInt(path[i + 5]);
                        if (((num3 < 0) || (num4 < 0)) || ((num5 < 0) || (num6 < 0)))
                        {
                            goto Label_0106;
                        }
                        ch = (char)((((num3 << 12) | (num4 << 8)) | (num5 << 4)) | num6);
                        i += 5;
                        decoder.AddChar(ch);
                        continue;
                    }
                    int num7 = HexToInt(path[i + 1]);
                    int num8 = HexToInt(path[i + 2]);
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
            return queryParamerterIndex >= 0 ? decoder.GetString() + s.Substring(queryParamerterIndex) : decoder.GetString();
        }



        /// <summary>
        /// 代替IdReplace方法，替换URL中的后缀， ？url= , ?list= ,?sourcedoc=
        /// </summary>
        /// <param name="oldUrl"></param>
        /// <param name="siteMappingManager"></param>
        /// <param name="sourceSiteInfo"></param>
        /// <param name="destSiteUrl"></param>
        /// <param name="needReplaceLast"></param>
        /// <returns></returns>
        public static string SuffixReplace(string oldUrl, AveSiteMappingManager siteMappingManager, AveSiteInfo sourceSiteInfo, string destSiteUrl, ref bool needReplaceLast)
        {
            try
            {
                Dictionary<string, string> suffixDic = new Dictionary<string, string>();
                string tempUrl = oldUrl.Substring(oldUrl.LastIndexOf('?') + 1);
                if (string.IsNullOrEmpty(tempUrl))
                {
                    return oldUrl;
                }
                string suffixUrl = oldUrl.Substring(oldUrl.LastIndexOf('?') + 1);
                string[] suffixs = suffixUrl.Split('&');
                foreach (string id in suffixs)
                {
                    string[] kv = id.Split('=');
                    if (kv.Length == 2)
                    {
                        suffixDic.Add(kv[0], kv[1]);
                    }
                }
                foreach (KeyValuePair<string, string> kvp in suffixDic)
                {
                    try
                    {
                        if (string.Equals(kvp.Key, "list", StringComparison.OrdinalIgnoreCase))
                        {
                            Guid id = new Guid(kvp.Value);
                            Guid destId = siteMappingManager.GetListIdMapping(id);
                            if (destId != Guid.Empty)
                            {
                                //Guid类型ToString是小写，idUrl中大小写不一定，做一个特殊处理
                                int index = suffixUrl.IndexOf(id.ToString(), StringComparison.OrdinalIgnoreCase);
                                string sourceId = suffixUrl.Substring(index, id.ToString().Length);
                                suffixUrl = suffixUrl.Replace(sourceId, destId.ToString());
                            }
                            else
                            {
                                needReplaceLast = true;
                                return oldUrl;
                            }
                        }
                        else if (string.Equals(kvp.Key, "sourcedoc", StringComparison.OrdinalIgnoreCase)) //[SAAS-12613],[SAAS-10941]
                        {
                            Guid id = new Guid(kvp.Value);
                            Guid destId = siteMappingManager.GetDocumentUniqueIdMapping(id);
                            if (destId != Guid.Empty)
                            {
                                //Guid类型ToString是小写，idUrl中大小写不一定，做一个特殊处理
                                int index = suffixUrl.IndexOf(id.ToString(), StringComparison.OrdinalIgnoreCase);
                                string sourceId = suffixUrl.Substring(index, id.ToString().Length);
                                suffixUrl = suffixUrl.Replace(sourceId, destId.ToString());
                            }
                            else
                            {
                                needReplaceLast = true;
                                return oldUrl;
                            }
                        }
                        else if (string.Equals(kvp.Key, "url", StringComparison.OrdinalIgnoreCase)) //replace url
                        {
                            tempUrl = kvp.Value;
                            suffixUrl = UrlReplace(tempUrl, siteMappingManager.SiteManagedMappings,
                                                             new ReplaceOption(true, true), sourceSiteInfo, destSiteUrl);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetIdFailed, e);
                    }
                }
                return oldUrl.Replace(tempUrl, suffixUrl);
            }
            catch (Exception ex)
            {
                log.Warn("Replace Id Error. Message:" + ex.ToString());
            }
            return oldUrl;
        }

        #region
        public static string IdReplace(string oldUrl, AveSiteMappingManager siteMappingManager,  ref bool needReplaceLast)
        {
            try
            {
                Dictionary<string, string> idDic = new Dictionary<string, string>();
                string tempUrl = oldUrl.Substring(oldUrl.LastIndexOf('?') + 1);
                if (string.IsNullOrEmpty(tempUrl))
                {
                    return oldUrl;
                }
                string idUrl = oldUrl.Substring(oldUrl.LastIndexOf('?') + 1);
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
                        if (kvp.Key.Equals("list", StringComparison.OrdinalIgnoreCase))
                        {
                            Guid id = new Guid(kvp.Value);
                            Guid destId = siteMappingManager.GetListIdMapping(id);
                            if (destId !=Guid.Empty)
                            {
                                //Guid类型ToString是小写，idUrl中大小写不一定，做一个特殊处理
                                int index = idUrl.IndexOf(id.ToString(), StringComparison.OrdinalIgnoreCase);
                                string sourceId = idUrl.Substring(index, id.ToString().Length);
                                idUrl = idUrl.Replace(sourceId,  destId.ToString());
                            }
                            else
                            {
                                needReplaceLast = true;
                                return oldUrl;
                            }
                        }
                        else if (kvp.Key.Equals("sourcedoc", StringComparison.OrdinalIgnoreCase))
                        {
                            Guid id = new Guid(kvp.Value);
                            Guid destId = siteMappingManager.GetDocumentUniqueIdMapping(id);
                            if (destId != Guid.Empty)
                            {
                                //Guid类型ToString是小写，idUrl中大小写不一定，做一个特殊处理
                                int index = idUrl.IndexOf(id.ToString(), StringComparison.OrdinalIgnoreCase);
                                string sourceId = idUrl.Substring(index, id.ToString().Length);
                                idUrl = idUrl.Replace(sourceId, destId.ToString());
                            }
                            else
                            {
                                needReplaceLast = true;
                                return oldUrl;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetIdFailed, e);
                    }
                }
                return oldUrl.Replace(tempUrl, idUrl);
            }
            catch (Exception ex)
            {
                log.Warn("Replace Id Error. Message:" + ex.ToString());
            }
            return oldUrl;
        }
        #endregion

        public static string UrlReplace(string oldUrl, Dictionary<string, string> mapping, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            Dictionary<string, string>[] tmpMapping = new Dictionary<string, string>[] { mapping };
            return UrlReplace(oldUrl, tmpMapping, option, sourceSiteInfo, destSiteUrl);
        }

        public static string UrlReplace(string oldUrl, Dictionary<string, string> mapping, Dictionary<string, string> fullUrlMapping, ReplaceOption option, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            Dictionary<string, string>[] tmpMapping = new Dictionary<string, string>[] { mapping, fullUrlMapping };
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
                builder.Replace(link, newLink);
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
                }
                index++;
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
            List<string> SpecialUrls = new List<string> { "/_layouts/images/", "/_layouts/15/images/" };
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
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCReplaceSQLCommandError, e.ToString());
                return cmdText;
            }
            return tempReplaceString;
        }

        private class UrlDecoder
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

    }

    public class ReplaceOption
    {
        private bool mIsNeedReplace;
        private bool mNeedReplaceAbsoluteUrl = false;
        private bool mNeedReplaceHostHeader = false;

        public bool IsNeedReplace
        {
            get { return mIsNeedReplace; }
            set { mIsNeedReplace = value; }
        }

        public bool NeedReplaceAbsoluteUrl
        {
            get { return mNeedReplaceAbsoluteUrl; }
            set { mNeedReplaceAbsoluteUrl = value; }
        }

        public bool NeedReplaceHostHeader
        {
            get { return mNeedReplaceHostHeader; }
            set { mNeedReplaceHostHeader = value; }
        }

        public ReplaceOption(bool needReplace)
        {
            mIsNeedReplace = needReplace;
        }
        public ReplaceOption(bool needReplace, bool needReplaceAbsoluteUrl)
        {
            mIsNeedReplace = needReplace;
            mNeedReplaceAbsoluteUrl = needReplaceAbsoluteUrl;
            mNeedReplaceHostHeader = WrapperConfiguration.WrapperConfigurationForBPOS.UpdateSpecificLinks;
        }
        public ReplaceOption(bool needReplace, bool needReplaceAbsoluteUrl,bool needReplaceHostHeader)
        {
            mIsNeedReplace = needReplace;
            mNeedReplaceAbsoluteUrl = needReplaceAbsoluteUrl;
            mNeedReplaceHostHeader = needReplaceHostHeader;
        }
    }

}
