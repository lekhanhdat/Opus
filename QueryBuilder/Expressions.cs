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
namespace SqlKata
{
    public static class Expressions
    {
        /// <summary>
        /// Instruct the compiler to resolve the value from the predefined variables
        /// In the current query or parents queries.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Variable Variable(string name)
        {
            return new Variable(name);
        }

        /// <summary>
        /// Instruct the compiler to treat this as a literal.
        /// WARNING: don't pass user data directly to this method.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="replaceQuotes">if true it will esacpe single quotes</param>
        /// <returns></returns>
        public static UnsafeLiteral UnsafeLiteral(string value, bool replaceQuotes = true)
        {
            return new UnsafeLiteral(value, replaceQuotes);
        }
    }
}