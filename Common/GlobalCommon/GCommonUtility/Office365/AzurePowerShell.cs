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
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.Client.Framework;
using Microsoft.Online.Administration;
using Microsoft.Online.Administration.Automation;
using Microsoft.Win32;
using System.Diagnostics;
using System.ServiceModel;
using AvePoint.GCommon;

namespace AvePoint.Common.Office365
{
    internal class AzurePowerShell:IDisposable
    {
        private String provisioningServiceSiteName;
        private AveLogger logger = AveLogger.GetInstance(typeof(AzurePowerShell));
        private IProvisioningWebService mProxy;
        private MicrosoftOnlineInstanceDetail mOnlineInstanceDetail;
        private ChannelFactory<IProvisioningWebService> mChannelFactory;
        public static readonly Uri BecWebService = new Uri("https://provisioningapi.microsoftonline.com/provisioningwebservice.svc");

        private readonly String token;
        public String SecurityToken
        {
            get { return this.token; }
        }

        public AzurePowerShell(String username, String password)
        {
            var index = username.IndexOf('@');
            var str = (index == -1) ? username : username.Substring(index + 1);
            var str2 = str.Substring(str.IndexOf('.') + 1);
            MicrosoftOnlineInstanceDetail instanceDetail;
            if (str2.Equals(MicrosoftOnlineInstance.Production.InitialDomainNameSuffix, StringComparison.InvariantCultureIgnoreCase))
            {
                instanceDetail = MicrosoftOnlineInstance.Production;
            }
            else instanceDetail = MicrosoftOnlineInstance.FromDomainOrPrincipalName(username);
            if (instanceDetail != null)
            {
                this.provisioningServiceSiteName = instanceDetail.ProvisioningServiceSiteName;
                this.token = LogOnServiveAndGetToken(username, password,
                    instanceDetail.FederationProviderIdentifier, string.Format("ps.{0}", instanceDetail.MsodsEndpointDomainNameSuffix));
            }
            else this.token = LogOnServiveAndGetToken(username, password, null, null);
        }

        private String GetFedrationProviderIdentifier()
        {
            var federationProviderIdentifierFromRegistry = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\MSOnlinePowerShell\Path",
                    "FederationProviderIdentifier", null) as String;
            return federationProviderIdentifierFromRegistry;
        }

