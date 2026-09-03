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

namespace AvePoint.Wrapper.Core.Common
{
    /// <summary>
    /// login name的解码
    /// 
    /// provider: i (user), c (group)
    /// :
    /// 0
    /// claim type: 
    /// '!', SPClaimTypes.IdentityProvider
    /// '"',SPClaimTypes.UserIdentifier
    ///'#',SPClaimTypes.UserLogonName
    ///'$',SPClaimTypes.DistributionListClaimType
    ///'%',SPClaimTypes.FarmId
    ///'&',"http://schemas.microsoft.com/sharepoint/2009/08/claims/processidentitysid"
    ///'\'',"http://schemas.microsoft.com/sharepoint/2009/08/claims/processidentitylogonname"
    ///'(',SPClaimTypes.IsAuthenticated
    ///')',"http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid"
    ///'*',"http://schemas.microsoft.com/ws/2008/06/identity/claims/primarygroupsid"
    ///'+',"http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid"
    ///'-',"http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ///'.',ClaimTypes.Anonymous
    ///'/',ClaimTypes.Authentication
    ///'0',ClaimTypes.AuthorizationDecision
    ///'1',ClaimTypes.Country
    ///'2',ClaimTypes.DateOfBirth
    ///'3',ClaimTypes.DenyOnlySid
    ///'4',ClaimTypes.Dns
    ///'5',ClaimTypes.Email
    ///'6',ClaimTypes.Gender
    ///'7',ClaimTypes.GivenName
    ///'8',ClaimTypes.Hash
    ///'9',ClaimTypes.HomePhone
    ///'<',ClaimTypes.Locality
    ///'=',ClaimTypes.MobilePhone
    ///'>',ClaimTypes.Name
    ///'?',ClaimTypes.NameIdentifier
    ///'@',ClaimTypes.OtherPhone
    ///'[',ClaimTypes.PostalCode
    ///'\\',ClaimTypes.PPID
    ///']',ClaimTypes.Rsa
    ///'^',ClaimTypes.Sid
    ///'_',ClaimTypes.Spn
    ///'`',ClaimTypes.StateOrProvince
    ///'a',ClaimTypes.StreetAddress
    ///'b',ClaimTypes.Surname
    ///'c',ClaimTypes.System
    ///'d',ClaimTypes.Thumbprint
    ///'e',ClaimTypes.Upn
    ///'f',ClaimTypes.Uri
    ///'g',ClaimTypes.Webpage
    ///'h',SPClaimTypes.ProviderUserKey
    /// 
    /// Claim Value Type: .(string) +(rfc822 name)
    /// 
    /// Original Issuer Type: w (119 windows), s (115 security token service) m (asp.net membership provider) r (asp.net role provider) t (116 trusted STS) c (99 ClaimProvider) f(102 forms)  (SPOriginalIssuerType)
    /// | claim value (for w & s)
    /// | name of original issuer | claim value (for m, r, t, c, f)
    /// 
    /// </summary>
    public class SPUserLoginNameDecoder
    {
        private bool isClaims;
        private bool isEmail;
        private SPUserOriginalIssuerType originalIssueType;
        private string loginName;

        /// <summary>
        /// Original Issue Type
        /// </summary>
        internal SPUserOriginalIssuerType OriginalIssueType { get { return originalIssueType; } }

        /// <summary>
        /// Is Claims
        /// </summary>
        public bool IsClaims { get { return isClaims; } }

