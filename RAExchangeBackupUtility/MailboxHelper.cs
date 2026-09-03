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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Reflection;

namespace ExchangeUtility
{
    public class ExchangeMailbox
    {
        private static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public string OriginalMailboxAddress { get; private set; }
        public string MailboxAddress { get; private set; }
        public bool IsArchiveMailbox { get; private set; }
        public bool IsResourceMailbox { get; private set; }
        internal ExchangeService service;
        protected const int DEFAULT_RETRY = 5;
        private const ExchangeVersion EXCHANGE_VERSION = ExchangeVersion.Exchange2016;
        private const int TIMEOUT = 1500000;
        public AuthObject AuthObject { get; private set; }
        public bool EnableScpLookup { get; set; }
        public static string UserAgent { get; set; }
        public string UserName { get { return this.AuthObject.UserName; } }

        public string ServiceUrl
        {
            get { return this.AuthObject.EWSServiceUrl; }
            protected set { this.AuthObject.EWSServiceUrl = value; }
        }

        static ExchangeMailbox()
        {
            UserAgent = string.Format("ISV|AvePoint|CloudRecords/{0}", ExchangeGlobalConfig.ProductVersion);
            logger.Debug($"User Agent: {UserAgent}");
            // $"ISV|AvePoint|RECO/{DateTime.UtcNow.ToString("yyyyMM")}|Interactive";
            //$@"AvePoint\{ExchangeGlobalConfig.ProductVersion} RECO";
        }
        public string ImpersonateId
        {
            get
            {
                return GlobalExchangeSetting.GetImpersonateIdByMailbox(this.OriginalMailboxAddress);
            }
        }
        public bool IsPublicFolder { get { return this.MailboxType == ExchangeMailboxType.PublicFolder; } }

        public ExchangeMailboxType MailboxType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public WellKnownFolderName MsgFolderRoot
        {
            get
            {
                switch (this.MailboxType)
                {
                    case ExchangeMailboxType.PublicFolder:
                        return WellKnownFolderName.PublicFoldersRoot;
                    default:
                        return this.IsArchiveMailbox ? WellKnownFolderName.ArchiveMsgFolderRoot : WellKnownFolderName.MsgFolderRoot;
                }
            }
        }


        public FolderId RootFolderId 
        {
            get 
            {
                return new FolderId(
                    this.MsgFolderRoot,
                    new Mailbox()
                    {
                        Address = this.MailboxAddress
                    });
            }
        }

        public ExchangeMailbox(string originalMailAddress, ExchangeMailboxType type)
        {
            if (string.IsNullOrEmpty(originalMailAddress)) throw new ArgumentNullException("originalMailAddress");

            this.OriginalMailboxAddress = originalMailAddress;

            this.MailboxType = type;
            string mailboxAddress = originalMailAddress;
            if (IsArchiveMailboxAddress(originalMailAddress, out mailboxAddress))
            {
                this.IsArchiveMailbox = true;
            }
            else if (IsResourceMailboxAddress(originalMailAddress, out mailboxAddress))
            {
                this.IsResourceMailbox = true;
            }
            this.MailboxAddress = this.IsPublicFolder ? null : mailboxAddress;
        }

        public ExchangeMailbox(string originalMailAddress, AuthObject authObj)
        {
            if (string.IsNullOrEmpty(originalMailAddress)) throw new ArgumentNullException("originalMailAddress");
            this.OriginalMailboxAddress = originalMailAddress;
            this.AuthObject = authObj;
            string mailboxAddress = originalMailAddress;
            if (IsArchiveMailboxAddress(originalMailAddress, out mailboxAddress))
            {
                this.IsArchiveMailbox = true;
            }
            else if (IsResourceMailboxAddress(originalMailAddress, out mailboxAddress))
            {
                this.IsResourceMailbox = true;
            }
            this.MailboxAddress = this.IsPublicFolder ? null : mailboxAddress;
            this.service = CreateExchangeService();
            SetServiceUrl(this.service);
            AddImpersonationHeader(this.service, MailboxAddress);
            SetImpersonateId(this.service, ImpersonateId);
        }

