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
namespace ExchangeUtility
{
    using AvePoint.RA.Common.Configurations;
    using AvePoint.RA.CommonUtil;
    using Microsoft.Exchange.WebServices.Data;
    using Microsoft365.Contract;
    using System;

    public abstract class ExchangeObjectBase
    {
        private static RALogger _logger = RALogger.GetInstance(typeof(ExchangeObjectBase));

        protected const int DEFAULT_RETRY = 5;
        private const ExchangeVersion EXCHANGE_VERSION = ExchangeVersion.Exchange2016;
        private const int TIMEOUT = 1500000;

        protected static readonly PropertySet DEFAULT_FOLDER_PROPERTY_SET =
        EXCHANGE_VERSION >= ExchangeVersion.Exchange2013 ?
        new PropertySet(BasePropertySet.FirstClassProperties,
            new PropertyDefinitionBase[]
            {
                    new ExtendedPropertyDefinition(26293, MapiPropertyType.String),//Path
                    FolderSchema.WellKnownFolderName,
            }) :
        new PropertySet(BasePropertySet.FirstClassProperties, new PropertyDefinitionBase[]
            {
                    new ExtendedPropertyDefinition(26293, MapiPropertyType.String),//Path
            });
        public string UserName { get { return this.AuthObject.UserName; } }

        public string ServiceUrl
        {
            get { return this.AuthObject.EWSServiceUrl; }
            protected set { this.AuthObject.EWSServiceUrl = value; }
        }

        public AuthObject AuthObject { get; private set; }
        /// <summary>
        /// Enable scp lookup when autodiscover. It is recommended to enable scp lookup only for in-network autodiscover.
        /// For exchange online, it is always out-network.
        /// </summary>
        public bool EnableScpLookup { get; set; }
        public static string UserAgent { get; set; }

        static ExchangeObjectBase()
        {
            UserAgent = string.Format("ISV|AvePoint|CloudRecords/{0}", ExchangeGlobalConfig.ProductVersion);
            _logger.Debug($"User Agent: {UserAgent}");
            //$"ISV|AvePoint|RECO/{DateTime.UtcNow.ToString("yyyyMM")}|Interactive";
            //$@"AvePoint\{ExchangeGlobalConfig.ProductVersion} RECO";
        }


        protected ExchangeObjectBase(AuthObject authObj)
        {
            if (authObj == null) throw new ArgumentNullException("authObj");
            this.AuthObject = authObj;
        }


        protected ExchangeService CreateExchangeService()
        {
            return CreateExchangeService(DEFAULT_RETRY);
        }

        protected ExchangeService CreateExchangeService(int maxRetryCount)
        {
            var service = new ExchangeService(EXCHANGE_VERSION);
            this.AuthObject.BindToExchangeService(service);
            service.Timeout = TIMEOUT;
            service.EnableScpLookup = this.EnableScpLookup;
            service.UserAgent = UserAgent;
            service.ClientRequestId = Guid.NewGuid().ToString();
            service.ReturnClientRequestId = true;
            SetRetry(maxRetryCount, service);
            //EnableEWSTraceLog(service);
            return service;
        }

        protected void SetServiceUrl(ExchangeService service, string originalMailboxAddress)
        {
            if (!string.IsNullOrEmpty(this.ServiceUrl))
            {
                service.Url = new Uri(this.ServiceUrl);
            }
            else
            {
                var userName = this.UserName;
                if(string.IsNullOrWhiteSpace(userName))
                {
                    userName = originalMailboxAddress;
                }
                //TODO: Replace Graph API
                RunAutoDiscoverUrl(service, userName);
                this.ServiceUrl = service.Url.AbsoluteUri;
            }
        }

