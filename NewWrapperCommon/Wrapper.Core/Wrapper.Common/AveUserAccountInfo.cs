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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// 这个类表示的是BPOS的account信息，值从Manager获得，用来初始化ObjectModelFactory。如果用的是Server API,不需要初始化这个值
    /// </summary>
    [Serializable]
    public class AveBPOSAccountInfo
    {
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string APPSecret { get; set; }
        public string TenantId { get; set; }
        public string AdminUrl { get; set; }
        public string ClientId { get; set; }
        //spublic string RedirectUri { get; set; }
        [System.Xml.Serialization.XmlIgnore]
        public X509Certificate2 AppCert { get; set; }
        public BposConnectionType ConnectionType { get; set; }
        public AzureRegions AzureRegion { get; set; }


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

        public override string ToString()
        {
            return string.Format("Domain:{0},UserName:{1}", Domain, UserName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetAccountName()
        {
            if (ConnectionType == BposConnectionType.AppToken || ConnectionType == BposConnectionType.MixAuthorize)
            {
                return this.ClientId;
            }
            return this.UserName;
        }

        public void CopyTo(AveBPOSAccountInfo accountInfo)
        {
            accountInfo.Domain = Domain;
            accountInfo.UserName = UserName;
            accountInfo.Password = Password;
            accountInfo.TenantId = TenantId;
            accountInfo.ClientId = ClientId;
            accountInfo.ConnectionType = ConnectionType;
            accountInfo.AppCert = AppCert;
            accountInfo.AzureRegion = AzureRegion;
        }
    }

    public enum BposConnectionType
    {
        ServiceAccount,
        AppToken,
        MixAuthorize,
    }
    public enum AzureRegions
    {
        Unknown = 0,
        AzureGlobal = 1,
        Azure21V = 2,
        AzureGerman = 3,
        AzureUSGov = 4,
        AzureUSGovDoD = 5
    }
}