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




namespace AvePoint.Media.Core.Index
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;
    using AvePoint.Common;
    using AvePoint.GCommon;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.Contract.Services;
    using System.IO;
    using System.Diagnostics;

    #endregion

    public sealed class IndexProcessor<TIndexProcessorParameter>
        : IIndexProcessor<TIndexProcessorParameter>
          where TIndexProcessorParameter : IndexProcessorParameter
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IndexDatabaseHelper dbHelper = new IndexDatabaseHelper();
        String CheckColumnCommandTemplateText = "SELECT SQL FROM SQLITE_MASTER WHERE tbl_name='{0}' AND lower(sql) LIKE lower('%{1}%')";
        String currentIndexFullPath;
        private Boolean needCheckIntegrity;

        IIndexDatabaseScriptGenerator scriptGenerator = new IndexDatabaseScriptGenerator();
        IIndexDatabaseUpgradeManager upgradeManager = new IndexDatabaseUpgradeManager();

        #region IIndexProcessor Interface Member

        public void Open(TIndexProcessorParameter param)
        {
            var openMode = param.DownLoadResult.Status.GetAttribute<OpenModeAttribute>().OpenMode;
            needCheckIntegrity = param.IsNeedCheckIntegrity;
            if (!string.IsNullOrEmpty(param.DBPassWord))
            {
                this.InnerOpen(
                databaseFullPath: param.DownLoadResult.IndexFullPath,
                openMode: openMode,
                isForceUpgrade: param.IsForceUpgrade,
                param.DBPassWord);
            }
            else
            {
                this.InnerOpen(
                    databaseFullPath: param.DownLoadResult.IndexFullPath,
                    openMode: openMode,
                    isForceUpgrade: param.IsForceUpgrade);
            }
        }
        public void Close(Boolean isCheckIntegrity = false)
        {
            try
            {
                logger.Info("Begin to close IndexProcessor.isCheckIntegrity: {0},needCheckIntegrity:{1},currentIndexFullPath:{2}", isCheckIntegrity, needCheckIntegrity, currentIndexFullPath.LogBase64());
                if (!string.IsNullOrEmpty(this.currentIndexFullPath) && isCheckIntegrity && this.needCheckIntegrity)
                {
                    var checkResult = CheckIntegrity();
                    if (!checkResult.EqualsIgnoreCase("ok"))
                    {
                        throw new Exception(string.Format("The SQLite .db file is not integrated. Details: {0}", checkResult));
                    }
                }
            }
            finally
            {
                this.dbHelper.Close();
                this.currentIndexFullPath = null;
                logger.Info("Finish closing IndexProcessor.");
            }
        }

        public String CheckIntegrity()
        {
            logger.Info("Begin to Check Index DB Integrity:{0}", currentIndexFullPath.LogBase64());
            DateTime start = DateTime.Now;
            string commandText = "PRAGMA integrity_check";
            if (File.Exists(currentIndexFullPath)) 
            {
                FileInfo info = new FileInfo(currentIndexFullPath);
                if (info.Length > 1024 * 1024 * 1024) // >1GB
                {
                    commandText = "PRAGMA quick_check";
                }
            } 
            string checkResult = String.Empty;
            logger.Info("Index DB Check method:{0}", commandText.LogBase64());
            checkResult = (string)this.dbHelper.ExecuteScalar(commandText);
            logger.Info("Check action time cost {0}.CheckMethod:{1}.The check result for the SQLite .db file is {2}.", DateTime.Now - start, commandText.LogBase64(), checkResult);
            return checkResult;
        }

        public void Execute(String commandText, Dictionary<String, Object> parameters)
        {
            this.dbHelper.ExecuteNonQuery(commandText: commandText, parameters: parameters);
        }

        public void Execute(String tableName, DataTable dataTable)
        {
            this.dbHelper.ExecuteNonQuery(tableName: tableName, dataTable: dataTable);
        }

        public void Insert<TIndexable>(List<TIndexable> indexes) where TIndexable : IIndexable
        {
            this.dbHelper.ExecuteNonQuery<TIndexable>(indexList: indexes);
        }

        public void Insert<TIndexable>(TIndexable index) where TIndexable : IIndexable
        {
            Insert<IIndexable>(new List<IIndexable> { index });
        }

        public List<TIndexable> ExecuteQuery<TIndexable>(String commandText, Dictionary<String, Object> parameters)
            where TIndexable : IIndexable
        {
            return this.dbHelper.ExecuteReader<TIndexable>(commandText: commandText, parameters: parameters);
        }

        public DataTable ExecuteQuery(String commandText, Dictionary<String, Object> parameters)
        {
            return this.dbHelper.ExecuteReader(commandText: commandText, parameters: parameters);
        }

        public Object ExecuteScalar(String commandText, Dictionary<String, Object> parameters)
        {
            return this.dbHelper.ExecuteScalar(commandText: commandText, parameters: parameters);
        }

        public List<T> ExecuteQueryForOneColume<T>(String commandText, Dictionary<String, Object> parameters)
            where T : class
        {
            return this.dbHelper.ExecuteQueryForOneColume<T>(commandText: commandText, parameters: parameters);
        }
        public List<Int64> ExecuteQueryForOneColumeInt64(String commandText, Dictionary<String, Object> parameters)
        {
            return this.dbHelper.ExecuteQueryForOneColumeInt64(commandText: commandText, parameters: parameters);
        }
        public List<T> ExecuteQueryForAllClass<T>(String commandText, Dictionary<String, Object> parameters)
           where T : class
        {
            return this.dbHelper.ExecuteQueryForAllClass<T>(commandText: commandText, parameters: parameters);
        }
        #endregion

        void InnerOpen(
            String databaseFullPath,
            IndexDatabaseOpenMode openMode,
            Boolean isForceUpgrade)
        {
            if (!(this.dbHelper.IsOpen
                && databaseFullPath.EqualsIgnoreCase(this.currentIndexFullPath)))
            {
                if (this.currentIndexFullPath != null)
                {
                    this.logger.Info($"IndexProcessor Inner Open UseDBCurrentCatalog:{this.currentIndexFullPath.LogBase64()}");
                    this.dbHelper.Close();
                }
                this.currentIndexFullPath = databaseFullPath;

                this.dbHelper.Open("Data Source = {0}".FormatWith(this.currentIndexFullPath));
                if (openMode == IndexDatabaseOpenMode.Create)
                {
                    try
                    {
                        FileInfo finfo = new FileInfo(this.currentIndexFullPath);
                        if (finfo.Exists)
                        {
                            this.logger.Warn("The index file exists {0} Create time{1}", this.currentIndexFullPath.LogBase64(), finfo.CreationTimeUtc.ToString());

                            foreach (var file in finfo.Directory.GetFiles())
                            {
                                this.logger.Info(file.FullName.LogBase64());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this.logger.Warn("check index file exists failed error :{0}", e.ToString());
                    }
                    var initialDatabaseScript = this.scriptGenerator.GenerateInitialScript(typeof(TIndexProcessorParameter).Name);
                    this.dbHelper.ExecuteNonQuery(initialDatabaseScript, default(Dictionary<String, Object>));
                }
                else if (openMode == IndexDatabaseOpenMode.Open && isForceUpgrade == true)
                {
                    this.upgradeManager.DatabaseHelper = this.dbHelper;
                    //var isNeedUpgradeIndexDatabase = this.upgradeManager.CheckUpgrade(this.currentIndexFullPath);
                    //if (isNeedUpgradeIndexDatabase)
                    //{
                    //    var upgradeDatabaseScript = this.scriptGenerator.GenerateUpgradeScript(typeof(TIndexProcessorParameter).Name, (columnName, tableName) =>
                    //    {
                    //        var checkColumnCommand = this.CheckColumnCommandTemplateText.FormatWith(tableName, columnName);
                    //        var checkResult = this.dbHelper.ExecuteScalar(checkColumnCommand);
                    //        return !Validator.IsNullOrEmpty(checkResult);
                    //    });
                    //    this.upgradeManager.Upgrade(upgradeDatabaseScript);
                    //}
                }
            }
        }

        void InnerOpen(
            String databaseFullPath,
            IndexDatabaseOpenMode openMode,
            Boolean isForceUpgrade,
            string dbPassWord)
        {
            if (!(this.dbHelper.IsOpen
                && databaseFullPath.EqualsIgnoreCase(this.currentIndexFullPath)))
            {
                if (this.currentIndexFullPath != null)
                {
                    this.logger.Info($"IndexProcessor Inner Open UseDBCurrentCatalog:{this.currentIndexFullPath.LogBase64()}");
                    this.dbHelper.Close();
                }
                this.currentIndexFullPath = databaseFullPath;

                this.dbHelper.Open(this.currentIndexFullPath, dbPassWord.Replace("\"", "#").Replace("\\", "*"));
                if (openMode == IndexDatabaseOpenMode.Create)
                {
                    try
                    {
                        FileInfo finfo = new FileInfo(this.currentIndexFullPath);
                        if (finfo.Exists)
                        {
                            this.logger.Info("The index file exists {0}", this.currentIndexFullPath.LogBase64());

                            foreach (var file in finfo.Directory.GetFiles())
                            {
                                this.logger.Info($"{file.FullName.LogBase64()} | {file.Length}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this.logger.Warn("check index file exists failed error :{0}", e.ToString());
                    }
                    var initialDatabaseScript = this.scriptGenerator.GenerateInitialScript(typeof(TIndexProcessorParameter).Name);
                    this.dbHelper.ExecuteNonQuery(initialDatabaseScript, default(Dictionary<String, Object>));
                }
                //else if (openMode == IndexDatabaseOpenMode.Open && isForceUpgrade == true)
                //{
                //    this.upgradeManager.DatabaseHelper = this.dbHelper;
                //    var isNeedUpgradeIndexDatabase = this.upgradeManager.CheckUpgrade(this.currentIndexFullPath);
                //    if (isNeedUpgradeIndexDatabase)
                //    {
                //        var upgradeDatabaseScript = this.scriptGenerator.GenerateUpgradeScript(typeof(TIndexProcessorParameter).Name, (columnName, tableName) =>
                //        {
                //            var checkColumnCommand = this.CheckColumnCommandTemplateText.FormatWith(tableName, columnName);
                //            var checkResult = this.dbHelper.ExecuteScalar(checkColumnCommand);
                //            return !Validator.IsNullOrEmpty(checkResult);
                //        });
                //        this.upgradeManager.Upgrade(upgradeDatabaseScript);
                //    }
                //}
            }
        }
    }
}