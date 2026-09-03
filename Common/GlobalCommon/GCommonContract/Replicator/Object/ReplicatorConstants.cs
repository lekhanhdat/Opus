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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.Replicator.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SpeedUnit
    {
        [EnumMember]
        KBPerSecond,

        [EnumMember]
        MBPerSecond
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NetworkSpeed
    {
        public NetworkSpeed() { Value = DefaultValue; }

        public const int DefaultValue = -1;

        [DataMember]
        public int Value { get; set; }

        [DataMember]
        public SpeedUnit Unit { get; set; }
    }

    [Flags]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AveDayOfWeek
    {
        [EnumMember]
        Sunday = 1,

        [EnumMember]
        Monday = 2,

        [EnumMember]
        Tuesday = 4,

        [EnumMember]
        Wednesday = 8,

        [EnumMember]
        Thursday = 16,

        [EnumMember]
        Friday = 32,

        [EnumMember]
        Saturday = 64
    }

    [Flags]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicationEvent : long
    {
        [EnumMember]
        None = 0L,

        [EnumMember]
        FoldersOrItemsCreated = 1 << 0,

        [EnumMember]
        FoldersOrItemsUpdated = 1 << 1,

        [EnumMember]
        FoldersOrItemsDeleted = 1 << 2,

        [EnumMember]
        FoldersOrItemsMoved = 1 << 3,

        [EnumMember]
        CheckedIn = 1 << 4,

        //[EnumMember]
        //CheckedOut = 1 << 5,

        [EnumMember]
        ListAdded = 1 << 6,

        //[EnumMember]
        //ListDeleted = 1 << 7,

        [EnumMember]
        CheckedOutDiscarded = 1 << 8,

        [EnumMember]
        ListColumnChanged = 1 << 9,

        //[EnumMember]
        //SiteCollectionDelete = 1 << 10,

        [EnumMember]
        SiteMove = 1 << 11,

        //[EnumMember]
        //SiteDelete = 1 << 12,

        //[EnumMember]
        //VersionDelete = 1 << 13,

        //[EnumMember]
        //WorkflowStart = 1 << 14,

        //[EnumMember]
        //WorkflowComplete = 1 << 15,

        [EnumMember]
        UsersInGroupOrGroupsCreate = 1 << 16,

        [EnumMember]
        UsersInGroupOrGroupsUpdate = 1 << 17,

        //[EnumMember]
        //UsersInGroupOrGroupsDelete = 1 << 18,

        [EnumMember]
        PermissionOrPermissionLevelCreate = 1 << 19,

        [EnumMember]
        PermissionOrPermissionLevelUpdate = 1 << 20,

        [EnumMember]
        PermissionOrPermissionLevelDelete = 1 << 21,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DatabaseAuthentication
    {
        [EnumMember]
        [XmlEnum("0")]
        Windows = 0,

        [EnumMember]
        [XmlEnum("1")]
        SQL = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TimeUnit
    {
        [EnumMember]
        [XmlEnum("0")]
        Days = 0,

        [EnumMember]
        [XmlEnum("1")]
        Months = 1,

        [EnumMember]
        [XmlEnum("2")]
        Years = 2,

        [EnumMember]
        [XmlEnum("3")]
        Minutes = 3,

        [EnumMember]
        [XmlEnum("4")]
        Hours = 4,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DataSizeUnit
    {
        [EnumMember]
        [XmlEnum("0")]
        KB = 0,

        [EnumMember]
        [XmlEnum("1")]
        MB = 1,

        [EnumMember]
        [XmlEnum("2")]
        GB = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConflictAction
    {
        [EnumMember]
        [XmlEnum("0")]
        Skip = 0,

        [EnumMember]
        [XmlEnum("1")]
        Overwrite = 1,

        [EnumMember]
        [XmlEnum("2")]
        ManualConflictResolution = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConflictWinnerRuleType
    {
        [EnumMember]
        [XmlEnum("0")]
        SourceOrTargetAlwaysWins = 0,

        [EnumMember]
        [XmlEnum("1")]
        ItemWithLatestModificationWins = 1,

        [EnumMember]
        [XmlEnum("2")]
        ItemWithHighestVersionWins = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConflictWinnerRuleValue
    {
        [EnumMember]
        [XmlEnum("0")]
        SourceWins = 0,

        [EnumMember]
        [XmlEnum("1")]
        TargetWins = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConflictWinnerRule
    {
        [DataMember]
        [XmlAttribute("ruleOrder")]
        public int RuleOrder { get; set; }

        [DataMember]
        [XmlAttribute("ruleType")]
        public ConflictWinnerRuleType RuleType { get; set; }

        [DataMember]
        [XmlAttribute("ruleValue")]
        public ConflictWinnerRuleValue RuleValue { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorDirection
    {
        [EnumMember]
        UnSpecified = 0,

        [EnumMember]
        OneWay = 1,

        [EnumMember]
        TwoWay = 2,

        [EnumMember]
        OneWayPull = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmConfigDBInfo
    {
        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public string DatabaseName { get; set; }

        [DataMember]
        public string ProfileId { get; set; }

        [DataMember]
        public FarmDto Farm { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmByteLevelInfo
    {
        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public ProfileDto ByteLevelSetting { get; set; }
    }

    public enum ReplicatorPlanType
    {
        Deleted = -1,
        Unknown = 0,
        Import = 1,
        Export = 2,
        Replicate = 3,
        PlanImport = 4,
    }
    public enum Enable
    {
        False=0,
        True=1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorMappingType
    {
        UnSpecified = 0,
        Online = 1,
        Import = 2,
        Export = 3,
    }

    /// <summary>
    /// 冲突发生后，提醒信息发送的相关信息
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfictNotificationInfo
    {
        [DataMember]
        public string NotificationReportName { get; set; }

        [DataMember]
        public ConflictNotificationType NotificationType { get; set; }

        [DataMember]
        public string NotificationRecipient { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConflictNotificationType
    {
        [EnumMember]
        UnSpecified = 0,

        [EnumMember]
        ItemCreator,

        [EnumMember]
        LastModifierOfTheLosingVersion,

        [EnumMember]
        SiteCollectionAdministrator,

        [EnumMember]
        EmailAddress,

        [EnumMember]
        UserColumn,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorJobType
    {
        [EnumMember]
        UnSpecified = 0,

        [EnumMember]
        BackupBeforeJob = 1,

        [EnumMember]
        RollbackJob = 2,

        [EnumMember]
        ReplicatorJob = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorPlanActive
    {
        [EnumMember]
        Deleted = -1,

        [EnumMember]
        Active = 0,

        [EnumMember]
        Draft = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SODataOption
    {
        [EnumMember]
        RealContent,

        [EnumMember]
        StubOnly,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreThreadOption
    {
        [EnumMember]
        MultipleThreads,

        [EnumMember]
        SingleThread,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConflictResolution
    {
        [EnumMember]
        None = 0, //用来标示未升级的数据
        [EnumMember]
        Overwrite,
        [EnumMember]
        NotOverwrite,
        [EnumMember]
        IgnoreDifference,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SkipHiddenList
    {
        [EnumMember]
        None,
        [EnumMember]
        Skip,
        [EnumMember]
        NotSkip,
    }
}
