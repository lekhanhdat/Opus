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
using AvePoint.RA.SharePoint.Common.CAMLHelper.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.SharePoint.Common.CAMLHelper.CAML
{
    /// <summary>
    /// Class that defines a single field query element of a CAML query.
    /// </summary>
    public sealed class Query : BaseElement
    {
        #region Private Members

        /// <summary>
        /// {0} = Type of query
        /// {1} = Types.FieldRefType
        /// {2} = Name or ID of field
        /// {3} = Type of field
        /// {4} = DateTime Filed is include time value
        /// {5} = Value of field
        /// </summary>
        const string CAML = "<{0}><FieldRef {1}='{2}'/><Value Type='{3}'{4}>{5}</Value></{0}>";

        const string IDCAML = "<{0}><FieldRef {1}='{2}'/>{3}</{0}>";

        const string CAML_INCLUDETIME = " IncludeTimeValue='TRUE' StorageTZ='TRUE' ";

        const string CAML_MMS = "<In><FieldRef Name='{0}' LookupId='TRUE'/><Values>{1}</Values></In>";

        const string CAML_MMS_VALUE = "<Value Type='Integer'>{0}</Value>";

        /// <summary>
        /// {0} = Type of query
        /// {1} = Types.FieldRefType
        /// {2} = Name or ID of field
        /// </summary>
        const string CAML_NOVALUE = "<{0}><FieldRef {1}='{2}'/></{0}>";

        string _value1;
        string _value2;
        int[] _values;
        bool _includeTimeValue;
        Types.FieldTypes _fieldType;
        Types.QueryTypes _queryType;

        #endregion Private Members

        #region Public Properties

        /// <summary>
        /// Get or set the value of the SharePoint field that is being queried.
        /// </summary>
        public string Value1
        {
            get { return _value1; }
            set { _value1 = value; }
        }

        public string Value2
        {
            get { return _value2; }
            set { _value2 = value; }
        }

        /// <summary>
        /// Get or set the value of the SharePoint field that is being queried.
        /// </summary>
        public int[] Values
        {
            get { return _values; }
            set { _values = value; }
        }
        /// <summary>
        /// Get or set the datetime is include time value.
        /// </summary>
        public bool IncludeTimeValue
        {
            get { return _includeTimeValue; }
            set { _includeTimeValue = value; }
        }
        /// <summary>
        /// Get or set the type of the field being queried.
        /// </summary>
        public Types.FieldTypes FieldType
        {
            get { return _fieldType; }
            set { _fieldType = value; }
        }

        /// <summary>
        /// Get or set the type of defalut query to perform.
        /// </summary>
        public Types.QueryTypes QueryType
        {
            get { return _queryType; }
            set { _queryType = value; }
        }

        public static string CAML_MMS_VALUE1 => CAML_MMS_VALUE;

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Initialise the <see cref="T:SharePointStu.CAMLHelper.CAML.Query"/> object.
        /// </summary>
        /// <param name="field">The value of the field identifier to query against.</param>
        /// <param name="fieldRefType">The <see cref="T:SharePointStu.CAMLHelper.CAML.Types.FieldRefTypes"/> object type of the field being queried.</param>
        /// <param name="fieldType">The <see cref="T:SharePointStu.CAMLHelper.CAML.Types.FieldTypes"/> object of the field.</param>
        /// <param name="queryType">The <see cref="T:SharePointStu.CAMLHelper.CAML.Types.FieldRefTypes"/> object that defines the type of query to perform.</param>
        /// <param name="value">The value of the field being queried.</param>
        public Query(string field, Types.FieldRefTypes fieldRefType, Types.FieldTypes fieldType, Types.QueryTypes queryType, string value, bool isIncludeTimeValue)
            : base(field, fieldRefType)
        {
            _fieldType = fieldType;
            _queryType = queryType;
            _value1 = value;
            _includeTimeValue = isIncludeTimeValue;
            if (queryType != Types.QueryTypes.IsNotNull && queryType != Types.QueryTypes.IsNull)
            {
                Validate();
            }
        }

        public Query(string field, Types.FieldRefTypes fieldRefType, Types.FieldTypes fieldType, Types.QueryTypes queryType, string value, string value2, bool isIncludeTimeValue)
            : base(field, fieldRefType)
        {
            _fieldType = fieldType;
            _queryType = queryType;
            _value1 = value;
            _value2 = value2;
            _includeTimeValue = isIncludeTimeValue;
            if (queryType != Types.QueryTypes.IsNotNull && queryType != Types.QueryTypes.IsNull)
            {
                Validate();
            }
        }
        public Query(string field, Types.FieldRefTypes fieldRefType, Types.FieldTypes fieldType, Types.QueryTypes queryType, int[] values)
            : base(field, fieldRefType)
        {
            _fieldType = fieldType;
            _queryType = queryType;
            _values = values;
            if (queryType != Types.QueryTypes.IsNotNull && queryType != Types.QueryTypes.IsNull)
            {
                Validate();
            }
        }
        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Get the CAML code for the individual query.
        /// </summary>
        /// <returns>CAML string for the individual query.</returns>
        public override string GetCAML()
        {
            string caml = string.Empty;

            if (!string.IsNullOrEmpty(this.Field))
            {
                if (this.QueryType == Types.QueryTypes.IsNull || this.QueryType == Types.QueryTypes.IsNotNull)
                {
                    caml = string.Format(CAML_NOVALUE, this.QueryType.ToString(), this.FieldRefType.ToString(), this.Field);
                }
                else
                {
                    if (Validate())
                    {
                        if (this.Field == "ID")
                        {
                            StringBuilder camlValue = new StringBuilder();
                            camlValue.Append(string.Format(CAML_MMS_VALUE, this.Value1));
                            caml = string.Format(IDCAML, this.QueryType.ToString(), this.FieldRefType.ToString(), this.Field, camlValue, this.FieldType.ToString());
                        }
                        else if (this.QueryType == Types.QueryTypes.In && this.FieldType == Types.FieldTypes.MMSData)
                        {
                            StringBuilder camlValue = new StringBuilder();
                            for (int i = 0; i < this.Values.Length; i++)
                            {
                                camlValue.Append(string.Format(CAML_MMS_VALUE1, this.Values[i]));
                            }
                            caml = string.Format(CAML_MMS, this.Field, camlValue);
                        }
                        else if (QueryType == Types.QueryTypes.FromTo)
                        {
                            caml = string.Format("<And>{0}{1}</And>",
                                string.Format(CAML, Types.QueryTypes.Gt.ToString(), this.FieldRefType.ToString(), this.Field, this.FieldType.ToString(),
                                _includeTimeValue ? CAML_INCLUDETIME : "", this.Value1),
                                string.Format(CAML, Types.QueryTypes.Lt.ToString(), this.FieldRefType.ToString(), this.Field, this.FieldType.ToString(),
                                _includeTimeValue ? CAML_INCLUDETIME : "", this.Value2));
                        }
                        else if (QueryType == Types.QueryTypes.FromTo_1_1)
                        {
                            caml = string.Format("<And>{0}{1}</And>", 
                                string.Format(CAML, Types.QueryTypes.Geq.ToString(), this.FieldRefType.ToString(), this.Field, this.FieldType.ToString(),
                                _includeTimeValue ? CAML_INCLUDETIME : "", this.Value1),
                                string.Format(CAML, Types.QueryTypes.Leq.ToString(), this.FieldRefType.ToString(), this.Field, this.FieldType.ToString(),
                                _includeTimeValue ? CAML_INCLUDETIME : "", this.Value2));
                        }
                        //else if (QueryType == Types.QueryTypes.FromTo_1_0)
                        //{
                        //    caml = string.Format("<And>{0}{1}</And>", 
                        //        string.Format(CAML, Types.QueryTypes.Geq.ToString(), this.FieldRefType.ToString(), this.Field, this.FieldType.ToString(),
                        //        _includeTimeValue ? CAML_INCLUDETIME : "", this.Value1),
                        //        string.Format(CAML, Types.QueryTypes.Lt.ToString(), this.FieldRefType.ToString(), this.Field, this.FieldType.ToString(),
                        //        _includeTimeValue ? CAML_INCLUDETIME : "", this.Value2));
                        //}
                        //else if (QueryType == Types.QueryTypes.FromTo_0_1)
                        //{
                        //    caml = string.Format("<And>{0}{1}</And>", 
                        //        string.Format(CAML, Types.QueryTypes.Gt.ToString(), this.FieldRefType.ToString(), this.Field, this.FieldType.ToString(),
                        //        _includeTimeValue ? CAML_INCLUDETIME : "", this.Value1),
                        //        string.Format(CAML, Types.QueryTypes.Leq.ToString(), this.FieldRefType.ToString(), this.Field, this.FieldType.ToString(),
                        //        _includeTimeValue ? CAML_INCLUDETIME : "", this.Value2));
                        //}
                        else
                        {
                            caml = string.Format(CAML, this.QueryType.ToString(), this.FieldRefType.ToString(), this.Field, this.FieldType.ToString(),
                                this.FieldType == Types.FieldTypes.DateTime && _includeTimeValue ? CAML_INCLUDETIME : "", this.Value1);
                        }
                    }
                }
            }

            return caml;
        }

        /// <summary>
        /// Get the CAML code for the individual query.
        /// </summary>
        /// <returns>CAML for the individual query as an XmlDocument.</returns>
        public XmlDocument GetCAMLAsXML()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(GetCAML());

            return doc;
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Validate the values of the <see cref="T:SharePointStu.CAMLHelper.CAML.Query"/> object.
        /// </summary>
        /// <returns>bool: if the <see cref="T:SharePointStu.CAMLHelper.CAML.Query"/> object values are valid.</returns>
        protected override bool Validate()
        {
            base.Validate();

            // Check the value for the query matches the data type of the field.
            switch (this.FieldType)
            {
                case Types.FieldTypes.YesNo:
                    bool boolValue;
                    if (!bool.TryParse(this.Value1, out boolValue))
                        throw new InvalidFieldValueException(this.Field, this.FieldType, this.Value1);
                    break;
                case Types.FieldTypes.Counter:
                    int counterValue;
                    if (!int.TryParse(this.Value1, out counterValue))
                        throw new InvalidFieldValueException(this.Field, this.FieldType, this.Value1);
                    break;
                case Types.FieldTypes.Number:
                    float numberValue;
                    if (!float.TryParse(this.Value1, out numberValue))
                        throw new InvalidFieldValueException(this.Field, this.FieldType, this.Value1);
                    break;
                case Types.FieldTypes.DateTime:
                    DateTime dateValue;
                    if (!DateTime.TryParse(this.Value1, out dateValue))
                        throw new InvalidFieldValueException(this.Field, this.FieldType, this.Value1);
                    if (QueryType == Types.QueryTypes.FromTo && !DateTime.TryParse(this.Value2, out dateValue))
                    {
                        throw new InvalidFieldValueException(this.Field, this.FieldType, this.Value2);
                    }
                    break;
            }

            return true;
        }

        #endregion Protected Methods
    }
}
