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

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditPruningChart : BaseChart
    {
        [DataMember]
        public AuditPruningChartType Type { get; set; }
        [DataMember]
        public ScopeProfile ScopeProfile { get; set; }
        [DataMember]
        public string PruningJobId { get; set; }

        [DataMember]
        public List<ScopeProfile> ScopeProfiles { get; set; }
        [DataMember]
        public List<RCCollectorJobDto> AuditPruningJobs { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuditPruningChartType
    {
        [EnumMember]
        RunPruning,
        [EnumMember]
        RunRestore,
        [EnumMember]
        RunPruningInThread,
        [EnumMember]
        RunRestoreInThread,
        [EnumMember]
        GetJobs,
        [EnumMember]
        GetProfiles,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PruningOption
    {
        [EnumMember]
        Delete = 0,
        [EnumMember]
        Move = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditPruningFilterType
    {
        /// <summary>
        /// 过滤的是userId
        /// </summary>
        [DataMember]
        public static string UserId { get { return "userId"; } }
        [DataMember]
        public static string Time { get { return "occurred"; } }

        [DataMember]
        public static string UserName { get { return "userName"; } }
    }
}
