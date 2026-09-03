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



namespace AvePoint.GCommon.MicroKernel
{
    #region using directives
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography.X509Certificates;
    #endregion

    internal class CoreChannelX509CertificateValidator
        : X509CertificateValidator
    {
        Object thumbprintValue;

        public CoreChannelX509CertificateValidator(Object findValue = null)
        {
            this.thumbprintValue = findValue ?? MicroKernelConstant.DefaultThumbprint;
        }
        public override void Validate(X509Certificate2 certificate)
        {
            var localCertificate = this.GetX509Certicate2ByThumbprintValue(this.thumbprintValue);
            var remoteCertificate = certificate;
            this.ValidateCertificateRelationship(localCertificate, remoteCertificate);
        }

        X509Certificate2 GetX509Certicate2ByThumbprintValue(Object thumbprint)
        {
            if (thumbprint is X509Certificate2)
            {
                return thumbprint as X509Certificate2;
            }
            var resultX509Certificate2 = default(X509Certificate2);
            var x509Store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            x509Store.Open(OpenFlags.ReadOnly);
            var x509CertificateCollection = x509Store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (x509CertificateCollection != null && x509CertificateCollection.Count > 0)
            {
                resultX509Certificate2 = x509CertificateCollection[0];
            }
            return resultX509Certificate2;
        }

        void ValidateCertificateRelationship(X509Certificate2 localCertificate, X509Certificate2 remoteCertificate)
        {
            if (localCertificate == null)
                throw new SecurityTokenException("local certificate is null");
            if (remoteCertificate == null)
                throw new SecurityTokenException("remote certificate is null");
            if (!localCertificate.Thumbprint.Equals(remoteCertificate.Thumbprint, StringComparison.OrdinalIgnoreCase))
            {
                var firstChain = new X509Chain();
                var secondChain = new X509Chain();
                firstChain.ChainPolicy.VerificationFlags |= X509VerificationFlags.AllowUnknownCertificateAuthority;
                secondChain.ChainPolicy.VerificationFlags |= X509VerificationFlags.AllowUnknownCertificateAuthority;
                firstChain.Build(localCertificate);
                secondChain.Build(remoteCertificate);
                if ((firstChain.ChainElements.Count > 1) && (secondChain.ChainElements.Count > 1))
                {
                    if (!firstChain.ChainElements[1].Certificate.Thumbprint.Equals(secondChain.ChainElements[1].Certificate.Thumbprint, StringComparison.OrdinalIgnoreCase))
                        throw new SecurityTokenException("Certificate parent relationship is invalid.");
                }
                else throw new SecurityTokenException("Certificate relationship is invalid.");
            }
        }
    }
}
