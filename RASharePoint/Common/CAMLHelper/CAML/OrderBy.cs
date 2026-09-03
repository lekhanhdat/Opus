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
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Common.CAMLHelper.CAML
{

    /// <summary>
    /// Class that defines a single OrderBy element of a CAML query.
    /// </summary>
    public class OrderBy : BaseElement
    {
        #region Private Members

        const string CAML = "<FieldRef {0}='{1}' Ascending='{2}'/>";

        bool _isAscending;

        #endregion Private Members

        #region Public Properties

        /// <summary>
        /// Get or set if the sort order of this OrderBy definition should in ascending or descending order.
        /// </summary>
        public bool IsAscending
        {
            get { return _isAscending; }
            set { _isAscending = value; }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Initialise the <see cref="T:SharePointStu.CAMLHelper.CAML.OrderBy"/> object.
        /// </summary>
        /// <param name="field">The value of the field identifier to set the order by rule with.</param>
        /// <param name="isAscending">Is the sort order ascending or descending.</param>
        /// <param name="fieldRefType">The <see cref="T:SharePointStu.CAMLHelper.CAML.Types.FieldRefTypes"/> object type of the field being ordered.</param>
        public OrderBy(string field, bool isAscending, Types.FieldRefTypes fieldRefType)
            : base(field, fieldRefType)
        {
            this.IsAscending = isAscending;
        }

        /// <summary>
        /// Initialise the <see cref="T:SharePointStu.CAMLHelper.CAML.OrderBy"/> object using a field reference type of name.
        /// </summary>
        /// <param name="field">The value of the field identifier to set the order by rule with.</param>
        /// <param name="isAscending">Is the sort order ascending or descending.</param>
        public OrderBy(string field, bool isAscending)
            : base(field, Types.FieldRefTypes.Name)
        {
            this.IsAscending = isAscending;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Get the CAML code for the individual order by node.
        /// </summary>
        /// <returns>CAML string for the individual order by node.</returns>
        public override string GetCAML()
        {
            string caml = string.Empty;

            if (Validate())
            {
                caml = string.Format(CAML, this.FieldRefType.ToString(), this.Field, this.IsAscending ? bool.TrueString : bool.FalseString);
            }

            return caml;
        }

        #endregion Public Methods
    }
}
