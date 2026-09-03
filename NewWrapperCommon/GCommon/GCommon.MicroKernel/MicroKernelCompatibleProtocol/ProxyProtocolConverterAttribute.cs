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



namespace AvePoint.GCommon.MicroKernel
{
    #region using directives

    using System;

    #endregion

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class ProxyProtocolConverterAttribute
        : Attribute
          , IProxyProtocolConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public Boolean IsUseExtensionName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String TypeName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isUseExtensionName"></param>
        public ProxyProtocolConverterAttribute(Boolean isUseExtensionName)
        {
            this.IsUseExtensionName = isUseExtensionName;
        }

        public String ConvertToCurrentType(String typeName)
        {
            return this.IsUseExtensionName ? typeName + "Ex" : this.TypeName;
        }
    }
}