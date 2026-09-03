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
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Office.CustomUI;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Configuration;
using System.Data.SqlClient;
using AvePoint.GCommon.Utility;
using System.Data.Entity;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMEamilTemplateDao : BaseDao<RMEmailTemplate>, IRMEamilTemplateDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RMEamilTemplateDao));
        private static readonly Guid LoanRequestEndUser_Template_Id = new Guid("AED805AD-AAD3-4238-BE51-18A629D27E9A");
        private static readonly Guid LoanRequestRM_Template_Id = new Guid("FEDCDDD8-6415-478A-BF3E-1E2F6D4E3892");
        private static readonly Guid LoanRequestApproved_Template_Id = new Guid("87B2759E-CBEE-42BB-AC79-8C993108E91B");
        private static readonly Guid LoanRequestReject_Template_Id = new Guid("61824ED2-EC98-47E9-A43E-982562F2BBD1");

        private static readonly Guid NewCreationRequestEndUser_Template_Id = new Guid("7F5CAABD-D7FA-4CA3-83D8-2C9FBAFFD3E9");
        private static readonly Guid NewCreationRequestRM_Template_Id = new Guid("60ABB721-46CB-4491-A5C8-28B939059876");
        private static readonly Guid CreationRequestApproved_Template_Id = new Guid("3FF42C82-3BB9-4F5F-98FA-4757DC1F550C");
        private static readonly Guid CreationRequestReject_Template_Id = new Guid("250D4D19-2B16-4432-84E3-F7DB83A22F49");

        private static readonly Guid MoveRequestEndUser_Template_Id = new Guid("3F2C6A0D-1B74-47C8-A9E2-5D9F2B1A8E61");
        private static readonly Guid MoveRequestRM_Template_Id = new Guid("D4A8F13E-96B7-4E2D-8B51-71C3F5A2D984");
        private static readonly Guid MoveRequestApprovedEndUser_Template_Id = new Guid("9B7E1F2A-3D4C-4A6F-BF90-2E15C8D7436B");
        private static readonly Guid MoveRequestReject_Template_Id = new Guid("F1C4A8D7-5E2B-41C9-8D63-A7B95E14F032");
        private static readonly Guid MoveRequestApprovedDestinationRM_Template_Id = new Guid("A7F3C291-8D64-4B17-A9E2-53C8F0D6B421");

        private static readonly Guid WaitingApproval_Template_Id = new Guid("E9A34229-0713-4F84-A5C3-65F46E616E4D");
        private static readonly Guid Approvaled_Template_Id = new Guid("0C8C8ACA-1B74-47C6-8CEC-B65F39D7238B");
        private static readonly Guid Rejected_Template_Id = new Guid("C4A7095B-E732-4341-8B78-156FF5F9594B");
        private static readonly Guid Escalated_Template_Id = new Guid("2E54FE0E-BB7D-425E-A4B8-DA68F8CEF816");

        private static readonly Guid ManualApproval_Template_Id = new Guid("5E4C71E3-A9F9-4DF3-B517-6CD45518EB56");

        private static readonly Guid ML_ManualApproval_Template_Id = new Guid("2C13FB99-4C44-4782-8AFF-40940B8C46F6");
        public static readonly Guid ExportZipPassword_Template_Id = new Guid("72D42C24-132A-4768-9D32-8A5DAFAFA083");
        private static readonly Guid JobNotification_Template_Id = new Guid("6f109e58-bb1e-4407-b88e-fcee0599742e");
        private static readonly Guid BorrowerNotification_Template_Id = new Guid("A7D9F2C3-6E4B-4FA1-9B8E-2D3C7A5F81D6");
        private static readonly Guid HoldNotification_Template_Id = new Guid("B3F7A2D1-8E4C-4F6A-9D5B-2C1E3A4F5B6D");
        public static readonly Guid HoldManagerNotification_Template_Id = new Guid("D4E5F6A7-B8C9-4D0E-9F1A-2B3C4D5E6F7A");

        private static readonly string BodyForLoanEndUser = "Hi $Request.Requester$,\n" +
            "Thank you for submitting a loan request for an existing physical item.\n" +
            "Your request has been submitted. Request ID: $Request.ID$.\n" +
            "We will process your request and will contact you for more information if required. You can use the request ID to track the progress of the request.\n" +
            "Thanks for contacting the records management team!\n" +
            "Sincerely.";

        private static readonly string BodyForLoanRM = "Hi $Request.Assignee$,\n" +
            "A physical item loan request has been submitted. A summary of the request is below.\n" +
            "Request ID: $Request.ID$\n" + "Requester: $Request.Requester$\n" + "Physical Item Name: $PhysicalRecords.Name$\n" + "Physical Item Unique ID: $PhysicalRecords.UID$\n" + "Comment: $Request.Comment$\n" +
            "Please go to AvePoint Opus > My Tasks > Requests for Review to process the request.\n" +
            "Sincerely.";

        private static readonly string BodyForLoanApproved = "Hi $Request.Requester$,\n" +
           "Your physical item loan request has been approved and the item will be delivered to you. Please find a summary below.\n" +
           "Request ID: $Request.ID$\n" + "Approved By: $Request.Assignee$\n" + "Physical Item Name: $PhysicalRecords.Name$\n" + "Physical Item Unique ID: $PhysicalRecords.UID$\n" + "Comment: $Request.Comment$\n" +
           "If you have any questions, please contact $Request.Assignee$ for assistance.\n" +
           "Sincerely.";

        private static readonly string BodyForLoanReject = "Hi $Request.Requester$,\n" +
          "Unfortunately, your physical item loan request cannot be completed. Please find a summary below.\n" +
          "Request ID: $Request.ID$\n" + "Completed By: $Request.Assignee$\n" + "Physical Item Name: $PhysicalRecords.Name$\n" + "Physical Item Unique ID: $PhysicalRecords.UID$\n" + "Comment: $Request.Comment$\n" +
          "If you have any questions, please contact $Request.Assignee$ for assistance.\n" +
          "Sincerely.";

        private static readonly string BodyForCreationEndUser = "Hi $Request.Requester$,\n" +
           "Thank you for submitting a physical item creation request.\n" +
           "Your request has been submitted. Request ID: $Request.ID$.\n" +
           "We will process your request and will contact you for more information if required. You can use the request ID to track the progress of the request.\n" +
           "Thanks for contacting the records management team!\n" +
           "Sincerely.";

        private static readonly string BodyForCreationRM = "Hi $Request.Assignee$,\n" +
           "A physical item creation request has been submitted. A summary of the request is below\n" +
           "Request ID: $Request.ID$\n" + "Requester: $Request.Requester$\n" + "Physical Item Name: $PhysicalRecords.Name$\n" + "Comment: $Request.Comment$\n" +
           "Please go to AvePoint Opus > My Tasks > Requests for Review to process the request.\n" +
           "Sincerely.";

        private static readonly string BodyForCreationApproved = "Hi $Request.Requester$,\n" +
          "Your physical item creation request has been approved. Please find a summary below.\n" +
          "Request ID: $Request.ID$\n" + "Approved By: $Request.Assignee$\n" + "Physical Item Name: $PhysicalRecords.Name$\n" + "Physical Item Unique ID: $PhysicalRecords.UID$\n" + "Comment: $Request.Comment$\n" +
          "If you have any questions, please contact $Request.Assignee$ for assistance.\n" +
          "Sincerely.";

        private static readonly string BodyForCreationReject = "Hi $Request.Requester$,\n" +
         "Unfortunately, your physical item creation request cannot be completed. Please find a summary below.\n" +
         "Request ID: $Request.ID$\n" + "Completed By: $Request.Assignee$\n" + "Physical Record Name: $PhysicalRecords.Name$\n" + "Comment: $Request.Comment$\n" +
         "If you have any questions, please contact $Request.Assignee$ for assistance.\n" +
         "Sincerely.";

        private static readonly string BodyForMoveEndUser = "Hi $Request.Requester$,\n" +
            "Thank you for submitting a move request for an existing physical item. Your request has been submitted. Request ID: $Request.ID$.\n" +
            "We will process your request and will contact you for more information if required. You can use the request ID to track the progress of the request.\n" +
            "Thanks for contacting the records management team!\n" +
            "Sincerely.";

        private static readonly string BodyForMoveRM = "Hi $Request.Assignee$,\n" +
            "A physical item movement request has been submitted for your review.\n" +
            "Request ID: $Request.ID$\n" + "Requester: $Request.Requester$\n" + "Destination Location: $Request.Destination$\n" + "Comment: $Request.Comment$\n" +
            "Please go to AvePoint Opus > My Tasks > Requests for Review to process the request.\n" +
            "Sincerely.";

        private static readonly string BodyForMoveApproved = "Hi $Request.Requester$,\n" +
           "Your physical item movement request has been processed and the item will be delivered to the new configured destination. Please find a summary below. \n" +
           "Request ID: $Request.ID$\n" + "Approved By: $Request.Assignee$\n" + "Successful Items: $Request.Successful.Count$\n" + "Failed Items: $Request.Failed.Count$\n" + "Comment: $Request.Comment$\n" +
           "If you have any questions, please contact $Request.Assignee$ for assistance.\n" +
           "Sincerely.";

        private static readonly string BodyForMoveReject = "Hi $Request.Requester$,\n" +
          "Unfortunately, your physical item movement request cannot be completed. Please find a summary below. \n" +
          "Request ID: $Request.ID$\n" + "Completed By: $Request.Assignee$\n" + "Comment: $Request.Comment$\n" +
          "If you have any questions, please contact $Request.Assignee$ for assistance.\n" +
          "Sincerely.";

        private static readonly string BodyForMoveApproveDesRM = "Hi $Destination.RecordsManager$,\n" +
          "A physical records movement request has been approved and the following items are scheduled to move to your location.\n" +
          "Request ID: $Request.ID$\n" + "Approved By: $Request.Assignee$\n" + "Original Location: $Request.SourceLocation$\n" + "Destination Location: $Request.Destination$\n" + "Number of Items: $Request.Successful.Count$\n" +
          "Please prepare to receive the listed physical items.\n" +
          "If you have any questions or detailed list of items, please contact $Request.Assignee$ - who approved this request for assistance.\n" +
          "Sincerely.";

        private static readonly string ManualApprovalEmailBody = @"Hi $Request.Reviewer$,

