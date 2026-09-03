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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using System.Reflection;

namespace AvePoint.GCommon.Contract.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType("GetKnownTypes")]
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

        public static IEnumerable<Type> GetKnownTypes()
        {
            return AveKnownTypeContext.GetKnonwTypes(MethodBase.GetCurrentMethod().DeclaringType);
        }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MessageType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        GranularBackup,
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
        ScanSiteCollection,
        [EnumMember]
        WebApplication,
        [EnumMember]
        PlatformRecoveryRestore,
        [EnumMember]
        DeploymentManagerDashBoard,
        [EnumMember]
        ContentManagerDashBoard,
        [EnumMember]
        HASync,
        [EnumMember]
        HAFailover,
        [EnumMember]
        UpdateAgentProxy,
        [EnumMember]
        GranularEndUserDownload,
        [EnumMember]
        OnlineSiteCollection,
        [EnumMember]
        PersonalSite,//Single Personal Site register
        [EnumMember]
        PersonalSiteScan, //Scan Personal Sites in tenant
        [EnumMember]
        PersonalSiteImport,   //Bulk import personal sites
        [EnumMember]
        OnlineManagement,   //Get Online AdminSiteCollection message
        [EnumMember]
        OnlineCreateSiteCollection,  //Create Online SiteCollection
        [EnumMember]
        Office365Account,  //Office365 Account Validation
        [EnumMember]
        ScanTeamSite,
        [EnumMember]
        SuperUserTest,
        [EnumMember]
        AppTokenTenantInfo,  //Get Tenant Id and Azure Region for App Profile
        [EnumMember]
        ValidateOnlineAppProfile,  //Validate Online App Profile
    }
}
