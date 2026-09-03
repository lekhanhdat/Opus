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






using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
namespace AvePoint.GCommon.Contract.Replicator.Object.Message
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicationOption
    {
        [DataMember]
        [XmlElement("SecurityOption")]
        public PermissionOption PermissionOption { get; set; }

        [DataMember]
        public ConfigurationOption ConfigurationOption { get; set; }

        [DataMember]
        public ContentOption ContentOption { get; set; }
    }

    #region Permission Option

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionOption
    {
        [DataMember]
        [XmlAttribute("enableSyncDeleteUserAndGroup")]
        public bool EnableSyncDeleteUserAndGroup { get; set; }

        [DataMember]
        public bool EnableSyncDeletePermission { get; set; }

        [DataMember]
        [XmlAttribute("isReceiveSecurityChange")]
        public bool IsReceiveSecurityChange { get; set; }

        [DataMember]
        [XmlAttribute("isReplicateSecurity")]
        public bool IsReplicateSecurity { get; set; }

        [DataMember]
        public bool IncludeGroupsWithNoPermissions { get; set; }

        [DataMember]
        [XmlElement("SiteCollectionLevel")]
        public PermissionSiteCollectionLevel SiteCollectionLevel { get; set; }

        [DataMember]
        [XmlElement("SiteLevel")]
        public PermissionSiteLevel SiteLevel { get; set; }

        [DataMember]
        [XmlElement("ListLevel")]
        public PermissionListLevel ListLevel { get; set; }

        [DataMember]
        [XmlElement("FolderLevel")]
        public PermissionFolderLevel FolderLevel { get; set; }

        [DataMember]
        [XmlElement("ItemLevel")]
        public PermissionItemLevel ItemLevel { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionSiteCollectionLevel
    {
        [DataMember]
        [XmlAttribute("isUsers")]
        public bool Users { get; set; }

        [DataMember]
        [XmlAttribute("isGroups")]
        public bool Groups { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionSiteLevel
    {
        [DataMember]
        [XmlAttribute("isUsers")]
        public bool Users { get; set; }

        [DataMember]
        [XmlAttribute("isGroups")]
        public bool Groups { get; set; }

        [DataMember]
        [XmlAttribute("isPermissionLevel")]
        public bool PermissionLevels { get; set; }

        [DataMember]
        [XmlAttribute("isSitePermission")]
        public bool SitePermission { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionListLevel
    {
        [DataMember]
        [XmlAttribute("isUsers")]
        public bool Users { get; set; }

        [DataMember]
        [XmlAttribute("isGroups")]
        public bool Groups { get; set; }

        [DataMember]
        [XmlAttribute("isListPermission")]
        public bool ListPermission { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionFolderLevel
    {
        [DataMember]
        [XmlAttribute("isUsers")]
        public bool Users { get; set; }

        [DataMember]
        [XmlAttribute("isGroups")]
        public bool Groups { get; set; }

        [DataMember]
        [XmlAttribute("isFolderLevel")]
        public bool FolderPermission { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionItemLevel
    {
        [DataMember]
        [XmlAttribute("isUsers")]
        public bool Users { get; set; }

        [DataMember]
        [XmlAttribute("isGroups")]
        public bool Groups { get; set; }

        [DataMember]
        [XmlAttribute("isItemPermission")]
        public bool ItemPermission { get; set; }
    }

    #endregion

    #region Configuraton Option

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfigurationOption
    {
        [DataMember]
        [XmlElement("SiteCollectionLevel")]
        public ConfigurationSiteCollectionLevel SiteCollectionLevel { get; set; }

        [DataMember]
        [XmlElement("SiteLevel")]
        public ConfigurationSiteLevel SiteLevel { get; set; }

        [DataMember]
        [XmlElement("ListLevel")]
        public ConfigurationListLevel ListLevel { get; set; }

        [DataMember]
        public bool ReceiveChangesFromDestination { get; set; }

        [DataMember]
        public bool IsReplicateConfiguration { get; set; }

        [DataMember]
        public bool DealWithItemSchema { get; set; }

        [DataMember]
        public bool RestoreItemWithSchema { get; set; }

        [DataMember]
        public bool RestoreItemSchemaOverwriteOption { get; set; }
        [DataMember]
        public ConflictResolution RestoreItemSchemaConflictResolution { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfigurationSiteCollectionLevel
    {
        [DataMember]
        [XmlElement("SiteCollectionFeatures")]
        public ConfigurationConflictRule Features { get; set; }

        [DataMember]
        [XmlElement("searchKeyAndScope")]
        public ConfigurationConflictRule SearchKeyAndScope { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfigurationSiteLevel
    {
        [DataMember]
        [XmlElement("SiteFeatures")]
        public ConfigurationConflictRule SiteFeatures { get; set; }

        [DataMember]
        [XmlElement("SiteColumn")]
        public ConfigurationConflictRule SiteColumn { get; set; }

        [DataMember]
        [XmlElement("Navigation")]
        public ConfigurationConflictRule Navigation { get; set; }

        [DataMember]
        [XmlElement("SiteTemplate")]
        public ConfigurationConflictRule SiteTemplate { get; set; }

        [DataMember]
        [XmlElement("SystemLists")]
        public ConfigurationConflictRule SystemLists { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfigurationListLevel
    {
        [DataMember]
        [XmlElement("ListSettings")]
        public ConfigurationConflictRule ListSettings { get; set; }

        [DataMember]
        [XmlElement("PublicViews")]
        public ConfigurationConflictRule PublicViews { get; set; }

        [DataMember]
        [XmlElement("PersonalViews")]
        public ConfigurationConflictRule PersonalViews { get; set; }

        [DataMember]
        [XmlElement("ListAlerts")]
        public ConfigurationConflictRule ListAlerts { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfigurationConflictRule
    {
        [DataMember]
        [XmlAttribute("conflictAction")]
        public ConflictAction ConflictAction { get; set; }

        [DataMember]
        [XmlAttribute("enable")]
        public bool Enable { get; set; }

        [DataMember]
        [XmlElement("ConflictWinnerRule")]
        public List<ConflictWinnerRule> Rules { get; set; }
    }

    #endregion

    #region Content Option

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentOption
    {
        [DataMember]
        public bool IsReplicateContent { get; set; }

        [DataMember]
        [XmlAttribute("isReceiveDelFromDest")]
        public bool ReceiveDeletionsFromDest { get; set; }

        [DataMember]
        [XmlAttribute("isIncludeUserProfiles")]
        public bool IncludeUserProfiles { get; set; }

        [DataMember]
        public bool IncludeWorkflowDefinition { get; set; }

        [DataMember]
        public bool IncludeWorkflowInstance { get; set; }

        [DataMember]
        public bool IncludeReplicateMetadataService { get; set; }

        [DataMember]
        public bool IncludeReplicateTheRelatedTermsOnly { get; set; }

        [DataMember]
        public bool BackupExtenderOrConnectorData { get; set; }

        [DataMember]
        public SODataOption SODataOption { get; set; }

        [DataMember]
        public ConflictOption ConflictOption { get; set; }

        [DataMember]
        public bool ReplicateFormPageWebPart { get; set; }
    }

    #endregion

    #region Conflict Option

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConflictOption
    {
        [DataMember]
        [XmlAttribute("resolutionType")]
        public ConflictAction ConflictAction { get; set; }

        [DataMember]
        [XmlElement("winnerRules")]
        public List<ConflictWinnerRule> WinnerRules { get; set; }

        [DataMember]
        [XmlElement("notificationInfos")]
        public List<ConfictNotificationInfo> Notifications { get; set; }
    }

    #endregion

}
