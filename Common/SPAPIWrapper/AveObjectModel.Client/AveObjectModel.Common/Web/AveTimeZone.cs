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
using System.Text.RegularExpressions;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveTimeZone : AveClientObject, IAveTimeZone
    {
        private AveRegionalSettings mRegionalSettings;
        private IAveRequest mRequest;

        public AveTimeZone(AveRegionalSettings regionalSettings, IAveRequest request, Dictionary<string, object> timeZoneProperties)
        {
            mRegionalSettings = regionalSettings;
            mRequest = request;
            base.DataCache.AddPropertyies(timeZoneProperties);
            mRegionalSettings.DataCache.AddChangedProperty("TimeZoneChangedProperties", base.DataCache.ChangedProperties);
        }

        public AveTimeZone(IAveRequest request, Dictionary<string, object> timeZoneProperties)
        {
            mRequest = request;
            base.DataCache.AddPropertyies(timeZoneProperties);
        }

        #region IAveTimeZone Members

        public DateTime UTCToLocalTime(DateTime dateTime)
        {
            if (Description != null)
            {
                string timeZone = Regex.Match(Description, "([^)]*)").Value;
                if (timeZone != null)
                {
                    if (timeZone.Length >= 6)
                    {
                        string time = timeZone.Substring(timeZone.Length - 6);
                        string[] hm = time.Split(':');
                        int h = int.Parse(hm[0].Substring(1));
                        int m = int.Parse(hm[1]);
                        switch (hm[0][0])
                        {
                            case '+':
                                dateTime = dateTime.AddHours(h).AddMinutes(m); ;
                                break;
                            case '-':
                                dateTime = dateTime.AddHours(-h).AddMinutes(-m);
                                break;
                            default:
                                dateTime = dateTime.ToUniversalTime();
                                break;
                        }
                    }
                    else
                    {
                        return dateTime;
                    }
                }
                else
                {
                    dateTime = dateTime.ToUniversalTime();
                }

            }
            return dateTime;
        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
        }

        public ushort ID
        {
            get
            {
                return base.DataCache.GetProperty<ushort>("ID");
            }
            set
            {
                base.DataCache.AddChangedProperty("ID", value);
            }
        }


        public DateTime LocalTimeToUTC(DateTime dateTime)
        {
            return dateTime.ToLocalTime().ToUniversalTime();
        }

        #endregion


    }
}
