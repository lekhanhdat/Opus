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

#region

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// Represents an HTML attribute.
    /// </summary>
    [DebuggerDisplay("Name: {OriginalName}, Value: {Value}")]
    public class HtmlAttribute : IComparable
    {
        #region Fields

        private int _line;
        internal int _lineposition;
        internal string _name;
        internal int _namelength;
        internal int _namestartindex;
        internal HtmlDocument _ownerdocument; // attribute can exists without a node
        internal HtmlNode _ownernode;
        private AttributeValueQuote _quoteType = AttributeValueQuote.DoubleQuote;
        internal int _streamposition;
        internal string _value;
        internal int _valuelength;
        internal int _valuestartindex;

        #endregion

        #region Constructors

        internal HtmlAttribute(HtmlDocument ownerdocument)
        {
            _ownerdocument = ownerdocument;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the line number of this attribute in the document.
        /// </summary>
        public int Line
        {
            get { return _line; }
            internal set { _line = value; }
        }

        /// <summary>
        /// Gets the column number of this attribute in the document.
        /// </summary>
        public int LinePosition
        {
            get { return _lineposition; }
        }

        /// <summary>
        /// Gets the qualified name of the attribute.
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = _ownerdocument._text.Substring(_namestartindex, _namelength);
                }
                return _name.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _name = value;
                if (_ownernode != null)
                {
                    _ownernode._innerchanged = true;
                    _ownernode._outerchanged = true;
                }
            }
        }

        /// <summary>
        /// Name of attribute with original case
        /// </summary>
        public string OriginalName
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the HTML document to which this attribute belongs.
        /// </summary>
        public HtmlDocument OwnerDocument
        {
            get { return _ownerdocument; }
        }

        /// <summary>
        /// Gets the HTML node to which this attribute belongs.
        /// </summary>
        public HtmlNode OwnerNode
        {
            get { return _ownernode; }
        }

        /// <summary>
        /// Specifies what type of quote the data should be wrapped in
        /// </summary>
        public AttributeValueQuote QuoteType
        {
            get { return _quoteType; }
            set { _quoteType = value; }
        }

        /// <summary>
        /// Gets the stream position of this attribute in the document, relative to the start of the document.
        /// </summary>
        public int StreamPosition
        {
            get { return _streamposition; }
        }

        /// <summary>
        /// Gets or sets the value of the attribute.
        /// </summary>
        public string Value
        {
            get
            {
                if (_value == null)
                {
                    _value = _ownerdocument._text.Substring(_valuestartindex, _valuelength);
                }
                return _value;
            }
            set
            {
                _value = value;
                if (_ownernode != null)
                {
                    _ownernode._innerchanged = true;
                    _ownernode._outerchanged = true;
                }
            }
        }

        internal string XmlName
        {
            get { return HtmlDocument.GetXmlName(Name); }
        }

        internal string XmlValue
        {
            get { return Value; }
        }

        /// <summary>
        /// Gets a valid XPath string that points to this Attribute
        /// </summary>
        public string XPath
        {
            get
            {
                string basePath = (OwnerNode == null) ? "/" : OwnerNode.XPath + "/";
                return basePath + GetRelativeXpath();
            }
        }

        #endregion

        #region IComparable Members

        /// <summary>
        /// Compares the current instance with another attribute. Comparison is based on attributes' name.
        /// </summary>
        /// <param name="obj">An attribute to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the names comparison.</returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong names are html node name.")]
        public int CompareTo(object obj)
        {
            HtmlAttribute att = obj as HtmlAttribute;
            if (att == null)
            {
                throw new ArgumentException("obj");
            }
            return string.Compare(Name, att.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a duplicate of this attribute.
        /// </summary>
        /// <returns>The cloned attribute.</returns>
        public HtmlAttribute Clone()
        {
            HtmlAttribute att = new HtmlAttribute(_ownerdocument);
            att.Name = Name;
            att.Value = Value;
            return att;
        }

        /// <summary>
        /// Removes this attribute from it's parents collection
        /// </summary>
        public void Remove()
        {
            _ownernode.Attributes.Remove(this);
        }

        #endregion

        #region Private Methods

        private string GetRelativeXpath()
        {
            if (OwnerNode == null)
                return Name;

            int i = 1;
            foreach (HtmlAttribute node in OwnerNode.Attributes)
            {
                if (node.Name != Name) continue;

                if (node == this)
                    break;

                i++;
            }
            return "@" + Name + "[" + i + "]";
        }

        #endregion
    }

    /// <summary>
    /// An Enum representing different types of Quotes used for surrounding attribute values
    /// </summary>
    public enum AttributeValueQuote
    {
        /// <summary>
        /// A single quote mark '
        /// </summary>
        SingleQuote,
        /// <summary>
        /// A double quote mark "
        /// </summary>
        DoubleQuote
    }
}