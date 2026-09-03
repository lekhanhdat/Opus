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
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.Replicator.Object.OperationResults
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProfileUsageConditionResult : ReplicatorOperationResult
    {
        public ProfileUsageConditionResult()
            : base(false, new ReplicatorOperationResultError(ReplicatorOperationResultErrorType.Unknown))
        {
            Summaries = new List<ReplicatorSummaryDto>();
        }

        [DataMember]
        public List<ReplicatorSummaryDto> Summaries { get; set; }

        /// <summary>
        /// get if the profile is used by others
        /// </summary>
        public bool IsUsedByOthers
        {
            get
            {
                return Summaries.Count > 0;
            }
        }

        public override string ToString()
        {
            if (!IsUsedByOthers)
            {
                return "Is not used by others";
            }
            var sb = new StringBuilder();
            sb.AppendLine("Is used by follows:");
            foreach (var summary in Summaries)
            {
                sb.AppendLine(string.Format("ID:[{0}], Name:[{1}], Type:[{2}]", summary.Id, summary.Name, summary.Type));
            }
            return sb.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorSummaryDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public ReplicatorResuableType Type { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorResuableType
    {
        [EnumMember]
        OnlineMapping,
        [EnumMember]
        OnlineMappingProfile,
        [EnumMember]
        OnlineReplicationSubProfile,
        [EnumMember]
        OnlineConflictSubProfile,

        [EnumMember]
        ExportMapping,
        [EnumMember]
        ExportMappingProfile,
        [EnumMember]
        ExportReplicationSubProfile,
        [EnumMember]
        ExportConflictSubProfile,

        [EnumMember]
        ImportMapping,
        [EnumMember]
        ImportMappingProfile,
        [EnumMember]
        ImportReplicationSubProfile,
        [EnumMember]
        ImportConflictSubProfile,

        [EnumMember]
        NetworkControl,
        [EnumMember]
        BytelevelSetting,
        [EnumMember]
        ReplicatorDatabase,
    }

    public static class ReplicatorResuableTypeConverter
    {
        public static ReplicatorResuableType Convert(ProfileType profileType)
        {
            switch(profileType)
            {
                case ProfileType.ReplicatorOnlineMapping:
                    return ReplicatorResuableType.OnlineMappingProfile;
                case ProfileType.ReplicatorOnlineReplication:
                    return ReplicatorResuableType.OnlineReplicationSubProfile;
                case ProfileType.ReplicatorOnlineConfliction:
                    return ReplicatorResuableType.OnlineConflictSubProfile;
                case ProfileType.ReplicatorExportMapping:
                    return ReplicatorResuableType.ExportMappingProfile;
                case ProfileType.ReplicatorExportReplication:
                    return ReplicatorResuableType.ExportReplicationSubProfile;
                case ProfileType.ReplicatorImportMapping:
                    return ReplicatorResuableType.ImportMappingProfile;
                case ProfileType.ReplicatorImportReplication:
                    return ReplicatorResuableType.ImportReplicationSubProfile;
                case ProfileType.ReplicatorImportConfliction:
                    return ReplicatorResuableType.ImportConflictSubProfile;
                case ProfileType.ReplicatorNetworkControl:
                    return ReplicatorResuableType.NetworkControl;
                case ProfileType.ReplicatorByteLevel:
                    return ReplicatorResuableType.BytelevelSetting;
                case ProfileType.ReplicatorConfigDB:
                    return ReplicatorResuableType.ReplicatorDatabase;
                default:
                    throw new ArgumentOutOfRangeException("profileType");
            }
        }

        public static ReplicatorResuableType Convert(ReplicatorPlanType planType)
        {
            switch(planType)
            {
                case ReplicatorPlanType.Replicate:
                    return ReplicatorResuableType.OnlineMapping;
                case ReplicatorPlanType.Export:
                    return ReplicatorResuableType.ExportMapping;
                case ReplicatorPlanType.Import:
                    return ReplicatorResuableType.ImportMapping;
                default:
                    throw new ArgumentOutOfRangeException("planType");
            }
        }
    }
}
