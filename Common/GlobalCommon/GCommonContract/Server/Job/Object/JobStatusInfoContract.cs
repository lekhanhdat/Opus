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
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Replicator.Object.Message;

namespace AvePoint.GCommon.Contract.Server.Job.Object
{
    [KnownType(typeof(ReplicatorJobStatusInfo))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobStatusInfo
    {
        /// <summary>
        /// job id, 可以写jobId和subJobId
        /// </summary>
        [DataMember]
        public string Id { set; get; }

        /// <summary>
        /// job AgentHost, 当前做job的agent注册时填写的AgentAddress
        /// </summary>
        [DataMember]
        public string AgentHost { set; get; }

        /// <summary>
        /// job type, 表示当前job类型  注：参考JobTypes枚举,值不能为空。
        /// </summary>
        [DataMember]
        public int Type { set; get; }

        /// <summary>
        /// 时间戳
        /// </summary>
        [DataMember]
        public long Stamp { set; get; }

        /// <summary>
        /// 是否为subJob
        /// </summary>
        [DataMember]
        public Boolean IsSubJob { get; set; }

        /// <summary>
        /// 当前进度, 调用UpdateJobProgress方法会更新进度. UpdateJobStatus方法不会更新进度.
        /// </summary>
        [DataMember]
        public int Progress { get; set; }

        /// <summary>
        /// subJob进度在整个job进度集合中所占的加权值,
        /// </summary>
        [DataMember]
        public int Weight { get; set; }

        /// <summary>
        /// job的状态. 调用UpdateJobStatus方法会更新状态. UpdateJobProgress方法不会更新状态.
        /// </summary>
        [DataMember]
        public int State { get; set; }

        /// <summary>
        /// 流量统计（发送流量）
        /// </summary>
        [DataMember]
        public long streamSent { get; set; }

        /// <summary>
        /// 流量统计（接收流量）
        /// </summary>
        [DataMember]
        public long streamReceived { get; set; }

        /// <summary>
        /// 保存job运行中出现的错误信息等.
        /// </summary>
        [DataMember]
        public List<ErrorInfo> ErrorInfos { get; set; }

        [DataMember]
        public bool NotDeleteInfoInJobQueue { get; set; }

        [DataMember]
        public bool TimeOutFailed { get; set; }

        [DataMember]
        public int WaitFlag { get; set; }

        public string MainJobId { get; set; }

        public bool JustNeedFinalOperation { get; set; }

        public virtual JobStatusInfo Clone()
        {
            JobStatusInfo clone = new JobStatusInfo();

            clone.AgentHost = this.AgentHost;
            clone.Id = this.Id;
            clone.IsSubJob = this.IsSubJob;
            clone.Progress = this.Progress;
            clone.Stamp = this.Stamp;
            clone.State = this.State;
            clone.Type = this.Type;
            clone.Weight = this.Weight;

            if (this.ErrorInfos != null)
            {
                clone.ErrorInfos = new List<ErrorInfo>();

                foreach (ErrorInfo item in this.ErrorInfos)
                {
                    clone.ErrorInfos.Add(item.Clone());
                }
            }

            return clone;
        }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Job Status Information:");
            stringBuilder.AppendFormat(" Agent Host:{0}", this.AgentHost);
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat(" Job Id:{0}", this.Id);
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat(" Job Type:{0}", this.Type);
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat(" Job Progress:{0}", this.Progress);
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat(" Job Weight:{0}", this.Weight);
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat(" Job Time Stamp:{0}", this.Stamp);
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat(" Job State:{0}", this.State);
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat(" Is sub job:{0}", this.IsSubJob);
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat(" Main Job Id:{0}", this.MainJobId);
            stringBuilder.AppendLine();
            return stringBuilder.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ErrorInfo
    {
        [DataMember]
        public int EventId { get; set; }

        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public ushort EventCategory { get; set; }
        /// <summary>
        /// AveErrorCodeException to Base 64.
        /// </summary>
        [DataMember]
        public string ErrorCodeExceptionBase64 { get; set; }

        [DataMember]
        public ErrorState State { get; set; }

        public ErrorInfo Clone()
        {
            ErrorInfo clone = new ErrorInfo();

            clone.EventId = this.EventId;
            clone.Error = this.Error;
            clone.State = this.State;

            return clone;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ErrorState
    {
        [EnumMember]
        Info,
        [EnumMember]
        Warn,
        [EnumMember]
        Error
    }
}