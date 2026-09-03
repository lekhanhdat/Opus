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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using System;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Office365AppToken.Object
{
    /// <summary>
    /// Profile表Extension字段xml序列化用
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365AppProfileContent : IProfileContent
    {
        [DataMember]
        public AppProfileType Type { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string ApplicationId { get; set; }
        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public byte[] Certificate { get; set; }
        [DataMember]
        public string CertificatePath { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public AzureRegions AzureRegion { get; set; }
        [DataMember]
        public int AppErrorType { get; set; }
        [DataMember]
        public string RedirectURL { get; set; }
        [DataMember]
        public long ModifiedTime { get; set; }
        [DataMember]
        public string AdminUrl { get; set; }
        [DataMember]
        public bool SaveWithoutAuth { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AppProfileType
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        DefaultAzure = 0,
        [EnumMember]
        CustomAzure = 1,
        [EnumMember]
        DefaultSlack = 2,
        [EnumMember]
        CustomSlack = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AppProfileNameAndIdDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
}
