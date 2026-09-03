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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.DocAve
{
    public interface IDocAveSharePointSiteService
    {
        List<RemoteWebApplication> GetAllSiteGroup();

        RemoteWebApplication GetRemoteSiteGroupById(string id);
        RemoteWebApplication GetSiteGroup(string groupName);
        bool RegisteRemoteSite(string siteurl, string user, string password, RemoteWebApplication webapp);
        RemoteSiteCollection CheckSiteUrlExist(string siteUrl);
        /// <summary>
        /// 注册SiteCollection到Remote Farm, 通过DocAve;  RevIM Online是否有实际webapi的需求， 如果有需求， DA Online需要做相应的实现
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="groupName">指定GroupName</param>
        /// <returns>
        /// 0. 成功
        /// 1. 找不到site group
        /// 2.没有可用的Agent, 或没有匹配Account的Agent
        /// 3. 301 没有可用的Agent,  当前的Site Group下
        /// 302 完全没有权限
        /// 303 部分没有权限
        /// 304 Type mismatch类型不匹配
        /// 4. 已经注册过了
        /// 10. Unexpected Exception
        /// </returns>
        //RABposResult CreateRemoteSite(string siteUrl, string groupName, string userName);

        Task<int> SetRMSharePointSettingAsync(RemoteWebApplication web, RemoteSiteCollection site, string defaultTermPath, string rootTermPath, bool applyExist = false, bool applyType = false);
        /// <summary>
        /// 标记成Physical Record Location
        /// </summary>
        /// <param name="site"></param>
        /// <param name="url"></param>
        /// <returns>
        /// 0. 成功 
        /// 3 实例化Url失败
        /// 4 执行过程中其它Unexpected Error
        /// </returns>
        Task<int> MarkPhysicalLocationAsync(RemoteWebApplication web, RemoteSiteCollection site, string url);
        int ApplyAllSharePointSettingJob();
        int ApplySharePointSettingJobOnNode(RemoteSiteCollection site);
    }
}
