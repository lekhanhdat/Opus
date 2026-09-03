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

namespace Microsoft365.Graph.Extensions;

public static class GraphErrorExtensions
{
    public static String ToFormatString(this ODataError error)
    {
        var errorBuilder = new System.Text.StringBuilder(error?.GetType().FullName ?? String.Empty);
        errorBuilder.AppendLine($"ResponseStatusCode:{error?.ResponseStatusCode}");
        if (error?.ResponseHeaders is not null)
        {
            errorBuilder.AppendLine($"Response Headers:");
            foreach (var header in error.ResponseHeaders)
            {
                errorBuilder.AppendLine($"{header.Key}:{header.Value}");
            }
        }
        if (error?.Error is not null)
        {
            errorBuilder.AppendLine($"MainError Code:{error.Error.Code}");
            errorBuilder.AppendLine($"MainError Message:{error.Error.Message}");
        }
        return errorBuilder.ToString();
    }
}
