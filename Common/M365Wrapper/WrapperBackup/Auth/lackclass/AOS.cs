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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Util.MSAzure;

namespace AvePoint.Application.AosApi.Invoker
{
    public enum GraphTokenType
    {
        None = 0,
        Graph = 1,
        ExchangeWebService = 2,
        Outlook = 3,
        Teams = 4,
        ExchangeGraph = 10,
        TeamsSkype = 11,
        Delegate = 15,
        PowerBI = 19,
    }

    public enum AosTokenType
    {
        ServiceAccount = 0,
        Exchange = 1,
        SharePoint = 2,
        Office365 = 3,
        CustomAzureApp = 4,
        DelegateApp = 5,
        Yammer = 6,
        CBForM365 = 7,
        CBForSharePointApp = 8,
        CBForExchangeApp = 9
    }

    public class AccountInfo
    {
        public string CustomerId { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string CustomAppId { get; set; }
        /// <summary>
        /// For the service account and account pool user, suggest use account id, account user name also work
        /// For the custom app, need use custom app id
        /// </summary>
        public string Identifier { get; set; }
        public GraphTokenType GraphTokenType { get; set; }
        public string SAResourceUrl { get; set; }
        public string AppResourceUrl { get; set; }
        /// <summary>
        /// O365 Api get token will use user name
        /// AOS token service ould use Identifier prop
        /// </summary>
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool MfaEnabled { get; set; }
        public AppType AppType { get; set; }
        public X509Certificate2 AppCert { get; set; }
        public AzureEnvironment AADEnvironment { get; set; }
        public AosTokenType AosTokenType { get; set; }
        //public TokenResourceType TokenResourceType { get; set; }
    }

   
}
