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

namespace AvePoint.RA.I18N.Core.DaoMigration
{
    public class DaoMigrationI18NEntity
    {
        public static string Execution(string key, params object[] args)
        {
            string i18N = SOI18NResource.Execution(key, args);
            if (string.IsNullOrEmpty(i18N))
            {
                i18N = EOI18NResource.Execution(key);
            }
            if (string.IsNullOrEmpty(i18N))
            {
                i18N = CPI18NResource.Execution(key);
            }
            if (string.IsNullOrEmpty(i18N))
            {
                i18N = I18NEntity.GetString(key, args);
            }
            return i18N.Replace("\\\"", "\"");
        }
    }
}
