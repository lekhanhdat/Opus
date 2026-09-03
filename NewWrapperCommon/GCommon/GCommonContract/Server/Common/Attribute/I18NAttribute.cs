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

namespace AvePoint.GCommon.Contract.Server.Common.Attribute
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class I18NAttribute : System.Attribute
    {
        //I18N key
        public string Key { get; private set; }
        //default value
        public string DefaultValue { get; private set; }

        //public I18NAttribute(string key)
        //{
        //    Key = key;
        //}
        /// <summary>
        /// 在构造方法中声明key和default value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        public I18NAttribute(string key, string defaultValue)
        {
            Key = key;
            DefaultValue = defaultValue;
        }
    }
}
