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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Excel
{
    public class ExcelExportProcessor4JPMC
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(ExcelExportProcessor4JPMC));
        private string FullPath;
        private string FileName;
        private int SheetIndex = 0;
        private static readonly int ExtPropsMaxColNum = 15;

        public static int ExcelSheetIndex_SiteStats = 0;
        public static int ExcelSheetIndex_Libraries = 1;
        public static int ExcelSheetIndex_DERs = 2;
        public static int ExcelSheetIndex_RCCs = 3;
        public static int ExcelSheetIndex_AllSites = 4;

        private JPMCExcelJsonConfig mJPMCExcelConfig;
        protected IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        public ExcelExportProcessor4JPMC(JPMCExcelJsonConfig config)
        {
            mJPMCExcelConfig = config;
        }

        public async Task InitAsync(string jobId)
        {
            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            //var nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(generalSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
            FileName = "MetricsReport" + "_" + jobId;
            var folderPath = GetTempFolder() + Path.DirectorySeparatorChar + FileName + Guid.NewGuid();
            FullPath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, FileName + ".xlsx");
            logger.Info($"Export file path: {FullPath}");
        }

        private string GetTempFolder()
        {
            return JobReportUtility.GetDownloadRecordExportReportTempleFolder("Temple");
        }

        public void Export(string sheetName, string[][] data)
        {
            if (SheetIndex == 0)
            {
                var folderPath = Path.GetDirectoryName(FullPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                ReportUtil.CreateExcel(FullPath, sheetName, data);
                logger.Info($"Create Excel success, sheet list index is:{SheetIndex}");
            }
            else
            {
                ReportUtil.InsertWorksheet(FullPath, sheetName, data);
                //ReportUtil.InsertWorksheet(FullPath, sheetName + SheetIndex, data);
            }
            SheetIndex++;
        }
        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();
        public async Task UploadBlobAsync(string subJobId)
        {
            var mainJobId = subJobId.Split("_")[0];
            var excelFilePath = FullPath;
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = GCommon.Utility.SecurityUtils.SafeCombinePath(customId, "MergeExcel", mainJobId, FileName + ".xlsx");
            var retryFailed = false;
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    RAStorageUtil.UploadHistoryReport(blobName, excelFilePath);
                    logger.Info($"Upload {excelFilePath} report success");
                    return Task.CompletedTask;
                });
            }
            catch
            {
                retryFailed = true;
                logger.Error($"Upload {excelFilePath} report failed");
            }
            if (retryFailed)
            {
                return;
            }

            logger.Info($"finish to upload blob name:{blobName}");
        }

        public string[][] ConvertData4SiteStats(Hashtable props, List<JPMCExcelColumnConfig> cols, Dictionary<string, string> definedPropsDic)
        {
            var sheetName = "SiteStats";
            logger.Info($"Start converting Data for {sheetName}");
            var configedCols = FilterAndValidateCols(cols, sheetName, JPMCSiteMetricsReportCache.SiteStatsConfigKeys);

            var data = new string[2][];
            data[0] = new string[configedCols.Count];
            data[1] = new string[configedCols.Count];

            for (int i = 0; i < configedCols.Count; i++)
            {
                var col = configedCols[i];

                // head
                data[0][i] = !string.IsNullOrWhiteSpace(col.DisplayName) ? col.DisplayName : col.ConfigKey;

                // body
                if (!definedPropsDic.TryGetValue(col.ConfigKey, out var value))
                {
                    value = !string.IsNullOrWhiteSpace(col.PropertyName) && props.ContainsKey(col.PropertyName)
                        ? props[col.PropertyName]?.ToString() ?? ""
                        : "";
                }
                data[1][i] = value;
            }

            logger.Info($"Finish converting Data for {sheetName}");
            return data;
        }

        public string[][] ConvertData4Libraries(Hashtable props, List<JPMCExcelColumnConfig> cols, Dictionary<int, Dictionary<string, string>> definedPropsDicList)
        {
            var sheetName = "Libraries";
            logger.Info($"Start converting Data for {sheetName}");
            var configedCols = FilterAndValidateCols(cols, sheetName, JPMCSiteMetricsReportCache.LibrariesConfigKeys);

            var data = new string[definedPropsDicList.Count + 1][];
            data[0] = new string[configedCols.Count];

            // head
            for (int i = 0; i < configedCols.Count; i++)
            {
                var col = configedCols[i];
                data[0][i] = !string.IsNullOrWhiteSpace(col.DisplayName) ? col.DisplayName : col.ConfigKey ?? "";
            }

            // body
            foreach (var item in definedPropsDicList)
            {
                var row = item.Key;
                data[row] = new string[configedCols.Count];

                for (int i = 0; i < configedCols.Count; i++)
                {
                    var col = configedCols[i];

                    if (!item.Value.TryGetValue(col.ConfigKey, out var value))
                    {
                        value = !string.IsNullOrWhiteSpace(col.PropertyName) && props.ContainsKey(col.PropertyName)
                        ? props[col.PropertyName]?.ToString() ?? ""
                        : "";
                    }
                    data[row][i] = value;
                }
            }

            logger.Info($"Finish converting Data for {sheetName}");
            return data;
        }

        public string[][] ConvertData4RCCs(Hashtable props, List<JPMCExcelColumnConfig> cols, Dictionary<int, Dictionary<string, string>> definedPropsDicList)
        {
            var sheetName = "RCCs";
            logger.Info($"Start converting Data for {sheetName}");
            var configedCols = FilterAndValidateCols(cols, sheetName, JPMCSiteMetricsReportCache.RCCsConfigKeys);

            var data = new string[definedPropsDicList.Count + 1][];
            data[0] = new string[configedCols.Count];

            // head
            for (int i = 0; i < configedCols.Count; i++)
            {
                var col = configedCols[i];
                data[0][i] = !string.IsNullOrWhiteSpace(col.DisplayName) ? col.DisplayName : col.ConfigKey ?? "";
            }

            // body
            foreach (var item in definedPropsDicList)
            {
                var row = item.Key;
                data[row] = new string[configedCols.Count];

                for (int i = 0; i < configedCols.Count; i++)
                {
                    var col = configedCols[i];

                    if (!item.Value.TryGetValue(col.ConfigKey, out var value))
                    {
                        value = !string.IsNullOrWhiteSpace(col.PropertyName) && props.ContainsKey(col.PropertyName)
                                ? props[col.PropertyName]?.ToString() ?? ""
                                : "";
                    }
                    data[row][i] = value;
                }
            }

            logger.Info($"Finish converting Data for {sheetName}");
            return data;
        }

        public string[][] ConvertData4DERs(Hashtable props, List<JPMCExcelColumnConfig> cols, Dictionary<string, string> definedPropsDic)
        {
            var sheetName = "DERs";
            logger.Info($"Start converting Data for {sheetName}");
            var configedCols = FilterAndValidateCols(cols, sheetName, JPMCSiteMetricsReportCache.DERsConfigKeys);

            var data = new string[2][];
            data[0] = new string[configedCols.Count];
            data[1] = new string[configedCols.Count];

            for (int i = 0; i < configedCols.Count; i++)
            {
                var col = configedCols[i];

                // head
                data[0][i] = !string.IsNullOrWhiteSpace(col.DisplayName) ? col.DisplayName : col.ConfigKey;

                // body
                if (!definedPropsDic.TryGetValue(col.ConfigKey, out var value))
                {
                    value = !string.IsNullOrWhiteSpace(col.PropertyName) && props.ContainsKey(col.PropertyName)
                            ? props[col.PropertyName]?.ToString() ?? ""
                            : "";
                }
                data[1][i] = value;
            }

            logger.Info($"Finish converting Data for {sheetName}");
            return data;
        }

        public string[][] ConvertData4AllSites(Hashtable props, List<JPMCExcelColumnConfig> cols, Dictionary<string, string> definedPropsDic)
        {
            var sheetName = "AllSites";
            logger.Info($"Start converting Data for {sheetName}");
            // all column with valid config key
            // or valid extend columns
            var configedCols = FilterAndValidateCols(cols, sheetName, JPMCSiteMetricsReportCache.AllSitesConfigKeys);

            var data = new string[2][];
            data[0] = new string[configedCols.Count];
            data[1] = new string[configedCols.Count];

            for (int i = 0; i < configedCols.Count; i++)
            {
                var col = configedCols[i];

                // head
                data[0][i] = !string.IsNullOrWhiteSpace(col.DisplayName) ? col.DisplayName : col.ConfigKey;

                // body
                if (!definedPropsDic.TryGetValue(col.ConfigKey, out var value))
                {
                    value = !string.IsNullOrWhiteSpace(col.PropertyName) && props.ContainsKey(col.PropertyName) 
                        ? props[col.PropertyName]?.ToString() ?? ""
                        : "";
                }
                data[1][i] = value;
            }

            logger.Info($"Finish converting Data for {sheetName}");
            return data;
        }

        public JPMCExcelColumnConfig GetTitleCoinfig(int index, string configKey, bool useConfigKeyToDisplay = true)
        {
            JPMCExcelColumnConfig defaultConfig = null;
            if (useConfigKeyToDisplay)
            {
                defaultConfig = new() { ConfigKey = configKey, DisplayName = configKey, PropertyName = configKey };
            }
            else
            {
                defaultConfig = new() { ConfigKey = "", DisplayName = "", PropertyName = "" };
            }
            return mJPMCExcelConfig?.SheetConfigs[index]?.Columns.FirstOrDefault(r => r.ConfigKey == configKey) ?? defaultConfig;
        }

        // not include extend columns
        private bool IsValidConfigKey(string configKey, string sheetName, List<string> configKeysList)
        {
            if (string.IsNullOrWhiteSpace(configKey)) return false;

            if (!configKeysList.Contains(configKey, StringComparer.OrdinalIgnoreCase))
            {
                //logger.Warn($"This config key does not exist in {sheetName}: {configKey}");
                return false;
            }

            return true;
        }

        // extend columns must have display name and not exceed AllSites_ExtendPropsMaxColumnNumber
        private bool IsValidExtendedProperty(string configKey, string displayName)
        {
            if (string.IsNullOrWhiteSpace(configKey) || !configKey.StartsWith("Extended Property ", StringComparison.OrdinalIgnoreCase)) return false;

            string suffix = configKey.Substring("Extended Property ".Length);
            if (!int.TryParse(suffix, out int number)
                || number < 1 || number > ExtPropsMaxColNum
                )
                return false;

            return !string.IsNullOrWhiteSpace(displayName);
        }

        private List<JPMCExcelColumnConfig> FilterAndValidateCols(List<JPMCExcelColumnConfig> cols, string sheetName, List<string> configKeysList)
        {
            var result = new List<JPMCExcelColumnConfig>();
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var col in cols)
            {
                if (!IsValidConfigKey(col.ConfigKey, sheetName, configKeysList) && 
                    !IsValidExtendedProperty(col.ConfigKey, col.DisplayName))
                {
                    logger.Warn($"Skip Invalid Column in sheet {sheetName}. ConfigKey: {col.ConfigKey}, DisplayName: {col.DisplayName}");
                    continue;
                }

                if (!seenKeys.Add(col.ConfigKey))
                {
                    logger.Warn($"Skip Duplicate ConfigKey in sheet {sheetName}. ConfigKey: {col.ConfigKey}");
                    continue;
                }

                result.Add(col);
            }

            return result;
        }
    }
}
