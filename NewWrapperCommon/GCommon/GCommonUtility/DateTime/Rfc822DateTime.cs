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


namespace AvePoint.GCommon.Utility
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    #endregion using directives

    /// <summary>
    /// Represents a RFC 822 DateTime structure.
    /// </summary>
    /// <remarks>See <a href="http://asg.web.cmu.edu/rfc/rfc822.html">http://asg.web.cmu.edu/rfc/rfc822.html</a>
    /// for for details on the RFC-822 date time guidelines.</remarks>
    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rfc822DateTime is unmodifiable as the cause of being referenced.")]
    public class Rfc822DateTime : IComparable
    {
        DateTime baseDateTime;

        /// <summary>
        /// This names is defined by RFC822,  but are not so standard, so currently we will not use
        /// the one word definition and we will use the offset description instead
        /// </summary>
#pragma warning disable 169
        static Dictionary<Int32, String> cachedTimeZoneCommonNameMapping = new Dictionary<Int32, String>
#pragma warning restore 169
        {
            { 1,"N"}, { 2,"O"}, { 3,"P"}, { 4,"Q"}, { 5,"R"}, { 6,"S"}, { 7,"T"},  { 8,"U"}, { 9,"V"}, { 10,"W"}, { 11,"X"}, { 12,"Y"},
            {-1,"A"}, {-2,"B"}, {-3,"C"}, {-4,"D"}, {-5,"E"}, {-6,"F"}, {-7,"G"},  {-8,"H"}, {-9,"I"}, {-10,"K"}, {-11,"L"}, {-12,"M"},
            {0,"UT"}
        };

        static readonly Dictionary<String, Int32> cachedTimeZoneTimeAdjustmentMapping = new Dictionary<String, Int32>
        {
            {"N",-1}, {"O",-2}, {"P",-3}, {"Q",-4}, {"R",-5}, {"S",-6}, {"T",-7},  {"U",-8}, {"V",-9}, {"W",-10}, {"X",-11}, {"Y",-12},
            {"A", 1}, {"B", 2}, {"C", 3}, {"D", 4}, {"E", 5}, {"F", 6}, {"G", 7},  {"H", 8}, {"I", 9}, {"K", 10}, {"L", 11}, {"M", 12},
            {"UT",0},   {"EDT",4},  {"CDT",5},  {"MDT",6},  {"PDT",7},{"EST",5},  {"CST",6},  {"MST",7},  {"PST",8},
        };
        static readonly Dictionary<Int32, String> cachedAmericaTimeZoneDaylightSavingTimeCommonNameMapping = new Dictionary<Int32, String>
        {
            {-4,"EDT"}, {-5,"CDT"}, {-6,"MDT"}, {-7,"PDT"}
        };

        static readonly Dictionary<Int32, String> cachedAmericaTimeZoneStandardTimeCommonNameMapping = new Dictionary<Int32, String>
        {
            {-5,"EST"}, {-6,"CST"}, {-7,"MST"}, {-8,"PST"}
        };

        static readonly List<String> cachedAmericaStandardTimeZoneNames = new List<String>
        {
            "Central Standard Time",   "Pacific Standard Time", "Mountain Standard Time", "Eastern Standard Time"
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        public Rfc822DateTime(DateTime dateTime) { this.baseDateTime = dateTime; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime DateTime { get { return this.baseDateTime; } }

        #region Parse(string value)

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTimeString"></param>
        /// <returns></returns>
        public static Rfc822DateTime Parse(String dateTimeString)
        {
            return new Rfc822DateTime(ParseToUtcTime(dateTimeString));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTimeString"></param>
        /// <returns></returns>
        public static DateTime ParseToLocalTime(String dateTimeString)
        {
            DateTime dateTime;
            var position = dateTimeString.LastIndexOf(" ", StringComparison.OrdinalIgnoreCase);
            try
            {
                dateTime = Convert.ToDateTime(dateTimeString, DateTimeFormatInfo.InvariantInfo);
            }
            catch (FormatException)
            {
                dateTime = Convert.ToDateTime(dateTimeString.Substring(0, position));
                var timeZoneString = dateTimeString.Substring(position + 1);
                var adjustmentTime = cachedTimeZoneTimeAdjustmentMapping[timeZoneString.ToUpperInvariant().Trim()];
                var tempDateTime = dateTime.AddHours(adjustmentTime);
                var utcDateTime = DateTime.SpecifyKind(tempDateTime, DateTimeKind.Utc);
                dateTime = utcDateTime.ToLocalTime();
            }

            return dateTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTimeString"></param>
        /// <returns></returns>
        public static DateTime ParseToUtcTime(String dateTimeString)
        {
            return ParseToLocalTime(dateTimeString).ToUniversalTime();
        }

        #endregion Parse(string value)

        #region ToString()

        public override String ToString()
        {
            return this.ToString(TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "yyyy")]
        public virtual String ToSpecificString()
        {
            return this.ToString(TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id), "MMM dd HH:mm:ss '{0}' yyyy");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetTimeZoneInfo"></param>
        /// <param name="dateTimeFormatString"></param>
        /// <returns></returns>
        /// 
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "dd")]
        public virtual String ToString(TimeZoneInfo targetTimeZoneInfo, String dateTimeFormatString = "ddd, dd MMM yyyy HH:mm:ss '{0}'")
        {
            var sourceTimeZoneInfo = TimeZoneInfo.Local;
            if (this.DateTime.Kind == DateTimeKind.Utc)
                sourceTimeZoneInfo = TimeZoneInfo.Utc;
            return this.ToString(sourceTimeZoneInfo, targetTimeZoneInfo, dateTimeFormatString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceTimeZoneInfo"></param>
        /// <param name="targetTimeZoneInfo"></param>
        /// <param name="dateTimeFormatString"></param>
        /// <returns></returns>
        public virtual String ToString(TimeZoneInfo sourceTimeZoneInfo, TimeZoneInfo targetTimeZoneInfo, String dateTimeFormatString = "ddd, dd MMM yyyy HH:mm:ss '{0}'")
        {
            var specificTimeZoneDateTime = TimeZoneInfo.ConvertTime(this.DateTime, sourceTimeZoneInfo, targetTimeZoneInfo);
            var offset = targetTimeZoneInfo.GetUtcOffset(specificTimeZoneDateTime).Hours;
            string timeZoneString;
            if (targetTimeZoneInfo.SupportsDaylightSavingTime
                && cachedAmericaStandardTimeZoneNames.Contains(targetTimeZoneInfo.Id))
            {
                timeZoneString = targetTimeZoneInfo.IsDaylightSavingTime(specificTimeZoneDateTime) ? cachedAmericaTimeZoneDaylightSavingTimeCommonNameMapping[offset] : cachedAmericaTimeZoneStandardTimeCommonNameMapping[offset];
            }
            else if (offset == 0) timeZoneString = "UT";  //here we can use UT, Z, or GMT;
            else timeZoneString = String.Format("{0}{1}", offset > 0 ? "+" : "-", Math.Abs(offset).ToString(NumberFormatInfo.InvariantInfo).PadLeft(2, '0')).PadRight(5, '0');

            return String.Format(specificTimeZoneDateTime.ToString(dateTimeFormatString, DateTimeFormatInfo.InvariantInfo), timeZoneString);
        }

        #endregion ToString()

        #region CompareTo(object obj)

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Rfc")]
        public Int32 CompareTo(Object obj)
        {
            if (obj == null) return 1;
            var value = obj as Rfc822DateTime;
            if (value != null)
                return this.DateTime.CompareTo(value.DateTime);
            // ReSharper disable PossiblyMistakenUseOfParamsMethod
            throw new ArgumentException(String.Format(null, "object is not of type Rfc822DateTime, type was found to be '{0}'.", obj.GetType().FullName), "item");
            // ReSharper restore PossiblyMistakenUseOfParamsMethod
        }

        #endregion CompareTo(object obj)

        #region Equals(Object obj)

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Rfc822DateTime"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Rfc822DateTime"/>.</param>
        /// <returns><b>true</b> if the specified <see cref="Object"/> is equal to the current <see cref="Rfc822DateTime"/>; otherwise, <b>false</b>.</returns>
        public override Boolean Equals(Object obj)
        {
            if (!(obj is Rfc822DateTime)) return false;
            return (this.CompareTo(obj) == 0);
        }

        #endregion Equals(Object obj)

        #region GetHashCode()

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override Int32 GetHashCode()
        {
            var charArray = this.ToString().ToCharArray();
            return charArray.GetHashCode();
        }

        #endregion GetHashCode()

        #region == operator

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static Boolean operator ==(Rfc822DateTime first, Rfc822DateTime second)
        {
            if (first == null && second == null)
                return true;
            return !(first == null) && first == second;
        }

        #endregion == operator

        #region != operator

        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static Boolean operator !=(Rfc822DateTime first, Rfc822DateTime second)
        {
            return !(first == second);
        }

        #endregion != operator

        #region < operator

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static Boolean operator <(Rfc822DateTime first, Rfc822DateTime second)
        {
            if (Equals(first, null) && Equals(second, null))
                return false;
            if (Equals(first, null))
                return true;

            return (first.CompareTo(second) < 0);
        }

        #endregion < operator

        #region > operator

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static Boolean operator >(Rfc822DateTime first, Rfc822DateTime second)
        {
            if (Equals(first, null) && Equals(second, null))
                return false;
            if (Equals(first, null))
                return false;
            return (first.CompareTo(second) > 0);
        }

        #endregion > operator
    }
}