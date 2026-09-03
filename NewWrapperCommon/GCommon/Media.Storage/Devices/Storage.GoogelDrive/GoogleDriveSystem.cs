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

namespace AvePoint.Media.Storage.GoogleDrive
{
    #region using directives
    using AvePoint.GCommon;
    using AvePoint.Media.Storage.Inner;
    using AvePoint.Media.Storage.Util;
    using Resources.GoogleDriveI18N;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Script.Serialization;
    #endregion

    class GoogleDriveSystem : AbstractXSystem
    {

        #region Field and property

        private AveLogger logger = AveLogger.GetInstance(typeof(GoogleDriveSystem));
        private static string GoogleDriveDefaultClientId = "145176449998-bhscqara60tsb75a7gbcrfc9g02793m2.apps.googleusercontent.com";
        public GoogleDriveOpenParameter OpenParameter { get; set; }
        public string SignInURL
        {
            get
            {
                return String.Format(StorageUrl.GoogleDriveSignIn, this.OpenParameter.ClientId);
            }
        }
        public override StorageInterfaceType StorageInterfaceType
        {
            get
            {
                return StorageInterfaceType.Object;
            }
        }

        public override string Type
        {
            get
            {
                return "GoogleDriveSystem";
            }
        }
        public string LastMetaId { get; set; }
        public string LastContentId { get; set; }
        public StorageInfo LastStreamInfo { get; set; }
        public delegate T RetryDelegate<T>();

        #endregion

