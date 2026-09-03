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


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// Represents the type of parsing error.
    /// </summary>
    public enum HtmlParseErrorCode
    {
        /// <summary>
        /// A tag was not closed.
        /// </summary>
        TagNotClosed,

        /// <summary>
        /// A tag was not opened.
        /// </summary>
        TagNotOpened,

        /// <summary>
        /// There is a charset mismatch between stream and declared (META) encoding.
        /// </summary>
        CharsetMismatch,

        /// <summary>
        /// An end tag was not required.
        /// </summary>
        EndTagNotRequired,

        /// <summary>
        /// An end tag is invalid at this position.
        /// </summary>
        EndTagInvalidHere
    }
}