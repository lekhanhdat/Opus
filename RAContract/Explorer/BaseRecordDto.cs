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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    [DataContract]
    public class BaseRecordDto
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public int CreateDate { set; get; }
        [DataMember]
        public string RecordsId { get; set; }
        [DataMember]
        public Guid ScopeId { get; set; }
        [DataMember]
        public Guid NodeId { get; set; }
        [DataMember]
        public string DirPath { get; set; }// to do next
        [DataMember]
        public string EmailAddress { get; set; }
        [DataMember]
        public string ExternalId { get; set; }
        [DataMember]
        public string ContainerId { get; set; }

        /// <summary>
        /// For Physical Record
        /// 0: Undefined
        /// 1: Location
        /// 100: Box
        /// 500: File
        /// 1000: Record
        /// </summary>
        [DataMember] 
        public int NodeType { get; set; }
        [DataMember]
        public string LeafName { get; set; }
        [DataMember]
        public string ExtensionForFile { get; set; }
        [DataMember]
        public int RecordStatus { get; set; }
        [DataMember]
        public Guid TermId { get; set; }
        [DataMember]
        public string TermName { get; set; }
        [DataMember]
        public Guid RuleId { get; set; }
        [DataMember]
        public string RuleName { get; set; }
        [DataMember] 
        public int RuleLevel { get; set; }
        [DataMember]
        public bool HoldStatus { get; set; }
        [DataMember]
        public string HoldSetting { get; set; }
        [DataMember]
        public string HoldBy { get; set; }
        [DataMember]
        public string RelatedRecords { get; set; }
        [DataMember]
        public int RelatedRecordsCount { get; set; }
        [DataMember] 
        public string ModifiedBy { get; set; }

        /// <summary>
        /// -1: None
        /// 0: All
        /// 1: SharePoint
        /// 2: FileSystem
        /// 3: Exchange
        /// 4: Physical
        /// </summary>
        [DataMember] 
        public int SourceFlag { get; set; }
        [DataMember]
        public string SourceName { get; set; }
        [DataMember]
        public string CreatedBy { get; set; }
        [DataMember]
        public string Audits { get; set; }
        [DataMember]
        public bool DeclareAsRecord { get; set; }
        [DataMember]
        public bool LockedByRecordLabel { get; set; }
        [DataMember]
        public string RecordOwner { get; set; }
        [DataMember] 
        public string RecordOwnerPrincipalName{ get; set; }
        [DataMember]
        public int DisposalAction { get; set; }
        [DataMember] 
        public int ExchangeDisposalAction { get; set; }
        [DataMember] 
        public string DisposalDueDate { get; set; }
        [DataMember]
        public long TimeCreated { get; set; }
        [DataMember]
        public string TimeCreatedStr { get; set; }

        [DataMember]
        public long TimeLastModified { get; set; }

        [DataMember]
        public string TimeLastModifiedStr { get; set; }

        [DataMember]
        public long TimeArchived { get; set; }

        [DataMember]
        public string TimeArchivedStr { get; set; }

        [DataMember]
        public long CollectionTime { get; set; }
        [DataMember] 
        public string RecordHistory { get; set; }
        [DataMember] 
        public Guid BoxId { get; set; }
        [DataMember] 
        public Guid FileId { get; set; }
        [DataMember] 
        public List<Guid> Ancestors { get; set; }
        [DataMember] 
        public Guid ParentId { get; set; }
        [DataMember] 
        public Guid LocationId { get; set; }
        [DataMember] 
        public int TemplateId { get; set; }

        #region for SP
        /// <summary>
        /// Root Location Id for Physical...
        /// </summary>
        [DataMember] 
        public string AveSiteId { get; set; }
        [DataMember] 
        public Guid WebId { get; set; }
        [DataMember]
        public Guid ListId { get; set; }
        [DataMember]
        public Guid FolderId { get; set; }
        [DataMember] 
        public Guid ItemId { get; set; }
        [DataMember] 
        public int ItemRowId { get; set; }
        [DataMember]
        public string FullPath { get; set; }
        [DataMember]
        public string MetaInfo { get; set; }
        [DataMember] 
        public string ReleaseTime { get; set; }
        [DataMember] 
        public string HoldTitle { get; set; }
        [DataMember] 
        public string HoldId { get; set; }
        [DataMember] 
        public List<HoldUser> HoldByUsers { get; set; }
        [DataMember] 
        public string[] AppendHolds_Array { get; set; }
        [DataMember]
        public long HoldReleaseTime { get; set; }
        [DataMember]
        public string PreviosDisposalDueDate { get; set; }

        //add for archivedContentDownload
        [DataMember] 
        public int ContentDownloadStatus { get; set; }
        [DataMember]
        public long DestryoedTime { get; set; }

        #endregion

        #region for Phy Loan
        [DataMember]
        public bool PersonHold { set; get; }
        [DataMember]
        public string PersonHoldBy { get; set; }
        [DataMember]
        public string PersonHoldReleaseTime { get; set; }
        [DataMember]
        public bool BoxPersonHold { set; get; }
        #endregion

        #region Pick Status
        [DataMember]
        public int LoanPickStatus { get; set; }
        [DataMember]
        public int DestructionPickStatus { get; set; }
        [DataMember]
        public int ManualApprovedBy { get; set; }
        [DataMember]
        public Dictionary<string, CustomColumn> CustomColumnDic { set; get; }
        #endregion Pick Status

        #region Machine Learning

        [DataMember]
        public Guid PredictTermId { get; set; }

        [DataMember]
        public string PredictTermName { get; set; }

        [DataMember]
        public long PredictTime { get; set; }

        [DataMember]
        public int MLApprovalStatus { get; set; }
        #endregion


        public bool HasManagerHold { get; set; }
        public bool HasDelegatedAdmin { get; set; }
        /// <summary>
        /// Used for transfer extra value between services, not for serialization.
        /// </summary>
        [IgnoreDataMember]
        public string ExtensionValue { get; set; }
    }

    public class RecordKey
    {
        public Guid ScopeId { get; set; }

        public string DirPath { get; set; }

        public override string ToString()
        {
            return this.ScopeId + ";" + this.DirPath;
        }
    }
}
