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
//namespace AvePoint.GCommon.Utility.Cloud
//{
//    using global::Cloud.Sdk.Data.Aos;
//    using System;
//    using System.Collections.Generic;
//    using System.Security.Cryptography.X509Certificates;
//    using System.Threading.Tasks;

//    public class AuthenticationProfileUtility
//    {
        
//        public static List<string> GetTenantIds(string customerId, string aosApiUrl)
//        {
//            var ids = new List<string>();
//#if DEBUG
//            if(!string.IsNullOrEmpty(GCommonRoleConfiguration.AosCustomerId))
//            {
//                customerId = GCommonRoleConfiguration.AosCustomerId;
//            }
//#endif

//            if (!string.IsNullOrEmpty(customerId))
//            {
//                if (string.IsNullOrEmpty(aosApiUrl))
//                {
//                    throw new System.ArgumentNullException("aosApiUrl");
//                }

//                var profiles = GetAuthenticationProfiles(customerId, aosApiUrl);

//                foreach (var item in profiles)
//                {
//                    ids.Add(item.TenantId);
//                }
//            }
//#if DEBUG
//            if (!string.IsNullOrEmpty(GCommonRoleConfiguration.Office365TenantIdForDev))
//            {
//                if (!ids.Contains(GCommonRoleConfiguration.Office365TenantIdForDev))
//                {
//                    ids.Add(GCommonRoleConfiguration.Office365TenantIdForDev);
//                }
//            }
//#endif
            
//            return ids;
//        }

//        /// <summary>
//        /// Aos需要依赖于DocAve证书来进行通信。如果出现证书找不到的问题，请安装server\VCCommon\Shared证书
//        /// </summary>
//        /// <param name="customerId"></param>
//        /// <param name="aosApiUrl"></param>
//        /// <returns></returns>
//        public static List<AuthenticationProfile> GetAuthenticationProfiles(string customerId, string aosApiUrl)
//        {
//            return Execute(() => AosApiClient.AuthenticationService.GetAuthenticationProfiles(customerId, IdentityProviderType.SharePointOnline));
//        }
//        #region Execute Async

//        public static T Execute<T>(Func<Task<T>> func)
//        {
//            try
//            {
//                return Task.Run(async () => await func()).Result;
//            }
//            catch (Exception e)
//            {
//                logger.Error("An error occurred while get data from aos. {0}, {1} {2}", func.Method.Name, e.Message, e);
//                throw;
//            }
//        }

//        #endregion
//    }
//}
