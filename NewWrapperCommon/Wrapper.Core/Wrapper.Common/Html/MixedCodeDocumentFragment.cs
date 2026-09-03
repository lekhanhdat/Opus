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
    /// Represents a base class for fragments in a mixed code document.
    /// </summary>
    public abstract class MixedCodeDocumentFragment
    {
        #region Fields

        internal MixedCodeDocument Doc;
        private string _fragmentText;
        internal int Index;
        internal int Length;
        private int _line;
        internal int _lineposition;
        internal MixedCodeDocumentFragmentType _type;

        #endregion

        #region Constructors

        internal MixedCodeDocumentFragment(MixedCodeDocument doc, MixedCodeDocumentFragmentType type)
        {
            Doc = doc;
            _type = type;
            switch (type)
            {
                case MixedCodeDocumentFragmentType.Text:
                    Doc._textfragments.Append(this);
                    break;

                case MixedCodeDocumentFragmentType.Code:
                    Doc._codefragments.Append(this);
                    break;
            }
            Doc._fragments.Append(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the fragement text.
        /// </summary>
        public string FragmentText
        {
            get
            {
                if (_fragmentText == null)
                {
                    _fragmentText = Doc._text.Substring(Index, Length);
                }
                return _fragmentText;
            }
            internal set { _fragmentText = value; }
        }

        /// <summary>
        /// Gets the type of fragment.
        /// </summary>
        public MixedCodeDocumentFragmentType FragmentType
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the line number of the fragment.
        /// </summary>
        public int Line
        {
            get { return _line; }
            internal set { _line = value; }
        }

        /// <summary>
        /// Gets the line position (column) of the fragment.
        /// </summary>
        public int LinePosition
        {
            get { return _lineposition; }
        }

        /// <summary>
        /// Gets the fragment position in the document's stream.
        /// </summary>
        public int StreamPosition
        {
            get { return Index; }
        }

        #endregion
    }
}