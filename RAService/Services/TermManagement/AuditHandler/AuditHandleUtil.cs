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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.TermManagement.AuditHandler
{
   public class AuditHandleUtil
    {
        #region 获取Retension值
        /// <summary>
        /// 获取Retension值
        /// </summary>
        /// <param name="retention">0:No Retension,1:sharepoint,2:exchange,3:sp&ex</param>
        /// <param name="level">33-SharePoint,11-Exchange</param>
        /// <returns></returns>
        public static string GetEnforceRetention(int retention, Level level = Level.Root)
        {
            var isChecked = false;
            if (retention != 0)
            {
                if (level == Level.Root)
                {
                    isChecked = true;
                }
                else 
                {
                    if (level == Level.SharePoint && ((retention & (int)EnforceRetentionType.SharePoint) == (int)EnforceRetentionType.SharePoint))
                    {
                        isChecked = true;
                    }
                    if (level == Level.Exchange && ((retention & (int)EnforceRetentionType.Exchange) == (int)EnforceRetentionType.Exchange))
                    {
                        isChecked = true;
                    }
                    if (level == Level.OneDrive && ((retention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive))
                    {
                        isChecked = true;
                    }
                    if (level == Level.Teams && ((retention & (int)EnforceRetentionType.Teams) == (int)EnforceRetentionType.Teams))
                    {
                        isChecked = true;
                    }
                }
            }
            return isChecked ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
        }
        #endregion
    }


    public enum Level
    {
        Root,
        SharePoint,
        Exchange,
        OneDrive,
        Teams
    }
}
