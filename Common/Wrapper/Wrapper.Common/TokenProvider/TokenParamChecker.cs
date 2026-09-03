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
using AvePoint.Wrapper.Common;
using System;
using AvePoint.RA.I18N.Core;

namespace AvePoint.Wrapper.Common
{
    public static class TokenParamCheckExtention
    {
        /// <summary>
        /// Get Token Param Check option
        /// </summary>
        /// <param name="isServiceAccount">true：service account，false:app profile</param>
        public static TokenParamCheckOption GetAveTokenProviderCheckOption(bool isServiceAccount)
        {
            TokenParamCheckOption option = TokenParamCheckOption.ConnectionType | TokenParamCheckOption.AdminUrl | TokenParamCheckOption.TenantGroupId | TokenParamCheckOption.TenantId;
            option = isServiceAccount ? option | TokenParamCheckOption.UserName : option | TokenParamCheckOption.AuthenticationprofileId;
            return option;
        }

        public static TokenParamCheckOption GetMixTokenProviderCheckOption(bool isServiceAccount)
        {
            TokenParamCheckOption option = GetAveTokenProviderCheckOption(isServiceAccount);
            option = isServiceAccount ? (option | TokenParamCheckOption.Password) : (option | TokenParamCheckOption.ClientId | TokenParamCheckOption.AppCert);
            return option;
        }

        public static TokenParamCheckOption GetIDCRLTokenProviderCheckOption()
        {
            TokenParamCheckOption option = TokenParamCheckOption.ConnectionType | TokenParamCheckOption.AdminUrl | TokenParamCheckOption.UserName | TokenParamCheckOption.Password; ;
            return option;
        }

        public static TokenParamCheckOption GetAppOnlyBearTokenProviderCheckOption()
        {
            TokenParamCheckOption option = TokenParamCheckOption.ConnectionType | TokenParamCheckOption.AdminUrl | TokenParamCheckOption.TenantId | TokenParamCheckOption.ClientId | TokenParamCheckOption.AppCert;
            return option;
        }

        public static void CheckForMixTokenProvider(this AveBPOSAccountInfo info)
        {
            AvePoint.Common.Singleton<TokenParamChecker>.SingletonInstance.Check(info, GetMixTokenProviderCheckOption(info.ConnectionType == BposConnectionType.ServiceAccount));
        }

        public static void CheckForAveTokenProvider(this AveBPOSAccountInfo info)
        {
            AvePoint.Common.Singleton<TokenParamChecker>.SingletonInstance.Check(info, GetAveTokenProviderCheckOption(info.ConnectionType == BposConnectionType.ServiceAccount));
        }

        public static void CheckForIDCRLTokenProvider(this AveBPOSAccountInfo info)
        {
            AvePoint.Common.Singleton<TokenParamChecker>.SingletonInstance.Check(info, GetIDCRLTokenProviderCheckOption());
        }

        public static void CheckForAppOnlyBearTokenProvider(this AveBPOSAccountInfo info)
        {
            AvePoint.Common.Singleton<TokenParamChecker>.SingletonInstance.Check(info, GetAppOnlyBearTokenProviderCheckOption());
        }

        public static void CheckForMixTokenProvider(this TokenParam info)
        {
            AvePoint.Common.Singleton<TokenParamChecker>.SingletonInstance.Check(info, GetMixTokenProviderCheckOption(info.SpTokenType == SharePointTokenType.IDCRL));
        }

        public static void CheckForAveTokenProvider(this TokenParam info)
        {
            AvePoint.Common.Singleton<TokenParamChecker>.SingletonInstance.Check(info, GetAveTokenProviderCheckOption(info.SpTokenType == SharePointTokenType.IDCRL));
        }

        public static void CheckForIDCRLTokenProvider(this TokenParam info)
        {
            AvePoint.Common.Singleton<TokenParamChecker>.SingletonInstance.Check(info, GetIDCRLTokenProviderCheckOption());
        }

        public static void CheckForAppOnlyBearTokenProvider(this TokenParam info)
        {
            AvePoint.Common.Singleton<TokenParamChecker>.SingletonInstance.Check(info, GetAppOnlyBearTokenProviderCheckOption());
        }
    }

    [Flags]
    public enum TokenParamCheckOption
    {
        ConnectionType = 1,
        TenantGroupId = 2,
        UserName = 4,
        Password = 8,
        TenantId = 16,
        AuthenticationprofileId = 32,
        AppCert = 64,
        ClientId = 128,
        AdminUrl = 256,
        All = Int32.MaxValue
    }

    public interface ITokenParamChecker
    {
        void Check(AveBPOSAccountInfo info, TokenParamCheckOption option);
        void Check(AvePoint.GCommon.Utility.TokenParam info, TokenParamCheckOption option);
    }

