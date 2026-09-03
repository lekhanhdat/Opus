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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.TaxonomyModel;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.TaxonomyModel
{
    [DataContract]
    public class TermSettingsInfo
    {
        [DataMember]
        public int tId { get; set; }
        [DataMember]
        public Guid TermUniqueId { get; set; }
        [DataMember]
        public string des { get; set; }
        [DataMember]
        public List<RuleDisplayInfo> infos { get; set; }
        [DataMember]
        public string beginTime { get; set; }
        [DataMember]
        public string endTime { get; set; }
        [DataMember]
        public long beginTimeForDB { get; set; }
        [DataMember]
        public long endTimeForDB { get; set; }
        [DataMember]
        public bool IsDayLight { get; set; }
        [DataMember]
        public string TimeZoneId { get; set; }
        [DataMember]
        public DateType selDateType { get; set; }
        [DataMember]
        public double aSpace { get; set; }
        [DataMember]
        public int EnforceRetention { get; set; }
        [DataMember]
        public string EXORetentionLabel { get; set; }
        [DataMember]
        public string SPRetentionLabel { get; set; }
        [DataMember]
        public string OneDriveRetentionLabel { get; set; }
        [DataMember]
        public string TeamsRetentionLabel { get; set; }
        [DataMember]
        public bool breakInhert { get; set; }
        [DataMember]
        public string advanceSettings { get; set; }
    }
    [DataContract]
    public class TermInfo
    {
        [DataMember]
        public int TermId { get; set; }
        [DataMember]
        public Guid TermUniqueId { get; set; }
        [DataMember]
        public int TermSetId { get; set; }
        [DataMember]
        public Guid TermSetUniqueId { get; set; }
        [DataMember]
        public int TermGroupId { get; set; }
        [DataMember]
        public int ParentTermId { get; set; }
        [DataMember]
        public Guid ParentTermUniqueId { get; set; }
        [DataMember]
        public Guid TermGroupUniqueId { get; set; }
        [DataMember]
        public string TermName { get; set; }
        [DataMember]
        public string TermSetName { get; set; }
        [DataMember]
        public string TermGroupName { get; set; }
        [DataMember]
        public string TermStoreId { get; set; }
        [DataMember]
        public string TermStoreName { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool UsingMMSSpecified { get; set; }
        [DataMember]
        public List<RMSiteInfo> ReSiteInfos { get; set; }
        [DataMember]
        public string AdvanceSetting { get; set; }
        [DataMember]
        public int GoogleTermSyncOption { get; set; }
        [DataMember]
        public int M365TermSyncOption { get; set; }
    }

    public class ParentTermSettings
    {
        public string RuleInfos { get; set; }
        public int EnforceRetention { get; set; }
    }
    [DataContract]
    public class ContainerTypeInfo
    {
        [DataMember]
        public int ContainerId { get; set; }
        [DataMember]
        public string TypeName { get; set; }
        [DataMember]
        public float Size { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool IsDefault { get; set; }
    }

    public enum RMRetentionSourceType 
    {
        None = -1,
        Exchange = 0,
        SharePoint = 1,
        OneDrive = 2,
        Teams = 3,
    }

    public enum RMRetentionLabelStatus
    {
        FromGUI = 0,
        Previous = 1,
        JobProcessing = 2,
        Failed = 3
    }
    [DataContract]
    public enum RMTermType
    {
        [EnumMember]
        Root = 1,
        [EnumMember]
        TermGroup = 2,
        [EnumMember]
        TermSet = 3,
        [EnumMember]
        Term = 4
    }
    [DataContract]
    public enum RMRuleType
    {
        [EnumMember]
        Root = 1,
        [EnumMember]
        RuleContainer = 2,
        [EnumMember]
        Rule = 3
    }

    public enum ExportTermsWithRulesStatus
    {
        None = 0,
        InProgress = 1,
        Finished = 2,
    }
    public class ExportAddition
    {
        public string[] TermColumArray { get; set; }
        public string[] RuleColumArray { get; set; }
        public string[] ConditionArray { get; set; }
        public bool HasUpgradeTeams { get; set; }
        public bool IsSupportRecordLabelFunction { get; set; }
        public bool IsNeedAddRowData { get; set; }
    }
}