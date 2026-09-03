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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.ReportCenter
{
    public class BaseReport
    {
        public int ObjectLevel { get; set; }
        public string TitleOrName { get; set; }
        public string Url { get; set; }
        public string CreatedBy { get; set; }
        public long CreatedTime { get; set; }
        public string LastModifiedBy { get; set; }
        public long LastModifiedTime { get; set; }
        public string SPWebTimeZoneName { get; set; }

        //不在DB中存储，时间转换用
        public string LastModifiedTimeStr { get; set; }
        public string CreatedTimeStr { get; set; }
    }

    public class ReportCell
    {
        public string Key { get; set; }
        public object Value { get; set; }
    }

    public enum RMReportStatus
    {
        Successful = 0,
        Failed,
        Skip
    }

    public class ReportFilter
    {
        public Dictionary<ReportFilterType, List<ReportFilterData>> Filters { get; set; }
    }

    public enum ReportFilterType
    {
        User = 1,
        Action = 2,
        ObjectLevel = 3,
    }

    public class ReportFilterData
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public enum RMReportObjectLevel
    {
        None = 0,
        Document = 1,
        SiteCollection,
        Site,
        List,
        Item,
        PhysicalFile,
        PhysicalRecord,
        Folder,
        Attachment,
        PhysicalBox,
        ExchangeOnlineItem = 5110,
        PhyBox = 9300,
        PhyCustom = 9250,
        PhyFolder = 9400,
        PhyRecord = 9500,
        FSFolder = 2100,
        FSFile = 2200,
        CustomizeConnectorItem = 2300,
        BoxFolder = 7003,
        BoxFile = 7104,
        GoogleDrive = 7201,
        GoogleFolder = 7202,
        GoogleFile = 7203,
        DocumentVersion = 998,
        ItemVersion = 999,
    }

    public enum ClientAuditObjType
    {
        [EnumMember]
        All = -1,
        [EnumMember]
        None = 0,
        [EnumMember]
        SiteCollection = 1,
        [EnumMember]
        Site = 2,
        [EnumMember]
        List = 4,
        [EnumMember]
        Folder = 8,
        [EnumMember]
        ListItem = 16,
        [EnumMember]
        Document = 32,
    }
}
