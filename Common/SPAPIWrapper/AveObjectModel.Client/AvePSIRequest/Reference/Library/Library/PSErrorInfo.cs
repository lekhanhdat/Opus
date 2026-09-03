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
	// Token: 0x02000E6A RID: 3690
	[Serializable]
	public class PSErrorInfo
	{
		// Token: 0x06000F03 RID: 3843 RVA: 0x00057B38 File Offset: 0x00055D38
		internal PSErrorInfo(PSErrorID errId, Guid errUid, string errName, string[] errAttrNames, string[] errAttrs)
		{
			this.errId = errId;
			this.errUid = errUid;
			this.errName = errName;
			this.errAttrNames = errAttrNames;
			this.errAttrs = errAttrs;
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000F04 RID: 3844 RVA: 0x00057B65 File Offset: 0x00055D65
		public PSErrorID ErrId
		{
			get
			{
				return this.errId;
			}
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x06000F05 RID: 3845 RVA: 0x00057B6D File Offset: 0x00055D6D
		public Guid ErrUid
		{
			get
			{
				return this.errUid;
			}
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x06000F06 RID: 3846 RVA: 0x00057B75 File Offset: 0x00055D75
		public string ErrName
		{
			get
			{
				return this.errName;
			}
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x06000F07 RID: 3847 RVA: 0x00057B7D File Offset: 0x00055D7D
		public string[] ErrorAttributes
		{
			get
			{
				return this.errAttrs;
			}
		}

		// Token: 0x06000F08 RID: 3848 RVA: 0x00057B85 File Offset: 0x00055D85
		public string[] ErrorAttributeNames()
		{
			return (string[])this.errAttrNames.Clone();
		}

		// Token: 0x0400442C RID: 17452
		private PSErrorID errId;

		// Token: 0x0400442D RID: 17453
		private Guid errUid;

		// Token: 0x0400442E RID: 17454
		private string errName;

		// Token: 0x0400442F RID: 17455
		private string[] errAttrs;

		// Token: 0x04004430 RID: 17456
		private string[] errAttrNames;
	}
}
