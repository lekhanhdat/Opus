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
    using System.Globalization;

    /// <summary>
    /// 封装SPUserResource,只有Server10,13 Mode支持
    /// </summary>
    public interface IAveUserResource
    {
        string Name { get; }
        object Parent { get;}
        AveResourceScope Scope { get; }
        AveResourceType Type { get; }
        /// <summary>
        /// 标识是否是SharePoint自定义的Resource，如果为True，则是以$Resources开头，并通过SharePoint资源文件国际化成对应语言。
        /// </summary>
        bool ResxBased { get; }
        /// <summary>
        /// user resource的key值，可以根据此key值，使用SPUtility.GetLocalizedString方法获取对应语言的name或title等value
        /// 10，13有，client没有该属性
        /// </summary>
        string ResxResourceId { get; set; }
        string GetValueForUICulture(CultureInfo cultureInfo);
        void SetVauleForUICulture(CultureInfo cultureInfo, string value);
        /// <summary>
        /// Client API did not support update UserResource, Please update resource parent directly.
        /// </summary>
        void Update();

        /// <summary>
        /// 最好不要使用，和Thread.CurrentThread.CurrentUICulture)相关
        /// </summary>
        //string Vaule { get; set; }
    }
}
