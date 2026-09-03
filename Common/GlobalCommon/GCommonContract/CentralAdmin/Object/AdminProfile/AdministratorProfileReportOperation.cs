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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdministratorProfileReportOperation : CAOperation
    {
        //6.4已废弃，API仍使用无法删除
        [DataMember]
        [Obsolete]
        public string GenerateReportJobId { get; set; }

        /// <summary>
        /// 标记此操作的类型
        /// </summary>
        [DataMember]
        public ProfileReportAction ReportAction { get; set; }

        /// <summary>
        /// Export Report时候传递给Server的文件类型
        /// </summary>
        [DataMember]
        public ReportFileType ReportFileType { get; set; }

        /// <summary>
        /// Generate Report时由GUI传给Server的用户选择的节点
        /// </summary>
        [DataMember]
        public List<SPTreeNodeDto> ReportScopes { get; set; }

        //6.4已废弃，API仍使用无法删除
        [DataMember]
        [Obsolete]
        public long StartTime { get; set; }

        //6.4已废弃，API仍使用无法删除
        [DataMember]
        [Obsolete]
        public long EndTime { get; set; }

        /// <summary>
        /// 显示Detail级别的个数
        /// </summary>
        [DataMember]
        public int DisplayDetailNum { get; set; }

        /// <summary>
        /// 违规节点数量大于配置文件中设置的最大值
        /// </summary>
        [DataMember]
        public bool IsLargeNumViolationNodes { get; set; }

        /// <summary>
        /// Server端组合好的Tree节点
        /// </summary>
        [DataMember]
        public List<CAProfileReportTreeNodeDto> ReportNodes { get; set; }

        /// <summary>
        /// 用于Load Tree传递给Agent的CurrentNode信息
        /// </summary>
        [DataMember]
        public ProfileContextSource ContextSource { get; set; }

        /// <summary>
        /// Agent通过contextSource组装出一个tree，发给server，用来处理manually fix的go to administrator跳转
        /// </summary>
        [DataMember]
        public SPTreeNodeDto ContextNode { get; set; }

        /// <summary>
        /// 执行Fix manul Undo的时候使用的属性
        /// </summary>
        [DataMember]
        public List<AdminRuleBasicInfo> FixContextsInRule { get; set; }

        /// <summary>
        /// 执行Hide时使用的属性
        /// </summary>
        [DataMember]
        public List<AdminRuleBasicInfo> HideContextsInRule { get; set; }

        /// <summary>
        /// 执行Hide时使用的属性,区分Daily、Weekly、Monthly
        /// </summary>
        [DataMember]
        public IntervalType IntervalType { get; set; }

        /// <summary>
        /// HiddenList数据,用于获取及更新HiddenList数据
        /// </summary>
        [DataMember]
        public List<PEHiddenListDto> HiddenListDtos { get; set; }

        /// <summary>
        /// 优化generate页面性能问题代替ReportScopes属性
        /// </summary>
        [DataMember]
        public List<TreeNodeCollection> TreeNodeCollection { get; set; }

        [DataMember]
        public int SiteCountEachPage { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProfileReportAction
    {
        [EnumMember]
        None,
        [EnumMember]
        GetReportData,
        [EnumMember]
        ExportReport,
        [EnumMember]
        LoadTree,
        [EnumMember]
        Undo,
        [EnumMember]
        Refresh,
        [EnumMember]
        Hide,
        [EnumMember]
        GetHiddenListData,
        [EnumMember]
        ChangeExpiredDate,
        [EnumMember]
        Unhide,
    }
}
