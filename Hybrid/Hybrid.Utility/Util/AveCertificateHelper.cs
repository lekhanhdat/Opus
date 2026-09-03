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
using AvePoint.Hybrid.Utility.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace  AvePoint.Hybrid.Utility
{
    public class AveCertificateHelper
    {
        private static Dictionary<string, X509Certificate2> certificateMapping = new Dictionary<string, X509Certificate2>();

        public static X509Certificate2 GetCertificate(string thumbprint)
        {
            ThrowUtil.ThrowIfNull(thumbprint, "thumbprint");
            lock (certificateMapping)
            {
                if (certificateMapping.ContainsKey(thumbprint))
                {
                    return certificateMapping[thumbprint];
                }
                else
                {
                    var certificate = Get509Cert(StoreLocation.LocalMachine, thumbprint);
                    if (certificate == null)
                    {
                        certificate = Get509Cert(StoreLocation.CurrentUser, thumbprint);
                    }
                    if (certificate == null)
                    {
                        throw new Exception(string.Format("Can't find certificate by thumbprint {0}.", thumbprint));
                    }
                    else
                    {
                        certificateMapping.Add(thumbprint, certificate);
                    }
                    return certificate;

                }
            }
        }

        public static X509Certificate2 Get509Cert(StoreLocation location, string thumbprint)
        {
            var store = new X509Store(StoreName.My, location);
            store.Open(OpenFlags.ReadOnly);
            var x509cerCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (x509cerCollection.Count == 0)
            {
                return null;
            }
            X509Certificate2 cer = x509cerCollection[0];
            store.Close();
            return cer;
        }



    }
}