    public class TokenParamChecker : ITokenParamChecker, AvePoint.Common.ISingleton
    {
        private TokenParamChecker() { }
        public void Check(AveBPOSAccountInfo info, TokenParamCheckOption option)
        {
            if (info == null) { return; }
            bool needForceCheck = option == TokenParamCheckOption.All;
            if (option.HasFlag(TokenParamCheckOption.ConnectionType) || needForceCheck)
            {
                if (info.ConnectionType != BposConnectionType.ServiceAccount && info.ConnectionType != BposConnectionType.AppToken)
                {
                    //$"Unknown connection type:{info.ConnectionType}"
                    throw new AveWrapperInvalidDataException();
                }
            }
            if (option.HasFlag(TokenParamCheckOption.TenantGroupId) || needForceCheck)
            {
                //tenant group id
                Guid tenantGroupId;
                if (string.IsNullOrEmpty(info.TenantGroupId) || !Guid.TryParse(info.TenantGroupId, out tenantGroupId))
                {
                    throw new ArgumentException($"Token param tenantGroupId:{info.TenantGroupId} is invalid, token type:{info.ConnectionType}");
                }
            }
            if (option.HasFlag(TokenParamCheckOption.UserName) || needForceCheck)
            {
                //service account user name
                if (string.IsNullOrEmpty(info.UserName))
                {
                    throw new ArgumentException("RM_DSB_NoTenant_Or_Profile");
                }
            }
            if (option.HasFlag(TokenParamCheckOption.Password) || needForceCheck)
            {
                if (info.Password.IsNullOrEmpty())
                {
                    //SAAS-40842
                    throw new ArgumentException($"Token param Password is null, token type:{info.ConnectionType}");
                }
            }
            if (option.HasFlag(TokenParamCheckOption.TenantId) || needForceCheck)
            {
                // tenant id
                Guid office365TenantId;
                if (string.IsNullOrEmpty(info.TenantId) || !Guid.TryParse(info.TenantId, out office365TenantId))
                {
                    throw new ArgumentException($"Token param tenantId:{info.TenantId} is invalid, token type:{info.ConnectionType}");
                }
            }
            if (option.HasFlag(TokenParamCheckOption.AuthenticationprofileId) || needForceCheck)
            {
                //authenticationprofileId
                Guid authenticationprofileId;
                if (string.IsNullOrEmpty(info.AuthenticationProfileId) || !Guid.TryParse(info.AuthenticationProfileId, out authenticationprofileId))
                {
                    throw new ArgumentException($"Token param authenticationprofileId:{info.AuthenticationProfileId} is invalid, token type:{info.ConnectionType}");
                }
            }
            //if (option.HasFlag(TokenParamCheckOption.AppCert) || needForceCheck)
            //{
            //    if (info.AppCert == null)
            //    {
            //        throw new ArgumentException($"Token param AppCert is null, token type:{info.ConnectionType}");
            //    }
            //}
            if (option.HasFlag(TokenParamCheckOption.ClientId) || needForceCheck)
            {
                Guid clientId;
                if (string.IsNullOrEmpty(info.ClientId) || !Guid.TryParse(info.ClientId, out clientId))
                {
                    //SAAS-40842
                    throw new ArgumentException($"Token param authenticationprofileId is invalid, token type:{info.ConnectionType}");
                }
            }
            if (option.HasFlag(TokenParamCheckOption.AdminUrl) || needForceCheck)
            {
                //AdminUrl
                if (string.IsNullOrEmpty(info.AdminUrl))
                {
                    throw new ArgumentException($"Token param AdminUrl:{info.AdminUrl} is null, token type:{info.ConnectionType}");
                }
            }
        }

        public void Check(TokenParam info, TokenParamCheckOption option)
        {
            if (info == null) { return; }
            bool needForceCheck = option == TokenParamCheckOption.All;
            if (option.HasFlag(TokenParamCheckOption.ConnectionType) || needForceCheck)
            {
                if (info.SpTokenType != SharePointTokenType.IDCRL && info.SpTokenType != SharePointTokenType.Bearer)
                {
                    //$"Unknown connection type:{info.ConnectionType}"
                    throw new AveWrapperInvalidDataException();
                }
            }
            if (option.HasFlag(TokenParamCheckOption.TenantGroupId) || needForceCheck)
            {
                //tenant group id
                Guid tenantGroupId;
                if (string.IsNullOrEmpty(info.CustomerId) || !Guid.TryParse(info.CustomerId, out tenantGroupId))
                {
                    throw new ArgumentException($"Token param tenantGroupId:{info.CustomerId} is invalid, token type:{info.CustomerId}");
                }
            }
            if (option.HasFlag(TokenParamCheckOption.UserName) || needForceCheck)
            {
                //service account user name
                if (string.IsNullOrEmpty(info.Identity))
                {
                    throw new ArgumentException("RM_DSB_NoTenant_Or_Profile");
                }
            }
            if (option.HasFlag(TokenParamCheckOption.Password) || needForceCheck)
            {

            }
            if (option.HasFlag(TokenParamCheckOption.TenantId) || needForceCheck)
            {
                // tenant id
                Guid office365TenantId;
                if (string.IsNullOrEmpty(info.TenantId) || !Guid.TryParse(info.TenantId, out office365TenantId))
                {
                    throw new ArgumentException($"Token param tenantId:{info.TenantId} is invalid, token type:{info.SpTokenType}");
                }
            }
            if (option.HasFlag(TokenParamCheckOption.AuthenticationprofileId) || needForceCheck)
            {
                //authenticationprofileId
                Guid authenticationprofileId;
                if (string.IsNullOrEmpty(info.Identity) || !Guid.TryParse(info.Identity, out authenticationprofileId))
                {
                    throw new ArgumentException($"Token param authenticationprofileId:{info.Identity} is invalid, token type:{info.SpTokenType}");
                }
            }
            if (option.HasFlag(TokenParamCheckOption.AppCert) || needForceCheck)
            {
                // certificate
            }
            if (option.HasFlag(TokenParamCheckOption.ClientId) || needForceCheck)
            {
                //ClientId
            }
            if (option.HasFlag(TokenParamCheckOption.AdminUrl) || needForceCheck)
            {
                //SiteUrl
                if (string.IsNullOrEmpty(info.SiteUrl))
                {
                    throw new ArgumentException($"Token param SiteUrl:{info.SiteUrl} is null, token type:{info.SpTokenType}");
                }
            }
        }
    }
}
