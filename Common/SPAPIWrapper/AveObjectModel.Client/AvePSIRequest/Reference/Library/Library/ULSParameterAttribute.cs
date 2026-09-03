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
	// Token: 0x02000EE8 RID: 3816
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
	internal sealed class ULSParameterAttribute : Attribute
	{
		// Token: 0x06001021 RID: 4129 RVA: 0x0005C565 File Offset: 0x0005A765
		public ULSParameterAttribute(int order, string paramName)
		{
			this._order = order;
			this._paramName = paramName;
		}

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x06001022 RID: 4130 RVA: 0x0005C57B File Offset: 0x0005A77B
		public int Order
		{
			get
			{
				return this._order;
			}
		}

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x06001023 RID: 4131 RVA: 0x0005C583 File Offset: 0x0005A783
		public string ParamName
		{
			get
			{
				return this._paramName;
			}
		}

		// Token: 0x040048AF RID: 18607
		private int _order;

		// Token: 0x040048B0 RID: 18608
		private string _paramName;
	}
}
