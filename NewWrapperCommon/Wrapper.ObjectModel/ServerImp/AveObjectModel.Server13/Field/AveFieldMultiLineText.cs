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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System;
using Microsoft.SharePoint.Utilities;

namespace AvePoint.ObjectModel.Server13
{
    class AveFieldMultiLineText : AveField, IAveFieldMultiLineText
    {
        private SPFieldMultiLineText mFieldMultiLineText;
        
        public AveFieldMultiLineText(AveFieldCollection fieldColl, SPFieldMultiLineText field)
            : base(fieldColl, field)
        {
            mFieldMultiLineText = field;
        }

        public AveFieldMultiLineText(SPFieldMultiLineText fieldMultiLineText)
            : base(fieldMultiLineText)
        {
            mFieldMultiLineText = fieldMultiLineText;
        }

        public string XPath
        {
            get
            {
                return mFieldMultiLineText.XPath;
            }
            set
            {
                mFieldMultiLineText.XPath = value;
            }

        }
        public override Type FieldValueType
        {
            get
            {
                return typeof(string);
            }
        }

        public override string GetFieldValueAsText(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            if (this.RichText)
            {
                return SPHttpUtility.ConvertSimpleHtmlToText(value.ToString(), -1);
            }
            return value.ToString();
        }

        #region IAveFieldMultiLineText Members

        public bool AllowHyperlink
        {
            get
            {
                return mFieldMultiLineText.AllowHyperlink;
            }
            set
            {
                mFieldMultiLineText.AllowHyperlink = value;
            }
        }

        public bool AppendOnly
        {
            get
            {
                return mFieldMultiLineText.AppendOnly;
            }
            set
            {
                mFieldMultiLineText.AppendOnly = value;
            }
        }

        public int DifferencingLimit
        {
            get
            {
                return mFieldMultiLineText.DifferencingLimit;
            }
            set
            {
                mFieldMultiLineText.DifferencingLimit = value;
            }
        }

        public bool IsolateStyles
        {
            get
            {
                return mFieldMultiLineText.IsolateStyles;
            }
            set
            {
                mFieldMultiLineText.IsolateStyles = value;
            }
        }

        public int NumberOfLines
        {
            get
            {
                return mFieldMultiLineText.NumberOfLines;
            }
            set
            {
                mFieldMultiLineText.NumberOfLines = value;
            }
        }

        public bool RestrictedMode
        {
            get
            {
                return mFieldMultiLineText.RestrictedMode;
            }
            set
            {
                mFieldMultiLineText.RestrictedMode = value;
            }
        }

        public bool RichText
        {
            get
            {
                return mFieldMultiLineText.RichText;
            }
            set
            {
                mFieldMultiLineText.RichText = value;
            }
        }

        public AveRichTextMode RichTextMode
        {
            get
            {
                return (AveRichTextMode)mFieldMultiLineText.RichTextMode;
            }
            set
            {
                mFieldMultiLineText.RichTextMode = (SPRichTextMode)value;
            }
        }

        public bool UnlimitedLengthInDocumentLibrary
        {
            get
            {
                return mFieldMultiLineText.UnlimitedLengthInDocumentLibrary;
            }
            set
            {
                mFieldMultiLineText.UnlimitedLengthInDocumentLibrary = value;
            }
        }

        public bool WikiLinking
        {
            get { return mFieldMultiLineText.WikiLinking; }
        }

        #endregion
    }
}
