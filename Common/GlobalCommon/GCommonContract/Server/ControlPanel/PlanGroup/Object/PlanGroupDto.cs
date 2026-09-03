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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanGroupDto : AbstractGroup, IComparable<PlanGroupDto>
    {
        [DataMember]
        public List<PlanOrderInfos> PlanOrderInfos { get; set; }
        [DataMember]
        public PlanGroupSetting GroupSetting { get; set; }
        [DataMember]
        public int ErrorMessageType { get; set; }
        [DataMember]
        public bool IsChecked { get; set; }
        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }
        [DataMember]
        public List<PlanDtoForPlanGroup> Plans { get; set; }
        [DataMember]
        public List<PlanDtoForPlanGroup> DMPlans { get; set; }
        [DataMember]
        public List<PlanDtoForPlanGroup> AllPlans { get; set; }
        [DataMember]
        public List<PlanDtoForPlanGroup> AddedPlans { get; set; }
        [DataMember]
        public List<PlanDtoForPlanGroup> RemovedPlans { get; set; }
        [DataMember]
        public int FinishOrder { get; set; }
        [DataMember]
        public string PlanGroupExecutionId { get; set; }

        public int CompareTo(PlanGroupDto other)
        {
            return this.ObjectInfo.UpdateTime.CompareTo(other.ObjectInfo.UpdateTime);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanGroupSetting
    {
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public NotificationDto Notification { get; set; }
        [DataMember]
        public ConflictResolution ContainerLevel { get; set; }
        [DataMember]
        public bool IsRecursion { get; set; }
        [DataMember]
        public ConflictResolution ContentLevel { get; set; }
        [DataMember]
        public PlanGroupTypes GroupType { get; set; }
        [DataMember]
        public int ThreadNumbers { get; set; }
        [DataMember]
        public bool IsTerminate { get; set; }
        [DataMember]
        public int ParallelRunOrder { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanDtoForPlanGroup : PlanDto, IComparable<PlanDtoForPlanGroup>
    {
        [DataMember]
        public List<PlanGroupDto> PlanGroups { get; set; }
        [DataMember]
        public List<string> PlanGroupIds { get; set; }
        [DataMember]
        public string SourceFarmName { get; set; }
        [DataMember]
        public string DestFarmName { get; set; }
        [DataMember]
        public int Order { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            PlanDtoForPlanGroup dto = obj as PlanDtoForPlanGroup;
            if (dto == null)
            {
                return false;
            }
            return this.Id == dto.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public int CompareTo(PlanDtoForPlanGroup other)
        {
            return this.ObjectInfo.UpdateTime.CompareTo(other.ObjectInfo.UpdateTime);
        }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanGroupParaDto
    {
        [DataMember]
        public string PlanId { get; set; }

        /// <summary>
        /// plan的顺序
        /// </summary>
        [DataMember]
        public int PlanOrder { get; set; }

        [DataMember]
        public PlanCategory Category { get; set; }

        /// <summary>
        ///plan 对应的plangroupid
        /// </summary>
        [DataMember]
        public string PlanGroupId { get; set; }

        [DataMember]
        public string PlanGroupName { get; set; }

        [DataMember]
        public string PlanGroupExecutionId { get; set; }

        /// <summary>
        /// plan对应的jobid
        /// </summary>
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public PGScheduleType PGScheduleType { get; set; }

        /// <summary>
        /// Plan group并行Run时的最大job数
        /// </summary>
        [DataMember]
        public int Count { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanGroupJobResult
    {
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public PlanGroupJobState JobState { get; set; }
        [DataMember]
        public JobLicenseStatus LicenseStatus { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobLicenseStatus
    {
        [EnumMember]
        Allow = 0,
        [EnumMember]
        Deny = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PlanGroupJobState
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        Successful = 0,
        [EnumMember]
        Skiped = 1,
        [EnumMember]
        Failed = 2
    }



    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanOrderInfos : IComparable<PlanOrderInfos>
    {
        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public string PlanName { get; set; }

        //[DataMember]
        //public string Description { get; set; }

        /// <summary>
        /// plan的顺序
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public PlanCategory planCategory { get; set; }

        public int CompareTo(PlanOrderInfos other)
        {
            return this.Order.CompareTo(other.Order);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanGroupDtoForOtherModule
    {
        [DataMember]
        public string PlanGroupId { get; set; }
        [DataMember]
        public string PlanGroupName { get; set; }
        [DataMember]
        public List<string> PlanGroupIds { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string PlanName { get; set; }
        [DataMember]
        public PlanCategory Category { get; set; }

        public override string ToString()
        {
            return this.PlanGroupName;
        }
    }

    public enum PlanGroupResult
    {
        [EnumMember]
        Failed,
        [EnumMember]
        Successful
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConflictResolution : int
    {
        [EnumMember]
        Skip = 0,
        [EnumMember]
        Merge = 1,
        [EnumMember]
        Replace = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PlanGroupTypes : int
    {
        [EnumMember]
        Parallel = 0,
        [EnumMember]
        Sequential = 1
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum Option : int
    {
        [EnumMember]
        Full = 0,
        [EnumMember]
        Incremental = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot(ElementName = "PlanOrderSettings")]
    public class PlanOrderSettings
    {
        [DataMember]
        [XmlArray("PlanOrderInfos")]
        public List<PlanOrderInfos> PlanOrderInfos { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PlanOfGroupState : int
    {
        [EnumMember]
        Enable = 0,
        [EnumMember]
        Disabled = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PGScheduleType : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Full = 1,
        [EnumMember]
        Incremental = 2,
        [EnumMember]
        Differential = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum Modules : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Migration = 1,
        [EnumMember]
        ContentManager = 2,
        [EnumMember]
        DeploymentManager = 3,
        [EnumMember]
        CentralAdmin = 4,
        [EnumMember]
        Replicator = 5,
        [EnumMember]
        PlatformRecovery = 6,
        [EnumMember]
        Item = 7,

    }
}
