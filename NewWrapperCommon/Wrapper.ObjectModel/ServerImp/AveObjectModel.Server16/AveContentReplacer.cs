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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server16
{
    class AveSPDocContentReplacer
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveSPDocContentReplacer));

        private AveSite mSite;
        private SPFile mFile;
        private int mVersion;
        AveDocumentInfo mDocumentInfo;
        Stream mStream;
        
        //For 13 Post Action to change content
        public AveSPDocContentReplacer(AveSite site, SPFile file, AveDocumentInfo info)
        {
            mSite = site;
            mFile = file;
            mVersion = info.OriginalVersion;
            mDocumentInfo = info;

            if (info == null)
            {
                throw new ArgumentNullException("documentInfo");
            }
        }

        public AveSPDocContentReplacer(AveSite site, Stream stream, AveDocumentInfo info)
        {
            mSite = site;
            mFile = null;
            mVersion = info.OriginalVersion;
            mDocumentInfo = info;
            mStream = stream;

            if (mStream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (info == null)
            {
                throw new ArgumentNullException("documentInfo");
            }
        }

        private string GetAttribute(string content, int index, string tag)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPDocContentReplacer.GetAttribute"))
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

        }

        public string GetInnerText(string content, int index, string tag)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPDocContentReplacer.GetInnerText"))
            {

                int newIndex = index;
                string startTag = "<" + tag + ">";
                index = content.IndexOf(startTag, index, StringComparison.OrdinalIgnoreCase);
                string newContent = "";
                if (index == -1)
                {
                    startTag = "<" + tag + " ";
                    index = content.IndexOf(startTag, newIndex, StringComparison.OrdinalIgnoreCase);
                    if (index == -1)
                    {
                        return null;
                    }
                    index = content.IndexOf(">", index, StringComparison.OrdinalIgnoreCase);
                    newContent = content.Substring(index + 1);
                }
                else
                {
                    newContent = content.Substring(index + startTag.Length);
                    index += (tag.Length + 1);
                }
                int endIndex = index;
                startTag = "</" + tag + ">";
                endIndex = content.IndexOf(startTag, index, StringComparison.OrdinalIgnoreCase);
                newContent = newContent.Substring(0, endIndex - index - 1);
                return newContent;

            }

        }

        private string ReplaceString(string content, int index, string oldValue, string newValue)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPDocContentReplacer.ReplaceString"))
            {

                if ((index = content.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    content = content.Substring(0, index) + newValue + content.Substring(index + oldValue.Length);
                }
                return content;

            }

        }

        /// <summary>
        /// 替换attribute的Url值
        /// </summary>
        /// <param name="content"></param>
        /// <param name="tag"></param>
        /// <param name="attribute"></param>
        /// <param name="hasChanged"></param>
        /// <param name="oneTime">是否只替换第一个</param>
        /// <returns></returns>
        private string ReplaceUrlContent(string content, string tag, string attribute, out bool hasChanged, bool oneTime)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPDocContentReplacer.ReplaceUrlContent"))
            {

                hasChanged = false;
                int index = 0;

                if (oneTime)
                {
                    if ((index = content.IndexOf(tag, index, StringComparison.OrdinalIgnoreCase)) != -1)
                    {
                        int innerTextIndex = index;
                        content = ReplaceUrlContent(content, attribute, ref index, ref hasChanged);//change index,out index -> ref index
                        if (index != -1 && index < content.Length)
                        {
                            content = ReplaceUrlContent(content, ">", ref index, ref hasChanged);
                        }
                    }
                }
                else
                {
                    while (index != -1 && (index = content.IndexOf(tag, index, StringComparison.OrdinalIgnoreCase)) != -1)
                    {
                        index += tag.Length;
                        content = ReplaceUrlContent(content, attribute, ref index, ref hasChanged);//changge index,out index -> ref index
                        if (index != -1 && index < content.Length)//ADO-85996 替换inner text
                        {
                            content = ReplaceUrlContent(content, ">", ref index, ref hasChanged);
                        }
                        if (index >= content.Length)
                        {
                            break;
                        }
                    }
                }
                return content;

            }

        }

        [SuppressMessage("CheckHardCode", "Z100009:CheckString", Justification = "")]
        private string ReplaceUrlContent(string content, string attribute, ref int newIndex, ref bool hasChanged)// change int index out int new index
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPDocContentReplacer.ReplaceUrlContent_1"))
            {

                newIndex = content.IndexOf(attribute, newIndex, StringComparison.OrdinalIgnoreCase);// change s index ->new index
                if (newIndex != -1)
                {
                    string url = GetAttribute(content, newIndex, attribute);
                    if (!String.IsNullOrEmpty(url) && !url.StartsWith("/_layouts", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("../", StringComparison.OrdinalIgnoreCase))
                    {
                        string newUrl = AveReplaceProcessor.UrlReplace(url, mDocumentInfo.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mDocumentInfo.MappingManager.SiteMappingManager.SourceSiteInfo, mDocumentInfo.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                        if (!url.Equals(newUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            content = ReplaceString(content, newIndex, url, newUrl);//index->newIndex
                            hasChanged = true;
                            newIndex += newUrl.Length;
                        }
                    }
                    newIndex += attribute.Length;
                }
                return content;

            }

        }

        /// <summary>
        /// 替换所有需要替换的attribute
        /// </summary>
        /// <param name="content"></param>
        /// <param name="tag"></param>
        /// <param name="attribute"></param>
        /// <param name="hasChanged"></param>
        /// <returns></returns>
        private string ReplaceUrlContent(string content, string tag, string attribute, out bool hasChanged)
        {
            return ReplaceUrlContent(content, tag, attribute, out hasChanged, false);
        }

        private string ReplaceUrlContent(string content, out bool hasChanged)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPDocContentReplacer.ReplaceUrlContent_2"))
            {

                bool changed = false;
                content = ReplaceUrlContent(content, "<img ", "src=", out hasChanged);
                changed = changed || hasChanged;
                content = ReplaceUrlContent(content, "<a ", "href=", out hasChanged);
                changed = changed || hasChanged;
                content = ReplaceUrlContent(content, "<?mso-infoPathSolution ", "href=", out hasChanged);
                changed = changed || hasChanged;
                content = ReplaceUrlContent(content, "<%@ Page ", "MasterPageFile=", out hasChanged);
                changed = changed || hasChanged;
                content = ReplaceUrlContent(content, "<SharePoint:SiteLogoImage", "LogoImageUrl=", out hasChanged);
                changed = changed || hasChanged;
                content = ReplaceUrlContent(content, "<SharePoint:SPLinkButton", "NavigateUrl=", out hasChanged);
                changed = changed || hasChanged;
                content = ReplacePageLayout(content, out hasChanged);
                changed = changed || hasChanged;
                //替换如<td onclick="window.location.href='/home/About us';">About Us</td>中的url
                content = ReplaceUrlContent(content, "<td", "window.location.href=", out hasChanged);
                changed = changed || hasChanged;
                if (changed)
                {
                    hasChanged = true;
                }
                return content;

            }

        }
        /// <summary>
        /// 发现pagelayout不替换，即使把column设置正确但systemupdate后仍然是源端的，所以暂时将content的pagelayout替换掉，有可能是对象与页面不同步有关，但是reload仍然不好使。如果可能的话可以不使用此方法
        /// </summary>
        /// <param name="content"></param>
        /// <param name="hasChanged"></param>
        /// <returns></returns>
        private string ReplacePageLayout(string content, out bool hasChanged)
        {
            hasChanged = false;
            string result = content;
            string oldPageLayoutUrl = GetInnerText(content, 0, "mso:PublishingPageLayout");
            AveSiteMappingManager siteMappingManager = mDocumentInfo.MappingManager.SiteMappingManager;
            string newpageLayoutUrl = AveReplaceProcessor.UrlReplace(oldPageLayoutUrl, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
            if (string.IsNullOrEmpty(newpageLayoutUrl))
            {
                return content;
            }
            else if (!newpageLayoutUrl.Equals(oldPageLayoutUrl, StringComparison.OrdinalIgnoreCase))
            {
                result = result.Replace(oldPageLayoutUrl, newpageLayoutUrl);
                hasChanged = true;
            }
            return result;
        }
        private string ReplaceCSSContent(string content, out bool hasChanged)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPDocContentReplacer.ReplaceCSSContent"))
            {

                bool tempChanged = false;
                hasChanged = false;
                int index = 0;
                while ((index = content.IndexOf("url", index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    content = ReplaceCSSUrlContent(content, "(", index, out index, out tempChanged);
                    if (tempChanged)
                    {
                        hasChanged = tempChanged;
                    }
                    if (index == -1 || index >= content.Length)
                    {
                        break;
                    }
                }
                return content;

            }

        }

        private string ReplaceCSSUrlContent(string content, string attribute, int index, out int newIndex, out bool hasChanged)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPDocContentReplacer.ReplaceCSSUrlContent"))
            {

                hasChanged = false;
                newIndex = content.IndexOf(attribute, index, StringComparison.OrdinalIgnoreCase);
                if (newIndex != -1)
                {
                    string url = GetAttribute(content, newIndex, attribute);
                    string newUrl = url;
                    //string sourceWebFullUrl = mDocumentInfo.SourceWebUrl.TrimEnd('/');
                    AveSiteMappingManager siteMappingManager = mDocumentInfo.MappingManager.SiteMappingManager;
                    if (!String.IsNullOrEmpty(newUrl) && !newUrl.StartsWith("/_layouts", StringComparison.OrdinalIgnoreCase))
                    {
                        #region ADO-62052
                        /***
                         * 以"../"开头的URL替换逻辑
                         * 该逻辑下只判定了"../"是缩减web级别的层次结构而并没有考虑"../"还需要解决缩减list和folder级别的层次结构
                         * 以下逻辑为通过MAPPING寻找LIST的URL以便进行替换，无法做到LIST下FOLDER层次的替换，因为做MAPPING的时候没有做到FOLDER级别
                         * */
                        //if (newUrl.StartsWith("../", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    int flagNum = 0;
                        //    while (newUrl.IndexOf("../", StringComparison.OrdinalIgnoreCase) >= 0)
                        //    {
                        //        newUrl = newUrl.Remove(0, 3);
                        //        flagNum++;
                        //    }
                        //    newUrl = newUrl.TrimStart('/');
                        //    while (flagNum != 0)
                        //    {
                        //        flagNum--;
                        //        sourceWebFullUrl = sourceWebFullUrl.Substring(0, sourceWebFullUrl.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
                        //    }
                        //    newUrl = sourceWebFullUrl.TrimEnd('/') + "/" + newUrl;
                        //}
                        #endregion
                        newUrl = AveReplaceProcessor.UrlReplace(newUrl, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);                        
                    }
                    if (!string.IsNullOrEmpty(url) && !url.Equals(newUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        content = ReplaceString(content, newIndex, url, newUrl);
                        hasChanged = true;
                        newIndex += newUrl.Length;
                    }
                    newIndex += attribute.Length;
                }
                return content;

            }

        }


        //List Post Action: mDocumentInfo.AveItem is null
        private void AddPostAction(Guid listId)
        {
            Guid webId = Guid.Empty;
            if (mDocumentInfo.AveItem != null)
            {
                webId = mDocumentInfo.AveItem.Web.ID;
            }
            else if (mFile != null)
            {
                webId = mFile.ParentFolder.ParentWeb.ID;
            }
            else
            {
                logger.Warn("Cannot get web id in content replace post action.");
            }
            mDocumentInfo.MappingManager.SiteMappingManager.AddUnupdateFileCache(mDocumentInfo.AveItem.Web.ID, listId, mDocumentInfo.ServerRelativeUrl, mVersion);
        }

        public Stream ReplaceWebPartContent()
        {
            bool changed = false;
            return ReplaceWebPartContent(out changed);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used in sharepoint web part content")]
        public Stream ReplaceWebPartContent(out bool changed)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPDocContentReplacer.ReplaceWebPartContent"))
            {

                changed = false;
                try
                {
                    //需要添加对ContentType的判断，这样更加准确
                    if (!mDocumentInfo.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)
                        && !mDocumentInfo.Name.EndsWith(".master", StringComparison.OrdinalIgnoreCase)
                        && !mDocumentInfo.Name.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                        && !mDocumentInfo.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                        )
                    {
                        return mStream;
                    }
                    if (mDocumentInfo.Name.Equals("Nintex_AutoStartRules.xml", StringComparison.OrdinalIgnoreCase)
                        || (mDocumentInfo != null && mDocumentInfo.IsLinkFile))
                    {
                        return mStream;
                    }
                    byte[] content = null;
                    bool realStream = false;
                    if (mFile != null)
                    {
                        if (mDocumentInfo.Version == mVersion)
                        {
                            content = mFile.OpenBinary(SPOpenBinaryOptions.SkipVirusScan);
                        }
                        else
                        {
                            SPFileVersion fileVersion = mFile.Versions.GetVersionFromID(mVersion);
                            content = fileVersion.OpenBinary();
                        }
                    }
                    else
                    {
                        //Use stream to replace the URL before adding file
                        content = new byte[mStream.Length];
                        mStream.Read(content, 0, (int)mStream.Length);
                        realStream = true;
                    }
                    string contentStr = string.Empty;
                    if (content[0] == 255 && content[1] == 254)
                    {//Unicode
                        contentStr = Encoding.Unicode.GetString(content);
                    }
                    else
                    {//UTF8
                        contentStr = Encoding.UTF8.GetString(content);
                    }

                    bool fileChanged = false;
                    WebPartType type; 
                    string webPartName = "";
                    int index = 0;
                    Guid listId = Guid.Empty;
                    string listName = string.Empty;
                    string arrtibuteName = string.Empty;
                    string url = string.Empty;
                    string newUrl = string.Empty;
                    int tempIndex = -1;
                    AveSiteMappingManager siteMappingManager = mDocumentInfo.MappingManager.SiteMappingManager;
                    if (mDocumentInfo.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)
                       || mDocumentInfo.Name.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
                    {
                        #region replace WebPart info
                        while ((type = GetWebPartType(contentStr, index, ref webPartName, ref index)) != WebPartType.None)
                        {
                            string webPartStart = "<" + webPartName + " ";
                            string webPartEnd = "</" + webPartName + ">";
                            int endIndex = contentStr.IndexOf(webPartEnd, index, StringComparison.OrdinalIgnoreCase);

                            switch (type)
                            {
                                case WebPartType.XsltListView:
                                    //Replace List Name
                                    arrtibuteName = " ListName=";
                                    listName = GetAttribute(contentStr, index, arrtibuteName);
                                    if (!String.IsNullOrEmpty(listName))
                                    {
                                        listId = new Guid(listName);
                                        var value = Guid.Empty;
                                        if (mDocumentInfo.MappingManager.SiteMappingManager.GetValueFromListIdMapping(listId, out value))
                                        {
                                            contentStr = ReplaceString(contentStr, index, listName, value.ToString("B").ToUpper(CultureInfo.InvariantCulture));
                                            fileChanged = true;
                                        }
                                        else
                                        {
                                            AddPostAction(listId);
                                            index = endIndex;
                                            continue;
                                        }
                                    }

                                    tempIndex = contentStr.IndexOf("<View ", index, StringComparison.OrdinalIgnoreCase);
                                    arrtibuteName = " Url=";
                                    tempIndex = contentStr.IndexOf(arrtibuteName, index, StringComparison.OrdinalIgnoreCase);
                                    if (tempIndex != -1)
                                    {
                                        url = GetAttribute(contentStr, tempIndex, arrtibuteName);
                                        if (!String.IsNullOrEmpty(url))
                                        {
                                            newUrl = AveReplaceProcessor.UrlReplace(url, mDocumentInfo.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mDocumentInfo.MappingManager.SiteMappingManager.SourceSiteInfo, mDocumentInfo.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                                            if (!url.Equals(newUrl, StringComparison.OrdinalIgnoreCase))
                                            {
                                                contentStr = ReplaceString(contentStr, tempIndex, url, newUrl);
                                                fileChanged = true;
                                            }
                                        }
                                    }
                                    break;
                                case WebPartType.ListView:
                                    //Replace List Name
                                    string tagName = "ListName";
                                    listName = GetInnerText(contentStr, index, tagName);
                                    if (!String.IsNullOrEmpty(listName))
                                    {
                                        listId = new Guid(listName);
                                        var value = Guid.Empty;
                                        if (mDocumentInfo.MappingManager.SiteMappingManager.GetValueFromListIdMapping(listId, out value))
                                        {
                                            contentStr = ReplaceString(contentStr, index, listName, value.ToString("B").ToUpper(CultureInfo.InvariantCulture));
                                            fileChanged = true;
                                        }
                                        else
                                        {
                                            AddPostAction(listId);
                                            index = endIndex;
                                            continue;
                                        }
                                    }

                                    //Replace Web Id
                                    tagName = "WebId";
                                    string web = GetInnerText(contentStr, index, tagName);
                                    if (!String.IsNullOrEmpty(listName))
                                    {
                                        Guid webId = new Guid(web);
                                        if (webId != Guid.Empty)
                                        {
                                            if (mDocumentInfo.MappingManager.SiteMappingManager.WebIDMapping.ContainsKey(webId))
                                            {
                                                contentStr = ReplaceString(contentStr, web, mDocumentInfo.MappingManager.SiteMappingManager.WebIDMapping[webId].ToString().ToUpper(CultureInfo.InvariantCulture), index);
                                                fileChanged = true;
                                            }
                                        }
                                    }

                                    //Replace Detail Link
                                    tagName = "DetailLink";
                                    url = GetInnerText(contentStr, index, tagName);
                                    tempIndex = contentStr.IndexOf("<DetailLink", index, StringComparison.OrdinalIgnoreCase);
                                    if (!String.IsNullOrEmpty(url))
                                    {
                                        newUrl = AveReplaceProcessor.UrlReplace(url, mDocumentInfo.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mDocumentInfo.MappingManager.SiteMappingManager.SourceSiteInfo, mDocumentInfo.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                                        if (!url.Equals(newUrl, StringComparison.OrdinalIgnoreCase))
                                        {
                                            contentStr = ReplaceString(contentStr, tempIndex, url, newUrl);
                                            fileChanged = true;
                                        }
                                    }

                                    //Replace ListView Url
                                    tagName = "ListViewXml";
                                    string listViewXml = GetInnerText(contentStr, index, tagName);
                                    tempIndex = contentStr.IndexOf("<ListViewXml", index, StringComparison.OrdinalIgnoreCase);
                                    try
                                    {
                                        XmlDocument doc = new XmlDocument();
                                        doc.LoadXml("<A>" + listViewXml + "</A>");
                                        string listView = doc.DocumentElement.InnerText;
                                        doc.LoadXml(listView);
                                        url = doc.DocumentElement.GetAttribute("Url");
                                        if (!String.IsNullOrEmpty(url))
                                        {
                                            newUrl = AveReplaceProcessor.UrlReplace(url, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                                            if (!url.Equals(newUrl, StringComparison.OrdinalIgnoreCase))
                                            {
                                                contentStr = ReplaceString(contentStr, tempIndex, url, newUrl);
                                                fileChanged = true;
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.SetXmlAttError, e.ToString());
                                    }
                                    break;
                                case WebPartType.Image:
                                    arrtibuteName = " ImageLink=";
                                    tempIndex = contentStr.IndexOf(arrtibuteName, index, StringComparison.OrdinalIgnoreCase);
                                    if (tempIndex != -1)
                                    {
                                        url = GetAttribute(contentStr, tempIndex, arrtibuteName);
                                        if (!String.IsNullOrEmpty(url))
                                        {
                                            newUrl = AveReplaceProcessor.UrlReplace(url, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                                            if (!url.Equals(newUrl, StringComparison.OrdinalIgnoreCase))
                                            {
                                                contentStr = ReplaceString(contentStr, tempIndex, url, newUrl);
                                                fileChanged = true;
                                            }
                                        }
                                    }
                                    break;
                                case WebPartType.DataForm:
                                    //Replace List Name
                                    arrtibuteName = " ListName=";
                                    listName = GetAttribute(contentStr, index, arrtibuteName);
                                    if (!String.IsNullOrEmpty(listName))
                                    {
                                        listId = new Guid(listName);
                                        var value = Guid.Empty;
                                        if (mDocumentInfo.MappingManager.SiteMappingManager.GetValueFromListIdMapping(listId, out value))
                                        {
                                            contentStr = ReplaceString(contentStr, listName, value.ToString("B").ToUpper(CultureInfo.InvariantCulture));
                                            fileChanged = true;
                                        }
                                        else
                                        {
                                            AddPostAction(listId);
                                            index = endIndex;
                                            continue;
                                        }
                                    }
                                    Dictionary<string, List<string>> bindingAndDataSourceDic = GetBindingAndDataSourceDic(contentStr, index);
                                    if (bindingAndDataSourceDic != null && bindingAndDataSourceDic.Count > 0)
                                    {
                                        if (bindingAndDataSourceDic.ContainsKey("ListID"))
                                        {
                                            foreach (string temp in bindingAndDataSourceDic["ListID"])
                                            {
                                                try
                                                {
                                                    listId = new Guid(temp);
                                                }
                                                catch (Exception e)
                                                {
                                                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetListIdError, e.ToString());
                                                    index = endIndex;
                                                    continue;
                                                }
                                                var value = Guid.Empty;
                                                if (mDocumentInfo.MappingManager.SiteMappingManager.GetValueFromListIdMapping(listId, out value))
                                                {
                                                    contentStr = ReplaceString(contentStr, listId.ToString(), value.ToString());
                                                    fileChanged = true;
                                                }
                                                else 
                                                {
                                                    AddPostAction(listId);
                                                    index = endIndex;
                                                    continue;
                                                }
                                            }
                                        }
                                        if (bindingAndDataSourceDic.ContainsKey("WebURL"))
                                        {
                                            foreach (string webUrl in bindingAndDataSourceDic["WebURL"])
                                            {
                                                string dKey = "\"" + webUrl + "\"";
                                                string dValue = "\"" + AveReplaceProcessor.UrlReplace(webUrl, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl) + "\"";
                                                if (!dKey.Equals(dValue, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    contentStr = ReplaceString(contentStr, dKey, dValue);
                                                    fileChanged = true;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case WebPartType.PictureLibrarySlideshow:
                                    arrtibuteName = " LibraryGuid=";
                                    tempIndex = contentStr.IndexOf(arrtibuteName, index, StringComparison.OrdinalIgnoreCase);
                                    if (tempIndex != -1)
                                    {   //利用已转移list的映射mapping，替换关联list的id。
                                        listId = new Guid(GetAttribute(contentStr, tempIndex, arrtibuteName));
                                        if (listId != Guid.Empty)
                                        {
                                            var value = Guid.Empty;
                                            if (mDocumentInfo.MappingManager.SiteMappingManager.GetValueFromListIdMapping(listId, out value))
                                            {
                                                contentStr = ReplaceString(contentStr, tempIndex, listId.ToString(), value.ToString());
                                                fileChanged = true;
                                            }
                                            else
                                            {
                                                AddPostAction(listId);
                                                index = endIndex;
                                                continue;
                                            }
                                        }
                                    }
                                    arrtibuteName = " ViewGuid=";
                                    tempIndex = contentStr.IndexOf(arrtibuteName, index, StringComparison.OrdinalIgnoreCase);
                                    if (tempIndex != -1)
                                    {   //相关联list的view文件id。
                                        Guid webPartId = new Guid(GetAttribute(contentStr, tempIndex, arrtibuteName));
                                        if (webPartId != Guid.Empty)
                                        {   //webpart的正确还原需要保证关联的view aspx文件也同步到目的端。s
                                            Guid viewGuidMappingValue;
                                            if (mDocumentInfo.MappingManager.SiteMappingManager.GetViewGuidMappingValue(webPartId, out viewGuidMappingValue))
                                            {
                                                contentStr = ReplaceString(contentStr, tempIndex, webPartId.ToString(), viewGuidMappingValue.ToString());
                                                fileChanged = true;
                                            }
                                            else
                                            {
                                                if (listId != Guid.Empty)
                                                {
                                                    AddPostAction(listId);
                                                    index = endIndex;
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case WebPartType.ContentByQuery:
                                    arrtibuteName = " ListName=";
                                    tempIndex = contentStr.IndexOf(arrtibuteName, index, StringComparison.OrdinalIgnoreCase);
                                    if (tempIndex != -1)
                                    {
                                        listName = GetAttribute(contentStr, tempIndex, arrtibuteName);
                                    }
                                    arrtibuteName = " ListGuid=";
                                    tempIndex = contentStr.IndexOf(arrtibuteName, index, StringComparison.OrdinalIgnoreCase);
                                    if (tempIndex != -1)
                                    {
                                        listId = new Guid(GetAttribute(contentStr, tempIndex, arrtibuteName));
                                        if (listId != Guid.Empty)
                                        {
                                            Guid destListId = GetMappingList(mDocumentInfo.AveItem.Web.ID, listName, listId);
                                            if (!destListId.Equals(Guid.Empty))
                                            {
                                                contentStr = ReplaceString(contentStr, tempIndex, listId.ToString(), destListId.ToString().ToUpper(CultureInfo.InvariantCulture));
                                                fileChanged = true;
                                            }
                                            else
                                            {
                                                string webUrl = null;
                                                arrtibuteName = " WebUrl=";
                                                tempIndex = contentStr.IndexOf(arrtibuteName, tempIndex, StringComparison.OrdinalIgnoreCase);
                                                if (index != -1)
                                                {
                                                    url = GetAttribute(contentStr, tempIndex, arrtibuteName);
                                                    if (!String.IsNullOrEmpty(url))
                                                    {
                                                        webUrl = GetMappedWebUrl(url);
                                                    }
                                                }
                                                if (webUrl != null && webUrl.StartsWith("~sitecollection", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    webUrl = mDocumentInfo.ParentSiteServerRelativeUrl + webUrl.Substring(15);
                                                }
                                                try
                                                {
                                                    using (IAveWeb tempWeb = mSite.OpenWeb(webUrl))
                                                    {
                                                        IAveList list = tempWeb.Lists[listName];
                                                        destListId = list.ID;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    logger.Log(AveLogLevel.WARN, "Cannot get list ID by list title: {0} from the Web: {1}, Exception: {2}", listName, webUrl, ex.ToString());
                                                }
                                                if (!destListId.Equals(Guid.Empty))
                                                {
                                                    contentStr = ReplaceString(contentStr, tempIndex, listId.ToString(), destListId.ToString().ToUpper(CultureInfo.InvariantCulture));
                                                    fileChanged = true;
                                                }
                                                else
                                                {
                                                    AddPostAction(listId);
                                                    index = endIndex;
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                    arrtibuteName = " WebUrl=";
                                    tempIndex = contentStr.IndexOf(arrtibuteName, tempIndex, StringComparison.OrdinalIgnoreCase);
                                    if (index != -1)
                                    {
                                        url = GetAttribute(contentStr, tempIndex, arrtibuteName);
                                        if (!String.IsNullOrEmpty(url))
                                        {
                                            string mappedUrl = GetMappedWebUrl(url);
                                            if (!url.Equals(mappedUrl, StringComparison.OrdinalIgnoreCase))
                                            {
                                                contentStr = ReplaceString(contentStr, tempIndex, url, mappedUrl);
                                                fileChanged = true;
                                            }
                                        }
                                    }
                                    break;
                                case WebPartType.TableOfContents:
                                    arrtibuteName = " AnchorLocation=";
                                    tempIndex = contentStr.IndexOf(arrtibuteName, index, StringComparison.OrdinalIgnoreCase);
                                    if (tempIndex != -1)
                                    {
                                        url = GetAttribute(contentStr, tempIndex, arrtibuteName);
                                        if (!String.IsNullOrEmpty(url))
                                        {
                                            newUrl = AveReplaceProcessor.UrlReplace(url, siteMappingManager.SiteManagedMappings, new ReplaceOption(true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                                            if (!url.Equals(newUrl, StringComparison.OrdinalIgnoreCase))
                                            {
                                                contentStr = ReplaceString(contentStr, tempIndex, url, newUrl);
                                                fileChanged = true;
                                            }
                                        }
                                    }
                                    break;
                            }
                            index = endIndex;
                        }
                        #endregion
                    }

                    bool strChanged;
                    if (mDocumentInfo.Name.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                    {
                        contentStr = ReplaceCSSContent(contentStr, out strChanged);
                    }
                    else
                    {
                        contentStr = ReplaceUrlContent(contentStr, out strChanged);
                    }
                    if (fileChanged || strChanged || realStream)
                    {
                        changed = fileChanged || strChanged;
                        content = Encoding.UTF8.GetBytes(contentStr);
                        return new MemoryStream(content);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.ContentReplaceFailed,
                        mFile == null ? mDocumentInfo.Url : mFile.Url, ex);
                }
                return mStream;

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "sitecollection")]
        private string GetMappedWebUrl(string url)
        {
            string mappedUrl = string.Empty;
            if (!url.StartsWith("~sitecollection", StringComparison.OrdinalIgnoreCase))
            {
                string destUrl = AveReplaceProcessor.UrlReplace(url, mDocumentInfo.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), mDocumentInfo.MappingManager.SiteMappingManager.SourceSiteInfo, mDocumentInfo.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                //contentStr = ReplaceString(contentStr, tempIndex, url, destUrl);
                //fileChanged = true;
                mappedUrl = destUrl;
            }
            else
            {
                string webServerRelativeUrl = string.Empty;
                string siteServerRelativeUrl = mDocumentInfo.MappingManager.SiteMappingManager.SourceSiteInfo.ServerRelativeUrl;//mAveWebPart.Manager.Web.Site.ServerRelativeUrl;
                if (url == "~sitecollection")
                {
                    if (siteServerRelativeUrl == "/")
                    {
                        webServerRelativeUrl = "/";
                    }
                    else
                    {
                        webServerRelativeUrl = siteServerRelativeUrl;
                    }
                }
                else if (url.StartsWith("~sitecollection", StringComparison.OrdinalIgnoreCase))
                {
                    if (siteServerRelativeUrl == "/")
                    {
                        webServerRelativeUrl = url.Substring(15);
                    }
                    else
                    {
                        webServerRelativeUrl = siteServerRelativeUrl + url.Substring(15);
                    }
                }
                else { }

                string destWebUrl = AveReplaceProcessor.UrlReplace(webServerRelativeUrl, mDocumentInfo.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), mDocumentInfo.MappingManager.SiteMappingManager.SourceSiteInfo, mDocumentInfo.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);

                if (mDocumentInfo.ParentSiteServerRelativeUrl == "/")
                {
                    if (destWebUrl == "/")
                    {
                        destWebUrl = string.Empty;//root sitecollection, root web 
                    }
                }
                else
                {
                    if (destWebUrl == mDocumentInfo.ParentSiteServerRelativeUrl)
                    {
                        destWebUrl = string.Empty;// root web 
                    }
                    else
                    {
                        destWebUrl = destWebUrl.Substring(mDocumentInfo.ParentSiteServerRelativeUrl.Length);
                    }
                }

                string destUrl = "~sitecollection" + destWebUrl;
                //contentStr = ReplaceString(contentStr, tempIndex, url, destUrl);
                mappedUrl = destUrl;
            }
            return mappedUrl;
        }

        private Guid GetMappingList(Guid webId, string title, Guid listId)
        {
            var value = Guid.Empty;
            if (mDocumentInfo.MappingManager.SiteMappingManager.GetValueFromListIdMapping(listId, out value))
            {
                return value;
            }
            //return GetListByNative(SqlConn, webId, title);
            if (string.IsNullOrEmpty(title))
            {
                return Guid.Empty;
            }
            return mSite.GetListId(webId, title);
        }

        public Dictionary<string, List<string>> GetBindingAndDataSourceDic(string contentStr, int index)
        {
            if (string.IsNullOrEmpty(contentStr) || index <= 0)
            {
                return null;
            }
            Guid listId = Guid.Empty;
            string bindingStart = "<ParameterBindings>";
            string bindingEnd = "</ParameterBindings>";
            int start = contentStr.IndexOf(bindingStart, index, StringComparison.OrdinalIgnoreCase);
            int end = 0;
            Dictionary<string, List<string>> bindingAndDataSourceDict = new Dictionary<string, List<string>>();
            if (start > 0)
            {
                end = contentStr.IndexOf(bindingEnd, index, StringComparison.OrdinalIgnoreCase);
                if (end <= 0)
                {
                    //mLog.Warn("Cannot get right bindingEnd. contenstr:{0}.", contentStr);
                    return bindingAndDataSourceDict;
                }
                string bindingXml = contentStr.Substring(start, end - start + bindingEnd.Length);
                XmlDocument xDoc = new XmlDocument();
                xDoc.PreserveWhitespace = true;
                xDoc.LoadXml(bindingXml);
                foreach (XmlElement node in xDoc.GetElementsByTagName("ParameterBinding"))
                {
                    string strName = node.GetAttribute("Name");
                    string value = node.GetAttribute("DefaultValue");
                    if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(value))
                    {
                        if (!bindingAndDataSourceDict.ContainsKey(strName))
                        {
                            List<string> temp = new List<string>();
                            temp.Add(value);
                            bindingAndDataSourceDict.Add(strName, temp);
                        }
                        else if (!bindingAndDataSourceDict[strName].Contains(value))
                        {
                            bindingAndDataSourceDict[strName].Add(value);
                        }
                    }
                }
            }

            string dataSourceStart = "<DataSources>";
            string dataSourceEnd = "</DataSources>";
            start = contentStr.IndexOf(dataSourceStart, index, StringComparison.OrdinalIgnoreCase);
            if (start > 0)
            {
                end = contentStr.IndexOf(dataSourceEnd, index, StringComparison.OrdinalIgnoreCase);
                if (end <= 0)
                {
                    //mLog.Warn("Cannot get right dataSourceEnd. contenstr:{0}.", contentStr);
                    return bindingAndDataSourceDict;
                }
                string dataSourceXml = contentStr.Substring(start, end - start + dataSourceEnd.Length);
                dataSourceXml = dataSourceXml.Replace(':', '_');
                XmlDocument xDoc = new XmlDocument();
                xDoc.PreserveWhitespace = true;
                xDoc.LoadXml(dataSourceXml);
                foreach (XmlElement node in xDoc.GetElementsByTagName("WebPartPages_DataFormParameter"))
                {
                    string strName = node.GetAttribute("Name");
                    string value = node.GetAttribute("DefaultValue");
                    if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(value))
                    {
                        if (!bindingAndDataSourceDict.ContainsKey(strName))
                        {
                            List<string> temp = new List<string>();
                            temp.Add(value);
                            bindingAndDataSourceDict.Add(strName, temp);
                        }
                        else if (!bindingAndDataSourceDict[strName].Contains(value))
                        {
                            bindingAndDataSourceDict[strName].Add(value);
                        }
                    }
                }
                foreach (XmlElement node in xDoc.GetElementsByTagName("asp_Parameter"))
                {
                    string strName = node.GetAttribute("Name");
                    string value = node.GetAttribute("DefaultValue");
                    if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(value))
                    {
                        if (!bindingAndDataSourceDict.ContainsKey(strName))
                        {
                            List<string> temp = new List<string>();
                            temp.Add(value);
                            bindingAndDataSourceDict.Add(strName, temp);
                        }
                        else if (!bindingAndDataSourceDict[strName].Contains(value))
                        {
                            bindingAndDataSourceDict[strName].Add(value);
                        }
                    }
                }
            }

            return bindingAndDataSourceDict;
        }

        private string ReplaceString(string content, string oldValue, string newValue)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPDocContentReplacer.ReplaceString"))
            {

                int index = 0;
                while ((index = content.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    content = content.Substring(0, index) + newValue + content.Substring(index + oldValue.Length);
                    index += newValue.Length;
                }
                return content;

            }

        }

        private string ReplaceString(string content, string oldValue, string newValue, int endIndex)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPDocContentReplacer.ReplaceString_1"))
            {

                int index = 0;
                while ((index = content.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    if (index > endIndex)
                    {
                        break;
                    }
                    content = content.Substring(0, index) + newValue + content.Substring(index + oldValue.Length);
                    index += newValue.Length;
                }
                return content;

            }

        }

        private WebPartType GetWebPartType(string content, int start, ref string name, ref int index)
        {


            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPDocContentReplacer.GetWebPartType"))
            {

                int tempIndex = -1;
                bool found = false;
                WebPartType wpType = WebPartType.None;
                if ((tempIndex = content.IndexOf("<WebPartPages:ListViewWebPart ", start, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    name = "WebPartPages:ListViewWebPart";
                    wpType = WebPartType.ListView;
                    index = tempIndex;
                    found = true;
                }
                if ((tempIndex = content.IndexOf("<WebPartPages:XsltListViewWebPart ", start, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    if (index == 0 || tempIndex < index || !found)
                    {
                        name = "WebPartPages:XsltListViewWebPart";
                        wpType = WebPartType.XsltListView;
                        index = tempIndex;
                        found = true;
                    }
                }
                if ((tempIndex = content.IndexOf("<WebPartPages:ImageWebPart ", start, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    if (index == 0 || tempIndex < index || !found)
                    {
                        name = "WebPartPages:ImageWebPart";
                        wpType = WebPartType.Image;
                        index = tempIndex;
                        found = true;
                    }
                }
                if ((tempIndex = content.IndexOf("<WebPartPages:DataFormWebPart ", start, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    if (index == 0 || tempIndex < index || !found)
                    {
                        name = "WebPartPages:DataFormWebPart";
                        wpType = WebPartType.DataForm;
                        index = tempIndex;
                        found = true;
                    }
                }
                if ((tempIndex = content.IndexOf("<WebPartPages:PictureLibrarySlideshowWebPart ", start, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    if (index == 0 || tempIndex < index || !found)
                    {
                        name = "WebPartPages:PictureLibrarySlideshowWebPart";
                        wpType = WebPartType.PictureLibrarySlideshow;
                        index = tempIndex;
                        found = true;
                    }
                }
                if ((tempIndex = content.IndexOf("<WebControls:ContentByQueryWebPart ", start, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    if (index == 0 || tempIndex < index || !found)
                    {
                        name = "WebControls:ContentByQueryWebPart";
                        wpType = WebPartType.ContentByQuery;
                        index = tempIndex;
                        found = true;
                    }
                }
                if ((tempIndex = content.IndexOf("<a08fba8b1:TableOfContentsWebPart ", start, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    if (index == 0 || tempIndex < index || !found)
                    {
                        name = "a08fba8b1:TableOfContentsWebPart";
                        wpType = WebPartType.TableOfContents;
                        index = tempIndex;
                        found = true;
                    }
                }
                //ADO-163163 添加对该类型webpart的替换逻辑
                if ((tempIndex = content.IndexOf("<a08fba8b1:ContentByQueryWebPart ", start, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    if (index == 0 || tempIndex < index || !found)
                    {
                        name = "a08fba8b1:ContentByQueryWebPart";
                        wpType = WebPartType.ContentByQuery;
                        index = tempIndex;
                        found = true;
                    }
                }
            if (!found)
                {
                    index = 0;
                    return WebPartType.None;
                }
                return wpType;

            }


        }

        private enum WebPartType
        {
            None,
            ListView,
            Image,
            XsltListView,
            DataForm,
            PictureLibrarySlideshow,
            ContentByQuery,
            TableOfContents
        }
    }
}
