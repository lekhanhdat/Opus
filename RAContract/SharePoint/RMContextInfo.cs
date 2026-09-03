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

namespace RMContract.SharePoint
{
    public class RMContextInfo
    {
        public RMContextInfo()
        {
            SharePointContextInfo = new SharePointContextInfo();
            IdentityContextInfo = new IdentityContextInfo();
        }
        public SharePointContextInfo SharePointContextInfo { set; get; }

        public IdentityContextInfo IdentityContextInfo { set; get; }
    }
    public class SharePointContextInfo
    {
        public string AccessToken { get; set; }
        public string ContextToken { get; set; }
        public string RefreshToken { get; set; }
        public string Realm { get; set; }

        public EnvType EnvType { get; set; }
        public string UrlAuthority { get; set; }
        public string WebId { get; set; }

        public string SpHostUrl { get; set; }
        public string SpLanguage { get; set; }
        public string SpClientTag { get; set; }
        public string SpProductNumber { get; set; }
        public string StandardTokens { get; set; }

        public string TargetPrincipalName { get; set; }
        public string MySiteUrl { get; set; }
        public string MySiteAccessToken { get; set; }

        public override string ToString()
        {
            return string.Format("\n" +
                                 "Env Type          : {0} \n" +
                                 "Url Authority     : {1} \n" +
                                 "Web Id            : {2} \n" +
                                 "SP Host Url       : {3} \n" +
                                 "SP Language       : {4} \n" +
                                 "SP Client Tag     : {5} \n" +
                                 "SP Product Number : {6} \n" +
                                 "Standard Tokens   : {7} \n",
                                 EnvType, UrlAuthority, WebId, SpHostUrl, SpLanguage, SpClientTag, SpProductNumber, StandardTokens);
        }
    }

    public enum EnvType
    {
        OnPremise,
        OnCloud,
        Development
    }

    public class IdentityContextInfo
    {
        public string ManagerIdentitySqlId { get; set; }
        public string EndUserIdentitySqlId { get; set; }
        public string WindowsIdentityId { get; set; }
        public string LoginName { get; set; }
        public string DisplayName { get; set; }
        public string Ip { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public override string ToString()
        {
            return string.Format("\n" +
                                "Manager Identity Sql Id : {0} \n" +
                                "End User Identity Sql Id: {1} \n" +
                                "Windows Identity Id     : {2} \n" +
                                "Login Name              : {3} \n" +
                                "Display Name            : {4} \n" +
                                "Ip                      : {5} \n" +
                                "AuthenticationType      : {6} \n",
                                 ManagerIdentitySqlId, EndUserIdentitySqlId, WindowsIdentityId, LoginName, DisplayName, Ip, AuthenticationType);
        }
    }

    public enum AuthenticationType
    {
        Anonymous,
        Authenticated
    }
}
