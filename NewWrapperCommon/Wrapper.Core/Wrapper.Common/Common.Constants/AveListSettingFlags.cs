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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// 1           ListSettingBackup       (0-No,      1-Yes)
    /// 2           IsListSettingChanged    (0-No,      1-Yes)
    /// 4           EnableVersions          (0-Disable, 1-Enable)
    /// 8           EnableMinorVersions     (0-Disable, 1-Enable)
    /// 16          EnableModeration        (0-Disable, 1-Enable)
    /// 32          ForceCheckout           (0-Disable, 1-Enable)
    /// </summary>
    public class AveListSettingFlags
    {
        public const int LIST_SETTING_NULL                  = 0;
        public const int LIST_SETTING_BACKUP                = 1;
        public const int LIST_SETTING_CHANGED               = 2;
        public const int LIST_ENABLE_VERSIONS               = 4;
        public const int LIST_ENABLE_MINOR_VERSIONS         = 8;
        public const int LIST_ENABLE_MODERATION             = 16;
        public const int LIST_FORCE_CHECK_OUT               = 32;
    }
}
