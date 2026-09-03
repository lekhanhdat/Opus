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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail
{
    [DataContract]
    public enum JobReportGenerateState
    {
        [EnumMember]
        Waiting = 0,
        [EnumMember]
        Generating = 1,
        [EnumMember]
        Finished = 2,
        [EnumMember]
        Failed = 3,
        [EnumMember]
        CompressedFailed = 4,
        [EnumMember]
        Skipped = 6
    }
    [DataContract]
    public class JobReportGenerateDto
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public int Progress { get; set; }

        [DataMember]
        public JobReportGenerateState State { get; set; }


        public bool IsFinal
        {
            get
            {
                return IsfinalState(State);
            }
        }

        public bool IsfinalState(JobReportGenerateState state)
        {
            return state == JobReportGenerateState.CompressedFailed ||
                    state == JobReportGenerateState.Failed ||
                    state == JobReportGenerateState.Finished ||
                    state == JobReportGenerateState.Skipped;
        }

        public bool IsIgnoreProgress
        {
            get
            {
                return IsFailed || State == JobReportGenerateState.Skipped;
            }
        }

        public bool IsFailed
        {
            get
            {
                return State == JobReportGenerateState.Failed ||
                    State == JobReportGenerateState.CompressedFailed;
            }
        }
    }
}
