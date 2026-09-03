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

namespace AvePoint.ObjectModel.Server19
{
    class AveFieldUser : AveFieldLookup, IAveFieldUser
    {
        private SPFieldUser mFieldUser;

        public AveFieldUser(AveFieldCollection fieldColl, SPFieldUser field)
            : base(fieldColl, field)
        {
            mFieldUser = field;
        }

        public override Type FieldValueType
        {
            get
            {
                if (this.AllowMultipleValues)
                {
                    return typeof(IAveFieldUserValueCollection);
                }
                return typeof(IAveFieldUserValue);
            }
        }

        public override object GetFieldValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            if (this.AllowMultipleValues)
            {
                return new AveFieldUserValueCollection(new SPFieldUserValueCollection((base.Fields.Web as AveWeb).Web, value));
            }
            return new AveFieldUserValue(new SPFieldUserValue((base.Fields.Web as AveWeb).Web, value));
        }

        /// <summary>
        /// SharePoint API for GetFieldValueAsText()
        /// </summary>
        /// <param name="value">this value object should be Ave Object or null</param>
        /// <returns></returns>
        public override string GetFieldValueAsText(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            if (value is AveFieldUserValueCollection)
            {
                SPFieldUserValueCollection typedFieldValue = (value as AveFieldUserValueCollection).FieldUserValueCollection;

                if (typedFieldValue == null)
                {
                    typedFieldValue = this.GetTypedFieldValue(value);
                }
                if ((typedFieldValue == null) || (typedFieldValue.Count == 0))
                {
                    return string.Empty;
                }
                return mFieldUser.GetFieldValueAsText(typedFieldValue);
            }
            else
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// SharePoint API for GetFieldValueAsText
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private SPFieldUserValueCollection GetTypedFieldValue(object value)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldUser.GetTypedFieldValue"))
            {

                if (value == null)
                {
                    return null;
                }
                SPFieldUserValueCollection values = (value as AveFieldUserValueCollection).FieldUserValueCollection;
                if (values == null)
                {
                    values = new SPFieldUserValueCollection();
                    if (value is string)
                    {
                        object fieldValue = this.GetFieldValue((string)value);
                        if (this.AllowMultipleValues)
                        {
                            return (fieldValue as AveFieldUserValueCollection).FieldUserValueCollection;
                        }
                        values.Add((fieldValue as AveFieldUserValue).FieldUserValue);
                        return values;
                    }
                    SPFieldUserValue item = (value as AveFieldUserValue).FieldUserValue;
                    if (item == null)
                    {
                        throw new ArgumentException();
                    }
                    values.Add(item);
                }
                return values;

            }

        }

        #region IAveFieldUser Members

        public bool AllowDisplay
        {
            get
            {
                return mFieldUser.AllowDisplay;
            }
            set
            {
                mFieldUser.AllowDisplay = value;
            }
        }

        public bool Presence
        {
            get
            {
                return mFieldUser.Presence;
            }
            set
            {
                mFieldUser.Presence = value;
            }
        }

        public int SelectionGroup
        {
            get
            {
                return mFieldUser.SelectionGroup;
            }
            set
            {
                mFieldUser.SelectionGroup = value;
            }
        }

        public AveFieldUserSelectionMode SelectionMode
        {
            get
            {
                return (AveFieldUserSelectionMode)mFieldUser.SelectionMode;
            }
            set
            {
                mFieldUser.SelectionMode = (SPFieldUserSelectionMode)value;
            }
        }

        #endregion
    }
}
