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
using AvePoint.GCommon;
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.ObjectModel.WebServiceCore
{
    internal static class WebPartWebServiceHandler
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(WebPartWebServiceHandler));
        public static string WebPagePagesGetWebPartOnPage(this IAveWebServiceNetWork network, string pageFullUrl)
        {
            XmlNode webPartProperties = GetWebPartProperties(network, pageFullUrl);
            XmlNamespaceManager nsmgr = null;
            if (webPartProperties.HasChildNodes)
            {
                nsmgr = new XmlNamespaceManager(webPartProperties.FirstChild.OwnerDocument.NameTable);
                nsmgr.AddNamespace("v2LV", "http://schemas.microsoft.com/WebPart/v2/ListView");
                nsmgr.AddNamespace("v2", "http://schemas.microsoft.com/WebPart/v2");
            }

            string sWppHtml = null;
            string pageContent = null;
            List<Dictionary<string, string>> webPartPropertiesList = null;
            string[] properties = new string[] { "ID", "__WebPartId", "ExportMode", "IsIncluded", "IsClosed", "AssemblyFullName", "TypeFullName", "SolutionId", "FormLocation", "ContentTypeId" };// "Title", "Description",

            //此处处理webpart子节点namespace的问题
            XmlNode webPart = webPartProperties.OwnerDocument.CreateNode(XmlNodeType.Element, webPartProperties.Name, webPartProperties.NamespaceURI);
            foreach (XmlNode node7 in webPartProperties.ChildNodes.OfType<XmlElement>())
            {
                try
                {
                    if (string.IsNullOrEmpty(sWppHtml))
                    {
                        string documentName = pageFullUrl;
                        sWppHtml = network.GetWebPartPage(documentName);
                        //pageContent = GetPageConentInEditMode(documentName);
                        //这个会导致page被checkout，尽管我们有disable force checkout的逻辑，但是不清楚客户 哪里为啥会出现这个问题。
                        webPartPropertiesList = AveHtmlUtility.CollectZoneIdAndPartOrders(sWppHtml, pageContent, properties);
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Get webpart page html failed.Url:{0}.Error Message:{1}", pageFullUrl, ex.ToString());
                    return webPartProperties.OuterXml;
                }
                string[] needProperties = new string[] { "IsIncluded", "WebPartIdProperty" };
                Dictionary<string, string> tempwebpartProps = null;
                if (node7.Attributes != null && node7.Attributes["ID"] != null)
                {
                    string Id = node7.Attributes["ID"].Value;
                    //之前的两个foreach会导致webPartPropertiesList中所有元素的属性都为最后一个webpart的
                    foreach (Dictionary<string, string> webpartProps in webPartPropertiesList)
                    {
                        if (webpartProps.Count <= 0)
                        {
                            continue;
                        }
                        string webpartid = string.Empty;
                        if (webpartProps.ContainsKey("__WebPartId"))
                        {
                            webpartid = webpartProps["__WebPartId"].Trim('{', '}');
                        }
                        else if (webpartProps.ContainsKey("ID"))
                        {
                            string strWebpart = string.Empty;
                            if (webpartProps["ID"].StartsWith("g_", StringComparison.OrdinalIgnoreCase))
                            {
                                strWebpart = webpartProps["ID"].Substring(2).Replace("_", "-");
                            }
                            else
                            {
                                strWebpart = webpartProps["ID"].Trim(new char[] { '{', '}' });
                            }
                            if (!AveTypeHelper.IsGuid(strWebpart))
                            {
                                continue;
                            }
                            webpartid = strWebpart;
                        }
                        if (Id.Equals(webpartid, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                tempwebpartProps = webpartProps;
                                HandleWebPartProperties(node7, webpartProps, new string[] { "ExportMode", "Title", "AssemblyFullName", "TypeFullName", "SolutionId", "FormLocation", "ContentTypeId" });
                                FormatWebpartPropertyInCache(node7, webpartProps);
                                AddPropertiesToWebpartDefinationXml(node7, needProperties, webpartProps);
                            }
                            catch (Exception ex)
                            {
                                logger.Warn("Get the webpart({0}) property ZoneID and PartOrder failed:{1}", Id, ex.ToString());
                            }
                            break;
                        }
                    }
                    XmlNode newNode = null;
                    //此处为了还原时格式的正确，v3的节点必须要有webParts节点，v2则不需要
                    if (node7.NamespaceURI.Equals("http://schemas.microsoft.com/WebPart/v2"))
                    {
                        newNode = node7.OwnerDocument.CreateNode(XmlNodeType.Element, node7.Name, node7.NamespaceURI);
                    }
                    else
                    {
                        newNode = node7.OwnerDocument.CreateNode(XmlNodeType.Element, "webParts", node7.NamespaceURI);
                    }
                    XmlAttribute nodeAttribute = node7.OwnerDocument.CreateAttribute("ID");
                    nodeAttribute.Value = node7.Attributes["ID"].Value;
                    newNode.Attributes.Append(nodeAttribute);
                    //innerXML会有namespace，故改用append，规范xmlnamespace，此处的namespace不可用string.replace，可能会有namespace前半段和该被替换的namespace重合的情况出现，造成还原抛异常
                    for (int i = 0; i < node7.ChildNodes.Count; i++)
                    {
                        XmlNode tempNode = node7.ChildNodes[i].CloneNode(true);
                        newNode.AppendChild(tempNode);
                        tempNode = null;
                    }
                    if (tempwebpartProps != null)
                    {
                        newNode = ConvertSandBoxWebpart(newNode, needProperties, tempwebpartProps);
                    }
                    webPart.AppendChild(newNode);
                }
            }
            //将根节点的namespace替换掉，也是为了xml的格式问题。
            if (webPart.ChildNodes.Count == 0)
            {
                return null;
            }
            return webPart.OuterXml.ToString().Replace(webPart.NamespaceURI, "").Replace("xmlns=\"\"", "");
        }

        private static XmlNode GetWebPartProperties(IAveWebServiceNetWork network,string pageFullUrl)
        {
            XmlNode webPartProperties = null;
            try
            {
                webPartProperties = network.GetWebPartProperties2(pageFullUrl);
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    HttpWebResponse exceptionReponse = (e as WebException).Response as HttpWebResponse;
                    if (exceptionReponse != null
                        && (exceptionReponse.StatusCode == HttpStatusCode.Forbidden || exceptionReponse.StatusCode == HttpStatusCode.Unauthorized))
                    {
                        //throw new AveSecurityTrimingException("The request failed with HTTP status 401: Unauthorized.", e);
                        throw;
                    }
                }
                try
                {
                    logger.Warn("Get webpart:{0} failed.Error Message:{1}", pageFullUrl, e.ToString());
                    webPartProperties = network.GetWebPartProperties(pageFullUrl);
                }
                catch (Exception ex)
                {
                    if (webPartProperties?.LastChild is XmlComment)
                    {
                        throw new Exception("Fetching web parts for page " + pageFullUrl + " failed. " + ((XmlComment)webPartProperties.LastChild).Value + ".Error Message:" + ex.ToString());
                    }
                }
            }
            return webPartProperties;
        }

        /// <summary>
        /// Add webpart properties to webpart information xml
        /// </summary>
        /// <param name="node">webpart information xml's root node</param>
        /// <param name="webpartProps">properties get from file's content</param>
        /// <param name="properties">properties need to be add to xml</param>
        private static void HandleWebPartProperties(XmlNode node, Dictionary<string, string> webpartProps, string[] properties)
        {
            try
            {
                foreach (string property in properties)
                {
                    if (!webpartProps.ContainsKey(property))
                    {
                        continue;
                    }
                    XmlNode tempNode = node.SelectSingleNode(".//*[name() = 'properties']");
                    if (tempNode != null)
                    {
                        bool finded = false;
                        foreach (XmlNode childNode in tempNode.ChildNodes)
                        {
                            if (childNode.Attributes == null)
                            { continue; }
                            if (property.Equals(childNode.Attributes["name"].Value, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(webpartProps[property]) && string.IsNullOrEmpty(childNode.InnerText))
                            {
                                childNode.InnerText = webpartProps[property];
                                finded = true;
                                break;
                            }
                        }
                        if (!finded)
                        {
                            XmlElement propertyNode = tempNode.OwnerDocument.CreateElement("property", tempNode.NamespaceURI) as XmlElement;
                            propertyNode.SetAttribute("name", property);
                            propertyNode.InnerText = webpartProps[property];
                            tempNode.AppendChild(propertyNode);
                        }
                    }
                    else
                    {
                        tempNode = node.SelectSingleNode(".//*[name() = '" + property + "']");
                        if (tempNode != null)
                        {
                            if (property.Equals(tempNode.Name, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(webpartProps[property]) && string.IsNullOrEmpty(tempNode.InnerText))
                            {
                                tempNode.InnerText = webpartProps[property];
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn(AveWebServiceRequestResource.HandleWebPartPropertiesError, "Title" + " Description" + " ExportMode", node.OuterXml, ex.ToString());
            }
        }

        private static void FormatWebpartPropertyInCache(XmlNode root, Dictionary<string, string> webpartProps)
        {
            if (!webpartProps.ContainsKey("IsIncluded") && webpartProps.ContainsKey("IsClosed"))
            {
                webpartProps["IsIncluded"] = webpartProps["IsClosed"].Equals(Boolean.FalseString, StringComparison.OrdinalIgnoreCase) ? "true" : "false";
            }
            if (webpartProps.ContainsKey("ID"))
            {
                string strWebpart = string.Empty;
                if (webpartProps["ID"].StartsWith("g_", StringComparison.OrdinalIgnoreCase))
                {
                    strWebpart = webpartProps["ID"].Substring(2).Replace("_", "-");
                }
                else
                {
                    strWebpart = webpartProps["ID"].Trim(new char[] { '{', '}' });
                }
                if (AveTypeHelper.IsGuid(strWebpart))
                {
                    webpartProps["WebPartIdProperty"] = strWebpart;
                    return;
                }
            }
            XmlNode Id = root.SelectSingleNode(".//*[name()='ID']");
            if (Id == null || !AveTypeHelper.IsGuid(Id.InnerText.TrimStart('g').Replace("_", "")))
            {
                Id = root.SelectSingleNode(".//*[@name='ID']");
            }
            if (Id != null)
            {
                webpartProps["WebPartIdProperty"] = new Guid(Id.InnerText.TrimStart('g').Replace("_", "")).ToString();
            }
        }

        private static XmlNode ConvertSandBoxWebpart(XmlNode node, string[] needProperties, Dictionary<string, string> webpartProps)
        {
            string defaultNamespace = "http://microsoft.com/sharepoint/webpartpages";
            IWebPartPropertyExtractor extractor = WebPartExtractorFactory.Create(node.OuterXml, defaultNamespace);
            if (extractor != null && extractor.TypeFullName.StartsWith("Microsoft.SharePoint.WebPartPages.SPUserCodeWebPart"))
            {
                string sandboxWebpart = "<webPart xmlns=\"http://schemas.microsoft.com/WebPart/v3\"><metaData><type name=\"{0}\" /><importErrorMessage>$Resources:core,ImportErrorMessage;</importErrorMessage><Solution SolutionId=\"{1}\" xmlns=\"http://schemas.microsoft.com/sharepoint/\" /></metaData><data><properties></properties></data></webPart>";
                XmlElement elment = node.OwnerDocument.CreateElement("webParts");
                elment.InnerXml = string.Format(sandboxWebpart, webpartProps["TypeFullName"] + "," + webpartProps["AssemblyFullName"], extractor.GetProperty("SolutionId"));
                if (node.Attributes["ID"] != null)
                {
                    elment.SetAttribute("ID", node.Attributes["ID"].Value);
                }
                XmlElement originalPropertiesNode = node.SelectSingleNode(".//*[name() = 'properties']") as XmlElement;
                if (originalPropertiesNode != null)
                {
                    XmlElement propertiesNode = elment.SelectSingleNode(".//*[name() = 'properties']") as XmlElement;
                    propertiesNode.InnerXml = originalPropertiesNode.InnerXml;
                }
                AddPropertiesToWebpartDefinationXml(elment.FirstChild, needProperties, webpartProps);
                return elment;
            }
            return node;
        }

        private static void AddPropertiesToWebpartDefinationXml(XmlNode root, string[] needProperties, Dictionary<string, string> webpartProps)
        {
            foreach (string property in needProperties)
            {
                if (!webpartProps.ContainsKey(property))
                {
                    continue;
                }
                if (root.NamespaceURI.Equals("http://schemas.microsoft.com/WebPart/v2"))
                {
                    XmlNode node = root.SelectSingleNode(".//*[name()='" + property + "']");
                    if (node == null)
                    {
                        XmlElement tempElement = root.OwnerDocument.CreateElement(property);
                        node = root.AppendChild(tempElement);
                    }
                    bool boolValue = false;
                    if (!string.IsNullOrEmpty(webpartProps[property]) && bool.TryParse(webpartProps[property], out boolValue))
                    {
                        node.InnerText = webpartProps[property].ToLowerInvariant();
                    }
                    else
                    {
                        node.InnerText = webpartProps[property];
                    }
                }
                else
                {
                    XmlElement tempElement = root.OwnerDocument.CreateElement(property);
                    tempElement.InnerText = webpartProps[property];
                    root.AppendChild(tempElement);
                }
            }
        }



    }
}
