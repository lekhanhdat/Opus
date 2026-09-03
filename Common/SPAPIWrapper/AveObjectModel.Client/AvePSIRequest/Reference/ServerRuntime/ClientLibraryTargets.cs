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

namespace Microsoft.SharePoint.Client
{
	// Token: 0x02000034 RID: 52
	[Flags]
	public enum ClientLibraryTargets
	{
		// Token: 0x04000150 RID: 336
		None = 0,
		// Token: 0x04000151 RID: 337
		DotNetFramework = 1,
		// Token: 0x04000152 RID: 338
		JavaScript = 2,
		// Token: 0x04000153 RID: 339
		Silverlight = 4,
		// Token: 0x04000154 RID: 340
		RESTful = 8,
		// Token: 0x04000155 RID: 341
		IntrinsicRESTful = 16,
		// Token: 0x04000156 RID: 342
		All = 268435455,
		// Token: 0x04000157 RID: 343
		NonRESTful = 268435431,
		// Token: 0x04000158 RID: 344
		NonJavaScript = 268435453
	}
}
