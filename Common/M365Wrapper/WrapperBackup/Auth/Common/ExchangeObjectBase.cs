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

using ExchangeBackupUtility.Graph;

namespace ExchangeUtility.Graph
{
    using System;

    using Microsoft.Exchange.WebServices.Data;

    using Microsoft365.Configuration;

    using AvePoint.RA.CommonUtil;
    using Util.MSAzure;
    using Endpoints = Util.MSAzure.Endpoints;

    public abstract class ExchangeObjectBase : BaseExchangeItem
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(ExchangeObjectBase));

        protected const int DEFAULT_RETRY = 5;
        protected const ExchangeVersion EXCHANGE_VERSION = ExchangeVersion.Exchange2016;
        private const int TIMEOUT = 600000;

        protected static readonly PropertySet DEFAULT_FOLDER_PROPERTY_SET =
            EXCHANGE_VERSION >= ExchangeVersion.Exchange2013 ?
            new PropertySet(BasePropertySet.FirstClassProperties,
                [
                    new ExtendedPropertyDefinition(26293, MapiPropertyType.String),//Path
                    new ExtendedPropertyDefinition(4340,MapiPropertyType.Boolean),//HiddenFolder
                    FolderSchema.WellKnownFolderName, new ExtendedPropertyDefinition(13825, MapiPropertyType.Integer),//PR_FOLDER_TYPE
                ]) :
            new PropertySet(BasePropertySet.FirstClassProperties,
                [
                    new ExtendedPropertyDefinition(26293, MapiPropertyType.String),//Path
                    new ExtendedPropertyDefinition(4340,MapiPropertyType.Boolean),//HiddenFolder
                    new ExtendedPropertyDefinition(13825, MapiPropertyType.Integer),//PR_FOLDER_TYPE
                ]);

        public string UserName => AuthObject.UserName;

        public string ServiceUrl
        {
            get => AuthObject.EWSServiceUrl;
            protected set => AuthObject.EWSServiceUrl = value;
        }

        public IEWSAuthObject AuthObject { get; private set; }

        /// <summary>
        /// Enable scp lookup when autodiscover. It is recommended to enable scp lookup only for in-network autodiscover.
        /// For exchange online, it is always out-network.
        /// </summary>
        public bool EnableScpLookup { get; set; }

        public static string UserAgent { get; set; }

        static ExchangeObjectBase()
        {
            try
            {
                UserAgent = Microsoft365Configuration.CommonConfiguration.UserAgent;
            }
            catch
            {
                UserAgent = $@"AvePoint LiteBackup";
            }
        }

        protected ExchangeObjectBase(IEWSAuthObject authObj)
        {
            ArgumentNullException.ThrowIfNull(authObj);
            AuthObject = authObj;
        }

        protected ExchangeService CreateExchangeService()
        {
            return CreateExchangeService(DEFAULT_RETRY);
        }

        protected POXAutodiscoverService CreatePOXAutoDiscoverService()
        {
            var poxAutoDiscoverService = new POXAutodiscoverService();
            this.AuthObject.BindToPOXAutoDiscoverService(poxAutoDiscoverService);
            poxAutoDiscoverService.Timeout = TIMEOUT;
            poxAutoDiscoverService.EXCHANGE_VERSION = EXCHANGE_VERSION;
            poxAutoDiscoverService.ServiceUrl = this.ServiceUrl;
            poxAutoDiscoverService.EnableScpLookup = this.EnableScpLookup;
            return poxAutoDiscoverService;
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
            EnableEWSTraceLog(service);
            return service;
        }

        protected void SetServiceUrl(ExchangeService service)
        {
            if (!string.IsNullOrEmpty(this.ServiceUrl))
            {
                service.Url = new Uri(this.ServiceUrl);
            }
            else
            {
                RunAutoDiscoverUrl(service, this.UserName);
                this.ServiceUrl = service.Url.AbsoluteUri;
            }
        }

        protected void RunAutoDiscoverUrl(ExchangeService service, string emailAddress)
        {
            logger.Warn("Run autodiscover.");
            try
            {
                var url = Endpoints.GetEndpoints(AuthObject.Environment).ExchangeWeb;
                ArgumentNullException.ThrowIfNull(url);
                service.Url = new Uri(url);
                logger.Info($"AutoDiscoverUrl success, serivce url: {service.Url.AbsoluteUri}");
            }
            catch (Exception ex)
            {
                logger.Warn("Autodiscover service execute with exception, reason: {0}", ex.ToString());
                service.Url = new Uri(Endpoints.Worldwide.ExchangeWeb);
                logger.Info($"Use default service url: {service.Url.AbsoluteUri}");
            }
        }

        private void EnableEWSTraceLog(ExchangeService service)
        {
            if (ExchangeGlobalConfig.EnableTraceLog)
            {
                service.EnableTraceLog();
            }
        }

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
            if (this.AuthObject.AuthType == AuthObjectType.AccessToken || this.AuthObject.AuthType == AuthObjectType.PasswordAccessToken)
            {
                retryable = new AADTokenRefresher(this.AuthObject, service, retryable); //refresh add token when expired
            }
            retryable = new Retryable(maxRetryCount, retryable); //retry for EWS error
            retryable = new ExceptionWrapper(new FormattedMessageException.Context() { AuthObject = this.AuthObject }, retryable); //wrapper EWS exception and format msg
            return retryable;
        }

        protected virtual void SetImpersonateId(ExchangeService service, string impersonatedUserAddress)
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

        protected ExchangeService CloneExchangeService(ExchangeService oldService)
        {
            return CloneExchangeService(oldService, DEFAULT_RETRY);
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