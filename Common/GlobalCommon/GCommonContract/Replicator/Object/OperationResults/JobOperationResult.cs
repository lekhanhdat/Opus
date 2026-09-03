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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Replicator.Object.ViewModels;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;

namespace AvePoint.GCommon.Contract.Replicator.Object.OperationResults
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobOperationResult : ReplicatorOperationResult
    {
        public JobOperationResult()
            : base(false, null)
        {

        }

        public JobOperationResult(bool hasError, ReplicatorOperationResultError exception)
            : base(hasError, exception)
        {

        }

        public static readonly JobOperationResult Empty = new JobOperationResult();

        [DataMember]
        public List<ReplicatorSubJobDto> SubJobs { get; set; }

        [DataMember]
        public List<ReplicatorJobSummary> JobSummaries { get; set; }

        [DataMember]
        public List<JobDetailDto> SubJobDetails { get; set; }

        [DataMember]
        public int DetailsTotalCount { get; set; }

        [DataMember]
        public List<ReplicatorSubJobReport> SubJobReports { get; set; }

        [DataMember]
        public List<BaseJobDto> ImportPlanJobs { get; set; }

        [DataMember]
        public Dictionary<string, ReplicatorPlanInfoModel> ImportPlanJobSummaries { get; set; }
    }
}
