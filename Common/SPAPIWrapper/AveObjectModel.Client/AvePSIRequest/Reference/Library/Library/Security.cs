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

namespace Microsoft.Office.Project.Server.Library
{
	// Token: 0x02000053 RID: 83
	public class Security
	{
		// Token: 0x04000631 RID: 1585
		[Obsolete("PROPOSAL_APPROVERS_GROUP_UID group uid has been deprecated")]
		public static readonly Guid PROPOSAL_APPROVERS_GROUP_UID = new Guid("87983F58-F88A-4FBC-95ED-9B65D55EC388");

		// Token: 0x02000054 RID: 84
		public enum PermissionID
		{
			// Token: 0x04000633 RID: 1587
			None,
			// Token: 0x04000634 RID: 1588
			Read,
			// Token: 0x04000635 RID: 1589
			ReadWrite,
			// Token: 0x04000636 RID: 1590
			ReadOnly,
			// Token: 0x04000637 RID: 1591
			Deny
		}

		// Token: 0x02000055 RID: 85
		public enum FieldAccessID
		{
			// Token: 0x04000639 RID: 1593
			NoAccess,
			// Token: 0x0400063A RID: 1594
			ReadAccess,
			// Token: 0x0400063B RID: 1595
			ReadWriteAccess,
			// Token: 0x0400063C RID: 1596
			DenyAccess
		}

		// Token: 0x02000056 RID: 86
		public enum FieldGroupID
		{
			// Token: 0x0400063E RID: 1598
			NormalField,
			// Token: 0x0400063F RID: 1599
			CostField,
			// Token: 0x04000640 RID: 1600
			BaselineField,
			// Token: 0x04000641 RID: 1601
			BaselineCostField
		}

		// Token: 0x02000057 RID: 87
		public enum SecurityPrincipalType
		{
			// Token: 0x04000643 RID: 1603
			User,
			// Token: 0x04000644 RID: 1604
			Group
		}

		// Token: 0x02000058 RID: 88
		public enum PermissionMode
		{
			// Token: 0x04000646 RID: 1606
			SharePoint = 1,
			// Token: 0x04000647 RID: 1607
			ProjectServer,
			// Token: 0x04000648 RID: 1608
			UninitializedSharePoint = 101,
			// Token: 0x04000649 RID: 1609
			UninitializedProjectServer
		}

		// Token: 0x02000059 RID: 89
		internal enum EffectiveRightsPermissionType
		{
			// Token: 0x0400064B RID: 1611
			Global,
			// Token: 0x0400064C RID: 1612
			Project,
			// Token: 0x0400064D RID: 1613
			Resource,
			// Token: 0x0400064E RID: 1614
			View
		}

		// Token: 0x0200005A RID: 90
		internal enum EffectiveRightMask
		{
			// Token: 0x04000650 RID: 1616
			Allow = 2,
			// Token: 0x04000651 RID: 1617
			Deny = 1,
			// Token: 0x04000652 RID: 1618
			Unknown = 0
		}
	}
}
