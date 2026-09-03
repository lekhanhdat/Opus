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
namespace AvePoint.Media.Storage.S3Compatible.REST
{
    #region using directives
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Cloud.S3Compatible;
    using AvePoint.Media.Storage.Util;
    using GCommon.Utility.Cryptography;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.XPath;
    #endregion

    /// <summary>
    /// Amazon使用Virtual Folder的概念，那么
    /// </summary>
    class S3CompatibleClient : AbstractRESTOprationExecutor, ICloudOprationExecutor
    {
        S3CompatibleOpenParameter openParams;
        #region -- Constructor --
        public S3CompatibleClient(String endpoint)
        {
            Protocol = "http";
            Endpoint = endpoint;
            HttpClient = new S3CompatibleHttpClient();
            Logger = StorageLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        }
        #endregion

        #region -- ICloudOprationExecutor Members --

        public override void InitConfig(CloudOpenParameter prams)
        {
            openParams = prams as S3CompatibleOpenParameter;
            (HttpClient as S3CompatibleHttpClient).OpenParam = openParams;
            this.CloudOpenParam = openParams;
            this.InitProxySetting();
            InitRetry(prams);
        }

        //在每一种Cloud中重写这个方法就是为了把openParams.FlushDNS这个参数默认false，这个属性Cloud中不会用到
        public override void InitRetry(CloudOpenParameter openParams)
        {
            Logger.Info("Init Retry: retryCount " + openParams.MaxRetryCount + ",RetryInterval " + openParams.RetryInterval);
            RetryRequset = new Retry(openParams.MaxRetryCount, openParams.RetryInterval, openParams.NeedRetry, true);
        }

