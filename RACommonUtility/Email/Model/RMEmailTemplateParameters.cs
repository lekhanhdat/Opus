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
using AvePoint.RA.Common.Security;
using AvePoint.RA.I18N.Core;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Email.Model
{
    public enum RMEmailTemplateType
    {
        None = 0,
        Physical = 1,
        Manual = 2,
        ExportZipPassword = 3,
        JobNotification = 4,
        HoldNotification = 5,
        BorrowerNotification = 6,
        HoldManagerNotification = 7
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class RMEmailTemplatePlaceholderAttribute : Attribute
    {

        public string PlaceHolder { get; private set; }

        public RMEmailTemplatePlaceholderAttribute(string placeholder)
        {
            PlaceHolder = placeholder;
        }
    }

    public class RMEmailTemplateParameters
    {
        public RMEmailTemplateType TemplateType { get; set; }

        public string ToUser { get; set; }

        public override bool Equals(object obj)
        {
            if (
                obj is not RMEmailTemplateParameters target ||
                string.IsNullOrWhiteSpace(ToUser) ||
                string.IsNullOrWhiteSpace(target.ToUser)
                )
            {
                return false;
            }

            return ToUser.Equals(target.ToUser, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return ToUser.ToLower().GetHashCode();
        }
    }

    public class RMPhysicalEmailTemplateParameters : RMEmailTemplateParameters
    {
        [RMEmailTemplatePlaceholder("$Request.ID$")]
        public string RequestId { get; set; }

        [RMEmailTemplatePlaceholder("$Request.Comment$")]
        public string RequestComment { get; set; }

        [RMEmailTemplatePlaceholder("$Request.Requester$")]
        public string RequestRequester { get; set; }

        [RMEmailTemplatePlaceholder("$Request.Assignee$")]
        public string RequestAssignee { get; set; }

        [RMEmailTemplatePlaceholder("$Request.Name$")]
        public string PhysicalRecordsName { get; set; }

        [RMEmailTemplatePlaceholder("$Request.UID$")]
        public string PhysicalRecordsUID { get; set; }

        [RMEmailTemplatePlaceholder("$Request.Requester.FirstName$")]
        public string RequestRequesterFirstName { get; set; }
    }

    public class RMManualEmailTemplateParameters : RMEmailTemplateParameters
    {
        [RMEmailTemplatePlaceholder("$Request.Reviewer$")]
        public string RequestReviewer { get; set; }

        [RMEmailTemplatePlaceholder("$Request.Comment$")]
        public string RequestComment { get; set; }

        public string UserId { get; set; } // guid

        public string RequestLink => AveUrlUtility.CombineUrl(RMSSOHelper.RecoSsoLoginUrl, "?redirect=Root/RDM/ManualApprovalReview");

        public string RequestLinkTitle => I18NEntity.GetString("RM_DAM_ManualApprovalReview");

        [RMEmailTemplatePlaceholder("$Current.Date$")]
        public string CurrentDate { get; set; }

        [RMEmailTemplatePlaceholder("$Request.Reviewer.FirstName$")]
        public string RequestReviewerFirstName { get; set; }
    }
    public class RMExportZipPasswordEmailTemplateParameters : RMEmailTemplateParameters
    {
        [RMEmailTemplatePlaceholder("$Request.Reviewer$")]
        public string RequestReviewer { get; set; }

        [RMEmailTemplatePlaceholder("$Request.JobId$")]
        public string RequestJobId { get; set; }

        [RMEmailTemplatePlaceholder("$Request.Location$")]
        public string RequestLocation { get; set; }

        [RMEmailTemplatePlaceholder("$Request.Password$")]
        public string RequestPassword { get; set; }

        public string UserId { get; set; } // guid

        [RMEmailTemplatePlaceholder("$Request.Reviewer.FirstName$")]
        public string RequestReviewerFirstName { get; set; }
    }

    public class RMJobNotificationEmailTemplateParameters : RMEmailTemplateParameters
    {
        [RMEmailTemplatePlaceholder("$Request.Reviewer$")]
        public string RequestReviewer { get; set; }

        [RMEmailTemplatePlaceholder("$Notification.Summary$")]
        public string RequestJobDetail { get; set; }

        public string RequestLink => AveUrlUtility.CombineUrl(RMSSOHelper.RecoSsoLoginUrl, "?redirect=Root/JM/Index");
    }

    public class RMBorrowerNotificationEmailTemplateParameters : RMEmailTemplateParameters
    {
        [RMEmailTemplatePlaceholder("$Physical.ItemName$")]
        public string PhysicalItemName { get; set; }

        [RMEmailTemplatePlaceholder("$Borrower.Name$")]
        public string BorrowerName { get; set; }

        [RMEmailTemplatePlaceholder("$PhysicalRecords.Name$")]
        public string PhysicalRecordName { get; set; }

        [RMEmailTemplatePlaceholder("$PhysicalRecords.UID$")]
        public string PhysicalRecordUID { get; set; }

        [RMEmailTemplatePlaceholder("$Return.Date$")]
        public string ReturnDate { get; set; }
    }
    public class RMHoldNotificationEmailTemplateParameters : RMEmailTemplateParameters
    {
        [RMEmailTemplatePlaceholder("$Email.Recipient$")]
        public string RequestReviewer { get; set; }

        [RMEmailTemplatePlaceholder("$Hold.Reminder.Summary$")]
        public string RequestHoldsInformation { get; set; }

        public string RequestLink => AveUrlUtility.CombineUrl(RMSSOHelper.RecoSsoLoginUrl, "?redirect=Root/BCM/ManageHold");
    }
    public class RMHoldManagerEmailTemplateParameters : RMEmailTemplateParameters
    {
        [RMEmailTemplatePlaceholder("$Email.Recipient$")]
        public string HoldManager { get; set; }
        [RMEmailTemplatePlaceholder("$Hold.Title$")]
        public string HoldName { get; set; }
        public string RequestLink => AveUrlUtility.CombineUrl(RMSSOHelper.RecoSsoLoginUrl, "?redirect=Root/BCM/ManageHold");
    }
}
