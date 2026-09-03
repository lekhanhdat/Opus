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
using System.Text;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;

namespace AvePoint.GCommon.Utility
{
    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
    /// <summary>
    /// DateTime格式化类
    /// 添加方法：
    /// 1. 在Date Type中添加需要格式化的format，请使用const修饰符，并且是private
    /// 2. 在Convert方法添加自己函数，一次编号，由于每个Type的含义都不太一样，使用有意义的名字比较难，所以请使用编号，谢谢。
    /// </summary>
    public class AveDateTimeUtility
    {
        #region -- Date Type --
        //Common Start
        private const string DATETYPEForCommon000 = "yyyyMMddhhmmssfff";//high precision
        //Common End

        private const string DATETYPE001 = "yyyy-MM-dd";

        //CP-Configuration Start

        private const string DATATYPECONFIGURATION001 = "M/d/yyyy h:mm:ss tt";
        private const string DATATYPECONFIGURATION002 = "M/d/yyyy h:mm tt";
        private const string DATATYPECONFIGURATION003 = "MM/dd/yyyy hh:mm:ss";
        private const string DATATYPECONFIGURATION004 = "M/d/yyyy h:mm:ss";
        private const string DATATYPECONFIGURATION005 = "M/d/yyyy hh:mm tt";
        private const string DATATYPECONFIGURATION006 = "M/d/yyyy hh tt";
        private const string DATATYPECONFIGURATION007 = "M/d/yyyy h:mm";
        private const string DATATYPECONFIGURATION008 = "M/d/yyyy h:mm";
        private const string DATATYPECONFIGURATION009 = "MM/dd/yyyy hh:mm";
        private const string DATATYPECONFIGURATION0010 = "M/dd/yyyy hh:mm";
        private const string DATATYPECONFIGURATION0011 = "MM-dd-yy hh:mmtt";

        //CP-Configuration End

        //CA Start
        private const string DATETYPEForCA001 = "MM/d/yyyy h:mm tt";
        private const string DATETYPEForCA002 = "MM/dd/yyyy h/mm tt";
        private const string DATETYPEForCA003 = "MM/dd/yyyy h:mm tt";
        private const string DATETYPEForCA004 = "yyyy-MM-ddTHH:mm:ssZ";
        private const string DATETYPEForCA005 = "yyyyMMddHHmmss";
        //CA End

        private const string DATETYPE002 = "yyyyMMddhhmmss";//PR FAST Search Server 会用到这个格式，如果要改动这个格式，请先联系PR的developer，谢谢
        private const string DATETYPE003 = "dd-MM-yyyy_HH.mm.ss";
        private const string DATETYPE004 = "MM/dd/yyyy HH:mm:ss";
        private const string DATETYPE005 = "yyyyMMddHHmm";
        private const string DATETYPE006 = "yyyy-MM-dd HH:mm:ss.fff";
        private const string DATETYPE007 = "yyyy-MM-dd HH:mm:ss";
        private const string DATETYPE008 = "MM_dd_yyyy_hh_mm_ss";//时间作为文件的后缀用_

        private const string DATETYPE009 = "yyyy_MM_dd_HH.mm.ss.fff";//PR
        private const string DATETYPE010 = "yyyyMMddHHmmss";//PR //SO
        public const string DATETYPE011 = "yyyy-MM-dd HH:mm:ss";//PR

        private const string DATETYPE013 = "yyyy-MM-dd hh-mm-ss";

        private const string DATETYPE012 = "yyyyMMdd";//Wrapper Report Center

        public const string DATETYPE014 = "MM/dd/yyyy"; //定义成public是因为在Migration Tool里面的控件的Format直接要引用这个字符串
        public const string DATETYPE015 = "yy";  //在FMOpenXml中Check Excel的内容格式调用，故定义成Public
        public const string DATETYPE016 = "mm"; //同上
        public const string DATETYPE017 = "yyyyMMddHHmmss";//File Migration检查pdf文件的属性格式调用 // SO
        private const string DATETYPE018 = "MM/dd/yyyy HH:mm";
        public const string DATETYPE019 = "yyyy-MM-dd";

        //for replicator
        public const string DATETYPE020 = "yyyy/MM/dd hh:mm:ss";
        public const string DATETYPE021 = "HH-mm-ss";
        public const string DATETYPE022 = "yyyyMMddhhmmss";
        public const string DATETYPEForRP = "yyyy_MM_dd_hh_mm_ss";
        public const string DATETYPERorRP001 = "MMddhhmmss";

