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
using AvePoint.GCommon.Contract.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Explorer
{
    /// <summary>
    /// 序列化到Creation的Request中,  Approve后存到Record
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [JsonObject]
    public class PhysicalObjectDto
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int CreateDate { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public RMNodeType NodeType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string UniqueId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid TermId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid LocationId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string LocationName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string HomeLocationFullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermFullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid BoxId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid FileId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int TemplateId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsLocked { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string LockedBy { get; set; }

        /// <summary>
        ///  1:Open, 2:Destroyed, 3: delete(RM 删除的文件，理论上不显示),  6:closed, 7: Missing. 不使用3， 4， 5 防止与其他值混淆
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int Status { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Dictionary<string, string> MetaInfo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public TemplateDto Template { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<SimplifyTemplateDto> ChildTemplates { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int BoxTemplateId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string CreatedBy { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public long CreateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string CreateTimeStr { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ModifiedBy { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string BarcodeBase64Str { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string BarcodeId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public long ModifiedTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ModifiedTimeStr { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DisposalDueDate { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string RecordOwner { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int HoldType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public HoldStatus HoldStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool DisposalHold { set; get; }
        /// <summary>
        /// Only for disposal hold display
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string HoldBy { get; set; }
        /// <summary>
        /// Only for disposal hold display
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public long HoldReleaseTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string HoldReleaseTimeStr { get; set; }

        /// <summary>
        /// for disposal hold display
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string HoldProfileTitle { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string HoldProfileId{ set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string HoldProfileComment { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid RuleId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int SourceFlag { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool PersonHold { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool BoxPersonHold { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string PersonHoldBy { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public long PersonHoldReleaseTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string PersonHoldReleaseTimeStr { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string RuleName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int RuleAction { get; set; }
        //Only for Room location
        [DataMember(EmitDefaultValue =false)]
        [JsonProperty]
        public double Capacity { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int RelatedRecordsCount { get; set; }
        //Add for Mobile Now, Mobile 目前只需要额外加一个属性，如果以后加的多，可以考虑继承当前类进行扩展
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool HasRequest { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public PhysicalRequestDto PhysicalRequestDto { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool ExportToRECO { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public PhysicalObjectPermissionDto ScopePerDto { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int ScopePermissionId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ColumnB { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ColumnC { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Dictionary<string,string> ColumnD { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ColumnE { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ColumnF { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ImageBase64Str { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<RecordHistory> RecordHistory { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DestroyedTime{ get; set; }

        #region custom container
        /// <summary>
        /// ancestor id start from bottom location
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<Guid> Ancestors { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid ParentId { get; set; }

        #endregion

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string PhysicalActionAudit { get; set; }
    }
    [DataContract]
    public enum HoldStatus
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Self = 1,
        [EnumMember]
        Inherit = 2
    }
    [DataContract]
    public class BuldUpdatePhysicalDto
    {
        [DataMember]
        public List<Guid> RecordIds { get; set; }
        [DataMember]
        public Dictionary<string, string> MetaInfo { get; set; }
        [DataMember]
        public int NodeType { get; set; }
        [DataMember]
        public int TemplateId { get; set; }
    }
    [DataContract]
    public class QueryTemplateDto
    {
        [DataMember]
        public Guid LocationUid { get; set; }
        [DataMember]
        public Guid TemplateId { get; set; }
        [DataMember]
        public PhysicalObjectDto PhyNodeInfo { get; set; }
    }
    [DataContract]
    public class QueryPhyObjectDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public int NodeType { get; set; }
        [DataMember]
        public string TemplateIdPath { get; set; }
        [DataMember]
        public PhysicalObjectDto PhyNodeInfo { get; set; }
    }

    public class BarCodeImageInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double HR { get; set; }
        public double VR { get; set; }
    }

    [DataContract]
    public class PhysicalAudit
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public long ActionTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ActionTimeStr { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ActionUser { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public PhysicalActionType ActionType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<PhysicalAuditItem> ModifyContent { get; set; }
    }

    [DataContract]
    public class PhysicalReturnHistory
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ReturnTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ItemName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string UniqueId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string RequestBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string HomeLocation { get; set; }
    }

    public class PhysicalReturnHistoryResponse
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<PhysicalReturnHistory> Datas { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int TotalCount { get; set; }
    }

    public class PhysicalAuditItem
    {
        public Guid Id { get; set; }
        public string TargetSetting { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }
    }

    public enum PhysicalActionType
    {
        Create = 0,
        Edit = 1,
        ManageRelated = 2,
        ImportCreate = 3,
        ImportEdit = 4,
        Disposal = 5,
        Move = 6,
        PlaceHold = 7, 
        CancelHold = 8,
        ExtendHold = 9,
        AccessControl = 10,
        Loan = 11,
        Reclassify = 12,
        ReturnLoan = 13,
        AddHold = 14,
        ChangeHold = 15,
        //CalculateDisposalDate = 16,
    }
}
