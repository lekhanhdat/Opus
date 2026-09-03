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
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.OneDrive.OneDriveSystem.#GetUploadUrl(System.String)", MessageId = "onedrive")]
namespace AvePoint.Media.Storage.OneDrive
{
    #region reference
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Inner;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    #endregion
    #region CodeReview
    [AveCodeReview(
    "2012/9/13",
    "rongbiao.sun@avepoint.com",
    "nan.shen@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
     null,
     true)]
    #endregion
    #region CodeReview
    [AveCodeReview(
        "2013/12/12",
        "xiao.zhang@avepoint.com",
        "xiao.zhang@avepoint.com",
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 },
        "ADO-103897",
        true)]
    #endregion
    class OneDriveSystem : AbstractXSystem
    {
        #region Field and property
        String objectID = String.Empty;
        private AveLogger logger = AveLogger.GetInstance(typeof(OneDriveSystem));
        private static string OneDriveDefaultClientId = "0000000044122D4D";
        public OneDriveOpenParameter OpenParameter { get; set; }
        public String SignInURL
        {
            get
            {
                return String.Format(StorageUrl.OneDriveSignIn, this.OpenParameter.ClientId, OneDriveConstant.SCOPES, OneDriveConstant.RESPONSE_TYPE, HttpUtility.UrlEncode(this.OpenParameter.RedirectDomain)); ;
            }
        }
        public override String Type
        {
            get
            {
                return "SkyDriveSystem";
            }
        }
        public delegate T RetryDelegate<T>();
        public String LastMetaId { get; set; }
        public String LastContentId { get; set; }
        public StorageInfo LastStreamInfo { get; set; }
        public override StorageInterfaceType StorageInterfaceType
        {
            get
            {
                return StorageInterfaceType.Object;
            }
        }
        #endregion

        static OneDriveSystem()
        {
            ServicePointManager.DefaultConnectionLimit = 1024;
            ServicePointManager.ServerCertificateValidationCallback =
                            new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public OneDriveSystem(String xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            this.SystemHealth = XSystemHealth.Unknown;
            this.IsSupportAutoChangeDataBlock = true;
            this.SpaceThresholdUnit = SpaceThresholdUnit.MB;
            this.OpenParameter = new OneDriveOpenParameter();
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
            if (XriObject.Params.ContainsKey(XRIParameterKeys.REFRESH_TOKEN))
            {
                this.OpenParameter.RefreshToken = SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.REFRESH_TOKEN]);
                //this.OpenParameter.RefreshToken = XriObject.Params[XRIParameterKeys.REFRESH_TOKEN];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.ONEDRIVE_CUSTOMIZED_APP))
            {
                var isCustomizedApp = bool.Parse(XriObject.Params[XRIParameterKeys.ONEDRIVE_CUSTOMIZED_APP]);
                if (!isCustomizedApp)
                {
                    XriObject.Params[XRIParameterKeys.Client_ID] = "0000000044122D4D";
                    XriObject.Params[XRIParameterKeys.Client_Secret] = SecretUtil.EncryptPassword("zUdFpbAuyf2GueZwyYQ7NC-OggdDdqGl");
                    XriObject.Params[XRIParameterKeys.Redirect_Domain] = "https://www.avepointonlineservices.com/getcloudtoken/onedrive";
                }
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
                //this.OpenParameter.ClientSecret = XriObject.Params[XRIParameterKeys.Client_Secret];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Redirect_Domain))
            {
                this.OpenParameter.RedirectDomain = XriObject.Params[XRIParameterKeys.Redirect_Domain];
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
            if (XriObject.Params.ContainsKey(XRIParameterKeys.ONEDRIVE_PROXY_SETTING) && Boolean.Parse(XriObject.Params[XRIParameterKeys.ONEDRIVE_PROXY_SETTING]))
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
            this.Properties[SystemPropertyKeys.SystemDescriptionKey] = "OneDrive Object Storage Server";
            var keys = new List<String>();
            keys.Add(this.OpenParameter.ClientId);
            keys.Add(this.OpenParameter.RedirectDomain);
            var securityKeys = new List<String>();
            keys.Add(this.OpenParameter.ClientSecret);
            keys.Add(this.OpenParameter.RefreshToken);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        public SpaceInfo CheckFreeSpace()
        {
            return Retry<SpaceInfo>(delegate()
            {
                SpaceInfo spaceInfo = new SpaceInfo();
                var url = String.Format(StorageUrl.OneDriveQuota, GetAccessToken());
                HttpWebRequest request = GenerateRequest(url, "GET");
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(String.Format("CheckFreeSpace failed, StatusCode={0} URL={1}", response.StatusCode, url));
                    }
                    Stream respStream = response.GetResponseStream();
                    using (StreamReader streamreader = new StreamReader(respStream))
                    {
                        var quotaStr = streamreader.ReadToEnd();
                        Regex regex = new Regex("\"quota\"\\:\\s*([0-9]+)[^\"]*\"available\"\\:\\s*([0-9]+)");
                        Match match = regex.Match(quotaStr);
                        if (!match.Success)
                        {
                            throw new Exception("Match quota string failed, quota=" + quotaStr);
                        }
                        spaceInfo.TotalSpace = ulong.Parse(match.Groups[1].Value);
                        spaceInfo.TotalFreeSpace = ulong.Parse(match.Groups[2].Value);
                        spaceInfo.TotalUsedSpace = spaceInfo.TotalSpace - spaceInfo.TotalFreeSpace;
                    }
                }
                request.Abort();
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
                if (String.IsNullOrEmpty(this.OpenParameter.RootFolderId))
                {
                    this.OpenParameter.RootFolderId = GetRootFolderId();
                }
                this.Properties[XRIParameterKeys.AppendConnectionStringKey] = "&" + XRIParameterKeys.Root_Folder_Id + "=" + this.OpenParameter.RootFolderId;
                SpaceInfo spaceInfo = CacheUtil.GetSpaceInfo(VIMName.SkyDrive, this.XriString, new CheckFreeSpace(this.CheckFreeSpace));
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
                logger.Error("Validate Error: {0}", ex);
                result.SystemHealth = XSystemHealth.AuthenticationFailed;
            }
            finally
            {
                this.SystemHealth = result.SystemHealth;
            }
            return result;
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

        public String GetRootFolderId()
        {
            String result = null;
            var url = String.Format(StorageUrl.OneDriveGetFolderId, GetAccessToken());
            return Retry<String>(delegate()
            {
                HttpWebRequest request = GenerateRequest(url, "GET");
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception("Get root folder ID failed.");
                    }
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader streamReader = new StreamReader(responseStream))
                        {
                            var jsonStr = streamReader.ReadToEnd();
                            if (jsonStr.Contains(this.OpenParameter.RootFolderName))
                            {
                                result = OneDriveUtil.ParseRootFolderId(jsonStr, this.OpenParameter.RootFolderName);
                            }
                            if (String.IsNullOrEmpty(result))
                            {
                                result = CreateRootFolder(this.OpenParameter.RootFolderName);
                            }
                        }
                    }
                }
                request.Abort();
                return result;
            });
        }

        public override StorageResult CommitStream(Stream commitStream, StorageInfo info)
        {
            var result = new StorageResult();
            if (info.Length < this.OpenParameter.BlockLength)
            {
                result = base.CommitStream(commitStream, info);
            }
            else
            {
                try
                {
                    this.logger.Info("Begin uploading large file,details{0}.", info.ToString());
                    this.LastStreamInfo = info.Clone();
                    var pattern = new String[] { "/", "\\", ":", ";", "*", "<", ">", "|", "?", "#" };
                    var replacePattern = new String[] { "%255c", "%255c", "%253a", "%3b", "%252A", "%253c", "%253e", "%257c", "%253f", "%23" };
                    String fileName = info.HighPlusLowName;
                    for (int i = 0; i < pattern.Length; i++)
                    {
                        fileName = fileName.Replace(pattern[i], replacePattern[i]);
                    }
                    var url = this.GetUploadUrl(fileName);
                    this.UploadLargeFile(commitStream, url);
                    result.StorageInfo = string.Format("<StorageInfo metaId=\"{0}\" contentId=\"{1}\"/>", "", this.objectID);
                    var xURIResult = new XURIResult();
                    xURIResult.SInfo = new StorageInfo() { HighName = string.Empty, LowName = this.objectID, ExtraStorageInfo = result.StorageInfo };
                    result.URI = xURIResult;
                    result.URI.SdType = 407;
                    result.URI.SysId = this.SystemID;
                    result.PdId = this.SystemID;
                    result.UriId = this.objectID;
                    result.IsCommited = true;
                }
                catch (Exception e)
                {
                    this.logger.Error("An error occurred while committing files,details:{0}.", e.Message);
                    throw;
                }
            }
            return result;
        }

        private String GetUploadUrl(String fileName)
        {
            var uploadUrl = default(String);
            var baseRootFolderID = this.OpenParameter.RootFolderId.Substring(this.OpenParameter.RootFolderId.LastIndexOf(".", StringComparison.OrdinalIgnoreCase) + 1);
            var url = String.Format(@"https://api.onedrive.com/v1.0/drive/items/{0}:/{1}:/upload.createSession", baseRootFolderID, fileName);
            var req = WebRequest.Create(url) as HttpWebRequest;
            req.Method = "POST";
            req.Headers.Add("Authorization", "Bearer " + this.GetAccessToken());
            req.ContentType = "application/json";
            using (Stream requestStream = req.GetRequestStream())
            {
            }
            try
            {
                var s = req.GetResponse() as HttpWebResponse;
                using (Stream responseStream = s.GetResponseStream())
                {
                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        var jsonStr = streamReader.ReadToEnd();
                        logger.Debug("Get folder info after create:{0}", jsonStr);
                        var tempResult = Regex.Match(jsonStr, "\"uploadUrl\":\"https://[^@]+").Groups[0].Value;
                        var tempUrl = Regex.Match(tempResult, "https://[^@]+").Groups[0].Value;
                        var redundantUrl = Regex.Match(tempResult, "\"expirationDateTime\":\"[^@]+").Groups[0].Value;
                        uploadUrl = tempUrl.Remove(tempUrl.LastIndexOf(redundantUrl, StringComparison.OrdinalIgnoreCase) - 2);
                    }
                }
            }
            catch (WebException e)
            {
                this.logger.Error("An error occurred while getting upload url,details:{0}.", e.Message);
                throw;
            }
            return uploadUrl;
        }

        private void UploadLargeFile(Stream stream, String uploadUrl)
        {
            Int64 fileLength = stream.Length;
            Int64 beginLength = 0;
            Int32 perLength = 10485760;
            Int64 finishLength = 10485759;
            Int32 retryCount = 0;
            while (true)
            {
                try
                {
                    var req = WebRequest.Create(uploadUrl) as HttpWebRequest;
                    req.Method = "PUT";
                    req.Timeout = 600000;
                    req.Headers.Add("Authorization", "Bearer " + GetAccessToken());
                    if (finishLength > fileLength)
                    {
                        finishLength = fileLength - 1;
                        perLength = Int32.Parse((fileLength - beginLength).ToString());
                        req.ContentLength = fileLength - beginLength;
                    }
                    else
                        req.ContentLength = perLength;
                    var range = "bytes " + beginLength.ToString() + "-" + finishLength.ToString() + "/" + fileLength.ToString();
                    req.Headers.Add("Content-Range", range);
                    using (var requestStream = req.GetRequestStream())
                    {
                        this.logger.Warn("Content length:{0},range:{1}", perLength.ToString(), range);
                        Byte[] buffer = new byte[perLength];
                        var readLength = stream.Read(buffer, 0, perLength);
                        requestStream.Write(buffer, 0, perLength);
                        using (var resp = req.GetResponse() as HttpWebResponse)
                        {
                            if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Created)
                            {
                                var responseStream = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                                Regex driveIdRegex = new Regex("\"driveId\":\"[^\n]*");
                                Match driveIdMatch = driveIdRegex.Match(responseStream);
                                Regex eTagRegex = new Regex("\"eTag\":\"[^\n]*");
                                Regex idRegex = new Regex("\"id\":\"[^\n]*");
                                Match eTagMatch = eTagRegex.Match(responseStream);
                                Match idMatch = idRegex.Match(eTagMatch.Value);
                                this.objectID = "file." + driveIdMatch.Value.Substring(11, driveIdMatch.Value.IndexOf("\"id\"", StringComparison.OrdinalIgnoreCase) - 13)
                                                                 + "."
                                                                 + idMatch.Value.Substring(6, idMatch.Value.IndexOf("\"lastModifiedBy\"", StringComparison.OrdinalIgnoreCase) - 8);

                                break;
                            }
                            else if (resp.StatusCode == HttpStatusCode.ServiceUnavailable && retryCount < 6)
                            {
                                retryCount++;
                                continue;
                            }
                            else if (resp.StatusCode == HttpStatusCode.Accepted)
                            {
                                beginLength += perLength;
                                finishLength += perLength;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    this.logger.Error("An error occurred while uploading large file,details:{0}.", e.Message);
                    var response = e.Response as HttpWebResponse;
                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable && retryCount < 6)
                    {
                        retryCount++;
                        continue;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
            this.logger.Info("Upload large file finished.");
        }

        private String CreateRootFolder(String fileName)
        {
            String result;
            var stringBuilder = new StringBuilder();
            var url = StorageUrl.OneDriveCreateFolder;
            return Retry<String>(delegate()
            {
                HttpWebRequest request = GenerateRequest(url, "POST");
                request.Headers.Add("Authorization", "Bearer " + GetAccessToken());
                request.ContentType = "application/json";
                stringBuilder.Append("{ name: \"").Append(fileName).Append("\" }");
                using (Stream requestStream = request.GetRequestStream())
                {
                    Byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    requestStream.Write(buffer, 0, buffer.Length);
                }
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.Created)
                    {
                        throw new Exception(String.Format("Create root folder failed, StatusCode={0}.", response.StatusCode));
                    }
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader streamReader = new StreamReader(responseStream))
                        {
                            var jsonStr = streamReader.ReadToEnd();
                            logger.Debug("Get folder info after create:{0}", jsonStr);
                            result = OneDriveUtil.GetNewRootFolderId(jsonStr);
                        }
                    }
                }
                request.Abort();
                return result;
            });
        }

        private Boolean CheckSkyDriveFileId(String fileId)
        {
            Regex regex = new Regex(@"^file\.(([0-9a-z]*[0-9][0-9a-z]*[a-z][0-9a-z]*)|([0-9a-z]*[a-z][0-9a-z]*[0-9][0-9a-z]*))\.(([0-9A-Z]*[0-9][0-9A-Z]*[A-Z][0-9A-Z]*)|([0-9A-Z]*[A-Z][0-9A-Z]*[0-9][0-9A-Z]*))![1-9][0-9]*$");
            return regex.Match(fileId).Success;
        }

        private Boolean CheckSkyDriveFolderId(string folderId)
        {
            Regex regex = new Regex(@"^folder\.(([0-9a-z]*[0-9][0-9a-z]*[a-z][0-9a-z]*)|([0-9a-z]*[a-z][0-9a-z]*[0-9][0-9a-z]*))\.(([0-9A-Z]*[0-9][0-9A-Z]*[A-Z][0-9A-Z]*)|([0-9A-Z]*[A-Z][0-9A-Z]*[0-9][0-9A-Z]*))![1-9][0-9]*$");
            return regex.Match(folderId).Success;
        }

        public override Boolean FileExists(StorageInfo info)
        {
            Boolean result;
            if (!CheckSkyDriveFileId(info.ObjectId))
            {
                result = false;
            }
            else
            {
                try
                {
                    result = Retry<Boolean>(delegate()
                    {
                        logger.Debug("info:{0}", info.ObjectId);
                        var url = String.Format(StorageUrl.OneDriveObject, info.ObjectId, GetAccessToken());
                        var request = this.GenerateRequest(url, "GET");
                        using (var response = request.GetResponse() as HttpWebResponse)
                        {
                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                throw new Exception(String.Format("Read file properties failed. StatusCode={0}.", response.StatusCode));
                            }
                        }
                        request.Abort();
                        return true;
                    });
                }
                catch (PathNotFoundException ex)
                {
                    logger.Info("Can not find the file. Message = {0}", ex);
                    result = false;
                }
                catch (Exception e)
                {
                    logger.Error("Can't get file properties. Message = {0}", e);
                    throw;
                }
            }
            return result;
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            CheckState();
            StorageDeleteResult deleteResult = new StorageDeleteResult();
            try
            {
                var url = String.Format(StorageUrl.OneDriveObject, info.ClipId, GetAccessToken());
                Retry<Boolean>(delegate()
                {
                    HttpWebRequest request = this.GenerateRequest(url, "DELETE");
                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.NoContent)
                        {
                            throw new Exception(String.Format("Delete folder failed, StatusCode={0}.", response.StatusCode));
                        }
                    }
                    request.Abort();
                    return true;
                });
            }
            catch (PathNotFoundException ex)
            {
                logger.Info("Can't find the folder. Message = {0}", ex);
            }
            catch (Exception e)
            {
                logger.Error("Delete folder failed. Message = {0}", e);
                throw;
            }
            deleteResult.IsDeleted = true;
            Deletion = true;
            return deleteResult;
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            CheckState();
            StorageDeleteResult deleteResult = new StorageDeleteResult();
            try
            {
                XFileInfo fileInfo = OpenFile(info);
                if (fileInfo != null)
                {
                    deleteResult.DeletedFileSize = fileInfo.FileSize;
                }
                var url = String.Format(StorageUrl.OneDriveObject, info.ObjectId, GetAccessToken());
                HttpWebRequest request = this.GenerateRequest(url, "DELETE");
                Retry<Boolean>(delegate()
                {
                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.NoContent)
                        {
                            throw new Exception(String.Format("Delete file failed, StatusCode={0}.", response.StatusCode));
                        }
                    }
                    request.Abort();
                    return true;
                });
            }
            catch (PathNotFoundException ex)
            {
                logger.Info("Can't find the folder. Message = {0}", ex.Message);
                deleteResult.DeletedFileSize = -1;
            }
            catch (Exception e)
            {
                logger.Error("Delete object failed.Message = {0}", e.Message);
                throw;
            }
            deleteResult.IsDeleted = true;
            Deletion = true;
            return deleteResult;
        }

        //private void DeleteUploadedFile(StorageInfo info, StorageDeleteResult deleteResult)
        //{
        //    //CheckState();
        //    if (FileExists(info))
        //    {
        //        var nextMetaID = GetNextMetaID(info.ObjectId);
        //        if (!String.IsNullOrEmpty(nextMetaID))
        //        {
        //            DeleteUploadedFile(new StorageInfo() { ObjectId = nextMetaID }, deleteResult);
        //        }
        //        deleteResult.DeletedFileSize += OpenFile(info).FileSize;
        //        Retry<Boolean>(delegate()
        //        {
        //            foreach (var id in SplitFileID(info.ObjectId))
        //            {
        //                var url = "https://apis.live.net/v5.0/" + id + "?access_token=" + GetAccessToken();
        //                HttpWebRequest request = this.GenerateRequest(url, "DELETE");
        //                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        //                {
        //                    if (response.StatusCode != HttpStatusCode.NoContent)
        //                    {
        //                        throw new Exception(String.Format("DeleteFile failed, StatusCode={0} URL={1}", response.StatusCode, url));
        //                    }
        //                }
        //                request.Abort();
        //            }
        //            logger.Debug("DeleteFile success, fileID:" + info.ObjectId);
        //            return true;
        //        });
        //    }
        //}
        //todo
        public override void MergeStorageInfo<T>(List<T> indexList, StorageResult result, PropertyInfo propertyInfo)
        {
            //CheckState();
            if (!String.IsNullOrEmpty(result.StorageInfo))
            {
                String value = null;
                if (this.LastStreamInfo != null && this.LastStreamInfo.DataType == DataBlockType.MetaData)
                {
                    OneDriveStorageInfo casInfo = OneDriveUtil.Convert2CAStorStorageInfo(result.StorageInfo);
                    foreach (T index in indexList)
                    {
                        value = propertyInfo.GetValue(index, null) as String;
                        OneDriveStorageInfo cas = OneDriveUtil.Convert2CAStorStorageInfo(value);
                        if (string.IsNullOrEmpty(cas.MetaId))
                        {
                            cas.MetaId = casInfo.ContentId;
                            propertyInfo.SetValue(index, OneDriveUtil.Convert2StorageInfo(cas), null);
                        }
                    }
                    result.NeedCommit = true;
                }
                else
                {
                    OneDriveStorageInfo casInfo = OneDriveUtil.Convert2CAStorStorageInfo(result.StorageInfo);
                    foreach (T index in indexList)
                    {
                        value = propertyInfo.GetValue(index, null) as string;
                        OneDriveStorageInfo cas = OneDriveUtil.Convert2CAStorStorageInfo(value);
                        if (string.IsNullOrEmpty(cas.ContentId))
                        {
                            cas.ContentId = casInfo.ContentId;
                            propertyInfo.SetValue(index, OneDriveUtil.Convert2StorageInfo(cas), null);
                        }
                    }
                }
            }
        }

        public String GetAccessToken()
        {
            if (OpenParameter.AccessToken == null || OpenParameter.AccessToken.IsTimeOut)
            {
                ResetToken();
            }
            return this.OpenParameter.AccessToken.TokenString;
        }

        public void ResetToken()
        {
            if (String.IsNullOrEmpty(this.OpenParameter.RefreshToken))
            {
                throw new Exception("RefreshToken is null or empty.");
            }
            else if (this.OpenParameter.ClientId.Equals(OneDriveDefaultClientId, StringComparison.OrdinalIgnoreCase))
            {
                var url = string.Format("https://api.avepointonlineservices.com/api/cloudtoken/GetRefreshToken?refreshToken={0}&deviceType=onedrive", HttpUtility.UrlEncode(XRI.ValueDecode(SecretUtil.EncryptPassword(this.OpenParameter.RefreshToken))));
                var result = RefreshAccessTokenFromAos(url);
                if (!string.IsNullOrEmpty(result) && !result.Contains("Error"))
                {
                    //由于AOS API 返回的字符串中，会自动走Jason序列化，在前后增加"，所以这里需要手动remove
                    this.OpenParameter.AccessToken = new AccessToken(DateTime.Now, SecretUtil.DescryptPassword(result.Split(',')[0].Replace("\"", String.Empty)));
                    this.OpenParameter.RefreshToken = SecretUtil.DescryptPassword(result.Split(',')[1].Replace("\"", String.Empty));
                }
                else
                {
                    throw new Exception("Can not refresh access token from AOS, Error : " + result);
                }
            }
            else
            {
                var body = String.Format("client_id={0}&redirect_uri={1}&grant_type=refresh_token&refresh_token={2}&client_secret={3}",
                                    this.OpenParameter.ClientId, this.OpenParameter.RedirectDomain, this.OpenParameter.RefreshToken, this.OpenParameter.ClientSecret);
                this.SetToken(body);
            }
        }

        private Boolean SetToken(String body)
        {
            var url = StorageUrl.OneDriveToken;
            HttpWebRequest request = this.GenerateRequest(url, "POST");
            request.ContentType = "application/x-www-form-urlencoded";
            using (Stream reqStream = request.GetRequestStream())
            {
                Byte[] buffer = Encoding.ASCII.GetBytes(body);
                reqStream.Write(buffer, 0, buffer.Length);
            }
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(String.Format("Set token failed, StatusCode={0}.", response.StatusCode));
                }
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        var token = streamReader.ReadToEnd();
                        Regex regex = new Regex("\"access_token\":\"([^\"]+)\"\\,\"refresh_token\":\"([^\"]+)\"");
                        Match match = regex.Match(token);
                        if (match.Success)
                        {
                            this.OpenParameter.AccessToken = new AccessToken(DateTime.Now, match.Groups[1].Value);
                            this.OpenParameter.RefreshToken = match.Groups[2].Value;
                            logger.Info("Get access token and refresh token succeed. RefreshToken = {0}", this.OpenParameter.RefreshToken);
                        }
                        else
                        {
                            throw new Exception("Match token failed, tokenStr = " + token);
                        }
                    }
                }
            }
            request.Abort();
            return true;
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            CheckState();
            XStream stream = null;
            if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew || fileMode == FileMode.Truncate || fileMode == FileMode.OpenOrCreate)//todo
            {
                this.Written = true;
                try
                {
                    OneDriveStream skyDriveStream = new OneDriveStream(info, this);
                    skyDriveStream.InitWriteStream(info);
                    this.LastStreamInfo = info.Clone();
                    stream = skyDriveStream;
                }
                catch (Exception e)
                {
                    logger.Error("Open stream failed.Message = {0}", e.ToString());
                    throw;
                }
            }
            else
            {
                stream = GetDownloadStream(info);
            }
            return stream;
        }

        //public XStream DownloadFile(StorageInfo info)
        //{
        //    var fileID = info.ObjectId;
        //    var initOffset = info.Offset;
        //    XStream skyDriveStream = null;
        //    var isBlockRead = false;
        //    //String[] fileIDs = SplitFileID(fileID);
        //    String tempFileID = String.Empty;
        //    Queue<String> fileIDQueue = null;
        //    //if (fileIDs.Length > 1)
        //    //{
        //        //fileIDQueue = new Queue<String>(fileIDs);
        //        for (int i = 0; i < (initOffset / this.OpenParameter.EachBlockLength); i++)
        //        {
        //            fileIDQueue.Dequeue();
        //        }
        //        tempFileID = fileIDQueue.Peek();
        //        //if (fileIDs.Length > 1)
        //        //{
        //            initOffset = initOffset % this.OpenParameter.EachBlockLength;
        //        //}
        //    }
        //    //else
        //    //{
        //    //    tempFileID = fileIDs[0];
        //    //}

        //    Stream respStream = GetDownloadStream(tempFileID, initOffset);
        //    #region new SkyDriveStream
        //    skyDriveStream = new SkyDriveStream(this, info, respStream, (buffer, offset, count) =>
        //    {
        //        int readLen = 0;
        //        try
        //        {
        //            readLen = respStream.Read(buffer, offset, count);
        //            if (readLen <= 0 && fileIDQueue != null && fileIDQueue.Count > 1)
        //            {
        //                isBlockRead = true;
        //                fileIDQueue.Dequeue();
        //                tempFileID = fileIDQueue.Peek();
        //                respStream.Close();
        //                respStream = GetDownloadStream(tempFileID);
        //                readLen = respStream.Read(buffer, offset, count);
        //            }
        //        }
        //        catch (IOException)
        //        {
        //            if (isBlockRead)
        //            {
        //                initOffset = initOffset % this.OpenParameter.EachBlockLength;
        //            }
        //            respStream.Close();
        //            respStream = GetDownloadStream(tempFileID, initOffset);
        //            readLen = respStream.Read(buffer, offset, count);
        //        }
        //        initOffset += readLen;
        //        return readLen;
        //    });
        //    #endregion

        //    return skyDriveStream;
        //}

        private void AddHeadersWithoutValidate(HttpWebRequest request, string key, string value)
        {
            MethodInfo method = request.Headers.GetType().GetMethod("AddWithoutValidate",
                                    BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null,
                                    new Type[] { typeof(string), typeof(string) }, null);
            method.Invoke(request.Headers, new object[] { key, value });
        }

        public string GetNextMetaID(String fileID)
        {
            String result = null;
            var propertiesStr = ReadFileProperties(fileID);
            Regex regex = new Regex("\\'" + OneDriveConstant.META_ID_HEADER + "\\'\\:\\s*\\'([^\\']+)'");
            Match match = regex.Match(propertiesStr);
            if (match.Success)
            {
                result = match.Groups[1].Value;
            }
            return result;
        }

        private XStream GetDownloadStream(StorageInfo info)
        {
            try
            {
                var url = String.Format(StorageUrl.OneDriveDownload, info.ObjectId, GetAccessToken());
                return Retry<XStream>(delegate()
                {
                    var request = this.GenerateRequest(url, "GET");
                    if (info.Offset > 0)
                    {
                        AddHeadersWithoutValidate(request, "Range", "bytes=" + info.Offset + "-");
                    }
                    var response = request.GetResponse() as HttpWebResponse;
                    if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.PartialContent)
                    {
                        throw new Exception(String.Format("Download file failed, StatusCode = {0}.", response.StatusCode));
                    }
                    Stream responseStream = response.GetResponseStream();
                    OneDriveStream skyDriveStream = new OneDriveStream(this, info, responseStream, (buffer, offset, count) =>
                    {
                        int readLen = responseStream.Read(buffer, offset, count);
                        return readLen;
                    });
                    return skyDriveStream;
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            XFileInfo result = new XFileInfo();
            try
            {
                String propertiesStr = ReadFileProperties(fileInfo.ObjectId);
                Regex regex = new Regex("\"size\"\\:\\s*([0-9]+)");
                Match match = regex.Match(propertiesStr);
                if (!match.Success)
                {
                    throw new Exception(String.Format("Match properties string failed, fileID={0}, properties={1}", fileInfo.ObjectId, propertiesStr));
                }
                Int64 fileSize = Int64.Parse(match.Groups[1].Value);
                result = new OneDriveFileInfo(fileInfo.HighName, fileInfo.LowName, fileSize, fileInfo.ObjectId);
            }
            catch (PathNotFoundException pe)
            {
                this.logger.Warn("The file is not exist.Message = {0}", pe.ToString());
                result = null;
            }
            catch (Exception e)
            {
                logger.Error("Open file failed.Message = {0}", e);
                throw;
            }
            return result;
        }

        public String ReadFileProperties(String fileId)
        {
            String result = null;
            var url = String.Format(StorageUrl.OneDriveObject, fileId, GetAccessToken());
            return Retry<String>(delegate()
            {
                var request = this.GenerateRequest(url, "GET");
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(String.Format("Read file properties failed, StatusCode={0}.", response.StatusCode));
                    }
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            result = streamReader.ReadToEnd();
                        }
                    }
                }
                request.Abort();//todo
                return result;
            });
        }

        public override void Close()
        {
        }

        internal T Retry<T>(RetryDelegate<T> del)
        {
            Int32 counter = 0;
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
                        this.logger.Error("too many retry failed. Retry count = {0}, message = {1}", counter, ex);
                        throw;
                    }
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response.StatusCode == HttpStatusCode.Unauthorized || (Int32)response.StatusCode == 420)
                        {
                            ResetToken();
                            continue;
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new PathNotFoundException(ex.Message, ex);
                        }
                        else if (response.StatusCode == HttpStatusCode.InternalServerError || response.StatusCode == HttpStatusCode.RequestTimeout || response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            this.logger.Info("This exception is a connection fail exception:" + ex.Message);
                            if (counter < this.MaxRetryCount)
                            {
                                this.logger.Info("Retry after " + this.RetryInterval + " ms. Retry count: " + counter);
                                Thread.Sleep(this.RetryInterval);
                                continue;
                            }
                            else
                            {
                                throw;
                            }
                        }
                        else
                        {
                            var body = String.Empty;
                            using (var responseStream = response.GetResponseStream())
                            {
                                using (var streamReader = new StreamReader(responseStream))
                                {
                                    body = streamReader.ReadToEnd();
                                }
                            }
                            this.logger.Error("Execute request failed, message = {0}, response body = {1}:", ex, body);
                            throw;
                        }
                    }
                    else if (ex.Status == WebExceptionStatus.ConnectionClosed || ex.Status == WebExceptionStatus.ConnectFailure || ex.Status == WebExceptionStatus.NameResolutionFailure || ex.Status == WebExceptionStatus.Timeout)
                    {
                        this.logger.Info("This exception is a connection fail exception:" + ex.Message);
                        if (counter < this.MaxRetryCount)
                        {
                            this.logger.Info("Retry after " + this.RetryInterval + " ms. Retry count: " + counter);
                            Thread.Sleep(this.RetryInterval);
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        this.logger.Error("execute request failed: {0}", ex);
                        throw;
                    }
                }
            }
        }
        #endregion Methond

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
