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




namespace System
{
    #region using directives
    using AvePoint.GCommon.Contract.CodeReview;

    #endregion

    #region CodeReview

    [AveCodeReview(
    "2012/1/16",
    "yhzhang@avepoint.com",
    "yhzhang@avepoint.com",
    new string[] { },
    null,
    true)]
    #endregion

    ///<Summary>
    /// extension of the System.Int64 class
    ///</Summary>
    public static class Int64Extension
    {
        /// <summary>
        /// when the date time is before 1970 1 1 0 0 , the ticks in java is a minus number
        /// </summary>
        /// <param name="dotNetTimeInLong">dot net date time in long ticks</param>
        /// <returns></returns>
        public static Int64 DotNetToJavaTime(this Int64 dotNetTimeInLong)
        {
            var javaTicksTime = dotNetTimeInLong - new DateTime(1970, 1, 1, 0, 0, 0).Ticks;
            return javaTicksTime / 10000;
        }

        /// <summary>
        /// The java long time to dot net time ticks
        /// </summary>
        /// <param name="javaTimeInLong">java time in long</param>
        /// <returns>dot time in ticks</returns>
        public static Int64 JavaToDotNetTimeInLong(this Int64 javaTimeInLong)
        {
            return (javaTimeInLong * 10000L) + new DateTime(1970, 1, 1, 0, 0, 0).Ticks;
        }

        /// <summary>
        /// when the date time is before 1970 1 1 0 0 , the ticks in java is a minus number
        /// </summary>
        /// <param name="dotNetTimeInLong">dot net date time in long ticks</param>
        /// <returns></returns>
        public static Int64 DotNetToWindowsFileTime(this Int64 dotNetTimeInLong)
        {
            return dotNetTimeInLong - new DateTime(1601, 1, 1, 0, 0, 0).Ticks;
        }

        /// <summary>
        /// Convert time in long type of java to DateTime
        /// </summary>
        /// <param name="javaTimeInLong">date time in java long</param>
        /// <returns>the dot net time</returns>
        public static DateTime JavaToDotNetTime(this Int64 javaTimeInLong)
        {
            return new DateTime(javaTimeInLong.JavaToDotNetTimeInLong());
        }

        /// <summary>
        /// Unix time is offset second of 1970, 1, 1, 0, 0, 0
        /// </summary>
        /// <param name="unixTimeInLong">time in long</param>
        /// <returns>the dot net date time</returns>
        public static DateTime UnixToDotNetTime(this Int64 unixTimeInLong)
        {
            return unixTimeInLong.UnixToJavaTime().JavaToDotNetTime();
        }

        /// <summary>
        /// Unix time is offset second of 1970, 1, 1, 0, 0, 0
        /// </summary>
        /// <param name="unixTimeInLong">dre time in long type</param>
        /// <returns>java time in long type</returns>
        public static Int64 UnixToJavaTime(this Int64 unixTimeInLong)
        {
            return unixTimeInLong * 1000L;
        }

        /// <summary>
        /// Unix time is offset second of 1970, 1, 1, 0, 0, 0
        /// </summary>
        /// <param name="unixTimeInLong">time in long</param>
        /// <returns>the dot net date time</returns>
        public static Int64 UnixToDotNetTimeInLong(this Int64 unixTimeInLong)
        {
            return unixTimeInLong.UnixToJavaTime().JavaToDotNetTimeInLong();
        }

        /// <summary>
        /// Unix time is offset second of 1970, 1, 1, 0, 0, 0
        /// </summary>
        /// <param name="unixTimeInLong">time in long</param>
        /// <returns>the dot net date time</returns>
        public static DateTime WindowsFileTimeToDotNetTime(this Int64 windowsFileTimeInLong)
        {
            return DateTime.FromFileTimeUtc(windowsFileTimeInLong);
        }

        /// <summary>
        /// Unix time is offset second of 1970, 1, 1, 0, 0, 0
        /// </summary>
        /// <param name="unixTimeInLong">time in long</param>
        /// <returns>the dot net date time</returns>
        public static Int64 WindowsFileTimeToDotNetTimeInLong(this Int64 windowsFileTimeInLong)
        {
            return DateTime.FromFileTimeUtc(windowsFileTimeInLong).ToUniversalTime().Ticks;
        }
    }
}