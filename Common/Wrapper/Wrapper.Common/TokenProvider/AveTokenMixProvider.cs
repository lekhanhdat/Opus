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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft365.Authentication;

namespace AvePoint.Wrapper.Common
{
    public class AveTokenMixProvider : ITokenProvider
    {
        private ITokenProvider[] _spareTokenProviders;
        private ITokenProvider _currentTokenProvider;
        private static AvePoint.GCommon.AveLogger mLogger = AvePoint.GCommon.AveLogger.GetInstance(typeof(AveTokenMixProvider));
        private string realIdentifier;
        private TokenType realTokenType;

        public AveTokenMixProvider(ITokenProvider impleTokenProvider, params ITokenProvider[] spareTokenProviders)
        {
            if (impleTokenProvider == null)
            {
                throw new ArgumentException("Not found AveTokenProvider");
            }
            SetPropertiesToCurrentTokenProvider(impleTokenProvider);
            if (spareTokenProviders == null || !spareTokenProviders.Any())
            {
                mLogger.Warn("Spare tokenProviders are null");
            }
            this._spareTokenProviders = spareTokenProviders;
        }

        public TokenType TokenType
        {
            get
            {
                return this.realTokenType;
            }
        }

        public string Identifier
        {
            get
            {
                return this.realIdentifier;
            }
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return _currentTokenProvider.GetCredential(uri, authType);
        }

        public string GetToken(Uri url, bool refresh = false)
        {
            string token = string.Empty;
            try
            {
                token = _currentTokenProvider.GetToken(url, refresh);
            }
            catch (Exception e)
            {
                mLogger.Error("An error occured when use AveTokenProvider and will use spare token provoder, error:{0}", e);
                token = TryUseSpareTokenProviderToGetToken(url, refresh);
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw;
                }
            }
            return token;
        }

        private string TryUseSpareTokenProviderToGetToken(Uri url, bool refresh = false)
        {
            string token = string.Empty;
            try
            {
                ITokenProvider[] spareTokenProviders;
                string errorMsg = string.Empty;
                if (TryGetSpareTokenProviders(_currentTokenProvider.TokenType, url.ToString(), out spareTokenProviders))
                {
                    foreach (var spareTokenProvider in spareTokenProviders)
                    {
                        try
                        {
                            token = spareTokenProvider.GetToken(url, refresh);
                            mLogger.Info($"Get token with spare token provider successfully. Token Type:{spareTokenProvider.TokenType}");
                            SetPropertiesToCurrentTokenProvider(spareTokenProvider);
                            return token;
                        }
                        catch (Exception ex)
                        {
                            errorMsg = ex.Message;
                            mLogger.Warn("An error occured when use spare TokenProvider, error:{0}", ex);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occured when try to use SpareTokenProviderToGetToken, error:{0}", e);
            }
            return token;
        }

        private bool TryGetSpareTokenProviders(TokenType tokenType, string currentTokenProviderUrl, out ITokenProvider[] spareTokenProviders)
        {
            try
            {
                if (_spareTokenProviders != null && _spareTokenProviders.Any(it => it.TokenType == TokenType))
                {
                    spareTokenProviders = _spareTokenProviders.Where(it => it.TokenType == TokenType).ToArray();
                    return true;
                }
            }
            catch (Exception e)
            {
                mLogger.Error("An error occured when try to get SpareToken Providers, error:{0}", e);
            }
            spareTokenProviders = null;
            return false;
        }

        private void SetPropertiesToCurrentTokenProvider(ITokenProvider tokenProvider)
        {
            this._currentTokenProvider = tokenProvider;
            this.realIdentifier = tokenProvider.Identifier;
            this.realTokenType = tokenProvider.TokenType;
        }
    }
}
