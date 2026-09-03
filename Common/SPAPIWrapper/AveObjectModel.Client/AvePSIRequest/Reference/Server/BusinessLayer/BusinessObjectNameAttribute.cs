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
	// Token: 0x020001E4 RID: 484
	internal sealed class BusinessObjectNameAttribute : Attribute
	{
		// Token: 0x06000E8C RID: 3724 RVA: 0x00089B58 File Offset: 0x00087D58
		public BusinessObjectNameAttribute(string objectname)
		{
			this._objectname = objectname;
			this._methodname = string.Empty;
		}

		// Token: 0x06000E8D RID: 3725 RVA: 0x00089B72 File Offset: 0x00087D72
		public BusinessObjectNameAttribute(string objectname, string methodname)
		{
			this._objectname = objectname;
			this._methodname = methodname;
		}

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x06000E8E RID: 3726 RVA: 0x00089B88 File Offset: 0x00087D88
		// (set) Token: 0x06000E8F RID: 3727 RVA: 0x00089B90 File Offset: 0x00087D90
		public string ObjectName
		{
			get
			{
				return this._objectname;
			}
			set
			{
				this._objectname = value;
			}
		}

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x06000E90 RID: 3728 RVA: 0x00089B99 File Offset: 0x00087D99
		// (set) Token: 0x06000E91 RID: 3729 RVA: 0x00089BA1 File Offset: 0x00087DA1
		public string MethodName
		{
			get
			{
				return this._methodname;
			}
			set
			{
				this._methodname = value;
			}
		}

		// Token: 0x0400058A RID: 1418
		private string _objectname;

		// Token: 0x0400058B RID: 1419
		private string _methodname;
	}
}
