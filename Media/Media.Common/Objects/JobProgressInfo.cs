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



namespace AvePoint.Media.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    #endregion

    public class JobProgressInfo
    {
        public String Id { set; get; }
        public String AgentHost { set; get; }
        public Int32 Type { set; get; }
        public Int64 Stamp { set; get; }
        public Boolean IsSubJob { get; set; }
        public Int32 Progress { get; set; }
        public Int32 Weight { get; set; }
        public Int32 State { get; set; }
        public Boolean IsFinal { get; set; }

        public JobProgressInfo()
        { }

        public JobProgressInfo(JobStatusInfo jobStatusInfo)
        {
            this.Id = jobStatusInfo.Id;
            this.AgentHost = jobStatusInfo.AgentHost;
            this.Type = jobStatusInfo.Type;
            this.Stamp = jobStatusInfo.Stamp;
            this.IsSubJob = jobStatusInfo.IsSubJob;
            this.Progress = jobStatusInfo.Progress;
            this.Weight = jobStatusInfo.Weight;
            this.State = jobStatusInfo.State;
        }

        public JobStatusInfo ToStatusInfo()
        {
            return new JobStatusInfo
            {
                Id = this.Id,
                AgentHost = this.AgentHost,
                Type = this.Type,
                Stamp = this.Stamp,
                IsSubJob = this.IsSubJob,
                Progress = this.Progress,
                Weight = this.Weight,
                State = this.State
            };
        }
    }
}
