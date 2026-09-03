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
using AvePoint.Common;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace AvePoint.RA.Web.Common.Authorize
{
    /// <summary>
    /// 关于jwttoken方法静态类
    /// </summary>
    public static class JwtTokenHelper
    {
        public static string TenantGroupIdClaimName = "TenantGroupId";
        public static string UserNameClaimName = "UserName";
        public static string UserIdClaimName = "UserId";
        /// <summary>
        /// 获取JwtToken
        /// </summary>
        /// <param name="tenantGroupId"></param>
        /// <returns></returns>
        public static string GetJwtToken(string tenantGroupId, string userId,string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Iat, $"{DateTime.UtcNow}"),
                new Claim(JwtRegisteredClaimNames.Nbf, $"{DateTime.UtcNow}"),
                new Claim(JwtRegisteredClaimNames.Exp, $"{DateTime.UtcNow.AddMinutes(10)}"),
            };
            if (!string.IsNullOrEmpty(tenantGroupId))
            {
                var rsaHelper = new RsaHelper(GetCertificate());
                claims.Add(new Claim(TenantGroupIdClaimName, rsaHelper.Encrypt(tenantGroupId)));
                claims.Add(new Claim(UserIdClaimName, rsaHelper.Encrypt(userId)));
                claims.Add(new Claim(UserNameClaimName, rsaHelper.Encrypt(userName)));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetCertificate().Thumbprint));
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public static string Decrypt(string encryptString)
        {
            var rsaHelper = new RsaHelper(GetCertificate());
            return rsaHelper.Decrypt(encryptString);
        }

        private static X509Certificate2 GetCertificate()
        {

            return null;
        }
    }
}