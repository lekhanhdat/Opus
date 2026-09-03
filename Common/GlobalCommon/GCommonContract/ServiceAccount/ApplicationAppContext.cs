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
//using System;

//namespace AvePoint.GCommon.Contract.CentralAdmin.Object;

//public class AuthenticationContextBase
//{
//    public virtual string? CustomerId { get; set; }
//    public virtual string? TenantId { get; set; }
//    public virtual string? SharePointAdminUrl { get; set; }
//    public AADEnvironment AADEnvironment { get; set; }

//    public override string ToString()
//    {
//        return $"CustomerId:{CustomerId},TenantId:{TenantId},Environment:{AADEnvironment}";
//    }
//}
//public class ApplicationAppContext: AuthenticationContextBase
//{
//    public string? AppClientId { get; set; }
//    public string? AppId { get; set; }
//    public AppType? AppType { get; set; }
//    public string? AuthorizationUserName { get; set; }

//    public override string ToString()
//    {
//        return $"[ApplicationAppContext]{base.ToString()},AppId:{AppId},AppType:{AppType},AppClientId:{AppClientId}";
//    }
//}

//public class ServiceAccountContext: AuthenticationContextBase
//{
//    public string? UserName { get; set; }
//    public bool? EnableMfa { get; set; }

//    public override string ToString()
//    {
//        return $"[ServiceAccountContext]{base.ToString()},UserName:{UserName},EnableMfa:{EnableMfa}";
//    }
//}

//public static class ApplicationAppContextExtensions
//{
//    public static BposInfo ToBposInfo(this ApplicationAppContext applicationAppContext)
//    {
//        return new BposInfo
//        {
//            CustomerId = applicationAppContext.CustomerId,
//            ConnectionType = BposConnectionType.AppToken,
//            AppType = applicationAppContext.AppType.Value,
//            SiteUrl = string.Empty,
//            AADEnvType = applicationAppContext.AADEnvironment,
//            UserAccountInfo = new BposUserAccountInfo
//            {
//                CustomerAppId = applicationAppContext.AppId,
//                AppProfileUsername = applicationAppContext.AuthorizationUserName,
//                TenantId = applicationAppContext.TenantId,
//                AppType = applicationAppContext.AppType.Value,
//                AppClientId = applicationAppContext.AppClientId,
//                AdminUrl= applicationAppContext.SharePointAdminUrl
//            }
//        };
//    }

//    public static BposInfo ToBposInfo(this AuthenticationContextBase context)
//    {
//        ArgumentNullException.ThrowIfNull(context);
//        if (context is ServiceAccountContext)
//        {
//           return (context as ServiceAccountContext).ToBposInfo();
//        }

//        if (context is ApplicationAppContext)
//        {
//            return (context as ApplicationAppContext).ToBposInfo();
//        }

//        throw new System.NotSupportedException($"{context.GetType().Name} is not supported");
//    }

//    public static BposInfo ToBposInfo(this ServiceAccountContext serviceAccountContext)
//    {
//        return new BposInfo
//        {
//            CustomerId = serviceAccountContext.CustomerId,
//            ConnectionType = BposConnectionType.ServiceAccount,
//            SiteUrl = string.Empty,
//            AADEnvType = serviceAccountContext.AADEnvironment,
//            UserAccountInfo = new BposUserAccountInfo
//            {
//                TenantId = serviceAccountContext.TenantId,
//                AdminUrl = serviceAccountContext.SharePointAdminUrl,
//                ServiceAccountUsername= serviceAccountContext.UserName,
//                ServiceAccountIsMFA= serviceAccountContext?.EnableMfa??false
//            }
//        };
//    }
//}
