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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource;
using System.Xml;
using AvePoint.Wrapper.Restore.Core;

namespace AvePoint.Wrapper.Restore
{
    public static class AvePostAction
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static void Output()
        {
            var time = AveReusableStreamProvider.Timer.Elapsed;
            var times = AveReusableStreamProvider.Times;
            log.Info($"AveReusableStreamProvider Execute {times} times,TimeCost:{time},Average:{time.TotalSeconds/times} seconds.");
        }
        public static void SitePostAction(AveSPSite AveSite)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AvePostAction.SitePostAction"))
            {
#endif
            Output();
                try
                {
                    try
                    {
                        if (!AveSite.SPSite.AllowUnsafeUpdates)
                        {
                            AveSite.SPSite.AllowUnsafeUpdates = true;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetWebSafeUpdateError, e.ToString());
                    }
                    AveSite.RestoreVariationsSettings();
                    AveSite.RestoreNavNodes();
                    AveSite.ResotreWebMetaInfo();
                    AveSite.RestoreHiddenSiteProperty();
                    //AveSite.RestoreLookupFields();
                    AveSite.RestoreUrlIDNeedReplace();
                    AveSite.RestoreDependentUrlFieldValues();
                    AveSite.RestoreDataSourceFields();
                    AveSite.RestoreUrlNeedPost();
                    AveSite.RestoreMasterPageProperty();
                    AveSite.RestoreMySiteRecentBlog();
                    AveSite.RestoreUnRestoreWebPart();
                    AveSite.RestoreCalendarSettings();
                    AveSite.RestoreLookupFieldValues();
                    AveSite.FieldPostCache.FieldCacheSitePostAction();
                    AveSite.RestoreProjectWebGuidValues();
                    AveSite.RestoreInfoPathDoc();
                    AveSite.RestorePerformancePointProperties();
                    AveSite.UserCustomActionSerializer.RestoreFromCache();
                    AveSite.RestoreProjectTimeline();
                    AveSite.RestoreNintexFormInPostAction();
                    AveSite.RestoreNintexFormValueInPostAction();
                    AveSite.RestoreEndListSettings();
                    //if (AveSite.CheckRestoreOption(AveSite.IsNewCreated, AveRestoreMode.RestoreProperty))
                    //{
                    //    AveSite.RestoreSiteQuotaSettings();
                    //}
                    if (AveSPEnv.IsMoss)
                    {
                        try
                        {
                            if (!AveSite.SPSite.AllowUnsafeUpdates)
                            {
                                AveSite.SPSite.AllowUnsafeUpdates = true;
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetWebSafeUpdateError, e.ToString());
                        }
                        //AveSite.RestoreUnRestoreFileHoldStatus();
                        //AveSite.RestoreUnRestoreItemHoldStatus();
                        AveSite.RestoreUnRestoreHoldRecord();
                    }
                    //开启alert应该放在最后，防止开了以后还原数据。          
                    AveSPAlert.EnableAllAlerts(AveSite);
                    //通过数据库更新list的last modified time.
                    AveSite.RestoreListLastModifiedTime();

                    SharePointDocumentDataProcessor.PostAction(AveSite);

                    //由于API修改Web 的 modified time后，Sharepoint还会查数据库重新赋值，此方法无效。
                    //AveSite.RestoreWebLastModifiedTime();

                    //不能够在sitepostaction进行这种操作，因为第一个web需要remove，第二个web可能就不需要remove了；这种操作添加到webpostaction
                    //if (AveSite.NeedClosePublishingFeature)
                    //{
                    //    Guid publishingFeature = new Guid("f6924d36-2fa8-4f0b-b16d-06b7250180fa");
                    //    AveSite.SPSite.Features.Remove(publishingFeature,true);
                    //}
                }
                catch (Exception ex)
                {
                    log.Error("An error occurred when post site action,due to {0}",ex);
                    //mLog.Log(AveLogSeverity.Warn, "WP10RTPostAct062", AveSite.SPSite.Url, ex);
                    log.Warn("An error occurred when restoring lookup fields of site, url: {0}. Reason: {1}", AveSite.SPSite.Url, ex.ToString());
                }
                //try
                //{
                //    AveSite.RestoreLookupFieldValues(false);
                //}
                //catch (Exception ex)
                //{
                //   mLog.Log(AveLogSeverity.Warn, "WP10RTPostAct071", AveSite.SPSite.Url, ex);
                //    mLog.Warn("An error occurred when restoring lookup fields values of site, url: {0}. Reason: {2}", AveSite.SPSite.Url, ex.ToString());
                //}
#if PerformanceLog               
            }
#endif
        }

        public static void WebPostAction(AveSPWeb AveWeb, bool isLast)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AvePostAction.WebPostAction"))
            {
#endif
                try
                {
                    AveWeb.ReloadWeb();//还原过程中的一些EventReceiver可能会修改web的属性，需要在先reload一下
                    if (!AveWeb.SPWeb.AllowUnsafeUpdates)
                    {
                        AveWeb.SPWeb.AllowUnsafeUpdates = true;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetWebSafeUpdateError, e.ToString());
                }
                //if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreProperty))//是否还原受Configuration选项控制
                //{
                    AveWeb.RestoreThemeCssFolderUrl();
                    AveWeb.RestoreSiteLogoUrl();
                //}
                if (!WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore)
                {
                    //End User Restore does not need restore RequestAccessEmail
                    AveWeb.RestoreRequestAccessEmail();
                    //for community site
                    AveWeb.ReCalculateForCommunitySite();
                    AveWeb.RestoreWebIndexedProperty();
                    AveWeb.UpdateDesignManagerViewSetting();
                    AveWeb.RestoreWorkflowStartOptions();
                }
                AveWeb.RestoreAlternateCSSUrl();
                AveWeb.RestoreAuthor();
                AveWeb.RestoreWelcomePage();
                AveWeb.RestoreHiddenPageProperty();
                AveWeb.RestorePostUserInfo();
                if (AveWeb.CheckRestoreOption(AveWeb.IsNewCreated, AveRestoreMode.RestoreSecurity))
                {
                    AveWeb.RestoreAssociateGroups();
                }
                AveWeb.RestoreCacheProfileListId();
                AveWeb.RestoreRelationShipListSetting();
                AveWeb.RestoreEmailSubmittedRecordsListIDProperty();
                AveWeb.RestoreOriginTitle();
                AveWeb.RestoreContentOrginazerSetting();

                AveWeb.ParentSite.PWASettings.RevertRequiredCustomField();

                if (AveWeb.ParentSite.SPMembers != null)
                {
                    AveWeb.ParentSite.SPMembers.RestoreGroupOwner();
                }
                try
                {
                    WFConflictResolution.Instance.ExecutePostAction(AveWeb.ParentSite);
                }
                catch (Exception e) 
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.WorkflowPostAction, e);
                }

                if (AveWeb.ParentSite.NeedClosePublishingFeature)
                {
                    Guid publishingFeature = new Guid("f6924d36-2fa8-4f0b-b16d-06b7250180fa");
                    AveWeb.ParentSite.SPSite.Features.Remove(publishingFeature, true);
                }
                WFConflictResolution.ClearResolution(); //SAAS-21766 在还原一个新的web之前，dispose上一个web使用的workflow信息
