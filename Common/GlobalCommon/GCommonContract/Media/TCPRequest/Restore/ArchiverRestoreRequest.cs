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




namespace AvePoint.GCommon.Contract.Media.TCPRequest.Restore
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverRestoreRequest : MediaTCPRequest
    {
        [DataMember]
        public String FarmName { get; set; }

        [DataMember]
        public String SiteUrl { get; set; }

        [DataMember]
        public String WebAppUrl { get; set; }

        [DataMember]
        public RestoreOption RestoreOption { get; set; }

        [DataMember]
        public RestoreFSOption RestoreFSOption { get; set; }

        [DataMember]
        public Int64 ArchiveTime { get; set; }

        [DataMember]
        public String ArchiveJobId { get; set; }

        [DataMember]
        public String PlanId { get; set; }

        [DataMember]
        public String RestoreJobId { get; set; }

        [DataMember]
        public PlatformType PlatformType { get; set; }

        [DataMember]
        public ProductVersion ProductVersion { get; set; }

        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }

        [DataMember]
        public List<LogicalDeviceDto> DataLogicalDeviceList { get; set; }

        [DataMember]
        public LogicalDeviceDto IndexLogicalDevice { get; set; }

        [DataMember]
        public ArchiverLoadTreeOption LoadTreeOption { get; set; }

        [DataMember]
        public SPTreeNodeDto TreeRoot { get; set; }

        [DataMember]
        public PhysicalDeviceDto DestinationFSDevice { get; set; }

        [DataMember]
        public List<RestoreSecurityInfoWrapper> RestoreSecurityInfos { get; set; }

        [DataMember]
        public List<String> PathMD5List { get; set; }

        [DataMember]
        public List<SearchRequestResult> SearchResultList { get; set; }

        [DataMember]
        public Boolean IsSearchTree { get; set; }

        [DataMember]
        public String ZipFilePassword { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string AdminUrl { get; set; }

        [DataMember]
        public long StorageQuota { get; set; }
        [DataMember]
        public bool UseBackupStorageQuota { get; set; }

        [DataMember]
        public double ResourceQuota { get; set; }
        [DataMember]
        public bool UseBackupResourceQuota { get; set; }

        [DataMember]
        public string SitesGroupName { get; set; }

        [DataMember]
        public bool OverwriteRecyclebin { get; set; }

        [DataMember]
        public bool IncludeListView { get; set; }  //SAAS-12519 增加contract支持List View

        public bool IsEndUserRequest { get; set; }
        public bool IsRecenterExport { get; set; }

        [DataMember]
        public List<EndUserRestoreItem> EndUserRequestItems { get; set; }

        [DataMember]
        public bool IsEndUserRestoreAccessTier { get; set; }

        [DataMember]
        public string EndUserRestoreToFSStorageString { get; set; }

        [DataMember]
        public ArchiveIntegrationModules IntegrationModule { get; set; }

        [DataMember]
        public string TenantGroupId { get; set; }
        [DataMember]
        public List<ToExportUserInfo> NotificationUsers { get; set; }
        [DataMember]
        public DocAveOnline.WebApi.Contracts.PreviewDataParam PreviewParam { get; set; }
        [DataMember]
        public int KeepVersionsNumber { get; set; }
        [DataMember]
        public RestoreDocumentVersionsOption RestoreVersionsOption { get; set; }
        [DataMember]
        public BackupDataSearchContract SearchContract { get; set; }
        [DataMember]
        public bool IsSearchAllRestore { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Archiver Restore Request: ");
            stringBuilder.AppendFormat("Farm Name: {0}, ", this.FarmName);
            stringBuilder.AppendFormat("Site Url: {0}, ", this.SiteUrl);
            stringBuilder.AppendFormat("Restore Option: {0}, ", this.RestoreOption);
            stringBuilder.AppendFormat("Restore FS Option: {0}, ", this.RestoreFSOption);
            stringBuilder.AppendFormat("Archive Job Id: {0}, ", this.ArchiveJobId);
            stringBuilder.AppendFormat("Restore Job Id: {0}, ", this.RestoreJobId);
            stringBuilder.AppendFormat("Tree Root: {0}, ", this.TreeRoot);
            stringBuilder.AppendFormat("Cache Location: {0}, ", this.CacheLocation);
            stringBuilder.AppendFormat("Index Logical Device: {0}", this.IndexLogicalDevice);
            stringBuilder.AppendFormat("Include List View:{0}", this.IncludeListView);
            stringBuilder.AppendFormat("Search Contract:{0}", this.SearchContract);
            return stringBuilder.ToString();
        }
    }
}
