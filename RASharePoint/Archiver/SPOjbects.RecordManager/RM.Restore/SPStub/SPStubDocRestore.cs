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

using AvePoint.GCommon;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Restore;
using System;
using System.Reflection;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class SPStubDocRestore : AveSPDoc, IDisposable
    {
        //AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        ScheduleConfiguration mConfig;
        AveSPSite aveSPSite;
        AveSPWeb aveSPWeb;
        Guid aveSPWebId;
        AveSPList aveSPList;
        Guid aveSPListId;
        //暴露此属性，为了调用Wrapper方法传值.
        public AveSPFolder aveSPFolder;
        IAveSite site;
        IAveWeb web;
        IAveList list;
        IAveFolder currentIAveFolder;
        IAveORecords records;
        Guid aveSPFolderId;
        private DateTime mInitialTime = DateTime.MinValue;//用于记录Site的生存时间
        private string mSiteUrl = string.Empty;
        private readonly object mInitLock = new object();
        public SPStubDocRestore(ScheduleConfiguration config)
        {
            mConfig = config;
        }

        public SPStubDocRestore()
        {

        }

        public void Init(ScheduleConfiguration config)
        {
            mConfig = config;
        }

        public IAveORecords Record
        {
            get
            {
                if (records == null)
                {
                    records = mConfig.aveObjectModelFactory.CreateRecords();
                }
                return records;
            }
        }

        public IAveFolder EnsureFolder(string folderUrl)
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPStubDocRestore.EnsureFolder"))
            {
                if (currentIAveFolder == null || !folderUrl.Equals(currentIAveFolder.ParentWeb.Url + "/" + currentIAveFolder.Url))
                {
                    currentIAveFolder = web.GetFolder(folderUrl);
                    if (!currentIAveFolder.Exists)
                    {
                        throw new Exception(string.Format("Folder Not Exists :{0}", currentIAveFolder.Name));
                    }
                }
                return currentIAveFolder;
            }
        }

        public void RestoreParentInfo(string desUrl)
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPStubDocRestore.RestoreParentInfo"))
            {
                AveBPOSAccountInfo user = null;
                string siteUrl = string.Empty;
                string userName = string.Empty;
                AveObjectModelFactory factory = null;
                AvePoint.GCommon.Contract.CentralAdmin.Object.BposInfo bposInfo = null;
                factory = mConfig.aveObjectModelFactory;
                userName = mConfig.user.UserName;

                user = mConfig.user;
                siteUrl = factory.CreateSiteServiceHelper().TryToRectifySiteUrl(desUrl, user);//获取的是Web URL，而不是实际的SC URL
                lock (mInitLock)
                {
                    if (site == null)
                    {
                        mInitialTime = DateTime.Now;
                        // 重新实例化site 对象，必须释放aveSPSite，并且把aveSPSite 置空，保证能走到restore site 逻辑中
                        if (aveSPSite != null)
                        {
                            aveSPSite.Dispose();
                            aveSPSite = null;
                            aveSPWeb = null;
                            aveSPList = null;
                            aveSPFolder = null;
                            currentIAveFolder = null;
                        }
                        site = factory.CreateSite(siteUrl);
                        mSiteUrl = siteUrl;
                        web = site.OpenWeb();
                    }
                    else if ((string.Compare(siteUrl, mSiteUrl, StringComparison.OrdinalIgnoreCase) != 0)
                                || mInitialTime.AddHours(23) < DateTime.Now)
                    {
                        site.Dispose();
                        // 重新实例化site 对象，必须释放aveSPSite，并且把aveSPSite 置空，保证能走到restore site 逻辑中
                        if (aveSPSite != null)
                        {
                            aveSPSite.Dispose();
                            aveSPSite = null;
                            aveSPWeb = null;
                            aveSPList = null;
                            aveSPFolder = null;
                            currentIAveFolder = null;
                        }
                        mInitialTime = DateTime.Now;
                        site = factory.CreateSite(siteUrl);
                        mSiteUrl = siteUrl;
                        web = site.OpenWeb(AveUrlUtility.GetServerRelativeUrl(siteUrl));
                    }
                    if (desUrl.Contains("#/"))
                    {
                        desUrl = desUrl.Substring(desUrl.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2);
                    }
                    list = web.GetList(desUrl);
                    EnsureFolder(list.RootFolder.ServerRelativeUrl);
                    RestoreSiteInfo(site, user);
                    RestoreWebInfo();
                    RestoreListInfo();
                    RestoreFolderInfo();
                }
            }
        }

        public void RestoreSiteInfo(IAveSite site, AveBPOSAccountInfo user)
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPStubDocRestore.RestoreSiteInfo"))
            {
                if (aveSPSite == null)
                {
                    if (user != null)
                    {
                        aveSPSite = new AveSPSite(site.Url, site.Url, AveContextKind.ClientObjectModel, user);
                    }
                    else
                    {
                        aveSPSite = new AveSPSite(site.Url, site.Url, AveContextKind.Server13ObjectModel, null);
                    }
                    aveSPSite.RestoreSiteSelf(site.SiteSerializer.GetObjectData());
                }
            }
        }

        private void RestoreWebInfo()
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPStubDocRestore.RestoreWebInfo"))
            {
                if (aveSPWeb == null || aveSPWebId == null || aveSPWebId != currentIAveFolder.ParentWeb.ID)
                {
                    aveSPWeb = new AveSPWeb(aveSPSite, web.ServerRelativeUrl);                    
                    aveSPWebId = web.ID;
                    aveSPWeb.RestoreWebSelf(web.WebSerializer.GetObjectData());
                }
            }
        }

        private void RestoreListInfo()
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPStubDocRestore.RestoreListInfo"))
            {
                if (aveSPList == null || aveSPListId == null || aveSPListId != list.ID)
                {
                    aveSPList = new AveSPList(aveSPWeb, list.Title);
                    aveSPListId = list.ID;
                    aveSPList.RestoreListSelf(list.GetListInfo());
                }
            }
        }

        private void RestoreFolderInfo()
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPStubDocRestore.RestoreFolderInfo"))
            {
                if (aveSPFolder == null || aveSPFolderId == null || aveSPFolderId != currentIAveFolder.ParentList.RootFolder.UniqueId)
                {
                    aveSPFolder = new AveSPFolder(aveSPList, currentIAveFolder.Name);
                    aveSPFolderId = currentIAveFolder.UniqueId;                    
                }
            }
        }

        public void Dispose()
        {

        }

        private void DisposeObj(IDisposable obj)
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
            }
        }
    }
}
