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

namespace ExchangeUtility.Graph
{
    public static class ExchangeExceptionUtil
    {
        public static bool IsNoExchangeAdminRoleException(Exception ex)
        {
            if (ex.Message.Contains("The role assigned to application"))
            {
                return true;
            }

            return false;
        }

        public static string GetAppIdFromErrorMessage(Exception ex)
        {
            string[] words = ex.Message.Split(' ');
            foreach (string word in words)
            {
                if (Guid.TryParse(word, out Guid guid))
                {
                    return guid.ToString();
                }
            }
            return null;
        }
        public static bool IsImmutablePolicyEnabledException(this Exception ex)
        {
            if (ex.Message.Contains("This operation is not permitted as the blob is immutable") || (ex is Azure.RequestFailedException ae && ae.ErrorCode.Equals("BlobImmutableDueToPolicy"))) return true;
            if (ex.InnerException is not null) return ex.InnerException.IsImmutablePolicyEnabledException();
            return false;
        }
    }
}