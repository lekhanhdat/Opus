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
    class AveFieldDateTime : AveField, IAveFieldDateTime
    {
        private SPFieldDateTime mFieldDateTime;

        public AveFieldDateTime(AveFieldCollection fieldColl, SPFieldDateTime field)
            : base(fieldColl, field)
        {
            mFieldDateTime = field;
        }

        #region IAveFieldDateTime Members

        public AveDateTimeFieldFormatType DisplayFormat
        {
            get
            {
                return (AveDateTimeFieldFormatType)mFieldDateTime.DisplayFormat;
            }
            set
            {
                mFieldDateTime.DisplayFormat = (SPDateTimeFieldFormatType)value;
            }
        }

        public AveCalendarType CalendarType
        {
            get
            {
                return (AveCalendarType)mFieldDateTime.CalendarType;
            }
            set
            {
                mFieldDateTime.CalendarType = (SPCalendarType)value;
            }
        }

        public override Type FieldValueType
        {
            get
            {
                return typeof(DateTime);
            }
        }

        public override string GetFieldValueAsText(object value)
        {
            return mFieldDateTime.GetFieldValueAsText(value);
        }

        public override object GetFieldValue(string value)
        {
            return mFieldDateTime.GetFieldValue(value);
        }

        #endregion

        #region add for SP2013
        public AveSPDateTimeFieldFriendlyFormatType FriendlyDisplayFormat
        {
            get { return (AveSPDateTimeFieldFriendlyFormatType)mFieldDateTime.FriendlyDisplayFormat; }
            set { mFieldDateTime.FriendlyDisplayFormat = (SPDateTimeFieldFriendlyFormatType)value; }
        }
        #endregion
    }
}
