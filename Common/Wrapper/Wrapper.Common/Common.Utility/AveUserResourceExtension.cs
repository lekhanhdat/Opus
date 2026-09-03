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
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common
{
    public static class AveUserResourceExtension
    {
        static AveLogger mLogger = AveLogger.GetInstance(typeof(AveUserResourceExtension));

        /// <summary>
        /// 支持的语言名称
        /// </summary>
        public static List<string> SupportedResourceCultureNames
        {
            get
            {
                return WrapperConfiguration.WrapperConfigurationForBPOS.EnableMultiLanguage ? WrapperConfiguration.WrapperConfigurationForBPOS.MultiLanguageList : null;
            }
        }

        /// <summary>
        /// 根据web.SupportedUICultures对Resource对象赋值,不对当前Web的Language赋值
        /// </summary>
        public static bool SetUserResource(this IAveUserResource resource, IAveWeb web, Dictionary<string, string> info, bool compare,bool forceSet = false)
        {
            bool change = false;
            if (resource != null && info != null && web != null && web.IsMultilingual)
            {
                //var webCulture = new CultureInfo((int)web.GetWorkingLanguage());
                foreach (var keyValue in info)
                {
                    if (/*!string.Equals(keyValue.Key, webCulture.Name, StringComparison.Ordinal) &&  */!string.IsNullOrEmpty(keyValue.Value))
                    {
                        if (compare)
                        {
                            var currentValue = resource.GetValueForUICulture(keyValue.Key);
                            if (string.Compare(keyValue.Value, currentValue, false) != 0)
                            {
                                resource.SetValueForUICulture(keyValue.Key, keyValue.Value,forceSet);
                                change = true;
                            }
                        }
                        else
                        {
                            resource.SetValueForUICulture(keyValue.Key, keyValue.Value,forceSet);
                            change = true;
                        }
                    }
                }
            }
            return change;
        }

        /// <summary>
        /// 比较两个Resource是否兼容。
        /// 标准：
        /// 1.源端或目的端不支持多语言，认为兼容。
        /// 2.源端的Resource在目的端能找到对应的语言，并且对应语言的Resource是不一样的认为不兼容。
        /// 3.其他情况认为兼容
        /// </summary>
        public static bool CompareUserResource(this IAveUserResource resource, IAveWeb web, Dictionary<string, string> info)
        {
            //resource为null表示目的端不支持User Resource, info为null表示源端不支持或源端没有使用自定义Resource
            if (resource != null && info != null && web != null && web.IsMultilingual)
            {
                var webCulture = new CultureInfo((int)web.Language);
                foreach (var keyValue in info)
                {
                    if (!string.Equals(keyValue.Key, webCulture.Name, StringComparison.Ordinal) && !string.IsNullOrEmpty(keyValue.Value))
                    {
                        var currentValue = resource.GetValueForUICulture(keyValue.Key);
                        if (!string.Equals(currentValue, keyValue.Value, StringComparison.Ordinal))
                        {
                            mLogger.Info("Compare user resource conflict, language name:{0}, source value:{1}, dest value:{2}", keyValue.Key, keyValue.Value, currentValue);
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
