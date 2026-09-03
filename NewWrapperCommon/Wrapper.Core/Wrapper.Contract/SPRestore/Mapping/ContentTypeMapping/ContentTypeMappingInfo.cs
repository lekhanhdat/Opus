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

namespace AvePoint.Wrapper.Core.SPRestore.Mapping
{
    class ContentTypeMappingInfo
    {
        /// <summary>
        /// 原端CT Name
        /// </summary>
        internal string SourceName { get; set; }

        /// <summary>
        /// 目的端CT Name
        /// </summary>
        internal string DestinationName { get; set; }

        /// <summary>
        /// Content Type mapping condition 条件
        /// </summary>
        internal MappingCondition MappingCondition { get; set; }

        internal static ContentTypeMappingInfo Create(System.Xml.XmlElement node)
        {
            ContentTypeMappingInfo contentTypeMappingInfo = new ContentTypeMappingInfo();
            contentTypeMappingInfo.Load(node);
            return contentTypeMappingInfo;
        }

        private void Load(System.Xml.XmlElement node)
        {
            this.SourceName = node.GetAttribute("sourceName");
            this.DestinationName = node.GetAttribute("destinationName");
        }

        internal SPContentTypeInfo ConvertToSPContentTypeInfo()
        {
            return new SPContentTypeInfo() { Name = DestinationName };
        }
    }
}
