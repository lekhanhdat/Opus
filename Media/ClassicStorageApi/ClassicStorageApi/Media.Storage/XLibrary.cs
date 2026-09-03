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




namespace AvePoint.Media.ClassicStorage
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CentralAdmin.Object.STSAdmin;
    using AvePoint.Media.ClassicStorage.Inner;
    using AvePoint.Media.ClassicStorage.Util;
    using global::Storage;
    #endregion

    /// <summary>
    /// XLibrary 用于对应Logical Device, 里面包含了对Storage最底层对象Physical Device的组织应用
    /// </summary>
    public class XLibraryCommon : AbstractXSystem
    {
        public XLibraryCommon()
        {
            this.findCondition = Basic;
        }

        private AveLogger logger = AveLogger.GetInstance(typeof(AveLogger));
        List<IXSystemCommon> subSystems = new List<IXSystemCommon>();
        IXSystemCommon workingSystem;
        Predicate<IXSystemCommon> findCondition;
        XSystemHealth MaxSystemHealth = XSystemHealth.Unknown;

        public IXSystemCommon GetWorkingSystem()
        {
            return workingSystem;

        }

        public override XRI XriObject { get { return this.workingSystem.XriObject; } set { this.workingSystem.XriObject = value; } }

        public ulong GetAvaliableSpace()
        {
            ulong avaliableSpace = 0;
            List<ulong> freeSpaceList = new List<ulong>();
            foreach (IXSystem system in SubSystems)
            {
                system.Open();
                system.Validate();
                if (system.SystemHealth >= XSystemHealth.AvailableAndNotFull)
                {
                    if (!(system is IXSpaceInfo spaceInfo))
                    {
                        logger.Warn("The system does not support space related properties");
                        continue;
                    }
                    if (!freeSpaceList.Contains(spaceInfo.TotalFreeSpace))
                    {
                        ulong size = spaceInfo.TotalFreeSpace;
                        if (((AbstractXSystem)system).SpaceThresholdUnit == SpaceThresholdUnit.MB)
                        {
                            size = spaceInfo.TotalFreeSpace - ((AbstractXSystem)system).SpaceThreshold * 1024 * 1024;
                        }
                        else if (((AbstractXSystem)system).SpaceThresholdUnit == SpaceThresholdUnit.PERCENT)
                        {
                            size = spaceInfo.TotalFreeSpace - (ulong)(spaceInfo.TotalSpace * (((AbstractXSystem)system).SpaceThreshold / 100.0));
                        }
                        if (avaliableSpace + size < long.MaxValue - 1)
                        {
                            avaliableSpace += size;
                        }
                        freeSpaceList.Add(spaceInfo.TotalFreeSpace);
                    }
                }
            }
            logger.Info("get use able space for this library:" + avaliableSpace);
            return avaliableSpace;
        }

        public override Int64 GetDirectorySize(StorageInfo info)
        {
            var result = 0L;
            foreach (var subSystem in subSystems)
            {
                if (subSystem.DirectoryExists(info))
                    result = subSystem.GetDirectorySize(info);
            }
            return result;
        }

        private ulong totalSpace = 0;
        public override ulong TotalSpace
        {
            get
            {
                if (totalSpace <= 0)
                {
                    foreach (IXSystem system in subSystems)
                    {
                        if (!(system is IXSpaceInfo spaceInfo))
                        {
                            logger.Warn("The system does not support space related properties");
                            continue;
                        }
                        totalSpace += spaceInfo.TotalSpace;
                    }
                }
                return totalSpace;
            }
        }

        private ulong totalUsedSpace = 0;
        public override ulong TotalUsedSpace
        {
            get
            {
                totalUsedSpace = 0;
                foreach (IXSystem system in SubSystems)
                {
                    if (!(system is IXSpaceInfo spaceInfo))
                    {
                        logger.Warn("The system does not support space related properties");
                        continue;
                    }
                    totalUsedSpace += spaceInfo.TotalUsedSpace;
                }
                return totalUsedSpace;
            }
        }

        private ulong totalFreeSpace = 0;
        public override ulong TotalFreeSpace
        {
            get
            {
                totalFreeSpace = 0;
                foreach (IXSystem system in SubSystems)
                {
                    if (!(system is IXSpaceInfo spaceInfo))
                    {
                        logger.Warn("The system does not support space related properties");
                        continue;
                    }
                    totalFreeSpace += spaceInfo.TotalFreeSpace;
                }
                return totalFreeSpace;
            }
        }
        public override ulong AvailableSpace
        {
            get
            {
                return GetAvaliableSpace();
            }
        }
        public override bool IsSupportAutoChangeDataBlock { get { return (workingSystem as AbstractXSystem)?.IsSupportAutoChangeDataBlock == true; } }
        public override bool IsSupportAutoDeletion { get { return workingSystem.IsSupportAutoDeletion; } }
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
        public override Predicate<IXSystemCommon> FindCondition
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

        public List<IXSystemCommon> SubSystems
        {
            get
            {
                return subSystems;
            }
        }

        public static readonly Predicate<IXSystemCommon> Basic = delegate(IXSystemCommon s)
        {
            if (s.SystemHealth == XSystemHealth.Unknown)
            {
                s.Open();
                s.Validate();
            }
            if (s.SystemHealth >= XSystemHealth.AvailableAndNotFull)
            {
                return true;
            }
            return false;
        };
        public static readonly Predicate<IXSystemCommon> WriteNotFull = delegate(IXSystemCommon s)
        {
            if (s.SystemHealth == XSystemHealth.Unknown)
            {
                s.Open();
                s.Validate();
            }
            if (s.SystemHealth >= XSystemHealth.AvailableAndNotFull)
            {
                return true;
            }
            return false;
        };

        public override void MergeStorageInfo<T>(List<T> ts, StorageResult rs, System.Reflection.PropertyInfo p)
        {
            EnsureValidSystem(XSystemHealth.Available);
            //这里暂时有bug 
            workingSystem.MergeStorageInfo<T>(ts, rs, p);
        }
        public void AddVIM(string xriStr, IVIM vim)
        {
            this.subSystems.Add(vim.CreateSystem(xriStr, this));
        }

        public override bool IsCacheSystem
        {
            get
            {
                EnsureValidSystem(XSystemHealth.Available);
                return workingSystem.IsCacheSystem;
            }
        }

        public override bool IsDirectSystem
        {
            get
            {
                EnsureValidSystem(XSystemHealth.Available);
                return workingSystem.IsDirectSystem;
            }
        }


        public override string SystemLocation
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
            EnsureValidSystem(XSystemHealth.Available);
            return null;
        }

        public StorageOpenValidResult Open(FeatureCustomized featureCustomized)
        {
            SetFeatureCustomized(featureCustomized);
            return Open();
        }
        protected override void SetFeatureCustomized(FeatureCustomized featureCustomized)
        {
            this.FeatureCustomized = featureCustomized;
        }
        public override List<string> GetUsedSystemsDuringWritten()
        {
            List<string> descriptions = new List<string>();
            foreach(IXSystem sys in subSystems)
            {
                if ((sys as AbstractXSystem).Written)
                {
                    descriptions.Add(sys.Properties[SystemPropertyKeys.SystemDescriptionKey] as string);
                }
            }
            return descriptions;
        }

        public override string Type
        {
            get
            {
                return this.workingSystem.Type;
            }
            set
            {
              
            }
        }

        public override List<string> GetUsedSystemsDuringDeletion()
        {
            List<string> descriptions = new List<string>();
            foreach (IXSystem sys in subSystems)
            {
                //retention job [column=physical device] 把成功执行删除的和没有成功的信息都显示出来。
                descriptions.Add(sys.Properties[SystemPropertyKeys.SystemDescriptionKey] as string);
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

        //public override FileBlockType SupportedFileType
        //{
        //    get
        //    {
        //        EnsureValidSystem(XSystemHealth.Available);
        //        return (workingSystem as AbstractXSystem)?.SupportedFileType ?? FileBlockType.SingleInstanceLevel_Block;
        //    }
        //}


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
                        catch (Exception t)
                        {
                            logger.Error(t.Message, t);
                        }
                        if (!fileExist)
                        {
                            foreach (IXSystemCommon subSystem in subSystems)
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
                                catch (Exception t)
                                {
                                    logger.Error(t.Message, t);
                                }
                            }
                        }
                    }
                    if(workingSystem == null || workingSystem.SystemHealth < XSystemHealth.Available)
                    {
                        throw new XSystemException("Cannot find any available system to read.");
                    }
                    return workingSystem.OpenStream(info, fileMode);
                case FileMode.Append:
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                    {
                        if (!(workingSystem is IXSpaceInfo spaceInfo))
                        {
                            logger.Warn("The system does not support space related properties");
                            break;
                        }
                        for (int i = 0; (spaceInfo.IsFull || spaceInfo.TotalFreeSpace - (ulong)info.Length < 0) && i < subSystems.Count; i++)
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
                            //if (workingSystem.SystemHealth < XSystemHealth.AvailableAndNotFull)
                            //{
                            //    workingSystem.Validate();
                            //}

                            if (!(workingSystem is IXSpaceInfo newSpaceInfo))
                            {
                                logger.Warn("The system does not support space related properties");
                                break;
                            }
                            if (workingSystem.SystemHealth >= XSystemHealth.AvailableAndNotFull)
                            {
                                if (newSpaceInfo.IsFull || (long)newSpaceInfo.TotalFreeSpace - info.Length < 0)
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
                            throw new NotEnoughFreeSpaceException("There is no enough space on the physical devices");
                        }

                        if (workingSystem == null || workingSystem.SystemHealth < XSystemHealth.AvailableAndNotFull)
                        {
                            throw new XSystemException("Cannot find any available device");
                        }
                        this.Written = true;
                        break;
                    }
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
                    logger.Error("open stream for file {0} failed:{1}", info.HighPlusLowName, ex.Message, ex);
                    logger.Info("we will try skip to another device");
                    workingSystem.SystemHealth = XSystemHealth.Unaccessable;
                    EnsureValidSystem(XSystemHealth.AvailableAndNotFull);
                    logger.Info("device skip succeed");
                }
            }
        }

        public override StorageResult CommitStream(Stream commitStream, StorageInfo info)
        {
            bool isFull = false;

            if (workingSystem is IXSpaceInfo spaceInfo)
            {
                for (int i = 0; (isFull = spaceInfo.IsFull || (long)spaceInfo.TotalFreeSpace - info.Length < 0) && i < subSystems.Count; i++)
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
                    //if (workingSystem.SystemHealth < XSystemHealth.AvailableAndNotFull)
                    //{
                    //    workingSystem.Validate();
                    //}
                    if (!(workingSystem is IXSpaceInfo newSpaceInfo))
                    {
                        isFull = false;
                        logger.Warn("The system does not support space related properties");
                        break;
                    }
                    if (workingSystem.SystemHealth >= XSystemHealth.AvailableAndNotFull)
                    {
                        if ((isFull = newSpaceInfo.IsFull) || ((long)newSpaceInfo.TotalFreeSpace - info.Length < 0))
                        {
                            workingSystem.SystemHealth = XSystemHealth.Available;
                        }
                        else
                        {
                            break;
                        }
                    }

                }
            }
            else
            {
                logger.Warn("The system does not support space related properties");
            }

            if (isFull)
            {
                workingSystem.SystemHealth = XSystemHealth.Available;
            }
            else
            {
                workingSystem.SystemHealth = XSystemHealth.AvailableAndNotFull;
            }

            if (workingSystem != null && workingSystem.SystemHealth == XSystemHealth.Available)
            {
                throw new NotEnoughFreeSpaceException("There is no enough space on the physical devices");
            }

            if (workingSystem == null || workingSystem.SystemHealth < XSystemHealth.AvailableAndNotFull)
            {
                throw new XSystemException("Cannot find any available device");
            }
            this.Written = true;

            //return workingSystem.CommitStream(commitStream, info);
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
            EnsureValidSystem(XSystemHealth.AvailableAndNotFull);
            
            if (workingSystem == null || workingSystem.SystemHealth < XSystemHealth.Available)
            {
                throw new XSystemException("Cannot find any available system to read.");
            }
            if (workingSystem.IsDirectSystem)
            {
                return workingSystem.CopyFile(sourceFileInfo, targetFileInfo, isOverWrite);
            }
            else
            {
                if ((workingSystem as AbstractXSystem).IsSimulReadWriteSystem)
                {
                    return workingSystem.CopyFile(sourceFileInfo, targetFileInfo, isOverWrite);
                }
                if (workingSystem is Cloud.Azure.AzureSystem && sourceFileInfo.FileTierType == AccessTierType.Archive)
                {
                    return workingSystem.CopyFile(sourceFileInfo, targetFileInfo, isOverWrite);
                }
                StorageCopyResult rs = new StorageCopyResult();
                try
                {
                    byte[] cacheBuffer = new byte[64 * 1024];
                    targetFileInfo.Length = workingSystem.OpenFile(sourceFileInfo).FileSize;
                    sourceFileInfo.Length = targetFileInfo.Length;
                    using (XStream cacheStream = workingSystem.OpenStream(sourceFileInfo, FileMode.Open))
                    {
                        using (XStream uploader = workingSystem.OpenStream(targetFileInfo, FileMode.Create))
                        {
                            int readLen = 0;
                            while ((readLen = cacheStream.Read(cacheBuffer, 0, cacheBuffer.Length)) > 0)
                            {
                                uploader.Write(cacheBuffer, 0, readLen);
                            }
                            uploader.Commit(true);
                        }
                    }
                    rs.IsCopyed = true;
                 }
                catch (Exception e)
                {
                    rs.Message = e.Message;
                    rs.IsCopyed = false;
                    logger.Error("copy file failed:" + e.ToString());
                }
                return rs;
            }
        }

        public override StorageCopyResult CopyFile(StorageInfo srcFile, IXSystemCommon destSystem, StorageInfo destFile, bool isOverWrite)
        {
            return base.CopyFile(srcFile, destSystem, destFile, isOverWrite);
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            Exception e = null;
            StorageDeleteResult result = new StorageDeleteResult();
            result.IsDeleted = true;
            foreach (IXSystemCommon subSystem in subSystems)
            {
                try
                {
                    StorageDeleteResult tempResult = subSystem.DeleteDirectory(info);
                    result.DeletedFileSize += tempResult.DeletedFileSize;
                    result.IsDeleted = result.IsDeleted & tempResult.IsDeleted;
                    result.Message += tempResult.Message;
                }
                catch (Exception ex)
                {
                    logger.Error("Delete directory failed:" + ex.ToString());
                    e = ex;
                }
            }
            if (e != null)
            {
                throw e;
            }
            return result;
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            StorageDeleteResult rs = new StorageDeleteResult();
            rs.IsDeleted = true;
            foreach (IXSystemCommon subSystem in subSystems)
            {
                bool fileExist = subSystem.FileExists(info);
                if (fileExist)
                {
                    StorageDeleteResult s = subSystem.DeleteFile(info);
                    rs.DeletedFileSize += s.DeletedFileSize;
                    rs.IsDeleted = rs.IsDeleted & s.IsDeleted;
                    rs.Message += s.Message;
                }
            }
            return rs;
        }

        public override bool DirectoryExists(StorageInfo info)
        {
            bool rs = false;
            Exception exception = null;
            foreach (IXSystemCommon subSystem in subSystems)
            {
                try
                {
                    rs = subSystem.DirectoryExists(info);
                }
                catch (System.Exception ex)
                {
                    exception = ex;
                    Trace.TraceError(ex.ToString());
                }
                if (rs)
                {
                    return rs;
                }
            }
            if (exception != null)
            {
                throw exception;
            }
            return rs;
        }

        public override bool FileExists(StorageInfo info)
        {
            bool rs = false;
            Exception exception = null;
            foreach (IXSystemCommon subSystem in subSystems)
            {
                try
                {
                    rs = subSystem.FileExists(info);
                }
                catch (System.Exception ex)
                {
                    exception = ex;
                    logger.Warn(subSystem.GetType() + " :" + ex.ToString());
                }
                if (rs)
                {
                    this.workingSystem = subSystem;
                    return rs;
                }
            }
            if (exception != null)
            {
                throw exception;
            }
            return rs;
        }

        public override StorageOpenValidResult Validate()
        {
            StorageOpenValidResult rs = new StorageOpenValidResult();
            foreach (IXSystemCommon sys in subSystems)
            {
                if (sys.SystemHealth == XSystemHealth.Unknown)
                {
                    sys.Open();
                }
                StorageOpenValidResult sr = sys.Validate();
                if (workingSystem.SystemHealth < sys.SystemHealth)
                {
                    workingSystem = sys;
                }
                rs.SubResult.Add(sr);
            }
            rs.SystemHealth = workingSystem.SystemHealth;
            return rs;
        }

        public override void Close()
        {
            if (this.subSystems != null && this.subSystems.Count > 0)
            {
                foreach (IXSystem sys in this.subSystems)
                {
                    sys.Close();
                }
            }
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            EnsureValidSystem(XSystemHealth.Available);
            return workingSystem.OpenDirectory(dirInfo, mode);
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
            catch (System.Exception ex)
            {
                exception = ex;
                Trace.TraceError(ex.ToString());
            }
            foreach (IXSystemCommon subSystem in subSystems)
            {
                try
                {
                    if (subSystem.FileExists(fileInfo))
                    {
                        workingSystem = subSystem;
                        return workingSystem.OpenFile(fileInfo);
                    }
                }
                catch (System.Exception ex)
                {
                    exception = ex;
                    Trace.TraceError(ex.ToString());
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
            Dictionary<string, XDirectoryInfo> tempResult = new Dictionary<string, XDirectoryInfo>();
            foreach (IXSystemCommon subSystem in subSystems)
            {
                if (subSystem.DirectoryExists(dirInfo))
                {
                    workingSystem = subSystem;
                    List<XDirectoryInfo> xDirInfo = subSystem.ListDirectories(dirInfo);
                    if (xDirInfo != null && xDirInfo.Count != 0) 
                    {
                        foreach (XDirectoryInfo info in xDirInfo)
                        {
                            tempResult[info.HighName + info.Name] = info;
                        }
                    }
                }
            }
            return new List<XDirectoryInfo>(tempResult.Values);
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            EnsureValidSystem(XSystemHealth.Available);
            Dictionary<string, XFileInfo> tempResult = new Dictionary<string, XFileInfo>();
            foreach (IXSystemCommon subSystem in subSystems)
            {
                if (subSystem.DirectoryExists(dirInfo))
                {
                    workingSystem = subSystem;
                    List<XFileInfo> xFileInfo = subSystem.ListFiles(dirInfo);
                    if (xFileInfo != null && xFileInfo.Count != 0) 
                    {
                        foreach (XFileInfo info in xFileInfo)
                        {
                            tempResult[info.HighName + info.LowName] = info;
                        }
                    }
                }
            }
            return new List<XFileInfo>(tempResult.Values);
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
        private IXSystemCommon GetNextValidSystem(XSystemHealth state, FindMethod post)
        {
            IXSystemCommon sysObj = null;
            this.FeatureCustomized = this.FeatureCustomized ?? FeatureCustomized.Default;
            switch (post)
            {
                case FindMethod.Beginning:
                    sysObj = subSystems.Find(delegate(IXSystemCommon s)
                    {
                        if (s.SystemHealth == XSystemHealth.Unknown)
                        {
                            s.Open(this.FeatureCustomized);
                            s.Validate();
                        }
                        //else if (s.SystemHealth < state && s.SystemHealth > XSystemHealth.Unknown)
                        //{
                        //    s.Validate();
                        //}
                        if (s.SystemHealth >= state)
                        {
                            return true;
                        }
                        else if (MaxSystemHealth < s.SystemHealth)
                        {
                            MaxSystemHealth = s.SystemHealth;
                        }
                        return false;
                    });
                    break;
                case FindMethod.Continue:
                    sysObj = subSystems.FindLast(delegate(IXSystemCommon s)
                    {
                        if (s.SystemHealth == XSystemHealth.Unknown)
                        {
                            s.Open(this.FeatureCustomized);
                            s.Validate();
                        }
                        else if (s.SystemHealth < state && s.SystemHealth > XSystemHealth.Unknown)
                        {
                            s.Validate();
                        }

                        if (s.SystemHealth >= state)
                        {
                            return true;
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
                return (IXSystemCommon)sysObj;
            }
            throw new Exception("Can not find valid storage system");
        }

        public XSystemHealth GetMaxSystemHealth()
        {
            XSystemHealth tempSystemHealth = MaxSystemHealth;
            MaxSystemHealth = XSystemHealth.Unknown;
            return tempSystemHealth;
        }

        public override StorageChangeResult ChangeFileTier(StorageInfo info)
        {
            StorageChangeResult result = new StorageChangeResult();
            Exception exception = null;
            foreach (IXSystemCommon subSystem in subSystems)
            {
                try
                {
                    result = subSystem.ChangeFileTier(info);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    logger.Warn(subSystem.GetType() + " :" + ex.ToString());
                }
                if (result.IsChanged)
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
            /// 继续轮询剩余的XSystem
            /// </summary>
            Continue = 1,
            /// <summary>
            /// 按照上层所传的条件去查找
            /// </summary>
            Custom=2
        }
    }
}
