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
namespace Microsoft365.Authentication.ServiceEndPoint
{
    public class MicrosoftOnlineInstanceDetail
    {
        internal const string AdminWebServiceEndpointUrlFormat = "https://adminwebservice.{0}/provisioningservice.svc";

        internal const string AdminWebServiceSiteNameFormat = "adminwebservice.{0}";

        internal const string OrgIdClientConfigUrlFormat = "clientconfig.{0}";

        internal const string ProvisioningServiceEndpointUrlFormat = "https://provisioningapi.{0}/provisioningwebservice.svc";

        internal const string ProvisioningServiceSiteNameFormat = "provisioningapi.{0}";

        public string AdalAuthorityEndpointUrl
        {
            get;
            private set;
        }

        public string AdalGraphServiceResource
        {
            get;
            private set;
        }

        public string AdalMsGraphServiceResource
        {
            get;
            private set;
        }

        public string AdminWebServiceEndpointUrl
        {
            get;
            private set;
        }

        public string AdminWebServiceSiteName
        {
            get;
            private set;
        }

        public string FederationEndpointDomainNameSuffix
        {
            get;
            private set;
        }

        public string FederationProviderIdentifier
        {
            get;
            private set;
        }

        public string InitialDomainNameSuffix
        {
            get;
            private set;
        }

        public string MsodsEndpointDomainNameSuffix
        {
            get;
            private set;
        }

        public string ProvisioningServiceEndpointUrl
        {
            get;
            private set;
        }

        public string ProvisioningServiceSiteName
        {
            get;
            private set;
        }

        public string OrgIdClientConfigUrl
        {
            get;
            private set;
        }

        public string OrgIdEndpointDomainNameSuffix
        {
            get;
            private set;
        }

        public string DeviceRegistrationServiceEndpoint
        {
            get;
            private set;
        }

        public string ExchangeWebServiceEndpoint
        {
            get;
            private set;
        }

        public AveAzureEnvironment CloudType
        {
            get;
            private set;
        }

        public string EWSServiceUrl { get { return $"{ExchangeWebServiceEndpoint}/EWS/Exchange.asmx"; } }

        public string PSConnectionUrl { get { return $"{ExchangeWebServiceEndpoint}/Powershell/"; } }

        public MicrosoftOnlineInstanceDetail(
            string adalAuthorityEndpointUrl,
            string adalGraphServiceResource,
            string adalMsGraphServiceResource,
            string msodsEndpointDomainNameSuffix,
            string initialDomainNameSuffix,
            string orgIdEndpointDomainNameSuffix,
            string federationEndpointDomainNameSuffix,
            string federationProviderIdentifier,
            string deviceRegistrationServiceEndpoint,
            string exchangeWebServiceEndpoint,
            AveAzureEnvironment cloudType)
        {
            AdalAuthorityEndpointUrl = adalAuthorityEndpointUrl;
            AdalGraphServiceResource = adalGraphServiceResource;
            AdalMsGraphServiceResource = adalMsGraphServiceResource;
            MsodsEndpointDomainNameSuffix = msodsEndpointDomainNameSuffix;
            InitialDomainNameSuffix = initialDomainNameSuffix;
            OrgIdEndpointDomainNameSuffix = orgIdEndpointDomainNameSuffix;
            FederationEndpointDomainNameSuffix = federationEndpointDomainNameSuffix;
            FederationProviderIdentifier = federationProviderIdentifier;
            DeviceRegistrationServiceEndpoint = deviceRegistrationServiceEndpoint;
            AdminWebServiceEndpointUrl = string.Format("https://adminwebservice.{0}/provisioningservice.svc", msodsEndpointDomainNameSuffix);
            AdminWebServiceSiteName = string.Format("adminwebservice.{0}", msodsEndpointDomainNameSuffix);
            ProvisioningServiceEndpointUrl = string.Format("https://provisioningapi.{0}/provisioningwebservice.svc", msodsEndpointDomainNameSuffix);
            ProvisioningServiceSiteName = string.Format("provisioningapi.{0}", msodsEndpointDomainNameSuffix);
            OrgIdClientConfigUrl = string.Format("clientconfig.{0}", orgIdEndpointDomainNameSuffix);
            ExchangeWebServiceEndpoint = exchangeWebServiceEndpoint;
            CloudType = cloudType;
        }
    }
}