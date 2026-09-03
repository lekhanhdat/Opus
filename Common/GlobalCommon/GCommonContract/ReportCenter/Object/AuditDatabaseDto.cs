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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditDatabaseDto : IProfileContent
    {
        [DataMember]
        public string Id { get; set; }
        /// <summary>
        /// DB Server 名包括实例
        /// </summary>
        [DataMember]
        public string DatabaseServerName { get; set; }
        /// <summary>
        /// DB 名
        /// </summary>
        [DataMember]
        public string DatabaseName { get; set; }
        /// <summary>
        /// DB 用户名
        /// </summary>
        [DataMember]
        public string AccountName { get; set; }

        /// <summary>
        /// DB 密码
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// 是否是default DB
        /// </summary>
        [DataMember]
        public bool IsDefault { get; set; }
        /// <summary>
        /// DB优先级
        /// </summary>
        [DataMember]
        public int Order { get; set; }
        /// <summary>
        /// DB 总大小
        /// </summary>
        [DataMember]
        public long DatabaseTotalSize { get; set; }
        /// <summary>
        /// DB 中Audit 数据数据量
        /// </summary>
        [DataMember]
        public long DatabaseAuditDataSize { get; set; }

        /// <summary>
        /// DB 中剩余空间大小
        /// </summary>
        [DataMember]
        public long DatabaseFreeSize { get; set; }

        /// <summary>
        /// DB 中剩允许Auditor 数据占用的百分比，占总DB 空间的百分比
        /// </summary>
        [DataMember]
        public int AuditDataMaxPercent { get; set; }
    }
}
