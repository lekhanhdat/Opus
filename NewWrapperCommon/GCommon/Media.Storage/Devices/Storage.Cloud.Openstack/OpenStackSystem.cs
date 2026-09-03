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
using AvePoint.Media.Storage.Resources.OpenStackI18N;
using AvePoint.Media.Storage.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    internal class OpenStackSystem : AbstractXSystem
    {
        StorageLogger logger = StorageLogger.GetInstance(typeof(OpenStackSystem));
        OpenStackOpenParameter openParameter;
        OpenStackBaseOperationClient operationClient;

        public override StorageInterfaceType StorageInterfaceType
        {
            get
            {
                return StorageInterfaceType.Namespace;
            }
        }
        public override String Type
        {
            get
            {
                return "OpenStackSystem";
            }
        }

        static OpenStackSystem()
        {
            ServicePointManager.DefaultConnectionLimit = 1024;//TODO
        }

        public OpenStackSystem(String xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            this.SystemHealth = XSystemHealth.Unknown;
            this.IsSupportAutoChangeDataBlock = false;
            this.SpaceThresholdUnit = SpaceThresholdUnit.MB;
            this.openParameter = new OpenStackOpenParameter();
            this.Open();
        }

        public override StorageOpenValidResult Open()
        {
            //logger.Info(XriString);
            if (this.SystemHealth != XSystemHealth.Unknown)
            {
                return new StorageOpenValidResult();
            } 
            base.Open();
            if (XriObject.Params.ContainsKey(OpenStackConstants.TENANTNAME_KEY))
            {
                openParameter.TenantName = XriObject.Params[OpenStackConstants.TENANTNAME_KEY].Trim();
            }
            if (XriObject.Params.ContainsKey(OpenStackConstants.TENANTID_KEY))
            {
                openParameter.TenantId = XriObject.Params[OpenStackConstants.TENANTID_KEY].Trim();
            }
            if (XriObject.Params.ContainsKey(OpenStackConstants.USERNAME_KEY))
            {
                openParameter.UserName = XriObject.Params[OpenStackConstants.USERNAME_KEY].Trim();
            }
            if (XriObject.Params.ContainsKey(OpenStackConstants.PASSWORD_KEY))
            {
                openParameter.Password = SecretUtil.DescryptPassword(XriObject.Params[OpenStackConstants.PASSWORD_KEY]);
            }
            if (XriObject.Params.ContainsKey(OpenStackConstants.AUTHENTICATION_URL_KEY))
            {
                openParameter.AuthenticationURL = XriObject.Params[OpenStackConstants.AUTHENTICATION_URL_KEY].Trim();
            }
            this.openParameter.AuthenticationType = this.XriObject.Params.ContainsKey(OpenStackConstants.AUTHENTICATION_TYPE_KEY) ? this.XriObject.Params[OpenStackConstants.AUTHENTICATION_TYPE_KEY].Trim() : "keystone";
            this.openParameter.AuthenticationVersion = this.XriObject.Params.ContainsKey(OpenStackConstants.AUTHENTICATION_VERSION_KEY) ? Int32.Parse(this.XriObject.Params[OpenStackConstants.AUTHENTICATION_VERSION_KEY].Trim()) : 2;
            if (XriObject.Params.ContainsKey(OpenStackConstants.CREATE_IF_NOT_EXISTS))
            {
                openParameter.CreateIfNotExists = bool.Parse(XriObject.Params[OpenStackConstants.CREATE_IF_NOT_EXISTS]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CDN_KEY))
            {
                if (string.Compare(XriObject.Params[XRIParameterKeys.CDN_KEY].ToLower(CultureInfo.InvariantCulture), "true", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    openParameter.CdnEnabled = true;
                }
            }
            this.openParameter.EnableSLO = this.XriObject.Params.ContainsKey(OpenStackConstants.ENABLESLO_KEY) && bool.Parse(this.XriObject.Params[OpenStackConstants.ENABLESLO_KEY]);
            this.openParameter.EnableBulkDelete = this.XriObject.Params.ContainsKey(OpenStackConstants.EnableBulkDelete_KEY) && bool.Parse(this.XriObject.Params[OpenStackConstants.EnableBulkDelete_KEY]);
            if (XriObject.Params.ContainsKey(OpenStackConstants.SingleUploadMaxSize_KEY))
            {
                openParameter.SingleUploadMaxSize = Int64.Parse(XriObject.Params[OpenStackConstants.SingleUploadMaxSize_KEY]);
            }
            else
            {
                openParameter.SingleUploadMaxSize = 64 * 1024 * 1024L; ;
            }
            if (XriObject.Params.ContainsKey(OpenStackConstants.SegmentMinSize_KEY))
            {
                openParameter.SegmentMinSize = long.Parse(XriObject.Params[OpenStackConstants.SegmentMinSize_KEY]);
            }
            else
            {
                openParameter.SegmentMinSize = 16 * 1024 * 1024L; ;
            }
            if (XriObject.Params.ContainsKey(OpenStackConstants.MaxFileSize_KEY))
            {
                openParameter.MaxFileSize = Int64.Parse(XriObject.Params[OpenStackConstants.MaxFileSize_KEY]);
            }
            else
            {
                openParameter.MaxFileSize = 5 * 1024 * 1024 * 1024L;
            }

            this.IsRetry = !this.XriObject.Params.ContainsKey(XRIParameterKeys.IS_RETRY) || bool.Parse(this.XriObject.Params[XRIParameterKeys.IS_RETRY]);
            openParameter.UploadCheckMD5 = true;
            openParameter.RetryInterval = this.RetryInterval;
            openParameter.MaxRetryCount = this.MaxRetryCount;
            openParameter.NeedRetry = this.IsRetry;
            this.SystemLocation = this.XriObject[OpenStackConstants.SystemLocationKeyName];
            if (string.IsNullOrEmpty(SystemLocation))
            {
                SystemLocation = "DocAve";
            }
            openParameter.SystemLocation = SystemLocation;
            this.SystemHealth = XSystemHealth.AvailableAndNotFull;
            SetSystemDescription();
            operationClient = new OpenStackBaseOperationClient(openParameter);
            return new StorageOpenValidResult();
        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "OpenStack Swift Object Storage Server";
            List<string> keys = new List<string>();
            keys.Add(this.openParameter.SystemLocation);
            List<string> securityKeys = new List<string>();
            keys.Add(this.openParameter.UserName);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        public override StorageOpenValidResult Validate()
        {
            CheckState();
            var openValidResult = new StorageOpenValidResult();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            try
            {
                openValidResult = operationClient.Authentication();

                if (!operationClient.ContainerExists(this.SystemLocation))
                {
                    if (CreateIfNotExists)
                    {
                        operationClient.CreateContainer(this.SystemLocation);
                    }
                    else
                    {
                        logger.Info("the root folder don't exist:" + SystemLocation);
                        openValidResult.SystemHealth = XSystemHealth.Unaccessable;
                        openValidResult.IsDeleteAble = false;
                        openValidResult.IsWriteAble = false;
                        openValidResult.IsReadAble = false;
                        return openValidResult;
                    }
                }

                SpaceInfo spaceInfo = CacheUtil.GetSpaceInfo(XriObject.VIM, this.XriString, new CheckFreeSpace(this.CheckFreeSpace));
                openValidResult.SystemHealth = XSystemHealth.AvailableAndNotFull;
                openValidResult.TotalUsedSpace = spaceInfo.TotalUsedSpace;
                openValidResult.TotalSpace = spaceInfo.TotalSpace;
                openValidResult.TotalFreeSpace = spaceInfo.TotalSpace - spaceInfo.TotalUsedSpace;
                openValidResult.IsDeleteAble = true;
                openValidResult.IsReadAble = true;
                openValidResult.IsWriteAble = true;
                innerTotalFreeSpace = openValidResult.TotalFreeSpace;
                if (ValidateIsFull())
                {
                    openValidResult.SystemHealth = XSystemHealth.Available;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred when validate cloud system. {0}", ex);

                if (ex is AuthenticationFailedException)
                {
                    openValidResult.Message = OpenStackI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Authentication_failed", AbstractXSystem.Culture);
                }
                else
                {
                    openValidResult.Message = OpenStackI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Test_failed", AbstractXSystem.Culture);
                    //TODO
                    //EventIds.Storage.VerifyFailedEventMessage verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage(this.SystemLocation, ContextValues.Storage.StorageType.OpenStack, ex);
                    //this.logger.Log(EventSources.DocAveStorageAPIService, 111, verifyFailedEventMessage);
                }
                openValidResult.SystemHealth = XSystemHealth.AuthenticationFailed;
            }
            finally
            {
                this.SystemHealth = openValidResult.SystemHealth;
            }
            return openValidResult;
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            CheckState();
            var infoTemp = PreproccessStorageInfo(info);
            var cloudStream = operationClient.OpenStream(infoTemp, fileMode);
            cloudStream.Info = info;
            //TODO for upload  why?
            cloudStream.System = this;
            return cloudStream;
        }

        public override Boolean DirectoryExists(StorageInfo info)
        {
            CheckState();
            var dirInfoTemp = Preproccess2DirectoryStorageInfo(info);
            return operationClient.DirectoryExists(dirInfoTemp);
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            CheckState();
            var dirInfoTemp = Preproccess2DirectoryStorageInfo(dirInfo);
            var dir = operationClient.OpenDirectory(dirInfoTemp, mode) as OpenStackDirectoryInfo;
            return dir;
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            CheckState();
            var infoTemp = PreproccessStorageInfo(info);
            StorageDeleteResult rs;
            try
            {
                rs = operationClient.DeleteDirectory(infoTemp);
            }
            catch (Exception e)
            {
                logger.Error("error when delete directory, directory name : {0} details: {1}", info.HighName, e);
                throw;
            }
            return rs;
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            CheckState();
            var sInfo = Preproccess2DirectoryStorageInfo(dirInfo);
            var results = operationClient.ListSubDirectoriesAndFiles(sInfo);
            return results;
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            CheckState();
            var sInfo = Preproccess2DirectoryStorageInfo(dirInfo);
            var results = operationClient.ListDirectories(sInfo);
            return results;
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            CheckState();
            var sInfo = Preproccess2DirectoryStorageInfo(dirInfo);
            var results = operationClient.ListFiles(sInfo);
            return results;
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            CheckState();
            var srcStorageInfo = PreproccessStorageInfo(sourceFileInfo);
            var destStorageInfo = PreproccessStorageInfo(targetFileInfo);
            var rs = new StorageCopyResult();
            try
            {
                rs = operationClient.CopyFile(srcStorageInfo, destStorageInfo, isOverWrite);
            }
            catch (Exception e)
            {
                rs.Message = e.ToString();
                rs.IsCopyed = false;
                logger.Error("copy file failed: {0}", e);
            }
            return rs;
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            CheckState();
            var rs = new StorageDeleteResult();
            var infoTemp = PreproccessStorageInfo(info);
            try
            {
                rs = operationClient.DeleteFile(infoTemp);
            }
            catch (Exception e)
            {
                logger.Error("error when delete object, container name : {0}, object name : {1}. details : {2}", infoTemp.HighName, infoTemp.LowName, e);
                throw;
            }
            //标记执行过删除
            Deletion = true;
            return rs;
        }

        public override Boolean FileExists(StorageInfo info)
        {
            CheckState();
            var storageInfo = PreproccessStorageInfo(info);
            Boolean rs;
            try
            {
                rs = operationClient.FileExists(storageInfo);
            }
            catch (Exception e)
            {
                logger.Error("error when check object : {0}, object name : {1}, details: {2}", info.HighName, info.LowName, e);
                throw;
            }
            return rs;
        }

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, Boolean isOverWrite)
        {
            CheckState();
            var sourceInfo = Preproccess2DirectoryStorageInfo(sourceDirInfo);
            var targetInfo = Preproccess2DirectoryStorageInfo(targetDirInfo);
            //StorageMoveResult moveResult = null;  //TODO 赋值为Null的话move过程中抛异常会在catch异常后再次抛空引用异常
            var moveResult = new StorageMoveResult();
            try
            {
                moveResult = operationClient.MoveDirectory(sourceInfo, targetInfo, isOverWrite);
            }
            catch (Exception ex)
            {
                moveResult.IsMoved = false;
                moveResult.Message = ex.Message;
            }
            return moveResult;
        }

        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            CheckState();
            var sourceInfo = PreproccessStorageInfo(sourceFileInfo);
            var targetInfo = PreproccessStorageInfo(targetFileInfo);
            var moveResult = operationClient.MoveFile(sourceInfo, targetInfo, isOverWrite);
            return moveResult;
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            CheckState();
            var tempInfo = PreproccessStorageInfo(fileInfo);
            OpenStackFileInfo openStackFileInfo;
            try
            {
                openStackFileInfo = (OpenStackFileInfo)operationClient.OpenFile(tempInfo);
                if (openStackFileInfo != null)
                {
                    openStackFileInfo.HighName = fileInfo.HighName;
                    openStackFileInfo.LowName = fileInfo.LowName;
                }
            }
            catch (Exception e)
            {
                logger.Error("error when check object container name : {0}, object name : {1}, details: {2}", fileInfo.HighName, fileInfo.LowName, e);
                throw;
            }
            return openStackFileInfo;
        }

        public SpaceInfo CheckFreeSpace()
        {
            return null;
        }

        public StorageInfo PreproccessStorageInfo(StorageInfo storageInfo)
        {
            StorageInfo info = storageInfo.Clone();
            if (String.IsNullOrEmpty(info.LowName))
            {
                info.LowName = String.Empty;
            }
            if (!String.IsNullOrEmpty(SystemLocation))
            {
                info.LowName = PathUtil.CombinePath(info.HighName, info.LowName);
                info.HighName = SystemLocation;
            }
            if (info.LowName.Equals("\\"))
            {
                info.LowName = "";
            }
            info.LowName = info.LowName.Replace('\\', '/').TrimStart('/');
            return info;
        }

        public StorageInfo Preproccess2DirectoryStorageInfo(StorageInfo storageInfo)
        {
            StorageInfo info = storageInfo.Clone();
            if (String.IsNullOrEmpty(info.LowName))
            {
                info.LowName = "\\";
            }
            if (!String.IsNullOrEmpty(SystemLocation))
            {
                info.LowName = PathUtil.CombinePath(info.HighName, info.LowName);
                info.HighName = SystemLocation;
            }
            if (info.LowName.Equals("\\"))
            {
                info.LowName = "";
            }
            //info.LowName = info.LowName.Replace('\\', '/').TrimEnd('/').TrimStart('/') + "/"; //TODO 直接trim
            info.LowName = info.LowName.Replace('\\', '/').Trim('/') + "/";
            return info;
        }

        public override void Close()
        {
            logger.Info("OpenStackSystem Close.");//TODO
        } //TODO close方法什么都没做
    }
}
