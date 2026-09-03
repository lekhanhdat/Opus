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
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvePoint.RA.Common.Util;

namespace AvePoint.RA.Web.Models.ControlPanel
{
    public class GeneralSettingJsModel
    {
        public GeneralSettingModel GeneralSettingModel { get; set; }

        public List<GCommon.Contract.Server.Common.TimeZone.AveTimeZone> TimeZones = DateTimeUtil.GetAllStaticTimeZones();//GeneralSettingConfig.TimeZones;
        public List<KeyValuePair<TimeFormat, string>> TimeFormats
        { 
            get
            {
                List<KeyValuePair<TimeFormat, string>> value = new List<KeyValuePair<TimeFormat, string>>();
               foreach (var item in GeneralSettingConfig.TimeFormats)
               {
                   value.Add(item);
               };
               return value;
            }
        }
        public List<KeyValuePair<DateFormat, string>> DateFormats
        {
            get{
                List<KeyValuePair<DateFormat, string>> value = new List<KeyValuePair<DateFormat, string>>();
                foreach (var item in GeneralSettingConfig.DateFormats)
                {
                    value.Add(item);
                };
                return value;
            }
        }
        public List<KeyValuePair<SessionTimeUnit, string>> SessionTimeUnits
        {
            get{
                return new List<KeyValuePair<SessionTimeUnit, string>>()
                {
                    new KeyValuePair<SessionTimeUnit,string>(SessionTimeUnit.hours,I18NEntity.GetString("RM_GS_SessionTime_Hour")),
                    new KeyValuePair<SessionTimeUnit,string>(SessionTimeUnit.minutes,I18NEntity.GetString("RM_GS_SessionTime_Minute"))
                };
            }
        }
    }
}