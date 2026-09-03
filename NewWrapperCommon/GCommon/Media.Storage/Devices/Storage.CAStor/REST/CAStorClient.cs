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



namespace AvePoint.Media.Storage.CAStor
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Media.Storage.Util;
    using System.IO;
    using AvePoint.GCommon;
    using System.Net;
    using Scsp;
    using System.Reflection;
    using AvePoint.Media.Storage.Inner;
    using AvePoint.GCommon.Utility.I18N;
    using System.Text.RegularExpressions;
    using System.Diagnostics;
    using AvePoint.GCommon.Contract.CodeReview;
    using System.Web;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
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
    class CAStorClient
    {
        static AveLogger logger = new AveLogger(typeof(CAStorClient));
        public CAStorOpenParameter OpenParam { get; set; }
        ScspClient scspClient;
        ScspClient remoteClient;
        Regex objectIdRegex = new Regex("^[a-zA-Z0-9]{32}$");
        public string VimName { get; set; }

        private static SafeDictionary<string, CAStorOpenParameter> cacheOpenParams = new SafeDictionary<string, CAStorOpenParameter>();

        public CAStorClient(CAStorOpenParameter openParam)
        {
            this.OpenParam = openParam;
            if (!string.IsNullOrEmpty(openParam.PhysicalIdAndMidifyTime) && cacheOpenParams.ContainsKey(this.OpenParam.PhysicalIdAndMidifyTime))
            {
                CAStorOpenParameter param = cacheOpenParams[this.OpenParam.PhysicalIdAndMidifyTime];
                if (param != null)
                {
                    if (param.IsSecondaryTimeOut)
                    {
                        logger.Debug("endCacheSecondaryTime={0}", DateTime.UtcNow);
                        cacheOpenParams.Remove(this.OpenParam.PhysicalIdAndMidifyTime);
                    }
                    else
                    {
                        this.OpenParam.IsLocalClientFailed = true;
                    }
                }
            }
        }

        public SpaceInfo CheckFreeSpace()
        {
            return (SpaceInfo)Invoke("CheckFreeSpaceMethod", null);
        }

        private SpaceInfo CheckFreeSpaceMethod()
        {
            return CheckFreeSpace(GetScspClient());
        }

        private SpaceInfo CheckFreeSpace(ScspClient _scspClient)
        {
            SpaceInfo spaceInfo = new SpaceInfo();
            ScspResponse ncResponse = null;
            try
            {
                //get cluster Info
                ncResponse = _scspClient.Info("", "", new ScspQueryArgs(), new ScspHeaders());
                if ((HttpStatusCode)ncResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    spaceInfo.TotalFreeSpace = (ulong.Parse(ncResponse.ResponseHeaders.GetHeaderValues(CAStorConstants.AVAILABEL_SPACE)[0])) * 1024 * 1024 * 1024;
                    spaceInfo.TotalSpace = (ulong.Parse(ncResponse.ResponseHeaders.GetHeaderValues(CAStorConstants.TOTAL_SPACE)[0])) * 1024 * 1024 * 1024;
                    spaceInfo.TotalUsedSpace = spaceInfo.TotalSpace - spaceInfo.TotalFreeSpace;
                }
            }
            catch (Exception ex)
            {
                logger.Info("get free space failed:{0}, status code:{1}", ex.Message, (ncResponse != null ? Convert.ToString(ncResponse.HttpStatusCode) : "null"));
                spaceInfo.TotalFreeSpace = long.MaxValue - 1;
                spaceInfo.TotalSpace = long.MaxValue - 1;
                spaceInfo.TotalUsedSpace = 0;
            }
            return spaceInfo;
        }



        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Stor")]
        private ScspClient GetLocalScspClient()
        {
            if (scspClient == null)
            {
                ushort timeOut = (ushort)(OpenParam.RetryInterval / 1000);
                switch (OpenParam.LocatorType)
                {
                    case LocatorType.Static:
                        List<string> hosts = new List<string>();
                        string[] host = OpenParam.PrimaryNodes.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string ip in host)
                        {
                            hosts.Add(ip);
                        }
                        scspClient = new ScspClient(hosts, (ushort)OpenParam.PrimaryPort, (ushort)OpenParam.MaxRetryCount, new StaticLocator(hosts), timeOut, timeOut, timeOut, 1000);
                        scspClient.Start();
                        logger.Info("get a CAStor client, static locator, host: {0}, port{1}:", OpenParam.PrimaryNodes, (ushort)OpenParam.PrimaryPort);
                        break;
                    case LocatorType.Proxy:
                    default:
                        ProxyLocator locator = new ProxyLocator(OpenParam.ClusterName, OpenParam.PrimaryNodes, (ushort)OpenParam.PrimaryPort, timeOut);
                        scspClient = new ScspClient(null, (ushort)OpenParam.PrimaryPort, (ushort)OpenParam.MaxRetryCount, locator, timeOut, timeOut, timeOut, 1000);
                        scspClient.Start();
                        logger.Info("get a CAStor client, proxy locator, host: {0}, port{1}:", OpenParam.PrimaryNodes, (ushort)OpenParam.PrimaryPort);
                        break;
                }
            }
            return scspClient;
        }

        public ScspClient GetScspClient()
        {
            try
            {
                if (this.OpenParam.IsLocalClientFailed)
                {
                    return GetRemoteScspClient();
                }
                else
                {
                    return GetLocalScspClient();
                }
            }
            catch (Exception ex)
            {
                logger.Error("get Castor storage client failed:" + ex.Message, ex);
                throw;
            }
        }



        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Stor")]
        public ScspClient GetRemoteScspClient()
        {
            if (remoteClient == null)
            {
                List<String> remoteHosts = new List<String>();
                remoteHosts.Add(GetRemoteNodeHost());

                ushort timeOut = (ushort)(OpenParam.RetryInterval / 1000);
                if (OpenParam.RemoteClusterType == 0)
                {
                    remoteClient = new ScspClient(remoteHosts, (ushort)GetRemoteNodePort(), (ushort)OpenParam.MaxRetryCount, new StaticLocator(remoteHosts, timeOut), timeOut, timeOut, timeOut, 1000);
                    logger.Info("get a Remote CAStor client, static locator, host: {0}, port{1}:", OpenParam.RemoteCSNHost, (ushort)OpenParam.RemoteCSNPort);
                }
                else
                {
                    ProxyLocator locator = new ProxyLocator(OpenParam.RemoteClusterName, GetRemoteNodeHost(), (ushort)GetRemoteNodePort(), timeOut);
                    remoteClient = new ScspClient(null, (ushort)OpenParam.PrimaryPort, (ushort)OpenParam.MaxRetryCount, locator, timeOut, timeOut, timeOut, 1000);
                    logger.Info("get a Remote CAStor client, proxy locator, host: {0}, port{1}:", OpenParam.ScspProxyHost, (ushort)OpenParam.ScspProxyPort);
                }
                remoteClient.Start();
            }
            return remoteClient;
        }

        public StorageOpenValidResult HasPermissions()
        {
            StorageOpenValidResult rs = new StorageOpenValidResult();
            ScspClient tempLocalScspClient = GetLocalScspClient();
            ScspClient tempRemoteScspClient = null;
            if (OpenParam.UseRemoteCluster)
            {
                ScspClient temp = GetRemoteScspClient();
                if (OpenParam.IsLocalClientFailed)
                {
                    tempLocalScspClient = temp;
                    tempRemoteScspClient = tempLocalScspClient;
                }
                else
                {
                    tempRemoteScspClient = temp;
                }
            }

            try
            {
                bool localClientIsOk = HasPermissions(tempLocalScspClient);
            }
            catch (Exception ex)
            {
                logger.Error("HasPermissions error: " + ex.Message);
                tempLocalScspClient = null;
                if (OpenParam.IsLocalClientFailed)
                {
                    if (remoteClient != null)
                    {
                        remoteClient.Stop();
                        remoteClient = null;
                    }

                }
                else
                {
                    if (scspClient != null)
                    {
                        scspClient.Stop();
                        scspClient = null;
                    }
                }

                if (OpenParam.UseRemoteCluster)
                {
                    bool remoteClientIsOk = HasPermissions(tempRemoteScspClient);
                }
                else
                {
                    throw;
                }
            }

            SpaceInfo info = CheckFreeSpace(tempLocalScspClient != null ? tempLocalScspClient : tempRemoteScspClient);
            rs.IsHasPermission = true;
            rs.TotalFreeSpace = info.TotalFreeSpace;
            rs.TotalSpace = info.TotalSpace;
            rs.TotalUsedSpace = info.TotalUsedSpace;
            rs.IsReadAble = true;
            rs.IsWriteAble = true;
            rs.IsDeleteAble = true;
            return rs;
        }

        public bool HasPermissions(ScspClient scspClient)
        {
            bool result = false;
            if (scspClient == null)
            {
                return false;
            }
            String testData = "DocAve test write";
            UInt64 testDataLength = 0;
            MemoryStream inputStream = MakeStream(testData, ref testDataLength);
            byte[] bytes = Encoding.Default.GetBytes(testData.ToCharArray());
            ScspResponse wcResponse = scspClient.WriteMutable("", inputStream, testDataLength, GetScspQueryArgs(), new ScspHeaders());
            if (wcResponse.HttpStatusCode == (ushort)HttpStatusCode.Created)
            {
                string uuid = wcResponse.ResponseHeaders.GetHeaderValues(CAStorConstants.OBJCET_UUID)[0];
                ScspResponse deResponse = scspClient.DeleteMutable(uuid, string.Empty, GetScspQueryArgs(), new ScspHeaders());
                if (deResponse.HttpStatusCode == (ushort)HttpStatusCode.NoContent || deResponse.HttpStatusCode == (ushort)HttpStatusCode.OK)
                {
                    result = true;
                }
                else
                {
                    throw new AuthenticationFailedException(string.Format("validate {0} failed, status :{1}", scspClient.ToString(), deResponse.HttpStatusCode));
                }
            }
            else
            {
                throw new AuthenticationFailedException(string.Format("validate {0} failed, status :{1}", scspClient.ToString(), wcResponse.HttpStatusCode));
            }
            return result;
        }

        private ScspQueryArgs GetScspQueryArgs()
        {
            ScspQueryArgs args = new ScspQueryArgs();
            args.SetValue("alias", "yes");
            return args;
        }

        private MemoryStream MakeStream(String val, ref UInt64 contentLength)
        {
            byte[] bytes = Encoding.Default.GetBytes(val.ToCharArray());
            contentLength = (UInt64)bytes.Length;
            return new MemoryStream(bytes);
        }

        private string GetRemoteObjectPath()
        {
            string path = string.Empty;
            if (OpenParam.RemoteClusterType != 0)
            {
                path = "_proxy/" + OpenParam.RemoteClusterName;
            }
            return path;
        }

        private string GetRemoteNodeHost()
        {
            if (OpenParam.RemoteClusterType == 0)
            {
                return OpenParam.RemoteCSNHost;
            }
            return OpenParam.ScspProxyHost;
        }

        private int GetRemoteNodePort()
        {
            if (OpenParam.RemoteClusterType == 0)
            {
                return OpenParam.RemoteCSNPort;
            }
            return OpenParam.ScspProxyPort;
        }

        //public long GetFreeSpace()
        //{
        //    return GetClusterSpaceInfo(true);
        //}

        //public long GetTotalSpace()
        //{
        //    return GetClusterSpaceInfo(false);
        //}

        //private long GetClusterSpaceInfo(bool isFreeSpace)
        //{
        //    long space = 0;
        //    ScspResponse ncResponse = null;
        //    try
        //    {
        //        //get cluster Info
        //        ncResponse = localRequest.Info("", "", new ScspQueryArgs(), new ScspHeaders());
        //        if ((HttpStatusCode)ncResponse.HttpStatusCode == HttpStatusCode.OK)
        //        {
        //            if (isFreeSpace)
        //            {
        //                space = long.Parse(ncResponse.ResponseHeaders.GetHeaderValues(CAStorConstants.AVAILABEL_SPACE)[0]);
        //            }
        //            else
        //            {
        //                space = long.Parse(ncResponse.ResponseHeaders.GetHeaderValues(CAStorConstants.TOTAL_SPACE)[0]);
        //            }
        //            return space * 1024 * 1024 * 1024;
        //        }
        //        else
        //        {
        //            logger.Error("get free space failed:" + ncResponse.HttpStatusCode);
        //            return long.MaxValue;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("get free space failed:" + ncResponse.HttpStatusCode);
        //        return long.MaxValue;
        //    }
        //}

        //public StorageDeleteResult DeleteFile(StorageInfo info)
        //{
        //    StorageDeleteResult rs = new StorageDeleteResult();
        //    string nextObjId = GetNextMetaId(info);
        //    while (!string.IsNullOrEmpty(nextObjId))
        //    {
        //        StorageInfo storageInfo = info.Clone();
        //        storageInfo.ObjectId = nextObjId;
        //        nextObjId = GetNextMetaId(storageInfo);
        //        DeleteSingleOject(storageInfo);
        //    }
        //    DeleteSingleOject(info);
        //    rs.IsDeleted = true;
        //    return rs;
        //}
        public StorageDeleteResult DeleteFile(StorageInfo info)
        {
            StorageDeleteResult rs = new StorageDeleteResult();
            logger.Info("try to delete object id:" + info.ObjectId);
            try
            {
                ScspResponse icResponse = GetScspClient().DeleteMutable(info.ObjectId, "", GetScspQueryArgs(), new ScspHeaders());
                if (icResponse.HttpStatusCode == (ushort)HttpStatusCode.OK || icResponse.HttpStatusCode == (ushort)HttpStatusCode.NoContent || icResponse.HttpStatusCode == (ushort)HttpStatusCode.NotFound)
                {
                    logger.Info("delete object succeed, id:" + info.ObjectId);
                }
                else
                {
                    if (icResponse.HttpStatusCode == (ushort)HttpStatusCode.Forbidden)
                    {
                        throw new AuthenticationFailedException("delete object failed, id:" + info.ObjectId + ", msg:" + icResponse.HttpStatusCode + ", " + icResponse.ResponseBody.ToString());
                    }
                    else
                    {
                        throw new Exception("delete object failed, id:" + info.ObjectId + ", msg:" + icResponse.HttpStatusCode);
                    }
                }
            }
            catch (AuthenticationFailedException ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
            rs.IsDeleted = true;
            return rs;
        }

        public bool FileExists(StorageInfo info)
        {
            bool result = false;
            string objectId = info.ObjectId;
            if (objectIdRegex.IsMatch(objectId))
            {
                ScspResponse icResponse = GetScspClient().InfoMutable(objectId, string.Empty, GetScspQueryArgs(), new ScspHeaders());
                if (icResponse.HttpStatusCode == (ushort)HttpStatusCode.OK)
                {
                    //logger.Debug("FileExists(), check object succeed, id:" + objectId);
                    result = true;
                }
                else
                {
                    if (icResponse.HttpStatusCode != 404)
                    {
                        throw new Exception("check object with local cluster failed, id:" + objectId + ", msg:" + icResponse.HttpStatusCode);
                    }
                }
            }
            return result;
        }

        public XFileInfo OpenFile(StorageInfo fileInfo)
        {
            XFileInfo result = null;
            long length = GetFileSize(fileInfo);
            result = new CAStorFileInfo(fileInfo.HighName, fileInfo.LowName, length, fileInfo.ObjectId);
            return result;
        }

        public long GetFileSize(StorageInfo info)
        {
            ScspResponse icResponse = GetScspClient().InfoMutable(info.ObjectId, string.Empty, GetScspQueryArgs(), new ScspHeaders());
            if (icResponse.HttpStatusCode == (ushort)HttpStatusCode.OK)
            {
                logger.Info("check object succeed, id:" + info.ObjectId);
                long size = long.Parse(icResponse.ResponseHeaders.GetHeaderValues(CAStorConstants.CONTENT_LENGTH)[0]);
                return size;
            }
            else
            {
                throw new Exception("check object with local cluster failed, id:" + info.ObjectId + ", msg:" + icResponse.HttpStatusCode);
            }
        }

        public ScspResponse InitReadStream(StorageInfo info, string tmpFilePath)
        {
            logger.Info("begin read object:" + info.HighName + "\\" + info.LowName + ", id:" + info.ObjectId);
            //Stream cacheStream = new FileStream(tmpFilePath, FileMode.OpenOrCreate);
            ScspHeaders headers = new ScspHeaders();
            if (info.Length > 0)
            {
                //-1代表不设置end
                headers.AddRange(info.Offset, -1);
            }
            ScspResponse icResponse = GetScspClient().ReadMutable(info.ObjectId, string.Empty, null, GetScspQueryArgs(), headers);
            if (icResponse.HttpStatusCode == (ushort)HttpStatusCode.OK || icResponse.HttpStatusCode == (ushort)HttpStatusCode.PartialContent || icResponse.HttpStatusCode == (ushort)HttpStatusCode.NoContent)
            {
                logger.Info("open object succeed, id:" + info.ObjectId);
                return icResponse;
            }
            else
            {
                throw new Exception("open object failed, msg:" + icResponse.HttpStatusCode);
            }
        }

        /**
        *  From: http://www.ietf.org/rfc/rfc2183.txt
        *  Content-Disposition: filename=genome.jpeg;
        *  modification-date="Wed, 12 Feb 1997 16:29:51 -0500";
        */

        private string GetDiscriptionValue(StorageInfo info)
        {
            string dis = "filename=" + Encode(info.LowName) + "; modification-date=" + NormalizeDate(System.DateTime.Now).ToUniversalTime().ToString("r");    // RFC 1123 standard format specifier
            return dis;
        }

        private string Encode(string str2Encode)
        {
            return HttpUtility.UrlEncode(str2Encode).Replace("+", "%20").Replace("%2f", "/").Replace("%5c", "/");//make .Net Framework4.5 happy
        }

        private DateTime NormalizeDate(DateTime date)
        {
            DateTime utcDate = date.ToUniversalTime();
            return new DateTime(utcDate.Year, utcDate.Month, utcDate.Day, utcDate.Hour, utcDate.Minute, utcDate.Second, DateTimeKind.Utc);
        }

        private void AddDefaultMetaDatas(ScspHeaders headers, Dictionary<string, string> defaultMetas)
        {
            foreach (KeyValuePair<string, string> entry in defaultMetas)
            {
                headers.AddValue("x-" + HttpUtility.UrlEncode(entry.Key) + "-meta", HttpUtility.UrlEncode(entry.Value));
            }
        }

        private void AddCustomizedMetaDatas(ScspHeaders headers, Dictionary<string, string> customMetas)
        {
            foreach (KeyValuePair<string, string> entry in customMetas)
            {
                headers.AddValue("x-" + HttpUtility.UrlEncode(entry.Key) + "-meta", HttpUtility.UrlEncode(entry.Value));
            }
        }

        private void AddExtendedParameters(ScspHeaders headers, Dictionary<string, string> defaultMetaDatas)
        {
            switch (this.OpenParam.CustomizedMetaMode)
            {
                case CustomizedMode.Close:
                    break;
                case CustomizedMode.SupportAll:
                    AddDefaultMetaDatas(headers, defaultMetaDatas);
                    AddCustomizedMetaDatas(headers, this.OpenParam.CustomizedMetaData);
                    break;
                case CustomizedMode.DocAveOnly:
                    AddDefaultMetaDatas(headers, defaultMetaDatas);
                    break;
                case CustomizedMode.CustomizedOnly:
                    AddCustomizedMetaDatas(headers, this.OpenParam.CustomizedMetaData);
                    break;
                default:
                    AddDefaultMetaDatas(headers, defaultMetaDatas);                      //默认仅支持默认的MetaData.
                    break;
            }
        }

        private void HandleCustomMetaDatas(ScspHeaders headers, Dictionary<string, string> defaultMetaDatas)
        {
            bool autoDelete = false;
            long keepTime = 0L;
            long archiveTime = 0l;
            if (defaultMetaDatas != null)
            {
                foreach (KeyValuePair<string, string> entry in defaultMetaDatas)
                {
                    if (entry.Key.EndsWith("Archive-KeepTime".ToLower(CultureInfo.InvariantCulture), StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (long.Parse(entry.Value) > 0)
                        {
                            autoDelete = true;
                            keepTime = long.Parse(entry.Value);
                        }
                    }
                    if (entry.Key.EndsWith("Archive-BackupTime".ToLower(CultureInfo.InvariantCulture), StringComparison.CurrentCultureIgnoreCase))
                    {
                        archiveTime = long.Parse(entry.Value);
                    }
                }
            }

            AddExtendedParameters(headers, defaultMetaDatas);

            if (OpenParam.CompressionType != 0)
            {
                headers.AddValue("Allow-Encoding", "*");
            }
            int mode = this.OpenParam.CompressionType;
            string compressOption = "";
            if (mode == CAStorConstants.BEST_COMPRESS)
            {
                compressOption = "compress=best";
            }
            else if (mode == CAStorConstants.FAST_COMPRESS)
            {
                compressOption = "compress=fast";
            }

            DateTime now = new DateTime(archiveTime);
            DateTime deferCompressTime = now.AddDays(OpenParam.DerferCompresstion);
            DateTime retentionTime = now.AddSeconds(keepTime);
            if (autoDelete)
            {
                if (OpenParam.CompressionType != 0)
                {
                    if (OpenParam.DerferCompresstion > 0)
                    {
                        if (retentionTime > deferCompressTime)
                        {
                            headers.AddLifepoint(ScspLifepoint.DC_NotDeletable, OpenParam.Replication, new ScspDate(System.DateTime.Now.AddDays(OpenParam.DerferCompresstion)));
                        }
                    }
                    headers.AddLifepoint(ScspLifepoint.DC_NotDeletable, OpenParam.Replication, new ScspDate(System.DateTime.Now.AddSeconds(keepTime)), new List<string>() { compressOption });
                }
                else
                {
                    headers.AddLifepoint(ScspLifepoint.DC_NotDeletable, OpenParam.Replication, new ScspDate(System.DateTime.Now.AddSeconds(keepTime)));
                }
                headers.AddLifepoint(ScspLifepoint.DC_MustDelete, 0, null);
            }
            else
            {
                if (OpenParam.CompressionType != 0)
                {
                    if (OpenParam.DerferCompresstion > 0)
                    {
                        headers.AddLifepoint(ScspLifepoint.DC_Deletable, OpenParam.Replication, new ScspDate(System.DateTime.Now.AddDays(OpenParam.DerferCompresstion)));
                    }
                    headers.AddLifepoint(ScspLifepoint.DC_Deletable, OpenParam.Replication, null, new List<string>() { compressOption });
                }
                else
                {
                    headers.AddLifepoint(ScspLifepoint.DC_Deletable, OpenParam.Replication, null);
                }
            }
        }

        private ScspHeaders GetAuthWriteHeaders(StorageInfo info)
        {
            ScspHeaders headers = new ScspHeaders();

            //add contentType
            //in dell dx we use application/octet-stream for content-type
            headers.AddValue(CAStorConstants.CONTENT_TYPE, "application/octet-stream");

            //add contentDiscription
            headers.AddValue(CAStorConstants.CONTENT_DISCRIPUTION, GetDiscriptionValue(info));

            if (this.VimName.Equals(VIMName.CAStor))
            {
                // add header required by dell
                headers.AddValue(CAStorConstants.CREATOR, "AvePoint_DocAve");
                headers.AddValue(CAStorConstants.CREATOR_VERSION, "6;5;0"); //6;0;1 是Docave 版本号， 每次版本升级，需要手动改。
            }
            else if (this.VimName.Equals(VIMName.Caringo))
            {
                headers.AddValue(CAStorConstants.CARINGO_CREATOR, "AvePoint_DocAve");
                headers.AddValue(CAStorConstants.CARINGO_CREATOR_VERSION, "6;0;1");
            }
            else
            {
                throw new Exception(string.Format("Vim Name error, this.VimName={0}", this.VimName));
            }

            // add liftpoint header
            // Lifepoint: [HTTP date for object creating] reps=2, deletable=yes compress=no, best, fast

            // add for cache duration
            headers.AddValue(CAStorConstants.CACHE_TIME_OUT, "0");

            HandleCustomMetaDatas(headers, info.MetaInfos);

            return headers;
        }

        private string GetLiftPointValue()
        {
            ScspLifepoint lifepoint = new ScspLifepoint(ScspLifepoint.DC_Deletable, this.OpenParam.Replication, null, null);

            int mode = this.OpenParam.CompressionType;
            if (mode == CAStorConstants.BEST_COMPRESS)
            {
                lifepoint.CustomAttributes.Add("compress=best");
                logger.Info("set compress to best");
            }
            else if (mode == CAStorConstants.FAST_COMPRESS)
            {
                lifepoint.CustomAttributes.Add("compress=fast");
                logger.Info("set compress to fast");
            }
            return lifepoint.ToString();
        }

        private string GetNoneCompressLifePointValue()
        {
            ScspLifepoint lifepoint = null;
            DateTime datetime = new DateTime().AddDays(this.OpenParam.DerferCompresstion);

            lifepoint = new ScspLifepoint(ScspLifepoint.DC_Deletable, this.OpenParam.Replication, new ScspDate(datetime), null);
            return lifepoint.ToString();
        }

        public string EndWriteStream(Stream cacheStream, StorageInfo info)
        {
            ScspHeaders headers = GetAuthWriteHeaders(info);
            ScspResponse wcResponse;
            if (info.ObjectId.Equals(info.LowName))
            {
                wcResponse = GetScspClient().WriteMutable("", cacheStream, (ulong)cacheStream.Length, GetScspQueryArgs(), headers);
            }
            else
            {
                ScspResponse icResponse = GetScspClient().InfoMutable("", info.ObjectId, GetScspQueryArgs(), headers);
                if (icResponse.HttpStatusCode == (ushort)HttpStatusCode.NotFound)
                {
                    //logger.Debug("UpdateObjectId:{0} not found.", info.ObjectId);
                    wcResponse = GetScspClient().WriteMutable("", cacheStream, (ulong)cacheStream.Length, GetScspQueryArgs(), headers);
                }
                else
                {
                    //logger.Debug("UpdateObjectId:{0} exist, LowName:{1}, Excute UpdateMutable.", info.ObjectId, info.LowName);
                    wcResponse = GetScspClient().UpdateMutable("", info.ObjectId, cacheStream, (ulong)cacheStream.Length, GetScspQueryArgs(), headers);
                }
            }
            if (wcResponse.HttpStatusCode == (ushort)HttpStatusCode.Created)
            {
                string id = wcResponse.ResponseHeaders.GetHeaderValues(CAStorConstants.OBJCET_UUID)[0];
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("create object succeed, name:" + info.HighName + info.LowName + ", id:" + id);
                }
                return id;
            }
            else
            {
                throw new Exception("execute request failed,msg:" + wcResponse.HttpStatusCode);
            }
        }

        public bool UpdateObjectMeta(string objectId, Dictionary<string, string> headers)
        {
            logger.Info("begin update object:" + objectId);
            ScspResponse reponse = GetScspClient().InfoMutable(objectId, string.Empty, GetScspQueryArgs(), new ScspHeaders());
            if ((HttpStatusCode)reponse.HttpStatusCode == HttpStatusCode.OK)
            {
                ScspHeaders sheaders = reponse.ResponseHeaders;
                //sheaders.Remove("Lifepoint");
                sheaders.Remove("Content-Length");
                sheaders.Remove("Date");

                foreach (var item in headers)
                {
                    sheaders.AddValue(item.Key, item.Value);
                }
                reponse = GetScspClient().CopyMutable(objectId, string.Empty, GetScspQueryArgs(), sheaders);
                if ((HttpStatusCode)reponse.HttpStatusCode == HttpStatusCode.OK || reponse.HttpStatusCode == Convert.ToUInt16(HttpStatusCode.Created))
                {
                    logger.Info("update object succeed:" + objectId);
                    return true;
                }
                else
                {
                    throw new Exception("update object failed:" + reponse.HttpStatusCode);
                }
            }
            else
            {
                throw new Exception("update object failed:" + reponse.HttpStatusCode);
            }

        }

        public string GetNextMetaId(StorageInfo info)
        {
            ScspResponse icResponse = GetScspClient().InfoMutable(info.ObjectId, string.Empty, GetScspQueryArgs(), new ScspHeaders());
            if (icResponse.HttpStatusCode == (ushort)HttpStatusCode.OK)
            {
                logger.Info("query object succeed, id:" + info.ObjectId);
                if (icResponse.ResponseHeaders.GetHeaderValues(CAStorConstants.META_ID_HEADER) != null)
                {
                    return icResponse.ResponseHeaders.GetHeaderValues(CAStorConstants.META_ID_HEADER)[0];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new Exception("check object with local cluster failed, id:" + info.ObjectId + ", msg:" + icResponse.HttpStatusCode);
            }
        }

        public void Close()
        {
            try
            {
                if (this.scspClient != null)
                {
                    this.scspClient.Stop();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
            }
        }

        public Type[] GetTypes(object[] objs)
        {
            if (objs == null)
            {
                return null;
            }
            Type[] types = new Type[objs.Length];
            for (int i = 0; i < objs.Length; i++)
            {
                types[i] = objs[i].GetType();
            }
            return types;
        }

        public void SetAvalableScspClient(string id, CAStorOpenParameter param)
        {
            param.BeginCacheSecondaryTime = DateTime.UtcNow;
            logger.Debug("beginCacheSecondaryTime={0}", param.BeginCacheSecondaryTime);
            cacheOpenParams[id] = param;
        }

        public object Invoke(string methodName, object[] objs)
        {
            logger.Debug("InvokeMethodName: " + methodName);
            object result = null;
            Type[] types = GetTypes(objs);
            MethodInfo methodInfo;
            if (types == null)
            {
                methodInfo = this.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.ExactBinding | BindingFlags.Static);
            }
            else
            {
                methodInfo = this.GetType().GetMethod(methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.ExactBinding | BindingFlags.Static,
                                             null, CallingConventions.Any, types, null);
                if (methodInfo == null)
                {
                    methodInfo = this.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.ExactBinding | BindingFlags.Static);
                }
            }

            try
            {
                result = methodInfo.Invoke(this, objs);
            }
            catch (Exception ex)
            {
                logger.Error("DELL invoke Error:{0}", ex);
                if (this.OpenParam.UseRemoteCluster && !cacheOpenParams.ContainsKey(this.OpenParam.PhysicalIdAndMidifyTime))
                {
                    this.OpenParam.IsLocalClientFailed = true;
                    if (this.scspClient != null)
                    {
                        this.scspClient.Stop();
                        this.scspClient = null;
                    }

                    result = methodInfo.Invoke(this, objs);
                    if (OpenParam.CacheSecondary && OpenParam.SecondaryTimeout != 0)
                    {
                        SetAvalableScspClient(OpenParam.PhysicalIdAndMidifyTime, OpenParam);
                    }
                    logger.Debug("InvokeMethodName: " + methodName + ", Retry Invoke succeed.");
                }
                else
                {
                    throw;
                }
            }
            return result;
        }
    }
}
