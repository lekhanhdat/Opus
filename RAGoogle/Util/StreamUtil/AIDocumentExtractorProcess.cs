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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.RAProcess;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AIExtractionModel;
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using Util.AI.Text.Extractor;

namespace RAGoogle.Util.StreamUtil
{
    public class AIDocumentExtractorProcess
    {
        private readonly string _msgDir;
        private readonly string _resDir;
        private readonly bool _isDev;
        private readonly string _workerExePath; // using in dev: run exe
        private readonly string _workerDllPath; // using in PROD: dotnet <dll>
        private readonly int _timeoutMs = 1000 * 60 * 5;
        private readonly RALogger _logger;
        public AIDocumentExtractorProcess()
        {
            _workerExePath = SecurityUtils.SanitizeCommandArgs(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "AIExtractWorker.exe"));
            _workerDllPath = SecurityUtils.SanitizeCommandArgs(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "AIExtractWorker.dll"));
            _msgDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AIExtract", "messages");
            _resDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AIExtract", "results");
            _isDev = RMGlobalConfiguration.EnvSetting.IsDevEnvironment;
            Directory.CreateDirectory(_msgDir);
            Directory.CreateDirectory(_resDir);
            _logger = RALogger.GetInstance(typeof(AIDocumentExtractorProcess));
        }

        public async Task<(bool Success, string Content, string Error)> ExtractFromFileAsync(
            string itemId,
            string sourceFilePath,
            string extension,
            int maxCharsCount)
        {
            using (new PerformanceScope("MachineLearning.TryGetFileContent.ExtractContent", $"worker extract one item content, itemId:[{itemId}]", true))
            {
                // 1) Create file message/result
                var msgPath = Path.Combine(_msgDir, $"{itemId}_{Guid.NewGuid()}.message.json");
                var resPath = Path.Combine(_resDir, $"{itemId}_{Guid.NewGuid()}.result.json");

                var msg = new AIExtractMessage
                {
                    ItemId = itemId,
                    FilePath = sourceFilePath,
                    Extension = extension,
                    MaxCharsCount = maxCharsCount,
                    ResultPath = resPath
                };

                await File.WriteAllTextAsync(msgPath, JsonHelper.Serialize(msg));

                // 2) running new process
                var (proc, startedOk, startErr) = StartWorkerProcess(msgPath);
                if (!startedOk)
                {
                    SafeDelete(msgPath);
                    return (false, string.Empty, $"Cannot start worker: {startErr}");
                }

                // 3) waiting sucess process -> close process 
                bool exitOk = proc.WaitForExit(_timeoutMs);
                try
                {
                    proc.Close();
                }
                catch
                {
                    /* ignore */
                }

                if (!exitOk)
                {
                    TryKill(proc);
                    SafeDelete(msgPath);
                    return (false, string.Empty, "Worker timed out.");
                }

                // 4) read result file
                if (!File.Exists(resPath))
                {
                    SafeDelete(msgPath);
                    return (false, string.Empty, "Result file not found.");
                }

                try
                {
                    var json = await File.ReadAllTextAsync(resPath);
                    var result = JsonHelper.Deserialize<AIExtractResult>(json) ?? new AIExtractResult { Succeed = false, ErrorMessage = "Invalid result JSON." };

                    return (result.Succeed, result.Content ?? string.Empty, result.ErrorMessage ?? string.Empty);
                }
                finally
                {
                    SafeDelete(resPath);
                    SafeDelete(msgPath);
                }
            }
        }

        private (Process Proc, bool Ok, string Err) StartWorkerProcess(string messagePath)
        {
            try
            {
                if (_isDev)
                {
                    if (!File.Exists(_workerExePath))
                        return (null, false, $"Worker exe not found: {_workerExePath}");
                }
                else
                {
                    if (!File.Exists(_workerDllPath))
                        return (null, false, $"Worker dll not found: {_workerDllPath}");
                }
                if (!File.Exists(messagePath))
                    return (null, false, $"Message file not found: {messagePath}");
                var psi = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                };
                if (_isDev)
                {
                    psi.FileName = _workerExePath;
                    psi.Arguments = $"\"{messagePath}\"";
                }
                else
                {
                    psi.FileName = "dotnet";
                    psi.Arguments = $"\"{_workerDllPath}\" \"{messagePath}\""; 
                }
                var proc = new Process { StartInfo = psi };
                proc.Start();
                _logger.Info($"Worker started. PID={proc.Id}, File={psi.FileName}, Args={psi.Arguments}, WD={psi.WorkingDirectory}");
                if (proc == null)
                {
                    return (null!, false, "Process.Start returned null.");
                }
                return (proc, true, string.Empty);
            }
            catch (Exception ex)
            {
                return (null!, false, ex.Message);
            }
        }

        private static void TryKill(Process p)
        {
            try
            {
                if (!p.HasExited) p.Kill(entireProcessTree: true);
            }
            catch { /* ignore */ }
        }

        private static void SafeDelete(string path)
        {
            try 
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch { /* ignore */ }
        }
        public static class JsonHelper
        {
            private static readonly JsonSerializerOptions _opts = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            public static string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, _opts);
            public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _opts);
        }

    }
}
