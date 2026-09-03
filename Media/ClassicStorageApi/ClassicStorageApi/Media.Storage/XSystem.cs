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



using System.Globalization;

namespace AvePoint.Media.ClassicStorage
{

    #region using directives
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.ClassicStorage.Util;
    using global::Storage;
    #endregion


    /// <summary>
    /// 包含device验证信息的一个对象, 用于直接操作Directory, File, Stream等, 层次上对应于一个Physical Device.
    /// </summary>
    public interface IXSystemCommon : IEnumerable, IDisposable, IXSystem
    {
        #region public properties
        /// <summary>
        /// 以Int形式显示System类型
        /// </summary>
        Int32 TypeValue { get; }
        /// <summary>
        /// 以字符串形式显示System类型
        /// </summary>
        /// <value>The type.</value>
        string Type { get; }

        /// <summary>
        /// 根据device的配置信息，返回一个key,
        /// 用来判断不同的system的配置信息是否相同.
        /// </summary>
        string SystemKey { get; }

        /// <summary>
        /// 当前系统是否在线，是DocAve系统的状态
        /// </summary>
        //XSystemStatus SystemStatus { get; set; }

        /// <summary>
        /// 当前系统用途，存放all/data/index
        /// </summary>
        XSystemUsage SystemUsage { get; }

        /// <summary>
        /// TO DO 5.x中的属性，6.0中应该去掉
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is cache system; otherwise, <c>false</c>.
        /// </value>
        bool IsCacheSystem { get; }

        XRI XriObject { get; set; }

        /// <summary>
        /// 定义查找可用XSystem的条件, 支持自定义
        /// </summary>
        Predicate<IXSystemCommon> FindCondition { set; get; }

        bool IsSupportAutoDeletion { set; get; }

        bool IsSupportAutoCheck { get; }

        #endregion

        #region common function

        /// <summary>
        /// 在调用open方法时，我们会对device的配置进行验证，同时会返回一个StorageOpenValidResult
        /// 该对象中包含了device的一些信息，如果剩余空间等。
        /// </summary>
        /// <returns></returns>
        StorageOpenValidResult Open();

        /// <summary>
        /// 初始化IXSystem, Required.
        /// </summary>
        /// <param name="featureCustomized">
        /// 有些功能对特殊的介质需要定制特性: 比如Extender & Hold数据存在EMC Centera上时是一个Blob一个Clip, 而其他功能像Item\PR\Archiver则是多个Blob在同一个Clip, 对应该的上层逻辑是有要求的.
        /// 如果是新的功能需要用到Storage API, 请联系API Developer确认相关注意点.
        /// </param>
        /// <returns></returns>
        StorageOpenValidResult Open(FeatureCustomized featureCustomized);

        StorageOpenValidResult Open(string xri);

        /// <summary>
        /// 如果需要在运行时验证已经存在的System。可以调用这个方法。
        /// </summary>
        /// <returns></returns>
        StorageOpenValidResult Validate();

        /// <summary>
        /// 在System调用完成之后，调用该方法，进行一些回收操作。
        /// </summary>
        void Close();

        void MergeStorageInfo<T>(List<T> ts, StorageResult rs, PropertyInfo p);

        /// <summary>
        /// 在上传和下载数据时，都是调用这个方法，我们通过fileMode判断是上传还是下载。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="fileMode"></param>
        /// <returns></returns>
        XStream OpenStream(StorageInfo info, FileMode fileMode);

        /// <summary>
        /// 这个文件主要是用于上传，目前还不支持续写。
        /// </summary>
        /// <param name="commitStream"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        StorageResult CommitStream(Stream commitStream, StorageInfo info);

        /// <summary>
        /// 在想要获取一个Directory的相关信息调用该方法。
        /// 在Directory不存在时，我们会根据mode判断是不是要进行创建操作。
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode);

        /// <summary>
        ///  在想要获取一个File的相关信息调用该方法。
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        XFileInfo OpenFile(StorageInfo fileInfo);

