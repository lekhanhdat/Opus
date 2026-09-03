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
using Microsoft.SharePoint.Client;

namespace Microsoft.Office.Project.Server.Library
{
	// Token: 0x02000EA5 RID: 3749
	public struct TimesheetEnum
	{
		// Token: 0x02000EA6 RID: 3750
		public enum Process
		{
			// Token: 0x04004744 RID: 18244
			Approve,
			// Token: 0x04004745 RID: 18245
			Reject
		}

		// Token: 0x02000EA7 RID: 3751
		public enum LineStatus
		{
			// Token: 0x04004747 RID: 18247
			Pending,
			// Token: 0x04004748 RID: 18248
			Approved,
			// Token: 0x04004749 RID: 18249
			Rejected,
			// Token: 0x0400474A RID: 18250
			NotApplicable,
			// Token: 0x0400474B RID: 18251
			PendingApproval
		}

		// Token: 0x02000EA8 RID: 3752
		public enum Status
		{
			// Token: 0x0400474D RID: 18253
			InProgress,
			// Token: 0x0400474E RID: 18254
			Submitted,
			// Token: 0x0400474F RID: 18255
			Acceptable,
			// Token: 0x04004750 RID: 18256
			Approved,
			// Token: 0x04004751 RID: 18257
			Rejected,
			// Token: 0x04004752 RID: 18258
			PendingSubmit
		}

		// Token: 0x02000EA9 RID: 3753
		public enum QueueJobStatus
		{
			// Token: 0x04004754 RID: 18260
			Passed,
			// Token: 0x04004755 RID: 18261
			Pending,
			// Token: 0x04004756 RID: 18262
			Failed
		}

		// Token: 0x02000EAA RID: 3754
		public enum Navigation
		{
			// Token: 0x04004758 RID: 18264
			Current,
			// Token: 0x04004759 RID: 18265
			Previous,
			// Token: 0x0400475A RID: 18266
			Next
		}

		// Token: 0x02000EAB RID: 3755
		public enum Action
		{
			// Token: 0x0400475C RID: 18268
			Submit,
			// Token: 0x0400475D RID: 18269
			PendingApproval,
			// Token: 0x0400475E RID: 18270
			Approve,
			// Token: 0x0400475F RID: 18271
			Reject
		}

		// Token: 0x02000EAC RID: 3756
		public enum ActionState
		{
			// Token: 0x04004761 RID: 18273
			Current,
			// Token: 0x04004762 RID: 18274
			History,
			// Token: 0x04004763 RID: 18275
			All
		}

		// Token: 0x02000EAD RID: 3757
		public enum AuditResType
		{
			// Token: 0x04004765 RID: 18277
			Invalid,
			// Token: 0x04004766 RID: 18278
			Owner = 2,
			// Token: 0x04004767 RID: 18279
			Surrogate = 4,
			// Token: 0x04004768 RID: 18280
			FinalApprover = 8,
			// Token: 0x04004769 RID: 18281
			PreviousApprover = 16,
			// Token: 0x0400476A RID: 18282
			Accepter = 32,
			// Token: 0x0400476B RID: 18283
			Adjuster = 64,
			// Token: 0x0400476C RID: 18284
			FinalApproverAdjuster = 72,
			// Token: 0x0400476D RID: 18285
			AccepterAdjuster = 96,
			// Token: 0x0400476E RID: 18286
			Reviewer = 128
		}

		// Token: 0x02000EAE RID: 3758
		public enum AuditType
		{
			// Token: 0x04004770 RID: 18288
			ByResource,
			// Token: 0x04004771 RID: 18289
			ByAdjuster,
			// Token: 0x04004772 RID: 18290
			ByBoth
		}

		// Token: 0x02000EAF RID: 3759
		public enum AuditOperationType
		{
			// Token: 0x04004774 RID: 18292
			Delete,
			// Token: 0x04004775 RID: 18293
			Add
		}

		// Token: 0x02000EB0 RID: 3760
		public enum PreloadType
		{
			// Token: 0x04004777 RID: 18295
			None,
			// Token: 0x04004778 RID: 18296
			AdminTimes,
			// Token: 0x04004779 RID: 18297
			Projects,
			// Token: 0x0400477A RID: 18298
			AdminTimesAndProjects,
			// Token: 0x0400477B RID: 18299
			Assignments,
			// Token: 0x0400477C RID: 18300
			AdminTimesAndAssignments,
			// Token: 0x0400477D RID: 18301
			ProjectsAndAssignments,
			// Token: 0x0400477E RID: 18302
			All,
			// Token: 0x0400477F RID: 18303
			Default
		}

