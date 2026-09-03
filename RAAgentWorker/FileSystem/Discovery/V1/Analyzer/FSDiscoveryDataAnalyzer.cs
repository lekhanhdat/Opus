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
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Services;
using Newtonsoft.Json;
using RAFileSystem.FileSystem.Discovery.V1.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RAFileSystem.FileSystem.Discovery.V1.Analyzer
{
    public class FSDiscoveryDataAnalyzer
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly FSDiscoveryAnalyzedConnectionDataInfo _connDataInfo = new FSDiscoveryAnalyzedConnectionDataInfo();

        private readonly Dictionary<int, FSDiscoveryAnalyzedDataInfo> _analyzedDataInfoes = new Dictionary<int, FSDiscoveryAnalyzedDataInfo>();

        private const string LOCAL_FOLDER_NAME = "fs-analyzed-file-container";

        private static string OpusTenantId => CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);

        private Guid _connectionId;

        private string _localFilePath;

        private readonly object _lockObject = new object();

        public FSDiscoveryDataAnalyzer(Guid connectionId)
        {
            _connectionId = connectionId;
            _localFilePath = GetLocalFilePath();
        }

        public void Analyze(FileInfo file, Dictionary<string, object> tagValues)
        {
            if(file == null && tagValues == null)
            {
                _analyzedDataInfoes.Add(DateTime.Now.GetHashCode(), new FSDiscoveryAnalyzedDataInfo()
                {
                    DateRangeId = 0,
                    SizeRangeId = 0,
                    FileExtension = string.Empty,
                    AggregationInfo = new FSDiscoveryAnalyzedDataAggregationInfo()
                    {
                        FileSumCount = 0,
                        FileTotalSize = 0
                    },
                    RuleData = new Dictionary<Guid, FSDiscoveryAnalyzedDataAggregationInfo> { }
                });
                return;
            }
            AnalyzeConnectionDataInfo(file);
            AnalyzeFileDataInfo(file, tagValues);
        }

        public void CommitAnalyzedFile()
        {
            try
            {
                s_logger.Info($"Begin commit analyzed file. Path [{_localFilePath.LogBase64()}].");
                WriteAnalyzedDataToFile();
                UploadAnalyzedFile();
            }
            catch (Exception ex)
            {
                s_logger.Error($"An error occurred while commit analyzed file. Ex: {ex}");
                throw;
            }
            finally
            {
                CleanLocalFilesPath();
            }
        }

        public void Init()
        {
            try
            {
                FSDiscoveryTagRuleService.Instance.InitTagRuleInfos();
                var fileInfo = new FileInfo(_localFilePath);
                if (!fileInfo.Directory.Exists)
                {
                    s_logger.Info($"Creating directory for analyzed file. Path [{fileInfo.Directory.FullName?.LogBase64()}].");
                    fileInfo.Directory.Create();
                }
            }
            catch (Exception ex)
            {
                s_logger.Error($"An error occurred while initializing the FSDiscoveryDataAnalyzer. Ex: {ex}");
                throw;
            }
        }

        public long GetProcessedFileCount() => _connDataInfo.FileSumCount;

        #region Private Methods
        private void AnalyzeConnectionDataInfo(FileInfo file)
        {
            _connDataInfo.FileTotalSize += file.Length;
            _connDataInfo.FileSumCount++;
            _connDataInfo.VersionTotalSize += file.Length;
            _connDataInfo.MinCreatedMonth = Math.Min(_connDataInfo.MinCreatedMonth, long.Parse(file.CreationTime.ToString("yyyyMM")));
        }

        private void AnalyzeFileDataInfo(FileInfo file, Dictionary<string, object> tagValues)
        {
            try
            {
                var dataInfo = new FSDiscoveryAnalyzedDataInfo();
                TryGetInt32Value(FSTagRuleConstants.SIZE_RANGE_NAME, tagValues, out var sizeRangeId);
                TryGetInt32Value(FSTagRuleConstants.DATE_RANGE_COLUMN_NAME, tagValues, out var dateRangeId);
                dataInfo.SizeRangeId = sizeRangeId;
                dataInfo.DateRangeId = dateRangeId;
                dataInfo.FileExtension = file?.Extension?.Replace(".", "") ?? string.Empty;
                var hashCode = dataInfo.GetHashCode();

                lock (_lockObject)
                {
                    if (!_analyzedDataInfoes.ContainsKey(hashCode))
                    {
                        _analyzedDataInfoes.Add(hashCode, dataInfo);
                    }
                    else
                    {
                        dataInfo = _analyzedDataInfoes[hashCode];
                    }
                }

                dataInfo.AggregationInfo.FileTotalSize += file.Length;
                dataInfo.AggregationInfo.FileSumCount++;

                foreach (var customRule in FSDiscoveryTagRuleService.Instance.GetCustomTagRules())
                {
                    if (TryGetInt64Value(customRule.Name, tagValues, out long matchValue))
                    {
                        if (!dataInfo.RuleData.TryGetValue(customRule.Id, out var ruleData))
                        {
                            dataInfo.RuleData[customRule.Id] = ruleData = new FSDiscoveryAnalyzedDataAggregationInfo();
                        }
                        ruleData.FileTotalSize += matchValue;
                        ruleData.FileSumCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                s_logger.Error($"An error occurred while analyzing file data info [{file.Name.LogBase64()}]. Ex: {ex}");
                throw;
            }
        }

        private static bool TryGetInt32Value(string tagColumn, Dictionary<string, object> tagValues, out int value)
        {
            value = 0;
            if (!tagValues.TryGetValue(tagColumn, out var tagValue))
            {
                return false;
            }

            if (!int.TryParse(tagValue.ToString(), out value))
            {
                return false;
            }

            return true;
        }

        private static bool TryGetInt64Value(string tagColumn, Dictionary<string, object> tagValues, out long value)
        {
            value = 0;
            if (!tagValues.TryGetValue(tagColumn, out var tagValue))
            {
                return false;
            }

            if (!long.TryParse(tagValue.ToString(), out value))
            {
                return false;
            }

            return true;
        }

        private void WriteAnalyzedDataToFile()
        {
            using (var writer = File.AppendText(_localFilePath))
            {
                s_logger.Info($"Begin write analyzed data to file. Path [{_localFilePath.LogBase64()}], Count [{_analyzedDataInfoes.Values.Count}] .");
                writer.WriteLine(JsonConvert.SerializeObject(_connDataInfo));
                foreach (var dataInfo in _analyzedDataInfoes.Values)
                {
                    if (dataInfo != null)
                    {
                        writer.WriteLine(JsonConvert.SerializeObject(dataInfo));
                    }
                }
            }
        }

        private void UploadAnalyzedFile()
        {
            try
            {
                var fileName = Path.GetFileName(_localFilePath);
                var blockBytes = 1024 * 1024; //1M
                using (var file = File.OpenRead(_localFilePath))
                {
                    int bytesRead;
                    var buffer = new byte[blockBytes];
                    while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var newArry = new Span<byte>(buffer, 0, bytesRead).ToArray();
                        HybridApiClient.Instance.UploadAnalyzedFileToStorage(new DiscoveryAnalyzedDataInfo
                        {
                            ConnectionId = _connectionId.ToString().ToLower(),
                            FileName = fileName,
                            File = newArry,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                s_logger.Error($"An error occurred while uploading the analyzed file. Ex: {ex}");
                throw;
            }
        }

        private string GetLocalFilePath()
        {
            return SecurityUtils.SafeCombinePath(Path.GetTempPath(), LOCAL_FOLDER_NAME, OpusTenantId.ToLower(), string.Format("{0}_data.txt", _connectionId.ToString().ToLower()));
        }

        private void CleanLocalFilesPath()
        {
            var filePaths = new List<string>
            {
                _localFilePath,
            };
            foreach (var path in filePaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch (Exception ex)
                {
                    s_logger.Warn($"Failed to delete local files on attempt [{Path.GetFileName(path).LogBase64()}]. Exception: {ex.Message}");
                }
            }
        }
        #endregion
    }
}
