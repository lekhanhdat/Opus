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

namespace AvePoint.RA.Contract.RMWeb.CP
{
    [DataContract]
    public class EmailTemplateDto
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public Guid UniqueId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public string Subject { get; set; }
        [DataMember]
        public string CC { get; set; }
        [DataMember]
        public string Body { get; set; }
        [DataMember]
        public int IsUseDefaultFooter { get; set; }
        [DataMember]
        public bool IsNewTemplate { get; set; }
        [DataMember]
        public bool IsRemoved { get; set; }
        [DataMember]
        public bool IsCustomTemplate { get; set; }
        [DataMember]
        public List<EmailImageDto> ImageList { get; set; }
        [DataMember]
        public int? CopySourceId { get; set; }
    }
    [DataContract]
    public class GetAllEmailTemplateDto
    {
        [DataMember]
        public int PagerIndex { get; set; }
        [DataMember]
        public int PagerSize { get; set; }
    }

    public class EmailTemplatePagerInfo
    {
		public int PagerIndex { get; set; }
		public int PagerSize { get; set; }
		public int TotalCount { get; set; }
	}

	public class EmailTemplatesInfo
	{
		public List <EmailTemplateDto> Items { get; set; }
		public EmailTemplatePagerInfo PagerInfo { get; set; }
	}

    public enum EmailTemplateType
    {
        BoxOrFile = 1,
        FileOrRecord = 2,
        RecordsForReview = 3,
        MLRecordsForReview = 4,
        ExportZipPasswordForReview = 5,
        JobNotification=6,
        HoldNotification = 7,
        BorrowerNotification = 8,
        HoldManagerNotification = 9,
    };
    [DataContract]
    public enum EmailTemplateInternalType
    {
        [EnumMember]
        LoanRequsetToEndUser = 1,
        [EnumMember]
        LoanRequsetToRM = 2,
        [EnumMember]
        LoanRequsetApproved = 3, 
        [EnumMember]
        LoanRequsetRejected = 4,
        [EnumMember]
        CreationRequestToEndUser = 5,
        [EnumMember]
        CreationRequestToRM = 6,
        [EnumMember]
        CreationRequestApproved = 7,
        [EnumMember]
        CreationRequestRejected = 8,
        [EnumMember]
        WaitingApproval = 9,
        [EnumMember]
        Approved = 10,
        [EnumMember]
        Rejected = 11,
        [EnumMember]
        Escalated = 12,
        [EnumMember]
        ManualApproval = 13,
        [EnumMember]
        MLManualApproval = 14,
        [EnumMember]
        ExportZipPassword = 15,
        [EnumMember]
        JobNotification = 16,
        [EnumMember]
        BorrowerNotification = 17,
        [EnumMember]
        MoveRequestToEndUser = 18,
        [EnumMember]
        MoveRequestToRM = 19,
        [EnumMember]
        MoveRequestApprovedToEndUser = 20,
        [EnumMember]
        MoveRequestRejected = 21,
        [EnumMember]
        MoveRequestApprovedToDestinationRM = 22,
    };

    public class ParameterDto
    {
        public string RequestID { get; set; }
        public string RequsetComment { get; set; }
        public string Requester { get; set; }
        public string Assignee { get; set; }
        public string PhscicalRecordName { get; set; }
        public string PhscicalRecordUID { get; set; }
        public string ExportLocation { get; set; }
        public string ZipPassword { get; set; }
        public string RestoreJobid { get; set; }
        public string Reviewer { get; set; }
        public string CurrentDate { get; set; }
        public string RequestRequesterFirstname { get; set; }
        public string BorrowName { get; set; }
        public string ReturnDate { get; set; }
        public ParameterMoveDto MoveInfo { get; set; }
    }
    public class ParameterMoveDto
    {
        public int SuccessfullCount { get; set; }
        public int FailedCount { get; set; }
        public string OriginalLocation { get; set; }
        public string DestinationLocation { get; set; }
        public string DestinationRM { get; set; }
    }

    public class ManualParameterDto
    {
        public string Reviewer { get; set; }

        public string ReviewerEmail { get; set; }

        public string Comment { get; set; }

        public string CurrentDate { get; set; }

        public string RequestReviewerFirstName { get; set; }
    }

    public class JobNotificationParameterDto
    {
        public string Reviewer { get; set; }

        public string ReviewerEmail { get; set; }

        public string JobDetail { get; set; }

    }

    public class HoldNotificationParameterDto
    {
        public string Reviewer { get; set; }

        public string ReviewerEmail { get; set; }

        public string HoldsInformation { get; set; }

    }

    public class RequestLinkDto
    {
        public string Url { get; set; }

        public string Title { get; set; }
    }
    [DataContract]
    public class EmailImageDto
    {
        [DataMember]
        public string ImageId { get; set; }
        [DataMember]
        public string Base64 { get; set; }
        [DataMember]
        public string FileType { get; set; }
        [DataMember]
        public string EmailTemplateId { get; set; }
    }

    public enum DefaultFooterStatus
    {
        UseDefaultFooter = 0,
        NoUseDefaultFooter,
    }


}
