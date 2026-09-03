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
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public abstract class AveBasePostAction : IAvePostAction
    {
        protected IReport reportor = new AveWrapperReport();
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public void Dispose()
        {
            reportor.Dispose();
        }

        public abstract void Excute();

        public IReport GetReport()
        {
            return this.reportor;
        }
        
    }

    public class AveSPSitePostAction : AveBasePostAction
    {
        private AveSPSite site;

        public AveSPSitePostAction(IRestoreableObject siteParam)
        {
            this.site = siteParam as AveSPSite;
        }

        public override void Excute()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AvePostAction.SitePostAction"))
            {

                try
                {
                    try
                    {
                        this.site.ReloadSite();
                        if (!this.site.SPSite.AllowUnsafeUpdates)
                        {
                            this.site.SPSite.AllowUnsafeUpdates = true;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetWebSafeUpdateError, e.ToString());
                    }
                    this.site.RestoreNavNodes(reportor);
                    this.site.RestoreWebMetaInfo();
                    this.site.RestoreWebWelcomePage();
                    this.site.RestoreHiddenSiteProperty();
                    //this.site.RestoreLookupFields();
                    this.site.RestoreUrlIDNeedReplace();
                    this.site.RestoreDataSourceFields();
                    this.site.RestoreUrlNeedPost();
                    this.site.RestoreMasterPageProperty();
                    this.site.RestoreMySiteRecentBlog();
                    this.site.RestoreUnRestoreWebPart(reportor);
                    this.site.ReplaceWebPartContent();
                    this.site.RestoreCalendarSettings();
                    this.site.RestoreLookupFieldValues();
                    this.site.RestoreLinkingUrlFieldValues();
                    this.site.RestoreInfoPathDoc();
                    this.site.RestoreRelatedItemsValue();
                    this.site.RestorePerformancePointProperties();
                    this.site.ScheduleDocument();
                    this.site.RestoreSocialRatingInfo();
                    this.site.PostUpdateSocialItems();
                    this.site.RestoreNintexFormInPostAction();
                    this.site.RestoreNintexFormValueInPostAction();
                    if (this.site.SPMembers != null)
                    {
                        this.site.SPMembers.RestoreGroupOwner();
                    }
                    if (this.site.AveLanguageProcesser != null)
                    {
                        this.site.AveLanguageProcesser.DeleteLanguageFile();
                    }
                    //if (this.site.CheckRestoreOption(this.site.IsNewCreated, AveRestoreMode.RestoreProperty))
                    //{
                    //    this.site.RestoreSiteQuotaSettings();
                    //}
                    if (AvePoint.Common.AveEnv.IsMoss || this.site.SPContextKind == AveContextKind.ClientObjectModel)//暂时无法判断client mode的环境类型
                    {
                        try
                        {
                            if (!this.site.SPSite.AllowUnsafeUpdates)
                            {
                                this.site.SPSite.AllowUnsafeUpdates = true;
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetWebSafeUpdateError, e.ToString());
                        }
                        //this.site.RestoreUnRestoreFileHoldStatus();
                        //this.site.RestoreUnRestoreItemHoldStatus();
                        this.site.RestoreUnRestoreHoldRecord();
                    }

                    this.site.RestoreDefaultContentTypeRequiredProperty();
                    //this.site.RestoreWorkflowStartOptions();
                    //开启alert应该放在最后，防止开了以后还原数据。          
                    AveSPAlert.EnableAllAlerts(this.site);
                    //通过数据库更新list的last modified time.
                    this.site.RestoreListLastModifiedTime();

                    //Morden Page
                    if (this.site.MappingManager.SiteMappingManager.DocumentPostActions.Count > 0)
                    {
                        var processor = this.site.ObjectModelFactory.CreateSharepointDataProcessor(this.site.SPSite, this.site.MappingManager.SiteMappingManager, this.site.SourceSiteInfo, this.site.SPMembers.GetMappingUserLogin);
                        if (processor != null)
                        {
                            processor.PostActionImpl();
                        }
                    }
                    //由于API修改Web 的 modified time后，Sharepoint还会查数据库重新赋值，此方法无效。
                    //this.site.RestoreWebLastModifiedTime();
                    try
                    {
                        if (LS.SPWorkflowProcessor.SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache != null)
                        {
                            LS.SPWorkflowProcessor.SPWorkflowProcessorRuntime.RestoredWorkflowTemplateIdCache.Remove(this.site.SPSite.ID);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while clear workflow template id cache for site {0} ,{1} ,{2}", this.site.SPSite.Url, this.site.SPSite.ID, e);
                    }
                    this.site.RestoreWorkflowStartOption();
                    this.site.RestoreAssignEmailSetting();
                    this.site.RemoveTempMasterPage();
                    this.site.ReplaceMetadataTermSetAndTermPropertyUrl();
                    if(this.site.SPSite.IsOnlineSite)
                    {
                        this.site.RestoreListComplianceTagProperties();
                    }
                    //this.site.ResetPropertyForMordenSite();
                    this.site.ResetDenyPermissionsMask();
                }
                catch (Exception ex)
                {
                    log.Warn("An error occurred when doing post action of site, url: {0}. Reason: {1}", this.site.SPSite.Url, ex.ToString());
                }
            }
        }
    }

    public class AveSPWebPostAction : AveBasePostAction
    {
        private AveSPWeb web;

        public AveSPWebPostAction(IRestoreableObject webParam)
        {
            this.web = webParam as AveSPWeb;
        }

        public override void Excute()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AvePostAction.WebPostAction"))
            {

                try
                {
                    this.web.ReloadWeb();//还原过程中的一些EventReceiver可能会修改web的属性，需要在先reload一下
                    if (!this.web.SPWeb.AllowUnsafeUpdates)
                    {
                        this.web.SPWeb.AllowUnsafeUpdates = true;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetWebSafeUpdateError, e.ToString());
                }
                this.web.ContentTypes.ContentTypeRestorePostAction();
                this.web.RestoreRequestAccessEmail();
                this.web.RestoreThemeCssFolderUrl();
                this.web.RestoreAlternateCSSUrl();
                this.web.RestoreWelcomePage();
                this.web.RestoreSiteLogoUrl();
                this.web.RestoreHiddenPageProperty();
                this.web.RestoreAssociateGroups();
                this.web.RestoreCacheProfileListId();
                this.web.RestoreRelationShipListSetting();
                this.web.RestoreEmailSubmittedRecordsListIDProperty();
                //Used for DocAve5 Site bin Restore, never used in DocAve6
                //this.web.RestoreOriginTitle();
                this.web.RestoreContentOrginazerSetting();
                this.web.RestoreProjectPolicy();
                this.web.RestoreWebIndexedProperty();
                DoWorkflowPostAction();
                this.web.RestoreCacheListCustomActions();
            }
        }

        private void DoWorkflowPostAction()
        {
            try
            {
                WFConflictResolution.Instance.ExecutePostAction(this.web);
                WFConflictResolution.Instance.ClearCache(this.web.SPWeb.Site.ID, this.web.SPWeb.ID);
                if (this.web.HasNintexWF)
                {
                    this.web.UpdateNintexWorkflow();
                }
                this.web.RestoreWFEnable();
                using (var wfReport = WFConflictResolution.Instance.GetReport())
                {
                    this.reportor.AddDetails(wfReport.GetDetails());
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.WorkflowPostAction, e);
            }
        }
    }

    public class AveSPListPostAction : AveBasePostAction
    {
        private AveSPList list;

        public AveSPListPostAction(IRestoreableObject listParam)
        {
            this.list = listParam as AveSPList;
        }

        public override void Excute()
        {
            using (new AvePerformanceScope("Restore.AvePostAction.ListPostAction"))
            {
                try
                {
                    if (list.isPosted)
                    {
                        return;
                    }
                    list.isPosted = true;
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
                    list.RestoreListSetting();
                    list.RestoreListRootFolderProperties();
                    list.RestoreUnRestoreWebPart(reportor);
                    list.RestoreMetadataNavigationSettings();
                    list.RestoreDocumentTemplateUrl();
                    //list.RestoreListRssViewField();
                    list.AveFields.FieldRestorePostAction();
                    list.AveContentTypes.ContentTypeRestorePostAction();
                    list.UpdateDefaultValue();
                    if (list.SPList != null)
                    {
                        //list.DeleteItemsForCategory();
                        list.AddDefaultViewUrl(list.SPList.DefaultViewUrl);
                        if (list.ListInfo != null)
                        {
                            list.ParentWeb.ParentSite.ReplaceWebPartContent(list.ListInfo.Id);
                        }
                        list.RestoreDocumentsFromDropOffZone(list.ParentWeb.ParentSite.AutoDropOffContentOrganizer);
                        list.UpdateDiscussionLikedCount();
                    }
                    list.RestoreDocumentSetMetaInfo();
                    //master page删除不掉时，使用了move临时处理，需要在postAction中将move的文件删除。
                    list.RemoveTempMasterPage();
                    list.UpdateMicroFeedItem();
                    list.UpdateDefaultView();
                    list.UpdateSpotlightViews();
                    list.RestoreUnRestoreAlerts();
                    list.ActiveSandBoxFeature();
                    WFConflictResolution.Instance.ClearCache(Guid.Empty, Guid.Empty);
                    if (list.SPList != null && (int)list.SPList.BaseTemplate == 171)
                    {
                        list.UpdateTaskEventReceiverSynchronous(false);
                    }
                }
                catch (Exception e)
                {
                    log.Warn(e.ToString());
                }
                finally
                {
                    if (list.SPList != null)
                    {
                        list.ParentSite.MappingManager.SiteMappingManager.AddListLevelContentTypeIdMapping(list.SPList.ID,
                            list.ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping);
                    }
                    list.ParentSite.MappingManager.ListMappingManager.Dispose();
                }
            }
        }
    }
}
