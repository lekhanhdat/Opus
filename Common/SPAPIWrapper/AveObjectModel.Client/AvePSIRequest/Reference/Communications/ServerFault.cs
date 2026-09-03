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
using System.ServiceModel;

namespace Microsoft.Office.Project.Server.Interfaces
{
	// Token: 0x0200001B RID: 27
	[DataContract(Namespace = "http://Microsoft.Office.Project.Interfaces/")]
	public class ServerFault
	{
		public ServerFault()
		{
		}

		public ServerFault(Exception serverSideException)
		{
			if (serverSideException != null)
			{
				this.message = serverSideException.Message;
				this.stackTrace = serverSideException.StackTrace;
				if (null != serverSideException.TargetSite)
				{
					this.targetSite = serverSideException.TargetSite.ToString();
				}
				this.source = serverSideException.Source;
				OperationContext operationContext = OperationContext.Current;
				if (operationContext != null)
				{
					this.Actor = operationContext.RequestContext.RequestMessage.Headers.Action;
				}
			}
		}

		[DataMember]
		public string TargetSite
		{
			get
			{
				return this.targetSite;
			}
			set
			{
				this.targetSite = value;
			}
		}

		[DataMember]
		public string Source
		{
			get
			{
				return this.source;
			}
			set
			{
				this.source = value;
			}
		}

		[DataMember]
		public string Message
		{
			get
			{
				return this.message;
			}
			set
			{
				this.message = value;
			}
		}

		[DataMember]
		public string StackTrace
		{
			get
			{
				return this.stackTrace;
			}
			set
			{
				this.stackTrace = value;
			}
		}

		[DataMember]
		public string Actor { get; set; }

		[DataMember]
		public int LastError { get; set; }

		private string message = string.Empty;

		private string stackTrace = string.Empty;

		private string targetSite;

		private string source = string.Empty;
	}
}
