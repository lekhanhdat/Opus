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
namespace AvePoint.Media.Storage.Cloud.Cleversafe
{
    #region using directives
    using Amazon;
    using Common;
    using Resources.CloudCommonI18N;
    using Util;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;
    #endregion
    class CleversafeClient
    {
        CleversafeOpenParameter openParameter = new CleversafeOpenParameter();
        StorageLogger logger = new StorageLogger(typeof(CleversafeClient));
        String workingAccesserIP;
        Dictionary<String, AccesserIPHealth> accesserIPMap = new Dictionary<string, AccesserIPHealth>();
        public delegate T ChangeAccesserIPDelegate<T>();
        public delegate T DoExcuteDelegate<T>();
        protected static readonly Int32 MAX_OBJECT_COUNT = 1000;
        private string protocol;
        public Retry RetryRequset { get; set; }
        public CleversafeHttpClient HttpClient { get; set; }
        public CloudOpenParameter CloudOpenParam { get; set; }
        public Dictionary<String, String> Headers
        {
            get
            {
                String date = CleversafeUtils.GetReqeustDate(HttpClient.TimeOffset);
                return new Dictionary<String, String>() { { CleversafeConstant.DATE, date } };
            }
        }

        public virtual Dictionary<String, String> CopyFileQueryParams
        {
            get { return new Dictionary<String, String>(); }
        }

        public Dictionary<String, String> QueryParams
        {
            get { return new Dictionary<String, String>() { { "max-keys".ToLower(CultureInfo.InvariantCulture), "1000" } }; }
        }

        public CleversafeClient()
        {
            HttpClient = new CleversafeHttpClient();
        }

        public void InitConfig(CleversafeOpenParameter parameter)
        {
            openParameter = parameter;
            HttpClient.OpenParam = parameter;
            this.CloudOpenParam = parameter;
            this.HttpClient.CurrentSystem = parameter.system;
            SetIP();
            workingAccesserIP = parameter.AccesserIPs[0];
            this.protocol = parameter.Protocol;
            InitRetry(parameter);
        }

        public void InitRetry(CloudOpenParameter openParams)
        {
            logger.Info("Init Retry: retryCount {0}, RetryInterval {1}", openParams.MaxRetryCount, openParams.RetryInterval);
            RetryRequset = new Retry(openParams.MaxRetryCount, openParams.RetryInterval, openParams.NeedRetry, true);
        }

