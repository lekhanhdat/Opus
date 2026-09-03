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

using Cloud.Sdk.Data.AosModern;
using System;

namespace Microsoft365.Authentication.TokenProvider.TokenService;

internal static class CloudSdkResourceTypeConvert
{

    public static IdentityProviderType ToIdentityProviderType(this TokenAppType tokenAppType)
    {
        switch (tokenAppType)
        {
            case TokenAppType.CustomAzureApp:
                return IdentityProviderType.CustomAzureApp;
            case TokenAppType.Office365:
                // IdentityProviderType.SharePointOnline is Office365 App
                return IdentityProviderType.Office365;
            case TokenAppType.Exchange:
                return IdentityProviderType.Exchange;
            case TokenAppType.SharePoint:
                return IdentityProviderType.SharePoint;
            default:
                return IdentityProviderType.Office365;
        }
    }
  
    public static AccessTokenResult ConvertToAccessTokenResult(this TokenResult tokenResult, TokenType tokenType)
    {
        if (tokenResult == null)
        {
            return null;
        }
        Exception exception = null;
        switch (tokenResult.ErrorCodeType)
        {
            case ErrorCodeType.Msal:
                if (string.IsNullOrEmpty(tokenResult.Error))
                {
                    exception = new Microsoft365.Authentication.ADAL.AdalException(tokenResult.ErrorCode);
                }
                else
                {
                    exception = new Microsoft365.Authentication.ADAL.AdalException(tokenResult.ErrorCode, tokenResult.Error);
                }
                break;
            case ErrorCodeType.Idcrl:
                if (int.TryParse(tokenResult.ErrorCode, out int hresult))
                {
                    exception = new Microsoft365.Authentication.Token.Idclr.AuthenticationIdclrException(tokenResult.Error, hresult);
                }
                else
                {
                    exception = new Microsoft365.Authentication.Token.Idclr.AuthenticationIdclrException(tokenResult.Error);
                }
                break;
            default:
                break;
        }
        return new AccessTokenResult(tokenResult.AccessToken, tokenResult.Error, tokenResult.ExpiresOn, tokenType)
        {
            Exception=exception
        };
    }
}