        /*
         * <?xml version="1.0" encoding="UTF-8"?>
         * <ListAllMyBucketsResult xmlns="http://s3.amazonaws.com/doc/2006-03-01/">
         * <Owner>
         *      <ID>5725c654e7ac80d760d0ba9d9a6bb0263d8b440ea1884f30d85ab5ba58041bf9</ID>
         *      <DisplayName>avepoint</DisplayName>
         * </Owner>
         * <Buckets>
         *      <Bucket>
         *          <Name>avepointpatch</Name>
         *          <CreationDate>2008-04-29T14:17:34.000Z</CreationDate>
         *      </Bucket>
         * </Buckets>
         * </ListAllMyBucketsResult>
         */
        public override List<string> ListContainers()
        {
            List<string> result = null;

            try
            {
                S3CompatibleRequest request = GetS3CompatibleRequst(BuildURL());
                request.Method = RESTCommands.GET;

                using (HttpWebResponse response = DoExecute(request))
                {
                    string message = GetStringFromResponse(response);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = ExtractMessageWithNP(message, "Buckets/Bucket/Name");

                        int index = result.Count - 1;
                        string item = null;
                        while (index >= 0)
                        {
                            item = result[index--];
                            /*
                             * 屏蔽公司保护的Bucket
                             */
                            if (item.Equals("DOWNLOAD2.AVEPOINT.COM".ToLower(CultureInfo.InvariantCulture), StringComparison.CurrentCultureIgnoreCase) || item.Equals("download2", StringComparison.CurrentCultureIgnoreCase) || item.Equals("AVEPOINTPATCH".ToLower(CultureInfo.InvariantCulture), StringComparison.CurrentCultureIgnoreCase) || item.Equals("AVEPOINTPATCH.AVEPOINT.COM".ToLower(CultureInfo.InvariantCulture), StringComparison.CurrentCultureIgnoreCase))
                            {
                                result.Remove(item);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("List containers failed.", e);
                throw;
            }

            return result;
        }

        public override bool Login(string xSetName)
        {
            bool result = false;
            try
            {
                if (CheckBucket(xSetName))
                {
                    result = true;
                }
                else if (openParams.ModuleType == 0)
                {
                    result = CreateBucket(xSetName);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
                throw;
            }
            return result;

        }

        private bool CopyFile(string baseURL, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
        {
            bool result = false;
            HttpWebRequest requestPut = HttpClient.CreateRequestPut(baseURL, queryParams);
            using (HttpWebResponse resp = DoExecute(requestPut, headers))
            {
                using (Stream inputStream = resp.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(inputStream))
                    {
                        if (resp.StatusCode == HttpStatusCode.OK)
                        {
                            result = true;
                        }
                        else
                        {
                            Logger.Error("CopyFile failed {0}", requestPut.RequestUri);
                        }
                    }
                }
            }
            return result;
        }

        public override bool CheckContainer(string xSetName)
        {
            bool result = false;
            try
            {
                string uri = BuildURL(xSetName);
                S3CompatibleRequest request = GetS3CompatibleRequst(uri);
                request.Method = RESTCommands.GET;

                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
                    else if (response.StatusCode == HttpStatusCode.MovedPermanently)
                    {
                        throw new BucketInOtherRegionException(String.Format("The container named {0} is already exist in another region.", xSetName));
                    }
                    else
                    {
                        string errorMsg = string.Empty;
                        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        {
                            errorMsg = sr.ReadToEnd();
                        }
                        throw new Exception("Status Code: " + response.StatusCode + "Error Message: " + errorMsg);
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                Trace.TraceWarning(e.Message);
                result = false;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                throw;
            }
            return result;
        }

        public override bool CreateContainer(string xSetName)
        {
            bool result = false;
            try
            {
                string uri = BuildURL(xSetName);
                S3CompatibleRequest request = GetS3CompatibleRequst(uri);
                request.Method = RESTCommands.PUT;
                AddLocationConstraint(request);
                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(GetCreateCtnErrorMsg(xSetName), e);
                throw;
            }
            return result;
        }

        public override bool DeleteContainer(string xSetName)
        {
            bool result = false;
            try
            {
                List<string> objects = ListObject(xSetName);
                foreach (string obj in objects)
                {
                    DeleteObject(xSetName, obj);
                }

                string uri = BuildURL(xSetName);
                S3CompatibleRequest request = GetS3CompatibleRequst(uri);
                request.Method = RESTCommands.DELETE;

                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        result = true;
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                Logger.Warn("Cannot find the object, maybe it was deleted successfully before : " + e.Message);
            }
            catch (Exception e)
            {
                Logger.Error(GetDeleteCtnErrorMsg(xSetName), e);
                throw;
            }
            return result;
        }

        public override List<string> ListObject(string xSetName)
        {
            return ListObject(xSetName);
        }

        public override List<string> ListObject(string xSetName, string prefix)
        {
            List<string> objFullNames = ListObjectWithPreFix(xSetName, prefix, true);
            List<string> objNames = new List<string>();
            foreach (string name in objFullNames)
            {
                string streamName = name.Substring(name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
                if (!string.IsNullOrEmpty(streamName))
                {
                    objNames.Add(streamName);
                }
            }
            return objNames;
        }

        public long GetContainerSize(string xSetName, string prefix)
        {
            return GetXsetSize(xSetName, prefix);
        }

        protected override List<string> ListXstream(string xSetName, string prefix, int limit, string marker, bool isGetName)
        {
            List<string> result = null;
            S3CompatibleRequest request = null;
            try
            {
                string baseUri = BuildURL(xSetName);
                Dictionary<string, string> paramaters = new Dictionary<string, string>();
                if (string.IsNullOrEmpty(prefix))
                {
                    paramaters.Add("delimiter", "/");
                }
                if (!string.IsNullOrEmpty(prefix))
                {
                    if (prefix.Contains("\\"))
                    {
                        prefix = prefix.Replace("\\", "/");
                    }
                    paramaters.Add(S3CompatibleConstants.PREFIX, prefix);
                }
                if (!string.IsNullOrEmpty(marker))
                {
                    paramaters.Add(S3CompatibleConstants.MARKER, marker);
                }
                if (limit > 0)
                {
                    paramaters.Add(S3CompatibleConstants.MAX_KEYS, limit.ToString());
                }
                string queryStr = ConvertQueryList2String(paramaters);
                request = GetS3CompatibleRequst(baseUri + queryStr);
                request.Method = RESTCommands.GET;

                using (HttpWebResponse response = DoExecute(request))
                {
                    string message = GetStringFromResponse(response);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if (isGetName)
                        {
                            result = ExtractMessageWithNP(message, "Contents/Key");
                        }
                        else
                        {
                            result = ExtractMessageWithNP(message, "Contents/Size");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(GetListObjErrorMsg(prefix), e);
                throw;
            }

            return result;
        }

        public override bool CheckObject(string xSetName, string xStreamName)
        {
            bool result = false;
            string prefix = xStreamName;
            Dictionary<string, string> queryParams = new Dictionary<string, string>();

            try
            {
                if (!prefix.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    string fullURL = BuildURL(xSetName, xStreamName);
                    HttpWebRequest webRequest = HttpClient.CreateRequestHead(fullURL, null);
                    webRequest.ContentLength = 0;
                    using (HttpWebResponse response = DoExecute(webRequest, this.Headers))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            result = true;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(prefix) && "/".Equals(prefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = CheckContainer(xSetName);
                }
                else
                {
                    string baseURL = BuildURLWithOutQueryParams(xSetName);
                    queryParams.Add("prefix", prefix);
                    queryParams.Add("delimiter", "/");
                    string finalURL = HttpClient.CombiningQueryParams(baseURL, queryParams);
                    S3CompatibleRequest request = GetS3CompatibleRequst(finalURL);
                    request.Method = RESTCommands.GET;

                    using (HttpWebResponse response = DoExecute(request))
                    {
                        result = HasRespContentCounts(response);
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                Logger.Info(e.Message + " CanNotFound.");
                result = false;
            }
            catch (Exception e)
            {
                Logger.Error(GetCheckObjErrorMsg(xSetName, prefix), e);
                throw;
            }

            return result;
        }

        public bool HasRespContentCounts(HttpWebResponse resp)
        {
            bool result = false;
            ResponseInfo respInfo = new ResponseInfo();
            using (Stream inputStream = resp.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(inputStream))
                {
                    respInfo.ResponseXml = Decode(reader.ReadToEnd());
                    respInfo.ResponseXml = respInfo.ResponseXml.Replace(" xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\"", "");
                    List<XPathNavigator> navs = FirstStepAnalyzeXML(respInfo.ResponseXml, "ListBucketResult/Contents");
                    navs.AddRange(FirstStepAnalyzeXML(respInfo.ResponseXml, "ListBucketResult/CommonPrefixes"));
                    if (navs.Count > 0)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        public override HttpWebRequest GetUploadRequest(string xSetName, string xStreamName, string mimeType, HttpWebRequest webRequest, int blockNumber, long dataLength)
        {
            if (webRequest != null)
            {
                return webRequest;
            }
            string uri = BuildURL(xSetName, xStreamName);
            S3CompatibleRequest request = GetS3CompatibleRequst(uri);
            request.Method = RESTCommands.PUT;
            request.Headers.Add("Content-Type", mimeType);
            request.Headers.Add("Content-Length", dataLength.ToString());
            request.Headers.Add("Expect", "100-Continue");
            Logger.Info("get stream for file, xSet:" + xSetName + ", xStream:" + xStreamName);
            return HttpClient.GetWebRequestForUpLoad(request);
        }

        public override bool CreateObject(string xSetName, string xStreamName, HttpWebRequest request, long dataLength)
        {
            bool result = false;

            try
            {
                using (HttpWebResponse response = UpLoad(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
                    else
                    {
                        Logger.Error("Create object failed. object : " + xStreamName + ",container : " + xSetName + ", statues:" + response.StatusCode);
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
                Logger.Error(GetCreateObjErrorMsg(xStreamName, xSetName), e);
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
                    result = DeleteObject(xSetName, PathUtil.CombinePath(xStreamName, name));
                    if (!result)
                    {
                        return false;
                    }
                }
            }
            result = DeleteObject(xSetName, xStreamName);
            return result;
        }

        public ResponseInfo DeleteObjects(string fullURL, Dictionary<string, string> requestParams, Dictionary<string, string> requestHeaders, string content)
        {

            HttpWebRequest request = HttpClient.CreateRequestPost(fullURL, requestParams);

            byte[] cbytes = System.Text.Encoding.UTF8.GetBytes(content);
            requestHeaders["Content-Length"] = cbytes.Length.ToString();
            requestHeaders["Content-MD5"] = S3CompatibleUtils.Base64Encoded128BitMD5Digest(content);
            requestHeaders["Content-Type"] = "application/xml";

            HttpClient.AddHeaders(request, requestHeaders);
            request.AllowWriteStreamBuffering = false;
            request.AllowAutoRedirect = false;
            request.Timeout = StorageConstants.DefaultHttpRequestTimeout; //never timeout

            //if (openParams.SignatureVersion == 4)
            //{
            //    byte[] hashByte = CryptoUtil.ComputeHash(cbytes, 0, cbytes.Length);
            //    string hashStr = CryptoUtil.ToHex(hashByte, true);
            //    request.Headers.Add("X-Amz-Content-SHA256", hashStr);
            //}
            S3CompatibleUtils.AddAuthorization(request, openParams.UserName, openParams.Password);


            using (Stream uploader = request.GetRequestStream())
            {
                uploader.Write(cbytes, 0, cbytes.Length);
            }
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        string responseXML = string.Empty;
                        using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                        {
                            responseXML = reader.ReadToEnd();
                        }
                        return new ResponseInfo()
                        {
                            ResponseXml = responseXML
                        };

                    }
                }
            }
            catch (WebException e)
            {
                Logger.Error(e.Message, e);
                throw;
            }


            return null;
        }

        private bool DeleteObject(string xSetName, string xStreamName)
        {
            bool result = false;

            string uri = BuildURL(xSetName, xStreamName);
            S3CompatibleRequest request = GetS3CompatibleRequst(uri);
            request.Method = RESTCommands.DELETE;

            try
            {
                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        result = true;
                    }
                    else
                    {
                        string message = GetStringFromResponse(response);
                        Logger.Warn("Delete object " + xStreamName + " failed, message : " + message);
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                Logger.Warn("Cannot find the object, maybe it was deleted successfully before : " + e.Message);
                result = true;
            }
            catch (Exception e)
            {
                Logger.Error(GetDeleteObjErrorMsg(xStreamName, xSetName), e);
                throw;
            }

            return result;
        }

        public override CloudFileInfo GetObjectInfo(string xSetName, string xStreamName)
        {
            CloudFileInfo result = new CloudFileInfo();
            try
            {
                string uri = BuildURL(xSetName, xStreamName);
                S3CompatibleRequest request = GetS3CompatibleRequst(uri);
                request.Method = RESTCommands.GET;
                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result.FileSize = response.ContentLength;
                    }
                    else
                    {
                        string message = GetStringFromResponse(response);
                        Logger.Warn("Get object " + xStreamName + " size failed, message : " + message);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Get object size failed, object : " + xStreamName + ", container : " + xSetName, e);
                throw;
            }
            return result;
        }

        public override Stream OpenObject(string container, string objectName, int[] lengths, FileMode mode)
        {
            //string uri = BuildURL(
            string uri = BuildURL(container, objectName);
            S3CompatibleRequest request = GetS3CompatibleRequst(uri);

            switch (mode)
            {
                case FileMode.Open:
                    request.Method = RESTCommands.GET;
                    if (lengths != null && lengths.Length == 3)
                    {
                        int rangFrom = lengths[1];
                        int rangTo = lengths[2];
                        if (rangFrom >= 0 && rangTo >= 0 && rangFrom < rangTo)
                        {
                            string range = "bytes=" + rangFrom + "-" + rangTo;
                            request.Headers.Add("Range", range);
                        }
                    }

                    HttpWebResponse response = DoExecute(request);
                    return new HttpDownloadStream(response);
                case FileMode.Create:
                case FileMode.CreateNew:
                    request.Method = RESTCommands.PUT;
                    request.Headers.Add("Content-Type", "DocAve/data".ToLower(CultureInfo.InvariantCulture));
                    int dataLength = lengths[0];
                    request.Headers.Add("Content-Length", dataLength.ToString());
                    request.Headers.Add("Expect", "100-Continue");
                    //logger.Info("get stream for file, xSet:" + xSetName + ", xStream:" + xStreamName);
                    return HttpClient.GetWebRequestForUpLoad(request).GetRequestStream();
                default:
                    break;

            }
            return null;

        }

        public override Stream OpenObject(string xSetName, string xStreamName, int rangFrom, int rangeTo)
        {
            Stream result = null;

            string uri = BuildURL(xSetName, xStreamName);
            S3CompatibleRequest request = GetS3CompatibleRequst(uri);
            request.Method = RESTCommands.GET;
            if (rangFrom >= 0 && rangeTo >= 0 && rangFrom < rangeTo)
            {
                string range = "bytes=" + rangFrom + "-" + rangeTo;
                request.Headers.Add("Range", range);
            }
            try
            {
                HttpWebResponse response = DoExecute(request);
                return new HttpDownloadStream(response);
            }
            catch (Exception e)
            {
                Logger.Error(GetOpenObjErrorMsg(xStreamName, xSetName), e);
                throw;
            }
        }

        #endregion

        #region -- Bucket Related --

        public string GetBucketName()
        {
            if (openParams == null || openParams.EndPoint == null)
            {
                return "";
            }
            StringBuilder bucket = new StringBuilder();
            bucket.Append(openParams.Bucket)
                  .Append(".")
                  .Append(openParams.UserName.ToLower(CultureInfo.InvariantCulture));
            return bucket.ToString();
        }

        /*
         * <?xml version="1.0" encoding="UTF-8"?>
            <Error>
            	<Code>BucketAlreadyOwnedByYou</Code>
            	<Message>Your previous request to create the named bucket succeeded and you already own it.</Message>
            	<BucketName>test.test.test.docave.test</BucketName>
            	<RequestId>A3C7825C59E5E141</RequestId>
            	<HostId>Ly66hgggQXwHpNF/NcRmN1zYRtMtW/tXI+sOC5hPydAMfy6DPGT45g41bC1YqyuI</HostId>
            </Error>
         */
        protected bool CreateBucket(string bucketName)
        {
            bool result = false;

            string uri = BuildURL(bucketName);
            S3CompatibleRequest request = GetS3CompatibleRequst(uri);
            request.Method = RESTCommands.PUT;
            AddLocationConstraint(request);

            try
            {
                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
                }
            }
            //TODO know why
            //catch (WebException e)
            //{

            //    if (IsProtocalError(e))
            //    {
            //        HttpWebResponse resp = e.Response as HttpWebResponse;
            //        string message = GetStringFromResponse(resp);
            //        if (resp.StatusCode == HttpStatusCode.Conflict)
            //        {
            //            List<String> codes = AnalyzeXML(resp.GetResponseStream(), "Error/Code");

            //            if (codes != null && codes.Contains("BucketAlreadyOwnedByYou"))
            //            {
            //                result = true;
            //            }
            //        }
            //        else
            //        {
            //            logger.Warn("Create bucket " + bucketName + " failed, message : " + message);
            //            throw;
            //        }
            //    }
            //    else
            //    {
            //        logger.Warn("Create bucket " + bucketName + " failed");
            //        throw;
            //    }
            //}
            catch (Exception e)
            {
                Logger.Error("Error when create bucket", e);
                throw;
            }

            return result;
        }

        protected bool CheckBucket(string bucketName)
        {
            bool result = false;

            string uri = BuildURL(bucketName);
            S3CompatibleRequest request = GetS3CompatibleRequst(uri);
            request.Method = RESTCommands.GET;
            try
            {
                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                Trace.TraceWarning(e.Message);
                result = false;
            }
            catch (Exception e)
            {
                Logger.Error("Error when check bucket", e);
                throw;
            }
            return result;
        }

        #endregion

        #region -- Amazon Special Methods --

        protected List<string> ExtractMessageWithNP(string xml, string xPath)
        {
            List<string> result = null;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("amazon", "http://s3.amazonaws.com/doc/2006-03-01/");
            result = AnalyzeXML(xml, xPath, dic);

            return result;
        }

        protected void AddLocationConstraint(S3CompatibleRequest request)
        {
            string bucket = "";
            Stream stream = new MemoryStream();
            stream.Write(Encoding.UTF8.GetBytes(bucket), 0, bucket.Length);
            request.DataStream = stream;
            stream.Position = 0;
            request.Headers.Add("Content-Length", stream.Length.ToString());
        }

        /*
         * Status Code : 403
         <?xml version="1.0" encoding="UTF-8"?>
         * <Error>
              <Code>RequestTimeTooSkewed</Code>
              <Message>The difference between the request time and the current time is too large.<Message>
              <MaxAllowedSkewMilliseconds>900000</MaxAllowedSkewMilliseconds>
              <RequestId>8F2C8B90F112BF07</RequestId>
              <HostId>iJ01YH9NEYCVj4bF4WMKNh36f5KyVT6J9NcsITYPCXvH2NdL/TofrM51UbxcXP4G</HostId>
              <RequestTime>Mon, 11 Jan 2010 04:05:53 GMT</RequestTime>
              <ServerTime>2010-02-05T04:05:27Z</ServerTime>
         * </Error>
         */
        protected bool IsRequestTimeTooSkewed(HttpWebResponse response)
        {
            bool result = false;

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                string message = GetStringFromResponse(response);
                Logger.Warn("Message : " + message);
                List<string> codes = AnalyzeXML(message, "Error/Code");
                if (codes.Contains("RequestTimeTooSkewed"))
                {
                    result = true;
                }
            }

            return result;
        }

        protected TimeSpan GetTimeOffset(HttpWebResponse response)
        {
            string serverDate = response.Headers.GetValues("Date")[0];
            return DateTime.Parse(serverDate) - DateTime.Now;
        }

        protected bool IsProtectedBucket(string bucketName)
        {
            bool result = false;

            return result;
        }

        #endregion

        #region -- Build URL --

        /*
         * Buckets with names containing uppercase characters are not accessible using the virtual hosted-style request.
         * 因此我们要判断一下，如果bucketName中包含大写字母则使用path-style
         */
        protected string BuildURL(string bucketName)
        {
            if (Endpoint.EndsWith("/"))
            {
                Endpoint = Endpoint.TrimEnd('/');
            }
            if (Endpoint.EndsWith("\\"))
            {
                Endpoint = Endpoint.TrimEnd('\\');
            }
            return Endpoint.Trim('/') + "/" + bucketName;
        }

        protected string BuildURL(string bucketName, string objectName)
        {
            string url = string.Empty;
            if (objectName.Contains("\\"))
            {
                objectName = objectName.Replace("\\", "/");
            }
            objectName = Encode(objectName);
            url = BuildURL(bucketName) + "/" + objectName;
            return url;
        }

        protected string BuildURL(string bucketName, string xSetName, string xStreamName)
        {
            if (xStreamName.Contains("\\"))
            {
                xStreamName = xStreamName.Replace("\\", "/");
            }
            if (openParams.ModuleType != 0)
            {
                // 如果不是media使用，xset代表真实的bucket
                return BuildURL(xSetName) + Encode(xStreamName);
            }
            else
            {
                // 如果是media使用，xset代表是虚拟文件夹
                return BuildURL(bucketName, xSetName) + Encode(xStreamName);
            }
        }

        #endregion

        #region -- AbstractRESTOprationExecutor Members --

        protected override bool SpecialRetryCondition(BasicRequest request, HttpWebResponse resp)
        {
            bool result = false;

            if (resp.StatusCode == HttpStatusCode.TemporaryRedirect)
            {
                //mLogger.Info("URI redirect, new location : " + request.URI);

                request.URI = resp.ResponseUri.ToString();

                if (request.DataStream != null)
                {
                    request.DataStream.Position = 0;
                }
                result = true;
            }
            else if (IsRequestTimeTooSkewed(resp))
            {
                TimeSpan timeOffset = GetTimeOffset(resp);
                request.Headers[S3CompatibleConstants.S3Compatible_ALTERNATIVE_DATE] = S3CompatibleUtils.GetReqeustDate(timeOffset);
                HttpClient.TimeOffset = timeOffset;

                result = true;
            }

            return result;
        }

        #endregion

        protected S3CompatibleRequest GetS3CompatibleRequst(string uri)
        {
            S3CompatibleRequest request = new S3CompatibleRequest();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add(S3CompatibleConstants.S3Compatible_ALTERNATIVE_DATE, S3CompatibleUtils.GetReqeustDate(HttpClient.TimeOffset));
            request.URI = uri;
            request.UserName = openParams.UserName;
            request.Password = openParams.Password;
            request.Headers = headers;
            return request;
        }

        public override string GetDocAveDefaultContainer()
        {
            return GetBucketName();
        }

        public override string BuildObjectAbsoluteURL(string container, string objectName)
        {
            return BuildURL(container, objectName);
        }

        public override string BuildURLWithOutQueryParams(string container)
        {
            return BuildURL(container);
        }

        public override Dictionary<string, string> Headers
        {
            get
            {
                string date = S3CompatibleUtils.GetReqeustDate(HttpClient.TimeOffset);
                return new Dictionary<string, string>() { 
                    { S3CompatibleConstants.S3Compatible_ALTERNATIVE_DATE,  date},
                };
            }
        }

        public override Dictionary<string, string> ListDirectoryHeaders
        {
            get
            {
                Dictionary<string, string> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<string, string> OpenDirectoryWriteModeHeaders
        {
            get
            {
                Dictionary<string, string> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<string, string> OpenFileWriteModeHeaders
        {
            get
            {
                Dictionary<string, string> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<string, string> ListDirectoryQueryParams
        {
            get { return new Dictionary<string, string>() { { "max-keys".ToLower(CultureInfo.InvariantCulture), "1000" } }; }
        }

        public override Dictionary<string, string> OpenFileReadModeHeaders
        {
            get
            {
                Dictionary<string, string> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<string, string> OpenDirectoryReadModeHeaders
        {
            get
            {
                Dictionary<string, string> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<string, string> OpenStreamReadModeHeaders
        {
            get
            {
                Dictionary<string, string> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<string, string> OpenStreamWriteModeHeaders
        {
            get
            {
                Dictionary<string, string> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<string, string> ListObjectHeaders
        {
            get
            {
                Dictionary<string, string> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<string, string> CopyFileHeaders
        {
            get
            {
                Dictionary<string, string> headers = Headers;
                return headers;
            }
        }

        public override HttpUploadStream OpenObjectForWrite(string fullURL, Dictionary<string, string> headers)
        {
            long length = Convert.ToInt64(headers["Content-Length"]);
            if (length > openParams.BlockLength * 1024 * 1024)
            {
                return new S3CompatibeMultipartUploadStream(this, fullURL, headers);
            }
            else
            {
                HttpWebRequest request = HttpClient.CreateRequestPut(fullURL, null);
                HttpClient.AddHeaders(request, headers);
                return new S3CompatibleUploadStream(request, openParams) { HttpClient = this.HttpClient, System = this.HttpClient.CurrentSystem };
            }
        }

        public string InitiateMultipartUpload(string fullURL, Dictionary<string, string> headers)
        {
            if (headers.ContainsKey("Content-Type"))
            {
                headers.Remove("Content-Type");
            }
            if (headers.ContainsKey("Content-Length"))
            {
                headers.Remove("Content-Length");
            }
            fullURL = fullURL + "?uploads";
            HttpWebRequest req = HttpClient.CreateRequestPost(fullURL, null);
            req.ContentLength = 0;
            req.Headers.Add(S3CompatibleConstants.S3Compatible_REST_HEADER_PREFIX + "storage-class", "STANDARD");
            req.ContentType = "application/octet-stream";
            req.ServicePoint.Expect100Continue = false;

            string uploadId = null;
            using (HttpWebResponse resp = DoExecute(req, headers))
            {
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    string respXml = sr.ReadToEnd();
                    Regex r = new Regex("<UploadId>([^<].*)</UploadId>");
                    Match m = r.Match(respXml);
                    if (!m.Success)
                    {
                        throw new Exception(string.Format("Not found UploadId, xml='{0}'", respXml));
                    }
                    uploadId = m.Groups[1].Value;
                }
            }
            Logger.Debug("Initiate Multipart Upload succeed, uploadId={0} url='{1}'.", uploadId, fullURL);
            return uploadId;
        }

        public string UploadPart(string fullURL, byte[] buffer, int offset, int count)
        {
            return RetryRequset.CloudRetry<string>(delegate()
            {
                HttpWebRequest req = HttpClient.CreateRequestPut(fullURL, null);
                req.ContentLength = count;
                HttpClient.AddHeaders(req, this.Headers);
                req.AllowWriteStreamBuffering = false;
                req.AllowAutoRedirect = false;
                req.Timeout = StorageConstants.DefaultHttpRequestTimeout; //never timeout
                IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(AvePoint.GCommon.Utility.Cryptography.HashAlgorithm.MD5);
                byte[] data = md5.ComputeHash(buffer, offset, count);
                string contentMD5 = Convert.ToBase64String(data);
                req.Headers.Add("Content-MD5", contentMD5);
                S3CompatibleUtils.AddAuthorization(req, openParams.UserName, openParams.Password);
                using (Stream upStream = req.GetRequestStream())
                {
                    upStream.Write(buffer, 0, count);
                }

                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("Upload Part failed, url='{0}' HttpStatusCode={1}", resp.ResponseUri, resp.StatusCode));
                    }
                    string eTag = resp.Headers["ETag"];
                    Logger.Debug("Upload Part succeed, eTag={0} url='{1}'", eTag, fullURL);
                    return eTag;
                }
            });
        }

        public bool CompleteMultipartUpload(string fullURL, Dictionary<int, string> eTags)
        {
            return RetryRequset.CloudRetry<bool>(delegate()
            {
                StringBuilder xmlData = new StringBuilder();
                xmlData.Append("<CompleteMultipartUpload>");
                foreach (var eTag in eTags)
                {
                    xmlData.Append(string.Format("<Part><PartNumber>{0}</PartNumber><ETag>{1}</ETag></Part>", eTag.Key, eTag.Value));
                }
                xmlData.Append("</CompleteMultipartUpload>");
                byte[] xmlDataBuffer = Encoding.UTF8.GetBytes(xmlData.ToString());

                HttpWebRequest req = HttpClient.CreateRequestPost(fullURL, null);
                req.ContentLength = xmlDataBuffer.Length;
                req.ContentType = "application/xml";
                req.ServicePoint.Expect100Continue = false;

                HttpClient.AddHeaders(req, this.Headers);
                req.AllowWriteStreamBuffering = false;
                req.AllowAutoRedirect = false;
                req.Timeout = StorageConstants.DefaultHttpRequestTimeout; //never timeout
                IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(AvePoint.GCommon.Utility.Cryptography.HashAlgorithm.MD5);
                byte[] data = md5.ComputeHash(xmlDataBuffer, 0, xmlDataBuffer.Length);
                string contentMD5 = Convert.ToBase64String(data);
                req.Headers.Add("Content-MD5", contentMD5);
                S3CompatibleUtils.AddAuthorization(req, openParams.UserName, openParams.Password);
                using (Stream upStream = req.GetRequestStream())
                {
                    upStream.Write(xmlDataBuffer, 0, xmlDataBuffer.Length);
                }

                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("Complete Multipart Upload failed, url='{0}' HttpStatusCode={1}", resp.ResponseUri, resp.StatusCode));
                    }
                    Logger.Debug("Complete Multipart Upload succeed, url='{0}'", fullURL);
                    return true;
                }
            });
        }

        public override string GetFinalUrl(StorageInfo info)
        {
            throw new NotImplementedException();
        }

        public override string BuildObjectAbsoluteURL(string url, string container, string objectName)
        {
            throw new NotImplementedException();
        }

        public override string ListAzureMetaName(string baseURL, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
        {
            throw new NotImplementedException();
        }

        public override CloudOpenParameter ConveryParams(Dictionary<string, string> prams)
        {
            throw new NotImplementedException();
        }

        public override long GetContainerSize(string xSetName)
        {
            throw new NotImplementedException();
        }

        public override SpaceInfo GetUserAccountInfo()
        {
            throw new NotImplementedException();
        }

        public override List<XDirectoryInfo> Parse2Directory(string responseXmlString, string path)
        {
            throw new NotImplementedException();
        }

        public override List<XFileInfo> Parse2File(string responseXmlString)
        {
            throw new NotImplementedException();
        }
    }
}
