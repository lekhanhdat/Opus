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

using AvePoint.RA.CommonUtil;
using Microsoft365.Authentication.Token.ModernToken;
using System;

namespace Microsoft365.Authentication.Token.Modern;

public class SPOModernAuthenticationProvider
{
    private static RALogger logger = RALogger.GetInstance(typeof(SPOModernAuthenticationProvider));
    private IDelegateUserTokenProvider mDelegateUserTokenProvider;
    private ITokenTypeConverter mTokenTypeConverter;
    private string mDelegateUserTokenProviderFullName;

    public SPOModernAuthenticationProvider(IDelegateUserTokenProvider delegateUserTokenProvider, ITokenTypeConverter tokenTypeConverter)
    {
        mDelegateUserTokenProvider = delegateUserTokenProvider;
        mTokenTypeConverter = tokenTypeConverter;
        mDelegateUserTokenProviderFullName = mDelegateUserTokenProvider.GetType().FullName;
    }


    public string GetAuthenticationCookie(Uri url, bool alwaysThrowOnFailure)
    {
        logger.Info($"Try Get Bear token,and convert to IDCLR token.DelegateUserTokenProviderType:{mDelegateUserTokenProviderFullName},RequestUrl:{url}");
        var token = mDelegateUserTokenProvider.GetUserToken(url);
        return mTokenTypeConverter.ConvertBearToCookie(url, token, alwaysThrowOnFailure);
    }
}