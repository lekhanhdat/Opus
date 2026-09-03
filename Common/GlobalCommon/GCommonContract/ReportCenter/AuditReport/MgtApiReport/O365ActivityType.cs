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
using AvePoint.GCommon.Contract.Common;
using System.Runtime.Serialization;


namespace AvePoint.GCommon.Contract.ReportCenter.AuditReport.MgtApiReport
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum O365ActivityType
    {
        [EnumMember]
        Undefined = 0,

        [EnumMember]
        AzureAD = 1,

        [EnumMember]
        Exchange = 2,

        [EnumMember]
        SharePoint = 3,

        [EnumMember]
        Teams = 4,

        [EnumMember]
        Sway = 5,

        [EnumMember]
        Yammer = 6,

        [EnumMember]
        Stream = 7,

        [EnumMember]
        Flow = 8,

        [EnumMember]
        Project = 9,

        [EnumMember]
        PowerBI = 10,

        [EnumMember]
        SecurityComplianceCenter = 11,

        [EnumMember]
        SkypeForBusiness = 12,

        [EnumMember]
        CRM = 13,

        [EnumMember]
        PowerApps = 14,

        [EnumMember]
        MicrosoftForms = 15,

        [EnumMember]
        ThreatIntelligence = 16,

        [EnumMember]
        AirInvestigation = 17,
    }


    /// <summary>
    /// Azure AD 中的 Office 365 Group 的类型
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum O365GroupType
    {
        [EnumMember]
        Undefined = 0,

        [EnumMember]
        Office365Group = 1,

        [EnumMember]
        DistributionList = 2,

        [EnumMember]
        SecurityGroup = 3,

        [EnumMember]
        MailedEnabledSecurity = 4,
    }
    /// <summary>
    /// SharePoint Online 中的 SP online site 类型
    /// </summary>
    public enum SharePointOnlineSitesType
    {
        [EnumMember]
        Undefined = 0,

        [EnumMember]
        SharePointSites = 1,

        [EnumMember]
        TeamSites = 2,

        [EnumMember]
        ODFBSites = 3,
    }
}
