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



using System.ComponentModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobState
    {
        [Description("None")]
        [EnumMember]
        None = -2,

        [Description("Waiting")]
        [EnumMember]
        Waiting = -1,

        [Description("In Progress")]
        [EnumMember]
        InProgress = 0,

        [Description("In Progress")]
        [EnumMember]
        Started = 1,

        [Description("Finished")]
        [EnumMember]
        Finished = 2,

        [Description("Failed")]
        [EnumMember]
        Failed = 3,

        [Description("Stopped")]
        [EnumMember]
        Stopped = 4,

        [Description("Paused")]
        [EnumMember]
        Paused = 5,

        [Description("Skipped")]
        [EnumMember]
        Skiped = 6,

        [Description("Finished with Exception")]
        [EnumMember]
        FinishedException = 7,

        [Description("Pending")]
        [EnumMember]
        Pending = 8,

        [Description("Stopping")]
        [EnumMember]
        Stopping = 9,

        [Description("Pausing")]
        [EnumMember]
        Pausing = 10,

        [Description("WaittingInServiceBus")]
        [EnumMember]
        WaitingInServiceBus = 11,

        [Description("Pausing")]
        [EnumMember]
        WaitingReSendToServiceBus = 12,

        [Description("Delay")]
        [EnumMember]
        Delay = 13,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FinishedStates
    {
        [Description("Finished")]
        [EnumMember]
        Finished = 2,

        [Description("Failed")]
        [EnumMember]
        Failed = 3,

        [Description("Stopped")]
        [EnumMember]
        Stopped = 4,

        [Description("Paused")]
        [EnumMember]
        Paused = 5,

        [Description("Skipped")]
        [EnumMember]
        Skiped = 6,

        [Description("Finished with Exception")]
        [EnumMember]
        FinishedException = 7
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobUpdateState
    {
        [EnumMember]
        Successful = 1,

        [EnumMember]
        Failed = 2,

        [EnumMember]
        NeedNotUpdate = 3,
    }
}
