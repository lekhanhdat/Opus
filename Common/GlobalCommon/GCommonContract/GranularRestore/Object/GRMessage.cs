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




namespace AvePoint.GCommon.Contract.GranularRestore.Object
{
    #region == using directives ==
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Server.ControlPanel.LanguageMapping.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object;
    using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
    using AvePoint.GCommon.Contract.SharePointBrowser.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GRMessage : AveMessage
    {
        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public SPTreeNodeDto DestTreeNode { get; set; }

        [DataMember]
        public PhysicalDeviceDto DestFSInfo { get; set; }

        [DataMember]
        public StoragePolicyDto DestStoragePolicy { get; set; }

        [DataMember]
        public RestoreConfig Config { get; set; }

        [DataMember]
        public GranularRestoreRequest ConfigForMedia { get; set; }

        [DataMember]
        public ArchiverRestoreRequest ArchiverConfigForMedia { get; set; }   //SAAS-10617 support Site Collection

        [DataMember]
        public ServiceDto MediaInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RestoreConfig
    {
        [DataMember]
        public int JobType { get; set; }

        [DataMember]
        public int JobCategory { get; set; }

        [DataMember]
        public BackupLevel BackupLevel { get; set; }

        [DataMember]
        public RestoreType RestoreType { get; set; }

        /// <summary> if True, means Attach, else Merge。 </summary>
        [DataMember]
        public bool RestoreContentsToSub { get; set; }

        [DataMember]
        public bool IncludeItemsReport { get; set; }

        #region == Item level ==
        [DataMember]
        public bool IncludingRecycleBinData { get; set; }

        [DataMember]
        public RestoreVersionSetting RestoreVersionSetting { get; set; }

        [DataMember]
        public bool IsIncludeSharedLinks { get; set; }

        [DataMember]
        public int VersionCount { get; set; }

        [DataMember]
        public BackupRestoreWorkflow WorkflowState { get; set; }

        [DataMember]
        public ItemDependencyOption ItemDependencyType { get; set; }

        [DataMember]
        public bool IncludeUserProfile { get; set; }

        [DataMember]
        public bool IncludeListView { get; set; }

        /// <summary> if True, means Skip Special Lists Under PersonalSite。 </summary>
        [DataMember]
        public bool SkipHiddenList { get; set; }

        [DataMember]
        public RestoreThreadType RestoreThreadType { get; set; }

        /// <summary> whether or not exclude group or user without permission. </summary>
        [DataMember]
        public bool ExcludeGroupWithoutPermissions { get; set; }

        [DataMember]
        public bool IncludeVersion { get; set; }

        #endregion ==

        [DataMember]
        public ConflictResolutionType ContainerConflictResolution { get; set; }

        [DataMember]
        public ConflictResolutionType ContentConflictResolution { get; set; }

        [DataMember]
        public ConflictResolutionType AppsConflictResolution { get; set; }

        [DataMember]
        public DestinationInfo DestinationInfo { set; get; }

        [DataMember]
        public LanguageMappingDto LanguageMapping { get; set; }

        [DataMember]
        public UserAndDomainMapping UserMapping { get; set; }

        [DataMember]
        public GlobalRestoreOption RestoreGlobalOption { set; get; }

        [DataMember]
        public bool IncludeCustomPropertyBags { get; set; }

        [DataMember]
        public bool IncludeProjectsData { get; set; }
        [DataMember]
        public bool IsSpecifyUser { get; set; }
        [DataMember]
        public AvePoint.GCommon.Contract.Server.Common.BackupDataSearch.ToExportUserInfo SpecifyUser { get; set; }
        [DataMember]
        public bool IsRestoreToSPOLibOrFolder { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DestinationInfo
    {
        [DataMember]
        public uint Language { get; set; }

        [DataMember]
        public char ReplaceType { get; set; }

        [DataMember]
        public string OwerLogin { get; set; }

        [DataMember]
        public Guid ContentDBId { get; set; }
    }

}