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
    using GCommon;
    using RecordsHotfixMaintenanceService;
    #endregion

    internal class IndexDatabaseScriptGenerator
        : IIndexDatabaseScriptGenerator
    {
        readonly static Object syncRoot = new Object();
        private AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Dictionary<String, String> ScriptDictionary { get; set; }

        public String GenerateInitialScript(String databaseType)
        {
            var scriptRelativeName = this.ScriptDictionary[databaseType];
            var scriptFullPath = ServiceConstants.IndexDatabaseInitialScriptPathTemplate.FormatWith(scriptRelativeName);
            var initialScript = ResourceUtility.UnpackResourceAsString(scriptFullPath, Assembly.GetExecutingAssembly());
            return initialScript.Replace(ServiceConstants.VersionTemplateValue, RecordsEnv.ProductVersion.ToString());
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
                    logger.Error("An error occured while delete the upgrade script file {0},details:{1}.", uppackedFileFullPath, e.Message);
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