        public String InitiateMultipartUpload(String fullURL, Dictionary<String, String> headers)
        {
            if (headers.ContainsKey("Content-Type"))
            {
                headers.Remove("Content-Type");
            }
            if (headers.ContainsKey("Content-Length"))
            {
                headers.Remove("Content-Length");
            }
            return DoExcuteWithMultiAccesserIPRetry<String>(delegate ()
            {
                fullURL = fullURL + "?uploads";
                HttpWebRequest request = HttpClient.CreateRequestPost(fullURL, null);
                request.ContentLength = 0;
                request.Headers.Add(CleversafeConstant.AWS3_REST_HEADER_PREFIX + "storage-class", "STANDARD");
                request.ContentType = "application/octet-stream";
                request.ServicePoint.Expect100Continue = false;
                String uploadId = null;
                using (HttpWebResponse response = DoExcute(request, headers))
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        String responseXml = streamReader.ReadToEnd();
                        Regex regex = new Regex("<UploadId>([^<].*)</UploadId>");
                        Match match = regex.Match(responseXml);
                        if (!match.Success)
                        {
                            throw new Exception(String.Format("Not found UploadId, xml='{0}'", responseXml));
                        }
                        uploadId = match.Groups[1].Value;
                    }
                }
                logger.Debug("Initiate Multipart Upload succeed, uploadId={0} url='{1}'.", uploadId, fullURL);
                return uploadId;
            });
        }

        public String BuildURL(String vaultName, String objectName = null)
        {
            String result = default(String);
            if (objectName == null)
            {
                if (vaultName.Contains("\\"))
                {
                    vaultName = vaultName.Replace("\\", "/");
                }
                result = this.protocol + "://" + workingAccesserIP + "/" + Encode(vaultName);
            }
            else
            {
                objectName = objectName.Contains("\\") ? objectName.Replace("\\", "/") : Encode(objectName);
                result = BuildURL(vaultName) + "/" + objectName;
            }
            return result;
        }

        public Boolean CheckObject(String vaultName, String objectName, ObjectType objectType)
        {
            Boolean result = false;
            String prefix = objectName;
            String finalUrl = null;
            Dictionary<String, String> queryParams;
            try
            {
                return DoExcuteWithMultiAccesserIPRetry<Boolean>(delegate ()
                {
                    queryParams = new Dictionary<String, String>();
                    switch (objectType)
                    {
                        case ObjectType.File:
                            finalUrl = BuildURL(vaultName, objectName);
                            break;
                        case ObjectType.Directory:
                            String baseURL = BuildURL(vaultName);
                            queryParams.Add("prefix", prefix);
                            queryParams.Add("delimiter", "/");
                            finalUrl = HttpClient.CombiningQueryParams(baseURL, queryParams);
                            break;
                        case ObjectType.Vault:
                            String uri = BuildURL(vaultName);
                            queryParams.Add("prefix", "");
                            queryParams.Add("max-keys".ToLower(CultureInfo.InvariantCulture), "1000");
                            queryParams.Add("delimiter", "/");
                            finalUrl = HttpClient.CombiningQueryParams(uri, queryParams);
                            break;
                    }
                    var request = GetCleversafeRequest(finalUrl);
                    request.Method = RESTCommands.GET;
                    HttpWebRequest webRequest = HttpClient.GetHttpWebRequest(request);
                    using (HttpWebResponse response = DoExcute(webRequest))
                    {
                        if (objectType == ObjectType.Directory)
                        {
                            result = HasRespContentCounts(response);
                        }
                        else
                        {
                            result = response.StatusCode == HttpStatusCode.OK;
                        }
                    }
                    return result;
                });
            }
            catch (PathNotFoundException ex)
            {
                logger.Warn("Path not found. Details : {0}", ex);
                result = false;
            }
            catch (Exception e)
            {
                logger.Error("Check object failed, vaultName : {0}, objectName : {1}. Details : {2}", vaultName, objectName, e);
                throw;
            }
            return result;
        }

        public StorageOpenValidResult GetPermissions()
        {
            return new StorageOpenValidResult()
            {
                IsHasPermission = true,
            };
        }

        public Boolean CheckVault(String vaultName)
        {
            Boolean result = false;
            try
            {
                return DoExcuteWithMultiAccesserIPRetry<Boolean>(delegate ()
                {
                    String uri = BuildURL(vaultName);
                    Dictionary<String, String> queryParams = new Dictionary<String, String>();
                    queryParams.Add("prefix", "");
                    queryParams.Add("max-keys".ToLower(CultureInfo.InvariantCulture), "1000");
                    queryParams.Add("delimiter", "/");
                    String finalURL = HttpClient.CombiningQueryParams(uri, queryParams);
                    CleversafeRequest request = GetCleversafeRequest(finalURL);
                    request.Method = RESTCommands.GET;
                    HttpWebRequest webRequest = HttpClient.GetHttpWebRequest(request);
                    using (HttpWebResponse response = DoExcute(webRequest))
                    {
                        result = response.StatusCode == HttpStatusCode.OK;
                    }
                    return result;
                });
            }
            catch (PathNotFoundException e)
            {
                logger.Warn("Path not found. Details : {0}", e);
                result = false;
            }
            catch (Exception e)
            {
                logger.Error("Check Vault failed, vaultName : {0}. Details : {1}", vaultName, e);
                throw;
            }
            return result;
        }


        public Boolean CopyFile(StorageInfo sourceStorageInfo, StorageInfo targetStorageInfo, Dictionary<String, String> queryParams)
        {
            Boolean result = false;
            Dictionary<String, String> queryHeaders = Headers;
            String sourcePath = "/" + PathUtil.CombinePath(openParameter.VaultName, sourceStorageInfo.LowName);
            sourcePath = sourcePath.Replace("\\", "/");
            queryHeaders.Add(CleversafeConstant.AWS3_REST_HEADER_PREFIX + "copy-source", Encode(sourcePath));
            return DoExcuteWithMultiAccesserIPRetry<Boolean>(delegate ()
            {
                String destUrl = BuildURL(targetStorageInfo.HighName, targetStorageInfo.LowName);
                HttpWebRequest requestPut = HttpClient.CreateRequestPut(destUrl, queryParams);
                requestPut.ContentLength = 0;
                using (HttpWebResponse response = DoExcute(requestPut, queryHeaders))
                {
                    using (Stream inputStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(inputStream))
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                result = true;
                            }
                            else
                            {
                                logger.Error("CopyFile failed {0}.", requestPut.RequestUri);
                            }
                        }
                    }
                }
                return result;
            });
        }

        public virtual Boolean CreateObjectWithNoContent(String vaultName, String objectName, Dictionary<String, String> headers)
        {
            return DoExcuteWithMultiAccesserIPRetry<Boolean>(delegate ()
            {
                String fullURL = BuildURL(vaultName, objectName);
                HttpWebRequest requestPut = HttpClient.CreateRequestPut(fullURL, null);
                using (HttpWebResponse resp = DoExcute(requestPut, headers)) { }
                return true;
            });
        }

        public Boolean CompleteMultipartUpload(String fullURL, Dictionary<int, String> eTags)
        {
            return RetryRequset.CloudRetry<Boolean>(delegate ()
            {
                StringBuilder xmlData = new StringBuilder();
                xmlData.Append("<CompleteMultipartUpload>");
                foreach (var eTag in eTags)
                {
                    xmlData.Append(String.Format("<Part><PartNumber>{0}</PartNumber><ETag>{1}</ETag></Part>", eTag.Key, eTag.Value));
                }
                xmlData.Append("</CompleteMultipartUpload>");
                Byte[] xmlDataBuffer = Encoding.UTF8.GetBytes(xmlData.ToString());
                HttpWebRequest request = HttpClient.CreateRequestPost(fullURL, null);
                request.ContentLength = xmlDataBuffer.Length;
                request.ContentType = "application/xml";
                request.ServicePoint.Expect100Continue = false;
                HttpClient.CombiningRequestWithHeaders(request, this.Headers);
                using (Stream upStream = request.GetRequestStream())
                {
                    upStream.Write(xmlDataBuffer, 0, xmlDataBuffer.Length);
                }
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(String.Format("Complete Multipart Upload failed, url='{0}' HttpStatusCode= {1}", response.ResponseUri, response.StatusCode));
                    }
                    logger.Info("Complete Multipart Upload succeed, url='{0}'", fullURL);
                    return true;
                }
            });
        }

        public ResponseInfo DeleteObjects(String fullURL, Dictionary<String, String> requestParams, Dictionary<String, String> requestHeaders, String content)
        {
            HttpWebRequest request = HttpClient.CreateRequestPost(fullURL, requestParams);
            Byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            requestHeaders["Content-Length"] = contentBytes.Length.ToString();
            requestHeaders["Content-MD5"] = CleversafeUtils.Base64Encoded128BitMD5Digest(content);
            requestHeaders["Content-Type"] = "application/xml";
            HttpClient.AddHeaders(request, requestHeaders);
            request.AllowWriteStreamBuffering = false;
            request.AllowAutoRedirect = true;
            request.Timeout = 0x7ffffffe;
            Byte[] hashByte = CryptoUtil.ComputeHash(contentBytes, 0, contentBytes.Length);
            String hashString = CryptoUtil.ToHex(hashByte, true);
            request.Headers.Add("X-Amz-Content-SHA256", hashString);
            CleversafeUtils.AddAuthorization(request, openParameter.UserName, openParameter.Password);
            using (Stream uploader = request.GetRequestStream())
            {
                uploader.Write(contentBytes, 0, contentBytes.Length);
            }
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        String responseXML = String.Empty;
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            responseXML = reader.ReadToEnd();
                        }
                        return new ResponseInfo() { ResponseXml = responseXML };
                    }
                }
            }
            catch (WebException e)
            {
                logger.Error("Delete Objects failed. Details : {0}", e);
                throw;
            }
            return null;
        }

        public Boolean DeleteObject(String vaultName, String objectName, Boolean isDeleteSubFile)
        {
            Boolean result = false;
            if (isDeleteSubFile)
            {
                List<String> subFiles = ListObject(vaultName, objectName);
                foreach (String name in subFiles)
                {
                    result = InnerDeleteObject(vaultName, PathUtil.CombinePath(vaultName, name));
                    if (!result)
                    {
                        return false;
                    }
                }
            }
            result = InnerDeleteObject(vaultName, objectName);
            return result;
        }

        private Boolean InnerDeleteObject(String vaultName, String objectName)
        {
            Boolean result = false;
            try
            {
                return DoExcuteWithMultiAccesserIPRetry<Boolean>(delegate ()
                {
                    String uri = BuildURL(vaultName, objectName);
                    CleversafeRequest request = GetCleversafeRequest(uri);
                    request.Method = RESTCommands.DELETE;
                    HttpWebRequest webRequest = HttpClient.GetHttpWebRequest(request);
                    using (HttpWebResponse response = DoExcute(webRequest))
                    {
                        if (response.StatusCode == HttpStatusCode.NoContent)
                        {
                            result = true;
                        }
                        else
                        {
                            String message = GetStringFromResponse(response);
                            logger.Warn("Delete object {0} failed, message : {1}", objectName, message);
                        }
                    }
                    return result;
                });
            }
            catch (PathNotFoundException e)
            {
                logger.Warn("Cannot find the object, maybe it was deleted successfully before. Details : {0}", e.Message);
                result = true;
            }
            catch (Exception e)
            {
                logger.Error("Delete object failed, object : {0}, container : {1}. Details : {2}", objectName, vaultName, e);
                throw;
            }
            return result;
        }

        public T DoExcuteWithMultiAccesserIPRetry<T>(ChangeAccesserIPDelegate<T> del)
        {
            while (true)
            {
                try
                {
                    return del.Invoke();
                }
                catch (PathNotFoundException e)
                {
                    logger.Warn("Path not found.Details:{0}", e.Message);
                    throw;
                }
                catch (AuthenticationFailedException e)
                {
                    logger.Error("Authentication failed. Please verify the entered information and try again.Details : {0}", e);
                    throw;
                }
                catch (NotSupportedException e)
                {
                    logger.Error("Not suppor exception.Details : {0}", e);
                    throw;
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred when doExcute.Change accesserIP,retry doExcute. Details: {0} .", e);
                    workingAccesserIP = GetNextValidAccesserIP(AccesserIPHealth.Unaccessable);
                }
            }
        }

        private String Decode(String str2Decode)
        {
            return HttpUtility.UrlDecode(str2Decode);
        }

        public String CopyFileEncode(String str2Encode)
        {
            return HttpUtility.UrlEncode(str2Encode).Replace("+", "%20").Replace("/", "%2F");
        }

        protected String Encode(String str2Encode)
        {
            return HttpUtility.UrlEncode(str2Encode).Replace("+", "%20").Replace("%2f", "/").Replace("%5c", "/");//make .Net Framework4.5 happy
        }

        public virtual List<XPathNavigator> FirstStepAnalyzeXML(String xml, String xpath)
        {
            List<XPathNavigator> result = new List<XPathNavigator>();
            XPathDocument document = null;
            XPathNavigator navigator = null;
            XPathNodeIterator iterator = null;
            try
            {
                using (MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
                {
                    document = new XPathDocument(input);
                    navigator = document.CreateNavigator();
                    iterator = navigator.Select(xpath);
                    while (iterator.MoveNext())
                    {
                        result.Add(iterator.Current.CreateNavigator());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Analyze XML failes.Details : {0}.", ex);
                throw;
            }
            return result;
        }

        private CleversafeRequest GetCleversafeRequest(String uri)
        {
            CleversafeRequest request = new CleversafeRequest();
            Dictionary<String, String> headers = new Dictionary<String, String>();
            headers.Add(CleversafeConstant.DATE, CleversafeUtils.GetReqeustDate(HttpClient.TimeOffset));
            request.URI = uri;
            request.UserName = openParameter.UserName;
            request.Password = openParameter.Password;
            request.Headers = headers;
            return request;
        }

        private String GetNextValidAccesserIP(AccesserIPHealth state)
        {
            String resultIP = null;
            accesserIPMap[workingAccesserIP] = AccesserIPHealth.Unaccessable;
            foreach (var accesserHealth in accesserIPMap)
            {
                if (accesserHealth.Value > state)
                {
                    resultIP = accesserHealth.Key;
                    break;
                }
            }
            if (null == resultIP)
            {
                logger.Error("Can not find valid accesserIP While changing valid accesserIP.");
                throw new Exception("Can not find valid accesserIP while changing valid accesserIP.");
            }
            return resultIP;
        }

        private String GetStringFromResponse(HttpWebResponse resp)
        {
            if (resp == null || resp.ContentLength == 0)
            {
                return "";
            }
            StringBuilder builder = new StringBuilder();
            using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
            {
                String line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    builder.Append(line);
                }
            }
            return builder.ToString();
        }

        public CloudFileInfo GetObjectInfo(String vaultName, String objectName)
        {
            CloudFileInfo result = new CloudFileInfo();
            try
            {
                return DoExcuteWithMultiAccesserIPRetry<CloudFileInfo>(delegate ()
                {
                    String uri = BuildURL(vaultName, objectName);
                    CleversafeRequest request = GetCleversafeRequest(uri);
                    request.Method = RESTCommands.GET;
                    HttpWebRequest webRequest = HttpClient.GetHttpWebRequest(request);
                    using (HttpWebResponse response = DoExcute(webRequest))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            result.FileSize = response.ContentLength;
                        }
                        else
                        {
                            String message = GetStringFromResponse(response);
                            logger.Warn("Get object {0} info failed, message : {1} ", objectName, message);
                        }
                    }
                    return result;
                });
            }
            catch (Exception e)
            {
                logger.Error("Get object size failed, vaultName : {0} , objectName : {1} . Details : {2}", vaultName, objectName, e);
                throw;
            }
        }

        public HttpWebRequest GetUploadRequest(String vaultName, String objectName, String mimeType, HttpWebRequest webRequest, Int32 blockNumber, Int64 dataLength)
        {
            if (webRequest != null)
            {
                return webRequest;
            }
            String uri = BuildURL(vaultName, objectName);
            CleversafeRequest request = GetCleversafeRequest(uri);
            request.Method = RESTCommands.PUT;
            request.Headers.Add("Content-Type", mimeType);
            request.Headers.Add("Content-Length", dataLength.ToString());
            request.Headers.Add("Expect", "100-Continue");
            logger.Info("Get upload request, vaultName:{0}, objectName:{1}.", vaultName, objectName);
            var httpWebRequest = HttpClient.GetWebRequestForUpLoad(request);
            httpWebRequest.AllowAutoRedirect = true;
            return httpWebRequest;
        }

        Boolean HasRespContentCounts(HttpWebResponse response)
        {
            Boolean result = false;
            ResponseInfo responseInfo = new ResponseInfo();
            using (Stream inputStream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(inputStream))
                {
                    responseInfo.ResponseXml = Decode(reader.ReadToEnd());
                    responseInfo.ResponseXml = responseInfo.ResponseXml.Replace(" xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\"", "");
                    List<XPathNavigator> navigators = FirstStepAnalyzeXML(responseInfo.ResponseXml, "ListBucketResult/Contents");
                    navigators.AddRange(FirstStepAnalyzeXML(responseInfo.ResponseXml, "ListBucketResult/CommonPrefixes"));
                    if (navigators.Count > 0)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        private HttpWebResponse DoExcute(HttpWebRequest webRequest, Dictionary<String, String> headers = null)
        {
            HttpWebResponse webResponse = null;
            try
            {
                if (headers == null)
                {
                    webRequest.AllowWriteStreamBuffering = false;
                    webRequest.AllowAutoRedirect = true;
                    webRequest.Timeout = 5 * 60 * 1000;
                }
                else
                {
                    if (webRequest == null)
                    {
                        throw new Exception("HttpWebRequest is null.");
                    }
                    HttpClient.CombiningRequestWithHeaders(webRequest, headers);
                }
                webResponse = webRequest.GetResponse() as HttpWebResponse;
                HttpClient.CalcDataFlow(webRequest, webResponse);
                if (webResponse != null)
                {
                    logger.Info("HttpStatusCode=" + Convert.ToInt32(webResponse.StatusCode) + " " + webResponse.StatusCode.ToString() + "; RequestUri=" + webRequest.RequestUri.ToString());
                }
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ConnectFailure || we.Status == WebExceptionStatus.Timeout || we.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    logger.Debug("This exception is a connection fail exception:" + we.Message);
                    throw new RetryableException(we.Message, we);
                }
                else if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    using (HttpWebResponse httpWebResponse = we.Response as HttpWebResponse)
                    {
                        if (httpWebResponse.StatusCode == HttpStatusCode.GatewayTimeout)
                        {
                            throw new GatewayTimeoutException(we.Message, we);
                        }
                        if (IsServerIntertalError(httpWebResponse.StatusCode))
                        {
                            throw new RetryableException(we.Message, we);
                        }
                        else if (SpecialRetryCondition(webRequest, httpWebResponse))
                        {
                            throw new RetryableException(we.Message, we);
                        }
                        else if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new PathNotFoundException(webRequest.RequestUri.ToString(), we);
                        }
                        else if (httpWebResponse.StatusCode == HttpStatusCode.Forbidden || httpWebResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            logger.Error("There is a exception when get response from server {0}", httpWebResponse.StatusDescription);
                            throw new AuthenticationFailedException(we.ToString() + "\r\n" + webRequest.RequestUri.ToString(), we);
                        }
                        else
                        {
                            throw new UnknownException(webRequest.RequestUri.ToString(), we);
                        }
                    }
                }
                else if (we.Status == WebExceptionStatus.NameResolutionFailure || we.Status == WebExceptionStatus.ConnectionClosed)
                {
                    throw new AuthenticationFailedException(CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Cannot_connect_to_the_remote_server", AbstractXSystem.Culture));
                }
                else
                {
                    throw new UnknownException(webRequest.RequestUri.ToString(), we);
                }
            }
            catch (Exception e)
            {
                logger.Error("DoExecute error: {0}", e.Message);
                try
                {
                    if (webRequest != null)
                    {
                        webRequest.Abort();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred while closing web request,details{0}. ", ex.Message);
                }
                throw;
            }
            return webResponse;
        }

        public Boolean IsServerIntertalError(HttpStatusCode code)
        {
            Boolean restult = false;
            if (code == HttpStatusCode.InternalServerError || code == HttpStatusCode.RequestTimeout || code == HttpStatusCode.ServiceUnavailable)
            {
                restult = true;
            }
            return restult;
        }

        public List<String> ListContainers()
        {
            throw new NotSupportedException();
        }

        public List<String> ListObject(String vaultName, String prefix)
        {
            List<String> objFullNames = ListObjectWithPreFix(vaultName, prefix, true);
            List<String> objNames = new List<String>();
            foreach (String name in objFullNames)
            {
                String xStreamName = name.Substring(name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
                if (!String.IsNullOrEmpty(xStreamName))
                {
                    objNames.Add(xStreamName);
                }
            }
            return objNames;
        }

        protected virtual List<string> ListObjectWithPreFix(String vaultName, String prefix, Boolean isGetName)
        {
            List<string> objects = new List<string>();
            String maker = String.Empty;
            while (true)
            {
                if (objects.Count != 0)
                {
                    maker = objects[objects.Count - 1];
                }
                List<string> files = ListXstream(vaultName, prefix, MAX_OBJECT_COUNT, maker, isGetName);
                if (files.Count < MAX_OBJECT_COUNT)
                {
                    if (objects.Count == 0)
                    {
                        return files;
                    }
                    else
                    {
                        foreach (String name in files)
                        {
                            objects.Add(name);
                        }
                        return objects;
                    }
                }
                else
                {
                    foreach (String name in files)
                    {
                        objects.Add(name);
                    }
                }
            }
        }

        protected List<String> ListXstream(String valutName, String prefix, Int32 limit, String marker, Boolean isGetName)
        {
            List<String> result = null;
            CleversafeRequest request = null;
            try
            {
                Dictionary<String, String> paramaters = new Dictionary<String, String>();
                if (String.IsNullOrEmpty(prefix))
                {
                    paramaters.Add("delimiter", "/");
                }
                else
                {
                    if (prefix.Contains("\\"))
                    {
                        prefix = prefix.Replace("\\", "/");
                    }
                    paramaters.Add(CleversafeConstant.PREFIX, prefix);
                }
                if (!String.IsNullOrEmpty(marker))
                {
                    paramaters.Add(CleversafeConstant.MARKER, marker);
                }
                if (limit > 0)
                {
                    paramaters.Add(CleversafeConstant.MAX_KEYS, limit.ToString());
                }
                String queryStr = ConvertQueryList2String(paramaters);
                return DoExcuteWithMultiAccesserIPRetry<List<String>>(delegate ()
                {
                    String baseUri = BuildURL(valutName);
                    request = GetCleversafeRequest(baseUri + queryStr);
                    request.Method = RESTCommands.GET;
                    HttpWebRequest webRequest = HttpClient.GetHttpWebRequest(request);
                    using (HttpWebResponse response = DoExcute(webRequest))
                    {
                        String message = GetStringFromResponse(response);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            result = isGetName ? ExtractMessageWithNP(message, "Contents/Key") : ExtractMessageWithNP(message, "Contents/Size");
                        }
                    }
                    return result;
                });
            }
            catch (Exception e)
            {
                logger.Error("List Xstream failed, vauleName : {0}, prefix:{1}. Details : {2}", valutName, prefix, e.Message);
                throw;
            }
        }

        protected List<String> ExtractMessageWithNP(String xml, String xPath)
        {
            List<String> result = null;
            Dictionary<String, String> dictionary = new Dictionary<String, String>();
            dictionary.Add("amazon", "http://s3.amazonaws.com/doc/2006-03-01/");
            result = AnalyzeXML(xml, xPath, dictionary);
            return result;
        }

        public List<String> AnalyzeXML(String xml, String xpath, Dictionary<String, String> np)
        {
            List<string> result = null;
            try
            {
                Stream stream = GetStreamByString(xml);
                result = AnalyzeXML(stream, xpath, np);
            }
            catch (Exception ex)
            {
                logger.Error("Analyze XML failed. Details: {0}", ex);
                throw;
            }
            return result;
        }

        public Stream GetStreamByString(String strSource)
        {
            byte[] bSource = Encoding.UTF8.GetBytes(strSource);
            Stream stream = new MemoryStream(bSource);
            return stream;
        }

        protected String ConvertQueryList2String(Dictionary<String, String> parameters)
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

        public List<String> AnalyzeXML(Stream input, String xpath, Dictionary<String, String> dictionary)
        {
            List<String> result = new List<String>();
            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager nameSpaceManager = new XmlNamespaceManager(doc.NameTable);
            XmlNodeList nodeList = null;
            String[] elements = xpath.Split('/');
            try
            {
                using (input)
                {
                    doc.Load(input);
                    foreach (String key in dictionary.Keys)
                    {
                        nameSpaceManager.AddNamespace(key, dictionary[key]);
                        StringBuilder path = new StringBuilder();
                        path.Append("//");
                        Boolean first = true;
                        foreach (String el in elements)
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

                        nodeList = doc.SelectNodes(path.ToString(), nameSpaceManager);
                        foreach (XmlNode node in nodeList)
                        {
                            result.Add(node.InnerText);
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Analyze XML failed.Detailes : {0}. ", ex);
                throw;
            }
            return result;
        }

        public ResponseInfo ListObjects(String baseURL, Dictionary<String, String> queryParams)
        {
            ResponseInfo responseInfo = new ResponseInfo();
            return DoExcuteWithMultiAccesserIPRetry<ResponseInfo>(delegate ()
            {
                HttpWebRequest requestGet = HttpClient.CreateRequestGet(baseURL, queryParams);
                using (HttpWebResponse response = DoExcute(requestGet, Headers))
                {
                    using (Stream inputStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(inputStream))
                        {
                            responseInfo.ResponseXml = Decode(reader.ReadToEnd());
                            return responseInfo;
                        }
                    }
                }
            });
        }

        private Boolean SpecialRetryCondition(HttpWebRequest request, HttpWebResponse httpWebResponse)
        {
            return false;
        }

        private Boolean SpecialRetryCondition(BasicRequest request, HttpWebResponse response)
        {
            return false;
        }

        private void SetIP()
        {
            foreach (var item in openParameter.AccesserIPs)
            {
                accesserIPMap[item] = AccesserIPHealth.Accessable;
            }
        }

        public XStream OpenObjectForWrite(String fullURL, Dictionary<String, String> writerHeaders)
        {

            long length = Convert.ToInt64(writerHeaders["Content-Length"]);
            if (length > openParameter.BlockLength * 1024 * 1024)
            {
                return new CleversafeMultipartUploadStream(this, fullURL, writerHeaders);
            }
            else
            {
                HttpWebRequest request = HttpClient.CreateRequestPut(fullURL, null);
                HttpClient.AddHeaders(request, writerHeaders);
                return new CleversafeUploadStream(request, openParameter) { HttpClient = this.HttpClient, System = this.HttpClient.CurrentSystem };
            }
        }

        internal XStream OpenObjectForRead(String fullURL, Dictionary<String, String> readHeaders)
        {
            return DoExcuteWithMultiAccesserIPRetry<HttpDownloadStream>(delegate ()
            {
                HttpWebRequest request = HttpClient.CreateRequestGet(fullURL, null);
                return new HttpDownloadStream(DoExcute(request, readHeaders)) { System = this.HttpClient.CurrentSystem };
            });
        }

        public String UploadPart(String fullURL, Byte[] buffer, Int32 offset, Int32 count)
        {
            return RetryRequset.CloudRetry<String>(delegate ()
            {
                HttpWebRequest request = HttpClient.CreateRequestPut(fullURL, null);
                request.ContentLength = count;
                HttpClient.AddHeaders(request, this.Headers);
                request.AllowWriteStreamBuffering = false;
                request.AllowAutoRedirect = true;
                request.Timeout = 0x7ffffffe; //never timeout              
                CleversafeUtils.AddAuthorization(request, openParameter.UserName, openParameter.Password);
                using (Stream upStream = request.GetRequestStream())
                {
                    upStream.Write(buffer, 0, count);
                }
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        logger.Error("Upload Part failed, url='{0}',HttpStatusCode={1}.", response.ResponseUri, response.StatusCode);
                        throw new Exception(String.Format("Upload Part failed, url='{0}' HttpStatusCode={1}", response.ResponseUri, response.StatusCode));
                    }
                    String eTag = response.Headers["ETag"];
                    logger.Info("Upload Part succeed, eTag={0} url='{1}'", eTag, fullURL);
                    return eTag;
                }
            });
        }

        internal SpaceInfo GetUserAccountInfo()
        {
            throw new NotImplementedException();
        }
    }
}
