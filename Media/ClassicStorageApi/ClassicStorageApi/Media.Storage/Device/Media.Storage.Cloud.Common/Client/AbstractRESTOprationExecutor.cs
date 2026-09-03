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




namespace AvePoint.Media.ClassicStorage.Cloud.Common.Client
{

    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Net;
    using System.IO;
    using System.Xml.XPath;
    using System.Xml;
    using System.Web;
    using System.Security.Cryptography;
    using AvePoint.GCommon;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Request;
    using AvePoint.Media.ClassicStorage.Cloud.Common.HttpHelper;
    using System.Text.RegularExpressions;
    using AvePoint.Media.ClassicStorage.Util;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Config;
    using System.Reflection;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Globalization;
    using AvePoint.GCommon.Utility.Cryptography;
    using DocumentFormat.OpenXml.Wordprocessing;
    using System.Configuration;
    using System.Xml.Schema;
    #endregion

    public abstract class AbstractRESTOprationExecutor : ICloudOprationExecutor, IHttpRequestPrepare, IHttpResponseHandler
    {
        public Data_Version Data_Version { set; get; }
        protected string Protocol { get; set; }
        protected string Endpoint { get; set; }
        protected int Port { get; set; }
        public Retry RetryRequset { get; set; }
        public CloudOpenParameter CloudOpenParam { get; set; }
        private AveLogger log = AveLogger.GetInstance(typeof(AbstractRESTOprationExecutor));
        protected AveLogger Logger
        {
            set
            {
                log = value;
            }
            get
            {
                return this.log;
            }
        }

        public AbstractHttpClient HttpClient { get; set; }

        protected static readonly int MAX_OBJECT_COUNT = 1000;

        private Hashtable activedRequest = new Hashtable() { { "ACTIVED".ToLower(CultureInfo.InvariantCulture) + "Request", new List<HttpWebRequest>() } };
        protected List<HttpWebRequest> ActivedRequest { get { return activedRequest["ACTIVED".ToLower(CultureInfo.InvariantCulture) + "Request"] as List<HttpWebRequest>; } }


        #region Init Methods

        public virtual void InitRetry(CloudOpenParameter openParams)
        {
            Logger.Info("Init Retry: retryCount" + openParams.MaxRetryCount + ",RetryInterval" + openParams.RetryInterval);
            RetryRequset = new Retry(openParams.MaxRetryCount, openParams.RetryInterval, openParams.NeedRetry, openParams.FlushDNS);
        }

        #endregion

        #region Retry

        protected HttpWebResponse Retry(BasicRequest request)
        {
            return RetryRequset.retry(new CloudRetryMethod(request, this));
        }

        protected HttpWebResponse RetryUpLoad(HttpWebRequest request)
        {
            return RetryRequset.retry(new CloudRetryMethodForUpLoad(request, this));
        }

        private class CloudRetryMethod : IRetryMethod<HttpWebResponse>
        {
            private BasicRequest request;
            private AbstractRESTOprationExecutor executor;
            public CloudRetryMethod(BasicRequest request, AbstractRESTOprationExecutor executor)
            {
                this.request = request;
                this.executor = executor;
            }

            public HttpWebResponse retry()
            {
                return executor.ReExecute(request);
            }
        }

        private class CloudRetryMethodForUpLoad : IRetryMethod<HttpWebResponse>
        {
            private HttpWebRequest request;
            private AbstractRESTOprationExecutor executor;
            public CloudRetryMethodForUpLoad(HttpWebRequest request, AbstractRESTOprationExecutor executor)
            {
                this.request = request;
                this.executor = executor;
            }

            public HttpWebResponse retry()
            {
                return executor.ReExecuteUpLoad(request);
            }
        }

        #endregion

        #region Analyze Message

        //public virtual List<XPathNavigator> FirstStepAnalyzeXML(string xml, string xpath, string xmlns)
        //{
        //    List<XPathNavigator> result = new List<XPathNavigator>();

        //    XPathDocument xp = null;
        //    XPathNavigator nav = null;
        //    XPathNodeIterator it = null;

