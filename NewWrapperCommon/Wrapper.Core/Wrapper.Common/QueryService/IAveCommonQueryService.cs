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
using System.Text;
using System.Collections;

namespace AvePoint.Wrapper.Common
{
    public interface IAveCommonQueryService : IAveMetadataServiceQueryService, IAveCentralAdminQueryService
    {

        #region Replicator

        /// <summary>
        /// 获取DB中所有web的信息
        /// </summary>
        /// <returns>包含web查询结果的IAveQueryDataReader</returns>
        IAveQueryDataReader GetAllWebs();

        /// <summary>
        /// 获取特定Web下的所有List
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="includeRecycleBin">是否包含回收站内的list</param>
        /// <returns>包含list查询结果的IAveQueryDataReader</returns>
        IAveQueryDataReader GetAllListsInWeb(Guid siteId, Guid webId, bool includeRecycleBin);

        /// <summary>
        /// 获取一个ContentDB中指定assembly的所有EventReceivers信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="assemblyFullName"></param>
        /// <returns>包含所有EventReceiver查询结果的IAveQueryDataReader</returns>
        IAveQueryDataReader GetAllEventReceivers(string assemblyFullName);

        /// <summary>
        /// 根据scripts更新EventReceiver信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="scripts">sql脚本集合</param>
        void Commit(List<string> scripts);

        [Obsolete("Use GetAllSubWebsInContentDB instead.")]
        /// <summary>
        /// 获取一个ContentDB下的所有Web(非RootWeb)的ID信息
        /// </summary>
        /// <param name="dataBase"></param>
        /// <param name="allWebs"></param>
        void GetAllWebsByContentDB(IAveContentDatabase dataBase, Dictionary<Guid, Guid> allWebs);

        /// <summary>
        /// 获取一个ContentDB下的所有Web(非RootWeb)的ID信息
        /// </summary>
        /// <param name="dataBase"></param>
        /// <param name="allWebs">Key: SiteId.  Value: list of webs</param>
        void GetAllSubWebsInContentDB(IAveContentDatabase dataBase, Dictionary<Guid, List<Guid>> allWebs);

        /// <summary>
        /// 将EventReceivers信息写入数据库中
        /// 创建web Move and delete event receiver
        /// Rewrite and Tested at 2015/8/4
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="siteId"></param>
        /// <param name="assemblyFullName"></param>
        /// <param name="eventHandlerClassNames"></param>
        void WebDelAndMoveEventHandler(Guid webId, Guid siteId, string assemblyFullName, string eventHandlerClassNames);

        [Obsolete("Please use GetAllNewWebsInContentDB instead.")]
        /// <summary>
        /// 查询特定时间段内新创建的Web信息
        /// </summary>
        /// <param name="newWebs"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="sBuilder"></param>
        void GetNewWebsByContentDB(Dictionary<Guid, Guid> newWebs, DateTime startTime, DateTime endTime, StringBuilder sBuilder);

        /// <summary>
        /// 查询特定时间段内新创建的Web信息
        /// </summary>
        /// <param name="newWebs">key:SiteId, Value:web list</param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        void GetAllNewWebsInContentDB(Dictionary<Guid, List<Guid>> newWebs, DateTime startTime, DateTime endTime);

        [Obsolete("Please use GetCheckOutUserID(Guid siteID, Guid parentID, Guid itemID) instead,note that there are reference outside")]
        int GetCheckOutUserID(Guid siteID, Guid itemID);

        /// <summary>
        /// 获取File的Cherckout Version上的CheckOut User Id
        /// API会涉及权限问题，有API实现
        /// </summary>
        /// <param name="siteID"></param>
        /// <param name="parentID"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        int GetCheckOutUserID(Guid siteID, Guid parentID, Guid itemID);

        #endregion

        #region UtilityProcess

        /// <summary>
        /// 根据传入的name查询login name或Title 匹配的user 或 group，效率考虑，有API实现.(People picker使用)
        /// </summary>
        /// <param name="userSearchInfo"></param>
        /// <param name="flag"></param>
        /// <param name="siteId"></param>
        /// <param name="isExact"></param>
        /// <returns></returns>
        List<AveUserDetail> GetUserDetailByNative(string userSearchInfo, AveAccountSearchFlag flag, string siteId, bool isExact);

        #endregion

        /// <summary>
        /// 查询DB中所有Connector stub的size
        /// 13和16有问题,需要重写
        /// </summary>
        /// <returns></returns>
        ulong GetConnectorDataSize();

        #region GA+

        /// <summary>
        /// 获取DB中所有站点的StorageInfo(不包括回收站数据)
        /// </summary>
        /// <returns></returns>
        Dictionary<Guid, StorageUsageInfo> GetSitesStorageInfo();

        #endregion

        #region Migration

        /// <summary>
        /// 根据user name或sid查询特定user在Sql Server上有没有指定的权限(特定的ServerRole)
        /// </summary>
        /// <param name="userName">需要check的user的name</param>
        /// <param name="sRole"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        bool CheckDatabaseServerRole(string userName, ServerRole sRole, byte[] sid);

        /// <summary>
        /// 根据user name或sid查询特定user在Sql Server上有没有指定的权限(特定的ServerRole)
        /// </summary>
        /// <param name="logins">可能是多个user or group以，分隔</param>
        /// <param name="dbRole"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        bool CheckDatabaseRole(string logins, DatabaseRole dbRole, byte[] sid);

        #endregion

    }
}
