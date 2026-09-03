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
    using System;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    #endregion

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    internal abstract class Aes : SymmetricAlgorithm
    {
        // Fields
        private static KeySizes[] s_legalBlockSizes = new KeySizes[] { new KeySizes(0x80, 0x80, 0) };
        private static KeySizes[] s_legalKeySizes = new KeySizes[] { new KeySizes(0x80, 0x100, 0x40) };

        // Methods
        protected Aes()
        {
            base.LegalBlockSizesValue = s_legalBlockSizes;
            base.LegalKeySizesValue = s_legalKeySizes;
            base.BlockSizeValue = 0x80;
            base.FeedbackSizeValue = 8;
            base.KeySizeValue = 0x100;
            base.ModeValue = CipherMode.CBC;
        }

        public new static Aes Create()
        {
            return Create(typeof(AesCryptoServiceProvider).FullName);
        }

        public new static Aes Create(string algorithmName)
        {
            if (algorithmName == null)
            {
                throw new ArgumentNullException("algorithmName");
            }
            return CoreCryptoConfig.CreateFromName<Aes>(algorithmName);
        }
    }
}
