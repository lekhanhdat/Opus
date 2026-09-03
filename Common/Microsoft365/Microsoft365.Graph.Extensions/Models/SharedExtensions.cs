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

namespace Microsoft365.Graph.Extensions
{
    /// <summary>
    /// Provides extension methods for Graph Beta models.
    /// </summary>
    public static partial class ModelExtensions
    {


        public static string? Email(this Identity identity)
        {
            return identity.GetFromAdditionalData<string>("email");
        }

        internal static T? GetFromAdditionalData<T>(this IAdditionalDataHolder identity, string key)
        {
            if (identity.TryGetAdditionalData(key, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
                else if (value != null && typeof(T).IsAssignableFrom(value.GetType()))
                {
                    return (T)value;
                }
            }
            return default;
        }

        internal static bool TryGetAdditionalData(this IAdditionalDataHolder identity, string key, [MaybeNullWhen(false)] out object value)
        {
            value = default;
            return identity.AdditionalData?.TryGetValue(key, out value) ?? false;
        }

        internal static void SetAdditionalData(this Identity identity, string key, string value)
        {
            identity.AdditionalData ??= new Dictionary<string, object>();
            identity.AdditionalData[key] = value;
        }
    }
}