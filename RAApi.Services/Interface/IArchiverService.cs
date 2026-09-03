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
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using AvePoint.Api.Contract.Job;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using DocAveOnline.WebApi.Contracts;

namespace AvePoint.Api.Service.Interface
{
    [ServiceContract]
    public interface IArchiverService
    {
        Task<SearchResult> AdvanceSearchAsync(AdvanceSearchCondition searchCondition);
        Task<ArchiverRestoreResult> AOSPAdvanceSearchAsync(AdvanceSearchCondition searchCondition);
        Task<ExportedDataResult> GetExportedDataSASByJobInfoAsync(ExportJobInfo config);
        Task<bool> OpusStorageOptimizationEnabled();
        Task<int> GetTenantJobQueueCount();
        Task<List<Microsoft365Group>> GetTeamsAsync(Microsoft365User microsoft365User);
        Task<List<Microsoft365Group>> GetGroupsAsync(Microsoft365User microsoft365User);
        Task<Byte[]> GetPhotoAsync(Microsoft365User microsoft365User);
        Task<bool> InitTenantForMigrationJob(string logonUserId);
        Task<MigrationJobReportSASResult> GetMigrationJobReportSASAsync(string jobId);
        Task<bool> ClearLicenseUsageAsync();
        Task<List<string>> GetAllStubSearchResultAsync(Microsoft365User microsoft365User);
        Task<Stream> GetStubPreviewStreamAsync(PreviewDataParam param);
        Task<SearchResult> AdvanceFullTextAsync(AdvanceSearchCondition searchCondition);
        JMJobSummary GetJobSummary(string id);
        JMDetailsResult GetJobDetails(JMDetailsQuery queryModel);
        JMJobDetails GetJobSummaryStatistics(string id);
        List<JMJobInfo> GetOpusJobListByIds(List<string> ids);
    }
}
