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
namespace Microsoft.Office.Project.Server.Interfaces
{
    using Microsoft.Office.Project.Server.Library;
    using Microsoft.Office.Project.Server.Schema;
    using System.ServiceModel;

    [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
    [ServiceContract(Namespace = "http://schemas.microsoft.com/office/project/server/webservices/Admin/", Name = "Admin")]
    public interface IAdmin
    {
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Admin/ReadTimeSheetSettings", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Admin/ReadTimeSheetSettingsResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        TimeSheetSettingsDataSet ReadTimeSheetSettings();

        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Admin/UpdateTimeSheetSettings", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Admin/UpdateTimeSheetSettingsResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        void UpdateTimeSheetSettings(TimeSheetSettingsDataSet dsDelta);

        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Admin/GetActiveDirectorySyncEnterpriseResourcePoolSettings3", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Admin/GetActiveDirectorySyncEnterpriseResourcePoolSettings3Response")]
        [FaultContract(typeof(ServerExecutionFault))]
        ADSyncERPSettings3 GetActiveDirectorySyncEnterpriseResourcePoolSettings3();

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Admin/SetActiveDirectorySyncEnterpriseResourcePoolSettings3", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Admin/SetActiveDirectorySyncEnterpriseResourcePoolSettings3Response")]
        void SetActiveDirectorySyncEnterpriseResourcePoolSettings3(ADSyncERPSettings3 settings);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Admin/ReadLineClasses", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Admin/ReadLineClassesResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        TimesheetLineClassDataSet ReadLineClasses(TimesheetEnum.LineClassType type, TimesheetEnum.LineClassState classState);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Admin/UpdateLineClasses", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Admin/UpdateLineClassesResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        void UpdateLineClasses(TimesheetLineClassDataSet dsDelta);

        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Admin/ReadFiscalPeriods", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Admin/ReadFiscalPeriodsResponse")]
        FiscalPeriodDataSet ReadFiscalPeriods(int fiscalYear);

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Admin/ReadAllDefinedFiscalYears", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Admin/ReadAllDefinedFiscalYearsResponse")]
        FiscalYearDataSet ReadAllDefinedFiscalYears();

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Admin/ReadPeriods", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Admin/ReadPeriodsResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        TimePeriodDataSet ReadPeriods(TimesheetEnum.PeriodState p0);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/Admin/UpdateReportingPeriods", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/Admin/UpdateReportingPeriodsResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        void UpdateReportingPeriods(TimePeriodDataSet dsDelta, bool validationOnly);
    }
}
