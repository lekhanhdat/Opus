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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
namespace AvePoint.ObjectModel.Common
{
    class AveFieldDateTime : AveField, IAveFieldDateTime
    {
        private IAveRequest mRequest;
        private AveList mParentList;
        private AveWeb mWeb;
        private AveFieldCollection mFieldCollection;
        private string mFieldSource;
        private Dictionary<string, object> mContentTypeProp;

        public AveFieldDateTime(IAveRequest request, AveList list, AveWeb web, string fieldSource, AveFieldCollection fieldCollection, Dictionary<string, object> contentTypeProp, Dictionary<string, object> prop)
            : base(request, list, web, fieldSource, fieldCollection, contentTypeProp, prop)
        {
            mRequest = request;
            mParentList = list;
            mWeb = web;
            mFieldCollection = fieldCollection;
            mFieldSource = fieldSource;
            mContentTypeProp = contentTypeProp;
            base.DataCache.AddPropertyies(prop);
        }
        public AveCalendarType CalendarType
        {
            get
            {
                return base.DataCache.GetProperty<AveCalendarType>("CalendarType");
            }
            set
            {
                base.DataCache.AddChangedProperty("CalendarType", (int)value);
            }
        }
        public AveDateTimeFieldFormatType DisplayFormat
        {
            get
            {
                return base.DataCache.GetProperty<AveDateTimeFieldFormatType>("DisplayFormat");
            }
            set
            {
                base.DataCache.AddChangedProperty("DisplayFormat", (int)value);
            }
        }

        #region add for SP2013
        private AveSPDateTimeFieldFriendlyFormatType mFriendlyDisplayFormat = AveSPDateTimeFieldFriendlyFormatType.Unspecified;
        public AveSPDateTimeFieldFriendlyFormatType FriendlyDisplayFormat
        {
            get { return base.DataCache.GetProperty<AveSPDateTimeFieldFriendlyFormatType>("FriendlyDisplayFormat"); }
            set { base.DataCache.AddChangedProperty("FriendlyDisplayFormat", (int)value); }
        }
        #endregion

        public override object DefaultValueTyped
        {
            get
            {
                return InitializeDefaultValueTyped();
            }
        }

        /// <summary>
        /// not used for now
        /// </summary>
        /// <returns></returns>
        private object InitializeDefaultValueTyped()
        {
            string defaultValueTyped = (string)base.DefaultValueTyped;
            if (string.IsNullOrEmpty(defaultValueTyped))
            {
                return null;
            }

            if (0 == CultureInfo.InvariantCulture.CompareInfo.Compare(defaultValueTyped, "[Today]", CompareOptions.IgnoreCase))
            {
                DateTime time = mWeb.RegionalSettings.TimeZone.UTCToLocalTime(DateTime.UtcNow).AddHours(1.0);
                return new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0, new GregorianCalendar());
            }

            try
            {
                DateTime date = new AveUtility().CreateSystemDateTimeFromXmlDataDateTimeFormat(defaultValueTyped);

                if ((this.DefaultValue == null) || (this.DisplayFormat != AveDateTimeFieldFormatType.DateTime))
                {
                    return date;
                }
                return mWeb.RegionalSettings.TimeZone.UTCToLocalTime(date);
            }
            catch (FormatException)
            {
                return null;
            }

        }
    }
}
