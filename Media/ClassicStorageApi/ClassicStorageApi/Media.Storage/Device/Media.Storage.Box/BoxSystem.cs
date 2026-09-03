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


using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using AvePoint.Media.ClassicStorage.Util;
using AvePoint.Media.ClassicStorage.Inner;
using System.Globalization;
using Newtonsoft.Json;
using Storage;
using XSystemHealth = Storage.XSystemHealth;
//using FileBlockType = Storage.FileBlockType;
using StorageInterfaceType = Storage.StorageInterfaceType;
using AvePoint.Media.StorageApi;
using AvePoint.GCommon;

namespace AvePoint.Media.ClassicStorage.Box
{

    public class BoxSystem : AbstractXSystem
    {
        #region Field and property
        private AveLogger logger = AveLogger.GetInstance(typeof(BoxSystem));
        public BoxOpenParameter OpenParameter { get; set; }
        public delegate T RetryDelegate<T>();
        private BoxConfigFileHandler config;
        public string LastMetaId { get; set; }
        public string LastContentId { get; set; }
        public StorageInfo LastStreamInfo { get; set; }
        public override StorageInterfaceType StorageInterfaceType
        {
            get
            {
                return StorageInterfaceType.Object;
            }
        }
        public override string SystemPath
        {
            get
            {
                return this.OpenParameter.RootFolderName;
            }
        }
        #endregion

        static BoxSystem()
        {
            ServicePointManager.DefaultConnectionLimit = 1024;
        }

        public BoxSystem(string xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            //this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            this.SystemHealth = XSystemHealth.Unknown;
            this.IsSupportAutoChangeDataBlock = true;
            this.SpaceThresholdUnit = SpaceThresholdUnit.MB;
            this.OpenParameter = new BoxOpenParameter();

        }

        #region Method

