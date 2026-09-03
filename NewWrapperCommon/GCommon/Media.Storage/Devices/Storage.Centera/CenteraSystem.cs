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


namespace AvePoint.Media.Storage.Centera
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using AvePoint.Media.Storage.Util;
    using System.IO;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Inner;
    using AvePoint.GCommon.Contract.CodeReview;
    using System.Text.RegularExpressions;
    using AvePoint.Media.Storage.Resources.CenteraI18N;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/5/23",
    "rongbiao.sun@avepoint.com",
    "nan.shen@avepoint.com",
     new String[] { CodeReviewConstants.CHECK_LIST_ID_HC_1 },
    null,
     true)]
    #endregion
    class CenteraSystem : AbstractXSystem
    {
        private FPPool pool;
        private CenteraClient client;
        private UInt64 retentionDays;
        private String connectString;
        private Object locker = new Object();
        private CenteraPoolResourceTag resource;
        private CASLevel casLevel = CASLevel.MulitiBlobs;
        private Dictionary<String, FPClip> clips = new Dictionary<String, FPClip>();
        private AveLogger logger = new AveLogger(typeof(CenteraSystem));
        public CASLevel CASLevel { get { return casLevel; } }
        //TODO: 属性
        private CenteraClient Client
        {
            get
            {
                if (client == null)
                {
                    client = new CenteraClient(this.GetCentetraPool(), this.retentionDays);
                }
                return client;
            }
            set
            {
                client = value;
            }
        }

        public override StorageInterfaceType StorageInterfaceType
        {
            get
            {
                return StorageInterfaceType.Object;
            }
        }

        public override String Type
        {
            get
            {
                return "CenteraSystem";
            }
        }

        public CenteraSystem()
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_File;
            this.IsSupportAutoChangeDataBlock = true;
        }

        public CenteraSystem(String xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_File;
            SystemHealth = XSystemHealth.Unknown;
            this.IsSupportAutoChangeDataBlock = true;
            this.Open();
        }

        public override StorageOpenValidResult Open()
        {
            try
            {
                if (this.SystemHealth != XSystemHealth.Unknown)
                {
                    return new StorageOpenValidResult();
                }
                base.Open();
                if (XriObject.Params.ContainsKey(XRIParameterKeys.Centera_KEY_RetentionDays))
                {
                    this.retentionDays = UInt64.Parse(XriObject.Params[XRIParameterKeys.Centera_KEY_RetentionDays]);
                }
                this.SetSystemDescription();
                this.SystemHealth = XSystemHealth.AvailableAndNotFull;
            }
            catch (Exception ex)
            {
                this.SystemHealth = XSystemHealth.Unaccessable;
                logger.Error("Open EMC system failed:{0}", ex);
            }
            return new StorageOpenValidResult();
        }

        protected override void SetFeatureCustomized(FeatureCustomized featureCustomized)
        {
            casLevel = featureCustomized.CASLevel;
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            dirInfo.ClipId = dirInfo.LowName;
            List<XFileInfo> files = new List<XFileInfo>();
            using (FPClip clip = GetCentetraPool().OpenClip(dirInfo.ClipId))
            {
                var topTag = clip.GetTopTag();
                topTag.Close();
                var tag = clip.FetchNext();// clip.nextTag();
                while (tag != null)
                {
                    CenteraFileInfo file = new CenteraFileInfo(dirInfo.ClipId, tag.Name, tag.Length);
                    files.Add(file);
                    tag.Close();
                    tag = clip.FetchNext();//.nextTag();
                }
            }
            var results = new StorageListResult { Files = files };
            return results;
        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "EMC Centera Cluster, Cluster Address : " + XriObject.Params[XRIParameterKeys.AUTHENTICATION_ADDRESS_KEY];
            String authType = XriObject.Params[XRIParameterKeys.AUTHENTICATION_TYPE_KEY];
            if (authType.EndsWith(XRIParameterKeys.AUTHENTICATION_PROFILES_SECRET, StringComparison.OrdinalIgnoreCase))//StringComparison.OrdinalIgnoreCase
            {
                List<String> keys = new List<String>();
                keys.Add(this.XriObject.Params[XRIParameterKeys.AUTHENTICATION_ADDRESS_KEY]);
                keys.Add(this.XriObject.Params[XRIParameterKeys.AUTHENTICATION_PROFILES_KEY]);
                keys.Add(this.XriObject.Params[XRIParameterKeys.AUTHENTICATION_PROFILES_NAME_KEY]);
                List<String> securityKeys = new List<String>();
                securityKeys.Add(this.XriObject.Params[XRIParameterKeys.AUTHENTICATION_PROFILES_PASSWORD_KEY]);
                this.SystemKey = GenerateSystemKey(keys, securityKeys);
            }
            else
            {
                List<String> keys = new List<String>();
                keys.Add(this.XriObject.Params[XRIParameterKeys.AUTHENTICATION_ADDRESS_KEY]);
                keys.Add(this.XriObject.Params[XRIParameterKeys.AUTHENTICATION_NAME_KEY]);
                List<String> securityKeys = new List<String>();
                securityKeys.Add(this.XriObject.Params[XRIParameterKeys.AUTHENTICATION_SECRET_KEY]);
                this.SystemKey = GenerateSystemKey(keys, securityKeys);
            }
        }

        public override void Close()
        {
            try
            {
                this.SystemHealth = XSystemHealth.Unknown;
                lock (locker)
                {
                    foreach (FPClip clip in clips.Values)
                    {
                        clip.Close();
                    }
                    clips.Clear();
                }
                if (pool != null)
                {
                    pool = null;
                }
                if (!String.IsNullOrEmpty(connectString))
                {
                    CenteraResourcePool.Instance.ReturnObject(connectString, resource);
                }
                if (Client != null)
                {
                    Client.Dispose();
                    Client = null;
                }

            }
            catch (Exception ex)
            {
                logger.Warn("close Centera system error:" + ex.Message);
            }
        }

        public SpaceInfo CheckFreeSpace()
        {
            SpaceInfo spaceInfo = new SpaceInfo();
            FPPoolInfo info = GetCentetraPool().GetInfo();
            spaceInfo.TotalSpace = info.Capcity;
            spaceInfo.TotalFreeSpace = info.FreeSpace;
            spaceInfo.TotalUsedSpace = spaceInfo.TotalSpace - spaceInfo.TotalFreeSpace;
            logger.Info(" Server Version:{0}. ",info.Version);
            return spaceInfo;
        }

        public override StorageOpenValidResult Validate()
        {
            CheckState();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            StorageOpenValidResult result = new StorageOpenValidResult();
            try
            {
                pool = GetCentetraPool();
                Client = new CenteraClient(pool, this.retentionDays);
                result.IsReadAble = pool.GetPermisson(FPOption.FP_READ, FPOption.FP_ALLOWED);
                result.IsWriteAble = pool.GetPermisson(FPOption.FP_WRITE, FPOption.FP_ALLOWED);
                result.IsDeleteAble = pool.GetPermisson(FPOption.FP_DELETE, FPOption.FP_ALLOWED);
                SpaceInfo spaceInfo = CacheUtil.GetSpaceInfo(VIMName.Centera, connectString, CheckFreeSpace);
                result.TotalSpace = spaceInfo.TotalSpace;
                result.TotalFreeSpace = spaceInfo.TotalFreeSpace;
                result.TotalUsedSpace = spaceInfo.TotalUsedSpace;
                this.innerTotalSpace = spaceInfo.TotalSpace;
                this.innerTotalFreeSpace = spaceInfo.TotalFreeSpace;
                this.innerTotalUsedSpace = spaceInfo.TotalUsedSpace;
                logger.Info("Capacity:{0}, Available Space:{1}, Read:{2}, Write:{3}, Delete:{4}", spaceInfo.TotalSpace, spaceInfo.TotalFreeSpace, result.IsReadAble, result.IsWriteAble, result.IsDeleteAble);
                if (result.IsReadAble && result.IsWriteAble)
                {
                    if (ValidateIsFull())
                    {
                        this.SystemHealth = XSystemHealth.Available;
                    }
                    else
                    {
                        this.SystemHealth = XSystemHealth.AvailableAndNotFull;
                    }
                    if (!result.IsDeleteAble)
                    {
                        result.Message = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Does_not_have_delete_permission", AbstractXSystem.Culture);
                    }
                }
                else
                {
                    this.SystemHealth = XSystemHealth.Unaccessable;
                }
            }
            catch (Exception ex)
            {
                EventIds.Storage.VerifyFailedEventMessage verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage(this.SystemLocation, ContextValues.Storage.StorageType.EMCCentera, ex);
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.EMC_Centera, verifyFailedEventMessage);
                this.SystemHealth = XSystemHealth.AuthenticationFailed;
                if (ex is AuthenticationFailedException)
                {
                    logger.Error("validate EMC server failed, AuthenticationFailedException:" + ex.Message, ex);
                    result.Message = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Authentication_failed", AbstractXSystem.Culture);
                }
                else if (ex is FPKnownException)
                {
                    logger.Error("validate EMC server failed, FPKnownException:" + ex.Message, ex);
                    result.Message = CenteraI18N.ResourceManager.GetString((ex as FPKnownException).ErrorInformation, AbstractXSystem.Culture);
                }
                else
                {
                    logger.Error("validate EMC server failed:" + ex.Message, ex);
                    result.Message = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Test_failed", AbstractXSystem.Culture);
                }
            }
            result.SystemHealth = this.SystemHealth;
            logger.Info("Centera system Health :" + this.SystemHealth);
            return result;
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            CheckState();
            lock (locker)
            {
                this.CheckState();
                info = info.Clone();
                switch (fileMode)
                {
                    case FileMode.CreateNew:
                    case FileMode.Create:
                    case FileMode.Append:
                    case FileMode.OpenOrCreate:
                    case FileMode.Truncate:
                        logger.Debug("begin create stream, name:" + info.HighName + info.LowName);
                        CenteraOutputStream stream = new CenteraOutputStream(Client, info, this);
                        this.Written = true;
                        return stream;
                    case FileMode.Open:
                        try
                        {
                            logger.Debug("begin open stream, clipId:" + info.ClipId + ",name:" + info.LowName);
                            if (this.casLevel == Storage.CASLevel.SingleBlob)
                            {
                                return new CenteraSingleBlobInputStream(info, Client, this);
                            }
                            return new CenteraInputStream(info, Client, this);
                        }
                        catch (Exception e)
                        {
                            logger.Error("Opened the BLOB failed, name:{0}.", info.LowName + "@" + info.ClipId);
                            logger.Error(e.Message, e);
                            throw;
                        }
                    default:
                        throw new UnsupportedXException("Unknown File Mode : " + fileMode);
                }
            }
        }

        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            logger.Debug(String.Format("Open file, clip: {0}, name: {1}", fileInfo.ClipId, Path.Combine(fileInfo.HighName, fileInfo.LowName)));
            XFileInfo xFileInfo = default(XFileInfo);
            this.CheckState();
            try
            {
                if (casLevel == Storage.CASLevel.SingleBlob)
                {
                    using (FPClip clip = GetCentetraPool().OpenClip(fileInfo.ClipId))
                    {
                        using (FPTag tag = clip.OpenTag(Client.CheckName(fileInfo.LowName)))
                        {
                            if (tag != null)
                                xFileInfo = new CenteraFileInfo(fileInfo.HighName, fileInfo.LowName, tag.Length);
                        }
                    }
                }
                else
                {
                    using (FPTag tag = Client.FetchTag(fileInfo.ClipId, fileInfo.LowName))
                    {
                        if (tag != null)
                            xFileInfo = new CenteraFileInfo(fileInfo.HighName, fileInfo.LowName, tag.Length);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Open file failed, Message:{0}.", e);
            }
            if (xFileInfo == null)
            {
                logger.Warn("Open file failed, clip:{0}, name:{1}, level:{2}.", fileInfo.ClipId, fileInfo.LowName, casLevel);
            }
            return xFileInfo;
        }

        private Boolean CheckEmcClipId(String clipId)
        {
            Regex regex = new Regex(@"^[A-Za-z0-9]+$");
            return regex.Match(clipId).Success;
        }

        public override Boolean FileExists(StorageInfo info)
        {
            this.CheckState();

            if (!CheckEmcClipId(info.ClipId))
            {
                return false;
            }
            if (null == info.LowName)
            {
                if (GetCentetraPool().ExistsClip(info.ClipId))
                {
                    return true;
                }
                logger.Info("clip:" + info.ClipId + "   not exist");
                return false;
            }
            if (casLevel == Storage.CASLevel.SingleBlob)
            {
                using (FPClip clip = GetCentetraPool().OpenClip(info.ClipId))
                {
                    using (FPTag tag = clip.OpenTag(Client.CheckName(info.LowName)))
                    {
                        return tag == null ? false : true;
                    }
                }

            }
            using (FPTag tag = Client.FetchTag(info.ClipId, info.LowName))
            {
                if (tag == null)
                {
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("ClipID : {0}, File Name: {1} not exists", info.ClipId, Path.Combine(info.HighName, info.LowName));
                    }
                    return false;
                }
                return true;
            }
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            this.CheckState();
            StorageDeleteResult result = new StorageDeleteResult();
            try
            {
                logger.Info("try to delete clip clipId:   " + info.ClipId);
                using (FPClip clip = GetCentetraPool().OpenClip(info.ClipId))
                {
                    result.DeletedFileSize = Int64.Parse(clip.GetTotalSize().ToString());
                }
                GetCentetraPool().DeleteClip(info.ClipId);
            }
            catch (Exception e)
            {
                logger.Error("Delete clip failed, id:" + info.ClipId + ", message:" + e.ToString());
                if (e is FPKnownException)
                {
                    var message = CenteraI18N.ResourceManager.GetString((e as FPKnownException).ErrorInformation, AbstractXSystem.Culture);
                    throw new Exception(message);
                }
                throw;
            }
            result.IsDeleted = (!GetCentetraPool().ExistsClip(info.ClipId));
            Deletion = true;
            return result;

        }

        public override void MergeStorageInfo<T>(List<T> indexList, StorageResult result, PropertyInfo propertyInfo)
        {
            if (!String.IsNullOrEmpty(result.StorageInfo))
            {
                foreach (T index in indexList)
                {
                    propertyInfo.SetValue(index, result.StorageInfo, null);
                }
                result.NeedCommit = true;
            }
        }

        private FPPool GetCentetraPool()
        {
            if (pool == null)
            {
                lock (locker)
                {
                    String poolAddress = XriObject.Params[XRIParameterKeys.AUTHENTICATION_ADDRESS_KEY];
                    String authType = XriObject.Params[XRIParameterKeys.AUTHENTICATION_TYPE_KEY];
                    switch (authType)
                    {
                        case XRIParameterKeys.AUTHENTICATION_NAME_SECRET:
                            String name = XriObject.Params[XRIParameterKeys.AUTHENTICATION_NAME_KEY];
                            String secret = SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.AUTHENTICATION_SECRET_KEY]);
                            connectString = poolAddress + "?name=" + name + ",secret=" + secret;
                            logger.Info("open a Centera pool, name secret mode, name :{0}, address:{1}.", name, poolAddress);
                            resource = CenteraResourcePool.Instance.borrowObject(connectString);
                            break;
                        case XRIParameterKeys.AUTHENTICATION_PROFILES_SECRET:
                            String profile = XriObject.Params[XRIParameterKeys.AUTHENTICATION_PROFILES_KEY];
                            String profileName = XriObject.Params[XRIParameterKeys.AUTHENTICATION_PROFILES_NAME_KEY];
                            String profilePassword = SecretUtil.DescryptPassword(XriObject.Params[XRIParameterKeys.AUTHENTICATION_PROFILES_PASSWORD_KEY]);
                            String domain = ".";
                            String userName = profileName;
                            if (profileName.Contains("\\"))
                            {
                                domain = profileName.Substring(0, profileName.IndexOf("\\", StringComparison.Ordinal));
                                userName = profileName.Substring(profileName.IndexOf("\\", StringComparison.Ordinal) + 1);
                            }
                            UNCIdentity identity = new UNCIdentity(profile, domain, userName, profilePassword);
                            using (identity.Impersonate())
                            {
                                connectString = poolAddress + "?" + profile;
                                logger.Info("open a Centera pool, pea file mode, user name :{0}, domain:{1}, address:{2}", userName, domain, connectString);
                                resource = CenteraResourcePool.Instance.borrowObject(connectString);
                            }
                            break;
                        default:
                            throw new InvalidXRIException("Unknown EMC Centera authentication type. " + XriObject.Params[XRIParameterKeys.AUTHENTICATION_TYPE_KEY]);
                    }
                    pool = resource.Resource;
                }
            }
            return pool;
        }

        #region Not Supported Method
        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            throw new NotSupportedException();
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

        public override Boolean DirectoryExists(StorageInfo info)
        {
            throw new NotSupportedException();
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            throw new NotSupportedException();
        }

        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            throw new NotSupportedException();
        }

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, Boolean isOverWrite)
        {
            throw new NotSupportedException();
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
