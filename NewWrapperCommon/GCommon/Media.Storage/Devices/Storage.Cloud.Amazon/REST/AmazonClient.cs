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





namespace AvePoint.Media.Storage.Cloud.Amazon
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.XPath;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Util;
    #endregion

    #region CodeReview
    [AveCodeReview(
       "2012/8/9",
       "rongbiao.sun@avepoint.com",
       "dapeng.zhang@avepoint.com",
        new String[] { CodeReviewConstants.CHECK_LIST_ID_LOG_1 },
        null,
        true)]
    #endregion
    /// <summary>
    /// Amazon使用Virtual Folder的概念，那么
    /// </summary>
    class AmazonClient : AbstractRESTOprationExecutor, ICloudOprationExecutor
    {
        AmazonOpenParameter openParams;
        StorageLogger logger = new StorageLogger(typeof(AmazonClient));
        #region -- Constructor --
        public AmazonClient()
        {
            Protocol = "https";
            Endpoint = StorageUrl.AmazonHostName;
            HttpClient = new AmazonHttpClient();
        }
        #endregion

        #region -- ICloudOprationExecutor Members --
        public override void InitConfig(CloudOpenParameter prams)
        {
            openParams = prams as AmazonOpenParameter;
            HttpClient.OpenParam = openParams;
            this.CloudOpenParam = openParams;
            this.InitProxySetting();
            InitRetry(prams);
            this.Protocol = prams.Protocol;
        }

        //在每一种Cloud中重写这个方法就是为了把openParams.FlushDNS这个参数默认false，这个属性Cloud中不会用到
        public override void InitRetry(CloudOpenParameter openParams)
        {
            logger.Info("Init Retry: retryCount {0}, RetryInterval {1}", openParams.MaxRetryCount, openParams.RetryInterval);
            RetryRequset = new Retry(openParams.MaxRetryCount, openParams.RetryInterval, openParams.NeedRetry, true);
        }

        public override List<String> ListContainers()
        {
            List<String> result = null;
            try
            {
                AmazonRequest request = GetAmazonRequst(BuildURL());
                request.Method = RESTCommands.GET;
                using (HttpWebResponse response = DoExecute(request))
                {
                    String message = GetStringFromResponse(response);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = ExtractMessageWithNP(message, "Buckets/Bucket/Name");
                        Int32 index = result.Count - 1;
                        String item = null;
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
                logger.Error("List containers failed. Details : {0}", e);
                throw;
            }
            return result;
        }

        public override Boolean Login(String xSetName)
        {
            Boolean result = false;
            try
            {
                if (CheckContainer(xSetName))
                {
                    result = true;
                }
                else if (openParams.ModuleType == 0)
                {
                    result = CreateContainer(xSetName);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error when Login. Details : {0}", ex);
                throw;
            }
            return result;
        }

        private Boolean CopyFile(String baseURL, Dictionary<String, String> queryParams, Dictionary<String, String> headers)
        {
            Boolean result = false;
            HttpWebRequest requestPut = HttpClient.CreateRequestPut(baseURL, queryParams);
            requestPut.ContentLength = 0;
            using (HttpWebResponse response = DoExecute(requestPut, headers))
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
        }

        public override Boolean CheckContainer(String xSetName)
        {
            Boolean result = false;
            try
            {
                String uri = BuildURL(xSetName);
                AmazonRequest request = GetAmazonRequst(uri);
                request.Method = RESTCommands.GET;
                using (HttpWebResponse response = DoExecute(request))
                {
                    result = response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (PathNotFoundException e)
            {
                logger.Warn("Path not found. Details : {0}", e);
                result = false;
            }
            catch (Exception e)
            {
                logger.Error("Check container failed, container : {0}. Details : {1}", xSetName, e);
                throw;
            }
            return result;
        }

        public override Boolean CreateContainer(String xSetName)
        {
            Boolean result = false;
            try
            {
                String uri = BuildURL(xSetName);
                AmazonRequest request = GetAmazonRequst(uri);
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
                logger.Error("Create container failed, container : {0}. Details : {1}", xSetName, e);
                throw;
            }
            return result;
        }

        public override Boolean DeleteContainer(String xSetName)
        {
            Boolean result = false;
            try
            {
                List<String> objects = ListObject(xSetName);
                foreach (String obj in objects)
                {
                    DeleteObject(xSetName, obj);
                }
                String uri = BuildURL(xSetName);
                AmazonRequest request = GetAmazonRequst(uri);
                request.Method = RESTCommands.DELETE;
                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        result = true;
                    }
                }
            }
            catch (PathNotFoundException ex)
            {
                logger.Warn("Cannot find the object, maybe it was deleted successfully before. Details:{0}", ex.Message);
            }
            catch (Exception e)
            {
                logger.Error("Delete container failed, container : {0}. Details:{1}", xSetName, e);
                throw;
            }
            return result;
        }

        public override List<String> ListObject(String xSetName)
        {
            return ListObject(xSetName, null);
        }

        public override List<String> ListObject(String xSetName, String prefix)
        {
            List<String> objFullNames = ListObjectWithPreFix(xSetName, prefix, true);
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

        public long GetContainerSize(String xSetName, String prefix)
        {
            return GetXsetSize(xSetName, prefix);
        }

        protected override List<String> ListXstream(String xSetName, String prefix, int limit, String marker, Boolean isGetName)
        {
            List<String> result = null;
            AmazonRequest request = null;
            try
            {
                String baseUri = BuildURL(xSetName);
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
                    paramaters.Add(AmazonConstants.PREFIX, prefix);
                }
                if (!String.IsNullOrEmpty(marker))
                {
                    paramaters.Add(AmazonConstants.MARKER, marker);
                }
                if (limit > 0)
                {
                    paramaters.Add(AmazonConstants.MAX_KEYS, limit.ToString());
                }
                String queryStr = ConvertQueryList2String(paramaters);
                request = GetAmazonRequst(baseUri + queryStr);
                request.Method = RESTCommands.GET;
                using (HttpWebResponse response = DoExecute(request))
                {
                    String message = GetStringFromResponse(response);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = isGetName ? ExtractMessageWithNP(message, "Contents/Key") : ExtractMessageWithNP(message, "Contents/Size");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("List object failed, container : {0}. Details : {1}", xSetName, e);
                throw;
            }
            return result;
        }

        public override Boolean CheckObject(String xSetName, String xStreamName)
        {
            Boolean result = false;
            String prefix = xStreamName;
            Dictionary<String, String> queryParams = new Dictionary<String, String>();
            try
            {
                if (!prefix.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    String fullURL = BuildURL(xSetName, xStreamName);
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
                else if (!String.IsNullOrEmpty(prefix) && "/".Equals(prefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = CheckContainer(xSetName);
                }
                else
                {
                    String baseURL = BuildURLWithOutQueryParams(xSetName);
                    queryParams.Add("prefix", prefix);
                    queryParams.Add("delimiter", "/");
                    String finalURL = HttpClient.CombiningQueryParams(baseURL, queryParams);
                    AmazonRequest request = GetAmazonRequst(finalURL);
                    request.Method = RESTCommands.GET;
                    using (HttpWebResponse response = DoExecute(request))
                    {
                        result = HasRespContentCounts(response);
                    }
                }
            }
            catch (PathNotFoundException ex)
            {
                logger.Warn("Path not found. Details : {0}", ex);
                result = false;
            }
            catch (Exception e)
            {
                logger.Error("Check object failed, object : {0}, prefix : {1}. Details : {2}", xSetName, prefix, e);
                throw;
            }
            return result;
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
                    List<XPathNavigator> navs = FirstStepAnalyzeXML(responseInfo.ResponseXml, "ListBucketResult/Contents");
                    navs.AddRange(FirstStepAnalyzeXML(responseInfo.ResponseXml, "ListBucketResult/CommonPrefixes"));
                    if (navs.Count > 0)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        public override HttpWebRequest GetUploadRequest(String xSetName, String xStreamName, String mimeType, HttpWebRequest webRequest, int blockNumber, long dataLength)
        {
            if (webRequest != null)
            {
                return webRequest;
            }
            String uri = BuildURL(xSetName, xStreamName);
            AmazonRequest request = GetAmazonRequst(uri);
            request.Method = RESTCommands.PUT;
            request.Headers.Add("Content-Type", mimeType);
            request.Headers.Add("Content-Length", dataLength.ToString());
            request.Headers.Add("Expect", "100-Continue");
            logger.Info("get stream for file, xSetName:{0}, xStreamName:{1}.", xSetName, xStreamName);
            return HttpClient.GetWebRequestForUpLoad(request);
        }

        public override Boolean CreateObject(String xSetName, String xStreamName, HttpWebRequest request, Int64 dataLength)
        {
            Boolean result = false;
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
                        logger.Error("Create object failed. object : {0} ,container : {1}, statues:{2}", xStreamName, xSetName, response.StatusCode);
                        throw new Exception("Create object failed. object : " + xStreamName + ",container : " + xSetName);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Create object failed, object : {0} , container : {1}. Details : {2}", xStreamName, xSetName, e);
                throw;
            }
            return result;
        }

        public override Boolean DeleteObject(String xSetName, String xStreamName, Boolean isDeleteSubFile)
        {
            Boolean result = false;
            if (isDeleteSubFile)
            {
                List<String> subFiles = ListObject(xSetName, xStreamName);
                foreach (String name in subFiles)
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "X-Amz-Content-SHA")]
        public ResponseInfo DeleteObjects(String fullURL, Dictionary<String, String> requestParams, Dictionary<String, String> requestHeaders, String content)
        {
            HttpWebRequest request = HttpClient.CreateRequestPost(fullURL, requestParams);
            Byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            requestHeaders["Content-Length"] = contentBytes.Length.ToString();
            requestHeaders["Content-MD5"] = AmazonUtils.Base64Encoded128BitMD5Digest(content);
            requestHeaders["Content-Type"] = "application/xml";
            HttpClient.AddHeaders(request, requestHeaders);
            request.AllowWriteStreamBuffering = false;
            request.AllowAutoRedirect = false;
            request.Timeout = 0x7ffffffe; //never timeout
            if (openParams.SignatureVersion == 4)
            {
                Byte[] hashByte = CryptoUtil.ComputeHash(contentBytes, 0, contentBytes.Length);
                String hashStr = CryptoUtil.ToHex(hashByte, true);
                request.Headers.Add("X-Amz-Content-SHA256", hashStr);
            }
            AmazonUtils.AddAuthorization(request, openParams.UserName, openParams.Password, openParams.SignatureVersion, openParams.Region);
            using (Stream uploader = request.GetRequestStream())
            {
                uploader.Write(contentBytes, 0, contentBytes.Length);
            }
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        String responseXML = String.Empty;
                        using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
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

        private Boolean DeleteObject(String xSetName, String xStreamName)
        {
            Boolean result = false;
            String uri = BuildURL(xSetName, xStreamName);
            AmazonRequest request = GetAmazonRequst(uri);
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
                        String message = GetStringFromResponse(response);
                        logger.Warn("Delete object {0} failed, message : {1}", xStreamName, message);
                    }
                }
            }
            catch (PathNotFoundException e)
            {
                logger.Warn("Cannot find the object, maybe it was deleted successfully before. Details : {0}", e.Message);
                result = true;
            }
            catch (Exception e)
            {
                logger.Error("Delete object failed, object : {0}, container : {1}. Details : {2}", xStreamName, xSetName, e);
                throw;
            }
            return result;
        }

        public override CloudFileInfo GetObjectInfo(String xSetName, String xStreamName)
        {
            CloudFileInfo result = new CloudFileInfo();
            try
            {
                String uri = BuildURL(xSetName, xStreamName);
                AmazonRequest request = GetAmazonRequst(uri);
                request.Method = RESTCommands.GET;
                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result.FileSize = response.ContentLength;
                    }
                    else
                    {
                        String message = GetStringFromResponse(response);
                        logger.Warn("Get object {0} info failed, message : {1} ", xStreamName, message);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Get object size failed, object : {0} , container : {1} . Details : {2}", xStreamName, xSetName, e);
                throw;
            }
            return result;
        }

        public override Stream OpenObject(String container, String objectName, Int32[] lengths, FileMode mode)
        {
            String uri = BuildURL(container, objectName);
            AmazonRequest request = GetAmazonRequst(uri);
            switch (mode)
            {
                case FileMode.Open:
                    request.Method = RESTCommands.GET;
                    if (lengths != null && lengths.Length == 3)
                    {
                        Int32 rangFrom = lengths[1];
                        Int32 rangTo = lengths[2];
                        if (rangFrom >= 0 && rangTo >= 0 && rangFrom < rangTo)
                        {
                            String range = "bytes=" + rangFrom + "-" + rangTo;
                            request.Headers.Add("Range", range);
                        }
                    }
                    HttpWebResponse response = DoExecute(request);
                    return new HttpDownloadStream(response);
                case FileMode.Create:
                case FileMode.CreateNew:
                    request.Method = RESTCommands.PUT;
                    request.Headers.Add("Content-Type", "DocAve/data".ToLower(CultureInfo.InvariantCulture));
                    Int32 dataLength = lengths[0];
                    request.Headers.Add("Content-Length", dataLength.ToString());
                    request.Headers.Add("Expect", "100-Continue");
                    return HttpClient.GetWebRequestForUpLoad(request).GetRequestStream();
                default:
                    break;
            }
            return null;
        }

        public override Stream OpenObject(String xSetName, String xStreamName, Int32 rangFrom, Int32 rangeTo)
        {
            String uri = BuildURL(xSetName, xStreamName);
            AmazonRequest request = GetAmazonRequst(uri);
            request.Method = RESTCommands.GET;
            if (rangFrom >= 0 && rangeTo >= 0 && rangFrom < rangeTo)
            {
                String range = "bytes=" + rangFrom + "-" + rangeTo;
                request.Headers.Add("Range", range);
            }
            try
            {
                HttpWebResponse response = DoExecute(request);
                return new HttpDownloadStream(response);
            }
            catch (Exception e)
            {
                logger.Error("Open object failed, object : {0}, container : {1}. Details : {2}", xStreamName, xSetName, e);
                throw;
            }
        }
        #endregion

        #region -- Bucket Related --

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "frankfurt")]
        String GetBucketName()
        {
            if (openParams == null || openParams.Region == null)
            {
                return String.Empty;
            }
            var region = new StringBuilder();
            switch (openParams.Region)
            {
                case AmazonConstants.US:
                    region.Append(StorageUrl.AmazonBucket_US);
                    break;
                case AmazonConstants.EU:
                    region = region.Append(StorageUrl.AmazonBucket_EU);
                    break;
                case AmazonConstants.US_WEST:
                    region = region.Append(StorageUrl.AmazonBucket_US_West);
                    break;
                case AmazonConstants.APAC:
                    region = region.Append(StorageUrl.AmazonBucket_APAC);
                    break;
                case AmazonConstants.TOKYO:
                    region = region.Append(StorageUrl.AmazonBucket_Tokyo);
                    break;
                case AmazonConstants.EU_Frankfurt:
                    region = region.Append(StorageUrl.AmazonBucket_Frankfurt);
                    break;
                default:
                    break;
            }
            region.Append(".").Append(openParams.UserName.ToLower(CultureInfo.InvariantCulture));
            return region.ToString();
        }

        #endregion

        #region -- Amazon Special Methods --
        protected List<String> ExtractMessageWithNP(String xml, String xPath)
        {
            List<String> result = null;
            Dictionary<String, String> dictionary = new Dictionary<String, String>();
            dictionary.Add("amazon", "http://s3.amazonaws.com/doc/2006-03-01/");
            result = AnalyzeXML(xml, xPath, dictionary);
            return result;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "X-Amz-Content-SHA")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "frankfurt")]
        protected void AddLocationConstraint(AmazonRequest request)
        {
            String regionStr = "";
            switch (openParams.Region)
            {
                case AmazonConstants.US:
                    request.Headers.Add("Content-Length", "0");
                    return;
                case AmazonConstants.US_WEST:
                    regionStr = AmazonConstants.REGION_US_WEST;
                    break;
                case AmazonConstants.OREGON:
                    regionStr = AmazonConstants.REGION_OREGON;
                    break;
                case AmazonConstants.EU:
                    regionStr = AmazonConstants.REGION_EU;
                    break;
                case AmazonConstants.APAC:
                    regionStr = AmazonConstants.REGION_APAC;
                    break;
                case AmazonConstants.SYDNEY:
                    regionStr = AmazonConstants.REGION_SYDNEY;
                    break;
                case AmazonConstants.TOKYO:
                    regionStr = AmazonConstants.REGION_TOKYO;
                    break;
                case AmazonConstants.SAO_PAULO:
                    regionStr = AmazonConstants.REGION_SAOPAULO;
                    break;
                case AmazonConstants.EU_Frankfurt:
                    regionStr = AmazonConstants.REGION_FRANKFURT;
                    break;
                default:
                    break;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("<CreateBucketConfiguration><LocationConstraint>")
                   .Append(regionStr)
                   .Append("</LocationConstraint></CreateBucketConfiguration>");
            Stream stream = new MemoryStream();
            Byte[] buffer = Encoding.UTF8.GetBytes(builder.ToString());
            stream.Write(buffer, 0, buffer.Length);
            request.DataStream = stream;
            stream.Position = 0;
            request.Headers.Add("Content-Length", stream.Length.ToString());
            if (openParams.SignatureVersion == 4)
            {
                Byte[] hashByte = CryptoUtil.ComputeHash(buffer, 0, buffer.Length);
                String hashStr = CryptoUtil.ToHex(hashByte, true);
                request.Headers.Add("X-Amz-Content-SHA256", hashStr);
            }
        }

        protected Boolean IsRequestTimeTooSkewed(HttpWebResponse response)
        {
            Boolean result = false;
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                String message = GetStringFromResponse(response);
                logger.Warn("Message : " + message);
                List<String> codes = AnalyzeXML(message, "Error/Code");
                result = codes.Contains("RequestTimeTooSkewed");
            }
            return result;
        }

        protected TimeSpan GetTimeOffset(HttpWebResponse response)
        {
            String serverDate = response.Headers.GetValues("Date")[0];
            return DateTime.Parse(serverDate) - DateTime.Now;
        }
        #endregion

        #region -- Build URL --
        /* Buckets with names containing uppercase characters are not accessible using the virtual hosted-style request.
         * 因此我们要判断一下，如果bucketName中包含大写字母则使用path-style
         */

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "frankfurt")]
        private String GetEndPointByRegion()
        {
            String result = String.Empty;
            switch (openParams.Region)
            {
                case AmazonConstants.US:
                    result = StorageUrl.AmazonHostName;
                    break;
                case AmazonConstants.US_WEST:
                    result = StorageUrl.AmazonCaliforniaHostName;
                    break;
                case AmazonConstants.OREGON:
                    result = StorageUrl.AmazonOregonHostName;
                    break;
                case AmazonConstants.EU:
                    result = StorageUrl.AmazonIrelandHostName;
                    break;
                case AmazonConstants.APAC:
                    result = StorageUrl.AmazonSingaporeHostName;
                    break;
                case AmazonConstants.SYDNEY:
                    result = StorageUrl.AmazonSydneyHostName;
                    break;
                case AmazonConstants.TOKYO:
                    result = StorageUrl.AmazonTokyoHostName;
                    break;
                case AmazonConstants.SAO_PAULO:
                    result = StorageUrl.AmazonSaopauloHostName;
                    break;
                case AmazonConstants.EU_Frankfurt:
                    result = StorageUrl.AmazonFrankfurtHostName;
                    break;
                default:
                    if (!String.IsNullOrEmpty(openParams.CustomizedRegion))
                    {
                        result = openParams.CustomizedRegion;
                    }
                    break;
            }
            return result;
        }

        protected String BuildURL(String bucketName, String objectName = null)
        {
            var result = default(String);
            if (objectName == null)
            {
                Regex regexAZ = new Regex("[A-Z]+");
                Regex regexaz = new Regex("[a-z]+");
                if (bucketName != null && regexAZ.IsMatch(bucketName) && regexaz.IsMatch(bucketName))
                {
                    result = this.Protocol + "://" + GetEndPointByRegion() + "/" + Encode(bucketName);
                }
                else
                    result = this.Protocol + "://" + Encode(bucketName.ToLower(CultureInfo.InvariantCulture)) + "." + GetEndPointByRegion();
            }
            else
            {
                objectName = objectName.Contains("\\") ? objectName.Replace("\\", "/") : Encode(objectName);
                result = BuildURL(bucketName) + "/" + objectName;
            }
            return result;
        }


        #endregion

        #region -- AbstractRESTOprationExecutor Members --
        protected override Boolean SpecialRetryCondition(BasicRequest request, HttpWebResponse response)
        {
            Boolean result = false;
            if (response.StatusCode == HttpStatusCode.TemporaryRedirect)
            {
                request.URI = response.ResponseUri.ToString();
                if (request.DataStream != null)
                {
                    request.DataStream.Position = 0;
                }
                result = true;
            }
            else if (IsRequestTimeTooSkewed(response))
            {
                TimeSpan timeOffset = GetTimeOffset(response);
                request.Headers[AmazonConstants.AWS3_ALTERNATIVE_DATE] = AmazonUtils.GetReqeustDate(timeOffset);
                HttpClient.TimeOffset = timeOffset;
                result = true;
            }
            return result;
        }
        #endregion

        protected AmazonRequest GetAmazonRequst(String uri)
        {
            AmazonRequest request = new AmazonRequest();
            Dictionary<String, String> headers = new Dictionary<String, String>();
            headers.Add(AmazonConstants.AWS3_ALTERNATIVE_DATE, AmazonUtils.GetReqeustDate(HttpClient.TimeOffset));
            request.URI = uri;
            request.UserName = openParams.UserName;
            request.Password = openParams.Password;
            request.Headers = headers;
            return request;
        }

        public override String GetDocAveDefaultContainer()
        {
            return GetBucketName();
        }

        public override String BuildObjectAbsoluteURL(String container, String objectName)
        {
            return BuildURL(container, objectName);
        }

        public override String BuildURLWithOutQueryParams(String container)
        {
            return BuildURL(container);
        }

        public override Dictionary<String, String> Headers
        {
            get
            {
                String date = AmazonUtils.GetReqeustDate(HttpClient.TimeOffset);
                return new Dictionary<String, String>() { 
                    { AmazonConstants.AWS3_ALTERNATIVE_DATE,  date},
                };
            }
        }

        public override Dictionary<String, String> ListDirectoryHeaders
        {
            get
            {
                Dictionary<String, String> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<String, String> OpenDirectoryWriteModeHeaders
        {
            get
            {
                Dictionary<String, String> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<String, String> OpenFileWriteModeHeaders
        {
            get
            {
                Dictionary<String, String> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<String, String> ListDirectoryQueryParams
        {
            get { return new Dictionary<String, String>() { { "max-keys".ToLower(CultureInfo.InvariantCulture), "1000" } }; }
        }

        public override Dictionary<String, String> OpenFileReadModeHeaders
        {
            get
            {
                Dictionary<String, String> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<String, String> OpenDirectoryReadModeHeaders
        {
            get
            {
                Dictionary<String, String> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<String, String> OpenStreamReadModeHeaders
        {
            get
            {
                Dictionary<String, String> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<String, String> OpenStreamWriteModeHeaders
        {
            get
            {
                Dictionary<String, String> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<String, String> ListObjectHeaders
        {
            get
            {
                Dictionary<String, String> headers = Headers;
                return headers;
            }
        }

        public override Dictionary<String, String> CopyFileHeaders
        {
            get
            {
                Dictionary<String, String> headers = Headers;
                return headers;
            }
        }

        public override HttpUploadStream OpenObjectForWrite(String fullURL, Dictionary<String, String> headers)
        {
            long length = Convert.ToInt64(headers["Content-Length"]);
            if (length > openParams.BlockLength * 1024 * 1024)
            {
                return new AmazonMultipartUploadStream(this, fullURL, headers);
            }
            else
            {
                HttpWebRequest request = HttpClient.CreateRequestPut(fullURL, null);
                HttpClient.AddHeaders(request, headers);
                return new AmazonUploadStream(request, openParams) { HttpClient = this.HttpClient, System = this.HttpClient.CurrentSystem };
            }
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
            fullURL = fullURL + "?uploads";
            HttpWebRequest request = HttpClient.CreateRequestPost(fullURL, null);
            request.ContentLength = 0;
            request.Headers.Add(AmazonConstants.AWS3_REST_HEADER_PREFIX + "storage-class", "STANDARD");
            request.ContentType = "application/octet-stream";
            request.ServicePoint.Expect100Continue = false;
            String uploadId = null;
            using (HttpWebResponse response = DoExecute(request, headers))
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
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "X-Amz-Content-SHA")]
        public String UploadPart(String fullURL, Byte[] buffer, Int32 offset, Int32 count)
        {
            return RetryRequset.CloudRetry<String>(delegate()
            {
                HttpWebRequest request = HttpClient.CreateRequestPut(fullURL, null);
                request.ContentLength = count;
                HttpClient.AddHeaders(request, this.Headers);
                request.AllowWriteStreamBuffering = false;
                request.AllowAutoRedirect = false;
                request.Timeout = 0x7ffffffe; //never timeout
                if (openParams.SignatureVersion == 4)
                {
                    Byte[] hashByte = CryptoUtil.ComputeHash(buffer, offset, count);
                    String hashStr = CryptoUtil.ToHex(hashByte, true);
                    request.Headers.Add("X-Amz-Content-SHA256", hashStr);
                }
                AmazonUtils.AddAuthorization(request, openParams.UserName, openParams.Password, openParams.SignatureVersion, openParams.Region);
                using (Stream upStream = request.GetRequestStream())
                {
                    upStream.Write(buffer, 0, count);
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(String.Format("Upload Part failed, url='{0}' HttpStatusCode={1}", response.ResponseUri, response.StatusCode));
                    }
                    String eTag = response.Headers["ETag"];
                    logger.Debug("Upload Part succeed, eTag={0} url='{1}'", eTag, fullURL);
                    return eTag;
                }
            });
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "X-Amz-Content-SHA")]
        public Boolean CompleteMultipartUpload(String fullURL, Dictionary<int, String> eTags)
        {
            return RetryRequset.CloudRetry<Boolean>(delegate()
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
                if (openParams.SignatureVersion == 4)
                {
                    Byte[] hashByte = CryptoUtil.ComputeHash(xmlDataBuffer, 0, xmlDataBuffer.Length);
                    String hashStr = CryptoUtil.ToHex(hashByte, true);
                    request.Headers["X-Amz-Content-SHA256"] = hashStr;
                }
                AmazonUtils.AddAuthorization(request, openParams.UserName, openParams.Password, openParams.SignatureVersion, openParams.Region);
                using (Stream upStream = request.GetRequestStream())
                {
                    upStream.Write(xmlDataBuffer, 0, xmlDataBuffer.Length);
                }
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(String.Format("Complete Multipart Upload failed, url='{0}' HttpStatusCode={1}", response.ResponseUri, response.StatusCode));
                    }
                    logger.Debug("Complete Multipart Upload succeed, url='{0}'", fullURL);
                    return true;
                }
            });
        }

        public override String BuildObjectAbsoluteURL(String url, String container, String objectName)
        {
            throw new NotSupportedException();
        }

        public override String ListAzureMetaName(String baseURL, Dictionary<String, String> queryParams, Dictionary<String, String> headers)
        {
            throw new NotSupportedException();
        }

        public override CloudOpenParameter ConveryParams(Dictionary<String, String> prams)
        {
            throw new NotSupportedException();
        }

        public override long GetContainerSize(String xSetName)
        {
            throw new NotSupportedException();
        }

        public override SpaceInfo GetUserAccountInfo()
        {
            throw new NotSupportedException();
        }

        public override List<XDirectoryInfo> Parse2Directory(String responseXmlString, String path)
        {
            throw new NotSupportedException();
        }

        public override List<XFileInfo> Parse2File(String responseXmlString)
        {
            throw new NotSupportedException();
        }

        public override String GetFinalUrl(StorageInfo info)
        {
            throw new NotSupportedException();
        }
    }
}
