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
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Cryptography;
using Microsoft365.Authentication;
using Microsoft365.Authentication.Extension;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// 这个类表示的是BPOS的account信息，值从Manager获得，用来初始化ObjectModelFactory。如果用的是Server API,不需要初始化这个值
    /// 
    /// 对于service account，需要保证UserName和Pwd，并且ConnectionType 为service account，最好包含AdminUrl，其次是Tenant Id。
    /// 对于App Token方式，需要保证AdminUrl, TenantId, ClientId, ConnectionType为AppToken即可。
    /// </summary>
    [Serializable]
    public class AveBPOSAccountInfo
    {
        public string Id { get; set; } //for aosp restore job
        public string Domain { get; set; }
        public string UserName { get; set; }
        public SecureString Password { get; set; }
        public string AdminUrl { get; set; }
        public string TenantId { get; set; }
        public string TenantGroupId { get; set; }
        public string AuthenticationProfileId { get; set; }
        public AvePoint.GCommon.Contract.CentralAdmin.Object.AppType AppType { get; set; }
        public string ClientId { get; set; }
        [System.Xml.Serialization.XmlIgnore]
        public X509Certificate2 AppCert { get; set; }
        [System.Xml.Serialization.XmlIgnore]
        public ITokenProvider TokenProvider { get; set; }
        public BposConnectionType ConnectionType { get; set; }
        public string SecurityGroup { get; set; } //the account pool group that include current user 

        public AveAzureEnvironment AADEnvironment { get; set; }
        public bool ExsitAppProfile { get; set; }
        public void IniFromEncryptString(string accountInfo)
        {
            //AveCrypto cryp = new AveCrypto();
            //var encryption = EncryptionFactory.GetDefaultEncryption();
            string[] userAndPassword = accountInfo.Split('#');
            UserName = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(userAndPassword[0]));
            Password = CspCommunicationWrapper.UnWrapKeyToSecureString(userAndPassword[1]);
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
            var builder = new StringBuilder();
            builder.Append("Domain:");
            builder.Append(Domain);
            builder.Append(", UserName:");
            builder.Append(UserName.LogBase64());
            builder.Append(", IsExistPassword:");
            builder.Append(Password.IsNullOrEmpty() ? "False" : "True");
            builder.Append(", AdminUrl:");
            builder.Append(AdminUrl);
            builder.Append(", TenantId:");
            builder.Append(TenantId);
            builder.Append(", IsExistClientId:");
            builder.Append(ClientId == null ? "False" : "True");
            //builder.Append(", IsExistAppCert:");
            //builder.Append(AppCert == null ? "False" : "True");
            builder.Append(", ConnectionType:");
            builder.Append(ConnectionType);
            builder.Append(", TokenProvider:");
            builder.Append(TokenProvider);
            builder.Append(", SecurityGroup:");
            builder.Append(SecurityGroup);
            builder.Append(", AADEnvironment:");
            builder.Append(AADEnvironment);

            return builder.ToString();
        }

        public override bool Equals(object obj)
        {
            var target = obj as AveBPOSAccountInfo;

            if (target != null)
            {
                if (ConnectionType == target.ConnectionType)
                {
                    if (ConnectionType == BposConnectionType.AppToken)
                    {
                        //不做client id检查，因为不同client id应该有对应的权限，如果需要再添加。
                        //不做admin url检查。
                        return string.Compare(TenantId, target.TenantId, StringComparison.OrdinalIgnoreCase) == 0;
                    }
                    else
                    {
                        return string.Compare(Domain, target.Domain, StringComparison.OrdinalIgnoreCase) == 0 &&
                            string.Compare(UserName, target.UserName, StringComparison.OrdinalIgnoreCase) == 0 &&
                            Password.GetHashCodeV1() == target.Password.GetHashCodeV1();
                    }
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.Domain.GetHashCode()+ this.UserName.GetHashCode();
        }

        public void CopyTo(AveBPOSAccountInfo accountInfo)
        {
            accountInfo.Domain = Domain;
            accountInfo.UserName = UserName;
            accountInfo.Password = Password;
            accountInfo.AdminUrl = AdminUrl;
            accountInfo.TenantId = TenantId;
            accountInfo.ClientId = ClientId;
            accountInfo.ConnectionType = ConnectionType;
            //accountInfo.AppCert = AppCert;
            accountInfo.TokenProvider = TokenProvider;
            accountInfo.SecurityGroup = SecurityGroup;
            accountInfo.AADEnvironment = AADEnvironment;
            accountInfo.TenantGroupId = TenantGroupId;
            accountInfo.AuthenticationProfileId = AuthenticationProfileId;
            accountInfo.AppType = AppType;
        }
    }

    public enum BposConnectionType
    {
        ServiceAccount,
        AppToken,
        Both
    }
}