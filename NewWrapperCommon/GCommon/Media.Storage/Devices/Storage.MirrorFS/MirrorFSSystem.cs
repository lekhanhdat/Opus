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

namespace AvePoint.Media.Storage.MirrorFS
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Storage.Util; 
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2013/2/22",
    "yanxin.fu@avepoint.com",
    "shouqiang.liu@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_3, CodeReviewConstants.CHECK_LIST_ID_CO_12 },
    null,
    true)]
    #endregion
    class MirrorFSSystem : AbstractXSystem
    {

        private Dictionary<int, List<IXSystem>> innerSystems = new Dictionary<int, List<IXSystem>>();
        public Dictionary<int, List<IXSystem>> InnerSystems { get { return this.innerSystems; } }
        public SyncMode SyncMode { get; set; }
        private static StorageLogger logger = new StorageLogger(typeof(MirrorFSSystem));
        public static readonly string stubRootFolder = @"DocAveSyncFileStubs\DataManager";
        private static readonly string newStubRootFolder = @"NewSyncFileStub";
        public MirrorFSSystem(string xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            //mirrorFSvim?id=logicalid&syncmode=0/1&physicalid=phyiscalconnectionString
            XriObject = XRI.ValueOf(xriString);
            this.SyncMode = Storage.SyncMode.ASynchronous;
            foreach (KeyValuePair<string, string> entity in XriObject.Params)
            {
                if (entity.Key.Equals("id", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                else if (entity.Key.Equals(XRIParameterKeys.SyncModeKey, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.SyncMode = (SyncMode)int.Parse(XriObject.Params[XRIParameterKeys.SyncModeKey]);
                }
                else
                {
                    IXSystem sys = XFactory.InstanceSystem(entity.Value);
                    int groupNumber = GetGroupNumber(entity.Value);
                    if (innerSystems.ContainsKey(groupNumber))
                    {
                        innerSystems[groupNumber].Add(sys);
                    }
                    else
                    {
                        List<IXSystem> newGroup = new List<IXSystem>();
                        newGroup.Add(sys);
                        innerSystems[groupNumber] = newGroup;
                    }
                }
            }
            foreach (var item in innerSystems)
            {
                logger.Info("Preferred Storage Group :{0} Storage Group", this.ConvertIntToOrder(item.Key + 1));
                break;
            }
            this.Open();
        }

        private Dictionary<int, List<IXSystem>> InstanceNewRAIDSystem()
        {
            Dictionary<int, List<IXSystem>> newInstance = new Dictionary<int, List<IXSystem>>();
            foreach (KeyValuePair<string, string> entity in XriObject.Params)
            {
                if (entity.Key.Equals("id", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                else if (entity.Key.Equals(XRIParameterKeys.SyncModeKey, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                else
                {
                    IXSystem sys = XFactory.InstanceSystem(entity.Value);
                    sys.Open();
                    int groupNumber = GetGroupNumber(entity.Value);
                    if (newInstance.ContainsKey(groupNumber))
                    {
                        newInstance[groupNumber].Add(sys);
                    }
                    else
                    {
                        List<IXSystem> newGroup = new List<IXSystem>();
                        newGroup.Add(sys);
                        newInstance[groupNumber] = newGroup;
                    }
                }
            }
            return newInstance;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "groupnum")]
        private int GetGroupNumber(string connectString)
        {
            Regex r = new Regex("groupnum=[^&]+");
            Match m = r.Match(connectString);
            if (m.Success)
            {
                return Convert.ToInt32(m.Groups[0].Value.Split('=')[1]);
            }
            else
            {
                throw new Exception();
            }
        }

        public override StorageOpenValidResult Open()
        {
            base.Open();
            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem system in group)
                {
                    try
                    {
                        system.Open();
                        if (system.SystemHealth > this.SystemHealth)
                        {
                            this.SystemHealth = system.SystemHealth;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        logger.Info("open sub-system failed:" + system.SystemName, ex.Message);
                        system.SystemHealth = XSystemHealth.Unaccessable;
                    }
                }
            }
            SetSystemDescription();
            this.IsDirectSystem = (innerSystems[0])[0].IsDirectSystem;
            return new StorageOpenValidResult();
        }

        /// <summary>
        /// totalSpace
        /// </summary>
        private ulong totalSpace;
        public override ulong TotalSpace
        {
            get
            {
                return totalSpace;
            }
        }

        /// <summary>
        /// totalFreeSpace
        /// </summary>
        private ulong totalFreeSpace;
        public override ulong TotalFreeSpace
        {
            get
            {
                return totalFreeSpace;
            }
        }

        /// <summary>
        /// totalUsedSpace
        /// </summary>
        private ulong totalUsedSpace;
        public override ulong TotalUsedSpace
        {
            get
            {
                return totalUsedSpace;
            }
        }

        /// <summary>
        /// SetSystemDescription
        /// </summary>
        protected override void SetSystemDescription()
        {
            StringBuilder desc = new StringBuilder();
            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem sys in group)
                {
                    desc.Append(sys.Properties[SystemPropertyKeys.SystemDescriptionKey]);
                    desc.Append("\r\n");
                }
            }
            Properties[SystemPropertyKeys.SystemDescriptionKey] = desc.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override StorageOpenValidResult Validate()
        {
            List<StorageOpenValidResult> rss = new List<StorageOpenValidResult>();
            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem sys in group)
                {
                    sys.Open();
                    rss.Add(sys.Validate());
                }
            }
            StorageOpenValidResult result = GetValidResultFromAllResultsOrderByStorage(rss);
            return result;
        }

        /// <summary>
        /// GetValidResultFromAllResultsOrderByStorage
        /// </summary>
        /// <param name="rss"></param>
        /// <returns></returns>
        private StorageOpenValidResult GetValidResultFromAllResultsOrderByStorage(List<StorageOpenValidResult> rss)
        {
            StorageOpenValidResult result = new StorageOpenValidResult();
            ulong spaceFreeSize = 0;
            result.SystemHealth = XSystemHealth.Unknown;
            foreach (StorageOpenValidResult sovr in rss)
            {
                if (sovr.SystemHealth > result.SystemHealth)
                {
                    if (sovr.TotalFreeSpace > spaceFreeSize)
                    {
                        spaceFreeSize = sovr.TotalFreeSpace;
                        this.SystemHealth = sovr.SystemHealth;
                        result.SystemHealth = sovr.SystemHealth;
                        result.TotalFreeSpace = sovr.TotalFreeSpace;
                        result.TotalSpace = sovr.TotalSpace;
                        result.TotalUsedSpace = sovr.TotalUsedSpace;
                        this.totalFreeSpace = sovr.TotalFreeSpace;
                        this.totalSpace = sovr.TotalSpace;
                        this.totalUsedSpace = sovr.TotalUsedSpace;
                    }
                }
                if (sovr.SystemHealth < XSystemHealth.Available)
                {
                    result.IsAllDeviceAvailable = XSystemValidateStatus.UnAvailableDeviceExist;
                }
            }
            return result;
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            XDirectoryInfo irectoryInfo = null;
            bool isFindOut = false;

            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem sys in group)
                {
                    try
                    {
                        IXSystem sourceSystem = sys;
                        irectoryInfo = sourceSystem.OpenDirectory(dirInfo, mode);
                        if (irectoryInfo != null)
                        {
                            isFindOut = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.ToString());
                    }
                }
                if (isFindOut)
                {
                    break;
                }
            }
            return irectoryInfo;
        }
        /// <summary>
        /// OpenFile
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            StorageInfo info = new StorageInfo();
            XFileInfo xfileInfo = null;
            XFileInfo tempFileInfo = null;
            info.HighName = stubRootFolder;
            bool isFindOut = false;
            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem sys in group)
                {
                    try
                    {
                        IXSystem sourceSystem = sys;
                        tempFileInfo = sourceSystem.OpenFile(fileInfo);
                        if (tempFileInfo != null && (xfileInfo == null || (xfileInfo != null && tempFileInfo.FileSize > xfileInfo.FileSize)))
                        {
                            xfileInfo = tempFileInfo;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.ToString());
                    }
                }
                if (isFindOut)
                {
                    break;
                }
            }
            return xfileInfo;
        }

        /// <summary>
        /// OpenStream
        /// </summary>
        /// <param name="info"></param>
        /// <param name="fileMode"></param>
        /// <returns></returns>
        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew || fileMode == FileMode.OpenOrCreate)
            {
                this.Written = true;
            }
            MirrorFSStream stream = new MirrorFSStream(info, fileMode, this);
            stream.InitStream();
            return stream;
        }

        /// <summary>
        /// CreatStubFile
        /// </summary>
        /// <param name="systems"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool CreatStubFile(List<IXSystem> systems, StorageInfo info)
        {
            StorageInfo sInfo = new StorageInfo();
            sInfo.HighName = PathUtil.CombinePath(PathUtil.CombinePath(stubRootFolder, this.SystemID), newStubRootFolder);
            //sInfo.LowName = AveConverter.EncodeSpecialChar(info.HighPlusLowName);
            sInfo.LowName = HashCodeHelper.ToMD5HashCode(info.HighPlusLowName);
            bool hasSucceed = false;
            foreach (IXSystem system in systems)
            {
                try
                {
                    using (XStream stream = system.OpenStream(sInfo, FileMode.OpenOrCreate))
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.WriteLine(AveConverter.EncodeSpecialChar(info.HighPlusLowName));
                        }
                        stream.Commit();
                    }
                    hasSucceed = true;
                    break;
                }
                catch (Exception ex)
                {
                    logger.Warn("create stub {0} failed:{1}", sInfo.HighPlusLowName, ex);
                    continue;
                }
            }
            if (!hasSucceed)
            {
                throw new Exception(string.Format("create stub {0} failed", sInfo.HighPlusLowName));
            }
            return true;
        }

        /// <summary>
        /// CommitStreamSyncMode
        /// </summary>
        /// <param name="commitStream"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public StorageResult CommitStreamSyncMode(Stream commitStream, StorageInfo info)
        {
            StorageResult result = new StorageResult();
            int succeedKey = 0;
            IXSystem succeedSystem = null;
            foreach (int key in innerSystems.Keys)
            {
                foreach (IXSystem system in innerSystems[key])
                {
                    if (system.SystemHealth >= XSystemHealth.Available && !system.IsFull)
                    {
                        StorageResult sr = CommitStream(system, commitStream, info);
                        if (sr.IsCommited)
                        {
                            if (CreatStubFile(new List<IXSystem>() { system }, info))
                            {
                                result = sr;
                                succeedKey = key;
                                succeedSystem = system;
                                break;
                            }
                        }
                    }

                }
                if (result.IsCommited)
                {
                    break;
                }
            }
            if (!result.IsCommited)
            {
                throw new Exception("commit stream failed:" + info.HighPlusLowName);
            }

            StartSyncRAIDDevice(succeedKey, info, succeedSystem);
            result.IsCommited = true;
            return result;
        }

        public void StartSyncRAIDDevice(int succeedKey, StorageInfo info, IXSystem succeedSystem)
        {
            try
            {
                if (!MirrorFSSynchronizer.IsAlive())
                {
                    MirrorFSSynchronizer.StartSynchronize();
                }
                Dictionary<int, List<IXSystem>> systems = InstanceNewRAIDSystem();
                List<IXSystem> succeedSys = systems[succeedKey];
                systems.Remove(succeedKey);
                IXSystem succeedNewSystem = succeedSys.Find(C => C.SystemID.Equals(succeedSystem.SystemID));
                MirrorFSSynchronizer.EnQueue(new MirrorFSInfo(this.SystemID, succeedNewSystem, systems, info));
            }
            catch (Exception ex)
            {
                logger.Error("Start Synchronize exception: " + ex);
                throw;
            }
        }

        /// <summary>
        /// CommitStreamASyncMode
        /// </summary>
        /// <param name="commitStream"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public StorageResult CommitStreamASyncMode(Stream commitStream, StorageInfo info)
        {
            StorageResult result = new StorageResult();
            bool hasSucceed = false;
            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem system in group)
                {
                    try
                    {
                        if (system.SystemHealth >= XSystemHealth.Available && !system.IsFull)
                        {
                            StorageResult sr = CommitStream(system, commitStream, info);
                            if (sr.IsCommited)
                            {
                                CreatStubFile(new List<IXSystem>() { system }, info);
                                hasSucceed = true;
                                result = sr;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("commit file {0} error:{1}", info.HighPlusLowName, ex.Message, ex);
                        continue;
                    }
                }
                if (hasSucceed)
                {
                    break;
                }
            }
            if (!hasSucceed)
            {
                throw new Exception("upload file " + info.HighPlusLowName + " failed");
            }
            return result;
        }

        /// <summary>
        /// CommitStreamSyncModeOrASyncMode(Stream commitStream, StorageInfo info)
        /// </summary>
        /// <param name="commitStream"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public StorageResult CommitStreamSyncModeOrASyncMode(Stream commitStream, StorageInfo info)
        {
            StorageResult sr = new StorageResult();
            if (SyncMode == Storage.SyncMode.Synchronous)
            {
                sr = CommitStreamSyncMode(commitStream, info);
            }
            else
            {
                sr = CommitStreamASyncMode(commitStream, info);
            }
            return sr;
        }

        /// <summary>
        /// CommitStream
        /// </summary>
        /// <param name="commitStream"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public override StorageResult CommitStream(Stream commitStream, StorageInfo info)
        {
            CheckState();
            logger.Debug("Commit file:" + info.HighPlusLowName);
            StorageResult sr = new StorageResult();
            try
            {
                sr = CommitStreamSyncModeOrASyncMode(commitStream, info);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
            //finally
            //{
            //    if (commitStream != null)
            //    {
            //        commitStream.Close();
            //    }
            //}
            //we need return the logical id for raid device
            return sr;
        }

        /// <summary>
        /// CommitStream(IXSystem system, Stream commitStream, StorageInfo info, int startPos = 0)
        /// </summary>
        /// <param name="system"></param>
        /// <param name="commitStream"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public StorageResult CommitStream(IXSystem system, Stream commitStream, StorageInfo info)
        {
            StorageResult sr = new StorageResult();
            byte[] buffer = new byte[64 * 1024];
            int readLen = 0;
            try
            {
                commitStream.Position = info.Offset;
                using (XStream stream = system.OpenStream(info, FileMode.Create))
                {
                    while ((readLen = commitStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        stream.Write(buffer, 0, readLen);
                    }
                    sr = stream.Commit();
                    sr.URI = stream.GetURI();
                    sr.URI.SysId = this.SystemID + "&" + system.SystemID;
                    sr.IsCommited = true;
                }
                this.Written = true;
            }
            catch (Exception ex)
            {
                logger.Error("upload file {0}error:{1}", info.HighPlusLowName, ex.Message, ex);
                sr.IsCommited = false;
                sr.Message = ex.ToString();
                try
                {
                    system.DeleteFile(info);
                }
                catch (Exception e)
                {
                    logger.Warn("delete dirty file {0} failed:", info.HighPlusLowName, e.Message, e);
                }
            }
            return sr;
        }

        public override void Close()
        {
            if (innerSystems != null && innerSystems.Count > 0)
            {
                foreach (List<IXSystem> group in innerSystems.Values)
                {
                    foreach (IXSystem sys in group)
                    {
                        sys.Close();
                    }
                }
            }
            while (MirrorFSSynchronizer.IsAlive() && MirrorFSSynchronizer.isSynchronizing())
            {
                logger.Info("The raid1 data is still synchronizing...");
                Thread.Sleep(2000);
            }
        }

        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            StorageDeleteResult sdr = new StorageDeleteResult();
            List<StorageDeleteResult> sdrs = new List<StorageDeleteResult>();
            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem sys in group)
                {
                    StorageInfo stubFileInfo = info.Clone();
                    stubFileInfo.HighName = PathUtil.CombinePath(stubRootFolder, this.SystemID);
                    stubFileInfo.LowName = AveConverter.EncodeSpecialChar(info.HighPlusLowName);
                    var newStubInfo = new StorageInfo();
                    newStubInfo.HighName = PathUtil.CombinePath(stubFileInfo.HighName, newStubRootFolder);
                    newStubInfo.LowName = HashCodeHelper.ToMD5HashCode(info.HighPlusLowName);
                    if (sys.FileExists(stubFileInfo))
                    {
                        sys.DeleteFile(stubFileInfo);
                    }
                    else if (sys.FileExists(newStubInfo))
                    {
                        sys.DeleteFile(newStubInfo);
                    }
                    else
                    {
                        //for old data(before 6.3)
                        stubFileInfo.HighName = stubRootFolder;
                        if (sys.FileExists(stubFileInfo))
                        {
                            sys.DeleteFile(stubFileInfo);
                        }
                    }
                    sdrs.Add(sys.DeleteFile(info));
                    //break;
                }
            }
            foreach (StorageDeleteResult s in sdrs)
            {
                if (!s.IsDeleted)
                {
                    sdr.IsDeleted = false;
                    break;
                }
                sdr.IsDeleted = true;
                sdr.DeletedFileSize = s.DeletedFileSize;
            }
            //标记执行过删除
            Deletion = true;
            return sdr;
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            StorageDeleteResult sdr = new StorageDeleteResult();
            List<StorageDeleteResult> sdrs = new List<StorageDeleteResult>();
            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem sys in group)
                {
                    sdrs.Add(DeleteDirectory(sys, info, null));
                }
            }
            foreach (StorageDeleteResult s in sdrs)
            {
                if (!s.IsDeleted)
                {
                    sdr.IsDeleted = false;
                    break;
                }
                sdr.IsDeleted = true;
                sdr.DeletedFileSize = s.DeletedFileSize;
            }
            //标记执行过删除
            Deletion = true;
            return sdr;
        }

        private StorageDeleteResult DeleteDirectory(IXSystem system, StorageInfo info, StorageDeleteResult sr)
        {
            if (sr == null)
            {
                sr = new StorageDeleteResult();
            }
            if (system.DirectoryExists(info))
            {
                StorageListResult listSR = system.ListSubDirectoriesAndFiles(info);
                if (listSR.Files != null && listSR.Files.Count > 0)
                {
                    foreach (XFileInfo file in listSR.Files)
                    {
                        StorageInfo tmpInfo = new StorageInfo();
                        tmpInfo.HighName = info.HighPlusLowName;
                        tmpInfo.LowName = file.Name;
                        long fileSize = (system.OpenFile(tmpInfo)).Length;
                        DeleteFile(tmpInfo);
                        sr.DeletedFileSize += fileSize;
                    }
                }
                if (listSR.SubDirs != null && listSR.SubDirs.Count > 0)
                {
                    foreach (XDirectoryInfo dir in listSR.SubDirs)
                    {
                        StorageInfo tmpInfo = new StorageInfo();
                        tmpInfo.HighName = PathUtil.CombinePath(info.HighPlusLowName, dir.Name);
                        DeleteDirectory(system, tmpInfo, sr);
                    }
                }
                system.DeleteDirectory(info);
            }
            return sr;
        }

        public override bool FileExists(StorageInfo info)
        {
            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem sys in group)
                {
                    if (sys.FileExists(info))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool DirectoryExists(StorageInfo info)
        {
            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem sys in group)
                {
                    if (sys.DirectoryExists(info))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            List<StorageCopyResult> scrs = new List<StorageCopyResult>();

            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem sys in group)
                {
                    scrs.Add(sys.CopyFile(sourceFileInfo, targetFileInfo, isOverWrite));
                }
            }
            StorageCopyResult result = new StorageCopyResult();
            foreach (StorageCopyResult scr in scrs)
            {
                if (!scr.IsCopyed)
                {
                    result.IsCopyed = false;
                    result.Message = "Copy failed, please take a look at the log for the detail.";
                    return result;
                }
                result.IsCopyed = true;
            }
            return result;
        }

        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            StorageMoveResult result = null;
            List<IXSystem> succeedSystemList = new List<IXSystem>();
            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem sys in group)
                {
                    StorageMoveResult sr = sys.MoveFile(sourceFileInfo, targetFileInfo, isOverWrite);
                    if (sr != null && sr.IsMoved)
                    {
                        succeedSystemList.Add(sys);
                        if (SyncMode == SyncMode.Synchronous)
                        {
                            CreatStubFile(succeedSystemList, sourceFileInfo);
                            result.IsMoved = true;
                            return result;
                        }
                    }
                }
            }
            if (succeedSystemList.Count == innerSystems.Count)
            {
                result.IsMoved = true;
            }
            else
            {
                result.IsMoved = false;
                result.Message = "Move failed, please take a look at the log for the detail.";
                return result;
            }
            return result;
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

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite)
        {
            throw new NotSupportedException();
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "nd")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "st")]
        private String ConvertIntToOrder(Int32 num)
        {
            switch (num)
            {
                case 1:
                    return "Primary";
                case 2:
                    return "Secondary";
                case 3:
                    return "3rd";
            }
            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return "The " + num + "th";
            }
            switch (num % 10)
            {
                case 1:
                    return "The " + num + "st";
                case 2:
                    return "The " + num + "nd";
                case 3:
                    return "The " + num + "rd";
                default:
                    return "The " + num + "th";
            }
        }
    }
}