        // for e-discovery
        public const String DATETYPE023 = "dd/MM/yyyy";

        private const string DATETYPEForPF001 = "yyyy-MM-ddThh:mm:ssZ";//PF
        private const string DATETYPEForPF002 = "yyyy-MM-ddThh:mm:00Z";//PF
        private const string DATETYPEForPF003 = "yyyyMMdd";//PF
        private const string DATETYPEForPF004 = "yyyy-MM-dd HH:mm:ss";//PF

        #region SystemOptionWeb Used
        public static string DATETYPE_FOR_SYSTEMOPTION001 // SystemOption Used
        {
            get
            {
                return "yyyy-MM-dd";
            }
        }
        public static string TIMETYPE_FOR_SYSTEMOPTION001 // SystemOption Used
        {
            get
            {
                return "HH:mm:ss";
            }
        }
        private class DateType
        {
            public const string DATE_TYPE_001 = "M-d-yyyy";
            public const string DATE_TYPE_003 = "M-d-yy";
            public const string DATE_TYPE_004 = "MM-dd-yy";
            public const string DATE_TYPE_006 = "d-MMM-yy";
            public const string DATE_TYPE_007 = "MMM d,yyyy";
            public const string DATE_TYPE_009 = "d-MMM-yyyy";
            public const string DATE_TYPE_010 = "yyyy-MM-dd";
            public const string DATE_TYPE_011 = "yy-MM-dd";
            public const string DATE_TYPE_012 = "yyyy MM dd";
            public const string DATE_TYPE_014 = "dd-MM-yyyy";
            public const string DATE_TYPE_015 = "d-M-yyyy";
            public const string DATE_TYPE_016 = "dd.MM.yyyy";
            public const string DATE_TYPE_017 = "dd.MM.yy";
            public const string DATE_TYPE_018 = "d.M.yy";
            public const string DATE_TYPE_019 = "dd-MM-yy";
            public const string DATE_TYPE_020 = "yy-M-d";
            public const string DATE_TYPE_021 = "yyyy-M-d";
            public const string DATE_TYPE_022 = "yyyy.MM.dd";
            public const string DATE_TYPE_023 = "d.M.yyyy";
            public const string DATE_TYPE_026 = "d-M-yy";
            public const string DATE_TYPE_027 = "d-M yyyy";
            public const string DATE_TYPE_028 = "dd-MM yyyy";
            public const string DATE_TYPE_029 = "dd-MM yy";
            public const string DATE_TYPE_030 = "d-M yy";
            public const string DATE_TYPE_031 = "yy.MM.dd";
            public const string DATE_TYPE_033 = "d MMM yy";
            public const string DATE_TYPE_034 = "d MMM yyyy";
            public const string DATE_TYPE_035 = "MM-dd-yyyy";
        }
        private class TimeType
        {
            public const string TIME_TYPE_001 = "hh:mm:ss tt";
            public const string TIME_TYPE_003 = "h:mm:ss tt";
            public const string TIME_TYPE_005 = "HH:mm:ss";
            public const string TIME_TYPE_006 = "H:mm:ss";
        }


