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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Inner;
    using AvePoint.Media.Storage.Resources.CAStorI18N;
    using AvePoint.Media.Storage.Util;
    using Scsp;
    #endregion

    #region CodeReview
    [AveCodeReview(
   "2012/6/21",
   "rongbiao.sun@avepoint.com",
   "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
    null,
    true)]
    [AveCodeReview(
    "2012/3/22",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_HC_2 },
    "ADO-28237",
    true)]
    #endregion
    class CAStorSystem : AbstractXSystem
    {
        AveLogger logger = new AveLogger(typeof(CAStorSystem));
        CAStorOpenParameter openParam;
        public CAStorClient client;
        StorageInfo lastStreamInfo;

        public string LastMetaId { get; set; }

        public override StorageInterfaceType StorageInterfaceType
        {
            get
            {
                return StorageInterfaceType.Object;
            }
        }

        //public override bool IsFull
        //{
        //    get
        //    {
        //        SpaceInfo spaceInfo = CacheUtil.GetSpaceInfo(VIMName.CAStor, client.openParam.PrimaryNodes, client.CheckFreeSpace);
        //        TotalFreeSpace = spaceInfo.TotalFreeSpace;
        //        TotalSpace = spaceInfo.TotalSpace;
        //        TotalUsedSpace = spaceInfo.TotalUsedSpace;
        //        return ValidateIsFull();
        //    }
        //}

        public CAStorSystem(string xristring, AbstractXSystem parentSystem)
            : base(xristring, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_File;
            this.SystemHealth = XSystemHealth.Unknown;
            this.IsSupportAutoChangeDataBlock = true;
            //this.IsSupportAutoDeletion = true;
            this.SpaceThresholdUnit = SpaceThresholdUnit.MB;
            this.Open();
        }

        public override StorageOpenValidResult Open()
        {
            if (this.SystemHealth != XSystemHealth.Unknown)
            {
                return new StorageOpenValidResult();
            }
            this.openParam = new CAStorOpenParameter();
            try
            {
                base.Open();
                Dictionary<string, string> parms = XriObject.Params;
                if (XriObject.Params.ContainsKey(XRIParameterKeys.CustomizedModeKey))
                {
                    string customizedmetamode = XriObject.Params[XRIParameterKeys.CustomizedModeKey];
                    this.openParam.CustomizedMetaMode = (CustomizedMode)Enum.Parse(typeof(CustomizedMode), customizedmetamode.ToLower(CultureInfo.InvariantCulture).Trim(), true);
                }

                if (XriObject.Params.ContainsKey(XRIParameterKeys.CustomizedMetaKey))
                {

                    this.openParam.CustomizedMetaData = ParseCustomizedMetaData(XriObject.Params[XRIParameterKeys.CustomizedMetaKey]);

                }

                if (XriObject.Params.ContainsKey(XRIParameterKeys.LocatorType))
                {
                    string locatortype = XriObject.Params[XRIParameterKeys.LocatorType];
                    this.openParam.LocatorType = (LocatorType)Enum.Parse(typeof(LocatorType), locatortype.ToLower(CultureInfo.InvariantCulture).Trim(), true);
                }


                if (XriObject.Params.ContainsKey(XRIParameterKeys.Caringo_Communication_Key))
                {
                    string locatortype = XriObject.Params[XRIParameterKeys.Caringo_Communication_Key];
                    this.openParam.LocatorType = (LocatorType)Enum.Parse(typeof(LocatorType), locatortype.ToLower(CultureInfo.InvariantCulture).Trim(), true);
                }

                if (parms.ContainsKey(XRIParameterKeys.PramaryNodeKey))
                {
                    this.openParam.PrimaryNodes = parms[XRIParameterKeys.PramaryNodeKey];
                    if (openParam.LocatorType.Equals(LocatorType.None))
                    {
                        try
                        {
                            XRI tmpxri = XRI.ValueOf("docave-xam://" + openParam.PrimaryNodes);
                            openParam.PrimaryNodes = tmpxri.VIM;
                            if (tmpxri.Params.ContainsKey(XRIParameterKeys.Locator))
                            {
                                openParam.LocatorType = (LocatorType)int.Parse(tmpxri.Params[XRIParameterKeys.Locator]);
                            }
                            else
                            {
                                openParam.LocatorType = LocatorType.Proxy;
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceWarning(ex.Message);
                            openParam.LocatorType = LocatorType.Proxy;
                        }
                    }
                }
                if (parms.ContainsKey(XRIParameterKeys.ParamyNodePortKey))
                {
                    this.openParam.PrimaryPort = int.Parse(parms[XRIParameterKeys.ParamyNodePortKey]);
                }
                if (parms.ContainsKey(XRIParameterKeys.ClusterNameKey))
                {
                    this.openParam.ClusterName = parms[XRIParameterKeys.ClusterNameKey];
                }
                if (parms.ContainsKey(XRIParameterKeys.CRPublisherKey))
                {
                    this.openParam.PrimaryPublisher = parms[XRIParameterKeys.CRPublisherKey];
                }
                if (parms.ContainsKey(XRIParameterKeys.CRPublisherPortKey))
                {
                    this.openParam.PrimaryPublisherPort = int.Parse(parms[XRIParameterKeys.CRPublisherPortKey]);
                }

                if (parms.ContainsKey(XRIParameterKeys.WithRemoteClusterKey)
                    || parms.ContainsKey(XRIParameterKeys.CRGWithRemoteClusterKey))
                {
                    if (parms.ContainsKey(XRIParameterKeys.WithRemoteClusterKey))
                    {
                        this.openParam.UseRemoteCluster = bool.Parse(parms[XRIParameterKeys.WithRemoteClusterKey]);
                    }
                    if (parms.ContainsKey(XRIParameterKeys.CRGWithRemoteClusterKey))
                    {
                        this.openParam.UseRemoteCluster = bool.Parse(parms[XRIParameterKeys.CRGWithRemoteClusterKey]);

                    }
                    if (parms.ContainsKey(XRIParameterKeys.RemoteCSNHostKey))
                    {
                        this.openParam.RemoteClusterType = 0;
                        this.openParam.RemoteCSNHost = parms[XRIParameterKeys.RemoteCSNHostKey];
                        if (parms.ContainsKey(XRIParameterKeys.RemoteCSNPorttKey))
                        {
                            this.openParam.RemoteCSNPort = int.Parse(parms[XRIParameterKeys.RemoteCSNPorttKey]);
                        }
                    }
                    if (parms.ContainsKey(XRIParameterKeys.SCSPProxyHostKey))
                    {
                        this.openParam.RemoteClusterType = 1;
                        this.openParam.ScspProxyHost = parms[XRIParameterKeys.SCSPProxyHostKey];
                        if (parms.ContainsKey(XRIParameterKeys.SCSPProxyPortKey))
                        {
                            this.openParam.ScspProxyPort = int.Parse(parms[XRIParameterKeys.SCSPProxyPortKey]);
                        }
                        if (parms.ContainsKey(XRIParameterKeys.RemoteClusterNameKey))
                        {
                            this.openParam.RemoteClusterName = parms[XRIParameterKeys.RemoteClusterNameKey];
                        }
                    }
                }

                if (parms.ContainsKey(XRIParameterKeys.NumberOfObjectReplicasKey))
                {
                    this.openParam.Replication = ushort.Parse(parms[XRIParameterKeys.NumberOfObjectReplicasKey]);
                }
                if (parms.ContainsKey(XRIParameterKeys.DxOptimizerCompressionValueKey))
                {
                    string compress = parms[XRIParameterKeys.DxOptimizerCompressionValueKey];
                    if (compress.Equals(XRIParameterKeys.DxOptimizerBestCompressionValueKey))
                    {
                        this.openParam.CompressionType = 1;
                    }
                    else if (compress.Equals(XRIParameterKeys.DxOptimizerFastCompressionValueKey))
                    {
                        this.openParam.CompressionType = 2;
                    }
                    else
                    {
                        this.openParam.CompressionType = 0;
                    }
                }
                if (parms.ContainsKey(XRIParameterKeys.DerferCompresstionKey))
                {
                    this.openParam.DerferCompresstion = int.Parse(parms[XRIParameterKeys.DerferCompresstionKey]);
                }
                if (XriObject.Params.ContainsKey(XRIParameterKeys.REMOTE_HOST_TIMEOUT))
                {
                    this.openParam.SecondaryTimeout = long.Parse(XriObject.Params[XRIParameterKeys.REMOTE_HOST_TIMEOUT]);
                }
                if (XriObject.Params.ContainsKey(XRIParameterKeys.CACHE_REMOTE_HOST))
                {
                    this.openParam.CacheSecondary = bool.Parse(XriObject.Params[XRIParameterKeys.CACHE_REMOTE_HOST]);
                }
                if (parms.ContainsKey(XRIParameterKeys.DSM_MODIFY_TIME_KEY))
                {
                    this.openParam.ModifyTime = parms[XRIParameterKeys.DSM_MODIFY_TIME_KEY];
                }
                if (!string.IsNullOrEmpty(this.SystemID))
                {
                    this.openParam.PhysicalId = this.SystemID;
                }

                if (parms.ContainsKey(XRIParameterKeys.RETRY_INTERVAL))
                {
                    int retryInterval = int.Parse(XriObject.Params[XRIParameterKeys.RETRY_INTERVAL]);
                    if (retryInterval < 1000 || retryInterval >= int.MaxValue)
                    {
                        throw new Exception(string.Format("unknown retryInterval value {0}.", retryInterval));
                    }
                    openParam.RetryInterval = retryInterval;
                }

                if (parms.ContainsKey(XRIParameterKeys.RETRY_COUNT))
                {
                    int retryCount = int.Parse(XriObject.Params[XRIParameterKeys.RETRY_COUNT]);
                    if (retryCount < 1 || retryCount >= int.MaxValue)
                    {
                        throw new Exception(string.Format("unknown retryCount value {0}.", retryCount));
                    }
                    openParam.MaxRetryCount = retryCount;
                }

                if (client == null)
                {
                    client = new CAStorClient(openParam);
                    client.VimName = this.XriObject.VIM;
                }
                SetSystemDescription();
                this.SystemHealth = XSystemHealth.AvailableAndNotFull;

            }
            catch (Exception ex)
            {
                logger.Error("open castor storage system failed:" + ex.Message, ex);
                this.SystemHealth = XSystemHealth.Unaccessable;
            }
            return new StorageOpenValidResult();
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Stor")]
        public override string Type
        {
            get
            {
                return "CAStorSystem";
            }
        }
        public override StorageOpenValidResult Validate()
        {
            CheckState();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            StorageOpenValidResult rs = new StorageOpenValidResult();
            try
            {
                rs = client.HasPermissions();
                this.innerTotalFreeSpace = rs.TotalFreeSpace;
                this.innerTotalUsedSpace = rs.TotalUsedSpace;
                this.innerTotalSpace = rs.TotalSpace;

                if (ValidateIsFull())
                {
                    rs.SystemHealth = XSystemHealth.Available;
                }
                else
                {
                    rs.SystemHealth = XSystemHealth.AvailableAndNotFull;
                }
                this.SystemHealth = rs.SystemHealth;
            }
            catch (Exception ex)
            {
                EventIds.Storage.VerifyFailedEventMessage verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage(this.SystemLocation, ContextValues.Storage.StorageType.DELLDXStorage, ex);
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.DELL_DX_Storage, verifyFailedEventMessage);
                //this.logger.Error(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.DELL, verifyFailedEventMessage, accessDeniedErrorCodeException);
                rs.SystemHealth = XSystemHealth.ConnectedFailed;
                this.SystemHealth = XSystemHealth.ConnectedFailed;
                if (ex is NoCastorNodesLocatedException)
                {
                    logger.Error("validate Castor storage client failed, NoCastorNodesLocatedException:" + ex.Message, ex);
                    rs.Message = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Create_proxy_locator_failed", AbstractXSystem.Culture);
                }
                else if (ex is ScspWebException)
                {
                    logger.Error("validate Castor storage client failed, SCSPWebException:" + ex.Message, ex);
                    rs.Message = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Cannot_connect_to_the_remote_server", AbstractXSystem.Culture);
                }
                else
                {
                    logger.Error("validate Castor storage client failed:" + ex.Message, ex);
                    rs.Message = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Test_failed", AbstractXSystem.Culture);
                    rs.SystemHealth = XSystemHealth.AuthenticationFailed;
                    this.SystemHealth = XSystemHealth.AuthenticationFailed;
                }
            }
            return rs;
        }

        protected override void SetSystemDescription()
        {
            if (client.VimName.Equals(VIMName.CAStor))
            {
                Properties[SystemPropertyKeys.SystemDescriptionKey] = "DELL DX Object Storage Server";
            }
            else if (client.VimName.Equals(VIMName.Caringo))
            {
                Properties[SystemPropertyKeys.SystemDescriptionKey] = "Caringo Object Storage Server";
            }
            else
            {
                throw new Exception(string.Format("Vim Name error, client.VimName={0}", client.VimName));
            }
            List<string> keys = new List<string>();
            if (this.openParam.LocatorType == LocatorType.Static)
            {
                keys.Add(this.openParam.PrimaryNodes);
                keys.Add(this.openParam.PrimaryPort.ToString());
            }
            else
            {
                keys.Add(this.openParam.ScspProxyHost);
                keys.Add(this.openParam.ScspProxyPort.ToString());
                keys.Add(this.openParam.ClusterName);
            }
            List<string> securityKeys = new List<string>();
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        public override void Close()
        {
            try
            {
                if (client != null)
                {
                    try
                    {
                        client.Close();
                    }
                    catch (Exception e)
                    {
                        logger.Error("close client Error : " + e.Message, e);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.Message);
            }
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            StorageDeleteResult rs = new StorageDeleteResult();
            //标记执行过删除
            Deletion = true;
            return rs;
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
            //标记执行过删除
            Deletion = true;
            return rs;
        }

        private void DeleteUploadedFile(StorageInfo info, StorageDeleteResult rs)
        {
            if (FileExists(info))
            {
                string nextMetaID = (string)client.Invoke("GetNextMetaId", new object[] { info });
                if (!string.IsNullOrEmpty(nextMetaID))
                {
                    DeleteUploadedFile(new StorageInfo() { ObjectId = nextMetaID }, rs);
                }
                rs.DeletedFileSize += OpenFile(info).FileSize;
                client.Invoke("DeleteFile", new object[] { info });
            }
        }

        public override bool FileExists(StorageInfo info)
        {
            CheckState();
            try
            {
                //return client.FileExists(info);
                return (bool)client.Invoke("FileExists", new object[] { info });
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            CheckState();
            //return client.OpenFile(fileInfo);
            return (XFileInfo)client.Invoke("OpenFile", new object[] { fileInfo });
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            CAStorStream stream = null;
            if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew || fileMode == FileMode.Truncate || fileMode == FileMode.OpenOrCreate)
            {
                this.Written = true;
                this.lastStreamInfo = info.Clone();
            }

            CheckState();
            try
            {
                if (lastStream != null && !string.IsNullOrEmpty(lastStream.LastMetaId))
                {
                    this.LastMetaId = lastStream.LastMetaId;
                }
                stream = new CAStorStream(this.client, info, LastMetaId, this);
                stream.InitStream(fileMode);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
            return stream;
        }

        private CAStorStream lastStream;

        public override StorageResult CommitStream(Stream commitStream, StorageInfo info)
        {
            CAStorStream stream = null;
            CheckState();
            StorageResult rs = new StorageResult();
            try
            {
                this.lastStreamInfo = info.Clone();
                if (lastStream != null && !string.IsNullOrEmpty(lastStream.LastMetaId))
                {
                    this.LastMetaId = lastStream.LastMetaId;
                }
                using (stream = new CAStorStream(this.client, info, LastMetaId, this, commitStream))
                {
                    rs = stream.Commit();
                    rs.URI = stream.GetURI();
                    rs.IsCommited = true;
                    this.lastStream = stream;
                }
                this.Written = true;
            }
            catch (Exception ex)
            {
                logger.Error("Upload file {0}error:{1}", info.HighPlusLowName, ex);
                rs.IsCommited = false;
                rs.Message = ex.ToString();
                throw;
            }
            return rs;
        }

        public override void MergeStorageInfo<T>(List<T> indexList, StorageResult result, PropertyInfo propertyInfo)
        {
            if (!string.IsNullOrEmpty(result.StorageInfo))
            {
                string value = null;
                if (this.lastStreamInfo.DataType == DataBlockType.MetaData)
                {
                    CAStorStorageInfo casInfo = CAStorUtil.Convert2CAStorStorageInfo(result.StorageInfo);
                    foreach (T index in indexList)
                    {
                        value = propertyInfo.GetValue(index, null) as string;
                        CAStorStorageInfo cas = CAStorUtil.Convert2CAStorStorageInfo(value);
                        if (string.IsNullOrEmpty(cas.MetaId))
                        {
                            cas.MetaId = casInfo.ContentId;
                            propertyInfo.SetValue(index, CAStorUtil.Convert2StorageInfo(cas), null);
                        }
                    }
                    result.NeedCommit = true;
                }
                else
                {
                    CAStorStorageInfo casInfo = CAStorUtil.Convert2CAStorStorageInfo(result.StorageInfo);
                    foreach (T index in indexList)
                    {
                        value = propertyInfo.GetValue(index, null) as string;
                        CAStorStorageInfo cas = CAStorUtil.Convert2CAStorStorageInfo(value);
                        if (string.IsNullOrEmpty(cas.ContentId))
                        {
                            cas.ContentId = casInfo.ContentId;
                            propertyInfo.SetValue(index, CAStorUtil.Convert2StorageInfo(cas), null);
                        }
                    }
                }
            }
        }

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