        protected void RunAutoDiscoverUrl(ExchangeService service, string emailAddress)
        {
            _logger.Warn("Run autodiscover.");
            try
            {
                service.AutodiscoverUrl(emailAddress, new AutoDiscoverCallback().RedirectionUrlValidationCallback).GetAwaiter().GetResult();
                if (string.IsNullOrWhiteSpace(service.Url?.AbsoluteUri))
                {
                    throw new Exception("Autodiscover service url is null");
                }
                else
                {
                    _logger.Info($"1. ExchangeService.AutoDiscoverUrl, serivce url: {service.Url.AbsoluteUri}");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn("Autodiscover service execute with exception, reason: {0}", ex.ToString());
                var url = AzureEnvironment.FromDomainOrPrincipalName(emailAddress)?.EWSServiceUrl;
                if (!string.IsNullOrEmpty(url))
                {
                    service.Url = new Uri(url);
                    _logger.Info($"2. Assume email domain is on Office365, AzureEnvironment.FromDomainOrPrincipalName, service url: {service.Url.AbsoluteUri}");
                }
                else
                {
                    service.Url = new Uri(AzureEnvironment.DefaultCloud.EWSServiceUrl);
                    _logger.Info($"3. Use default service url: {service.Url.AbsoluteUri}");
                }
            }
        }

        //private void EnableEWSTraceLog(ExchangeService service)
        //{
        //    if (ExchangeGlobalConfig.EnableTraceLog)
        //    {
        //        service.EnableTraceLog();
        //    }
        //}

        private void SetRetry(int maxRetryCount, ExchangeService service)
        {
            service.RetryController = AssemblyRetryController(maxRetryCount, service);
        }

        private IRetryable AssemblyRetryController(int maxRetryCount, ExchangeService service)
        {
            #region Data flow for basic retry implement
            /// 
            ///  User context                                                   User context 
            ///             ∨                                                  ∧
            /// ExceptionWrapper (Wrap wellknown error to user friendly format) ExceptionWrapper
            ///             ∨                                                  ∧
            ///         Retryable     (wait and retry n times when error)       Retryable
            ///             ∨                                                  ∧
            /// AADTokenRefresher     (Refress access token when expired)       AADTokenRefresher
            ///             ∨                                                  ∧
            ///            Exchange Web Service(Microsoft.Exchange.WebServive.DLL)
            #endregion
            IRetryable retryable = null;
            if (this.AuthObject.AuthType == AuthObjectType.AccessToken)
            {
                retryable = new AADTokenRefresher(this.AuthObject as AppTokenAuthObject, service, retryable); //refresh add token when expired
            }           
            retryable = new Retryable(maxRetryCount, retryable); //retry for EWS error
            retryable = new ExceptionWrapper(new FormattedMessageException.Context { AuthObject = this.AuthObject }, retryable); //wrapper EWS exception and format msg
            return retryable;
        }

        //protected ExchangeServiceBinding CreateExchangeServiceBinding(string xAnchorMailbox = null)
        //{
        //    var serviceBinding = new ExchangeServiceBindingV2();
        //    this.AuthObject.BindToExchangeServiceBinding(serviceBinding, xAnchorMailbox);
        //    serviceBinding.RequestServerVersionValue = new RequestServerVersion();
        //    serviceBinding.RequestServerVersionValue.Version = ExchangeVersionType.Exchange2010_SP1;
        //    serviceBinding.Timeout = TIMEOUT;

        //    return serviceBinding;
        //}

        protected void SetImpersonateId(ExchangeService service, string impersonatedUserAddress)
        {
            this.AuthObject.SetImpersonatedUserId(service, impersonatedUserAddress);
        }

        protected void RemoveImpersonatedUserId(ExchangeService service)
        {
            this.AuthObject.RemoveImpersonatedUserId(service);
        }

        protected void AddImpersonationHeader(ExchangeService service, string mailbox)
        {
            this.AuthObject.AddImpersonationHeader(service, mailbox);
        }

        protected ExchangeService CloneExchangeService(ExchangeService oldService, int maxRetryCount)
        {
            var service = CreateExchangeService(maxRetryCount);
            service.Url = oldService.Url;
            if (oldService.HttpHeaders.ContainsKey(ExchangeConstants.IMPERSONATION_HEADER_NAME))
            {
                AddImpersonationHeader(service, oldService.HttpHeaders[ExchangeConstants.IMPERSONATION_HEADER_NAME]);
            }
            if (oldService.ImpersonatedUserId != null)
            {
                SetImpersonateId(service, oldService.ImpersonatedUserId.Id);
            }
            return service;
        }
    }
}