        public static SortedDictionary<string, LocaleValue> Type = new SortedDictionary<string, LocaleValue>()
        {
            { Locale.VIETNAMESE, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_026, DateType.DATE_TYPE_015, DateType.DATE_TYPE_033 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.ARABIC, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_010, DateType.DATE_TYPE_015 }, TimeList = new List<string> { TimeType.TIME_TYPE_005 } }},
            { Locale.CHINESE_PRC, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_004, DateType.DATE_TYPE_003, DateType.DATE_TYPE_006, DateType.DATE_TYPE_020, DateType.DATE_TYPE_021 }, TimeList = new List<string> { TimeType.TIME_TYPE_001, TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005, TimeType.TIME_TYPE_006 } }},
            { Locale.CHINESE_T, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_003, DateType.DATE_TYPE_004, DateType.DATE_TYPE_006, DateType.DATE_TYPE_021 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.CZECH, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_026, DateType.DATE_TYPE_019, DateType.DATE_TYPE_015 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.DANISH, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_010, DateType.DATE_TYPE_022, DateType.DATE_TYPE_019, DateType.DATE_TYPE_011, DateType.DATE_TYPE_018, DateType.DATE_TYPE_027, DateType.DATE_TYPE_028, DateType.DATE_TYPE_029, DateType.DATE_TYPE_030, DateType.DATE_TYPE_016, DateType.DATE_TYPE_017, DateType.DATE_TYPE_023 }, TimeList = new List<string> { TimeType.TIME_TYPE_001, TimeType.TIME_TYPE_005 } }},
            { Locale.ENGLISH_US, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_010, DateType.DATE_TYPE_001, DateType.DATE_TYPE_003, DateType.DATE_TYPE_004, DateType.DATE_TYPE_006, DateType.DATE_TYPE_007, DateType.DATE_TYPE_009 }, TimeList = new List<string> { TimeType.TIME_TYPE_005, TimeType.TIME_TYPE_003 } }},
            { Locale.ENGLISH_UK, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_014, DateType.DATE_TYPE_019, DateType.DATE_TYPE_026, DateType.DATE_TYPE_018, DateType.DATE_TYPE_010 }, TimeList = new List<string> { TimeType.TIME_TYPE_001, TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005, TimeType.TIME_TYPE_006 } }},
            { Locale.ENGLISH_A, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_014, DateType.DATE_TYPE_019, DateType.DATE_TYPE_026, DateType.DATE_TYPE_015, DateType.DATE_TYPE_006, DateType.DATE_TYPE_010, DateType.DATE_TYPE_011 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005, TimeType.TIME_TYPE_006 } }},
            { Locale.ENGLISH_C, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_014, DateType.DATE_TYPE_019, DateType.DATE_TYPE_026, DateType.DATE_TYPE_011, DateType.DATE_TYPE_010, DateType.DATE_TYPE_003, DateType.DATE_TYPE_007, DateType.DATE_TYPE_009 }, TimeList = new List<string> { TimeType.TIME_TYPE_001, TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005, TimeType.TIME_TYPE_006 } }},
            { Locale.ENGLISH_S, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_011, DateType.DATE_TYPE_010 }, TimeList = new List<string> { TimeType.TIME_TYPE_001, TimeType.TIME_TYPE_005 } }},
            { Locale.FILIPINO, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_001, DateType.DATE_TYPE_003, DateType.DATE_TYPE_004, DateType.DATE_TYPE_011, DateType.DATE_TYPE_010, DateType.DATE_TYPE_006, DateType.DATE_TYPE_035 }, TimeList = new List<string> { TimeType.TIME_TYPE_001, TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005, TimeType.TIME_TYPE_006 } }},
            { Locale.FINNISH, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_018, DateType.DATE_TYPE_023, DateType.DATE_TYPE_010 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.FRENCH_FRANCE, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_026, DateType.DATE_TYPE_019, DateType.DATE_TYPE_001 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.GERMAN_GERMANY, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_018, DateType.DATE_TYPE_017, DateType.DATE_TYPE_023 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.ITALIAN_ITALY, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_026, DateType.DATE_TYPE_019, DateType.DATE_TYPE_006, DateType.DATE_TYPE_015, DateType.DATE_TYPE_009 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.JAPANESE, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_021, DateType.DATE_TYPE_003, DateType.DATE_TYPE_004, DateType.DATE_TYPE_006 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.MALAY, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_014, DateType.DATE_TYPE_019, DateType.DATE_TYPE_010 }, TimeList = new List<string> { TimeType.TIME_TYPE_005, TimeType.TIME_TYPE_006 } }},
            { Locale.POLISH, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_010, DateType.DATE_TYPE_011, DateType.DATE_TYPE_015, DateType.DATE_TYPE_033 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.PORTUGUESE_PORTUGAL, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_014, DateType.DATE_TYPE_019, DateType.DATE_TYPE_026, DateType.DATE_TYPE_006, DateType.DATE_TYPE_015, DateType.DATE_TYPE_009 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.ROMANIAN, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_026, DateType.DATE_TYPE_019, DateType.DATE_TYPE_015 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.RUSSIAN, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_026, DateType.DATE_TYPE_019, DateType.DATE_TYPE_015 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.SPANISH_SPAIN, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_026, DateType.DATE_TYPE_019, DateType.DATE_TYPE_006, DateType.DATE_TYPE_015, DateType.DATE_TYPE_009 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.SWEDISH, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_010, DateType.DATE_TYPE_011, DateType.DATE_TYPE_027, DateType.DATE_TYPE_030, DateType.DATE_TYPE_026, DateType.DATE_TYPE_012 }, TimeList = new List<string> { TimeType.TIME_TYPE_005 } }},
            { Locale.THAI, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_015, DateType.DATE_TYPE_026 }, TimeList = new List<string> { TimeType.TIME_TYPE_003, TimeType.TIME_TYPE_005 } }},
            { Locale.TURKISH, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_026, DateType.DATE_TYPE_019, DateType.DATE_TYPE_014, DateType.DATE_TYPE_001, DateType.DATE_TYPE_034, }, TimeList = new List<string> { TimeType.TIME_TYPE_005 } }},
            { Locale.UKRAINIAN, new LocaleValue { DateList = new List<string> { DateType.DATE_TYPE_016, DateType.DATE_TYPE_017, DateType.DATE_TYPE_010, }, TimeList = new List<string> { TimeType.TIME_TYPE_005, TimeType.TIME_TYPE_006 } }},
         };
        #endregion

        #region eRoom, Livelink, Documentum Migration

        private const string DATETYPEForLivelink001 = "yyyyMMddhhmmss"; // Livelink in Migration Tool
        public const string DATETYPEForLivelink002 = "MM-dd-yyyy";// Livelink in Migration Tool
        public const string DATETYPEForLivelink003 = "H:m:s";// Livelink in Migration Tool
        public const string DATETYPEForLivelink004 = "MM-dd-yyyy H:m:s";// Livelink in Migration Tool

        private const string DATETYPEForEMC001 = "yyyy-MM-dd HH:mm:ss"; // Documentum in Migration Tool
        private const string DATETYPEForEMC002 = "yyyyMMddHHmmss"; // Documentum in Migration Tool
        private const string DATETYPEForEMC003 = "mm/dd/yyyy hh:mi:ss"; // Documentum last job time format
        private const string DATETYPEForEMC004 = "yyyy-MM-dd HH:mm:ss"; // Documentum date time format

        private const string DATETYPEForeRoom001 = "yyyy'-'MM'-'dd'T'hh':'mm':'ss'Z'"; // eRoom Export
        private const string DATETYPEForeRoom002 = "dddd, MMMM dd, yyyy hh:mm tt"; // eRoom Export
        private const string DATETYPEForeRoom003 = "yyyy-MM-dd HH:mm:ss (\"UTC\"zzz)"; // eRoom Filter

        #endregion

        public static string DATETYPEForCmdlet001 // Cmdlet
        {
            get
            {
                return "yyyy-MM-dd HH:mm:ss,fff";
            }
        }

        public static string DATETYPEForAPI001 // API
        {
            get
            {
                return GConstants.TimeFormatTemplate.DATEPATTERN;
            }
        }

        public static string DATETYPEForAPI002 // API
        {
            get
            {
                return GConstants.TimeFormatTemplate.TIMEPATTERN;
            }
        }

        public static string DATETYPEForAPI003 // API
        {
            get
            {
                return "yyyy-MM-dd HH:mm";
            }
        }

        #endregion

        #region -- Convert Method --

        //CP-Configuration Start
        /// <summary>
        /// 使用GetDateTypeForConfiguration1来获取时间样式
        /// </summary>
        /// <returns></returns>
        public static string GetDateTypeForConfiguration1()
        {
            return DATATYPECONFIGURATION001;
        }

        public static string GetDateTypeForConfiguration2()
        {
            return DATATYPECONFIGURATION002;
        }

        public static string GetDateTypeForConfiguration3()
        {
            return DATATYPECONFIGURATION003;
        }

        public static string GetDateTypeForConfiguration4()
        {
            return DATATYPECONFIGURATION004;
        }

        public static string GetDateTypeForConfiguration5()
        {
            return DATATYPECONFIGURATION005;
        }

        public static string GetDateTypeForConfiguration6()
        {
            return DATATYPECONFIGURATION006;
        }

        public static string GetDateTypeForConfiguration7()
        {
            return DATATYPECONFIGURATION007;
        }

        public static string GetDateTypeForConfiguration8()
        {
            return DATATYPECONFIGURATION008;
        }

        public static string GetDateTypeForConfiguration9()
        {
            return DATATYPECONFIGURATION009;
        }

        public static string GetDateTypeForConfiguration10()
        {
            return DATATYPECONFIGURATION0010;
        }

        public static string GetDateTypeForConfiguration11()
        {
            return DATATYPECONFIGURATION0011;
        }

        //CP-Configuration End

        //Common Start
        /// <summary>
        /// 使用Common000方案来输出时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToTypeForCommon000(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForCommon000, DateTimeFormatInfo.InvariantInfo);
        }
        //Common End

        /// <summary>
        /// 使用001方案来输出时间--"yyyy-MM-dd"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToType001(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE001);
        }

        //CA Start
        /// <summary>
        /// 使用CA000方案来输出时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToTypeForCA001(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForCA001, DateTimeFormatInfo.InvariantInfo);
        }
        /// <summary>
        /// 使用CA002方案来输出时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToTypeForCA002(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForCA002, DateTimeFormatInfo.InvariantInfo);
        }
        /// <summary>
        /// 使用CA003方案来输出时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToTypeForCA003(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForCA003, DateTimeFormatInfo.InvariantInfo);
        }
        /// <summary>
        /// 使用CA004方案来输出时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToTypeForCA004(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForCA004);
        }
        /// <summary>
        /// 使用CA005方案来输出时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToTypeForCA005(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForCA005);
        }
        //CA End

        /// <summary>
        /// 使用002方案来输出时间--"yyyyMMddhhmmss"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToType002(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE002);
        }
        /// <summary>
        /// 使用003方案来输出时间--"dd-MM-yyyy_HH.mm.ss"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToType003(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE003);
        }
        /// <summary>
        /// 使用004方案输出时间--"MM/dd/yyyy HH:mm:ss"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToType004(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE004);
        }
        public static DateTime ConvertToType004(string dateTime)
        {
            return DateTime.ParseExact(dateTime, DATETYPE004, null);
        }

        /// <summary>
        /// 使用005方案输出时间--"yyyyMMddHHmm"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToType005(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE005);
        }
        /// <summary>
        /// 使用006方案输出时间--"yyyy-MM-dd HH:mm:ss.fff"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToType006(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE006);
        }
        /// <summary>
        /// 使用007方案输出时间--"yyyy-MM-dd HH:mm:ss"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToType007(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE007);
        }

        /// <summary>
        /// 使用008方案输出时间--"MM_dd_yyyy_hh_mm_ss"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToType008(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE008);
        }

        public static string ConvertToType009(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE009);
        }
        public static string ConvertToType010(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE010);
        }
        public static string ConvertToType011(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE011);
        }

        /// <summary>
        /// 使用013方案输出时间--"yyyy-MM-dd hh-mm-ss"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToType013(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE013);
        }
        public static string ConvertToType012(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE012);
        }
        public static string ConvertToType014(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE014);
        }

        public static string ConvertToType017(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE017);
        }

        public static string ConvertToType018(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE018);
        }

        public static string ConvertToFormatString(string value)
        {
            return string.Format("{0:yyyy-MM-dd}", value); ;
        }
        /// <summary>
        /// 使用PF001方案输出时间--"yyyy-MM-ddThh:mm:ssZ"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToTypeForPF001(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForPF001);
        }
        /// <summary>
        /// 使用PF002方案输出时间--"yyyy-MM-ddThh:mm:00Z"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToTypeForPF002(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForPF002);
        }
        /// <summary>
        /// 使用PF003方案输出时间--"yyyyMMdd"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToTypeForPF003(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForPF003);
        }
        /// <summary>
        /// 使用PF004方案输出时间--"yyyy-MM-dd HH:mm:ss"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToTypeForPF004(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForPF004);
        }

        public static string ConvertToTypeForNonSPMigration001(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE004, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 使用Livelink001方案输出时间--"yyyyMMddhhmmss"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToTypeForLivelink001(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForLivelink001);
        }

        public static string ConvertToTypeForeRoom001(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForeRoom001);
        }

        public static string ConvertToTypeForeRoom002(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPEForeRoom002, DateTimeFormatInfo.InvariantInfo);
        }

        public static string GetDateTypeForeRoom003()
        {
            return DATETYPEForeRoom003;
        }

        public static string GetDateTypeForEMC001()
        {
            return DATETYPEForEMC001;
        }

        public static string GetDateTypeForEMC002()
        {
            return DATETYPEForEMC002;
        }

        public static string GetDateTypeForEMC003()
        {
            return DATETYPEForEMC003;
        }

        public static string GetDateTypeForEMC004()
        {
            return DATETYPEForEMC004;
        }

        public static string ConvertToType020(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE020);
        }

        public static string ConvertToType021(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE021);
        }

        public static string ConvertToType022(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE022);
        }

        public static string ConvertToTypeForRP001(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPERorRP001);
        }

        /// <summary>
        /// yyyyMMddHHmmss
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToInvariantInfo(DateTime dateTime)
        {
            return dateTime.ToString(DATETYPE010, DateTimeFormatInfo.InvariantInfo);
        }
        #endregion
    }
}
