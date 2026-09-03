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
using System.Collections.Generic;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.ExportReport.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.Server.Common.ExportReport
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMExportReportService
    {
        [OperationContract]
        bool CreateExportReport(ExportReportDto dto);

        [OperationContract]
        bool EditExportReport(ExportReportDto dto);

        [OperationContract]
        List<ExportReportDto> GetAllExportReport();

        /// <summary>
        /// 此方法只获取Export report dto的ID, Name和IsEachSite属性
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<ExportReportDto> GetAllSimpleExportReport();

        [OperationContract]
        ExportReportDto GetExportReportById(string id);

        [OperationContract]
        List<string> DeleteExportReport(List<string> ids);

        [OperationContract]
        PhysicalDeviceResult PhysicalDeviceDetailTest(PhysicalDeviceDto dto, ServiceType serviceType);

        [OperationContract]
        void ExportJobReport(ExportJobReportData dto);

        [OperationContract]
        bool IsExportReportExist(string name, string id);
    }
}
