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

namespace AvePoint.RA.SharePoint.Common.CAMLHelper.CAML
{
    /// <summary>
    /// Class that defines a <see cref="T:SharePointStu.CAMLHelper.CAML.Query"/> object and where in the overall CAML query
    /// it should be created, based on the <see cref="T:SharePointStu.CAMLHelper.CAML.QueryGroup.MergeTypes"/> enumerator and existing
    /// query nodes.
    /// </summary>
    public class QueryCondition
    {
        #region Private Members

        readonly Query _query;
        Types.JoinTypes _joinType;

        #endregion Private Members

        #region Public Properties

        /// <summary>
        /// Get the <see cref="T:SharePointStu.CAMLHelper.CAML.Query"/> object for this grouping.
        /// </summary>
        public Query Query
        {
            get { return _query; }
        }

        /// <summary>
        /// Get or set the type of logical join to perform for this query group.
        /// </summary>
        public Types.JoinTypes JoinType
        {
            get { return _joinType; }
            set { _joinType = value; }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Initialise the <see cref="T:SharePointStu.CAMLHelper.CAML.QueryGroup"/> object.
        /// </summary>
        public QueryCondition()
        {
            _query = new Query("", Types.FieldRefTypes.Name, Types.FieldTypes.Text, Types.QueryTypes.IsNotNull, "", true);
        }

        public QueryCondition(Types.JoinTypes joinType, Types.FieldRefTypes fieldRefType, string field, Types.FieldTypes fieldType, Types.QueryTypes queryType, string value, string value2, bool isIncludeTimeValue = true)
            : this(joinType)
        {
            _query = new Query(field, fieldRefType, fieldType, queryType, value, value2, isIncludeTimeValue);
        }

        /// <summary>
        /// Initialise the <see cref="T:SharePointStu.CAMLHelper.CAML.QueryGroup"/> object.
        /// </summary>
        /// <param name="joinType">The type of logical join to perform with the query.</param>
        /// <param name="mergeType">The merge type of the query.</param>
        public QueryCondition(Types.JoinTypes joinType)
        {
            this.JoinType = joinType;
        }

        /// <summary>
        /// Initialise the <see cref="T:SharePointStu.CAMLHelper.CAML.QueryGroup"/> object.
        /// </summary>
        /// <param name="joinType">The type of logical join to perform with the query.</param>
        /// <param name="field">The name or id of the field to query against.</param>
        /// <param name="fieldType">The <see cref="T:SharePointStu.CAMLHelper.CAML.Types.FieldTypes"/> object of the field.</param>
        /// <param name="queryType">The <see cref="T:SharePointStu.CAMLHelper.CAML.Types.FieldRefTypes"/> object that defines the type of query to perform.</param>
        /// <param name="value">The value of the field being queried.</param>
        public QueryCondition(Types.JoinTypes joinType, string field, Types.FieldTypes fieldType, Types.QueryTypes queryType, string value)
            : this(joinType, Types.FieldRefTypes.Name, field, fieldType, queryType, value)
        { }

        /// <summary>
        /// Initialise the <see cref="T:SharePointStu.CAMLHelper.CAML.QueryGroup"/> object.
        /// </summary>
        /// <param name="joinType">The type of logical join to perform with the query.</param>
        /// <param name="fieldRefType">The <see cref="T:SharePointStu.CAMLHelper.CAML.Types.FieldRefTypes"/> object type of the field being queried.</param>
        /// <param name="field">The name or id of the field to query against.</param>
        /// <param name="fieldType">The <see cref="T:SharePointStu.CAMLHelper.CAML.Types.FieldTypes"/> object of the field.</param>
        /// <param name="queryType">The <see cref="T:SharePointStu.CAMLHelper.CAML.Types.FieldRefTypes"/> object that defines the type of query to perform.</param>
        /// <param name="value">The value of the field being queried.</param>
        public QueryCondition(Types.JoinTypes joinType, Types.FieldRefTypes fieldRefType, string field, Types.FieldTypes fieldType, Types.QueryTypes queryType, string value, bool isIncludeTimeValue = true)
            : this(joinType)
        {
            _query = new Query(field, fieldRefType, fieldType, queryType, value, isIncludeTimeValue);
        }

        public QueryCondition(Types.JoinTypes joinType, Types.FieldRefTypes fieldRefType, string field, Types.FieldTypes fieldType, Types.QueryTypes queryType, int[] values)
            : this(joinType)
        {
            _query = new Query(field, fieldRefType, fieldType, queryType, values);
        }
        #endregion Constructors
    }
}