#if PerformanceLog
            }
#endif
        }

        public static void ListPostAction(AveSPList list)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AvePostAction.ListPostAction"))
            {
#endif
                try
                {
                    list.ResolvePostActions(list);
                    try
                    {
                        if (!list.ParentWeb.SPWeb.AllowUnsafeUpdates)
                        {
                            list.ParentWeb.SPWeb.AllowUnsafeUpdates = true;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetWebSafeUpdateError, e.ToString());
                    }
                    if (list.SPList != null && list.ParentWeb.ParentSite.IsListIncludeEnableAssignEmail(list.SPList))
                    {
                        list.ParentWeb.ParentSite.UpdateListRestoreFinishedTimePoint(list.SPList);
                    }
                    list.RestoreListSetting();
                    list.RevertListVersionSettings();
                    list.RestoreListRootFolderProperties();
                    list.RestoreUnRestoreWebPart();
                    list.RestoreWelComePage();
                    list.RestoreMetadataNavigationSettings();
                    list.RestoreDocumentTemplateUrl();
                    list.AveContentTypes.RestoreContentTypesPostAction();
                    list.RestoreListRssViewField();
                    //list.RestoreFieldIndexes();  不支持
                    list.RestoreSolutionStatus();
                    list.RestoreWorkflowStartOptions();  //SAAS-13078 支持还原Start Options
                    list.ParentSite.FieldPostCache.FieldCachePostAction(list.SPList, PostActionType.ListPostAction);
                    if (list.CheckRestoreOption(list.IsNewCreated, AveRestoreMode.RestoreProperty))//是否还原受Configuration选项控制
                    {
                        list.RestoreSolutionFeatures();
                        list.RestoreListComplianceInfo();
                    }
                    list.UpdateDefaultValue();
                    if (list.SPList != null)
                    {
                        //list.DeleteItemsForCategory();
                        list.AddDefaultViewUrl(list.SPList.DefaultViewUrl);
                    }

                    if (list.ListInfo != null)
                    {
                        list.ParentWeb.ParentSite.RestoreLookupFields(list.ListInfo.Id);
                        list.ParentWeb.ParentSite.RestoreLookupFieldValues(list.ListInfo.Id);
                        if (list.SPList != null)
                        {
                            list.ParentWeb.ParentSite.RestoreLookupFieldValues(list.SPList.ID);
                        }
                    }
                    if (list.SPList != null && list.SPList.RootFolder != null)
                    {
                        list.ParentWeb.ParentSite.RestoreDependentUrlFieldValues(list.SPList.RootFolder.ServerRelativeUrl, list.SPList.ID);
                    }

                   

                    if (list.SPList != null)
                    {
                        list.ParentWeb.ParentSite.RestoreUnupdateFile(list.ListInfo.Id);
                    }
                    if (list.SPList != null)
                    {
                        list.RestoreDocumentsFromDropOffZone(list.ParentWeb.ParentSite.AutoDropOffContentOrganizer);
                    }
                    AveFieldHelper.UpdateFieldSchemaIdMappingProperty(list.SPList, list.AveFields.FieldMapping.EnumFieldSchemaMapping().ToDictionary(pair => pair.Key, pair => pair.Value));
                    if (null != list.AveContentTypes.ContentTypeHelper)
                    {
                        list.AveContentTypes.ContentTypeHelper.UpdateContentTypeIdMappingProperty(list.AveContentTypes.ContentTypeMapping.EnumContentTypeIdMapping().ToDictionary(pair => pair.Key, pair => pair.Value));
                    }
                    list.RestoreDocumentSetMetaInfo();
                    //master page删除不掉时，使用了move临时处理，需要在postAction中将move的文件删除。
                    list.RemoveTempMasterPage();
                    
                }
                catch (Exception e)
                {
                    log.Warn(e.ToString());
                }
                finally
                {
                    list.ParentSite.MappingManager.ListMappingManager.Dispose();
                    list.Dispose();
                }
#if PerformanceLog
            }
