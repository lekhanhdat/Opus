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
	// Token: 0x02000021 RID: 33
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class ClientCallableAttribute : Attribute
	{
		// Token: 0x17000041 RID: 65
		// (get) Token: 0x0600016D RID: 365 RVA: 0x0000BD85 File Offset: 0x00009F85
		// (set) Token: 0x0600016E RID: 366 RVA: 0x0000BD8D File Offset: 0x00009F8D
		public string Name
		{
			get
			{
				return this.m_name;
			}
			set
			{
				this.m_name = value;
			}
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x0600016F RID: 367 RVA: 0x0000BD96 File Offset: 0x00009F96
		// (set) Token: 0x06000170 RID: 368 RVA: 0x0000BD9E File Offset: 0x00009F9E
		public ClientLibraryTargets ClientLibraryTargets
		{
			get
			{
				return this.m_clientTargets;
			}
			set
			{
				this.m_clientTargets = value;
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000171 RID: 369 RVA: 0x0000BDA7 File Offset: 0x00009FA7
		// (set) Token: 0x06000172 RID: 370 RVA: 0x0000BDAF File Offset: 0x00009FAF
		public bool Internal { get; set; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x06000173 RID: 371 RVA: 0x0000BDB8 File Offset: 0x00009FB8
		// (set) Token: 0x06000174 RID: 372 RVA: 0x0000BDC0 File Offset: 0x00009FC0
		public bool IsBeta
		{
			get
			{
				return this.m_isBeta;
			}
			set
			{
				this.m_isBeta = value;
			}
		}

		// Token: 0x04000053 RID: 83
		private string m_name;

		// Token: 0x04000054 RID: 84
		private ClientLibraryTargets m_clientTargets = ClientLibraryTargets.All;

		// Token: 0x04000055 RID: 85
		private bool m_isBeta;
	}
}