You have been assigned a review task for records that are due for disposal. You can find the items that require you decision at this link $Request.Link$. You can choose to approve, reject, or delay the disposal of the items assigned to you. You can also reassign the decision for any items that should not be assigned to you.
Comment: $Request.Comment$

If you have any questions, or this review has been assigned to you in error, please contact your Administrator.

Thank you for your participation in this process.

Best Regards.
";


        private static readonly string MLManualApprovalEmailBody = @"Hi $Request.Reviewer$,

You have been assigned a review task to review items that have been classified using Maestro AI. You can find the items that require a review decision at this link $Request.Link$. You can choose to approve the classification that was applied by Maestro AI or you can choose to reclassify the item with a more appropriate term. You can also reassign the items for anything that should not be assigned to you.
Comment: $Request.Comment$

If you have any questions, or this review has been assigned to you in error, please contact your Administrator.

Thank you for your participation in this process.

Best Regards.
";

        public static readonly string ExportZipPasswordEmailBody = "Dear $Request.Reviewer$\r\n\r\n" +
            "You requested the restoration of content that was archived, which was processed in the JobID $Request.JobId$. " +
            "The content you have asked to restore will be available in this location $Request.Location$.\r\n\r\nTo ensure that content is appropriately protected, " +
            "It has been password protected. You will need to enter the following password when you extract the content to a storage location or local file system. $Request.Password$\r\n\r\n" +
            "If you have any questions or did not request that content be restored, please contact your AvePoint Opus Administrator ASAP.\r\n\r\nKind Regards,";


        private static readonly string JobNotificationEmailBody = @"Hi $Request.Reviewer$,