        //    try
        //    {
        //        using (MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
        //        {
        //            xp = new XPathDocument(input);
        //            nav = xp.CreateNavigator();
        //            if (!string.IsNullOrEmpty(xmlns))
        //            {
        //                nav.MoveToNamespace(xmlns);
        //            }
        //            it = nav.Select(xpath);

        //            while (it.MoveNext())
        //            {
        //                result.Add(it.Current.CreateNavigator());
        //            }

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex.Message, ex);
        //        throw;
        //    }

        //    return result;
        //}
        protected List<string> AnalyzeXML(Stream input, string xpath)
        {
            List<string> result = new List<string>();

            XPathDocument xp = null;
            XPathNavigator nav = null;
            XPathNodeIterator it = null;

            try
            {
                using (input)
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.ValidationType = ValidationType.Schema;
                    settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
                    using (XmlReader reader = XmlReader.Create(input, settings))
                    {
                        xp = new XPathDocument(reader);

                        nav = xp.CreateNavigator();
                        it = nav.Select(xpath);

                        while (it.MoveNext())
                        {
                            result.Add(it.Current.Value);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw;
            }

            return result;
        }

        public virtual List<XPathNavigator> FirstStepAnalyzeXML(string xml, string xpath)
        {
            List<XPathNavigator> result = new List<XPathNavigator>();


            XPathDocument xp = null;
            XPathNavigator nav = null;
            XPathNodeIterator it = null;

            try
            {
                using (MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.ValidationType = ValidationType.Schema;
                    settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
                    using (XmlReader reader = XmlReader.Create(input, settings))
                    {
                        xp = new XPathDocument(reader);

                        nav = xp.CreateNavigator();
                        it = nav.Select(xpath);

                        while (it.MoveNext())
                        {
                            result.Add(it.Current.CreateNavigator());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw;
            }

            return result;
        }

        private static void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            // 处理验证错误  
            throw new XmlSchemaValidationException($"Validation error: {e.Message}", e.Exception);
        }

        public List<string> AnalyzeXML(string xml, string xpath)
        {
            List<string> result = null;

            byte[] bSource = Encoding.UTF8.GetBytes(xml);
            try
            {
                using (Stream stream = new MemoryStream(bSource))
                {
                    result = AnalyzeXML(stream, xpath);
                }
            }
            catch (Exception t)
            {
                Logger.Error(t.Message, t);
                throw;
            }

            return result;
        }

        public List<string> AnalyzeXML(Stream input, string xpath, Dictionary<string, string> np)
        {
            List<string> result = new List<string>();

            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
            XmlNodeList nodeList = null;

            string[] elements = xpath.Split('/');
            try
            {
                using (input)
                {
                    doc.Load(input);

                    foreach (string key in np.Keys)
                    {
                        nsm.AddNamespace(key, np[key]);

                        StringBuilder path = new StringBuilder();
                        path.Append("//");
                        bool first = true;
                        foreach (string el in elements)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                path.Append("/");
                            }

                            path.Append(key)
                                .Append(":")
                                .Append(el);
                        }

                        nodeList = doc.SelectNodes(path.ToString(), nsm);
                        foreach (XmlNode node in nodeList)
                        {
                            result.Add(node.InnerText);
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw;
            }

            return result;
        }

        public List<string> AnalyzeXML(string xml, String xpath, Dictionary<String, String> np)
        {
            List<string> result = null;

            try
            {
                Stream stream = GetStreamByString(xml);
                result = AnalyzeXML(stream, xpath, np);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw;
            }

            return result;
        }

        public virtual string GetFinalUrl(StorageInfo info)
        {
            throw new NotImplementedException();
        }

        protected XmlNodeList GetNodeList(string xml, string xpath)
        {
            XmlNodeList result = null;
            XmlDocument doc = null;
            XmlElement root = null;
            XmlNamespaceManager xnsm = null;

            try
            {
                doc = new XmlDocument();
                doc.LoadXml(xml);
                root = doc.DocumentElement;

                string np = root.NamespaceURI;
                xnsm = new XmlNamespaceManager(doc.NameTable);
                xnsm.AddNamespace("NP".ToLower(CultureInfo.InvariantCulture), np);
                result = doc.SelectNodes(xpath.Replace("/", "/NP:").ToLower(CultureInfo.InvariantCulture), xnsm);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw;
            }

            return result;
        }
        #endregion

        #region Build URL

        protected string Encode(string str2Encode)
        {
            return HttpUtility.UrlEncode(str2Encode).Replace("+", "%20").Replace("%2f", "/");
        }

        protected string Decode(string str2Decode)
        {
            return Uri.UnescapeDataString(str2Decode);
        }

        #endregion

        #region Execute Request Methods


        public HttpWebResponse DoExecute(BasicRequest request)
        {
            HttpWebResponse result = null;

            try
            {
                result = HttpClient.Execute(request);
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure || we.Status == WebExceptionStatus.Timeout)
                {
                    Logger.Info("this exception is a connection fail exception:" + we.Message);
                    throw new RetryableException(we.Message, we);
                    //result = Retry(request);
                }
                else if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    using (HttpWebResponse response = we.Response as HttpWebResponse)
                    {
                        if (IsServerIntertalError(response.StatusCode))
                        {
                            throw new RetryableException(we.Message, we);
                            //result = Retry(request);
                        }
                        else if (SpecialRetryCondition(request, response))
                        {
                            throw new RetryableException(we.Message, we);
                            //result = Retry(request);
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new PathNotFoundException(request.URI, we);
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            throw new AuthenticationFailedException(response.StatusDescription + " : " + request.URI, we);
                        }
                        else
                        {
                            throw new UnknownException(request.URI, we);
                        }
                    }
                }
                else
                {
                    throw new UnknownException(request.URI, we);
                }
            }
            catch (FormatException fe)
            {
                throw new AuthenticationFailedException(fe.Message, fe);
            }
            catch (Exception t)
            {
                throw new UnknownException(request.URI, t);
            }
            return result;
        }

        protected HttpWebResponse UpLoad(HttpWebRequest request)
        {
            HttpWebResponse result = null;

            try
            {
                result = HttpClient.UpLoad(request);
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    result = RetryUpLoad(request);
                }
                else if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    using (HttpWebResponse response = we.Response as HttpWebResponse)
                    {
                        if (IsServerIntertalError(response.StatusCode))
                        {
                            result = RetryUpLoad(request);
                        }
                        else if (SpecialRetryCondition(request, response))
                        {
                            result = RetryUpLoad(request);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw;
            }

            return result;
        }

        protected virtual HttpWebResponse ReExecute(BasicRequest request)
        {
            HttpWebResponse result = null;

            try
            {
                result = HttpClient.Execute(request);
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    throw new RetryableException("", we);
                }
                else if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    using (HttpWebResponse response = we.Response as HttpWebResponse)
                    {
                        if (IsServerIntertalError(response.StatusCode))
                        {
                            throw new RetryableException("", we);
                        }
                        else if (SpecialRetryCondition(request, response))
                        {
                            throw new RetryableException("", we);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw;
            }

            return result;
        }

        protected virtual HttpWebResponse ReExecuteUpLoad(HttpWebRequest request)
        {
            HttpWebResponse result = null;

            try
            {
                ResetRequest(request);
                result = HttpClient.UpLoad(request);
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    throw new RetryableException("", we);
                }
                else if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    using (HttpWebResponse response = we.Response as HttpWebResponse)
                    {
                        if (IsServerIntertalError(response.StatusCode))
                        {
                            throw new RetryableException("", we);
                        }
                        else if (SpecialRetryCondition(request, response))
                        {
                            throw new RetryableException("", we);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw;
            }

            return result;
        }

        protected virtual void ResetRequest(HttpWebRequest request)
        {
            Logger.Info("reset request");
        }

        #endregion

        #region Error Message

        protected string GetCheckCtnErrorMsg(string xSetName)
        {
            return string.Format("Check container failed, container : {0}.", xSetName);
        }

        protected string GetCreateCtnErrorMsg(string xSetName)
        {
            return string.Format("Create container failed, container : {0}.", xSetName);
        }

        protected string GetDeleteCtnErrorMsg(string xSetName)
        {
            return string.Format("Delete container failed, container : {0}.", xSetName);
        }

        protected string GetListObjErrorMsg(string xSetName)
        {
            return string.Format("List object failed, container : {0}.", xSetName);
        }

        protected string GetCheckObjErrorMsg(string xSetName, string xStreamName)
        {
            return string.Format("Check object failed, object : {0}, container : {1}.", xStreamName, xSetName);
        }

        protected string GetCreateObjErrorMsg(string xSetName, string xStreamName)
        {
            return string.Format("Create object failed, object : {0}, container : {1}.", xStreamName, xSetName);
        }

        protected string GetOpenObjErrorMsg(string xSetName, string xStream)
        {
            return string.Format("Open object failed, object : {0}, container : {1}.", xStream, xSetName);
        }

        protected string GetDeleteObjErrorMsg(string xSetName, string xStream)
        {
            return string.Format("Delete object failed, object : {0}, container : {1}.", xStream, xSetName);
        }
        #endregion

        public Stream GetStreamByString(string strSource)
        {
            byte[] bSource = Encoding.UTF8.GetBytes(strSource);
            Stream stream = new MemoryStream(bSource);
            return stream;
        }

        //public string GetMD5(Stream stream)
        //{
        //    byte[] bytes = new MD5CryptoServiceProvider().ComputeHash(stream);
        //    StringBuilder builder = new StringBuilder();
        //    foreach (byte bit in bytes)
        //    {
        //        builder.Append(bit.ToString("x2"));
        //    }

        //    stream.Position = 0;
        //    return builder.ToString();
        //}

        public string GetMD5(string str)
        {
            StringBuilder result = new StringBuilder();

            IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(GCommon.Utility.Cryptography.HashAlgorithm.MD5);
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));

            foreach (byte bit in data)
            {
                result.Append(bit.ToString("x2"));
            }

            return result.ToString();
        }

        protected string ConvertQueryList2String(Dictionary<string, string> parameters)
        {
            StringBuilder builder = new StringBuilder();
            if (parameters == null || parameters.Count == 0)
            {
                return "";
            }

            bool first = true;

            foreach (KeyValuePair<string, string> item in parameters)
            {
                if (first)
                {
                    builder.Append("?");
                    first = false;
                }

                else
                {
                    builder.Append("&");
                }

                builder.Append(Encode(item.Key))
                       .Append("=")
                       .Append(Encode(item.Value));
            }
            return builder.ToString();

        }



        protected string GetStringFromResponse(HttpWebResponse resp)
        {
            if (resp == null || resp.ContentLength == 0)
            {
                return "";
            }

            StringBuilder builder = new StringBuilder();
            using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    builder.Append(line);
                }
            }

            return builder.ToString();
        }

        protected void LogServerMsg(HttpWebResponse resp, string msg)
        {
            // string serverMsg = GetStringFromResponse(resp);
            //logger.Error(msg + ", Status Code : " + resp.StatusCode + ", server response message : " + serverMsg);
        }

        protected bool IsProtocalError(WebException e)
        {
            if (e.Status == WebExceptionStatus.ProtocolError)
            {
                return true;
            }

            return false;
        }

        protected virtual string BuildURL()
        {
            if (Port == 80 || Port == 443 || Port == 0)
            {
                return Protocol + "://" + Endpoint;
            }
            return Protocol + "://" + Endpoint + ":" + Port;
        }

        public virtual string BuildURLWithOutQueryParams(string container)
        {
            throw new NotImplementedException();
        }

        public virtual string BuildObjectAbsoluteURL(string container, string objectName)
        {
            throw new NotImplementedException();
        }

        public virtual string BuildObjectAbsoluteURL(string url, string container, string objectName)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsServerIntertalError(HttpStatusCode code)
        {
            bool restult = false;

            if (code == HttpStatusCode.InternalServerError || code == HttpStatusCode.RequestTimeout || code == HttpStatusCode.ServiceUnavailable)
            {
                restult = true;
            }
            return restult;
        }

        protected virtual bool SpecialRetryCondition(BasicRequest request, HttpWebResponse resp)
        {
            return false;
        }

        protected virtual bool SpecialRetryCondition(HttpWebRequest request, HttpWebResponse resp)
        {
            return false;
        }

        protected virtual List<string> ListObjectWithPreFix(string xSetName, string prefix, bool isGetName)
        {
            List<string> objects = new List<string>();
            string marker = string.Empty;
            while (true)
            {
                if (objects.Count != 0)
                {
                    marker = objects[objects.Count - 1];
                    objects.RemoveAt(objects.Count - 1);
                }
                List<string> files = ListXstream(xSetName, prefix, MAX_OBJECT_COUNT, marker, isGetName);
                if (files.Count <= MAX_OBJECT_COUNT)
                {
                    if (objects.Count == 0)
                    {
                        return files;
                    }
                    else
                    {
                        foreach (string name in files)
                        {
                            objects.Add(name);
                        }
                        return objects;
                    }
                }
                else
                {
                    foreach (string name in files)
                    {
                        objects.Add(name);
                    }
                }
            }
        }

        protected virtual List<string> ListXstream(string xSetName, string prefix, int limit, string marker, bool isGetName)
        {
            throw new UnsupportedXException("this method need override");
        }

        protected virtual long GetXsetSize(string xSetName, string prefix)
        {
            long size = 0;
            List<string> sizeList = ListObjectWithPreFix(xSetName, prefix, false);
            foreach (string objSize in sizeList)
            {
                if (!string.IsNullOrEmpty(objSize))
                {
                    size += long.Parse(objSize);
                }
            }
            return size;
        }

        public virtual string ListAzureMetaName(string baseURL, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
        {
            throw new NotImplementedException();
        }

        public virtual void InitConfig(CloudOpenParameter prams)
        {
            throw new NotImplementedException();
        }

        public virtual CloudOpenParameter ConveryParams(Dictionary<string, string> prams)
        {
            throw new NotImplementedException();
        }

        public virtual List<string> ListContainers()
        {
            throw new NotImplementedException();
        }

        public virtual bool CheckContainer(string xSetName)
        {
            throw new NotImplementedException();
        }

        public virtual bool CreateContainer(string xSetName)
        {
            throw new NotImplementedException();
        }

        public virtual bool DeleteContainer(string xSetName)
        {
            throw new NotImplementedException();
        }

        public virtual List<string> ListObject(string xSetName)
        {
            throw new NotImplementedException();
        }

        public virtual List<string> ListObject(string xSetName, string prefix)
        {
            throw new NotImplementedException();
        }

        public virtual bool CheckObject(string xSetName, string xStreamName)
        {
            throw new NotImplementedException();
        }

        public virtual HttpWebRequest GetUploadRequest(string xSetName, string xStreamName, string mimeType, HttpWebRequest webRequest, int blockNumber, long dataLength)
        {
            throw new NotImplementedException();
        }

        public virtual bool CreateObject(string xSetName, string xStreamName, HttpWebRequest request, long dataLength)
        {
            throw new NotImplementedException();
        }

        public virtual Stream OpenObject(string xSetName, string xStreamName, int rangFrom, int rangeTo)
        {
            throw new NotImplementedException();
        }

        public virtual Stream OpenObject(string container, string objectName, int[] lengths, FileMode mode)
        {
            throw new NotImplementedException();
        }

        public virtual bool DeleteObject(string xSetName, string xStreamName, bool isDeleteSubFile)
        {
            throw new NotImplementedException();
        }

        public virtual CloudFileInfo GetObjectInfo(string xSetName, string xStreamName)
        {
            throw new NotImplementedException();
        }

        public virtual bool Login(string xSetName)
        {
            throw new NotImplementedException();
        }

        public virtual long GetContainerSize(string xSetName)
        {
            throw new NotImplementedException();
        }

        public virtual StorageOpenValidResult HasPermissions()
        {
            return new StorageOpenValidResult()
            {
                IsHasPermission = true,
                //TotalSpace = long.MaxValue - 1,
                //TotalFreeSpace = long.MaxValue - 1,
                //TotalUsedSpace = 0
            };
        }

        public virtual string GetDocAveDefaultContainer()
        {
            return null;
        }

        //from new interface

        public class HttpRequestRetryMethod : IRetryMethod<HttpWebResponse>
        {
            private HttpWebRequest request;
            private AbstractRESTOprationExecutor executor;
            public HttpRequestRetryMethod(HttpWebRequest request, AbstractRESTOprationExecutor executor)
            {
                this.request = request;
                this.executor = executor;
            }

            public HttpWebResponse retry()
            {
                return executor.ReExecute(request);
            }
        }

        protected virtual HttpWebResponse ReExecute(HttpWebRequest request)
        {
            HttpWebResponse result = null;

            try
            {
                result = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure || we.Status == WebExceptionStatus.Timeout)
                {
                    throw new RetryableException("", we);
                }
                else
                    if (we.Status == WebExceptionStatus.ProtocolError)
                    {
                        using (HttpWebResponse response = we.Response as HttpWebResponse)
                        {
                            if (IsServerIntertalError(response.StatusCode))
                            {
                                throw new RetryableException("", we);
                            }
                            else if (SpecialRetryCondition(request, response))
                            {
                                throw new RetryableException("", we);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                    else
                    {
                        throw;
                    }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw;
            }

            return result;
        }

        protected virtual HttpWebResponse Retry(HttpWebRequest request)
        {
            return RetryRequset.retry(new HttpRequestRetryMethod(request, this));
        }

        protected virtual HttpWebResponse DoExecute(HttpWebRequest request, Dictionary<string, string> headers)
        {
            HttpWebResponse resp = null;
            try
            {
                if (request == null)
                {
                    throw new Exception("HttpWebRequest is null.");
                }
                HttpClient.SetUpProxy(request, Proxy, ProxyCredential);
                HttpClient.CombiningRequestWithHeaders(request, headers);
                resp = request.GetResponse() as HttpWebResponse;
                HttpClient.CalcDataFlow(request, resp);
                if (resp != null)
                {
                    var statusCode = Convert.ToInt32(resp.StatusCode);
                    if (statusCode != 200)
                    {
                        Logger.Debug("HttpStatusCode=" + Convert.ToInt32(resp.StatusCode) + " "
                                   + resp.StatusCode.ToString() + "; RequestUri=" + request.RequestUri);
                    }
                }
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.Timeout || we.Status == WebExceptionStatus.NameResolutionFailure || we.Status == WebExceptionStatus.SecureChannelFailure)
                {
                    Logger.Info("this exception is a connection fail exception:" + we.Message);
                    throw new RetryableException(we.Message, we);
                }
                else if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    using (HttpWebResponse response = we.Response as HttpWebResponse)
                    {
                        if (IsServerIntertalError(response.StatusCode))
                        {
                            throw new RetryableException(we.Message, we);
                        }
                        else if (SpecialRetryCondition(request, response))
                        {
                            throw new RetryableException(we.Message, we);
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new PathNotFoundException(request.RequestUri.ToString(), we);
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            throw new AuthenticationFailedException(we.ToString() + "\r\n" + request.RequestUri.ToString(), we);
                        }
                        else
                        {
                            throw new UnknownException(request.RequestUri.ToString(), we);
                        }
                    }
                }
                else if (we.Status == WebExceptionStatus.NameResolutionFailure || we.Status == WebExceptionStatus.ConnectionClosed)
                {
                    throw new AuthenticationFailedException(AvePoint.Media.ClassicStorage.Resources.CloudCommonI18N.CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Cannot_connect_to_the_remote_server", AbstractXSystem.Culture));
                }
                else
                {
                    throw new UnknownException(request.RequestUri.ToString(), we);
                }
            }
            catch (Exception e)
            {
                Logger.Error("DoExecute error:", e);
                try
                {
                    if (request != null)
                    {
                        request.Abort();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);
                }
                throw;
            }
            return resp;
        }

        public virtual bool IsConnectFailureError(WebException we)
        {
            if (we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.NameResolutionFailure)
            {
                return true;
            }
            return false;
        }

        public virtual ResponseInfo ListObjects(string baseURL, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
        {
            ResponseInfo responseInfo = new ResponseInfo();
            //string url = CombiningQueryParams(baseURL, queryParams);
            HttpWebRequest requestGet = HttpClient.CreateRequestGet(baseURL, queryParams);
            using (HttpWebResponse resp = DoExecute(requestGet, headers))
            {
                using (Stream inputStream = resp.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(inputStream))
                    {
                        responseInfo.ResponseXml = Decode(reader.ReadToEnd());
                        return responseInfo;
                    }
                }
            }
        }

        public virtual void CreateObjectWithNoContent(string fullURL, Dictionary<string, string> headers)
        {
            //try
            //{
            HttpWebRequest requestPut = HttpClient.CreateRequestPut(fullURL, null);
            using (HttpWebResponse resp = DoExecute(requestPut, headers))
            {
                //no code, just to close resp.
            }
            //}
            //catch (WebException tx)
            //{
            //    HttpWebResponse hasClosedResponse = tx.Response as HttpWebResponse;
            //    if (hasClosedResponse.StatusCode == HttpStatusCode.BadRequest)
            //        throw new PathNotFoundException("The requested container does not exist");
            //    throw new UnknownException("Create ObjectWithNoContent Failed : " + tx.Message, tx);
            //}
        }

        public virtual HttpUploadStream OpenObjectForWrite(string fullURL, Dictionary<string, string> headers)
        {
            HttpWebRequest request = HttpClient.CreateRequestPut(fullURL, null);
            HttpClient.CombiningRequestWithHeaders(request, headers);
            return new HttpUploadStream(request) { HttpClient = this.HttpClient, System = this.HttpClient.CurrentSystem };
        }

        public virtual HttpDownloadStream OpenObjectForRead(string fullURL, Dictionary<string, string> headers)
        {
            HttpWebRequest request = HttpClient.CreateRequestGet(fullURL, null);
            return new HttpDownloadStream(DoExecute(request, headers)) { System = this.HttpClient.CurrentSystem };
        }

        public bool DeleteObject(string fullURL, Dictionary<string, string> parameters, Dictionary<string, string> headers)
        {
            try
            {
                HttpWebRequest request = HttpClient.CreateRequestDelete(fullURL, parameters);
                using (HttpWebResponse resp = DoExecute(request, headers))
                {
                    if (resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                Logger.Warn("Cannot find object, may it was deleted successful : " + e.Message);
                return true;
            }
            return false;
        }

        public virtual SpaceInfo GetUserAccountInfo()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            //foreach (HttpWebRequest request in ActivedRequest)
            //{
            //    request.Abort();
            //}
        }

        public virtual bool CheckObject(string fullURL, Dictionary<string, string> parameters, Dictionary<string, string> headers)
        {
            bool result = false;
            try
            {
                HttpWebRequest request = HttpClient.CreateRequestGet(fullURL, null);
                //SignRequest(request);
                using (HttpWebResponse resp = DoExecute(request, headers))
                {
                    if (resp != null && (resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.OK))
                    {
                        //Logger.Debug("Object exists, object : " + xStreamName + ", container : " + xSetName);
                        result = true;
                    }
                }
            }
            catch (PathNotFoundException ex)
            {
                Trace.TraceWarning(ex.Message);
                result = false;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                throw;
            }

            return result;
        }

        public virtual Dictionary<string, string> GetObjectInfo(string url, Dictionary<string, string> requestParams, Dictionary<string, string> requestHeaders)
        {
            return null;
        }

        public virtual object Invoke(string methodName, object[] args)
        {
            //Logger.Debug("InvokeMethodName: " + methodName);
            object result = null;
            Type[] types = GetTypes(args);
            MethodInfo methodInfo;
            methodInfo = this.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.ExactBinding, null, types, null);
            try
            {
                result = methodInfo.Invoke(this, args);
            }
            catch (Exception ex)
            {
                Logger.Error(" invoke Error:" + ex);
                if (ex is RetryableException || ex.InnerException is RetryableException)
                {
                    result = RetryRequset.ExcuteRetry(methodInfo, this, args);
                }
                else
                {
                    Logger.Error(ex.Message + "this exception does not need to retry, throw it out");
                    throw ex.InnerException;
                }
            }
            return result;
        }

        public Type[] GetTypes(object[] objs)
        {
            if (objs == null)
            {
                return Type.EmptyTypes;
            }
            Type[] types = new Type[objs.Length];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] == null)
                {
                    return Type.EmptyTypes;
                }
                types[i] = objs[i].GetType();
            }
            return types;
        }
        #region From IHttpRequestPrepare Interface

        public IWebProxy Proxy { get; set; }
        public NetworkCredential ProxyCredential { get; set; }

        public virtual Dictionary<string, string> ListDirectoryQueryParams
        {
            get { return new Dictionary<string, string>(); }
        }

        public virtual Dictionary<string, string> OpenStream4Write
        {
            get { return new Dictionary<string, string>(); }
        }

        public virtual Dictionary<string, string> OpenStream4Read
        {
            get { return new Dictionary<string, string>(); }
        }

        public virtual Dictionary<string, string> ListObjectQueryParams
        {
            get { return new Dictionary<string, string>(); }
        }

        public virtual Dictionary<string, string> ListDirectoryHeaders
        {
            get { return Headers; }
        }

        public virtual Dictionary<string, string> ListObjectHeaders
        {
            get { return Headers; }
        }

        public virtual Dictionary<string, string> CopyFileQueryParams
        {
            get { return new Dictionary<string, string>(); }
        }

        public virtual Dictionary<string, string> DleteFileParams
        {
            get { return new Dictionary<string, string>(); }
        }

        public virtual Dictionary<string, string> DleteFolderParams
        {
            get { return new Dictionary<string, string>(); }
        }

        public virtual Dictionary<string, string> CopyFileHeaders
        {
            get { return Headers; }
        }

        public virtual Dictionary<string, string> OpenDirectoryWriteModeHeaders
        {
            get { return Headers; }
        }

        public virtual Dictionary<string, string> OpenDirectoryReadModeHeaders
        {
            get { return Headers; }
        }

        public virtual Dictionary<string, string> OpenFileWriteModeHeaders
        {
            get { return Headers; }
        }

        public virtual Dictionary<string, string> OpenFileReadModeHeaders
        {
            get { return Headers; }
        }

        public virtual Dictionary<string, string> OpenStreamWriteModeHeaders
        {
            get { return Headers; }
        }

        public virtual Dictionary<string, string> OpenStreamReadModeHeaders
        {
            get { return Headers; }
        }

        public virtual Dictionary<string, string> Headers
        {
            get { return new Dictionary<string, string>(); }
        }
        #endregion

        #region from IHttpResponseHandler Interface
        public virtual List<XDirectoryInfo> Parse2Directory(string responseXmlString, string path)
        {
            throw new NotImplementedException();
        }

        public virtual List<XFileInfo> Parse2File(string responseXmlString)
        {
            throw new NotImplementedException();
        }
        #endregion


    }
    public class ResponseInfo
    {
        private Dictionary<string, string> headers = new Dictionary<string, string>();
        private string responseXml;
        public Dictionary<string, string> Headers
        {
            get { return headers; }
            set { headers = value; }
        }
        public string ResponseXml
        {
            get { return responseXml; }
            set { responseXml = value; }
        }
    }
    public delegate void Parse2DirectoryDelegate(string responseXmlString);

    public class DnsUtil
    {
        //Flush DNS, use another Windows undocumented API: DnsFlushResolverCache, located in dnsapi.dll.
        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        private static extern UInt32 DnsFlushResolverCache();
        public static void FlushMyCache()
        {
            UInt32 result = DnsFlushResolverCache();
        }
    }
}
