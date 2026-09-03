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
using System.Text;
using AvePoint.GCommon;
using System.IO;
using System.Net;
using AvePoint.Media.Storage.Util;
using System.Threading;
using System.Reflection;
using System.Globalization;
using System.Web;

namespace AvePoint.Media.Storage.Cloud.ObjectAtmos
{
    class ObjectAtmosClient
    {
        ObjectAtmosOpenParameter openParam;
        ObjectAtmosSystem system;
        private static AveLogger logger = new AveLogger(typeof(ObjectAtmosClient));
        public string Endpoint { get; set; }
        delegate T RetryDelegate<T>();

        public ObjectAtmosClient(string endpoint, ObjectAtmosOpenParameter openParam, ObjectAtmosSystem system)
        {
            this.system = system;
            this.openParam = openParam;
            this.Endpoint = endpoint;
        }

        public bool CheckObject(StorageInfo info)
        {
            return Retry<bool>(delegate()
            {
                try
                {
                    string url = BuildURL(info);
                    HttpWebRequest request = GenerateRequest("HEAD", url, GetDefaultHeaders());
                    using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                    {
                        if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.NoContent)
                        {
                            return true;
                        }
                        else
                        {
                            throw new Exception(string.Format("get file {0} info failed:{1}", info.ObjectId, resp.ToString()));
                        }
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse resp = ex.Response as HttpWebResponse;
                    if (resp != null && (resp.StatusCode == HttpStatusCode.NotFound || resp.StatusCode == HttpStatusCode.BadRequest))
                    {
                        return false;
                    }
                    else
                    {
                        throw;
                    }
                }
            });
        }

        public Stream GetRequestStream(HttpWebRequest request)
        {
            try
            {
                return request.GetRequestStream();
            }
            catch (WebException ex)
            {
                logger.Error("get stream error {0}", ex);
                //this.system.RemoveDeadHost(this.Endpoint);
                throw;
            }
        }

        public HttpWebRequest GetUploadRequest(StorageInfo info)
        {
            Dictionary<string, string> writerHeaders = GetDefaultHeaders();
            writerHeaders["Content-Type"] = "DOCAVE/data".ToLower(CultureInfo.InvariantCulture);
            writerHeaders["Content-Length"] = info.Length.ToString();
            if (!string.IsNullOrEmpty(info.checksum))
            {
                writerHeaders["x-emc-wschecksum"] = "sha1/" + info.Length + "/" + info.checksum;
            }
            AddMetadata(info, writerHeaders);
            return GenerateRequest("POST", BuildURL(), writerHeaders);
        }

        private Dictionary<string, string> GetDefaultHeaders()
        {
            string date = DateTime.Now.ToUniversalTime().ToString("r");
            Dictionary<string, string> defaultHeaders = new Dictionary<string, string>();
            defaultHeaders.Add("x-emc-uid", this.openParam.FullTokenId);
            defaultHeaders.Add("x-emc-date", date);
            defaultHeaders.Add("Date", date);
            return defaultHeaders;
        }

        public HttpWebResponse OpenObject(StorageInfo info)
        {
            return Retry<HttpWebResponse>(delegate()
            {
                string url = BuildURL(info);
                Dictionary<string, string> headers = GetDefaultHeaders();
                if (info.Offset > 0)
                {
                    headers.Add("Range", "Bytes=" + info.Offset + "-");
                }
                HttpWebRequest request = GenerateRequest("GET", url, headers);
                HttpWebResponse resp = request.GetResponse() as HttpWebResponse;
                if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.NoContent)
                {
                    return resp;
                }
                else
                {
                    throw new Exception(string.Format("download file {0} info failed:{1}", info.ObjectId, resp.ToString()));
                }
            });
        }

