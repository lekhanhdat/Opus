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
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Object
{
    [DataContract]
    public class RMAgentDto
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public SourceType SourceType { get; set; }
        [DataMember]
        public string ClientId { get; set; }
        [DataMember]
        public string Version { get; set; } = "N/A";
        [DataMember]
        public Guid CertificateId { get; set; }
        [DataMember]
        public string CertificateThumbprint { get; set; }
        [DataMember]
        public CertificateStatus? CertificateStatus { get; set; }
        [DataMember]
        public string InstallationCode { get; set; }
        [DataMember]
        public string AuthCode { get; set; }
        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public ServiceStatus Status { get; set; }
        [DataMember]
        public ServiceErrors Errors { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string TenantId { set; get; }
        [DataMember]
        public int JobCounts { set; get; }
        [DataMember]
        public long TimeStamp { set; get; }
        [DataMember]
        public long CPUHZ { set; get; }
        [DataMember]
        public long CPUUsage { set; get; }
        [DataMember]
        public long TotalMemory { set; get; }
        [DataMember]
        public long AvailableMemeory { set; get; }
        [DataMember]
        public string OSName { set; get; }
        [DataMember]
        public int OSVersionNumber { set; get; }
        [DataMember]
        public string FarmId { get; set; }
        [DataMember]
        public bool IsSupportUpgrade { get; set; }
        [DataMember]
        public bool CollectLog { get; set; }
        [DataMember]
        public string DCInternalName { get; set; }
        [DataMember]
        public string DCDisplayName { get; set; }
    }

    [DataContract]
    public class AgentResultDto
    {
        [DataMember]
        public string AgentId { get; set; }
        [DataMember]
        public RMAgentCreateResult AgentCreateResult { get; set; }
    }
    public enum RMAgentCreateResult
    {
        Succeed,
        NoClientId,
        NoCertificate,
        Failed,
        SameNameExist,
        UpdateCommonDataFailed,
    }

    [DataContract]
    public class RMAgentUpgradeDto
    {
        [DataMember]
        public List<Guid> AgentsId { get; set; }
        [DataMember]
        public RMAgentUpgradeResult Result { get; set; }
        [DataMember]
        public RMAgentUpgradeMode Mode { get; set; }
    }

    public enum RMAgentUpgradeResult
    {
        /// <summary>Upgrade completed successfully.</summary>
        Success,
        /// <summary>General failure during upgrade.</summary>
        Failed,
        /// <summary>No newer version is available.</summary>
        NoLatestVersion,
        /// <summary>No active agent is running.</summary>
        NoActiveAgent,
        /// <summary>An agent is busy with another job.</summary>
        HasRunningJob
    }

    public enum RMAgentUpgradeMode
    {
        AllAgent,
        Specific
    }

    public class AgentQueryResult
    {
        public List<RMAgentDto> Agents { get; set; }
        public int TotalCount { get; set; }
        public bool HasMismatchedAgent { get; set; }
        public bool HasMinorVersionMismatchedAgent { get; set; }
    }

    [DataContract]
    public class AgentQueryParams
    {
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public string SortBy { get; set; }
        [DataMember]
        public bool IsAscending { get; set; }
        [DataMember]
        public List<Guid> AddAgentList { get; set; }
        [DataMember]
        public string DataCenterName { get; set; }
        [DataMember]
        public string MainDCName { get; set; }
    }
}
