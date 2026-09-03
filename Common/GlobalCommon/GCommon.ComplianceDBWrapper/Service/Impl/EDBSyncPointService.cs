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
using System.Data.SqlClient;
using AvePoint.GCommon.ComplianceDBWrapper.Core;
using AvePoint.GCommon.ComplianceDBWrapper.Utility;

namespace AvePoint.GCommon.ComplianceDBWrapper.Service.Impl
{
    public class EDBSyncPointService : AbstractService
    {
        public EDBSyncPointService(SqlConnection conn, EDDBWrapper dbWrapper)
            : base(conn, dbWrapper)
        {
        }

        #region - 获得WebApp的同步时间点 -

        /// <summary>
        /// 根据FarmID，与WebAppID,获得该WebApp上次的同步时间点.
        /// </summary>
        /// <param name="farmID"></param>
        /// <param name="webAppID"></param>
        /// <returns></returns>
        public DateTime GetPoint(string farmID, Guid webAppID)
        {
            #region - execute sql -

            const string executeSql = @"SELECT 
                                            TimePoint 
                                        FROM 
                                            CPLED_SyncPoint  
                                        WHERE 
                                            FarmID = @FarmID 
                                        AND 
                                            WebAppID = @WebAppID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@FarmID", farmID);
            cmd.AddValue("@WebAppID", webAppID);
            object time = cmd.ExecuteScalar();
            if (time.IsNull())
            {
                time = DateTime.MinValue;
            }
            cmd.Dispost();
            return (DateTime)time;
        }

        #endregion

        #region - 插入WebApp的同步时间点 -

        /// <summary>
        /// 只插入，不检查
        /// </summary>
        /// <param name="farmID"></param>
        /// <param name="webAppID"></param>
        /// <param name="webAppURL"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        [Obsolete("Use ResetPoint(string farmID, Guid webAppID, string webAppUrl, DateTime point)")]
        public int SetPoint(string farmID, Guid webAppID, string webAppURL, DateTime point)
        {

            #region - execute sql -

            const string executeSql = @"INSERT INTO 
                                    CPLED_SyncPoint  
                                    (FarmID,WebAppID,WebAppURL,TimePoint) 
                                VALUES 
                                    (@FarmID,@WebAppID,@WebAppURL,@TimePoint) ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@FarmID", farmID);
            cmd.AddValue("@WebAppID", webAppID);
            cmd.AddValue("@WebAppURL", webAppURL);
            cmd.AddValue("@TimePoint", point);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        #endregion

        #region - 更新WebApp的同步时间点 -
        /// <summary>
        /// 只更新，不检查
        /// </summary>
        /// <param name="farmID"></param>
        /// <param name="webAppID"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        [Obsolete("Use ResetPoint(string farmID, Guid webAppID, string webAppUrl, DateTime point)")]
        public int ResetPoint(string farmID, Guid webAppID, DateTime point)
        {
            #region - executeSql -

            const string executeSql = @"UPDATE CPLED_SyncPoint  SET TimePoint = @TimePoint WHERE FarmID = @FarmID And WebAppID = @WebAppID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@TimePoint", point);
            cmd.AddValue("@FarmID", farmID);
            cmd.AddValue("@WebAppID", webAppID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }
        /// <summary>
        /// 检查是否有记录，有记录更新，没记录插入
        /// </summary>
        /// <param name="farmID"></param>
        /// <param name="webAppID"></param>
        /// <param name="webAppUrl"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public int ResetPoint(string farmID, Guid webAppID, string webAppUrl, DateTime point)
        {
            #region Command Text

            const string executeSql =
                @"  IF EXISTS(SELECT top(1) WebAppID FROM CPLED_SyncPoint  WHERE FarmID = @FarmID AND WebAppID = @WebAppID)
                        UPDATE CPLED_SyncPoint  SET TimePoint = @TimePoint WHERE FarmID = @FarmID AND WebAppID = @WebAppID
                    ELSE
                        INSERT INTO  CPLED_SyncPoint  (FarmID,WebAppID,WebAppURL,TimePoint) VALUES (@FarmID,@WebAppID,@WebAppURL,@TimePoint) ";
            #endregion Command Text

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@FarmID", farmID);
            cmd.AddValue("@WebAppID", webAppID);
            cmd.AddValue("@WebAppURL", webAppUrl);
            cmd.AddValue("@TimePoint", point);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        #endregion
    }
}
