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
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Utility;
using Microsoft365.Authentication;
using Microsoft365.Authentication.Token.BearToken;

namespace AvePoint.Wrapper.Common
{
    public class TokenProviderFactory
    {
        private AveBPOSAccountInfo _info;
        private static AvePoint.GCommon.AveLogger mLogger = AvePoint.GCommon.AveLogger.GetInstance(typeof(TokenProviderFactory));
        private GCommon.Utility.TokenParam _tokenParam;
        public static volatile TokenProviderFactory _instance;
        public readonly static object _lockObj = new object();

        public static TokenProviderFactory GetInstance()
        {
            if (_instance == null)
            {
                lock (_lockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new TokenProviderFactory();
                    }
                }
            }
             return _instance;
        }

        public TokenProviderFactory(AveBPOSAccountInfo info)
        {
            Init(info);
        }

        public TokenProviderFactory()
        {
            
        }

        public ITokenProvider Get(AveBPOSAccountInfo info = null)
        {
            Init(info == null ? _info : info);
            return CreateNewProvider();
        }

        public ITokenProvider Get(BposConnectionType connectionType, AveBPOSAccountInfo info = null)
        {
            Init(info == null ? _info : info);
            ResetConnectionType(connectionType);
            return Get();
        }
        public ITokenProvider GetMix(AveBPOSAccountInfo info = null)
        {
            mLogger.Info("primary check for mix provider.");
            InitForMix(info == null ? _info : info);
            var spareTokenProvider = _info.ConnectionType == BposConnectionType.ServiceAccount ? TokenProviderFactory.GetServiceAccountTokenProviderByOld(_info) : TokenProviderFactory.GetAppprofileTokenProviderByOld(_info);           
            return new AveTokenMixProvider(CreateNewProvider(), spareTokenProvider);
        }

        public static ITokenProvider GetServiceAccountTokenProviderByOld(AveBPOSAccountInfo info)
        {
            info.CheckForIDCRLTokenProvider();
            return new BearerMsalTokenProvider(info.TenantId, info.UserName, info.Password.ToPlainString());
        }
        public static ITokenProvider GetAppprofileTokenProviderByOld(AveBPOSAccountInfo info)
        {
            info.CheckForAppOnlyBearTokenProvider();
            return new AppOnlyBearerTokenProvider(info.TenantId, info.ClientId, info.AppCert, info.AADEnvironment);
        }

        private void Init(AveBPOSAccountInfo info)
        {
            SetBposInfo(info);
            _tokenParam = ConvertToTokenInfo(info);
            mLogger.Info("Init TokenProviderFactory success");
        }

        private void InitForMix(AveBPOSAccountInfo info)
        {
            SetBposInfoForMix(info);
            _tokenParam = ConvertToTokenInfo(info);
            mLogger.Info($"Init TokenProviderFactory success, param:{_tokenParam.ToString()}");
        }

        private void SetBposInfo(AveBPOSAccountInfo info)
        {
            ValidateBPOSInfo(info);
            this._info = info;
        }

        private void SetBposInfoForMix(AveBPOSAccountInfo info)
        {
            ValidateBPOSInfoForMix(info);
            this._info = info;
        }

        private ITokenProvider CreateNewProvider()
        {
            ITokenProvider provider = new AveTokenProvider(_tokenParam);
            return provider;
        }

        private bool ResetConnectionType(BposConnectionType connectionType)
        {
            if (_info.ConnectionType != connectionType)
            {
                _info.ConnectionType = connectionType;
                _tokenParam = ConvertToTokenInfo(_info);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Try convert AveBPOSAccountInfo to TokenParam
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private GCommon.Utility.TokenParam ConvertToTokenInfo(AveBPOSAccountInfo info)
        {
            AvePoint.GCommon.Utility.TokenParam param = new AvePoint.GCommon.Utility.TokenParam()
            {
                CustomerId = info.TenantGroupId,//tenant group id
                TenantId = info.TenantId,//office tenant id
                Identity = _info.ConnectionType == BposConnectionType.ServiceAccount ? info.UserName : info.AuthenticationProfileId,
                SiteUrl = info.AdminUrl,
                SpTokenType = _info.ConnectionType == BposConnectionType.ServiceAccount ? AvePoint.GCommon.Utility.SharePointTokenType.IDCRL : AvePoint.GCommon.Utility.SharePointTokenType.Bearer,
                AppType = info.AppType
            };
            return param;
        }

        private void ValidateBPOSInfo(AveBPOSAccountInfo info)
        {
            info.CheckForAveTokenProvider();
        }

        private void ValidateBPOSInfoForMix(AveBPOSAccountInfo info)
        {
            info.CheckForMixTokenProvider();
        }
    }
}
