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
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    #endregion

    internal static class CoreServiceBehaviorBuilder
    {
        public static IServiceBehavior BuildDependencyInjectionServiceBehavior<TIocContainer>(TIocContainer container)
        {
            return new DependencyInjectionServiceBehavior<TIocContainer>(container);
        }

        public static IServiceBehavior BuildThrottlingBehavior()
        {
            return new ServiceThrottlingBehavior
            {
                MaxConcurrentCalls = 200,
                MaxConcurrentSessions = 200,
                MaxConcurrentInstances = 400
            };
        }

        public static IServiceBehavior BuildCredentialsBehavior(Object thumbprint = default(Object))
        {
            var result = new ServiceCredentials();
            if (thumbprint is X509Certificate2)
            {
                result.ServiceCertificate.Certificate = (X509Certificate2)thumbprint;
            }
            else
            {
                result.ServiceCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindByThumbprint, thumbprint ?? MicroKernelConstant.DefaultThumbprint);
            }
            result.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            result.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.Custom;
            result.ClientCertificate.Authentication.CustomCertificateValidator = new CoreChannelX509CertificateValidator(thumbprint);
            return result;
        }

        public static IServiceBehavior BuildDebugBehavior()
        {
            return new ServiceDebugBehavior
            {
                IncludeExceptionDetailInFaults = 1 < 2
            };
        }
    }
}