        /// <summary>
        /// Is Email
        /// </summary>
        public bool IsEmail { get { return isEmail; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginName"></param>
        public SPUserLoginNameDecoder(string loginName)
        {
            this.loginName = loginName;
            Decode();
        }

        private void Decode()
        {
            /*
             * 如果满足下面条件:
             * 1. 以i或者c开头
             * 2. 第二个字符是:
             * 3. 第六个字符是|
             * 
             * 基本上可以确定是claims base用户, 例如i:0#.w|spcartoon\long 
             */
            if (loginName.Length > 7 && loginName[6] == '|' && 
                (loginName[0] =='i' || loginName[0] == 'c') && loginName[1] == ':')
            {
                isClaims = true;
                originalIssueType = (SPUserOriginalIssuerType)loginName[5];
                if(originalIssueType != SPUserOriginalIssuerType.Windows && originalIssueType != SPUserOriginalIssuerType.SecurityTokenService)
                {
                    isEmail = loginName.IndexOf('@', 7) > 0;
                }
            }
            else
            {
                #region check is FBA or email format
                var isFBA = false;
                var isEmail = false;
                var isWindows = false;
                foreach (var item in loginName)
                {
                    switch(item)
                    {
                        case ':':
                            isFBA = true;
                            break;
                        case '\\':
                            isWindows = true;
                            break;
                        case '@':
                            isEmail = true;
                            break;
                    }
                }

                if(isFBA)
                {
                    originalIssueType = SPUserOriginalIssuerType.Forms;
                    if(isEmail)
                    {
                        this.isEmail = isEmail;
                    }
                }
                else if(isWindows)
                {
                    originalIssueType = SPUserOriginalIssuerType.Windows;
                }
                #endregion
            }
        }

        /// <summary>
        /// Get original claim value
        /// </summary>
        /// <returns></returns>
        internal string GetClaimValue()
        {
            if(isClaims)
            {
                return loginName.Substring(7);
            }

            return loginName;
        }

        /// <summary>
        /// change the | to : if the user is FBA user
        /// </summary>
        /// <returns></returns>
        internal string GetFormattedClaimValue()
        {
            if(isClaims)
            {
                var name = loginName.Substring(7);

                if(originalIssueType != SPUserOriginalIssuerType.Windows && originalIssueType != SPUserOriginalIssuerType.SecurityTokenService)
                {
                    name = name.Replace('|', ':');
                }

                return name;
            }

            return loginName;
        }

        /// <summary>
        /// The prefix of user login name
        /// </summary>
        public string Header
        {
            get
            {
                if (isClaims)
                {
                    return loginName.Substring(0, 7);
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Encode the user name to full name
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="loginName"></param>
        /// <param name="isFBA"></param>
        /// <returns></returns>
        internal static string Encode(string prefix, string loginName, bool isFBA)
        {
            if(isFBA)
            {
                return string.Concat(prefix, loginName.Replace(':', '|'));
            }

            return string.Concat(prefix, loginName);
        }

        internal bool ReplaceDomain(FuncWithOut<string, string, bool> getMappingDomainName, out string fullName)
        {
            fullName = null;

            if(getMappingDomainName == null)
            {
                return false;
            }

            if (originalIssueType == SPUserOriginalIssuerType.Windows)
            {
                fullName = ReplaceWindowsDomain(getMappingDomainName, isClaims);
            }
            else if (originalIssueType == SPUserOriginalIssuerType.Forms || originalIssueType == SPUserOriginalIssuerType.ASPNETMemberShip
                || originalIssueType == SPUserOriginalIssuerType.ASPNETRole)
            {
                fullName = ReplaceFBAProvider(getMappingDomainName, isClaims, isEmail);
            }
            else
            {
                fullName = ReplaceClaimsProvider(getMappingDomainName, isEmail);
            }

            return !string.IsNullOrEmpty(fullName);
        }

        private string ReplaceClaimsProvider(FuncWithOut<string, string, bool> getMappingDomainName, bool isEmail)
        {
            string fullName = null;
            var index = loginName.IndexOf('\\');

            if(index > 0)
            {
                var domainName = loginName.Substring(0, index);
                string destDomainName = null;

                if(getMappingDomainName(domainName, out destDomainName))
                {
                    fullName = string.Concat(destDomainName, loginName.Substring(index));
                }
            }
            else if(isEmail)
            {
                var emailIndex = loginName.IndexOf('@', 7);

                if (emailIndex > 0)
                {
                    var domainName = string.Concat("{0}", loginName.Substring(emailIndex));
                    string destDomainName = null;

                    if (getMappingDomainName(domainName, out destDomainName))
                    {
                        fullName = string.Format(destDomainName, loginName.Substring(0, emailIndex));
                    }
                    else
                    {
                        index = loginName.IndexOf('|', 7);
                        if (index > 0)
                        {
                            domainName = string.Concat(loginName.Substring(0, index + 1), domainName);

                            if (getMappingDomainName(domainName, out destDomainName))
                            {
                                fullName = string.Format(destDomainName, loginName.Substring(index + 1, emailIndex - index - 1));
                            }
                        }
                    }
                }
            }

            return fullName;
        }

        private string ReplaceFBAProvider(FuncWithOut<string, string, bool> getMappingDomainName, bool isClaims, bool isEmail)
        {
            string fullName = null;
            int index = 0;

            if (isClaims)
            {
                index = loginName.IndexOf('|', 7);
            }
            else
            {
                index = loginName.IndexOf(':');
            }

            if (index > 0)
            {
                string domainName = null;

                if(isClaims)
                {
                    domainName = string.Concat(loginName.Substring(7, index - 7), ":");
                }
                else
                {
                    domainName = loginName.Substring(0, index + 1);
                }
                string destDomainName = null;

                if (getMappingDomainName(domainName, out destDomainName))
                {
                    if (!isClaims)
                    {
                        fullName = string.Concat(destDomainName, loginName.Substring(index + 1));
                    }
                    else
                    {
                        fullName = string.Concat(loginName.Substring(0, 7), destDomainName.Replace(':', '|'), loginName.Substring(index + 1));
                    }
                }
                else if (isEmail)
                {
                    var emailIndex = loginName.IndexOf('@', index);

                    if (emailIndex > 0)
                    {
                        domainName = string.Concat("{0}", loginName.Substring(emailIndex));
                        if (getMappingDomainName(domainName, out destDomainName))
                        {
                            fullName = string.Format(destDomainName, loginName.Substring(0, emailIndex));
                        }
                        else
                        {
                            domainName = string.Concat(loginName.Substring(0, index + 1), domainName);

                            if (getMappingDomainName(domainName, out destDomainName))
                            {
                                fullName = string.Format(destDomainName, loginName.Substring(index + 1, emailIndex - index - 1));
                            }
                        }
                    }
                }
            }

            return fullName;
        }

        private string ReplaceWindowsDomain(FuncWithOut<string, string, bool> getMappingDomainName, bool isClaims)
        {
            string fullName = null;

            var index = loginName.IndexOf('\\');
            if (index > 0)
            {
                var domainName = loginName.Substring(0, index);
                string destDomainName = null;
                if (getMappingDomainName(domainName, out destDomainName))
                {
                    fullName = string.Concat(destDomainName, loginName.Substring(index));
                }
                else if(isClaims)
                {
                    domainName = loginName.Substring(7, index - 7);
                    if (getMappingDomainName(domainName, out destDomainName))
                    {
                        fullName = string.Concat(loginName.Substring(0, 7), destDomainName, loginName.Substring(index));
                    }
                }
            }

            return fullName;
        }


    }

    /// <summary>
    /// Original Issuer Type
    /// </summary>
    internal enum SPUserOriginalIssuerType
    {
        Windows = 119,
        Forms = 102,
        TrustedProvider = 116,
        SecurityTokenService = 115,
        ClaimProvider = 99,
        ASPNETMemberShip = 109,
        ASPNETRole = 114,
    }
}
