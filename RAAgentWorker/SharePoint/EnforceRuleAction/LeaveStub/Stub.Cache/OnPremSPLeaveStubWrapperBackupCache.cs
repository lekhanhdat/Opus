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
using AvePoint.RA.Contract.Services;
using AvePoint.GCommon;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.SharePoint.EnforceRuleAction.LeaveStub
{
    public class OnPremSPLeaveStubWrapperBackupCache
    {
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveObjectModelFactory CurrentModelFactory;
        private AveBPOSAccountInfo BposInfo;
        private static OnPremSPLeaveStubWrapperBackupCache actions = null;
        private static object mLock = new object();
        public AveSPSite StubBackupAveSPSite { get; set; }
        public AveSPWeb StubBackupAveSPWeb { get; set; }
        public AveSPList StubBackupAveSPList { get; set; }
        public AveSPFolder StubBackupAveSPRootFolder { get; set; }
        public AveSPFolder StubOnlyBackupAveSPCurrentFolder { get; set; }

        public static OnPremSPLeaveStubWrapperBackupCache GetInstance(AveObjectModelFactory objectModelFactory, AveBPOSAccountInfo bposInfo)
        {
            if (actions == null)
            {
                lock (mLock)
                {
                    if (actions == null)
                    {
                        actions = new OnPremSPLeaveStubWrapperBackupCache(objectModelFactory, bposInfo);
                    }
                }
            }
            return actions;
        }

        private OnPremSPLeaveStubWrapperBackupCache(AveObjectModelFactory objectModelFactory, AveBPOSAccountInfo bposInfo)
        {
            CurrentModelFactory = objectModelFactory;
            BposInfo = bposInfo;
        }

        public void InitStubAveBackupContainer(string siteUrl, Guid webId, string webTitle, Guid listId, string listTitle, OnPremSPLeaveStubWrapperAveObjectInfo onPremSPLeaveStubAveObjectInfo)
        {
            using (var performance = new AgentPerformanceScope("ArchiveBackUp.InitStubSourceAveBackupContainer", addToStatistics: true))
            {
                lock (mLock)
                {
                    if (StubBackupAveSPSite == null || siteUrl != StubBackupAveSPSite.SPSite.Url)
                    {
                        mLog.Info("Begin init StubBackupAveSPSite when InitStubAveBackupContainer.");
                        StubBackupAveSPSite = new AveSPSite(siteUrl, AveContextKind.ClientObjectModel, BposInfo, null);
                        onPremSPLeaveStubAveObjectInfo.AveSiteInfo = new AveSPSiteInfo(StubBackupAveSPSite).GetSiteInfo();
                    }
                    if (StubBackupAveSPWeb == null || StubBackupAveSPWeb.SPWeb.ID != webId)
                    {
                        mLog.Info("Begin init StubBackupAveSPWeb when InitStubAveBackupContainer.WebName:{0}.", webTitle.LogBase64());
                        if (StubBackupAveSPWeb != null)
                        {
                            StubBackupAveSPWeb.Dispose();//spWebId不等于file web ID时，需要先进行dispose
                        }
                        StubBackupAveSPWeb = new AveSPWeb(StubBackupAveSPSite, webId, webTitle);
                        onPremSPLeaveStubAveObjectInfo.AveWebInfo = new AveSPWebInfo(StubBackupAveSPWeb).GetWebInfo();
                    }
                    if (StubBackupAveSPList == null || StubBackupAveSPList.SPList.ID != listId)
                    {
                        mLog.Info("Begin init StubBackupAveSPList when InitStubAveBackupContainer.ListTitle:{0}.", listTitle.LogBase64());
                        StubBackupAveSPList = new AveSPList(StubBackupAveSPWeb, listId, listTitle);
                        onPremSPLeaveStubAveObjectInfo.AveListInfo = new AveSPListInfo(StubBackupAveSPList).GetListInfo();
                    }
                    if (StubBackupAveSPRootFolder == null || StubBackupAveSPRootFolder.AveList.SPList.ID != listId)
                    {
                        mLog.Info("Begin init StubBackupAveSPFolder when InitStubAveBackupContainer.ListTitle:{0}.", listTitle.LogBase64());
                        if (StubBackupAveSPRootFolder != null)
                        {
                            StubBackupAveSPRootFolder.Dispose();
                        }
                        // 每个List缓存Root Folder，然后每个SubFolder单独获取，如果是同一个Subfolder则不需要重新实例化
                        StubBackupAveSPRootFolder = new AveSPFolder(StubBackupAveSPList);
                        StubOnlyBackupAveSPCurrentFolder = StubBackupAveSPRootFolder;
                    }
                }
            }
        }

        /// <summary>
        /// 每个List缓存Root Folder，然后每个SubFolder单独获取，如果是同一个Subfolder则不需要重新实例化
        /// </summary>
        public AveSPFolder GetCurrentAveBackupFolder(IAveFolder folder)
        {            
            using (var performance = new AgentPerformanceScope("ArchiveBackUp.ReGetStubRestoreAveSPFolder", addToStatistics: true))
            {
                lock (mLock)
                {
                    AveSPFolder result;
                    if (folder.UniqueId != StubOnlyBackupAveSPCurrentFolder.Id)
                    {
                        mLog.Info("GetCurrentAveBackupFolder Current folder :{0} doesn't match StubBackupAveSPCurrentFolder:{1} that need get new folder.", folder?.UniqueId, StubOnlyBackupAveSPCurrentFolder?.SPFolder?.UniqueId);
                        //SP Query方式，返回的先是SubFolder，之后才是Root Folder。此处添加直接返回Root Folder逻辑，避免造成Root Folder获取不到的情况
                        if (folder.ServerRelativeUrl == StubBackupAveSPRootFolder.ServerRelativeUrl)
                        {
                            result = StubBackupAveSPRootFolder;
                        }
                        else
                        {
                            result = new AveSPFolder(GetCurrentAveBackupFolderByRootFolder(folder.ParentFolder), folder.Name, folder.UniqueId, folder.Item.ID, 512/*folder.Item.Versions[0].VersionId*/);
                        }
                        StubOnlyBackupAveSPCurrentFolder = result;
                    }
                    else
                    {
                        result = StubOnlyBackupAveSPCurrentFolder;
                    }
                    return result;
                }
            }
        }

        public AveSPFolder GetCurrentAveBackupFolderByRootFolder(IAveFolder folder)
        {
            using (var performance = new AgentPerformanceScope("ArchiveBackUp.GetCurrentAveBackupFolderByRootFolder", addToStatistics: true))
            {
                lock (mLock)
                {
                    AveSPFolder result;
                    if (folder.UniqueId != StubBackupAveSPRootFolder.Id)
                    {
                        mLog.Info("GetCurrentAveBackupFolderByRootFolder Current folder :{0} doesn't match StubBackupAveSPCurrentFolder:{1} that need get new folder.", folder?.UniqueId, StubBackupAveSPRootFolder?.SPFolder?.UniqueId);
                        result = new AveSPFolder(GetCurrentAveBackupFolderByRootFolder(folder.ParentFolder), folder.Name, folder.UniqueId, folder.Item.ID, folder.Item.Versions[0].VersionId);
                    }
                    else
                    {
                        result = StubBackupAveSPRootFolder;
                    }
                    return result;
                }
            }
        }
    }
}
