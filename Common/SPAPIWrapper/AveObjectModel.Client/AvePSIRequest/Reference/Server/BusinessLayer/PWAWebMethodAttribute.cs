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
	// Token: 0x020001E6 RID: 486
	internal sealed class PWAWebMethodAttribute : ProjectWebMethodAttribute
	{
		// Token: 0x06000E95 RID: 3733 RVA: 0x00089BCA File Offset: 0x00087DCA
		public PWAWebMethodAttribute()
		{
		}

		// Token: 0x06000E96 RID: 3734 RVA: 0x00089BD2 File Offset: 0x00087DD2
		public PWAWebMethodAttribute(string Description) : base(Description)
		{
		}

		// Token: 0x06000E97 RID: 3735 RVA: 0x00089BDB File Offset: 0x00087DDB
		public PWAWebMethodAttribute(string Description, string SecurityPermission) : base(Description, SecurityPermission)
		{
		}

		// Token: 0x06000E98 RID: 3736 RVA: 0x00089BE5 File Offset: 0x00087DE5
		public PWAWebMethodAttribute(string Description, bool AnyPermissionAllows, params string[] SecurityPermissions) : base(Description, AnyPermissionAllows, SecurityPermissions)
		{
		}
	}
}