        private String GetBecWebServiceLogonSiteName()
        {
            var obj2 = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\MSOnlinePowerShell\Path",
              "WebServiceUrl", 0);
            try
            {
                if (obj2 != null && !String.IsNullOrEmpty(obj2.ToString()))
                {
                    provisioningServiceSiteName = new Uri(obj2.ToString()).Host;
                    return provisioningServiceSiteName.Replace("provisioningapi.", "ps.");
                }
            }
            catch (UriFormatException ex)
            {
                logger.Error("Invalid configuration:{0}", ex.ToString());
            }
            return String.Empty;
        }

        private String LogOnServiveAndGetToken(String username, String password,
            String federationProviderIdnetifier, String provisionSiteName)
        {
            username = GetOffice365UserEmail(username);
            if (string.IsNullOrEmpty(federationProviderIdnetifier))
                federationProviderIdnetifier = GetFedrationProviderIdentifier();
            if (string.IsNullOrEmpty(provisionSiteName))
                provisionSiteName = GetBecWebServiceLogonSiteName();
            var result = String.Empty;
            var liveIdManager = new LiveIdentityManager();
            try
            {
                result = liveIdManager.LogOnUser(federationProviderIdnetifier, username, password,
               provisionSiteName, "MCMBI", null);
            }
            catch (Exception ex)
            {
                logger.Error("LogOnUser error:{0}", ex.ToString());
            }
            return result;
        }

        private String GetOffice365UserEmail(String externalUser)
        {
            var userEmail = externalUser;
            if (userEmail.Contains("#EXT#@"))
            {
                var temp = userEmail.Substring(0, userEmail.IndexOf("#EXT#@", StringComparison.OrdinalIgnoreCase));
                var lastPosition = temp.LastIndexOf('_');
                if (lastPosition > 0)
                    userEmail = temp.Substring(0, lastPosition) + "@" + temp.Substring(lastPosition + 1, temp.Length - lastPosition - 1);
            }
            return userEmail;
        }

        //public List<Dictionary<string, object>> GetOffice365Domains()
        //{
        //    List<string> retryUrls = new List<string>();
        //    int retriedUrlIndex = 0;
        //    do
        //    {
        //        InitProxy(retryUrls);
        //        using (OperationContextScope contextScope = new OperationContextScope(mProxy as IContextChannel))
        //        {
        //            try
        //            {
        //                ListDomainsRequest request = new ListDomainsRequest();
        //                request.SearchFilter = new DomainSearchFilter();
        //                request.BecVersion = Microsoft.Online.Administration.Version.Version16;
        //                ListDomainsResponse response = mProxy.ListDomains(request);
        //                List<Microsoft.Online.Administration.Domain> domains = response.ReturnValue;
        //                return domains.Select(domain => new Dictionary<String, Object>
        //                    {
        //                        {"Name", domain.Name},
        //                        {"IsInitial", domain.IsInitial},
        //                        {"IsDefault", domain.IsDefault},
        //                        {"Status", domain.Status},
        //                        {"RootDomain", domain.RootDomain},
        //                        {"Capabilities", domain.Capabilities},
        //                        {"Authentication", domain.Authentication}
        //                    }).ToList();
        //            }
        //            catch (FaultException<BindingRedirectionException> e)
        //            {
        //                if (retryUrls.Count == 0)
        //                {
        //                    retryUrls = (e as FaultException<BindingRedirectionException>).Detail.Locations;
        //                }
        //                else
        //                {
        //                    retriedUrlIndex++;
        //                }
        //                InitProxy(retryUrls, retriedUrlIndex);
        //            }
        //            catch (Exception e)
        //            {
        //                logger.Warn("Failed to get O365 domains, error detail : {0}", e.ToString());
        //                return null;
        //            }
        //        }
        //    }
        //    while (retriedUrlIndex < retryUrls.Count);
        //    logger.Warn("Failed to get O365 domains");
        //    return null;
        //}

        //public string GetOffice365Domain()
        //{
        //    List<Dictionary<string, object>> domainProperties = GetOffice365Domains();
        //    try
        //    {
        //        if (domainProperties != null)
        //        {
        //            foreach (Dictionary<string, object> domainProp in domainProperties)
        //            {
        //                if (Convert.ToBoolean(domainProp["IsInitial"]))
        //                {
        //                    return domainProp["Name"].ToString();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error("Failed to get O365 domain by user account, error detail : {0}", e.ToString());
        //        return null;
        //    }
        //    return string.Empty;
        //}

        //private void InitProxy(IList<string> retryUrls, int retriedUrlIndex = 0)
        //{
        //    try
        //    {
        //        WSHttpBinding binding = new WSHttpBinding(SecurityMode.Transport, false) { MaxReceivedMessageSize = 0x7fffffff };
        //        binding.ReceiveTimeout = new TimeSpan(0, 5, 0);
        //        binding.SendTimeout = new TimeSpan(0, 5, 0);
        //        binding.OpenTimeout = new TimeSpan(0, 5, 0);
        //        string siteUrl = retryUrls.Count == 0 ? GetBecWebServiceUri() : retryUrls[retriedUrlIndex];
        //        EndpointAddress endpoint = new EndpointAddress(siteUrl);
        //        mChannelFactory = new ChannelFactory<IProvisioningWebService>(binding, endpoint);

        //        BecWebServiceInspector becWebServiceInspector = new BecWebServiceInspector(token);
        //        BecWebServiceCustomBehavior becWebServiceCustomBehavior = new BecWebServiceCustomBehavior(becWebServiceInspector);
        //        mChannelFactory.Endpoint.Behaviors.Add(becWebServiceCustomBehavior);
        //        mProxy = mChannelFactory.CreateChannel();
        //    }
        //    catch (Exception e1)
        //    {
        //        logger.Warn("Failed to initial AzurePowerShellRequest, error detail : {0}", e1.ToString());
        //        return;
        //    }
        //}

        private string GetBecWebServiceUri()
        {
            if (mOnlineInstanceDetail != null)
            {
                return mOnlineInstanceDetail.ProvisioningServiceEndpointUrl;
            }
            else
            {
                return AzurePowerShell.BecWebService.ToString();
            }
        }

        public void Dispose()
        {
            if(mChannelFactory != null)
            {
                mChannelFactory.Close();
            }
        }
    }
}
