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
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Collections;
    using AvePoint.Media.Storage.Util;
    using System.IO;
    using System.Diagnostics;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Utility; 
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2013/3/13",
    "shouqiang.liu@avepoint.com",
    "yanxin.fu@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_8, CodeReviewConstants.CHECK_LIST_ID_CO_12 },
     null,
     true)]
    #endregion

    class MirrorFSSynchronizer
    {
        private static readonly string stubRootFolder = @"DocAveSyncFileStubs\DataManager";
        private static readonly string newStubRootFolder = @"NewSyncFileStub";
        private static Queue syncInfoQueue;
        private static StorageLogger logger = new StorageLogger(typeof(MirrorFSSynchronizer));
        private static Thread innerThread;

        public static void EnQueue(MirrorFSInfo msInfo)
        {
            if (syncInfoQueue == null)
            {
                syncInfoQueue = Queue.Synchronized(new Queue());
            }
            syncInfoQueue.Enqueue(msInfo);
        }

        public static MirrorFSInfo DeQueue(MirrorFSInfo msInfo)
        {
            if (syncInfoQueue == null || syncInfoQueue.Count == 0)
            {
                return null;
            }
            return syncInfoQueue.Dequeue() as MirrorFSInfo;
        }

        public static bool IsAlive()
        {
            if (innerThread != null && innerThread.IsAlive)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool isSynchronizing()
        {
            if (syncInfoQueue == null || syncInfoQueue.Count == 0)
            {
                return false;
            }
            return true;
        }

        public static void StartSynchronize()
        {
            logger.Info("begin InnerSynchronizer thread");
            innerThread = new Thread(new ThreadStart(SynchronizeDevices));
            innerThread.Name = "RAIDInnerSynchronizer";
            innerThread.IsBackground = true;
            innerThread.Start();
        }

        public static void SynchronizeDevices()
        {
            while (true)
            {
                try
                {
                    if (syncInfoQueue == null || syncInfoQueue.Count == 0)
                    {
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        SynchronizeDevice((MirrorFSInfo)syncInfoQueue.Dequeue());
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning(ex.ToString());
                }
            }
        }

        public static void SynchronizeDevice(MirrorFSInfo MsInfo)
        {
            byte[] buffer = new byte[1024 * 64];
            int readLen = 0;
            logger.Debug("begin sync file:" + MsInfo.Info.HighPlusLowName);
            List<IXSystem> succeedSystemList = new List<IXSystem>();
            foreach (List<IXSystem> group in MsInfo.InnerSystems.Values)
            {
                foreach (IXSystem destSystem in group)
                {
                    try
                    {
                        if (destSystem.SystemHealth >= XSystemHealth.Available && !destSystem.IsFull)
                        {
                            using (XStream sourceSream = MsInfo.StubSystem.OpenStream(MsInfo.Info, FileMode.Open))
                            {
                                using (XStream destStream = destSystem.OpenStream(MsInfo.Info, FileMode.OpenOrCreate))
                                {
                                    while ((readLen = sourceSream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        destStream.Write(buffer, 0, readLen);
                                    }
                                    destStream.Commit();
                                    succeedSystemList.Add(destSystem);
                                    break;
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        logger.Warn("sync file {0} failed:{1}", MsInfo.Info.HighPlusLowName, ex);
                    }
                }
            }
            if (succeedSystemList.Count == MsInfo.InnerSystems.Count)
            {
                StorageInfo sInfo = new StorageInfo();
                sInfo.HighName = PathUtil.CombinePath(stubRootFolder, MsInfo.LogicalId);
                sInfo.LowName = AveConverter.EncodeSpecialChar(MsInfo.Info.HighPlusLowName);
                if (MsInfo.StubSystem.FileExists(sInfo))
                {
                    MsInfo.StubSystem.DeleteFile(sInfo);
                }
                else
                {
                    var newStubInfo = new StorageInfo();
                    newStubInfo.HighName = PathUtil.CombinePath(sInfo.HighName, newStubRootFolder);
                    newStubInfo.LowName = HashCodeHelper.ToMD5HashCode(MsInfo.Info.HighPlusLowName);
                    if (MsInfo.StubSystem.FileExists(newStubInfo))
                    {
                        MsInfo.StubSystem.DeleteFile(newStubInfo);
                    }
                }
            }
        }
    }

    class MirrorFSInfo
    {
        public Dictionary<int, List<IXSystem>> InnerSystems { get; set; }

        public string LogicalId { get; set; }

        public StorageInfo Info { get; set; }

        public IXSystem StubSystem { get; set; }

        public MirrorFSInfo(string logicalId, IXSystem StubSystem, Dictionary<int, List<IXSystem>> innerSystems, StorageInfo info)
        {
            this.LogicalId = logicalId;
            this.StubSystem = StubSystem;
            this.InnerSystems = innerSystems;
            this.Info = info;
        }
    }
}
