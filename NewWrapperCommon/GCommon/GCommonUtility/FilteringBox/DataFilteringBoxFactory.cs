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
using System.Text;
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.GCommon.Utility.FilteringBox
{
    public class DataFilteringBoxFactory
    {

        static DataFilteringBoxFactory() 
        {
            CryptographyManagement.CryptoInit();
        }

        //public static IDataFilteringBox GetEncryptionFilteringBox(EncryptionAlgorithm method)
        //{
        //    IDataFilteringBox filterBox;
        //    switch (method)
        //    {
        //        case EncryptionAlgorithm.BLOWFISH_ENCRYPTION:
        //            IEncryption blowfishEncryption = EncryptionFactory.GetDefaultKeyEncryption(EncryptionAlgorithm.BLOWFISH_ENCRYPTION);
        //            filterBox = new EncryptionFilteringBox(blowfishEncryption);
        //            break;
        //        case EncryptionAlgorithm.AES_ENCRYPTION:
        //            IEncryption aesEncryption = EncryptionFactory.GetDefaultKeyEncryption(EncryptionAlgorithm.AES_ENCRYPTION);
        //            filterBox = new EncryptionFilteringBox(aesEncryption);
        //            break;
        //        default:
        //            throw new NotImplementedException(method.ToString());
        //    }
        //    return filterBox;
        //}

        public static IDataFilteringBox GetEncryptionFilteringBox(EncryptionAlgorithm method, string encryptedkey)
        {
            IDataFilteringBox filterBox;


            filterBox = new EncryptionFilteringBox(true, encryptedkey, method);

            return filterBox;
        }

        public static IDataFilteringBox GetDecryptionFilteringBox(EncryptionAlgorithm method, string encryptedkey)
        {
            IDataFilteringBox filterBox;

            filterBox = new EncryptionFilteringBox(false, encryptedkey, method);

            return filterBox;
        }

        //public static IDataFilteringBox GetDecryptionFilteringBox(EncryptionAlgorithm method)
        //{
        //    IDataFilteringBox filterBox;
        //    switch (method)
        //    {
        //        case EncryptionAlgorithm.BLOWFISH_ENCRYPTION:
        //            IEncryption blowfishEncryption = EncryptionFactory.GetDefaultKeyEncryption(EncryptionAlgorithm.BLOWFISH_ENCRYPTION);
        //            filterBox = new EncryptionFilteringBox(blowfishEncryption, false);
        //            break;
        //        case EncryptionAlgorithm.AES_ENCRYPTION:
        //            IEncryption aesEncryption = EncryptionFactory.GetDefaultKeyEncryption(EncryptionAlgorithm.AES_ENCRYPTION);
        //            filterBox = new EncryptionFilteringBox(aesEncryption, false);
        //            break;
        //        default:
        //            throw new NotImplementedException(method.ToString());
        //    }
        //    return filterBox;
        //}

        public static IDataFilteringBox GetDeCompressionFilteringBox(CompressionMethods method)
        {
            IDataFilteringBox filterBox;
            switch (method)
            {
                case CompressionMethods.ZLIB_COMPRESSION:
                    filterBox = new ZlibCompressionFilteringBox();
                    break;
                default:
                    throw new NotImplementedException(method.ToString());
            }
            return filterBox;
        }

        public static IDataFilteringBox GetCompressionFilteringBox(CompressionMethods method, int compressionType = -1)
        {
            IDataFilteringBox filterBox;
            switch (method)
            {
                case CompressionMethods.ZLIB_COMPRESSION:
                    filterBox = new ZlibCompressionFilteringBox(compressionType);
                    break;
                default:
                    throw new NotImplementedException(method.ToString());
            }
            return filterBox;
        }

        //public static IDataFilteringBox GetCompressionAndEncryptionFilteringBox(EncryptionAlgorithm encMethod, CompressionMethods compMethod, int compressionType)
        //{
        //    IDataFilteringBox compress = GetCompressionFilteringBox(compMethod, compressionType);
        //    IDataFilteringBox enc = GetEncryptionFilteringBox(encMethod);
        //    IDataFilteringBox box = new MixedFilteringBox(compress, enc, true);
        //    return box;
        //}

        //public static IDataFilteringBox GetDeCompressionAndDecryptionFilteringBox(EncryptionAlgorithm encMethod, CompressionMethods compMethod)
        //{
        //    IDataFilteringBox compress = GetDeCompressionFilteringBox(compMethod);
        //    IDataFilteringBox enc = GetDecryptionFilteringBox(encMethod);
        //    IDataFilteringBox box = new MixedFilteringBox(compress, enc, false);
        //    return box;
        //}

        public static IDataFilteringBox GetCompressionAndEncryptionFilteringBox(EncryptionAlgorithm encMethod, string encryptedKey, CompressionMethods compMethod, int compressionType)
        {
            IDataFilteringBox compress = GetCompressionFilteringBox(compMethod, compressionType);
            IDataFilteringBox enc = GetEncryptionFilteringBox(encMethod, encryptedKey);
            IDataFilteringBox box = new MixedFilteringBox(compress, enc, true);
            return box;
        }

        public static IDataFilteringBox GetDeCompressionAndDecryptionFilteringBox(EncryptionAlgorithm encMethod, string encryptedKey, CompressionMethods compMethod)
        {
            IDataFilteringBox compress = GetDeCompressionFilteringBox(compMethod);
            IDataFilteringBox enc = GetDecryptionFilteringBox(encMethod, encryptedKey);
            IDataFilteringBox box = new MixedFilteringBox(compress, enc, false);
            return box;
        }
    }
}
