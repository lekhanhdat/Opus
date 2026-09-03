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




namespace System
{
    #region using directives
    using AvePoint.GCommon.Contract.CodeReview;

    #endregion

    ///<Summary>
    /// Extended the System.Int32 class
    ///</Summary>
    #region CodeReview

    [AveCodeReview(
    "2012/1/9",
    "yhzhang@avepoint.com",
    "yhzhang@avepoint.com",
    new string[] { },
    null,
    true)]
    #endregion

    public static class Int32Extension
    {
        /// <summary>
        /// Convert a int value to a specific enum value
        /// </summary>
        /// <typeparam name="T">the enum type</typeparam>
        /// <param name="value">the underline int value</param>
        /// <returns>the result of enum value</returns>
        public static T ToEnum<T>(this Int32 value)
        {
            return Enum.GetName(typeof(T), value).ToEnum<T>();
        }
    }
}