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



namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListInformationPolicySettingsOperation : CAOperation //data for list
    {
        [DataMember]
        public List<ContentTypePolicyInfo> ContentTypePolicies { get; set; }

        [DataMember]
        public List<SiteCollectionPolicy> AllSiteCollectionPolicies { get; set; }

        //List work flows, type:all
        [DataMember]
        public Dictionary<string, string> ListWorkFlows { get; set; }

        //for set content type
        [DataMember]
        public ContentTypePolicyInfo SetContentType { get; set; }

        //Farm level Retention is available
        [DataMember]
        public bool RetentionAvailable { get; set; }

        //Farm level Auditing is available
        [DataMember]
        public bool AuditingAvailable { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentTypePolicyInfo //data for content type
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string PolicyStatement { get; set; }
        [DataMember]
        public bool IsInherited { get; set; }
        [DataMember]
        public bool IsReadOnly { get; set; }
        [DataMember]
        public SiteCollectionPolicyStatus PolicyStatus { get; set; }
        [DataMember]
        public SiteCollectionPolicy CurrentSiteCollectionPolicy { get; set; }
        [DataMember]
        public bool EnableRetention { get; set; }
        [DataMember]
        public List<Stage> Stages { get; set; }
        [DataMember]
        public bool EnableAudit { get; set; }
        [DataMember]
        public List<string> SelectedAudits { get; set; }
        [DataMember]
        public List<string> AllEventProperties { get; set; }
        //content type work flow, type:content type
        [DataMember]
        public Dictionary<string, string> ContentTypeWorkFlows { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Stage
    {
        //Event
        [DataMember]
        public string EventPropertyName { get; set; }
        [DataMember]
        public int EventTime { get; set; }
        [DataMember]
        public DateTimeUnit EventTimeUnit { get; set; }

        //Action
        [DataMember]
        public string ActionType { get; set; }
        [DataMember]
        public ActionValue ActionId { get; set; } 
        [DataMember]
        public string WorkflowActionValue { get; set; }//work flow name

        //Recurrence
        [DataMember]
        public bool IsRecurrence { get; set; }
        [DataMember]
        public int RecurrenceTime { get; set; }
        [DataMember]
        public DateTimeUnit RecurrenceTimeUnit { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteCollectionPolicy
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SiteCollectionPolicyStatus
    {
        [EnumMember]
        None,
        [EnumMember]
        CustomDefined,
        [EnumMember]
        SiteCollectionDefined,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ActionValue
    {
        [EnumMember]
        None,
        [EnumMember]
        MoveToRecycleBin,
        [EnumMember]
        PermanentlyDelete,
        [EnumMember]
        SkipToNextStage,
        [EnumMember]
        DeletePreviousDrafts,
        [EnumMember]
        DeleteAllPreviousVersions,
    }
}