        public string GetRealMailboxGuid()
        {
            //https://stackoverflow.com/questions/50255718/find-user-mailbox-guid-using-their-email-address-ews
            string mailBoxGuid = string.Empty;
            try
            {
                PropertySet exProp = new PropertySet(BasePropertySet.FirstClassProperties);
                NameResolutionCollection ncCol = service.ResolveName(MailboxAddress, ResolveNameSearchLocation.DirectoryOnly, true, exProp).GetAwaiter().GetResult();
                if (ncCol.Count == 1)
                {
                    mailBoxGuid = ncCol[0].Contact.DirectoryId;
                    logger.Info("Mailbox DirectoryId is:{0}.Address:{1}.", mailBoxGuid, MailboxAddress);
                    mailBoxGuid = mailBoxGuid.Substring("<GUID=".Length, mailBoxGuid.Length - 1 - "<GUID=".Length);
                    logger.Info("Mailbox real mailBoxGuid is:{0}.Address:{1}.", mailBoxGuid, MailboxAddress);
                }
            }
            catch (Exception ex)
            {
                logger.Info("Can not get real mailbox guid.Message:{0}.", ex.ToString());
            }
            return mailBoxGuid;
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

            SetRetry(maxRetryCount, service);
            //EnableEWSTraceLog(service);
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
                service.AutodiscoverUrl(emailAddress, new AutoDiscoverCallback().RedirectionUrlValidationCallback).GetAwaiter().GetResult();
                if (string.IsNullOrWhiteSpace(service.Url?.AbsoluteUri))
                {
                    throw new Exception("Autodiscover service url is null");
                }
                else
                {
                    logger.Info($"1. ExchangeService.AutoDiscoverUrl, serivce url: {service.Url.AbsoluteUri}");
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Autodiscover service execute with exception, reason: {0}", ex.ToString());
                var url = AzureEnvironment.FromDomainOrPrincipalName(emailAddress)?.EWSServiceUrl;
                if (!string.IsNullOrEmpty(url))
                {
                    service.Url = new Uri(url);
                    logger.Info($"2. Assume email domain is on Office365, AzureEnvironment.FromDomainOrPrincipalName, service url: {service.Url.AbsoluteUri}");
                }
                else
                {
                    service.Url = new Uri(AzureEnvironment.DefaultCloud.EWSServiceUrl);
                    logger.Info($"3. Use default service url: {service.Url.AbsoluteUri}");
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
            retryable = new ExceptionWrapper(new FormattedMessageException.Context() { AuthObject = this.AuthObject }, retryable); //wrapper EWS exception and format msg
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

        private bool IsArchiveMailboxAddress(string originalMailboxAddress, out string mailboxAddress)
        {
            mailboxAddress = originalMailboxAddress;

            int index = originalMailboxAddress.LastIndexOf(string.Format("({0})", ExchangeConstants.InPlaceArchiveMailbox));
            if (index > 0)
            {
                mailboxAddress = originalMailboxAddress.Substring(0, index);
                return true;
            }
            return false;
        }

        private bool IsResourceMailboxAddress(string originalMailboxAddress, out string mailboxAddress)
        {
            mailboxAddress = originalMailboxAddress;

            int index = originalMailboxAddress.LastIndexOf(string.Format("({0})", ExchangeConstants.ResourceMailbox));
            if (index > 0)
            {
                mailboxAddress = originalMailboxAddress.Substring(0, index);
                return true;
            }
            return false;
        }

        public static string DecodeEmailAddress(string old)
        {
            return new ExchangeMailbox(old, ExchangeMailboxType.None).MailboxAddress;
        }

    }


    public enum ExchangeMailboxType
    {
        None = 0,
        PublicFolder = 1,
        User = 2,
        Group = 3,
    }

}
