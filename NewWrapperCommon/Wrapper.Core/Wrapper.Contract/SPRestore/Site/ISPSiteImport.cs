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

using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.Internal;
using AvePoint.Wrapper.Core.SPAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// SiteCollection还原的接口
    /// </summary>
    public interface ISPSiteImport : IDisposable
    {
        /// <summary>
        /// User Mapping
        /// 
        /// by default is null
        /// </summary>
        IUserMapping UserMapping { get; set; }

        /// <summary>
        /// Language Mapping Controller
        /// </summary>
        ILanguageMappingController LanguageMappingController { get; }

        /// <summary>
        /// Template Mapping include site template and list template
        /// </summary>
        ITemplateMapping TemplateMapping { get; set; }

        /// <summary>
        /// Site
        /// </summary>
        IAveSite SPSite { get; }

        /// <summary>
        /// 是否触发event receiver还是不触发
        /// 
        /// 默认是触发
        /// </summary>
        bool EventReceiverFiringDisabled { get; set; }

        /// <summary>
        /// Restore the site according to the restore stream And restore options
        /// 
        /// 如果site collection没有还原出来，就是basic info没有还原出来，会抛异常给外围。
        /// 
        /// 如果是其他属性没有还原出来，则是返回report给外围，所以外围需要catch异常以及处理report。
        /// </summary>
        /// <param name="restoreStream"></param>
        /// <param name="spSiteRestoreOption"></param>
        /// <returns></returns>
        SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPSiteRestoreOption spSiteRestoreOption);

        /// <summary>
        /// Restore the site according to the restore stream and restore option
        /// </summary>
        /// <param name="restoreStream"></param>
        /// <param name="spSiteRestoreOption"></param>
        /// <param name="profiler"></param>
        void Restore(IAveRestoreStream restoreStream, SPSiteRestoreOption spSiteRestoreOption, ISPSiteImportProfiler profiler);
    }
}
