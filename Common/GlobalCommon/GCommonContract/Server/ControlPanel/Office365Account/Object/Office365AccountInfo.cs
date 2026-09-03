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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.CentralAdmin.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public struct ScanSiteCollectionParameter
    {
        [DataMember]
        public Office365AccountInfo Account { get; set; }

        [DataMember]
        public bool SaveAsProfile { get; set; }

        [DataMember]
        public Office365ScanSitesResult ScanSitesResult { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365AccountInfo
    {
        [DataMember]
        public String Id { get; set; }
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public String UserName { get; set; }
        [DataMember]
        public String Password { get; set; }
        [DataMember]
        public String AdminUrl { get; set; }
    }

    /// <summary>
    /// Profile表Extension字段xml序列化用
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365AccountProfile : IProfileContent
    {
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public String UserName { get; set; }
        [DataMember]
        public String AdminUrl { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AppProfile
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string ProfileName { get; set; }
        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public string CustomerId { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string AdminUrl { get; set; }
        [DataMember]
        public AppType IdentityProviderType { get; set; }
        //[DataMember]
        //public string AppCertContent { get; set; }
        [DataMember]
        public string AppCertSecret { get; set; }
        [DataMember]
        public string AppClientId { get; set; }
        [DataMember]
        public string AppCertSecretContent { get; set; }
        [DataMember]
        public AADEnvironment AADEnvironment { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServiceAccount
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string AdminUrl { get; set; }
        [DataMember]
        public int Role { get; set; }
        [DataMember]
        public bool EnableMFA { get; set; }
        [DataMember]
        public string DomainName { get; set; }
        [DataMember]
        public AADEnvironment AADEnvironment { get; set; }
        public override string ToString()
        {
            return $"Name: {Name}, Status: {Status}, EnableMFA:{EnableMFA}, Environment:{AADEnvironment}";
        }
    }
}