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
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Storage;
using AvePoint.Hybrid.Contract.DTOs;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;

namespace AvePoint.RA.Api.Web.Public.Utils
{
    public class GenerateAgentLogSasUrlUtil
    {
        private static readonly string s_connectionString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_PUBLIC_STORAGE_CONNECTION_STRING];
        private const string _blogContainerName = "agent-log-container";

        public static AgentLogSaSResponse GenerateAgentLogSasUrl(AgentLogSaSRequest request)
        {
            var containerClient = Util.MSAzure.StorageUtil.GetContainerClient(s_connectionString, _blogContainerName);
            containerClient.CreateIfNotExists();

            var categoryName = request.AgentLogCategory switch
            {
                AgentLogCategory.AgentService => "AgentService",
                AgentLogCategory.AgentBrowser => "AgentBrowser",
                AgentLogCategory.AgentJob => "AgentJob",
                _ => "Agent_Log"
            };
            var pathPrefix = $"{request.TenantId}/{request.AgentId}/{categoryName}".Replace("\\", "/");

            var expiresOn = DateTimeOffset.UtcNow.AddDays(7);
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _blogContainerName,
                Resource = "c",
                StartsOn = DateTimeOffset.UtcNow.AddSeconds(-60),
                ExpiresOn = expiresOn
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write | BlobSasPermissions.Create | BlobSasPermissions.List);
            var token = string.Empty;

            if (s_connectionString.StartsWith("DefaultEndpointsProtocol="))
            {
                var dictionary = Util.MSAzure.StorageUtil.ParseConnectionString(s_connectionString);
                var sharedKeyCredential = new StorageSharedKeyCredential(dictionary["AccountName"], dictionary["AccountKey"]);
                token = sasBuilder.ToSasQueryParameters(sharedKeyCredential).ToString();
            }
            else
            {
                var delegationKey = Util.MSAzure.StorageUtil.GetServiceClient(s_connectionString).GetUserDelegationKey(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddDays(7)).Value;
                var accountName = s_connectionString.Substring(0, s_connectionString.IndexOf("."));
                token = sasBuilder.ToSasQueryParameters(delegationKey, accountName).ToString();
            }

            var sasUrl = $"{containerClient.Uri}?{token}";

            return new AgentLogSaSResponse
            {
                SasUrl = sasUrl,
                ExprireOn = DateTime.Now,
                PathPrefix = pathPrefix
            };
        }
    }
}
