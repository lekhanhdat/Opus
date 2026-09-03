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

using System.Diagnostics.CodeAnalysis;
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.DeviceSynchronizer.DeviceSynchronizer.#GetGroupNumber(System.String)", MessageId = "groupnum")]
namespace AvePoint.Media.Storage.DeviceSynchronizer
{
    #region using directives
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using AvePoint.GCommon.Utility; 
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/5/16",
    "dapeng.zhang@avepoint.com",
    "shouqiang.liu@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
    null,
    true)]
    #endregion
    public class DeviceSynchronizer
    {
        private static SafeDictionary<string, List<string>> WorkingDeviceDic { get; set; }
        private static SafeDictionary<string, List<string>> WaitingDeviceDic { get; set; }
        private static SafeDictionary<string, Thread> threadPool;
        private static readonly string stubRootFolder = @"DocAveSyncFileStubs\DataManager";
        private static readonly string newStubRootFolder = @"NewSyncFileStub";
        private static readonly string threadName = "DeviceSynchronizer";
        private static readonly string LockFileName = "LockFile";
        private static StorageLogger logger = new StorageLogger(typeof(DeviceSynchronizer));
        private static object objLock = new object();
        private static int sleepTime = 1000 * 60 * 30;
        bool isFirstRun = true;
        public static void SetSyncInterval(int time)
        {
            sleepTime = time;
        }

        public static bool IsAlive()
        {
            if (threadPool != null && threadPool.ContainsKey(threadName) && threadPool[threadName].IsAlive)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void AddDevice(List<string> xri)
        {
            lock (objLock)
            {
                SafeDictionary<string, List<string>> deviceDic = GetDeviceFromXRI(xri);
                if (WaitingDeviceDic == null)
                {
                    WaitingDeviceDic = deviceDic;
                }
                else
                {
                    foreach (KeyValuePair<string, List<string>> device in deviceDic)
                    {
                        logger.Debug("add new device:" + device.Key);
                        WaitingDeviceDic[device.Key] = device.Value;
                    }
                }
            }
        }

        public static void RemoveDevice(string id)
        {
            lock (objLock)
            {
                if (WaitingDeviceDic != null && WaitingDeviceDic.ContainsKey(id))
                {
                    WaitingDeviceDic.Remove(id);
                    logger.Debug("remove waiting device:" + id);
                }
            }
        }

        public static void Clear()
        {
            lock (objLock)
            {
                if (WaitingDeviceDic != null)
                {
                    WaitingDeviceDic.Clear();
                }
            }
        }

        private static SafeDictionary<string, List<string>> GetDeviceFromXRI(List<string> xri)
        {
            logger.Info("begin GetDeviceFromXRI method XRI :" + xri[0].ToString());
            SafeDictionary<string, List<string>> deviceDic = new SafeDictionary<string, List<string>>();
            XRI XriObject = XRI.ValueOf(xri[0]);
            string logicalId = string.Empty;
            foreach (KeyValuePair<string, string> entity in XriObject.Params)
            {
                if (entity.Key.Equals("id", StringComparison.CurrentCultureIgnoreCase))
                {
                    logicalId = entity.Value;
                    deviceDic[logicalId] = new List<string>();
                    continue;
                }
                else if (entity.Key.Equals(XRIParameterKeys.SyncModeKey, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                else
                {
                    deviceDic[logicalId].Add(entity.Value);
                }
            }
            return deviceDic;
        }

        /// <summary>
        /// SynchronizeDevices
        /// </summary>
        public void SynchronizeDevices()
        {
            while (true)
            {
                try
                {
                    //clear working dic
                    lock (objLock)
                    {
                        if (WorkingDeviceDic != null)
                        {
                            WorkingDeviceDic.Clear();
                        }
                    }
                    //add new device
                    if (WaitingDeviceDic != null && WaitingDeviceDic.Count > 0)
                    {
                        if (WorkingDeviceDic == null)
                        {
                            WorkingDeviceDic = new SafeDictionary<string, System.Collections.Generic.List<string>>();
                        }
                        foreach (KeyValuePair<string, List<string>> device in WaitingDeviceDic)
                        {
                            WorkingDeviceDic[device.Key] = device.Value;
                        }
                    }

                    //sync working devices
                    if (WorkingDeviceDic != null && WorkingDeviceDic.Count > 0)
                    {
                        logger.Info("begin sync devices, count:" + WorkingDeviceDic.Count);
                        //遍历logical级别
                        foreach (string id in WorkingDeviceDic.Keys)
                        {
                            Dictionary<int, List<IXSystem>> innerSystems = new Dictionary<int, List<IXSystem>>();
                            foreach (string xri in WorkingDeviceDic[id])
                            {
                                IXSystem system = XFactory.InstanceSystem(xri);
                                system.Open();
                                system.Validate();
                                int groupNumber = GetGroupNumber(xri);
                                if (innerSystems.ContainsKey(groupNumber))
                                {
                                    innerSystems[groupNumber].Add(system);
                                }
                                else
                                {
                                    List<IXSystem> newGroup = new List<IXSystem>();
                                    newGroup.Add(system);
                                    innerSystems[groupNumber] = newGroup;
                                }
                            }
                            //同步单独的logical(新数据格式from 6.3)
                            StorageInfo info = new StorageInfo();
                            info.HighName = PathUtil.CombinePath(stubRootFolder, id);
                            logger.Debug("begin sync logical level, logicalId:" + id);
                            SynchronizeDevice(info, id, innerSystems);

                            if (isFirstRun)
                            {
                                //同步单独的logical(旧数据格式to 6.2)
                                StorageInfo oldInfo = new StorageInfo();
                                oldInfo.HighName = stubRootFolder;
                                SynchronizeDevice(oldInfo, id, innerSystems);
                            }
                        }
                    }
                    isFirstRun = false;
                    Thread.Sleep(sleepTime);
                }
                catch (Exception ex)
                {
                    logger.Error("sync device failed:" + ex.Message, ex);
                    Thread.Sleep(sleepTime);
                }
            }
        }

        private int GetGroupNumber(string connectString)
        {
            Regex r = new Regex("groupnum=[^&]+");//加入白名单，针对指定的正则表达式初始化 Regex 类的新实例。
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

        /// <summary>
        /// SynchronizeDevice
        /// </summary>
        /// <param name="systems"></param>
        public void SynchronizeDevice(StorageInfo info, string logicalID, Dictionary<int, List<IXSystem>> systems)
        {
            foreach (int groupNumber in systems.Keys)
            {
                foreach (IXSystem sourceSystem in systems[groupNumber])
                {
                    try
                    {
                        logger.Debug("begin sync device:" + sourceSystem.SystemLocation);
                        if (sourceSystem.DirectoryExists(info))
                        {
                            StorageInfo tmpInfo = info.Clone();
                            tmpInfo.LowName = LockFileName;
                            XFileInfo lockFile = sourceSystem.OpenFile(tmpInfo);
                            bool locked = false;
                            if (lockFile != null && lockFile.Exists)
                            {
                                long createTime = DateTime.Now.Ticks - lockFile.CreationTime.Ticks;
                                TimeSpan elapsedSpan = new TimeSpan(createTime);
                                if (elapsedSpan.Hours < 24)
                                {
                                    locked = true;
                                }
                                else
                                {
                                    sourceSystem.DeleteFile(tmpInfo);
                                    locked = false;
                                }
                            }
                            if (!locked)
                            {
                                var files = sourceSystem.ListFiles(info);
                                if (files != null && files.Count != 0)
                                {
                                    LockSyncSystem(sourceSystem, info);
                                    foreach (XFileInfo fileInfo in files)
                                    {
                                        try
                                        {
                                            SynchronizeSingleFile(fileInfo, sourceSystem, systems, groupNumber);
                                        }
                                        catch (Exception e)
                                        {
                                            logger.Error("Sync file {0} failed:{1}", fileInfo.FullName, e.Message);
                                        }
                                    }
                                    UnLockSyncSystem(sourceSystem, info);
                                }
                                var folders = sourceSystem.ListDirectories(info);
                                if (folders != null && folders.Count != 0)
                                {
                                    foreach (var folder in folders)
                                    {
                                        if (newStubRootFolder.Equals(folder.Name, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            var newStubFiles = sourceSystem.ListFiles(folder);
                                            foreach (var stubFile in newStubFiles)
                                            {
                                                string path = string.Empty;
                                                using (var stream = sourceSystem.OpenStream(stubFile, FileMode.Open))
                                                {
                                                    using (var reader = new StreamReader(stream))
                                                    {
                                                        path = reader.ReadLine();
                                                    }
                                                }
                                                if (!string.IsNullOrEmpty(path))
                                                {
                                                    var fileInfo = new XFileInfo();
                                                    fileInfo.HighName = info.HighName;
                                                    fileInfo.LowName = path;
                                                    try
                                                    {
                                                        SynchronizeSingleFile(fileInfo, sourceSystem, systems, groupNumber);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        logger.Error("Sync file {0} failed:{1}", fileInfo.FullName, e.Message);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        logger.Debug("end sync device:" + sourceSystem.SystemName);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Sync device {0} failed:{1}", sourceSystem.SystemName, ex.Message, ex);
                        UnLockSyncSystem(sourceSystem, info);
                    }
                }
            }
        }

        private void LockSyncSystem(IXSystem sourceSystem, StorageInfo info)
        {
            info.LowName = LockFileName;
            using (XStream stream = sourceSystem.OpenStream(info, FileMode.CreateNew))
            {
            }
        }

        private void UnLockSyncSystem(IXSystem sourceSystem, StorageInfo info)
        {
            info.LowName = LockFileName;
            sourceSystem.DeleteFile(info);
        }

        /// <summary>
        /// SynchronizeSingleFile
        /// </summary>
        /// <param name="fileInfo">表示的stub file info</param>
        /// <param name="sourceSystem"></param>
        /// <param name="systems"></param>
        public void SynchronizeSingleFile(XFileInfo fileInfo, IXSystem sourceSystem, Dictionary<int, List<IXSystem>> systems, int groupNumber)
        {
            byte[] buffer = new byte[1024 * 64];
            int readLen = 0;
            logger.Debug("begin sync file:" + fileInfo.HighPlusLowName);
            StorageInfo sInfo = GetStorageInfoFromFileName(fileInfo);
            try
            {
                int succeedGroupCount = 0;
                //get data file info
                XFileInfo sourceFile = sourceSystem.OpenFile(sInfo);
                foreach (int gNumber in systems.Keys)
                {
                    if (gNumber == groupNumber)
                    {
                        succeedGroupCount++;
                        continue;
                    }
                    //to one group
                    foreach (IXSystem destSystem in systems[gNumber])
                    {
                        try
                        {
                            if (destSystem.SystemHealth >= XSystemHealth.Available && !destSystem.IsFull)
                            {
                                //get dest data file info
                                XFileInfo destFile = destSystem.OpenFile(sInfo);
                                if (destFile == null || !destFile.Exists || destFile.FileSize != sourceFile.FileSize)
                                {
                                    using (XStream sourceSream = sourceSystem.OpenStream(sInfo, FileMode.Open))
                                    {
                                        using (XStream destStream = destSystem.OpenStream(sInfo, FileMode.OpenOrCreate))
                                        {
                                            while ((readLen = sourceSream.Read(buffer, 0, buffer.Length)) > 0)
                                            {
                                                destStream.Write(buffer, 0, readLen);
                                            }
                                            destStream.Commit();
                                        }
                                    }
                                }
                                succeedGroupCount++;
                                break;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            logger.Error("sync file {0} failed:{1}", fileInfo.HighPlusLowName, ex.Message, ex);
                            try
                            {
                                if (destSystem.FileExists(sInfo))
                                {
                                    destSystem.DeleteFile(sInfo);
                                }
                            }
                            catch (System.Exception exc)
                            {
                                Trace.TraceError(exc.ToString());
                            }
                        }
                    }
                }
                //delete stub file
                if (succeedGroupCount == systems.Count)
                {
                    if (sourceSystem.FileExists(fileInfo))
                    {
                        sourceSystem.DeleteFile(fileInfo);
                    }
                    else
                    {
                        var info = XConvert.FromNames(PathUtil.CombinePath(fileInfo.HighName, newStubRootFolder), HashCodeHelper.ToMD5HashCode(sInfo.HighPlusLowName));
                        if (sourceSystem.FileExists(info))
                        {
                            sourceSystem.DeleteFile(info);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                logger.Error("sync file {0} failed:{1}", fileInfo.HighPlusLowName, ex.Message, ex);
            }
        }

        public StorageInfo GetStorageInfoFromFileName(XFileInfo info)
        {
            StorageInfo sInfo = new StorageInfo();
            string fullName = AveConverter.DecodeSpecialChar(info.LowName);
            sInfo.HighName = fullName.Substring(0, fullName.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase));
            sInfo.LowName = fullName.Substring(fullName.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
            return sInfo;
        }

        public void StartSynchronize()
        {
            if (threadPool == null)
            {
                threadPool = new SafeDictionary<string, Thread>();
            }
            if (!threadPool.ContainsKey(threadName) || !threadPool[threadName].IsAlive)
            {
                logger.Info("begin StartSynchronize thread");
                Thread thread = new Thread(new ThreadStart(SynchronizeDevices));
                thread.Name = threadName;
                threadPool[threadName] = thread;
                thread.IsBackground = true;
                thread.Start();
            }
        }
    }
}
