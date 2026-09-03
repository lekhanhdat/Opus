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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AvePoint.GCommon;
    using AvePoint.Media.Storage.Inner;
    using AvePoint.Media.Storage.Util;
    #endregion

    /// <summary>
    /// XLibrary 用于对应Logical Device, 里面包含了对Storage最底层对象Physical Device的组织应用
    /// </summary>
    public class XLibrary : AbstractXSystem
    {
        IXSystem workingSystem;
        Predicate<IXSystem> findCondition = Basic;
        List<IXSystem> subSystems = new List<IXSystem>();
        XSystemHealth MaxSystemHealth = XSystemHealth.Unknown;
        AveLogger logger = AveLogger.GetInstance(typeof(XLibrary));

        public IXSystem GetWorkingSystem()
        {
            return workingSystem;
        }

        public override XRI XriObject { get { return this.workingSystem.XriObject; } set { this.workingSystem.XriObject = value; } }

        public override UInt64 TotalSpace
        {
            get
            {
                UInt64 totalSpace = 0;
                foreach (IXSystem system in subSystems)
                {
                    totalSpace += system.TotalSpace;
                }
                return totalSpace;
            }
        }

        public override UInt64 TotalUsedSpace
        {
            get
            {
                UInt64 totalUsedSpace = 0;
                foreach (IXSystem system in SubSystems)
                {
                    totalUsedSpace += system.TotalUsedSpace;
                }
                return totalUsedSpace;
            }
        }

        public override UInt64 TotalFreeSpace
        {
            get
            {
                UInt64 totalFreeSpace = 0;
                foreach (IXSystem system in SubSystems)
                {
                    totalFreeSpace += system.TotalFreeSpace;
                }
                return totalFreeSpace;
            }
        }
        public override UInt64 AvailableSpace
        {
            get
            {
                UInt64 availableSpace = 0;
                List<UInt64> freeSpaceList = new List<UInt64>(); //为了防止两个physical device指向同一个磁盘的不同folder而统计重复
                foreach (IXSystem system in SubSystems)
                {
                    system.Open();
                    system.Validate();
                    if (system.SystemHealth >= XSystemHealth.AvailableAndNotFull)
                    {
                        if (!freeSpaceList.Contains(system.TotalFreeSpace))
                        {
                            UInt64 size = system.TotalFreeSpace;
                            if (((AbstractXSystem)system).SpaceThresholdUnit == SpaceThresholdUnit.MB)
                            {
                                size = system.TotalFreeSpace - ((AbstractXSystem)system).SpaceThreshold * 1024 * 1024;
                            }
                            else if (((AbstractXSystem)system).SpaceThresholdUnit == SpaceThresholdUnit.PERCENT)
                            {
                                size = system.TotalFreeSpace - (UInt64)(system.TotalSpace * (((AbstractXSystem)system).SpaceThreshold / 100.0));
                            }
                            if (availableSpace + size < Int64.MaxValue - 1)
                            {
                                availableSpace += size;
                            }
                            freeSpaceList.Add(size);
                        }
                    }
                }
                logger.Info("The available space for this library is " + availableSpace);
                return availableSpace;
            }
        }

        public override Boolean IsSupportAutoChangeDataBlock { get { return workingSystem.IsSupportAutoChangeDataBlock; } }

        /// <summary>
        /// 自定义查找可用XSystem的条件
        /// Example : 
        /// <code>
        /// public Predicate&lt;XSystem&gt; FindCondition = delegate(XSystem s)
        /// {
        ///     if (s.SystemHealth == XSystemHealth.Unknown)
        ///     {
        ///         s.Open();
        ///     }
        ///     if (s.SystemHealth >= XSystemHealth.AvailableAndNotFull &amp; s.TotalFreeSpace > 20 * 1024 * 1024 * 1024)
        ///     {
        ///         return true;
        ///     }
        ///     return false;
        /// };
        /// </code>
        /// </summary>
        /// 
        public override Predicate<IXSystem> FindCondition
        {
            set
            {
                this.findCondition = value;
                workingSystem = GetNextValidSystem(XSystemHealth.Available, FindMethod.Custom);
            }
            get
            {
                return this.findCondition;
            }
        }

        public List<IXSystem> SubSystems
        {
            get
            {
                return subSystems;
            }
        }

        public static readonly Predicate<IXSystem> Basic = delegate(IXSystem system)
        {
            if (system.SystemHealth == XSystemHealth.Unknown)
            {
                system.Open();
                system.Validate();
            }
            return system.SystemHealth >= XSystemHealth.AvailableAndNotFull;
        };

        public override void MergeStorageInfo<T>(List<T> indexList, StorageResult result, System.Reflection.PropertyInfo propertyInfo)
        {
            EnsureValidSystem(XSystemHealth.Available);
            workingSystem.MergeStorageInfo<T>(indexList, result, propertyInfo);
        }
        public void AddVIM(String xriStr, IVIM vim)
        {
            this.subSystems.Add(vim.CreateSystem(xriStr, this));
        }

        public override Boolean IsDirectSystem
        {
            get
            {
                EnsureValidSystem(XSystemHealth.Available);
                return workingSystem.IsDirectSystem;
            }
        }

        public override String SystemLocation
        {
            get
            {
                EnsureValidSystem(XSystemHealth.Available);
                return workingSystem.SystemLocation;
            }
        }

        public void Open(XSystemHealth state)
        {
            EnsureValidSystem(state);
        }

        public override StorageOpenValidResult Open()
        {
            this.FeatureCustomized = this.FeatureCustomized ?? FeatureCustomized.Default;
            foreach (var system in subSystems)
            {
                system.Open(this.FeatureCustomized);
            }
            EnsureValidSystem(XSystemHealth.Available);
            return null;
        }

        public override StorageOpenValidResult Open(FeatureCustomized featureCustomized)
        {
            SetFeatureCustomized(featureCustomized);
            return Open();
        }
        protected override void SetFeatureCustomized(FeatureCustomized featureCustomized)
        {
            this.FeatureCustomized = featureCustomized;
        }
        public override List<String> GetUsedSystemsDuringWritten()
        {
            var descriptions = new List<String>();
            foreach (IXSystem sys in subSystems)
            {
                if ((sys as AbstractXSystem).Written)
                {
                    descriptions.Add(sys.Properties[SystemPropertyKeys.SystemDescriptionKey] as String);
                }
            }
            return descriptions;
        }

        public override String Type
        {
            get
            {
                return this.workingSystem.Type;
            }
        }

        public override List<String> GetUsedSystemsDuringDeletion()
        {
            var descriptions = new List<String>();
            foreach (IXSystem sys in subSystems)
            {
                if ((sys as AbstractXSystem).Deletion)
                {
                    descriptions.Add(sys.Properties[SystemPropertyKeys.SystemDescriptionKey] as String);
                }
            }
            return descriptions;
        }

        public override StorageInterfaceType StorageInterfaceType
        {
            get
            {
                EnsureValidSystem(XSystemHealth.Available);
                return workingSystem.StorageInterfaceType;
            }
        }

        public override FileBlockType SupportedFileType
        {
            get
            {
                EnsureValidSystem(XSystemHealth.Available);
                return workingSystem.SupportedFileType;
            }
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            switch (fileMode)
            {
                case FileMode.Open:
                    EnsureValidSystem(XSystemHealth.Available);
                    if (this.subSystems.Count > 1)
                    {
                        bool fileExist = false;
                        try
                        {
                            fileExist = workingSystem.FileExists(info);
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while checking file for open stream, details : {0}.", e);
                        }
                        if (!fileExist)
                        {
                            foreach (IXSystem subSystem in subSystems)
                            {
                                try
                                {
                                    subSystem.Open();
                                    if (subSystem.SystemHealth >= XSystemHealth.Available && subSystem.FileExists(info))
                                    {
                                        workingSystem = subSystem;
                                        break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    logger.Warn("An error occurred while checking file for open stream, details : {0}.", e);
                                }
                            }
                        }
                    }
                    if (workingSystem == null || workingSystem.SystemHealth < XSystemHealth.Available)
                    {
                        throw new DeviceNotAvailableException("Cannot find any available device to read.");
                    }
                    return workingSystem.OpenStream(info, fileMode);
                case FileMode.Append:
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:

                    for (int i = 0; (workingSystem.IsFull || (Int64)workingSystem.TotalFreeSpace - info.Length < 0) && i < subSystems.Count; i++)
                    {
                        workingSystem.SystemHealth = XSystemHealth.Available;
                        if (workingSystem == subSystems[i])
                        {
                            continue;
                        }
                        workingSystem = subSystems[i];
                        if (workingSystem.SystemHealth == XSystemHealth.Unknown)
                        {
                            workingSystem.Open();
                            workingSystem.Validate();
                        }
                        if (workingSystem.SystemHealth >= XSystemHealth.AvailableAndNotFull)
                        {
                            if (workingSystem.IsFull || (Int64)workingSystem.TotalFreeSpace - info.Length < 0)
                            {
                                workingSystem.SystemHealth = XSystemHealth.Available;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (workingSystem != null && workingSystem.SystemHealth == XSystemHealth.Available)
                    {
                        logger.Info("working system free space {0}", workingSystem.TotalFreeSpace);
                        throw new NotEnoughFreeSpaceException("There is no enough space on the physical devices");
                    }
                    if (workingSystem == null || workingSystem.SystemHealth < XSystemHealth.AvailableAndNotFull)
                    {
                        throw new DeviceNotAvailableException("Cannot find any available device");
                    }
                    this.Written = true;
                    break;
                default:
                    throw new Exception("Unknown File Mode : " + fileMode);
            }
            while (true)
            {
                try
                {
                    return workingSystem.OpenStream(info, fileMode);
                }
                catch (Exception ex)
                {
                    logger.Error("Open stream for file {0} failed, details : {1}", info.HighPlusLowName, ex);
                    logger.Info("We will try skip to another device");
                    workingSystem.SystemHealth = XSystemHealth.Unaccessable;
                    EnsureValidSystem(XSystemHealth.AvailableAndNotFull);
                    logger.Info("Device skip succeed");
                }
            }
        }

        public override StorageResult CommitStream(Stream commitStream, StorageInfo info)
        {
            bool isFull = false;
            for (int i = 0; (isFull = workingSystem.IsFull || (Int64)workingSystem.TotalFreeSpace - info.Length < 0) && i < subSystems.Count; i++)
            {
                workingSystem.SystemHealth = XSystemHealth.Available;
                workingSystem = subSystems[i];
                workingSystem.Open();
                workingSystem.Validate();
                if (workingSystem.SystemHealth >= XSystemHealth.AvailableAndNotFull)
                {
                    if ((isFull = workingSystem.IsFull) || ((Int64)workingSystem.TotalFreeSpace - info.Length < 0))
                    {
                        workingSystem.SystemHealth = XSystemHealth.Available;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (isFull)
            {
                if (workingSystem.SystemHealth > XSystemHealth.Available)
                    workingSystem.SystemHealth = XSystemHealth.Available;
            }
            else
            {
                workingSystem.SystemHealth = XSystemHealth.AvailableAndNotFull;
            }

            if (workingSystem != null && workingSystem.SystemHealth == XSystemHealth.Available)
            {
                logger.Info("working system free space {0}", workingSystem.TotalFreeSpace);
                throw new NotEnoughFreeSpaceException("There is no enough space on the physical devices");
            }

            if (workingSystem == null || workingSystem.SystemHealth < XSystemHealth.AvailableAndNotFull)
            {
                throw new DeviceNotAvailableException("Cannot find any available device");
            }
            this.Written = true;
            while (true)
            {
                try
                {
                    return workingSystem.CommitStream(commitStream, info);
                }
                catch (NotEnoughFreeSpaceException ex)
                {
                    logger.Error("commit stream for file {0} failed:{1}", info.HighPlusLowName, ex.Message, ex);
                    logger.Info("we will try skip to another device");
                    workingSystem.SystemHealth = XSystemHealth.Unaccessable;
                    EnsureValidSystem(XSystemHealth.AvailableAndNotFull);
                    logger.Info("device skip succeed");
                }
            }
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            var copyResut = new StorageCopyResult();
            var sourceSystem = this.GetMatchedSystem(sourceFileInfo) ?? this.workingSystem;
            var destinationSystem = this.GetNextValidSystem(XSystemHealth.AvailableAndNotFull, FindMethod.Beginning);
            if (destinationSystem == null || destinationSystem.SystemHealth < XSystemHealth.AvailableAndNotFull)
            {
                throw new DeviceNotAvailableException("Cannot find any available device to read.");
            }
            if (sourceSystem.SystemID == destinationSystem.SystemID &&
                (sourceSystem.IsDirectSystem || (sourceSystem as AbstractXSystem).IsSimulReadWriteSystem))
            {
                copyResut = sourceSystem.CopyFile(sourceFileInfo, targetFileInfo, isOverWrite);
            }
            else
            {
                try
                {
                    copyResut = sourceSystem.CopyFile(sourceFileInfo, destinationSystem, targetFileInfo, true);
                }
                catch (Exception e)
                {
                    copyResut.Message = e.Message;
                    copyResut.IsCopyed = false;
                    logger.Error("Copy file failed: " + e);
                }
            }
            return copyResut;
        }

        private IXSystem GetMatchedSystem(StorageInfo fileInfo)
        {
            var matchedSystem = default(IXSystem);
            foreach (var subSystem in this.subSystems)
            {
                subSystem.Open(this.FeatureCustomized);
                subSystem.Validate();
                if (subSystem.FileExists(fileInfo))
                {
                    matchedSystem = subSystem;
                    break;
                }
            }
            return matchedSystem;
        }

        public override StorageCopyResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite)
        {
            return base.CopyFile(srcFile, destSystem, destFile, isOverWrite);
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            var result = new StorageDeleteResult();
            result.IsDeleted = true;
            foreach (IXSystem subSystem in subSystems)
            {
                StorageDeleteResult tempResult = subSystem.DeleteDirectory(info);
                result.DeletedFileSize += tempResult.DeletedFileSize;
                result.IsDeleted = result.IsDeleted & tempResult.IsDeleted;
                result.Message += tempResult.Message;
            }
            return result;
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            var result = new StorageDeleteResult();
            result.IsDeleted = true;
            foreach (IXSystem subSystem in subSystems)
            {
                bool fileExist = subSystem.FileExists(info);
                if (fileExist)
                {
                    var tempResult = subSystem.DeleteFile(info);
                    result.DeletedFileSize += tempResult.DeletedFileSize;
                    result.IsDeleted = result.IsDeleted & tempResult.IsDeleted;
                    result.Message += tempResult.Message;
                }
            }
            return result;
        }

        public override bool DirectoryExists(StorageInfo info)
        {
            bool result = false;
            Exception exception = null;
            foreach (IXSystem subSystem in subSystems)
            {
                try
                {
                    result = subSystem.DirectoryExists(info);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    logger.Warn("Check directory exist failed, details : {0}.", ex.ToString());
                }
                if (result)
                {
                    return result;
                }
            }
            if (exception != null)
            {
                throw exception;
            }
            return result;
        }

        public override bool FileExists(StorageInfo info)
        {
            bool result = false;
            Exception exception = null;
            foreach (IXSystem subSystem in subSystems)
            {
                try
                {
                    result = subSystem.FileExists(info);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    logger.Warn("An error occurred while checking file in {0}, details {1}.", subSystem.GetType(), ex);
                }
                if (result)
                {
                    this.workingSystem = subSystem;
                    return result;
                }
            }
            if (exception != null)
            {
                throw exception;
            }
            return result;
        }

        public FileStatus CheckFileStatus(StorageInfo info)
        {
            Exception exception = null;
            var badSystemCount = 0;
            foreach (var subSystem in subSystems)
            {
                try
                {
                    if (subSystem.FileExists(info))
                    {
                        this.workingSystem = subSystem;
                        return FileStatus.Exist;
                    }
                }
                catch (Exception ex)
                {
                    badSystemCount++;
                    exception = ex;
                    logger.Warn("An error occurred while checking file in {0}, details {1}.", subSystem.GetType(), ex);
                }
            }
            if (badSystemCount == subSystems.Count && exception != null)
            {
                throw exception;
            }
            return badSystemCount == 0 ? FileStatus.NotExist : FileStatus.Unknown;
        }

        public override StorageOpenValidResult Validate()
        {
            var result = new StorageOpenValidResult();
            foreach (IXSystem system in subSystems)
            {
                if (system.SystemHealth == XSystemHealth.Unknown)
                {
                    system.Open();
                }
                var subResult = system.Validate();
                if (workingSystem.SystemHealth < system.SystemHealth)
                {
                    workingSystem = system;
                }
                result.SubResult.Add(subResult);
            }
            result.SystemHealth = workingSystem.SystemHealth;
            return result;
        }

        public override void Close()
        {
            if (this.subSystems != null && this.subSystems.Count > 0)
            {
                foreach (IXSystem system in this.subSystems)
                {
                    system.Close();
                }
            }
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            EnsureValidSystem(XSystemHealth.Available);
            return workingSystem.OpenDirectory(dirInfo, mode);
        }

        public override bool ConvertLongPathToSymlink(String symlinkPath, String targetPath)
        {
            EnsureValidSystem(XSystemHealth.Available);
            return workingSystem.ConvertLongPathToSymlink(symlinkPath, targetPath);
        }
        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            EnsureValidSystem(XSystemHealth.Available);
            Exception exception = null;
            try
            {
                if (workingSystem.FileExists(fileInfo))
                {
                    return workingSystem.OpenFile(fileInfo);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                logger.Warn("An error occurred while open file, details {0}.", ex.ToString());
            }
            foreach (IXSystem subSystem in subSystems)
            {
                try
                {
                    if (subSystem.FileExists(fileInfo))
                    {
                        workingSystem = subSystem;
                        return workingSystem.OpenFile(fileInfo);
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    logger.Warn("An error occurred while open file, details {0}.", ex.ToString());
                }
            }
            if (exception != null)
            {
                throw exception;
            }
            else
            {
                return null;
            }
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            EnsureValidSystem(XSystemHealth.Available);
            var result = new Dictionary<String, XDirectoryInfo>();
            foreach (IXSystem subSystem in subSystems)
            {
                if (subSystem.DirectoryExists(dirInfo))
                {
                    workingSystem = subSystem;
                    var xDirInfo = subSystem.ListDirectories(dirInfo);
                    if (xDirInfo != null && xDirInfo.Count != 0)
                    {
                        foreach (XDirectoryInfo info in xDirInfo)
                        {
                            result[info.HighName + info.Name] = info;
                        }
                    }
                }
            }
            return new List<XDirectoryInfo>(result.Values);
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            EnsureValidSystem(XSystemHealth.Available);
            var result = new Dictionary<String, XFileInfo>();
            foreach (IXSystem subSystem in subSystems)
            {
                if (subSystem.DirectoryExists(dirInfo))
                {
                    workingSystem = subSystem;
                    var xFileInfo = subSystem.ListFiles(dirInfo);
                    if (xFileInfo != null && xFileInfo.Count != 0)
                    {
                        foreach (XFileInfo info in xFileInfo)
                        {
                            result[info.HighName + info.LowName] = info;
                        }
                    }
                }
            }
            return new List<XFileInfo>(result.Values);
        }

        /// <summary>
        /// 确保WorkingSystem不为null(否则抛出异常)。
        /// 并且重置lastposition指针位置。
        /// </summary>
        private void EnsureValidSystem(XSystemHealth state)
        {
            if (workingSystem == null || workingSystem.SystemHealth < state)
            {
                workingSystem = GetNextValidSystem(state, FindMethod.Beginning);
            }
        }


        /// <summary>
        /// 不会抛出异常，如果找不到则返回null，需要调用者处理；找到的XSystem已经被open，需要调用者close
        /// </summary>
        /// <param name="state"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        private IXSystem GetNextValidSystem(XSystemHealth state, FindMethod post)
        {
            IXSystem sysObj = null;
            this.FeatureCustomized = this.FeatureCustomized ?? FeatureCustomized.Default;
            switch (post)
            {
                case FindMethod.Beginning:
                    sysObj = subSystems.Find(delegate(IXSystem system)
                    {
                        system.Open(this.FeatureCustomized);
                        system.Validate();
                        if (system.SystemHealth >= state)
                        {
                            return true;
                        }
                        else if (MaxSystemHealth < system.SystemHealth)
                        {
                            MaxSystemHealth = system.SystemHealth;
                        }
                        return false;
                    });
                    break;
                case FindMethod.Custom:
                    sysObj = this.subSystems.Find(findCondition);
                    break;
                default:
                    throw new Exception("Unknown Find Method : " + post.ToString());
            }
            if (sysObj != null)
            {
                return sysObj;
            }
            throw new DeviceNotAvailableException("Cannot find any available device");
        }

        public XSystemHealth GetMaxSystemHealth()
        {
            XSystemHealth tempSystemHealth = MaxSystemHealth;
            MaxSystemHealth = XSystemHealth.Unknown;
            return tempSystemHealth;
        }

        /// <summary>
        /// 寻找下一个valid XSystem的方式
        /// </summary>
        private enum FindMethod
        {
            /// <summary>
            /// 以当前位置为起点，轮询所有的XSystem
            /// </summary>
            Beginning = 0,
            /// <summary>
            /// 按照上层所传的条件去查找
            /// </summary>
            Custom = 1
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
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
