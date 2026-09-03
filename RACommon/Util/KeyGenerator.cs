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
using System.Security.Cryptography;
using System.Text;

namespace AvePoint.RA.Common.Util
{
    public static class KeyGenerator
    {
        private const string _allCharacters = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static RNGCryptoServiceProvider _random = new RNGCryptoServiceProvider();

        public static string Create(int keyLength = 32)
        {
            StringBuilder randomString = new StringBuilder(keyLength);
            for (int i = 0; i < keyLength; ++i)
            {
                randomString.Append(GetRandomCharacter());
            }
            return randomString.ToString();
        }

        private static char GetRandomCharacter()
        {
            byte[] array = new byte[8];
            _random.GetBytes(array);
            decimal maxValue = (decimal)long.MaxValue + 1;
            var randomNum = (int)(Math.Abs(BitConverter.ToInt64(array, 0)) / maxValue * _allCharacters.Length);
            return _allCharacters[randomNum];
        }

    }
}
