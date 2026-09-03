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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    class AveTimeZone : AveClientObject, IAveTimeZone
    {
        private AveRegionalSettings mRegionalSettings;
        private IAveRequest mRequest;
        protected static AveLogger Log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveTimeZone(AveRegionalSettings regionalSettings, IAveRequest request, Dictionary<string, object> timeZoneProperties)
        {
            mRegionalSettings = regionalSettings;
            mRequest = request;
            base.DataCache.AddPropertyies(timeZoneProperties);
            mRegionalSettings.DataCache.AddChangedProperty("TimeZoneChangedProperties", base.DataCache.ChangedProperties);
        }

        public AveTimeZone(IAveRequest request,Dictionary<string, object> timeZoneProperties)
        {
            mRequest = request;
            base.DataCache.AddPropertyies(timeZoneProperties);
        }

        #region IAveTimeZone Members

        public DateTime UTCToLocalTime(DateTime dateTime)
        {
            DateTime result = dateTime;
            //模拟13以上的版本直接用API
            if ((mRequest.Type == AveClientRequestType.AveClientOM2013Request || mRequest.Type == AveClientRequestType.AveClientOMOffice365Request) &&
                mRegionalSettings != null && mRegionalSettings.Web != null)
            {
               result = (mRequest ).GetUTCToLocalTime(mRegionalSettings.Web.ServerRelativeUrl, dateTime);
            }
            else
            {
                if (Description != null)
                {
                    try
                    {
                        string timeZone = Regex.Match(Description, "([^)]*)").Value;
                        //(UTC-08:00) Pacific Time (US and Canada)   0时区比较特殊((UTC) Coordinated Universal Time)
                        if (!string.IsNullOrEmpty(timeZone) && timeZone.Length >= 6)
                        {
                            string time = timeZone.Substring(timeZone.Length - 6);
                            string[] hm = time.Split(':');
                            int h = int.Parse(hm[0].Substring(1));
                            int m = 0;
                            if (hm.Length == 2 && hm[0].Length > 0)
                            {
                                //如果没有计算出值，时差默认算为0
                                if (!int.TryParse(hm[0].Substring(1), out h))
                                {
                                    h = 0;
                                }
                                int.TryParse(hm[1], out m);
                                switch (hm[0][0])
                                {
                                    case '+':
                                        result = dateTime.AddHours(h).AddMinutes(m);
                                        ;
                                        break;
                                    case '-':
                                        result = dateTime.AddHours(-h).AddMinutes(-m);
                                        break;
                                    default:
                                        result = dateTime.ToUniversalTime();
                                        break;
                                }

                            }
                        }
                        else
                        {
                            result = dateTime.ToUniversalTime();
                        }
                    }
                    catch (Exception e)
                    {
                        result = dateTime.ToUniversalTime();
                        Log.Warn("An error occurred while convert utc time to local by TimeZone Description {0},Error:{1}",Description,e);
                    }

                }
            }
            return result;
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
            //对于Unspecified的time，我们会默认处理成UTC时间，如果直接ToUniversalTime会当成local time处理，所以先ToLocalTime再ToUniversalTime
            DateTime uTC =  dateTime.ToLocalTime().ToUniversalTime();
            if (mRequest.Type == AveClientRequestType.AveClientOM2013Request
                || mRequest.Type == AveClientRequestType.AveClientOM2016Request
                || mRequest.Type == AveClientRequestType.AveClientOM2019Request
                || mRequest.Type == AveClientRequestType.AveClientOMOffice365Request)
            {
                if (mRegionalSettings != null && mRegionalSettings.Web != null)
                {
                    uTC = (mRequest ).GetLocalToUTCTime(mRegionalSettings.Web.ServerRelativeUrl, dateTime);                 
                }
            }
            if (uTC.Kind != DateTimeKind.Utc)
            {
                uTC = DateTime.SpecifyKind(uTC, DateTimeKind.Utc);
            }
            return uTC;
        }

        #endregion


    }
}
