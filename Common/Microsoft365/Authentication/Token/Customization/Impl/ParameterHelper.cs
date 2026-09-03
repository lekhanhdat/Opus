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
    using System;
    using System.Collections.Generic;

    class ParameterHelper
    {
        private Dictionary<string, string> objs;

        public ParameterHelper(string parameters)
        {
            if (!string.IsNullOrEmpty(parameters))
            {
                var items = parameters.Split('=', ';');

                if ((items.Length & 1) != 0) //==> items.Length %2 != 0
                {
                    throw new ArgumentOutOfRangeException("parameters", parameters);
                }

                objs = new Dictionary<string, string>(items.Length / 2, StringComparer.OrdinalIgnoreCase);

                for (int index = 0; index < items.Length; index++)
                {
                    objs[items[index]] = items[++index];
                }
            }
        }

        public bool TryGetValue(string key, out string value)
        {
            value = null;

            if (objs != null)
            {
                return objs.TryGetValue(key, out value);
            }

            return false;
        }

        public string GetValue(string key)
        {
            string value;

            TryGetValue(key, out value);

            return value;
        }
    }
}