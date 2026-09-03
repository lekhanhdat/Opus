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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Web;
using Microsoft.InformationProtection.Exceptions;
using System.Collections;

namespace AvePoint.Wrapper.Common
{
    public class InfoPathLinkReplace
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "xml namespace")]
        static InfoPathLinkReplace()
        {
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "file extension")]
        [HandleProcessCorruptedStateExceptions]
        public byte[] FixXSNBinary(byte[] originalCabBinary, string fileRelativeUrl, AveMappingManager mappingManager, ref bool fChanged, ref string contentTypeId, ref bool isListForm)
        {
            using (new AvePerformanceScope("Common.InfoPathLinkReplace.FixXSNBinary"))
            {
                try
                {
                    log.Info("start to fix infopath FixXSNBinary.fileRelativeUrl: {0}", fileRelativeUrl);
                    if (BinaryHasDigitalSig(originalCabBinary))
                    {
                        return null;
                    }
                    Stream stream = new MemoryStream(originalCabBinary, false);
                    Stream fileStream = null;
                    using (CabinetExtractor extractor = new CabinetExtractor())
                    {
                        fileStream = extractor.Extract(stream, "manifest.xsf");
                    }
                    Stream newManifestStream = this.FixManifestXML(fileStream, fileRelativeUrl, mappingManager, ref fChanged, ref contentTypeId, ref isListForm);
                    if (fChanged && (newManifestStream != null))
                    {
                        Stream input = GenerateFixedXSNCab(stream, newManifestStream);
                        newManifestStream.Close();
                        input.Seek(0L, SeekOrigin.Begin);
                        byte[] buffer = new byte[input.Length];
                        BinaryReader sr = new BinaryReader(input);
                        int bRead = 0;
                        while (bRead < buffer.Length)
                        {
                            int rd = sr.Read(buffer, bRead, buffer.Length - bRead);
                            if (rd == -1)
                            {
                                throw new FileIOException("file is unusually small");
                            }
                            bRead += rd;
                        }
                        input.Close();
                        return buffer;
                    }
                    if (newManifestStream != null)
                    {
                        newManifestStream.Close();
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred in FixXSNBinary,fileRelativeUrl: " + fileRelativeUrl, e.ToString());
                }
                return null;
            }
        }

        private static bool BinaryHasDigitalSig(byte[] binaryData)
        {
            bool flag = false;
            string tempFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            SaveBinaryIntoFile(binaryData, tempFileName);
            flag = HasDigitalSig(tempFileName);
            File.Delete(tempFileName);
            return flag;
        }

        private static void SaveBinaryIntoFile(byte[] binaryData, string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(binaryData);
                }
            }
        }

        private static bool HasDigitalSig(string path)
        {
            try
            {
                X509Certificate.CreateFromSignedFile(path);
            }
            catch (CryptographicException)
            {
                return false;
            }
            return true;
        }

        private Stream FixManifestXML(Stream fileStream, string fileRelativeUrl, AveMappingManager mappingManager, ref bool fChanged, ref string contentTypeId, ref bool isListForm)
        {
            return this.FixXML(fileStream, true, fileRelativeUrl, mappingManager, ref fChanged, ref contentTypeId, ref isListForm);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "xml namespace")]
        private Stream FixXML(Stream fileStream, bool fManifest, string filePath, AveMappingManager mappingManager, ref bool fChanged, ref string contentTypeId, ref bool isListForm)
        {
            using (new AvePerformanceScope("Common.InfoPathLinkReplace.FixXML"))
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    fileStream.Seek(0L, SeekOrigin.Begin);
                    xmlDocument.PreserveWhitespace = true;
                    XmlReader reader = XmlReader.Create(fileStream);
                    try
                    {
                        xmlDocument.Load(reader);
                    }
                    catch (XmlException)
                    {
                        return null;
                    }
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                    if (fManifest)
                    {
                        nsmgr.AddNamespace("xsf", "http://schemas.microsoft.com/office/infopath/2003/solutionDefinition");
                        nsmgr.AddNamespace("xsf2", "http://schemas.microsoft.com/office/infopath/2006/solutionDefinition/extensions");
                        nsmgr.AddNamespace("xsf3", "http://schemas.microsoft.com/office/infopath/2009/solutionDefinition/extensions");
                    }
                    else
                    {
                        nsmgr.AddNamespace("udc", "http://schemas.microsoft.com/office/infopath/2006/udc");
                    }

                    XmlNode node1 = xmlDocument.SelectSingleNode("/xsf:xDocumentClass/xsf:extensions/xsf:extension[@name='SolutionDefinitionExtensions']/xsf2:solutionDefinition/xsf2:solutionPropertiesExtension", nsmgr);
                    XmlNode node2 = xmlDocument.SelectSingleNode("/xsf:xDocumentClass/xsf:extensions/xsf:extension[@name='SolutionDefinitionExtensions']/xsf3:solutionDefinition/xsf3:baseUrl/@relativeUrlBase", nsmgr);

                    string sourceBaseUrl = null;
                    InfoPathPublishType infoPathPublishType = InfoPathPublishType.None;
                    switch (node1.Attributes["branch"].InnerText)
                    {
                        case "wss":
                            infoPathPublishType = InfoPathPublishType.Library;
                            //XmlAttribute path = node1.ChildNodes[1].Attributes["path"];
                            //path.Value = GetSiteUrl(GetRootUrl(path.Value), path.Value, infoPathPublishType);
                            //ReplaceDestAttributeValue(node1.ChildNodes[1].Attributes["path"], null, InfoPathPublishType.Library, filePath, mappingManager);
                            break;
                        case "contentTypeTemplate":
                            infoPathPublishType = InfoPathPublishType.ContentType;
                            break;
                        case "admin":
                            infoPathPublishType = InfoPathPublishType.FormService;
                            //ReplaceDestAttributeValue(node1.ChildNodes[1].Attributes["site"], null, InfoPathPublishType.FormService, filePath, mappingManager);
                            //fChanged=true;
                            break;
                        case "list":
                            infoPathPublishType = InfoPathPublishType.List;
                            break;
                        default:
                            infoPathPublishType = InfoPathPublishType.None;
                            break;

                    }
                    if (node2 != null)
                    {
                        sourceBaseUrl = node2.InnerText;
                        if (infoPathPublishType == InfoPathPublishType.FormService)
                        {
                            node2.InnerText = AveUrlUtility.GetServerUrl(filePath);//GetRootUrl(filePath);
                            fChanged = true;
                        }
                        else
                        {
                            node2.InnerText = filePath;
                            fChanged = true;
                        }
                    }

                    InfoPathUrlConvertProcess urlReplacePro = new InfoPathUrlConvertProcess(sourceBaseUrl, filePath, mappingManager, infoPathPublishType);
                    GetDataObjectsGuidAndUrl(xmlDocument, urlReplacePro, nsmgr, ref isListForm, ref contentTypeId);

                    if (fChanged)
                    {
                        MemoryStream outStream = new MemoryStream();
                        xmlDocument.Save(outStream);
                        outStream.Seek(0L, SeekOrigin.Begin);
                        return outStream;
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred in FixXML,filePath: " + filePath, e.ToString());
                }
                return null;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong names are xml node name")]
        public void GetDataObjectsGuidAndUrl(XmlDocument xmlDocument, InfoPathUrlConvertProcess urlReplacePro, XmlNamespaceManager nsmgr, ref bool isListForm, ref string contentTypeId)
        {
            using (new AvePerformanceScope("Common.InfoPathLinkReplace.GetDataObjectsGuidAndUrl"))
            {
                try
                {
                    //log.Debug("InfoPath Before Replace: "+xmlDocument.InnerXml);
                    XmlNode dataObjectsNode = xmlDocument.SelectSingleNode("/xsf:xDocumentClass/xsf:dataObjects", nsmgr);
                    //替换data connection相关的list的id和对应site的url
                    if (dataObjectsNode != null)
                    {
                        foreach (XmlNode dataObject in dataObjectsNode.ChildNodes)
                        {
                            if (dataObject.Name.Equals("xsf:dataObject", StringComparison.OrdinalIgnoreCase))
                            {
                                GetAttributeOfNode(dataObject, urlReplacePro, ref contentTypeId);
                            }
                        }
                    }

                    //替换infopath query对应的siteurl和listID（这种query是在sharepoint list infopath 才会有的）
                    XmlNode queryNode = xmlDocument.SelectSingleNode("/xsf:xDocumentClass/xsf:query", nsmgr);
                    if (queryNode != null)
                    {
                        if (queryNode.ChildNodes.Count >= 2)
                        {
                            GetAttributeOfNode(queryNode, urlReplacePro, ref contentTypeId);
                            isListForm = true;
                        }
                    }

                    XmlNode connectionNode = xmlDocument.SelectSingleNode("/xsf:xDocumentClass/xsf:extensions/xsf:extension/xsf2:solutionDefinition/xsf2:dataConnections", nsmgr);
                    if (connectionNode != null)
                    {
                        nsmgr.AddNamespace("con", "http://schemas.microsoft.com/sharepoint/soap/");
                        foreach (XmlNode node in connectionNode.ChildNodes)
                        {
                            if (node.Name.Equals("xsf2:sharepointListAdapterRWExtension", StringComparison.OrdinalIgnoreCase))
                            {
                                XmlAttribute queryKey = node.Attributes["queryKey"];
                                string url = queryKey.Value.Substring(0, queryKey.Value.IndexOf('<'));
                                XmlDocument xmlDoc = new XmlDocument();
                                xmlDoc.LoadXml(queryKey.Value.Substring(url.Length));
                                XmlNode listNameNode = xmlDoc.SelectSingleNode("con:GetListItems/con:listName", nsmgr);
                                if (listNameNode != null)
                                {
                                    listNameNode.InnerText = "{" + urlReplacePro.ReplaceGuid(listNameNode.InnerText).Trim('{').Trim('}') + "}";
                                }
                                queryKey.Value = urlReplacePro.ReplaceOriginalUrl(url).TrimEnd('/') + '/' + xmlDoc.InnerXml;
                            }
                        }
                    }

                    /*
                     * <xsf2:sharepointListAdapterRWExtension ref="対応状況" 
                     * queryKey="https://kkiw.sharepoint.com/incident/<GetListItems xmlns="http://schemas.microsoft.com/sharepoint/soap/">
                     * <listName>{0C921A44-C9D0-4E67-93CF-2775BEDAD52A}</listName><queryOptions><QueryOptions><IncludeAttachmentUrls>
                     * TRUE</IncludeAttachmentUrls><ExpandUserField>true</ExpandUserField><ViewAttributes Scope="RecursiveAll"/>
                     * </QueryOptions></queryOptions><viewFields><ViewFields><FieldRef Name="Title"/><FieldRef Name="ID"/></ViewFields>
                     * </viewFields><rowLimit>4294967295</rowLimit></GetListItems>" queryFile="_____offline.xml" queryThisFormOnly="no"/>
                     */
                    //替换submit对应的folderUrl
                    //XmlNode submitNode = xmlDocument.SelectSingleNode("/xsf:xDocumentClass/xsf:submit/xsf:davAdapter/xsf:folderURL", nsmgr);
                    //if (submitNode != null)
                    //{
                    //    if (sourceRootUrl != null && submitNode.Attributes["value"] != null && submitNode.Attributes["value"].Value!="")
                    //    {
                    //        submitNode.Attributes["value"].Value = GetSiteUrl(sourceRootUrl, submitNode.Attributes["value"].Value,InfoPathPublishType.Library);
                    //    }
                    //}
                    XmlNode needReplaceByType = null;
                    switch (urlReplacePro.infoPathType)
                    {
                        case InfoPathPublishType.Library:
                            needReplaceByType = xmlDocument.SelectSingleNode("/xsf:xDocumentClass/xsf:extensions/xsf:extension[@name='SolutionDefinitionExtensions']/xsf2:solutionDefinition/xsf2:solutionPropertiesExtension/xsf2:wss/@path", nsmgr);
                            break;
                        case InfoPathPublishType.List:
                            needReplaceByType = xmlDocument.SelectSingleNode("/xsf:xDocumentClass/xsf:extensions/xsf:extension[@name='SolutionDefinitionExtensions']/xsf2:solutionDefinition/xsf2:solutionPropertiesExtension/xsf2:list/@path", nsmgr);
                            break;
                        case InfoPathPublishType.ContentType:
                            needReplaceByType = xmlDocument.SelectSingleNode("/xsf:xDocumentClass/xsf:extensions/xsf:extension[@name='SolutionDefinitionExtensions']/xsf2:solutionDefinition/xsf2:solutionPropertiesExtension/xsf2:contentTypeTemplate/@site", nsmgr);
                            break;
                        case InfoPathPublishType.FormService:
                            needReplaceByType = xmlDocument.SelectSingleNode("/xsf:xDocumentClass/xsf:extensions/xsf:extension[@name='SolutionDefinitionExtensions']/xsf2:solutionDefinition/xsf2:solutionPropertiesExtension/xsf2:admin/@site", nsmgr);
                            break;
                        default:
                            break;
                    }
                    if (needReplaceByType != null)
                    {
                        needReplaceByType.InnerText = urlReplacePro.ReplaceUrl(needReplaceByType.InnerText);
                        if (urlReplacePro.infoPathType == InfoPathPublishType.FormService)
                        {
                            if (!needReplaceByType.InnerText.Contains(urlReplacePro.sourceRootUrl) && !needReplaceByType.InnerText.Contains(urlReplacePro.destRootUrl))
                            {
                                needReplaceByType.InnerText = urlReplacePro.destRootUrl + needReplaceByType.InnerText;
                            }
                        }
                    }
                    //log.Debug("InfoPath Xml After Replace: " + xmlDocument.InnerXml);
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred in GetDataObjectsGuidAndUrl", e.ToString());
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "xsf")]
        public void GetAttributeOfNode(XmlNode dataObject, InfoPathUrlConvertProcess urlReplacePro, ref string contentTypeId)
        {
            using (new AvePerformanceScope("Common.InfoPathLinkReplace.GetAttributeOfNode"))
            {
                XmlAttribute sharepointGuid = null;
                XmlAttribute siteUrl = null;
                XmlAttribute relativeListURL = null;
                XmlNode node = null;
                try
                {
                    if (dataObject.Name.Equals("xsf:query", StringComparison.OrdinalIgnoreCase))
                    {
                        node = dataObject.ChildNodes[1];
                    }
                    else
                    {
                        node = dataObject.ChildNodes[1].ChildNodes[1];
                    }
                    if (node != null)
                    {
                        sharepointGuid = node.Attributes["sharepointGuid"] ?? node.Attributes["sharePointListID"];
                        XmlAttribute originalContentTypeId = node.Attributes["contentTypeID"] ?? node.Attributes["contentTypeID"];
                        siteUrl = node.Attributes["siteUrl"] ?? node.Attributes["siteURL"];
                        relativeListURL = node.Attributes["relativeListUrl"];
                        if (sharepointGuid != null)
                        {
                            sharepointGuid.Value = urlReplacePro.ReplaceGuid(sharepointGuid.Value);
                            if (!sharepointGuid.Value.Contains("{"))
                            {
                                sharepointGuid.Value = "{" + sharepointGuid.Value + "}";
                            }
                        }
                        if (siteUrl != null)
                        {
                            siteUrl.Value = urlReplacePro.ReplaceUrl(siteUrl.Value);
                            if (!siteUrl.Value.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                            {
                                siteUrl.Value = siteUrl.Value + "/";
                            }
                        }
                        if (originalContentTypeId != null)
                        {
                            ArgumentCheck.CheckNotNull(sharepointGuid);
                            originalContentTypeId.Value = urlReplacePro.ReplaceContentTypeId(new Guid(sharepointGuid?.Value), originalContentTypeId.Value);
                            contentTypeId = originalContentTypeId.Value;
                        }
                        if (relativeListURL != null)
                        {
                            relativeListURL.Value = urlReplacePro.ReplaceUrl(relativeListURL.Value);
                            if (!relativeListURL.Value.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                            {
                                relativeListURL.Value = relativeListURL.Value + "/";
                            }
                        }
                        //if (sharepointGuid != null && siteUrl != null)
                        //{
                        //    id = new Guid(sharepointGuid.Value);
                        //    if (mappingManager.SiteMappingManager.ListIdMapping.ContainsKey(id))
                        //    {
                        //        //data connection的library被改变
                        //        ReplaceSourceAttributeValue(siteUrl, relativeListURL, infoPathPublishType, sourceRootUrl);
                        //        ReplaceDestAttributeValue(siteUrl, relativeListURL, infoPathPublishType, fileRelativeUrl, mappingManager);
                        //        sharepointGuid.Value = "{" + mappingManager.SiteMappingManager.ListIdMapping[id].ToString() + "}";
                        //        changed = true;
                        //    }
                        //    else if (sourceRootUrl != null)
                        //    {
                        //        //data connection的library没有被改变
                        //        ReplaceSourceAttributeValue(siteUrl, relativeListURL, infoPathPublishType, sourceRootUrl);
                        //        changed = true;
                        //    }
                        //}
                    }
                }
                catch (Exception e)
                {
                    log.Warn("When replaced the site url and list id throw exception ", e.ToString());
                    throw;
                }
            }
        }

        //public void ReplaceSourceAttributeValue(XmlAttribute siteUrl, XmlAttribute relativeListUrl, InfoPathPublishType infoPathPublishType, string sourceRootUrl)
        //{
        //    using (new AvePerformanceScope("Common.InfoPathLinkReplace.ReplaceSourceAttributeValue"))
        //    {
        //        if (relativeListUrl != null)
        //        {
        //            switch (infoPathPublishType)
        //            {
        //                case InfoPathPublishType.Library:
        //                case InfoPathPublishType.List:
        //                case InfoPathPublishType.ContentType:
        //                    siteUrl.Value = GetSiteUrl(sourceRootUrl, siteUrl.Value, infoPathPublishType);
        //                    relativeListUrl.Value = GetSiteUrl(sourceRootUrl, relativeListUrl.Value, infoPathPublishType);
        //                    break;
        //                case InfoPathPublishType.FormService:
        //                    siteUrl.Value = GetRootUrl(sourceRootUrl) + siteUrl.Value;
        //                    relativeListUrl.Value = sourceRootUrl + relativeListUrl.Value;
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //    }
        //}

        //public void ReplaceDestAttributeValue(XmlAttribute siteUrl, XmlAttribute relativeListUrl, InfoPathPublishType infoPathPublishType, string fileRelativeUrl, AveMappingManager mappingManager)
        //{
        //    using (new AvePerformanceScope("Common.InfoPathLinkReplace.ReplaceDestAttributeValue"))
        //    {
        //        try
        //        {
        //            string destRootUrl = GetRootUrl(fileRelativeUrl);
        //            string sourceUrl = null;
        //            if (relativeListUrl != null)
        //            {
        //                sourceUrl = relativeListUrl.Value;
        //            }
        //            else
        //            {
        //                sourceUrl = siteUrl.Value;
        //            }
        //            string rootUrl = GetRootUrl(sourceUrl);
        //            sourceUrl = sourceUrl.Replace(rootUrl, "");
        //            if (sourceUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
        //            {
        //                sourceUrl = sourceUrl.Remove(sourceUrl.LastIndexOf("/", StringComparison.OrdinalIgnoreCase), 1);
        //            }
        //            if (!sourceUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        //            {
        //                sourceUrl = "/" + sourceUrl;
        //            }
        //            sourceUrl = HttpUtility.UrlDecode(sourceUrl);
        //            if (mappingManager.SiteMappingManager.ListUrlMapping.ContainsKey(sourceUrl))
        //            {
        //                if (relativeListUrl != null)
        //                {
        //                    relativeListUrl.Value = destRootUrl + mappingManager.SiteMappingManager.ListUrlMapping[sourceUrl];
        //                    siteUrl.Value = GetSiteUrl(relativeListUrl.Value, infoPathPublishType);
        //                }
        //                else
        //                {
        //                    siteUrl.Value = destRootUrl + mappingManager.SiteMappingManager.ListUrlMapping[sourceUrl];
        //                }
        //            }
        //            else if (mappingManager.SiteMappingManager.WebUrlMapping.ContainsKey(sourceUrl))
        //            {
        //                siteUrl.Value = destRootUrl + mappingManager.SiteMappingManager.WebUrlMapping[sourceUrl];
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            log.Warn("When replaced the destination site url and list id throw exception ", e.ToString());
        //            throw;
        //        }
        //    }
        //}

        //public string GetSiteUrl(string sourceRootUrl, string dataObjectSiteUrl,InfoPathPublishType infoPathPublishType)
        //{
        //    using (new AvePerformanceScope("Common.InfoPathLinkReplace.GetSiteUrl_sourceRootUrl"))
        //    {
        //        StringBuilder stringBuilder = new StringBuilder();
        //        try
        //        {
        //            string[] sourceStrings = sourceRootUrl.Split(new string[] { "/", "//" }, StringSplitOptions.RemoveEmptyEntries);
        //            string[] dataObjects = dataObjectSiteUrl.Split(new string[] { "/", "//" }, StringSplitOptions.RemoveEmptyEntries);
        //            //List<string> newstrings = new List<string>();
        //            int relativeRow = 0;
        //            for (int i = 0; i < dataObjects.Length; i++)
        //            {
        //                if (dataObjects[i].Equals(".."))
        //                {
        //                    relativeRow++;
        //                }
        //            }
        //            if (relativeRow == 0)
        //            {
        //                return dataObjectSiteUrl;
        //            }
        //            if (infoPathPublishType == InfoPathPublishType.Library || infoPathPublishType == InfoPathPublishType.ContentType)
        //            {
        //                for (int i = 0; i < sourceStrings.Length - relativeRow - 2; i++)
        //                {
        //                    if (i == 0)
        //                    {
        //                        stringBuilder.Append(sourceStrings[0] + "//" + sourceStrings[1] + "/");
        //                    }
        //                    else
        //                    {
        //                        stringBuilder.Append(sourceStrings[i + 1] + "/");
        //                    }
        //                }
        //            }
        //            if (infoPathPublishType == InfoPathPublishType.List)
        //            {
        //                for (int i = 0; i < sourceStrings.Length - relativeRow - 1; i++)
        //                {
        //                    if (i == 0)
        //                    {
        //                        stringBuilder.Append(sourceStrings[0] + "//" + sourceStrings[1] + "/");
        //                    }
        //                    else
        //                    {
        //                        stringBuilder.Append(sourceStrings[i + 1] + "/");
        //                    }
        //                }
        //            }
        //            for (int i = relativeRow; i < dataObjects.Length; i++)
        //            {
        //                stringBuilder.Append(dataObjects[i]);
        //                stringBuilder.Append("/");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            log.Warn("An error occurred when GetSiteUrl by sourceRootUrl,sourceRootUrl: " + sourceRootUrl + "dataObjectSiteUrl:" + dataObjectSiteUrl, e.ToString());
        //            throw;
        //        }
        //        return stringBuilder.ToString();
        //    }
        //}

        //public string GetSiteUrl(string sourceListUrl,InfoPathPublishType infoPathPublishType)
        //{
        //    using (new AvePerformanceScope("Common.InfoPathLinkReplace.GetSiteUrl_sourceListUrl"))
        //    {
        //        StringBuilder stringBuilder = new StringBuilder();
        //        int isLists = 1;
        //        try
        //        {
        //            string[] strs = sourceListUrl.Split(new string[] { "/", "//" }, StringSplitOptions.RemoveEmptyEntries);
        //            if (sourceListUrl.Contains("/Lists/"))
        //            {
        //                isLists = 2;
        //            }
        //            for (int i = 0; i < strs.Length - isLists; i++)
        //            {
        //                if (i == 0)
        //                {
        //                    stringBuilder.Append(strs[i]);
        //                    stringBuilder.Append("//");
        //                }
        //                else
        //                {
        //                    stringBuilder.Append(strs[i]);
        //                    stringBuilder.Append("/");
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            log.Warn("An error occurred when GetSiteUrl by sourceListUrl,sourceListUrl: " + sourceListUrl, e.ToString());
        //            throw;
        //        }
        //        return stringBuilder.ToString();
        //    }
        //}

        //public string GetRootUrl(string rootUrl)
        //{
        //    using (new AvePerformanceScope("Common.InfoPathLinkReplace.GetRootUrl"))
        //    {
        //        StringBuilder stringBuilder = new StringBuilder();
        //        try
        //        {
        //            string[] strs = rootUrl.Split(new string[] { "/", "//" }, StringSplitOptions.RemoveEmptyEntries);
        //            stringBuilder.Append(strs[0]);
        //            stringBuilder.Append("//");
        //            stringBuilder.Append(strs[1]);
        //        }
        //        catch (Exception e)
        //        {
        //            log.Warn("An error occurred when GetRootUrl ,GetRootUrl: " + rootUrl, e.ToString());
        //            throw;
        //        }
        //        return stringBuilder.ToString();
        //    }
        //}        

        private static Stream GenerateFixedXSNCab(Stream originalCabStream, Stream newManifestStream)
        {
            using (new AvePerformanceScope("Common.InfoPathLinkReplace.GenerateFixedXSNCab"))
            {
                try
                {
                    MemoryStream newCabStream = new MemoryStream();
                    using (CabinetExtractor extractor = new CabinetExtractor())
                    {
                        IList<CabinetFileInfo> fileInfo = extractor.GetFileInfo(originalCabStream);
                        List<string> list2 = new List<string>(fileInfo.Count);
                        Dictionary<string, CabinetFileInfo> cabFilesInfo = new Dictionary<string, CabinetFileInfo>(fileInfo.Count, StringComparer.OrdinalIgnoreCase);
                        foreach (CabinetFileInfo info in fileInfo)
                        {
                            cabFilesInfo.Add(info.Name, info);
                            list2.Add(info.Name);
                        }
                        using (CabinetCreator creator = new CabinetCreator())
                        {
                            creator.Create(new CabinetCreatorHelper(newCabStream, newManifestStream, originalCabStream, cabFilesInfo, extractor), list2.ToArray());
                        }
                    }
                    return newCabStream;
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred when GenerateFixedXSNCab ", e.ToString());
                    throw;
                }
            }
        }

      

    }

    public sealed class CabinetCreatorHelper : ICabinetCreateStreamContext
    {
        // Fields
        private Dictionary<string, CabinetFileInfo> _cabFilesInfo;
        private CabinetExtractor _cabinetExtractor;
        private Stream _newCabStream;
        private Stream _newManifestStream;
        private Stream _originalCabStream;

        // Methods
        internal CabinetCreatorHelper(Stream newCabStream, Stream newManifestStream, Stream originalCabStream, Dictionary<string, CabinetFileInfo> cabFilesInfo, CabinetExtractor cabinetExtractor)
        {
            this._newCabStream = newCabStream;
            this._newManifestStream = newManifestStream;
            this._originalCabStream = originalCabStream;
            this._cabFilesInfo = cabFilesInfo;
            this._cabinetExtractor = cabinetExtractor;
        }

        public void CloseCabinetWriteStream(int cabinetNumber, string cabinetName, Stream stream)
        {
            if (this._newCabStream != null)
            {
                this._newCabStream.Flush();
            }
        }

        public void CloseFileReadStream(string path, Stream stream)
        {
        }

        public string GetCabinetName(int cabinetNumber)
        {
            return string.Empty;
        }

        public Stream OpenCabinetWriteStream(int cabinetNumber, string cabinetName)
        {
            return this._newCabStream;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "file extension")]
        public Stream OpenFileReadStream(string path, out FileAttributes attributes, out DateTime lastWriteTime)
        {
            CabinetFileInfo info = this._cabFilesInfo[path];
            attributes = info.Attributes;
            lastWriteTime = info.LastWriteTime;
            if (string.Compare(path, "manifest.xsf", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this._newManifestStream;
            }
            return this._cabinetExtractor.Extract(this._originalCabStream, path);
        }
    }

    public sealed class Entry
    {
        // Fields
        public string _entryXPath;
        public bool _fIsUrl;

        // Methods
        public Entry(bool fIsUrl, string entryXPath)
        {
            this._fIsUrl = fIsUrl;
            this._entryXPath = entryXPath;
        }
    }

    public enum InfoPathPublishType
    {
        None,
        Library,
        List,
        FormService,
        ContentType
    }

    public class InfoPathUrlConvertProcess
    {
        private AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public string sourceBaseUrl;
        public string destBaseUrl;
        public AveMappingManager mappingManager;
        public InfoPathPublishType infoPathType;
        private string mSourceRootUrl;
        private string mDestRootUrl;

        public InfoPathUrlConvertProcess(string sourceBaseUrl, string destBaseUrl, AveMappingManager mappingManager, InfoPathPublishType infopathType)
        {
            this.sourceBaseUrl = sourceBaseUrl;
            this.destBaseUrl = destBaseUrl;
            this.mappingManager = mappingManager;
            this.infoPathType = infopathType;
        }

        public string sourceRootUrl
        {
            get
            {
                if (string.IsNullOrEmpty(mSourceRootUrl) && !string.IsNullOrEmpty(sourceBaseUrl))
                {
                    mSourceRootUrl = AveUrlUtility.GetServerUrl(sourceBaseUrl);// GetRootUrl(sourceBaseUrl);
                }
                return mSourceRootUrl;
            }
            set
            {
                mSourceRootUrl = value;
            }
        }

        public string destRootUrl
        {
            get
            {
                if (string.IsNullOrEmpty(mDestRootUrl) && !string.IsNullOrEmpty(destBaseUrl))
                {
                    mDestRootUrl = AveUrlUtility.GetServerUrl(destBaseUrl);//GetRootUrl(destBaseUrl);
                }
                return mDestRootUrl;
            }
            set
            {
                mDestRootUrl = value;
            }
        }

        public string GetAbsoluteUrl(string url)
        {
            using (new AvePerformanceScope("Common.InfoPathUrlConvertProcess.GetAbsoluteUrl"))
            {
                StringBuilder stringBuilder = new StringBuilder();
                try
                {
                    if (string.IsNullOrEmpty(this.sourceBaseUrl))
                    {
                        return url;
                    }
                    string[] splitSourceBaseUrl = this.sourceBaseUrl.Split(new string[] { "/", "//" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] splitUrl = url.Split(new string[] { "/", "//" }, StringSplitOptions.RemoveEmptyEntries);
                    //List<string> newstrings = new List<string>();
                    int relativeRow = 0;
                    for (int i = 0; i < splitUrl.Length; i++)
                    {
                        if (splitUrl[i].Equals(".."))
                        {
                            relativeRow++;
                        }
                    }
                    if (relativeRow == 0)
                    {
                        return url;
                    }
                    if (infoPathType == InfoPathPublishType.Library || infoPathType == InfoPathPublishType.ContentType)
                    {
                        for (int i = 0; i < splitSourceBaseUrl.Length - relativeRow - 2; i++)
                        {
                            if (i == 0)
                            {
                                stringBuilder.Append(splitSourceBaseUrl[0] + "//" + splitSourceBaseUrl[1] + "/");
                            }
                            else
                            {
                                stringBuilder.Append(splitSourceBaseUrl[i + 1] + "/");
                            }
                        }
                    }
                    if (infoPathType == InfoPathPublishType.List)
                    {
                        for (int i = 0; i < splitSourceBaseUrl.Length - relativeRow - 1; i++)
                        {
                            if (i == 0)
                            {
                                stringBuilder.Append(splitSourceBaseUrl[0] + "//" + splitSourceBaseUrl[1] + "/");
                            }
                            else
                            {
                                stringBuilder.Append(splitSourceBaseUrl[i + 1] + "/");
                            }
                        }
                    }
                    for (int i = relativeRow; i < splitUrl.Length; i++)
                    {
                        stringBuilder.Append(splitUrl[i]);
                        stringBuilder.Append("/");
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred when GetSiteUrl by sourceRootUrl,sourceBaseUrl: " + sourceBaseUrl + "url:" + url, e.ToString());
                    throw;
                }
                return stringBuilder.ToString();
            }

        }

        public string ReplaceUrl(string url)
        {
            string newUrl = url;
            newUrl = GetAbsoluteUrl(url);
            if (string.IsNullOrEmpty(this.sourceBaseUrl))
            {
                sourceRootUrl = new Uri(url).GetLeftPart(UriPartial.Authority);//GetRootUrl(url);
            }
            newUrl = newUrl.Replace(sourceRootUrl, "");
            newUrl = HttpUtility.UrlDecode(newUrl);
            if (newUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                newUrl = newUrl.Remove(newUrl.LastIndexOf("/", StringComparison.OrdinalIgnoreCase), 1);
            }
            if (!newUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                newUrl = "/" + newUrl;
            }
            if (mappingManager.SiteMappingManager.ListUrlMapping.ContainsKey(newUrl))
            {
                newUrl = mappingManager.SiteMappingManager.ListUrlMapping[newUrl];
                return GetRelativeUrl(destRootUrl + newUrl);
            }
            else if (mappingManager.SiteMappingManager.WebUrlMapping.ContainsKey(newUrl))
            {
                newUrl = mappingManager.SiteMappingManager.WebUrlMapping[newUrl];
                return GetRelativeUrl(destRootUrl + newUrl);
            }
            return GetRelativeUrl(sourceRootUrl + newUrl);
        }

        public string GetRelativeUrl(string url)
        {
            if (string.IsNullOrEmpty(this.sourceBaseUrl))
            {
                return url;
            }
            if (this.infoPathType == InfoPathPublishType.FormService)
            {
                if (!string.IsNullOrEmpty(this.destRootUrl) && this.destRootUrl.Equals(AveUrlUtility.GetServerUrl(url), StringComparison.OrdinalIgnoreCase))
                {
                    return url.Replace(destRootUrl, "");
                }
                else
                {
                    return url;
                }
            }
            string[] splitDestBaseUrl = this.destBaseUrl.Split(new string[] { "/", "//" }, StringSplitOptions.RemoveEmptyEntries);
            string[] splitUrl = url.Split(new string[] { "/", "//" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder relativeUrl = new StringBuilder();
            bool[] isSame = new bool[splitDestBaseUrl.Length];
            for (int i = 0; i < splitUrl.Length; i++)
            {
                if (splitDestBaseUrl[i].Equals(splitUrl[i], StringComparison.OrdinalIgnoreCase))
                {
                    isSame[i] = true;
                }
                else
                {
                    isSame[i] = false;
                }
            }
            int tempint = 1;
            if (this.infoPathType == InfoPathPublishType.List)
            {
                tempint = 0;
            }
            for (int i = 0; i < isSame.Length; i++)
            {
                if (!isSame[i])
                {
                    for (int j = 0; j < splitDestBaseUrl.Length - tempint - i; j++)
                    {
                        relativeUrl.Append("../");
                    }
                    for (int k = i; k < splitUrl.Length; k++)
                    {
                        relativeUrl.Append(splitUrl[k]);
                        relativeUrl.Append("/");
                    }
                    break;
                }
            }
            return relativeUrl.ToString();
        }

        //public string GetRootUrl(string url)
        //{
        //    using (new AvePerformanceScope("Common.InfoPathUrlConvertProcess.GetRootUrl"))
        //    {
        //        StringBuilder stringBuilder = new StringBuilder();
        //        try
        //        {
        //            string[] strs = url.Split(new string[] { "/", "//" }, StringSplitOptions.RemoveEmptyEntries);
        //            stringBuilder.Append(strs[0]);
        //            stringBuilder.Append("//");
        //            stringBuilder.Append(strs[1]);
        //        }
        //        catch (Exception e)
        //        {
        //            log.Warn("An error occurred when GetRootUrl ,GetRootUrl: " + url, e.ToString());
        //            throw;
        //        }
        //        return stringBuilder.ToString();
        //    }
        //}

        public string ReplaceGuid(string id)
        {
            if (mappingManager.SiteMappingManager.ListIdMapping.ContainsKey(new Guid(id)))
            {
                return mappingManager.SiteMappingManager.ListIdMapping[new Guid(id)].ToString();
            }
            return id;
        }

        public string ReplaceContentTypeId(Guid listId, string sourceContntTypeId)
        {
            if (mappingManager.SiteMappingManager.ListContentTypeIdMapping.ContainsKey(listId))
            {
                if (mappingManager.SiteMappingManager.ListContentTypeIdMapping[listId].ContainsKey(sourceContntTypeId))
                {
                    return mappingManager.SiteMappingManager.ListContentTypeIdMapping[listId][sourceContntTypeId];
                }
            }
            return sourceContntTypeId;
        }

        public string ReplaceOriginalUrl(string url)
        {
            string newUrl = url;
            newUrl = GetAbsoluteUrl(url);
            if (string.IsNullOrEmpty(this.sourceBaseUrl))
            {
                sourceRootUrl = new Uri(url).GetLeftPart(UriPartial.Authority);//GetRootUrl(url);
            }
            newUrl = newUrl.Replace(sourceRootUrl, "");
            newUrl = HttpUtility.UrlDecode(newUrl);
            if (newUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                newUrl = newUrl.Remove(newUrl.LastIndexOf("/", StringComparison.OrdinalIgnoreCase), 1);
            }
            if (!newUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                newUrl = "/" + newUrl;
            }
            if (mappingManager.SiteMappingManager.ListUrlMapping.ContainsKey(newUrl))
            {
                newUrl = mappingManager.SiteMappingManager.ListUrlMapping[newUrl];
                return destRootUrl + newUrl;
            }
            else if (mappingManager.SiteMappingManager.WebUrlMapping.ContainsKey(newUrl))
            {
                newUrl = mappingManager.SiteMappingManager.WebUrlMapping[newUrl];
                return destRootUrl + newUrl;
            }
            return sourceRootUrl + newUrl;
        }
    }

}