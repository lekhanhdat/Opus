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
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.RecordsCloudAgentUpgrader
{
    public static class RecordsAgentDownloader
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int DEFAULT_MAX_RETRIES = 3;

        private const int DEFAULT_INITIAL_DELAY_MS = 2000;

        public static async Task<string> DownloadInstallerAsync(bool isMajorUpgrade, Guid agentId)
        {
            string url = isMajorUpgrade ? RecordsAgentUpgraderConst.MSI_AGENT_INSTALLER_URL
                                        : RecordsAgentUpgraderConst.MSP_AGENT_INSTALLER_URL;

            string extension = isMajorUpgrade ? "msi" : "msp";

            string fileName = string.Format(RecordsAgentUpgraderConst.GENERAL_FILE_NAME_FORMAT, agentId, extension);
            string folderPath = Path.Combine(Path.GetTempPath(), RecordsAgentUpgraderConst.INSTALL_FOLDER);
            EnsureDirectory(folderPath);
            string destinationPath = Path.Combine(folderPath, fileName);
            EnsureFileNotExists(destinationPath);

            for (int attempt = 1; attempt <= DEFAULT_MAX_RETRIES; attempt++)
            {
                try
                {
                    using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) })
                    using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                        {
                            if (response.StatusCode == HttpStatusCode.NotFound)
                                s_logger.Error("Installer not found (404). No retry.");
                            response.EnsureSuccessStatusCode(); 
                        }

                        if ((int)response.StatusCode >= 500)
                            throw new HttpRequestException($"Server error: {(int)response.StatusCode}");

                        using (var remoteStream = await response.Content.ReadAsStreamAsync())
                        using (var localStream = new FileStream(
                            destinationPath,
                            FileMode.Create,
                            FileAccess.Write,
                            FileShare.None,
                            bufferSize: 128 * 1024,
                            useAsync: true))
                        {
                            await remoteStream.CopyToAsync(localStream);
                        }

                        return destinationPath;
                    }
                }
                catch (Exception ex)
                {
                    bool isLastAttempt = (attempt == DEFAULT_MAX_RETRIES);

                    if (!ShouldRetry(ex))
                    {
                        s_logger.Error($"Non-retryable error: {ex.Message}");
                        throw;
                    }

                    if (isLastAttempt)
                    {
                        s_logger.Error("Max retry attempts exceeded.");
                        throw;
                    }

                    int wait = DEFAULT_INITIAL_DELAY_MS * attempt;
                    s_logger.Warn($"Attempt {attempt} failed. Retrying in {wait}ms...");
                    await Task.Delay(wait);
                }
            }

            throw new InvalidOperationException("Unexpected exit from retry loop.");
        }

        public static void EnsureDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                s_logger.Warn($"Failed to create directory: {path}, {ex.Message}");
            }
        }

        public static void EnsureFileNotExists(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                s_logger.Warn($"Failed to delete exist file: {path}, {ex.Message}");
            }
        }

        private static bool ShouldRetry(Exception ex)
        {
            if (ex is TaskCanceledException)
                return true;

            if (ex is HttpRequestException)
                return true;

            if (ex is IOException)
                return true;

            return false; // others → do not retry
        }
    }
}
