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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Cert
{
    public class CreateCert
    {
        public static X509Certificate2 CreateSelfSignedCertificate(DateTime notAfter)
        {
            var subjectName = $"CN={GetCNName()}";
            using (var ras = RSA.Create(2048))
            {
                var certRequest = new CertificateRequest(
                        subjectName,
                        ras,
                        HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pss
                    );
                certRequest.CertificateExtensions.Add(
                    new X509KeyUsageExtension(
                        keyUsages: X509KeyUsageFlags.DigitalSignature,
                        critical: false
                    )
                );
                certRequest.CertificateExtensions.Add(
                    new X509SubjectKeyIdentifierExtension(key: certRequest.PublicKey, critical: false)
                );
                certRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
                    new OidCollection
                    {
                    new Oid("1.3.6.1.5.5.7.3.1", "Server Authentication")
                    }, false
                ));
                var cert = certRequest.CreateSelfSigned(DateTime.Now, notAfter);
                byte[] data = cert.Export(X509ContentType.Pfx);
                return new X509Certificate2(data, "", X509KeyStorageFlags.Exportable);
            }
        }

        public static string GetCNName()
        {
            var cnName = string.Empty;
            try
            {
                cnName = System.Net.Dns.GetHostName() + "." + System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().Name;
            }
            catch (Exception ex)
            {
                cnName = System.Net.Dns.GetHostName();
            }

            return cnName;
        }
    }
}
