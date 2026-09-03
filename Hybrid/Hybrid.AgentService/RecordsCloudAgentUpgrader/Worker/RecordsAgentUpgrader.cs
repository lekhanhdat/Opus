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
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility;
using AvePoint.Hybrid.Utility.Configuration;
using AvePoint.RA.Common.Hybrid;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.RecordsCloudAgentUpgrader
{
    public class RecordsAgentUpgrader
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int DEFAULT_MAX_RETRIES = 3;

        private const int DEFAULT_DELAY_SECOND = 5;

        private const int DEFAULT_TIMEOUT_MINUTES = 30;

        private RecordsAgentUpraderInfo _upgraderInfo;

        private string _targetVersionFallback;

        private AgentAccount _agentAccount;

        private AgentInfo _agentInfo;

        private ServiceStatus _initialStatus;

        private RecordsBatScriptBuilder _batScriptBuilder;

        public RecordsAgentUpgrader(AgentInfo agentInfo, string targetVersion)
        {
            _agentInfo = agentInfo;
            _targetVersionFallback = targetVersion;
            _agentAccount = AgentAccountUtil.Get();
            _batScriptBuilder = new RecordsBatScriptBuilder()
                .EnableRequireAdminPermission()
                .EnableParamValidation()
                .EnableKillWorker()
                .EnableRollback()
                .EnableReapplyServiceAccount()
                .EnableAutoStartService();
        }

        public RecordsAgentUpgrader(AgentInfo agentInfo, string targetVersion, bool isDebugMode)
        {
            if (!isDebugMode)
                throw new InvalidOperationException("This constructor is only for debug mode.");
            _agentInfo = agentInfo;
            _targetVersionFallback = targetVersion;
            AveEnv.AgentVersion = "15.7.0.178";
            _batScriptBuilder = new RecordsBatScriptBuilder()
                .EnableAutoStartService()
                .EnableRequireAdminPermission()
                .EnableParamValidation()
                .EnableReapplyServiceAccount()
                .DisableRollback()
                .DisableKillWorker();
        }

        public async Task ProcessUpgradeCloudAgentAsync()
        {
            try
            {
                int finalExitCode = -1;
                bool success = false;
                await PrepareRecordsUpgraderAgentInfoAsync();

                for (int attempt = 1; attempt <= DEFAULT_MAX_RETRIES; attempt++)
                {
                    try
                    {
                        s_logger.Info($"Starting upgrade attempt {attempt} of {DEFAULT_MAX_RETRIES}");
                        var arguments = _batScriptBuilder.GenerateArguments(
                            _upgraderInfo.ServiceName,
                            _upgraderInfo.ServiceUser,
                            _upgraderInfo.ServicePass,
                            _upgraderInfo.InstallerPath,
                            _upgraderInfo.LogFilePath);

                        var psi = new ProcessStartInfo
                        {
                            FileName = _upgraderInfo.BatFilePath,
                            Arguments = arguments,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        };

                        using (var process = new Process { StartInfo = psi })
                        {
                            process.Start();

                            bool exited = process.WaitForExit((int)TimeSpan.FromMinutes(DEFAULT_TIMEOUT_MINUTES).TotalMilliseconds);
                            if (!exited)
                            {
                                s_logger.Error($"Upgrade still running after {DEFAULT_TIMEOUT_MINUTES} minutes. Leaving process timeout.");
                                finalExitCode = (int)InteralExitCode.PROCESS_TIMEOUT;
                                try
                                {
                                    process.Kill();
                                }
                                catch (Exception ex)
                                {
                                    s_logger.Error("Failed to kill upgrade process.", ex);
                                }
                            }
                            else
                            {
                                finalExitCode = process.ExitCode;
                            }

                            s_logger.Info($"Attempt {attempt}: BAT exit code: {finalExitCode}");
                            DetectExitCodeInterpretation(finalExitCode);

                            if (finalExitCode == (int)InteralExitCode.SUCCESS
                                || finalExitCode == (int)InteralExitCode.SUCCESS_REQUIRE_REBOOT
                                || finalExitCode == (int)InteralExitCode.SUCCESS_REBOOT_INITIATED)
                            {
                                s_logger.Info($"Upgrade successful on attempt {attempt}. Breaking retry loop.");
                                success = true;
                                break;
                            }
                            else if (finalExitCode == (int)InteralExitCode.REQUIRE_ADMINISTRATOR
                                || finalExitCode == (int)InteralExitCode.REAPPLY_SERVICE_ACCOUNT_FAILURE)
                            {
                                s_logger.Error($"Upgrade cannot proceed due to critical error on attempt {attempt}. Exiting retry loop.");
                                break;
                            }
                            s_logger.Warn($"Upgrade failed on attempt {attempt} with exit code {finalExitCode}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        s_logger.Error($"Attempt {attempt}: Failed to execute upgrade BAT file.", ex);
                        finalExitCode = (int)InteralExitCode.ABSOLUTE_FAILED;
                    }

                    if (!success && attempt < DEFAULT_MAX_RETRIES)
                    {
                        s_logger.Info($"Waiting for {DEFAULT_DELAY_SECOND} seconds before attempt {attempt + 1}...");
                        await Task.Delay(DEFAULT_DELAY_SECOND * 1000);
                    }
                }

                if (success) return;
                s_logger.Error($"Upgrade failed after all {DEFAULT_MAX_RETRIES} attempts. Last exit code: {finalExitCode}");
            }
            finally
            {
                s_logger.Info("Finished upgrade process. Cleaning up temporary files...");
                CleanupTempFiles(_upgraderInfo.BatFilePath, _upgraderInfo.InstallerPath);
                s_logger.Info("Cleaned up temporary files.");
                ResetAgentAfterUpgradingProcess();
            }
        }

        public async Task PrepareRecordsUpgraderAgentInfoAsync()
        {
            try
            {
                s_logger.Info("Preparing Cloud Agent upgrade info…");
                MarkAgentForUnderUpgrading();
                RecordsAgentUpgraderConst.ReadConfigFile();
                _upgraderInfo = new RecordsAgentUpraderInfo();
                _upgraderInfo.AgentId = _agentInfo.AgentId;
                _upgraderInfo.ServiceName = RecordsAgentUpgraderConst.CLOUD_AGENT_SERVICE_NAME;
                _upgraderInfo.CurrentVersion = AveEnv.AgentVersion;
                _upgraderInfo.TargetVersion = await GetLatestAgentInstallerVersionAsync();
                _upgraderInfo.IsMajorUpgrade = CheckIsMajorUpgrade(_upgraderInfo.CurrentVersion, _upgraderInfo.TargetVersion);
                _upgraderInfo.BatFilePath = GenerateBatScriptFile(_upgraderInfo.AgentId);
                _upgraderInfo.InstallerPath = await RecordsAgentDownloader.DownloadInstallerAsync(_upgraderInfo.IsMajorUpgrade, _upgraderInfo.AgentId);
                _upgraderInfo.LogFilePath = string.Format(RecordsAgentUpgraderConst.INSTALL_LOG_FILE_PATH, _agentInfo.AgentId);
                _upgraderInfo.ServiceUser = !string.IsNullOrWhiteSpace(_agentAccount?.Domain) ? $"{_agentAccount?.Domain}\\{_agentAccount?.UserName}" : null;
                _upgraderInfo.ServicePass = _agentAccount?.Password;
                LogPreparedInfo();
                s_logger.Info("Cloud Agent upgrade info prepared; agent status set to Upgrading.");
            }
            catch (Exception ex)
            {
                s_logger.Error("Failed to prepare cloud agent upgrader info.", ex);
                throw;
            }
        }

        private async Task<string> GetLatestAgentInstallerVersionAsync()
        {
            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) })
                using (var response = await client.GetAsync(RecordsAgentUpgraderConst.AGENT_INSTALLER_INFO_URL))
                {
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    if (json.IsNullOrEmpty()) return string.Empty;
                    s_logger.Info($"The latest cloud agent installer json: {json}");
                    var version = JObject.Parse(json)["Version"]?.ToString();
                    return version.IsNotNullOrEmpty() ? version : _targetVersionFallback;
                }
            }
            catch (Exception ex)
            {
                s_logger.Error($"Failed to get latest cloud agent installer version. So using the fallback version [{_targetVersionFallback}] to process.", ex);
                return _targetVersionFallback;
            }
        }

        private void LogPreparedInfo()
        {
            s_logger.Info("--- Upgrade Info Summary ---");
            s_logger.Info($"Current installed version: {_upgraderInfo.CurrentVersion}");
            s_logger.Info($"Latest available version: {_upgraderInfo.TargetVersion}");
            s_logger.Info($"Is major upgrade: {_upgraderInfo.IsMajorUpgrade}");
            s_logger.Info($"Service name: {_upgraderInfo.ServiceName}");
            s_logger.Info($"Is password empty: {string.IsNullOrWhiteSpace(_upgraderInfo.ServicePass)}");
            s_logger.Info($"BAT file path: {_upgraderInfo.BatFilePath}");
            s_logger.Info($"Installer path: {_upgraderInfo.InstallerPath}");
            s_logger.Info($"Log file path: {_upgraderInfo.LogFilePath}");
            s_logger.Info("----------------------------");
        }

        private bool CheckIsMajorUpgrade(string currentVersion, string latestVersion)
        {
            var cur = new Version(currentVersion);
            var latest = new Version(latestVersion);
            return latest.Major > cur.Major || latest.Minor > cur.Minor;
        }

        private string GenerateBatScriptFile(Guid agentId)
        {
            try
            {
                string outputFileName = string.Format(RecordsAgentUpgraderConst.GENERAL_FILE_NAME_FORMAT, agentId, "bat");
                string folderPath = Path.Combine(Path.GetTempPath(), RecordsAgentUpgraderConst.INSTALL_FOLDER);
                RecordsAgentDownloader.EnsureDirectory(folderPath);
                string outputPath = Path.Combine(folderPath, outputFileName);
                RecordsAgentDownloader.EnsureFileNotExists(outputPath);
                _batScriptBuilder.SaveToFile(outputPath);
                s_logger.Info($"Generated BAT script upgrade file at: {outputPath}");
                return outputPath;
            }
            catch (Exception ex)
            {
                s_logger.Error("Generate BAT file failed.", ex);
                throw;
            }
        }

        private void CleanupTempFiles(params string[] paths)
        {
            if(paths == null)
            {
                s_logger.Info("No temporary files to clean up.");
                return;
            }
            s_logger.Info("Starting temporary file cleanup.");
            foreach (var path in paths)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(path)) continue;
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        s_logger.Info($"Successfully deleted temp file: {path}");
                    }
                    else if (Directory.Exists(path))
                    {
                        Directory.Delete(path, recursive: true);
                        s_logger.Info($"Successfully deleted temp directory (recursive): {path}");
                    }
                    else
                    {
                        s_logger.Debug($"Cleanup skipped. Path does not exist: {path}");
                    }

                }
                catch (IOException ioEx)
                {
                    s_logger.Warn($"Failed to delete {path}. File might be in use. Details: {ioEx.Message}");
                }
                catch (Exception ex)
                {
                    s_logger.Error($"FATAL cleanup failure for path: {path}", ex);
                }
            }
            s_logger.Info("Temporary file cleanup finished.");
        }

        private static void DetectExitCodeInterpretation(int exitCode)
        {
            if (exitCode == (int)InteralExitCode.SUCCESS)
            {
                s_logger.Info("Interpretation: SUCCESS (0)");
                s_logger.Info("-> The process completed successfully.");
                return;
            }
            else if (exitCode == (int)InteralExitCode.GENERAL_FAILURE)
            {
                s_logger.Error("Interpretation: GENERAL ERROR (1)");
                s_logger.Error("-> A general or unspecified failure occurred.");
                return;
            }
            else if (exitCode == (int)InteralExitCode.INVALID_PARAMETER)
            {
                s_logger.Error("Interpretation: FILE NOT FOUND or INVALID ARGUMENTS (2)");
                s_logger.Error("-> A required file is missing or parameters are invalid.");
                return;
            }
            else if (exitCode == (int)InteralExitCode.INVALID_INSTALLER_EXTENTION)
            {
                s_logger.Error("Interpretation: INVALID INSTALLER EXTENTION (3)");
                s_logger.Error("-> The installer file has an invalid extension.");
                return;
            }
            else if (exitCode == (int)InteralExitCode.REAPPLY_SERVICE_ACCOUNT_FAILURE)
            {
                s_logger.Error("Interpretation: REAPPLY SERVICE ACCOUNT FAILURE (4)");
                s_logger.Error("-> Failed to reapply the service account after installation.");
                return;
            }
            else if (exitCode == (int)InteralExitCode.REQUIRE_ADMINISTRATOR)
            {
                s_logger.Error("Interpretation: ADMINISTRATOR IS REQUIRED (999)");
                s_logger.Error("-> A required administrator to process the BAT file.");
                return;
            }
            else if (exitCode > 2 && exitCode < 128)
            {
                s_logger.Warn($"Interpretation: APPLICATION-SPECIFIC ERROR ({exitCode})");
                s_logger.Warn("-> Non-zero code defined by the application.");
                return;
            }
            else
            {
                s_logger.Error($"Interpretation: UNKNOWN ERROR ({exitCode})");
                s_logger.Error("-> Possibly application-specific or runtime script error.");
            }
        }

        private void ResetAgentAfterUpgradingProcess()
        {
            try
            {
                _agentInfo.Status = _initialStatus;
                HybridApiClient.Instance.UpdateAgentStatus(_agentInfo);
            }
            catch (Exception ex)
            {
                s_logger.Error($"Failed to reset agent to initial status [{_initialStatus}].", ex);
            }
            finally
            {
                CommonConfiguration.SetInUpgradingProcess(false);
                s_logger.Info($"Successfully reset agent to initial status [{_initialStatus}].");
            }
        }

        private void MarkAgentForUnderUpgrading()
        {
            try
            {
                _initialStatus = _agentInfo.Status;
                CommonConfiguration.SetInUpgradingProcess(true);
                s_logger.Info($"Marked agent under upgrading process with initital status [{_initialStatus}].");
            }
            catch (Exception ex)
            {
                s_logger.Error($"Failed to update agent status to upgrading.", ex);
            }
        }
    }
}
