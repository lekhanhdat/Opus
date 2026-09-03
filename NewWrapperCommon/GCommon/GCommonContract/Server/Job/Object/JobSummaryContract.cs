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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.DeploymentManager.Object;

namespace AvePoint.GCommon.Contract.Server.Job.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobSummary
    {
        /// <summary>
        /// Key为5.6 summary中左侧的名称
        /// </summary>
        [DataMember]
        public string Key { get; set; }
        /// <summary>
        /// Value为右侧这个Key所对应的值
        /// </summary>
        [DataMember]
        public object Value { get; set; }

        /// <summary>
        /// 用来区分该条记录所属的SubJob
        /// </summary>
        [DataMember]
        public string SubJobId { get; set; }

        /// <summary>
        /// 用来区分该条记录的类型
        /// </summary>
        [DataMember]
        public int EntityType { get; set; }

        /// <summary>
        /// 用于存储国际化可能带的参数,该参数目前由PropertyItems 取代
        /// </summary>
        [Obsolete]
        [DataMember]
        public object[] Args { get; set; }

        /// <summary>
        /// comment 为多个key的情况
        /// </summary>
        [DataMember]
        public List<PropertyItem> PropertyItems { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobDetailGlueType
    {
        [EnumMember]
        Base = 0,
        [EnumMember]
        SubJob = 1
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobDetailGlue
    {
        [DataMember]
        public JobDetailGlueType GlueType { get; set; }
        [DataMember]
        public BaseJobDto Job { get; set; }

        [DataMember]
        public PlanDto Plan { get; set; }
        [DataMember]
        public AbstractGroup PlanGroup { get; set; }
        /// <summary>
        /// DPM用来区分Queue的类型
        /// </summary>
        [DataMember]
        public int DMPlanCategory { get; set; }

        [DataMember]
        public Dictionary<string, string> JobSettingInfo { get; set; }

        [DataMember]
        public Dictionary<string, string> JobSummaryInfo { get; set; }

        [DataMember]
       public List<JobSummary> JobSummaries { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SubJobDetailGlue : JobDetailGlue
    {
        public SubJobDetailGlue()
        {
            this.GlueType = JobDetailGlueType.SubJob;
        }
        [DataMember]
        public SubJobDto SubJob { get; set; }
        [DataMember]
        public AbstractGroup PlanGroup { get; set; }
        ///// <summary>
        ///// DPM用来区分Queue的类型
        ///// </summary>
        //[DataMember]
        //public DMPlanCategory DMPlanCategory { get; set; }
    }
}
