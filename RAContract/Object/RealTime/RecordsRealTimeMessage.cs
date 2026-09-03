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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Object.RealTime
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RecordsRealTimeMessage
    {
        [DataMember(EmitDefaultValue = false)]
        public RealTimeAction Action { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string JobId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public ChangeTermOption ChangeTermOption { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> RecordIds { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public ServiceDto AgentInfo { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public RecordsDBInfo RecordsDBInfo { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public RecordsDBInfo ExplorerDBInfo { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string DeclareBy { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string LogonGroupId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string CurrentUserName { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public PhysicalMoveOption PhysicalMoveOption { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public GlobalSearchActionDto GlobalSearchInfo { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string ClientIP { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public ChangeLabelOption ChangeLabelOption { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public List<PhysicalMoveRequest> PhysicalMoveRequests{ set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ChangeTermOption
    {
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> SourceRecordIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> SourceEXORecordIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> SourcePhyRecordIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> SourceFSRecordIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> SourceSPOnPremRecordIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> SourceOneDriveRecordIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> SourceAzureFileShareRecordIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> SourceBoxRecordIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> SourceCustomizeConnectorRecordIds { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> GoogleDriveRecordIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> SourceTeamsRecordIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int TargetTermId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string TargetTermName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid TargetTermUniqueId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool OverWriteSubFiles { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool ReclassifySubFiles { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string LogonUser { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Comment { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsManualData { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public ChangeTermOrigin ChangeTermOrigin { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ChangeLabelOption
    {

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> GoogleDriveRecordIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int TargetLabelId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string TargetLabelName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid TargetLabelUniqueId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool OverWriteSubFiles { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool ReclassifySubFiles { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string LogonUser { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Comment { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RealTimeAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ChangeTerm = 1,
        [EnumMember]
        Declare = 2,
        [EnumMember]
        UnDeclare = 3,
        [EnumMember]
        PhysicalMove = 4,
        [EnumMember]
        GlobalSearchAction = 5,
        [EnumMember]
        MLReviewChangeTerm = 6,
        [EnumMember]
        MLReviewApprove = 7,
        [EnumMember]
        ChangeLabel = 8,
        [EnumMember]
        PhysicalMoveRequest = 9,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PhysicalMoveOption
    {
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> SourcePhyRecordIds { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string LocationId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string BoxId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string FolderId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public NameConflictOption NameConflictOption { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public PhysicalMoveHoldConflictOption HoldConflictOption { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string DestinationPath { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int FromModule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsSendEmailToDestinationRM { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum NameConflictOption
    {
        [EnumMember]
        Skip = 1,
        [EnumMember]
        Overwrite = 2,
        [EnumMember]
        Rename = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PhysicalMoveHoldConflictOption
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        UseDest = 1,
        [EnumMember]
        UseLongest = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ChangeTermType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SearchChangeTerm = 1,
        [EnumMember]
        AIMAChangeTerm = 2,
        [EnumMember]
        AIMADirectlyApprove = 3
    }
    public class PhysicalMoveRequest
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid GroupRequestId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PhysicalMoveOption PhysicalMoveOption { get; set; }
    }
}