        public bool DeleteObject(StorageInfo info)
        {
            return Retry<Boolean>(delegate()
            {
                try
                {
                    string url = BuildURL(info);
                    HttpWebRequest request = GenerateRequest("DELETE", url, GetDefaultHeaders());
                    using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                    {
                        if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.NoContent)
                        {
                            return true;
                        }
                        else
                        {
                            throw new Exception(string.Format("delete file {0} info failed:{1}", info.ObjectId, resp.ToString()));
                        }
                    }
                }
                catch (PathNotFoundException)
                {
                    return false;
                }
            });
        }

        public ObjectAtmosFileInfo GetObjectInfo(StorageInfo info)
        {
            return Retry<ObjectAtmosFileInfo>(delegate()
            {
                try
                {
                    string url = BuildURL(info);
                    HttpWebRequest request = GenerateRequest("HEAD", url, GetDefaultHeaders());
                    using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                    {
                        if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.NoContent)
                        {
                            return new ObjectAtmosFileInfo(info.HighName, info.LowName, resp.ContentLength, info.ObjectId);
                        }
                        else
                        {
                            throw new Exception(string.Format("get file {0} info failed:{1}", info.ObjectId, resp.ToString()));
                        }
                    }
                }
                catch (PathNotFoundException)
                {
                    return null;
                }
            });
        }

        public StorageOpenValidResult HasPermissions()
        {
            StorageOpenValidResult sr = new StorageOpenValidResult();
            sr.TotalFreeSpace = long.MaxValue - 1;
            sr.TotalSpace = long.MaxValue - 1;
            sr.TotalUsedSpace = 0;
            sr.SystemHealth = XSystemHealth.AvailableAndNotFull;
            sr.IsReadAble = true;
            sr.IsWriteAble = true;
            sr.IsDeleteAble = true;
            return sr;
        }

        private HttpWebRequest GenerateRequest(string method, string url, Dictionary<string, string> headers)
        {
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = method;
            AtmosUtils.AddSignatureHeader(webRequest, this.openParam.SharedSecret, headers);
            if (headers.Count > 0)
            {
                AddHeaders(webRequest, headers);
            }
            webRequest.AllowWriteStreamBuffering = false;
            webRequest.AllowAutoRedirect = false;
            webRequest.Timeout = 0x7ffffffe; //never timeout
            if (this.openParam.Proxy != null)
            {
                webRequest.Proxy = this.openParam.Proxy;
                if (webRequest.Proxy.Credentials != null)
                {
                    webRequest.PreAuthenticate = true;
                }
            }
            return webRequest;
        }

        public bool AddMetadata(StorageInfo storageInfo, Dictionary<string, string> writerHeaders)
        {
            StringBuilder listableMetas = new StringBuilder();
            if (openParam.CustomizedMetaMode.Equals(CustomizedMode.Close))
            {
                return true;
            }
            else if (openParam.CustomizedMetaMode.Equals(CustomizedMode.CustomizedOnly))
            {
                foreach (KeyValuePair<string, string> entry in openParam.CustomizedMetaData)
                {
                    listableMetas.Append(entry.Key);
                    listableMetas.Append("=");
                    listableMetas.Append(entry.Value != null ? Encode(entry.Value) : entry.Value);
                    listableMetas.Append(",");
                }
            }
            else if (openParam.CustomizedMetaMode.Equals(CustomizedMode.DocAveOnly))
            {
                foreach (KeyValuePair<string, string> entry in storageInfo.MetaInfos)
                {
                    listableMetas.Append(entry.Key);
                    listableMetas.Append("=");
                    listableMetas.Append(entry.Value != null ? Encode(entry.Value) : entry.Value);
                    listableMetas.Append(",");
                }
            }
            else if (openParam.CustomizedMetaMode.Equals(CustomizedMode.SupportAll))
            {
                foreach (KeyValuePair<string, string> entry in openParam.CustomizedMetaData)
                {
                    listableMetas.Append(entry.Key);
                    listableMetas.Append("=");
                    listableMetas.Append(entry.Value != null ? Encode(entry.Value) : entry.Value);
                    listableMetas.Append(",");
                }
                foreach (KeyValuePair<string, string> entry in storageInfo.MetaInfos)
                {
                    listableMetas.Append(entry.Key);
                    listableMetas.Append("=");
                    listableMetas.Append(entry.Value != null ? Encode(entry.Value) : entry.Value);
                    listableMetas.Append(",");
                }
            }
            else
            {
                throw new Exception("unKnown Customized Mode");
            }
            if (listableMetas.Length > 0)
            {
                writerHeaders["X-EMC-META"] = listableMetas.ToString().TrimEnd(',');
            }
            return true;
        }

        private string Encode(string str2Encode)
        {
            return HttpUtility.UrlEncode(str2Encode).Replace("+", "%20").Replace("/", "%2F");
        }

        public string EndWriteStream(HttpWebRequest httpWebRequest, StorageInfo storageInfo)
        {
            HttpWebResponse resp = httpWebRequest.GetResponse() as HttpWebResponse;
            if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.Created)
            {
                return resp.Headers["location"].ToString().TrimStart("/rest/objects/".ToCharArray());
            }
            else
            {
                this.system.RemoveDeadHost(this.Endpoint);
                throw new RetryableException("upload file failed:" + resp.ToString());
            }
        }

        private string BuildURL(StorageInfo info)
        {
            string url = this.Endpoint.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? StorageUrl.ObjectAtmos : StorageUrl.ObjectAtmosWithHttp;
            return string.Format(url, this.Endpoint, info.ObjectId);
        }

        private string BuildURL()
        {
            string url = this.Endpoint.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? StorageUrl.ObjectAtmosUpload : StorageUrl.ObjectAtmosUploadWithHttp;
            return string.Format(url, this.Endpoint);
        }

        public virtual void AddHeaders(HttpWebRequest request, Dictionary<string, string> headers)
        {
            MethodInfo method = request.Headers.GetType().GetMethod("AddWithoutValidate",
                                BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null,
                                new Type[] { typeof(string), typeof(string) }, null);

            foreach (KeyValuePair<string, string> item in headers)
            {
                if (item.Key.Equals("Content-Length"))
                {
                    request.ContentLength = Convert.ToInt64(item.Value);
                    //continue;
                }

                method.Invoke(request.Headers, new object[] { item.Key, item.Value });
            }
        }

        private T Retry<T>(RetryDelegate<T> del)
        {
            int counter = 0;
            while (true)
            {
                try
                {
                    counter++;
                    return del.Invoke();
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        HttpWebResponse resp = ex.Response as HttpWebResponse;
                        if (resp.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new PathNotFoundException(ex.Message, ex);
                        }
                        else if (resp.StatusCode == HttpStatusCode.InternalServerError || resp.StatusCode == HttpStatusCode.RequestTimeout || resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            logger.Info("this exception is a connection fail exception:" + ex.Message);
                            if (counter <= this.openParam.MaxRetryCount && !openParam.IsValidate)
                            {
                                logger.Info("Retry after " + this.openParam.RetryInterval + " ms. Retry count: " + counter + ". error message: " + ex.Message);
                                Thread.Sleep(this.openParam.RetryInterval);
                                continue;
                            }
                            else
                            {
                                this.system.RemoveDeadHost(this.Endpoint);
                                throw new RetryableException(ex.Message, ex);
                            }
                        }
                        else
                        {
                            string body = string.Empty;
                            using (Stream respStream = resp.GetResponseStream())
                            {
                                using (StreamReader sr = new StreamReader(respStream))
                                {
                                    body = sr.ReadToEnd();
                                }
                            }
                            logger.Error("execute request failed, msg:{0}, response body:{1}:", ex.Message, body, ex);
                            throw;
                        }
                    }
                    else if (ex.Status == WebExceptionStatus.ConnectionClosed || ex.Status == WebExceptionStatus.ConnectFailure || ex.Status == WebExceptionStatus.NameResolutionFailure || ex.Status == WebExceptionStatus.Timeout)
                    {
                        if (counter <= this.openParam.MaxRetryCount && !openParam.IsValidate)
                        {
                            logger.Info("Retry after " + this.openParam.RetryInterval + " ms. Retry count: " + counter + ". error message: " + ex.Message);
                            Thread.Sleep(this.openParam.RetryInterval);
                            continue;
                        }
                        else
                        {
                            this.system.RemoveDeadHost(this.Endpoint);
                            throw new RetryableException(ex.Message, ex);
                        }
                    }
                    else
                    {
                        logger.Error("execute request failed:" + ex.Message, ex);
                        throw;
                    }
                }
            }
        }
    }
}
