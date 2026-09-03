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
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Feedback.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FeedbackInfoDto
    {
        [DataMember]
        public String AccountId { get; set; }

        [DataMember]
        public String AccountOrganization { get; set; }

        [DataMember]
        public Boolean IsMindContact { get; set; }

        [DataMember]
        public RateExperience RateExperience { get; set; }

        [DataMember]
        public BugReportInfoDto BugReportInfo { get; set; }

        [DataMember]
        public InterfaceFeedbackInfoDto InterfaceFeedbackInfo { get; set; }

        [DataMember]
        public FeatureSuggestionInfoDto FeatureSuggestionInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BugReportInfoDto
	{
        [DataMember]
        public List<ProductType> ProductTypes { get; set; }

        [DataMember]
        public BugType BugType { get; set; }

        [DataMember]
        public GUIBugType GUIBugType { get; set; }

        [DataMember]
        public BugSeverity Severity { get; set; }

        [DataMember]
        public String Description { get; set; }
	}

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InterfaceFeedbackInfoDto
	{
        [DataMember]
        public TreePerformance TreePerformance { get; set; }

        [DataMember]
        public LearningExperience LearningExperience { get; set; }

        [DataMember]
        public String AdditionalFeedback { get; set; }
	}

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FeatureSuggestionInfoDto
    {
        [DataMember]
        public List<ProductType> ProductTypes { get; set; }

        [DataMember]
        public String Suggestion { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RateExperience : int
    { 
        [EnumMember]
        None = 0,

        [EnumMember]
        One = 1,

        [EnumMember]
        Two = 2,

        [EnumMember]
        Three = 3,

        [EnumMember]
        Four = 4,

        [EnumMember]
        Five = 5,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProductType
    {
        [EnumMember]
        None,

        [EnumMember]
        General,

        [EnumMember]
        Administrator,

        [EnumMember]
        ContentManager,

        [EnumMember]
        Granular,

        [EnumMember]
        Replicator,

        [EnumMember]
        ExchangeOnline,

        [EnumMember]
        ReportCenter,

        [EnumMember]
        DeploymentManager,

        [EnumMember]
        Archiver,

        [EnumMember]
        CloudAppAdmin
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BugSeverity 
    {
        [EnumMember]
        Trivial,

        [EnumMember]
        Minor,

        [EnumMember]
        Normal,

        [EnumMember]
        Major,

        [EnumMember]
        Critical,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ContactType
    {
        [EnumMember]
        Phone,

        [EnumMember]
        Email,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BugType
    {
        [EnumMember]
        Logical,

        [EnumMember]
        GUI,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum IssueType
    {
        [EnumMember]
        Guidance,

        [EnumMember]
        Troubleshoot,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum GUIBugType
    {
        [EnumMember]
        Interaction,

        [EnumMember]
        Spelling,

        [EnumMember]
        Grammar,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TreePerformance
    {
        [EnumMember]
        None,

        [EnumMember]
        Fast,

        [EnumMember]
        FastWithIssue,

        [EnumMember]
        Slow,

        [EnumMember]
        SlowWithIssue,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LearningExperience
    {
        [EnumMember]
        None,

        [EnumMember]
        Natural,

        [EnumMember]
        Easy,

        [EnumMember]
        Tricky,

        [EnumMember]
        Difficult,
    }
}
