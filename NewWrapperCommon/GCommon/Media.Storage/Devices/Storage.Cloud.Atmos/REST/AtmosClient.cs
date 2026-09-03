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



namespace AvePoint.Media.Storage.Cloud.Atmos
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.GCommon;
    using System.Net;
    using System.IO;
    using AvePoint.Media.Storage.Cloud.Common;
    using System.Xml;
    using System.Reflection;
    using AvePoint.Media.Storage.Util;
    using System.Diagnostics;
    using AvePoint.GCommon.Contract.CodeReview;
    using System.Globalization;
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
    class AtmosClient : AbstractRESTOprationExecutor
    {
        private AtmosOpenParameter openParams;

        public AtmosClient() 
        {
            Logger = StorageLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            HttpClient = new AtmosHttpClient();
        }

        //在每一种Cloud中重写这个方法就是为了把openParams.FlushDNS这个参数默认false，这个属性Cloud中不会用到
        public override void InitRetry(CloudOpenParameter openParams)
        {
            Logger.Info("Init Retry: retryCount " + openParams.MaxRetryCount + ",RetryInterval " + openParams.RetryInterval);
            RetryRequset = new Retry(openParams.MaxRetryCount, openParams.RetryInterval, openParams.NeedRetry, true);
        }

        #region Implement ICloudOprationExecutor Methods

        public override void InitConfig(CloudOpenParameter prams)
        {
            this.openParams = prams as AtmosOpenParameter;
            if (openParams.CType.Equals(XRIParameterKeys.CTYRE_ATMOS, StringComparison.CurrentCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(prams.AccessPoint))
                {
                    Endpoint = StorageUrl.Atmos;
                }
                else
                {
                    Endpoint = prams.AccessPoint + "/rest/namespace";
                }
            }
            else if (openParams.CType.Equals(XRIParameterKeys.CTYRE_ATT, StringComparison.CurrentCultureIgnoreCase))
            {
                Endpoint = StorageUrl.AT_T;
            }
            else
            {
                throw new Exception("Unknown cloud type :" + openParams.CType);
            }
            HttpClient.OpenParam = openParams;
            this.CloudOpenParam = openParams;
            base.InitProxySetting();
            InitRetry(prams);
        }

        /**
         *   <?xml version='1.0' encoding='UTF-8'?>
         *   <ListDirectoryResponse xmlns='http://www.emc.com/cos/'>
         *      <DirectoryList>
         *          <DirectoryEntry>
         *              <ObjectID>4a3fd8dfa2a8482004a3fd9315cf4704a76e6f2f1072</ObjectID>
         *              <FileType>regular</FileType>
         *              <Filename>samplefile</Filename>
         *          </DirectoryEntry>
         *      </DirectoryList>
         *   </ListDirectoryResponse>
         */
        public override List<string> ListContainers()
        {
            List<string> result = new List<string>();
            try
            {
                AtmosRequest request = GetAtmosRequest(Endpoint);
                request.Method = RESTCommands.GET;
                using (HttpWebResponse response = DoExecute(request))
                {
                    string message = GetStringFromResponse(response);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Dictionary<string, string> np = new Dictionary<string, string>();
                        np.Add("EMC".ToLower(CultureInfo.InvariantCulture), "HTTP://WWW.EMC.COM/COS/".ToLower(CultureInfo.InvariantCulture));
                        result = AnalyzeXML(message, "DirectoryList/DirectoryEntry/Filename", np);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("List containers failed.", e.ToString());
                throw;
            }
            return result;
        }

        public override bool CheckContainer(string xSetName)
        {
            bool result = false;
            try
            {
                AtmosRequest request = GetAtmosRequest(BuildURL(xSetName));
                request.Method = RESTCommands.GET;

                using (HttpWebResponse response = DoExecute(request))
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
                Logger.Error(GetCheckCtnErrorMsg(xSetName), e.ToString());
                throw;
            }
            return result;
        }

        public override bool CreateContainer(string xSetName)
        {
            bool result = false;
            try
            {
                AtmosRequest request = GetAtmosRequest(BuildURL(xSetName));
                request.Method = RESTCommands.POST;
                request.Headers.Add("Content-Type", "application/octet-stream");
                request.Headers.Add("Content-Length", "0");
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
                List<string> objects = ListObject(xSetName);
                foreach (string obj in objects)
                {
                    if (obj.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        DeleteContainer(xSetName + "/" + obj);
                    }
                    else
                    {
                        DeleteObject(xSetName, obj);
                    }
                }
                AtmosRequest request = GetAtmosRequest(BuildURL(xSetName));
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
                Logger.Warn("Cannot find object, maybe it was deleted successfully before : " + e.Message);
                result = true;
            }
            catch (Exception e)
            {
                Logger.Error(GetDeleteCtnErrorMsg(xSetName), e);
                throw;
            }
            return result;
        }

        /**
          *
          *  <?xml version='1.0' encoding='UTF-8'?>
            <ListDirectoryResponse xmlns='http://www.emc.com/cos/'>
                <DirectoryList>	
                    <DirectoryEntry>		
                        <ObjectID>4980cdb2ae10109704bd112098202804c7746de29ad0</ObjectID> 		
                        <FileType>regular</FileType> 		
                        <Filename>123</Filename>	
                    </DirectoryEntry>	
                    <DirectoryEntry>		
                        <ObjectID>4980cdb2b010109904bd0d883482a204c775173eb2af</ObjectID> 		
                        <FileType>regular</FileType> 		
                        <Filename>test.txt</Filename>	
                    </DirectoryEntry>	
                    <DirectoryEntry>		
                        <ObjectID>4980cdb2b010109704bd0ef19892bf04c777e34c471f</ObjectID> 		
                        <FileType>directory</FileType> 		
                        <Filename>subfolder</Filename>	
                        </DirectoryEntry>	
                    <DirectoryEntry>		
                        <ObjectID>4980cdb2b710109804bd0d5314073f04c777e5dada2a</ObjectID> 	
                        <FileType>directory</FileType> 	
                        <Filename>su</Filename>	
                    </DirectoryEntry>	
                </DirectoryList>
            </ListDirectoryResponse>
          */
        public override List<string> ListObject(string xSetName)
        {
            List<string> result = new List<string>();
            try
            {
                AtmosRequest request = GetAtmosRequest(BuildURL(xSetName));
                request.Method = RESTCommands.GET;

                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string xml = GetStringFromResponse(response);
                        if (!string.IsNullOrEmpty(xml))
                        {
                            result = DistinguishDirectoryAndFile(xml);
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

        public override List<string> ListObject(string xSetName, string prefix)
        {
            List<string> result = new List<string>();
            if (prefix.Contains("\\"))
            {
                prefix = prefix.Replace("\\", "/");
            }
            result = ListObject(xSetName + "/" + prefix);
            return result;
        }

        public override long GetContainerSize(string xSetName)
        {
            long size = 0;
            List<string> objNames = ListObject(xSetName);
            foreach (string name in objNames)
            {
                long objSize = GetObjectInfo(xSetName, name).FileSize;
                if (objSize >= 0)
                {
                    size += objSize;
                }
            }
            return size;
        }

        public override bool CheckObject(string xSetName, string xStreamName)
        {
            return CheckAtmosOrAttObjectExist(BuildURL(xSetName, xStreamName));
        }

        private string GetObjectUserMetaData(string xSetName, string xStreamName, bool isUserMeta)
        {
            return GetObjectMetaData(xSetName, xStreamName, true);
        }

        private string GetObjectSystemMetaData(string xSetName, string xStreamName, bool isUserMeta)
        {
            return GetObjectMetaData(xSetName, xStreamName, false);
        }

        private string GetObjectMetaData(string xSetName, string xStreamName, bool isUserMeta)
        {
            string meta = null;

            try
            {
                string uri = BuildURL(xSetName, xStreamName);
                if (isUserMeta)
                {
                    uri += "?metadata/user";
                }
                else
                {
                    uri += "?metadata/system";
                }
                AtmosRequest request = GetAtmosRequest(uri);
                request.Method = RESTCommands.GET;

                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        meta = response.Headers.Get("x-emc-meta");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("get meta data for file, xSet:" + xSetName + ", xStream:" + xStreamName, ex);
                throw;
            }
            return meta;
        }

        public bool SetObjectMetaData(string xSetName, string xStreamName, string meta)
        {
            bool result = false;
            try
            {
                if (string.IsNullOrEmpty(meta))
                {
                    Logger.Warn("meta set to object is null or empty");
                    return false; 
                }
                string uri = BuildURL(xSetName, xStreamName) + "?metadata/user";
                AtmosRequest request = GetAtmosRequest(uri);
                request.Method = RESTCommands.POST;
                request.Headers.Add("x-emc-meta", meta);

                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("set meta data for file, xSet:" + xSetName + ", xStream:" + xStreamName + ",meta:" + meta, ex);
                throw;
            }
            return result;
        }


        public override HttpWebRequest GetUploadRequest(string xSetName, string xStreamName, string mimeType, HttpWebRequest webRequest, int blockNumber, long dataLength)
        {
            if (webRequest != null)
            {
                return webRequest;
            }
            AtmosRequest request = GetAtmosRequest(BuildURL(xSetName, xStreamName));
            if (CheckObject(xSetName, xStreamName))
            {
                request.Method = RESTCommands.PUT;
            }
            else
            {
                request.Method = RESTCommands.POST;
            }
            request.Headers.Add("Content-Type", mimeType);
            request.Headers.Add("Content-Length", dataLength.ToString());
            if (!string.IsNullOrEmpty(openParams.Policy))
            {
                request.Headers.Add("x-emc-meta", openParams.Policy);
            }
            Logger.Info("get stream for file, xSet:" + xSetName + ", xStream:" + xStreamName);
            return HttpClient.GetWebRequestForUpLoad(request);
        }

        public override bool CreateObject(string xSetName, string xStreamName, HttpWebRequest request, long dataLength)
        {
            bool result = false;

            try
            {
                using (HttpWebResponse resp = UpLoad(request))
                {
                    if (resp.StatusCode == HttpStatusCode.Created || resp.StatusCode == HttpStatusCode.OK)
                    {
                        Logger.Info("create xStream succeed, xSet:" + xSetName + ", xStream:" + xStreamName);
                        result = true;
                    }
                    else
                    {
                        Logger.Error("Create object failed. object : " + xStreamName + ",container : " + xSetName + ", statues:" + resp.StatusCode);
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

        public override Stream OpenObject(string container, string objectName, int[] lengths, FileMode mode)
        {
            AtmosRequest request = GetAtmosRequest(BuildURL(container, objectName));
            switch (mode)
            {
                case FileMode.Open:
                    request = GetAtmosRequest(BuildReadURL(container, objectName));
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
                    if (!CheckObject(container, objectName))
                    {
                        request.Method = RESTCommands.PUT;
                    }
                    else
                    {
                        request.Method = RESTCommands.POST;
                    }
                    int dataLength = lengths[0];
                    request.Headers.Add("Content-Type", "DocAve/data".ToLower(CultureInfo.InvariantCulture));
                    request.Headers.Add("Content-Length", dataLength.ToString());
                    if (!string.IsNullOrEmpty(openParams.Policy))
                    {
                        request.Headers.Add("x-emc-meta", openParams.Policy);
                    }
                    return HttpClient.GetWebRequestForUpLoad(request).GetRequestStream();
                default:
                    break;
            }
            return null;
        }

        public override Stream OpenObject(string xSetName, string xStreamName, int rangFrom, int rangeTo)
        {
            try
            {
                AtmosRequest request = GetAtmosRequest(BuildURL(xSetName, xStreamName));
                request.Method = RESTCommands.GET;
                if (rangFrom >= 0 && rangeTo >= 0 && rangFrom < rangeTo)
                {
                    string range = "bytes=" + rangFrom + "-" + rangeTo;
                    request.Headers.Add("Range", range);
                }
                HttpWebResponse response = DoExecute(request);
                return new HttpDownloadStream(response);
            }
            catch (Exception e)
            {
                Logger.Error(GetOpenObjErrorMsg(xStreamName, xSetName), e);
                throw;
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

        private bool DeleteObject(string xSetName, string xStreamName)
        {
            bool result = false;
            try
            {
                AtmosRequest request = GetAtmosRequest(BuildURL(xSetName, xStreamName));
                request.Method = RESTCommands.DELETE;

                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        result = true;
                    }
                }
            }
            catch (PathNotFoundException t)
            {
                Logger.Warn("Cannot find the object, maybe it was deleted successfully before." + t.Message);
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
                AtmosRequest request = GetAtmosRequest(BuildURL(xSetName, xStreamName));
                request.Method = RESTCommands.HEAD;
                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string meta = response.Headers.Get("x-emc-meta");
                        string[] metaBuffer = meta.Split(',');
                        foreach (string value in metaBuffer)
                        {
                            if (value.Contains("size"))
                            {
                                result.FileSize = long.Parse(value.Substring(value.IndexOf('=') + 1));
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(GetCheckObjErrorMsg(xStreamName, xSetName), e);
                throw;
            }
            return result;
        }

        public override bool Login(string xSetName)
        {
            bool result = false;
            try
            {
                if (CheckContainer(xSetName))
                {
                    result = true;
                }
                else if (openParams.ModuleType == 0)
                {
                    result = CheckContainer(xSetName);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
                return false;
            }
            return result;
        }

        #endregion

        #region Build URL

        protected string BuildURL(string containerName)
        {
            if (containerName == null)
            {
                return Endpoint;
            }
            if (containerName.Contains("\\"))
            {
                containerName = containerName.Replace("\\", "/");
            }
            if (!containerName.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                containerName += "/";
            }
            containerName = Encode(containerName);
            string url = Endpoint + "/" + containerName;
            return url;
        }

        protected string BuildURL(string containerName, string objectName)
        {
            if ("/".Equals(objectName, StringComparison.CurrentCultureIgnoreCase))
            {
                objectName = string.Empty;
                return BuildURL(containerName);
            }
            return BuildURL(containerName) + Encode(objectName);
        }

        private string BuildReadURL(string container, string objectName)
        {
            if (objectName.Contains("\\"))
            {
                objectName = objectName.Replace("\\", "/");
            }
            return BuildURL(container) + objectName;
        }

        public override string BuildURLWithOutQueryParams(string container)
        {
            return BuildURL(container);
        }

        public override string BuildObjectAbsoluteURL(string container, string objectName)
        {
            return BuildURL(container, objectName);
        }

        #endregion

        private AtmosRequest GetAtmosRequest(string uri)
        {
            AtmosRequest request = new AtmosRequest();
            string date = DateTime.Now.ToUniversalTime().ToString("r");
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("x-emc-uid", openParams.UserName);
            headers.Add("x-emc-date", date);
            headers.Add("Date", date);
            request.URI = uri;
            request.UserName = openParams.UserName;
            request.Password = openParams.Password;
            request.Headers = headers;
            return request;
        }

        public override bool IsServerIntertalError(HttpStatusCode code)
        {
            bool result = false;
            if (base.IsServerIntertalError(code) || code == HttpStatusCode.Conflict)
            {
                result = true;
            }
            return result;
        }

        /**
          *
          *  <?xml version='1.0' encoding='UTF-8'?>
            <ListDirectoryResponse xmlns='http://www.emc.com/cos/'>
                <DirectoryList>	
                    <DirectoryEntry>		
                        <ObjectID>4980cdb2ae10109704bd112098202804c7746de29ad0</ObjectID> 		
                        <FileType>regular</FileType> 		
                        <Filename>123</Filename>	
                    </DirectoryEntry>	
                    <DirectoryEntry>		
                        <ObjectID>4980cdb2b010109904bd0d883482a204c775173eb2af</ObjectID> 		
                        <FileType>regular</FileType> 		
                        <Filename>test.txt</Filename>	
                    </DirectoryEntry>	
                    <DirectoryEntry>		
                        <ObjectID>4980cdb2b010109704bd0ef19892bf04c777e34c471f</ObjectID> 		
                        <FileType>directory</FileType> 		
                        <Filename>subfolder</Filename>	
                        </DirectoryEntry>	
                    <DirectoryEntry>		
                        <ObjectID>4980cdb2b710109804bd0d5314073f04c777e5dada2a</ObjectID> 	
                        <FileType>directory</FileType> 	
                        <Filename>su</Filename>	
                    </DirectoryEntry>	
                </DirectoryList>
            </ListDirectoryResponse>
          */
        protected List<string> DistinguishDirectoryAndFile(string xml)
        {
            List<string> result = new List<string>();
            XmlNodeList entries = GetNodeList(xml, "/ListDirectoryResponse/DirectoryList/DirectoryEntry"); 
            foreach (XmlNode entry in entries)
            {
                XmlNodeList children = entry.ChildNodes;
                if (children[1].InnerText.Equals("directory"))
                {
                    result.Add(children[2].InnerText + "/");
                }
                else
                {
                    result.Add(children[2].InnerText);
                }
            }
            return result;
        }

        protected override void ResetRequest(HttpWebRequest request)
        {
            if (request.Method == "POST")
            {
                try
                {
                    AtmosRequest deleteRequest = GetAtmosRequest(request.RequestUri.ToString());
                    deleteRequest.Method = RESTCommands.DELETE;
                    using (HttpWebResponse response = DoExecute(deleteRequest))
                    {
                        if (response.StatusCode == HttpStatusCode.NoContent)
                        {
                            Logger.Info("clear no use xStream before retry succeed, url:" + request.RequestUri.ToString());
                        }
                    }
                }
                catch (WebException we)
                {
                    if (we.Status == WebExceptionStatus.ProtocolError)
                    {
                        HttpWebResponse response = we.Response as HttpWebResponse;
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            Logger.Info("clear no use xStream before retry succeed, url:" + request.RequestUri.ToString());
                        }
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message,ex);
                    throw;
                }
            }
        }

        public override CloudOpenParameter ConveryParams(Dictionary<string, string> prams)
        {
            AtmosOpenParameter openParams = new AtmosOpenParameter();
            if (prams != null)
            {
                //if (prams.ContainsKey("
            }
            return null;
        }

        public override HttpDownloadStream OpenObjectForRead(string fullURL, Dictionary<string, string> headers)
        {
            HttpWebRequest request = HttpClient.CreateRequestGet(fullURL, null);
            return new HttpDownloadStream(DoExecute(request, headers));
        }

        private bool CheckAtmosOrAttObjectExist(string fullURL)
        {
            bool result = false;
            try
            {
                AtmosRequest request = GetAtmosRequest(fullURL);
                request.Method = RESTCommands.HEAD;

                using (HttpWebResponse response = DoExecute(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
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
                Logger.Error("check object failed:{0},details{1}", fullURL, e.ToString());
                throw;
            }
            return result;
        }

        public override ResponseInfo ListObjects(string baseURL, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
        {
            ResponseInfo responseInfo = new ResponseInfo();
            HttpWebRequest requestGet = HttpClient.CreateRequestGet(baseURL, queryParams);
            using (HttpWebResponse resp = DoExecute(requestGet, headers))
            {
                using (Stream inputStream = resp.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(inputStream))
                    {
                        responseInfo.ResponseXml = Decode(reader.ReadToEnd());
                        responseInfo.Headers["X-EMC-TOKEN".ToLower(CultureInfo.InvariantCulture)] = resp.GetResponseHeader("X-EMC-TOKEN".ToLower(CultureInfo.InvariantCulture));
                        return responseInfo;
                    }
                }
            }
        }

        public override HttpUploadStream OpenObjectForWrite(string fullURL, Dictionary<string, string> headers)
        {
            HttpWebRequest request = HttpClient.CreateRequestPost(fullURL, null);
            if (CheckAtmosOrAttObjectExist(fullURL))
                request.Method = "PUT";
            HttpClient.CombiningRequestWithHeaders(request, headers);
            return new HttpUploadStream(request) { HttpClient = this.HttpClient };
        }

        private void AssmbleRequest(HttpWebRequest request, Dictionary<string, string> headers)
        {
            HttpClient.CombiningRequestWithHeaders(request, headers);
            string date = DateTime.Now.ToUniversalTime().ToString("r");
            headers.Add("x-emc-uid", openParams.UserName);
            headers.Add("x-emc-date", date);
            headers.Add("Date", date);
            Dictionary<string, string> header4Request = new Dictionary<string, string>();
            header4Request.Add("x-emc-uid", openParams.UserName);
            header4Request.Add("x-emc-date", date);
            header4Request.Add("Date", date);
            HttpClient.CombiningRequestWithHeaders(request, header4Request);
            AtmosUtils.AddSignatureHeader(request, openParams.Password, headers);
        }

        public override void CreateObjectWithNoContent(string fullURL, Dictionary<string, string> headers)
        {
            try
            {
                HttpWebRequest requestGet = HttpClient.CreateRequestPost(fullURL, null);
                if (CheckAtmosOrAttObjectExist(fullURL))
                requestGet.Method = "PUT";
                using (HttpWebResponse resp = DoExecute(requestGet, headers))
                {
                    //no code, just to close resp.
                }
            }
            catch (WebException tx)
            {
                HttpWebResponse hasClosedResponse = tx.Response as HttpWebResponse;
                if (hasClosedResponse.StatusCode == HttpStatusCode.BadRequest)
                    throw new PathNotFoundException("The requested container does not exist");
                throw new UnknownException("Create ObjectWithNoContent Failed : " + tx.Message, tx);
            }

        }

        public override Dictionary<string, string> Headers
        {
            get
            {
                string date = DateTime.Now.ToUniversalTime().ToString("r");
                Dictionary<string, string> headers = new Dictionary<string, string>()
                {
                    {"x-emc-uid", openParams.UserName},
                    {"x-emc-date", date},
                    {"Date", date}
                };
                return headers;
            }
        }

        public override string BuildObjectAbsoluteURL(string url, string container, string objectName)
        {
            throw new NotSupportedException();
        }

        public override string ListAzureMetaName(string baseURL, Dictionary<string, string> queryParams, Dictionary<string, string> headers)
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
