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
using AvePoint.Common;
using AvePoint.RA.Contract.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

//namespace AvePoint.RA.Common.Security
//{
//    public class RMDigitalSignatureHelper : IRMDigitalSignatureHelper
//    {
//        private readonly RsaHelper rsaHelper;

//        public RMDigitalSignatureHelper(String type)
//        {
//            var x509Certificate2 = RMCertificateHelper.GetCertificate(type);
//            if (x509Certificate2 != null)
//                this.rsaHelper = new RsaHelper(x509Certificate2);
//        }

//        public RMDigitalSignatureHelper(X509Certificate2 cert)
//        {
//            this.rsaHelper = new RsaHelper(cert);
//        }

//        //public Boolean Verify(String plainText, String signature)
//        //{
//        //    return this.rsaHelper.VerifyData(plainText, signature);
//        //}

//        //public String SignData(String plainText)
//        //{
//        //    return this.rsaHelper.SignData(plainText);
//        //}
//    }
//}