		// Token: 0x02000EB1 RID: 3761
		public enum ProjectTimesheetLineQueryType
		{
			// Token: 0x04004781 RID: 18305
			StatusManagerLineItems,
			// Token: 0x04004782 RID: 18306
			ApprovedLineItems,
			// Token: 0x04004783 RID: 18307
			AllApprovedLineItems
		}

		// Token: 0x02000EB2 RID: 3762
		public enum EntryMode
		{
			// Token: 0x04004785 RID: 18309
			Daily,
			// Token: 0x04004786 RID: 18310
			Weekly
		}

		// Token: 0x02000EB3 RID: 3763
		[ClientCallableType(ServerTypeId = "AD1936DB-E776-4018-B186-B41428D31BF1", Name = "TimeSheetValidationType")]
		public enum ValidationType
		{
			// Token: 0x04004788 RID: 18312
			Unverified,
			// Token: 0x04004789 RID: 18313
			Verified,
			// Token: 0x0400478A RID: 18314
			ProjectLevel
		}

		// Token: 0x02000EB4 RID: 3764
		public enum LineClassState
		{
			// Token: 0x0400478C RID: 18316
			Enabled,
			// Token: 0x0400478D RID: 18317
			Disabled,
			// Token: 0x0400478E RID: 18318
			All
		}

		// Token: 0x02000EB5 RID: 3765
		public enum LineClassType
		{
			// Token: 0x04004790 RID: 18320
			Regular,
			// Token: 0x04004791 RID: 18321
			NonWork,
			// Token: 0x04004792 RID: 18322
			NonProject,
			// Token: 0x04004793 RID: 18323
			AllNonProject,
			// Token: 0x04004794 RID: 18324
			All
		}

		// Token: 0x02000EB6 RID: 3766
		public enum PeriodState
		{
			// Token: 0x04004796 RID: 18326
			Open,
			// Token: 0x04004797 RID: 18327
			Closed,
			// Token: 0x04004798 RID: 18328
			All
		}

		// Token: 0x02000EB7 RID: 3767
		public enum ListSelect
		{
			// Token: 0x0400479A RID: 18330
			InProgress = 1,
			// Token: 0x0400479B RID: 18331
			Submitted,
			// Token: 0x0400479C RID: 18332
			Acceptable = 4,
			// Token: 0x0400479D RID: 18333
			Approved = 8,
			// Token: 0x0400479E RID: 18334
			Rejected = 16,
			// Token: 0x0400479F RID: 18335
			AllExisting = 31,
			// Token: 0x040047A0 RID: 18336
			AllPeriods,
			// Token: 0x040047A1 RID: 18337
			CreatedByMe = 64,
			// Token: 0x040047A2 RID: 18338
			PendingSubmit = 128
		}

		// Token: 0x02000EB8 RID: 3768
		public enum TimeSheetDefaultDisplay : byte
		{
			// Token: 0x040047A4 RID: 18340
			DoNotUseStandardOverTimeAndNonBillable,
			// Token: 0x040047A5 RID: 18341
			UseStandardOverTimeAndNonBillable = 7
		}

		// Token: 0x02000EB9 RID: 3769
		public enum PreloadOptionsForTimeSheet : byte
		{
			// Token: 0x040047A7 RID: 18343
			PreloadAdminTimes,
			// Token: 0x040047A8 RID: 18344
			PreloadAdminTimesAndAssignments,
			// Token: 0x040047A9 RID: 18345
			PreloadAdminTimesAndProjects
		}

		// Token: 0x02000EBA RID: 3770
		public enum WorkReportingUnits : byte
		{
			// Token: 0x040047AB RID: 18347
			Hours,
			// Token: 0x040047AC RID: 18348
			Days
		}

		// Token: 0x02000EBB RID: 3771
		public enum DefaultDataEntryMode : byte
		{
			// Token: 0x040047AE RID: 18350
			Days,
			// Token: 0x040047AF RID: 18351
			Weeks
		}
	}
}
