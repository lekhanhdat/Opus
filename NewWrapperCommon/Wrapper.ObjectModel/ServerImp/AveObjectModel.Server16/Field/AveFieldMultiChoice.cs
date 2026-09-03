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
using System.Collections.Specialized;
using System;
using System.Text;

namespace AvePoint.ObjectModel.Server16
{
    class AveFieldMultiChoice : AveField, IAveFieldMultiChoice
    {
        private SPFieldMultiChoice mFieldMultiChoice;

        public AveFieldMultiChoice(AveFieldCollection filedColl, SPFieldMultiChoice field)
            : base(filedColl, field)
        {
            mFieldMultiChoice = field;
        }

        public AveFieldMultiChoice(SPFieldMultiChoice fieldMultiChoice)
            : base(fieldMultiChoice)
        {
            mFieldMultiChoice = fieldMultiChoice;
        }

        #region IAveFieldMultiChoice Members

        public StringCollection Choices
        {
            get
            {
                return mFieldMultiChoice.Choices;
            }
        }

        public bool FillInChoice
        {
            get
            {
                return mFieldMultiChoice.FillInChoice;
            }
            set
            {
                mFieldMultiChoice.FillInChoice = value;
            }
        }

        public override Type FieldValueType
        {
            get
            {
                return typeof(IAveFieldMultiChoiceValue);
            }
        }

        public override object GetFieldValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return new AveFieldMultiChoiceValue(new SPFieldMultiChoiceValue(value));

        }

        /// <summary>
        /// SharePoint API
        /// </summary>
        /// <param name="value">This value must be an Ave Object or null</param>
        /// <returns></returns>
        public override string GetFieldValueAsText(object value)
        {
            SPFieldMultiChoiceValue value2;
            if (value == null)
            {
                return string.Empty;
            }
            if (value is AveFieldMultiChoiceValue)
            {
                value2 = (value as AveFieldMultiChoiceValue).FieldMultiChoiceValue;
            }
            else
            {
                value2 = new SPFieldMultiChoiceValue((string)value);
            }
            string str = ", ";
            StringBuilder builder = new StringBuilder(0xff);
            for (int i = 0; i < value2.Count; i++)
            {
                string str2 = value2[i];
                if (!string.IsNullOrEmpty(str2))
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(str);
                    }
                    builder.Append(str2);
                }
            }
            return builder.ToString();
        }

        public override void Update()
        {
            mFieldMultiChoice.Update();
        }

        #endregion
    }
}
