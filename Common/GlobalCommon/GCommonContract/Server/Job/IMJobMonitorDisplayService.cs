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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Wcf;

namespace AvePoint.GCommon.Contract.Server.Job
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMJobMonitorDisplayService
    {
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        JobMonitorDisplay GetDefaultDisplay();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        JobMonitorDisplay GetSystemDefaultDisplay();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        JobMonitorDisplay GetSystemAllDisplay();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        JobMonitorDisplay GetUserDisplay(JobMonitorParameter param);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Module GetModuleDisplay(JobMonitorParameter param);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Module GetModuleDisplayNoValue(JobMonitorParameter param);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateUserDisplay(JobMonitorDisplay display, JobMonitorParameter param);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        IDictionary<int, string> GetJobStateDict();
    }
}
