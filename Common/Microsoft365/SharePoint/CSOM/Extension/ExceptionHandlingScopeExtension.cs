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
namespace Microsoft365.SharePoint.CSOM.Extension
{
    using Microsoft.SharePoint.Client;
    using System.Text;
    public static class ExceptionHandlingScopeExtension
    {
        public static string ExtractException(this ExceptionHandlingScope scope)
        {
            if (scope != null && scope.HasException)
            {
                var builder = new StringBuilder();

                builder.Append("Error:");
                builder.Append(scope.ErrorMessage);
                builder.Append(", ErrorCode:");
                builder.Append(scope.ServerErrorCode);
                if (scope.ServerErrorDetails != null)
                {
                    builder.Append(", ErrorDetails:");
                    builder.Append(scope.ServerErrorDetails);
                }
                if (!string.IsNullOrEmpty(scope.ServerErrorTypeName))
                {
                    builder.Append(", ErrorTypeName:");
                    builder.Append(scope.ServerErrorTypeName);
                }
                if (!string.IsNullOrEmpty(scope.ServerErrorValue))
                {
                    builder.Append(", ErrorValue:");
                    builder.Append(scope.ServerErrorValue);
                }
                if (!string.IsNullOrEmpty(scope.ServerStackTrace))
                {
                    builder.Append(", StackTrace:");
                    builder.Append(scope.ServerStackTrace);
                }

                return builder.ToString();
            }

            return null;
        }
    }
}