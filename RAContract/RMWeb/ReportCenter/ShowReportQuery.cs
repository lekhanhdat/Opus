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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.ReportCenter
{
    [DataContract]
    public class ShowReportQuery
    {
        [DataMember]
        public JobType ReportJobType { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public List<string> SearcheKeys { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int CurrentPage { get; set; }
        [DataMember]
        public int ProfileId { get; set; }
        [DataMember]
        public string JobId{ get; set; }
        [DataMember]
        public List<int> FilterLevels { get; set; }
        [DataMember]
        public int Operation { get; set; }

        //用于排序
        [DataMember]
        public bool isAscending { get; set; }
        [DataMember]
        public string SortBy { get; set; }

        /// <summary>
        /// For SPOActionAuditReport and OneDriveActionAuditReport, will use this filter users
        /// </summary>
        [DataMember]
        public List<string> FilterListObject { get; set; }

        /// <summary>
        /// For SPOActionAuditReport and OneDriveActionAuditReport, will use this filter Action, And will transfer int32  value.
        /// </summary>
        [DataMember]
        public string FilterObjectString { get; set; }
    }
    [DataContract]
    public class ManualReviewQuery
    {
        [DataMember]
        public DateTime? StartTime { get; set; }
        [DataMember]
        public DateTime? EndTime { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public List<string> SearcheKeys { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int CurrentPage { get; set; }
        [DataMember]
        public Dictionary<int, List<dynamic>> FilterInfos { get; set; }
        //用于排序
        [DataMember]
        public bool isAscending { get; set; }
        [DataMember]
        public string SortBy { get; set; }
        [DataMember]
        public ViewTab viewTab { get; set; }

    }
    public class QueryResult
    {
        public int TotalCount { get; set; }

        public List<int> ids { get; set; }
    }
    [DataContract]
    public class ManualReviewJobQuery
    {
        [DataMember]
        public DateTime? StartTime { get; set; }
        [DataMember]
        public DateTime? EndTime { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public List<string> SearcheKeys { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int CurrentPage { get; set; }
        [DataMember]
        public Dictionary<int, List<dynamic>> FilterInfos { get; set; }
        //用于排序
        [DataMember] 
        public bool isAscending { get; set; }
        [DataMember] 
        public string SortBy { get; set; }
        [DataMember]
        public ViewTab viewTab { get; set; }
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public List<int> ids { get; set; }
        [DataMember]
        public string UserId { get; set; }

    }

    public enum ViewTab
    {
        Independent = 1,
        Related = 2,
        History = 3
    }

    public enum EscalateType
    {
        Escalate = 0,
        Reassign = 1
    }
    [DataContract]
    public class EscalateModel
    {
        [DataMember]
        public List<ToUserInfo> EscalateTos { get; set; }
        [DataMember]
        public List<int> ids { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public bool isSendMail { get; set; }
        [DataMember]
        public EscalateType EscalateType { get; set; }
    }
    [DataContract]
    public class ChangeActionModel
    {
        [DataMember]
        public RelatedRecordOption relatedRecordAction { get; set; }
        [DataMember]
        public List<ChangedItems> ids { get; set; }
    }

    public class ChangedItems
    {
        public int id;
        public int SourceFlag;
    }
    [DataContract]
    public class ToUserInfo
    {
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string UserPrincipalName { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public AccountType InviteType { get; set; }
        [DataMember]
        public int RMUserId { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string SurName { get; set; }
        [DataMember]
        public string GivenName { get; set; }
        [DataMember]
        public string TenantId { get; set; }
    }
}
