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
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// Template只负责转换template name，不会和policy混合在一起
    /// </summary>
    public interface ITemplateMapping
    {
        /// <summary>
        /// 获取Site Template的mapping name
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        string GetSiteTemplateMappingName(string templateName);

        /// <summary>
        /// 获取List Template的mapping name
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        string GetListTemplateMappingName(string templateName);

        /// <summary>
        /// 为了兼容老代码
        /// </summary>
        /// <returns></returns>
        [Obsolete("This method will be deprecated and removed later. key--001")]
        System.Xml.XmlElement ExportXml();
    }
}
