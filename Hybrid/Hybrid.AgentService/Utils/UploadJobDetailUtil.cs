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
using AvePoint.Hybrid.Contract.DTOs;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.Hybrid;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.Utils
{
    public static class UploadJobDetailUtil
    {
        private static readonly AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static HybridApiClient ApiClient { get { return HybridApiClient.Instance; } }

        public static async Task UploadJobDetail(AgentLogCategory type,string[] logFiles)
        {
            try
            {
                var tenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);
                var agentId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAgentId);

                var agentLogSaSResponse = ApiClient.GetAgentLogUploadSas(new Hybrid.Contract.DTOs.AgentLogSaSRequest
                {
                    AgentId = agentId,
                    TenantId = tenantId,
                    AgentLogCategory = type
                });

                var prefix = "";
                switch (type)
                {
                    case AgentLogCategory.AgentBrowser:
                        prefix = "Agent_Browser";
                        break;
                    case AgentLogCategory.AgentService:
                        prefix = "Agent_Service";
                        break;
                } ;

                var containerClient = new BlobContainerClient(new Uri(agentLogSaSResponse.SasUrl));

                var tasks = logFiles.Select(async filePath =>
                {
                    var surfix = "";
                    var fileNameLocal = Path.GetFileName(filePath);
                    if (fileNameLocal.IndexOf("High", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        surfix = "_High";
                    }
                    var fileName = File.GetCreationTime(filePath).ToString("yyyyMMddHHmmssfff");
                    var blobClient = containerClient.GetBlobClient($"{agentLogSaSResponse.PathPrefix}/{prefix}_{fileName}{surfix}.log");

                    logger.Info($"Uploading log '{filePath}' to blob '{blobClient.Name}'.");

                    var uploadOptions = new Azure.Storage.Blobs.Models.BlobUploadOptions
                    {
                        AccessTier = Azure.Storage.Blobs.Models.AccessTier.Cool
                    };
                    using (var stream = File.OpenRead(filePath))
                    {
                        await blobClient.UploadAsync(stream, options: uploadOptions).ConfigureAwait(false);
                    }

                });
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error($"Uploaded faild: {ex.Message}");
            }

        }
    }
}
