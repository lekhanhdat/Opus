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




namespace AveClientRequest.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Net;
    using System.Xml;
    using AvePoint.Wrapper.Common;
    using System.Web;

    public class AveHttpWebRequestUtility
    {
        private const string UserAgent = "User-Agent: Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; .NET4.0C; .NET4.0E)";

        public static string HttpGet(string url, object obj)
        {
            string sharePointPageHtml = null;
            AveHttpWebRequest httpGetRequest = AveHttpWebRequest.Create(url);
            httpGetRequest.UserAgent = UserAgent;
            NetworkCredential credential = obj as NetworkCredential;
            if (credential != null)
            {
                httpGetRequest.Credentials = credential;
            }
            else
            {
                httpGetRequest.CookieContainer = obj as CookieContainer;
            }
            httpGetRequest.Method = "GET";
            try
            {                
                WebResponse response = httpGetRequest.GetResponse();
                if (response != null)
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    sharePointPageHtml = sr.ReadToEnd();
                    sr.Close();
                    response.Close();
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    ex.Response.Close();
                }
            }
            return sharePointPageHtml;
        }

        public static void HttpPost(string url, object obj, string contentType, byte[] body, Dictionary<string, object> headerInformation)
        {
            AveHttpWebRequest httpPostRequest = AveHttpWebRequest.Create(url);
            NetworkCredential credential = obj as NetworkCredential;
            if (credential != null)
            {
                httpPostRequest.Credentials = credential;
            }
            else
            {
                httpPostRequest.CookieContainer = obj as CookieContainer;
            }
            httpPostRequest.ContentType = contentType;
            httpPostRequest.Method = "Post";
            httpPostRequest.ContentLength = body.Length;
            if (headerInformation != null && headerInformation.Count > 0)
            {
                foreach (string key in headerInformation.Keys)
                {
                    httpPostRequest.Headers.Add(key, headerInformation[key].ToString());
                }
            }
            try
            {
                AveWebStream stream = httpPostRequest.GetRequestStream() as AveWebStream;
                stream.Write(body, 0, body.Length);
                stream.Close();
                AveHttpWebResponse response = httpPostRequest.GetResponse() as AveHttpWebResponse;
                if (response != null)
                {                  
                    response.Close();
                }
            }
            catch (WebException e)
            {                
                if (e.Response != null)
                {
                    Stream s = e.Response.GetResponseStream();
                    e.Response.Close();
                }
            }
        }

        public static string HttpReturn(string url, object obj, string contentType, byte[] body, Dictionary<string, object> headerInformation)
        {
            string postHtml = null;
            AveHttpWebRequest httpPostRequest = AveHttpWebRequest.Create(url);
            httpPostRequest.UserAgent = UserAgent;
            NetworkCredential credential = obj as NetworkCredential;
            if (credential != null)
            {
                httpPostRequest.Credentials = credential;
            }
            else
            {
                httpPostRequest.CookieContainer = obj as CookieContainer;
            }
            httpPostRequest.ContentType = contentType;
            httpPostRequest.Method = "Post";
            httpPostRequest.ContentLength = body.Length;
            if (headerInformation != null && headerInformation.Count > 0)
            {
                foreach (string key in headerInformation.Keys)
                {
                    httpPostRequest.Headers.Add(key, headerInformation[key].ToString());
                }
            }
            try
            {
                AveWebStream stream = httpPostRequest.GetRequestStream() as AveWebStream;
                stream.Write(body, 0, body.Length);
                stream.Close();
                AveHttpWebResponse response = httpPostRequest.GetResponse() as AveHttpWebResponse;
                if (response != null)
                {
                    long length = response.ContentLength;
                    postHtml = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    response.Close();
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    e.Response.Close();
                }
            }
            return postHtml;
        }

        public static Dictionary<string, object> GetInput(string html, string searchContent, Dictionary<string, object> inputDic)
        {
            List<string> inputList = new List<string>();
            int index = 0;
            index = html.IndexOf(searchContent, index,StringComparison.OrdinalIgnoreCase);
            while (index >= 0)
            {
                int endIndex = html.IndexOf("/>", index,StringComparison.OrdinalIgnoreCase) + 1;
                string information = html.Substring(index, endIndex - index + 1);
                inputList.Add(information);
                index = html.IndexOf(searchContent, index + 1,StringComparison.OrdinalIgnoreCase);
            }
            inputDic = GetInputDic(inputList, inputDic);
            return inputDic;
        }

        public static Dictionary<string, object> GetPostFormValues(string html, string formId = "aspnetForm")
        {
            Dictionary<string, object> formValues = new Dictionary<string, object>();
            
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode formNode = doc.GetElementbyId(formId);
            if (formNode != null)
            {
                CollectInputControlValues(formNode, "//input[@type='hidden']", formValues);
                CollectInputControlValues(formNode, "//input[@type='text']", formValues);
                CollectInputControlValues(formNode, "//input[@type='radio'][@checked='checked']", formValues);
                CollectInputControlValues(formNode, "//input[@type='checkbox'][@checked='checked']", formValues);
                CollectSelectControlValues(formNode, formValues);
            }

            return formValues;
        }

        public static void CollectInputControlValues(HtmlNode formNode, string pattern, Dictionary<string, object> formValues)
        {
            HtmlNodeCollection inputNodes = formNode.SelectNodes(pattern);
            if (inputNodes != null)
            {
                foreach (HtmlNode inputNode in inputNodes)
                {
                    string value = inputNode.GetAttributeValue("value", string.Empty);
                    formValues[inputNode.GetAttributeValue("name", string.Empty)] = HttpUtility.UrlEncode(value);
                }
            }
        }

        private static void CollectSelectControlValues(HtmlNode formNode, Dictionary<string, object> formValues)
        {
            HtmlNodeCollection inputNodes = formNode.SelectNodes("//select");
            if (inputNodes != null)
            {
                foreach (HtmlNode inputNode in inputNodes)
                {
                    string value = inputNode.FirstChild.GetAttributeValue("value", string.Empty);
                    foreach (HtmlNode optionNode in inputNode.ChildNodes)
                    {
                        if ("selected".Equals(optionNode.GetAttributeValue("selected", string.Empty), StringComparison.OrdinalIgnoreCase))
                        {
                            value = optionNode.GetAttributeValue("value", string.Empty);
                            break;
                        }
                    }
                    formValues[inputNode.GetAttributeValue("name", string.Empty)] = HttpUtility.UrlEncode(value);
                }
            }
        }

        public static Dictionary<string, string> GetInputForNavigationNodeProperties(string html, string searchContent, Dictionary<string, string> inputDic)
        {
            List<string> inputList = new List<string>();
            int index = 0;
            index = html.IndexOf(searchContent, index, StringComparison.OrdinalIgnoreCase);
            while (index >= 0)
            {
                int endIndex = html.IndexOf(");", index, StringComparison.OrdinalIgnoreCase) + 1;
                string information = html.Substring(index + searchContent.Length, endIndex - index - searchContent.Length + 1);
                inputList.Add(information);
                index = html.IndexOf(searchContent, index + 1, StringComparison.OrdinalIgnoreCase);
            }
            inputDic = GetInputDicForNavigationNodeProperties(inputList, inputDic);
            return inputDic;
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
            int index = startIndex;
            index = html.IndexOf(searchContent, index, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                int endIndex = html.IndexOf(endContent, index, StringComparison.OrdinalIgnoreCase) + endContent.Length;
                return html.Substring(index, endIndex - index);
            }
            return string.Empty;
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

        private static Dictionary<string, string> GetInputDicForNavigationNodeProperties(List<string> strList, Dictionary<string, string> inputDic, params string[] properties)
        {
            //properties 可用于属性的扩展，目前只取NodeType属性
            string key = string.Empty;
            string value = string.Empty;
            foreach (string str in strList)
            {                               
                int i=0;
                int index = str.IndexOf("', '");
                if (index <= 0)
                {
                    continue;
                }
                key = str.Substring(1, index);
                value = str.Substring(index + 4);
                do
                {
                    index = value.IndexOf("', '");
                    value = value.Substring(index + 4);
                    i++;
                }
                while(i<3);
                value = value.Substring(0, value.IndexOf("', '"));                
                inputDic[key] = value;
            }
            return inputDic;
        }

        public static string GetValue(string pStr, string str)
        {
            int index = pStr.IndexOf(str,StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                int startIndex = index + str.Length + 1;
                int endIndex = pStr.IndexOf("\"", startIndex,StringComparison.OrdinalIgnoreCase);
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
            byte[] bodyData = Encoding.Default.GetBytes(body);
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
                int index = html.IndexOf("<div id='" + id + "'",StringComparison.OrdinalIgnoreCase);
                int startIndex = html.IndexOf("name=", index + 1,StringComparison.OrdinalIgnoreCase) + 6;
                int endIdex = html.IndexOf("\"", startIndex,StringComparison.OrdinalIgnoreCase);
                featureList = html.Substring(startIndex, endIdex - startIndex);
            }
            catch (ArgumentException)
            {
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
                    int endIdex = information.IndexOf('"');//SAAS-576的处理（原来PorName包含空格时，空格后面部分显示不出来）
                    return information.Substring(0, endIdex).TrimEnd(' ');
                }
            }
            return string.Empty;
        }

        public static string GetInnerText(string html, string searchContent, string endContent)
        {
            int index = 0;
            index = html.IndexOf(searchContent, index, StringComparison.OrdinalIgnoreCase);
            while (index >= 0)
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
            int startIndex = html.IndexOf(searchContent);
            if (startIndex > 0)
            {
                int endIndex = html.IndexOf(endContent, startIndex) + endContent.Length;
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
            int startIndex = html.IndexOf(searchContent);
            if (startIndex > 0)
            {
                int endIndex = html.IndexOf(endContent, startIndex) + endContent.Length;
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
    }
}
