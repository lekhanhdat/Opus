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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Util.Security;

namespace AvePoint.RA.Common.Encryption
{
    public class RMAesGcmEncryptor : IRMAesEncryptor
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AesGcm _instance;
        public RMAesGcmEncryptor(byte[] key)
        {
            _instance = new AesGcm(key);
        }

        public string Encrypt(string plainText)
        {
            try
            {
                return _instance.Encrypt(plainText);
            }
            catch (Exception e)
            {
                logger.Error($"[Aes Gcm Encrypt Failed] {e}");
                throw;
            }
        }

        public string Decrypt(string cipher)
        {
            try
            {
                return _instance.Decrypt(cipher);
            }
            catch (Exception e)
            {
                logger.Error($"[Aes Gcm Decrypt Failed] {e}");
                throw;
            }
        }

        public byte[] Encrypt(byte[] plain)
        {
            try
            {
                return _instance.Encrypt(plain);
            }
            catch (Exception e)
            {
                logger.Error($"[Aes Gcm Encrypt Failed] {e}");
                throw;
            }
        }

        public byte[] Decrypt(byte[] cipher)
        {
            try
            {
                return _instance.Decrypt(cipher);
            }
            catch (Exception e)
            {
                logger.Error($"[Aes Gcm Decrypt Failed] {e}");
                throw;
            }
        }
    }
}
