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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.ContentManager.Object;
using AvePoint.GCommon.Contract.Replicator.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.DeploymentManager.Message;

namespace AvePoint.GCommon.Contract.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(CAMessage))]
    [KnownType(typeof(BrowserMessage))]
    [KnownType(typeof(ReplicatorMessage))]
    [KnownType(typeof(CMMessage))]
    [KnownType(typeof(DMMessage))]
    [KnownType(typeof(SCDMMessage))]
    [KnownType(typeof(CompareMessage))]
    public class AveMessage
    {
        [DataMember]
        public ApiObjectModelType ObjectModelType { get; set; }

        [DataMember]
        public BposInfo BposInfo { get; set; }

        [DataMember]
        public ServiceDto AgentInfo { get; set; }

        [DataMember]
        public MessageType MsgType { get; set; }

        [DataMember]
        public String TenantGroupId { get; set; }

        [DataMember]
        public String TenantGroupOwner { get; set; }

        [DataMember]
        public String TenantUser { get; set; }

        [DataMember]
        public ControlDBInfo ControlDBInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ControlDBInfo
    {
        [DataMember]
        public String DatabaseInstance { get; set; }
        [DataMember]
        public String DatabaseName { get; set; }
        [DataMember]
        public String DatabaseUsername { get; set; }
        [DataMember]
        public String DatabasePassword { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MessageType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        GranularBackup,
        [EnumMember]
        ExchangeOnlineBackup,
        [EnumMember]
        QuickBackupGetStatistics,
        [EnumMember]
        GranularRestore,
        [EnumMember]
        PlatformRecoveryBackup,
        [EnumMember]
        PRQuickBackupGetStatistics,
        [EnumMember]
        SiteCollection,
        [EnumMember]
        PersonalSite,
        [EnumMember]
        PersonalSiteImport,
        [EnumMember]
        PersonalSiteReconnect,
        [EnumMember]
        PersonalSiteScan,
        [EnumMember]
        OnlineSiteCollectionUrls,
        [EnumMember]
        OnlineSiteCollection,
        [EnumMember]
        Office365ListUsers,
        [EnumMember]
        WebApplication,
        [EnumMember]
        PlatformRecoveryRestore,
        [EnumMember]
        DeploymentManagerDashBoard,
        [EnumMember]
        ContentManagerDashBoard,
        [EnumMember]
        ExchangeOnlineRestore,
        [EnumMember]
        OnlineUploadTemplateSolution,

        [EnumMember]
        OnlineManagement,
        [EnumMember]
        OnlineCheckAvailableStorageQuota,
        [EnumMember]
        OnlineCreateSiteCollection,

        /// <summary>
        /// auto scan test account
        /// </summary>
        [EnumMember]
        ValidateOnlineAccount,
        /// <summary>
        /// auto scan test account for OneDrive
        /// </summary>
        [EnumMember]
        ValidateOnlineAccountForOneDrive,

        /// <summary>
        /// test if service account have permission to get project data
        /// </summary>
        [EnumMember]
        ValidateProjectAccount,

        #region Deployment Manager
        [EnumMember]
        DesignManager,
        [EnumMember]
        SolutionCenter,
        #endregion

        /// <summary>
        /// Archiver records management
        /// </summary>
        [EnumMember]
        SOValidateSharePointListUrl,

        /// <summary>
        /// Scan O365 group sites
        /// </summary>
        [EnumMember]
        ScanSitesFromGroup,

        /// <summary>
        /// Check Admin Url available
        /// </summary>
        [EnumMember]
        CheckAdminUrl,

        /// <summary>
        /// Check whether site collection changes or not
        /// </summary>
        [EnumMember]
        CheckSiteChange,
        /// <summary>
        /// Check whether user has permission
        /// </summary>
        [EnumMember]
        CheckUserHasPermission
    }
}
