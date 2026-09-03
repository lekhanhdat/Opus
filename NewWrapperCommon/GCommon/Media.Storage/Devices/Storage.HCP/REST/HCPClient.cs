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

namespace AvePoint.Media.Storage.HCP
{
    #region using directives
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Resources.HCPI18N;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    #endregion

    #region CodeReview

    [AveCodeReview(
    "2012/3/22",
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

    #endregion CodeReview

    class HCPClient : AbstractRESTOprationExecutor, ICloudOprationExecutor, IHttpRequestPrepare, IHttpResponseHandler
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(HCPClient));

        private bool isUseDefaultPath = true;

        public bool IsUseDefaultPath
        {
            get { return isUseDefaultPath; }
            set { isUseDefaultPath = value; }
        }

        public HCPOpenParameter OpenParam { get; set; }

        public bool IsSecondHost
        {
            get { return !string.IsNullOrEmpty(OpenParam.SecondaryHost); }
        }

        private static SafeDictionary<string, HCPOpenParameter> cacheOpenParams = new SafeDictionary<string, HCPOpenParameter>();

        public HCPClient()
        {
            Logger = StorageLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            HttpClient = new HCPHttpClient();
        }

        public SpaceInfo CheckFreeSpace()
        {
            return (SpaceInfo)Invoke("CheckFreeSpaceMethod", null);
        }

        private SpaceInfo CheckFreeSpaceMethod()
        {
            string baseURL = OpenParam.PrimaryHost + "/proc/statistics";
            SpaceInfo spaceInfo = new SpaceInfo();
            ResponseInfo responseInfo = ListObjects(baseURL, null, Headers);
            string xml = responseInfo.ResponseXml;
            Regex regex = new Regex("totalCapacityBytes=\"([0-9]+)\"[\\s\\S]*usedCapacityBytes=\"([0-9]+)\"");
            Match match = regex.Match(xml);
            if (!match.Success)
            {
                throw new Exception("CheckFreeSpace ResponseXml format error, ResponseXml = " + xml);
            }
            spaceInfo.TotalSpace = ulong.Parse(match.Groups[1].Value);
            spaceInfo.TotalUsedSpace = ulong.Parse(match.Groups[2].Value);
            spaceInfo.TotalFreeSpace = spaceInfo.TotalSpace - spaceInfo.TotalUsedSpace;
            return spaceInfo;
        }

        public override void InitRetry(CloudOpenParameter openParams)
        {
            int retryCount = openParams.MaxRetryCount;
            int retryInterval = openParams.RetryInterval;
            bool needRetry = true;
            Logger.Info("Max Retry Count : {0}, Retry Interval : {1}", retryCount, retryInterval);
            Logger.Info("OpenParameter.FailOverMode: {0}.", ((HCPOpenParameter)openParams).FailOverMode);
            Logger.Info("OpenParameter.FlushDNS: {0} ", openParams.FlushDNS);
            RetryRequset = new Retry(retryCount, retryInterval, needRetry, openParams.FlushDNS);
        }

        public bool IsDefaultNamespace
        {
            get { return Regex.Match(OpenParam.PrimaryHost, "^http[s]{0,1}\\:\\/\\/default\\.default\\..*").Success; }
        }

        public override void InitConfig(CloudOpenParameter prams)
        {
            HttpClient = new HCPHttpClient();
            OpenParam = prams as HCPOpenParameter;
            HttpClient.OpenParam = OpenParam;
            this.CloudOpenParam = prams;
            base.InitProxySetting();
        }

        public override bool Login(string xSetName)
        {
            return true;
        }

        public string Cookie
        {
            get { return HCPUtility.GenerateCookie(OpenParam); }
        }

        private Dictionary<string, string> hcpHeader = new Dictionary<string, string>();

        public Dictionary<string, string> HcpHeader
        {
            get { return hcpHeader; }
            set { hcpHeader = value; }
        }

        public override Dictionary<string, string> Headers
        {
            get
            {
                return new Dictionary<string, string>() {
                    {"Cookie", HCPUtility.GenerateCookie(OpenParam)}
                };
            }
        }

        public override ResponseInfo ListObjects(string baseURL, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
        {
            ResponseInfo responseInfo = new ResponseInfo();
            HttpWebRequest requestGet = HttpClient.CreateRequestGet(baseURL, queryParams);
            using (HttpWebResponse resp = DoExecute(requestGet, headers))
            {
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream inputStream = resp.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(inputStream))
                        {
                            responseInfo.ResponseXml = reader.ReadToEnd(); //不能DeCode
                            return responseInfo;
                        }
                    }
                }
                else
                {
                    throw new Exception("ListObjects error, base URL" + baseURL + ", message:" + resp.StatusCode);
                }
            }
        }

        private bool ValidatePremissions(string host)
        {
            string url = host + "/proc/permissions";
            Dictionary<string, string> headers = Headers;
            ResponseInfo responseInfo = (ResponseInfo)Invoke("ListObjects", new object[] { url, new Dictionary<string, string>(), headers });
            string responseXmlString = responseInfo.ResponseXml;
            if (responseXmlString.Contains("permissions"))
            {
                return true;
            }
            return false;
        }

        public override StorageOpenValidResult GetPermissions()
        {
            StorageOpenValidResult result = null;

            string tempPrimaryHost = OpenParam.PrimaryHost;
            string tempSecondaryHost = OpenParam.SecondaryHost;
            if (OpenParam.CacheSecondary && OpenParam.SecondaryTimeout != 0)
            {
                string avilableHost = GetAvailableHost(OpenParam.PhysicalIdAndMidifyTime);
                if (!string.IsNullOrEmpty(avilableHost) &&
                    !string.IsNullOrEmpty(tempSecondaryHost) &&
                    cacheOpenParams[OpenParam.PhysicalIdAndMidifyTime] != null)
                {
                    tempPrimaryHost = OpenParam.SecondaryHost;
                    tempSecondaryHost = OpenParam.PrimaryHost;
                }
            }

            try
            {
                result = (StorageOpenValidResult)HasPermissions(tempPrimaryHost);
            }
            catch (Exception ex)
            {
                Logger.Error("HasPermissions(OpenParam.PrimaryHost) error: {0}.", ex);
                if (!string.IsNullOrEmpty(tempSecondaryHost) && OpenParam.FailOverMode != FailoverMode.Off)
                {
                    Logger.Info("Primary host is not available: {0}", tempPrimaryHost);
                    Logger.Info("Begin try second host: {0}", tempSecondaryHost);
                    result = (StorageOpenValidResult)HasPermissions(tempSecondaryHost);
                }
                else
                {
                    throw;
                }
            }
            return result;
        }

        public StorageOpenValidResult HasPermissions(string host)
        {
            StorageOpenValidResult result = new StorageOpenValidResult();
            try
            {
                if (Regex.Match(host, "^http[s]{0,1}\\:\\/\\/default\\.default\\..*").Success)
                {
                    string guid = Guid.NewGuid().ToString();
                    if (!(bool)Invoke("CreateContainer", new object[] { guid }))
                    {
                        throw new AuthenticationFailedException(HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Cannot_connect_to_the_remote_server", AbstractXSystem.Culture));
                    }
                    if (!(bool)Invoke("DeleteContainer", new object[] { guid }))
                    {
                        throw new AuthenticationFailedException(HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Cannot_connect_to_the_remote_server", AbstractXSystem.Culture));
                    }
                    result.IsHasPermission = true;
                    result.IsWriteAble = true;
                    result.IsReadAble = true;
                    result.IsDeleteAble = true;
                    return result;
                }
                else
                {
                    string url = host + "/proc/permissions";
                    Dictionary<string, string> headers = Headers;
                    ResponseInfo responseInfo = ListObjects(url, new Dictionary<string, string>(), headers);
                    UserPermissions userPermissions = GetUserPermissions(responseInfo.ResponseXml);
                    result.IsHasPermission = true;
                    result.IsReadAble = userPermissions.Read;
                    result.IsWriteAble = userPermissions.Write;
                    result.IsDeleteAble = userPermissions.Delete;

                    string checkSpaceUrl = host + "/proc/statistics";
                    ResponseInfo checkSpaceUrlResponseInfo = ListObjects(checkSpaceUrl, new Dictionary<string, string>(), headers);
                    GetSpaceInfo(checkSpaceUrlResponseInfo.ResponseXml, result);
                }
                if (!result.IsReadAble || !result.IsWriteAble || !result.IsDeleteAble)
                {
                    throw new AuthenticationFailedException(
                        string.Format("{0} doesn't have all permission, read={1} write={2} delete={3}",
                            host, result.IsReadAble, result.IsWriteAble, result.IsDeleteAble));
                }
            }
            catch (AuthenticationFailedException ex)
            {
                logger.Error("HasPermissions(string host) error: {0}", ex);
                throw new AuthenticationFailedException(HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Authentication_failed", AbstractXSystem.Culture));
            }
            return result;
        }

        private void GetSpaceInfo(string xml, StorageOpenValidResult result)
        {
            Regex regex = new Regex("[\\s\\S]+totalCapacityBytes=\"([^\"]+)\"[\\s\\S]+usedCapacityBytes=\"([^\"]+)\"[\\s\\S]");
            Match m = regex.Match(xml);
            if (!m.Success)
            {
                logger.Warn("Cannot get space information from HCP server.");
                result.TotalSpace = long.MaxValue;
                result.TotalUsedSpace = 0;
                result.TotalFreeSpace = long.MaxValue;
            }
            result.TotalSpace = ulong.Parse(m.Groups[1].Value);
            result.TotalUsedSpace = ulong.Parse(m.Groups[2].Value);
            result.TotalFreeSpace = result.TotalSpace - result.TotalUsedSpace;
        }

        private UserPermissions GetUserPermissions(string xml)
        {
            Regex regex = new Regex("userPermissions[\\s\\S]+read=\"([^\"]+)\"[\\s\\S]+write=\"([^\"]+)\"[\\s\\S]+delete=\"([^\"]+)\"");
            Match m = regex.Match(xml);
            if (!m.Success)
            {
                throw new AuthenticationFailedException(HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Authentication_failed", AbstractXSystem.Culture));
            }
            return new UserPermissions() { Read = bool.Parse(m.Groups[1].Value), Write = bool.Parse(m.Groups[2].Value), Delete = bool.Parse(m.Groups[3].Value) };
        }

        public override Dictionary<string, string> OpenDirectoryWriteModeHeaders
        {
            get
            {
                return Headers;
            }
        }

        public override bool CheckContainer(string xSetName)
        {
            string fullURL = BuildURLWithOutQueryParams(xSetName);
            return CheckObject(fullURL, null, Headers);
        }

        public bool CheckContainer(string xSetName, bool isCheckSecondary)
        {
            string fullURL = BuildURLWithOutQueryParams(xSetName);
            return CheckObject(fullURL, null, Headers, isCheckSecondary);
        }

        private string GetDataAccessPoint()
        {
            if (IsUseDefaultPath)
            {
                if (IsDefaultNamespace)
                {
                    return OpenParam.PrimaryHost + "/FCFS_DATA/".ToLower(CultureInfo.InvariantCulture);
                }
                return OpenParam.PrimaryHost + "/rest/Data/";
            }
            else
            {
                return OpenParam.PrimaryHost + "/";
            }
        }

        public Dictionary<string, string> GetObjectInfo(string highName, string lowName, Dictionary<string, string> requestParams, Dictionary<string, string> requestHeaders)
        {
            string url = BuildObjectAbsoluteURL(highName, lowName);
            return GetObjectInfo(url, requestParams, requestHeaders);
        }

        public override Dictionary<string, string> GetObjectInfo(string url, Dictionary<string, string> requestParams, Dictionary<string, string> requestHeaders)
        {
            HttpWebRequest request = HttpClient.CreateRequestHead(url, requestParams);
            using (HttpWebResponse resp = DoExecute(request, requestHeaders))
            {
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    Dictionary<string, string> result = new Dictionary<string, string>();
                    foreach (string key in resp.Headers.Keys)
                    {
                        result[key] = resp.Headers[key];
                    }
                    if (result.ContainsKey("Content-Length"))
                    {
                        return result;
                    }
                }
                throw new Exception("get object info failed:" + resp.StatusCode + "URL:" + url);
            }
        }

        public override bool CreateContainer(string xSetName)
        {
            string url = BuildURLWithOutQueryParams(xSetName);
            if (CheckContainer(xSetName, false))
            {
                //Logger.Debug("When creating folders, folder exist.");
                return true;
            }
            //else
            //{
            //    Logger.Debug("When creating folders, folder not exist.");
            //}
            if (IsDefaultNamespace)
            {
                Regex r = new Regex("^https{0,1}://.*");
                if (r.IsMatch(xSetName))
                {
                    url = xSetName;
                }
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "MKDIR";
                HttpWebResponse resp = request.GetResponse() as HttpWebResponse;
                if (resp.StatusCode == HttpStatusCode.Created)
                {
                    return true;
                }
                else
                {
                    throw new Exception("Create container failed:" + resp.StatusCode + ",container name:" + xSetName);
                }
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters[HCPConsts.KEY_URL_Type] = HCPConsts.KEY_VAL_Directory;
            HttpWebRequest requestGet = HttpClient.CreateRequestPut(url, parameters);
            Dictionary<string, string> headers = Headers;

            using (HttpWebResponse resp = DoExecute(requestGet, headers))
            {
                if (resp.StatusCode == HttpStatusCode.Created || resp.StatusCode == HttpStatusCode.Conflict)
                {
                    return true;
                }
                else
                {
                    throw new Exception("Create container failed:" + resp.StatusCode + ",container name:" + xSetName);
                }
            }
        }

        public override bool DeleteContainer(string xSetName)
        {
            string url = BuildURLWithOutQueryParams(xSetName);
            HttpWebRequest requestDelete = HttpClient.CreateRequestDelete(url, null);
            Dictionary<string, string> headers = Headers;
            try
            {
                using (HttpWebResponse resp = DoExecute(requestDelete, headers))
                {
                    if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception("delete container failed:" + resp.StatusCode + ",container name:" + xSetName);
                    }
                }
            }
            catch (PathNotFoundException)
            {
                Logger.Warn("The container already no exist, name: {0}", xSetName);
                return true;
            }
        }

        public override string BuildURLWithOutQueryParams(string container)
        {
            return GetDataAccessPoint() + Encode(container.Replace("\\", "/").TrimEnd('/'));
        }

        public override string BuildObjectAbsoluteURL(string container, string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return GetDataAccessPoint() + Encode(container);
            }
            string url = GetDataAccessPoint() + Encode((PathUtil.CombinePath(container, objectName.TrimEnd('/'))).Replace("\\", "/"));
            return url;
        }

        public override Dictionary<string, string> OpenStreamWriteModeHeaders
        {
            get
            {
                return base.OpenStreamWriteModeHeaders;
            }
        }

        public override bool CheckObject(string xSetName, string xStreamName)
        {
            string fullURL = BuildObjectAbsoluteURL(xSetName, xStreamName);
            //Logger.Debug("check object:" + fullURL);
            return CheckObject(fullURL, null, Headers);
        }

        private bool CheckObject(string xSetName, string xStreamName, bool isCheckSecondary)
        {
            string fullURL = BuildObjectAbsoluteURL(xSetName, xStreamName);
            //Logger.Debug("check object:" + fullURL);
            return CheckObject(fullURL, null, Headers, isCheckSecondary);
        }

        private bool CheckObject(string fullURL, Dictionary<string, string> parameters, Dictionary<string, string> headers, bool isCheckSecondary)
        {
            bool result = false;
            try
            {
                HttpWebRequest request = HttpClient.CreateRequestHead(fullURL, null);
                using (HttpWebResponse resp = DoExecute(request, Headers))
                {
                    if (resp != null && (resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.OK))
                    {
                        if (resp.ResponseUri.Equals(fullURL) && !string.IsNullOrEmpty(resp.Headers.Get("P" + "Rag".ToLower(CultureInfo.InvariantCulture) + "Ma".ToLower(CultureInfo.InvariantCulture))))
                        {
                            result = true;
                        }
                        else
                        {
                            throw new Exception("CheckObject error" + resp.StatusCode + ",object path:" + fullURL);
                        }
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                Trace.TraceWarning(e.ToString());
                if (OpenParam.IsHaveSecondaryHost && isCheckSecondary)
                {
                    throw;
                }
                result = false;
            }
            return result;
        }

        public override bool CheckObject(string fullURL, Dictionary<string, string> parameters, Dictionary<string, string> headers)
        {
            return CheckObject(fullURL, parameters, headers, false);
        }

        public HttpUploadStream OpenObjectForWrite(string highName, string lowName, Dictionary<string, string> headers)
        {
            string fullURL = BuildObjectAbsoluteURL(highName, lowName);
            HttpWebRequest request = HttpClient.CreateRequestPut(fullURL, null);
            HttpClient.CombiningRequestWithHeaders(request, headers);
            OpenParam.WriteHeaders = headers;
            try
            {
                if (!string.IsNullOrEmpty(OpenParam.SecondaryHost) && OpenParam.FailOverMode == FailoverMode.ReadWrite)
                {
                    Logger.Info("Second is not empty, we will try to use it");
                    string temp = OpenParam.PrimaryHost;
                    OpenParam.PrimaryHost = OpenParam.SecondaryHost;
                    fullURL = BuildObjectAbsoluteURL(highName, lowName);
                    OpenParam.SecondaryHost = OpenParam.PrimaryHost;
                    OpenParam.PrimaryHost = temp;
                    HttpWebRequest secRequest = HttpClient.CreateRequestPut(fullURL, null);
                    HttpClient.CombiningRequestWithHeaders(secRequest, headers);
                    return new HCPHttpUploadStream(request, secRequest, OpenParam);
                }
                else
                {
                    return new HCPHttpUploadStream(request, OpenParam);
                }
            }
            catch (Exception e)
            {
                logger.Debug("Use second host failed.Message = {0}", e);
                return new HCPHttpUploadStream(request, OpenParam);
            }
        }

        public virtual HttpDownloadStream OpenObjectForRead(string highName, string lowName, Dictionary<string, string> headers)
        {
            HttpDownloadStream result = null;

            if (CheckObject(highName, lowName))
            {
                string fullURL = BuildObjectAbsoluteURL(highName, lowName);
                HttpWebRequest request = HttpClient.CreateRequestGet(fullURL, null);
                result = new HCPDownloadStream(DoExecute(request, headers), OpenParam);
            }
            else
            {
                throw new Exception("OpenObjectForRead.CheckObject is false.");
            }
            return result;
        }

        public bool ModifySystemMetadata(string highName, string lowName, string postData, Dictionary<string, string> headers)
        {
            string fullURL = BuildObjectAbsoluteURL(highName, lowName);
            HttpWebResponse resp = null;
            HttpWebRequest request = null;
            byte[] byteArray = null;
            if (IsDefaultNamespace)
            {
                Regex regex = new Regex("retention=([a-zA-Z0-9+-]+)");
                Match match = regex.Match(postData);
                string key = null;
                string value = null;
                if (match.Success)
                {
                    key = match.Groups[1].Value;
                    value = match.Groups[2].Value;
                    fullURL = fullURL.Replace(OpenParam.PrimaryHost + "/FCFS_DATA/".ToLower(CultureInfo.InvariantCulture), OpenParam.PrimaryHost + "/FCFS_METADATA/".ToLower(CultureInfo.InvariantCulture));
                    fullURL += "/retention.txt";
                }
                else
                {
                    throw new Exception("modify system metadata param error.");
                }

                byteArray = Encoding.UTF8.GetBytes(value);
                request = WebRequest.Create(fullURL) as HttpWebRequest;
                request.Method = "PUT";
                headers.Clear();
                headers.Add("Content-Length", byteArray.Length.ToString());
                HttpClient.AddHeaders(request, headers);
            }
            else
            {
                byteArray = Encoding.UTF8.GetBytes(postData);
                request = WebRequest.Create(fullURL) as HttpWebRequest;
                request.Method = "POST";
                if (!headers.ContainsKey("Content-Type"))
                {
                    headers.Add("Content-Type", "APPLICATION/X-WWW-FORM-URLENCODED".ToLower(CultureInfo.InvariantCulture));
                }
                if (!headers.ContainsKey("Content-Length"))
                {
                    headers.Add("Content-Length", byteArray.Length.ToString());
                }
                HttpClient.AddHeaders(request, headers);
            }

            Stream stream = request.GetRequestStream();
            stream.Write(byteArray, 0, byteArray.Length);
            stream.Close();
            using (resp = request.GetResponse() as HttpWebResponse)
            {
                //Logger.Debug("HttpStatusCode=" + Convert.ToInt32(resp.StatusCode) + " "
                //                     + resp.StatusCode.ToString() + "; RequestUri=" + request.RequestUri);
                if (resp.StatusCode == HttpStatusCode.Created || resp.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    throw new Exception("Modify system metadata retention fail.");
                }
            }
        }

        public bool AddCustomMetadata(string highName, string lowName, string metadataXml)
        {
            string fullURL = BuildObjectAbsoluteURL(highName, lowName) + "?type=custom-metadata";
            byte[] data = Encoding.UTF8.GetBytes(metadataXml);
            HttpWebRequest request = WebRequest.Create(fullURL) as HttpWebRequest;
            request.Method = "PUT";
            request.Headers.Add("Cookie", Cookie);
            request.ContentLength = data.Length;
            //Logger.Debug("AddCustomMetadata, request.RequestUri=" + request.RequestUri);
            using (Stream upStream = request.GetRequestStream())
            {
                upStream.Write(data, 0, data.Length);
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        throw new RetryableException("AddCustomMetadata error:" + response.StatusCode);
                    }
                }
            }
        }

        public bool AddDefaultCustomMetadata(string highName, string lowName, string metadataXml)
        {
            string fullURL = BuildObjectAbsoluteURL(highName, lowName) + "/custom-metadata.xml";
            fullURL = fullURL.Replace(OpenParam.PrimaryHost + "/FCFS_DATA/".ToLower(CultureInfo.InvariantCulture), OpenParam.PrimaryHost + "/FCFS_METADATA/".ToLower(CultureInfo.InvariantCulture));
            byte[] byteArray = Encoding.UTF8.GetBytes(metadataXml);
            HttpWebRequest request = WebRequest.Create(fullURL) as HttpWebRequest;
            request.Method = "PUT";
            request.ContentLength = metadataXml.Length;
            //Logger.Debug("AddDefaultCustomMetadata, request.RequestUri=" + request.RequestUri);
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(byteArray, 0, byteArray.Length);
                HttpWebResponse resp = request.GetResponse() as HttpWebResponse;
                if (resp.StatusCode == HttpStatusCode.Created || resp.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    throw new RetryableException("Add Default Namespace CustomMetadata error:" + resp.StatusCode + ",URL:" + fullURL);
                }
            }
        }

        public bool DeleteObject(string highName, string lowName, Dictionary<string, string> parameters, Dictionary<string, string> headers)
        {
            try
            {
                string fullURL = BuildObjectAbsoluteURL(highName, lowName);
                Logger.Debug("Delete file: {0}", fullURL);
                HttpWebRequest request = HttpClient.CreateRequestDelete(fullURL, null);
                using (HttpWebResponse resp = DoExecute(request, headers))
                {
                    if (resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception("Delete Object error:" + resp.StatusCode + ",object path:" + fullURL);
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                Logger.Warn("Cannot find object, may it was deleted successful : {0}", e.Message);
                return true;
            }
        }

        public bool DeleteObject(string highName, string lowName, Dictionary<string, string> parameters, Dictionary<string, string> headers, bool isPrivileged)
        {
            try
            {
                string fullURL = BuildObjectAbsoluteURL(highName, lowName) + (IsDefaultNamespace ? "" : "?privileged=true&reason=reason");
                Logger.Debug("Delete file: {0}", fullURL);
                HttpWebRequest request = HttpClient.CreateRequestDelete(fullURL, null);
                using (HttpWebResponse resp = DoExecute(request, headers))
                {
                    if (resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception("Delete Object error:" + resp.StatusCode + ",object path:" + fullURL);
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                Logger.Warn("Cannot find object, may it was deleted successful : {0}", e.Message);
                return true;
            }
        }

        public object HandleFailOverMode(MethodInfo methodInfo, object obj, object[] args, Exception ex)
        {
            object result = null;
            if (OpenParam.FailOverMode == FailoverMode.Off)
            {
                throw new FailoverModeException(string.Format("HandleFailOverMode() InvokeMethodName {0} failed, because failover mode is off.", methodInfo.Name));
            }
            if (OpenParam.IsHaveSecondaryHost && !OpenParam.IsUsedSecondaryHost
                && string.IsNullOrEmpty(GetAvailableHost(OpenParam.PhysicalIdAndMidifyTime)))
            {
                if (OpenParam.FlushDNS)
                {
                    Logger.Debug("Begin flushing DNS");
                    DnsUtil.FlushMyCache();
                    Logger.Debug("Finished flushing DNS");
                }

                Logger.Debug("Begin sleeping for delay retry");
                Thread.Sleep(OpenParam.RetryInterval);
                Logger.Debug("Finished sleeping for delay retry");

                Logger.Debug("PrimaryHost = SecondHost");
                OpenParam.FailedPrimaryHost = OpenParam.PrimaryHost;
                OpenParam.PrimaryHost = OpenParam.SecondaryHost;
                OpenParam.SecondaryHost = string.Empty;
                OpenParam.IsUsedSecondaryHost = true;

                Logger.Info("Begin retrying with second host");
                if (methodInfo != null)
                {
                    if (OpenParam.FailOverMode == FailoverMode.ReadWrite)
                    {
                        result = RetryRequset.ExcuteRetry(methodInfo, this, args);
                    }
                    else if (OpenParam.FailOverMode == FailoverMode.Read
                            && (methodInfo.Name.Equals("OpenObjectForRead")
                                || methodInfo.Name.Equals("CheckContainer")
                                || methodInfo.Name.Equals("CheckObject")
                                || methodInfo.Name.Equals("DeleteObject")
                                || methodInfo.Name.Equals("DeleteContainer")
                                || methodInfo.Name.Equals("GetObjectInfo")))
                    {
                        result = RetryRequset.ExcuteRetry(methodInfo, this, args);
                    }
                    else
                    {
                        Logger.Info("Unknown failover mode");
                        throw new Exception("Unknown failover mode");
                    }
                }
                else
                {
                    if (OpenParam.FailOverMode != FailoverMode.ReadWrite)
                    {
                        throw ex;
                    }
                }

                if (OpenParam.CacheSecondary && OpenParam.SecondaryTimeout != 0)
                {
                    SetAvailableHost(OpenParam.PhysicalIdAndMidifyTime, OpenParam);
                }
            }
            else
            {
                if (cacheOpenParams.ContainsKey(OpenParam.PhysicalIdAndMidifyTime))
                {
                    HCPOpenParameter param = cacheOpenParams[OpenParam.PhysicalIdAndMidifyTime];
                    if (param != null && !string.IsNullOrEmpty(param.FailedPrimaryHost))
                    {
                        try
                        {
                            Logger.Debug("When secondary namespace failed, retry FailedPrimaryHost '{0}'", param.FailedPrimaryHost);
                            HasPermissions(param.FailedPrimaryHost);
                            this.OpenParam.SecondaryHost = param.PrimaryHost;
                            this.OpenParam.PrimaryHost = param.FailedPrimaryHost;
                            this.OpenParam.FailedPrimaryHost = null;
                            this.OpenParam.IsUsedSecondaryHost = false;
                            cacheOpenParams[OpenParam.PhysicalIdAndMidifyTime] = null;
                            if (methodInfo != null)
                            {
                                result = RetryRequset.ExcuteRetry(methodInfo, this, args);
                            }
                        }
                        catch (Exception exc)
                        {
                            Logger.Error("HandleFailOverMode, HasPermissions(param.FailedPrimaryHost) Error: {0}", exc);
                            throw;
                        }
                    }
                }
                else
                {
                    throw ex;
                }
            }
            return result;
        }

        //public override List<string> ListObject(string xSetName, string prefix)
        //{
        //    List<string> result = new List<string>();
        //    if (prefix.Contains("\\"))
        //    {
        //        prefix = prefix.Replace("\\", "/");
        //    }
        //    string xml = ListObjects(xSetName + "/" + prefix, null, Headers).ResponseXml;
        //    Dictionary<string, List<string>> dictionarys = new Dictionary<string, List<string>>();
        //    List<XPathNavigator> navs = FirstStepAnalyzeXML(xml, "directory/entry", "http://www.w3.org/2001/XMLSchema-instance");

        //    string xmlfileDtoName = "urlName";
        //    string xmlfileDtoType = "type";
        //    string xmlfileDtoTypeValue = "object";
        //    if (IsDefaultNamespace)
        //    {
        //        xmlfileDtoName = "name";
        //        xmlfileDtoType = "fileType";
        //        xmlfileDtoTypeValue = "file";
        //    }

        //    string type;
        //    string name;
        //    foreach (XPathNavigator nav in navs)
        //    {
        //        name = null;
        //        name = nav.GetAttribute(xmlfileDtoName, "");
        //        type = nav.GetAttribute(xmlfileDtoType, "");
        //        if (xmlfileDtoTypeValue.Equals(type, StringComparison.CurrentCultureIgnoreCase))
        //        {
        //            result.Add(name);
        //        }

        //    }
        //    return result;
        //}

        public override bool DeleteObject(string xSetName, string xStreamName, bool isDeleteSubFile)
        {
            bool result = false;
            //if (isDeleteSubFile)
            //{
            //    List<string> subFiles = ListObject(xSetName, xStreamName);
            //    foreach (string name in subFiles)
            //    {
            //        result = DeleteObject(xSetName, PathUtil.CombinePath(xStreamName, name), null, Headers);
            //        if (!result)
            //        {
            //            return false;
            //        }
            //    }
            //}
            result = DeleteObject(xSetName, xStreamName, null, Headers);
            return result;
        }

        protected bool IsRetryableException(Exception ex)
        {
            bool result = false;
            Exception tempEx = ex;
            if (ex != null)
            {
                while (true)
                {
                    if (tempEx is RetryableException || tempEx.InnerException is RetryableException)
                    {
                        result = true;
                        break;
                    }
                    if (tempEx.InnerException == null)
                    {
                        break;
                    }
                    else
                    {
                        tempEx = tempEx.InnerException;
                    }
                }
            }
            return result;
        }

        private string GetAvailableHost(string id)
        {
            string result = null;
            if (cacheOpenParams.ContainsKey(id))
            {
                HCPOpenParameter param = cacheOpenParams[id];
                if (param != null)
                {
                    result = param.PrimaryHost;
                    if (param.IsSecondaryTimeOut)
                    {
                        try
                        {
                            logger.Debug("IsSecondaryTimeOut = true, endCacheSecondaryTime={0}", DateTime.UtcNow);
                            HasPermissions(param.FailedPrimaryHost);
                            param.SecondaryHost = param.PrimaryHost;
                            result = param.FailedPrimaryHost;
                            logger.Debug("Validate FailedPrimaryHost '{0}' succeed.", param.FailedPrimaryHost);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceWarning(ex.Message);
                            logger.Debug("Validate FailedPrimaryHost '{0}' failed.", param.FailedPrimaryHost);
                        }
                        finally
                        {
                            cacheOpenParams[id] = null;
                        }
                    }
                    logger.Debug("GetAvailableHost: {0}", result);
                }
            }
            return result;
        }

        public void SetAvailableHost(string id, HCPOpenParameter param)
        {
            param.BeginCacheSecondaryTime = DateTime.UtcNow;
            logger.Debug("SetAvailableHost: {0} , beginCacheSecondaryTime={1} .", param.PrimaryHost, param.BeginCacheSecondaryTime);
            cacheOpenParams[id] = param;
        }

        private object DeleteSecondary(MethodInfo methodInfo, object obj, object[] args)
        {
            object result = null;
            string tempHost = null;
            try
            {
                result = methodInfo.Invoke(this, args);

                tempHost = this.OpenParam.PrimaryHost;

                if (!string.IsNullOrEmpty(this.OpenParam.SecondaryHost))
                {
                    this.OpenParam.PrimaryHost = this.OpenParam.SecondaryHost;
                }
                else if (!string.IsNullOrEmpty(this.OpenParam.FailedPrimaryHost))
                {
                    this.OpenParam.PrimaryHost = this.OpenParam.FailedPrimaryHost;
                }
                else
                {
                    throw new Exception("FailedPrimaryHost and SecondaryHost is NullOrEmpty.");
                }
                result = methodInfo.Invoke(this, args);
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw new DeleteFailedException(ex.Message, ex);
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempHost))
                {
                    this.OpenParam.PrimaryHost = tempHost;
                }
            }
        }

        //public object ExcuteInvoke(MethodInfo methodInfo, object obj, object[] args)
        //{
        //    object result = null;
        //    if (OpenParam.IsHaveSecondaryHost &&
        //        (methodInfo.Name.Equals("DeleteObject") || methodInfo.Name.Equals("DeleteContainer")))
        //    {
        //        result = DeleteSecondary(methodInfo, this, args);
        //    }
        //    else
        //    {
        //        result = methodInfo.Invoke(this, args);
        //    }
        //    return result;
        //}

        public override object Invoke(string methodName, object[] args)
        {
            Logger.Debug("InvokeMethodName: {0}", methodName);
            object result = null;
            Type[] types = GetTypes(args);
            MethodInfo methodInfo = methodInfo = this.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.ExactBinding, null, types, null);
            try
            {
                if (OpenParam.CacheSecondary && OpenParam.SecondaryTimeout != 0)
                {
                    string avilableHost = GetAvailableHost(OpenParam.PhysicalIdAndMidifyTime);
                    if (!string.IsNullOrEmpty(avilableHost))
                    {
                        this.OpenParam.PrimaryHost = avilableHost;
                    }
                }

                if (OpenParam.IsUsedSecondaryHost)
                {
                    if (OpenParam.FailOverMode == FailoverMode.Off)
                    {
                        throw new FailoverModeException(string.Format("InvokeMethodName: {0} failed, because failover mode is off.", methodName));
                    }
                    if (OpenParam.FailOverMode == FailoverMode.ReadWrite)
                    {
                        result = methodInfo.Invoke(this, args);
                    }
                    else if (OpenParam.FailOverMode == FailoverMode.Read
                            && (methodInfo.Name.Equals("OpenObjectForRead")
                                || methodInfo.Name.Equals("CheckContainer")
                                || methodInfo.Name.Equals("CheckObject")
                                || methodInfo.Name.Equals("GetObjectInfo")))
                    {
                        result = methodInfo.Invoke(this, args);
                    }
                    else
                    {
                        throw new FailoverModeException(string.Format("InvokeMethodName: {0} failed, FailOverMode={1}", methodName, OpenParam.FailOverMode.ToString()));
                    }
                }
                else
                {
                    result = methodInfo.Invoke(this, args);
                }
            }
            catch (DeleteFailedException t)
            {
                logger.Error(t.Message, t);
                throw;
            }
            catch (FailoverModeException t)
            {
                logger.Error(t.Message, t);
                throw;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is PathNotFoundException)
                {
                    throw ex.InnerException;
                }

                if (IsRetryableException(ex))
                {
                    try
                    {
                        result = RetryRequset.ExcuteRetry(methodInfo, this, args);
                    }
                    catch (Exception e)
                    {
                        logger.Error("The PrimaryHost retry failed cause: {0}.", e);
                    }
                }

                if (result == null)
                {
                    logger.Error("The PrimaryHost is failed: {0}.", ex);
                    result = HandleFailOverMode(methodInfo, this, args, ex);
                }
            }
            return result;
        }

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

        public override List<string> ListContainers()
        {
            throw new NotSupportedException();
        }

        public override List<string> ListObject(string xSetName)
        {
            throw new NotSupportedException();
        }

        public override List<string> ListObject(string xSetName, string prefix)
        {
            throw new NotSupportedException();
        }

        public override HttpWebRequest GetUploadRequest(string xSetName, string xStreamName, string mimeType, HttpWebRequest webRequest, int blockNumber, long dataLength)
        {
            throw new NotSupportedException();
        }

        public override bool CreateObject(string xSetName, string xStreamName, HttpWebRequest request, long dataLength)
        {
            throw new NotSupportedException();
        }

        public override Stream OpenObject(string xSetName, string xStreamName, int rangFrom, int rangeTo)
        {
            throw new NotSupportedException();
        }

        public override Stream OpenObject(string container, string objectName, int[] lengths, FileMode mode)
        {
            throw new NotSupportedException();
        }

        public override CloudFileInfo GetObjectInfo(string xSetName, string xStreamName)
        {
            throw new NotSupportedException();
        }

        public override long GetContainerSize(string xSetName)
        {
            throw new NotSupportedException();
        }

        public override SpaceInfo GetUserAccountInfo()
        {
            throw new NotSupportedException();
        }

        public override List<XDirectoryInfo> Parse2Directory(string responseXmlString, string path)
        {
            throw new NotSupportedException();
        }

        public override List<XFileInfo> Parse2File(string responseXmlString)
        {
            throw new NotSupportedException();
        }

        public override string GetFinalUrl(StorageInfo info)
        {
            throw new NotSupportedException();
        }
    }
}