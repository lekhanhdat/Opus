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
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Office.Project.Server.Interfaces
{
	// Token: 0x0200001D RID: 29
	[DataContract(Namespace = "http://Microsoft.Office.Project.Interfaces/")]
	public class ServerExecutionFault : DefaultServerFault
	{
		// Token: 0x060003C5 RID: 965 RVA: 0x00002209 File Offset: 0x00000409
		public ServerExecutionFault(XmlElement xmlExceptionDetails) : base(null)
		{
			this.xmlExceptionDetails = xmlExceptionDetails;
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x00002219 File Offset: 0x00000419
		public ServerExecutionFault(XmlNode xmlExceptionDetailsNode) : base(null)
		{
			this.xmlExceptionDetails = (XmlElement)xmlExceptionDetailsNode;
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x060003C7 RID: 967 RVA: 0x0000222E File Offset: 0x0000042E
		// (set) Token: 0x060003C8 RID: 968 RVA: 0x00002236 File Offset: 0x00000436
		[DataMember]
		public XmlElement ExceptionDetails
		{
			get
			{
				return this.xmlExceptionDetails;
			}
			set
			{
				this.xmlExceptionDetails = value;
			}
		}

		// Token: 0x04000007 RID: 7
		private XmlElement xmlExceptionDetails;
	}
}
