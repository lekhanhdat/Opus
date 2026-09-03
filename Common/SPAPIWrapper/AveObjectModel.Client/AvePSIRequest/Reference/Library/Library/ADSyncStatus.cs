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
	[Serializable]
	public enum ADSyncStatus : short
	{
		// Token: 0x0400484E RID: 18510
		NeverRun,
		// Token: 0x0400484F RID: 18511
		PartialFail,
		// Token: 0x04004850 RID: 18512
		Failed,
		// Token: 0x04004851 RID: 18513
		InProgress,
		// Token: 0x04004852 RID: 18514
		GroupNotFound,
		// Token: 0x04004853 RID: 18515
		Succeeded
	}
}
