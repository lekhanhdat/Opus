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
using AvePoint.GCommon.Contract.ReportCenter.AuditReport.MgtApiReport;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class O365AuditDataInfo
    {
        [DataMember]
        public long Date { set; get; }
        [DataMember]
        public string IP { set; get; }
        [DataMember]
        public string UserName { set; get; }
        [DataMember]
        public string DisplayName { set; get; }
        [DataMember]
        public string Activity { set; get; }
        [DataMember]
        public string Item { set; get; }
        [DataMember]
        public string Detail { set; get; }
        [DataMember]
        public string DateDisplayValue { set; get; }
        [DataMember]
        public string Url { set; get; }
        [DataMember]
        public string OperationSystem { set; get; }
        [DataMember]
        public O365ActivityType DataSource { set; get; }
        [DataMember]
        public string AdminActionDetail { get; set; }
    }

    public enum O365ItemType
    {
        Invalid = 0,
        File = 1,
        Folder = 5,
        Web = 6,
        Site = 7,
        Tenant = 8,
        DocumentLibrary = 9,
        Page = 11,
        List = 12,
        ListItem = 13,
        Field = 14,
    }

    public class O365GroupInfo
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupDisplayname { get; set; }
        public O365GroupType GroupType { get; set; }
        public string GroupEmail { get; set; }
        public string GroupTeamSiteUrl { get; set; }
    }

}