Please check the job status summary below. If any issues or concern, please feel free to contact AvePoint support team.

$Notification.Summary$

If you have any questions, please contact your Administrator.

Go to AvePoint Opus &gt; Job monitor to check the details.

Sincerely,
The Opus Team @ AvePoint
";
        private static readonly string BorrowerNotificationEmailBody = @"Hi $Borrower.Name$,
The following physical item assigned to you is currently overdue for return. Please review the item details and return the physical asset as soon as possible.
•	Physical Item Name: $PhysicalRecords.Name$
•	Physical Item Unique ID: $PhysicalRecords.UID$
•	Current Return Date: $Return.Date$
Go to AvePoint Opus &#62; Physical Records &#62; Explorer to review the item details and return status.
If you have any questions, please contact your Administrator.
Best Regards,
The Opus Team @ AvePoint.
";

        private static readonly string HoldNotificationEmailBody =@"Hi $Email.Recipient$,

The following hold(s) are approaching their expiration date soon. Please review the hold details and determine whether the hold(s) should be extended or allowed to expire.

$Hold.Reminder.Summary$

After the hold expires, protected content may become eligible for standard lifecycle and disposal processing based on existing retention policies.

Go to AvePoint Opus &gt; Manage Holds to view hold details and the list of items attached.

If you have any questions, please contact your Administrator.

