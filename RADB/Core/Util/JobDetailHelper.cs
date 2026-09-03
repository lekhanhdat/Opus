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
using AvePoint.GCommon.Utility.I18N;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.JobMonitor.Util
{
    public class JobDetailHelper
    {
        private static RALogger logger = RALogger.GetInstance(typeof(JobDetailHelper));


        /// <summary>
        /// 默认保留小数点后两位
        /// </summary>
        /// <param name="size">单位为KB</param>
        /// <returns></returns>
        public static string GetDataSizeToView(double size)
        {
            if (size < 1)
            {
                return I18NDataSize(string.Format("{0}{1}", (int)(size * 1024), I18NEntity.GetString("Bytes")));
            }
            else if (size >= 1 && size < 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size, I18NEntity.GetString("KB")));
            }
            else if (size >= 1024 && size < 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / 1024.0, I18NEntity.GetString("MB")));
            }
            else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024 * 1024.0), I18NEntity.GetString("GB")));
            }
            else
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024 * 1024 * 1024.0), I18NEntity.GetString("TB")));
            }
        }

        /// <summary>
        /// 默认保留小数点后两位
        /// </summary>
        /// <param name="size">单位为Byte</param>
        /// <returns></returns>
        public static string GetDataSizeToView(long size)
        {
            if (size < 1024)
            {
                return I18NDataSize(string.Format("{0}{1}", size, I18NEntity.GetString("Bytes")));
            }
            else if (size >= 1024 && size < 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / 1024.0, I18NEntity.GetString("KB")));
            }
            else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024 * 1024.0), I18NEntity.GetString("MB")));
            }
            else if (size >= 1024 * 1024 * 1024 && size < 1024L * 1024 * 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024 * 1024 * 1024.0), I18NEntity.GetString("GB")));
            }
            else
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024L * 1024 * 1024 * 1024.0), I18NEntity.GetString("TB")));
            }
        }
        public static string GetDataSizeToViewForFS(long size)
        {
            if (size < 1024)
            {
                return I18NDataSize(string.Format("{0}{1}", size, I18NEntity.GetString("RM_FS_JobReportSizeUnitBytes")));
            }
            else if (size >= 1024 && size < 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / 1024.0, I18NEntity.GetString("RM_FS_JobReportSizeUnitKB")));
            }
            else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024 * 1024.0), I18NEntity.GetString("RM_FS_JobReportSizeUnitMB")));
            }
            else if (size >= 1024 * 1024 * 1024 && size < 1024L * 1024 * 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024 * 1024 * 1024.0), I18NEntity.GetString("RM_FS_JobReportSizeUnitGB")));
            }
            else
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024L * 1024 * 1024 * 1024.0), I18NEntity.GetString("RM_FS_JobReportSizeUnitTB")));
            }
        }
        public static string GetDataSizeToViewForScreenRestoreReport(long size)
        {
            if (size < 1024)
            {
                return I18NDataSize(string.Format("{0} {1}", size, I18NEntity.GetString("Bytes")));
            }
            else if (size >= 1024 && size < 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F} {1}", size / 1024.0, I18NEntity.GetString("KB")));
            }
            else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F} {1}", size / (1024 * 1024.0), I18NEntity.GetString("MB")));
            }
            else if (size >= 1024 * 1024 * 1024 && size < 1024L * 1024 * 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F} {1}", size / (1024 * 1024 * 1024.0), I18NEntity.GetString("GB")));
            }
            else
            {
                return I18NDataSize(string.Format("{0:F} {1}", size / (1024L * 1024 * 1024 * 1024.0), I18NEntity.GetString("TB")));
            }
        }
        public static string GetDataSizeToViewForRestoreReport(long size)
        {
            double value = size / 1024.0;
            return I18NDataSize(string.Format("{0:F} {1}", Math.Round(value, 2), I18NEntity.GetString("KB")));
        }
        public static string I18NDataSize(string size)
        {
            if (I18NUtility.curCulture == "fr-FR" || I18NUtility.curCulture == "fr-CA")//TODO Cyrus
            {
                return size.Replace(".", ",");
            }
            return size;
        }

        private static bool CopyTable(string tableName, string sourceDBPath, string targetDBPath)
        {
            string createTableSql = null;
            string createIndexSql = null;
            ExecuteSqlAndDoAction(sourceDBPath, string.Format(JobMonitorConstants.GET_TABLE_AND_INDEX_DEFINE, tableName),
                reader =>
                {
                    while (reader.Read())
                    {
                        if (reader["type"].ToString().Equals("table"))
                        {
                            createTableSql = reader["sql"].ToString() + ";";
                        }
                        else
                        {
                            createIndexSql += reader["sql"].ToString() + ";";
                        }
                    }
                });
            return SQLCommond.ExecuteNonQuery(targetDBPath, createTableSql + createIndexSql) > 0;
        }

        public static bool MergeJobDetails(string tableName, string sourceDBPath, string targetDBPath)
        {
            if (!IsExistTable(sourceDBPath, tableName))
            {
                logger.Error($@"source table not exist");
                return false;
            }
            if (!IsExistTable(targetDBPath, tableName))
            {
                if (CopyTable(tableName, sourceDBPath, targetDBPath))
                {
                    logger.Error($@"create target table fail");
                    return false;
                }
            }
            IEnumerable<string> columns = GetTableColumns(sourceDBPath, tableName);
            IEnumerable<string> autoIncreateColumns = GetAutoIncreaseColumns(sourceDBPath, tableName);
            columns = columns.Except(autoIncreateColumns);
            string insertSql = BuildInsertSqlForSpeficalCol(columns, tableName);
            int currentPageDataCount = 0;
            long lastRowId = 0, size = 5000;
            while (true)
            {
                List<List<SQLiteParameter>> datas = PageGetDataForMergeJobDetail(tableName, sourceDBPath, ref lastRowId, size, columns);
                currentPageDataCount = datas.Count;
                if (!SQLCommond.BatchExecuteNonQueryStable(targetDBPath, insertSql, datas))
                {
                    logger.Error($@"insert data fail");
                    return false;
                }
                if (currentPageDataCount < size)
                {
                    break;
            }
            }
            return true;
        }

        private static List<List<SQLiteParameter>> PageGetDataForMergeJobDetail(string tableName, string sourceDBPath, ref long lastRowId, long size, IEnumerable<string> columns)
        {
            List<List<SQLiteParameter>> res = new List<List<SQLiteParameter>>();
            long tempLastId = lastRowId;
            ExecuteSqlAndDoAction(sourceDBPath, string.Format(JobMonitorConstants.PAGE_GET_DATA_BY_CURSOR, tableName, lastRowId, size),
                sqlReader =>
                {
                    while (sqlReader.Read())
                    {
                        List<SQLiteParameter> parameters = new List<SQLiteParameter>();
                        foreach (string col in columns)
                        {
                            if(col.Equals(JobMonitorConstants.Row_ID_COLUMN, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            object value = sqlReader[col];
                            if(value != null && value is Guid)
                            {
                                parameters.Add(new SQLiteParameter($@"@{col}", value?.ToString()));
                            }
                            else
                            {
                                parameters.Add(new SQLiteParameter($@"@{col}", value));
                            }
                        }
                        res.Add(parameters);
                        tempLastId = sqlReader.GetInt64(sqlReader.GetOrdinal(JobMonitorConstants.Row_ID_COLUMN));
                    }
                });
            lastRowId = tempLastId;
            return res;
        }

        public static string GenerateCreateTableSql(string tableName, IEnumerable<ColumnDefinition> columns)
        {
            var columnDefinitions = string.Join(
                ", ",
                columns.Select(c =>
                    string.IsNullOrWhiteSpace(c.Constraint)
                        ? $"{c.Name} {c.Type}"
                        : $"{c.Name} {c.Type} {c.Constraint}"));
            return string.Format(JobMonitorConstants.CREATE_TABLE_WITH_DEFINITION, tableName, columnDefinitions);
        }

        public static void InsertMainJobDetails(string sourceDBPath, RMJobProgress detailInfo, BaseJobDto subJobDto, bool isTeams = false)
        {
            detailInfo.Status = subJobDto.Status;
            detailInfo.JobType = subJobDto.JobType;
            detailInfo.Comment = subJobDto.Comment;
            detailInfo.IsSavedJobDetails = true;
            var summaryTableName = JobMonitorConstants.JOBSUMMAYDETAIL;
            if (!IsExistTable(sourceDBPath, summaryTableName))
            {
                logger.Warn($"Table {summaryTableName} not exist for {subJobDto.Id}");
            }
            else
            {
                GetDataForInsertMainJobDetails(summaryTableName, sourceDBPath, detailInfo, isTeams);
            }
        }

        private static void GetDataForInsertMainJobDetails(string tableName, string sourceDBPath, RMJobProgress detailInfo, bool isTeams)
        {
            ExecuteSqlAndDoAction(sourceDBPath, string.Format(JobMonitorConstants.PAGE_GET_DATA, tableName, 1, 0),
                sqlReader =>
                {
                    if (sqlReader.Read())
                    {
                        var ordinal = sqlReader.GetOrdinal(JobMonitorConstants.SUMMARY_STATISTICS_COLUMN);
                        if (ordinal != -1)
                        {
                            string value = sqlReader.GetString(ordinal);
                            var actionStatistics = string.IsNullOrEmpty(value) ? null : JsonExtension.JsonDeserialize<JMSOSummaryDetails>(value.ToString()).ActionStatistics;
                            long totalSuccess = 0, totalFailed = 0, totalSkipped = 0;
                            foreach (var stat in actionStatistics)
                            {
                                totalSuccess += isTeams ? stat.SuccessfulObj.TeamsTotalCount : stat.SuccessfulObj.TotleCount;
                                totalFailed += isTeams ? stat.FailedObj.TeamsTotalCount : stat.FailedObj.TotleCount;
                                totalSkipped += isTeams ? stat.SkippedObj.TeamsTotalCount : stat.SkippedObj.TotleCount;
                            }
                            detailInfo.Successful = totalSuccess;
                            detailInfo.Failed = totalFailed;
                            detailInfo.Skipped = totalSkipped;
                        }
                    }
                });
        }

        public static bool IsExistTable(string reportFilePath, string tableName)
        {
            //The preferred method is to integrate CheckFileExist with this method,
            //but there are too many methods to refer to. 
            //In order to make it easier to modify and test, only in this method is modified.
            try
            {
                if (System.IO.File.Exists(reportFilePath))
                {
                    return SQLCommond.IsExistTable(reportFilePath, tableName);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.Warn($"IsExistTable error: {e}");
                return false;
            }
        }

        private static string BuildInsertSqlForSpeficalCol(IEnumerable<string> cols, string tableName)
        {
            return $"INSERT INTO {tableName} ({string.Join(",", cols)}) VALUES (@{string.Join(",@", cols)})";
        }

        public static IEnumerable<string> GetTableColumns(string reportFilePath, string tableName)
        {
            Queue<string> res = new Queue<string>();
            ExecuteSqlAndDoAction(reportFilePath, string.Format(JobMonitorConstants.GET_TABLE_ALL_COL, tableName),
                sqlReader =>
                {
                    while (sqlReader.Read())
                    {
                        res.Enqueue(sqlReader[0].ToString());
                    }
                });
            return res;
        }

        public static IEnumerable<string> GetAutoIncreaseColumns(string reportFilePath, string tableName)
        {
            Queue<string> res = new Queue<string>();
            ExecuteSqlAndDoAction(reportFilePath, string.Format(JobMonitorConstants.GET_TABLE_DEFINE, tableName),
                reader =>
                {
                    if (!reader.Read())
                    {
                        logger.Warn(@$"Fail get table define, path:{reportFilePath}, table name:{tableName}");
                        return;
                    }
                    string sql = reader["sql"].ToString();
                    logger.Info($@"FilePath:{reportFilePath}, table define:{sql}");
                    var match = Regex.Match(sql, @"\((.*)\)");
                    if (match.Success)
                    {
                        string columnsPart = match.Groups[1].Value;
                        foreach (string colDef in columnsPart.Split(','))
                        {
                            if (colDef.Contains("autoincrement", StringComparison.OrdinalIgnoreCase))
                            {
                                res.Enqueue(colDef.Trim().Split(" ").First());
                            }
                        }
                    }
                });
            return res;
        }

        private static void ExecuteSqlAndDoAction(string dbPath, string sql, Action<SQLiteDataReader> action, IEnumerable<SQLiteParameter> parameters = null)
        {
            try
            {
                if (SecurityUtils.ValidateSQLiteConnectionWithBuilder(dbPath, out var builder))
                {
                    using (SQLiteConnection conn = new SQLiteConnection(builder.ConnectionString))
                    {
                        conn.Open();
                        try
                        {
                            using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                            {
                                if (parameters != null)
                                {
                                    cmd.Parameters.AddRange(parameters.ToArray());
                                }
                                using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                                {
                                    action(sqlReader);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("{0},{1}", e.Message, e));
                            throw;
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
                else
                {
                    logger.Warn(@$"Fail Validate SQLiteConnection With Builder, report file path:{dbPath}");
                }
            }
            catch (Exception e)
            {
                logger.Error($@"Fail ExecuteSqlAndDoAction,file path:{dbPath},sql:{sql}, ex:{e}");
            }
        }

        private static long GetCountOfTable(string dbPath, string tableName)
        {
            long res = 0;
            ExecuteSqlAndDoAction(dbPath, string.Format(JobMonitorConstants.GET_COUNT_OF_TABLE, tableName),
                reader =>
                {
                    reader.Read();
                    res = long.Parse(reader[0].ToString());
                });
            return res;
        }
    }
}
