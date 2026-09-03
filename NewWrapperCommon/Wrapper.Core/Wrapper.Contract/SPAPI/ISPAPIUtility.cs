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
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.Wrapper.Core.SPAPI
{
    /// <summary>
    /// Create是需要new 一个SP对象
    /// Get是获取SP中已存在的对象。
    /// </summary>
    public interface ISPAPIUtility
    {
        /// <summary>
        /// Create a AveQuery Object
        /// </summary>
        /// <returns></returns>
        IAveQuery CreateQuery();

        /// <summary>
        /// Create Regional setting
        /// </summary>
        /// <param name="web"></param>
        /// <param name="bIsUserRegionalSetting"></param>
        /// <returns></returns>
        IAveRegionalSettings CreateRegionalSetting(IAveWeb web, bool bIsUserRegionalSetting);

        /// <summary>
        /// according to filed value to create field user value obj.
        /// </summary>
        /// <param name="web"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        IAveFieldUserValue CreateFieldUserValue(IAveWeb web, string fieldValue);

        /// <summary>
        /// according to filed value to create field url value obj.
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        IAveFieldUrlValue CreateFieldUrlValue(string fieldValue);

        /// <summary>
        /// Get Web Application
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        IAveWebApplication GetWebApplication(string url);
        /// <summary>
        /// Get Office 365 admin site
        /// </summary>
        /// <param name="adminUrl"></param>
        /// <param name="o365AccountInfo"></param>
        /// <returns></returns>
        IAveTenant GetAdminSite(string adminUrl, Common.O365AccountInfo o365AccountInfo);
        /// <summary>
        /// Get Site Collection
        /// if the site collection does not exist, return null; otherwise return the related object.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        IAveSite GetSiteCollection(string url);

        /// <summary>
        /// Get Site Collection
        /// if the site collection does not exist, return null; otherwise return the related object.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="o365AccountInfo"></param>
        /// <returns></returns>
        IAveSite GetSiteCollection(string url, Common.O365AccountInfo o365AccountInfo);

        /// <summary>
        /// Get Site Collection
        /// if the site collection does not exist, return null; otherwise return the related object.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IAveSite GetSiteCollection(string url, IAveUserToken token);

        /// <summary>
        /// Create Service Context
        /// </summary>
        /// <returns></returns>
        IAveServiceContext GetServiceContext();

        /// <summary>
        /// Create Site Subscription Identifier
        /// </summary>
        /// <returns></returns>
        IAveSiteSubscriptionIdentifier GetSiteSubscriptionIdentifier();

        /// <summary>
        /// Get UserProfile Manager, the 'site' parameter is just to be used by O365.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IAveOUserProfileManager GetUserProfileManager(IAveServiceContext context, IAveSite site);

        /// <summary>
        /// Get SocialComment Manager
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IAveOSocialCommentManager GetSocialCommentManager(IAveServiceContext context);

        /// <summary>
        /// Get Taxonomy Session
        /// </summary>
        /// <returns></returns>
        IAveTaxonomySession GetTaxonomySession(IAveServiceContext context);

        /// <summary>
        /// Wrapper Site collection
        /// </summary>
        /// <param name="siteObj"></param>
        /// <returns></returns>
        IAveSite WrapperSiteCollection(object siteObj);

        /// <summary>
        /// support spmode and version
        /// </summary>
        /// <param name="spMode"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        bool Support(Common.WrapperSPMode spMode, Version version);

        /// <summary>
        /// Get Farm Object
        /// </summary>
        /// <returns></returns>
        IAveFarm GetFarm();

        /// <summary>
        /// Get Web Service
        /// </summary>
        /// <returns></returns>
        IAveWebService GetContentService();

        /// <summary>
        /// Get Security
        /// </summary>
        IAveSecurity GetSecurity();

        /// <summary>
        /// 创建SearchScopeInfo。
        /// </summary>
        /// <returns></returns>
        IAveOScopeInfo CreateSearchScopeInfo();

        /// <summary>
        /// 创建RuleInfo。
        /// </summary>
        /// <returns></returns>
        IAveORuleInfo CreateRuleInfo();

        /// <summary>
        /// 创建DisplayGroupInfo。
        /// </summary>
        /// <returns></returns>
        IAveODisplayGroupInfo CreateDisplayGroupInfo();

        /// <summary>
        /// 进行一些初始化工作，如初始化AveAssebmlyUtility的Assembly Cache。
        /// </summary>
        /// <returns></returns>
        void Initialize();

        /// <summary>
        /// 创建FieldLink。
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        IAveFieldLink CreateFieldLink(IAveField field);

        /// <summary>
        /// 创建WorkflowAssociation。
        /// </summary>
        /// <returns></returns>
        IAveWorkflowAssociation CreateWorkflowAssociation();

        /// <summary>
        /// 用于Force Create ContentType。
        /// </summary>
        /// <param name="contentTypId"></param>
        /// <returns></returns>
        IAveContentType CreateContentType(IAveContentTypeId contentTypId);
    }
}
