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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.Client;

namespace AvePoint.ObjectModel.Common
{
    class AvePublishing : AveClientObject, IAvePublishing
    {
        private AveSite mSite;
        private IAveRequest mRequest;
        static AveLogger mLogger = AveLogger.GetInstance(typeof(AvePublishing));

        public AvePublishing(IAveSite site)
        {
            mSite = site as AveSite;
            mRequest = mSite.Request;
        }

        public Guid AverageRatings
        {
            get
            {
                if (mSite.IsPublish)
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
                if (mSite.IsPublish)
                {
                    //代表 Microsoft.SharePoint.Publishing.FieldId.RatingsCount
                    return new Guid("b1996002-9167-45e5-a4df-b2c41c6723c7");
                }
                return Guid.Empty;
            }
        }

        public void SetWelcomePage(IAveWeb web, string welcomePageUrl)
        {
            try
            {
                if (mSite.IsPublish && web.IsPublish)
                {
                    IAvePublishingWeb currentPublishingWeb = new AvePublishingWeb(mSite, web as AveWeb, mRequest.GetPublishingWeb(web.ServerRelativeUrl));
                    IAveFile welcomePage = web.GetFile(welcomePageUrl);
                    currentPublishingWeb.DefaultPage = welcomePage;
                    currentPublishingWeb.Update();
                }
            }
            catch (AveSecurityTrimingException)
            {
                throw;
            }
            catch (Exception e)
            {
                mLogger.Debug(AveObjectModel_CommonResource.SetWelcomPageError, welcomePageUrl, e.ToString());
                //
            }
        }

        public void SetWebMasterPageInfo(AveWebMasterPageInfo pageInfo, IAveWeb web, string destPageUrl)
        {
            if (web.Site.APIType == AveAPIType.BPOS_S)
            {
                SetMasterPageSettingByClient(pageInfo, web, destPageUrl);
            }
            else
            {
                SetMasterPageSettingBySolution(pageInfo, web, destPageUrl);
            }
        }

        private void SetMasterPageSettingByClient(AveWebMasterPageInfo pageInfo, IAveWeb web, string destPageUrl)
        {
            if (!string.IsNullOrEmpty(destPageUrl)
                || !web.WebTemplate.Equals("SRCHCENTERLITE", StringComparison.OrdinalIgnoreCase)
                && !web.WebTemplate.Equals("BLANKINTERNET", StringComparison.OrdinalIgnoreCase)) //basic search center和Publishing Portal类型的站点SAAS-520 SAAS-450
            {
                web.RestoreMasterPage(pageInfo, destPageUrl);
            }
        }

        private void SetMasterPageSettingBySolution(AveWebMasterPageInfo pageInfo, IAveWeb web, string destPageUrl)
        {
            //这里原先判断的是IsPublishingWeb，但是07允许PublishingSite开启Master Page设置
            //所以改用判断是否是IsPublishingSite(web.Site)
            if (!AveSPEnv.IsMoss || !mSite.IsPublish)
            {
                return;
            }
            //因为PublishingWeb和普通Web对Master Page设置的更新不同，所以加以区分
            if (web.IsPublish)
            {
                IAvePublishingWeb pWeb;
                try
                {
                    pWeb = new AvePublishingWeb(mSite, web as AveWeb, mRequest.GetPublishingWeb(web.ServerRelativeUrl));
                }
                catch (NotImplementedException e)
                {
                    mLogger.Debug(AveObjectModel_CommonResource.SetWebMasterPageInfoError_GetPublishWebFailed, web.Url, e.ToString());
                    return;
                }
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
                        }
                        catch (Exception e)
                        {
                            mLogger.Debug(AveObjectModel_CommonResource.SetCustomWebMasterPageInfoError, destCPageUrl, e.ToString());
                            //    
                        }
                    }
                    pWeb.CustomMasterUrl.SetInherit(pageInfo.CInheriting, false);
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
                        }
                        catch (Exception e)
                        {
                            mLogger.Debug(AveObjectModel_CommonResource.SetWebMasterPageInfoError, destMPageUrl, e.ToString());
                            //
                        }
                    }
                    pWeb.MasterUrl.SetInherit(pageInfo.MInheriting, false);
                }

                if (!pWeb.AlternateCssUrl.Value.Equals(destPageUrl, StringComparison.OrdinalIgnoreCase) || pWeb.AlternateCssUrl.IsInheriting != pageInfo.Inheriting)
                {
                    pWeb.AlternateCssUrl.SetInherit(pageInfo.Inheriting, false);
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
                        }
                        catch (Exception e)
                        {
                            mLogger.Debug(AveObjectModel_CommonResource.SetWebMasterPageInfoErrorWithoutInheriting, destCPageUrl, e.ToString());
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
                        }
                        catch (Exception e)
                        {
                            mLogger.Debug(AveObjectModel_CommonResource.SetWebMasterPageInfoErrorDestExisting, destMPageUrl, e.ToString());
                        }
                    }
                }
                //不是PublishingWeb需要Update()
                web.Update();
            }
        }
    }
}
