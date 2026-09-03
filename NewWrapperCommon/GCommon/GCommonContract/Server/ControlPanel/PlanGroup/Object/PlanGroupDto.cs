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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.PlatformRecovery.Object;
using AvePoint.Common;
using System.Reflection;

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
        public PlanGroupParaDto PlanGroupParaDto { get; set; }
        [DataMember]
        public int FinishOrder { get; set; }
        [DataMember]
        public bool IsLastRunComplete { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string PlanGroupExecutionId { get; set; }

        [DataMember]
        public string UserName { get; set; }

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

        /// <summary>
        /// 何种状态的job会导致后续job Skipped
        /// </summary>
        [DataMember]
        public List<JobState> status { get; set; }
        [DataMember]
        public bool OnlyObeyPlanGroupSchedule { get; set; }
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
            return base.GetHashCode();
        }
        //public int CompareTo(PlanDtoForPlanGroup other)
        //{
        //    return this.ObjectInfo.UpdateTime.CompareTo(other.ObjectInfo.UpdateTime);
        //}
        public int CompareTo(PlanDtoForPlanGroup other)
        {
            return this.UpdateTime.CompareTo(other.UpdateTime);
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
        public RunMode RunMode { get; set; }

        #region Plan Group Run Now 时应用的Default Setting
        [DataMember]
        public PRBackupLevel PRBackupLevel { get; set; }
        [DataMember]
        public BackupType GranularBackupType { get; set; }
        [DataMember]
        public bool IncludeItemsReport { get; set; }
        [DataMember]
        public ModuleScheduleExtension ReplicatorExtension { get; set; }
         [DataMember]
        public PlanGroupScheduleExtension PlanGroupExtension { get; set; }
        #endregion
         [DataMember]
        public PGScheduleType PGScheduleType { get; set; }
        [DataMember]
        public CommonScheduleSetting MigrationScheduleSetting { get; set; }

        [DataMember]
        public CommonScheduleSetting PRScheduleSetting { get; set; }

        [DataMember]
        public ScheduleDto Schedule { get; set; }

        [DataMember]
        public string UserId { get; set; }

        /// <summary>
        /// 各模块创建job时可以直接赋值该userName
        /// </summary>
        [DataMember]
        public string UserName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanGroupScheduleExtension
    {
        [DataMember]
        public CommonScheduleSetting SPMigrationScheduleSetting { get; set; }

        [DataMember]
        public CommonScheduleSetting SPHighSpeedMigrationScheduleSetting { get; set; }

        [DataMember]
        public CommonScheduleSetting NonSPMigrationScheduleSetting { get; set; }

        [DataMember]
        public PRObjectBase PRScheduleAdvanceOption { get; set; }

        [DataMember]
        public PRObjectBase PRSNBackupInfoDto { get; set; }
    }



    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType("GetKnownTypes")]
    public class CommonScheduleSetting 
    {
        public static IEnumerable<Type> GetKnownTypes()
        {
            return AveKnownTypeContext.GetKnonwTypes(MethodBase.GetCurrentMethod().DeclaringType);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRScheduleSetting
    {
        [DataMember]
        public PRBackupType PRBackupType { get; set; }

        [DataMember]
        public PRBackupLevel PRBackupLevel { get; set; }

        [DataMember]
        public PRObjectBase PRScheduleAdvanceOption { get; set; }

        [DataMember]
        public PRObjectBase PRSNBackupInfoDto { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RunMode
    {
        [EnumMember]
        ManualRun = 0,
        [EnumMember]
        ScheduleRun = 1
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
        [DataMember]
        public bool HasPerm 
        {
            get { return hasPerm; }
            set { hasPerm = value; }
        }
        private bool hasPerm = true;
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
        public PlanCategory Category { get; set; }
        [DataMember]
        public List<string> AuditNames { get; set; }

        [DataMember]
        public bool OnlyObeyPlanGroupSchedule { get; set; }

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
    public enum RunResult
    {
        [EnumMember]
        Successful = 0 ,
        [EnumMember]
        Failed = 1,
        [EnumMember]
        NoPlans = 2,
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

    public enum Modules : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ContentManager = 1,
        [EnumMember]
        DeploymentManager = 2,
        [EnumMember]
        Migration = 3,
        [EnumMember]
        Replicator = 4,
        [EnumMember]
        Item = 5,
        [EnumMember]
        PlatformRecovery = 6,
        [EnumMember]
        PlatformRecoveryForSMSP = 7,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PGScheduleValidationResultType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        StartTimeNotValid = 1,
        [EnumMember]
        EndTimeNotValid = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PGScheduleValidationResult
    {
        [DataMember]
        public PGScheduleValidationResultType Type { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GroupPlanValidateResult 
    {
        [DataMember]
        public ResultTypes Type { get; set; }

        /// <summary>
        /// 国际化后的词条，用于提示用户验证失败原因
        /// </summary>
        [DataMember]
        public string Message { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ResultTypes
    {
        [EnumMember]
        Alow =1,

        [EnumMember]
        Deny = 2,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PermissionType
    {
        [EnumMember]
        All=0,
        [EnumMember]
        Edit = 1,
        [EnumMember]
        View = 2,
    }
}
