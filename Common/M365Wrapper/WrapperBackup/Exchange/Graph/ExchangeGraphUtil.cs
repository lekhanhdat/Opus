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
namespace ExchangeBackupUtility.Graph;

using Microsoft.Graph.Models;
using System;
public static class ExchangeGraphUtil
{
    public static String ToEwsId(this String restId)
    {
        return restId?.Replace('_', '+').Replace('-', '/');
    }
    public static String ToRestId(this String ewsId)
    {
        return ewsId?.Replace('/', '-').Replace('+', '_');
    }

    internal static ExportItemResult ToEwsId(this ExportItemResult result)
    {
        result.Id = result.Id.ToEwsId();
        return result;
    }
    public static string ToFormatString (EmailAddress emailAddress)
    {
        if (string.IsNullOrEmpty(emailAddress.Address)) return emailAddress.Name;
        if (string.IsNullOrEmpty(emailAddress.Name)) return emailAddress.Address;
        return string.Format("{0} <{1}>", emailAddress.Name, emailAddress.Address);
    }
}