Best Regards,

The Opus Team @ AvePoint
";

        private static readonly string HoldManagerNotificationEmailBody = @"Hi $Email.Recipient$,

You have been assigned as a Hold Manager for the following hold: $Hold.Title$.

As a Hold Manager, you can access the Manage Holds page to:

    • Create, edit, extend, or delete the hold.
    • Apply or remove the hold from records.
    • Search, import, and export objects on hold.

Go to AvePoint Opus &gt; Manage Holds to view and manage the hold.

If you have any questions, please contact your Administrator.

Best Regards,

The Opus Team @ AvePoint
";
        /// <summary>
        /// need add upgrade template to this list
        /// </summary>
        private List<Guid> templateList = new List<Guid>()
        {
            LoanRequestEndUser_Template_Id,
            LoanRequestRM_Template_Id,
            LoanRequestApproved_Template_Id,
            LoanRequestReject_Template_Id,
            CreationRequestReject_Template_Id,
            CreationRequestApproved_Template_Id,
            NewCreationRequestRM_Template_Id,
            NewCreationRequestEndUser_Template_Id,
            ManualApproval_Template_Id,
            ML_ManualApproval_Template_Id,
            ExportZipPassword_Template_Id,
            JobNotification_Template_Id,
            BorrowerNotification_Template_Id,
            HoldNotification_Template_Id,
            HoldManagerNotification_Template_Id,
            MoveRequestEndUser_Template_Id,
            MoveRequestRM_Template_Id,
            MoveRequestApprovedEndUser_Template_Id,
            MoveRequestApprovedDestinationRM_Template_Id,
            MoveRequestReject_Template_Id
        };
        private RALogger logger = RALogger.GetInstance(typeof(RMEamilTemplateDao));

        public void InitDefaultData(RMDbContext context)
        {
            try
            {
                if (context.EmailTemplate.Count(item => templateList.Contains(item.UniqueId)) == templateList.Count)
                {
                    logger.Info($"The tenant: [{TenantLocalValue.LogonGroupId}] has init email template.");
                    return;
                }
                using (var ctx = context.Database.BeginTransaction())
                {
                    CheckBoxOrFileTemplate(context);
                    CheckFileOrRecordTemplate(context);
                    CheckManualApprovalTemplate(context);
                    CheckMLManualApprovalTemplate(context);
                    CheckJobNotificationTemplate(context);
                    CheckBorrowerNotificationTemplate(context);
                    CheckHoldNotificationTemplate(context);
                    CheckHoldManagerNotificationTemplate(context);
                    ctx.Commit();
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while init Email Template,error is:{0}", ex.ToString());
            }
        }

        public void InitDefaultData()
        {
            using (var context = GetNewContext())
            {
                CheckBoxOrFileTemplate(context);
                CheckFileOrRecordTemplate(context);
                CheckManualApprovalTemplate(context);
                CheckMLManualApprovalTemplate(context);
                CheckJobNotificationTemplate(context);
                CheckBorrowerNotificationTemplate(context);
                CheckHoldNotificationTemplate(context);
            }
        }

        private void CheckBoxOrFileTemplate(RMDbContext context)
        {
            if (!context.EmailTemplate.Any(a => a.UniqueId == LoanRequestEndUser_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = LoanRequestEndUser_Template_Id,
                    DisplayName = "RM_CP_Email_LoanRequestToEndUser",
                    Subject = "Loan Request: $Request.ID$ has been submitted",
                    CC = "",
                    Body = BodyForLoanEndUser,
                    Type = EmailTemplateType.BoxOrFile,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
            if (!context.EmailTemplate.Any(a => a.UniqueId == LoanRequestRM_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = LoanRequestRM_Template_Id,
                    DisplayName = "RM_CP_Email_LoanRequestToRM",
                    Subject = "Loan Request: $Request.ID$ has been submitted",
                    CC = "",
                    Body = BodyForLoanRM,
                    Type = EmailTemplateType.BoxOrFile,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
            if (!context.EmailTemplate.Any(a => a.UniqueId == LoanRequestApproved_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = LoanRequestApproved_Template_Id,
                    DisplayName = "RM_CP_Email_LoanRequestApprovedToEndUser",
                    Subject = "Loan Request: $Request.ID$ has been approved",
                    CC = "",
                    Body = BodyForLoanApproved,
                    Type = EmailTemplateType.BoxOrFile,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
            if (!context.EmailTemplate.Any(a => a.UniqueId == LoanRequestReject_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = LoanRequestReject_Template_Id,
                    DisplayName = "RM_CP_Email_LoanRequestRejectedToEndUser",
                    Subject = "Loan Request: $Request.ID$ cannot be completed",
                    CC = "",
                    Body = BodyForLoanReject,
                    Type = EmailTemplateType.BoxOrFile,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
            if (!context.EmailTemplate.Any(a => a.UniqueId == ExportZipPassword_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = ExportZipPassword_Template_Id,
                    DisplayName = "RM_CP_Email_ExportZipPasswordTemplate",
                    Subject = "Opus Information",
                    CC = "",
                    Body = ExportZipPasswordEmailBody,
                    Type = EmailTemplateType.ExportZipPasswordForReview,
                    IsNewTemplate = true,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
        }

        private void CheckFileOrRecordTemplate(RMDbContext context)
        {
            if (!context.EmailTemplate.Any(a => a.UniqueId == NewCreationRequestEndUser_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = NewCreationRequestEndUser_Template_Id,
                    DisplayName = "RM_CP_Email_CreationRequestToEndUser",
                    Subject = "Creation Request: $Request.ID$ has been submitted",
                    CC = "",
                    Body = BodyForCreationEndUser,
                    Type = EmailTemplateType.FileOrRecord,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
            if (!context.EmailTemplate.Any(a => a.UniqueId == NewCreationRequestRM_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = NewCreationRequestRM_Template_Id,
                    DisplayName = "RM_CP_Email_CreationRequestToRecordsManager",
                    Subject = "Creation Request: $Request.ID$ has been submitted",
                    CC = "",
                    Body = BodyForCreationRM,
                    Type = EmailTemplateType.FileOrRecord,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
            if (!context.EmailTemplate.Any(a => a.UniqueId == CreationRequestApproved_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = CreationRequestApproved_Template_Id,
                    DisplayName = "RM_CP_Email_CreationRequestApprovedToEndUser",
                    Subject = "Creation Request: $Request.ID$ has been approved",
                    CC = "",
                    Body = BodyForCreationApproved,
                    Type = EmailTemplateType.FileOrRecord,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
            if (!context.EmailTemplate.Any(a => a.UniqueId == CreationRequestReject_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = CreationRequestReject_Template_Id,
                    DisplayName = "RM_CP_Email_CreationRequestRejectedToEndUser",
                    Subject = "Creation Request: $Request.ID$ cannot be completed",
                    CC = "",
                    Body = BodyForCreationReject,
                    Type = EmailTemplateType.FileOrRecord,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
            if (!context.EmailTemplate.Any(a => a.UniqueId == MoveRequestEndUser_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = MoveRequestEndUser_Template_Id,
                    DisplayName = "RM_CP_Email_MoveRequestToEndUser",
                    Subject = "Move Request: $Request.ID$ has been submitted",
                    CC = "",
                    Body = BodyForMoveEndUser,
                    Type = EmailTemplateType.FileOrRecord,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
            if (!context.EmailTemplate.Any(a => a.UniqueId == MoveRequestRM_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = MoveRequestRM_Template_Id,
                    DisplayName = "RM_CP_Email_MoveRequestToRM",
                    Subject = "Move Request: $Request.ID$ has been submitted",
                    CC = "",
                    Body = BodyForMoveRM,
                    Type = EmailTemplateType.FileOrRecord,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
            if (!context.EmailTemplate.Any(a => a.UniqueId == MoveRequestApprovedEndUser_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = MoveRequestApprovedEndUser_Template_Id,
                    DisplayName = "RM_CP_Email_MoveRequestApprovedToEndUser",
                    Subject = "Move Request: $Request.ID$ has been approved",
                    CC = "",
                    Body = BodyForMoveApproved,
                    Type = EmailTemplateType.FileOrRecord,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
            if (!context.EmailTemplate.Any(a => a.UniqueId == MoveRequestReject_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = MoveRequestReject_Template_Id,
                    DisplayName = "RM_CP_Email_MoveRequestRejectedToEndUser",
                    Subject = "Move Request: $Request.ID$ cannot be completed",
                    CC = "",
                    Body = BodyForMoveReject,
                    Type = EmailTemplateType.FileOrRecord,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
            if (!context.EmailTemplate.Any(a => a.UniqueId == MoveRequestApprovedDestinationRM_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = MoveRequestApprovedDestinationRM_Template_Id,
                    DisplayName = "RM_CP_Email_MoveRequestApprovedToDesRM",
                    Subject = "List of physical items is approved to be moved to your location.",
                    CC = "",
                    Body = BodyForMoveApproveDesRM,
                    Type = EmailTemplateType.FileOrRecord,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
        }

        private void CheckManualApprovalTemplate(RMDbContext context)
        {
            if (!context.EmailTemplate.Any(a => a.UniqueId == ManualApproval_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = ManualApproval_Template_Id,
                    DisplayName = "RM_CP_Email_ManualApprovalForRecordsReviewer",
                    Subject = "New Records Pending Review",
                    CC = "",
                    Body = ManualApprovalEmailBody,
                    Type = EmailTemplateType.RecordsForReview,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
        }

        private void CheckMLManualApprovalTemplate(RMDbContext context)
        {
            if (!context.EmailTemplate.Any(a => a.UniqueId == ML_ManualApproval_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = ML_ManualApproval_Template_Id,
                    DisplayName = "RM_CP_Email_MLManualApprovalForRecordsReviewer",
                    Subject = "Smart Classification Review Task",
                    CC = "",
                    Body = MLManualApprovalEmailBody,
                    Type = EmailTemplateType.MLRecordsForReview,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
        }

        private void CheckJobNotificationTemplate(RMDbContext context)
        {
            if (!context.EmailTemplate.Any(a => a.UniqueId == JobNotification_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = JobNotification_Template_Id,
                    DisplayName = "RM_CP_Email_JobNotification",
                    Subject = "Job Notification",
                    CC = "",
                    Body = JobNotificationEmailBody,
                    Type = EmailTemplateType.JobNotification,
                    IsNewTemplate = true,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
        }
        private void CheckBorrowerNotificationTemplate(RMDbContext context)
        {
            if (!context.EmailTemplate.Any(a => a.UniqueId == BorrowerNotification_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = BorrowerNotification_Template_Id,
                    DisplayName = "RM_CP_Email_BorrowerNotification",
                    Subject = "Physical item <$Physical.ItemName$> is overdue for return.",
                    CC = "",
                    Body = BorrowerNotificationEmailBody,
                    Type = EmailTemplateType.BorrowerNotification,
                    IsNewTemplate = true,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
        }

        private void CheckHoldNotificationTemplate(RMDbContext context)
        {
            if (!context.EmailTemplate.Any(a => a.UniqueId == HoldNotification_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = HoldNotification_Template_Id,
                    DisplayName = "RM_CP_Email_HoldNotification",
                    Subject = "Hold Expiration Reminder.",
                    CC = "",
                    Body = HoldNotificationEmailBody,
                    Type = EmailTemplateType.HoldNotification,
                    IsNewTemplate = true,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
        }

        private void CheckHoldManagerNotificationTemplate(RMDbContext context)
        {
            if (!context.EmailTemplate.Any(a => a.UniqueId == HoldManagerNotification_Template_Id))
            {
                var entity = new RMEmailTemplate()
                {
                    UniqueId = HoldManagerNotification_Template_Id,
                    DisplayName = "RM_CP_Email_HoldManagerNotification",
                    Subject = "You have been assigned as a Hold Manager",
                    CC = "",
                    Body = HoldManagerNotificationEmailBody,
                    Type = EmailTemplateType.HoldManagerNotification,
                    IsNewTemplate = true,
                };
                context.EmailTemplate.Add(entity);
                context.SaveChanges();
            }
        }
        public List<RMEmailTemplate> GetAllEmailTemplate()
        {
            List<RMEmailTemplate> result = new List<RMEmailTemplate>();
            using (var ctx = GetNewContext())
            {
                result = ctx.EmailTemplate.AsNoTracking().ToList();
            }
            return result;
        }

        public RMEmailTemplate GetEmailTemplateById(int id)
        {
            RMEmailTemplate result = new RMEmailTemplate();
            using (var ctx = GetNewContext())
            {
                result = ctx.EmailTemplate.AsNoTracking().Where(t => t.Id == id).FirstOrDefault();
            }
            return result;
        }

		public RMEmailTemplate GetEmailTemplateByUniqueId(Guid UniqueId)
		{
			RMEmailTemplate result = new RMEmailTemplate();
			using (var ctx = GetNewContext())
			{
				result = ctx.EmailTemplate.AsNoTracking().Where(t => t.UniqueId == UniqueId).FirstOrDefault();
			}
			return result;
		}

        public async Task<RMEmailTemplate> GetEmailTemplate(Guid uniqueId)
        {
            using var context = GetNewContext();

            var res = context.EmailTemplate.AsNoTracking().First(item => item.UniqueId == uniqueId);

            return res;
        }

        public RMEmailTemplate GetEmailTemplateByInternalType(EmailTemplateInternalType type)
        {
            RMEmailTemplate result = new RMEmailTemplate();
            Guid uniqueId = new Guid();
            switch (type)
            {
                case EmailTemplateInternalType.LoanRequsetToEndUser:
                    uniqueId = LoanRequestEndUser_Template_Id;
                    break;
                case EmailTemplateInternalType.LoanRequsetToRM:
                    uniqueId = LoanRequestRM_Template_Id;
                    break;
                case EmailTemplateInternalType.LoanRequsetApproved:
                    uniqueId = LoanRequestApproved_Template_Id;
                    break;
                case EmailTemplateInternalType.LoanRequsetRejected:
                    uniqueId = LoanRequestReject_Template_Id;
                    break;
                case EmailTemplateInternalType.CreationRequestToEndUser:
                    uniqueId = NewCreationRequestEndUser_Template_Id;
                    break;
                case EmailTemplateInternalType.CreationRequestToRM:
                    uniqueId = NewCreationRequestRM_Template_Id;
                    break;
                case EmailTemplateInternalType.CreationRequestApproved:
                    uniqueId = CreationRequestApproved_Template_Id;
                    break;
                case EmailTemplateInternalType.CreationRequestRejected:
                    uniqueId = CreationRequestReject_Template_Id;
                    break;
                case EmailTemplateInternalType.MoveRequestToEndUser:
                    uniqueId = MoveRequestEndUser_Template_Id;
                    break;
                case EmailTemplateInternalType.MoveRequestToRM:
                    uniqueId = MoveRequestRM_Template_Id;
                    break;
                case EmailTemplateInternalType.MoveRequestApprovedToEndUser:
                    uniqueId = MoveRequestApprovedEndUser_Template_Id;
                    break;
                case EmailTemplateInternalType.MoveRequestRejected:
                    uniqueId = MoveRequestReject_Template_Id;
                    break;
                case EmailTemplateInternalType.MoveRequestApprovedToDestinationRM:
                    uniqueId = MoveRequestApprovedDestinationRM_Template_Id;
                    break;
                case EmailTemplateInternalType.WaitingApproval:
                    uniqueId = WaitingApproval_Template_Id;
                    break;
                case EmailTemplateInternalType.Approved:
                    uniqueId = Approvaled_Template_Id;
                    break;
                case EmailTemplateInternalType.Rejected:
                    uniqueId = Rejected_Template_Id;
                    break;
                case EmailTemplateInternalType.Escalated:
                    uniqueId = Escalated_Template_Id;
                    break;
                case EmailTemplateInternalType.ManualApproval:
                    uniqueId = ManualApproval_Template_Id;
                    break;
                case EmailTemplateInternalType.MLManualApproval:
                    uniqueId = ML_ManualApproval_Template_Id;
                    break;
                case EmailTemplateInternalType.ExportZipPassword:
                    uniqueId = ExportZipPassword_Template_Id;
                    break;
                case EmailTemplateInternalType.JobNotification:
                    uniqueId = JobNotification_Template_Id;
                    break;
                case EmailTemplateInternalType.BorrowerNotification:
                    uniqueId = BorrowerNotification_Template_Id;
                    break;
            }
            using (var ctx = GetNewContext())
            {
                result = ctx.EmailTemplate.AsNoTracking().Where(t => t.UniqueId == uniqueId).FirstOrDefault();
            }
            return result;
        }

        public bool UpdateEmailTemplate(int id, string body)
        {
            try
            {
                using var ctx = GetNewContext();

                var email = ctx.EmailTemplate.FirstOrDefault(item => item.Id == id);
                if (email != null)
                {
                    email.Body = body;
                    ctx.EmailTemplate.AddOrUpdate(email);
                    ctx.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while init Email Template,error is:{0}", ex.ToString());
                return false;
            }
        }

        public bool UpdateEmailTemplate(string name, int id, string subject, string cc, string body,int isUseDefaultFooter)
        {
            try
            {
				using var ctx = GetNewContext();

				var email = ctx.EmailTemplate.FirstOrDefault(item => item.Id == id);
                if(email != null) 
                {
                    if (email.IsCustomTemplate)
                    {
                        email.DisplayName = name;
                    }
                    email.Subject = subject;
                    email.CC = cc;
                    email.Body = body;
                    email.IsUseDefaultFooter = (DefaultFooterStatus)isUseDefaultFooter;
                    email.IsNewTemplate = true;
                    ctx.EmailTemplate.AddOrUpdate(email);
                    ctx.SaveChanges();
                    return true;
                }
                return false;
			}
            catch (Exception ex)
            {
				logger.Error("error occurred while init Email Template,error is:{0}", ex.ToString());
				return false;
            }
		}

		public bool CreateEmailTemplate(Guid UniqueId, string name, string subject, string cc, string body, int isUseDefaultFooter)
		{
            using var context = GetNewContext();

			var entity = new RMEmailTemplate()
			{
				UniqueId = UniqueId,
				DisplayName = name,
				Subject = subject,
				CC = cc,
				Body = body,
				Type = EmailTemplateType.RecordsForReview,
				IsUseDefaultFooter = (DefaultFooterStatus)isUseDefaultFooter,
				IsNewTemplate = true,
                IsCustomTemplate = true
		    };

            context.EmailTemplate.Add(entity);
            var res = context.SaveChangesAsync().GetAwaiter().GetResult();
			return res > 0;
		}

		public bool CheckTemplateNameExist(EmailTemplateDto template)
		{
			using var context = GetNewContext();
            List<RMEmailTemplate> checkedTemplateList = context.EmailTemplate.AsNoTracking().Where(item => !item.IsRemoved && template.Id != item.Id).ToList();
			var exist = checkedTemplateList.Any(item => template.Name.Equals(I18NEntity.GetString(item.DisplayName)));
			return exist;
		}

		public bool CheckTemplateUsed(Guid uniqueId)
		{
			using var context = GetNewContext();
            var isUsed = context.WorkflowStep.Any(item => item.UsedEmailTemplateId == uniqueId);
            return isUsed;
		}

		public bool DeleteEmailTemplate(Guid uniqueId)
		{
			using var context = GetNewContext();

			var currentEmailTemplate = context.EmailTemplate.AsQueryable().Where(item => item.UniqueId.Equals(uniqueId)).ToList();

			foreach (var item in currentEmailTemplate)
			{
				item.IsRemoved = true;
			}

			var res = context.SaveChangesAsync().GetAwaiter().GetResult();

			return res > 0;
		}

		public EmailTemplateDto GetCustomDefaultEmailTemplate(EmailTemplateInternalType type)
		{
			var emailTemplate = new EmailTemplateDto();
            switch (type)
            {
				case EmailTemplateInternalType.ManualApproval:
					emailTemplate.Name = "";
					emailTemplate.Subject = "New Records Pending Review";
					emailTemplate.CC = "";
					emailTemplate.Body = ManualApprovalEmailBody;
					emailTemplate.Type = (int)EmailTemplateType.RecordsForReview;
					emailTemplate.IsCustomTemplate = true;
					emailTemplate.UniqueId = Guid.NewGuid();
					break;
			}
			return emailTemplate;
		}

        public async Task<long> MultiGeoInsertEmailTemplateTableAsync(IEnumerable<RMEmailTemplate> emailTemplates)
        {
            using var context = GetNewContext();
            string tableName = "RMEmailTemplates";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new StringBuilder();
                var parameters = new List<SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, UniqueId, DisplayName, Subject, CC, Body, Type, IsUseDefaultFooter, IsNewTemplate, IsRemoved, IsCustomTemplate) VALUES ");
                int i = 0;
                foreach (var item in emailTemplates)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex +4}, @p{paramIndex + 5}, @p{paramIndex + 6}, @p{paramIndex + 7}, @p{paramIndex + 8}, @p{paramIndex + 9}, @p{paramIndex + 10})");

                    parameters.Add(new SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 1}", item.UniqueId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 2}", (object)item.DisplayName ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 3}", (object)item.Subject ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 4}", (object)item.CC ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 5}", (object)item.Body ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 6}", item.Type));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 7}", item.IsUseDefaultFooter));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 8}", item.IsNewTemplate));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 9}", item.IsRemoved));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 10}", item.IsCustomTemplate));
                    paramIndex += 11;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMEmailTemplates data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }
        public async Task<long> MultiGeoDeleteAllEmailTemplateAsync()
        {
            return await TruncateAllDataInTableAsync("RMEmailTemplates");
        }

        public async Task<IEnumerable<RMEmailTemplate>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.EmailTemplate.AsNoTracking().OrderBy(o => o.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

    }
}
