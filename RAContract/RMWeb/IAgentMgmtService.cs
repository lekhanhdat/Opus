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
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IAgentMgmtService
    {
        Task<bool> SaveClientIdAsync(string clientId);
        AgentConfigurtion DownloadConfig(RMAgentDto agent);
        RMCertificateDto DownloadCert(Guid certId);
        bool CreateAgent(RMAgentDto dto);
        Guid? CreateAgentAndGetId(RMAgentDto dto);
        Task<Guid?> CreateReplicaAgentAndGetIdAsync(RMAgentDto dto);

        Task<string> UpdateAgentAsync(RMAgentDto dto);
        Task<bool> UpdateAgentResourceUsageAsync(RMAgentDto dto);
        Task<bool> UpdateAuthCodeAsync(Guid id, string authCode);

        /// <summary>
        /// update the certificate id column of agents
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="certificateId"></param>
        /// <returns></returns>
        Task<bool> UpdateCertificateIdAsync(List<Guid> ids, Guid certificateId);

        bool CheckAgentIsUnderGroup(Guid id);

        /// <summary>
        /// Get all of the non deleted agents. there is no license check for this method
        /// </summary>
        /// <returns></returns>
        Task<IList<RMAgentDto>> GetAllAsync();
        /// <summary>
        /// Get all of the available agents based on source type.
        /// This method will check license according to the source type.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        Task<IList<RMAgentDto>> GetAvailableAgentsBySourceTypeAsync(string tenantId, SourceType sourceType);

        Task<IList<RMAgentDto>> GetAvailableAgentsBySourceTypeAndConnectionGroupIdAsync(string tenantId, SourceType sourceType, Guid connecitonGroupId);

        Task<IList<RMAgentDto>> GetAllActiveAgentsAsync();

        Task<IList<RMAgentDto>> GetAllByFarmIdAsync(string farmId);

        RMAgentDto Get(Guid id, bool includeAuthCode = false);

        Task<bool> DeleteAsync(Guid id);

        Task<bool> UpdateInstallationCodeAsync(Guid id, string installationCode);

        Task<bool> DisableAsync(Guid id);

        Task<bool> EnableAsync(Guid id);

        /// <summary>
        /// if the time stamp of an active agent was not updated in seconds, will update the agent status to targetStatus
        /// </summary>
        System.Threading.Tasks.Task CheckAndUpdateStatusAsync(int seconds, ServiceStatus targetStatus);
        Task<bool> UpdateStatusAsync(Guid id, Hybrid.Contract.Object.ServiceStatus status);
        Task<int> UpdateAgentsStatusAsync(IEnumerable<Guid> ids, ServiceStatus status);

        Task<bool> UpdateAgentRelateFarmIdAsync(Guid id, string FarmId);

        Task<bool> CheckIfEnableFSUniqueIdSetting();

        Task<(List<RMAgentDto> dtos, RMAgentUpgradeResult)> UpgradeCloudAgentAsync(RMAgentUpgradeDto dto = null);
        bool HasAgentsInUpgradingProcess();
        bool TryCreateSkippedJobIfAgentUpgrading(JobType jobType, string jobRunByUser, out string jobId);
        Task<IList<RMAgentDto>> GetAvailableAgentsAsync(string tenantId);
        bool TryCreateSkippedJobIfRunningJobExceedLimit(JobType jobType, string jobRunByUser, out string jobId);
        Task<AgentQueryResult> QueryAgentsAsync(AgentQueryParams queryDto);
        Task<AgentQueryResult> FilterAgentsByDCAsync(AgentQueryParams queryDto);

        Task<IList<RMAgentDto>> GetAgentsByIdsAsync(List<Guid> agentIds);
        Task<List<Guid>> GetAgentIdsOnMainDCAsync();
    }
}