        static GoogleDriveSystem()
        {
            ServicePointManager.DefaultConnectionLimit = 512;
            ServicePointManager.ServerCertificateValidationCallback =
                            new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public GoogleDriveSystem(string xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            this.SystemHealth = XSystemHealth.Unknown;
            this.IsSupportAutoChangeDataBlock = true;
            this.SpaceThresholdUnit = SpaceThresholdUnit.MB;
            this.OpenParameter = new GoogleDriveOpenParameter();
            this.Open();
        }


        #region Method

        public override StorageOpenValidResult Open()
        {
            if (this.SystemHealth != XSystemHealth.Unknown)
            {
                return new StorageOpenValidResult();
            }
            base.Open();
            if (XriObject.Params.ContainsKey(XRIParameterKeys.GOOGLEDRIVE_CUSTOMIZED_APP))
            {
                var isCustomizedApp = bool.Parse(XriObject.Params[XRIParameterKeys.GOOGLEDRIVE_CUSTOMIZED_APP]);
                if (!isCustomizedApp)
                {
                    XriObject.Params[XRIParameterKeys.Client_ID] = "145176449998-bhscqara60tsb75a7gbcrfc9g02793m2.apps.googleusercontent.com";
                    XriObject.Params[XRIParameterKeys.Client_Secret] = SecretUtil.EncryptPassword("57a1JYFBohKda3hhlxfZQ4No");
                }
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.REFRESH_TOKEN))
            {
                this.OpenParameter.RefreshToken = SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.REFRESH_TOKEN]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.BLOCK_LENGTH))
            {
                this.OpenParameter.BlockLength = int.Parse(XriObject.Params[XRIParameterKeys.BLOCK_LENGTH]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Client_ID))
            {
                this.OpenParameter.ClientId = XriObject.Params[XRIParameterKeys.Client_ID];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Client_Secret))
            {
                this.OpenParameter.ClientSecret = SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.Client_Secret]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Root_Folder_Id))
            {
                this.OpenParameter.RootFolderId = XriObject.Params[XRIParameterKeys.Root_Folder_Id];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Root_Folder_Name))
            {
                this.OpenParameter.RootFolderName = XriObject.Params[XRIParameterKeys.Root_Folder_Name];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_INFO))
            {
                this.Proxy = new WebProxy(XriObject.Params[XRIParameterKeys.PROXY_INFO]);//兼容老数据
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.GOOGLEDRIVE_PROXY_SETTING) && Boolean.Parse(XriObject.Params[XRIParameterKeys.GOOGLEDRIVE_PROXY_SETTING]))
            {
                if (XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_IP) && XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_PORT))
                {
                    var ProxyIp = XriObject.Params[XRIParameterKeys.PROXY_IP];
                    var ProxyPort = int.Parse(XriObject.Params[XRIParameterKeys.PROXY_PORT]);
                    this.Proxy = new WebProxy(ProxyIp, ProxyPort);
                    if (XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_USERNAME) && XriObject.Params.ContainsKey(XRIParameterKeys.PROXYPASSWORD))
                    {
                        var userName = this.XriObject.Params[XRIParameterKeys.PROXY_USERNAME];
                        var password = SecretUtil.DescryptPassword(this.XriObject.Params[XRIParameterKeys.PROXYPASSWORD]);
                        var nameAndDomain = userName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                        if (nameAndDomain.Length > 1)
                            this.Proxy.Credentials = new NetworkCredential(nameAndDomain[1], password, nameAndDomain[0]);
                        else
                            this.Proxy.Credentials = new NetworkCredential(userName, password);
                    }
                }
            }
            this.SystemHealth = XSystemHealth.AvailableAndNotFull;
            SetSystemDescription();
            return new StorageOpenValidResult();
        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "GoogleDrive Object Storage Server";
            List<string> keys = new List<string>();
            keys.Add(this.OpenParameter.RootFolderId);
            keys.Add(this.OpenParameter.ClientId);
            List<string> securityKeys = new List<string>();
            keys.Add(this.OpenParameter.ClientSecret);
            keys.Add(this.OpenParameter.RefreshToken);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        internal HttpWebRequest GenerateRequest(String url, String method)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = method;
            if (this.Proxy != null)
            {
                request.Proxy = this.Proxy;
                if (request.Proxy.Credentials != null)
                {
                    request.PreAuthenticate = true;
                }
            }
            return request;
        }

        public SpaceInfo CheckFreeSpace()
        {
            return Retry<SpaceInfo>(delegate()
            {
                SpaceInfo spaceInfo = new SpaceInfo();
                string url = String.Format(StorageUrl.GoogleDriveAbout, GetAccessToken());
                HttpWebRequest req = GenerateRequest(url, "GET");
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("CheckFreeSpace failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                    }
                    Stream respStream = resp.GetResponseStream();
                    using (StreamReader sr = new StreamReader(respStream))
                    {
                        string quotaStr = sr.ReadToEnd();
                        Regex r = new Regex("\"quotaBytesTotal\"\\:\\s*\"([0-9]+)\"[^\"]*\"quotaBytesUsed\"\\:\\s*\"([0-9]+)\"");
                        Match m = r.Match(quotaStr);
                        if (!m.Success)
                        {
                            throw new Exception("Match quota string failed, quota=" + quotaStr);
                        }
                        spaceInfo.TotalSpace = ulong.Parse(m.Groups[1].Value);
                        spaceInfo.TotalUsedSpace = ulong.Parse(m.Groups[2].Value);
                        spaceInfo.TotalFreeSpace = spaceInfo.TotalSpace - spaceInfo.TotalUsedSpace;
                    }
                }
                req.Abort();
                return spaceInfo;
            });
        }

        public override StorageOpenValidResult Validate()
        {
            CheckState();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            StorageOpenValidResult result = null;
            try
            {
                result = new StorageOpenValidResult();
                if (string.IsNullOrEmpty(this.OpenParameter.RootFolderId))
                {
                    logger.Info("try to get id for folder : " + this.OpenParameter.RootFolderName);
                    this.OpenParameter.RootFolderId = GetRootFolderId();
                    logger.Info("get id for folder {0} succeed, ID : {1}", this.OpenParameter.RootFolderName, this.OpenParameter.RootFolderId);
                    this.Properties[XRIParameterKeys.AppendConnectionStringKey] = "&" + XRIParameterKeys.Root_Folder_Id + "=" + this.OpenParameter.RootFolderId;
                }
                else
                {
                    this.Properties[XRIParameterKeys.AppendConnectionStringKey] = "&" + XRIParameterKeys.Root_Folder_Id + "=" + this.OpenParameter.RootFolderId;
                }
                SpaceInfo spaceInfo = CacheUtil.GetSpaceInfo(VIMName.GoogleDrive, this.XriString, new CheckFreeSpace(this.CheckFreeSpace));
                result.TotalSpace = this.innerTotalSpace = spaceInfo.TotalSpace;
                result.TotalFreeSpace = this.innerTotalFreeSpace = spaceInfo.TotalFreeSpace;
                result.TotalUsedSpace = this.innerTotalUsedSpace = spaceInfo.TotalUsedSpace;
                result.IsReadAble = true;
                result.IsDeleteAble = true;
                result.IsHasPermission = true;
                if (ValidateIsFull())
                {
                    result.SystemHealth = XSystemHealth.Available;
                }
                else
                {
                    result.IsWriteAble = true;
                    result.SystemHealth = XSystemHealth.AvailableAndNotFull;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Validate Error:", ex);
                result.Message = ex.Message;
                result.SystemHealth = XSystemHealth.AuthenticationFailed;
            }
            finally
            {
                this.SystemHealth = result.SystemHealth;
            }
            return result;
        }

        private string GetRootFolderId()
        {
            return Retry<string>(delegate()
            {
                string result = null;
                string jsonStr = null;
                string encodingStr = HttpUtility.UrlEncode("title = '" + this.OpenParameter.RootFolderName + "'");
                string url = String.Format(StorageUrl.GoogleDriveQueryFolder, GetAccessToken(), encodingStr);
                HttpWebRequest req = GenerateRequest(url, "GET");
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception("GetRootFolderID method failed");
                    }
                    using (Stream respStream = resp.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            jsonStr = sr.ReadToEnd();
                            logger.Debug("get folder info:" + jsonStr);
                            result = ParseJsonString(jsonStr);
                            if (string.IsNullOrEmpty(result))
                            {
                                result = CreateRootFolder(this.OpenParameter.RootFolderName);
                            }
                        }
                    }
                }
                req.Abort();
                return result;
            });
        }

        private string ParseJsonString(string jsonString)
        {
            var idStr = string.Empty;
            var isRoot = false;
            var typeStr = string.Empty;
            var serializer = new JavaScriptSerializer();
            var JsonData = (Dictionary<string, object>)serializer.DeserializeObject(jsonString);
            object[] rows = (object[])JsonData["items"];
            foreach (object obj in rows)
            {
                var val = (Dictionary<string, object>)obj;
                foreach (var pair in val)
                {
                    if (pair.Key.Equals("id", StringComparison.OrdinalIgnoreCase))
                    {
                        idStr = ((string)pair.Value);
                    }
                    if (pair.Key.Equals("mimeType", StringComparison.OrdinalIgnoreCase))
                    {
                        typeStr = ((string)pair.Value);
                    }
                    if (pair.Key.Equals("parents", StringComparison.OrdinalIgnoreCase))
                    {
                        object[] parents = (object[])pair.Value;
                        var parent = (Dictionary<string, object>)parents[0];
                        foreach (var entry in parent)
                        {
                            if (entry.Key.Equals("isRoot", StringComparison.OrdinalIgnoreCase))
                            {
                                isRoot = (bool)entry.Value;
                                if (isRoot && typeStr.EndsWith("folder", StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }
                            }
                        }

                    }
                }
            }
            return isRoot ? idStr : string.Empty;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "vnd")]
        private string CreateRootFolder(string folderName)
        {
            return Retry<string>(delegate()
            {
                string result = null;
                StringBuilder sb = new StringBuilder();
                string url = StorageUrl.GoogleDriveNormal;
                HttpWebRequest req = GenerateRequest(url, "POST");
                req.Headers.Add("Authorization", "Bearer " + GetAccessToken());
                req.ContentType = "application/json";
                sb.Append("{ title: \"")
                  .Append(folderName).Append("\",")
                  .Append("\"mimeType\": \"application/vnd.google-apps.folder\"")
                  .Append("}");
                using (Stream reqStream = req.GetRequestStream())
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
                    reqStream.Write(buffer, 0, buffer.Length);
                }
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.Created && resp.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("CreateRootFolder failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                    }
                    Stream respStream = resp.GetResponseStream();
                    using (StreamReader sr = new StreamReader(respStream))
                    {
                        string jsonStr = sr.ReadToEnd();
                        logger.Debug("get folder info after create:" + jsonStr);
                        result = GoogleDriveUtil.GetNewRootFolderId(jsonStr);
                    }
                }
                req.Abort();
                return result;
            });
        }

        private void ResetToken()
        {
            if (string.IsNullOrEmpty(this.OpenParameter.RefreshToken))
            {
                throw new Exception("RefreshToken IsNullOrEmpty.");
            }
            else if (this.OpenParameter.ClientId.Equals(GoogleDriveDefaultClientId, StringComparison.OrdinalIgnoreCase))
            {
                var url = string.Format("https://api.avepointonlineservices.com/api/cloudtoken/GetRefreshToken?refreshToken={0}&deviceType=googledrive", HttpUtility.UrlEncode(XRI.ValueDecode(SecretUtil.EncryptPassword(this.OpenParameter.RefreshToken))));
                var result = RefreshAccessTokenFromAos(url);
                if (!string.IsNullOrEmpty(result) && !result.Contains("Error"))
                {
                    //由于AOS API 返回的字符串中，会自动走Jason序列化，在前后增加"，所以这里需要手动remove
                    this.OpenParameter.AccessToken = new AccessToken(DateTime.Now, SecretUtil.DescryptPassword(result.Replace("\"", String.Empty)));
                }
                else
                {
                    this.logger.Info("GoogleDrive server return error : " + result);
                    var errorMsg = "";
                    if (result.Contains("unauthorized_client") || result.Contains("invalid_grant"))
                    {
                        errorMsg = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_UnauthorizedRefeshToken", AbstractXSystem.Culture);
                    }
                    else
                    {
                        errorMsg = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_ValidationTestFiled", AbstractXSystem.Culture);
                    }
                    throw new Exception(errorMsg);
                }
            }
            else
            {
                string body = string.Format("client_id={0}&client_secret={1}&grant_type=refresh_token&refresh_token={2}",
                                this.OpenParameter.ClientId, this.OpenParameter.ClientSecret, this.OpenParameter.RefreshToken);
                SetToken(body);
            }
        }

        public void SignIn(string code)
        {
            logger.Debug("SignIn code:" + code);
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("Authentication code IsNullOrEmpty");
            }
            string body = string.Format("client_id={0}&redirect_uri={1}&client_secret={2}&code={3}&grant_type=authorization_code",
                                    this.OpenParameter.ClientId, GoogleDriveConstant.RedirectDomain, this.OpenParameter.ClientSecret, code);
            SetToken(body);
        }

        public string GetAccessToken()
        {
            if (OpenParameter.AccessToken == null || OpenParameter.AccessToken.IsTimeOut)
            {
                ResetToken();
            }
            return this.OpenParameter.AccessToken.TokenString;
        }

        private bool SetToken(string body)
        {
            string url = StorageUrl.GoogleDriveSetToken;
            HttpWebRequest req = GenerateRequest(url, "POST");
            req.ContentType = "application/x-www-form-urlencoded";
            using (Stream reqStream = req.GetRequestStream())
            {
                byte[] buffer = Encoding.ASCII.GetBytes(body);
                reqStream.Write(buffer, 0, buffer.Length);
            }
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                if (resp.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(string.Format("SetToken failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                }
                Stream respStream = resp.GetResponseStream();
                using (StreamReader sr = new StreamReader(respStream))
                {
                    string token = sr.ReadToEnd();
                    Regex r = new Regex("\"access_token\"\\s*\\:\\s*\"([^\"]+)\"");
                    Match m = r.Match(token);
                    if (m.Success)
                    {
                        this.OpenParameter.AccessToken = new AccessToken(DateTime.Now, m.Groups[1].Value);
                    }
                    else
                    {
                        throw new Exception("Match token failed, tokenStr = " + token);
                    }


                }
            }
            req.Abort();
            return true;

        }

        public string[] SplitFileID(string fileID)
        {
            return fileID.Split(new string[] { GoogleDriveConstant.FILE_ID_SEPARATOR }, StringSplitOptions.None);
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            CheckState();
            StorageDeleteResult result = new StorageDeleteResult();
            //标记执行过删除
            Deletion = true;
            return result;
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            CheckState();
            GoogleDriveStream stream = null;
            if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew || fileMode == FileMode.Truncate || fileMode == FileMode.OpenOrCreate)
            {
                this.Written = true;
                try
                {
                    stream = new GoogleDriveStream(info, this);
                    stream.InitWriteStream(info);
                    this.LastStreamInfo = info.Clone();
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                    throw;
                }
                return stream;
            }
            else
            {
                return DownloadFile(info);
            }
        }

        public XStream DownloadFile(StorageInfo info)
        {
            string fileID = info.ObjectId;
            long initOffset = info.Offset;
            XStream googleDriveStream = null;
            bool isBlockRead = false;
            string[] fileIDs = SplitFileID(fileID);
            string tempFileID = string.Empty;
            Queue<string> fileIDQueue = null;
            if (fileIDs.Length > 1)
            {
                fileIDQueue = new Queue<string>(fileIDs);
                for (int i = 0; i < (initOffset / this.OpenParameter.EachBlockLength); i++)
                {
                    fileIDQueue.Dequeue();
                }
                tempFileID = fileIDQueue.Peek();
                if (fileIDs.Length > 1)
                {
                    initOffset = initOffset % this.OpenParameter.EachBlockLength;
                }
            }
            else
            {
                tempFileID = fileIDs[0];
            }

            Stream respStream = GetDownloadStream(tempFileID, initOffset);
            #region new GoogleDriveStream
            googleDriveStream = new GoogleDriveStream(this, info, respStream, (buffer, offset, count) =>
            {
                int readLen = 0;
                try
                {
                    readLen = respStream.Read(buffer, offset, count);
                    if (readLen <= 0 && fileIDQueue != null && fileIDQueue.Count > 1)
                    {
                        isBlockRead = true;
                        fileIDQueue.Dequeue();
                        tempFileID = fileIDQueue.Peek();
                        respStream.Close();
                        respStream = GetDownloadStream(tempFileID);
                        readLen = respStream.Read(buffer, offset, count);
                    }
                }
                catch (IOException)
                {
                    if (isBlockRead)
                    {
                        initOffset = initOffset % this.OpenParameter.EachBlockLength;
                    }
                    respStream.Close();
                    respStream = GetDownloadStream(tempFileID, initOffset);
                    readLen = respStream.Read(buffer, offset, count);
                }
                initOffset += readLen;
                return readLen;
            });
            #endregion

            return googleDriveStream;
        }

        private Stream GetDownloadStream(string fileID, long initOffset = 0)
        {
            return Retry<Stream>(delegate()
            {
                string jsonStr = ReadFileProperties(fileID);

                Regex r = new Regex("\"downloadUrl\\\"\\s*\\:\\s*\"([^\"]+)\"");
                Match m = r.Match(jsonStr);
                if (!m.Success)
                {
                    throw new Exception("Match jsonStr failed, jsonStr = " + jsonStr);
                }
                string downloadUrl = m.Groups[1].Value;

                HttpWebRequest req = GenerateRequest(downloadUrl, "GET");
                req.Headers.Add("Authorization", "Bearer " + GetAccessToken());
                if (initOffset > 0)
                {
                    AddHeadersWithoutValidate(req, "Range", "bytes=" + initOffset + "-");
                }

                HttpWebResponse resp = req.GetResponse() as HttpWebResponse; //Can't close HttpWebResponse.
                return resp.GetResponseStream();
            });
        }

        private void AddHeadersWithoutValidate(HttpWebRequest req, string key, string value)
        {
            MethodInfo method = req.Headers.GetType().GetMethod("AddWithoutValidate",
                                    BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null,
                                    new Type[] { typeof(string), typeof(string) }, null);
            method.Invoke(req.Headers, new object[] { key, value });
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            CheckState();
            StorageDeleteResult rs = new StorageDeleteResult();
            try
            {
                foreach (string objId in info.ObjectIds)
                {
                    if (!string.IsNullOrEmpty(objId))
                    {
                        try
                        {
                            DeleteUploadedFile(new StorageInfo() { ObjectId = objId }, rs);
                        }
                        catch (PathNotFoundException)
                        {
                            logger.Info("file already not exists, fileID=" + objId);
                        }
                    }
                }
                rs.IsDeleted = true;
            }
            catch (Exception e)
            {
                logger.Error("delete object failed, id:" + info.ObjectId + ", msg:" + e.Message);
                throw;
            }
            Deletion = true;
            return rs;
        }

        private void DeleteUploadedFile(StorageInfo info, StorageDeleteResult rs)
        {
            if (FileExists(info))
            {
                string nextMetaID = GetNextMetaID(info.ObjectId);
                if (!string.IsNullOrEmpty(nextMetaID))
                {
                    DeleteUploadedFile(new StorageInfo() { ObjectId = nextMetaID }, rs);
                }
                rs.DeletedFileSize += OpenFile(info).FileSize;
                Retry<bool>(delegate()
                {
                    foreach (var id in SplitFileID(info.ObjectId))
                    {
                        string url = String.Format(StorageUrl.GoogleDriveFile, id, GetAccessToken());
                        HttpWebRequest req = GenerateRequest(url, "DELETE");
                        using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                        {
                            if (resp.StatusCode != HttpStatusCode.NoContent)
                            {
                                throw new Exception(string.Format("DeleteFile failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                            }
                        }
                        req.Abort();
                    }
                    return true;
                });
            }
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            CheckState();
            long fileSize = 0;
            string[] ids = SplitFileID(fileInfo.ObjectId);
            foreach (string fileID in ids)
            {
                string propertiesStr = ReadFileProperties(fileID);
                Regex r = new Regex("\"fileSize\"\\:\\s*\"([0-9]+)\"");
                Match m = r.Match(propertiesStr);
                if (!m.Success)
                {
                    throw new Exception(string.Format(
                        "Match properties string failed, fileID={0}, properties={1}", fileID, propertiesStr));
                }
                fileSize += long.Parse(m.Groups[1].Value);
            }
            return new GoogleDriveFileInfo(fileInfo.HighName, fileInfo.LowName, fileSize, fileInfo.ObjectId);
        }

        public string GetNextMetaID(string fileID)
        {
            string result = null;
            string propertiesString = ReadFileProperties(fileID);
            Regex regex = new Regex("\\'" + GoogleDriveConstant.META_ID_HEADER + "\\'\\:\\s*\\'([^\\']+)'");
            Match match = regex.Match(propertiesString);
            if (match.Success)
            {
                result = match.Groups[1].Value;
            }
            return result;
        }

        public string ReadFileProperties(string fileID)
        {
            return Retry<string>(delegate()
            {
                string result = null;
                string url = String.Format(StorageUrl.GoogleDriveFile, SplitFileID(fileID)[0], GetAccessToken());
                HttpWebRequest req = GenerateRequest(url, "GET");
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("ReadFileProperties failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                    }
                    using (Stream respStream = resp.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            result = sr.ReadToEnd();
                        }
                    }
                }
                req.Abort();
                return result;
            });

        }

        public override void MergeStorageInfo<T>(List<T> indexList, StorageResult result, PropertyInfo propertyInfo)
        {
            CheckState();
            if (!string.IsNullOrEmpty(result.StorageInfo))
            {
                string value = null;
                GoogleDriveStorageInfo casInfo = GoogleDriveUtil.Convert2CAStorStorageInfo(result.StorageInfo);
                if (this.LastStreamInfo != null && this.LastStreamInfo.DataType == DataBlockType.MetaData)
                {
                    foreach (T index in indexList)
                    {
                        value = propertyInfo.GetValue(index, null) as string;
                        GoogleDriveStorageInfo cas = GoogleDriveUtil.Convert2CAStorStorageInfo(value);
                        if (string.IsNullOrEmpty(cas.MetaId))
                        {
                            cas.MetaId = casInfo.ContentId;
                            propertyInfo.SetValue(index, GoogleDriveUtil.Convert2StorageInfo(cas), null);
                        }
                    }
                    result.NeedCommit = true;
                }
                else
                {
                    foreach (T index in indexList)
                    {
                        value = propertyInfo.GetValue(index, null) as string;
                        GoogleDriveStorageInfo cas = GoogleDriveUtil.Convert2CAStorStorageInfo(value);
                        if (string.IsNullOrEmpty(cas.ContentId))
                        {
                            cas.ContentId = casInfo.ContentId;
                            propertyInfo.SetValue(index, GoogleDriveUtil.Convert2StorageInfo(cas), null);
                        }
                    }
                }
            }
        }

        public override bool FileExists(StorageInfo info)
        {
            CheckState();
            try
            {
                return Retry<bool>(delegate()
                {
                    string url = String.Format(StorageUrl.GoogleDriveFile, SplitFileID(info.ObjectId)[0], GetAccessToken());
                    HttpWebRequest req = GenerateRequest(url, "GET");
                    using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                    {
                        if (resp.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception(string.Format("ReadFileProperties failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                        }
                    }
                    req.Abort();
                    return true;
                });
            }
            catch (PathNotFoundException)
            {
                return false;
            }
        }

        public override void Close()
        {
        }

        public T Retry<T>(RetryDelegate<T> del)
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
                    if (counter > this.MaxRetryCount)
                    {
                        logger.Error("Too many retry failed. Retry count:{0}, error message:{1}", counter, ex);
                        throw;
                    }
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        HttpWebResponse response = ex.Response as HttpWebResponse;
                        if (response.StatusCode == HttpStatusCode.Unauthorized
                            || (int)response.StatusCode == 420)
                        {
                            ResetToken();
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new PathNotFoundException(ex.Message, ex);
                        }
                        else if (response.StatusCode == HttpStatusCode.InternalServerError || response.StatusCode == HttpStatusCode.RequestTimeout || response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            logger.Info("this exception is a connection fail exception:" + ex.Message);
                            if (counter < this.MaxRetryCount)
                            {
                                logger.Info("Retry after " + this.RetryInterval + " ms. Retry count: " + counter);
                                Thread.Sleep(this.RetryInterval);
                            }
                            else
                            {
                                throw;
                            }
                        }
                        else
                        {
                            string body = string.Empty;
                            using (Stream respStream = response.GetResponseStream())
                            {
                                using (StreamReader sr = new StreamReader(respStream))
                                {
                                    body = sr.ReadToEnd();
                                }
                            }
                            logger.Error("execute request failed, details : {0}, response body:{1}:", ex, body);
                            throw;
                        }
                    }
                    else if (ex.Status == WebExceptionStatus.ConnectionClosed || ex.Status == WebExceptionStatus.ConnectFailure || ex.Status == WebExceptionStatus.NameResolutionFailure || ex.Status == WebExceptionStatus.Timeout)
                    {
                        logger.Info("this exception is a connection fail exception:" + ex.Message);
                        if (counter < this.MaxRetryCount)
                        {
                            logger.Info("Retry after " + this.RetryInterval + " ms. Retry count: " + counter);
                            Thread.Sleep(this.RetryInterval);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        logger.Error("execute request failed:" + ex);
                        throw;
                    }
                }
            }
        }

        #endregion

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            throw new NotSupportedException();
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        public override bool DirectoryExists(StorageInfo info)
        {
            throw new NotSupportedException();
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            throw new NotSupportedException();
        }

        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            throw new NotSupportedException();
        }

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        {
            throw new NotSupportedException();
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }
    }
}
