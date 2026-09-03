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
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using AvePoint.Common;
    using AvePoint.Media.Common;
    using AvePoint.RA.Contract.Services;
    using AvePoint.Media.Service.DomainModel;
    using GCommon;
    using RecordsHotfixMaintenanceService;
    #endregion

    internal class IndexDatabaseScriptGenerator
        : IIndexDatabaseScriptGenerator
    {
        readonly static Object syncRoot = new Object();
        private AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Dictionary<String, String> ScriptDictionary = new Dictionary<string, string>() { { "GranularIndexProcessorParameter", "Granular" },{ "GeneralIndexProcessorParameter", "General" },{ "ArchiverIndexProcessorParameter" , "Archiver" },{ "ExportIndexProcessorParameter", "Export" } };

        public String GenerateInitialScript(String databaseType)
        {
            return @"CREATE TABLE [tb_body_index] ( 
    [COL_ID] CHAR(36) not null primary key,
    [COL_FLAG] BIGINT,
	[COL_TYPE] CHAR(2),
    [COL_NAME] VARCHAR(32672), COL_PATH_MD5 CHAR(32),
    [COL_PARENT_PATH_MD5] CHAR(32),
    [COL_DATA_FILE_NUMBER] BIGINT,
    [COL_DATA_FILE_OFFSET] BIGINT,
    [COL_DATA_FILE_LENGTH] BIGINT,
    [COL_CRC] BIGINT,
    [COL_FILE_HEADER_TYPE] INT,
    [COL_ARCHIVE_TIME] BIGINT,
    [COL_CREATE_TIME] BIGINT,
    [COL_MODIFY_TIME] BIGINT,
    [COL_RECYCLE_TIME] BIGINT,
    [COL_RETENTION] VARCHAR(32672),
    [COL_ATTRIBUTES] VARCHAR(32672),
    [COL_EXTRAINFO] VARCHAR(32672),
    [COL_STUBINFO] VARCHAR(32672),
    [COL_PLANID] VARCHAR(32672),
    [COL_CYCLEID] VARCHAR(32672),
    [COL_JOBID] VARCHAR(32672),
    [COL_SEQUENCE] BIGINT,
    [COL_STORAGEPOLICYID] CHAR(36),
    [COL_EXTENSION_1] INT,
    [COL_EXTENSION_2] INT,
    [COL_EXTENSION_3] INT,
    [COL_EXTENSION_4] BIGINT,
    [COL_EXTENSION_5] BIGINT,
    [COL_EXTENSION_6] VARCHAR(32672),
    [COL_EXTENSION_7] VARCHAR(32672),
    [COL_EXTENSION_8] VARCHAR(32672),
    [COL_EXTENSION_9] TEXT,
    [COL_EXTENSION_10] TEXT,
    [COL_CONTENT_DATA_OFFSET] BIGINT,
    [COL_CONTENT_DATA_FILE_NUMBER] BIGINT,
    [COL_CONTENT_DATA_FILE_PREFIX_NUMBER] BIGINT,
    [COL_STORAGEINFO] TEXT,
    [COL_META_DATA_HEADER_OFFSET] BIGINT,
    [COL_CONTENT_DATA_HEADER_OFFSET] BIGINT,
    [COL_CONTENT_PAGE_SIZE] BIGINT,
    [COL_STATUS] BIGINT default 0,
    [COL_PRUNE_TIME] BIGINT,
    [COL_BLOB_INFO] TEXT,
	[COL_SITE_PATH] VARCHAR(255),
	[COL_RETENTION_STATUS] INT,
	[COL_META_TAIL_LENGTH] BIGINT
);

CREATE TABLE [tb_job_info](
    [COL_GUID] VARCHAR(36) not null primary key,
    [COL_JOB_ID] VARCHAR(32672),
    [COL_KEY] VARCHAR(32672),
    [COL_VALUE] VARCHAR(32672),
    [COL_EXTENSION_3] INT,
    [COL_EXTENSION_4] BIGINT,
    [COL_EXTENSION_5] BIGINT,
    [COL_EXTENSION_6] VARCHAR(32672),
    [COL_EXTENSION_7] VARCHAR(32672),
    [COL_EXTENSION_8] VARCHAR(32672),
    [COL_EXTENSION_9] TEXT,
    [COL_EXTENSION_10] TEXT
);
CREATE TABLE [tb_master_index_info](
    [COL_GUID] VARCHAR(36) not null primary key,
    [COL_UNC_PATH] VARCHAR(32672),
    [COL_JOB_ID] VARCHAR(32672),
    [COL_CONNECTION_PATH] VARCHAR(32672),
    [COL_CONNECTIONID] VARCHAR(32672),
    [COL_ARCHIVER_TIME] BIGINT,
    [COL_EXTENSION_3] INT,
    [COL_EXTENSION_4] BIGINT,
    [COL_EXTENSION_5] BIGINT,
    [COL_EXTENSION_6] VARCHAR(32672),
    [COL_EXTENSION_7] VARCHAR(32672),
    [COL_EXTENSION_8] VARCHAR(32672)
);

INSERT INTO tb_job_info (COL_GUID, COL_KEY, COL_VALUE) VALUES( 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 'version','6.0.0.0');

CREATE INDEX 
            IF NOT EXISTS IDX_BODY_DUP_STATUS_RECYCLE_TIME
            on tb_body_index(COL_EXTENSION_3, COL_RECYCLE_TIME asc);";
        }

        public String GenerateUpgradeScript(
            String databaseType,
            Func<String, String, Boolean> checkColumn)
        {
            var result = new StringBuilder();

            var scriptRelativeName = this.ScriptDictionary[databaseType];
            var scriptFullPath = ServiceConstants.IndexDatabaseUpgradeScriptPathTemplate.FormatWith(scriptRelativeName);
            var unpackedFileName = scriptRelativeName + @"." + Guid.NewGuid().ToString() + @".config";
            var uppackedFileFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, unpackedFileName);
            lock (syncRoot)
            {
                var unpackedResult = this.UnpackUpgradeConfigFileName(scriptFullPath, uppackedFileFullPath);
                if (unpackedResult)
                {
                    var config = ConfigurationManager.OpenMappedExeConfiguration(
                        new ExeConfigurationFileMap() { ExeConfigFilename = uppackedFileFullPath }, ConfigurationUserLevel.None);
                    var upgradeConfig = config.GetSection(ServiceConstants.UpgradeConfigurationSectionHandlerName)
                        as UpgradeConfigurationSectionHandler;
                    var upgradeCollection = upgradeConfig.UpgradeConfigurationCollection;
                    foreach (UpgradeConfiguration item in upgradeCollection)
                    {
                        var upgradeType = (UpgradeType)Enum.Parse(typeof(UpgradeType), item.UpgradeType, ignoreCase: true);
                        if (upgradeType == UpgradeType.Column)
                        {
                            if (!checkColumn(item.ColumnName, item.TableName))
                            { result.Append(item.UpgradeExpression); }
                        }
                        else if (upgradeType == UpgradeType.Table)
                        { result.Append(item.UpgradeExpression); }
                    }
                }
                try
                {
                    File.Delete(uppackedFileFullPath);
                }
                catch (Exception e)
                {
                    logger.Error("An error occured while delete the upgrade script file {0},details:{1}.", uppackedFileFullPath.LogBase64(), e.Message);
                }
                return result.ToString();
            }
        }

        Boolean UnpackUpgradeConfigFileName(String resourceName, String targetPath)
        {
            var result = default(Boolean);
            var exePath = Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
            var exeDateUtc = File.GetLastWriteTimeUtc(exePath);
            if (File.Exists(targetPath))
            {
                if (File.GetLastWriteTimeUtc(targetPath) > exeDateUtc) result = 1 < 2;
                else result = ResourceUtility.UnpackResourceAsFile(resourceName, targetPath, Assembly.GetExecutingAssembly());
            }
            else result = ResourceUtility.UnpackResourceAsFile(resourceName, targetPath, Assembly.GetExecutingAssembly());
            return result;
        }
    }
}
