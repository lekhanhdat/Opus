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
using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASecurityImportPermissionsOperation : CAOperation
    {
        [DataMember]
        public SecurityConfigurationFileInfo ConfigurationFileInfo { get; set; }
        [DataMember]
        public ImportAction Action { get; set; }
        [DataMember]
        public TransferOption ConflictOption { get; set; }
        [DataMember]
        public List<CAImportResultInfo> SearchResultsList { get; set; }
        [DataMember]
        public Int32 TotalResultCount { get; set; }
       
        [DataMember]
        public bool BreakInheritNodes { get; set; }
        [DataMember]
        public bool IsCopyPermission { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SecurityConfigurationFileInfo
    {
        [DataMember]
        public String FileName { get; set; }
        [DataMember]
        public string StorageLowName { get; set; }
        [DataMember]
        public long FileLength { get; set; }
        [DataMember]
        public String SubFolder { get; set; }
        [DataMember]
        public ResultStatus UploadResult { get; set; }

        public SecurityConfigurationFileInfo()
        {
            FileName = string.Empty;
            StorageLowName = string.Empty;
            FileLength = 0;
            SubFolder = string.Empty;
            UploadResult = ResultStatus.None;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ImportAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Validate = 1,
        [EnumMember]
        Update = 2,
        [EnumMember]
        Rollback = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAImportResultInfo : ResultBase
    {
        [DataMember]
        public string FullPath { get; set; }
        [DataMember]
        public string WebUrl { get; set; }
        [DataMember]
        public string ListTitle { get; set; }
        [DataMember]
        public int ItemRowId { get; set; }
        [DataMember]
        public string ObjectName { get; set; }
        [DataMember]
        public NodeLevel PathType { get; set; }
        [DataMember]
        public int Inherit { get; set; }
        [DataMember]
        public string MemberName { get; set; }
        [DataMember]
        public string LoginName { get; set; }
        [DataMember]
        public MemberType MemberType { set; get; }
        [DataMember]
        public List<Permission> Permissions { set; get; }
        [DataMember]
        public List<Permission> OriginalPermissions { get; set; }
        [DataMember]
        public ChangeMode Change { get; set; }
        [DataMember]
        public CheckPrincipalStatus CheckStatus { get; set; }
        [DataMember]
        public string FailedReason { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public CAStringFormatMessage FormatFailedReason { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAImportResultStatistics
    {
        [DataMember]
        public Int32 TotalResult { get; set; }
        [DataMember]
        public Int32 FailedResult { get; set; }
        [DataMember]
        public Int32 SucceedResult { get; set; }
    }

    [DataContract(Namespace = (ContractConstants.Namespace))]
    public enum ChangeMode
    {
        [EnumMember]
        None,
        [EnumMember]
        Add,
        [EnumMember]
        Delete,
        [EnumMember]
        Modify,
        [EnumMember]
        Inherit
    }

    [DataContract(Namespace = (ContractConstants.Namespace))]
    public enum CheckPrincipalStatus
    {
        [EnumMember]
        Failed,
        [EnumMember]
        Succeed,
    }
}
