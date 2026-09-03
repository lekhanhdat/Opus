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
using AvePoint.RA.Contract.CodeView;

namespace AvePoint.RA.RACommonUtility.CAMLHelper.Exceptions
{
    /// <summary>
    /// Invalid Guid for field exception class.
    /// </summary>
    [Serializable]
    [RACodeReview("Allen Yin")]
    public class InvalidGuidForFieldException : BaseException
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="T:SharePointStu.CAMLHelper.InvalidGuidForFieldException"/> class.
        /// </summary>
        /// <param name="field">The field that should be a Guid value.</param>
        public InvalidGuidForFieldException(string field)
        {
            string s = "The SharePointStu Library generated an unexpected error{0}";
            _msg = string.Format(s, field);
        }

        #endregion Constructors and Destructors
    }
}
