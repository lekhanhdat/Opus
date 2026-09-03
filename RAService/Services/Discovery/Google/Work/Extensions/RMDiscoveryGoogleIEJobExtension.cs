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
using System.Threading.Tasks;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using Cloud.Sdk.Data.IE;
using Cloud.Sdk.IE;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Extensions
{
    public static class RMDiscoveryGoogleIEJobExtension
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleIEJobExtension));
        public static RMDiscoveryJobStatus ToOpusDiscoveryJobStatus(this JobStatus status)
        {
            return status switch
            {
                JobStatus.None => RMDiscoveryJobStatus.None,
                JobStatus.Pending => RMDiscoveryJobStatus.Pending,
                JobStatus.Running => RMDiscoveryJobStatus.Running,
                JobStatus.Skipped => RMDiscoveryJobStatus.Skipped,
                JobStatus.Failed => RMDiscoveryJobStatus.Failed,
                JobStatus.Finshed => RMDiscoveryJobStatus.Finished,
                JobStatus.FinishedWithException => RMDiscoveryJobStatus.Exception,
                _ => throw new NotSupportedException(status.ToString()),
            };
        }

        public static async Task<string> GetByODataUrlWithRetryAsync(this IEApiClient ieClient, string odataUrl, string googleOrganizationId, string queryName = "")
        {
            try
            {
                var retryer = RMRetryerBuilder.CreateBuilder().WithStopStrategy(new RMRetryStopAfterAttemptStrategy(5)).Build();
                return await retryer.RetryAsync(async () =>
                {
                    return await ieClient.GetByODataUrlAsync(odataUrl, googleTenantId: googleOrganizationId);
                });
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query [{queryName}], sql [{odataUrl}]. Error: {e}");
                throw;
            }
        }

        public static async Task<bool> ModifyTagWithRetryAsync(this IEApiClient ieClient, string azureTenantId, string objectId, ModifyTagModel model)
        {
            var retryer = RMRetryerBuilder.CreateBuilder().WithStopStrategy(new RMRetryStopAfterAttemptStrategy(5)).Build();
            return await retryer.RetryAsync(async () =>
            {
                return await ieClient.GoogleDriveDocumentTagService.ModifyAsync(azureTenantId, objectId, model);
            });
        }
    }
}
