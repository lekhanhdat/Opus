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
using AvePoint.Hybrid.AgentService.Utils;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.LogCollector
{
    public class AgentServiceLogCollector : IAgentLogCollector
    {
        private readonly AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string targetDirectory;
        private readonly ICollectorCheckPointStore checkpointStore;
        public string Name => "AgentService";

        public AgentServiceLogCollector(ICollectorCheckPointStore checkpointStore)
        {
            this.checkpointStore = checkpointStore;
            this.targetDirectory = Path.Combine(AveEnv.AgentLogFolder, MultiTenantFileLocker.CommonFolder);
        }

        public async Task CollectAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!Directory.Exists(this.targetDirectory))
                {
                    logger.Error($"Do exist the folder directory {this.targetDirectory}");
                    return;
                }

                var files = GetCandidateFiles();
                logger.Debug("Prepared {0} agent service log files for processing.", files.Count);
                await UploadJobDetailUtil.UploadJobDetail(Contract.DTOs.AgentLogCategory.AgentService, files.Select(file => file.FullName).ToArray());
                var latestTimestamp = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero);
                logger.Info($"Update last check point time to {latestTimestamp}");
                checkpointStore.UpdateCheckPoint(this.Name, latestTimestamp);
            }
            catch (OperationCanceledException)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Failed to enumerate agent service log files. Details: {0}.", ex.ToString());
            }

            return;
        }

        private IReadOnlyCollection<FileInfo> GetCandidateFiles()
        {
            var lastCheckpoint = this.checkpointStore.GetLastCheckPoint(this.Name) ?? DateTimeOffset.MinValue;
            logger.Info($"The last checkpoint of agent services {lastCheckpoint}");
            return Directory
                .EnumerateFiles(this.targetDirectory, "*.log", SearchOption.AllDirectories)
                .Select(path => new FileInfo(path))
                .Where(file => file.Exists)
                .Where(file => new DateTimeOffset(file.LastWriteTimeUtc, TimeSpan.Zero) > lastCheckpoint)
                .OrderBy(file => file.LastWriteTimeUtc)
                .ToList();
        }

    }
}
