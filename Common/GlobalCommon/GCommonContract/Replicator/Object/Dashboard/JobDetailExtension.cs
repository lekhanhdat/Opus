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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.Adonis.Replicator.Contract.Settings;
using AvePoint.Adonis.Replicator.Contract;

namespace AvePoint.GCommon.Contract.Replicator.Object.Dashboard
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobDetailExtension
    {
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public string PlanName { get; set; }

        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public ReplicatorRunLevel JobMode { get; set; }

        [DataMember]
        public Boolean IsException { get; set; }

        [DataMember]
        public string TriggerEvent { get; set; }

        [DataMember]
        public int EventReceiverType { get; set; }

        [DataMember]
        public bool IsRealTime { get; set; }

        [DataMember]
        public int SuccessfulBackupMetadata { get; set; }

        [DataMember]
        public int FailedBackupMetadata { get; set; }

        [DataMember]
        public int SuccessfulRestoreMetadata { get; set; }

        [DataMember]
        public int FailedRestoreMetadata { get; set; }

        [DataMember]
        public string DeletionDetails { get; set; }

        [DataMember]
        public string MappingId { get; set; }

        /// <summary>
        /// true代表是replication details的记录，需要在replication details中显示出来；
        /// false代表不是replication details的记录，不需要在replication details中显示出来；
        /// </summary>
        [DataMember]
        public bool IsReplicationDetailsRecord { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum Metadata
    {
        /// <summary>
        /// Basic Information
        /// </summary>
        [EnumMember]
        BasicInformation = 1,//sitecollection,web,list

        /// <summary>
        /// Audience
        /// </summary>
        [EnumMember]
        Audience = 2,//sitecollection

        /// <summary>
        /// Metadata Service
        /// </summary>
        [EnumMember]
        MetadataService = 4,//sitecollection

        /// <summary>
        /// Settings
        /// </summary>
        [EnumMember]
        Settings = 8,//sitecollection,web,list

        /// <summary>
        /// Features
        /// </summary>
        [EnumMember]
        Features = 16,//sitecollection,web,

        /// <summary>
        /// Search Scopes and Search Keywords
        /// </summary>
        [EnumMember]
        SearchScopesandKeywords = 32,//sitecollection

        /// <summary>
        /// Users
        /// </summary>
        [EnumMember]
        Users = 64,//sitecollection,web,list

        /// <summary>
        /// Groups
        /// </summary>
        [EnumMember]
        Groups = 128,//sitecollection,web,list

        /// <summary>
        /// User Profiles
        /// </summary>
        [EnumMember]
        UserProfile = 256,//sitecollection,

        /// <summary>
        /// User Profiles Properties
        /// </summary>
        [EnumMember]
        UserProfileProperties = 512,//sitecollection,

        /// <summary>
        /// EventReceivers
        /// </summary>
        [EnumMember]
        EventReceivers = 1024,//web,list,listitem,document

        /// <summary>
        /// Columns
        /// </summary>
        [EnumMember]
        Columns = 2048,//web,list

        /// <summary>
        /// Content types
        /// </summary>
        [EnumMember]
        ContentTypes = 4096,//web,list

        /// <summary>
        /// Navigation
        /// </summary>
        [EnumMember]
        Navigation = 8192,//web

        /// <summary>
        /// Permission Level
        /// </summary>
        [EnumMember]
        PermissionLevel = 16384,//web

        /// <summary>
        /// Permission
        /// </summary>
        [EnumMember]
        Permission = 32768,//web,list,folder,listitem,document

        /// <summary>
        /// Workflow Definition
        /// </summary>
        [EnumMember]
        WorkflowDefinition = 65536,//web,list

        /// <summary>
        ///  content type workflow definition
        /// </summary>
        [EnumMember]
        ContentTypeWorkflowDefinition = 131072,//web,list

        /// <summary>
        /// Alert
        /// </summary>
        [EnumMember]
        Alert = 262144,//list,listitem,document

        /// <summary>
        /// Metadata
        /// </summary>
        [EnumMember]
        Metadata = 524288,//folder,listitem,document

        /// <summary>
        /// Social Tag
        /// </summary>
        [EnumMember]
        SocialTag = 1048576,//web,folder,listitem,document

        /// <summary>
        /// Social Comment
        /// </summary>
        [EnumMember]
        SocialComment = 2097152,//web,folder,listitem,document

        /// <summary>
        /// Workflow Instance
        /// </summary>
        [EnumMember]
        WorkflowInstance = 4194304,//web,folder,listitem,document

        /// <summary>
        /// Content
        /// </summary>
        [EnumMember]
        Content = 8388608,//attachment

        /// <summary>
        /// WebParts
        /// </summary>
        [EnumMember]
        WebParts = 16777216,//document
    }

    /// <summary>
    /// Use JobDetail Remark6
    /// 0-->insert;
    /// 1-->update status
    /// 2-->update last report
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MonitorOperationControl : int
    {
        [EnumMember]
        Insert = 0,
        [EnumMember]
        UpdateState,
        [EnumMember]
        UpdateCommentStatus,//just update detail status and comment;
        [EnumMember]
        UpdateFinalReport,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicationDetailStatus : int
    {
        [EnumMember]
        None = -1,

        [EnumMember]
        Finished = 0,

        [EnumMember]
        Failed = 1,

        [EnumMember]
        Skipped = 2,

        //[EnumMember]
        //Exception = 3,

        [EnumMember]
        Restoring = 4,

        [EnumMember]
        Transferring = 5,

        [EnumMember]
        BackingUp = 6,

        [EnumMember]
        Waiting = 7,

    }
}
