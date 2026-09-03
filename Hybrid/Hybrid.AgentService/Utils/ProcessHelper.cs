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
using AvePoint.Hybrid.AgentService.RecordsCloudAgentUpgrader;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.CommonUtil;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.Utils
{
    public class ProcessHelper
    {
        static AvePoint.GCommon.AveLogger logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void StopProcess(string process)
        {
            try
            {
                var procs = Process.GetProcesses();

                for (int i = 0; i < procs.Length; i++)
                {
                    if (process.Contains(procs[i].ProcessName))
                    {
                        logger.Info("Stop process : " + process + ", process id : " + procs[i].Id);
                        procs[i].Kill();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("stop process error : ", e);
            }
        }

        public static void ExecuteAgentStopLogic()
        {
            try
            {
                StopProcess(Constants.RecordsBrowserExe);
                NotifyAgentStop();
                Thread.Sleep(1 * 1000);
                logger.Info("Sleep 1s and wait for the thread stopped safely.");
            }
            catch (Exception e)
            {
                logger.Error("Execute agent stop logic error : ", e);
            }
        }

        private static void NotifyAgentStop()
        {
            try
            {
                string agentId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAgentId);
                string tenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);

                logger.Warn("Keep update thread need to be quit, agent id : " + agentId);
                AgentManagementArgs args = new AgentManagementArgs();
                args.Type = MessageType.Onstop;
                args.TenantId = tenantId;
                args.AgentId = agentId;
                var result = System.Threading.Tasks.Task.Run(() => RASignalRProxy.GetManagerProxy().SendToManagerAsync<SAgentManagement>(new SAgentManagement() { MethodArgs = args }));
                logger.Warn("Update agent to stop status, agent id : " + agentId);
            }
            catch (Exception e)
            {
                logger.Error("Update agent to stop status", e);
            }
        }

        public static bool RestartAgentServices(params string[] serviceNames)
        {
            try
            {
                var requestedServices = serviceNames
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (requestedServices.Length == 0)
                {
                    logger.Warn("No current service name is provided for restart.");
                    return false;
                }

                var currentServiceName = requestedServices.FirstOrDefault(IsCurrentService);
                if (string.IsNullOrWhiteSpace(currentServiceName))
                {
                    logger.Warn("Failed to resolve current service name for restart.");
                    return false;
                }

                Task.Run(() => StopProcess(Constants.RecordsBrowserExe));

                return RequestBatScriptRestart(currentServiceName);
            }
            catch (Exception e)
            {
                logger.Error("restart service error : ", e);
                return false;
            }
        }

        private static bool RequestBatScriptRestart(string serviceName)
        {
            try
            {
                var scriptPath = CreateRestartScriptFile();
                var logPath = CreateRestartLogFilePath();
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c \"\"" + scriptPath + "\" \"" + EscapeBatchArgument(serviceName) + "\" \"" + EscapeBatchArgument(logPath) + "\"\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(scriptPath)
                });

                if (process == null)
                {
                    logger.Error("Failed to start bat script for service restart.");
                    return false;
                }

                logger.Info("Triggered bat script restart for current service: " + serviceName + ", script path: " + scriptPath + ", log path: " + logPath);
                return true;
            }
            catch (Exception e)
            {
                logger.Error("request bat script restart error : ", e);
                return false;
            }
        }

        private static string CreateRestartScriptFile()
        {
            var folder = Path.Combine(Path.GetTempPath(), "AvePoint", "ServiceRestart");
            Directory.CreateDirectory(folder);

            var scriptPath = Path.Combine(folder, "RestartAgentService_" + Guid.NewGuid().ToString("N") + ".cmd");
            File.WriteAllText(scriptPath, BuildRestartScript(), Encoding.ASCII);
            return scriptPath;
        }

        private static string CreateRestartLogFilePath()
        {
            var folder = Path.Combine(Path.GetTempPath(), "AvePoint", "ServiceRestart");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "RestartAgentService_" + Guid.NewGuid().ToString("N") + ".log");
        }

        private static string BuildRestartScript()
        {
            var sb = new StringBuilder();
            sb.Append(RestartBatScriptSection.HEADER);
            sb.Append(RestartBatScriptSection.PARAMETERS_DEFINITION);
            sb.Append(RestartBatScriptSection.VALIDATION);
            sb.Append(RestartBatScriptSection.RESTART_SERVICE);
            sb.Append(RestartBatScriptSection.FOOTER);

            return sb.ToString();
        }

        private static string EscapeBatchArgument(string value)
        {
            return (value ?? string.Empty).Replace("\"", "\"\"");
        }

        private static bool IsCurrentService(string name)
        {
            return !string.IsNullOrWhiteSpace(name)
                && name.Equals(RecordsAgentUpgraderConst.CLOUD_AGENT_SERVICE_NAME, StringComparison.OrdinalIgnoreCase);
        }
    }
}
