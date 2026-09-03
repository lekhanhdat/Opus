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
using System.Security;
using System.Security.Cryptography;

namespace AvePoint.GCommon.Utility.Cryptography.Encryption
{
    [Obsolete("Fortify weak encryption issue: Use strong encryption algorithms with large key sizes to protect sensitive data. A strong alternative to DES is AES (Advanced Encryption Standard, formerly Rijndael)")]
    public class DESEncryption : AbstractEncryption
    {

        public DESEncryption()
        {
            Crypto = new DESCryptoServiceProvider();
            this.SetKeyAndIV(null);
        }

        public DESEncryption(SecureString key) 
        {
            Crypto = new DESCryptoServiceProvider();
            this.SetKeyAndIV(CryptoUtil.ConvertSecureStringToBytes(key));
        }

        public DESEncryption(byte[] key)
        {
            Crypto = new DESCryptoServiceProvider();
            this.SetKeyAndIV(key);
        }


        public override CryptoMode FipsMode
        {
            get { return CryptoMode.NoneFIPS; }
        }


    }
}
