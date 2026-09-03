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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Common;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Collections;

using System.Xml;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPWebPartManager : IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveSPDoc mAveDoc;
        //private AveSqlConnection mSqlCon;
        private IAveLimitedWebPartManager mManager;
        private Dictionary<AveListTemplateType, string> mValidBaseViewIDCollection = new Dictionary<AveListTemplateType, string>();
        //private AveSPWebPart mWebPart;
        //private AveSPViewWebPart mViewWebPart;

        //private AveRestoreOption mRestoreOption;

        public AveSPWebPartManager(AveSPDoc aveDoc)
        {
            mAveDoc = aveDoc;
            //mSqlCon = aveDoc.AveSite.SqlConn;

        }

        /// <param name="needCheckDelete">当我们在进行List Pose Action来还原没有还原的ListWebPart时，不应该删除已经存在的Web Part</param>
        public void Restore(IList webParts, AveRestoreOption restoreOption, bool needCheckDelete)
        {
            try
            {
                RealRestore(webParts, restoreOption, needCheckDelete);
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
                    RealRestore(webParts, restoreOption, needCheckDelete);
                }
            }
        }

        private void RealRestore(IList webParts, AveRestoreOption restoreOption, bool needCheckDelete)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebPartManager.RealRestore"))
            {
#endif
                if (mAveDoc.ParentSite.SPContextKind == AveContextKind.ClientObjectModel)
                {
                    try
                    {
                        mManager = mAveDoc.ParentSite.ObjectModelFactory.CreateLimitedWebPartManager(mAveDoc.ParentSite.SPSite, mAveDoc.Web, mAveDoc.SPFile.ServerRelativeUrl);
                    }
                    catch (Exception e)
                    {
                        //Add for SecurityTriming SPFile is null???
                        if (mAveDoc.SPFile == null)
                        {
                            throw new AveSecurityTrimingException(e.Message,e);
                        }
                    }
                }
                else
                {
                    mManager = mAveDoc.SPFile.GetLimitedWebPartManager(AvePersonalizationScope.Shared);
                }
                mManager.Cache = GetWebPartCache();
                mManager.SetRestoreReport(new AveWrapperReport());
                mManager.RestoreWebParts(webParts, needCheckDelete);
#if PerformanceLog
            }
#endif
        }

        internal AveWebPartCache GetWebPartCache()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebPartManager.GetWebPartCache"))
            {
#endif
                AveWebPartCache cache = new AveWebPartCache();
                AveSPSite aveSite = mAveDoc.ParentSite;
                //Dictionary<string, IAveContentType> ctMapping = null;
                Dictionary<string, string> fieldMapping = null;
                Dictionary<string, string> fieldDisplayNameMapping = null;
                if (mAveDoc.AveSPItem != null && mAveDoc.AveSPItem.ParentList != null)
                {
                    //ctMapping = mAveDoc.AveSPItem.ParentList.ParentWeb.ParentSite.MappingManager.ListMappingManager.ListLevelCTMapping;//SAAS-21766 去掉保存CT，只保存CT ID，这个没有引用到，所以去掉
                    if (mAveDoc.AveSPItem.ParentList.AveFields != null)
                    {
                        fieldMapping = mAveDoc.AveSPItem.ParentList.AveFields.FieldMapping.EnumFieldInternalNameMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                        fieldDisplayNameMapping = mAveDoc.AveSPItem.ParentList.AveFields.FieldMapping.EnumFieldDisplayNameMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                    }                    
                }

                cache.WebPartMapping = mAveDoc.ParentSite.MappingManager.SiteMappingManager.WebPartMapping;
                cache.ViewInfo = mAveDoc.AveView;
                cache.ListIdMapping = aveSite.MappingManager.SiteMappingManager.ListIdMapping;
                cache.UnRestoreWebPartCache = aveSite.MappingManager.SiteMappingManager.UnRestoreWebPartCache;
                cache.SiteUrlMapping = aveSite.MappingManager.SiteMappingManager.SiteUrlMapping;
                cache.WebUrlMapping = aveSite.MappingManager.SiteMappingManager.WebUrlMapping;
                cache.SiteManagedMappings = aveSite.MappingManager.SiteMappingManager.SiteManagedMappings;
                cache.NeedWebPartIDMapping = aveSite.MappingManager.SiteMappingManager.NeedWebPartIDMapping;
                cache.WebPartTypeIDMapping = aveSite.MappingManager.SiteMappingManager.WebPartTypeIDMapping;
                cache.WebIDMapping = aveSite.MappingManager.SiteMappingManager.WebIDMapping;
                cache.DefaultUser = aveSite.DefaultUser;
                cache.LanguageProcesser = aveSite.AveLanguageProcesser;
                cache.AudienceIDMapping = aveSite.MappingManager.SiteMappingManager.AudienceIDMapping;
                cache.SiteUserIDMapping = aveSite.SPMembers.UserAndDomainMapping.EnumUserMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                cache.SiteUserNameMapping = aveSite.SPMembers.UserAndDomainMapping.EnumCustomUserMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                //cache.ListLevelCTMapping = ctMapping;
                cache.ListLevelCTIdMapping = aveSite.MappingManager.ListMappingManager.ListLevelCTIdMapping;
                cache.DesListCTIdMapping = aveSite.MappingManager.ListMappingManager.DesListCTIdMapping;
                cache.UserMapping = aveSite.SPMembers.UserAndDomainMapping.EnumUserMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                cache.ViewGuidMapping = aveSite.MappingManager.SiteMappingManager.ViewGuidMapping;
                cache.NeedResetCalendarSettingsViews = aveSite.MappingManager.SiteMappingManager.NeedResetCalendarSettingsViews;
                cache.FieldInternalNameMapping = fieldMapping;
                cache.FieldDisplayNameMapping = fieldDisplayNameMapping;
                cache.ListFieldsMapping = aveSite.MappingManager.SiteMappingManager.ListFieldsMapping;
                cache.ListUrlMapping = aveSite.MappingManager.SiteMappingManager.ListUrlMapping;
                cache.FullUrlMapping = aveSite.MappingManager.SiteMappingManager.AbsoluteUrlMapping;
                cache.SourceSiteInfo = aveSite.MappingManager.SiteMappingManager.SourceSiteInfo;
                cache.DestSiteInfo = aveSite.MappingManager.SiteMappingManager.DestSiteInfo;
                cache.TermIdMapping = aveSite.MetadataService.TermIdMapping;
                cache.ListCTIdMapping = aveSite.MappingManager.SiteMappingManager.ListContentTypeIdMapping;
                cache.ProjectCustomFieldIdMapping = aveSite.MappingManager.ProjectMappingManager.CustomFieldIdMapping;
                return cache;
#if PerformanceLog
            }
#endif
        }

        private void Reload()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebPartManager.Reload"))
            {
#endif
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
                    mAveDoc.ParentFolder.Reload();
                    mAveDoc.AveSPItem.ReloadFile();
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReloadAllError, e);
                }

#if PerformanceLog
            }
#endif
        }

        public string GetValidBaseViewIdStr(IAveList list)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebPartManager.GetValidBaseViewIdStr"))
            {
#endif
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

#if PerformanceLog
            }
#endif
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