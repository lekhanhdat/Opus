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
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using AvePoint.Media.Storage.Util;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    class OpenStackBaseRestClient
    {
        private StorageLogger logger = StorageLogger.GetInstance(typeof(OpenStackBaseRestClient));
        private OpenStackOpenParameter openParameter;

        protected String tenant;
        protected String username;
        protected String password;
        protected String authenticationURL;
        protected String storageURL;
        protected String cdnStorageURL;
        protected Boolean cdnEnabled;
        protected String cdnURL;
        protected String authToken;
        protected OpenStackIdentityInfo openStackIdentityInfo;

        public OpenStackBaseRestClient(OpenStackOpenParameter openParameter)
        {
            this.InitConfig(openParameter);
        }

        public virtual void InitConfig(OpenStackOpenParameter openParameter)
        {
            this.openParameter = openParameter;
            this.tenant = openParameter.TenantName;
            this.username = openParameter.UserName;
            this.password = openParameter.Password;
            this.authenticationURL = openParameter.AuthenticationURL;
            this.cdnEnabled = openParameter.CdnEnabled;
        }

        public virtual Boolean CheckContainer(String containerName)
        {
            var result = false; // TODO 可以去掉这个局部变量
            try
            {
                var url = BuildURL(GetStorageUrl(), containerName);
                var urlParameters = new Dictionary<String, String> { { "limit", "1" } };
                using (var response = RetryExecuteWebRequest(url, OpenStackConstants.HttpMethod_GET, null, urlParameters))
                {
                    if (response != null && (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK))
                    {
                        result = true;
                        var containerObjectCount = response.Headers.Get("X-Container-Object-Count");
                        var containerBytesUsed = response.Headers.Get("X-Container-Bytes-Used");
                        logger.Info("Container already exists, container : " + containerName + " " + containerObjectCount + " " + containerBytesUsed);
                    }
                }
                return result;
            }
            catch (PathNotFoundException)
            {
                result = false;
            }
            return result;
        }

        public Boolean CreateContainer(String containerName, Dictionary<String, String> headerParameters = null, Dictionary<String, String> urlParameters = null)
        {
            var result = default(Boolean); // TODO 可以去掉这个局部变量,而且永远都不会返回False，随意可以是Void
            var url = BuildURL(GetStorageUrl(), containerName);
            using (var response = RetryExecuteWebRequest(url, OpenStackConstants.HttpMethod_PUT, headerParameters, urlParameters))
            {
                if (response != null && (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.NoContent))
                {
                    result = true;
                    logger.Info("Create container successfully, name : " + containerName);
                }
            }
            return result;
        }

        //TODO
        public Boolean CreateObjectWithNoContent(String containerName, String objectName, Dictionary<String, String> headerParameters = null, Dictionary<String, String> urlParameters = null)
        {
            bool result = false; // TODO 可以去掉这个局部变量,而且永远都不会返回False，所以可以是Void
            var fullURL = BuildURL(GetStorageUrl(), containerName, objectName);
            using (var response = RetryExecuteWebRequest(fullURL, OpenStackConstants.HttpMethod_PUT, headerParameters, urlParameters))
            {
                if (response != null && (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.Accepted))
                {
                    result = true;
                    logger.Info("Create object successfully, name : " + containerName + "/" + objectName);
                }
            }
            return result;
        }

        public Boolean CheckObject(String containerName, String objectName)
        {
            var result = default(Boolean);
            String url;
            try
            {
                url = BuildURL(GetStorageUrl(), containerName, objectName);
                using (var response = RetryExecuteWebRequest(url, OpenStackConstants.HttpMethod_HEAD, null, null))
                {
                    if (response != null && (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK))
                    {
                        result = true;
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                logger.Warn("Cannot find object, maybe it was deleted successful : {0}  , details : {1}", objectName, e);
                result = false;
            }
            catch (Exception e)
            {
                try
                {
                    logger.Warn("Occurred a error when check Object : {0}  , details : {1}", objectName, e);
                    url = BuildURL(GetStorageUrl(), containerName, objectName); //TODO 不需要重复生成一遍
                    using (var response = ExecuteWebRequest(url, OpenStackConstants.HttpMethod_GET, null, null)) //TODO 虽然方法写的意思是不用Retey但是最后调的是同样的方法
                    {
                        if (response != null && (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK))
                        {
                            result = true;
                        }
                    }
                }
                catch (PathNotFoundException ex)
                {
                    logger.Warn("Cannot find object, maybe it was deleted successful : {0}  , details : {1}", objectName, ex);
                    result = false;
                }
            }
            return result;
        }

        internal OpenStackFileInfo GetObjectInfo(String containerName, String objectName)
        {
            OpenStackFileInfo result = null;
            String url;
            try
            {
                url = BuildURL(GetStorageUrl(), containerName, objectName);
                using (var response = RetryExecuteWebRequest(url, OpenStackConstants.HttpMethod_HEAD, null, null))
                {
                    if (response != null && (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK))
                    {
                        result = new OpenStackFileInfo(containerName, objectName, response.ContentLength);
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                logger.Warn("Cannot find object : {0}  , details : {1}", objectName, e);
                result = null;
            }
            catch (Exception e)
            {
                try
                {
                    logger.Warn("Occurred a error when get Object : {0} info, details : {1}", objectName, e);
                    url = BuildURL(GetStorageUrl(), containerName, objectName);
                    using (var response = ExecuteWebRequest(url, OpenStackConstants.HttpMethod_GET, null, null))
                    {
                        if (response != null && (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK))
                        {
                            result = new OpenStackFileInfo(containerName, objectName, response.ContentLength);
                        }
                    }
                }
                catch (PathNotFoundException ex)
                {
                    logger.Warn("Cannot find object : {0}  , details : {1}", objectName, ex);
                    result = null;
                }
            }
            return result;
        }

        //删除一个对象
        public Boolean DeleteObject(String containerName, String objectName, Dictionary<String, String> headerParameters = null, Dictionary<String, String> urlParameters = null)
        {
            var result = default(Boolean);
            try
            {
                var url = BuildURL(GetStorageUrl(), containerName, objectName);
                using (var response = RetryExecuteWebRequest(url, OpenStackConstants.HttpMethod_DELETE, headerParameters, urlParameters))
                {
                    if (response != null && (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK))
                    {
                        logger.Info("Delete object successfully : {0}", objectName);
                        result = true;
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                logger.Warn("Cannot find object, maybe it was deleted successful : {0}  , details : {1}", objectName, e.Message);
                result = true;
            }
            return result;
        }

        public Boolean BulkDelete(String deleteContent, Dictionary<String, String> headerParameters = null, Dictionary<String, String> urlParameters = null)
        {
            var result = default(Boolean);
            try
            {
                var url = GetStorageUrl() + "?bulk-delete";
                //var webRequest = GetWebRequest(url, "delete", headerParameters, urlParameters);
                //TODO
                //webRequest.Accept = "application/json";
                using (var response = RetryExecuteWebRequest(url, "delete", headerParameters, urlParameters, Encoding.UTF8.GetBytes(deleteContent))) //TODO 为什么不用 HttpMethod_DELETE
                {
                    if (response != null && (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK))
                    {
                        logger.Info("Delete object successfully : {0}", AveHttpWebRequestUtil.GetResopnseString(response));
                        result = true;
                    }
                }
            }
            catch (PathNotFoundException e) //TODO 批量删除时某一个文件夹不存在怎么办？？？
            {
                logger.Warn("Cannot find object, may it was deleted successful : " + e.Message);
                result = true;
            }
            return result;
        }

        //list 出前缀是 prefixName 的所有object，使用时注意内存
        public List<XFileInfo> ListAllObjects(String containerName, String prefixName, Dictionary<String, String> urlParameters = null)
        {
            if (urlParameters == null)
                urlParameters = new Dictionary<String, String>();
            if (!String.IsNullOrEmpty(prefixName) && !"/".Equals(prefixName, StringComparison.CurrentCultureIgnoreCase))
            {
                urlParameters.Add("prefix", prefixName);
            }
            var result = this.ListAllDirFilesByPage(containerName, prefixName, urlParameters);
            return result.Files;
        }

        public StorageListResult ListAllDirFiles(String containerName, String prefixName, Dictionary<String, String> headerParameters = null, Dictionary<String, String> urlParameters = null)
        {
            if (urlParameters == null)
                urlParameters = new Dictionary<String, String>();
            if (!String.IsNullOrEmpty(prefixName) && !"/".Equals(prefixName, StringComparison.CurrentCultureIgnoreCase))
            {
                urlParameters.Add("prefix", prefixName);
            }
            urlParameters.Add("delimiter", "/");
            var result = this.ListAllDirFilesByPage(containerName, prefixName, urlParameters);
            return result;
        }

        private StorageListResult ListAllDirFilesByPage(String containerName, String prefixName, Dictionary<String, String> urlParameters = null)
        {
            var limitCount = 1000;
            if (urlParameters == null)
                urlParameters = new Dictionary<String, String>();
            if (!urlParameters.ContainsKey("limit"))
            {
                urlParameters.Add("limit", limitCount + "");
            }
            else
            {
                limitCount = Int32.Parse(urlParameters["limit"]);
            }
            var marker = "";
            var nextMarker = "";
            var results = new StorageListResult();
            while (true)
            {
                var subResult = ListObjects(containerName, prefixName, marker, ref nextMarker, urlParameters);
                results.SubDirs.AddRange(subResult.SubDirs);
                results.Files.AddRange(subResult.Files);
                if (subResult.SubDirs.Count + subResult.Files.Count < limitCount)
                {
                    break;
                }
                marker = nextMarker;
            }
            return results;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "subdir")]
        //list 指定prefixName下的所有object。 
        public StorageListResult ListObjects(String containerName, String prefixName, String marker, ref String nextMarker, Dictionary<String, String> urlParameters = null)
        {
            var dirs = new List<XDirectoryInfo>();
            var files = new List<XFileInfo>();
            if (!String.IsNullOrEmpty(marker))
            {
                urlParameters["marker"] = marker;
            }
            var responseXmlString = this.ExcuteListObjects(containerName, urlParameters);
            var js = new JavaScriptSerializer();
            var jsonData = (Object[])js.DeserializeObject(responseXmlString);
            foreach (var dirOrFile in jsonData)
            {
                var tempInfo = dirOrFile as Dictionary<String, Object>;
                if (tempInfo.ContainsKey("subdir"))
                {
                    var name = (tempInfo["subdir"] as String);
                    nextMarker = name;
                    //name = name.RemoveFirst(prefixName).TrimEnd('/').TrimStart('/');
                    name = name.RemoveFirst(prefixName).Trim('/');
                    var dir = new OpenStackDirectoryInfo(prefixName, name, true);
                    dirs.Add(dir);
                }
                else
                {
                    var name = (tempInfo["name"] as String);
                    nextMarker = name;
                    //name = name.RemoveFirst(prefixName).TrimEnd('/').TrimStart('/');
                    name = name.RemoveFirst(prefixName).Trim('/');
                    var size = Int64.Parse(tempInfo["bytes"].ToString());
                    var file = new OpenStackFileInfo(prefixName, name, size);
                    files.Add(file);
                }
            }
            var results = new StorageListResult { SubDirs = dirs, Files = files };
            return results;
        }

        private String ExcuteListObjects(String containerName, Dictionary<String, String> queryParams)
        {
            var url = BuildURL(GetStorageUrl(), containerName);
            using (var response = RetryExecuteWebRequest(url, OpenStackConstants.HttpMethod_GET, null, queryParams))
            {
                using (var inputStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(inputStream))
                    {
                        var result = HttpUtility.UrlDecode(reader.ReadToEnd());
                        return result;
                    }
                }
            }
        }

        public Boolean CopyFile(String sourceContainerName, String sourceObjectName, String destContainerName, String destObjectName)
        {
            var result = default(Boolean);
            var headParams = new Dictionary<String, String>();
            var destURL = HttpUtil.Encode(destContainerName) + "/" + HttpUtil.Encode(destObjectName);
            headParams.Add("Destination", destURL);
            var url = BuildURL(GetStorageUrl(), sourceContainerName, sourceObjectName);
            using (var response = RetryExecuteWebRequest(url, OpenStackConstants.HttpMethod_COPY, headParams, null))
            {
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    result = true;
                }
                else
                {
                    logger.Error("CopyFile failed {0}", url);
                }
            }
            return result;
        }

        public HttpWebRequest UploadObjectRequest(String containerName, String objectName, Dictionary<String, String> headerParameters = null, Dictionary<String, String> urlParameters = null)
        {
            var url = BuildURL(GetStorageUrl(), containerName, objectName);
            var webRequest = GetWebRequest(url, OpenStackConstants.HttpMethod_PUT, headerParameters, urlParameters);
            return webRequest;
        }

        //TODO 没有调用
        //public HttpWebRequest DownloadObjectRequest(String containerName, String objectName, Dictionary<String, String> headerParameters = null, Dictionary<String, String> urlParameters = null)
        //{
        //    String fullURL;
        //    if (this.cdnEnabled)
        //    {
        //        if (String.IsNullOrEmpty(cdnURL))
        //        {
        //            cdnURL = this.InitCDNURL(containerName);
        //        }
        //        fullURL = BuildURL(cdnURL, objectName);
        //    }
        //    else
        //    {
        //        fullURL = BuildURL(GetStorageUrl(), containerName, objectName);
        //    }
        //    var webRequest = GetWebRequest(fullURL, OpenStackConstants.HttpMethod_GET, headerParameters, urlParameters);
        //    return webRequest;
        //}

        public HttpWebResponse DownloadObjectResponse(String containerName, String objectName, Dictionary<String, String> headerParameters = null, Dictionary<String, String> urlParameters = null)
        {
            String fullURL;
            if (this.cdnEnabled)
            {
                if (String.IsNullOrEmpty(cdnURL))
                {
                    cdnURL = this.InitCDNURL(containerName);
                }
                fullURL = BuildURL(cdnURL, objectName);
            }
            else
            {
                fullURL = BuildURL(GetStorageUrl(), containerName, objectName);
            }
            var webResponse = RetryExecuteWebRequest(fullURL, OpenStackConstants.HttpMethod_GET, headerParameters, urlParameters);
            return webResponse;
        }

        public void Close()
        {
        }

        public OpenStackIdentityInfo Authentication()
        {
            lock (this)
            {
                this.openStackIdentityInfo = OpenStackIdentityService.GetIdentityService(this.openParameter).Authentication();
                if (!this.openStackIdentityInfo.HasAuthentication)
                {
                    throw new AuthenticationFailedException(this.openStackIdentityInfo.ErrorMessage);
                }
                this.storageURL = this.openStackIdentityInfo.StorageURL;
                this.cdnStorageURL = this.openStackIdentityInfo.CdnURL;
                this.authToken = this.openStackIdentityInfo.AuthToken;
                return this.openStackIdentityInfo;
            }
        }

        public String GetStorageUrl()
        {
            if (String.IsNullOrEmpty(this.storageURL))
            {
                this.Authentication();
            }
            return this.storageURL;
        }

        public String GetCDNStorageUrl()
        {
            if (String.IsNullOrEmpty(cdnStorageURL))
            {
                Authentication();
            }
            return this.cdnStorageURL;
        }

        private String InitCDNURL(String containerName)
        {
            var result = default(String);
            try
            {
                var url = BuildURL(GetCDNStorageUrl(), containerName);
                var headerParameters = new Dictionary<String, String> { { OpenStackConstants.X_CDN_ENABLED, "true" } };
                using (var response = RetryExecuteWebRequest(url, OpenStackConstants.HttpMethod_PUT, headerParameters, null))
                {
                    //if (response != null && response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.Created) //TODO 
                    if (response != null && (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.Created))
                    {
                        result = response.Headers[OpenStackConstants.X_CDN_URI];
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Init CDN failed, container : " + containerName + ", " + e.Message, e);
                throw;
            }
            return result;
        }

        private String BuildURL(String baseURL, String xSetName)
        {
            return baseURL + "/" + HttpUtil.Encode(xSetName);
        }

        private String BuildURL(String baseURL, String xSetName, String xStreamName)
        {
            return xStreamName.Equals("/", StringComparison.OrdinalIgnoreCase)
                       ? this.BuildURL(baseURL, xSetName) + "/"
                       : this.BuildURL(baseURL, xSetName) + "/" + HttpUtil.Encode(xStreamName);
        }

        protected HttpWebRequest GetWebRequest(string containerName, string objectName, string httpMethod, Dictionary<string, string> headerParameters = null, Dictionary<string, string> urlParameters = null)
        {
            string fullURL = null;
            if (string.IsNullOrEmpty(objectName))
            {
                fullURL = BuildURL(GetStorageUrl(), containerName);
            }
            else
            {
                fullURL = BuildURL(GetStorageUrl(), containerName, objectName);
            }

            return GetWebRequest(fullURL, httpMethod, headerParameters, urlParameters);
        }

        protected HttpWebRequest GetWebRequest(String url, String httpMethod, Dictionary<String, String> headerParameters = null, Dictionary<String, String> urlParameters = null)
        {
            if (urlParameters == null)
            {
                urlParameters = new Dictionary<String, String>();
            }
            urlParameters["format"] = "json"; ;
            var fullURL = HttpUtil.CombiningQueryParams(url, urlParameters);
            var webRequest = WebRequest.Create(fullURL) as HttpWebRequest;
            webRequest.Method = httpMethod;
            if (headerParameters != null)
            {
                HttpUtil.CombiningRequestWithHeaders(webRequest, headerParameters);
            }
            webRequest.Headers[OpenStackConstants.X_AUTH_TOKEN] = this.authToken;
            //webRequest.Accept = "application/json";
            return webRequest;
        }

        public HttpWebResponse ExecuteWebRequest(string url, string httpMethod, Dictionary<string, string> headerParameters, Dictionary<string, string> urlParameters)
        {
            return RetryExecuteWebRequest(url, httpMethod, headerParameters, urlParameters, null, 0, 0);
        }

        public HttpWebResponse RetryExecuteWebRequest(string url, string httpMethod, Dictionary<string, string> headerParameters = null, Dictionary<string, string> urlParameters = null)
        {
            return RetryExecuteWebRequest(url, httpMethod, headerParameters, urlParameters, null, openParameter.MaxRetryCount, openParameter.RetryInterval);
        }

        public HttpWebResponse ExecuteWebRequest(string url, string httpMethod, Dictionary<string, string> headerParameters = null, Dictionary<string, string> urlParameters = null, byte[] content = null)
        {
            return RetryExecuteWebRequest(url, httpMethod, headerParameters, urlParameters, content, 0, 0);
        }

        public HttpWebResponse RetryExecuteWebRequest(string url, string httpMethod, Dictionary<string, string> headerParameters = null, Dictionary<string, string> urlParameters = null, byte[] content = null)
        {
            return RetryExecuteWebRequest(url, httpMethod, headerParameters, urlParameters, content, openParameter.MaxRetryCount, openParameter.RetryInterval);
        }

        public HttpWebResponse RetryExecuteWebRequest(String url, String httpMethod, Dictionary<String, String> headerParameters, Dictionary<String, String> urlParameters, Byte[] content, Int32 maxRetryCount, Int32 retryInterval)
        {
            var retryCount = default(Int32);
            var retryNow = default(Boolean);
            while (true)
            {
                try
                {
                    var webRequest = GetWebRequest(url, httpMethod, headerParameters, urlParameters);
                    if (content != null && content.Length > 0)
                    {
                        webRequest.ContentLength = content.Length;
                        webRequest.GetRequestStream().Write(content, 0, content.Length);
                    }
                    var webResponse = webRequest.GetResponse() as HttpWebResponse;
                    return webResponse;
                }
                catch (WebException ex)
                {
                    if (maxRetryCount == 0)
                        throw;
                    if (retryCount > maxRetryCount)
                    {
                        logger.Error("too many retry failed. Retry count:{0}, msg:{1}", retryCount, ex.Message, ex);
                        throw;
                    }
                    switch (ex.Status)
                    {
                        case WebExceptionStatus.ProtocolError:
                            using (var httpWebResponse = ex.Response as HttpWebResponse)
                            {
                                var body = AveHttpWebRequestUtil.GetResopnseString(httpWebResponse);
                                this.logger.Error("execute request failed:{0}, response body:{1}:", ex, body);
                                if (httpWebResponse != null && (httpWebResponse.StatusCode == HttpStatusCode.Unauthorized || (Int32)httpWebResponse.StatusCode == 420))
                                {
                                    this.Authentication();
                                    retryNow = true;
                                }
                                else if (httpWebResponse != null && httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                                {
                                    throw new PathNotFoundException(ex.Message, ex);
                                }
                                else if (httpWebResponse != null && (httpWebResponse.StatusCode == HttpStatusCode.InternalServerError || httpWebResponse.StatusCode == HttpStatusCode.RequestTimeout || httpWebResponse.StatusCode == HttpStatusCode.ServiceUnavailable))
                                {
                                    this.logger.Warn("this exception is a connection fail exception:{0}", ex);
                                }
                                else
                                {
                                    this.logger.Error("execute request failed:{0}, response body:{1}:", ex, body);
                                    throw;
                                }
                            }
                            break;
                        case WebExceptionStatus.Timeout:
                        case WebExceptionStatus.NameResolutionFailure:
                        case WebExceptionStatus.ConnectFailure:
                        case WebExceptionStatus.ConnectionClosed:
                            this.logger.Info("this exception is a connection fail exception:{0}", ex);
                            break;
                        default:
                            this.logger.Error("execute request failed:{0}", ex);
                            throw;
                    }
                    if (retryCount >= maxRetryCount)
                    {
                        throw;
                    }
                    retryCount++;
                    if (retryNow)
                    {
                        this.logger.Info("Retry now. Retry count: {0}", retryCount);
                        retryNow = false;
                    }
                    else
                    {
                        this.logger.Info("Retry after " + retryInterval + " ms. Retry count: " + retryCount);
                        Thread.Sleep(retryInterval);
                    }
                }
            }
        }
    }
}
