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

using AvePoint.GCommon;
using AvePoint.RA.CommonUtil;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExchangeUtility
{
    //http://www.neroblanco.co.uk/2016/11/autodiscover-exposed-exchange-office-365/
    class OfficeDataConnectionHelper
    {
        private static RALogger logger = RALogger.GetInstance(typeof(OfficeDataConnectionHelper));
        public const string ENDPOINT_GET_FEDERATION_PROVIDER = @"https://odc.officeapps.live.com/odc/emailhrd/getfederationprovider?domain={0}";//emaildomain
        public const string ENDPOINT_GET_IDENTITY_PROVIDER = @"https://odc.officeapps.live.com/odc/emailhrd/getidp?hm=0&emailAddress={0}";//emailaddress
        public const string ENDPOINT_GET_OPEN_ID_CONFIGURATION = @"https://login.microsoftonline.com/{0}/.well-known/openid-configuration";//emaildomain

        /// <summary>
        /// if the return value is "Global".
        /// it is recommanded to use other method to get the fp name 
        /// even a non-exist domain will return "Global"
        /// </summary>
        /// <param name="emailAddressOrEmailDomain"></param>
        /// <returns>
        /// ChinaCloud: partner.microsoftonline.cn
        /// GermanCloud: microsoftonline.de
        /// Default: Global, do not believe this value, even a non-exist domain will return global
        /// </returns>
        public static string GetFederationProvider(string emailAddressOrEmailDomain)
        {
            var domain = emailAddressOrEmailDomain.GetDomain();
            var uri = string.Format(ENDPOINT_GET_FEDERATION_PROVIDER, domain);
            return HttpGet(uri);
        }

        public static string GetIdentityProvider(string emailAddress)
        {
            var uri = string.Format(ENDPOINT_GET_IDENTITY_PROVIDER, emailAddress);
            return HttpGet(uri);
        }

        public static string GetOpenIdConfiguration(string emailAddressOrEmailDomain)
        {
            var domain = emailAddressOrEmailDomain.GetDomain();
            var uri = string.Format(ENDPOINT_GET_OPEN_ID_CONFIGURATION, domain);
            return HttpGet(uri);
        }
        public static HttpClient client = new HttpClient();

        private static string HttpGet(string requestUri)
        {
            try
            {
                //using (var client = new HttpClient())
                //{
                    var result = client.GetStringAsync(requestUri).ConfigureAwait(false).GetAwaiter().GetResult();
                    logger.Info($@"Request url:{requestUri}
Body:{result}");
                    return result;
                //}
            }
            catch (Exception ex)
            {
                logger.Warn($@"Request url:{requestUri}, error: {ex}");
                return null;
            }
        }
    }
}
