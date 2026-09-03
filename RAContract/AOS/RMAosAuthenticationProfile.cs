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
using Cloud.Sdk.Data.AosModern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Aos
{
    [DataContract(Namespace = "www.avepoint.com")]
    public class RMAosAuthenticationProfile
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string AppCertSecretContent { get; set; }
        [DataMember]
        public RMAOSAADEnvironment AADEnvironment { get; set; }
        //[DataMember]
        //public string AppCertContent { get; set; }
        [DataMember]
        public string AppCertSecret { get; set; }
        [DataMember]
        public string AppClientId { get; set; }
        [DataMember]
        public string TenantId { get; set; }
        // Cloud.Sdk.Data.AosModern.IdentityProviderType
        [DataMember]
        public int AppType { get; set; }
    }

    [DataContract(Namespace = "www.avepoint.com")]
    public class RMAosGoogleAppProfile
    {
        [DataMember]
        public string CustomerId { get; set; }
        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public Guid ClientId { get; set; }
        [DataMember]
        public string AOSAppId { get; set; }
        [DataMember]
        public string DomainName { get; set; }
        [DataMember]
        public string DefaultDomainName { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string ProfileName { get; set; }
        [DataMember]
        public int TokenType { get; set; }
        [DataMember]
        public string ServiceAccount { get; set; }
        [DataMember]
        public string PrivateKey { get; set; }
        [DataMember]
        public string TokenServerUrl { get; set; }
        [DataMember]
        public GoogleAuthenticationType AuthenticationType { get; set; }

        public RMAosGoogleAppProfile(string customerId)
        {
            this.CustomerId = customerId;
        }
    }

    [DataContract(Namespace = "www.avepoint.com")]
    public enum RMAOSAADEnvironment 
    {
        AzureCloud = 0,
        AzureChinaCloud = 1,
        USGovernment = 2,
        AzureGermanyCloud = 3,
        USGovernment_DoD = 4,
        AzurePPE = 99,
        None = 255
    }

    [DataContract(Namespace = "www.avepoint.com")]
    public enum RMAOSIdentityTokenType
    {
        AzureAd = 0,
        SharePointOnline = 1,
        ExchangeOnline = 2,
        Expired = 3,
        SalesForce = 4,
        Yammer = 5,
        AzureAD = 6,
        Sandbox = 7,
        CustomAzureApp = 10
    }

    [DataContract(Namespace = "www.avepoint.com")]
    public enum RMAOSIdentityProviderType
    {
        Local = 0,
        SharePointOnline = 6,
        SalesForce = 7,
        Yammer = 8,
        AzureAD = 9,
        Sandbox = 10,
        SharePoint = 11,
        Exchange = 12,
        CustomAzureApp = 15,
        DynamicsAX = 16,
        DynamicsCustomerEngagement = 17
    }

}
