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




using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Media.Storage.Inner;
using AvePoint.Media.Storage.Resources.FSI18N;
using AvePoint.Media.Storage.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;

namespace AvePoint.Media.Storage.FS
{
    #region CodeReview
    [AveCodeReview(
    "2012/5/23",
    "rongbiao.sun@avepoint.com",
    "yanxin.fu@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_HC_1 },
    null,
     true)]
    [AveCodeReview(
    "2013/6/6",
    "chunhui.li@avepoint.com",
    "nan.shen@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_FA_9 },
    null,
     true)]
    [AveCodeReview(
    "2013/7/1",
    "chunhui.li@avepoint.com",
    "shouqiang.liu@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_LOG_1, CodeReviewConstants.CHECK_LIST_ID_LOG_2, CodeReviewConstants.CHECK_LIST_ID_LOG_3 },
    null,
     true,
     new string[] { CodeReviewConstants.CHECK_LIST_ID_LOG_1 })]
    [AveCodeReview(
    "2013/9/19",
    "da.sun@avepoint.com",
    "da.sun@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_CS_2, CodeReviewConstants.CHECK_LIST_ID_CS_3 },
    "ADO-89254",
     true,
     new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_3 })]
    #endregion
    class FSSystem : AbstractXSystem
    {
        StorageLogger logger = new StorageLogger(typeof(FSSystem));

        private UInt64 diskTotalSpace;
        private Boolean securelyDelete;
        private Int32 bufferSize = 1024 * 1024;
        private String oSystemLocation;
        private string dfsName;
        private DFSENUMLEVEL enumLevel = DFSENUMLEVEL.EnumRoot;
        private bool readFailover;
        internal bool ReadFailover { get { return readFailover; } }
        private string readFailoverPrefix = "";
        private List<string> readFailoverLocations;
        internal List<string> ReadFailoverLocations { get { return readFailoverLocations; } }
        private AuthMethod authMethod = AuthMethod.LogonUser;
        private IFSClient alphaFSClient;
        private IFSClient fsClient;
        
        public FSIdentity Identity { set; get; }
        public String SystemDomain { get; set; }
        public String SystemUserName { get; set; }
        public String SystemPassword { get; set; }
        public FileOptions FileOptions { get; set; }

        public AuthMethod AuthMethod
        {
            get { return this.authMethod; }
            set { this.authMethod = value; }
        }
        public override UInt64 TotalFreeSpace
        {
            get
            {
                UpdateSpaceParams();
                return this.innerTotalFreeSpace;
            }
        }
        public override UInt64 TotalSpace
        {
            get
            {
                UpdateSpaceParams();
                return this.innerTotalSpace;
            }
        }
        public override UInt64 TotalUsedSpace
        {
            get
            {
                UpdateSpaceParams();
                return this.innerTotalUsedSpace;
            }
        }
        public override UInt64 AvailableSpace
        {
            get
            {
                return getDeviceAvailable();
            }
        }
        public override Boolean IsFull
        {
            get
            {
                UpdateSpaceParams();
                return ValidateIsFull();
            }
        }

        #region 构造函数
        public FSSystem() : this(null, null) { }
        public FSSystem(string xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            this.SystemHealth = XSystemHealth.Unknown;
            this.innerTotalFreeSpace = long.MaxValue - 1;
            this.createIfNotExist = false;
            this.Open();
        }
        #endregion

        #region 常用方法
        private UInt64 getDeviceAvailable()
        {
            UpdateSpaceParams();
            UInt64 availableSpace = 0;
            if (!IsFull)
            {
                if (this.SpaceThresholdUnit == SpaceThresholdUnit.MB)
                {
                    availableSpace = this.TotalFreeSpace - this.SpaceThreshold * 1024 * 1024;
                }
                else     //(this.SpaceThresholdUnit == SpaceThresholdUnit.PERCENT)
                {
                    availableSpace = this.TotalFreeSpace - (UInt64)(this.TotalSpace * (this.SpaceThreshold / 100.0));
                }
            }
            return availableSpace;
        }
        public SpaceInfo CheckFreeSpace()
        {
            var spaceInfo = new SpaceInfo();
            using (Identity.Impersonate())
            {
                UInt64 totalFreeSpace;
                UInt64 totalSpace;
                AveFileSystemUtil.GetDiskSpace(this.SystemLocation, out totalFreeSpace, out totalSpace, out diskTotalSpace);
                this.innerTotalFreeSpace = totalFreeSpace;
                this.innerTotalSpace = totalSpace;
                this.innerTotalUsedSpace = this.innerTotalSpace - this.innerTotalFreeSpace;
            }
            spaceInfo.TotalSpace = this.innerTotalSpace;
            spaceInfo.TotalFreeSpace = this.innerTotalFreeSpace;
            spaceInfo.TotalUsedSpace = this.innerTotalUsedSpace;
            return spaceInfo;
        }
        public virtual void UpdateSpaceParams()
        {
            CheckState();
            if (this.SystemHealth >= Util.XSystemHealth.Available)
            {
                var spaceInfo = CacheUtil.GetSpaceInfo(VIMName.FS, this.SystemLocation, CheckFreeSpace);
                this.innerTotalSpace = spaceInfo.TotalSpace;
                this.innerTotalFreeSpace = spaceInfo.TotalFreeSpace;
                this.innerTotalUsedSpace = spaceInfo.TotalUsedSpace;
            }
            else
            {
                this.innerTotalFreeSpace = 0;
                this.innerTotalUsedSpace = 0;
                this.innerTotalSpace = 0;
            }
        }
        private void GenerateOptimalSystemLocation()
        {
            var tmp = this.SystemLocation;
            try
            {
                UNCObject obj = UNCObject.ValueOf(this.SystemLocation);
                //IPV6
                this.SystemLocation = obj.ToLocation();
                //To Local
                using (Identity.Impersonate())
                {
                    if (this.SystemLocation.StartsWith(FSSystemConst.UNC_FLAG, StringComparison.OrdinalIgnoreCase))
                    {
                        if (AveNetworkingUtil.IsLocalAddress(obj.Host))
                        {
                            var localPath = AveNetworkingUtil.GetNetShareLocalPath(obj.Host, obj.ShareName);
                            // \\?\GLOBALROOT\Device\在快照备份时，备份出来的local 文件还是以\\开头，这种情况下直接读取不了，所以采用UNC的方式读取.
                            if (!String.IsNullOrEmpty(localPath) && !localPath.StartsWith(FSSystemConst.UNC_FLAG, StringComparison.OrdinalIgnoreCase))
                            {
                                if (String.IsNullOrEmpty(obj.OtherPath))
                                {
                                    this.SystemLocation = localPath;
                                }
                                else
                                {
                                    if (localPath.EndsWith(FSSystemConst.SEPARATER, StringComparison.Ordinal))
                                    {
                                        this.SystemLocation = localPath + obj.OtherPath;
                                    }
                                    else
                                    {
                                        this.SystemLocation = localPath + FSSystemConst.SEPARATER + obj.OtherPath;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
                //ignore exception
                this.SystemLocation = tmp;
                //logger.Error(e.Message, e);
            }
        }

        public override StorageOpenValidResult Open()
        {
            if (this.SystemHealth != XSystemHealth.Unknown)
            {
                return new StorageOpenValidResult();
            }
            StorageOpenValidResult vaildResult = new StorageOpenValidResult();
            try
            {
                base.Open();
                XriObject["location"] = XriObject["location"].Replace('/', '\\').TrimEnd('\\');
                this.SystemLocation = XriObject["location"];
                HandleReadFailoverParams();
                this.oSystemLocation = this.SystemLocation;
                if (XriObject.Params.ContainsKey(XRIParameterKeys.FS_KEY_BufferSize))
                {
                    this.bufferSize = Int32.Parse(XriObject.Params[XRIParameterKeys.FS_KEY_BufferSize]);
                    if (bufferSize <= 0)
                    {
                        throw new Exception("Invalid buffer size:" + bufferSize);
                    }
                }
                if (XriObject.Params.ContainsKey(XRIParameterKeys.XRI_KEY_AUTH_METHOD))
                {
                    AuthMethod = (AuthMethod)Enum.Parse(typeof(AuthMethod), XriObject.Params[XRIParameterKeys.XRI_KEY_AUTH_METHOD], true);
                }
                if (XriObject.Params.ContainsKey(XRIParameterKeys.READONLY))
                {
                    ReadOnly = Boolean.Parse(XriObject.Params[XRIParameterKeys.READONLY]);
                }
                if (XriObject.Params.ContainsKey(XRIParameterKeys.USERNAME_KEY))
                {
                    String username = XriObject.Params[XRIParameterKeys.USERNAME_KEY];
                    if (!String.IsNullOrEmpty(username))
                    {
                        if (username.Contains("\\"))
                        {
                            this.SystemDomain = username.Substring(0, username.IndexOf(@"\", StringComparison.OrdinalIgnoreCase));
                            this.SystemUserName = username.Substring(username.IndexOf(@"\", StringComparison.OrdinalIgnoreCase) + 1);
                        }
                        else if (username.Contains("@"))
                        {
                            this.SystemUserName = username.Substring(0, username.IndexOf("@", StringComparison.OrdinalIgnoreCase));
                            this.SystemDomain = username.Substring(username.IndexOf("@", StringComparison.OrdinalIgnoreCase) + 1);
                        }
                        else
                        {
                            this.SystemDomain = ".";
                            this.SystemUserName = username;
                        }
                    }
                }
                if (XriObject.Params.ContainsKey(XRIParameterKeys.XRI_KEY_AUTH_METHOD))
                {
                    this.AuthMethod = (AuthMethod)Enum.Parse(typeof(AuthMethod), XriObject.Params[XRIParameterKeys.XRI_KEY_AUTH_METHOD], true);
                }
                if (XriObject.Params.ContainsKey(XRIParameterKeys.PASSWORD_KEY))
                {
                    String pass = XriObject.Params[XRIParameterKeys.PASSWORD_KEY];
                    if (!String.IsNullOrEmpty(pass))
                    {
                        this.SystemPassword = SecretUtil.DescryptPassword(pass);
                    }
                }
                this.Identity = new FSIdentity(this);
                GenerateOptimalSystemLocation();                
                if (XriObject.Params.ContainsKey(XRIParameterKeys.CREATE_IF_NOT_EXISTS))
                {
                    this.createIfNotExist = Boolean.Parse(XriObject.Params[XRIParameterKeys.CREATE_IF_NOT_EXISTS]);
                }
                if (XriObject.Params.ContainsKey(XRIParameterKeys.SECURELY_DELETE))
                {
                    this.securelyDelete = true;
                }
                this.SetFileOptions();
                this.IsDirectSystem = true;
                this.SystemHealth = XSystemHealth.AvailableAndNotFull;
                this.Type = "FSSystem";
                var openParam = AssembleFSClientOpenParam(true);
                this.alphaFSClient = new AlphaFSClient(openParam);
                openParam = AssembleFSClientOpenParam(false);
                this.fsClient = new FSClient(openParam);
                SetSystemDescription();
                this.SystemHealth = XSystemHealth.AvailableAndNotFull;
            }
            catch (Exception e)
            {
                logger.Error("Open netShare system failed. Error:{0}", e);
                this.SystemHealth = XSystemHealth.Unaccessable;
                vaildResult.Message = e.Message;
                throw;
            }
            return vaildResult;
        }

        private void SetFileOptions()
        {
            this.FileOptions = FileOptions.None;
            if (XriObject.Params.ContainsKey(XRIParameterKeys.FS_Key_FileOptions))
            {
                var fileOptionValue = XriObject.Params[XRIParameterKeys.FS_Key_FileOptions];
                if (fileOptionValue.Equals(StorageConstants.FILE_FLAG_NO_BUFFERING, StringComparison.OrdinalIgnoreCase))
                {
                    this.FileOptions = (FileOptions)0x20000000 | FileOptions.WriteThrough;
                }
                else
                {
                    this.FileOptions = (FileOptions)Enum.Parse(typeof(FileOptions), fileOptionValue);
                }
            }
        }

        private FSClientOpenParam AssembleFSClientOpenParam(bool isAlphaFS)
        {
            var param = new FSClientOpenParam();
            param.StorageIdentity = this.Identity;
            param.ModuleType = this.moduleType;
            if (!this.AuthMethod.Equals(AuthMethod.NetUse) && !this.AuthMethod.Equals(AuthMethod.NetUse_DeleteOld))
            {
                this.SystemLocation = this.SystemLocation.TrimEnd('\\') + '\\';
            }
            if (isAlphaFS)
            {
                if (oSystemLocation.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
                {
                    param.OriginalSystemLocation = this.oSystemLocation;
                    param.SystemLocation = "\\\\?\\UNC\\" + this.oSystemLocation.Substring(2);
                }
                else
                {
                    param.OriginalSystemLocation = this.oSystemLocation;
                    param.SystemLocation = "\\\\?\\" + this.oSystemLocation;
                }
            }
            else
            {
                param.OriginalSystemLocation = this.oSystemLocation;
                param.SystemLocation = this.SystemLocation;
            }
            param.SystemUserName = this.SystemUserName;
            param.SystemPassword = this.SystemPassword;
            param.SystemDomain = this.SystemDomain;
            param.StorageSystem = this;
            param.IsReadonly = this.ReadOnly;
            param.securelyDelete = this.securelyDelete;
            return param;
        }
        private void HandleReadFailoverParams()
        {
            try
            {
                String tempString = "docave-xam://" + SystemLocation;
                XRI tmpXRIObj = XRI.ValueOf(tempString);
                this.SystemLocation = tmpXRIObj.VIM;
                if (tmpXRIObj.Params.ContainsKey(XRIParameterKeys.XRI_KEY_AUTH_METHOD))
                {
                    String authMethodString = tmpXRIObj.Params[XRIParameterKeys.XRI_KEY_AUTH_METHOD];
                    if (!String.IsNullOrEmpty(authMethodString))
                    {
                        authMethodString = authMethodString.TrimEnd(new Char[] { '\\', '/' });
                        this.AuthMethod = (AuthMethod)Int32.Parse(authMethodString);
                    }
                }
                if (tmpXRIObj.Params.ContainsKey(XRIParameterKeys.XRI_KEY_ReadFailover))
                {
                    this.readFailover = Boolean.Parse(tmpXRIObj.Params[XRIParameterKeys.XRI_KEY_ReadFailover]);
                    //if (tmpXRIObj.Params.ContainsKey(XSystemConst.XRI_KEY_ReadFailover_DFSNAME))
                    //{
                    //    dfsName = tmpXRIObj.Params[XSystemConst.XRI_KEY_ReadFailover_DFSNAME];
                    //}
                    //if (tmpXRIObj.Params.ContainsKey(XSystemConst.XRI_KEY_ReadFailover_ENUMLEVEL))
                    //{
                    //    enumLevel = (DFSENUMLEVEL)(int.Parse(tmpXRIObj.Params[XSystemConst.XRI_KEY_ReadFailover_DFSNAME]));
                    //}
                    dfsName = SystemLocation;
                    if (tmpXRIObj.Params.ContainsKey(XRIParameterKeys.XRI_KEY_ReadFailover_Prefix))
                    {
                        readFailoverPrefix = tmpXRIObj.Params[XRIParameterKeys.XRI_KEY_ReadFailover_Prefix];
                    }
                    if (this.readFailover)
                    {
                        List<DFS_STORAGE_INFO> dfstargets = DFSUtility.EnumDFS(dfsName, enumLevel);
                        if (dfstargets != null && dfstargets.Count > 0)
                        {
                            readFailoverLocations = new List<String>();
                            dfstargets.ForEach((Action<DFS_STORAGE_INFO>)((item) =>
                            {
                                readFailoverLocations.Add("\\\\" + item.ServerName + "\\" + item.ShareName + "\\" + readFailoverPrefix);
                            }));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.Message);
            }
        }

        protected override void SetSystemDescription()
        {
            this.Properties[SystemPropertyKeys.SystemDescriptionKey] = this.SystemLocation;
            List<String> keys = new List<String>();
            keys.Add(this.SystemLocation.ToLower(CultureInfo.InvariantCulture));
            List<String> securityKeys = new List<String>();
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            CheckState(fileMode);
            var client = GetProperClient(info, true);
            info.BufferSize = this.bufferSize;
            //Business layer can override the file options 
            if (info.FileOptions == FileOptions.None)
            {
                info.FileOptions = this.FileOptions;
            }
            if (fileMode != FileMode.Open)
            {
                this.Written = true;
            }
            return client.OpenStream(info, fileMode);
        }

        public override bool DirectoryExists(StorageInfo info)
        {
            CheckState();
            var client = GetProperClient(info, false);
            return client.DirectoryExists(info);
        }
        public override bool FileExists(StorageInfo info)
        {
            CheckState();
            var client = GetProperClient(info, true);
            return client.FileExists(info);
        }
        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            CheckState();
            var client = GetProperClient(info, false, info.IsUseAlpha);
            StorageDeleteResult rs = new StorageDeleteResult();

            //try
            //{
            //    rs = client.DeleteDirectory(info);
            //}
            //catch (IOException e)
            //{
            //    this.logger.Warn("Delete the directory [{0}] failed, maybe the path is too long, try to delete with alphaFS. Error: {1}", info.HighPlusLowName, e);
            //    rs = alphaFSClient.DeleteDirectory(info);
            //}
            rs = client.DeleteDirectory(info);
            if (rs.DeleteExceptionType == DeleteExceptionType.IOException)
            {
                var deletedSize = rs.DeletedFileSize;
                this.logger.Warn("Delete the directory [{0}] failed, maybe the path is too long, try to delete with alphaFS.", info.HighPlusLowName);
                rs = alphaFSClient.DeleteDirectory(info);
                rs.DeletedFileSize += deletedSize;
            }
            if (rs.IsDeleted == true)
            {
                this.Deletion = true;
            }
            return rs;
        }
        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            CheckState();
            var client = GetProperClient(info, true);
            StorageDeleteResult rs = client.DeleteFile(info);
            if (rs.IsDeleted == true)
            {
                this.Deletion = true;
            }
            return rs;
        }
        public override void Close()
        {
        }
        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            CheckState();
            var client = GetProperClient(dirInfo, false);
            return client.OpenDirectory(dirInfo, mode);
        }
        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            CheckState();
            var client = GetProperClient(fileInfo, true);
            return client.OpenFile(fileInfo);
        }
        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            CheckState();
            var client = GetProperClient(dirInfo, false);
            try
            {
                return client.ListDirectories(dirInfo);
            }
            catch (PathTooLongException ex)
            {
                logger.Warn("Some file under dir {0} path too long. Error:{1}", dirInfo.HighPlusLowName, ex);
                client = alphaFSClient;
                return client.ListDirectories(dirInfo);
            }
        }
        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {

            CheckState();
            var client = GetProperClient(dirInfo, false);
            try
            {
                return client.ListFiles(dirInfo);
            }
            catch (PathTooLongException ex)
            {
                logger.Warn("Some file under dir {0} path too long. Error:{1}", dirInfo.HighPlusLowName, ex);
                client = alphaFSClient;
                return client.ListFiles(dirInfo);
            }
        }
        public override IEnumerable<List<XFileInfo>> GetFilesInBatch(StorageInfo dirInfo, int batchSize)
        {

            CheckState();
            var client = GetProperClient(dirInfo, false);
            try
            {
                return client.ListFilesInBatches(dirInfo, batchSize);
            }
            catch (PathTooLongException ex)
            {
                logger.Warn("Some file under dir {0} path too long. Error:{1}", dirInfo.HighPlusLowName, ex);
                client = alphaFSClient;
                return client.ListFilesInBatches(dirInfo, batchSize);
            }
        }

        public override IEnumerable<List<XDirectoryInfo>> GetDirectoriesInBatch(StorageInfo dirInfo, int batchSize)
        {
            CheckState();
            var client = GetProperClient(dirInfo, false);
            try
            {
                return client.ListDirectoriesInBatches(dirInfo, batchSize);
            }
            catch (PathTooLongException ex)
            {
                logger.Warn("Some file under dir {0} path too long. Error:{1}", dirInfo.HighPlusLowName, ex);
                client = alphaFSClient;
                return client.ListDirectoriesInBatches(dirInfo, batchSize);
            }
        }
        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            CheckState();
            var result = new StorageListResult();
            var client = GetProperClient(dirInfo, false);
            try
            {
                result.Files = client.ListFiles(dirInfo);
                result.SubDirs = client.ListDirectories(dirInfo);
            }
            catch (PathTooLongException ex)
            {
                logger.Warn("Some file under dir {0} path too long. Error:{1}", dirInfo.HighPlusLowName, ex);
                client = alphaFSClient;
                result.Files = client.ListFiles(dirInfo);
                result.SubDirs = client.ListDirectories(dirInfo);
            }
            return result;
        }
        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            CheckState();
            var clientSource = GetProperClient(sourceFileInfo, true);
            var clientTarget = GetProperClient(targetFileInfo, true);
            var client = ((clientSource is AlphaFSClient) || (clientTarget is AlphaFSClient)) ? alphaFSClient : fsClient;
            var result = client.MoveFile(sourceFileInfo, targetFileInfo, isOverWrite);
            if (!result.IsMoved && !string.IsNullOrEmpty(result.Message) && result.Message.Contains("The specified path, file name, or both are too long."))
            {
                logger.Warn("The file path is too long, need retry with alphaFS.");
                result = alphaFSClient.MoveFile(sourceFileInfo, targetFileInfo, isOverWrite);
            }
            return result;
        }
        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        {
            CheckState();
            var clientSource = GetProperClient(sourceDirInfo, false);
            var clientTarget = GetProperClient(targetDirInfo, false);
            var client = ((clientSource is AlphaFSClient) || (clientTarget is AlphaFSClient)) ? alphaFSClient : fsClient;
            return client.MoveDirectory(sourceDirInfo, targetDirInfo, isOverWrite);
        }
        public override StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile)
        {
            if (srcFile.IsSameDrive)
            {
                var clientSource = GetProperClient(srcFile, false);
                var clientTarget = GetProperClient(destFile, false);
                var client = ((clientSource is AlphaFSClient) || (clientTarget is AlphaFSClient)) ? alphaFSClient : fsClient;
                return client.MoveFile(srcFile, destSystem, destFile, true);
            }
            return base.MoveFile(srcFile, destSystem, destFile);
        }
        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            CheckState();
            var clientSource = GetProperClient(sourceFileInfo, true);
            var clientTarget = GetProperClient(targetFileInfo, true);
            var client = ((clientSource is AlphaFSClient) || (clientTarget is AlphaFSClient)) ? alphaFSClient : fsClient;
            return client.CopyFile(sourceFileInfo, targetFileInfo, isOverWrite);
        }
        public void HandleValidateResultByErrorCode(int errCode, StorageOpenValidResult rs)
        {
            //ERROR_FILE_NOT_FOUND              2       The system cannot find the file specified.
            //ERROR_PATH_NOT_FOUND              3       The system cannot find the path specified.
            //ERROR_PATH_NOT_FOUND              123     The system cannot find the path specified.
            //ERROR_PATH_NOT_FOUND              1008    The system cannot find the path specified.
            //ERROR_ACCESS_DENIED               5       Access is denied
            //ERROR_BAD_NETPATH                 53      The network path was not found.
            //ERROR_BAD_NET_NAME                67      The network name cannot be found.  
            //ERROR_INVALID_PASSWORD            86      The specified network password is not correct.
            //ERROR_INVALID_SERVICE_ACCOUNT     1057    The account name is invalid or does not exist, or the password is invalid for the account name specified
            //ERROR_INVALID_PASSWORDNAME        1216    The format of the specified password is invalid.
            //ERROR_LOGON_TYPE_NOT_GRANTED      1385    Logon failure: the user has not been granted the requested logon type at this computer. 
            //ERROR_LOGON_FAILURE               1326    Logon failure: unknown user name or bad password.
            //ERROR_ACCOUNT_RESTRICTION         1327    Logon failure: user account restriction. Possible reasons are blank passwords not allowed, logon hour restrictions, or a policy restriction has been enforced.
            //ERROR_PASSWORD_EXPIRED            1330    Logon failure: the specified account password has expired.
            if (errCode == 2 || errCode == 3 || errCode == 53 || errCode == 123 || errCode == 67 || errCode == 1008)
            {
                this.SystemHealth = XSystemHealth.ConnectedFailed;
                rs.Message = FSI18N.ResourceManager.GetString("MediaStorage_FS_The_UNC_Path_cannot_be_found", AbstractXSystem.Culture);
                var verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage(this.SystemLocation, ContextValues.Storage.StorageType.FileSystem, new AvePoint.GCommon.Utility.Exceptions.Storage.PathNotFoundException());
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.NetShare, verifyFailedEventMessage);
            }
            else if (errCode == 86 || errCode == 1057 || errCode == 1216 || errCode == 1326 || errCode == 1327 || errCode == 1330 || errCode == 1385)
            {
                this.SystemHealth = XSystemHealth.AuthenticationFailed;
                rs.Message = FSI18N.ResourceManager.GetString("MediaStorage_FS_Authentication_failed", AbstractXSystem.Culture);
                var verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage(this.SystemLocation, ContextValues.Storage.StorageType.FileSystem, new AvePoint.GCommon.Utility.Exceptions.Storage.IncorrectUserNameOrPasswordException(XriObject[XRIParameterKeys.USERNAME_KEY]));
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.NetShare, verifyFailedEventMessage);
            }
            else if (errCode == 5)
            {
                this.SystemHealth = XSystemHealth.AuthenticationFailed;
                rs.Message = string.Format(FSI18N.ResourceManager.GetString("MediaStorage_FS_AccessDenied", AbstractXSystem.Culture), this.SystemLocation);
                var verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage(this.SystemLocation, ContextValues.Storage.StorageType.FileSystem, new AvePoint.GCommon.Utility.Exceptions.Storage.UnauthorizedAccessException(XriObject[XRIParameterKeys.USERNAME_KEY]));
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.NetShare, verifyFailedEventMessage);
            }
            else if (errCode == 112)
            {
                this.SystemHealth = XSystemHealth.Available;
            }
            else if (errCode == 1909)
            {
                this.SystemHealth = XSystemHealth.Unaccessable;
                rs.Message = FSI18N.ResourceManager.GetString("MediaStorage_FS_UserLocked", AbstractXSystem.Culture);
                var verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage(this.SystemLocation, ContextValues.Storage.StorageType.FileSystem, new AvePoint.GCommon.Utility.Exceptions.Storage.UnauthorizedAccessException(XriObject[XRIParameterKeys.USERNAME_KEY]));
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.NetShare, verifyFailedEventMessage);
            }
            else
            {
                this.SystemHealth = XSystemHealth.Unaccessable;
                rs.Message = FSI18N.ResourceManager.GetString("MediaStorage_FS_Test_failed", AbstractXSystem.Culture);
                var verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage(this.SystemLocation, ContextValues.Storage.StorageType.FileSystem, new Exception(rs.Message));
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.NetShare, verifyFailedEventMessage);
            }
        }
        public override StorageOpenValidResult Validate()
        {
            CheckState();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            var rs = new StorageOpenValidResult();
            String tempFileForValidate = System.Guid.NewGuid().ToString() + "." + System.DateTime.Now.Ticks + "_DocAve.tmp";
            String fileName = this.SystemLocation.TrimEnd('\\') + "\\" + tempFileForValidate;
            Int32 errCode = 0;
            try
            {
                try
                {
                    using (Identity.Impersonate())
                    {
                        var rootFolderPath = this.SystemLocation.EndsWith("\\", StringComparison.OrdinalIgnoreCase) ? this.SystemLocation : this.SystemLocation + "\\";
                        if (this.ReadOnly && Directory.Exists(rootFolderPath))
                        {
                            rs.IsReadAble = true;
                            rs.IsWriteAble = false;
                            rs.IsDeleteAble = false;
                        }
                        else if (!this.ReadOnly && (Directory.Exists(rootFolderPath) || this.CreateIfNotExists))
                        {
                            if (!Directory.Exists(rootFolderPath) && CreateIfNotExists)
                            {
                                Directory.CreateDirectory(rootFolderPath);
                            }
                            rs.IsReadAble = true;
                            if (fileName.Length < 260)
                            {
                                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                                {
                                    fs.WriteByte(0x00);
                                    rs.IsWriteAble = true;
                                }
                            }
                            else
                            {
                                String tempSystemLocation;
                                if (SystemLocation.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
                                {
                                    tempSystemLocation = "\\\\?\\UNC\\" + this.SystemLocation.Substring(2);
                                }
                                else
                                {
                                    tempSystemLocation = "\\\\?\\" + this.SystemLocation;
                                }
                                fileName = tempSystemLocation.TrimEnd('\\') + "\\" + tempFileForValidate;
                                var aFileInfo = new Alphaleonis.Win32.Filesystem.FileInfo(fileName);
                                using (FileStream fs = aFileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                                {
                                    fs.WriteByte(0x00);
                                    rs.IsWriteAble = true;
                                }
                            }
                        }
                        else
                        {
                            errCode = FSUtil.GetLastError();
                            logger.Error("Validate Physical Device Failed, Error Code : {0}", errCode);
                            HandleValidateResultByErrorCode(errCode, rs);
                        }
                        if (this.SystemHealth >= XSystemHealth.Available)
                        {
                            if (!IsFull)
                            {
                                this.SystemHealth = XSystemHealth.AvailableAndNotFull;
                            }
                            else
                            {
                                this.SystemHealth = XSystemHealth.Available;
                            }
                        }
                        rs.TotalSpace = this.innerTotalSpace;
                        rs.TotalFreeSpace = this.innerTotalFreeSpace;
                        rs.TotalUsedSpace = this.innerTotalSpace - this.innerTotalFreeSpace;
                    }
                }
                catch (Exception e)
                {
                    errCode = FSUtil.GetLastError();
                    logger.Error("Validate physical device failed, error code {0}. Error:{1}", errCode, e);
                    HandleValidateResultByErrorCode(errCode, rs);
                }
                finally
                {
                    using (Identity.Impersonate())
                    {
                        try
                        {
                            if (fileName.Length < 260)
                            {
                                if (File.Exists(fileName))
                                {
                                    File.Delete(fileName);
                                    rs.IsDeleteAble = true;
                                }
                            }
                            else
                            {
                                if (Alphaleonis.Win32.Filesystem.File.Exists(fileName))
                                {
                                    Alphaleonis.Win32.Filesystem.File.Delete(fileName);
                                    rs.IsDeleteAble = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            rs.IsDeleteAble = false;
                            rs.Message = FSI18N.ResourceManager.GetString("MediaStorage_FS_Does_not_have_delete_permission", AbstractXSystem.Culture);
                            logger.Warn("The user {0} doesn't has delete permission for device {1}.Error:{2}", this.SystemUserName, this.oSystemLocation, ex);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errCode = FSUtil.GetLastError();
                logger.Error("Validate physical device failed, error code {0}. Error:{1}", errCode, e);
                HandleValidateResultByErrorCode(errCode, rs);
            }
            rs.SystemHealth = this.SystemHealth;
            return rs;
        }
        public string GetFormatLocation()
        {
            if (this.SystemLocation.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
            {
                var obj = UNCObject.ValueOf(this.SystemLocation);
                this.SystemLocation = obj.ToLocation();
                using (Identity.Impersonate())
                {
                    var localPath = AveNetworkingUtil.GetNetShareLocalPath(obj.Host, obj.ShareName);
                    var hostinf = Dns.GetHostEntry(obj.Host);
                    return PathUtil.CombinePath(hostinf.HostName, localPath.Substring(0, 1));
                }
            }
            else
            {
                return this.SystemLocation.Substring(0, 1);
            }
        }
        public IFSClient GetProperClient(StorageInfo info, bool isFile, bool usealphaFS = false)
        {
            var dirPath = String.Empty;
            var filePath = String.Empty;
            if (SystemLocation.StartsWith("\\\\?\\UNC\\", StringComparison.OrdinalIgnoreCase))
            {
                return this.alphaFSClient;
            }
            if (isFile)
            {
                dirPath = PathUtil.CombinePath(SystemLocation, info.HighName);
                filePath = PathUtil.CombinePath(dirPath, info.LowName);
                return (dirPath.Length >= 248 || filePath.Length >= 260) ? this.alphaFSClient : this.fsClient;
            }
            if (usealphaFS)
            {
                return this.alphaFSClient;
            }
            else
            {
                dirPath = PathUtil.CombinePath(SystemLocation, info.HighPlusLowName);
                return (dirPath.Length >= 248) ? this.alphaFSClient : this.fsClient;
            }
        }

        public override Boolean ConvertLongPathToSymlink(String symlinkPath, String targetPath)
        {
            return this.fsClient.ConvertLongPathToSymlink(symlinkPath, targetPath);
        }

        public override XPerformanceResult GetDevicePerformance(IOType type, int writeRatio = 0, string blokeSize = "64k")
        {
            if (writeRatio < 0 || writeRatio > 100)
            {
                throw new Exception("The write radio exceeds the limit(0 - 100), the value is " + writeRatio);
            }
            bool isAuthMethodChanged = false;
            var tempFile = new StorageInfo() { LowName = Guid.NewGuid().ToString(), Length = 50 * 1024 * 1024 };
            try
            {
                using (var stream = this.OpenStream(tempFile, FileMode.Create))
                {
                    var b = new Byte[tempFile.Length];
                    stream.Write(b, 0, b.Length);
                }
                var tempFilePath = PathUtil.CombinePath(SystemLocation, tempFile.HighPlusLowName);
                if (this.AuthMethod == AuthMethod.LogonUser && tempFilePath.StartsWith("\\\\"))
                {
                    this.SystemLocation = this.SystemLocation.TrimEnd('\\');
                    this.AuthMethod = AuthMethod.NetUse;
                    isAuthMethodChanged = true;
                }
                var result = this.fsClient.GetNetshareSpeed(type, writeRatio, blokeSize, tempFilePath);

                var IopsResult = new XPerformanceResult();
                IopsResult.Throughput = result.TimeSpan.Thread.Target.BytesCount / result.TimeSpan.Thread.Target.Iops.Bucket.Count;
                IopsResult.ReadBytes = result.TimeSpan.Thread.Target.ReadBytes;
                IopsResult.WriteBytes = result.TimeSpan.Thread.Target.WriteBytes;
                IopsResult.IOCount = result.TimeSpan.Thread.Target.IOCount;
                IopsResult.ReadIOCount = result.TimeSpan.Thread.Target.ReadCount;
                IopsResult.WriteIOCount = result.TimeSpan.Thread.Target.WriteCount;
                IopsResult.ReadIopsStdDev = result.TimeSpan.Thread.Target.Iops.ReadIopsStdDev;
                IopsResult.WriteIopsStdDev = result.TimeSpan.Thread.Target.Iops.WriteIopsStdDev;
                IopsResult.IopsStdDev = result.TimeSpan.Thread.Target.Iops.IopsStdDev;

                double total = 0;
                double read = 0;
                double write = 0;
                for (int i = 0; i < result.TimeSpan.Thread.Target.Iops.Bucket.Count; i++)
                {
                    total += result.TimeSpan.Thread.Target.Iops.Bucket[i].Total;
                    read += result.TimeSpan.Thread.Target.Iops.Bucket[i].Read;
                    write += result.TimeSpan.Thread.Target.Iops.Bucket[i].Write;
                }
                IopsResult.Iops = Math.Round(total / result.TimeSpan.Thread.Target.Iops.Bucket.Count, 2);
                IopsResult.ReadIops = Math.Round(read / result.TimeSpan.Thread.Target.Iops.Bucket.Count, 2);
                IopsResult.WriteIops = Math.Round(write / result.TimeSpan.Thread.Target.Iops.Bucket.Count, 2);
                return IopsResult;
            }
            catch(Exception e)
            {
                logger.Error("Get device speed information failed, {0}", e);
                throw;
            }
            finally
            {
                if (isAuthMethodChanged)
                {
                    this.SystemLocation = this.SystemLocation.TrimEnd('\\') + '\\';
                    this.AuthMethod = AuthMethod.LogonUser;
                }
                this.DeleteFile(tempFile);
            }
        }

        #endregion

        /// <summary>
        /// 选择normal client或者是long client
        /// </summary>
        /// <param name="info"></param>
        /// <param name="isFile"></param>
        /// <returns></returns>

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }
    }
}