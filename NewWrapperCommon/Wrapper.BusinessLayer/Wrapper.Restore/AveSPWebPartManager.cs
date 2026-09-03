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





using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPWebPartManager : IDisposable, AvePoint.Wrapper.Restore.IAveSPWebPartManager
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveSPDoc mAveDoc;
        //private AveSqlConnection mSqlCon;
        private IAveLimitedWebPartManager mManager;
        private Dictionary<AveListTemplateType, string> mValidBaseViewIDCollection = new Dictionary<AveListTemplateType, string>();
        //private AveSPWebPart mWebPart;
        //private AveSPViewWebPart mViewWebPart;

        //private AveRestoreOption mRestoreOption;

        private IReport mReport;
        public IReport Report
        {
            get
            {
                return mReport;
            }
        }

        public bool NeedReloadList { get; private set; }
        
        public AveSPWebPartManager(AveSPDoc aveDoc)
        {
            mAveDoc = aveDoc;
            //mSqlCon = aveDoc.AveSite.SqlConn;
            mReport = aveDoc.GetReport();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webParts"></param>
        /// <param name="restoreOption"></param>
        /// <param name="clearAllBeforeRestore">当我们在进行List Pose Action来还原没有还原的ListWebPart时，不应该删除已经存在的Web Part</param>
        /// <returns>Need reload list</returns>
        public void Restore(IList webParts, AveRestoreOption restoreOption, bool clearAllBeforeRestore)
        {
            try
            {
                if (webParts.Count == 1)
                {
                    //ADO-160852 不去构造空webpart,会把目的端的webpart给清除掉,此处兼容老数据
                    AveWebPartBaseInfo baseInfo = webParts[0] as AveWebPartBaseInfo;
                    if (baseInfo != null && baseInfo.ID == Guid.Empty)
                    {
                        log.Info("No webpart need to restore");
                        return;
                    }
                }
                IList<AveWebPartBaseInfo> webpartList = webParts as IList<AveWebPartBaseInfo>;
                if (webpartList != null && !this.mAveDoc.IsView)
                {
                    int sourceCount = webParts.Count;
                    webParts = TrimUncompatibleWebparts(webpartList) as IList;
                    if (sourceCount > 0 && webParts.Count <= 0)
                    {
                        return;
                    }
                }
                RealRestore(webParts, restoreOption, clearAllBeforeRestore);
            }
            catch (AveSecurityTrimingException)
            {
                throw;
            }
            catch (AveException e)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.RealRestoreWebPartFailed, e);
                Dispose();
                Reload();
                if (mAveDoc.ParentSite.SPContextKind != AveContextKind.ClientObjectModel)
                {
                    RealRestore(webParts, restoreOption, clearAllBeforeRestore);
                }
            }
        }

        //365要支持2013的10风格site到2013的13风格site的转移，由于一些webpart类型在13里不存在了，需要把这些webpart过滤掉
        private IList<AveWebPartBaseInfo> TrimUncompatibleWebparts(IList<AveWebPartBaseInfo> webparts)
        {
            if (!mAveDoc.ParentSite.SPSite.IsOnlineSite && mAveDoc.ParentSite.SPSite.SPVersion.StartsWith("15.", StringComparison.OrdinalIgnoreCase))
            {
                var parentSite = mAveDoc.ParentSite;
                bool isOnlineToLocal13 = WrapperConfiguration.RestoreWebPartFromOnlineToLocal && parentSite.SourceSiteInfo.IsOnline && parentSite.SPContextKind == AveContextKind.Server13ObjectModel;
                if (!isOnlineToLocal13)
                {
                    AveWebPartAssemblyFilter webpartFilter = new AveWebPartAssemblyFilter(mAveDoc.TagUrl, mAveDoc.ParentSite.SPSite.SPVersion);
                    webparts = webpartFilter.FilterWebParts(webparts);
                    Report.AddDetails(webpartFilter.FilteredWebParts);
                }
            }
            return webparts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webParts"></param>
        /// <param name="restoreOption"></param>
        /// <param name="clearAllBeforeRestore"></param>
        /// <returns>need reload list</returns>
        private void RealRestore(IList webParts, AveRestoreOption restoreOption, bool clearAllBeforeRestore)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebPartManager.RealRestore"))
            {
                if (mAveDoc.ParentSite.SPContextKind == AveContextKind.ClientObjectModel)
                {
                    try
                    {
                        mManager = mAveDoc.ParentSite.ObjectModelFactory.CreateLimitedWebPartManager(mAveDoc.ParentSite.SPSite, mAveDoc.SPWeb, mAveDoc.SPFile.ServerRelativeUrl);
                    }
                    catch (Exception e)
                    {
                        //Add for SecurityTriming SPFile is null???
                        if (mAveDoc.SPFile == null)
                        {
                            throw new AveSecurityTrimingException(e.Message, e);
                        }
                    }
                }
                else
                {
                    //change for [ADO-54528] (NewExport.aspx Exception)                    
                    try
                    {
                        mManager = mAveDoc.SPFile.GetLimitedWebPartManager(PersonalizationScope.Shared);
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, "Get Web Part Manager Error. {0}", ex.ToString());
                        mManager = null;
                    }
                }
                if (mManager != null)
                {
                    foreach (var webPartInfo in webParts)
                    {
                        if ((webPartInfo is AveWebPartBaseInfo) && ((AveWebPartBaseInfo)webPartInfo).UserID > 0)
                        {
                            mAveDoc.ParentSite.SPMembers.FindMember(((AveWebPartBaseInfo)webPartInfo).UserID, true);
                        }
                    }
                    mManager.Cache = GetWebPartCache();
                    mManager.SetRestoreReport(Report);
                    mManager.RestoreWebParts(webParts, clearAllBeforeRestore);
                    if(mManager.NeedReloadList)
                    {
                        NeedReloadList = true;
                    }
                }
            }
        }

        internal AveWebPartCache GetWebPartCache()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebPartManager.GetWebPartCache"))
            {
                AveWebPartCache cache = new AveWebPartCache();
                AveSPSite aveSite = mAveDoc.ParentSite;
                #region Don't need lock
                cache.ViewInfo = mAveDoc.AveView;
                cache.DefaultUser = aveSite.DefaultUser;
                cache.SourceSiteInfo = aveSite.MappingManager.SiteMappingManager.SourceSiteInfo;
                cache.DestSiteInfo = aveSite.MappingManager.SiteMappingManager.DestSiteInfo;
                cache.IsSitePostRestore = aveSite.IsSitePostRestore;
                cache.SiteManagedMappings = aveSite.MappingManager.SiteMappingManager.SiteManagedMappings;
                #endregion

                cache.SiteMappingManager = aveSite.MappingManager.SiteMappingManager;

                #region 暂时不加Lock。
                //Restore Webpart中只用到了LanguageProcesser中的ListMapping，目前没有List并发还原。
                cache.LanguageProcesser = aveSite.AveLanguageProcesser;
                //反插Restore可能会有问题。
                cache.ListLevelCTIdMapping = aveSite.MappingManager.ListMappingManager.ListLevelCTIdMapping;

                //SiteUserIDMapping和UserMapping的value为什么一样？ 构建了新的Dictionary，并且不会操作Value，不需要加锁。
                cache.SiteUserIDMapping = aveSite.SPMembers.UserAndDomainMapping.EnumUserMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                cache.UserMapping =       aveSite.SPMembers.UserAndDomainMapping.EnumUserMapping().ToDictionary(pair => pair.Key, pair => pair.Value);

                //构建了新的Dictionary不需要加锁。
                cache.SiteUserNameMapping = aveSite.SPMembers.UserAndDomainMapping.EnumCustomUserMapping().ToDictionary(pair => pair.Key, pair => pair.Value);

                //Term only可能会有问题。
                cache.TermIdMapping = aveSite.MetadataService.TermIdMapping;
                cache.TermSetIdMapping = aveSite.MetadataService.TermSetIdMapping;
                cache.TermStoreIdMapping = aveSite.MetadataService.TermStoreIdMapping;

                #endregion

                return cache;
            }
        }

        private void Reload()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebPartManager.Reload"))
            {

                try
                {
                    mAveDoc.ParentSite.ReloadSite();
                    mAveDoc.ParentFolder.ParentList.ParentWeb.ReloadWeb();
                    mAveDoc.ParentFolder.ParentList.ReloadList();
                    if (mAveDoc.ParentFolder.ParentList.SPList != null)
                    {
                        Guid folderId = mAveDoc.ParentFolder.ParentList.SPList.RootFolder.UniqueId;
                        Guid id = mAveDoc.ParentFolder.SPFolder.UniqueId;
                        while (true)
                        {
                            AveSPFolder folder = mAveDoc.ParentFolder;

                            while (true)
                            {
                                if (folder.ParentFolder.SPFolder.UniqueId == folderId)
                                {
                                    folder.ParentFolder.ReloadFolder();
                                    folderId = folder.SPFolder.UniqueId;
                                    break;
                                }
                                folder = folder.ParentFolder;
                            }

                            if (folder.SPFolder.UniqueId.Equals(id))
                            {
                                break;
                            }
                        }
                    }
                    mAveDoc.ParentFolder.ReloadFolder();
                    mAveDoc.ReloadFile();
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReloadAllError, e);
                }


            }

        }

        public string GetValidBaseViewIdStr(IAveList list)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebPartManager.GetValidBaseViewIdStr"))
            {

                string validBaseViewIdStr = string.Empty;
                if (mValidBaseViewIDCollection.ContainsKey(list.BaseTemplate))
                {
                    validBaseViewIdStr = mValidBaseViewIDCollection[list.BaseTemplate];
                }
                else
                {

                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        string uncustomizedViewSchema = list.GetPropertiesXmlForUncustomizedViews();
                        if (!string.IsNullOrEmpty(uncustomizedViewSchema))
                        {
                            doc.LoadXml(uncustomizedViewSchema);
                            StringBuilder sb = new StringBuilder("|");
                            foreach (XmlNode xd in doc.DocumentElement.ChildNodes)
                            {
                                if (xd is XmlElement)
                                {
                                    XmlElement viewProperties = (XmlElement)xd;
                                    if (viewProperties.HasAttribute("BaseViewID"))
                                    {
                                        sb.Append(viewProperties.GetAttribute("BaseViewID"));
                                    }
                                    sb.Append("|");
                                }
                            }
                            validBaseViewIdStr = sb.ToString();
                            mValidBaseViewIDCollection.Add(list.BaseTemplate, validBaseViewIdStr);
                        }
                        else
                        {
                            log.Warn("UnCustomizedViews is null..");
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while GetValidBaseViewId. error:{0}.", e.ToString());
                        //mLog.Warn("An error occurred while GetValidBaseViewId. error:{0}.", e.ToString());
                    }
                    finally
                    {
                        doc.RemoveAll();
                    }
                }
                return validBaseViewIdStr;


            }

        }

        public void Dispose()
        {
            if (mManager != null)
            {
                mManager.Dispose();
                mManager = null;
            }
        }
    }
}