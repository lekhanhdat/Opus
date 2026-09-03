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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Models.API;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.API
{
    /// RECO-20916 remove web api method, need to build API Authentication with Identity service if have requirement. 
    //public class ApplyClassificationController : RAWebApiBase
    //{
    //    private RALogger logger = RALogger.GetInstance(typeof(ApplyClassificationController));
    //    private IDocAveSharePointSiteService _SharePointSiteService;
    //    private IDocAveSharePointSiteService SharePointSiteService => PlatformWindsorManager.GetService(ref _SharePointSiteService);
        
    //    [HttpPost]
    //    public async Task<int> Post([FromBody] APIClasificationModel model)
    //    {
    //        //            1.      Sample Function Signatures

    //        //function ApplyClassification (string SharePointUrl) { ApplyClassification (SharePointUrl, null, null) }
    //        //function ApplyClassification (string SharePointUrl, string DefaultClassificationTerm) { ApplyClassification (SharePointUrl, DefaultClassificationTerm, null) }
    //        //function ApplyClassification (string SharePointUrl, string DefaultClassificationTerm, string RootClassificationTerm) {
    //        //…
    //        //}
    //        //2.      Execution Logic
    //        //If (SharePointUrl points to a site collection and SharePointUrl is not a registered site collection) {
    //        //                Use DocAve SDK to register the site collection to the RevIM group
    //        //}

    //        //var classificationInfo
    //        //If (SharePointUrl is a Site Collection) {
    //        //classificationInfo = Get the default classification (scope and default values) for the Group
    //        //} else {
    //        //                classificationInfo = Get the classification values (scope and default values) of the container
    //        //                }
    //        //                If (DefaultClassificationTerm is not null) {
    //        //                                Change the default term in the classificationInfo to DefaultClassificationTerm
    //        //}
    //        //                If (RootClassificationTerm is not null) {
    //        //                                Change the root classification term in the classificationInfo to RootClassificationTerm
    //        //}

    //        //                Apply the classificationInfo to SharePointUrl
    //        //                Execute the RevIM timer job that updates SharePoint

    //        //Office365Service.CreateRemoteSiteCollection(SiteDto)
    //        try
    //        {
    //            //string productName = "DocAve";

    //            //Aos.Sdk.Models.AccountInfo account = Aos.Sdk.AosApi.UserService.LogOn(userName, pwd, GAConstants.GAOnline);
    //            ///option for Apply all jobs
    //            if (model.ApplySettingNow.Equals("true"))
    //            {
    //                return SharePointSiteService.ApplyAllSharePointSettingJob();
    //            }
    //            else
    //            {
    //                #region set custom setting first
    //                //DAOAPIClientV1 test = new DAOAPIClientV1();
    //                //RemoteSiteCollection site = test.GetRemoteSiteCollectionByUrl(model.SiteCollectionUrl);
    //                RemoteSiteCollection site = RABrowserClient.GetRemoteSiteCollectionByUrl(model.SiteCollectionUrl);
    //                // RemoteSiteCollection site = SharePointSiteService.CheckSiteUrlExist(model.SiteCollectionUrl);
    //                //if (site == null)
    //                //{
    //                //    logger.Info("no exist remote site with site url {0}, register it.", model.SiteCollectionUrl);
    //                //    RABposResult result = SharePointSiteService.CreateRemoteSite(model.SiteCollectionUrl, model.GroupName, model.UserName);
    //                //    logger.Info("Create remote site collection, result {0}", result.Result);
    //                //    if (result.Result != 0 && result.Result != 4)
    //                //    {
    //                //        logger.Warn("Validate and register site failed,  result is {0}", result.Result);
    //                //        return result.Result;
    //                //    }
    //                //    site = result.Site;
    //                //}
    //                //if (site == null)
    //                //{
    //                //    logger.Info("Site after registed is null, check it again.");
    //                //    site = test.CheckSiteUrlExist(model.SiteCollectionUrl);
    //                //}
    //                if (site == null)
    //                {
    //                    return 305;     //注册成功  但是url改变了, 无法获取详细信息
    //                }
    //                if (string.Equals(model.SiteCollectionUrl, site.url, StringComparison.OrdinalIgnoreCase))
    //                {
    //                    //参数是真正的SiteCollection
    //                    logger.Debug("url from model is a Site Collection");
    //                }
    //                else
    //                {
    //                    logger.Debug("url from model is not a Site Collection");
    //                    //其它级别
    //                }
    //                logger.Debug("Finish auto register site to DocAve. start to apply classification");
    //                //RemoteWebApplication remoteSiteGroup = test.GetWebApplicationById(site.parentId);
    //                RemoteWebApplication remoteSiteGroup = RABrowserClient.GetWebApplicationById(site.parentId);
    //                if (model.DefaultTermPath != null || model.RootTermPath != null)
    //                {
    //                    return await SharePointSiteService.SetRMSharePointSettingAsync(remoteSiteGroup, site, model.DefaultTermPath, model.RootTermPath);
    //                }
    //                else
    //                {
    //                    logger.Info("default term and root term is both null.");
    //                }

    //                #endregion
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            logger.Error(e.Message, e);
    //            return 10;
    //        }
    //        return 0;
    //    }
    //}
}