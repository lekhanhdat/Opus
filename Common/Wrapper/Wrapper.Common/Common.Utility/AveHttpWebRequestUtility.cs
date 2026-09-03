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
using System.IO;
using System.Net;
using System.Xml;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;
using Microsoft365.Authentication;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Microsoft365.SharePoint.Extension;

namespace AvePoint.Wrapper.Common
{
    public class AveHttpWebRequestUtility
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static string mUnauthorizedMessage = "The remote server returned an error: (401) Unauthorized.";
        public static object HeaderInfo { get; private set; }
        [ThreadStatic]
        public static Exception LastException;

        public static string HttpGet(string url, ITokenProvider provider, SecurityTrimObject parentTrimObj = null, params string[] properties)
        {
            return HttpGet(url, provider, false, parentTrimObj, properties);
        }

        public static string HttpGet(string url, ITokenProvider provider, bool allowAutoRedirect, SecurityTrimObject parentTrimObj = null, params string[] properties)
        {
            LastException = null;
            string sharePointPageHtml = null;
            ReliableHttpWebRequest httpGetRequest = ReliableHttpWebRequest.CreateRequest(url) as ReliableHttpWebRequest;
            httpGetRequest.SetTokenProvider(url, provider, false);
            httpGetRequest.Method = "GET";
            httpGetRequest.AllowAutoRedirect = allowAutoRedirect;
            httpGetRequest.Timeout = WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestTimeout;
            try
            {
                WebResponse response = httpGetRequest.GetResponse();
                if (response != null)
                {
                    sharePointPageHtml = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                    HeaderInfo = response.Headers;
                    response.Close();
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    ex.Response.Close();
                }
                if (ex.Message.Equals(mUnauthorizedMessage, StringComparison.OrdinalIgnoreCase))
                {
                    if (parentTrimObj != null)
                    {
                        foreach (string property in properties)
                        {
                            if (!parentTrimObj.TrimmedProperties.ContainsKey(property))
                            {
                                parentTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", mUnauthorizedMessage, url);
                            }
                        }
                    }
                    throw new AveSecurityTrimingException(ex.Message, ex);
                }
                else
                {
                    log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCGetSPPageHtmlError, ex.ToString());
                    LastException = ex;
                }
            }
            return sharePointPageHtml;
        }

        public static void HttpPost(string url, ITokenProvider tokenProvider, string contentType, byte[] body, Dictionary<string, object> headerInformation)
        {
            LastException = null;
            ReliableHttpWebRequest httpPostRequest = ReliableHttpWebRequest.CreateRequest(url) as ReliableHttpWebRequest;
            httpPostRequest.SetTokenProvider(url, tokenProvider, false);
            httpPostRequest.AllowAutoRedirect = false;
            httpPostRequest.ContentType = contentType;
            httpPostRequest.Method = "Post";
            httpPostRequest.ContentLength = body.Length;
            httpPostRequest.Timeout = 10 * 60 * 1000;
            httpPostRequest.ReadWriteTimeout = 10 * 60 * 1000;
            if (headerInformation != null && headerInformation.Count > 0)
            {
                foreach (string key in headerInformation.Keys)
                {
                    httpPostRequest.Headers.Add(key, headerInformation[key].ToString());
                }
            }
            try
            {
                Stream stream = httpPostRequest.GetRequestStream();
                stream.Write(body, 0, body.Length);
                stream.Close();
                HttpWebResponse response = httpPostRequest.GetResponse() as HttpWebResponse;
                if (response != null)
                {
                    response.Close();
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    ex.Response.Close();
                }
                log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCSendSPPageHtmlError, ex.ToString());
                if (ex.Message.Contains(mUnauthorizedMessage))
                {
                    throw new AveSecurityTrimingException(mUnauthorizedMessage, ex);
                }
                LastException = ex;
            }
        }

        public static string HttpReturn(string url, ITokenProvider tokenProvider, string contentType, byte[] body, Dictionary<string, object> headerInformation, string userAgent = "")
        {
            LastException = null;
            string postHtml = null;
            ReliableHttpWebRequest httpPostRequest = ReliableHttpWebRequest.CreateRequest(url) as ReliableHttpWebRequest;
            httpPostRequest.SetTokenProvider(url, tokenProvider, false);
            httpPostRequest.ContentType = contentType;
            httpPostRequest.Method = "Post";
            httpPostRequest.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(userAgent))
            {
                httpPostRequest.UserAgent = userAgent;
            }
            httpPostRequest.ContentLength = body.Length;
            httpPostRequest.Timeout = 10 * 60 * 1000;
            httpPostRequest.ReadWriteTimeout = 10 * 60 * 1000;
            if (headerInformation != null && headerInformation.Count > 0)
            {
                foreach (string key in headerInformation.Keys)
                {
                    httpPostRequest.Headers.Add(key, headerInformation[key].ToString());
                }
            }
            try
            {
                Stream stream = httpPostRequest.GetRequestStream();
                stream.Write(body, 0, body.Length);
                stream.Close();
                HttpWebResponse response = httpPostRequest.GetResponse() as HttpWebResponse;
                if (response != null)
                {
                    long length = response.ContentLength;
                    postHtml = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    response.Close();
                }
            }
            catch (WebException ex)
            {
                log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCSendSPPageHtmlError, ex.ToString());
                LastException = ex;
            }
            return postHtml;
        }

        public static Dictionary<string, object> GetInput(string html, string searchContent, Dictionary<string, object> inputDic)
        {
            List<string> inputList = new List<string>();
            int index = 0;
            index = html.IndexOf(searchContent, index, StringComparison.OrdinalIgnoreCase);
            while (index >= 0)
            {
                int endIndex = html.IndexOf("/>", index, StringComparison.OrdinalIgnoreCase) + 1;
                string information = html.Substring(index, endIndex - index + 1);
                inputList.Add(information);
                index = html.IndexOf(searchContent, index + 1, StringComparison.OrdinalIgnoreCase);
            }
            inputDic = GetInputDic(inputList, inputDic);
            return inputDic;
        }

        public static Dictionary<string, object> GetPostFormValues(string html, string formId = "aspnetForm", bool encoded = true)
        {
            return GetPostFormValues(html, encoded, formId);
        }

        public static Dictionary<string, object> GetPostFormValues(string html, bool encoded, string formId = "aspnetForm")
        {
            Dictionary<string, object> formValues = new Dictionary<string, object>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode formNode = doc.GetElementbyId(formId);
            if (formNode != null)
            {
                CollectInputControlValues(formNode, "//input[@type='hidden']", encoded, formValues);
                //CollectInputControlValues(formNode, "//input[@type='submit']", encoded, formValues);
                CollectInputControlValues(formNode, "//input[@type='text']", encoded, formValues);
                CollectInputControlValues(formNode, "//input[@type='radio'][@checked='checked']", encoded, formValues);
                CollectInputControlValues(formNode, "//input[@type='checkbox'][@checked='checked']", encoded, formValues);
                CollectSelectControlValues(formNode, encoded, formValues);
            }

            return formValues;
        }

        /// <summary>
        /// checkbox,radio buttion,selected node
        /// </summary>
        /// <param name="html"></param>
        /// <param name="encoded"></param>
        /// <param name="formId"></param>
        /// <returns>return null if met error,else return a list</returns>
        public static List<string> LoadCheckedControls(string html, bool encoded, string formId = "aspnetForm")
        {
            
            Dictionary<string, object> formValues = new Dictionary<string, object>();
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                HtmlNode formNode = doc.GetElementbyId(formId);
                if (formNode != null)
                {
                    CollectInputControlValues(formNode, "//input[@type='radio'][@checked='checked']", encoded, formValues);
                    CollectInputControlValues(formNode, "//input[@type='checkbox'][@checked='checked']", encoded, formValues);
                    CollectSelectControlValues(formNode, encoded, formValues);
                }

                return formValues.Keys.ToList();
            }
            catch (Exception e)
            {
                log.Warn("LoadCheckedControls failed.Error:{0} ",e);
            }
            return null;
        }

        /// <summary>
        /// for add slide folder
        /// </summary>
        /// <param name="html"></param>
        /// <param name="encoded"></param>
        /// <param name="includeSubmit"></param>
        /// <param name="formId"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetPostFormValues(string html, bool encoded, bool includeSubmit, string formId = "aspnetForm")
        {
            Dictionary<string, object> formValues = new Dictionary<string, object>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode formNode = doc.GetElementbyId(formId);
            if (formNode != null)
            {
                CollectInputControlValues(formNode, "//input[@type='hidden']", encoded, formValues);
                if (includeSubmit)
                {
                    CollectInputControlValues(formNode, "//input[@type='submit']", encoded, formValues);
                }
                CollectInputControlValues(formNode, "//input[@type='text']", encoded, formValues);
                CollectInputControlValues(formNode, "//input[@type='radio'][@checked='checked']", encoded, formValues);
                CollectInputControlValues(formNode, "//input[@type='checkbox'][@checked='checked']", encoded, formValues);
                CollectSelectControlValues(formNode, encoded, formValues);
            }

            return formValues;
        }

        public static void CollectInputControlValues(HtmlNode formNode, string pattern, bool encoded, Dictionary<string, object> formValues)
        {
            HtmlNodeCollection inputNodes = formNode.SelectNodes(pattern);
            if (inputNodes != null)
            {
                foreach (HtmlNode inputNode in inputNodes)
                {
                    string name = inputNode.GetAttributeValue("name", string.Empty);
                    if (!string.IsNullOrEmpty(name))
                    {
                        string value = inputNode.GetAttributeValue("value", string.Empty);
                        formValues[name] = encoded ? HttpUtility.UrlEncode(value) : value;
                    }
                }
            }
        }

        private static void CollectSelectControlValues(HtmlNode formNode, bool encoded, Dictionary<string, object> formValues)
        {
            HtmlNodeCollection inputNodes = formNode.SelectNodes("//select");
            if (inputNodes != null)
            {
                foreach (HtmlNode inputNode in inputNodes)
                {
                    if (inputNode.ChildNodes != null && inputNode.FirstElementChild != null)
                    {
                        string value = inputNode.FirstElementChild.GetAttributeValue("value", string.Empty);

                        foreach (HtmlNode optionNode in inputNode.ChildNodes)
                        {
                            if ("selected".Equals(optionNode.GetAttributeValue("selected", string.Empty), StringComparison.OrdinalIgnoreCase))
                            {
                                value = optionNode.GetAttributeValue("value", string.Empty);
                                break;
                            }
                        }

                        formValues[inputNode.GetAttributeValue("name", string.Empty)] = encoded ? HttpUtility.UrlEncode(value) : value;
                    }
                }
            }
        }        

        /// <summary>
        /// 在一个string串中查找一段字符串
        /// </summary>
        /// <param name="html"></param>
        /// <param name="searchContent">开始串</param>
        /// <param name="endContent">结束串</param>
        /// <returns></returns>
        public static string GetInput(string html, string searchContent, string endContent)
        {
            int index = 0;
            index = html.IndexOf(searchContent, index, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                int endIndex = html.IndexOf(endContent, index, StringComparison.OrdinalIgnoreCase) + endContent.Length;
                return html.Substring(index, endIndex - index);
            }
            return string.Empty;
        }

        /// <summary>
        /// 在一个string串中,在一个指定位置之后，查找一段字符串
        /// </summary>
        /// <param name="html"></param>
        /// <param name="searchContent">开始串</param>
        /// <param name="endContent">结束串</param>
        /// <returns></returns>
        public static string GetInput(string html, int startIndex, string searchContent, string endContent)
        {
            int index = html.IndexOf(searchContent, startIndex, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                int endIndex = html.IndexOf(endContent, index, StringComparison.OrdinalIgnoreCase) + endContent.Length;
                return html.Substring(index, endIndex - index);
            }
            return string.Empty;
        }

        public static List<string> GetProperties(string html, int startIndex, string searchContent, string endContent)
        {
            List<string> result = new List<string>();
            int index = html.IndexOf(searchContent, startIndex, StringComparison.OrdinalIgnoreCase) + searchContent.Length;
            int endIndex = -1;
            while (index >= searchContent.Length)
            {
                endIndex = html.IndexOf(endContent, index, StringComparison.OrdinalIgnoreCase);
                result.Add(html.Substring(index, endIndex - index));
                html = html.Substring(endIndex);
                index = html.IndexOf(searchContent, startIndex, StringComparison.OrdinalIgnoreCase) + searchContent.Length;
            }
            return result;
        }

        private static Dictionary<string, object> GetInputDic(List<string> strList, Dictionary<string, object> inputDic)
        {
            string key = string.Empty;
            string value = string.Empty;
            foreach (string str in strList)
            {
                key = GetValue(str, "name=");
                value = GetValue(str, "value=");
                inputDic[key] = value;
            }
            return inputDic;
        }

        public static void GetNodesProperties(string html, string searchContent, Dictionary<string, object> inputDic)
        {
            List<string> inputList = new List<string>();
            int index = 0;
            if (!string.IsNullOrEmpty(html))
            {
                index = html.IndexOf(searchContent, index, StringComparison.OrdinalIgnoreCase);
                while (index >= 0)
                {
                    int endIndex = html.IndexOf(");", index, StringComparison.OrdinalIgnoreCase) + 1;
                    inputList.Add(html.Substring(index + searchContent.Length, endIndex - index - searchContent.Length - 1));
                    index = html.IndexOf(searchContent, index + 1, StringComparison.OrdinalIgnoreCase);
                }
            }
            GetInputDicForNavigationNodeProperties(inputList, inputDic);
        }

        private static void GetInputDicForNavigationNodeProperties(List<string> strList, Dictionary<string, object> inputDic)
        {
            foreach (string str in strList)
            {
                Dictionary<string, object> nodeProp = new Dictionary<string, object>();
                List<string> nodeStrs = JsonConvert.DeserializeObject<List<string>>("[" + str + "]");
                nodeProp["NodeType"] = nodeStrs[4];
                nodeProp["NodeUrl"] = nodeStrs[2];
                nodeProp["Description"] = nodeStrs[3];
                nodeProp["Audience"] = nodeStrs[7];
                nodeProp["IsVisible"] = "visible".Equals(nodeStrs[5].ToLowerInvariant());
                nodeProp["Target"] = nodeStrs[6];
                inputDic[nodeStrs[0]] = nodeProp;
            }
        }

        public static string GetValue(string pStr, string str)
        {
            int index = pStr.IndexOf(str, StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                int startIndex = index + str.Length + 1;
                int endIndex = pStr.IndexOf("\"", startIndex, StringComparison.OrdinalIgnoreCase);
                if ((endIndex - startIndex) == 0)
                {
                    return string.Empty;
                }
                else if (endIndex < 0)
                {
                    endIndex = pStr.IndexOf("'", startIndex, StringComparison.OrdinalIgnoreCase);
                    if (endIndex > startIndex)
                    {
                        return pStr.Substring(startIndex, endIndex - startIndex);
                    }
                }
                else
                {
                    return pStr.Substring(startIndex, endIndex - startIndex);
                }
            }
            return string.Empty;
        }

        public static byte[] GetByte(Dictionary<string, object> inputDic, string inputOrder)
        {
            string body = string.Empty;
            if (inputDic != null && inputDic.Count > 0)
            {
                int i = 0;
                foreach (string key in inputDic.Keys)
                {
                    if (i == 0)
                    {
                        body = key + "=" + inputDic[key];
                        i++;
                    }
                    else
                    {
                        body = body + "&" + key + "=" + inputDic[key];
                    }
                }
            }
            else if (!string.IsNullOrEmpty(inputOrder))
            {
                body = inputOrder;
            }
            byte[] bodyData = Encoding.UTF8.GetBytes(body);//SAAS-462 当为日语的时候Encoding.Default出现问题.
            return bodyData;
        }

        public static byte[] GetMiltiByte(Dictionary<string, object> inputDic, string boundary)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string key in inputDic.Keys)
            {
                sb = sb.Append("--");
                sb = sb.Append(boundary);
                sb = sb.Append("\r\n");
                sb = sb.Append("Content-Disposition: form-data; name=\"" + key + "\"\r\n\r\n");
                sb = sb.Append(inputDic[key].ToString());
                sb = sb.Append("\r\n");
            }
            sb.Append("--" + boundary + "--\r\n");
            byte[] bodyData = Encoding.UTF8.GetBytes(sb.ToString());
            return bodyData;
        }

        public static string GetFeatureTarget(string html, string id)
        {
            string featureList = string.Empty;
            try
            {
                int index = html.IndexOf("<div id='" + id + "'", StringComparison.OrdinalIgnoreCase);
                int startIndex = html.IndexOf("name=", index + 1, StringComparison.OrdinalIgnoreCase) + 6;
                int endIdex = html.IndexOf("\"", startIndex, StringComparison.OrdinalIgnoreCase);
                featureList = html.Substring(startIndex, endIdex - startIndex);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetFeatureTargetError, e.ToString());
                return string.Empty;
            }
            return featureList;
        }

        public static string GetComponentValue(string html, string searchContent)
        {
            int index = 0;
            index = html.IndexOf(searchContent, index, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                int endIndex = html.IndexOf("/>", index, StringComparison.OrdinalIgnoreCase) + 1;
                string information = html.Substring(index, endIndex - index + 1);
                if (information.Contains("value"))
                {
                    int startIndex = information.IndexOf("value", 0, StringComparison.OrdinalIgnoreCase) + 7;
                    information = information.Substring(startIndex);
                    int endIdex = 0;
                    if (information.Contains("\" maxlength"))
                    {
                        endIdex = information.IndexOf("\" maxlength");
                    }
                    else
                    {
                        endIdex = information.IndexOf("\" size");
                    }
                    return information.Substring(0, endIdex);
                }
            }
            return string.Empty;
        }

        public static string GetInnerText(string html, string searchContent, string endContent)
        {
            int index = 0;
            index = html.IndexOf(searchContent, index, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                index += searchContent.Length;
                int endIndex = html.IndexOf(endContent, index, StringComparison.OrdinalIgnoreCase);
                return html.Substring(index, endIndex - index);
            }
            return string.Empty;
        }

        public static string GetSelectInputValue(string html, string searchContent)
        {
            //searchContetn like <select……>
            Dictionary<string, object> selectDic = new Dictionary<string, object>();
            string endContent = "</select>";
            int startIndex = html.IndexOf(searchContent, StringComparison.OrdinalIgnoreCase);
            if (startIndex > 0)
            {
                int endIndex = html.IndexOf(endContent, startIndex, StringComparison.OrdinalIgnoreCase) + endContent.Length;
                string content = html.Substring(startIndex, endIndex - startIndex);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);
                foreach (XmlNode node in doc.FirstChild.ChildNodes)
                {
                    if (node.Attributes.Count > 1)
                    {
                        return node.Attributes["value"].Value;
                    }
                }
            }
            return null;
        }

        public static Dictionary<string, object> GetSelectInput(string html, string searchContent)
        {
            //searchContetn like <select……>
            Dictionary<string, object> selectDic = new Dictionary<string, object>();
            string endContent = "</select>";
            int startIndex = html.IndexOf(searchContent, StringComparison.OrdinalIgnoreCase);
            if (startIndex > 0)
            {
                int endIndex = html.IndexOf(endContent, startIndex, StringComparison.OrdinalIgnoreCase) + endContent.Length;
                string content = html.Substring(startIndex, endIndex - startIndex);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);
                foreach (XmlNode node in doc.FirstChild.ChildNodes)
                {
                    if (node.Attributes.Count > 1)
                    {
                        string value = node.Attributes["value"].Value;
                        string text = node.InnerText;
                        selectDic["Value"] = value;
                        selectDic["Text"] = text;
                        return selectDic;
                    }
                }
            }
            return null;
        }

        public static bool GetCheckInput(string html, string searchContent)
        {
            string endContent = "/>";
            int index = 0;
            index = html.IndexOf(searchContent, index, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                int endIndex = html.IndexOf(endContent, index, StringComparison.OrdinalIgnoreCase) + endContent.Length;
                string checkContent = html.Substring(index, endIndex - index);
                if (checkContent.Contains("checked=\"checked\""))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public static void GetInput(string html, string searchContent, string endContent, List<string> keyWordsNames)
        {
            int index = 0;
            index = html.IndexOf(searchContent, index, StringComparison.OrdinalIgnoreCase);
            while (index >= 0)
            {
                int endIndex = html.IndexOf(endContent, index, StringComparison.OrdinalIgnoreCase);
                string information = html.Substring(index + searchContent.Length, endIndex - index - searchContent.Length);
                keyWordsNames.Add(information);
                index = html.IndexOf(searchContent, index + 1, StringComparison.OrdinalIgnoreCase);
            }
        }
        public static void UpateAlertTimeProperties(Dictionary<string, object> data, string html)
        {
            if (data.ContainsKey("NotifyFreq"))
            {
                DateTime time = (DateTime)data["NotifyTime"];
                data["Day"] = (int)time.DayOfWeek;
                if (html.Contains("23:00"))
                {
                    data["Time"] = time.ToString("HH:00");
                }
                else
                {
                    string strTime = time.ToString("h:00");
                    if (time.ToString("yyyy-MM-dd hh:mm").Equals(time.ToString("yyyy-MM-dd HH:mm")))
                    {
                        strTime = strTime + " AM";
                    }
                    else
                    {
                        strTime = strTime + " PM";
                    }
                    data["Time"] = strTime;
                }
            }
            else
            {
                data["NotifyFreq"] = 0;
            }
        }
        public static string GetFilterValue(Dictionary<string, object> data)
        {
            if (data["AlertTemplateName"].Equals("SPAlertTemplateType.Tasks")
                || data["AlertTemplateName"].Equals("SPAlertTemplateType.DocumentLibrary")
                || data["AlertTemplateName"].Equals("SPAlertTemplateType.WebPageLibrary")
                || data["AlertTemplateName"].Equals("SPAlertTemplateType.DiscussionBoard"))
            {
                string properties = data["Properties"].ToString();
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(properties);
                XmlNode node = xDoc.DocumentElement.SelectSingleNode("property[@name='filterindex']");
                if ((data["AlertTemplateName"].Equals("SPAlertTemplateType.Tasks") && node.Attributes["value"].Value.Equals("8"))
                    || (data["AlertTemplateName"].Equals("SPAlertTemplateType.DocumentLibrary") && node.Attributes["value"].Value.Equals("4"))
                    || (data["AlertTemplateName"].Equals("SPAlertTemplateType.WebPageLibrary") && node.Attributes["value"].Value.Equals("4"))
                    || (data["AlertTemplateName"].Equals("SPAlertTemplateType.DiscussionBoard") && node.Attributes["value"].Value.Equals("4")))
                {
                    XmlNode node1 = xDoc.DocumentElement.SelectSingleNode("property[@name='viewid']");
                    //data["ViewId"] = node1.Attributes["value"].Value;
                    data["Filter"] = "Someone changes an item that appears in the following view:";
                }
            }
            return data["Filter"].ToString();
        }
        public static string GetInputControlValue(string html, string pattern)
        {
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(pattern))
            {
                return string.Empty;
            }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode inputControl = doc.DocumentNode.SelectSingleNode(pattern);
            string searchResult = null;
            if (inputControl != null)
            {
                searchResult = inputControl.GetAttributeValue("value", string.Empty);
            }
            return searchResult;
        }
        public static string GetPeoplePickerValue(Dictionary<string, object> user)
        {
            StringBuilder value = new StringBuilder();
            value.Append("&nbsp;");
            string userFormat = "<SPAN id=span<%ReplaceUser%> class=ms-entity-resolved onmouseover=this.contentEditable=false; title=<%ReplaceUser%> tabIndex=-1 onmouseout=this.contentEditable=true;contentEditable=true isContentType=\"true\">"
                                + "<DIV style=\"DISPLAY: none\" id=divEntityData description=\"<%ReplaceUser%>\" isresolved=\"True\" displaytext=\"<%ReplaceDisplayUser%>\" key=\"<%ReplaceUser%>\">"
                                + "<DIV data='<ArrayOfDictionaryEntry xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">"
                                + "<DictionaryEntry><Key xsi:type=\"xsd:string\">SPUserID</Key><Value xsi:type=\"xsd:string\"><%ReplaceUserID%></Value></DictionaryEntry>"
                                + "<DictionaryEntry><Key xsi:type=\"xsd:string\">AccountName</Key><Value xsi:type=\"xsd:string\"><%ReplaceUser%></Value></DictionaryEntry>"
                //+"<DictionaryEntry><Key xsi:type=\"xsd:string\">Email</Key><Value xsi:type=\"xsd:string\">lxzou@avepoint.com</Value></DictionaryEntry>"
                                + "<DictionaryEntry><Key xsi:type=\"xsd:string\">PrincipalType</Key><Value xsi:type=\"xsd:string\">User</Value></DictionaryEntry></ArrayOfDictionaryEntry>'></DIV></DIV>"
                                + "<SPAN id=content oncontextmenu=onContextMenuSpnRw(event,ctx); tabIndex=-1 contentEditable=true onmousedown=onMouseDownRw(event);><%ReplaceUser%> </SPAN></SPAN>;";
            string userInfo = userFormat.Replace("<%ReplaceUser%>", user["DisplayName"].ToString()).Replace("<%ReplaceUserID%>", user["UserId"].ToString()).Replace("<%ReplaceDisplayUser%>", user["Name"].ToString());
            return userInfo;
        }

        public static bool IsUrlAvailable(string url, ITokenProvider tokenProvider, bool allowAutoRedirect = false)
        {
            try
            {
                ReliableHttpWebRequest request = ReliableHttpWebRequest.CreateRequest(url) as ReliableHttpWebRequest;
                request.SetTokenProvider(url, tokenProvider, false);
                request.AllowAutoRedirect = allowAutoRedirect;
                using (request.GetResponse())
                {
                }
            }
            catch (WebException exception)
            {
                HttpWebResponse response = exception.Response as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                log.Warn("Check Url availability failed, url : \"{0}\", error detail : {1}", url, exception.ToString());
            }
            catch (Exception e)
            {
                log.Warn("Check Url availability failed, url : \"{0}\", error detail : {1}", url, e.ToString());
            }
            return true;
        }

        public static bool CheckSiteAvailability(string siteUrl)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(siteUrl);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpWebResponse.Close();
            }
            catch (WebException e)
            {
                log.Warn("Check availability for site:{0} failed:{1}", siteUrl, e);
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("Check availability for site:{0} failed:{1}", siteUrl, e);
            }
            return true;
        }
    }
}
