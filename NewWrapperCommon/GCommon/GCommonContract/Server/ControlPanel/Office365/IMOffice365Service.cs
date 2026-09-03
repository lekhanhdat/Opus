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

using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using System;
using System.Collections.Generic;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Office365
{
    public interface IMOffice365Service
    {
        /// <summary>
        /// 获取对应的Site Group
        /// </summary>
        /// <param name="id">site group id</param>
        /// <returns></returns>
        RemoteWebApplication GetRemoteWebApplicationById(String id);

        /// <summary>
        /// 获取当前登录user可见的全部Site Group
        /// </summary>
        /// <returns></returns>
        List<RemoteWebApplication> GetAllRemoteWebApplication();

        /// <summary>
        /// 获取当前Site Group下的全部Site Collection
        /// </summary>
        /// <param name="parentId">site group id</param>
        /// <param name="states">包含的站点状态</param>
        /// <returns></returns>
        List<RemoteSiteCollection> GetRemoteSiteCollectionByParentId(String parentId, SiteCollectionState[] states, Boolean isFilterTreeNode = false);

        /// <summary>
        /// 获取当前有权限的RemoteSiteCollection中与CA PrefixUrl匹配的
        /// </summary>
        /// <param name="prefixUrl">CA PrefixUrl</param>
        /// <param name="MaxCount">获取数量限制</param>
        /// <returns></returns>
        List<RemoteSiteCollection> GetRemoteSiteCollectionByUrlLikePrefixUrl(String prefixUrl, int MaxCount = 1000);

        /// <summary>
        /// 获取当前Site Group下的全部Site Collection（重载方法，用来兼容旧版本的API）
        /// </summary>
        /// <param name="parentId">site group id</param>
        /// <param name="states">包含的站点状态</param>
        /// <returns></returns>
        List<RemoteSiteCollection> GetRemoteSiteCollectionByParentId(String parentId, SiteCollectionState[] states);
        /// <summary>
        /// 只有DocAve System（Schedule user）可以使用此方法，外部已经无法调用
        /// </summary>
        /// <returns></returns>
        List<RemoteSiteCollection> GetAllRemoteSiteCollection();

        /// <summary>
        /// 根据Site Collection Url获取此站点对应的最新用户名密码
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <returns></returns>
        BposInfo GetBposAccountBySiteUrl(string siteUrl);

        /// <summary>
        /// 根据Site Collection Id获取此站点对应的最新用户名密码
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        BposInfo GetBposAccountBySiteId(string siteId);

        /// <summary>
        /// 根据Site Collection Url获取此站点对应的最新用户名密码或App Token相关信息(根据Site Collection的AuthorizeType)
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <returns></returns>
        BposInfo GetBposInfoBySiteUrl(string siteUrl);

        /// <summary>
        /// 根据Site Collection Id获取此站点对应的最新用户名密码或App Token相关信息(根据Site Collection的AuthorizeType)
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        BposInfo GetBposInfoBySiteId(string siteId);

        /// <summary>
        /// 根据Tree Node节点获取Agents
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        List<ServiceDto> GetAgentsByRemoteFarmTreeNode(SPTreeNodeDto node);

        /// <summary>
        /// 通过Id读取对应的Remote Site Collection
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        RemoteSiteCollection GetRemoteSiteCollection2(string id, int mark);

        /// <summary>
        /// 根据Site Collection Url和Site Group Name获取指定的Remote Site Collection
        /// </summary>
        /// <param name="sitecollectionURL">Site Collection Url</param>
        /// <param name="webapplicationURL">Site Group Name</param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        RemoteSiteCollection GetSiteCollectionByNameAndParentUrl(String sitecollectionURL, String webapplicationURL, String accountId);

        /// <summary>
        /// 创建Site Group
        /// </summary>
        /// <param name="remote"></param>
        /// <returns></returns>
        String CreateRemoteWebApplication(RemoteWebApplication remote);

        /// <summary>
        /// 创建Site Collection
        /// </summary>
        /// <param name="remote"></param>
        /// <returns></returns>
        String CreateRemoteSiteCollection(RemoteSiteCollection remote);

        /// <summary>
        /// 检测指定Site的状态
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="agentGroupId">agent group id</param>
        /// <param name="siteCollectionId">如果是已经存在的Site需要指定此属性</param>
        /// <returns></returns>
        Office365TestResult TestForOffice365(Office365MessageContract message, RemoteWebApplication siteGroup, String siteCollectionId);

        /// <summary>
        /// 检测指定Url是否存在
        /// </summary>
        /// <param name="adminCenterId">CA sties ID</param>
        /// <param name="urlList">指定Url的集合</param>
        /// <returns>不存在或者有异常的Site</returns>
        List<RemoteSiteCollection> TestSitesForCMCreateContainer(string adminCenterId, List<string> urlList);

        /// <summary>
        /// 检测指定Site的状态 （重载方法，用来兼容旧版本的API）
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="agentGroupId">agent group id</param>
        /// <param name="siteCollectionId">如果是已经存在的Site需要指定此属性</param>
        /// <returns></returns>
        Office365TestResult TestForOffice365(Office365MessageContract message, String agentGroupId, String siteCollectionId);
        /// <summary>
        /// 滤掉没有权限的节点
        /// </summary>
        /// <param name="treeNodeDtos"></param>
        /// <returns></returns>
        IList<SPTreeNodeDto> FilterTreeNode(IList<SPTreeNodeDto> treeNodeDtos);

        /// <summary>
        /// 滤掉没有权限的File System节点
        /// </summary>
        /// <param name="treeNodeDtos"></param>
        /// <returns></returns>
        IList<FSTreeNodeDto> FilterFSTreeNode(IList<FSTreeNodeDto> treeNodeDtos);

        /// <summary>
        /// 更新site group
        /// </summary>
        /// <param name="remote"></param>
        /// <returns>0代表成功更新</returns>
        int UpdateRemoteWebApplication(RemoteWebApplication remote);

        /// <summary>
        /// 更新site
        /// </summary>
        /// <param name="remote"></param>
        /// <returns>0代表成功更新</returns>
        int UpdateRemoteSiteCollection(RemoteSiteCollection remote);

        /// <summary>
        /// 删除site group
        /// 需设置DeleteMode=WebAppliation
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Office365ResponseInfo DeleteRemoteWebApplication(Office365RequestInfo request);

        /// <summary>
        /// 删除site
        /// 需设置DeleteMode=SiteCollection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Office365ResponseInfo DeleteRemoteSiteCollection(Office365RequestInfo request);

        /// <summary>
        /// 跳过DocAve Control端的验证直接删除站点
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        int DeleteRemoteSiteCollectionByIds(List<String> ids);

        /// <summary>
        /// 判断Site和SiteGroup是否重名
        /// </summary>
        /// <param name="profileType">Profile类型（Office365RemoteWebApplication/Office365RemoteSitecollection）</param>
        /// <param name="name">Name</param>
        /// <param name="excludeId">需要验证的site/site group Id，新加的时候可以传Null</param>
        /// <returns></returns>
        bool IsNameExist(ProfileType profileType, string name, string excludeId);

        /// <summary>
        /// 获取当前站点的可用agent
        /// </summary>
        /// <param name="siteCollectionId">site collection id</param>
        /// <param name="agentType">模块对应的Agent type</param>
        /// <returns></returns>
        IList<ServiceDto> GetBPOSAgents(string siteCollectionId, List<string> agentType);

        /// <summary>
        /// 获取当前站点的的一个可用agent
        /// </summary>
        /// <param name="siteCollectionId"></param>
        /// <param name="agentType"></param>
        /// <returns></returns>
        ServiceDto GetBPOSAgent(string siteCollectionId, List<string> agentType);

        /// <summary>
        /// 获取user有权限的所有site collection
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        List<RemoteSiteCollection> GetSiteCollectionByName(String accountId);

        /// <summary>
        /// 根据Site collection id获取对应的agent group id
        /// </summary>
        /// <param name="siteCollectionId"></param>
        /// <returns></returns>
        string GetAgentGroupId(string siteCollectionId);

        Dictionary<string, List<string>> GetAuthorisedWebApplicationIds();

        List<RemoteSiteCollection> GetRemoteSiteCollectionByIds(List<string> ids);

        /// <summary>
        /// 此方法提供给需要目的端选择admin center站点实现office365 create功能的模块使用
        /// 由于权限限制，需要使用Run Job的User权限去创建站点
        /// </summary>
        /// <param name="adminCenterId">admin center站点的id，对应的是tree上的</param>
        /// <param name="urlList"></param>
        /// <param name="currentUser">Run Job的User</param>
        void TestAndSaveAllSites(string adminCenterId, List<string> urlList, string currentUser);

        void RebuildRemoteSites(List<SPTreeNodeDto> remoteTreeNode);
        
        CreateSiteCollection4GAResult CreateRemoteSiteCollectionForGA(string siteUrl, string tenantId, string account, string password, string containerId = null);
    }
}