#endif
        }

        public static void ProjectPostAction(AveSPSite AveSite)
        {
            AveSite.PWASettings.RestoreStage();
            AveSite.PWASettings.RestoreEnterpriseProjectType();
        }
    }

    public interface IPostAction<T>
    {
        bool Resolve(T item);
    }

    public abstract class CommonPostAction<T> : IPostAction<T>
    {
        protected static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected StringBuilder history;

        public CommonPostAction()
        {

        }

        public abstract bool Resolve(T item);

        protected void Log(string format, params object[] args)
        {
            if (history == null)
            {
                history = new StringBuilder();
            }

            if (args != null && args.Length >= 1)
            {
                history.AppendFormat(format, args);
                history.AppendLine();
            }
            else
            {
                history.AppendLine(format);
            }
        }

        protected string History
        {
            get
            {
                if (history != null)
                {
                    return history.ToString();
                }
                return string.Empty;
            }
        }
    }

    class DocumentSetWelcomeViewPostAction : CommonPostAction<AveSPList>
    {
        string contentTypeId;
        string namespaceUri;
        string xml;

        public DocumentSetWelcomeViewPostAction(string contentTypeId, string namespaceUri, string xml)
        {
            this.contentTypeId = contentTypeId;
            this.namespaceUri = namespaceUri;
            this.xml = xml;
        }

        public static bool ReplaceViewId(string namespaceUri, XmlDocument document, AveSPList list, Action<string, object[]> log)
        {
            var changed = false;
            var item = document.DocumentElement;
            //var namespaceManager = new XmlNamespaceManager(document.NameTable);
            //namespaceManager.AddNamespace("wpv", namespaceUri);
            //var item = document.DocumentElement.SelectSingleNode("./wpv:WelcomePageView", namespaceManager) as XmlElement;
            mLog.Info("ReplaceViewId in DocumentSet ContentType:BeforeReplace:{0}",document.OuterXml);
            if (item != null)
            {
                Guid viewId;
                var viewIdString = item.GetAttribute("ViewId");
                if (Guid.TryParse(viewIdString, out viewId))
                {
                    Guid targetId;
                    lock(list.ParentSite.MappingManager.SiteMappingManager.ViewGuidMapping)
                    {
                        if (list.ParentSite.MappingManager.SiteMappingManager.ViewGuidMapping.TryGetValue(viewId, out targetId))
                        {
                            item.SetAttribute("ViewId", targetId.ToString("D"));
                            if (log != null)
                            {
                                log("Find the target view id:{0} with source id:{1}", new object[] { targetId, viewId });
                            }
                            changed = true;
                        }
                        else
                        {
                            if (log != null)
                            {
                                log("Cannot find the target view id with source id:{0}", new object[] { viewId });
                            }
                        }
                    }
                }
                else
                {
                    if (log != null)
                    {
                        log("Parse View Id:{0} to GUID failed.", new object[] { viewIdString });
                    }
                }
            }
            //else
            //{
            //    if (log != null)
            //    {
            //        log("Cannot find wpv:WelcomePageView element.", null);
            //    }
            //}
            mLog.Info("ReplaceViewId in DocumentSet ContentType,changed:{0}:AfterReplace:{1}", changed,document.OuterXml,changed);
            return changed;
        }

        public override bool Resolve(AveSPList list)
        {
            var document = new XmlDocument();
            document.LoadXml(xml);

            if (ReplaceViewId(namespaceUri, document, list, Log))
            {
                //Log("The changed xml:{0}", document.OuterXml);
                var contentType = list.SPList.ContentTypes.GetById(contentTypeId);
                contentType.XmlDocuments.Delete(namespaceUri);
                contentType.XmlDocuments.Add(document);
                contentType.Update();

                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("Content Type Id:{0}, namespace:{1}, xml:{2}, history:{3}", contentTypeId, namespaceUri, xml, History);
        }
    }
}
