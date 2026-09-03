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

namespace AvePoint.Media.Storage
{
    #region using directives
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/4/11",
    "rongbiao.sun@avepoint.com",
    "yanxin.fu@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_STREAM_1 },
    "ADO-28237",
     true)]
    [AveCodeReview(
    "2013/9/19",
    "da.sun@avepoint.com",
    "da.sun@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_CS_2, CodeReviewConstants.CHECK_LIST_ID_CS_3 },
    "ADO-89254",
     true,
     new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_3 })]
    #endregion
    public abstract class AbstractXSystem : IXSystem
    {
        private long totalReadBytes;
        private long totalReadTicks;
        private long totalWriteBytes;
        private long totalWriteTicks;
        private Hashtable properties = Hashtable.Synchronized(new Hashtable());
        private AveLogger logger = AveLogger.GetInstance(typeof(AbstractXSystem));

        protected bool createIfNotExist = true;
        protected ulong innerTotalSpace = long.MaxValue - 1;
        protected ulong innerTotalUsedSpace = 0;
        protected ulong innerTotalFreeSpace = long.MaxValue - 1;

        #region 构造函数

        public AbstractXSystem(string xriString, AbstractXSystem parentSystem)
        {
            this.XriString = xriString;
            this.ParentSystem = parentSystem;
            this.SystemHealth = XSystemHealth.Unknown;
            if (!string.IsNullOrEmpty(XriString))
            {
                this.XriObject = XRI.ValueOf(this.XriString);
            }
            this.Properties.Add(SystemPropertyKeys.SystemDescriptionKey, "");
        }

        public AbstractXSystem(string xriString)
            : this(xriString, null)
        { }

        public AbstractXSystem()
            : this(null)
        { }

        #endregion

        #region 常用属性

        public AbstractXSystem ParentSystem { get; set; }
        public virtual XRI XriObject { get; set; }
        public virtual string SystemID { get; set; }
        public virtual string SystemName { get; set; }
        public virtual string SystemLocation { get; set; }
        public virtual string RootFolerPath { get; set; }
        public virtual string SystemKey { get; set; }
        public virtual string Type { get; set; }
        public virtual XSystemHealth SystemHealth { get; set; }
        public virtual XSystemStatus SystemStatus { get; set; }
        public virtual XSystemUsage SystemUsage { get; set; }
        public virtual bool IsDirectSystem { get; set; }
        /// <summary>
        /// 是否支持同时读写, 主要是约定可用于支持同时读写的System, 目前主要指TSM 
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is supports simultaneous reading and writing; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsSimulReadWriteSystem { get; set; }
        public FeatureCustomized FeatureCustomized { set; get; }
        public virtual ulong TotalSpace { get { return innerTotalSpace; } }
        public virtual ulong TotalUsedSpace { get { return innerTotalUsedSpace; } }
        public virtual ulong TotalFreeSpace { get { return innerTotalFreeSpace; } }
        public SpaceThresholdUnit SpaceThresholdUnit { get; set; } //0--Unknown, 1--MB, 2--%
        public ulong SpaceThreshold { get; set; }
        public virtual bool IsFull { get { return false; } }
        public virtual bool CreateIfNotExists { get { return createIfNotExist; } }
        public virtual StorageInterfaceType StorageInterfaceType { get { return StorageInterfaceType.Namespace; } }
        public Hashtable Properties { get { return properties; } }
        public virtual Predicate<IXSystem> FindCondition { get; set; }
        public virtual FileBlockType SupportedFileType { get; set; }
        public virtual string XriString { get; set; }
        public virtual bool IsSupportAutoChangeDataBlock { get; set; }
        public virtual bool IsSupportAutoCheck { get { return true; } }
        public virtual ulong AvailableSpace { get { return 0; } }
        //这个参数主要是用来区分上层功能的，media的话，不给这个值赋值。connector是1.其他功能也要赋相应的值
        public ModuleType moduleType { get; set; }
        public bool ReadOnly { get; set; }
        public bool Written { get; set; }
        public bool Deletion { get; set; }
        public static CultureInfo Culture { get; set; }
        public List<PhysicalDeviceDto> PhysicalDtos { set; get; }

        //这些参数主要是用来表示retry的
        public int MaxRetryCount { get; set; }
        public int RetryInterval { get; set; }
        public bool IsRetry { get; set; }
        public bool IsForcePassValidation { get; set; }
        protected WebProxy Proxy { get; set; }
        #endregion

        #region 最常用方法

        public virtual StorageOpenValidResult Open()
        {
            if (string.IsNullOrEmpty(XriString))
            {
                return null;
            }
            XriObject = XRI.ValueOf(XriString);
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SYSTEM_ID_KEY))
            {
                SystemID = XriObject.Params[XRIParameterKeys.SYSTEM_ID_KEY];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SYSTEM_NAME_KEY))
            {
                SystemName = XriObject.Params[XRIParameterKeys.SYSTEM_NAME_KEY];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SYSTEM_STATUS_KEY))
            {
                SystemStatus = (XSystemStatus)int.Parse(XriObject.Params[XRIParameterKeys.SYSTEM_STATUS_KEY]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SYSTEM_USAGE_KEY))
            {
                SystemUsage = (XSystemUsage)int.Parse(XriObject.Params[XRIParameterKeys.SYSTEM_USAGE_KEY]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.MODULE_TYPE_KEY))
            {
                moduleType = (ModuleType)(int.Parse(XriObject.Params[XRIParameterKeys.MODULE_TYPE_KEY]));
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SPACE_THRESHOLD_KEY))
            {
                long size = long.Parse(XriObject.Params[XRIParameterKeys.SPACE_THRESHOLD_KEY]);
                this.SpaceThreshold = size >= 0 ? (ulong)size : 0;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SPACE_THRESHOLD_UNIT_KEY))
            {
                this.SpaceThresholdUnit = (SpaceThresholdUnit)int.Parse(XriObject.Params[XRIParameterKeys.SPACE_THRESHOLD_UNIT_KEY]);
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CultureInfo_Key))
            {
                Culture = new CultureInfo(XriObject.Params[XRIParameterKeys.CultureInfo_Key]);
            }
            else
            {
                Culture = new CultureInfo("en");
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.ADVANCED_KEY))
            {
                if (XriObject.Params[XRIParameterKeys.ADVANCED_KEY].Equals("True", StringComparison.CurrentCultureIgnoreCase)
                    && XriObject.Params.ContainsKey(XRIParameterKeys.EXTENDED_PARAMETERS))
                {
                    logger.Debug("get advanced option:" + XriObject.Params[XRIParameterKeys.EXTENDED_PARAMETERS]);
                    AssembleAdvancedOption(XriObject.Params, XriObject.Params[XRIParameterKeys.EXTENDED_PARAMETERS]);
                }
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.RETRY_COUNT))
            {
                this.MaxRetryCount = int.Parse(XriObject.Params[XRIParameterKeys.RETRY_COUNT]);
            }
            else
            {
                this.MaxRetryCount = 6;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.RETRY_INTERVAL))
            {
                this.RetryInterval = int.Parse(XriObject.Params[XRIParameterKeys.RETRY_INTERVAL]);
                if (RetryInterval <= 0 || RetryInterval >= int.MaxValue)
                {
                    throw new Exception("unknown RetryInterval value");
                }
            }
            else
            {
                this.RetryInterval = 30 * 1000;
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.IS_RETRY))
            {
                this.IsRetry = bool.Parse(XriObject.Params[XRIParameterKeys.IS_RETRY]);
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.ForcePassValidationKey))
            {
                this.IsForcePassValidation = bool.Parse(XriObject.Params[XRIParameterKeys.ForcePassValidationKey]);
            }
            return null;
        }

        public virtual StorageOpenValidResult Open(FeatureCustomized featureCustomized)
        {
            SetFeatureCustomized(featureCustomized);
            return this.Open();
        }

        protected virtual void SetFeatureCustomized(FeatureCustomized featureCustomized) { }

        public virtual StorageResult CommitStream(Stream commitStream, StorageInfo info)
        {
            CheckState();
            while (true)
            {
                try
                {
                    long writeLength = 0;
                    StorageResult rs = new StorageResult();
                    commitStream.Position = 0;
                    StorageInfo infoClone = info.Clone();
                    logger.Debug("commit file {0} length {1}, stream length {2}", infoClone.HighPlusLowName, info.Length, commitStream.Length);
                    byte[] buffer = new byte[64 * 1024];
                    int readLen = 0;
                    using (XStream stream = OpenStream(infoClone, FileMode.Create))
                    {
                        //stream.IsCommitStream = true;
                        while ((readLen = commitStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            stream.Write(buffer, 0, readLen);
                            writeLength += readLen;
                        }
                        rs = stream.Commit(infoClone.IsClosing);
                        rs.URI = stream.GetURI();
                        rs.IsCommited = true;
                    }
                    logger.Debug("write length for file {0} : {1}", info.LowName, writeLength);
                    this.Written = true;
                    return rs;
                }
                catch (NotEnoughFreeSpaceException e)
                {
                    logger.Error("commit file {0} failed:{1}", info.HighPlusLowName, e);
                    throw;
                }
                catch (Exception e)
                {
                    logger.Error("commit file {0} failed:{1}", info.HighPlusLowName, e);
                    if (info.CurrentRetryCount < this.MaxRetryCount && this.IsRetry)
                    {
                        logger.Info("this is a retry able exception, retry it, retry count:{0}, max retry:{1}", info.CurrentRetryCount, this.MaxRetryCount);
                        info.CurrentRetryCount++;
                        Thread.Sleep(this.RetryInterval);
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        #region for box
        public virtual XFileInfo CreateFileSharedLink(StorageInfo info, AcessMode accessMode, Boolean canDownload)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual XDirectoryInfo CreateFolderSharedLink(StorageInfo info, AcessMode accessMode, Boolean canDownload)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual XFileInfo DisableFileSharedLink(StorageInfo info)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual XDirectoryInfo DisableFolderSharedLink(StorageInfo info)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual bool LockFile(StorageInfo info)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual bool UnlockFile(StorageInfo info)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual XFileInfo OpenFileWithTags(StorageInfo info)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        #endregion

        public virtual XPerformanceResult GetDevicePerformance(IOType type, int writeRatio = 0, string blokeSize = "64k")
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual StorageOpenValidResult Validate()
        {
            StorageOpenValidResult result = new StorageOpenValidResult();
            result.IsDeleteAble = true;
            result.IsWriteAble = true;
            result.IsReadAble = true;
            this.SystemHealth = XSystemHealth.AvailableAndNotFull;
            result.SystemHealth = XSystemHealth.AvailableAndNotFull;
            result.TotalFreeSpace = long.MaxValue - 1;
            result.TotalSpace = long.MaxValue - 1;
            result.TotalUsedSpace = 0;
            this.innerTotalFreeSpace = long.MaxValue - 1;
            this.innerTotalSpace = long.MaxValue - 1;
            this.innerTotalUsedSpace = 0;
            if (IsFull)
            {
                this.SystemHealth = XSystemHealth.Available;
            }
            return result;
        }

        public void Dispose()
        {
            Close();
        }
       
        protected virtual void CheckState(Int32 stackLevel = 2)
        {
            HandleReadOnlyMethod(stackLevel);
            if (this.SystemHealth <= XSystemHealth.Unknown)
            {
                this.Open();
            }
        }

        private void HandleReadOnlyMethod(Int32 stackLevel)
        {
            if (!this.ReadOnly) return;
            var st = new StackTrace();
            var methodName = st.GetFrame(stackLevel).GetMethod().Name;
            if (!this.GetWriteAndUpdateMethods().Contains(methodName)) return;
            this.Properties[SystemPropertyKeys.SystemDescriptionKey] = this.SystemLocation;
            this.logger.Warn("The method :" + methodName + " is not support for readOnly type ");
            throw new MethodNotSupportForReadOnlyDeviceException("The device is set to read-only");
        }

        private List<string> GetWriteAndUpdateMethods()
        {
            return new List<string>() { "DeleteFile", "DeleteDirectory", "MoveDirectory", "CopyFile", "MoveFile", "OpenStream" };
        }

        protected virtual void CheckState(FileMode fileMode, int stackLevel = 2)
        {
            switch (fileMode)
            {
                case FileMode.Open:
                    if (this.SystemHealth < XSystemHealth.Available)
                    {
                        this.Validate();
                    }
                    break;
                case FileMode.OpenOrCreate:
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.Truncate:
                case FileMode.Append:
                    HandleReadOnlyMethod(stackLevel);
                    this.Written = true;
                    if (this.SystemHealth < XSystemHealth.AvailableAndNotFull)
                    {
                        this.Validate();
                    }
                    if (this.IsFull)
                    {
                        this.SystemHealth = XSystemHealth.Available;
                    }
                    break;
                default:
                    break;
            }
        }

        public virtual void MergeStorageInfo<T>(List<T> indexList, StorageResult result, PropertyInfo propertyInfo)
        {
            result.NeedCommit = true;
        }
       
        public virtual StorageCopyResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite)
        {
            StorageCopyResult result = new StorageCopyResult();
            using (XStream readStream = this.OpenStream(srcFile, FileMode.Open))
            {
                if (readStream.CanSeek)
                {
                    if (isOverWrite || !destSystem.FileExists(destFile))
                    {
                        StorageResult rs = destSystem.CommitStream(readStream, destFile);
                        if (rs.IsCommited)
                        {
                            result.URI = rs.URI;
                            return result;
                        }
                    }
                    else
                    {
                        return result;
                    }
                }
                else
                {
                    XStream writeStream;
                    if (isOverWrite || !destSystem.FileExists(destFile))
                    {
                        writeStream = destSystem.OpenStream(destFile, FileMode.OpenOrCreate);
                    }
                    else
                    {
                        return result;
                    }
                    using (writeStream)
                    {
                        byte[] buffer = new byte[1024 * 64];
                        int readLen = 0;
                        while ((readLen = readStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            writeStream.Write(buffer, 0, readLen);
                        }
                        writeStream.Commit();
                        XURIResult uri = writeStream.GetURI();
                        result.URI = uri;
                    }
                }
            }
            return result;
        }

        protected virtual void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "";
        }

        public virtual List<string> GetUsedSystemsDuringWritten()
        {
            if (Written)
            {
                return new List<string>() { Properties[SystemPropertyKeys.SystemDescriptionKey] as string };
            }
            return null;
        }

        public virtual List<string> GetUsedSystemsDuringDeletion()
        {
            if (Deletion)
            {
                return new List<string>() { Properties[SystemPropertyKeys.SystemDescriptionKey] as string };
            }
            return null;
        }

        #endregion

        #region internal method to set properties

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void UpdatePropertyValue(string key, object value)
        {
            if (Properties.ContainsKey(key))
            {
                Properties[key] = value;
            }
            else
            {
                Properties.Add(key, value);
            }
        }

        public void IncreaseTotalReadBytes(long count)
        {
            Interlocked.Add(ref totalReadBytes, count);
            UpdatePropertyValue(SystemPropertyKeys.TotalReadBytes, totalReadBytes);
            if (ParentSystem != null)
            {
                ParentSystem.IncreaseTotalReadBytes(count);
            }
        }

        public void IncreaseTotalReadTicks(long count)
        {
            Interlocked.Add(ref totalReadTicks, count);
            UpdatePropertyValue(SystemPropertyKeys.TotalReadTicks, totalReadTicks);
            if (ParentSystem != null)
            {
                ParentSystem.IncreaseTotalReadTicks(count);
            }
        }

        public void IncreaseTotalWriteBytes(long count)
        {
            Interlocked.Add(ref totalWriteBytes, count);
            UpdatePropertyValue(SystemPropertyKeys.TotalWriteBytes, totalWriteBytes);
            if (ParentSystem != null)
            {
                ParentSystem.IncreaseTotalWriteBytes(count);
            }
        }

        public void IncreaseTotalWriteTicks(long count)
        {
            Interlocked.Add(ref totalWriteTicks, count);
            UpdatePropertyValue(SystemPropertyKeys.TotalWriteTicks, totalWriteTicks);
            if (ParentSystem != null)
            {
                ParentSystem.IncreaseTotalWriteTicks(count);
            }
        }

        public void UpdateProperty<T>(String key, T value)
        {
            properties[key] = value;
        }

        public T GetProperty<T>(String key)
        {
            if (properties.ContainsKey(key))
            {
                return (T)properties[key];
            }
            else
            {
                return default(T);
            }
        }

        public void IncreaseValue(String key, long value)
        {
            long v = GetProperty<long>(key);
            UpdateProperty<long>(key, v + value);
        }

        #endregion

        #region abstract function
        public abstract StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo);

        public abstract StorageDeleteResult DeleteFile(StorageInfo info);

        public abstract bool DirectoryExists(StorageInfo info);

        public abstract StorageDeleteResult DeleteDirectory(StorageInfo info);

        public abstract bool FileExists(StorageInfo info);

        public abstract StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite);

        public abstract StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite);

        public abstract StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite);

        public abstract void Close();

        public abstract XStream OpenStream(StorageInfo info, FileMode fileMode);

        public abstract XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode);

        public abstract XFileInfo OpenFile(StorageInfo fileInfo);

        public abstract List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo);

        public abstract List<XFileInfo> ListFiles(StorageInfo dirInfo);

        public abstract StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo);
        
        public virtual IEnumerable<List<XFileInfo>> GetFilesInBatch(StorageInfo dirInfo, int batchSize)
        {
            return Array.Empty<List<XFileInfo>>();
        }

        public virtual IEnumerable<List<XDirectoryInfo>> GetDirectoriesInBatch(StorageInfo dirInfo, int batchSize)
        {
            return Array.Empty<List<XDirectoryInfo>>();
        }

        #endregion
        public StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem)
        {
            return MoveFile(srcFile, destSystem, srcFile);
        }

        public virtual StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile)
        {
            StorageCopyResult sr = CopyFile(srcFile, destSystem, destFile, true);
            if (sr.IsCopyed)
            {
                StorageMoveResult result = new StorageMoveResult();
                result.URI = sr.URI;
                DeleteFile(srcFile);
                return result;
            }
            else
            {
                throw new Exception(string.Format("move file {0} failed, destination file : {1}", srcFile.HighPlusLowName, destFile.HighPlusLowName));
            }
        }

        public virtual bool AddMetadata(StorageInfo storageInfo) { return false; }

        protected bool ValidateIsFull()
        {
            if (this.SpaceThresholdUnit == SpaceThresholdUnit.MB)
            {
                if (this.innerTotalFreeSpace <= this.SpaceThreshold * 1024 * 1024)
                {
                    return true;
                }
            }
            if (this.SpaceThresholdUnit == SpaceThresholdUnit.PERCENT)
            {
                if (this.innerTotalFreeSpace * 100.0 / this.innerTotalSpace <= this.SpaceThreshold)
                {
                    return true;
                }
            }
            return false;
        }

        protected string GenerateSystemKey(List<string> keys, List<string> securityKeys)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string key in keys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    sb.Append(key).Append(XRIParameterKeys.ParamSeparator);
                }
            }
            foreach (string securityKey in securityKeys)
            {
                if (!string.IsNullOrEmpty(securityKey))
                {
                    sb.Append(HashCodeHelper.ToMD5HashCode(securityKey)).Append(XRIParameterKeys.ParamSeparator);
                }
            }
            string systemKey = sb.ToString();
            if (!string.IsNullOrEmpty(systemKey) && systemKey.EndsWith(XRIParameterKeys.ParamSeparator, StringComparison.OrdinalIgnoreCase))
            {
                systemKey = systemKey.Substring(0, systemKey.Length - XRIParameterKeys.ParamSeparator.Length);
            }
            return systemKey;
        }

        protected string RefreshAccessTokenFromAos(string url)
        {
            logger.Debug("The request token url is {0}", url);
            var request = WebRequest.Create(url) as HttpWebRequest;
            var result = string.Empty;
            request.Method = "GET";
            request.ContentType = "application/json";
            if (this.Proxy != null)
            {
                request.Proxy = this.Proxy;
                if (request.Proxy.Credentials != null)
                {
                    request.PreAuthenticate = true;
                }
            }
            try
            {
                var response = request.GetResponse() as WebResponse;
                if (response != null)
                {
                    result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                var errorResponse = ex.Response as HttpWebResponse;
                if (errorResponse != null)
                {
                    using (var errorReader = new StreamReader(errorResponse.GetResponseStream()))
                    {
                        logger.Error("The response error is:{0}", errorReader.ReadToEnd());
                    }
                }
                throw;
            }
            return result;
        }

        public virtual Boolean ConvertLongPathToSymlink(String symlinkPath, String targetPath)
        {
            return true;
        }

        protected void AssembleAdvancedOption(Dictionary<string, string> param, string extendedParameters)
        {
            var extendedParams = extendedParameters.Replace("%3D", "=").Replace("%3d", "=");
            var regex = new Regex("([^=\r\n]+)=([^\r\n]+)");
            var matchCollection = regex.Matches(extendedParams);
            foreach (Match match in matchCollection)
            {
                string key = match.Groups[1].Value.ToLower(CultureInfo.InvariantCulture).Trim();
                string value = match.Groups[2].Value.Trim();
                if (!param.ContainsKey(key))
                {
                    param[key] = value;
                }
            }
        }

        //{[test1,test1],[test2,test2],[test3,tests3]}    \\[([^,]+),([^\\]]+)
        protected Dictionary<string, string> ParseCustomizedMetaData(string metaData)
        {
            var customizedMetaDatas = new Dictionary<string, string>();
            var regex = new Regex("\\[([^,]+),([^\\]]+)");
            var matchCollection = regex.Matches(metaData);
            foreach (Match match in matchCollection)
            {
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value;
                if (!customizedMetaDatas.ContainsKey(key))
                {
                    customizedMetaDatas[key] = value;
                }
            }
            return customizedMetaDatas;
        }
    }
}
