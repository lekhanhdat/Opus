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



using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EDJobDto : BaseJobDto
    {
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string FarmId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string FarmName { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string WorkServiceId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string WorkServiceName { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_1)]
        public int DataSource { get; set; }                     // 0 - SharePoint Data, 1 - Archived Data
        

        //public static EDJobDto GenerateEDJob(EDJobDto baseJob,string jobID,int jobType)
        //{
        //    var job = GenerateEDJob(jobID, jobType, baseJob.UserName, baseJob.FarmId, baseJob.FarmName, baseJob.WorkServiceId,
        //                  baseJob.WorkServiceName);
        //    job.PlanType = baseJob.PlanType;
        //    return job;
        //}


        //public static EDJobDto GenerateEDJob(string jobId, int jobType, string userName, string farmId, string farmName, string workServiceId, string workServiceName)
        //{
        //    EDJobDto job = new EDJobDto();
        //    job.Id = jobId;
        //    job.Dependency = jobId;
        //    job.State = (int)JobState.Waiting;
        //    job.Category = (int)PlanCategory.EDiscovery;
        //    job.StartTime = DateTime.UtcNow.Ticks;
        //    job.Type = jobType;
        //    job.PlanType = jobType;
        //    job.UserName = userName;
        //    job.FarmId = farmId;
        //    job.FarmName = farmName;
        //    job.WorkServiceId = workServiceId;
        //    job.WorkServiceName = workServiceName;
        //    return job;
        //}

    }
}
