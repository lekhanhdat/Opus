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




namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
    using AvePoint.GCommon.Contract.Tree.Object;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditControllerChart : BaseChart
    {
        [DataMember]
        public AuditControllerOption Option { get; set; }
        [DataMember]
        public SPTreeNodeDto Node { get; set; }
        [DataMember]
        public string ProfileId { get; set; }
        [DataMember]
        public ScheduleDto Schedule { get; set; }
    }

    //需要chartFactory执行的动作
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuditControllerOption
    {
        [EnumMember]
        RunApplyRule,
        [EnumMember]
        RunRetrieveData,
        [EnumMember]
        GetAuditActions
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ApplyRuleType
    {
        [EnumMember]
        Override,
        [EnumMember]
        Append
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuditAction
    {
        // 一下的与SharePoint一一对应
        [EnumMember]
        CheckOut = 1,
        [EnumMember]
        CheckIn = 2,
        [EnumMember]
        View = 4,
        [EnumMember]
        Delete = 8,
        [EnumMember]
        Update = 16,
        [EnumMember]
        ProfileChange = 32,
        [EnumMember]
        ChildDelete = 64,
        [EnumMember]
        SchemaChange = 128,
        [EnumMember]
        SecurityChange = 256,
        [EnumMember]
        Undelete = 512,
        [EnumMember]
        Workflow = 1024,
        [EnumMember]
        Copy = 2048,
        [EnumMember]
        Move = 4096,
        [EnumMember]
        Search = 8192,
        //一下三个SharePoint中没有，属于自定义的值
        [EnumMember]
        TrickleDown = 16384,
        [EnumMember]
        SiteDeletion = 32768,
        [EnumMember]
        SiteCreation = 65536,
        //[EnumMember]
        //SiteCollectionDeleteion = 65536,

        [EnumMember]
        CheckOutCheckIn = 131072,
        [EnumMember]
        CopyMove = 262144,
        [EnumMember]
        DeleteUndelete = 524288,
        [EnumMember]
        ProfileSchemaChange = 1048576,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuditEventType
    {
        [EnumMember]
        All = -1,
        [EnumMember]
        None = 0,
        [EnumMember]
        CheckOut = 1,
        [EnumMember]
        CheckIn = 2,
        [EnumMember]
        View = 4,
        [EnumMember]
        Delete = 8,
        [EnumMember]
        Update = 16,
        [EnumMember]
        ProfileChange = 1048576,
        [EnumMember]
        ChildDelete = 134217728,
        [EnumMember]
        SchemaChange = 2097152,
        [EnumMember]
        Undelete = 32,
        //[EnumMember]
        //Workflow = 64,
        [EnumMember]
        Copy = 262144,
        [EnumMember]
        Move = 524288,
        [EnumMember]
        AuditMaskChange = 8388608,
        [EnumMember]
        Search = 128,
        [EnumMember]
        ChildMove = 4194304,
        [EnumMember]
        FileFragmentWrite = 268435456,
        [EnumMember]
        CreateGroup = 256,
        [EnumMember]
        DeleteGroup = 512,
        [EnumMember]
        AddGroupMember = 1024,
        [EnumMember]
        DeleteGroupMember = 2048,
        [EnumMember]
        CreatePermissionLevel = 4096,
        [EnumMember]
        DeletePermissionLevel = 8192,
        [EnumMember]
        ChangePermissionLevel = 16384,
        [EnumMember]
        BreakPermissionLevelInheritance = 32768,
        [EnumMember]
        ChangePermission = 65536,
        [EnumMember]
        InheritPermissionSetting = 131072,


        [EnumMember]
        BreakPermissionInheritance = 16777216,
        [EnumMember]
        EventsDeleted = 33554432,
        [EnumMember]
        Custom = 67108864,
        // Copy 262144  Move 524288 ChildMove 4194304 AuditMaskChange 8388608 EventsDeleted 33554432 Custom 67108864 ChildDelete 134217728 FileFragmentWrite = 268435456
        [EnumMember]
        Others = 516685824,//248250432 - 64(workflow) + 268435456(FileFragmentWrite)
         [EnumMember]
        AppPermissionGrant = 536870912,

        [EnumMember]
        AppPermissionRemoval = 1073741824,

        [EnumMember]
        Download = 64,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuditItemType
    {
        [EnumMember]
        All = -1,
        [EnumMember]
        None = 0,
        [EnumMember]
        Document = 1,
        [EnumMember]
        ListItem = 3,
        [EnumMember]
        List = 4,
        [EnumMember]
        Folder = 5,
        [EnumMember]
        Site = 6,
        [EnumMember]
        SiteCollection = 7,
    }

    /// <summary>
    /// sharepoint中EventType
    /// </summary>
    public enum SPAuditEventType
    {
        None = 0,
        CheckOut = 1,
        CheckIn = 2,
        View = 3,
        Delete = 4,
        Update = 5,
        Undelete = 10,
        //Workflow = 11,
        Search = 15,
        Copy = 12,
        Move = 13,
        AuditMaskChange = 14,
        ChildMove = 16,
        FileFragmentWrite = 17,
        CreateGroup = 30,
        DeleteGroup = 31,
        AddGroupMember = 32,
        DeleteGroupMember = 33,
        CreatePermissionLevel = 34,
        DeletePermissionLevel = 35,
        ChangePermissionLevel = 36,
        BreakPermissionLevelInheritance = 37,
        ChangePermission = 38,
        InheritPermissionSetting = 39,
        BreakPermissionInheritance = 40,
        EventsDeleted = 50,
        Custom = 100,
        ProfileChange = 6,
        ChildDelete = 7,
        SchemaChange = 8,
        AppPermissionGrant = -2,
        AppPermissionRemoval = -3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuditControllerPlanType
    {
        [EnumMember]
        ApplyAndRetrieve = 0,
        [EnumMember]
        Apply = 1,
        [EnumMember]
        Retrieve = 2,
    }
}