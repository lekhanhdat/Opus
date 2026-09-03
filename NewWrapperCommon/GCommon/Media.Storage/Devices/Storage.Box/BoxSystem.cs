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

namespace AvePoint.Media.Storage.Box
{
    using AvePoint.GCommon;
    using AvePoint.Media.Storage.Inner;
    using AvePoint.Media.Storage.Resources.BoxI18N;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
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

    class BoxSystem : AbstractXSystem
    {
        #region Field and property

        private AveLogger logger = AveLogger.GetInstance(typeof(BoxSystem));
        private String originalEmailAddress;
        public BoxOpenParameter OpenParameter { get; set; }

        public delegate T RetryDelegate<T>();

        public String LastMetaId { get; set; }
        public String LastContentId { get; set; }
        public StorageInfo LastStreamInfo { get; set; }
        public Boolean IsAsUserMode { get; set; }
        private static Object locker = new object();
        private static string BoxDefaultClientId = "6wlvcp6l8tujowomdwrbjtqlwhdxzqfq";

        public override StorageInterfaceType StorageInterfaceType
        {
            get { return StorageInterfaceType.Object; }
        }

        public override string Type
        {
            get { return "BoxSystem"; }
        }

        #endregion

        static BoxSystem()
        {
            //Box no longer support for Tls1.0 from 2019-05-13.
            //3888 is the value of 'SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12', requirement for CLR v4.0 
            var securityProtocolType = (SecurityProtocolType)3888;
            ServicePointManager.SecurityProtocol = securityProtocolType;
            ServicePointManager.DefaultConnectionLimit = 1024;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public BoxSystem(String xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            this.SystemHealth = XSystemHealth.Unknown;
            this.IsSupportAutoChangeDataBlock = true;
            this.SpaceThresholdUnit = SpaceThresholdUnit.MB;
            this.OpenParameter = new BoxOpenParameter();
            this.Open();
        }

        public override StorageOpenValidResult Open()
        {
            this.logger.Debug("Start open system.");
            if (this.SystemHealth != XSystemHealth.Unknown)
            {
                return new StorageOpenValidResult();
            }
            base.Open();
            this.IsRetry = true;
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Client_ID))
            {
                this.OpenParameter.ClientId = XriObject.Params[XRIParameterKeys.Box_Client_ID];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Email_Address))
            {
                this.originalEmailAddress = XriObject.Params[XRIParameterKeys.Box_Email_Address];
                this.OpenParameter.EmailAddress = XriObject.Params[XRIParameterKeys.Box_Email_Address].ToLower();
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Root_Folder_Name))
            {
                this.OpenParameter.RootFolderName = XriObject.Params[XRIParameterKeys.Box_Root_Folder_Name];
                this.SystemLocation = this.OpenParameter.RootFolderName;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Root_Folder_Id))
            {
                this.OpenParameter.RootFolderId = XriObject.Params[XRIParameterKeys.Root_Folder_Id];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Client_Secret))
            {
                this.OpenParameter.ClientSecret =
                    SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.Box_Client_Secret]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Refresh_Token))
            {
                this.OpenParameter.RefreshToken =
                    SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.Box_Refresh_Token]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Config_Location))
            {
                this.OpenParameter.ConfigLocation = XriObject.Params[XRIParameterKeys.Box_Config_Location];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Config_Username))
            {
                this.OpenParameter.ConfigUsername = XriObject.Params[XRIParameterKeys.Box_Config_Username];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Config_Password))
            {
                this.OpenParameter.ConfigPassword =
                    SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.Box_Config_Password]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Manager_User_Name))
            {
                IsAsUserMode = true;
                this.OpenParameter.ManagerUserName = XriObject.Params[XRIParameterKeys.Box_Manager_User_Name];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Manager_User_Id))
            {
                this.OpenParameter.ManagerUserId = XriObject.Params[XRIParameterKeys.Box_Manager_User_Id];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Validate_Key))
            {
                this.OpenParameter.IsValidate = Boolean.Parse(XriObject.Params[XRIParameterKeys.Box_Validate_Key]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.PROXY_INFO))
            {
                this.Proxy = new WebProxy(XriObject.Params[XRIParameterKeys.PROXY_INFO]);//兼容老数据
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.BOX_PROXY_SETTING) && Boolean.Parse(XriObject.Params[XRIParameterKeys.BOX_PROXY_SETTING]))
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
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "Box Object Storage Server";
            List<String> keys = new List<String>();
            keys.Add(this.OpenParameter.RootFolderId);
            keys.Add(this.OpenParameter.ClientId);
            List<String> securityKeys = new List<String>();
            securityKeys.Add(this.OpenParameter.ClientSecret);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        public override StorageOpenValidResult Validate()
        {
            this.logger.Debug("Start validate.");
            BoxConfigFileHandler config = null;
            CheckState();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            var openValidResult = new StorageOpenValidResult();
            try
            {
                config = new BoxConfigFileHandler(this.OpenParameter.ConfigLocation,
                    this.OpenParameter.ConfigUsername, this.OpenParameter.ConfigPassword, this.OpenParameter.EmailAddress, this.originalEmailAddress);
                if (config.ValidateResult.SystemHealth != XSystemHealth.AvailableAndNotFull)
                {
                    config.Close();
                    throw new AuthenticationFailedException("The config location is not available.");
                }
                if (String.IsNullOrEmpty(this.OpenParameter.ManagerUserId))
                {
                    if (IsAsUserMode)
                    {
                        this.OpenParameter.ManagerUserId = this.GetManagedUserId();
                    }
                    else
                    {
                        this.OpenParameter.ManagerUserId = this.GetCurrentUser().Id;
                    }
                    this.Properties[XRIParameterKeys.AppendConnectionStringKey] += "&" + XRIParameterKeys.Box_Manager_User_Id + "=" + this.OpenParameter.ManagerUserId;
                }
                if (string.IsNullOrEmpty(this.OpenParameter.RootFolderId))
                {
                    this.OpenParameter.RootFolderId = GetRootFolderId(this.OpenParameter.RootFolderName);
                    this.Properties[XRIParameterKeys.AppendConnectionStringKey] += "&" + XRIParameterKeys.Root_Folder_Id + "=" + this.OpenParameter.RootFolderId;
                }
                //else
                //{
                //    this.Properties[XRIParameterKeys.AppendConnectionStringKey] += "&" + XRIParameterKeys.Root_Folder_Id + "=" + this.OpenParameter.RootFolderId;
                //}
                var spaceInfo = CacheUtil.GetSpaceInfo(VIMName.Box, this.XriString,
                    new CheckFreeSpace(this.CheckFreeSpace));
                openValidResult.TotalSpace = this.innerTotalSpace = spaceInfo.TotalSpace;
                openValidResult.TotalFreeSpace = this.innerTotalFreeSpace = spaceInfo.TotalFreeSpace;
                openValidResult.TotalUsedSpace = this.innerTotalUsedSpace = spaceInfo.TotalUsedSpace;
                openValidResult.IsReadAble = true;
                openValidResult.IsHasPermission = true;
                if (ValidateIsFull())
                {
                    openValidResult.SystemHealth = XSystemHealth.Available;
                }
                else
                {
                    var info = new StorageInfo();
                    var b = new Byte[1];
                    info.Length = b.Length;
                    info.ClipId = this.OpenParameter.RootFolderId;
                    info.LowName = System.Guid.NewGuid().ToString();
                    using (XStream stream = OpenStream(info, FileMode.CreateNew))
                    {
                        b[0] = 0x00;
                        stream.Write(b, 0, b.Length);
                        var commitResult = stream.Commit(false);
                        info.ObjectId = commitResult.URI.SInfo.ObjectId;
                    }
                    openValidResult.IsWriteAble = true;
                    try
                    {
                        DeleteFile(info);
                        openValidResult.IsDeleteAble = true;
                    }
                    catch (Exception e)
                    {
                        logger.Warn("cannot delete the temp file , ID : {0} and error : {1}", info.ObjectId, e);
                    }
                    openValidResult.SystemHealth = XSystemHealth.AvailableAndNotFull;
                }
            }
            catch (WebException we)
            {
                if (we.Response != null)
                {
                    if ((we.Response as HttpWebResponse).StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
                    {
                        logger.Error("Validate Error:{0}", we);
                        openValidResult.Message = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Proxy_Authentication_Filed", AbstractXSystem.Culture);
                        openValidResult.SystemHealth = XSystemHealth.ConnectedFailed;
                    }
                    else
                    {
                        logger.Error("Validate Error:{0}", we);
                        openValidResult.Message = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Test_failed", AbstractXSystem.Culture);
                        openValidResult.SystemHealth = XSystemHealth.ConnectedFailed;
                    }
                }
                else
                {
                    logger.Error("Validate Error:{0}", we);
                    openValidResult.Message = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Test_failed", AbstractXSystem.Culture);
                    openValidResult.SystemHealth = XSystemHealth.ConnectedFailed;
                }
            }
            catch (TokenNotFoundException te)
            {
                logger.Error("Validate Error:{0}", te);
                openValidResult.Message = BoxI18N.ResourceManager.GetString("MediaStorage_Box_TokenNotExist", AbstractXSystem.Culture);
                openValidResult.SystemHealth = XSystemHealth.Unaccessable;
            }
            catch (PathNotFoundException pe)
            {
                logger.Error("Validate Error:{0}", pe);
                openValidResult.Message = BoxI18N.ResourceManager.GetString("MediaStorage_Box_RootFolderNotExist", AbstractXSystem.Culture);
                openValidResult.SystemHealth = XSystemHealth.Unaccessable;
            }
            catch (DeviceNotAvailableException de)
            {
                logger.Error("Validate Error:{0}", de);
                openValidResult.Message = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Test_failed", AbstractXSystem.Culture);
                openValidResult.SystemHealth = XSystemHealth.ConnectedFailed;
            }
            catch (AuthenticationFailedException ae)
            {
                logger.Error("Validate Error:{0}", ae);
                openValidResult.Message = BoxI18N.ResourceManager.GetString("MediaStorage_Box_ConfigPathNotAvailable", AbstractXSystem.Culture);
                openValidResult.SystemHealth = XSystemHealth.Unaccessable;
            }
            catch (Exception ex)
            {
                logger.Error("Validate Error:{0}", ex);
                openValidResult.Message = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Test_failed", AbstractXSystem.Culture);
                openValidResult.SystemHealth = XSystemHealth.AuthenticationFailed;
            }
            finally
            {
                this.SystemHealth = openValidResult.SystemHealth;
            }
            return openValidResult;
        }

        public SpaceInfo CheckFreeSpace()
        {
            return Retry<SpaceInfo>(delegate ()
            {
                var spaceInfo = new SpaceInfo();
                var url = StorageUrl.BoxUserInfo;
                var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(String.Format("CheckFreeSpace failed, StatusCode={0} URL={1}",
                            response.StatusCode, url));
                    }
                    var responseStream = response.GetResponseStream();
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        var space = streamReader.ReadToEnd();
                        spaceInfo.TotalSpace = UInt64.Parse(BoxUtil.ParseSpaceField(space, "\"space_amount\":[^,]+"),
                            NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);
                        spaceInfo.TotalUsedSpace = UInt64.Parse(BoxUtil.ParseSpaceField(space, "\"space_used\":[^,]+"),
                            NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);
                        spaceInfo.TotalFreeSpace = spaceInfo.TotalSpace - spaceInfo.TotalUsedSpace;
                    }
                }
                request.Abort();
                return spaceInfo;
            });
        }

        #region 跟具路径找Id

        public String GetRootFolderId(String path)
        {
            if (string.IsNullOrEmpty(path) || "".Equals(path.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new PathNotFoundException("The folder name can not be null or empty");
            }
            else if ("/".Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                return "0";//root folder
            }
            var boxObjectList = new List<BoxObject>();
            try
            {
                var countOfLevel = 0;
                String[] array = path.Replace("/", "\\")
                    .Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var levelName in array)
                {
                    if (countOfLevel == 0)
                    {
                        boxObjectList.Add(FindIdByName("0", levelName.ToString()));
                    }
                    else
                    {
                        boxObjectList.Add(FindIdByName(boxObjectList[countOfLevel - 1].Id, levelName.ToString()));
                    }
                    countOfLevel++;
                }
            }
            catch (System.Exception ex)
            {
                logger.Error("Get folder {0} id error:{1}", path, ex);
                throw;
            }
            return boxObjectList[boxObjectList.Count - 1].Id;
        }

        private BoxObject FindIdByName(String id, String name)
        {
            return FindIdByNameLimit(id, name, 100, 0);
        }

        private BoxObject FindIdByNameLimit(String id, String name, Int32 limit, Int32 offset)
        {
            return Retry<BoxObject>(delegate ()
            {
                var boxObject = new BoxObject();
                var url = String.Format(StorageUrl.BoxFindId, id, limit,
                    offset);
                var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        boxObject = AnalysesJsonForFind(result, name);
                        if (boxObject.Id == null)
                        {
                            if (boxObject.Total_Count >= limit)
                            {
                                offset += limit;
                                boxObject = FindIdByNameLimit(id, name, limit, offset);
                            }
                            else
                            {
                                throw new PathNotFoundException("Can't find folder " + name);
                            }
                        }
                    }
                }
                return boxObject;
            });
        }

        private BoxObject AnalysesJsonForFind(String json, String name)
        {
            var boxObject = new BoxObject();
            var js = new JavaScriptSerializer();
            var dicJson = (Dictionary<string, object>)js.DeserializeObject(json);
            //if (dicJson["total_count"] != null)
            //{
            //    boxObject.Total_Count = Convert.ToInt32(dicJson["total_count"]);
            //}
            if (dicJson["entries"] != null)
            {
                var lists = dicJson["entries"] as object[];
                boxObject.Total_Count = lists.Length;
                foreach (var obj in lists)
                {
                    var box = new BoxObject();
                    var dicBox = (Dictionary<string, object>)obj;
                    if (dicBox["name"].ToString().Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        boxObject.Id = dicBox["id"].ToString();
                        boxObject.Type = dicBox["type"].ToString();
                        boxObject.Name = dicBox["name"].ToString();
                        break;
                    }
                }
            }

            return boxObject;
        }

        #endregion

        private Boolean CheckBoxId(String boxId)
        {
            Int64 result = 0;
            return long.TryParse(boxId, out result);
        }

        #region Folder method

        public override Boolean DirectoryExists(StorageInfo info)
        {
            CheckState();
            var clipId = String.Empty;
            if (String.IsNullOrEmpty(info.ClipId))
            {
                try
                {
                    clipId =
                        this.GetRootFolderId(PathUtil.CombinePath(this.OpenParameter.RootFolderName,
                            info.HighPlusLowName));
                }
                catch (PathNotFoundException)
                {
                    return false;
                }
            }
            else
            {
                clipId = info.ClipId;
            }
            //if (!CheckBoxId(info.ClipId))
            //{
            //    return false;
            //}
            try
            {
                return Retry<Boolean>(delegate ()
                {
                    var url = String.Format("https://api.box.com/2.0/folders/{0}", clipId);
                    var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception(String.Format("ReadFileProperties failed, StatusCode={0} URL={1}",
                                response.StatusCode, url));
                        }
                    }
                    request.Abort();
                    return true;
                });
            }
            catch (PathNotFoundException)
            {
                return false;
            }
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            return Retry<BoxFolderInfo>(delegate ()
            {
                try
                {
                    BoxFolderInfo directoryInfo = null;
                    if (mode != FileMode.Open)
                    {
                        return CreateNewFolder(dirInfo);
                    }
                    else
                    {
                        var url = String.Format("https://api.box.com/2.0/folders/{0}", dirInfo.ClipId);
                        var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
                        using (var response = request.GetResponse() as HttpWebResponse)
                        {
                            if (response.StatusCode == HttpStatusCode.Created ||
                                response.StatusCode == HttpStatusCode.OK)
                            {
                                using (var streamReader = new StreamReader(response.GetResponseStream()))
                                {
                                    var result = streamReader.ReadToEnd();
                                    var boxObject = ParseJsonString(result);
                                    directoryInfo = new BoxFolderInfo(this, dirInfo.HighName, dirInfo.LowName, boxObject);
                                }
                            }
                            else
                            {
                                throw new Exception("create new folder failed:" + response.ToString());
                            }
                        }
                        return directoryInfo;
                    }
                }
                catch (WebException e)
                {
                    HttpWebResponse resp = e.Response as HttpWebResponse;
                    if (resp != null && resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    throw;
                }
            });
        }

        private BoxFolderInfo CreateNewFolder(StorageInfo dirInfo)
        {
            var directoryInfo = new BoxObject();
            var stringBuilder = new StringBuilder();
            var url = "https://api.box.com/2.0/folders";
            var request = GenerateRequest(BoxConstants.HttpMethod_POST, url);
            request.ContentType = "application/json";
            stringBuilder.Append("{ \"name\": \"")
              .Append(dirInfo.LowName.TrimEnd('\\'))
              .Append("\", \"parent\": {\"id\":\"")
              .Append(String.IsNullOrEmpty(dirInfo.ClipId) ? this.OpenParameter.RootFolderId : dirInfo.ClipId)
              .Append("\"}")
              .Append(" }");
            using (var requestStream = request.GetRequestStream())
            {
                Byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                requestStream.Write(buffer, 0, buffer.Length);
            }
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    throw new Exception(String.Format("CreateRootFolder failed, StatusCode={0} URL={1}",
                        response.StatusCode, url));
                }
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    directoryInfo = ParseJsonString(result);
                }
            }
            request.Abort();
            return new BoxFolderInfo(this, dirInfo.HighName, dirInfo.LowName, directoryInfo);
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            CheckState();
            return Retry<StorageDeleteResult>(delegate ()
            {
                StorageDeleteResult deleteResult = new StorageDeleteResult();
                try
                {
                    var deleteSize = GetFolderSize(info);
                    var url = string.Format("https://api.box.com/2.0/folders/{0}?recursive=true", info.ClipId);
                    var request = GenerateRequest(BoxConstants.HttpMethod_DELETE, url);
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.NoContent && response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception(string.Format("DeleteDirectory failed, StatusCode={0} URL={1}",
                                response.StatusCode, url));
                        }
                    }
                    deleteResult.IsDeleted = true;
                    deleteResult.DeletedFileSize = deleteSize;
                    Deletion = true;
                }
                catch (PathNotFoundException)
                {
                    deleteResult.IsDeleted = true;
                }
                catch (Exception e)
                {
                    logger.Error("delete folder name:{0}, id:{1}, failed: {2}", info.HighPlusLowName, info.ClipId, e);
                    deleteResult.IsDeleted = false;
                }
                return deleteResult;
            });
        }

        private Int64 GetFolderSize(StorageInfo info)
        {
            Int64 deleteSize = 0;
            var url = string.Format("https://api.box.com/2.0/folders/{0}", info.ClipId);
            var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    var javaScriptSerializer = new JavaScriptSerializer();
                    var jsonData = (Dictionary<string, object>)javaScriptSerializer.DeserializeObject(result);
                    if (jsonData.ContainsKey("size"))
                    {
                        deleteSize = Convert.ToInt64(jsonData["size"]);
                    }
                }
            }
            return deleteSize;
        }

        #endregion

        #region Retrieve a Folder's items

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFilesAddLimit(dirInfo, 100, 0);
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).SubDirs;
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).Files;
        }

        private StorageListResult ListSubDirectoriesAndFilesAddLimit(StorageInfo info, int limit, int offset)
        {
            return Retry<StorageListResult>(delegate ()
            {
                var listResults = new StorageListResult();
                var boxFolderObject = new BoxFolderInfo();
                var boxObject = new BoxObject();
                var url = string.Format("https://api.box.com/2.0/folders/{0}/items?fields=size,name&limit={1}&offset={2}", info.ClipId,
                    limit, offset);
                var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        boxObject = ParseJsonString(result);
                    }
                }
                foreach (var dto in boxObject.Entries)
                {
                    if (dto.Type.Equals("file", StringComparison.OrdinalIgnoreCase))
                    {
                        listResults.Files.Add(new BoxFileInfo(this, info.HighPlusLowName, dto.Name, dto));
                    }
                    else
                    {
                        listResults.SubDirs.Add(new BoxFolderInfo(this, info.HighPlusLowName, dto.Name, dto));
                    }
                }
                if (boxObject.Total_Count > limit + offset)
                {
                    var listResult = new StorageListResult();
                    offset += limit;
                    listResult = ListSubDirectoriesAndFilesAddLimit(info, limit, offset);
                    foreach (var fileObject in listResult.Files)
                    {
                        listResults.Files.Add(fileObject);
                    }
                    foreach (var folderObject in listResult.SubDirs)
                    {
                        listResults.SubDirs.Add(folderObject);
                    }
                }
                return listResults;
            });
        }

        #endregion

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo,
            bool isOverWrite)
        {
            XFileInfo targetFile = null;
            if (targetFileInfo.IsCreateNewVersion)
            {
                if (string.IsNullOrEmpty(targetFileInfo.ObjectId))
                {
                    throw new Exception("The target file object id is not exist. File is " + targetFileInfo.HighPlusLowName);
                }
            }
            else
            {
                if (isOverWrite)
                {
                    if (!targetFileInfo.ObjectId.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                    {
                        targetFile = this.SetNewFileName(targetFileInfo, targetFileInfo.Name + "_" + DateTime.Now.Ticks);
                    }
                    else
                    {
                        var fileList = this.ListFiles(targetFileInfo);
                        foreach (var file in fileList)
                        {
                            if (file.LowName.Equals(targetFileInfo.LowName))
                            {
                                targetFile = this.SetNewFileName(file, file.Name + "_" + DateTime.Now.Ticks);
                                break;
                            }
                        }
                    }
                }
            }
            try
            {
                return Retry<StorageCopyResult>(delegate ()
                {
                    var copyResult = new StorageCopyResult();
                    if (!String.IsNullOrEmpty(sourceFileInfo.VersionId))
                    {
                        copyResult = this.CopyFileOldVersion(sourceFileInfo, targetFileInfo);
                    }
                    else
                    {
                        var url = string.Format(StorageUrl.BoxCopyFile, sourceFileInfo.ObjectId);
                        var request = GenerateRequest(BoxConstants.HttpMethod_POST, url);
                        var stringBuilder = new StringBuilder();
                        stringBuilder.Append("{\"parent\":{\"id\":")
                            .Append(targetFileInfo.ClipId)
                            .Append("}")
                            .Append(", \"name\":\"")
                            .Append(targetFileInfo.LowName)
                            .Append("\"}");
                        using (var requestStream = request.GetRequestStream())
                        {
                            Byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                            requestStream.Write(buffer, 0, buffer.Length);
                        }
                        using (var response = request.GetResponse() as HttpWebResponse)
                        {
                            using (var streamReader = new StreamReader(response.GetResponseStream()))
                            {
                                if (response.StatusCode != HttpStatusCode.Created &&
                                    response.StatusCode != HttpStatusCode.OK)
                                {
                                    throw new Exception(string.Format("CopyFile failed, StatusCode={0} URL={1}",
                                        response.StatusCode, url));
                                }
                                else
                                {
                                    var boxObject = ParseJsonString(streamReader.ReadToEnd());
                                    copyResult.IsCopyed = true;
                                    var uriResult = new XURIResult();
                                    uriResult.SdType = 408;
                                    uriResult.SysId = this.SystemID;
                                    uriResult.SInfo = new StorageInfo();
                                    uriResult.SInfo.ObjectId = boxObject.Id;
                                    copyResult.URI = uriResult;
                                }
                            }
                        }
                    }
                    if (targetFile != null)
                    {
                        this.DeleteFile(targetFile);
                    }
                    return copyResult;
                });
            }
            catch (Exception)
            {
                if (targetFile != null)
                {
                    this.SetNewFileName(targetFile, targetFileInfo.LowName);
                }
                throw;
            }
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            CheckState();
            var deleteResult = new StorageDeleteResult();
            if (!string.IsNullOrEmpty(info.VersionId))
            {
                deleteResult.IsDeleted = DeleteOldFileVersion(info);
            }
            else
            {
                try
                {
                    foreach (string objId in info.ObjectIds)
                    {
                        if (!string.IsNullOrEmpty(objId))
                        {
                            try
                            {
                                DeleteUploadedFile(new StorageInfo() { ObjectId = objId }, deleteResult);
                            }
                            catch (PathNotFoundException)
                            {
                                logger.Info("file already not exists, fileID {0}, name {1}", objId, info.LowName);
                            }
                        }
                    }
                    deleteResult.IsDeleted = true;
                }
                catch (Exception e)
                {
                    logger.Error("delete object failed, id:{0}, name:{1}, msg:{2}", info.ObjectId, info.LowName, e);
                    throw;
                }
            }
            Deletion = true;
            return deleteResult;
        }

        public override bool FileExists(StorageInfo info)
        {
            CheckState();
            if (!CheckBoxId(info.ObjectId))
            {
                return false;
            }
            try
            {
                return Retry<bool>(delegate ()
                {
                    var url = String.Format(StorageUrl.BoxFileInfo, info.ObjectId);
                    var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception(string.Format("ReadFileProperties failed, StatusCode={0} URL={1}",
                                response.StatusCode, url));
                        }
                    }
                    request.Abort();
                    return true;
                });
            }
            catch (PathNotFoundException)
            {
                return false;
            }
        }

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo,
            bool isOverWrite)
        {
            return Retry<StorageMoveResult>(delegate ()
            {
                var boxObject = new BoxObject();
                var result = new StorageMoveResult();
                var url = string.Format(StorageUrl.BoxCopyFile, sourceDirInfo.ClipId);
                var request = GenerateRequest(BoxConstants.HttpMethod_POST, url);
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("{\"parent\":{\"id\":").Append("\"").Append(targetDirInfo.ClipId).Append("\"}}");
                using (var requestStream = request.GetRequestStream())
                {
                    Byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    requestStream.Write(buffer, 0, buffer.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception(String.Format("CopyFile failed, StatusCode={0} URL={1}",
                                response.StatusCode, url));
                        }
                        else
                        {
                            boxObject = ParseJsonString(streamReader.ReadToEnd());
                        }

                    }
                }
                if (DeleteDirectory(sourceDirInfo).IsDeleted)
                {
                    result.IsMoved = true;
                    var res = new XURIResult();
                    res.SdType = 408;
                    res.SysId = this.SystemID;
                    res.SInfo = new StorageInfo();
                    if (boxObject != null)
                    {
                        res.SInfo.ClipId = boxObject.Id;
                    }
                    result.URI = res;
                }
                return result;
            });
        }

        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo,
            bool isOverWrite)
        {
            StorageMoveResult storageMoveResult;
            try
            {
                if (!String.IsNullOrEmpty(sourceFileInfo.VersionId))
                {
                    storageMoveResult = this.MoveSingleVersionOfFile(sourceFileInfo, targetFileInfo, isOverWrite);
                }
                else
                {
                    storageMoveResult = this.MoveAllVersionOfFile(sourceFileInfo, targetFileInfo, isOverWrite);
                }
            }
            catch (Exception e)
            {
                this.logger.Error("Move file failed.Message = {0}", e);
                throw;
            }
            return storageMoveResult;
        }

        private StorageMoveResult MoveSingleVersionOfFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            var moveResult = new StorageMoveResult();
            var copyResult = this.CopyFile(sourceFileInfo, targetFileInfo, isOverWrite);
            if (copyResult.IsCopyed)
            {
                this.DeleteOldFileVersion(sourceFileInfo);
            }
            moveResult.IsMoved = true;
            var uri = new XURIResult();
            uri.SysId = this.SystemID;
            uri.SdType = 408;
            uri.SInfo = new StorageInfo();
            uri.SInfo.ObjectId = copyResult.URI.SInfo.ObjectId;
            moveResult.URI = uri;
            return moveResult;
        }

        private StorageMoveResult MoveAllVersionOfFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            XFileInfo targetFile = null;
            var fileList = new List<XFileInfo>();
            if (isOverWrite)
            {
                if (!targetFileInfo.ObjectId.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                {
                    targetFile = this.SetNewFileName(targetFileInfo, targetFileInfo.Name + "_" + DateTime.Now.Ticks);
                }
                else
                {
                    fileList = this.ListFiles(targetFileInfo);
                    foreach (var file in fileList)
                    {
                        if (file.LowName.Equals(targetFileInfo.LowName))
                        {
                            targetFile = this.SetNewFileName(file, file.Name + "_" + DateTime.Now.Ticks);
                            break;
                        }
                    }
                }
            }
            try
            {
                return Retry<StorageMoveResult>(delegate ()
                {
                    var moveResult = new StorageMoveResult();
                    var boxObject = new BoxObject();
                    var url = String.Format(StorageUrl.BoxFileInfo, sourceFileInfo.ObjectId);
                    var request = GenerateRequest(BoxConstants.HttpMethod_PUT, url);
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append("{\"parent\": {\"")
                        .Append("id\": \"")
                        .Append(targetFileInfo.ClipId)
                        .Append("\"")
                        .Append("}, ")
                        .Append("\"name\":\"")
                        .Append(targetFileInfo.LowName)
                        .Append("\"}");
                    using (var requestStream = request.GetRequestStream())
                    {
                        Byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                        requestStream.Write(buffer, 0, buffer.Length);
                    }
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var stream = response.GetResponseStream())
                            {
                                using (var streamReader = new StreamReader(stream))
                                {
                                    var result = streamReader.ReadToEnd();
                                    boxObject = ParseJsonString(result);
                                }
                            }
                            if (targetFile != null)
                            {
                                this.DeleteFile(targetFile);
                            }
                            moveResult.IsMoved = true;
                            var uri = new XURIResult();
                            uri.SysId = this.SystemID;
                            uri.SdType = 408;
                            uri.SInfo = new StorageInfo();
                            uri.SInfo.ObjectId = boxObject.Id;
                            moveResult.URI = uri;
                            return moveResult;
                        }
                        else
                        {
                            throw new Exception(String.Format("Move file with all version failed.highName = {0}, lowName = {1}", sourceFileInfo.HighName, sourceFileInfo.LowName));
                        }
                    }
                });
            }
            catch (Exception)
            {
                if (targetFile != null)
                {
                    this.SetNewFileName(targetFile, targetFileInfo.LowName);
                }
                throw;
            }
        }
        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            CheckState();
            BoxStream stream = null;
            if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew || fileMode == FileMode.OpenOrCreate)
            {
                this.Written = true;
                try
                {
                    info.FileMode = fileMode;
                    stream = new BoxStream(info, this);
                    stream.InitWriteStream();
                    this.LastStreamInfo = info.Clone();
                }
                catch (Exception e)
                {
                    logger.Error("Occurred a error when open stream : {0}", e);
                    throw;
                }
                return stream;
            }
            else if (fileMode == FileMode.Append || fileMode == FileMode.Truncate)
            {
                throw new NotSupportedException(string.Format("Box device don't support {0} stream.", fileMode));
            }
            else
            {
                return GetDownloadStream(info);
            }
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            CheckState();
            XFileInfo result = null;
            try
            {
                result = new BoxFileInfo(this, fileInfo.HighName, fileInfo.LowName, GetFileInfo(fileInfo));
            }
            catch (PathNotFoundException e)
            {
                Trace.TraceWarning(e.ToString());
            }
            return result;
        }

        public override XFileInfo OpenFileWithTags(StorageInfo info)
        {
            var fInfo = OpenFile(info);
            fInfo.Tags = GetFileTags(info).Tags;
            return fInfo;
        }

        public override void MergeStorageInfo<T>(List<T> indexList, StorageResult result, PropertyInfo propertyInfo)
        {
            CheckState();
            if (!string.IsNullOrEmpty(result.StorageInfo))
            {
                string value = null;
                if (this.LastStreamInfo != null && this.LastStreamInfo.DataType == DataBlockType.MetaData)
                {
                    BoxStorageInfo casInfo = BoxUtil.Convert2CAStorStorageInfo(result.StorageInfo);
                    foreach (T index in indexList)
                    {
                        value = propertyInfo.GetValue(index, null) as string;
                        BoxStorageInfo cas = BoxUtil.Convert2CAStorStorageInfo(value);
                        if (string.IsNullOrEmpty(cas.MetaId))
                        {
                            cas.MetaId = casInfo.ContentId;
                            propertyInfo.SetValue(index, BoxUtil.Convert2StorageInfo(cas), null);
                        }
                    }
                    result.NeedCommit = true;
                }
                else
                {
                    BoxStorageInfo casInfo = BoxUtil.Convert2CAStorStorageInfo(result.StorageInfo);
                    foreach (T index in indexList)
                    {
                        value = propertyInfo.GetValue(index, null) as string;
                        BoxStorageInfo cas = BoxUtil.Convert2CAStorStorageInfo(value);
                        if (string.IsNullOrEmpty(cas.ContentId))
                        {
                            cas.ContentId = casInfo.ContentId;
                            propertyInfo.SetValue(index, BoxUtil.Convert2StorageInfo(cas), null);
                        }
                    }
                }
            }
        }

        private XStream GetDownloadStream(StorageInfo info)
        {
            return Retry<XStream>(delegate ()
            {
                var url = String.Format(StorageUrl.BoxDownload, info.ObjectId);
                if (info.VersionId != null)
                {
                    url = String.Format(StorageUrl.BoxDownloadWithVersion, info.ObjectId,
                        info.VersionId);
                }
                var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
                if (info.Offset > 0)
                {
                    if (info.Length > 0)
                    {
                        AddHeadersWithoutValidate(request, "Range",
                            "bytes=" + info.Offset + "-" + (info.Offset + info.Length));
                    }
                    else
                    {
                        AddHeadersWithoutValidate(request, "Range", "bytes=" + info.Offset + "-");
                    }
                }
                var response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.PartialContent && response.StatusCode != HttpStatusCode.Accepted)
                {
                    throw new Exception(string.Format("DownloadFile failed, StatusCode={0} URL={1}", response.StatusCode,
                        url));
                }
                info.Length = response.ContentLength;
                var responseStream = response.GetResponseStream();
                var boxStream = new BoxStream(this, info, responseStream, (buffer, offset, count) =>
                {
                    Int32 readLen = responseStream.Read(buffer, offset, count);
                    return readLen;
                });
                return boxStream;
            });
        }

        private void DeleteUploadedFile(StorageInfo info, StorageDeleteResult rs)
        {
            if (FileExists(info))
            {
                var nextMetaID = GetNextMetaID(info);
                if (!string.IsNullOrEmpty(nextMetaID))
                {
                    DeleteUploadedFile(new StorageInfo() { ObjectId = nextMetaID }, rs);
                }
                rs.DeletedFileSize += OpenFile(info).FileSize;
                Retry<Boolean>(delegate ()
                {
                    foreach (var id in info.ObjectIds)
                    {
                        var url = String.Format(StorageUrl.BoxFileInfo, id);
                        var request = GenerateRequest(BoxConstants.HttpMethod_DELETE, url);
                        using (var response = request.GetResponse() as HttpWebResponse)
                        {
                            if (response.StatusCode != HttpStatusCode.NoContent &&
                                response.StatusCode != HttpStatusCode.OK)
                            {
                                throw new Exception(string.Format("DeleteFile failed, StatusCode={0} URL={1}",
                                    response.StatusCode, url));
                            }
                        }
                        request.Abort();
                    }
                    logger.Debug("DeleteFile success, fileID: {0}", info.ObjectId);
                    return true;
                });
            }
        }

        public string GetNextMetaID(StorageInfo info)
        {
            string result = null;
            BoxObject obj = GetFileInfo(info);
            Regex r = new Regex("\\'" + BoxConstants.META_ID_HEADER + "\\'\\:\\s*\\'([^\\']+)'");
            Match m = r.Match(obj.Description);
            if (m.Success)
            {
                result = m.Groups[1].Value;
            }
            return result;
        }

        private BoxObject GetFileInfo(StorageInfo info)
        {
            return Retry<BoxObject>(delegate ()
            {
                string result = string.Empty;
                string url = string.Format(StorageUrl.BoxFileInfo, info.ObjectId);
                var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("ReadFileProperties failed, StatusCode={0} URL={1}",
                            response.StatusCode, url));
                    }
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            result = streamReader.ReadToEnd();
                        }
                    }
                }
                request.Abort();
                return ParseJsonString(result);
            });
        }

        internal BoxObject GetFileTags(StorageInfo info)
        {
            return Retry<BoxObject>(delegate ()
            {
                string result = string.Empty;
                string url = string.Format(StorageUrl.BoxFileTags, info.ObjectId);
                var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("ReadFileProperties failed, StatusCode={0} URL={1}",
                            response.StatusCode, url));
                    }
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            result = streamReader.ReadToEnd();
                        }
                    }
                }
                request.Abort();
                return ParseJsonString(result);
            });
        }

        internal BoxObject GetFolderTags(StorageInfo info)
        {
            return Retry<BoxObject>(delegate ()
            {
                string result = string.Empty;
                string url = string.Format(StorageUrl.BoxFolderTags, info.ClipId);
                var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("ReadFileProperties failed, StatusCode={0} URL={1}",
                            response.StatusCode, url));
                    }
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            result = streamReader.ReadToEnd();
                        }
                    }
                }
                request.Abort();
                return ParseJsonString(result);
            });
        }

        internal BoxObject ParseJsonString(string jsonStr)
        {
            var boxObject = new BoxObject();
            var javaScriptSerializer = new JavaScriptSerializer();
            boxObject = javaScriptSerializer.Deserialize<BoxObject>(jsonStr);
            return boxObject;
        }

        internal HttpWebRequest GenerateRequest(String method, String url)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Headers.Add("Authorization", String.Format("Bearer {0}", this.GetAccessToken()));
            request.Method = method;
            if (IsAsUserMode && !String.IsNullOrEmpty(this.OpenParameter.ManagerUserId))
            {
                request.Headers.Add("As-User", this.OpenParameter.ManagerUserId);
            }
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

        private void AddHeadersWithoutValidate(HttpWebRequest req, string key, string value)
        {
            MethodInfo method = req.Headers.GetType().GetMethod("AddWithoutValidate",
                BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null,
                new Type[] { typeof(string), typeof(string) }, null);
            method.Invoke(req.Headers, new object[] { key, value });
        }

        public XFileInfo SetNewFileName(StorageInfo info, string name)
        {
            try
            {
                var boxObject = new BoxObject();
                var url = String.Format(StorageUrl.BoxFileInfo, info.ObjectId);
                var request = GenerateRequest(BoxConstants.HttpMethod_PUT, url);
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("{\"name\":\"").Append(name).Append("\"}");
                Byte[] metadataData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                request.ContentLength = metadataData.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(metadataData, 0, metadataData.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("ReadFileProperties failed, StatusCode={0} URL={1}",
                            response.StatusCode, url));
                    }
                    else
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            using (var streamReader = new StreamReader(stream))
                            {
                                var resultInfo = streamReader.ReadToEnd();
                                boxObject = ParseJsonString(resultInfo);
                            }
                        }
                    }
                }
                request.Abort();
                return new BoxFileInfo(this, info.HighName, name, boxObject);
            }
            catch (WebException ex)
            {
                throw new Exception(String.Format("Set new file name failed.FileName = {0}, Message = {1}", info.LowName,
                    ex.ToString()));
            }
        }

        public void SetNewFolderName(StorageInfo info, String name)
        {
            try
            {
                var result = string.Empty;
                var url = String.Format(StorageUrl.BoxFolderInfo, info.ClipId);
                var request = GenerateRequest(BoxConstants.HttpMethod_PUT, url);
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("{\"name\":\"").Append(name).Append("\"}");
                Byte[] metadataData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                request.ContentLength = metadataData.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(metadataData, 0, metadataData.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("ReadFileProperties failed, StatusCode={0} URL={1}",
                            response.StatusCode, url));
                    }
                }
                request.Abort();
            }
            catch (WebException ex)
            {
                throw new Exception(String.Format("Set new folder name failed.FolderName = {0}, Message = {1}",
                    info.LowName, ex.ToString()));
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
                        logger.Error("too many retry failed. Retry count:{0}, msg:{1}", counter, ex);
                        throw;
                    }
                    if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                    {
                        HttpWebResponse resp = ex.Response as HttpWebResponse;
                        if (resp.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            ResetToken();
                            continue;
                        }
                        else if (resp.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new PathNotFoundException(ex.Message, ex);
                        }
                        else if (resp.StatusCode == HttpStatusCode.InternalServerError || resp.StatusCode == HttpStatusCode.RequestTimeout || resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            if (this.OpenParameter.IsValidate || counter >= this.MaxRetryCount)
                            {
                                throw new DeviceNotAvailableException(String.Format("This exception is a connection fail exception:" + ex.Message));
                            }
                            else
                            {
                                logger.Info("Retry after " + this.RetryInterval + " ms. Retry count: " + counter);
                                Thread.Sleep(this.RetryInterval);
                                continue;
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
                            logger.Error("execute request failed, msg:{0}, response body:{1}:", ex, body);
                            throw;
                        }
                    }
                    else if (ex.Status == WebExceptionStatus.ConnectionClosed || ex.Status == WebExceptionStatus.ConnectFailure || ex.Status == WebExceptionStatus.NameResolutionFailure || ex.Status == WebExceptionStatus.Timeout)
                    {
                        if (this.OpenParameter.IsValidate || counter >= this.MaxRetryCount)
                        {
                            throw new DeviceNotAvailableException(String.Format("This exception is a connection fail exception:" + ex.Message));
                        }
                        else
                        {
                            logger.Info("Retry after " + this.RetryInterval + " ms. Retry count: " + counter);
                            Thread.Sleep(this.RetryInterval);
                            continue;
                        }
                    }
                    else
                    {
                        logger.Error("Execute request failed: {0}", ex);
                        throw;
                    }
                }
                catch (RetryableException re)
                {
                    if (counter > this.MaxRetryCount)
                    {
                        logger.Error("too many retry failed. Retry count:{0}, msg:{1}", counter, re);
                        throw;
                    }
                    logger.Info("Retry after at once. Retry count: " + counter);
                    continue;
                }
            }
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        ///// <summary>
        ///// test succeed.
        ///// </summary>
        private String GetManagedUserId()
        {
            return Retry<String>(delegate ()
            {
                var boxUserList = new List<BoxUser>();
                var url = StorageUrl.BoxUsers;
                var request = this.GenerateRequest(BoxConstants.HttpMethod_GET, url);
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            using (var streamReader = new StreamReader(stream))
                            {
                                var result = streamReader.ReadToEnd();
                                return this.SerializeToUserId(result, this.OpenParameter.ManagerUserName);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception(String.Format("Get managed user failed.UserName = {0}",
                            this.OpenParameter.ManagerUserName));
                    }
                }
            });
        }

        private String SerializeToUserId(String result, String login)
        {
            String userId = String.Empty;
            if (!String.IsNullOrEmpty(result))
            {
                var javaScriptSerializer = new JavaScriptSerializer();
                var serializaObject = javaScriptSerializer.DeserializeObject(result) as Dictionary<String, Object>;
                if (serializaObject.ContainsKey("entries"))
                {
                    var userList = serializaObject["entries"] as Object[];
                    foreach (var user in userList)
                    {
                        var serializeUser = user as Dictionary<String, Object>;
                        if (serializeUser.ContainsKey("login") && this.OpenParameter.ManagerUserName.Equals(serializeUser["login"]))
                        {
                            userId = serializeUser["id"].ToString();
                            break;
                        }
                    }
                }
            }
            if (String.IsNullOrEmpty(userId))
            {
                throw new AuthenticationFailedException("Get managed user id failed, please check your ManagerUserName.");
            }
            return userId;
        }

        public override StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile)
        {
            StorageMoveResult moveResult;
            if (this.IsSameSystem(destSystem))
            {
                moveResult = this.MoveFile(srcFile, destFile, true);
            }
            else
            {
                moveResult = base.MoveFile(srcFile, destSystem, destFile);
            }
            return moveResult;
        }

        public override StorageCopyResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite)
        {
            StorageCopyResult copyResult = new StorageCopyResult();
            if (this.IsSameSystem(destSystem))
            {
                copyResult = this.CopyFile(srcFile, destFile, isOverWrite);
            }
            else
            {
                copyResult = Retry<StorageCopyResult>(delegate ()
                {
                    if (isOverWrite)
                    {
                        copyResult = base.CopyFile(srcFile, destSystem, destFile, isOverWrite);
                    }
                    else
                    {
                        using (var readStream = this.OpenStream(srcFile, FileMode.Open))
                        {
                            using (var writeStream = destSystem.OpenStream(destFile, FileMode.CreateNew))
                            {
                                var buffer = new byte[1024 * 64];
                                int readLen = 0;
                                while ((readLen = readStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    writeStream.Write(buffer, 0, readLen);
                                }
                                writeStream.Commit();
                                var uri = writeStream.GetURI();
                                copyResult.URI = uri;
                            }
                        }
                    }
                    return copyResult;
                });
            }
            return copyResult;
        }

        public Boolean IsSameSystem(IXSystem destSystem)
        {
            Boolean result;
            var boxSystem = destSystem as BoxSystem;
            if (boxSystem != null)
            {
                if (this.OpenParameter.ManagerUserId.Equals(boxSystem.OpenParameter.ManagerUserId))
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        public BoxUser GetCurrentUser()
        {
            return Retry<BoxUser>(delegate ()
            {
                var result = false;
                var boxUser = new BoxUser();
                var url = StorageUrl.BoxUserInfo;
                var request = this.GenerateRequest(BoxConstants.HttpMethod_GET, url);
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            var resultString = (new StreamReader(stream)).ReadToEnd();
                            var javaScriptSerializer = new JavaScriptSerializer();
                            boxUser = javaScriptSerializer.Deserialize<BoxUser>(resultString);
                        }
                    }
                    else
                    {
                        logger.Error("Get current user failed.StatusCode={0}", response.StatusCode.ToString());
                    }
                    return boxUser;
                }
            });
        }

        private Boolean DeleteOldFileVersion(StorageInfo info)
        {
            var url = String.Format(StorageUrl.BoxDeleteFile, info.ObjectId, info.VersionId);
            var result = false;
            try
            {
                result = Retry<Boolean>(delegate ()
                {
                    var request = this.GenerateRequest(BoxConstants.HttpMethod_DELETE, url);
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.NoContent)
                        {
                            result = true;
                        }
                        else
                        {
                            throw new Exception(String.Format("delete file failed.id = {0}, versionId = {1}", info.ObjectId, info.VersionId));
                        }
                    }
                    return result;
                });
            }
            catch (Exception e)
            {
                logger.Warn("Delete old version failed, error {0}, name {1}", e, info.LowName);
                result = false;
            }
            return result;
        }

        private String GetAccessToken()
        {
            String accessToken;
            if (this.OpenParameter.AccessToken == null)
            {
                BoxConfigFileHandler config = new BoxConfigFileHandler(this.OpenParameter.ConfigLocation,
                    this.OpenParameter.ConfigUsername, this.OpenParameter.ConfigPassword, this.OpenParameter.EmailAddress, this.originalEmailAddress);
                if (config.ConfigFileExist())
                {
                    BoxAuthInfo AuthInfo = config.GetAuthInfo();
                    accessToken = AuthInfo.AccessToken;
                    this.OpenParameter.AccessToken = AuthInfo.AccessToken;
                    this.OpenParameter.ClientSecret = AuthInfo.ClientSecret;
                    this.OpenParameter.ClientId = AuthInfo.ClientId;
                }
                else
                {
                    throw new TokenNotFoundException("The config file is not exist.");
                }
            }
            else
            {
                accessToken = this.OpenParameter.AccessToken;
            }
            return accessToken;
        }

        private BoxAuthInfo CreateTokenByRefreshToken(String refreshToken)
        {
            BoxAuthInfo authInfo = new BoxAuthInfo();
            if (this.OpenParameter.ClientId.Equals(BoxDefaultClientId, StringComparison.OrdinalIgnoreCase))
            {
                var url = string.Format(StorageUrl.BoxGetAuthTokenWithDocAveOnline, HttpUtility.UrlEncode(XRI.ValueDecode(SecretUtil.EncryptPassword(refreshToken))));
                var result = RefreshAccessTokenFromAos(url);
                if (!string.IsNullOrEmpty(result) && !result.Contains("Error"))
                {
                    //由于AOS API 返回的字符串中，会自动走Jason序列化，在前后增加"，所以这里需要手动remove
                    authInfo.AccessToken = SecretUtil.DescryptPassword(result.Split(',')[0].Replace("\"", String.Empty));
                    authInfo.RefreshToken = SecretUtil.DescryptPassword(result.Split(',')[1].Replace("\"", String.Empty));
                }
                else
                {
                    throw new Exception("Can not refresh access token from AOS, Error : " + result);
                }
            }
            else
            {
                var url = StorageUrl.BoxAuthToken;
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                if (this.Proxy != null)
                {
                    request.Proxy = this.Proxy;
                    if (request.Proxy.Credentials != null)
                    {
                        request.PreAuthenticate = true;
                    }
                }
                var body = String.Format("grant_type=refresh_token&refresh_token={0}&client_id={1}&client_secret={2}",
                    refreshToken, this.OpenParameter.ClientId, this.OpenParameter.ClientSecret);
                using (Stream requestStream = request.GetRequestStream())
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(body.ToString());
                    requestStream.Write(buffer, 0, buffer.Length);
                }
                try
                {
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            var result = new StreamReader(stream).ReadToEnd();
                            Regex regex = new Regex("\"access_token\":\"([^\"]+)\",.+refresh_token\":\"([^\"]+)\"");
                            Match match = regex.Match(result);
                            if (match.Success)
                            {
                                authInfo.AccessToken = match.Groups[1].Value;
                                authInfo.RefreshToken = match.Groups[2].Value;
                                this.logger.Info("Get access_token and refresh_token successful.ClientId = {0}",
                                    this.OpenParameter.ClientId);
                            }
                            else
                            {
                                throw new Exception("Can not get access token and refresh token");
                            }
                        }
                    }
                }
                catch (WebException we)
                {
                    string response = string.Empty;
                    if (we.Response != null)
                    {
                        using (var stream = we.Response.GetResponseStream())
                        {
                            response = new StreamReader(stream).ReadToEnd().ToString();
                        }
                    }
                    logger.Error("Get refresh token and access token failed.Client id = {0}, message = {1}, Error = {2}", this.OpenParameter.ClientId, response, we);
                    throw;
                }
            }
            return authInfo;
        }

        public void ResetToken()
        {
            BoxAuthInfo tokenResult;
            String refreshToken;
            //String accessToken;
            BoxConfigFileHandler config = new BoxConfigFileHandler(this.OpenParameter.ConfigLocation,
                this.OpenParameter.ConfigUsername, this.OpenParameter.ConfigPassword, this.OpenParameter.EmailAddress, this.originalEmailAddress);
            //if system accessToken is equal with config accessToken,it means the config has not been changed.
            lock (locker)
            {
                var authInfo = config.GetAuthInfo();
                if (this.OpenParameter.AccessToken.Equals(authInfo.AccessToken, StringComparison.CurrentCultureIgnoreCase))
                {
                    refreshToken = authInfo.RefreshToken;
                    tokenResult = this.CreateTokenByRefreshToken(refreshToken);
                    config.CreateOrUpdateConfigFile(tokenResult.RefreshToken, tokenResult.AccessToken, DateTime.Now.Ticks.ToString(), this.OpenParameter.ClientSecret, authInfo.ClientId);
                    this.OpenParameter.AccessToken = tokenResult.AccessToken;
                }
                else
                {
                    if ((DateTime.Now.Ticks - long.Parse(authInfo.Time)) > 55 * 60 * 1000)
                    {
                        refreshToken = authInfo.RefreshToken;
                        tokenResult = this.CreateTokenByRefreshToken(refreshToken);
                        config.CreateOrUpdateConfigFile(tokenResult.RefreshToken, tokenResult.AccessToken, DateTime.Now.Ticks.ToString(), this.OpenParameter.ClientSecret, authInfo.ClientId);
                        this.OpenParameter.AccessToken = tokenResult.AccessToken;
                    }
                    else
                    {
                        this.OpenParameter.AccessToken = authInfo.AccessToken;
                    }
                }
            }
        }

        public String CreateShareLink(StorageInfo info)
        {
            BoxObject boxObject = new BoxObject();
            return Retry<String>(delegate ()
            {
                var url = String.Format(StorageUrl.BoxFileInfo, info.ObjectId);
                var request = this.GenerateRequest(BoxConstants.HttpMethod_PUT, url);
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(
                    "{\"shared_link\":{\"access\":\"company\",\"permissions\":{\"can_download\":true,\"can_preview\":false}}}");
                using (Stream requestStream = request.GetRequestStream())
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    requestStream.Write(buffer, 0, buffer.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            using (var streamReader = new StreamReader(stream))
                            {
                                var result = streamReader.ReadToEnd();
                                boxObject = ParseJsonString(result);
                            }
                        }
                    }
                }
                return boxObject.Shared_link.Url;
            });
        }

        public XFileInfo UpdateFileInfo(StorageInfo info)
        {
            XFileInfo fileInfo = new XFileInfo();
            return Retry<XFileInfo>(delegate ()
            {
                var url = String.Format(StorageUrl.BoxFileInfo, info.ObjectId);
                var request = this.GenerateRequest(BoxConstants.HttpMethod_PUT, url);
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("{\"modified_at\":\"").Append(DateTime.Now).Append("\"}");
                using (Stream requestStream = request.GetRequestStream())
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    requestStream.Write(buffer, 0, buffer.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            using (var streamReader = new StreamReader(stream))
                            {
                                var result = streamReader.ReadToEnd();
                                fileInfo = new BoxFileInfo(this, info.HighName, info.LowName, ParseJsonString(result));
                            }
                        }
                    }
                }
                return fileInfo;
            });
        }

        public override XFileInfo CreateFileSharedLink(StorageInfo info, AcessMode accessMode, Boolean canDownload)
        {
            var boxObject = new BoxObject();
            var url = String.Empty;
            return Retry<XFileInfo>(delegate ()
            {
                url = String.Format(StorageUrl.BoxFileInfo, info.ObjectId);
                var request = GenerateRequest("PUT", url);
                var stringBuilder = new StringBuilder();
                var baseAccess = (stringBuilder.Append(",")
                    .Append("\"permissions\":").Append("{")
                    .Append("\"can_download\":").Append(canDownload.ToString().ToLower(CultureInfo.InvariantCulture))
                    .Append("}")).ToString();
                if (accessMode.Equals(AcessMode.Collaborators))
                {
                    baseAccess = null;
                }
                stringBuilder = new StringBuilder();
                stringBuilder.Append("{\"shared_link\":{\"access\":").Append("\"").Append(accessMode).Append("\"")
                    .Append(baseAccess)
                    .Append("}}");
                Byte[] data = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            var result = streamReader.ReadToEnd();
                            boxObject = ParseJsonString(result);
                        }
                    }
                }
                return new BoxFileInfo(this, info.HighName, info.LowName, boxObject);
            });
        }

        public override XDirectoryInfo CreateFolderSharedLink(StorageInfo info, AcessMode accessMode, Boolean canDownload)
        {
            var boxObject = new BoxObject();
            var url = String.Empty;
            return Retry<XDirectoryInfo>(delegate ()
            {
                url = String.Format(StorageUrl.BoxFolderInfo, info.ClipId);
                var request = GenerateRequest("PUT", url);
                var stringBuilder = new StringBuilder();
                String baseAccess = (stringBuilder.Append(",").Append("\"permissions\":").Append("{")
                    .Append("\"can_download\":").Append(canDownload.ToString().ToLower(CultureInfo.InvariantCulture))
                    .Append("}")).ToString();
                if (accessMode.Equals(AcessMode.Collaborators))
                {
                    baseAccess = string.Empty;
                }
                stringBuilder = new StringBuilder();
                stringBuilder.Append("{\"shared_link\":{\"access\":").Append("\"").Append(accessMode).Append("\"")
                    .Append(baseAccess)
                    .Append("}}");
                Byte[] data = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            var result = streamReader.ReadToEnd();
                            boxObject = ParseJsonString(result);
                        }
                    }
                }
                return new BoxFolderInfo(this, info.HighName, info.LowName, boxObject);
            });
        }

        public override XFileInfo DisableFileSharedLink(StorageInfo info)
        {
            var boxObject = new BoxObject();
            var url = String.Empty;
            return Retry<XFileInfo>(delegate ()
            {
                url = String.Format(StorageUrl.BoxFileInfo, info.ObjectId);
                var request = GenerateRequest("PUT", url);
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("{\"shared_link\":null}");
                Byte[] data = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            var result = streamReader.ReadToEnd();
                            boxObject = ParseJsonString(result);
                        }
                    }
                }
                return new BoxFileInfo(this, info.HighName, info.LowName, boxObject);
            });
        }

        public override XDirectoryInfo DisableFolderSharedLink(StorageInfo info)
        {
            var boxObject = new BoxObject();
            var url = String.Empty;
            return Retry<XDirectoryInfo>(delegate ()
            {
                url = String.Format(StorageUrl.BoxFolderInfo, info.ClipId);
                var request = GenerateRequest("PUT", url);
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("{\"shared_link\":null}");
                Byte[] data = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            var result = streamReader.ReadToEnd();
                            boxObject = ParseJsonString(result);
                        }
                    }
                }
                return new BoxFolderInfo(this, info.HighName, info.LowName, boxObject);
            });
        }

        public List<XFileInfo> GetFileVersion(String fileId, String highName, String lowName)
        {
            var boxFileVersionList = new List<XFileInfo>();
            return Retry<List<XFileInfo>>(delegate ()
            {
                var boxObject = new BoxObject();
                var url = String.Format(StorageUrl.BoxListFileVersion, fileId);
                var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            var streamReader = new StreamReader(stream);
                            var result = streamReader.ReadToEnd();
                            boxObject = ParseJsonString(result);
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("Get File Version Failed, file ID = {0}", fileId));
                    }
                }
                if (boxObject.Entries == null || boxObject.Entries.Count == 0)
                {
                    boxFileVersionList = null;
                }
                else
                {
                    foreach (var dto in boxObject.Entries)
                    {
                        boxFileVersionList.Add(new BoxFileInfo(this, highName, dto.Name, dto));
                    }
                    boxFileVersionList = SortFileVersion(boxFileVersionList);
                }
                return boxFileVersionList;
            });
        }

        private List<XFileInfo> SortFileVersion(List<XFileInfo> boxFileVersionList)
        {
            var isRetry = true;
            var counter = 1;
            while (isRetry)
            {
                isRetry = false;
                for (Int32 k = 0; k < boxFileVersionList.Count - 1; k++)
                {
                    if (boxFileVersionList[k].LastWriteTimeUtc > boxFileVersionList[k + 1].LastWriteTimeUtc)
                    {
                        var temp = boxFileVersionList[k];
                        boxFileVersionList[k] = boxFileVersionList[k + 1];
                        boxFileVersionList[k + 1] = temp;
                        isRetry = true;
                    }
                }
                counter++;
            }
            return boxFileVersionList;
        }

        public StorageMoveResult MoveFileWithAllVersion(StorageInfo sourceFileInfo, StorageInfo targetFileInfo,
            bool isOverWrite)
        {
            return Retry<StorageMoveResult>(delegate ()
            {
                var moveResult = new StorageMoveResult();
                var boxObject = new BoxObject();
                var url = String.Format(StorageUrl.BoxFileInfo, sourceFileInfo.ObjectId);
                var request = GenerateRequest(BoxConstants.HttpMethod_PUT, url);
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("{\"parent\": {\"")
                    .Append("id\": \"")
                    .Append(targetFileInfo.ClipId)
                    .Append("\"")
                    .Append("}}");
                using (var requestStream = request.GetRequestStream())
                {
                    Byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    requestStream.Write(buffer, 0, buffer.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            using (var streamReader = new StreamReader(stream))
                            {
                                var result = streamReader.ReadToEnd();
                                boxObject = ParseJsonString(result);
                            }
                        }
                        moveResult.IsMoved = true;
                        var uri = new XURIResult();
                        uri.SysId = this.SystemID;
                        uri.SdType = 408;
                        uri.SInfo.ObjectId = boxObject.Id;
                        moveResult.URI = uri;
                        return moveResult;
                    }
                    else
                    {
                        throw new Exception(
                            String.Format("Move file with all version failed.highName = {0}, lowName = {1}",
                                sourceFileInfo.HighName, sourceFileInfo.LowName));
                    }
                }
            });
        }

        private StorageCopyResult CopyFileOldVersion(StorageInfo sourceFileInfo, StorageInfo targetFileInfo)
        {
            return Retry<StorageCopyResult>(delegate ()
            {
                var copyResult = new StorageCopyResult();
                var storageInfo = targetFileInfo.Clone();
                StorageResult storageResult;
                //storageInfo.Length = this.GetFileInfo(sourceFileInfo).Size;
                using (var sourceStream = this.OpenStream(sourceFileInfo, FileMode.Open))
                {
                    using (var stream = this.OpenStream(storageInfo, FileMode.CreateNew))
                    {
                        var buffer = new byte[64 * 1024];
                        var len = 0;
                        while ((len = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            stream.Write(buffer, 0, len);
                        }
                        storageResult = stream.Commit();
                    }
                }
                copyResult.IsCopyed = storageResult.IsCommited;
                copyResult.URI = storageResult.URI;
                return copyResult;
            });
        }

        public override bool LockFile(StorageInfo info)
        {
            return Retry<Boolean>(delegate ()
            {
                var url = String.Format(StorageUrl.BoxLockFile, info.ObjectId);
                var request = GenerateRequest(BoxConstants.HttpMethod_PUT, url);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("{\"lock\":{\"is_download_prevented\":false}}");
                using (Stream reqStream = request.GetRequestStream())
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    reqStream.Write(buffer, 0, buffer.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception("Can not lock the file, File object id is " + info.ObjectId);
                    }
                }
            });
        }

        public override bool UnlockFile(StorageInfo info)
        {
            return Retry<Boolean>(delegate ()
            {
                var url = String.Format(StorageUrl.BoxFileInfo, info.ObjectId);
                var request = GenerateRequest(BoxConstants.HttpMethod_PUT, url);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("{\"lock\":null}");
                using (Stream reqStream = request.GetRequestStream())
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    reqStream.Write(buffer, 0, buffer.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception("Can not lock the file, File object id is " + info.ObjectId);
                    }
                }
            });
        }

        public bool IsLocked(StorageInfo info)
        {
            return Retry<Boolean>(delegate ()
            {
                var url = String.Format(StorageUrl.BoxLockFile, info.ObjectId);
                var request = GenerateRequest(BoxConstants.HttpMethod_GET, url);
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        BoxObject boxObject;
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            boxObject = ParseJsonString(result);
                        }
                        if (boxObject != null && boxObject.Lock != null)
                        {
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        throw new Exception("Can not get the lock status, File object id is " + info.ObjectId);
                    }
                }
            });
        }
    }
}
