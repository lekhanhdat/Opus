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
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    /// <summary>
    /// TODO: Need a common class .
    /// </summary>
    [DataContract(Name = ContractConstants.Namespace)]
    public class ReportServerInfo
    {
        [DataMember]
        public string ReportServerName { get; set; }
        [DataMember]
        public string ReportServerHostOrIpAddress { get; set; }
        [DataMember]
        public int ReportServerControlPort { get; set; }
        [DataMember]
        public string ReportServerProductVersion 
        {
            get{return ProductVersion;}
            set { ProductVersion = value; }
        }
        [DataMember]
        public string ProductVersion { get; set; }
        [DataMember]
        public string DisplayVersion { get; set; }
        [DataMember]
        public string ReportServerPlatform { get; set; }
        [DataMember]
        public string ReportServerCertThumbPrint { get; set; }
        [DataMember]
        public string ReportServerScheme { get; set; }
        [DataMember]
        public string ControlServerAddress { get; set; }
        [DataMember]
        public int ControlServerPort { get; set; }

        [DataMember]
        public Int32 ReportServerRegisterMaxTries { get; set; }
        [DataMember]
        public Int32 ReportServerRegisterWaitSeconds { get; set; } 

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(Environment.NewLine);

            AddNameAndValue(builder, "ReportServerName", ReportServerName);
            AddNameAndValue(builder, "ReportServerHostOrIpAddress", ReportServerHostOrIpAddress);
            AddNameAndValue(builder, "ReportServerControlPort", ReportServerControlPort);
            AddNameAndValue(builder, "ReportServerProductVersion", ReportServerProductVersion);
            AddNameAndValue(builder, "ReportServerPlatform", ReportServerPlatform);
            AddNameAndValue(builder, "ReportServerScheme", ReportServerScheme);
            AddNameAndValue(builder, "ControlServerAddress", ControlServerAddress);
            AddNameAndValue(builder, "ControlServerPort", ControlServerPort);

            return builder.ToString();
        }

        private void AddNameAndValue(StringBuilder builder, string name, object value)
        {
            builder.Append(name).Append("\t\t").Append(value).Append(Environment.NewLine);
        }
    }

    public class ExtConfig
    {
        //每个Audit数据库最大占用空间，单位MB
        public int AuditorTableSize { get; set; }
        //每个Audit数据表最大数据行
        public int AuditorDatabaseSize { get; set; }
        //Report发Email附件的大小限制，单位MB
        public int AttachmentSize { get; set; }
        //Cache过期时间,单位Minute
        public int CacheExpiredTime { get; set; }
        public bool IsAuditorBuiltin { get; set; }

        public int AuditReportPdfSize { get; set; }
        /// <summary>
        /// 匹配ip时每批次处理的iislog数量
        /// </summary>
        public int MatchingIPBatchSize { get; set; }

        /// <summary>
        /// Compliance Reports导出xlsx每个Sheet容纳的最大数据行
        /// </summary>
        public int AuditReportXlsxSheetSize { get; set; }
    }
}
