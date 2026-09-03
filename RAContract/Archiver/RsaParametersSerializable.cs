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
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Archiver
{
    public class RsaParametersSerializable
    {
        public byte[] D { get; set; }
        public byte[] DP { get; set; }
        public byte[] DQ { get; set; }
        public byte[] Exponent { get; set; }
        public byte[] InverseQ { get; set; }
        public byte[] Modulus { get; set; }
        public byte[] P { get; set; }
        public byte[] Q { get; set; }

        public RsaParametersSerializable() { }

        public RsaParametersSerializable(RSAParameters parameters)
        {
            D = parameters.D;
            DP = parameters.DP;
            DQ = parameters.DQ;
            Exponent = parameters.Exponent;
            InverseQ = parameters.InverseQ;
            Modulus = parameters.Modulus;
            P = parameters.P;
            Q = parameters.Q;
        }

        public RSAParameters ToRSAParameters()
        {
            return new RSAParameters
            {
                D = this.D,
                DP = this.DP,
                DQ = this.DQ,
                Exponent = this.Exponent,
                InverseQ = this.InverseQ,
                Modulus = this.Modulus,
                P = this.P,
                Q = this.Q
            };
        }
    }

    public class ExportSignatureInfo
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string SharedParametersJson { get; set; }
        public bool EnableExportSignature { get; set; }
        public byte[] Certificate { get; set; }
        public string Password { get; set; }
        public string Thumbprint { get; set; }
    }
}
