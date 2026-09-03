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



namespace AvePoint.Wrapper.Common
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Utility;
    internal static class IAveUserResourceExtension
    {
        /// <summary>
        /// 根据web.SupportedUICultures去将resource对象中的值转换成Info类,不获取当前Web Language对应的词条
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="web">当前resource所在Web，如果resource.AveResourceScope=Web请使用该重载</param>
        /// <returns></returns>
        public static AveUserResourceInfo GetUserResourceInfo(this IAveUserResource resource, IAveWeb web)
        {
            //IsMultilingual表示是否是多语言, resource==null表示不支持User Resource, resource.ResxBased表示是SharePoint自定义的Resource($Resource)
            if (resource == null || web == null || !web.IsMultilingual || resource.ResxBased 
                ||(!WrapperConfiguration.BackupOnlineUserResource && web.Site.IsOnlineSite))
            {
                return null;
            }
            var resourceInfo = new AveUserResourceInfo()
            {
                Name = resource.Name,
                Scope = resource.Scope,
                Type = resource.Type,
                Vaules = new Dictionary<int, string>(),
            };
            string currentWebLanguageVaule = resource.GetValueForUICulture(new CultureInfo((int)web.WorkingLanguage));
            foreach (var culture in web.SupportedUICultures)
            {
                //  local Web本身语言的不备份  ; 365 是注册user 的语言    
                if (culture.LCID == web.WorkingLanguage)
                {
                    if (web.WorkingLanguage != web.Language)// 只有365的WorkingLanguage 和 Language 会不一样
                    {
                        string value = resource.GetValueForUICulture(culture);
                        resourceInfo.Vaules[culture.LCID] = value;
                    }
                    continue;
                }
                string alternateLanguageValue = resource.GetValueForUICulture(culture);
                //如果没有对alternate language对应的Resource设置过值,取出来的就是默认语言的值,因此不需要备份
                if (string.Equals(currentWebLanguageVaule, alternateLanguageValue, System.StringComparison.Ordinal))
                {
                    continue;
                }
                resourceInfo.Vaules[culture.LCID] = alternateLanguageValue;
            }
            return resourceInfo.Vaules.Count > 0 ? resourceInfo : null;
        }
        /// <summary>
        /// 根据web.SupportedUICultures去将resource对象中的值转换成Xml,不获取当前Web Language对应的词条
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="web">当前resource所在Web，如果resource.AveResourceScope=Web请使用该重载</param>
        /// <returns></returns>
        public static string GetUserResourceInfoXml(this IAveUserResource resource, IAveWeb web)
        {
            var info = GetUserResourceInfo(resource, web);
            return info == null ? null : SerializerHelper.SerializeByDataContractSerializer(info);
        }
        /// <summary>
        /// 根据list.ParentWeb.SupportedUICultures去将resource对象中的值转换成Info类,不获取当前Web Language对应的词条
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="list">当前resource所在list，如果resource.AveResourceScope=List请使用该重载</param>
        /// <returns></returns>
        public static AveUserResourceInfo GetUserResourceInfo(this IAveUserResource resource, IAveList list)
        {
            if (list == null)
            {
                return null;
            }
            return GetUserResourceInfo(resource, list.ParentWeb);
        }
        /// <summary>
        /// 根据web.SupportedUICultures对Resource对象赋值,不对当前Web的Language赋值
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="web">当前resource所在Web，如果resource.AveResourceScope=Web请使用该重载</param>
        /// <param name="info"></param>
        public static bool SetUserResource(this IAveUserResource resource, IAveWeb web, AveUserResourceInfo info)
        {
            bool change = false;
            if (resource == null || info == null || web == null || !web.IsMultilingual)
            {
                return change;
            }
            string value = null;
            foreach (var culture in web.SupportedUICultures)
            {
                if (culture.LCID == web.WorkingLanguage)
                {
                    if (web.WorkingLanguage != web.Language)// 只有365的WorkingLanguage 和 Language 会不一样
                    {
                        if (info.Vaules.TryGetValue(culture.LCID, out value) && !string.IsNullOrEmpty(value))
                        {
                            resource.SetVauleForUICulture(culture, value);
                            change = true;
                        }
                    }
                    continue;
                }
                if (info.Vaules.TryGetValue(culture.LCID, out value) && !string.IsNullOrEmpty(value) &&
                    !string.Equals(value, resource.GetValueForUICulture(culture)))
                {
                    resource.SetVauleForUICulture(culture, value);
                    change = true;
                }
            }
            return change;
        }
        /// <summary>
        /// 根据list.ParentWeb.SupportedUICultures对Resource对象赋值,不对当前Web的Language赋值
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="list">当前resource所在list，如果resource.AveResourceScope=List请使用该重载</param>
        /// <param name="info"></param>
        public static bool SetUserResource(this IAveUserResource resource, IAveList list, AveUserResourceInfo info)
        {
            if (list != null)
            {
                return SetUserResource(resource, list.ParentWeb, info);
            }
            return false;
        }

        /// <summary>
        /// 比较两个Resource是否兼容。
        /// 标准：
        /// 1.源端或目的端不支持多语言，认为兼容。
        /// 2.源端的Resource在目的端能找到对应的语言，并且对应语言的Resource是不一样的认为不兼容。
        /// 3.其他情况认为兼容
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="web"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool CompareUserResource(this IAveUserResource resource, IAveWeb web, AveUserResourceInfo info)
        {
            //resource为null表示目的端不支持User Resource, info为null表示源端不支持或源端没有使用自定义Resource
            if (resource == null || info == null || web == null || !web.IsMultilingual)
            {
                return true;
            }
            string value = null;
            foreach (var culture in web.SupportedUICultures)
            {
                if (culture.LCID == web.WorkingLanguage)
                {
                    continue;
                }
                if (info.Vaules.TryGetValue(culture.LCID, out value) && !string.IsNullOrEmpty(value))
                {
                    if (!string.Equals(resource.GetValueForUICulture(culture), value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
