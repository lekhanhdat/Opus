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
using System.Xml.Serialization;
using System.Reflection;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    public interface IJob
    {
        string Id { get; }
        int Type { get; }
        int State { get; }
        IJob Parent { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    //[KnownType(typeof(BaseJobDto))]
    //[KnownType(typeof(SubJobDto))]
    [KnownType("GetKnownTypes")]
    public class JobData
    {
        public static IEnumerable<Type> GetKnownTypes()
        {
            return AveKnownTypeContext.GetKnonwTypes(MethodBase.GetCurrentMethod().DeclaringType);
        }

        List<IJob> jobs;
        [DataMember]
        public List<IJob> Jobs
        {
            get { return jobs; }
            set { jobs = value; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SubJobDto : IJob
    {
        [XmlIgnore]
        public IJob Parent { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string ParentId { get; set; }
        [DataMember]
        public long StartTime { get; set; }
        [DataMember]
        public long FinishTime { get; set; }
        [DataMember]
        public double Progress { get; set; }
        [DataMember]
        public int State { get; set; }
        [DataMember]
        public int ControlState { get; set; }
        [DataMember]
        public string SrcAgentName { get; set; }
        [DataMember]
        public string DestAgentName { get; set; }
        [DataMember]
        public double Weight { get; set; }
        [DataMember]
        public string Detail { get; set; }
        [DataMember]
        public string ServiceGroupId { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string StateStr { get; set; }
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public string StartTimeStr { get; set; }
        [DataMember]
        public string FinishTimeStr { get; set; }
        [DataMember]
        public long Stamp { get; set; }

        /// <summary>
        /// 记录该subJob在使用哪个media service做backup job。（注：Item这一块多media备份，用此属性标示Running的子Job占据了哪些media）
        /// </summary>
        [DataMember]
        public string MediaServiceId { get; set; }

        [DataMember]
        public string Performance { get; set; }

        [DataMember]
        public string Dependency { get; set; }

        [DataMember]
        public JobContextDto JobContext { get; set; }

        [DataMember]
        public string String1 { get; set; }

        [DataMember]
        public string String2 { get; set; }


        [DataMember]
        public string CLBGuid { get; set; }


        [DataMember]
        public int WaitFlag { get; set; }

        /// <summary>
        /// DPM Job为了区分Type是Deploy Export还是Import而设立的（只有DPM在使用）
        /// </summary>
        [DataMember]
        public int QueueType { get; set; }

        /// <summary>
        /// 标记Job的类型
        /// </summary>
        [DataMember]
        public long Tags { get; set; }

        public JobTags Tag
        {
            get { return (JobTags)Tags; }
        }

    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum WaitFlag
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        WaitAgent = 1,
        [EnumMember]
        ReadyToRun = 2,
    }
    [Flags]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobTags : long
    {
        /// <summary>
        /// Default Value
        /// </summary>
        [EnumMember]
        Nil = 0,        //0  
        /// <summary>
        /// Remote Farm
        /// </summary>
        [EnumMember]
        RemoteFarm = 1, //1  
        /// <summary>
        /// local Farm
        /// </summary>
        [EnumMember]
        LocalFarm = 1 << 1,      //10  
        /// <summary>
        /// job use sp2010
        /// </summary>
        [EnumMember]
        SP2010 = 1 << 3,     //1000 
        /// <summary>
        /// job use sp2013
        /// </summary>
        [EnumMember]
        SP2013 = 1 << 2,     //100 
    }

    public enum SubJobConverterType
    {
        Default = 0
    }

}
