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

namespace Microsoft.Office.Project.Server.BusinessLayer
{
	// Token: 0x020001E3 RID: 483
	internal abstract class ProjectWebMethodAttribute : Attribute
	{
		// Token: 0x06000E7C RID: 3708 RVA: 0x00089A84 File Offset: 0x00087C84
		public ProjectWebMethodAttribute() : this(string.Empty, false, new string[0])
		{
		}

		// Token: 0x06000E7D RID: 3709 RVA: 0x00089A98 File Offset: 0x00087C98
		public ProjectWebMethodAttribute(string Description) : this(Description, false, new string[0])
		{
		}

		// Token: 0x06000E7E RID: 3710 RVA: 0x00089AA8 File Offset: 0x00087CA8
		public ProjectWebMethodAttribute(string Description, string SecurityPermission) : this(Description, false, new string[]
		{
			SecurityPermission
		})
		{
		}

		// Token: 0x06000E7F RID: 3711 RVA: 0x00089AC9 File Offset: 0x00087CC9
		public ProjectWebMethodAttribute(string Description, bool AnyPermissionAllows, params string[] SecurityPermissions)
		{
			this._description = Description;
			this._anyPermissionAllows = AnyPermissionAllows;
			this._securityPermissions = SecurityPermissions;
			this._inProcPermissions = new string[0];
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x06000E80 RID: 3712 RVA: 0x00089AF2 File Offset: 0x00087CF2
		// (set) Token: 0x06000E81 RID: 3713 RVA: 0x00089AFA File Offset: 0x00087CFA
		public string ExternalMethodName
		{
			get
			{
				return this._externalMethodName;
			}
			set
			{
				this._externalMethodName = value;
			}
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x06000E82 RID: 3714 RVA: 0x00089B03 File Offset: 0x00087D03
		// (set) Token: 0x06000E83 RID: 3715 RVA: 0x00089B0B File Offset: 0x00087D0B
		public string Description
		{
			get
			{
				return this._description;
			}
			set
			{
				this._description = value;
			}
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x06000E84 RID: 3716 RVA: 0x00089B14 File Offset: 0x00087D14
		// (set) Token: 0x06000E85 RID: 3717 RVA: 0x00089B1C File Offset: 0x00087D1C
		public bool AnyPermissionAllows
		{
			get
			{
				return this._anyPermissionAllows;
			}
			set
			{
				this._anyPermissionAllows = value;
			}
		}

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x06000E86 RID: 3718 RVA: 0x00089B25 File Offset: 0x00087D25
		// (set) Token: 0x06000E87 RID: 3719 RVA: 0x00089B2D File Offset: 0x00087D2D
		public string[] SecurityPermissions
		{
			get
			{
				return this._securityPermissions;
			}
			set
			{
				this._securityPermissions = value;
			}
		}

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x06000E88 RID: 3720 RVA: 0x00089B36 File Offset: 0x00087D36
		// (set) Token: 0x06000E89 RID: 3721 RVA: 0x00089B3E File Offset: 0x00087D3E
		public string[] InProcPermissions
		{
			get
			{
				return this._inProcPermissions;
			}
			set
			{
				this._inProcPermissions = value;
			}
		}

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x06000E8A RID: 3722 RVA: 0x00089B47 File Offset: 0x00087D47
		// (set) Token: 0x06000E8B RID: 3723 RVA: 0x00089B4F File Offset: 0x00087D4F
		public bool IsReadOnlySafeMethod
		{
			get
			{
				return this._isReadOnlySafeMethod;
			}
			set
			{
				this._isReadOnlySafeMethod = value;
			}
		}

		// Token: 0x04000584 RID: 1412
		private string _description;

		// Token: 0x04000585 RID: 1413
		private string[] _securityPermissions;

		// Token: 0x04000586 RID: 1414
		private string[] _inProcPermissions;

		// Token: 0x04000587 RID: 1415
		private bool _anyPermissionAllows;

		// Token: 0x04000588 RID: 1416
		private string _externalMethodName;

		// Token: 0x04000589 RID: 1417
		private bool _isReadOnlySafeMethod;
	}
}
