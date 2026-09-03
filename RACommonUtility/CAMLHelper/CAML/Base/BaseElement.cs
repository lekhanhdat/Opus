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
using AvePoint.RA.RACommonUtility.CAMLHelper.Exceptions;
using AvePoint.RA.RACommonUtility.CAMLHelper.General;

namespace AvePoint.RA.RACommonUtility.CAMLHelper.CAML.Base
{
    /// <summary>
    /// Base class that defines a single element for a CAML query.
    /// </summary>
    public abstract class BaseElement
    {
        #region Private Members

        const string CAML = "<FieldRef {0}='{1}' />";

        string _field;
        Types.FieldRefTypes _fieldRefType;

        #endregion Private Members

        #region Public Properties

        /// <summary>
        /// Get or set the name of the SharePoint field to include in the result data.
        /// </summary>
        public string Field
        {
            get { return _field; }
            set { _field = value; }
        }

        /// <summary>
        /// Get or set the field reference type.
        /// </summary>
        public Types.FieldRefTypes FieldRefType
        {
            get { return _fieldRefType; }
            set { _fieldRefType = value; }
        }

        #endregion Public Properties

        #region Initialiation

        /// <summary>
        /// Base constructor for the <see cref="T:SharePointStu.CAMLHelper.CAML.BaseElement"/> object.
        /// </summary>
        /// <param name="fieldValue">The value of the field identifier</param>
        /// <param name="fieldRefType">The <see cref="T:SharePointStu.CAMLHelper.CAML.Types.FieldRefTypes"/> object type of the field to be viewed.</param>
        protected BaseElement(string fieldValue, Types.FieldRefTypes fieldRefType)
        {
            Field = fieldValue;
            FieldRefType = fieldRefType;
        }

        #endregion Initialiation

        #region Public Methods

        /// <summary>
        /// Get the CAML code for the individual element.
        /// </summary>
        /// <returns>CAML string for the individual element.</returns>
        public virtual string GetCAML()
        {
            string caml = string.Empty;

            if (Validate())
            {
                caml = string.Format(CAML, this.FieldRefType.ToString(), this.Field);
            }

            return caml;
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Validate the values of the <see cref="T:SharePointStu.CAMLHelper.CAML.BaseElement"/> object.
        /// </summary>
        /// <returns>bool: if the <see cref="T:SharePointStu.CAMLHelper.CAML.BaseElement"/> object values are valid.</returns>
        protected virtual bool Validate()
        {
            bool result = true;

            if (this.FieldRefType == Types.FieldRefTypes.ID)
            {
                // Ensure the field value is a valid Guid
                Guid id;
                if (!Helper.IsGuid(this.Field, out id))
                    throw new InvalidGuidForFieldException(this.Field);
            }

            if (string.IsNullOrEmpty(this.Field))
            {
                result = false;
            }

            return result;
        }

        #endregion Protected Methods
    }
}
