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





namespace AvePoint.GCommon.Utility.Cryptography.Encryption.Aes
{
    #region using directives

    #endregion

    public enum SignatureVerificationResult
    {
        AssemblyIdentityMismatch = 1,
        BadDigest = -2146869232,
        BadSignatureFormat = -2146762749,
        BasicConstraintsNotObserved = -2146869223,
        CertificateExpired = -2146762495,
        CertificateExplicitlyDistrusted = -2146762479,
        CertificateMalformed = -2146762488,
        CertificateNotExplicitlyTrusted = -2146762748,
        CertificateRevoked = -2146762484,
        CertificateUsageNotAllowed = -2146762490,
        ContainingSignatureInvalid = 2,
        CouldNotBuildChain = -2146762486,
        GenericTrustFailure = -2146762485,
        InvalidCertificateName = -2146762476,
        InvalidCertificatePolicy = -2146762477,
        InvalidCertificateRole = -2146762493,
        InvalidCertificateSignature = -2146869244,
        InvalidCertificateUsage = -2146762480,
        InvalidCountersignature = -2146869245,
        InvalidSignerCertificate = -2146869246,
        InvalidTimePeriodNesting = -2146762494,
        InvalidTimestamp = -2146869243,
        IssuerChainingError = -2146762489,
        MissingSignature = -2146762496,
        PathLengthConstraintViolated = -2146762492,
        PublicKeyTokenMismatch = 3,
        PublisherMismatch = 4,
        RevocationCheckFailure = -2146762482,
        SystemError = -2146869247,
        UnknownCriticalExtension = -2146762491,
        UnknownTrustProvider = -2146762751,
        UnknownVerificationAction = -2146762750,
        UntrustedCertificationAuthority = -2146762478,
        UntrustedRootCertificate = -2146762487,
        UntrustedTestRootCertificate = -2146762483,
        Valid = 0
    }
}
