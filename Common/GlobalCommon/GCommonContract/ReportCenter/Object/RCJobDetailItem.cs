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




namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Contract.Tree.Object;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCJobDetailItem
    {
        public RCJobDetailItem()
        {
            PropertyItems = new List<PropertyItem>();
        }

        [DataMember]
        public string Id { get; set; }
        /// <summary>
        /// main jobId
        /// </summary>
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public ReportType ReportType { get; set; }
        [DataMember]
        public string FarmName { get; set; }
        [DataMember]
        public NodeLevel Level { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public long Value { get; set; }
        [DataMember]
        public State State { get; set; }
        [DataMember]
        public string Comment { set; get; }
        [DataMember]
        public ReportCenterErrorConstant ErrorCode { set; get; }

        /// <summary>
        /// key 国际化词条对应的常量, 保存在RCJobReportMessageKey.cs中
        /// Args 国际化词条的参数
        /// </summary>
        [DataMember]
        public List<PropertyItem> PropertyItems { get; set; }

        [DataMember]
        public int EntityType { get; set; }

        public override string ToString()
        {
            return string.Format("RCJobDetailItem[Id {0}, JobId {1}, ReportType {2}, FarmName {3}, Level {4}, Url {5}, Value {6}, State {7}, Comment {8}]"
                , Id, JobId, ReportType, FarmName, Level, Url, Value, State, Comment);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum State
    {
        [EnumMember]
        Successed,
        [EnumMember]
        Failed,
        [EnumMember]
        Skipped,
    }

    public class RCJobDetailItemComparer : IEqualityComparer<RCJobDetailItem>
    {
        public bool Equals(RCJobDetailItem x, RCJobDetailItem y)
        {
            var result = (x.JobId == y.JobId && x.FarmName == y.FarmName && x.Level == y.Level && x.Url == y.Url);
            return result;
        }
        public int GetHashCode(RCJobDetailItem obj)
        {
            int hash = (obj.JobId + obj.FarmName + obj.Level + obj.Url).GetHashCode();
            return hash;
        }
    }
}