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
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Explorer;
using Cloud.Sdk.Data.IE;
using Cloud.Sdk.IE;
using Cloud.Sdk.IE.Services;
using Microsoft.AspNetCore.Razor.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Util.SettingNameConstants;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions
{
    public static class RMDiscoveryOffice365IEJobExtension
    {

        private static readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365IEJobExtension));

        public static RMDiscoveryJobStatus ToOpusDiscoveryJobStatus(this Cloud.Sdk.Data.IE.JobStatus status)
        {
            return status switch
            {
                Cloud.Sdk.Data.IE.JobStatus.None => RMDiscoveryJobStatus.None,
                Cloud.Sdk.Data.IE.JobStatus.Pending => RMDiscoveryJobStatus.Pending,
                Cloud.Sdk.Data.IE.JobStatus.Running => RMDiscoveryJobStatus.Running,
                Cloud.Sdk.Data.IE.JobStatus.Skipped => RMDiscoveryJobStatus.Skipped,
                Cloud.Sdk.Data.IE.JobStatus.Failed => RMDiscoveryJobStatus.Failed,
                Cloud.Sdk.Data.IE.JobStatus.Finshed => RMDiscoveryJobStatus.Finished,
                Cloud.Sdk.Data.IE.JobStatus.FinishedWithException => RMDiscoveryJobStatus.Exception,
                _ => throw new NotSupportedException(status.ToString()),
            };
        }

        public static async Task<string> GetByODataUrlWithRetryAsync(this IEApiClient ieClient, string odataUrl, string office365TenantId, string queryName = "")
        {
            try
            {
                var retryer = RMRetryerBuilder.CreateBuilder().WithStopStrategy(new RMRetryStopAfterAttemptStrategy(5)).Build();
                return await retryer.RetryAsync(async () =>
                {
                    return await ieClient.GetByODataUrlAsync(odataUrl, office365TenantId);
                });
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while query [{queryName}], sql [{odataUrl}]. Error: {e}");
                throw;
            }
        }

        public static async Task<bool> ModifyTagWithRetryAsync(this IEApiClient ieClient, SourceFlag contentSource, string azureTenantId, string objectId, ModifyTagModel model)
        {
            var retryer = RMRetryerBuilder.CreateBuilder().WithStopStrategy(new RMRetryStopAfterAttemptStrategy(5)).Build();
            return await retryer.RetryAsync(async () =>
            {
                if (contentSource == SourceFlag.SharePoint)
                {
                    return await ieClient.SPDocumentTagService.ModifyAsync(azureTenantId, objectId, model);
                }
                else
                {
                    return await ieClient.SPOneDriveDocumentTagService.ModifyAsync(azureTenantId, objectId, model);
                }
            });
        }
    }
}
