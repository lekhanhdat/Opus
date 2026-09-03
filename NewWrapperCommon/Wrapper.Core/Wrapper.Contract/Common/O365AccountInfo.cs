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
using System.Text;
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.Wrapper.Core.Common
{
    /// <summary>
    /// 这个类表示的是BPOS的account信息，值从Manager获得，用来初始化ObjectModelFactory。如果用的是Server API,不需要初始化这个值
    /// </summary>
    [Serializable]
    public class O365AccountInfo
    {
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public void IniFromEncryptString(string accountInfo)
        {
            //AveCrypto cryp = new AveCrypto();
            //var encryption = EncryptionFactory.GetDefaultEncryption();
            string[] userAndPassword = accountInfo.Split('#');
            UserName = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(userAndPassword[0]));//encryption.DecryptString(userAndPassword[0]);
            Password = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(userAndPassword[1]));//encryption.DecryptString(userAndPassword[1]);
            string[] domainAndUsername = UserName.Split('\\');
            if (domainAndUsername.Length == 2)
            {
                Domain = domainAndUsername[0];
                UserName = domainAndUsername[1];
            }
            else
            {
                UserName = domainAndUsername[0];
            }
        }
    }
}