        public override StorageOpenValidResult Open()
        {
            base.Open();
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Client_ID))
            {
                this.OpenParameter.ClientId = XriObject.Params[XRIParameterKeys.Box_Client_ID];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Client_Secret))
            {
                //this.logger.Error(XriObject.Params[XRIParameterKeys.Box_Client_Secret]);
                this.OpenParameter.ClientSecret = SecretUtil.Decrypt(XriObject.Params[XRIParameterKeys.Box_Client_Secret]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.BLOCK_LENGTH))
            {
                this.OpenParameter.BlockLength = int.Parse(XriObject.Params[XRIParameterKeys.BLOCK_LENGTH]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Root_Folder_Id))
            {
                this.OpenParameter.RootFolderId = XriObject.Params[XRIParameterKeys.Root_Folder_Id];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Root_Folder_Name))
            {
                this.OpenParameter.RootFolderName = XriObject.Params[XRIParameterKeys.Box_Root_Folder_Name];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Refresh_Token))
            {
                //this.logger.Error(XriObject.Params[XRIParameterKeys.Box_Refresh_Token]);
                this.OpenParameter.RefreshToken = SecretUtil.Decrypt(XriObject.Params[XRIParameterKeys.Box_Refresh_Token]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Box_Email_Address))
            {
                this.OpenParameter.EmailAddress = XriObject.Params[XRIParameterKeys.Box_Email_Address];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.BOX_VALIDATE_KEY))
            {
                this.OpenParameter.IsValidate = bool.Parse(XriObject.Params[XRIParameterKeys.BOX_VALIDATE_KEY]);
            }
            if (!this.OpenParameter.IsValidate)
            {
                this.config = new BoxConfigFileHandler(this.OpenParameter.ClientId, this.OpenParameter.EmailAddress, this.OpenParameter.RefreshToken);
            }
            else 
            {
                this.config = new BoxConfigFileHandler(this.OpenParameter.ClientId, this.OpenParameter.EmailAddress);
            }
            if (this.XriObject.Params.ContainsKey(XRIParameterKeys.CREATE_IF_NOT_EXISTS))
            {
                this.CreateIfNotExists = Boolean.Parse(this.XriObject.Params[XRIParameterKeys.CREATE_IF_NOT_EXISTS]);
            }
            if (string.IsNullOrEmpty(this.OpenParameter.RootFolderId))
            {
                try
                {
                    this.OpenParameter.RootFolderId = GetRootFolderId();
                }
                catch (PathNotFoundException)
                {
                    if (this.CreateIfNotExists)
                    {
                        this.OpenParameter.RootFolderId = this.CreateRootFolder(this.OpenParameter.RootFolderName);
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Invalid refresh token"))
                    {
                        if (config.DeleteConfig())
                        {
                            this.OpenParameter.RootFolderId = this.GetRootFolderId();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            this.IsRetry = true;
            this.SystemHealth = XSystemHealth.AvailableAndNotFull;
            this.Type = "BoxSystem";
            this.TypeValue = 9;
            SetSystemDescription();
            return new StorageOpenValidResult();
        }


        public SpaceInfo CheckFreeSpace()
        {
            return Retry<SpaceInfo>(delegate()
            {
                SpaceInfo spaceInfo = new SpaceInfo();
                string url = "https://api.box.com/2.0/users/me";
                var request = this.GenerateRequest("GET", url);
                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("CheckFreeSpace failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                    }
                    Stream respStream = resp.GetResponseStream();
                    using (StreamReader sr = new StreamReader(respStream))
                    {
                        string space = sr.ReadToEnd();
                        spaceInfo.TotalSpace = UInt64.Parse(BoxUtil.ParseSpaceField(space, "\"space_amount\":[^,]+"),
                            NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);
                        spaceInfo.TotalUsedSpace = UInt64.Parse(BoxUtil.ParseSpaceField(space, "\"space_used\":[^,]+"),
                            NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);
                        spaceInfo.LoginEmail = BoxUtil.ParseSpaceField(space, "\"login\":[^,]+").Trim('"');
                        spaceInfo.TotalFreeSpace = spaceInfo.TotalSpace - spaceInfo.TotalUsedSpace;
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
            StorageOpenValidResult rs = null;
            try
            {
                rs = new StorageOpenValidResult();
                if (config.ValidateResult.SystemHealth != XSystemHealth.AvailableAndNotFull)
                {
                    config.Close();
                    throw new Exception("The config location is not available.");
                }
                if (string.IsNullOrEmpty(this.OpenParameter.RootFolderId))
                {
                    try
                    {
                        this.OpenParameter.RootFolderId = GetRootFolderId();
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("Invalid refresh token"))
                        {
                            if (config.DeleteConfig())
                            {
                                this.OpenParameter.RootFolderId = this.GetRootFolderId();
                            }
                            else
                            {
                                throw;
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                    this.Properties[XRIParameterKeys.AppendConnectionStringKey] = "&" + XRIParameterKeys.Root_Folder_Id + "=" + this.OpenParameter.RootFolderId;
                }
                //else
                //{
                //    this.Properties[XRIParameterKeys.AppendConnectionStringKey] = "&" + XRIParameterKeys.Root_Folder_Id + "=" + this.OpenParameter.RootFolderId;
                //}
                SpaceInfo spaceInfo = CacheUtil.GetSpaceInfo(VIMName.Box, this.XriString, new CheckFreeSpace(this.CheckFreeSpace));
                if ("6wlvcp6l8tujowomdwrbjtqlwhdxzqfq".Equals(this.OpenParameter.ClientId, StringComparison.OrdinalIgnoreCase) && !spaceInfo.LoginEmail.Equals(this.OpenParameter.EmailAddress, StringComparison.InvariantCultureIgnoreCase))
                {
                    rs.SystemHealth = XSystemHealth.AuthenticationFailed;
                }
                else
                {
                    rs.TotalSpace = this.totalSpace = spaceInfo.TotalSpace;
                    rs.TotalFreeSpace = this.totalFreeSpace = spaceInfo.TotalFreeSpace;
                    rs.TotalUsedSpace = this.totalUsedSpace = spaceInfo.TotalUsedSpace;
                    rs.IsReadAble = true;
                    rs.IsDeleteAble = true;
                    rs.IsHasPermission = true;
                    if (ValidateIsFull())
                    {
                        rs.SystemHealth = XSystemHealth.Available;
                    }
                    else
                    {
                        rs.IsWriteAble = true;
                        rs.SystemHealth = XSystemHealth.AvailableAndNotFull;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Validate Error:", ex);
                rs.SystemHealth = XSystemHealth.AuthenticationFailed;
            }
            finally
            {
                this.SystemHealth = rs.SystemHealth;
            }
            return rs;
        }

        internal HttpWebRequest GenerateRequest(String method, String url)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request != null)
            {
                request.Headers.Add("Authorization", string.Format("Bearer {0}", this.GetAccessToken()));
                request.Method = method;
            }
            return request;
        }

        public string GetRootFolderId()
        {
            //return Retry<string>(delegate()
            //{
            //    string result = null;
            //    string jsonStr = null;
            //    //The root folder of a Box account is always represented by the id “0″.
            //    string url = "https://api.box.com/2.0/folders/0";
            //    var request = this.GenerateRequest("GET", url);
            //    using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
            //    {
            //        if (resp.StatusCode != HttpStatusCode.OK)
            //        {
            //            throw new Exception("GetRootFolderID method failed");
            //        }
            //        using (Stream respStream = resp.GetResponseStream())
            //        {
            //            using (StreamReader sr = new StreamReader(respStream))
            //            {
            //                jsonStr = sr.ReadToEnd();
            //                result = parseJsonString(jsonStr);
            //                if (string.IsNullOrEmpty(result))
            //                {
            //                    result = CreateRootFolder(this.OpenParameter.RootFolderName);
            //                }
            //            }
            //        }
            //    }
            //    request.Abort();
            //    return result;
            //});
            var boxObjectList = new List<BoxObject>();
            try
            {
                var countOfLevel = 0;
                String[] array = this.OpenParameter.RootFolderName.Replace("/", "\\")
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
                logger.Error("Get folder {0} id error:{1}", this.OpenParameter.RootFolderName, ex);
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
            return Retry<BoxObject>(delegate()
            {
                var boxObject = new BoxObject();
                var url = String.Format(@"https://api.box.com/2.0/folders/{0}/items?limit={1}&offset={2}", id, limit,
                    offset);
                var request = GenerateRequest("GET", url);
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
            //var js = new JavaScriptSerializer();
            var dicJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
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

        /*private string parseJsonString(string jsonStr)
        {
            string result = string.Empty;
            //JavaScriptSerializer s = new JavaScriptSerializer();
            Dictionary<string, object> JsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
            foreach (KeyValuePair<string, object> pair in JsonData)
            {
                if (pair.Key.Equals("item_collection"))
                {
                    Dictionary<string, object> folders = (Dictionary<string, object>)pair.Value;
                    object[] rows = (object[])folders["entries"];
                    foreach (object o in rows)
                    {
                        Dictionary<string, object> innerFolder = (Dictionary<string, object>)o;
                        string tempStr = string.Empty;
                        foreach (KeyValuePair<string, object> entry in innerFolder)
                        {
                            if ("id".Equals(entry.Key, StringComparison.OrdinalIgnoreCase))
                            {
                                tempStr = (string)entry.Value;
                            }
                            if ("name".Equals(entry.Key, StringComparison.OrdinalIgnoreCase)
                                && this.OpenParameter.RootFolderName.Equals((string)entry.Value, StringComparison.OrdinalIgnoreCase))
                            {
                                result = tempStr;
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(result))
                        {
                            break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(result))
                {
                    break;
                }
            }
            return result;
        }*/

        private string CreateRootFolder(string fileName)
        {
            return Retry<string>(delegate()
            {
                string result = null;
                StringBuilder sb = new StringBuilder();
                string url = "https://api.box.com/2.0/folders";
                var request = this.GenerateRequest("POST", url);
                request.ContentType = "application/json";
                sb.Append("{ \"name\": \"")
                  .Append(fileName)
                  .Append("\", \"parent\": {\"id\":\"0\"}")
                  .Append(" }");
                using (Stream reqStream = request.GetRequestStream())
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
                    reqStream.Write(buffer, 0, buffer.Length);
                }
                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.Created)
                    {
                        throw new Exception(string.Format("CreateRootFolder failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                    }
                    Stream respStream = resp.GetResponseStream();
                    using (StreamReader sr = new StreamReader(respStream))
                    {
                        string jsonStr = sr.ReadToEnd();
                        result = GetNewRootFolderId(jsonStr);
                    }
                }
                request.Abort();
                return result;
            });
        }

        public static string GetNewRootFolderId(string jsonStr)
        {
            Match m = Regex.Match(jsonStr, "\"id\":\"[^\"]+");
            string[] tempStrs = m.Groups[0].Value.Split(':');
            return tempStrs[1].Substring(1);
        }

        public string[] SplitFileID(string fileID)
        {
            return fileID.Split(new string[] { BoxConstant.FILE_ID_SEPARATOR }, StringSplitOptions.None);
        }

        public override bool FileExists(StorageInfo info)
        {
            CheckState();
            try
            {
                return Retry<bool>(delegate()
                {
                    string url = string.Format("https://api.box.com/2.0/files/{0}", SplitFileID(info.ObjectId)[0]);
                    var request = this.GenerateRequest("GET", url);
                    using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                    {
                        if (resp.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception(string.Format("ReadFilePoperties failed, StatusCode={0} URL={1}", resp.StatusCode, url));
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

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            CheckState();
            StorageDeleteResult rs = new StorageDeleteResult();
            //标记执行过删除
            Deletion = true;
            return rs;
        }

        public override bool DirectoryExists(StorageInfo info)
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
                    var request = GenerateRequest(BoxConstant.HttpMethod_GET, url);
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
                        string etag = GetEtag(id);
                        string url = string.Format("https://api.box.com/2.0/files/{0}", id);
                        var request = this.GenerateRequest("DELETE", url);
                        request.Headers.Add("If-Match", etag);
                        using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                        {
                            if (resp.StatusCode != HttpStatusCode.NoContent && resp.StatusCode != HttpStatusCode.OK)
                            {
                                throw new Exception(string.Format("DeleteFile failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                            }
                        }
                        request.Abort();
                    }
                    logger.Debug("DeleteFile success, fileID:" + info.ObjectId);
                    return true;
                });
            }
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            CheckState();
            BoxStream stream = null;
            if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew || fileMode == FileMode.Truncate || fileMode == FileMode.OpenOrCreate)
            {
                this.Written = true;
                try
                {
                    stream = new BoxStream(info, this);
                    stream.InitWriteStream(info);
                    this.LastStreamInfo = info.Clone();
                }
                catch (Exception e)
                {
                    logger.Error("Open stream failed, {0}", e);
                    throw;
                }
                return stream;
            }
            else
            {
                return DownloadFile(info);
            }
        }

        public override void MergeStorageInfo<T>(List<T> ts, StorageResult rs, PropertyInfo p)
        {
            CheckState();
            if (!string.IsNullOrEmpty(rs.StorageInfo))
            {
                string value = null;
                if (this.LastStreamInfo != null && this.LastStreamInfo.DataType == DataBlockType.MetaData)
                {
                    BoxStorageInfo casInfo = BoxUtil.Convert2CAStorStorageInfo(rs.StorageInfo);
                    foreach (T index in ts)
                    {
                        value = p.GetValue(index, null) as string;
                        BoxStorageInfo cas = BoxUtil.Convert2CAStorStorageInfo(value);
                        if (string.IsNullOrEmpty(cas.MetaId))
                        {
                            cas.MetaId = casInfo.ContentId;
                            p.SetValue(index, BoxUtil.Convert2StorageInfo(cas), null);
                        }
                    }
                    rs.NeedCommit = true;
                }
                else
                {
                    BoxStorageInfo casInfo = BoxUtil.Convert2CAStorStorageInfo(rs.StorageInfo);
                    foreach (T index in ts)
                    {
                        value = p.GetValue(index, null) as string;
                        BoxStorageInfo cas = BoxUtil.Convert2CAStorStorageInfo(value);
                        if (string.IsNullOrEmpty(cas.ContentId))
                        {
                            cas.ContentId = casInfo.ContentId;
                            p.SetValue(index, BoxUtil.Convert2StorageInfo(cas), null);
                        }
                    }
                }
            }
        }

        private void AddHeadersWithoutValidate(HttpWebRequest req, string key, string value)
        {
            MethodInfo method = req.Headers.GetType().GetMethod("AddWithoutValidate",
                                    BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null,
                                    new Type[] { typeof(string), typeof(string) }, null);
            method.Invoke(req.Headers, new object[] { key, value });
        }

        public XStream DownloadFile(StorageInfo info)
        {
            string fileID = info.ObjectId;
            long initOffset = info.Offset;
            XStream boxStream = null;
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
            #region new BoxStream
            boxStream = new BoxStream(this, info, respStream, (buffer, offset, count) =>
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

            return boxStream;
        }

        public string GetNextMetaID(string fileID)
        {
            string result = null;
            string propertiesStr = ReadFileProperties(fileID);
            Regex r = new Regex("\\'" + BoxConstant.META_ID_HEADER + "\\'\\:\\s*\\'([^\\']+)'");
            Match m = r.Match(propertiesStr);
            if (m.Success)
            {
                result = m.Groups[1].Value;
            }
            return result;
        }


        private string GetEtag(string fileID)
        {
            string result = null;
            string propertiesStr = ReadFileProperties(fileID);
            Regex r = new Regex("\"etag\":\"[^,]+\"");
            Match m = r.Match(propertiesStr);
            if (m.Success)
            {
                string[] tempStrs = m.Groups[0].Value.Split(':');
                result = tempStrs[1].Substring(1, tempStrs[1].Length - 2);
            }
            return result;
        }

        private Stream GetDownloadStream(string fileID, long initOffset = 0)
        {
            return Retry<Stream>(delegate()
            {
                string url = string.Format("https://api.box.com/2.0/files/{0}/content", fileID);
                var request = this.GenerateRequest("GET", url);
                if (initOffset > 0)
                {
                    AddHeadersWithoutValidate(request, "Range", "bytes=" + initOffset + "-");
                }
                HttpWebResponse resp = request.GetResponse() as HttpWebResponse;

                if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.PartialContent)
                {
                    throw new Exception(string.Format("DownloadFile failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                }
                return resp.GetResponseStream();
            });
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            CheckState();
            long fileSize = 0;
            string[] ids = SplitFileID(fileInfo.ObjectId);
            foreach (string fileID in ids)
            {
                string propertiesStr = ReadFileProperties(fileID);
                Regex r = new Regex("\"size\":[0-9]+");
                Match m = r.Match(propertiesStr);
                if (!m.Success)
                {
                    throw new Exception(string.Format(
                        "Match properties string failed, fileID={0}, properties={1}", fileID, propertiesStr));
                }
                string[] tempStrs = m.Groups[0].Value.Split(':');
                fileSize += long.Parse(tempStrs[1]);
            }
            return new BoxFileInfo(fileInfo.HighName, fileInfo.LowName, fileSize, fileInfo.ObjectId);
        }

        public string ReadFileProperties(string fileID)
        {
            fileID = SplitFileID(fileID)[0];
            return Retry<string>(delegate()
            {
                string result = null;
                string url = string.Format("https://api.box.com/2.0/files/{0}", fileID);
                var request = this.GenerateRequest("GET", url);
                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("ReadFilePoperties failed, StatusCode={0} URL={1}", resp.StatusCode, url));
                    }
                    using (Stream respStream = resp.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            result = sr.ReadToEnd();
                        }
                    }
                }
                request.Abort();
                return result;
            });

        }

        public override void Close()
        {
            if (config != null)
            {
                config.Close();
            }
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
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        HttpWebResponse resp = ex.Response as HttpWebResponse;
                        if (resp.StatusCode == HttpStatusCode.Unauthorized
                            || (int)resp.StatusCode == 420)
                        {
                            ResetToken();
                            continue;
                        }
                        else if (resp.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new PathNotFoundException(ex.Message, ex);
                        }
                        else if (resp.StatusCode == HttpStatusCode.InternalServerError || resp.StatusCode == HttpStatusCode.RequestTimeout || resp.StatusCode == HttpStatusCode.ServiceUnavailable || resp.StatusCode == HttpStatusCode.GatewayTimeout || resp.StatusCode == HttpStatusCode.BadGateway)
                        {
                            logger.Info("this exception is a connection fail exception:" + ex.Message);
                            if (counter < this.MaxRetryCount)
                            {
                                logger.Info("Retry after " + this.RetryInterval + " ms. Retry count: " + counter);
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
                        logger.Info("this exception is a connection fail exception:" + ex.Message);
                        if (counter < this.MaxRetryCount)
                        {
                            logger.Info("Retry after " + this.RetryInterval + " ms. Retry count: " + counter);
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
                        logger.Error("execute request failed:" + ex.Message, ex);
                        throw;
                    }
                }
            }
        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "Box Object Storage Server";
            List<string> keys = new List<string>();
            keys.Add(this.OpenParameter.RootFolderId);
            List<string> securityKeys = new List<string>();
            keys.Add(this.OpenParameter.ClientId);
            keys.Add(this.OpenParameter.RefreshToken);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        private String GetAccessToken()
        {
            //String accessToken;
            String refreshToken;
            BoxAuthInfo tokenResult;
            if (this.OpenParameter.AccessToken == null)
            {
                if (config.ConfigFileExist() && config.OriginalTokenExist())
                {
                    logger.Debug("get token from config file.");
                    this.OpenParameter.AccessToken = config.GetAuthInfo().AccessToken;
                }
                else
                {
                    refreshToken = this.OpenParameter.RefreshToken;
                    tokenResult = this.CreateTokenByRefreshToken(refreshToken);
                    this.OpenParameter.AccessToken = tokenResult.AccessToken;
                    if (this.OpenParameter.IsValidate)
                    {
                        config.UpdateEmailAddress(CheckFreeSpace().LoginEmail);
                    }
                    config.ConfigOrUpdateConfigFile(tokenResult.RefreshToken, tokenResult.AccessToken, DateTime.Now.Ticks.ToString());
                }
            }
            return this.OpenParameter.AccessToken;
        }

        private BoxAuthInfo CreateTokenByRefreshToken(String refreshToken)
        {
            BoxAuthInfo authInfo = new BoxAuthInfo();
            var url = "https://www.box.com/api/oauth2/token";
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            using (Stream requestStream = request.GetRequestStream())
            {
                byte[] buffer = Encoding.ASCII.GetBytes(($"grant_type=refresh_token&refresh_token={refreshToken}&client_id={this.OpenParameter.ClientId}&client_secret={this.OpenParameter.ClientSecret}").ToString());
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
                            this.logger.Info("Get access token and refresh token successful.RefreshToken is {0}", !string.IsNullOrEmpty(authInfo.RefreshToken));
                        }
                        else
                        {
                            throw new Exception("Can not get access token and refresh token, the response is " + result);
                        }
                    }
                }
                return authInfo;
            }
            catch (WebException we)
            {
                using (var response = we.Response)
                {
                    using (var stream = response.GetResponseStream())
                    {
                        var result = new StreamReader(stream).ReadToEnd().ToString();
                        logger.Error("Get refresh_token and access_token failed.Message = {0}", we.Message);
                        throw new Exception(result);
        }
                    }
                }
            }

        private void ResetToken()
        {
            BoxAuthInfo tokenResult = null;
            int retryCount = 0;
            logger.Info("The refresh token has expired, and start to refresh it.");
            while (true)
            {
                retryCount++;
                try
                {
                    var info = config.GetAuthInfo();
                    if (this.OpenParameter.AccessToken == info.AccessToken)//if system accessToken is equal with config accessToken,it means the config has not been changed.
                    {
                        tokenResult = this.CreateTokenByRefreshToken(info.RefreshToken);
                        this.OpenParameter.AccessToken = tokenResult.AccessToken;
                        if (this.OpenParameter.IsValidate)
                        {
                            config.UpdateEmailAddress(CheckFreeSpace().LoginEmail);
                        }
                        config.ConfigOrUpdateConfigFile(tokenResult.RefreshToken, tokenResult.AccessToken, DateTime.Now.Ticks.ToString());
                    }
                    else
                    {
                        var timeSpan = DateTime.Now.Ticks - long.Parse(info.Time);
                        TimeSpan elapsedSpan = new TimeSpan(timeSpan);
                        var time = (int)elapsedSpan.TotalSeconds;
                        if (time > 55 * 60)
                        {
                            tokenResult = this.CreateTokenByRefreshToken(info.RefreshToken);
                            this.OpenParameter.AccessToken = tokenResult.AccessToken;
                            if (this.OpenParameter.IsValidate)
                            {
                                config.UpdateEmailAddress(CheckFreeSpace().LoginEmail);
                            }
                            config.ConfigOrUpdateConfigFile(tokenResult.RefreshToken, tokenResult.AccessToken, DateTime.Now.Ticks.ToString());
                        }
                        else
                        {
                            this.OpenParameter.AccessToken = info.AccessToken;
                        }
                    }
                    break;
                }
                catch (Exception e)
                {
                    if (retryCount < 6)
                    {
                        logger.Warn("Error occured when refresh the access token, {0}", e);
                        Thread.Sleep(5 * 1000);
                    }
                    else
                    {
                        logger.Error("Refresh the token failed, error is {1}", e);
                        throw;
                    }
                }
            }
            logger.Info("Reset token done.");
        }


        #endregion Methond
    }
}
