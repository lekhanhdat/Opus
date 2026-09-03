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

using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace AvePoint.RA.RACommonUtility.CAMLHelper.CAML
{
    /// <summary>
    /// CAML manager class for generating CAML queries.
    /// </summary>
    public sealed class CAMLManager
    {
        #region Private Members
        private const string ELEMENT_VIEW = "View";
        private const string ELEMENT_QUERY = "Query";
        private const string ELEMENT_WHERE = "Where";
        private const string ELEMENT_ORDERBY = "OrderBy";
        private const string ELEMENT_ROWLIMIT = "RowLimit";

        private Types.ScopeTypes mScopeType;
        private QueryGroup mQueryGroup;
        private List<OrderBy> mOrderBy;
        private List<ViewField> mViewFields;
        private int mRowLimit;
        #endregion Private Members

        #region Public Properties
        
        public Types.ScopeTypes ScopeType
        {
            get { return mScopeType; }
            set { mScopeType = value; }
        }
        public QueryGroup QueryGroup
        {
            get { return mQueryGroup; }
            set { mQueryGroup = value; }
        }

        /// <summary>
        /// Get or set the generic list of <see cref="SharePointStu.CAMLHelper.CAML.OrderBy"/> objects.
        /// </summary>
        public List<OrderBy> OrderBy
        {
            get { return mOrderBy; }
            set { mOrderBy = value; }
        }

        /// <summary>
        /// Get or set the generic list of <see cref="SharePointStu.CAMLHelper.CAML.ViewField"/> objects.
        /// </summary>
        public List<ViewField> ViewFields
        {
            get { return mViewFields; }
            set { mViewFields = value; }
        }

        /// <summary>
        /// Get or set the row limit value for CAML query results.
        /// </summary>
        public int RowLimit
        {
            get { return mRowLimit; }
            set { mRowLimit = value; }
        }
        #endregion Public Properties

        #region Constructors
        /// <summary>
        /// Initialise the <see cref="T:SharePointStu.CAMLHelper.CAML.CAMLManager"/> object.
        /// </summary>
        public CAMLManager(Types.ScopeTypes scope = Types.ScopeTypes.Default)
        {
            mScopeType = scope;
            mQueryGroup = new QueryGroup(Types.JoinTypes.And, new List<QueryGroup>());
            mOrderBy = new List<OrderBy>();
            mViewFields = new List<ViewField>();
        }
        #endregion Constructors

        #region Public Methods
        public void AddViewFields(string field)
        {
            mViewFields.Add(new ViewField(field));
        }

        /// <summary>
        /// Get the XML CAML string for the current collection of <see cref="T:SharePointStu.CAMLHelper.CAML.QueryGroup"/>
        /// and <see cref="T:SharePointStu.CAMLHelper.CAML.OrderBy"/> objects, plus the ViewField and RowLimit elements.
        /// </summary>
        /// <remarks>The ViewField and RowLimit elements will not be returned if no valid values exist.</remarks>
        /// <returns>Full XML CAML query string.</returns>
        public string GetFullCAML()
        {
            string query = GetWhereCAML();
            string viewFields = GetViewFields();
            string rowLimit = GetRowLimit();

            return string.Format("<{0} Scope=\"{1}\">{2}{3}{4}</{0}>", ELEMENT_VIEW, mScopeType.ToString(), viewFields, query, rowLimit);
        }

        /// <summary>
        /// Get the XML CAML string for the current collection of <see cref="T:SharePointStu.CAMLHelper.CAML.QueryGroup"/>
        /// and <see cref="T:SharePointStu.CAMLHelper.CAML.OrderBy"/> objects, plus the ViewField and RowLimit elements.
        /// </summary>
        /// <remarks>The ViewField and RowLimit elements will not be returned if no valid values exist.</remarks>
        /// <returns>Full XML CAML query string.</returns>
        public string GetFullCAML(bool withRowLimit = true)
        {
            string query = GetWhereCAML();
            string viewFields = GetViewFields();
            string rowLimit = GetRowLimit();
            if (withRowLimit)
            {
                return string.Format("<{0} Scope=\"{1}\">{2}{3}{4}</{0}>", ELEMENT_VIEW, mScopeType.ToString(), viewFields, query, rowLimit);
            }
            else
            {
                return string.Format("<{0} Scope=\"{1}\">{2}{3}</{0}>", ELEMENT_VIEW, mScopeType.ToString(), viewFields, query);//Debug without rowlimit...
            }
        }

        /// <summary>
        /// Get a <see cref="T:System.Xml.XmlDocument"/> object containing the CAML query for the current
        /// collection of <see cref="T:SharePointStu.CAMLHelper.CAML.QueryGroup"/> and <see cref="T:SharePointStu.CAMLHelper.CAML.OrderBy"/>
        /// objects, plus the ViewField and RowLimit elements.
        /// </summary>
        /// <remarks>The ViewField and RowLimit elements will not be returned if no valid values exist.</remarks>
        /// <returns>A <see cref="T:System.Xml.XmlDocument"/> object containing the full CAML query.</returns>
        public XmlDocument GetFullCAMLAsXML()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(GetFullCAML());

            return doc;
        }
        #endregion Public Methods

        #region Private Methods
        private string GetViewFields()
        {
            StringBuilder sb = new StringBuilder();

            if (ViewFields.Count > 0)
            {
                foreach (ViewField view in ViewFields)
                {
                    sb.Append(view.GetCAML());
                }
            }

            if (sb.Length > 0)
            {
                return string.Format("<ViewFields>{0}</ViewFields>", sb.ToString());
            }

            return string.Empty;
        }

        private string GetWhereCAML()
        {
            XmlDocument doc = new XmlDocument();

            XmlElement queryEl = doc.CreateElement(ELEMENT_QUERY);
            doc.AppendChild(queryEl);

            string orderByXml = GetOrderBy();
            if (orderByXml.Length > 0)
            {
                queryEl.InnerXml = orderByXml;
            }

            XmlElement whereEl = doc.CreateElement(ELEMENT_WHERE);
            whereEl.InnerXml = mQueryGroup.GetUnionCAML();

            if (whereEl.InnerXml.Length > 0)
            {
                queryEl.AppendChild(whereEl);
                return doc.InnerXml;
            }

            return string.Empty;
        }

        private string GetRowLimit()
        {
            string result = string.Empty;

            if (RowLimit > 0)
            {
                result = string.Format("<{0}>", ELEMENT_ROWLIMIT);
                result += RowLimit.ToString();
                result += string.Format("</{0}>", ELEMENT_ROWLIMIT);
            }

            return result;
        }

        private string GetOrderBy()
        {
            StringBuilder sb = new StringBuilder();

            if (OrderBy.Count > 0)
            {
                sb.Append(string.Format("<{0}>", ELEMENT_ORDERBY));
                foreach (OrderBy order in OrderBy)
                {
                    sb.Append(order.GetCAML());
                }
                sb.Append(string.Format("</{0}>", ELEMENT_ORDERBY));
            }

            if (sb.Length > 0)
            {
                return sb.ToString();
            }
            return string.Empty;
        }

        #endregion Private Methods
    }
}
