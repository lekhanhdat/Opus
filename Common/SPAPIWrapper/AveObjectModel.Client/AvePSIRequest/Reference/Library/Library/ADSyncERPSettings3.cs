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
using System.Linq;

namespace Microsoft.Office.Project.Server.Library
{
	[Serializable]
	public class ADSyncERPSettings3
	{
		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000FDB RID: 4059 RVA: 0x0005B2A2 File Offset: 0x000594A2
		// (set) Token: 0x06000FDC RID: 4060 RVA: 0x0005B2E0 File Offset: 0x000594E0
		public Guid[] ADGroupGuids
		{
			get
			{
				if (this._groupGuids != null)
				{
					return (from g in this._groupGuids
					where g != Guid.Empty
					select g).ToArray<Guid>();
				}
				return new Guid[0];
			}
			set
			{
				this._groupGuids = value;
			}
		}

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x06000FDD RID: 4061 RVA: 0x0005B2E9 File Offset: 0x000594E9
		// (set) Token: 0x06000FDE RID: 4062 RVA: 0x0005B2F1 File Offset: 0x000594F1
		public bool ScheduledUpdates
		{
			get
			{
				return this._scheduledUpdates;
			}
			set
			{
				this._scheduledUpdates = value;
			}
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x06000FDF RID: 4063 RVA: 0x0005B2FA File Offset: 0x000594FA
		// (set) Token: 0x06000FE0 RID: 4064 RVA: 0x0005B302 File Offset: 0x00059502
		public ADSyncStatus Status
		{
			get
			{
				return this._lastStatus;
			}
			set
			{
				this._lastStatus = value;
			}
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000FE1 RID: 4065 RVA: 0x0005B30B File Offset: 0x0005950B
		// (set) Token: 0x06000FE2 RID: 4066 RVA: 0x0005B313 File Offset: 0x00059513
		public DateTime LastUpdateTime
		{
			get
			{
				return this._lastUpdateTime;
			}
			set
			{
				this._lastUpdateTime = value;
			}
		}

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000FE3 RID: 4067 RVA: 0x0005B31C File Offset: 0x0005951C
		// (set) Token: 0x06000FE4 RID: 4068 RVA: 0x0005B324 File Offset: 0x00059524
		public bool AutoReactivateInactiveUsers
		{
			get
			{
				return this._autoReactivateInactiveUsers;
			}
			set
			{
				this._autoReactivateInactiveUsers = value;
			}
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000FE5 RID: 4069 RVA: 0x0005B32D File Offset: 0x0005952D
		// (set) Token: 0x06000FE6 RID: 4070 RVA: 0x0005B335 File Offset: 0x00059535
		internal string OnlineTenantDistinguishedName
		{
			get
			{
				return this._onlineTenantDistinguishedName;
			}
			set
			{
				this._onlineTenantDistinguishedName = value;
			}
		}

		// Token: 0x04004845 RID: 18501
		internal const int MaximumGroupsCount = 5;

		// Token: 0x04004846 RID: 18502
		private Guid[] _groupGuids;

		// Token: 0x04004847 RID: 18503
		private bool _scheduledUpdates;

		// Token: 0x04004848 RID: 18504
		private ADSyncStatus _lastStatus;

		// Token: 0x04004849 RID: 18505
		private DateTime _lastUpdateTime;

		// Token: 0x0400484A RID: 18506
		private bool _autoReactivateInactiveUsers;

		// Token: 0x0400484B RID: 18507
		[NonSerialized]
		private string _onlineTenantDistinguishedName;
	}
}
