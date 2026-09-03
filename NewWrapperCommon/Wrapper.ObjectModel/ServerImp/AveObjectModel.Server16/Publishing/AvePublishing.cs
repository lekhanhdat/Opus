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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint.Publishing;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server16
{
    [AveCodeReview("2012/01/31", "Navy.Li@avepoint.com", "yanjun.wang@AvePoint.com", new string[3] { CodeReviewConstants.CHECK_LIST_ID_FA_10, CodeReviewConstants.CHECK_LIST_ID_CO_3, CodeReviewConstants.CHECK_LIST_ID_CS_1 }, null, true)]
    class AvePublishing:IAvePublishing
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AvePublishing));
        private AveSite mSite;

        public AvePublishing(IAveSite site)
        {
            mSite = site as AveSite;
        }

        public Guid AverageRatings
        {
            get
            {
                if (AveEnv.IsPublishing)
                {
                    //代表Microsoft.SharePoint.Publishing.FieldId.AverageRatings;
                    return new Guid("5a14d1ab-1513-48c7-97b3-657a5ba6c742");
                }
                return Guid.Empty;
            }
        }

        public Guid RatingsCount
        {
            get
            {
                if (AveEnv.IsPublishing)
                {
                    //代表 Microsoft.SharePoint.Publishing.FieldId.RatingsCount
                    return new Guid("b1996002-9167-45e5-a4df-b2c41c6723c7");
                }
                return Guid.Empty;
            }
        }

        public Guid LikedBy
        {
            get
            {
                if (AveEnv.IsPublishing)
                {
                    //代表 Microsoft.SharePoint.Publishing.FieldId.RatingsCount
                    return new Guid("2cdcd5eb-846d-4f4d-9aaf-73e8e73c7312");
                }
                return Guid.Empty;
            }
        }

        public Guid LikesCount
        {
            get
            {
                if (AveEnv.IsPublishing)
                {
                    //代表 Microsoft.SharePoint.Publishing.FieldId.RatingsCount
                    return new Guid("6e4d832b-f610-41a8-b3e0-239608efda41");
                }
                return Guid.Empty;
            }
        }

        public Guid RatedBy
        {
            get
            {
                if (AveEnv.IsPublishing)
                {
                    //代表 Microsoft.SharePoint.Publishing.FieldId.RatedBy
                    return new Guid("4d64b067-08c3-43dc-a87b-8b8e01673313");
                }
                return Guid.Empty;
            }
        }

        public Guid Ratings
        {
            get
            {
                if (AveEnv.IsPublishing)
                {
                    //代表 Microsoft.SharePoint.Publishing.FieldId.Ratings
                    return new Guid("434f51fb-ffd2-4a0e-a03b-ca3131ac67ba");
                }
                return Guid.Empty;
            }
        }

        public void SetWelcomePage(IAveWeb web, string welcomePageUrl)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AvePublishing.SetWelcomePage"))
            {

                try
                {
                    if (AveEnv.IsPublishing && web.IsPublish)
                    {
                        using (IAvePublishingWeb currentPublishingWeb = new AvePublishingWeb(web))
                        {
                            IAveFile welcomePage = web.GetFile(welcomePageUrl);
                            if (welcomePage.Exists)
                            {
                                currentPublishingWeb.DefaultPage = welcomePage;
                                currentPublishingWeb.Update();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.WelcomePageSettingSetFailed, welcomePageUrl, e);
                }

            }

        }

        public void SetWebMasterPageInfo(AveWebMasterPageInfo pageInfo, IAveWeb web, string destPageUrl, bool changeAlternateCssUrl = true)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AvePublishing.SetWebMasterPageInfo"))
            {

                //这里原先判断的是IsPublishingWeb，但是07允许PublishingSite开启Master Page设置
                //所以改用判断是否是IsPublishingSite(web.Site)
                if (!WrapperRuntime.CurrentContext.IsMoss)// || !web.Site.IsPublish),DB Attach的Master Page与新建的默认值不同，不开启也需要还原
                {
                    return;
                }
                //因为PublishingWeb和普通Web对Master Page设置的更新不同，所以加以区分
                if (web.IsPublish)
                {
                    IAvePublishingWeb pWeb = new AvePublishingWeb(web);
                    string destCPageUrl = pageInfo.CPageUrl;
                    if (pWeb.CustomMasterUrl.Value != destCPageUrl || pWeb.CustomMasterUrl.IsInheriting != pageInfo.CInheriting)
                    {
                        if (!string.IsNullOrEmpty(destCPageUrl))
                        {
                            try
                            {
                                IAveFile cPageFile = web.Site.RootWeb.GetFile(destCPageUrl);
                                if (cPageFile.Exists)
                                {
                                    pWeb.CustomMasterUrl.SetValue(destCPageUrl);
                                }
                                else
                                {
                                    logger.Warn("Page File doesn't exist. Url: {0} WebId: {1}", destCPageUrl, web.ID);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.MasterPageSettingSetFailed,
                                    pageInfo == null ? string.Empty : pageInfo.PageUrl, e);
                            }
                        }
                        //if (pageInfo.CInheriting && !destCPageUrl.Equals(web.Site.RootWeb.CustomMasterUrl, StringComparison.OrdinalIgnoreCase))
                        //{
                        //    //If we have to set customerMasterUrl inherited, the customerMasterUrl value should be the same between current and parent in source(Doc-67530).
                        //    //mLog.Warn("'System Master Page' url is different in source site although inherited, so inherited cannot be restored in {0}", web.Url);
                        //}
                        //else
                        //{
                        if (!web.IsRootWeb)
                        {
                            pWeb.CustomMasterUrl.SetInherit(pageInfo.CInheriting, false);
                        }
                        //}
                    }

                    string destMPageUrl = pageInfo.MPageUrl;
                    if (pWeb.MasterUrl.Value != destMPageUrl || pWeb.MasterUrl.IsInheriting != pageInfo.MInheriting)
                    {
                        if (!string.IsNullOrEmpty(destMPageUrl))
                        {
                            try
                            {
                                IAveFile mPageFile = web.Site.RootWeb.GetFile(destMPageUrl);
                                if (mPageFile.Exists)
                                {
                                    pWeb.MasterUrl.SetValue(destMPageUrl);
                                }
                                else
                                {
                                    logger.Warn("File doesn't exist. Url: {0} web: {1}", destMPageUrl, web.ID);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.MasterPageSettingSetFailed,
                                    pageInfo == null ? string.Empty : pageInfo.PageUrl, e);
                            }
                        }
                        //if (pageInfo.MInheriting && !destMPageUrl.Equals(web.Site.RootWeb.MasterUrl, StringComparison.OrdinalIgnoreCase))
                        //{
                        //    //If we have to set MasterUrl inherited, the MasterUrl value should be the same between current and parent in source(Doc-67530).
                        //    //mLog.Warn("'Site Master Page' url is different in source site although inherited, so inherited cannot be restored in {0}", web.Url);
                        //}
                        //else
                        //{
                        if (!web.IsRootWeb)
                        {
                            pWeb.MasterUrl.SetInherit(pageInfo.MInheriting, false);
                        }
                        //}
                    }
                    if (!pWeb.AlternateCssUrl.Value.Equals(destPageUrl, StringComparison.OrdinalIgnoreCase) || pWeb.AlternateCssUrl.IsInheriting != pageInfo.Inheriting)
                    {
                        if (changeAlternateCssUrl)
                        {
                            if (!web.IsRootWeb)
                            {
                                pWeb.AlternateCssUrl.SetInherit(pageInfo.Inheriting, false);
                            }
                            else
                            {
                                web.AlternateCssUrl = destPageUrl;
                            }
                        }
                    }
                }
                else
                {
                    //普通的web不处理继承关系，因为如果存在PageUrl都会更新
                    string destCPageUrl = pageInfo.CPageUrl;
                    if (web.CustomMasterUrl != destCPageUrl)
                    {
                        if (!string.IsNullOrEmpty(destCPageUrl))
                        {
                            try
                            {
                                IAveFile cPageFile = web.Site.RootWeb.GetFile(destCPageUrl);
                                if (cPageFile.Exists)
                                {
                                    web.CustomMasterUrl = destCPageUrl;
                                }
                                else
                                {
                                    logger.Warn("CPageFile does not exist. Page url: {0} Web ID: {1}", destCPageUrl, web.ID);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.MasterPageSettingSetFailed,
                                    pageInfo == null ? string.Empty : pageInfo.PageUrl, e);
                            }
                        }
                    }

                    string destMPageUrl = pageInfo.MPageUrl;
                    if (web.MasterUrl != destMPageUrl)
                    {
                        if (!string.IsNullOrEmpty(destMPageUrl))
                        {
                            try
                            {
                                IAveFile mPageFile = web.Site.RootWeb.GetFile(destMPageUrl);
                                if (mPageFile.Exists)
                                {
                                    web.MasterUrl = destMPageUrl;
                                }
                                else
                                {
                                    logger.Warn("MPageFile does not exist. Url: {0}", destMPageUrl, web.ID);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.MasterPageSettingSetFailed,
                                    pageInfo == null ? string.Empty : pageInfo.PageUrl, e);
                            }
                        }
                    }
                }
                web.Update();//升级上来的Web和不是PublishingWeb需要Update()

            }

        }

        public static bool IsPublishingWeb(IAveWeb web)
        {
            return IsPublishingWeb(web as AveWeb);
        }

        public static bool IsPublishingWeb(AveWeb web)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AvePublishing.IsPublishingWeb"))
            {

                if (AveEnv.IsPublishing)
                {
                    return AveSPUtility.GetBooleanProperty(web.Web.AllProperties, "__PublishingFeatureActivated", false);
                    //return PublishingWeb.IsPublishingWeb(((AveWeb)web).Web);
                }
                return false;

            }

        }

        public static bool IsPublishingSite(IAveSite site)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AvePublishing.IsPublishingSite"))
            {

                if (AveEnv.IsPublishing)
                {
                    return PublishingSite.IsPublishingSite(((AveSite)site).Site);
                }
                return false;

            }

        }

        public static AveWebSettingInfo ProcessWebSettingInfo(AveWebSettingInfo webSettingInfo, IAveWeb web)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AvePublishing.ProcessWebSettingInfo"))
            {

                if (AveEnv.IsPublishing)
                {
                    PublishingWeb pWeb = PublishingWeb.GetPublishingWeb((web as AveWeb).Web);
                    if (pWeb != null)
                    {
                        webSettingInfo.InheritAlertCss = pWeb.AlternateCssUrl.IsInheriting;
                        webSettingInfo.InheritAlertCssUrl = pWeb.AlternateCssUrl.Value;
                        webSettingInfo.CInheriting = pWeb.CustomMasterUrl.IsInheriting;
                        webSettingInfo.CPageUrl = pWeb.CustomMasterUrl.Value;
                        webSettingInfo.MInheriting = pWeb.MasterUrl.IsInheriting;
                        webSettingInfo.MPageUrl = pWeb.MasterUrl.Value;
                    }
                }
                return webSettingInfo;

            }

        }
    }
}
