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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgumentCheck = AvePoint.GCommon.Utility.ArgumentCheck;

namespace AvePoint.RA.Common.Util
{
    public class TenantUtil
    {
        public static string GetAveId(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            var hash = HashCodeHelper.ToMD5HashCode(text.ToLowerInvariant());
            return hash.Replace("-", "").Substring(8, 16);
        }
        public static void RunUnderTenant<T>(string tenantId, string email, Action<List<T>> action, List<T> args)
        {
            ThrowUtil.ThrowIfNull(tenantId, "tenantId");
            var originalTenantId = TenantLocalValue.LogonGroupId;
            var originalTenantEmail = TenantLocalValue.LogonUserEmail;
            var originalUserId = TenantLocalValue.LogonUserId;
            try
            {
                TenantLocalValue.LogonGroupId = tenantId;
                TenantLocalValue.LogonUserEmail = email;
                action(args);
            }
            finally
            {
                TenantLocalValue.LogonGroupId = originalTenantId;
                TenantLocalValue.LogonUserEmail = originalTenantEmail;
                TenantLocalValue.LogonUserId = originalUserId;
            }
        }

        public static Task RunUnderTenantAsync<T>(string tenantId, string email, string clientIP, string partnerUser, Func<List<T>,Task> action, List<T> args)
        {
            ThrowUtil.ThrowIfNull(tenantId, "tenantId");
            var originalTenantId = TenantLocalValue.LogonGroupId;
            var originalTenantEmail = TenantLocalValue.LogonUserEmail;
            var originalPartnerUser = TenantLocalValue.PartnerUser;
            var originalUserId = TenantLocalValue.LogonUserId;
            var originalClientIP = ClientRequestLocalValue.ClientIP;
            try
            {
                TenantLocalValue.LogonGroupId = tenantId;
                TenantLocalValue.LogonUserEmail = email;
                TenantLocalValue.PartnerUser = partnerUser;
                ClientRequestLocalValue.ClientIP = clientIP;
                return action(args);
            }
            finally
            {
                TenantLocalValue.LogonGroupId = originalTenantId;
                TenantLocalValue.LogonUserEmail = originalTenantEmail;
                TenantLocalValue.PartnerUser = originalPartnerUser;
                TenantLocalValue.LogonUserId = originalUserId;
                ClientRequestLocalValue.ClientIP = originalClientIP;
            }
        }

        public static T RunUnderTenant<T>(TenantContext context, Func<T> action)
        {
            ArgumentCheck.NotNull(context, nameof(context));
            var originalTenantId = TenantLocalValue.LogonGroupId;
            var originalTenantEmail = TenantLocalValue.LogonUserEmail;
            var originalUserId = TenantLocalValue.LogonUserId;
            try
            {
                TenantLocalValue.LogonGroupId = context.CustomerId;
                TenantLocalValue.LogonUserEmail = context.UserEmail;
                TenantLocalValue.LogonUserId = context.UserId;
                return action();
            }
            finally
            {
                TenantLocalValue.LogonGroupId = originalTenantId;
                TenantLocalValue.LogonUserEmail = originalTenantEmail;
                TenantLocalValue.LogonUserId = originalUserId;
            }
        }
        public static T RunUnderTenant<T>(string tenantId, Func<string,bool,bool,T> action,string storageId)
        {
            var originalTenantId = TenantLocalValue.LogonGroupId;
            var originalTenantEmail = TenantLocalValue.LogonUserEmail;
            var originalUserId = TenantLocalValue.LogonUserId;
            try
            {
                TenantLocalValue.LogonGroupId = tenantId;
                return action(storageId,false,false);
            }
            finally
            {
                TenantLocalValue.LogonGroupId = originalTenantId;
                TenantLocalValue.LogonUserEmail = originalTenantEmail;
                TenantLocalValue.LogonUserId = originalUserId;
            }
        }

        public static Task<T> RunUnderTenantAsync<T>(TenantContext context, Func<Task<T>> action)
        {
            ArgumentCheck.NotNull(context, nameof(context));
            var originalTenantId = TenantLocalValue.LogonGroupId;
            var originalTenantEmail = TenantLocalValue.LogonUserEmail;
            var originalUserId = TenantLocalValue.LogonUserId;
            try
            {
                TenantLocalValue.LogonGroupId = context.CustomerId;
                TenantLocalValue.LogonUserEmail = context.UserEmail;
                TenantLocalValue.LogonUserId = context.UserId;
                return action();
            }
            finally
            {
                TenantLocalValue.LogonGroupId = originalTenantId;
                TenantLocalValue.LogonUserEmail = originalTenantEmail;
                TenantLocalValue.LogonUserId = originalUserId;
            }
        }

