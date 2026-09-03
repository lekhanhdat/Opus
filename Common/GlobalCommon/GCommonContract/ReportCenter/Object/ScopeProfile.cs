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
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AuditReport.MgtApiReport;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScopeProfile : BaseConfigSetting
    {
        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public ReportType Type { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public NodeLevel Level { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public ProfileContent Content { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool Collectable { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool EmailSet { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool Hidden { get; set; }
        //For Blob Calculator
        [DataMember(EmitDefaultValue = false)]
        public string JobId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool HasJobRunned { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long LastRunTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string EmailNoctificationName { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string ExportLocationId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string CreateByUserId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string ModifyUserId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public long CreateTime { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public long ModifyTime { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string ScheduleExportEmailId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<string> SiteCollectionIds { get; set; }
        
        /// <summary>
        /// [SAAS-23604]目前只用在了 rc RunAuditReportNewJob
        /// </summary>
        public string CreateJobBy { get; set; }
        /// <summary>
        /// 是否需要Check SiteCollection
        /// </summary>
        [DataMember]
        public bool NeedCheck { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ObjectInfoDto ObjectInfo { get; set; }

        public bool IsShared()
        {
            if (ObjectInfo == null || ObjectInfo.ObjectPermissions == null)
            {
                throw new Exception("Object permission info is null");
            }
            var planPermissions = ObjectInfo.ObjectPermissions;
            int sharedCount = 0;
            foreach (var permission in planPermissions)
            {
                if (permission.PermissionScope == ObjectPermissionScopeType.User && permission.Permission > 0)
                {
                    sharedCount++;
                }
            }
            var isPlanShared = sharedCount > 1;
            return isPlanShared;
        }

        public string ScheduleJobQueueId { get; set; }
        //for auditReport ShowReport MessageBar LastestJobID which Can show Report.
        [DataMember(EmitDefaultValue = false)]
        public string LastestSuccessJobId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long LastestSuccessRuntime { get; set; }

        public ScopeProfileErrorType ErrorType { get; set; }

        public override string ToString()
        {
            return string.Format("ScopeProfile[Id {0}, Name {1}, CreateBy {2}, ModifyBy {3}]", Id, Name, CreateByUserId, ModifyUserId);
        }
    }

    public enum ScopeProfileErrorType
    {
        None,
        NoSelectNode,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProfileContent
    {
        [DataMember(EmitDefaultValue = false)]
        public SPTreeScope TreeScope { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public AuditControllerScope AuditControllerScope { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public AuditReportScope AuditReportScope { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public AuditPruningScope AuditPruningScope { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public SPUserScope UserScope { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public ExportReportSettingsScope ExportReportSettingsSope { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public AdminReportScope AdminReportScope { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public List<RCEmailNotificationScope> EmailScopes { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public ScheduleExportReportScope ScheduleExportScope { get; set; }
        [DataMember(EmitDefaultValue =false)]
        public ManagementAPIReportScope ManagementAPIReportScope { get; set; }
        [DataMember(EmitDefaultValue =false)]
        public UsageReportScope UsageReportScope { get; set; }
    }
}