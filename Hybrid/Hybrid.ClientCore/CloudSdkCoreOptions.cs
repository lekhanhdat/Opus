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
namespace AvePoint.Hybrid.ClientCore
{
    using System.Security.Cryptography.X509Certificates;

    public class CloudSdkCoreOptions
    {
        public X509Certificate2 CommunicationCertificate { get; set; }

        public string Product { get; set; }

        // HttpClientSetting
        public string DefaultHttpClientName { get; set; }
        public bool UseCustomizedRetryPolicy { get; set; }

        //in old logic, vcloud proudct name is different with aos product name
        public string VCloudProduct { get; set; }

        public bool IsIdentityServerConfigured { get; set; }
        public string IdentityServerAddress { get; set; }

        public string IdentityServerClientId { get; set; }

        public string IdentityServerScope { get; set; }

        public bool IsInternalIdentityServer { get; set; } = false;
    }
}
