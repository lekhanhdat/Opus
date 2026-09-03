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
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveFieldGeolocationValue : IAveFieldGeolocationValue
    {
        private SPFieldGeolocationValue mFieldGeolocationValue;
        public AveFieldGeolocationValue()
        {
            mFieldGeolocationValue = new SPFieldGeolocationValue();
        }

        public AveFieldGeolocationValue(string fieldValue)
        {
            mFieldGeolocationValue = new SPFieldGeolocationValue(fieldValue);
        }

        public AveFieldGeolocationValue(double latitude, double longitude)
        {
            mFieldGeolocationValue = new SPFieldGeolocationValue(latitude, longitude);
        }

        public AveFieldGeolocationValue(double latitude, double longitude, double altitude, double measure)
        {
            mFieldGeolocationValue = new SPFieldGeolocationValue(latitude, longitude, altitude, measure);
        }

        public double Altitude
        {
            get
            {
                return mFieldGeolocationValue.Altitude;
            }
            set
            {
                mFieldGeolocationValue.Altitude = value;
            }
        }

        public double Latitude
        {
            get
            {
                return mFieldGeolocationValue.Latitude;
            }
            set
            {
                mFieldGeolocationValue.Latitude = value;
            }
        }

        public double Longitude
        {
            get
            {
                return mFieldGeolocationValue.Longitude;
            }
            set
            {
                mFieldGeolocationValue.Longitude = value;
            }
        }

        public double Measure
        {
            get
            {
                return mFieldGeolocationValue.Measure;
            }
            set
            {
                mFieldGeolocationValue.Measure = value;
            }
        }

        public string ToString()
        {
            return mFieldGeolocationValue.ToString();
        }
    }
}
