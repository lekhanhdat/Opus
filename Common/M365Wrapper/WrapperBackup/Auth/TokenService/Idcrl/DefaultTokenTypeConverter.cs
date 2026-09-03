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
using Microsoft365.Authentication.Token.Idclr;
using Microsoft365.Authentication.Token.ModernToken;
using Microsoft365.Common.Exception;
using Microsoft365.Common.HttpUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using HttpClientFactory = Microsoft365.Common.HttpUtil.HttpClientFactory;

namespace Microsoft365.Authentication.Token.Modern;

public class DefaultTokenTypeConverter : ITokenTypeConverter
{
    private static RALogger logger = RALogger.GetInstance(typeof(DefaultTokenTypeConverter));
    private const string convertTokenEndpoint = "/_api/SP.OAuth.NativeClient/Authenticate";
    static DefaultTokenTypeConverter()
    {
        Instance = new DefaultTokenTypeConverter();
    }
    public static ITokenTypeConverter Instance { get; private set; }
    public string ConvertBearToCookie(Uri accessSharePointUrl, string bearTokenWithHeader, bool alwaysThrowOnFailure)
    {
        if (!string.IsNullOrEmpty(bearTokenWithHeader))
        {
            return GetCookie(accessSharePointUrl, convertTokenEndpoint, bearTokenWithHeader, alwaysThrowOnFailure);
        }
        logger.Error($"Cannot get IDCRL cookie from bear token.{accessSharePointUrl}");
        if (alwaysThrowOnFailure)
        {
            throw new AuthenticationIdclrException(Mirosoft365ApiErrorMessage.PPCRL_REQUEST_E_UNKNOWNFormat());
        }
        return null;
    }

    private string OutputHttpHeaders(HttpHeaders headers)
    {
        StringBuilder headersInfo = new StringBuilder();
        foreach (var header in headers)
        {
            if (header.Key.ToLower().Contains("cookie"))
            {
                headersInfo.AppendLine($"{header.Key}: {string.Join(", ", header.Value.Select(v => $"(length is {v?.Length})"))}");
            }
            else
            {
                headersInfo.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
        }
        return headersInfo.ToString();
    }

    private string GetCookie(Uri url, string endpoint, string token, bool throwIfFail)
    {
        logger.Info($"modern mode,The url is  {url}");
        var convertRequestSiteUrl = AppSiteAuthenticationConvertUrlCache.GetMappedUrl(url.ToString());
        var uri = new Uri(convertRequestSiteUrl.TrimEnd('/') + endpoint);
        logger.Info($"[AppWebUrlMapping]Get Cookie With Request Url:{uri},OriginalUrl:{url}");
        string cookie = "";
        try
        {
            using var client =HttpClientFactory.CreateHttpClient(null);//stodo
            using var source = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var message = new HttpRequestMessage(HttpMethod.Post, uri);
            message.Headers.Add("X-IDCRL_ACCEPTED", "t");
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var httpResponse = client.SendAsync(message, HttpCompletionOption.ResponseContentRead, source.Token).ConfigureAwait(false).GetAwaiter().GetResult();
            if (httpResponse.Headers.TryGetValues("set-cookie", out IEnumerable<string>? values))
            {
                cookie = values?.FirstOrDefault()?.Split(';').FirstOrDefault();
                logger.Info($"Cookie Length:{cookie?.Length}");
            }
            else
            {
                logger.Warn($"Failed to get Cookie, status code: {httpResponse.StatusCode}, response headers: {Environment.NewLine}{OutputHttpHeaders(httpResponse.Headers)} content headers: {Environment.NewLine}{OutputHttpHeaders(httpResponse.Content.Headers)}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Get cookie using {uri} and cookie Error is {ex}");
        }

        if (string.IsNullOrWhiteSpace(cookie))
        {
            logger.Warn($"Cannot get cookie for {url}");
            if (throwIfFail)
            {
                throw new AuthenticationRequestException(Mirosoft365ApiErrorMessage.CannotGetCookieFormat(new object[]
                {
                    url
                }));
            }
        }
        return cookie;
    }
}