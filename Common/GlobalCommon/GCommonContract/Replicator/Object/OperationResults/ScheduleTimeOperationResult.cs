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



namespace AvePoint.GCommon.Contract.Replicator.Object.OperationResults
{
    #region using namespace

    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;

    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduleTimeOperationResult : ReplicatorOperationResult
    {
        public ScheduleTimeOperationResult()
            : base(false,null)
        { }

        [DataMember]
        public ScheduleValidationResult ValidationResult { get; set; }

        [DataMember]
        public List<ScheduleDto> PreviewJobs { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduleValidationResult
    {
        [DataMember]
        public ReplicatorSchedule Schedule { get; set; }
        [DataMember]
        public ScheduleValidationResultType ResultType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleValidationResultType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        StartTimeNotValid = 1 << 0,
        [EnumMember]
        StartTimeLaterThanNow = 1 << 1,
        [EnumMember]
        StartTimeLaterThanEndTime = 1 << 2,
        [EnumMember]
        EndTimeNotValid = 1 << 3,
    }
}
