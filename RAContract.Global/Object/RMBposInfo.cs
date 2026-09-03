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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Global.Object
{
    [DataContract]
    public class BposInfo
    {
        [DataMember]
        public String SiteUrl { get; set; }

        [DataMember]
        public BposUserAccountInfo UserAccountInfo { get; set; }

        [DataMember]
        public BPOSMode Mode { get; set; }

        [DataMember]
        public BposConnectionType ConnectionType { get; set; }

        [DataMember]
        public AppType AppType { get; set; }

        [DataMember]
        public MailboxType MailboxType { get; set; }
        [DataMember]
        public string TenantGroupId { get; set; }
    }

    [DataContract]
    public enum MailboxType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        PublicFolder = 1,
        [EnumMember]
        User = 2,
        [EnumMember]
        Group = 3,
        [EnumMember]
        Teams = 4,
    }

    [DataContract]
    public enum BposConnectionType
    {
        [EnumMember]
        ServiceAccount = 0,

        [EnumMember]
        AppToken = 1
    }

    [DataContract]
    public enum AppType
    {
        [EnumMember]
        Office365 = 0,

        [EnumMember]
        SharePoint = 1,

        [EnumMember]
        Exchange = 2,

        [EnumMember]
        CustomAzureApp = 3,
    }

    [DataContract]
    public enum BPOSMode
    {
        [EnumMember]
        Undetermined,

        [EnumMember]
        SecurityTrimming,

        [EnumMember]
        Office365
    }

    [DataContract]
    public enum AADEnvironment
    {
        [EnumMember]
        AzureCloud = 0,
        [EnumMember]
        AzureChinaCloud = 1,
        [EnumMember]
        USGovernment = 2,
        [EnumMember]
        AzureGermanyCloud = 3,
        [EnumMember]
        AzurePPE = 99,
        [EnumMember]
        None = 255
    }

    [DataContract]
    public class BposUserAccountInfo
    {
        [DataMember]
        public string Domain { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public string AdminUrl { get; set; }

        [DataMember]
        public string SecondaryUsername { get; set; }

        [DataMember]
        public string SecondaryPassword { get; set; }
        [DataMember]
        public string SecurityGroup { get; set; }
        [DataMember]
        public string AppClientId { get; set; }
        [DataMember]
        public string AppCertSecret { get; set; }
        //[DataMember]
        //public string AppCertContent { get; set; }
        [DataMember]
        public string AppCertSecretContent { get; set; }
        [DataMember]
        public AADEnvironment AADEnvironment { get; set; }
        [DataMember]
        public string AppId { get; set; }
    }
}
