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
    class DynamicContentTypeMapping:IContentTypeMapping
    {
        /// <summary>
        /// 用于兼容并构造原端Column Mapping，原有实现弃用后删除
        /// </summary>
        [Obsolete]
        internal byte[] Assembly { get; set; }

        /// <summary>
        /// 用于兼容并构造原端Column Mapping，原有实现弃用后删除
        /// </summary>
        [Obsolete]
        internal string FullTypeName { get; set; }

          /// <summary>
        /// Get the custom content type mapping class in the specified assembly.
        /// </summary>
        /// <param name="assembly">The byte[] of the assembly content</param>
        /// <exception cref="ArgumentExcpetion">Will throw ArgumentException if something wrong when we try to get the expected class</exception>
        public DynamicContentTypeMapping(byte[] assembly)
            : this(assembly, string.Empty)
        {
        }

        /// <summary>
        /// Get the specified custom content type mapping class in the specified assembly
        /// </summary>
        /// <param name="assembly">The byte[] of the assembly content</param>
        /// <param name="fullTypeName">The full type name which you want to get from the assembly</param>
        /// <exception cref="ArgumentException">Will throw ArgumentException if something wrong when we try to get the expected class</exception>
        public DynamicContentTypeMapping(byte[] assembly, string fullTypeName)
        {
            this.Assembly = assembly;
            this.FullTypeName = fullTypeName;

            //todo
        }

        public SPContentTypeInfo GetMappingContentTypeInfo(SPConditionableContentTypeInfo sourceContentTypeInfo)
        {
            throw new NotImplementedException();
        }
    }
}
