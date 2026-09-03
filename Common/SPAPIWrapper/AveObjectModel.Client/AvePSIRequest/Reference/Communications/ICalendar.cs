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
using System.ServiceModel;
using Microsoft.Office.Project.Server.Schema;

namespace Microsoft.Office.Project.Server.Interfaces
{
	// Token: 0x02000004 RID: 4
	[ServiceContract(Namespace = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/", Name = "Calendar")]
	[XmlSerializerFormat(Style = OperationFormatStyle.Document)]
	public interface ICalendar
	{
		// Token: 0x06000044 RID: 68
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/CheckOutCalendars", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/CheckOutCalendarsResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		void CheckOutCalendars(Guid[] array);

		// Token: 0x06000045 RID: 69
		[FaultContract(typeof(ServerExecutionFault))]
		[FaultContract(typeof(DefaultServerFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/CheckInCalendars", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/CheckInCalendarsResponse")]
		void CheckInCalendars(Guid[] array, bool force);

		// Token: 0x06000046 RID: 70
		[FaultContract(typeof(ServerExecutionFault))]
		[FaultContract(typeof(DefaultServerFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/CreateCalendars", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/CreateCalendarsResponse")]
		void CreateCalendars(CalendarDataSet calendarDataSet, bool validateOnly, bool autoCheckIn);

		// Token: 0x06000047 RID: 71
		[FaultContract(typeof(DefaultServerFault))]
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/DeleteCalendars", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/DeleteCalendarsResponse")]
		void DeleteCalendars(Guid[] calendarGuids);

		// Token: 0x06000048 RID: 72
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/ListCalendars", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/ListCalendarsResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		[FaultContract(typeof(ServerExecutionFault))]
		CalendarDataSet ListCalendars();

		// Token: 0x06000049 RID: 73
		[FaultContract(typeof(ServerExecutionFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/ReadCalendars", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/ReadCalendarsResponse")]
		[FaultContract(typeof(DefaultServerFault))]
		CalendarDataSet ReadCalendars(string filter, bool autoCheckOut);

		// Token: 0x0600004A RID: 74
		[FaultContract(typeof(DefaultServerFault))]
		[OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/UpdateCalendars", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Calendar/UpdateCalendarsResponse")]
		[FaultContract(typeof(ServerExecutionFault))]
		void UpdateCalendars(CalendarDataSet calendarDataSet, bool validateOnly, bool autoCheckIn);
	}
}