        public static void RunUnderTenant(string tenantId, string email, Action action)
        {
            ThrowUtil.ThrowIfNull(tenantId, "tenantId");
            var originalTenantId = TenantLocalValue.LogonGroupId;
            var originalTenantEmail = TenantLocalValue.LogonUserEmail;
            try
            {
                TenantLocalValue.LogonGroupId = tenantId;
                TenantLocalValue.LogonUserEmail = email;
                action();
            }
            finally
            {
                TenantLocalValue.LogonGroupId = originalTenantId;
                TenantLocalValue.LogonUserEmail = originalTenantEmail;
            }
        }

        public static Task RunUnderTenantAsync(string tenantId, string email, Func<Task> action)
        {
            ThrowUtil.ThrowIfNull(tenantId, "tenantId");
            var originalTenantId = TenantLocalValue.LogonGroupId;
            var originalTenantEmail = TenantLocalValue.LogonUserEmail;
            try
            {
                TenantLocalValue.LogonGroupId = tenantId;
                TenantLocalValue.LogonUserEmail = email;
                return action();
            }
            finally
            {
                TenantLocalValue.LogonGroupId = originalTenantId;
                TenantLocalValue.LogonUserEmail = originalTenantEmail;
            }
        }

        public static T RunUnderTenant<T>(string tenantId, string email, Func<T> action)
        {
            ThrowUtil.ThrowIfNull(tenantId, "tenantId");
            var originalTenantId = TenantLocalValue.LogonGroupId;
            var originalTenantEmail = TenantLocalValue.LogonUserEmail;
            try
            {
                TenantLocalValue.LogonGroupId = tenantId;
                TenantLocalValue.LogonUserEmail = email;
                return action();
            }
            finally
            {
                TenantLocalValue.LogonGroupId = originalTenantId;
                TenantLocalValue.LogonUserEmail = originalTenantEmail;
            }
        }

        public static Task<T> RunUnderTenantAsync<T>(string tenantId, string email, Func<Task<T>> action)
        {
            ThrowUtil.ThrowIfNull(tenantId, "tenantId");
            var originalTenantId = TenantLocalValue.LogonGroupId;
            var originalTenantEmail = TenantLocalValue.LogonUserEmail;
            try
            {
                TenantLocalValue.LogonGroupId = tenantId;
                TenantLocalValue.LogonUserEmail = email;
                return action();
            }
            finally
            {
                TenantLocalValue.LogonGroupId = originalTenantId;
                TenantLocalValue.LogonUserEmail = originalTenantEmail;
            }
        }

        public static TR RunUnderTenant<T, TR>(string tenantId, string email, Func<List<T>, TR> action, List<T> args)
        {
            ThrowUtil.ThrowIfNull(tenantId, "tenantId");
            var originalTenantId = TenantLocalValue.LogonGroupId;
            var originalTenantEmail = TenantLocalValue.LogonUserEmail;
            try
            {
                TenantLocalValue.LogonGroupId = tenantId;
                TenantLocalValue.LogonUserEmail = email;
                return action(args);
            }
            finally
            {
                TenantLocalValue.LogonGroupId = originalTenantId;
                TenantLocalValue.LogonUserEmail = originalTenantEmail;
            }
        }

        public static async Task<TR> RunUnderTenantAsync<T, TR>(string tenantId, string email, Func<List<T>, Task<TR>> action, List<T> args)
        {
            ThrowUtil.ThrowIfNull(tenantId, "tenantId");
            var originalTenantId = TenantLocalValue.LogonGroupId;
            var originalTenantEmail = TenantLocalValue.LogonUserEmail;
            try
            {
                TenantLocalValue.LogonGroupId = tenantId;
                TenantLocalValue.LogonUserEmail = email;
                return await action(args);
            }
            finally
            {
                TenantLocalValue.LogonGroupId = originalTenantId;
                TenantLocalValue.LogonUserEmail = originalTenantEmail;
            }
        }
    }
}
