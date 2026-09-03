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

namespace Microsoft365.Common.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.IdentityModel.JsonWebTokens;
    using Microsoft365.Common.Logger;

    using Newtonsoft.Json.Linq;

    public class JwtUtil
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(JwtUtil));
        public static string GetPayload(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length >= 2)
                {
                    var payload = parts[1];
                    var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
                    var payloadData = JObject.Parse(payloadJson);
                    return payloadData.ToString();
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Get JsonWebToken Payload failed.Error:{ex}");
            }
            return string.Empty;
        }

        // from JWT spec
        private static byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }
        private static JsonWebTokenHandler handler = new JsonWebTokenHandler();
        public static HashSet<String> GetRolesFromToken(String jwtToken)
        {
            try
            {
                var result = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
                var token = handler.ReadJsonWebToken(jwtToken);
                foreach (var claim in token.Claims)
                {
                    if (String.Equals(claim.Type, "roles", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(claim.Value.Trim());
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Get roles from token failed,payload:{GetPayload(jwtToken)}.Error:{ex}");
                return null;
            }
        }
    }
}