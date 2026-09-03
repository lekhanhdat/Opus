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
namespace Microsoft365.Authentication
{
    using Microsoft365.Authentication.Configuration;
    using Microsoft365.Common.Logger;
    using Microsoft365.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    public static class AuthenticationFramework
    {
        //private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(AuthenticationFramework));
        static Dictionary<string, IAuthProviderApi> authApis = new Dictionary<string, IAuthProviderApi>(StringComparer.OrdinalIgnoreCase)
        {
            { "",new GeneralAuthProviderApi()}
        };
        //static AuthenticationFramework()
        //{
        //    try
        //    {
        //        InitializeApis(Microsoft365Configuration.AuthenticationConfiguration.AuthenticationElements, authApis);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Initialize the federation authentication failed:{0}", ex);
        //        throw;
        //    }
        //}

        //private static void InitializeApis<T>(IList<AuthenticationElement> collection, Dictionary<string, T> apis)
        //{
        //    lock (apis)
        //    {
        //        var uniqueInstances = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        //        logger.Info($"Start to Initial DICLR Authentication APIs.");
        //        foreach (AuthenticationElement item in collection)
        //        {
        //            try
        //            {
        //                logger.Info($"AuthenticationElement - {item}");
        //                var key = string.Concat(item.Method, item.Parameters?.ToString() ?? new AuthenticationParameter().ToString());

        //                T instance;

        //                if (uniqueInstances.TryGetValue(key, out instance))
        //                {
        //                    apis[item.Domain] = instance;
        //                }
        //                else
        //                {
        //                    Type type = Type.GetType(item.Method, true);

        //                    if (item.Parameters == null)
        //                    {
        //                        instance = (T)Activator.CreateInstance(type);
        //                    }
        //                    else
        //                    {
        //                        instance = (T)Activator.CreateInstance(type, item.Parameters);
        //                    }
        //                    uniqueInstances[key] = instance;
        //                    apis[item.Domain] = instance;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                logger.Error($"Initial IDCLR authentication api for domain {item.Domain} failed.Error:{ex}");
        //            }
        //        }
        //    }
        //}

        public static IAuthProviderApi GetAuthProviderApi(string domainName)
        {
            IAuthProviderApi api;

            if (!authApis.TryGetValue(domainName, out api))
            {
                api = authApis[string.Empty];
            }

            return api;
        }
    }
}