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
using AvePoint.RA.Contract.Common;
using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace AvePoint.RA.Contract.Explorer
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [ProtoContract]
    public class RecordDto
    {
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(1)]
        public Guid Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(2)]
        public int CreateDate { set; get; }

        // <summary>
        /// -1: None
        /// 1: SharePoint
        /// 2: FileSystem
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(3)]
        public int SourceFlag { get; set; }
        /// <summary>
        /// sp: real site id
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(4)]
        public Guid ScopeId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(5)]
        public Guid NodeId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(6)]
        public string DirPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(7)]
        public string RuleName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(8)]
        public string RecordsId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(9)]
        public int NodeType { get; set; }
        /// <summary>
        /// LeafName 是FullTextIndex字段, 需要not null
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(10)]
        public string LeafName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(11)]
        public string ExtensionForFile { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(12)]
        public Guid TermId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(13)]
        public string TermName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(14)]
        public Guid RuleId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(15)]
        public int RuleLevel { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(16)]
        public bool HoldStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(17)]
        public long HoldReleaseTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(18)]
        public int HoldType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(19)]
        public string HoldBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(20)]
        public string HoldId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(21)]
        public string RecordOwner { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(22)]
        public string RelatedRecords { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(23)]
        public int RelatedRecordsCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(24)]
        public string CreatedBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(25)]
        public string DisposalDueDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(26)]
        public string PreviosDisposalDueDate { get; set; }//TODO merge July2020

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(27)]
        public bool DeclareAsRecord { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool LockedByRecordLabel { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(28)]
        public DateTime TimeCreated1 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(29)]
        public long TimeLastModified { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(30)]
        public long CollectionTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(31)]
        public string RecordHistory { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(32)]
        public long SortTicks { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(33)]
        public long DestroyedTime { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(34)]
        public long TimeCreated { get; set; }

        #region for SP
        /// <summary>
        /// docave siteId, not real sp siteId
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(35)]
        public string AveSiteId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(36)]
        public Guid WebId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(37)]
        public Guid ListId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(38)]
        public Guid FolderId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(39)]
        public Guid ItemId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(40)]
        public int ItemRowId { get; set; }
        /// <summary>
        /// FullPath 是FullTextIndex字段, 需要not null
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(41)]
        public string FullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(42)]
        public string SourceLocation { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(43)]
        public string DestinationLocation { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(44)]
        public string MetaInfo { get; set; }
        #endregion

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(45)]
        public string DeclaredBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ApplyRecordLabelBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(46)]
        public string ModifiedBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(47)]
        public string Extsion1 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(48)]
        public Guid ParentId { get; set; }

        /// <summary>
        /// **此属性不足以判断具体数据类型，如果需要当做条件，应确认好原端类型以及ID 等字段进行确认
        /// 1:active 2, archived, 3 delete, 4 moved, 5 overwrited(Move job destination file can be overwrited)
        /// For physical: 1:Open, 2:Destroyed, 3: delete(RM 删除的文件，理论上不显示),  6:closed, 7: Missing. 不使用3， 4， 5 防止与其他值混淆
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(49)]
        public int RecordStatus { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(50)]
        public string ContainerId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(51)]
        public string ApproveUsers { get; set; }

        #region Physical Property
        /// <summary>
        /// Nearest Parent Location Id
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(52)]
        public Guid LocationId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(53)]
        public Guid BoxId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(54)]
        public Guid FileId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(55)]
        public int TemplateId { get; set; }
        #endregion

        #region not mapped propertity
        //[NotMapped]
        //public string FullPath { get; set; }
        #endregion

        #region
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(56)]
        public int HasRelatedDocument { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(57)]
        public int DeleteRelatedRecords { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(58)]
        public string RelatedRecordInfo { get; set; }
        #endregion
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(59)]
        public string[] RecordOwner_Array { get; set; }
        //used for job detail only
        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(60)]
        public string Comment { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(61)]
        public bool BulkImportEnabled { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [ProtoMember(62)]
        public int BulkSize { get; set; }

    }
}
