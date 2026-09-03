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


using AvePoint.GCommon.Utility.TimeZoneConvert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class AveTimeZoneUtility
    {
        private static Dictionary<string, string> timeZoneInfoAndSPTimeZoneDisolayNameMapping;
        private static Dictionary<int, string> timeZoneInfoAndSPTimeZoneIdMapping;

        static AveTimeZoneUtility()
        {
            #region DisplayName mapping
            timeZoneInfoAndSPTimeZoneDisolayNameMapping = new Dictionary<string, string>() 
            {
                {"(UTC) Greenwich Mean Time : Dublin, Edinburgh, Lisbon, London","(UTC) Dublin, Edinburgh, Lisbon, London"},
                {"(UTC+11:00) Magadan, Solomon Is., New Caledonia","(UTC+11:00) Magadan"},
                {"(UTC-05:00) Eastern Time (US and Canada)","(UTC-05:00) Eastern Time (US & Canada)"},
                {"(UTC-07:00) Mountain Time (US and Canada)","(UTC-07:00) Mountain Time (US & Canada)"},
                {"(UTC-08:00) Pacific Time (US and Canada)","(UTC-08:00) Pacific Time (US & Canada)"},
            };
            #endregion

            #region Init Id Mapping
            timeZoneInfoAndSPTimeZoneIdMapping = new Dictionary<int, string>()
            {
                {2,"GMT Standard Time"},
                {3,"Romance Standard Time"},
                {4,"W. Europe Standard Time"},
                {5,"GTB Standard Time"},
                {6,"Central Europe Standard Time"},
                {7,"E. Europe Standard Time"},
                {8,"E. South America Standard Time"},
                {9,"Atlantic Standard Time"},
                {10,"Eastern Standard Time"},
                {11,"Central Standard Time"},
                {12,"Mountain Standard Time"},
                {13,"Pacific Standard Time"},
                {14,"Alaskan Standard Time"},
                {15,"Hawaiian Standard Time"},
                {16,"Samoa Standard Time"},
                {17,"New Zealand Standard Time"},
                {18,"E. Australia Standard Time"},
                {19,"Cen. Australia Standard Time"},
                {20,"Tokyo Standard Time"},
                {21,"Singapore Standard Time"},
                {22,"SE Asia Standard Time"},
                {23,"India Standard Time"},
                {24,"Arabian Standard Time"},
                {25,"Iran Standard Time"},
                {26,"Arabic Standard Time"},
                {27,"Israel Standard Time"},
                {28,"Newfoundland Standard Time"},
                {29,"Azores Standard Time"},
                {30,"Mid-Atlantic Standard Time"},
                {31,"Greenwich Standard Time"},
                {32,"SA Eastern Standard Time"},
                {33,"SA Western Standard Time"},
                {34,"US Eastern Standard Time"},
                {35,"SA Pacific Standard Time"},
                {36,"Canada Central Standard Time"},
                {37,"Central Standard Time (Mexico)"},
                {38,"US Mountain Standard Time"},
                {39,"Dateline Standard Time"},
                {40,"Fiji Standard Time"},
                {41,"Central Pacific Standard Time"},
                {42,"Tasmania Standard Time"},
                {43,"West Pacific Standard Time"},
                {44,"AUS Central Standard Time"},
                {45,"China Standard Time"},
                {46,"N. Central Asia Standard Time"},
                {47,"West Asia Standard Time"},
                {48,"Afghanistan Standard Time"},
                {49,"Egypt Standard Time"},
                {50,"South Africa Standard Time"},
                {51,"Russian Standard Time"},
                {53,"Cape Verde Standard Time"},
                {54,"Azerbaijan Standard Time"},
                {55,"Central America Standard Time"},
                {56,"E. Africa Standard Time"},
                {57,"Central European Standard Time"},
                {58,"Ekaterinburg Standard Time"},
                {59,"FLE Standard Time"},
                {60,"Greenland Standard Time"},
                {61,"Myanmar Standard Time"},
                {62,"Nepal Standard Time"},
                {63,"North Asia East Standard Time"},
                {64,"North Asia Standard Time"},
                {65,"Pacific SA Standard Time"},
                {66,"Sri Lanka Standard Time"},
                {67,"Tonga Standard Time"},
                {68,"Vladivostok Standard Time"},
                {69,"W. Central Africa Standard Time"},
                {70,"Yakutsk Standard Time"},
                {71,"Bangladesh Standard Time"},
                {72,"Korea Standard Time"},
                {73,"W. Australia Standard Time"},
                {74,"Arab Standard Time"},
                {75,"Taipei Standard Time"},
                {76,"AUS Eastern Standard Time"},
                {77,"Mountain Standard Time (Mexico)"},
                {78,"Pacific Standard Time (Mexico)"},
                {79,"Jordan Standard Time"},
                {80,"Middle East Standard Time"},
                {81,"Central Brazilian Standard Time"},
                {82,"Georgian Standard Time"},
                {83,"Namibia Standard Time"},
                {84,"Caucasus Standard Time"},
                {85,"Argentina Standard Time"},
                {86,"Morocco Standard Time"},
                {87,"Pakistan Standard Time"},
                {88,"Venezuela Standard Time"},
                {89,"Mauritius Standard Time"},
                {90,"Montevideo Standard Time"},
                {91,"Paraguay Standard Time"},
                {92,"Kamchatka Standard Time"},
                {93,"UTC"},
                {94,"Ulaanbaatar Standard Time"},
                {95,"UTC-11"},
                {96,"UTC-02"},
                {97,"UTC+12"},
                {98,"Syria Standard Time"},
            };
            #endregion
        }

        public static TimeZoneInfo ToTimeZoneInfo(IAveTimeZone zone)
        {
            if (zone == null)
            {
                throw new ArgumentNullException("zone");
            }
            string id = ToTimeZoneInfoId(zone.ID);
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Can not convert to System.TimeZoneInfo, zone.ID is {0}", zone.ID.ToString());
            }
            //return TimeZoneInfo.FindSystemTimeZoneById(id); //TODO Cyrus
            return TimeZoneConvertHelper.FindSystemTimeZoneById(id);
        }

        public static string ToTimeZoneInfoDisplayName(string spTimeZoneDescription)
        {
            string mappedName = spTimeZoneDescription;
            if (timeZoneInfoAndSPTimeZoneDisolayNameMapping.ContainsKey(spTimeZoneDescription))
            {
                mappedName = timeZoneInfoAndSPTimeZoneDisolayNameMapping[spTimeZoneDescription];
            }
            return mappedName;
        }

        public static string ToTimeZoneInfoId(ushort spTimeZonId)
        {
            if (timeZoneInfoAndSPTimeZoneIdMapping.ContainsKey(spTimeZonId))
            {
                return timeZoneInfoAndSPTimeZoneIdMapping[spTimeZonId];
            }
            return string.Empty;
        }

        /// <summary>
        /// 将不同格式的时间格式转化成DataTime
        /// </summary>
        /// <param name="timeStr"></param>
        /// <param name="localedId"></param>
        /// <param name="isTime24">是否为24小时制</param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(string timeStr, int localedId, bool isTime24)
        {
            timeStr = timeStr.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", "");
            try
            {
                return Convert.ToDateTime(timeStr);
            }
            catch
            {
                string timeFormat = string.Empty;
                DateTime time;
                switch (localedId)
                {
                    //如果发现没有匹配的格式,将其添加进来
                    case 1033: timeFormat = isTime24 ? "M/d/yyyy h:mm" : "M/d/yyyy h:mm tt";
                        break;
                    case 4100: timeFormat = isTime24 ? "d/M/yyyy h:mm" : "d/M/yyyy tt h:mm";
                        break;
                    case 5129: timeFormat = isTime24 ? "d/MM/yyyy h:mm" : "d/MM/yyyy h:mm tt";
                        break;
                    default: timeFormat = isTime24 ? "M/d/yyyy h:mm" : "M/d/yyyy h:mm tt";
                        break;
                }
                if (DateTime.TryParseExact(timeStr, timeFormat, new System.Globalization.CultureInfo(localedId), System.Globalization.DateTimeStyles.None, out time))
                {
                    return time;
                }
                return DateTime.UtcNow;
            }
        }
    }
}
