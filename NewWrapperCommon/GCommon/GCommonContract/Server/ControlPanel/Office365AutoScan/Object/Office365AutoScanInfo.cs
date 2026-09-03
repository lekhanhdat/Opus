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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365AppToken.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Office365AutoScan.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365AutoScanProfileInfo : IProfileContent
    {
        [DataMember]
        public String Id { get; set; }

        [DataMember]
        public String PlanId { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public String Description { get; set; }

        [DataMember]
        public ScanType ScanType { get; set; }

        [DataMember]
        public NameAndIdDto AccountInfo { get; set; }

        [DataMember]
        public ScheduleDto ScheduleDto { get; set; }

        [DataMember]
        public RegistrationSetting RegistrationSetting { get; set; }

        [DataMember]
        public AuthorizeType AuthorizeType { get; set; }

        [DataMember]
        public AppProfileNameAndIdDto AppProfileInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365AutoScanMessage
    {
        [DataMember]
        public ServiceDto AgentInfo { get; set; }

        [DataMember]
        public String PlanId { get; set; }

        [DataMember]
        public String JobId { get; set; }

        [DataMember]
        public ScanType ScanType { get; set; }

        [DataMember]
        public Office365AccountInfo AccountInfo { get; set; }

        [DataMember]
        public List<FilterPolicyInfo> FilterPolicyList { get; set; }

        [DataMember]
        public AuthorizeType AuthorizeType { get; set; }

        [DataMember]
        public AppTokenInfo AppTokenInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RegistrationSetting
    {
        //[For HTML_DEV]对应GUI上的isAllContainer,如果选中此RadioBtn 使用下面的ContainerForAllSites储存 
        [DataMember]
        public bool AddAllSitesToOneContainer { get; set; }//Register all sites to one container
        [DataMember]
        public RemoteWebApplication ContainerForAllSites { get; set; }

        [DataMember]
        public List<RegistrationPolicy> RegistrationPolicyList { get; set; }

        [DataMember]
        public bool AddOthersToSiteGroup { get; set; }//Add other sites to the container

        [DataMember]
        public RemoteWebApplication SiteGroup { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RegistrationPolicy
    {
        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public NameAndIdDto FilterPolicy { get; set; }

        [DataMember]
        public RemoteWebApplication SiteGroup { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScanType
    {
        [EnumMember]
        Normal = 0,

        [EnumMember]
        OneDrive = 1,

        [EnumMember]
        TeamSite = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AutoScanPageData
    {
        [DataMember]
        public List<NameAndIdDto> AccountList { get; set; }

        [DataMember]
        public List<NameAndIdDto> FilterList { get; set; }

        [DataMember]
        public List<RemoteWebApplication> SiteGroupList { get; set; }
    }
}    
