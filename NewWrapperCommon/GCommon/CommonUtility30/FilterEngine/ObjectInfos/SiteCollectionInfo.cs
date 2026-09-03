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
    using AvePoint.GCommon.Contract.CentralAdmin.Object;

    #endregion

    /// <summary>
    /// Site Collection级别信息对象类
    /// </summary>
    public class SiteCollectionInfo : ObjectInfoBase
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Owner { get; set; }
        public string OwnerLogonName { get; set; }
        public string OwnerLogonNameWithPrefix { get; set; }
        public string OwnerTitle { get; set; }
        /// <summary>
        /// Site的模板
        /// 注意这个模板的格式是STS#0
        /// </summary>
        public string Template { get; set; }
        /// <summary>
        /// Template的Display Name.
        /// eg:Team Site, Blog,etc.
        /// </summary>
        public string TemplateName { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public DateTime AccessTime { get; set; }
        public long Size { get; set; }
        public bool EnableAuditing { get; set; }
        public bool EnableAnonymousAccess { get; set; }
        public LockStatus LockStatus { get; set; }

        public Hashtable Properties { get; set; }


        internal override FilterLevel Level
        {
            get { return FilterLevel.SiteCollection; }
        }
    }
}
