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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.SharePoint.EnforceRuleAction.LeaveStub
{
    public class OnPremSPLeaveStubWrapperRestoreCache
    {
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveObjectModelFactory CurrentModelFactory;
        private AveBPOSAccountInfo BposInfo;
        private static OnPremSPLeaveStubWrapperRestoreCache actions = null;
        private static object mLock = new object();
        public AveSPSite StubRestoreAveSPSite { get; set; }
        public AveSPWeb StubRestoreAveSPWeb { get; set; }
        public AveSPList StubRestoreAveSPList { get; set; }
        public AveSPFolder StubRestoreAveSPRootFolder { get; set; }
        public AveSPFolder StubOnlyRestoreAveSPCurrentFolder { get; set; }
        public static OnPremSPLeaveStubWrapperRestoreCache GetInstance(AveObjectModelFactory objectModelFactory, AveBPOSAccountInfo bposInfo)
        {
            if (actions == null)
            {
                lock (mLock)
                {
                    if (actions == null)
                    {
                        actions = new OnPremSPLeaveStubWrapperRestoreCache(objectModelFactory, bposInfo);
                    }
                }
            }
            return actions;
        }

        private OnPremSPLeaveStubWrapperRestoreCache(AveObjectModelFactory objectModelFactory, AveBPOSAccountInfo bposInfo)
        {
            CurrentModelFactory = objectModelFactory;
            BposInfo = bposInfo;
        }

        public void InitStubAveRestoreContainer(string siteUrl, Guid webId, string webTitle, Guid listId, string listTitle, OnPremSPLeaveStubWrapperAveObjectInfo onPremSPLeaveStubAveObjectInfo)
        {
            using (var performance = new AgentPerformanceScope("ArchiveBackUp.InitStubAveRestoreContainer", addToStatistics: true))
            {
                lock (mLock)
                {
                    if (StubRestoreAveSPSite == null || siteUrl != StubRestoreAveSPSite.SPSite.Url)
                    {
                        mLog.Info("Begin init StubRestoreAveSPSite when InitStubAveRestoreContainer.");
                        StubRestoreAveSPSite = new AveSPSite(siteUrl, siteUrl, AveContextKind.ClientObjectModel, BposInfo);
                        StubRestoreAveSPSite.RestoreSiteSelf(onPremSPLeaveStubAveObjectInfo.AveSiteInfo);
                    }
                    if (StubRestoreAveSPWeb == null || StubRestoreAveSPWeb.SPWeb.ID != webId)
                    {
                        mLog.Info("Begin init StubRestoreAveSPWeb when InitStubAveRestoreContainer.Web.Url:{0}.", webTitle.LogBase64());
                        if (StubRestoreAveSPWeb != null)
                        {
                            StubRestoreAveSPWeb.Dispose();//spWebId不等于file web ID时，需要先进行dispose
                        }
                        StubRestoreAveSPWeb = new AveSPWeb(StubRestoreAveSPSite, webTitle);
                        StubRestoreAveSPWeb.RestoreWebSelf(onPremSPLeaveStubAveObjectInfo.AveWebInfo);
                    }
                    if (StubRestoreAveSPList == null || StubRestoreAveSPList.SPList.ID != listId)
                    {
                        mLog.Info("Begin init StubRestoreAveSPList when InitStubAveRestoreContainer.List.Title:{0}.", listTitle.LogBase64());
                        StubRestoreAveSPList = new AveSPList(StubRestoreAveSPWeb, listTitle);
                        StubRestoreAveSPList.RestoreListSelf(onPremSPLeaveStubAveObjectInfo.AveListInfo);
                    }
                    // 每个List缓存Root Folder，然后每个SubFolder单独获取，如果是同一个Subfolder则不需要重新实例化
                    if (StubRestoreAveSPRootFolder == null || StubRestoreAveSPRootFolder.ParentList.SPList.ID != listId)
                    {
                        mLog.Info("Begin init StubRestoreAveSPFolder when InitStubAveRestoreContainer.List.Title:{0}.", listTitle.LogBase64());
                        StubRestoreAveSPRootFolder = new AveSPFolder(StubRestoreAveSPList, StubRestoreAveSPList.RootFolder.Name);
                        StubOnlyRestoreAveSPCurrentFolder = StubRestoreAveSPRootFolder;
                    }
                }
            }
        }

        public AveSPFolder GetStubRestoreAveCurrentFolder(string subFolderUrl, Guid parentFolderId)
        {
            using (var performance = new AgentPerformanceScope("ArchiveBackUp.ReGetStubRestoreAveSPFolder", addToStatistics: true))
            {
                lock (mLock)
                {
                    if (!StubOnlyRestoreAveSPCurrentFolder.Id.Equals(parentFolderId))
                    {
                        mLog.Info("GetStubRestoreAveCurrentFolder StubSubFolderUrl:{0}.", subFolderUrl.LogBase64());
                        if (parentFolderId.Equals(StubRestoreAveSPRootFolder.Id))
                        {
                            StubOnlyRestoreAveSPCurrentFolder = StubRestoreAveSPRootFolder;
                        }
                        else
                        {
                            StubOnlyRestoreAveSPCurrentFolder = GetRestoreSubAveSPFolder(StubRestoreAveSPRootFolder, subFolderUrl);
                        }
                        return StubOnlyRestoreAveSPCurrentFolder;
                    }
                    else
                    {
                        return StubOnlyRestoreAveSPCurrentFolder;
                    }
                }
            }
        }

        private AveSPFolder GetRestoreSubAveSPFolder(AveSPFolder parentFolder, string destFolderUrl)
        {
            if (string.IsNullOrEmpty(destFolderUrl))
            {
                return parentFolder;
            }
            if (!destFolderUrl.Contains("/"))
            {
                AveSPFolder subFolder = new AveSPFolder(parentFolder, destFolderUrl);
                subFolder.InitSPFolder();
                return subFolder;
            }
            int pos = destFolderUrl.IndexOf("/");
            if (pos > -1)
            {
                string subDest = destFolderUrl.Substring(0, pos);
                string subLastDest = destFolderUrl.Substring(pos + 1);
                AveSPFolder subFolder = new AveSPFolder(parentFolder, subDest);
                subFolder.InitSPFolder();
                return this.GetRestoreSubAveSPFolder(subFolder, subLastDest);
            }
            return parentFolder;
        }
    }
}
