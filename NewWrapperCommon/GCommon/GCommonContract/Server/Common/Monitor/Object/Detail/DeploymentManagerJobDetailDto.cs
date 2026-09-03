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

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeploymentManagerJobDetailDto : JobDetailDto
    {

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Order { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public double Progress { get; set; }

        [DataMember]
        public bool IsFarmSolution { get; set; }

        [DataMember]
        public long StartTime { get; set; }

        [DataMember]
        public long FinishTime { get; set; }

        [DataMember]
        public string SourceHostName { get; set; }

        [DataMember]
        public string DestinationHostName { get; set; }

        [DataMember]
        public string SolutionName { get; set; }

        [DataMember]
        public string SolutionID { get; set; }

        [DataMember]
        public string Operation { get; set; }

        [DataMember]
        public string FeatureName { get; set; }

        [DataMember]
        public string SubJobId { get; set; }

        [DataMember]
        public int JobType { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string Size { get; set; }

        [DataMember]
        public string DPMStatus { get; set; }

        [DataMember]
        public string RelatedObjectTitle { get; set; }

        [DataMember]
        public string ConfigName { get; set; }

        [DataMember]
        public string ConfigProperty { get; set; }

        [DataMember]
        public int QueueType { get; set; }

        [DataMember]
        public string PrimarySiteTitle { get; set; }

        [DataMember]
        public string SecondarySiteTitle { get; set; }

        [DataMember]
        public string ListTitle { get; set; }

        [DataMember]
        public string CompareResults { get; set; }

        [DataMember]
        public string AppName { get; set; }

        [DataMember]
        public string UpdateStatus { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public string CurrentVersion { get; set; }
    }
}
