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




namespace AvePoint.GCommon.Contract.Server.Common.BackupDataSearch
{
    using System.Collections.Generic;
    #region == using directives ==
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon.Contract.SharePointBrowser;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using Newtonsoft.Json;
    using RestoreOption = StorageOptimization.Object.RestoreOption;

    #endregion ==

    [DataContract]
    public class RestoreInfo
    {
        [JsonProperty]
        [DataMember]
        public RestoreType RestoreTypeSelect { set; get; }
        [JsonProperty]
        [DataMember]
        public List<ArchiverRestoreSerchResult> NodeObjects { get; set; }
        [JsonProperty]
        [DataMember]
        public StorageDeviceUIDto StorageDeviceDto { get; set; }
        [JsonProperty]
        [DataMember]
        public bool IncludeWorkflowDefinition { get; set; }
        [JsonProperty]
        [DataMember]
        public bool IncludeSharingLink { get; set; }
        [JsonProperty]
        [DataMember]
        public RestoreOption RestoreOption { set; get; }
        [JsonProperty]
        [DataMember]
        public RestoreOption RestoreAPPOption { set; get; }
        [JsonProperty]
        [DataMember]
        public List<ToExportUserInfo> NotificationUsers { get; set; }
        [DataMember]
        public string JobId { get; set; }//for recenter
        [DataMember]
        public bool IsEndUserJob { get; set; } //for recenter
        [DataMember]
        public string ConnectionString { get; set; } //for recenter
        [DataMember]
        public int NodeType { get; set; } //for recenter
        [DataMember]
        public bool IsOpusArchivedDownloadJob { get; set; }
        [DataMember]
        public bool IsRecenterExport { get; set; }
        [DataMember]
        public string OopStubUrl { get; set; }
        [DataMember]
        public int KeepVersionsNumber { get; set; }
        [DataMember]
        public RestoreDocumentVersionsOption RestoreVersionOption { get; set; }
        [DataMember]
        public string BackUpJobId { get; set; }
        [DataMember]
        public int DataSource { get; set; }

    }

    [DataContract]
    public class ToExportUserInfo
    {
        [DataMember]
        [JsonProperty]
        public string UserId { get; set; }
        [DataMember]
        [JsonProperty]
        public string UserName { get; set; }
        [DataMember]
        [JsonProperty]
        public string UserPrincipalName { get; set; }
        [DataMember]
        [JsonProperty]
        public string Email { get; set; }
        [DataMember]
        [JsonProperty]
        public string DisplayName { get; set; }
        [DataMember]
        [JsonProperty]
        public AccountType InviteType { get; set; }
        [DataMember]
        [JsonProperty]
        public int RMUserId { get; set; }
        [DataMember]
        [JsonProperty]
        public string Id { get; set; }
        [DataMember]
        [JsonProperty]
        public string SurName { get; set; }
        [DataMember]
        [JsonProperty]
        public string GivenName { get; set; }
        [DataMember]
        [JsonProperty]
        public string TenantId { get; set; }
    }
    [DataContract]
    public enum AccountType
    {
        [EnumMember]
        User = 0,
        [EnumMember]
        Group
    }

    [DataContract]
    public enum RestoreType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        InPlace =1,
        [EnumMember]
        OutOfPlace =2,
        [EnumMember]
        StubOop = 3
    }
    [DataContract]
    public enum RestoreDocumentVersionsOption
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SpecifyVersions = 1,
        [EnumMember]
        AllVersions = 2,
    }
}
