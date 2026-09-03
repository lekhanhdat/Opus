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

namespace AvePoint.GCommon.Contract.PlatformRecovery
{
    /// <summary>
    /// 需要判断是NetApp环境才能使用
    /// </summary>
    public interface IAveSOPRNetAppIntegration
    {
        string GetFarmStubDB();

        string GetWebAppStubDB(Guid webAppId);

        string GetContentDBStubDB(Guid contentDBId);

        /// <summary>
        /// 返回ContentDB 使用的所有Physical Device
        /// </summary>
        /// <param name="contentDBId">Content database Id</param>
        /// <returns>以physical device Id为key, 以PhysicalDeviceDto对象为value的Dictionary</returns>
        Dictionary<Guid, object> GetContentDBPhysicalDevices(Guid contentDBId);
        /// <summary>
        /// 返回WebApp 使用的所有Physical Device
        /// </summary>
        /// <param name="WebAppId">WebApplication Id</param>
        /// <returns>以physical device Id为key, 以PhysicalDeviceDto对象为value的Dictionary</returns>
        Dictionary<Guid, object> GetWebAppPhysicalDevices(Guid WebAppId);
        /// <summary>
        /// 返回Site Collection 使用的所有Physical Device
        /// </summary>
        /// <param name="siteCollectionId">Site Collection Id</param>
        /// <returns>以physical device Id为key, 以PhysicalDeviceDto对象为value的Dictionary</returns>
        Dictionary<Guid, object> GetSiteCollectionPhysicalDevices(Guid siteCollectionId);
        /// <summary>
        /// 指定一个Content database，指定一个Physical Device，返回隶属于此Content database，使用此Physical Device的所有Site collection,如果 不能保证Pool与SiteCollection的对应关系，throw NotSupportException
        /// </summary>
        /// <param name="contentDBId">Content database Id</param>
        /// <param name="PhysicalDeviceId">Physical Device Id</param>
        /// <returns>包含Site Collection Id 的List</returns>
        List<Guid> GetSiteCollectionInPhysicalDevice(Guid contentDBId, Guid PhysicalDeviceId);
        /// <summary>
        /// 获取SiteCollection Device路径，like Data_Extender\Farm(HYYINSP2010#SHAREPOINT_CONFIG-NEWJT)\http###hyyinsp2010#25000#\cc7dfda3-c0fb-49a2-9218-1915500b9556
        /// 注意：在确保Pool和SiteCollection对应关系的前提下保证此路径下的数据都属于此SiteCollection
        /// 否则会有多余数据!
        /// </summary>
        /// <param name="siteCollectionId">SiteCollectionId</param>
        /// <param name="physicalDeviceId">PhysicalDeviceId</param>
        /// <returns>Path List</returns>
        List<string> GetDevicePathsForSpecificSiteCollection(Guid siteCollectionId, Guid physicalDeviceId);
        /// <summary>
        /// 获取WebApp Device路径，like Data_Extender\Farm(HYYINSP2010#SHAREPOINT_CONFIG-NEWJT)\http###hyyinsp2010#25000#
        /// 保证此路径下的数据都属于此WebApp
        /// </summary>
        /// <param name="webAppId">WebApplication Id</param>
        /// <param name="physicalDeviceId">PhysicalDeviceId</param>
        /// <returns>Path List</returns>
        List<string> GetDevicePathsForSpecificWebApp(Guid webAppId, Guid physicalDeviceId);
        /// <summary>
        /// 获取ContentDatabase Device路径，like Data_Extender\Farm(HYYINSP2010#SHAREPOINT_CONFIG-NEWJT)\http###hyyinsp2010#25000#\cc7dfda3-c0fb-49a2-9218-1915500b9556
        /// 保证此路径下的数据都属于此ContentDatabase
        /// </summary>
        /// <param name="contentDBId">ContentDatabase Id</param>
        /// <param name="physicalDeviceId">PhysicalDeviceId</param>
        /// <returns>Path List</returns>
        List<string> GetDevicePathsForSpecificContentDB(Guid contentDBId, Guid physicalDeviceId);

    }
}
