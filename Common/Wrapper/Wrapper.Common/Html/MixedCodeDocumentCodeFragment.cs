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
    /// Represents a fragment of code in a mixed code document.
    /// </summary>
    public class MixedCodeDocumentCodeFragment : MixedCodeDocumentFragment
    {
        #region Fields

        private string _code;

        #endregion

        #region Constructors

        internal MixedCodeDocumentCodeFragment(MixedCodeDocument doc)
            :
                base(doc, MixedCodeDocumentFragmentType.Code)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the fragment code text.
        /// </summary>
        public string Code
        {
            get
            {
                if (_code == null)
                {
                    _code = FragmentText.Substring(Doc.TokenCodeStart.Length,
                                                   FragmentText.Length - Doc.TokenCodeEnd.Length -
                                                   Doc.TokenCodeStart.Length - 1).Trim();
                    if (_code.StartsWith("="))
                    {
                        _code = Doc.TokenResponseWrite + _code.Substring(1, _code.Length - 1);
                    }
                }
                return _code;
            }
            set { _code = value; }
        }

        #endregion
    }
}