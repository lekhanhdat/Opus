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
    /// Represents a parsing error found during document parsing.
    /// </summary>
    public class HtmlParseError
    {
        #region Fields

        private HtmlParseErrorCode _code;
        private int _line;
        private int _linePosition;
        private string _reason;
        private string _sourceText;
        private int _streamPosition;

        #endregion

        #region Constructors

        internal HtmlParseError(
            HtmlParseErrorCode code,
            int line,
            int linePosition,
            int streamPosition,
            string sourceText,
            string reason)
        {
            _code = code;
            _line = line;
            _linePosition = linePosition;
            _streamPosition = streamPosition;
            _sourceText = sourceText;
            _reason = reason;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of error.
        /// </summary>
        public HtmlParseErrorCode Code
        {
            get { return _code; }
        }

        /// <summary>
        /// Gets the line number of this error in the document.
        /// </summary>
        public int Line
        {
            get { return _line; }
        }

        /// <summary>
        /// Gets the column number of this error in the document.
        /// </summary>
        public int LinePosition
        {
            get { return _linePosition; }
        }

        /// <summary>
        /// Gets a description for the error.
        /// </summary>
        public string Reason
        {
            get { return _reason; }
        }

        /// <summary>
        /// Gets the the full text of the line containing the error.
        /// </summary>
        public string SourceText
        {
            get { return _sourceText; }
        }

        /// <summary>
        /// Gets the absolute stream position of this error in the document, relative to the start of the document.
        /// </summary>
        public int StreamPosition
        {
            get { return _streamPosition; }
        }

        #endregion
    }
}