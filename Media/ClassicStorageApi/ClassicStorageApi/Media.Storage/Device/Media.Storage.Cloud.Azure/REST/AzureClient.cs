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




using System.Diagnostics.CodeAnalysis;
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.Azure.REST.AzureClient.#GetCdnEnabledString(System.String)", MessageId = "vo")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.Azure.REST.AzureClient.#GetCdnEnabledString(System.String)", MessageId = "msecnd")]
namespace AvePoint.Media.ClassicStorage.Cloud.Azure.REST
{
    #region using directives
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Utility.Cryptography;
    using AvePoint.Media.ClassicStorage.Cloud.Azure.BigDBContext;
    using AvePoint.Media.ClassicStorage.Cloud.Common;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Client;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Config;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Request;
    using AvePoint.Media.ClassicStorage.Util;
    using AvePoint.Media.StorageApi;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Xml.XPath;
    #endregion

    #region CodeReview
    [AveCodeReview(
       "2012/8/9",
       "rongbiao.sun@avepoint.com",
       "dapeng.zhang@avepoint.com",
        new string[] { CodeReviewConstants.CHECK_LIST_ID_LOG_1 },
        null,
        true)]
    #endregion
    public class AzureClient : AbstractRESTOprationExecutor, ICloudOprationExecutor
    {
        private AzureOpenParameter openParams;
        public AzureOpenParameter OpenParams
        {
            set { this.openParams = value; }
            get { return this.openParams; }
        }
        private List<string> blockIds;

        public AzureClient()
        {
            HttpClient = new MSAzureHttpClient();
        }

        //在每一种Cloud中重写这个方法就是为了把openParams.FlushDNS这个参数默认false，这个属性Cloud中不会用到
        public override void InitRetry(CloudOpenParameter openParams)
        {
            //Logger.Info("Init Retry: retryCount " + openParams.MaxRetryCount + ",RetryInterval " + openParams.RetryInterval);
            RetryRequset = new Retry(openParams.MaxRetryCount, openParams.RetryInterval, openParams.NeedRetry, true);
        }

        #region ICloudOprationExecutor Members

        public override void InitConfig(CloudOpenParameter prams)
        {
            openParams = prams as AzureOpenParameter;
            HttpClient.OpenParam = openParams;
            this.CloudOpenParam = openParams;
            InitRetry(prams);
        }

        public override bool CheckContainer(string xSetName)
        {
            bool result = false;

            try
            {
                string fullURL = BuildURL(xSetName, "/");
                result = CheckObject(fullURL, null, null);
            }
            catch (PathNotFoundException ex)
            {
                Trace.TraceWarning(ex.Message);
                result = false;
            }
            catch (Exception e)
            {
                Logger.Error(GetCheckCtnErrorMsg(xSetName), e);
                throw;
            }

            return result;
        }

