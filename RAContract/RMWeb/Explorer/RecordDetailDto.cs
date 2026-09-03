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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AvePoint.RA.Contract.RMWeb
{
    public class RecordDetailDto
    {
        public RecordSummary Summary { get; set; }
        //detail页面点击move 需要完整的record信息
        public BaseRecordDto Record { get; set; }
        public GeneralProperty GeneralProperty { get; set; }
        public ManualReviewInfo ManualReviewInfo { get; set; }
        public RelatedRecordInfo RelatedRecordInfo { get; set; }
        public List<RecordHistory> RecordHistory { get; set; }
    }

    public class RecordSummary
    {
        public SourceFlag SourceFlag { get; set; }
        public string LeafName { get; set; }
        public string FullPath { get; set; }
        public string RecordId { get; set; }
        public string Term { get; set; }
        public string DisposalAction { get; set; }
        public string DisposalDate { get; set; }
        public bool DeclareAsRecord { get; set; }
        public bool LockByRecordLabel { get; set; }
        public Guid RuleId { get; set; }
        public string RuleName { get; set; }
        //public RMRuleInfos RuleDetail { get; set; }
        public HoldSetting HoldSetting { get; set; }
        public bool HoldStatus { get; set; }
        public string HoldReleaseTime { get; set; }
        public string HoldId { get; set; }
        public string HoldBy { get; set; }
        public string DeclaredBy { get; set; }
        public string ApplyRecordLabelBy { get; set; }
        public string TermSettings { get; set; }
    }

    public class GeneralProperty
    {
        public string DateType { get; set; }
        public string FileSize { get; set; }
        public string TimeCreated { get; set; }
        public string CreatedBy { get; set; }
        public string TimeModified { get; set; }
        public string ModifiedBy { get; set; }
        public string FolderPath { get; set; }
        public string CollectionTime { get; set; }
        public string SendTime { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Attachment { get; set; }
    }

    public class ManualReviewInfo
    {
        [JsonProperty("recordOwner")]
        public string RecordOwner { get; set; }

        [JsonProperty("reviewAudits")]
        public List<ReviewAudits> ReviewAudits { get; set; }
    }

    public class ReviewAudits
    {
        [JsonProperty("reviewTime")]
        public string ReviewTime { get; set; }

        [JsonProperty("reviewTimeTicks")]
        public long ReviewTimeTicks { get; set; }


        [JsonProperty("reviewBy")]
        public string ReviewBy { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("quickReason")]
        public string QuickReason { get; set; }

        [JsonProperty("extendTime")]
        public string ExtendTime { get; set; }
    }

    public class RelatedRecordInfo
    {
        public int RelateRecordCount { get; set; }
        public List<BaseRecordDto> Records { get; set; }
    }
    [DataContract]
    public class RecordHistory
    {
        /// <summary>
        /// for UI display
        /// </summary>
        [DataMember]
        public string DisplayTime { get; set; }
        /// <summary>
        /// store in db
        /// </summary>
        [DataMember]
        public long TimeUTC { get; set; }
        [DataMember]
        public string User { get; set; }
        [DataMember]
        public string Action { get; set; }
        [DataMember]
        public string Comment { get; set; }
    }

    [XmlRoot("RecordHistory")]
    public class RecordHistoryXml : XmlFile
    {
        [XmlElement("HistoryList")]
        public List<RecordHistory> HistoryList { get; set; }
    }

    public class XmlFile
    {
        
    }
    [DataContract]
    public enum ExplorerDetailTab
    {
        [EnumMember]
        All = 0,
        [EnumMember]
        Summary = 1,
        [EnumMember]
        Property = 2,
        [EnumMember]
        RelatedRecord = 3,
        [EnumMember]
        History = 4
    }
    [DataContract]
    public class UpdateHoldDto
    {
        [DataMember]
        public List<Guid> ReletedIds { get; set; }
        [DataMember]
        public HoldSetting HoldSetting { get; set; }
        [DataMember]
        public List<string> HoldIds { get; set; }
        [DataMember]
        public bool AllFolder { get; set; }
        /// <summary>
        /// 0:sp, exo; 1.Personal 2:physical
        /// </summary>
        [DataMember]
        public int HoldCategory { set; get; }
        /// <summary>
        /// 只用于Physcial File (Folder)
        /// </summary>
        [DataMember]
        public List<CompactRecord> FileIds { set; get; }
        /// <summary>
        /// 只用于Physical Box-->File之间的Override
        /// </summary>
        [DataMember]
        public bool IsOverRide { set; get; }
        [DataMember]
        public bool NeedCheckOverride { set; get; }

        //"change/append"
        [DataMember]
        public string HoldAction { get; set; }
        [DataMember]
        public bool IsSendEmailToBorrower { get; set; }
        [DataMember]
        public List<ToUserInfo> UserHoldManagers { get; set; }
        [DataMember]
        public bool IsHoldManagerEmailNotificationEnabled { get; set; }

    }
    [DataContract]
    public class CompactRecord
    {
        [DataMember]
        public Guid Id { set; get; }
        [DataMember]
        public RMNodeType NodeType { set; get; }
        [DataMember]
        public Guid BoxId { set; get; }
        [DataMember]
        public Guid LocationId { set; get; }
    }

    #region Hold notification
    [DataContract]
    public class HoldEmailNotification
    {
        [DataMember]
        public bool IsEnabled { get; set; }
        [DataMember]
        public int ReminderDurationDays { get; set; }
        [DataMember]
        public List<AOSUserDto> EmailRecipients { get; set; }
    }

    public class UserHoldNotification
    {
        public AOSUserDto User { get; set; }
        public List<HoldEmailItem> Holds { get; set; }
    }

    public class HoldEmailItem
    {
        public string HoldId { get; set; }
        public string HoldName { get; set; }
        public long HoldUtil { get; set; }
        public int RecordCount { get; set; }
    }
    #endregion
}
