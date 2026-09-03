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
	// Token: 0x02000022 RID: 34
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false)]
	public class ClientCallableTypeAttribute : ClientCallableAttribute
	{
		// Token: 0x17000045 RID: 69
		// (get) Token: 0x06000176 RID: 374 RVA: 0x0000BDDC File Offset: 0x00009FDC
		// (set) Token: 0x06000177 RID: 375 RVA: 0x0000BDE4 File Offset: 0x00009FE4
		public bool ValueObject
		{
			get
			{
				return this.m_valueObject;
			}
			set
			{
				this.m_valueObject = value;
			}
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000178 RID: 376 RVA: 0x0000BDED File Offset: 0x00009FED
		// (set) Token: 0x06000179 RID: 377 RVA: 0x0000BDF5 File Offset: 0x00009FF5
		public string ObjectIdentityPropertyName
		{
			get
			{
				return this.m_objectIdentityPropertyName;
			}
			set
			{
				this.m_objectIdentityPropertyName = value;
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x0600017A RID: 378 RVA: 0x0000BDFE File Offset: 0x00009FFE
		// (set) Token: 0x0600017B RID: 379 RVA: 0x0000BE06 File Offset: 0x0000A006
		public string ObjectUrlPathPropertyName { get; set; }

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x0600017C RID: 380 RVA: 0x0000BE0F File Offset: 0x0000A00F
		// (set) Token: 0x0600017D RID: 381 RVA: 0x0000BE17 File Offset: 0x0000A017
		public Type FactoryType
		{
			get
			{
				return this.m_factoryType;
			}
			set
			{
				this.m_factoryType = value;
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x0600017E RID: 382 RVA: 0x0000BE20 File Offset: 0x0000A020
		// (set) Token: 0x0600017F RID: 383 RVA: 0x0000BE28 File Offset: 0x0000A028
		public string ObjectVersionPropertyName
		{
			get
			{
				return this.m_objectVersionPropertyName;
			}
			set
			{
				this.m_objectVersionPropertyName = value;
			}
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000180 RID: 384 RVA: 0x0000BE31 File Offset: 0x0000A031
		// (set) Token: 0x06000181 RID: 385 RVA: 0x0000BE39 File Offset: 0x0000A039
		public string ServerTypeId
		{
			get
			{
				return this.m_serverTypeId;
			}
			set
			{
				this.m_serverTypeId = value;
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000182 RID: 386 RVA: 0x0000BE42 File Offset: 0x0000A042
		// (set) Token: 0x06000183 RID: 387 RVA: 0x0000BE4A File Offset: 0x0000A04A
		public Type CollectionChildItemType
		{
			get
			{
				return this.m_collectionChildItemType;
			}
			set
			{
				this.m_collectionChildItemType = value;
			}
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x06000184 RID: 388 RVA: 0x0000BE53 File Offset: 0x0000A053
		// (set) Token: 0x06000185 RID: 389 RVA: 0x0000BE5B File Offset: 0x0000A05B
		public string CollectionIndexerMethodClientName { get; set; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x06000186 RID: 390 RVA: 0x0000BE64 File Offset: 0x0000A064
		// (set) Token: 0x06000187 RID: 391 RVA: 0x0000BE6C File Offset: 0x0000A06C
		public string CollectionCreateEntityMethodClientName { get; set; }

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x06000188 RID: 392 RVA: 0x0000BE75 File Offset: 0x0000A075
		// (set) Token: 0x06000189 RID: 393 RVA: 0x0000BE7D File Offset: 0x0000A07D
		public string PutUpdateMethodClientName { get; set; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x0600018A RID: 394 RVA: 0x0000BE86 File Offset: 0x0000A086
		// (set) Token: 0x0600018B RID: 395 RVA: 0x0000BE8E File Offset: 0x0000A08E
		public string PatchUpdateMethodClientName { get; set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x0600018C RID: 396 RVA: 0x0000BE97 File Offset: 0x0000A097
		// (set) Token: 0x0600018D RID: 397 RVA: 0x0000BE9F File Offset: 0x0000A09F
		public string DeleteMethodClientName { get; set; }

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x0600018E RID: 398 RVA: 0x0000BEA8 File Offset: 0x0000A0A8
		// (set) Token: 0x0600018F RID: 399 RVA: 0x0000BEB0 File Offset: 0x0000A0B0
		public string ReadStreamMethodClientName { get; set; }

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000190 RID: 400 RVA: 0x0000BEB9 File Offset: 0x0000A0B9
		// (set) Token: 0x06000191 RID: 401 RVA: 0x0000BEC1 File Offset: 0x0000A0C1
		public string WriteStreamMethodClientName { get; set; }

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000192 RID: 402 RVA: 0x0000BECA File Offset: 0x0000A0CA
		// (set) Token: 0x06000193 RID: 403 RVA: 0x0000BED2 File Offset: 0x0000A0D2
		public string EntityKeyPropertyNames { get; set; }

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000194 RID: 404 RVA: 0x0000BEDB File Offset: 0x0000A0DB
		// (set) Token: 0x06000195 RID: 405 RVA: 0x0000BEE3 File Offset: 0x0000A0E3
		public string ETagPropertyName { get; set; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000196 RID: 406 RVA: 0x0000BEEC File Offset: 0x0000A0EC
		// (set) Token: 0x06000197 RID: 407 RVA: 0x0000BEF4 File Offset: 0x0000A0F4
		public string ChildItemsName { get; set; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x06000198 RID: 408 RVA: 0x0000BEFD File Offset: 0x0000A0FD
		// (set) Token: 0x06000199 RID: 409 RVA: 0x0000BF05 File Offset: 0x0000A105
		public string OnChildItemEnumerated { get; set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x0600019A RID: 410 RVA: 0x0000BF0E File Offset: 0x0000A10E
		// (set) Token: 0x0600019B RID: 411 RVA: 0x0000BF16 File Offset: 0x0000A116
		public string ScriptClientNamespace
		{
			get
			{
				return this.m_scriptClientNamespace;
			}
			set
			{
				this.m_scriptClientNamespace = value;
			}
		}

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x0600019C RID: 412 RVA: 0x0000BF1F File Offset: 0x0000A11F
		// (set) Token: 0x0600019D RID: 413 RVA: 0x0000BF27 File Offset: 0x0000A127
		public string ManagedClientNamespace
		{
			get
			{
				return this.m_managedClientNamespace;
			}
			set
			{
				this.m_managedClientNamespace = value;
			}
		}

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x0600019E RID: 414 RVA: 0x0000BF30 File Offset: 0x0000A130
		// (set) Token: 0x0600019F RID: 415 RVA: 0x0000BF38 File Offset: 0x0000A138
		public string ServerStubNamespace { get; set; }

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x060001A0 RID: 416 RVA: 0x0000BF41 File Offset: 0x0000A141
		// (set) Token: 0x060001A1 RID: 417 RVA: 0x0000BF49 File Offset: 0x0000A149
		public bool PublicServerStub { get; set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x060001A2 RID: 418 RVA: 0x0000BF52 File Offset: 0x0000A152
		// (set) Token: 0x060001A3 RID: 419 RVA: 0x0000BF5A File Offset: 0x0000A15A
		public string ExpandoFieldsPropertyName
		{
			get
			{
				return this.m_expandoFieldsPropertyName;
			}
			set
			{
				this.m_expandoFieldsPropertyName = value;
			}
		}

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x060001A4 RID: 420 RVA: 0x0000BF63 File Offset: 0x0000A163
		// (set) Token: 0x060001A5 RID: 421 RVA: 0x0000BF6B File Offset: 0x0000A16B
		public string GetExpandoFieldValueMethodName
		{
			get
			{
				return this.m_getExpandoFieldValueMethodName;
			}
			set
			{
				this.m_getExpandoFieldValueMethodName = value;
			}
		}

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x060001A6 RID: 422 RVA: 0x0000BF74 File Offset: 0x0000A174
		// (set) Token: 0x060001A7 RID: 423 RVA: 0x0000BF7C File Offset: 0x0000A17C
		public string OnQueryingMethodName
		{
			get
			{
				return this.m_onQueryingMethodName;
			}
			set
			{
				this.m_onQueryingMethodName = value;
			}
		}

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060001A8 RID: 424 RVA: 0x0000BF85 File Offset: 0x0000A185
		// (set) Token: 0x060001A9 RID: 425 RVA: 0x0000BF8D File Offset: 0x0000A18D
		public string OnRESTfulQueryingMethodName { get; set; }

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060001AA RID: 426 RVA: 0x0000BF96 File Offset: 0x0000A196
		// (set) Token: 0x060001AB RID: 427 RVA: 0x0000BF9E File Offset: 0x0000A19E
		public string RESTfulQueryResultMethodName { get; set; }

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x060001AC RID: 428 RVA: 0x0000BFA7 File Offset: 0x0000A1A7
		// (set) Token: 0x060001AD RID: 429 RVA: 0x0000BFAF File Offset: 0x0000A1AF
		public string TypeAlias { get; set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x060001AE RID: 430 RVA: 0x0000BFB8 File Offset: 0x0000A1B8
		// (set) Token: 0x060001AF RID: 431 RVA: 0x0000BFC0 File Offset: 0x0000A1C0
		public string SampleUrl { get; set; }

		// Token: 0x04000057 RID: 87
		private bool m_valueObject;

		// Token: 0x04000058 RID: 88
		private string m_objectIdentityPropertyName;

		// Token: 0x04000059 RID: 89
		private Type m_factoryType;

		// Token: 0x0400005A RID: 90
		private string m_objectVersionPropertyName;

		// Token: 0x0400005B RID: 91
		private string m_serverTypeId;

		// Token: 0x0400005C RID: 92
		private Type m_collectionChildItemType;

		// Token: 0x0400005D RID: 93
		private string m_scriptClientNamespace;

		// Token: 0x0400005E RID: 94
		private string m_managedClientNamespace;

		// Token: 0x0400005F RID: 95
		private string m_expandoFieldsPropertyName;

		// Token: 0x04000060 RID: 96
		private string m_getExpandoFieldValueMethodName;

		// Token: 0x04000061 RID: 97
		private string m_onQueryingMethodName;
	}
}
