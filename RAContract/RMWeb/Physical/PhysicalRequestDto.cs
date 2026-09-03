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
using AvePoint.RA.Contract.Object.RealTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Physical
{
    [DataContract]
    public class PhysicalRequestDto
    {
        [DataMember]
        public int Id { get; set; }
        /// <summary>
        /// for display in GUI
        /// </summary>
        [DataMember]
        public string RequestId { set; get; }
        [DataMember]
        public PhysicalRequestType Type { set; get; }
        [DataMember]
        public string Title { set; get; }
        [DataMember]
        public string RecordId { set; get; }
        [DataMember]
        public List<string> Titles { set; get; }
        [DataMember]
        public List<string> RecordIds { set; get; }
        [DataMember]
        public PhysicalRequestStatus Status { set; get; }
        [DataMember]
        public string CreatedUserId { set; get; }
        [DataMember]
        public string CreatedUserDisplay { set; get; }
        [DataMember]
        public string HoldUserId { set; get; }
        [DataMember]
        public string HoldUserDisplay { set; get; }
        [DataMember]
        public string ManagerUserId { set; get; }
        [DataMember]
        public string ManagerUserDisplay { set; get; }
        [DataMember]
        public long CreatedTime { set; get; }
        [DataMember]
        public string CreatedTimeStr { set; get; }
        [DataMember]
        public long ModifiedTime { set; get; }
        [DataMember]
        public string ModifiedTimeStr { set; get; }

        [DataMember]
        public string Comment { set; get; }

        //[Column(TypeName = "int")] 
        //public int DisposalClassId { set; get; } 
        [DataMember]
        public PhysicalRequestDisposal DisposalClass { set; get; }
        [DataMember]
        public string DisposalDetail { set; get; }

        [DataMember]
        public PhysicalObjectDto PhysicalFileInfo { set; get; }

        [DataMember]
        public List<PhysicalObjectDto> PhysicalFileInfos { set; get; }

        [DataMember]
        public Guid GroupRequestId { set; get; }
        [DataMember]
        public PhysicalMoveOption MoveDto { set; get; }
    }
    [DataContract]
    public enum PhysicalRequestType
    {
        [EnumMember]
        Loan,
        [EnumMember]
        Creation,
        [EnumMember]
        Move
    }
    [DataContract]
    public enum PhysicalRequestStatus
    {
        [EnumMember]
        WaitingForApproval = 0,
        [EnumMember]
        Approved,
        [EnumMember]
        Rejected,
        [EnumMember]
        CancelRequest
    }
    [DataContract]
    public class PhysicalRequestDisposal
    {
        [DataMember]
        public int Id { set; get; }
        [DataMember]
        public HoldCategory HoldCategory { set; get; }
        [DataMember]
        public string HoldAction { set; get; }
        [DataMember]
        public int HoldNumber { set; get; }
        [DataMember]
        public HoldUnit HoldUnit { set; get; }
        [DataMember]
        public string ReviewComment { set; get; }
        /// <summary>
        /// 用于回显
        /// </summary>
        [DataMember]
        public string EndTimeStr { set; get; }
        [DataMember]
        public string TimeZoneId { set; get; }
        [DataMember]
        public bool IsDaylightSavingTime { set; get; }
        /// <summary>
        /// 用于计算
        /// </summary>
        [DataMember]
        public long EndTime { set; get; }
        [DataMember]
        public int RequestId { set; get; }
    }
    [DataContract]
    public enum HoldCategory
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Before,
        [EnumMember]
        Last
    }
    [DataContract]
    public enum HoldUnit
    {
        [EnumMember]
        Day = 0,
        [EnumMember]
        Month,
        [EnumMember]
        Year
    }
    public enum PhysicalRequestFailType
    {
        None = 0,
        IsLoanedRecord = 1,
        ReturnTimeExpired = 2
    }

    public class PhysicalQueryRequestDto
    {
        public int Id { get; set; }

        public int Type { set; get; }

        public string Title { set; get; }

        public string PhysicalFileId { set; get; }

        public int Status { set; get; }

        public string CreatedUserId { set; get; }

        public string HoldUserId { set; get; }

        public string HoldByDisplayName { get; set; }

        public string ManagerUserId { set; get; }

        public long CreatedTime { set; get; }

        public long ModifiedTime { set; get; }

        public string MetaData { set; get; }

        public int HoldCategory { set; get; }

        public int HoldNumber { set; get; }

        public int HoldUnit { set; get; }

        public string TimeZoneId { set; get; }

        public bool IsDaylightSavingTime { set; get; }

        public string EndTimeStr { set; get; }

        public long EndTime { set; get; }

        public Guid GroupRequestId { set; get; }
        public string MoveInfo { set; get; }

        //public string Comment { set; get; }
        //public string ReviewComment { set; get; }
    }

    public class RequestBy
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string UserPrincipalName { get; set; }
    }
}
