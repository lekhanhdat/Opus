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



namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Xml;
    #endregion

    public class CustomX509CertificateValidator : X509CertificateValidator
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(CustomX509CertificateValidator));

        static object syncRoot = new object();
        static X509Certificate2 localCertificate;

        /// <summary>
        /// Certificate validation function.
        /// </summary>
        /// <param name="certificate">a x509 certificate</param>
        public override void Validate(X509Certificate2 certificate)
        {
            InitLocalCertificate();
            ValidateCertificateRelationship(certificate, localCertificate);
        }

        private void ValidateCertificateRelationship(X509Certificate2 remoteCertificate, X509Certificate2 localCertificate)
        {
            if (remoteCertificate == null)
            {
                logger.Error("Remote certificate is null");
                throw new SecurityTokenException("Remote certificate is null");
            }
            if (localCertificate == null)
            {
                logger.Error("Local certificate is null");
                throw new SecurityTokenException("Local certificate is null");
            }

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            X509Chain remoteChain = new X509Chain();
            X509Chain localChain = new X509Chain();
            remoteChain.ChainPolicy.VerificationFlags |= X509VerificationFlags.AllowUnknownCertificateAuthority;
            localChain.ChainPolicy.VerificationFlags |= X509VerificationFlags.AllowUnknownCertificateAuthority;
            remoteChain.ChainPolicy.RevocationMode = IsBuiltinCertificate(remoteCertificate.Thumbprint) ? X509RevocationMode.NoCheck : X509RevocationMode.Online;
            localChain.ChainPolicy.RevocationMode = IsBuiltinCertificate(localCertificate.Thumbprint) ? X509RevocationMode.NoCheck : X509RevocationMode.Online;
            remoteChain.Build(remoteCertificate);
            localChain.Build(localCertificate);
            
            watch.Stop();
            
            if (watch.Elapsed.TotalSeconds > 3)
            {
                logger.Warn("Build the chain of certificate:{0} and {1} with online revocation mode takes {2}s", localCertificate.Subject, remoteCertificate.Subject, watch.Elapsed.TotalSeconds);
            }


            foreach (var chainStatus in remoteChain.ChainStatus)
            {
                if (chainStatus.Status == X509ChainStatusFlags.Revoked)
                {
                    logger.Info("Remote certificate Thumbprint:{0} IssueTo:{1} IssueBy:{2}", remoteCertificate.Thumbprint, remoteCertificate.Subject, remoteCertificate.Issuer);
                    logger.Error("Certificate is revoked.");
                    throw new SecurityTokenException("Certificate is revoked.");
                }
            }
            foreach (var chainStatus in localChain.ChainStatus)
            {
                if (chainStatus.Status == X509ChainStatusFlags.Revoked)
                {
                    logger.Info("Local certificate Thumbprint:{0} IssueTo:{1} IssueBy:{2}", localCertificate.Thumbprint, localCertificate.Subject, localCertificate.Issuer);
                    logger.Error("Certificate is revoked.");
                    throw new SecurityTokenException("Certificate is revoked.");
                }
            }

            //check certificate relationship
            if (localCertificate.Thumbprint == remoteCertificate.Thumbprint) return;

            if (IsBuiltinCertificate(localCertificate.Thumbprint)
                && IsBuiltinCertificate(remoteCertificate.Thumbprint))
            {
                return;
            }

            if ((remoteChain.ChainElements.Count > 1) && (localChain.ChainElements.Count > 1))
            {
                if (remoteChain.ChainElements[1].Certificate.Thumbprint == localChain.ChainElements[1].Certificate.Thumbprint)
                {
                    return;
                }
            }
            logger.Info("Local certificate Thumbprint:{0} IssueTo:{1} IssueBy:{2}", localCertificate.Thumbprint, localCertificate.Subject, localCertificate.Issuer);
            logger.Info("Remote certificate Thumbprint:{0} IssueTo:{1} IssueBy:{2}", remoteCertificate.Thumbprint, remoteCertificate.Subject, remoteCertificate.Issuer);
            logger.Error("Certificate relationship is invalid.");
            throw new SecurityTokenException("Certificate relationship is invalid.");
        }

        private void InitLocalCertificate()
        {
            if (localCertificate == null)
            {
                lock (syncRoot)
                {
                    if (localCertificate == null)
                    {
                        string localCerttificateThumbprint = GetLocalCertificateThumbprint();
                        logger.Info("Local certificate thumbprint is {0}", localCerttificateThumbprint);
                        X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                        store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
                        X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByThumbprint, localCerttificateThumbprint, false);
                        if (certs.Count == 0) throw new SecurityTokenException("Local certificate not found.");
                        localCertificate = certs[0];
                        store.Close();
                    }
                }
            }
        }

        private string GetLocalCertificateThumbprint()
        {
            var behaviorsConfig = String.Empty;
            var agentBehaviorsConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AgentCommonWCFBehaviors.config");
            var mediaBehaviorsConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MediaWcfBehaviorsConfigurations.config");
            var reportingBehaviorsConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReportCenterWCFBehaviors.config");
            var controlTimerBehaviorsConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"bin\Config\ControlWCFBehaviors.config");
            var controlWebBehaviorsConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\ControlWCFBehaviors.config");
            var gaPlusWebBehaviorsConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\GAControlWCFBehaviors.config");

            if (File.Exists(agentBehaviorsConfig))
            {
                behaviorsConfig = agentBehaviorsConfig;
            }
            else if (File.Exists(mediaBehaviorsConfig))
            {
                behaviorsConfig = mediaBehaviorsConfig;
            }
            else if (File.Exists(reportingBehaviorsConfig))
            {
                behaviorsConfig = reportingBehaviorsConfig;
            }
            else if (File.Exists(controlWebBehaviorsConfig))
            {
                behaviorsConfig = controlWebBehaviorsConfig;
            }
            else if (File.Exists(controlTimerBehaviorsConfig))
            {
                behaviorsConfig = controlTimerBehaviorsConfig;
            }
            else if (File.Exists(gaPlusWebBehaviorsConfig))
            {
                behaviorsConfig = gaPlusWebBehaviorsConfig;
            }
            logger.Info("WCF behavior file is {0}", behaviorsConfig);
            if (File.Exists(behaviorsConfig))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(behaviorsConfig);
                XmlNode clientCertificateNode = xDoc.SelectSingleNode(@"/behaviors/endpointBehaviors/behavior/clientCredentials/clientCertificate");
                XmlNode serviceCertificateNode = xDoc.SelectSingleNode(@"/behaviors/serviceBehaviors/behavior/serviceCredentials/serviceCertificate");
                string wcfThumbprint1 = clientCertificateNode.Attributes["findValue"].Value;
                if (serviceCertificateNode != null)
                {
                    string wcfThumbprint2 = serviceCertificateNode.Attributes["findValue"].Value;
                    if (string.Compare(wcfThumbprint1, wcfThumbprint2, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        logger.Info("The two thumbprint values in WCF behavior configuration file are different. {0}", behaviorsConfig);
                        throw new SecurityTokenException("WCF behavior configuration file is invalid.");
                    }
                }
                return wcfThumbprint1;
            }
            else
            {
                logger.Info("Can not find WCF behavior configuration file {0}", behaviorsConfig);
                throw new SecurityTokenException("Can not find WCF behavior configuration file.");
            }
        }

        private bool IsBuiltinCertificate(string thrumbprint)
        {
            var defaults = new string[] 
            {
                BuiltInCertificates.DocAveBuiltInCertificate,
                BuiltInCertificates.DocAveBuiltInCertificateEx,
                BuiltInCertificates.DocAveBuiltInCertificateSHA2
            };

            foreach (var d in defaults)
            {
                if (d.Equals(thrumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class PermissiveCertificatePolicy
    {
        static PermissiveCertificatePolicy currentPolicy;
        static readonly Object syncRoot = new Object();

        private PermissiveCertificatePolicy()
        {
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertValidate);
        }

        public static void Enact()
        {
            if (currentPolicy == null)
            {
                lock (syncRoot)
                {
                    if (currentPolicy == null)
                    {
                        currentPolicy = new PermissiveCertificatePolicy();
                    }
                }
            }
        }

        bool RemoteCertValidate(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }
    }

    public class CustomIdentityVerifer : IdentityVerifier
    {
        public override bool CheckAccess(EndpointIdentity identity, AuthorizationContext authContext)
        {
            return true;
        }

        public override bool TryGetIdentity(EndpointAddress reference, out EndpointIdentity identity)
        {
            identity = null;
            return true;
        }

        public static void Plug(ChannelFactory cf)
        {
            if (cf.Endpoint.Binding is CustomBinding)
            {
                SslStreamSecurityBindingElement bindingElement = (cf.Endpoint.Binding as CustomBinding).Elements.Find<SslStreamSecurityBindingElement>();
                if (bindingElement != null)
                {
                    bindingElement.IdentityVerifier = new CustomIdentityVerifer();
                }
            }
        }
    }
}