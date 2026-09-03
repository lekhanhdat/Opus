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

namespace HybridServer.Utils
{
    public class RSAHelper
    {
        readonly X509Certificate2 certificate2;
        public RSAHelper(X509Certificate2 certificate2) 
        {
            this.certificate2 = certificate2;
        }
		public string Encrypt(string text)
		{
			bool flag = string.IsNullOrEmpty(text);
			string result;
			if (flag)
			{
				result = text;
			}
			else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                using (RSA rSAPublicKey = certificate2.GetRSAPublicKey())
                {
                    byte[] inArray = rSAPublicKey.Encrypt(bytes, RSAEncryptionPadding.OaepSHA1);
                    result = Convert.ToBase64String(inArray);
                }
			}
			return result;
		}

		public string Decrypt(string text)
		{
			bool flag = string.IsNullOrEmpty(text);
			string result;
			if (flag)
			{
				result = text;
			}
			else
			{
                byte[] bytes = Convert.FromBase64String(text);
                using (RSA rSAPrivateKey = certificate2.GetRSAPrivateKey())
                {
                    byte[] inArray = rSAPrivateKey.Decrypt(bytes, RSAEncryptionPadding.OaepSHA1);
                    result = Encoding.UTF8.GetString(inArray);
                }
               
			}
			return result;
		}
	}
}
