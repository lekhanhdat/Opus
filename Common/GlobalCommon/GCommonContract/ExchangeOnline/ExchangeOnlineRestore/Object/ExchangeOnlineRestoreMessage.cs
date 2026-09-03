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




namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object
{
    #region == using directives ==
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.GCommon.Contract.Server.ControlPanel.LanguageMapping.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;

    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ERMessage : AveMessage
    {
        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public ExchangeOnlineTreeNodeDto DestTreeNode { get; set; }

        [DataMember]
        public DestStorageInfo DestStorageInfo { get; set; }

        [DataMember]
        public EORestoreConfig Config { get; set; }

        [DataMember]
        public ExchangeRestoreRequest ConfigForMedia { get; set; }

        [DataMember]
        public ServiceDto MediaInfo { get; set; }

        [DataMember]
        public Dictionary<string, BposInfo> EmailBposInfoMap { get; set; }
        [DataMember]
        public Dictionary<string, BposInfo> OutPlaceEmailBposInfoMap { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreConversationType
    {
        [EnumMember]
        Skip = -1,
        [EnumMember]
        Html = 0,
        [EnumMember]
        Original = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EORestoreConfig
    {
        [DataMember]
        public int JobType { get; set; }

        [DataMember]
        public int JobCategory { get; set; }

        [DataMember]
        public EORestoreType RestoreType { get; set; }

        [DataMember]
        public EOConflictResolutionType ContainerConflictResolution { get; set; }

        [DataMember]
        public EOConflictResolutionType ContentConflictResolution { get; set; }

        //[DataMember]
        //public EORestorePermissionOption RestorePermissionOption { get; set; }

        //Use mailbox type to replace IsO365Group in the feature.
        [DataMember]
        public bool IsO365Group { get; set; }

        [DataMember]
        public bool IsMicrosoftTeams { get; set; }

        [DataMember]
        public bool IsYammerGroup { get; set; }

        [DataMember]
        public bool IsSoftDeleted { get; set; }

        //[DataMember]
        //public RA.Contract.Global.Object.MailboxType MailboxType { get; set; }

        [DataMember]
        public LanguageMappingDto LanguageMapping { get; set; }

        [DataMember]
        public UserAndDomainMapping UserMapping { get; set; }

        [DataMember]
        public UserAndDomainMapping DomainMapping { get; set; }

        [DataMember]
        public string ZipFilePassword { get; set; }

        [DataMember]
        public bool ByosRehydrateInt { get; set; }

        [DataMember]
        public bool IsByosRestore { get; set; }
        [DataMember]
        public bool IsSkipRestoreConversation { get; set; }
        [DataMember]
        public RestoreConversationType RestoreConversationType { get; set; }

        [DataMember]
        public bool ReportOnlyHighLevel { get; set; }

        [DataMember]
        public bool NeedMergeConversation { get; set; }
        public JobTags JobTags { get; set; }
        [DataMember]
        public List<string> SkippedErrorCodeList { get; set; }

        [DataMember]
        public bool IsBackupRecoverableItemsVersionsFolder { get; set; }
        [DataMember]
        public PhysicalDeviceDto DestinationFSDevice { get; set; }
        [DataMember]
        public List<ToExportUserInfo> NotificationUsers { get; set; }
        [DataMember]
        public string DestinationDeviceSystemPath { get; set; }
        [DataMember]
        public bool IsSpecifyUser { get; set; }
        [DataMember]
        public List<ToExportUserInfo> SpecifyUserList { get; set; }
        [DataMember]
        public bool UseImportApi { get; set; }
		[DataMember]
        public RestoreDocumentVersionsOption RestoreVersionOption { get; set; }
        [DataMember]
        public int KeepVersionsNumber { get; set; }    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DestStorageInfo
    {
        [DataMember]
        public EORestoreDestFileType DestFileType { get; set; }

        [DataMember]
        public string Prefix { get; set; }

        [DataMember]
        public StoragePolicyDto DestStoragePolicy { get; set; }

        [DataMember]
        public Int32 PostFileFolderCount { get; set; }
    }
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class DestinationInfo
    //{
    //    [DataMember]
    //    public uint Language { get; set; }

    //    [DataMember]
    //    public char ReplaceType { get; set; }

    //    [DataMember]
    //    public string OwerLogin { get; set; }

    //    [DataMember]
    //    public Guid ContentDBId { get; set; }
    //}

}