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
using System;

namespace AvePoint.RA.SharePoint.Common.CAMLHelper.Exceptions
{
    /// <summary>
    /// General exception class.
    /// </summary>
    [RACodeReview("Allen Yin")]
    [Serializable]
    public class BaseException : ApplicationException
    {
        #region Members

        /// <summary>
        /// Defines the details of the exception
        /// </summary>
        protected string _msg;

        #endregion Members

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SharePointStu.CAMLHelper.BaseException"/> class.
        /// </summary>
        public BaseException()
        {
            _msg = "The SharePointStu Library generated an unexpected error{0}";
        }

        /// <summary>
        /// Creates a new BaseException with the default message and the supplied inner exception.
        /// </summary>
        /// <param name="msg">The exception message text.</param>
        /// <param name="innerException">The inner exception object.</param>
        /// <remarks>Note that msg must be localized before the call and only
        /// refers to the InnerException message.  The Message for the derived class
        /// must still be manually set and localized as normal.
        /// </remarks>
        public BaseException(string msg, Exception innerException)
            : base(msg, innerException)
        {
            _msg = msg;
        }

        /// <summary>
        /// Creates a new instance of the Exception with the given exception details
        /// </summary>
        /// <param name="details">Provides information concerning the exception</param>
        public BaseException(string details)
        {
            string s = "The SharePointStu Library generated an unexpected error{0}";
            _msg = string.Format(s, details);
        }

        #endregion Constructors and Destructors

        #region Public Properties

        /// <summary>
        /// Provides a description of the exception
        /// </summary>
        public override string Message
        {
            get { return _msg; }
        }

        #endregion Public Properties
    }
}
