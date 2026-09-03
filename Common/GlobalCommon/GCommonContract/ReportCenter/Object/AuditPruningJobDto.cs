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
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditPruningJobDto : BaseJobDto
    {
        /// <summary>
        /// 选择MoveData时该值对应数据文件fullpath
        /// </summary>
        [DataMember]
        public string PrunedDataFile { get; set; }
        /// <summary>
        /// 选择move data并且没有执行Restore之前该值为true
        /// 之后Restore之后原来数据就被删除了，该值应设为false
        /// 选择deeleteData时该值应该设为false
        /// </summary>
        [DataMember]
        public bool CanRestoreData { get; set; }

        [DataMember]
        public int ProcessedCount { get; set; }

        [DataMember]
        public PruningOption PruningOption { get; set; }

        [DataMember]
        public AuditPruningJobType AuditPruningJobType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuditPruningJobType
    {
        [EnumMember]
        Delete = 0,
        [EnumMember]
        Move = 1,
        [EnumMember]
        Restore = 2,
    }
}