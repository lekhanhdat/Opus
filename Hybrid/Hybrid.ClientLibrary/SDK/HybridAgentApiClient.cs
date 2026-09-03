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
using AvePoint.Hybrid.ClientCore;
using AvePoint.Hybrid.ClientCore.Clients;
using AvePoint.Hybrid.ClientCore.Logging;
using AvePoint.Hybrid.ClientLibrary;
using AvePoint.Hybrid.ClientLibrary.SDK.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.ClientLibrary.SDK
{
    public class HybridAgentApiClient : ApiClientBase
    {
        public ITreeBrowserService treeBrowserService => CreateServiceProxy<ITreeBrowserService>();

        public IJobMonitorService JobMonitorService => CreateServiceProxy<IJobMonitorService>();

        public IRecordsJobService RecordsJobService => CreateServiceProxy<IRecordsJobService>();

        public IAgentMgmtService AgentMgmtService => CreateServiceProxy<IAgentMgmtService>();

        public ISharePointBrowserService SharePointBrowserService => CreateServiceProxy<ISharePointBrowserService>();

        public IExchangeBrowserService ExchangeOnlineBrowserSErvice => CreateServiceProxy<IExchangeBrowserService>();

        public ISharePointOnPremBrowserService SharePointOnPremBrowserService => CreateServiceProxy<ISharePointOnPremBrowserService>();

        public ISharePointOnPremJobService SharePointJobService => CreateServiceProxy<ISharePointOnPremJobService>();

        public ISharePointOnPremLocalNodeService SharePointOnPremLocalNodeService => CreateServiceProxy<ISharePointOnPremLocalNodeService>();

        public IBoxBrowserService BoxBrowserService => CreateServiceProxy<IBoxBrowserService>();

        public IStorageDeviceService StorageDeviceService => CreateServiceProxy<IStorageDeviceService>();

        public IMediaDatasService MediaDatasService => CreateServiceProxy<IMediaDatasService>();

        public IFSMasterIndexService FSMasterIndexService => CreateServiceProxy<IFSMasterIndexService>();

        public IFSArchiverManagementService FSArchiverManagementService => CreateServiceProxy<IFSArchiverManagementService>();
        public IFSIndexSubInfoService FSIndexSubInfoService => CreateServiceProxy<IFSIndexSubInfoService>();
        public ITelemetryService TelemetryService => CreateServiceProxy<ITelemetryService>();
        public ISettingProfileService SettingProfileService => CreateServiceProxy<ISettingProfileService>();

        public ITeamsBrowserService TeamsBrowserService => CreateServiceProxy<ITeamsBrowserService>();

        public IFSDiscoveryService FSDiscoveryService => CreateServiceProxy<IFSDiscoveryService>();
        public IAgentLogCollectorService AgentLogCollectorService => CreateServiceProxy<IAgentLogCollectorService>();
        public IDataIngestionService DataIngestionService => CreateServiceProxy<IDataIngestionService>();

        public IRMFSConnManagementService RMFSConnManagementService => CreateServiceProxy<IRMFSConnManagementService>();

        public IFileSystemService FileSystemService => CreateServiceProxy<IFileSystemService>();

        private readonly MemoryCache cache;

        internal string ApiUrl { get; set; }
        internal string TenantId { get; set; }
        
        internal string HybridAgentAuth { get; set; }


        protected override string BaseUrl => ApiUrl;
        protected int ExpiredMinutes = 70;

        public HybridAgentApiClient(ILogger<HybridAgentApiClient> logger,
            ISdkLogger sdkLogger,
            ApiMemoryCache cache,
            ICloudSdkHttpClientFactory cloudSdkHttpClientFactory,
            IOptions<CloudSdkCoreOptions> coreOptions,
            IOptions<HybridAgentApiOption> option,
            ICloudSdkIdentityServerTokenService tokenService)
            : base(coreOptions.Value, cloudSdkHttpClientFactory, option.Value, tokenService)
        {
            this.logger = logger;
            this._logger = sdkLogger;
            this.cache = cache.Cache;
            this.IdentityServerScope = coreOptions.Value.IdentityServerScope ?? HBContractConstants.HybridAgentScope;
        }

        public override async Task AssembleRequestHeaders(HttpRequestMessage request)
        {
            SetIdentityServerToken(request, AuthenticationHeaderScheme.Bearer, await GetIdentityServerToken(TenantId,HybridAgentId,HybridAgentAuth));
        }

    }
}
