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
using AvePoint.GCommon.Contract.AveLicense;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.LicenseManager
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LicenseJobMessageContract
    {
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public ServiceDto AgentInfo { get; set; }
        [DataMember]
        public List<Office365AccountInfo> Office365AccountInfos { get; set; }
        [DataMember]
        public AveSPVersion AveSPVersion { get; set; }
        [DataMember]
        public bool IsRemoteMessage { get; set; }
        [DataMember]
        public List<MessageInfo> MessageInfos { get; set; }
        [DataMember]
        public List<RemoteSiteCollection> LocalSimulateRemoteSiteCollections { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MessageInfo
    {
        [DataMember]
        public List<UserInfo> UserInfos { get; set; }
        [DataMember]
        public ErrorInfo ErrorInfo { get; set; }
        [DataMember]
        public string CAUrlOrFarmName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserInfo
    {
        [DataMember]
        public string LoginName { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string UserPrincipalName { get; set; }
        [DataMember]
        public List<ServiceInfo> LicensedServiceInfos { get; set; }
        [DataMember]
        public bool IsRemoteUser { get; set; }
        [DataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServiceInfo
    {
        [DataMember]
        public string ServiceName { get; set; }
        /// <summary>
        /// For Extention, No used currently.
        /// </summary>
        [DataMember]
        public Guid ServicePlanId { get; set; }
        [DataMember]
        public string SkuPartNumber { get; set; }
    }
}
