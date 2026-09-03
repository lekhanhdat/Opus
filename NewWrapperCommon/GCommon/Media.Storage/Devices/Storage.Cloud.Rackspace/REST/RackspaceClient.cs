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



namespace AvePoint.Media.Storage.Cloud.Rackspace
{
    #region using directives
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Xml.XPath;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/6/21",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
     null,
     true)]
    [AveCodeReview(
   "2012/8/9",
   "rongbiao.sun@avepoint.com",
   "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_LOG_1 },
    null,
    true)]
    #endregion
    class RackspaceClient : AbstractRESTOprationExecutor, ICloudOprationExecutor, IHttpRequestPrepare, IHttpResponseHandler
    {
        private string username;
        private string password;
        private bool isCDNEabled;
        private string storageURL;
        private string cdnStorageURL;
        private string authToken;
        private bool isCDNFalied;
        private RackspaceOpenParameter openParams;
        string cdnURL = string.Empty;
        string lowName = string.Empty;

        public string GetStorageUrl()
        {
            if (string.IsNullOrEmpty(storageURL))
            {
                Login(null);
            }
            return this.storageURL;
        }

        public string GetCDNStorageUrl()
        {
            if (string.IsNullOrEmpty(cdnStorageURL))
            {
                Login(null);
            }
            return this.cdnStorageURL;
        }
        #region Constructor
        public RackspaceClient()
        {
            Logger = StorageLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            HttpClient = new RackspaceHttpClient();
        }
        #endregion

        #region ICloudOprationExecutor Members
        public override void InitConfig(CloudOpenParameter prams)
        {
            this.openParams = prams as RackspaceOpenParameter;
            this.username = prams.UserName;
            this.password = prams.Password;
            this.isCDNEabled = prams.CdnEnaled;
            HttpClient.OpenParam = this.openParams;
            this.CloudOpenParam = openParams;
            base.InitProxySetting();
            InitRetry(prams);
        }

        public override List<string> ListContainers()
        {
            List<string> names = new List<string>();
            try
            {
                RackspaceHttpRequest request = GetOperateRequest(GetStorageUrl());
                request.Method = RESTCommands.GET;

                /*
                 *  A 204 (No Content) HTTP return code will be passed back
                 *  if the account has no containers.
                 */
                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream inputStream = resp.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(inputStream))
                            {
                                string line = null;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    names.Add(line);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("List containers failed: {0}", e);
                throw;
            }

            return names;
        }

        public override bool Login(string xSetName)
        {
            bool result = false;

            try
            {
                RackspaceHttpRequest request = GetAuthRequest(StorageUrl.Rackspace);
                request.Method = RESTCommands.GET;

                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response != null && response.StatusCode == HttpStatusCode.NoContent)
                    {
                        storageURL = response.Headers[RackspaceConstants.X_STORAGE_URL];
                        cdnStorageURL = response.Headers[RackspaceConstants.X_CDN_MANAGEMENT_URL];
                        authToken = response.Headers[RackspaceConstants.X_AUTH_TOKEN];
                        result = true;
                        Logger.Info("Login Succeed.");
                    }
                }
            }
            catch (Exception t)
            {
                Logger.Error("Login failed: {0}", t);
                throw;
            }
            return result;
        }

        //在每一种Cloud中重写这个方法就是为了把openParams.FlushDNS这个参数默认false，这个属性Cloud中不会用到
        public override void InitRetry(CloudOpenParameter openParams)
        {
            Logger.Info("Init Retry: retryCount {0}, RetryInterval {1}", openParams.MaxRetryCount, openParams.RetryInterval);
            RetryRequset = new Retry(openParams.MaxRetryCount, openParams.RetryInterval, openParams.NeedRetry, true);
        }

        public override bool CreateContainer(string xSetName)
        {
            bool result = false;
            try
            {
                string url = BuildURL(GetStorageUrl(), xSetName);
                RackspaceHttpRequest request = GetOperateRequest(url);
                request.Headers["Content-Length"] = "0";
                request.Method = RESTCommands.PUT;

                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp != null && (resp.StatusCode == HttpStatusCode.Created || resp.StatusCode == HttpStatusCode.Accepted))
                    {
                        result = true;
                        Logger.Info("Create container successfully, name : {0}", xSetName);
                    }
                }
            }
            catch (Exception t)
            {
                Logger.Error(GetCreateCtnErrorMsg(xSetName) + "{0}", t);
                throw;
            }
            return result;
        }

        public override StorageOpenValidResult GetPermissions()
        {
            return new StorageOpenValidResult()
            {
                IsHasPermission = Login(null),
                TotalSpace = long.MaxValue - 1,
                TotalFreeSpace = long.MaxValue - 1,
                TotalUsedSpace = 0
            };
        }

        public override bool CheckContainer(string xSetName)
        {
            bool result = false;
            try
            {
                string url = BuildURL(GetStorageUrl(), xSetName);
                RackspaceHttpRequest request = GetOperateRequest(url);
                request.Method = RESTCommands.GET;

                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp != null && (resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.OK))
                    {
                        Logger.Info("Container already exists, container : {0}", xSetName);
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
                Logger.Error(GetCheckCtnErrorMsg(xSetName) + "{0}", e);
                throw;
            }
            return result;
        }

        public bool CheckFolderForConnector(string xSetName, string xStreamName)
        {
            bool result = false;
            RackspaceHttpRequest request;
            try
            {
                String url = BuildURL(GetStorageUrl(), xSetName, xStreamName);
                request = GetOperateRequest(url);
                request.Method = RESTCommands.GET;
                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }

                    if (resp != null && resp.StatusCode == HttpStatusCode.NoContent && "/".Equals(xStreamName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        result = true;
                    }
                }
            }
            catch (PathNotFoundException ex)
            {
                Trace.TraceError(ex.Message, ex);
                try
                {
                    String url = BuildURL(GetStorageUrl(), xSetName, xStreamName.TrimEnd('/'));
                    request = GetOperateRequest(url);
                    request.Method = RESTCommands.GET;
                    using (HttpWebResponse resp = DoExecute(request))
                    {
                        if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                        {
                            result = true;
                        }
                    }
                }
                catch (PathNotFoundException e)
                {
                    Trace.TraceError(e.Message, e);
                    result = false;
                }
            }
            catch (Exception e)
            {
                Logger.Error(GetCheckObjErrorMsg(xStreamName, xSetName) + "{0}", e);
                throw;
            }

            return result;
        }

        public override bool DeleteContainer(string xSetName)
        {
            bool result = false;
            Logger.Debug("Try to delete container: {0}", xSetName);
            xSetName = ConvertContainerName(xSetName);
            try
            {
                List<string> objectsNames = ListObject(xSetName);
                foreach (string objectsName in objectsNames)
                {
                    DeleteObject(xSetName, objectsName);
                }

                string url = BuildURL(GetStorageUrl(), xSetName);
                RackspaceHttpRequest request = GetOperateRequest(url);
                request.Method = RESTCommands.DELETE;

                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp.StatusCode == HttpStatusCode.NoContent)
                    {
                        Logger.Debug("Delete container successfully: {0}", xSetName);
                        result = true;
                    }
                }
            }
            catch (PathNotFoundException t)
            {
                Logger.Warn("Cannot find path, think it deleted successful: {0}", t);
                result = true;
            }
            catch (Exception e)
            {
                Logger.Error(GetDeleteCtnErrorMsg(xSetName) + "{0}", e);
                throw;
            }
            return result;
        }

        public override List<string> ListObject(string xSetName)
        {
            return ListObject(xSetName, null);
        }

        public override List<string> ListObject(string xSetName, string prefix)
        {
            return ListObjectNames(ListObjectWithPreFix(xSetName, prefix, true)[1]);
        }

        private List<string> ListObjectNames(string responseXmlString)
        {
            List<string> names = new List<string>();
            List<XPathNavigator> navs = FirstStepAnalyzeXML(responseXmlString, "container/object");
            XPathNavigator singleNav;
            string name;
            foreach (XPathNavigator nav in navs)
            {
                name = null;
                singleNav = nav.SelectSingleNode("name");
                if (singleNav != null)
                {
                    name = singleNav.Value;


                    if (name.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        name = name.Substring(0, name.LastIndexOf('/'));
                        names.Add(name);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            names.Add(name);
                        }
                    }
                }
            }

            return names;
        }

        public override long GetContainerSize(string xSetName)
        {
            long size = 0;
            try
            {
                string url = BuildURL(GetStorageUrl(), xSetName);
                RackspaceHttpRequest request = GetOperateRequest(url);
                request.Method = RESTCommands.GET;
                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp != null && (resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.OK))
                    {
                        Logger.Info("Container already exists, container: {0}", xSetName);
                        size = long.Parse(resp.Headers[RackspaceConstants.X_CONTAINER_BYTES_USED]);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(GetCheckCtnErrorMsg(xSetName) + "{0}", e);
                throw;
            }
            return size;
        }

        protected override List<string> ListXstream(string xSetName, string prefix, int limit, string marker, bool isGetName)
        {
            List<string> names = new List<string>();
            RackspaceHttpRequest request = null;
            try
            {
                string url = BuildURL(GetStorageUrl(), xSetName);
                Dictionary<string, string> paramaters = new Dictionary<string, string>();
                if (string.IsNullOrEmpty(prefix))
                {
                    paramaters.Add(RackspaceConstants.LIST_ROOT_NAME_QUERY, "/");
                }
                if (!string.IsNullOrEmpty(prefix))
                {
                    if (prefix.Equals("/", StringComparison.CurrentCultureIgnoreCase))
                    {
                        paramaters.Add(RackspaceConstants.LIST_ROOT_NAME_QUERY, "/");
                    }
                    else
                    {
                        if (prefix.Contains("\\"))
                        {
                            prefix = prefix.Replace("\\", "/");
                        }
                        paramaters.Add(RackspaceConstants.LIST_CONTAINER_NAME_QUERY, prefix);
                        paramaters.Add(RackspaceConstants.LIST_ROOT_NAME_QUERY, "/");
                        paramaters.Add("format", "xml");
                    }
                }
                if (!string.IsNullOrEmpty(marker))
                {
                    paramaters.Add(RackspaceConstants.LIST_CONTAINER_MARKER_QUERY, marker);
                }
                if (limit > 0)
                {
                    paramaters.Add(RackspaceConstants.LIST_CONTAINER_LIMIT_OBJ_COUNT_QUERY, limit.ToString());
                }
                string queryStr = ConvertQueryList2String(paramaters);
                request = GetOperateRequest(url + queryStr);
                request.Method = RESTCommands.GET;

                using (HttpWebResponse resp = DoExecute(request))
                {
                    using (Stream inputStream = resp.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(inputStream))
                        {
                            string line = null;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line.Equals(prefix, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    continue;
                                }
                                names.Add(line);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(GetListObjErrorMsg(xSetName) + "{0}", e);
                throw;
            }

            return names;
        }

        public override HttpWebRequest GetUploadRequest(string xSetName, string xStreamName, string mimeType, HttpWebRequest webRequest, int blockNumber, long dataLength)
        {
            if (webRequest != null)
            {
                return webRequest;
            }
            string url = BuildURL(GetStorageUrl(), xSetName, xStreamName);
            RackspaceHttpRequest request = GetOperateRequest(url);
            request.Method = RESTCommands.PUT;
            request.Headers.Add("Content-Type", mimeType);
            request.Headers.Add("Content-Length", dataLength.ToString());
            Logger.Info("get stream for file, xSet: {0}, xStream: {1}", xSetName, xStreamName);
            return HttpClient.GetWebRequestForUpLoad(request);
        }

        public override bool CreateObject(string xSetName, string xStreamName, HttpWebRequest request, long dataLength)
        {
            bool result = false;
            try
            {
                using (HttpWebResponse resp = UpLoad(request))
                {
                    if (resp != null && resp.StatusCode == HttpStatusCode.Created)
                    {
                        Logger.Info("create xStream succeed, xSet: {0}, xStream: {1}", xSetName, xStreamName);
                        result = true;
                    }
                    else
                    {
                        Logger.Error("Create object failed. object:{0} ,container:{1} , statues:{2}", xStreamName, xSetName, resp.StatusCode);
                        throw new Exception("Create object failed. object : " + xStreamName + ",container : " + xSetName);
                    }
                }
            }
            catch (WebException e)
            {
                if (IsProtocalError(e))
                {
                    HttpWebResponse resp = e.Response as HttpWebResponse;
                    LogServerMsg(resp, GetCreateObjErrorMsg(xStreamName, xSetName));
                }
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(GetCreateObjErrorMsg(xStreamName, xSetName) + "{0}", e);
                throw;
            }
            return result;
        }

        public override bool CheckObject(string fullURL, Dictionary<string, string> parameters, Dictionary<string, string> headers)
        {
            bool result = false;
            HttpWebRequest request = null;

            try
            {
                request = HttpClient.CreateRequestGet(fullURL, null);
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
                Trace.TraceError(ex.Message, ex);
                try
                {
                    String url = fullURL.TrimEnd('/');
                    request = HttpClient.CreateRequestGet(url, null);
                    using (HttpWebResponse resp = DoExecute(request, headers))
                    {
                        if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                        {
                            result = true;
                        }
                    }
                }
                catch (PathNotFoundException e)
                {
                    Trace.TraceError(e.Message, e);
                    result = false;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw;
            }

            return result;
        }

        public override bool CheckObject(string xSetName, string xStreamName)
        {
            bool result = false;
            RackspaceHttpRequest request;
            try
            {
                if (!xStreamName.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    String url = BuildURL(GetStorageUrl(), xSetName, xStreamName);
                    request = GetOperateRequest(url);
                    request.Method = RESTCommands.GET;
                }
                else
                {
                    //if (this.openParams.ModuleType == 1)
                    //{
                    //    return CheckFolderForConnector(xSetName, xStreamName);
                    //}
                    string urlWithoutQueryParms = BuildURLWithOutQueryParams(xSetName);
                    Dictionary<string, string> queryParams = new Dictionary<string, string>();

                    if (!string.IsNullOrEmpty(xStreamName)
                         && !"/".Equals(xStreamName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        queryParams.Add("delimiter", "/");
                        queryParams.Add("prefix", xStreamName);
                    }
                    string finalURL = HttpClient.CombiningQueryParams(urlWithoutQueryParms, queryParams);
                    request = GetOperateRequest(finalURL);
                    request.Method = RESTCommands.GET;
                }
                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
                    if (resp != null && resp.StatusCode == HttpStatusCode.NoContent && "/".Equals(xStreamName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        result = true;
                    }
                }
            }
            catch (PathNotFoundException ex)
            {
                Trace.TraceError(ex.Message, ex);
                result = false;
            }
            catch (Exception e)
            {
                Logger.Error(GetCheckObjErrorMsg(xStreamName, xSetName) + "{0}", e);
                throw;
            }

            return result;
        }

        public override bool DeleteObject(string xSetName, string xStreamName, bool isDeleteSubFile)
        {
            bool result = false;
            if (isDeleteSubFile)
            {
                List<string> subFiles = ListObject(xSetName, xStreamName);
                foreach (string name in subFiles)
                {
                    result = DeleteObject(xSetName, name);// Path.Combine(xStreamName, name));
                    if (!result)
                    {
                        return false;
                    }
                }
            }
            result = DeleteObject(xSetName, xStreamName);
            return result;
        }

        private bool CopyFile(string baseURL, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
        {
            bool result = false;
            HttpWebRequest requestCopy = HttpClient.CreateRequestCopy(baseURL, queryParams);
            using (HttpWebResponse resp = DoExecute(requestCopy, headers))
            {
                using (Stream inputStream = resp.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(inputStream))
                    {
                        if (resp.StatusCode == HttpStatusCode.Created)
                        {
                            result = true;
                        }
                        else
                        {
                            Logger.Error("CopyFile failed {0}", requestCopy.RequestUri);
                        }
                    }
                }
            }
            return result;
        }

        private bool DeleteObject(string xSetName, string xStreamName)
        {
            bool result = false;
            try
            {
                string url = BuildURL(GetStorageUrl(), xSetName, xStreamName);
                RackspaceHttpRequest request = GetOperateRequest(url);
                request.Method = RESTCommands.DELETE;

                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.OK)
                    {
                        Logger.Info("Delete object successfully: {0}", xStreamName);
                        result = true;
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                Logger.Warn("Cannot find object, may it was deleted successful: {0}", e.Message);
                result = true;
            }
            catch (Exception e)
            {
                Logger.Error(GetDeleteObjErrorMsg(xStreamName, xSetName) + "{0}", e);
                throw;
            }
            return result;
        }

        public override Stream OpenObject(string xSetName, string xStreamName, int rangFrom, int rangeTo)
        {
            Stream result = null;
            try
            {
                if (this.isCDNEabled && !this.isCDNFalied)
                {
                    try
                    {
                        string cdnURL = InitCDNURL(xSetName);
                        result = OpenObject(cdnURL, xSetName, xStreamName, rangFrom, rangeTo);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceWarning(e.Message);
                        Logger.Error("Open object with cdn failed, object:{0}, xSetName:{1}", xStreamName, xSetName);
                        this.isCDNFalied = true;
                        result = OpenObject(GetStorageUrl(), xSetName, xStreamName, rangFrom, rangeTo);
                    }
                }
                else
                {
                    result = OpenObject(GetStorageUrl(), xSetName, xStreamName, rangFrom, rangeTo);
                }
            }
            catch (Exception e)
            {
                Logger.Error(GetOpenObjErrorMsg(xStreamName, xSetName) + "{0}", e);
                throw;
            }
            return result;
        }

        public override CloudFileInfo GetObjectInfo(string xSetName, string xStreamName)
        {
            CloudFileInfo result = new CloudFileInfo();
            try
            {
                String url = BuildURL(GetStorageUrl(), xSetName, xStreamName);
                RackspaceHttpRequest request = GetOperateRequest(url);
                request.Method = RESTCommands.GET;

                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp != null && (resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.OK))
                    {
                        if (resp.ContentLength >= 0)
                        {
                            result.FileSize = resp.ContentLength;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Get object size failed, object:{0}, container:{1}, details:{2} ", xStreamName, xSetName, e);
                throw;
            }
            return result;
        }
        #endregion

        #region Build URL
        private string InitCDNURL(string xSetName)
        {
            string result = null;
            try
            {
                string url = BuildURL(GetCDNStorageUrl(), xSetName);
                RackspaceHttpRequest request = GetOperateRequest(url);
                request.Headers[RackspaceConstants.X_CDN_ENABLED] = "True";
                request.Method = RESTCommands.PUT;
                request.KeepAlive = false;
                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp != null && resp.StatusCode == HttpStatusCode.Accepted || resp.StatusCode == HttpStatusCode.Created)
                    {
                        result = resp.Headers[RackspaceConstants.X_CDN_URI];
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Init cdn failed, container:{0}, {1} ", xSetName, e);
                throw;
            }

            return result;
        }

        private string BuildURL(string baseURL, string xSetName)
        {
            return baseURL + "/" + Encode(xSetName);
        }

        public override string BuildURLWithOutQueryParams(string container)
        {
            return BuildURL(GetStorageUrl(), container);
        }

        public override string BuildObjectAbsoluteURL(string container, string objectName)
        {
            lowName = objectName;
            return BuildURL(GetStorageUrl(), container, objectName);
        }

        public override HttpDownloadStream OpenObjectForRead(string fullURL, Dictionary<string, string> headers)
        {
            if (this.isCDNEabled)
            {
                if (string.IsNullOrEmpty(cdnURL))
                {
                    if (string.IsNullOrEmpty(openParams.D5FolderName))
                    {
                        cdnURL = InitCDNURL(this.HttpClient.CurrentSystem.SystemLocation);
                    }
                    else
                    {
                        cdnURL = InitCDNURL(openParams.D5FolderName);
                    }
                }
                fullURL = BuildURL(cdnURL, string.Empty) + (lowName.Equals("/", StringComparison.OrdinalIgnoreCase) ? string.Empty : Encode(lowName));
            }
            HttpWebRequest request = HttpClient.CreateRequestGet(fullURL, null);
            return new HttpDownloadStream(DoExecute(request, headers)) { System = this.HttpClient.CurrentSystem };
        }

        private string BuildURL(string baseURL, string xSetName, string xStreamName)
        {
            if (xStreamName.Equals("/", StringComparison.OrdinalIgnoreCase))
            {
                xStreamName = string.Empty;
                return BuildURL(baseURL, xSetName) + "/";
            }
            else
            {
                return BuildURL(baseURL, xSetName) + "/" + Encode(xStreamName);
            }
        }

        #endregion

        #region Construct Request

        private RackspaceHttpRequest GetAuthRequest(string uri)
        {
            RackspaceHttpRequest request = new RackspaceHttpRequest(uri);
            Dictionary<string, string> hds = new Dictionary<string, string>();
            hds.Add(RackspaceConstants.X_STORAGE_USER, username);
            hds.Add(RackspaceConstants.X_STORAGE_PASS, password);
            request.Headers = hds;
            return request;
        }

        private RackspaceHttpRequest GetOperateRequest(string uri)
        {
            RackspaceHttpRequest request = new RackspaceHttpRequest(uri);
            Dictionary<string, string> hds = new Dictionary<string, string>();
            hds.Add(RackspaceConstants.X_AUTH_TOKEN, this.authToken);
            request.Headers = hds;
            return request;
        }

        #endregion

        public override Stream OpenObject(string container, string objectName, int[] lengths, FileMode mode)
        {
            string url = BuildURL(GetStorageUrl(), container, objectName);
            RackspaceHttpRequest request = GetOperateRequest(url);
            request.Headers.Add("Content-Type", RackspaceConstants.STREAM_CONTENT_TYPE);
            switch (mode)
            {
                case FileMode.Open:
                    request.Method = RESTCommands.GET;
                    if (lengths != null && lengths.Length == 3)
                    {
                        int rangFrom = lengths[1];
                        int rangeTo = lengths[2];
                        if (rangFrom >= 0 && rangeTo >= 0 && rangFrom < rangeTo)
                        {
                            string range = "bytes=" + rangFrom + "-" + rangeTo;
                            request.Headers.Add("Range", range);
                        }
                    }
                    HttpWebResponse response = DoExecute(request);
                    return new HttpDownloadStream(response);
                case FileMode.Create:
                case FileMode.CreateNew:
                    request.Method = RESTCommands.PUT;
                    request.Headers.Add("x-ms-blob-type", "BlockBlob");
                    request.Headers.Add("Content-Length", lengths[0] + "");
                    return HttpClient.GetWebRequestForUpLoad(request).GetRequestStream();
                default:
                    break;
            }

            return null;
        }

        public Stream OpenObject(string baseURL, string xSetName, string xstream, int rangeFrom, int rangeTo)
        {
            try
            {
                string url = BuildURL(baseURL, xSetName, xstream);
                RackspaceHttpRequest request = GetOperateRequest(url);
                request.Method = RESTCommands.GET;
                request.Headers.Add("Content-Type", RackspaceConstants.STREAM_CONTENT_TYPE);
                if (rangeFrom >= 0 && rangeTo >= 0 && rangeFrom < rangeTo)
                {
                    string range = "bytes=" + rangeFrom + "-" + rangeTo;
                    request.Headers.Add("Range", range);
                }

                HttpWebResponse response = DoExecute(request);
                return new HttpDownloadStream(response);
            }
            catch (Exception t)
            {
                Logger.Error(t.ToString());
                throw;
            }
        }

        public static bool Validate(string username, string apiAccessKey)
        {
            bool result = false;
            HttpWebResponse response = null;
            RackspaceHttpRequest request = null;
            try
            {
                request = new RackspaceHttpRequest(StorageUrl.Rackspace);
                request.Headers.Add(RackspaceConstants.X_STORAGE_USER, username);
                request.Headers.Add(RackspaceConstants.X_STORAGE_PASS, apiAccessKey);
                request.Method = RESTCommands.GET;
                RackspaceHttpClient client = new RackspaceHttpClient();
                response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    result = true;
                }
            }
            catch (Exception t)
            {
                Trace.TraceWarning(t.Message);
                throw;
            }
            return result;
        }

        private string ConvertContainerName(string container)
        {
            return container.Replace('/', ':').Replace('\\', ':');
        }

        protected override bool SpecialRetryCondition(BasicRequest request, HttpWebResponse resp)
        {
            bool result = false;

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                //如果是x_auth_token超时，则重新login并重试；否则返回false。
                if (request.Headers.ContainsKey(RackspaceConstants.X_AUTH_TOKEN))
                {
                    Login(openParams.SystemLocation);
                    request.Headers[RackspaceConstants.X_AUTH_TOKEN] = authToken;
                    result = true;
                }
            }

            return result;
        }

        #region from IHttpRequestPrepare
        public override Dictionary<string, string> ListDirectoryQueryParams
        {
            get { return new Dictionary<string, string>() { { "delimiter", "/" }}; }
        }

        public override Dictionary<string, string> ListObjectQueryParams
        {
            get { return new Dictionary<string, string>(); }
        }

        public override Dictionary<string, string> CopyFileQueryParams
        {
            get { return new Dictionary<string, string>(); }
        }

        public override Dictionary<string, string> CopyFileHeaders
        {
            get { return new Dictionary<string, string>() { { RackspaceConstants.X_AUTH_TOKEN, authToken }}; }
        }

        public override Dictionary<string, string> ListDirectoryHeaders
        {
            get { return new Dictionary<string, string>() { { RackspaceConstants.X_AUTH_TOKEN, authToken }}; }
        }

        public override Dictionary<string, string> ListObjectHeaders
        {
            get { return new Dictionary<string, string>() { { RackspaceConstants.X_AUTH_TOKEN, authToken }}; }
        }

        public override Dictionary<string, string> OpenDirectoryWriteModeHeaders
        {
            get { return new Dictionary<string, string>() { { RackspaceConstants.X_AUTH_TOKEN, authToken }}; }
        }

        public override Dictionary<string, string> OpenDirectoryReadModeHeaders
        {
            get { return new Dictionary<string, string>() { { RackspaceConstants.X_AUTH_TOKEN, authToken }}; }
        }

        public override Dictionary<string, string> OpenFileWriteModeHeaders
        {
            get { return new Dictionary<string, string>() { { RackspaceConstants.X_AUTH_TOKEN, authToken }}; }
        }

        public override Dictionary<string, string> OpenFileReadModeHeaders
        {
            get { return new Dictionary<string, string>() { { RackspaceConstants.X_AUTH_TOKEN, authToken }}; }
        }

        public override Dictionary<string, string> OpenStreamWriteModeHeaders
        {
            get { return new Dictionary<string, string>() { { RackspaceConstants.X_AUTH_TOKEN, authToken }}; }
        }

        public override Dictionary<string, string> OpenStreamReadModeHeaders
        {
            get { return new Dictionary<string, string>() { { RackspaceConstants.X_AUTH_TOKEN, authToken }}; }
        }

        public override Dictionary<string, string> Headers
        {
            get { return new Dictionary<string, string>() { { RackspaceConstants.X_AUTH_TOKEN, authToken }}; }
        }

        #endregion

        #region from IHttpResponseHandler Interface
        public override List<XDirectoryInfo> Parse2Directory(string responseXmlString, string path)
        {
            List<string> results = AnalyzeXML(responseXmlString, "container/" + "SUBDIR".ToLower(CultureInfo.InvariantCulture) + "/name");
            List<XDirectoryInfo> dirs = new List<XDirectoryInfo>();
            CloudDirectoryInfo dir = null;
            foreach (string rs in results)
            {
                dir = new CloudDirectoryInfo(rs.TrimEnd('/'));
                dir.IsExists = true;
                dirs.Add(dir);
            }
            return dirs;
        }

        public override List<XFileInfo> Parse2File(string responseXmlString)
        {
            throw new NotSupportedException();
        }
        #endregion

        public override string BuildObjectAbsoluteURL(string url, string container, string objectName)
        {
            throw new NotSupportedException();
        }

        public override string ListAzureMetaName(string baseURL, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
        {
            throw new NotSupportedException();
        }

        public override CloudOpenParameter ConveryParams(Dictionary<string, string> prams)
        {
            throw new NotSupportedException();
        }

        public override SpaceInfo GetUserAccountInfo()
        {
            throw new NotSupportedException();
        }

        public override string GetFinalUrl(StorageInfo info)
        {
            throw new NotSupportedException();
        }
    }
}