        /// <summary>
        /// 将windows不支持的长路径通过symbollink的方式转换为短路径
        /// </summary>
        /// <param name="symlinkPath">转换后的路径</param>
        /// <param name="targetPath">原路径</param>
        /// <returns>If the function succeeds, the return value is nonzero.If the function fails, the return value is zero.</returns>
        Boolean ConvertLongPathToSymlink(String symlinkPath, String targetPath);

        /// <summary>
        /// 删除整个文件夹及其下面的文件
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        StorageDeleteResult DeleteDirectory(StorageInfo info);

        /// <summary>
        /// 删除指定的文件
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        StorageDeleteResult DeleteFile(StorageInfo info);

        /// <summary>
        /// 获取指定的文件夹下的所有子文件夹。（仅往下展开一层）
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns></returns>
        List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo);

        /// <summary>
        /// 获取指定的文件夹下的文件。（仅往下展开一层）
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns></returns>
        List<XFileInfo> ListFiles(StorageInfo dirInfo);

        /// <summary>
        ///  获取指定的文件夹下的文件和子文件夹。（仅往下展开一层）
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns></returns>
        StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo);


        /// <summary>
        ///  带有缓冲策略的获取指定的文件夹下的文件和子文件夹。（仅往下展开一层 ， 推荐使用）
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns></returns>
        StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo);

        /// <summary>
        /// 单纯判断指定文件夹是否存在
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        bool DirectoryExists(StorageInfo info);
        bool DirectoryExistsAzure(AvePoint.Media.ClassicStorage.StorageInfo info);
        /// <summary>
        /// 单纯判断文件是否存在
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        bool FileExists(StorageInfo info);




        bool IsSupportMultithreaded();
        /// <summary>
        /// 将指定的文件复制到目标文件夹下。
        /// </summary>
        /// <param name="sourceFileInfo"></param>
        /// <param name="targetFileInfo"></param>
        /// <param name="isOverWrite"></param>
        /// <returns></returns>
        StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite);

        /// <summary>
        /// 将指定的文件剪切到目标文件夹下。
        /// </summary>
        /// <param name="sourceFileInfo"></param>
        /// <param name="targetFileInfo"></param>
        /// <param name="isOverWrite"></param>
        /// <returns></returns>
        StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite);

        /// <summary>
        /// 将指定的文件夹及其下面的内容剪切到目标文件夹下。
        /// </summary>
        /// <param name="sourceDirInfo"></param>
        /// <param name="targetDirInfo"></param>
        /// <param name="isOverWrite"></param>
        /// <returns></returns>
        StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite);

        StorageCopyResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite);

        /// <summary>
        /// Move file from one system to another system.
        /// </summary>
        /// <param name="fileNeedMoved"></param>
        /// <param name="destSystem"></param>
        /// <exception cref="PathNotFoundException"/>
        /// <exception cref="PathAlreadyExistsException"/>
        /// <returns></returns>
        StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem);

        /// <summary>
        /// Move file from one system to another system.
        /// </summary>
        /// <param name="fileNeedMoved"></param>
        /// <param name="destSystem"></param>
        /// <exception cref="PathNotFoundException"/>
        /// <exception cref="PathAlreadyExistsException"/>
        /// <returns></returns>
        StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile);

        /// <summary>
        /// 获取文件夹中所有文件的大小，只给Online Azure使用
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        Int64 GetDirectorySize(StorageInfo info);

        List<string> GetUsedSystemsDuringWritten();

        List<string> GetUsedSystemsDuringDeletion();

        //  Boolean VidatateFilterInformation(OntapFilerInfo filterInformation);

        StorageChangeResult ChangeFileTier(StorageInfo info);

        #endregion
    }

    #region CodeReview
    [AveCodeReview(
    "2012/4/11",
    "rongbiao.sun@avepoint.com",
    "yanxin.fu@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_STREAM_1 },
    "ADO-28237",
     true)]
    #endregion
    public abstract class AbstractXSystem : XObject, IXSystemCommon
    {
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

        public virtual Boolean ConvertLongPathToSymlink(String symlinkPath, String targetPath)
        {
            return true;
        }

        #endregion

        #region 常用属性

        public AbstractXSystem ParentSystem { get; set; }
        public virtual XRI XriObject { get; set; }
        public virtual string SystemID { get; set; }
        public virtual string SystemName { get; set; }
        public virtual string SystemLocation { get; set; }
        public virtual string SystemKey { get; set; }
        public virtual string Type { get; set; }
        public virtual Int32 TypeValue { get; set; }
        public virtual XSystemHealth SystemHealth { get; set; }
        //public virtual XSystemStatus SystemStatus { get; set; }
        public virtual XSystemUsage SystemUsage { get; set; }
        public virtual bool IsDirectSystem { get; set; }
        public virtual string SystemPath { get; set; }

        /// <summary>
        /// 是否支持同时读写, 主要是约定可用于支持同时读写的System, 目前主要指TSM 
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is supports simultaneous reading and writing; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsSimulReadWriteSystem { get; set; }
        public virtual bool IsCacheSystem { get; set; }
        protected ulong totalSpace = long.MaxValue - 1;
        protected ulong totalUsedSpace = 0;
        protected ulong totalFreeSpace = long.MaxValue - 1;
        public FeatureCustomized FeatureCustomized { set; get; }
        public virtual ulong TotalSpace { get { return totalSpace; } }
        public virtual ulong TotalUsedSpace { get { return totalUsedSpace; } }
        public virtual ulong TotalFreeSpace { get { return totalFreeSpace; } }
        public SpaceThresholdUnit SpaceThresholdUnit { get; set; } //0--Unknown, 1--MB, 2--%
        public ulong SpaceThreshold { get; set; }
        public virtual bool IsFull { get { return false; } }
        public virtual bool CreateIfNotExists { get; set; }
        public virtual StorageInterfaceType StorageInterfaceType { get { return StorageInterfaceType.Namespace; } }
        private Hashtable properties = Hashtable.Synchronized(new Hashtable());
        public Hashtable Properties { get { return properties; } }
        public virtual Predicate<IXSystemCommon> FindCondition { get; set; }
        //public virtual FileBlockType SupportedFileType { get; set; }
        public virtual string XriString { get; set; }
        public virtual bool IsSupportAutoChangeDataBlock { get; set; }
        public virtual bool IsSupportAutoDeletion { get; set; }
        public virtual bool IsSupportAutoCheck { get { return true; } }
        public virtual ulong AvailableSpace { get { return 0; } }

        //这个参数主要是用来区分上层功能的，media的话，不给这个值赋值。connector是1.其他功能也要赋相应的值
        public ModuleType moduleType { get; set; }
        public bool ReadOnly { get; set; }
        public bool Written { get; set; }
        public bool Deletion { get; set; }
        private StorageApi.AdvancedOptionUtil optionUtil = new StorageApi.AdvancedOptionUtil();
        public StorageApi.AdvancedOptionUtil OptionUtil { set { this.optionUtil = value; } get { return this.optionUtil; } }
        public static CultureInfo Culture { get; set; }
        public List<PhysicalDeviceDto> PhysicalDtos { set; get; }

        //这些参数主要是用来表示retry的
        public int MaxRetryCount { get; set; }
        public int RetryInterval { get; set; }
        public bool IsRetry { get; set; }
        public bool IsForcePassValidation { get; set; }

        public XStorageType StorageType => throw new NotImplementedException();

        public string LocalTempPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Predicate<IXSystemCommon> IXSystemCommon.FindCondition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Predicate<IXSystem> IXSystem.FindCondition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private bool isRetryOverwrite;
        #endregion

        #region 最常用方法

        public virtual StorageOpenValidResult Open()
        {
            return Open(XriString);
        }

        public StorageOpenValidResult Open(FeatureCustomized featureCustomized)
        {
            SetFeatureCustomized(featureCustomized);
            return this.Open();
        }

        protected virtual void SetFeatureCustomized(FeatureCustomized featureCustomized) { }

        public virtual StorageOpenValidResult Open(string xri)
        {
            if (string.IsNullOrEmpty(xri))
            {
                return null;
            }

            XriObject = XRI.ValueOf(xri);
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SYSTEM_ID_KEY))
            {
                SystemID = XriObject.Params[XRIParameterKeys.SYSTEM_ID_KEY];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SYSTEM_NAME_KEY))
            {
                SystemName = XriObject.Params[XRIParameterKeys.SYSTEM_NAME_KEY];
            }
            //if (XriObject.Params.ContainsKey(XRIParameterKeys.SYSTEM_STATUS_KEY))
            //{
            //    SystemStatus = (XSystemStatus)int.Parse(XriObject.Params[XRIParameterKeys.SYSTEM_STATUS_KEY]);
            //}
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
                    OptionUtil.AssembleAdvancedOption(XriObject.Params, XriObject.Params[XRIParameterKeys.EXTENDED_PARAMETERS]);
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
            else
            {
                this.IsRetry = true;
            }

            if (XriObject.Params.ContainsKey(XRIParameterKeys.ForcePassValidationKey))
            {
                this.IsForcePassValidation = bool.Parse(XriObject.Params[XRIParameterKeys.ForcePassValidationKey]);
            }
            return null;
        }

        public virtual XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual StorageResult CommitStream(Stream commitStream, StorageInfo info)
        {

            CheckState();
            while (true)
            {
                var storageResult = new StorageResult();
                try
                {
                    commitStream.Position = 0;
                    var tempInfo = info.Clone();
                    this.logger.Debug("commit file:{0}", info.HighPlusLowName);
                    var buffer = new byte[64 * 1024];
                    using (var stream = OpenStream(tempInfo, FileMode.Create))
                    {
                        stream.IsCommitStream = true;
                        int readLen;
                        try
                        {
                            while ((readLen = commitStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                stream.Write(buffer, 0, readLen);
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.Error("An error occuured while commit stream,details:{0}.", e.ToString());
                            throw;
                        }
                        storageResult = stream.Commit(tempInfo.IsClosing);
                        storageResult.URI = stream.GetURI();
                        storageResult.IsCommited = true;
                    }
                    this.Written = true;
                    return storageResult;
                }
                catch (Exception e)
                {
                    this.logger.Error("commit file {0} failed:{1}", info.HighPlusLowName, e.ToString());
                    if (info.CurrentRetryCount < this.MaxRetryCount && this.IsRetry)
                    {
                        logger.Info("this is a retry able exception, retry it, retry count:{0}, max retry:{1}", info.CurrentRetryCount, this.MaxRetryCount);
                        info.CurrentRetryCount++;
                        Thread.Sleep(this.RetryInterval);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public virtual StorageDeleteResult DeleteFile(StorageInfo info)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual bool FileExists(StorageInfo info)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual StorageOpenValidResult Validate()
        {
            StorageOpenValidResult rs = new StorageOpenValidResult();
            rs.IsDeleteAble = true;
            rs.IsWriteAble = true;
            rs.IsReadAble = true;
            this.SystemHealth = XSystemHealth.AvailableAndNotFull;
            rs.SystemHealth = XSystemHealth.AvailableAndNotFull;
            rs.TotalFreeSpace = long.MaxValue - 1;
            rs.TotalSpace = long.MaxValue - 1;
            rs.TotalUsedSpace = 0;
            this.totalFreeSpace = long.MaxValue - 1;
            this.totalSpace = long.MaxValue - 1;
            this.totalUsedSpace = 0;
            if (IsFull)
            {
                this.SystemHealth = XSystemHealth.Available;
            }
            return rs;
        }

        public virtual void Close()
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        //public virtual Boolean VidatateFilterInformation(OntapFilerInfo filterInformation)
        //{
        //    throw new NotImplementedException("Not Implemented in this layer.");
        //}

        public virtual XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual XFileInfo OpenFile(StorageInfo fileInfo)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }



        public override void Dispose()
        {
            base.Dispose();
            Close();
        }

        public virtual bool DirectoryExists(StorageInfo info)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        protected virtual void CheckState(int stackLevel = 2)
        {
            HandleReadOnlyMethod(stackLevel);
            if (this.SystemHealth <= XSystemHealth.Unknown)
            {
                this.Open();
            }
        }

        private void HandleReadOnlyMethod(int stackLevel)
        {
            if (ReadOnly)
            {
                StackTrace st = new StackTrace();
                string methodName = st.GetFrame(stackLevel).GetMethod().Name;

                if (GetWriteAndUpdateMethods().Contains(methodName))
                {
                    Properties[SystemPropertyKeys.SystemDescriptionKey] = this.SystemLocation;
                    logger.Warn("The method :" + methodName + " is not support for readOnly type ");
                    throw new MethodNotSupportForReadOnlyDeviceException("The device is set to read-only");
                }
            }
        }

        private List<string> GetWriteAndUpdateMethods()
        {
            return new List<string>() { "DeleteFile", "DeleteDirectory", "MoveDirectory", "CopyFile", "MoveFile", "OpenStream" };
        }

        /*protected virtual void CheckState(FileMode fileMode, int stackLevel = 2)
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
        }*/


        public virtual bool IsSupportMultithreaded()
        {
            return true;
        }

        public virtual void MergeStorageInfo<T>(List<T> ts, StorageResult rs, PropertyInfo p)
        {
            rs.NeedCommit = true;
        }

        public virtual StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public virtual StorageCopyResult CopyFile(StorageInfo srcFile, IXSystemCommon destSystem, StorageInfo destFile, bool isOverWrite)
        {
            if (!(destSystem is XLibrary) && (destSystem.Type.Equals("SFTPSystem") || destSystem.Type.Equals("FTPSystem")))//TODO
            {
                logger.Info("Try to copy SFTP/FTP file, source file {0}, destination file {1}", srcFile.HighPlusLowName, destFile.HighPlusLowName);
                var tempInfo = destFile.Clone();
                tempInfo.LowName = destFile.LowName + "_" + Guid.NewGuid().ToString();
                var result = RetryCopyFile(srcFile, destSystem, tempInfo, isOverWrite);
                var srcFileInfo = OpenFile(srcFile);
                var dstFileInfo = destSystem.OpenFile(tempInfo);
                if (srcFileInfo == null || dstFileInfo == null || srcFileInfo.FileSize != dstFileInfo.FileSize)
                {
                    throw new Exception("The source file " + srcFile.HighPlusLowName + " and destination file size" + destFile.HighPlusLowName + " are not matching");
                }
                var moveResult = destSystem.MoveFile(tempInfo, destFile, true);
                if (moveResult.IsMoved)
                {
                    return result;
                }
                else
                {
                    return new StorageCopyResult() { IsCopyed = false, Message = moveResult.Message };
                }
            }
            else
            {
                return RetryCopyFile(srcFile, destSystem, destFile, isOverWrite);
            }
        }

        private StorageCopyResult RetryCopyFile(StorageInfo srcFile, IXSystemCommon destSystem, StorageInfo destFile, bool isOverWrite)
        {
            int retryIndex = 0;
            int maxRetryCount = 6;
            Exception error = null;
            this.isRetryOverwrite = isOverWrite;
            logger.Info("Try to copy file, source file {0}, destination file {1}", srcFile.HighPlusLowName, destFile.HighPlusLowName);
            while (true)
            {
                try
                {
                    return CommonCopyFile(srcFile, destSystem, destFile, this.isRetryOverwrite);
                }
                catch (Exception e)
                {
                    error = e;
                    logger.Error(e.Message, e);
                }

                if (retryIndex < maxRetryCount)
                {
                    logger.Info($"this is a retry able exception, retry it, retry count:{retryIndex}, max retry:{maxRetryCount}");
                    retryIndex++;
                }
                else
                {
                    logger.Error($"cannot copy file from {srcFile} to {destFile}");
                    throw error;
                }
                Thread.Sleep(30 * 1000);
            }
        }

        private StorageCopyResult CommonCopyFile(StorageInfo srcFile, IXSystemCommon destSystem, StorageInfo destFile, bool isOverWrite)
        {
            StorageCopyResult result = new StorageCopyResult();
            using (XStream readStream = this.OpenStream(srcFile, FileMode.Open))
            {
                if (readStream.CanSeek)
                {
                    if (isOverWrite || !destSystem.FileExists(destFile))
                    {
                        this.isRetryOverwrite = true;
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
                        this.isRetryOverwrite = true;
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

        #region property values

        private long totalReadBytes;
        private long totalReadTicks;
        private long totalWriteBytes;
        private long totalWriteTicks;

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

        public void UpdateProperty<T>(SystemPropertyKey key, T value)
        {
            properties[key] = value;
        }

        public T GetProperty<T>(SystemPropertyKey key)
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

        public void IncreaseValue(SystemPropertyKey key, long value)
        {
            long v = GetProperty<long>(key);
            UpdateProperty<long>(key, v + value);
        }

        #endregion


        public StorageMoveResult MoveFile(StorageInfo srcFile, IXSystemCommon destSystem)
        {
            return MoveFile(srcFile, destSystem, srcFile);
        }

        public virtual StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        public StorageMoveResult MoveFile(StorageInfo srcFile, IXSystemCommon destSystem, StorageInfo destFile)
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

        private AveLogger logger = AveLogger.GetInstance(typeof(AbstractXSystem));
        //private StorageMoveResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile)
        //{
        //    StorageMoveResult sr = new StorageMoveResult();

        //    using (XStream srcStream = OpenStream(srcFile, FileMode.Open))
        //    {
        //        XStream destStream = null;
        //        using (destStream = destSystem.OpenStream(destFile, FileMode.CreateNew))
        //        {
        //            byte[] buffer = new byte[1024 * 1024];
        //            int readLen = 0;
        //            while (true)
        //            {
        //                readLen = srcStream.Read(buffer, 0, buffer.Length);
        //                if (readLen <= 0)
        //                {
        //                    break;
        //                }
        //                destStream.Write(buffer, 0, readLen);
        //            }
        //            destStream.Commit();
        //            XURIResult uri = destStream.GetURI();
        //            sr.URI = uri;
        //            if (uri != null && uri.SInfo != null)
        //            {
        //                logger.Debug("[Write]Commit Successfully : " + uri.SInfo.HighName + " | " + uri.SInfo.LowName);
        //            }
        //        }


        //    }
        //    return sr;
        //}

        public virtual bool AddMetadata(StorageInfo storageInfo) { return false; }

        protected bool ValidateIsFull()
        {
            if (this.SpaceThresholdUnit == SpaceThresholdUnit.MB)
            {
                if (this.totalFreeSpace <= this.SpaceThreshold * 1024 * 1024)
                {
                    return true;
                }
            }
            if (this.SpaceThresholdUnit == SpaceThresholdUnit.PERCENT)
            {
                if (this.totalFreeSpace * 100.0 / this.totalSpace <= this.SpaceThreshold)
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


        public virtual Int64 GetDirectorySize(StorageInfo info)
        {
            throw new NotSupportedException();
        }

        public virtual StorageChangeResult ChangeFileTier(StorageInfo info)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }

        void IXSystem.Open()
        {
            this.Open();
        }

        global::Storage.StorageOpenValidResult IXSystem.Validate()
        {
            throw new NotImplementedException();
        }

        public StorageAccountProps GetStorageAccountProps()
        {
            throw new NotImplementedException();
        }

        public global::Storage.XStream OpenStream(global::Storage.StorageInfo info, FileMode fileMode)
        {
            throw new NotImplementedException();
        }

        public void DownloadFile(global::Storage.StorageInfo info, Stream inputStream)
        {
            throw new NotImplementedException();
        }

        public global::Storage.StorageResult CommitStream(Stream commitStream, global::Storage.StorageInfo info)
        {
            throw new NotImplementedException();
        }

        public global::Storage.XDirectoryInfo OpenDirectory(global::Storage.StorageInfo dirInfo, FileMode mode)
        {
            throw new NotImplementedException();
        }

        public global::Storage.XFileInfo OpenFile(global::Storage.StorageInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public global::Storage.StorageDeleteResult DeleteDirectory(global::Storage.StorageInfo info)
        {
            throw new NotImplementedException();
        }

        public void DeleteDirectoryDirectly(global::Storage.StorageInfo info)
        {
            throw new NotImplementedException();
        }

        public global::Storage.StorageDeleteResult DeleteFile(global::Storage.StorageInfo info)
        {
            throw new NotImplementedException();
        }

        public List<global::Storage.XDirectoryInfo> ListDirectories(global::Storage.StorageInfo dirInfo)
        {
            throw new NotImplementedException();
        }

        public List<global::Storage.XFileInfo> ListFiles(global::Storage.StorageInfo dirInfo)
        {
            throw new NotImplementedException();
        }

        public global::Storage.StorageListResult ListSubDirectoriesAndFiles(global::Storage.StorageInfo dirInfo)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(global::Storage.StorageInfo info)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(global::Storage.StorageInfo info)
        {
            throw new NotImplementedException();
        }

        public global::Storage.StorageCopyResult CopyFile(global::Storage.StorageInfo sourceFileInfo, global::Storage.StorageInfo targetFileInfo, bool isOverWrite)
        {
            throw new NotImplementedException();
        }

        public global::Storage.StorageCopyResult MoveFile(global::Storage.StorageInfo sourceFileInfo, global::Storage.StorageInfo targetFileInfo, bool isOverWrite)
        {
            throw new NotImplementedException();
        }

        public global::Storage.StorageCopyResult MoveDirectory(global::Storage.StorageInfo sourceDirInfo, global::Storage.StorageInfo targetDirInfo, bool isOverWrite)
        {
            throw new NotImplementedException();
        }

        public global::Storage.StorageCopyResult CopyFile(global::Storage.StorageInfo srcFile, IXSystem destSystem, global::Storage.StorageInfo destFile, bool isOverWrite)
        {
            throw new NotImplementedException();
        }

        public global::Storage.StorageCopyResult MoveFile(global::Storage.StorageInfo srcFile, IXSystem destSystem, global::Storage.StorageInfo destFile)
        {
            throw new NotImplementedException();
        }

        public Task<global::Storage.StorageOpenValidResult> ValidateAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Stream> OpenStreamAsync(global::Storage.StorageInfo info)
        {
            throw new NotImplementedException();
        }

        public Task DownloadFileAsync(global::Storage.StorageInfo info, Stream inputStream)
        {
            throw new NotImplementedException();
        }

        public Task<global::Storage.StorageResult> UploadAsync(Stream stream, global::Storage.StorageInfo info)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(global::Storage.StorageInfo info, bool isDirectory)
        {
            throw new NotImplementedException();
        }

        public Task<global::Storage.StorageDeleteResult> DeleteAsync(global::Storage.StorageInfo info, bool isDirectory)
        {
            throw new NotImplementedException();
        }

        public Task<global::Storage.XFileInfo> OpenFileAsync(global::Storage.StorageInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public Task<global::Storage.XDirectoryInfo> OpenDirectoryAsync(global::Storage.StorageInfo dirInfo, FileMode mode)
        {
            throw new NotImplementedException();
        }

        public Task<List<global::Storage.XDirectoryInfo>> ListDirectoryAsync(global::Storage.StorageInfo dirInfo)
        {
            throw new NotImplementedException();
        }

        public Task<List<global::Storage.XFileInfo>> ListFileAsync(global::Storage.StorageInfo dirInfo)
        {
            throw new NotImplementedException();
        }

        public Task<global::Storage.StorageListResult> ListSubDirectoryAndFileAsync(global::Storage.StorageInfo dirInfo)
        {
            throw new NotImplementedException();
        }

        public Task<global::Storage.StorageCopyResult> CopyFileAsync(global::Storage.StorageInfo srcFile, global::Storage.StorageInfo destFile, bool isOverWrite)
        {
            throw new NotImplementedException();
        }

        public Task<global::Storage.StorageCopyResult> CopyFileAsync(global::Storage.StorageInfo srcFile, IXSystem destSystem, global::Storage.StorageInfo destFile, bool isOverWrite)
        {
            throw new NotImplementedException();
        }

        public StorageCopyResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite)
        {
            throw new NotImplementedException();
        }

        public StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem)
        {
            throw new NotImplementedException();
        }

        public StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile)
        {
            throw new NotImplementedException();
        }

        public virtual bool DirectoryExistsAzure(AvePoint.Media.ClassicStorage.StorageInfo info)
        {
            throw new NotImplementedException();
        }
    }




    public class SystemPropertyKeys
    {

        public const string TotalReadBytes = "TotalReadBytes";
        public const string TotalReadTicks = "TotalReadTicks";
        public const string TotalWriteBytes = "TotalWriteBytes";
        public const string TotalWriteTicks = "TotalWriteTicks";

        //used system information during one write session
        public const string SystemsUsedDuringWriting = "SystemsUsedDuringWriting";
        public const string SystemDescriptionKey = "SystemDescription";

        //PUT, COPY, POST, LIST, GET, DELETE
        public static readonly SystemPropertyKey DATA_TRANSFER_IN = new SystemPropertyKey("Data Transfer In");
        public static readonly SystemPropertyKey DATA_TRANSFER_OUT = new SystemPropertyKey("Data Transfer Out");
        public static readonly SystemPropertyKey REQUEST_PUT = new SystemPropertyKey("Request Put");
        public static readonly SystemPropertyKey REQUEST_COPY = new SystemPropertyKey("Request Copy");
        public static readonly SystemPropertyKey REQUEST_POST = new SystemPropertyKey("Request Post");
        public static readonly SystemPropertyKey REQUEST_LIST = new SystemPropertyKey("Request List");
        public static readonly SystemPropertyKey REQUEST_GET = new SystemPropertyKey("Request Get");
        public static readonly SystemPropertyKey REQUEST_DELETE = new SystemPropertyKey("Request Delete");
        public static readonly SystemPropertyKey REQUEST_HEAD = new SystemPropertyKey("Request Head");
    }

    public class SystemPropertyKey
    {
        private string key;
        public SystemPropertyKey(string key)
        {
            this.key = key;
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }


    public enum SpaceThresholdUnit
    {
        UNKNOWN = 0,
        MB = 1,
        PERCENT = 2,
    }

    public class FeatureCustomized
    {

        public FeatureCustomized()
        {
            PhysicalDeviceDtos = new List<PhysicalDeviceDto>();
        }
        /// <summary>
        /// Muliti Thread Safe
        /// </summary>
        public bool MTSafe { get; set; }

        public CASLevel CASLevel { get; set; }
        public List<PhysicalDeviceDto> PhysicalDeviceDtos { get; set; }
        public static readonly FeatureCustomized Default = new FeatureCustomized() { CASLevel = CASLevel.MulitiBlobs };
        public static readonly FeatureCustomized ForBlob = new FeatureCustomized() { CASLevel = CASLevel.SingleBlob };
        //public static readonly FeatureCustomized ForHold = new FeatureCustomized() { CASLevel = CASLevel.SingleBlob };

    }

    public enum CASLevel
    {
        SingleBlob,
        MulitiBlobs,
    }

}
