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




namespace AvePoint.Common
{
    #region using directives
    using System;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// This is a common utility class to validate some kinds of object
    /// </summary>
    public class Validator
    {
        #region 判断对象是否为空

        /// <summary>
        /// 判断对象是否为空，为空返回true
        /// </summary>
        /// <param name="data">要验证的对象</param>
        public static bool IsNullOrEmpty(object data)
        {
            //如果为null
            if (data == null)
            {
                return true;
            }

            //如果为""
            if (data.GetType() == typeof(String))
            {
                if (string.IsNullOrEmpty(data.ToString().Trim()))
                {
                    return true;
                }
            }

            //如果为DBNull
            if (data.GetType() == typeof(DBNull))
            {
                return true;
            }

            //不为空
            return false;
        }
        #endregion

        #region 验证IP地址是否合法
        /// <summary>
        /// 验证IP地址是否合法
        /// </summary>
        /// <param name="ip">要验证的IP地址</param>        
        public static bool IsIP(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                return false;
            }

            //模式字符串
            string pattern = @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$";

            //验证
            return Regex.IsMatch(ip.Trim(), pattern);
        }
        #endregion

        #region  验证EMail是否合法
        /// <summary>
        /// 验证EMail是否合法
        /// </summary>
        /// <param name="email">要验证的Email</param>
        public static bool IsEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            //模式字符串
            string pattern = @"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$";

            //验证
            return Regex.IsMatch(email.Trim(), pattern);
        }
        #endregion

        #region 验证是否为整数

        /// <summary>
        /// 验证是否为整数
        /// </summary>
        /// <param name="number">要验证的整数</param>  
        /// <returns></returns>
        public static bool IsInt(object number)
        {
            if (IsNullOrEmpty(number))
                return false;

            return IsInt(number.ToString());
        }

        /// <summary>
        /// 验证是否为整数
        /// </summary>
        /// <param name="number">要验证的整数</param>  
        /// <returns></returns>
        public static bool IsInt(string number)
        {
            if (string.IsNullOrEmpty(number))
            {
                return false;
            }

            int i = 0;

            return int.TryParse(number, out i);
        }
        #endregion

        #region 验证是否为数字
        /// <summary>
        /// 验证是否为数字  可带小数
        /// </summary>
        /// <param name="number">要验证的数字</param>  
        /// <returns></returns>
        public static bool IsNumber(object number)
        {
            if (IsNullOrEmpty(number))
                return false;

            return IsNumber(number.ToString());
        }

        /// <summary>
        /// 验证是否为数字  可带小数
        /// </summary>
        /// <param name="number">要验证的数字</param>  
        /// <returns></returns>
        public static bool IsNumber(string number)
        {
            if (string.IsNullOrEmpty(number))
            {
                return false;
            }

            //模式字符串
            string pattern = @"^[-]?[1-9]+[0-9]*[.]?[0-9]*$";

            //验证
            return Regex.IsMatch(number.Trim(), pattern);
        }
        #endregion

        #region 验证日期是否合法
        /// <summary>
        /// 验证日期是否合法
        /// </summary>
        /// <param name="date">日期</param>
        public static bool IsDate(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                return false;
            }

            DateTime dt = DateTime.Now;

            if (DateTime.TryParse(date, out dt))
                return true;

            return false;
        }
        #endregion

        #region 验证身份证是否合法
        /// <summary>
        /// 验证身份证是否合法  15 和  18位两种
        /// </summary>
        /// <param name="idCard">要验证的身份证</param>        
        public static bool IsIdCard(string idCard)
        {
            if (string.IsNullOrEmpty(idCard))
            {
                return false;
            }

            if (idCard.Length == 15)
            {
                return Regex.IsMatch(idCard, @"^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$");
            }
            else if (idCard.Length == 18)
            {
                return Regex.IsMatch(idCard, @"^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[A-Z])$", RegexOptions.IgnoreCase);
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 检测 url

        /// <summary>
        /// 检测是否是正确的Url
        /// </summary>
        /// <param name="strUrl">要验证的Url</param>
        /// <returns>判断结果</returns>
        public static bool IsURL(string strUrl)
        {
            return Regex.IsMatch(strUrl, @"^(http|https)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$", RegexOptions.IgnoreCase);
        }
        #endregion

        #region 判断是否为base64字符串
        /// <summary>
        /// 判断是否为base64字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsBase64String(string str)
        {
            return Regex.IsMatch(str, @"[A-Za-z0-9\+\/\=]");
        }
        #endregion

        #region 验证GUID
        /// <summary>
        /// 验证字符串是否是GUID  
        /// </summary>
        /// <param name="guid">字符串</param>
        /// <returns></returns>
        public static bool IsGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return false;

            return Regex.IsMatch(guid, "[A-F0-9]{8}(-[A-F0-9]{4}){3}-[A-F0-9]{12}|[A-F0-9]{32}", RegexOptions.IgnoreCase);
        }
        #endregion
    }
}
