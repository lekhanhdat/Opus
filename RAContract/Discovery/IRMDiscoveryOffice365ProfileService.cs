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
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.ExportDiscoveryProfile;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery
{
    public interface IRMDiscoveryOffice365ProfileService
    {
        Task<List<RMDiscoveryProfileDataInfo>> GetInactiveProfileInfoesAsync(Guid o365TenantId);

        Task<RAReturnMessage> AddInactiveProfileInfoAsync(RMDiscoveryProfileDataInfo dataInfo);

        Task<RAReturnMessage> UpdateInactiveProfileInfoAsync(RMDiscoveryProfileDataInfo dataInfo);

        Task<RAReturnMessage> DeleteInactiveProfileInfoAsync(RMDiscoveryProfileDataInfo dataInfo);

        Task<List<RMDiscoveryProfileDataInfo>> GetRotProfileInfoesAsync(Guid o365TenantId);

        Task<RAReturnMessage> AddRotProfileInfoAsync(RMDiscoveryProfileDataInfo dataInfo);

        Task<RAReturnMessage> UpdateRotProfileInfoAsync(RMDiscoveryProfileDataInfo dataInfo);

        Task<RAReturnMessage> DeleteRotProfileInfoAsync(RMDiscoveryProfileDataInfo dataInfo);

        System.Threading.Tasks.Task GenerateExportProfileAsync(ExportDiscoveryProfileParam exportParam, RMDiscoveryProfileDataInfo profile);
        Task<RMDiscoveryProfileDataInfo> GetProfileInfoByIdAsync(Guid o365TenantId, Guid profileId, string discoveryType);

        bool SendProfileJob(JobRunBy runBy, RMDiscoveryProfileJobDefinition definition);

        string RealRunProfileJob(JobQueueDto queueDto);
        RAReturnMessage RunExportProfileDiscoveryDataAnalysisForOffice365Job(DiscoveryO365DataAnalysis o365DataAnalysis);
        Task<string> RealRunExportProfileDiscoveryDataAnalysisForOffice365Job(JobRunBy jobRunBy, string jobRunByUser, string o365DataAnalysis);
    }
}
