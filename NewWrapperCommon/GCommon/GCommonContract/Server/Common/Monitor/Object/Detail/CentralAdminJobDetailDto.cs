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
using System.Text;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CentralAdminJobDetailDto : JobDetailDto
    {
        /// <summary>
        /// 用于表示Job的类型
        /// </summary>
        [DataMember]
        public Int32 PlanType { get; set; }

        #region DeadAccount

        /// <summary>
        /// For Dead Account Cleaner
        /// </summary>
        [DataMember]
        public string DeadAccountType { get; set; }
        [DataMember]
        public string DeadAccountSiteURL { get; set; }
        [DataMember]
        public string DeadAccountUsername { get; set; }
        [DataMember]
        public string DeadAccountDisplayName { get; set; }
        [DataMember]
        public string DeadAccountPermissionLevels { get; set; }
        [DataMember]
        public string DeadAccountCloneUserPermissions { get; set; }
        [DataMember]
        public string DeadAccountComment { get; set; }

        #endregion

        #region Import Configuration File

        /// <summary>
        /// For Import Configuration File
        /// </summary>
        [DataMember]
        public string ImportConfigurationFilePath { get; set; }
        [DataMember]
        public string ImportConfigurationFileNameOrTitle { get; set; }
        [DataMember]
        public string ImportConfigurationFileLevel { get; set; }
        [DataMember]
        public string ImportConfigurationFileAccountType { get; set; }
        [DataMember]
        public string ImportConfigurationFileUserAndGroup { get; set; }
        [DataMember]
        public string ImportConfigurationFileDisplayName { get; set; }
        //For POC-4336 SecuritySearchResult添加Email列
        [DataMember]
        public string ImportConfigurationFileEmail { get; set; }
        [DataMember]
        public string ImportConfigurationFilePermissionLevels { get; set; }
        [DataMember]
        public string ImportConfigurationFileOriginalPermissionLevels { get; set; }
        [DataMember]
        public string ImportConfigurationFileChangeType { get; set; }
        [DataMember]
        public string ImportConfigurationFileComment { get; set; }

        #endregion

        #region Check Broken Link

        /// <summary>
        /// For Check Broken Link
        /// </summary>
        [DataMember]
        public string CheckBrokenLinkSiteURL { get; set; }
        [DataMember]
        public string CheckBrokenLinkLinkURL { get; set; }
        [DataMember]
        public string CheckBrokenLinkLinkedFrom { get; set; }
        [DataMember]
        public string CheckBrokenLinkProtocol { get; set; }
        [DataMember]
        public string CheckBrokenLinkContentType { get; set; }
        [DataMember]
        public string CheckBrokenLinkSize { get; set; }
        [DataMember]
        public string CheckBrokenLinkCharset { get; set; }
        [DataMember]
        public string CheckBrokenLinkAccessTime { get; set; }
        [DataMember]
        public string CheckBrokenLinkLocation { get; set; }
        [DataMember]
        public string CheckBrokenLinkStatus { get; set; }
        [DataMember]
        public string CheckBrokenLinkComment { get; set; }

        #endregion

        #region Delete Orphan Site

        /// <summary>
        /// For Delete Orphan Site
        /// </summary>
        [DataMember]
        public string DeleteOrphanSiteSiteCollectionURL { get; set; }
        [DataMember]
        public string DeleteOrphanSiteSiteCollectionTitle { get; set; }
        [DataMember]
        public string DeleteOrphanSiteSiteCollectionGUID { get; set; }
        [DataMember]
        public string DeleteOrphanSiteDatabaseName { get; set; }
        [DataMember]
        public string DeleteOrphanSiteSQLServerName { get; set; }
        [DataMember]
        public string DeleteOrphanSiteComment { get; set; }

        #endregion

        #region Search Web Part

        /// <summary>
        /// For Search Web Part
        /// </summary>
        [DataMember]
        public string SearchWebPartTitle { get; set; }
        [DataMember]
        public string SearchWebPartOrder { get; set; }
        [DataMember]
        public string SearchWebPartZone { get; set; }
        [DataMember]
        public string SearchWebPartPageURL { get; set; }
        [DataMember]
        public string SearchWebPartWebURL { get; set; }
        [DataMember]
        public string SearchWebPartWebTitle { get; set; }
        [DataMember]
        public string SearchWebPartTemplate { get; set; }

        #endregion

        #region Web Part Template

        /// <summary>
        /// For Web Part Template
        /// </summary>
        [DataMember]
        public string WebPartTemplate { get; set; }
        [DataMember]
        public string WebPartTemplateUsage { get; set; }
        [DataMember]
        public string WebPartTemplateCreatedBy { get; set; }
        [DataMember]
        public string WebPartTemplateLastModifiedBy { get; set; }

        #endregion

        #region Import Configuration File Edit Group

        [DataMember]
        public string EditGroupSiteURL { get; set; }
        [DataMember]
        public string EditGroupSiteTitle { get; set; }
        [DataMember]
        public string EditGroupType { get; set; }
        [DataMember]
        public string EditGroupGroupName { get; set; }
        [DataMember]
        public string EditGroupUserName { get; set; }
        [DataMember]
        public string EditGroupDisplayName { get; set; }
        [DataMember]
        public string EditGroupAction { get; set; }

        #endregion

        #region Move SiteCollection
        [DataMember]
        public string MoveSiteCollectionURL { get; set; }
        [DataMember]
        public string MoveSiteCollectionTitle { get; set; }
        [DataMember]
        public string SiteCollectionOriginalDataBase { get; set; }
        [DataMember]
        public string SiteCollectionDestinationDataBase { get; set; }
        [DataMember]
        public string MoveSiteCollectionStatus { get; set; }
        [DataMember]
        public string MoveSiteCollectionComment { get; set; }
        #endregion

        #region New Web App

        [DataMember]
        public string NewWebAppName { get; set; }
        [DataMember]
        public string NewWebAppURL { get; set; }
        [DataMember]
        public string NewWebAppPort { get; set; }
        [DataMember]
        public string NewWebAppStatus { get; set; }
        [DataMember]
        public string NewWebAppComment { get; set; }

        #endregion

        #region Search Duplicate File

        [DataMember]
        public string DuplicateFileFileKeyName { get; set; }
        [DataMember]
        public string DuplicateFileFileKeySize { get; set; }
        [DataMember]
        public string DuplicateFileAverageSize { get; set; }
        [DataMember]
        public string DuplicateFileFileNumber { get; set; }
        [DataMember]
        public string DuplicateFileFileURL { get; set; }
        [DataMember]
        public string DuplicateFileFileOrAttachment { get; set; }
        [DataMember]
        public string DuplicateFileFileSize { get; set; }
        [DataMember]
        public string DuplicateFileFileVersion { get; set; }
        [DataMember]
        public string DuplicateFileModifiedBy { get; set; }

        #endregion

        #region Push Inherit Down
        [DataMember]
        public string PushInheritNodeName { get; set; }
        [DataMember]
        public string PushInheritNodeLevel { get; set; }
        [DataMember]
        public string PushInheritNodeUrl { get; set; }
        [DataMember]
        public string PushInheritNodeOriginalInherit { get; set; }
        [DataMember]
        public string PushInheritNodeInherit { get; set; }
        [DataMember]
        public string PushInheritStatus { get; set; }
        [DataMember]
        public string PushInheritComment { get; set; }
        #endregion

        #region Clone User Permission

        [DataMember]
        public string ClonePermUrl { get; set; }

        [DataMember]
        public string ClonePermLevel { get; set; }

        [DataMember]
        public string ClonePermTitle { get; set; }

        [DataMember]
        public string ClonePermSourceUserName { get; set; }

        [DataMember]
        public string ClonePermSourceUserPerms { get; set; }

        [DataMember]
        public string ClonePermDestUserName { get; set; }

        [DataMember]
        public string ClonePermDestUserOriginalPerms { get; set; }

        [DataMember]
        public string ClonePermDestUserCurrentPerms { get; set; }

        #endregion

        #region Change Column Metadata Import
        [DataMember]
        public string ChangeColumnMetadataImportUrl { get; set; }

        [DataMember]
        public string ChangeColumnMetadataImportName { get; set; }

        [DataMember]
        public string ChangeColumnMetadataImportSuccessfulUpdateColumns { get; set; }

        [DataMember]
        public string ChangeColumnMetadataImportFailedUpdateColumns { get; set; }

        [DataMember]
        public string ChangeColumnMetadataImportStatus { get; set; }

        [DataMember]
        public string ChangeColumnMetadataImportComment { get; set; }
        #endregion

        #region Apply Profile
        [DataMember]
        public string ApplyProfileScope { get; set; }

        [DataMember]
        public string ApplyProfileSharePointObjectName { get; set; }

        [DataMember]
        public string ApplyProfileSharePointObjectLevel { get; set; }

        [DataMember]
        public string ApplyProfileUrl { get; set; }

        [DataMember]
        public string ApplyProfileSharePointObjectDetail { get; set; }

        [DataMember]
        public string ApplyProfileRuleName { get; set; }

        [DataMember]
        public string ApplyProfileRuleDescription { get; set; }

        [DataMember]
        public string ApplyProfileUsedEventType { get; set; }

        [DataMember]
        public string ApplyProfileAutoUndoSetting { get; set; }

        [DataMember]
        public string ApplyProfileCanUndoSetting { get; set; }

        [DataMember]
        public string ApplyProfileRuleParameter { get; set; }

        [DataMember]
        public string ApplyProfileProfileName { get; set; }

        [DataMember]
        public string ApplyProfileStatus { get; set; }

        [DataMember]
        public string ApplyProfileComment { get; set; }

        [DataMember]
        public string ApplyProfileParameterEvents { get; set; }

        [DataMember]
        public string ApplyProfileFilterPolicyInfoName { get; set; }
        #endregion

        #region Delete Expired Group

        [DataMember]
        public string DeleteExpiredGroupURL { get; set; }
        [DataMember]
        public string DeleteExpiredGroupGroupName { get; set; }
        [DataMember]
        public string DeleteExpiredGroupExpiredDate { get; set; }
        [DataMember]
        public string DeleteExpiredGroupUsers { get; set; }
        [DataMember]
        public string DeleteExpiredGroupPermission { get; set; }
        [DataMember]
        public string DeleteExpiredGroupStatus { get; set; }
        [DataMember]
        public string DeleteExpiredGroupComment { get; set; }

        #endregion

        #region Delete Site Collection

        [DataMember]
        public string DeleteSiteCollecionURL { get; set; }

        [DataMember]
        public string DeleteSiteCollecionLevel { get; set; }

        [DataMember]
        public string DeleteSiteCollecionTitle { get; set; }

        [DataMember]
        public string DeleteSiteCollecionStatus { get; set; }

        [DataMember]
        public string DeleteSiteCollecionComment { get; set; }

        #endregion
    }
}
