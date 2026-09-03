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
namespace AvePoint.GCommon.Contract.CloudServiceCommon
{
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.SuperUserConfiguration.Object;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public sealed class JobQueueMessage
    {
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public string SubJobId { get; set; }

        [DataMember]
        public int JobType { get; set; }

        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public JobTenantInfo JobTenantInfo { get; set; }

        [DataMember]
        public QueueMessageStatus MessageStatus { get; set; }

        [DataMember]
        public string ProductVersion { get; set; }


        [DataMember]
        public string Extension { get; set; }

        public string JobQueueName { get; set; }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.AppendFormat("JobId:{0}, ", JobId);
            buffer.AppendFormat("SubJobId:{0}, ", SubJobId);
            buffer.AppendFormat("JobType:{0}, ", JobType);
            buffer.AppendFormat("PlanId:{0}, ", PlanId);
            buffer.AppendFormat("Extension:{0}, ", Extension);
            buffer.AppendFormat("JobQueueName:{0}", JobQueueName);
            if (JobTenantInfo != null)
            {
                buffer.AppendFormat("JobTenantInfo:{0}", JobTenantInfo.TenantName);
            }
            return buffer.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public sealed class JobTenantInfo
    {

        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public string TenantName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public sealed class QueueMessageExtensionSetting
    {
        [DataMember]
        public bool OD4BNew { get; set; }

        [DataMember]
        public bool EnableSuperUserDecryptsFiles { get; set; }

        [DataMember]
        public SuperUserConfigurationDto SuperUserConfigurationDto { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum QueueMessageStatus
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        TimeOutResend = 1
    }

    // 非通信用枚举，不需要添加contract标签
    public enum QueueMessageAction
    {
        Receive = 0,
        Abandon = 1,
        Drop = 2
    }
}
