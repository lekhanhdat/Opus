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


namespace AvePoint.Common
{
    #region using directives
    using System;
    using System.Security.Cryptography.X509Certificates;

    #endregion
    public class SecurityKeyEncryptor
    {
        readonly RsaHelper rsaHelper;

        public SecurityKeyEncryptor(X509Certificate2 certificate2)
        {
            this.rsaHelper = new RsaHelper(certificate2);
        }

        public String Encrypt(String plainKey)
        {
            return this.rsaHelper.Encrypt(plainKey);
        }

        public byte[] Encrypt(Byte[] keys)
        {
            return this.rsaHelper.Encrypt(keys);
        }

        public String Decrypt(String cipherKey)
        {
            return this.rsaHelper.Decrypt(cipherKey);
        }

        public Byte[] DecryptToBytes(byte[] cipherKey)
        {
            return (this.rsaHelper.Decrypt2(cipherKey));
        }
    }
}
