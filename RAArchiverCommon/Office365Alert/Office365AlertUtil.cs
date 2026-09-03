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
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon;
using Org.BouncyCastle.Ocsp;
using System.Xml;
using Microsoft.Graph.Models;
using Microsoft.SharePoint.Client;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Common;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public class Office365AlertUtil
    {
        #region Private Member
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private ScheduleConfiguration mConfig;

        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private DateTime mInitialTime = DateTime.MinValue;//用于记录mSite的生存时间

        private IAveSite mSite = null;

        private IAveWeb mWeb = null;

        private IAveList mList = null;

        private string mSiteUrl = string.Empty;

        private Guid mWebID = Guid.Empty;

        private Guid mListID = Guid.Empty;


        private readonly static object mLock = new object();

        private Dictionary<string, Office365AlertCache> cacheLibraryDisableAlert = new Dictionary<string, Office365AlertCache>();

        private List<string> disableAlertLibrarys = new List<string>();

        private bool enableOperateAlert = false;

        #endregion

        #region Porperty
        private IAveSite Site
        {
            get
            {
                if (null == mSite)
                {
                    mLog.Info("Init site for ArchiverDeletion.mSiteUrl:{0}.", mSiteUrl);
                    mInitialTime = DateTime.Now;
                    AveObjectModelFactory factory = mConfig.aveObjectModelFactory;
                    mSite = factory.CreateSite(mSiteUrl);
                }
                else if ((string.Compare(mSite.Url, mSiteUrl, StringComparison.OrdinalIgnoreCase) != 0)
                            || mInitialTime.AddHours(23) < DateTime.Now)
                {
                    mLog.Info("Init site for ArchiverDeletion.mSiteUrl:{0}.", mSiteUrl);
                    mSite.Dispose();
                    mInitialTime = DateTime.Now;
                    AveObjectModelFactory factory = mConfig.aveObjectModelFactory;
                    mSite = factory.CreateSite(mSiteUrl);
                }
                return mSite;
            }
        }

        private IAveWeb Web
        {
            get
            {
                if (null == mWeb)
                {
                    mLog.Info("Init web for ArchiverDeletion.webGuid:{0}.", mWebID);
                    mWeb = Site.OpenWeb(mWebID);
                }
                else if (!mWeb.ID.Equals(mWebID))
                {
                    mLog.Info("Init web for ArchiverDeletion.webGuid:{0}.", mWebID);
                    mWeb.Dispose();
                    mWeb = Site.OpenWeb(mWebID);
                }
                return mWeb;
            }
        }

        private IAveList List
        {
            get
            {
                if (Guid.Empty.Equals(mListID))//如果listGuid为空，说明是systemList，则赋值为null
                {
                    mList = null;
                }
                else if (null == mList || !mListID.Equals(mList.ID))
                {
                    mLog.Info("Init list for ArchiverDeletion.listGuid:{0}.", mListID);
                    mList = Web.Lists[mListID];
                }
                return mList;
            }
        }
        #endregion

        #region Construct and Init

        public Office365AlertUtil(ScheduleConfiguration config)
        {
            mConfig = config;
            enableOperateAlert = GetEnableOperateAlert();
        }

        public void Dispose()
        {
            try
            {
                //Keep Data & Deletion目前都是多线程，且Container对象都是外围传来的，直接外围Dispose
                if (mList != null)
                {
                    //mList = null;
                }
                if (mWeb != null)
                {
                    //mWeb.Dispose();
                    //mWeb = null;
                }
                if (mSite != null)
                {
                    //mSite.Dispose();
                    //mSite = null;
                }
            }
            catch (Exception e)
            {
                mLog.Info("Archiver Keep Data Dispose Error: {0}", e.ToString());
            }

        }
        #endregion

        public void DisableLibraryAlert(string siteUrl, Guid webID, Guid listID)
        {
            try
            {
                if (enableOperateAlert)
                {
                    string value = String.Concat(listID.ToString(), "|", webID.ToString(), "|", siteUrl);
                    if (!disableAlertLibrarys.Contains(value))
                    {
                        mLog.Info("DisableLibraryAlert.SiteUrl:{0}.ListId:{1}.", siteUrl, listID);
                        InternalDisableLibraryAlert(listID, webID, siteUrl, value);
                        disableAlertLibrarys.Add(value);
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed DisableLibraryAlert.SiteUrl:{siteUrl}.ListId:{listID}.Message:{ex}.");
            }
        }

        public void EnableLibraryAlert(string siteUrl, Guid webID, Guid listID)
        {
            try
            {
                if (enableOperateAlert)
                {
                    string value = String.Concat(listID.ToString(), "|", webID.ToString(), "|", siteUrl);
                    if (disableAlertLibrarys.Contains(value))
                    {
                        mLog.Info("EnableLibraryAlert.SiteUrl:{0}.ListId:{1}.Sleep 1 minutes.", siteUrl, listID);
                        Thread.Sleep(60 * 1000);
                        InternalEnableLibraryAlert(listID, webID, siteUrl, value);
                        disableAlertLibrarys.Remove(value);
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed EnableLibraryAlert.SiteUrl:{siteUrl}.ListId:{listID}.Message:{ex}.");
            }
        }

        public void EnableAllCacheLibraryAlert()
        {
            try
            {
                if (enableOperateAlert)
                {
                    if (cacheLibraryDisableAlert != null && cacheLibraryDisableAlert.Count > 0)
                    {
                        mLog.Info($"EnableAllCacheLibraryAlert.Count:{cacheLibraryDisableAlert.Count}.Sleep 1 minutes.");
                        Thread.Sleep(60 * 1000);
                        foreach (var keyValuePair in cacheLibraryDisableAlert)
                        {
                            InternalEnableAllCacheLibraryAlert(keyValuePair.Value);
                        }
                        cacheLibraryDisableAlert.Clear();
                        disableAlertLibrarys.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed EnableAllCacheLibraryAlert.Message:{ex}.");
            }
            
        }

        private bool GetEnableOperateAlert()
        {
            try
            {
                var key = RMKeyValueDao.GetValueByKey("EnableOperateAlert");
                bool.TryParse(key?.Value, out bool result);
                mLog.Info($"GetEnableOperateAlert:{result}.");
                return result;
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed GetEnableOperateAlert.Message:{ex}.");
            }
            return false;
        }

        private void InternalDisableLibraryAlert(Guid listId, Guid webID, string siteUrl, string fullPath)
        {
            if (listId != Guid.Empty)
            {
                mLog.Info($"Begin InternalDisableLibraryAlert.listId:{listId}.webId:{webID}.siteUrl:{siteUrl}.CachePath:{fullPath}.");
                mSiteUrl = siteUrl;
                mWebID = webID;
                mListID = listId;
                try
                {
                    using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.InternalDisableLibraryAlert"))
                    {
                        var webAlerts = Web.AlertsV2;
                        mLog.Info($"InternalDisableLibraryAlert.webId:{webID}.webAlertsCount:{webAlerts.Count}.");
                        var listEnableAlert = webAlerts.Where(x => x.ListID.Equals(listId) && x.Status == AveAlertStatus.On).ToList();
                        mLog.Info($"InternalDisableLibraryAlert.listId:{listId}.listEnableAlert:{listEnableAlert.Count}.");
                        var listEnableAlertIds = listEnableAlert.Select(x => x.ID).ToList();
                        if (cacheLibraryDisableAlert != null && !cacheLibraryDisableAlert.ContainsKey(fullPath))
                        {
                            var cacheAlert = new Office365AlertCache()
                            {
                                SiteUrl = siteUrl,
                                WebID = webID,
                                ListID = listId,
                                LibraryDisableAlert = listEnableAlertIds
                            };
                            cacheLibraryDisableAlert.Add(fullPath, cacheAlert);
                            Web.DisableAlert(Web.ServerRelativeUrl, listEnableAlertIds);
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while InternalDisableLibraryAlert, error:{0}, listId:{1}, webId:{2}, siteUrl:{3}", e, listId, webID, siteUrl);
                }
            }
        }

        private void InternalEnableLibraryAlert(Guid listId, Guid webID, string siteUrl, string fullPath)
        {
            if (listId != Guid.Empty)
            {
                mLog.Info($"Begin InternalEnableLibraryAlert.listId:{listId}.webId:{webID}.siteUrl:{siteUrl}.CachePath:{fullPath}.");
                mSiteUrl = siteUrl;
                mWebID = webID;
                mListID = listId;
                try
                {
                    using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.InternalEnableLibraryAlert"))
                    {
                        if (cacheLibraryDisableAlert != null && cacheLibraryDisableAlert.ContainsKey(fullPath))
                        {
                            var listEnableAlertIds = cacheLibraryDisableAlert[fullPath];
                            mLog.Info($"InternalEnableLibraryAlert.webId:{webID}.cacheLibraryDisableAlert:{listEnableAlertIds.LibraryDisableAlert.Count}.");
                            Web.EnableAlert(Web.ServerRelativeUrl, listEnableAlertIds.LibraryDisableAlert);
                            cacheLibraryDisableAlert.Remove(fullPath);
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while InternalEnableLibraryAlert, error:{0}, listId:{1}, webId:{2}, siteUrl:{3}", e, listId, webID, siteUrl);
                }
            }
        }

        private void InternalEnableAllCacheLibraryAlert(Office365AlertCache office365AlertCache)
        {
            if (office365AlertCache != null)
            {
                mLog.Info($"Begin InternalEnableAllCacheLibraryAlert.listId:{office365AlertCache.ListID}.webId:{office365AlertCache.WebID}.siteUrl:{office365AlertCache.SiteUrl}.cacheLibraryDisableAlert:{office365AlertCache.LibraryDisableAlert.Count}.");
                mSiteUrl = office365AlertCache.SiteUrl;
                mWebID = office365AlertCache.WebID;
                mListID = office365AlertCache.ListID;
                try
                {
                    using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.InternalEnableAllCacheLibraryAlert"))
                    {
                        {
                            Web.EnableAlert(Web.ServerRelativeUrl, office365AlertCache.LibraryDisableAlert);
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error($"An error occurred while InternalEnableAllCacheLibraryAlert, error:{e}, listId:{office365AlertCache.ListID}, webId:{office365AlertCache.WebID}, siteUrl:{office365AlertCache.SiteUrl}.");
                }
            }
        }
    }

    public class Office365AlertCache
    {
        public string SiteUrl { get; set; }
        public Guid WebID { get; set; }
        public Guid ListID { get; set; }
        public List<Guid> LibraryDisableAlert { get; set; }
    }
}
