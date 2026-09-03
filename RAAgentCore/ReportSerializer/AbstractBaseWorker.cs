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
using AvePoint.GCommon;
using AvePoint.RA.Common.Database;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RAFileSystemCore.ReportSerializer
{
    abstract public class AbstractBaseWorker<T>
    {
        protected readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected readonly RALogger RAlogger = RALogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal string TABLE_NAME { get; set; }
        internal string CREATE_TABLE_SQL { get; set; }
        internal string INSERT_DATA_SQL { get; set; }
        internal string SELECT_DATA_SQL { get; set; }
        internal string SELECT_REPORT_COUNT_SQL { get; set; }

        private string mExpandedName = ".rpt";
        public string ExpandedName
        {
            get { return mExpandedName; }
            set { mExpandedName = value; }
        }
        abstract public void SaveReportJobDatas(IEnumerable<T> baseJobs, string location);

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="location"></param>
        /// <returns>数据库的路径</returns>
        public virtual string NeedCreateTable(string reportFilePath)
        {
            logger.Info("Set report file path.");
            if (!CheckFileExist(reportFilePath) || !SQLiteUtil.IsExistTable(reportFilePath, TABLE_NAME))    //文件存在  并且  表存在时  不需要新创建表
            {
                CreateTableNew(reportFilePath);
            }
            return reportFilePath;
        }

        public virtual string NeedCreateTable(string reportFilePath, string tableName, string createTableSql)
        {
            logger.Info("Set report file path.");
            if (!CheckFileExist(reportFilePath) || !SQLiteUtil.IsExistTable(reportFilePath, tableName))    //文件存在  并且  表存在时  不需要新创建表
            {
                CreateTableNew(reportFilePath, tableName, createTableSql);
            }
            return reportFilePath;
        }

        /// <summary>
        /// 检查是否存在path路径下的同名文件。
        /// </summary>
        /// <param name="path">.rpt文件的位置</param>
        /// <returns>是否存在</returns>
        public bool CheckFileExist(string path)
        {
            FileInfo file = new FileInfo(path);
            return file.Exists;
        }
        /// <summary>
        /// 创建表
        /// </summary>
        protected void CreateTableNew(string reportFilePath)
        {
            try
            {
                CheckAndCreateDirectory(reportFilePath);
                SQLiteUtil.ExecuteNonQuery(RAlogger, reportFilePath, CREATE_TABLE_SQL);
                logger.Debug("Successfulfull to create table {0}.", TABLE_NAME);
            }
            catch (Exception ex)
            {
                logger.Error("failed to create table {0}.", TABLE_NAME);
                logger.Error(ex.ToString());
            }

        }

        protected void CreateTableNew(string reportFilePath, string tableName, string createTableSql)
        {
            try
            {
                CheckAndCreateDirectory(reportFilePath);
                SQLiteUtil.ExecuteNonQuery(RAlogger, reportFilePath, createTableSql);
                logger.Debug("Successfulfull to create table {0}.", tableName);
            }
            catch (Exception ex)
            {
                logger.Error("failed to create table {0}.", tableName);
                logger.Error(ex.ToString());
            }

        }

        /// <summary>
        /// 检验是否存在目录  不存在时直接创建目录
        /// </summary>
        protected void CheckAndCreateDirectory(string reportFilePath)
        {
            FileInfo reportFile = new FileInfo(reportFilePath);
            if (!reportFile.Directory.Exists)
            {
                reportFile.Directory.Create();
                logger.Debug("Create Directory:", reportFile.Directory);
            }

        }


        public void InitGetDataCountSQLString(string conditionFilter)
        {
            if (string.IsNullOrEmpty(conditionFilter))
            {
                SELECT_REPORT_COUNT_SQL = string.Format(ReportConstants.SELECT_REPORT_COUNT_SQL, TABLE_NAME);
            }
            else
            {
                SELECT_REPORT_COUNT_SQL = string.Format(ReportConstants.SELECT_REPORT_COUNT_ON_CONDITION_SQL, TABLE_NAME, conditionFilter);
            }
        }

    }
}
