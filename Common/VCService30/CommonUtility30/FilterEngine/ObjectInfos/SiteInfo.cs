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




namespace AvePoint.Common.FilterEngine
{
    #region using directives
    using System;
    using System.Collections;

    #endregion

    /// <summary>
    /// Site级别信息对象类
    /// </summary>
    public class SiteInfo : CommonInfoBase
    {

        public string Url { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        /// <summary>
        /// Web的模板
        /// </summary>
        public string Template { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        /// <summary>
        /// Template的Display Name.
        /// eg:Team Site, Blog,etc.
        /// </summary>
        public string TemplateName { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public bool InheritPermission { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public bool EnableAuditing { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public bool EnableAnonymousAccess { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }

        public Hashtable ColumnInfos { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }

    }
}
