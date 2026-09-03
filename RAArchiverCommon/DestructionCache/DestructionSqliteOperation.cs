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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.SharePoint.ArchiverCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAArchiverCommon.DestructionCache
{
    internal class DestructionSqliteOperation : SqliteDBBase
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(DestructionSqliteOperation));
        private string tableName = "DestructionCacheTable";
        public DestructionSqliteOperation(string dirPath, string name) : base(dirPath, name)
        {           
        }

        public override void CreateSchemaIfNotExists(IDbCommand command)
        {
            string query = string.Format($"CREATE TABLE IF NOT EXISTS {SecurityUtils.SanitizeSQLSchemaName(tableName)}(" +
                "[NodeID] [nvarchar](2000) not null," +
                "[ListId] [uniqueidentifier]," +
                "[RuleID] [uniqueidentifier]," +
                "[ArchivedTime] [bigint]," +
                "[JsonMeta] [nvarchar](2000)," +
                "[FullPath] [nvarchar](2000)," +
                "[ActionType] [int]," +                
                "[SortTicks] [nvarchar](500)" +                
                ");" +
                $"CREATE INDEX IF NOT EXISTS SortTicksIndex ON {SecurityUtils.SanitizeSQLSchemaName(tableName)}(SortTicks asc)");

            command.CommandText = query;
            command.ExecuteNonQuery();
        }

        public void InsertValueToDB(List<DestructionReport> destructionReports)
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.DestructionUtility.InsertValueToDB", addToStatistics: true))
            {
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        InternalInsertValueToDB(connection, command, destructionReports);
                    }
                });
            }
        }

        private void InternalInsertValueToDB(SQLiteConnection conn, IDbCommand command, List<DestructionReport> archiverEntities)
        {
            using (SQLiteTransaction tr = conn.BeginTransaction())
            {
                foreach (var archiverEn in archiverEntities)
                {
                    StringBuilder query = new StringBuilder();
                    query.Append($"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(tableName)}(NodeID,ListId,RuleID,ArchivedTime,JsonMeta,FullPath,ActionType,SortTicks) ");
                    query.Append(@"VALUES (@NodeID,@ListId,@RuleID,@ArchivedTime,@JsonMeta,@FullPath,@ActionType,@SortTicks)");
                    SQLiteParameter[] parameters = {
                    new SQLiteParameter("@NodeID"),
                    new SQLiteParameter("@ListId"),
                    new SQLiteParameter("@RuleID"),
                    new SQLiteParameter("@ArchivedTime"),
                    new SQLiteParameter("@JsonMeta"),
                    new SQLiteParameter("@FullPath"),
                    new SQLiteParameter("@ActionType"),
                    new SQLiteParameter("@SortTicks"),
                    };
                    parameters[0].Value = archiverEn.NodeId;
                    parameters[1].Value = archiverEn.ListId;
                    parameters[2].Value = archiverEn.RuleID;
                    parameters[3].Value = archiverEn.ArchivedTime;
                    parameters[4].Value = archiverEn.JsonMeta;
                    parameters[5].Value = archiverEn.FullPath;
                    parameters[6].Value = archiverEn.ActionType;
                    parameters[7].Value = archiverEn.SortTicks;                    
                    foreach (var para in parameters)
                    {
                        command.Parameters.Add(para);
                    }
                    command.CommandText = query.ToString();
                    command.ExecuteNonQuery();
                }
                tr.Commit();
            }
        }


        public List<DestructionReport> SelectValuesFromDB(int offset, int pageSize)
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.DestructionUtility.SelectValuesFromDB", addToStatistics: true))
            {
                List<DestructionReport> reports = new List<DestructionReport>();
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        if (CheckColumnExist(command, "ActionType"))
                        {
                            mLog.Info($"This report is new schema, file path:{_dbFilePath}");
                            reports = InternalSelectValuesFromDB4NewSchema(command, offset, pageSize);
                        }
                        else
                        {
                            mLog.Info($"This report is old schema, file path:{_dbFilePath}");
                            reports = InternalSelectValuesFromDB(command, offset, pageSize);
                        }
                    }
                });
                return reports;
            }
        }
        private bool CheckColumnExist(IDbCommand command, string colName) {

            command.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE name = @tableName AND sql LIKE @colName";
            command.Parameters.Add(new SQLiteParameter("@tableName", tableName));
            command.Parameters.Add(new SQLiteParameter("@colName", $"%{colName}%"));
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
        private List<DestructionReport> InternalSelectValuesFromDB(IDbCommand command, int offset, int pageSize)
        {
            List<DestructionReport> archiverEntities = new List<DestructionReport>();
            string query = string.Format(@$"SELECT " +
                "[NodeID]," +
                "[ListId]," +
                "[RuleID]," +
                "[ArchivedTime]," +
                "[JsonMeta]," +
                "[FullPath]," +
                "[SortTicks]" +
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} order by SortTicks limit @pageSize offset @offset");

            command.CommandText = query;
            command.Parameters.Add(new SQLiteParameter("@pageSize", pageSize));
            command.Parameters.Add(new SQLiteParameter("@offset", offset));
            using (var sr = command.ExecuteReader())
            {
                while (sr.Read())
                {
                    DestructionReport archiverEn = new DestructionReport();
                    archiverEn.NodeId = sr.GetString(0);
                    archiverEn.ListId = sr.GetGuid(1);
                    archiverEn.RuleID = sr.GetGuid(2);
                    archiverEn.ArchivedTime = sr.GetInt64(3);
                    archiverEn.JsonMeta = sr.GetString(4);
                    archiverEn.FullPath = sr.GetString(5);
                    archiverEn.SortTicks = sr.GetString(6);
                    archiverEntities.Add(archiverEn);
                }
            }
            return archiverEntities;
        }

        private List<DestructionReport> InternalSelectValuesFromDB4NewSchema(IDbCommand command, int offset, int pageSize)
        {
            List<DestructionReport> archiverEntities = new List<DestructionReport>();
            string query = string.Format("SELECT " +
                "[NodeID]," +
                "[ListId]," +
                "[RuleID]," +
                "[ArchivedTime]," +
                "[JsonMeta]," +
                "[FullPath]," +
                "[ActionType]," +
                "[SortTicks]" +
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} order by SortTicks limit {pageSize} offset {offset}");

            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                while (sr.Read())
                {
                    DestructionReport archiverEn = new DestructionReport();
                    archiverEn.NodeId = sr.GetString(0);
                    archiverEn.ListId = sr.GetGuid(1);
                    archiverEn.RuleID = sr.GetGuid(2);
                    archiverEn.ArchivedTime = sr.GetInt64(3);
                    archiverEn.JsonMeta = sr.GetString(4);
                    archiverEn.FullPath = sr.GetString(5);
                    archiverEn.ActionType = sr.GetInt32(6);
                    archiverEn.SortTicks = sr.GetString(7);
                    archiverEntities.Add(archiverEn);
                }
            }
            return archiverEntities;
        }

        public string GetFilePath()
        {
            return base._dbFilePath;
        }


        public int GetTotalCount()
        {
            int totalCount = 0;
            string query = string.Format($"SELECT COUNT(*)  FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)}");
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.DestructionUtility.GetTotalCount", addToStatistics: true))
            {
                List<DestructionReport> reports = new List<DestructionReport>();
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        using (var sr = command.ExecuteReader())
                        {
                            if (sr.Read())
                            {
                                totalCount = sr.GetInt32(0);
                            }
                        }
                    }
                });
            }
            return totalCount;
        }
    }
}