        public override bool CreateContainer(string xSetName)
        {
            bool result = false;

            try
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add(MSAzureConstants.RESTYPE, "container");
                string url = BuildURL(xSetName, parameters);

                MSAzureRequest request = GetAzureRequest(url);
                request.Method = RESTCommands.PUT;
                request.Headers.Add("Content-Length", "0");
                request.Headers.Add("x-ms-meta-name", Encode(xSetName));

                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.Created)
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
                List<string> objectsNames = ListObject(xSetName);
                foreach (string objectsName in objectsNames)
                {
                    DeleteObject(xSetName, objectsName);
                }
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add(MSAzureConstants.RESTYPE, "container");
                string url = BuildURL(xSetName, parameters);
                MSAzureRequest request = GetAzureRequest(url);
                request.Method = RESTCommands.DELETE;

                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(GetDeleteCtnErrorMsg(xSetName), e);
                throw;
            }
            return result;
        }

        protected override List<string> ListXstream(string xSetName, string prefix, int limit, string marker, bool isGetName)
        {
            var result = new List<string>();
            MSAzureRequest request = null;
            try
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add(MSAzureConstants.RESTYPE, "container");
                parameters.Add("comp", "list");
                if (!string.IsNullOrEmpty(prefix))
                {
                    if (prefix.Contains("\\"))
                    {
                        prefix = prefix.Replace("\\", "/");
                    }
                    parameters.Add(MSAzureConstants.Prefix, prefix);
                }
                if (!string.IsNullOrEmpty(marker))
                {
                    parameters.Add(MSAzureConstants.Marker, marker);
                }
                if (limit > 0)
                {
                    parameters.Add(MSAzureConstants.MaxResults, limit.ToString());
                }

                string url = BuildURL(xSetName, parameters);
                request = GetAzureRequest(url);
                request.Method = RESTCommands.GET;

                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                    {
                        var responseStream = resp.GetResponseStream();
                        if (responseStream != null)
                        {
                            using (var streamReader = new StreamReader(responseStream))
                            {
                                var responseXml = streamReader.ReadToEnd();
                                var selectPath = isGetName ? "EnumerationResults/Blobs/Blob/Name" : "EnumerationResults/Blobs/Blob/Properties/Content-Length";
                                this.FirstStepAnalyzeXML(responseXml, selectPath).ForEach(navigator => result.Add(navigator.Value));
                                this.FirstStepAnalyzeXML(responseXml, "EnumerationResults/NextMarker").ForEach(navigator => result.Add(navigator.Value));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(GetListObjErrorMsg(xSetName), e);
                throw;
            }

            return result;
        }

        /*
         *  <?xml version=\"1.0\" encoding=\"utf-8\"?>
            <EnumerationResults ContainerName=\"http://devteststorage.blob.core.windows.net/archive-1c0cc5d49a2a6f54d7c9f679756cedd6\">
	            <Blobs>
		            <Blob>
			            <Name>content0_0.dat</Name>
			            <Url>http://devteststorage.blob.core.windows.net/archive-1c0cc5d49a2a6f54d7c9f679756cedd6/content0_0.dat</Url>
			            <Properties>
				            <Last-Modified>Mon, 16 Aug 2010 01:59:33 GMT</Last-Modified>
				            <Etag>0x8CD0AFDCE81325E</Etag>
				            <Content-Length>25417</Content-Length>
				            <Content-Type>docave/content-data</Content-Type>
				            <Content-Encoding />
				            <Content-Language />
				            <Content-MD5 />
				            <Cache-Control />
				            <BlobType>BlockBlob</BlobType>
				            <LeaseStatus>unlocked</LeaseStatus>
			            </Properties>
		            </Blob>
	            </Blobs>
                <NextMarker />
            </EnumerationResults>
         */
        public override List<string> ListObject(string xSetName)
        {
            return ListObject(xSetName, null);
        }

        public override List<string> ListObject(string xSetName, string prefix)
        {
            return ListObjectWithPreFix(xSetName, prefix, true);
        }

        public override long GetContainerSize(string xSetName)
        {
            return GetXsetSize(xSetName, null);
        }

        public override bool CheckObject(string xSetName, string xStreamName)
        {
            bool result = false;
            try
            {
                if (!string.IsNullOrEmpty(xStreamName) && !xStreamName.EndsWith("/", StringComparison.CurrentCultureIgnoreCase))
                {
                    string fullURL = BuildURL(xSetName, xStreamName);
                    result = CheckObject(fullURL, null, null);
                }
                else if (!string.IsNullOrEmpty(xStreamName) && "/".Equals(xStreamName, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = CheckContainer(xSetName);
                }
                else
                {
                    string urlWithoutQueryParms = BuildURLWithOutQueryParams(xSetName);
                    Dictionary<string, string> queryParams = new Dictionary<string, string>();
                    queryParams.Add("prefix", xStreamName);
                    queryParams.Add("delimiter", "/");
                    queryParams.Add("comp", "list");
                    queryParams.Add("restype", "container");
                    string finalURL = HttpClient.CombiningQueryParams(urlWithoutQueryParms, queryParams);
                    MSAzureRequest request = GetAzureRequest(finalURL);
                    request.Method = RESTCommands.GET;

                    using (HttpWebResponse resp = DoExecute(request))
                    {
                        if (resp != null
                            && resp.StatusCode == HttpStatusCode.OK
                            && !xStreamName.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                        {
                            result = true;
                        }
                        else
                        {
                            result = HasRespContentCounts(resp);
                        }
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
                Logger.Error("Check object failed, object : " + xStreamName + ", container : " + xSetName, e);
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
                    List<XPathNavigator> navs = FirstStepAnalyzeXML(respInfo.ResponseXml, "EnumerationResults/Blobs/Blob");
                    navs.AddRange(FirstStepAnalyzeXML(respInfo.ResponseXml, "EnumerationResults/Blobs/BlobPrefix"));
                    if (navs.Count > 0)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        private string GetCdnEnabledString(string fullURL)
        {
            string cdnUrl = string.Format("http://{0}.vo.msecnd.net", openParams.CdnGuid);
            Uri uri = new Uri(openParams.AccessPoint);
            fullURL = fullURL.Replace(uri.Scheme + "://" + openParams.UserName + ".blob.core.windows.net", cdnUrl);
            return fullURL;
        }

        /// <summary>
        /// AzureBigDBTest
        /// </summary>
        /// <param name="xSetName"></param>
        /// <param name="xStreamName"></param>
        /// <param name="mimeType"></param>
        /// <param name="webRequest"></param>
        /// <param name="blockNumber"></param>
        /// <param name="dataLength"></param>
        /// <returns></returns>
        public override HttpUploadStream OpenObjectForWrite(string fullURL, Dictionary<string, string> headers)
        {
            DBContext db = new DBContext(this, headers, HttpClient, fullURL);

            //HttpWebRequest request = db.CreateRequestPut(fullURL, null);
            //db.CombiningRequestWithHeaders(request, headers);
            return db.GetHttpUploadStream(null);

        }

        public override HttpDownloadStream OpenObjectForRead(string fullURL, Dictionary<string, string> headers)
        {
            if (!string.IsNullOrEmpty(openParams.CdnGuid) && openParams.CdnEnaled)
            {
                fullURL = GetCdnEnabledString(fullURL);
            }
            var request = HttpClient.CreateRequestGet(fullURL, null);
            //Fix the issue of 403 error, When the timeout request occurs, and have a retry logic.
            UpdateHeaderDate(headers);
            return new HttpDownloadStream(DoExecute(request, headers)) { System = this.HttpClient.CurrentSystem };
        }

        private static void UpdateHeaderDate(Dictionary<String, String> headers)
        {
            if (headers.ContainsKey(MSAzureConstants.DateHeader))
            {
                var dateTimeString = MSAzureUtils.convertDateTimeToHttpString(DateTime.UtcNow);
                headers[MSAzureConstants.DateHeader] = dateTimeString;
            }
        }

        public override HttpWebRequest GetUploadRequest(string xSetName, string xStreamName, string mimeType, HttpWebRequest webRequest, int blockNumber, long dataLength)
        {
            try
            {
                if (webRequest != null)
                {
                    using (HttpWebResponse resp = UpLoad(webRequest))
                    {
                        if (resp.StatusCode != HttpStatusCode.Created)
                        {
                            throw new Exception("Put block failed, ");
                        }
                    }
                }
                else
                {
                    blockIds = new List<string>();
                }
                if (dataLength > MSAzureConstants.MaxBlobSize)
                {
                    int contentLength = (int)(dataLength - blockNumber * MSAzureConstants.BlockSize);
                    if (contentLength >= MSAzureConstants.BlockSize)
                    {
                        contentLength = MSAzureConstants.BlockSize;
                    }
                    int blockCount = (int)Math.Ceiling((double)dataLength / MSAzureConstants.BlockSize);
                    string blockId = null;
                    Dictionary<string, string> prams = new Dictionary<string, string>();
                    prams.Add("comp", "Block");
                    blockId = MSAzureUtils.GenerateBlockId(blockNumber, blockCount);
                    blockIds.Add(blockId);
                    if (prams.ContainsKey(MSAzureConstants.BLOCK_ID))
                    {
                        prams[MSAzureConstants.BLOCK_ID] = blockId;
                    }
                    else
                    {
                        prams.Add(MSAzureConstants.BLOCK_ID, blockId);
                    }
                    MSAzureRequest request = GetAzureRequest(BuildURL(xSetName, xStreamName, prams));
                    request.Method = RESTCommands.PUT;
                    request.Headers.Add("Content-Type", mimeType);
                    request.Headers.Add("Content-Length", contentLength.ToString());
                    return HttpClient.GetWebRequestForUpLoad(request);
                }
                else
                {
                    MSAzureRequest request = GetAzureRequest(BuildURL(xSetName, xStreamName));
                    request.Method = RESTCommands.PUT;
                    request.Headers.Add("Content-Type", mimeType);
                    request.Headers.Add("x-ms-blob-type", "BlockBlob");
                    request.Headers.Add("Content-Length", dataLength.ToString());
                    return HttpClient.GetWebRequestForUpLoad(request);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw;
            }
        }

        public override Stream OpenObject(string container, string objectName, int[] lengths, FileMode mode)
        {
            string url = BuildURL(container, objectName);
            MSAzureRequest request = GetAzureRequest(url);
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
                    request.Headers.Add("Content-Type", "DOCAVE".ToLower(CultureInfo.InvariantCulture) + "\\" + "DIRECTORY".ToLower(CultureInfo.InvariantCulture));
                    request.Headers.Add("x-ms-blob-type", "BlockBlob");
                    request.Headers.Add("Content-Length", lengths[0] + "");
                    return HttpClient.GetWebRequestForUpLoad(request).GetRequestStream();
                default:
                    break;
            }

            return null;
        }

        public override bool CreateObject(string xSetName, string xStreamName, HttpWebRequest request, long dataLength)
        {
            bool result = false;

            try
            {
                if (dataLength > MSAzureConstants.MaxBlobSize)
                {
                    using (HttpWebResponse resp = UpLoad(request))
                    {
                        if (resp.StatusCode != HttpStatusCode.Created)
                        {
                            throw new Exception("Put block failed, ");
                        }
                    }
                    result = PutBlockList(xSetName, xStreamName, XConst.DOCAVE + "/data", blockIds);
                }
                else
                {
                    using (HttpWebResponse resp = UpLoad(request))
                    {
                        if (resp.StatusCode == HttpStatusCode.Created)
                        {
                            result = true;
                        }
                        else
                        {
                            Logger.Error("Create object failed. object : " + xStreamName + ",container : " + xSetName + ", statues:" + resp.StatusCode);
                            throw new Exception("Create object failed. object : " + xStreamName + ",container : " + xSetName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                blockIds = null;
            }

            return result;
        }

        public override Stream OpenObject(string xSetName, string xStreamName, int rangFrom, int rangeTo)
        {
            try
            {
                string uri = BuildCDNURL(xSetName, xStreamName);
                MSAzureRequest request = GetAzureRequest(uri);
                request.Method = RESTCommands.GET;
                if (rangFrom >= 0 && rangeTo >= 0 && rangFrom < rangeTo)
                {
                    string range = "bytes=" + rangFrom + "-" + rangeTo;
                    request.Headers.Add("Range", range);
                }

                HttpWebResponse response = DoExecute(request);
                return new HttpDownloadStream(response) { System = this.HttpClient.CurrentSystem };
            }
            catch (Exception e)
            {
                Logger.Error(GetOpenObjErrorMsg(xStreamName, xSetName), e);
                throw;
            }
        }

        private string nameSub(string tempName)
        {
            if (tempName.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                //  string nameTemp = tempName.Substring(0, tempName.LastIndexOf('/'));
                //  string nameTemp2 = nameTemp.Substring(nameTemp.LastIndexOf('/'), nameTemp.Length - nameTemp.LastIndexOf('/'));
                return "";
            }
            else
            {
                return tempName.Substring(tempName.LastIndexOf('/') + 1, tempName.Length - tempName.LastIndexOf('/') - 1);
            }

        }

        public override bool DeleteObject(string xSetName, string xStreamName, bool isDeleteSubFile)
        {
            bool result = false;
            if (isDeleteSubFile)
            {
                List<string> subFiles = ListObject(xSetName, xStreamName);
                foreach (string name in subFiles)
                {
                    string tempName = nameSub(name);
                    result = DeleteObject(xSetName, PathUtil.CombinePath(xStreamName, tempName));
                    if (!result)
                    {
                        return false;
                    }
                }
            }
            result = DeleteObject(xSetName, xStreamName);
            return result;
        }


        private bool DeleteObject(string xSetName, string xStreamName)
        {
            bool result = false;

            try
            {
                if (!CheckObject(xSetName, xStreamName))
                {
                    return true;
                }

                string uri = BuildURL(xSetName, xStreamName);
                MSAzureRequest request = GetAzureRequest(uri);
                request.Method = RESTCommands.DELETE;
                request.Headers.Add("x-ms-delete-snapshots", "include");

                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp.StatusCode == HttpStatusCode.Accepted || resp.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
                }
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
                MSAzureRequest request = GetAzureRequest(BuildURL(xSetName, xStreamName));
                request.Method = RESTCommands.HEAD;
                using (HttpWebResponse response = DoExecute(request))
                {
                    if (HttpStatusCode.OK == response.StatusCode)
                    {
                        result.FileSize = response.ContentLength;
                        if (!String.IsNullOrEmpty(response.Headers["Last-Modified"]))
                        {
                            result.LastWriteTimeUtc = DateTime.Parse(response.Headers["Last-Modified"]).ToUniversalTime();
                        }
                        if (!String.IsNullOrEmpty(response.Headers[MSAzureConstants.AccessTierHeader]))
                        {
                            var accessTierHeaderValue = response.Headers[MSAzureConstants.AccessTierHeader].Trim();
                            AccessTierType fileTierType;
                            Enum.TryParse(accessTierHeaderValue, out fileTierType);
                            result.FileTierType = fileTierType;
                        }
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
        #endregion

        #region Assistant Method

        protected MSAzureRequest GetAzureRequest(string uri)
        {
            MSAzureRequest result = new MSAzureRequest();
            result.URI = uri;
            result.UserName = openParams.UserName;
            result.Password = openParams.Password;
            result.Headers = this.Headers;
            return result;
        }

        protected bool PutBlob(string xSetName, string xStreamName, string mimeType, Stream stream)
        {
            bool result = false;

            try
            {
                string url = BuildURL(xSetName, xStreamName);
                MSAzureRequest request = GetAzureRequest(url);
                request.Method = RESTCommands.PUT;
                request.DataStream = stream;
                request.Headers.Add("Content-Length", (stream.Length - stream.Position).ToString());
                request.Headers.Add("Content-Type", mimeType);
                request.Headers.Add("x-ms-blob-type", "BlockBlob");

                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception t)
            {
                Logger.Error(t.Message, t);
                throw;
            }

            return result;
        }

        protected bool CreateBlob(string xSetName, string xStreamName, string mimeType, Stream stream)
        {
            bool result = false;

            long size = stream.Length - stream.Position;
            int blockCount = (int)Math.Ceiling((double)size / MSAzureConstants.BlockSize);

            string blockId = null;
            List<string> blockIds = new List<string>();
            Dictionary<string, string> prams = new Dictionary<string, string>();
            prams.Add("comp", "Block");
            for (int i = 0; i < blockCount; i++)
            {
                blockId = MSAzureUtils.GenerateBlockId(i, blockCount);
                blockIds.Add(blockId);
                if (prams.ContainsKey(MSAzureConstants.BLOCK_ID))
                {
                    prams[MSAzureConstants.BLOCK_ID] = blockId;
                }
                else
                {
                    prams.Add(MSAzureConstants.BLOCK_ID, blockId);
                }

                using (Stream segmentStream = MSAzureUtils.GetSegmentStream(stream, MSAzureConstants.BlockSize))
                {
                    PutBlock(xSetName, xStreamName, mimeType, segmentStream, prams);
                }
            }

            PutBlockList(xSetName, xStreamName, mimeType, blockIds);

            return result;
        }

        protected void PutBlock(string xSetName, string xStreamName, string mimeType, Stream stream, Dictionary<string, string> prams)
        {
            try
            {
                String url = BuildURL(xSetName, xStreamName, prams);

                MSAzureRequest request = GetAzureRequest(url);
                request.Method = RESTCommands.PUT;
                request.DataStream = stream;
                request.Headers.Add("Content-Length", (stream.Length - stream.Position).ToString());
                request.Headers.Add("Content-Type", mimeType);

                using (HttpWebResponse resp = DoExecute(request))
                {
                    if (resp.StatusCode != HttpStatusCode.Created)
                    {
                        throw new Exception("Put block failed, ");
                    }
                }
            }
            catch (Exception t)
            {
                Logger.Error(t.Message, t);
                throw;
            }
        }

        protected bool PutBlockList(string xSetName, string xStreamName, string mimeType, List<string> blockIds)
        {
            bool result = false;
            try
            {
                Dictionary<string, string> prams = new Dictionary<string, string>();
                prams.Add("comp", "BlockList".ToLower(CultureInfo.InvariantCulture));
                MSAzureRequest request = GetAzureRequest(BuildURL(xSetName, xStreamName, prams));
                request.Method = RESTCommands.PUT;

                using (Stream stream = MSAzureUtils.BuildBlockListXml(blockIds))
                {
                    request.DataStream = stream;
                    request.Headers.Add("Content-Length", (stream.Length - stream.Position).ToString());
                    request.Headers.Add("Content-Type", mimeType);

                    using (HttpWebResponse resp = DoExecute(request))
                    {
                        if (resp.StatusCode == HttpStatusCode.Created)
                        {
                            result = true;
                        }
                        else
                        {
                            Logger.Error("Create object failed. object : " + xStreamName + ",container : " + xSetName + ", statues:" + resp.StatusCode);
                            throw new Exception("Create object failed. object : " + xStreamName + ",container : " + xSetName);
                        }
                    }
                }
            }
            catch (Exception t)
            {
                Logger.Error(t.Message, t);
                throw;
            }
            return result;
        }

        #endregion

        #region Build URL

        public static string GetDataTypeByName(string name)
        {
            if (name.StartsWith("Farm", StringComparison.OrdinalIgnoreCase))
            {
                return "granular-backup";
            }
            else if (name.StartsWith("data_archive", StringComparison.OrdinalIgnoreCase))
            {
                return "archive";
            }
            else if (name.StartsWith("data_realtime_archive", StringComparison.OrdinalIgnoreCase))
            {
                return "realtime-archive";
            }
            else if (name.StartsWith("data_compliance_archive", StringComparison.OrdinalIgnoreCase))
            {
                return "compliance-archive";
            }
            else if (name.StartsWith("data_hold_backup", StringComparison.OrdinalIgnoreCase))
            {
                return "hold-backup";
            }
            return null;
        }

        private string GetDataType(string xsetName)
        {
            string dataType = GetDataTypeByName(xsetName);
            if (dataType != null)
            {
                return dataType + "-";
            }
            return "";
        }

        protected virtual string BuildURL(string xSetName)
        {
            string tempStr = xSetName;

            if (xSetName.Contains("\\"))
            {
                xSetName = xSetName.Replace("\\", "/");
            }

            if (openParams.AccessPoint.Equals("http://blob.core.windows.net", StringComparison.CurrentCultureIgnoreCase) || openParams.AccessPoint.Equals("https://blob.core.windows.net", StringComparison.CurrentCultureIgnoreCase))
            {
                Uri uri = new Uri(openParams.AccessPoint);
                if (Data_Version == Data_Version.DocAve5)
                {
                    xSetName = GetMD5(xSetName);
                    return uri.Scheme + "://" + openParams.UserName + ".blob.core.windows.net/" + GetDataType(tempStr) + xSetName;
                    //Data_Version = 0;
                }
                else
                {
                    return uri.Scheme + "://" + openParams.UserName + ".blob.core.windows.net/" + openParams.SystemLocation;
                }
            }
            else
            {
                if (Data_Version == Data_Version.DocAve5)
                {
                    xSetName = GetMD5(xSetName);
                    Data_Version = 0;
                    return openParams.AccessPoint.TrimEnd('/') + "/" + openParams.UserName + "/" + GetDataType(tempStr) + xSetName;
                }
                else
                {
                    return openParams.AccessPoint.TrimEnd('/') + "/" + openParams.SystemLocation;
                }
            }
        }

        protected string BuildURL(string xSetName, string xStreamName)
        {

            if ("/".Equals(xStreamName, StringComparison.OrdinalIgnoreCase))
            {
                xStreamName = string.Empty;
                return BuildURL(xSetName) + "?restype=container";
            }
            else
            {
                return BuildURL(xSetName) + "/" + Encode(xStreamName);
            }
        }

        protected string BuildURL(Dictionary<string, string> prams)
        {
            if (prams == null)
            {
                return openParams.AccessPoint;
            }
            return openParams.AccessPoint + ConvertQueryList2String(prams);
        }

        protected string BuildURL(string xSetName, string xStreamName, Dictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return BuildURL(xSetName, xStreamName);
            }

            return BuildURL(xSetName, xStreamName) + ConvertQueryList2String(parameters);
        }

        protected string BuildURL(string xSetName, Dictionary<string, string> prams)
        {
            if (prams == null)
            {
                return BuildURL(xSetName);
            }

            return BuildURL(xSetName) + ConvertQueryList2String(prams);
        }

        protected virtual string BuildCDNURL(string xSetName, string xStreamName)
        {
            if (xStreamName.Contains("\\"))
            {
                xStreamName.Replace("\\", "/");
            }
            if (string.IsNullOrEmpty(openParams.CdnGuid) || (!openParams.CdnEnaled))
            {
                return BuildURL(xSetName, xStreamName);
            }
            else
            {
                //return openParams.CdnUrl + "/" + GetDataType(xSetName) + Encode(GetMD5(xSetName)) + "/" + Encode(xStreamName);
                string tempUrl = BuildURL(xSetName, xStreamName);
                return GetCdnEnabledString(tempUrl);
            }
        }

        #endregion

        public override string BuildObjectAbsoluteURL(string container, string objectName)
        {
            //if (!string.IsNullOrEmpty(openParams.CdnGuid) && openParams.CdnEnaled)
            //{
            //    string tempUrl = BuildURL(container, objectName);
            //    return GetCdnEnabledString(tempUrl);
            //}
            //else
            //{
            //    return BuildURL(container, objectName);
            //}
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
                string datetime = MSAzureUtils.convertDateTimeToHttpString(DateTime.UtcNow);
                return new Dictionary<string, string>()
                { 
                    { MSAzureConstants.ApiVersionHeader, MSAzureConstants.ApiVersion },
                    {MSAzureConstants.DateHeader, datetime}
                };
            }
        }

        //internal string GetCopyResource(StorageInfo copyFrom, string storageKey)
        //{
        //    var time = DateTime.UtcNow;
        //    var version = "2014-02-14";
        //    var startTime = HttpUtility.UrlEncode(time.ToString("s")) + "Z";
        //    var expireTime = HttpUtility.UrlEncode(time.AddDays(7).ToString("s")) + "Z";
        //    var permission = "r";
        //    var signedResource = "b";
        //    var resource = "/" + PathUtil.CombinePath(openParams.UserName, PathUtil.CombinePath(OpenParams.SystemLocation, PathUtil.CombinePath(copyFrom.HighName, copyFrom.LowName))).Replace("\\", "/");
        //    var signature = HttpUtility.UrlEncode(MSAzureUtils.GetSignForSAS(resource, storageKey, time));
        //    var url = BuildURL(OpenParams.SystemLocation, PathUtil.CombinePath(copyFrom.HighName, copyFrom.LowName).TrimStart(new char[] { '\\', '/' }));
        //    var result = url.TrimEnd('/') + string.Format("?sv={0}&ss={1}&se={2}&sp={3}&sr={4}&sig={5}", version, startTime, expireTime, permission, signedResource, signature);
        //    return result;
        //}

        internal bool CopyFile(string copyResouce, string copyTo)
        {
            bool result = false;
            try
            {
                HttpWebRequest webRequest = HttpClient.CreateRequestPut(copyTo, null);
                webRequest.ContentLength = 0;
                var headers = this.Headers;
                headers.Add("x-ms-copy-source", copyResouce);
                using (HttpWebResponse response = DoExecute(webRequest, headers))
                {
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                throw;
            }
            return result;
        }

        internal bool CopyFile(string copyResouce, string copyTo, string accessTier)
        {
            bool result = false;
            try
            {
                var request = this.GetAzureRequest(copyTo);
                request.Method = RESTCommands.PUT;
                request.Headers.Add(MSAzureConstants.AccessTierHeader, accessTier);
                request.Headers["x-ms-copy-source"] = copyResouce;
                request.Headers.Add("Content-Length", "0");
                request.Headers[MSAzureConstants.ApiVersionHeader] = MSAzureConstants.ApiVersion;

                using (HttpWebResponse response = DoExecute(request))
                {
                    var copyId = response.Headers.Get("x-ms-copy-id");
                    var status = response.Headers.Get("x-ms-copy-status");
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                throw;
            }
            return result;
        }


        public override bool CheckObject(string fullURL, Dictionary<string, string> parameters, Dictionary<string, string> headers)
        {
            bool result = false;
            try
            {
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

        public override Dictionary<string, string> ListDirectoryQueryParams
        {
            get { return new Dictionary<string, string>() { { "RESTYPE".ToLower(CultureInfo.InvariantCulture), "container" }, { "comp", "list" }, { "maxResults".ToLower(CultureInfo.InvariantCulture), "5000" } }; }
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
                headers["Content-Length"] = "0";
                headers["Content-Type"] = "DOCAVE/DIRECTORY".ToLower(CultureInfo.InvariantCulture);
                headers["x-ms-blob-type"] = "BlockBlob";
                return headers;
            }
        }

        public override Dictionary<string, string> OpenStreamWriteModeHeaders
        {
            get
            {
                Dictionary<string, string> headers = Headers;
                headers["Content-Length"] = "0";
                headers["Content-Type"] = "DOCAVE/DIRECTORY".ToLower(CultureInfo.InvariantCulture);
                headers["x-ms-blob-type"] = "BlockBlob";
                return headers;
            }
        }

        public Dictionary<string, string> BigDBOpenStreamWriteModeHeaders
        {
            get
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                //headers["x-ms-blob-type"] = "PageBlob";
                headers["x-ms-blob-content-length"] = "2048";
                headers["x-ms-blob-sequence-number"] = "0";
                return headers;
            }
        }

        public override Dictionary<string, string> OpenFileWriteModeHeaders
        {
            get
            {
                Dictionary<string, string> headers = Headers;
                headers["Content-Length"] = "0";
                headers["Content-Type"] = "DOCAVE/DIRECTORY".ToLower(CultureInfo.InvariantCulture);
                headers["x-ms-blob-type"] = "BlockBlob";
                return headers;
            }
        }

        public override string ListAzureMetaName(string baseURL, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
        {
            //base.ListAzureMetaName();
            string name = string.Empty;
            HttpWebRequest requestGet = HttpClient.CreateRequestGet(baseURL, queryParams);
            using (HttpWebResponse resp = DoExecute(requestGet, headers))
            {
                name = resp.Headers["x-ms-meta-name"];
                name = Decode(name);
            }
            return name;
        }

        public bool PutBlock(string fullURL, string blockIdBase64, byte[] buffer, int offset, int count)
        {
            blockIdBase64 = Encode(blockIdBase64);
            IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.MD5);
            byte[] data = md5.ComputeHash(buffer, offset, count);
            string contentMD5 = Convert.ToBase64String(data);
            return RetryRequset.CloudRetry<bool>(delegate()
            {
                HttpWebRequest webRequest = HttpClient.CreateRequestPut(fullURL + "?comp=block&blockId=".ToLower(CultureInfo.InvariantCulture) + blockIdBase64, null);
                Dictionary<string, string> writerHeaders = OpenStreamWriteModeHeaders;
                writerHeaders["Content-Type"] = "DOCAVE/data".ToLower(CultureInfo.InvariantCulture);
                writerHeaders["Content-Length"] = count.ToString();
                writerHeaders["Content-MD5"] = contentMD5;
                HttpClient.CombiningRequestWithHeaders(webRequest, writerHeaders);
                return PutBlockContent(webRequest, buffer, offset, count);
            });
        }

        public bool PutBlockList(string fullURL, List<string> blockIds)
        {
            return RetryRequset.CloudRetry<bool>(delegate()
            {
                StringBuilder content = new StringBuilder();
                content.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                content.Append("<BlockList>");
                foreach (var blockId in blockIds)
                {
                    content.Append("<Latest>" + blockId + "</Latest>");
                }
                content.Append("</BlockList>");
                byte[] buffer = Encoding.UTF8.GetBytes(content.ToString());
                IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.MD5);
                byte[] data = md5.ComputeHash(buffer, 0, buffer.Length);
                string contentMD5 = Convert.ToBase64String(data);


                HttpWebRequest webRequest = HttpClient.CreateRequestPut(fullURL + "?comp=blockList".ToLower(CultureInfo.InvariantCulture), null);
                Dictionary<string, string> writerHeaders = Headers;
                writerHeaders["Accept-Charset"] = "UTF-8";
                writerHeaders["Content-Length"] = buffer.Length.ToString();
                writerHeaders["Content-MD5"] = contentMD5;
                HttpClient.CombiningRequestWithHeaders(webRequest, writerHeaders);

                return PutBlockContent(webRequest, buffer, 0, buffer.Length);
            });
        }



       

        private bool PutBlockContent(HttpWebRequest webRequest, byte[] buffer, int offset, int count)
        {
            using (Stream reqStream = webRequest.GetRequestStream())
            {
                reqStream.Write(buffer, offset, count);
            }
            using (HttpWebResponse response = DoExecute(webRequest, new Dictionary<string, string>()))
            {
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    response.Dispose();
                    return true;
                }
                else
                {
                    response.Dispose();
                    throw new Exception(string.Format("PutBlock url:{0} StatusCode={1}.", response.ResponseUri, response.StatusCode));
                }
            }
        }

        public bool SetBlobMetadata(string fullURL, Dictionary<string, string> metadataList)
        {
            if (metadataList == null || metadataList.Count == 0)
            {
                return false;
            }
            return RetryRequset.CloudRetry<bool>(delegate()
            {
                HttpWebRequest webRequest = HttpClient.CreateRequestPut(fullURL + "?comp=metadata", null);
                webRequest.ContentLength = 0;
                if (metadataList != null)
                {
                    foreach (var metadata in metadataList)
                    {
                        webRequest.Headers.Add("x-ms-meta-" + MSAzureUtils.MetadataKeyEncode(metadata.Key), string.IsNullOrEmpty(metadata.Value) ? "NULL" : metadata.Value);
                    }
                }
                HttpWebResponse response = DoExecute(webRequest, this.Headers);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    throw new Exception(string.Format("PutBlock url:{0} StatusCode={1}.", response.ResponseUri, response.StatusCode));
                }
            });
        }

        public bool ChangeBlobTier(string xSetName, string xStreamName, string tier)
        {
            var result = false;
            try
            {
                var parameters = new Dictionary<String, String>();
                parameters.Add(MSAzureKeyValueParams.COMP, "tier");
                var uri = this.BuildURL(xSetName, xStreamName, parameters);
                var request = this.GetAzureRequest(uri);
                request.Method = RESTCommands.PUT;
                request.Headers.Add(MSAzureConstants.AccessTierHeader, this.Encode(tier));
                request.Headers.Add("Content-Length", "0");
                request.Headers[MSAzureConstants.ApiVersionHeader] = MSAzureConstants.ApiVersion;
                using (var response = this.DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        response.Dispose();
                        result = true;
                    }
                    else
                    {
                        response.Dispose();
                        throw new Exception(string.Format("PutBlock url:{0} StatusCode={1}.", response.ResponseUri, response.StatusCode));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Change blob tier failed, object : " + xStreamName + ", container : " + xSetName, e);
                throw;
            }
            return result;
        }
    }
}
