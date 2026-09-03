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
namespace AvePoint.Wrapper.QueryService
{

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using AvePoint.Wrapper.Resource.QueryService;
    using static SP2016InsertQueryString;
    using static SP2016SelectQueryString;

    internal partial class AveQueryService:IAveCommonQueryService
    {
        #region Replicator

        #region Replicator private methods

        /// <summary>
        /// 插入web event receiver
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <param name="assemblyFullName"></param>
        /// <param name="eventHandlerClassNames"></param>
        private void AddWebEventReceiver(Guid webId, Guid siteId, AveEventReceiverType type, string assemblyFullName, string eventHandlerClassNames)
        {
            ExceptionHandlingScope(() =>
            {
                var cmdText = InsertWebMoveAndMoveEventReceiver_Insert_proc_InsertEventReceiver;
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@AssemblyFullName", assemblyFullName);
                mQueryWorker.AddParameter("@EventHandlerClassNames", eventHandlerClassNames);
                mQueryWorker.AddParameter("@NewId", Guid.NewGuid());
                mQueryWorker.AddParameter("@Type", (int)type);
                mQueryWorker.ExecuteNonQuery(cmdText);
            });
        }

        /// <summary>
        /// 根据传入的参数条件获取Document的CheckOutUserId
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId">可能为Guid.Empty,不为空才做为查询条件</param>
        /// <param name="itemId"></param>
        /// <param name="version">大于0才会作为查询条件</param>
        /// <param name="level">只有255才会作为查询条件</param>
        /// <returns></returns>
        private int GetCheckOutUserId(Guid siteId,Guid parentId,Guid itemId,int version,int level)
        {
            object obj = null;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@Id", itemId);
                mQueryWorker.AddParameter("@Level", level);
                mQueryWorker.AddParameter("@ParentId", parentId);
                mQueryWorker.AddParameter("@Version", version);
                if (parentId == Guid.Empty)
                {
                    logger.Warn("info.ParentId equals to Guid.Empty, may be not initialized");
                }
                var commandText = GetCheckoutUserIdByItemGuid_Select_AllDocs(parentId,version,level);
                obj = mQueryWorker.ExecuteScalar(commandText);
            });
            return Convert.ToInt32(obj);
        }

        #endregion Replicator private methods

        /// <summary>
        /// 获取DB中所有web的信息
        /// </summary>
        /// <returns>包含web查询结果的IAveQueryDataReader</returns>
        public IAveQueryDataReader GetAllWebs()
        {
            IAveQueryDataReader allWebsReader = null;
            ExceptionHandlingScope(() =>
            {
                var cmdText = GetAllWebs_Select_AllSites_AllWebs;
                mQueryWorker.ResetCommand(CommandType.Text);
                allWebsReader = new AveQueryDataReader(mQueryWorker.ExecuteReader(cmdText));
            });
            return allWebsReader;
        }

        /// <summary>
        /// 获取特定Web下的所有List
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="includeRecycleBin">是否包含回收站内的list</param>
        /// <returns>包含list查询结果的IAveQueryDataReader</returns>
        public IAveQueryDataReader GetAllListsInWeb(Guid siteId, Guid webId, bool includeRecycleBin)
        {
            IAveQueryDataReader dataReader = null;
            ExceptionHandlingScope(() =>
            {
                var cmdText = includeRecycleBin
                    ? GetAllListsInWebWithRecycleBin_Select_AllLists
                    : GetAllListsInWebWithoutRecycleBin_Select_AllLists;
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                dataReader = new AveQueryDataReader(mQueryWorker.ExecuteReader(cmdText));
            });
            return dataReader;
        }

        /// <summary>
        /// 获取一个ContentDB中指定assembly的所有EventReceivers信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="assemblyFullName"></param>
        /// <returns>包含所有EventReceiver查询结果的IAveQueryDataReader</returns>
        public IAveQueryDataReader GetAllEventReceivers(string assemblyFullName)
        {
            IAveQueryDataReader dataReader = null;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@AssemblyFullName", assemblyFullName);
                var cmdText = GetEventReceiversByAssembly_Select_EventReceivers_AllSites;
                dataReader = new AveQueryDataReader(mQueryWorker.ExecuteReader(cmdText));
            });
            return dataReader;
        }

        /// <summary>
        /// 根据scripts更新EventReceiver信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="scripts">sql脚本集合</param>
        public void Commit(List<string> scripts)
        {
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.Command.Transaction = mQueryWorker.BeginTransaction();
                var count = 0;
                foreach (var str in scripts)
                {
                    mQueryWorker.ExecuteNonQuery(str);
                    count++;
                    if (count > 1000)
                    {
                        mQueryWorker.Command.Transaction.Commit();
                        mQueryWorker.Command.Transaction = mQueryWorker.BeginTransaction();
                        count = 0;
                    }
                }
                mQueryWorker.Command.Transaction.Commit();
            });
        }

        /// <summary>
        /// 获取一个ContentDB下的所有Web(非RootWeb)的ID信息
        /// </summary>
        /// <param name="dataBase"></param>
        /// <param name="allWebs"></param>
        public void GetAllWebsByContentDB(IAveContentDatabase dataBase, Dictionary<Guid, Guid> allWebs)
        {
            ExceptionHandlingScope(() =>
            {
                var cmdText = GetAllWebsInDB_Select_AllWebs;
                using (var reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(2))
                        {
                            allWebs.Add(reader.GetGuid(0), reader.GetGuid(1));
                        }
                    }
                }
            });
        }
        public void GetAllSubWebsInContentDB(IAveContentDatabase dataBase, Dictionary<Guid, List<Guid>> allWebs)
        {
            ExceptionHandlingScope(() =>
            {
                var cmdText = GetAllWebsInDB_Select_AllWebs;
                using (var reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(2))
                        {
                            var siteId = reader.GetGuid(1);
                            List<Guid> webList;
                            if (!allWebs.TryGetValue(siteId, out webList))
                            {
                                webList = new List<Guid>();
                                allWebs[siteId] = webList;
                            }
                            webList.Add(reader.GetGuid(0));
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 将EventReceivers信息写入数据库中
        /// 创建web Move and delete event receiver
        /// Rewrite and Tested at 2015/8/4
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="siteId"></param>
        /// <param name="assemblyFullName"></param>
        /// <param name="eventHandlerClassNames"></param>
        public void WebDelAndMoveEventHandler(Guid webId, Guid siteId, string assemblyFullName, string eventHandlerClassNames)
        {
            AddWebEventReceiver(webId, siteId, AveEventReceiverType.WebDeleted, assemblyFullName, eventHandlerClassNames);
            AddWebEventReceiver(webId, siteId, AveEventReceiverType.WebMoved, assemblyFullName, eventHandlerClassNames);
        }

        /// <summary>
        /// 查询特定时间段内新创建的Web信息
        /// </summary>
        /// <param name="newWebs"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="sBuilder"></param>
        public void GetNewWebsByContentDB(Dictionary<Guid, Guid> newWebs, DateTime startTime, DateTime endTime, StringBuilder sBuilder)
        {
            ExceptionHandlingScope(() =>
            {
                var cmdText = GetNewCreatedWebsInDB_Select_EventCache_AllWebs;
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@StartTime", startTime);
                mQueryWorker.AddParameter("@EndTime", endTime);
                using (var reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (reader.Read())
                    {
                        //ADO-10271不同sitecollection下可能有相同webid，如果用webid作为key会出现问题 //相同webid是存在于不同的contentDB中的，因此去掉
                        var webId = reader.GetGuid(0);
                        var siteId = reader.GetGuid(1);
                        var serverRelativeUrl = reader.GetString(2);
                        //string keyWebValue = webId.ToString()；// + serverRelativeUrl;
                        //todo：wbhu,封装的返回值不对，Copy出来的site的webId是一样的，这里会出错
                        newWebs.Add(webId, siteId);
                        sBuilder.AppendFormat("\r\nFind new subWeb added. WebId: {0} WebName: {1} StartTime: {2} EndTime: {3}", webId, serverRelativeUrl, startTime, endTime);
                    }
                }
            });
        }

        public void GetAllNewWebsInContentDB(Dictionary<Guid, List<Guid>> newWebs, DateTime startTime, DateTime endTime)
        {
            ExceptionHandlingScope(() =>
            {
                var cmdText = GetNewCreatedWebsInDB_Select_EventCache_AllWebs_2;
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@StartTime", startTime);
                mQueryWorker.AddParameter("@EndTime", endTime);
                using (var reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (reader.Read())
                    {
                        Guid webId = reader.GetGuid(0);
                        Guid siteId = reader.GetGuid(1);
                        List<Guid> webList;
                        if (!newWebs.TryGetValue(siteId, out webList))
                        {
                            webList = new List<Guid>();
                            newWebs[siteId] = webList;
                        }
                        webList.Add(webId);
                    }
                }
            });
        }

        [Obsolete("Please use GetCheckOutUserID(Guid siteID, Guid parentID, Guid itemID) instead,note that there are reference outside")]
        public int GetCheckOutUserID(Guid siteID, Guid itemID)
        {
            throw new NotSupportedException("GetCheckOutUserID is not supported in SP2016 at time moment.");
        }

        /// <summary>
        /// 获取File的Cherckout Version上的CheckOut User Id
        /// API会涉及权限问题，有API实现
        /// </summary>
        /// <param name="siteID"></param>
        /// <param name="parentID"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        [QueryReview("2012/05/15", "Kexin Guo", true, "Rewrite")]
        [QueryReview("2012/12/17", "hyyin")]
        public int GetCheckOutUserID(Guid siteID, Guid parentID, Guid itemID)
        {
            return GetCheckOutUserId(siteID, parentID, itemID, 0, 255);
        }

        #endregion

        #region UtilityProcess

        #region UtilityProcess Private Methods

        private static string ReplaceSpecialCharactersInQueryLikeCondition(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Trim();
                str = str.Replace("%", "[%]");
                str = str.Replace("_", "[_]");
                return str;
            }
            return str;
        }

        /// <summary>
        /// 初始化查询User和Group的Query语句和Parameter
        /// 在SQL查询语句的like语句中，如果匹配的name含有特定字符'%'和'_'，则需要转为'[%]'和'[_]'，以区别于通配符。
        /// </summary>
        /// <param name="userSearchInfo"></param>
        /// <param name="mFlag"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han", true, "Use union all instead of union to improve the performance")]
        private string InitGetUserGroupNativeCommand(string userSearchInfo, AveAccountSearchFlag mFlag, string siteId)
        {
            userSearchInfo = ReplaceSpecialCharactersInQueryLikeCondition(userSearchInfo);
            mQueryWorker.ResetCommand(CommandType.Text);
            mQueryWorker.AddParameter("@displayName", userSearchInfo);
            mQueryWorker.AddParameter("@loginName", userSearchInfo);
            mQueryWorker.AddParameter("@emailAddress", userSearchInfo);
            mQueryWorker.AddParameter("@siteId", (!string.IsNullOrEmpty(siteId)) ? new Guid(siteId) : SqlGuid.Null);
            return GetUserOrGroupByName_Select_Alldocs(mFlag);
        }

        #endregion UtilityProcess Private Methods

        /// <summary>
        /// 根据传入的name查询login name或Title 匹配的user 或 group，效率考虑，有API实现.(People picker使用)
        /// </summary>
        /// <param name="userSearchInfo"></param>
        /// <param name="flag"></param>
        /// <param name="siteId"></param>
        /// <param name="isExact"></param>
        /// <returns></returns>
        public List<AveUserDetail> GetUserDetailByNative(string userSearchInfo, AveAccountSearchFlag flag, string siteId, bool isExact)
        {
            var userDetails = new List<AveUserDetail>();
            try
            {
                ExceptionHandlingScope(() =>
                {
                    var commandText = InitGetUserGroupNativeCommand(userSearchInfo, flag, siteId);
                    using (var reader = mQueryWorker.ExecuteReader(commandText))
                    {
                        while (reader.Read())
                        {
                            var isGroup = reader.GetString(3).Equals("2");
                            var detail = new AveUserDetail
                            {
                                LoginName = reader.GetString(0),
                                DisplayName = reader.GetString(1),
                                Email = reader.GetString(2),
                                AccountType = isGroup ? AveAccountType.SharePointGroup : AveAccountType.SharePointUser
                            };
                            userDetails.Add(detail);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Warn(WrapperQueryServiceResource.GetUserInfoError, ex);
            }
            return userDetails;
        }

        #endregion

        /// <summary>
        /// 获得所有connector stub的大小,无API实现. TODO: improve it in different way.
        /// </summary>
        /// <returns></returns>
        public ulong GetConnectorDataSize()
        {
            //如果实现，13和16要重新写
            throw new NotSupportedException("GetConnectorDataSize is not supported in SP2016 at time moment.");
        }

        #region GA+

        /// <summary>
        /// 获取DB中所有站点的StorageInfo(不包括回收站数据)
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, StorageUsageInfo> GetSitesStorageInfo()
        {
            var sitesStorageInfo = new Dictionary<Guid, StorageUsageInfo>();
            ExceptionHandlingScope(() =>
            {
                var cmd = GetSiteStorageInfoInDB_Select_AllSites;
                using (var reader = mQueryWorker.ExecuteReader(cmd))
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var siteInfo = new StorageUsageInfo()
                            {
                                SiteId = reader.GetGuid(0),
                                DiskUsed = reader.GetInt64(1),
                                DiskQuota = reader.GetInt64(2)
                            };
                            sitesStorageInfo.Add(siteInfo.SiteId, siteInfo);
                        }
                    }
                }
            });
            return sitesStorageInfo;
        }

        #endregion

        #region Migration(check permission)

        #region check permission private methods
        /// <summary>
        /// 检查传入的所有login中，是否某个login有对应的DBRole权限
        /// </summary>
        /// <param name="loginNameList"></param>
        /// <param name="dbRole"></param>
        /// <returns></returns>
        private bool CheckLoginNameListInDBRole(string loginNameList, DatabaseRole dbRole)
        {
            var checkStatus = false;
            ExceptionHandlingScope(() =>
            {
                var groupNames = new List<string>();
                mQueryWorker.ResetCommand(CommandType.Text);
                var commandTextForSearchLoginName = GetGroupNameByLogins_Select_sysusers_syslogins(loginNameList);
                using (var reader = mQueryWorker.ExecuteReader(commandTextForSearchLoginName))
                {
                    while (reader.Read())
                    {
                        groupNames.Add(Convert.ToString(reader[0]));
                    }
                }

                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@RoleName", dbRole.ToString());
                foreach (var groupName in groupNames)
                {
                    mQueryWorker.AddParameter("@UserName", groupName);
                    if ((int)mQueryWorker.ExecuteScalar(IsMemberOfDBRole_Select_IS_ROLEMEMBER) == 1)
                    {
                        checkStatus = true;
                        break;
                    }
                }
            });
            return checkStatus;
        }

        /// <summary>
        /// 查询特定user有没有指定的DBRole的权限
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="dbRole"></param>
        /// <returns></returns>
        private bool CheckUserInDBRole(string userName, DatabaseRole dbRole)
        {
            var checkStatus = false;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@RoleName", dbRole.ToString());
                mQueryWorker.AddParameter("@UserName", userName);
                checkStatus = (int)mQueryWorker.ExecuteScalar(CheckUserInDBRole_Select_IS_ROLEMEMBER) == 1;
            });
            return checkStatus;
        }

        #endregion check permission private methods

        /// <summary>
        /// 根据user name或sid查询特定user在Sql Server上有没有指定的权限(特定的ServerRole)
        /// </summary>
        /// <param name="userName">需要check的user的name</param>
        /// <param name="sRole"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        public bool CheckDatabaseServerRole(string userName, ServerRole sRole, byte[] sid)
        {
            var checkStatus = false;
            try
            {
                ExceptionHandlingScope(() =>
                {
                    var commandText = CheckUserInServerRole_Select_server_role_members_server_principals;
                    mQueryWorker.AddParameter("@RoleName", sRole.ToString());
                    mQueryWorker.AddParameter("@LoginNames", userName);
                    mQueryWorker.AddParameter("@Sid", sid);
                    using (var reader = mQueryWorker.ExecuteReader(commandText))
                    {
                        checkStatus = reader.Read();
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Warn("Error happened when check database server role.Reason:{0}.", ex);
            }
            return checkStatus;
        }

        /// <summary>
        /// 根据user name或sid查询特定user在Sql Server上有没有指定的权限(特定的ServerRole)
        /// </summary>
        /// <param name="logins">may be one or more logins</param>
        /// <param name="dbRole"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        public bool CheckDatabaseRole(string logins, DatabaseRole dbRole, byte[] sid)
        {
            var checkStatus = false;
            try
            {
                checkStatus = logins.IndexOf(',') < 0 ? CheckUserInDBRole(logins, dbRole) : CheckLoginNameListInDBRole(logins, dbRole);
            }
            catch (Exception ex)
            {
                logger.Warn("Error happened when check database role.Reason:{0}.", ex.ToString());
            }
            return checkStatus;
        }       

        #endregion

    }
}
