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

using AvePoint.RA.Contract.CodeView;

namespace AvePoint.RA.SharePoint.Common.CAMLHelper.Exceptions
{
    /// <summary>
    /// Invalid field value exception class.
    /// </summary>
    [RACodeReview("Allen Yin")]
    public class InvalidFieldValueException : BaseException
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="T:SharePointStu.CAMLHelper.InvalidFieldValueException"/> class.
        /// </summary>
        /// <param name="field">The object we were trying to update.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="value">The id that we were trying to load as a Guid.</param>
        public InvalidFieldValueException(string field, CAML.Types.FieldTypes fieldType, string value)
        {
            string s = "The SharePointStu Library generated an unexpected error{0}";
            _msg = string.Format(s, field);
        }

        #endregion Constructors and Destructors
    }
}
