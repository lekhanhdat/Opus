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
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Dedeplication;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Dashboard
{
    public interface IDashboardService
    {
        string RealRunDashboardJob(JobRunBy runB);

        bool SchduleRunDashboardJob(JobRunBy runBy);

        Task<bool> IsAdminAsync();

        Task<bool> IsSOAdminAsync();

        Task<bool> IsEndUserAsync();

        bool ExistsJobQueue();

        bool HasRunningJob();

        Task<int> GetEndUserPermissionAsync();

        Task<bool> SaveSOPriceConfigurationAsync(ArchiverPriceConfiguration priceConfiguration);

        Task<ArchiverPriceConfiguration> GetSOPriceConfigurationAsync();
        Task<TenantArchiverDataInfo> GetTenantArchivedDataInfo(Guid o365TenantId);
        Task<TenantArchiverDataInfo> GetTenantArchivedDataInfo(Guid o365TenantId, int type);
        Task<bool> IsRunSODashboardJobAsync();

        Task<RAReturnMessage> RunExportArchiverSiteInfoJobAsync(ArchiverExportReportDto reportDto);

        Task<RAReturnMessage> RunExportArchiverRetentionSimulateInfoJobAsync();
        Task<RAReturnMessage> RunArchiverDeduplicationReportJobAsync(DedeplicationExportReportDto reportDto);
        Task<string> RealRunExportArchiverSiteInfoJobAsync(string param);

        Task<string> RealRunExportArchiverDedupSiteInfoJobAsync(string param);
        Task<RAReturnMessage> RunExportArchiverGDriveInfoJobAsync(ArchiverExportReportDto reportDto);

        Task<SOSummaryTotalDataDetails> GetSOTotalDataInfos(string o365TenantId, string siteId);
        Task<SOSummaryTotalDataDetails> GetSOTotalDataInfosByTenant(string o365TenantId);
    }
}
